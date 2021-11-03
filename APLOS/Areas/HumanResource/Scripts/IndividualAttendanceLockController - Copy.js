'use strict';
IndividualAttendanceLockController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function IndividualAttendanceLockController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Individual Attendance Lock';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/HrmsSettings/';
    $scope.UnLockEmpDataListUrl = $scope.path + 'LoadEmployeeIndividualAttendanceUnLock';
    $scope.LockEmpDataListUrl = $scope.path + 'LoadEmployeeIndividualAttendanceLock';


    $scope.CreateLockUrl = $scope.path + 'CreateEmployeeIndividualAttendanceLock';
    $scope.CreateUnLockUrl = $scope.path + 'CreateEmployeeIndividualAttendanceUnLock';


    $scope.UnLockMLVEmpDataListUrl = $scope.path + 'LoadEmployeeMLVAttendanceUnLock';
    $scope.LockMLVEmpDataListUrl = $scope.path + 'LoadEmployeeMLVAttendanceLock';


    $scope.CreateMLVLockUrl = $scope.path + 'CreateEmployeeMLVAttendanceLock';
    $scope.CreateMLVUnLockUrl = $scope.path + 'CreateEmployeeMLVAttendanceUnLock';


    $scope.customPara = {
        lockDate: null,
        UnlockDate: null,
        MLVlockDate: null,
        MLVUnlockDate: null
    };


    //#region Tab




    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];
        $scope.getUnLockDateList();

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;
        $scope.getLockDateList();

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    $scope.setTab33 = function (newTab) {
        $scope.tabh = newTab; 
        $scope.getMLVUnLockDateList();
    };
    $scope.isSet33 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab44 = function (newTab) {
        $scope.tabh = newTab;
    };
    $scope.isSet44 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    // #endregion Tab

    $scope.UnLockEmployeeData = [];
    $scope.LockEmployeeData = [];

 

    $scope.getUnLockDateList = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                $http({
                    method: "GET",
                    dataType: 'JSON',
                    //data: { 'lockDate': $scope.customPara.lockDate},
                    url: $scope.UnLockEmpDataListUrl + '?lockDate=' + $filter('dateFiltering')($scope.customPara.lockDate, 'dd-M-yyyy')

                }).then(function successCallback(response) {

                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {

                        $scope.UnLockEmployeeData = [];
                        $scope.UnLockEmployeeData = response.data.data;
                        for (var i = 0; i < $scope.UnLockEmployeeData.length; i++) {
                            $scope.UnLockEmployeeData[i].DOJ = new Date($scope.UnLockEmployeeData[i].DOJ);
                            $scope.UnLockEmployeeData[i].DOS = new Date($scope.UnLockEmployeeData[i].DOS);
                        }

                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }



        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getLockDateList = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.customPara.UnlockDate)) {
                $http({
                    method: "GET",
                    dataType: 'JSON',
                    //data: { 'lockDate': $scope.customPara.lockDate},
                    url: $scope.LockEmpDataListUrl + '?lockDate=' + $filter('dateFiltering')($scope.customPara.UnlockDate, 'dd-M-yyyy')

                }).then(function successCallback(response) {

                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {

                        $scope.LockEmployeeData = [];
                        $scope.LockEmployeeData = response.data.data;
                        for (var i = 0; i < $scope.LockEmployeeData.length; i++) {
                            $scope.LockEmployeeData[i].DOJ = new Date($scope.LockEmployeeData[i].DOJ);
                            $scope.LockEmployeeData[i].DOS = new Date($scope.LockEmployeeData[i].DOS);
                        }

                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }



        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteSelected1();
        $scope.MLVactionCompleteSelected();
        $scope.MLVactionCompleteSelected1();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridUnLockEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridUnLockEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLockEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridLockEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.SaveUnLockDateList = function () {
        try {


            var EmployeeSystemIds = [];
            for (var i = 0; i < $scope.UnLockEmployeeData.length; i++) {
                if ($scope.UnLockEmployeeData[i].CheckBoxSelect == true) {
                    EmployeeSystemIds.push($scope.UnLockEmployeeData[i].SystemID);
                }
            }
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Enter Date";
            }
            if (EmployeeSystemIds.length == 0) {
                throw "Select data";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateLockUrl,
                data: { 'LockDate': $filter('dateFiltering')($scope.customPara.lockDate, 'dd-M-yyyy'), 'EmployeeSystemIds': EmployeeSystemIds }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    ShowResult(response.status.Message, 'success');
                    $scope.getUnLockDateList();
                    //$scope.getLockDateList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SaveLockDateList = function () {
        try {


            var EmployeeSystemIds = [];
            for (var i = 0; i < $scope.LockEmployeeData.length; i++) {
                if ($scope.LockEmployeeData[i].CheckBoxSelect == true) {
                    EmployeeSystemIds.push($scope.LockEmployeeData[i].SystemID);
                }
            }
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Enter Date";
            }
            if (EmployeeSystemIds.length == 0) {
                throw "Select data";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateUnLockUrl,
                data: { 'LockDate': $filter('dateFiltering')($scope.customPara.UnlockDate, 'dd-M-yyyy'), 'EmployeeSystemIds': EmployeeSystemIds }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    ShowResult(response.status.Message, 'success');
                    //$scope.getUnLockDateList();
                    $scope.getLockDateList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    ///==================================================================MLV=======================================================================
    $scope.MLVactionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridUnLockMLVEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridUnLockMLVEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.MLVactionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridMLVLockEmployeeData").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridMLVLockEmployeeData").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };


    $scope.MLVUnLockEmployeeData = [];
    $scope.MLVLockEmployeeData = [];



    $scope.getMLVUnLockDateList = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.customPara.MLVlockDate)) {
                $http({
                    method: "GET",
                    dataType: 'JSON',
                    //data: { 'lockDate': $scope.customPara.lockDate},
                    url: $scope.UnLockEmpDataListUrl + '?lockDate=' + $filter('dateFiltering')($scope.customPara.lockDate, 'dd-M-yyyy')

                }).then(function successCallback(response) {

                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {

                        $scope.MLVUnLockEmployeeData = [];
                        $scope.MLVUnLockEmployeeData = response.data.data;
                        for (var i = 0; i < $scope.MLVUnLockEmployeeData.length; i++) {
                            $scope.MLVUnLockEmployeeData[i].DOJ = new Date($scope.MLVUnLockEmployeeData[i].DOJ);
                            $scope.MLVUnLockEmployeeData[i].DOS = new Date($scope.MLVUnLockEmployeeData[i].DOS);
                        }

                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }



        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getMLVLockDateList = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.customPara.UnlockDate)) {
                $http({
                    method: "GET",
                    dataType: 'JSON',
                    //data: { 'lockDate': $scope.customPara.lockDate},
                    url: $scope.LockEmpDataListUrl + '?lockDate=' + $filter('dateFiltering')($scope.customPara.UnlockDate, 'dd-M-yyyy')

                }).then(function successCallback(response) {

                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {

                        $scope.LockEmployeeData = [];
                        $scope.LockEmployeeData = response.data.data;
                        for (var i = 0; i < $scope.LockEmployeeData.length; i++) {
                            $scope.LockEmployeeData[i].DOJ = new Date($scope.LockEmployeeData[i].DOJ);
                            $scope.LockEmployeeData[i].DOS = new Date($scope.LockEmployeeData[i].DOS);
                        }

                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
            }



        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
  
 
    $scope.SaveMLVUnLockDateList = function () {
        try {


            var EmployeeSystemIds = [];
            for (var i = 0; i < $scope.UnLockEmployeeData.length; i++) {
                if ($scope.UnLockEmployeeData[i].CheckBoxSelect == true) {
                    EmployeeSystemIds.push($scope.UnLockEmployeeData[i].SystemID);
                }
            }
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Enter Date";
            }
            if (EmployeeSystemIds.length == 0) {
                throw "Select data";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateLockUrl,
                data: { 'LockDate': $filter('dateFiltering')($scope.customPara.lockDate, 'dd-M-yyyy'), 'EmployeeSystemIds': EmployeeSystemIds }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    ShowResult(response.status.Message, 'success');
                    $scope.getUnLockDateList();
                    //$scope.getLockDateList();
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


            var EmployeeSystemIds = [];
            for (var i = 0; i < $scope.LockEmployeeData.length; i++) {
                if ($scope.LockEmployeeData[i].CheckBoxSelect == true) {
                    EmployeeSystemIds.push($scope.LockEmployeeData[i].SystemID);
                }
            }
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Enter Date";
            }
            if (EmployeeSystemIds.length == 0) {
                throw "Select data";
            }
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.CreateUnLockUrl,
                data: { 'LockDate': $filter('dateFiltering')($scope.customPara.UnlockDate, 'dd-M-yyyy'), 'EmployeeSystemIds': EmployeeSystemIds }


            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {

                    ShowResult(response.status.Message, 'success');
                    //$scope.getUnLockDateList();
                    $scope.getLockDateList();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}