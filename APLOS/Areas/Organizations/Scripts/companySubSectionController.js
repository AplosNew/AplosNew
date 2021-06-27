'use strict';
CompanySubSectionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanySubSectionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companySubSectionList = [];
    $scope.path = 'Organizations/SubSection/getlistwithcompany';
    $scope.getSubSectionMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companysubSection/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companySubSectionList = response.data.Rows;
            if ($scope.companySubSectionList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.companySubSection = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        SubSectionId: null,
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

    //SubSectionList for modal
    $scope.ShowSubSectionList = function () {
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
        if ($scope.companySubSection.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/SubSection/getlistsubSectionwithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.subSectionListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companySubSectionList, function (item) {
                        for (var i = 0; i < $scope.subSectionListWithCompanyWise.length; i++) {
                            if ($scope.subSectionListWithCompanyWise[i]['SubSectionId'] == item.SubSectionId) {
                                $scope.subSectionListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#subSectionPopUp')).modal('show');
        $scope.getData();
    };
    //End SubSectionList for modal
    //Passing Data For SubSection List
    $scope.SubSectionSelectdCloseListPopUp = function () {
        angular.forEach($scope.subSectionListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companySubSectionList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companySubSection.CompanyId,
                        SubSectionId: item.SubSectionId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#subSectionPopUp')).modal('hide');
        if ($scope.companySubSectionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.subSectionSelectedList = [];
        if ($scope.companySubSectionList.length > 0) {
            angular.forEach($scope.companySubSectionList, function (item) {
                if (item.Flag) {
                    $scope.subSectionSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companysubSection/create',
                    data: { 'CompanySubSection': $scope.companySubSectionList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSubSectionMasterOnCompanyChange($scope.companySubSection.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any SubSection.", 'failure');
        }
    };
    //Deleting Rows from CompanySubSectionList
    $scope.valuePassInDelModal = function (index, SubSectionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.SubSectionId = SubSectionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteSubSectionList = function () {
        for (var i = 0; i < $scope.companySubSectionList.length; i++) {
            if ($scope.companySubSectionList[i].Id == null && $scope.companySubSectionList[i].SubSectionId == $scope.SubSectionId) {
                $scope.companySubSectionList.splice($scope.index, 1);
            }
            else if ($scope.companySubSectionList[i].Id != null && $scope.companySubSectionList[i].SubSectionId == $scope.SubSectionId)
                $scope.companySubSectionList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.SubSectionId = null;
        if ($scope.companySubSectionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
}