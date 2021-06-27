'use strict';
biometricDeviceAsAccessListController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function biometricDeviceAsAccessListController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window)
{
    $rootScope.title = 'Access Controller List';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.biometricDeviceAsAccessLists = [];
    $scope.path = 'Attendances/accesscontrollerlist/';
    $scope.zonepath = 'Biometric/AttendanceDeviceZone/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Plant', 'Plant');


    $scope.biometricDeviceAsAccessList = {
        Id: null,
        AttendanceDeviceZoneid: null,
        CompanyGroupId: null,
        PlantId: $window.plantId,
        MachineID: null,
        MachineIP: null,
        IsActive: true,
        IsDataAutoDownloadBySched: false,
        IsAdmin: false,
        AdminEnrollID: null,
        AdminPassword: null,
        AdminProxiCard: null,
        Description: null,
        Remarks: null,
        OneFlag: null,
        ZeroFlag: null,
        RegisTypeDec: false,
        RegisTypeHex: false,
        RegisCharacter: 0,
        DownLdEnrollID: false,
        DownLdTypeDec: false,
        DownLdTypeHex: false,
        DownLdTypeScan: false,
        DownLdCharacter: 0,
        IsDataClearAftDW: false,
        IsDeviceBasedInOut: false,
        DeviceInOutFlag: null
    };

    $scope.biometricDeviceAsAccessListNew = Object.assign({}, $scope.biometricDeviceAsAccessList);

    $scope.getData = function (pageno)
    {
        $rootScope.parameters.PlantId = $scope.biometricDeviceAsAccessListNew.PlantId;
        baseService.pagination(pageno)
            .then(function (result)
            {
                $scope.biometricDeviceAsAccessLists = result.Rows;
            }, function ()
                {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function ()
                {
                });
    };
    $scope.getData();

    $scope.ZoneList = [];
    $scope.getZoneData = function ()
    {
        $http({
            method: 'GET',
            url: $scope.zonepath + 'GetAllZone',
            dataType: 'JSON'
        }).then(function successCallback(response)
        {
            $scope.ZoneList = response.data;
        });
    }
    $scope.getZoneData();

    $scope.searchByList = [
        {
            'name': 'Plant',
            'value': 'Plant'
        },
        {
            'name': 'Machine Id',
            'value': 'MachineID'
        },
        {
            'name': 'Machine IP',
            'value': 'MachineIP'
        },
        {
            'name': 'Zone',
            'value': 'Zone'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    cboService.getCboPlantByCompany(null, function (result)
    {
        $scope.PlantList = result;
    });

    $scope.show = function ()
    {
        var x = document.getElementById("deviced");
        var y = document.getElementById("flag1");
        var z = document.getElementById("flag2");
        if (x.style.display === "none")
        {
            x.style.display = "none";
            y.style.display = "block";
            z.style.display = "block";
        }
        else
        {
            x.style.display = "none";
            y.style.display = "block";
            z.style.display = "block";
        }
    };
    $scope.hide = function ()
    {
        var x = document.getElementById("deviced");
        var y = document.getElementById("flag1");
        var z = document.getElementById("flag2");
        if (x.style.display === "none")
        {
            x.style.display = "block";
            y.style.display = "none";
            z.style.display = "none";
        } else
        {
            x.style.display = "block";
            y.style.display = "none";
            z.style.display = "none";
        }
    };


    $scope.Get = function (id, index)
    {
        $scope.index = index;
        $scope.biometricDeviceAsAccessList = $scope.biometricDeviceAsAccessLists[$scope.index];
        $scope.biometricDeviceAsAccessListNew = Object.assign({}, $scope.biometricDeviceAsAccessList);
        if ($scope.biometricDeviceAsAccessListNew.IsDeviceBasedInOut)
        {
            $scope.biometricDeviceAsAccessListNew.DeviceInOutFlag = $scope.biometricDeviceAsAccessListNew.DeviceInOutFlag;
            $scope.hide();
        } else
        {

            $scope.biometricDeviceAsAccessListNew.ZeroFlag = $scope.biometricDeviceAsAccessListNew.ZeroFlag;
            $scope.biometricDeviceAsAccessListNew.OneFlag = $scope.biometricDeviceAsAccessListNew.OneFlag;
            $scope.show();
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed)
        {
            $rootScope.toggle();
        }
    };

    $scope.SetChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsAccessListNew.DownLdEnrollID = event.currentTarget.checked;
    };

    $scope.SetDecimalChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeDec = event.currentTarget.checked;
    };
    $scope.SetHexaDecimalChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeScan = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeHex = event.currentTarget.checked;
    };
    $scope.SetScanChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.DownLdEnrollID = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeDec = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeHex = false;
        $scope.biometricDeviceAsAccessListNew.DownLdTypeScan = event.currentTarget.checked;
    };
    $scope.SetRegDecimalChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.RegisTypeHex = false;
        $scope.biometricDeviceAsAccessListNew.RegisTypeDec = event.currentTarget.checked;
    };
    $scope.SetRegHexDecimalChecked = function (event)
    {
        $scope.biometricDeviceAsAccessListNew.RegisTypeDec = false;
        $scope.biometricDeviceAsAccessListNew.RegisTypeHex = event.currentTarget.checked;
    };

    $scope.Save = function ()
    {
        try
        {
            if ($scope.biometricDeviceAsAccessListNew.IsDeviceBasedInOut === false)
            {
                $scope.biometricDeviceAsAccessListNew.DeviceInOutFlag = null;
                if (baseService.isUndefinedOrNull($scope.biometricDeviceAsAccessListNew.ZeroFlag))
                {
                    throw '0 flag is required.';
                }
                if (baseService.isUndefinedOrNull($scope.biometricDeviceAsAccessListNew.OneFlag))
                {
                    throw '1 flag is required.';
                }
            } else
            {
                $scope.biometricDeviceAsAccessListNew.ZeroFlag = null;
                $scope.biometricDeviceAsAccessListNew.OneFlag = null;
                if (baseService.isUndefinedOrNull($scope.biometricDeviceAsAccessListNew.DeviceInOutFlag))
                {
                    throw 'Deviced Based is required.';
                }
            }
            angular.copy($scope.biometricDeviceAsAccessListNew, $scope.biometricDeviceAsAccessList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.biometricDeviceAsAccessListNewForm.$valid)
            {
                if ($scope.Action === 'Save')
                {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.biometricDeviceAsAccessList,
                        dataType: 'JSON'
                    }).then(function successCallback(response)
                    {
                        if (response.data.Error === true)
                        {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else
                        {
                            ShowResult(response.data.Message, 'success');
                            $scope.biometricDeviceAsAccessLists.push(response.data.AccessControllerList);
                            baseService.paginationAdd();
                            ClearFields();
                            $scope.getData();
                        }
                    }), function errorCallBack(response)
                        {
                            ShowResult(response.data.Message, 'failure');
                        };
                }
                else if ($scope.Action === 'Update')
                {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.biometricDeviceAsAccessList,
                        dataType: 'JSON'
                    }).then(function successCallback(response)
                    {
                        if (response.data.Error === true)
                        {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else
                        {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1)
                            {
                                $scope.biometricDeviceAsAccessLists[$scope.index] = $scope.biometricDeviceAsAccessList;
                            }
                            ClearFields();
                            $scope.getData();
                        }
                    }, function errorCallBack(response)
                        {
                            ShowResult(response.data.Message, 'failure');
                        });
                }
            }
        } catch (e)
        {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function ()
    {
        if (!baseService.isUndefinedOrNull($scope.biometricDeviceAsAccessListNew.Id))
        {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.biometricDeviceAsAccessListNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response)
            {
                if (response.data.Error === true)
                {
                    ShowResult(response.data.Message, 'failure');
                }
                else
                {
                    ShowResult(response.data.Message, 'success');
                    $scope.biometricDeviceAsAccessLists.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response)
                {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else
        {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Clear = function ()
    {
        ClearFields();
        return true;
    };

    function ClearFields()
    {
        $scope.Action = 'Save';
        $scope.PlantId = $scope.biometricDeviceAsAccessListNew.PlantId;
        $scope.biometricDeviceAsAccessList = {};
        $scope.biometricDeviceAsAccessListNew = {};
        $scope.biometricDeviceAsAccessListNew.PlantId = $scope.PlantId;
        $scope.biometricDeviceAsAccessListNew.IsDeviceBasedInOut = false;
        $scope.biometricDeviceAsAccessListNew.IsActive = true;
        if ($scope.biometricDeviceAsAccessListNew.IsDeviceBasedInOut === false)
        {
            $scope.biometricDeviceAsAccessListNew.ZeroFlag = null;
            $scope.biometricDeviceAsAccessListNew.OneFlag = null;
        }
        $scope.show();
    }
}