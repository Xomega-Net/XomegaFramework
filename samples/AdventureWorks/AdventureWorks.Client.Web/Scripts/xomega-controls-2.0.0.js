/*! Client functions for configuring some JQuery UI based controls. */

var xomegaControls = xomegaControls || {};

/**
 * Modal view popup for Web Forms (using jQuery UI Dialog).
 * Parameters:
 * - action:                Dialog action, can be 'show' or 'hide'.
 * - viewID:                ID of a view composition.
 * - uplContainerID:        If exists, ID of the UpdatePanel that contains the view.
 * - nestedInUpdatePanel:   A flag specifying if the UpdatePanel container lies within
 *                          another UpdatePanel, can be true or false.
 * Notes:
 * - Views within an UpdatePanel shall remain inside it even when shown as a dialog.
 *   Otherwise, since jQuery UI dialog displaces the target element in the DOM, a partial
 *   postback can reintroduce a second copy of the element to the DOM, which can break the
 *   page and widget due to duplicate content and IDs.
 * - Dialogs that host an UpdatePanel that was within another UpdatePanel shall be destroyed
 *   and parent UpdatePanel shall be refreshed server side when the dialog closes to restore
 *   the child UpdatePanel at its original position in the DOM within its parent UpdatePanel.
 *   Otherwise, the page and widget can break due to possible duplicate content and IDs.
 */
xomegaControls._modalViewPopup = function (action, viewID, uplContainerID, nestedInUpdatePanel) {

    var targetID = uplContainerID ? uplContainerID : viewID;

    if (action === 'show') {
        let target = $('#' + targetID);

        // close via view's button to fire a close event
        target.on('dialogclose', function () {
            target.find('.btn-close').click();
        });

        // workaround for a jQuery UI bug with calculating z-index
        // manifested by our moving the dialong inside the form element
        $.widget("ui.dialog", $.ui.dialog, {
            _moveToTop: function (event, silent) {
                var moved = false,
                    //zIndices = this.uiDialog.siblings(".ui-front:visible").map(function () {
                    zIndices = $(".ui-front:visible").map(function () {
                        return +$(this).css("z-index");
                    }).get(),
                    zIndexMax = Math.max.apply(null, zIndices);

                if (zIndexMax >= +this.uiDialog.css("z-index")) {
                    this.uiDialog.css("z-index", zIndexMax + 1);
                    moved = true;
                }

                if (moved && !silent) {
                    this._trigger("focus", event);
                }
                return moved;
            },
        });

        let dlg = xomegaControls.createViewDialog(target);
        dlg.appendTo('form:first'); // move inside the form element to handle postbacks
    }
    else if (action === 'hide') {
        let target = $('.ui-dialog > #' + targetID);
        if (nestedInUpdatePanel) {
            // destroy, the parent panel will need to update and recreate the view
            target.dialog('destroy').remove();
        } else target.dialog('close'); // don't destroy, or we'll lose the update panel
    }
}

/**
 * Creates and configures a popup dialog for the specified target that is or contains a view.
 */
xomegaControls.createViewDialog = function (target) {
    let view = target.find('.view').first();

    // use min-width from the view only to set the dialog's initial width
    let width = parseInt(view.attr('data-width'), 10);

    // create the dialog
    let dlg = target.dialog({
        dialogClass: 'popup-view',
        modal: true,
        width: width > 0 ? width + 'px' : 'auto',
        height: 'auto'
    }).parent();

    // set view header elements as the title
    let dlgTitle = dlg.find('.ui-dialog-title');
    dlgTitle.empty(); // remove &nbsp;
    let hdr = view.find('.view-header');
    dlgTitle.append(hdr.children());
    hdr.remove();

    // restrict height to min-height, if one is specified
    let height = parseInt(view.attr('data-height'));
    if (height > 0)
        dlg.children('.ui-dialog-content').height(height);

    return dlg;
}



/**
 * Splits the specified JQuery panel and configures the splitter
 */
