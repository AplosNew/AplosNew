'use strict';
MasterPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MasterPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Master Plan';
    $scope.Action = 'Save';
    $scope.path = 'Productions/MasterPlan/';

    $scope.processList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlan/GetProcessList'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.entityList = [];
    $scope.GetEntityList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlan/GetEntityList?ProcessId=' + PId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }

    $scope.PlanStatusList = [];
    $scope.getAllPlanStatus = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetPlanStatus"
        }).then(function successCallback(response) {
            $scope.PlanStatusList = response.data;
        });
    }
    $scope.getAllPlanStatus();

    $scope.MasterPlan = {
        Id: null
        , ProcessId: null
        , EntityId: null
        , PlanName: null
        , UserId: $window.employeeId
        , User: $window.employeeName
        , PlanStatus: "Active"
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , Remarks: null
        , LineItem: false
        , SKU1: false
        , SKU2: false
    };
    $scope.MasterPlanNew = Object.assign({}, $scope.MasterPlan);


    $scope.GetMasterPlanFieldStatusList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlan/GetMasterPlanFieldStatusList?ProcessId=' + PId
        }).then(function successCallback(response) {
            $scope.MasterPlanNew.LineItem = response.data[0].LineItem;
            $scope.MasterPlanNew.SKU1 = response.data[0].SKU1;
            $scope.MasterPlanNew.SKU2 = response.data[0].SKU2;
        });
    }

    $scope.Employee = null;
    $scope.ResponsiblePersonList = [];
    $scope.selectResponsiblePerson = function (flag) {
        $scope.Employee = flag;
        $http({
            method: 'POST',
            url: $scope.path + 'GetUserName',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsiblePersonList = resp.data;
        });
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('show');
    }

    $scope.doubleResponsiblePerson = function (e) {
        if ($scope.Employee === 'User') {
            $scope.MasterPlanNew.UserId = e.data.SystemId;
            $scope.MasterPlanNew.User = e.data.EmployeeName;
        }
        else {
            $scope.MasterPlanNew.ResponsiblePersonId = e.data.SystemId;
            $scope.MasterPlanNew.ResponsiblePerson = e.data.EmployeeName;
        }
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.MasterPlanList = [];
    $scope.LoadMasterPlanList = function () {
        $http({
            method: 'Get',
            url: 'Productions/MasterPlan/LoadMasterPlanList'
        }).then(function successCallback(response) {
            $scope.MasterPlanList = response.data;
            var gridObj = $("#GridMasterPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadMasterPlanList();

    $scope.MaterialID = "";
    $scope.isAlternative = -1;
    $scope.rowDataBound = function rowDataBound(e) {
        if ($scope.MaterialID != e.data.ProductionGrouping + e.data.MaterialMasterId) {
            $scope.isAlternative = $scope.isAlternative * -1;
            $scope.MaterialID = e.data.ProductionGrouping + e.data.MaterialMasterId;
        }
        if ($scope.isAlternative > 0)
            e.row.css("background-color", "#90EE90");
        else
            e.row.css("background-color", '##013220');
    }

    $scope.GetMasterPlanDetails = function (args) {
        $scope.MasterPlanId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Productions/MasterPlan/LoadMasterPlanEditData?MasterPlanId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.MasterPlanNew = response.data.MasterPlan[0];
            $scope.MasterPlanNew.ResponsiblePerson = response.data.MasterPlan[0].ResponsiblePerson;
            $scope.MasterPlanNew.User = response.data.MasterPlan[0].UserName;
            getMasterPlanDetailsList();
            $scope.GetProcessList();
            $scope.GetEntityList($scope.MasterPlanNew.ProcessId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.View = function () {
        getMasterPlanDetailsList();
    }

    $scope.MasterPlanListSelected = [];
    function getMasterPlanDetailsList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetMasterPlanDetailsList?ProcessId=' + $scope.MasterPlanNew.ProcessId + '&PlanId=' + $scope.MasterPlanNew.Id,
        }).then(function successCallback(response) {
            $scope.MasterPlanListSelected = response.data;
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.refreshTemplateMasterPlan = function (args) {
        $("#Cheadchk").ejCheckBox({ "change": CheckBoxSelectAllMasterPlan });
    };
    function CheckBoxSelectAllMasterPlan(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSOItemSelected").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MasterPlanListSelected.length; i++) {
                $scope.MasterPlanListSelected[i].Status = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Status = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.PlanPer = 0;
    $scope.SOQty = 0;
    $scope.SOPlanQtyCal = function (data) {
        try {
            $scope.PlanPer = 0;
            $scope.SOQty = 0;
            $scope.SOPlanQty = 0;
            $scope.PlanPer = data.data.PlanPercentage;
            $scope.SOQty = data.data.Qty;
            data.data.SOPlanQty = $scope.SOQty + ($scope.PlanPer * $scope.SOQty / 100);
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.PG = null;
    $scope.SaveMasterPlan = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            try {
                $scope.SaveList = [];
                $scope.TempList = [];
                for (var i = 0; i < $scope.MasterPlanListSelected.length; i++) {
                    if ($scope.MasterPlanListSelected[i].Status == true || ($scope.MasterPlanListSelected[i].Status == false && $scope.MasterPlanListSelected[i].Id != null)) {
                        $scope.TempList.push($scope.MasterPlanListSelected[i]);
                    }
                }
                for (var j = 0; j < $scope.TempList.length; j++) {
                    if (j > 0) {
                        if ($scope.TempList[j].ProductionGrouping.toUpperCase() == $scope.PG.toUpperCase()) {
                            $scope.SaveList.push($scope.TempList[j]);
                        }
                        else
                        {
                            throw "Different Production Group are not supported... ";
                        }
                    }
                    else {
                        $scope.SaveList.push($scope.TempList[j]);
                    }
                    $scope.PG = $scope.TempList[j].ProductionGrouping;
                }

                $http({
                    method: "POST",
                    url: 'Productions/MasterPlan/CreateData',
                    data: {
                        'data': $scope.MasterPlanNew,
                        'DataList': $scope.SaveList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadMasterPlanList();
                        MasterPlanClearFields();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
                        /*getMasterPlanDetailsList()*/;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;

            } catch (e) {
                ShowResult(e, "failure");
            }
        }
    };

    $scope.ClearMasterPlan = function () {
        MasterPlanClearFields();
    };

    function MasterPlanClearFields() {
        $scope.Action = "Save";
        $scope.MasterPlanNew = Object.assign({}, $scope.MasterPlan);
        $scope.MasterPlanListSelected = [];
    }
}