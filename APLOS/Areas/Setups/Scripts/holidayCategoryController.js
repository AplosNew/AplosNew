'use strict';
HolidayCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function HolidayCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Holiday Category';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.holidayCategorys = [];
    $scope.path = 'Setups/HolidayCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.companyGroupList = [];

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.holidayCategory = {
        Id: null,
        CompanyGroupId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.holidayCategoryNew = Object.assign({}, $scope.holidayCategory);
    //var url = 'Setups/HolidayCategory/getlist?companyGroupId=' + $scope.holidayCategoryNew.CompanyGroupId;
    //baseService.init(url, null, null, null, "UserName", "UserName");
    //$scope.getData = function (pageno) {
    //    baseService.pagination(pageno)
    //        .then(function (result) {
    //            $scope.holidayCategorys = result.Rows;
    //        }, function () {
    //            ShowResult(commonMessage.NetworkError, 'failure');
    //        }).finally(function () {
    //        });
    //};

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.holidayParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getData = function () {
        try {
            $scope.GLUrl3 = 'Setups/HolidayCategory/getlist?companyGroupId=' + $scope.holidayCategoryNew.CompanyGroupId,
                $scope.LoadDataList = function (pageno) {
                    baseService.paginationBase($scope.GLUrl3, pageno, $scope.holidayParameters)
                        .then(function (data) {
                            $scope.holidayCategorys = data.Rows;
                            $scope.holidayParameters.total_count = data.Total;
                        }, function () {
                            ShowResult(commonMessage.NetworkError, 'failure');
                        }).finally(function () {
                        });
                };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.holidayCategoryNew.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.holidayCategory = $scope.holidayCategorys[$scope.index];
        $scope.holidayCategoryNew = Object.assign({}, $scope.holidayCategory);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        angular.copy($scope.holidayCategoryNew, $scope.holidayCategory);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.holidayCategoryNewForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.holidayCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.holidayCategorys.push(response.data.HolidayCategory);
                        $scope.holidayCategorys = $filter('orderBy')($scope.holidayCategorys, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.holidayCategory,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.holidayCategorys[$scope.index] = $scope.holidayCategory;
                            $scope.holidayCategorys = $filter('orderBy')($scope.holidayCategorys, 'Sequence');
                        }
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.holidayCategoryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.holidayCategoryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.holidayCategorys.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.holidayCategory = { 'CompanyGroupId': $scope.holidayCategoryNew.CompanyGroupId };
        $scope.holidayCategoryNew = { 'CompanyGroupId': $scope.holidayCategoryNew.CompanyGroupId };
        $scope.holidayCategoryNew.Sequence = seq;
    }
};