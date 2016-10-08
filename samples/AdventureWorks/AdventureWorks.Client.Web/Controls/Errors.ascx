<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Errors.ascx.cs" Inherits="AdventureWorks.Client.Web.Errors" %>

<asp:Panel ID="pnlMessages" runat="server">
  <asp:ListView ID="errorList" runat="server">
    <LayoutTemplate>
      <p>
        Please view the following errors/warnings:
      </p>
      <ul runat="server" id="errLst">
        <li id="itemPlaceholder" runat="server"></li>
      </ul>
    </LayoutTemplate>
    <ItemTemplate>
      <li id="Li1" runat="server">
        <asp:Label ID="err" runat="server" Text='<%# Eval("Message") %>' CssClass='<%# Eval("Severity") %>'></asp:Label>
      </li>
    </ItemTemplate>
  </asp:ListView>
</asp:Panel>
