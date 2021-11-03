'use strict';
shiftGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function shiftGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.shiftGroupList = [];
    $scope.path = 'Setups/ShiftGroup/';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.shiftGroup = {
        Id: null,
        PlantId: null,
        JobLocationId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
        $scope.shiftGroupList = [];
    });
    $scope.jobLocationList = [];
    $scope.getJobLocationOnPlantChange = function () {
        cboService.getJobLocationCbo($scope.shiftGroup.PlantId, function (result) {
            $scope.jobLocationList = result;
        });
    };
    $scope.GetSequence = function () {
        $http.get('Setups/ShiftGroup/getautosequence?plantId=' + $scope.shiftGroup.PlantId + '&joblocationId=' + $scope.shiftGroup.JobLocationId)
            .then(function (response) {
                $scope.shiftGroup.Sequence = response.data;
            });
    };
    //SectionList for modal
    $scope.getShiftOnJobLocationChange = function () {
        $scope.shiftGroupList = [];
        $scope.searchByList = [
            {
                'name': 'Sequence',
                'value': 'Sequence'
            },
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Short Name',
                'value': 'ShortName'
            },
            {
                'name': 'Standard Name',
                'value': 'StandardName'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];
        $scope.shiftGroupListParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'UserName',
            searchBy: 'UserName',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        baseService.setCurrentPage('shiftGroupList');
        $scope.getData = function (pageno) {
            baseService.paginationBase('Setups/ShiftGroup/GetList?plantId=' + $scope.shiftGroup.PlantId + '&joblocationId=' + $scope.shiftGroup.JobLocationId, pageno, $scope.shiftGroupListParameters)
                .then(function (data) {
                    $scope.shiftGroupList = data.Rows;
                    $scope.shiftGroupList.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        //baseService.init('Setups/ShiftGroup/GetList?plantId=' + $scope.shiftGroup.PlantId + '&joblocationId=' + $scope.shiftGroup.JobLocationId);
        //$scope.getData = function (pageno) {
        //    baseService.pagination(pageno)
        //        .then(function (result) {
        //            $scope.shiftGroupList = result.Rows;
        //        }, function () {
        //            ShowResult(commonMessage.NetworkError, 'failure');
        //        }).finally(function () {
        //        });
        //};
        $scope.getData();
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.shiftGroup = $scope.shiftGroupList[$scope.index];
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.shiftGroupForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.shiftGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.shiftGroupList.push(response.data.ShiftGroup);
                        $scope.shiftGroupList = $filter('orderBy')($scope.shiftGroupList, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.shiftGroup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.shiftGroupList[$scope.index] = $scope.shiftGroup;
                            $scope.shiftGroupList = $filter('orderBy')($scope.shiftGroupList, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.shiftGroup.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.shiftGroup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.shiftGroupList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.shiftGroup = { PlantId: $scope.shiftGroup.PlantId, JobLocationId: $scope.shiftGroup.JobLocationId };
        $scope.shiftGroup.Sequence = seq;
    }
}