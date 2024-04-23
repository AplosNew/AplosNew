'use strict';
otApproveController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller','$window'];
function otApproveController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
     

    $scope.ClanderYearModel = {
        Id: null,
        YearNo: null
    };
    $scope.Id = null;
    $scope.ClanderYear = [];
    $scope.GetClanderYear = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'HumanResource/AttendanceManagement/GetClanderYear'

        }).then(function successCallback(response) {
            $scope.ClanderYear = response.data.data;

        });
    };
    $scope.GetClanderYear();


    $scope.GetOtFinalReport = function () {
        var ReportFormat = 'Excel';
        location.href = 'HumanResource/AttendanceManagement/GetOtFinalReport?reportFormat=' + ReportFormat + '&year=' + $scope.Year + '&month=' + $scope.Month;
    };


}