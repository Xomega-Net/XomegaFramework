/*! Client functions for splitter panels. */

/**
 * Vertical splitter panel (using jQuery UI Draggable).
 * Parameters:
 * - viewID:                ID of a view composition.
 */
function vSplitterPanel(viewID)
{
    var panel = $("#" + viewID + ".view-composition");
    if (panel.length < 1) return;

    panel.addClass("vsplitter-panel");
    for (var i = 0; i < panel.children().length; i++) {
        var pane = panel.children().eq(i);
        pane.attr("ID", "Pane" + (i + 1));
        pane.addClass("vsplitter-pane");
    }

    panel.children().eq(0).after(
        "<div class='vsplitter'>" +
        "  <div class='vsplitter-buttons'>" +
        "    <a class='left-pane-button'></a>" +
        "    <a class='right-pane-button'></a>" +
        "  </div>" +
        "</div>");

    var splitter = panel.children(".vsplitter");
    splitter.draggable({
        axis: "x",
        distance: 1,
        cancel: ".vsplitter-buttons",
        start: function () {
            var $this = $(this);
            $this.data("lPaneWidth", $this.prev().width());
        },
        drag: function (event, ui) {
            var offset = ui.position.left - ui.originalPosition.left;
            if (Math.abs(offset) >= 1) {
                var $this = $(this);
                var lPaneWidth = Math.max(0, $this.data("lPaneWidth") + offset);
                var lPaneWidthPercent = Math.min(100, ((lPaneWidth / $this.parent().width()) * 100).toFixed(3)) + '%';
                $this.prev().css("width", lPaneWidthPercent);
            }
        }
    });
    splitter.css("position", "static");

    var lButton = splitter.find(".left-pane-button");
    lButton.click(function () {
        var $this = $(this);
        if ($this.hasClass('ui-state-disabled')) return;
        var splitter = $this.parent().parent();
        var lPane = splitter.prev();
        var rPane = splitter.next();
        var rb = $this.next();
        if (rPane.is(':hidden')) {
            rPane.show();
            rb.show();
            var lPaneWidthPercent;
            if (rPane.data("loaded"))
                lPaneWidthPercent = lPane.data("width");
            else {
                var minW = parseFloat(rPane.find(".view").first().css("min-width"));
                var rPaneWidth = minW > 0 ? minW : rPane.width();
                lPaneWidthPercent = 100 - (rPaneWidth / rPane.parent().width()) * 100;
                lPaneWidthPercent = Math.min(100, Math.max(0, lPaneWidthPercent)).toFixed(3) + '%';
                rPane.data("loaded", "true");
            }
            lPane.css("width", lPaneWidthPercent);
            splitter.draggable("enable");
        }
        else {
            lPane.hide();
            $this.hide();
            splitter.draggable("disable");
        }
    });

    var rButton = splitter.find(".right-pane-button");
    rButton.click(function () {
        var $this = $(this);
        if ($this.hasClass('ui-state-disabled')) return;
        var splitter = $this.parent().parent();
        var lPane = splitter.prev();
        var rPane = splitter.next();
        var lb = $this.prev();
        if (lPane.is(':hidden')) {
            lPane.show();
            lb.show();
            splitter.draggable("enable");
        }
        else {
            lPane.data("width", lPane[0].style.width);
            lPane.css("width", "100%");
            rPane.hide();
            $this.hide();
            splitter.draggable("disable");
        }
    });

    var rightView = panel.find("> #Pane2 > * > .view");
    if (rightView.length === 0)
        vSplitterPanel_OnViewVisibilityChange(panel.find("> #Pane2 > *").attr("ID"));
}

/**
 * OnViewVisibilityChange handler.
 * Parameters:
 * - viewContainerId:       ID of a view container.
 */
function vSplitterPanel_OnViewVisibilityChange(viewContainerId)
{
    var container = $("#" + viewContainerId);
    var panel = container.parent().parent();
    if (!panel.hasClass("vsplitter-panel")) return;

    var lButton = panel.find("> .vsplitter .left-pane-button");
    var rButton = panel.find("> .vsplitter .right-pane-button");
    var rPane = panel.children("#Pane2");
    var visible = container.find("> .view-composition").length > 0;

    if (visible && rPane.is(':hidden')) {
        lButton.removeClass('ui-state-disabled');
        lButton.click();
    }
    if (!visible && rPane.is(':visible')) {
        rButton.click();
        if (rPane.is(':visible'))
            rButton.click();
        lButton.addClass('ui-state-disabled');
    }
}