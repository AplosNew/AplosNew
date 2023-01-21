DailyAttendanceStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function DailyAttendanceStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Daily Attendance Status';
    $scope.path = 'humanresource/DailyAttendanceStatusReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.InStatusList = [];
    $scope.LateStatusList = [];
    $scope.EmpCategoryList = [];
    $scope.ResponsiblePersonList = [];
    $scope.TeamLeaderList = [];
    $scope.FavList = [];
    $scope.EmpStatusList = [];
    $scope.ShiftList = [];

    // #region    EmployeePop
    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.GetResponsiblePerson();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }
    $scope.EmployeeList = [];
    $scope.GetResponsiblePerson = function () {
        $http({
            method: 'GET',
            url: 'Productions/Parameter/GetResponsiblePerson',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }

    $scope.EmployeeId = null;
    $scope.Employee = null;
    $scope.doubleEmploye = function (e) {
        $scope.ModelNew.EmpSystemId = e.data.EmpSystemId;
        $scope.ModelNew.EmployeeName = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        /*$scope.viewFurniturePolicyGrids();*/
    }



    $scope.getResponsiblePersonId = function () {
        $http({
            method: 'POST',
            data: { 'ResponsiblePersonId': $scope.EmployeeId, },
            url: $scope.path + 'getResponsiblePersonId',
        }).then(function success(response) {
            $scope.ResponsiblePerson = JSON.stringify(response.data[0].EmployeeName.replace(/\"/g, ""));
            $scope.ResponsiblePerson = $scope.ResponsiblePerson.replace(/\"/g, "");

        });
    }
        // #endregion    EmployeePop

    $scope.GetShift = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }
    $scope.GetShift();

    $scope.summaryfileName = "Daily Attendance Status.xlsx"
    $scope.XlsSalaryUnDisburseReport = function () {
       
            var parameters = {
                'effectiveDate': $scope.effectiveDate, 'salaryProcessId': $scope.salaryProcessId, 'payRollGroup': $scope.payGroupListSelected, 'isActive': $scope.isActive,
                'isSeperated': $scope.isSeperated,
                'isMaternity': $scope.isMaternity
            };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: $scope.path + 'GetDailyAttendanceStatusXls',
                //data: parameters
            })
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

        };
   

}