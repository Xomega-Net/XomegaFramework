using System;
using System.Collections.Generic;
using System.Web.UI;
using Xomega.Framework;

namespace AdventureWorks.Client.Web
{
    public partial class AppliedCriteria : UserControl
    {
        public void BindTo(List<FieldCriteriaSetting> settings)
        {
            pnlWidget.Attributes["title"] = Tooltip(settings);
            rptSettings.Visible = settings != null && settings.Count > 0;
            rptSettings.DataSource = settings;
            rptSettings.DataBind();
            lblNoData.Visible = !rptSettings.Visible;
        }

        private static string Tooltip(List<FieldCriteriaSetting> settings)
        {
            if (settings == null || settings.Count == 0) return null;
            string tooltip = string.Empty;
            foreach (var s in settings)
            {
                if (tooltip.Length > 0) tooltip += "; ";
                tooltip += s.Label + ":" +
                    (s.Operator != null ? " " + s.Operator : string.Empty) +
                    (s.Value.Length > 0 ? " " + string.Join(" and ", s.Value) : "");
            }
            return tooltip;
        }
    }
}