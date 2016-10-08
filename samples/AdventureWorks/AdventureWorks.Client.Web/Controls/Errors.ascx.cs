using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AdventureWorks.Client.Web
{
    public partial class Errors : UserControl
    {
        public ListView List { get { return errorList; } }
    }
}