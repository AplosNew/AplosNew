'use strict';
VASReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function VASReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Video Analysis Report";
    $scope.Action = 'Save';
    $scope.path = 'IE/VASReport/';
    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.report = {
        FromDate: new Date(),
        ToDate: new Date(),
    };
    $scope.FromDateTransform = function () {
        var d = new Date();
        d.setDate(d.getDate() - 3);
        $scope.report.FromDate = d;


        $scope.report.FromDate = $filter('dateFiltering')($scope.report.FromDate);
        $scope.report.ToDate = $filter('dateFiltering')($scope.report.ToDate);
    }
    $scope.FromDateTransform();

    $scope.dateRengeDataList = [];
    $scope.refreshTemplateOperation = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllOperation });
    };
    function CheckBoxSelectAllOperation(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridVASReport").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.dateRengeDataList.length; i++) {
                $scope.dateRengeDataList[i].Checked = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].Checked = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridVASReport").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.LoadSearchdData = function () {
        //$scope.$broadcast('show-errors-check-validity');
        try {
            //if ($scope.frmVASReport.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'GetSelectedDataRangeData',
                data: {
                    FromDate: $scope.report.FromDate,
                    ToDate: $scope.report.ToDate
                }
            }).then(function successCallback(response) {
                if (response.data.length > 0) {

                    for (var i = 0; i < response.data.length; i++) {
                        if (!baseService.isUndefinedOrNull(response.data[i].AddedDate))
                            response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                        if (!baseService.isUndefinedOrNull(response.data[i].ApprovedDate))
                            response.data[i].ApprovedDate = new Date(response.data[i].ApprovedDate);
                    }

                    $scope.dateRengeDataList = response.data;
                }
                else {
                    $scope.dateRengeDataList = [];
                    ShowResult("No Data Found..!", "failure");
                }
            });
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadSearchdData();
    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedOperationTimeDetails?VASMasterID=' + filteredData
        }).then(function successCallback(response) {
            $scope.lst = response.data;
            $scope.data = $scope.lst;

            e.detailsElement.find("#detailGrid").ejGrid({
                dataSource: $scope.lst,
                allowPaging: true,
                allowReordering: true,
                allowSorting: true,
                allowResizing: true,
                allowTextWrap: true,
                columns: [{ field: "Sequence", headerText: "Element", width: 1, textAlign: 'center' },
                { field: "ElementType", headerText: "Element Type", width: 1, textAlign: 'center' },
                { field: "ElementCode", headerText: "Element Code", width: 1, textAlign: 'center' },
                { field: "TMU", headerText: "TMU", width: 1, textAlign: 'center' },
                { field: "CT1", headerText: "CT1 Sec", width: 1, textAlign: 'center' },
                { field: "CT2", headerText: "CT2 Sec", width: 1, textAlign: 'center' },
                { field: "CT3", headerText: "CT3 Sec", width: 1, textAlign: 'center' },
                { field: "CT4", headerText: "CT4 Sec", width: 1, textAlign: 'center' },
                { field: "CT5", headerText: "CT5 Sec", width: 1, textAlign: 'center' },
                { field: "TimeAvg", headerText: "TimeAvg", width: 1, textAlign: 'center' },
                { field: "Ratings", headerText: "Ratings %", width: 1, textAlign: 'center' },
                { field: "BasicTime", headerText: "Basic Time", width: 1, textAlign: 'center' }]
            });
        });
    };

    $scope.onClickSingleVASReport = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.vasData = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        window.open($scope.path + 'GetVASReport?reportFormat=' + reportFormat + '&&ReportData=' + $scope.vasData.Id, '_blank');
    };
    $scope.onClickVASReport = function () {
        var rptArray = [];
        var reportFormat = "Excel";
        var gridObj = $("#GridVASReport").ejGrid("instance");


        //var filteredRecords = gridObj.getFilteredRecords();
        //if (angular.isUndefinedOrNull(filteredRecords) == false) {
        //    if (filteredRecords.length > 0) {


        var Griddata = gridObj.getFilteredRecords();
        if (Griddata.length == 0)
            Griddata = $scope.dateRengeDataList;

        $.each(Griddata, function (key, value) {
            if (value.Checked === true)
                rptArray.push(value.Id);
        });
        var _reportID = rptArray.toString();

        if (_reportID !== "") {
            try {


                $http({
                    method: 'POST',
                    url: $scope.path + 'GetVASReport',
                    data: { 'ReportData': _reportID }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                });

            } catch (e) {

            }
        }
        else
            ShowResult("No option has been selected..!", "failure");
    };
    $scope.Operation_Video_Version_Name = "";
    $scope.loadOperationVideo = function (data) {
        $('#vdid').empty();
        $("#vdid").removeAttr("src");
        $('#vdid').html("");

        var videoElement = document.getElementById('vdid');
        videoElement.pause();
        videoElement.removeAttribute('src');
        videoElement.load();

        $scope.Operation_Video_Version_Name = "";

        $http({
            method: 'POST',
            url: 'IE/VASApproval/GetOperationVideoName?OperationVariationSystemId=' + data.OperationVariationSystemId + "&&Version=" + data.Version
        }).then(function successCallback(response) {
            $scope.Operation_Video_Version_Name = "Operation Code: " + data.OperationVariationSystemId + "      Version: " + data.Version + " Video Name:" + response.data[0].OriginalVideoName
            $('#vdid').html("<source id='vdids' src='POPResources/vas/" + response.data[0].VASVideoName + "' type='video/mp4'>");
        });

        angular.element(document.querySelector("#modalPlayVideo")).modal("toggle");
    };

}