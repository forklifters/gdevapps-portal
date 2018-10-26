var ParentStudents = (function () {
    var table;
    function init() {
        intiDdls();
    }


    function intiDdls() {
        $('#ddlStudents').on("change", function (item) {
            var url = "/Parent/GetClasses";
            debugger;
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
                    var $ddlClasses = $('#ddlClasses');
                    $ddlClasses.empty();
                    $.each(response, function (index, item) {
                        if(item && item.text && item.uniqueId){
                            $ddlClasses.append($('<option></option>').text(item.text).val(item.uniqueId));
                        }
                    });
                    var gradeBookId = '';
                    var classId = $('#ddlClasses').val();
                    var hasValue = !!$('#ddlClasses option').filter(function() { return !this.disabled; }).length; 
                    var duration = 'slow';
                    if(hasValue){
                        $('#divGradeBooks').show('slide', { direction: 'left' });
                        gradeBookId = $ddlClasses.val();
                    }else{
                        $('#divGradeBooks').hide('slide', { direction: 'left' });
                    }

                    var $grdStudents = $("#grdStudents");
                    // if (table) {
                    //     table.destroy();
                    //     $grdStudents.removeData('loaded')
                    // }
                    // initDataTable(classId, gradeBookId);
                    // $grdStudents.removeAttr('hidden');
                })
                .fail(function (msg) {
                    alert("Error occurred while retrieving the Gradeboks: " + msg.responseText);
                });

        });
    }


    return {
        init: init
    }
})(jQuery)