xomegaControls.vSplitViewPanel = function (view) {
    var panel = $(view);
    if (!panel || panel.length < 1) return;

    panel.addClass('vsplitter-panel');
    for (var i = 0; i < panel.children().length; i++) {
        var pane = panel.children().eq(i);
        pane.addClass('pane' + (i + 1));
        pane.addClass('vsplitter-pane');
    }

    panel.children().eq(0).after(
        "<div class='vsplitter'>" +
        "  <div class='vsplitter-buttons'>" +
        "    <a class='left-pane-button'></a>" +
        "    <a class='right-pane-button'></a>" +
        "  </div>" +
        "</div>");

    var splitter = panel.children('.vsplitter');
    splitter.draggable({
        axis: 'x',
        distance: 1,
        cancel: '.vsplitter-buttons',
        start: function () {
            var $this = $(this);
            $this.data('lPaneWidth', $this.prev().width());
        },
        drag: function (event, ui) {
            var offset = ui.position.left - ui.originalPosition.left;
            if (Math.abs(offset) >= 1) {
                var $this = $(this);
                var lPaneWidth = Math.max(0, $this.data('lPaneWidth') + offset);
                var lPaneWidthPercent = Math.min(100, ((lPaneWidth / $this.parent().width()) * 100).toFixed(3)) + '%';
                $this.prev().css('width', lPaneWidthPercent);
            }
        }
    });
    splitter.css('position', 'static');

    var lButton = splitter.find('.left-pane-button');
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
            var view = rPane.find('.view').first()
            var lPaneWidthPercent;
            if (rPane.data('loaded'))
                lPaneWidthPercent = lPane.data('width');
            else {
                var width = parseFloat(view.attr('data-width'));
                var rPaneWidth = width > 0 ? width : 500; // default to 500px
                lPaneWidthPercent = 100 - (rPaneWidth / rPane.parent().width()) * 100;
                lPaneWidthPercent = Math.min(100, Math.max(0, lPaneWidthPercent)).toFixed(3) + '%';
                rPane.data('loaded', 'true');
            }
            lPane.css('width', lPaneWidthPercent);
            splitter.draggable('enable');
        }
        else {
            lPane.hide();
            $this.hide();
            splitter.draggable('disable');
        }
    });

    var rButton = splitter.find('.right-pane-button');
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
            splitter.draggable('enable');
        }
        else {
            lPane.data('width', lPane[0].style.width);
            lPane.css('width', '100%');
            rPane.hide();
            $this.hide();
            splitter.draggable('disable');
        }
    });

    var rightPanel = panel.find('> .pane2 > *');
    if (rightPanel.find('.view').length === 0)
        xomegaControls.vSplitViewVisibilityChange(rightPanel);
}

/**
 * Updates splitter when the child view is shown/hidden.
 */
