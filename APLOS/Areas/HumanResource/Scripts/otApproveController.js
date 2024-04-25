'use strict';
otApproveController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function otApproveController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $scope.title = "OT Approve";


    $scope.ModelTemp = {
        Id: null,
        EmpSystemId: null,
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        WorkDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        InTime: null,
        OutTime: null,
        OThour: null,
        EmpName: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        Remarks: null,
        IsConfirmed: false,
        APDEmpWorkDate: null,
        PlantId: $window.plantId,
    };
    $scope.OTManual = Object.assign({}, $scope.ModelTemp);

    $scope.otApproveList = [];
    $scope.GetWorkOverStayData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.OTManual.WorkDate)) {
                throw "Select Work Date.";
            }
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'HumanResource/OTConfirmationProcess/GetWorkOverStayData?workDate=' + $scope.OTManual.WorkDate
            }).then(function successCallback(response) {
                $scope.otApproveList = response.data;

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.DownloadOTData = function () {
        try {
            var dataList = [];
            var g = $("#GridEmployeeInfoList").data("ejGrid");
            dataList = g.getFilteredRecords();

            if (dataList.length == 0) {
                dataList = $scope.otApproveList;
            }
           
            $scope.fileName = "OTDataReport.xlsx";

            $http({
                method: 'POST',
                url: "HumanResource/OTConfirmationProcess/GetOTDataXls",
                data: {'data': dataList, 'reportFileName': $scope.fileName },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }



    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            try {
                if ($scope.otApproveList.length == 0) {
                    throw 'Enter atleast one Employee OT';
                }

                var dataList = [];
                var g = $("#GridEmployeeInfoList").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.otApproveList;
                }



                $http({
                    method: 'POST',
                    data: { data: $scope.OTManual, SaveMultipleEmpOTExcel: dataList },
                    url: 'HumanResource/OTConfirmationProcess/SaveOTData'

                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        
                        ClearFields();
                    }
                });
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        }
    }


    function ClearFields() {
        $scope.OTManual = Object.assign({}, $scope.ModelTemp);
        $scope.otApproveList = [];
    }
}