'use strict';
OrderLineCostingItemController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OrderLineCostingItemController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "OrderLineCostingItem";
    $scope.Action = 'Save';
    $scope.FormulaDetails = [];
    $scope.path = 'Costings/OrderLineCostingItem/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.Model = {
        Id: null,
        PlantId: null,
        Sequence: null,
        UserName: null,
        LineItemCostingSandardName: null,
        CostingSegment: null,
        SOItemName: null,
        Active: true,
        IsFixedValue: false,
        ValueInPercentage: null,
        FixedValue: null,
        Formula: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        Operator: null,
        Precedence: null,
        Value:null
    }
    $scope.ModelNew = Object.assign({}, $scope.Model);

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

    $scope.CostingComponentList = [];
    $scope.GetCostingTypeComponent = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/costingTypeComponent/GetCostingComponent?costingType=' + $scope.ModelNew.CostingType,
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


    $scope.NoticePeriodList = [];
    $scope.GetData = function () {
        $scope.NoticePeriodList = [];
        $http.get("payrolls/NoticePeriodSetting/GetList?plantId=" + $scope.ModelNew.PlantId)
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

    //$scope.ModelNew.Formula = null;
    //$scope.ModelNew.FormulaDesID = null;
    //$scope.ModelNew.SalaryHeadFormula = null;
    //$scope.ModelNew.FormulaDescription = null;
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

            if (formula === 'Value') {
                $scope.ModelNew.FormulaDescription = null;
                $scope.ModelNew.FormulaIDDescription = null;

                if (!baseService.isUndefinedOrNull($scope.ModelNew.Value)) {
                    $scope.ModelNew.FormulaDes = $scope.ModelNew.Value;
                    $scope.ModelNew.FormulaDesID = $scope.ModelNew.Value;
                }
                $scope.FormulaArray.push($scope.ModelNew.FormulaDes);

                $scope.ModelNew.FormulaIDDescription = null;
                $scope.ModelNew.FormulaDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                        $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

               

            }

            else if (formula === 'Operator') {
                $scope.ModelNew.FormulaIDDescription = null;
                $scope.ModelNew.FormulaDescription = null;

                if (!baseService.isUndefinedOrNull($scope.ModelNew.Operator)) {
                    $scope.ModelNew.FormulaDes = $scope.ModelNew.Operator;
                    $scope.ModelNew.FormulaDesID = $scope.ModelNew.Operator;
                }
                $scope.FormulaArray.push($scope.ModelNew.FormulaDes);

                $scope.ModelNew.FormulaIDDescription = null;
                $scope.ModelNew.FormulaDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                        $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

                

            }
            else if (formula === 'Precedence') {
                $scope.ModelNew.FormulaDescription = null;
                $scope.ModelNew.FormulaIDDescription = null;

                if (!baseService.isUndefinedOrNull($scope.ModelNew.Precedence)) {
                    $scope.ModelNew.FormulaDes = $scope.ModelNew.Precedence;
                }
                $scope.FormulaArray.push($scope.ModelNew.FormulaDes);

                $scope.ModelNew.FormulaIDDescription = null;
                $scope.ModelNew.FormulaDescription = null;
                for (var i = 0; i < $scope.FormulaArray.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.ModelNew.FormulaDescription)) {
                        $scope.ModelNew.FormulaDescription = $scope.FormulaArray[i];
                    }
                    else {
                        $scope.ModelNew.FormulaDescription += ' ' + $scope.FormulaArray[i];
                    }
                }

                

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



    $scope.Get = function (obj) {
        $scope.FormulaDetails = [];
        $scope.CompanyId = $scope.ModelNew.CompanyId;
        $scope.ModelNew.SalaryHeadIdFormula = null;
        $scope.ModelNew.Operator = null;
        $scope.ModelNew.Precedence = null;
        $scope.ModelNew.Value = null;

        $scope.objectData = obj.data;
        $scope.ModelNew = Object.assign({}, $scope.objectData);

        $http({
            method: 'GET',
            url: "payrolls/NoticePeriodSetting/GetDetailList?NoticePeriodSettingId=" + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.FormulaDetails = response.data;

                $scope.ModelNew.Formula = '';
                $scope.ModelNew.FormulaDesID = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                    if (!baseService.isUndefinedOrNull($scope.ModelNew.Formula)) {
                        $scope.ModelNew.Formula += ' ' + $scope.FormulaDetails[i].SalaryHead;

                        $scope.ModelNew.FormulaDesID += ' ' + ($scope.FormulaDetails[i].SalaryHeadID == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].SalaryHeadID);
                    } else {
                        $scope.ModelNew.Formula = $scope.FormulaDetails[i].SalaryHead;
                        $scope.ModelNew.FormulaDesID = $scope.FormulaDetails[i].SalaryHeadID;
                    }
                }

                $scope.ModelNew.FormulaDescription = $scope.ModelNew.Formula;
                $scope.ModelNew.FormulaIDDescription = $scope.ModelNew.FormulaDesID;


            }
        });


        var value = null;

        $scope.ModelNew.CompanyId = $scope.CompanyId;

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

            // CheckDuplicate($scope.ModelNew);

            $scope.ModelNew.Formula = $scope.ModelNew.FormulaDescription;
            $scope.ModelNew.FormulaDesID = $scope.ModelNew.FormulaIDDescription;
            $scope.ModelNew.SalaryHead = $("#SH option:selected").text();

            $scope.Row = 'Add Row';
            $scope.ModelNew.FormulaDescription = null;
            $scope.ModelNew.FormulaIDDescription = null;

            $scope.ModelNew.SalaryHeadIdFormula = null;
            $scope.ModelNew.Operator = null;
            $scope.ModelNew.Precedence = null;
            $scope.ModelNew.Value = null;

            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.CompanyId = $scope.ModelNew.CompanyId;
        $scope.PlantId = $scope.ModelNew.PlantId;
        $scope.ModelNew = {};
        $scope.ModelNew.CompanyId = $scope.CompanyId;
        $scope.ModelNew.PlantId = $scope.PlantId;
        $scope.Action = 'Save';
        $scope.ModelNew.FormulaDescription = null;
        $scope.ModelNew.FormulaIDDescription = null;
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
            //if (baseService.isUndefinedOrNull($scope.ModelNew.CompanyId)) {
            //    throw "Company is required.";
            //}
            //if (baseService.isUndefinedOrNull($scope.ModelNew.PlantId)) {
            //    throw "Plant is required.";
            //}
            $scope.ModelNew.Formula = $scope.ModelNew.FormulaDescription;
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew/*, 'details': $scope.FormulaDetails*/ },
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
                url: 'payrolls/NoticePeriodSetting/Delete?id=' + $scope.ModelNew.Id
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