xomegaControls.vSplitViewVisibilityChange = function (view, splitPanel) {
    var container = $(view);
    var panel = splitPanel || container.parents('.vsplitter-panel');
    if (panel.length === 0) return;

    var lButton = panel.find('> .vsplitter .left-pane-button');
    var rButton = panel.find('> .vsplitter .right-pane-button');
    var rPane = panel.children('.pane2');
    var visible = container.find('.view').length > 0;

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

/**
 * Vertical splitter panel for Web Forms.
 * Parameters:
 * - viewID:                ID of a view composition.
 */
xomegaControls._vSplitViewPanel = function (viewID) {
    xomegaControls.vSplitViewPanel($('#' + viewID + '.view-composition'));
}

/**
 * OnViewVisibilityChange handler for Web Forms.
 * Parameters:
 * - viewContainerId:       ID of a view container.
 */
xomegaControls._vSplitViewVisibilityChange = function (viewContainerId) {
    xomegaControls.vSplitViewVisibilityChange($('#' + viewContainerId));
}


/**
 * Configures JQueryUI date picker for the given input element.
 */
xomegaControls.datePicker = function (element, format) {
    // convert JS format to jqUI date picker format
    let jqFmt = format.replace(/m/g, '0') // reset minutes
                      .replace(/H/g, '0') // reset hours
                      .replace('YYYY', 'yy')
                      .replace('YY', 'y')
                      .replace(/M/g, 'm')
                      .replace(/D/g, 'd');
    $(element).datepicker({ dateFormat: jqFmt })
}

xomegaControls._autoComplete = function (settings) {
    xomegaControls.autoComplete(null, null, settings);
}

xomegaControls.autoComplete = function (element, property, settings) {
    let el = element ? $(element) : $('#' + settings.controlId);
    el.autocomplete({
        minLength: 0,
        source: function (request, response) {
            let multival = property ? property.IsMultiValued : settings.multivalue;
            let term = request.term;
            if (multival) {
                let str = term;
                let pos = el.get(0).selectionStart || 0;
                let delim = property ? property.DisplayListSeparator : settings.delimiter || ', ';
                let i1 = str.substring(0, pos).lastIndexOf(delim.trim()) + 1;
                term = str.substring(i1, pos).trim();
            }
            let items = property ? property.getPossibleValues().map(function (v) {
                return {
                    value: property.convertValue(v, xomega.ValueFormat.DisplayString),
                    editValue: property.convertValue(v, xomega.ValueFormat.EditString)
                };
            }) : settings.items;
            response($.ui.autocomplete.filter(items, term));
        },
        focus: function () {
            return false; // disable default behavior that automatically inserts focused items into textbox
        },
        select: function (event, ui) {
            let multival = property ? property.IsMultiValued : settings.multivalue;
            let val = ui.item.editValue || ui.item.value;
            if (multival) {
                let str = this.value;
                let pos = el.get(0).selectionStart || 0;
                let delim = property ? property.DisplayListSeparator : settings.delimiter || ', ';
                let i1 = str.substring(0, pos).lastIndexOf(delim.trim());
                let i2 = str.indexOf(delim.trim(), pos);
                this.value = (i1 < 0 ? '' : str.substr(0, i1) + delim) + val + (i2 < 0 ? '' : str.substring(i2));
                if (this.value.trim().indexOf(delim.trim(), this.value.trim().length - delim.trim().length) === -1)
                    this.value += delim; // add delimiter if value doesn't end with it
            } else {
                this.value = val;
            }
            return false;
        }
    });
}

/* =============================================================================
 * Functions for SPA controls that work with AMD (like RequreJS)
   =============================================================================*/

/**
 * Modal view popup dialog for SPA views
 */
xomegaControls.modalViewPopup = function (viewmodel, view) {
    let target = $(view);

    // try closing via view's button to fire a close event
    target.on('dialogbeforeclose', function () {
        target.find('.btn-close').click();
        return false; // prevent standard close
    });

    // destroy the dialog on view model close
    viewmodel.onViewEvent(function (view, event) {
        if (event.isClosed())
            target.dialog('destroy').remove();
    });

    xomegaControls.createViewDialog(target);
}

/**
 * Split view popup for SPA views
 */
xomegaControls.vSplitViewPopup = function (viewmodel, view) {
    let target = $(view);

    xomegaControls.vSplitViewVisibilityChange(target);

    // capture parent instead, since view will be detached from its splitter panel when closed
    let viewParent = target.parent();
    // update splitter on view model close
    viewmodel.onViewEvent(function (view, event) {
        if (event.isClosed()) xomegaControls.vSplitViewVisibilityChange(
            viewParent.children('.view-composition'), viewParent.parents('.vsplitter-panel'));
    });
}

/**
 * Export/define the view popup function that can be consumed in SPA
 */
if (typeof module === 'object' && module.exports) {
    // CommonJS (Node)
    module.exports = xomegaControls;
}
else if (typeof define === 'function' && define['amd']) {
    // AMD
    define(['jquery-ui'], function () {
        return xomegaControls;
    });
}
