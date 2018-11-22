var Parents  = (function () {
    var $parents;
    function init() {
        initDataTables();
    }
    function initDataTables() {
        $parents = $('#parents');
        if (!$parents.data('loaded')) {
            $parents.DataTable({
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [[10, 25, -1], [10, 25, "All"]],
                "ajax": {
                    "url": "/Teacher/GetAllParents",
                    "method": "POST",
                    "headers": {
                        "RequestVerificationToken": $('input[name="__RequestVerificationToken"').val()
                    }
                },
                "columns": [
                    {
                        "className": 'details-control',
                        "orderable": false,
                        "data": '',
                        "render": function (data, type, full, meta) {
                            //https://stackoverflow.com/questions/35547647/how-to-make-datatable-row-or-cell-clickable
                            return '<button class="btn btn-danger remove-parent" data-parent-email="'+full.email+'" title="Remove parent" onclick="Parents.remove(this)">Remove</button>';
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
                    "data": "studentEmail",
                    "render": function (data, type, full, meta) {
                        return data;
                    }
                },
                {
                    "data": "parentGradebookName",
                    "render": function (data, type, full, meta) {
                        return "<a href="+full.parentGradebookLink+">"+full.parentGradebookName+"</a>";
                    }
                },
                {
                    "data": "mainGradeBookName",
                    "render": function (data, type, full, meta) {
                        return "<a href="+full.mainGradeBookLink+">"+full.mainGradeBookName+"</a>";
                    }
                },
                {
                    "data": "hasAccount",
                    "render": function (data, type, full, meta) {
                        if (data) {
                            return "<span class='green'><i class='material-icons'>check_circle</i></span>"
                        }
                        return "<span class='red' data-toggle='popover' data-trigger='hover' title='Help' data-content='This user must sign in to the Portal to create an account.'><i class='material-icons'>error</i></span>";
                    }
                }
                ],
                "ordering": true,
                "processing": true,
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ parents",
                    "lengthMenu": "Show _MENU_ parents",
                    "emptyTable": "There are no parents where you were assigned by this teacher",
                    "processing":'<div id="loader"><img src="../images/google/google-loader.gif" alt="ASP.NET" class="loader-spin" /></div>'
                },
                'createdRow': function( row, data, dataIndex ) {
                    $(row).attr('data-maingb-id', data.mainGradeBookNameUniqueId).attr('data-parentgb-id', data.parentGradebookUniqueId);
                },
                "initComplete": function(){
                    var spinner = $('#loader');
                    if (spinner) {
                        spinner.hide();
                    };
                    initPopOvers();
                }
            });
            $parents.data('loaded', true);
        }
    }

    function initPopOvers(){
        $('[data-toggle="popover"]').popover({
            container: 'body'
        }); 
    }

    function remove(me) {
        BootstrapDialog.confirm('Are you sure you want to delete this parent? All information about the shared GradeBooks will be lost.', function (result) {
            if (result) {
                var parentEmail = $(me).data("parent-email");
                var url = "/Teacher/RemoveParent";
                var data = {
                    parentEmail: parentEmail
                };
                var $parentsProcessing = $("#parents_processing");
                $.ajax({
                    method: "POST",
                    url: url,
                    data: data,
                    headers: {
                        RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                    }
                }).done(function (response) {
                    if ($parentsProcessing) {
                        $parentsProcessing.css("display", "none");
                    };

                    BootstrapDialog.show({
                        type: BootstrapDialog.TYPE_SUCCESS,
                        title: 'Delete Parent',
                        message: 'Parent was successfully deleted!',
                    })
                })
                    .fail(function (response) {
                        if ($parentsProcessing) {
                            $parentsProcessing.css("display", "none");
                        };

                        BootstrapDialog.show({
                            type: BootstrapDialog.TYPE_DANGER,
                            title: 'Delete Parent',
                            message: 'An error occurred while deleting this parent!',
                        })
                    })
            } else {
                event.preventDefault();
            }
        });
    }

    return {
        init: init,
        remove: remove
    }
})(jQuery)