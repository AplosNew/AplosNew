'use strict';
VehicleTripController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleTripController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Trip"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    //$scope.saveTripUrl = $scope.path + 'SaveVehicleAllocation';
    $scope.saveTripUrl = $scope.path + 'GenerateTripNumber';
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

    $scope.NoOfPassengers = [
        {
            'name': '0',
            'value': 0
        },
        {
            'name': '1',
            'value': 1
        },
        {
            'name': '2',
            'value': 2
        },
        {
            'name': '3',
            'value': 3
        },
        {
            'name': '4',
            'value': 4
        },
        {
            'name': '5',
            'value': 5
        }
    ];

    $scope.Get = function (args) {
        $scope.VehicleRequisitionModel = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $scope.GetVehicleList();
            $scope.GetDriverList();
            angular.element(document.querySelector("#vehicleInOutPopup")).modal('show');
            //$rootScope.toggle();
        }
    }

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


    // #region 1st Tab
    $scope.VehicleMovementReqList = [];
    $scope.GetVehicleRequisitiontData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "ApprovedRequisitionForMerged",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMovementReqList = response.data;

        });
    }
    $scope.GetVehicleRequisitiontData();

    $scope.GeneratedTripList = [];
    $scope.GetTripGenerated = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetTripGenerated",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GeneratedTripList = response.data;

        });
    }
    $scope.GetTripGenerated();

    $scope.RequisitionList = [];
    $scope.GetVehicleRequisitionChildData = function (headerId, e) {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitionChildData",
            data: { 'headerId': headerId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionList = response.data;

            if (baseService.arrayLength($scope.RequisitionList) > 0) {
                var data = ej.DataManager($scope.RequisitionList).executeLocal(ej.Query().where("VehicleMovementRequisitionId", "equal", parseInt(headerId), true).take(100));
                e.detailsElement.find("#requisitiondetailgrid").ejGrid({
                    dataSource: data,
                    allowSelection: true,
                    selectionType: ej.Grid.SelectionType.Single,
                    selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                    cellSelected: $scope.tung,
                    columns: ["Row_Num", "FromLocation", "ToLocation"]

                });
                e.detailsElement.find(".tabcontrol").ejTab();
            }

        });
    }
    // $scope.GetVehicleRequisitionChildData();


    $scope.requisitiondetailTemp = "#requisitiontabGridContents";
    $scope.requisitiondetailgrid = function requisitiondetailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        $scope.GetVehicleRequisitionChildData(filteredData, e);

    }
    // #endregion 1st Tab

    var currentDate = new Date();
    $scope.ApproveRequisitionTemp = {
        Id: null,
        FromDate: null,
        ToDate: null,
        FromTime: null,
        ToTime: null,

    };
    $scope.ApproveRequisitionModel = Object.assign({}, $scope.ApproveRequisitionTemp);

    // Save
    $scope.CheckedVehicleRequisitionModel = [];

    $scope.showApprovePopUp = false;
    $scope.isMergedList = [];
    $scope.MergeRows = function () {
        try {
            var FromDate = null;
            var ToDate = null;
            var FromTime = null;
            var ToTime = null;
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
                        ToDate = $scope.VehicleMovementReqList[i].ToDate;
                        FromTime = $scope.VehicleMovementReqList[i].FromTime;
                        ToTime = $scope.VehicleMovementReqList[i].ToTime;
                    }
                    //ToDate = $scope.VehicleMovementReqList[i].ToDate;
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
                $scope.ApproveRequisitionModel.FromDate = FromDate;
                $scope.ApproveRequisitionModel.ToDate = ToDate;
                $scope.ApproveRequisitionModel.FromTime = FromTime;
                $scope.ApproveRequisitionModel.ToTime = ToTime;
                angular.element(document.querySelector("#reqPopup")).modal('show');

            }


        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    // #region 
    $scope.SaveVehicleTrip = function () {
       
        if ($scope.isMergedList.length > 0) {
            $http({
                method: 'POST',
                url: $scope.saveTripUrl,
                data: {
                    'data': $scope.ApproveRequisitionModel,
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
                    $scope.isMergedList = [];
                    $scope.GetVehicleRequisitiontData();
                    $scope.GetTripGenerated();
                    

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }


    }
    // #endregion

    // #region Update Requisition
    $scope.PurposeList = [];
    $scope.GetPurposeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPurposeList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurposeList = response.data;

        });
    }
    $scope.VehicleRequisitionTemp = {
        Id: null,
        Date: null,
        FromTime: null,
        ToTime: null,
        PurposeId: null,
        PersonalOfficial: null,
        EmpSystemId: null,
        EmployeeName: null,
        ResponsiblePersonCode: null,
        NumberOfPassengers: null,
        Remarks: null
    };
    $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);

    $scope.EditRequisitionPopup = function (args) {
        $scope.VehicleRequisitionModel = Object.assign({}, args.data);
        $scope.VehicleRequisitionModel.NumberOfPassengers = $scope.VehicleRequisitionModel.NumberOfPassengers.toString();
        $scope.GetPurposeList();
        angular.element(document.querySelector("#editreqPopup")).modal('show');
    }

    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleRequisition';
    $scope.SaveMovement = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.VehicleRequisitionForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveVehicleReqUrl,
                data: {
                    'data': $scope.VehicleRequisitionModel
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.VehicleRequisitionModel.Id = response.data.Id;
                    angular.element(document.querySelector("#editreqPopup")).modal('hide');
                    //ClearFieldsMovement();
                    //$scope.GetVehicleRequisitiontData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };



    $scope.ClearMovement = function () {
        ClearFieldsMovement();
        return true;
    };

    function ClearFieldsMovement() {
        $scope.MovementAction = 'Save';
        $scope.VehicleRequisitionModel = {
            Id: null,
            Date: null,
            FromTime: null,
            ToTime: null,
            PurposeId: null,
            PersonalOfficial: null,
            EmpSystemId: null,
            EmployeeName: null,
            ResponsiblePersonCode: null,
            NumberOfPassengers: null,
            Remarks: null
        };
        $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);


    }
    // #endregion Update Requisition

    

}