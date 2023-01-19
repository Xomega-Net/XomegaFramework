using System;
using Xomega.Framework;
using Xomega.Framework.Properties;

namespace AdventureWorks.Client.Common.DataObjects
{
    public class AuthenticationObjectCustomized : AuthenticationObject
    {
        public AuthenticationObjectCustomized()
        {
        }

        public AuthenticationObjectCustomized(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // construct properties and child objects
        protected override void Initialize()
        {
            base.Initialize();
            // add custom construction code here
        }

        // perform post initialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            TrackModifications = false;
            IsNew = false;
        }

        // add custom code here
    }
}