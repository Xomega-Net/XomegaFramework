import { BusinessEntityAddressReadListCacheLoader } from 'CacheLoaders/BusinessEntityAddressReadListCacheLoader';
import { SalesOrderCustomerObject as GeneratedDataObject } from 'DataObjects/Sales/SalesOrderCustomerObject';
import { BusinessEntityAddress } from 'Enumerations/Enumerations';
import { LookupTable } from 'xomega';

export class SalesOrderCustomerObject extends GeneratedDataObject {

    // perform post intialization
    onInitialized() {
        super.onInitialized();
        this.LookupObject.TrackModifications = false;
        new CustomerAddressLoader(this);
    }
}

class CustomerAddressLoader extends BusinessEntityAddressReadListCacheLoader {

    private customer: SalesOrderCustomerObject;

    constructor(customer: SalesOrderCustomerObject) {
        super(true);
        this.customer = customer;
        customer.StoreId.InternalValue.subscribe(() => this.update(), this);
        customer.PersonId.InternalValue.subscribe(() => this.update(), this);
    }
    // overrde base method to provide proper input value(s)
    protected loadRequest(): JQueryAjaxSettings {
        let id = this.customer.StoreId.isNull() ? // use store or person id
            this.customer.PersonId.InternalValue() : this.customer.StoreId.InternalValue();
        return this.getLoadRequest(id);
    }
    private update() {
        if (this.customer.PersonId.isNull() && this.customer.StoreId.isNull()) return;
        let cl = this;
        this.loadCache(BusinessEntityAddress.EnumName, function (tbl: LookupTable) {
            cl.customer.BillingAddressObject.AddressId.setLookupTable(tbl);
            cl.customer.ShippingAddressObject.AddressId.setLookupTable(tbl);
        });
    }
}






