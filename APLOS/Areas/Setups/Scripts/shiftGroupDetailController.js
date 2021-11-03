'use strict';
shiftGroupDetailController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function shiftGroupDetailController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.shiftGroupDetailList = [];
    $scope.path = 'Setups/ShiftGroup/';
    $scope.getUrl = $scope.path + 'getDetailList';
    $scope.saveUrl = $scope.path + 'detailCreate';
    $scope.updateUrl = $scope.path + 'detailEdit';
    $scope.deleteUrl = $scope.path + 'detailDelete/';
    $scope.shiftGroupDetail = {
        Id: null,
        PlantId: null,
        JobLocationId: null,
        ShiftGroupId: null
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
        $scope.shiftGroupDetailList = [];
    });
    $scope.jobLocationList = [];
    $scope.getJobLocationOnPlantChange = function () {
        cboService.getJobLocationCbo($scope.shiftGroupDetail.PlantId, function (result) {
            $scope.jobLocationList = result;
        });
    };
    $scope.shiftGroupCboList = [];
    $scope.getShiftGroupOnLocationChange = function () {
        cboService.getShiftGroupCbo($scope.shiftGroupDetail.PlantId, $scope.shiftGroupDetail.JobLocationId, function (result) {
            $scope.shiftGroupCboList = result;
        });
    };

    //SectionList for modal
    $scope.getShiftOnJobLocationChange = function () {
        $scope.shiftGroupDetailList = [];
        $scope.searchByList = [
            {
                'name': 'User Name',
                'value': 'UserName'
            },
            {
                'name': 'Shift Type',
                'value': 'ShiftType'
            }
        ];
        baseService.init('Setups/ShiftGroup/GetShiftGroupDetailList', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.shiftGroupId = $scope.shiftGroupDetail.ShiftGroupId;
            $rootScope.parameters.plantId = $scope.shiftGroupDetail.PlantId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.shiftGroupDetailList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    //End SectionList for modal
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.shiftGroupDetail = $scope.shiftGroupDetailList[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function getSaveList() {
        $scope.shiftGroupDetailSaveList = [];
        angular.forEach($scope.shiftGroupDetailList, function (item) {
            if (item.Flag) {
                item.ShiftGroupId = $scope.shiftGroupDetail.ShiftGroupId;
                item.ShiftDefinationId = item.SystemID;
                $scope.shiftGroupDetailSaveList.push(item);
            }
        });
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.shiftGroupDetailForm.$valid) {
            if ($scope.Action === "Save") {
                getSaveList();
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.shiftGroupDetailSaveList,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getShiftOnJobLocationChange();
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.shiftGroupDetail.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.shiftGroupDetail.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.shiftGroupDetailList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.shiftGroupDetail = { PlantId: $scope.shiftGroupDetail.PlantId, JobLocationId: $scope.shiftGroupDetail.JobLocationId, ShiftGroupId: $scope.shiftGroupDetail.ShiftGroupId };
    }
}