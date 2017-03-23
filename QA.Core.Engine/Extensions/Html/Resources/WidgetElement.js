$(function () {
    $('.control-panel').draggable({
        cursor: 'move',
        handle: '.control-panel-controls'
    });

    $('.titleBar .command').click(function (e) {
        e.preventDefault();

        try {
            var itemId = $(this).data('itemid');
            var action = $(this).data('action');
            var subItems = $(this).parents('.zoneItem').first().data('all-children');

            if (!action) {
                alert('Не поддерживается');
                return false;
            }

            switch (action) {
                case "edit":
                    QA.Engine.Editing.editQPArticle(itemId, ContentIdToEdit, CurrentReturnUrl, false);
                    break;
                case "delete":
                    QA.Engine.Editing.deleteQPArticle(itemId, ContentIdToEdit, subItems, CurrentReturnUrl, false);
                    break;

                default:
                    alert('Не поддерживается: ' + action);
                    return false;
            }

            return false;

        } catch (e) {
            alert(e);
        }

        return false;
    });
});