'use strict';
VehicleMovementRequisitionController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function VehicleMovementRequisitionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Vehicle Movement Requisition"
    $scope.path = 'HumanResource/VehicleMovementMaster/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';

    // #region MovementMaster
    $scope.VehicleMovementReqList = [];
    $scope.MovementAction = 'Save';
    $scope.getMovementListUrl = $scope.path + 'GetMovementList';
    
    $scope.saveVehicleReqUrl = $scope.path + 'SaveVehicleRequisition';
    $scope.deleteMovementUrl = $scope.path + 'deleteMovement/';

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
        NumberOfPassengers:null,
        Remarks: null
    };
    $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);

   

    $scope.GetVehicleRequisition = function (args) {

        $scope.VehicleRequisitionModel = Object.assign({}, args.data);
        $scope.MovementAction = 'Update';
        if (!$rootScope.isCollapsed) {
          //  $scope.GetVehicleRequisitionChildData();
            $rootScope.toggle();
        }
    };


    $scope.GetVehicleRequisitiontData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitiontData",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VehicleMovementReqList = response.data;
            //ClearFields(response.data.Sequence);

        });
    }
    $scope.GetVehicleRequisitiontData();

    $scope.GetVehicleRequisitionChildData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetVehicleRequisitionChildData",
            data: { 'headerid': $scope.VehicleRequisitionModel.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RequisitionList = response.data;
            $scope.ToLocationListBasedOnFromLoc($scope.FromLocationId);
            $scope.CreateBlankRows();

        });
    }

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
                    //ClearFieldsMovement();
                    $scope.GetVehicleRequisitiontData();

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
            Remarks: null
        };
        $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);


    }

    // #region Employee popup
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.Name = null;
    $scope.employeeList = [];
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.VehicleRequisitionModel.EmpSystemId = data.SystemId;
        $scope.VehicleRequisitionModel.EmployeeName = data.EmployeeName;
        $scope.VehicleRequisitionModel.ResponsiblePersonCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    // #endregion Employee popup

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
    $scope.GetPurposeList();

    // #endregion MovementMaster

    $scope.FromLocationList = [];
    $scope.GetFromToLocationList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetFromToLocationList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FromLocationList = response.data;

        });
    }
    $scope.GetFromToLocationList();

    $scope.ToLocListBasedOnFromLocList = [];
    $scope.ToLocationListBasedOnFromLoc = function (FromLocationId) {
        $http({
            method: 'POST',
            url: $scope.path + "ToLocationListBasedOnFromLoc",
            data: { 'fromlocId': FromLocationId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ToLocListBasedOnFromLocList = response.data;

        });
    }

   document.getElementById("reqHideShowId").style.display = "none";

    $scope.RequisitionObj = {
        Id: null,
        FromLocationId: null,
        ToLocationId:null,
    };
    $scope.RequisitionList = [];
    $scope.CreateBlankRows = function () {       
        document.getElementById("reqHideShowId").style.display = "block";        
        for (var i = 0; i < 2; i++) {
            var obj = angular.copy($scope.RequisitionObj);
            
            $scope.RequisitionList.push(obj);
           
        }

    }
    //$scope.CreateBlankRows();

    $scope.AssignToLocInFromLoc = function (LocationId, index) {
        $scope.RequisitionList[index + 1].FromLocationId = LocationId;

    }

    // Save
    $scope.SaveRequisitionChid = function () {
        $scope.ChkdRequisitionList = [];
        for (var i = 0; i < $scope.RequisitionList.length; i++) {
            if ($scope.RequisitionList[i].isSelected) {
                $scope.ChkdRequisitionList.push($scope.RequisitionList[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'SaveRequisitionChid',
            data: {
                'data': $scope.ChkdRequisitionList,
                'headerId': $scope.VehicleRequisitionModel.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');


            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');

        }
    }

    
}