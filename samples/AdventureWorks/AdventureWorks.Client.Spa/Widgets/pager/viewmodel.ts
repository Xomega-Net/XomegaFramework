import * as ko from 'knockout';
import { makeObservable } from 'xomega'

class PagerWidget {

    public pageSizes: number[];
    public pageSize: KnockoutObservable<number>;
    public maxPage: KnockoutComputed<number>;
    public curPage: KnockoutObservable<number>;
    public pageFrom: KnockoutComputed<number>;
    public pageTo: KnockoutComputed<number>;
    public pagedItems: KnockoutComputed<any[]>;
    public settings: any;

    constructor() {
    }

    public activate(settings) {
        this.settings = settings;
        // paginate items
        this.pageSizes = this.settings.pageSizes || [10, 20, 50, 100];
        this.pageSize = makeObservable(this.settings.pageSize, 20);
        this.curPage = makeObservable(this.settings.curPage, 1);
        this.maxPage = ko.computed(function () {
            return Math.ceil(this.settings.items().length / this.pageSize());
        }, this);
        this.maxPage.subscribe(function () {
            this.curPage(1);
        }, this);
        var pagerSize: number = this.settings.pagerSize || 5;
        var pagerOffset: number = Math.floor(pagerSize / 2);
        pagerSize = pagerOffset * 2 + 1; // ensure odd pager size
        this.pageFrom = ko.computed(function () {
            return Math.max(1, Math.min(this.maxPage() - pagerSize + 1, this.curPage() - pagerOffset));
        }, this);
        this.pageTo = ko.computed(function () {
            return Math.min(this.maxPage(), Math.max(pagerSize, this.curPage() + pagerOffset));
        }, this);
        this.pagedItems = ko.computed(function () {
            var page = this.curPage(),
                start = this.pageSize() * (page - 1),
                end = start + this.pageSize();
            return this.settings.items().slice(start, end);
        }, this);
    }
}

export = PagerWidget;
