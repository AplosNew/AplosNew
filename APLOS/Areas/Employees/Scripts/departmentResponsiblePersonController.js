'use strict';
departmentResponsiblePersonController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function departmentResponsiblePersonController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Department Responsible Person';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.budgetApprovalPersons = [];
    $scope.path = 'employees/departmentResponsiblePerson/';
    $scope.getListUrl = $scope.path + 'getdepartmentresponsiblepersonlist';
    $scope.saveUrl = $scope.path + 'create';

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: 'employees/departmentResponsiblePerson/getdepartmentresponsiblepersonlist?companyId=' + $scope.budgetApprovalPerson.CompanyId + '&entityId=' + $scope.budgetApprovalPerson.EntityId
        }).then(function successCallback(response) {
            $scope.budgetApprovalPersons = response.data;
        });
    };

    $scope.budgetApprovalPerson = {
        Id: null,
        AnnualBudgetId: null,
        PositionId: null,
        ManpowerBudgetId: null,
        EmployeeId: null,
        ApprovalLevel: null,
        SourceType: null,
        Active: null,
        Archive: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,
        BudgetMasterId: null,
        DepartmentId: null,
        EntityId: null,
        CompanyId: null,
        PlantId: null
    };

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });

    $scope.entityList = [];
    $scope.getEntityWithChange = function () {
        cboService.getCboEntityCompanyWise(null, $scope.budgetApprovalPerson.CompanyId, function (result) {
            $scope.entityList = result;
        });
    };

    // #region  Dynamic PopUp
    $scope.popUpList = [];
    $scope.index = -1;
    $scope.popUp = function (name, index) {
        $scope.idx = index;
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

            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatabycompany?companyId=' + $scope.budgetApprovalPerson.CompanyId;
            $scope.popUpParameters.sort = 'EmployeeName';
            $scope.popUpParameters.searchBy = 'EmployeeName';

            if (name === 'EmployeeInfo') {
                $scope.popUpTitle = 'Employee Information';
            }

            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
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

    $scope.selectdblClick = function (data) {
        //if (checkExistTempList($scope.budgetApprovalPersons, data.SystemId) === false) {
        //    $scope.budgetApprovalPersons[$scope.idx].EmployeeId = data.SystemId;
        //    $scope.budgetApprovalPersons[$scope.idx].EmployeeName = data.EmployeeName;
        //}

        $scope.budgetApprovalPersons[$scope.idx].EmployeeId = data.SystemId;
        $scope.budgetApprovalPersons[$scope.idx].EmployeeName = data.EmployeeName;

        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    //function checkExistTempList(list, EmpSystemId) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].EmpSystemId === EmpSystemId) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    $scope.valueData = '';

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
    // #endregion

    $scope.clearEmployeeInfo = function () {
        $scope.budgetApprovalPersons[$scope.idx].EmployeeId = null;
        $scope.budgetApprovalPersons[$scope.idx].EmployeeName = null;
    };

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.budgetApprovalPerson.CompanyId)) {
                throw "Company is required";
            }
            if (baseService.isUndefinedOrNull($scope.budgetApprovalPerson.EntityId)) {
                throw "Entity is required";
            }
            for (var i = 0; i < $scope.budgetApprovalPersons.length; i++) {
                $scope.budgetApprovalPersons[i].SourceType = 'Department';
                $scope.budgetApprovalPersons[i].CompanyId = $scope.budgetApprovalPerson.CompanyId;
                $scope.budgetApprovalPersons[i].EntityId = $scope.budgetApprovalPerson.EntityId;
                $scope.budgetApprovalPersons[i].DepartmentId = $scope.budgetApprovalPersons[i].DPTId;
            }
            if ($scope.Action === 'Save' && $scope.budgetApprovalPersons.length > 0) {
                $http({
                    method: 'post',
                    url: $scope.saveUrl,
                    data: $scope.budgetApprovalPersons,
                    dataType: 'json'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                };
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}