var getCurtDevVehicles, generateConfigTable, loadNotes;

$(function () {
    $("#tabs").tabs();
    $('#find').hide();

    $('#make').on('change', function (e) {
        $('#find').hide();
        $('#model').html('<option value="">Select a Model</option>');
        $('#model').attr('disabled', 'disabled');
        var idstr = $(this).val();
        $.getJSON('/ACES/GetModels/' + idstr, function (data) {
            $(data).each(function (i, obj) {
                var opt = '<option value="' + obj.ID + '">' + obj.ModelName + '</option>';
                $('#model').append(opt);
            });
            $('#model').removeAttr('disabled', 'disabled');
        });
    });

    $('#model').on('change', function (e) {
        if ($(this).val() == "") {
            $('#find').hide();
        } else {
            $('#find').show();
        }
    });

    $('#find').on('click', function () {
        getCurtDevVehicles();
    });

    $('div.configs').show();

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

    $(document).on('click', 'a.viewNotes', function (e) {
        e.preventDefault();
        var vpid = $(this).data('id');
        loadNotes(vpid);
    });

    $(document).on('click', '.removeBV,.removeSubmodel', function (e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var toolobj = $(this).parent();
        if (confirm('Are you sure you want to remove this vehicle from this part?')) {
            $.post(href, function (data) {
                if (data) {
                    $(toolobj).fadeOut('400', function () {
                        $(toolobj).remove();
                    });
                }
            }, "json");
        }
    });

    $(document).on('click', '.removeConfig', function (e) {
        e.preventDefault();
        var href = $(this).attr('href');
        var trobj = $(this).parent().parent();
        if (confirm('Are you sure you want to remove this vehicle from this part?')) {
            $.post(href, function (data) {
                if (data) {
                    $(trobj).fadeOut('400', function () {
                        $(trobj).remove();
                    });
                }
            }, "json");
        }
    });

});

