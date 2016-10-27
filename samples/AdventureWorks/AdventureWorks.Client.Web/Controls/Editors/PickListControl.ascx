<%@ Control Language="C#" CodeBehind="PickListControl.ascx.cs" Inherits="AdventureWorks.Client.Web.PickListControl" %>

<asp:Panel ID="pnl_Wrapper" CssClass="picklist" runat="server">
  <asp:ListBox ID="lbx_Items" runat="server" />
  <asp:Panel ID="pnl_Buttons" CssClass="picklist-buttons" runat="server">
    <asp:Button ID="btn_AddAll" Text=">>" runat="server" />
    <asp:Button ID="btn_Add" Text=">" runat="server" />
    <asp:Button ID="btn_Remove" Text="<" runat="server" />
    <asp:Button ID="btn_RemoveAll" Text="<<" runat="server" />
  </asp:Panel>
  <asp:ListBox ID="lbx_Selection" runat="server" />
</asp:Panel>
