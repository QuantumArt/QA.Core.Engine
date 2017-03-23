var Control_Panel_Managment_Url = Control_Panel_Managment_Url || '';
var Control_Panel_Managment_Mode = Control_Panel_Managment_Mode || 'Visible';
var QA = QA || { Widgets: {} };

//#region Widget utilities
QA.Widgets = function (ctx) {
    var _mode = false;

    // удляем у всех ссылок атрибут target
    $('a').removeAttr('target');

    var isEditingMode = function () {
        return _mode || (Control_Panel_Managment_Mode == 'Editing');
    }

    var setIsEditingMode = function (mode) {
        _mode = mode;
    }

    var processCreating = function (args) {
        console.log(args);
        try {
            QA.Overlays.showPageOverlay();
            $.getJSON(Control_Panel_Managment_Url + '/oncreatepart', args,
                function (data) {
                    if (data && data.Result) {
                        if (!data.IsSucceeded) {
                            alert(data.Message);
                            return;
                        }

                        console.log(data.Result.FieldsToDisable);
                        console.log(data.Result.FieldsToSet);
                        QA.Engine.Editing.createPartArticle(data.Result.FieldsToSet, data.Result.FieldsToDisable, ContentIdToEdit, CurrentReturnUrl)

                    } else {
                        alert("В данный момент добавить виджет невозможно.");
                        return;
                    }
                });
            //TODO: request qp8
        } catch (ex) {
            QA.Overlays.hidePageOverlay();
            throw ex;
        }
    }

    var processMoving = function (args) {
        QA.Overlays.showPageOverlay();
        var url = Control_Panel_Managment_Url + '/movenode';
        var jqxhr = $.post(url, {
            TargetNodeId: args.itemId,
            DestinationNodeId: args.path,
            MovementType: "zone",
            ZoneName: args.zone
        })
            .done(function (data) {
                console.log(data);
                if (data && !data.IsSucceeded) {
                    if (data.Message) {
                        alert(data.Message);
                    }
                }
            })
            .fail(function (arg) {
                alert("Ошибка во время обработки запроса.");
            })
            .always(function (arg) {
                QA.Overlays.hidePageOverlay();
                window.location = CurrentReturnUrl;
            });

    }

    return {
        isEditingMode: isEditingMode,
        setIsEditingMode: setIsEditingMode,
        processCreating: processCreating,
        processMoving: processMoving,
    };
}();

//#endregion

//#region Overlays
QA.Overlays = QA.Overlays || (function () {
    var showPageOverlay = function (id) {
        $('#' + (id || 'whole-page-overlay')).show();
    };

    var hidePageOverlay = function (id) {
        $('#' + (id || 'whole-page-overlay')).hide();
    };

    return {
        showPageOverlay: showPageOverlay,
        hidePageOverlay: hidePageOverlay
    };
})();
//#endregion


