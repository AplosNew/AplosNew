'use strict';
routeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function routeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route';
    $scope.Action = 'Save';
    $scope.path = 'employees/route/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';

    $scope.route = {
        Id: null,
        DriverId: null,
        AssetId: null,
        Code: null,
        UserName: null,
        StandardName: null,
        ShortName: null,
        Description: null,
        Remarks: null,
        Active: true,
        UpOrDown: 'Up',      
    };
    $scope.routeNew = Object.assign({}, $scope.route);
    
    $scope.Save = function () {
        try {
            ValidationMaster();

            var StopageList = [];
            for (var i = 0; i < $scope.StopageListNew.length; i++) {
                StopageList.push($scope.StopageListNew[i]);
            }
            if (StopageList.length == 0) {
                throw "Please Select Stopage";
            }
            $scope.$broadcast('show-errors-check-validity');
            //if ($scope.routeNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'Route': $scope.routeNew, 'StopageList': StopageList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Update';
                       $scope.getData();
                        $scope.getRouteStopageData();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Code", $scope.routeNew.Code);
            CheckField("Short Name", $scope.routeNew.ShortName);
            CheckField("Standard Name", $scope.routeNew.StandardName);
            CheckField("User Name", $scope.routeNew.UserName);           
        } catch (ex) {
            throw ex;
        }
    };

    $scope.ModelList = [];
    $scope.getData = function () {
       // $scope.routeNew = Object.assign({}, $scope.route);
        $scope.ModelList = [];
        $http.get('employees/route/getlist')
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();

    $scope.StopageListNew = [];
    $scope.getRouteStopageData = function (args) {
        $scope.routeNew = Object.assign({}, args.data);
        $http.get('employees/route/getRouteStopage?RouteId=' + $scope.routeNew.Id)
            .then(function (response) {
                $scope.StopageListNew = response.data;
            });
    };

    $scope.recorddoubleclick = function (args) {
        $scope.routeNew = Object.assign({}, args.data);
        $scope.employeeInfo.EmployeeCode = $scope.routeNew.EmployeeCode;
        $scope.employeeInfo.EmployeeName = $scope.routeNew.DriverName;
        $scope.AssetInfo.Id = $scope.routeNew.AssetId;       
        $scope.AssetInfo.FixedAssetName = $scope.routeNew.FixedAsset;
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.getRouteStopageData(args);
        } catch (e) {

        }
    };

    $scope.StopageList = [];
    $scope.GetStopageInformation = function () {
        try {
            var eDialog = $("#StoppageInfo").data("ejDialog");
            eDialog.open();

            $http({
                method: 'GET',
                url: 'employees/route/GetStopageInformation'
            }).then(function successCallback(response) {
                $scope.StopageList = response.data;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;

    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };
    $scope.DeleteStopage = function (data) {
        $scope.StopagePrimaryId = data.StopagePrimaryId;
        if (baseService.isUndefinedOrNull(data.UserName))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteRow = function () {
        var tempData = $scope.StopageListNew;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].StopagePrimaryId === $scope.StopagePrimaryId) {
                $scope.StopageListNew.splice(i, 1);
            }
        }
        $scope.Id = null;
        tempData = [];
    };

    $scope.StopageListNew = [];
    $scope.OK = function () {
        try {

            for (var i = 0; i < $scope.StopageList.length; i++) {
                if ($scope.StopageList[i].CheckBoxSelect == true) {
                    if (checkDoubleStopageInformation($scope.StopageListNew, $scope.StopageList[i].StopagePrimaryId) === false) {
                        $scope.StopageListNew.push($scope.StopageList[i]);
                    }
                }
            }

            var eDialog = $("#StoppageInfo").data("ejDialog");
            eDialog.close();

            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    function checkDoubleStopageInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].StopagePrimaryId === Id) {
                return true;
            }
        }
        return false;
    }
    
    $scope.dataList = [];
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.employeeInfo = {};
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/route/getDriver'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    
    $scope.employeeInfo = {};
    $scope.SetEmpData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.routeNew.DriverId = emp.SystemID;      
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.AssetList = [];
        $scope.AssetInfo = {};
    $scope.GetAssetInfo = function () {
       // $scope.AssetList = [];
        $http({
            method: 'GET',
            url: 'employees/route/getFixedAsset'
        }).then(function successCallback(response) {
            $scope.AssetList = response.data;
            });

        angular.element(document.querySelector('#AssetPopUp')).modal('show');
    }

    $scope.AssetInfo = {};
    $scope.SetAssetData = function (obj) {
        var asset = obj.data;
        $scope.AssetInfo.Id = asset.Id;
        $scope.AssetInfo.FixedAssetName = asset.FixedAssetName;
        $scope.routeNew.AssetId = asset.Id;
        angular.element(document.querySelector('#AssetPopUp')).modal('hide');
    };

    $scope.closeAssetPopUp = function () {
        angular.element(document.querySelector('#AssetPopUp')).modal('hide');
    }

    $scope.Clear = function (obj) {
        ClearFields();
    };
    function ClearFields() { 
        $scope.routeNew = Object.assign({}, $scope.route);
        $scope.StopageListNew = [];
        $scope.employeeInfo.EmployeeCode = null;
        $scope.employeeInfo.EmployeeName = null;
        $scope.AssetInfo.Id = null;
        $scope.employeeInfo.DriverName = null;
        $scope.AssetInfo.FixedAssetName = null;
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.routeNew.Id)) {
            $http.get('employees/route/Delete?Id=' + $scope.routeNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                        $scope.getData();
                        $scope.getRouteStopageData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };
}