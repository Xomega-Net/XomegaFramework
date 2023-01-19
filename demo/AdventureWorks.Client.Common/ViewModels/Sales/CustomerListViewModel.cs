//---------------------------------------------------------------------------------------------
// This file was AUTO-GENERATED by "View Models" Xomega.Net generator.
//
// Manual CHANGES to this file WILL BE LOST when the code is regenerated.
//---------------------------------------------------------------------------------------------

using AdventureWorks.Client.Common.DataObjects;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Common.ViewModels
{
    public partial class CustomerListViewModel : SearchViewModel
    {
        public CustomerList ListObj => List as CustomerList;
        public CustomerCriteria CritObj => List.CriteriaObject as CustomerCriteria;

        public CustomerListViewModel(IServiceProvider sp) : base(sp)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            List = ServiceProvider.GetService<CustomerList>();
            List.CriteriaObject = ServiceProvider.GetService<CustomerCriteria>();
        }
    }
}