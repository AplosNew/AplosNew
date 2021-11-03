"use strict";
nonFinancialMaterialOpeningBalancePostController.$inject = ["accountService", "cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller"];
function nonFinancialMaterialOpeningBalancePostController(accountService, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Opening Balance Journal Voucher";
    $scope.Action = "Save";
    $scope.index = -1;
    $scope.voucherDetailList = [];
    $scope.voucherDetailCurrencyList = [];
    $scope.voucherList = [];
    $scope.isCrBankAmount = false;
    $scope.isDrBankAmount = false;
    $scope.currencyDisable = false;
    $scope.isAdvance = true;
    $scope.postUrl = "accounts/OpeningBalance/PostNonFinancialMaterialOB";
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $controller("cashBaseController", { $scope: $scope, $http: $http });

    $scope.voucherDetailList = [];
    $scope.searchvoucherList = [
        {
            "name": "VoucherNo",
            "value": "VoucherNo"
        },
        {
            "name": "VoucherDate",
            "value": "VoucherDate"
        },
        {
            "name": "Doc Ref No",
            "value": "DocRefNo"
        },
        {
            "name": "Voucher Type",
            "value": "VoucherType"
        }
    ];

    $scope.voucherListParameters = {
        limit: 10,
        offset: 0,
        order: "DESC",
        sort: "DocRefNo",
        searchBy: "DocRefNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    baseService.init("Accounts/OpeningBalance/GetNonFinancialMaterialOBPostedList", null, null, "DESC", "PostingDate DESC, DocRefNo", "PostingDate");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.voucherList = result.Rows;
                $scope.voucherListParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.voucher = {
        Id: null,
        PartyId: null,
        PartyName: null,
        PartyType: 'Material',
        CurrencyId: null,
        CompanyCurrencyRate: 1,
        Type: null,
        VoucherNo: null,
        VoucherDate: $filter("dateFiltering")(Date.now()),
        PostingDate: null,
        DocDate: null,
        DocRefNo: null,
        Amount: 0,
        Narration: null,
        EmployeeTransactionTypeName: null,
        IsPark: false,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        FAType: null
    };

    $scope.voucherDetail = {
        Id: null,
        GLGeneralInfoId: null,
        BudgetMasterId: null,
        ActivityId: null,
        COAICode: null,
        AccountTypeId: null,
        CurrencyId: null,
        DocRefNo: null,
        DrAmount: null,
        CrAmount: null,
        Narration: null,
        BankMasterId: null,
        CashMasterId: null,
        PartyId: null,
        PartyPlantId: null,
        TransactionTypeId: null,
        FAType: null,
        DrDisable: false,
        CrDisable: false,
        CashCurrencyId: null,
        BankCurrencyId: null,
        BankAmount:null
    };

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
        $scope.getCutOffDate();
    });


    $scope.getOBDetailList = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetOBAdvanceJournalDetail?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.voucherDetailList = response.data.Rows;
            //angular.forEach($scope.voucherDetailList, function (item, i) {
            //    item.DocDate = $filter('dateFiltering')(item.DocDate);
            //    if (!baseService.isUndefinedOrNull(item.CompanyId)) {
            //        $scope.companyChange(item.CompanyId);
            //        if (!baseService.isUndefinedOrNull(item.PlantId)) {
            //            $scope.plantChange(item.PlantId, item.CompanyId);
            //        }
            //    }
            //});
        });
    };

    $scope.plantChange = function (plantId, companyId, row) {
        if (baseService.isUndefinedOrNull(plantId) || baseService.isUndefinedOrNull(companyId)) {
            return;
        }
        $scope.interEntityList = [];
        cboService.getCboEntityPlantWise(null, companyId, plantId, function (result) {
            $scope.interEntityList = result;
        });
        $scope.getPlantInfo(plantId, row);
    };

    $scope.plant = null;
    $scope.getPlantInfo = function (plantId, row) {
        if (!baseService.isUndefinedOrNull(plantId)) {
            $scope.plant = $filter("filter")($scope.interplantList, { "PlantId": plantId });
            if ($scope.plant.length) {
                if (!baseService.isUndefinedOrNull($scope.plant[0].PartyId)) {
                    row.PartyId = $scope.plant[0].PartyId;
                    row.PartyPlantId = $scope.plant[0].PartyPlantId;
                }
                else {
                    row.PartyId = null;
                    row.PartyPlantId = null;
                }
            }
        }
        else
            $scope.plant = null;
    };

    $scope.companyChange = function (id) {
        $scope.interEntityList = [];
        cboService.getCboInterPlant(null, id, null, function (result) {
            $scope.interplantList = result;
        });
    };

    $scope.Get = function (data) {
        $scope.voucher.Id = data.Id;
        $scope.voucher.OpeningBalanceId = data.OpeningBalanceId;
        $scope.voucher.PostingDate = $filter("dateFiltering")(data.PostingDate);
        $scope.voucher.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.voucher.DocRefNo = data.DocRefNo;
        $scope.voucher.Narration = data.Narration;
        $scope.voucher.CurrencyId = data.CurrencyId;
        $scope.voucher.EntityId = data.EntityId;
        $scope.voucher.AddedBy = data.AddedBy;
        $scope.voucher.IsPark = data.IsPark;
        $scope.voucher.AddedDate = data.AddedDate;
        $scope.voucher.AddedFromIP = data.AddedFromIP;
        $scope.currencyDisable = true;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getOBDetailList($scope.voucher.Id);
    };



    $scope.removeRow = function (index) {
        $scope.voucherDetailList.splice(index, 1);
    };


    $scope.checkDrAmount = function (index) {
        if ($scope.voucherDetailList[index].DrAmount > 0) {
            $scope.voucherDetailList[index].CrAmount = null;
        }
    };

    $scope.checkCrAmount = function (index) {
        if ($scope.voucherDetailList[index].CrAmount > 0) {
            $scope.voucherDetailList[index].DrAmount = null;
        }
    };

    $scope.getEntityCboByCostCenter = function (costCenterId) {
        $scope.voucherDetail.CostCenterName = $("#costCenterId option:selected").text();
        $scope.voucherDetail.CostCenterId = costCenterId;
        cboService.getCboEntityByCostCenter(costCenterId, function (result) {
            $scope.costCenterEntityList = result;
        });
    };

    $scope.SelectedCostCenterEntityItem = function (id) {
        $scope.voucherDetail.EntityName = $("#costcenterentityId option:selected").text();
        $scope.voucherDetail.EntityId = id;
    };

   
    $scope.Clear = function () {
        $scope.Action = "Save";
        $scope.voucher.Active = true;
        $scope.voucher.Amount = 0;
        $scope.voucher.PartyType= 'Material';
        $scope.voucher.DocRefNo = null;
        $scope.voucher.Narration = null;
        $scope.voucher.CompanyCurrencyRate = 1;
        $scope.voucher.CurrencyId = $scope.selectBaseCurrency();
        $scope.voucher.VoucherDate = $filter("date")(Date.now(), "dd-MMM-yyyy");
        $scope.currencyDisable = false;
        $scope.currencyExchangeRate = [];
        $scope.voucherDetailList = [];
    };


  
    $scope.voucherId = null;
    $scope.confirmPost = function (voucherId) {
        $scope.voucherId = voucherId;
        $scope.message_confirmation = "Are you sure to Post?";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };

    $scope.post = function () {
        $http({
            method: "POST",
            url: "accounts/OpeningBalance/PostNonFinancialMaterialOB",
            data: {
                "voucherVM": $scope.voucher,
                "voucherDetailVMList": JSON.stringify($scope.voucherDetailList)
            },
            dataType: 'JSON'
            , contentType: "application/json charset=utf-8"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.clear();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    // #region





    $scope.searchmMGLByList = [
        {
            "name": "Doc Ref No ",
            "value": "DocRefNo"
        },
        {
            "name": "Amount ",
            "value": "Amount"
        }
    ];

    $scope.mMGLListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "DocRefNo",
        searchBy: "DocRefNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetmMGLList = function () {
        $scope.GLUrl5 = "Accounts/OpeningBalance/GetNonFinancialMaterialMasterOB";
        $scope.GetMMGLData = function (pageno) {
            baseService.paginationBase($scope.GLUrl5, pageno, $scope.mMGLListParameters)
                .then(function (result) {
                    $scope.mMGLList = result.Rows;
                    $scope.mMGLListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetMMGLData();

    };

    $scope.closeMMGLListPopUp = function () {
        angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("hide");
    };

    $scope.closeMMGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#MaterialMasterOBPopUp")).modal("show");
        }
    };
    $scope.mMDetailList = [];
    $scope.getOBMaterialMasterDetail = function (id) {
        $http({
            method: "get",
            url: "accounts/openingbalance/GetMaterialMasterOBGL?openingBalanceId=" + id
        }).then(function successCallback(response) {
            $scope.mMDetailList = response.data;

            for (var i = 0; i < $scope.mMDetailList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.mMDetailList[i].ActivityId)) {
                    ShowResult("This Material has no Article!", "failure", "MaterialMasterOBPopUp");
                    return true;
                }
                if (baseService.isUndefinedOrNull($scope.mMDetailList[i].MaterialMasterId)) {
                    ShowResult("There is no Material", "failure", "MaterialMasterOBPopUp");
                    return true;
                }
                var getRowMM = $filter("filter")($scope.voucherDetailList, { "MaterialMasterId": $scope.mMDetailList[i].MaterialMasterId, "ArticleId": $scope.mMDetailList[i].ArticleId, "BudgetMasterId": $scope.mMDetailList[i].BudgetMasterId, "ActivityId": $scope.mMDetailList[i].ActivityId });
                if (!baseService.isUndefinedOrNull(getRowMM) && getRowMM.length > 0 && getRowMM[0].MaterialMasterId === $scope.mMDetailList[i].MaterialMasterId) {
                    ShowResult("This Material is already added!", "failure", "MaterialMasterOBPopUp");
                }
                else {

                    $scope.voucherDetail.BudgetMasterId = $scope.mMDetailList[i].BudgetMasterId;
                    $scope.voucherDetail.BudgetCode = $scope.mMDetailList[i].BudgetCode;
                    $scope.voucherDetail.BudgetName = $scope.mMDetailList[i].BudgetName;
                    $scope.voucherDetail.ActivityId = $scope.mMDetailList[i].ActivityId;
                    $scope.voucherDetail.ActivityCode = $scope.mMDetailList[i].ActivityCode;
                    $scope.voucherDetail.ActivityName = $scope.mMDetailList[i].ActivityName;
                    $scope.voucherDetail.MaterialMasterId = $scope.mMDetailList[i].MaterialMasterId;
                    $scope.voucherDetail.MaterialMasterName = $scope.mMDetailList[i].MaterialMasterName;
                    $scope.voucherDetail.ArticleId = $scope.mMDetailList[i].ArticleId;
                    $scope.voucherDetail.ArticleName = $scope.mMDetailList[i].ArticleName;
                    $scope.voucherDetail.Quantity = $scope.mMDetailList[i].Quantity;

                    $scope.voucherDetail.GLGeneralInfoId = $scope.mMDetailList[i].GLGeneralInfoId;
                    $scope.voucherDetail.GLGeneralInfoCode = $scope.mMDetailList[i].GLGeneralInfoCode;
                    $scope.voucherDetail.GLGeneralInfoName = $scope.mMDetailList[i].GLGeneralInfoName;

                    $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
                    $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
                    $scope.voucherDetail.Narration = $scope.voucher.Narration;
                    $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
                    $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
                    $scope.voucherDetail.CrAmount = null;
                    $scope.voucherDetail.DrAmount = $scope.mMDetailList[i].CompanyCurrencyAmountDr;
                    $scope.voucherDetail.DrDisable = true;
                    $scope.voucherDetail.CrDisable = true;
                    $scope.voucherDetail.OpeningBalanceId = $scope.mMDetailList[i].OpeningBalanceId;
                    $scope.voucherDetail.MaterialMasterOpeningBalanceDetailId = $scope.mMDetailList[i].MaterialMasterOpeningBalanceDetailId;
                    $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
                    $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
                    $scope.voucherDetail = {};
                }
            }

        });
    };


    $scope.setMMGLListSelected = function (data) {
        if (data.voucherNo == null) {
            $scope.getOBMaterialMasterDetail(data.Id);

        }
        $scope.closeMMGLListPopUp();

        // $scope.addMMRow(data);
    };
    $scope.addMMRow = function (data) {
        if (baseService.isUndefinedOrNull($scope.voucher.CurrencyId)) {
            ShowResult("Please select Currency!", "failure", "MaterialMasterOBPopUp");
            return true;
        }
        if (baseService.isUndefinedOrNull(data.ActivityId)) {
            ShowResult("This Material has no Article!", "failure", "MaterialMasterOBPopUp");
            return true;
        }
        if (baseService.isUndefinedOrNull(data.MaterialMasterId)) {
            ShowResult("There is no Material", "failure", "MaterialMasterOBPopUp");
            return true;
        }
        if ($scope.companyConfig.IsVoucherFromBudget)
            var getRowMM = $filter("filter")($scope.voucherDetailList, { "MaterialMasterId": data.MaterialMasterId, "ArticleId": data.ArticleId, "BudgetMasterId": data.BudgetMasterId, "ActivityId": data.ActivityId });

        if (!baseService.isUndefinedOrNull(getRowMM) && getRowMM.length > 0 && getRowMM[0].MaterialMasterId === data.MaterialMasterId) {
            ShowResult("This Material is already added!", "failure", "MaterialMasterOBPopUp");
        }
        else {
            $scope.voucherDetail.BudgetMasterId = data.BudgetMasterId;
            $scope.voucherDetail.BudgetCode = data.BudgetCode;
            $scope.voucherDetail.BudgetName = data.BudgetName;
            $scope.voucherDetail.ActivityId = data.ActivityId;
            $scope.voucherDetail.ActivityCode = data.ActivityCode;
            $scope.voucherDetail.ActivityName = data.ActivityName;
            $scope.voucherDetail.MaterialMasterId = data.MaterialMasterId;
            $scope.voucherDetail.ParticularName = data.MaterialMasterName;
            $scope.voucherDetail.ArticleId = data.ArticleId;
            $scope.voucherDetail.ArticleName = data.ArticleName;

            $scope.voucherDetail.GLGeneralInfoId = data.GLGeneralInfoId;
            $scope.voucherDetail.GLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.voucherDetail.GLGeneralInfoName = data.GLGeneralInfoName;

            $scope.voucherDetail.DocDate = $filter("dateFiltering")($scope.voucher.DocDate);
            $scope.voucherDetail.DocRefNo = $scope.voucher.DocRefNo;
            $scope.voucherDetail.Narration = $scope.voucher.Narration;
            $scope.voucherDetail.EntityId = $scope.voucher.EntityId;
            $scope.voucherDetail.PlantId = $scope.voucher.PlantId;
            $scope.voucherDetail.CrAmount = null;
            $scope.voucherDetail.DrAmount = data.CompanyCurrencyAmountDr;
            $scope.voucherDetail.DrDisable = true;
            $scope.voucherDetail.CrDisable = true;
            $scope.voucherDetail.OpeningBalanceId = data.OpeningBalanceId;
            $scope.voucherDetail.MaterialMasterOpeningBalanceDetailId = data.MaterialMasterOpeningBalanceDetailId;
            $scope.voucherDetail.PartyType = $scope.voucher.PartyType;
            $scope.voucherDetailList.splice(0, 0, $scope.voucherDetail);
            $scope.voucherDetail = {};
        }
    };

    
}