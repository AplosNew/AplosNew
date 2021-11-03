'use strict';
IndividualAttendanceUnLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function IndividualAttendanceUnLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Individual Attendance Un-Lock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';

    $scope.loadSepratedEmpListUrl = $scope.path + 'LoadSeparatedEmployeeList';
    $scope.loadMLVEmpListUrl = $scope.path + 'LoadMLVEmployeeList';
    $scope.LoadWorkDateSeparatedEmployeeUrl = $scope.path + 'LoadWorkDateForUnLock';
    $scope.CreateSCreateIndividualUnLockUrl = $scope.path + 'CreateIndividualUnLock';


    
  



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
                var gridObj = $("#GridSeparatedEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridSeparatedEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
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
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400} });
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
                    //for (var i = 0; i < $scope.MLVUnLockEmployeeData.length; i++) {
                    //    $scope.MLVUnLockEmployeeData[i].DOJ = new Date($scope.MLVUnLockEmployeeData[i].DOJ);
                    //    $scope.MLVUnLockEmployeeData[i].DOS = new Date($scope.MLVUnLockEmployeeData[i].DOS);
                    //}

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
                var gridObj = $("#GridSeparatedEmployeeList").data("ejGrid");
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

            var FromDate = $filter('dateFiltering')($scope.customPara.mlvfromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.mlvtodate, 'dd-M-yyyy');
            if (new Date($scope.customPara.mlvfromdate) > new Date($scope.customPara.mlvtodate)) {
                throw "From date must be less than or equal to date";
            }

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
         
            var FromDate = $filter('dateFiltering')($scope.customPara.fromdateWD, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todateWD, 'dd-M-yyyy');
            if (new Date(FromDate) > new Date(ToDate)) {
                throw "From date must be less than or equal to date";
            }
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
        
    };


    $scope.SaveData = function () {
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
                url: $scope.CreateSCreateIndividualUnLockUrl,
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
                    //for (var i = 0; i < $scope.MLVUnLockEmployeeData.length; i++) {
                    //    $scope.MLVUnLockEmployeeData[i].DOJ = new Date($scope.MLVUnLockEmployeeData[i].DOJ);
                    //    $scope.MLVUnLockEmployeeData[i].DOS = new Date($scope.MLVUnLockEmployeeData[i].DOS);
                    //}

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };









    
}