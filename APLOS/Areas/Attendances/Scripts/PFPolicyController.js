'use strict';
PFPolicyController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function PFPolicyController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'PF Policy';
    $scope.path = 'Attendances/PFPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveDetailsUrl = $scope.path + 'SaveDetails';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'Delete';
    $scope.Action = 'Save';
    $scope.EmployerList = [];
    $scope.ModelList = [];
    $scope.EmployeeList = [];
    $scope.HeadList = [];
    $scope.getData = function () {
        $scope.ModelList = [];
        $http({
            method: 'POST',
            url: $scope.getListUrl,
            data: { PlantId: $scope.PFPolicyMaster.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }

    $scope.PFPolicyDetailsList = [];
    $scope.getDetails = function () {
        $http.get('Attendances/PFPolicy/GetDetailsList?MasterId=' + $scope.PFPolicyMaster.ID)
            .then(function (response) {
                $scope.PFPolicyDetailsList = response.data;
            });
    };

    $scope.PFPolicyMaster = {
        ID: null,
        PFPolicyName: null,
        PFPolicyDescription: null,
        Eligibility: null,
        EligibilityBaseOn: 'DAY',
        EligibilityTimeLenght: null,
        MaturityBaseOn: 'MONTH',
        IsAllEmpApplocable: false,
        PlantID: null,
        GroupID: null,
        EligibilityTimeLenghtDay: 0,
        EligibilityTimeLenghtMonth: 0,
        MaturityTimeLenghtMonth: 0,
        MaturityTimeLenghtYear: 0,
    };
    $scope.PFPolicyMasterModel = Object.assign({}, $scope.PFPolicyMaster);

    $scope.PFPolicyDetailsMaster = {
        ID: null,
        PFPolicyMasterID: null,
        EarningValueRangeFrom: null,
        EarningValueRangeTo: null,
        IsMandatory: false,


        IsFixedEmp: false,
        IsFixedEmployer: false,


        FixedValueEmp: 0,
        IsFormulaEmp: false,
        IsContributionSlrHDdependOnEarningEmp: false,
        IsDistributionEmp: false,
        FixedValueEmployer: 0,
        IsFormulaEmployer: false,
        IsContributionSlrHDdependOnEarningEmployer: false,
        FormulaDesEmployer: null,
        IsDistributionEmployer: false,
        EmpCntValPer: 0,
        EmployerCntValPer: 0,
        EmpVolunValPer: 0,
        IsVoluntaryPF: false,
        IsNotEntGetEmplrAlwn: false,
        IsIndividualAlwn: false,
        AlwnSlrHd: null,
        IsAgeLimit: false,
        AgeLimit: 0,

        //EmpFixedValue: 'FixedValue',
        //EmployerFixedValue: 'FixedValue',

        FormulaDesIDEarning: null,
        FormulaDesEarning: null,
        SalaryHeadIDEarning: null,
        FormulaDesEmp: null,
        FormulaDesIDEmp: null,
        SalaryHeadIDEmp: null,
        FormulaDesIDEmployer: null,
        SalaryHeadIDEmployer: null,
        FormulaDesIDEmployerDis: null,
        FormulaDesIDEmpDis: null,

        EmployeeID: null,
        EmployeePFPolicyDetailsID: null,
        EmployeeValue: null,
        EmployeeSalaryHeadID: null,
        EmployeeUpperLimit: null,
        EmployeeResidualValueSlrHdID: null,

        EmployerID: null,
        EmployerPFPolicyDetailsID: null,
        EmployerValue: null,
        EmployerSalaryHeadID: null,
        EmployerUpperLimit: null,
        EmployerResidualValueSlrHdID: null,
        EmployeeID: null,
        EmployeerID: null,

    };
    $scope.PFPolicyDetailsMasterModel = Object.assign({}, $scope.PFPolicyDetailsMaster);

    $scope.MasterId = null;
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $http({
                method: 'POST',
                url: $scope.saveMUrl,
                data: { 'Master': $scope.PFPolicyMaster, 'PFPolicySalaryHeadList': $scope.HeadList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.PFPolicyMaster.ID = response.data.MasterId;
                    $scope.PFPolicyDetailsMaster.ID = response.data.MasterId;
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveDetails = function () {
        try {
            if ($scope.PFPolicyDetailsMaster.IsFixedEmp == 'FixedValue' || $scope.PFPolicyDetailsMaster.IsFixedEmp == true) {
                $scope.PFPolicyDetailsMaster.IsFixedEmp = true;
                $scope.PFPolicyDetailsMaster.IsFormulaEmp = false;
            }
            else {
                $scope.PFPolicyDetailsMaster.IsFormulaEmp = true;
            }
            if ($scope.PFPolicyDetailsMaster.IsFixedEmployer == 'FixedValue' || $scope.PFPolicyDetailsMaster.IsFixedEmployer == true) {
                $scope.PFPolicyDetailsMaster.IsFixedEmployer = true;
                $scope.PFPolicyDetailsMaster.IsFormulaEmployer = false;
            }
            else {
                $scope.PFPolicyDetailsMaster.IsFormulaEmployer = true;
            }
            $scope.PFPolicyDetailsMaster.ID = $scope.PFPolicyDetailsMaster.ID;
            $scope.PFPolicyDetailsMaster.PFPolicyMasterID = $scope.PFPolicyMaster.PFPolicyMasterID;
            $scope.PFPolicyDetailsMaster.FormulaDesEarning = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.PFPolicyDetailsMaster.FormulaDesIDEarning = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.PFPolicyDetailsMaster.SalaryHeadIDEarning = $scope.salaryRuleGeneral.FormulaIDDescription;

            $scope.PFPolicyDetailsMaster.FormulaDesEmp = $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee;
            $scope.PFPolicyDetailsMaster.FormulaDesIDEmp = $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee;
            $scope.PFPolicyDetailsMaster.FormulaDesIDEmpDis = "( " + $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee + " ) * " + $scope.PFPolicyDetailsMaster.EmpCntValPer + " / 100";
            $scope.PFPolicyDetailsMaster.SalaryHeadIDEmp = $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee;

            $scope.PFPolicyDetailsMaster.FormulaDesEmployer = $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer;
            $scope.PFPolicyDetailsMaster.FormulaDesIDEmployer = $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer;
            $scope.PFPolicyDetailsMaster.FormulaDesIDEmployerDis = "( " + $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer + " ) * " + $scope.PFPolicyDetailsMaster.EmployerCntValPer + " / 100";
            $scope.PFPolicyDetailsMaster.SalaryHeadIDEmployer = $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer;

            ValidationDetails();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveDetailsUrl,
                data: { 'Details': $scope.PFPolicyDetailsMaster, 'Master': $scope.PFPolicyMaster.ID, 'Employer': $scope.EmployerList, 'Employee': $scope.EmployeeList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.getDetailMaster();
                    var eDialog = $("#dialogPFSetting").data("ejDialog");
                    eDialog.close();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridPFPolicy").data("ejGrid");
        $scope.PFPolicyMaster = gridObj.getSelectedRecords()[0];
        $scope.GetHeadList($scope.PFPolicyMaster.ID);
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
        }
        $scope.getDetailMaster($scope.PFPolicyMaster.ID);
    };
    $scope.getDetailMaster = function () {
        $http.get('Attendances/PFPolicy/GetDetailsListM?MasterId=' + $scope.PFPolicyMaster.ID)
            .then(function (response) {
                $scope.PFPolicyDetailsList = response.data;
            });
    };
    $scope.GetHeadList = function (MasterId) {
        $http.get('Attendances/PFPolicy/GetHeadList?MasterId=' + MasterId)
            .then(function (response) {
                $scope.HeadList = response.data;
            });
    };
    $scope.recorddoubleclickDetails = function () {


        var gridObj = $("#PFBonusPolicyDetails").data("ejGrid");
        $scope.PFPolicyDetailsMaster = gridObj.getSelectedRecords()[0];

        $scope.salaryRuleGeneral.FormulaDescription = $scope.PFPolicyDetailsMaster.FormulaDesEarning;
        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.PFPolicyDetailsMaster.FormulaDesIDEarning;

        $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.PFPolicyDetailsMaster.FormulaDesEmp;
        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.PFPolicyDetailsMaster.FormulaDesIDEmp;

        $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.PFPolicyDetailsMaster.FormulaDesEmployer;
        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.PFPolicyDetailsMaster.FormulaDesIDEmployer;

        if ($scope.salaryRuleGeneral.FormulaDescription != null) {
            var str = $scope.salaryRuleGeneral.FormulaDescription;
            $scope.FormulaArray = str.split(" ");

            var strId = $scope.salaryRuleGeneral.FormulaIDDescription;
            $scope.FormulaIdArray = strId.split(" ");
        }
        if ($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee != null) {
            var str = $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee;
            $scope.FormulaArrayEmployee = str.split(" ");

            var strId = $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee;
            $scope.FormulaIdArrayEmployee = strId.split(" ");
        }
        if ($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer != null) {
            var str = $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer;
            $scope.FormulaArrayEmployer = str.split(" ");

            var strId = $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer;
            $scope.FormulaIdArrayEmployer = strId.split(" ");
        }

        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogPFSetting").data("ejDialog");
            eDialog.open();
            $scope.getEmployeer($scope.PFPolicyDetailsMaster.ID);

        } catch (e) {

        }
    };

    $scope.getEmployeer = function (DetailId) {
        $http.get('Attendances/PFPolicy/GetEmloyeerDetails?Details=' + DetailId)
            .then(function (response) {
                $scope.EmployerList = response.data;
            });
        $http.get('Attendances/PFPolicy/GetEmloyeeDetails?Details=' + DetailId)
            .then(function (response) {
                $scope.EmployeeList = response.data;
            });
    };
    $scope.EmployeerL = function () {
        var gridObj = $("#GridPFPolicyD").data("ejGrid");
        $scope.PFPolicyDetailsMasterR = gridObj.getSelectedRecords()[0];
        $scope.PFPolicyDetailsMaster.EmployeerID = $scope.PFPolicyDetailsMasterR.EmployeerID;
        $scope.PFPolicyDetailsMaster.EmployerValue = $scope.PFPolicyDetailsMasterR.EmployerValue;
        $scope.PFPolicyDetailsMaster.EmployerUpperLimit = $scope.PFPolicyDetailsMasterR.EmployerUpperLimit;
        $scope.PFPolicyDetailsMaster.EmployerSalaryHeadID = $scope.PFPolicyDetailsMasterR.EmployerSalaryHeadID;
        $scope.PFPolicyDetailsMaster.EmployerResidualValueSlrHdID = $scope.PFPolicyDetailsMasterR.EmployerResidualValueSlrHdID;
    };
    $scope.EmployeeL = function () {
        var gridObj = $("#GridPFPolicyeD").data("ejGrid");
        $scope.PFPolicyDetailsMasterrR = gridObj.getSelectedRecords()[0];
        $scope.PFPolicyDetailsMaster.EmployeeID = $scope.PFPolicyDetailsMasterrR.EmployeeID;
        $scope.PFPolicyDetailsMaster.EmployeeValue = $scope.PFPolicyDetailsMasterrR.EmployeeValue;
        $scope.PFPolicyDetailsMaster.EmployeeUpperLimit = $scope.PFPolicyDetailsMasterrR.EmployeeUpperLimit;
        $scope.PFPolicyDetailsMaster.EmployeeSalaryHeadID = $scope.PFPolicyDetailsMasterrR.EmployeeSalaryHeadID;
        $scope.PFPolicyDetailsMaster.EmployeeResidualValueSlrHdID = $scope.PFPolicyDetailsMasterrR.EmployeeResidualValueSlrHdID;
    };
    $scope.DeleteMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.PFPolicyMaster.ID)) {
            $http.get('Attendances/PFPolicy/DeleteM?ID=' + $scope.PFPolicyMaster.ID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearM();
                        $scope.getData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteDetails = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { ID: $scope.PFPolicyDetailsMaster.ID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getDetailMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearM = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.PFPolicyMaster = {
            ID: null,
            PFPolicyName: null,
            PFPolicyDescription: null,
            Eligibility: null,
            EligibilityBaseOn: 'DAY',
            EligibilityTimeLenght: null,
            MaturityBaseOn: 'MONTH',
            PlantID: $scope.PFPolicyMaster.PlantID,
            CompanyId: $scope.PFPolicyMaster.CompanyId,
            IsAllEmpApplocable: false,
        };
        //$scope.PFPolicyMasterModel = Object.assign({}, $scope.PFPolicyMaster);
        $scope.PFPolicyDetailsList = [];
        $scope.PFPolicyHead = {
            Id: null,
            PFPolicyMasterID: $scope.PFPolicyMaster.ID,
            SalaryHeadID: null,
            SalaryHeadName: null,
        }
        $scope.HeadList = [];
    }

    $scope.Clear = function () {
        ClearField();
        return true;
    };

    function ClearField() {
        $scope.PFPolicyDetailsMaster = {
            ID: null,
            DetailsId: null,
            PFPolicyMasterID: null,
            EarningValueRangeFrom: null,
            EarningValueRangeTo: null,
            IsMandatory: false,
            IsFixedEmp: false,
            IsFixedEmployer: false,
            FixedValueEmp: 0,
            IsFormulaEmp: false,
            IsContributionSlrHDdependOnEarningEmp: false,
            IsDistributionEmp: false,
            FixedValueEmployer: 0,
            IsFormulaEmployer: false,
            IsContributionSlrHDdependOnEarningEmployer: false,
            FormulaDesEmployer: null,
            IsDistributionEmployer: false,
            EmpCntValPer: 0,
            EmployerCntValPer: 0,
            EmpVolunValPer: 0,
            IsVoluntaryPF: false,
            IsNotEntGetEmplrAlwn: false,
            IsIndividualAlwn: false,
            AlwnSlrHd: null,
            IsAgeLimit: false,
            AgeLimit: 0,

            //EmpFixedValue: 'FixedValue',
            //EmployerFixedValue: 'FixedValue',

            FormulaDesIDEarning: null,
            FormulaDesEarning: null,
            SalaryHeadIDEarning: null,
            FormulaDesEmp: null,
            FormulaDesIDEmp: null,
            SalaryHeadIDEmp: null,
            FormulaDesIDEmployer: null,
            SalaryHeadIDEmployer: null,
            FormulaDesIDEmployerDis: null,
            FormulaDesIDEmpDis: null,

            EmployeeID: null,
            EmployeePFPolicyDetailsID: null,
            EmployeeValue: null,
            EmployeeSalaryHeadID: null,
            EmployeeUpperLimit: null,
            EmployeeResidualValueSlrHdID: null,

            EmployerID: null,
            EmployerPFPolicyDetailsID: null,
            EmployerValue: null,
            EmployerSalaryHeadID: null,
            EmployerUpperLimit: null,
            EmployerResidualValueSlrHdID: null,
            EmployeeID: null,
            EmployeerID: null,
        };
        $scope.PFPolicyDetailsMasterModel = Object.assign({}, $scope.PFPolicyDetailsMaster);
        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;

        $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;
        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;
        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

        $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;
        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;
        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

        $scope.salaryRuleGeneral.Operator = null;
        $scope.salaryRuleGeneral.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneralEmployer.Operator = null;
        $scope.salaryRuleGeneralEmployer.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneral.Precedence = null;
        $scope.salaryRuleGeneralEmployer.Precedence = null;
        $scope.salaryRuleGeneral.Value = null;
        $scope.salaryRuleGeneralEmployer.Value = null;

        $scope.salaryRuleGeneralEmployee.SalaryHeadIdFormula = null;
        $scope.salaryRuleGeneralEmployee.Operator = null;
        $scope.salaryRuleGeneralEmployee.Precedence = null;
        $scope.salaryRuleGeneralEmployee.Value = null;

        $scope.EmployeeList = [];
        $scope.EmployerList = [];
        $scope.FormulaArray = [];
        $scope.FormulaIdArray = [];
        $scope.FormulaArrayEmployee = [];
        $scope.FormulaIdArrayEmployee = [];
        $scope.FormulaArrayEmployer = [];
        $scope.FormulaIdArrayEmployer = [];
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Plant", $scope.PFPolicyMaster.PlantID);
            CheckField("PF Policy Name", $scope.PFPolicyMaster.PFPolicyName);
            //CheckField("PF Policy Description", $scope.PFPolicyMaster.PFPolicyDescription);
            CheckField("Eligibility", $scope.PFPolicyMaster.Eligibility);
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationDetails() {
        try {
            CheckField("Base on Net Pay", $scope.PFPolicyDetailsMaster.IsContributionSlrHDdependOnEarningEmployer);
            CheckField("Age Limit Applicable", $scope.PFPolicyDetailsMaster.IsAgeLimit);
        } catch (ex) {
            throw ex;
        }
    };

    $scope.EligibilityDay = function () {
        $scope.PFPolicyMaster.EligibilityTimeLenghtMonth = 0;
    };

    $scope.EligibilityMonth = function () {
        $scope.PFPolicyMaster.EligibilityTimeLenghtDay = 0;
    };

    $scope.MaturityMonth = function () {
        $scope.PFPolicyMaster.MaturityTimeLenghtYear = 0;
    };
    $scope.MaturityYear = function () {
        $scope.PFPolicyMaster.MaturityTimeLenghtMonth = 0;
    };

    $scope.salaryRuleGeneral = {
        FormulaDescription: null,
        FormulaIDDescription: null,

    };

    $scope.salaryRuleGeneralEmployee = {
        FormulaDescriptionEmployee: null,
        FormulaIDDescriptionEmployee: null,
    };

    $scope.salaryRuleGeneralEmployer = {
        FormulaDescriptionEmployer: null,
        FormulaIDDescriptionEmployer: null,
    };

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogPFSetting").data("ejDialog");
            eDialog.open();

            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
            $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;
            $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
            $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;
            $scope.salaryRuleGeneral = {};
            $scope.salaryRuleGeneralEmployee = {};
            $scope.salaryRuleGeneralEmployer = {};

            $scope.PFPolicyDetailsMaster = {
                ID: null,
                PFPolicyMasterID: null,
                EarningValueRangeFrom: null,
                EarningValueRangeTo: null,
                IsMandatory: false,
                IsFixedEmp: false,
                FixedValueEmp: 0,
                IsFormulaEmp: false,
                IsContributionSlrHDdependOnEarningEmp: false,
                IsDistributionEmp: false,
                FixedValueEmployer: 0,
                IsFormulaEmployer: false,
                IsContributionSlrHDdependOnEarningEmployer: false,
                FormulaDesEmployer: null,
                IsDistributionEmployer: false,
                EmpCntValPer: 0,
                EmployerCntValPer: 0,
                EmpVolunValPer: 0,
                IsVoluntaryPF: false,
                IsNotEntGetEmplrAlwn: false,
                IsIndividualAlwn: false,
                AlwnSlrHd: null,
                IsAgeLimit: false,
                AgeLimit: 0,
                //EmpFixedValue: 'FixedValue',
                //EmployerFixedValue: 'FixedValue',
                FormulaDesIDEarning: null,
                FormulaDesEarning: null,
                SalaryHeadIDEarning: null,
                FormulaDesEmp: null,
                FormulaDesIDEmp: null,
                SalaryHeadIDEmp: null,
                FormulaDesIDEmployer: null,
                SalaryHeadIDEmployer: null,
                FormulaDesIDEmployerDis: null,
                FormulaDesIDEmpDis: null,
                EmployeeID: null,
                EmployeerID: null,
            };
            $scope.PFPolicyDetailsMasterModel = Object.assign({}, $scope.PFPolicyDetailsMaster);

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.salaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Attendances/PFPolicy/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.salaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

    //$scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }, { Text: "<=", Value: "<=" }, { Text: ">=", Value: ">=" }, { Text: "<", Value: "<" }, { Text: ">", Value: ">" }];

    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.FormulaArrayEmployee = [];
    $scope.FormulaIdArrayEmployee = [];

    $scope.FormulaArrayEmployer = [];
    $scope.FormulaIdArrayEmployer = [];

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

    $scope.checkFormulaEmployee = function (List, lastvalue) {
        var available = false;
        for (var i = 0; i < List.length; i++) {
            if (List[i].Text === lastvalue) {
                available = true;
                break;
            }
        }
        return available;
    }

    $scope.checkFormulaEmployer = function (List, lastvalue) {
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


    $scope.SetFormulaEmployee = function (formula) {
        try {

            if (formula === 'SHead') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.SalaryHeadIdFormula)) {

                    $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
                    $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

                    var lastvalue = $scope.FormulaArrayEmployee[$scope.FormulaArrayEmployee.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormulaEmployee($scope.OperatorList, lastvalue)) {
                            $scope.salaryRuleGeneralEmployee.SalaryHeadFormula = $("#SalaryHeadFormulaEmployee option:selected").text();

                            var str = $scope.salaryRuleGeneralEmployee.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.SalaryHeadIdFormula;
                            $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                            $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);
                        }
                        else {
                            $scope.salaryRuleGeneralEmployee.SalaryHeadFormula = $("#SalaryHeadFormulaEmployee option:selected").text();

                            var str = $scope.salaryRuleGeneralEmployee.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.SalaryHeadIdFormula;
                            $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                            $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);
                        }
                    }
                    else {
                        $scope.salaryRuleGeneralEmployee.SalaryHeadFormula = $("#SalaryHeadFormulaEmployee option:selected").text();

                        var str = $scope.salaryRuleGeneralEmployee.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');

                        $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.Formula;
                        $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.SalaryHeadIdFormula;
                        $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                        $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);
                    }
                }

                $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
                $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

                for (var i = 0; i < $scope.FormulaArrayEmployee.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee)) {
                        $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.FormulaArrayEmployee[i];
                    }
                    else {
                        $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee += ' ' + $scope.FormulaArrayEmployee[i];
                    }
                }

                for (var i = 0; i < $scope.FormulaIdArrayEmployee.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee)) {
                        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.FormulaIdArrayEmployee[i];
                    }
                    else {
                        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee += ' ' + $scope.FormulaIdArrayEmployee[i];
                    }
                }

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.Operator)) {

                    $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
                    $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

                    var lastvalue = $scope.FormulaArrayEmployee[$scope.FormulaArrayEmployee.length - 1];

                    if ($scope.checkFormulaEmployee($scope.OperatorList, lastvalue) === false) {
                        $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.salaryRuleGeneralEmployee.Operator;
                        $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.Operator;
                        $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                        $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);
                    }

                    for (var i = 0; i < $scope.FormulaArrayEmployee.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee)) {
                            $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.FormulaArrayEmployee[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee += ' ' + $scope.FormulaArrayEmployee[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArrayEmployee.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee)) {
                            $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.FormulaIdArrayEmployee[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee += ' ' + $scope.FormulaIdArrayEmployee[i];
                        }
                    }


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.Precedence)) {

                    $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
                    $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

                    $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.salaryRuleGeneralEmployee.Precedence;
                    $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDes)) {
                        $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                        $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);

                        for (var i = 0; i < $scope.FormulaArrayEmployee.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee)) {
                                $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.FormulaArrayEmployee[i];
                            }
                            else {
                                $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee += ' ' + $scope.FormulaArrayEmployee[i];
                            }
                        }

                        for (var i = 0; i < $scope.FormulaIdArrayEmployee.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee)) {
                                $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.FormulaIdArrayEmployee[i];
                            }
                            else {
                                $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee += ' ' + $scope.FormulaIdArrayEmployee[i];
                            }
                        }

                    }
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.Value)) {

                    $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
                    $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;

                    $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.salaryRuleGeneralEmployee.Value;
                    $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.salaryRuleGeneralEmployee.Value;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDes)) {
                        $scope.FormulaArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDes);
                        $scope.FormulaIdArrayEmployee.push($scope.salaryRuleGeneralEmployee.FormulaDesID);
                    }


                    for (var i = 0; i < $scope.FormulaArrayEmployee.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee)) {
                            $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.FormulaArrayEmployee[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee += ' ' + $scope.FormulaArrayEmployee[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArrayEmployee.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee)) {
                            $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.FormulaIdArrayEmployee[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee += ' ' + $scope.FormulaIdArrayEmployee[i];
                        }
                    }

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveFormulaEmployee = function () {
        $scope.salaryRuleGeneralEmployee.FormulaDesID = null;

        var count = $scope.FormulaArrayEmployee.length;
        $scope.FormulaArrayEmployee.splice(count - 1);

        var count = $scope.FormulaIdArrayEmployee.length;
        $scope.FormulaIdArrayEmployee.splice(count - 1);

        $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = null;
        $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = null;
        $scope.salaryRuleGeneralEmployee.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArrayEmployee.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee)) {
                $scope.salaryRuleGeneralEmployee.FormulaDes = $scope.FormulaArrayEmployee[i];
                $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee = $scope.FormulaArrayEmployee[i];


            } else {
                $scope.salaryRuleGeneralEmployee.FormulaDes += $scope.FormulaArrayEmployee[i];
                $scope.salaryRuleGeneralEmployee.FormulaDescriptionEmployee += ' ' + $scope.FormulaArrayEmployee[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArrayEmployee.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee)) {
                $scope.salaryRuleGeneralEmployee.FormulaDesID = $scope.FormulaIdArrayEmployee[i];
                $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee = $scope.FormulaIdArrayEmployee[i];


            } else {
                $scope.salaryRuleGeneralEmployee.FormulaDesID += $scope.FormulaIdArrayEmployee[i];
                $scope.salaryRuleGeneralEmployee.FormulaIDDescriptionEmployee += ' ' + $scope.FormulaIdArrayEmployee[i];
            }
        }
    }


    $scope.SetFormulaEmployer = function (formula) {
        try {

            if (formula === 'SHead') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.SalaryHeadIdFormula)) {

                    $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
                    $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

                    var lastvalue = $scope.FormulaArrayEmployer[$scope.FormulaArrayEmployer.length - 1];

                    if (!baseService.isUndefinedOrNull(lastvalue)) {
                        if ($scope.checkFormulaEmployer($scope.OperatorList, lastvalue)) {
                            $scope.salaryRuleGeneralEmployer.SalaryHeadFormula = $("#SalaryHeadFormulaEmployer option:selected").text();

                            var str = $scope.salaryRuleGeneralEmployer.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.SalaryHeadIdFormula;
                            $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                            $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);
                        }
                        else {
                            $scope.salaryRuleGeneralEmployer.SalaryHeadFormula = $("#SalaryHeadFormulaEmployer option:selected").text();

                            var str = $scope.salaryRuleGeneralEmployer.SalaryHeadFormula;
                            $scope.Formula = str.replace(/\s/g, '');

                            $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.Formula;
                            $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.SalaryHeadIdFormula;
                            $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                            $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);
                        }
                    }
                    else {
                        $scope.salaryRuleGeneralEmployer.SalaryHeadFormula = $("#SalaryHeadFormulaEmployer option:selected").text();

                        var str = $scope.salaryRuleGeneralEmployer.SalaryHeadFormula;
                        $scope.Formula = str.replace(/\s/g, '');

                        $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.Formula;
                        $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.SalaryHeadIdFormula;
                        $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                        $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);
                    }
                }

                $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
                $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

                for (var i = 0; i < $scope.FormulaArrayEmployer.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer)) {
                        $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.FormulaArrayEmployer[i];
                    }
                    else {
                        $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer += ' ' + $scope.FormulaArrayEmployer[i];
                    }
                }

                for (var i = 0; i < $scope.FormulaIdArrayEmployer.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer)) {
                        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.FormulaIdArrayEmployer[i];
                    }
                    else {
                        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer += ' ' + $scope.FormulaIdArrayEmployer[i];
                    }
                }

            }
            else if (formula === 'Operator') {
                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.Operator)) {

                    $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
                    $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

                    var lastvalue = $scope.FormulaArrayEmployer[$scope.FormulaArrayEmployer.length - 1];

                    if ($scope.checkFormulaEmployer($scope.OperatorList, lastvalue) === false) {
                        $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.salaryRuleGeneralEmployer.Operator;
                        $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.Operator;
                        $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                        $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);
                    }

                    for (var i = 0; i < $scope.FormulaArrayEmployer.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer)) {
                            $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.FormulaArrayEmployer[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer += ' ' + $scope.FormulaArrayEmployer[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArrayEmployer.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer)) {
                            $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.FormulaIdArrayEmployer[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer += ' ' + $scope.FormulaIdArrayEmployer[i];
                        }
                    }


                } else {
                    throw "First select Salary Head.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.Precedence)) {

                    $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
                    $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

                    $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.salaryRuleGeneralEmployer.Precedence;
                    $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.Precedence;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDes)) {
                        $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                        $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);

                        for (var i = 0; i < $scope.FormulaArrayEmployer.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer)) {
                                $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.FormulaArrayEmployer[i];
                            }
                            else {
                                $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer += ' ' + $scope.FormulaArrayEmployer[i];
                            }
                        }

                        for (var i = 0; i < $scope.FormulaIdArrayEmployer.length; i++) {
                            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer)) {
                                $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.FormulaIdArrayEmployer[i];
                            }
                            else {
                                $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer += ' ' + $scope.FormulaIdArrayEmployer[i];
                            }
                        }

                    }
                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.Value)) {

                    $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
                    $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;

                    $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.salaryRuleGeneralEmployer.Value;
                    $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.salaryRuleGeneralEmployer.Value;


                    if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDes)) {
                        $scope.FormulaArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDes);
                        $scope.FormulaIdArrayEmployer.push($scope.salaryRuleGeneralEmployer.FormulaDesID);
                    }


                    for (var i = 0; i < $scope.FormulaArrayEmployer.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer)) {
                            $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.FormulaArrayEmployer[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer += ' ' + $scope.FormulaArrayEmployer[i];
                        }
                    }

                    for (var i = 0; i < $scope.FormulaIdArrayEmployer.length; i++) {
                        if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer)) {
                            $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.FormulaIdArrayEmployer[i];
                        }
                        else {
                            $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer += ' ' + $scope.FormulaIdArrayEmployer[i];
                        }
                    }

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.RemoveFormulaEmployer = function () {
        $scope.salaryRuleGeneralEmployer.FormulaDesID = null;

        var count = $scope.FormulaArrayEmployer.length;
        $scope.FormulaArrayEmployer.splice(count - 1);

        var count = $scope.FormulaIdArrayEmployer.length;
        $scope.FormulaIdArrayEmployer.splice(count - 1);

        $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = null;
        $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = null;
        $scope.salaryRuleGeneralEmployer.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArrayEmployer.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer)) {
                $scope.salaryRuleGeneralEmployer.FormulaDes = $scope.FormulaArrayEmployer[i];
                $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer = $scope.FormulaArrayEmployer[i];


            } else {
                $scope.salaryRuleGeneralEmployer.FormulaDes += $scope.FormulaArrayEmployer[i];
                $scope.salaryRuleGeneralEmployer.FormulaDescriptionEmployer += ' ' + $scope.FormulaArrayEmployer[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArrayEmployer.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer)) {
                $scope.salaryRuleGeneralEmployer.FormulaDesID = $scope.FormulaIdArrayEmployer[i];
                $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer = $scope.FormulaIdArrayEmployer[i];


            } else {
                $scope.salaryRuleGeneralEmployer.FormulaDesID += $scope.FormulaIdArrayEmployer[i];
                $scope.salaryRuleGeneralEmployer.FormulaIDDescriptionEmployer += ' ' + $scope.FormulaIdArrayEmployer[i];
            }
        }
    }


    $scope.ChangeEmployeeFormula = function () {
        $scope.PFPolicyDetailsMaster.FixedValueEmp = 0;
    }
    $scope.ChangeEmployerFormula = function () {
        $scope.PFPolicyDetailsMaster.FixedValueEmployer = 0;
    }
    $scope.ChangeFixedValueCheckboxEmployee = function () {
        $scope.PFPolicyDetailsMaster.IsContributionSlrHDdependOnEarningEmp = false;
    }
    $scope.ChangeFixedValueCheckboxEmployer = function () {
        $scope.PFPolicyDetailsMaster.IsContributionSlrHDdependOnEarningEmployer = false;
    }

    $scope.ChangeAgeLimitApplicable = function () {
        if ($scope.PFPolicyDetailsMaster.IsAgeLimit == false) {
            $scope.PFPolicyDetailsMaster.AgeLimit = 0;
        }
    }

    $scope.ChangeVoluntaryPF = function () {
        if ($scope.PFPolicyDetailsMaster.IsVoluntaryPF == false) {
            $scope.PFPolicyDetailsMaster.EmpVolunValPer = 0;
        }
    }

    $scope.plantList = [];
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.PFPolicyMaster.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }
    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };


    $scope.Submit = function () {
        try {
            var obj = {};
            if ($scope.PFPolicyDetailsMaster.EmployeerID != null) {
                for (var i = 0; i < $scope.EmployerList.length; i++) {
                    if ($scope.PFPolicyDetailsMaster.EmployeerID == $scope.EmployerList[i].EmployeerID) {
                        $scope.EmployerList.splice(i, 1);
                    }
                }
            }

            var total = 0;
            if ($scope.EmployerList.length > 0) {
                for (var i = 0; i < $scope.EmployerList.length; i++) {
                    total = parseFloat(total) + parseFloat($scope.EmployerList[i].EmployerValue);
                }
                var Ltotal = 0;
                Ltotal = parseFloat(total) + parseFloat($scope.PFPolicyDetailsMaster.EmployerValue);
                if ($scope.PFPolicyDetailsMaster.EmployerCntValPer < Ltotal) {
                    throw "Total Employeer Value Cannot be Greater than Employer Contribution Value [ " + $scope.PFPolicyDetailsMaster.EmployerCntValPer + " ]";
                }
            }

            if (baseService.isUndefinedOrNull($scope.PFPolicyDetailsMaster.EmployerValue)) {
                throw "Enter Distribution Value..";
            }
            if (baseService.isUndefinedOrNull($scope.PFPolicyDetailsMaster.EmployerSalaryHeadID)) {
                throw "Enter Distribution Salary Head..";
            }
            obj.EmployeerID = $scope.PFPolicyDetailsMaster.EmployeerID;
            obj.EmployerValue = $scope.PFPolicyDetailsMaster.EmployerValue;
            obj.EmployerUpperLimit = $scope.PFPolicyDetailsMaster.EmployerUpperLimit;
            obj.EmployerSalaryHeadID = $scope.PFPolicyDetailsMaster.EmployerSalaryHeadID;
            obj.EmployerResidualValueSlrHdID = $scope.PFPolicyDetailsMaster.EmployerResidualValueSlrHdID;
            obj.EmployerSalaryHead = $("#SalaryHeadIdaa option:selected").text();
            obj.EmployerResidualValueSlrHd = $("#SalaryHeadIdee option:selected").text();
            $scope.EmployerList.push(obj);
            $scope.ClearSubmit();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.ClearSubmit = function () {
        $scope.PFPolicyDetailsMaster.EmployeerID = null;
        $scope.PFPolicyDetailsMaster.EmployerValue = null;
        $scope.PFPolicyDetailsMaster.EmployerUpperLimit = null;
        $scope.PFPolicyDetailsMaster.EmployerSalaryHeadID = null;
        $scope.PFPolicyDetailsMaster.EmployerResidualValueSlrHdID = null;

    };
    $scope.DistributionEmp = function () {
        if ($scope.PFPolicyDetailsMaster.IsDistributionEmp == false) {
            $scope.EmployeeList = [];
        }
    };
    $scope.IsDistributionEmployer = function () {
        if ($scope.PFPolicyDetailsMaster.IsDistributionEmployer == false) {
            $scope.EmployerList = [];
        }
    };
    $scope.SubmitE = function () {
        try {
            var obj = {};
            if ($scope.PFPolicyDetailsMaster.EmployeeID != null) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    if ($scope.PFPolicyDetailsMaster.EmployeeID == $scope.EmployeeList[i].EmployeeID) {
                        $scope.EmployeeList.splice(i, 1);
                    }
                }
            }

            var total = 0;
            if ($scope.EmployeeList.length > 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    total = parseFloat(total) + parseFloat($scope.EmployeeList[i].EmployeeValue);
                }
                var Ltotal = 0;
                Ltotal = parseFloat(total) + parseFloat($scope.PFPolicyDetailsMaster.EmployeeValue);
                if ($scope.PFPolicyDetailsMaster.EmpCntValPer < Ltotal) {
                    throw "Total Employeer Value Cannot be Greater than Employee Contribution Value [ " + $scope.PFPolicyDetailsMaster.EmpCntValPer + " ]";
                }
            }

            if (baseService.isUndefinedOrNull($scope.PFPolicyDetailsMaster.EmployeeValue)) {
                throw "Enter Distribution Value..";
            }
            if (baseService.isUndefinedOrNull($scope.PFPolicyDetailsMaster.EmployeeSalaryHeadID)) {
                throw "Enter Distribution Salary Head..";
            }
            obj.EmployeeID = $scope.PFPolicyDetailsMaster.EmployeeID;
            obj.EmployeeValue = $scope.PFPolicyDetailsMaster.EmployeeValue;
            obj.EmployeeUpperLimit = $scope.PFPolicyDetailsMaster.EmployeeUpperLimit;
            obj.EmployeeSalaryHeadID = $scope.PFPolicyDetailsMaster.EmployeeSalaryHeadID;
            obj.EmployeeResidualValueSlrHdID = $scope.PFPolicyDetailsMaster.EmployeeResidualValueSlrHdID;
            obj.EmployeeSalaryHead = $("#SalaryHeadIdeee option:selected").text();
            obj.EmployeeResidualValueSlrHd = $("#SalaryHeadIdd option:selected").text();
            $scope.EmployeeList.push(obj);
            $scope.ClearSubmitE();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.ClearSubmitE = function () {
        $scope.PFPolicyDetailsMaster.EmployeeID = null;
        $scope.PFPolicyDetailsMaster.EmployeeValue = null;
        $scope.PFPolicyDetailsMaster.EmployeeUpperLimit = null;
        $scope.PFPolicyDetailsMaster.EmployeeSalaryHeadID = null;
        $scope.PFPolicyDetailsMaster.EmployeeResidualValueSlrHdID = null;

    };

    //#region Update 

    
    $scope.PFPolicyHead = {
        Id: null,
        PFPolicyMasterID: $scope.PFPolicyMaster.ID,
        SalaryHeadID: null,
        SalaryHeadName: null,
    }
    $scope.SubmitHeads = function () {
        try {
            for (var i = 0; i < $scope.HeadList.length; i++) {
                if ($scope.HeadList[i].SalaryHeadID == $scope.PFPolicyHead.SalaryHeadID) {
                    throw "This Salary head already Exist";
                }
            }
            for (var i = 0; i < $scope.salaryHeadList.length; i++) {
                if ($scope.salaryHeadList[i].Id == $scope.PFPolicyHead.SalaryHeadID) {
                    $scope.PFPolicyHead.SalaryHeadName = $scope.salaryHeadList[i].UserName;
                    break;
                }
            }
            var newObj = Object.assign({}, $scope.PFPolicyHead);
            $scope.HeadList.push(newObj);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
        
    $scope.message_confirmation = null;
    $scope.RemoveHead = function (obj) {
        $scope.PFPolicyHead = Object.assign({}, obj.data);
        if (!baseService.isUndefinedOrNull($scope.PFPolicyHead.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpHead')).modal('show');
    }
    $scope.DeleteHeadList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteHeadMaster?ID=' + $scope.PFPolicyHead.Id,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Invalid Head ");
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetHeadList($scope.PFPolicyMaster.ID);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    //#endregion



}
