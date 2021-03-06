import * as React from 'react';
import {Header} from 'semantic-ui-react';
import {Link} from 'react-router-dom';

/**
 * Component containing the sixth section of the user manual.
 */
class Section6 extends React.Component<any, any> {
    /** Render component **/
    render() {
        return (
            <div style={{marginTop: '2em'}}>
                <Header as="h1" id="6"> Troubleshooting </Header>
                {/** Known bugs **/}
                <Header as='h2' color="teal" id="6.1"> Known bugs </Header>
                <p> If you encountered a problem which could be a bug, please read our list of known bugs linked below
                </p>
                {/** Contact **/}
                <Header as='h2' color="teal" id="6.2"> Contact </Header>
                <p> If the provided support did not suite your needs, you can always contact the
                    developers of UnSHACLed by navigating to our <Link to="/contact"> contact page </Link>
                </p>
            </div>
        );
    }
}

export default Section6;
