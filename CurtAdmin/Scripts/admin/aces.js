$(function () {
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
        var makeid = $('#make').val();
        var modelid = $('#model').val();
        $('#vehicleData').empty();
        $('#loading').show();
        $.getJSON('/ACES/GetVehicles', { makeid: makeid, modelid: modelid }, function (vData) {
            $('#loading').hide();
            if (vData.length > 0) {
                $(vData).each(function (i, obj) {
                    var configs = "";
                    if (obj.ConfigID != null) {
                        $(obj.VehicleConfig.VehicleConfigAttributes).each(function (x, attrib) {
                            configs += " " + attrib.ConfigAttribute.value;
                        });

                    }
                    var opt = '<li>' + obj.BaseVehicle.YearID + ' ' + obj.BaseVehicle.vcdb_Make.MakeName + ' ' + obj.BaseVehicle.vcdb_Model.ModelName + ' ' + ((obj.Submodel != null) ? obj.Submodel.SubmodelName : "") + configs + '</li>';
                    $('#vehicleData').append(opt);
                });
            } else {
                $('#vehicleData').append('<p>No Vehicles</p>');
            }
        });
        $.getJSON('/ACES/GetVCDBVehicles', { makeid: makeid, modelid: modelid }, function (vcdbData) {

        });

    });

});