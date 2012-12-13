$(function () {
    $(document).on('click', 'a.showConfig', function (e) {
        e.preventDefault();
        if ($(this).parent().parent().find('div.configs').css('display') == 'none') {
            $(this).find('span.arrow').css({ WebkitTransform: 'rotate(90deg)' });
            $(this).find('span.arrow').css({ '-moz-transform': 'rotate(90deg)' });
        } else {
            $(this).find('span.arrow').css({ WebkitTransform: 'rotate(0deg)' });
            $(this).find('span.arrow').css({ '-moz-transform': 'rotate(0deg)' });
        }
        $(this).parent().parent().find('div.configs').slideToggle();
    });
});