'use strict';
OTPolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTPolicyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Policy';
    $scope.Action = 'Save';
    $scope.Action1 = 'Submit';
    $scope.ModelList = [];
    $scope.path = 'Attendances/OTPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.submitUrl = $scope.path + 'Submit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.ModelTemp = {
        ID: null,
        OverTimePolicyName: null,
        OverTimePolicyDescription: null,
        GroupID: null,
        PlantID: null,
        IsDefault: true,
        Plant: null,
        //CompanyId: null,
        //Company: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    $scope.PolicyDetailTemp = {
        ID: null,
        OverTimePmtPolicyID: null,
        OverTimeDayType: null,
        IsFixed: false,
        FixedValue: null,
        IsFormula: false,
        IsDependOnEarning: false,
        FormulaDescription: null,
        FormulaIDDescription: null,
        SalaryHeadIdFormula: null
    };
    $scope.PolicyDetail = Object.assign({}, $scope.PolicyDetailTemp);

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
    $scope.getData = function () {
        $scope.ModelList = [];
        $scope.DetailList = [];
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { PlantID: $scope.ModelNew.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getDetail = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        var masterId = $scope.ModelNew.ID;
        $http({
            method: 'POST',
            url: $scope.path + "GetDetail",
            data: { masterId: masterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailList = response.data;
        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });
    $scope.Save = function () {
        try {
            if ($scope.ModelNew.PlantID == null || $scope.ModelNew.PlantID == '' || $scope.ModelNew.PlantID == 'undifined') {
                throw "Select Plant First";
            }
            if ($scope.DetailList == null || $scope.DetailList == '' || $scope.DetailList == 'undifined') {
                throw "Policy Details is Blank"
            }
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
                    $scope.getData();
                    ClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.DetailList = [];
    $scope.PolicyDetail = [];
    $scope.Submit = function () {
        try {
            if ($scope.PolicyDetail.OverTimeDayType === null || $scope.PolicyDetail.OverTimeDayType === 'undefined' || $scope.PolicyDetail.OverTimeDayType === '') {
                throw "Over Time Day Type cannot be null";
            }
            else {
                for (var i = 0; i < $scope.DetailList.length; i++) {
                    var p = $scope.DetailList[i].OverTimeDayType;
                    if (p === $scope.PolicyDetail.OverTimeDayType) {
                        throw " Over Time Day Type can not be same!";
                    }
                }//for
            }
            if ($scope.PolicyDetail.IsFixed === true) {
                if ($scope.PolicyDetail.FixedValue === null || $scope.PolicyDetail.FixedValue === '' || $scope.PolicyDetail.FixedValue === 'undefined') {
                    throw "Fixed Value Cannot be Blank...";
                }
            }
            else {
                if ($scope.PolicyDetail.FormulaDescription === null || $scope.PolicyDetail.FormulaDescription === '' || $scope.PolicyDetail.FormulaDescription === 'undefined') {
                    throw "Formula Cannot be Blank...";
                }
            }
            if ($scope.PolicyDetail.IsFixed === false) {
                $scope.PolicyDetail.FixedValue = null;
            }
            if ($scope.PolicyDetail.IsFormula === false) {
                $scope.PolicyDetail.FormulaDescription = null;
            }
            if ($scope.PolicyDetail.OverTimeDayType == null || $scope.PolicyDetail.OverTimeDayType == '' || $scope.PolicyDetail.OverTimeDayType == 'undifined') {
                throw "OverTimeDayType is Blank"
            }
            var newObj = Object.assign({}, $scope.PolicyDetail);
            $scope.DetailList.push(newObj);
            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

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
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    $scope.PolicyDetail.FormulaDes = null;
    $scope.PolicyDetail.FormulaDesID = null;
    $scope.PolicyDetail.SalaryHeadFormula = null;
    $scope.PolicyDetail.FormulaDescription = null;
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];
    $scope.SetFormula = function (formula) {
        try {
            if (formula === 'SHead') {
                if (!baseService.isUndefinedOrNull($scope.PolicyDetail.SalaryHeadIdFormula)) {
                    $scope.PolicyDetail.FormulaDescription = null;
                    $scope.PolicyDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.PolicyDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.PolicyDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.PolicyDetail.FormulaDes = $scope.Formula;
                            $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                            $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                        }
                        else {
                            $scope.PolicyDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                            var str = $scope.PolicyDetail.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');
                            $scope.PolicyDetail.FormulaDes = $scope.Formula;
                            $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                            $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                        }
                    }
                    else {
                        $scope.PolicyDetail.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();
                        var str = $scope.PolicyDetail.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');
                        $scope.PolicyDetail.FormulaDes = $scope.Formula;
                        $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                    }
                }
                $scope.PolicyDetail.FormulaDescription = null;
                $scope.PolicyDetail.FormulaIDDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDescription)) {
                        $scope.PolicyDetail.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.PolicyDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }
                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaIDDescription)) {
                        $scope.PolicyDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.PolicyDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }
            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.PolicyDetail.Operator)) {
                    $scope.PolicyDetail.FormulaDescription = null;
                    $scope.PolicyDetail.FormulaIDDescription = null;
                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];
                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.PolicyDetail.FormulaDes = $scope.PolicyDetail.Operator;
                        $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.Operator;
                        $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDescription)) {
                            $scope.PolicyDetail.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.PolicyDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaIDDescription)) {
                            $scope.PolicyDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.PolicyDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                } else {
                    throw "First select Salary Head.";
                }
            }
            else if (formula === 'Precedence') {
                if (!baseService.isUndefinedOrNull($scope.PolicyDetail.Precedence)) {
                    $scope.PolicyDetail.FormulaDescription = null;
                    $scope.PolicyDetail.FormulaIDDescription = null;
                    $scope.PolicyDetail.FormulaDes = $scope.PolicyDetail.Precedence;
                    $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.Precedence;
                    if (!baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDes)) {
                        $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDescription)) {
                                $scope.PolicyDetail.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.PolicyDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }
                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaIDDescription)) {
                                $scope.PolicyDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.PolicyDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }
                    }
                }
            }
            else if (formula === 'Value') {
                if (!baseService.isUndefinedOrNull($scope.PolicyDetail.Value)) {
                    $scope.PolicyDetail.FormulaDescription = null;
                    $scope.PolicyDetail.FormulaIDDescription = null;
                    $scope.PolicyDetail.FormulaDes = $scope.PolicyDetail.Value;
                    $scope.PolicyDetail.FormulaDesID = $scope.PolicyDetail.Value;
                    if (!baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDes)) {
                        $scope.FormulaArray.push($scope.PolicyDetail.FormulaDes);
                        $scope.FormulaIdArray.push($scope.PolicyDetail.FormulaDesID);
                    }
                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDescription)) {
                            $scope.PolicyDetail.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.PolicyDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }
                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaIDDescription)) {
                            $scope.PolicyDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.PolicyDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.ClearFormulaSource = function () {
        $scope.PolicyDetail.Operator = '';
        $scope.PolicyDetail.Precedence = '';
        $scope.PolicyDetail.Value = '';
    }
    $scope.ClearDescription = function () {
        $scope.PolicyDetail.FormulaDescription = '';
        $scope.PolicyDetail.FixedValue = '';
        $scope.PolicyDetail.IsDependOnEarning = '';
    }
    $scope.RemoveFormula = function () {
        $scope.PolicyDetail.FormulaDesID = null;
        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);
        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);
        $scope.PolicyDetail.FormulaDescription = null;
        $scope.PolicyDetail.FormulaIDDescription = null;
        $scope.PolicyDetail.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaDescription)) {
                $scope.PolicyDetail.FormulaDes = $scope.FormulaArray[i];
                $scope.PolicyDetail.FormulaDescription = $scope.FormulaArray[i];
            } else {
                $scope.PolicyDetail.FormulaDes += $scope.FormulaArray[i];
                $scope.PolicyDetail.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }
        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.PolicyDetail.FormulaIDDescription)) {
                $scope.PolicyDetail.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.PolicyDetail.FormulaIDDescription = $scope.FormulaIdArray[i];
            } else {
                $scope.PolicyDetail.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.PolicyDetail.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = {
            ID: null,
            OverTimePolicyName: null,
            OverTimePolicyDescription: null,
            IsDefault: true,
            CompanyId: $scope.ModelNew.CompanyId,
            PlantID: $scope.ModelNew.PlantID,
        };
        $scope.PolicyDetail = Object.assign({}, $scope.PolicyDetailTemp);

        $scope.DetailList = [];
    }
    $scope.radiovalue = false;
    $scope.radioFixedValue = false;
    $scope.radioFormulaValue = false;
    $scope.setRadioFixedValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = true;
        $scope.radioFormulaValue = false;
        $scope.PolicyDetail.IsFixed = true;
        $scope.PolicyDetail.IsFormula = false;
    }
    $scope.setRadioFormulaValue = function () {
        $scope.radiovalue = true;
        $scope.radioFixedValue = false;
        $scope.radioFormulaValue = true;
        $scope.PolicyDetail.IsFormula = true;
        $scope.PolicyDetail.IsFixed = false;
    }
    $scope.message_confirmation = null;
    $scope.RemoveDetail = function (obj) {
        $scope.PolicyDetail = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.PolicyDetail.ID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }
    $scope.DeleteChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Delete?Id=' + $scope.PolicyDetail.ID,
        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.DetailList.length; i++) {
                    if ($scope.DetailList[i].ID == $scope.PolicyDetail.ID) {
                        $scope.DetailList.splice(i, 1);
                    }
                }
                $scope.PolicyDetail = {
                    FixedValue: null,
                    IsFormula: false,
                    IsDependOnEarning: false,
                    FormulaDescription: null,
                    FormulaIDDescription: null,
                    SalaryHeadIdFormula: null
                }
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    $scope.RemoveMaster = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.ModelNew.ID))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteMaster?Id=' + $scope.ModelNew.ID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Over Time Poicy Detail first!");
            }
            else {

                ShowResult(response.data.Message, 'success');
                for (var i = 0; i < $scope.ModelList.length; i++) {
                    if ($scope.ModelList[i].ID == $scope.ModelNew.ID) {
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
