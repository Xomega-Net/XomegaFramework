<%@ Control Language="C#" CodeBehind="DateTimeControl.ascx.cs" Inherits="AdventureWorks.Client.Web.DateTimeControl" %>

<%@ Register Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" tagprefix="ajaxToolkit" %>

<asp:TextBox ID="txt_DateTime" runat="server" />
<ajaxToolkit:CalendarExtender ID="extCalendar" TargetControlID="txt_DateTime" runat="server"/>
