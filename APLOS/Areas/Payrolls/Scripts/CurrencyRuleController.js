'use strict';
CurrencyRuleController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CurrencyRuleController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Currency Rule';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/CurrencyRule/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.ModelNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList?PlantID=" + $scope.ModelNew.PlantID,
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data.data;
        });
    }
    //$scope.getData();

    $scope.ModelTemp = {
        SystemID: null,
        CurrencyRuleName: null,
        CurrencyDescription: null,
        GroupID: null,
        PlantID: null,
        CompanyId: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.CurrencyDetailTemp = {
        SystemID: null,
        MstSystemId: null,
        SalaryHeadID: null,
        AmtEntryCurrency: null,
        AmtDefinitionCurrency: null,
        AmtDisbusmentCurrency: null,
        AccumulateExchangeRate: false,
        AccumulateExchangeSalaryHeadID: null,
        IntegerInDisb: false,
        RoundOption: null,
        IsDecimalInDisb: false,
        DecimalNo: null,
        NoOfUsed: null
    };
    $scope.CurrencyDetail = Object.assign({}, $scope.CurrencyDetailTemp);

    $scope.recorddoubleclickDetails = function (obj) {        
        $scope.CurrencyDetail = obj.data;
        $scope.CurrencyDetail.DecimalNo = obj.data.DecimalNo.toString();
        if (obj.data.DecimalNo != null) {
            $scope.RadioDecimalValue = true;
        }
        try {
            $scope.Action = 'Update';
        } catch (e) {
        }
        //$scope.getWorkingDayListData();
    };

    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    $scope.CurrencyList = [];
    $scope.LoadCurrency= function getCurrency() {
    cboService.getCurrencyCbo($scope.ModelNew.PlantID, function (result) {
        $scope.CurrencyList = result;
    });
    }

    $scope.getDetail = function (obj) {
        $scope.ModelNew = obj.data;
        var SystemID = $scope.ModelNew.SystemID;
        $http({
            method: 'POST',
            url: $scope.path + "GetDetail",
            data: { SystemID: SystemID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailList = response.data;
        });
        $scope.getData();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    
    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'data': $scope.ModelNew, 'detail': $scope.DetailList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ModelNew.Id = response.data.SystemID;
                $scope.getData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
            CurrencyRuleName: null,
            CurrencyDescription: null,
            GroupID: $scope.ModelNew.GroupID,
            PlantID: $scope.ModelNew.PlantID,
            CompanyId: $scope.ModelNew.CompanyId,
        };
        $scope.CurrencyDetail = Object.assign({}, $scope.CurrencyDetailTemp);
        $scope.DetailList = [];
    }

    //radio button and check button
    $scope.radiovalue = false;
    $scope.newRadiovalue = false;
    $scope.RadioIntegerValue = false;
    $scope.RadioDecimalValue = false;
    $scope.ExchangeRateValue = false;
    $scope.setRadioIntegerValue = function () {
        $scope.radiovalue = true;
        $scope.RadioIntegerValue = true;
        $scope.RadioDecimalValue = false;
        $scope.CurrencyDetail.IntegerInDisb = true;
        $scope.CurrencyDetail.IsDecimalInDisb = false;
    }
    $scope.setRadioDecimalValue = function () {
        $scope.radiovalue = true;
        $scope.RadioIntegerValue = false;
        $scope.RadioDecimalValue = true;
        $scope.CurrencyDetail.IsDecimalInDisb = true;
        $scope.CurrencyDetail.IntegerInDisb = false;
    }


    //validation for submit button
    function check(field, msg) {
        try {
            if (field === null || field === 'undefined' || field === '') {
                throw msg;
            }
        } catch (e) {
            throw e;
        }
    }
    //submit button
    $scope.DetailList = [];
    $scope.Submit = function () {
        try {
            check($scope.CurrencyDetail.AmtEntryCurrency, "Amount Entry Currency cannot be null");
            check($scope.CurrencyDetail.AmtDisbusmentCurrency, "Amount Disbursement Currency cannot be null");
            check($scope.CurrencyDetail.AmtDefinitionCurrency, "Amount Definition Currency Currency cannot be null");
            check($scope.CurrencyDetail.RoundOption, "Fraction Calculation cannot be null");

            var a = $scope.CurrencyDetail.AmtEntryCurrency;
            var b = $scope.CurrencyDetail.AmtDisbusmentCurrency;
            var c = $scope.CurrencyDetail.AmtDefinitionCurrency;

            $scope.CurrencyDetail.AmtEntryCurrencyName = $("#AmtEntryCurrency option:selected").text();
            $scope.CurrencyDetail.AmtDisbusmentCurrencyName = $("#AmtDisbusmentCurrency option:selected").text();
            $scope.CurrencyDetail.AmtDefinitionCurrencyName = $("#AmtDefinitionCurrency option:selected").text();
            $scope.CurrencyDetail.SalaryHead = $("#SalaryHead option:selected").text();

            if (a === b && b === c && c === a) {

            }
            else {
                throw "Currency Should be Same";
            }
            if ($scope.CurrencyDetail.IsDecimalInDisb === false && $scope.CurrencyDetail.IntegerInDisb === false) {
                throw "Select  Integer In Disbursement or Decimal In Disbursement";
            }
            var newObj = Object.assign({}, $scope.CurrencyDetail);
            $scope.DetailList.push(newObj);
            $scope.FormulaArray = [];
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    //Delete part
    $scope.message_confirmation = null;
    $scope.RemoveDetail = function (obj) {
        $scope.CurrencyDetail = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.CurrencyDetail.SystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }
    $scope.DeleteChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Delete?SystemID=' + $scope.CurrencyDetail.SystemID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.DetailList.length; i++) {
                    if ($scope.DetailList[i].SystemID == $scope.CurrencyDetail.SystemID) {
                        $scope.DetailList.splice(i, 1);
                    }
                }
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    $scope.RemoveMaster = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.ModelNew.SystemID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMaster?SystemID=' + $scope.ModelNew.SystemID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Currency Rule Child first!");
            }
            else {
                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.ModelList.length; i++) {
                    if ($scope.ModelList[i].SystemID == $scope.ModelNew.SystemID) {
                        $scope.ModelList.splice(i, 1);
                    }
                }
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

}