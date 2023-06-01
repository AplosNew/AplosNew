'use strict';
VehicleReqForApproveController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleReqForApproveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Requisition For Approval"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleAllocation';
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
            url: $scope.path + "GetVehicleRequisitiontData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMovementReqList = response.data;

        });
    }
    $scope.GetVehicleRequisitiontData();

    $scope.RequisitionList = [];
    $scope.GetVehicleRequisitionChildData = function (headerId, e) {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitionChildData",
            data: { 'headerId': headerId},
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
                    columns: ["Id", "FromLocation", "ToLocation"]

                });
                e.detailsElement.find(".tabcontrol").ejTab();
            }

        });
    }
    $scope.GetVehicleRequisitionChildData();


    $scope.requisitiondetailTemp = "#requisitiontabGridContents";
    $scope.requisitiondetailgrid = function requisitiondetailGridData(e) {
        //debugger;

        var filteredData = e.data["Id"];
        $scope.GetVehicleRequisitionChildData(filteredData, e);

    }
    // #endregion 1st Tab


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
            //if ($scope.showApprovePopUp) {
            //    angular.element(document.querySelector("#reqPopup")).modal('show');
            //    $scope.GetDriverList();
            //    $scope.GetVehicleList();
            //}


        } catch (e) {
            ShowResult(e, 'failure');
        }

    }

    // #region 
    $scope.SaveVehicleAllocation = function () {
       
        $scope.MergeRows();

        if ($scope.isMergedList.length > 0) {
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

        }


    }
    // #endregion

    


    // #region Vehicle InOut
    //  #region VehicleIn
    $scope.VehicleInTemp = {
        Id: null,
        InDate: null,
        InTime: null,
        InKillometer: null,
        InRemarks: null,
        OutDate: null,
        OutTime: null,
        OutKillometer: null,
        OutRemarks: null

    };

    $scope.VehicleInModel = Object.assign({}, $scope.VehicleInTemp);

    $scope.GetIn = function (args) {
        $scope.VehicleInModel = Object.assign({}, args.data);
        $scope.ActionIn = 'Update';
        if (!$rootScope.isCollapsed) {

            // $rootScope.toggle();
        }
    }

    $scope.VehicleInList = [];
    $scope.VehicleInData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleInData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleInList = response.data;

        });
    }
    $scope.VehicleInData();

    $scope.SaveVehicleIn = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleInForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveVehicleIn',
                data: {
                    'data': $scope.VehicleInModel,
                    'headerId': $scope.VehicleRequisitionModel.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleIN();
                    $scope.VehicleInData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVehicleIn = function () {
        if (!baseService.isUndefinedOrNull($scope.VehicleInModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteVehicleIn' + $scope.VehicleInModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleIN();
                    $scope.VehicleInData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVehicleIn = function () {
        ClearFieldsVehicleIN();
        return true;
    };

    function ClearFieldsVehicleIN() {
        $scope.ActionIn = 'Save';

        $scope.VehicleInModel = {
            Id: null,
            InDate: null,
            InTime: null,
            InKillometer: null,
            InRemarks: null,
            OutDate: null,
            OutTime: null,
            OutKillometer: null,
            OutRemarks: null
        };
        $scope.VehicleInModel = Object.assign({}, $scope.VehicleInTemp);
    }
    //  #endregion VehicleIn

    //  #region VehicleOut
    $scope.VehicleOutTemp = {
        Id: null,
        OutDate: null,
        OutTime: null,
        OutKillometer: null,
        OutRemarks: null

    };

    $scope.VehicleOutModel = Object.assign({}, $scope.VehicleOutTemp);

    $scope.GetOut = function (args) {
        $scope.VehicleOutModel = Object.assign({}, args.data);
        $scope.ActionOut = 'Update';
        if (!$rootScope.isCollapsed) {

            //$rootScope.toggle();
        }
    }

    $scope.VehicleOutList = [];
    $scope.VehicleOutData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleOutData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleOutList = response.data;

        });
    }


    $scope.SaveVehicleOut = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.VehicleOutForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveVehicleOut',
                data: {
                    'data': $scope.VehicleInModel,
                    'headerId': $scope.VehicleRequisitionModel.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleOut();
                    $scope.VehicleOutData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteVehicleOut = function () {
        if (!baseService.isUndefinedOrNull($scope.VehicleOutModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteVehicleOut' + $scope.VehicleOutModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsVehicleOut();
                    $scope.VehicleOutData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearVehicleOut = function () {
        ClearFieldsVehicleOut();
        return true;
    };

    function ClearFieldsVehicleOut() {
        $scope.ActionOut = 'Save';

        $scope.VehicleOutModel = {
            Id: null,
            OutDate: null,
            OutTime: null,
            OutKillometer: null,
            Remarks: null
        };
        $scope.VehicleOutModel = Object.assign({}, $scope.VehicleOutTemp);
    }
    //  #endregion VehicleOut
    // #endregion Vehicle InOut

}