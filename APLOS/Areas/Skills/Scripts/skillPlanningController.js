'use strict';
skillPlanningController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function skillPlanningController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Skill";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.skills = [];
    $scope.skillplanList = [];
    $scope.path = 'skills/skill/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

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
        TraningName: null,
        BatchNo: null,
        UserBatchNo: null,
        RequirementGivenBy: null,
        ResponsiblePersonId: null,
        RequirementDate: null,
        StartDate: null,
        TransferDate: null,
        FinalClosingDate: null,
        BatchStatus: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.skillNew = Object.assign({}, $scope.skill);

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    //$scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.skillNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.skillNew.ResponsiblePerson = obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.Get = function (obj) {
        $scope.skill = obj.data;
        $scope.skillNew = Object.assign({}, $scope.skill);
        //$scope.getSavedPositionData($scope.skillNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.skillNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'skills/skill/SaveSkillPlanning',
                data: {
                    'data': $scope.skillNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSPData();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.skillNew.Id)) {
            $http({
                method: 'POST'
                , url: 'skills/skill/DeleteSkillPlanning'
                , data: { 'Id': $scope.skillNew.Id }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSPData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };

    $scope.getSPData = function () {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSPList'
        }).then(function successCallback(response) {
            $scope.skills = response.data;
        });
    }
    $scope.getSPData();

    $scope.Clear = function () {
        $scope.skill = {
            Id: null,
            TraningName: null,
            BatchNo: null,
            UserBatchNo: null,
            RequirementGivenBy: null,
            ResponsiblePersonId: null,
            RequirementDate: null,
            StartDate: null,
            TransferDate: null,
            FinalClosingDate: null,
            BatchStatus: null,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null

        };
        $scope.skillNew = Object.assign({}, $scope.skill);

    }


    $scope.skillList = [];
    $scope.SkillPopUp = function () {
        $http({
            method: 'GET',
            url: 'skills/skill/GetList',
        }).then(function succ(response) {
            $scope.skillList = response.data.Rows;
            angular.element(document.querySelector('#SkillPopUp')).modal('show');
        });
    }

    $scope.SetSkill = function (obj) {
        $scope.skillPNew.SkillId = obj.data.Id;
        $scope.skillPNew.SkillName = obj.data.UserName;
        angular.element(document.querySelector('#SkillPopUp')).modal('hide');
    };

    $scope.skillP = {
        Id: null,
        SkillName: null,
        BatchNo: null,
        SkillId: null,
        TraineeRequired: null,
        TraineePlan: null,
        DurationPlan: null,
        TrainerName: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.skillPNew = Object.assign({}, $scope.skillP);

    $scope.GetSP = function (obj) {
        $scope.skillP = obj.data;
        $scope.skillPNew = Object.assign({}, $scope.skillP);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveSP = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.skillPlanNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'skills/skill/SaveSkillPlan',
                data: {
                    'data': $scope.skillPNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSPlanData();
                    $scope.ClearSP();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    }
    $scope.message = null;

    $scope.DltSP = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.skillPNew.Id))
                $scope.message = 'Are you sure want to delete permanently [ ' + $scope.skillPNew.BatchNo + ' ]';
            angular.element(document.querySelector('#removerPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteSP = function () {
        if (!baseService.isUndefinedOrNull($scope.skillPNew.Id)) {
            $http({
                method: 'POST'
                , url: 'skills/skill/DeleteSkillPlan'
                , data: { 'Id': $scope.skillPNew.Id }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSPlanData();
                    $scope.ClearSP();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };


    $scope.skillplanList = [];
    $scope.getSPlanData = function () {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSPDataList'
        }).then(function successCallback(response) {
            $scope.skillplanList = response.data;
        });
    }
    $scope.getSPlanData();

    $scope.ClearSP = function () {
        $scope.skillP = {
            Id: null,
            SkillName: null,
            BatchNo: null,
            SkillId: null,
            TraineeRequired: null,
            TraineePlan: null,
            DurationPlan: null,
            TrainerName: null,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null

        };
        $scope.skillPNew = Object.assign({}, $scope.skillP);
    }







}