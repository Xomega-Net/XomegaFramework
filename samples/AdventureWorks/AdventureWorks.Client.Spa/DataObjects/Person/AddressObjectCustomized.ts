import { BusinessEntityAddress } from 'Enumerations/Enumerations';
import { Header } from 'xomega';

import { AddressObject as GeneratedDataObject } from 'DataObjects/Person/AddressObject';

export class AddressObject extends GeneratedDataObject {

    // perform post intialization
    onInitialized() {
        super.onInitialized();
        let obj = this;
        this.AddressId.InternalValue.subscribe(function (h: Header) {
            obj.AddressLine1.InternalValue(h ? h.attr[BusinessEntityAddress.Attributes.AddressLine1] : null);
            obj.AddressLine2.InternalValue(h ? h.attr[BusinessEntityAddress.Attributes.AddressLine2] : null);
            // for composite properties display null if an underlying property is null
            obj.CityState.InternalValue(h && h.attr[BusinessEntityAddress.Attributes.City] ?
                `${h.attr[BusinessEntityAddress.Attributes.City]}, ${h.attr[BusinessEntityAddress.Attributes.State]}` : null);
            obj.PostalCode.InternalValue(h ? h.attr[BusinessEntityAddress.Attributes.PostalCode] : null);
            obj.Country.InternalValue(h ? h.attr[BusinessEntityAddress.Attributes.Country] : null);
        });
    }
}

