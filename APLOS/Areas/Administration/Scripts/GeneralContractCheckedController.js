'use strict';
GeneralContractCheckedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window','$controller'];
function GeneralContractCheckedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window, $controller) {
    $rootScope.title = 'General Contract Checked';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractChecked/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });


    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.GriddataISUnCheckedList = [];
    $scope.GetUncheckedData = function () {
        $http.get('Administration/GeneralContractChecked/GetUncheckedData')
            .then(function successCallback(response) {
                $scope.GriddataISUnCheckedList = response.data;
            })
    }
    $scope.GetUncheckedData();

    $scope.GriddataISCheckedList = [];
    $scope.GetcheckedData = function () {
        $http.get('Administration/GeneralContractChecked/GetcheckedData')
            .then(function successCallback(response) {
                $scope.GriddataISCheckedList = response.data;
            })
    }
    $scope.GetcheckedData();

    // Employee Who Responsible For Approving
    $scope.CheckedByList = [];
    $scope.GetAllCheckBy = function () {
        $http.get('Administration/GeneralContractChecked/GetAllCheckBy')
            .then(function successCallback(response) {
                $scope.CheckedByList = response.data;
            })
    }
    $scope.GetAllCheckBy();

    //-----------------------------------------------------------------------------------
    $scope.onClickPOA = function (z) {
        debugger;

        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.podata = gridObj.getSelectedRecords()[0];

        $scope.message = 'Are you sure want to ' + $scope.podata.CheckedStatus + '?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');

    };
    $scope.approvalAlert = function () {
        $scope.message = 'Are you sure want to Approve?';
        angular.element(document.querySelector('#poapprovealert')).modal('show');
    };

    //-----------------------------------------------------------------------------------
    $scope.POApprovalList = [
        {
            'Text': 'Checked',
            'Value':'Checked'
        }
    ];
    //$scope.poApproved = function () {
    //    cboService.getEnumCbo("enum/GetPOApprovalStatusCbo", function (result) {
    //        $scope.POApprovalList = result[3];
    //    });
    //}
    //$scope.poApproved();

    $scope.ContractItemList = []
    $scope.GetChildList = function () {
        $http.get('Administration/GeneralContractChecked/GetChildList')
            .then(function successCallback(response) {
                $scope.ContractItemList = response.data;
            });
    }
    $scope.GetChildList();
   
    //$scope.lst = [];
    //$scope.data1 = $scope.lst;
    
    $scope.detailTemp = "#tabGridContents";
    
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

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

    //  #region Save
    $scope.poApp = function () {
        try {

            if ($scope.podata.CheckedStatus === "For Checked" || $scope.podata.CheckedStatus === "Select" || baseService.isUndefinedOrNull($scope.podata.CheckedStatus)) {
                ShowResult("Please Select Checked By Status", 'failure');
                return false;
            }

            else if ($scope.podata.CheckedStatus === "Checked" && baseService.isUndefinedOrNull($scope.podata.AuthorizedBy)) {
                ShowResult("Please Select To be Approved By", 'failure');
                return false;
            }
            else if (($scope.podata.CheckedStatus === "Hold" || $scope.podata.CheckedStatus === "Reject") && baseService.isUndefinedOrNull($scope.podata.CheckedRejectReason)) {
                ShowResult("Enter The Reason", 'failure');
                return false;
            }

            var filteredData = $scope.podata.Id;
            var data = ej.DataManager($scope.ContractItemList).executeLocal(ej.Query().where("GeneralContractEntryId", "equal", parseInt(filteredData), true).take(100));

            if (data.length == 0) {
                throw "Requisition Details is reuired.";
            }


            $http({
                method: 'POST',
                url: 'Administration/GeneralContractChecked/GeneralContractChecked',
                data: {
                    'headerId': $scope.podata.Id,
                   
                    'CheckedStataus': $scope.podata.CheckedStatus,
                    
                    'AuthorizedById': $scope.podata.AuthorizedBy,
                    'CheckedReason': $scope.podata.CheckedRejectReason,

                },

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //  #endregion Save
}