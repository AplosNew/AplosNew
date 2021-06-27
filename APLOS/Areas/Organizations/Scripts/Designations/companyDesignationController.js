'use strict';
CompanyDesignationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function CompanyDesignationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.tableShow = false;
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.desMstCompanyWiseList = [];
    $scope.designationMasters = [];
    $scope.getDesMstComListUrl = 'Organizations/CompanyDesignation/getlist/';
    $scope.getDesignationMstListUrl = 'Organizations/designationmaster/getlistforcomdesignation/';
    $scope.onCompanyGroupChange = function (companyId) {
        baseService.init($scope.getDesMstComListUrl, null, null, null, 'Id', null);
        $scope.getData = function (pageno) {
            $rootScope.parameters.companyId = $scope.desMstCompanyWise.CompanyId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.desMstCompanyWiseList = result.Rows;
                    if ($scope.desMstCompanyWiseList.length > 0)
                        $scope.tableShow = true;
                    else
                        $scope.tableShow = false;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }
    $scope.desMstCompanyWise = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DesignationMasterId: null
    };
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'DesignationName',
        searchBy: "DesignationName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ShowDesMasterListPopUp = function () {
        if ($scope.desMstCompanyWise.CompanyId === null) {
            return ShowResult('Please at first select company', 'failure');
        }
        $scope.popUpUrl = 'Organizations/designationmaster/getlistforcomdesignation/companyId=' + $scope.desMstCompanyWise.CompanyId;
        baseService.setCurrentPage('designationMasters');
        $scope.getDesMasterData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    angular.forEach($scope.desMstCompanyWiseList, function (element, i) {
                        for (var ii = 0; ii < result.Rows.length; ii++) {
                            if (result.Rows[ii]['DesignationMasterId'] === element.DesignationMasterId)
                                result.Rows.splice(ii, 1);
                        }
                    });
                    $scope.designationMasters = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'divisionPopUp');
                }).finally(function () {
                });
        };
        $scope.getDesMasterData();
        angular.element(document.querySelector('#divisionPopUp')).modal('show');
    }
    $scope.searchDesignationMstByList = [
        {
            'name': 'Designation Group',
            'value': 'DesignationGroupName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.closeDesMstListPopUp = function () {
        angular.element(document.querySelector('#divisionPopUp')).modal('hide');
    }
    $scope.addDivisionMasterInGrid = function () {
        //$scope.company = document.getElementById("companyId").options[document.getElementById('companyId').selectedIndex].text;
        angular.forEach($scope.designationMasters, function (a) {
            if (a.Flag) {
                $scope.desMstCompanyWiseList.push({
                    Id: null,
                    CompanyId: $scope.desMstCompanyWise.CompanyId,
                    DesignationMasterId: a.DesignationMasterId,
                    DesignationMasterName: a.DesignationMasterName,
                    DesignationGroupName: a.DesignationGroupName,
                    DesignationName: a.DesignationName,
                    EmployeeCategoryName: a.EmployeeCategoryName,
                    Code: a.Code,
                    Archive: false
                });
            }
        });
        if (!$scope.tableShow)
            $scope.tableShow = true;
        angular.element(document.querySelector('#divisionPopUp')).modal('hide');
    }
    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'Organizations/CompanyDesignation/create',
            data: { 'designationMaster': $scope.desMstCompanyWiseList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.onCompanyGroupChange($scope.desMstCompanyWise.CompanyId);
            }
        });
        return true;
    }

    $scope.valuePassInDelModal = function (id, designationMasterId, index) {
        $scope.id = id;
        $scope.index = index;
        $scope.designationMasterId = designationMasterId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data?';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.desMstCompanyWiseList.length; i++) {
            if ($scope.desMstCompanyWiseList[i].Id === null && $scope.desMstCompanyWiseList[i].DesignationMasterId === $scope.designationMasterId) {
                $scope.desMstCompanyWiseList.splice(i, 1);
            }
            else if ($scope.desMstCompanyWiseList[i].Id !== null && $scope.desMstCompanyWiseList[i].DesignationMasterId === $scope.designationMasterId)
                $scope.desMstCompanyWiseList[i].Archive = true;
        }
        if ($scope.desMstCompanyWiseList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
        $scope.id = null;
        $scope.index = -1;
        $scope.designationMasterId = null;
    };
};