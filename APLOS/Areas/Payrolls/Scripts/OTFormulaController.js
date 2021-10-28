'use strict';
OTFormulaController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function OTFormulaController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "OT Formula";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Payrolls/OTFormula/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';


    $scope.OTFormulaNew = {
        Id: null
        , CompanyId: null
        , Sequence: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , FormulaDes: null
        , FormulaDesID: null
        , Active: true
        , AddedBy: null
        , AddedDate: null
        , AddedFromIP: null
        , UpdatedBy: null
        , UpdatedDate: null
        , UpdatedFromIP: null
        , SalaryHeadID: null
    }

    $scope.GetSequence = function () {
        $http.get("payrolls/OTFormula/GetSequence?CompanyId=" + $scope.OTFormulaNew.CompanyId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.OTFormulaNew.Sequence = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });


    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });


    $scope.OTFormulaList = [];
    $scope.GetData = function () {
        $scope.OTFormulaList = [];
        $http.get("payrolls/OTFormula/GetList?CompanyId=" + $scope.OTFormulaNew.CompanyId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.OTFormulaList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.OTFormulaNew.FormulaDes = null;
    $scope.OTFormulaNew.FormulaDesID = null;
    $scope.OTFormulaNew.SalaryHeadFormula = null;
    $scope.OTFormulaNew.FormulaDescription = null;
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
                formulaObj.OTFormulaId = $scope.OTFormulaNew.Id == null ? null : $scope.OTFormulaNew.Id;
                formulaObj.SalaryHeadID = $scope.OTFormulaNew.SalaryHeadIdFormula;
                formulaObj.SalaryHead = $("#SalaryHeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.OTFormulaNew.FormulaDes = '';
                $scope.OTFormulaNew.FormulaDesID = '';

                $scope.OTFormulaNew.FormulaDescription = '';
                $scope.OTFormulaNew.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.FormulaDes)) {
                        $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        //$scope.OTFormulaNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
                        $scope.OTFormulaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.OTFormulaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.OTFormulaNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
                $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.OTFormulaId = $scope.OTFormulaNew.Id == null ? null : $scope.OTFormulaNew.Id;
                        formulaObj.SalaryHeadID = null;
                        formulaObj.Component = $scope.OTFormulaNew.Operator;
                        formulaObj.SalaryHead = $scope.OTFormulaNew.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.OTFormulaNew.FormulaDes = '';
                        $scope.OTFormulaNew.FormulaDesID = '';

                        $scope.OTFormulaNew.FormulaDescription = '';
                        $scope.OTFormulaNew.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.OTFormulaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                        }

                        $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
                        $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }



            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OTFormulaId = $scope.OTFormulaNew.Id == null ? null : $scope.OTFormulaNew.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.OTFormulaNew.Precedence;
                    formulaObj.Component = $scope.OTFormulaNew.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.OTFormulaNew.FormulaDes = '';
                    $scope.OTFormulaNew.FormulaDesID = '';

                    $scope.OTFormulaNew.FormulaDescription = '';
                    $scope.OTFormulaNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.OTFormulaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
                    $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.OTFormulaId = $scope.OTFormulaNew.Id == null ? null : $scope.OTFormulaNew.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.OTFormulaNew.Value;
                    formulaObj.Component = $scope.OTFormulaNew.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.OTFormulaNew.FormulaDes = '';
                    $scope.OTFormulaNew.FormulaDesID = '';

                    $scope.OTFormulaNew.FormulaDescription = '';
                    $scope.OTFormulaNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.OTFormulaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
                    $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;

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

        $scope.OTFormulaNew.FormulaDes = '';
        $scope.OTFormulaNew.FormulaDesID = '';

        $scope.OTFormulaNew.FormulaDescription = '';
        $scope.OTFormulaNew.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.FormulaDes)) {
                $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.OTFormulaNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
            } else {
                $scope.OTFormulaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.OTFormulaNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
            }
        }

        $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
        $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;

    }

    $scope.Get = function (obj) {
        $scope.FormulaDetails = [];
        //$scope.CompanyId = $scope.OTFormulaNew.CompanyId;
        $scope.OTFormulaNew.SalaryHeadIdFormula = null;
        $scope.OTFormulaNew.Operator = null;
        $scope.OTFormulaNew.Precedence = null;
        $scope.OTFormulaNew.Value = null;

        $scope.objectData = obj.data;
        $scope.OTFormulaNew = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "payrolls/OTFormula/GetDetailList?OTFormulaId=" + $scope.OTFormulaNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.OTFormulaNew.FormulaDes = '';
                $scope.OTFormulaNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.OTFormulaNew.FormulaDes)) {
                        $scope.OTFormulaNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.OTFormulaNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.OTFormulaNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.OTFormulaNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.OTFormulaNew.FormulaDescription = $scope.OTFormulaNew.FormulaDes;
                $scope.OTFormulaNew.FormulaIDDescription = $scope.OTFormulaNew.FormulaDesID;


            }
        });


        var value = null;

        $scope.OTFormulaNew.CompanyId = $scope.CompanyId;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < $scope.OTFormulaList.length; i++) {
                if (ob.SalaryRuleGeneralSystemID !== $scope.OTFormulaList[i].SalaryRuleGeneralSystemID && ob.SalaryHeadID === $scope.OTFormulaList[i].SalaryHeadID) {
                    throw "Salary Head has already been taken...";
                }
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.AddEditRow = function () {
        try {

            // ValidationRuleGeneral();

            // CheckDuplicate($scope.OTFormulaNew);

            $scope.OTFormulaNew.FormulaDes = $scope.OTFormulaNew.FormulaDescription;
            $scope.OTFormulaNew.FormulaDesID = $scope.OTFormulaNew.FormulaIDDescription;
            $scope.OTFormulaNew.SalaryHead = $("#SH option:selected").text();

            $scope.Row = 'Add Row';
            $scope.OTFormulaNew.FormulaDescription = null;
            $scope.OTFormulaNew.FormulaIDDescription = null;

            $scope.OTFormulaNew.SalaryHeadIdFormula = null;
            $scope.OTFormulaNew.Operator = null;
            $scope.OTFormulaNew.Precedence = null;
            $scope.OTFormulaNew.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.CompanyId = $scope.OTFormulaNew.CompanyId;
        $scope.OTFormulaNew = {};
        $scope.OTFormulaNew.CompanyId = $scope.CompanyId;
        $scope.OTFormulaNew.Active = true;
        $scope.Action = 'Save';
        $scope.OTFormulaNew.FormulaDescription = null;
        $scope.OTFormulaNew.FormulaIDDescription = null;
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
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
            if (baseService.isUndefinedOrNull($scope.OTFormulaNew.CompanyId)) {
                throw "Company is required.";
            }

            $scope.AddEditRow();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.OTFormulaNew, 'details': $scope.FormulaDetails },
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


    $scope.Delete = function () {
        try {
            $http({
                method: 'POST',
                url: 'payrolls/OTFormula/Delete?id=' + $scope.OTFormulaNew.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetData();
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



}
