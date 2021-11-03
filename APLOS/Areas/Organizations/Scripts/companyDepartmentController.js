'use strict';
CompanyDepartmentController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyDepartmentController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companyDepartmentList = [];
    $scope.path = 'Organizations/Department/getlistwithcompany';
    $scope.getDepartmentMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companydepartment/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companyDepartmentList = response.data.Rows;
            if ($scope.companyDepartmentList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };

    $scope.companyDepartment = {
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
    $scope.ShowDepartmentList = function () {
        $scope.searchByList = [
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'User Name',
                'value': 'UserName'
            }
        ];
        if ($scope.companyDepartment.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/Department/getlistdepartmentwithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.departmentListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companyDepartmentList, function (item) {
                        for (var i = 0; i < $scope.departmentListWithCompanyWise.length; i++) {
                            if ($scope.departmentListWithCompanyWise[i]['DepartmentId'] == item.DepartmentId) {
                                $scope.departmentListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#departmentPopUp')).modal('show');
        $scope.getData();
    };
    //End DepartmentList for modal
    //Passing Data For Department List
    $scope.DepartmentSelectdCloseListPopUp = function () {
        angular.forEach($scope.departmentListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companyDepartmentList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companyDepartment.CompanyId,
                        DepartmentId: item.DepartmentId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#departmentPopUp')).modal('hide');
        if ($scope.companyDepartmentList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.departmentSelectedList = [];
        if ($scope.companyDepartmentList.length > 0) {
            angular.forEach($scope.companyDepartmentList, function (item) {
                if (item.Flag) {
                    $scope.departmentSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companydepartment/create',
                    data: { 'CompanyDepartment': $scope.companyDepartmentList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDepartmentMasterOnCompanyChange($scope.companyDepartment.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Department.", 'failure');
        }
    };
    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, DepartmentId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.DepartmentId = DepartmentId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteDepartmentList = function () {
        for (var i = 0; i < $scope.companyDepartmentList.length; i++) {
            if ($scope.companyDepartmentList[i].Id == null && $scope.companyDepartmentList[i].DepartmentId == $scope.DepartmentId) {
                $scope.companyDepartmentList.splice($scope.index, 1);
            }
            else if ($scope.companyDepartmentList[i].Id != null && $scope.companyDepartmentList[i].DepartmentId == $scope.DepartmentId)
                $scope.companyDepartmentList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.DepartmentId = null;
        if ($scope.companyDepartmentList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
}