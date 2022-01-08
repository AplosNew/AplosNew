'use strict';
POMappingWithPIController.$inject = ['commonMessage', '$controller', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function POMappingWithPIController(commonMessage, $controller, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "PO Mapping With PI";
    $scope.Action = 'Save';
    $scope.fabricRollMasters = [];
    $scope.selectedGRNList = [];
    $scope.path = 'Commercial/POMappingWithPI/';
    //$scope.CostingPath = 'Costings/costingItem/';
    //$scope.getListUrl = $scope.path + 'getlist';
    //$scope.Deletepath = $scope.path + 'DeletePI';
    $scope.saveUrl = $scope.path + 'Save';

    $scope.PIHeadModel = {
        Id: null
        , PINo: null
        , PIDate: null
        , RefNo: null
        , RevisionNo: null
        , BuyerId: null
        , Buyer: null
        , CustomerId: null
        , Customer: null
        , Currency: null
        , Description: null
        , Remarks: null
        , Quantity: 0
        , UoM: null
        , DeliveryDate: null
        , Amount: 0
        , CurrencyId: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , PartyCode: null
        , CustomerName: null
        , PartyId: null
        , PartyAccountGroupId: null
        , IsPaymentTermChangeable: null
        , PaymentTermId: null
        , SumAmount: 0
    };
    $scope.PImodelNew = Object.assign({}, $scope.PIHeadModel);

    $scope.searchPIByList = [
        {
            name: 'Id',
            value: 'Id'
        },
        {
            name: 'PI No.',
            value: 'PINo'
        },
        {
            name: 'Ref No.',
            value: 'RefNo'
        },
        {
            name: 'PI Date',
            value: 'PIDate'
        },
        {
            name: 'Currency',
            value: 'CurrencyId'
        },
        {
            name: 'Buyer',
            value: 'BuyerId'
        },
        {
            name: 'Customer',
            value: 'CustomerId'
        },
        {
            name: 'Invoicing by Address',
            value: 'InvoicingByAddress'
        },
        {
            name: 'Delivery by Address',
            value: 'DeliveryByAddress'
        }
    ];
    $scope.POGridModel = {
        Id: null
        , PIMasterId: null
        , PIVersionId: null
        , MaterialGroupMasterId: null
        , Description: null
        , Quantity: 0
        , Rate: 0
        , UoMId: null
        , UoM: null
        , DeliveryDate: null
        , CurrencyId: null
        , Amount: 0

    };

    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.POGridModel));


    $scope.SubmitH = function (data) {
        try {
            var newObj = Object.assign({}, $scope.POGridModel);
            if (data != null) {
                newObj = {
                    Id: null
                    , PIMasterId: null
                    , PIVersionId: null
                    , MaterialGroupMasterId: null
                    , Description: null
                    , Quantity: 0
                    , Rate: 0
                    , UoMId: null
                    , UoM: null
                    , DeliveryDate: null
                    , CurrencyId: null
                    , Amount: 0
                }
            }
            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
    }


    $scope.PISearchBy = "Id";
    $scope.PISearch = "";
    $scope.PIGridList = [];
    $scope.LoadPISearchList = function () {
        $scope.PIGridList = [];
        try {
            $http({
                method: 'POST',
                url: $scope.path + "PIList",
                data: { 'column': $scope.PISearchBy, 'value': $scope.PISearch },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.PIGridList = [];
                $scope.PIGridList = response.data;
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.LoadPISearchList();


    $scope.GetAllVersionData = function () {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $http({
            method: 'GET',
            url: $scope.path + "GetAllVersionData?PIMasterId=" + $scope.PImodelNew.Id,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.SelectedPIVersion = null;
                $scope.DataList = [];
                $scope.DataList.push(Object.assign({}, $scope.PIGridModel));

                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    };

    $scope.LoadPISearchList();
    $scope.LastVersion = null;
    $scope.Get = function (args) {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $scope.PIMId = args.data.Id;
        $scope.PIVId = args.data.PIVersionId;
        $scope.SelectedPIVersion = args.data.PIVersionId;
        $scope.LastVersion = args.data.LastVersionNo;
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + args.data.Id + '&VersionId=' + args.data.PIVersionId,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
                $scope.PImodelNew = response.data.PIMaster[0];
                $scope.PIVersionModel = response.data.VarsionData;
                $scope.PImodelNew.VersionNo = $scope.LastVersion;
                $scope.DataList = response.data.ItemData;
                $scope.PIVersionModel.Id = $scope.PIVersionModel[0].Id;
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.VersionList = $scope.PIVersionModel;
            }

        });

    };

    $scope.MaterialModel = {
       
                    Id: null
                    , PIMaterialID: null
                    , PODetailId: null
                    , QuantityAtPIUoM: null
                    , PIUoMId: null
                    , POQuantity: 0
                    , POUoMId: null
         
    };
    $scope.ModelPO = {};
    $scope.PODataList = [];
    $scope.POPopUpHeader = {};
    $scope.GetPOPopUpNew = function (args) {
        //$scope.MaterialModel = args.data;
        $scope.POPopUpHeader = args.data;
        $scope.ModelPO = args.data;
        $scope.PIMaterialId = args.data.Id;
        $scope.PODataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetPODetailsData?MaterialGroupMasterId=" + args.data.MaterialGroupMasterId + '&PIMaterialId=' + $scope.PIMaterialId ,

        }).then(function (response) {
            $scope.PODataList = response.data.Polist;
        });
        angular.element(document.querySelector('#POPopUpNew')).modal('show');

    }

    $scope.closePOPopup = function () {
        angular.element(document.querySelector('#POPopUpNew')).modal('hide');
    }

    $scope.refreshPOMappingWithPI = function (args) {
        $("#POheadchk").ejCheckBox({ "change": CheckBoxSelectPOListWise });
    };

    function CheckBoxSelectPOListWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPacking").data("ejGrid").getFilteredRecords();
        if (baseService.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PODataList.length; i++) {
                $scope.PODataList[i].check = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPacking").data("ejGrid");
        gridObj.refreshContent();
        $scope.SummaryPOMappingWithPI();
    };

  
    $scope.Save = function () {
        try {
            var LIST = [];
            for (var i = 0; i < $scope.PODataList.length; i++) {
                if ($scope.PODataList[i].check) {
                    LIST.push($scope.PODataList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'PIMaterial': $scope.ModelPO, 'POList': LIST},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadPISearchList();
                    angular.element(document.querySelector('#POPopUpNew')).modal('hide');
                    $scope.GetMaterialGrid();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
           
        } catch (e) {
            ShowResult(e, 'failure')
        }

    };
    
    
    $scope.GetMaterialGrid = function () {
        //$scope.getHeader(args.data.Id, args.data.PIVersionId);
        $http({
            method: 'GET',
            url: $scope.path + "GetAllData?PIMasterId=" + $scope.PIMId + '&VersionId=' + $scope.PIVId,
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data)) {
              
                $scope.DataList = response.data.ItemData;
               
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
             
            }

        });

    };

  

    $scope.SumModel = {
        QTY: 0,
        Amount: 0
    };
    $scope.ASSSSDFG = function () {
        $scope.SumModel.QTY = 0;
        $scope.SumModel.Amount = 0;
        for (var i = 0; i < $scope.PODataList.length; i++) {
            if ($scope.PODataList[i].check) {
                $scope.SumModel.QTY += parseFloat($scope.PODataList[i].QuantityAtPIUoM);
                $scope.SumModel.Amount += parseFloat($scope.PODataList[i].POAmount);
            }
        }
        $scope.SumModel.QTY= parseFloat($scope.SumModel.QTY).toFixed(2);
        $scope.SumModel.Amount=parseFloat($scope.SumModel.Amount).toFixed(2);
    }

    $scope.summaryPO = [{
        title: "Total :", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "POAmount", dataMember: "POAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "QuantityAtPIUoM", dataMember: "QuantityAtPIUoM", format: "{0:N2}" }],
        showCaptionSummary: true

    }];


}
