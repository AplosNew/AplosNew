'use strict';
employeePromotionNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', '$window'];

function employeePromotionNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $compile, $window) {
    $rootScope.title = 'Employee Promotion';
    $scope.index = -1;
    $scope.employees = [];
    $scope.EmpSalaryInfoDefine = [];
    $scope.path = 'humanresource/employeepromotionNew/';
    $scope.getApprovedEmpListUrl = $scope.path + 'GetSalaryStrcApprovedEmployeeList';
    $scope.getUnApprovedEmpListUrl = $scope.path + 'GetSalaryStrcUnApprovedEmployeeList';
    $scope.updateUrl = $scope.path + 'update';
    $scope.UpdateSalaryStractureUrl = $scope.path + 'UpdateSalaryStracture';
    $scope.CalculateSalaryUrl = $scope.path + 'CalculateSalary';
    $scope.LoadEmpSalaryInfoDefineDataUrl = $scope.path + 'LoadEmpSalaryInfoDefineData';
    $scope.GetLegalDesignationUrl = $scope.path + 'GetLegalDesignation';

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


    $scope.ShowPFButton = false;


    $scope.model2 = {
        Increment: false,
        Promotion: false,
        Adjustment: false

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
        $scope.model2.Adjustment = false;
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
            url: "humanresource/employeepromotionNew/save",
            data: JSON.stringify(data),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                $scope.ShowResultCustom(response.data.Message, 'failure');
            }
            else {
                $scope.ShowResultCustom(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            $scope.ShowResultCustom(response.data.Message, 'failure');
        };
    };

    $scope.Clear = function () {
        $scope.employee = {};
        return true;
    };

    //Employee Load
    $scope.IsApprovedEmpList = false;
    $scope.employees = [];


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
        IsPromotion: null,
        LegalSalaryGradeId: null,
        NewLegalSalaryGradeId: null
    };
    $scope.Calculated = false;
    $scope.MinWage = null;
    $scope.NetGross = null;
    $scope.NetCTC = null;
    $scope.NetTotalGross = null;
    $scope.NewNetTotalGross = null;
    $scope.NewNetGross = null;
    $scope.NewNetCTC = null;
    $scope.newFormula_Desc = null;
    $scope.approvedFormula_Desc = null;
    $scope.ApprovedNextDueDate = null;
    $scope.ApprovedEffectiveDate = null;
    $scope.UnApprovedNextDueDate = null;

    $scope.IsSalaryRuleEditableEmployee = false;
    $scope.IsFreshEntry = false;
    $scope.getEmpSalaryInfoDefineData = function (EmpSystemId) {
        $scope.ShowPFButton = false;
        $scope.ResetPFSettingModel();
        try {
            $http.get('humanresource/employeepromotionNew/LoadEmpSalaryInfoDefineData?EmpSystemId=' + EmpSystemId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, 'failure');
                    }
                    else {
                        $scope.givenDesignationChange();
                        $scope.Calculated = false;
                        $scope.NewNetGross = null;
                        $scope.NewNetCTC = null;
                        //$scope.EmpSalaryInfoDefine = [];
                        $scope.EmpSalaryOpenHeadCurrent = {};
                        $scope.EmpSalaryApprovedOpenHead = null;
                        $scope.EmpSalaryInfoDefine = null;
                        $scope.MinWage = null;
                        $scope.SalaryRole = null;
                        $scope.SelectedSalaryRole = null;
                        $scope.EmpSalaryOpenHead = {};
                        $scope.newFormula_Desc = null;
                        $scope.approvedFormula_Desc = null;
                        $scope.UnApprovedNextDueDate = null;
                        $scope.approvalStatusNew = null;
                        $scope.approvalStatusApproved = null;

                        $scope.NewNetTotalGross = null;
                        $scope.NewNetGross = null;
                        $scope.NewNetCTC = null;
                        $scope.IsFreshEntry = false;
                        if (baseService.isUndefinedOrNull(response.data.EmpApprovedSalaryInfoDefine)) {
                            $scope.EmpSalaryInfoDefine = [];
                        } else {
                            $scope.EmpSalaryInfoDefine = response.data.EmpApprovedSalaryInfoDefine;
                        }

                        $scope.IsFreshEntry = response.data.IsFreshEntry;
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

                        for (var i = 0; i < $scope.EmpSalaryInfoDefine.length; i++) {

                            if ($scope.EmpSalaryInfoDefine[i].HeadCategory.toUpperCase() === 'CTC') {
                                $scope.NetCTC = $scope.EmpSalaryInfoDefine[i].EntryAmount;
                            }
                            if ($scope.EmpSalaryInfoDefine[i].HeadCategory.toUpperCase() === 'GROSS') {
                                $scope.NetGross = $scope.EmpSalaryInfoDefine[i].EntryAmount;
                            }
                            if ($scope.EmpSalaryInfoDefine[i].HeadCategory.toUpperCase() === 'TOTAL GROSS') {
                                $scope.NetTotalGross = $scope.EmpSalaryInfoDefine[i].EntryAmount;
                            }
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


                        $scope.VPFPercentageModel = response.data.VPFPersentage;
                        $scope.VPFEffectiveDateModel = response.data.VPFEffectiveDate;


                        if (!baseService.isUndefinedOrNull(response.data.IsPFEntitle)) {
                            $scope.PFSettingModel.IsPFEntitle = response.data.IsPFEntitle;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsESICEntitle)) {
                            $scope.PFSettingModel.IsESICEntitle = response.data.IsESICEntitle;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsVPFEntitle)) {
                            $scope.PFSettingModel.IsVPFEntitle = response.data.IsVPFEntitle;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsBonusEntitle)) {
                            $scope.PFSettingModel.IsBonusEntitle = response.data.IsBonusEntitle;
                        }


                        if (!baseService.isUndefinedOrNull(response.data.IsPFMandatory)) {
                            $scope.PFSettingModel.IsPFMandatoryNew = response.data.IsPFMandatory;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsVPFMandatory)) {
                            $scope.PFSettingModel.IsVPFMandatoryNew = response.data.IsVPFMandatory;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsESICMandatory)) {
                            $scope.PFSettingModel.IsESICMandatoryNew = response.data.IsESICMandatory;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsBonusMandatory)) {
                            $scope.PFSettingModel.IsBonusMandatoryNew = response.data.IsBonusMandatory;
                        }

                        if (!baseService.isUndefinedOrNull(response.data.IsBonusPolicyDefined)) {
                            $scope.PFSettingModel.IsBonusPolicyDefined = response.data.IsBonusPolicyDefined;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsESICPolicyDefined)) {
                            $scope.PFSettingModel.IsESICPolicyDefined = response.data.IsESICPolicyDefined;
                        }
                        if (!baseService.isUndefinedOrNull(response.data.IsPFPolicyDefined)) {
                            $scope.PFSettingModel.IsPFPolicyDefined = response.data.IsPFPolicyDefined;
                        }

                        $scope.LoadAdditionalPolicySettingData();
                        $scope.UpdateAdditionalPolicyModel();
                        //$scope.LoadPFSetting();
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
            $http.get('humanresource/employeepromotionNew/GetSalaryStrcApprovedEmployeeById?EmpSystemId=' + EmpSystemId)
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
    $scope.calculateSalary = function () {
        //$scope.EmpSalaryOpenHead
        try {
            //angular.forEach($scope.EmpSalaryOpenHead,
            //    function (item) {
            //        if (item.NewAmount == null){
            //            $scope.EmpSalaryOpenHead.Amount = 0;
            //        }
            //        else {
            //            $scope.EmpSalaryOpenHead.Amount = item.NewAmount;

            //        }

            //    });

            //for (var i = 0; i < $scope.EmpSalaryOpenHeadCurrent.length; i++) {
            //    if ($scope.EmpSalaryOpenHead[i].OldAmount === null) {
            //        $scope.EmpSalaryOpenHead[i].Amount = 0;
            //    }
            //    else {
            //        $scope.EmpSalaryOpenHead[i].Amount = $scope.EmpSalaryOpenHead[i].OldAmount;

            //    }
            //}
            if ($scope.ShowPFButton === false) {
                $scope.SettingModel = [];
                $scope.PFSettingModel.IsPFEntitle = false;
                $scope.PFSettingModel.IsESICEntitle = false;
                $scope.PFSettingModel.IsbuttonPFClicked = 'NO';
                $scope.PFSettingModel.IsVPFEntitle = false;
                $scope.PFSettingModel.IsBonusEntitle = false;
            } else {
                if ($scope.IsFreshEntry === false) {
                    for (var i = 0; i < $scope.SettingModel.length; i++) {


                        if ($scope.SettingModel[i].SalaryHeadEnum === 'PF') {
                            $scope.PFSettingModel.IsPFEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'ESIC') {
                            $scope.PFSettingModel.IsESICEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'VPF') {
                            $scope.PFSettingModel.IsVPFEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'BonusRetain') {
                            $scope.PFSettingModel.IsBonusEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                    }
                    $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
                }


                if ($scope.PFSettingModel.IsbuttonPFClicked === 'YES') {
                    for (var i = 0; i < $scope.SettingModel.length; i++) {


                        if ($scope.SettingModel[i].SalaryHeadEnum === 'PF') {
                            $scope.PFSettingModel.IsPFEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'ESIC') {
                            $scope.PFSettingModel.IsESICEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'VPF') {
                            $scope.PFSettingModel.IsVPFEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                        if ($scope.SettingModel[i].SalaryHeadEnum === 'BonusRetain') {
                            $scope.PFSettingModel.IsBonusEntitle = $scope.SettingModel[i].IsEntitle;

                        }
                    }
                    $scope.PFSettingModel.IsbuttonPFClicked = 'YES';

                }
                else {
                    ///
                }

            }








            if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.CalculateSalaryUrl,
                    data: {
                        'employeeInformation': $scope.budgetCodeChangeNew
                        , 'SalaryRuleMasterSystemID': $scope.EmpSalaryInfo.SalaryRuleMasterSystemID
                        , 'EmpSalaryOpenHeadNew': $scope.EmpSalaryOpenHeadCurrent
                        , 'IsbuttonPFClicked': $scope.PFSettingModel.IsbuttonPFClicked
                        , 'IsPFEntitle': $scope.PFSettingModel.IsPFEntitle
                        , 'IsESICEntitle': $scope.PFSettingModel.IsESICEntitle
                        , 'IsVPFEntitle': $scope.PFSettingModel.IsVPFEntitle
                        , 'VPFPescentage': $scope.VPFPercentageModel
                        , 'IsBonusEntitle': $scope.PFSettingModel.IsBonusEntitle
                        , 'EffectiveDate': $scope.EmpSalaryInfo.EffectiveDate
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    $scope.EmpSalaryInfoDefineNew = null;
                    $scope.EmpSalaryInfoDefineNew = response.data.EmpSalaryInfoDefine;
                    //$scope.NewNetGross = response.data.newGross;
                    //$scope.NewNetCTC = response.data.newCTC;

                    try {
                        $scope.NewNetTotalGross = null;
                        $scope.NewNetGross = null;
                        $scope.NewNetCTC = null;
                        for (var i = 0; i < $scope.EmpSalaryInfoDefineNew.length; i++) {

                            if ($scope.EmpSalaryInfoDefineNew[i].HeadCategory.toUpperCase() === 'CTC') {
                                $scope.NewNetCTC = $scope.EmpSalaryInfoDefineNew[i].EntryAmount;
                            }
                            if ($scope.EmpSalaryInfoDefineNew[i].HeadCategory.toUpperCase() === 'GROSS') {
                                $scope.NewNetGross = $scope.EmpSalaryInfoDefineNew[i].EntryAmount;
                            }
                            if ($scope.EmpSalaryInfoDefineNew[i].HeadCategory.toUpperCase() === 'TOTAL GROSS') {
                                $scope.NewNetTotalGross = $scope.EmpSalaryInfoDefineNew[i].EntryAmount;
                            }
                        }
                    } catch (e) {
                        ///
                    }


                    $scope.newFormula_Desc = response.data.newFormula_Desc;
                    $scope.EmpSalaryInfo.SalaryApprovedStatus = 'UnApproved';
                    $scope.Calculated = true;

                    $scope.PFSettingModel.IsPFEntitle = response.data.IsPFEntitleNew;
                    $scope.PFSettingModel.IsESICEntitle = response.data.IsESICEntitleNew;
                    $scope.PFSettingModel.IsVPFEntitle = response.data.IsVPFEntitleNew;
                    $scope.PFSettingModel.IsBonusEntitle = response.data.IsBonusEntitleNew;




                    $scope.PFSettingModel.IsPFMandatoryNew = response.data.IsPFMandatoryNew;
                    $scope.PFSettingModel.IsVPFMandatoryNew = response.data.IsVPFMandatoryNew;
                    $scope.PFSettingModel.IsESICMandatoryNew = response.data.IsESICMandatoryNew;
                    $scope.PFSettingModel.IsBonusMandatoryNew = response.data.IsBonusMandatoryNew;





                    $scope.PFSettingModel.IsBonusPolicyDefined = response.data.IsBonusPolicyDefined;
                    $scope.PFSettingModel.IsESICPolicyDefined = response.data.IsESICPolicyDefined;
                    $scope.PFSettingModel.IsPFPolicyDefined = response.data.IsPFPolicyDefined;

                    $scope.PFSettingModel.IsPFEntitle = response.data.IsPFEntitleNew;
                    $scope.PFSettingModel.IsESICEntitle = response.data.IsESICEntitleNew;
                    $scope.PFSettingModel.IsVPFEntitle = response.data.IsVPFEntitleNew;
                    $scope.PFSettingModel.IsBonusEntitle = response.data.IsBonusEntitleNew;



                    //  if ($scope.PFSettingModel.IsbuttonPFClicked === 'NO') {
                    ///    $scope.PFSettingModel.IsPFEntitle = response.data.IsPFEntitleNew;
                    ///    $scope.PFSettingModel.IsESICEntitle = response.data.IsESICEntitleNew;
                    //    $scope.PFSettingModel.IsVPFEntitle = response.data.IsVPFEntitleNew;
                    //        $scope.PFSettingModel.IsBonusEntitle = response.data.IsBonusEntitleNew;
                    //  }
                    //$scope.ShowPFButton = true;

                    //$scope.PFSettingModel.IsbuttonPFClicked = 'YES';

                    // pf
                    //if ($scope.PFSettingModel.IsbuttonPFClicked === 'YES') {
                    try {
                        if ($scope.PFSettingModel.IsPFEntitle === false) {
                            for (var i = 0; i < $scope.EmpSalaryInfoDefineNew.length; i++) {


                                if ($scope.EmpSalaryInfoDefineNew[i].SalaryCategory.toUpperCase() === 'PF') {
                                    $scope.EmpSalaryInfoDefineNew[i].EntryAmount = 0;
                                    $scope.EmpSalaryInfoDefineNew[i].DefineAmount = 0;
                                }
                            }
                        }
                    } catch (e) {
                        ///
                    }
                    //}
                    if (response.data.Error === true) {
                        $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                    }
                    else {
                        //$scope.ShowResultCustom(response.data.Message, "success", 'EntryPopUp');
                        $scope.Clear();

                    }
                    //$scope.LoadPFSettingData();
                    $scope.UpdateAdditionalPolicyModel();
                    $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
                }, function errorCallback(response) {
                    $scope.ShowResultCustom(response.status.Message, "failure", 'EntryDiv');
                });
                return true;
            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
        }
    };
    $scope.onCreate = function (args) {

        $("#buttonName").ejButton({
            text: "Re-Calculate",
            click: function (args) {
                $scope.reCalculateSalary();
            }
        });
    }
    $scope.reCalculateSalary = function () {

        try {
            $.ajax({
                type: "POST",
                url: "humanresource/employeepromotionNew/ReCalculateSalaryStracture",
                data: { 'EmpSalaryInfoDefineOld': $scope.EmpSalaryInfoDefineNew },
                dataType: "json",
                success: function (response) {
                    $scope.EmpSalaryInfoDefineNew = response.data;
                    var eDialog = $("#dialogAPI2").data("ejDialog");
                    eDialog.close();
                }

            });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
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
            $scope.ShowResultCustom(e, "failure");
        }
    };


    $scope.givenDesignationChange = function () {
        $http.get('humanresource/employeepromotionNew/GivenDesignationChange?GivenDesignationId=' + $scope.budgetCodeChangeNew.GivenDesignationId)
            .then(function (response) {
                $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data[0].SalaryRuleMasterId;
            });
    };


    $scope.getSalaryRuleMasterSystemIDBygivenDesignation = function (id) {
        $http.get('humanresource/employeepromotionNew/GivenDesignationChange?GivenDesignationId=' + id)
            .then(function (response) {
                return response.data[0].SalaryRuleMasterId;
            });
    };
    $scope.xgivenDesignationChange = function () {
        $http.get('humanresource/employeepromotionNew/GivenDesignationChange?GivenDesignationId=' + $scope.budgetCodeChangeNew.GivenDesignationId)
            .then(function (response) {

                $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data.ResultData[0].SalaryRuleMasterSystemID;


            });
    };
    $scope.SalaryRuleChange = function () {
        $http.get('humanresource/employeepromotionNew/SalaryRuleChange?EmpSystemId=' + $scope.budgetCodeChangeNew.SystemId + '&SalaryRuleId=' + $scope.EmpSalaryInfo.SalaryRuleMasterSystemID)
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

    //$scope.popUp = function () {
    //    $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
    //    baseService.setCurrentPage('dataList');
    //    $scope.getPopUpData = function (pageno) {
    //        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
    //            .then(function (result) {
    //                $scope.popUpDataList = result.Rows;
    //                $scope.popUpParameters.total_count = result.Total;
    //                if (baseService.arrayLength($scope.popUpList) === 0) {
    //                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
    //                }
    //            }, function () {
    //                $scope.ShowResultCustom(commonMessage.NetworkError, 'failure', 'EntryDiv');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#popUpId')).modal('show');
    //    $scope.getPopUpData();
    //};

    $scope.popUp = function (name) {
        $scope.name = name;
        //if ($scope.name === 'Budget') {
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Code';
        $scope.popUpParameters.searchBy = 'Code';
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
                    $scope.popUpParameters.sort = 'Code';
                    $scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
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

    $scope.name = null;
    $scope.selectDoubleClick = function (data) {

        $scope.budgetCodeChangeNew.BudgetCode = data.Id;
        $scope.budgetCodeChangeNew.Code = data.Code;

        $scope.NewbudgetCodeChange.EntityName = data.EntityName;
        $scope.NewbudgetCodeChange.Designation = data.Designation;
        $scope.NewbudgetCodeChange.PositionName = data.PositionName;
        $scope.NewbudgetCodeChange.DesignationId = data.DesignationId;




        $scope.NewbudgetCodeChange.BudgetCode = data.Id;
        $scope.NewbudgetCodeChange.Code = data.Code;

        //$scope.NewbudgetCodeChange.DesignationSystemID = data.DesignationId;
        //$scope.NewbudgetCodeChange.Designation = data.Designation;
        //$scope.NewbudgetCodeChange.PositionName = data.PositionName;
        //$scope.NewbudgetCodeChange.DesignationId = data.DesignationId;
        $scope.NewbudgetCodeChange.UnitId = data.UnitId;
        $scope.NewbudgetCodeChange.DivisionId = data.DivisionId;
        $scope.NewbudgetCodeChange.DepartmentId = data.DepartmentId;
        $scope.NewbudgetCodeChange.SectionId = data.SectionId;
        $scope.NewbudgetCodeChange.SubSectionId = data.SubSectionId;
        $scope.NewbudgetCodeChange.SubdivisionID = data.SubdivisionID;
        $scope.NewbudgetCodeChange.LineId = data.LineId;
        $scope.NewbudgetCodeChange.EmploymentType = data.EmploymentType;
        $scope.NewbudgetCodeChange.PositionID = data.PositionId;
        $scope.NewbudgetCodeChange.IsDirect = data.IsDirect;

        //$scope.budgetCodeChangeNew.LegalDesignationId = data.DesignationId;
        //$scope.GetGivenDesignationByLegalDesignaiton($scope.budgetCodeChangeNew.LegalDesignationId);
        //$scope.name = null;
        angular.element(document.querySelector('#popUpId')).modal('hide');
        //} else {
        //    $scope.budgetCodeChangeNew.LegalDesignationId = data.Id;
        //    $scope.budgetCodeChangeNew.LegalDesignation = data.UserName;
        //    $scope.GetGivenDesignationByLegalDesignaiton($scope.budgetCodeChangeNew.LegalDesignationId);
        //    //$scope.name = null;
        //    angular.element(document.querySelector('#LDPopUp')).modal('hide');
        //}

        // $scope.budgetCodeChangeNew.GivenDesignationId = null;

        //cboService.getCboLowerGivenDesignation($scope.budgetCodeChange.DesignationId, function (result) {
        //    $scope.givenDesignationList = result;
        //    $scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChangeNew.DesignationId;
        //    //preRecruitmentEmployeeNew.GivenDesignationId
        //});

        //$scope.closePopUp();
    };

    $scope.GetGivenDesignationByLegalDesignaiton = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + legalDesignationId + '&BudgetCode=' + $scope.budgetCodeChangeNew.BudgetCode
        }).then(function successCallback(response) {
            //$scope.givenDesignationList = response.data;
            if (response.data.length > 0) {
                $scope.budgetCodeChangeNew.GivenDesignationId = response.data[0].Value;
            } else {
                $scope.budgetCodeChangeNew.GivenDesignationId = null;
            }

        })
    };

    $scope.LegalDesignationList = [];

    $scope.getLegalDesignation = function () {
        try {


            $http({
                method: 'GET',
                url: $scope.GetLegalDesignationUrl,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ////
                }
                else {
                    $scope.LegalDesignationList = response.data;

                }
            }, function errorCallback(response) {

            });
            return true;

        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
        }
    };
    $scope.getLegalDesignation();
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

    $scope.ClearData = function () {
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
        $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = null;
        $scope.imageSrc = null;
        $scope.obj = {};
        $scope.Calculated = false;
        $scope.NewNetGross = null;
        $scope.NewNetCTC = null;
        $scope.MinWage = null;
        $scope.SalaryRole = null;
        $scope.SelectedSalaryRole = null;
        $scope.EmpSalaryOpenHead = null;
        $scope.newFormula_Desc = null;
        $scope.approvedFormula_Desc = null;
        $scope.UnApprovedNextDueDate = null;
        $scope.approvalStatusNew = null;
        $scope.approvalStatusApproved = null;

        $scope.NewNetTotalGross = null;
        $scope.newFormula_Desc = null;
        $scope.approvedFormula_Desc = null;
        $scope.EmpSalaryInfo.SalaryID = null;
        $scope.EmpSalaryInfo.SalaryApprovedStatus = null;
        $scope.EmpSalaryInfo.EffectiveDate = null;
        $scope.ApprovedNextDueDate = null;
        $scope.EmpSalaryInfo.NextDueDate = null;
        $scope.ApprovedEffectiveDate = null;
        $scope.NetCTC = null;
        $scope.NetGross = null;
        $scope.NetTotalGross = null;
        $scope.IsSalaryRuleEditableEmployee = false;
        $scope.IncrementHistory.EmpSystemID = null;
        $scope.IncrementHistory.FromEffectiveDate = null;
        $scope.IncrementHistory.FromSalaryId = null;
        $scope.IncrementHistory.EmpSystemID = null;
        $scope.IncrementHistory.FromEffectiveDate = null;
        $scope.IncrementHistory.FromSalaryId = null;
        $scope.VPFPercentageModel = null;
        $scope.VPFEffectiveDateModel = null;
        $scope.PFSettingModel = {
            IsPFEntitle: false,
            IsVPFEntitle: false,
            IsESICEntitle: false,
            IsBonusEntitle: false,
            IsPFOptionalNew: false,
            IsPFMandatoryNew: false,
            IsVPFMandatoryNew: false,
            IsESICOptionalNew: false,
            IsESICMandatoryNew: false,
            IsBonusOptionalNew: false,
            IsBonusMandatoryNew: false,

            IsPFPolicyDefined: false,
            IsESICPolicyDefined: false,
            IsBonusPolicyDefined: false,
            IsPFNotEntitleGetAllo: false,
            PFEffectiveDate: null,
            IsbuttonPFClicked: 'NO',
            EmpSystemId: null
        };
        $scope.SalaryRole = { SalaryRuleMasterSystemID: null, SalaryRuleName: null, SalaryRuleDescription: null };

    };

    $scope.Get = function (data) {
        $scope.ClearData();

        //Confirmation
        if ($scope.qempid !== null && $scope.qstatus === 'Confirmation') {
            $scope.divFreshEntry = false;

            //$scope.budgetCodeChange = $scope.employees;
            //$scope.budgetCodeChangeOld = $scope.employees;
            //$scope.budgetCodeChangeNew = $scope.budgetCodeChange;

            $scope.budgetCodeChange = Object.assign({}, $scope.employees);
            $scope.budgetCodeChangeOld = Object.assign({}, $scope.employees);
            $scope.budgetCodeChangeNew = Object.assign({}, $scope.budgetCodeChange);

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
            // $scope.obj = $filter("filter")($scope.employees, { SystemId: data.data.SystemId });
            //$scope.obj = data.data;
            $scope.obj = Object.assign({}, data.data);

            //$scope.budgetCodeChange = $scope.obj[0];         
            //$scope.budgetCodeChangeOld = $scope.obj[0];
            //$scope.budgetCodeChangeNew = $scope.obj[0];

            angular.copy($scope.obj, $scope.budgetCodeChange);
            angular.copy($scope.obj, $scope.budgetCodeChangeOld);
            angular.copy($scope.obj, $scope.budgetCodeChangeNew);


            $scope.NewbudgetCodeChange.EntityName = $scope.budgetCodeChangeOld.EntityName;
            $scope.NewbudgetCodeChange.PositionName = $scope.budgetCodeChangeOld.PositionName;

            //angular.copy($scope.budgetCodeChange, $scope.budgetCodeChangeNew);

            //$scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
            //$scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;

            //angular.copy($scope.budgetCodeChange.Code, $scope.budgetCodeChangeNew.Code)
            //angular.copy($scope.budgetCodeChange.GivenDesignationId, $scope.budgetCodeChangeNew.GivenDesignationId)


            $http({
                method: 'GET',
                url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + $scope.budgetCodeChangeOld.LegalDesignationId + '&BudgetCode=' + $scope.budgetCodeChangeNew.BudgetCode
            }).then(function successCallback(response) {
                //$scope.givenDesignationList = response.data;
                if (response.data.length > 0) {
                    $scope.budgetCodeChangeNew.GivenDesignationId = response.data[0].Value;
                    $scope.budgetCodeChangeOld.GivenDesignationId = response.data[0].Value;
                } else {
                    $scope.budgetCodeChangeNew.GivenDesignationId = null;
                    $scope.budgetCodeChangeOld.GivenDesignationId = null;
                }

            })







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
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
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
            $scope.IncrementHistory.ToGivenDesignationId = $scope.budgetCodeChangeNew.GivenDesignationId;
            $scope.IncrementHistory.ToBudgetCode = $scope.budgetCodeChangeNew.BudgetCode;
            $scope.IncrementHistory.ToLegalDesignationId = $scope.budgetCodeChangeNew.LegalDesignationId;
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
                        $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                    }
                    else {
                        $scope.ShowResultCustom(response.data.Message, "success", 'EntryDiv');
                        $scope.Clear();
                        //angular.element(document.querySelector('#EntryPopUp')).modal('hide');
                        //$scope.getData();


                        $scope.NewbudgetCodeChange = {};
                    }
                }, function errorCallback(response) {
                    $scope.ShowResultCustom(response.data.Message, "failure", 'EntryDiv');
                });
                return true;
            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
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
                    data: { 'employeeInformation': $scope.budgetCodeChangeNew, 'EmpSalaryInfo': $scope.EmpSalaryInfo, 'EmpSalaryInfoDefineNew': $scope.EmpSalaryInfoDefineNew, 'IncrementHistory': $scope.IncrementHistory, 'AdditionalPolicySettingModel': $scope.SettingModel },
                    dataType: "json",
                    success: function (data) {
                        if (data.Error === true) {
                            $scope.ShowResultCustom(data.Message, "failure", 'EntryDiv');
                        }
                        else {
                            $scope.ShowResultCustom(data.Message, "success", 'EntryDiv');
                        }

                    }

                });


            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
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
        IsVPFEntitle: false,
        IsESICEntitle: false,
        IsBonusEntitle: false,
        IsPFOptionalNew: false,
        IsPFMandatoryNew: false,
        IsVPFMandatoryNew: false,
        IsESICOptionalNew: false,
        IsESICMandatoryNew: false,
        IsBonusOptionalNew: false,
        IsBonusMandatoryNew: false,

        IsPFPolicyDefined: false,
        IsESICPolicyDefined: false,
        IsBonusPolicyDefined: false,
        IsPFNotEntitleGetAllo: false,
        PFEffectiveDate: null,
        IsbuttonPFClicked: 'NO',
        EmpSystemId: null
    };

    //$scope.SettingModel = {
    //    RuleId: null,
    //    SalaryHeadEnum: null,
    //    IsMandatory: null,
    //    IsEntitle: false,
    //    IsEditable: false  
    //};
    $scope.SettingModel = [];
    $scope.ResetPFSettingModel = function () {
        $scope.PFSettingModel.IsPFEntitle = false;
        $scope.PFSettingModel.IsVPFEntitle = false;
        $scope.PFSettingModel.IsESICEntitle = false;
        $scope.PFSettingModel.IsBonusEntitle = false;
        $scope.PFSettingModel.IsPFPolicyDefined = false;
        $scope.PFSettingModel.IsESICPolicyDefined = false;
        $scope.PFSettingModel.IsBonusPolicyDefined = false;
        $scope.PFSettingModel.IsPFNotEntitleGetAllo = false;
        $scope.PFSettingModel.PFEffectiveDate = null;
        $scope.PFSettingModel.IsbuttonPFClicked = 'NO';
        $scope.PFSettingModel.EmpSystemId = null;
        $scope.SettingModel = [];
    };

    $scope.OpenAdditionalPolicyDialog = function () {
        try {
            if (baseService.arrayLength($scope.EmpSalaryOpenHeadCurrent) > 0) {
                for (var i = 0; i < $scope.EmpSalaryOpenHeadCurrent.length; i++) {
                    if ($scope.EmpSalaryOpenHeadCurrent[i].Amount === null || baseService.isUndefinedOrNull($scope.EmpSalaryOpenHeadCurrent[i].Amount)) {
                        throw "Amount is required for " + $scope.EmpSalaryOpenHeadCurrent[i].SalaryHead + ".";
                    }
                }
            }

            var eDialog = $("#dialogPFSetting").data("ejDialog");
            eDialog.open();
            var gridObj = $("#GridAddp").data("ejGrid");
            gridObj.refreshContent(true);
            $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.UpdateAdditionalPolicyModel = function () {


        // if ($scope.PFSettingModel.IsbuttonPFClicked === 'NO') {
        if ($scope.SettingModel.length > 0) {
            for (var i = 0; i < $scope.SettingModel.length; i++) {


                if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'PF') {
                    $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsPFEntitle;

                    if ($scope.PFSettingModel.IsPFMandatoryNew === true || $scope.PFSettingModel.IsPFPolicyDefined === false) {
                        $scope.SettingModel[i].IsEditable = false;
                    } else {
                        $scope.SettingModel[i].IsEditable = true;
                    }
                    if ($scope.PFSettingModel.IsPFMandatoryNew === true) {
                        $scope.SettingModel[i].IsMandatory = 'YES';
                    } else {
                        $scope.SettingModel[i].IsMandatory = 'NO';
                    }
                    //$scope.PFSettingModel.IsPFEntitle = $scope.SettingModel[i].IsEntitle;
                }

                if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'ESIC') {
                    $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsESICEntitle;

                    if ($scope.PFSettingModel.IsESICMandatoryNew === true || $scope.PFSettingModel.IsESICPolicyDefined === false) {
                        $scope.SettingModel[i].IsEditable = false;
                    } else {
                        $scope.SettingModel[i].IsEditable = true;
                    }
                    if ($scope.PFSettingModel.IsESICMandatoryNew === true) {
                        $scope.SettingModel[i].IsMandatory = 'YES';
                    } else {
                        $scope.SettingModel[i].IsMandatory = 'NO';
                    }
                    //$scope.PFSettingModel.IsESICEntitle = $scope.SettingModel[i].IsEntitle;
                }

                if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'BONUSRETAIN') {
                    $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsBonusEntitle;

                    if ($scope.PFSettingModel.IsBonusMandatoryNew === true || $scope.PFSettingModel.IsBonusPolicyDefined === false) {
                        $scope.SettingModel[i].IsEditable = false;
                    } else {
                        $scope.SettingModel[i].IsEditable = true;
                    }
                    if ($scope.PFSettingModel.IsBonusMandatoryNew === true) {
                        $scope.SettingModel[i].IsMandatory = 'YES';
                    } else {
                        $scope.SettingModel[i].IsMandatory = 'NO';
                    }
                    //$scope.PFSettingModel.IsBonusEntitle = $scope.SettingModel[i].IsEntitle;
                }
            }

            for (var i = 0; i < $scope.SettingModel.length; i++) {
                if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'PF' && $scope.SettingModel[i].IsEntitle === true) {
                    var obj = {
                        Id: 'VPF-1',
                        SalaryHeadEnum: 'VPF',
                        SalaryRuleId: $scope.SettingModel[i].SalaryRuleId,
                        IsEditable: true,
                        IsEntitle: false,
                        IsMandatory: 'NO',
                        Percentage: null,
                        EffectiveDate: null
                    };
                    if ($scope.SettingModel[i].IsEntitle === true) {
                        if (checkExistList($scope.SettingModel, obj.Id, obj.SalaryHeadEnum) === false) {
                            $scope.SettingModel.push(obj);
                        }
                    }
                }
            }


            for (var i = 0; i < $scope.SettingModel.length; i++) {
                if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'VPF') {
                    $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsVPFEntitle;

                    if ($scope.SettingModel[i].IsEditable === true) {
                        $scope.SettingModel[i].IsEditable = true;
                    }
                    if ($scope.PFSettingModel.IsVPFMandatoryNew === true) {
                        $scope.SettingModel[i].IsMandatory = 'YES';
                    } else {
                        $scope.SettingModel[i].IsMandatory = 'NO';
                    }
                    //$scope.PFSettingModel.IsVPFEntitle = $scope.SettingModel[i].IsEntitle;
                }
            }
        }
        // }


    };

    function checkExistList(list, Id, SalaryHeadEnum) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id == Id && list[i].SalaryHeadEnum == SalaryHeadEnum) {
                return true;
            }
        }
        return false;
    }


    $scope.CloseAdditionalPolicyDialog = function () {
        try {
            if ($scope.SettingModel.length > 0) {
                for (var i = 0; i < $scope.SettingModel.length; i++) {
                    if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'VPF') {
                        if ($scope.SettingModel[i].IsEntitle === true) {

                            // #region validation
                            if (baseService.isUndefinedOrNull($scope.VPFEffectiveDateModel)) {

                                throw "Enter valid Effective Date.";
                            } else {
                                $scope.VPFEffectiveDateModel = $filter('dateFiltering')($scope.VPFEffectiveDateModel, 'dd-M-yyyy');
                            }

                            if (baseService.isUndefinedOrNull($scope.VPFPercentageModel)) {

                                throw "Enter valid VPF Percentage.";
                            }
                        }
                    }
                }
                var eDialog = $("#dialogPFSetting").data("ejDialog");
                eDialog.close();
                $scope.calculateSalary();
            }
        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };

    $scope.LoadAdditionalPolicySettingData = function () {

        try {
            $http.get('humanresource/employeepromotionNew/GivenDesignationChange?GivenDesignationId=' + $scope.budgetCodeChangeNew.GivenDesignationId)
                .then(function (response) {
                    $scope.EmpSalaryInfo.SalaryRuleMasterSystemID = response.data[0].SalaryRuleMasterId;


                    $.ajax({
                        type: "Post",
                        //url: "humanresource/employeepromotionNew/GetSettingsByRule?SalaryRuleId=" + $scope.SelectedSalaryRole[0].SalaryRuleMasterSystemID,
                        url: "humanresource/employeepromotionNew/GetSettingsByRule?SalaryRuleId=" + $scope.EmpSalaryInfo.SalaryRuleMasterSystemID,
                        //data: { 'SalaryRuleId': $scope.SelectedSalaryRole },
                        dataType: "json",
                        success: function (response) {
                            $scope.SettingModel = response.data;

                            if ($scope.SettingModel.length > 0) {
                                $scope.ShowPFButton = true;
                            }
                        }

                    });

                });


        } catch (e) {
            ShowResult(e.Message, "failure");
        }
    };
    $scope.xLoadPFSettingData = function () {
        //var eDialog = $("#dialogPFSetting").data("ejDialog");
        //eDialog.open();
        try {
            //$.ajax({
            //    type: "Post",
            //    url: "humanresource/employeepromotionNew/GetSettingsByRule?SalaryRuleId=" + $scope.SelectedSalaryRole[0].SalaryRuleMasterSystemID,
            //    //data: { 'SalaryRuleId': $scope.SelectedSalaryRole },
            //    dataType: "json",
            //    success: function (response) {
            //        $scope.SettingModel = response.data;
            //    }

            //});

            if ($scope.PFSettingModel.IsbuttonPFClicked === 'NO') {
                if ($scope.SettingModel.length > 0) {
                    for (var i = 0; i < $scope.SettingModel.length; i++) {


                        if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'PF') {
                            $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsPFMandatoryNew;

                            if ($scope.SettingModel[i].IsEditable === true && $scope.PFSettingModel.IsPFOptionalNew === true) {
                                $scope.SettingModel[i].IsEditable = true;
                            }
                            if ($scope.PFSettingModel.IsPFMandatoryNew === true) {
                                $scope.SettingModel[i].IsMandatory = 'YES';
                            } else {
                                $scope.SettingModel[i].IsMandatory = 'NO';
                            }
                            $scope.PFSettingModel.IsPFEntitle = $scope.SettingModel[i].IsEntitle;
                        }

                        if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'ESIC') {
                            $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsESICMandatoryNew;

                            if ($scope.SettingModel[i].IsEditable === true && $scope.PFSettingModel.IsESICOptionalNew === true) {
                                $scope.SettingModel[i].IsEditable = true;
                            }
                            if ($scope.PFSettingModel.IsESICMandatoryNew === true) {
                                $scope.SettingModel[i].IsMandatory = 'YES';
                            } else {
                                $scope.SettingModel[i].IsMandatory = 'NO';
                            }
                            $scope.PFSettingModel.IsESICEntitle = $scope.SettingModel[i].IsEntitle;
                        }


                        if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'VPF') {
                            $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsVPFEntitle;

                            if ($scope.SettingModel[i].IsEditable === true) {
                                $scope.SettingModel[i].IsEditable = true;
                            }
                            if ($scope.PFSettingModel.IsVPFEntitle === true) {
                                $scope.SettingModel[i].IsMandatory = 'YES';
                            } else {
                                $scope.SettingModel[i].IsMandatory = 'NO';
                            }
                            $scope.PFSettingModel.IsVPFEntitle = $scope.SettingModel[i].IsEntitle;
                        }


                        if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'BONUSRETAIN') {
                            $scope.SettingModel[i].IsEntitle = $scope.PFSettingModel.IsBonusEntitle;

                            if ($scope.SettingModel[i].IsEditable === true) {
                                $scope.SettingModel[i].IsEditable = true;
                            }
                            if ($scope.PFSettingModel.IsBonusEntitle === true) {
                                $scope.SettingModel[i].IsMandatory = 'YES';
                            } else {
                                $scope.SettingModel[i].IsMandatory = 'NO';
                            }
                            $scope.PFSettingModel.IsBonusEntitle = $scope.SettingModel[i].IsEntitle;
                        }
                    }
                }
            }



            //$scope.PFSettingModel.IsbuttonPFClicked = 'YES';



        } catch (e) {
            ShowResult(e.Message, "failure");
        }
    };
    $scope.ShowPFSetting = function () {

        var eDialog = $("#dialogPFSetting").data("ejDialog");
        eDialog.open();


        try {
            $.ajax({
                type: "POST",
                url: "humanresource/employeepromotionNew/ShowPFSetting",
                data: { 'EmpSystemId': $scope.budgetCodeChangeOld.SystemId, 'PFSettingModel': $scope.PFSettingModel },
                dataType: "json",
                success: function (data) {
                    $scope.PFSettingModel = data.PFCheckAndUnCheck;
                    $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
                }

            });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
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

            $scope.PFSettingModel.IsPFEntitle = $scope.SettingModel[0].IsEntitle;
            $scope.PFSettingModel.IsbuttonPFClicked = 'YES';
            $scope.calculateSalary();
            //$http.get('humanresource/employeepromotionNew/PFCheckAndUnCheckDone?EmpSystemId=' + $scope.budgetCodeChangeOld.SystemId + '&IsbuttonPFClicked=' + $scope.PFSettingModel.IsbuttonPFClicked + '&IsPFEntitle=' + $scope.PFSettingModel.IsPFEntitle + '&PFEffectiveDate=' + $scope.PFSettingModel.PFEffectiveDate)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            $scope.ShowResultCustom(response.data.Message, 'failure');
            //        }

            //        function errorCallBack(response) {
            //            $scope.ShowResultCustom(response.data.Message, 'failure');
            //        }
            //    });


        } catch (e) {
            $scope.ShowResultCustom(e, "failure");
        }
    };

    $scope.VPFEntryFrom = function (arg) {
        var gridObj = $("#GridAddp").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

        if (data.SalaryHeadEnum.toUpperCase() === 'PF') {
            if ($scope.SettingModel.length > 0) {
                for (var i = 0; i < $scope.SettingModel.length; i++) {
                    if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'PF') {
                        var obj = {
                            Id: 'VPF-1',
                            SalaryHeadEnum: 'VPF',
                            SalaryRuleId: $scope.SettingModel[i].SalaryRuleId,
                            IsEditable: true,
                            IsEntitle: false,
                            IsMandatory: 'NO',
                            Percentage: null,
                            EffectiveDate: null
                        };
                        if ($scope.SettingModel[i].IsEntitle === true) {
                            $scope.SettingModel.push(obj);
                            //if ($scope.SettingModel.find(x => x.SalaryHeadEnum !== 'VPF')) {
                            //    $scope.SettingModel.push(obj);
                            //};
                        };
                        if ($scope.SettingModel[i].IsEntitle === false) {
                            $scope.SettingModel.splice($scope.SettingModel.indexOf(obj));
                        };

                    }
                }
            }
        }

        if (data.SalaryHeadEnum.toUpperCase() === 'VPF') {
            if ($scope.SettingModel.length > 0) {
                for (var i = 0; i < $scope.SettingModel.length; i++) {
                    if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'VPF') {
                        if ($scope.SettingModel[i].IsEntitle === true) {
                            var eDialog = $("#dialogVPFEntryFrom").data("ejDialog");
                            eDialog.open();
                        }
                    }
                }
            }
        }
    };


    $scope.VPFEffectiveDateModel = null;
    $scope.VPFPercentageModel = null;
    $scope.VPFEntryFromOK = function (arg) {


        try {
            // #region validation
            if (baseService.isUndefinedOrNull($scope.VPFEffectiveDateModel)) {

                throw "Enter valid Effective Date.";
            } else {
                $scope.VPFEffectiveDateModel = $filter('dateFiltering')($scope.VPFEffectiveDateModel, 'dd-M-yyyy');
            }

            if (baseService.isUndefinedOrNull($scope.VPFPercentageModel)) {

                throw "Enter valid VPF Percentage.";
            }


            if ($scope.SettingModel.length > 0) {
                for (var i = 0; i < $scope.SettingModel.length; i++) {
                    if ($scope.SettingModel[i].SalaryHeadEnum.toUpperCase() === 'VPF') {
                        if ($scope.SettingModel[i].IsEntitle === true) {


                            $scope.SettingModel[i].EffectiveDate = $scope.VPFEffectiveDateModel;
                            $scope.SettingModel[i].Percentage = $scope.VPFPercentageModel;

                            var eDialog = $("#dialogVPFEntryFrom").data("ejDialog");
                            eDialog.close();
                        }
                    }
                }
            }




        } catch (e) {
            $scope.ShowResultCustom(e, "failure", 'EntryDiv');
        }
    };


}