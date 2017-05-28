import { MainMenu } from 'Views/MainMenu';
import * as app from 'durandal/app';
import * as ko from 'knockout';
import * as router from 'plugins/router';
import { PersonType } from 'Enumerations/Enumerations';
import { AuthManager } from 'xomega';

// default routes configuration
let Routes = [
    { route: '', moduleId: 'Views/home', title: 'Home', hash: '#' },
    { route: 'login', moduleId: 'login', title: 'Login', hash: '', allowAnonymous: true },
];

class Shell {

    public title: string = app.title;
    public menu: any = [];
    public loggedIn: KnockoutComputed<boolean> = ko.computed(function () { return AuthManager.Current.LoggedIn(); });

    public userName() {
        let am = AuthManager.Current;
        return am.LoggedIn() && am.Claims ? am.Claims.unique_name : '';
    }

    public logout() {
        AuthManager.Current.signOut();
    }

    public activate() {

        // map routes
        AuthManager.forEachItem(Routes, this.mapRoute, this);
        AuthManager.forEachItem(MainMenu, this.mapRoute, this);

        // build menu
        this.menu.push(Routes[0]); // home
        this.menu = this.menu.concat(MainMenu);
        AuthManager.forEachItem(this.menu, this.setUpRoles, this);
        AuthManager.forEachItem(this.menu, AuthManager.Current.setUpAllowed, AuthManager.Current);

        let rootRouter = router;
        rootRouter.guardRoute = AuthManager.guardRoute;

        return router.activate();
    }

    private mapRoute(item) {
        if (item.route != null) router.map(item);
    }

    private setUpRoles(item) {
        // TODO: check item.route and set item.roles array accordingly
        if (item.route != null && item.route.indexOf('Sales') >= 0)
            item.roles = [PersonType.StoreContact, PersonType.IndividualCustomer, PersonType.SalesPerson];
    }
}

export = Shell;
