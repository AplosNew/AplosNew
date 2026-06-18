'use strict';
InspectionTransactionController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function InspectionTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
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
        if ($scope.tag == 'QI') {
            $scope.ModelNew.QualityInchargeId = obj.data.SystemID;
            $scope.ModelNew.QualityIncharge = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        else if ($scope.tag == 'PI') {
            $scope.ModelNew.ProductionInchargeId = obj.data.SystemID;
            $scope.ModelNew.ProductionIncharge = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
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
        EmployeeId: $window.employeeId,
        WCInchargeId: null,
        ReportingOfficerId: null,
        QualityInchargeId: null,
        ProductionInchargeId: null,
        InspectionTypeId: null,
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
        $scope.loadProcessList();
        $scope.GetShiftList();
        $scope.loadWC();
        $scope.getInspectionTypeEntryLevel();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.inspectionTypeList = [];
    $scope.getInspectionType = function () {
        $http({
            method: 'Get',
            url: "QMS/QualityProcess/GetInspectionTypeCbo"
        }).then(function successCallback(response) {
            $scope.inspectionTypeList = response.data;

        });
    }
    $scope.getInspectionType();

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
                $scope.loadProcessList();
            }
        });
    }
    $scope.getAllEntities();

    $scope.loadProcessList = function () {
        cboService.GetEntityProcessCbo($scope.ModelNew.EntityId, function (result) {
            $scope.processList = result;
        });
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.ModelNew.ProcessId)
            .then(function (response) {
                $scope.shiftList = response.data;
            });
    }

    $scope.wcList = [];
    $scope.loadWC = function () {
        cboService.GetWCProcessCbo($scope.ModelNew.ProcessId, $scope.ModelNew.EntityId, $scope.ModelNew.ShiftId, function (result) {
            $scope.wcList = result;
        });

    };
   

    $scope.InspectionTypeEntryLevelList = [];
    $scope.getInspectionTypeEntryLevel = function () {
        $http({
            method: 'POST',
            url: "QMS/QualityProcess/GetInspectionTypeEntryLevelList?imageInspectionTypeId=" + $scope.ModelNew.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InspectionTypeEntryLevelList = response.data;
           
        });
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $scope.ModelNew.EmployeeId = $window.employeeId;
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

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function (data,name) {

        $scope.ProductionOrderList = [];
        var path = '';
        if (name=='PO') {
            path = 'Productions/ProductionSummary/GetProductionOrderDataListWC?entityid=' + $scope.ModelNew.EntityId + '&workCenterMasterId=' + $scope.ModelNew.WorkCenterMasterId + '&productionLevel=' + 'ProductionOrder' + '&processId=' + $scope.ModelNew.ProcessId + '&ToCloseAllowed=' + true;
        }
       else if (name == 'PC') {
            path = 'Productions/ProductionSummary/GetProductionOrderDataListWC?entityid=' + $scope.ModelNew.EntityId + '&workCenterMasterId=' + $scope.ModelNew.WorkCenterMasterId + '&productionLevel=' + 'ProductCode' + '&processId=' + $scope.ModelNew.ProcessId + '&ToCloseAllowed=' + true;
        }
        else if (name == 'LI') {
            path = 'Productions/ProductionSummary/GetProductionOrderDataListWC?entityid=' + $scope.ModelNew.EntityId + '&workCenterMasterId=' + $scope.ModelNew.WorkCenterMasterId + '&productionLevel=' + 'LineItem' + '&processId=' + $scope.ModelNew.ProcessId + '&ToCloseAllowed=' + true;
        }
        else if (name == 'SO') {
            path = 'Productions/ProductionSummary/GetProductionOrderDataListWC?entityid=' + $scope.ModelNew.EntityId + '&workCenterMasterId=' + $scope.ModelNew.WorkCenterMasterId + '&productionLevel=' + 'SalesOrder' + '&processId=' + $scope.ModelNew.ProcessId + '&ToCloseAllowed=' + true;
        }
        else {
                
        }
        $http.get(path)
            .then(
                function successCallback(response) {
                    $scope.ProductionOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };


    $scope.SetPrOData = function ($event) {
        $scope.NewObject.ProductionOrderId = $event.data.POId;
        var gridObj = $("#GridEditISP").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');

    }

    $scope.SOItemList = [];
    $scope.getMaterialMasterbyTypePopUp = function (flag) {
        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
            return ShowResult('Please Production Order.', 'failure');
        }
        $scope.SOItemList = [];
        $http.get('Productions/ProductionSummary/GetItemsData?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
            angular.element(document.querySelector('#POItemPopup')).modal('show');
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            angular.element(document.querySelector('#SOItemPopup')).modal('show');
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
            angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
        }
        else {
            angular.element(document.querySelector('#ProductCodePopup')).modal('show');
        }
    };

    $scope.selectSOItem = function ($event) {
        try {
            var soitem = $event.data;
            //if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
            //    $scope.productionSummaryNew.ProductionOrderId = soitem.POId;
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            //    $scope.productionSummaryNew.SalesOrderId = soitem.SOId;
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
            //    $scope.productionSummaryNew.MasterOrderItemId = soitem.MasterOrderItemId;
            //}
            //else {
            //    $scope.productionSummaryNew.ProductLibraryId = soitem.ProductLibraryId;
            //}

            $scope.productionSummaryNew.ProductLibraryId = soitem.ProductLibraryId;
            $scope.productionSummaryNew.ProductCode = soitem.ProductCode;
            $scope.productionSummaryNew.MasterOrderItemId = soitem.MasterOrderItemId;
            $scope.productionSummaryNew.SalesOrderId = soitem.SOId;


            $scope.productionSummaryNew.MaterialMasterId = soitem.MaterialMasterId;
            $scope.productionSummaryNew.MaterialMaster = soitem.MaterialMaster;
            $scope.productionSummaryNew.ArticleId = soitem.ArticleId;
            $scope.productionSummaryNew.Article = soitem.Article;
            $scope.productionSummaryNew.Customer = soitem.Customer;
            $scope.productionSummaryNew.UOM = soitem.UOM;
            $scope.productionSummaryNew.MOQty = soitem.MOQty;
            $scope.productionSummaryNew.ExtraP = soitem.ExtraP;
            $scope.productionSummaryNew.WastageP = soitem.WastageP;
            $scope.productionSummaryNew.MasterOrderNo = soitem.MasterOrderNo;
            $scope.productionSummaryNew.CharCount = soitem.CharCount;
            $scope.productionSummaryNew.PONumber = soitem.PONumber;

            $scope.productionSummaryNew.BuyerOrder = soitem.BuyerOrder;
            $scope.productionSummaryNew.OwnOrder = soitem.OwnOrder;

            $scope.productionSummaryNew.BuyerItem = soitem.BuyerItem;
            $scope.productionSummaryNew.OwnItem = soitem.OwnItem;

            if (!baseService.isUndefinedOrNull(soitem.RemainingQty)) {
                $scope.RemainQty = parseFloat(soitem.RemainingQty.toFixed(2));
            }
            if (!baseService.isUndefinedOrNull(soitem.PlannedQty)) {
                $scope.TotalSalesOrderQty = parseFloat(soitem.PlannedQty.toFixed(2));
            }
            if (!baseService.isUndefinedOrNull(soitem.TotalProductionQty)) {
                $scope.TotalProductionBookingQty = parseFloat(soitem.TotalProductionQty.toFixed(2));
            }


            angular.element(document.querySelector('#SOItemPopup')).modal('hide');
            angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
            angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');


            $scope.GetTotalProductionBookingQty();
            $scope.getLotNumberCbo();
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }




}