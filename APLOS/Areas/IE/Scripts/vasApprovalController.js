'use strict';
VASApprovalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function VASApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "VAS Approval";
    $scope.Operation_Version_Name = "";
    $scope.Operation_Video_Version_Name = "";
    $scope.path = 'IE/VASApproval/';
    $scope.tab = 1;
    $scope.report = {
        FromDate: $filter('dateFiltering')(new Date()),
        ToDate: $filter('dateFiltering')(new Date())
    };
   
    $scope.FromDateTransform = function () {
        var d = new Date();
        d.setDate(d.getDate() - 3);
        $scope.report.FromDate = d;

        $scope.report.FromDate = $filter('dateFiltering')($scope.report.FromDate);
        $scope.report.ToDate = $filter('dateFiltering')($scope.report.ToDate);

        $scope.reportApproved = Object.assign({}, $scope.report);
    }
    $scope.FromDateTransform();
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.newVASList = [];
    $scope.newVASVersionList = [];
    $scope.approvedVASList = [];

    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {
        try {
            if (!e.data.Acknowledged)
                e.row.css("background-color", '#00ffAA');
        } catch (e) {

        }
    }
    //$scope.getNewVASList = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetOperationList'
    //    }).then(function successCallback(response) {
    //        for (var i = 0; i < response.data.length; i++) {
    //            if (!baseService.isUndefinedOrNull(response.data[i].AddedDate))
    //                response.data[i].AddedDate = new Date(response.data[i].AddedDate);
    //            if (!baseService.isUndefinedOrNull(response.data[i].ApprovedDate))
    //                response.data[i].ApprovedDate = new Date(response.data[i].ApprovedDate);
    //        }
    //        $scope.newVASList = response.data;
    //    });
    //};
    $scope.getNewVASList = function () {
        //$scope.$broadcast('show-errors-check-validity');
        try {
            //if ($scope.frmVASReport.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'GetOperationList',
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

                    $scope.newVASList = response.data;
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
    $scope.getApprovedVASList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetApprovedOperationList',
            data: {
                FromDate: $scope.reportApproved.FromDate,
                ToDate: $scope.reportApproved.ToDate
            }
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                if (!baseService.isUndefinedOrNull(response.data[i].AddedDate))
                    response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                if (!baseService.isUndefinedOrNull(response.data[i].ApprovedDate))
                    response.data[i].ApprovedDate = new Date(response.data[i].ApprovedDate);
            }
            $scope.approvedVASList = response.data;
        });
    };

    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.OperationVariationSystemId;
        //x.data.Acknowledged = true;
        $scope.Operation_Version_Name = x.data.OperationVariationSystemId;
        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationVersion?OperationVariationSystemId=' + $scope.RowId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                if (!baseService.isUndefinedOrNull(response.data[i].AddedDate))
                    response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                if (!baseService.isUndefinedOrNull(response.data[i].ApprovedDate))
                    response.data[i].ApprovedDate = new Date(response.data[i].ApprovedDate);
            }
            $scope.newVASVersionList = response.data;
        });
        angular.element(document.querySelector("#modalVersionList")).modal("toggle");


        //var gridObjRunning = $("#GridVASApproved").ejGrid("instance");
        //gridObjRunning.refreshContent(true);
        //gridObjRunning.refreshTemplate();
        //gridObjRunning = $("#GridVASNew").ejGrid("instance");
        //gridObjRunning.refreshContent(true);
        //gridObjRunning.refreshTemplate();
    };

    $scope.onClickVASVersion = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        var filteredData = $scope.selecteddata.OperationVariationSystemId;
        $scope.Operation_Version_Name = $scope.selecteddata.OperationVariationSystemId;
        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationVersion?OperationVariationSystemId=' + filteredData
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                if (!baseService.isUndefinedOrNull(response.data[i].AddedDate))
                    response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                if (!baseService.isUndefinedOrNull(response.data[i].ApprovedDate))
                    response.data[i].ApprovedDate = new Date(response.data[i].ApprovedDate);
            }
            $scope.newVASVersionList = response.data;
        });
        angular.element(document.querySelector("#modalVersionList")).modal("toggle");
    };

    $scope.approveOperationVersion = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.filteredData = $scope.selecteddata.Id;
        $scope.filteredOperationVariationSystemId = $scope.selecteddata.OperationVariationSystemId;
        $scope.message_confirmation = 'Are You Sure Want to Approve?';
        angular.element(document.querySelector('#modalConfirm')).modal('show');
    };

    $scope.vasApprove = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "ApproveOperation?Id=" + $scope.filteredData + "&&OperationVariationSystemId=" + $scope.filteredOperationVariationSystemId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector("#modalVersionList")).modal("toggle");
                    $scope.getNewVASList();
                    $scope.getApprovedVASList();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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
                columns: [{ field: "Sequence", headerText: "Element", width: 40, textAlign: 'center' },
                { field: "ElementType", headerText: "Element Type", width: 50, textAlign: 'center' },
                { field: "ElementCode", headerText: "Element Code", width: 50, textAlign: 'center' },
                { field: "TMU", headerText: "TMU", width: 40, textAlign: 'center' },
                { field: "CT1", headerText: "CT1 Sec", width: 40, textAlign: 'center' },
                { field: "CT2", headerText: "CT2 Sec", width: 40, textAlign: 'center' },
                { field: "CT3", headerText: "CT3 Sec", width: 40, textAlign: 'center' },
                { field: "CT4", headerText: "CT4 Sec", width: 40, textAlign: 'center' },
                { field: "CT5", headerText: "CT5 Sec", width: 40, textAlign: 'center' },
                { field: "TimeAvg", headerText: "TimeAvg", width: 40, textAlign: 'center' },
                { field: "Ratings", headerText: "Ratings %", width: 40, textAlign: 'center' },
                { field: "BasicTime", headerText: "Basic Time", width: 40, textAlign: 'center' }]
            });
        });
    };

    $scope.loadOperationVideo = function (Id) {
        $('#vdid').empty();
        $("#vdid").removeAttr("src");
        $('#vdid').html("");

        var videoElement = document.getElementById('vdid');
        videoElement.pause();
        videoElement.removeAttribute('src');
        videoElement.load();

        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.filteredOperationVariationSystemId = $scope.selecteddata.OperationVariationSystemId;
        $scope.filteredVersion = $scope.selecteddata.Version;
        $scope.Operation_Video_Version_Name = "";

        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationVideoName?OperationVariationSystemId=' + $scope.filteredOperationVariationSystemId + "&&Version=" + $scope.filteredVersion
        }).then(function successCallback(response) {
            $scope.Operation_Video_Version_Name = "Operation Code: " + $scope.filteredOperationVariationSystemId + "      Version: " + $scope.filteredVersion + " Video Name:" + response.data[0].OriginalVideoName
            $('#vdid').html("<source id='vdids' src='POPResources/vas/" + response.data[0].VASVideoName + "' type='video/mp4'>");
        });

        angular.element(document.querySelector("#modalPlayVideo")).modal("toggle");
    };

    $scope.getNewVASList();
    $scope.getApprovedVASList();
}