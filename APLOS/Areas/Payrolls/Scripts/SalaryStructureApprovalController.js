'use strict';
SalaryStructureApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalaryStructureApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Salary Structure Approval';
    $scope.index = -1;

    $scope.EmpSalaryInfoDefine = [];
    $scope.path = 'Payrolls/SalaryStructureApproval/';
    $scope.getApprovedEmpListUrl = $scope.path + 'GetSalaryStrcApprovedEmployeeList';
    $scope.getEmpListUrl = $scope.path + 'GetEmployeeListForSalaryStrcApproval';
    $scope.SaveSalaryStructureApprovalDataUrl = $scope.path + 'SaveSalaryStructureApprovalData';
    $scope.IsApprovedEmpList = false;
    $scope.employees = [];

    $scope.LoadEmployeeDataForGrid = function (url) {
        try {
            $http.get(url)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employees = null;
                        $scope.employees = response.data;


                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };


    $scope.LoadEmployeeDataForGrid($scope.getEmpListUrl);

    $scope.messageText = "";

    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

    $scope.ShowEmpHeader = null;
    $scope.ShowMenuDiv = true;
    $scope.ShowEmpDiv = false;
    $scope.model2 = {
        Increment: false,
        Promotion: false

    };

    $scope.model = {
        Id: null,
        XLUploadMasterId: null,
        EmployeeCode: null,
        BudgetCode: null,
        GivenDesignationId: null,
        SalaryHead: null,
        PreviousAmount: null,
        CurrentAmount: null
    };

    $scope.ShowHideModel = function () {
        $scope.ShowMenuDiv = false;
        $scope.ShowEmpDiv = true;

    };

    $scope.qempid = null;
    $scope.qstatus = null;
    $scope.qstatus = null;
    $scope.DOC = null;
    $scope.divFreshEntry = true;



    // Employee Load
    $scope.LoadEmployeeData = function (IsApprovedEmpList) {

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
                $scope.ShowEmpHeader = 'Increment/Promotion';

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

    $scope.LoadEmployeeDataForGrid = function (url) {
        try {
            $http.get(url)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employees = null;
                        $scope.employees = response.data;


                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };


    $scope.commandApproval = [{
        type: "details", buttonOptions: {
            text: "Details",
            width: "100",
            height: "20",

            click: onClickApproval
        }
    }];

    $scope.ApprovalTitle = "";
    function onClickApproval(arg) {
        $scope.ApprovalTitle = "Employee Salary Structure Change Approval";
        var eDialog = $("#dialogApproval").data("ejDialog");
        eDialog.open();

        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];


        angular.copy(data, $scope.budgetCodeChangeOld);

        $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
        $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;
        $scope.imageSrc = virtualPath.EmployeePic + $scope.budgetCodeChangeOld.EmpPicPath;
        $scope.Action = 'Update';       
        $scope.getEmpSalaryInfoDefineData(data.SystemId);


     
    }


    $scope.messageText = "";
    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

























    $scope.IsApprovedEmpList = false;
    $scope.employees = [];


    // Employee Load
    $scope.LoadEmployeeData = function (IsApprovedEmpList) {

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
                $scope.ShowEmpHeader = 'Increment/Promotion';

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


    //-------increment
    $scope.EmpSalaryInfoDefine = {
        SalaryRuleMasterSystemID: null,
        CRCSystemID: null,
        AmtEntryCurrency: null,
        AmtDefinitionCurrency: null,
        AmtDisbusmentCurrency: null,
        AccumulateExchangeRate: null,
        AccumulateExchangeSalaryHeadID: null,
        egerInDisb: null,
        DisbusmentCurrencyID: null,
        DisbusmentCurrency: null,
        RoundOption: null,
        IsDecimalInDisb: null,
        DecimalNo: null,
        SalaryHdSequence: null,
        IsCTCComponent: null,
        IsGrossComponent: null,
        SlrInfoDefSystemID: null,
        CurrencyRuleChildSystemID: null,
        SalaryHeadID: null,
        SalaryHead: null,
        HeadType: null,
        FormulaDesID: null,
        FixedValue: null,
        IsOpen: null,
        EntryCurrencyID: null,
        EntryCurrency: null,
        DefinitionCurrencyID: null,
        DefinitionCurrency: null,
        DefineAmount: null,
        TagAndUnTag: null,
        MonthPeriod: null,
        IsNA: null,
        HeadCategory: null,
        SalaryCategory: null
    };
    $scope.EmpSalaryInfoDefineNew = {
        SalaryRuleMasterSystemID: null,
        CRCSystemID: null,
        AmtEntryCurrency: null,
        AmtDefinitionCurrency: null,
        AmtDisbusmentCurrency: null,
        AccumulateExchangeRate: null,
        AccumulateExchangeSalaryHeadID: null,
        egerInDisb: null,
        DisbusmentCurrencyID: null,
        DisbusmentCurrency: null,
        RoundOption: null,
        IsDecimalInDisb: null,
        DecimalNo: null,
        SalaryHdSequence: null,
        IsCTCComponent: null,
        IsGrossComponent: null,
        SlrInfoDefSystemID: null,
        CurrencyRuleChildSystemID: null,
        SalaryHeadID: null,
        SalaryHead: null,
        HeadType: null,
        FormulaDesID: null,
        FixedValue: null,
        IsOpen: null,
        EntryCurrencyID: null,
        EntryCurrency: null,
        DefinitionCurrencyID: null,
        DefinitionCurrency: null,
        DefineAmount: null,
        TagAndUnTag: null,
        MonthPeriod: null,
        IsNA: null,
        HeadCategory: null,
        SalaryCategory: null
    };


    $scope.EmpSalaryOpenHead = {
        SalaryHeadID: null,
        SalaryHead: null,
        Description: null,
        SalaryRuleDescription: null,
        HeadType: null,
        EntryCurrency: null,
        Amount: null,
        HeadCategory: null,
        EffectiveDate: null,
        SalaryID: null,
        SalaryHdSequence: null,
        OldAmount: null
    };
    $scope.EmpSalaryApprovedOpenHead = {
        SalaryHeadID: null,
        SalaryHead: null,
        Description: null,
        SalaryRuleDescription: null,
        HeadType: null,
        EntryCurrency: null,
        Amount: null,
        HeadCategory: null,
        EffectiveDate: null,
        SalaryID: null,
        SalaryHdSequence: null,
        OldAmount: null
    };
    $scope.EmpSalaryOpenHeadCurrent = {
        SalaryHeadID: null,
        SalaryHead: null,
        Description: null,
        SalaryRuleDescription: null,
        HeadType: null,
        EntryCurrency: null,
        Amount: null,
        HeadCategory: null,
        EffectiveDate: null,
        SalaryID: null,
        SalaryHdSequence: null,
        OldAmount: null
    };
    $scope.SalaryRole = {
        SalaryRuleMasterSystemID: null,
        SalaryRuleName: null,
        SalaryRuleDescription: null
    };
    $scope.SelectedSalaryRole = null;
    $scope.EmpSalaryInfo = {
        SalaryRuleMasterSystemID: null,
        SalaryApprovedStatus: null,
        SalaryID: null,
        EffectiveDate: null,
        NextDueDate: null,
        IsFreshEntry: false

    };
    $scope.IncrementHistory =
        {
            SystemID: null,
            EmpSystemID: null,
            IncrementType: null,
            FromSalaryId: null,
            ToSalaryId: null,
            FromGivenDesignationId: null,
            ToGivenDesignationId: null,
            FromLegalDesignationId: null,
            ToLegalDesignationId: null,
            FromEffectiveDate: null,
            ToEffectiveDate: null,
            FromBudgetCode: null,
            ToBudgetCode: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null,
            IsConfirmation: null,
            IsPromotion: null
        };
    $scope.Calculated = false;
    $scope.MinWage = null;
    $scope.NetGross = null;


    $scope.ResultTatalGross = null;
    $scope.ResultCTC = null;
    $scope.ResultNetpay = null;

    $scope.NewNetGross = null;
    $scope.NewNetCTC = null;
    $scope.newFormula_Desc = null;
    $scope.approvedFormula_Desc = null;
    $scope.ApprovedNextDueDate = null;
    $scope.ApprovedEffectiveDate = null;
    $scope.UnApprovedNextDueDate = null;

    $scope.IsSalaryRuleEditableEmployee = false;

    $scope.getEmpSalaryInfoDefineData = function (EmpSystemId) {
        try {
            $http.get('Payrolls/SalaryStructureApproval/LoadEmpSalaryInfoDataForApproval?EmpSystemId=' + EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        //$scope.givenDesignationChange();
                        $scope.Calculated = false;
                        $scope.NewNetGross = null;
                        $scope.NewNetCTC = null;
                        $scope.EmpSalaryInfoDefine = null;
                        //$scope.EmpSalaryOpenHeadCurrent = null;
                        //$scope.EmpSalaryApprovedOpenHead = null;
                        $scope.EmpSalaryInfoDefine = null;
                        //$scope.MinWage = null;
                        $scope.SalaryRole = null;
                        $scope.SelectedSalaryRole = null;
                        //$scope.EmpSalaryOpenHead = null;
                        //$scope.newFormula_Desc = null;
                        //$scope.approvedFormula_Desc = null;
                        $scope.UnApprovedNextDueDate = null;




                        $scope.EmpSalaryInfoDefine = response.data.EmpSalaryInfoDefine;
                   
                        $scope.SelectedSalaryRole = response.data.ResultSelectedSalaryRule;
                        ///EmpSalaryInfo.SalaryRuleMasterSystemID= $scope.getSalaryRuleMasterSystemIDBygivenDesignation($scope.budgetCodeChangeNew.GivenDesignationId);

                        //$scope.EmpSalaryOpenHead = response.data.ResultOpenHead;
                        //$scope.EmpSalaryApprovedOpenHead = response.data.ResultApprovedOpenHead;
                        //$scope.EmpSalaryOpenHeadCurrent = response.data.ResultOpenHead;
                        //$scope.newFormula_Desc = response.data.NewFormula_Desc;
                        //$scope.approvedFormula_Desc = response.data.ApprovedFormula_Desc;
                        //$scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data.ResultSelectedSalaryRule[0].SalaryRuleMasterSystemID;
                        //if (baseService.isUndefinedOrNull(response.data.ResultOpenHead[0].SalaryID)) {
                        //    $scope.EmpSalaryInfo.SalaryID = null;
                        //} else {
                        //    $scope.EmpSalaryInfo.SalaryID = response.data.ResultOpenHead[0].SalaryID;
                        //}

                        //if (baseService.isUndefinedOrNull(response.data.ApprovalStatus)) {
                        //    $scope.EmpSalaryInfo.SalaryApprovedStatus = null;
                        //} else {
                        //    $scope.EmpSalaryInfo.SalaryApprovedStatus = response.data.ApprovalStatus;
                        //}
                        ////-------
                        //if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {//--- for Confirmation

                        //    $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')($scope.DOC, 'dd-M-yyyy');

                        //}
                        //else { //--- for increment
                            if (baseService.isUndefinedOrNull(response.data.ResultEffectiveDate)) {
                                $scope.EmpSalaryInfo.EffectiveDate = null;
                            } else {
                                $scope.EmpSalaryInfo.EffectiveDate = response.data.ResultEffectiveDate;
                            }
                        //}



                        //if (baseService.isUndefinedOrNull(response.data.ApprovedNextDueDate)) {
                        //    $scope.ApprovedNextDueDate = null;
                        //} else {
                        //    $scope.ApprovedNextDueDate = response.data.ApprovedNextDueDate;
                        //}
                        if (baseService.isUndefinedOrNull(response.data.UnApprovedNextDueDate)) {
                            $scope.EmpSalaryInfo.NextDueDate = null;
                        } else {
                            $scope.EmpSalaryInfo.NextDueDate = response.data.UnApprovedNextDueDate;
                        }



                        //if (baseService.isUndefinedOrNull(response.data.ApprovedEffectiveDate)) {
                        //    $scope.ApprovedEffectiveDate = null;
                        //} else {
                        //    $scope.ApprovedEffectiveDate = response.data.ApprovedEffectiveDate;
                        //}
                        if (baseService.isUndefinedOrNull(response.data.ResultTatalGross)) {
                            $scope.ResultTatalGross = null;
                        } else {
                            $scope.ResultTatalGross = response.data.ResultTatalGross;
                        }
                        if (baseService.isUndefinedOrNull(response.data.ResultCTC)) {
                            $scope.ResultCTC = null;
                        } else {
                            $scope.ResultCTC = response.data.ResultCTC;
                        }
                        if (baseService.isUndefinedOrNull(response.data.ResultNetpay)) {
                            $scope.ResultNetpay = null;
                        } else {
                            $scope.ResultNetpay = response.data.ResultNetpay;
                        }





                        //$scope.ResultTatalGross = null;
                        //$scope.ResultCTC = null;
                        //$scope.ResultNetpay = null;
                        //if (baseService.isUndefinedOrNull(response.data.IsSalaryRuleEditableEmployee)) {
                        //    $scope.IsSalaryRuleEditableEmployee = false;
                        //} else {
                        //    $scope.IsSalaryRuleEditableEmployee = !response.data.IsSalaryRuleEditableEmployee;
                        //}

                        //Increment History
                        //$scope.IncrementHistory.EmpSystemID = EmpSystemId;
                        //$scope.IncrementHistory.FromEffectiveDate = $scope.ApprovedEffectiveDate;

                        //if (!baseService.isUndefinedOrNull(response.data.ResultApprovedOpenHead)) {
                        //    $scope.IncrementHistory.FromSalaryId = response.data.ResultApprovedOpenHead[0].SalaryID;
                        //}

                         $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')(response.data.ResultOpenHead[0].EffectiveDate, 'dd-M-yyyy');
                        //if (response.data.ApprovalStatus !== 'Approved') {
                        //    for (var i = 0; i < $scope.EmpSalaryOpenHead.length; i++) {

                        //        $scope.EmpSalaryOpenHead[i].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHead[i].EffectiveDate, 'dd-M-yyyy');
                        //        //if ($scope.EmpSalaryOpenHead[i].Amount === null) {
                        //        //    $scope.EmpSalaryOpenHead[i].OldAmount = 0;
                        //        //}
                        //        //else {
                        //        //    $scope.EmpSalaryOpenHead[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                        //        //    $scope.EmpSalaryOpenHead[i].Amount = null;
                        //        //}
                        //        if ($scope.EmpSalaryOpenHead[i].Amount === null) {
                        //            $scope.EmpSalaryOpenHead[i].Amount = 0;
                        //        }
                        //        else {
                        //            $scope.EmpSalaryOpenHead[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                        //            //$scope.EmpSalaryOpenHead[i].Amount = null;
                        //        }
                        //    }
                        //}
                        //else {
                        //    for (var j = 0; j < $scope.EmpSalaryOpenHead.length; j++) {

                        //        $scope.EmpSalaryOpenHead[j].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHead[j].EffectiveDate, 'dd-M-yyyy');
                        //        if ($scope.EmpSalaryOpenHead[j].Amount === null) {
                        //            $scope.EmpSalaryOpenHead[j].OldAmount = 0;
                        //        }
                        //        else {
                        //            $scope.EmpSalaryOpenHead[j].OldAmount = $scope.EmpSalaryOpenHead[j].Amount;
                        //            $scope.EmpSalaryOpenHead[j].Amount = null;
                        //        }

                        //    }
                        //}
                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };
    $scope.getEmpDataById = function (EmpSystemId) {
        try {
            $http.get('humanresource/employeepromotion/GetSalaryStrcApprovedEmployeeById?EmpSystemId=' + EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        if (baseService.isUndefinedOrNull(response.data[0])) {
                            throw "Invalid User.";
                        }
                        $scope.employees = response.data[0];

                        $scope.Get(null, 0);
                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };
    $scope.LoadEmployeeDataForGrid = function (url) {
        try {
            $http.get(url)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.employees = null;
                        $scope.employees = response.data;


                    }
                    function errorCallBack(response) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };
   


    //-------Promotion
    $scope.budgetCodeChangeOld = {
        SystemId: null,
        EmployeeId: null,
        EmployeeCode: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        BudgetCode: null,
        PositionID: null,
        IsDirect: null,
        SalaryPercentage: null,
        CardNumber: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        EmployeeName: null,
        NickName: null,
        LocalEmployeeName: null,
        EmpPicPath: null,
        EmpType: null,
        EmploymentType: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: null,
        DOCDay: null,
        DOCIsMonth: null,
        DOCMonth: null,
        DOC: null,
        DOS: null,
        IsConfirmed: null,
        ReActiveDate: null,
        EmployeeStatus: null,
        NationalID: null,
        TIN: null,
        CitizenID: null,
        FatherName: null,
        MotherName: null,
        ReligionID: null,
        CivilStatusID: null,
        employeeID: null,
        GenderID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        PresCityID: null,
        ParmCityID: null,
        PresAreaID: null,
        ParmAreaID: null,
        TelePhnNo: null,
        CellPhnNo: null,
        EmailId: null,
        BudgetCategoryID: null,
        EmployeeCategorySystemID: null,
        LVPolicyMasterSystemID: null,
        SalaryRuleMasterSystemID: null,
        BankSystemID: null,
        BankName: null,
        BankAccNo: null,
        BankAddedBy: null,
        BankDateAdded: null,
        BankUpdatedBy: null,
        BankDateUpdated: null,
        RegisterFP: null,
        RegisterProximate: null,
        SuperViser: null,
        IsSlvDevReg: null,
        IsAttdnProcBaseOnDeviceData: null,
        SubSecStrucSystemID: null,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        EmrCntPer1Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2Name: null,
        EmrCntPer2CellNo: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        Image: null,
        DesignationGroupName: null
    };

    $scope.budgetCodeChange = {
        SystemId: null,
        EmployeeId: null,
        EmployeeCode: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        BudgetCode: null,
        PositionID: null,
        IsDirect: null,
        SalaryPercentage: null,
        CardNumber: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        EmployeeName: null,
        NickName: null,
        LocalEmployeeName: null,
        EmpPicPath: null,
        EmpType: null,
        EmploymentType: null,
        EmployeeGroupSystemID: null,
        JobLocationID: null,
        DOB: null,
        DOJ: null,
        DOCIsDay: null,
        DOCDay: null,
        DOCIsMonth: null,
        DOCMonth: null,
        DOC: null,
        DOS: null,
        IsConfirmed: null,
        ReActiveDate: null,
        EmployeeStatus: null,
        NationalID: null,
        TIN: null,
        CitizenID: null,
        FatherName: null,
        MotherName: null,
        ReligionID: null,
        CivilStatusID: null,
        employeeID: null,
        GenderID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCountryID: null,
        ParmCountryID: null,
        PresCityID: null,
        ParmCityID: null,
        PresAreaID: null,
        ParmAreaID: null,
        TelePhnNo: null,
        CellPhnNo: null,
        EmailId: null,
        BudgetCategoryID: null,
        EmployeeCategorySystemID: null,
        LVPolicyMasterSystemID: null,
        SalaryRuleMasterSystemID: null,
        BankSystemID: null,
        BankName: null,
        BankAccNo: null,
        BankAddedBy: null,
        BankDateAdded: null,
        BankUpdatedBy: null,
        BankDateUpdated: null,
        RegisterFP: null,
        RegisterProximate: null,
        SuperViser: null,
        IsSlvDevReg: null,
        IsAttdnProcBaseOnDeviceData: null,
        SubSecStrucSystemID: null,
        AddedBy: null,
        DateAdded: null,
        UpdatedBy: null,
        DateUpdated: null,
        EmrCntPer1Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2Name: null,
        EmrCntPer2CellNo: null,
        GivenDesignationId: null,
        LegalDesignationId: null,
        AgreedDOJ: null,
        TotalSalary: null,
        SpecialReviewDuration: null,
        SpecialReviewAmount: null,
        Image: null,
        DesignationGroupName: null
    };

    $scope.budgetCodeChangeNew = Object.assign({}, $scope.budgetCodeChange);




    $scope.NewbudgetCodeChange = {
        EntityName: null,
        Designation: null,
        PositionName: null,
        DesignationId: null
    };

    $scope.popUpList = [];
    $scope.valueData = '';


    $scope.popUp = function () {
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    $scope.ShowResultCustom(commonMessage.NetworkError, 'failure', 'EntryDiv');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.detailsPopUp = function () {
        // angular.element(document.querySelector('#detailsPopUpModal')).modal('show');
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        // $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };

    $scope.detailsNewPopUp = function () {
        //angular.element(document.querySelector('#detailsNewPopUpModal')).modal('show');
        var eDialog = $("#dialogAPI2").data("ejDialog");
        eDialog.open();
    };

    $scope.selectDoubleClick = function (data) {
        $scope.budgetCodeChangeNew.BudgetCode = data.Id;
        $scope.budgetCodeChangeNew.Code = data.Code;

        $scope.NewbudgetCodeChange.EntityName = data.EntityName;
        $scope.NewbudgetCodeChange.Designation = data.Designation;
        $scope.NewbudgetCodeChange.PositionName = data.PositionName;
        $scope.NewbudgetCodeChange.DesignationId = data.DesignationId;

        // $scope.budgetCodeChangeNew.GivenDesignationId = null;

        //cboService.getCboLowerGivenDesignation($scope.budgetCodeChange.DesignationId, function (result) {
        //    $scope.givenDesignationList = result;
        //    $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChangeNew.DesignationId;
        //    //preRecruitmentEmployeeNew.GivenDesignationId
        //});

        $scope.closePopUp();
    };

    $scope.LegalDesignationList = [];
    cboService.getCboLegalDesignation(null, function (result) {
        $scope.LegalDesignationList = result;
    });


 

    $scope.clearCode = function () {
        $scope.budgetCodeChangeNew.BudgetId = null;
        $scope.budgetCodeChangeNew.EntityName = null;
        $scope.budgetCodeChangeNew.Designation = null;
        $scope.budgetCodeChangeNew.PositionName = null;
        $scope.budgetCodeChangeNew.GivenDesignationId = null;
        $scope.budgetCodeChangeNew.LegalDesignationId = null;
    };

    $scope.lowerGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboLowerGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.budgetCodeChangeNew.GivenDesignationId = gid;
        });
    };

    $scope.uppderGivenDesignationCbo = function (id, gid) {
        $scope.givenDesignationList = [];
        cboService.getCboUpperGivenDesignation(id, function (result) {
            $scope.givenDesignationList = result;
            $scope.budgetCodeChangeNew.GivenDesignationId = gid;
        });
    };
    $scope.getDes = function () {
        if ($scope.budgetCodeChangeNew.IsExceptionalDesigApplicable === false) {
            $scope.lowerGivenDesignationCbo($scope.budgetCodeChangeNew.DesignationId);
        }
        else {
            $scope.uppderGivenDesignationCbo($scope.budgetCodeChangeNew.DesignationId);
        }
    };


    $scope.Get = function (data) {
        //Confirmation
        if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {
            $scope.divFreshEntry = false;


            //$scope.budgetCodeChange = $filter("filter")($scope.employees, { SystemId: data.data.SystemId });
            //$scope.budgetCodeChangeOld = $filter("filter")($scope.employees, { SystemId: data.data.SystemId});

            $scope.budgetCodeChange = $scope.employees;
            $scope.budgetCodeChangeOld = $scope.employees;
            // angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);
            $scope.budgetCodeChangeNew = $scope.budgetCodeChange;
            $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
            $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

            $scope.imageSrc = virtualPath.EmployeePic + $scope.budgetCodeChangeOld.EmpPicPath;


            $scope.Action = 'Update';
            $scope.EntryShow();
            $scope.getEmpSalaryInfoDefineData($scope.qempid);
        }
        else {
            //$scope.index = index;
            //$scope.budgetCodeChange = $scope.employees[$scope.index];
            //$scope.budgetCodeChangeOld = $scope.employees[$scope.index];
            $scope.obj = $filter("filter")($scope.employees, { SystemId: data.data.SystemId });

            //$scope.budgetCodeChange = $scope.obj[0];         
            //$scope.budgetCodeChangeOld = $scope.obj[0];
            //$scope.budgetCodeChangeNew = $scope.obj[0];

            angular.copy($scope.obj[0], $scope.budgetCodeChange);
            angular.copy($scope.obj[0], $scope.budgetCodeChangeOld);
            angular.copy($scope.obj[0], $scope.budgetCodeChangeNew);


            //angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);
            $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
            $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

            $scope.imageSrc = virtualPath.EmployeePic + $scope.budgetCodeChangeOld.EmpPicPath;
            $scope.Action = 'Update';
            $scope.EntryShow();
            $scope.getEmpSalaryInfoDefineData(data.data.SystemId);
        }




    };
    $scope.EntryShow = function () {
        //angular.element(document.querySelector('#EntryPopUp')).modal('show');
        var eDialog = $("#dialogAPIm").data("ejDialog");
        eDialog.open();


    };


    $scope.givenDesignationList = [];
    cboService.getCboGivenDesignation(function (result) {
        $scope.givenDesignationList = result;
    });

    // #region Update
    

    $scope.recorddoubleclick = function (args) {
        Get(args);
    };


    $scope.saveData = function () {
        try {
            var EmpSystemIds = [];
            for (var i = 0; i < $scope.employees.length; i++) {
                //$scope.employees[i].CheckBoxSelect = true;
                if ($scope.employees[i].CheckBoxSelect === true) {
                    EmpSystemIds.push($scope.employees[i].SystemId)
                }
            }
            if (EmpSystemIds.length===0) {
                throw 'Select data.'
            }

            //if ($scope.budgetCodeChangeNew.IsExceptionalDesigApplicable === true)
            //    $scope.budgetCodeChangeNew.IsExceptionalDesigApplicable = 1;
            
                $http({
                    method: 'POST',
                    url: $scope.SaveSalaryStructureApprovalDataUrl,
                    data: { 'EmpSystemId': EmpSystemIds },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                     
                    }
                    else {
                        $scope.ShowResultCustom(response.data.Message, "success", 'EntryDiv');
                        //$scope.Clear();                      
                        var eDialog = $("#dialogApproval").data("ejDialog");
                        eDialog.close();
                        $scope.LoadEmployeeDataForGrid($scope.getEmpListUrl);

                        $scope.budgetCodeChangeOld = {};
                        $scope.NewbudgetCodeChange = {};
                    }
                }, function errorCallback(response) {
                    $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                });
                return true;
            
        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
        }
    };

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employees.length; i++) {
                $scope.employees[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };



}