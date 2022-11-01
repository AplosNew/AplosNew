'use strict';
authorizationConfigController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', '$window', 'cboService'];
function authorizationConfigController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, $window, cboService) {
    $rootScope.title = "Authorization Config";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'employees/authorizationConfig/';
    $scope.authorizationList = [];

    $scope.getData = function () {
        $scope.authorizationList = [];
        $http.get('employees/authorizationConfig/GetList?actionStatus=' + $scope.employeeInformation.ActionStatus)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.authorizationList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.onrowdatabound = function (e) {
        if (e.data.EmployeeStatus === 'Separated')
            e.row.css("background-color", "red");
    };

    $scope.ActionStatusList = [];
    cboService.getEnumCbo("enum/GetAuthorizationCbo", function (result) {
        $scope.ActionStatusList = result;
    });

    // #region  Dynamic PopUp
    $scope.popUpList = [];

    $scope.employeeInformation = {
        PlantId: $window.plantId
        , EmployeeCode: null
        , EmployeeName: null
        , SystemId: null

    };
    $scope.popUpDataList = [];
    $scope.popUp = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.employeeInformationForm.$valid) {
                $scope.popUpDataList = [];
                $http({
                    method: 'GET',
                    url: 'employees/authorizationconfig/getallemployeedata'

                }).then(function successCallback(response) {
                    $scope.popUpDataList = response.data;
                });
                angular.element(document.querySelector('#popUp')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        $scope.employeeInformation.EmployeeId = ob.SystemId;
        $scope.employeeInformation.EmployeeName = ob.EmployeeName;
        $scope.employeeInformation.EmployeeCode = ob.EmployeeCode;
        $scope.employeeInformation.CompanyId = ob.CompanyId;
        $scope.employeeInformation.PlantId = ob.PlantId;
        $scope.employeeInformation.GroupID = ob.GroupID;
        angular.element(document.querySelector('#popUp')).modal('hide');
        $scope.Save();
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    // #endregion

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.employeeInformationForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.path + 'Create',
                        data: $scope.employeeInformation,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            $scope.Clear();
                        }
                    }, function errorCallback(response) {
                    });
                    return true;
                }
            }
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.valuePassInDelModal = function (obj) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.employeeInformation = gridObj.getSelectedRecords()[0];

        $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + $scope.employeeInformation.EmployeeCode + ' - ' + $scope.employeeInformation.EmployeeName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'Delete',
                data: $scope.employeeInformation,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.authorizationList = [];
                    $scope.getData();
                    $scope.Clear();
                }
            }, function errorCallback(response) {
            });
            return true;
            //}
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
        $scope.employeeInformation.EmployeeId = null;
        $scope.employeeInformation = { ActionStatus: $scope.employeeInformation.ActionStatus };
    }

}