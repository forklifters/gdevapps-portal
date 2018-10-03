var Students = (function () {
    var table;
    function init() {
        intiDdls();
        initAddButtons();
    }

    function intiDdls() {
        $('#ddlClasses').on("change", function (item) {
            var url = "/Teacher/GetGradeBooks";
            var id = $(this).val(); // Use $(this) so you don't traverse the DOM again
            var data = { classId: id }

            $.ajax({
                method: "POST",
                url: url,
                data: data,
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                }
            })
                .done(function (response) {
                    debugger;
                    var $ddlGradeBooks = $('#ddlGradeBooks');
                    $ddlGradeBooks.empty();
                    $.each(response, function (index, item) {
                        $ddlGradeBooks.append($('<option></option>').text(item.text).val(item.uniqueId));
                    });
                    var gradeBookId = '';
                    var classId = $('#ddlClasses').val();
                    if($ddlGradeBooks.length > 0 || ($('#divGradeBooks').css("display") != 'none' && $ddlGradeBooks.length === 0)){
                        $('#divGradeBooks').toggle( "slide" );
                        gradeBookId = $ddlGradeBooks.val();
                    }

                    var $grdStudents = $("#grdStudents");
                    if (table) {
                        table.destroy();
                        $grdStudents.removeData('loaded')
                    }
                    initDataTable(classId, gradeBookId);
                    $grdStudents.removeAttr('hidden');
                })
                .fail(function (msg) {
                    alert("Error occurred while retrieving the Gradeboks: " + msg.responseText);
                });

        });
    }

    function getParentsCell(d) {
        var row = '';
        d.parents.forEach(parent => {
            //cell = cell + '<li class="list"><a target="_blank" href="https://mail.google.com/mail/?view=cm&fs=1&to=' + email + '">' + email + '</li>'
            var btn = '<button type="button" class="btn btn-primary" data-email="' + parent.email + '" data-name="' + parent.name + '" data-toggle="ajax-modal" onclick="Students.getTeacherInfo(this)">ADD USER</button>';
            if(parent.hasAccount){
                btn = '<button type="button" class="btn btn-primary" data-email="' + parent.email + '" data-name="' + parent.name + '" data-toggle="ajax-modal" >SHARE GRADEBOOK</button>';
            }
            
            row = row + '<tr><td class="pricing-plans__features ng-scope">'
            + (parent.name ? parent.name : "") +'</td><td> <a target="_blank" class="pricing-plans__feature feature-icon icon--gmail " href="https://mail.google.com/mail/?view=cm&fs=1&to=' 
            + parent.email + '">' + parent.email + '</a></td> <td>'+btn+'</td></tr>'
        });
        return row;
    }

    function formatParent(d) {
        return '<table class="table table-condensed">' +
            '<tr>' +
            '<td class="col-xs-12" colspan="3">Parent emails:</td>' +
            '</tr>' + getParentsCell(d) + '</table>';
    };

    function initDataTable(id, gradeBookId) {
        var $grdStudents = $("#grdStudents");
        if (!$grdStudents.data('loaded')) {
            var data = {
                classId: id,
                gradeBookId:gradeBookId
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
                    "data": "email",
                    "render": function (data, type, full, meta) {
                        return data;
                        //return full.email;
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
                    row.child(formatParent(row.data())).show();
                    tr.addClass('shown');
                }
            });
        }
    }

    function initAddButtons() {
        var placeholderElement = $('#modal-placeholder-students');
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

    function getTeacherInfo(me) {
        var placeholderElement = $('#modal-placeholder-students');
        var url = "/Account/AddTeacher";
        $.get(url).done(function (result) {
            placeholderElement.html(result);
            placeholderElement.find('.modal').modal('show');
        });
    }
    return {
        init: init,
        getTeacherInfo: getTeacherInfo
    }
})(jQuery)