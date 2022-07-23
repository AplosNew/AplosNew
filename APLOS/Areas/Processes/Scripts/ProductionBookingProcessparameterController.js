'use strict';
ProductionBookingProcessparameterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionBookingProcessparameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "ProductionBookingProcessparameter";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Processes/ProductionBookingProcessparameter/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveProcessParameterUrl = $scope.path + 'CreateProcessParameter';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.Model = {Id: null, ProcessId: null, UoMId: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null}
    $scope.ModelNew = Object.assign({}, $scope.Model);

    $scope.ModelProcessPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible:false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null}
    $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);

    $scope.ModelQualityPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
    $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelQualityPara);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.setCheckedValue = function (name) {
        if (name === 'ValueinPercentage') {
            $scope.ModelProcessPara.ValueinPercentage = true;
            $scope.ModelProcessPara.ValueinDecimal = false;
        }
        if (name === 'ValueinDecimal') {
            $scope.ModelProcessPara.ValueinDecimal = true;
            $scope.ModelProcessPara.ValueinPercentage = false;
        }
    }

    $scope.processList = [];
    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processList = response.data;
    });

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelProcessPara.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.OrderLineCostingItemList = [];
    $scope.GetOrderLineCostingItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Processes/ProductionBookingProcessparameter/GetHeaderItemCbo?Id=' + $scope.ModelProcessPara.Id,
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

    $scope.ModelList = [];
    $scope.GetData = function () {
        $scope.ModelList = [];
        $http.get("Processes/ProductionBookingProcessparameter/GetList")
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
                url: 'Costings/OrderLineCostingItem/Delete?id=' + $scope.ModelProcessPara.Id
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

    $scope.ModelProcessPara.FormulaDes = null;
    $scope.ModelProcessPara.FormulaDesID = null;
    $scope.ModelProcessPara.SalaryHeadFormula = null;
    $scope.ModelProcessPara.FormulaDescription = null;
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
                formulaObj.OrderLineCostingItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                formulaObj.OrderLineHeadId = $scope.ModelProcessPara.HeadIdFormula;
                formulaObj.SalaryHead = $("#HeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.ModelProcessPara.FormulaDes = '';
                $scope.ModelProcessPara.FormulaDesID = '';

                $scope.ModelProcessPara.FormulaDescription = '';
                $scope.ModelProcessPara.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);
                    } else {
                        $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId;
                    }
                }

                $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.OrderLineCostingItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                        formulaObj.OrderLineHeadId = null;
                        formulaObj.Component = $scope.ModelProcessPara.Operator;
                        formulaObj.SalaryHead = $scope.ModelProcessPara.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelProcessPara.FormulaDes = '';
                        $scope.ModelProcessPara.FormulaDesID = '';

                        $scope.ModelProcessPara.FormulaDescription = '';
                        $scope.ModelProcessPara.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                        }

                        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OrderLineCostingItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.OrderLineHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Precedence;
                    formulaObj.Component = $scope.ModelProcessPara.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OrderLineCostingItemId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.OrderLineHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Value;
                    formulaObj.Component = $scope.ModelProcessPara.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

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

        $scope.ModelProcessPara.FormulaDes = '';
        $scope.ModelProcessPara.FormulaDesID = '';

        $scope.ModelProcessPara.FormulaDescription = '';
        $scope.ModelProcessPara.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID += ' ' + $scope.FormulaDetails[i].OrderLineHeadId;
            } else {
                $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId;
            }
        }

        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

    }

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.masterId = $scope.ModelNew.Id;
        $scope.GetProcessParameterData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.GetProcessPara = function (obj) {
        $scope.ProductionAction = 'Update';
        
        $scope.FormulaDetails = [];
        $scope.ModelProcessPara.HeadIdFormula = null;
        $scope.ModelProcessPara.Operator = null;
        $scope.ModelProcessPara.Precedence = null;
        $scope.ModelProcessPara.Value = null;

        $scope.objectData = obj.data;
        $scope.ModelProcessPara = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "Processes/ProductionBookingProcessparameter/GetDetailList?OrderLineCostingItemId=" + $scope.ModelProcessPara.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.ModelProcessPara.FormulaDes = '';
                $scope.ModelProcessPara.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.FormulaDes)) {
                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId);
                    } else {
                        $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].OrderLineHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].OrderLineHeadId;
                    }
                }

                $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;


            }
        });


        var value = null;

        $scope.GetOrderLineCostingItemCbo();

    };


    $scope.AddEditRow = function () {
        try {


            $scope.ModelProcessPara.FormulaDes = $scope.ModelProcessPara.FormulaDescription;
            $scope.ModelProcessPara.FormulaDesID = $scope.ModelProcessPara.FormulaIDDescription;

            $scope.ModelProcessPara.Formula = $scope.ModelProcessPara.FormulaDescription;
            $scope.ModelProcessPara.FormulaId = $scope.ModelProcessPara.FormulaIDDescription;

            $scope.ModelProcessPara.SalaryHead = $("#SH option:selected").text();

            $scope.Row = 'Add Row';
            $scope.ModelProcessPara.FormulaDescription = null;
            $scope.ModelProcessPara.FormulaIDDescription = null;

            $scope.ModelProcessPara.HeadIdFormula = null;
            $scope.ModelProcessPara.Operator = null;
            $scope.ModelProcessPara.Precedence = null;
            $scope.ModelProcessPara.Value = null;

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

    $scope.masterId = null;
    $scope.Save = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Id;
                    $scope.masterId = response.data.Id;
                    $scope.GetData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ProcessParameterList = [];
    $scope.GetProcessParameterData = function () {
        $scope.ProcessParameterList = [];
        $http.get("Processes/ProductionBookingProcessParameter/GetProcessParameterList?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.ProcessParameterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SaveProcessParameter = function () {
        try {
            $scope.AddEditRow();
            $scope.ModelProcessPara.ProductionBookingProcessParameterId = $scope.masterId;
            $http({
                method: 'POST',
                url: $scope.saveProcessParameterUrl,
                data: { 'data': $scope.ModelProcessPara, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetProcessParameterData();
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
    $scope.ProductionAction = 'Save';
    $scope.Clear = function () {
        $scope.ModelProcessPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);
        $scope.ProductionAction = 'Save';
        $scope.ModelProcessPara.FormulaDescription = null;
        $scope.ModelProcessPara.FormulaIDDescription = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.GetSequence();
        $scope.GetOrderLineCostingItemCbo();
        $scope.ModelProcessParaEntryState = 'Entry';
    }


}
