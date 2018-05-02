using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using Pixie;
using Pixie.Markup;
using Pixie.Options;
using Pixie.Terminal;
using Pixie.Transforms;

namespace SeleniumTests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            bool errorEncountered = false;

            // Acquire a log for when things go sideways.
            var log = new TransformLog(
                TerminalLog.Acquire(),
                entry => DiagnosticExtractor.Transform(entry, "selenium-tests"),
                entry =>
                {
                    if (entry.Severity == Severity.Error)
                    {
                        errorEncountered = true;
                    }
                    return entry;
                });

            // Parse command-line arguments.
            var optParser = new GnuOptionSetParser(Options.All, Options.Url);

            // Actually parse the options.
            var parsedOptions = optParser.Parse(args, log);

            if (errorEncountered)
            {
                // Ouch. Command-line arguments were bad. Stop testing now.
                return 1;
            }
            else if (parsedOptions.GetValue<bool>(Options.Help))
            {
                // Print a cute little help message (to stdout instead of stderr).
                var helpLog = TerminalLog.AcquireStandardOutput();
                helpLog.Log(
                    new Pixie.LogEntry(
                        Severity.Message,
                        new HelpMessage(
                            "Runs UnSHACLed's functional tests.",
                            "selenium-tests [url-or-options...]",
                            Options.All)));
                return 0;
            }

            string testUri = parsedOptions.GetValue<string>(Options.Url);
            bool noUri = string.IsNullOrWhiteSpace(testUri);

            if ((noUri && !parsedOptions.ContainsOption(Options.BuildApplication))
                || parsedOptions.GetValue<bool>(Options.BuildApplication))
            {
                // Build the application if there's no URL and `--no-build-app`
                // was not specified or if `--build-app` was specified.
                log.Log(
                    new Pixie.LogEntry(
                        Severity.Info,
                        "status",
                        "building UnSHACLed..."));
                BuildUnSHACLed();
                log.Log(
                    new Pixie.LogEntry(
                        Severity.Info,
                        "status",
                        "UnSHACLed built successfully!"));
            }

            if (noUri)
            {
                // If nobody bothered to specify a URL, then we'll just point it to
                // the application.
                testUri = "file://" + Path.GetFullPath(Path.Combine("..", "build", "index.html"));
            }

            var browserNames = parsedOptions.ContainsOption(Options.Browsers)
                ? parsedOptions.GetValue<IReadOnlyList<string>>(Options.Browsers)
                : new string[] { "firefox" };

            var browsersToUse = ParseBrowserNames(browserNames, log);

            if (errorEncountered)
            {
                // Couldn't parse the command-line args. Better quit now.
                return 1;
            }

            Run(testUri, browsersToUse, log);

            // If things went swimmingly, then return a zero exit code.
            // Otherwise, let the world know that something is wrong.
            return errorEncountered ? 1 : 0;
        }

        private static IReadOnlyDictionary<string, Func<IWebDriver>> Drivers =
            new Dictionary<string, Func<IWebDriver>>()
        {
            {
                "firefox",
                () => new FirefoxDriver(
                    new FirefoxOptions() { LogLevel = FirefoxDriverLogLevel.Fatal })
            },
            {
                "chrome",
                () => new ChromeDriver()
            }
        };

        /// <summary>
        /// Parses a list of browser names to use.
        /// </summary>
        /// <param name="names">The names to parse.</param>
        /// <param name="log">A log for sending errors to.</param>
        /// <returns>A list of web driver builders.</returns>
        private static IEnumerable<Func<IWebDriver>> ParseBrowserNames(
            IEnumerable<string> names, ILog log)
        {
            var browsersToUse = new List<Func<IWebDriver>>();
            foreach (var name in names)
            {
                if (Drivers.ContainsKey(name))
                {
                    browsersToUse.Add(Drivers[name]);
                }
                else
                {
                    // Try to guess what the user meant.
                    var suggestion = NameSuggestion.SuggestName(name, Drivers.Keys);
                    if (suggestion == null)
                    {
                        // Log an error if we couldn't find a reasonable guess.
                        log.Log(
                            new Pixie.LogEntry(
                                Severity.Error,
                                "unknown browser",
                                Quotation.QuoteEvenInBold(
                                    "specified browser ",
                                    name,
                                    " is not a known browser.")));
                    }
                    else
                    {
                        // Give the user a hand otherwise.
                        var diff = Diff.Create(name, suggestion);
                        log.Log(
                            new Pixie.LogEntry(
                                Severity.Error,
                                "unknown browser",
                                Quotation.QuoteEvenInBold(
                                    "specified browser ",
                                    TextDiff.RenderDeletions(diff),
                                    " is not a known browser; did you mean ",
                                    TextDiff.RenderInsertions(diff),
                                    "?")));
                    }
                }
            }
            return browsersToUse;
        }

        /// <summary>
        /// Runs all the tests based on parsed command-line options
        /// and a log.
        /// </summary>
        /// <param name="uri">The URI to test.</param>
        /// <param name="driverBuilders">
        /// A sequence of functions that each produce a driver to run the tests with.
        /// </param>
        /// <param name="log">A log to send messages to.</param>
        private static void Run(string uri, IEnumerable<Func<IWebDriver>> driverBuilders, ILog log)
        {
            // Create a new instance of the Firefox driver.
            // Note that it is wrapped in a using clause so that the browser is closed 
            // and the webdriver is disposed (even in the face of exceptions).

            // Also note that the remainder of the code relies on the interface, 
            // not the implementation.

            // Further note that other drivers (InternetExplorerDriver,
            // ChromeDriver, etc.) will require further configuration 
            // before this example will work. See the wiki pages for the
            // individual drivers at http://code.google.com/p/selenium/wiki
            // for further information.
            foreach (var builder in driverBuilders)
            {
                using (IWebDriver driver = builder())
                {
                    //Notice navigation is slightly different than the Java version
                    //This is because 'get' is a keyword in C#
                    driver.Navigate().GoToUrl(uri);

                    // // Find the text input element by its name
                    // IWebElement query = driver.FindElement(By.Name("q"));

                    // // Enter something to search for
                    // query.SendKeys("Cheese");

                    // // Now submit the form. WebDriver will find the form for us from the element
                    // query.Submit();

                    // // Google's search is rendered dynamically with JavaScript.
                    // // Wait for the page to load, timeout after 10 seconds
                    // var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    // wait.Until(d => d.Title.StartsWith("cheese", StringComparison.OrdinalIgnoreCase));

                    log.Log(
                        new Pixie.LogEntry(
                            Severity.Message,
                            "page title",
                            driver.Title));
                }
            }
        }

        private static void BuildUnSHACLed()
        {
            var process = new Process();
            process.StartInfo.WorkingDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
            process.StartInfo.FileName = "gulp";
            process.StartInfo.Arguments = "build";
            process.StartInfo.UseShellExecute = false;
            // process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            process.WaitForExit();
        }
    }
}
