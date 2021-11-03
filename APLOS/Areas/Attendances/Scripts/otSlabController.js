'use strict';
otSlabController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function otSlabController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'OT Slab';
    $scope.WDAction = 'Save';
    $scope.WOAction = 'Save';
    $scope.HDAction = 'Save';
    //$scope.Action = 'Save';
    $scope.path = 'Attendances/OTSlab/';

    $scope.getWorkingDayListUrl = $scope.path + 'getWorkingDaylist';
    $scope.saveWorkingDayUrl = $scope.path + 'SaveWorkingDay';
    $scope.deleteWorkingDayUrl = $scope.path + 'Deleteworkingday/';

    $scope.getWeekOffListUrl = $scope.path + 'getWeekOfflist';
    $scope.saveWeekOffUrl = $scope.path + 'SaveWeekOffDay';
    $scope.deleteWeekOffUrl = $scope.path + 'DeleteWeekOffday/';

    $scope.getHolidayListUrl = $scope.path + 'getHolidaylist';
    $scope.saveHolidayUrl = $scope.path + 'SaveHolidayDay';
    $scope.deleteHolidayUrl = $scope.path + 'DeleteHolidayday/';

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
    $scope.setTab44 = function (newTab) {
        $scope.tabh = newTab;

    };

    $scope.getData = function () {
        $scope.getWorkingDayListData();
        $scope.HolidayListData();
        $scope.getWeekOffListData();

    };

    // #endregion Tab

    //For Company and Plant ID
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.WorkingDayModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.WorkingDayModel = {
        SystemID: null,
        GroupID: null,
        PlantID: null,
        DayType: 'NW',
        FromDate: null,
        ToDate: null,
        firstSlab: null,

        OTStartFrom: 'PunchInTime',

       // IsTotalWorkTimeAsOT: false,
    };

    $scope.WeekOffModel = {
        SystemID: null,
        GroupID: null,
        PlantID: null,
        DayType: 'W',
        FromDate: null,
        ToDate: null,
        firstSlab: null,

        //OTStartFrom: 'PunchInTime',

        IsTotalWorkTimeAsOT: false,
        IsTotalWorkTimeAsOTFromShift: false
    };

    $scope.HolidayModel = {
        SystemID: null,
        GroupID: null,
        PlantID: null,
        DayType: 'H',
        FromDate: null,
        ToDate: null,
        firstSlab: null,

        //OTStartFrom: 'PunchInTime',

        //IsTotalWorkTimeAsOT: false,
        IsTotalWorkTimeAsOT: false,
        IsTotalWorkTimeAsOTFromShift: false
    };

    $scope.WorkingDayList = [];
    $scope.getWorkingDayListData = function () {
        $scope.WorkingDayModel.SystemID = null;
        $scope.WorkingDayModel.DayType = 'NW';
        $scope.WorkingDayModel.FromDate = null;
        $scope.WorkingDayModel.ToDate = null;
        $scope.WorkingDayModel.firstSlab = null;

        $scope.WorkingDayModel.IsMandatoryAlignWithSalary = false;
        $http({
            method: 'POST',
            url: 'Attendances/OTSlab/getWorkingDaylist',
            data: { PlantID: $scope.WorkingDayModel.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (baseService.arrayLength(response.data) > 0) {
                $scope.WorkingDayModel.SystemID = response.data[0].SystemID;
                $scope.WorkingDayModel.DayType = response.data[0].DayType;
                $scope.WorkingDayModel.FromDate = response.data[0].FromDate;
                $scope.WorkingDayModel.ToDate = response.data[0].ToDate;
                $scope.WorkingDayModel.firstSlab = response.data[0].firstSlab;

                $scope.WorkingDayModel.IsMandatoryAlignWithSalary = response.data[0].IsMandatoryAlignWithSalary;
            }
            function errorCallback(response) {
                ShowResult(response, 'failure');
            }
        });
        
    }


    $scope.WeekOffList = [];
    $scope.getWeekOffListData = function () {
        $scope.WeekOffModel.SystemID = null;           
        $scope.WeekOffModel.DayType = 'W';
        $scope.WeekOffModel.FromDate = null;
        $scope.WeekOffModel.ToDate = null;
        $scope.WeekOffModel.firstSlab = null;

        $scope.WeekOffModel.IsTotalWorkTimeAsOT = false;
        $scope.WeekOffModel.IsTotalWorkTimeAsOTFromShift = false;
        $http({
            method: 'POST',
            url: 'Attendances/OTSlab/getWeekOfflist',
            data: { PlantID: $scope.WorkingDayModel.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.WeekOffModel.SystemID = response.data[0].SystemID;
                $scope.WeekOffModel.DayType = response.data[0].DayType;
                $scope.WeekOffModel.FromDate = response.data[0].FromDate;
                $scope.WeekOffModel.ToDate = response.data[0].ToDate;
                $scope.WeekOffModel.firstSlab = response.data[0].firstSlab;

                $scope.WeekOffModel.IsTotalWorkTimeAsOT = response.data[0].IsTotalWorkTimeAsOT;
                $scope.WeekOffModel.IsTotalWorkTimeAsOTFromShift = response.data[0].IsTotalWorkTimeAsOTFromShift;
            }
            function errorCallback(response) {
                ShowResult(response, 'failure');
            }
        });

    }

    $scope.HolidayList = [];

    $scope.HolidayListData = function () {
        $scope.HolidayModel.SystemID = null;
        $scope.HolidayModel.DayType = 'H';
        $scope.HolidayModel.FromDate = null;
        $scope.HolidayModel.ToDate = null;
        $scope.HolidayModel.firstSlab = null;

        $scope.HolidayModel.IsTotalWorkTimeAsOT = false;
        $scope.HolidayModel.IsTotalWorkTimeAsOTFromShift = false;
        $http({
            method: 'POST',
            url: 'Attendances/OTSlab/getHolidaylist',
            data: { PlantID: $scope.WorkingDayModel.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.HolidayModel.SystemID = response.data[0].SystemID;
                $scope.HolidayModel.DayType = response.data[0].DayType;
                $scope.HolidayModel.FromDate = response.data[0].FromDate;
                $scope.HolidayModel.ToDate = response.data[0].ToDate;
                $scope.HolidayModel.firstSlab = response.data[0].firstSlab;

                $scope.HolidayModel.IsTotalWorkTimeAsOT = response.data[0].IsTotalWorkTimeAsOT;
                $scope.HolidayModel.IsTotalWorkTimeAsOTFromShift = response.data[0].IsTotalWorkTimeAsOTFromShift;
            }
            function errorCallback(response) {
                ShowResult(response, 'failure');
            }
        });

    }

    $scope.SaveWorkingDay = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.WorkingDayModel.FromDate)) {
                throw 'From Date is required.';
            }

            else if (baseService.isUndefinedOrNull($scope.WorkingDayModel.ToDate)) {
                throw 'To Date is required.';
            }

            else if (new Date($scope.WorkingDayModel.FromDate) > new Date($scope.WorkingDayModel.ToDate)) {
                throw 'From date must be below or equal to To Date';
            }

            else if (new Date($scope.WorkingDayModel.ToDate) < new Date($scope.WorkingDayModel.FromDate)) {
                throw 'To date must be above or equal to From Date.';
            }
            ValidationWorkingDay();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveWorkingDayUrl,
                data: { 'OTSlabDefineGeneral': $scope.WorkingDayModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getWorkingDayListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveWeekOff = function () {
        $scope.WeekOffModel.PlantID = $scope.WorkingDayModel.PlantID;
        try {
            if (baseService.isUndefinedOrNull($scope.WeekOffModel.FromDate)) {
                throw 'From Date is required.';
            }

            else if (baseService.isUndefinedOrNull($scope.WeekOffModel.ToDate)) {
                throw 'To Date is required.';
            }

            else if (new Date($scope.WeekOffModel.FromDate) > new Date($scope.WeekOffModel.ToDate)) {
                throw 'From date must be below or equal to To Date';
            }

            else if (new Date($scope.WeekOffModel.ToDate) < new Date($scope.WeekOffModel.FromDate)) {
                throw 'To date must be above or equal to From Date.';
            }

            if ($scope.WeekOffModel.IsTotalWorkTimeAsOT == true && $scope.WeekOffModel.IsTotalWorkTimeAsOTFromShift == true) {
                throw 'Punch Time and From Shift Cannot be Checked at the same time.';
            }
            if ($scope.WeekOffModel.IsTotalWorkTimeAsOT != true) {
                $scope.WeekOffModel.IsTotalWorkTimeAsOTFromShift == true
            }
            else {
                $scope.WeekOffModel.IsTotalWorkTimeAsOT == true
            }

            ValidationWeekOff();
            $scope.$broadcast('show-errors-check-validity');
            
            $http({
                method: 'POST',
                url: $scope.saveWeekOffUrl,
                data: { 'OTSlabDefineGeneral': $scope.WeekOffModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    
                    $scope.getWeekOffListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveHoliday = function () {
        $scope.HolidayModel.PlantID = $scope.WorkingDayModel.PlantID;
        try {
   if (baseService.isUndefinedOrNull($scope.HolidayModel.FromDate)) {
                throw 'From Date is required.';
            }

            else if (baseService.isUndefinedOrNull($scope.HolidayModel.ToDate)) {
                throw 'To Date is required.';
            }

            else if (new Date($scope.HolidayModel.FromDate) > new Date($scope.HolidayModel.ToDate)) {
                throw 'From date must be below or equal to To Date';
            }

            else if (new Date($scope.HolidayModel.ToDate) < new Date($scope.HolidayModel.FromDate)) {
                throw 'To date must be above or equal to From Date.';
            }
            if ($scope.HolidayModel.IsTotalWorkTimeAsOT == true && $scope.HolidayModel.IsTotalWorkTimeAsOTFromShift == true) {
                throw 'Punch Time and From Shift Cannot be Checked at the same time.';
            }
            if ($scope.HolidayModel.IsTotalWorkTimeAsOT != true) {
                $scope.HolidayModel.IsTotalWorkTimeAsOTFromShift == true
            }
            else {
                $scope.HolidayModel.IsTotalWorkTimeAsOT == true
            }
            ValidationHoliday();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveHolidayUrl,
                data: { 'OTSlabDefineGeneral': $scope.HolidayModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                    
                    $scope.HolidayListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteWeekOff = function () {
        if (!baseService.isUndefinedOrNull($scope.WeekOffModel.SystemID)) {
            $http.get('Attendances/OTSlab/Deleteworkingday?SystemID=' + $scope.WeekOffModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.WeekOffModel = {
                            SystemID: null,
                            GroupID: null,
                            PlantID: null,
                            DayType: 'W',
                            FromDate: null,
                            ToDate: null,
                            firstSlab: null,
                            OTStartFrom: 'PunchInTime',

                            //IsTotalWorkTimeAsOT: false,
                            // IsTotalWorkTimeAsOTFromShift: false
                        };
                        $scope.ClearWeekOff();
                        $scope.getWeekOffListData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteWorkingDay = function () {
        if (!baseService.isUndefinedOrNull($scope.WorkingDayModel.SystemID)) {
            $http.get('Attendances/OTSlab/Deleteworkingday?SystemID=' + $scope.WorkingDayModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.WorkingDayModel = {
                            SystemID: null,
                            GroupID: null,
                            PlantID: null,
                            DayType: 'NW',
                            FromDate: null,
                            ToDate: null,
                            firstSlab: null,
                            OTStartFrom: 'PunchInTime',

                            // IsTotalWorkTimeAsOT: false,
                        };
                        $scope.ClearWorkingDay();
                        $scope.getWorkingDayListData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteHolidayday = function () {
        if (!baseService.isUndefinedOrNull($scope.HolidayModel.SystemID)) {
            $http.get('Attendances/OTSlab/DeleteHolidayday?SystemID=' + $scope.HolidayModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.HolidayModel = {
                            SystemID: null,
                            GroupID: null,
                            PlantID: null,
                            DayType: 'H',
                            FromDate: null,
                            ToDate: null,
                            firstSlab: null,
                            OTStartFrom: 'PunchInTime',

                            //IsTotalWorkTimeAsOT: false,
                        };
                        $scope.ClearHoliday();
                        $scope.HolidayListData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.ClearWorkingDay = function () {

        $scope.WDAction = 'Save';

        $scope.WorkingDayModel = {
            CompanyId: $scope.WorkingDayModel.CompanyId,
            PlantID: $scope.WorkingDayModel.PlantID,
            SystemID: null,
            DayType: 'NW',
            FromDate: null,
            ToDate: null,
            firstSlab: null,
            OTStartFrom: 'PunchInTime',
            // IsTotalWorkTimeAsOT: false,
        };
        //$scope.WorkingDayList = [];
        //$scope.plantList = [];
    };
    $scope.ClearWeekOff = function () {
        
        $scope.WOAction = 'Save';
        
        $scope.WeekOffModel = {
            SystemID: null,
            DayType: 'W',
            FromDate: null,
            ToDate: null,
            firstSlab: null,
            OTStartFrom: 'PunchInTime',
            

            //IsTotalWorkTimeAsOT: false,
            // IsTotalWorkTimeAsOTFromShift: false
        };
        //$scope.WeekOffList = [];
        
    };
    $scope.ClearHoliday = function () {

        $scope.HDAction = 'Save';
        $scope.HolidayModel = {
            SystemID: null,
            DayType: 'H',
            FromDate: null,
            ToDate: null,
            firstSlab: null,
            OTStartFrom: 'PunchInTime',

            //IsTotalWorkTimeAsOT: false,
        };
        //$scope.HolidayList = [];
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationWorkingDay() {
        try {
            CheckField("Over Time For A Day", $scope.WorkingDayModel.firstSlab);
        } catch (ex) {
            throw ex;
        }
    }
    function ValidationWeekOff() {
        try {
            CheckField("Over Time For A Day", $scope.WeekOffModel.firstSlab);
        } catch (ex) {
            throw ex;
        }
    }
   function ValidationHoliday() {
        try {
            CheckField("Over Time For A Day", $scope.HolidayModel.firstSlab);
        } catch (ex) {
            throw ex;
        }
    }

}