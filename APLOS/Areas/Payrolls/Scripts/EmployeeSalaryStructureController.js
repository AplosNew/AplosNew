'use strict';
EmployeeSalaryStructureController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSalaryStructureController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Salary Structure';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    //$scope.path = 'Payrolls/EmployeeSalaryRuleSetup/';

    $scope.path = 'humanresource/employeepromotionNew/';
    $scope.getApprovedEmpListUrl = $scope.path + 'GetSalaryStrcApprovedEmployeeList';
    $scope.getUnApprovedEmpListUrl = $scope.path + 'GetSalaryStrcUnApprovedEmployeeList';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.model2 = { Adjustment: false, Promotion: false, Increment: false }

    $scope.SetCheckforAdjustment = function () {
        if ($scope.model2.Adjustment === true) {
            $scope.model2.Promotion = false;
            $scope.model2.Increment = false;
        }
        if ($scope.model2.Promotion === true) {
            $scope.model2.Adjustment === false;
        }
        if ($scope.model2.Increment === true) {
            $scope.model2.Adjustment === false;
        }
    }
    $scope.employeeList = [];
    $scope.LoadEmployeeDataForGrid = function () {
        try {
            $http.get($scope.getUnApprovedEmpListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employeeList = [];
                        $scope.employeeList = response.data;
                        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');

                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };

    $scope.LoadEmployeeData = function (IsApprovedEmpList) {
        $scope.ResetPFSettingModel();
        if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {//--- for Confirmation


            $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')($scope.DOC, 'dd-M-yyyy');
            $scope.getEmpDataById($scope.qempid);

            $scope.getEmpSalaryInfoDefineData($scope.qempid);
            $scope.divFreshEntry = false;
            $scope.Action = 'Update';
            $scope.EntryShow();

        }
        else { //--- for increment
            $scope.Clear();
            if (IsApprovedEmpList) {
                //baseService.init($scope.getApprovedEmpListUrl, null, 10, null, 'EmployeeCode', 'EmployeeCode');
                $scope.LoadEmployeeDataForGrid($scope.getApprovedEmpListUrl);
                $scope.EmpSalaryInfo.IsFreshEntry = false;
                $scope.ShowEmpHeader = null;
                $scope.ShowEmpHeader = 'Increment/Promotion/Adjustment';

            }
            else {
                //baseService.init($scope.getUnApprovedEmpListUrl, null, 10, null, 'EmployeeCode', 'EmployeeCode');
                $scope.LoadEmployeeDataForGrid($scope.getUnApprovedEmpListUrl);
                $scope.model2.Increment = true;
                $scope.EmpSalaryInfo.IsFreshEntry = true;
                $scope.ShowEmpHeader = null;
                $scope.ShowEmpHeader = 'Fresh Entry';

            }

            // $scope.getData();
            $scope.ShowHideModel();
        }


    };


    $scope.EmpSalaryOpenHeadCurrent = [];
    $scope.salaryDataList = [];
    $scope.EmpSalaryInfo = {};

    $scope.Get = function (data) {
        try {
            $http.get('Payrolls/EmployeeSalaryRuleSetup/GetEmployeeSalaryData?empId=' + data.rowData.SystemId + '&designationId=' + data.rowData.GivenDesignationId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.Action = "Update";
                        $scope.EmpSalaryInfo = response.data.salaryItem[0];
                        $scope.salaryDataList = response.data.salaryData;
                        if (baseService.arrayLength($scope.salaryDataList) == 0) {
                            $scope.Action = "Save";
                            $scope.EmpSalaryOpenHeadCurrent = response.data.salaryItem;
                        }
                        else {
                            $scope.GetSalaryInfo($scope.EmpSalaryInfo);
                        }
                        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSalaryInfo = function (data) {
        try {
            $scope.EmpSalaryInfo.SystemID = data.SystemID;
            $scope.EmpSalaryInfo.EmpInfoSystemID = data.EmpInfoSystemID;
            $scope.EmpSalaryInfo.PlantId = data.PlantId;
            $scope.EmpSalaryInfo.GroupID = data.GroupID;
            $scope.EmpSalaryInfo.EffectiveDate = data.EffectiveDate;
            $scope.EmpSalaryInfo.NextDueDate = data.NextDueDate;
            $scope.EmpSalaryInfo.IsApproved = data.IsApproved;
            $scope.EmpSalaryInfo.EmployeeSalaryRuleSetupId = data.EmployeeSalaryRuleSetupId;

            $http.get('Payrolls/EmployeeSalaryRuleSetup/GetSalaryInfoData?SalaryID=' + data.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryOpenHeadCurrent = response.data;
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetSalaryInfoData = function (data) {
        try {
            $scope.EmpSalaryInfo.SystemID = data.rowData.SystemID;
            $scope.EmpSalaryInfo.EmpInfoSystemID = data.rowData.EmpInfoSystemID;
            $scope.EmpSalaryInfo.PlantId = data.rowData.PlantId;
            $scope.EmpSalaryInfo.GroupID = data.rowData.GroupID;
            $scope.EmpSalaryInfo.EffectiveDate = data.rowData.EffectiveDate;
            $scope.EmpSalaryInfo.NextDueDate = data.rowData.NextDueDate;
            $scope.EmpSalaryInfo.IsApproved = data.rowData.IsApproved;
            $scope.EmpSalaryInfo.EmployeeSalaryRuleSetupId = data.rowData.EmployeeSalaryRuleSetupId;

            $http.get('Payrolls/EmployeeSalaryRuleSetup/GetSalaryInfoData?SalaryID=' + data.rowData.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmpSalaryOpenHeadCurrent = response.data;
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Save = function () {
        try {
            $scope.IncrementHistory = {};
            $scope.EmpSalaryInfoNew = {};
            if (baseService.arrayLength($scope.EmpSalaryOpenHeadCurrent) < 0) {
                throw "Define Designation in Salary Rule Setup.";
            }
            $scope.EmpSalaryInfoNew.SystemID = $scope.EmpSalaryInfo.SystemID == null ? null : $scope.EmpSalaryInfo.SystemID;
            $scope.EmpSalaryInfoNew.EmpInfoSystemID = $scope.EmpSalaryInfo.EmpInfoSystemID;
            $scope.EmpSalaryInfoNew.PlantId = $scope.EmpSalaryInfo.PlantId;
            $scope.EmpSalaryInfoNew.GroupID = $scope.EmpSalaryInfo.CompanyGroupId;
            $scope.EmpSalaryInfoNew.EffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;
            $scope.EmpSalaryInfoNew.NextDueDate = $scope.EmpSalaryInfo.NextDueDate;
            $scope.EmpSalaryInfoNew.IsApproved = $scope.EmpSalaryInfo.IsApproved;
            $scope.EmpSalaryInfoNew.EmployeeSalaryRuleSetupId = $scope.EmpSalaryInfo.EmployeeSalaryRuleSetupId;
            $scope.EmpSalaryInfoNew.IncrementHistoryId = $scope.EmpSalaryInfo.IncrementHistoryId;
            $scope.newList = [];
            var ob = {};
            for (var i = 0; i < $scope.EmpSalaryOpenHeadCurrent.length; i++) {
                ob.SystemID = $scope.EmpSalaryOpenHeadCurrent[i].SystemID == null ? null : $scope.EmpSalaryOpenHeadCurrent[i].SystemID;
                ob.SalaryID = $scope.EmpSalaryOpenHeadCurrent[i].SalaryID == null ? null : $scope.EmpSalaryOpenHeadCurrent[i].SalaryID;;
                ob.SalaryHeadID = $scope.EmpSalaryOpenHeadCurrent[i].SalaryHeadID;
                ob.EntryCurrencyID = null;
                ob.EntryAmount = $scope.EmpSalaryOpenHeadCurrent[i].Amount;
                ob.DefineCurrencyID = null;
                ob.DefineAmount = $scope.EmpSalaryOpenHeadCurrent[i].Amount;
                ob.AmtDefinitionCurrencyID = null;
                ob.AmtDefinitionRate = null;
                ob.SalaryCategory = null;
                ob.SequenceNo = $scope.EmpSalaryOpenHeadCurrent[i].Sequence;

                $scope.newList.push(ob);
                ob = {};
            }

            $scope.IncrementHistory.SystemID = $scope.EmpSalaryInfo.IncrementHistoryId == null ? null : $scope.EmpSalaryInfo.IncrementHistoryId;
            $scope.IncrementHistory.EmpSystemID = $scope.EmpSalaryInfo.EmpInfoSystemID;
            $scope.IncrementHistory.FromGivenDesignationId = $scope.EmpSalaryInfo.GivenDesignationId;
            $scope.IncrementHistory.FromBudgetCode = $scope.EmpSalaryInfo.BudgetCode;
            $scope.IncrementHistory.FromLegalDesignationId = $scope.EmpSalaryInfo.LegalDesignationId;
            $scope.IncrementHistory.ToGivenDesignationId = $scope.EmpSalaryInfo.GivenDesignationId;
            $scope.IncrementHistory.ToBudgetCode = $scope.EmpSalaryInfo.BudgetCode;
            $scope.IncrementHistory.ToLegalDesignationId = $scope.EmpSalaryInfo.LegalDesignationId;
            $scope.IncrementHistory.ToEffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;
            $scope.IncrementHistory.IncrementType = "Fresh Entry";

            $http({
                method: 'POST',
                url: 'Payrolls/EmployeeSalaryRuleSetup/CreateSalary',
                data: { 'master': $scope.EmpSalaryInfoNew, 'data': $scope.newList, 'IncrementHistory': $scope.IncrementHistory },
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
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.EmpSalaryOpenHeadCurrent = [];
        $scope.EmpSalaryInfo = {};
        $scope.salaryDataList = {};
    }

    // #region Update
    $scope.saveData = function () {
        try {
            // #region validation
            if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.EffectiveDate)) {
                throw "Enter valid Effective Date.";
            } else {
                $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryInfo.EffectiveDate, 'dd-M-yyyy');
            }

            if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.NextDueDate)) {
                throw "Enter valid Next Due Date.";
            }
            else {
                $scope.EmpSalaryInfo.NextDueDate = $filter('dateFiltering')($scope.EmpSalaryInfo.NextDueDate, 'dd-M-yyyy');
            }

            // #endregion
            if ($scope.model2.Adjustment === false) {
                if ($scope.model2.Promotion === true && $scope.model2.Increment === false) {
                    $scope.IncrementHistory.IsPromotion = true;
                }
            }


            //with Confirmation
            if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {

                if ($scope.model2.Promotion === true) {
                    $scope.IncrementHistory.IncrementType = "Confirmation with Promotion";
                    $scope.IncrementHistory.IsConfirmation = true;
                    $scope.Update();
                }
                if ($scope.model2.Increment === true) {

                    if ($scope.model2.Promotion === true) {
                        $scope.IncrementHistory.IncrementType = "Confirmation with Increment and Promotion";
                        $scope.IncrementHistory.IsConfirmation = true;

                    } else {
                        $scope.IncrementHistory.IncrementType = "Confirmation with Increment";
                        $scope.IncrementHistory.IsConfirmation = true;
                    }


                    if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                        throw "Enter valid Salary Rule Master.";
                    }
                    if ($scope.Calculated === false) {
                        throw "Calculate Salary.";
                    } else {

                        $scope.UpdateSalaryStracture();

                    }

                }

                if ($scope.model2.Adjustment === true) {
                    $scope.IncrementHistory.IncrementType = "Confirmation with Adjustment";
                    $scope.IncrementHistory.IsConfirmation = true;

                    if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                        throw "Enter valid Salary Rule Master.";
                    }
                    if ($scope.Calculated === false) {
                        throw "Calculate Salary.";
                    } else {
                        $scope.UpdateSalaryStracture();
                    }
                }
            }//only Increment or Promotion
            else {
                if ($scope.budgetCodeChangeNew.IsPending === 1) {

                    if ($scope.model2.Promotion === true) {
                        $scope.IncrementHistory.IncrementType = "Confirmation with Promotion";
                        $scope.IncrementHistory.IsConfirmation = true;
                        $scope.Update();
                    }
                    if ($scope.model2.Increment === true) {

                        if ($scope.model2.Promotion === true) {
                            $scope.IncrementHistory.IncrementType = "Confirmation with Increment and Promotion";
                            $scope.IncrementHistory.IsConfirmation = true;

                        } else {
                            $scope.IncrementHistory.IncrementType = "Confirmation with Increment";
                            $scope.IncrementHistory.IsConfirmation = true;
                        }


                        if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                            throw "Enter valid Salary Rule Master.";
                        }
                        if ($scope.Calculated === false) {
                            throw "Calculate Salary.";
                        } else {

                            $scope.UpdateSalaryStracture();

                        }

                    }

                    if ($scope.model2.Adjustment === true) {
                        $scope.IncrementHistory.IncrementType = "Confirmation with Adjustment";
                        $scope.IncrementHistory.IsConfirmation = true;

                        if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                            throw "Enter valid Salary Rule Master.";
                        }
                        if ($scope.Calculated === false) {
                            throw "Calculate Salary.";
                        } else {
                            $scope.UpdateSalaryStracture();
                        }
                    }

                } else {

                    if ($scope.model2.Promotion === true) {
                        $scope.IncrementHistory.IncrementType = "Promotion";
                        $scope.Update();
                    }
                    if ($scope.model2.Increment === true) {

                        if ($scope.model2.Promotion === true) {
                            $scope.IncrementHistory.IncrementType = "Increment and Promotion";

                        } else {
                            $scope.IncrementHistory.IncrementType = "Increment";
                        }


                        if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                            throw "Enter valid Salary Rule Master.";
                        }
                        if ($scope.Calculated === false) {
                            throw "Calculate Salary.";
                        } else {

                            $scope.UpdateSalaryStracture();

                        }

                    }

                    if ($scope.model2.Adjustment === true) {
                        $scope.IncrementHistory.IncrementType = "Confirmation with Adjustment";
                        $scope.IncrementHistory.IsConfirmation = true;

                        if (baseService.isUndefinedOrNull($scope.EmpSalaryInfo.SalaryRuleMasterSystemID)) {
                            throw "Enter valid Salary Rule Master.";
                        }
                        if ($scope.Calculated === false) {
                            throw "Calculate Salary.";
                        } else {
                            $scope.UpdateSalaryStracture();
                        }
                    }
                }

            }
            //$scope.ResetPFSettingModel();   ///  commented by mizan on 01-Nov-2021 not to reset



        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };



    $scope.Update = function () {
        try {

            $scope.IncrementHistory.EmpSystemID = $scope.budgetCodeChangeOld.SystemId;
            $scope.IncrementHistory.FromGivenDesignationId = $scope.budgetCodeChangeOld.GivenDesignationId;
            $scope.IncrementHistory.FromBudgetCode = $scope.budgetCodeChangeOld.BudgetCode;
            $scope.IncrementHistory.FromLegalDesignationId = $scope.budgetCodeChangeOld.LegalDesignationId;
            $scope.IncrementHistory.ToGivenDesignationId = $scope.budgetCodeChangeNew.GivenDesignationId;
            $scope.IncrementHistory.ToBudgetCode = $scope.budgetCodeChangeNew.BudgetCode;
            $scope.IncrementHistory.ToLegalDesignationId = $scope.budgetCodeChangeNew.LegalDesignationId;
            $scope.IncrementHistory.ToEffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;

            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'IncrementHistory': $scope.IncrementHistory },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, "failure");
                    }
                    else {
                        $scope.ShowResultCustom(response.data.Message, "success");
                        $scope.Clear();


                        $scope.NewbudgetCodeChange = {};
                    }
                }, function errorCallback(response) {
                    $scope.ShowResultCustom(response.data.Message, "failure");
                });
                return true;
            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };

    $scope.UpdateSalaryStracture = function () {
        try {

            if ($scope.Action === "Update") {
               
                $.ajax({
                    type: "POST",
                    url: $scope.UpdateSalaryStractureUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'EmpSalaryInfo': $scope.EmpSalaryInfo, 'EmpSalaryInfoDefineNew': $scope.EmpSalaryInfoDefineNew, 'IncrementHistory': $scope.IncrementHistory, 'AdditionalPolicySettingModel': $scope.SettingModel },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            $scope.ShowResultCustom(data.Message, "failure");
                        }
                        else {
                            $scope.ShowResultCustom(data.Message, "success");
                        }

                    }

                });


            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };
    
  

    // #endregion
}