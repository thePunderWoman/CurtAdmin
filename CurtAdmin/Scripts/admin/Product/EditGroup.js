var groupTable, showForm, clearForm;
showForm = (function (groupID, name) {
    $('#groupID').val(groupID);
    $('#name').val(name);
    $('.form_left').slideDown();
});

clearForm = (function () {
    $('#groupID').val(0);
    $('#name').val('');
    $('.form_left').slideUp();
});

$(function () {
    groupTable = $('table').dataTable({ "bJQueryUI": true });

    $(document).on('click', '#addGroup', function (e) {
        e.preventDefault();
        showForm(0, '');
    });

    $(document).on('click','.edit', function (e) {
        e.preventDefault();
        var groupID = $(this).data('id');
        var clicked_link = $(this);
        $.getJSON('/Product/GetGroup', { 'groupID': groupID }, function (response) {
            if (response.error == null) {
                groupTable.fnDeleteRow($(clicked_link).parent().parent().get()[0]);
                showForm(response.id, response.name);
            } else {
                showMessage(response.error);
            }
        });
    });

    $(document).on('click', '.remove', function (e) {
        e.preventDefault();
        clearForm();
        var clicked_link = $(this);
        var groupID = $(this).data('id');
        if (groupID > 0 && confirm('Are you sure you want to delete this group?')) {
            $.get('/Product/DeleteGroup', { 'groupID': groupID }, function (response) {
                if (response == "") {
                    groupTable.fnDeleteRow($(clicked_link).parent().parent().get()[0]);
                    showMessage("Group removed.");

                } else {
                    showMessage(response);
                }
            });
        } else if (contentID <= 0) {
            showMessage("Group ID not valid.");
        }
    });

    $(document).on('click','#btnReset', function () {
        var groupID = $('#groupID').val();
        if (groupID > 0) {
            $.getJSON('/Product/GetGroup', { 'groupID': groupID }, function (response) {
                var addId = groupTable.fnAddData([
                                response.name,
                                response.Parts.length,
                                '<a href="#" class="edit" data-id="' + response.id + '" title="Edit Group">Edit</a> | <a href="#" class="parts" data-id="' + response.id + '" title="Edit Parts">Edit Parts</a> | <a href="#" class="remove" data-id="' + response.id + '" title="Remove Group">Remove</a>'
                ]);
                var theCell = groupTable.fnSettings().aoData[addId[0]].nTr.cells[2];
                theCell.className = "center"
            })
        }
        clearForm();
    });

    $(document).on('click','#btnSave', function () {
        var name = $('#name').val().trim();
        var partID = $('#partID').val();
        if (partID > 0 && name.length > 0) {
            var groupID = $('#groupID').val();
            $.getJSON("/Product/SaveGroup", { 'partID': partID, 'name': name, 'groupID': groupID }, function (response) {
                if (response.error == null) {
                    var addId = groupTable.fnAddData([
                                    response.name,
                                    response.Parts.length,
                                    '<a href="#" class="edit" data-id="' + response.id + '" title="Edit Group">Edit</a> | <a href="#" class="parts" data-id="' + response.id + '" title="Edit Parts">Edit Parts</a> | <a href="#" class="remove" data-id="' + response.id + '" title="Remove Group">Remove</a>'
                    ]);
                    var theCell = groupTable.fnSettings().aoData[addId[0]].nTr.cells[2];
                    theCell.className = "center"
                    showMessage("Group Saved.");
                    clearForm();
                } else {
                    showMessage(response.error);
                }
            });
        } else {
            if (partID <= 0) {
                showMessage("Error getting part number.");
            } else if (name.length == 0) {
                showMessage("Name cannot be blank.");
            } else {
                showMessage("Error encountered.");
            }
        }
        return false;
    });
});