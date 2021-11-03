'use strict';
oDDeleteController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function oDDeleteController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'OD Delete';
    $scope.index = -1;
    $scope.OdList = [];
    $scope.path = 'Employees/ODDelete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.deleteUrl = $scope.path + 'delete/';

    // #region Dynamic PopUp
    $scope.popUpList = [];

    $scope.employeeInformation = {
        PlantId: $window.plantId
        , EmployeeCode: null
        , EmployeeName: null
        , SystemId: null
    };

    $scope.popUp = function (name) {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: '',
            searchBy: '',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        try {

            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';

            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist?plantId=' + $scope.employeeInformation.PlantId;
            $scope.popUpParameters.sort = 'EmployeeCodeNumeric';
            $scope.popUpParameters.searchBy = 'EmployeeCode';

            if (name === 'EmployeeInformation') {
                $scope.popUpTitle = 'Employee Information';
            }

            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#popUp')).modal('show');
            $scope.popUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectdblClick = function (data) {
        setPartyName(data);
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    function setPartyName(ob) {
      
            $scope.employeeInformation.SystemId = ob.SystemId;
            $scope.employeeInformation.EmployeeName = ob.EmployeeName;
            $scope.employeeInformation.EmployeeCode = ob.EmployeeCode;
            $scope.getData($window.plantId, $scope.employeeInformation.SystemId);
        
    }
    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
// #endregion

    $scope.getData = function (PlantId, EmpSystemId) {
        $http({
            method: 'GET',
            url: 'Employees/ODDelete/Query?PlantId=' + PlantId + '&EmpSystemId=' + EmpSystemId
        }).then(function successCallback(response) {
            $scope.OdList = response.data;

        });
    };
    

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.employeeInformation = data;
        if (!baseService.isUndefinedOrNull($scope.employeeInformation.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.employeeInformation.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Employees/ODDelete/Delete?id=' + $scope.employeeInformation.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.OdList = [];
                $scope.getData($window.plantId, $scope.employeeInformation.EmpSystemId);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

}