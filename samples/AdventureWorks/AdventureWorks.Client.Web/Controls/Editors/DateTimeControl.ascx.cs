using Xomega.Framework.Properties;
using Xomega.Framework.Web;

namespace AdventureWorks.Client.Web
{
    public partial class DateTimeControl : BaseDateTimeControl
    {
        public override void OnPropertyBound(DateTimeProperty prop)
        {
            base.OnPropertyBound(prop);
            extCalendar.Format = prop.Format;
        }
    }
}