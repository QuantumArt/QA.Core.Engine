if (jQuery.type(QA) == "undefined") {
    var QA = {};
}
if (jQuery.type(QP) == "undefined") {
    var QP = {};
}


QA.Engine = QA.Engine || {};

QA.Engine.Notification = QA.Engine.Notification || (function () {
    return {
        articleEdited: function (entityId, parentId) { console.log("editied" + "id: " + entityId + " pid: " + parentId); }
    };
})();


QA.Engine.Editing = QA.Engine.Editing || {};

QA.Engine.Editing = function () {

    var showQPForm = function () { alert("Данный функционал не поддерживается."); };

    if (QP.Utils) {

        showQPForm = function (id, contentId, callback, isInTab, isPage, isCreate, fieldsToSet, fieldsToBlock) {
            var uid = QP.Utils.newUID();
            var actionCode = "edit_article";
            var entityTypeCode = "article";
            var observer = QP.Utils.initNew("qaCorecontentEditorObserver", function (eventType, args) {
                console.log(eventType);
                console.log(args);
                if (args.actionUID != uid) {
                    console.log('not matched: ' + uid);
                    return;
                }

                if (QP.Enums.EventType.CloseHost.Id == eventType) {
                    if (callback) { callback(); }
                }

                if (QP.Enums.EventType.ActionHost.Id == eventType) {
                    if (args.actionCode == 'update_article' || args.actionCode == 'update_article_and_up') {
                        if (callback) { callback(); }
                    }
                }

            });

            if (isCreate) { actionCode = "new_article"; }

            var options = QP.Utils.initOptions(actionCode, entityTypeCode, id, contentId, uid,
                observer);

            if (fieldsToSet) {
                QP.Utils.setFieldValues(options, fieldsToSet);
            } else {
                if (isCreate) {
                    alert('Не установлены значения полей.');
                    return;
                }
            }

            if (fieldsToBlock) {
                QP.Utils.setDisabledFields(options, fieldsToBlock);
            }
            else {
                if (isPage) {
                    // TODO: убрать магические строки
                    QP.Utils.setDisabledFields(options, [
                        "field_1145",
                        "field_1147",
                        "field_1149",
                        "field_1150",
                        "field_1156",
                        "field_1159",
                        "field_1158"]);
                }
                else {
                    // TODO: убрать магические строки
                    QP.Utils.setDisabledFields(options, [
                        "field_1145",
                        "field_1147",
                        "field_1149",
                        "field_1156",
                        "field_1159",
                        "field_1158"]);
                }
            }
            var hostId = QP.Utils.hostId();
            console.log('hostId: ' + hostId);
            if (!isInTab) {
                QP.Utils.executeWindow(options, hostId);
            } else {
                QP.Utils.executeTab(options, hostId);
            }
        };

    }


    var editQPArticle = function (id, contentId, returnUrl, isPage) {
        showQPForm(id, contentId, function () {
            try {
                QA.Engine.Notification.articleEdited(id, contentId);
            } catch (ex) { }
            window.location = returnUrl;
        }, false, isPage)
    };


    var showQPArticle = function (id, contentId, returnUrl, isPage) {
        showQPForm(id, contentId, function () {
            try {
                QA.Engine.Notification.articleEdited(id, contentId);
            } catch (ex) { }
            window.location = returnUrl;
        }, false, isPage)
    };

    var createPartArticle = function (fieldsToSet, fieldsToBlock, contentId, returnUrl) {
        showQPForm(0, contentId, function () {
            try {
                QA.Engine.Notification.articleEdited(0, contentId);
            } catch (exc) { }
            window.location = returnUrl;
        }, false, false, true, fieldsToSet, fieldsToBlock);
    };

    var deleteQPArticle = function (id, contentId, subItemsString, returnUrl) {
        var ids = [];
        if (subItemsString) {
            ids = subItemsString.split(',');
        }

        var uid = QP.Utils.newUID();
        var actionCode = QpActionCodes.archive_article;
        var entityTypeCode = "article";

        if (ids.length > 0) {
            actionCode = QpActionCodes.multiple_archive_article;
            alert("Необходимо сначала удалить все дочерние элементы.");
            return;
        }

        var observer = QP.Utils.initNew("qaCorecontentEditorObserver", function (eventType, args) {
            console.log(eventType);
            console.log(args);
            if (args.actionUID != uid) {
                console.log('not matched: ' + uid);
                return;
            }

            if (QP.Enums.EventType.ActionHost.Id == eventType) {
                if (args.actionCode == actionCode) {
                    try {
                        QA.Engine.Notification.articleEdited(id, contentId);
                    } catch (ex) { }
                    if (returnUrl)
                        window.location = returnUrl;
                }
            }
        });

        var options = QP.Utils.initOptions(actionCode, entityTypeCode, id, contentId, uid,
                observer);

        if (ids.length > 0) {
            options.selectedEntityIDs = ids;
        }

        var hostId = QP.Utils.hostId();
        console.log('hostId: ' + hostId);

        QP.Utils.executeAction(options, hostId);

    };

    return {
        editQPArticle: editQPArticle,
        showQPArticle: showQPArticle,
        deleteQPArticle: deleteQPArticle,
        createPartArticle: createPartArticle
    };
}();
