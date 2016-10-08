<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AppliedCriteria.ascx.cs" Inherits="AdventureWorks.Client.Web.AppliedCriteria" %>

<%@ Import Namespace="Xomega.Framework" %>

<asp:Panel ID="pnlWidget" class="applied-criteria" runat="server">
  <h4>Search Criteria</h4>
  <asp:Repeater ID="rptSettings" runat="server">
    <HeaderTemplate><span class="settings"></HeaderTemplate>
    <ItemTemplate>
      <span class="field-criteria">
        <span class="fc-label"><%# ((FieldCriteriaSetting)Container.DataItem).Label %></span>
        <span class="fc-operator"><%# ((FieldCriteriaSetting)Container.DataItem).Operator %></span>
        <asp:Repeater ID="rptValues" DataSource='<%# ((FieldCriteriaSetting)Container.DataItem).Value %>' runat="server">
          <ItemTemplate><span class="fc-datum"><%# (string)Container.DataItem %></span></ItemTemplate>
        </asp:Repeater>
      </span>
    </ItemTemplate>
    <FooterTemplate></span></FooterTemplate>
  </asp:Repeater>
  <span ID="lblNoData" class="settings" runat="server">None</span>
</asp:Panel>
