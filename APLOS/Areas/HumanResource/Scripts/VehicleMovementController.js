'use strict';
VehicleMovementController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleMovementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Requisition For Approval"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    //$scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleAllocation';
    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleDriverAllocation';
    $scope.Action = 'Update';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

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
    //$scope.GetVehicleList();

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
    //$scope.GetDriverList();

    $scope.TripList = [];
    $scope.GetTripData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetTripData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.TripList = response.data;

        });
    }
    //$scope.GetTripData();

    $scope.RequisitionCildList = [];
    $scope.GetVehicleRequisitionChildData = function (headerId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitionChildData",
            data: { 'headerId': headerId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionChildList = response.data;

        });
    }

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var v2 = filteredData;
        $scope.GetVehicleReqTreeViewData(filteredData, v2);
       //$scope.GetMergedRequisition(filteredData, e);
       // $scope.GetVehicleRequisitionChildData(filteredData)

    }

    $scope.RequisitionMergedList = [];
    $scope.GetMergedRequisition = function (AppliedId, e) {
        $http({
            method: 'POST',
            url: $scope.path + "GetMergedRequisition",
            data: { 'appliedid': AppliedId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionMergedList = response.data;

                if (baseService.arrayLength($scope.RequisitionMergedList) > 0) {
                    var data = ej.DataManager($scope.RequisitionMergedList).executeLocal(ej.Query().where("AppliedId", "equal", parseInt(AppliedId), true).take(100));
                    e.detailsElement.find("#detailGrid").ejGrid({
                        dataSource: data,
                        columns: ["Id", "FromDate", "ToDate", "FromTime", "ToTime", "FromLocation", "ToLocation", "ByWhom", "Department", "Purpose", "PersonalOfficial", "AppliedId"],
                        childGrid: {
                            dataSource: $scope.RequisitionChildList,
                            queryString: "VehicleMovementRequisitionId",
                            columns: ["Id", "FromLocation", "ToLocation"]
                        }

                    });
                    e.detailsElement.find(".tabcontrol").ejTab();
                }
            
        });
    }

    

    // #region comment
    
    $scope.GetVehicleReqTreeViewData = function () {

            $http({
                method: 'POST',
                url: $scope.path + "GetTripData",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.TripList = response.data;

            $http({
                method: 'POST',
                url: $scope.path + "GetMergedRequisition",
               // data: { 'appliedid': AppliedId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.RequisitionMergedList = response.data;

                $http({
                    method: 'POST',
                    url: $scope.path + "GetVehicleRequisitionChildData",
                   // data: { 'headerId': VehicleMovementRequisitionId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.RequisitionChildList = response.data;

                    $scope.loadGrid($scope.RequisitionMergedList, $scope.RequisitionChildList, $scope.TripList);

                });
            });

        });
        
    }

    $scope.GetVehicleReqTreeViewData();

    $scope.loadGrid = function (mergedreqData, reqChildData, tripData) {
        $scope.RequisitionMergedList = mergedreqData;
        $scope.RequisitionChildList = reqChildData;
        $scope.TripList = tripData;
        var gridObj = $("#Grid").data("ejGrid");

        if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();

        $("#Grid").ejGrid({
            dataSource: $scope.TripList,
            allowSelection: true,
            selectionType: ej.Grid.SelectionType.Single,
            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
            cellSelected: $scope.VehicleAllocationPopup,
            columns: ["Row_Num", "FromDate", "ToDate", "FromTime", "ToTime"],

            childGrid: {

                dataSource: $scope.RequisitionMergedList,
                queryString: "AppliedId",
                allowSelection: true,
                selectionType: ej.Grid.SelectionType.Single,
                selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                columns: ["Row_Num", "FromDate", "ToDate", "FromTime", "ToTime",  "ByWhom", "Department", "Purpose", "PersonalOfficial", "AppliedId"],

                childGrid: {
                    dataSource: $scope.RequisitionChildList,
                    queryString: "VehicleMovementRequisitionId",
                    columns: ["Row_Num", "FromLocation", "ToLocation"]

                }
            }

        }).render();

    };


    //#endregion comment

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

    $scope.TripId = null
    $scope.VehicleAllocationPopup = function (args) {
        $scope.TripId = args.data.Id;
        $scope.GetVehicleList();
        $scope.GetDriverList();
        $scope.VehicleRequisitionModel.FromDate = args.data.FromDate;
        $scope.VehicleRequisitionModel.ToDate = args.data.ToDate;
        $scope.VehicleRequisitionModel.FromTime = args.data.FromTime;
        $scope.VehicleRequisitionModel.ToTime = args.data.ToTime;
        angular.element(document.querySelector("#reqPopup")).modal('show');
    }

    $scope.SaveVehicleAllocation = function () {

        
            $http({
                method: 'POST',
                url: $scope.saveVehicleReqUrl,
                data: {
                    'data': $scope.VehicleRequisitionModel,
                    'tripId': $scope.TripId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector("#reqPopup")).modal('hide');
                    $scope.GetTripApproved();
                    $scope.GetTripData();
                    $scope.GetVehicleReqTreeViewData();
                   
                    ClearVehicleAllocation();
                    
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

    }

    $scope.ClearVehicleAllocation = function () {
        ClearFieldVehicleAllocation();
        return true;
    };

    function ClearFieldVehicleAllocation() {

        $scope.VehicleRequisitionModel = {
            Id: null,
            FromDate: null,
            ToDate: null,
            FromTime: null,
            ToTime: null,
            VehicleMasterId: null,
            DriverMasterId: null
        };
        $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);
    }

    $scope.ApprovedTripList = [];
    $scope.DataMappedWithTripList = [];
    $scope.GetTripApproved = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDataMappedWithTrip",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DataMappedWithTripList = response.data;

            $http({
                method: 'POST',
                
                url: $scope.path + "GetTripApproved",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ApprovedTripList = response.data;
                

                $scope.loadTripFGridData($scope.DataMappedWithTripList, $scope.ApprovedTripList);
            });
        });
    }
    $scope.GetTripApproved();

    $scope.loadTripFGridData = function (mwtData, gtaData) {
        $scope.DataMappedWithTripList = mwtData;
        $scope.ApprovedTripList = gtaData;
        

        var gridObj = $("#TripGrid").data("ejGrid");

        if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();

        $("#TripGrid").ejGrid({
            dataSource: $scope.DataMappedWithTripList,
            allowSelection: true,
            selectionType: ej.Grid.SelectionType.Single,
            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
           // cellSelected: $scope.VehicleAllocationPopup,
            columns: ["Row_Num", "FromDate", "ToDate", "FromTime", "ToTime", "DriverName", "VehicleName", "VehicleNumber"],

            childGrid: {

                dataSource: $scope.ApprovedTripList,
                queryString: "TripId",
                allowSelection: true,
                selectionType: ej.Grid.SelectionType.Single,
                selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                columns: ["Row_Num", "TripNumber" ,"FromDate", "ToDate", "FromTime", "ToTime"],
               
            }

        }).render();

    }
    

}