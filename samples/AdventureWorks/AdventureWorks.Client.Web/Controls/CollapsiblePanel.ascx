<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CollapsiblePanel.ascx.cs" Inherits="AdventureWorks.Client.Web.CollapsiblePanel" %>

<%@ Register Namespace="AjaxControlToolkit" Assembly="AjaxControlToolkit" tagprefix="ajaxToolkit" %>

<div class="cpanel widget">
  <asp:Panel ID="Header" CssClass="widget-header" runat="server">
    <div class="widget-toolbar">
      <div class="widget-toolbar-icon">
        <asp:Label ID="lblIcon" CssClass="fa" runat="server" />
      </div>
    </div>
    <div class="widget-caption">
      <asp:Label ID="lblTitle" CssClass="widget-title" Text="Criteria" runat="server" />
    </div>
  </asp:Panel>
  <asp:Panel ID="Body" CssClass="widget-body" runat="server">
    <asp:PlaceHolder ID="Content" runat="server" />
  </asp:Panel>
  <ajaxToolkit:CollapsiblePanelExtender ID="cpe"
    targetcontrolid="Body"
    collapsecontrolid="Header"
    expandcontrolid="Header"
    collapsed="false"
    textlabelid="lblIcon"
    collapsedtext="&#xf078;"
    expandedtext="&#xf077;"
    collapsedsize="0"
    runat="server" />
</div>
