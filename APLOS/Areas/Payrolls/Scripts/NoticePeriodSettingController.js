'use strict';
NoticePeriodSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', 'cboService'];
function NoticePeriodSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, cboService) {
    $rootScope.title = "Notice Period Setting";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Payrolls/NoticePeriodSetting/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.NoticePeriodNew = {
        Id: null, CompanyId: null, PlantId: null, FormulaDes: null, FormulaDesID: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, SalaryHeadID: null
    }

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.NoticePeriodNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });


    $scope.NoticePeriodList = [];
    $scope.GetData = function () {
        $scope.NoticePeriodList = [];
        $http.get("payrolls/NoticePeriodSetting/GetList?plantId=" + $scope.NoticePeriodNew.PlantId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.NoticePeriodList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.NoticePeriodNew.FormulaDes = null;
    $scope.NoticePeriodNew.FormulaDesID = null;
    $scope.NoticePeriodNew.SalaryHeadFormula = null;
    $scope.NoticePeriodNew.FormulaDescription = null;
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
                formulaObj.NoticePeriodSettingId = $scope.NoticePeriodNew.Id == null ? null : $scope.NoticePeriodNew.Id;
                formulaObj.SalaryHeadID = $scope.NoticePeriodNew.SalaryHeadIdFormula;
                formulaObj.SalaryHead = $("#SalaryHeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.NoticePeriodNew.FormulaDes = '';
                $scope.NoticePeriodNew.FormulaDesID = '';

                $scope.NoticePeriodNew.FormulaDescription = '';
                $scope.NoticePeriodNew.FormulaIDDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        //$scope.NoticePeriodNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.NoticePeriodSettingId = $scope.NoticePeriodNew.Id == null ? null : $scope.NoticePeriodNew.Id;
                        formulaObj.SalaryHeadID = null;
                        formulaObj.Component = $scope.NoticePeriodNew.Operator;
                        formulaObj.SalaryHead = $scope.NoticePeriodNew.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.NoticePeriodNew.FormulaDes = '';
                        $scope.NoticePeriodNew.FormulaDesID = '';

                        $scope.NoticePeriodNew.FormulaDescription = '';
                        $scope.NoticePeriodNew.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                            $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                          
                        }

                        $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                        $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

                    }
                }
                else {
                    throw "First select Salary Head or input value.";
                }



            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.NoticePeriodSettingId = $scope.NoticePeriodNew.Id == null ? null : $scope.NoticePeriodNew.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.NoticePeriodNew.Precedence;
                    formulaObj.Component = $scope.NoticePeriodNew.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.NoticePeriodNew.FormulaDes = '';
                    $scope.NoticePeriodNew.FormulaDesID = '';

                    $scope.NoticePeriodNew.FormulaDescription = '';
                    $scope.NoticePeriodNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                    $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.NoticePeriodSettingId = $scope.NoticePeriodNew.Id == null ? null : $scope.NoticePeriodNew.Id;
                    formulaObj.SalaryHeadID = null;
                    formulaObj.SalaryHead = $scope.NoticePeriodNew.Value;
                    formulaObj.Component = $scope.NoticePeriodNew.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.NoticePeriodNew.FormulaDes = '';
                    $scope.NoticePeriodNew.FormulaDesID = '';

                    $scope.NoticePeriodNew.FormulaDescription = '';
                    $scope.NoticePeriodNew.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);

                    }

                    $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                    $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

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

        $scope.NoticePeriodNew.FormulaDes = '';
        $scope.NoticePeriodNew.FormulaDesID = '';

        $scope.NoticePeriodNew.FormulaDescription = '';
        $scope.NoticePeriodNew.FormulaIDDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                $scope.NoticePeriodNew.FormulaDesID += ' ' + $scope.FormulaDetails[i].SalaryHeadID;
            } else {
                $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
            }
        }

        $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
        $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;

    }

    $scope.Get = function (obj) {
        $scope.FormulaDetails = [];
        $scope.CompanyId = $scope.NoticePeriodNew.CompanyId;
        $scope.NoticePeriodNew.SalaryHeadIdFormula = null;
        $scope.NoticePeriodNew.Operator = null;
        $scope.NoticePeriodNew.Precedence = null;
        $scope.NoticePeriodNew.Value = null;

        $scope.objectData = obj.data;
        $scope.NoticePeriodNew = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "payrolls/NoticePeriodSetting/GetDetailList?NoticePeriodSettingId=" + $scope.NoticePeriodNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.NoticePeriodNew.FormulaDes = '';
                $scope.NoticePeriodNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    
                    if (!baseService.isUndefinedOrNull($scope.NoticePeriodNew.FormulaDes)) {
                        $scope.NoticePeriodNew.FormulaDes += ' ' + $scope.FormulaDetails[i].SalaryHead;
                        
                        $scope.NoticePeriodNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.NoticePeriodNew.FormulaDes = $scope.FormulaDetails[i].SalaryHead;
                        $scope.NoticePeriodNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.NoticePeriodNew.FormulaDescription = $scope.NoticePeriodNew.FormulaDes;
                $scope.NoticePeriodNew.FormulaIDDescription = $scope.NoticePeriodNew.FormulaDesID;


            }
        });


        var value = null;

        $scope.NoticePeriodNew.CompanyId = $scope.CompanyId;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < $scope.NoticePeriodList.length; i++) {
                if (ob.SalaryRuleGeneralSystemID !== $scope.NoticePeriodList[i].SalaryRuleGeneralSystemID && ob.SalaryHeadID === $scope.NoticePeriodList[i].SalaryHeadID) {
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

            // CheckDuplicate($scope.NoticePeriodNew);

            $scope.NoticePeriodNew.FormulaDes = $scope.NoticePeriodNew.FormulaDescription;
            $scope.NoticePeriodNew.FormulaDesID = $scope.NoticePeriodNew.FormulaIDDescription;
            $scope.NoticePeriodNew.SalaryHead = $("#SH option:selected").text();

            $scope.Row = 'Add Row';
            $scope.NoticePeriodNew.FormulaDescription = null;
            $scope.NoticePeriodNew.FormulaIDDescription = null;

            $scope.NoticePeriodNew.SalaryHeadIdFormula = null;
            $scope.NoticePeriodNew.Operator = null;
            $scope.NoticePeriodNew.Precedence = null;
            $scope.NoticePeriodNew.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.CompanyId = $scope.NoticePeriodNew.CompanyId;
        $scope.PlantId = $scope.NoticePeriodNew.PlantId;
        $scope.NoticePeriodNew = {};
        $scope.NoticePeriodNew.CompanyId = $scope.CompanyId;
        $scope.NoticePeriodNew.PlantId = $scope.PlantId;
        $scope.Action = 'Save';
        $scope.NoticePeriodNew.FormulaDescription = null;
        $scope.NoticePeriodNew.FormulaIDDescription = null;
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
            if (baseService.isUndefinedOrNull($scope.NoticePeriodNew.CompanyId)) {
                throw "Company is required.";
            }
            if (baseService.isUndefinedOrNull($scope.NoticePeriodNew.PlantId)) {
                throw "Plant is required.";
            }
            $scope.AddEditRow();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.NoticePeriodNew, 'details': $scope.FormulaDetails },
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
                url: 'payrolls/NoticePeriodSetting/Delete?id=' + $scope.NoticePeriodNew.Id
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
