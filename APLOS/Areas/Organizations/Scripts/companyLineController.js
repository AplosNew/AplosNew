'use strict';
CompanyLineController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanyLineController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companyLineList = [];
    $scope.path = 'Organizations/Line/getlistwithcompany';
    $scope.getLineMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companyline/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companyLineList = response.data.Rows;
            if ($scope.companyLineList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.companyLine = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        LineId: null,
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

    //LineList for modal
    $scope.ShowLineList = function () {
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
        if ($scope.companyLine.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/Line/getlistlinewithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.lineListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companyLineList, function (item) {
                        for (var i = 0; i < $scope.lineListWithCompanyWise.length; i++) {
                            if ($scope.lineListWithCompanyWise[i]['LineId'] == item.LineId) {
                                $scope.lineListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#linePopUp')).modal('show');
        $scope.getData();
    };
    //End LineList for modal
    //Passing Data For Line List
    $scope.LineSelectdCloseListPopUp = function () {
        angular.forEach($scope.lineListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companyLineList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companyLine.CompanyId,
                        LineId: item.LineId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#linePopUp')).modal('hide');
        if ($scope.companyLineList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.lineSelectedList = [];
        if ($scope.companyLineList.length > 0) {
            angular.forEach($scope.companyLineList, function (item) {
                if (item.Flag) {
                    $scope.lineSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companyline/create',
                    data: { 'CompanyLine': $scope.companyLineList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getLineMasterOnCompanyChange($scope.companyLine.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Line.", 'failure');
        }
    };
    //Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, LineId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.LineId = LineId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteLineList = function () {
        for (var i = 0; i < $scope.companyLineList.length; i++) {
            if ($scope.companyLineList[i].Id == null && $scope.companyLineList[i].LineId == $scope.LineId) {
                $scope.companyLineList.splice($scope.index, 1);
            }
            else if ($scope.companyLineList[i].Id != null && $scope.companyLineList[i].LineId == $scope.LineId)
                $scope.companyLineList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.LineId = null;
        if ($scope.companyLineList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
}