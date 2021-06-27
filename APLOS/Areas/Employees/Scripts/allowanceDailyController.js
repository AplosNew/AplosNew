'use strict';
allowanceDailyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function allowanceDailyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Allowance Daily';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.bloodGroups = [];
    $scope.path = 'employees/AllowanceDaily/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.bloodGroups = result;
              
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.bloodGroup = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        SalaryHeadId: null,
        Description: null,
        Remarks: null,
        Active: true,
        FormulaDescription: null,
        FormulaDesID: null,
        CalculationBasics: 'Rate',
        Catagory: null
    };

    $scope.bloodGroupNew = Object.assign({}, $scope.bloodGroup);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.bloodGroupNew.Sequence = data[0].Sequence;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.bloodGroup = $scope.bloodGroups[$scope.index];
        $scope.bloodGroupNew = Object.assign({}, $scope.bloodGroup);
        $scope.Action = 'Update';
        if ($scope.bloodGroupNew.CalculationBasics == 'Formula') {
            $scope.ShowFormulaDiv = true;
        } else {
            $scope.ShowFormulaDiv = false;
        }
        $scope.salaryRuleGeneral.FormulaDescription = $scope.bloodGroupNew.FormulaDescription;
        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.bloodGroupNew.FormulaDesID;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    $scope.Save = function () {
        
        try {
           
            if ($scope.bloodGroupNew.CalculationBasics == 'Formula') {
                $scope.bloodGroup.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
                $scope.bloodGroup.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
                $scope.bloodGroupNew.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
                $scope.bloodGroupNew.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
                if (baseService.isUndefinedOrNull($scope.bloodGroupNew.Catagory)) {
                    throw "Please select Catagory.";
                }
                if (baseService.isUndefinedOrNull($scope.bloodGroupNew.FormulaDesID)) {
                    throw "Please select Formula.";
                }
            }
            else {
                $scope.bloodGroupNew.FormulaDescription = null;
                $scope.bloodGroupNew.FormulaDesID = null;
                $scope.bloodGroupNew.Catagory = null;
            }
            angular.copy($scope.bloodGroupNew, $scope.bloodGroup);
            $scope.$broadcast('show-errors-check-validity');
            //if ($scope.bloodGroupNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.bloodGroup,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.bloodGroups.push(response.data.bloodGroup);
                            $scope.bloodGroups = $filter('orderBy')($scope.bloodGroups, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields();
                            $scope.getData();
                            $scope.GetSequence();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.bloodGroup,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.bloodGroups[$scope.index] = $scope.bloodGroup;
                                $scope.bloodGroups = $filter('orderBy')($scope.bloodGroups, 'Sequence');
                            }
                            ClearFields();
                            $scope.GetSequence();

                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
           //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.bloodGroupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.bloodGroupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.bloodGroups.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.GetSequence();
                });
            
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.bloodGroup = {};
        $scope.bloodGroupNew = {}; 
        $scope.bloodGroupNew.Sequence = seq;
        $scope.bloodGroupNew.Active = true;
        $scope.bloodGroupNew.FormulaDescription = null;
        $scope.bloodGroupNew.FormulaDesID = null;
        $scope.bloodGroupNew.CalculationBasics = null;
        $scope.bloodGroupNew.Catagory = null;
        $scope.ShowFormulaDiv = false;
    }


    $scope.ShowFormulaDiv = false;
    $scope.ChangeCalculation = function () {
        if ($scope.bloodGroupNew.CalculationBasics =='Formula') {
            $scope.ShowFormulaDiv = true;
        } else {
            $scope.ShowFormulaDiv = false;
        }
      
    };
    $scope.salaryRuleGeneral = {
        FormulaDescription: null,
        FormulaIDDescription: null
    };
    //$scope.getData();
    $scope.salaryHeadList = [];
    //$scope.getSH = function () {
    //    try {
    //        $http.get($scope.getSHListUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.salaryHeadList = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    //$scope.getSH();
   
   
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.SetFormula = function (formula) {

        if (formula === 'SHead') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {
                $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.SalaryHeadFormula;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
            }

            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Operator') {
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Precedence') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Value') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
    };

    $scope.RemoveFormula = function () {
        $scope.salaryRuleGeneral.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDes += $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                $scope.salaryRuleGeneral.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    };
}