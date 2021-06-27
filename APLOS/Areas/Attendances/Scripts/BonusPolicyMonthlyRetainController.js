'use strict';
BonusPolicyMonthlyRetainController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function BonusPolicyMonthlyRetainController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Monthly Retain Bonus Policy';
    $scope.path = 'Attendances/BonusPolicyMonthlyRetain/';
    $scope.Action = 'Save';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.saveUrl1 = $scope.path + 'CreateDetails';
    $scope.saveUrl2 = $scope.path + 'CreateDistribution';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';
    $scope.EOperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.companyList = [];

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.BnsPlcMthRetain.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }
    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    // Master Model
    $scope.BnsPlcMthRetain = {
        ID: null,
        BnsPlcMthRetainName: null,
        BnsPlcMthRetainDescription: null,
        IsAllEmpApplocable: false,
        IsIndividual: false,
        GroupID: null,
        PlantID: null
    };

    //Month Model
    $scope.BnsPlcMthRetainMthNoTemp = {
        BnsPlcMthRetainMstID: null,
        MonthNo: null,
        MonthName: null
    };
    $scope.BnsPlcMthRetainMthNo = Object.assign({}, $scope.BnsPlcMthRetainMthNoTemp);
    // Details Model
    $scope.BnsPlcMthRetainDetailTemp = {
        ID: null,
        BnsPlcMthRetainID: null,
        FormulaDesEarning: null,
        FormulaDesIDEarning: null,
        SalaryHeadIDEarning: null,
        EarningValueRangeFrom: null,
        EarningValueRangeTo: null,
        IsMandatory: false,
        IsFixed: false,
        FixedValue: 0,
        IsFormula: false,
        IsDependOnEarning: false,
        IsMinWages: false,
        CompMinWagesAndOrginal: null,
        GroupID: null,
        PlantID: null,
        FormulaDes: null,
        FormulaDesID: null,
        FormulaDescription: null,
        FormulaIDDescription: null,
        SalaryHeadIdFormula: null,
        SalaryHeadID: null
    };
    $scope.BnsPlcMthRetainDetail = Object.assign({}, $scope.BnsPlcMthRetainDetailTemp);

    // Detail max
    $scope.BnsPlcMthRetainDistribution = {
        ID: null,
        BonusPolicyDetailsID: null,
        FstValue: null,
        FstSalaryHeadID: null,
        SndValue: 0,
        SndSalaryHeadID: null
    };

    $scope.radiovalue = false;
    $scope.radioFixedValue = false;
    $scope.radioFormulaValue = false;
    $scope.radioMinValue = false;
    $scope.setRadioFixedValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = true;
        $scope.radioFormulaValue = false;
        $scope.BnsPlcMthRetainDetail.IsFixed = true;
        $scope.BnsPlcMthRetainDetail.IsFormula = false;
    }
    $scope.setRadioFormulaValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = false;
        $scope.radioFormulaValue = true;
        $scope.BnsPlcMthRetainDetail.IsFormula = true;
        $scope.BnsPlcMthRetainDetail.IsFixed = false;
    }

    $scope.setRadioMinValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = false;
        $scope.radioFormulaValue = true;
        if ($scope.BnsPlcMthRetainDetail.IsMinWages)
            $scope.radioMinValue = true;
        else
            $scope.radioMinValue = false;
        $scope.BnsPlcMthRetainDetail.IsFormula = true;
        $scope.BnsPlcMthRetainDetail.IsFixed = false;
    }

    // Formula For Earning Value Calculation
    $scope.EcheckFormula = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    $scope.BnsPlcMthRetainDetail.FormulaDes = null;
    $scope.BnsPlcMthRetainDetail.FormulaDesID = null;
    $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = null;
    $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
    $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
    $scope.EFormulaArray = [];
    $scope.EFormulaIdArray = [];
    $scope.SetEarningFormula = function (formula) {
        try {
            if (formula === 'EarningSHead') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.SalaryHeadIDEarning)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
                    var lastvalue = $scope.EFormulaArray[$scope.EFormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.EcheckFormula($scope.EOperatorList, lastvalue)) {
                            $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                            $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIDEarning;
                            $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                            $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                            $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIDEarning;
                            $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                            $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        }
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                        var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                        $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIDEarning;
                        $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                }
                $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
                $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
                for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesEarning)) {
                        $scope.BnsPlcMthRetainDetail.FormulaDesEarning = $scope.EFormulaArray[i];
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                    }
                }                

                for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesIDEarning)) {
                        $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'EarningOperator') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.EOperator)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
                    var lastvalue = $scope.EFormulaArray[$scope.EFormulaArray.length - 1];
                    if ($scope.EcheckFormula($scope.EOperatorList, lastvalue) === false) {
                        $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.EOperator;
                        $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.EOperator;
                        $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesEarning)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDesEarning = $scope.EFormulaArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesIDEarning)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                        }
                    }
                }
                else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'EarningPrecedence') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.Precedence)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.Precedence;
                    $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDes)) {
                        $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesEarning)) {
                                $scope.BnsPlcMthRetainDetail.FormulaDesEarning = $scope.EFormulaArray[i];
                            }
                            else {
                                $scope.BnsPlcMthRetainDetail.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesIDEarning)) {
                                $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                            }
                            else {
                                $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'EarningValue') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.EValue)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.EValue;
                    $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.EValue;
                    if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDes)) {
                        $scope.EFormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesEarning)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDesEarning = $scope.EFormulaArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesIDEarning)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveEarningFormula = function () {
        $scope.BnsPlcMthRetainDetail.FormulaDesID = null;
        var count = $scope.EFormulaArray.length;
        $scope.EFormulaArray.splice(count - 1);
        var count = $scope.EFormulaIdArray.length;
        $scope.EFormulaIdArray.splice(count - 1);
        $scope.BnsPlcMthRetainDetail.FormulaDesEarning = null;
        $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = null;
        $scope.BnsPlcMthRetainDetail.FormulaDes = null;
        for (var i = 0; i < $scope.EFormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesEarning)) {
                $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.EFormulaArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDesEarning = $scope.EFormulaArray[i];
            } else {
                $scope.BnsPlcMthRetainDetail.FormulaDes += $scope.EFormulaArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDesIDEarning)) {
                $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.EFormulaIdArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
            } else {
                $scope.BnsPlcMthRetainDetail.FormulaDesID += $scope.EFormulaIdArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
            }
        }
    }

    //Formula For Bonus Value Conrp.
    $scope.BnsPlcMthRetainDetail.FormulaDes = null;
    $scope.BnsPlcMthRetainDetail.FormulaDesID = null;
    $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = null;
    $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
    $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];
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
    $scope.SetFormula = function (formula) {
        try {
            if (formula === 'SHead') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.SalaryHeadIdFormula)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                            var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                            $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                            $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                            var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                            $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                            $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        }
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                        var str = $scope.BnsPlcMthRetainDetail.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.Formula;
                        $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                }
                $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
                $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDescription)) {
                        $scope.BnsPlcMthRetainDetail.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaIDDescription)) {
                        $scope.BnsPlcMthRetainDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.BnsPlcMthRetainDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.Operator)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.Operator;
                        $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.Operator;
                        $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDescription)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaIDDescription)) {
                            $scope.BnsPlcMthRetainDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                }
                else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'Precedence') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.Precedence)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.Precedence;
                    $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDes)) {
                        $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDescription)) {
                                $scope.BnsPlcMthRetainDetail.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.BnsPlcMthRetainDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaIDDescription)) {
                                $scope.BnsPlcMthRetainDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.BnsPlcMthRetainDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'Value') {
                if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.Value)) {
                    $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
                    $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.BnsPlcMthRetainDetail.Value;
                    $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.BnsPlcMthRetainDetail.Value;
                    if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDes)) {
                        $scope.FormulaArray.push($scope.BnsPlcMthRetainDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.BnsPlcMthRetainDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDescription)) {
                            $scope.BnsPlcMthRetainDetail.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaIDDescription)) {
                            $scope.BnsPlcMthRetainDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.BnsPlcMthRetainDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                }
            }
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveFormula = function () {
        $scope.BnsPlcMthRetainDetail.FormulaDesID = null;
        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);
        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);
        $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
        $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
        $scope.BnsPlcMthRetainDetail.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaDescription)) {
                $scope.BnsPlcMthRetainDetail.FormulaDes = $scope.FormulaArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDescription = $scope.FormulaArray[i];
            }
            else {
                $scope.BnsPlcMthRetainDetail.FormulaDes += $scope.FormulaArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.FormulaIDDescription)) {
                $scope.BnsPlcMthRetainDetail.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
            }
            else {
                $scope.BnsPlcMthRetainDetail.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.BnsPlcMthRetainDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    //Get  Master Data
    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetMaster",
            data: { PlantID: $scope.BnsPlcMthRetain.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }

    //Get Month Data
    $scope.MonthList = [];
    $scope.getMonths = function (masterID) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMonths",
            data: { MasterID: masterID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MonthList = response.data;
        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }


    //Get Details Data
    $scope.DetailsList = [];
    $scope.getDetails = function (obj) {
        $scope.BnsPlcMthRetain = obj.data;
        $http({
            method: 'POST',
            url: $scope.path + "GetDetails",
            data: { BnsPlcMthRetainID: $scope.BnsPlcMthRetain.ID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;
            //$scope.getDistributions($scope.BnsPlcMthRetainDetail.ID);
        });
        $scope.getMonths($scope.BnsPlcMthRetain.ID);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }


    // get Detail
    $scope.getSavedDetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDetails",
            data: { BnsPlcMthRetainID: $scope.BnsPlcMthRetain.ID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;
            //$scope.getDistributions($scope.BnsPlcMthRetainDetail.ID);
        });
        $scope.getMonths($scope.BnsPlcMthRetain.ID);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }


    //Submit Button For months
    $scope.MonthList = [];
    $scope.SubmitMonths = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.BnsPlcMthRetainMthNo.MonthName)) {
                throw "Select Month First"
            }
            var newObj = Object.assign({}, $scope.BnsPlcMthRetainMthNo);
            $scope.MonthList.push(newObj);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };


    // Save Function for Master and Months
    $scope.Save = function () {
        try {
            ValidationMaster();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.BnsPlcMthRetain, 'months': $scope.MonthList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.getData();            
                    ShowResult(response.data.Message, 'success');
                    $scope.BnsPlcMthRetain.ID = response.data.Data;
                    $scope.getSavedDetails($scope.BnsPlcMthRetain);
                    //$scope.getMonths();
                    // $scope.getDetails();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    // Save Function for Details
    $scope.SaveD = function () {
        $scope.BnsPlcMthRetainDetail.PlantID = $scope.BnsPlcMthRetain.PlantID;
        $scope.BnsPlcMthRetainDetail.GroupID = $scope.BnsPlcMthRetain.GroupID;
        $scope.BnsPlcMthRetainDetail.BnsPlcMthRetainID = $scope.BnsPlcMthRetain.ID;
        if ($scope.BnsPlcMthRetainDetail.IsFixed == false) {
            $scope.BnsPlcMthRetainDetail.FixedValue = 0;
        }
        if ($scope.BnsPlcMthRetainDetail.IsFormula == false) {
            $scope.BnsPlcMthRetainDetail.IsDependOnEarning = false;
            $scope.BnsPlcMthRetainDetail.IsMinWages = false;
            $scope.BnsPlcMthRetainDetail.CompMinWagesAndOrginal = null;
            $scope.BnsPlcMthRetainDetail.SalaryHeadIdFormula = null;
            $scope.BnsPlcMthRetainDetail.FormulaDescription = null;
            $scope.BnsPlcMthRetainDetail.FormulaIDDescription = null;
        }
        $http({
            method: 'POST',
            url: $scope.saveUrl1,
            data: { 'details': $scope.BnsPlcMthRetainDetail },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                // $scope.getData();
                $scope.ClearD();
                $scope.getSavedDetails($scope.BnsPlcMthRetain);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    // Adding LineItem for Distribution
    $scope.ShowDiv = false;

    $scope.AddLineItem = function () {
        try {
            var gridObj = $("#BPolicyId").data("ejGrid");
            var _do = gridObj.getSelectedRecords()[0];
            $scope.BonusPolicyDetailsID = _do.ID;
            $scope.ShowDiv = true;
            var eDialog = $("#BPMRDistribution").data("ejDialog");
            eDialog.open();
            $scope.BnsPlcMthRetainDistribution = {
                ID: null,
                BonusPolicyDetailsID: $scope.BonusPolicyDetailsID,
                FstValue: null,
                FstSalaryHeadID: null,
                SndValue: null,
                SndSalaryHeadID: null
            };
            $scope.getDistributions(_do.ID);
        }
        catch (e) {
            ShowResult(e, "failure");
        }

    };


    // Save Function for Distribution
    $scope.SaveDis = function () {
        var gridObj = $("#BPolicyId").data("ejGrid");
        var _do = gridObj.getSelectedRecords()[0];
        $scope.BnsPlcMthRetainDistribution.BonusPolicyDetailsID = _do.ID;
        $http({
            method: 'POST',
            url: $scope.saveUrl2,
            data: { 'distribution': $scope.BnsPlcMthRetainDistribution },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.getDetails();
                $scope.getSavedDetails($scope.BnsPlcMthRetain);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    // Validation
    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Plant", $scope.BnsPlcMthRetain.PlantID);
            CheckField("Policy Name", $scope.BnsPlcMthRetain.BnsPlcMthRetainName);
            CheckField("Month", $scope.BnsPlcMthRetainMthNo.MonthName);
        } catch (ex) {
            throw ex;
        }
    };

    // add new pop up
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#BPolicyId").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();
                $("#BPolicyId").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };


    $scope.recorddoubleclickDetails = function (obj) {
        angular.copy(obj.data, $scope.BnsPlcMthRetainDetail);
        //var gridObj = $("#BPolicyId").data("ejGrid");
        //$scope.BnsPlcMthRetainDetail = gridObj.getSelectedRecords()[0];
        //$scope.BnsPlcMthRetainDetail = obj.data;


        if ($scope.BnsPlcMthRetainDetail.FormulaDesEarning != null) {
            var strP = $scope.BnsPlcMthRetainDetail.FormulaDesEarning;
            $scope.EFormulaArray = strP.split(" ");

            var strIdP = $scope.BnsPlcMthRetainDetail.FormulaDesIDEarning;
            $scope.EFormulaIdArray = strIdP.split(" ");
        }

        if ($scope.BnsPlcMthRetainDetail.FormulaDescription != null) {
            var str = $scope.BnsPlcMthRetainDetail.FormulaDescription;
            $scope.FormulaArray = str.split(" ");

            var strId = $scope.BnsPlcMthRetainDetail.FormulaIDDescription;
            $scope.FormulaIdArray = strId.split(" ");
        }

        try {
            $scope.ShowDiv = true;
            var eDialog = $("#BPMRDetails").data("ejDialog");
            eDialog.open();
            $scope.Action = 'Update';
            $scope.getListDetailsData();
        } catch (e) {
        }

    };

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#BPMRDetails").data("ejDialog");
            eDialog.open();

            $scope.BnsPlcMthRetainDetail = {
                ID: null,
                BnsPlcMthRetainID: $scope.BnsPlcMthRetain.ID,
                FormulaDesEarning: null,
                FormulaDesIDEarning: null,
                SalaryHeadIDEarning: null,
                EarningValueRangeFrom: null,
                EarningValueRangeTo: null,
                IsMandatory: false,
                IsFixed: null,
                FixedValue: null,
                IsFormula: false,
                IsDependOnEarning: null,
                IsMinWages: null,
                CompMinWagesAndOrginal: null,
                GroupID: null,
                PlantID: null,
                FormulaDes: null,
                FormulaDesID: null,
                FormulaDescription: null,
                FormulaIDDescription: null,
                SalaryHeadID: null
            };
            $scope.BnsPlcMthRetainDetail.BnsPlcMthRetainID = $scope.BnsPlcMthRetain.ID;
        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    // popup delete
    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };

    // Clear Master
    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.BnsPlcMthRetain = {
            ID: null,
            BnsPlcMthRetainName: null,
            BnsPlcMthRetainDescription: null,
            IsAllEmpApplocable: false,
            IsIndividual: false,
            GroupID: $scope.BnsPlcMthRetain.GroupID,
            PlantID: $scope.BnsPlcMthRetain.PlantID,
            CompanyId: $scope.BnsPlcMthRetain.CompanyId,
        };
        $scope.BnsPlcMthRetainMthNo = {
            ID: null,
            BnsPlcMthRetainMstID: null,
            MonthNo: null,
            MonthName: null
        };
        $scope.BnsPlcMthRetainDetail = {
            ID: null,
            BnsPlcMthRetainID: null,
            FormulaDesEarning: null,
            FormulaDesIDEarning: null,
            SalaryHeadIDEarning: null,
            EarningValueRangeFrom: null,
            EarningValueRangeTo: null,
            IsMandatory: false,
            IsFixed: null,
            FixedValue: null,
            IsFormula: false,
            IsDependOnEarning: null,
            IsMinWages: null,
            CompMinWagesAndOrginal: null,
            GroupID: $scope.BnsPlcMthRetainDetail.GroupID,
            PlantID: $scope.BnsPlcMthRetainDetail.PlantID,
            FormulaDes: null,
            FormulaDesID: null,
            FormulaDescription: null,
            FormulaIDDescription: null,
            SalaryHeadID: null
        };
        $scope.DetailsList = [];
        $scope.MonthList = [];
        //$scope.ModelList.response.data[0].MonthName = [];
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];

    };



    //Get Distribution Data
    $scope.getDistributions = function (ID) {
        var detail_id = ID;
        $http({
            method: 'POST',
            url: $scope.path + "GetDistribution",
            data: { detailsID: detail_id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            angular.copy(response.data[0], $scope.BnsPlcMthRetainDistribution);
            $scope.BnsPlcMthRetainDistribution.BonusPolicyDetailsID = $scope.BonusPolicyDetailsID;
        });
    }

    //Clear Details
    $scope.ClearD = function () {
        $scope.Action = 'Save';
        $scope.BnsPlcMthRetainDetail = {
            ID: null,
            BnsPlcMthRetainID: null,
            FormulaDesEarning: null,
            FormulaDesIDEarning: null,
            SalaryHeadIDEarning: null,
            EarningValueRangeFrom: null,
            EarningValueRangeTo: null,
            IsMandatory: false,
            IsFixed: null,
            FixedValue: null,
            IsFormula: false,
            IsDependOnEarning: null,
            IsMinWages: null,
            CompMinWagesAndOrginal: null,
            GroupID: null,
            PlantID: null,
            FormulaDes: null,
            FormulaDesID: null,
            FormulaDescription: null,
            FormulaIDDescription: null,
            SalaryHeadIdFormula: null,
            SalaryHeadID: null
        };

        $scope.EFormulaArray = [];
        $scope.EFormulaIdArray = [];
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
    };

    //Clear Distribution
    $scope.ClearDis = function () {
        $scope.Action = 'Save';
        $scope.BnsPlcMthRetainDistribution = {
            ID: null,
            BonusPolicyDetailsID: null,
            FstValue: null,
            FstSalaryHeadID: null,
            SndValue: null,
            SndSalaryHeadID: null
        };
    }

    // Delete Master
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMaster?Id=' + $scope.BnsPlcMthRetain.ID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Bonus Policy Monthly Retain Details first!");
            }
            else {

                ShowResult(response.data.Message, 'success');
                //for (var i = 0; i < $scope.ModelList.length; i++) {
                //    if ($scope.ModelList[i].ID == $scope.ModelNew.ID) {
                //        $scope.ModelList.splice(i, 1);
                //    }
                //}
                $scope.Clear();
                $scope.getData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // Delete Month
    $scope.message_confirmation = null;
    $scope.RemoveMonth = function (obj) {
        $scope.BnsPlcMthRetainMthNo = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainMthNo.BnsPlcMthRetainMstID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpMonth')).modal('show');
    }
    $scope.DeleteMonth = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMonth?ID=' + $scope.BnsPlcMthRetainMthNo.BnsPlcMthRetainMstID + '&&monthno=' + $scope.BnsPlcMthRetainMthNo.MonthNo,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Invalid Month ");
            }
            else {

                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.MonthList.length; i++) {
                    if ($scope.MonthList[i].MonthNo == $scope.BnsPlcMthRetain.MonthNo) {
                        $scope.MonthList.splice(i, 1);
                    }
                }
                $scope.getMonths($scope.BnsPlcMthRetain.ID);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    // Delete Details
    $scope.message_confirmation = null;
    $scope.RemoveDetail = function (obj) {
        $scope.BnsPlcMthRetainDetail = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.BnsPlcMthRetainDetail.ID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpDetails')).modal('show');
    }
    $scope.DeleteDetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteDetails?ID=' + $scope.BnsPlcMthRetainDetail.ID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Bonus Policy Monthly Retain Distribution first!");
            }
            else {

                ShowResult(response.data.Message, 'success');
                $scope.getSavedDetails($scope.BnsPlcMthRetain);
                //for (var i = 0; i < $scope.ModelList.length; i++) {
                //    if ($scope.ModelList[i].ID == $scope.BnsPlcMthRetainDetail.ID) {
                //        $scope.ModelList.splice(i, 1);
                //    }
                //}
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    //Delete Distribution
    $scope.DeleteDistribution = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + "DeleteDistribution",
                data: { ID: $scope.BnsPlcMthRetainDistribution.ID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearDis();
                    $scope.ConfirmClose();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}