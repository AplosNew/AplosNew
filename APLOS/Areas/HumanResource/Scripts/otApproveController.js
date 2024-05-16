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

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.otApproveList.length; i++) {
                $scope.otApproveList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
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

    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.ModelNew = { FileName: null };
    $scope.ImportData = function () {
        try {
            $scope.otApproveList = [];
            $scope.msg = "";

            var picData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.picdata)) {
                $scope.ModelNew.FileName = $scope.picdata.name;
            } else {
                throw "Please select File.";
            }


            $http({
                method: 'POST',
                url: 'HumanResource/OTConfirmationProcess/ImportOTData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    picData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                        picData.append('file', data.file);
                    }
                    return picData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    $scope.otApproveList = response.data;
                    for (var i = 0; i < $scope.otApproveList.length; i++) {
                        $scope.otApproveList[i].CheckBoxSelect = true;
                    }
                   
                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            try {
                

                var dataList = [];
                var tosavedataList = [];
                var g = $("#GridEmployeeInfoList").data("ejGrid");
                dataList = g.getFilteredRecords();

                if (dataList.length == 0) {
                    dataList = $scope.otApproveList;
                }


                for (var i = 0; i < dataList.length; i++) {
                    if (dataList[i].OTHr > dataList[i].CalculatedOT) {
                        throw "Extra OT will not exceed OverStay for this Employee " + dataList[i].EmployeeCode+".";
                    }
                    if (dataList[i].CheckBoxSelect == true) {
                        tosavedataList.push(dataList[i]);
                    }
                }
                if (tosavedataList.length == 0) {
                    throw 'Select Employee.';
                }

                $http({
                    method: 'POST',
                    data: { data: $scope.OTManual, SaveMultipleEmpOTExcel: tosavedataList },
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