'use strict';
routeEmployeeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function routeEmployeeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route Employee';
    $scope.Action = 'Save';
    $scope.path = 'employees/routeemployee/';
    $scope.UAsaveUrl = $scope.path + 'SaveUnAssign';
    $scope.ApplyForAllUrl = $scope.path + 'SaveApplyForAll';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //#region Tab
    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    // #endregion Tab

    $scope.SaveUnAssign = function () {
        try {
            var AllCheckUAEmployeeList = [];
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                if ($scope.UnassignEmpList[i].CheckBoxSelect == true) {
                    var ob = {};
                    ob.RouteId = $scope.UArouteEmployee.UARouteUpId;
                    ob.StoppageId = $scope.UArouteEmployee.UAStopageUpId;
                    ob.ShiftId = $scope.UArouteEmployee.ShiftId;
                    ob.EmployeeId=$scope.UnassignEmpList[i].SystemID;
                    AllCheckUAEmployeeList.push(ob);
                    ob = {};
                }
            }
            if (AllCheckUAEmployeeList.length == 0) {
                throw "Please Select Employee";
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.UAsaveUrl,
                data: { 'UArouteEmployeeList': AllCheckUAEmployeeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getUnassignEmp();
                    $scope.getModalData_Employee();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

   
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };
    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#UnassignGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                $scope.UnassignEmpList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#UnassignGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.ModelList = [];
    }

    //NEW

    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };


    //#region The Filters 
    $scope.ModelList = [];
    $scope.view = function () {
        $http({
            method: "Get",
            url: $scope.path + 'GetRouteEmployeesData',
            //data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        })
    }

    $scope.view();
    //#endregion The Filters
   
    $scope.PlantId = null;
    $scope.dataList = [];
    $scope.PlantId = null;
    $scope.routeId = null;
    $scope.AvailablePopUpData = function (data) {
        $scope.TripId =  data.data.TripId;
        $scope.PlantId = data.data.PlantId;
        $scope.routeId = data.data.RouteId;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getemployeeDataListRouteEmp?plantId=' + data.data.PlantId
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
            $scope.GetStopageInformation();
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.closeEmployeePopUps = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        MakeData();
        $scope.view();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.saveList = [];
    function MakeData() {
        $scope.saveList = [];
        for (var i = 0; i < $scope.dataList.length; i++) {
            if ($scope.dataList[i].isSelected == true) {
                $scope.dataList[i].TripId = $scope.TripId;
                $scope.dataList[i].AssignStatus = true;
                $scope.dataList[i].AssignDate = Date.now();
                $scope.dataList[i].UnassignDate = null;
                $scope.saveList.push($scope.dataList[i]);
            }
        }
        $scope.SaveEmployeeTransportAllocation();
    }

    $scope.SaveEmployeeTransportAllocation = function () {
        try {
            for (var i = 0; i < $scope.saveList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.saveList[i].StoppageId)) {
                    throw 'Stoppage is required !';
                }
            }

            $http({
                method: 'POST',
                url: $scope.path + 'employeeTransportAllocationSave',
                data: { 'EmployeeList': $scope.saveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.view();
                    $scope.UnassignView();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPartyWise });
    };

    function CheckBoxSelectAllPartyWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.dataList.length; i++) {
                $scope.dataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmp").data("ejGrid");
        gridObj.refreshContent();
    };

 

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === id) {
                return true;
            }
        }
        return false;
    }

    $scope.StopageDataList = [];
    $scope.GetStopageInformation = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'GetStopageInformation?routeId=' +$scope.routeId ,
            }).then(function successCallback(response) {
                $scope.StopageDataList = response.data;
                for (var i = 0; i < $scope.dataList.length; i++) {
                    $scope.dataList[i].StopageDataList = $scope.StopageDataList;
                }
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    

    $scope.ModelUnassignList = [];
    $scope.UnassignView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId)) {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'viewUnassign?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelUnassignList = response.data;
        })
    }
    $scope.UnassignView();

    $scope.refreshTemplateemployee = function (args) {
        $("#headcheck").ejCheckBox({ "change": CheckBoxSelectAllPartyWises });
    };

    function CheckBoxSelectAllPartyWises(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEUnassign").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelUnassignList.length; i++) {
                $scope.ModelUnassignList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEUnassign").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveUnassignData = function () {
        $scope.unassignLoop = [];
        for (var i = 0; i < $scope.ModelUnassignList.length; i++) {

            if ($scope.ModelUnassignList[i].isSelected) {
                $scope.unassignLoop.push($scope.ModelUnassignList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'SaveUnassignData',
            data: { 'employeeList': $scope.unassignLoop },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.UnassignView();
                $scope.view();
            }
        });
    }
    //$scope.AssignReport = function () {
    //    $scope.fileName = 'To Assign List';

    //    var dataList = [];
    //    var g = $("#GridEmp").data("ejGrid");
    //    dataList = g.getFilteredRecords();

    //    if (dataList.length == 0) {
    //        dataList = $scope.dataList;
    //    }

    //    $http({
    //        method: 'POST',
    //        url: $scope.exportgriddataUrl,
    //        data: {
    //            'reportFileName': $scope.fileName,
    //            'data': dataList
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });

    //};

    //$scope.UnassignReport = function () {
    //    $scope.fileName = 'To Unassign List';
    //    var dataList = [];
    //    var g = $("#GridEUnassign").data("ejGrid");
    //    dataList = g.getFilteredRecords();

    //    if (dataList.length == 0) {
    //        dataList = $scope.ModelUnassignList;
    //    }
    //    $http({
    //        method: 'POST',
    //        url: $scope.exportgriddataUrl,
    //        data: {
    //            'reportFileName': $scope.fileName,
    //            'data': dataList
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });

    //};

    //End
}