"use strict";
expenseBookingPotalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter", "$controller", "$window"];
function expenseBookingPotalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller, $window) {
    $rootScope.title = "Expense Booking";
    $scope.Action = "Save";
    $scope.CAction = "Add";
    $scope.isPartyListing = false;
    $scope.index = -1;
    $scope.budgetTransactionMasters = [];
    $scope.approvalStatusList = [];
    $scope.CurrencyList = [];
    $scope.path = "accounts/expenseBooking/";
    $scope.reportUrl = $scope.path + "/GetExpensesBookingReport?expensesBookingId=";
    $scope.partyType = "Vendor";

    $scope.getListUrl = $scope.path + "getlist";
    $scope.saveUrl = $scope.path + "PotalBookingCreate";
    $scope.updateUrl = $scope.path + "Edit";
    $scope.deleteUrl = $scope.path + "delete/";

    $scope.isWriteOff = true;
    $controller("currencyBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller("partyBaseController", { $scope: $scope, $http: $http });

    cboService.getEnumCbo("enum/GetExpensesBookingApprovalStatusCbo", function (result) {
        $scope.approvalStatusList = result;
    });
    cboService.getEnumCbo("enum/GetFuleTypeCbo", function (result) {
        $scope.fuleTypeList = result;
    });
    cboService.getEnumCbo("enum/GetTransportTypeCbo", function (result) {
        $scope.transportTypeList = result;
    });
    //$scope.getExpensesBooking = function (status) {
    //    $scope.getListUrl = $scope.path + "getlist?status=" + status;
    //    baseService.init($scope.getListUrl, null, null, "DESC", "InvoiceDate DESC, InvoiceNumber", "InvoiceNumber");
    //    $scope.getData = function (pageno) {
    //        baseService.pagination(pageno)
    //            .then(function (result) {
    //                $scope.budgetTransactionMasters = result.Rows;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, "failure");
    //            }).finally(function () {
    //            });
    //    };
    //    //$scope.getData();
    //};

    //$scope.getExpensesBooking("Pending");

    $scope.searchByList = [
        {
            "name": "Transaction Id#",
            "value": "InvoiceNumber"
        }
    ];
    $scope.budgetCategoryList = [];
    cboService.getBudgetCategoryCbo(function (result) {
        $scope.budgetCategoryList = result;
    });

    $scope.budgetList = [];
    cboService.getCboEmployeeBudgetList("", function (result) {
        $scope.budgetList = result;
    });


    $scope.activityList = [];
    $scope.IsOrderSpecific = "";
    $scope.ActivityOrderType = "";
    $scope.getCboEmployeeBudgetActivityList = function (budgetId, level, employeeId) {
        cboService.GetBudgetMasterActivityLevelPotalCbo(budgetId, level, employeeId, function (result) {
            $scope.activityList = result;
            if ($scope.activityList.length === 1) {
                $scope.budgetTransactionDetail.ActivityId = $scope.activityList[0].ActivityId;
                $scope.IsOrderSpecific = "";
                $scope.ActivityOrderType = "";

                $scope.IsOrderSpecific = $scope.activityList[0].IsOrderSpecific;
                $scope.ActivityOrderType = $scope.activityList[0].ActivityOrderType;
            }
        });
    };

    $scope.getCboEmployeeBudgetActivityWithServiceMasterList = function (budgetId, level, employeeId) {
        cboService.GetBudgetMasterActivityLevelWithServiceMasterPotalCbo(budgetId, level, employeeId, function (result) {
            $scope.activityList = result;
            if ($scope.activityList.length === 1) {
                $scope.budgetTransactionDetail.ActivityId = $scope.activityList[0].ActivityId;

                $scope.IsOrderSpecific = "";
                $scope.ActivityOrderType = "";

                $scope.IsOrderSpecific = $scope.activityList[0].IsOrderSpecific;
                $scope.ActivityOrderType = $scope.activityList[0].ActivityOrderType;
            }
        });
    };

    $scope.phoneList = [];
    $scope.getCboActivityPhoneByEmployeeActivity = function (budgetId, activityId) {
        cboService.getCboActivityPhoneByEmployeeActivity("", budgetId, activityId, function (result) {
            $scope.phoneList = result;
        });
    };

    $scope.searchbyRegisterlist = [
        {
            "name": "Serial No",
            "value": "SerialNo"
        },
        {
            "name": "Asset No",
            "value": "AssetNo"
        },
        {
            "name": "Asset Type",
            "value": "AssetType"
        },
        {
            "name": "Article",
            "value": "Article"
        }
    ];

    $scope.registerListParameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "SerialNo",
        searchBy: "SerialNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showRegisterPopUp = function () {
        $scope.loadRegisterData = function (pageno) {
            baseService.paginationBase("fixedassets/fixedassetregister/GetRegisterByMaterialMaster?materialMasterId=" + $scope.budgetTransactionDetail.MaterialMasterId, pageno, $scope.registerListParameters)
                .then(function (result) {
                    $scope.registerList = result.Rows;
                    $scope.registerListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyRegisterlist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyRegisterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#registersearchpopup")).modal("show");
        $scope.loadRegisterData();
    };

    $scope.GetRegisterIndex = function (data) {
        $scope.budgetTransactionDetail.FixedAssetRegisterId = data.Id;
        $scope.budgetTransactionDetail.SerialNo = data.SerialNo;
        angular.element(document.querySelector("#registersearchpopup")).modal("hide");
    };

    $scope.changeFAItem = function () {
        $scope.registerList = [];
        $scope.budgetTransactionDetail.FixedAssetRegisterId = null;
        $scope.budgetTransactionDetail.SerialNo = null;
    };

    $scope.getCboFALinkedList = function (activityId) {
        var activity = $.grep($scope.activityList, function (item) {
            return item.ActivityId === activityId;
        })[0];
        $scope.IsOrderSpecific = "";
        $scope.ActivityOrderType = "";
        $scope.IsOrderSpecific = activity.IsOrderSpecific;
        $scope.ActivityOrderType = activity.ActivityOrderType;
        $scope.ActivityType = activity.ActivityType;
        $scope.FALinked = activity.FALinked;
        if (!baseService.isUndefinedOrNull($scope.FALinked)) {
            cboService.getEnumCbo("Accounts/BudgetMaster/GetFALinkedList?budgetMasterId=" + $scope.selectedBudgetMasterId + "&activityId=" + activityId + "&faLinked=" + activity.FALinked, function (result) {
                if ($scope.FALinked === "Item" || $scope.FALinked === "Register") {
                    $scope.faRegisterList = [];
                    $scope.faMasterList = result;
                }
                else if ($scope.FALinked === "Register") {
                    $scope.faMasterList = [];
                    $scope.faRegisterList = result;
                }
                else {
                    $scope.faMasterList = [];
                    $scope.faRegisterList = [];
                    $scope.FALinked = null;
                }
            });
        }
    };

    $scope.selectedBudgetId = null;
    $scope.selectedBudgetCodeName = null;
    $scope.selectedbudgetId = function (selected) {
        if (selected) {
            $scope.selectedBudgetId = selected.originalObject.BudgetId;
            $scope.selectedBudgetCodeName = selected.originalObject.BudgetCodeName;
            $scope.selectedBudgetMasterId = selected.originalObject.Id;
            cboService.getCboEmployeeBudgetActivityList(" ", selected.originalObject.Id, function (result) {
                $scope.activityList = result;
            });
        }
    };

    $scope.budgetTransactionMaster = {
        Id: null,
        EmployeeId: null,
        EntityId: null,
        PlantId: null,
        InvoiceNumber: null,
        InvoiceDate: null,
        Active: true,
        CurrencyId: null,
        Status: "ToBeChecked",
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        BeneficiaryType: "Self",
        PartyName: null,
        PartyId: null,
        PartyPlantId: null
    };

    $scope.budgetTransactionDetail = {
        Id: null,
        ExpenseBookingId: null,
        PartyId: null,
        BudgetId: null,
        ActivityId: null,
        ActivityPhoneId: null,
        Amount: null,
        DocDate: new Date(),
        DocRefNo: null,
        MaterialMasterId: null,
        FixedAssetRegisterId: null,
        BudgetCategory: null,
        BudgetSubCategory: null,
        BudgetName: null,
        BudgetGroup: null,
        GLGeneralInfoCode: null,
        GL: null
    };
    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        if (new Date($scope.budgetTransactionMaster.InvoiceDate) > new Date()) {
            $scope.invalidDocDate = true;
            msg = "Invoice Date must be below or equal to current Date!";
        }
        else $scope.invalidDocDate = false;
        return manualValidation("div_DocDate", $scope.invalidDocDate, msg);
    };

    $scope.GetCboParallelCurrency = function () {
        cboService.getCboParallelCurrency(function (result) {
            $scope.tranCurrencyList = result;
            if ($scope.tranCurrencyList.length == 1) {
                $scope.budgetTransactionMaster.CurrencyId = $scope.tranCurrencyList[0].CurrencyId;
                // $scope.GetCurrencyExchangeRateList();
            }
        });
    }
    $scope.GetCboParallelCurrency();

    $scope.beneficiaryTypeList = [];
    $scope.getBeneficiaryType = function () {
        $http({
            method: "GET",
            url: "Enum/GetBeneficiaryTypeCbo/"
        }).then(function successCallback(response) {
            $scope.beneficiaryTypeList = response.data;
            $scope.budgetTransactionMaster.BeneficiaryType = $scope.beneficiaryTypeList[0].Value;
        });
    };
    $scope.getBeneficiaryType();

    $scope.GetEmployeeTransactionNo = function () {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetPotalEmployeeTransactionNo"
        }).then(function successCallback(response) {
            $scope.employeeTransactionNo = response.data;
            $scope.budgetTransactionMaster.InvoiceNumber = "EX-" + $scope.employeeTransactionNo;
        });
    };
    $scope.GetEmployeeTransactionNo();
    $scope.entityList = [];
    $scope.entityLoad = function () {
        //baseService.getCompanyConfiguration(function (result) {
        //    $scope.companyConfig = result;
        //    cboService.getCboEntityByPlant(null, null, "", function (result) {
        //        $scope.entityList = result;
        //        $scope.companyConfigLoad();
        //    });
        //});

        baseService.getCompanyConfiguration(function (result) {
            $scope.companyConfig = result;
            cboService.getEntityCboByPlant(null, null, "", function (result) {
                $scope.entityList = result;
                $scope.companyConfigLoad();
            });
        });
    }
    $scope.entityLoad();

    $scope.costCenterCboList = [];
    $scope.GetCboCostCenterIdByEntity = function (entityId) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetCboCostCenterIdByEntity?entityId=" + entityId
        }).then(function successCallback(response) {
            $scope.costCenterCboList = response.data;

        });
    };
    $scope.companyConfigLoad = function () {
        baseService.getCompanyConfiguration(function (result) {
            $scope.companyConfig = result;
            cboService.getCboWithEmployee(null, null, function (result) {
                $scope.entityEmployeeList = result;
                if ($scope.entityEmployeeList.length > 0) {
                    $scope.budgetTransactionMaster.EntityId = $scope.entityEmployeeList[0].Value;
                    $scope.GetCboCostCenterIdByEntity($scope.budgetTransactionMaster.EntityId);
                }
            });
        });
    }
    


    // #region Activity
    $scope.budgetTransactionDetailList = [];
    function checkNullValue() {
        try {
            if ($scope.budgetTransactionDetail.DocDate === null || $scope.budgetTransactionDetail.DocDate === "") {
                throw "Please input ExpenseDate";
            } else if ($scope.budgetTransactionDetail.BudgetId === null || $scope.budgetTransactionDetail.BudgetId === "") {
                throw "Please input Budget";
            }
        } catch (e) {
            throw e;
        }
    }

    function checkDuplicate(ActivityId, BudgetId, MaterialMasterId, FixedAssetRegisterId) {
        try {
            if (baseService.isUndefinedOrNull(ActivityId)) {
                throw "Please select Activity!";
            }
            if ($scope.FALinked === "Item" || $scope.FALinked === "Register") {
                if (baseService.isUndefinedOrNull(MaterialMasterId)) {
                    throw "Please select Fixed Asset Item !";
                }
                if ($scope.FALinked === "Register" && baseService.isUndefinedOrNull(FixedAssetRegisterId)) {
                    throw "Please select Fixed Asset Register !";
                }
                var getRow = $filter("filter")($scope.budgetTransactionDetailList, {
                    "BudgetMasterId": BudgetId, "ActivityId": ActivityId, "MaterialMasterId": MaterialMasterId, "FixedAssetRegisterId": FixedAssetRegisterId
                });
                if (!baseService.isUndefinedOrNull(getRow) && getRow.length > 0 && getRow[0].BudgetMasterId === BudgetId) {
                    throw "This Fixed Asset Item is already added!";
                }
                else {
                    return true;
                }
            }
            else {
                var data = $filter("filter")($scope.budgetTransactionDetailList, { "BudgetMasterId": BudgetId, "ActivityId": ActivityId });
                if (!baseService.isUndefinedOrNull(data) && data.length > 0 && data[0].BudgetMasterId === BudgetId) {
                    throw "This Activity is already added!";
                }
                else {
                    return true;
                }
            }

        } catch (e) {
            throw e;
        }
    }

    $scope.searchByBudgetMasterList = [
        {
            "name": "GL",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget Group",
            "value": "BudgetGroup"
        },
        {
            "name": "Budget Category",
            "value": "BudgetCategory"
        },
        {
            "name": "Budget SubCategory",
            "value": "BudgetSubCategory"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        },
        {
            "name": "Level",
            "value": "MappingLevel"
        }
    ];

    $scope.budgetMasterParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "BudgetName",
        searchBy: "BudgetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GetBudgetMasterList = function () {
        $scope.faMasterList = [];
        $scope.FALinked = null;
        $scope.GLUrl1 = "accounts/BudgetMaster/GetCboEmployeeBudgetPopUpList";
        $scope.GetBudgetMasterListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.budgetMasterParameters)
                .then(function (result) {
                    $scope.budgetMasterList = result.Rows;
                    $scope.budgetMasterParameters.total_count = result.Total;
                },
                    function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
        };
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetBudgetMasterListData();
    };

    $scope.closeBudgetMasterPopUp = function () {
        angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
        angular.element(document.querySelector("#budgetMasterWithServiceMasterPopUp")).modal("hide");
    };

    $scope.closeBudgetMasterPopUpSelected = function () {
        if ($scope.setSelected !== null) {
            angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
            angular.element(document.querySelector("#budgetMasterWithServiceMasterPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.GetBudgetMasterWithServiceMasterList = function () {
        $scope.budgetTransactionDetail = {};
        $scope.faMasterList = [];
        $scope.FALinked = null;
        $scope.GLUrl1 = "SetUps/ServiceMaster/GetCboEmployeeBudgetWithServiceMasterPopUpList";
        $scope.GetBudgetMasterWithServiceMasterListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.budgetMasterParameters)
                .then(function (result) {
                    $scope.budgetMasterList = result.Rows;
                    $scope.budgetMasterParameters.total_count = result.Total;
                },
                    function () {
                        ShowResult(commonMessage.NetworkError, "failure");
                    }).finally(function () {
                    });
        };
        angular.element(document.querySelector("#budgetMasterWithServiceMasterPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetBudgetMasterWithServiceMasterListData();
    };

   
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.responsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.responsiblePersonIndex];
            $scope.budgetTransactionMaster.ResponsiblePersonName = employee.EmployeeName;
            $scope.budgetTransactionMaster.ResponsiblePersonId = employee.SystemId;
        }
        $scope.hideResponsiblePersonPopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };

    $scope.clearResponsiblePerson = function () {
        $scope.budgetTransactionMaster.ResponsiblePersonName = null;
        $scope.budgetTransactionMaster.ResponsiblePersonId = null;
    };

    $scope.partyPlantList = [];
    $scope.getPartyPlantList = function (partyId) {
        $scope.partyPlantList = [];
        $http.get("Parties/party/GetPartyPlantCbo?partyId=" + partyId)
            .then(function (response) {
                angular.forEach(response.data, function (item, i) {
                    $scope.partyPlantList.push(item);
                    if (item.IsDefault) {
                        $scope.partyPlantId = item.Value;
                        $scope.budgetTransactionMaster.PartyPlantId = item.Value;
                    }
                });
            });
    };

    $scope.closePartyPopUp = function (x) {
      
            var party = x.data;
            $scope.budgetTransactionMaster.PartyId = party.Id;
            $scope.budgetTransactionMaster.PartyName = party.UserName;
            $scope.getPartyPlantList(party.Id);
        $scope.hidePartyPopUp();
    };

    $scope.clearPartyPopUp = function () {
        $scope.budgetTransactionMaster.PartyId = null;
        $scope.budgetTransactionMaster.PartyName = null;
        $scope.budgetTransactionMaster.PartyPlantId = null;
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.GLGeneralInfoCode;
        $scope.budgetTransactionDetail.BudgetCategoryId = x.BudgetCategoryId;
        $scope.budgetTransactionDetail.BudgetCategory = x.BudgetCategory;
        $scope.budgetTransactionDetail.BudgetSubCategoryId = x.BudgetSubCategoryId;
        $scope.budgetTransactionDetail.BudgetSubCategory = x.BudgetSubCategory;
        $scope.budgetTransactionDetail.BudgetId = x.BudgetId;
        $scope.budgetTransactionDetail.BudgetName = x.BudgetName;
        $scope.budgetTransactionDetail.GLGeneralInfoId = x.GLGeneralInfoId;
        $scope.budgetTransactionDetail.GL = x.GL;
        $scope.budgetTransactionDetail.ServiceMasterId = x.ServiceMasterId;
        $scope.selectedBudgetMasterId = x.Id;
        if ($scope.companyConfig.IsInboundInvoiceServiceApplicable === true) {
            $scope.getCboEmployeeBudgetActivityWithServiceMasterList(x.Id, x.MappingLevel, null);
        }
        else {
            $scope.getCboEmployeeBudgetActivityList(x.Id, x.MappingLevel, null);
        }
        
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#budgetMasterPopUp")).modal("hide");
            angular.element(document.querySelector("#budgetMasterWithServiceMasterPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.addRow = function () {
        try {
            $scope.BudgetName = angular.element("#Budget :selected").text();
            $scope.ActivityName = angular.element("#activity :selected").text();
            if ($scope.FALinked === "Item" || $scope.FALinked === "Register") {
                $scope.FixedAsset = angular.element("#FixedAssetMaster :selected").text();
            }
            else
                $scope.FixedAsset = null;
            checkNullValue();
            checkDuplicate($scope.budgetTransactionDetail.ActivityId, $scope.selectedBudgetMasterId,
                $scope.budgetTransactionDetail.MaterialMasterId, $scope.budgetTransactionDetail.FixedAssetRegisterId);
            if ($scope.CAction === "Add") {
                $scope.budgetTransactionDetailList.push({
                    Id: null,
                    DocRefNo: $scope.budgetTransactionMaster.InvoiceNumber,
                    DocDate: $scope.budgetTransactionMaster.InvoiceDate,
                    BudgetName: $scope.budgetTransactionDetail.BudgetName,
                    ActivityName: $scope.ActivityName,
                    ServiceMasterId: $scope.budgetTransactionDetail.ServiceMasterId,
                    GLGeneralInfoId: $scope.budgetTransactionDetail.GLGeneralInfoId,
                    BudgetId: $scope.budgetTransactionDetail.BudgetId,
                    BudgetMasterId: $scope.selectedBudgetMasterId,
                    PartyId: $scope.budgetTransactionDetail.PartyId,
                    ActivityId: $scope.budgetTransactionDetail.ActivityId,
                    ActivityPhoneId: $scope.budgetTransactionDetail.ActivityPhoneId,
                    PhoneName: $scope.PhoneName,
                    Amount: $scope.budgetTransactionDetail.Amount,
                    FixedAsset: $scope.FixedAsset,
                    MaterialMasterId: $scope.budgetTransactionDetail.MaterialMasterId,
                    FixedAssetRegisterId: $scope.budgetTransactionDetail.FixedAssetRegisterId,
                    SerialNo: $scope.budgetTransactionDetail.SerialNo,
                    ActivityType: $scope.ActivityType,
                    IsOrderSpecific: $scope.IsOrderSpecific,
                    ActivityOrderType: $scope.ActivityOrderType
                });
                $scope.budgetTransactionDetail = {};
                $scope.selectedBudgetCodeName = null;
                $scope.selectedBudgetMasterId = null;
                $scope.activityList = [];
                $scope.ActivityType = null;
                $scope.FixedAsset = null;
                $scope.convenyenceModel = {};
                $scope.fuelModel = {};
                $scope.IsOrderSpecific = null;
                $scope.ActivityOrderType = null;
            }

            if ($scope.indexdetails !== -1 && $scope.CAction === "Update") {
                $scope.budgetTransactionDetail.BudgetName = $scope.BudgetName;
                $scope.budgetTransactionDetail.ActivityName = $scope.ActivityName;
                $scope.budgetTransactionDetail.PhoneName = $scope.PhoneName;
                $scope.budgetTransactionDetail.PartyName = $scope.PartyName;
                $scope.budgetTransactionDetail.FixedAsset = $scope.FixedAsset;
                $scope.budgetTransactionDetailList[$scope.indexdetails] = $scope.budgetTransactionDetail;
                $scope.budgetTransactionDetail = {};
                $scope.indexdetails = -1;
                $scope.CAction = "Add";
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
    };

    $scope.GetVoucherDetailrow = function (data, index) {
        $scope.indexdetails = index;
        data.DocDate = $filter("dateFiltering")(data.DocDate);
        $scope.budgetTransactionDetail = data;
        $scope.getCboEmployeeBudgetActivityList($scope.budgetTransactionDetail.ActivityId, null);
        $scope.getCboActivityPhoneByEmployeeActivity($scope.budgetTransactionDetail.ActivityId);
        $scope.CAction = "Update";
    };

    $scope.valuePassInDelModal = function (x, index) {
        $scope.id = x.Id;
        $scope.dindex = index;
        $scope.message_confirmation = "Are you sure want to delete this data....";
        angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
    };

    $scope.GetBudgetTransactionDetail = function (id) {
        $http({
            method: "GET",
            url: "accounts/expenseBooking/GetExpensesBookingDetail?expenseBookingId=" + id
        }).then(function successCallback(response) {
            $scope.budgetTransactionDetailList = response.data;
        });
    };

    $scope.Get = function (data) {
        $scope.budgetTransactionMaster = data.data;
        $scope.GetBudgetTransactionDetail($scope.budgetTransactionMaster.Id);
        $scope.budgetTransactionMaster.AddedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.AddedDate);
        $scope.budgetTransactionMaster.UpdatedDate = $filter("dateFiltering")($scope.budgetTransactionMaster.UpdatedDate);
        $scope.budgetTransactionMaster.InvoiceDate = $filter("dateFiltering")($scope.budgetTransactionMaster.InvoiceDate);
        $scope.budgetTransactionMaster.PartyId = $scope.budgetTransactionMaster.PartyId;
        $scope.budgetTransactionMaster.PartyPlantId = $scope.budgetTransactionMaster.PartyPlantId;
        $scope.budgetTransactionMaster.PartyName = $scope.budgetTransactionMaster.PartyName;
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.validation = function () {
        if (baseService.isUndefinedOrNull($scope.budgetTransactionMaster.CurrencyId)) {
            ShowResult("Please Select Currency", "failure");
            return true;
        }
        if (baseService.isUndefinedOrNull($scope.budgetTransactionMaster.InvoiceNumber)) {
            ShowResult("Please Input Invoice Number.", "failure");
            return true;
        }
        if ($scope.budgetTransactionMaster.BeneficiaryType === "Vendor" && baseService.isUndefinedOrNull($scope.budgetTransactionMaster.PartyId)) {
            ShowResult("Please select Vendor.", "failure");
            return true;
        }
        if ($scope.budgetTransactionMaster.BeneficiaryType === "Vendor" && baseService.isUndefinedOrNull($scope.budgetTransactionMaster.PartyPlantId)) {
            ShowResult("Please select Invoicing Vendor.", "failure");
            return true;
        }
        if (new Date($scope.budgetTransactionMaster.InvoiceDate) > new Date()) {
            ShowResult("Invoice Date must be below or equal to current Date!", "failure");
            return true;
        }
        //for (var i = 0; i < $scope.budgetTransactionDetailList.length; i++) {
        //    if ($scope.budgetTransactionDetailList[i].IsOrderSpecific === true && $scope.invoiceDetailChargesList.length === 0) {
        //        ShowResult($scope.budgetTransactionDetailList[i].BudgetName + ",  Please Distribute Expense!", "failure");
        //        return true;
        //    }
        //}
        return false;
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.budgetTransactionMaster.Id)) {
            $http({
                method: "POST",
                url: $scope.deleteUrl + $scope.budgetTransactionMaster.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.budgetTransactionMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.GetEmployeeTransactionNo();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.budgetTransactionMaster.CurrencyId = $scope.selectBaseCurrency();

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.budgetTransactionMaster = {};
        $scope.budgetTransactionDetailList = [];
        $scope.budgetTransactionMaster.Active = true;
        $scope.budgetTransactionMaster.Status = $scope.approvalStatusList[0].Value;
        $scope.getBeneficiaryType();
        $scope.GetCboParallelCurrency();
        $scope.GetEmployeeTransactionNo();
        $scope.entityLoad();

        $scope.invoiceDetailChargesList = [];
        $scope.checkedInvoiceList = [];
        $scope.checkedOutBoundInvoiceList = [];
        $scope.CustomerAvailableInvoiceList = [];
        $scope.checkedMasterOrderList = [];
        $scope.checkedContractList = [];
    }

    $scope.report = function (voucherId) {
        location.href = $scope.reportUrl + voucherId;
    };
    $scope.invalidRow = false;
    $scope.checkRowValidation = function (data, index) {
        if (manualValidation("td_DocRef_" + index, baseService.isUndefinedOrNull(data.DocRefNo), "Invoice No is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_DocDate_" + index, baseService.isUndefinedOrNull(data.DocDate), "Invoice Date is required.")) {
            $scope.invalidRow = true;
        }
        else if (manualValidation("td_Amount_" + index, baseService.isUndefinedOrNull(data.Amount), "Amount is required.")) {
            $scope.invalidRow = true;
        }
        else
            $scope.invalidRow = false;
    };


    $scope.confirmRemoveRow = function (index, data) {
        $scope.index = index;
        $scope.delDatarow = data;
        $scope.message_confirmation = "Are you sure want to delete this data....";
        angular.element(document.querySelector("#confirmgenericPopUp")).modal("show");
    };

    $scope.removeRow = function () {

        $scope.budgetTransactionDetailList.splice($scope.index, 1);
        var i = $scope.expActivityList.length;
        while (i--) {
            if ($scope.expActivityList[i]["BudgetMasterId"] === $scope.delDatarow.BudgetMasterId && $scope.expActivityList[i]["ActivityId"] === $scope.delDatarow.ActivityId
                && $scope.expActivityList[i]["FixedAssetRegisterId"] === $scope.delDatarow.FixedAssetRegisterId) {
                $scope.expActivityList.splice(i, 1);
            }
        }
        $scope.delDatarow = {};
    };

    $scope.convenyenceModel = {
        From: null,
        To: null,
        ConvenyenceDate: null,
        TransportType: null
    }
    $scope.fuelModel = {
        Kilometer: null,
        QtyinLiter: null,
        FuelType: null,
        BillNo: null
    }
    $scope.expActivityList = [];
    $scope.showFuleExpPopUp = function () {

        angular.element(document.querySelector("#fuelExpPopUp")).modal("show");
    }

    $scope.closefuelExpPopUp = function () {
        var getRow = $filter("filter")($scope.expActivityList, {
            "BudgetMasterId": $scope.selectedBudgetMasterId, "ActivityId": $scope.budgetTransactionDetail.ActivityId, "FixedAssetRegisterId": $scope.budgetTransactionDetail.FixedAssetRegisterId
        });
        if (getRow.length == 0) {
            $scope.expActivityList.push({
                Id: null,
                GLGeneralInfoId: $scope.budgetTransactionDetail.GLGeneralInfoId,
                BudgetMasterId: $scope.selectedBudgetMasterId,
                ActivityId: $scope.budgetTransactionDetail.ActivityId,
                FixedAssetRegisterId: $scope.budgetTransactionDetail.FixedAssetRegisterId,
                Kilometer: $scope.fuelModel.Kilometer,
                QtyinLiter: $scope.fuelModel.QtyinLiter,
                FuelType: $scope.fuelModel.FuelType,
                BillNo: $scope.fuelModel.BillNo,
                Remarks: $scope.fuelModel.Remarks,
                ActivityType: $scope.ActivityType,
            });
        }
        else {
            for (var i = 0; i < $scope.expActivityList.length; i++) {
                if ($scope.expActivityList[i].BudgetMasterId == $scope.selectedBudgetMasterId && $scope.expActivityList[i].ActivityId == $scope.budgetTransactionDetail.ActivityId) {
                    $scope.expActivityList[i].Kilometer = $scope.fuelModel.Kilometer;
                    $scope.expActivityList[i].QtyinLiter = $scope.fuelModel.QtyinLiter;
                    $scope.expActivityList[i].FuelType = $scope.fuelModel.FuelType;
                    $scope.expActivityList[i].BillNo = $scope.fuelModel.BillNo;
                    $scope.expActivityList[i].Remarks = $scope.fuelModel.Remarks;
                }
            }
        }

        angular.element(document.querySelector("#fuelExpPopUp")).modal("hide");
    }

    $scope.closeConvenyenceExpPopUp = function () {
        var getRow = $filter("filter")($scope.expActivityList, {
            "BudgetMasterId": $scope.selectedBudgetMasterId, "ActivityId": $scope.budgetTransactionDetail.ActivityId, "FixedAssetRegisterId": $scope.budgetTransactionDetail.FixedAssetRegisterId
        });
        if (getRow.length == 0) {
            $scope.expActivityList.push({
                Id: null,
                GLGeneralInfoId: $scope.budgetTransactionDetail.GLGeneralInfoId,
                BudgetMasterId: $scope.selectedBudgetMasterId,
                ActivityId: $scope.budgetTransactionDetail.ActivityId,
                FixedAssetRegisterId: $scope.budgetTransactionDetail.FixedAssetRegisterId,
                From: $scope.convenyenceModel.From,
                To: $scope.convenyenceModel.To,
                ConvenyenceDate: $scope.convenyenceModel.ConvenyenceDate,
                TransportType: $scope.convenyenceModel.TransportType,
                Remarks: $scope.convenyenceModel.Remarks,
                ActivityType: $scope.ActivityType,
            });
        } else {
            for (var i = 0; i < $scope.expActivityList.length; i++) {
                if ($scope.expActivityList[i].BudgetMasterId == $scope.selectedBudgetMasterId && $scope.expActivityList[i].ActivityId == $scope.budgetTransactionDetail.ActivityId) {
                    $scope.expActivityList[i].From = $scope.convenyenceModel.From;
                    $scope.expActivityList[i].To = $scope.convenyenceModel.To;
                    $scope.expActivityList[i].ConvenyenceDate = $scope.convenyenceModel.ConvenyenceDate;
                    $scope.expActivityList[i].TransportType = $scope.convenyenceModel.TransportType;
                    $scope.expActivityList[i].Remarks = $scope.convenyenceModel.Remarks;
                }
            }
        }
        angular.element(document.querySelector("#ConvenyenceExpPopUp")).modal("hide");
    }
    $scope.showConveyancePopUp = function () {
        angular.element(document.querySelector("#ConvenyenceExpPopUp")).modal("show");
    }

    //$scope.Save = function () {
    //    $scope.$broadcast("show-errors-check-validity");
    //    angular.forEach($scope.budgetTransactionDetailList, function (item, i) {
    //        if ($scope.invalidRow) {
    //            return;
    //        }
    //        $scope.checkRowValidation(item, i);
    //    });
    //    if ($scope.budgetTransactionMasterForm.$valid && !$scope.invalidRow && !$scope.validation()) {
    //        try {
    //            if ($scope.budgetTransactionDetailList.length < 1) {
    //                throw "Please add at least one TransactionDetail. ";
    //            }
    //            if ($scope.Action === "Save") {
    //                $http({
    //                    method: "POST",
    //                    url: $scope.saveUrl,
    //                    data: {
    //                        "expenseBooking": $scope.budgetTransactionMaster,
    //                        "expenseBookingDetails": $scope.budgetTransactionDetailList,
    //                        "expActdetails": $scope.expActivityList
    //                    },
    //                    dataType: "JSON"
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error === true) {
    //                        ShowResult(response.data.Message, "failure");
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, "success");
    //                        $scope.getExpensesBooking("Pending");
    //                        baseService.paginationAdd();
    //                        ClearFields(response.data.Sequence);
    //                    }
    //                }, function errorCallback(response) {
    //                    ShowResult(response.status.Message, "failure");
    //                });
    //                return true;
    //            }
    //            else if ($scope.Action === "Update") {
    //                $http({
    //                    method: "POST",
    //                    url: $scope.updateUrl,
    //                    data: {
    //                        "expenseBooking": $scope.budgetTransactionMaster,
    //                        "expenseBookingDetails": $scope.budgetTransactionDetailList,
    //                        "expActdetails": $scope.expActivityList
    //                    },
    //                    dataType: "JSON"
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error === true) {
    //                        ShowResult(response.data.Message, "failure");
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, "success");
    //                        if ($scope.index > -1) {
    //                            $scope.budgetTransactionMasters[$scope.index] = $scope.budgetTransactionMaster;
    //                        }
    //                        ClearFields(response.data.Sequence);

    //                    }
    //                }, function errorCallback(response) {
    //                    ShowResult(response.status.Message, "failure");
    //                });
    //                return true;
    //            }
    //        } catch (e) {
    //            throw ShowResult(e, "failure");
    //        }
    //    }
    //    return true;
    //};

    // #region   document

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExpensesDocument + '/' + data.Id + extention;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };

    $scope.invoiceDetailChargesList = [];
    $scope.InvoiceDetailChargesList = function myfunction() {
        $scope.invoiceDetailChargesList = $scope.checkedInvoiceList.concat($scope.checkedOutBoundInvoiceList).concat($scope.checkedMasterOrderList).concat($scope.checkedContractList);

    };

    $scope.Save = function () {
        $scope.InvoiceDetailChargesList();
        $scope.$broadcast("show-errors-check-validity");
        angular.forEach($scope.budgetTransactionDetailList, function (item, i) {
            if ($scope.invalidRow) {
                return;
            }
            $scope.checkRowValidation(item, i);
        });
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.budgetTransactionMaster.FileName = fileName;
        if (!baseService.isUndefinedOrNull($scope.budgetTransactionMaster.FileName)) {
            if ($scope.budgetTransactionMaster.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        if ($scope.budgetTransactionMasterForm.$valid && !$scope.validation() && !$scope.invalidRow) {
            try {
                if ($scope.budgetTransactionDetailList.length < 1) {
                    throw "Please add at least one TransactionDetail. ";
                }
                var formData = new FormData();
                if ($scope.Action === "Save") {
                    $http({
                        method: "POST",
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("expenseBooking", angular.toJson($scope.budgetTransactionMaster));
                            formData.append("expenseBookingDetails", angular.toJson($scope.budgetTransactionDetailList));
                            formData.append("expActdetails", angular.toJson($scope.expActivityList));
                            formData.append("invoiceDetailChargesList", angular.toJson($scope.invoiceDetailChargesList));
                            if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                                formData.append('file', data.file);
                            }
                            return formData;
                        },
                        data: {
                            "expenseBookingDetails": $scope.budgetTransactionDetailList,
                            "expActdetails": $scope.expActivityList,
                            "file": $scope.filedata,
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.GetExBooking();
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: "POST",
                        url: $scope.updateUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("expenseBooking", angular.toJson($scope.budgetTransactionMaster));
                            formData.append("expenseBookingDetails", angular.toJson($scope.budgetTransactionDetailList));
                            formData.append("expActdetails", angular.toJson($scope.expActivityList));
                            formData.append("invoiceDetailChargesList", angular.toJson($scope.invoiceDetailChargesList));
                            if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                                formData.append('file', data.file);
                            }
                            return formData;
                        },
                        data: {
                            "expenseBookingDetails": $scope.budgetTransactionDetailList,
                            "expActdetails": $scope.expActivityList,
                            "file": $scope.filedata,
                        },
                        dataType: "JSON"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.budgetTransactionMaster.Status = "ToBeChecked";
                            $scope.GetExBooking();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                
            } catch (e) {
                throw ShowResult(e, "failure");
            }
        }
        return true;
    };

    $scope.checkedByList = [];
    $scope.getCboCheckedByList = function () {
        cboService.getAuthorizationConfigCbo('ExpenseBookingCheckedBy', function (result) {
            $scope.checkedByList = result;
            if ($scope.checkedByList.length == 1) {
                $scope.budgetTransactionMaster.ResponsiblePersonId = $scope.checkedByList[0].Id;
            }
        });
    };
    $scope.getCboCheckedByList();

    $scope.GetExBooking = function () {
        debugger;
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + $scope.budgetTransactionMaster.Status
        }).then(function successCallback(response) {
            $scope.budgetTransactionMasters = response.data;
        });
    }
    $scope.GetExBooking();

    $scope.CheckedDataList = [];
    $scope.GetCheckedExBooking = function () {
        debugger;
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'ToBeApproved'
        }).then(function successCallback(response) {
            $scope.CheckedDataList = response.data;
        });
    }


    $scope.CheckedHoldDataList = [];
    $scope.GetCheckedHoldExBooking = function () {
        $scope.CheckedHoldDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'CheckedHolded'
        }).then(function successCallback(response) {
            $scope.CheckedHoldDataList = response.data;
        });
    }

    $scope.CheckedRejectDataList = [];
    $scope.GetCheckedRejectExBooking = function () {
        $scope.CheckedRejectDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'CheckedRejected'
        }).then(function successCallback(response) {
            $scope.CheckedRejectDataList = response.data;
        });
    }

    $scope.Booking = "";
    $scope.tab = 1;
    $scope.setTabBookingList = function (newTab) {
        $scope.tab = newTab;
        //// alert('Tab 1');
        //$scope.Booking = 0;
        //$scope.GetGRN();
    };
    $scope.isSetBookingList = function (tabNum) {
        return $scope.tab === tabNum;
    };



    $scope.Booking = "";
    // $scope.tab = 2;
    $scope.setTabNotApproveCheck = function (newTab) {
        debugger;
        $scope.tab = newTab;
        //  alert('Tab 2');
        //$scope.Booking = 0;
        //$scope.GetEmployeeNotApproveChecked();
        $scope.GetNotApproveChecked();
    };
    $scope.isSetNotApproveCheck = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.Booking = 0;
    };

    $scope.setTabChecked = function (newTab) {
        $scope.tab = newTab;
        $scope.GetCheckedExBooking();
    };
    $scope.isSetChecked = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.Booking = 0;
    };

    $scope.setTabCheckedHold = function (newTab) {
        $scope.tab = newTab;
        $scope.GetCheckedHoldExBooking();
    };
    $scope.isSetCheckedHold = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabCheckedReject = function (newTab) {
        $scope.tab = newTab;
        $scope.GetCheckedRejectExBooking();
    };
    $scope.isSetCheckedReject = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //#region my task from here 
    $scope.setTabApprovedHold = function (newTab) {
        $scope.tab = newTab;
        $scope.GetApprovedHoldExBooking();
    };
    $scope.isSetApprovedHold = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabApprovedReject = function (newTab) {
        $scope.tab = newTab;
        $scope.GetApprovedRejectExBooking();
    };
    $scope.isSetApprovedReject = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabApproved = function (newTab) {
        $scope.tab = newTab;
        $scope.GetApprovedExBooking();
    };
    $scope.isSetApproved = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setTabPosted = function (newTab) {
        $scope.tab = newTab;
        $scope.GetPostedExBooking();
    };
    $scope.isSetPosted = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ApprovedHoldDataList = [];
    $scope.GetApprovedHoldExBooking = function () {
        $scope.ApprovedHoldDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'ApprovedHolded'
        }).then(function successCallback(response) {
            $scope.ApprovedHoldDataList = response.data;
        });
    }

    $scope.ApprovedRejectDataList = [];
    $scope.GetApprovedRejectExBooking = function () {
        $scope.ApprovedRejectDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'ApprovedRejected'
        }).then(function successCallback(response) {
            $scope.ApprovedRejectDataList = response.data;
        });
    }

    $scope.ApprovedDataList = [];
    $scope.GetApprovedExBooking = function () {
        $scope.ApprovedDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "getlist?status=" + 'Approved'
        }).then(function successCallback(response) {
            $scope.ApprovedDataList = response.data;
        });
    }

    $scope.PostedDataList = [];
    $scope.GetPostedExBooking = function () {
        $scope.PostedDataList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetPotalPostedList"
        }).then(function successCallback(response) {
            $scope.PostedDataList = response.data;
        });
    }

    //#endregion my task hend here 

    $scope.onClickPdfPrint = function (args) {

        var gridObj = $("#BookingGridId1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrint = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",

            click: $scope.onClickPdfPrint
        }
    }];

    $scope.onClickExcelPrint = function (args) {

        var gridObj = $("#BookingGridId1").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrint = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",

            click: $scope.onClickExcelPrint
        }
    }];

    $scope.onClickPdfPrintChecked = function (args) {
        var gridObj = $("#BookingGridId2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintChecked = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintChecked
        }
    }];

    $scope.onClickExcelPrintChecked = function (args) {
        var gridObj = $("#BookingGridId2").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintChecked = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintChecked
        }
    }];

    $scope.onClickPdfPrintCheckedHold = function (args) {
        var gridObj = $("#GridCheckedHoldId").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintCheckedHold = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintCheckedHold
        }
    }];

    $scope.onClickExcelPrintCheckedHold = function (args) {
        var gridObj = $("#GridCheckedHoldId").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintCheckedHold = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintCheckedHold
        }
    }];

    $scope.onClickPdfPrintCheckedReject = function (args) {
        var gridObj = $("#GridCheckedRejectId").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };

    $scope.PdfPrintCheckedReject = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintCheckedReject
        }
    }];

    $scope.onClickPdfPrintApprovedHold = function (args) {
        var gridObj = $("#GridApprovedHold").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintApprovedHold = [{

        type: "details",
        buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintApprovedHold
        }
    }];

    $scope.onClickPdfPrintApprovedRejected = function (args) {
        var gridObj = $("#GridApprovedReject").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintApprovedRejected = [{

        type: "details",
        buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintApprovedRejected
        }
    }];

    $scope.onClickPdfPrintApproved = function (args) {
        var gridObj = $("#GridApproved").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.PdfPrintApproved = [{

        type: "details",
        buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintApproved
        }
    }];

    $scope.onClickExcelPrintCheckedReject = function (args) {
        var gridObj = $("#GridCheckedRejectId").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintCheckedReject = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintCheckedReject
        }
    }];









    $scope.onClickExcelPrintApprovedHold = function (args) {
        var gridObj = $("#GridApprovedHold").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintApprovedHold = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintApprovedHold
        }
    }];


    $scope.onClickExcelPrintApprovedReject = function (args) {
        var gridObj = $("#GridApprovedReject").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintApprovedReject = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintApprovedReject
        }
    }];

    $scope.onClickExcelPrintApproved = function (args) {
        var gridObj = $("#GridApproved").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetExpensesBookingReport?reportFormat=' + reportFormat + '&expensesBookingId=' + data.Id, '_blank');
    };
    $scope.ExcelPrintApproved = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintApproved
        }
    }];


    $scope.onClickPdfPrintPosted = function (args) {
        var gridObj = $("#GridPosted").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.PdfPrintPosted = [{

        type: "details",
        buttonOptions: {
            text: "PDF",
            width: "50",
            height: "20",
            click: $scope.onClickPdfPrintPosted
        }
    }];


    $scope.onClickExcelPrintPosted = function (args) {
        var gridObj = $("#GridPosted").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open('Employees/EmployeeReport/GetEmployeePayableExpenseReport?reportFormat=' + reportFormat + '&voucherId=' + data.VoucherId, '_blank');
    };
    $scope.ExcelPrintPosted = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "50",
            height: "20",
            click: $scope.onClickExcelPrintPosted
        }
    }];

    $scope.activityOrderType = "";
    $scope.GLGeneralInfoId = 0;
    $scope.BudgetMasterId = 0;
    $scope.ActivityId = 0;
    $scope.getExpenseDistribute = function (index, item) {
        $scope.activityOrderType = "";
        $scope.TotalChargesAmount = 0;
        $scope.GLGeneralInfoId = 0;
        $scope.BudgetMasterId = 0;
        $scope.ActivityId = 0;
        $scope.activityOrderType = item.ActivityOrderType;
        $scope.TotalChargesAmount = item.Amount;
        $scope.GLGeneralInfoId = item.GLGeneralInfoId;
        $scope.BudgetMasterId = item.BudgetMasterId;
        $scope.ActivityId = item.ActivityId;

        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.isSet(1);
            $scope.calDistributedAmount();
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.isSet(2);
            $scope.calOutBoundDistributedAmount();
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.isSet(1);
            $scope.calDistributedAmount();
            $scope.calOutBoundDistributedAmount();
        }
        else if ($scope.activityOrderType == "Order") {
            $scope.isSet(3);
            $scope.calMasterOrderDistributedAmount();
        }
        else if ($scope.activityOrderType == "Contract") {
            $scope.isSet(4);
            $scope.calContractDistributedAmount();
        }

        angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("show");
    };

    $scope.closeExpenseDistributePopUp = function () {
        angular.element(document.querySelector("#ExpenseDistributePopUp")).modal("hide");
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.checkedInvoiceList = [];
    $scope.VendorAvailableInvoiceList = [];
    $scope.showInvoicePopUp = function () {
        $http({
            method: 'GET',
            url: 'accounts/Invoice/GetVendorAvailableInvoiceList1',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VendorAvailableInvoiceList = response.data;

            if (baseService.arrayLength($scope.checkedInvoiceList) > 0) {
                for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
                    for (var j = 0; j < baseService.arrayLength($scope.VendorAvailableInvoiceList); j++) {
                        if ($scope.checkedInvoiceList[i].InvoiceId == $scope.VendorAvailableInvoiceList[j].InvoiceId) {
                            $scope.VendorAvailableInvoiceList[j].Active = true;
                        }
                    }
                }
            }
        });

        angular.element(document.querySelector('#InboundInvoicePopUp')).modal('show');

    };
    function checkLCExist(list, InvoiceId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === InvoiceId) {

                return true;
            }
        }
        return false;
    }
    $scope.hideInvoicePopUp = function () {
        angular.element(document.querySelector("#InboundInvoicePopUp")).modal("hide");
    };
    $scope.checkedOutBoundInvoiceList = [];
    $scope.CustomerAvailableInvoiceList = [];
    $scope.showOutBoundInvoicePopUp = function () {
        try {
            $http({
                method: 'GET',
                url: 'accounts/CustomerInvoice/GetCustomerAvailableReceivableData',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.CustomerAvailableInvoiceList = response.data;
                if (baseService.arrayLength($scope.checkedOutBoundInvoiceList) > 0) {
                    for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
                        for (var j = 0; j < baseService.arrayLength($scope.CustomerAvailableInvoiceList); j++) {
                            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.CustomerAvailableInvoiceList[j].InvoiceId) {
                                $scope.CustomerAvailableInvoiceList[j].Active = true;
                            }
                        }
                    }
                }
            });
        } catch (e) {
            throw e;
        }
        angular.element(document.querySelector('#OutBoundInvoicePopUp')).modal('show');
    };
    $scope.hideOutBoundInvoicePopUp = function () {
        angular.element(document.querySelector("#OutBoundInvoicePopUp")).modal("hide");
    };
    $scope.ShowResultMasterOrderPopUp = function () {
        $scope.GetMasterOrderList();
        angular.element(document.querySelector('#masterOrderPopUp')).modal('show');
    }
    $scope.masterOrderList = [];
    $scope.GetMasterOrderList = function () {
        $scope.masterOrderList = [];
        $http({
            method: 'GET',
            url: "accounts/CustomerInvoice/GetMasterOrderPopUp"
        }).then(function (response) {
            $scope.masterOrderList = response.data;
        });
    }
    $scope.CloseMasterOrder = function () {
        angular.element(document.querySelector('#masterOrderPopUp')).modal('hide');
    }
    $scope.ShowResultContractPopUp = function () {
        $scope.getcontractList();
        angular.element(document.querySelector('#contractPopUp')).modal('show');
    }
    $scope.contractList = [];
    $scope.getcontractList = function () {
        $scope.contractList = [];
        $http.get("Commercial/Contract/getlist")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.contractList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.CloseContract = function () {
        angular.element(document.querySelector('#contractPopUp')).modal('hide');
    }
    $scope.TotalInvoiceAmount = 0;
    $scope.getTotalInvoiceAmount = function () {
        $scope.TotalInvoiceAmount = 0;
        if ($scope.activityOrderType == "InboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            if (baseService.arrayLength($scope.checkedInvoiceList) > 0)
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
            if (baseService.arrayLength($scope.checkedOutBoundInvoiceList))
                $scope.TotalInvoiceAmount += parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        }

    }
    $scope.TotalChargesAmount = 0;
    $scope.calDistributedAmount = function myfunction() {
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            if ($scope.checkedInvoiceList.length == 1) {
                $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
            }
            else {
                if ($scope.checkedInvoiceList.length - 1 == i) {

                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                    $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }

    }
    $scope.calOutBoundDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.voucherDetailList), "Amount"));
        $scope.getTotalInvoiceAmount();
        $scope.TotalDistributedInvoiceAmount = 0;

        $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }

        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

            if ($scope.checkedOutBoundInvoiceList.length == 1) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
            }
            else {
                if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                }
                else {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                }
            }
        }

    }
    $scope.calMasterOrderDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = 0;
        for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
            $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;

        }

    }
    $scope.calContractDistributedAmount = function myfunction() {
        //$scope.TotalChargesAmount = 0;
        for (var i = 0; i < $scope.checkedContractList.length; i++) {
            $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

        }

    }
    $scope.calReDistributedAmount = function myfunction(index, item) {
        $scope.TotalChargesAmount = parseFloat($scope.budgetTransactionDetailList[index].Amount);
        $scope.activityOrderType = "";
        $scope.activityOrderType = item.ActivityOrderType;
        if ($scope.activityOrderType == "InboundInvoice") {
            $scope.getTotalInvoiceAmount();
            $scope.TotalDistributedInvoiceAmount = 0;
            $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
            var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

            for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                $scope.checkedInvoiceList[i].DistributedAmount = 0;
            }

            for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                if ($scope.checkedInvoiceList.length == 1) {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                }
                else {
                    if ($scope.checkedInvoiceList.length - 1 == i) {

                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                        $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                    }
                    else {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "OutboundInvoice") {
            $scope.getTotalInvoiceAmount();
            $scope.TotalDistributedInvoiceAmount = 0;

            $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
            var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

            for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
            }

            for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                if ($scope.checkedOutBoundInvoiceList.length == 1) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                }
                else {
                    if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                    }
                    else {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "BothInOutboundInvoice") {
            $scope.getTotalInvoiceAmount();
            $scope.TotalDistributedInvoiceAmount = 0;

            $scope.TotalDistributedAmountInvoice = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
            var totali = parseFloat(($scope.TotalDistributedAmountInvoice * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

            for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                $scope.checkedInvoiceList[i].DistributedAmount = 0;
            }

            for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
                if ($scope.checkedInvoiceList.length == 1) {
                    $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                    $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                }
                else {
                    if ($scope.checkedInvoiceList.length - 1 == i) {

                        $scope.TotalDistributedInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
                        $scope.checkedInvoiceList[i].DistributedAmount = (totali - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                    }
                    else {
                        $scope.checkedInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedInvoiceList[i].BooksAmount) * $scope.TotalChargesAmount / $scope.TotalInvoiceAmount).toFixed(2);
                    }
                }
            }

            $scope.TotalDistributedAmountout = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
            var tatalout = parseFloat(($scope.TotalDistributedAmountout * $scope.TotalChargesAmount) / $scope.TotalInvoiceAmount);

            for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
                $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
            }

            for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {

                if ($scope.checkedOutBoundInvoiceList.length == 1) {
                    $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                    $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");
                }
                else {
                    if ($scope.checkedOutBoundInvoiceList.length - 1 == i) {
                        $scope.TotalDistributedInvoiceAmount = $filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount");

                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = (tatalout - $scope.TotalDistributedInvoiceAmount).toFixed(2);
                    }
                    else {
                        $scope.checkedOutBoundInvoiceList[i].DistributedAmount = parseFloat(parseFloat($scope.checkedOutBoundInvoiceList[i].BooksAmount) * parseFloat($scope.TotalChargesAmount) / $scope.TotalInvoiceAmount).toFixed(2);
                    }
                }
            }
        }
        else if ($scope.activityOrderType == "Order") {
            for (var i = 0; i < $scope.checkedMasterOrderList.length; i++) {
                $scope.checkedMasterOrderList[i].DistributedAmount = $scope.TotalChargesAmount;

            }
        }
        else if ($scope.activityOrderType == "Contract") {
            for (var i = 0; i < $scope.checkedContractList.length; i++) {
                $scope.checkedContractList[i].DistributedAmount = $scope.TotalChargesAmount;

            }
        }

    }

    $scope.totalBooksAmount = 0;
    $scope.totalDistributedAmount = 0;
    $scope.InBoundInvoiceAmount = 0; $scope.OutBoundInvoiceAmount = 0;
    $scope.InBoundDistributed = 0; $scope.OutBoundDistributed = 0;
    $scope.totalBooksAmountCal = function () {

        $scope.InBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "BooksAmount"));
        $scope.OutBoundInvoiceAmount = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "BooksAmount"));
        $scope.totalBooksAmount = parseFloat($scope.InBoundInvoiceAmount + $scope.OutBoundInvoiceAmount)

        $scope.InBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedInvoiceList), "DistributedAmount"));
        $scope.OutBoundDistributed = parseFloat($filter("sumByKey")($filter("filter")($scope.checkedOutBoundInvoiceList), "DistributedAmount"));
        $scope.totalDistributedAmount = parseFloat($scope.InBoundDistributed + $scope.OutBoundDistributed)
    }
    $scope.AddInvoice = function () {

        if (baseService.arrayLength($scope.VendorAvailableInvoiceList) > 0) {
            $scope.checkedInvoiceList = [];
            angular.forEach($scope.VendorAvailableInvoiceList, function (a) {
                if (a.Active) {
                    $scope.checkedInvoiceList.push({
                        InvoiceId: a.InvoiceId
                        , InvoiceDetailId: a.InvoiceDetailId
                        , Amount: a.Receivable
                        , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                        , DistributedAmount: 0
                        , ChargesAmount: 0
                        , TaxAmount: 0
                        , Active: true
                        , PostingDate: a.PostingDate
                        , PartyPlantName: a.PartyPlantName
                        , CurrencyCode: a.CurrencyCode
                        , VoucherNo: a.VoucherNo
                        , InvoiceType: 'InboundInvoice'
                        , GLGeneralInfoId: $scope.GLGeneralInfoId
                        , BudgetMasterId: $scope.BudgetMasterId
                        , ActivityId: $scope.ActivityId
                        , DocRefNo: a.DocRefNo
                    });
                }
            });
        }

        $scope.hideInvoicePopUp();
        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();
    };
    $scope.checkedOutBoundInvoiceList = [];
    $scope.AddIOutBoundInvoice = function () {
        if (baseService.arrayLength($scope.CustomerAvailableInvoiceList) > 0) {
            angular.forEach($scope.CustomerAvailableInvoiceList, function (a) {
                if (checkLCExist($scope.checkedOutBoundInvoiceList, a.InvoiceId) === false) {
                    if (a.Active) {
                        $scope.checkedOutBoundInvoiceList.push({
                            InvoiceId: a.InvoiceId
                            , InvoiceDetailId: a.InvoiceDetailId
                            , Amount: a.Receivable
                            , BooksAmount: a.Receivable * a.CompanyCurrencyRate
                            , DistributedAmount: 0
                            , ChargesAmount: 0
                            , TaxAmount: 0
                            , Active: true
                            , PostingDate: a.PostingDate
                            , PartyPlantName: a.PartyPlantName
                            , CurrencyCode: a.CurrencyCode
                            , VoucherNo: a.VoucherNo
                            , InvoiceType: 'OutboundInvoice'
                            , GLGeneralInfoId: $scope.GLGeneralInfoId
                            , BudgetMasterId: $scope.BudgetMasterId
                            , ActivityId: $scope.ActivityId
                            , DocRefNo: a.DocRefNo
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.checkedOutBoundInvoiceList, function (a) {
                if (!baseService.valueCheckInList($scope.checkedOutBoundInvoiceList, 'Id', a.InvoiceId))
                    $scope.checkedOutBoundInvoiceList.splice(a, 1);
            });
        $scope.hideOutBoundInvoicePopUp();
        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();
    };
    $scope.checkedMasterOrderList = [];
    $scope.AddOrder = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.masterOrderList) > 0) {
            $scope.checkedMasterOrderList = [];
            $scope.checkedMasterOrderList.push({
                InvoiceId: null
                , InvoiceDetailId: null
                , Amount: 0
                , BooksAmount: 0
                , DistributedAmount: 0
                , ChargesAmount: 0
                , TaxAmount: 0
                , Active: true
                , PostingDate: ""
                , PartyPlantName: a.InvoicingPartyPlant
                , CurrencyCode: ""
                , VoucherNo: ""
                , InvoiceType: 'Order'
                , GLGeneralInfoId: $scope.GLGeneralInfoId
                , BudgetMasterId: $scope.BudgetMasterId
                , ActivityId: $scope.ActivityId
                , MasterOrderId: a.MasterOrderId
                , ContractId: null
                , CustomerName: a.CustomerName
                , InvoicingPartyPlant: a.InvoicingPartyPlant
                , DeliveryPartyPlant: a.DeliveryPartyPlant
                , Type: a.Type
            });
        }



        $scope.CloseMasterOrder();
        $scope.calMasterOrderDistributedAmount();

    };
    $scope.checkedContractList = [];
    $scope.AddContract = function (x) {
        var a = x.rowData;
        if (baseService.arrayLength($scope.contractList) > 0) {
            $scope.checkedContractList = [];
            $scope.checkedContractList.push({
                InvoiceId: null
                , InvoiceDetailId: null
                , Amount: 0
                , BooksAmount: 0
                , DistributedAmount: 0
                , ChargesAmount: 0
                , TaxAmount: 0
                , Active: true
                , PostingDate: ""
                , PartyPlantName: ""
                , CurrencyCode: ""
                , VoucherNo: ""
                , InvoiceType: 'Contract'
                , GLGeneralInfoId: $scope.GLGeneralInfoId
                , BudgetMasterId: $scope.BudgetMasterId
                , ActivityId: $scope.ActivityId
                , MasterOrderId: null
                , ContractId: a.Id
                , ContractNo: a.ContractNo
                , UDNo: a.UDNo
                , CustomerName: a.CustomerName
                , Buyer: a.Buyer
                , Remarks: a.Remarks
            });
        }



        $scope.CloseContract();
        $scope.calContractDistributedAmount();

    };

    $scope.DeleteConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteConfirmationPopUp")).modal("show");
    };
    $scope.RemoveInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedInvoiceList); i++) {
            if ($scope.checkedInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedInvoiceList.splice(i, 1);
        }

        for (var i = 0; i < $scope.checkedInvoiceList.length; i++) {
            $scope.checkedInvoiceList[i].DistributedAmount = 0;
        }
        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();

    }
    $scope.DeleteOutBoutConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteOutBoundConfirmationPopUp")).modal("show");
    };
    $scope.RemoveOutBoundInvoice = function () {

        for (var i = 0; i < baseService.arrayLength($scope.checkedOutBoundInvoiceList); i++) {
            if ($scope.checkedOutBoundInvoiceList[i].InvoiceId == $scope.InvoiceId)
                $scope.checkedOutBoundInvoiceList.splice(i, 1);
        }
        for (var i = 0; i < $scope.checkedOutBoundInvoiceList.length; i++) {
            $scope.checkedOutBoundInvoiceList[i].DistributedAmount = 0;
        }
        $scope.calDistributedAmount();
        $scope.calOutBoundDistributedAmount();
        $scope.totalBooksAmountCal();
    }
    $scope.DeleteMasterOrderConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteMasterOrderConfirmationPopUp")).modal("show");
    };
    $scope.RemoveMasterOrderInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedMasterOrderList); i++) {
            if ($scope.checkedMasterOrderList[i].MasterOrderId == $scope.InvoiceId)
                $scope.checkedMasterOrderList.splice(i, 1);
        }

        $scope.calMasterOrderDistributedAmount();
    }
    $scope.DeleteContractConfirmation = function (InvoiceId) {
        $scope.InvoiceId = InvoiceId;
        $scope.message_conf = "Are you sure to Delete?";
        angular.element(document.querySelector("#DeleteContractConfirmationPopUp")).modal("show");
    };
    $scope.RemoveContractInvoice = function () {
        for (var i = 0; i < baseService.arrayLength($scope.checkedContractList); i++) {
            if ($scope.checkedContractList[i].ContractId == $scope.InvoiceId)
                $scope.checkedContractList.splice(i, 1);
        }

        $scope.calContractDistributedAmount();
    }
}