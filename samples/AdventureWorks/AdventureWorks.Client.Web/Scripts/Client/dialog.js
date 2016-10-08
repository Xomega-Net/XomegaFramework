/*! Client functions for dialogs. */

/**
 * Modal view popup (using jQuery UI Dialog).
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
function modalViewPopup(action, viewID, uplContainerID, nestedInUpdatePanel)
{
    var targetID = uplContainerID ? uplContainerID : viewID;

    if (action === 'show') {
        var composition = $("#" + viewID);
        var view = composition.find(".view").first();
        var minW = parseInt(view.css("min-width"), 10);
        var settings = {
            title: view.find("> .view-header .view-title").text(),
            dialogClass: "popup-view",
            modal: true,
            width: minW > 0 ? minW + 'px' : 'auto',
            height: 'auto'
        };
        var dlg = $("#" + targetID).dialog(settings).parent();
        var content = dlg.children(".ui-dialog-content");
        var maxH = parseFloat(content.css("max-height"));
        if (content.height() - maxH >= 1)
            content.height(maxH);
        dlg.appendTo("form:first");
    }
    else if (action === 'hide') {
        var target = $(".ui-dialog > #" + targetID);
        if (nestedInUpdatePanel) {
            target.dialog("destroy");
            target.remove();
        }
        else target.dialog("close");
    }
}