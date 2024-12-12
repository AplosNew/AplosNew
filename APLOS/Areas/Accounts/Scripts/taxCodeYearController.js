'use strict';
TaxCodeYearController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function TaxCodeYearController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "TaxCode";
    $scope.Action = 'Save';
    $scope.CAction = 'Add';
    $scope.CuAction = 'Add';
    $scope.index = -1;
    $scope.taxcodes = [];
    $scope.TaxCodeList = [];
    $scope.taxCodeDetails = [];
    $scope.path = 'accounts/taxcode/';
    $scope.getListUrl = $scope.path + 'GetTaxCodeYearList';
    $scope.saveUrl = $scope.path + 'TaxCodeYearInsert';
    $scope.updateUrl = $scope.path + 'TaxCodeYearInsert';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'UserName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.CountryId = $scope.taxCode.CountryId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.taxcodes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.taxCode = {
        Id: null,
        TaxCodeId: null,
        TaxCodeName: null,
        Code: null,
        COAId: null,
        CountryId: null,
        TaxCategoryId: null,
        TaxCategoryName: null,
        UserName: null,
        StandardName: null,
        IsCreditable: 'true',
        IsMerge: false,
        IsWithhold: true,
        BaseGrossOrNet: 'Gross',
        InvoiceOrPayment: 'Invoice',
        InputOrOutput: 'Input',
        ManuallyEditable: true,
        Description: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
    };
    $scope.taxCodeYear = {
        Id: null,
        CountryId: null,
        TaxCodeId: null,
        TaxYearId: null,
        Type: null,
        Active: true
    };
    $scope.taxCodeDetail = {
        Id: null,
        Sequence: 1,
        TaxCodeId: null,
        SlabDefine: null,
        TaxableIncome: null,
        ValueOfFixed: 0,
        TaxRate: null,
        Active: true
    };
    $scope.taxCodeDetailValueOfFixed = {
        ValueOfFixed: 0,
        TaxCodeId: null,
        TaxCodeYearId: null
    };
    $scope.taxCodeGL = {
        Id: null,
        CompanyId: null,
        TaxCodeId: null,
        WithholdCreditableGL: null,
        CreditableGL: null,
        ExpensesGL: null,
        TaxYearId: null,
        Active: true
    };

    $scope.searchtaxcodeByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Base Gross Or Net',
            'value': 'BaseGrossOrNet'
        }
    ];
    $scope.taxCodeAddedRow = [];
    $scope.accTextBoxShow = false;
    $scope.fRowShow = false;
    $scope.sRowShow = false;

    $scope.uncheckMerge = function () {
        $scope.taxCode.IsMerge = false;
    };

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });

    $scope.onCountryChange = function (countryId) {
        $http({
            method: 'GET',
            url: 'accounts/taxcode/GetTaxCodeCbo?countryId=' + countryId
        }).then(function successCallback(response) {
            $scope.TaxCodecboList = response.data;
        });
    };
    $scope.getTaxCode = function (taxcodeId) {
        $http({
            method: 'GET',
            url: 'accounts/taxcode/GetTaxCodeByPKId?taxCodeId=' + taxcodeId
        }).then(function successCallback(response) {
            $scope.TaxCodeList = response.data;
            $scope.taxCode = $scope.TaxCodeList;
            $scope.taxCode.TaxCodeId = $scope.TaxCodeList.Id;
            $scope.taxCode.TaxCodeName = $scope.TaxCodeList.UserName;
            $scope.IsTaxYearDisable = false;
            $scope.Action = 'Save';
        });
    };

    $scope.Get = function (data) {
        $scope.taxCodeYear.TaxYearId = data.TaxYearId;
        $scope.taxCodeYear.Id = data.Id;
        $scope.IsTaxYearDisable = true;
        $scope.taxCodeYear.Type = data.Type;
        if (data.Type === 'Cumulative' || data.Type === 'BreakUp') {
            $scope.getTypeId(data.TaxCodeId, data.Type, data.TaxYearId);
            if (data.Type === 'Cumulative') {
                $scope.accTextBoxShow = false;
                $scope.fRowShow = false;
                $scope.sRowShow = true;
            }
            if (data.Type === 'BreakUp') {
                $scope.accTextBoxShow = false;
                $scope.sRowShow = false;
                $scope.fRowShow = true;
            }
        }
        else {
            $scope.getFixedValueData(data.TaxCodeId, data.Type, data.Id);
            $scope.accTextBoxShow = true;
        }
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetTaxCategory = function myfunction(id) {
        $scope.Clear();
        $scope.taxCode.CountryId = id;
        $scope.TaxCategoryList = [];
        $scope.getData();
        cboService.getTaxCategoryCboByCountry(id, function (result) {
            $scope.TaxCategoryList = result;
        });
    };

    $scope.onTaxCodeChange = function (taxcodeId, countryId) {
        baseService.init('accounts/taxcode/GetTaxCodeYearList?taxCodeId=' + taxcodeId + '&&countryId=' + countryId, null, null, 'DESC', "Id", "TaxYearName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxcodes = result.Rows;
                    $scope.taxCodeYear = {};
                    $scope.taxCodeDetailValueOfFixed = {};
                    $scope.taxCodeAddedRow = [];
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    // #endregion

    cboService.getTaxYearCbo(null, function (result) {
        $scope.TaxYearList = result;
    });

    // #endregion
    $scope.onTaxYearChange = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/taxcode/GetTaxCodeYearIdByTaxYearId?taxyearid=' + item + '&&taxcodeid=' + $scope.taxCode.Id
        }).then(function successCallback(response) {
            $scope.taxCodeYear = response.data;
            $scope.taxCodeYear.TaxCodeId = $scope.taxCode.Id;
            $scope.taxCodeYear.TaxYearId = item;
            if (!baseService.isUndefinedOrNull($scope.taxCodeYear.Id) && !baseService.isUndefinedOrNull($scope.taxCodeYear.Type)) {
                if ($scope.taxCodeYear.Type === 'Cumulative' || $scope.taxCodeYear.Type === 'BreakUp') {
                    $scope.getTypeId($scope.taxCode.Id, $scope.taxCodeYear.Type, $scope.taxCodeYear.Id);
                }
                else {
                    $scope.getFixedValueData($scope.taxCode.Id, $scope.taxCodeYear.Type, $scope.taxCodeYear.Id);
                }
                $scope.changeType($scope.taxCodeYear.Type);
            }
        });
    }

    $scope.changeType = function (type) {
        if (type === "FixedPercentage" || type === "FixedValue") {
            $scope.sRowShow = false;
            $scope.fRowShow = false;
            $scope.accTextBoxShow = true;
            $scope.taxCodeAddedRow[0].ValueOfFixed = $scope.taxCodeDetailValueOfFixed.ValueOfFixed;
        } else if (type === "BreakUp") {
            $scope.accTextBoxShow = false;
            $scope.sRowShow = false;
            $scope.fRowShow = true;
        } else if (type === "Cumulative") {
            $scope.accTextBoxShow = false;
            $scope.fRowShow = false;
            $scope.sRowShow = true;
        }
    }

    // #region OnBalanceAmount Change Disable
    $scope.onbalanceIncomeReadonly = false;
    $scope.OnBalanceAmountChange = function () {
        if ($scope.taxCodeDetail.SlabDefine === "OnBalanceAmount") {
            $scope.onbalanceIncomeReadonly = true
        }
        else
            $scope.onbalanceIncomeReadonly = false;
    }
    // #endregion

    $scope.addRow = function () {
        if ($scope.CAction === 'Add') {
            if ($scope.taxCodeAddedRow.length < 1 && $scope.taxCodeDetail.SlabDefine === "First") {
                $scope.taxCodeAddedRow.push($scope.taxCodeDetail);
            } else if ($scope.taxCodeAddedRow.length >= 1 && $scope.taxCodeDetail.SlabDefine !== "First") {
                $scope.taxCodeDetail.Sequence += 1;
                $scope.taxCodeAddedRow.push($scope.taxCodeDetail);
            }
            else {
                ShowResult("Slab firs must be select for 1st row", 'failure');
            }
            $scope.clearDetailRow();
            console.log($scope.taxCodeAddedRow.length - 1, $scope.taxCodeAddedRow, $scope.taxCodeDetail.SlabDefine, $scope.taxCodeAddedRow[$scope.taxCodeAddedRow.length - 1].SlabDefine)
        }
        if ($scope.indexdetails != -1 && $scope.CAction == 'Update') {
            $scope.taxCodeAddedRow[$scope.indexdetails] = $scope.taxCodeDetail;
            $scope.indexdetails = -1;
            $scope.CAction = 'Add';
            $scope.clearDetailRow();
        }
    };
    // #region
    $scope.addCumulativeRow = function () {
        if ($scope.CuAction === 'Add') {
            if ($scope.taxCodeAddedRow.length < 1 && $scope.taxCodeDetail.SlabDefine === "Cumulative") {
                $scope.taxCodeAddedRow.push($scope.taxCodeDetail);
            } else if ($scope.taxCodeAddedRow.length >= 1) {
                $scope.taxCodeDetail.Sequence += 1;
                $scope.taxCodeAddedRow.push($scope.taxCodeDetail);
            }
            else {
                ShowResult("Slab Cumulative must be select for 1st row", 'failure');
            }
            $scope.clearDetailRow();
        }
        if ($scope.indexdetails !== -1 && $scope.CuAction === 'Update') {
            $scope.taxCodeAddedRow[$scope.indexdetails] = $scope.taxCodeDetail;
            $scope.indexdetails = -1;
            $scope.CuAction = 'Add';
            $scope.clearDetailRow();
        }
    }
    // #endregion
    $scope.valuePassInDelModal = function (index, id) {
        $scope.TaxCodedId = id;
        $scope.TaxCodedIndex = index;
        if (baseService.isUndefinedOrNull($scope.TaxCodedId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.TaxCodedId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeTaxCodeRow = function () {
        $scope.taxCodeAddedRow.splice($scope.TaxCodedIndex, 1);
    };
    $scope.GetTaxCodDetailrow = function (x, id, index) {
        $scope.indexdetails = index;
        $scope.taxCodeDetail = x;
        //$scope.COAIText = x.GLGeneralInfoCode;
        $scope.CAction = 'Update';
    };

    $scope.selectedTaxCodeGlList = [];
    $scope.getSelectedTaxCodeList = function (x) {
        if (x.Active && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId) === false) {
            $scope.selectedTaxCodeGlList.push(x);
        } else if (x.Active === false && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId)) {
            for (var i = 0; i < $scope.selectedTaxCodeGlList.length; i++) {
                if ($scope.selectedTaxCodeGlList[i].CompanyId === x.CompanyId) {
                    $scope.selectedTaxCodeGlList.splice(i, 1);
                }
            }
        }
    }

    function checkAvailable(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CompanyId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.taxCodeDetailFixedValue = [];
    $scope.taxCodeDetailFixedValue.push($scope.taxCodeDetail);
    function checkCalcType() {
        if ($scope.taxCodeAddedRow.length > 0) {
            if (($scope.CAction == 'Add' && $scope.taxCodeAddedRow[$scope.taxCodeAddedRow.length - 1].SlabDefine == 'OnBalanceAmount') || ($scope.CuAction == 'Add' && $scope.taxCodeAddedRow[$scope.taxCodeAddedRow.length - 1].SlabDefine == 'OnBalanceAmount')) {
                return true;
            }
            else {
                ShowResult("Last Slab must be On Balance Amount", 'failure');
                return false;
            }
        }
        else {
            ShowResult("Please add slab row", 'failure');
            return false;
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.taxCodeYear.TaxCodeId = $scope.taxCode.TaxCodeId;
        if ($scope.taxCodeForm.$valid) {
            try {
                if ($scope.Action === "Save") {
                    if ($scope.taxCodeYear.Type === "FixedPercentage" || $scope.taxCodeYear.Type === "FixedValue") {
                        $http({
                            method: 'POST',
                            url: $scope.saveUrl,
                            data: { 'taxcodeyear': $scope.taxCodeYear, 'taxCodeDetail': $scope.taxCodeAddedRow, 'taxCodederailfixedvalue': $scope.taxCodeDetailValueOfFixed },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, "failure");
                            }
                            else {
                                ShowResult(response.data.Message, "success");
                                ClearFields();
                                $scope.getData();
                            }
                        });
                        return true;
                    }
                    else if (checkCalcType()) {
                        $http({
                            method: 'POST',
                            url: $scope.saveUrl,
                            data: { 'taxcodeyear': $scope.taxCodeYear, 'taxCodeDetail': $scope.taxCodeAddedRow, 'taxCodederailfixedvalue': $scope.taxCodeDetailValueOfFixed },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, "failure");
                            }
                            else {
                                ShowResult(response.data.Message, "success");
                                ClearFields();
                                $scope.getData();
                            }
                        });
                        return true;
                    }
                }
                else if ($scope.Action === "Update") {
                    if ($scope.taxCodeYear.Type === "FixedPercentage" || $scope.taxCodeYear.Type === "FixedValue") {
                        $http({
                            method: 'POST',
                            url: $scope.updateUrl,
                            data: { 'taxcodeyear': $scope.taxCodeYear, 'taxCodeDetail': $scope.taxCodeAddedRow, 'taxCodederailfixedvalue': $scope.taxCodeDetailValueOfFixed },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, "failure");
                            }
                            else {
                                ShowResult(response.data.Message, "success");
                                if ($scope.index > -1) {
                                    $scope.taxcodes[$scope.index] = $scope.taxCode;
                                    ClearFields();
                                    $scope.getData();
                                }
                            }
                        }, function errorCallback(response) {
                            ShowResult(response.status.Message, "failure");
                        });
                        return true;
                    }
                    else if (checkCalcType()) {
                        $http({
                            method: 'POST',
                            url: $scope.updateUrl,
                            data: { 'taxcodeyear': $scope.taxCodeYear, 'taxCodeDetail': $scope.taxCodeAddedRow, 'taxCodederailfixedvalue': $scope.taxCodeDetailValueOfFixed },
                            dataType: 'JSON'
                        }).then(function successCallback(response) {
                            if (response.data.Error === true) {
                                ShowResult(response.data.Message, "failure");
                            }
                            else {
                                ShowResult(response.data.Message, "success");
                                if ($scope.index > -1) {
                                    $scope.taxcodes[$scope.index] = $scope.taxCode;
                                    ClearFields();
                                    $scope.getData();
                                }
                            }
                        }, function errorCallback(response) {
                            ShowResult(response.status.Message, "failure");
                        });
                        return true;
                    }
                }
            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
        return true;
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.paymentTerm.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.paymentTerm.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.paymentTerms.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function (response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    }

    // #region *********GetTypeId********
    $scope.getTypeId = function (id, type, taxCodeYearId) {
        $http({
            method: 'GET',
            url: 'accounts/taxcode/gettaxcodedetaillist?id=' + id + '&&type=' + type + '&&taxCodeYearId=' + taxCodeYearId
        }).then(function successCallback(response) {
            $scope.taxCodeAddedRow = response.data.Rows;
            console.log('taxCodeAddedRow', response.data.Rows);
        });
    }

    $scope.getFixedValueData = function (id, type, taxCodeYearId) {
        $http({
            method: 'GET',
            url: 'accounts/taxcode/gettaxcodedetaillist?id=' + id + '&&type=' + type + '&&taxCodeYearId=' + taxCodeYearId
        }).then(function successCallback(response) {
            console.log('taxCodeDetailValueOfFixed', response.data.Rows);

            $scope.taxcodedetailTypeData = response.data.Rows;
            if ($scope.taxcodedetailTypeData.length > 0) {
                if ($scope.taxCodeDetailValueOfFixed.Id === !undefined) {
                    $scope.taxCodeDetailValueOfFixed.Id = $scope.taxcodedetailTypeData[0].Id;
                }
                $scope.taxCodeDetailValueOfFixed.ValueOfFixed = $scope.taxcodedetailTypeData[0].ValueOfFixed;
                $scope.taxCodeDetailValueOfFixed.TaxCodeId = $scope.taxcodedetailTypeData[0].TaxCodeId;
                $scope.taxCodeDetailValueOfFixed.TaxCodeYearId = $scope.taxcodedetailTypeData[0].TaxCodeYearId;
                $scope.taxCodeDetailValueOfFixed.Id = $scope.taxcodedetailTypeData[0].Id;
            }
        });
    }

    $scope.clearDetailRow = function () {
        $scope.taxCodeDetail = { Sequence: 1 };
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.taxCode = {};
        $scope.taxCodeGL = {};
        $scope.accTextBoxShow = false;
        $scope.fRowShow = false;
        $scope.sRowShow = false;
        $scope.taxCodeDetail = {};
        $scope.taxCodeYear = {};
        $scope.taxCodeAddedRow = [];
        $scope.taxCode.Active = true;
        $scope.selectedTaxCodeGlList = [];
        $scope.taxCodeDetailValueOfFixed.ValueOfFixed = 0;
    }
    $scope.tab = 1;

    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.searchByTaxCode = "UserName"; $scope.searchTaxCode = "";
    $scope.searchByTaxCodeList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: "Tax Code" }, { value: 'BaseGrossOrNet', name: "Base Gross Or Net" }];

    $scope.taxCodeLists = [];
    $scope.getTaxCodeDataList = function () {
        $http({
            method: 'POST',
            url: 'accounts/taxcode/GetTaxCodeDataPopUpList',
            data: { column: $scope.searchByTaxCode, value: $scope.searchTaxCode, countryId: $scope.taxCode.CountryId },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.taxCodeLists = response.data;
        });
        angular.element(document.querySelector('#TaxCodePopUpNew')).modal('show');
    };
    $scope.closeTaxCodeDataPopUp = function () {
        angular.element(document.querySelector("#TaxCodePopUpNew")).modal("hide");
    };

    $scope.TaxCodeSelect = function (obj) {
        $scope.model = obj.data;
        $scope.taxCode.TaxCodeId = $scope.model.Id;
        $scope.taxCode.TaxCodeName = $scope.model.UserName;
        $scope.onTaxCodeChange($scope.taxCode.TaxCodeId, $scope.taxCode.CountryId);
        $scope.getTaxCode($scope.taxCode.TaxCodeId);
        $scope.closeTaxCodeDataPopUp();
    };
}