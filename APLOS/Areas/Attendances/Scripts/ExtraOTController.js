'use strict';
ExtraOTController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function ExtraOTController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Extra OT';
    $scope.path = 'Attendances/ExtraOT/';
    $scope.AttendanceeProcessDataDateWiseUrl = $scope.path + 'GetAttendanceProcessDataDateWise';
    $scope.SaveAttendanceProcessDataDateWiseUrl = $scope.path + 'SaveAttendanceProcessDataDateWise';

    $scope.GetAllEmploteeListUrl = $scope.path + 'GetAllEmploteeList';
    $scope.AttendanceProcessDataEmployeeWiseUrl = $scope.path + 'GetAttendanceProcessDataEmployeeWise';
    $scope.SaveAttendanceProcessDataEmployeeWiseUrl = $scope.path + 'SaveAttendanceProcessDataEmployeeWise';

    $scope.GetAttendanceProcessDataDateRangWiseUrl = $scope.path + 'GetAttendanceProcessDataDateRangWise';
    $scope.GetOTSlabDefineGeneralUrl = $scope.path + 'GetOTSlabDefineGeneral';
    $scope.GetAttendanceProcessUserDefineUrl = $scope.path + 'GetAttendanceProcessUserDefine';

    $scope.ShowSaveButton = false;
    $scope.AttendanceProcessDataDateWise = [];
    $scope.customPara = {
        procdate: $filter('dateFiltering')(Date.now()),
        fromdate: null,
        todate: null
       
    };
    $scope.onrowdatabound = function (e) {
        if (e.data.ManualInTimeFlag === 'YES')
            e.row.css("background-color", "red");
       
    };
    $scope.onrowdatabound1 = function (e) {
        if (e.data.ManualInTimeFlag === 'YES')
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
                url: $scope.AttendanceeProcessDataDateWiseUrl + "?WDate=" + $scope.day  ,
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

    $scope.SaveData = function () {


        $scope.employees = [];

        if ($scope.tabh === 11) {
           //
        }
        else if ($scope.tabh === 22) {
            $scope.employees = [];
            for (var i = 0; i < $scope.AttendanceProcessDataDateWise.length; i++) {

                if ($scope.AttendanceProcessDataDateWise[i].CheckBoxSelect === true) {
                    $scope.employees.push($scope.AttendanceProcessDataDateWise[i]);
                }

            }
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

        $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');

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

        //            $scope.GetAttendanceProcessDataDateWise();
        //        }

        //    }

        //});


        $http({
            method: 'POST',
            url: $scope.SaveAttendanceProcessDataDateWiseUrl,
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
                $scope.GetAttendanceProcessDataDateWise();
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

    };


    $scope.refreshTemplateAttendanceRawDataDateWise = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AttendanceProcessDataDateWise.length; i++) {
                $scope.AttendanceProcessDataDateWise[i].CheckBoxSelect = ChkOrUnchk;
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

   
    $window.onresize = function (event) {

        $scope.actionCompleteSelected();
        $scope.actionCompleteSelected1();
        

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
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
                url: $scope.GetAllEmploteeListUrl ,
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

            if ($scope.employees.length==0) {
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
                url: $scope.GetAttendanceProcessDataDateRangWiseUrl + "?FromDate=" + FromDate + "&ToDate=" + ToDate ,
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