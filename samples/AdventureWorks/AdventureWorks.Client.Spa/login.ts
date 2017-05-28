import router = require('plugins/router');
import LoginView = require('Views/Person/LoginViewCustomized');
import jwtDecode = require('jwt_decode');
import { AuthManager, ViewModel } from 'xomega';

class Login extends LoginView {

    //public getView() {
    //    return document.createElement('div');
    //}

    public activate(query) {
        // auto-login anonymously
        //$.ajax(this.getLoginRequest());
    }

    public onSave() {
        this.obj.validate(true);
        if (this.obj.ValidationErrors.hasErrors()) return;

        let req = this.getLoginRequest();
        req.data.username = this.obj.Email.EditStringValue();
        req.data.password = this.obj.Password.EditStringValue();
        req.error = (jqXHR, textStatus, errorThrow) => {
            this.obj.ValidationErrors.mergeWith(xomega.ErrorList.fromErrorResponse(jqXHR, errorThrow));
        };

        $.ajax(req);
    }

    private getLoginRequest(): JQueryAjaxSettings {
        let req: JQueryAjaxSettings = AuthManager.Current.createAjaxRequest();
        req.type = 'POST';
        req.url += 'oauth2/token';
        req.data = {
            grant_type: 'password',
        };
        let vm = this;
        req.success = function (data, textStatus, jqXHR) {
            var json = (<any>jqXHR).responseJSON;
            AuthManager.Current.signIn(json.access_token, jwtDecode(json.access_token));
            router.navigate('#' + vm.Params[xomega.AuthManager.ReturnParam]);
        };
        return req;
    }
}
export = Login;
