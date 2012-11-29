$(document).ready(function () {

    $('table').dataTable({
        "bJQueryUI": true
    });


    $('.isEnabled').live('click', function () {
        var record_id = $(this).attr('id').split(':')[1];
        set_isEnabled(record_id);
    });

});


/*
* This function is going to make an AJAX call to the controller and set the isEnabled field.
* @param record_id: Primary Key for web property
*/
function set_isEnabled(record_id) {
    $.get('/Customers/SetWebPropertyStatus', { 'record_id': record_id }, function (response) {
        if (response != '') {
            showMessage(response);
        } else {
            showMessage("The Web Property's status has been updated.");
        }
    },"html");
}
