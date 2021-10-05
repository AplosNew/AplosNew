'use strict';
OTLimitTransactionFromAppController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function OTLimitTransactionFromAppController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'OT Limit Transaction From App';
    $scope.path = 'Attendances/OTLimitTransactionFromApp/';
    $scope.GetOTLimitSettingUrl = $scope.path + 'GetOTLimitSetting';
    $scope.GetOTLimitSettingDetailsUrl = $scope.path + 'GetOTLimitSettingDetails';

    $scope.GetOTLimitOverlapDataUrl = $scope.path + 'GetOTLimitOverlapData';

    $scope.SaveOTLimitOverlapDataUrl = $scope.path + 'SaveOTLimitOverlapData';





    $scope.AttendanceeProcessDataDateWiseUrl = $scope.path + 'GetAttendanceProcessDataDateWise';
    $scope.SaveAttendanceProcessDataDateWiseUrl = $scope.path + 'SaveAttendanceProcessDataDateWise';

    $scope.GetAllEmploteeListUrl = $scope.path + 'GetAllEmploteeList';
    $scope.AttendanceProcessDataEmployeeWiseUrl = $scope.path + 'GetAttendanceProcessDataEmployeeWise';
    $scope.SaveAttendanceProcessDataEmployeeWiseUrl = $scope.path + 'SaveAttendanceProcessDataEmployeeWise';

    $scope.GetAttendanceProcessDataDateRangWiseUrl = $scope.path + 'GetAttendanceProcessDataDateRangWise';
    $scope.GetOTSlabDefineGeneralUrl = $scope.path + 'GetOTSlabDefineGeneral';
    $scope.GetAttendanceProcessUserDefineUrl = $scope.path + 'GetAttendanceProcessUserDefine';
    $scope.GetDetailsDataUrl = $scope.path + 'GetDetailsData';

    $scope.ShowSaveButton = false;













    $scope.CustomPara = {
        YearNo: new Date().getFullYear().toString(),
        MonthNo: new Date().getMonth().toString(),
        OTLimitSettingId: null
    };

    $scope.monthList = [
        {
            Value: 'Jan',
            Text: 'January'
        },
        {
            Value: 'Feb',
            Text: 'February'
        },
        {
            Value: 'Mar',
            Text: 'March'
        },
        {
            Value: 'Apr',
            Text: 'April'
        },
        {
            Value: 'May',
            Text: 'May'
        },
        {
            Value: 'Jun',
            Text: 'June'
        },
        {
            Value: 'Jul',
            Text: 'July'
        },
        {
            Value: 'Aug',
            Text: 'August'
        },
        {
            Value: 'Sep',
            Text: 'September'
        },
        {
            Value: 'Oct',
            Text: 'October'
        },
        {
            Value: 'Nov',
            Text: 'November'
        },
        {
            Value: 'Dec',
            Text: 'December'
        }
    ];
    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    //cboService.getCboLeaveYear(function (result) {
    //    $scope.yearList = result;
    //});

    GetYearList();

    function GetYearList() {
        var FromYear = 2017
        var ToYear = parseInt(new Date().getFullYear().toString());
        while (FromYear <= ToYear) {
            $scope.yearList.push(FromYear);
            FromYear++;
        }
    }


    $scope.OTLimitSettingList = [];

    GetOTLimitSettingList();
    function GetOTLimitSettingList() {
        $http({
            method: 'GET',
            url: $scope.GetOTLimitSettingUrl,
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.Message, 'failure');
            } else {
                $scope.OTLimitSettingList = response.data.data;

            }

        }), function errorCallBack(response) {
            ShowResult(response.Message, 'failure');
        };
    }

    //$scope.OTLimitSettingModel = {
    //    FromDay: null,
    //    ToDay: null,
    //    UserName: null,
    //    Description: null,
    //    OTLimit: null
    //}
    $scope.OTLimitSettingModel = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        FromDay: null,
        ToDay: null,
        UserName: null,
        Description: null,
        Active: true,
        MinOTLimitParDay: null,
        MaxOTLimitParDay: null,
        //MinOTLimitParWeek: null,
        MaxOTLimitParWeek: null,
        OTReductionFactor: null

    }

    $scope.OTLimitOverlapDataList = [];

    $scope.GetOTLimitOverlapData = function () {
        try {
            //var previousDay = null;
            //if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
            //    //
            //}

            //$scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.GetOTLimitOverlapDataUrl + "?YearNo=" + $scope.CustomPara.YearNo
                    + "&MonthNo=" + $scope.CustomPara.MonthNo
                    + "&OTLimitSettingId=" + $scope.CustomPara.OTLimitSettingId,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.OTLimitOverlapDataList = [];
                    $scope.OTLimitOverlapDataList = response.data.data;
                    //$scope.ShowSaveButton = true;
                    //for (var i = 0; i < $scope.AttendanceProcessDataDateWise.length; i++) {
                    //    $scope.AttendanceProcessDataDateWise[i].DOJ = new Date($scope.AttendanceProcessDataDateWise[i].DOJ);
                    //}
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetOTLimitSettingDetails = function () {
        try {
            //var previousDay = null;
            //if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
            //    //
            //}


            $http({
                method: 'GET',
                url: $scope.GetOTLimitSettingDetailsUrl + "?Id=" + $scope.CustomPara.OTLimitSettingId,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {


                    //$scope.OTLimitSettingModel.FromDay = response.data.data[0].FromDay;
                    //$scope.OTLimitSettingModel.ToDay = response.data.data[0].ToDay;

                    $scope.OTLimitSettingModel.MinOTLimitParDay = response.data.data[0].MinOTLimitParDay;
                    $scope.OTLimitSettingModel.MaxOTLimitParDay = response.data.data[0].MaxOTLimitParDay;

                    $scope.OTLimitSettingModel.MaxOTLimitParWeek = response.data.data[0].MaxOTLimitParWeek;
                    $scope.OTLimitSettingModel.OTReductionFactor = response.data.data[0].OTreductionFactor;


                    var YearNo = $scope.CustomPara.YearNo;
                    var MonthNo = $scope.CustomPara.MonthNo;
                    if (response.data.data[0].Week == "First Week") {
                        $scope.OTLimitSettingModel.FromDay = "01-" + MonthNo + "-" + YearNo;
                        $scope.OTLimitSettingModel.ToDay = "07-" + MonthNo + "-" + YearNo;
                    }

                    if (response.data.data[0].Week == "Second Week") {
                        $scope.OTLimitSettingModel.FromDay = "08-" + MonthNo + "-" + YearNo;
                        $scope.OTLimitSettingModel.ToDay = "14-" + MonthNo + "-" + YearNo;
                    }
                    if (response.data.data[0].Week == "Third Week") {
                        $scope.OTLimitSettingModel.FromDay = "15-" + MonthNo + "-" + YearNo;
                        $scope.OTLimitSettingModel.ToDay = "21-" + MonthNo + "-" + YearNo;
                    }
                    if (response.data.data[0].Week == "Last Week") {
                        $scope.OTLimitSettingModel.FromDay = "22-" + MonthNo + "-" + YearNo;
                        //$scope.OTLimitSettingModel.ToDay = Date.parse("01-" + MonthNo + "-" + YearNo).AddMonths(1).AddDays(-1).ToString("dd-MMMM-yyyy");

                        var lastDate = new Date("01-" + MonthNo + "-" + YearNo);

                        //var lastmonth = lastDate.setMonth(lastDate.getMonth() + 1);

                        //var lastdayofmonth = lastmonth.setDate(lastmonth.getDate() - 1);
                        var lastDay = new Date(lastDate.getFullYear(), lastDate.getMonth() + 1, 0);


                        $scope.OTLimitSettingModel.ToDay = $filter('dateFiltering')(lastDay, 'dd-MMM-yyyy');   
                    }


                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };





    $scope.refreshTemplateOTLimitOverlapDataList = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridOTLimitOverlapDataList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.OTLimitOverlapDataList.length; i++) {
                $scope.OTLimitOverlapDataList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridOTLimitOverlapDataList").data("ejGrid");
        gridObj.refreshContent();
    };


    $window.onresize = function (event) {

        $scope.actionCompleteSelected();



    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridOTLimitOverlapDataList").ejGrid("instance");
                var scrollerwidth = $(".site-heading").width();//Obtain the width of the container

                $("#GridOTLimitOverlapDataList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResult(e, 'failure');
        }
    };




    $scope.DetailsDataList = [];


    $scope.GetDetailsData = function (arg) {


        var eDialog = $("#DetailsInfo").data("ejDialog");
        eDialog.open();



        $http({
            method: 'POST',
            url: $scope.GetDetailsDataUrl,
            data:
            {
                'EmpSystemIds': arg.data.EmpSystemID,
                'YearNo': $scope.CustomPara.YearNo,
                'MonthNo': $scope.CustomPara.MonthNo,
                'OTLimitSettingId': $scope.CustomPara.OTLimitSettingId

            },
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                $scope.DetailsDataList = response.data.oOTLimitTransactionFromApp;


            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    };









    $scope.SaveData = function () {

        $scope.employees = [];
        for (var i = 0; i < $scope.OTLimitOverlapDataList.length; i++) {

            if ($scope.OTLimitOverlapDataList[i].CheckBoxSelect === true) {
                $scope.employees.push($scope.OTLimitOverlapDataList[i].EmpSystemID);
            }

        }

        if ($scope.employees.length == 0) {
            throw "Please select data";
        }

        //$scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');




        $http({
            method: 'POST',
            url: $scope.SaveOTLimitOverlapDataUrl,
            data:
            {
                'EmpSystemIds': $scope.employees,
                'YearNo': $scope.CustomPara.YearNo,
                'MonthNo': $scope.CustomPara.MonthNo,
                'OTLimitSettingId': $scope.CustomPara.OTLimitSettingId

            },
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.GetOTLimitOverlapData();

            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    };




    $scope.AttendanceProcessDataDateWise = [];
    $scope.xcustomPara = {
        procdate: $filter('dateFiltering')(Date.now()),
        fromdate: null,
        todate: null

    };
    $scope.onrowdatabound = function (e) {
        if (e.data.IsManualInTime === 'YES')
            e.row.css("background-color", "red");

    };
    $scope.onrowdatabound1 = function (e) {
        if (e.data.IsManualInTime === 'YES')
            e.row.css("background-color", "red");

    };
    $scope.GetAttendanceProcessDataDateWise = function () {
        try {
            //var previousDay = null;
            if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
                //
            }

            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.AttendanceeProcessDataDateWiseUrl + "?WDate=" + $scope.day,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.AttendanceProcessDataDateWise = [];
                    $scope.AttendanceProcessDataDateWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    for (var i = 0; i < $scope.AttendanceProcessDataDateWise.length; i++) {
                        $scope.AttendanceProcessDataDateWise[i].DOJ = new Date($scope.AttendanceProcessDataDateWise[i].DOJ);
                    }
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };







    // #region Tab











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
    // #endregion

    // #endregion



    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridAttendanceProcessDataEmployeeWise").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };









    // employee wise 


    $scope.Employeelist = [];
    $scope.getAllEmployee = function () {
        try {
            //var previousDay = null;
            if (baseService.isUndefinedOrNull($scope.customPara.fromdate)) {
                throw "Enter From Date";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.todate)) {
                throw "Enter to Date";
            }
            if (new Date($scope.customPara.fromdate) > new Date($scope.customPara.todate)) {
                throw "From date must be less than or equal to date";
            }

            $http({
                method: 'GET',
                url: $scope.GetAllEmploteeListUrl,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.Employeelist = response.data.data;
                    var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
                    eDialog.open();
                    $scope.ShowSaveButton = true;
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.employees = {};
    $scope.AttendanceProcessDataEmployeeWise = [];
    $scope.selectSignleEmployee = function () {
        try {
            $scope.employees = {};
            var gridObj = $("#GridWorkCenterProduct").data("ejGrid");
            $scope.employees = gridObj.getSelectedRecords()[0];
            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
            eDialog.close();


            $scope.GetAttendanceProcessDataEmployeeWise();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetAttendanceProcessDataEmployeeWise = function () {
        try {



            var FromDate = $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todate, 'dd-M-yyyy');
            if (new Date(FromDate) > new Date(ToDate)) {
                throw "From date must be less than or equal to date";
            }
            $http({
                method: 'GET',
                url: $scope.AttendanceProcessDataEmployeeWiseUrl + "?FromDate=" + FromDate + "&ToDate=" + ToDate + "&EmpSystemId=" + $scope.employees.SystemId,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.AttendanceProcessDataEmployeeWise = [];
                    $scope.AttendanceProcessDataEmployeeWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    //for (var i = 0; i < $scope.AttendanceRawDataEmployeeWise.length; i++) {
                    //    $scope.AttendanceRawDataEmployeeWise[i].DOJ = new Date($scope.AttendanceRawDataEmployeeWise[i].DOJ);
                    //    $scope.AttendanceRawDataEmployeeWise[i].PDate = new Date($scope.AttendanceRawDataEmployeeWise[i].PDate);
                    //}
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateAttendanceRawDataEmployeeWise = function (args) {
        $("#headchk1").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise1 });
    };

    function CheckBoxSelectAllEmolyeeWise1(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridAttendanceProcessDataEmployeeWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AttendanceProcessDataEmployeeWise.length; i++) {
                $scope.AttendanceProcessDataEmployeeWise[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridAttendanceProcessDataEmployeeWise").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.SaveDataEmployeeWise = function () {


        try {
            $scope.employees = [];

            if ($scope.tabh === 11) {
                $scope.employees = [];
                for (var i = 0; i < $scope.AttendanceProcessDataEmployeeWise.length; i++) {

                    if ($scope.AttendanceProcessDataEmployeeWise[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.AttendanceProcessDataEmployeeWise[i]);
                    }

                }
            }

            if ($scope.employees.length == 0) {
                throw "Please select data";
            }



            var FromDate = $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy');
            var Todate = $filter('dateFiltering')($scope.customPara.todate, 'dd-M-yyyy');
            //$.ajax({
            //    type: "POST",
            //    url: $scope.SaveAttendanceProcessDataEmployeeWiseUrl,
            //    data:
            //    {
            //        'AttendanceProcessData': $scope.employees,
            //        'pFromDate': FromDate,
            //        'pToDate': Todate
            //    },
            //    dataType: "json",
            //    success: function (response) {


            //        if (response.Error === true) {

            //            ShowResult(response.Message, 'failure');
            //        }
            //        else {


            //            ShowResult(response.Message, 'success');
            //            $scope.GetAttendanceProcessDataEmployeeWise();
            //        }


            //    }

            //});

            $http({
                method: 'POST',
                url: $scope.SaveAttendanceProcessDataEmployeeWiseUrl,
                data:
                {
                    'AttendanceProcessData': $scope.employees,
                    'pFromDate': FromDate,
                    'pToDate': Todate
                },
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetAttendanceProcessDataEmployeeWise();
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };






        } catch (e) {
            ShowResult(e, "failure");
        }

    };



    /////////////////////////////////////////////////////////
    $scope.actionCompleteSelectedDateRangeWise = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridAttendanceProcessDataDateRangeWise").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };
    $scope.actionCompleteSelectedDetailsDateRangeWise = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridRangWise").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };


    $scope.refreshTemplateAttendancDateRangeWise = function (args) {
        $("#headchk3").ejCheckBox({ "change": CheckBoxSelectAllDateRangeWise });
    };

    function CheckBoxSelectAllDateRangeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridRangWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AttendanceProcessDetailsDataDateRangWise.length; i++) {
                $scope.AttendanceProcessDetailsDataDateRangWise[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridRangWise").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SearchPartRangWise = true;
    $scope.DataPartRangWise = false;

    $scope.NWDayType = null;
    $scope.HDayType = null;
    $scope.WDayType = null;

    $scope.AttendanceProcessDataDateRangWise = [];
    $scope.AttendanceProcessDetailsDataDateRangWise = [];
    $scope.GetAttendanceProcessDataDateRangWise = function () {
        try {



            var FromDate = $filter('dateFiltering')($scope.customPara.fromdaterange, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todaterange, 'dd-M-yyyy');
            if (new Date(FromDate) > new Date(ToDate)) {
                throw "From date must be less than or equal to date";
            }
            $http({
                method: 'GET',
                url: $scope.GetAttendanceProcessDataDateRangWiseUrl + "?FromDate=" + FromDate + "&ToDate=" + ToDate,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.AttendanceProcessDataDateRangWise = [];
                    $scope.AttendanceProcessDataDateRangWise = response.data.data;
                    $scope.ShowSaveButton = true;

                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.GetAttendanceProcessDetailsDataDateRangWise = function () {
        try {
            //var previousDay = null;
            var gridObj = $("#GridAttendanceProcessDataDateRangeWise").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];

            $scope.day = $filter('dateFiltering')(modeldata.WorkDate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.AttendanceeProcessDataDateWiseUrl + "?WDate=" + $scope.day,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.SearchPartRangWise = false;
                    $scope.DataPartRangWise = true;
                    $scope.AttendanceProcessDetailsDataDateRangWise = [];
                    $scope.AttendanceProcessDetailsDataDateRangWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    for (var i = 0; i < $scope.AttendanceProcessDetailsDataDateRangWise.length; i++) {
                        $scope.AttendanceProcessDetailsDataDateRangWise[i].DOJ = new Date($scope.AttendanceProcessDetailsDataDateRangWise[i].DOJ);
                    }

                    $scope.GetOTSlabDefineGeneral();
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetOTSlabDefineGeneral = function () {
        try {
            //var previousDay = null;
            var gridObj = $("#GridAttendanceProcessDataDateRangeWise").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];

            $scope.day = $filter('dateFiltering')(modeldata.WorkDate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.GetOTSlabDefineGeneralUrl + "?WDate=" + $scope.day,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {


                    $scope.NWDayType = response.data.NWDayType;
                    $scope.HDayType = response.data.HDayType;
                    $scope.WDayType = response.data.WDayType;
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetAttendanceProcessDetailsDataUserDefineWise = function () {
        try {
            //var previousDay = null;
            var gridObj = $("#GridAttendanceProcessDataDateRangeWise").data("ejGrid");
            var modeldata = gridObj.getSelectedRecords()[0];

            $scope.day = $filter('dateFiltering')(modeldata.WorkDate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.GetAttendanceProcessUserDefineUrl + "?WDate=" + $scope.day + "&NWDayType=" + $scope.NWDayType + "&HDayType=" + $scope.HDayType + "&WDayType=" + $scope.WDayType,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.SearchPartRangWise = false;
                    $scope.DataPartRangWise = true;
                    $scope.AttendanceProcessDetailsDataDateRangWise = [];
                    $scope.AttendanceProcessDetailsDataDateRangWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    for (var i = 0; i < $scope.AttendanceProcessDetailsDataDateRangWise.length; i++) {
                        $scope.AttendanceProcessDetailsDataDateRangWise[i].DOJ = new Date($scope.AttendanceProcessDetailsDataDateRangWise[i].DOJ);
                    }


                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveDataDateRangeWise = function () {


        $scope.employees = [];

        if ($scope.tabh === 11) {
            //
        }
        else if ($scope.tabh === 22) {
            //
        }
        else if ($scope.tabh === 33) {
            $scope.employees = [];
            for (var i = 0; i < $scope.AttendanceProcessDetailsDataDateRangWise.length; i++) {

                if ($scope.AttendanceProcessDetailsDataDateRangWise[i].CheckBoxSelect === true) {
                    $scope.employees.push($scope.AttendanceProcessDetailsDataDateRangWise[i]);
                }

            }
        }


        if ($scope.employees.length == 0) {
            throw "Please select data";
        }

        var gridObj = $("#GridAttendanceProcessDataDateRangeWise").data("ejGrid");
        var modeldata = gridObj.getSelectedRecords()[0];
        $scope.day = $filter('dateFiltering')(modeldata.WorkDate, 'dd-M-yyyy');

        //$.ajax({
        //    type: "POST",
        //    url: $scope.SaveAttendanceProcessDataDateWiseUrl,
        //    data:
        //    {
        //        'AttendanceProcessData': $scope.employees,
        //        'WDate': $scope.day

        //    },
        //    dataType: "json",
        //    success: function (response) {


        //        if (response.Error === true) {

        //            ShowResult(response.Message, 'failure');
        //        }
        //        else {
        //            ShowResult(response.Message, 'success');

        //            $scope.GetAttendanceProcessDetailsDataUserDefineWise();
        //        }

        //    }

        //});


        $http({
            method: 'POST',
            url: $scope.SaveAttendanceProcessDataDateWiseUrl,
            //data: JSON.stringify(data),
            data:
            {
                'AttendanceProcessData': $scope.employees,
                'WDate': $scope.day
            },
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.GetAttendanceProcessDetailsDataUserDefineWise();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };




    };


    $scope.Back = function () {
        $scope.SearchPartRangWise = true;
        $scope.DataPartRangWise = false;
        $scope.GetAttendanceProcessDataDateRangWise();
    };
}