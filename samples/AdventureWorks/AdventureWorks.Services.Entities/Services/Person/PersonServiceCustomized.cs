using System;
using System.Linq;
using Xomega.Framework;

namespace AdventureWorks.Services.Entities
{
    public class PersonServiceCustomized : PersonService
    {
        public PersonServiceCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override void Authenticate(Credentials _credentials)
        {
            // lookup password
            var pwdQry = from em in ctx.EmailAddress
                         join pw in ctx.Password on em.BusinessEntityId equals pw.BusinessEntityId
                         where em.EmailAddress1 == _credentials.Email
                         select pw;
            var pwd = pwdQry.FirstOrDefault();

            // validate credentials
            bool valid = false;
            if (pwd != null && _credentials.Password != null)
            {
                valid = _credentials.Password.Equals("password"); // for testing only
                // TODO: hash _credentials.Password using pwd.PasswordSalt,
                //       and compare it with pwd.PasswordHash instead
            }
            if (!valid) currentErrors.CriticalError(ErrorType.Security, "Invalid credentials");
        }

        public override PersonInfo Read(string _email)
        {
            // lookup and return person info
            var qry = from em in ctx.EmailAddress
                      join ps in ctx.Person on em.BusinessEntityId equals ps.BusinessEntityId
                      join bc in ctx.BusinessEntityContact on ps.BusinessEntityId equals bc.PersonId into bec
                      from bc in bec.DefaultIfEmpty()
                      join st in ctx.Store on bc.BusinessEntityId equals st.BusinessEntityId into store
                      join vn in ctx.Vendor on bc.BusinessEntityId equals vn.BusinessEntityId into vendor
                      from st in store.DefaultIfEmpty()
                      from vn in vendor.DefaultIfEmpty()
                      where em.EmailAddress1 == _email
                      select new PersonInfo
                      {
                          BusinessEntityId = ps.BusinessEntityId,
                          PersonType = ps.PersonType,
                          FirstName = ps.FirstName,
                          LastName = ps.LastName,
                          Email = em.EmailAddress1,
                          Store = st.BusinessEntityId,
                          Vendor = vn.BusinessEntityId
                      };
            var person = qry.FirstOrDefault();
            if (person == null) currentErrors.CriticalError(ErrorType.Data, "Person info not found");

            return person;
        }
    }
}