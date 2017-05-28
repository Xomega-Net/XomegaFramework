class AppliedCriteriaWidget {

    public settings: any;

    constructor() {
    }

    public activate(settings) {
        this.settings = settings;
        if (!settings.title)
            settings.title = 'Search Criteria';
    }
}

export = AppliedCriteriaWidget;
