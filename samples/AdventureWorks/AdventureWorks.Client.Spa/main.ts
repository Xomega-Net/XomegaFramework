// load and execute require configurations first
require(['App_Start/RequireConfig', 'Views/ViewsConfig'], function () {
    // load the startup module next
    require(['App_Start/Startup']);
})
