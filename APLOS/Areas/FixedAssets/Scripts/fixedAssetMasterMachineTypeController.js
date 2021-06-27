'use strict';
fixedAssetMasterMachineTypeController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', "$http", "$filter", "$compile", "cboService"];
function fixedAssetMasterMachineTypeController(commonMessage, $rootScope, $scope, baseService, $http, $filter, $compile, cboService) {
    $rootScope.title = 'Fixed Asset Master MachineType';
    $scope.Action = 'Save';
    $scope.fixedAssetMasterMachineTypeList = [];
    $scope.machineTypeSelectedList = [];
    $scope.machineTypeList = [];
    $scope.searchByMachineTypeList = [];
    var url = '/fixedassets/FixedAssetMasterMachineType/getlist';
    $scope.DataList = [];
    $scope.FieldDataList = [];
    $scope.fixedAssetMasterMachineTypeDataList = [];
    $scope.dataListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        searchBy: 'Id',
        sort: 'Id',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        baseService.paginationBase(url, pageno, $scope.dataListParameters)
            .then(function (response) {
                $scope.DataList = response.rows;
                $scope.dataListParameters.total_count = response.total;
                if (baseService.arrayLength($scope.FieldDataList) == 0) {
                    baseService.getDDLSearchColumn(response.rows, $scope.FieldDataList);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    //$scope.getData();
    $scope.fixedAssetMasterMachineType = {
        Id: null,
        MachineTypeId: null,
        FixedAssetMasterId: null,
    };
    $scope.fixedAssetMaster = {
        Id: null,
        UserName: null,
        FixedAssetCategory: null,
        FixedAssetSubCategory: null,
        FixedAssetClass: null,
        FixedAssetSubClass: null,
        FixedAsset: null
    }
    $rootScope.paymentLinkList = [
        {
            'Text': 'Skill',
            'Value': 'Skill'
        },
        {
            'Text': 'Others',
            'Value': 'Others'
        }];

    cboService.getCboRecruitmentProcessSetByCompanyGroup(null, function (result) {
        $scope.recruitmentProcessSetList = result;
    });
    //function----------------------------------------------------------------------------------------------------
    $scope.fixedAssetMasterList = [];
    $scope.popUpList = [];
    $scope.fixedAssetMasterParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'FixedAsset',
        searchBy: "FixedAsset",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.fixedAssetMasterSearchPopup = function () {
        $scope.popUpUrl = "fixedassets/fixedassetmaster/GetListForDynamicPopupWithType?type=Machine";
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.fixedAssetMasterParameters)
                .then(function (result) {
                    $scope.fixedAssetMasterList = result.rows;
                    $scope.fixedAssetMasterParameters.total_count = result.total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'mastersearchpopup');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
        $scope.getPopUpData();
    };

    $scope.getFixedAssetMaster = function (data) {
        $scope.fixedAssetMaster = data;
        $scope.selectedRow = data.Id;
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
        $scope.machineTypeSelectedList = [];
        getDataByFixedAssetMaster(data.Id);
    };

    function getDataByFixedAssetMaster(fixedAssetMasterId) {
        $http({
            method: 'GET',
            url: '/FixedAssets/FixedAssetMasterMachineType/GetList?fixedAssetMasterId=' + fixedAssetMasterId
        }).then(function successCallback(response) {
            var resultData = response.data.rows;
            angular.forEach(resultData, function (item) {
                if (item.Id) {
                    $scope.machineTypeSelectedList.push(
                        {
                            Id: item.Id,
                            MachineTypeId: item.MachineTypeId,
                            Sequence: item.Sequence,
                            Code: item.Code,
                            MachineType: item.MachineType,
                            MachineClass: item.MachineClass,
                            Skill: item.Skill,
                            Flag: item.Flag,
                            Archive: false,
                            Active: true
                        }
                    );
                }
            });
            if ($scope.machineTypeSelectedList.length > 0) {
                $scope.tableShow = true;
            }
            else {
                $scope.tableShow = false;
            }
        });
    }
    //
    //FixedAssetMasterMachineType for modal
    $scope.addMachineType = function () {
        try {
            if ($scope.fixedAssetMaster.Id === null) {
                throw 'Please Select Fixed Asset Master';
            }
            $scope.searchByMachineTypeList = [];
            $scope.machineTypeParameters = {
                limit: 10,
                offset: 0,
                order: 'asc',
                sort: 'MachineType',
                searchBy: "MachineType",
                pageSize: 10,
                total_count: 0,
                search: null,
                serverPagination: true
            };
            $scope.popUpUrl = 'machines/machinetype/getmachinetypeexistid?machineTypeIds=' + isMachineTypeIdExistGrid($scope.machineTypeSelectedList);
            //baseService.init('machines/machinetype/getmachinetypeexistid?machineTypeIds=' + isMachineTypeIdExistGrid($scope.machineTypeSelectedList), null, 25, null, 'UserName', 'UserName');
            $scope.loadRegisterData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.machineTypeParameters)
                    .then(function (result) {
                        $scope.machineTypeList = result.rows;
                        if (baseService.arrayLength($scope.searchByMachineTypeList) === 0) {
                            baseService.getDDLSearchColumn(result.rows, $scope.searchByMachineTypeList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#machineTypePopUp')).modal('show');
            $scope.loadRegisterData();
        } catch (e) {
            // throw e;
            throw ShowResult(e, "failure");
        }
    };

    function isMachineTypeIdExistGrid(list) {
        $scope.machineTypeIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.machineTypeIds.push(list[i]['MachineTypeId']);
                }
            }
        }
        return JSON.stringify($scope.machineTypeIds);
    };
    $scope.machineTypeSelectdCloseListPopUp = function () {
        angular.forEach($scope.machineTypeList, function (item) {
            if (item.Flag) {
                $scope.machineTypeSelectedList.push(
                    {
                        MachineTypeId: item.Id,
                        Sequence: item.Sequence,
                        Code: item.Code,
                        MachineType: item.MachineType,
                        MachineClass: item.MachineClass,
                        Skill: item.Skill,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#machineTypePopUp')).modal('hide');
        if ($scope.machineTypeSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //End JobList for modal
    $scope.jobDescriptionSelectdCloseListPopUp = function () {
        angular.forEach($scope.jobDescriptionList, function (item) {
            if (item.Flag) {
                $scope.machineTypeSelectedList.push(
                    {
                        JobDescriptionId: item.Id,
                        FixedAssetMasterMachineTypeId: $scope.companyStructureSetup.Id,
                        JobDescriptionCategoryName: item.JobDescriptionCategoryName,
                        JobDescriptionSubCategoryName: item.JobDescriptionSubCategoryName,
                        JobDescriptionItemName: item.JobDescriptionItemName,
                        JobLevel: item.JobLevel,
                        PrimaryOrSecondary: item.PrimaryOrSecondary,
                        Frequency: item.Frequency,
                        NatureOfActivity: item.NatureOfActivity,
                        SystemOrManual: item.SystemOrManual,
                        EstimatedTimeRequired: item.EstimatedTimeRequired,
                        DocumentApplicable: item.DocumentApplicable,
                        TotalAttachment: item.TotalAttachment,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#jobDescriptionPopUp')).modal('hide');
        if ($scope.machineTypeSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    $scope.getPRJobDescription = function (id) {
        $http.get('/organizations/FixedAssetMasterMachineType/GetFixedAssetMasterMachineTypeJobDescriptionList?positionId=' + id)
            .then(function (response) {
                $scope.machineTypeSelectedList = response.data.rows;
                if ($scope.machineTypeSelectedList.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            });
    };

    // Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, id, MachineTypeId) {
        $scope.id = id;
        $scope.index = index;
        $scope.machineTypeId = MachineTypeId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.deleteMachineTypeSelectedList = function () {
        for (var i = 0; i < $scope.machineTypeSelectedList.length; i++) {
            if ($scope.machineTypeSelectedList[i].Id === null && $scope.machineTypeSelectedList[i].MachineTypeId === $scope.machineTypeId) {
                $scope.machineTypeSelectedList.splice($scope.index, 1);
            } else if ($scope.machineTypeSelectedList[i].Id === $scope.id && $scope.machineTypeSelectedList[i].MachineTypeId === $scope.machineTypeId) {
                $scope.machineTypeSelectedList.splice($scope.index, 1);
            }
            $scope.fixedAssetMasterMachineTypeDataList = [];
        }
        $scope.id = null;
        $scope.index = null;
        $scope.machineTypeId = null;
        if ($scope.machineTypeSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.Get = function (id) {
        $http.get('/organizations/FixedAssetMasterMachineType/GetById?id=' + id)
            .then(function (response) {
                $scope.companyStructureSetup = response.data;
                $scope.getCompanyStructurerRelation($scope.companyStructureSetup);
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                    $scope.Action = 'Update';
                }
            });
        $scope.getPRJobDescription(id);
    };

    function addForSaveList(list) {
        for (var i = 0; i < list.length; i++) {
            $scope.fixedAssetMasterMachineTypeDataList.push(
                {
                    Id: $scope.fixedAssetMasterMachineType.Id,
                    MachineTypeId: list[i].MachineTypeId,
                    FixedAssetMasterId: $scope.fixedAssetMaster.Id
                });
        }
    }

    $scope.Save = function () {
        try {
            if ($scope.machineTypeSelectedList.length < 1) {
                throw "Please select at least one machine type";
            }
            addForSaveList($scope.machineTypeSelectedList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fixedAssetMasterMachineTypeForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: "/fixedassets/fixedassetmastermachinetype/Create",
                        data: { 'fixedAssetMasterMachineType': $scope.fixedAssetMasterMachineTypeDataList, 'fixedAssetMasterId': $scope.fixedAssetMaster.Id },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.storeId = $scope.fixedAssetMaster.Id;
                            $scope.machineTypeSelectedList = [];
                            $scope.fixedAssetMasterMachineTypeDataList = [];
                            getDataByFixedAssetMaster($scope.storeId);
                        }
                    });
                    return true;
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: '/organizations/FixedAssetMasterMachineType/Edit',
                        data: { 'positionStructureSetup': $scope.companyStructureSetup, 'positionJobDescription': $scope.machineTypeSelectedList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            ClearFields();
                        }
                    });
                    return true;
                }
            }
        } catch (e) {
            throw ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: "/fixedassets/fixedassetmastermachinetype/Delete/?id=" + $scope.fixedAssetMaster.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ClearFields();
            }
        });
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    $scope.clearTbl = function () {
        if ($scope.machineTypeSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.clearTbl();
    function ClearFields() {
        $scope.Action = "Save";
        $scope.fixedAssetMaster = {};
        $scope.machineTypeSelectedList = [];
        $scope.fixedAssetMasterMachineTypeDataList = [];
        $scope.clearTbl();
    }
    // #region ReturnToRequiredTab
    function reDirectToRequiredTab() {
        if ($scope.companySSFormTab1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.companySSFormTab2.$invalid) {
            $scope.setTab(2);
        }
    }
    // #endregion
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
}