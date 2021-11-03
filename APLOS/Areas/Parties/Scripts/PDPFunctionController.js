'use strict';
function PDPFunctionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'PDPFunction';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.pDPFunctions = [];
    $scope.path = 'Parties/pdpfunction/';
    $scope.getListUrl = $scope.path + 'getpdpfunctionlistbypdpid';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.getData = function (pageno) {
        $rootScope.parameters.partnerDeterminationProcedureId = $scope.pDPFunction.PartnerDeterminationProcedureId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.pDPFunctions = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.pDPFunction = {
        Id: null,
        PartnerDeterminationProcedureId: null,
        PartnerFunctionId: null,
        IsMandatory: true,
        IsModifiable: true,
        IsDefaultValue: true,
        Active: true
    };

    $scope.searchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'PF Name',
            'value': 'PFName'
        },
        {
            'name': 'Account Type',
            'value': 'AccountType'
        }];

    // #region Get
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.pDPFunction = $scope.pDPFunctions[$scope.index];
        $scope.pDPFunction.AddedDate = $filter('dateFilter')($scope.pDPFunction.AddedDate);
        $scope.pDPFunction.UpdatedDate = $filter('dateFilter')($scope.pDPFunction.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    // #endregion

    // #region partnerDetarminationProcedureList
    $scope.partnerDetarminationProcedureList = [];
    $scope.getPartnerDetarminationProcedure = function () {
        $http.get('Parties/partnerdeterminationprocedure/getcbo/')
            .then(function (response) {
                $scope.partnerDetarminationProcedureList = response.data;
            });
    };
    $scope.getPartnerDetarminationProcedure();
    // #endregion

    // #region PartnerFunctionModalData
    $scope.searchpartnerFunctionList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'PF Name',
            'value': 'PFName'
        },
        {
            'name': 'Account Type',
            'value': 'AccountType'
        }];
    $scope.partnerfunctionparameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "AccountType",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getPartnerFunctionList = function () {
        try {
            if ($scope.pDPFunction.PartnerDeterminationProcedureId == null || $scope.pDPFunction.PartnerDeterminationProcedureId == '') {
                throw 'Please First Select Partner Determination Procedure !!!';
            }
            baseService.paginationBase('Parties/partnerfunction/getpartnerfunctionlist', 1, $scope.partnerfunctionparameters)
                .then(function (result) {
                    $scope.partnerFunctionList = result.Rows;
                    for (var i = 0; i < $scope.partnerFunctionList.length; i++) {
                        var obj = angular.copy($scope.partnerFunctionList[i]);
                        for (var s = 0; s < $scope.partnerFunctionDataList.length; s++) {
                            if ($scope.partnerFunctionDataList[s].Code == obj.Code) {
                                $scope.partnerFunctionList[i].Active = true;
                            }
                        }
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            angular.element(document.querySelector('#PartnerFunctionPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.partnerFunctionDataList = [];
    $scope.closePartnerFunctionPopUp = function () {
        for (var i = 0; i < $scope.partnerFunctionList.length; i++) {
            if ($scope.partnerFunctionList[i].Active == true) {
                var obj = angular.copy($scope.partnerFunctionList[i]);
                var has = false;
                for (var j = 0; j < $scope.partnerFunctionDataList.length; j++) {
                    if ($scope.partnerFunctionDataList[j].Code == obj.Code) {
                        has = true;
                        break;
                    }
                }
                if (has == false)
                    $scope.partnerFunctionDataList.push(obj);
                $scope.partnerFunctionList.slice(i, 1);
            }
        }
        angular.element(document.querySelector('#PartnerFunctionPopUp')).modal('hide');
    };

    $scope.getPDPFunction = function (id) {
        $http.get('Parties/pdpfunction/getpdpfunctionlistbypdpid?partnerDeterminationProcedureId=' + id)
            .then(function (response) {
                $scope.partnerFunctionDataList = response.data.Rows;
            });
    };

    // #endregion

    // #region Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.newList = [];
        angular.forEach($scope.partnerFunctionDataList, function (item) {
            $scope.newList.push(
                {
                    Id: item.Id,
                    PartnerDeterminationProcedureId: $scope.pDPFunction.PartnerDeterminationProcedureId,
                    PartnerFunctionId: item.PartnerFunctionId,
                    IsMandatory: item.IsMandatory,
                    IsModifiable: item.IsModifiable,
                    IsDefaultValue: item.IsDefaultValue,
                    Active: true
                }
            );
        });
        if ($scope.pDPFunctionForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'pDPFunction': $scope.newList, 'PartnerDeterminationProcedureId': $scope.pDPFunction.PartnerDeterminationProcedureId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };
    // #endregion

    // #region Delete

    $scope.isArchive = function (archive) {
        if (archive) {
            return false;
        }
        else {
            return true;
        }
    };

    $scope.DeleteFunction = function () {
        try {
            if ($scope.pDPFunction.Id == null || $scope.pDPFunction.Id == '') {
                $scope.partnerFunctionDataList.splice($scope.PIndex, 1);
                $scope.PIndex = -1;
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.deleteUrl,
                    dataType: 'JSON',
                    data: { 'id': $scope.pDPFunction.Id }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.partnerFunctionDataList.splice($scope.PIndex, 1);
                        $scope.PIndex = -1;
                        angular.element(document.querySelector('#confirmdelete')).modal('hide');
                        deletDel($scope.pDPFunction.Id, $scope.partnerFunctionDataList);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.PIndex = -1;
    $scope.valuePassInDelModal = function (index, id, PFName) {
        $scope.PIndex = index;
        $scope.pDPFunction.Id = id;
        $scope.pDPFunction.PFName = PFName;
        $scope.message_confirmation = 'Are you sure to delete [ ' + PFName + ' ]';
        angular.element(document.querySelector('#confirmdelete')).modal('show');
    };

    function deletDel(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Code == id) {
                list.splice(i, 1);
            }
        }
    }
    $scope.removeRow = function () {
        angular.element(document.querySelector('#confirmdelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.pDPFunction.Id)) {
            for (var i = 0; i < $scope.partnerFunctionDataList.length; i++) {
                if ($scope.pDPFunction.PFName == $scope.partnerFunctionDataList[i].PFName) {
                    $scope.partnerFunctionDataList.splice(i, 1);
                }
            }
            deletDel($scope.pDPFunction.Id, $scope.partnerFunctionDataList);
        }
        else {
            $scope.DeleteFunction();
        }
    };

    // #endregion

    // #region Clear
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.PartnerDeterminationProcedureId = $scope.pDPFunction.PartnerDeterminationProcedureId;
        $scope.pDPFunction = {};
        $scope.pDPFunction.PartnerDeterminationProcedureId = $scope.PartnerDeterminationProcedureId;
        $scope.pDPFunction.Active = true;
    }
    // #endregion
}
PDPFunctionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];