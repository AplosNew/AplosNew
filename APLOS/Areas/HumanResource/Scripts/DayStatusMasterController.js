'use strict';
DayStatusMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DayStatusMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Day Status Master';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/DayStatusMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

  
    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
       
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

   

    // Functions for the Current Day Status
    $scope.DayTypesList = [];
    function getDayTypes() {
        $http({
            method: 'GET',
            url: $scope.path + 'getDayTypes',
        }).then(function succ(resp) {
            $scope.DayTypesList = resp.data;
        });
    };

    getDayTypes();

    

    //*********************  Operations Staring for the Pages  ******************************\\

    //Getting the Master Grid
    $scope.masterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getMaster',
        }).then(function succ( resp ){
            $scope.masterList = [];
            $scope.masterList = resp.data;
        });
    }
    $scope.getMaster();

    $scope.getMasterDetails = function (e) {
        $scope.Master = e.data;
        $scope.ConvertBool();
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: {'MasterId': $scope.Master.Id}
        }).then(function success(resp){
            //$scope.childDataList = [];
            //$scope.childDataList = resp.data;
            //$scope.Child.MasterId = $scope.Master.Id;
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        })
    }


    //*********************  Operations for the Master Tab  *************************\\
    $scope.saveMaster = function () {
        $scope.$broadcast('show-errors-check-validity');
        allValidations();
        $scope.ConvertBits();
        if ($scope.MasterForm2.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveMaster',
                data: {'Master' : $scope.Master}
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');

                    $scope.ConvertBool();
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.Child.MasterId = response.data.Data.Id;
                    $scope.Master.Id = response.data.Data.Id;
                    $scope.getMaster();
                    $scope.ConvertBool();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

                $scope.ConvertBool();
            }
        }
    }

    
    //For Deleting of A Master
    $scope.Delete = function () {
       //

        $http({
            method: 'POST',
            url: $scope.path + 'deleteMaster',
            data: { 'id': $scope.Master.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMaster();
                $scope.Clear();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.ConvertBool();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

 

    //All the Validation Funcitons

  
    $scope.ConvertBits = function () {
        if ($scope.Master.FirstSource == "Yes") {
            $scope.Master.FirstSource = true;
        }
        else {
            $scope.Master.FirstSource = false;
        } 

        if ($scope.Master.ManualAuto == "Yes") {
            $scope.Master.ManualAuto = true;
        }
        else {
            $scope.Master.ManualAuto = false;
        }

        if ($scope.Master.InStatusApplicable == "Yes") {
            $scope.Master.InStatusApplicable = true;
        }
        else {
            $scope.Master.InStatusApplicable = false;
        }

        if ($scope.Master.OutStatusApplicable == "Yes") {
            $scope.Master.OutStatusApplicable = true;
        }
        else {
            $scope.Master.OutStatusApplicable = false;
        }

        if ($scope.Master.DurationApplicable == "Yes") {
            $scope.Master.DurationApplicable = true;
        }
        else {
            $scope.Master.DurationApplicable = false;
        }



        if ($scope.Master.WorkingDayOTApplicable == "Yes") {
            $scope.Master.WorkingDayOTApplicable = true;
        }
        else {
            $scope.Master.WorkingDayOTApplicable = false;
        }

        if ($scope.Master.NonWorkingDayOTApplicable == "Yes") {
            $scope.Master.NonWorkingDayOTApplicable = true;
        }
        else {
            $scope.Master.NonWorkingDayOTApplicable = false;
        }


        if ($scope.Master.CompensatoryApplicable == "Yes") {
            $scope.Master.CompensatoryApplicable = true;
        }
        else {
            $scope.Master.CompensatoryApplicable = false;
        }

        if ($scope.Master.GoodWorkApplicable == "Yes") {
            $scope.Master.GoodWorkApplicable = true;
        }
        else {
            $scope.Master.GoodWorkApplicable = false;
        }

        if ($scope.Master.ToCheck == "Yes") {
            $scope.Master.ToCheck = true;
        }
        else {
            $scope.Master.ToCheck = false;
        }
    }

    //Converting To Yes/No from True/False
    $scope.ConvertBool = function () {
        if ($scope.Master.FirstSource == true) {
            $scope.Master.FirstSource = "Yes";
        }
        else {
            $scope.Master.FirstSource = "No";
        }

        if ($scope.Master.ManualAuto == true) {
            $scope.Master.ManualAuto = "Yes";
        }
        else {
            $scope.Master.ManualAuto = "No";
        }

        if ($scope.Master.InStatusApplicable == true) {
            $scope.Master.InStatusApplicable = "Yes";
        }
        else {
            $scope.Master.InStatusApplicable = "No";
        }

        if ($scope.Master.OutStatusApplicable == true) {
            $scope.Master.OutStatusApplicable = "Yes";
        }
        else {
            $scope.Master.OutStatusApplicable = "No";
        }

        if ($scope.Master.DurationApplicable == true) {
            $scope.Master.DurationApplicable = "Yes";
        }
        else {
            $scope.Master.DurationApplicable = "No";
        }

        if ($scope.Master.WorkingDayOTApplicable == true) {
            $scope.Master.WorkingDayOTApplicable = "Yes";
        }
        else {
            $scope.Master.WorkingDayOTApplicable = "No";
        }

        if ($scope.Master.NonWorkingDayOTApplicable == true) {
            $scope.Master.NonWorkingDayOTApplicable = "Yes";
        }
        else {
            $scope.Master.NonWorkingDayOTApplicable = "No";
        }


        if ($scope.Master.CompensatoryApplicable == true) {
            $scope.Master.CompensatoryApplicable = "Yes";
        }
        else {
            $scope.Master.CompensatoryApplicable = "No";
        }

        if ($scope.Master.GoodWorkApplicable == true) {
            $scope.Master.GoodWorkApplicable = "Yes";
        }
        else {
            $scope.Master.GoodWorkApplicable = "No";
        }

        if ($scope.Master.ToCheck == true) {
            $scope.Master.ToCheck = "Yes";
        }
        else {
            $scope.Master.ToCheck = "No";
        }
    }

    // ****************************************** Main Page Operations ******************************************** \\ 

    var j = document.getElementById("tab_show");
    j.style.display = "none";
    //Showing the Childs
    function showTabs() {
        if ($scope.Header.Id != null) {
            j.style.display = "block";
        }
        else {
            j.style.display = "none";
        }
    }


    // The List for the Responsible Person
    $scope.EmployeesList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getEmployees",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.EmployeesList = [];
        $scope.EmployeesList = response.data;
    });

    // Double Click the Main Header Grid
    $scope.getHeaderDetails = function (e) {
        $scope.Header = e.data;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

        $scope.ClearDayTypeChild();
        $scope.ClearDayStatus();
        $scope.LeaveDt = {
            HeaderId: null,
            DayTypeWithValuesId: null,
            DayTypeWithValue: null,
        };
        $scope.LeaveList = [];
        $scope.DayChild.HeaderId = e.data.Id;
        $scope.DayS.HeaderId = e.data.Id;
        $scope.Child.HeaderId = e.data.Id;
        $scope.LeaveDt.HeaderId = e.data.Id;
        $scope.GetSequenceDayStatus();
        $scope.getDayTypeChild();
        updateChild();
        $scope.getDaystatusChild();
        showTabs();
        LoadLeaveDayType();
        
    }


    /// ******************************* Header Operations ******************************* \\\
    $scope.Header = {
        Id: null,
        ShortName:null,
        StandardName:null,
        UserName: null,
        Sequence: 0,
        Remarks: null,
        Active:false,
    };

    $scope.HeaderList = [];

    // Operations to Get the Header
    $scope.getHeader = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getHeader',
        }).then(function succ(resp) {
            $scope.HeaderList = [];
            $scope.HeaderList = resp.data;
        });

    }

    $scope.getHeader();


    //Saving The Header
    $scope.saveHeader = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.HeaderForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveHeader',
                data: { 'Header': $scope.Header }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Header = response.data.Data;
                    $scope.DayChild.HeaderId = response.data.Data.Id;
                    $scope.DayS.HeaderId = response.data.Data.Id;
                    $scope.Child.HeaderId = response.data.Data.Id;
                    $scope.LeaveDt.HeaderId = response.data.Data.Id;
                    showTabs();
                    LoadLeaveDayType();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    //Getting the Header Sequence
    $scope.GetSequenceHeader = function () {
        cboService.getSequence($scope.path +'GetAutoSequenceHeader', function (data) {
            $scope.Header.Sequence = data;
        });
    };

    $scope.GetSequenceHeader();

    //Clearing the Whole Header
    $scope.clearHeader = function () {
        $scope.Header = {
            Id: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Sequence: 0,
            Remarks: null,
            Active: false,
        };
        $scope.GetSequenceHeader();
        showTabs();

    }


    //// ************************************* Day Types With Values ************************************* ////

    $scope.DayChild = {
        Id: null,
        HeaderId: null,
        Category: null,
        SubCategory: null,
        ResponsiblePersonId: null,
        ShortName: null,
        StandardName: null,
        DayType: null,
        UserName: null,
        Code: null,
        ReportCode: null,
        Remarks: null,
        Active: false,
        TotalWorkingDay: 0,
        ActualWorkingDay: 0,
        PayDay: 0,
        NonPayDay: 0,
        PresentValuePD: 0,
        LeaveValueLP: 0,
        LeaveValueLWP: 0,
        AbsentValueAB: 0,
        WeeklyOffWO: 0,
        HolidayH: 0,
        Other: 0,
        AttendanceBonus: 0,
        OTApplicable: 0,
        CompensatoryApplicable: false,
        GoodWorkApplicable: 0,
        SandwichStatusFlag: 0,
        ToAudit: false,
        OTMultiplier: 0,
        OTHourLimit: 0,
        OverStayLimit: 0,
        AttendanceReProcessApplicable: false,
        OTLimitLockApplicable: false,
        OTCalculation: 0, 
        CasualLeaveValueCV: 0,
        MedicalLeaveValueMV: 0,
        PriviledgeLeavePL: 0,
        MaternityLeaveValueMLV: 0,
        LateValueLV: 0,
        WeekOffHoliDayWOH: 0,
        CompAssignLv: 0,
        ManualStatusAllowed: false,
        DayStatusChange: false,
        AutoLock: false,
        EarnedPL: 0,
        EarnedCL: 0,
        IsCreditLimitAllowed: 0,
        DayLimit :0,
        Week1Limit: 0,
        Week2Limit: 0,
        Week3Limit: 0,
        Week4Limit: 0,
        MonthlyLimit: 0,
        ApplicableWM: null,
        OTMultiplingFactor: 0,
        OTRateLegal: null,
        OTRateExtra: null,
        OTConfirmation: null,
        isOTConfirmationAuto: false,
        DisplayInOutTime: null,
        AttnBonusAbsent: 0,
        AttnBonusLate: 0,
        AttnBonusLeave: 0,
        OTCategory: null,
    };

    //Getting the OTRate List
    $scope.OTRateList = [];

    $scope.getOTRateList = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getOTRateList',
        }).then(function succ(resp) {
            $scope.OTRateList = resp.data;
        });
    }

    $scope.getOTRateList();

    //Seleting the Current Day Status Starts
    $scope.selectCurrDayStatus = function () {
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('show');
    }

    $scope.doubleCurrDayStatus = function (e) {
        $scope.DayChild.DayType = e.data.DayType;
        angular.element(document.querySelector('#CurrDayStatusModal')).modal('hide');
    }
    // Ends here

    //To Select the Responsible Person Starts
    $scope.selectRespPerson = function () {
        angular.element(document.querySelector('#RespPersonModal')).modal('show');
    }

    $scope.RespPerson = null;

    $scope.doubleRespPerson = function (e) {
        $scope.RespPerson = e.data.EmployeeName;
        $scope.DayChild.ResponsiblePersonId = e.data.SystemId;
        angular.element(document.querySelector('#RespPersonModal')).modal('hide');
    }
    /// End Here

    //Saving of the Day Type With Values
    $scope.saveDayTypeChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        allValidations();
        if ($scope.DayTypeChild.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveDayTypeChild',
                data: { 'DayTypeChild': $scope.DayChild , 'Leave' : $scope.LeaveList }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDayTypeChild();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    // Validations for the Day Child Values
    function allValidations() {

        allZeros();

        if (parseFloat($scope.DayChild.TotalWorkingDay) > 1 || parseFloat($scope.DayChild.ActualWorkingDay) > 1) {
            ShowResult("Please check Working Day Value. It cannot be more than 1!", 'failure');
            throw ("Error");
        }

        if ((parseFloat($scope.DayChild.PayDay) + parseFloat($scope.DayChild.NonPayDay)) > 1) {
            ShowResult("Please check Pay Day Value. Total cannot be more than 1!", 'failure');
            throw ("Error");
        }
        if ((parseFloat($scope.DayChild.PayDay) + parseFloat($scope.DayChild.NonPayDay)) != 1) {
            ShowResult("Please check Pay Day Value. Total should be 1!", 'failure');
            throw ("Error");
        }

        if (parseFloat($scope.DayChild.OTConsider) > 1 || parseFloat($scope.DayChild.CompensatoryApplicable) > 1 || parseFloat($scope.DayChild.AttendanceBonus) > 1) {
            ShowResult("Please check Other. None Of the Values can be more than 1!", 'failure');
            throw ("Error");
        }

        if ((parseFloat($scope.DayChild.PresentValuePD) + parseFloat($scope.DayChild.LeaveValueLP) +
            parseFloat($scope.DayChild.LeaveValueLWP) + parseFloat($scope.DayChild.AbsentValueAB) + parseFloat($scope.DayChild.WeeklyOffWO)
            + parseFloat($scope.DayChild.HolidayH) + parseFloat($scope.DayChild.Other) + parseFloat($scope.DayChild.LateValueLV)+
        parseFloat($scope.DayChild.WeekOffHoliDayWOH)+
        parseFloat($scope.DayChild. CompAssignLv)) > 1) {
            ShowResult("Please check Day Status Value. Total cannot be more than 1!", 'failure');
            throw ("Error");
        }
        if ((parseFloat($scope.DayChild.PresentValuePD) + parseFloat($scope.DayChild.LeaveValueLP) +
            parseFloat($scope.DayChild.LeaveValueLWP) + parseFloat($scope.DayChild.AbsentValueAB) + parseFloat($scope.DayChild.WeeklyOffWO)
            + parseFloat($scope.DayChild.HolidayH) + parseFloat($scope.DayChild.Other) + parseFloat($scope.DayChild.LateValueLV) +
            parseFloat($scope.DayChild.WeekOffHoliDayWOH) +
            parseFloat($scope.DayChild.CompAssignLv)) != 1) {
            ShowResult("Please check Day Status Value. Total should be 1!", 'failure');
            throw ("Error");
        }
    };

    function allZeros() {
        //Print 13 to 24
        for (var i = 13; i < 25; i++) {
            if (Object.values($scope.DayChild)[i] == '') {
                var jj = Object.keys($scope.DayChild)[i];
                $scope.DayChild[jj] = 0;
            }
        }
    };

    //Clearing of the Day Type Child
    $scope.ClearDayTypeChild = function () {
        $scope.DayChild = {
            Id: null,
            HeaderId: null,
            Category: null,
            SubCategory: null,
            ResponsiblePersonId: null,
            ShortName: null,
            StandardName: null,
            DayType: null,
            UserName: null,
            Code: null,
            ReportCode: null,
            Remarks: null,
            Active: false,
            TotalWorkingDay: 0,
            ActualWorkingDay: 0,
            PayDay: 0,
            NonPayDay: 0,
            PresentValuePD: 0,
            LeaveValueLP: 0,
            LeaveValueLWP: 0,
            AbsentValueAB: 0,
            WeeklyOffWO: 0,
            HolidayH: 0,
            Other: 0,
            AttendanceBonus: 0,
            OTApplicable: 0,
            CompensatoryApplicable: false,
            GoodWorkApplicable: 0,
            SandwichStatusFlag: 0,
            ToAudit: false,
            OTMultiplier: 0,
            OTHourLimit: 0,
            OverStayLimit: 0,
            AttendanceReProcessApplicable: false,
            OTLimitLockApplicable: false,
            OTCalculation: 0,
            CasualLeaveValueCV: 0,
            MedicalLeaveValueMV: 0,
            PriviledgeLeavePL: 0,
            MaternityLeaveValueMLV: 0,
            LateValueLV: 0,
            WeekOffHoliDayWOH: 0,
            CompAssignLv: 0,
            ManualStatusAllowed: false,
            DayStatusChange: false,
            AutoLock: false,
            EarnedPL: 0,
            EarnedCL: 0,
            IsCreditLimitAllowed: 0,
            DayLimit: 0,
            Week1Limit: 0,
            Week2Limit: 0,
            Week3Limit: 0,
            Week4Limit: 0,
            MonthlyLimit: 0,
            ApplicableWM: null,
            OTMultiplingFactor: 0,
            OTRateLegal: null,
            OTRateExtra: null,
            OTConfirmation: null,
            isOTConfirmationAuto: false,
            DisplayInOutTime: null,
            AttnBonusAbsent: 0,
            AttnBonusLate: 0,
            AttnBonusLeave: 0,
            OTCategory: null,
        };
        $scope.DayChild.HeaderId = $scope.Header.Id;
        
    }

    $scope.DayTypeChildList = [];
    //Getting the Day Type Child Grid
    $scope.getDayTypeChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getDayTypeChild",
            data: { 'Id': $scope.Header.Id},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DayTypeChildList = [];
            $scope.DayTypeChildList = response.data;
        });
    }

    //Double Click the Day Type Child Grid
    $scope.getDayTypeChildDetails = function (e) {
        $scope.DayChild = e.data;
        for (var i = 0; i < $scope.EmployeesList.length; i++)
        {
            if ($scope.EmployeesList[i].SystemId == $scope.DayChild.ResponsiblePersonId) {
                $scope.RespPerson = $scope.EmployeesList[i].EmployeeName;
            }
        }
        LoadLeaveDayType();
        $scope.DayChild.OverStayLimit = String(e.data.OverStayLimit);
        $scope.DayChild.OTApplicable = String(e.data.OTApplicable);
        $scope.DayChild.GoodWorkApplicable = String(e.data.GoodWorkApplicable);
        $scope.DayChild.SandwichStatusFlag = String(e.data.SandwichStatusFlag);
        $scope.DayChild.OTCalculation = String(e.data.OTCalculation);
        $scope.DayChild.OTMultiplier = String(e.data.OTMultiplier);
        $scope.DayChild.OTHourLimit = String(e.data.OTHourLimit);
        $scope.DayChild.ApplicableWM = String(e.data.ApplicableWM);
        $scope.DayChild.OTConfirmation = String(e.data.OTConfirmation);
        $scope.DayChild.DisplayInOutTime = String(e.data.DisplayInOutTime);
        $scope.DayChild.OTCategory = String(e.data.OTCategory);
    }

    //Delete Day Status
    $scope.DeleteDT = function () {
        console.log("Delete DT");
    }

     //// ************************************* Day Status ************************************* ////
    $scope.DayS = {
        Id: null,
        HeaderId: null,
        DayTypeWithValuesId: null,
        Category: null,
        SubCategory: null,
        Sequence: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Code: null,
        Remarks: null,
        Active: false,
        FirstSource: false,
        MaunalAuto: null,
        InStatusApplicable: false,
        OutStatusApplicable: false,
        DurationApplicable: false,
    };

    $scope.selectStatusDayType = function () {
        angular.element(document.querySelector('#DayStatusDayType')).modal('show');
    }

    $scope.DayStatusType = null;

    $scope.doubleDayStatusType = function (e) {
        $scope.DayStatusType = e.data.DayType;
        $scope.DayS.DayTypeWithValuesId = e.data.Id;
        angular.element(document.querySelector('#DayStatusDayType')).modal('hide');
    }

    //Get Auto Sequence for the Day Status Child
    $scope.GetSequenceDayStatus = function () {
        cboService.getSequence($scope.path + 'GetAutoSequenceDayStatus', function (data) {
            $scope.DayS.Sequence = data;
        });
    };

    //Clear Day Status Child
    $scope.ClearDayStatus = function()
    {
        $scope.DayS = {
            Id: null,
            HeaderId: null,
            DayTypeWithValuesId: null,
            Category: null,
            SubCategory: null,
            Sequence: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Code: null,
            Remarks: null,
            Active: false,
            FirstSource: false,
            MaunalAuto: null,
            InStatusApplicable: false,
            OutStatusApplicable: false,
            DurationApplicable: false,
        };
        $scope.DayS.HeaderId = $scope.Header.Id;
        $scope.GetSequenceDayStatus();
        $scope.DayStatusType = null;
    }

    //Saving of the Day Status Child
    $scope.saveDayStatus = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DayStatusForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveDayStatusChild',
                data: { 'DaystatusChild': $scope.DayS }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearDayStatus();
                    $scope.getDaystatusChild();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }

    //Getting The Day Status Grid
    $scope.DayStatusList = [];
    $scope.getDaystatusChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDayStatusChild',
            data: {'HeaderId':$scope.DayS.HeaderId}
        }).then(function succ(resp) {
            $scope.DayStatusList = [];
            $scope.DayStatusList = resp.data;
        })
    }

    //Double Click on the Day Status Child Grid
    $scope.doubleDayStatusChild = function (e) {
        $scope.DayS = e.data;
        for (var i = 0; i < $scope.DayTypeChildList.length; i++) {
            if ($scope.DayTypeChildList[i].Id == $scope.DayS.DayTypeWithValuesId) {
                $scope.DayStatusType = $scope.DayTypeChildList[i].DayType;
            }
        }
    }

    //Delete Day Status
    $scope.DeleteDS = function () {
        console.log("Delete DS");
    }

    // ********************************************** Plant EmpType Child

    $scope.Child = {
        Id: null,
        HeaderId: null,
        PlantId: null,
        EmpTypeId: null,
    };

   
    $scope.EmpTypeList = [];

    // Getting the Plants and the Company List

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }


    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: 'humanresource/RosterPattern/getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();



    //Filling of the Employee Type List

    $scope.fillPlantsEmps = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmpType',

        }).then(function success(response) {
            $scope.EmpTypeList = [];
            $scope.EmpTypeList = response.data;
        })

    }

    $scope.fillPlantsEmps();

    // Refreshing The Child Table
    $scope.childDataList = [];
    function updateChild() {
        $http({
            method: 'POST',
            url: $scope.path + 'getChildData',
            data: { 'MasterId': $scope.Header.Id }
        }).then(function success(resp) {
            $scope.childDataList = [];
            $scope.childDataList = resp.data;
        });
    }


    //Deleting the Child Table
    $scope.DeleteChildData = [];
    $scope.confirmModal = function (data) {
        $scope.DeleteChildData = [];
        $scope.DeleteChildData = data;
        angular.element(document.querySelector('#confirmPOPUPD')).modal('show');
    }

    $scope.DeleteChild = function () {

        var obj = $scope.DeleteChildData;
        if (!baseService.isUndefinedOrNull(obj.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteChild',
                data: { 'id': obj.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    updateChild();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };



    //Save The Child Data

    $scope.saveChild = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ChildForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'saveChild',
                data: { 'Child': $scope.Child }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    console.log(response.data.Data);
                    updateChild();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    }



    // ********************************************** Leave Day Type Codes

    $scope.LeaveDt = {
        HeaderId: null,
        DayTypeWithValuesId: null,
        DayTypeWithValue: null,
    };

    //Getting the DayTypeWithValues
    $scope.LeaveList = [];
    $scope.selectLeaveDayType = function () {

        $http({
            method: 'POST',
            url: $scope.path + 'getleaveDayTypes',
            data: { 'DayTypeWithValuesId': $scope.DayChild.Id }
        }).then(function succ(resp) {
            $scope.LeaveList = [];
            $scope.LeaveList = resp.data;
        });
        angular.element(document.querySelector('#LeaveDayType')).modal('show');
    }

    function LoadLeaveDayType () {
        $http({
            method: 'POST',
            url: $scope.path + 'getleaveDayTypes',
            data: { 'DayTypeWithValuesId': $scope.DayChild.Id }
        }).then(function succ(resp) {
            $scope.LeaveList = [];
            $scope.LeaveList = resp.data;
        });
    }
    
    //$scope.doubleLeaveDayType = function (e) {
    //    $scope.LeaveDt.DayTypeWithValue = e.data.DayType;
    //    $scope.LeaveDt.DayTypeWithValuesId = e.data.Id;

    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'getleaveDayTypes',
    //        data: { 'DayTypeWithValuesId': $scope.LeaveDt.DayTypeWithValuesId }
    //    }).then(function succ(resp) {
    //        $scope.LeaveList = [];
    //        $scope.LeaveList = resp.data;
    //    });

    //    angular.element(document.querySelector('#LeaveDayType')).modal('hide');
    //}

    $scope.saveLeaveDayType = function ()
    {
            $http({
                method: 'POST',
                url: $scope.path + 'saveLeaveDayType',
                data: { 'Data': $scope.LeaveList, 'DayTypeWithValuesId': $scope.LeaveDt.DayTypeWithValuesId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $http({
                        method: 'POST',
                        url: $scope.path + 'getleaveDayTypes',
                        data: { 'DayTypeWithValuesId': $scope.LeaveDt.DayTypeWithValuesId }
                    }).then(function succ(resp) {
                        $scope.LeaveList = [];
                        $scope.LeaveList = resp.data;
                    });
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        
    }

      
}