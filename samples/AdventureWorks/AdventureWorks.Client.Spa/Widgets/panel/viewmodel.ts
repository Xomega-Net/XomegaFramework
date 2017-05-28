import { makeObservable } from 'xomega'

class PanelWidget {

    public collapsed: KnockoutObservable<boolean>;
    public settings: any;

    constructor() {
    }

    public activate(settings) {
        this.settings = settings;
        this.collapsed = makeObservable(settings.collapsed, false);
    }
}

export = PanelWidget;
