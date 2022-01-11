'use strict';
LeaveYearEndProcessNewController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function LeaveYearEndProcessNewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Leave Year End Process New';
    $scope.path = 'Attendances/LeaveYearEndProcess/';
    $scope.getYearlyCalendarUrl = $scope.path + 'LoadYearlyCalendar';
    $scope.LeaveYearEndProcessSummaryDataUrl = $scope.path + 'GetLeaveYearEndProcessSummaryData';
    $scope.LeaveYearEndProcessSummaryDataIndividualUrl = $scope.path + 'GetLeaveYearEndProcessSummaryDataIndividual';
    $scope.LeaveYearEndProcessUrl = $scope.path + 'LeaveYearEndProcessNew';
    $scope.LeaveYearEndProcessIndividualUrl = $scope.path + 'LeaveYearEndProcessIndividualNew';
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    }

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.Date = ($filter('dateFiltering')(new Date(), 'dd-MM-yyyy'));

    $scope.ShowProcButton = false;
    $scope.ShowProcButtonIndividual = false;
    $scope.YearlyCalendarId = null;
    $scope.LeaveYearEndProcessSummary = [];
    $scope.LeaveYearEndProcessSummaryIndividual = [];
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
    $scope.LeaveYearEndProcessIndividual = function () {
        try {


            $http.get($scope.LeaveYearEndProcessIndividualUrl + '?ToDate=' + $scope.Date)
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
    
    $scope.GetLeaveYearEndProcessSummaryDataIndividual = function () {
        try {
            $http.get($scope.LeaveYearEndProcessSummaryDataIndividualUrl + '?ToDate=' + $scope.Date)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveYearEndProcessSummaryIndividual = response.data;
                        $scope.ShowProcButtonIndividual = true;
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
 
   
}