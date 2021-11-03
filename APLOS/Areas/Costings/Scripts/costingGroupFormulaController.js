'use strict';
costingGroupFormulaController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function costingGroupFormulaController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Costing Group Formula";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingGroupFormulaList = [];
    $scope.path = 'Costings/CostingGroupFormula/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
   
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    function InitiallizeCostingGroupFormula() {
        $scope.costingGroupFormula = {
            Id: null, CostingGroupId: null, CostingType: null, FormulaId: null, Formula: null
        }
    }
    InitiallizeCostingGroupFormula();
    
    $scope.CostingTypeList = [];
    cboService.getEnumCbo("enum/GetCostingTypeEnumCbo", function (result) {
        $scope.CostingTypeList = result;
    });


    function getCostingGroup() {
        $http({
            method: "GET",
            url: "Costings/costingItem/GetCostingGroups",
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.CostingGroupList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    getCostingGroup();

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.costingGroupFormula.FormulaDes = null;
    $scope.costingGroupFormula.FormulaDesID = null;
    $scope.costingGroupFormula.SalaryHeadFormula = null;
    $scope.costingGroupFormula.FormulaDescription = null;
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

                if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.CostingGroupFormulaId)) {

                    $scope.costingGroupFormula.FormulaDescription = null;
                    $scope.costingGroupFormula.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.costingGroupFormula.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.SalaryHeadFormula;
                            $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.CostingGroupFormulaId;
                            $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                            $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);
                        }
                        else {
                            $scope.costingGroupFormula.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.SalaryHeadFormula;
                            $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.CostingGroupFormulaId;
                            $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                            $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);
                        }
                    }
                    else {
                        $scope.costingGroupFormula.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                        $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.SalaryHeadFormula;
                        $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.CostingGroupFormulaId;
                        $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                        $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);
                    }
                }

                $scope.costingGroupFormula.FormulaDescription = null;
                $scope.costingGroupFormula.FormulaIDDescription = null;

                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDescription)) {
                        $scope.costingGroupFormula.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.costingGroupFormula.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

                for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaIDDescription)) {
                        $scope.costingGroupFormula.FormulaIDDescription = $scope.FormulaIdArray[i];
                    }
                    else {
                        $scope.costingGroupFormula.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                    }
                }

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.Operator)) {

                    $scope.costingGroupFormula.FormulaDescription = null;
                    $scope.costingGroupFormula.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.Operator;
                        $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.Operator;
                        $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                        $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);
                    }

                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDescription)) {
                            $scope.costingGroupFormula.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.costingGroupFormula.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaIDDescription)) {
                            $scope.costingGroupFormula.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.costingGroupFormula.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.Precedence)) {

                    $scope.costingGroupFormula.FormulaDescription = null;
                    $scope.costingGroupFormula.FormulaIDDescription = null;

                    $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.Precedence;
                    $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDes)) {
                        $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                        $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);

                        for (var i = 0; i < $scope.FormulaArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDescription)) {
                                $scope.costingGroupFormula.FormulaDescription = $scope.FormulaArray[i];
                            }
                            else {
                                $scope.costingGroupFormula.FormulaDescription += ' ' + $scope.FormulaArray[i];
                            }
                        }

                        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaIDDescription)) {
                                $scope.costingGroupFormula.FormulaIDDescription = $scope.FormulaIdArray[i];
                            }
                            else {
                                $scope.costingGroupFormula.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                            }
                        }

                    }
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.Value)) {

                    $scope.costingGroupFormula.FormulaDescription = null;
                    $scope.costingGroupFormula.FormulaIDDescription = null;

                    $scope.costingGroupFormula.FormulaDes = $scope.costingGroupFormula.Value;
                    $scope.costingGroupFormula.FormulaDesID = $scope.costingGroupFormula.Value;


                    if (!baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDes)) {
                        $scope.FormulaArray.push($scope.costingGroupFormula.FormulaDes);
                        $scope.FormulaIdArray.push($scope.costingGroupFormula.FormulaDesID);
                    }


                    for (var i = 0; i < $scope.FormulaArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDescription)) {
                            $scope.costingGroupFormula.FormulaDescription = $scope.FormulaArray[i];
                        }
                        else {
                            $scope.costingGroupFormula.FormulaDescription += ' ' + $scope.FormulaArray[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaIDDescription)) {
                            $scope.costingGroupFormula.FormulaIDDescription = $scope.FormulaIdArray[i];
                        }
                        else {
                            $scope.costingGroupFormula.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                        }
                    }

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {
        $scope.costingGroupFormula.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.costingGroupFormula.FormulaDescription = null;
        $scope.costingGroupFormula.FormulaIDDescription = null;
        $scope.costingGroupFormula.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaDescription)) {
                $scope.costingGroupFormula.FormulaDes = $scope.FormulaArray[i];
                $scope.costingGroupFormula.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.costingGroupFormula.FormulaDes += $scope.FormulaArray[i];
                $scope.costingGroupFormula.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.costingGroupFormula.FormulaIDDescription)) {
                $scope.costingGroupFormula.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.costingGroupFormula.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.costingGroupFormula.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.costingGroupFormula.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    }

    $scope.costingGroupFormulaList = [];
    $scope.getData = function () {
        $scope.costingGroupFormulaList = [];
        $http({
            method: 'GET',
            url: $scope.getListUrl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.costingGroupFormulaList = response.data;
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.costingGroupFormula = Object.assign({}, args.data);
        $scope.costingGroupFormula.FormulaDescription = $scope.costingGroupFormula.Formula;
        var str = $scope.costingGroupFormula.Formula;
        $scope.FormulaArray = str.split(" ");

        var strId = $scope.costingGroupFormula.FormulaId;
        $scope.FormulaIdArray = strId.split(" ");
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.costingGroupFormula.FormulaId = $scope.costingGroupFormula.FormulaIDDescription;
        $scope.costingGroupFormula.Formula = $scope.costingGroupFormula.FormulaDescription;
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'costingGroupFormula': $scope.costingGroupFormula },
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
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.Update = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'costingGroupFormula': $scope.costingGroupFormula },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };
    $scope.Delete = function () {
       
            $http({
                method: 'GET',
                url: 'Costings/CostingGroupFormula/Delete?id=' + $scope.costingGroupFormula.Id,
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
        
    };

    $scope.Clear = function () {

        $scope.Action = "Save";
       InitiallizeCostingGroupFormula();
        $scope.FormulaIdArray = [];
        $scope.FormulaArray = [];
        $scope.costingCategory = {};
       // $scope.costingCategoryNew = { Sequence: seq, Active: true };
        $scope.costingGroupFormula.CostingGroupFormulaId = null;
        
    };

    function ClearFields(seq) {

        $scope.Action = "Save";
        InitiallizeCostingGroupFormula();
        $scope.costingGroupFormulaList = [];
        $scope.costingCategory = {};
        $scope.costingCategoryNew = { Sequence: seq, Active: true };
    }
}