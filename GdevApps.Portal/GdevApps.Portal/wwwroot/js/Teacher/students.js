var Students = (function () {
    var table;

    function init() {
        intiDdls();
    }

    function intiDdls(){
        $('#ddlClasses').on("change", function(item) {
            var $grdStudents = $("#grdStudents");
            if (table) {
                table.destroy();
                $grdStudents.removeData('loaded')
            }
            initDataTable(this.value);
            $grdStudents.removeAttr('hidden');
        });
    }

    function getParentsCell(d) {

        var cell = "<div class='col-xs-4'><ul class='pricing-plans__features ng-scope'>";
        d.prentEmails.forEach(email => {
            //cell = cell + '<li class="list"><a target="_blank" href="https://mail.google.com/mail/?view=cm&fs=1&to=' + email + '">' + email + '</li>'
            cell = cell + '<li class="pricing-plans__feature feature-icon icon--gmail "><a target="_blank" href="https://mail.google.com/mail/?view=cm&fs=1&to=' + email + '">' + email + '</a></li>'
        });
        cell = cell + "</ul></div>";
        return cell;
    }

    function format(d) {
        return '<table class="table">' +
            '<tr>' +
            '<td class="col-xs-1">Parent emails:</td>' +
            '<td class="col-xs-3">' + getParentsCell(d) + '</td>' +
            '</tr>' +
            '</table>';
    };

    function initDataTable(id) {
        debugger;
        var $grdStudents = $("#grdStudents");
        if (!$grdStudents.data('loaded')) {
            var data = {
                classId: id
            };
            table = $grdStudents.DataTable({
                "processing": false,
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [
                    [3, 10, 25, -1],
                    [3, 10, 25, "All"]
                ],
                "ajax": {
                    "url": "/Teacher/GetStudents",
                    "method": "POST",
                    "data": data,
                    "headers": {
                        "RequestVerificationToken": $('input[name="__RequestVerificationToken"').val()
                    }
                },
                "columns": [{
                    "className": 'details-control',
                    "orderable": false,
                    "data": '',
                    "defaultContent": '<span class="glyphicon glyphicon-th-list"></span>'
                }, {
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
                },
                {
                    "data": "isInClassroom",
                    "render": function (data, type, full, meta) {
                        if (data) {
                            return "<span class='green'><i class='material-icons'>check_circle</i></span>"
                        }
                        return "<span class='red'><i class='material-icons'>error</i></span>";
                    }
                }
                ],
                "ordering": true,
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ students",
                    "lengthMenu": "Show _MENU_ students",
                    "emptyTable": "There are no students in this class",
                    "search": "<i class='fa fa-search'></i>",
                    "searchPlaceholder": "Search"
                }
            });
            $grdStudents.data('loaded', true);

            $('#grdStudents tbody').on('click', 'td.details-control', function() {
                var tr = $(this).closest('tr');
                var row = table.row(tr);
    
                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                } else {
                    // Open this row
                    row.child(format(row.data())).show();
                    tr.addClass('shown');
                }
            });
        }
    }
 
    return {
        init: init
    }
})(jQuery)