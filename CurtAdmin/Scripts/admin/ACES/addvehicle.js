var saveNewYear;

$(function () {
    $('#find').hide();
    $("#tabs").tabs();
    $('.addImg:first').fadeIn();

    $(document).on('change', '#vcdbmake', function () {
        $('#find').hide();
        $('#vcdbmodel').html('<option value="">Select a Model</option>');
        $('#vcdbmodel').attr('disabled', 'disabled');
        var makeid = $(this).val();
        if (makeid != "") {
            $.getJSON('/ACES/GetVCDBModels/' + makeid, function (data) {
                $(data).each(function (i, model) {
                    $('#vcdbmodel').append('<option value="' + model.ModelID + '">' + $.trim(model.ModelName) + '</option>');
                });
                $('#vcdbmodel').removeAttr('disabled', 'disabled');
            })
        }
    });

    $(document).on('click', 'a.add', function (e) {
        e.preventDefault();
        var aobj = $(this);
        var href = $(aobj).attr('href');
        $.getJSON(href, function (data) {
            if (data.ID > 0) {
                $(aobj).hide();
                $(aobj).parent().append('<span class="added">Added</span>');
                $(aobj).parent().addClass('added');
            }
        })
    });
    

    $('#vcdbmodel').on('change', function (e) {
        if ($(this).val() == "") {
            $('#find').hide();
        } else {
            $('#find').show();
        }
    });

    $('#find').on('click', function () {
        var makeid = $('#vcdbmake').val();
        var modelid = $('#vcdbmodel').val();
        $('#vehicleData').empty();
        $('#loading').show();
        $.getJSON('/ACES/GetBaseVehicles', { makeid: makeid, modelid: modelid }, function (data) {
            $('#loading').hide();
            if (data.length > 0) {
                $(data).each(function (i, obj) {
                    var opt = '<li>' + obj.YearID + ' ' + obj.Make.MakeName + ' ' + obj.Model.ModelName + '<a href="/ACES/AddBaseVehicle/' + obj.BaseVehicleID + '" data-id="' + obj.BaseVehicleID + '" class="add">Add</a></li>';
                    $('#vehicleData').append(opt);
                });
            } else {
                $('#vehicleData').append('<p>No Unused Base Vehicles</p>');
            }
        });

    });

    $('#nonyear').change(function () {
        var yearID = $(this).val();
        $('.delImg').fadeOut();
        $('.editImg').fadeOut();
        //$('#editYear').fadeIn();
        var targetid = 'addMake';
        if (yearID > 0) {
            $.getJSON('/ACES/GetMakesByYear', { 'yearID': yearID }, function (makes) {
                if (makes.length == 0) {
                    $('#delYear').show();
                }
                $('#nonmake').html('<option value="">- Select Make -</option>');
                $('#nonmodel').html('<option value="">- Select Model -</option>');
                $('#nonsubmodel').html('<option value="">- Select Submodel -</option>');
                $.each(makes, function (i, make) {
                    var new_option = '<option value="' + make.ID + '|' + make.AAIAID + '">' + make.name.trim() + '</option>';
                    $('#nonmake').append(new_option);
                });
            });
            //$('.addImg').fadeOut();
        } else {
            $('#nonmake').html('<option value="0">- Select Make -</option>');
            $('#nonmodel').html('<option value="0">- Select Model -</option>');
            $('#nonsubmodel').html('<option value="0">- Select Submodel -</option>');
            targetid = 'addYear';
        }
        $('.addImg').each(function (i, obj) {
            if ($(this).attr('id') == targetid) {
                $(this).fadeIn();
            } else {
                $(this).fadeOut();
            }
        });
    });

    // Get the models for this year and remove all other content for select boxes.
    $('#nonmake').change(function () {
        var yearID = $('#nonyear').val();
        var makeID = $(this).val();
        var acesID = Number(makeID.split('|')[1]);
        $('#delModel').fadeOut();
        $('#delSubmodel').fadeOut();
        if (acesID == 0) {
            $('#editMake').fadeIn();
        }
        $('#editModel').fadeOut();
        $('#editSubmodel').fadeOut();
        var targetid = 'addModel';
        if (makeID != "") {
            $.getJSON('/ACES/GetModelsByMake', { 'yearID': yearID, 'makeID': makeID }, function (models) {
                if (models.length == 0) {
                    $('#delModel').show();
                }
                $('#nonmodel').html('<option value="">- Select Model -</option>');
                $('#nonsubmodel').html('<option value="">- Select Submodel -</option>');
                $.each(models, function (i, model) {
                    var new_option = '<option value="' + model.ID + '|' + model.AAIAID + '">' + model.name.trim() + '</option>';
                    $('#nonmodel').append(new_option);
                });
            });
        } else {
            $('#nonmodel').html('<option value="0">- Select Model -</option>');
            $('#nonsubmodel').html('<option value="0">- Select Submodel -</option>');
            targetid = 'addMake';
        }
        $('.addImg').each(function (i, obj) {
            if ($(this).attr('id') == targetid) {
                $(this).fadeIn();
            } else {
                $(this).fadeOut();
            }
        });
    });

    // Get the styles for this year and remove all other content for select boxes.
    $('#nonmodel').change(function () {
        var yearID = $('#nonyear').val();
        var makeID = $('#nonmake').val();
        var modelID = $(this).val();
        var acesID = Number(modelID.split('|')[1]);
        var targetid = 'addSubmodel';
        $('#delSubmodel').fadeOut();
        if (acesID == 0) {
            $('#editModel').fadeIn();
        }
        $('#editStyle').fadeOut();
        if (modelID != "") {
            $.getJSON('/ACES/GetSubmodelsByModel', { 'yearID': yearID, 'makeID': makeID, 'modelID': modelID }, function (submodels) {
                $('#nonsubmodel').html('<option value="">- Select Submodel -</option>');
                $.each(submodels, function (i, submodel) {
                    var new_option = '<option value="' + submodel.ID + '|' + submodel.AAIAID + '">' + submodel.name.trim() + '</option>';
                    $('#nonsubmodel').append(new_option);
                });
            });
        } else {
            $('#nonsubmodel').html('<option value="">- Select Submodel -</option>');
            targetid = 'addModel';
        }
        $('.addImg').each(function (i, obj) {
            if ($(this).attr('id') == targetid) {
                $(this).fadeIn();
            } else {
                $(this).fadeOut();
            }
        });
    });

    $('#nonsubmodel').change(function () {
        var submodelID = $(this).val();
        var acesID = Number(submodelID.split('|')[1]);
        if (acesID == 0) {
            $('#editSubmodel').fadeIn();
            $('#delSubmodel').fadeIn();
        }
    });

    $('#addYear').click(function () {
        var html = '<input type="text" name="newYear" id="newYear" class="prompt_text" placeholder="Enter new year..." /><br />';
        $.prompt(html, {
            submit: saveNewYear,
            buttons: { Save: true }
        });
    });

    $('#delYear').live('click', function (e) {
        e.preventDefault();
        var yearID = $('#nonyear').val();

        if (yearID > 0 && confirm("Are you sure you want to remove this year?")) {
            $.getJSON('/ACES/RemoveYear', { 'year': yearID }, function (data) {
                if (data.success) {
                    loadYears();
                } else {
                    showMessage("There was a problem removing the year.")
                }
            });
        } else {
            if (yearID == 0) { // 
                showMessage('Invalid year.');
            }
        }
    });

});

saveNewYear = function (action, f, d, m) {
    var year = m.newYear;
    if (!isNaN(year) && year.length > 0 && year > 0) {
        $.getJSON('/ACES/AddYear', { 'year': year }, function (response) {
            loadYears();
        });
    } else {
        showMessage('Invalid year.');
    }
}

loadYears = function () {
    $.getJSON('/ACES/GetYears', function (years) {
        $('#nonyear').empty();
        $('#nonyear').append('<option value="">- Select Year -</option>');
        $(years).each(function (i, year) {
            var opt = '<option value="' + year + '">' + year + '</option>'
            $('#nonyear').append(opt);
        });
        $('#nonyear').trigger('change');
    });
}


/*String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g,"");
}
String.prototype.ltrim = function() {
    return this.replace(/^\s+/,"");
}
String.prototype.rtrim = function() {
    return this.replace(/\s+$/,"");
}*/