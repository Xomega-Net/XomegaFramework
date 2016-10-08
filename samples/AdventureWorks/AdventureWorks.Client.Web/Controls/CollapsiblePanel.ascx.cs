using System;
using System.Web.UI;
using Xomega.Framework.Web;

namespace AdventureWorks.Client.Web
{
    public partial class CollapsiblePanel : BaseCollapsiblePanel
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

        public override bool Collapsed
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