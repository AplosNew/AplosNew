'use strict';
InspectionTransactionController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function InspectionTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Inspection";

    $scope.Action = 'Save';
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


    $scope.empearch = "";
    $scope.searchByEmp = "EmployeeCode"; $scope.empearch = "";
    $scope.searchEmpByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "Employee Name" }];

    $scope.popUpEmpDataList = [];
    $scope.employee = [];
    $scope.tag = "";

    $scope.getEmpPopUpData = function (tag) {
        try {
            $scope.tag = tag;

            $scope.employee = [];
            $scope.popUpEmpDataList = [];
            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/getemployeelist',
                data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: null },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.employee = response.data;
                $scope.popUpEmpDataList = response.data;
                angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.getEmpData = function () {
        $scope.employee = [];
        $scope.popUpEmpDataList = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/getemployeelist',
            data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: null },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
            $scope.popUpEmpDataList = response.data;
        });
    }

    $scope.setEmpData = function (obj) {
        if ($scope.tag == 'EN') {
            $scope.ModelNew.EmployeeId = obj.data.SystemID;
            $scope.ModelNew.EmployeeName = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        else if ($scope.tag == 'WC') {
            $scope.ModelNew.WCInchargeId = obj.data.SystemID;
            $scope.ModelNew.WCIncharge = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        else {
            $scope.ModelNew.ReportingOfficerId = obj.data.SystemID;
            $scope.ModelNew.ReportingOfficer = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.searchBy = "InspectionUserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'InspectionUserName', name: "User Name" }, { value: 'Remarks', name: "Remarks" }];
    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: "QMS/QualityProcess/GetInspectionList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        EntityId: null,
        ProcessId: null,
        WorkCenterMasterId: null,
        DateTime: null,
        ShiftId: null,
        EmployeeId: null,
        WCInchargeId: null,
        ReportingOfficerId: null,
        InspectionUserName: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.loadProcessList($scope.ModelNew.EntityId);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.ModelNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList($scope.ModelNew.EntityId);
            }
        });
    }
    $scope.getAllEntities();

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            $scope.GetShiftList();
        });
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.ModelNew.ProcessId)
            .then(function (response) {
                $scope.shiftList = response.data;
                $scope.loadWC($scope.ModelNew.ProcessId, $scope.ModelNew.EntityId, $scope.ModelNew.ShiftId);
            });
    }

    $scope.wcList = [];
    $scope.loadWC = function (processid, entityId, shiftId) {
        cboService.GetWCProcessCbo(processid, entityId, shiftId, function (result) {
            $scope.wcList = result;
        });

    };



    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/CreateInspection',
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.deleteUrl = 'QMS/QualityProcess/DeleteInspection/';
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
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }






}