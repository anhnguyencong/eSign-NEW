export class PageListModel {
    items: any[];
    totalItems: number;
    hasMore: boolean;

    constructor() {
        this.items = [];
        this.totalItems = 0;
        this.hasMore = false;
    }
}
