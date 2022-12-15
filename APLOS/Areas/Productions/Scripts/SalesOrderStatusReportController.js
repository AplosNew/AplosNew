'use strict';
SalesOrderStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function SalesOrderStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Sales Order Status Report';
    $scope.path = 'Productions/SalesOrderStatusReport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    //$scope.Dates = new Date();
   // $scope.EntityList = null;

    //$scope.parameters = [];
    //$scope.filters = [];
    //$scope.loadfilters = function () {
    //    $http({
    //        method: 'POST',
    //        url:"Productions/ProductiveAllowanceRateSetup/getEntity",
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.EntityList = response.data;
    //    });
    //}
    //$scope.loadfilters();

    $scope.OrderStatusId = null;
    $scope.orderStatusList = [];
    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });


    //var getString = function (data, column) {
    //    var string = "''";
    //    var collection = [];

    //    for (var i = 0; i < data.length; i++) {
    //        if (collection.includes(data[i][column]) == false) {
    //            string += ",'" + data[i][column] + "'";
    //            collection.push(data[i][column]);
    //        }
    //    }
    //    return string;
    //}

    $scope.GetReport = function (reportType) {
        try {


            var DropDownJobLocationListObjE = $("#selOS").data("ejDropDownList");
            var osLists = DropDownJobLocationListObjE.getSelectedValue();
            if (angular.isUndefinedOrNull(osLists)) {
                throw "Select Order Status.";
            }


            // The Report Code
            $http({
                method: 'POST',
                url: $scope.path + '/XlsSalesOrderStatusReport',
                data: { 'orderStatusId': osLists },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}