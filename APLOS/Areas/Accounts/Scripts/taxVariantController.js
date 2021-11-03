'use strict';
TaxVariantController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function TaxVariantController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Tax Variant';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.taxVariantList = [];
    $scope.taxVariantDetailList = [];
    $scope.path = 'accounts/taxcategory/';
    $scope.getListUrl = $scope.path + 'GetTaxVariantList/';
    $scope.getSeqUrl = $scope.path + 'GetTaxVariantAutoSequence';
    $scope.saveUrl = $scope.path + 'CreateTaxVariant';
    $scope.updateUrl = $scope.path + 'EditTaxVariant';
    $scope.deleteUrl = $scope.path + 'DeleteTaxVariant/';

    $scope.taxVariant = {
        Id: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , CompanyGroupId: window.companyGroupId
        , CountryId: null
        , TaxFor: null
        , DifferentIn: null
        , Different: 'Same'
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.TaxVariantDetail = {
        Id: null
        , TaxVariantId: null
        , TaxCategoryId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
    }
    $scope.taxVariantNew = Object.assign({}, $scope.taxVariant);

    $scope.getList = function () {
        baseService.init($scope.getListUrl);
        $scope.getData = function (pageno) {
            $rootScope.parameters.countryId = $scope.taxVariantNew.CountryId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.taxVariantList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.taxVariantNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.countryList = [];
    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    cboService.getEnumCbo("enum/GetTaxForCbo", function (result) {
        $scope.taxForList = result;
    });

    cboService.getEnumCbo("enum/GetDifferentInCbo", function (result) {
        $scope.differentInList = result;
    });

    $scope.taxCategorySearchList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.taxCategoryParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.taxCategoryList = [];
        baseService.setCurrentPage('taxCategoryList');
        $scope.taxCategoryData = function (pageno) {
            baseService.paginationBase('Accounts/TaxCategory/GetList?countryId=' + $scope.taxVariantNew.CountryId, pageno, $scope.taxCategoryParameters)
                .then(function (result) {
                    $scope.taxCategoryList = result.Rows;
                    $scope.taxCategoryParameters.total_count = result.total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'TaxCategoryPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#TaxCategoryPopUp')).modal('show');
        $scope.taxCategoryData();
    };
    $scope.taxCategorySelect = function (data) {
        $scope.TaxVariantDetail.TaxCategoryId = data.Id;
        $scope.TaxVariantDetail.Code = data.Code;
        $scope.TaxVariantDetail.ShortName = data.ShortName;
        $scope.TaxVariantDetail.StandardName = data.StandardName;
        $scope.TaxVariantDetail.UserName = data.UserName;
        $scope.closeGLPopUp();
        $scope.addRow();
    };
    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector('#TaxCategoryPopUp')).modal('hide');
    };
    $scope.addRow = function () {
        var ob = Object.assign({}, $scope.TaxVariantDetail);
        if (baseService.valueCheckInList($scope.taxVariantDetailList, 'TaxCategoryId', ob.TaxCategoryId))
            return ShowResult(ob.UserName + " already added.", 'failure');
        $scope.taxVariantDetailList.push(ob);
    }

    function clear() {
        $scope.gLMappingOb.Id = null;
        $scope.gLMappingOb.GLGeneralInfoName = null;
        $scope.gLMappingOb.GLGeneralInfoId = null;
        $scope.gLMappingOb.BudgetMasterId = null;
        $scope.gLMappingOb.BudgetName = null;
        $scope.gLMappingOb.ActivityId = null;
        $scope.gLMappingOb.ActivityName = null;
        $scope.gLMappingOb.OldGLId = null;
    }
    //Deleting Rows from GLMappingList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempTaxVariantOb = data;
        $scope.taxVariantIndex = index;
        $scope.message_confirmation = 'Are you sure want to parmenently delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempTaxVariantOb.Id))
            $scope.taxVariantDetailList.splice($scope.taxVariantIndex, 1);
        else {
            $http({
                method: 'POST',
                url: 'Accounts/TaxCategory/DeleteTaxVariantDetail',
                dataType: 'JSON',
                data: { 'id': $scope.tempTaxVariantOb.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taxVariantDetailList.splice($scope.taxVariantIndex, 1);
                    $scope.taxVariantIndex = -1;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        $scope.taxVariantIndex = -1;
        $scope.tempTaxVariantOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };

    //end region
    $scope.getTaxVariantDetailListData = function (id) {
        $http.get("Accounts/TaxCategory/GetTaxVariantDetailList?masterId=" + id)
            .then(
            function successCallback(response) {
                $scope.taxVariantDetailList = response.data.Rows;
            },
            function errorCallback(response) {
                ShowResult(response, 'failure');
            });
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.taxVariant = $scope.taxVariantList[$scope.index];
        $scope.taxVariantNew = Object.assign({}, $scope.taxVariant);
        $scope.getTaxVariantDetailListData($scope.taxVariantNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.taxVariantNewForm.$valid) {
            if ($scope.taxVariantNew.TaxFor === 'OverseasSales' || $scope.taxVariantNew.TaxFor === 'OverseasPurchase') {
                $scope.taxVariantNew.DifferentIn = null;
                $scope.taxVariantNew.Different = null;
            }
            angular.copy($scope.taxVariantNew, $scope.taxVariant);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'taxVariant': $scope.taxVariant, 'taxVariantDetail': $scope.taxVariantDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.taxVariantList.push(response.data.TaxVariant);
                        $scope.taxVariantList = $filter('orderBy')($scope.taxVariantList, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'taxVariant': $scope.taxVariant, 'taxVariantDetail': $scope.taxVariantDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.taxVariantList[$scope.index] = $scope.taxVariant;
                            $scope.taxVariantList = $filter('orderBy')($scope.taxVariantList, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.taxVariantNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.taxVariantNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.taxVariantList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.taxVariant = {};
        $scope.taxVariantNew = { CountryId: $scope.taxVariantNew.CountryId, Sequence: seq, Different: 'Same' };
        $scope.taxVariantDetailList = [];
        $scope.TaxVariantDetail = {};
        $scope.taxVariantNew.Active = true;
    }
}