$(function () {
    if (QA.Widgets.isEditingMode()) {
        $('.control-panel').disableSelection();
    }

    // ContentIdToEdit
    $('.page-edit-command').click(function (e) {
        e.preventDefault();
        try {
            var itemId = $(this).attr('data-itemid');

            if (!itemId) {
                alert('Не поддерживается');
                return false;
            }

            var action = $(this).attr('data-action');
            if (action != 'edit') {
                alert('Не поддерживается');
                return false;
            }

            QA.Engine.Editing.editQPArticle(itemId, ContentIdToEdit, CurrentReturnUrl, true);
        } catch (e) {
            alert(e);
        }
        return false;
    });

    if (window.localStorage) {
        var cookieValue = localStorage.getItem("control_panel_cookie");
        if (cookieValue) {
            var cookie = JSON.parse(cookieValue);
            $('.control-panel .group').each(function (i, elem) {
                if (cookie[i]) {
                    $(this).removeClass('collapsed');
                } else {
                    $(this).addClass('collapsed')
                }
            });
        }
    }
    var checkAccept = function (ui) {
        try {
            var $zone = $(this);
            if ($zone.is('.dropZone') == false) {
                $zone = $zone.parents('.dropZone').first();
            }
            var type = $(ui).attr('data-type');
            var allowedTypes = $zone.attr('data-allowed');
            if (allowedTypes == null || allowedTypes.search(type) != -1) {
                return true;
            }
        }
        catch (ex) {
            return false;
        }
    };

    var ondrop = function (arg, elem) {
        var $zone = $(this);
        if ($zone.is('.dropZone') == false) {
            $zone = $zone.parents('.dropZone').first();
        }
        var path = $zone.attr('data-item');
        var zone = $zone.attr('data-zone');
        var type = elem.draggable.attr('data-type');
        var itemId = elem.draggable.attr('data-itemid');

        if (itemId) {
            elem.draggable.hide();
            QA.Widgets.processMoving({
                path: path, zone: zone, type: type, itemId: itemId
            });
        } else {
            QA.Widgets.processCreating({
                path: path, zone: zone, type: type, itemId: itemId
            });
        }
    };

    var onStop = function () {
        $('.zone-hover').removeClass('zone-hover');
        $('.helper-hover').removeClass('helper-hover');
        $('.helper-allowed').removeClass('helper-allowed');
        $('.zone-allowed').removeClass('zone-allowed');
    }

    $('.definition-draggable').draggable(
    {
        helper: 'clone',
        //function (event) {
        //    var type = $(event.currentTarget).attr('data-type');
        //    var iconUrl = $(event.currentTarget).attr('data-icon-url');
        //    var title = $(event.currentTarget).attr('data-title');
        //    return $("<div class='ui-widget-header'><span class='definition-title' style='background:url(" + iconUrl + ") no-repeat;'>" + (title || type || "na") + "</span>" + "</div>");
        //},
        revert: 'invalid',
        refreshPositions: true,
        scroll: true,
        cursor: 'move',
        start: function () {
            $(this)
                .addClass('definition-item-dragged')
                .css('z-index', 15);
            //.find('.description').addClass('hidden');
        },
        stop: function () {
            //$(this).find('.description').removeClass('hidden');
            $(this).removeClass('definition-item-dragged');
            onStop();
        }
    });
    $('.zoneItem').draggable(
    {
        handle: '.titleBar',
        revert: 'invalid',
        refreshPositions: true,
        scroll: true,
        cursor: 'move',
        start: function () {
            $(this)
                .addClass('zone-item-dragged')
                .css('z-index', 105)
                .find('.widget-element').addClass('hidden');
        },
        stop: function () {
            $(this)
                .removeClass('zone-item-dragged')
                .css('z-index', 10)
                .find('.widget-element').removeClass('hidden');
            onStop();
        }

    });
    $('.zoneItem').droppable({
        greedy: true,
        accept: function (ui) {
            return true;
        }
    });

    //$('.dropZone').droppable({
    //    greedy: true,
    //    tolerance: 'pointer',
    //    accept: checkAccept,
    //    activeClass: 'zone-allowed',
    //    hoverClass: 'zone-hover',
    //});
    $('.dropZone').droppable({
        //$('.zone-item-wrapper').droppable({
        greedy: true,
        tolerance: 'pointer',
        accept: checkAccept,
        activeClass: 'helper-allowed',
        hoverClass: 'helper-hover',
        //out: function (ev, ui) {
        //    //$(this).parents('.dropZone')
        //    //    .first()
        //    //    .removeClass('zone-hover');
        //},
        //over: function (ev, ui) {
        //    //$(this)
        //    //    .parents('.dropZone')
        //    //    .first()
        //    //    .addClass('zone-hover');
        //},
        drop: ondrop
    });

    $('.expander-header').click(function () {
        if (window.localStorage) {
            $(this).parents('.group').first().toggleClass('collapsed');

            var cookie = {};
            $('.control-panel .group').each(function (i, elem) {
                cookie[i] = !$(this).hasClass('collapsed');
            });

            localStorage.setItem("control_panel_cookie", JSON.stringify(cookie));
        }
    });

});
