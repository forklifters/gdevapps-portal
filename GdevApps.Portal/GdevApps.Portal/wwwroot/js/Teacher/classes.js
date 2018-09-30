var Classes = (function () {
    var table;
    var $classes;
    function init() {
        initDataTables();
        initAddButtons();
    }

    function initDataTables() {
        $classes = $('#classes');
        if (!$classes.data('loaded')) {
            table = $classes.DataTable({
                "processing": false,
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [[5, 10, 25, -1], [5, 10, 25, "All"]],
                "ajax": {
                    "url": "/Teacher/GetClasses",
                    "method": "GET",
                    "headers": {
                        "RequestVerificationToken": $('input[name="__RequestVerificationToken"').val()
                    }
                },
                "columns": [{
                    "className": 'details-control',
                    "orderable": false,
                    "data": '',
                    "defaultContent": '<span class="glyphicon glyphicon-th-list"></span>'
                },
                {
                    "data": "name",
                    "render": function (data, type, full, meta) {
                        //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                        return data;
                    }
                },
                {
                    "data": "id",
                    "render": function (data, type, full, meta) {
                        return data;
                    }
                }
                ],
                "columnDefs": [{
                    "targets": 2,
                    "data": null,
                    "defaultContent": "<button>Click!</button>"
                }],
                "ordering": true,
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ classes",
                    "lengthMenu": "Show _MENU_ classes",
                    "emptyTable": "There are no classes where you were assigned as a teacher"
                }
            });
            $classes.data('loaded', true);

            function format(d) {
                return '<table class="table">' +
                    '<tr>' +
                    '<td class="col-xs-2">Description:</td>' +
                    '<td colspan="2" class="col-xs-6">' + (d.description ? d.description : "No description") + '</td>' +
                    '</tr>' +
                    '<tr>' +
                    '<td class="col-xs-2">Number of students:</td>' +
                    '<td colspan="2" class="col-xs-6">' + (d.studentsCount ? d.studentsCount : "No students") + '</td>' +
                    '</tr>' +
                    '<tr>' +
                    '<td class="col-xs-2">Number of assignments:</td>' +
                    '<td colspan="2" class="col-xs-6">' + (d.courseWorksCount ? d.courseWorksCount : "No assignments") + '</td>' +
                    '</tr>' +
                    '<tr>' +
                    '<td class="col-xs-2">Gradebooks:</td>' +
                    '<td class="col-xs-2">' + getSheetInfoCell(d) + '</td>' +
                    '<td class="col-xs-4"><button type="button" class="btn btn-primary" data-toggle="ajax-modal" data-id="' + d.id + '" onclick="Classes.getInfo(this)">ADD GRADEBOOK</button></td>' +
                    '</tr>' +
                    '</table>';
            };

            function getSheetInfoCell(d) {
                var cell = "<div class='cols-xs-12'>";
                d.classroomSheets.forEach(sheet => {
                    cell = cell + '<div class="col-xs-8 list-group"><center><a href=' + sheet.link + ' class="list-group-item list-group-item-action editGradebook" data-id="' + sheet.id + '" data-classroomid="' + sheet.classroomId + '" onclick="Classes.linkClick(this)">' +
                        '<div class="d-flex w-100 justify-content-between">' +
                        ' <h4 class="mb-1">' + sheet.name + '</h4>' +
                        '</div>' +
                        '<p class="mb-1 wordwrap">' + sheet.GoogleUniqueId + '</p></a></center></div>' +
                        '<div class="col-xs-2 list-group"><button type="button" class="btn btn-danger" data-id="' + sheet.id + '" data-classroomid="' + sheet.classroomId + '" onclick="Classes.removeGradebook(this)">REMOVE</button>' +
                        '</div>'
                });
                cell = cell + "</div>";
                return cell;
            }

            $('#classes tbody').on('click', 'td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = table.row(tr);

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

    function initAddButtons() {
        var placeholderElement = $('#modal-placeholder');
        $('button[data-toggle="ajax-modal"]').click(function (event) {
            var url = $(this).data('url');
            debugger;
            $.get(url).done(function (data) {
                placeholderElement.html(data);
                placeholderElement.find('.modal').modal('show');
            });
        });

        placeholderElement.on('click', '[data-save="modal"]', function (event) {
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
                    location.reload();
                }
            });
        });
    }

    function getInfo(me) {
        var placeholderElement = $('#modal-placeholder');
        var url = "/Teacher/AddGradebook";
        var data = { classroomId: $(me).data("id") };
        $.get(url, data).done(function (result) {
            placeholderElement.html(result);
            placeholderElement.find('.modal').modal('show');
        });
    }

    function removeGradebook(me) {
        var url = "/Teacher/RemoveGradebook";
        var data = { classroomId: $(me).data("classroomid") , gradebookId: $(me).data("id")  }

        $.ajax({
            method: "POST",
            url: url,
            data: data,
            headers: {
                RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
            }
        })
            .done(function (msg) {
                alert("Gradebook was successfully deleted");
                location.reload();
            })
            .fail(function (msg) {
                alert("Error occurred while deleting the Gradebok: " + msg.responseText);
            });
    }

    function linkClick(me) {
        event.preventDefault();
        var placeholderElement = $('#modal-placeholder');
        var url = "/Teacher/GetGradebookById";
        var data = { classroomId: $(me).data("classroomid") , gradebookId: $(me).data("id")  }
        $.get(url, data).done(function (result) {
            placeholderElement.html(result);
            placeholderElement.find('.modal').modal('show');
        });
    }

    return {
        init: init,
        getInfo: getInfo,
        removeGradebook: removeGradebook,
        linkClick: linkClick
    }
})(jQuery)