'use strict';
TaxInvestmentMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TaxInvestmentMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Tax Investment Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/TaxInvestmentMaster/';
    $scope.SMSNotificationButton = 'Payrolls/TaxInvestmentMaster/SmsNotification';

    //#region --Model
    $scope.ModelTemp = {
        Id: null,
        CompanyId: null,
        TaxTypeId: null,
        TaxYearId: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion

    //#region --Get--
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.TaxTypeList = [];
    $scope.getTaxGroup = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/TaxPolicy/GetTaxGroup',
        }).then(function successCallback(response) {
            $scope.TaxTypeList = response.data;

        });
    }
    $scope.getTaxGroup();

    $scope.YearList = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/TaxPolicy/GetTaxYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
    }
    $scope.getData();

    $scope.getMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList" ,
            data: { Company: $scope.ModelNew.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }

    //#endregion

    //#region --GEt MAster Details--

    $scope.GetDetails = function (obj) {
        $scope.Action = 'Update';
        $scope.ModelNew = obj.data;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    //#endregion

    //#region --Master Save--
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        
        $http({
            method: 'POST',
            url: $scope.path + "Create",
            data: { 'data': $scope.ModelNew },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
                    $scope.getMasterData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = {
            Id: null,
            CompanyId: null,
            TaxTypeId: null,
            TaxYearId: null,
        };        
    }

    $scope.ClickButton = function () {
        $http({
            method: 'POST',
            url: $scope.SMSNotificationButton ,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //#endregion
}