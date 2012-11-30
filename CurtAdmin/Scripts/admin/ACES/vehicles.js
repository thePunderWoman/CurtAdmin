var getVCDBVehicles, getCurtDevVehicles;
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

    $(document).on('click', 'a.add', function (e) {
        e.preventDefault();
        var aobj = $(this);
        var href = $(aobj).attr('href');
        $.getJSON(href, function (data) {
            if (data.ID > 0) {
                $(aobj).hide();
                $(aobj).after('<span class="added">Added</span>');
                getCurtDevVehicles();
            }
        })
    });

    $(document).on('click', 'a.remove', function (e) {
        e.preventDefault();
        var aobj = $(this);
        var href = $(aobj).attr('href');
        if (confirm('Are you sure you want to remove this vehicle? It will remove all submodels, configurations and part associations as well.')) {
            $.getJSON(href, function (data) {
                if (data.success) {
                    $(aobj).parent().parent().remove();
                    getVCDBVehicles();
                } else {
                    showMessage("There was a problem removing the vehicle.")
                }
            })
        }
    });

    $(document).on('click', 'a.removesubmodel', function (e) {
        e.preventDefault();
        var aobj = $(this);
        var href = $(aobj).attr('href');
        if (confirm('Are you sure you want to remove this vehicle submodel? It will remove all of the submodel\'s configurations and part associations as well.')) {
            $.getJSON(href, function (data) {
                if (data.success) {
                    $(aobj).parent().parent().remove();
                    getVCDBVehicles();
                } else {
                    showMessage("There was a problem removing the submodel.")
                }
            })
        }
    });

    $(document).on('click', 'a.showConfig', function (e) {
        e.preventDefault();
        if ($(this).parent().parent().find('div.configs').css('display') == 'none') {
            $(this).find('span').css({ WebkitTransform: 'rotate(90deg)' });
            $(this).find('span').css({ '-moz-transform': 'rotate(90deg)' });
        } else {
            $(this).find('span').css({ WebkitTransform: 'rotate(0deg)' });
            $(this).find('span').css({ '-moz-transform': 'rotate(0deg)' });
        }
        $(this).parent().parent().find('div.configs').slideToggle();
    });

    $('#find').on('click', function () {
        getCurtDevVehicles();
        getVCDBVehicles();
    });

});