getCurtDevVehicles = function () {
    var makeid = $('#make').val();
    var modelid = $('#model').val();
    $('#curtDevData').empty();
    $('#loadingCurtDev').show();
    $.getJSON('/ACES/GetVehicles', { makeid: makeid, modelid: modelid }, function (vData) {
        //console.log(vData);
        $('#loadingCurtDev').hide();
        if (vData.length > 0) {
            $(vData).each(function (y, BaseVehicle) {
                var opt = '<li id="bv-' + BaseVehicle.ID + '">' + BaseVehicle.YearID + ' ' + BaseVehicle.Make.MakeName + ' ' + BaseVehicle.Model.ModelName + ((BaseVehicle.AAIABaseVehicleID != "") ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '<span class="tools"><a href="#" class="gear" data-bvid="' + BaseVehicle.ID + '" title="View Parts"></a><a class="remove" href="/ACES/RemoveBaseVehicle/' + BaseVehicle.ID + '" title="Remove Base Vehicle">&times;</a></span><ul class="submodels">';
                $(BaseVehicle.Submodels).each(function (i, submodel) {
                    opt += '<li id="bv' + BaseVehicle.ID + 's' + submodel.SubmodelID + '">' + submodel.submodel.SubmodelName.trim() + ((submodel.vcdb) ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '<span class="tools">';
                    opt += '<a href="/ACES/RemoveSubmodel?BaseVehicleID=' + BaseVehicle.ID + '&SubmodelID=' + submodel.SubmodelID + '" class="removesubmodel" title="Remove Submodel Vehicle">&times;</a>';
                    opt += '<a href="/ACES/AddConfig?BaseVehicleID=' + BaseVehicle.ID + '&SubmodelID=' + submodel.SubmodelID + '" data-bvid="' + BaseVehicle.ID + '" data-submodelID="' + submodel.SubmodelID + '"  class="addconfig" title="Add Configuration">+</a>';
                    opt += ' <a href="#" class="gear" data-bvid="' + BaseVehicle.ID + '" data-submodelID="' + submodel.SubmodelID + '" title="View Parts"></a><a href="#" class="showConfig" title="Show / Hide Configurations">';
                    opt += '<span class="vehicleCount">' + submodel.vehicles.length + '</span><span class="arrow"></span>';
                    opt += '</a>';
                    opt += '</span><span class="clear"></span>';
                    opt += generateConfigTable(submodel);
                });
                opt += '</ul></li>';
                $('#curtDevData').append(opt);
            });
        } else {
            $('#curtDevData').append('<p>No Vehicles</p>');
        }
    });
};

generateConfigTable = function (submodel) {
    var configTable = "";
    configTable += '<div class="configs"><table>';
    configTable += '<thead><tr>';
    configTable += '<th>VCDB</th>'
    $(submodel.configlist).each(function (z, config) {
        configTable += '<th>' + config.name + '</th>';
    });
    configTable += '<th></th>';
    configTable += '</tr></thead><tbody>';
    $(submodel.vehicles).each(function (x, vehicle) {
        configTable += '<tr>';
        configTable += '<td>' + ((vehicle.vcdb) ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '</td>';
        $(submodel.configlist).each(function (z, config) {
            configTable += '<td>';
            $(vehicle.configs).each(function (q, attr) {
                if (attr.ConfigAttributeType.name == config.name) {
                    configTable += attr.value + '<a href="/ACES/RemoveConfigAttribute?vehicleID=' + vehicle.ID + '&attrID=' + attr.ID + '" data-vehicleID="' + vehicle.ID + '" data-attrID="' + attr.ID + '" class="removeattribute">&times;</a>';
                }
            });
            configTable += '</td>';
        });
        configTable += '<td><a href="#" class="change" data-id="' + vehicle.ID + '" title="Add new attributes">Change</a> | <a href="#" class="custom" data-id="' + vehicle.ID + '" title="Custom Configuration">Custom</a> | <a href="#" class="parts" data-vid="' + vehicle.ID + '" title="View Parts">Parts</a> | <a href="/ACES/removeVehicle/' + vehicle.ID + '" data-id="' + vehicle.ID + '" class="removeconfig" title="Remove Configuration">&times;</a></td></tr>'
    });
    configTable += '</tbody></table></div>';
    return configTable;
};

loadNotes = function (vPartID) {
    $("#notes-dialog").empty();
    $.getJSON('/ACES/GetNotes/' + vPartID, function (data) {
        var notemsg = '<ul id="notelist">';
        $(data).each(function (i, note) {
            notemsg += '<li>' + note.note1 + '<a href="/ACES/RemoveNote/' + note.ID + '" class="removeNote">&times;</a></li>';
        });
        notemsg += '</ul>';
        if (data.length == 0) {
            notemsg += '<p id="nonotes">No Notes</p>';
        }
        notemsg += '<label for="addNote">Add Note<br /><input type="text" id="addNote" data-vpid="' + vPartID + '" placeholder="Enter a note" /></label>';
        notemsg += '<button id="submitNote">Add</button>';
        $("#notes-dialog").append(notemsg);
        $('#addNote').autocomplete({
            minLength: 1,
            source: function (request, response) {
                $.getJSON('/ACES/SearchNotes', { keyword: $('#addNote').val() }, function (data) {
                    response($.map(data, function (item) {
                        return {
                            label: item.label,
                            value: item.value,
                            id: item.ID
                        }
                    }));
                })
            },
            open: function () {
                $(this).removeClass("ui-corner-all").addClass("ui-corner-top");
            },
            close: function () {
                $(this).removeClass("ui-corner-top").addClass("ui-corner-all");
            },
            select: function (e, ui) {
                e.preventDefault();
                $('#addNote').val(ui.item.value);
            }
        });
        $("#notes-dialog").dialog({
            modal: true,
            title: "Vehicle Part Notes",
            width: 'auto',
            height: 'auto',
            buttons: {
                "Done": function () {
                    $(this).dialog("close");
                    $('#addPart').focus();
                }
            }
        });
    });
}