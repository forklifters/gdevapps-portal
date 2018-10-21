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
                    var $ddlGradeBooks = $('#ddlGradeBooks');
                    $ddlGradeBooks.empty();
                    $.each(response, function (index, item) {
                        if(item && item.text && item.uniqueId){
                            $ddlGradeBooks.append($('<option></option>').text(item.text).val(item.uniqueId));
                        }
                    });
                    var gradeBookId = '';
                    var classId = $('#ddlClasses').val();
                    var hasValue = !!$('#ddlGradeBooks option').filter(function() { return !this.disabled; }).length; 
                    var duration = 'slow';
                    if(hasValue){
                        $('#divGradeBooks').show('slide', { direction: 'left' });
                        gradeBookId = $ddlGradeBooks.val();
                    }else{
                        $('#divGradeBooks').hide('slide', { direction: 'left' });
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
        var studentEmail = d.email;
        var classId = d.classId;
        var $divGradeBooks = $('#divGradeBooks');
        if ($divGradeBooks.css("display") != 'none') {
            d.parents.forEach(parent => {
                //cell = cell + '<li class="list"><a target="_blank" href="https://mail.google.com/mail/?view=cm&fs=1&to=' + email + '">' + email + '</li>'
                var btn = '<button type="button" class="btn btn-primary" data-email="' + parent.email + '" data-name="' + parent.name + '" data-toggle="ajax-modal" onclick="Students.getTeacherInfo(this)">ADD USER</button>';
                if (parent.hasAccount) {
                    btn = '<div class="col-xs-12 col-md-6"><button type="button" class="btn btn-primary" data-email="' + parent.email + '" data-name="' + parent.name + '" data-student-email="' + studentEmail + '" data-class-id="' + classId + '" data-toggle="ajax-modal" onclick="Students.share(this)" >SHARE GRADEBOOK</button></div>';
                    btn += '<div class="col-xs-12 col-md-6"><button type="button" class="btn btn-primary" data-email="' + parent.email + '" data-name="' + parent.name + '" data-student-email="' + studentEmail + '" data-class-id="' + classId + '" data-toggle="ajax-modal" onclick="Students.unshare(this)" >UNSHARE GRADEBOOK</button></div>';
                }

                row = row + '<tr><td class="pricing-plans__features ng-scope">'
                    + (parent.name ? parent.name : "") + '</td><td> <a target="_blank" class="pricing-plans__feature feature-icon icon--gmail " href="https://mail.google.com/mail/?view=cm&fs=1&to='
                    + parent.email + '">' + parent.email + '</a></td> <td class="col-xs-5">' + btn + '</td></tr>'
            });
            return row ? row : "<tr><td>No parents were found for this student</td></tr>";
        }else{
            return "<tr><td>GradeBook was not found to retrieve parents</td></tr>"
        }
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
                "dom": "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
                    "<'row'<'col-sm-12'tr>>" +
                    "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
                "lengthMenu": [
                    [10, 25, -1],
                    [10, 25, "All"]
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
                "processing": true,
                "language": {
                    "info": "Showing _START_ to _END_ of _TOTAL_ students",
                    "lengthMenu": "Show _MENU_ students",
                    "emptyTable": "There are no students in this class",
                    "search": "<i class='fa fa-search'></i>",
                    "searchPlaceholder": "Search",
                    "processing":'<div id="loader"><img src="../images/google/google-loader.gif" alt="ASP.NET" class="loader-spin" /></div>'
                },
                "complete": function(){
                    var spinner = $('#loader');
                    if(spinner){
                        spinner.hide();
                    }
                }
            });
            $grdStudents.data('loaded', true);

            $('#grdStudents tbody').off('click').on('click', 'td.details-control', function() {
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
    };

    function unshareGradeBook(me){
        var $ddlGradeBooks = $('#ddlGradeBooks');
        if ($ddlGradeBooks.css("display") != 'none') {
            var mainGradeBookId = $ddlGradeBooks.val();
            var url = "/Teacher/UnshareGradeBook";
            var parentEmail = $(me).data("email");
            var data = {
                parentEmail: parentEmail,
                mainGradeBookId: mainGradeBookId
            };

            var $grdStudents_processing = $("#grdStudents_processing");
            if($grdStudents_processing){
                $grdStudents_processing.css("display","block");
            };

            $.ajax({
                method: "POST",
                url: url,
                data: data,
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                }
            }).done(function(response){
                if($grdStudents_processing){
                    $grdStudents_processing.css("display","none");
                };

                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_SUCCESS,
                    title: 'Unshare GradeBook',
                    message: 'The GradeBook was successfully unshared!',
                })
            })
            .fail(function(response){
                if($grdStudents_processing){
                    $grdStudents_processing.css("display","none");
                };

                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_DANGER,
                    title: 'Unshare GradeBook',
                    message: 'An error occurred while unsharing this GradeBook!',
                })
            })
        }

    }

    function shareGradeBook(me) {
        var $ddlGradeBooks = $('#ddlGradeBooks');
        if ($ddlGradeBooks.css("display") != 'none') {
            var mainGradeBookId = $ddlGradeBooks.val();
            var url = "/Teacher/ShareGradeBook";
            var classId = $(me).data("class-id");
            var studentEmail = $(me).data("student-email");
            var parentEmail = $(me).data("email");
            var parentName = $(me).data("name");
            var className = $('#ddlClasses option:selected').text();
            var data = {
                className: className,
                parentEmail: parentEmail,
                studentEmail: studentEmail,
                mainGradeBookId: mainGradeBookId
            };

           var $grdStudents_processing = $("#grdStudents_processing");
            if($grdStudents_processing){
                $grdStudents_processing.css("display","block");
            };
            $.ajax({
                method: "POST",
                url: url,
                data: data,
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                }
            }).done(function(response){
                if($grdStudents_processing){
                    $grdStudents_processing.css("display","none");
                };

                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_SUCCESS,
                    title: 'Share GradeBook',
                    message: 'The GradeBook was successfully shared!',
                })
            })
            .fail(function(response){
                if($grdStudents_processing){
                    $grdStudents_processing.css("display","none");
                };

                BootstrapDialog.show({
                    type: BootstrapDialog.TYPE_DANGER,
                    title: 'Share GradeBook',
                    message: 'An error occurred while sharing this GradeBook!',
                })
            })
        }
    };

    return {
        init: init,
        getTeacherInfo: getTeacherInfo,
        share: shareGradeBook,
        unshare: unshareGradeBook
    }
})(jQuery)