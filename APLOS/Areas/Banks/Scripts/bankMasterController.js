"use strict";
bankMasterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function bankMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Bank Master";
    $scope.Action = "Save";
    $scope.ContactAction = "Add Row";
    $scope.index = -1;
    $scope.indexContact = -1;
    $scope.bankMasters = [];
    $scope.path = "banks/bankmaster/";
    $scope.saveUrl = $scope.path + "create";
    $scope.updateUrl = $scope.path + "edit";
    $scope.deleteUrl = $scope.path + "delete/";
    $scope.getListUrl = $scope.path + "GetBankMasterQuery";
    $scope.getBankMasterContactListUrl = "addresses/contactmasterbank/getlistbybank/";
    baseService.init($scope.getListUrl, null, null, null, "Code", "Code");
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.bankMaster.CompanyId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.bankMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $scope.bankMaster = {
        Id: null,
        CompanyId: null,
        CompanyGroupId: null,
        GLGeneralInfoId: null,
        GLGeneralInfoName: null,
        COAICode: null,
        COAIText: null,
        PlantId: null,
        EntityId: null,
        PlantName: null,
        BankCategoryId: null,
        BankCategoryName: null,
        BankSubCategoryId: null,
        BankSubCategoryName: null,
        BankAccountTypeId: null,
        BankAccountType: null,
        BankId: null,
        BudgetId: null,
        BudgetMasterId: null,
        ActivityId: null,
        BankName: null,
        BankBranchId: null,
        BankBranchName: null,
        CurrencyId: null,
        Code: null,
        AccountTitle: null,
        OpeningDate: null,
        ShortAccountNumber: null,
        AccountNumber: null,
        IsHouseBank: true,
        IsLimitApplicable: true,
        IsBeyondLimitTransactionAllowed: true,
        IsLoanAccount: false,
        LimitAmount: 0,
        EffectiveDate: null,
        ReviewDate: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remarks: null,
        Description: null,
        Active: true,
        AccountType: null,
        IsNegotiatingBank:false
    };

    $scope.contactMaster = {
        Id: null,
        ContactPerson: null,
        ContactPersonDesignation: null,
        Phone1: null,
        Phone2: null,
        Phone3: null,
        Fax: null,
        Email1: null,
        Email2: null,
        Email3: null,
        Website: null,
        Category: null,
        SubCategory: null,
        Type: null,
        ResponsiblePerson: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), "yyyy-MM-dd"),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.budgetList = [];

    function getBudget() {
        cboService.getBudgetMasterCboByCompanyAndGLId($scope.bankMaster.CompanyId, $scope.bankMaster.GLGeneralInfoId, function (result) {
            $scope.budgetList = result;
        });
    }

    $scope.activityList = [];
    $scope.getActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.bankMaster.BudgetMasterId, function (result) {
            $scope.activityList = result;
        });
    };

    cboService.getEnumCbo("Enum/GetCboAccountType", function (result) {
        $scope.accountTypeList = result;
    });

    $scope.getBankMasterContact = function () {
        $scope.parameterscn = {
            limit: 20,
            offset: 0,
            order: "asc",
            sort: "Type",
            searchBy: "BankMasterId",
            search: $scope.bankMaster.Id
        };
        baseService.paginationBase($scope.getBankMasterContactListUrl, 1, $scope.parameterscn)
            .then(function (result) {
                $scope.contactMasters = result.Rows;
                $scope.showContactMaster = true;
            }, function () {
                ShowResult(commonMessage.NetworkError, "failure");
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            "name": "Code",
            "value": "Code"
        },
        {
            "name": "Bank",
            "value": "BankName"
        },
        {
            "name": "Bank Branch",
            "value": "BankBranch"
        },
        {
            "name": "Account Number",
            "value": "AccountNumber"
        },
        {
            "name": "Account Title",
            "value": "AccountTitle"
        },
        {
            "name": "Type",
            "value": "AccountType"
        },
        {
            "name": "Currency",
            "value": "CurrencyName"
        },
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Plant",
            "value": "PlantName"
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.bankMaster = $scope.bankMasters[$scope.index];
        $scope.bankMaster.OpeningDate = $filter("dateFiltering")($scope.bankMaster.OpeningDate, "dd-MMM-yyyy");
        $scope.bankMaster.EffectiveDate = $filter("dateFiltering")($scope.bankMaster.EffectiveDate, "dd-MMM-yyyy");
        $scope.bankMaster.ReviewDate = $filter("dateFiltering")($scope.bankMaster.ReviewDate, "dd-MMM-yyyy");
        $scope.getBankMasterContact();
        $scope.onBankChange($scope.bankMaster.BankId);
        getBudget();
        $scope.getActivity();
        $scope.getCboEntityByPlant($scope.bankMaster.CompanyId, $scope.bankMaster.PlantId);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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

    $scope.getCboPlantByCompany = function (companyId) {
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.entityList = [];
    $scope.getCboEntityByPlant = function (companyId, plantId) {
        cboService.getCboEntityByPlant(null, companyId, plantId, function (result) {
            $scope.entityList = result;
        });
    };

    $http({
        method: "GET",
        url: "banks/banksubcategory/getbanksubcategorylistcbo"
    }).then(function successCallback(response) {
        $scope.bankSubCategoryList = response.data;
    });

    $http({
        method: "GET",
        url: "banks/bankcategory/getbankcategorylistcbo"
    }).then(function successCallback(response) {
        $scope.bankCategoryList = response.data;
    });

    $http({
        method: "GET",
        url: "banks/bankaccounttype/getbankaccounttypelistcbo"
    }).then(function successCallback(response) {
        $scope.bankAccountTypeList = response.data;
    });

    $http({
        method: "GET",
        url: "banks/bank/getbanklistcbo"
    }).then(function successCallback(response) {
        $scope.bankList = response.data;
    });

    $scope.onBankChange = function (item) {
        $http({
            method: "GET",
            url: "banks/bankbranch/getcbobybankid?bankid=" + item
        }).then(function successCallback(response) {
            $scope.bankBranchList = response.data;
        });
    };

    $scope.popUpList = [];
    $scope.popUpParameters = {
        limit: 20,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function (x, name) {
        $scope.popUpUrl = "";
        $scope.popUpParameters.sort = "";
        $scope.popUpParameters.searchBy = "";
        if (name === "ResponsiblePerson") {
            $scope.popUpTitle = "Responsible Person";
            $scope.popUpUrl = "employees/EmployeeInformation/getemployeelistbycompanygroup";
            $scope.popUpParameters.sort = "EmployeeCode";
            $scope.popUpParameters.searchBy = "FirstName";
        }
        else if (name === "GLGeneralInfoName") {
            $scope.popUpTitle = "GL General Info Profile";
            $scope.popUpUrl = "accounts/glitem/getbudgetmastergl";
            $scope.popUpParameters.sort = "GLItem";
            $scope.popUpParameters.searchBy = "GLItem";
        }

        $scope.popUpDataList = [];
        $scope.popUpList = [];
        baseService.setCurrentPage("popUpDataList");
        $scope.popUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        $scope.fieldId = x;
        $scope.fieldName = name;
        angular.element(document.querySelector("#popUp")).modal("show");
        $scope.popUpData();
    };

    $scope.selectdblClick = function (data) {
        if ($scope.fieldName === "GLGeneralInfoName") {
            $scope.bankMaster[$scope.fieldId] = data.GLGeneralInfoId;
            $scope.bankMaster[$scope.fieldName] = data.GLItem;
        } else {
            $scope.bankMaster[$scope.fieldId] = data.SystemId;
            $scope.bankMaster.ResponsiblePersonId = data.SystemId;
            $scope.bankMaster[$scope.fieldName] = data.Id;
            if (data.MiddleName !== null)
                $scope.bankMaster[$scope.fieldName] = data.FirstName + " " + data.MiddleName + " " + data.LastName;
            else
                $scope.bankMaster[$scope.fieldName] = data.FirstName + " " + data.LastName;
        }
        $scope.fieldId = "";
        $scope.fieldName = "";
        angular.element(document.querySelector("#popUp")).modal("hide");
    };

    $scope.valueData = "";
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };

    $scope.SelectByButton = function () {
        if ($scope.valueData === "") {
            alert("Please at first select row");
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = "";
        angular.element(document.querySelector("#popUp")).modal("hide");
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector("#popUp")).modal("hide");
    };

    $scope.GetContact = function (id, index) {
        $scope.contactMaster = {
            Id: null,
            ContactPerson: null,
            ContactPersonDesignation: null,
            Phone1: null,
            Phone2: null,
            Phone3: null,
            Fax: null,
            Email1: null,
            Email2: null,
            Email3: null,
            Website: null,
            Category: null,
            SubCategory: null,
            Type: null,
            ResponsiblePerson: null,
            Active: true,
            Archive: false
        };
        $scope.indexContact = index;
        var obj = $scope.contactMasters[$scope.indexContact];
        for (var i in $scope.contactMaster) {
            $scope.contactMaster[i] = obj[i];
        }
        $scope.ContactAction = "Update Row";
    };

    $scope.contactMasters = [];
    $scope.addRow = function () {
        try {
            if ($scope.bankMaster.Code === null || $scope.bankMaster.Code === "") {
                throw "Bank Master Code Can Not Be Blank !!!";
            }
            if ($scope.contactMaster.ContactPerson === null || $scope.contactMaster.ContactPerson === "") {
                throw "Please Enter Person Name  !!!";
            }
            if ($scope.ContactAction === "Add Row") {
                if ($scope.contactMaster !== {}) {
                    if ($scope.indexContact !== -1)
                        $scope.contactMasters[$scope.indexContact] = $scope.contactMaster;
                    else
                        $scope.contactMasters.push($scope.contactMaster);
                    $scope.indexContact = -1;
                    $scope.contactMaster = {};
                }
            }
            else if ($scope.ContactAction === "Update Row") {
                if ($scope.contactMaster !== {}) {
                    if ($scope.indexContact !== -1)
                        $scope.contactMasters[$scope.indexContact] = $scope.contactMaster;
                    else
                        $scope.contactMasters.push($scope.contactMaster);
                    $scope.indexContact = -1;
                    $scope.contactMaster = {};
                }
            }
            $scope.showContactMaster = true;
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.isArchive = function (archive) {
        if (archive) {
            return false;
        }
        else {
            return true;
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
            "name": "GL",
            "value": "GLItem"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "AccountGroupName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetCOAICodeList = function () {
        $scope.GLUrl1 = "accounts/glitem/GetBankGLAccountCode?companyId=" + $scope.bankMaster.CompanyId;
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
        $scope.bankMaster.COAICode = x.GLGeneralInfoCode;
        $scope.bankMaster.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.set();
        $scope.selectedCode = x.GLGeneralInfoCode;
        $scope.bankMaster.GLGeneralInfoName = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
        getBudget();
    };

    $scope.clearGLData = function () {
        $scope.bankMaster.GLGeneralInfoName = null;
        $scope.bankMaster.GLGeneralInfoId = null;
        $scope.budgetList = [];
    };

    $scope.clearResponsiblePersonData = function () {
        $scope.bankMaster.ResponsiblePersonId = null;
        $scope.bankMaster.ResponsiblePerson = null;
    };

    function reDirectToRequiredTab() {
        if ($scope.bankMasterForm1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.bankMasterForm2.$invalid) {
            $scope.setTab(2);
        }
    }

    $scope.Save = function () {
        try {
            var typeId = $("#AccountType option:selected").text();
            if (typeId === "HouseBank") {
                if (baseService.isUndefinedOrNull($scope.bankMaster.GLGeneralInfoId)) {
                    throw "GL is required.";
                }
            }
            var glId = $scope.bankMaster.GLGeneralInfoId;
            if (!baseService.isUndefinedOrNull(glId)) {
                if (baseService.isUndefinedOrNull($scope.bankMaster.BudgetMasterId)) {
                    throw "Budget is required.";
                }
            }

            $scope.bankCategoryId = $("#bankCategoryId option:selected").text();
            $scope.bankSubCategoryId = $("#bankSubCategoryId option:selected").text();
            $scope.bankId = $("#bankId option:selected").text();
            $scope.bankBranchId = $("#bankBranchId option:selected").text();
            $scope.bankAccountTypeId = $("#bankAccountTypeId option:selected").text();
            $scope.plantId = $("#plantId option:selected").text();
            $scope.$broadcast("show-errors-check-validity");
            reDirectToRequiredTab();
            if ($scope.bankMasterForm.$valid && $scope.bankMasterForm1.$valid && $scope.bankMasterForm2.$valid) {
                if ($scope.bankMaster.IsLoanAccount && $scope.bankMaster.IsHouseBank === false) {
                    return ShowResult("The Bank Account must be House bank", "failure");
                }
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        data: { "bankMaster": $scope.bankMaster, "contactMaster": $scope.contactMasters },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.bankMaster = response.data.BankMaster;
                            $scope.bankMaster.BankCategoryName = $scope.bankCategoryId;
                            $scope.bankMaster.BankSubCategoryName = $scope.bankSubCategoryId;
                            $scope.bankMaster.BankName = $scope.bankId;
                            $scope.bankMaster.BankBranchName = $scope.bankBranchId;
                            $scope.bankMaster.BankAccountType = $scope.bankAccountTypeId;
                            $scope.bankMaster.PlantName = $scope.plantId;
                            $scope.bankMasters.push($scope.bankMaster);
                            $scope.bankMasters = $filter("orderBy")($scope.bankMasters, "Code");
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
                        data: { "bankMaster": $scope.bankMaster, "contactMaster": $scope.contactMasters },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            if ($scope.index > -1) {
                                $scope.bankMaster.BankCategoryName = $scope.bankCategoryId;
                                $scope.bankMaster.BankSubCategoryName = $scope.bankSubCategoryId;
                                $scope.bankMaster.BankName = $scope.bankId;
                                $scope.bankMaster.BankBranchName = $scope.bankBranchId;
                                $scope.bankMaster.BankAccountType = $scope.bankAccountTypeId;
                                $scope.bankMaster.PlantName = $scope.PlantId;
                                $scope.bankMasters[$scope.index] = $scope.bankMaster;
                                $scope.bankMasters = $filter("orderBy")($scope.bankMasters, "Code");
                                $scope.getData();
                            }
                            ClearFields();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, "failure");
                    });
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bankMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.bankMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.bankMasters.splice($scope.index, 1);
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
        $scope.CompanyId = $scope.bankMaster.CompanyId;
        $scope.bankMaster = {};
        $scope.bankMaster.CompanyId = $scope.CompanyId;
        $scope.contactMaster = {};
        $scope.contactMasters = [];
        $scope.bankMaster.Active = true;
        $scope.bankMaster.IsHouseBank = true;
        $scope.bankMaster.IsLimitApplicable = true;
        $scope.contactMaster.Email1 = null;
        $scope.contactMaster.Email2 = null;
        $scope.contactMaster.Email3 = null;
        $scope.bankMaster.IsBeyondLimitTransactionAllowed = true;
        $scope.bankMaster.LimitAmount = 0;
        $scope.budgetList = [];
        $scope.activityList = [];
        $scope.bankBranchList = [];
        $scope.entityList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}