var Teacher = (function () {
    var $table;
    var $users;
    function init() {
        initDataTables();
        initAddButtons();
    }

    function initDataTables() {
        $users = $('#users');
        if (!$users.data('loaded')) {
            $table = $users.DataTable({
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [[10, 25, -1], [10, 25, "All"]],
                "ordering": true,
                "columns": [{
                    "className": 'details-control',
                    "orderable": false,
                    "data": '',
                    "defaultContent": '<span class="glyphicon glyphicon-th-list"></span>'
                },
                {
                    "data": "username",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                {
                    "data": "id",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                {
                    "data": "email",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                {
                    "data": "role",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                {
                    "data": "createdby",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                ],
                "processing": true,
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ classes",
                    "lengthMenu": "Show _MENU_ classes",
                    "emptyTable": "There are no users",
                    "processing": '<div class="loader"><img src="../images/google/google-loader.gif" alt="ASP.NET" class="loader-spin" /></div>'
                },
                "complete": function () {
                    var spinner = $('.loader');
                    if (spinner) {
                        spinner.hide();
                    }
                }
            });

            $users.data('loaded', true);
        }
    }

    function initAddButtons(){
        var placeholderElement = $('#modal-placeholder');
        placeholderElement.on('click', '[data-add="modal"]', function (event) {
            event.preventDefault();
            var form = $(this).parents('.modal').find('form');
            var actionUrl = form.attr('action');
            var dataToSend = form.serialize();
            $.post(actionUrl, dataToSend).done(function (data) {
                var newBody = $('.modal-body', data);
                placeholderElement.find('.modal-body').replaceWith(newBody);
                // find IsValid input field and check it's value
                // if it's valid then hide modal window
                var isValid = newBody.find('[name="IsValid"]').val() == 'True';
                if (isValid) {
                    placeholderElement.find('.modal').modal('hide');
                    //location.reload();
                    BootstrapDialog.show({
                        type: BootstrapDialog.TYPE_SUCCESS,
                        title: 'Add Role',
                        message: 'Teacher was successfully created!',
                        buttons: [{
                            label: 'Close',
                            action: function(dialogItself){
                                dialogItself.close();
                                location.reload();
                            }
                        }]
                    });
                } 
            }).fail(function (msg) {
                placeholderElement.find('.modal').modal('hide');
                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_DANGER,
                    title: ' Role',
                    message: 'An error occurred while creating a teacher. Error: ' + msg.responseText,
                    buttons: [{
                        label: 'Close',
                        action: function(dialogItself){
                            dialogItself.close();
                        }
                    }]
                });
            });;
        });
    }

    function addTeacher(me){
        var placeholderElement = $('#modal-placeholder');
        var url = "/Manage/AddTeacher";
        $.get(url).done(function (result) {
            placeholderElement.html(result);
            placeholderElement.find('.modal').modal('show');
        });
    }
    return {
        init: init,
        addTeacher:addTeacher
    }
})(jQuery)