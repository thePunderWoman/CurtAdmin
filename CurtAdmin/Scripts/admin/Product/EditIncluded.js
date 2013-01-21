$(function () {
    var includedTable = $('#includedParts').dataTable({ "bJQueryUI": true });
    var allTable = $('#allParts').dataTable({ "bJQueryUI": true });

    $("#loading_area").fadeOut();
    $('#tableContainer').fadeIn();

    $('a.add').live('click', function () {
        var includedID = $(this).attr('id');
        var partID = $('#partID').val();
        var part = $(this).attr('title').substr(4, $(this).attr('title').length);
        if (partID > 0 && includedID > 0 && confirm('Are you sure you want to make ' + part + ' an included part?')) {
            // execute AJAX
            $.getJSON('/Product/AddIncluded', { 'partID': partID, 'includedID': includedID }, function (response) {
                if (response.error == null) {
                    // Add row to table
                    includedTable.fnAddData([
                            response.partID,
                            response.shortDesc,
                            response.dateModified,
                            response.listPrice,
                            '<a href="javascript:void(0)" title="Remove ' + response.shortDesc + '" class="remove center" id="' + response.partID + '">Remove</a>'
                        ]);
                    allTable.fnDeleteRow($('#all\\:' + response.partID).get()[0]);
                    showMessage(response.shortDesc + ' added.');
                } else {
                    showMessage(response.error);
                }
            });
        }
    });

    $('.remove').live('click', function () {
        var includedID = $(this).attr('id');
        var partID = $('#partID').val();
        var part = $(this).attr('title').substr(7, $(this).attr('title').length);
        if (partID > 0 && relatedID > 0 && confirm('Are you sure you want to remove the relationship to ' + part + '?')) {
            $.getJSON('/Product/DeleteIncluded', { 'partID': partID, 'includedID': includedID }, function (response) {
                if (response.error == null) {
                    // Add row to table
                    allTable.fnAddData([
                                response.partID,
                                response.shortDesc,
                                response.dateModified,
                                response.listPrice,
                                '<a href="javascript:void(0)" title="Add ' + response.shortDesc + '" class="add center" id="' + response.partID + '">Add</a>'
                            ]);
                    includedTable.fnDeleteRow($('#included\\:' + response.partID).get()[0]);
                    showMessage(response.shortDesc + ' removed.');
                } else {
                    showMessage(response.error);
                }
            });
        }
    });
});