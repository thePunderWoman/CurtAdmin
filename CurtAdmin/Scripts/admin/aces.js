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

    $(document).on('click','a.showConfig', function (e) {
        e.preventDefault();
        $(this).parent().find('ul.configs').slideToggle('fast');
    });
    $('#find').on('click', function () {
        var makeid = $('#make').val();
        var modelid = $('#model').val();
        $('#vehicleData').empty();
        $('#vcdbData').empty();
        $('#loadingVCDB').show();
        $('#loadingCurtDev').show();
        $.getJSON('/ACES/GetVehicles', { makeid: makeid, modelid: modelid }, function (vData) {
            $('#loadingCurtDev').hide();
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
            $('#loadingVCDB').hide();
            console.log(vcdbData);
            if (vcdbData.length > 0) {
                $(vcdbData).each(function (i, obj) {
                    var opt = '<li>' + obj.BaseVehicle.YearID + ' ' + obj.BaseVehicle.Make.MakeName + ' ' + obj.BaseVehicle.Model.ModelName + ' ' + ((obj.Submodel != null) ? obj.Submodel.SubmodelName : "");
                    if (obj.VehicleConfigs.length > 0) {
                        opt += ' (<a href="#" class="showConfig">' + obj.VehicleConfigs.length + ' Config' + ((obj.VehicleConfigs.length > 1) ? 's' : '') + '</a>)';
                        opt += '<ul class="configs">';
                        $(obj.VehicleConfigs).each(function (x, config) {
                            opt += '<li>' + config.BodyStyleConfig.BodyType.BodyTypeName.trim() + ' ' + config.BodyStyleConfig.BodyNumDoor.BodyNumDoors.trim() + '-door';
                            opt +=  ' ' + config.EngineConfig.EngineBase.Liter.trim() + ' liter ' + config.EngineConfig.EngineBase.BlockType.trim() + config.EngineConfig.EngineBase.Cylinders.trim();
                            opt += ' ' + config.DriveType.DriveTypeName.trim() + ' ' + config.EngineConfig.FuelType.FuelTypeName.trim();
                            if (config.BedConfig.BedLength.BedLength1.trim() != 'N/R' && config.BedConfig.BedType.BedTypeName.trim() != 'N/R') {
                                opt += ' ' + config.BedConfig.BedLength.BedLength1.trim() + ' In. ' + config.BedConfig.BedType.BedTypeName.trim() + ' Bed';
                            }
                            opt += ' ' + config.BrakeConfig.BrakeAB.BrakeABSName.trim() + ' ' + ((config.WheelBase.WheelBase1.trim() != '-') ? config.WheelBase.WheelBase1.trim() + ' In. WheelBase' : '');
                            opt += '</li>';
                        });
                        opt += '</ul>'
                    }
                    opt += '</li>';
                    $('#vcdbData').append(opt);
                });
            } else {
                $('#vcdbData').append('<p>No Vehicles</p>');
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