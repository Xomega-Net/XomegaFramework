interface XomegaControlsModule {
    modalViewPopup(viewmodel, view): void;
    vSplitViewPopup(viewmodel, view): void;
    vSplitViewPanel(view): void;
}

declare module 'xomega-controls' {
    var theModule: XomegaControlsModule;
    export = theModule;
}
