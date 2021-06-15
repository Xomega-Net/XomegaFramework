using System;

namespace AdventureWorks.Client.Objects
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

        // perform post intialization
        protected override void OnInitialized()
        {
            base.OnInitialized();
            EmailProperty.SetValue("jay1@adventure-works.com");
            PasswordProperty.SetValue("password");
            TrackModifications = false;
            IsNew = false;
        }

        // add custom code here
    }
}