'use strict';
CutPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function CutPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Master Plan';
    $scope.Action = 'Save';
    $scope.path = 'Productions/CutPlan/';

    $scope.processList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetProcessList'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.entityList = [];
    $scope.GetEntityList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetEntityList?ProcessId=' + PId
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

    $scope.cutplan = {
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
    $scope.cutplanNew = Object.assign({}, $scope.cutplan);


    $scope.GetMasterPlanFieldStatusList = function (PId) {
        $http({
            method: 'GET',
            url: 'Productions/CutPlan/GetMasterPlanFieldStatusList?ProcessId=' + PId
        }).then(function successCallback(response) {
            $scope.cutplanNew.LineItem = response.data[0].LineItem;
            $scope.cutplanNew.SKU1 = response.data[0].SKU1;
            $scope.cutplanNew.SKU2 = response.data[0].SKU2;
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
            $scope.cutplanNew.UserId = e.data.SystemId;
            $scope.cutplanNew.User = e.data.EmployeeName;
        }
        else {
            $scope.cutplanNew.ResponsiblePersonId = e.data.SystemId;
            $scope.cutplanNew.ResponsiblePerson = e.data.EmployeeName;
        }
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    }

    $scope.CutPlanList = [];
    $scope.LoadCutPlanList = function () {
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/LoadCutPlanList'
        }).then(function successCallback(response) {
            $scope.CutPlanList = response.data;
            var gridObj = $("#GridCutPlan").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadCutPlanList();

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

    $scope.GetCutPlanDetails = function (args) {
        $scope.CutPlanId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Productions/CutPlan/LoadCutPlanEditData?CutPlanId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.cutplanNew = response.data.cutplan[0];
            $scope.cutplanNew.ResponsiblePerson = response.data.cutplan[0].ResponsiblePerson;
            $scope.cutplanNew.User = response.data.cutplan[0].UserName;
            getCutPlanDetailsList();
            $scope.GetProcessList();
            $scope.GetEntityList($scope.cutplanNew.ProcessId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.View = function () {
        getCutPlanDetailsList();
    }

    $scope.CutPlanListSelected = [];
    function getCutPlanDetailsList() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCutPlanDetailsList?ProcessId=' + $scope.cutplanNew.ProcessId + '&PlanId=' + $scope.cutplanNew.Id,
        }).then(function successCallback(response) {
            $scope.CutPlanListSelected = response.data;
            var gridObj = $("#GridSOItemSelected").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.refreshTemplateCutPlan = function (args) {
        $("#Cheadchk").ejCheckBox({ "change": CheckBoxSelectAllCutPlan });
    };
    function CheckBoxSelectAllCutPlan(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSOItemSelected").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.CutPlanListSelected.length; i++) {
                $scope.CutPlanListSelected[i].Status = ChkOrUnchk;
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
    $scope.SaveCutPlan = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            try {
                $scope.SaveList = [];
                $scope.TempList = [];
                for (var i = 0; i < $scope.CutPlanListSelected.length; i++) {
                    if ($scope.CutPlanListSelected[i].Status == true || ($scope.CutPlanListSelected[i].Status == false && $scope.CutPlanListSelected[i].Id != null)) {
                        $scope.TempList.push($scope.CutPlanListSelected[i]);
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
                    url: 'Productions/CutPlan/CreateData',
                    data: {
                        'data': $scope.cutplanNew,
                        'DataList': $scope.SaveList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.LoadCutPlanList();
                        CutPlanClearFields();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
                        /*getCutPlanDetailsList()*/;
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

    $scope.ClearCutPlan = function () {
        CutPlanClearFields();
    };

    function CutPlanClearFields() {
        $scope.Action = "Save";
        $scope.cutplanNew = Object.assign({}, $scope.cutplan);
        $scope.CutPlanListSelected = [];
    }
}