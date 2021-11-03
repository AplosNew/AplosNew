'use strict';
issueGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function issueGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'issueGroup';
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.path = 'issueTracker/issueGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.IssueGroups = [];
    $scope.issueGroup = {

        Id: null,
        Name: null,
        ResponsiblePersonId: null,
        CreateDate: null
    };

    baseService.init('issueTracker/IssueGroup/GetIssueGroups');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.IssueGroups = result;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();


    $scope.onClickExcelPrint = function (args) {

        var data = args.data;
        var reportFormat = "Excel";

        try {
            window.open('issueTracker/IssueGroup/GetIssueGroupReport?reportFormat=' + reportFormat + '&issueGroupId=' + data.Id, '_blank');
            //location.href = 'Accounts/Advance/GetRequisitionReport?reportFormat=' + reportFormat + '&requisitionId=' + data.Id;
        } catch (e) {

        }
    };

    $scope.onClickPdfPrint = function (args) {

        var data = args.data;
        var reportFormat = "Pdf";
        try {
            window.open('issueTracker/IssueGroup/GetIssueGroupReport?reportFormat=' + reportFormat + '&issueGroupId=' + data.Id, '_blank');
            //location.href = 'Accounts/Advance/GetRequisitionReport?reportFormat=' + reportFormat + '&requisitionId=' + data.Id;

        } catch (e) {

        }
    };

}