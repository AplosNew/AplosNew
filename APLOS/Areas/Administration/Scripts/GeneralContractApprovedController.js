'use strict';
GeneralContractApprovedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window', '$controller'];
function GeneralContractApprovedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller) {
    $rootScope.title = 'General Contract Approved';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractApproved/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
   
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.approvalStatusList = [
        {
            'Text': 'Approved',
            'Value': 'Approved'
        }
    ];
    //$scope.LoadapprovalStatus = function () {
    //    cboService.getEnumCbo("enum/GetCheckedStatusCbo", function (result) {
    //        $scope.approvalStatusList = result;
    //    });

    //}
    //$scope.LoadapprovalStatus();

    $scope.UnapprovedList = [];
    $scope.GetcheckedData = function () {
        $http.get('Administration/GeneralContractChecked/GetcheckedData')
            .then(function successCallback(response) {
                $scope.UnapprovedList = response.data;
            })
    }
    $scope.GetcheckedData();

    $scope.ApprovedList = [];
    $scope.GetcheckedApprovedData = function () {
        $http.get('Administration/GeneralContractApproved/GetcheckedApprovedData')
            .then(function successCallback(response) {
                $scope.ApprovedList = response.data;
            })
    }
    $scope.GetcheckedApprovedData();

    $scope.onClickPOA = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];

        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');

    };

    $scope.ContractItemList = []
    $scope.GetChildList = function () {
        $http.get('Administration/GeneralContractChecked/GetChildList')
            .then(function successCallback(response) {
                $scope.ContractItemList = response.data;
            });
    }
    $scope.GetChildList();

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        var filteredData = e.data["Id"];
        var data = ej.DataManager($scope.ContractItemList).executeLocal(ej.Query().where("GeneralContractEntryId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({

            dataSource: data,
            columns: [
                { field: "UserName", headerText: "Item", width: 100 },
                { field: "AvgQty", headerText: "Avg Qty", width: 100 },
                { field: "TransactionQuantity", headerText: "Transaction Quantity", width: 100 },
                { field: "Rate", headerText: "Rate", width: 100 },
                { field: "Amount", headerText: "Amount", width: 100 },
            ]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
    }

    $scope.poAppAuth = function () {

        if (baseService.isUndefinedOrNull($scope.podata.CheckedStatus) || $scope.podata.CheckedStatus === "Select") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }
        else if ($scope.podata.CheckedStatus === "Select" || $scope.podata.CheckedStatus === "For Approval") {
            ShowResult("Please Select Approved By Status", 'failure');
            return false;
        }
        else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.RejectApprovedReason)) {

            ShowResult("Enter The Reason", 'failure');
            return false;
        }

        var filteredData = $scope.podata.Id;
        var data = ej.DataManager($scope.ContractItemList).executeLocal(ej.Query().where("GeneralContractEntryId", "equal", parseInt(filteredData), true).take(100));
        if (data.length == 0) {
            throw "Contract Details is reuired.";
        }


        //debugger;
        $http({
            method: 'POST',
            url: 'Administration/GeneralContractApproved/GeneralContractAuth',
            data: {
                'headerId': $scope.podata.Id,
               
                'ApprovedStataus': $scope.podata.CheckedStatus,
                'ApprovedReason': $scope.podata.RejectApprovedReason,
                'AuthorizedBy': $scope.podata.AuthorizedBy,                
                'CheckedBy': $scope.podata.CheckedBy,               
            },

            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.RequisitionUnapproved();
                $scope.LoadapprovalStatus();
                $route.reload();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.PrintData = function (data) {
        try {
            $scope.fileName = "General Contract Approved Report.xlsx";
            //$scope.ReportFormat = 'Excel';
            $scope.ReportFormat = 'Pdf';
            var url = 'Administration/GeneralContractChecked/GetGeneralContractReport?reportFormat=' + $scope.ReportFormat + '&ContractId=' + data.data.Id;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}