'use strict';
EmployeeSkillMatrixController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeSkillMatrixController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Skill Matrix';
    $scope.path = 'HumanResource/EmployeeSkillMatrix/';
    $scope.downloadgriddataUrlPath = 'Banks/CheckManagement/DownloadUsingFullPath';
    baseService.init($scope.getListUrl);

    $scope.EmployeeSkillMatrixList = [];
    $scope.getData = function () {
        $scope.EmployeeSkillMatrixList = [];
        $http.get('HumanResource/EmployeeSkillMatrix/getEmployeeSkillMatrixList') 
            /*?PostingDate=' + $filter("dateFiltering")(Date.now()))*/
            .then(function (response) {
                $scope.EmployeeSkillMatrixList = response.data;
            });
    };
    $scope.getData();

    $scope.ReportEmployeeSkillMatrixEmployeeWise = function () {
        try {
            $scope.fileName = "Employee Skill Matrix Employee Wise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetEmployeeSkillMatrixEmployeeWise",
                //data: { 'LineId': $scope.LineId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }



    $scope.ReportEmployeeSkillMatrixLineWise = function () {
        try {
            $scope.fileName = "Employee Skill Matrix Line Wise.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetEmployeeSkillMatrixLineWise",
                //data: { 'LineId': $scope.LineId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}