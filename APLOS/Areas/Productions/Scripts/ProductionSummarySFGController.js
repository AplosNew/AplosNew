'use strict';
ProductionSummarySFGController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionSummarySFGController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Production Booking";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.gradeList = [];
    $scope.path = 'Productions/productionSummary/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateSFG';
    $scope.saveInOutUrl = $scope.path + 'createinout';
    $scope.saveDetailUrl = $scope.path + 'createDetail';
    $scope.saveSecondDetailUrl = $scope.path + 'createSecondDetail';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'DeleteSFG/';
    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;
    $scope.FromEntity = null;
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
        ProductionGrade: null,
        Quantity: 0,
        QtyWithoutScan: 0,
        SKUQty: 0,
        ScanQty: 0,
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
        FromId: null,
        ToWorkCenterMasterId: null, FromSFGInventoryId: null, ToSFGInventoryId: null, ToProcessId: null,
        LotNumber: null,
        ToEntityId: null,
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
    };
    $scope.getAllEntities();

    $scope.loadProcessList = function (entityid) {
        $scope.listToProcessOrSFGInventory = [];
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;

                //default
                $scope.loadWC($scope.productionSummaryNew.ProcessId);
            }
        });
    };

    $scope.productTimeList = [];
    cboService.getProductionBookingPeriodCbo(function (result) {
        $scope.productTimeList = result;
    });

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

    $scope.ProdQtyCount = 0;
    $scope.getProdQty = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/Productionsummary/GetTotalProductionQty?wcid=' + $scope.productionSummaryNew.WorkCenterMasterId + '&workdate=' + $scope.productionSummaryNew.ProductionDate)
                .then(function (response) {
                    $scope.ProdQtyCount = 0;
                    if (!baseService.isUndefinedOrNull(response.data[0].TotalProductionQty)) {
                        $scope.ProdQtyCount = response.data[0].TotalProductionQty;
                        $scope.TotalProductionBookingQty = response.data[0].TotalProductionQty.toFixed(0);
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.IsProductionHourOpen = false;
    $scope.GetIsProductionHourOpen = function () {
        try {
            $http.get('Productions/Productionsummary/GetIsProductionHourOpen')
                .then(function (response) {
                    $scope.IsProductionHourOpen = response.data[0].IsProductionHourOpen;
                })
        } catch (e) {
            ShowResult(e, 'failure')
        }
    };
    $scope.GetIsProductionHourOpen();

    $scope.ProductionBookingPeriodList = [];
    $scope.GetProductionBookingPeriodCbo = function () {
        try {
            $http.get('Productions/Productionsummary/GetProductionBookingPeriodCbo')
                .then(function (response) {
                    $scope.ProductionBookingPeriodList = response.data;
                })
        } catch (e) {
            ShowResult(e, 'failure')
        }
    };
    $scope.GetProductionBookingPeriodCbo();

    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;

    $scope.InQuantity = 0;
    $scope.OutQuantity = 0;
    $scope.KillQuantity = 0;

    $scope.GetSFGWIPQty = function () {

        $scope.InQuantity = 0;
        $scope.OutQuantity = 0;
        $scope.KillQuantity = 0;
        //if ($scope.Status === 'PROCESS') {
        //    $scope.ProcessId = $scope.productionSummaryNew.ProcessId;
        //} else {
        //    $scope.ProcessId = $scope.productionSummaryNew.ToProcessId;
        //}


        $http({
            method: 'GET',
            url: 'Productions/ProductionSummary/GetSFGWIPQty?EntityId=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&status=' + $scope.Status + '&IsCrossAllowed=' + $scope.IsCrossAllowed,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if (baseService.arrayLength(response.data) > 0) {
                    if (!baseService.isUndefinedOrNull(response.data[0].InQuantity)) {
                        $scope.InQuantity = parseFloat(response.data[0].InQuantity).toFixed(0);
                    }
                    if (!baseService.isUndefinedOrNull(response.data[0].OutQuantity)) {
                        $scope.OutQuantity = parseFloat(response.data[0].OutQuantity).toFixed(0);
                    }
                    if (!baseService.isUndefinedOrNull(response.data[0].WIP)) {
                        $scope.KillQuantity = parseFloat(response.data[0].WIP).toFixed(0);
                    }
                }
                else {
                    $scope.InQuantity = 0;
                    $scope.OutQuantity = 0;
                    $scope.KillQuantity = 0;
                }

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    //$scope.shiftList = [];
    //cboService.GetProductionShiftCbo(function (result) {
    //    $scope.shiftList = result;
    //    if (baseService.arrayLength(result) === 1) {
    //        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
    //    }
    //});

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        if ($scope.Status === 'PROCESS') {
            $scope.ProcessId = $scope.productionSummaryNew.ProcessId;
        } else {
            $scope.ProcessId = $scope.productionSummaryNew.ToProcessId;
        }
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.ProcessId)
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
                throw "[" + fieldname + "] can not be blank...";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            if ($scope.Status === 'PROCESS') {
                CheckField("From Work Center", $scope.productionSummaryNew.WorkCenterMasterId);
            }
            if ($scope.ToStatus === 'PROCESS') {
                CheckField("To", $scope.productionSummaryNew.ToProcessId);
                CheckField("To Work Center", $scope.productionSummaryNew.ToWorkCenterMasterId);
            }
            if ($scope.ToStatus === 'INVENTORY') {
                CheckField("To", $scope.productionSummaryNew.ToSFGInventoryId);
            }
            if ($scope.LotNumberCapture && $scope.LotNumberMandatory) {
                CheckField("Lot Number", $scope.productionSummaryNew.LotNumber);
            }
            if ($scope.productionSummaryNew.ProductionBookingLevel === "ProductionOrder") {
                CheckField("Production Order", $scope.productionSummaryNew.ProductionOrderId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            } else {
                CheckField("Sales Order", $scope.productionSummaryNew.SalesOrderId);
                CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
                CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
                CheckField("Article", $scope.productionSummaryNew.ArticleId);
                CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
                CheckField("Quantity", $scope.productionSummaryNew.Quantity);
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

        $scope.ProdQtyCount = 0;
        $scope.InQuantity = 0;
        $scope.OutQuantity = 0;
        $scope.KillQuantity = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;
        $scope.GetIsProductionHourOpen();
    };

    $scope.ToEntitydisable = true;
    $scope.ToProdEntity = false;
    $scope.EnableDiffEntity = function () {
        if ($scope.ToProdEntity == true) {
            $scope.ToEntitydisable = false;
        } else {
            $scope.ToEntitydisable = true;
        }
    }

    //#region SFG Movement
    $scope.listFromProcessOrSFGInventory = [];
    $scope.GetSFGMovementFromCbo = function (entity) {
        $scope.productionSummaryNew.ToEntityId = $scope.productionSummaryNew.EntityId;

        $http({
            method: 'GET',
            url: 'Productions/ProductionSummary/GetSFGMovementFromCbo?entity=' + entity,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //sccuess 
                $scope.listFromProcessOrSFGInventory = response.data;

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.wcList = [];
    $scope.loadWC = function (processid) {
        $scope.wcList = [];
        if ($scope.Status === 'PROCESS') {
            //cboService.GetWCProcessCbo(processid, $scope.productionSummaryNew.EntityId, function (result) {
            //    $scope.wcList = result;

            //});
            cboService.GetWCProcessCbo(processid, $scope.productionSummaryNew.EntityId, $scope.productionSummaryNew.ProductionShiftId, function (result) {
                $scope.wcList = result;
            });

        }
    };

    $scope.Status = null;
    $scope.Sequence = 0;
    $scope.SeqProcess = null;
    $scope.IsCrossAllowed = null;
    $scope.LotNumberList = [];
    $scope.disGo = false;
    $scope.PQEnable = true;
    $scope.LotNumberCapture = false;
    $scope.LotNumberMandatory = false;
    $scope.IsSKU1 = false;
    $scope.IsSKU2 = false;
    $scope.IsSKU3 = false;

    $scope.changeProcess = function () {
        $scope.Process = $("#Process option:selected").text();
        $scope.Status = null;

        for (var i = 0; i < $scope.listFromProcessOrSFGInventory.length; i++) {
            if ($scope.productionSummaryNew.ProcessId === $scope.listFromProcessOrSFGInventory[i].FromId) {
                $scope.productionSummaryNew.ProductionBookingLevel = $scope.listFromProcessOrSFGInventory[i].ProductionBookingLevel;
                $scope.LotNumberCapture = $scope.listFromProcessOrSFGInventory[i].LotNumberCapture;
                $scope.LotNumberMandatory = $scope.listFromProcessOrSFGInventory[i].LotNumberMandatory;
                $scope.IsFirst = $scope.listFromProcessOrSFGInventory[i].IsFirst;
                $scope.Status = $scope.listFromProcessOrSFGInventory[i].Status;
                $scope.IsCrossAllowed = $scope.listFromProcessOrSFGInventory[i].IsCrossAllowed;
                $scope.IsSKU1 = $scope.listFromProcessOrSFGInventory[i].IsSKU1;
                $scope.IsSKU2 = $scope.listFromProcessOrSFGInventory[i].IsSKU2;
                $scope.IsSKU3 = $scope.listFromProcessOrSFGInventory[i].IsSKU3;
                $scope.Sequence = $scope.listFromProcessOrSFGInventory[i].Sequence - 1;
                break;
            }
        }

        if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
            $scope.ProductionLevel = 'Production Order';
            $scope.PQEnable = false;
            $scope.disGo = false;
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            $scope.ProductionLevel = 'Sales Order';
            $scope.PQEnable = false;
            $scope.disGo = false;
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem') {
            $scope.ProductionLevel = 'Master Order Item';
            $scope.PQEnable = false;
            $scope.disGo = false;
        }
        else if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductCode') {
            $scope.ProductionLevel = 'Product Code';
            $scope.PQEnable = false;
            $scope.disGo = false;
        }
        else {
            $scope.disGo = true;
            $scope.PQEnable = true;
            ShowResult('Production Booking Level is not defined for selected process.', 'failure');
        }

        if ($scope.IsSKU1 === true || $scope.IsSKU2 === true || $scope.IsSKU2 === true) {
            $scope.PQEnable = true;
            $scope.disGo = false;
        }

        $scope.loadWC($scope.productionSummaryNew.ProcessId);
        $scope.GetSFGMovementToCbo();
    };

    $scope.Process = null;
    $scope.ToStatus = null;
    $scope.listToProcessOrSFGInventory = [];
    $scope.GetSFGMovementToCbo = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProductionSummary/GetSFGMovementToCbo?FromId=' + $scope.productionSummaryNew.ProcessId + "&flag=" + $scope.Status + "&EntityId=" + $scope.productionSummaryNew.ToEntityId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.listToProcessOrSFGInventory = response.data;

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.wcToProcessList = [];

    $scope.ToProcessStatus = function () {
        $scope.ToStatus = null;

        for (var i = 0; i < $scope.listToProcessOrSFGInventory.length; i++) {
            if ($scope.productionSummaryNew.ToProcessId === $scope.listToProcessOrSFGInventory[i].ToId) {
                $scope.ToStatus = $scope.listToProcessOrSFGInventory[i].Status;
                break;
            }
        }

        $scope.wcToProcessList = [];
        if ($scope.ToStatus === 'PROCESS') {
            cboService.GetToWCProcessCbo($scope.productionSummaryNew.ToProcessId, $scope.productionSummaryNew.ToEntityId, function (result) {
                $scope.wcToProcessList = result;
            });
        }
        if ($scope.shiftList.length == 0) {
            $scope.GetShiftList();
        }
    };


    //#endregion SFG Movement

    $scope.SOItemList = [];
    $scope.getMaterialMasterbyTypePopUp = function (flag) {
        $scope.ProdQtyCount = 0;
        $scope.InQuantity = 0;
        $scope.OutQuantity = 0;
        $scope.KillQuantity = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;

        $scope.ProcessId = null;

        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
            if ($scope.Status === 'PROCESS')
                return ShowResult('Please select Work Center.', 'failure');
        }

        $scope.WorkCenterMasterId = $scope.productionSummaryNew.WorkCenterMasterId;
        if (baseService.isUndefinedOrNull($scope.WorkCenterMasterId)) {
            $scope.WorkCenterMasterId = $scope.productionSummaryNew.ToWorkCenterMasterId;
        }

        if ($scope.Status === 'PROCESS') {
            $scope.ProcessId = $scope.productionSummaryNew.ProcessId;
        } else {
            $scope.ProcessId = $scope.productionSummaryNew.ToProcessId;
        }

        $scope.SOItemList = [];

        $http.get('Productions/ProductionSummary/GetSFGSOItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.ProcessId + '&status=' + $scope.Status + '&IsFirst=' + $scope.IsFirst + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(
                function successCallback(response) {
                    $scope.SOItemList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
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

            $scope.productionSummaryNew.ProductionOrderId = soitem.POId;
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
            $scope.GetSFGWIPQty();
            $scope.getLotNumberCbo();
            $scope.GetEntityProcessOrderTotalQty();
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.ProductOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        $scope.ProdQtyCount = 0;
        $scope.InQuantity = 0;
        $scope.OutQuantity = 0;
        $scope.KillQuantity = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;

        $scope.ProcessId = null;

        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
            if ($scope.Status === 'PROCESS')
                return ShowResult('Please select Work Center.', 'failure');
        }

        $scope.WorkCenterMasterId = $scope.productionSummaryNew.WorkCenterMasterId;
        if (baseService.isUndefinedOrNull($scope.WorkCenterMasterId)) {
            $scope.WorkCenterMasterId = $scope.productionSummaryNew.ToWorkCenterMasterId;
        }


        $scope.ProductOrderList = [];
        $http.get('Productions/ProductionSummary/GetProductionOrderData?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&status=' + $scope.Status)
            .then(
                function successCallback(response) {
                    $scope.ProductOrderList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

        angular.element(document.querySelector('#POItemPopup')).modal('show');

    };

    $scope.SetPrOData = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.POId;

        $scope.productionSummaryNew.BuyerOrder = $event.data.BuyerOrder;
        $scope.productionSummaryNew.OwnOrder = $event.data.OwnOrder;

        $scope.productionSummaryNew.BuyerItem = $event.data.BuyerItem;
        $scope.productionSummaryNew.OwnItem = $event.data.OwnItem;


        $scope.productionSummaryNew.ProductLibraryId = null;
        $scope.productionSummaryNew.ProductCode = null;
        $scope.productionSummaryNew.MasterOrderItemId = null;
        $scope.productionSummaryNew.SalesOrderId = null;

        angular.element(document.querySelector('#POItemPopup')).modal('hide');
        $scope.GetTotalProductionBookingQty();
        $scope.GetSFGWIPQty();
        $scope.getLotNumberCbo();
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
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }


    $scope.EntityProcessOrderQty = 0;
    $scope.GetEntityProcessOrderTotalQty = function () {

        $http.get('Productions/Productionsummary/GetEntityProcessOrderTotalQty?EntityId=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&status=' + $scope.Status)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.EntityProcessOrderQty = response.data[0].EntityProcessOrderQty;
                }
            });
    };

    $scope.psdList = [];
    $scope.char1Save = function () {
        try {
            //console.log('d',$scope.ProductionSummaryDetail);
            //$scope.psdList = [];
            //for (var i = 0; i < bankService.arrayLength($scope.firstSKUList); i++) {
            //   // $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);
            //    var ob = Object.assign({}, $scope.ProductionSummaryDetail);
            //    ob.Characteristics1Qty = $scope.firstSKUList[i].Characteristics1Qty;
            //    ob.FCharId = $scope.firstSKUList[i].FCharId;
            //    ob.Characteristics1Id = $scope.firstSKUList[i].Characteristics1Id;
            //    ob.Characteristics1ValueId = $scope.firstSKUList[i].Characteristics1ValueId;
            //    $scope.psdList.push($scope.ProductionSummaryDetail);
            //}
            angular.element(document.querySelector('#firstPopup')).modal('hide');
        } catch (ex) {
            ShowResult(ex, 'error');
        }
    };

    $scope.ClearMasterPart = function () {
        $scope.ProductionOrderId = $scope.productionSummaryNew.ProductionOrderId;
        $scope.SalesOrderId = $scope.productionSummaryNew.SalesOrderId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
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
        $scope.productionSummaryNew.ProductionGrade = null;

        $scope.productionSummaryNew.Quantity = 0;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.SKUQty = 0;
        $scope.productionSummaryNew.ScanQty = 0;
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
        $scope.productionSummaryNew.LotNumber = null;

        $scope.productionSummaryNew.BuyerOrder = null;
        $scope.productionSummaryNew.OwnOrder = null;
        $scope.productionSummaryNew.BuyerItem = null;
        $scope.productionSummaryNew.OwnItem = null;

        $scope.productionSummaryNew.NewLotNumber = true;
        $scope.ShowLotNum = false;
        $scope.ShowNew = false;
       

    };

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
    };

    $scope.CharacteristicsValueId = null;
    $scope.characteristicsValueList = [];
    $scope.XshowFirstPopup = function (master) {
        try {
            $scope.productionSummaryNew.Id = master.Id;
            $scope.productionSummaryNew.MaterialMasterId = master.MaterialMasterId;
            $scope.productionSummaryNew.SalesOrderId = master.SalesOrderId;
            $scope.productionSummaryNew.ProductionOrderId = master.ProductionOrderId;
            $scope.productionSummaryNew.ArticleId = master.ArticleId;
            $scope.productionSummaryNew.CharCount = master.CharCount;
            //ValidationDetail(master);

            $scope.GetcharacteristicsValueList(master.SalesOrderId);

            //cboService.getCharacteristicsValueCbo(, function (result) {
            //    $scope.characteristicsValueList = result;
            //});
            //if ($scope.productionSummaryNew.CharCount === 1) {
            if ($scope.productionSummaryNew.ProductionBookingLevel === 'UptoSKU1') {
                angular.element(document.querySelector('#firstPopup')).modal('show');
                angular.element(document.querySelector('#secondPopup')).modal('hide');
                angular.element(document.querySelector('#thirdPopup')).modal('hide');
            }
            //else if ($scope.productionSummaryNew.CharCount === 2) {
            else if ($scope.productionSummaryNew.ProductionBookingLevel === 'UptoSKU2') {
                angular.element(document.querySelector('#firstPopup')).modal('hide');
                angular.element(document.querySelector('#secondPopup')).modal('show');
                angular.element(document.querySelector('#thirdPopup')).modal('hide');
            }
            //$scope.getCharInfo(master.CharCount, master.Id, $scope.productionSummaryNew.ProductionDate, master.MaterialMasterId, master.SalesOrderId, master.ArticleId);

        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

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

            $scope.GetcharacteristicsValueList(master.SalesOrderId);

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

            $scope.GetcharacteristicsValueList(master.SalesOrderId);

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
        cboService.getCharacteristicsValueCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getCharInfo();
        });
    };

    $scope.GetBothcharacteristicsValueList = function (soId) {
        cboService.getCharacteristicsValueCbo(soId, function (result) {
            $scope.characteristicsValueList = result;
            if (baseService.arrayLength($scope.characteristicsValueList) > 0) {
                $scope.CharacteristicsValueId = $scope.characteristicsValueList[0].Value;
            }
            $scope.getChar2Info();
        });
    }

    $scope.getCharInfo = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetChar1Info?masterid=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.SalesOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.getChar2Info = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/Productionsummary/GetCharInfo?masterid=' + $scope.productionSummaryNew.Id + '&workdate=' + $scope.productionSummaryNew.ProductionDate + '&mmid=' + $scope.productionSummaryNew.MaterialMasterId + '&soid=' + $scope.productionSummaryNew.SalesOrderId + '&artid=' + $scope.productionSummaryNew.ArticleId + '&CharCount=' + $scope.productionSummaryNew.CharCount + '&CharacteristicsValueId=' + $scope.CharacteristicsValueId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

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
    };

    //$scope.getCharInfo = function () {

    //    $scope.ProductionSummaryDetail = [];
    //    if ($scope.productionSummaryNew.ProductionBookingLevel === 'UptoSKU1') {
    //        $http.get('Productions/Productionsummary/GetChar1Info?masterid=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.SalesOrderId)
    //            .then(function (response) {
    //                $scope.ProductionSummaryDetail = [];
    //                $scope.ProductionSummaryDetail = response.data;
    //            });
    //    }
    //    else {
    //        $http.get('Productions/Productionsummary/GetCharInfo?masterid=' + $scope.productionSummaryNew.Id + '&workdate=' + $scope.productionSummaryNew.ProductionDate + '&mmid=' + $scope.productionSummaryNew.MaterialMasterId + '&soid=' + $scope.productionSummaryNew.SalesOrderId + '&artid=' + $scope.productionSummaryNew.ArticleId + '&CharCount=' + $scope.productionSummaryNew.CharCount + '&CharacteristicsValueId=' + $scope.CharacteristicsValueId)
    //            .then(function (response) {
    //                $scope.ProductionSummaryDetail = [];
    //                $scope.ProductionSummaryDetail = response.data;
    //            });
    //    }

    //    //}//CharCount 2
    //};

    $scope.closeCharPopUp = function () {
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    };

    function clearMaster() {
        $scope.productionSummaryNew.Id = null;
        $scope.productionSummaryNew.ProductionGrade = null;
        $scope.productionSummaryNew.Quantity = 0;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.SKUQty = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.UOM = null;
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
            $scope.ClearMasterPart();
            var processid = null;
            var wcid = null;
            var entityid = $scope.productionSummaryNew.EntityId;

            processid = $scope.productionSummaryNew.ProcessId;
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.WorkCenterMasterId)) {
                wcid = $scope.productionSummaryNew.ToWorkCenterMasterId;
            }
            else {
                wcid = $scope.productionSummaryNew.WorkCenterMasterId;
            }

            var workdate = $scope.productionSummaryNew.ProductionDate;
            var shiftid = $scope.productionSummaryNew.ProductionShiftId;


            $scope.LineGridList = [];

            $http.get('Productions/Productionsummary/GetLineItemGridSFG?entityid=' + entityid + '&processid=' + processid + '&workdate=' + workdate + '&shiftid=' + shiftid + '&wcid=' + wcid + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&status=' + $scope.Status)
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


    $scope.Get = function (obj) {
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        //var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        var ProductionBookingLevel = $scope.productionSummaryNew.ProductionBookingLevel;

        $scope.productionSummary = obj.data;
        $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);


        if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ToProcessId) && !baseService.isUndefinedOrNull($scope.productionSummaryNew.ToSFGInventoryId)) {
            $scope.productionSummaryNew.ToProcessId = $scope.productionSummaryNew.ToSFGInventoryId;
        }

        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.ProductionDate = workdate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        // $scope.productionSummaryNew.WorkCenterMasterId = wcid;
        $scope.productionSummaryNew.ProductionBookingLevel = ProductionBookingLevel;
        $scope.Action = 'Update';
        if ($scope.productionSummaryNew.ProductionBookingLevel === 'UptoSKU1') {
            $scope.getChar1($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        if ($scope.productionSummaryNew.ProductionBookingLevel === 'UptoSKU2') {
            $scope.getCharInfo($scope.productionSummaryNew.Id, $scope.productionSummaryNew.SalesOrderId);
        }
        $scope.productionSummaryNew.InTime = new Date($scope.productionSummaryNew.InTime);
        if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.OutTime)) {
            $scope.productionSummaryNew.OutTime = new Date($scope.productionSummaryNew.OutTime);
        }
        else {
            $scope.productionSummaryNew.OutTime = null;
        }
        $scope.productionSummaryNew.WorkCenterMasterId = $scope.productionSummary.WorkCenterMasterId;
        $scope.productionSummaryNew.ToWorkCenterMasterId = $scope.productionSummary.ToWorkCenterMasterId;
        $scope.GetTotalProductionBookingQty();/// get Order Qty  Produced Qty  Balance Qty
        $scope.GetSFGWIPQty();
        //$scope.GetWIPQtyForValidation();
        $scope.GetcharacteristicsValueList($scope.productionSummaryNew.SalesOrderId);
        // $scope.getProdQty();

        $scope.ToProcessStatus();

        if ($scope.Status === 'INVENTORY') {
            $scope.productionSummaryNew.FromSFGInventoryId = $scope.productionSummaryNew.ProcessId;
        }

        if ($scope.ToStatus === 'INVENTORY') {
            $scope.productionSummaryNew.ToProcessId = $scope.productionSummaryNew.ToSFGInventoryId;
            //$scope.productionSummaryNew.ToSFGInventoryId = $scope.productionSummaryNew.ToProcessId;
        }
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

    $scope.InQ = 0;
    $scope.OutQ = 0;

    $scope.GetWIPQtyForValidation = function () {
        $http({
            method: 'GET',
            url: 'Productions/ProductionSummary/GetWIPQtyForValidation?Id=' + $scope.productionSummaryNew.Id + '&EntityId=' + $scope.productionSummaryNew.EntityId + '&processId=' + $scope.productionSummaryNew.ProcessId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&status=' + $scope.Status + '&IsCrossAllowed=' + $scope.IsCrossAllowed
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                if (baseService.arrayLength(response.data) > 0) {
                    if (!baseService.isUndefinedOrNull(response.data[0].InQuantity)) {
                        $scope.InQ = parseFloat(response.data[0].InQuantity).toFixed(0);
                    }
                    if (!baseService.isUndefinedOrNull(response.data[0].OutQuantity)) {
                        $scope.OutQ = parseFloat(response.data[0].OutQuantity).toFixed(0);
                    }
                }
                else {
                    $scope.InQ = 0;
                    $scope.OutQ = 0;
                }
            }
        });
    };

    $scope.GetTotalProductionBookingQty = function () {
        try {

            $scope.ProcessId = null;

            if ($scope.Status === 'PROCESS') {

                $scope.ProcessId = $scope.productionSummaryNew.ProcessId;
                if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                        $scope.productionSummaryNew.ProductionOrderId = $scope.ProductionOrderId;
                    }
                    $http.get('Productions/Productionsummary/GetSFGTotalPOQty?productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&processId=' + $scope.ProcessId + '&status=' + $scope.Status)
                        .then(function (response) {

                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            }
                        });
                }
                else {
                    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                        $scope.productionSummaryNew.SalesOrderId = $scope.SalesOrderId;
                    }
                    $http.get('Productions/Productionsummary/GetSFGTotalQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.ProcessId + '&status=' + $scope.Status)
                        .then(function (response) {

                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            }

                        });
                }

            } else {
                $scope.ProcessId = $scope.productionSummaryNew.ProcessId;

                if ($scope.productionSummaryNew.ProductionBookingLevel === 'ProductionOrder') {
                    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProductionOrderId)) {
                        $scope.productionSummaryNew.ProductionOrderId = $scope.ProductionOrderId;
                    }
                    $http.get('Productions/Productionsummary/GetSFGTotalPOQty?productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&processId=' + $scope.ProcessId + '&status=' + $scope.Status)
                        .then(function (response) {

                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            }
                        });
                }
                else {
                    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
                        $scope.productionSummaryNew.SalesOrderId = $scope.SalesOrderId;
                    }
                    $http.get('Productions/Productionsummary/GetSFGTotalQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.ProcessId + '&status=' + $scope.Status)
                        .then(function (response) {

                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            }

                        });
                }


            }


        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveMaster = function () {
        try {

            if (new Date($scope.productionSummaryNew.ProductionDate) > new Date()) {
                throw "Future Date not allowed for Production Booking.";
            }

            if ($scope.Status === 'INVENTORY') {
                $scope.productionSummaryNew.FromSFGInventoryId = $scope.productionSummaryNew.ProcessId;
                $scope.productionSummaryNew.ProcessId = null;
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.FromSFGInventoryId)) {
                    throw "Process is required.";
                }
            }


            if ($scope.Status === 'PROCESS') {
                $scope.productionSummaryNew.FromSFGInventoryId = null;
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ProcessId)) {
                    throw "Process is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ToProcessId)) {
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ToSFGInventoryId)) {
                    throw "To is required.";
                }
            }
            if ($scope.ToStatus === 'INVENTORY') {
                $scope.productionSummaryNew.ToSFGInventoryId = $scope.productionSummaryNew.ToProcessId;
                $scope.productionSummaryNew.ToProcessId = null;
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ToSFGInventoryId)) {
                    throw "To is required.";
                }
            }

            if ($scope.ToStatus === 'PROCESS') {
                $scope.productionSummaryNew.ToSFGInventoryId = null;
                if (baseService.isUndefinedOrNull($scope.productionSummaryNew.ToProcessId)) {
                    throw "To is required.";
                }
            }

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
                $scope.productionSummaryNew.QtyWithoutScan = $scope.ProdQty;
                $scope.productionSummaryNew.Quantity = $scope.ProdQty;
                $scope.productionSummaryNew.SKUQty = $scope.ProdQty;
                $scope.productionSummaryNew.SourceType = "SKU";
            }
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SourceType)) {
                $scope.productionSummaryNew.SourceType = "PB";
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    "ps": $scope.productionSummaryNew,
                    "psd": $scope.ProductionSummaryDetail,
                    "level": $scope.productionSummaryNew.ProductionBookingLevel,
                    "productionOrderId": $scope.productionSummaryNew.ProductionOrderId,
                    "salesOrderId": $scope.productionSummaryNew.SalesOrderId,
                    "processId": $scope.ProcessId,
                    "status": $scope.Status,
                    "IsFirst": $scope.IsFirst,
                    "IsCrossAllowed": $scope.IsCrossAllowed
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    if ($scope.Status === 'INVENTORY') {
                        $scope.productionSummaryNew.ProcessId = $scope.productionSummaryNew.FromSFGInventoryId;
                    }

                    if ($scope.ToStatus === 'INVENTORY') {
                        $scope.productionSummaryNew.ToProcessId = $scope.productionSummaryNew.ToSFGInventoryId;
                    }
                }
                else {
                    if ($scope.Status === 'INVENTORY') {
                        $scope.productionSummaryNew.ProcessId = $scope.productionSummaryNew.FromSFGInventoryId;
                    }

                    if ($scope.ToStatus === 'INVENTORY') {
                        $scope.productionSummaryNew.ToProcessId = $scope.productionSummaryNew.ToSFGInventoryId;
                    }
                    $scope.getLineGrid();
                    $scope.Action = 'Save';
                    //$scope.getProdQty();
                    $scope.closeCharPopUp();
                    //$scope.GetTotalProductionBookingQty();
                    //$scope.GetSFGWIPQty();
                    //$scope.GetWIPQtyForValidation();

                    $scope.ClearMasterPart();

                    $scope.ProdQtyCount = 0;
                    $scope.InQuantity = 0;
                    $scope.OutQuantity = 0;
                    $scope.KillQuantity = 0;
                    $scope.TotalSalesOrderQty = 0;
                    $scope.TotalProductionBookingQty = 0;
                    $scope.TotalSalesOrderQty = 0;
                    $scope.RemainQty = 0;


                    ShowResult(response.data.Message, 'success');
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

    $scope.message_detailconfirmation = null;
    $scope.removeData = function (obj) {

        $scope.productionSummaryNew.Id = obj.data.Id;
        if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.deleteMaster = function (master) {
        if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productionSummaryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getLineGrid();
                    //$scope.ProdQty();
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

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.productionSummary = {};
        $scope.productionSummaryNew = {};
        $scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');

        $scope.SetBack(false);
        $scope.IsGo = false;
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
        $scope.ProdQtyCount = 0;
        $scope.InQuantity = 0;
        $scope.OutQuantity = 0;
        $scope.KillQuantity = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;
        $scope.GetIsProductionHourOpen();
    }

    //#region WIP Location
    $scope.SelectedEntityText = $('#ddlEntity option:selected').text();
    $scope.SelectedWC = $('#ddlWC option:selected').text();


    $scope.InOutKillWC = [];
    $scope.GetInWC = function (flag, type, data) {
        $scope.InOutKillWC = [];
        $scope.SelectedWorkcenter = data;

        var _url = $scope.path + 'GetInWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        if (type == "OUT")
            _url = $scope.path + 'GetOutWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))
        else if (type == "KILL")
            _url = $scope.path + 'GetKillWC?FDUD=' + flag + '&PlantId=' + $scope.SelectedPlant + '&EntityId=' + data.EntityId + '&ProcessId=' + $scope.SelectedProcess.Id + '&WorkCenterMasterId=' + data.WorkCenterMasterId + '&date=' + ($filter('dateFiltering')(new Date($scope.FromDateParameter), 'dd-MM-yyyy'))

        $http({
            method: 'GET',
            url: _url
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++)
                response.data[i].ProductionDate = new Date(response.data[i].ProductionDate);

            $scope.InOutKillWC = response.data;
        });
        $rootScope.openPopup('dialogInOutKill');

    }
    //#endregion 

}