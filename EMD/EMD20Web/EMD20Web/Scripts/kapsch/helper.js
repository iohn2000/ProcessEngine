

var helper = {
    IsInternetExplorer: function () {
        var ua = window.navigator.userAgent;

        var msie = ua.indexOf('MSIE ');
        if (msie > 0) {
            // IE 10 or older => return version number
            return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
        }

        var trident = ua.indexOf('Trident/');
        if (trident > 0) {
            // IE 11 => return version number
            var rv = ua.indexOf('rv:');
            return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
        }

        var edge = ua.indexOf('Edge/');
        if (edge > 0) {
            // Edge (IE 12+) => return version number
            return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
        }

        // other browser
        return false;
    }
}

helper.Functions = {

    AutoResize: function () {
        jQuery.each(jQuery('textarea[data-autoresize]'), function () {
            var offset = this.offsetHeight - this.clientHeight;

            var resizeTextarea = function (el) {
                jQuery(el).css('height', 'auto').css('height', el.scrollHeight + offset);
            };
            //   jQuery(this).on('keyup input', function () { resizeTextarea(this); }).removeAttr('data-autoresize');

            resizeTextarea(this);
        });
    },

    InitialzePopupTooltips: function () {
        $('.kapschmodalwindow .windowMetaDataTooltip[initialized="false"]').each(function () {
            var tooltip = $(this).attr('tooltip');
            var tooltipUrl = $(this).attr('tooltipUrl');
            var tooltipWidth = parseInt($(this).attr('tooltipWidth'));
            var tooltipHeight = parseInt($(this).attr('tooltipHeight'));

            $(this).attr('initialized', 'true');

            var $tooltipnode = $(this).closest('.kapschmodalwindow').find('.info-tooltip.popup');
            var $tooltipnodeIcon = $(this).closest('.kapschmodalwindow').find('.info-tooltip.popup > i');

            if ($tooltipnode.length > 0 && $tooltipnodeIcon.length > 0) {

                if (tooltipUrl && tooltipUrl.length > 0) {
                    $tooltipnode.show();

                    $tooltipnodeIcon.kendoTooltip({
                        animation: {
                            close: {
                                duration: 100
                            }
                        },
                        autoHide: false,
                        content: {
                            url: tooltipUrl
                        },
                        width: tooltipWidth,
                        height: tooltipHeight


                    });
                }
                else if (tooltip && tooltip.length > 0) {
                    $tooltipnode.show();

                    $tooltipnodeIcon.kendoTooltip({
                        animation: {
                            close: {
                                duration: 100
                            }
                        },
                        autoHide: false,
                        content: function (e) {
                            return tooltip;
                        }
                    });
                }
            }
        });

    },

    ConvertToCSV: function (objArray, removeLineBreaks) {
        var array = typeof objArray != 'object' ? JSON.parse(objArray) : objArray;
        var str = '';

        for (var i = 0; i < array.length; i++) {
            var line = '';
            for (var index in array[i]) {
                if (line != '') line += '||'

                var text = array[i][index];
                if (removeLineBreaks) {
                    text = text.toString().replace(/\r?\n|\r/g, " ");
                }

                line += text;
            }

            str += line + '\r\n';
        }

        return str;
    },

    InvokeSaveAsDialog: function (file, fileName) {
        if (!file) {
            throw 'Blob object is required.';
        }

        if (!file.type) {
            file.type = 'video/webm';
        }

        var fileExtension = file.type.split('/')[1];

        if (fileName && fileName.indexOf('.') !== -1) {
            var splitted = fileName.split('.');
            fileName = splitted[0];
            fileExtension = splitted[1];
        }

        var fileFullName = (fileName || (Math.round(Math.random() * 9999999999) + 888888888)) + '.' + fileExtension;

        if (typeof navigator.msSaveOrOpenBlob !== 'undefined') {
            return navigator.msSaveOrOpenBlob(file, fileFullName);
        } else if (typeof navigator.msSaveBlob !== 'undefined') {
            return navigator.msSaveBlob(file, fileFullName);
        }

        var hyperlink = document.createElement('a');
        hyperlink.href = URL.createObjectURL(file);
        hyperlink.target = '_blank';
        hyperlink.download = fileFullName;

        if (!!navigator.mozGetUserMedia) {
            hyperlink.onclick = function () {
                (document.body || document.documentElement).removeChild(hyperlink);
            };
            (document.body || document.documentElement).appendChild(hyperlink);
        }

        var evt = new MouseEvent('click', {
            view: window,
            bubbles: true,
            cancelable: true
        });

        hyperlink.dispatchEvent(evt);

        if (!navigator.mozGetUserMedia) {
            URL.revokeObjectURL(hyperlink.href);
        }
    }
}

helper.Functions.Performance = {


    StartTime: null,
    EndTime: null,
    DoShow: true,
    TargetIdForRender: 'performance_value',


    Initialize: function (targetIdForClick) {
        var self = this;
        this.StartTime = new Date();

        if (document.URL.toLowerCase().indexOf('person') <= 0) {
            this.DoShow = false;
        }
        else {
            $('#' + targetIdForClick).css('cursor', 'copy');
        }

        $('header .breadcrumb span:last').append('<span id="' + this.TargetIdForRender + '" style="margin-left: 20px; visibility: hidden"></span>');

        $(document).on('click', '#' + targetIdForClick, function (e) {
            if (self.DoShow) {
                if ($('#performance_value').css('visibility') == 'hidden') {
                    $('#' + self.TargetIdForRender).css('visibility', 'visible');
                    $('#' + targetIdForClick).css('cursor', 'default');
                }
                else {
                    $('#' + self.TargetIdForRender).css('visibility', 'hidden');
                    $('#' + targetIdForClick).css('cursor', 'copy');
                }
            }
        });

        $(document).on('click', '.k-window-action.k-link', function (e) {
            self.StartTime = new Date();
        });
    },

    /*
     * override start time
     */
    SetStarted: function () {
        this.StartTime = new Date();
        this.EndTime = this.StartTime;
        this.RenderTime();
    },

    SetStopped: function () {
        if (this.DoShow) {
            this.EndTime = new Date();
            this.RenderTime();
        }
    },

    RenderTime: function () {

        var timeSpan = this.EndTime.getTime() - this.StartTime.getTime();

        var seconds = timeSpan / 1000;

        $('#' + this.TargetIdForRender).html('>> Loading time: ' + seconds + ' seconds <<');

    }

}