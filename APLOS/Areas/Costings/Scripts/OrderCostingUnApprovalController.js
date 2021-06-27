'use strict';
OrderCostingUnApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter', 'cboService', '$window', 'fileReader'];
function OrderCostingUnApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $controller, $filter, $cboService, $window, fileReader) {
    $rootScope.title = 'Order Costing Un-Approve ';
    $scope.ModelList = [];
    $scope.path = 'Costings/OrderCostingUnApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.Action = 'Save';
    $scope.searchBy = "UserName"; $scope.searchBySO = "MasterOrderId"; $scope.searchSO = ''; $scope.search = "";

    $scope.OrderCostingMasterTemplateId = '';
    $scope.ApprovaQuickCosting = function () {
        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }
        if ($scope.ModelNew.isQuickCostingApproved == false) {
            ShowResult('Already un-approved for pre costing', "failure");
            return;
        }
           
        $http({
            method: 'POST',
            url: $scope.path + "ApproveQuickCosting",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelNew.isQuickCostingApproved = false;
            $scope.getData();
        });
    }
    $scope.ApprovaPreCosting = function () {
        if ($scope.OrderCostingMasterTemplateId == '' || $scope.OrderCostingMasterTemplateId == null) {
            ShowResult('Select costing template first', "failure");
            return;
        }
        if ($scope.ModelNew.isPreCostingApproved == false) {
            ShowResult('Already un-approved for pre costing', "failure");
            return;
        }

        $http({
            method: 'POST',
            url: $scope.path + "ApprovePreCosting",
            data: { TemplateId: $scope.OrderCostingMasterTemplateId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelNew.isPreCostingApproved = false;
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

    $scope.ModelNew = [];
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
            var BuyerTarget = 0, CostingValue = 0, TotalGrossAmount = 0;
            for (var i = 0; i < ItemsBySegments.length; i++) {
                BuyerTarget += ItemsBySegments[i].BuyerTarget;
                CostingValue += ItemsBySegments[i].CostingValue;
                TotalGrossAmount += ItemsBySegments[i].TotalGrossAmount;
            }

            var tempData = { SegmentName: DistinctSegments[s].key, BuyerTarget: BuyerTarget, CostingValue: CostingValue, TotalGrossAmount: TotalGrossAmount };
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
            { summaryType: ej.Grid.SummaryType.Sum, textAlign: 'right', displayColumn: "TotalGrossAmount", dataMember: "TotalGrossAmount", format: "{0:N2}" }],
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
}