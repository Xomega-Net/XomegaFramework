<%---------------------------------------------------------------------------------------------
 This file was AUTO-GENERATED by "ASP.NET Search Pages" Xomega.Net generator.

 Manual CHANGES to this file WILL BE LOST when the code is regenerated.
----------------------------------------------------------------------------------------------%>

<%@ Control Language="C#" Inherits="AdventureWorks.Client.Web.SalesOrderListView" %>

<%@ Import Namespace="AdventureWorks.Client.Objects" %>
<%@ Register src="~/Controls/CollapsiblePanel.ascx" tagname="CollapsiblePanel" tagprefix="uc" %>
<%@ Register src="~/Controls/Editors/DateTimeControl.ascx" tagname="DateTimeControl" tagprefix="uc" %>
<%@ Register src="~/Controls/Errors.ascx" tagname="Errors" tagprefix="uc" %>
<%@ Register src="~/Controls/AppliedCriteria.ascx" tagname="AppliedCriteria" tagprefix="uc" %>
<%@ Register src="~/Pages/Sales/SalesOrderView.ascx" tagname="SalesOrderView" tagprefix="uc" %>

<asp:Panel ID="pnlComposition" CssClass="view-composition" runat="server">
  <asp:UpdatePanel ID="upl_Main" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
      <asp:Panel ID="pnlSalesOrderListView" CssClass="view vw-sales-order-list-view" runat="server">
        <div class="view-header">
          <asp:Label ID="lblSalesOrderListViewTitle" CssClass="view-title" Text="Sales Order List" runat="server"></asp:Label>
        </div>
        <div class="view-body">
          <div class="content indented">
            <uc:CollapsiblePanel ID="ucl_Criteria" runat="server">
              <ContentTemplate>
                <asp:Panel ID="pnlCriteria" CssClass="xw-obj" runat="server">
                  <table class="xw-fieldset-layout">
                    <tr>
                      <td class="fieldColumn">
                        <div class="field">
                          <asp:Label ID="lblSalesOrderNumber" Text="Sales Order Number:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblSalesOrderNumber" ID="ctlSalesOrderNumberOperator" Property="<%# SalesOrderCriteria.SalesOrderNumberOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlSalesOrderNumber" Property="<%# SalesOrderCriteria.SalesOrderNumber %>" runat="server"></asp:TextBox>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblStatus" Text="Status:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblStatus" ID="ctlStatusOperator" Property="<%# SalesOrderCriteria.StatusOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:DropDownList ID="ctlStatus" Property="<%# SalesOrderCriteria.Status %>" runat="server"></asp:DropDownList>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblOrderDate" Text="Order Date:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblOrderDate" ID="ctlOrderDateOperator" Property="<%# SalesOrderCriteria.OrderDateOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <uc:DateTimeControl ID="ctlOrderDate" Property="<%# SalesOrderCriteria.OrderDate %>" TextCssClass="date" runat="server"></uc:DateTimeControl>
                          <uc:DateTimeControl ID="ctlOrderDate2" Property="<%# SalesOrderCriteria.OrderDate2 %>" TextCssClass="date" runat="server"></uc:DateTimeControl>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblDueDate" Text="Due Date:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblDueDate" ID="ctlDueDateOperator" Property="<%# SalesOrderCriteria.DueDateOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <uc:DateTimeControl ID="ctlDueDate" Property="<%# SalesOrderCriteria.DueDate %>" TextCssClass="date" runat="server"></uc:DateTimeControl>
                          <uc:DateTimeControl ID="ctlDueDate2" Property="<%# SalesOrderCriteria.DueDate2 %>" TextCssClass="date" runat="server"></uc:DateTimeControl>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblTotalDue" Text="Total Due:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblTotalDue" ID="ctlTotalDueOperator" Property="<%# SalesOrderCriteria.TotalDueOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlTotalDue" Property="<%# SalesOrderCriteria.TotalDue %>" runat="server" CssClass="decimal"></asp:TextBox>
                          <asp:TextBox ID="ctlTotalDue2" Property="<%# SalesOrderCriteria.TotalDue2 %>" runat="server" CssClass="decimal"></asp:TextBox>
                        </div>
                      </td>
                      <td class="fieldColumn">
                        <div class="field">
                          <asp:Label ID="lblCustomerStore" Text="Customer Store:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblCustomerStore" ID="ctlCustomerStoreOperator" Property="<%# SalesOrderCriteria.CustomerStoreOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlCustomerStore" Property="<%# SalesOrderCriteria.CustomerStore %>" runat="server"></asp:TextBox>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblCustomerName" Text="Customer Name:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblCustomerName" ID="ctlCustomerNameOperator" Property="<%# SalesOrderCriteria.CustomerNameOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlCustomerName" Property="<%# SalesOrderCriteria.CustomerName %>" runat="server"></asp:TextBox>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblGlobalRegion" Text="Global Region:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblGlobalRegion" ID="ctlGlobalRegion" Property="<%# SalesOrderCriteria.GlobalRegion %>" runat="server"></asp:DropDownList>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblTerritoryId" Text="Territory Id:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblTerritoryId" ID="ctlTerritoryIdOperator" Property="<%# SalesOrderCriteria.TerritoryIdOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlTerritoryId" Property="<%# SalesOrderCriteria.TerritoryId %>" runat="server" CssClass="integer"></asp:TextBox>
                        </div>
                        <div class="field">
                          <asp:Label ID="lblSalesPersonId" Text="Sales Person Id:" CssClass="label" runat="server"></asp:Label>
                          <asp:DropDownList LabelID="lblSalesPersonId" ID="ctlSalesPersonIdOperator" Property="<%# SalesOrderCriteria.SalesPersonIdOperator %>" runat="server" AutoPostBack="true" CssClass="operator"></asp:DropDownList>
                          <asp:TextBox ID="ctlSalesPersonId" Property="<%# SalesOrderCriteria.SalesPersonId %>" runat="server" CssClass="integer"></asp:TextBox>
                        </div>
                      </td>
                    </tr>
                  </table>
                </asp:Panel>
              </ContentTemplate>
            </uc:CollapsiblePanel>
            <uc:Errors ID="errors" runat="server"></uc:Errors>
            <div class="action-bar">
              <asp:Button ID="btnSearch" Text="Search" OnClick="Search" runat="server"></asp:Button>
              <asp:Button ID="btnReset" Text="Reset" OnClick="Reset" runat="server"></asp:Button>
              <asp:LinkButton ID="lnkPermaLink" CssClass="permalink" Text="PermaLink" OnClick="lnkPermaLink_Click" runat="server"></asp:LinkButton>
            </div>
            <asp:Panel ID="pnlResults" CssClass="xw-obj" runat="server">
              <uc:AppliedCriteria ID="uclAppliedCriteria" runat="server"></uc:AppliedCriteria>
              <asp:GridView ID="grd_Results" runat="server">
                <Columns>
                  <asp:TemplateField HeaderText="Sales Order Number">
                    <ItemTemplate>
                      <asp:LinkButton ID="lnkDetails" runat="server" OnCommand="lnkDetails_Click" CommandArgument="<%# Container.DataItemIndex %>">
                        <asp:Label ID="fldSalesOrderNumber" Property="<%# SalesOrderList.SalesOrderNumber %>" runat="server"></asp:Label>
                      </asp:LinkButton>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                      <asp:Label ID="fldStatus" Property="<%# SalesOrderList.Status %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Order Date">
                    <ItemTemplate>
                      <asp:Label ID="fldOrderDate" Property="<%# SalesOrderList.OrderDate %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Ship Date">
                    <ItemTemplate>
                      <asp:Label ID="fldShipDate" Property="<%# SalesOrderList.ShipDate %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Due Date">
                    <ItemTemplate>
                      <asp:Label ID="fldDueDate" Property="<%# SalesOrderList.DueDate %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Total Due">
                    <ItemTemplate>
                      <asp:Label ID="fldTotalDue" Property="<%# SalesOrderList.TotalDue %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Online">
                    <ItemTemplate>
                      <asp:Label ID="fldOnlineOrderFlag" Property="<%# SalesOrderList.OnlineOrderFlag %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Customer Store">
                    <ItemTemplate>
                      <asp:Label ID="fldCustomerStore" Property="<%# SalesOrderList.CustomerStore %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Customer Name">
                    <ItemTemplate>
                      <asp:Label ID="fldCustomerName" Property="<%# SalesOrderList.CustomerName %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Sales Person Id">
                    <ItemTemplate>
                      <asp:Label ID="fldSalesPersonId" Property="<%# SalesOrderList.SalesPersonId %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                  <asp:TemplateField HeaderText="Territory Id">
                    <ItemTemplate>
                      <asp:Label ID="fldTerritoryId" Property="<%# SalesOrderList.TerritoryId %>" runat="server"></asp:Label>
                    </ItemTemplate>
                  </asp:TemplateField>
                </Columns>
              </asp:GridView>
              <asp:LinkButton ID="lnkNew" runat="server" OnCommand="lnkNew_Click">New</asp:LinkButton>
            </asp:Panel>
            <div class="action-bar">
              <asp:Button ID="btn_Select" Text="Select" OnClick="Select" runat="server"></asp:Button>
              <asp:Button ID="btn_Close" Text="Close" OnClick="Close" runat="server"></asp:Button>
            </div>
          </div>
        </div>
      </asp:Panel>
    </ContentTemplate>
  </asp:UpdatePanel>
  <asp:UpdatePanel ID="uplSalesOrderView" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
      <uc:SalesOrderView ID="uclSalesOrderView" Visible="false" runat="server"></uc:SalesOrderView>
    </ContentTemplate>
  </asp:UpdatePanel>
</asp:Panel>