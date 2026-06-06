'use strict';
SkillDevelopmentMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SkillDevelopmentMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Skill Development";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.skills = [];
    $scope.path = 'skills/skill/';
    $scope.saveUrl = $scope.path + 'SaveMaster';
    $scope.deleteUrl = $scope.path + 'DeleteSDM/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Get = function (obj) {
        $scope.skill = obj.data;
        $scope.skillNew = Object.assign({}, $scope.skill);
        $scope.getSavedPositionData($scope.skillNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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

    $rootScope.searchByList = [
        {
            'name': 'StandardTrainingName',
            'value': 'StandardTrainingName'
        },
        {
            'name': 'UserTrainingName',
            'value': 'UserTrainingName'
        }

    ];


    $scope.skill = {
        Id: null,
        StandardTrainingName: null,
        UserTrainingName: null,
        ReportName: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        TrainingName: null,
        Active: true,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.skillNew = Object.assign({}, $scope.skill);

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.skillNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
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
                    $scope.getSDMData();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    }

    $scope.getSDMData = function () {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSDMList'
        }).then(function successCallback(response) {
            $scope.skills = response.data;
        });
    }
    $scope.getSDMData();

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.skillNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl
                , data: { 'Id': $scope.skillNew.Id }
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSDMData();
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

    $scope.Clear = function () {
        $scope.skill = {
            Id: null,
            StandardTrainingName: null,
            UserTrainingName: null,
            ReportName: null,
            ResponsiblePersonId: null,
            ResponsiblePerson: null,
            TrainingName: null,
            Active: true,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null

        };
        $scope.skillNew = Object.assign({}, $scope.skill);
        $scope.positionList = [];

    }

    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = 'Organizations/Position/GetList';
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Id',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function () {

        $scope.getPositionData = function () {
            $http({
                method: 'GET',
                url: 'Organizations/Position/GetList?TPId=' + $scope.positionParameters,
            }).then(function succ(response) {
                $scope.positionDataList = response.data.Rows;
                for (var i = 0; i < $scope.positionDataList.length; i++) {
                    $scope.positionDataList[i].Flag = false;
                }
            });
        }
        angular.element(document.querySelector('#positionPopUp')).modal('show');
        $scope.getPositionData();
    };


    $scope.refreshPSTemplate = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPS });
    };

    function CheckBoxSelectAllPS(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPS").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.positionDataList.length; i++) {
                $scope.positionDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPS").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkPositoinExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].PositionId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.positionList = [];
    function MakeData() {

        for (var i = 0; i < $scope.positionDataList.length; i++) {
            if ($scope.positionDataList[i].Flag == true) {
                if (checkPositoinExist($scope.positionList, $scope.positionDataList[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.SkillDevelopmentMasterId = $scope.skillNew.Id;
                    ob.PositionId = $scope.positionDataList[i].Id;
                    ob.Code = $scope.positionDataList[i].Code;
                    ob.UserName = $scope.positionDataList[i].UserName;
                    ob.Division = $scope.positionDataList[i].Division;
                    ob.Department = $scope.positionDataList[i].Department;
                    ob.Section = $scope.positionDataList[i].Section;
                    ob.Subsection = $scope.positionDataList[i].Subsection;
                    ob.Designation = $scope.positionDataList[i].Designation;
                    ob.DirectManpowerCost = $scope.positionDataList[i].DirectManpowerCost;

                    $scope.positionList.push(ob);
                    ob = {};
                }
                else {
                    throw "This position " + $scope.positionDataList[i].UserName + " is already taken.";
                }
            }
        }

    }

    $scope.closePositionPopUp = function () {
        try {
            MakeData();
            $scope.SavePosition();
            angular.element(document.querySelector('#positionPopUp')).modal('hide');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SavePosition = function () {
        try {
            if (baseService.arrayLength($scope.positionList) < 0) {
                throw "Select Position.";
            }

            $http({
                method: 'POST',
                url: 'skills/skill/SavePositionData',
                data: { 'data': $scope.positionList, 'masterId': $scope.skillNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedPositionData($scope.skillNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getSavedPositionData = function (masterId) {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSDPList?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.positionList = response.data;
            $scope.getSavedSkillData(masterId);
        });
    }

    $scope.message_DelPosconfirmation = null;
    $scope.RemovePosition = function (data) {
        $scope.DelPos = data.data;
        if (!baseService.isUndefinedOrNull($scope.DelPos.Id))
            $scope.message_DelPosconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDelPosPopUp')).modal('show');
    }

    $scope.DeletePosition = function () {
        $http({
            method: 'POST',
            url: 'skills/skill//DeleteSP?id=' + $scope.DelPos.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSavedPositionData($scope.skillNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

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
        $scope.skillDevNew.SkillId = obj.data.Id;
        $scope.skillDevNew.SkillName = obj.data.UserName;
        angular.element(document.querySelector('#SkillPopUp')).modal('hide');
    };

    $scope.skillDev = {
        Id: null,
        SkillId: null,
        SkillDevelopmentMasterId: null,
        TrainingCentreDuration: null,
        OnJobTrainingDuration: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.skillDevNew = Object.assign({}, $scope.skillDev);

    $scope.SaveSkillDev = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.skillDevNew.SkillDevelopmentMasterId = $scope.skillNew.Id;
        if ($scope.skillDevNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'skills/skill/SaveSkillData',
                data: {
                    'data': $scope.skillDevNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedSkillData($scope.skillNew.Id);
                    $scope.ClearSD();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    }

    $scope.RemoveSkill = function (data) {
        $scope.DelSkill = data.data;
        if (!baseService.isUndefinedOrNull($scope.DelSkill.Id))
            $scope.message_DelPosconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDelSkillPopUp')).modal('show');
    }

    $scope.DeleteSkill= function () {
        $http({
            method: 'POST',
            url: 'skills/skill/DeleteSkill?id=' + $scope.DelSkill.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getSavedSkillData($scope.skillNew.Id);
                $scope.ClearSD();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ClearSD = function () {
        $scope.skillDev = {
            Id: null,
            SkillId: null,
            SkillDevelopmentMasterId: null,
            TrainingCentreDuration: null,
            OnJobTrainingDuration: null,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null

        };
        $scope.skillDevNew = Object.assign({}, $scope.skillDev);
    }

    $scope.selectedSkillList = [];
    $scope.getSavedSkillData = function (masterId) {
        $http({
            method: 'GET',
            url: 'skills/skill/GetSDSList?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.selectedSkillList = response.data;
        });
    }

    $scope.GetSD = function (obj) {
        $scope.skillDev = obj.data;
        $scope.skillDevNew = Object.assign({}, $scope.skillDev);
    };
}