'use strict';
routeEmployeeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function routeEmployeeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Route Employee';
    $scope.Action = 'Save';
    $scope.path = 'employees/routeemployee/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.UAsaveUrl = $scope.path + 'SaveUnAssign';
    $scope.ApplyForAllUrl = $scope.path + 'SaveApplyForAll';

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


    $scope.data = {
        Id: null,
        UserName: ''
    }


    $scope.routeEmployeeOrginal = {
        RouteUpId: null,
        RouteDownId: null,
        StopageUpId: null,
        StopageDownId: null,

        RouteUpGridId: null,
        StopageUpGridId: null,
        RouteDownGridId: null,
        StopageDownGridId: null,
    }
    $scope.routeEmployee = Object.assign({}, $scope.routeEmployeeModel);

    $scope.UArouteEmployeeOrginal = {
        UARouteUpId: null,
        UARouteDownId: null,
        UAStopageUpId: null,
        UAStopageDownId: null,

        UARouteUpGridId: null,
        UAStopageUpGridId: null,
        UARouteDownGridId: null,
        UAStopageDownGridId: null,
    }
    $scope.UArouteEmployee = Object.assign({}, $scope.UArouteEmployeeModel);

    $scope.ApplyForAll = function () {
        try {
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelectTwo == true) {

                    $scope.dataList[i].RouteUpList = $scope.RouteUpList;
                    $scope.dataList[i].StopageUpList = $scope.StopageUpList;
                    $scope.dataList[i].RouteDownList = $scope.RouteDownList;
                    $scope.dataList[i].StopageDownList = $scope.StopageDownList;

                    $scope.dataList[i].RouteUpGridId = $scope.routeEmployee.RouteUpId;
                    $scope.dataList[i].StopageUpGridId = $scope.routeEmployee.StopageUpId;
                    $scope.dataList[i].RouteDownGridId = $scope.routeEmployee.RouteDownId;
                    $scope.dataList[i].StopageDownGridId = $scope.routeEmployee.StopageDownId;
                }
            }

            var _data = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                _data.push(Object.assign({}, $scope.dataList[i]));
            }
            $scope.dataList = [];

            $scope.dataList = _data;


            var gridObj = $("#EmpGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.UnAssignApplyForAll = function () {
        try {

            //$scope.getUARouteUpList();
            //$scope.getUAstopageUpList(args);
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                if ($scope.UnassignEmpList[i].CheckBoxSelect == true) {
                    ///grid List=dropdown List
                    $scope.UnassignEmpList[i].UARouteUpList = $scope.UARouteUpList;
                    $scope.UnassignEmpList[i].UAStopageUpList = $scope.UAStopageUpList;
                    $scope.UnassignEmpList[i].UARouteDownList = $scope.UARouteDownList;
                    $scope.UnassignEmpList[i].UAStopageDownList = $scope.UAStopageDownList;
                    ////grid model=dropdown model
                    $scope.UnassignEmpList[i].UARouteUpGridId = $scope.UArouteEmployee.UARouteUpId;
                    $scope.UnassignEmpList[i].UAStopageUpGridId = $scope.UArouteEmployee.UAStopageUpId;
                    $scope.UnassignEmpList[i].UARouteDownGridId = $scope.UArouteEmployee.UARouteDownId;
                    $scope.UnassignEmpList[i].UAStopageDownGridId = $scope.UArouteEmployee.UAStopageDownId;
                }
            }
            var _data = [];
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                _data.push(Object.assign({}, $scope.UnassignEmpList[i]));
            }
            $scope.UnassignEmpList = [];

            //var gridObj = $("#UnassignGrid").data("ejGrid");
            //gridObj.refreshContent(true);
            //gridObj.refreshTemplate();

            $scope.UnassignEmpList = _data;


            var gridObj = $("#UnassignGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Save = function () {
        try {
            var AllCheckEmployeeList = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelectTwo == true) {
                    AllCheckEmployeeList.push($scope.dataList[i]);
                }
            }
            if (AllCheckEmployeeList.length == 0) {
                throw "Please Select Employee";
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'routeEmployee': $scope.routeEmployee, 'routeEmployeeList': AllCheckEmployeeList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.getUnassignEmp();
                    $scope.getModalData_Employee();


                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.SaveUnAssign = function () {
        try {
            var AllCheckUAEmployeeList = [];
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                if ($scope.UnassignEmpList[i].CheckBoxSelect == true) {
                    AllCheckUAEmployeeList.push($scope.UnassignEmpList[i]);
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

    //#region Grid DropDown

    $scope.RouteGridUpList = [];
    $scope.getGridRouteUpList = function () {
        $http.get('employees/routeemployee/GetGridUpRouteList')
            .then(function (response) {
                $scope.RouteGridUpList = response.data;
            });
    };
    $scope.getGridRouteUpList();

    $scope.StopageGridUpList = [];
    $scope.getGridstopageUpList = function (RouteUpGridId) {
        $http.get('employees/routeemployee/GetGridUpStopageList?RouteUpGridId=' + RouteUpGridId)
            .then(function (response) {
                $scope.StopageGridUpList = response.data;
            });
    };

    // #endregion 
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

    $scope.Delete = function () {
        try {
            var AllCheckEmployeeListForUnAssign = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelectTwo == true) {
                    AllCheckEmployeeListForUnAssign.push($scope.dataList[i]);
                }
            }
            if (AllCheckEmployeeListForUnAssign.length == 0) {
                throw "Please Select Employee";
            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { 'DeleteEmpList': AllCheckEmployeeListForUnAssign },
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

    $scope.RouteUpList = [];
    $scope.getRouteUpList = function () {
        $http.get('employees/routeemployee/GetUpRouteList')
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.RouteUpList = response.data;
            });
    };
    $scope.getRouteUpList();

    $scope.RouteDownList = [];
    $scope.getDownRouteList = function () {
        $http.get('employees/routeemployee/GetDownRouteList')
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.RouteDownList = response.data;
            });
    };
    $scope.getDownRouteList();

    $scope.StopageUpList = [];
    $scope.getstopageUpDropDownList = function () {
        $http.get('employees/routeemployee/GetUpDropDownStopageList?RouteUpId=' + $scope.routeEmployee.RouteUpId)
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.StopageUpList = response.data;
            });
    };

    $scope.StopageDownList = [];
    $scope.getstopageDownUpDropDownList = function () {
        $http.get('employees/routeemployee/GetDownDropDownStopageList?RouteDownId=' + $scope.routeEmployee.RouteDownId)
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.StopageDownList = response.data;
            });
    };

    $scope.getstopageUpList = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObjRunning = $("#EmpGrid").ejGrid("instance");
        var currRow = gridObjRunning.model.currentViewData[this.element.closest("tr").index()];

        $http.get('employees/routeemployee/getUpStopage?RouteId=' + currRow.RouteUpGridId + '&UpOrDown=Up')
            .then(function (response) {
                for (var i = 0; i < $scope.dataList.length; i++) {
                    if ($scope.dataList[i].SystemID == currRow.SystemID) {
                        response.data.unshift($scope.data);
                        $scope.dataList[i]['StopageUpList'] = response.data;
                        break;  //StopageUpList
                    }
                }
                var gridObj = $("#EmpGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            })
    };

    $scope.getstopageDownList = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObjRunning = $("#EmpGrid").ejGrid("instance");
        var currRow = gridObjRunning.model.currentViewData[this.element.closest("tr").index()];
        $http.get('employees/routeemployee/getDownStopage?RouteId=' + currRow.RouteDownGridId + '&UpOrDown=Down')
            .then(function (response) {
                for (var i = 0; i < $scope.dataList.length; i++) {
                    if ($scope.dataList[i].SystemID == currRow.SystemID) {
                        response.data.unshift($scope.data);
                        $scope.dataList[i]['StopageDownList'] = response.data;
                        break;
                    }
                }
                var gridObj = $("#EmpGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            });
    };
    
    $scope.refreshTemplateemployeeTWO = function (args) {
        $("#headchkTWO").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWiseTWO });
    };

    function CheckBoxSelectAllEmolyeeWiseTWO(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#EmpGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.dataList.length; i++) {
                $scope.dataList[i].CheckBoxSelectTwo = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelectTwo = ChkOrUnchk;
            }
        }
        var gridObj = $("#EmpGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {

    }

    $scope.UARouteUpList = [];
    $scope.getUARouteUpList = function () {
        $http.get('employees/routeemployee/GetUAUpRouteList')
            .then(function (response) {
                var data = {
                    Id: null, UserName: ''
                }
                response.data.unshift(data);
                $scope.UARouteUpList = response.data;

            });
    };
    $scope.getUARouteUpList();


    $scope.UAStopageUpList = [];
    $scope.getUAstopageUpDropDownList = function () {
        $http.get('employees/routeemployee/GetUpDropDownStopageList?RouteUpId=' + $scope.UArouteEmployee.UARouteUpId)
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.UAStopageUpList = response.data;
            });
    };

    $scope.UAStopageDownList = [];
    $scope.getUAstopageDownUpDropDownList = function () {
        $http.get('employees/routeemployee/GetDownDropDownStopageList?RouteDownId=' + $scope.UArouteEmployee.UARouteDownId)
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.UAStopageDownList = response.data;
            });
    };

    $scope.UARouteDownList = [];
    $scope.getUADownRouteList = function () {
        $http.get('employees/routeemployee/GetUADownRouteList')
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.UARouteDownList = response.data;
            });
    };
    $scope.getUADownRouteList();

    $scope.getUAstopageUpList = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObjRunning = $("#UnassignGrid").ejGrid("instance");
        var currRow = gridObjRunning.model.currentViewData[this.element.closest("tr").index()];

        $http.get('employees/routeemployee/getUpStopage?RouteId=' + currRow.UARouteUpGridId + '&UpOrDown=Up')
            .then(function (response) {
                for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                    if ($scope.UnassignEmpList[i].SystemID == currRow.SystemID) {
                        response.data.unshift($scope.data);
                        $scope.UnassignEmpList[i]['UAStopageUpList'] = response.data;
                        break;
                    }
                }
                var gridObj = $("#UnassignGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            })
    };

    $scope.getUAstopageDownList = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObjRunning = $("#UnassignGrid").ejGrid("instance");
        var currRow = gridObjRunning.model.currentViewData[this.element.closest("tr").index()];
        $http.get('employees/routeemployee/getUADownStopage?RouteId=' + currRow.UARouteDownGridId + '&UpOrDown=Down')
            .then(function (response) {
                for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                    if ($scope.UnassignEmpList[i].SystemID == currRow.SystemID) {
                        response.data.unshift($scope.data);
                        $scope.UnassignEmpList[i]['UAStopageDownList'] = response.data;
                        break;
                    }
                }
                var gridObj = $("#UnassignGrid").data("ejGrid");
                gridObj.refreshContent(true);
                gridObj.refreshTemplate();
            });
    };


    $scope.onrowdatabound1 = function (e) { if (e.data.Id != null) e.row.css("background-color", "#aea8d3"); };

    $scope.UnassignEmpList = [];
    $scope.getUnassignEmp = function () {
        $http({
            method: 'GET',
            url: 'employees/routeemployee/getUnassignEmployee'
        }).then(function successCallback(response) {

            for (var i = 0; i < response.data.length; i++) {
                response.data[i].UARouteUpList.unshift($scope.data);
                response.data[i].UAStopageUpList.unshift($scope.data);
                response.data[i].UARouteDownList.unshift($scope.data);
                response.data[i].UAStopageDownList.unshift($scope.data);
            }
            $scope.UnassignEmpList = response.data;
            var gridObj = $("#UnassignGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        });
    }
    $scope.getUnassignEmp();

    $scope.dataList = [];
    $scope.getModalData_Employee = function () {
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/routeemployee/getEmployee'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i].RouteUpList.unshift($scope.data);
                response.data[i].StopageUpList.unshift($scope.data);
                response.data[i].RouteDownList.unshift($scope.data);
                response.data[i].StopageDownList.unshift($scope.data);
            }
            $scope.dataList = response.data;
            var gridObj = $("#EmpGrid").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
        });
    }
    $scope.getModalData_Employee();

}