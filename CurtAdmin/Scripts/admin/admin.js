$(function () {
    $(document).on('click', '#lines', function (e) {
        e.preventDefault();
        $('#sidebar').slideToggle('fast');
    });

    $(document).on('change', '#websiteID', function () {
        var websiteID = $(this).val();
        $.post("/Website/ChooseWebsite/" + websiteID, function (resp) {
            if (resp != "") {
                location.reload(true);
            }
        })
    });
});