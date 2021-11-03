'use strict';
CompanyProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function CompanyProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companyProcessList = [];
    $scope.processes = [];
    $scope.getCompanyProcessListUrl = 'Processes/companyProcess/getlist?companyId=';
    //$scope.getProcessListUrl = ;
    $scope.getData = function () {
        $rootScope.tempList = [];
        $scope.companyProcessList = [];
        $http.get($scope.getCompanyProcessListUrl + $scope.companyProcess.CompanyId)
            .then(function (response) {
                $scope.companyProcessList = response.data.Rows;
            });
    }

    $scope.companyProcess = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        ProcessId: null
    };
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });


    //#region Process
    $scope.searchProcessByList = [
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
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.processPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.companyProcess.CompanyId))
            return ShowResult('Please at first select company.', 'failure');

        $rootScope.tempList = [];
        angular.forEach($scope.companyProcessList, function (a) {
            $rootScope.tempList.push({
                Id: a.ProcessId
                , Sequence: a.Sequence
                , Code: a.Code
                , ShortName: a.ShortName
                , StandardName: a.StandardName
                , UserName: a.UserName
                , Active: a.Active
            });
        });
        baseService.setCurrentPage('processes');
        $scope.getCompanyProcessData = function (pageno) {
            $scope.getProcessUrl = 'processes/process/GetList?processid=[]';// + baseService.getColumnValueList($scope.companyProcessList, 'ProcessId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.processes = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.processes); t++) {
                        $scope.processes[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processes[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getCompanyProcessData();
    };

    $scope.CloseProcessPopUp = function () {
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.addProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.companyProcessList, 'ProcessId', a.Id)) {
                    $scope.companyProcessList.push({
                        Id: null
                        , CompanyId: $scope.companyProcess.CompanyId
                        , ProcessId: a.Id
                        , Code: a.Code
                        , ProcessName: a.UserName
                        , StandardName: a.StandardName
                        , ShortName: a.ShortName
                        , class: 'new'
                    });
                }
            });
        }
        else
            $scope.companyProcessList = [];
        angular.forEach($scope.companyProcessList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ProcessId))
                $scope.companyProcessList.splice(a, 1);
        });
        $scope.CloseProcessPopUp();
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure want to delete [" + name + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t][$scope.tempId] === $scope[$scope.listName][$scope.popUpIndex][$scope.listId])
                $rootScope.tempList.splice(t, 1);
        }
        $scope[$scope.listName].splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    //#endregion Process

    $scope.Save = function () {
        $http({
            method: 'POST'
            , url: 'Processes/companyprocess/create'
            , data: $scope.companyProcessList
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        });
        return true;
    };

}