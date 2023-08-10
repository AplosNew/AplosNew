'use strict';
VehicleMovementMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleMovementMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    //$rootScope.title = "Vehicle Movement Master";
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';

    
    
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    // #region Vehicle Movement
    $scope.ActionVM = 'Save';
    
    $scope.VehicleMovementList = [];
    $scope.getListUrlVM = $scope.path + 'GetlistVehicleMaster';
    $scope.saveUrlVM = $scope.path + 'SaveVehicleMovement';
    $scope.deleteUrlVM = $scope.path + 'deleteVehicleMaster/';
    /*baseService.init($scope.getListUrlVM);*/

    $scope.VehicleMovementTemp = {
        Id: null,
        FromLocationId: null,
        ToLocationId: null,
        MinKillometer: null,
        Maxkillometer: null,
        CostPerKillometer: null,
        VehicleName: null,
        VehicleName: null,
        VehicleNumber: null,
        Milage: null,
        FuelType: null,
        Remarks: null,
        Remarks: null,

    };
    $scope.VehicleMovement = Object.assign({}, $scope.VehicleMovementTemp);


    $scope.GetVehicleMovement = function (args) {
        $scope.VehicleMovement = Object.assign({}, args.data);
        $scope.ActionVM = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetVehicleMovementData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleMovementData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMovementList = response.data;

        });
    }
    $scope.GetVehicleMovementData();

    $scope.SaveVehicleMovement = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleMovementForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlVM,
                data: {
                    'data': $scope.VehicleMovement,

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVM();
                    $scope.GetVehicleMovementData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVM = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVM = function () {
        ClearFieldsVM();
        return true;
    };

    function ClearFieldsVM() {
        $scope.ActionVM = 'Save';

        $scope.VehicleMovement = {
            Id: null,
            FromLocationId: null,
            ToLocationId: null,
            MinKillometer: null,
            Maxkillometer: null,
            CostPerKillometer: null,
            VehicleName: null,
            VehicleName: null,
            VehicleNumber: null,
            Milage: null,
            FuelType: null,
            Remarks: null,
            Remarks: null,
        };
        $scope.VehicleMovement = Object.assign({}, $scope.VehicleMovementTemp);
    }
    // #endregion Vehicle Movement

    // #region Vehicle Master
    $scope.ActionVM = 'Save';
    $scope.VehicleMasterList = [];
    $scope.getListUrlVehicleMaster = $scope.path + 'GetlistVehicleMaster';
    $scope.saveUrlVehicleMaster = $scope.path + 'CreateVehicleMaster';
    $scope.deleteUrlVehicleMaster = $scope.path + 'deleteVehicleMaster/';

    $scope.VehicleMasterTemp = {
        VehicleName: null,
        VehicleNumber: null,
        FuelType: null,
        Milage: null,
        AvgFuelCostPerKL: null,
        AvgMaintenancePerKL: null,
        Remarks:null
    }
    $scope.VehicleMaster = Object.assign({}, $scope.VehicleMovementTemp);

    $scope.GetVehicleMaster = function (args) {
        $scope.VehicleMaster = Object.assign({}, args.data);
        if (!$rootScope.isCollapsed) {
            $scope.ActionVM = 'Update';
            $rootScope.toggle();
        }
    }

    $scope.GetVehicleMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleMasterData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMasterList = response.data;

        });
    }
    $scope.GetVehicleMasterData();

    $scope.SaveVehicleMaster = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlVehicleMaster,
                data: {
                    'data': $scope.VehicleMaster,

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleMaster();
                    $scope.GetVehicleMasterData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVehicleMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.VehicleMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlVehicleMaster + $scope.VehicleMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleMaster();
                    $scope.GetVehicleMasterData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVehicleMaster = function () {
        ClearFieldsVehicleMaster();
        return true;
    };

    function ClearFieldsVehicleMaster() {
        $scope.ActionVM = 'Save';

        $scope.VehicleMaster = {
            VehicleName: null,
            VehicleNumber: null,
            FuelType: null,
            Milage: null,
            AvgFuelCostPerKL: null,
            AvgMaintenancePerKL: null,
            Remarks: null
        }
        $scope.VehicleMaster = Object.assign({}, $scope.VehicleMovementTemp);
    }
    // #endregion Vehicle Master

    //  #region PurposeMaaster
    $scope.ModelList = [];
    
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    //baseService.init($scope.getListUrl);

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,       
        StandardName: null,
        UserName: null,
        ShortName: null,
        Code: null,
        Active: true,
        Remarks: null,
       
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetemployeeDataList(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'datas': $scope.ModelNew,
                    
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
       
        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;

       

    }

    $scope.PurposeEmployeeList = [];
    $scope.GetemployeeDataList = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + "getemployeeDataList",
            data: { 'headerid': x },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurposeEmployeeList = response.data;

        });
    }
    $scope.GetemployeeDataList();

    $scope.ckdPurposeEmployeeList = [];
    $scope.SavePurposeRP = function () {
        for (var i = 0; i < $scope.PurposeEmployeeList.length; i++) {
            if ($scope.PurposeEmployeeList[i].isSelected == true && ($scope.PurposeEmployeeList[i].IsActive == null || $scope.PurposeEmployeeList[i].IsActive == false)) {
                $scope.ckdPurposeEmployeeList.push($scope.PurposeEmployeeList[i]);
            }
            else if ($scope.PurposeEmployeeList[i].isSelected == false && $scope.PurposeEmployeeList[i].Id != null) {

                $scope.ckdPurposeEmployeeList.push($scope.PurposeEmployeeList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'SavePurposeRP',
            data: {
                datalist: $scope.ckdPurposeEmployeeList,
                headerid: $scope.ModelNew.Id
            },
            dataType: 'JSON'
        })
            .then(function successCalback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.ckdPurposeEmployeeList = []
                    $scope.GetemployeeDataList($scope.ModelNew.Id);

                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            })

    }
    //  #endregion PurposeMaaster

    // #region LocationMaster
    $scope.LocationList = [];
    $scope.LocationAction = 'Save';
    $scope.getLocationListUrl = $scope.path + 'GetLocationList';
    $scope.getLocationSeqUrl = $scope.path + 'GetLocationSequence';
    $scope.saveLocationUrl = $scope.path + 'SaveLocation';
    $scope.deleteLocationUrl = $scope.path + 'deleteLocation/';

    $scope.LocationTemp = {
        Id: null,
        Sequence: 0,
        StandardName: null,
        UserName: null,
        ShortName: null,
        Code: null,
        Active: true,
        Remarks: null,
    };
    $scope.LocationNewModel = Object.assign({}, $scope.LocationTemp);

    $scope.GetLocationSequence = function () {
        cboService.getSequence($scope.getLocationSeqUrl, function (data) {
            $scope.LocationTemp.Sequence = data;
            $scope.LocationNewModel.Sequence = data;
        });
    };
    $scope.GetLocationSequence();

    $scope.GetLocation = function (args) {

        $scope.LocationNewModel = Object.assign({}, args.data);
        $scope.LocationAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    

    $scope.GetLocationData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetLocationList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.LocationList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetLocationSequence();
        });
    }
    $scope.GetLocationData();

    $scope.SaveLocation = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.LocationNewModelForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveLocationUrl,
                data: {
                    'data': $scope.LocationNewModel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsLocation(response.data.Sequence);
                    $scope.GetLocationData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteLocation = function () {
        if (!baseService.isUndefinedOrNull($scope.LocationNewModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteLocationUrl + $scope.LocationNewModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsLocation(response.data.Sequence);
                    $scope.GetLocationData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearLocation = function () {
        ClearFieldsLocation($scope.GetLocationSequence());
        return true;
    };

    function ClearFieldsLocation(seq) {
        $scope.LocationAction = 'Save';

        $scope.LocationNewModel = {
            Id: null,
            Sequence: 0,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
        };
        $scope.LocationNewModel = Object.assign({}, $scope.LocationTemp);
        $scope.LocationNewModel.Sequence = seq;

    }
    // #endregion LocationMaster

    // #region Fuel Master
    $scope.FuelList = [];
    $scope.FuelAction = 'Save';
    $scope.getFuelListUrl = $scope.path + 'GetMovementList';
    $scope.saveFuelUrl = $scope.path + 'SaveFuelMaster';
    $scope.deleteFuelUrl = $scope.path + 'deleteFuel/';

    $scope.FuelTemp = {
        Id: null,
        FuelType: null,
        Rate:0.00,
        EffectiveDate: null,
        Remarks: null
    };
    $scope.FuelNewModel = Object.assign({}, $scope.FuelTemp);

    $scope.GetFuel = function (args) {

        $scope.FuelNewModel = Object.assign({}, args.data);
        $scope.FuelAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.GetFuelData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetFuelData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FuelList = response.data;
            //ClearFields(response.data.Sequence);

        });
    }
    $scope.GetFuelData();

    $scope.SaveFuel = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.FuelForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveFuelUrl,
                data: {
                    'data': $scope.FuelNewModel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFuel();
                    $scope.GetFuelData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.ClearFuel = function () {
        ClearFieldsFuel();
        return true;
    };

    function ClearFieldsFuel() {
        $scope.FuelAction = 'Save';
        $scope.FuelNewModel = {
            Id: null,
            FuelName:null,
            EffectiveDate: null,
            Remarks: null
        };
        $scope.FuelNewModel = Object.assign({}, $scope.FuelTemp);

    }

    // #endregion Fuel Master
    
    // #region Get Fun
    $scope.FromLocationList = [];
    $scope.GetFromToLocationList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetFromToLocationList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FromLocationList = response.data;

        });
    }
    $scope.GetFromToLocationList();

    $scope.AllFuelList = [];
    $scope.GetFuelList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetFuelList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AllFuelList = response.data;

        });
    }
    $scope.GetFuelList();
    // #endregion Get Fun
    // Driver master

    // #region Driver Master
    $scope.DriverTemp = {
        Id: null,
        DriverName: null,
        DriverCode: null,
        DriverId: null,
        DriverName: null,
        LicenseNumber: null,
        ExpiryDate: null,
        Grade: null,
        AllowDutyHoursPerWeek: null
    };
    $scope.DriverMasterModel = Object.assign({}, $scope.DriverTemp);

    $scope.GetDriverMaster = function (args) {
        $scope.DriverMasterModel = Object.assign({}, args.data);
        $scope.ActionDM = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.DriverMasterList = [];
    $scope.GetDriverMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDriverMasterData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DriverMasterList = response.data;

        });
    }
    $scope.GetDriverMasterData();

    $scope.SaveDriverMaster = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.DriverMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveDriverMaster',
                data: {
                    'data': $scope.DriverMasterModel,

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDM();
                    $scope.GetDriverMasterData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDriverMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.DriverMasterModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteDriverMaster' + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDM();
                    $scope.GetDriverMasterData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearDM = function () {
        ClearFieldsVM();
        return true;
    };

    function ClearFieldsDM() {
        $scope.ActionDM = 'Save';

        $scope.DriverMasterModel = {
            Id: null,
            DriverName: null,
            EmpSystemId: null,
            EmployeeCode: null,
            EmployeeName: null,
            LicenseNumber: null,
            ExpiryDate: null,
            Grade: null,
            AllowDutyHoursPerWeek: null
        };
        $scope.DriverMasterModel = Object.assign({}, $scope.DriverTemp);
    }
    // #endregion Driver Master


    // #region Employee popup
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.Name = null;
    $scope.employeeList = [];
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.DriverMasterModel.DriverId = data.SystemId;
        $scope.DriverMasterModel.DriverName = data.EmployeeName;
        $scope.DriverMasterModel.DriverCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    // #endregion Employee popup

    
   
}