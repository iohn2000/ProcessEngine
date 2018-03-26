var kapsch = {



    control: function () {
        var controlHtml = "";
        controlHtml += "<input id='{{3}}' name='{{3}}' type='hidden' value='{{1}}'>";
        controlHtml += "<div style='width: 100 %; padding: 0'>";
        controlHtml += "<div id='prev{{0}}' style='width: 275px; color: #8c9099; border: 1px solid #ccc; padding-left: 15px;  padding-right: 15px; height: 60px; text-overflow: ellipsis; overflow: hidden;'></div>";
        controlHtml += "<div  style='float: right; width: 34px; height: 34px;'><button id='showPopup' style='margin: 0; padding: 7px' onClick='return false;' class='btn k-button kapsch-text-button'><i class='material-icons'>mode_edit</i></button></div>";
        controlHtml += "</div>";
        controlHtml += "<div id='kendoWindow'>";
        controlHtml += "<div style='height: 100%; width: 100%'>";
        controlHtml += "<textarea id='{{0}}editor' style='height: 80%; ' aria-label='editor'>{{1}}</textarea>";

        controlHtml += "<label></label>";
        controlHtml += " <div class='col-md-10' style='margin-top: 10px'>";
        controlHtml += "<button id='ok' class='k-button kapsch-text-button' style='float: right'>OK</button>";
        controlHtml += "<button id='cancel' class='k-button kapsch-text-button' style='float: right; margin-right: 10px;'>Cancel</button>";
        controlHtml += "</div>";
        controlHtml += "</div>";

        controlHtml += "</div>";

        this.id = null;
        this.text = null;
        this.idHiddenField = null;
        this.parent = null;
        this.kendoWindow = null;
        this.editor = null;
        this.toolConfig = kapsch.enum.mail;

        var _self = this;


        this.init = function (id, idHiddenField, title, text) {
            if (id != null) {
                _self.id = id;
            }
            if (idHiddenField != null) {
                _self.idHiddenField = idHiddenField;
            }
            if (title != null) {
                _self.title = title;
            }
            if (text != null) {
                _self.text = text;
            }

            // remove the old textarea, because otherwise we have double IDs
            var selectorTextarea = '#' + _self.id + 'editor';
            $(selectorTextarea).remove();

            _self.parent = $('#' + _self.id);
            controlHtml = controlHtml.replace('{{0}}', _self.id);
            controlHtml = controlHtml.replace('{{0}}', _self.id);
            controlHtml = controlHtml.replace('{{1}}', _self.text);
            controlHtml = controlHtml.replace('{{1}}', _self.text);
            controlHtml = controlHtml.replace('{{3}}', _self.idHiddenField);
            controlHtml = controlHtml.replace('{{3}}', _self.idHiddenField);

            _self.parent.html(controlHtml);


            _self.kendoWindow = _self.parent.find('#kendoWindow').kendoWindow({
                width: "600px",
                height: "600px",
                title: title,
                visible: false,
                actions: [
                    //   "Pin",
                    //    "Minimize",
                    "Maximize",
                    "Close"
                ]
                //,                close: _self.closeWindow()
            }).data("kendoWindow");

            //   kapschEditor.kendoWindow.element.find('#ok').click(function (e) {
            //    _self.onOkClicked(this)
            //});

            _self.kendoWindow.element.on('click', '#ok', function (e) {
                _self.onOkClicked(this)
            });

            _self.kendoWindow.element.on('click', '#cancel', function (e) {
                _self.editor.setText(_self.text);
                _self.closeWindow(this)
            });

            _self.parent.on('click', '#showPopup', function (e) {
                _self.showPopup(this)
            });

            _self.editor = new kapsch.editor();
            _self.editor.toolConfig = _self.toolConfig;
            // $('#editor').kendoEditor();
            _self.editor.init($('#' + _self.id + 'editor'));

            _self.fillPreviewbox(_self.text);
        }

        this.showPopup = function (e) {

            _self.kendoWindow.center().open();
        }

        this.onOkClicked = function (e) {
            _self.text = _self.editor.getText();

            _self.parent.find('#' + _self.idHiddenField).val(_self.text);

            var parser = new DOMParser;
            var dom = parser.parseFromString(
                '<!doctype html><body>' + _self.text,
                'text/html');

            _self.parent.find('#prev' + _self.id).html(dom.body.textContent);

            _self.closeWindow();
        }

        this.fillPreviewbox = function (text) {

            var parser = new DOMParser;
            var dom = parser.parseFromString(
                '<!doctype html><body>' + text,
                'text/html');

            _self.parent.find('#prev' + _self.id).html(dom.body.textContent);
        }

        this.closeWindow = function () {
            _self.kendoWindow.close();
        }
    },

    editor: function () {
        this.tagId = null;
        this.kendoEditor = null;
        this.text = null;

        this.toolConfig = null;

        var _self = this;

        this.init = function (selector) {
            _self.kendoEditor = $(selector).kendoEditor({
                tools: this.toolConfig
            });
        }

        this.getText = function () {
            return _self.kendoEditor.val()
        }

        this.setText = function (text) {
            var parser = new DOMParser;
            var dom = parser.parseFromString(
                '<!doctype html><body>' + text,
                'text/html');


            _self.kendoEditor.data('kendoEditor').value(dom.body.textContent);
        }

        this.destroy = function () {
            _self.kendoEditor.data('kendoEditor').destroy();
        }
    }
}

kapsch.enum = {
    text: ["bold", "italic", "underline", "insertUnorderedList", "insertOrderedList", "createLink"],
    mail: ["bold", "italic", "underline", "insertUnorderedList", "insertOrderedList", "createLink", "createTable"]
}