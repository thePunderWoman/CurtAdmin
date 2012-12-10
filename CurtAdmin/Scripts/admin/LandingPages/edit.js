var selectFile, imageSort;

$(function () {
    $("#tabs").tabs();

    $('#startDate,#endDate').datetimepicker({
        ampm: true
    });

    $('#addImage').on('click', function (e) {
        e.preventDefault();
        chooseFile();
    });

    imageSort();

    $(document).on('click', 'a.removeimage', function (e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var liobj = $(this).parent();
        $.getJSON(href, function (data) {
            $(liobj).fadeOut(400, function () {
                $(this).remove();
            });
        });
    });

    CKEDITOR.replace('page_content', {
        filebrowserImageUploadUrl: '/File/CKUpload',
        filebrowserImageBrowseUrl: '/File/CKIndex',
        filebrowserImageWindowWidth: '640',
        filebrowserImageWindowHeight: '480'
    });

});

selectFile = function(url) {
    var pageID = $('#pageID').val();
    $.getJSON('/LandingPages/AddImage', { pageID: pageID, image: url }, function (data) {
        $('#pageImages').empty();
        var lis = "";
        $(data).each(function (i, obj) {
            lis += '<li id="img_' + obj.id + '"><img src="' + obj.url + '" alt="page image" /><a href="/LandingPages/RemoveImage/' + obj.id + '" class="removeimage">&times;</a><span class="clear"></span></li>';
        });
        $('#pageImages').append(lis);
    });
    $("#file-dialog").dialog("close");
    $("#file-dialog").empty();
}

imageSort = function () {
    $("#pageImages").sortable("destroy")
    $("#pageImages").sortable({
        handle: "img",
        axis: "y",
        cursor: "move",
        update: function (event, ui) {
            var pageID = $('#pageID').val();
            var sortstr = $("#pageImages").sortable("serialize", { key: "img" });
            $.post('/LandingPages/UpdateSort?' + sortstr);
        }
    });
    $("#pageImages").disableSelection();
}