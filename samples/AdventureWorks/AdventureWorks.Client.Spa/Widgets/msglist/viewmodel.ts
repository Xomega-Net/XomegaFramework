import { ErrorSeverity } from 'xomega'

class MessageListWidget {

    public settings: any;

    public activate(settings) {
        this.settings = settings;
    }

    public iconClass(severity): string {
        switch (severity) {
            case ErrorSeverity.Critical:
            case ErrorSeverity.Error: return 'fa-exclamation-circle';
            case ErrorSeverity.Warning: return 'fa-times-circle';
            case ErrorSeverity.Info: return 'fa-info-circle';
        }
        return '';
    }
}

export = MessageListWidget;
