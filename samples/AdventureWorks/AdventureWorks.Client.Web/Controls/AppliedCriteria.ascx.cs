using System.Collections.Generic;
using System.Web.UI;
using Xomega.Framework;
using Xomega.Framework.Views;

namespace AdventureWorks.Client.Web
{
    public partial class AppliedCriteria : UserControl, IAppliedCriteriaPanel
    {
        public void BindTo(List<FieldCriteriaSetting> settings)
        {
            pnlWidget.Attributes["title"] = FieldCriteriaSetting.ToString(settings);
            rptSettings.Visible = settings != null && settings.Count > 0;
            rptSettings.DataSource = settings;
            rptSettings.DataBind();
            lblNoData.Visible = settings == null || settings.Count == 0;
        }
    }
}