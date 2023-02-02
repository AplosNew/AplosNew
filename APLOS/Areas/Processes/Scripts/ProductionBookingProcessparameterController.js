'use strict';
ProductionBookingProcessparameterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function ProductionBookingProcessparameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "ProductionBookingProcessparameter";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Processes/ProductionBookingProcessparameter/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveProcessParameterUrl = $scope.path + 'CreateProcessParameter';
    $scope.saveQualityProcessParameterUrl = $scope.path + 'CreateQualityProcessParameter';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getualityProcessParameterSeqUrl = $scope.path + 'GetQualityProcessParameterAutoSequence';

    $scope.Model = { Id: null, ProcessId: null, InputItemName: null, InputItemUoMId: null, OutputItemName: null, OutputItemUoMId: null, InPutOutPutRatio: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.ModelNew = Object.assign({}, $scope.Model);

    $scope.ModelProcessPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, IsPreviousValueApplicable:true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
    $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);

    $scope.ModelQuality = { Id: null, ProcessId: null, ProductionBookingProcessParameterId: null, ItemName: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    $scope.ModelQualityNew = Object.assign({}, $scope.ModelQuality);

    $scope.ModelQualityPara = { Id: null, QualityProcessId: null, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
    $scope.ModelQualityParaNew = Object.assign({}, $scope.ModelQualityPara);

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

    $scope.setCheckedEntry = function (name) {
        if (name === 'Entry') {
            $scope.ModelProcessPara.EntryState = 'Entry';
            $scope.ModelProcessPara.Formula = null;
            $scope.ModelProcessPara.FormulaId = null;
            $scope.ModelProcessPara.FormulaDes = null;
            $scope.ModelProcessPara.FormulaDesID = null;
            $scope.ModelProcessPara.SalaryHeadFormula = null;
            $scope.ModelProcessPara.FormulaDescription = null;
            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        }
    }

    $scope.qprocessList = [];
    $http({
        method: 'GET',
        url: 'QMS/QualityProcess/getcbo'
    }).then(function successCallback(response) {
        $scope.qprocessList = response.data;
    });


    $scope.processList = [];
    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processList = response.data;
    });

    $scope.uOMinList = [];
    $scope.uOMoutList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMinList = response;
    });
    cboService.getUoMCbo(function (response) {
        $scope.uOMoutList = response;
    });

    $scope.GetSequence = function () {
        $http.get("Processes/ProductionBookingProcessparameter/getautosequence?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.ModelProcessPara.Sequence = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });


    };
    $scope.GetSequence();

    $scope.OrderLineCostingItemList = [];
    $scope.GetOrderLineCostingItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Processes/ProductionBookingProcessparameter/GetHeaderItemCbo?Id=' + $scope.ModelProcessPara.Id + '&masterId=' + $scope.masterId,
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

    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: 'Processes/ProductionBookingProcessparameter/Delete?id=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetData();
                    $scope.MainClear();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.MainClear = function () {
        $scope.Model = { Id: null, ProcessId: null, InputItemName: null, InputItemUoMId: null, OutputItemName: null, OutputItemUoMId: null, InPutOutPutRatio: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.ModelNew = Object.assign({}, $scope.Model);
        $scope.Action = 'Save';
        $scope.masterId = null;
        $scope.ProcessParameterList = [];
        $scope.QualityParameterList = [];
    }

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
                formulaObj.ProductionBookingParameterId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                formulaObj.ProductionBookingParameterHeadId = $scope.ModelProcessPara.HeadIdFormula;
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
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);
                    } else {
                        $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].ProductionBookingParameterHeadId;
                    }
                }

                $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.ProductionBookingParameterId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                        formulaObj.ProductionBookingParameterHeadId = null;
                        formulaObj.Component = $scope.ModelProcessPara.Operator;
                        formulaObj.SalaryHead = $scope.ModelProcessPara.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelProcessPara.FormulaDes = '';
                        $scope.ModelProcessPara.FormulaDesID = '';

                        $scope.ModelProcessPara.FormulaDescription = '';
                        $scope.ModelProcessPara.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);

                        }

                        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                    }
                }
                else {
                    throw "First select Head or input value.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.ProductionBookingParameterId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.ProductionBookingParameterHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Precedence;
                    formulaObj.Component = $scope.ModelProcessPara.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);

                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelProcessPara.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.ProductionBookingParameterId = $scope.ModelProcessPara.Id == null ? null : $scope.ModelProcessPara.Id;
                    formulaObj.ProductionBookingParameterHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelProcessPara.Value;
                    formulaObj.Component = $scope.ModelProcessPara.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelProcessPara.FormulaDes = '';
                    $scope.ModelProcessPara.FormulaDesID = '';

                    $scope.ModelProcessPara.FormulaDescription = '';
                    $scope.ModelProcessPara.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelProcessPara.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);

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
                //$scope.ModelProcessPara.FormulaDesID += ' ' + $scope.FormulaDetails[i].ProductionBookingParameterHeadId;
                $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);
            } else {
                $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelProcessPara.FormulaDesID = ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);
            }
        }

        $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
        $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

    }

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.masterId = $scope.ModelNew.Id;
        $scope.GetSequence();
        $scope.GetProcessParameterData();
        $scope.GetQualityProcessList();
        $scope.GetOrderLineCostingItemCbo();
        $scope.Action = 'Update';
        $scope.ModelProcessPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelProcessPara.EntryState = 'Entry';
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
        if ($scope.ModelProcessPara.EntryState =="Calculate") {

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

                            $scope.ModelProcessPara.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);
                        } else {
                            $scope.ModelProcessPara.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelProcessPara.FormulaDesID = $scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId;
                        }
                    }

                    $scope.ModelProcessPara.FormulaDescription = $scope.ModelProcessPara.FormulaDes;
                    $scope.ModelProcessPara.FormulaIDDescription = $scope.ModelProcessPara.FormulaDesID;

                    $scope.ModelProcessPara.Formula = $scope.ModelProcessPara.FormulaDescription;
                    $scope.ModelProcessPara.FormulaId = $scope.ModelProcessPara.FormulaIDDescription;

                }
            });
        }


        var value = null;

        $scope.GetOrderLineCostingItemCbo();

    };

    $scope.GetQualityPro = function (args) {
        $scope.QualityAction = 'Update';
        $scope.ModelQualityNew = Object.assign({}, args.data);

    };



    function CheckField(fieldValue, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(fieldValue) || fieldValue === '') {
                throw ('[' + fieldName + '] is required...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.modelValidation = function (divId, modelName, fieldName, message) {
        var msg = fieldName + ' is required.';
        msg = baseService.isUndefinedOrNull(message) ? msg : message;
        var str = fieldName;
        if (baseService.isUndefinedOrNull($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else if (isNaN($scope[modelName][str.replace(/\s/g, '')]))
            throw manualValidation(divId, true, msg);
        else
            return manualValidation(divId, false);
    };
    $scope.masterId = null;
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelNewForm.$valid) {
                if ($scope.ModelNew.InPutOutPutRatio <= -1) {
                    return manualValidation('div_Ratio', true, "InPutOutPutRatio value can't less than -1 or -1.");

                }
                if ($scope.ModelNew.InPutOutPutRatio > 1) {
                    return manualValidation('div_Ratio', true, "InPutOutPutRatio value can't greater than 1.");
                }

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
                        $scope.ModelNew.Id = response.data.Id;
                        $scope.masterId = response.data.Id;
                        $scope.GetData();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

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

    $scope.SaveProcessParameter = function () {
        try {
            $scope.ModelProcessPara.ProductionBookingProcessParameterId = $scope.masterId;
            CheckField($scope.ModelProcessPara.ProductionBookingProcessParameterId, "Master");
            CheckField($scope.ModelProcessPara.UserName, "User Name");
            CheckField($scope.ModelProcessPara.SandardName, "Sandard Name");
            $scope.AddEditRow();

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
                    $scope.GetSequence();
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
        $scope.ModelProcessPara = { Id: null, ProductionBookingProcessParameterId: null, Sequence: 0, UserName: null, SandardName: null, Active: true, ValueinDecimal: false, ValueinPercentage: true, IsPreviousValueApplicable:true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelProcessParaNew = Object.assign({}, $scope.ModelProcessPara);
        $scope.ProductionAction = 'Save';
        $scope.GetSequence();
        $scope.ModelProcessPara.EntryState = 'Entry';
        $scope.ModelProcessPara.FormulaDescription = null;
        $scope.ModelProcessPara.FormulaIDDescription = null;
        $scope.ModelProcessPara.FormulaDes = null;
        $scope.ModelProcessPara.FormulaDesID = null;
        $scope.ModelProcessPara.Formula = null;
        $scope.ModelProcessPara.FormulaId = null;
        $scope.ModelProcessPara.HeadIdFormula = null;
        $scope.ModelProcessPara.Operator = null;
        $scope.ModelProcessPara.Precedence = null;
        $scope.ModelProcessPara.Value = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
    }

    $scope.QualityProcessList = [];
    $scope.GetQualityProcessList = function () {
        $scope.QualityProcessList = [];
        $http.get("Processes/ProductionBookingProcessparameter/GetQualityProcessList?masterId=" + $scope.masterId)
            .then(
                function successCallback(response) {
                    $scope.QualityProcessList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.QualityParameterList = [];
    $scope.GetQualityProcessParameterList = function () {
        $scope.QualityParameterList = [];
        $http.get("Processes/ProductionBookingProcessParameter/GetQualityProcessParameterList?masterId=" + $scope.QualityProcessId)
            .then(
                function successCallback(response) {
                    $scope.QualityParameterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.QualityAction = 'Save';
    $scope.saveQualityUrl = $scope.path + 'CreateQualityProcess';

    $scope.SaveQuality = function () {
        try {

            CheckField($scope.ModelQualityNew.ProcessId, "Process");
            CheckField($scope.ModelQualityNew.ItemName, "Item Name");

            $scope.ModelQualityNew.ProductionBookingProcessParameterId = $scope.masterId;
            $http({
                method: 'POST',
                url: $scope.saveQualityUrl,
                data: { 'data': $scope.ModelQualityNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetQualityProcessList();
                    $scope.ClearQuality();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearQuality = function () {
        $scope.QualityAction = 'Save';
        $scope.ModelQuality = { Id: null, ProcessId: null, ProductionBookingProcessParameterId: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
        $scope.ModelQualityNew = Object.assign({}, $scope.ModelQuality);
    }

    $scope.QualityProcessParameterHeaderList = [];
    $scope.GetQualityProcessParameterHeaderItemCbo = function () {
        try {
            $http({
                method: 'GET',
                url: 'Processes/ProductionBookingProcessparameter/GetQualityProcessParameterHeaderItemCbo?Id=' + $scope.ModelQualityParaNew.Id + '&masterId=' + $scope.QualityProcessId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.QualityProcessParameterHeaderList = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetQualityProcessParameterAutoSequence = function () {
        try {
            $http({
                method: 'GET',
                url: 'Processes/ProductionBookingProcessparameter/GetQualityProcessParameterAutoSequence?QualityProcessId=' + $scope.QualityProcessId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.ModelQualityParaNew.Sequence = response.data;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.QualityProcessId = null;
    $scope.GetQualityProcessParameterPopUp = function (obj) {
        $scope.QualityParameterAction = 'Save';
        $scope.QualityProcessId = obj.data.Id;
        $scope.ModelQualityPara = { Id: null, QualityProcessId: obj.data.Id, Sequence: 0, UserName: null, SandardName: null, ParameterGrade: null, GradeLot: null, IsCritical: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelQualityParaNew = Object.assign({}, $scope.ModelQualityPara);
        $scope.GetQualityProcessParameterList();
        $scope.GetQualityProcessParameterAutoSequence();
        $scope.GetQualityProcessParameterHeaderItemCbo();
        angular.element(document.querySelector('#QualityProcesspopup')).modal('show');
    }

    $scope.gradeList = [{ 'Value': 'A', 'Text': 'A' }, { 'Value': 'B', 'Text': 'B' }, { 'Value': 'C', 'Text': 'C' }];

    $scope.ModelQualityParaNew.FormulaDes = null;
    $scope.ModelQualityParaNew.FormulaDesID = null;
    $scope.ModelQualityParaNew.SalaryHeadFormula = null;
    $scope.ModelQualityParaNew.FormulaDescription = null;
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];
    $scope.FormulaDetails = [];
    $scope.SetQualityParaFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.QualityProcessParameterId = $scope.ModelQualityParaNew.Id == null ? null : $scope.ModelQualityParaNew.Id;
                formulaObj.QualityProcessParameterHeadId = $scope.ModelQualityParaNew.HeadIdFormula;
                formulaObj.SalaryHead = $("#QHeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.ModelQualityParaNew.FormulaDes = '';
                $scope.ModelQualityParaNew.FormulaDesID = '';

                $scope.ModelQualityParaNew.FormulaDescription = '';
                $scope.ModelQualityParaNew.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.FormulaDes)) {
                        $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].ProductionBookingParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].ProductionBookingParameterHeadId);
                    } else {
                        $scope.ModelQualityParaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelQualityParaNew.FormulaDesID = $scope.FormulaDetails[i].ProductionBookingParameterHeadId;
                    }
                }

                $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
                $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.QualityProcessParameterId = $scope.ModelQualityParaNew.Id == null ? null : $scope.ModelQualityParaNew.Id;
                        formulaObj.QualityProcessParameterHeadId = null;
                        formulaObj.Component = $scope.ModelQualityParaNew.Operator;
                        formulaObj.SalaryHead = $scope.ModelQualityParaNew.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.ModelQualityParaNew.FormulaDes = '';
                        $scope.ModelQualityParaNew.FormulaDesID = '';

                        $scope.ModelQualityParaNew.FormulaDescription = '';
                        $scope.ModelQualityParaNew.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);

                        }

                        $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
                        $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;

                    }
                }
                else {
                    throw "First select Head or input value.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.QualityProcessParameterId = $scope.ModelQualityParaNew.Id == null ? null : $scope.ModelQualityParaNew.Id;
                    formulaObj.QualityProcessParameterHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelQualityParaNew.Precedence;
                    formulaObj.Component = $scope.ModelQualityParaNew.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.ModelQualityParaNew.FormulaDes = '';
                    $scope.ModelQualityParaNew.FormulaDesID = '';

                    $scope.ModelQualityParaNew.FormulaDescription = '';
                    $scope.ModelQualityParaNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);

                    }

                    $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
                    $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.QualityProcessParameterId = $scope.ModelQualityParaNew.Id == null ? null : $scope.ModelQualityParaNew.Id;
                    formulaObj.QualityProcessParameterHeadId = null;
                    formulaObj.SalaryHead = $scope.ModelQualityParaNew.Value;
                    formulaObj.Component = $scope.ModelQualityParaNew.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.ModelQualityParaNew.FormulaDes = '';
                    $scope.ModelQualityParaNew.FormulaDesID = '';

                    $scope.ModelQualityParaNew.FormulaDescription = '';
                    $scope.ModelQualityParaNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);

                    }

                    $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
                    $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveQualityParaFormula = function () {

        var maxseq = Math.max.apply(Math, $scope.FormulaDetails.map(function (o) { return o.Sequence; }))

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (maxseq === $scope.FormulaDetails[i].Sequence) {
                $scope.FormulaDetails.splice(i, 1);
                break;
            }
        }

        $scope.ModelQualityParaNew.FormulaDes = '';
        $scope.ModelQualityParaNew.FormulaDesID = '';

        $scope.ModelQualityParaNew.FormulaDescription = '';
        $scope.ModelQualityParaNew.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.FormulaDes)) {
                $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);
            } else {
                $scope.ModelQualityParaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);
            }
        }

        $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
        $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;

    }

    $scope.AddQualityProcessEditRow = function () {
        try {
            $scope.ModelQualityParaNew.FormulaDes = $scope.ModelQualityParaNew.FormulaDescription;
            $scope.ModelQualityParaNew.FormulaDesID = $scope.ModelQualityParaNew.FormulaIDDescription;

            $scope.ModelQualityParaNew.Formula = $scope.ModelQualityParaNew.FormulaDescription;
            $scope.ModelQualityParaNew.FormulaId = $scope.ModelQualityParaNew.FormulaIDDescription;

            $scope.ModelQualityParaNew.SalaryHead = $("#HeadFormula option:selected").text();

            $scope.Row = 'Add Row';
            $scope.ModelQualityParaNew.FormulaDescription = null;
            $scope.ModelQualityParaNew.FormulaIDDescription = null;

            $scope.ModelQualityParaNew.HeadIdFormula = null;
            $scope.ModelQualityParaNew.Operator = null;
            $scope.ModelQualityParaNew.Precedence = null;
            $scope.ModelQualityParaNew.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.QualityParameterAction = 'Save';
    $scope.SaveQualityProcess = function () {
        try {
            CheckField($scope.ModelQualityParaNew.Sequence, "Sequence");
            CheckField($scope.ModelQualityParaNew.UserName, "User Name");
            CheckField($scope.ModelQualityParaNew.SandardName, "Sandard Name");
            CheckField($scope.ModelQualityParaNew.ParameterGrade, "Grade");
            CheckField($scope.ModelQualityParaNew.GradeLot, "Grade Lot No");

            $scope.AddQualityProcessEditRow();
            $scope.ModelQualityParaNew.QualityProcessId = $scope.QualityProcessId;
            $http({
                method: 'POST',
                url: $scope.saveQualityProcessParameterUrl,
                data: { 'data': $scope.ModelQualityParaNew, 'details': $scope.FormulaDetails },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetQualityProcessParameterAutoSequence();
                    $scope.GetQualityProcessParameterHeaderItemCbo();
                    $scope.GetQualityProcessParameterList();
                    $scope.ClearQualityProcessPara();
                    $scope.FormulaDetails = [];
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetQualityPara = function (obj) {
        $scope.QualityParameterAction = 'Update';

        $scope.FormulaDetails = [];
        $scope.ModelQualityParaNew.HeadIdFormula = null;
        $scope.ModelQualityParaNew.Operator = null;
        $scope.ModelQualityParaNew.Precedence = null;
        $scope.ModelQualityParaNew.Value = null;

        $scope.objectData = obj.data;
        $scope.ModelQualityParaNew = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "Processes/ProductionBookingProcessparameter/GetQualityProcessParameterDetailList?QualityProcessParameterId=" + $scope.ModelQualityParaNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.ModelQualityParaNew.FormulaDes = '';
                $scope.ModelQualityParaNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.ModelQualityParaNew.FormulaDes)) {
                        $scope.ModelQualityParaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.ModelQualityParaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId);
                    } else {
                        $scope.ModelQualityParaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelQualityParaNew.FormulaDesID = $scope.FormulaDetails[i].QualityProcessParameterHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].QualityProcessParameterHeadId;
                    }
                }

                $scope.ModelQualityParaNew.FormulaDescription = $scope.ModelQualityParaNew.FormulaDes;
                $scope.ModelQualityParaNew.FormulaIDDescription = $scope.ModelQualityParaNew.FormulaDesID;


            }
        });


        var value = null;

        $scope.GetQualityProcessParameterHeaderItemCbo();

    };

    $scope.ClearQualityProcessPara = function () {
        $scope.ModelQualityPara = { Id: null, QualityProcessId: $scope.QualityProcessId, Sequence: 0, UserName: null, SandardName: null, IsProduction: false, IsVisible: false, Active: true, ValueinDecimal: false, ValueinPercentage: true, DefaultValue: null, EntryState: 'Entry', FormulaId: null, Formula: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, FormulaDescription: null }
        $scope.ModelQualityParaNew = Object.assign({}, $scope.ModelQualityPara);
        $scope.QualityParameterAction = 'Save';
        $scope.GetQualityProcessParameterHeaderItemCbo();
        $scope.GetQualityProcessParameterList();
        $scope.GetQualityProcessParameterAutoSequence();
        $scope.ModelQualityParaNew.EntryState = 'Entry';
        $scope.ModelQualityParaNew.FormulaDescription = null;
        $scope.ModelQualityParaNew.FormulaIDDescription = null;
        $scope.ModelQualityParaNew.FormulaDescription = null;
        $scope.ModelQualityParaNew.FormulaIDDescription = null;
        $scope.ModelQualityParaNew.FormulaDes = null;
        $scope.ModelQualityParaNew.FormulaDesID = null;
        $scope.ModelQualityParaNew.Formula = null;
        $scope.ModelQualityParaNew.FormulaId = null;
        $scope.ModelQualityParaNew.HeadIdFormula = null;
        $scope.ModelQualityParaNew.Operator = null;
        $scope.ModelQualityParaNew.Precedence = null;
        $scope.ModelQualityParaNew.Value = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
    }

    $scope.message_PrductionParaconfirmation = null;
    $scope.removePrductionPara = function (obj) {

        $scope.PrductionPara = obj.data;
        if (!baseService.isUndefinedOrNull($scope.PrductionPara.Id))
            $scope.message_PrductionParaconfirmation = 'Are you sure want to delete permanently [ ' + $scope.PrductionPara.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeleteProductionBookingParameterPopUp')).modal('show');
    }

    $scope.DeleteProductionBookingParameter = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProductionBookingProcessparameter/DeleteProductionBookingParameter?id=' + $scope.PrductionPara.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProcessParameterData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.message_Qualityconfirmation = null;
    $scope.removeQuality = function (obj) {
        $scope.QualityNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.QualityNew.Id))
            $scope.message_Qualityconfirmation = 'Are you sure want to delete permanently [ ' + $scope.QualityNew.Process + ' ]';
        angular.element(document.querySelector('#confirmDeleteQualityPopUp')).modal('show');
    }

    $scope.DeleteQuality = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProductionBookingProcessparameter/DeleteQualityProcess?id=' + $scope.QualityNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetQualityProcessList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.message_QualityParaconfirmation = null;
    $scope.removeQualityPara = function (obj) {
        $scope.QualityParaNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.QualityParaNew.Id))
            $scope.message_QualityParaconfirmation = 'Are you sure want to delete permanently [ ' + $scope.QualityParaNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmDeleteQualityParaPopUp')).modal('show');
    }

    $scope.DeleteQualityPara = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProductionBookingProcessparameter/DeleteQualityProcessParameter?id=' + $scope.QualityParaNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetQualityProcessParameterAutoSequence();
                $scope.GetQualityProcessParameterHeaderItemCbo();
                $scope.GetQualityProcessParameterList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
}
