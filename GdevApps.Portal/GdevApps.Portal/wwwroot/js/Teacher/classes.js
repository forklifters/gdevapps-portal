var Classes = (function () {

    function init() {
        initDataTables();
        initAddButtons();
    }

    function initAddButtons(){
        var placeholderElement = $('#modal-placeholder');
        $('.btnClick').click(function(event) {
            debugger;
            var url = $(this).data('url');
            $.get(url).done(function(data) {
                placeholderElement.html(data);
                placeholderElement.find('.modal').modal('show');
            });
        });
    }

    function getInfo(id){
        debugger;
        var placeholderElement = $('#modal-placeholder');
        //$('button[data-toggle="ajax-modal"]').click(function(event) {
            var url = "/Teacher/GetClassSheetInfo";
            debugger;
            var data = {id: id}
            $.get(url, data).done(function(result) {
                placeholderElement.html(result);
                placeholderElement.find('.modal').modal('show');
            });
       // });
    }

    function initDataTables() {
        var $classes = $('#classes');
        if (!$classes.data('loaded')) {
           var table = $classes.DataTable({
                "processing": false,
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                "<'row'<'col-sm-12'tr>>" +
                "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [ [5, 10, 25, -1], [5, 10, 25, "All"] ],
                "ajax": {
                    "url": "/Teacher/GetClasses",
                    "method": "POST",
                    "headers": {
                        "RequestVerificationToken": $('input[name="__RequestVerificationToken"').val()
                    }
                },
                "columns": [{
                    "className":      'details-control',
                    "orderable":      false,
                    "data":           '',
                    "defaultContent": '<span class="glyphicon glyphicon-th-list"></span>'
                },
                {
                        "data": "name",
                        "render": function(data, type, full, meta) {
                            //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                            return data;
                        }
                    },
                    {
                        "data": "id",
                        "render": function(data, type, full, meta) {
                            return data;
                        }
                    }
                ],
                "columnDefs": [ {
                    "targets": 2,
                    "data": null,
                    "defaultContent": "<button>Click!</button>"
                } ],
                "ordering": true,
                "language":{
                    "info": "Showing _START_ to _END_ of _TOTAL_ classes",
                    "lengthMenu":     "Show _MENU_ classes",
                    "emptyTable":     "There are no classes where you were assigned as a teacher"
                }
            });
            $classes.data('loaded', true);
            $('.dataTable').on('click', 'tbody tr td .details-control', function() {
                console.log('API row values : ', table.row(this).data());
              });

              function format ( d ) {
                return '<table class="table">'+
                    '<tr>'+
                        '<td>Description:</td>'+
                        '<td>'+(d.description ? d.description : "No description")+'</td>'+
                    '</tr>'+
                    '<tr>'+
                        '<td>Number of students:</td>'+
                        '<td>'+(d.studentsCount ? d.studentsCount : "No students")+'</td>'+
                    '</tr>'+
                    '<tr>'+
                        '<td>Number of assignments:</td>'+
                        '<td>'+(d.courseWorksCount ? d.courseWorksCount : "No assignments")+'</td>'+
                    '</tr>'+
                    '<tr>'+
                        '<td>Gradebook:</td>'+
                        '<td><button type="button" class="btn btn-primary btnClick" data-toggle="ajax-modal" onclick="Classes.getInfo('+d.id+')" data-url="@Url.Action("GetClassSheetInfo","Teacher")">Add Gradebook</button></td>'+
                    '</tr>'+
                '</table>';
            };

            $('#classes tbody').on('click', 'td.details-control', function () {
                var tr = $(this).closest('tr');
                var row = table.row( tr );
         
                if ( row.child.isShown() ) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('shown');
                }
                else {
                    // Open this row
                    row.child( format(row.data()) ).show();
                    tr.addClass('shown');
                }
            } );
        }
    }

    return {
        init: init,
        getInfo: getInfo
    }
})(jQuery)