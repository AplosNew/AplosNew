'use strict';
ESICPolicyController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ESICPolicyController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'ESIC Policy';
    $scope.path = 'Attendances/ESICPolicy/';
    $scope.Action = 'Save';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.saveUrl1 = $scope.path + 'CreateDetails';
    $scope.saveUrl2 = $scope.path + 'CreateDistribution';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';
    //$scope.EOperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    //$scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.EOperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }, { Text: "<=", Value: "<=" }, { Text: ">=", Value: ">=" }, { Text: "<", Value: "<" }, { Text: ">", Value: ">" }];
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }, { Text: "<=", Value: "<=" }, { Text: ">=", Value: ">=" }, { Text: "<", Value: "<" }, { Text: ">", Value: ">" }];
    $scope.companyList = [];
    $scope.HeadList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.ESICPolicyMaster.CompanyID, function (result) {
            $scope.plantList = result;
        });
    }
    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    // Master Model
    $scope.ESICPolicyMaster = {
        ID: null,
        ESICPolicyName: null,
        ESICPolicyDescription: null,
        GroupID: null,
        PlantID: null,
        CompanyID: null
    };
    //Month Model
    $scope.ESICPolicyMonthNo = {
        ESICPolicyMasterID: null,
        MonthNo: null,
        MonthName: null,
    }
    //Leave Model
    $scope.ESICPolicyLeaveTypeTemp = {
        //IsSelectESICLeaveType: false,
        LeaveTypeID: null,
        ESICPolicyMasterID: null,

    };
    $scope.ESICPolicyLeaveType = Object.assign({}, $scope.ESICPolicyLeaveTypeTemp);
    // Details Model
    $scope.ESICPolicyDetailsTemp = {
        //	
        ID: null,
        ESICPolicyMasterID: null,
        FormulaDesEarning: null,
        FormulaDesIDEarning: null,
        SalaryHeadIDEarning: null,
        EarningValueRangeFrom: null,
        EarningValueRangeTo: null,
        IsMandatory: false,
        IsFixedEmp: false,
        FixedValueEmp: 0,
        IsFormulaEmp: true,
        IsContributionSlrHDdependOnEarningEmp: false,
        //FormulaDes: null,
        //FormulaDesID: null,
        FormulaDescription: null,
        FormulaIDDescription: null,
        SalaryHeadIdFormula: null,
        SalaryHeadID: null,
        IsFixedEmployer: false,
        FixedValueEmployer: 0,
        IsFormulaEmployer: true,
        IsContributionSlrHDdependOnEarningEmployer: false,
        FormulaDesEmployer: null,
        FormulaDesIDEmployer: null,
        SalaryHeadIDEmployer: null
    };
    $scope.ESICPolicyDetails = Object.assign({}, $scope.DetailsTemp);


    $scope.radiovalue = true;
    $scope.radioFixedValue = false;
    $scope.radioFixedValueE = false;
    $scope.radioFormulaValue = true;
    $scope.radioFormulaValueE = true;
    //$scope.radioMinValue = false;
    $scope.setRadioFixedValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = true;
        $scope.radioFormulaValue = false;
        $scope.ESICPolicyDetails.IsFixedEmp = true;
        $scope.ESICPolicyDetails.IsFormulaEmp = false;
    }
    $scope.setRadioFormulaValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = false;
        $scope.radioFormulaValue = true;
        $scope.ESICPolicyDetails.IsFormulaEmp = true;
        $scope.ESICPolicyDetails.IsFixedEmp = false;
    }

    $scope.setRadioFixedValueE = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValueE = true;
        $scope.radioFormulaValueE = false;
        $scope.ESICPolicyDetails.IsFixedEmployer = true;
        $scope.ESICPolicyDetails.IsFormulaEmployer = false;
    }
    $scope.setRadioFormulaValueE = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValueE = false;
        $scope.radioFormulaValueE = true;
        $scope.ESICPolicyDetails.IsFormulaEmployer = true;
        $scope.ESICPolicyDetails.IsFixedEmployer = false;
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
    //$scope.Clear = function () {
    //    ClearFields();
    //    return true;
    //};
    $scope.ESICPolicyDetails.FormulaDes = null;
    $scope.ESICPolicyDetails.FormulaDesID = null;
    $scope.ESICPolicyDetails.SalaryHeadFormula = null;
    $scope.ESICPolicyDetails.FormulaDesEarning = null;
    $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
    $scope.EFormulaArray = [];
    $scope.EFormulaIdArray = [];
    $scope.SetEarningFormula = function (formula) {
        try {
            if (formula === 'EarningSHead') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.SalaryHeadIDEarning)) {
                    $scope.ESICPolicyDetails.FormulaDesEarning = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
                    var lastvalue = $scope.EFormulaArray[$scope.EFormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.EcheckFormula($scope.EOperatorList, lastvalue)) {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEarning;
                            $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                        else {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEarning;
                            $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                    }
                    else {
                        $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                        var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEarning;
                        $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                }
                $scope.ESICPolicyDetails.FormulaDesEarning = null;
                $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
                for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEarning)) {
                        $scope.ESICPolicyDetails.FormulaDesEarning = $scope.EFormulaArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EFormulaDesIDEarning)) {
                        $scope.ESICPolicyDetails.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'EarningOperator') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EOperator)) {
                    $scope.ESICPolicyDetails.FormulaDesEarning = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
                    var lastvalue = $scope.EFormulaArray[$scope.EFormulaArray.length - 1];
                    if ($scope.EcheckFormula($scope.EOperatorList, lastvalue) === false) {
                        $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.EOperator;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.EOperator;
                        $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEarning)) {
                            $scope.ESICPolicyDetails.FormulaDesEarning = $scope.EFormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEarning)) {
                            $scope.ESICPolicyDetails.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                        }
                    }
                }
                else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'EarningPrecedence') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.Precedence)) {
                    $scope.ESICPolicyDetails.FormulaDesEarning = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.Precedence;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEarning)) {
                                $scope.ESICPolicyDetails.FormulaDesEarning = $scope.EFormulaArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEarning)) {
                                $scope.ESICPolicyDetails.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'EarningValue') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EValue)) {
                    $scope.ESICPolicyDetails.FormulaDesEarning = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.EValue;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.EValue;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.EFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEarning)) {
                            $scope.ESICPolicyDetails.FormulaDesEarning = $scope.EFormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEarning)) {
                            $scope.ESICPolicyDetails.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveEarningFormula = function () {
        $scope.ESICPolicyDetails.FormulaDesID = null;
        var count = $scope.EFormulaArray.length;
        $scope.EFormulaArray.splice(count - 1);
        var count = $scope.EFormulaIdArray.length;
        $scope.EFormulaIdArray.splice(count - 1);
        $scope.ESICPolicyDetails.FormulaDesEarning = null;
        $scope.ESICPolicyDetails.FormulaDesIDEarning = null;
        $scope.ESICPolicyDetails.FormulaDes = null;
        for (var i = 0; i < $scope.EFormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEarning)) {
                $scope.ESICPolicyDetails.FormulaDes = $scope.EFormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDesEarning = $scope.EFormulaArray[i];
            } else {
                $scope.ESICPolicyDetails.FormulaDes += $scope.EFormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDesEarning += ' ' + $scope.EFormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.EFormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEarning)) {
                $scope.ESICPolicyDetails.FormulaDesID = $scope.EFormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaDesIDEarning = $scope.EFormulaIdArray[i];
            } else {
                $scope.ESICPolicyDetails.FormulaDesID += $scope.EFormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaDesIDEarning += ' ' + $scope.EFormulaIdArray[i];
            }
        }
    }

    //Formula For Employee.
    $scope.ESICPolicyDetails.FormulaDes = null;
    $scope.ESICPolicyDetails.FormulaDesID = null;
    $scope.ESICPolicyDetails.SalaryHeadFormula = null;
    $scope.ESICPolicyDetails.FormulaDescription = null;
    $scope.ESICPolicyDetails.FormulaIDDescription = null;
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
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.SalaryHeadIdFormula)) {
                    $scope.ESICPolicyDetails.FormulaDescription = null;
                    $scope.ESICPolicyDetails.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                        else {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                    }
                    else {
                        $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula1 option:selected").text();
                        var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                }
                $scope.ESICPolicyDetails.FormulaDescription = null;
                $scope.ESICPolicyDetails.FormulaIDDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDescription)) {
                        $scope.ESICPolicyDetails.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaIDDescription)) {
                        $scope.ESICPolicyDetails.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.Operator)) {
                    $scope.ESICPolicyDetails.FormulaDescription = null;
                    $scope.ESICPolicyDetails.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.Operator;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.Operator;
                        $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDescription)) {
                            $scope.ESICPolicyDetails.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaIDDescription)) {
                            $scope.ESICPolicyDetails.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                }
                else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'Precedence') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.Precedence)) {
                    $scope.ESICPolicyDetails.FormulaDescription = null;
                    $scope.ESICPolicyDetails.FormulaIDDescription = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.Precedence;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDescription)) {
                                $scope.ESICPolicyDetails.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaIDDescription)) {
                                $scope.ESICPolicyDetails.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'Value') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.Value)) {
                    $scope.ESICPolicyDetails.FormulaDescription = null;
                    $scope.ESICPolicyDetails.FormulaIDDescription = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.Value;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.Value;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.FormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.FormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDescription)) {
                            $scope.ESICPolicyDetails.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaIDDescription)) {
                            $scope.ESICPolicyDetails.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
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
        $scope.ESICPolicyDetails.FormulaDesID = null;
        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);
        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);
        $scope.ESICPolicyDetails.FormulaDescription = null;
        $scope.ESICPolicyDetails.FormulaIDDescription = null;
        $scope.ESICPolicyDetails.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDescription)) {
                $scope.ESICPolicyDetails.FormulaDes = $scope.FormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDescription = $scope.FormulaArray[i];
            }
            else {
                $scope.ESICPolicyDetails.FormulaDes += $scope.FormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaIDDescription)) {
                $scope.ESICPolicyDetails.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaIDDescription = $scope.FormulaIdArray[i];
            }
            else {
                $scope.ESICPolicyDetails.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    // Formula For employer
    $scope.EmcheckFormula = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }
    //$scope.Clear = function () {
    //    ClearFields();
    //    return true;
    //};
    $scope.ESICPolicyDetails.FormulaDes = null;
    $scope.ESICPolicyDetails.FormulaDesID = null;
    $scope.ESICPolicyDetails.SalaryHeadFormula = null;
    $scope.ESICPolicyDetails.FormulaDesEmployer = null;
    $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
    $scope.EmployerFormulaArray = [];
    $scope.EmployerFormulaIdArray = [];
    $scope.SetEEarningFormula = function (formula) {
        try {
            if (formula === 'EEarningSHead') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.SalaryHeadIDEmployer)) {
                    $scope.ESICPolicyDetails.FormulaDesEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
                    var lastvalue = $scope.EmployerFormulaArray[$scope.EmployerFormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.EmcheckFormula($scope.EOperatorList, lastvalue)) {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula2 option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEmployer;
                            $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                        else {
                            $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula2 option:selected").text();
                            var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                            $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEmployer;
                            $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                            $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        }
                    }
                    else {
                        $scope.ESICPolicyDetails.SalaryHeadFormula = $("#SalaryHeadFormula2 option:selected").text();
                        var str = $scope.ESICPolicyDetails.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.ESICPolicyDetails.FormulaDes = $scope.Formula;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.SalaryHeadIDEmployer;
                        $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                }
                $scope.ESICPolicyDetails.FormulaDesEmployer = null;
                $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
                for (var i = 0; i < $scope.EmployerFormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEmployer)) {
                        $scope.ESICPolicyDetails.FormulaDesEmployer = $scope.EmployerFormulaArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaDesEmployer += ' ' + $scope.EmployerFormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.EmployerFormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EFormulaDesIDEarning)) {
                        $scope.ESICPolicyDetails.FormulaDesIDEmployer = $scope.EmployerFormulaIdArray[i];
                    }
                    else {
                        $scope.ESICPolicyDetails.FormulaDesIDEmployer += ' ' + $scope.EFormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'EEarningOperator') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EEOperator)) {
                    $scope.ESICPolicyDetails.FormulaDesEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
                    var lastvalue = $scope.EmployerFormulaArray[$scope.EmployerFormulaArray.length - 1];
                    if ($scope.EmcheckFormula($scope.EOperatorList, lastvalue) === false) {
                        $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.EEOperator;
                        $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.EEOperator;
                        $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EmployerFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEmployer)) {
                            $scope.ESICPolicyDetails.FormulaDesEmployer = $scope.EmployerFormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesEmployer += ' ' + $scope.EmployerFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EmployerFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEmployer)) {
                            $scope.ESICPolicyDetails.FormulaDesIDEmployer = $scope.EmployerFormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesIDEmployer += ' ' + $scope.EmployerFormulaIdArray[i];
                        }
                    }
                }
                else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'EEarningPrecedence') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.Precedence)) {
                    $scope.ESICPolicyDetails.FormulaDesEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.Precedence;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                        for (var i = 0; i < $scope.EmployerFormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEmployer)) {
                                $scope.ESICPolicyDetails.FormulaDesEmployer = $scope.EmployerFormulaArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaDesEmployer += ' ' + $scope.EmployerFormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.EmployerFormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEmployer)) {
                                $scope.ESICPolicyDetails.FormulaDesIDEmployer = $scope.EmployerFormulaIdArray[i];
                            }
                            else {
                                $scope.ESICPolicyDetails.FormulaDesIDEmployer += ' ' + $scope.EmployerFormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'EEarningValue') {
                if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.EEValue)) {
                    $scope.ESICPolicyDetails.FormulaDesEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
                    $scope.ESICPolicyDetails.FormulaDes = $scope.ESICPolicyDetails.EEValue;
                    $scope.ESICPolicyDetails.FormulaDesID = $scope.ESICPolicyDetails.EEValue;
                    if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDes)) {
                        $scope.EmployerFormulaArray.push($scope.ESICPolicyDetails.FormulaDes);
                        $scope.EmployerFormulaIdArray.push($scope.ESICPolicyDetails.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.EmployerFormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEmployer)) {
                            $scope.ESICPolicyDetails.FormulaDesEmployer = $scope.EmployerFormulaArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesEmployer += ' ' + $scope.EmployerFormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.EmployerFormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEmployer)) {
                            $scope.ESICPolicyDetails.FormulaDesIDEmployer = $scope.EmployerFormulaIdArray[i];
                        }
                        else {
                            $scope.ESICPolicyDetails.FormulaDesIDEmployer += ' ' + $scope.EmployerFormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveEEarningFormula = function () {
        $scope.ESICPolicyDetails.FormulaDesID = null;
        var count = $scope.EmployerFormulaArray.length;
        $scope.EmployerFormulaArray.splice(count - 1);
        var count = $scope.EmployerFormulaIdArray.length;
        $scope.EmployerFormulaIdArray.splice(count - 1);
        $scope.ESICPolicyDetails.FormulaDesEmployer = null;
        $scope.ESICPolicyDetails.FormulaDesIDEmployer = null;
        $scope.ESICPolicyDetails.FormulaDes = null;
        for (var i = 0; i < $scope.EmployerFormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesEmployer)) {
                $scope.ESICPolicyDetails.FormulaDes = $scope.EmployerFormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDesEmployer = $scope.EmployerFormulaArray[i];
            } else {
                $scope.ESICPolicyDetails.FormulaDes += $scope.EmployerFormulaArray[i];
                $scope.ESICPolicyDetails.FormulaDesEmployer += ' ' + $scope.EmployerFormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.EmployerFormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyDetails.FormulaDesIDEmployer)) {
                $scope.ESICPolicyDetails.FormulaDesID = $scope.EmployerFormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaDesIDEmployer = $scope.EmployerFormulaIdArray[i];
            } else {
                $scope.ESICPolicyDetails.FormulaDesID += $scope.EmployerFormulaIdArray[i];
                $scope.ESICPolicyDetails.FormulaDesIDEmployer += ' ' + $scope.EmployerFormulaIdArray[i];
            }
        }
    }

    //Get Master Data
    $scope.ModelNew = [];
    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetMaster?PlantID=" + $scope.ESICPolicyMaster.PlantID,
        }).then(function successCallback(response) {
            $scope.ModelNew = response.data;
        });
    }


    //Get  Leave Data
    $scope.LeaveList = [];
    $scope.getLeave = function (masterID) {
        $http.get("Attendances/ESICPolicy/GetLeaveList?masterID=" + masterID)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.LeaveList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };


    $scope.getLeave();
    //Get Month Data
    $scope.MonthList = [];
    $scope.getMonths = function (masterID) {
        $http({
            method: 'GET',
            url: $scope.path + "GetMonths?MasterID=" + masterID,
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
        $scope.ESICPolicyMaster = obj.data;
        $http({
            method: 'GET',
            url: $scope.path + "GetDetails?masterID=" + $scope.ESICPolicyMaster.ID,
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;
            //$scope.getDistributions($scope.ESICPolicyDetails.ID);
        });
        $scope.getMonths($scope.ESICPolicyMaster.ID);
        $scope.getHeads($scope.ESICPolicyMaster.ID);
        $scope.getLeave($scope.ESICPolicyMaster.ID);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    //$scope.HeadsList = [];
    $scope.getHeads = function (MasterID) {
        $http({
            method: 'GET',
            url: $scope.path + "GetHeads?masterID=" + MasterID,
        }).then(function successCallback(response) {
            $scope.HeadList = response.data;
        });
    }

    // get saved Detail
    $scope.getSavedDetails = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetDetails?masterID=" + $scope.ESICPolicyMaster.ID,
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;
            //$scope.getDistributions($scope.ESICPolicyDetails.ID);
        });
        $scope.getMonths($scope.ESICPolicyMaster.ID);
        $scope.getLeave($scope.ESICPolicyMaster.ID);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }


    //Submit Button For months
    $scope.MonthList = [];
    $scope.SubmitMonths = function () {
        try {
            var newObj = Object.assign({}, $scope.ESICPolicyMonthNo);
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
                data: { 'master': $scope.ESICPolicyMaster, 'months': $scope.MonthList, 'LeaveList': $scope.LeaveList, 'HeadList': $scope.HeadList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.ESICPolicyMaster.ID = response.data.Data;
                    $scope.getData();
                    $scope.getSavedDetails($scope.ESICPolicyMaster);
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
        $scope.ESICPolicyDetails.ESICPolicyMasterID = $scope.ESICPolicyMaster.ID;
        //if (true) {

        //}
        $http({
            method: 'POST',
            url: $scope.saveUrl1,
            data: { 'details': $scope.ESICPolicyDetails },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.ClearD();
                $scope.getSavedDetails($scope.ESICPolicyMaster);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    // Adding LineItem for Distribution
    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#BPMRDetails").data("ejDialog");
            eDialog.open();

            $scope.ESICPolicyDetails = {
                ID: null,
                ESICPolicyMasterID: $scope.ESICPolicyMaster.ID,
                FormulaDesEarning: null,
                FormulaDesIDEarning: null,
                SalaryHeadIDEarning: null,
                EarningValueRangeFrom: null,
                EarningValueRangeTo: null,
                IsMandatory: false,
                IsFixedEmp: false,
                FixedValueEmp: 0,
                IsFormulaEmp: true,
                IsContributionSlrHDdependOnEarningEmp: false,
                FormulaDes: null,
                FormulaDesID: null,
                FormulaDescription: null,
                FormulaIDDescription: null,
                SalaryHeadIdFormula: null,
                SalaryHeadID: null,
                IsFixedEmployer: false,
                FixedValueEmployer: 0,
                IsFormulaEmployer: true,
                IsContributionSlrHDdependOnEarningEmployer: false,
                FormulaDesEmployer: null,
                FormulaDesIDEmployer: null,
                SalaryHeadIDEmployer: null
            };
            $scope.ESICPolicyDetails.ESICPolicyMasterID = $scope.ESICPolicyMaster.ID;
        } catch (e) {
            ShowResult(e, "failure");
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
            CheckField("Plant", $scope.ESICPolicyMaster.PlantID);
            CheckField("Policy Name", $scope.ESICPolicyMaster.ESICPolicyName);
            //CheckField("Month", $scope.ESICPolicyMonthNo.MonthName);
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
        angular.copy(obj.data, $scope.ESICPolicyDetails);
        //var gridObj = $("#BPolicyId").data("ejGrid");
        //$scope.ESICPolicyDetails = gridObj.getSelectedRecords()[0];
        //$scope.ESICPolicyDetails = obj.data;


        if ($scope.ESICPolicyDetails.FormulaDesEarning != null) {
            var strP = $scope.ESICPolicyDetails.FormulaDesEarning;
            $scope.EFormulaArray = strP.split(" ");

            var strIdP = $scope.ESICPolicyDetails.FormulaDesIDEarning;
            $scope.EFormulaIdArray = strIdP.split(" ");
        }

        if ($scope.ESICPolicyDetails.FormulaDescription != null) {
            var str = $scope.ESICPolicyDetails.FormulaDescription;
            $scope.FormulaArray = str.split(" ");

            var strId = $scope.ESICPolicyDetails.FormulaIDDescription;
            $scope.FormulaIdArray = strId.split(" ");
        }
        if ($scope.ESICPolicyDetails.FormulaDesEmployer != null) {
            var str = $scope.ESICPolicyDetails.FormulaDesEmployer;
            $scope.EmployerFormulaArray = str.split(" ");

            var strId = $scope.ESICPolicyDetails.FormulaDesIDEmployer;
            $scope.EmployerFormulaIdArray = strId.split(" ");
        }
        if ($scope.ESICPolicyDetails.IsFixedEmp == true) {
            $scope.radioFixedValue = true;
        }
        else {
            $scope.radioFormulaValue = true;
        }
        if ($scope.ESICPolicyDetails.IsFixedEmployer == true) {
            $scope.radioFixedValueE = true;
        }
        else {
            $scope.radioFormulaValueE = true;
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
        $scope.ESICPolicyMaster = {
            ID: null,
            ESICPolicyName: null,
            ESICPolicyDescription: null,
            GroupID: $scope.ESICPolicyMaster.GroupID,
            PlantID: $scope.ESICPolicyMaster.PlantID,
            CompanyID: $scope.ESICPolicyMaster.CompanyID
        };
        $scope.ESICPolicyMonthNo = {
            ESICPolicyMasterID: null,
            MonthNo: null,
            MonthName: null,
        }
        $scope.ESICPolicyDetails = {
            ID: null,
            ESICPolicyMasterID: null,
            FormulaDesEarning: null,
            FormulaDesIDEarning: null,
            SalaryHeadIDEarning: null,
            EarningValueRangeFrom: null,
            EarningValueRangeTo: null,
            IsMandatory: false,
            IsFixedEmp: false,
            FixedValueEmp: 0,
            IsFormulaEmp: false,
            IsContributionSlrHDdependOnEarningEmp: false,
            FormulaDes: null,
            FormulaDesID: null,
            FormulaDescription: null,
            FormulaIDDescription: null,
            SalaryHeadIdFormula: null,
            SalaryHeadID: null,
            IsFixedEmployer: false,
            FixedValueEmployer: null,
            IsFormulaEmployer: false,
            IsContributionSlrHDdependOnEarningEmployer: false,
            FormulaDesEmployer: null,
            FormulaDesIDEmployer: null,
            SalaryHeadIDEmployer: null
        };
        $scope.getLeave();
        $scope.DetailsList = [];
        $scope.MonthList = [];
        //$scope.ModelList.response.data[0].MonthName = [];
        $scope.EFormulaArray = [];
        $scope.EFormulaIdArray = [];
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.EmployerFormulaArray = [];
        $scope.EmployerFormulaIdArray = [];
        $scope.ESICPolicyHead = {
            Id: null,
            ESICPolicyMasterID: $scope.ESICPolicyMaster.ID,
            SalaryHeadID: null,
            SalaryHeadName: null,
        }
        $scope.HeadList = [];
    };




    //Clear Details
    $scope.ClearD = function () {
       
        $scope.Action = 'Save';
        $scope.ESICPolicyDetails = {
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
        $scope.EmployerFormulaArray = [];
        $scope.EmployerFormulaIdArray = [];
        
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
            url: $scope.path + 'DeleteMaster?ID=' + $scope.ESICPolicyMaster.ID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete ESIC Policy Details first!");
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
        $scope.ESICPolicyMonthNo = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.ESICPolicyMonthNo.ESICPolicyMasterID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpMonth')).modal('show');
    }
    $scope.DeleteMonth = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMonth?ID=' + $scope.ESICPolicyMonthNo.ESICPolicyMasterID + '&&monthno=' + $scope.ESICPolicyMonthNo.MonthNo,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Invalid Month ");
            }
            else {

                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.MonthList.length; i++) {
                    if ($scope.MonthList[i].MonthNo == $scope.ESICPolicyMonthNo.MonthNo) {
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
        $scope.ESICPolicyDetails = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.ESICPolicyDetails.ID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpDetails')).modal('show');
    }
    $scope.DeleteDetails = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteDetails?ID=' + $scope.ESICPolicyDetails.ID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Error occured!");
            }
            else {

                ShowResult(response.data.Message, 'success');
                $scope.getSavedDetails($scope.ESICPolicyMaster);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    //#region Update 


    $scope.ESICPolicyHead = {
        Id: null,
        ESICPolicyMasterID: $scope.ESICPolicyMaster.ID,
        SalaryHeadID: null,
        SalaryHeadName: null,
    }
    $scope.SubmitHeads = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ESICPolicyHead.SalaryHeadID)) {
                throw "Select Salary Head";
            }
            for (var i = 0; i < $scope.HeadList.length; i++) {
                if ($scope.HeadList[i].SalaryHeadID == $scope.ESICPolicyHead.SalaryHeadID) {
                    throw "This Salary head already Exist";
                }
            }
            for (var i = 0; i < $scope.salaryHeadList.length; i++) {
                if ($scope.salaryHeadList[i].Value == $scope.ESICPolicyHead.SalaryHeadID) {
                    $scope.ESICPolicyHead.SalaryHeadName = $scope.salaryHeadList[i].Text;
                    break;
                }
            }
            var newObj = Object.assign({}, $scope.ESICPolicyHead);
            $scope.HeadList.push(newObj);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.message_confirmation = null;
    $scope.RemoveHead = function (obj) {
        $scope.ESICPolicyHead = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.ESICPolicyHead.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpHead')).modal('show');
    }
    $scope.DeleteHeadList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteHeadMaster?ID=' + $scope.ESICPolicyHead.Id,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Invalid Head ");
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getHeads($scope.ESICPolicyMaster.ID);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    //#endregion

}