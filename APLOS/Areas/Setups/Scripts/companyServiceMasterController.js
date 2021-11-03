'use strict';
companyServiceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function companyServiceMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Company Service Master";
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.serviceMasterCompanyExtensionList = [];
    $scope.getCompanyServiceMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;

        $scope.getData = function () {
            $http({
                method: 'GET',
                url: 'Setups/CompanyServiceMaster/GetListWithCompany?companyId=' + companyId
            }).then(function successCallback(response) {
                $scope.serviceMasterSelectedList = response.data;
                if ($scope.serviceMasterSelectedList.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            });
        };
        $scope.getData();
    };

    $scope.serviceMasterCompanyExtension = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DepartmentId: null,
        Remarks: null,
        Active: true,
        AddedDate: new Date(),
        UpdatedBy: null,
        UpdatedDate: new Date()
    };

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    //DepartmentList for modal

    $scope.serviceMasterList = [];
    $scope.searchByServiceMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Service Group',
            'value': 'ServiceGroup'
        }
    ];
    $scope.tempList = [];
    $scope.serviceMasterListParameters = {
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
    $scope.serviceMasterSearchPopup = function () {
        if ($scope.serviceMasterCompanyExtension.CompanyId === null)
            return ShowResult('Please at first select company.', 'failure');
        $scope.tempList = [];
       

        baseService.setCurrentPage('serviceMasterList');
        $scope.loadServiceMasterData = function (pageno) {
            //baseService.paginationBase('Setups/serviceMaster/GetList?ids=' + isServiceMasterIdExistGrid($scope.serviceMasterSelectedList), pageno, $scope.serviceMasterListParameters)
            baseService.paginationBase('Setups/serviceMaster/GetServiceMasterList', pageno, $scope.serviceMasterListParameters)
                .then(function (result) {
                    $scope.serviceMasterList = result.Rows;
                    $scope.serviceMasterListParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.serviceMasterList); t++) {
                        $scope.serviceMasterList[t].Flag = baseService.valueCheckInList($scope.tempList, 'Id', $scope.serviceMasterList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadServiceMasterData();
        angular.element(document.querySelector('#serviceMasterListPopUp')).modal('show');
    };

    $scope.serviceMasterCloseListPopUp = function () {
        ServiceMasterSelectedListfun();
        angular.element(document.querySelector('#serviceMasterListPopUp')).modal('hide');
    }
    $scope.serviceMasterSelectedList = [];
    function ServiceMasterSelectedListfun() {
        if (baseService.arrayLength($scope.serviceMasterList) > 0) {
            angular.forEach($scope.serviceMasterList, function (a) {
               
                if (checkExist($scope.serviceMasterSelectedList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.serviceMasterSelectedList.push({
                            Id: null
                            , ServiceMasterId: a.Id
                            , CompanyId: $scope.serviceMasterCompanyExtension.CompanyId
                            , ServiceGroupName: a.ServiceGroupName
                            , Code: a.Code
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                        });
                    }
                }
            });

            //baseService.paginationAdd();
        }
        //else
        //    $scope.serviceMasterSelectedList = [];
        //angular.forEach($scope.serviceMasterSelectedList, function (a) {
        //    if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ServiceMasterId))
        //        $scope.serviceMasterSelectedList.splice(a, 1);
        //    baseService.paginationRemove();
        //});
    }

    function checkExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ServiceMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    function isServiceMasterIdExistGrid(list) {
        $scope.ServiceMasterIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                $scope.ServiceMasterIds.push(list[i]['ServiceMasterId']);
            }
        }
        return JSON.stringify($scope.ServiceMasterIds);
    }

    //End DepartmentList for modal

    function ServiceMasterCDeleteIdList(list) {
        $scope.ServiceMasterCDeleteIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].isDelete) {
                    $scope.ServiceMasterCDeleteIds.push(list[i]['Id']);
                }
            }
        }
        return JSON.stringify($scope.ServiceMasterCDeleteIds);
    }
    $scope.Save = function () {
        $scope.departmentSelectedList = [];
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.Action === 'Save') {
            $http({
                method: 'POST',
                url: 'Setups/CompanyServiceMaster/create',
                data: { 'serviceMasterCompanyExtension': $scope.serviceMasterSelectedList, 'companyId': $scope.serviceMasterCompanyExtension.CompanyId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getCompanyServiceMasterOnCompanyChange($scope.serviceMasterCompanyExtension.CompanyId);
                }
            });
            return true;
        }
    };

    $scope.valuePassInDelModal = function (index, data) {
        $scope.id = data.Id;
        $scope.cIndex = index;
        $scope.message_confirmation = 'Are you sure want to permanently delete [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

   

    $scope.DeleteServiceMasterList = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST',
                url: 'Setups/CompanyServiceMaster/Delete?id=' + $scope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceMasterSelectedList.splice($scope.cIndex, 1);
                    $scope.cIndex = -1;
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } else {
            $scope.serviceMasterSelectedList.splice($scope.cIndex, 1);
            $scope.cIndex = -1;
        }
    };


}