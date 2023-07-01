'use strict';
VehicleInOutController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleInOutController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle In & Out"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleAllocation';
    $scope.ActionIn = "Save";
    $scope.ActionOut = "Save";
    $scope.Action = 'Update';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.intab = 1;
    $scope.insetTab = function (newTab) {
       
        $scope.intab = newTab;
    };

    $scope.isinSet = function (tabNum) {
        return $scope.intab === tabNum;
    };

    $scope.outsetTab = function (newTab) {
        $scope.tab = 1;
        $scope.tab = newTab;
    };

    $scope.isoutSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.VehicleAllocationList = [];
    $scope.GetVehicleAllocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleAllocation",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleAllocationList = response.data;

        });
    }
    $scope.GetVehicleAllocation();

    $scope.VehicleList = [];
    $scope.GetVehicleList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleList = response.data;

        });
    }

    $scope.DriverList = [];
    $scope.GetDriverList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDriverList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DriverList = response.data;

        });
    }

    $scope.VehicleRequisitionTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        FromTime: null,
        ToTime: null,
        VehicleMasterId: null,
        DriverMasterId: null,
        FromLocation: null,
        

    };
    $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);


    $scope.ClearVehicleRequisition = function () {
        ClearVehicleRequisitionFields();
        return true;
    };

    function ClearVehicleRequisitionFields() {
       
        $scope.VehicleRequisitionModel = {
            Id: null,
            FromDate: null,
            ToDate: null,
            FromTime: null,
            ToTime: null,
            VehicleMasterId: null,
            DriverMasterId: null,
            FromLocation: null,
        };
        $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);
    }

    $scope.Get = function (args) {
        $scope.VehicleRequisitionModel = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $scope.GetVehicleList();
            $scope.GetDriverList();
            $scope.VehicleInData(args.data.VehicleAllocationId);
           
            $rootScope.toggle();
            
        }
    }

    $scope.PendingInTripList = [];
    $scope.GetPendingInTrip = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPendingInTrip",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PendingInTripList = response.data;

        });
    }
    $scope.GetPendingInTrip();

    $scope.PendingOutTripList = [];
    $scope.GetPendingOutTrip = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPendingOutTrip",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PendingOutTripList = response.data;

        });
    }
    $scope.GetPendingOutTrip();

    //  #region VehicleIn
    var todaDate = new Date();
    $scope.VehicleInTemp = {
        Id: null,
        InDate: todaDate,
        InTime: todaDate,
        InReading: null,
        Remarks: null

    };

    $scope.VehicleInModel = Object.assign({}, $scope.VehicleInTemp);

    $scope.GetIn = function (args) {
        $scope.VehicleInModel = Object.assign({}, args[0]);
        $scope.VehicleInModel.InDate = todaDate;
        $scope.VehicleInModel.InTime = todaDate;
        $scope.ActionIn = 'Update';
        if (!$rootScope.isCollapsed) {
            
           // $rootScope.toggle();
        }
    }

    $scope.VehicleInList = [];
    $scope.VehicleInData = function (VehicleAllocationId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleInData",
            data: { 'vehicleallocationid': VehicleAllocationId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleInList = response.data;
            $scope.GetIn(response.data);
            

        });
    }
    $scope.VehicleInData();

    $scope.SaveVehicleIn = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleInForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path +  'SaveVehicleIn',
                data: {
                    'data': $scope.VehicleInModel,
                    'headerId': $scope.VehicleRequisitionModel.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleIN();
                    ClearVehicleRequisitionFields();
                    $scope.VehicleInData();
                    $scope.GetPendingInTrip();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVehicleIn = function () {
        if (!baseService.isUndefinedOrNull($scope.VehicleInModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteVehicleIn' + $scope.VehicleInModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleIN();
                    $scope.VehicleInData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVehicleIn = function () {
        ClearFieldsVehicleIN();
        return true;
    };

    function ClearFieldsVehicleIN() {
        $scope.ActionIn = 'Save';

        $scope.VehicleInModel = {
            Id: null,
            InDate: null,
            InTime: null,
            InReading: null,
            Remarks: null
        };
        $scope.VehicleInModel = Object.assign({}, $scope.VehicleInTemp);
    }
    //  #endregion VehicleIn

    //  #region VehicleOut
    $scope.VehicleOutTemp = {
        Id: null,
        OutDate: todaDate,
        OutTime: todaDate,
        OutReading: null,
        Remarks: null

    };

    $scope.VehicleOutModel = Object.assign({}, $scope.VehicleOutTemp);

    $scope.GetOut = function (args) {
        $scope.VehicleOutModel = Object.assign({}, args.data);
        $scope.ActionOut = 'Update';
        if (!$rootScope.isCollapsed) {

            //$rootScope.toggle();
        }
    }

    $scope.VehicleOutList = [];
    $scope.VehicleOutData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleOutData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleOutList = response.data;


        });
    }


    $scope.SaveVehicleOut = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleOutForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveVehicleOut',
                data: {
                    'data': $scope.VehicleOutModel,
                    'headerId': $scope.VehicleRequisitionModel.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleOut();
                    ClearVehicleRequisitionFields();
                    $scope.GetPendingOutTrip();
                    $scope.VehicleOutData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVehicleOut = function () {
        if (!baseService.isUndefinedOrNull($scope.VehicleOutModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteVehicleOut' + $scope.VehicleOutModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleOut();
                    $scope.VehicleOutData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVehicleOut = function () {
        ClearFieldsVehicleOut();
        return true;
    };

    function ClearFieldsVehicleOut() {
        $scope.ActionOut = 'Save';

        $scope.VehicleOutModel = {
            Id: null,
            OutDate: null,
            OutTime: null,
            OutReading: null,
            Remarks: null
        };
        $scope.VehicleOutModel = Object.assign({}, $scope.VehicleOutTemp);
    }
    //  #endregion VehicleOut

    $scope.ReportDataList = [];
    $scope.GetReportData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetReportData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ReportDataList = response.data;

        });
    }
    $scope.GetReportData();

}