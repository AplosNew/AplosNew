'use strict';
function MaterialGridController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.charTblShow = false
    $scope.materialgridlist = [];
    $scope.getListUrl = 'Materials/materialgrid/getlist';
    baseService.init($scope.getListUrl, null, null, null, "UserName", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialgridlist = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.materialgrid = {
        Id: null,
        CompanyGroupId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        Description: null,
        Active: true
    };
    $scope.materialgridNew = Object.assign({}, $scope.materialgrid);
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialgrid = $scope.materialgridlist[$scope.index];
        $scope.materialgridNew = Object.assign({}, $scope.materialgrid);
        $scope.GetMaterialgridCharacteristics();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $rootScope.searchMaterialgridByList = [
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

    ///--Start---MaterialGridCharacteristics---////
    $scope.characteristicsParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.characteristicslist = function () {
        $scope.characteristicsUrl = 'Materials/characteristics/getlist';
        $scope.getCharacterModalData = function (pageno) {
            baseService.paginationBase($scope.characteristicsUrl, pageno, $scope.characteristicsParameters)
                .then(function (result) {
                    $scope.charactermodallist = result.Rows;
                    $scope.characteristicsParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#characteristicsSearchPopUp')).modal('show');
        $scope.getCharacterModalData();
    }
    $scope.searchCharacteristicsModalList = [
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
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
        },
        {
            'name': 'Attribute Property',
            'value': 'AttributeProperty'
        }
    ];
    $scope.characs = [];
    $scope.addRow = function (x) {
        try {
            if ($scope.characs.length > 2)
                throw 'Total no of characteristics can not be more than 3...!';
            var isAvailable = false;
            for (var i = 0; i < $scope.characs.length; i++) {
                isAvailable = baseService.isAvailableInList($scope.characs[i].Id, x.Id, -1, -1);
                if (isAvailable) throw 'This charactristcs : [' + x.UserName + '] has been already taken';
            }
            $scope.characs.push({
                Id: x.Id
                , MaterialGridId: $scope.materialgridNew.MaterialGridId
                , Sort: $scope.characs.length + 1
                , Code: x.Code
                , ShortName: x.ShortName
                , StandardName: x.StandardName
                , UserName: x.UserName
                , AttributeProperty: x.AttributeProperty
                , IsMandatory: x.IsMandatory
                , IsPreDefinedField: x.IsPreDefinedField
                , Active: $scope.materialgridNew.Active
            });
            angular.element(document.querySelector('#characteristicsSearchPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure', 'characteristicsSearchPopUp');
        }
    };
    $scope.childArchiveId = [];
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($rootScope.id))
            $scope.childArchiveId.push($rootScope.id);
        $scope.characs.splice($rootScope.index, 1);
        $rootScope.id = null;
    };
    $scope.GetMaterialgridCharacteristics = function () {
        $http.get('Materials/materialgrid/getmaterialgridcharacteristics?materialGridId=' + $scope.materialgridNew.Id)
            .then(function (response) {
                $scope.characs = response.data;
                $scope.charTblShow = true;
                angular.element(document.querySelector('#characteristicsPopUp')).modal('show');
            });
    };
    ///--End---MaterialGridCharacteristics---////

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.materialgridForm.$valid) {
                angular.copy($scope.materialgridNew, $scope.materialgrid);
                if ($scope.Action == 'Save') {
                    $http({
                        method: 'POST',
                        url: 'Materials/materialgrid/create',
                        data: {
                            materialgrid: $scope.materialgrid,
                            materialGridCharacteristics: $scope.characs,
                            deletedItems: $scope.childArchiveId
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.materialgridlist.push(response.data.MaterialGrid);
                            $scope.materialgridlist = $filter('orderBy')($scope.materialgridlist, 'GridNo');
                            baseService.paginationAdd();
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                else if ($scope.Action == 'Update') {
                    $http({
                        method: 'POST',
                        url: 'Materials/materialgrid/create',
                        data: {
                            materialgrid: $scope.materialgrid,
                            materialGridCharacteristics: $scope.characs,
                            deletedItems: $scope.childArchiveId
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.materialgridlist[$scope.index] = $scope.materialgrid;
                                $scope.materialgridlist = $filter('orderBy')($scope.materialgridlist, 'GridNo');
                            }
                            $scope.childArchiveId = null;
                            ClearFields();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialgridNew.Id)) {
            $http({
                method: 'POST',
                url: 'Materials/materialgrid/delete/',
                data: { id: $scope.materialgridNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialgridlist.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sort);
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.materialgrid = {};
        $scope.materialgridNew = {};
        $scope.materialgridNew.Active = true;
        $scope.characs = [];
    }
};
MaterialGridController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];