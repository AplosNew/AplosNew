"use strict";
cashMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function cashMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Cash Master";
    $scope.Action = "Save";
    $scope.ContactAction = "Add Row";
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.cashMasters = [];
    $scope.path = "banks/cashmaster/";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = $scope.path + "getcashmasterlist";

    baseService.init($scope.getListUrl, null, null, null, "UserName", "UserName");
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.cashMaster.CompanyId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.cashMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.cashMaster = {
        Id: null,
        CompanyId: null,
        CompanyGroupId: null,
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        COAICode: null,
        COAIText: null,
        PlantId: null,
        PlantName: null,
        BudgetId: null,
        ActivityId: null,
        EntityId: null,
        EntityName: null,
        CurrencyId: null,
        Code: null,
        AccountTitle: null,
        OpeningDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
        ShortAccountNumber: null,
        AccountNumber: null,
        IsHouseBank: true,
        IsLimitApplicable: true,
        IsBeyondLimitTransactionAllowed: true,
        IsLoanAccount: false,
        LimitAmount: 0,
        EffectiveDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
        ReviewDate: $filter("date")(Date.now(), "dd-MMM-yyyy"),
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remarks: null,
        Description: null,
        Active: true
    };

    $scope.budgetList = [];
    function getBudget() {
        cboService.getBudgetMasterCboByCompanyAndGLId($scope.cashMaster.CompanyId, $scope.cashMaster.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }

    $scope.activityList = [];
    $scope.getActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.cashMaster.BudgetMasterId, function (result) {
            $scope.activityList = result;
        });
    };

    $rootScope.searchByList = [
        {
            "name": "Code",
            "value": "Code"
        },
        {
            "name": "Short Name",
            "value": "ShortName"
        },
        {
            "name": "User Name",
            "value": "UserName"
        },
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        }];

    $scope.companyList = [];
    $scope.currencyList = [];
    $scope.bankCategoryList = [];
    $scope.bankSubCategoryList = [];
    $scope.bankList = [];
    $scope.bankAccountTypeList = [];
    $scope.bankBranchList = [];

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.onCompanyChange = function (companyId) {
        cboService.getCboTransactionCurrencyByCompany(companyId, function (result) {
            $scope.currencyList = result;
        });
    };

    $scope.getCboEntityByPlant = function (companyId, plantId) {
        cboService.GetCboByPlantAdmin(null, companyId, plantId, function (result) {
            $scope.entityList = result;
        });
    };
    $scope.getCboPlantByCompany = function (companyId) {
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.onBankChange = function (item) {
        $http({
            method: "GET",
            url: "banks/bankbranch/getcbobybankid?bankid=" + item
        }).then(function successCallback(response) {
            $scope.bankBranchList = response.data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.cashMaster = $scope.cashMasters[$scope.index];
        $scope.cashMaster.OpeningDate = $filter("dateFiltering")($scope.cashMaster.OpeningDate, "dd-MMM-yyyy");
        $scope.cashMaster.EffectiveDate = $filter("dateFiltering")($scope.cashMaster.EffectiveDate, "dd-MMM-yyyy");
        $scope.cashMaster.ReviewDate = $filter("dateFiltering")($scope.cashMaster.ReviewDate, "dd-MMM-yyyy");
        $scope.getCboEntityByPlant($scope.cashMaster.CompanyId, $scope.cashMaster.PlantId);
        getBudget();
        $scope.getActivity();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.valuePassInDelModal = function (id, ContactPerson) {
        $scope.conid = id;
        $scope.message_confirmation = "Are you sure to delete [ " + ContactPerson + " ]";
        angular.element(document.querySelector("#confirmContactdelete")).modal("show");
    };

    $scope.removeContactMasterRow = function () {
        for (var i = 0; i < $scope.contactMasters.length; i++) {
            if ($scope.conid === $scope.contactMasters[i].Id) {
                $scope.contactMasters[i].Archive = true;
            }
        }
    };

    $scope.ClearContact = function () {
        $scope.indexContact = -1;
        $scope.contactMaster = {};
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.ContactAction = "Add Row";
    };

    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "accounts/glitem/GetBankGLAccountCode?companyId=" + $scope.cashMaster.CompanyId;
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.set = function () {
        if ($scope.selectedCode !== null) {
            $scope.selectedCode = null;
        }
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.cashMaster.COAICode = x.GLGeneralInfoCode;
        $scope.cashMaster.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.set();
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.cashMaster.GLGeneralInfoName = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
        getBudget();
    };

    $scope.clearGLData = function () {
        $scope.cashMaster.GLGeneralInfoName = null;
    };

    $scope.clearResponsiblePersonData = function () {
        $scope.cashMaster.ResponsiblePersonId = null;
        $scope.cashMaster.ResponsiblePerson = null;
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.cashMaster.PlantId)) {
            ShowResult("Please select Plant!", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.cashMaster.GLGeneralInfoId)) {
            ShowResult("Please select GL!", "failure");
            return true;
        }
        return false;
    };

    $scope.Save = function () {
        $scope.$broadcast("show-errors-check-validity");
        if ($scope.cashMasterForm.$valid && !$scope.validation()) {
            if ($scope.Action === "Save") {
                $http({
                    method: "POST",
                    url: $scope.saveUrl,
                    data: { "cashMaster": $scope.cashMaster },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.cashMaster = response.data.BankMaster;
                        $scope.cashMaster.PlantName = $scope.plantId;
                        $scope.cashMasters.push($scope.cashMaster);
                        $scope.getData();
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: "POST",
                    url: $scope.updateUrl,
                    data: { "cashMaster": $scope.cashMaster },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.cashMasters[$scope.index] = $scope.cashMaster;
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.cashMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.cashMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.cashMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, "failure");
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.CompanyId = $scope.cashMaster.CompanyId;
        $scope.cashMaster = {};
        $scope.cashMaster.CompanyId = $scope.CompanyId;
        $scope.cashMaster.Active = true;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}