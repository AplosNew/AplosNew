'use strict';
IndividualAttendanceLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function IndividualAttendanceLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Individual Attendance Lock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';

    $scope.loadSepratedEmpListUrl = $scope.path + 'LoadSeparatedEmployeeList';
    $scope.LoadWorkDateSeparatedEmployeeUrl = $scope.path + 'LoadWorkDateSeparatedEmployee';
    $scope.CreateSeparatedEmployeeAttendanceLockUrl = $scope.path + 'CreateSeparatedEmployeeAttendanceLock';


    $scope.loadMLVEmpListUrl = $scope.path + 'LoadMLVEmployeeList';
    //$scope.LoadWorkDateSeparatedEmployeeUrl = $scope.path + 'LoadWorkDateSeparatedEmployee';
    $scope.CreateMLVEmployeeAttendanceLockUrl = $scope.path + 'CreateMLVEmployeeAttendanceLock';
    $scope.GetOutPunchMissingDataForAlertUrl = $scope.path + 'GetOutPunchMissingDataForAlertEmpWise';


    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.OutPunchMissingDataForAlert = [];
    $scope.MLVPart = false;
    $scope.IsSeparatedPart = false;
    $scope.customPara = {
        fromdate: null,
        todate: null,
        fromdateWD: null,
        todateWD: null,
        mlvfromdate: null,
        mlvtodate: null
    };


    //#region Tab




    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;

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


    // #endregion Tab

    $window.onresize = function (event) {
        $scope.actionCompleteSelectedSelectEmployee();
        $scope.actionCompleteSeparatedEmployee();
        $scope.actionCompleteSelected1();


    };
    $scope.actionCompleteSelectedSelectEmployee = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridSelectEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridSelectEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    ///////// select all work date
    $scope.refreshTemplateWorkDateList = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridSelectEmployeeData").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.WorkDateList.length; i++) {
                if ($scope.WorkDateList[i].LockedStatus !== 'Lock') {
                    $scope.WorkDateList[i].CheckBoxSelect = ChkOrUnchk;
                }
              
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                if (filtered[j].LockedStatus !== 'Lock') {
                    filtered[j].CheckBoxSelect = ChkOrUnchk;
                }
           
            }


        }
        var gridObj = $("#GridSelectEmployeeData").data("ejGrid");
        gridObj.refreshContent();
    };



    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridMLVEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridMLVEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteSeparatedEmployee = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridSeparatedEmployeeListTemp").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridSeparatedEmployeeListTemp").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteMLVEmployee = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridMLVEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridMLVEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    /////================================================================seprated=====================================================================
    $scope.SeparatedEmpList = [];
    $scope.employees = {};
    $scope.getSeparatedEmpList = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.customPara.fromdate)) {
                throw "Select from data";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.todate)) {
                throw "Select to data";
            }
            if (new Date($scope.customPara.fromdate) > new Date($scope.customPara.todate)) {
                throw "From date must be less than or equal to date";
            }
            $http({
                method: "GET",
                dataType: 'JSON',
                //data: { 'lockDate': $scope.customPara.lockDate},
                url: $scope.loadSepratedEmpListUrl + '?FromDate=' + $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy') + '&ToDate=' + $scope.customPara.todate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    $scope.SeparatedEmpList = [];
                    $scope.SeparatedEmpList = response.data.data;
                    for (var i = 0; i < $scope.SeparatedEmpList.length; i++) {
                        $scope.SeparatedEmpList[i].DOJ = new Date($scope.SeparatedEmpList[i].DOJ);
                        $scope.SeparatedEmpList[i].DOS = new Date($scope.SeparatedEmpList[i].DOS);
                    }

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.selectSignleEmployee = function () {
        try {
            $scope.employees = {};
            if ($scope.tabh == 11) {
                var gridObj = $("#GridSeparatedEmployeeListTemp").data("ejGrid");
                $scope.employees = gridObj.getSelectedRecords()[0];
                $scope.LoadWorkDateSeparatedEmployee();
                $scope.MLVPart = false;
                $scope.IsSeparatedPart = true;
            }
            if ($scope.tabh == 22) {
                var gridObjMLV = $("#GridMLVEmployeeList").data("ejGrid");
                $scope.employees = gridObjMLV.getSelectedRecords()[0];
                $scope.LoadWorkDateMLVEmployee();
                $scope.MLVPart = true;
                $scope.IsSeparatedPart = false;
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.WorkDateList = [];
    $scope.LoadWorkDateSeparatedEmployee = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.customPara.fromdate)) {
                throw "Enter from Date";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.todate)) {
                throw "Enter to Date";
            }

            if (new Date($scope.customPara.fromdate) > new Date($scope.customPara.todate)) {
                throw "From date must be less than or equal to date";
            }
            var FromDate = $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todate, 'dd-M-yyyy');

            $scope.GetWorkDates(FromDate, ToDate, $scope.employees.SystemID);
            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
            eDialog.open();
            $scope.customPara.fromdateWD = FromDate;
            $scope.customPara.todateWD = ToDate;



        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadWorkDateMLVEmployee = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.customPara.mlvfromdate)) {
                throw "Enter from Date";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.mlvtodate)) {
                throw "Enter to Date";
            }
            if (new Date($scope.customPara.mlvfromdate) > new Date($scope.customPara.mlvtodate)) {
                throw "From date must be less than or equal to date";
            }
            var FromDate = $filter('dateFiltering')($scope.customPara.mlvfromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.mlvtodate, 'dd-M-yyyy');
          

            $scope.GetWorkDates(FromDate, ToDate, $scope.employees.SystemID);
            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
            eDialog.open();
            $scope.customPara.fromdateWD = FromDate;
            $scope.customPara.todateWD = ToDate;



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.LoadWorkDate = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.customPara.fromdateWD)) {
                throw "Enter from Date";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.todateWD)) {
                throw "Enter to Date";
            }
            if (new Date($scope.customPara.fromdateWD) > new Date($scope.customPara.todateWD)) {
                throw "From date must be less than or equal to date";
            }
            var FromDate = $filter('dateFiltering')($scope.customPara.fromdateWD, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todateWD, 'dd-M-yyyy');

            $scope.GetWorkDates(FromDate, ToDate, $scope.employees.SystemID);




        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.GetWorkDates = function (fromdate, todate, EmpSystemId) {
        try {


            if (baseService.isUndefinedOrNull(fromdate)) {
                throw "Enter from Date";
            }
            if (baseService.isUndefinedOrNull(todate)) {
                throw "Enter to Date";
            }
            if (new Date(fromdate) > new Date(todate)) {
                throw "From date must be less than or equal to date";
            }
            var FromDate = $filter('dateFiltering')(fromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')(todate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.LoadWorkDateSeparatedEmployeeUrl + "?FromDate=" + FromDate + "&ToDate=" + ToDate + "&EmpSystemId=" + EmpSystemId,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                } else {
                    $scope.WorkDateList = [];
                    $scope.WorkDateList = response.data.data;
                    $scope.ShowSaveButton = true;
                    //for (var i = 0; i < $scope.AttendanceRawDataEmployeeWise.length; i++) {
                    //    $scope.AttendanceRawDataEmployeeWise[i].DOJ = new Date($scope.AttendanceRawDataEmployeeWise[i].DOJ);
                    //    $scope.AttendanceRawDataEmployeeWise[i].PDate = new Date($scope.AttendanceRawDataEmployeeWise[i].PDate);
                    //}
                    var gridObj = $("#GridSelectEmployeeData").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.clearFiltering();                    
                    gridObj.gotoPage(1);


                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };





    $scope.SaveSeparatedLockDateList = function () {
        try {


            var LockDates = [];
            for (var i = 0; i < $scope.WorkDateList.length; i++) {
                if ($scope.WorkDateList[i].CheckBoxSelect == true) {
                    LockDates.push($scope.WorkDateList[i].WorkDate);
                }
            }

            if (LockDates.length == 0) {
                throw "Select data";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateSeparatedEmployeeAttendanceLockUrl,
                data: { 'LockDates': LockDates, 'EmployeeSystemId': $scope.employees.SystemID }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    
                    var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
                    eDialog.close();

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.SaveDataWithAlert = function () {


        try {
            

            var LockDates = [];
            for (var i = 0; i < $scope.WorkDateList.length; i++) {
                if ($scope.WorkDateList[i].CheckBoxSelect == true) {
                    LockDates.push($scope.WorkDateList[i].WorkDate);
                }
            }

            if (LockDates.length == 0) {
                throw "Select data";
            }

            $scope.OutPunchMissingDataForAlert = [];

            $http({
                method: "POST",
                dataType: 'JSON',               
                data: { 'LockDates': LockDates, 'EmployeeSystemId': $scope.employees.SystemID },
                url: $scope.GetOutPunchMissingDataForAlertUrl

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.OutPunchMissingDataForAlert = response.data;

                    if ($scope.OutPunchMissingDataForAlert.length > 0) {
                        var eDialog = $("#dialogMessageAlert").data("ejDialog");
                        eDialog.open();
                    } else {
                        $scope.SaveData();
                    }

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });











        } catch (e) {
            ShowResult(e, 'failure');
        }

















        
    };

    $scope.SaveData = function () {
        try {
            if ($scope.OutPunchMissingDataForAlert.length > 0) {
                var eDialog = $("#dialogMessageAlert").data("ejDialog");
                eDialog.close();
            }
            if ($scope.tabh == 11) {
                $scope.SaveSeparatedLockDateList();
            }
            if ($scope.tabh == 22) {
                $scope.SaveMLVLockDateList();
            }


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.MLVEmpList = [];
    $scope.getMLVEmpList = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.customPara.mlvfromdate)) {
                throw "Select from data";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.mlvtodate)) {
                throw "Select to data";
            }

            if (new Date($scope.customPara.mlvfromdate) > new Date($scope.customPara.mlvtodate)) {
                throw "From date must be less than or equal to date";
            }
            $http({
                method: "GET",
                dataType: 'JSON',
                //data: { 'lockDate': $scope.customPara.lockDate},
                url: $scope.loadMLVEmpListUrl + '?FromDate=' + $filter('dateFiltering')($scope.customPara.mlvfromdate, 'dd-M-yyyy') + '&ToDate=' + $scope.customPara.mlvtodate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    $scope.MLVEmpList = [];
                    $scope.MLVEmpList = response.data.data;
                    for (var i = 0; i < $scope.SeparatedEmpList.length; i++) {
                        $scope.MLVEmpList[i].DOJ = new Date($scope.MLVEmpList[i].DOJ);
                        $scope.MLVEmpList[i].FromDate = new Date($scope.MLVEmpList[i].FromDate);
                    }
                
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
   







   
    $scope.SaveMLVLockDateList = function () {
        try {


           
            var LockDates = [];
            for (var i = 0; i < $scope.WorkDateList.length; i++) {
                if ($scope.WorkDateList[i].CheckBoxSelect == true) {
                    LockDates.push($scope.WorkDateList[i].WorkDate);
                }
            }

            if (LockDates.length == 0) {
                throw "Select data";
            }



            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateMLVEmployeeAttendanceLockUrl,
                data: { 'LockDates': LockDates, 'EmployeeSystemId': $scope.employees.SystemID }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
                    eDialog.close();

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };




    $scope.Print = function (gridObj) {
        //var gridObj = $("#DetailGrid").data("ejGrid");
        var data = gridObj.model.currentViewData;
        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            data: { 'data': data }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                // ShowResult(response.data.Message, 'failure', 'recipeMaterialPopUp');

            }
            else {

                location.href = $scope.downloadgriddataUrl + "?FileName=" + response.data.FileName;
            }
        });
    };
    //var today = new Date();
    //var today_formatted = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + ('0' + today.getDate()).slice(-2);
    //var user_busy_days = ['2019-06-09', '2019-06-16', '2019-06-19'];
    // An array of dates



    $scope.DownloadOutPunchMissingDataForAlert = function () {

        var gridObj = $("#GridOutPunchMissingDataForAlert").ejGrid("instance");
        $scope.Print(gridObj);

    };

}