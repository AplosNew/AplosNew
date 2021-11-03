'use strict';
DesignationBudgetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function DesignationBudgetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Designation Budget';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.designationBudgets = [];
    $scope.path = 'Organizations/DesignationBudget/';
    $scope.getListUrl = $scope.path + 'getlist';

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.getListUrl
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.designationBudgets = response.data;

            }

        });
    };
    $scope.getData();

    $scope.designationBudget = {
        Id: null,
        BudgetCodeId: null,
        Code: null,
        LegalDesignationId: null,
        LegalDesignation: null,
        Activity: null,
        Remarks: null,
        BudgetNo: null,
        Requirement: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.designationBudgetNew = angular.copy($scope.designationBudget);

    $scope.Get = function (obj) {
      
        $scope.designationBudget = obj.data;
        $scope.designationBudgetNew = Object.assign({}, $scope.designationBudget);

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    //#region BudgetCode
    $scope.popUpTitle = "Manpower Budget";
    $scope.name = null;
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.budgetpopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {

        $scope.popUpDataList = [];
        $scope.popUpList = [];
        //$scope.popUpParameters.sort = 'Code';
        //$scope.popUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.budgetpopUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.budgetpopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    //$scope.popUpParameters.sort = 'Code';
                    //$scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        $scope.designationBudgetNew.BudgetCodeId = data.Id;
        $scope.designationBudgetNew.Code = data.Code;

        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.clearCode = function () {
        $scope.designationBudgetNew.BudgetCodeId = null;
        $scope.designationBudgetNew.Code = null;

    };

    //#endregion BudgetCode

    //#region LegalDesignation



    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.searchByUserList = [
        {
            'Text': 'Sequence',
            'Value': 'Sequence'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Short Name',
            'Value': 'ShortName'
        },
        {
            'Text': 'Standard Name',
            'Value': 'StandardName'
        },
        {
            'Text': 'User Name',
            'Value': 'UserName'
        }
    ];

    $scope.flg = null;
    $scope.popUpLD = function (flg) {
        $scope.flg = flg;
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Sequence';
        $scope.popUpParameters.searchBy = 'UserName';
        $scope.popUpUrl = 'employees/RecruitmentApproval/GetLegalDesignationCbo?companyGroupId=' + $window.companyGroupId;
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        for (var i = 0; i < $scope.searchByUserList.length; i++) {
                            $scope.popUpList.push($scope.searchByUserList[i]);
                        }

                    }
                    $scope.popUpParameters.sort = 'Sequence';
                    $scope.popUpParameters.searchBy = 'UserName';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#LDPopUp')).modal('show');
        $scope.getPopUpData();
    };


    $scope.selectLegalDesignationDoubleClick = function (data) {
        $scope.designationBudgetNew.LegalDesignationId = data.Id;
        $scope.designationBudgetNew.LegalDesignation = data.UserName;
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };

    $scope.clearLegalDesignaitonCode = function () {
        $scope.designationBudgetNew.LegalDesignationId = null;
        $scope.designationBudgetNew.LegalDesignation = null;
    };


    //#endregion LegalDesignation

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.designationBudgetNewForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/DesignationBudget/Create',
                    data: { 'data': $scope.designationBudgetNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Action = 'Save';
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else {
                $http({
                    method: 'POST',
                    url: 'Organizations/DesignationBudget/Edit',
                    data: { 'data': $scope.designationBudgetNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.getData();
                        $scope.Action = 'Save';
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
    };

    $scope.message_confirmation = null;
    $scope.removeData = function (obj) {
        $scope.designationBudgetNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.designationBudgetNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.designationBudgetNew.Code + '-' + $scope.designationBudgetNew.LegalDesignation + ' ]';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Organizations/DesignationBudget/DeleteData?id=' + $scope.designationBudgetNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.Clear();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.designationBudget = {};
        $scope.designationBudgetNew = {};
    }


}
