'use strict';
ProductionSystemAllowanceController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function ProductionSystemAllowanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Production System Allowance";
    $scope.Action = 'Save';
    $scope.path = 'IE/ProductionSystemAllowance/';

    $scope.VAS = {
        BundleHandleTimeId: 0,
        Factor: null,
        FactorValue: null
    };

    $scope.productionSystemAllowanceList = [];

    $scope.AddNew = function () {
        $scope.Clear();
        $scope.Action = 'Save';
        $scope.VAS.BundleHandleTimeId = 0;
        angular.element(document.querySelector("#modalProductionSystemAllowanceList")).modal("toggle");
    };

    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'

        }).then(function successCallback(response) {
            $scope.productionSystemAllowanceList = response.data;
        });
    };

    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.BundleHandleTimeId;
        $scope.PopulateSelectedDate($scope.RowId);
    };

    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.BundleHandleTimeId = $scope.selecteddata.BundleHandleTimeId;
        $scope.PopulateSelectedDate($scope.VAS.BundleHandleTimeId);
    };
    $scope.PopulateSelectedDate = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'BundleHandleTimeId': Id
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.VAS.BundleHandleTimeId = response.data[0].BundleHandleTimeId;
                $scope.VAS.Factor = response.data[0].Factor;
                $scope.VAS.FactorValue = response.data[0].FactorValue;

                $scope.Action = 'Update';
                angular.element(document.querySelector("#modalProductionSystemAllowanceList")).modal("toggle");
            }
            else {
                alert('No Data Found..!');
            }
        });
    };
    $scope.SaveData = function () {
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.productionSystemAllowanceForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'elementType': $scope.VAS },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.getAllData();
                        ShowResult(response.data.Message, 'success');
                        angular.element(document.querySelector("#modalProductionSystemAllowanceList")).modal("toggle");
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.BundleHandleTimeId = $scope.selecteddata.BundleHandleTimeId;

        $scope.message = 'Are you sure want to Remove?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?BundleHandleTimeId=" + $scope.VAS.BundleHandleTimeId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.getAllData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getAllData();

    $scope.Clear = function () {
        $scope.VAS = {};
    };
}