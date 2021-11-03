'use strict';
CompanySectionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompanySectionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.companySectionList = [];
    $scope.path = 'Organizations/Section/getlistwithcompany';
    $scope.getSectionMasterOnCompanyChange = function (companyId) {
        $scope.SelectedCompany = companyId;
        $http({
            method: 'GET',
            url: 'Organizations/companysection/getlistwithcompany?companyId=' + companyId
        }).then(function successCallback(response) {
            $scope.companySectionList = response.data.Rows;
            if ($scope.companySectionList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    };
    $scope.companySection = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        SectionId: null,
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

    //SectionList for modal
    $scope.ShowSectionList = function () {
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
        if ($scope.companySection.CompanyId == null) {
            return ShowResult('Please at first select company......', 'failure');
        }
        baseService.init('Organizations/Section/getlistsectionwithcompnay', null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.SelectedCompany;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.sectionListWithCompanyWise = result.Rows;
                    angular.forEach($scope.companySectionList, function (item) {
                        for (var i = 0; i < $scope.sectionListWithCompanyWise.length; i++) {
                            if ($scope.sectionListWithCompanyWise[i]['SectionId'] == item.SectionId) {
                                $scope.sectionListWithCompanyWise.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#sectionPopUp')).modal('show');
        $scope.getData();
    };
    //End SectionList for modal
    //Passing Data For Section List
    $scope.SectionSelectdCloseListPopUp = function () {
        angular.forEach($scope.sectionListWithCompanyWise, function (item) {
            if (item.Flag) {
                $scope.companySectionList.push(
                    {
                        Id: null,
                        CompanyId: $scope.companySection.CompanyId,
                        SectionId: item.SectionId,
                        Code: item.Code,
                        UserName: item.UserName,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#sectionPopUp')).modal('hide');
        if ($scope.companySectionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //Save
    $scope.Save = function () {
        $scope.sectionSelectedList = [];
        if ($scope.companySectionList.length > 0) {
            angular.forEach($scope.companySectionList, function (item) {
                if (item.Flag) {
                    $scope.sectionSelectedList.push(item);
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/companysection/create',
                    data: { 'CompanySection': $scope.companySectionList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getSectionMasterOnCompanyChange($scope.companySection.CompanyId);
                    }
                });
                return true;
            }
        } else {
            ShowResult("You have not selected any Section.", 'failure');
        }
    };
    //Deleting Rows from CompanySectionList
    $scope.valuePassInDelModal = function (index, SectionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.SectionId = SectionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteSectionList = function () {
        for (var i = 0; i < $scope.companySectionList.length; i++) {
            if ($scope.companySectionList[i].Id == null && $scope.companySectionList[i].SectionId == $scope.SectionId) {
                $scope.companySectionList.splice($scope.index, 1);
            }
            else if ($scope.companySectionList[i].Id != null && $scope.companySectionList[i].SectionId == $scope.SectionId)
                $scope.companySectionList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.SectionId = null;
        if ($scope.companySectionList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //
}