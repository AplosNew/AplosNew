'use strict';
companyCostCenterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function companyCostCenterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.costCenterCompanyExtensionList = [];
    $scope.getCompanyCostCenterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        baseService.init('Organizations/companyCostCenter/GetListWithCompany?companyId=' + companyId, null, 100, null, "UserName", "UserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.costCenterSelectedList = result.Rows;
                    if ($scope.costCenterSelectedList.length > 0) {
                        $scope.tableShow = true;
                    }
                    else {
                        $scope.tableShow = false;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        //$http({
        //    method: 'GET',
        //    url: 'Organizations/companyCostCenter/GetListWithCompany?companyId=' + companyId
        //}).then(function successCallback(response) {
        //    $scope.costCenterCompanyExtensionList = response.data.Rows;
        //    if ($scope.costCenterCompanyExtensionList.length > 0) {
        //        $scope.tableShow = true;
        //    }
        //    else {
        //        $scope.tableShow = false;
        //    }
        //});
    };

    $scope.companyCostCenter = {
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
    $scope.tempCostCenterList = [];
    $scope.selectCostCenterChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempCostCenterList($scope.tempCostCenterList, data.CostCenterId) === false) {
                    $scope.tempCostCenterList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempCostCenterList.length; i++) {
                    if ($scope.tempCostCenterList[i].CostCenterId === data.CostCenterId) {
                        $scope.tempCostCenterList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempCostCenterList(list, CostCenterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CostCenterId === CostCenterId) {
                return true;
            }
        }
        return false;
    }
    function getCostCenterActive(list, CostCenterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CostCenterId === CostCenterId) {
                return true;
            }
        }
        return false;
    }
    $scope.costCenterList = [];
    $scope.searchByCostCenterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Category',
            'value': 'CostCenterCategoryName'
        },
        {
            'name': 'Sub Category',
            'value': 'CostCenterSubCategoryName'
        }
    ];

    $scope.costCenterListParameters = {
        limit: 20,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 20,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.costCenterSearchPopup = function () {
        $scope.tempCostCenterList = [];
        if ($scope.companyCostCenter.CompanyId === null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.setCurrentPage('costCenterList');
        $scope.loadCostCenterData = function (pageno) {
            baseService.paginationBase('Organizations/CostCenter/GetCostCenterUnSelectedList?costCenterIds=' + isCostCenterIdExistGrid($scope.costCenterSelectedList), pageno, $scope.costCenterListParameters)
                .then(function (result) {
                    $scope.costCenterList = result.Rows;
                    $scope.costCenterListParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.costCenterList.length; i++) {
                        $scope.costCenterList[i].Flag = getCostCenterActive($scope.tempCostCenterList, $scope.costCenterList[i].CostCenterId);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.loadCostCenterData();
        angular.element(document.querySelector('#costCenterListPopUp')).modal('show');
    };

    $scope.costCenterCloseListPopUp = function () {
        CostCenterSelectedListfun();
        angular.element(document.querySelector('#costCenterListPopUp')).modal('hide');
        if ($scope.costCenterSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.costCenterSelectedList = [];
    function CostCenterSelectedListfun() {
        angular.forEach($scope.tempCostCenterList, function (item) {
            if (item.Flag) {
                $scope.costCenterSelectedList.push(
                    {
                        CostCenterId: item.CostCenterId,
                        Id: null,
                        CompanyId: $scope.companyCostCenter.CompanyId,
                        CostCenterCategoryId: item.CostCenterCategoryId,
                        CostCenterSubCategoryId: item.CostCenterSubCategoryId,
                        CostCenterCategoryName: item.CostCenterCategoryName,
                        CostCenterSubCategoryName: item.CostCenterSubCategoryName,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag
                    }
                );
            }
        });
    }

    function isCostCenterIdExistGrid(list) {
        $scope.costCenterIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                $scope.costCenterIds.push(list[i]['CostCenterId']);
            }
        }
        return JSON.stringify($scope.costCenterIds);
    }

    //End DepartmentList for modal

    //Save
    function costCenterCDeleteIdList(list) {
        $scope.costCenterCDeleteIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i].isDelete) {
                    $scope.costCenterCDeleteIds.push(list[i]['Id']);
                }
            }
        }
        return JSON.stringify($scope.costCenterCDeleteIds);
    }
    $scope.Save = function () {
        $scope.departmentSelectedList = [];
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.Action === 'Save') {
            $http({
                method: 'POST',
                url: 'Organizations/companyCostCenter/create',
                data: { 'companyCostCenter': $scope.costCenterSelectedList, 'companyId': $scope.companyCostCenter.CompanyId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getCompanyCostCenterOnCompanyChange($scope.companyCostCenter.CompanyId);
                }
            });
            return true;
        }
    };
    //Deleting Rows from CostCenterCompanyExtensionList
    $scope.valuePassInDelModal = function (index, CostCenterId, id) {
        $scope.id = id;
        $scope.cIndex = index;
        $scope.CostCenterId = CostCenterId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteCostCenterList = function () {
        for (var i = 0; i < $scope.costCenterSelectedList.length; i++) {
            if ($scope.costCenterSelectedList[i].Id === null && $scope.costCenterSelectedList[i].CostCenterId === $scope.CostCenterId) {
                $scope.costCenterSelectedList.splice($scope.cIndex, 1);
            }
            else if ($scope.costCenterSelectedList[i].Id !== null && $scope.costCenterSelectedList[i].CostCenterId === $scope.CostCenterId) {
                $scope.costCenterSelectedList.splice($scope.cIndex, 1);
            }
        }
        $scope.id = null;
        $scope.cIndex = null;
        $scope.CostCenterId = null;
        if ($scope.costCenterSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
}