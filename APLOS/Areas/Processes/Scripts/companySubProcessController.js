'use strict';
CompanySubProcessController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function CompanySubProcessController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.companySubProcessList = [];
    $scope.subProcesses = [];
    $scope.getcompanySubProcessListUrl = 'Processes/companySubProcess/getlist/';
    //$scope.getSubProcessListUrl = 'Processes/subprocess/getlistforcompanysubprocess/';
    $scope.companySubProcess = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        SubProcessId: null,
        ProcessId: null
    };
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.processList = [];
    $scope.companyChange = function (companyId) {
        $http({
            method: 'GET',
            url: 'Processes/CompanyProcess/GetCompanyProductionProcessCbo?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    };
    baseService.init($scope.getcompanySubProcessListUrl, null, null, null, 'SubProcessId', 'SubProcessId');
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.companySubProcess.CompanyId;
        $rootScope.parameters.processId = $scope.companySubProcess.ProcessId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.companySubProcessList = result.Rows;
                archiveCheckInList($scope.companySubProcessList);
                if ($scope.companySubProcessList.length > 0)
                    $scope.tableShow = true;
                else
                    $scope.tableShow = false;
                if ($scope.tempList.length > 0) {
                    dataAddInMainList($scope.tempList);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.processChange = function () {
        $scope.tempList = [];
        $scope.tempArchiveList = [];
    }
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ShowCompanySubProcessListPopUp = function () {
        if ($scope.companySubProcess.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        if ($scope.companySubProcess.ProcessId == null) {
            return ShowResult('Please at first select process......', 'failure');
        }
        $scope.popUpUrl = 'Processes/subprocess/getlistforcompanysubprocess/?companyId=' + $scope.companySubProcess.CompanyId + '&processId=' + $scope.companySubProcess.ProcessId
            + '&subProcessIds=' + isProcessIdExistGrid($scope.companySubProcessList);
        $scope.getCompanySubProcessData = function (pageno) {
            $rootScope.parameters.companyId = $scope.companySubProcess.CompanyId;
            $rootScope.parameters.processId = $scope.companySubProcess.ProcessId;
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.subProcesses = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });

            angular.element(document.querySelector('#subProcessPopUp')).modal('show');
        };
        $scope.getCompanySubProcessData();
    };
    function isProcessIdExistGrid(list) {
        $scope.ProcessIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] == false) {
                    $scope.ProcessIds.push(list[i]['SubProcessId']);
                }
            }
        }
        return JSON.stringify($scope.ProcessIds);
    }
    $scope.searchSubProcessByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.closeSubProcessListPopUp = function () {
        angular.element(document.querySelector('#subProcessPopUp')).modal('hide');
    };

    $scope.tempList = [];
    $scope.tempArchiveList = [];

    $scope.addCompanySubProcess = function () {
        angular.forEach($scope.subProcesses, function (a) {
            if (a.Flag) {
                $scope.tempList.push({
                    Id: $scope.createId(),
                    CompanyId: $scope.companySubProcess.CompanyId,
                    Code: a.Code,
                    SubProcessId: a.Id,
                    SubProcessName: a.UserName,
                    SubProcessCategoryName: a.SubProcessCategoryName,
                    ProcessId: $scope.companySubProcess.ProcessId,
                    Archive: false,
                    class: 'new'
                });
            }
        });
        dataAddInMainList($scope.tempList);
        if (!$scope.tableShow)
            $scope.tableShow = true;
        angular.element(document.querySelector('#subProcessPopUp')).modal('hide');
    }
    $scope.createId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };
    function dataAddInMainList(list) {
        for (var i = 0; i < list.length; i++) {
            $scope.companySubProcessList.push({
                Id: list[i].Id,
                CompanyId: $scope.companySubProcess.CompanyId,
                Code: list[i].Code,
                SubProcessId: list[i].SubProcessId,
                SubProcessName: list[i].SubProcessName,
                SubProcessCategoryName: list[i].SubProcessCategoryName,
                ProcessId: $scope.companySubProcess.ProcessId,
                Archive: false,
                class: 'new'
            });
        }
    }
    $scope.Save = function () {
        if ($scope.companySubProcessList == null) {
            return ShowResult('Please atleast one subprocess......', 'failure');
        }
        $http({
            method: 'POST',
            url: 'Processes/companysubprocess/create',
            data: {
                'companySubProcess': $scope.companySubProcessList
                , 'ids': JSON.stringify($scope.tempArchiveList)
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.tempList = [];
                $scope.tempArchiveList = [];
            }
        });
        return true;
    }

    $scope.valuePassInDelModal = function (data, index) {
        $scope.id = data.Id;
        $scope.index = index;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SubProcessName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.companySubProcessList.length; i++) {
            if ($scope.companySubProcessList[i].Id.startsWith('new') && $scope.companySubProcessList[i].Id === $scope.id) {
                $scope.companySubProcessList.splice($scope.index, 1);
                tempArchive($scope.tempList);
                break;
            }
            else if ($scope.companySubProcessList[i].Id == $scope.id) {
                $scope.tempArchiveList.push($scope.companySubProcessList[i].Id);
                $scope.companySubProcessList.splice($scope.index, 1);
                break;
            }
        }
        $scope.id = '';
        $scope.index = -1;
        if ($scope.companySubProcessList.length > 0)
            $scope.tableShow = true;
        else
            $scope.tableShow = false;
    };
    function tempArchive(list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === $scope.id) {
                list.splice($scope.index, 1);
                break;
            }
        }
    }

    function archiveCheckInList(mainList) {
        if ($scope.tempArchiveList.length > 0) {
            var dataIndex = -1;
            //for (var i = 0; i < mainList.length; i++) {
            //    dataIndex = $scope.tempArchiveList.indexOf(mainList[i].Id);
            //    if (dataIndex !== -1) {
            //        mainList.splice(i, 1);
            //    }
            //}
            for (var i = mainList.length - 1; i >= 0; i--) {
                dataIndex = $scope.tempArchiveList.indexOf(mainList[i].Id);
                if (dataIndex !== -1) {
                    mainList.splice(i, 1);
                }
            }
        }
    }
}