'use strict';
DesignationMasterConfigurationController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function DesignationMasterConfigurationController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Designation Configuration ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.designationMasterConfigurationList = [];
    $scope.salaryHeadList = [];
    $scope.plantList = [];
    $scope.designationGroupList = [];
    $scope.designationList = [];
    $scope.path = 'HumanResource/designationMasterConfiguration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.designationMasterConfiguration = {
        Id: null,
        PlantId: null,
        DesignationMasterId: null,
        RecruitmentProcessSetId: null,
        AccountsGroupId:null,
        SalaryRuleMasterId: null,
        LeavePolicyMasterId: null,
        SalaryFixationSettingId: null,
        BonusPolicyMasterId: null,
        OverTimePmtPolicyMasterID: null,
        BnsPlcMthRetainID: null,
        OverTimePmtPolicyMasterId: null,
        HolidayPayDayMasterId: null,
        AttdnBonusHeaderId: null,
        IsOTEntitled: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.designationMasterConfigurationNew = Object.assign({}, $scope.designationMasterConfiguration);

    /***Cbo***************/
    //cboService.getCompanyGroupCompanyCbo(null, function (result) {
    //    $scope.companyList = result;
    //});
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.getPlantList = function () {

        cboService.getCboPlantByCompany($scope.designationMasterConfigurationNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
        $scope.designationGroupList = result;
    });
    $scope.recruitmentPlanningProcessSetList = [];
    cboService.getCboRecruitmentProcessSetByCompanyGroup(null, function (result) {
        $scope.recruitmentPlanningProcessSetList = result;
    });
    $scope.accountsGroupList = [];
    cboService.getAccountsGroupCbo(function (result) {
        $scope.accountsGroupList = result;
    });
    $scope.salaryFixationList = [];
    cboService.getSalaryFixationCbo(function (result) {
        $scope.salaryFixationList = result;
    });
    $scope.salaryRuleMasterList = [];
    $scope.getSalaryRuleMasterWithPlant = function () {
        $http.get('Organizations/PlantDesignationGroupSalaryRule/GetSalaryRuleMasterWithPlantCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.salaryRuleMasterList = response.data;
            });
    };
    $scope.LeavePolicyMasterList = [];
    $scope.getLeavePolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetLeavePolicyCboList?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.LeavePolicyMasterList = response.data;
            });
    }
    $scope.AttdnBonusHeaderList = [];
    $scope.getAttdnBonusHeaderList = function () {
        $http.get($scope.path + 'GetAttdnBonusHeaderId?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.AttdnBonusHeaderList = response.data;
            });
    }

    $scope.BonusPolicyMasterList = [];
    $scope.getBonusPolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetBonusPolicyMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.BonusPolicyMasterList = response.data;
            });
    }
    $scope.AttdnBonusPmtPolicyMasterList = [];
    $scope.getAttdnBonusPmtPolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetAttdnBonusPmtPolicyMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.AttdnBonusPmtPolicyMasterList = response.data;
            });
    }
    $scope.BonusPolicyMonthlyRetainMasterList = [];
    $scope.getBonusPolicyMonthlyRetainMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetBonusPolicyMonthlyRetainMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.BonusPolicyMonthlyRetainMasterList = response.data;
            });
    }
    $scope.reSetDesignationList = function () {
        if ($scope.designationMasterConfigurationList.length > 0) {
            $scope.getDesignationMasterConfiguration();
        }
    }
    $scope.PFPolicyMasterList = [];
    $scope.getPFPolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetPFPolicyMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.PFPolicyMasterList = response.data;
            });
    }
    $scope.ESICPolicyMasterList = [];
    $scope.getESICPolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/GetESICPolicyMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.ESICPolicyMasterList = response.data;
            });
    }
    $scope.OverTimePmtPolicyMasterList = [];
    $scope.getOverTimePmtPolicyMaster = function () {
        $http.get('HumanResource/DesignationMasterConfiguration/OverTimePmtPolicyMasterCbo?plantId=' + $scope.designationMasterConfigurationNew.PlantId)
            .then(function (response) {
                $scope.OverTimePmtPolicyMasterList = response.data;
            });
    }

    $scope.HolidayPayDayMasterList = [];
    $scope.GetAdditionalPayDayCbo = function () {
        cboService.GetAdditionalPayDayCbo($scope.designationMasterConfigurationNew.PlantId, function (result) {
            $scope.HolidayPayDayMasterList = result;
        });
    }

    //--------------
    $scope.getDesignationMasterConfiguration = function () {
        var url = 'HumanResource/DesignationMasterConfiguration/GetDesignationListWithDesignationGroup?designationGroupId=' + $scope.designationMasterConfigurationNew.DesignationGroupId + '&plantId=' + $scope.designationMasterConfigurationNew.PlantId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.designationMasterConfigurationList = response.data;
            //angular.forEach(response.data, function (item) {
            //    $scope.designationMasterConfigurationList.push(item);
            //});
        });
    }
    function setDataForSave(list) {
        angular.forEach(list, function (item) {
            item.PlantId = $scope.designationMasterConfiguration.PlantId;
            item.CompanyGroupId = $window.companyGroupId;
            $scope.designationMasterConfigurationSaveList.push(item);
        });
    }
    $scope.designationMasterConfigurationSaveList = [];
    $scope.Save = function () {
        try {
            angular.copy($scope.designationMasterConfigurationNew, $scope.designationMasterConfiguration);
            $scope.designationMasterConfigurationSaveList = [];
            setDataForSave($scope.designationMasterConfigurationList);

            for (var i = 0; i < $scope.designationMasterConfigurationSaveList.length; i++) {
                if ($scope.designationMasterConfigurationSaveList[i].IsOTEntitled) {
                    if (baseService.isUndefinedOrNull($scope.designationMasterConfigurationSaveList[i].OverTimePmtPolicyMasterID)) {
                        throw "OT Pmt Policy Master is required for " + $scope.designationMasterConfigurationSaveList[i].UserName+".";
                    }
                }
            }

            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'DesignationMasterConfiguration': $scope.designationMasterConfigurationSaveList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDesignationMasterConfiguration();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    // #endregion
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.designationMasterConfiguration = { PlantId: $scope.designationMasterConfiguration.PlantId, CompanyId: $scope.designationMasterConfiguration.CompanyId, DesignationGroupId: $scope.designationMasterConfiguration.DesignationGroupId };
        $scope.designationMasterConfigurationNew = { PlantId: $scope.designationMasterConfigurationNew.PlantId, CompanyId: $scope.designationMasterConfigurationNew.CompanyId, DesignationGroupId: $scope.designationMasterConfigurationNew.DesignationGroupId };
    }
    $scope.ClearAll = function () {
        $scope.Action = "Save";
        $scope.designationMasterConfiguration = {
            Id: null,
            PlantId: null,
            DesignationId: null,
            DesignationGroupId: null,
            RecruitmentProcessSetId: null,
            AccountsGroupId:null,
            SalaryRuleMasterId: null,
            LeavePolicyMasterId: null,
            AttdnBonusHeaderId:null,
            IsOTEntitled: true,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedDate: null
        };
        $scope.designationMasterConfigurationNew = Object.assign({}, $scope.designationMasterConfiguration);
        $scope.designationMasterConfigurationList = [];
        $scope.designationMasterConfigurationSaveList = [];
    }

    $scope.legalDesignationList = [];
    $scope.details = function (designationMasterId) {
        $scope.legalDesignationList = [];
        $http.get('HumanResource/DesignationMasterConfiguration/GetLegalDesignationbyDesignationMaster?designationMasterId=' + designationMasterId)
            .then(function (response) {
                $scope.legalDesignationList = response.data;
            });
        angular.element(document.querySelector('#LegalDesignationPopUp')).modal('show');
    }
}