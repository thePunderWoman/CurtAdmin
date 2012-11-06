$(function () {
    $(document).on('click', 'a.showConfig', function (e) {
        e.preventDefault();
        $(this).parent().find('table.configs').slideToggle('fast');
    });
});