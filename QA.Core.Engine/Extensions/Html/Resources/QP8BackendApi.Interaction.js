if (jQuery.type(Quantumart) == "undefined") {
    var Quantumart = {
        QP8: {}
    };
}
if (jQuery.type(Quantumart.QP8) == "undefined") {
    Quantumart.QP8 = {};
}
if (jQuery.type(Quantumart.QP8.Interaction) == "undefined") {
    Quantumart.QP8.Interaction = (function () {

        //#region class BackendExternalMessage (сообщения для передачи в Backend)
        var BackendExternalMessage = function () {

        };
        BackendExternalMessage.prototype = {
            // Тип
            type: "",
            // UID хоста 
            hostUID: null,
            // параметры
            data: null
        };
        BackendExternalMessage.Types = {
            ExecuteAction: 1,
            CloseBackendHost: 2,
            OpenSelectWindow: 3
        };
        //#endregion

        //#region class ExecuteActionOtions (Парамеры сообщения на выполнение BackendAction)
        var ExecuteActionOtions = function () { };
        ExecuteActionOtions.prototype = {
            actionCode: "",
            entityTypeCode: "",
            parentEntityId: 0,
            entityId: 0,

            actionUID: null,
            callerCallback: "",
            changeCurrentTab: false,
            isWindow: false,

            options: null
        };
        //#endregion

        //#region class ArticleFormState (Параметры для инициализации формы статьи)
        var ArticleFormState = function () {
        };
        ArticleFormState.prototype = {
            initFieldValues: null, // значения для инициализации полей (массив ArticleFormState.InitFieldValue)
            disabledFields: null, // идентификаторы полей который должны быть disable (массив имен полей)
            hideFields: null, // идентификаторы полей которые должны быть скрыты (массив имен полей)

            disabledActionCodes: null, // массив Action Code для которых кнопки на тулбаре будут скрыты
            additionalParams: null // дополнительные параметры для выполнения Custom Action
        };
        // #region class ArticleFormState.InitFieldValue (значение поля)
        ArticleFormState.InitFieldValue = function () {
        };
        ArticleFormState.InitFieldValue.prototype = {
            fieldName: "", //имя поля
            value: null // значение (зависит от типа)
        };
        //#endregion
        //#endregion

        //#region class OpenSelectWindowOtions (Парамеры сообщения на открытие окна выбора из списка)
        var OpenSelectWindowOptions = function () { };
        ExecuteActionOtions.prototype = {
            selectActionCode: "",
            entityTypeCode: "",
            parentEntityId: 0,
            isMultiple: false,
            selectedEntityIDs: null,

            selectWindowUID: null, //ID для идентификации окна со списком
            callerCallback: ""
        };
        //#endregion

        //#region class BackendEventObserver (Observer сообщений от хоста)
        var BackendEventObserver = function (callbackProcName, callback) {
            this.callbackProcName = callbackProcName;
            this.callback = callback;

            pmrpc.register({
                publicProcedureName: this.callbackProcName,
                procedure: this.callback,
                isAsynchronous: true
            });
        };
        BackendEventObserver.prototype = {
            callbackProcName: "",
            callback: null,

            dispose: function () {
                pmrpc.unregister(this.callback);
            }
        };
        BackendEventObserver.EventType = {
            HostUnbinded: 1,
            ActionExecuted: 2,
            EntitiesSelected: 3,
            SelectWindowClosed: 4
        };
        BackendEventObserver.HostUnbindingReason = {
            Closed: "closed",
            Changed: "changed"
        }
        //#endregion

        return {
            // Observer сообщений от хоста
            BackendEventObserver: BackendEventObserver,
            // Парамеры сообщения на выполнение BackendAction
            ExecuteActionOtions: ExecuteActionOtions,
            // Параметры для инициализации формы статьи
            ArticleFormState: ArticleFormState,
            // Параметры открытия окна выбора из списка
            OpenSelectWindowOptions: OpenSelectWindowOptions,

            // Выполнить BackendAction
            executeBackendAction: function (executeOtions, hostUID, destination) {
                var message = new BackendExternalMessage();
                message.type = BackendExternalMessage.Types.ExecuteAction;
                message.hostUID = hostUID;
                message.data = executeOtions;
                pmrpc.call({
                    destination: destination,
                    publicProcedureName: message.hostUID,
                    params: [message]
                });
            },

            // Закрыть Backend хост
            closeBackendHost: function (actionUID, hostUID, destination) {
                var message = new BackendExternalMessage();
                message.type = BackendExternalMessage.Types.CloseBackendHost;
                message.hostUID = hostUID;
                message.data = { "actionUID": actionUID };
                pmrpc.call({
                    destination: destination,
                    publicProcedureName: message.hostUID,
                    params: [message]
                });
            },

            openSelectWindow: function (openSelectWindowOptions, hostUID, destination) {
                var message = new BackendExternalMessage();
                message.type = BackendExternalMessage.Types.OpenSelectWindow;
                message.hostUID = hostUID;
                message.data = openSelectWindowOptions;
                pmrpc.call({
                    destination: destination,
                    publicProcedureName: message.hostUID,
                    params: [message]
                });
            }
        };
    })();
};