'use strict';
JobWorkReceiveBillingController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'factoryService'];
function JobWorkReceiveBillingController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, factoryService) {
    //$scope.ToDoFilePath = virtualPath.JobWorkValueAddedContract;
    //$scope.ToDownloadFilePath = virtualPath.JobWorkTransformationContract;
    $rootScope.title = 'Receive Billing';
    $scope.Action = 'Save';
    $scope.ContractList = [];
    $scope.masterList = [];
    $scope.IssueTypeList = [];
    $scope.IndividualReportList = [];
    $scope.GateEntryNoList = [];
    $scope.GateEntryList = [];
    $scope.TransformationTypeList = [];
    $scope.EntityList = [];
    $scope.MaterialLocationList = [];
    $scope.GriddataMaster = [];
    $scope.path = 'JobWork/JobWorkReceiveBilling/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

    //////// Drop Down

    var d = new Date();
    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        Type: null,
        InvoiceNo: null,
        InvoiceDate: null,
        JWTransformationPurchaseOrderId:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ReceiptVAModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ByWhomId: null,
        DocumentReferenceNo: null,
        DocumentDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        InvoiceNo: null,
        InvoiceDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        GateEntryNoId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmployeeCode: null,
        ResponsiblePerson: null,

    };
    $scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);

    $scope.ReceiptTransformationModelTemp = {

        Id: null,
        GRNDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PartyId: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , CutOffDate: null
        , MaterialStorageId: null
        , CurrencyId: null
        , BaseCurrencyId: null
        , ToCurrencyRate: 0
        , PaymentTermId: null
        , BaseOnDueDate: null
        , BaseNoOfDays: null
        , MatureDate: null
        , DocRefNo: null
        , DocDate: null

        , EntryDate: null
        , FixedAssetOrInventory: 'Inventory'
        , PODepended: false
        , AlongwithInvoice: true
        , InvoiceNo: null
        , InvoiceDate: null
        , IsNonCreditable: false
        , IsNonVendor: false
        , TaxApplicable: null
        , IsTaxApplicable: false
        , IsTaxApplicableChangeable: false
        , PartyType: 'Vendor'
        , Reason: null
        , EmployeeId: null
        , NoteForAccounts: null
        , OrderSpecific: 'No'
        , IsFOC: false
        , ContractId: null
        , OrderSpecific: 'No'
        , PurchaseLCId: null
        , CustomerName: null
        , PaymentMode: null
        , ContractNo: null
        , LCRef: null
        , CheckedByStatusForNoti: null
        , ApprovedByStatusForNoti: null
        , labelCheckAndApproved: null
        , FromPlantId: null
        , TaxOption: 'Yes'
        , TaxOptionMat: 'Yes'
        , TaxOptionService: 'Yes'
        , TaxOptionServiceModify: 'Yes'
        , TaxOptionAddiTax: 'Yes'
        , ByWhomId: null
        , GateEntryNo: null
        , EmployeeCode: null
        , ResponsiblePerson: null
        , ByWhomEmployeeId: null
        , TransformationContractId: null
    };
    $scope.ReceiptTransformation = Object.assign({}, $scope.ReceiptTransformationModelTemp);

    $scope.ShowContractPopUp = function () {
       
        debugger;
        $scope.ModelNew.Type = "ValueAdded";
        $http({
            method: 'POST',
            url: $scope.path + "GetContractList",
            data: { column: $scope.searchBy, value: $scope.search, Type: $scope.ModelNew.Type },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ContractList = response.data;

            angular.element(document.querySelector("#ContractPopUp")).modal("show");
        });
    };

    $scope.CloseContractPopUp = function () {
        angular.element(document.querySelector("#ContractPopUp")).modal("hide");
        
    }

    $scope.SetContractData = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

   
    $scope.GetData = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkReceiveBilling/GetJWReceiveBillingData'
        }).then(function successCallback(response) {
            $scope.masterList = response.data;
          
        });
    }
    $scope.GetData();

    $scope.GetDetailData = function (masterId) {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkReceiveBilling/GetJWReceiveBillingDetailData?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.JWPOList = response.data;

        });
    }

    $scope.Get = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        $scope.GetDetailData($scope.ModelNew.Id);
        $scope.GetJWGRNDataChecking($scope.ModelNew.JWTransformationPurchaseOrderId);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    // #region checkbox all

    $scope.refreshJWPOTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridJWPO").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.JWPOList.length; i++) {
                $scope.JWPOList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridJWPO").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.SetOutSourcePO = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Transformation = Object.assign({}, args.data);
        $scope.ModelNew.JWTransformationPurchaseOrderId = $scope.ModelNew.JWTransformationPurchaseOrderId;
        var PId = $scope.Transformation.Id;
        var TabType = $scope.Transformation.TabType;
        $scope.TabTypeNew = $scope.Transformation.TabType;
        $scope.ReceiptTransformation.TransformationContractId = $scope.Transformation.Id;
        if ($scope.ModelNew.TabType == "Transformation") {
            $scope.GetJWGRNDataChecking($scope.ModelNew.JWTransformationPurchaseOrderId);
            $scope.ShowJWPOPopUp($scope.ModelNew.JWTransformationPurchaseOrderId);
        }
        else {
            $scope.GetReceiptVAChildData();
        }
        angular.element(document.querySelector("#ContractPopUp")).modal("hide");
    };

    $scope.GetJWGRNDataChecking = function (contractId) {
        $scope.GriddataMaster = [];
        $scope.lst = [];
        if ($scope.GRNbyPOCheckStatus === "ForChecked") {
            $scope.GRNbyPOCheckStatus = "ForChecked";
        }
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'JobWork/JobWorkReceiveBilling/GetInventoryReceiveByTransformationContractId?contractId=' + contractId,
        }).then(function successCallback(response) {
            $scope.GriddataMaster = response.data;
            $scope.GRNListDetails();
        });
    };

    $scope.lst = [];
    $scope.GRNListDetails = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'Products/GoodsReceiveNote/JWGRNDetailsData'
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            //$scope.detailgrid($scope.lst);
            window.lst = response.data;
        });
    }

    $scope.detailTemp = "#tabGridContents";
    //$scope.detailgrid = "detailGridData(e)";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager(window.lst).executeLocal(ej.Query().where("InventoryReceiveId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,
            columns: ["MaterialName", "Article", "SKU1", "SKU2", "SKU3", "TransactionQty", "TransactionUoM", "TransactionRate", "TotalMaterialTranAmount", "CurrencyName","MaterialFor"]
        });
        e.detailsElement.find(".tabcontrol").ejTab();


    }

    $scope.JWPOList = [];
  
    
    $scope.ShowJWPOPopUp = function (contractId) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'JobWork/JobWorkReceiveBilling/GetInventoryReceiveDetailByOutSourcePO?contractId=' + contractId,
        }).then(function successCallback(response) {
            $scope.JWPOList = response.data;
        });
    }

    //$scope.SelectedJWPOList = [];
    //function MakeData() {
    //    for (var i = 0; i < $scope.JWPOList.length; i++) {
    //        if ($scope.JWPOList[i].Flag == true) {
    //            if (checkExists($scope.SelectedJWPOList, $scope.JWPOList[i].InventoryReceiveDetailId) === false) {
    //                var ob = {};
    //                ob.Id = null;
    //                ob.InventoryReceiveDetailId = $scope.JWPOList[i].InventoryReceiveDetailId;
    //                ob.InventoryReceiveId = $scope.JWPOList[i].InventoryReceiveId;
    //                ob.ReceiveQty = $scope.JWPOList[i].ReceiveQty;
    //                ob.OrderQty = $scope.JWPOList[i].OrderQty;
    //                ob.BillingQty = $scope.JWPOList[i].BillingQty;
    //                ob.BalanceQty = $scope.JWPOList[i].BalanceQty;
    //                ob.JWTCMId = $scope.JWPOList[i].JWTCMId;
    //                ob.JWTCMDId = $scope.JWPOList[i].JWTCMDId;

    //                $scope.SelectedJWPOList.push(ob);

    //            }
    //            else {
    //                throw "This PO " + $scope.JWPOList[i].InventoryReceiveDetailId + " is already taken.";
    //            }
    //        }
    //    }
    //}

    //function checkExists(list, id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].InventoryReceiveDetailId === id) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //$scope.CloseJWPOPopUp = function () {
    //    try {
    //        MakeData();
    //        angular.element(document.querySelector("#JWPOPopUp")).modal("hide");
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}


    $scope.calculateBalance = function (data) {
        data.BalanceQty = data.ReceiveQty - data.BillingQty;
        data.Amount = data.BillingQty * data.MaterialTranRate;
        var gridObj = $("#GridJWPO").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();
    }

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.GriddataMaster = [];
        $scope.lst = [];
        $scope.JWPOList = [];
        $scope.SelectedJWPOList = [];
        $scope.ReceiptVA = Object.assign({}, $scope.ReceiptVAModelTemp);
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    $scope.Action = 'Save';

    $scope.Save = function () {
        try {
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'JobWork/JobWorkReceiveBilling/Create',
                    data: {
                        'master': $scope.ModelNew,'data': $scope.JWPOList
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };




}