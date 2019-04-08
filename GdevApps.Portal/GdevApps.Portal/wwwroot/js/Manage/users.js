var Users = (function () {
    var $table;
    var $users;
    function init() {
        initDataTables();
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
                    "data": "roles",
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
            function format(d) {
                if (!d)
                    return "<div class='cols-xs-12'>User does not have roles</div>"
                var roles = JSON.parse(d.roles);
                if (!roles || roles.length === 0) {
                    return "<div class='cols-xs-12'>User does not have roles</div>"
                }

                return '<table class="table">' +
                    '<tr>' +
                    '<td class="col-xs-2"><b>Roles:</b></td>' +
                    getSheetInfoCell(roles) + 
                    '</tr>' +
                    '</table>';
            };

            function getSheetInfoCell(roles) {
                var cell = "";
                roles.forEach(role => {
                    cell = cell + '<td class="col-xs-2"><div class="cols-xs-12">' + role.Name + '</div></td>' +
                    '<td class="col-xs-2"><button type="button" class="btn btn-danger" data-role-id="' + role.RoleId + '" data-role="' + role.Name + '" data-user-id="'+role.UserId+'" onclick="Users.removeUser(this)">REMOVE</button></td>'
                });

                cell = cell + "";
                return cell;
            }

            $('#users tbody').off('click').on('click', 'td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = $table.row(tr);

                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                }
                else {
                    // Open this row
                    row.child(format(row.data())).show();
                    tr.addClass('shown');
                }
            });
        }
    }

    function removeUser(me) {
        var url = "/Manage/DeleteUserRole";
        var data = { userRoleId: $(me).data("role-id") , userRole: $(me).data("role"), userId: $(me).data("user-id")  }
        $.ajax({
            method: "POST",
            url: url,
            data: data,
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            }
        })
            .done(function (msg) {
                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_SUCCESS,
                    title: 'Remove Role',
                    message: 'User was successfully from the role!',
                });
            })
            .fail(function (msg) {
                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_DANGER,
                    title: 'Remove Role',
                    message: 'An error occurred while removing this user from the role. Error: '+msg.responseText,
                })
            });
    }
    return {
        init: init,
        removeUser:removeUser
    }
})(jQuery)