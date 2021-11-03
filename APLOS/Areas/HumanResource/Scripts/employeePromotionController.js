'use strict';
employeePromotionController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile'];

function employeePromotionController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $compile) {
    $rootScope.title = 'Employee Promotion';
    $scope.index = -1;
    $scope.employees = [];
    $scope.EmpSalaryInfoDefine = [];
    $scope.path = 'humanresource/employeepromotion/';
    $scope.getApprovedEmpListUrl = $scope.path + 'GetSalaryStrcApprovedEmployeeList';
    $scope.getUnApprovedEmpListUrl = $scope.path + 'GetSalaryStrcUnApprovedEmployeeList';
    $scope.updateUrl = $scope.path + 'update';
    $scope.UpdateSalaryStractureUrl = $scope.path + 'UpdateSalaryStracture';
    $scope.CalculateSalaryUrl = $scope.path + 'CalculateSalary';
    $scope.LoadEmpSalaryInfoDefineDataUrl = $scope.path + 'LoadEmpSalaryInfoDefineData';


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
    $scope.approvalStatusNew = null;
    $scope.approvalStatusApproved = null;
    //Get Query String Data
    $scope.GetQueryStringData = function () {
        $scope.qempid = $location.search().EmpId;
        $scope.qstatus = $location.search().PromotionFlag;
        $scope.DOC = $location.search().DOC;
        if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {
            $scope.divFreshEntry = false;
        }


    };
    $scope.GetQueryStringData();
    $scope.back = function () {
        $scope.ShowMenuDiv = true;
        $scope.ShowEmpDiv = false;
        $scope.model2.Increment = false;
        $scope.model2.Promotion = false;
        $scope.ShowEmpHeader = null;
    };

    $scope.loadFile = function (files) {
        $scope.$apply(function () {

            $scope.selectedFile = files[0];
        });
    };

    var ColList = ["EmployeeCode", "BudgetCode", "GivenDesignationId", "SalaryHead", "PreviousAmount", "CurrentAmount"];
    $scope.handleFile = function () {
        var file = $scope.selectedFile;
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var data = e.target.result;
                var workbook = XLSX.read(data, { type: 'binary' });
                var first_sheet_name = workbook.SheetNames[0];
                var dataObjects = XLSX.utils.sheet_to_json(workbook.Sheets[first_sheet_name]);
                var savelist = [];

                for (var i = 0; i < dataObjects.length; i++) {
                    var ob = dataObjects[i];

                    for (var j in $scope.model) {
                        $scope.model[j] = null;
                    }

                    var c = 0;
                    for (var k in ob) {
                        $scope.model[ColList[c]] = ob[k];
                        $scope.newList = angular.copy($scope.model);
                        c++;
                    }
                    savelist.push($scope.newList);
                }
                if (dataObjects.length > 0) {
                    $scope.save(savelist);
                } else {
                    $scope.msg = "Error : Something Wrong !";
                }
            };
            reader.onerror = function (ex) {
            };
            reader.readAsBinaryString(file);
        }
    };

    $scope.save = function (data) {
        $http({
            method: 'POST',
            url: "humanresource/employeepromotion/save",
            data: JSON.stringify(data),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'EntryDivM');
            }
            else {
                ShowResult(response.data.Message, 'success', 'EntryDivM');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'EntryDivM');
        };
    };

    $scope.Clear = function () {
        $scope.employee = {};
        return true;
    };

    //Employee Load
    $scope.IsApprovedEmpList = false;
    $scope.employees = [];
    //baseService.init($scope.getApprovedEmpListUrl, null, 10, null, 'EmployeeCode', 'EmployeeCode');
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.employees = result.data;
    //            //$scope.empParameters = result.Rows.Total;
    //        }, function () {
    //            $scope.ShowResultCustom(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};
    //$scope.getData();

    $scope.searchsByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Budget Code',
            'value': 'Code'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        },
        {
            'name': 'Phone Number',
            'value': 'CellPhnNo'
        }
    ];
    $rootScope.parameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeCode',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };
    // Employee Load
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
    $scope.SelectedSalaryRole = [];
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
    $scope.NetCTC = null;
    $scope.NewNetGross = null;
    $scope.NewNetCTC = null;
    $scope.newFormula_Desc = null;
    $scope.approvedFormula_Desc = null;
    $scope.ApprovedNextDueDate = null;
    $scope.ApprovedEffectiveDate = null;
    $scope.UnApprovedNextDueDate = null;

    $scope.IsSalaryRuleEditableEmployee = false;

    $scope.getEmpSalaryInfoDefineData = function (EmpSystemId) {
        $scope.ResetPFSettingModel();
        try {
            $http.get('humanresource/employeepromotion/LoadEmpSalaryInfoDefineData?EmpSystemId=' + EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure','EntryDivM');
                    }
                    else {
                        $scope.givenDesignationChange();
                        $scope.Calculated = false;
                        $scope.NewNetGross = null;
                        $scope.NewNetCTC = null;
                        $scope.EmpSalaryInfoDefine = null;
                        $scope.EmpSalaryOpenHeadCurrent = null;
                        $scope.EmpSalaryApprovedOpenHead = null;
                        $scope.EmpSalaryInfoDefine = null;
                        $scope.MinWage = null;
                        $scope.SalaryRole = null;
                        $scope.SelectedSalaryRole = null;
                        $scope.EmpSalaryOpenHead = null;
                        $scope.newFormula_Desc = null;
                        $scope.approvedFormula_Desc = null;
                        $scope.UnApprovedNextDueDate = null;
                        $scope.approvalStatusNew = null;
                        $scope.approvalStatusApproved = null;



                        $scope.EmpSalaryInfoDefine = response.data.EmpApprovedSalaryInfoDefine;
                        $scope.MinWage = response.data.ResultMinWage;
                        $scope.SalaryRole = response.data.ResultSalaryRule;
                        $scope.SelectedSalaryRole = response.data.ResultSelectedSalaryRule;
                        ///EmpSalaryInfo.SalaryRuleMasterSystemID= $scope.getSalaryRuleMasterSystemIDBygivenDesignation($scope.budgetCodeChangeNew.GivenDesignationId);

                        $scope.EmpSalaryOpenHead = response.data.ResultOpenHead;
                        $scope.EmpSalaryApprovedOpenHead = response.data.ResultApprovedOpenHead;
                        $scope.EmpSalaryOpenHeadCurrent = response.data.ResultOpenHead;
                        $scope.newFormula_Desc = response.data.NewFormula_Desc;
                        $scope.approvedFormula_Desc = response.data.ApprovedFormula_Desc;
                        //$scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data.ResultSelectedSalaryRule[0].SalaryRuleMasterSystemID;
                        if (baseService.isUndefinedOrNull(response.data.ResultOpenHead[0].SalaryID)) {
                            $scope.EmpSalaryInfo.SalaryID = null;
                        } else {
                            $scope.EmpSalaryInfo.SalaryID = response.data.ResultOpenHead[0].SalaryID;
                        }

                        if (baseService.isUndefinedOrNull(response.data.ApprovalStatus)) {
                            $scope.EmpSalaryInfo.SalaryApprovedStatus = null;
                        } else {
                            $scope.EmpSalaryInfo.SalaryApprovedStatus = response.data.ApprovalStatus;
                        }
                        //-------
                        if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {//--- for Confirmation

                            $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')($scope.DOC, 'dd-M-yyyy');

                        }
                        else { //--- for increment
                            if (baseService.isUndefinedOrNull(response.data.ResultEffectiveDate)) {
                                $scope.EmpSalaryInfo.EffectiveDate = null;
                            } else {
                                $scope.EmpSalaryInfo.EffectiveDate = response.data.ResultEffectiveDate;
                            }
                        }



                        if (baseService.isUndefinedOrNull(response.data.ApprovedNextDueDate)) {
                            $scope.ApprovedNextDueDate = null;
                        } else {
                            $scope.ApprovedNextDueDate = response.data.ApprovedNextDueDate;
                        }
                        if (baseService.isUndefinedOrNull(response.data.UnApprovedNextDueDate)) {
                            $scope.EmpSalaryInfo.NextDueDate = null;
                        } else {
                            $scope.EmpSalaryInfo.NextDueDate = response.data.UnApprovedNextDueDate;
                        }



                        if (baseService.isUndefinedOrNull(response.data.ApprovedEffectiveDate)) {
                            $scope.ApprovedEffectiveDate = null;
                        } else {
                            $scope.ApprovedEffectiveDate = response.data.ApprovedEffectiveDate;
                        }
                        if (baseService.isUndefinedOrNull(response.data.ResultGross)) {
                            $scope.NetGross = null;
                        } else {
                            $scope.NetGross = response.data.ResultGross;
                        }
                        if (baseService.isUndefinedOrNull(response.data.ResultNetCTC)) {
                            $scope.NetCTC = null;
                        } else {
                            $scope.NetCTC = response.data.ResultNetCTC;
                        }
                        if (baseService.isUndefinedOrNull(response.data.IsSalaryRuleEditableEmployee)) {
                            $scope.IsSalaryRuleEditableEmployee = false;
                        } else {
                            $scope.IsSalaryRuleEditableEmployee = !response.data.IsSalaryRuleEditableEmployee;
                        }

                        //Increment History
                        if (!baseService.isUndefinedOrNull($scope.EmpSalaryApprovedOpenHead)) {
                            $scope.approvalStatusApproved = 'Approved';
                        }


                        $scope.IncrementHistory.EmpSystemID = EmpSystemId;
                        $scope.IncrementHistory.FromEffectiveDate = $scope.ApprovedEffectiveDate;

                        if (!baseService.isUndefinedOrNull(response.data.ResultApprovedOpenHead)) {
                            $scope.IncrementHistory.FromSalaryId = response.data.ResultApprovedOpenHead[0].SalaryID;
                        }

                        // $scope.EmpSalaryInfo.EffectiveDate = $filter('dateFiltering')(response.data.ResultOpenHead[0].EffectiveDate, 'dd-M-yyyy');
                        if (response.data.ApprovalStatus !== 'Approved') {
                            for (var i = 0; i < $scope.EmpSalaryOpenHead.length; i++) {

                                $scope.EmpSalaryOpenHead[i].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHead[i].EffectiveDate, 'dd-M-yyyy');
                                //if ($scope.EmpSalaryOpenHead[i].Amount === null) {
                                //    $scope.EmpSalaryOpenHead[i].OldAmount = 0;
                                //}
                                //else {
                                //    $scope.EmpSalaryOpenHead[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                                //    $scope.EmpSalaryOpenHead[i].Amount = null;
                                //}
                                if ($scope.EmpSalaryOpenHead[i].Amount === null) {
                                    $scope.EmpSalaryOpenHead[i].Amount = 0;
                                }
                                else {
                                    $scope.EmpSalaryOpenHead[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                                    //$scope.EmpSalaryOpenHead[i].Amount = null;
                                }
                            }
                            $scope.approvalStatusNew = 'Un-Approved';

                        }
                        else {
                            for (var j = 0; j < $scope.EmpSalaryOpenHead.length; j++) {

                                $scope.EmpSalaryOpenHead[j].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHead[j].EffectiveDate, 'dd-M-yyyy');
                                if ($scope.EmpSalaryOpenHead[j].Amount === null) {
                                    $scope.EmpSalaryOpenHead[j].OldAmount = 0;
                                }
                                else {
                                    $scope.EmpSalaryOpenHead[j].OldAmount = $scope.EmpSalaryOpenHead[j].Amount;
                                    $scope.EmpSalaryOpenHead[j].Amount = null;
                                }

                            }
                        }
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                });


        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };
    $scope.getEmpDataById = function (EmpSystemId) {
        try {
            $http.get('humanresource/employeepromotion/GetSalaryStrcApprovedEmployeeById?EmpSystemId=' + EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                    else {
                        if (baseService.isUndefinedOrNull(response.data[0])) {
                            throw "Invalid User.";
                        }
                        $scope.employees = response.data[0];

                        $scope.Get(null, 0);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                });


        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };
    $scope.LoadEmployeeDataForGrid = function (url) {
        try {
            $http.get(url)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                    else {
                        $scope.employees = null;
                        $scope.employees = response.data;


                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                });


        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };
    $scope.calculateSalary = function () {
        //$scope.EmpSalaryOpenHead
        try {
            //angular.forEach($scope.EmpSalaryOpenHead,
            //    function (item) {
            //        if (item.NewAmount == null){
            //            $scope.EmpSalaryOpenHead.Amount = 0;
            //        }
            //        else {
            //            $scope.EmpSalaryOpenHead.Amount = item.NewAmount;;

            //        }

            //    });

            //for (var i = 0; i < $scope.EmpSalaryOpenHead.length; i++) {
            //    if ($scope.EmpSalaryOpenHead[i].NewAmount == null) {
            //        $scope.EmpSalaryOpenHead[i].Amount = 0;
            //    }
            //    else {
            //        $scope.EmpSalaryOpenHead[i].Amount = $scope.EmpSalaryOpenHead[i].NewAmount;;

            //    }
            //}



            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.CalculateSalaryUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'SalaryRuleMasterSystemID': $scope.EmpSalaryInfo.SalaryRuleMasterSystemID, 'EmpSalaryOpenHeadNew': $scope.EmpSalaryOpenHeadCurrent, 'IsbuttonPFClicked': $scope.PFSettingModel.IsbuttonPFClicked, 'IsPFEntitle': $scope.PFSettingModel.IsPFEntitle },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    $scope.EmpSalaryInfoDefineNew = null;
                    $scope.EmpSalaryInfoDefineNew = response.data.EmpSalaryInfoDefine;
                    $scope.NewNetGross = response.data.newGross;
                    $scope.NewNetCTC = response.data.newCTC;
                    $scope.newFormula_Desc = response.data.newFormula_Desc;
                    $scope.EmpSalaryInfo.SalaryApprovedStatus = 'UnApproved';
                    $scope.Calculated = true;
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure", 'EntryDivM');
                    }
                    else {
                        //$scope.ShowResultCustom(response.data.Message, "success", 'EntryPopUp');
                        $scope.Clear();

                    }
                    // pf
                    if ($scope.PFSettingModel.IsbuttonPFClicked === 'YES') {
                        if ($scope.PFSettingModel.IsPFEntitle === false) {
                            for (var i = 0; i < $scope.EmpSalaryInfoDefineNew.length; i++) {


                                if ($scope.EmpSalaryInfoDefineNew[i].SalaryCategory === 'PF') {
                                    $scope.EmpSalaryInfoDefineNew[i].EntryAmount = 0;
                                    $scope.EmpSalaryInfoDefineNew[i].DefineAmount = 0;
                                }
                            }
                        }
                    }


                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", 'EntryDivM');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };



    $scope.showFormula = function (type) {

        try {



            $scope.messageTitle = " ";
            if (type === "newFormula") {
                $scope.messageTitle = "New Salary Calculation Formula";
                $scope.messageText = $scope.newFormula_Desc;
            }
            if (type === "approvedFormula") {
                $scope.messageText = $scope.approvedFormula_Desc;
                $scope.messageTitle = "Current Salary Calculation Formula";
                //$("#dialogMessage").ejDialog("setTitle", "Error");
            }
            $("#dialogFormula").ejDialog("setTitle", $scope.messageTitle);
            var eDialog = $("#dialogFormula").data("ejDialog");
            eDialog.open();


        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };


    $scope.givenDesignationChange = function () {
        $http.get('humanresource/employeepromotion/GivenDesignationChange?GivenDesignationId=' + $scope.budgetCodeChangeNew.GivenDesignationId)
            .then(function (response) {

                $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data[0].SalaryRuleMasterId;


            });
    };


    $scope.getSalaryRuleMasterSystemIDBygivenDesignation = function (id) {
        $http.get('humanresource/employeepromotion/GivenDesignationChange?GivenDesignationId=' + id)
            .then(function (response) {
                return response.data[0].SalaryRuleMasterId;
            });
    };
    $scope.xgivenDesignationChange = function () {
        $http.get('humanresource/employeepromotion/GivenDesignationChange?GivenDesignationId=' + $scope.budgetCodeChangeNew.GivenDesignationId)
            .then(function (response) {

                $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data.ResultData[0].SalaryRuleMasterSystemID;


            });
    };
    $scope.SalaryRuleChange = function () {
        $http.get('humanresource/employeepromotion/SalaryRuleChange?EmpSystemId=' + $scope.budgetCodeChangeNew.SystemId + '&SalaryRuleId=' + $scope.EmpSalaryInfo.SalaryRuleMasterSystemID)
            .then(function (response) {
                //$scope.EmpSalaryOpenHead = null;
                //$scope.EmpSalaryOpenHead = response.data.ResultOpenHead;
                //for (var i = 0; i < $scope.EmpSalaryOpenHead.length; i++) {

                //    $scope.EmpSalaryOpenHead[i].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHead[i].EffectiveDate, 'dd-M-yyyy');
                //    if ($scope.EmpSalaryOpenHead[i].Amount === null) {
                //        $scope.EmpSalaryOpenHead[i].OldAmount = 0;
                //    }
                //    else {
                //        $scope.EmpSalaryOpenHead[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                //        $scope.EmpSalaryOpenHead[i].Amount = null;
                //    }
                //}



                $scope.EmpSalaryOpenHeadCurrent = null;
                $scope.EmpSalaryOpenHeadCurrent = response.data.ResultOpenHead;
                $scope.newFormula_Desc = response.data.newFormula_Desc;
                for (var i = 0; i < $scope.EmpSalaryOpenHeadCurrent.length; i++) {

                    $scope.EmpSalaryOpenHeadCurrent[i].EffectiveDate = $filter('dateFiltering')($scope.EmpSalaryOpenHeadCurrent[i].EffectiveDate, 'dd-M-yyyy');
                    if ($scope.EmpSalaryOpenHeadCurrent[i].Amount === null) {
                        $scope.EmpSalaryOpenHeadCurrent[i].OldAmount = 0;
                    }
                    else {
                        $scope.EmpSalaryOpenHeadCurrent[i].OldAmount = $scope.EmpSalaryOpenHead[i].Amount;
                        $scope.EmpSalaryOpenHeadCurrent[i].Amount = null;
                    }
                }

            });
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


    function checkValidation() {
        CheckField($scope.budgetCodeChangeNew.BudgetId, "Budget Code");
        CheckField($scope.budgetCodeChangeNew.GivenDesignationId, "Given Designation");
        CheckField($scope.budgetCodeChangeNew.Gender, "Gender");
        CheckField($scope.budgetCodeChangeNew.FullName, "Full Name");
        CheckField($scope.budgetCodeChangeNew.EmpType, "Emp Type");
        CheckField($scope.budgetCodeChangeNew.Email, "Email");
        CheckField($scope.budgetCodeChangeNew.Phone, "Phone");
        CheckField($scope.budgetCodeChangeNew.AgreedDOJ, "Agreed DOJ");
        CheckField($scope.budgetCodeChangeNew.InterviewRankingId, "Rank");
        CheckField($scope.budgetCodeChangeNew.Status, "Status");

        if (isNaN($scope.budgetCodeChangeNew.Phone)) {
            throw "Enter valid phone number";
        }
        if (isNaN($scope.budgetCodeChangeNew.NationalID)) {
            throw "Enter valid national id";
        }
        if (isNaN($scope.budgetCodeChangeNew.TotalSalary)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.budgetCodeChangeNew.SpecialReviewDuration)) {
            throw "Enter valid number";
        }
        if (isNaN($scope.budgetCodeChangeNew.SpecialReviewAmount)) {
            throw "Enter valid number";
        }
        if ($scope.budgetCodeChangeNew.TotalSalary < 1) {
            throw "Total salary can not less than 1.";
        }
        if ($scope.budgetCodeChangeNew.SpecialReviewAmount < 0) {
            throw "Special review amount can not less than 0.";
        }
        if ($scope.budgetCodeChangeNew.SpecialReviewDuration < 0) {
            throw "Special review duration can not less than 0.";
        }

        var _ad = new Date($scope.budgetCodeChangeNew.AgreedDOJ);
        var _db = new Date($scope.budgetCodeChangeNew.DOB);

        var ad = $filter('dateFiltering')(_ad, 'dd-MMM-yyyy');
        var db = $filter('dateFiltering')(_db, 'dd-MMM-yyyy');

        if (_ad < _db) {
            throw "Date of birth [" + db + "] can not be greater than Agreed Date of join [" + ad + "]";
        }
    }

    $scope.NewbudgetCodeChange = {
        EntityName: null,
        Designation: null,
        PositionName: null,
        DesignationId: null
    };

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

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
                    ShowResult(commonMessage.NetworkError, 'failure', 'EntryDivM');
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


    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

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

    //$scope.xGet = function (data, index) {
    //    //Confirmation
    //    if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {
    //        $scope.divFreshEntry = false;


    //        //$scope.budgetCodeChange = $filter("filter")($scope.employees, { SystemId: $scope.qempid });
    //        //$scope.budgetCodeChangeOld = $filter("filter")($scope.employees, { SystemId: $scope.qempid });

    //        $scope.budgetCodeChange = $scope.employees;
    //        $scope.budgetCodeChangeOld = $scope.employees;
    //        // angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);
    //        $scope.budgetCodeChangeNew = $scope.budgetCodeChange;
    //        $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
    //        $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

    //        $scope.Action = 'Update';
    //        $scope.EntryShow();
    //        $scope.getEmpSalaryInfoDefineData($scope.qempid);
    //    }
    //    else {
    //        $scope.index = index;
    //        $scope.budgetCodeChange = $scope.employees[$scope.index];
    //        $scope.budgetCodeChangeOld = $scope.employees[$scope.index];

    //        //if (data.IsExceptionalDesigApplicable === 1)
    //        //    $scope.budgetCodeChange.IsExceptionalDesigApplicable = true;
    //        //else if (data.IsExceptionalDesigApplicable === 0)
    //        //    $scope.budgetCodeChange.IsExceptionalDesigApplicable = false;

    //        //if ($scope.budgetCodeChange.IsExceptionalDesigApplicable)
    //        //    $scope.uppderGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
    //        //else
    //        //    $scope.lowerGivenDesignationCbo(data.DesignationId, data.GivenDesignationId);
    //        angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);
    //        $scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
    //        $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

    //        $scope.Action = 'Update';
    //        $scope.EntryShow();
    //        $scope.getEmpSalaryInfoDefineData(data.SystemId);
    //    }




    //};
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
            if ($scope.model2.Promotion === true && $scope.model2.Increment === false) {
                $scope.IncrementHistory.IsPromotion = true;

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
                }

            }
            $scope.ResetPFSettingModel();



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.recorddoubleclick = function (args) {
        Get(args);
    };


    $scope.Update = function () {
        try {

            //Increment History
            $scope.IncrementHistory.EmpSystemID = $scope.budgetCodeChangeOld.SystemId;
            $scope.IncrementHistory.FromGivenDesignationId = $scope.budgetCodeChangeOld.GivenDesignationId;
            $scope.IncrementHistory.FromBudgetCode = $scope.budgetCodeChangeOld.BudgetCode;
            $scope.IncrementHistory.FromLegalDesignationId = $scope.budgetCodeChangeOld.LegalDesignationId;
            $scope.IncrementHistory.ToGivenDesignationId = $scope.NewbudgetCodeChange.GivenDesignationId;
            $scope.IncrementHistory.ToBudgetCode = $scope.NewbudgetCodeChange.BudgetCode;
            $scope.IncrementHistory.ToLegalDesignationId = $scope.NewbudgetCodeChange.LegalDesignationId;
            $scope.IncrementHistory.ToEffectiveDate = $scope.EmpSalaryInfo.EffectiveDate;


            //if ($scope.budgetCodeChangeNew.IsExceptionalDesigApplicable === true)
            //    $scope.budgetCodeChangeNew.IsExceptionalDesigApplicable = 1;
            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'IncrementHistory': $scope.IncrementHistory },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Clear();
                        //angular.element(document.querySelector('#EntryPopUp')).modal('hide');
                        $scope.getData();


                        $scope.NewbudgetCodeChange = {};
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, "failure");
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.UpdateSalaryStracture = function () {
        try {

            if ($scope.Action === "Update") {
                //$http({
                //    method: 'POST',
                //    url: $scope.UpdateSalaryStractureUrl,
                //    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'EmpSalaryInfo': $scope.EmpSalaryInfo, 'EmpSalaryInfoDefineNew': $scope.EmpSalaryInfoDefineNew, 'IncrementHistory': $scope.IncrementHistory },
                //    dataType: 'JSON'
                //}).then(function successCallback(response) {
                //    if (response.data.Error === true) {
                //        $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                //    }
                //    else {
                //        $scope.ShowResultCustom(response.data.Message, "success", 'EntryDiv');
                //        ////$scope.Clear();
                //        /// angular.element(document.querySelector('#EntryPopUp')).modal('hide');
                //        $scope.Calculated = false;

                //    }
                //}, function errorCallback(response) {
                //    $scope.ShowResultCustom(response.status.Message, "failure", 'EntryDiv');
                //});
                //return true;
                $.ajax({
                    type: "POST",
                    url: $scope.UpdateSalaryStractureUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'EmpSalaryInfo': $scope.EmpSalaryInfo, 'EmpSalaryInfoDefineNew': $scope.EmpSalaryInfoDefineNew, 'IncrementHistory': $scope.IncrementHistory, 'PFSettingModel': $scope.PFSettingModel },
                    dataType: "json",
                    success: function (data) {
                        ShowResult(data.Message, "success");
                        if (data.Error === true) {
                            //$scope.ShowResultCustom(data.Message, "failure", 'EntryDiv');
                            ShowResult(data.Message, "failure");
                        }
                        else {
                            ShowResult(data.Message, "success");
                        }
                    }

                });


            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.xUpdateSalaryStracture = function () {
        try {

            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.UpdateSalaryStractureUrl,
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'EmpSalaryInfo': $scope.EmpSalaryInfo, 'EmpSalaryInfoDefineNew': $scope.EmpSalaryInfoDefineNew, 'IncrementHistory': $scope.IncrementHistory },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                    }
                    else {
                        $scope.ShowResultCustom(response.data.Message, "success", 'EntryDiv');
                        ////$scope.Clear();
                        /// angular.element(document.querySelector('#EntryPopUp')).modal('hide');
                        $scope.Calculated = false;

                    }
                }, function errorCallback(response) {
                    $scope.ShowResultCustom(response.status.Message, "failure", 'EntryDiv');
                });
                return true;
            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
        }
    };
    $scope.xSaveData = function () {

        $.ajax({
            type: "POST",
            url: $scope.OTConfirmationSaveUrl,
            data:
            {
                'employeeOTInformation': $scope.employees,
                'ProcDate': $scope.customPara.procdate
            },
            dataType: "json",
            success: function (data) {
            }

        });

    };


    $scope.CloseModal = function (id) {
        angular.element(document.querySelector(id)).modal('hide');
        angular.element(document.querySelector('#EntryPopUp')).modal('show');
    };


    // #endregion

    //PF Setting
    $scope.PFSettingModel = {
        IsPFEntitle: false,
        IsPFNotEntitleGetAllo: false,
        PFEffectiveDate: null,
        IsbuttonPFClicked: 'NO',
        EmpSystemId: null
    };
    $scope.ResetPFSettingModel = function () {
        $scope.PFSettingModel.IsPFEntitle = false;
        $scope.PFSettingModel.IsPFNotEntitleGetAllo = false;
        $scope.PFSettingModel.PFEffectiveDate = null;
        $scope.PFSettingModel.IsbuttonPFClicked = 'NO';
        $scope.PFSettingModel.EmpSystemId = null;
    };

    $scope.ShowPFSetting = function () {

        var eDialog = $("#dialogPFSetting").data("ejDialog");
        eDialog.open();


        try {


            //$http.get('humanresource/employeepromotion/ShowPFSetting?EmpSystemId=' + $scope.budgetCodeChangeOld.SystemId + '&PFSettingModel=' + $scope.PFSettingModel)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            $scope.ShowResultCustom(response.data.Message, 'failure');
            //        }
            //        else {                        

            //            $scope.PFSettingModel = response.data.PFCheckAndUnCheck;  
            //            $scope.PFSettingModel.IsbuttonPFClicked = 'YES';

            //        }
            //        function errorCallBack(response) {
            //            $scope.ShowResultCustom(response.data.Message, 'failure');
            //        }
            //    });


            $.ajax({
                type: "POST",
                url: "humanresource/employeepromotion/ShowPFSetting",
                data: { 'EmpSystemId': $scope.budgetCodeChangeOld.SystemId, 'PFSettingModel': $scope.PFSettingModel },
                dataType: "json",
                success: function (data) {
                    $scope.PFSettingModel = data.PFCheckAndUnCheck;
                    $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
                }

            });


        } catch (e) {
            ShowResult(e, "failure",'EntryDivM');
        }
    };

    $scope.PFEntitleChange = function () {
        if ($scope.PFSettingModel.IsPFEntitle === false) {
            $scope.PFSettingModel.PFEffectiveDate = null;
        };
    };

    $scope.PFCheckAndUnCheckDone = function () {




        try {

            var eDialog = $("#dialogPFSetting").data("ejDialog");
            eDialog.close();
            $http.get('humanresource/employeepromotion/PFCheckAndUnCheckDone?EmpSystemId=' + $scope.budgetCodeChangeOld.SystemId + '&IsbuttonPFClicked=' + $scope.PFSettingModel.IsbuttonPFClicked + '&IsPFEntitle=' + $scope.PFSettingModel.IsPFEntitle + '&PFEffectiveDate=' + $scope.PFSettingModel.PFEffectiveDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'EntryDivM');
                    }
                });


        } catch (e) {
            ShowResult(e, "failure", 'EntryDivM');
        }
    };











}