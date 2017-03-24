if (jQuery.type(QA) == "undefined") {
    var QA = {};
}
if (jQuery.type(QP) == "undefined") {
    var QP = {};
}


QA.Engine.Editing = QA.Engine.Editing || {};
QA.Engine.ContentEditorWrapper = QA.Engine.ContentEditorWrapper || {};

QA.Engine.ContentEditorWrapper = function () {

    var showQPForm = function () { alert("Данный функционал не поддерживается."); };

    if (QP.Utils) {
        showQPForm = function (id, contentId, callback, isInTab) {
            var uid = QP.Utils.newUID();
            var actionCode = "edit_article";
            var entityTypeCode = "article";
            var observer = QP.Utils.initNew("contentEditorObserver", function (eventType, args) {
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

            var options = QP.Utils.initOptions(actionCode, entityTypeCode, id, contentId, uid,
                observer);

            var hostId = QP.Utils.hostId();
            console.log('hostId: ' + hostId);
            if (!isInTab) {
                QP.Utils.executeWindow(options, hostId);
            } else {
                QP.Utils.executeTab(options, hostId);
            }
        };
    }


    var editQPArticle = function (id, contentId, returnUrl) {
        console.log('edit');
        console.log(id);
        console.log(contentId);
        console.log(returnUrl);
        showQPForm(id, contentId, function () {
            if (returnUrl || returnUrl != null && returnUrl != "") {
                window.location = returnUrl;
            } else {
                window.location.reload();
            }
        })
    };


    var showQPArticle = function (id, contentId, returnUrl) {
        console.log('show');
        console.log(id);
        console.log(contentId);
        console.log(returnUrl);
        showQPForm(id, contentId, function () {
            if (returnUrl || returnUrl != null && returnUrl != "") {
                window.location = returnUrl;
            } else {
                window.location.reload();
            }
        })
    };

    var deleteQPArticle = function (id, contentId, returnUrl) {
        console.log('delete');
        console.log(id);
        console.log(contentId);
        console.log(returnUrl);
        showQPForm(id, contentId, function () {
            if (returnUrl || returnUrl != null && returnUrl != "") {
                window.location = returnUrl;
            } else {
                window.location.reload();
            }
        })
    };

    return {
        editQPArticle: editQPArticle,
        showQPArticle: showQPArticle,
        deleteQPArticle: deleteQPArticle
    };
}();
