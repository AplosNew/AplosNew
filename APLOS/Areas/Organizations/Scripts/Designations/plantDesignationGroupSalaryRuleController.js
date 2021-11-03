'use strict';
plantDesignationGroupSalaryRuleController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function plantDesignationGroupSalaryRuleController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "PlantDesignationGroupSalaryRule ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantDesignationGroupSalaryRules = [];
    $scope.plantDesignationGroupSalaryRuleList = [];
    $scope.designationGroupList = [];
    $scope.plantList = [];
    $scope.path = 'Organizations/plantDesignationGroupSalaryRule/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    /**********MasterDataList**********/
    $scope.getPlantDesignationGroupSalaryRule = function () {
        $http.get('Organizations/plantDesignationGroupSalaryRule/GetPlantDesignationGroupSalaryRule?plantId=' + $scope.plantDesignationGroupSalaryRuleNew.PlantId + '&salaryRuleMasterId=' + $scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId)
            .then(function (response) {
                $scope.plantDesignationGroupSalaryRuleList = response.data;
            });
    };
    /**********MasterDataList**********/

    $scope.plantDesignationGroupSalaryRule = {
        Id: null,
        PlantId: null,
        CompanyId: null,
        DesignationGroupId: null,
        EmployeeCategoryId: null,
        ComplianceDocumentId: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.plantDesignationGroupSalaryRuleNew = Object.assign({}, $scope.plantDesignationGroupSalaryRule);

    /***Cbo***************/
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.plantDesignationGroupSalaryRuleNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.getSalaryRuleMasterWithPlant = function () {
        $http.get('Organizations/PlantDesignationGroupSalaryRule/GetSalaryRuleMasterWithPlantCbo?plantId=' + $scope.plantDesignationGroupSalaryRuleNew.PlantId)
            .then(function (response) {
                $scope.salaryRuleMasterList = response.data;
            });
    };
    //--------------
    //******************Designation Group**************/
    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        if (event.currentTarget.checked) {
            if (!baseService.valueCheckInList($scope.tempList, 'Id', data.Id))
                $scope.tempList.push(data);
        }
        else {
            if (baseService.valueCheckInList($scope.tempList, 'Id', data.Id)) {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].Id === data.Id) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        }
        // $scope.tempList.splice($scope.tempList.indexOf(LoanTypeTakenId), 1);
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.searchByDesignationGroupList = [
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
            'value': 'Standard Name'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.designationGroupListParameters = {
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
    $scope.GetDesignationGroupList = function () {
        $scope.tempList = [];
        if (baseService.isUndefinedOrNull($scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId)) {
            return ShowResult("Please select Salary Rule Master", 'failure');
        }
        $scope.GLUrl3 = 'Organizations/PlantDesignationGroupSalaryRule/GetDesignationGroupWithoutExistingId?designationIds=' + addDesignationIds($scope.plantDesignationGroupSalaryRuleList) + '&salaryRuleMasterId=' + $scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId;
        $scope.GetDesignationGroupListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.designationGroupListParameters)
                .then(function (data) {
                    $scope.designationGroupList = data.Rows;
                    for (var i = 0; i < $scope.designationGroupList.length; i++) {
                        $scope.designationGroupList[i].Flag = getActive($scope.tempList, $scope.designationGroupList[i].Id);
                    }
                    $scope.designationGroupListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#designationGroupPopUp')).modal('show');
        $scope.GetDesignationGroupListData();
    };
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DesignationGroupId === id) {
                return true;
            }
        }
        return false;
    }
    function addDesignationIds(list) {
        var designationId = "''";
        angular.forEach(list, function (item) {
            if (designationId === "''") {
                designationId = "'" + item.DesignationGroupId + "'";
            } else {
                designationId += ",'" + item.DesignationGroupId + "'";
            }
        });
        return designationId;
    }
    $scope.GetPlantDesignationGroupSalaryRuleDetailList = function () {
        if (baseService.arrayLength($scope.tempList) < 1)
            return ShowResult('Please at first select row', 'failure', 'designationGroupPopUp');
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag && checkExist($scope.plantDesignationGroupSalaryRuleList, item.Id) === false) {
                $scope.plantDesignationGroupSalaryRuleList.push(
                    {
                        Id: null
                        , DesignationGroupId: item.Id
                        , PlantId: $scope.plantDesignationGroupSalaryRuleNew.PlantId
                        , SalaryRuleMasterId: $scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId
                        , PlantDesignationGroupSalaryRuleId: $scope.plantDesignationGroupSalaryRuleNew.Id
                        , Code: item.Code
                        , ShortName: item.ShortName
                        , StandardName: item.StandardName
                        , UserName: item.UserName
                        , Active: item.Active
                        , Flag: item.Flag
                    }
                );
            }
        });
        angular.element(document.querySelector('#designationGroupPopUp')).modal('hide');
    }
    //-----------------
    //Deleting Rows from DesignationGroupSelectedList
    $scope.valuePassInDelModal = function (data, index) {
        $scope.designationGroupRuleId = data.Id;
        $scope.designationGroupRuleIndex = index;
        $scope.message_confirmation = "Are you sure want to delete [ '" + data.UserName + "' ] ?";
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteRow = function () {
        $scope.plantDesignationGroupSalaryRuleList.splice($scope.designationGroupRuleIndex, 1);
        $scope.designationGroupRuleId = null;
        $scope.designationGroupRuleIndex = -1;
    };
    //
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantDesignationGroupSalaryRule = $scope.plantDesignationGroupSalaryRules[$scope.index];
        $scope.plantDesignationGroupSalaryRuleNew = Object.assign({}, $scope.plantDesignationGroupSalaryRule);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        angular.copy($scope.plantDesignationGroupSalaryRuleNew, $scope.plantDesignationGroupSalaryRule);
        if ($scope.plantDesignationGroupSalaryRuleList.length < 1) {
            return ShowResult("Add at least one designation group", 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.plantDesignationGroupSalaryRuleForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'plantDesignationGroupSalaryRule': $scope.plantDesignationGroupSalaryRuleList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getPlantDesignationGroupSalaryRule();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.plantDesignationGroupSalaryRuleNew.PlantId) && !baseService.isUndefinedOrNull($scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + '?plantId=' + $scope.plantDesignationGroupSalaryRuleNew.PlantId + '&salaryRuleMasterId=' + $scope.plantDesignationGroupSalaryRuleNew.SalaryRuleMasterId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.plantDesignationGroupSalaryRule = {};
        $scope.plantDesignationGroupSalaryRuleNew = {};
        $scope.plantDesignationGroupSalaryRuleNew.Id = null
        $scope.plantDesignationGroupSalaryRuleNew.Active = true;
        $scope.plantDesignationGroupSalaryRuleList = [];
        $scope.designationGroupList = [];
        $scope.tempList = [];
    }

    $scope.designationMasterReport = function () {
        location.href = 'Organizations/plantDesignationGroupSalaryRule/designationmasterreport?plantId=' + $scope.plantDesignationGroupSalaryRuleNew.PlantId;
    };
}