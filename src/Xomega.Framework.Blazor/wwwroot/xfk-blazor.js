// Copyright (c) 2023 Xomega.Net. All rights reserved.

var xfk = xfk || {};

// utility function to set up auto-complete elements to prevent some default actions
xfk.autoCompletePreventDefault = function (element, remove) {
    if (remove) {
        element.removeEventListener('keydown', xfk.preventAutoCompleteDefault, false);
    }
    else {
        element.addEventListener('keydown', xfk.preventAutoCompleteDefault, false);
    }
};

// utility function to prevent default actions for auto-complete elements with a dropdown open
xfk.preventAutoCompleteDefault = function (e) {
    var dropDown = e.target.parentNode.getElementsByTagName('ul');
    var ddOpen = dropDown.length && dropDown[0].classList.contains('show');
    if (ddOpen && (e.keyCode === 13 || e.key === "Enter" ||
        e.keyCode === 38 || e.key === "ArrowUp" ||
        e.keyCode === 40 || e.key === "ArrowDown")) {
        e.preventDefault()
        return false;
    }
}


// utility function to get an element property
xfk.getProperty = function (element, property) {
    return (element && property) ? element[property] : null;
};

// utility function to set an element property
xfk.setProperty = function (element, property, value) {
    if (element && property) {
        element[property] = value;
    }
};

// shows or hides a Bootstrap modal dialog
xfk.modalViewPopup = function (show, view) {
    var modal = bootstrap.Modal.getInstance(view);
    if (!modal) {
        if (show) modal = new bootstrap.Modal(view, { backdrop: 'static', keyboard: false });
        else return;
    }
    if (show) modal.show();
    else {
        modal.hide();
        modal.dispose();
    }
};

// utility function to set up Bootstrap popovers
xfk.setupPopovers = function (element) {
    const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]')
    const popoverList = [...popoverTriggerList].map(popoverTriggerEl => new bootstrap.Popover(popoverTriggerEl))
};