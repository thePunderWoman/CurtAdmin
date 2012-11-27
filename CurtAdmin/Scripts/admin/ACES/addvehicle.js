$(function () {
    $('#find').hide();

    $(document).on('change', '#make', function () {
        $('#find').hide();
        $('#model').html('<option value="">Select a Model</option>');
        $('#model').attr('disabled', 'disabled');
        var makeid = $(this).val();
        if (makeid != "") {
            $.getJSON('/ACES/GetVCDBModels/' + makeid, function (data) {
                $(data).each(function (i, model) {
                    $('#model').append('<option value="' + model.ModelID + '">' + $.trim(model.ModelName) + '</option>');
                });
                $('#model').removeAttr('disabled', 'disabled');
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
    

    $('#model').on('change', function (e) {
        if ($(this).val() == "") {
            $('#find').hide();
        } else {
            $('#find').show();
        }
    });

    $('#find').on('click', function () {
        var makeid = $('#make').val();
        var modelid = $('#model').val();
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

});

String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g,"");
}
String.prototype.ltrim = function() {
    return this.replace(/^\s+/,"");
}
String.prototype.rtrim = function() {
    return this.replace(/\s+$/,"");
}