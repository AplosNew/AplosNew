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
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

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

        RouteId: null,
        StoppageId: null,
        ShiftId: null,
        Shift: null,
        //UARouteUpGridId: null,
        //UAStopageUpGridId: null,
        //UARouteDownGridId: null,
        //UAStopageDownGridId: null,
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
            //gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
    $scope.UARouteUpList = [];
    $scope.UAStopageUpList = [];
    $scope.UnAssignApplyForAll = function () {
        try {

            //$scope.getUARouteUpList();
            //$scope.getUAstopageUpList(args);
            for (var i = 0; i < $scope.UnassignEmpList.length; i++) {
                if ($scope.UnassignEmpList[i].CheckBoxSelect == true) {
                    ///grid List=dropdown List
                    //$scope.UnassignEmpList[i].RouteList = $scope.RouteList;
                    //$scope.UnassignEmpList[i].StopageList = $scope.StopageList;
                    //$scope.UnassignEmpList[i].ShiftList = $scope.ShiftList;

                    //$scope.UnassignEmpList[i].UARouteUpList = $scope.UARouteUpList;
                    //$scope.UnassignEmpList[i].UAStopageUpList = $scope.UAStopageUpList;
                    //$scope.UnassignEmpList[i].UARouteDownList = $scope.UARouteDownList;
                    //$scope.UnassignEmpList[i].UAStopageDownList = $scope.UAStopageDownList;

                    ////grid model=dropdown model
                    $scope.UnassignEmpList[i].RouteId = $scope.UArouteEmployee.UARouteUpId;
                    $scope.UnassignEmpList[i].StoppageId = $scope.UArouteEmployee.UAStopageUpId;
                    $scope.UnassignEmpList[i].ShiftId = $scope.UArouteEmployee.ShiftId;


                    //$scope.UnassignEmpList[i].UARouteUpGridId = $scope.UArouteEmployee.UARouteUpId;
                    //$scope.UnassignEmpList[i].UAStopageUpGridId = $scope.UArouteEmployee.UAStopageUpId;
                    //$scope.UnassignEmpList[i].UARouteDownGridId = $scope.UArouteEmployee.UARouteDownId;
                    //$scope.UnassignEmpList[i].UAStopageDownGridId = $scope.UArouteEmployee.UAStopageDownId;
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
            //gridObj.refreshTemplate();


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

    $scope.RouteList = [];
    $scope.getUARouteUpList = function () {
        $http.get('employees/routeemployee/GetUAUpRouteList')
            .then(function (response) {
                var data = {
                    Id: null, UserName: ''
                }
                response.data.unshift(data);
                $scope.RouteList = response.data;

            });
    };
    $scope.getUARouteUpList();


    $scope.StopageList = [];
    $scope.getUAstopageUpDropDownList = function () {
        $http.get('employees/routeemployee/GetUpDropDownStopageList?RouteUpId=' + $scope.UArouteEmployee.UARouteUpId)
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.StopageList = response.data;
            });
    };

    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.UArouteEmployee.ShiftId = e.data.ShiftId;
        $scope.UArouteEmployee.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    //$scope.UAStopageDownList = [];
    //$scope.getUAstopageDownUpDropDownList = function () {
    //    $http.get('employees/routeemployee/GetDownDropDownStopageList?RouteDownId=' + $scope.UArouteEmployee.UARouteDownId)
    //        .then(function (response) {
    //            response.data.unshift($scope.data);
    //            $scope.UAStopageDownList = response.data;
    //        });
    //};

    $scope.ShiftList = [];
    $scope.getUADownRouteList = function () {
        $http.get('employees/routeemployee/GetUADownRouteList')
            .then(function (response) {
                response.data.unshift($scope.data);
                $scope.ShiftList = response.data;
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
                        $scope.UnassignEmpList[i]['StopageList'] = response.data;
                        //$scope.UnassignEmpList[i]['UAStopageUpList'] = response.data;
                        break;
                    }
                }
                var gridObj = $("#UnassignGrid").data("ejGrid");
                gridObj.refreshContent(true);
                //gridObj.refreshTemplate();
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
                //gridObj.refreshTemplate();
            });
    };


    $scope.onrowdatabound1 = function (e) { if (e.data.Id != null) e.row.css("background-color", "#aea8d3"); };

    $scope.UnassignEmpList = [];
    $scope.getUnassignEmp = function () {
        $http({
            method: 'GET',
            url: 'employees/routeemployee/getUnassignEmployee'
        }).then(function successCallback(response) {

            $scope.UnassignEmpList = response.data;
            var gridObj = $("#UnassignGrid").data("ejGrid");
            //gridObj.refreshContent(true);
            //gridObj.refreshTemplate();
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
            //for (var i = 0; i < response.data.length; i++) {
            //    response.data[i].RouteUpList.unshift($scope.data);
            //    response.data[i].StopageUpList.unshift($scope.data);
            //    response.data[i].RouteDownList.unshift($scope.data);
            //    response.data[i].StopageDownList.unshift($scope.data);
            //}
            $scope.dataList = response.data;
            var gridObj = $("#EmpGrid").data("ejGrid");
            //gridObj.refreshContent(true);
            //gridObj.refreshTemplate();
        });
    }
    $scope.getModalData_Employee();


    //NEW
    $scope.ModelList = [];
    $scope.path2 = 'HumanResource/ResidenceStatusAllocation/';
    $scope.getListUrl = $scope.path2 + 'getlist';
    $scope.saveUrl = $scope.path2 + 'Save';
    $scope.deleteUrl = $scope.path2 + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

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
            url: $scope.path + 'getemployeeDataList?plantId=' + data.data.PlantId 
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

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === id) {
                return true;
            }
        }
        return false;
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

    $scope.popupEmployeeList = [];
    $scope.PopupEmployeeView = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'PopupEmployeeView',
            data: {
                'EmployeeCategorySystemID': $scope.selectedData.EmployeeCategoryId,
                'fromDate': $scope.selectedData.fromDate,
                'toDate': $scope.selectedData.toDate,
            }

        }).then(function successCallback(response) {
            $scope.popupEmployeeList = response.data;
            document.getElementById("EmpGrid").style.display = "block";
        })
    }

    $scope.selResidenceMasterId = null;
    $scope.selResidenceMaster = function (e) {
        $scope.selResidenceMasterId = e.data.Id;
        $scope.openChildGrid();
        $scope.getResidenceStatusLocation();
    }

    $scope.openChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('show');
    }
    $scope.closeChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('hide');
    }



    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.PlantList = [];
        $scope.LocationList = [];
        $scope.ResidenceGroupIdList = [];
        $scope.ResidenceCategoryList = [];
        $scope.ResidenceSubCategoryList = [];
        $scope.BlockList = [];
        $scope.AssetNameList = [];
        $scope.ResidentTypeList = [];
        $scope.FloreList = [];
        $scope.ResidenceNumberList = [];
        $scope.EmployeeTypeIdList = [];
        $scope.RoomList = [];
 
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'getEmployee',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                'EmployeeCategoryId': $scope.selectedData.EmployeeCategoryId,
            },
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.selectEmpDetail = function () {
        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];
        for (var i = 0; i < $scope.EmployeeList.length; i++) {

            if ($scope.EmployeeList[i].isSelected == true) {
                $scope.SelEmpList.push($scope.EmployeeList[i]);
            }
        }

        if ($scope.SelEmpList.length > $scope.selectedData.VacancyList) {
            ShowResult('Selected Greater than vacancy allowed', 'failure');
            throw ('Invalid Request');
        }
        else {
            angular.element(document.querySelector('#EmpPop')).modal('hide');
        }

        $scope.getSelected();
    }

    $scope.EmpList = [];
    $scope.getSelected = function () {
        $scope.EmpList = $scope.SelEmpList;

    }


    // TAB - 2
    // ALL POP UPs

    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }
    // Select Emp
    $scope.EmployeeSelectedName = null;
    $scope.SelectedEmployeeId = null;
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;

        angular.element(document.querySelector('#EmployeePop')).modal('hide');


    }

    $scope.EmployeeList = [];
    $scope.getAllEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'getAllEmployee',
            data: { 'EmpCategoryId': $scope.EmpCategoryId },
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }
    //$scope.getAllEmployee();

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + "getEmployeeCategory",
            //data: { 'EmpId': $scope.SelectedEmployeeId},
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.EmployeeCategoryList = response.data;

        })
    }
    $scope.getEmployeeCategory();

    $scope.EmpCategoryId = null;
    $scope.EmpCategoryName = null;
    $scope.selEmployeeCategory = function (e) {
        $scope.EmpCategoryId = e.data.Id;
        $scope.EmpCategoryName = e.data.UserName;
        angular.element(document.querySelector('#EmpCategoryPop')).modal('hide');
        //  $scope.getAllEmployee();
    }


    $scope.ResidenceMasterList = [];
    $scope.getResidenceMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'getResidenceMaster',

        }).then(function success(resp) {
            $scope.ResidenceMasterList = resp.data;
        })
    }



    // Data Saved
    $scope.selectedDataR = {
        Id: null,
        isOccupied: false,
    };
    $scope.ResidenceData = Object.assign({}, $scope.selectedDataR);

    $scope.ResidenceStatusLocationList = [];
    $scope.getResidenceStatusLocation = function () {
        $http({
            method: "POST",
            url: $scope.path2 + "getResidenceStatusLocation",
            data: {
                'EmployeeId': $scope.SelectedEmployeeId,
                'ResidenceMasterId': $scope.selResidenceMasterId,
            },
        }).then(function seccessCallback(response) {
            $scope.ResidenceStatusLocationList = response.data
        })

    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headcheck").ejCheckBox({ "change": CheckBoxSelectAllPartyWises });
    };

    function CheckBoxSelectAllPartyWises(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEUnallocation").data("ejGrid").getFilteredRecords();
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
        var gridObj = $("#GridEUnallocation").data("ejGrid");
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
    $scope.AssignReport = function () {
        $scope.fileName = 'To Assign List';

        var dataList = [];
        var g = $("#GridRouteEmp").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.UnassignReport = function () {
        $scope.fileName = 'To Unassign List';
        var dataList = [];
        var g = $("#GridEUnallocation").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelUnassignList;
        }
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
    //-----------------------------------------------------------------------------------

    function openModal() {
        $('.confirm-delete').addClass('hide');
        $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
        $('#myModal').modal('show');
    }
    //-----------------------------------------------------------------------------------

    // REPORT DOWNLOAD
  

    $scope.ResidenceMasterReport = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + "XlsResidenceMaterReport",
            data: { 'empCurrentStatus': $scope.EmployeeNew.EmployeeCurrentStatus },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if (baseService.isUndefinedOrNull($scope.EmployeeNew.EmployeeCurrentStatus)) {
                    ShowResult('Employee Current Statusus Required.', 'failure');
                    throw "Invalid Request";
                }
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.allResidenceMasterReport = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + "XlsAllResidenceMaterReport",
            data: { 'empCurrentStatus': $scope.EmployeeNew.EmployeeCurrentStatus },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.ResidenceMasterList = [];
    $scope.gridViewResidenceMAster = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + "gridViewResidenceMAster",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ResidenceMasterList = response.data;
        })
    };

    $scope.EmployeeNew = {
        EmployeeCurrentStatus: null
    };

    $scope.EmployeeStatusList = [];
    $scope.employeeCurrentStatus = function () {
        $http({
            method: 'POST',
            url: $scope.path2 + 'employeeCurrrentStatus',
            dataType: 'JSON',
        }).then(function successCallback(response) {

            $scope.EmployeeStatusList = response.data;
        })
    };
    $scope.employeeCurrentStatus();

    $scope.ResidedenceGroupId = null;
    $scope.ResidenceGroupList = [];
    $scope.getResidence = function () {
        $http({
            method: 'GET',
            url: $scope.path2 + 'getemployeeDataList?plantId=' + $scope.PlantId + '&residenceGroupId=' + $scope.ResidedenceGroupId
        }).then(function successCallback(response) {
            $scope.dataList = response.data;


        });
    }



    $scope.ResidenceGroupList = [];
    $scope.ResidenceGroupCbo = function () {
        $http.get('employees/ResidenceGroup/GetCbo')
            .then(function (response) {
                $scope.ResidenceGroupList = response.data;

                $scope.ResidedenceGroupId = $scope.ResidenceGroupList[0].Value;


            });
    }

    $scope.ResidenceGroupCbo();


    //End
}