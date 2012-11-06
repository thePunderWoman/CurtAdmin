$(function () {
    $(document).on('click', '#lines', function (e) {
        e.preventDefault();
        $('#sidebar').slideToggle('fast');
    });
});