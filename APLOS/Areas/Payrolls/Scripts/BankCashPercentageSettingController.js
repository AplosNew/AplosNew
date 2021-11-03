'use strict';
BankCashPercentageSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BankCashPercentageSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Bank Cash Percentage Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/BankCashPercentageSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);

    $scope.getData = function () {
        $scope.ModelNew.Id = null;
        $scope.CashDetail.Id = null;
        $scope.ModelNew.FormulaDescription = null;
        $scope.CashDetail.CashFormulaDescription = null;
        $scope.ModelNew.FormulaIDDescription = null;
        $scope.CashDetail.FormulaIDDescription = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.CashFormulaArray = [];
        $scope.CashFormulaIdArray = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { PlantId: $scope.ModelNew.PlantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data.bank) > 0) {
                $scope.ModelNew.Id = response.data.bank[0].Id;
                $scope.ModelNew.FormulaDescription = response.data.bank[0].FormulaDescription;
                $scope.ModelNew.FormulaIDDescription = response.data.bank[0].FormulaIDDescription; 

                if (response.data.bank[0].FormulaDescription != null) {
                    var str = $scope.ModelNew.FormulaDescription;
                    $scope.FormulaArray = str.split(" ");

                    var strId = $scope.ModelNew.FormulaIDDescription;
                    $scope.FormulaIdArray = strId.split(" ");
                }
                
                }
            if (baseService.arrayLength(response.data.cash) > 0) {
                $scope.CashDetail.Id = response.data.cash[0].Id;
                $scope.CashDetail.CashFormulaDescription = response.data.cash[0].CashFormulaDescription;
                $scope.CashDetail.FormulaIDDescription = response.data.cash[0].FormulaIDDescription;    
                if (response.data.cash[0].CashFormulaDescription != null) {
                    var str = $scope.CashDetail.CashFormulaDescription;
                    $scope.CashFormulaArray = str.split(" ");

                    var strId = $scope.CashDetail.FormulaIDDescription;
                    $scope.CashFormulaIdArray = strId.split(" ");
                }
            }
            

            //console.log($scope.CashDetail);
        });
    }


    $scope.ModelTemp = {
        Id: null,
        HeadLabel: null,
        FormulaDescription: null,
        FormulaIDDescription: null,
        PlantId: null

    };
    $scope.ModelCash = {
        Id: null,
        HeadLabel: null,
        FormulaDescription: null,
        FormulaIDDescription: null,
        PlantId: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.CurrencyDetailTemp = {
        Id: null,
        HeadLabel: null,
        CashFormulaDescription: null,
        FormulaIDDescription: null,
        PlantId: null

    };
    $scope.CashDetail = Object.assign({}, $scope.CurrencyDetailTemp);
    $scope.salaryHeadList = [];
    //cboService.getSlrHeadCbo(function (result) {
    //    $scope.salaryHeadList = result;
    //});
    //Formula For Bank
    $scope.checkFormula = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }
    $scope.ModelNew.FormulaDes = null;
    $scope.ModelNew.FormulaDesID = null;
    $scope.ModelNew.SalaryHeadFormula = null;
    $scope.ModelNew.FormulaDescription = null;
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];
    $scope.SetFormula = function (formula) {
        try {

            if (formula === 'SHead') {

                if (!baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadIdFormula)) {

                    $scope.ModelNew.FormulaDescription = null;
                    $scope.ModelNew.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.ModelNew.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.ModelNew.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.ModelNew.FormulaDes = $scope.Formula;
                            $scope.ModelNew.FormulaDesID = $scope.ModelNew.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                            $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);
                        }
                        else {
                            $scope.ModelNew.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.ModelNew.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.ModelNew.FormulaDes = $scope.Formula;
                            $scope.ModelNew.FormulaDesID = $scope.ModelNew.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                            $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);
                        }
                    }
                    else {
                        $scope.ModelNew.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                        var str = $scope.ModelNew.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');

                        $scope.ModelNew.FormulaDes = $scope.Formula;
                        $scope.ModelNew.FormulaDesID = $scope.ModelNew.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);
                    }
                }

                $scope.ModelNew.FormulaDescription = null;
                $scope.ModelNew.FormulaIDDescription = null;

                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                        $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaIDDescription)) {
                        $scope.ModelNew.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.ModelNew.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.ModelNew.Operator)) {

                    $scope.ModelNew.FormulaDescription = null;
                    $scope.ModelNew.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.ModelNew.FormulaDes = $scope.ModelNew.Operator;
                        $scope.ModelNew.FormulaDesID = $scope.ModelNew.Operator;
                        $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);
                    }

                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                            $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaIDDescription)) {
                            $scope.ModelNew.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.ModelNew.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelNew.Precedence)) {

                    $scope.ModelNew.FormulaDescription = null;
                    $scope.ModelNew.FormulaIDDescription = null;

                    $scope.ModelNew.FormulaDes = $scope.ModelNew.Precedence;
                    $scope.ModelNew.FormulaDesID = $scope.ModelNew.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.ModelNew.FormulaDes)) {
                        $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);

                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                                $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }

                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaIDDescription)) {
                                $scope.ModelNew.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.ModelNew.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }

                    }
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelNew.Value)) {

                    $scope.ModelNew.FormulaDescription = null;
                    $scope.ModelNew.FormulaIDDescription = null;

                    $scope.ModelNew.FormulaDes = $scope.ModelNew.Value;
                    $scope.ModelNew.FormulaDesID = $scope.ModelNew.Value;


                    if (!baseService.isUndefinedOrNull($scope.ModelNew.FormulaDes)) {
                        $scope.FormulaArray.push($scope.ModelNew.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ModelNew.FormulaDesID);
                    }


                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                            $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaIDDescription)) {
                            $scope.ModelNew.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.ModelNew.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.CurrencyList = [];
    cboService.getCurrencyCbo(function (result) {
        $scope.CurrencyList = result;
    });


    $scope.Save = function () {
        try {
            $scope.ModelNew.HeadLabel = 'Bank';
            $scope.ModelCash.HeadLabel = 'Cash';
            $scope.ModelCash.Id = $scope.CashDetail.Id;
            $scope.ModelCash.FormulaDescription = $scope.CashDetail.CashFormulaDescription;
            $scope.ModelCash.FormulaIDDescription = $scope.CashDetail.FormulaIDDescription;
            $scope.ModelCash.PlantId = $scope.ModelNew.PlantId;
            if ($scope.ModelCash.FormulaDescription != null && $scope.ModelNew.FormulaDescription === null || $scope.ModelNew.FormulaDescription === '' || $scope.ModelNew.FormulaDescription === 'undifined') {
                throw "Enter Bank Value";
            }
            else if ($scope.ModelNew.FormulaDescription != null && $scope.ModelCash.FormulaDescription === null || $scope.ModelCash.FormulaDescription === '' || $scope.ModelCash.FormulaDescription === 'undifined') {
                throw "Enter Cash Value";
            }
            if ($scope.ModelNew.PlantId == null || $scope.ModelNew.PlantId == '' || $scope.ModelNew.PlantId == 'undifined') {
                throw "Select Plant First";
            }            
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'bp': $scope.ModelNew, 'cp': $scope.ModelCash },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$scope.ModelNew.Id = response.data.bpid;
                    //$scope.CashDetail.Id = response.data.cpid;
                    $scope.getData();
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
        
    };
    
    $scope.checkFormulaCash = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }
    $scope.CashDetail.FormulaDes = null;
    $scope.CashDetail.FormulaDesID = null;
    $scope.CashDetail.CashSalaryHeadFormula = null;
    $scope.CashDetail.CashFormulaDescription = null;
    $scope.CashFormulaArray = [];
    $scope.CashFormulaIdArray = [];
    $scope.SetCashFormula = function (formula) {
        try {
            if (formula === 'CashSHead') {
                if (!baseService.isUndefinedOrNull($scope.CashDetail.CashSalaryHeadIdFormula)) {
                    $scope.CashDetail.CashFormulaDescription = null;
                    $scope.CashDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.CashFormulaArray[$scope.CashFormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormulaCash($scope.OperatorList, lastvalue)) {
                            $scope.CashDetail.CashSalaryHeadFormula = $("#CashSalaryHeadFormula option:selected").text();
                            var str = $scope.CashDetail.CashSalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.CashDetail.FormulaDes = $scope.Formula;
                            $scope.CashDetail.FormulaDesID = $scope.CashDetail.CashSalaryHeadIdFormula;
                            $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                            $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                        }
                        else {
                            $scope.CashDetail.CashSalaryHeadFormula = $("#CashSalaryHeadFormula option:selected").text();
                            var str = $scope.CashDetail.CashSalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.CashDetail.FormulaDes = $scope.Formula;
                            $scope.CashDetail.FormulaDesID = $scope.CashDetail.CashSalaryHeadIdFormula;
                            $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                            $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                        }
                    }
                    else {
                        $scope.CashDetail.CashSalaryHeadFormula = $("#CashSalaryHeadFormula option:selected").text();
                        var str = $scope.CashDetail.CashSalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.CashDetail.FormulaDes = $scope.Formula;
                        $scope.CashDetail.FormulaDesID = $scope.CashDetail.CashSalaryHeadIdFormula;
                        $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                        $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                    }
                }
                $scope.CashDetail.CashFormulaDescription = null;
                $scope.CashDetail.FormulaIDDescription = null;
                for (var i = 0; i < $scope.CashFormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.CashDetail.CashFormulaDescription)) {
                        $scope.CashDetail.CashFormulaDescription = $scope.CashFormulaArray[i];
                    }
                    else {
                        $scope.CashDetail.CashFormulaDescription += ' ' + $scope.CashFormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.CashFormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.CashDetail.FormulaIDDescription)) {
                        $scope.CashDetail.FormulaIDDescription = $scope.CashFormulaIdArray[i];
                    }
                    else {
                        $scope.CashDetail.FormulaIDDescription += ' ' + $scope.CashFormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.CashDetail.Operator)) {
                    $scope.CashDetail.CashFormulaDescription = null;
                    $scope.CashDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.CashFormulaArray[$scope.CashFormulaArray.length - 1];
                    if ($scope.checkFormulaCash($scope.OperatorList, lastvalue) === false) {
                        $scope.CashDetail.FormulaDes = $scope.CashDetail.Operator;
                        $scope.CashDetail.FormulaDesID = $scope.CashDetail.Operator;
                        $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                        $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.CashFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.CashDetail.CashFormulaDescription)) {
                            $scope.CashDetail.CashFormulaDescription = $scope.CashFormulaArray[i];
                        }
                        else {
                            $scope.CashDetail.CashFormulaDescription += ' ' + $scope.CashFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.CashFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.CashDetail.FormulaIDDescription)) {
                            $scope.CashDetail.FormulaIDDescription = $scope.CashFormulaIdArray[i];
                        }
                        else {
                            $scope.CashDetail.FormulaIDDescription += ' ' + $scope.CashFormulaIdArray[i];
                        }
                    }
                } else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'Precedence') {
                if (!baseService.isUndefinedOrNull($scope.CashDetail.Precedence)) {
                    $scope.CashDetail.CashFormulaDescription = null;
                    $scope.CashDetail.FormulaIDDescription = null;
                    $scope.CashDetail.FormulaDes = $scope.CashDetail.Precedence;
                    $scope.CashDetail.FormulaDesID = $scope.CashDetail.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.CashDetail.FormulaDes)) {
                        $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                        $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                        for (var i = 0; i < $scope.CashFormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.CashDetail.CashFormulaDescription)) {
                                $scope.CashDetail.CashFormulaDescription = $scope.CashFormulaArray[i];
                            }
                            else {
                                $scope.CashDetail.CashFormulaDescription += ' ' + $scope.CashFormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.CashFormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.CashDetail.FormulaIDDescription)) {
                                $scope.CashDetail.FormulaIDDescription = $scope.CashFormulaIdArray[i];
                            }
                            else {
                                $scope.CashDetail.FormulaIDDescription += ' ' + $scope.CashFormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'Value') {
                if (!baseService.isUndefinedOrNull($scope.CashDetail.Value)) {
                    $scope.CashDetail.CashFormulaDescription = null;
                    $scope.CashDetail.FormulaIDDescription = null;
                    $scope.CashDetail.FormulaDes = $scope.CashDetail.Value;
                    $scope.CashDetail.FormulaDesID = $scope.CashDetail.Value;

                    if (!baseService.isUndefinedOrNull($scope.CashDetail.FormulaDes)) {
                        $scope.CashFormulaArray.push($scope.CashDetail.FormulaDes);
                        $scope.CashFormulaIdArray.push($scope.CashDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.CashFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.CashDetail.CashFormulaDescription)) {
                            $scope.CashDetail.CashFormulaDescription = $scope.CashFormulaArray[i];
                        }
                        else {
                            $scope.CashDetail.CashFormulaDescription += ' ' + $scope.CashFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.CashFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.CashDetail.FormulaIDDescription)) {
                            $scope.CashDetail.FormulaIDDescription = $scope.CashFormulaIdArray[i];
                        }
                        else {
                            $scope.CashDetail.FormulaIDDescription += ' ' + $scope.CashFormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {
        $scope.ModelNew.FormulaDesID = null;
        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);
        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);
        $scope.ModelNew.FormulaDescription = null;
        $scope.ModelNew.FormulaIDDescription = null;
        $scope.ModelNew.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                $scope.ModelNew.FormulaDes = $scope.FormulaArray[i];
                $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
            } else {
                $scope.ModelNew.FormulaDes += $scope.FormulaArray[i];
                $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaIDDescription)) {
                $scope.ModelNew.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.ModelNew.FormulaIDDescription = $scope.FormulaIdArray[i];
            } else {
                $scope.ModelNew.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.ModelNew.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

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
    $scope.CashRemoveFormula = function () {
        $scope.CashDetail.FormulaDesID = null;
        var count = $scope.CashFormulaArray.length;
        $scope.CashFormulaArray.splice(count - 1);
        var count = $scope.CashFormulaIdArray.length;
        $scope.CashFormulaIdArray.splice(count - 1);
        $scope.CashDetail.CashFormulaDescription = null;
        $scope.CashDetail.FormulaIDDescription = null;
        $scope.CashDetail.FormulaDes = null;
        for (var i = 0; i < $scope.CashFormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.CashDetail.CashFormulaDescription)) {
                $scope.CashDetail.FormulaDes = $scope.CashFormulaArray[i];
                $scope.CashDetail.CashFormulaDescription = $scope.CashFormulaArray[i];
            } else {
                $scope.CashDetail.FormulaDes += $scope.CashFormulaArray[i];
                $scope.CashDetail.CashFormulaDescription += ' ' + $scope.CashFormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.CashFormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.CashDetail.FormulaIDDescription)) {
                $scope.CashDetail.FormulaDesID = $scope.CashFormulaIdArray[i];
                $scope.CashDetail.FormulaIDDescription = $scope.CashFormulaIdArray[i];
            } else {
                $scope.CashDetail.FormulaDesID += $scope.CashFormulaIdArray[i];
                $scope.CashDetail.FormulaIDDescription += ' ' + $scope.CashFormulaIdArray[i];
            }
        }
    }

    $scope.plantList = [];
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

    //$scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Payrolls/EmployeeFixedServicMaster/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.salaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();
}