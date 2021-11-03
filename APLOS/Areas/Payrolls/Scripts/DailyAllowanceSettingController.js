'use strict';
DailyAllowanceSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyAllowanceSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Daily Allowance Setting';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.bloodGroups = [];
    $scope.DailyAllowanceList = [];
    $scope.path = 'Payrolls/DailyAllowanceSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.bloodGroups = result;

    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();
    $scope.CatagoryChangeMesssage = '';
    $scope.CatagoryChange = function () {
        if ($scope.bloodGroupNew.Catagory === 'WeekOffAllowance') {
            $scope.CatagoryChangeMesssage = 'Affecting only non-OT holder.';
        }
        else if ($scope.bloodGroupNew.Catagory === 'HolidayAllowance') {
            $scope.CatagoryChangeMesssage = 'Affecting only non-OT holder.';
        }
        else {
            $scope.CatagoryChangeMesssage = '';
        }
    };


    $scope.GetDailyAllowanceList = function () {
        try {
            $http.get($scope.getListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.DailyAllowanceList = response.data;//DailyAllowanceList
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetDailyAllowanceList();

    $scope.DailyAllowanceCatagoryList = [];
    cboService.getEnumCbo("enum/GetDailyAllowanceCatagoryEnumCbo", function (result) {
        $scope.DailyAllowanceCatagoryList = result;
    });

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
        Catagory: null,
        FromEffectiveDate: null,
        ToEffectiveDate: null,
        IsRateBasedOnSalaryRange: false,
        SalaryRangeBasedOnSalaryHeadId: null,
        IsVoucherPayment: false
    };

    $scope.bloodGroupNew = Object.assign({}, $scope.bloodGroup);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.bloodGroupNew.Sequence = data[0].Sequence;
        });
    };
    $scope.GetSequence();

    $scope.Get = function () {
        $scope.ShowSalaryRangeBasedOnSalaryHeadId = false;
        var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
        $scope.bloodGroup = gridObj.getSelectedRecords()[0];

        //$scope.index = index;
        //$scope.bloodGroup = $scope.bloodGroups[$scope.index];
        $scope.bloodGroupNew = Object.assign({}, $scope.bloodGroup);
        $scope.Action = 'Update';
        if ($scope.bloodGroupNew.CalculationBasics == 'Formula') {
            $scope.ShowFormulaDiv = true;
        } else {
            $scope.ShowFormulaDiv = false;
        }
        $scope.salaryRuleGeneral.FormulaDescription = $scope.bloodGroupNew.FormulaDescription;
        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.bloodGroupNew.FormulaDesID;



        if ($scope.bloodGroupNew.IsRateBasedOnSalaryRange == true) {
            $scope.ShowSalaryRangeBasedOnSalaryHeadId = true;
        }
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.ShowSalaryRangeBasedOnSalaryHeadId = false;
    $scope.RateBasedOnSalaryRangeChange = function () {
        if ($scope.bloodGroupNew.IsRateBasedOnSalaryRange == true) {
            $scope.ShowSalaryRangeBasedOnSalaryHeadId = true;
        } else {
            $scope.ShowSalaryRangeBasedOnSalaryHeadId = false;
            $scope.bloodGroupNew.SalaryRangeBasedOnSalaryHeadId = null;
        }

    };



    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    $scope.Save = function () {

        try {

            //if ($scope.bloodGroupNew.CalculationBasics == 'Formula') {
            //    $scope.bloodGroup.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
            //    $scope.bloodGroup.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            //    $scope.bloodGroupNew.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
            //    $scope.bloodGroupNew.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            //    if (baseService.isUndefinedOrNull($scope.bloodGroupNew.Catagory)) {
            //        throw "Please select Catagory.";
            //    }
            //    if (baseService.isUndefinedOrNull($scope.bloodGroupNew.FormulaDesID)) {
            //        throw "Please select Formula.";
            //    }
            //}
            //else {
            //    $scope.bloodGroupNew.FormulaDescription = null;
            //    $scope.bloodGroupNew.FormulaDesID = null;
            //    $scope.bloodGroupNew.Catagory = null;
            //}



            angular.copy($scope.bloodGroupNew, $scope.bloodGroup);

            if (!baseService.isUndefinedOrNull($scope.bloodGroupNew.IsRateBasedOnSalaryRange)) {
                if ($scope.bloodGroupNew.IsRateBasedOnSalaryRange == true) {
                    if (baseService.isUndefinedOrNull($scope.bloodGroupNew.SalaryRangeBasedOnSalaryHeadId)) {
                        throw "Select Salary Head for Salary Range.";
                    };
                }
            };

            if (baseService.isUndefinedOrNull($scope.bloodGroupNew.SalaryHeadId)) {
                throw "Enter Salary Head.";
            };
            if (baseService.isUndefinedOrNull($scope.bloodGroupNew.Catagory)) {
                throw "Enter Catagory.";
            };

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
                        //$scope.getData();
                        $scope.GetSequence();
                        $scope.GetDailyAllowanceList();


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
                        $scope.GetDailyAllowanceList();

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
                    $scope.GetDailyAllowanceList();
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

    ///===============================================================================================
    $scope.ShowFormulaDiv = false;
    $scope.ChangeCalculation = function () {
        if ($scope.bloodGroupNew.CalculationBasics == 'Formula') {
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



    //$scope.FormulaArray = [];
    //$scope.FormulaIdArray = [];

    //$scope.SetFormula = function (formula) {

    //    if (formula === 'SHead') {
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;

    //        if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {
    //            $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

    //            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.SalaryHeadFormula;
    //            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
    //        }

    //        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
    //        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;

    //        for (var i = 0; i < $scope.FormulaArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
    //                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
    //            }
    //        }

    //        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
    //                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
    //            }
    //        }

    //    }
    //    else if (formula === 'Operator') {
    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;
    //        $scope.salaryRuleGeneral.FormulaDescription = null;

    //        if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {
    //            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
    //            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
    //        }
    //        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
    //        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        for (var i = 0; i < $scope.FormulaArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
    //                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
    //            }
    //        }

    //        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
    //                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
    //            }
    //        }

    //    }
    //    else if (formula === 'Precedence') {
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;

    //        if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {
    //            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
    //            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;
    //        }
    //        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
    //        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        for (var i = 0; i < $scope.FormulaArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
    //                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
    //            }
    //        }

    //        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
    //                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
    //            }
    //        }

    //    }
    //    else if (formula === 'Value') {
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;

    //        if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {
    //            $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
    //            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;
    //        }
    //        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
    //        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

    //        $scope.salaryRuleGeneral.FormulaIDDescription = null;
    //        $scope.salaryRuleGeneral.FormulaDescription = null;
    //        for (var i = 0; i < $scope.FormulaArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
    //                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
    //            }
    //        }

    //        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
    //            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
    //                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
    //            }
    //            else {
    //                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
    //            }
    //        }

    //    }
    //};

    //$scope.RemoveFormula = function () {
    //    $scope.salaryRuleGeneral.FormulaDesID = null;

    //    var count = $scope.FormulaArray.length;
    //    $scope.FormulaArray.splice(count - 1);

    //    //var count = $scope.FormulaIdArray.length;
    //    //$scope.FormulaIdArray.splice(count - 1);

    //    $scope.salaryRuleGeneral.FormulaDescription = null;
    //    $scope.salaryRuleGeneral.FormulaIDDescription = null;
    //    $scope.salaryRuleGeneral.FormulaDes = null;
    //    for (var i = 0; i < $scope.FormulaArray.length; i++) {
    //        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
    //            $scope.salaryRuleGeneral.FormulaDes = $scope.FormulaArray[i];
    //            $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];


    //        } else {
    //            $scope.salaryRuleGeneral.FormulaDes += $scope.FormulaArray[i];
    //            $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
    //        }
    //    }

    //    for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
    //        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
    //            $scope.salaryRuleGeneral.FormulaDesID = $scope.FormulaIdArray[i];
    //            $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];


    //        } else {
    //            $scope.salaryRuleGeneral.FormulaDesID += $scope.FormulaIdArray[i];
    //            $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
    //        }
    //    }
    //};
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];

    $scope.salaryRuleGeneral.FormulaDes = null;
    $scope.salaryRuleGeneral.FormulaDesID = null;
    $scope.salaryRuleGeneral.SalaryHeadFormula = null;
    $scope.salaryRuleGeneral.FormulaDescription = null;
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

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormula($scope.OperatorList, lastvalue)) {
                            $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                        }
                        else {
                            $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                            var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                        }
                    }
                    else {
                        $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                        var str = $scope.salaryRuleGeneral.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');

                        $scope.salaryRuleGeneral.FormulaDes = $scope.Formula;
                        $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }
                }

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
            else if (formula === 'DaysInaMonth') {
                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.DaysInaMonth)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.DaysInaMonth;
                    $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.DaysInaMonth;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDes)) {
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }


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

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    var lastvalue = $scope.FormulaArray[$scope.FormulaArray.length - 1];

                    if ($scope.checkFormula($scope.OperatorList, lastvalue) === false) {
                        $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
                        $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }

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


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
                    $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDes)) {
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

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
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {

                    $scope.salaryRuleGeneral.FormulaDescription = null;
                    $scope.salaryRuleGeneral.FormulaIDDescription = null;

                    $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
                    $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDes)) {
                        $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
                        $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);
                    }


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
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

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
    }

    ///=============================== Open dialog Formula=================================================
    $scope.OpendialogFormula = function () {
        try {
            $scope.salaryRuleGeneral.FormulaDes = null;
            $scope.salaryRuleGeneral.FormulaDesID = null;
            $scope.salaryRuleGeneral.SalaryHeadFormula = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.FormulaArray = [];
            $scope.FormulaIdArray = [];

            var eDialog = $("#dialogFormula").data("ejDialog");
            eDialog.open();
            //var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            //var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);




        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    ///============================Designation==================================================


    $scope.getAllowanceUrl = $scope.path + 'GetAllowanceDaily';
    $scope.getAllowanceRateUrl = $scope.path + 'GetDailyAllowanceRate';

    $scope.getShiftInfoUrl = $scope.path + 'GetEmployeeCategoryInfo';
    $scope.getDailyAllowanceUrl = $scope.path + 'GetDailyAllowance';
    $scope.SaveDailyAllowanceRateUrl = $scope.path + 'SaveDailyAllowanceRate';
    $scope.deleteDailyAllowancerateUrl = $scope.path + 'DeleteRate';







    $scope.DesignationModel = {
        DesignationType: 'all',
        IsFixed: true,
        Rate: null,
        DesignationCatagory: 'Fixed'
    };
    $scope.SelectedDesignationShow = false;
    $scope.SelectedDesignationShow = false;
    $scope.AllDesignationFormulaShow = null;
    $scope.DesignationTypeChange = function () {
        if ($scope.DesignationModel.DesignationType == 'Selected') {
            $scope.SelectedDesignationShow = true;
        } else {
            $scope.SelectedDesignationShow = false;
        }
    };

    $scope.DesignationCatagoryChange = function () {
        if ($scope.DesignationModel.DesignationCatagory == 'Fixed') {
            $scope.DesignationModel.IsFixed = true;
        } else {
            $scope.DesignationModel.IsFixed = false;
        }
    };




    $scope.DesignationIsFixedChange = function (args) {
        if ($scope.DesignationModel.DesignationType == 'Selected' && $scope.DesignationModel.IsFixed == true) {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

                if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true)
                    $scope.DailyAllowanceRateList[i].IsFixed = true;
            }


        } else {
            ////$scope.SelectedDesignationShow = false;
        }
        if ($scope.DesignationModel.DesignationType == 'Selected' && $scope.DesignationModel.IsFixed == false) {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

                if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true)
                    $scope.DailyAllowanceRateList[i].Rate = null;
            }
            $scope.DesignationModel.Rate = null;

        }
    };

    $scope.DesignationRateSet = function (args) {
        if ($scope.DesignationModel.DesignationType == 'Selected' && $scope.DesignationModel.IsFixed == true) {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

                if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true) {
                    $scope.DailyAllowanceRateList[i].IsFixed = true;
                    $scope.DailyAllowanceRateList[i].Rate = $scope.DesignationModel.Rate;
                    $scope.DailyAllowanceRateList[i].FormulaDescription = null;
                    $scope.DailyAllowanceRateList[i].FormulaDesID = null;
                }
            }


        } else {
            ////
        }
        if ($scope.DesignationModel.DesignationType == 'Selected' && $scope.DesignationModel.IsFixed == false) {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

                if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true) {
                    $scope.DailyAllowanceRateList[i].Rate = null;
                    $scope.DailyAllowanceRateList[i].IsFixed = false;
                }
            }
            $scope.DesignationModel.Rate = null;

        }
    };

    $scope.DesignationSetFormula = function () {
        if ($scope.DesignationModel.DesignationType == 'Selected') {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

                if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true) {
                    $scope.DailyAllowanceRateList[i].IsFixed = false;
                    $scope.DailyAllowanceRateList[i].Rate = null;
                    $scope.DailyAllowanceRateList[i].FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
                    $scope.DailyAllowanceRateList[i].FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;

                }
            }


        } else {
            $scope.AllDesignationFormulaShow = $scope.salaryRuleGeneral.FormulaDescription;
        }
        var gridObj = $("#GridDesignation").data("ejGrid");
        gridObj.refreshContent();
        var eDialog = $("#dialogFormula").data("ejDialog");
        eDialog.close();
    };
    $scope.LegalDesignationDetails = null;
    $scope.OpendialogLegalDesignationDetails = function () {
        try {

            $scope.LegalDesignationDetails = null;
            var eDialog = $("#dialogLegalDesignation").data("ejDialog");
            eDialog.open();
            var gridObj = $("#GridDesignation").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            //$("#dialogDesignation").ejDialog("setTitle", modeldata.UserName);
            $scope.LegalDesignationDetails = modeldata.LegalDesignation;



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.messageTitle = null;
    $scope.DailyAllowanceRateList = [];
    $scope.OpendialogDesignation = function () {
        try {


            var eDialog = $("#dialogDesignation").data("ejDialog");
            eDialog.open();
            $scope.GetDailyAllowanceRateList();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetDailyAllowanceRateList = function () {
        $scope.DesignationModel.Rate = null;
        $scope.AllDesignationFormulaShow = null;
        var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
        var modeldata = gridObj.getSelectedRecords()[0];
        $("#dialogDesignation").ejDialog("setTitle", modeldata.UserName + ' Designation wise');

        if (!baseService.isUndefinedOrNull(modeldata.IsAllDesignation)) {
            if (modeldata.IsAllDesignation) {
                $scope.DesignationModel.DesignationType = 'all';
                $scope.DesignationModel.Rate = modeldata.Rate;
                $scope.DesignationModel.IsFixed = modeldata.IsFixed;
                $scope.AllDesignationFormulaShow = modeldata.FormulaDescription;

                if (modeldata.IsFixed) {
                    $scope.DesignationModel.DesignationCatagory = 'Fixed';

                } else {
                    $scope.DesignationModel.DesignationCatagory = 'Formula';
                }


            } else {
                $scope.DesignationModel.DesignationType = 'Selected';
            }


            $scope.DesignationTypeChange();
            $scope.DesignationIsFixedChange();
        };


        try {
            $http.get($scope.getAllowanceRateUrl + '?DailyAllowanceId=' + modeldata.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.DailyAllowanceRateList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClosedialogDesignation = function () {
        try {
            var eDialog = $("#dialogDesignation").data("ejDialog");
            eDialog.close();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveDailyAllowanceRateData = function () {

        try {

            var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            $scope.DailyAllowanceType = modeldata.Id;
            if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
                throw "Enter Allowance.";
            };
            //for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {
            //    if ($scope.DailyAllowanceRateList[i].CheckBoxSelect === true) {
            //        if (baseService.isUndefinedOrNull($scope.DailyAllowanceRateList[i].Rate)) {
            //            throw "Enter Rate.";
            //        };

            //    }
            //}
            console.log('modeldata', modeldata);
            // console.log('DailyAllowanceType', $scope.DailyAllowanceType);

            if ($scope.DesignationModel.DesignationType == 'Selected') {
                $.ajax({
                    type: "POST",
                    url: $scope.SaveDailyAllowanceRateUrl,
                    data: { 'DailyAllowanceRateData': $scope.DailyAllowanceRateList, 'DailyAllowanceType': $scope.DailyAllowanceType },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            ShowResult(data.Message, "failure");
                        }
                        else {
                            ShowResult(data.Message, "success");
                            //$scope.getDailyAllowanceRate();
                            $scope.GetDailyAllowanceRateList();
                        }

                    }

                });
            } else {
                modeldata.IsAllDesignation = true;
                modeldata.IsFixed = $scope.DesignationModel.IsFixed;
                modeldata.Rate = $scope.DesignationModel.Rate;

                modeldata.FormulaDescription = $scope.salaryRuleGeneral.FormulaDescription;
                modeldata.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;

                if ($scope.DesignationModel.IsFixed) {
                    if (baseService.isUndefinedOrNull($scope.DesignationModel.Rate)) {
                        throw "Enter rate.";
                    };
                } else {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                        throw "Enter Formula.";
                    };
                }


                console.log('modeldata2', modeldata);

                $.ajax({
                    type: "POST",
                    url: $scope.path + 'UpdateMasterForDesignation',
                    data: { 'data': modeldata },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            ShowResult(data.Message, "failure");
                        }
                        else {
                            ShowResult(data.Message, "success");

                        }

                    }

                });
            }
            $scope.ClosedialogDesignation();
            $scope.GetDailyAllowanceList();



        } catch (e) {
            ShowResult(e, "failure");
        }
    };





    $scope.refreshTemplateemployee5 = function (args) {
        $("#headchk5").ejCheckBox({ "change": CheckBoxSelectAll5 });
    };

    function CheckBoxSelectAll5(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridDesignation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {
                $scope.DailyAllowanceRateList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridDesignation").data("ejGrid");
        gridObj.refreshContent();
    };













    //$scope.DailyAllowanceType = null;
    //$scope.AllowanceList = [];
    //$scope.getAllowance = function () {
    //    try {
    //        $http.get($scope.getAllowanceUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.AllowanceList = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    //$scope.getAllowance();



    //$scope.ShiftInfoList = [];
    //$scope.getShiftInfo = function () {
    //    try {
    //        $http.get($scope.getShiftInfoUrl)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.Message, 'failure');
    //                }
    //                else {
    //                    $scope.ShiftInfoList = response.data;
    //                }
    //            },

    //                function errorCallBack(response) {
    //                    ShowResult(response.Message, 'failure');
    //                });


    //    } catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //};
    //$scope.getShiftInfo();














    $scope.custompara = {};
    $scope.message_confirmation = null;
    $scope.removeRate = function (obj) {
        var gridObj = $("#GridDesignation").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.custompara = data.Id;
        if (!baseService.isUndefinedOrNull(data.Id)) {
            $scope.message_confirmation_designation = 'Are you sure to Reset This Rate ?';
            angular.element(document.querySelector('#confirmPopUpDesignation')).modal('show');
        }
    };

    $scope.DeleteRate = function () {

        $.ajax({
            type: "POST",
            url: $scope.deleteDailyAllowancerateUrl,
            data:
            {

                'Id': $scope.custompara
            },
            dataType: "json",
            success: function (response) {
                //$scope.ShowResult(data.Message, "success");
                ShowResult(response.Message, 'success');
                $scope.GetDailyAllowanceRateList();

            }

        });
    };
    /////=======================================shift=================================================================
    $scope.ShiftModel = {
        ShiftType: 'all',
        EffectiveTime: null,
        IsSpecificTime: true
    };
    $scope.SelectedShiftShow = false;
    $scope.ShiftTypeChange = function () {
        if ($scope.ShiftModel.ShiftType == 'Selected') {
            $scope.SelectedShiftShow = true;
        } else {
            $scope.SelectedShiftShow = false;
        }
    };


    $scope.ShowEffectiveTime = false;
    $scope.ShiftTimeChange = function (args) {
        if ($scope.ShiftModel.ShiftType == 'all' && $scope.ShiftModel.IsSpecificTime == true) {
            //for (var i = 0; i < $scope.DailyAllowanceRateList.length; i++) {

            //    if ($scope.ShiftInfoList[i].CheckBoxSelect === true) {
            //        $scope.ShiftInfoList[i].EffectiveTime = args.value;

            //    }
            //}

            $scope.ShowEffectiveTime = true;
        } else {
            $scope.ShowEffectiveTime = false;
        }

    };
    $scope.OpendialogShift = function () {
        try {


            var eDialog = $("#dialogShift").data("ejDialog");
            eDialog.open();
            $scope.GetShiftList();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.CloseddialogShift = function () {
        try {


            var eDialog = $("#dialogShift").data("ejDialog");
            eDialog.close();
            $scope.GetShiftList();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.getShiftInfoUrl = $scope.path + 'GetShiftInfo';
    $scope.SaveDailyAllowanceUrl = $scope.path + 'SaveDailyAllowance';
    $scope.deleteDailyAllowanceUrl = $scope.path + 'DeleteShift';

    $scope.GetShiftList = function () {
        try {
            $scope.ShiftModel.IsSpecificTime = false;
            $scope.ShiftModel.EffectiveTime = null;
            var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            $("#dialogShift").ejDialog("setTitle", modeldata.UserName + ' Shift wise');

            if (!baseService.isUndefinedOrNull(modeldata.IsAllShift)) {
                if (modeldata.IsAllShift) {
                    $scope.ShiftModel.ShiftType = 'all';
                    $scope.ShiftModel.IsSpecificTime = modeldata.IsSpecificTime;
                    $scope.ShiftModel.EffectiveTime = modeldata.EffectiveTime;
                    $scope.ShiftTimeChange();
                } else {
                    $scope.ShiftModel.ShiftType = 'Selected';
                }
                $scope.ShiftTypeChange();
            };

            try {

                $http.get($scope.getShiftInfoUrl + '?DailyAllowanceId=' + modeldata.Id)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.ShiftInfoList = response.data;
                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });


            } catch (e) {
                ShowResult(e, "failure");
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };




    $scope.DailyAllowanceType = null;



    $scope.ShiftInfoList = [];




    $scope.SaveShiftData = function () {

        try {
            var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            $scope.DailyAllowanceType = modeldata.Id;
            if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
                throw "Enter Allowance.";
            };
            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {
                if ($scope.ShiftInfoList[i].CheckBoxSelect === true) {

                    if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].EffectiveTime) && $scope.ShiftInfoList[i].IsSpecificTime === true) {
                        throw "Enter Effective Time.";
                    };
                    //if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].FromDate)) {
                    //    throw "Enter From Date.";
                    //};
                    //if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].ToDate)) {
                    //    throw "Enter To Date.";
                    //};
                }
            }

            if ($scope.ShiftModel.ShiftType == 'Selected') {
                $.ajax({
                    type: "POST",
                    url: $scope.SaveDailyAllowanceUrl,
                    data: { 'DailyAllowanceData': $scope.ShiftInfoList, 'DailyAllowanceType': $scope.DailyAllowanceType },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            ShowResult(data.Message, "failure");
                        }
                        else {
                            ShowResult(data.Message, "success");
                            //$scope.getDailyAllowance();
                            $scope.ShiftInfoList = [];
                            $scope.GetShiftList();
                        }

                    }

                });
            } else {

                modeldata.IsAllShift = true;
                modeldata.EffectiveTime = $scope.ShiftModel.EffectiveTime;
                modeldata.IsSpecificTime = $scope.ShiftModel.IsSpecificTime;
                if (baseService.isUndefinedOrNull($scope.ShiftModel.EffectiveTime) && $scope.ShiftModel.IsSpecificTime === true) {
                    throw "Enter Effective Time.";
                };

                $.ajax({
                    type: "POST",
                    url: $scope.path + 'UpdateMasterForShift',
                    data: { 'data': modeldata },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            ShowResult(data.Message, "failure");
                        }
                        else {
                            ShowResult(data.Message, "success");

                        }

                    }

                });
            }
            $scope.CloseddialogShift();
            $scope.GetDailyAllowanceList();




        } catch (e) {
            ShowResult(e, "failure");
        }
    };





    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        //console.log('ok');


        if (e.model.checkState === "check") {

            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {

                $scope.ShiftInfoList[i].CheckBoxSelect = true;
            }
        }
        else {
            //console.log('co-ok');
            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {

                $scope.ShiftInfoList[i].CheckBoxSelect = false;


            }
        }
        //var gridObj = $("#GridShiftInfo").data("ejGrid");
        //gridObj.refreshContent();
    };

    $scope.customparas = {};
    $scope.message_confirmation_shift = null;
    $scope.removeshift = function (obj) {
        var gridObj = $("#GridShiftInfo").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.custompara = data.Id;
        if (!baseService.isUndefinedOrNull(data.Id)) {
            $scope.message_confirmation_shift = 'Are you sure to Reset This Setting ?';
            angular.element(document.querySelector('#confirmPopUpShift')).modal('show');
        };

    };

    $scope.DeleteShift = function () {

        $.ajax({
            type: "POST",
            url: $scope.deleteDailyAllowanceUrl,
            data:
            {

                'Id': $scope.custompara
            },
            dataType: "json",
            success: function (response) {
                //$scope.ShowResult(data.Message, "success");
                ShowResult(response.Message, 'success');
                $scope.GetShiftList();

            }

        });
    };

    ///===========================================================SalaryRange=================================================================
    $scope.GetDailyAllowanceRateSalaryRangeUrl = $scope.path + 'GetDailyAllowanceRateBasedOnSalaryRange';
    $scope.SaveDailyAllowanceRateBasedOnSalaryRangeUrl = $scope.path + 'SaveDailyAllowanceRateBasedOnSalaryRange';
    $scope.DeleteRateBasedOnSalaryRangeUrl = $scope.path + 'DeleteRateBasedOnSalaryRange';
    $scope.SalaryRangeModel = {};
    $scope.SalaryRangeList = [];
    $scope.OpendialogSalaryRange = function () {
        try {


            var eDialog = $("#dialogSalaryRange").data("ejDialog");
            eDialog.open();
            $scope.GetSalaryRangeList();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.CloseddialogSalaryRange = function () {
        try {


            var eDialog = $("#dialogSalaryRange").data("ejDialog");
            eDialog.close();
            $scope.GetSalaryRangeList();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetSalaryRangeList = function () {

        var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
        var modeldata = gridObj.getSelectedRecords()[0];
        $("#dialogSalaryRange").ejDialog("setTitle", modeldata.UserName + ' Salary Range wise');




        try {
            $http.get($scope.GetDailyAllowanceRateSalaryRangeUrl + '?DailyAllowanceId=' + modeldata.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SalaryRangeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveDailyAllowanceRateBasedOnSalaryRange = function () {

        try {

            var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];
            $scope.DailyAllowanceType = modeldata.Id;
            if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
                throw "Enter Allowance.";
            };



            $.ajax({
                type: "POST",
                url: $scope.SaveDailyAllowanceRateBasedOnSalaryRangeUrl,
                data: { 'DailyAllowanceRateData': $scope.SalaryRangeModel, 'DailyAllowanceType': $scope.DailyAllowanceType },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.SalaryRangeModel = {};
                        $scope.GetSalaryRangeList();
                    }

                }

            });

            $scope.CloseddialogSalaryRange();



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteDailyAllowanceRateBasedOnSalaryRange = function () {

        $.ajax({
            type: "POST",
            url: $scope.DeleteRateBasedOnSalaryRangeUrl,
            data:
            {

                'Id': $scope.custompara
            },
            dataType: "json",
            success: function (response) {
                //$scope.ShowResult(data.Message, "success");
                ShowResult(response.Message, 'success');
                $scope.GetSalaryRangeList();

            }

        });
    };
    $scope.removeRateBasedOnSalaryRange = function (obj) {
        var gridObj = $("#GridSalaryRange").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.custompara = data.Id;
        if (!baseService.isUndefinedOrNull(data.Id)) {
            $scope.message_confirmation_designation = 'Are you sure to Reset This Rate ?';
            angular.element(document.querySelector('#confirmPopUpSalaryRange')).modal('show');
        }
    };

    //======================================================================
    //#region AdditionalPolicy

    $scope.AttendanceBP = {
        ID: null,
        FixedOrFormula: 'Fixed',
        FixedValue: 500,
        MaxEarlyOutAllowed: null,
        FormulaDes: null,
        FormulaDesID: null,
        DailyAllowanceId: null,

        IsLateInApplicable: false,
        IsEarlyOutApplicable: false,
        IsLunchOutApplicable: false,
        IsAbsentApplicable: false,
        IsLateApplicable: false,
        IsLeaveApplicable: false,
        IsLeaveWithOutPayApplicable: false,
        IsRouteApplicableForLate: false,

        EOLIFromValue: 0,
        EOLIToValue: 3,
        LunchOutFromValue: 0,
        LunchOutToValue: 0,
        AbsentFromValue: 0,
        AbsentToValue: 0,
        LateFromValue: 0,
        LateToValue: 3,
        LeaveFromValue: 0,
        LeaveToValue: 31,
        LeaveWithOutPayFromValue: 0,
        LeaveWithOutPayToValue: 0,
    };

    $scope.OpendialogAdditionalPolicy = function () {
        try {


            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.GetAdditionalPolicyList();
           


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetAdditionalPolicyList = function () {
        try {
            

            try {
                $scope.ClearAdditionalPolicy();
                var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
                var modeldata = gridObj.getSelectedRecords()[0];

                $http.get($scope.path + 'GetAdditionalPolicyList?DailyAllowanceId=' + modeldata.Id)
                    .then(function (response) {
                        if (!baseService.isUndefinedOrNull(response.data[0])) {
                            $scope.AttendanceBP = response.data[0];
                            $scope.AttendanceBP.DailyAllowanceId = modeldata.Id;
                            $scope.getLeaveTypeList();
                        }
                    });

               

            } catch (e) {
                ShowResult(e, "failure");
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    $scope.ClearAdditionalPolicy = function () {
        $scope.AttendanceBP = {
            ID: null,
            FixedOrFormula: 'Fixed',
            FixedValue: 500,
            MaxEarlyOutAllowed: null,
            FormulaDes: null,
            FormulaDesID: null,
            DailyAllowanceId: null,

            IsLateInApplicable: false,
            IsEarlyOutApplicable: false,
            IsLunchOutApplicable: false,
            IsAbsentApplicable: false,
            IsLateApplicable: false,
            IsLeaveApplicable: false,
            IsLeaveWithOutPayApplicable: false,
            IsRouteApplicableForLate: false,

            EOLIFromValue: 0,
            EOLIToValue: 3,
            LunchOutFromValue: 0,
            LunchOutToValue: 0,
            AbsentFromValue: 0,
            AbsentToValue: 0,
            LateFromValue: 0,
            LateToValue: 3,
            LeaveFromValue: 0,
            LeaveToValue: 31,
            LeaveWithOutPayFromValue: 0,
            LeaveWithOutPayToValue: 0,
        };
    }

  

   

    $scope.AttendanceBPModel = Object.assign({}, $scope.AttendanceBP);

    $scope.OpenAdditionalPolicyDialog = function () {
        $scope.getLeaveTypeList();
        var eDialog = $("#dialogPFSetting").data("ejDialog");
        eDialog.open();
        
    };

    $scope.dataList = [];
    $scope.getLeaveTypeList = function () {
        $scope.dataList = [];
        $http.get($scope.path + 'GetLeaveList?AttdnBonusPmtPolicyDetailsId=' + $scope.AttendanceBP.ID)
            .then(function (response) {
                $scope.dataList = response.data;
            });
    };


    $scope.SaveLeaveType = function () {
        try {
            var NewdataList = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelect == true) {
                    NewdataList.push($scope.dataList[i]);
                }
            }

            if (NewdataList.length == 0) {
                throw "Please Select LeaveType";
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveLeaveUrl,
                data: { 'LeaveList': NewdataList, 'MasterId': $scope.AttendanceBPMaster.MID, 'DetailsId': $scope.AttendanceBP.ID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DetailsId = null;
    $scope.SaveAdditionalPolicy = function () {
        try {
            var NewdataList = [];
            for (var i = 0; i < $scope.dataList.length; i++) {
                if ($scope.dataList[i].CheckBoxSelect == true) {
                    NewdataList.push($scope.dataList[i]);
                }
            }

            //if ($scope.AttendanceBP.FixedValue < 0) {
            //    throw 'Fixed Value Can not below then 0';
            //}

            //$scope.AttendanceBP.FormulaDes = $scope.salaryRuleGeneral.FormulaDescription;
            //$scope.AttendanceBP.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
            //$scope.AttendanceBP.MID = $scope.AttendanceBPMaster.MID;

            //if ($scope.AttendanceBP.FixedOrFormula == 'Fixed') {
            //    $scope.AttendanceBP.FormulaDesID = null;
            //    $scope.AttendanceBP.FormulaDes = null;
            //}

            var gridObj = $("#GridDailyAllowanceList").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];



            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'SaveDailyAllowanceAdditionalPolicy',
                data: { 'DailyAllowanceAdditionalPolicyData': $scope.AttendanceBP, 'LeaveList': NewdataList, 'DailyAllowanceId': modeldata.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.AttendanceBP.ID = response.data.DetailsId;
                    //$scope.Clear();
                    //$scope.getMaster();
                    //$scope.getDetails();
                    //$scope.ConfirmrebateClose();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ConfirmrebateClose = function () {
        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
        eDialog.close();
    };

     //#region
}