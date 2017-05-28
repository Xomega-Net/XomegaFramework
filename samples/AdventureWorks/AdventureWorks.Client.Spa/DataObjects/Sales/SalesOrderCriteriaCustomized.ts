import { SalesOrderCriteria as GeneratedDataObject } from 'DataObjects/Sales/SalesOrderCriteria';
import { PersonType, SalesTerritory } from 'Enumerations/Enumerations'
import { AuthManager, AccessLevel, Header } from 'xomega'

export class SalesOrderCriteria extends GeneratedDataObject {

    // perform post intialization
    onInitialized() {
        super.onInitialized();
        this.Status.DisplayFormat = Header.fieldId + ' - ' + Header.fieldText;
        this.TerritoryId.setCascadingProperty(SalesTerritory.Attributes.Group, this.GlobalRegion);

        let claims = AuthManager.Current.Claims;
        if (claims && (claims.role == PersonType.StoreContact
                    || claims.role == PersonType.IndividualCustomer)) {
            this.CustomerStoreOperator.AccessLevel(AccessLevel.None);
            this.CustomerNameOperator.AccessLevel(AccessLevel.None);
        }
    }

    public validate(force: boolean) {
        super.validate(force);
        if (!this.OrderDate.isNull() && !this.OrderDate2.isNull() &&
            this.OrderDate.InternalValue() > this.OrderDate2.InternalValue())
            this.ValidationErrors.addError('From Order Date should be earlier than To Order Date');
    }
}