'use strict';
VehicleReqForApproveController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleReqForApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Requisition For Approval"
    $scope.path = 'HumanResource/VehicleMovementMaster/';

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

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager($scope.RequisitionList).executeLocal(ej.Query().where("VehicleMovementRequisitionId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data, 
            allowSelection: true,
            selectionType: ej.Grid.SelectionType.Single,
            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
            cellSelected: $scope.tung,
            columns: ["FromLocation", "ToLocation", "WithoutPassenger"]
            
        });
        e.detailsElement.find(".tabcontrol").ejTab();
      
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
            console.log($scope.RequisitionId );
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

   
    $scope.Test = function () {
        var FromDate = null;
        var ToDate = null;

        $scope.EqFromDateCheckList = [];
        $scope.EqFromTimeCheckList = [];
        $scope.isMergedList = [];
        $scope.IdList = [];
        var ob = {};
        for (var i = 0; i < $scope.VehicleMovementReqList.length; i++) {
            // #region
            //if ($scope.VehicleMovementReqList[i].isMerge) {
            //    $scope.isMergedList.push($scope.VehicleMovementReqList[i].isMerge);
            //    if ($scope.isMergedList.length >= 1) {
            //        $scope.EqFromDateCheckList.push($scope.VehicleMovementReqList[i].FromDate, $scope.VehicleMovementReqList[i].ToDate);
            //        if (($scope.EqFromDateCheckList[0] == $scope.EqFromDateCheckList[2]) && ($scope.EqFromDateCheckList[1] == $scope.EqFromDateCheckList[3])) {
            //            ob.Id = $scope.VehicleMovementReqList[i].Id;
            //            console.log(ob.Id);
            //        }
            //    }
            //    else {}
            //}
            // #endregion
            if ($scope.VehicleMovementReqList[i].isMerge) {
                FromDate = $scope.VehicleMovementReqList[i].FromDate;
                ToDate = $scope.VehicleMovementReqList[i].ToDate;
                if (FromDate == $scope.VehicleMovementReqList[i].FromDate && ToDate == $scope.VehicleMovementReqList[i].ToDate) {

                }

                $scope.CheckedVehicleRequisitionModel.push($scope.VehicleMovementReqList[i]);
            }
            
           
        }


    }

    $scope.SaveVehicleAllocation = function () {
        $scope.EqFromDateCheckList = [];
        for (var i = 0; i < $scope.VehicleRequisitionModel.length; i++) {
            $scope.CheckedVehicleRequisitionModel.push($scope.VehicleRequisitionModel[i].isMerge);
            for (var j = 0; j < $scope.CheckedVehicleRequisitionModel.length; j++) {
                $scope.EqFromDateCheckList.push($scope.CheckedVehicleRequisitionModel[j].FromDate);
                console.log($scope.EqFromDateCheckList);
            }
        }
        
    }

   
}