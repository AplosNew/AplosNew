'use strict';
issueReportController.$inject = ['cboService', 'commonMessage', '$window', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'fileReader'];
function issueReportController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, fileReader) {
    
    $rootScope.title = 'Issue Report';
    $scope.Action = 'Save';

    $scope.WithSubCategory = {
        CheckBox: false
    };

    $scope.path = 'issueTracker/IssueTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'IssueTransactionCreate';
 
    $scope.GetIssueReportExcel = function () {
        var url = 'IssueTracker/IssueTransaction/GetIssueReportExcel?checkbox=' + $scope.WithSubCategory.CheckBox;
        $window.open(url, '_blank');
    };
}