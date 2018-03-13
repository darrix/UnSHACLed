#!/usr/bin/env python3

# This script automatically generates a main index.

import os

def index_directories():
    """Iterates through all top-level directories and tries to identify
       each as an UnSHACLed build by looking for 'index.html' and
       'build-name' files. Returns a list of (name, directory) tuples."""
    unshacled_dirs = []
    for top_level_dir in os.listdir():
        try:
            build_name = open(top_level_dir + '/build-name', mode='r')
        except OSError:
            continue

        if os.path.exists(top_level_dir + '/index.html'):
            unshacled_dirs.append((build_name.readline().strip(), top_level_dir))

    unshacled_dirs.sort(key=lambda tuple: tuple[0])
    return unshacled_dirs

def generate_index(name_directory_tuples):
    """Generates a top-level index from a list of (name, directory) tuples.
       Returns this index as a string."""
    list_elements = []
    for name, directory in name_directory_tuples:
        list_elements.append(
            (' ' * 12) +
            """<li><a href="%s">%s</a></li>""" % (directory + '/index.html', name))

    return \
"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>UnSHACLed builds</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css">
</head>
<body>
    <div class="container">
        <h1>UnSHACLed builds</h1>

        What follows is a list of all UnSHACLed builds hosted here. Click a hyperlink to try out a particular build.

        <ul>
%s
        </ul>
    </div>
</body>
</html>""" % '\n'.join(list_elements)

if __name__ == '__main__':
    print(generate_index(index_directories()))
