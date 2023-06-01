'use strict';
maternityBenefitController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function maternityBenefitController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Maternity-Benefit';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Payrolls/MaternityBenefit/';
    $scope.getSTListUrl = $scope.path + 'GetSeparationTypelist';
    $scope.getSTSCUrl = $scope.path + 'SeparationTypeSelectedChange';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getNoBenefitEmployeeListUrl = $scope.path + 'LoadNoBenefitEmployeelist';
    $scope.getPaidEmployeeListUrl = $scope.path + 'LoadPaidEmployeelist';
    $scope.getETListUrl = $scope.path + 'GetEmploymentTypelist';
    $scope.getDataForEditUrl = $scope.path + 'GetDataForEdit';
    $scope.saveUrl = $scope.path + 'SaveSeparationType';
    $scope.getListUrl = $scope.path + 'GetSeparationTypelist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    //#region Tab
    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    $scope.setTab33 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet33 = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    // #endregion Tab

    var date = new Date(), y = date.getFullYear(), m = date.getMonth() - 6;
    var firstDay = new Date(y, m, 1);
    $scope.master = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        Id: null,
        LeaveTransactionId: null,
        EmpSystemId: null,
        WageRate: 0,
        LeaveDays: 0,
        BeforeAmount: 0,
        AfterAmount: 0,
        BeforePaymentDate: null,
        AfterPaymentDate: null,
        IsPaidAfter: false,
        IsPaidBefore: false,
        AdditionalAmountBefore: 0,
        Deduction: 0,
        AdditionalAmount: 0,
        Remark: null,
        AdditionalAmountAfter: 0
    };

    $scope.employee = {
        EmpSystemId: null,
        LeaveTransactionId: null,
        LeaveStartDate: null,
        AdditionalAmount: 0,
        Deduction: 0,
        AdditionalAmountBefore: 0,
        TotalEarning:0,
        TotalWorkingDays: 26
    };
    $scope.EmployeeModel = Object.assign({}, $scope.employee);

    $scope.SeparationTypeList = [];
    $scope.getSeparationType = function () {
        try {
            $http.get($scope.getSTListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSeparationType();
    $scope.EmpSalaryInfoShow = false;
    $scope.EmpSalaryInfo = [];
    $scope.MonthlyGross = null;
    $scope.CalculateSalary = function () {
        try {
            var empid = $scope.EmployeeModel.EmpSystemId;
            var empLeaveId = $scope.EmployeeModel.LeaveTransactionId;
            $http.get($scope.path + 'CalculateSalary?empid=' + empid + '&empLeaveId=' + empLeaveId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryInfo = response.data;
                        
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.ShowInfo = function () {
        try {
            var empid = $scope.EmployeeModel.EmpSystemId;
            var empLeaveId = $scope.EmployeeModel.LeaveTransactionId;
            var LeaveStartDate = $scope.EmployeeModel.LeaveStartDate;
            $http.get($scope.path + 'ShowInfo?empid=' + empid + '&LeaveStartDate=' + LeaveStartDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryInfo = response.data;
                        $scope.MonthlyGross = response.data[0].Gross;
                        //CalculateRate($scope.EmpSalaryInfo);
                        $scope.EmpSalaryInfoShow = false;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Calculate = function () {
        $scope.EmpSalaryInfo.WorkingDays = $scope.EmployeeModel.TotalWorkingDays

        if (!angular.isUndefinedOrNull($scope.EmployeeModel.TotalWorkingDays)) {
            CalculateR($scope.EmpSalaryInfo);
            $scope.EmpSalaryInfoShow = true;
        }

    }
    function CalculateR(list) {
        try {
            var totalDays = 0;
            var totalAmount = 0;
            for (var i = 0; i < list.length; i++) {
                totalDays = $scope.EmpSalaryInfo.WorkingDays;
                totalAmount += list[i].TotalEarnedAmount;
            }

            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmount)) {
                $scope.EmployeeModel.AdditionalAmount = 0;
            }
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.Deduction)) {
                $scope.EmployeeModel.Deduction = 0;
            }
            //parseFloat(v).toFixed(2)
            totalAmount += parseFloat($scope.EmployeeModel.AdditionalAmount);
            totalAmount -= parseFloat($scope.EmployeeModel.Deduction);

            $scope.EmployeeModel.TotalEarning = totalAmount.toFixed(2);
            
            //$scope.EmployeeModel.TotalWorkingDays = totalDays;
            var wrate = totalAmount / totalDays;
            $scope.EmployeeModel.WageRate = wrate.toFixed(2);//AdditionalAmountAfter          

            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountBefore)) {
                $scope.EmployeeModel.AdditionalAmountBefore = 0;
            }
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountAfter)) {
                $scope.EmployeeModel.AdditionalAmountAfter = 0;
            }
            $scope.EmployeeModel.TotalPayable = (wrate * $scope.EmployeeModel.LeaveDays).toFixed(2);
            $scope.EmployeeModel.BeforeAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.BeforePercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountBefore)).toFixed(2);
            $scope.EmployeeModel.AfterAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.AfterPercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountAfter)).toFixed(2);

        } catch (e) {
            throw e;
        }
    }
    
    $scope.CalculateRateUI = function () {
        //$scope.EmpSalaryInfo.WorkingDays = $scope.EmployeeModel.TotalWorkingDays
        CalculateRate($scope.EmpSalaryInfo);
        $scope.EmpSalaryInfoShow = true;
    }

    $scope.UpdateBeforeAmount = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountBefore)) {
                $scope.EmployeeModel.AdditionalAmountBefore = 0;
            }

            $scope.EmployeeModel.BeforeAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.BeforePercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountBefore)).toFixed(2);

        } catch (e) {
            throw e;
        }
    }
    $scope.UpdateAfterAmount = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountAfter)) {
                $scope.EmployeeModel.AdditionalAmountAfter = 0;
            }

            $scope.EmployeeModel.AfterAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.AfterPercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountAfter)).toFixed(2);
        } catch (e) {
            throw e;
        }
    }
    $scope.UpdateTotalEarnedAmount = function (obj) {
        try {

            // var _gridObj = $("#GridSalaryInfo").data("ejGrid");
            $scope.data = obj.data;
            //$scope.data = _gridObj.getSelectedRecords()[0];
            //var data = obj.data;
            $scope.data.TotalEarnedAmount = $scope.data.EarnedAmount + $scope.data.BonusAmount + $scope.data.OtherEarning;
            //var gridObj2 = $("#GridSalaryInfo").data("ejGrid");
            //gridObj2.refreshContent(true);
        } catch (e) {
            throw e;
        }
    }


    function CalculateRate(list) {
        try {
            var totalDays = 0;
            var totalAmount = 0;
            for (var i = 0; i < list.length; i++) {
                totalDays += list[i].WorkingDays;
                totalAmount += list[i].TotalEarnedAmount;
            }

            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmount)) {
                $scope.EmployeeModel.AdditionalAmount = 0;
            }
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.Deduction)) {
                $scope.EmployeeModel.Deduction = 0;
            }
            //parseFloat(v).toFixed(2)
            totalAmount += parseFloat($scope.EmployeeModel.AdditionalAmount);
            totalAmount -= parseFloat($scope.EmployeeModel.Deduction);

            $scope.EmployeeModel.TotalEarning = totalAmount.toFixed(2);
            //$scope.EmployeeModel.TotalWorkingDays = totalDays;
            var wrate = totalAmount / totalDays;
            $scope.EmployeeModel.WageRate = wrate.toFixed(2);//AdditionalAmountAfter          

            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountBefore)) {
                $scope.EmployeeModel.AdditionalAmountBefore = 0;
            }
            if (baseService.isUndefinedOrNull($scope.EmployeeModel.AdditionalAmountAfter)) {
                $scope.EmployeeModel.AdditionalAmountAfter = 0;
            }

            $scope.EmployeeModel.TotalPayable = (wrate * $scope.EmployeeModel.LeaveDays).toFixed(2);
            $scope.EmployeeModel.BeforeAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.BeforePercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountBefore)).toFixed(2);
            $scope.EmployeeModel.AfterAmount = (($scope.EmployeeModel.TotalPayable * $scope.EmployeeModel.AfterPercentage / 100) + parseFloat($scope.EmployeeModel.AdditionalAmountAfter)).toFixed(2);

        } catch (e) {
            throw e;
        }
    }
    

    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();

            $http.get('Payrolls/MaternityBenefit/LoadEmployeelist?FromDate=' + $scope.master.FromDate + '&ToDate=' + $scope.master.ToDate)
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.EmployeeInformationList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.NoBenefitEmployeeInformationList = [];
    $scope.LoadNoBenefitEmployeeList = function () {
        try {

            $http.get($scope.getNoBenefitEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.NoBenefitEmployeeInformationList = response.data;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadNoBenefitEmployeeList();

    $scope.PaiedEmployeeInformationList = [];
    $scope.LoadPaiedEmployeeList = function () {
        try {

            $http.get($scope.getPaidEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.PaiedEmployeeInformationList = response.data;
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadPaiedEmployeeList();


    $scope.EmployeeModel = {};
    $scope.SelectEmployee = function () {
        try {
            var gridObj = $("#GridEmployeeInfoListCalculateBenefit").data("ejGrid");
            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];
            $scope.ShowInfo();
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

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

    $scope.TempModel = {
        Id: null,
        YearNo: 0,
        DayNo: 0,
        RoundUp: false,
        EmploymentType: null
    };
    $scope.SeparationTypeDetails = [];
    $scope.CreateTempList = function () {
        $scope.SeparationTypeDetails = [];
        for (var i = 0; i < 30; i++) {
            $scope.TempModel = {};
            $scope.TempModel.Id = i + 1;
            $scope.TempModel.YearNo = i + 1;
            $scope.TempModel.DayNo = 0;
            $scope.TempModel.RoundUp = false;
            $scope.SeparationTypeDetails.push($scope.TempModel);
        }


    };
    $scope.CreateTempList();


    $scope.SeparationType = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        FormulaDes: null,
        FormulaDesID: null,
        IsGratuityApplicable: false,
        IsActive: true,
        AddedBy: null,
        AddedDate: new Date('dd-MMM-yyyy'),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.SeparationTypesList = [];
    $scope.getSeparationTypesList = function () {
        try {
            $http.get($scope.getListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationTypesList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSeparationTypesList();



    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.SeparationType.Sequence = response.data[0].Sequence;
            });
    };

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/SeparationType/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    function GetMasterValue() {
        try {
            $scope.master.Id = '';
            for (var fn in $scope.master) {
                if (fn !== 'Id') {
                    $scope.master[fn] = $scope.EmployeeModel[fn];
                }
            }
            console.log('master', $scope.master);
        } catch (e) {
            throw e;
        }
    }
    function GetDetailValue() {
        try {
            $scope.detail.Id = '';
            for (var fn in $scope.detail) {
                if (fn !== 'Id') {
                    $scope.detail[fn] = $scope.EmployeeModel[fn];
                }
            }
            console.log('master', $scope.EmpSalaryInfo);
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            if ($scope.EmployeeModel.TotalWorkingDays < 0) {
                throw 'Total Working Days can not be less then 0';
            }

            if (angular.isUndefinedOrNull($scope.EmployeeModel.TotalWorkingDays)) {
                throw 'Please Define Total Working Days';
            }

            if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SystemId)) {
                $scope.master.EmpSystemID = $scope.EmpSalaryInfo.SystemId;
            }

            for (var i = 0; i < $scope.EmpSalaryInfo.length; i++) {
                if ($scope.EmpSalaryInfo[i].OtherAmount < 0) {
                    throw 'Other Amount can not be negative value';
                }
            }
            if ($scope.EmployeeModel.AdditionalAmount < 0) {
                throw 'Additional Amount can not be negative value';
            }
            GetMasterValue();
            $http({
                method: 'POST',
                url: $scope.path + 'Save',
                data: { 'master': $scope.master, 'detail': $scope.EmpSalaryInfo, 'TotalWorkingDays': $scope.EmployeeModel.TotalWorkingDays, 'TotalPayable': $scope.EmployeeModel.TotalPayable},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                   // $scope.Clear();
                    $scope.master = {
                        FromDate: $filter('dateFiltering')(firstDay),
                        ToDate: $filter('dateFiltering')(Date.now()),   
                        Id: null,
                        LeaveTransactionId: null,
                        EmpSystemId: null,
                        WageRate: 0,
                        LeaveDays: 0,
                        BeforeAmount: 0,
                        AfterAmount: 0,
                        BeforePaymentDate: null,
                        AfterPaymentDate: null,
                        IsPaidAfter: false,
                        IsPaidBefore: false,
                        AdditionalAmountBefore: 0,
                        Deduction: 0,
                        AdditionalAmount: 0,
                        Remark: null,
                        AdditionalAmountAfter: 0
                    };
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });

        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SeparationTypeNew = {};
    $scope.getDataForEdit = function (Id) {
        try {
            $http.get($scope.getDataForEditUrl + '?Id=' + Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationType = response.data.SeparationType[0];
                        $scope.SeparationTypeDetails = response.data.SeparationTypeDetails;

                        $scope.salaryRuleGeneral.FormulaDescription = $scope.SeparationType.FormulaDes;
                        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.SeparationType.FormulaDesID;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EditSeparationType = function (obj) {
        var gridObj = $("#GridSeparationTypesList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.SeparationTypeNew = data;
        $scope.getDataForEdit(data.Id);
        //$scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);
        //$scope.getSH();
        //$scope.getAutoSequence($scope.salaryRuleNew.SystemID);
        $scope.Action = 'Update';
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
        // $scope.getSalaryRuleESIC();
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SeparationType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SeparationType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SeparationTypes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.SelectEmployee();
        $scope.ShowInfo();

    }
};