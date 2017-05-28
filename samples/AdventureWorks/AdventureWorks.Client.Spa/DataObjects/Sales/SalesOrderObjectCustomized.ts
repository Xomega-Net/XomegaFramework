import { PersonCreditCardReadListCacheLoader } from 'CacheLoaders/PersonCreditCardReadListCacheLoader';
import { SalesOrderObject as GeneratedDataObject } from 'DataObjects/Sales/SalesOrderObject';
import { BusinessEntityAddress } from 'Enumerations/Enumerations';
import { LookupTable } from 'xomega';

export class SalesOrderObject extends GeneratedDataObject {

    // perform post intialization
    onInitialized() {
        super.onInitialized();
        new PersonCreditCardLoader(this);
    }
}

class PersonCreditCardLoader extends PersonCreditCardReadListCacheLoader {

    private salesOrder: SalesOrderObject;

    constructor(salesOrder: SalesOrderObject) {
        super(true);
        this.salesOrder = salesOrder;
        salesOrder.CustomerObject.PersonId.InternalValue.subscribe(() => this.update(), this);
    }

    // overrde base method to provide proper input value(s)
    protected loadRequest(): JQueryAjaxSettings {
        return this.getLoadRequest(this.salesOrder.CustomerObject.PersonId.InternalValue());
    }

    public update() {
        if (this.salesOrder.CustomerObject.PersonId.isNull()) return;
        let cl = this;
        this.loadCache(BusinessEntityAddress.EnumName, function (tbl: LookupTable) {
            cl.salesOrder.PaymentObject.CreditCardObject.CreditCardId.setLookupTable(tbl);
        });
    }
}