import * as durandalApp from 'durandal/app'
import * as system from 'durandal/system'
import * as router from 'plugins/router'
import { AuthManager, DateTimeProperty, DateProperty, TimeProperty } from 'xomega'

// Configure and run Durandal application

system.debug(true); // enable output of debug messages to console

let app = durandalApp;
app.title = document.title;

// configure Durandal plugins
app.configurePlugins({
    router: true,
    widget: {
        kinds: ['criteria', 'msglist', 'pager', 'panel']
    }
});

// configure Xomega date/time formats
DateTimeProperty.DefaultEditFormat = 'M/D/YYYY HH:mm';
DateProperty.DefaultEditFormat = 'M/D/YYYY';
TimeProperty.DefaultEditFormat = 'HH:mm';

// configure Xomega API access and authentication behavior
AuthManager.ApiRoot = 'http://localhost:58395/'; // set root URL for web api
AuthManager.Current.handleUnauthorizedResponse(); // sign out on 401 (Unauthorized)
AuthManager.Current.LoggedIn.subscribe(function (val) { // redirect to login on sign out
    if (val) return;
    router.navigate('#' + AuthManager.Current.getLoginUrl(router.activeInstruction()));
});

// start the app, and load the main view
app.start().then(() => app.setRoot('shell'));
