'use strict';
QualityActionUpdateReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function QualityActionUpdateReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "QualityActionUpdateReport";
    $scope.Action = 'Save';
    $scope.path = 'Productions/QualityActionUpdateReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
  
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() - 3);
  
    $scope.status = {
        Id: null,
        FromDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        ActResponsiblePerson: $window.employeeName,
        ActResponsiblePersonId: $window.employeeId,
        ActionApplicable: false
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.selectActResponsiblePerson = function () {
        $scope.getActResponsiblePerson();
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('show');
    }

    $scope.ActResponsiblePersonList = [];
    $scope.getActResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ActResponsiblePersonList = resp.data;
        });
    }

    $scope.doubleActResponsiblePerson = function (e) {
        $scope.statusNew.ActResponsiblePersonId = e.data.SystemId;
        $scope.statusNew.ActResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeActResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ActResponsiblePersonPopup')).modal('hide');
    }

    $scope.QualityActionUpdateReportList = [];
    $scope.View = function () {
        try {
            $scope.QCCompleteList = [];
            $http.get('Productions/QualityActionUpdateReport/LoadQualityActionUpdateReport?FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&ResponsiblePersonId=' + $scope.statusNew.ActResponsiblePersonId + '&ActionApplicable=' + $scope.statusNew.ActionApplicable)
                .then(function (response) {
                    $scope.QualityActionUpdateReportList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.status = {
            Id: null,
            FromDate: $filter('dateFiltering')(date, 'dd-MM-yyyy'),
            ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
            ActResponsiblePerson: null,
            ActResponsiblePerson: null,
            ActionApplicable: false
        };
        $scope.statusNew = Object.assign({}, $scope.status);
    }

    $scope.QAUReport = function () {
        var dataList = [];
        var g = $("#GridQualityActionUpdateReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.QualityActionUpdateReportList;
        }

        $scope.fileName = "Quality Action Update Report";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

}

