'use strict';
ProductionSummaryWCController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionSummaryWCController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Production Booking";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.productionSummaryes = [];
    $scope.gradeList = [];
    $scope.path = 'Productions/productionSummary/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlWC = $scope.path + 'createWC';
    $scope.UpdateUrlWC = $scope.path + 'UpdateWC';
    $scope.saveUrlReason = $scope.path + 'createReason';
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
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

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
        ProductionInChargeId: null,
        ProductionInCharge: null,
        MasterOrderNo: null,
        SalesOrderId: null,
        MasterOrderItemId: null,
        ProductionOrderId: null,
        MaterialMasterId: null,
        MaterialMaster: null,
        ArticleId: null,
        Article: null,
        MOIArticle: null,
        SOArticle: null,
        ProductCodeArticle: null,
        WorkCenterMasterId: null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ProductionShiftId: null,
        ProductionGrade: $scope.gradeList[0].Value,
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
        InCharge: null,
        InChargeId: null,
        InTime: null,
        OutTime: null,
        ConsumeHour: 0,
        ManPower: 0,
        Remarks: null,
        CheckedBy: null,
        CheckedByName: null,
        LotNumber: null,
        DetentionSum: 0,
        PPQFlag: false,
        IsInventory: false,
        SourceType: 'PB'
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

    $scope.Reason = {
        Id: null,
        ProcessId: null,
        ReasonName: null,
    };
    $scope.ReasonNew = Object.assign({}, $scope.Reason);

    $scope.ProcessReasonList = [];
    $scope.GetProcessReasonList = function () {
        $http({
            method: 'GET',
            url: 'Productions/productionSummary/GetProcessReasonList'
        }).then(function successCallback(response) {
            $scope.ProcessReasonList = response.data;
        });
    }
    $scope.GetProcessReasonList();

    $scope.ReasonList = [];
    $scope.LoadReasonDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/productionSummary/LoadReasonDetails'
        }).then(function successCallback(response) {
            $scope.ReasonList = response.data;
        }
        )
    }
    $scope.LoadReasonDetails();

    $scope.GetReasonDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/productionSummary/LoadReasonDetailsEditData?ReasonId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ReasonNew = response.data.Reason[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.ReasonSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ReasoningDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlReason,
                data: {
                    'ReasonData': $scope.ReasonNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadReasonDetails();
                    ReasonClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ReasonClear = function () {
        ReasonClearFields();
    };

    function ReasonClearFields() {
        $scope.Action = "Save";
        $scope.ReasonNew = Object.assign({}, $scope.Reason);
    }

    $scope.ProductionId = null;
    $scope.ProductionReasonList = [];
    $scope.getReasonValuePopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProductionId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/productionSummary/LoadProcessReasonList?ProcessId=' + $scope.productionSummaryNew.ProcessId + '&ProductionId=' + $scope.ProductionId
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
                    $scope.ProductionReasonList[i].ProductionId = $scope.ProductionId;
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
            $http.get('Productions/ProductionSummary/GetWCProcessCboNew?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&ProductionInChargeId=' + $scope.productionSummaryNew.ProductionInChargeId)
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

    //$scope.productTimeList = [];
    //cboService.getProductionBookingPeriodCbo(function (result) {
    //    $scope.productTimeList = result;
    //});

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
    $scope.TotalActualPlannedQty = 0;
    $scope.TotalProcessPlanPercentage = 0;
    $scope.TotalPOQty = 0;
    $scope.TotalProcessPlanQty = 0;
    $scope.TotalCurPOBalProd = 0;
    $scope.TotalPOPreviousProdQty = 0;
    $scope.TotalPOProcessSequence = 0;

    $scope.GetTotalProductionBookingQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            $scope.TotalCurPOBalProd = 0;
            $scope.TotalPOPreviousProdQty = 0;
            $scope.TotalPOFirstProcessProdQty = 0;
            $scope.TotalPOProcessSequence = 0;

            if ($scope.NewObject.BookingLevel === 'ProductionOrder') {
                if (baseService.isUndefinedOrNull($scope.NewObject.ProductionOrderId)) {
                    $scope.NewObject.ProductionOrderId = $scope.ProductionOrderId;
                }
                $http.get('Productions/Productionsummary/GetPOQty?productionOrderId=' + $scope.NewObject.ProductionOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(0);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(0);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(0);
                            $scope.TotalCurPOBalProd = parseFloat(response.data[0].CurPOBalProd).toFixed(0);
                            $scope.TotalPOPreviousProdQty = parseFloat(response.data[0].POPreviousProdQty).toFixed(0);
                            $scope.TotalPOFirstProcessProdQty = parseFloat(response.data[0].POFirstProcessProductionQty).toFixed(0);
                            $scope.TotalPOProcessSequence = parseFloat(response.data[0].POProcessSequence).toFixed(0);
                            $scope.NewObject.RemainingQty = $scope.RemainQty;
                            $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewObject.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewObject.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewObject.POQty = $scope.TotalPOQty;
                            $scope.NewObject.ProcessPlanQty = $scope.TotalProcessPlanQty;
                            $scope.NewObject.CurPOBalProd = $scope.TotalCurPOBalProd;
                            $scope.NewObject.POPreviousProdQty = $scope.TotalPOPreviousProdQty;
                            $scope.NewObject.POFirstProcessProductionQty = $scope.TotalPOFirstProcessProdQty;
                            $scope.NewObject.POProcessSequence = $scope.TotalPOProcessSequence;
                        }
                    });
            }
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'SalesOrder') {
            //    if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SalesOrderId)) {
            //        $scope.productionSummaryNew.SalesOrderId = $scope.SalesOrderId;
            //    }
            //    $http.get('Productions/Productionsummary/GetTotalSOQty?salesOrderId=' + $scope.productionSummaryNew.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
            //        .then(function (response) {
            //            if (baseService.arrayLength(response.data) > 0) {
            //                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
            //                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
            //                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
            //                $scope.NewObject.RemainingQty = $scope.RemainQty;
            //                $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
            //                $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
            //            }
            //        });
            //}
            //else if ($scope.productionSummaryNew.ProductionBookingLevel === 'MasterOrderItem' || $scope.productionSummaryNew.ProductionBookingLevel === 'ProductionCode') {
            //    if (baseService.isUndefinedOrNull($scope.NewObject.MasterOrderItemId)) {
            //        $scope.NewObject.MasterOrderItemId = $scope.MasterOrderItemId;
            //    }
            //    $http.get('Productions/Productionsummary/GetTotalMOIQty?MasterOrderItemId=' + $scope.NewObject.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
            //        .then(function (response) {
            //            if (baseService.arrayLength(response.data) > 0) {
            //                $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(2);
            //                $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
            //                $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(2);
            //                $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(2);
            //                $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
            //                $scope.NewObject.RemainingQty = $scope.RemainQty;
            //                $scope.NewObject.OrderQty = $scope.TotalSalesOrderQty;
            //                $scope.NewObject.BookedQty = $scope.TotalProductionBookingQty;
            //                $scope.NewObject.ActualPlannedQty = $scope.TotalActualPlannedQty;
            //                $scope.NewObject.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
            //            }
            //        });
            //}
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetMasterOrderItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            $scope.TotalCurPOBalProd = 0;
            $scope.TotalPOPreviousProdQty = 0;
            $scope.TotalPOFirstProcessProdQty = 0;
            $scope.TotalPOProcessSequence = 0;
            if ($scope.NewobjectMOI.BookingLevel === 'MasterOrderItem' || $scope.NewobjectMOI.BookingLevel === 'ProductionCode') {
                if (baseService.isUndefinedOrNull($scope.NewobjectMOI.MasterOrderItemId)) {
                    $scope.NewobjectMOI.MasterOrderItemId = $scope.MasterOrderItemId;
                }
                $http.get('Productions/Productionsummary/GetTotalMOIQty?POId=' + $scope.NewobjectMOI.ProductionOrderId + '&MasterOrderItemId=' + $scope.NewobjectMOI.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(2);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(0);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(0);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(0);
                            $scope.TotalCurPOBalProd = parseFloat(response.data[0].CurPOBalProd).toFixed(0);
                            $scope.TotalPOPreviousProdQty = parseFloat(response.data[0].POPreviousProdQty).toFixed(0);
                            $scope.TotalPOFirstProcessProdQty = parseFloat(response.data[0].POFirstProcessProductionQty).toFixed(0);
                            $scope.TotalPOProcessSequence = parseFloat(response.data[0].POProcessSequence).toFixed(0);
                            $scope.NewobjectMOI.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectMOI.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectMOI.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectMOI.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectMOI.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectMOI.POQty = $scope.TotalPOQty;
                            $scope.NewobjectMOI.ProcessPlanQty = $scope.TotalProcessPlanQty;
                            $scope.NewobjectMOI.CurPOBalProd = $scope.TotalCurPOBalProd;
                            $scope.NewobjectMOI.POPreviousProdQty = $scope.TotalPOPreviousProdQty;
                            $scope.NewobjectMOI.POFirstProcessProductionQty = $scope.TotalPOFirstProcessProdQty;
                            $scope.NewobjectMOI.POProcessSequence = $scope.TotalPOProcessSequence;
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetProductCodeItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            $scope.TotalCurPOBalProd = 0;
            $scope.TotalPOPreviousProdQty = 0;
            $scope.TotalPOFirstProcessProdQty = 0;
            $scope.TotalPOProcessSequence = 0;
            if ($scope.NewobjectPC.BookingLevel === 'ProductCode') {
                if (baseService.isUndefinedOrNull($scope.NewobjectPC.MasterOrderItemId)) {
                    $scope.NewobjectPC.MasterOrderItemId = $scope.MasterOrderItemId;
                }
                $http.get('Productions/Productionsummary/GetTotalPCQty?POId=' + $scope.NewobjectPC.ProductionOrderId + '&MasterOrderItemId=' + $scope.NewobjectPC.MasterOrderItemId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(0);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(0);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(0);
                            $scope.TotalCurPOBalProd = parseFloat(response.data[0].CurPOBalProd).toFixed(0);
                            $scope.TotalPOPreviousProdQty = parseFloat(response.data[0].POPreviousProdQty).toFixed(0);
                            $scope.TotalPOFirstProcessProdQty = parseFloat(response.data[0].POFirstProcessProductionQty).toFixed(0);
                            $scope.TotalPOProcessSequence = parseFloat(response.data[0].POProcessSequence).toFixed(0);
                            $scope.NewobjectPC.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectPC.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectPC.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectPC.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectPC.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectPC.POQty = $scope.TotalPOQty;
                            $scope.NewobjectPC.ProcessPlanQty = $scope.TotalProcessPlanQty;
                            $scope.NewobjectPC.CurPOBalProd = $scope.TotalCurPOBalProd;
                            $scope.NewobjectPC.POPreviousProdQty = $scope.TotalPOPreviousProdQty;
                            $scope.NewobjectPC.POFirstProcessProductionQty = $scope.TotalPOFirstProcessProdQty;
                            $scope.NewobjectPC.POProcessSequence = $scope.TotalPOProcessSequence;
                        }
                    });
            }
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.GetSalesOrderItemQty = function () {
        try {
            $scope.TotalSalesOrderQty = 0;
            $scope.TotalProductionBookingQty = 0;
            $scope.RemainQty = 0;
            $scope.TotalActualPlannedQty = 0;
            $scope.TotalProcessPlanPercentage = 0;
            $scope.TotalPOQty = 0;
            $scope.TotalProcessPlanQty = 0;
            $scope.TotalCurPOBalProd = 0;
            $scope.TotalPOPreviousProdQty = 0;
            $scope.TotalPOFirstProcessProdQty = 0;
            $scope.TotalPOProcessSequence = 0;
            if ($scope.NewobjectSO.BookingLevel === 'SalesOrder') {
                if (baseService.isUndefinedOrNull($scope.NewobjectSO.SalesOrderId)) {
                    $scope.NewobjectSO.SalesOrderId = $scope.SalesOrderId;
                }
                $http.get('Productions/Productionsummary/GetTotalSO?POId=' + $scope.NewobjectSO.ProductionOrderId + '&salesOrderId=' + $scope.NewobjectSO.SalesOrderId + '&processId=' + $scope.productionSummaryNew.ProcessId)
                    .then(function (response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TotalSalesOrderQty = parseFloat(response.data[0].PlannedQty).toFixed(0);
                            $scope.RemainQty = parseFloat(response.data[0].RemainingQty).toFixed(0);
                            $scope.TotalProductionBookingQty = parseFloat(response.data[0].TotalProductionQty).toFixed(0);
                            $scope.TotalActualPlannedQty = parseFloat(response.data[0].TotalActualPlannedQty).toFixed(0);
                            $scope.TotalProcessPlanPercentage = parseFloat(response.data[0].TotalProcessPlanPercentage).toFixed(0);
                            $scope.TotalPOQty = parseFloat(response.data[0].POQty).toFixed(0);
                            $scope.TotalProcessPlanQty = parseFloat(response.data[0].ProcessPlanQty).toFixed(0);
                            $scope.TotalCurPOBalProd = parseFloat(response.data[0].CurPOBalProd).toFixed(0);
                            $scope.TotalPOPreviousProdQty = parseFloat(response.data[0].POPreviousProdQty).toFixed(0);
                            $scope.TotalPOFirstProcessProdQty = parseFloat(response.data[0].POFirstProcessProductionQty).toFixed(0);
                            $scope.TotalPOProcessSequence = parseFloat(response.data[0].POProcessSequence).toFixed(0);
                            $scope.NewobjectSO.RemainingQty = $scope.RemainQty;
                            $scope.NewobjectSO.OrderQty = $scope.TotalSalesOrderQty;
                            $scope.NewobjectSO.BookedQty = $scope.TotalProductionBookingQty;
                            $scope.NewobjectSO.ActualPlannedQty = $scope.TotalActualPlannedQty;
                            $scope.NewobjectSO.ProcessPlanPercentage = $scope.TotalProcessPlanPercentage;
                            $scope.NewobjectSO.POQty = $scope.TotalPOQty;
                            $scope.NewobjectSO.ProcessPlanQty = $scope.TotalProcessPlanQty;
                            $scope.NewobjectSO.CurPOBalProd = $scope.TotalCurPOBalProd;
                            $scope.NewobjectSO.POPreviousProdQty = $scope.TotalPOPreviousProdQty;
                            $scope.NewobjectSO.POFirstProcessProductionQty = $scope.TotalPOFirstProcessProdQty;
                            $scope.NewobjectSO.POProcessSequence = $scope.TotalPOProcessSequence;
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
            var date = new Date();
            date.setDate(date.getDate() - 1);
            $scope.Yestarday = $filter('dateFiltering')(date);
            $scope.ProdDate = $filter('dateFiltering')(ProductionDate);
            if ($scope.ProdDate < $scope.Yestarday) {
                throw "Production Date must be allow only Yestarday's Date!";
            }
            if (new Date(ProductionDate) > new Date()) {
                throw "Production Date must be equal to current Date!";
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };
    $scope.RemainingQtyValue = null;
    $scope.ValidateProdQty = function (ProcessId, POId) {
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

            if ($scope.productionSummaryNew.ProductionBookingLevel === "MasterOrderItem") {
                var getRow = $filter("filter")($scope.MasterOrderItemValidateList, { "MasterOrderItemId": $scope.NewObject.MasterOrderItemId });
                if (getRow.length === 0) {
                    throw "MO Item not belongs to the selected PO please refresh and proceed.";
                }
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
    $scope.getProductionOrderPopUp = function (data) {
        $scope.NewObject = data.data;
        $scope.NewObject.MOIArticle = null;
        $scope.NewObject.SOArticle = null;
        $scope.NewObject.ProductCodeArticle = null;
        if (baseService.isUndefinedOrNull(data.data.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.ProductionOrderList = [];
        $http.get('Productions/ProductionSummary/GetProductionOrderDataListWC?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + data.data.WorkCenterMasterId + '&productionLevel=' + $scope.productionSummaryNew.ProductionBookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ToCloseAllowed=' + $scope.ToCloseAllowed)
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
        $scope.NewObject.Article = $event.data.Article;
        $scope.NewObject.IsPreDefineLotApplicable = $event.data.IsPreDefineLotApplicable;
        $scope.NewObject.LotProcessPlanQty = $event.data.LotProcessPlanQty;
        $scope.NewObject.ProductionVerification = $event.data.ProductionVerification;
        //$scope.NewObject.RemainingQty = $event.data.RemainingQty;
        $scope.GetTotalProductionBookingQty();
        //$scope.getArticle($scope.NewObject.ProductionOrderId);
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
        $scope.productionSummaryNew.SKUQty = 0;
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
        $scope.productionSummaryNew.SKUQty = 0;
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

    $scope.CompareMaxValue = 0;
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

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
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
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.SourceType)) {
                $scope.productionSummaryNew.SourceType = "PB";
            }

            
            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }
            if ($scope.NewObject.POProcessSequence != 1) {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.NewObject.POFirstProcessProductionQty) {
                    throw "Produced qty cannot more than the first process qty.";
                }
                else {
                    $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
                    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                        throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                    }
                    else {

                        if (parseFloat($scope.NewObject.POPreviousProdQty) < parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) {
                            throw "If Previous Process Booked Qty is less than  Produced and Booked Qty then Please enter remarks and inform to departmental head without fail!";
                        }
                        else {
                            if (parseFloat($scope.NewObject.POPreviousProdQty) < parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) {
                                $scope.productionSummaryNew.PPQFlag = true;
                            }
                            else {
                                $scope.productionSummaryNew.PPQFlag = false;
                            }
                        }
                    }
                }
            }
            else {
                if ($scope.NewObject.ProductionVerification != true) {
                    throw "You cannot booked more than Verified Process...";
                }
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.NewObject.ProcessPlanQty) {
                    throw "You cannot booked greater than Process Plan Qty.";
                }
                else {
                    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) == parseFloat($scope.NewObject.ProcessPlanQty)) {
                        $scope.productionSummaryNew.PPQFlag = true;
                    }
                    else {
                        if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) < parseFloat($scope.NewObject.ProcessPlanQty) && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) { $scope.productionSummaryNew.PPQFlag = true; }
                        else {
                            $scope.productionSummaryNew.PPQFlag = false;
                            throw "If  Booked Qty and Produced Qty is less than Process Plan Qty then Please enter remarks and inform to departmental head without fail!";
                        }
                    }
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
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    $scope.ValidateProdQty($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.ProductionOrderId);
                    for (var i = 0; i < $scope.wcList.length; i++) {
                            $scope.wcList[i].ClickRow = false;
                    }
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
            ShowResult(ex, 'failure');
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
        //if ($scope.NewObject.BookingLevel === 'MasterOrderItem') {
        //    if (baseService.isUndefinedOrNull($scope.NewObject.MasterOrderItemId)) {
        //        throw "Select MO Item please.";
        //    }
        //    $scope.getMasterOrderValidateView($scope.NewObject.WorkCenterMasterId, $scope.NewObject.BookingLevel, $scope.NewObject.ProductionOrderId);
        //}
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

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
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
                $scope.productionSummaryNew.SKUQty = $scope.ProdQty;
                $scope.productionSummaryNew.SourceType = "SKU";
            }
            if ($scope.IsSKU1 || $scope.IsSKU2 || $scope.IsSKU3) {
                if ($scope.ProdQty === 0) {
                    throw "SKU Qty is required.";
                }
            }

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.RemainQty) < 0) {
            //        throw "Order Quantity dosen't available.";
            //    }
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.TotalSalesOrderQty) <= parseFloat($scope.TotalProductionBookingQty) + parseFloat($scope.productionSummaryNew.Quantity)) {
            //        throw " less than Order Quantity.";
            //    }
            //}

            if ($scope.NewObject.POProcessSequence != "1") {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.NewObject.POFirstProcessProductionQty) {
                    throw "Produced qty cannot more than the first process qty.";
                }
                else {
                    $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
                    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                        throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                    }
                    else {

                        if (parseFloat($scope.NewObject.POPreviousProdQty) < parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) {
                            throw "If Previous Process Booked Qty is less than  Produced and Booked Qty then Please enter remarks and inform to departmental head without fail!";
                        }
                        else {
                            if (parseFloat($scope.NewObject.POPreviousProdQty) < parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) {
                                $scope.productionSummaryNew.PPQFlag = true;
                            }
                            else {
                                $scope.productionSummaryNew.PPQFlag = false;
                            }
                        }
                    }
                }
            }
            else {
                if ($scope.NewObject.ProductionVerification != true)
                {
                    throw "You cannot booked more than Verified Process...";
                }
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.NewObject.ProcessPlanQty) {
                    throw "You cannot booked greater than Process Plan Qty.";
                }
                else {
                    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) == parseFloat($scope.NewObject.ProcessPlanQty)) {
                        $scope.productionSummaryNew.PPQFlag = true;
                    }
                    else {
                        if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) < parseFloat($scope.NewObject.ProcessPlanQty) && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks)) { $scope.productionSummaryNew.PPQFlag = true; }
                        else {
                            $scope.productionSummaryNew.PPQFlag = false;
                            throw "If  Booked Qty and Produced Qty is less than Process Plan Qty then Please enter remarks and inform to departmental head without fail!";
                        }
                    }
                }
            }

            //if ($scope.NewObject.POProcessSequence != 1) {
            //    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.NewObject.POFirstProcessProductionQty) {
            //        throw "Produced qty cannot more than the first process qty.";
            //    }
            //    else
            //    {

            //    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.POPreviousProdQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
            //        throw "If Current Total Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            //    }

            //    $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
            //    if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
            //        if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
            //            $scope.productionSummaryNew.PPQFlag = true;
            //        }
            //        else {
            //            throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
            //        }
            //    }
            //    else {
            //        $scope.productionSummaryNew.PPQFlag = false;
            //        }
            //    }
            //}


            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.RemainingQtyValue)) {
            //    throw "Produced Quantity should not be greater than RemainingQtyValue.";
            //}

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
            //    throw "Produced Quantity should not be greater than Balance Quantity.";
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
            //        throw "Produced Quantity should less than Order Quantity.";
            //    }
            //}


            $http({
                method: 'POST',
                url: $scope.saveUrlWC,
                data: {
                    "ps": $scope.productionSummaryNew,
                    "ProcessId": $scope.productionSummaryNew.ProcessId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.NewObject.Id = response.data.ProductionSummary.Id;
                    $scope.ValidateProdQty($scope.productionSummaryNew.ProcessId, $scope.productionSummaryNew.ProductionOrderId);
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        $scope.wcList[i].ClickRow = false;
                    }
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

            if ($scope.BookingLevel === 'MasterOrderItem') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
            }

            if ($scope.BookingLevel === 'SalesOrder') {
                $scope.productionSummaryNew.SalesOrderId = $scope.SOId;
            }

            if ($scope.BookingLevel === 'ProductCode') {
                $scope.productionSummaryNew.MasterOrderItemId = $scope.ItemId;
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

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.RemainQty) < 0) {
            //        throw "Order Quantity dosen't available.";
            //    }
            //}

            if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.POPreviousProdQty) && baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                throw "If Current Total Qty is greater than Previous Process Booked Qty then Please enter remarks and inform to departmental head without fail!";
            }

            $scope.CompareMaxValue = Math.max(parseFloat($scope.NewObject.ProcessPlanQty), parseFloat($scope.NewObject.POPreviousProdQty))
            if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue) {
                if (parseFloat($scope.NewObject.BookedQty) + parseFloat($scope.productionSummaryNew.Quantity) > $scope.CompareMaxValue && !baseService.isUndefinedOrNull($scope.productionSummaryNew.Remarks) && $scope.productionSummaryNew.ProcessId != 202028) {
                    $scope.productionSummaryNew.PPQFlag = true;
                }
                else {
                    throw "You cannot booked greater than Current Process Plan Qty or Previous Process Booked Qty.";
                }
            }
            else {
                $scope.productionSummaryNew.PPQFlag = false;
            }

            if (parseFloat($scope.productionSummaryNew.Quantity) < 0) {
                throw "Quantity should not be less than 0.";
            }

            //if (parseFloat($scope.productionSummaryNew.Quantity) > parseFloat($scope.NewObject.RemainingQty) && $scope.productionSummaryNew.Quantity > 0) {
            //    throw "Produced Quantity should not be greater than Balance Quantity.";
            //}

            //if ($scope.IsFirst == false) {
            //    if (parseFloat($scope.NewObject.RemainingQty) < 0 && $scope.productionSummaryNew.Quantity > 0) {
            //        throw "Produced Quantity should less than Order Quantity.";
            //    }
            //}

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
                url: 'Productions/productionSummary/DeleteMasterWC?id=' + master.data.Id,
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
    $scope.TotalPreviousProcessQty = 0;
    $scope.ProcessParaList = [];
    $scope.getProcessParaPopupPoPUp = function (data) {
        try {
            $scope.NewObject = data.data;

            if ($scope.NewObject.BookingLevel === 'MasterOrderItem') {
                if (baseService.isUndefinedOrNull($scope.NewObject.MasterOrderItemId)) {
                    throw "Select MO Item please.";
                }
                $scope.getMasterOrderValidateView($scope.NewObject.WorkCenterMasterId, $scope.NewObject.BookingLevel, $scope.NewObject.ProductionOrderId);
            }

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
            $scope.TotalPreviousProcessQty = $scope.NewObject.POPreviousProdQty;

            
                $scope.ProcessParaList = [];
                $http.get('Productions/ProductionSummary/GetProcessParaData?processId=' + $scope.productionSummaryNew.ProcessId + '&masterId=' + data.data.Id + '&ProductionOrderId=' + data.data.ProductionOrderId)
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
                url: 'Productions/ProductionSummary/Calculate',
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

            $http.get('Productions/ProductionSummary/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + data.data.WorkCenterMasterId + '&ProductionSummaryId=' + data.data.Id)
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

            $http.get('Productions/ProductionSummary/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + $scope.productionSummaryNew.workCenterId + '&ProductionSummaryId=' + $scope.ProductionSummaryId)
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
            $http.get('Productions/ProductionSummary/GetProcessDetentionData?processId=' + $scope.productionSummaryNew.ProcessId + '&entityId=' + $scope.productionSummaryNew.EntityId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&shiftId=' + $scope.productionSummaryNew.ProductionShiftId + '&workcenter=' + $scope.productionSummaryNew.workCenterId + '&ProductionSummaryId=' + $scope.ProductionSummaryId)
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
        $scope.NewobjectSO = data.data;
        $scope.getSalesOrder();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('show');
    }

    $scope.SalesOrderItemList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSalesOrder?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectSO.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectSO.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectSO.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SalesOrderItemList = resp.data;
        });
    }
    $scope.BookingLevel = null;
    $scope.SOId = null;
    $scope.selectSalesOrderItem = function (e) {
        $scope.NewobjectSO.SalesOrderId = e.data.SOId;
        $scope.NewobjectSO.SOArticle = e.data.Article;
        $scope.BookingLevel = $scope.NewobjectSO.BookingLevel;
        $scope.SOId = $scope.NewobjectSO.SalesOrderId;
        $scope.GetSalesOrderItemQty();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('hide');
    }

    $scope.getMasterOrderItemPopUp = function (data) {
        try {
            $scope.NewobjectMOI = data.data;
            for (var i = 0; i < $scope.wcList.length; i++) {
                if ($scope.wcList[i].WorkCenterMasterId == $scope.NewobjectMOI.WorkCenterMasterId) {
                    $scope.wcList[i].ClickRow = true;
                    break;
                }
            }
            var getRow = $filter("filter")($scope.wcList, { "ClickRow": true });
            if (getRow.length > 1) {
                throw "First complete pending record.";
            }
            else
            {
                $scope.getMasterOrderItem();
                angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
            }
           
        }
        catch (e)
        {
            ShowResult(e, 'failure');
        }
    }

    $scope.getMasterOrderItemViewPopUp = function (data) {
            $scope.NewobjectMOI = data.data;
            $scope.getMasterOrderItemView();
            angular.element(document.querySelector('#MasterOrderItemViewPopup')).modal('show');
    }


    $scope.MasterOrderItemList = [];
    $scope.getMasterOrderItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectMOI.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectMOI.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectMOI.ProductionOrderId + '&ToCloseAllowed=' + $scope.ToCloseAllowed,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemList = resp.data;
        });
    }

    $scope.MasterOrderItemViewList = [];
    $scope.getMasterOrderItemView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectMOI.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectMOI.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectMOI.ProductionOrderId + '&ToCloseAllowed=' + $scope.ToCloseAllowed,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemViewList = resp.data;
        });
    }

    $scope.MasterOrderItemValidateList = [];
    $scope.getMasterOrderValidateView = function (wcid, bl, poid) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + wcid + '&productionLevel=' + bl + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + poid + '&ToCloseAllowed=' + $scope.ToCloseAllowed,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemValidateList = resp.data;
        });
    }

    //$scope.refreshTemplateMOItem = function (args) {
    //    $("#Mheadchk").ejCheckBox({ "change": CheckBoxSelectAllMOItem });
    //};
    //function CheckBoxSelectAllMOItem(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#MasterOrderItemGrid").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.MasterOrderItemList.length; i++) {
    //            $scope.MasterOrderItemList[i].Flag = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].Flag = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#MasterOrderItemGrid").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //};

    $scope.ItemId = null;
    $scope.selectMasterOrderItem = function (data) {
        try {

            //$scope.NewobjectMOI.MasterOrderItemId = getRow[0].MasterOrderItemId;
            //$scope.NewobjectMOI.MOIArticle = getRow[0].Article;
            $scope.NewobjectMOI.MasterOrderItemId = data.data.MasterOrderItemId;
            $scope.NewobjectMOI.MOIArticle = data.data.Article;
            $scope.BookingLevel = $scope.NewobjectMOI.BookingLevel;
            $scope.ItemId = $scope.NewobjectMOI.MasterOrderItemId;
            $scope.GetMasterOrderItemQty();
            angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');

        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.getProductCodePopUp = function (data) {
        $scope.NewobjectPC = data.data;
        $scope.getProductCode();
        angular.element(document.querySelector('#ProductCodePopup')).modal('show');
    }

    $scope.ProductCodeList = [];
    $scope.getProductCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProductCode?entityid=' + $scope.productionSummaryNew.EntityId + '&workCenterMasterId=' + $scope.NewobjectPC.WorkCenterMasterId + '&productionLevel=' + $scope.NewobjectPC.BookingLevel + '&processId=' + $scope.productionSummaryNew.ProcessId + '&ProductionOrderId=' + $scope.NewobjectPC.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductCodeList = resp.data;
        });
    }

    $scope.selectProductCode = function (e) {
        $scope.NewobjectPC.MasterOrderItemId = e.data.MOIId;
        $scope.NewobjectPC.ProductCodeArticle = e.data.Article;
        $scope.BookingLevel = $scope.NewobjectPC.BookingLevel;
        $scope.ItemId = $scope.NewobjectPC.MasterOrderItemId;
        $scope.GetProductCodeItemQty();
        angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
    }
}