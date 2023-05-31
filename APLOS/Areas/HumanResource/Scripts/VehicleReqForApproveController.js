'use strict';
VehicleReqForApproveController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleReqForApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Requisition For Approval"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleAllocation';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.VehicleMovementReqList = [];
    $scope.GetVehicleRequisitiontData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitiontData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMovementReqList = response.data;

        });
    }
    $scope.GetVehicleRequisitiontData();

    $scope.RequisitionMergedList = [];
    $scope.GetMergedRequisition = function (AppliedId, e) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMergedRequisition",
            data: { 'appliedid': AppliedId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionMergedList = response.data;
            if (baseService.arrayLength($scope.RequisitionMergedList) > 0) {
                var data = ej.DataManager($scope.RequisitionMergedList).executeLocal(ej.Query().where("AppliedId", "equal", parseInt(AppliedId), true).take(100));
                e.detailsElement.find("#detailGrid").ejGrid({
                    dataSource: data,
                    allowSelection: true,
                    selectionType: ej.Grid.SelectionType.Single,
                    selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                    cellSelected: $scope.tung,
                    columns: ["Id", "FromDate", "ToDate", "FromTime", "ToTime", "FromLocation", "ToLocation", "ByWhom", "Department", "Purpose", "PersonalOfficial"]

                });
                e.detailsElement.find(".tabcontrol").ejTab();
            }

        });
    }

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        $scope.GetMergedRequisition(filteredData, e);
    //    if (baseService.arrayLength($scope.RequisitionMergedList) > 0) {
    //var data = ej.DataManager($scope.RequisitionMergedList).executeLocal(ej.Query().where("AppliedId", "equal", parseInt(filteredData), true).take(100));
    //        e.detailsElement.find("#detailGrid").ejGrid({
    //            dataSource: data,
    //            allowSelection: true,
    //            selectionType: ej.Grid.SelectionType.Single,
    //            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
    //            cellSelected: $scope.tung,
    //            columns: ["Id", "FromDate", "ToDate", "FromTime", "ToTime", "FromLocation", "ToLocation", "ByWhom", "Department", "Purpose", "PersonalOfficial"]

    //        });
    //        e.detailsElement.find(".tabcontrol").ejTab();
    //    }

    }

    $scope.tung = function (args) {
        $scope.PopupOpen();
    };

    $scope.RequisitionId = null;
    $scope.PopupOpen = function (args) {
        $scope.RequisitionId = args.data.Id;
        $scope.GetVehicleList();
        $scope.GetDriverList();

        angular.element(document.querySelector("#reqPopup")).modal('show');
    }


    $scope.RequisitionList = [];
    $scope.GetVehicleRequisitionChildData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitionChildData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionList = response.data;

        });
    }
    $scope.GetVehicleRequisitionChildData();

    

    $scope.VehicleList = [];
    $scope.GetVehicleList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleList = response.data;

        });
    }

    $scope.DriverList = [];
    $scope.GetDriverList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDriverList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DriverList = response.data;
            
        });
    }

    $scope.VehicleRequisitionTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        FromTime: null,
        ToTime: null,
        VehicleMasterId: null,
        DriverMasterId: null

    };
    $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);

    // Save
    $scope.CheckedVehicleRequisitionModel = [];

    $scope.showApprovePopUp = false;
    $scope.isMergedList = [];
    $scope.MergeRows = function () {
        try {
            var FromDate = null;
            var ToDate = null;

            $scope.EqFromDateCheckList = [];
            $scope.EqFromTimeCheckList = [];
            $scope.isMergedList = [];
            $scope.IdList = [];
            var ob = {};
            for (var i = 0; i < $scope.VehicleMovementReqList.length; i++) {

                if ($scope.VehicleMovementReqList[i].isMerge) {
                    $scope.isMergedList.push($scope.VehicleMovementReqList[i]);
                    
                    if (baseService.isUndefinedOrNull(FromDate)) {
                        FromDate = $scope.VehicleMovementReqList[i].FromDate;
                    }
                    ToDate = $scope.VehicleMovementReqList[i].ToDate;
                }
            }

            for (var i = 0; i < $scope.isMergedList.length; i++) {
                if (FromDate != $scope.isMergedList[i].FromDate) {
                    $scope.showApprovePopUp = false;
                    throw "Please check same From Date.";

                }
                else {
                    $scope.showApprovePopUp = true;
                }
            }
            if ($scope.showApprovePopUp) {
                angular.element(document.querySelector("#reqPopup")).modal('show');
                $scope.GetDriverList();
                $scope.GetVehicleList();
            }


        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    // #region 
    $scope.SaveVehicleAllocation = function () {
        

       // if ($scope.VehicleRequisitionForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveVehicleReqUrl,
                data: {
                    'data': $scope.VehicleRequisitionModel,
                    'reqdata': $scope.isMergedList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector("#reqPopup")).modal('hide');
                    
                    $scope.GetVehicleRequisitiontData();
                    $scope.GetVehicleAllocation();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

       // }


    }
    // #endregion

    $scope.VehicleAllocationList = [];
    $scope.GetVehicleAllocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleAllocation",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleAllocationList = response.data;
            
        });
    }
    $scope.GetVehicleAllocation();

}