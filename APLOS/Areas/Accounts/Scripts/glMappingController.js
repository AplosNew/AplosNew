'use strict';
glMappingController.$inject = ['cboService', 'commonMessage', "$window", '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function glMappingController(cboService, commonMessage, $window, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.gLMappingList = [];
    $scope.gLMappingSelectedList = [];
    $scope.path = 'Accounts/GLMapping/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';

    $scope.glMapping = {
        Id: null
        , CompanyGroupId: null
        , CompanyId: null
        , PlantId: null
        , GLGeneralInfoId: null
        , GLGeneralInfoCode: null
        , GLGeneralInfoName: null
        , BudgetMasterId: null
        , BudgetCode: null
        , BudgetName: null
        , ActivityId: null
        , ActivityCode: null
        , ActivityName: null
        , OldGLId: null
        , OldGLCode: null
        , OldGLName: null
        , PartyType: null
    };

    $scope.partyTypeList = [
        {
            'Value': 'Customer'
            , 'Text': 'Customer'
        },
        {
            'Value': 'Vendor'
            , 'Text': 'Vendor'
        }
    ];


    baseService.init('', null, null, null, 'GLGeneralInfoCode', 'GLGeneralInfoCode');
    $scope.getDataList = function () {
        baseService.init('Accounts/GLMapping/GetList?partyType=' + $scope.glMapping.PartyType, null, null, null, 'GLGeneralInfoCode', 'GLGeneralInfoCode');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.glMappingList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.searchByList = [
        {
            'name': 'Old GLId',
            'value': 'OldGLId'
        },
        {
            'name': 'Old GLCode',
            'value': 'OldGLCode'
        },
        {
            'name': 'Old GLName',
            'value': 'OldGLName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Budget Code',
            'value': 'BudgetCode'
        },
        {
            'name': 'Budget Name',
            'value': 'BudgetName'
        },
        {
            'name': 'Activity Code',
            'value': 'ActivityCode'
        },
        {
            'name': 'Activity Name',
            'value': 'ActivityName'
        }
    ];

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.glMapping = $scope.glMappingList[$scope.index];
        $scope.customerInvoiceGLParameters.search = "%" + $scope.glMapping.OldGLName + "%";
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.customerInvoiceGLSearchList = [
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL Code',
            'value': 'GLGeneralInfoCode'
        },
        {
            'name': 'GL Name',
            'value': 'GLGeneralInfoName'
        },
        {
            'name': 'Ref No',
            'value': 'RefNo'
        }
    ];

    $scope.customerInvoiceGLParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function () {
        $scope.customerInvoiceGLList = [];
        baseService.setCurrentPage('customerInvoiceGLList');
        $scope.customerInvoiceGLGLData = function (pageno) {
            baseService.paginationBase('Accounts/GLItem/GetAllGLList', pageno, $scope.customerInvoiceGLParameters)
                .then(function (result) {
                    $scope.customerInvoiceGLList = result.Rows;
                    $scope.customerInvoiceGLParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'CustomerInvoiceGLPopUp');
                }).finally(function () {
                });
        };
        $scope.customerInvoiceGLGLData();
        angular.element(document.querySelector('#CustomerInvoiceGLPopUp')).modal('show');
    };

    $scope.glSelect = function (data) {
        $scope.glMapping.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.glMapping.GLGeneralInfoName = data.GLGeneralInfoCode + ' - ' + data.GLGeneralInfoName;
        $scope.glMapping.BudgetMasterId = data.BudgetMasterId;
        $scope.glMapping.BudgetName = data.BudgetCode + ' - ' + data.BudgetName;
        $scope.glMapping.ActivityId = data.ActivityId;
        $scope.glMapping.ActivityName = data.ActivityCode + ' - ' + data.ActivityName;
        $scope.closeGLPopUp();
    };

    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector('#CustomerInvoiceGLPopUp')).modal('hide');
    };

    //Deleting Rows from GLMappingList
    $scope.valuePassInDelModal = function (index, data) {
        $scope.tempGLMappingOb = data;
        $scope.glMappingIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempGLMappingOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.BudgetName + ' ]';
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('show');
    };

    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempGLMappingOb.Id) === true) {
            $scope.gLMappingSelectedList.splice($scope.glMappingIndex, 1);
        } else {
            $scope.Delete($scope.tempGLMappingOb.Id, $scope.glMappingIndex);
        }
        $scope.glMappingIndex = -1;
        $scope.$scope.tempGLMappingOb.Id = null;
        angular.element(document.querySelector('#confirmDocumentdelete')).modal('hide');
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.partyMappingNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.glMapping,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.glMapping,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.glMappingList.splice(index, 1);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Clear = function () {
        $scope.glMapping = {};
        $scope.Action = 'Save';
    };
}