using System;
using System.Web.UI;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Web
{
    public partial class CollapsiblePanel : UserControl, ICollapsiblePanel
    {
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateInstance(TemplateInstance.Single)]
        public ITemplate ContentTemplate { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (ContentTemplate != null)
                ContentTemplate.InstantiateIn(Content);
        }

        public bool Collapsed
        {
            get { return cpe.ClientState == null ? cpe.Collapsed : cpe.ClientState.ToLower() == "true"; }
            set
            {
                cpe.Collapsed = value;
                cpe.ClientState = value.ToString();
            }
        }
    }
}