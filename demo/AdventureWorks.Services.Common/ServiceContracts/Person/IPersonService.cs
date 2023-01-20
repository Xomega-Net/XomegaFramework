//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "Service Contracts" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xomega.Framework.Services;

namespace AdventureWorks.Services.Common
{
    #region IPersonService interface

    ///<summary>
    /// Human beings involved with AdventureWorks: employees, customer contacts, and vendor contacts.
    ///</summary>
    public interface IPersonService
    {

        ///<summary>
        /// Authenticates a Person using email and password.
        ///</summary>
        Task<Output> AuthenticateAsync(Credentials _credentials, CancellationToken token = default);

        ///<summary>
        /// Reads person info by email as the key.
        ///</summary>
        Task<Output<PersonInfo>> ReadAsync(string _email, CancellationToken token = default);

        ///<summary>
        /// Reads enumeration data for Person Credit Card.
        ///</summary>
        Task<Output<ICollection<PersonCreditCard_ReadEnumOutput>>> CreditCard_ReadEnumAsync(int _businessEntityId, CancellationToken token = default);

    }
    #endregion

    #region PersonCreditCard_ReadEnumOutput structure

    ///<summary>
    /// The output structure of operation ICreditCardService.CreditCard_ReadEnumAsync.
    ///</summary>
    public class PersonCreditCard_ReadEnumOutput
    {

        ///<summary>
        /// Credit card identification number. Foreign key to CreditCard.CreditCardID.
        ///</summary>
        public int CreditCardId { get; set; }

        public string Description { get; set; }

        public string PersonName { get; set; }

        public string CardType { get; set; }

        public string CardNumber { get; set; }

        public byte ExpMonth { get; set; }

        public short ExpYear { get; set; }
    }
    #endregion

}