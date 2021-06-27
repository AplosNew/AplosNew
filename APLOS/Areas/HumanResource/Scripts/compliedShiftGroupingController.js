'use strict';
CompliedShiftGroupingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function CompliedShiftGroupingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = 'Shift Grouping';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.shiftgrouplist = [];
    $scope.path = 'humanresource/compliedShiftGrouping/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.compliedShiftGrouping = {
        Id: null,
        Code: null,
        Description: null,
        PlantId: $window.plantId,
        CompanyGroupId: $window.companyGroupId
    };
    $scope.shiftgrouplist = [];
    $scope.shiftDefinationlist = [];
    $scope.shiftgroupDetailList = [];
    $scope.compliedShiftGroupingNew = Object.assign({}, $scope.compliedShiftGrouping);
    $scope.plantList = [];
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.compliedShiftGroupingNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }

    ];
    $scope.parameters.searchBy = 'Code';
    $scope.getListData = function () {
        baseService.init("humanresource/compliedShiftGrouping/getList?companyGroupId=" + $scope.compliedShiftGroupingNew.CompanyGroupId + "&plantId=" + $scope.compliedShiftGroupingNew.PlantId, null, null, null, "Code", "Code");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.shiftgrouplist = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.getListData();

    //Detail List
    $scope.getShiftGroupDetailList = function () {
        $http.get('HumanResource/compliedShiftGrouping/GetDetailList?compliedShiftGroupId=' + $scope.compliedShiftGroupingNew.Id)
            .then(function (response) {
                $scope.shiftgroupDetailList = response.data;
            });
    };
    $scope.ShowshiftDefinationList = function () {
        $scope.searchByDefinationList = [
            {
                'name': 'Shift Defination Name',
                'value': 'UserName'
            },
            {
                'name': 'In Time',
                'value': 'InTime '
            },
            {
                'name': 'Out Time',
                'value': 'OutTime '
            },
            {
                'name': 'Shift Type ',
                'value': 'ShiftType '
            },
            {
                'name': 'Description ',
                'value': 'ShiftDefinationDescription '
            }
        ];
        $scope.shiftDefinationListParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'UserName',
            searchBy: "UserName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $rootScope.tempList = [];
        angular.forEach($scope.shiftgroupDetailList, function (a) {
            $rootScope.tempList.push({
                SystemID: a.ActualShiftId
                , UserName: a.UserName
                , InTime: a.InTime
                , OutTime: a.OutTime
                , ShiftType: a.ShiftType
            });
        });
        baseService.setCurrentPage('shiftDefinationlist');
        $scope.getShift = function (pageno) {
            baseService.paginationBase('HumanResource/compliedShiftGrouping/QueryshiftDefination?groupId=' + $scope.compliedShiftGrouping.CompanyGroupId + '&plantId=' + $scope.compliedShiftGrouping.PlantId, pageno, $scope.shiftDefinationListParameters)
                .then(function (result) {
                    $scope.shiftDefinationlist = result.Rows;
                    $scope.shiftDefinationListParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.shiftDefinationlist); t++) {
                        $scope.shiftDefinationlist[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'SystemID', $scope.shiftDefinationlist[t].SystemID);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#shiftdefinationPopUp')).modal('show');
        $scope.getShift();
    };
    $scope.SelectdShiftDefinationCloseListPopUp = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (item) {
                if (!baseService.valueCheckInList($scope.shiftgroupDetailList, 'ActualShiftId', item.SystemID)) {
                    $scope.shiftgroupDetailList.push({
                        Id: null
                        , CompliedShiftGroupingId: null
                        , ActualShiftId: item.SystemID
                        , UserName: item.UserName
                        , InTime: item.InTime
                        , OutTime: item.OutTime
                        , ShiftType: item.ShiftType
                        , ShiftDefinationDescription: item.ShiftDefinationDescription
                    });
                }
            });
        }
        else
            $scope.shiftgroupDetailList = [];
        angular.forEach($scope.shiftgroupDetailList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'SystemID', a.ActualShiftId))
                $scope.shiftgroupDetailList.splice(a, 1);
        });
        angular.element(document.querySelector('#shiftdefinationPopUp')).modal('hide');
    };
    function checkExist(id) {
        for (var i = 0; i < $scope.shiftgroupDetailList.length; i++) {
            var ob = $scope.shiftgroupDetailList[i];
            if (ob.ActualShiftId === id) {
                return true;
                break;
            }
        }
        return false;
    }
    //Save
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.compliedShiftGrouping = $scope.shiftgrouplist[$scope.index];
        $scope.compliedShiftGroupingNew = Object.assign({}, $scope.compliedShiftGrouping);
        $scope.getShiftGroupDetailList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.compliedShiftGroupingNew, $scope.compliedShiftGrouping);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.compliedShiftGroupingNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'compliedShiftGrouping': $scope.compliedShiftGrouping, 'details': $scope.shiftgroupDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getListData();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'compliedShiftGrouping': $scope.compliedShiftGrouping, 'details': $scope.shiftgroupDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getListData();
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.compliedShiftGroupingNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.compliedShiftGroupingNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.shiftgrouplist.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to permanently delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope[$scope.listName][$scope.popUpIndex].Id)) {
            $http({
                method: 'POST'
                , url: 'HumanResource/CompliedShiftGrouping/DeleteDetails?id=' + $scope[$scope.listName][$scope.popUpIndex].Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true)
                    ShowResult(response.data.Message, "failure");
                else {
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    //
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.compliedShiftGrouping = { PlantId: $scope.compliedShiftGroupingNew.PlantId, CompanyGroupId: $scope.compliedShiftGroupingNew.CompanyGroupId };
        $scope.compliedShiftGroupingNew = { PlantId: $scope.compliedShiftGroupingNew.PlantId, CompanyGroupId: $scope.compliedShiftGroupingNew.CompanyGroupId };
        $scope.shiftgroupDetailList = [];
    }
}