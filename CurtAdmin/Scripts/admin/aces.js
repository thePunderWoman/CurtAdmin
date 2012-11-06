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
        $(this).parent().find('table.configs').slideToggle('fast');
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
                $(vData).each(function (y, BaseVehicle) {
                    var opt = '<li>' + BaseVehicle.YearID + ' ' + BaseVehicle.vcdb_Make.MakeName + ' ' + BaseVehicle.vcdb_Model.ModelName + '<ul class="submodels">';
                    $(BaseVehicle.vcdb_Vehicles).each(function (i, obj) {
                        var configs = "";
                        if (obj.ConfigID != null) {
                            $(obj.VehicleConfig.VehicleConfigAttributes).each(function (x, attrib) {
                                configs += " " + attrib.ConfigAttribute.value;
                            });

                        }
                        if (obj.Submodel != null) {
                            opt += '<li>' + obj.Submodel.SubmodelName + configs + '</li>';
                        }
                    });
                    opt += '</ul></li>';
                    $('#vehicleData').append(opt);
                });
            } else {
                $('#vehicleData').append('<p>No Vehicles</p>');
            }
        });
        $.getJSON('/ACES/GetVCDBVehicles', { makeid: makeid, modelid: modelid }, function (vcdbData) {
            $('#loadingVCDB').hide();
            //console.log(vcdbData);
            if (vcdbData.length > 0) {
                $(vcdbData).each(function (i, obj) {
                    var opt = '<li>' + obj.Year + ' ' + obj.Make.MakeName + ' ' + obj.Model.ModelName + '<ul class="submodels">';
                    $(obj.Vehicles).each(function (y, vehicle) {
                        opt += '<li>' + vehicle.Submodel.SubmodelName.trim() + ' (' + vehicle.Region.RegionAbbr + ')'
                        if (vehicle.Configs.length > 0) {
                            opt += ' <a href="#" class="showConfig">' + vehicle.Configs.length + ' Config' + ((vehicle.Configs.length > 1) ? 's' : '') + '</a>';
                            opt += '<table class="configs">';
                            opt += '<thead><tr><th>Body Type</th><th>Doors</th><th>Engine</th><th>Engine Version</th><th>Valves</th><th>Drive Type</th><th>Fuel Type</th><th>Transmission</th><th>Bed Config</th><th>ABS</th><th>Brake System</th><th>Front Brakes</th><th>Rear Brakes</th><th>Wheel Base</th><th>MFR Body Code</th></tr></thead><tbody>';
                            $(vehicle.Configs).each(function (x, config) {
                                opt += '<tr>';
                                opt += '<td>' + config.BodyStyleConfig.BodyType.BodyTypeName.trim() + '</td><td>' + config.BodyStyleConfig.BodyNumDoor.BodyNumDoors.trim() + '-dr</td>';
                                opt += '<td>' + config.EngineConfig.EngineBase.Liter.trim() + 'L ' + config.EngineConfig.EngineBase.BlockType.trim() + config.EngineConfig.EngineBase.Cylinders.trim() + '</td><td>' + config.EngineConfig.EngineVersion.EngineVersion1.trim() + '</td><td>' + config.EngineConfig.Valve.ValvesPerEngine.trim();
                                opt += '<td>' + config.DriveType.DriveTypeName.trim() + '</td><td>' + config.EngineConfig.FuelType.FuelTypeName.trim() + '</td>';
                                opt += '<td>' + config.Transmission.TransmissionBase.TransmissionNumSpeed.TransmissionNumSpeeds.trim() + '-SP ' + config.Transmission.TransmissionBase.TransmissionControlType.TransmissionControlTypeName.trim() + ' ' + config.Transmission.TransmissionBase.TransmissionType.TransmissionTypeName.trim() + '</td>';
                                if (config.BedConfig.BedLength.BedLength1.trim() != 'N/R' && config.BedConfig.BedType.BedTypeName.trim() != 'N/R') {
                                    opt += '<td>' + config.BedConfig.BedLength.BedLength1.trim() + ' In. ' + config.BedConfig.BedType.BedTypeName.trim() + '</td>';
                                } else {
                                    opt += '<td></td>';
                                }
                                opt += '<td>' + config.BrakeConfig.BrakeAB.BrakeABSName.trim() + '</td><td>' + config.BrakeConfig.BrakeSystem.BrakeSystemName.trim() + '</td><td>' + config.BrakeConfig.FrontBrakeType.BrakeTypeName + '</td><td>' + config.BrakeConfig.RearBrakeType.BrakeTypeName + '</td>';
                                if (config.WheelBase.WheelBase1.trim() != '-') {
                                    opt += '<td>' + config.WheelBase.WheelBase1.trim() + ' In.</td>';
                                } else {
                                    opt += '<td></td>';
                                }
                                opt += '<td>' + config.MfrBodyCode.MfrBodyCodeName.trim() + '</td>';
                                opt += '</tr>';
                            });
                            opt += '</tbody></table>'
                        }
                    });
                    opt += '</ul></li>';
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