'use strict';
CompanySubDivisionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanySubDivisionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companySubDivisionList = [];
    $scope.path = 'Organizations/SubDivision/getlistwithcompany';
    $scope.getSubDivisionMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companysubDivision/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companySubDivisionList = response.data.Rows;
            if ($scope.companySubDivisionList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.companySubDivision = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        SubDivisionId: null,
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

    //SubDivisionList for modal
    $scope.ShowSubDivisionList = function () {
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
        if ($scope.companySubDivision.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/SubDivision/getlistsubdivisionwithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.subDivisionListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companySubDivisionList, function (item) {
                        for (var i = 0; i < $scope.subDivisionListWithCompanyWise.length; i++) {
                            if ($scope.subDivisionListWithCompanyWise[i]['SubDivisionId'] == item.SubDivisionId) {
                                $scope.subDivisionListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#subDivisionPopUp')).modal('show');
        $scope.getData();
    };
    //End SubDivisionList for modal
    //Passing Data For SubDivision List
    $scope.SubDivisionSelectdCloseListPopUp = function () {
        angular.forEach($scope.subDivisionListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companySubDivisionList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companySubDivision.CompanyId,
                        SubDivisionId: item.SubDivisionId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#subDivisionPopUp')).modal('hide');
        if ($scope.companySubDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.subDivisionSelectedList = [];
        if ($scope.companySubDivisionList.length > 0) {
            angular.forEach($scope.companySubDivisionList, function (item) {
                if (item.Flag) {
                    $scope.subDivisionSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companysubDivision/create',
                    data: { 'CompanySubDivision': $scope.companySubDivisionList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSubDivisionMasterOnCompanyChange($scope.companySubDivision.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Subdivision.", 'failure');
        }
    };
    //Deleting Rows from CompanySubDivisionList
    $scope.valuePassInDelModal = function (index, SubDivisionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.SubDivisionId = SubDivisionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteSubDivisionList = function () {
        for (var i = 0; i < $scope.companySubDivisionList.length; i++) {
            if ($scope.companySubDivisionList[i].Id == null && $scope.companySubDivisionList[i].SubDivisionId == $scope.SubDivisionId) {
                $scope.companySubDivisionList.splice($scope.index, 1);
            }
            else if ($scope.companySubDivisionList[i].Id != null && $scope.companySubDivisionList[i].SubDivisionId == $scope.SubDivisionId)
                $scope.companySubDivisionList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.SubDivisionId = null;
        if ($scope.companySubDivisionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
}