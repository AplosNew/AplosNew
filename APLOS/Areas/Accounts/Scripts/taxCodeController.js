'use strict';
TaxCodeController.$inject = ["addressService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "cboService"];
function TaxCodeController(addressService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "TaxCode";
    $scope.Action = 'Save';
    $scope.CAction = 'Add';
    $scope.CuAction = 'Add';
    $scope.index = -1;
    $scope.taxcodes = [];
    $scope.taxCodeDetails = [];
    $scope.path = 'accounts/taxcode/';
    $scope.getListUrl = $scope.path + 'gettaxcodelist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
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
        Code: null,
        COAId: null,
        CountryId: null,
        TaxCategoryId: null,
        UserName: null,
        StandardName: null,
        IsCreditable: 'Creditable',
        IsMerge: false,
        IsWithhold: true,
        BaseGrossOrNet: 'Gross',
        InvoiceOrPayment: 'Invoice',
        InputOrOutput: 'Input',
        ManuallyEditable: true,
        Description: null,
        Active: true,
        IsRCM: false,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: new Date(),
        UpdatedFromIP: null
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

    $scope.uncheckMerge = function () {
        $scope.taxCode.IsMerge = false;
    };

    $scope.Get = function (id, index) {
        ClearFields();
        $scope.index = index;
        $scope.taxCode = $scope.taxcodes[$scope.index];
        if ($scope.taxCode.IsCreditable === true)
            $scope.taxCode.IsCreditable = "Creditable";
        else
            $scope.taxCode.IsCreditable = "NonCreditable";
        cboService.getTaxCategoryCboByCountry($scope.taxCode.CountryId, function (result) {
            $scope.TaxCategoryList = result;
        });
        $scope.getData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.cOAList = [];
    cboService.getCboChartOfAccount('', function (result) {
        $scope.cOAList = result;
    });

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.countryList = result;
    });


    $scope.GetTaxCategory = function myfunction(id) {
        $scope.Clear();
        $scope.taxCode.CountryId = id;
        $scope.TaxCategoryList = [];
        $scope.getData();
        cboService.getTaxCategoryCboByCountry(id, function (result) {
            $scope.TaxCategoryList = result;
        });
    };

    $scope.onCOAChange = function (item) {
        baseService.init('accounts/taxcode/gettaxcodelist?coaId=' + item, null, null, null, "Code", "UserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxcodes = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.TaxYearList = [];
    $http({
        method: 'GET',
        url: 'accounts/taxyear/getcbo/'
    }).then(function successCallback(response) {
        $scope.TaxYearList = response.data;
    });

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

    $scope.selectedTaxCodeGlList = [];
    $scope.getSelectedTaxCodeList = function (x, index) {
        if (x.Active && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId) === false) {
            $scope.selectedTaxCodeGlList.push(x);
        } else if (x.Active === false && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId)) {
            for (var i = 0; i < $scope.selectedTaxCodeGlList.length; i++) {
                if ($scope.selectedTaxCodeGlList[i].CompanyId === x.CompanyId) {
                    $scope.selectedTaxCodeGlList.splice(i, 1);
                }
            }
        }
    };

    function checkAvailable(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CompanyId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.changeCreditable = function () {
        if ($scope.taxCode.IsCreditable == "Creditable")
            $scope.taxCode.IsCreditable = true;
        else
            $scope.taxCode.IsCreditable = false;
    };
    $scope.validation = function () {
        if ($scope.taxCode.IsRCM == true && $scope.taxCode.IsWithhold == false) {
                ShowResult("In case of RCM withlold is mandatory!", "failure");
                return true;
            }
            
        return false;
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.changeCreditable();
        if ($scope.taxCodeForm.$valid && !$scope.validation()) {
            try {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'taxcode': $scope.taxCode },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
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
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: { 'taxcode': $scope.taxCode },
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
            } catch (e) {
                ShowResult(e, 'failure');
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taxCode.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taxCode.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taxcodes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.taxCode.Code = null;
        $scope.taxCode.Id = null;
        $scope.taxCode.UserName = null;
        $scope.taxCode.ManuallyEditable = null;
        $scope.taxCode.TaxCategoryId = null;
        $scope.taxCode.StandardName = null;
        $scope.taxCode.Description = null;
        $scope.taxCode.Code = null;
        $scope.taxCode.IsRCM = false;
        $scope.taxCode.Active = true;
        $scope.taxCode.BaseGrossOrNet = 'Gross';
        $scope.taxCode.InvoiceOrPayment = 'Invoice';
        $scope.taxCode.InputOrOutput = 'Input';
        $scope.taxCode.IsWithhold = true;
        $scope.taxCode.IsCreditable = "Creditable";
        $scope.taxCode.IsMerge = false;
        $scope.taxCode.ManuallyEditable = true;
    }
    $scope.tab = 1;

    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}