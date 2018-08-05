var Login = (function () {

    function init() {
        initHandlers();

    }

    function initHandlers() {
        $('#btnLoginGoogle').on('click', function (event) {
            var chbxAgreeWithTerms = $('#chbxAgreeWithTerms');
            var divChbxAgreeWithTerms = $('#divChbxAgreeWithTerms');
            if (!chbxAgreeWithTerms.is(':checked')) {
                event.preventDefault();
                divChbxAgreeWithTerms.addClass("warningDiv")
                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_WARNING,
                    title: 'Log in',
                    message: 'Please, access our terms and conditions!',
                });
            } else {
                divChbxAgreeWithTerms.removeClass("warningDiv")
            }
        })
    }

    return {
        init: init
    }
})(jQuery)