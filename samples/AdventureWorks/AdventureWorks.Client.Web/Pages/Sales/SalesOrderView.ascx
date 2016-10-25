<%---------------------------------------------------------------------------------------------
 This file was AUTO-GENERATED by "ASP.NET Details Pages" Xomega.Net generator.

 Manual CHANGES to this file WILL BE LOST when the code is regenerated.
----------------------------------------------------------------------------------------------%>

<%@ Control Language="C#" Inherits="AdventureWorks.Client.Web.SalesOrderView" %>

<%@ Import Namespace="AdventureWorks.Client.Objects" %>
<%@ Register Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" tagprefix="ajaxToolkit" %>
<%@ Register src="~/Controls/Errors.ascx" tagname="Errors" tagprefix="uc" %>
<%@ Register src="~/Controls/Editors/DateTimeControl.ascx" tagname="DateTimeControl" tagprefix="uc" %>
<%@ Register src="~/Pages/Sales/SalesOrderDetailView.ascx" tagname="SalesOrderDetailView" tagprefix="uc" %>
<%@ Register src="~/Pages/Sales/SalesOrderReasonView.ascx" tagname="SalesOrderReasonView" tagprefix="uc" %>

<asp:Panel ID="pnlComposition" CssClass="view-composition" runat="server">
  <asp:UpdatePanel ID="upl_Main" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
      <asp:Panel ID="pnlSalesOrderView" CssClass="view with-footer vw-sales-order-view" runat="server">
        <div class="view-header">
          <asp:Label ID="lblSalesOrderViewTitle" CssClass="view-title" Text="Sales Order" runat="server"></asp:Label>
        </div>
        <div class="view-body">
          <div class="content indented">
            <uc:Errors ID="errors" runat="server"></uc:Errors>
            <asp:Panel ID="pnl_Object" CssClass="indented" runat="server">
              <asp:Panel ID="pnlMain" CssClass="xw-obj" GroupingText="Sales Order" runat="server">
                <table class="xw-fieldset-layout">
                  <tr>
                    <td class="fieldColumn">
                      <div class="field">
                        <asp:Label ID="lblSalesOrderNumber" Text="Sales Order Number:" CssClass="label" runat="server"></asp:Label>
                        <asp:Label LabelID="lblSalesOrderNumber" ID="ctlSalesOrderNumber" Property="<%# SalesOrderObject.SalesOrderNumber %>" runat="server"></asp:Label>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblOrderDate" Text="Order Date:" CssClass="label" runat="server"></asp:Label>
                        <asp:Label LabelID="lblOrderDate" ID="ctlOrderDate" Property="<%# SalesOrderObject.OrderDate %>" runat="server"></asp:Label>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblDueDate" Text="Due Date:" CssClass="label" runat="server"></asp:Label>
                        <uc:DateTimeControl LabelID="lblDueDate" ID="ctlDueDate" Property="<%# SalesOrderObject.DueDate %>" TextCssClass="datetime" runat="server"></uc:DateTimeControl>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblShipDate" Text="Ship Date:" CssClass="label" runat="server"></asp:Label>
                        <uc:DateTimeControl LabelID="lblShipDate" ID="ctlShipDate" Property="<%# SalesOrderObject.ShipDate %>" TextCssClass="datetime" runat="server"></uc:DateTimeControl>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblStatus" Text="Status:" CssClass="label" runat="server"></asp:Label>
                        <asp:DropDownList LabelID="lblStatus" ID="ctlStatus" Property="<%# SalesOrderObject.Status %>" runat="server"></asp:DropDownList>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblOnlineOrderFlag" Text="Online Order Flag:" CssClass="label" runat="server"></asp:Label>
                        <asp:CheckBox LabelID="lblOnlineOrderFlag" ID="ctlOnlineOrderFlag" Property="<%# SalesOrderObject.OnlineOrderFlag %>" runat="server"></asp:CheckBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblPurchaseOrderNumber" Text="Purchase Order Number:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblPurchaseOrderNumber" ID="ctlPurchaseOrderNumber" Property="<%# SalesOrderObject.PurchaseOrderNumber %>" runat="server"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblAccountNumber" Text="Account Number:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblAccountNumber" ID="ctlAccountNumber" Property="<%# SalesOrderObject.AccountNumber %>" runat="server"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblCustomerId" Text="Customer Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblCustomerId" ID="ctlCustomerId" Property="<%# SalesOrderObject.CustomerId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblSalesPersonId" Text="Sales Person Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:DropDownList LabelID="lblSalesPersonId" ID="ctlSalesPersonId" Property="<%# SalesOrderObject.SalesPersonId %>" runat="server"></asp:DropDownList>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblTerritoryId" Text="Territory Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:DropDownList LabelID="lblTerritoryId" ID="ctlTerritoryId" Property="<%# SalesOrderObject.TerritoryId %>" runat="server"></asp:DropDownList>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblBillToAddressId" Text="Bill To Address Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblBillToAddressId" ID="ctlBillToAddressId" Property="<%# SalesOrderObject.BillToAddressId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                    </td>
                    <td class="fieldColumn">
                      <div class="field">
                        <asp:Label ID="lblShipToAddressId" Text="Ship To Address Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblShipToAddressId" ID="ctlShipToAddressId" Property="<%# SalesOrderObject.ShipToAddressId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblShipMethodId" Text="Ship Method Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblShipMethodId" ID="ctlShipMethodId" Property="<%# SalesOrderObject.ShipMethodId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblCreditCardId" Text="Credit Card Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblCreditCardId" ID="ctlCreditCardId" Property="<%# SalesOrderObject.CreditCardId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblCreditCardApprovalCode" Text="Credit Card Approval Code:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblCreditCardApprovalCode" ID="ctlCreditCardApprovalCode" Property="<%# SalesOrderObject.CreditCardApprovalCode %>" runat="server"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblCurrencyRateId" Text="Currency Rate Id:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblCurrencyRateId" ID="ctlCurrencyRateId" Property="<%# SalesOrderObject.CurrencyRateId %>" runat="server" CssClass="integer"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblSubTotal" Text="Sub Total:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblSubTotal" ID="ctlSubTotal" Property="<%# SalesOrderObject.SubTotal %>" runat="server" CssClass="decimal"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblTaxAmt" Text="Tax Amt:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblTaxAmt" ID="ctlTaxAmt" Property="<%# SalesOrderObject.TaxAmt %>" runat="server" CssClass="decimal"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblFreight" Text="Freight:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblFreight" ID="ctlFreight" Property="<%# SalesOrderObject.Freight %>" runat="server" CssClass="decimal"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblTotalDue" Text="Total Due:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblTotalDue" ID="ctlTotalDue" Property="<%# SalesOrderObject.TotalDue %>" runat="server" CssClass="decimal"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblComment" Text="Comment:" CssClass="label" runat="server"></asp:Label>
                        <asp:TextBox LabelID="lblComment" ID="ctlComment" Property="<%# SalesOrderObject.Comment %>" runat="server"></asp:TextBox>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblRevisionNumber" Text="Revision Number:" CssClass="label" runat="server"></asp:Label>
                        <asp:Label LabelID="lblRevisionNumber" ID="ctlRevisionNumber" Property="<%# SalesOrderObject.RevisionNumber %>" runat="server"></asp:Label>
                      </div>
                      <div class="field">
                        <asp:Label ID="lblModifiedDate" Text="Modified Date:" CssClass="label" runat="server"></asp:Label>
                        <asp:Label LabelID="lblModifiedDate" ID="ctlModifiedDate" Property="<%# SalesOrderObject.ModifiedDate %>" runat="server"></asp:Label>
                      </div>
                    </td>
                  </tr>
                </table>
              </asp:Panel>
              <ajaxToolkit:TabContainer ID="tbcChildren" ActiveTabIndex="1" runat="server">
                <ajaxToolkit:TabPanel ID="tabDetail" HeaderText="Detail" runat="server">
                  <ContentTemplate>
                    <asp:Panel ID="pnlDetail" CssClass="xw-obj" runat="server">
                      <asp:GridView ID="gridDetail" runat="server" ChildObject="<%# SalesOrderObject.Detail %>">
                        <Columns>
                          <asp:TemplateField HeaderText="Product">
                            <ItemTemplate>
                              <asp:LinkButton ID="lnkDetailDetails" runat="server" OnCommand="lnkDetailDetails_Click" CommandArgument="<%# Container.DataItemIndex %>">
                                <asp:Label ID="fldProduct" Property="<%# SalesOrderDetailList.Product %>" runat="server"></asp:Label>
                              </asp:LinkButton>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Qty">
                            <ItemTemplate>
                              <asp:Label ID="fldOrderQty" Property="<%# SalesOrderDetailList.OrderQty %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Price">
                            <ItemTemplate>
                              <asp:Label ID="fldUnitPrice" Property="<%# SalesOrderDetailList.UnitPrice %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Discount">
                            <ItemTemplate>
                              <asp:Label ID="fldUnitPriceDiscount" Property="<%# SalesOrderDetailList.UnitPriceDiscount %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Special Offer">
                            <ItemTemplate>
                              <asp:Label ID="fldSpecialOffer" Property="<%# SalesOrderDetailList.SpecialOffer %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Total">
                            <ItemTemplate>
                              <asp:Label ID="fldLineTotal" Property="<%# SalesOrderDetailList.LineTotal %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Tracking #">
                            <ItemTemplate>
                              <asp:Label ID="fldCarrierTrackingNumber" Property="<%# SalesOrderDetailList.CarrierTrackingNumber %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                        </Columns>
                      </asp:GridView>
                      <asp:LinkButton ID="lnkDetailNew" runat="server" OnCommand="lnkDetailNew_Click">New</asp:LinkButton>
                    </asp:Panel>
                  </ContentTemplate>
                </ajaxToolkit:TabPanel>
                <ajaxToolkit:TabPanel ID="tabReason" HeaderText="Reason" runat="server">
                  <ContentTemplate>
                    <asp:Panel ID="pnlReason" CssClass="xw-obj" runat="server">
                      <asp:GridView ID="gridReason" runat="server" ChildObject="<%# SalesOrderObject.Reason %>">
                        <Columns>
                          <asp:TemplateField HeaderText="Sales Reason Id">
                            <ItemTemplate>
                              <asp:LinkButton ID="lnkReasonDetails" runat="server" OnCommand="lnkReasonDetails_Click" CommandArgument="<%# Container.DataItemIndex %>">
                                <asp:Label ID="fldSalesReasonId" Property="<%# SalesOrderReasonList.SalesReasonId %>" runat="server"></asp:Label>
                              </asp:LinkButton>
                            </ItemTemplate>
                          </asp:TemplateField>
                          <asp:TemplateField HeaderText="Modified Date">
                            <ItemTemplate>
                              <asp:Label ID="fldModifiedDate" Property="<%# SalesOrderReasonList.ModifiedDate %>" runat="server"></asp:Label>
                            </ItemTemplate>
                          </asp:TemplateField>
                        </Columns>
                      </asp:GridView>
                      <asp:LinkButton ID="lnkReasonNew" runat="server" OnCommand="lnkReasonNew_Click">New</asp:LinkButton>
                    </asp:Panel>
                  </ContentTemplate>
                </ajaxToolkit:TabPanel>
              </ajaxToolkit:TabContainer>
            </asp:Panel>
          </div>
        </div>
        <div class="view-footer action-bar">
          <asp:Button ID="btnSave" Text="Save" OnClick="btnSave_Click" runat="server"></asp:Button>
          <asp:Button ID="btnDelete" Text="Delete" OnClick="btnDelete_Click" Enabled="false" OnClientClick="if (!confirm('Are you sure you want to delete this object?\nThis action cannot be undone.')) return false;" runat="server"></asp:Button>
          <asp:Button ID="btn_Close" Text="Close" OnClick="Close" runat="server"></asp:Button>
        </div>
      </asp:Panel>
    </ContentTemplate>
  </asp:UpdatePanel>
  <asp:UpdatePanel ID="uplSalesOrderDetailView" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
      <uc:SalesOrderDetailView ID="uclSalesOrderDetailView" Visible="false" runat="server"></uc:SalesOrderDetailView>
    </ContentTemplate>
  </asp:UpdatePanel>
  <asp:UpdatePanel ID="uplSalesOrderReasonView" UpdateMode="Conditional" runat="server">
    <ContentTemplate>
      <uc:SalesOrderReasonView ID="uclSalesOrderReasonView" Visible="false" runat="server"></uc:SalesOrderReasonView>
    </ContentTemplate>
  </asp:UpdatePanel>
</asp:Panel>