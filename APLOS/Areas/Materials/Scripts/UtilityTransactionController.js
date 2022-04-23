'use strict';
UtilityTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UtilityTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Utility Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/UtilityTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Date', name: "Date" }, { value: 'Category', name: "Category" }, { value: 'SubCategory', name: "SubCategory" }, { value: 'Quantity', name: "Quantity" }, { value: 'Remarks', name: "Remarks" }];

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        CategoryId: null,
        Category: null,
        SubCategoryId: null,
        SubCategory: null,
        Quantity: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //data: { Id: $scope.ModelNew.Id },
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.subCategoryList = [];
    $scope.GetSubCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Materials/UtilityTransaction/GetSubCategoryList'
        }).then(function successCallback(response) {
            $scope.subCategoryList = response.data;
        });
    }
    $scope.GetSubCategoryList();

    $scope.categoryList = [];
    $scope.GetCategoryList = function () {
        $http({
            method: 'GET',
            url: 'Materials/UtilityTransaction/GetCategoryList'
        }).then(function successCallback(response) {
            $scope.categoryList = response.data;
        });
    }
    $scope.GetCategoryList();


    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.partyList = [];
    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + '' + '&PlantId=' + '';

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.utilityDetails = [];
    $scope.getUtilityGridData = function (id) {
        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityData",
            data: { 'UtilityMasterId': id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.utilityDetails = response.data;
            //$scope.ModelChildNew = Object.assign({}, response.data);
        });
    }

    $scope.UtilityDetaildoubleclick = function (args) {
        $scope.ModelChildNew = Object.assign({}, args);
    };

    $scope.removeUtilityDetailsRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmUtilityDetailsRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeUtilityDetailsRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/UtilityMaster/utilityDetailsDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getUtilityGridData($scope.ModelNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    //$scope.ModelChild = {
    //    Id: null,
    //    UtilityMasterId: null,
    //    EffectiveDate: null,
    //    Rate: 0,
    //    Remark: null
    //};
    //$scope.ModelChildNew = Object.assign({}, $scope.ModelChild);

    //$scope.SaveChild = function () {
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveChildUrl,
    //            data: { 'data': $scope.ModelChildNew, 'UtilityMasterId': $scope.ModelNew.Id },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.getData();
    //                $scope.getUtilityGridData($scope.ModelNew.Id);
    //                $scope.ClearUtilityDetail();
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //};

    //$scope.Clear = function () {
    //    ClearFields($scope.GetSequence());
    //    return true;
    //};

    //function ClearFields(seq) {
    //    $scope.Action = 'Save';
    //    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //    $scope.ModelNew.Sequence = seq;
    //    $scope.ModelChildNew = Object.assign({}, $scope.ModelChild);
    //}

    $scope.Clear = function () {
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.Action = 'Save';
    }
}