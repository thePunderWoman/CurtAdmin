$(document).ready(function () {

    $('table').dataTable({
        "bJQueryUI": true
    });

    $('.item_action').live('change', function () {
        var action = $(this).val();
        var item_id = $(this).attr('id').split(':')[1];
        switch (action) {
            case 'delete':
                if (confirm('Are you sure you want to remove this News Item?')) {
                    $.get('/techServices/DeleteNewsItem', { 'id': item_id }, function (data) {
                        deleteItem(data, item_id);
                    });
                }
                break;
            case 'edit':
                window.location.href = "/techServices/EditNews?id=" + item_id;
                break;
            default:

        }
        $(this).val(0);
    });

    $('.isActive').live('click', function () {
        var item_id = $(this).attr('id').split(':')[1];
        set_isActive(item_id);
    });

});


function set_isActive(itemID) {
    $.get('/Customers/SetNewsStatus',{'ID':itemID},function(response){
        if (response != '') {
            showMessage(response);
        }else{
            showMessage("The News item is now active.");
        }
    },"html");
}

function deleteItem(response, item_id) {
    if (response != '') {
        showMessage('There was an error while removing the item.');
    } else {
        $('#item\\:' + item_id).remove();
        showMessage('The News item was successfully removed.');
    }
}



function selectFile(url) {
    $('#file').val(url);
    $('#photo-file img').attr('src', url).attr('alt', 'Photo');
    $("#file-dialog").dialog("close");
    $("#file-dialog").empty();
}