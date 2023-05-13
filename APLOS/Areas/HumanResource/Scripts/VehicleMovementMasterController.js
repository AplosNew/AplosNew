'use strict';
VehicleMovementMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleMovementMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    //$rootScope.title = "Vehicle Movement Master";
   
    
    
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    //  #region PurposeMaaster
    $scope.ModelList = [];
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

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
    //  #endregion PurposeMaaster

    // #region Vehicle Master
    $scope.ModelList = [];
    $scope.pathVM = 'HumanResource/VehicleMovementMaster/';
    $scope.getListUrlVM = $scope.pathVM + 'GetlistVehicleMaster';
    $scope.saveUrlVM = $scope.pathVM + 'SaveVehicleMaster';
    $scope.deleteUrlVM = $scope.pathVM + 'deleteVehicleMaster/';
    baseService.init($scope.getListUrlVM);

    $scope.VehicleMovementTemp = {
        Id: null,
        FromLocation: null,
        ToLocation: null,
        MinKillometer: null,
        Maxkillometer: null,
        CostPerKillometer: null,
        VehicleName: null,
        VehicleName: null,
        VehicleNumber: null,
        Milage: null,
        FuelType: null,
        Remarks:null,
        Remarks: null,

    };
    $scope.VehicleMovement = Object.assign({}, $scope.ModelTemp);


    $scope.GetVM = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getDataVM = function () {
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
    //$scope.getDataVM();

    $scope.SaveVM = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlVM,
                data: {
                    'datas': $scope.VehicleMovement,

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

    // #endregion Vehicle Master
}