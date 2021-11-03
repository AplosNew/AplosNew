'use strict';
LeaveYearEndProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function LeaveYearEndProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Leave Year End Process';
    $scope.path = 'Attendances/LeaveYearEndProcess/';
    $scope.getYearlyCalendarUrl = $scope.path + 'LoadYearlyCalendar';
    $scope.LeaveYearEndProcessSummaryDataUrl = $scope.path + 'GetLeaveYearEndProcessSummaryData';
    $scope.LeaveYearEndProcessUrl = $scope.path + 'LeaveYearEndProcess';

    $scope.ShowProcButton = false;
    $scope.YearlyCalendarId = null;
    $scope.LeaveYearEndProcessSummary = [];
    $scope.LoadYearlyCalendarList = function () {
        try {


            $http.get($scope.getYearlyCalendarUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.YearlyCalendar = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadYearlyCalendarList();

    $scope.LeaveYearEndProcess = function () {
        try {


            $http.get($scope.LeaveYearEndProcessUrl + '?YearId=' + $scope.YearlyCalendarId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveYearEndProcessSummary = [];
                        $scope.ShowProcButton = false;
                        ShowResult(response.data.Message, 'success');
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetLeaveYearEndProcessSummaryData = function () {
        try {


            $http.get($scope.LeaveYearEndProcessSummaryDataUrl + '?sYearId=' + $scope.YearlyCalendarId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveYearEndProcessSummary = response.data;
                        $scope.ShowProcButton = true;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
 
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();    

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLeaveYearEndProcessSummary").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridLeaveYearEndProcessSummary").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    
   








   
}