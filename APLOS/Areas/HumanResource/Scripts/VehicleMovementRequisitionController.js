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
        Remarks: null
    };
    $scope.VehicleRequisitionModel = Object.assign({}, $scope.VehicleRequisitionTemp);

    $scope.GetMovementSequence = function () {
        cboService.getSequence($scope.getMovementSeqUrl, function (data) {
            $scope.VehicleRequisitionTemp.Sequence = data;
            $scope.VehicleRequisitionModel.Sequence = data;
        });
    };
    //$scope.GetMovementSequence();

    $scope.GetVehicleRequisition = function (args) {

        $scope.VehicleRequisitionModel = Object.assign({}, args.data);
        $scope.MovementAction = 'Update';
        if (!$rootScope.isCollapsed) {
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
                    ClearFieldsMovement();
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

}