'use strict';
CustomerRequirementControlController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function CustomerRequirementControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "CustomerRequirementControl";
    $scope.Action = 'Save';
    $scope.path = 'QMS/CustomerRequirementControl/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.ParameterStatusLists = [];
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() - 3);

    $scope.ParameterStatusLists = [
        {
            'Value': 'Pending',
            'Text': 'Pending'
        },
        {
            'Value': 'Completed',
            'Text': 'Completed'
        }
    ];

    $scope.status = {
        Id: null,
        ParameterStatus: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.CustomerRequirementControlList = [];
    $scope.View = function () {
        try {
            $scope.QCCompleteList = [];
            $http.get('QMS/CustomerRequirementControl/LoadCustomerRequirementControl?ParameterStatus=' + $scope.statusNew.ParameterStatus)
                .then(function (response) {
                    $scope.CustomerRequirementControlList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}

