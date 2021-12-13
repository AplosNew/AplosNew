'use strict';
GratuityReportCompanyController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function GratuityReportCompanyController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {

    $rootScope.title = 'Company Gratuity Report';
    $controller('employeeBaseController', { $scope: $scope, $http: $http });
    $scope.calculationDate = $filter('dateFiltering')(Date.now());

    $scope.payrollGroupId = null;
    $scope.employeeSystemId = null;
    $scope.reportType = null;

    $scope.payrollGroupList = [];
    
    cboService.getPayGroupCbo(function (result) {
        $scope.payrollGroupList = result;
    });

    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.getGratuityReport = function (reportType) {
        try {
            var DropDownListObj = $("#CWPlant").data("ejDropDownList");
            var PlantId = DropDownListObj.getSelectedValue();
            $http({
                method: 'POST',
                url: 'Payrolls/GratuityReportCompany/XlsEmployeeGratuity',
                data: {
                    'calculationDate': $scope.calculationDate,
                    'payrollGroup': $scope.payrollGroupId,
                    'employeeSystemId': null,
                    'reportType': reportType, 'PlantId': PlantId
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (reportType == "EXCEL") {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                    if (reportType == "PDF") {
                        $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                    }
                }
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.PlantIdFromUI = null;
    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: "humanresource/payrollReports/GetPlantList",
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            var index = 0;
            for (var i = 0; i < $scope.PlantList.length; i++) {
                if ($scope.PlantList[i].PlantId == $window.plantId) {
                    index = i;
                }
            }

            $('#CWPlant').ejDropDownList(
                {
                    dataSource: $scope.PlantList,
                    fields: { text: "PlantName", value: "PlantId" },
                    selectedIndex: index, showCheckBox: true, multiSelectMode: ej.MultiSelectMode.VisualMode
                    , width: 250
                });

        });
    }
    $scope.getPlant();

}