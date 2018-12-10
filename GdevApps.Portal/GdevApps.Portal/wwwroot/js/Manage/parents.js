var Parent = (function () {
    var $table;
    var $users;
    function init() {
        initDataTables();
        //initAddButtons();
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
                    "processing": '<div id="loader"><img src="../images/google/google-loader.gif" alt="ASP.NET" class="loader-spin" /></div>'
                },
                "complete": function () {
                    var spinner = $('#loader');
                    if (spinner) {
                        spinner.hide();
                    }
                }
            });

            $users.data('loaded', true);
        }
    }
    return {
        init: init,
    }
})(jQuery)