'use strict';
CompanyDivisionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyDivisionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companyDivisionList = [];
    $scope.path = 'Organizations/Division/getlistwithcompany';
    $scope.getDivisionMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companydivision/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companyDivisionList = response.data.Rows;
            if ($scope.companyDivisionList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.companyDivision = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DivisionId: null,
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

    //DivisionList for modal
    $scope.ShowDivisionList = function () {
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
        if ($scope.companyDivision.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/Division/getlistdivisionwithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.divisionListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companyDivisionList, function (item) {
                        for (var i = 0; i < $scope.divisionListWithCompanyWise.length; i++) {
                            if ($scope.divisionListWithCompanyWise[i]['DivisionId'] == item.DivisionId) {
                                $scope.divisionListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#divisionPopUp')).modal('show');
        $scope.getData();
    };
    //End DivisionList for modal
    //Passing Data For Department List
    $scope.DivisionSelectdCloseListPopUp = function () {
        angular.forEach($scope.divisionListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companyDivisionList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companyDivision.CompanyId,
                        DivisionId: item.DivisionId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#divisionPopUp')).modal('hide');
        if ($scope.companyDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.divisionSelectedList = [];
        if ($scope.companyDivisionList.length > 0) {
            angular.forEach($scope.companyDivisionList, function (item) {
                if (item.Flag) {
                    $scope.divisionSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companydivision/create',
                    data: { 'CompanyDivision': $scope.companyDivisionList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDivisionMasterOnCompanyChange($scope.companyDivision.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Division.", 'failure');
        }
    };
    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, DivisionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.DivisionId = DivisionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteDivisionList = function () {
        for (var i = 0; i < $scope.companyDivisionList.length; i++) {
            if ($scope.companyDivisionList[i].Id == null && $scope.companyDivisionList[i].DivisionId == $scope.DivisionId) {
                $scope.companyDivisionList.splice($scope.index, 1);
            }
            else if ($scope.companyDivisionList[i].Id != null && $scope.companyDivisionList[i].DivisionId == $scope.DivisionId)
                $scope.companyDivisionList[i].Archive = true;
        }
        if ($scope.companyDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.DivisionId = null;
    };
    //
}