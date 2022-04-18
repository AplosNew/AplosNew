'use strict';
OrderCostingApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', 'cboService', '$window', 'fileReader'];
function OrderCostingApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Order Costing Approve';
    $scope.ModelList = [];
    $scope.path = 'Costings/OrderCostingApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";

    $scope.SelectedCostingStage = '';
    $scope.PopupMessage = '';
    $scope.ApproveCostingPopup = function (Stage) {

        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }


        $scope.SelectedCostingStage = Stage;

        if (Stage == 'QUICK') {
            if ($scope.ModelNew.isQuickCostingApproved == true) {
                ShowResult('Already approved for quick costing', "failure");
                return;
            }
            $scope.PopupMessage = 'Are you sure to approve the quick costing?';
        }
        if (Stage == 'PRE') {
            if ($scope.ModelNew.isPreCostingApproved == true) {
                ShowResult('Already approved for pre costing', "failure");
                return;
            }
            $scope.PopupMessage = 'Are you sure to approve the pre costing?';
        }
        if (Stage == 'PROCUREMENT') {
            if ($scope.ModelNew.isProcurementCostingApproved == true) {
                ShowResult('Already approved for procurement costing', "failure");
                return;
            }
            $scope.PopupMessage = 'Are you sure to approve the procurement costing?';
        }

        angular.element(document.querySelector('#ApproveCosting')).modal('show');
    }

    $scope.ApproveCosting = function () {


        if ($scope.SelectedCostingStage == 'QUICK') {
           
            $scope.ApprovaQuickCosting();
        }
        if ($scope.SelectedCostingStage == 'PRE') {
            
            $scope.ApprovaPreCosting();
        }
        if ($scope.SelectedCostingStage == 'PROCUREMENT') {
           
            $scope.ApprovaProcurementCosting();
        }

    }

    $scope.OrderCostingMasterTemplateId = '';
    $scope.ApprovaQuickCosting = function () {

        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }

        if ($scope.ModelNew.isQuickCostingApproved == true) {
            ShowResult('Already approved for pre costing', "failure");
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "ApproveQuickCosting",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelNew.isQuickCostingApproved = true;
            $scope.getData();
        });
    }
    $scope.ApprovaPreCosting = function () {
        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }
        if ($scope.ModelNew.isPreCostingApproved == true) {
            ShowResult('Already approved for pre costing', "failure");
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "ApprovePreCosting",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelNew.isPreCostingApproved = true;
            $scope.getData();
        });
    }
    $scope.ApprovaProcurementCosting = function () {
        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }
        if ($scope.ModelNew.isProcurementCostingApproved == true) {
            ShowResult('Already approved for Procurement costing', "failure");
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "ApproveProcurementCosting",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelNew.isProcurementCostingApproved = true;
            $scope.getData();
        });
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelNew = { CostingStage: null };
    $scope.Get = function (args) {

        $http({
            method: 'POST',
            url: $scope.path + "GetListItem",
            data: { Id: args.data.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelNew = response.data[0];

            if ($scope.ModelNew.SpecifyTo == 'Customer')
                $scope.IsCustomer = true;
            if ($scope.ModelNew.IsPercentage == true) {
                $scope.ModelNew.IsPercentage = 'true';
            }
            else {
                $scope.ModelNew.IsPercentage = 'false';
            }


            var str = $scope.ModelNew.FileName;
            if (!baseService.isUndefinedOrNull(str)) {
                var extention = str.substr(str.indexOf('.'));
                $scope.imageSrc = virtualPath.QuickCostingImagePath + '/' + $scope.ModelNew.Id + extention;

                $scope.filedata = $scope.ModelNew.FileName;
            }

            $scope.OrderCostingMasterTemplateId = $scope.ModelNew.Id;
            $scope.GetSOListForTemplate();
            //$scope.getBuyerData();
            $scope.onChengeProductMaster();


            //$scope.AssignSegmentByeDirectMaterial();
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };

    $scope.SOTemplateList = [];
    $scope.GetSOListForTemplate = function () {
        $scope.SOTemplateList = [];

        $http({
            method: 'POST',
            url: $scope.path + "GetSOListForTemplate",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                try {
                    response.data.DeliveryDate = new Date(response.data.DeliveryDate);
                } catch (e) {

                }
            }
            $scope.SOTemplateList = response.data;
        });
    }

    $scope.totalCost = 0;
    $scope.SumCostingValue = function () {
        $scope.totalCost = 0;
        if ($scope.QuickCostingDetailList.length > 0) {
            for (let i = 0; i < $scope.QuickCostingDetailList.length; i++) {
                if (!isNaN($scope.QuickCostingDetailList[i].CostingValue)) {
                    $scope.totalCost += $scope.QuickCostingDetailList[i].CostingValue;
                }
            }
        }
        $scope.SumNetProfitOfSelling();
    }

    $scope.netProfit = 0;
    $scope.gainOrLos = 0;
    $scope.SumNetProfitOfSelling = function () {
        $scope.netProfit = 0;
        $scope.gainOrLos = 0;
        $scope.netProfit = $scope.ModelNew.TargetSellingPrice - $scope.totalCost;
        $scope.gainOrLos = $scope.netProfit;
        $scope.SumNetProfitOfGross();
    }
    $scope.NetProfitofGross = 0;
    $scope.SumNetProfitOfGross = function () {
        $scope.NetProfitofGross = $scope.GrossProfit - $scope.gainOrLos;
    }
    $scope.GrossProfit = 0;
    $scope.MKTGainOrLoss = 0;
    $scope.NetProfit = 0;
    ``
    $scope.CalculateProfit = function () {

        if ($scope.ModelNew.IsPercentage != null && $scope.ModelNew.IsPercentage == 'true') {
            //Percentage
            if ($scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetSellingPrice - ($scope.ModelNew.TargetSellingPrice * (100 / (100 + $scope.ModelNew.TargetProfit)));
        }
        else {

            //Fixed
            if ($scope.ModelNew.TargetSellingPrice != NaN && $scope.ModelNew.TargetProfit != NaN)
                $scope.GrossProfit = $scope.ModelNew.TargetProfit;
        }
        $scope.SumNetProfitOfGross();
    }

    $scope.QuickCostingDetailList = [];
    $scope.QuickCostingItemList = [];
    $scope.Status = 0;
    $scope.onChengeProductMaster = function () {

        $http({
            method: 'GET',
            url: $scope.path + "GetQuickCostingDetailByProductMaster?ProductMasterId=" + $scope.ModelNew.ProductMasterId + "&CostingVersionMasterTemplateId=" + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QuickCostingDetailList = response.data.ComponentList;
            $scope.QuickCostingItemList = response.data.ItemList;

            $scope.SumCostingValue();
            $scope.CalculateProfit();


            $scope.MakeSummaryBySegment();
        });
    }

    $scope.NavigateToOrderCosting = function (args) {
        

        $scope.DirectMaterialList = [];
        $scope.toatalItemGrossConsumption = 0;
        $scope.totalItemGrossAmount = 0;

        $scope.OperationList = [];
        $scope.totalOperationValue = 0;

        $scope.DirectProcessList = [];
        $scope.totalDirectProcessAmount = 0;

        $scope.SalesExpenseList = [];
        $scope.totalSalesExpenseAmount = 0;

        $scope.ValueLossList = [];
        $scope.totalValueLossAmount = 0;

        $scope.ProfitList = [];


        $scope.DirectProcurementCostingMaterialList = [];
        $scope.OperationProcurementCostingList = [];
        $scope.DirectProcessProcurementCostingList = [];
        $scope.SalesExpenseProcurementCostingList = [];
        $scope.ValueLossProcurementCostingList = [];
        $scope.ProfitProcurementCostingList = [];


        $scope.SelectedOrderCostingComponent = args;
        $scope.CostingComponentId = args.CostingComponentId;
        $scope.Segment = args.CostingSegment;
        if ($scope.Segment == 'DirectMaterial') {

            $scope.GetDirectCostingMaterialWithItemByComponentId();
        }
        else if ($scope.Segment == 'Operation') {


            $scope.GetOperationWithItemByComponentId();
        }
        else if ($scope.Segment == 'DirectProcess') {

            $scope.GetDirectProcessWithItemByComponentId();
        }
        else if ($scope.Segment == 'SalesExpense') {

            $scope.GetSalesExpenseWithItemByComponentId();
        }
        else if ($scope.Segment == 'ValueLoss') {

            $scope.GetValueLossWithItemByComponentId();
        }
        else if ($scope.Segment == 'Profit') {

            $scope.GetProfitWithItemByComponentId();
        }
        $scope.CalculateFinalCosting(null);
        $scope.CalculateFinalCostingProcurement(null);

    }

    $scope.GetDirectCostingMaterialWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectCostingMaterialWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectMaterialList = response.data.Pre;
            $scope.DirectProcurementCostingMaterialList = response.data.Procurement;

            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.GetOperationWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetOperationWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OperationList = response.data.Pre;
            $scope.OperationProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.GetDirectProcessWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetDirectProcessWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DirectProcessList = response.data.Pre;
            $scope.DirectProcessProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.GetSalesExpenseWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetSalesExpenseWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SalesExpenseList = response.data.Pre;
            $scope.SalesExpenseProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.GetValueLossWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetValueLossWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ValueLossList = response.data.Pre;
            $scope.ValueLossProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.ProfitList = [];
    $scope.GetProfitWithItemByComponentId = function () {
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetProfitWithItemByComponentId?costingComponentId=' + $scope.CostingComponentId + '&OrderCostingMasterTemplateId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProfitList = response.data.Pre;
            $scope.ProfitProcurementCostingList = response.data.Procurement;


            var elmnt = document.getElementById("CostingItemsEntry");
            elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
        });
    }

    $scope.totalItemGrossAmount = 0;
    $scope.toatalItemGrossConsumption = 0;

    $scope.CalculateItemValueByPerComponent = function () {


        $scope.totalItemGrossAmount = 0;
        $scope.toatalItemGrossConsumption = 0;
        //if ($scope.DirectMaterialList.length > 0) {
        for (var i = 0; i < $scope.DirectMaterialList.length; i++) {
            $scope.totalItemGrossAmount += $scope.DirectMaterialList[i].GrossAmount;
            $scope.toatalItemGrossConsumption += $scope.DirectMaterialList[i].GrossConsumption;

        }
        //}

        for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
            if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                $scope.QuickCostingDetailList[i].TotalGrossAmount = $scope.totalItemGrossAmount;

            }
        }
    };
    $scope.SummaryBySegmentList = [];
    $scope.MakeSummaryBySegment = function () {
        $scope.SummaryBySegmentList = [];
        var DistinctSegments = ej.DataManager($scope.QuickCostingDetailList).executeLocal(ej.Query().group("CostingSegment"));
        for (var s = 0; s < DistinctSegments.length; s++) {
            var ItemsBySegments = DistinctSegments[s].items; //ej.DataManager($scope.QuickCostingDetailList).executeLocal(ej.Query().where("CostingSegment", "equal", DistinctSegments[0].items[s]));
            var BuyerTarget = 0, CostingValue = 0, TotalGrossAmount = 0, TotalProcurementGrossAmount = 0;
            for (var i = 0; i < ItemsBySegments.length; i++) {
                BuyerTarget += ItemsBySegments[i].BuyerTarget;
                CostingValue += ItemsBySegments[i].CostingValue;
                TotalGrossAmount += ItemsBySegments[i].TotalGrossAmount;
                TotalProcurementGrossAmount += ItemsBySegments[i].TotalProcurementGrossAmount;
            }

            var tempData = { SegmentName: DistinctSegments[s].key, BuyerTarget: BuyerTarget, CostingValue: CostingValue, TotalGrossAmount: TotalGrossAmount, TotalProcurementGrossAmount: TotalProcurementGrossAmount };
            $scope.SummaryBySegmentList.push(tempData);
        }

        $scope.SumCostingValue();
    }
    $scope.CostingComponentId = '';
    $scope.Segment = '';
    $scope.CalculateFinalCosting = function (data) {

        //first try to push the data into main list
        try {
            for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
                if ($scope.QuickCostingItemList[i].Id == data.CostingItemId) {
                    if ($scope.Segment == "SalesExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.QuickCostingItemList[i].ValueType = data.Type;
                        $scope.QuickCostingItemList[i].Value = data.Value;
                    }
                    if ($scope.Segment == "PurchaseExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.QuickCostingItemList[i].ValueType = data.Type;
                        $scope.QuickCostingItemList[i].Value = data.Value;
                    }

                    if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectMaterial') {
                        data.GrossConsumption = (data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = data.GrossConsumption * data.Rate;
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.GrossConsumption * data.Rate;


                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;

                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectProcess') {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100) + data.Rate;

                        $scope.QuickCostingItemList[i].Rate = data.Rate;
                        $scope.QuickCostingItemList[i].Value = data.Value;

                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        totalPre += calValue("DirectProcess");

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }
                    else if ($scope.QuickCostingItemList[i].CostingSegment == 'PurchaseExpense') {
                        var totalPre = calValue("DirectMaterial");
                        totalPre += calValue("Operation");
                        totalPre += calValue("DirectProcess");

                        if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = data.Value;
                        }
                        else {
                            $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                    }

                }
            }
        } catch (e) {

        }


        try {
            for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {

                if ($scope.QuickCostingItemList[i].CostingSegment == 'DirectProcess') {
                    var totalPre = calValue("DirectMaterial");
                    totalPre += calValue("Operation");
                    $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100) + $scope.QuickCostingItemList[i].Rate;

                    // data.Amount = $scope.QuickCostingItemList[i].TotalGrossAmount;
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'SalesExpense') {
                    var totalPre = calValue("DirectMaterial");
                    totalPre += calValue("Operation");
                    totalPre += calValue("DirectProcess");

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }
                else if ($scope.QuickCostingItemList[i].CostingSegment == 'PurchaseExpense') {
                    var totalPre = calValue("DirectMaterial");
                    totalPre += calValue("Operation");
                    totalPre += calValue("DirectProcess");

                    if ($scope.QuickCostingItemList[i].ValueType == 'FIXED' || $scope.QuickCostingItemList[i].ValueType == 'Fixed') {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = $scope.QuickCostingItemList[i].Value;
                    }
                    else {
                        $scope.QuickCostingItemList[i].TotalGrossAmount = totalPre * ($scope.QuickCostingItemList[i].Value / 100);

                    }
                }

                //}
            }
        } catch (e) {

        }



        try {
            $scope.totalItemGrossAmount = 0;
            $scope.toatalOperationValue = 0;
            $scope.totalDirectProcessAmount = 0;
            $scope.totalSalesExpenseAmount = 0;
            $scope.totalPurchaseExpenseAmount = 0;
            $scope.TotalSegmentedValueByComponent = 0;


            for (var i = 0; i < $scope.QuickCostingDetailList.length; i++) {
                var TotalValue = 0;
                for (var k = 0; k < $scope.QuickCostingItemList.length; k++) {
                    if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.QuickCostingItemList[k].CostingComponentId) {
                        TotalValue += $scope.QuickCostingItemList[k].TotalGrossAmount;
                    }
                }
                $scope.QuickCostingDetailList[i].TotalGrossAmount = TotalValue;

                if ($scope.QuickCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                    $scope.TotalSegmentedValueByComponent = TotalValue;
                }
            }
        } catch (e) {

        }



    }
    $scope.TotalSegmentedValueByComponent = 0;

    $scope.TooltipModel = {};
    $scope.ShowToolTip = function (costingStage, SelectedData) {
        $scope.CostingStage = costingStage;
        $scope.TooltipModel = {};
        if (costingStage == 'PRE') {

            var ShowModel = {};

            if ($scope.Segment == 'DirectMaterial') {
                var saveList = ej.DataManager($scope.DirectProcurementCostingMaterialList).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));

                if (saveList.length > 0)
                    $scope.TooltipModel = Object.assign({}, saveList[0]);

                var purchaseDocument = ej.DataManager($scope.PurchaseGroupList).executeLocal(ej.Query().where("Id", "equal", SelectedData.PurchaseGroupId));
                if (purchaseDocument.length > 0)
                    $scope.TooltipModel.PurchaseGroupId = purchaseDocument[0].UserName;


                angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
                return;
            }
            else if ($scope.Segment == 'Operation') {

                ShowModel = $scope.OperationProcurementCostingList;
            }
            else if ($scope.Segment == 'DirectProcess') {

                ShowModel = $scope.DirectProcessProcurementCostingList;
            }
            else if ($scope.Segment == 'SalesExpense') {

                ShowModel = $scope.SalesExpenseProcurementCostingList;
            }
            else if ($scope.Segment == 'ValueLoss') {

                ShowModel = $scope.ValueLossProcurementCostingList;
            }
            else if ($scope.Segment == 'Profit') {

                ShowModel = $scope.ProfitProcurementCostingList;
            }

            var saveList = ej.DataManager(ShowModel).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));
            if (saveList.length > 0)
                $scope.TooltipModel = Object.assign({}, saveList[0]);

        }
        else if (costingStage == 'PROCUREMENT') {

            var ShowModel = {};

            if ($scope.Segment == 'DirectMaterial') {
                var saveList = ej.DataManager($scope.DirectMaterialList).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));

                if (saveList.length > 0)
                    $scope.TooltipModel = Object.assign({}, saveList[0]);

                var purchaseDocument = ej.DataManager($scope.PurchaseGroupList).executeLocal(ej.Query().where("Id", "equal", SelectedData.PurchaseGroupId));
                if (purchaseDocument.length > 0)
                    $scope.TooltipModel.PurchaseGroupId = purchaseDocument[0].UserName;


                angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
                return;
            }
            else if ($scope.Segment == 'Operation') {

                ShowModel = $scope.OperationList;
            }
            else if ($scope.Segment == 'DirectProcess') {

                ShowModel = $scope.DirectProcessList;
            }
            else if ($scope.Segment == 'SalesExpense') {

                ShowModel = $scope.SalesExpenseList;
            }
            else if ($scope.Segment == 'ValueLoss') {

                ShowModel = $scope.ValueLossList;
            }
            else if ($scope.Segment == 'Profit') {

                ShowModel = $scope.ProfitList;
            }

            var saveList = ej.DataManager(ShowModel).executeLocal(ej.Query().where("CostingItemId", "equal", SelectedData.CostingItemId));
            if (saveList.length > 0)
                $scope.TooltipModel = Object.assign({}, saveList[0]);

        }




        angular.element(document.querySelector("#itemDetailPopUp")).modal("show");
    }

    $scope.NewCostingItemList = [];
    $scope.AddNewCostingItemPopUp = function () {
        try {
            if (angular.isUndefinedOrNull($scope.OrderCostingMasterTemplateId))
                throw 'Please save the costing master first';
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "GetCostingItemForSubMaterial",
                data: { CostingStage: $scope.CostingStage, OrderCostingMasterTemplateId: $scope.OrderCostingMasterTemplateId, costingComponentId: $scope.CostingComponentId, Segment: $scope.Segment },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.NewCostingItemList = response.data;
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.totalProcurementItemGrossAmount = 0;
    $scope.totalProcurementOperationValue = 0;
    $scope.totalProcurementDirectProcessAmount = 0;
    $scope.totalProcurementSalesExpenseAmount = 0;
    $scope.totalProcurementValueLossAmount = 0;
    $scope.TotalProcurementSegmentedValueByComponent = 0;
    $scope.CalculateFinalCostingProcurement = function (data) {

        //first try to push the data into main list
        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {
                if ($scope.OrderCostingItemList[i].Id == data.CostingItemId) {
                    if ($scope.Segment == "SalesExpense" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.Segment == "ValueLoss" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.Segment == "Profit" && data.CostingComponentId == $scope.CostingComponentId) {
                        $scope.OrderCostingItemList[i].ProcurementProcurementValueType = data.Type;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;
                    }
                    if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectMaterial') {
                        data.GrossConsumption = data.Consumption / ((100 - data.ValueLoss) / 100); //(data.Consumption * data.ValueLoss / 100) + data.Consumption;
                        data.GrossAmount = data.GrossConsumption * data.Rate;
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.GrossConsumption * data.Rate;


                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Operation') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;

                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {
                        //first push the 
                        $scope.OrderCostingItemList[i].ProcurementRate = data.Rate;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;


                        var totalPre = getProcurementFixedAmountDirectMaterial();

                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = (totalPre / ((100 - data.Value) / 100)) - totalPre;// totalPre * (data.Value / 100)
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount += data.Rate;

                        $scope.OrderCostingItemList[i].ProcurementRate = data.Rate;
                        $scope.OrderCostingItemList[i].ProcurementValue = data.Value;

                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {


                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {
                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }
                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);


                            //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }
                    else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {

                        if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = data.Value;
                        }
                        else {

                            var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                            var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);
                            if (totalPercent >= 100) {
                                data.Value = 0;
                            }

                            if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);
                            else
                                $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * (data.Value / 100);

                            //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                            //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * (data.Value / 100);

                        }
                        data.Amount = $scope.OrderCostingItemList[i].TotalProcurementGrossAmount;
                    }

                }
            }
        } catch (e) {

        }

        try {
            for (var i = 0; i < $scope.OrderCostingItemList.length; i++) {

                if ($scope.OrderCostingItemList[i].CostingSegment == 'DirectProcess') {

                    var totalPre = getProcurementFixedAmountDirectMaterial();

                    $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = (totalPre / ((100 - $scope.OrderCostingItemList[i].ProcurementValue) / 100)) - totalPre;//totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                    $scope.OrderCostingItemList[i].TotalProcurementGrossAmount += $scope.OrderCostingItemList[i].ProcurementRate;

                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'SalesExpense') {

                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {

                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'ValueLoss') {

                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {
                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                    }
                }
                else if ($scope.OrderCostingItemList[i].CostingSegment == 'Profit') {


                    if ($scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'FIXED' || $scope.OrderCostingItemList[i].ProcurementProcurementValueType == 'Fixed') {
                        $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = $scope.OrderCostingItemList[i].ProcurementValue;
                    }
                    else {
                        //var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);

                        //$scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        var totalPre = getProcurementFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalCurr = getProcurementCurrentFixedAmount($scope.OrderCostingItemList[i].ComponentSequence);
                        var totalPercent = getProcurementCurrentPercent($scope.OrderCostingItemList[i].ComponentSequence);

                        if ($scope.OrderCostingItemList[i].CalculationMethod.toUpperCase() == "CUMULATIVE")
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = totalPre * ($scope.OrderCostingItemList[i].ProcurementValue / 100);
                        else
                            $scope.OrderCostingItemList[i].TotalProcurementGrossAmount = ((totalPre + totalCurr) / ((100 - totalPercent) / 100)) * ($scope.OrderCostingItemList[i].ProcurementValue / 100);

                    }
                }

                //}
            }
        } catch (e) {

        }



        try {
            $scope.totalProcurementItemGrossAmount = 0;
            $scope.totalProcurementOperationValue = 0;
            $scope.totalProcurementDirectProcessAmount = 0;
            $scope.totalProcurementSalesExpenseAmount = 0;
            $scope.totalProcurementValueLossAmount = 0;
            $scope.TotalProcurementSegmentedValueByComponent = 0;


            $scope.CostingSummaryDataNew = Object.assign({}, $scope.CostingSummaryDataMain);

            for (var i = 0; i < $scope.OrderCostingDetailList.length; i++) {


                var TotalValue = 0;
                for (var k = 0; k < $scope.OrderCostingItemList.length; k++) {
                    if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.OrderCostingItemList[k].CostingComponentId) {
                        TotalValue += $scope.OrderCostingItemList[k].TotalProcurementGrossAmount;
                    }
                }
                $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount = TotalValue;

                if ($scope.OrderCostingDetailList[i].CostingComponentId == $scope.CostingComponentId) {
                    $scope.TotalProcurementSegmentedValueByComponent = TotalValue;
                }

                //$scope.CostingSummaryDataMain = { BuyerTotal: 0, OrderCostingValue: 0, OrderCostingValue, ProfitBuyerCosting: 0, ProfitOrderCosting: 0, ProfitOrderCosting: 0 };

                //calculation
                if ($scope.OrderCostingDetailList[i].CostingSegment == 'Profit') {
                    $scope.CostingSummaryDataNew.ProfitBuyerCosting += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.ProfitQuickCosting += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.ProfitOrderCosting += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProfitProcurementCosting += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;
                }
                else {
                    $scope.CostingSummaryDataNew.BuyerTotal += $scope.OrderCostingDetailList[i].BuyerTarget;
                    $scope.CostingSummaryDataNew.QuickCostingValue += $scope.OrderCostingDetailList[i].CostingValue;
                    $scope.CostingSummaryDataNew.OrderCostingValue += $scope.OrderCostingDetailList[i].TotalGrossAmount;
                    $scope.CostingSummaryDataNew.ProcurementCostingValue += $scope.OrderCostingDetailList[i].TotalProcurementGrossAmount;

                }


            }

            liveUpdateProcurementCostingComponent();

        } catch (e) {

        }



    }

    $scope.BackToOrderCostingComponent = function () {
        $scope.DirectMaterialList = [];
        $scope.OperationList = [];
        $scope.DirectProcessList = [];
        $scope.SalesExpenseList = [];
        $scope.ValueLossList = [];
        $scope.ProfitList = [];
        $scope.Segment = '';



        var elmnt = document.getElementById("costingMain");
        elmnt.scrollIntoView(false, { behavior: "smooth", block: "end", inline: "nearest" });
    }

    function calValue(segmentName) {
        var sum = 0;
        for (var i = 0; i < $scope.QuickCostingItemList.length; i++) {
            if ($scope.QuickCostingItemList[i].CostingSegment == segmentName)
                sum += $scope.QuickCostingItemList[i].TotalGrossAmount;
        }
        return sum;
    }

    $scope.GridSummaryBySegmentSummaryRows = [{
        title: "Total", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "BuyerTarget", dataMember: "BuyerTarget", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "CostingValue", dataMember: "CostingValue", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalProcurementGrossAmount", dataMember: "TotalProcurementGrossAmount", format: "{0:N2}" }],
        showCaptionSummary: true

    }];

    $scope.buyerList = [];
    $scope.getBuyerData = function () {
        $scope.buyerList = [];
        $http({
            method: 'GET',
            url: 'Costings/OrderCosting/GetBuyerDataByCostingMasterId?costingMasterId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.buyerList = response.data;
        });
    }

    $scope.tabCosting = 1;
    $scope.setTabCosting = function (newTab) {
        $scope.tabCosting = newTab;
    }
    $scope.isSetCosting = function (tabNum) {
        return $scope.tabCosting === tabNum;
    };

    //#region The Filters 

    $scope.filters = [];
    $scope.MachineMasterTransactionloadfilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'From', width: 20, headerText: "From", type: "string" },
                { field: 'To', width: 20, headerText: "To", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'Process', width: 20, headerText: "Process", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'DetentionType', width: 20, headerText: "Detention Type", type: "string" },
                { field: 'Shift', width: 20, headerText: "Shift", type: "string" },
                { field: 'ResponsiblePerson', width: 20, headerText: "ResponsiblePerson", type: "string" },
                { field: 'DetentionCategory', width: 20, headerText: "Detention Category", type: "string" },
                { field: 'DetentionSubCategory', width: 20, headerText: "Detention Sub Category", type: "string" },
                { field: 'Avoidable', width: 20, headerText: "Avoidable/Unavoidable", type: "string" },
                { field: 'Criticality', width: 20, headerText: "Criticality", type: "string" },

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
    $scope.MachineMasterTransactionloadfilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#filters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "EntityId", "Value": getString(fl, "EntityId") });
        parameters.push({ "Key": "ProcessId", "Value": getString(fl, "ProcessId") });
        parameters.push({ "Key": "DepartmentId", "Value": getString(fl, "DepartmentId") });
        parameters.push({ "Key": "DetentionId", "Value": getString(fl, "DetentionId") });
        parameters.push({ "Key": "ShiftId", "Value": getString(fl, "ShiftId") });
        parameters.push({ "Key": "ResponsiblePersonId", "Value": getString(fl, "ResponsiblePersonId") });

        $scope.parameters = parameters;
    }

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }
    //#endregion

}