'use strict';
ProductionCuttingBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionCuttingBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Production Cutting Booking";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.gradeList = [];
    $scope.path = 'Productions/productionSummary/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveDetailUrl = $scope.path + 'createDetail';
    $scope.saveSecondDetailUrl = $scope.path + 'createSecondDetail';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;
    $scope.gradeList = [
        {
            'Value': 'A',
            'Text': 'A'
        },
        {
            'Value': 'B',
            'Text': 'B'
        },
        {
            'Value': 'C',
            'Text': 'C'
        }
    ];

    $scope.filters = [];
    $scope.SalesOrderStatusloadfilters = function () {
        $http({
            method: 'GET',
            url: 'Productions/productionSummary/getCutFilters?processId=' + $scope.productionSummaryNew.ProcessId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'Customer', width: 20, headerText: "Customer", type: "string" },
                { field: 'ProductionOrderId', width: 20, headerText: "ProductionOrderId", type: "string" },
                { field: 'PONumber', width: 20, headerText: "PONumber", type: "string" },
                { field: 'LotNumber', width: 20, headerText: "LotNumber", type: "string" },
                { field: 'ProductionGrouping', width: 20, headerText: "ProductionGrouping", type: "string" },
                { field: 'OwnReferenceNo', width: 20, headerText: "OwnReferenceNo", type: "string" },
                { field: 'ProductionStatus', width: 20, headerText: "ProductionStatus", type: "string" },
                { field: 'OrderStatusName', width: 20, headerText: "OrderStatusName", type: "string" },
                { field: 'ArticleName', width: 20, headerText: "ArticleName", type: "string" },
                { field: 'SalesOrderId', width: 20, headerText: "SalesOrderId", type: "string" }

            ];
            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });
    }
    $scope.CutPlantList = [];
    $scope.GetCutPlantCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetCutPlantCbo')
                .then(function (response) {
                    $scope.CutPlantList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.GetCutPlantCbo();

    $scope.productionSummary = {
        Id: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        MasterOrderNo: null,
        SalesOrderId: null,
        ProductionOrderId: null,
        MaterialMasterId: null,
        MaterialMaster: null,
        ArticleId: null,
        Article: null,
        WorkCenterMasterId: null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ProductionShiftId: null,
        ProductionGrade: 'A',
        Quantity: 0,
        ScanQty: 0,
        QtyWithoutScan: 0,
        SKUQty: 0,
        UOM: 0,
        MOQty: 0,
        ExtraP: 0,
        WastageP: 0,
        CharCount: 0,
        ProductionBookingLevel: null,
        MentorId: null,
        MentorName: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        InTime: null,
        OutTime: null,
        ConsumeHour: 0,
        ManPower: 0,
        Remarks: null,
        CheckedBy: null,
        CheckedByName: null,
        LotNumber: null,
        IsInventory: false,
        SourceType: 'PB'

    };
    $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

    $scope.ProductionSummaryDetail = {
        Id: null,
        ProductionSummaryId: null,
        FCharId: null,
        SCharId: null,
        TCharId: null,
        Characteristics1Id: null,
        Characteristics1ValueId: null,
        Characteristics2Id: null,
        Characteristics2ValueId: null,
        Characteristics3Id: null,
        Characteristics3ValueId: null,
        Qty: 0
    };

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.productionSummaryNew.EntityId = $scope.entityList[0].Value;
                //default
                $scope.loadProcessList($scope.productionSummaryNew.EntityId);
            }
        });
    }
    $scope.getAllEntities();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityCuttingProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProductionShiftId);
            }
        });
    };

    $scope.LotNumberList = [];
    $scope.disGo = false;
    $scope.PQEnable = true;
    $scope.LotNumberCapture = false;
    $scope.LotNumberMandatory = false;
    $scope.IsSKU1 = false;
    $scope.IsSKU2 = false;
    $scope.IsSKU3 = false;
    $scope.IsFirst = false;
    $scope.IsParameterBased = false;
    $scope.ToCloseAllowed = false;

    $scope.getProdLevel = function () {
        try {
            $scope.PQEnable = false;

            $scope.IsFirst = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsFirst;

            $scope.productionSummaryNew.ProductionBookingLevel = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].ProductionBookingLevel;

            $scope.LotNumberCapture = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].LotNumberCapture;

            $scope.LotNumberMandatory = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].LotNumberMandatory;

            $scope.IsSKU1 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU1;

            $scope.IsSKU2 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU2;

            $scope.IsSKU3 = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsSKU3;

            $scope.IsParameterBased = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].IsParameterBased;

            $scope.ToCloseAllowed = $.grep($scope.processList, function (item) {
                return item.Value === $scope.productionSummaryNew.ProcessId;
            })[0].ToCloseAllowed;

            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.ProductionLevel = 'Production Order';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.ProductionLevel = 'Sales Order';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.ProductionLevel = 'Master Order Item';
                $scope.disGo = false;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductCode') {
                $scope.ProductionLevel = 'Product Code';
                $scope.disGo = false;
            }
            else {
                $scope.disGo = true;
                $scope.PQEnable = true;
                throw 'Production Booking Level is not defined for selected process.';
            }

            if ($scope.IsSKU1 === true || $scope.IsSKU2 === true || $scope.IsSKU2 === true || $scope.IsParameterBased == true) {
                $scope.PQEnable = true;
                $scope.disGo = false;
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.wcList = [];
    $scope.loadWC = function (processid, entityId, shiftId) {
        cboService.GetWCProcessCbo(processid, entityId, shiftId, function (result) {
            $scope.wcList = result;
            //if (baseService.arrayLength(result) === 1) {
            //    $scope.productionSummaryNew.WorkCenterMasterId = $scope.wcList[0].Value;
            //}
        });
        if ($scope.shiftList.length == 0) {
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.ProcessId)) {
                $scope.GetShiftList();
            }
        }
    };

    $scope.productionSummaryNew.NewLotNumber = true;
    $scope.ShowLotNum = false;
    $scope.SetNewLotNumber = function () {
        if ($scope.productionSummaryNew.NewLotNumber) {
            $scope.ShowLotNum = false;
            $scope.productionSummaryNew.LotNumber = null;
        } else {
            $scope.ShowLotNum = true;
        }
    };
    $scope.ShowNew = false;
    $scope.getLotNumberCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetLotNumberCbo?SalesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&ProcessId=' + $scope.productionSummaryNew.ProcessId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
                .then(function (response) {
                    $scope.LotNumberList = response.data;
                    if (baseService.arrayLength($scope.LotNumberList) > 0) {
                        $scope.ShowLotNum = true;
                        $scope.ShowNew = true;
                        $scope.productionSummaryNew.NewLotNumber = false;
                    } else {
                        $scope.ShowLotNum = false;
                        $scope.productionSummaryNew.NewLotNumber = true;
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.CheckValidLotNumber = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                //if (/^[ A-Za-z0-9_@./#&+-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ProdQtyCount = 0;
    $scope.getProdQty = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/Productionsummary/GetTotalProductionQty?wcid=' + $scope.productionSummaryNew.WorkCenterMasterId + '&workdate=' + $scope.productionSummaryNew.ProductionDate)
                .then(function (response) {
                    $scope.ProdQtyCount = 0;
                    if (!baseService.isUndefinedOrNull(response.data[0].TotalProductionQty)) {
                        $scope.ProdQtyCount = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;
    $scope.GetTotalProductionBookingQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;

            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    $scope.productionSummaryNew.ProductionOrderId = $scope.ProductionOrderId;
                }
                $http.get('Productions/Productionsummary/GetTotalPOQty?productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                        }
                    });
            } else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    $scope.productionSummaryNew.SalesOrderId = $scope.SalesOrderId;
                }
                $http.get('Productions/Productionsummary/GetTotalSOQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.productionSummaryNew.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Work Center Master", $scope.productionSummaryNew.WorkCenterMasterId);

            if ($scope.LotNumberCapture && $scope.LotNumberMandatory) {
                CheckField("Lot Number", $scope.productionSummaryNew.LotNumber);
            }

            if ($scope.productionSummaryNew.ProductionBookingLevel === "ProductionOrder") {
                CheckField("Production Order", $scope.productionSummaryNew.ProductionOrderId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === "SalesOrder") {
                CheckField("Sales Order", $scope.productionSummaryNew.SalesOrderId);
                CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
                CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
                CheckField("Article", $scope.productionSummaryNew.ArticleId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === "MasterOrderItem") {
                CheckField("Master Order Item", $scope.productionSummaryNew.MasterOrderItemId);
                CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
                CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
                CheckField("Article", $scope.productionSummaryNew.ArticleId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
            else {
                CheckField("Product Code", $scope.productionSummaryNew.ProductLibraryId);
                CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
                CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
                CheckField("Article", $scope.productionSummaryNew.ArticleId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationPreMaster() {
        try {
            CheckField("Entity", $scope.productionSummaryNew.EntityId);
            CheckField("Process", $scope.productionSummaryNew.ProcessId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
            CheckField("Shift", $scope.productionSummaryNew.ProductionShiftId);
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationDetail(master) {
        try {
            CheckField("Production Summary Id", master.Id);
            CheckField("Sales Order", master.SalesOrderId);
            CheckField("MaterialMaster", master.MaterialMasterId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.IsGo = false;
    $scope.masterGo = function (isdisabled) {
        try {
            ValidationPreMaster();
            $scope.SetGo(isdisabled);
            $scope.getLineGrid();
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SetGo = function (isdisabled) {
        $scope.IsGo = isdisabled;
    };

    $scope.SetBack = function (isdisabled) {
        $scope.IsGo = isdisabled;
        $scope.ClearMasterPart();
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
    };

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

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.ProductionOrderList = [];
        $http.get('Productions/ProductionSummary/GetProductionOrderDataList?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
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
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;
        $scope.productionSummaryNew.BuyerItem = $event.data.BuyerItem;
        $scope.productionSummaryNew.OwnItem = $event.data.OwnItem;
        $scope.productionSummaryNew.BuyerOrder = $event.data.BuyerOrder;
        $scope.productionSummaryNew.OwnOrder = $event.data.OwnOrder;

        $scope.productionSummaryNew.ProductLibraryId = null;
        $scope.productionSummaryNew.ProductCode = null;
        $scope.productionSummaryNew.MasterOrderItemId = null;
        $scope.productionSummaryNew.SalesOrderId = null;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        $scope.GetTotalProductionBookingQty();
        $scope.getLotNumberCbo();
    }

    $scope.psdList = [];
    $scope.char1Save = function () {
        try {
            angular.element(document.querySelector('#firstPopup')).modal('hide');
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.SalesOrderId = null;
    $scope.ProductionOrderId = null;
    $scope.ClearMasterPart = function () {
        $scope.ProductionOrderId = $scope.productionSummaryNew.ProductionOrderId;
        $scope.SalesOrderId = $scope.productionSummaryNew.SalesOrderId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        $scope.LotNumber = $scope.productionSummaryNew.LotNumber;
        $scope.productionSummaryNew.Id = null;
        $scope.productionSummaryNew.SalesOrderId = null;
        $scope.productionSummaryNew.ProductionOrderId = null;
        $scope.productionSummaryNew.MaterialMasterId = null;
        $scope.productionSummaryNew.MaterialMaster = null;
        $scope.productionSummaryNew.ArticleId = null;
        $scope.productionSummaryNew.Article = null;
        $scope.productionSummaryNew.UOM = null;
        $scope.productionSummaryNew.MOQty = null;
        $scope.productionSummaryNew.ExtraP = null;
        $scope.productionSummaryNew.WastageP = null;
        $scope.productionSummaryNew.MasterOrderNo = null;
        $scope.productionSummaryNew.CharCount = null;

        $scope.productionSummaryNew.Quantity = 0;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.SKUQty = 0;
        $scope.productionSummaryNew.Customer = null;
        $scope.productionSummaryNew.ResponsiblePersonId = null;
        $scope.productionSummaryNew.ResponsiblePersonName = null;
        $scope.productionSummaryNew.MentorId = null;
        $scope.productionSummaryNew.MentorName = null;
        $scope.productionSummaryNew.PONumber = null;
        $scope.productionSummaryNew.InTime = null;
        $scope.productionSummaryNew.OutTime = null;
        $scope.productionSummaryNew.ConsumeHour = 0;
        $scope.productionSummaryNew.ManPower = 0;
        $scope.productionSummaryNew.CheckedBy = null;
        $scope.productionSummaryNew.CheckedByName = null;
        $scope.productionSummaryNew.Remarks = null;

        $scope.productionSummaryNew.BuyerOrder = null;
        $scope.productionSummaryNew.OwnOrder = null;
        $scope.productionSummaryNew.BuyerItem = null;
        $scope.productionSummaryNew.OwnItem = null;
        $scope.productionSummaryNew.NewLotNumber = true;
        $scope.ShowLotNum = false;
        $scope.ShowNew = false;
        $scope.productionSummaryNew.ProductionGrade = 'A';
        $scope.productionSummaryNew.ProductionOrderId = $scope.ProductionOrderId;
        $scope.productionSummaryNew.LotNumber = $scope.LotNumber;
    }

    $scope.selectLineItem = function (soitem) {
        try {
            $scope.productionSummaryNew.Id = soitem.Id;
            $scope.productionSummaryNew.SalesOrderId = soitem.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = soitem.ProductionOrderId;
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
            $scope.productionSummaryNew.Quantity = soitem.Quantity;
            $scope.productionSummaryNew.ProductionGrade = soitem.ProductionGrade;
            $scope.productionSummaryNew.LotNumber = soitem.LotNumber;
            $scope.productionSummaryNew.BuyerItem = soitem.BuyerItem;
            $scope.productionSummaryNew.OwnItem = soitem.OwnItem;
            $scope.productionSummaryNew.BuyerOrder = soitem.BuyerOrder;
            $scope.productionSummaryNew.OwnOrder = soitem.OwnOrder;
            angular.element(document.querySelector('#SOItemPopup')).modal('hide');

        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.CharacteristicsValueId = null;
    $scope.characteristicsValueList = [];
    $scope.showFirstPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetcharacteristicsValueList(master.ProductionOrderId);

            angular.element(document.querySelector('#firstPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.showSecondPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetcharacteristicsValueList(master.ProductionOrderId);

            angular.element(document.querySelector('#firstPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.showBothPopup = function (master) {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                    throw "Production Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                    throw "Sales Order is required.";
                }
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.MasterOrderItemId)) {
                    throw "Master Order Item is required.";
                }
            }
            else {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductLibraryId)) {
                    throw "Product Code is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionGrade)) {
                throw "Grade is required.";
            }
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.MasterOrderItemId = master.MasterOrderItemId;
            $scope.productionSummaryNew.ProductLibraryId = master.ProductLibraryId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;

            $scope.GetBothcharacteristicsValueList(master.SalesOrderId);

            angular.element(document.querySelector('#secondPopup')).modal('show');

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    }

    $scope.GetcharacteristicsValueList = function (soId) {
        cboService.getCharacteristicsValueByPrCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getCharInfo();
        });
    }

    $scope.GetBothcharacteristicsValueList = function (soId) {
        cboService.getCharacteristicsValueCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getChar2Info();
        });
    }

    $scope.ProductionSummaryDetail = [];
    $scope.getChar1Info = function () {
        $scope.ProductionSummaryDetail = [];
        $http.get('Productions/Productionsummary/GetChar1Info?id=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.SalesOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.getChar1 = function (masterid, soid) {
        $scope.ProductionSummaryDetail = [];
        $http.get('Productions/Productionsummary/GetChar1Info?id=' + masterid + '&soid=' + soid)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.mentorandresperson = [];
    $scope.getMentorAndRespPersonByWCM = function () {
        $http.get('productions/productionsummary/getmentorandresppersonbywcm?wcmId=' + $scope.productionSummaryNew.WorkCenterMasterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.productionSummaryNew.MentorId = response.data[0].MentorId;
                    $scope.productionSummaryNew.MentorName = response.data[0].MentorName;
                    $scope.productionSummaryNew.ResponsiblePersonId = response.data[0].ResponsiblePersonId;
                    $scope.productionSummaryNew.ResponsiblePersonName = response.data[0].ResponsiblePersonName;
                }
            })
    }

    $scope.getCharInfo = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetChar1InfobyPrO?masterid=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.getChar2Info = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetCharInfoByPrO?masterid=' + $scope.productionSummaryNew.Id + '&workdate=' + $scope.productionSummaryNew.ProductionDate + '&mmid=' + $scope.productionSummaryNew.MaterialMasterId + '&soid=' + $scope.productionSummaryNew.ProductionOrderId + '&artid=' + $scope.productionSummaryNew.ArticleId + '&CharCount=' + $scope.productionSummaryNew.CharCount + '&CharacteristicsValueId=' + $scope.CharacteristicsValueId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.closeCharPopUp = function () {
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    }

    function clearMaster() {
        $scope.productionSummaryNew.Id = null;
        $scope.productionSummaryNew.ProductionGrade = null;
        $scope.productionSummaryNew.Quantity = 0;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.SKUQty = 0;
        $scope.productionSummaryNew.UOM = null;
        //$scope.productionSummaryNew.ProductionHour = null;
        $scope.productionSummaryNew.MOQty = null;
        $scope.productionSummaryNew.ExtraP = null;
        $scope.productionSummaryNew.WastageP = null;
        $scope.productionSummaryNew.CharCount = null;

        $scope.LineGridList = [];
    }

    $scope.SearchLineGridList = [];
    $scope.LineGridList = [];
    $scope.getLineGrid = function () {
        try {
            //$scope.ClearMasterPart();
            var entityid = $scope.productionSummaryNew.EntityId;
            var processid = $scope.productionSummaryNew.ProcessId;
            var workdate = $scope.productionSummaryNew.ProductionDate;
            var shiftid = $scope.productionSummaryNew.ProductionShiftId;
            var wcid = $scope.productionSummaryNew.WorkCenterMasterId;

            $scope.LineGridList = [];
            $http.get('Productions/Productionsummary/GetLineItemGrid?entityid=' + entityid + '&processid=' + processid + '&workdate=' + workdate + '&shiftid=' + shiftid + '&wcid=' + wcid + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
                .then(function (response) {
                    $scope.LineGridList = [];
                    $scope.LineGridList = response.data;
                    if (baseService.arrayLength($scope.SearchLineGridList) === 0) {
                        baseService.getDDLSearchColumn(response.data, $scope.SearchLineGridList);
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.Get = function (id, index) {
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        var ProductionBookingLevel = $scope.productionSummaryNew.ProductionBookingLevel;

        $scope.index = index;
        $scope.productionSummary = $scope.LineGridList[$scope.index];
        $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.ProductionDate = workdate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.WorkCenterMasterId = wcid;
        $scope.productionSummaryNew.ProductionBookingLevel = ProductionBookingLevel;
        $scope.Action = 'Update';
        if ($scope.IsSKU1 == true && $scope.IsSKU2 == false && $scope.IsSKU3 == false) {
            $scope.GetcharacteristicsValueList($scope.productionSummaryNew.ProductionOrderId);
            //$scope.GetcharacteristicsValueList($scope.productionSummaryNew.SalesOrderId);
            //$scope.getChar1($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        if ($scope.IsSKU1 == false && $scope.IsSKU2 == true && $scope.IsSKU3 == false) {
            $scope.GetcharacteristicsValueList($scope.productionSummaryNew.ProductionOrderId);
            //$scope.getChar1($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        if ($scope.IsSKU1 == true && $scope.IsSKU2 == true) {
            $scope.GetBothcharacteristicsValueList($scope.productionSummaryNew.SalesOrderId);
            //$scope.getCharInfo($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        $scope.productionSummaryNew.InTime = new Date($scope.productionSummaryNew.InTime);
        if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.OutTime)) {
            $scope.productionSummaryNew.OutTime = new Date($scope.productionSummaryNew.OutTime);
        } else {
            $scope.productionSummaryNew.OutTime = null;
        }

        $scope.GetTotalProductionBookingQty();

        $scope.getLotNumberCbo();
    };

    // #region Employee Mentor

    $scope.employeeFilterList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'EmployeeCode, FirstName, MiddleName, LastName '
        , searchBy: 'EmployeeCode'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showEmployeeListPopUp = function (flag) {
        $scope.respOrMentor = flag;
        if ($scope.respOrMentor === 'ResponsiblePerson') { $scope.popUpTitle = 'Responsible Person'; }
        else if ($scope.respOrMentor === 'Mentor') { $scope.popUpTitle = 'Mentor'; }
        else if ($scope.respOrMentor === 'CheckedBy') { $scope.popUpTitle = 'CheckedBy'; }
        baseService.setCurrentPage('employeeList');
        $scope.searchEmployeeByList = [];
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.plantId = $window.plantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.employeeUrl = 'WorkCenters/workcentermaster/GetEmployeeListByPlant';

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.respOrMentor === 'ResponsiblePerson') {
                $scope.productionSummaryNew.ResponsiblePersonId = employee.SystemId;
                $scope.productionSummaryNew.ResponsiblePersonName = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'Mentor') {
                $scope.productionSummaryNew.MentorId = employee.SystemId;
                $scope.productionSummaryNew.MentorName = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'CheckedBy') {
                $scope.productionSummaryNew.CheckedBy = employee.SystemId;
                $scope.productionSummaryNew.CheckedByName = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
    };

    $scope.ClearEmployee = function () {
        if ($scope.respOrMentor === 'ResponsiblePerson') {
            $scope.productionSummaryNew.ResponsiblePersonId = null;
            $scope.productionSummaryNew.ResponsiblePersonName = null;
        }
        else if ($scope.respOrMentor === 'Mentor') {
            $scope.productionSummaryNew.MentorId = null;
            $scope.productionSummaryNew.MentorName = null;
        }
        else if ($scope.respOrMentor === 'CheckedBy') {
            $scope.productionSummaryNew.CheckedBy = null;
            $scope.productionSummaryNew.CheckedByName = null;
        }
    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    // #endregion Employee Mentor

    $scope.countProductQty = function () {
        $scope.ProdQty = 0;
        for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
            }
        }
        $scope.productionSummaryNew.Quantity = $scope.ProdQty;
    }

    $scope.SaveMaster = function () {
        try {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }

            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.MasterOrderItemId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.SalesOrderId = null;
                $scope.productionSummaryNew.ProductLibraryId = null;
            }
            else {
                $scope.productionSummaryNew.SalesOrderId = null;
            }

            if (new Date($scope.productionSummaryNew.ProductionDate) > new Date()) {
                throw "Future Date not allowed for Production Booking.";
            }
            $scope.productionSummaryNew.Quantity = $scope.productionSummaryNew.QtyWithoutScan;
            CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            ValidationMaster();
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
            $scope.ProdQty = 0;

            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                for (var i = 0; i < $scope.ProductionSummaryDetail.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ProductionSummaryDetail[i].Qty)) {
                        $scope.ProdQty = $scope.ProdQty + $scope.ProductionSummaryDetail[i].Qty;
                    }
                }
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
                $scope.productionSummaryNew.QtyWithoutScan = $scope.ProdQty;
                $scope.productionSummaryNew.SKUQty = $scope.ProdQty;
                $scope.productionSummaryNew.SourceType = "SKU";
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SourceType)) {
                $scope.productionSummaryNew.SourceType = "PB";
            }

            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            if ($scope.IsFirst == false) {
                if (parseFloat($scope.RemainQty) < 0) {
                    throw "Order Quantity dosen't available.";
                }
            }

            if ($scope.IsFirst == false) {
                if (parseFloat($scope.TotalSalesOrderQty) < parseFloat($scope.TotalProductionBookingQty) + parseFloat($scope.productionSummaryNew.Quantity)) {
                    throw "Produced Quantity should less than Order Quantity.";
                }
            }

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    "ps": $scope.productionSummaryNew,
                    "psd": $scope.ProductionSummaryDetail,
                    "ProcessParaList": $scope.ProcessParaList,
                    "ProcessId": $scope.productionSummaryNew.ProcessId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.getLineGrid();
                    $scope.Action = 'Save';
                    $scope.getProdQty();
                    $scope.closeCharPopUp();
                    $scope.GetTotalProductionBookingQty();
                    $scope.ClearMasterPart();
                    angular.element(document.querySelector('#ProcessParaPopup')).modal('hide');

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.charSave = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveSecondDetailUrl,
                data: {
                    "psd": $scope.ProductionSummaryDetail,
                    "productionSummary": $scope.productionSummaryNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'secondPopup');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'secondPopup');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'secondPopup');
            };
        } catch (e) {
            ShowResult(e, 'failure', 'secondPopup');
        }
    };

    $scope.SaveDetail = function () {
        $http({
            method: 'POST',
            url: $scope.saveDetailUrl,
            data: {
                "psid": $scope.productionSummaryNew.Id,
                "psd": $scope.ProductionSummaryDetail
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.productionSummaryes.push(response.data.ProductionSummary);
                //$scope.productionSummaryes = $filter('orderBy')($scope.productionSummaryes, 'PlanningGroupPriority');
                //baseService.paginationAdd();
                //ClearFields(response.data.PlanningGroupPriority);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

        //}
    };

    $scope.deleteMaster = function (master) {
        if (!baseService.isUndefinedOrNull(master.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + master.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getLineGrid();
                    $scope.getProdQty();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult("Production Summary not found...", 'Info');
        }
    }

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }
    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderOfProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsForProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId.data.POId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }

    $scope.getSalesOrderByProdOrderList = function (prodOrdId) {
        $scope.openPopup('dialogSOItemsFromProductionOrder');
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }
    //search

    $scope.ProcessParaList = [];
    $scope.getProcessParaPopupPoPUp = function () {
        try {
            ValidationMaster();
            $scope.ProcessParaList = [];
            $http.get('Productions/ProductionSummary/GetProcessParaData?processId=' + $scope.productionSummaryNew.ProcessId + '&masterId=' + $scope.productionSummaryNew.Id + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
                .then(
                    function successCallback(response) {
                        $scope.ProcessParaList = response.data;
                        for (var i = 0; i < $scope.ProcessParaList.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ProcessParaList[i].Id) && $scope.ProcessParaList[i].IsProduction==true) {
                                $scope.ProcessParaList[i].Value = 0;
                            }
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });

            angular.element(document.querySelector('#ProcessParaPopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.isCalculated = true;
    $scope.isChangeCalValue = true;

    $scope.ChangeCalValue = function () {
        $scope.isChangeCalValue = false;
        $scope.isCalculated = true;
    }

    $scope.Calculate = function () {
        try {
            $scope.productionSummaryNew.QtyWithoutScan = 0;
            $scope.productionSummaryNew.Quantity = 0;
            $http({
                method: 'POST',
                url: 'Productions/ProductionSummary/Calculate',
                data: { 'OpenHeadNew': $scope.ProcessParaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.isCalculated = false;
                for (var i = 0; i < response.data.NewData.length; i++) {
                    for (var j = 0; j < $scope.ProcessParaList.length; j++) {
                        if (response.data.NewData[i].UserName == $scope.ProcessParaList[j].UserName) {
                            $scope.ProcessParaList[j].Value = response.data.NewData[i].Value;
                        }
                    }
                    if (response.data.NewData[i].IsProduction == true) {
                        $scope.productionSummaryNew.Quantity = response.data.NewData[i].Value;
                        $scope.productionSummaryNew.QtyWithoutScan = response.data.NewData[i].Value;
                    }
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.isCalculated = true;
        $scope.isChangeCalValue = true;
        $scope.Action = "Save";
        $scope.productionSummary = {};
        $scope.productionSummaryNew = {};
        $scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionGrade = 'A';
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');
        $scope.ProdQtyCount = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;
        $scope.SetBack(false);
        $scope.IsGo = false;
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
    }
}