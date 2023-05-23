'use strict';
VehicleReqForApproveController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleReqForApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Requisition For Approval"
    $scope.path = 'HumanResource/VehicleMovementMaster/';


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

    $scope.detailgrid = function detailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        var data = ej.DataManager($scope.RequisitionList).executeLocal(ej.Query().where("VehicleMovementRequisitionId", "equal", parseInt(filteredData), true).take(100));
        e.detailsElement.find("#detailGrid").ejGrid({
            dataSource: data, 
            columns: ["FromLocation", "ToLocation", "WithoutPassenger"]
            //columns: [
            //    { field: "FromLocation", headerText: "From Location", width: 100 },
            //    { field: "ToLocation", headerText: "To Location", width: 100 },
            //    { field: "WithoutPassenger", headerText: "Without Passenger", width: 100 },
                
            //]
        });
        e.detailsElement.find(".tabcontrol").ejTab();
       
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
}