'use strict';
queryEditorController.$inject = ['$scope', '$rootScope', 'baseService', '$http', 'exportToExcel', '$timeout'];
function queryEditorController($scope, $rootScope, baseService, $http, exportToExcel, $timeout) {
    $rootScope.title = 'Query Editor';
    $scope.index = -1;
    $scope.queryEditors = [];
    $scope.path = 'query/';
    $scope.errorText = null;
    $scope.query = null;
    
    $scope.run = function () {
        $scope.resultList = [];
        $scope.headerList = [];
        $scope.errorText = null;
        $http({
            method: 'POST',
            url: $scope.path + 'getqueryresult',
            data: { sql: $scope.query },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                $scope.errorText = response.data.Message;
            else {
                $scope.resultList = response.data;
                if (baseService.arrayLength($scope.headerList) == 0)
                    getDDLSearchColumn(response.data, $scope.headerList);
            }
        }, function errorCallback(response) {
            $scope.errorText = response.status.Message;
        })
    }

    $scope.clear = function () {
        $scope.query = null;
        $scope.resultList = [];
        $scope.headerList = [];
        $scope.errorText = null;
    }

    function getDDLSearchColumn(processData, list) {
        if (processData !== null) {
            var obsx = {
                Text: null,
                Value: null,
                IsVisible: null
            };
            var ob = processData[0];
            for (var i in ob) {
                obsx.Text = i;
                obsx.Value = i;
                list.push(obsx);
                obsx = {
                    Text: null,
                    Value: null,
                    IsVisible: null
                };
            }
        }
    }

    $scope.exportToExcel = function (tableId) { // ex: '#my-table'
        var exportHref = exportToExcel.tableToExcel('#' + tableId, 'DataExport');
        $timeout(function () { location.href = exportHref; }, 100); // trigger download
    }
}