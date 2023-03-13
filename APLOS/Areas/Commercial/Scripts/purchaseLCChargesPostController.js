'use strict';
purchaseLCChargesPostController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService', 'bankService', '$controller', "accountService"];
function purchaseLCChargesPostController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService, bankService, $controller, accountService) {
    $rootScope.title = "PurchaseLC Posting";
    $scope.Action = 'Save';
    $scope.path = 'Commercial/PurchaseLC/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChargesUrl = $scope.path + 'InsertPurchaseLCChargesPost';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.receiveTaxList = [];
    $scope.purchaseLC = {
        Id: null,
        ContractId: null,
        VendorId: null,
        BenificiaryBankId: null,
        OpeningBankMasterId: null,
        BenificiaryDescription: null,
        LeinDescription: null,
        LeinBankId: null,
        OrderSpecific: 'Yes',
        LCRef: null,
        LCDate: null,
        ExpiryDate: null,
        Amount: null,
        Type: null,
        Tenure: 0,
        FinalDestinationId: null,
        PortOfLandingId: null,
        IsClose: false,
        CurrencyId: null,
        Rate: 0
    };
    $scope.purchaseLCId = null;
    $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);

    $scope.purchaseLCCharges = {
        Id: null,
        PurchaseLCId: null,
        OverHeadTypeGLId: null,
        Remarks: null,
        ChargesValue: 0,
        CurrencyId: null,
        Rate: 0,
        VendorId: null
    };
    $scope.purchaseLCChargesNew = Object.assign({}, $scope.purchaseLCCharges);

    $scope.doubleClick = function (obj) {
        $scope.purchaseLC = obj.data;
        $scope.purchaseLCNew = Object.assign({}, $scope.purchaseLC);
        getPurchaseLCChargesData($scope.purchaseLCNew.Id);
        $scope.Action = 'Save';
    };

    $scope.bankMasterList = [];
    bankService.getBankMasterHouseBankCboList(function (result) {
        $scope.bankMasterList = result;
    });

    //$scope.currencyList = [];
    //cboService.getCboTransactionCurrencyByCompany('', function (result) {
    //    $scope.currencyList = [];
    //    $scope.currencyList = result;
    //    $scope.fileNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    //});

    $scope.voucherTypeList = [];
    cboService.getCboVoucherTypePuechaseLCOpeningChargesList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    $scope.purchaseLCChargesPostedList = [];
    $scope.getData = function () {
        $scope.purchaseLCChargesPostedList = [];
        $http.get("Commercial/PurchaseLC/GetPurchaseLCChargesPostedData")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.purchaseLCChargesPostedList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getData();

    $scope.chargesVoucherRow = [];
    $scope.getPurchaseLCChargesData = function () {
        $scope.chargesVoucherRow = [];
        $http({
            method: 'GET',
            url: 'Commercial/PurchaseLC/GetPurchaseLCChargesList'
        }).then(function successCallback(response) {
            $scope.purchaseLCChargesList = response.data;

            
        });
    };
    $scope.getPurchaseLCChargesData();

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];
        var filteredType = e.data["Type"];
        var Version = e.data["Version"];
        var data = [];//ej.DataManager($scope.purchaseLCChargesList).executeLocal(ej.Query().where("PurchaseLCId", "equal", filteredData, true).take(100));
        for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
            if ($scope.purchaseLCChargesList[i].PurchaseLCId === filteredData &&
                $scope.purchaseLCChargesList[i].Type === filteredType &&
                $scope.purchaseLCChargesList[i].Version === Version)
                data.push($scope.purchaseLCChargesList[i]);
        }
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data,

            // editSettings: { allowEditing: true },
            editSettings: { allowEditing: true, allowAdding: true, allowDeleting: true, editMode: "normal" },
            allowPaging: true,
            // columns: ['LCChargesType', 'ChargesValue', 'CurrencyCode', 'BankAmount','OpeningBankMaster'] /// show data
            cellEdit: function (args) {
                if (args.Rate === 1) // Can check the condition
                    args.cancel = true; // Disable the edit for a cell
            },
            // /// show data + edit
            columns: [
                { field: "Id", headerText: "Id", width: 10, allowEditing: false, isPrimaryKey: true, visible: false },
                { field: "PurchaseLCId", headerText: "PurchaseLCId", width: 10, allowEditing: false, visible: false },
                { field: "OverHeadType", headerText: "OverHead Type", width: 100, allowEditing: false },
                { field: "CurrencyCode", headerText: "Currency", width: 50, allowEditing: false },
                { field: "Rate", headerText: "Rate", width: 50, allowEditing: true, editType: ej.Grid.EditingType.Numeric },
                { field: "ChargesValue", headerText: "Charge Value", width: 50, allowEditing: false },
                { field: "BankAmount", headerText: "Bank Amount", width: 50, allowEditing: false },
                { field: "OpeningBankMaster", headerText: "Opening Bank", width: 100, allowEditing: false }

            ],
            actionComplete: complete,
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    };

    function complete(args) {
       
        if (args.requestType == "beginedit") {

            if (this.getCurrentViewData()[args.rowIndex]["Flag"] == 1)//checking condition for some column 
                //as the column edit type is numeric edit, we disable the numeric text box control 
                $("#detailGridRate").ejNumericTextbox("disable");//Grid - GridId, EmployeeID- field name of the column that is to be disabled 
        }
    }

    $scope.purchaseLCList = [];
    $scope.getSavedData = function () {
        $scope.purchaseLCList = [];
        $http.get("Commercial/PurchaseLC/GetPurchaseLCUnPostList")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.purchaseLCList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getSavedData();
    var LCOpeningDate = null;
    $scope.validation = function () {
        for (var j = 0; j < $scope.chargesVoucherRow.length; j++) {
            if (baseService.isUndefinedOrNull($scope.chargesVoucherRow[j].Rate)) {
                ShowResult("Rate Can not 0!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.chargesVoucherRow[j].OpeningBankMasterId)) {
                ShowResult("OpeningBankMasterId Can not Empty!", "failure");
                return true;
            }
            if (baseService.isUndefinedOrNull($scope.chargesVoucherRow[j].LCDate)) {
                ShowResult("LC Opening Date Can not Empty!", "failure");
                return true;
            }
            if (new Date($scope.chargesVoucherRow[j].LCDate) > new Date()) {
                ShowResult("LC Opening Date must be below or equal to current Date!", "failure");
                return true;
            }
            
        }
        
        for (var i = 0; i < $scope.ChargesList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ChargesList[i].Rate)) {
                ShowResult("Please Input Rate!", "failure");
                return true;
            }
        }
        return false;
    };
    $scope.ChargesList = [];
    $scope.Save = function () {
        try {
            $scope.ChargesList = [];
            LCOpeningDate = null;
            for (var i = 0; i < $scope.purchaseLCList.length; i++) {
                if ($scope.purchaseLCList[i].Active) {
                    for (var j = 0; j < $scope.purchaseLCChargesList.length; j++) {
                        if ($scope.purchaseLCList[i].Id === $scope.purchaseLCChargesList[j].PurchaseLCId &&
                            $scope.purchaseLCList[i].Type === $scope.purchaseLCChargesList[j].Type &&
                            $scope.purchaseLCList[i].Version === $scope.purchaseLCChargesList[j].Version){
                            $scope.ChargesList.push($scope.purchaseLCChargesList[j]);
                        }
                    }
                    var getRowDr = $filter("filter")($scope.chargesVoucherRow, { "OpeningBankMasterId": $scope.purchaseLCList[i].OpeningBankMasterId });
                    if (getRowDr.length == 0 && $scope.purchaseLCList[i].OpeningBankMasterId != null) {
                        $scope.chargesVoucherRow.push($scope.purchaseLCList[i]);
                    }
                    if (LCOpeningDate !== null && new Date(LCOpeningDate) !== Date($scope.purchaseLCList[i].LCDate)) {
                        ShowResult("LC Opening Date must be same Date!", "failure");
                        return true;
                    }
                    LCOpeningDate = $scope.purchaseLCList[i].LCDate;
                }
            }
            if ($scope.ChargesList.length == 0)
                throw "Select Charges.";
            //for (var i = 0; i < $scope.ChargesList.length; i++) {
            //    var getRowDr = $filter("filter")($scope.chargesVoucherRow, { "OpeningBankMasterId": $scope.ChargesList[i].OpeningBankMasterId });
            //    if (getRowDr.length == 0 && $scope.ChargesList[i].OpeningBankMasterId != null) {
            //        $scope.chargesVoucherRow.push($scope.ChargesList[i]);
            //    }

            //}
            if (!$scope.validation()) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveChargesUrl,
                        data: {
                            'voucherTypeId': $scope.VoucherTypeId,
                            'voucherRows': $scope.chargesVoucherRow,
                            'purchaseLCChargesList': $scope.ChargesList,
                            'taxDetailVMList': $scope.receiveTaxList,
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.purchaseLCNew.Id = response.data.Id;
                            $scope.getSavedData();
                            $scope.getPurchaseLCChargesData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
            
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Clear = function () {
        ClearFields();
    }
    function ClearFields() {
        $scope.purchaseLC = {};
        $scope.purchaseLCNew = { OrderSpecific: 'Yes', Id: null, Tenure: 0 };
        $scope.purchaseLCChargesList = [];
        $scope.Action = 'Save';
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.onClickReportDownloadWord = function (args) {

        var gridObj = $("#GridPost").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('Commercial/PurchaseLC/PurchaseLCChargesReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };

    $scope.commandPDF = [{
        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadWord
        }
    }];

    $scope.onClickReportDownloadExcel = function (args) {

        var gridObj = $("#GridPost").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.VoucherId)) return ShowResult('No Id found', 'failure');
        $window.open('Commercial/PurchaseLC/PurchaseLCChargesReport?reportFormat=' + reportFormat + '&&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.commandExcel = [{
        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickReportDownloadExcel
        }
    }];

    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });
   
    $scope.taxDel = function (Id, index) {
        if (Id === null) {
            $(this).remove();
            $scope.receiveTaxList.splice(index);
            return false;
        }
        else {
            $scope.message = 'Are you sure want to permanently delete this?';
            angular.element(document.querySelector('#confirmTaxCodeDelPopUp')).modal('show');
            $scope.metTaxId = Id;
            $scope.smetTaxIndex = index;
        }
    };

    $scope.removeTaxCodeRow = function () {
        try {
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/DeleteTaxRow?Id=' + $scope.metTaxId,
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.metTaxId = null;
                    $scope.receiveTaxList.splice($scope.smetTaxIndex, 1);
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'success');
        }
    };

   
    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            TaxAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null,
            PurchaseLCChargesId: null,
            PurchaseLCId:null
        };
        data.PurchaseLCId = $scope.purchaseLCId;
        $scope.receiveTaxList.push(data);
        
    };
    $scope.closeReceiveTaxPopUp = function () {
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('hide');
        //
    };

    $scope.lcChargesList = [];
    $scope.onClickTax = function (args) {
        $scope.lcChargesList = [];
        $scope.temppurchaseLCId = null;
        var gridObj = $("#popUpData").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        //$scope.temppurchaseLCId = data.PurchaseLCId;
        for (var i = 0; i < $scope.purchaseLCChargesList.length; i++) {
            if ($scope.purchaseLCChargesList[i].PurchaseLCId == data.PurchaseLCId) {
                $scope.lcChargesList.push($scope.purchaseLCChargesList[i]);
            }
        }
        $scope.ShowTaxCategory(data.PurchaseLCId);
    };

    function getTaxData(id) {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Commercial/PurchaseLC/GetPurchaseLCChargesTaxByLCId?purchaseLCId=' + id
        }).then(function successCall(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.receiveTaxList = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.HSNCode = response.data[0]['HSNCode'];
                }
                $scope.TaxAction = 'Update';
            }
        })
    }

    $scope.ShowTaxCategory = function (purchaseLCId) {
        $scope.purchaseLCId = purchaseLCId;
        getTaxData(purchaseLCId);
        angular.element(document.querySelector('#receiveTaxPopUp')).modal('show');

    }

    $scope.commandTax = [{
        type: "details", buttonOptions: {
            text: "Tax",
            width: "50",
            height: "20",
            click: $scope.onClickTax
        }
    }];

    $scope.Postdelete = function (purchaseLCId, voucherId) {
        $http({
            method: "POST",
            url: 'commercial/PurchaseLC/DeletePostedPurchaseLCCharges',
            data: {
                "purchaseLCId": purchaseLCId, "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                //$scope.getData();
                //$scope.Clear();
                $scope.purchaseLCId = null;
                $scope.VoucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.purchaseLCId = data.PurchaseLCId;
        $scope.VoucherId = data.VoucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

}






