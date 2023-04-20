'use strict';
WCWorkStationsControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function WCWorkStationsControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "WC/Work Stations Control";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.gradeList = [];
    $scope.path = 'Productions/WCWorkStationsControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlWC = $scope.path + 'CreateWSC';
    $scope.UpdateUrlWC = $scope.path + 'UpdateWC';
    $scope.saveUrlReasonValue = $scope.path + 'createReasonValue';
    $scope.saveUrlDetentionWC = $scope.path + 'createDetentionWC';
    $scope.saveDetailUrl = $scope.path + 'createDetail';
    $scope.saveSecondDetailUrl = $scope.path + 'createSecondDetail';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.TotalSalesOrderQty = 0;
    $scope.TotalProductionBookingQty = 0;
    $scope.RemainQty = 0;
    $scope.DetentionSum = 0;

    

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
        WorkCenterMasterId: null,
        Date: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ShiftId: null,
        WSMId: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        InCharge: null,
        InChargeId: null,
        Remarks: null,
        Column1: null,
        Column2: null,
        Column3: null,
        Column4: null,
        PeriodId: null,
    };
    $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

    // Refreshing the serials
    function refreshSerial() {
        for (var j = 0; j < $scope.wcList.length; j++) {
            $scope.wcList[j].Serial = j;
        }
    }
    // Add Tiles
    $scope.AddTile = function (e) {
        console.log(e);
        let ob = {};
        Object.assign(ob, e);
        ob.Flag = 0;
        ob.Id = null;
        ob.WorkCenterMasterId = e.WorkCenterMasterId;
        ob.ProductionOrderId = null;
        ob.LotNumber = null;
        ob.ProductionGrade = $scope.gradeList[0].Value;
        ob.Quantity = 0;
        ob.DetentionSum = 0;
        ob.SumMin = 0;
        ob.Remarks = null;
        ob.ResponsiblePersonId = e.ResponsiblePersonId;
        ob.InChargeId = e.InChargeId;
        $scope.wcList.splice(e.Serial + 1, 0, ob);
        refreshSerial();
    }

    $scope.CD1 = null;
    $scope.CD2 = null;
    $scope.CD3 = null;
    $scope.CD4 = null;
    $scope.rowDataBoundWSC = function rowDataBoundWSC(e) {
        if (!baseService.isUndefinedOrNull(e.data.CD1)) {
            e.model.columns[4].visible = true;
            $scope.CD1 = e.data.CD1;
        }
        else {
            e.model.columns[4].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD2)) {
            e.model.columns[5].visible = true;
            $scope.CD2 = e.data.CD2;
        }
        else {
            e.model.columns[5].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD3)) {
            e.model.columns[6].visible = true;
            $scope.CD3 = e.data.CD3
        }
        else {
            e.model.columns[6].visible = false;
        }
        if (!baseService.isUndefinedOrNull(e.data.CD4)) {
            e.model.columns[7].visible = true;
            $scope.CD4 = e.data.CD4;
        }
        else {
            e.model.columns[7].visible = false;
        }
    }


    $scope.WSMUserNameList = [];
    $scope.GetWSMUserNameList = function () {
        $http({
            method: 'GET',
            url: 'Productions/WCWorkStationsControl/GetWSMUserNameList'
        }).then(function successCallback(response) {
            $scope.WSMUserNameList = response.data;
        });
    }
    $scope.GetWSMUserNameList();

    $scope.PeriodList = [];
    $scope.GetPeriodList = function () {
        $http({
            method: 'GET',
            url: 'Productions/WCWorkStationsControl/GetPeriodList'
        }).then(function successCallback(response) {
            $scope.PeriodList = response.data;
        });
    }
    $scope.GetPeriodList();

    $scope.WSCId = null;
    $scope.ProductionReasonList = [];
    $scope.getReasonValuePopup = function (data) {
        $scope.NewObject = data.data;
        $scope.WSCId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/WCWorkStationsControl/LoadProcessReasonList?ProcessId=' + $scope.productionSummaryNew.ProcessId + '&WSCId=' + $scope.WSCId
        }).then(function successCallback(response) {
            $scope.ProductionReasonList = response.data;
            var gridObj = $("#GridReasonValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ReasonValuePopup')).modal('show');
        }
        )
    }

    $scope.closeReasonValuePopup = function () {
        angular.element(document.querySelector('#ReasonValuePopup')).modal('hide');
    }

    $scope.SaveReasonValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProductionReasonList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.ProductionReasonList[i].ReasonValue)) {
                    $scope.ProductionReasonList[i].WSCId = $scope.WSCId;
                    $scope.SaveList.push($scope.ProductionReasonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlReasonValue,
                data: { 'ProductionReasonData': $scope.SaveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

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

    $scope.selectProductionInCharge = function () {
        $scope.getProductionInCharge();
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('show');
    }

    $scope.ProductionInChargeList = [];
    $scope.getProductionInCharge = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductionInChargeList = resp.data;
        });
    }

    $scope.doubleProductionInCharge = function (e) {
        $scope.productionSummaryNew.ProductionInChargeId = e.data.SystemId;
        $scope.productionSummaryNew.ProductionInCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('hide');
    }

    $scope.closeProductionInChargePopUp = function () {
        angular.element(document.querySelector('#ProductionInChargePopup')).modal('hide');
    }

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

    $scope.ArticleList = [];
    $scope.getArticle = function (POId) {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetArticle?POID=" + POId + ""
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
        });
    }

    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.productionSummaryNew.ProcessId = $scope.processList[0].Value;
                $scope.getProdLevel();
                //default
                $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId);
            }
        });
    };

    $scope.LotNumberList = [];
    $scope.disGo = false;
    $scope.IsVisible = true;
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
    $scope.loadWC = function () {
        try {
            $http.get('Productions/WCWorkStationsControl/GetWCProcessCboNew?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&Date=' + $scope.productionSummaryNew.Date + '&shiftId=' + $scope.productionSummaryNew.ShiftId + '&WSMId=' + $scope.productionSummaryNew.WSMId)
                .then(function (response) {
                    $scope.wcList = response.data;
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        Object.assign($scope.wcList[i], { 'Serial': parseInt(i) });
                    }
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
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
            $http.get('Productions/WCWorkStationsControl/GetLotNumberCbo?SalesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId + '&ProcessId=' + $scope.productionSummaryNew.ProcessId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
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

    //$scope.productTimeList = [];
    //cboService.getProductionBookingPeriodCbo(function (result) {
    //    $scope.productTimeList = result;
    //});

    $scope.ProdQtyCount = 0;
    $scope.getProdQty = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/WCWorkStationsControl/GetTotalProductionQty?wcid=' + $scope.productionSummaryNew.WorkCenterMasterId + '&workdate=' + $scope.productionSummaryNew.ProductionDate)
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
                if (baseService.isUndefinedOrNull($scope.NewObject.ProductionOrderId)) {
                    $scope.NewObject.ProductionOrderId = $scope.ProductionOrderId;
                }
                $http.get('Productions/WCWorkStationsControl/GetTotalPOQty?productionOrderId=' + $scope.NewObject.ProductionOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
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
                $http.get('Productions/WCWorkStationsControl/GetTotalSOQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
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

    //$scope.shiftList = [];
    //cboService.GetProductionShiftCbo(function (result) {
    //    $scope.shiftList = result;
    //    if (baseService.arrayLength(result) === 1) {
    //        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
    //    }
    //});

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }


    $scope.DateValidation = function (ProductionDate) {
        try {
            if (new Date(ProductionDate) > new Date()) {
                throw "Production Date must be below or equal to current Date!";
            }

        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };
    $scope.RemainingQtyValue = null;
    $scope.ValidateProdQty = function(ProcessId,POId) {
            $http({
                method: 'POST',
                url: $scope.path + 'GetProdQtyValidate?Processid=' + ProcessId + '&POId=' + POId,
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.RemainingQtyValue = resp.data[0].RemainingQty;
            });
    }

    function ValidationMaster() {
        try {
            //CheckField("Work Center Master", $scope.productionSummaryNew.WorkCenterMasterId);

            if ($scope.LotNumberCapture && $scope.LotNumberMandatory) {
                CheckField("Lot Number", $scope.NewObject.LotNumber);
            }

            /*  if ($scope.productionSummaryNew.ProductionBookingLevel === "ProductionOrder") {*/
            if ($scope.productionSummaryNew.ProductionOrderId == null) {
                CheckField("Production Order", $scope.productionSummaryNew.ProductionOrderId);
                /*CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);*/
                //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            }
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === "SalesOrder") {
            //    CheckField("Sales Order", $scope.productionSummaryNew.SalesOrderId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === "MasterOrderItem") {
            //    CheckField("Master Order Item", $scope.productionSummaryNew.MasterOrderItemId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
            //else {
            //    CheckField("Product Code", $scope.productionSummaryNew.ProductLibraryId);
            //    CheckField("Master Order No", $scope.productionSummaryNew.MasterOrderNo);
            //    CheckField("MaterialMaster", $scope.productionSummaryNew.MaterialMasterId);
            //    CheckField("Article", $scope.productionSummaryNew.ArticleId);
            //    CheckField("Production Grade", $scope.productionSummaryNew.ProductionGrade);
            //    //CheckField("Quantity", $scope.productionSummaryNew.Quantity);
            //}
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationPreMaster() {
        try {
            CheckField("Entity", $scope.productionSummaryNew.EntityId);
            CheckField("Process", $scope.productionSummaryNew.ProcessId);
            CheckField("Date", $scope.productionSummaryNew.Date);
            CheckField("Shift", $scope.productionSummaryNew.ShiftId);
            CheckField("WSM UserName", $scope.productionSummaryNew.WSMId);
            CheckField("Period", $scope.productionSummaryNew.PeriodId);
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
            $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId);
            $scope.loadWC($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.EntityId);
            $scope.getProdLevel();
            $scope.SetGo(isdisabled);
            if ($scope.IsParameterBased == true) {
                $scope.IsVisible = false;
            }
            else {
                $scope.IsVisible = true;
            }
            //$scope.getLineGrid();
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
        $http.get('Productions/WCWorkStationsControl/GetItemsData?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.productionSummaryNew.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.productionSummaryNew.ProductionOrderId)
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
    $scope.getProductionOrderPopUp = function (data) {
        $scope.NewObject = data.data;
        if (baseService.isUndefinedOrNull(data.data.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.ProductionOrderList = [];
        $http.get('Productions/WCWorkStationsControl/GetProductionOrderDataListWC?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + data.data.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
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
        $scope.NewObject.LotNumber = $event.data.LotNumber;
        $scope.NewObject.ResponsiblePerson = $scope.productionSummaryNew.HeaderResponsiblePerson;
        $scope.NewObject.RemainingQty = $event.data.RemainingQty;
        $scope.GetTotalProductionBookingQty();
        $scope.getArticle($scope.NewObject.ProductionOrderId);
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        angular.element(document.querySelector('#POItemPopup')).modal('hide');

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
    /*$scope.ProductionOrderId = null;*/
    $scope.ClearMasterPart = function () {
        $scope.ProductionOrderId = $scope.productionSummaryNew.ProductionOrderId;
        $scope.SalesOrderId = $scope.productionSummaryNew.SalesOrderId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var processid = $scope.productionSummaryNew.ProcessId;
        var workdate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var wcid = $scope.productionSummaryNew.WorkCenterMasterId;
        var piid = $scope.productionSummaryNew.ProductionInChargeId;
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

        $scope.productionSummaryNew.Quantity = null;
        $scope.productionSummaryNew.QtyWithoutScan = 0;
        $scope.productionSummaryNew.ScanQty = 0;
        $scope.productionSummaryNew.Customer = null;
        $scope.productionSummaryNew.ResponsiblePersonId = null;
        $scope.productionSummaryNew.ResponsiblePersonName = null;
        $scope.productionSummaryNew.InChargeId = null;
        $scope.productionSummaryNew.InCharge = null;
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
        $http.get('Productions/WCWorkStationsControl/GetChar1Info?id=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.SalesOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.getChar1 = function (masterid, soid) {
        $scope.ProductionSummaryDetail = [];
        $http.get('Productions/WCWorkStationsControl/GetChar1Info?id=' + masterid + '&soid=' + soid)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
        //CharCount 1
    };

    $scope.mentorandresperson = [];
    $scope.getMentorAndRespPersonByWCM = function () {
        $http.get('productions/WCWorkStationsControl/getmentorandresppersonbywcm?wcmId=' + $scope.productionSummaryNew.WorkCenterMasterId)
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

        $http.get('Productions/WCWorkStationsControl/GetChar1InfobyPrO?masterid=' + $scope.productionSummaryNew.Id + '&soid=' + $scope.productionSummaryNew.ProductionOrderId)
            .then(function (response) {
                $scope.ProductionSummaryDetail = [];
                $scope.ProductionSummaryDetail = response.data;
            });
    };

    $scope.getChar2Info = function () {
        $scope.ProductionSummaryDetail = [];

        $http.get('Productions/WCWorkStationsControl/GetCharInfoByPrO?masterid=' + $scope.productionSummaryNew.Id + '&workdate=' + $scope.productionSummaryNew.ProductionDate + '&mmid=' + $scope.productionSummaryNew.MaterialMasterId + '&soid=' + $scope.productionSummaryNew.ProductionOrderId + '&artid=' + $scope.productionSummaryNew.ArticleId + '&CharCount=' + $scope.productionSummaryNew.CharCount + '&CharacteristicsValueId=' + $scope.CharacteristicsValueId)
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
            $http.get('Productions/WCWorkStationsControl/GetLineItemGrid?entityid=' + entityid + '&processid=' + processid + '&workdate=' + workdate + '&shiftid=' + shiftid + '&wcid=' + wcid + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel)
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
        var piid = $scope.productionSummaryNew.ProductionInChargeId;
        var ProductionBookingLevel = $scope.productionSummaryNew.ProductionBookingLevel;

        $scope.index = index;
        $scope.productionSummary = $scope.LineGridList[$scope.index];
        $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.ProductionDate = workdate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.WorkCenterMasterId = wcid;
        $scope.productionSummaryNew.ProductionInChargeId = piid;
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

    $scope.showEmployeeListPopUp = function (data, flag) {
        $scope.respOrMentor = flag;
        $scope.NewObject = data.data;
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
                $scope.NewObject.ResponsiblePersonId = employee.SystemId;
                $scope.NewObject.ResponsiblePerson = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'Mentor') {
                $scope.NewObject.MentorId = employee.SystemId;
                $scope.NewObject.Mentor = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'CheckedBy') {
                $scope.NewObject.CheckedBy = employee.SystemId;
                $scope.NewObject.CheckedByName = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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
            $scope.getProdLevel();
            ValidationMaster();
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
            $scope.ValidateProdQty($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.ProductionOrderId);
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

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.TotalSalesOrderQty) <= parseFloat($scope.TotalProductionBookingQty) + parseFloat($scope.productionSummaryNew.Quantity)) {
            //        throw " less than Order Quantity.";
            //    }
            //}

            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.RemainingQtyValue))
            {
                throw "Produced Quantity should not be greater than RemainingQtyValue.";
            }

            if ($scope.IsFirst == false) {
                if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
                    throw "Produced Quantity should less than Order Quantity.";
                }
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
                throw "Produced Quantity should not be greater than Balance Quantity.";
            }

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    "ps": $scope.productionSummaryNew,
                    "psd": $scope.ProductionSummaryDetail,
                    "ProcessParaList": $scope.ProcessParaList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    //$scope.loadWC();
                    $scope.Action = 'Save';
                }
                angular.element(document.querySelector('#ProcessParaPopup')).modal('hide');
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.refreshTemplateProductionSummaryWC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWorkCenter });
    };
    function CheckBoxSelectAllWorkCenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.wcList.length; i++) {
                $scope.wcList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };
    $scope.refreshTemplateProductionSummaryDetentionWC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWorkCenterDetention });
    };
    function CheckBoxSelectAllWorkCenterDetention(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryDetentionWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                $scope.ProcessDetentionLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveMasterWC = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var date = $scope.productionSummaryNew.Date;
        var shiftid = $scope.productionSummaryNew.ShiftId;
        var wsmid = $scope.productionSummaryNew.WSMId;
        var periodid = $scope.productionSummaryNew.PeriodId;
       
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.Date = date;
        $scope.productionSummaryNew.ShiftId = shiftid;
        $scope.productionSummaryNew.WSMId = wsmid;
        $scope.productionSummaryNew.PeriodId = periodid;
      
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrlWC,
                data: {
                    "WSCData": $scope.productionSummaryNew,
                    "Column1": data.data.Column1,
                    "Column2": data.data.Column2,
                    "Column3": data.data.Column3,
                    "Column4": data.data.Column4
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    //$scope.loadWC();
                    $scope.Action = 'Save';
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.UpdateMasterWC = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var PInChargId = $scope.productionSummaryNew.ProductionInChargeId;
        var PInCharg = $scope.productionSummaryNew.ProductionInCharge;
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.ProductionInChargeId = PInChargId;
        $scope.productionSummaryNew.ProductionInCharge = PInCharg;
        try {
            var date = new Date();
            date.setDate(date.getDate() - 1);
            $scope.YDate = $filter('dateFiltering')(date);
            $scope.getProdLevel();
            if ($scope.productionSummaryNew.ProductionDate < $scope.YDate) {
                throw "Update should be perform only for today's and yestarday's date.";
            }
            ValidationMaster();
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

            $scope.productionSummaryNew.QtyWithoutScan = $scope.productionSummaryNew.Quantity;
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

            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
                throw "Produced Quantity should not be greater than Balance Quantity.";
            }

            if ($scope.IsFirst == false) {
                if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
                    throw "Produced Quantity should less than Order Quantity.";
                }
            }

            $http({
                method: 'POST',
                url: $scope.UpdateUrlWC,
                data: {
                    "ps": $scope.productionSummaryNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    $scope.Action = 'Save';
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

    $scope.deleteMasterWC = function (master) {
        if (!baseService.isUndefinedOrNull(master.data.Id)) {
            $http({
                method: 'POST',
                url: 'Productions/WCWorkStationsControl/DeleteMasterWC?id=' + master.data.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    // $scope.loadWC();
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        if ($scope.wcList[i].Id == master.data.Id) {
                            $scope.wcList[i].Id = null;
                            break;
                        }
                    }
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
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
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + prodOrdId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;

        });
    }
    //search

    $scope.ProcessParaList = [];
    $scope.getProcessParaPopupPoPUp = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        var PInChargId = $scope.productionSummaryNew.ProductionInChargeId;
        var PInCharg = $scope.productionSummaryNew.ProductionInCharge;
        $scope.productionSummaryNew = data.data;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.ProductionInChargeId = PInChargId;
        $scope.productionSummaryNew.ProductionInCharge = PInCharg;
        try {
            $scope.ProcessParaList = [];
            $http.get('Productions/WCWorkStationsControl/GetProcessParaData?processId=' + $scope.productionSummaryNew.ProcessId + '&masterId=' + data.data.Id + '&ProductionOrderId=' + data.data.ProductionOrderId)
                .then(
                    function successCallback(response) {
                        $scope.ProcessParaList = response.data;
                        $scope.GetTotalProductionBookingQty();
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });

            angular.element(document.querySelector('#ProcessParaPopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.Calculate = function () {
        try {
            $scope.productionSummaryNew.QtyWithoutScan = 0;
            $scope.NewObject.Quantity = 0;
            $http({
                method: 'POST',
                url: 'Productions/WCWorkStationsControl/Calculate',
                data: { 'OpenHeadNew': $scope.ProcessParaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                for (var i = 0; i < response.data.NewData.length; i++) {
                    for (var j = 0; j < $scope.ProcessParaList.length; j++) {
                        if (response.data.NewData[i].UserName == $scope.ProcessParaList[j].UserName) {
                            $scope.ProcessParaList[j].Value = response.data.NewData[i].Value;
                        }
                    }
                    if (response.data.NewData[i].IsProduction == true) {
                        $scope.NewObject.Quantity += response.data.NewData[i].Value;
                        $scope.productionSummaryNew.QtyWithoutScan = response.data.NewData[i].Value;
                    }
                }
                //$scope.SaveMaster();
                var gridObj = $("#ProductionSummaryWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
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
        $scope.Action = "Save";
        $scope.productionSummary = {};
        $scope.productionSummaryNew = {};
        $scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');
        $scope.ProdQtyCount = 0;
        $scope.TotalProductionBookingQty = 0;
        $scope.TotalSalesOrderQty = 0;
        $scope.RemainQty = 0;
        $scope.SetBack(false);
        $scope.IsGo = false;
        $scope.ProductionSummaryDetail = [];
        $scope.wcList = [];
    }

    $scope.ProcessDetention = {
        Id: null,
        ProductionSummaryId: null,
        EntityId: null,
        ProcessId: null,
        ShiftId: null,
        Date: null,
        WorkCenterMasterId: null,
        DepartmentId: null,
        DetentionId: null,
        ResponsiblePersonId: null,
        WorkCenter: null,
        Detention: null,
        DetentionType: null,
        DetentionTypeId: null,
        Department: null,
        ResponsiblePerson: null,
        Remark: null,
        FromTime: null,
        ToTime: null,
        Minute: null,
    };

    $scope.ProductionSummaryId = null;
    $scope.productionSummaryNew.DetentionSum = 0;
    $scope.ProcessDetentionLists = [];
    $scope.getProcessDetentionPopupPoPUp = function (data) {
        $scope.NewObject = data.data;
        var processid = $scope.productionSummaryNew.ProcessId;
        var entityid = $scope.productionSummaryNew.EntityId;
        var productiondate = $scope.productionSummaryNew.ProductionDate;
        var shiftid = $scope.productionSummaryNew.ProductionShiftId;
        $scope.productionSummaryNew = data.data;
        $scope.ProductionSummaryId = $scope.productionSummaryNew.Id;
        $scope.productionSummaryNew.ProcessId = processid;
        $scope.productionSummaryNew.EntityId = entityid;
        $scope.productionSummaryNew.ProductionDate = productiondate;
        $scope.productionSummaryNew.ProductionShiftId = shiftid;
        $scope.productionSummaryNew.workCenter = data.data.WorkCenter;
        $scope.productionSummaryNew.workCenterId = data.data.WorkCenterMasterId;
        try {
            ValidationMaster();
            $scope.ProcessDetentionLists = [];
            for (var i = 1; i < 6; i++) {
                var obj = angular.copy($scope.ProcessDetention);
                obj.Id = null;
                obj.ProductionSummaryId = $scope.ProductionSummaryId;
                obj.ProcessId = $scope.productionSummaryNew.ProcessId;
                obj.EntityId = $scope.productionSummaryNew.EntityId;
                obj.ProductionDate = $scope.productionSummaryNew.ProductionDate;
                obj.ProductionShiftId = $scope.productionSummaryNew.ProductionShiftId;
                obj.workCenter = $scope.productionSummaryNew.workCenterId;
                obj.Sequence = i;
                $scope.ProcessDetentionLists.push(obj);
            }

            $http.get('Productions/WCWorkStationsControl/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + data.data.WorkCenterMasterId + '&ProductionSummaryId=' + data.data.Id)
                .then(
                    function successCallback(response) {
                        /*$scope.ProcessDetentionLists = response.data;*/
                        if (response.data.length > 0) {

                            for (var j = 0; j < response.data.length; j++) {
                                for (var k = 0; k < $scope.ProcessDetentionLists.length; k++) {
                                    if ($scope.ProcessDetentionLists[k].Sequence == response.data[j].Sequence) {
                                        $scope.ProcessDetentionLists[k].Flag = response.data[j].Flag;
                                        $scope.ProcessDetentionLists[k].Id = response.data[j].Id;
                                        $scope.ProcessDetentionLists[k].workCenter = response.data[j].WorkCenter;
                                        $scope.ProcessDetentionLists[k].ProductionSummaryId = response.data[j].ProductionSummaryId;
                                        $scope.ProcessDetentionLists[k].DepartmentId = response.data[j].DepartmentId;
                                        $scope.ProcessDetentionLists[k].DepartmentName = response.data[j].DepartmentName;
                                        $scope.ProcessDetentionLists[k].DetentionTypeList = response.data[j].DetentionTypeList;
                                        $scope.ProcessDetentionLists[k].DetentionList = response.data[j].DetentionList;
                                        $scope.ProcessDetentionLists[k].DetentionId = response.data[j].DetentionId;
                                        $scope.ProcessDetentionLists[k].DetentionTypeId = response.data[j].DetentionTypeId;
                                        $scope.ProcessDetentionLists[k].Detention = response.data[j].Detention;
                                        //$scope.ProcessDetentionLists[k].FromTime = response.data[j].FromTime;
                                        //$scope.ProcessDetentionLists[k].ToTime = response.data[j].ToTime;
                                        $scope.ProcessDetentionLists[k].Minute = response.data[j].Minute;
                                        $scope.ProcessDetentionLists[k].ResponsiblePersonId = response.data[j].ResponsiblePersonId;
                                        $scope.ProcessDetentionLists[k].ResponsiblePerson = response.data[j].ResponsiblePerson;
                                        $scope.ProcessDetentionLists[k].Remark = response.data[j].Remark;
                                    }

                                }
                            }

                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#articlePoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };


    $scope.getProcessDetention = function () {
        try {
            $scope.ProcessDetentionLists = [];
            for (var i = 1; i < 6; i++) {
                var obj = angular.copy($scope.ProcessDetention);
                obj.Id = null;
                obj.ProductionSummaryId = $scope.ProductionSummaryId;
                obj.ProcessId = $scope.productionSummaryNew.ProcessId;
                obj.EntityId = $scope.productionSummaryNew.EntityId;
                obj.ProductionDate = $scope.productionSummaryNew.ProductionDate;
                obj.ProductionShiftId = $scope.productionSummaryNew.ProductionShiftId;
                obj.workCenter = $scope.productionSummaryNew.workCenterId;
                obj.Sequence = i;
                $scope.ProcessDetentionLists.push(obj);
            }

            $http.get('Productions/WCWorkStationsControl/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + $scope.productionSummaryNew.workCenterId + '&ProductionSummaryId=' + $scope.ProductionSummaryId)
                .then(
                    function successCallback(response) {
                        if (response.data.length > 0) {

                            for (var j = 0; j < response.data.length; j++) {
                                for (var k = 0; k < $scope.ProcessDetentionLists.length; k++) {
                                    if ($scope.ProcessDetentionLists[k].Sequence == response.data[j].Sequence) {
                                        $scope.ProcessDetentionLists[k].Flag = response.data[j].Flag;
                                        $scope.ProcessDetentionLists[k].Id = response.data[j].Id;
                                        $scope.ProcessDetentionLists[k].workCenter = response.data[j].WorkCenter;
                                        $scope.ProcessDetentionLists[k].ProductionSummaryId = response.data[j].ProductionSummaryId;
                                        $scope.ProcessDetentionLists[k].DepartmentId = response.data[j].DepartmentId;
                                        $scope.ProcessDetentionLists[k].DepartmentName = response.data[j].DepartmentName;
                                        $scope.ProcessDetentionLists[k].DetentionTypeList = response.data[j].DetentionTypeList;
                                        $scope.ProcessDetentionLists[k].DetentionList = response.data[j].DetentionList;
                                        $scope.ProcessDetentionLists[k].DetentionId = response.data[j].DetentionId;
                                        $scope.ProcessDetentionLists[k].DetentionTypeId = response.data[j].DetentionTypeId;
                                        $scope.ProcessDetentionLists[k].Detention = response.data[j].Detention;
                                        //$scope.ProcessDetentionLists[k].FromTime = response.data[j].FromTime;
                                        $scope.ProcessDetentionLists[k].ToTime = response.data[j].ToTime;
                                        $scope.ProcessDetentionLists[k].Minute = response.data[j].Minute;
                                        $scope.ProcessDetentionLists[k].ResponsiblePersonId = response.data[j].ResponsiblePersonId;
                                        $scope.ProcessDetentionLists[k].ResponsiblePerson = response.data[j].ResponsiblePerson;
                                        $scope.ProcessDetentionLists[k].Remark = response.data[j].Remark;
                                    }
                                }
                            }

                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.LoadDetentionList = function () {
        try {
            $http.get('Productions/WCWorkStationsControl/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + $scope.productionSummaryNew.workCenterId + '&ProductionSummaryId=' + $scope.ProductionSummaryId)
                .then(function (response) {
                    $scope.ProcessDetentionLists = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //$scope.DetentionTypeList = [];
    //$scope.GetDetentionTypeList = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'IE/MachineMasterTransaction/GetDetentionTypeList'
    //    }).then(function successCallback(response) {
    //        $scope.DetentionTypeList = response.data;
    //    });
    //}


    var currRow = null;
    $scope.DetentionList = [];
    $scope.GetDetentionList = function (data) {
        var gridObj = $("#ProductionSummaryDetentionWC").ejGrid("instance");
        currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionListWC?DetentiontypeId=' + currRow.DetentionTypeId
        }).then(function successCallback(response) {
            currRow.DetentionList = response.data;
            //if (response.data.length > 0)
            //{
            //    currRow.DetentionList = response.data;
            //    for (i = 0; i < response.data.length; i++)
            //    {
            //        if (response.data[i].Value == currRow.DetentionId)
            //        {
            //            currRow.DetentionId = response.data[i].Value;

            //            break;
            //        }
            //    }
            //}
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();

        });
    };

    $scope.selectDepartment = function (data) {
        $scope.Newobject = data.data;
        $scope.getsD();
        $scope.NewObject.DetentionId = null;
        $scope.NewObject.DetentionList = null;
        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        angular.element(document.querySelector('#DepartmentPop')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getsD = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterTransaction/GetDetentionDepartment',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DepartmentList = resp.data;
        });
    }

    $scope.doubleDepartment = function (e) {
        $scope.Newobject.DepartmentId = e.data.DepartmentId;
        $scope.Newobject.DepartmentName = e.data.DepartmentName;
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
        $scope.getDetentionTypeListByDepartment($scope.Newobject.DepartmentId);
        $scope.getDetentionListByDepartment($scope.Newobject.DepartmentId);
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }

    $scope.selectResponsible = function (data) {
        $scope.Newobject = data.data;
        $scope.Newobject.DetentionId = data.data.DetentionId;
        $scope.getsR();
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('show');
    }

    $scope.ResponsibleList = [];
    $scope.getsR = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterTransaction/GetDetentionResponsible?detentionId=' + $scope.Newobject.DetentionId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;
        });
    }

    $scope.doubleResponsible = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.ResponsiblePersonId;
        $scope.Newobject.ResponsiblePerson = e.data.ResponsiblePerson;
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }

    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }

    $scope.getDetentionTypeListByDepartment = function (departmentid) {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/getDetentionTypeListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            //$scope.DetentionList = null;
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                if ($scope.ProcessDetentionLists[i].DetentionId == null) {
                    $scope.ProcessDetentionLists[i].DetentionTypeList = response.data;
                }
            }
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }
    $scope.getDetentionListByDepartment = function (departmentid) {
        $scope.Newobject.DetentionList = null;
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/getDetentionListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            //$scope.DetentionList = null;
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                if ($scope.ProcessDetentionLists[i].DetentionId == null) {
                    $scope.ProcessDetentionLists[i].DetentionList = response.data;
                }
            }
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.refreshTemplateProductionSummaryDetentionWC = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllDetentionWorkCenter });
    };
    function CheckBoxSelectAllDetentionWorkCenter(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProductionSummaryDetentionWC").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.wcList.length; i++) {
                $scope.ProcessDetentionLists[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.SaveDetentionWC = function () {
        try {

            $scope.DetentionSaveList = [];
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                //if ($scope.ProcessDetentionLists[i].Flag == true)
                if (!baseService.isUndefinedOrNull($scope.ProcessDetentionLists[i].Minute)) {
                    $scope.ProcessDetentionLists[i].ProductionSummaryId = $scope.productionSummaryNew.Id;
                    $scope.ProcessDetentionLists[i].EntityId = $scope.productionSummaryNew.EntityId;
                    $scope.ProcessDetentionLists[i].ProcessId = $scope.productionSummaryNew.ProcessId;
                    $scope.ProcessDetentionLists[i].Date = $scope.productionSummaryNew.ProductionDate;
                    $scope.ProcessDetentionLists[i].shiftid = $scope.productionSummaryNew.ProductionShiftId;
                    $scope.ProcessDetentionLists[i].WorkCenterId = $scope.productionSummaryNew.WorkCenterMasterId;
                    $scope.DetentionSaveList.push($scope.ProcessDetentionLists[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrlDetentionWC,
                data: {
                    "DataList": $scope.DetentionSaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                    var Sum = 0;
                    for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                        if (!baseService.isUndefinedOrNull($scope.ProcessDetentionLists[i].Minute)) {
                            Sum = parseInt(Sum) + parseInt($scope.ProcessDetentionLists[i].Minute);
                        }

                    }
                    $scope.NewObject.SumMin = Sum;
                    $scope.getProcessDetention();
                    var gridObj = $("#ProductionSummaryWC").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                    //$scope.loadWC();
                }
                angular.element(document.querySelector('#articlePoUp')).modal('hide');
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.getMinute = function () {
        try {
            $scope.MinuteUrl = 'IE/MachineMasterTransaction/GetMinute/'
            $http({
                method: 'POST',
                url: $scope.MinuteUrl,
                data: { 'data': $scope.Newobject },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.Newobject.Minute = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $http.get('Productions/WCWorkStationsControl/GetShiftList?processId=' + $scope.productionSummaryNew.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

    $scope.selectGridIncharge = function (data) {
        $scope.Newobject = data.data;
        $scope.getsI();
        angular.element(document.querySelector('#InchargeGridPopup')).modal('show');
    }

    $scope.InchargeGridList = [];
    $scope.getsI = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.InchargeGridList = resp.data;
        });
    }

    $scope.doubleInchargeGrid = function (e) {
        $scope.Newobject.InChargeId = e.data.SystemId;
        $scope.Newobject.InCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');
    }

    $scope.closeInchargeGridPopup = function () {
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');

    }

    $scope.selectGridResponsible = function (data) {
        $scope.Newobject = data.data;
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.getSalesOrderPopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getSalesOrder();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('show');
    }

    $scope.SalesOrderItemList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSalesOrder?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SalesOrderItemList = resp.data;
        });
    }

    $scope.selectSalesOrderItem = function (e) {
        $scope.Newobject.SalesOrderId = e.data.SOId;
        $scope.Newobject.SOArticle = e.data.Article;
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('hide');
    }

    $scope.getMasterOrderItemPopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getMasterOrderItem();
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
    }

    $scope.MasterOrderItemList = [];
    $scope.getMasterOrderItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemList = resp.data;
        });
    }

    $scope.selectMasterOrderItem = function (e) {
        $scope.Newobject.MasterOrderItemId = e.data.MasterOrderItemId;
        $scope.Newobject.MOIArticle = e.data.Article;
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');
    }

    $scope.getProductCodePopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getProductCode();
        angular.element(document.querySelector('#ProductCodePopup')).modal('show');
    }

    $scope.ProductCodeList = [];
    $scope.getProductCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProductCode?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductCodeList = resp.data;
        });
    }

    $scope.selectProductCode = function (e) {
        $scope.Newobject.MasterOrderItemId = e.data.MOIId;
        $scope.Newobject.ProductCodeArticle = e.data.Article;
        angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
    }
}