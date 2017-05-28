
import GeneratedViewModel = require('Views/Person/LoginView');

class LoginViewCustomized extends GeneratedViewModel {

    public getView() {
        // change this method to specify a custom view, can return a path to view or DOM element
        return 'Views/Person/LoginView.html';
    }

    public attached(view, parent) {
        let v = $(view);
        v.find('.btn-save').val('Login');
        v.css('width', '500px');
        this.obj.TrackModifications = false;
    }
}

export = LoginViewCustomized;
