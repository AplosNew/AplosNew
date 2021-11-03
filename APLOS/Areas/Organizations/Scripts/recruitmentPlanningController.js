'use strict';
recruitmentPlanningController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','$window'];
function recruitmentPlanningController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService,$window) {
    $rootScope.title = 'Recruitment Planning';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.manPowerplanningmasters = [];
    $scope.path = 'Organizations/recruitmentplanning/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.CAction = 'Add';

    $scope.searchByList = [
        {
            'name': 'Remarks',
            'value': 'Remarks'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.recruitmentPlanning = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        PlantId: $window.plantId,
        UserName: null,
        Remarks: null,
        Active: true,
        Archive: false
    };

    $scope.recruitmentPlanningDetail = {
        Id: null,
        ManpowerBudgetId: null,
        RecruitmentGroupId: null,
        RecruitmentPlanningId: null,
        Male: 0,
        Female: 0,
        TotalManpower: 0,
        Remarks: null,
        Active: true
    };

    baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.recruitmentPlanning.CompanyId;
        $rootScope.parameters.plantId = $scope.recruitmentPlanning.PlantId;
        baseService.pagination(pageno)
            .then(function successCallback(result) {
                $scope.recruitmentPlannings = result.Rows;
                ClearFields();
            }, function errorCallback() {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getDetailList = function (recruitmentPlanningId) {
        $http({
            method: 'GET',
            url: $scope.path + 'getrpdetaillist?recruitmentPlanningId=' + recruitmentPlanningId
        }).then(function successCallback(response) {
            $scope.recruitmentPlanningDetailList = response.data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.recruitmentPlanning = $scope.recruitmentPlannings[$scope.index];
        $scope.getDetailList($scope.recruitmentPlanning.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantByCompany = function (companyId) {
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.getCboManpowerBudgetByCompanyAndPlant = function (companyId, plantId) {
        cboService.getCboManpowerBudgetByCompanyAndPlant(companyId, plantId, function (result) {
            $scope.manpowerBudgetList = result;
        });
    };

    $scope.getCboRecruitmentGroupByPlant = function (plantId) {
        cboService.getCboRecruitmentGroupByPlant(plantId, function (result) {
            $scope.recruitmentGroupList = result;
        });
    };

    $scope.entities = [];
    $scope.getRelationChain = function (companyId, id) {
        $scope.entities = [];
        $http({
            method: 'GET',
            url: 'Organizations/manpowerbudget/getmanpowerbudgetrelationchainbyid?companyId=' + companyId + '&&id=' + id
        }).then(function successCallback(response) {
            if (baseService.arrayLength($scope.entities) === 0) {
                var localValue = [];
                localValue.push(response.data);
                baseService.getDDLSearchColumn(localValue, $scope.entities);
                $scope.entityValue = localValue;
            }
        });
    };

    $scope.getOnboard = function (id, index) {
        var rg = $.grep($scope.recruitmentGroupList, function (rg) {
            return rg.Value === id;
        })[0];
        var row = $scope.recruitmentPlanningDetailList[index];
        row.OnBoardDate = $filter('dateFiltering')($filter('dateFilter')(rg.OnBoardDate), 'dd-MM-yyyy');
    };

    $scope.recruitmentPlanningDetailList = [];
    $scope.editIndex = -1;
    $scope.addRow = function () {
        if (baseService.isUndefinedOrNull($scope.recruitmentPlanningDetail.ManpowerBudgetId)) {
            ShowResult('Please select Manpower Budget!', 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.recruitmentPlanningDetail.RecruitmentGroupId)) {
            ShowResult('Please select Recruitment Group!', 'failure');
        }
        else if (baseService.isUndefinedOrNull($scope.recruitmentPlanningDetail.TotalManpower)) {
            ShowResult('Please input Male or Female numbers!', 'failure');
        }
        else {
            if ($scope.CAction === 'Add') {
                $scope.recruitmentPlanningDetailList.push($scope.recruitmentPlanningDetail);
            }
            else
                $scope.recruitmentPlanningDetailList[$scope.editIndex] = $scope.recruitmentPlanningDetail;
            $scope.recruitmentPlanningDetail = {};
            $scope.recruitmentPlanningDetail.Male = 0;
            $scope.recruitmentPlanningDetail.Female = 0;
            $scope.recruitmentPlanningDetail.Active = true;
            $scope.CAction = 'Add';
            $scope.editIndex = -1;
        }
    };

    $scope.getEditRow = function (index) {
        $scope.editIndex = index;
        $scope.recruitmentPlanningDetail = $scope.recruitmentPlanningDetailList[$scope.editIndex];
        $scope.CAction = 'Update';
    };

    $scope.removeRow = function (index) {
        $scope.recruitmentPlanningDetailList.splice(index, 1);
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.recruitmentPlanningForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'recruitmentPlanning': $scope.recruitmentPlanning, 'recruitmentPlanningDetails': $scope.recruitmentPlanningDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { 'recruitmentPlanning': $scope.recruitmentPlanning, 'recruitmentPlanningDetails': $scope.recruitmentPlanningDetailList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.recruitmentPlanning.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.recruitmentPlanning.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.recruitmentPlannings.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.recruitmentProcessSetDetailList = [];
    $scope.recruitmentProcessSetDetailPopUp = function (positionId, onBoardDate) {
        if (baseService.isUndefinedOrNull(positionId)) {
            $scope.manpowerBudgetDataList = [];
            ShowResult('Please select position.', 'failure');
        }
        else {
            $http({
                method: 'GET',
                url: 'employees/RecruitmentProcessSet/GetDetailListForPlanning?positionId=' + positionId + '&&targetDate=' + onBoardDate
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.recruitmentProcessSetDetailList = [];
                    ShowResult(response.data.Message, 'failure', 'recruitmentProcessSetDetailPopUp');
                }
                else {
                    $scope.recruitmentProcessSetDetailList = response.data;
                }
            });
            $scope.OnBoardDate = onBoardDate;
            angular.element(document.querySelector('#recruitmentProcessSetDetailPopUp')).modal('show');
        }
    };

    //*********************** Manpower Budget PopUp Start *************************************
    $scope.manpowerBudgetSearchList = [];
    $scope.manpowerBudgetDataList = [];
    $scope.manpowerBudgetSearch = [];
    $scope.manpowerBudgetUrl = 'Organizations/ManpowerBudget/getlistbyplant';
    $scope.manpowerBudgetParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Code',
        searchBy: 'Id',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.manpowerBudgetPopUp = function (plantId) {
        if (baseService.isUndefinedOrNull(plantId)) {
            $scope.manpowerBudgetDataList = [];
            ShowResult('Please select Plant.', 'failure');
        }
        else {
            $scope.manpowerBudgetParameters.plantId = plantId;
            $scope.getManpowerBudgetData = function (pageno) {
                baseService.paginationBase($scope.manpowerBudgetUrl, pageno, $scope.manpowerBudgetParameters)
                    .then(function (response) {
                        $scope.manpowerBudgetDataList = response.Rows;
                        $scope.manpowerBudgetParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.manpowerBudgetSearchList) === 0) {
                            $scope.manpowerBudgetSearchList.push(
                                {
                                    'Text': 'Id',
                                    'Value': 'Id'
                                });
                            baseService.getDDLSearchColumn($scope.manpowerBudgetDataList, $scope.manpowerBudgetSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('show');
            $scope.getManpowerBudgetData();
        }
    };

    $scope.closeManpowerBudgetPopUp = function () {
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
        $scope.recruitmentPlanningDetail.ManpowerBudgetId = $scope.selectedManpowerBudgetId;
    };

    $scope.selectManpowerBudgetPopUp = function () {
        angular.forEach($scope.manpowerBudgetDataList, function (element, i) {
            if (element.Active) {
                $scope.recruitmentPlanningDetailList.push({
                    PositionId: element.PositionId,
                    ManpowerBudgetId: element.Id,
                    ManpowerBudgetCode: element.Code,
                    Male: element.BudgetedMale,
                    Female: element.BudgetedFemale,
                    TotalManpower: element.BudgetedTotal
                });
            }
        });
        //var entity = $scope.manpowerBudgetDataList[document.querySelector('#selectedManpowerBudget:checked').value];
        //$scope.selectedManpowerBudgetId = entity.Id;
        //$scope.recruitmentPlanningDetail.ManpowerBudgetId = $scope.selectedManpowerBudgetId;
        //$scope.recruitmentPlanningDetail.PositionId = entity.PositionId;
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };

    $scope.clearEntity = function () {
        $scope.selectedEntityId = null;
        $scope.manPowerbudgetmasterNew.EntityId = null;
        $scope.manPowerbudgetmasterNew.EntityName = null;
        $scope.clearPosition();
        $scope.entityData = [];
        $scope.entitySearch = [];
    };
    //*********************** Entity PopUp End *************************************

    $scope.countTotalNumber = function (male, female, index) {
        var row = $scope.recruitmentPlanningDetailList[index];
        if (baseService.isUndefinedOrNull(male))
            male = 0;
        else if (baseService.isUndefinedOrNull(female))
            female = 0;
        row.TotalManpower = parseInt(male) + parseInt(female);
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.CAction = 'Add';
        var cId = $scope.recruitmentPlanning.CompanyId;
        var pId = $scope.recruitmentPlanning.PlantId;
        $scope.recruitmentPlanning = {};
        $scope.recruitmentPlanning.CompanyId = cId;
        $scope.recruitmentPlanning.PlantId = pId;
        $scope.recruitmentPlanning.Active = true;
        $scope.recruitmentPlanningDetail = {};
        $scope.recruitmentPlanningDetail.Male = 0;
        $scope.recruitmentPlanningDetail.Female = 0;
        $scope.recruitmentPlanningDetail.TotalManpower = 0;
        $scope.recruitmentPlanningDetail.Active = true;
        $scope.recruitmentPlanningDetailList = [];
    }
}