getCurtDevVehicles = function () {
    var makeid = $('#make').val();
    var modelid = $('#model').val();
    $('#vehicleData').empty();
    $('#loadingCurtDev').show();
    $.getJSON('/ACES/GetVehicles', { makeid: makeid, modelid: modelid }, function (vData) {
        console.log(vData);
        $('#loadingCurtDev').hide();
        if (vData.length > 0) {
            $(vData).each(function (y, BaseVehicle) {
                var opt = '<li>' + BaseVehicle.YearID + ' ' + BaseVehicle.Make.MakeName + ' ' + BaseVehicle.Model.ModelName + ((BaseVehicle.AAIABaseVehicleID != "") ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '<span class="tools"><a class="remove" href="/ACES/RemoveBaseVehicle/' + BaseVehicle.ID + '" title="Remove Base Vehicle">&times;</a></span><ul class="submodels">';
                $(BaseVehicle.Submodels).each(function (i, submodel) {
                    opt += '<li>' + submodel.submodel.SubmodelName.trim() + ((submodel.vcdb) ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '<span class="tools">';
                    opt += '<a href="/ACES/RemoveSubmodel?BaseVehicleID=' + BaseVehicle.ID + '&SubmodelID=' + submodel.SubmodelID + '" class="removesubmodel" title="Remove Submodel Vehicle">&times;</a>';
                    opt += '<a href="/ACES/AddConfig?BaseVehicleID=' + BaseVehicle.ID + '&SubmodelID=' + submodel.SubmodelID + '" class="addconfig" title="Add Configuration">+</a>';
                    if (submodel.vehicles.length > 0 && submodel.configlist.length > 0) {
                        opt += ' <a href="#" class="showConfig" title="Show / Hide Configurations">' + submodel.vehicles.length + '<span class="arrow"></span></a>';
                    }
                    opt += '</span><span class="clear"></span>';
                    if (submodel.vehicles.length > 0 && submodel.configlist.length > 0) {
                        opt += '<div class="configs"><table>';
                        opt += '<thead><tr>';
                        opt += '<th>VCDB</th>'
                        $(submodel.configlist).each(function (z, config) {
                            opt += '<th>' + config.name + '</th>';
                        });
                        opt += '<th></th>';
                        opt += '</tr></thead><tbody>';
                        $(submodel.vehicles).each(function (x, vehicle) {
                            opt += '<tr>';
                            opt += '<td>' + ((vehicle.vcdb) ? '<span class="vcdb">&#10004</span>' : '<span class="notvcdb">&times</span>') + '</td>';
                            $(submodel.configlist).each(function (z, config) {
                                opt += '<td>';
                                $(vehicle.configs).each(function (q, attr) {
                                    if (attr.ConfigAttributeType.name == config.name) {
                                        opt += attr.value;
                                    }
                                });
                                opt += '</td>';
                            });
                            opt += '<td><a href="#" class="alter" data-id="' + vehicle.ID + '" title="Change Configuration">Change</a> | <a href="/ACES/RemoveConfig?vehicleID=' + vehicle.ID + ' class="removeconfig" title="Remove Configuration">&times;</a></td></tr>'
                        });
                        opt += '</tbody></table></div>'
                    }
                });
                opt += '</ul></li>';
                $('#vehicleData').append(opt);
            });
        } else {
            $('#vehicleData').append('<p>No Vehicles</p>');
        }
    });
};

getVCDBVehicles = function () {
    var makeid = $('#make').val();
    var modelid = $('#model').val();
    $('#vcdbData').empty();
    $('#loadingVCDB').show();
    $.getJSON('/ACES/GetVCDBVehicles', { makeid: makeid, modelid: modelid }, function (vcdbData) {
        $('#loadingVCDB').hide();
        if (vcdbData.length > 0) {
            $(vcdbData).each(function (i, obj) {
                var opt = '<li>' + obj.Year + ' ' + obj.Make.MakeName + ' ' + obj.Model.ModelName;
                if (!obj.exists) {
                    opt += '<span class="tools"><a href="/ACES/AddBaseVehicle/' + obj.BaseVehicleID + '" data-id="' + obj.BaseVehicleID + '" class="add" title="Add Base Vehicle">+</a></span>';
                }
                opt += '<ul class="submodels">';
                $(obj.Vehicles).each(function (y, vehicle) {
                    opt += '<li>' + vehicle.Submodel.SubmodelName.trim() + ' (' + vehicle.Region.RegionAbbr + ')'
                    opt += '<span class="tools">';
                    if (!vehicle.exists) {
                        opt += '<a href="/ACES/AddSubmodel?basevehicleid=' + obj.BaseVehicleID + '&submodelid=' + vehicle.Submodel.SubmodelID + '" class="add" title="Add Submodel">+</a>';
                    }
                    if (vehicle.Configs.length > 0) {
                        opt += ' <a href="#" class="showConfig" title="Show / Hide Configurations">' + vehicle.Configs.length + '<span class="arrow"></span></a>';
                    }
                    opt += '</span><span class="clear"></span>';
                    if (vehicle.Configs.length > 0) {
                        opt += '<div class="configs"><table>';
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
                        opt += '</tbody></table></div>'
                    }
                });
                opt += '</ul></li>';
                $('#vcdbData').append(opt);
            });
        } else {
            $('#vcdbData').append('<p>No Vehicles</p>');
        }
    });
};

String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g,"");
}
String.prototype.ltrim = function() {
    return this.replace(/^\s+/,"");
}
String.prototype.rtrim = function() {
    return this.replace(/\s+$/,"");
}