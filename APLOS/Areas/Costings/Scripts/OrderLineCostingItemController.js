'use strict';
OrderLineCostingItemController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function OrderLineCostingItemController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "OrderLineCostingItem";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Costings/OrderLineCostingItem/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.Model = {
        Id: null,
        Sequence: null,
        UserName: null,
        LineItemCostingSandardName: null,
        CostingSegment: null,
        SOItemName: null,
        Active: true,
        ValueinDecimal: false,
        ValueinPercentage: true,
        DefaultValue: null,
        Formula: null,
        FormulaId:null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        Operator: null,
        Precedence: null,
        Value: null,
        EntryState: 'Entry',
        FormulaDes: null,
        FormulaDesID: null,
        SalaryHeadFormula: null,
        FormulaDescription:null

    }
    $scope.ModelNew = Object.assign({}, $scope.Model);

    $scope.setCheckedValue = function (name) {
        if (name === 'ValueinPercentage') {
            $scope.ModelNew.ValueinPercentage = true;
            $scope.ModelNew.ValueinDecimal = false;
        }

        if (name === 'ValueinDecimal') {
            $scope.ModelNew.ValueinDecimal = true;
            $scope.ModelNew.ValueinPercentage = false;
        }

    }

    $scope.CostingSOList = [];
    cboService.getEnumCbo("enum/GetCostingSOEnumCbo", function (result) {
        $scope.CostingSOList = result;
    });

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.ModelNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.CostingTypeList = [];
    cboService.getCostingTypesCbo(function (response) {
        $scope.CostingTypeList = response;
    });

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.OrderLineCostingItemList = [];
    $scope.GetOrderLineCostingItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/OrderLineCostingItem/GetOrderLineCostingItemCbo?Id=' + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.OrderLineCostingItemList = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.GetOrderLineCostingItemCbo();

    $scope.CostingComponentList = [];
    $scope.GetCostingTypeComponent = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/OrderLineCostingItem/GetCostingComponentByCostingType?costingType=' + $scope.ModelNew.CostingType,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.CostingComponentList = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ModelList = [];
    $scope.GetData = function () {
        $scope.ModelList = [];
        $http.get("Costings/OrderLineCostingItem/GetList")
            .then(
                function successCallback(response) {
                    $scope.ModelList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    $scope.GetData();

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.FormulaDetails = [];

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < $scope.NoticePeriodList.length; i++) {
                if (ob.SalaryRuleGeneralSystemID !== $scope.NoticePeriodList[i].SalaryRuleGeneralSystemID && ob.OrderLineHeadId === $scope.NoticePeriodList[i].OrderLineHeadId) {
                    throw "Salary Head has already been taken...";
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Clear = function () {
        $scope.Model = {
            Id: null,
            Sequence: null,
            UserName: null,
            LineItemCostingSandardName: null,
            CostingSegment: null,
            SOItemName: null,
            Active: true,
            ValueinDecimal: false,
            ValueinPercentage: true,
            DefaultValue: null,
            Formula: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null,
            Operator: null,
            Precedence: null,
            Value: null,
            EntryState: 'Entry'
        }
        $scope.ModelNew = Object.assign({}, $scope.Model);
        $scope.Action = 'Save';
        $scope.ModelNew.FormulaDescription = null;
        $scope.ModelNew.FormulaIDDescription = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.GetSequence();
        $scope.GetOrderLineCostingItemCbo();
        $scope.ModelNewEntryState = 'Entry';
    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue) || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: 'Costings/OrderLineCostingItem/Delete?id=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetData();
                    $scope.GetOrderLineCostingItemCbo();
                    $scope.Clear();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ModelNew.FormulaDes = null;
    $scope.ModelNew.FormulaDesID = null;
    $scope.ModelNew.SalaryHeadFormula = null;
    $scope.ModelNew.FormulaDescription = null;
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

    $scope.FormulaDetails = [];
    $scope.SetFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.OrderLineCostingItemId = $scope.ModelNew.Id == null ? null : $scope.ModelNew.Id;
                formulaObj.OrderLineHeadId = $scope.ModelNew.HeadIdFormula;
                formulaObj.SalaryHead = $("#HeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.ModelNew.FormulaDes = '';
                $scope.ModelNew.FormulaDesID = '';

                $scope.ModelNew.FormulaDescription = '';
                $scope.ModelNew.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ModelNew.FormulaDes)) {
                        $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);
                    } else {
                        $scope.ModelNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId;
                    }
                }

                $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
                $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelNew.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.OrderLineCostingItemId = $scope.ModelNew.Id == null ? null : $scope.ModelNew.Id;
                        formulaObj.OrderLineHeadId = null;
                        formulaObj.Component = $scope.ModelNew.Operator;
                        formulaObj.SalaryHead = $scope.ModelNew.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelNew.FormulaDes = '';
                        $scope.ModelNew.FormulaDesID = '';

                        $scope.ModelNew.FormulaDescription = '';
                        $scope.ModelNew.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                        }

                        $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
                        $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }



            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelNew.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OrderLineCostingItemId = $scope.ModelNew.Id == null ? null : $scope.ModelNew.Id;
                    formulaObj.OrderLineHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelNew.Precedence;
                    formulaObj.Component = $scope.ModelNew.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelNew.FormulaDes = '';
                    $scope.ModelNew.FormulaDesID = '';

                    $scope.ModelNew.FormulaDescription = '';
                    $scope.ModelNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                    }

                    $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
                    $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelNew.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OrderLineCostingItemId = $scope.ModelNew.Id == null ? null : $scope.ModelNew.Id;
                    formulaObj.OrderLineHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelNew.Value;
                    formulaObj.Component = $scope.ModelNew.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelNew.FormulaDes = '';
                    $scope.ModelNew.FormulaDesID = '';

                    $scope.ModelNew.FormulaDescription = '';
                    $scope.ModelNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                    }

                    $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
                    $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {

        var maxseq = Math.max.apply(Math, $scope.FormulaDetails.map(function (o) { return o.Sequence; }))

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (maxseq === $scope.FormulaDetails[i].Sequence) {
                $scope.FormulaDetails.splice(i, 1);
                break;
            }
        }

        $scope.ModelNew.FormulaDes = '';
        $scope.ModelNew.FormulaDesID = '';

        $scope.ModelNew.FormulaDescription = '';
        $scope.ModelNew.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ModelNew.FormulaDes)) {
                $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].OrderLineHeadId;
            } else {
                $scope.ModelNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelNew.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId;
            }
        }

        $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
        $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;

    }

    $scope.Get = function (obj) {
        $scope.FormulaDetails = [];
        $scope.ModelNew.HeadIdFormula = null;
        $scope.ModelNew.Operator = null;
        $scope.ModelNew.Precedence = null;
        $scope.ModelNew.Value = null;

        $scope.objectData = obj.data;
        $scope.ModelNew = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "Costings/OrderLineCostingItem/GetDetailList?OrderLineCostingItemId=" + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.ModelNew.FormulaDes = '';
                $scope.ModelNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.ModelNew.FormulaDes)) {
                        $scope.ModelNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);
                    } else {
                        $scope.ModelNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId;
                    }
                }

                $scope.ModelNew.FormulaDescription = $scope.ModelNew.FormulaDes;
                $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;


            }
        });


        var value = null;

        $scope.GetOrderLineCostingItemCbo();
        $scope.GetCostingTypeComponent();

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.AddEditRow = function () {
        try {


            $scope.ModelNew.FormulaDes = $scope.ModelNew.FormulaDescription;
            $scope.ModelNew.FormulaDesID = $scope.ModelNew.FormulaIDDescription;

            $scope.ModelNew.Formula = $scope.ModelNew.FormulaDescription;
            $scope.ModelNew.FormulaId = $scope.ModelNew.FormulaIDDescription;

            $scope.ModelNew.SalaryHead = $("#SH option:selected").text();

            $scope.Row = 'Add Row';
            $scope.ModelNew.FormulaDescription = null;
            $scope.ModelNew.FormulaIDDescription = null;

            $scope.ModelNew.HeadIdFormula = null;
            $scope.ModelNew.Operator = null;
            $scope.ModelNew.Precedence = null;
            $scope.ModelNew.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue) || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            $scope.AddEditRow();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetData();
                    $scope.Clear();
                    $scope.FormulaDetails = [];
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };



}
