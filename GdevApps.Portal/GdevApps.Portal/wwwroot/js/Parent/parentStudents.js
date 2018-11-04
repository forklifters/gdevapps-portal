var ParentStudents = (function () {
    var table;
    function init() {
        intiDdls();
    }


    function intiDdls() {
        $('#ddlStudents').on("change", function (item) {
            //var url = "/Parent/GetClasses";
            var url = "/Teacher/GetClasses"; //Test
            var id = $(this).val(); // Use $(this) so you don't traverse the DOM again
            var data = { classId: id }
            var $loader = $('#loader');
            $loader.removeClass("hidden");
            $.ajax({
                method: "POST",
                url: url,
                data: data,
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                }
            })
                .done(function (response) {
                    $loader.addClass("hidden");
                    var $ddlClasses = $('#ddlClasses');
                    $ddlClasses.empty();
                    $.each(response, function (index, item) {
                        if (item && item.id && item.name) {// TODO: Change this is test
                            $ddlClasses.append($('<option></option>').text(item.id).val(item.name));
                        }
                    });

                    var gradeBookId = '';
                    var classId = $('#ddlClasses').val();
                    var hasValue = !!$('#ddlClasses option').filter(function () { return !this.disabled; }).length;
                    var duration = 'slow';
                    if (hasValue) {
                        $('#divClasses').show('slide', { direction: 'left' });
                        gradeBookId = $ddlClasses.val();
                    } else {
                        $('#divClasses').hide('slide', { direction: 'left' });
                    }
                })
                .fail(function (msg) {
                    $loader.addClass("hidden");
                    console.log("Error occurred while retrieving the Gradeboks: " + msg.responseText);
                });

        });

        $("#ddlClasses").on("change", function () {
            var url = "/Teacher/GetReport"; //Test
            var id = $(this).val(); // Use $(this) so you don't traverse the DOM again
            var data = { gradebookId: id };
            var $loader = $('#loader');
            $loader.removeClass("hidden");
            $.ajax({
                method: "POST",
                url: url,
                data: data,
                headers: {
                    RequestVerificationToken: $('input[name="__RequestVerificationToken"').val()
                }
            }).done(function (result) {
                $loader.addClass("hidden");
                $('#dvReportResults').html(result)
            }).fail(function (err) {
                $loader.addClass("hidden");
                console.log(err);
            })
        })
    }


    return {
        init: init
    }
})(jQuery)