'use strict';
restController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function restController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Rest';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.rests = [];
    $scope.path = 'humanresource/rest/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl, null, null, null, 'RestDate', 'AttendanceRestDate');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.rests = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Attendance Rest Date',
            'value': 'AttendanceRestDate'
        },
        {
            'name': 'Remarks',
            'value': 'Remarks'
        }];

    $scope.rest = {
        Id: null,
        SectionId: null,
        SubSectionId: null,
        DepartmentId: null,
        AttendanceRestDate: null,
        Remarks: null,
        IsOTEntitle: false,
        RestTypeId:null
    };
    $scope.restDetails = {
        Id: null,
        AttendanceRestId: null,
        PlantId: null,
        EmpSystemId: null
    };

    $scope.restNew = Object.assign({}, $scope.rest);

    $scope.sectionList = [];
    $scope.subSectionList = [];
    $scope.departmentList = [];
    $scope.restTypeList = [];

    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.subSectionList = result;
    });

    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.sectionList = result;
    });

    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });
    cboService.getCboRestType(function (result) {
        $scope.restTypeList = result;
        if (baseService.arrayLength($scope.restTypeList) == 1) {
            $scope.restNew.RestTypeId = $scope.restTypeList[0].Value;
        }
    });

    $scope.popUpList = [];
    $scope.popUpDataList = [];
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

            if (baseService.isUndefinedOrNull($scope.restNew.IsOTEntitle)) {
                $scope.restNew.IsOTEntitle = false;
            }
            if (baseService.isUndefinedOrNull($scope.restNew.AttendanceRestDate)) {
                throw 'Please Select Attendance Rest Date';
            }
            $scope.popUpUrl = 'humanresource/rest/getallemployeelist?sectionId=' + $scope.restNew.SectionId + '&subSectionId=' + $scope.restNew.SubSectionId + '&departmentId=' + $scope.restNew.DepartmentId + '&isOTEntitle=' + $scope.restNew.IsOTEntitle + '&AttendanceRestDate=' + $scope.restNew.AttendanceRestDate;
            $scope.popUpParameters.sort = 'EmployeeCode';
            $scope.popUpParameters.searchBy = 'EmployeeCode';

            if (name === 'EmployeeInfo') {
                $scope.popUpTitle = 'Employee Information';
            }

            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        if (baseService.arrayLength($scope.popUpDataList) > 0) {
                            if (baseService.arrayLength($scope.tempList) !== 0) {
                                for (var i = 0; i < $scope.popUpDataList.length; i++) {
                                    $scope.popUpDataList[i].Active = getActive($scope.tempList, $scope.popUpDataList[i].EmpSystemId);
                                }
                            }
                            $scope.popUpParameters.total_count = result.Total;
                            if (baseService.arrayLength($scope.popUpList) === 0) {
                                baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                            }
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

    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmpSystemId) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].EmpSystemId === data.EmpSystemId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, EmpSystemId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === EmpSystemId) {
                return true;
            }
        }
        return false;
    }

    $scope.CheckAll = function (event) {
        var _isselected = event.target.checked;

        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            $scope.popUpDataList[i].Active = _isselected;
        }

        for (var j = 0; j < baseService.arrayLength($scope.popUpDataList); j++) {
            if (_isselected)
                $scope.tempList.push($scope.popUpDataList[j]);
            else
                for (var k = 0; k < $scope.tempList.length; k++) {
                    if ($scope.tempList[k].EmpSystemId === $scope.popUpDataList[j].EmpSystemId) {
                        $scope.tempList.splice(k, 1);
                        break;
                    }
                }
        }
    };

    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.rest = $scope.rests[$scope.index];
        $scope.restNew = Object.assign({}, $scope.rest);
        $scope.restid = $scope.restNew.Id;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.GetRestDetailsData($scope.restNew.Id);
    };

    $scope.GetRestDetailsData = function (restId) {
        $http({
            method: 'GET',
            url: 'humanresource/rest/getrestdetailsdata?restId=' + restId
        }).then(function successCallback(response) {
            $scope.tempList = response.data;
        });
    };

    $scope.Save = function () {
        try {
            angular.copy($scope.restNew, $scope.rest);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.restNewForm.$valid) {
                if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'rest': $scope.rest, 'restDetails': $scope.tempList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.rests.push(response.data.rest);
                            baseService.paginationAdd();
                            ClearFields();
                            $scope.restNew.Id = null;
                            $scope.tempList = [];
                            $scope.getData();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.restNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.restNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.rests.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.tempList = [];
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
        $scope.Action = 'Save';
        $scope.rest = {};
        $scope.restNew = {};
        $scope.restNew.Id = null;
        $scope.tempList = [];
        $scope.restNew.SectionId = null;
        $scope.restNew.SubSectionId = null;
        $scope.restNew.DepartmentId = null;
        $scope.restNew.IsOTEntitle = false;
    }

    $scope.confirmDelete = function (Id, EmployeeCode, index) {
        $scope.index = index;
        $scope.deleteId = Id;
        $scope.message_confirmation = "Are you sure to permanently delete [" + EmployeeCode + "]? ";
    };

    $scope.DeleteDetail = function () {
        if (baseService.isUndefinedOrNull($scope.deleteId)) {
            $scope.tempList.splice($scope.index, 1);
            $scope.index = -1;
        } else {
            $http({
                method: 'POST',
                url: 'humanresource/rest/deletedetail',
                dataType: 'JSON',
                data: { 'Id': $scope.deleteId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRestDetailsData($scope.restid);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
    };

}