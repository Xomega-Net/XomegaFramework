/*! Client functions for autocomplete feature on text boxes. */

/**
 * Autocomplete extender (using jQuery UI Autocomplete).
 * Settings:
 * - controlID:                 ID of the control
 * - items:                     list of possible values
 * - multivalue:                multivalue flag
 */
function autocomplete(settings)
{
    if (settings.multivalue) {

        function split(val) { return val.split(/,\s*/); }
        function extractLast(term) { return split(term).pop(); }

        $("#" + settings.controlID).autocomplete({
            minLength: 0,
            source: function (request, response) {
                // delegate back to autocomplete, but extract the last term
                response($.ui.autocomplete.filter(settings.items, extractLast(request.term)));
            },
            focus: function () {
                return false; // disable default behavior that automatically inserts focused items into textbox
            },
            select: function (event, ui) {
                var terms = split(this.value);
                terms.pop(); // remove the current input
                terms.push(ui.item.value); // add the selected item
                terms.push(""); // add placeholder to get the comma-and-space at the end
                this.value = terms.join(", ");
                return false;
            }
        });
    }
    else {
        $("#" + settings.controlID).autocomplete({
            source: settings.items
        });
    }
}