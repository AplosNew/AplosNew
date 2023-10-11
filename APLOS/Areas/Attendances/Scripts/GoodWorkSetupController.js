'use strict';
GoodWorkSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function GoodWorkSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'GoodWorkSetup';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWorkSetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'UserCode', name: "UserCode" },  { value: 'UserName', name: "User Name" }, { value: 'Remarks', name: "Remarks" }];

    //for tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        UserCode: null,
        UserName: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmpData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.ModelNew.ResponsiblePersonId = arg.data.SystemId;
        $scope.ModelNew.ResponsiblePerson = arg.data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.ModelNew.ResponsiblePersonId = null;
        $scope.ModelNew.ResponsiblePerson = null;
        $scope.ModelNew.ResponsiblePersonCode = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.entitySearchList = [];
    $scope.entityDataList = [];
    $scope.entitySearch = [];
    $scope.entityUrl = 'Organizations/entity/getlist?companyId=';
    $scope.entityParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.entityPopUp = function () {
        $scope.getEntityData = function (pageno) {
            baseService.paginationBase($scope.entityUrl + $window.companyId, pageno, $scope.entityParameters)
                .then(function (response) {
                    for (var i = 0; i < response.Rows.length; i++) {
                        response.Rows[i].Flag = false;
                    }
                    $scope.entityDataList = response.Rows;
                    $scope.entityParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.entitySearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.entityDataList, $scope.entitySearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#entityPopUp')).modal('show');
        $scope.getEntityData();
    };
    $scope.closeEntityPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.selectedEntityList = [];
    $scope.selectEntityPopUp = function () {
        if (baseService.arrayLength($scope.entityDataList) > 0) {
            angular.forEach($scope.entityDataList, function (a) {
                if (checkExistTempList($scope.selectedEntityList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.selectedEntityList.push({
                            Id: null
                            , EntityId: a.Id
                            , GoodWorkSetupId: $scope.ModelNew.Id
                            , Code: a.Code
                            , UserName: a.UserName
                            , Plant: a.Plant
                            , Division: a.Division
                            , SubDivision: a.SubDivision
                            , Unit: a.Unit
                            , EffectiveDate: a.EffectiveDate
                            , IsProductionEntity: a.IsProductionEntity
                        });
                    }
                }

            });
        }
        else
            $scope.selectedEntityList = [];
        angular.forEach($scope.selectedEntityList, function (a) {
            if (!baseService.valueCheckInList($scope.entityDataList, 'Id', a.ReportingGroupId))
                $scope.selectedEntityList.splice(a, 1);
        });
        $scope.closeEntityPopUp();
    };


    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.Id) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].Id === data.Id) {
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

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EntityId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
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
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}