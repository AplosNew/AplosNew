'use strict';
SkillController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SkillController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Skill";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.skills = [];
    $scope.path = 'skills/skill/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.skills = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'Skill Category',
            'value': 'SkillCategoryName'
        },
        {
            'name': 'Machine Applicable',
            'value': 'IsMachineApplicable'
        }
    ];

 
    $scope.skill = {
        Id: null,
        SkillCategoryId: null,
        SkillGroupId: null,
        SkillCategoryName: null,
        CompanyGroupId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        IsMachineApplicable: false,
        Active: true,
        DashboardApplicable: true,
        OperationApplicable: true
    };
    $scope.skillNew = Object.assign({}, $scope.skill);

    // #region name

    $scope.SkillGroupingList = [];
    $scope.GetCboSkillGroupingCbo = function () {
        //debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillGrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();

    $scope.skillcategoryList = [];
    $http({
        method: 'GET',
        url: 'skills/skillcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.skillcategoryList = response.data;
    });

    // #endregion

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.skillNew.Sequence = response.data;
            });
    };

    $scope.GetSequence();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.skill = $scope.skills[$scope.index];
        $scope.skillNew = Object.assign({}, $scope.skill);
        $scope.getSkillProcessList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.skillNewForm.$valid) {
            angular.copy($scope.skillNew, $scope.skill);
            $scope.skillCategoryName = document.getElementById("skillCategoryId").options[document.getElementById('skillCategoryId').selectedIndex].text;
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'entity': $scope.skill,
                        'skillProcess': $scope.skillProcessList
                    },
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.skill = response.data.Skill;
                        $scope.skill.SkillCategoryName = $scope.skillCategoryName;
                        $scope.skills.push($scope.skill);
                        $scope.skills = $filter('orderBy')($scope.skills, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'entity': $scope.skill,
                        'skillProcess': $scope.skillProcessList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.skill.SkillCategoryName = $scope.skillCategoryName;
                            $scope.skills[$scope.index] = $scope.skill;
                            $scope.skills = $filter('orderBy')($scope.skills, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                        $scope.getData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.skillNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.skillNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.skills.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.Action = "Save";
        $scope.skill = {};
        $scope.skillNew = { Sequence: seq, IsMachineApplicable: false, Active: true };
        $scope.skillProcessList = [];
        $rootScope.tempList = [];
        $scope.processList = [];
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Process
    $scope.skillProcessList = [];
    $scope.processList = [];
    $scope.getSkillProcessList = function () {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSkillProcessList?skillId=' + $scope.skillNew.Id
        }).then(function successCallback(response) {
            $scope.skillProcessList = response.data.Rows;
        });
    };

    $scope.processParameters = {
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
    $scope.processPopUp = function () {
        angular.forEach($scope.skillProcessList, function (a) {
            $rootScope.tempList.push({
                Id: a.ProcessId,
                Sequence: a.Sequence,
                Code: a.Code,
                ShortName: a.ShortName,
                StandardName: a.StandardName,
                UserName: a.UserName,
                MaterialType: a.MaterialType,
                Active: a.Active,
                Archive: false
            });
        });
        baseService.setCurrentPage('processList');
        $scope.getProcessData = function (pageno) {
            $scope.getProcessUrl = 'Processes/process/GetList?processid=' + baseService.getColumnValueList($scope.skillProcessList, 'ProcessId');
            baseService.paginationBase($scope.getProcessUrl, pageno, $scope.processParameters)
                .then(function (result) {
                    $scope.processList = result.Rows;
                    $scope.processParameters.total_count = result.Total;
                    for (var t = 0; t < baseService.arrayLength($scope.processList); t++) {
                        $scope.processList[t].Flag = baseService.valueCheckInList($rootScope.tempList, 'Id', $scope.processList[t].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };
    $rootScope.searchProcessByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.addProcess = function () {
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!baseService.valueCheckInList($scope.skillProcessList, 'ProcessId', a.Id)) {
                    $scope.skillProcessList.push({
                        Id: null,
                        ProcessId: a.Id,
                        Sequence: a.Sequence,
                        Code: a.Code,
                        ShortName: a.ShortName,
                        StandardName: a.StandardName,
                        UserName: a.UserName,
                        MaterialType: a.MaterialType,
                        Active: a.Active,
                        Archive: false
                    });
                }
            });
        }
        else
            $scope.skillProcessList = [];
        angular.forEach($scope.skillProcessList, function (a) {
            if (!baseService.valueCheckInList($rootScope.tempList, 'Id', a.ProcessId))
                $scope.skillProcessList.splice(a, 1);
        });
        $scope.CloseProcessPopUp();
    };
    $scope.CloseProcessPopUp = function () {
        $scope.processList = [];
        $rootScope.tempList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.Submaterial + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.removeRow = function () {
        for (var t = 0; t < baseService.arrayLength($rootScope.tempList); t++) {
            if ($rootScope.tempList[t].Id === $scope.skillProcessList[$scope.popUpIndex].ProcessId)
                $rootScope.tempList.splice(t, 1);
        }
        $scope.skillProcessList.splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };
    // #endregion
}