'use strict';
manpowerBudgetController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function manpowerBudgetController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = 'Manpower BudgetMaster';
    $scope.Action = 'Save';
    $scope.positionCode = false;
    $scope.index = -1;
    $scope.manPowerbudgetmasters = [];
    $scope.jobDescriptionList = [];
    $scope.jobDescriptionSelectedList = [];
    $scope.path = 'Organizations/ManpowerBudget/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.manpowerBudgetXLUrl = 'Organizations/manpowerbudget/manpowerbudgetreport';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Code', 'Code');
    $scope.manPowerbudgetmastersHead = [];
    $scope.getData = function (pageno) {
        if (baseService.isUndefinedOrNull($scope.manPowerbudgetmasterNew.CompanyId)) {
            $scope.positionDataList = [];
            ShowResult('Please select company.', 'failure');
        }
        else {
            $scope.entityList = [];
            $scope.positionList = [];
            $rootScope.parameters.companyId = $scope.manPowerbudgetmasterNew.CompanyId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.manPowerbudgetmasters = result.Rows;
                    if (baseService.arrayLength($scope.manPowerbudgetmastersHead) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.manPowerbudgetmastersHead);
                    }
                    ClearFields();
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }
    };

    $scope.searchByManPowerBdList = [];
    $scope.getCboEntityAndPositionRelationshipByCompanyGroupAndCompany = function (companyId) {
        cboService.getCboEntityAndPositionRelationshipByCompanyGroupAndCompany(null, companyId, function (result) {
            $scope.searchByManPowerBdList = result;
            $scope.searchByManPowerBdList.push(
                {
                    'name': 'Designation',
                    'value': 'Designation'
                },
                {
                    'name': 'Code',
                    'value': 'Code'
                },
                {
                    'name': 'Entity Code',
                    'value': 'EntityCode'
                },
                {
                    'name': 'Entity',
                    'value': 'Entity'
                },
                {
                    'name': 'Position Code',
                    'value': 'PositionCode'
                },
                {
                    'name': 'Position',
                    'value': 'Position'
                },
                {
                    'name': 'Male',
                    'value': 'Male'
                },
                {
                    'name': 'Female',
                    'value': 'Female'
                },
                {
                    'name': 'Total Number',
                    'value': 'TotalNumber'
                }
            );
        });
    };

    $scope.manPowerbudgetmaster = {
        Id: null,
        CompanyId: null,
        EntityId: null,
        PositionId: null,
        LineId: null,
        ShiftDefinationId: null,
        PayrollGroupId: null,
        Code: null,
        PositionCode: null,
        Male: 0,
        Female: 0,
        TotalNumber: 0,
        ROBudgetCode: null,
        ROBudgetName: null,
        PRBudgetCode: null,
        PRBudgetName: null,
        Remarks: null,
        Active: true,
        EmploymentType: null,
        IsOTEntitled: false,
        WorkGroupId: null,
        EmployeeLocationId: null,
        AttendanceGroupId: null,
        ResponsiblePerson: null,
        Email: null,
        Deployment:0,
        AccountsGroupId: null,
        IsRosterApplicable: false,
        IsScattedWeekOffApplicable:false,
        IsResidencePlan: false,
        IsTransportPlan: false
    };

    $scope.manpowerBudgetAllowance = {
        Id: null,
        ManpowerBudgetId: null,
        CurrencyId: null,
        EffectiveDate: null,
        MinimumSalary: null,
        MaximumSalary: null,
        SkillAllowance: null,
        ResponsibilityAllowance: null,
        Active: true,
    }

    $scope.manpowerBudgetDetail = {
        Id: null,
        ManpowerBudgetId: null,
        EffectiveDate: null,
        Male: 0,
        Female: 0,
        TotalNumber: 0,
        Active: true,
        TransportVacancy:0,
        ResidenceVacancy: 0,
        Deployment:0
    }
    $scope.manPowerbudgetmasterNew = Object.assign({}, $scope.manPowerbudgetmaster);

    $scope.manpowerBudgetKPIResponsible = {
        Id: null,
        ManpowerBudgetId: null,
        EffectiveDate: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        TeamLeaderId: null,
        TeamLeader: null,
        Remarks: null

    }
    $scope.manpowerBudgetKPIResponsibleNew = Object.assign({}, $scope.manpowerBudgetKPIResponsible);

    $scope.companyList = [];
    $scope.lineList = [];
    $scope.shiftList = [];
    $scope.entityList = [];
    $scope.currencyList = [];
    $scope.positionList = [];
    $scope.employeeLocationList = [];

    cboService.getEnumCbo('enum/GetEmploymentTypeCbo', function (result) {
        $scope.employmentTypeList = result;
    });

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    cboService.getCboCompanyGroupPayrollGroup(function (result) {
        $scope.payrollGroupList = result;
    });

    cboService.getEmployeeLocationCbo(function (result) {
        $scope.employeeLocationList = result;
    });

    $scope.getLineCbo = function (entityId) {
        cboService.getCboEntityLineById(entityId, function (result) {
            $scope.lineList = result;
        });
    };

    $scope.getShiftCbo = function (entityId) {
        cboService.getEntityPlantShiftCbo(entityId, function (result) {
            $scope.shiftList = result;
        });
    };

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    //$scope.getEntityCbo = function (companyId) {
    //    cboService.getCboProductionEntityByCompany(null, companyId, function (result) {
    //        $scope.entityList = result;
    //    });
    //};

    $scope.accountsGroupList = [];
    cboService.getAccountsGroupCbo(function (result) {
        $scope.accountsGroupList = result;
    });

    $scope.getCboPositionByEntityId = function (entityId) {
        cboService.getCboPositionByEntityId(entityId, function (result) {
            $scope.positionList = result;
        });
    };
    $scope.workGroupList = [];
    $scope.getCboWorkGroupWithPlant = function (plantId) {
        cboService.getCboWorkGroupListWithPlant(plantId, function (result) {
            $scope.workGroupList = result;
        });
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $http.get('Organizations/ManpowerBudget/GetManpowerBudgetById?id=' + id)
            .then(function (response) {
                $scope.manPowerbudgetmaster = response.data;
                $scope.manPowerbudgetmasterNew = Object.assign({}, $scope.manPowerbudgetmaster);
                //$scope.getEntityCbo($scope.manPowerbudgetmasterNew.CompanyId);
                $scope.getLineCbo($scope.manPowerbudgetmasterNew.EntityId);
                $scope.getShiftCbo($scope.manPowerbudgetmasterNew.EntityId);
                $scope.getCboPositionByEntityId($scope.manPowerbudgetmasterNew.EntityId);
                $scope.getEntityMapData($scope.manPowerbudgetmaster.EntityId);
                $scope.getPositionCode($scope.manPowerbudgetmaster.PositionId);
                $scope.getPMPBJobDescription($scope.manPowerbudgetmaster.Id);
                $scope.selectedEntityId = $scope.manPowerbudgetmaster.EntityId;
                $scope.manPowerbudgetmasterNew.EntityId = $scope.selectedEntityId;
                $scope.manPowerbudgetmasterNew.EntityName = $scope.manPowerbudgetmaster.Entity;
                $scope.selectedPositionId = $scope.manPowerbudgetmaster.PositionId;
                $scope.manPowerbudgetmasterNew.PositionId = $scope.selectedPositionId;
                $scope.manPowerbudgetmasterNew.PositionName = $scope.manPowerbudgetmaster.Position;

                if ($scope.manPowerbudgetmasterNew.IsProductionEntity) {
                    $scope.msg = "Production";
                } else {
                    $scope.msg = "Non Production";
                }

                $scope.Action = 'Update';
                $scope.getAllowance();
                $scope.getManpowerBudgetDetail();
                $scope.GetSavedAdditionalPlanData();
                $scope.GetSavedKPIResponsibleData();
                //$scope.GetCostCenterCboByCompanyandEntity($scope.manPowerbudgetmasterNew.EntityId);
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            },
                function (response) {
                    ShowResult(response, 'failure');
                });
    };

    // #region Get Employee
    $scope.employeeProfileList = [];
    $scope.employeeProfileDataList = [];
    $scope.employeeProfileParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Code',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.fieldName = '';
    $scope.employeeProfilePopUp = function (code) {
        if ($scope.manPowerbudgetmasterNew.CompanyId === null) {
            ShowResult('Company selection required.', 'failure');
        }
        baseService.setCurrentPage('employeeProfileDataList');
        $scope.employeeProfileUrl = 'Organizations/ManpowerBudget/getlist?companyId=' + $scope.manPowerbudgetmasterNew.CompanyId;

        $scope.getemployeeProfileData = function (pageno) {
            baseService.paginationBase($scope.employeeProfileUrl, pageno, $scope.employeeProfileParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        if (result.Rows[i].Code == $scope.manPowerbudgetmasterNew.Code) {
                            result.Rows[i].splice(i, 1);
                        }
                    }
                    $scope.employeeProfileDataList = result.Rows;
                    $scope.employeeProfileParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.employeeProfileList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.employeeProfileList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('show');
        $scope.getemployeeProfileData();
        $scope.fieldId = code;
    };

    $scope.selectBudgetdblClick = function (data) {
        $scope.manPowerbudgetmasterNew[$scope.fieldId] = data.Id;

        if ($scope.fieldId === 'ROBudgetCode') {
            $scope.manPowerbudgetmasterNew.ROBudgetCode = data.Id;
            $scope.manPowerbudgetmasterNew.ROBudgetCodeCode = data.Code;
        } else {
            $scope.manPowerbudgetmasterNew.PRBudgetCode = data.Id;
            $scope.manPowerbudgetmasterNew.PRBudgetCodeCode = data.Code;

        }
        $scope.fieldId = '';


        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.valueData = '';
    $scope.selectEmplyee = function (data) {
        $scope.valueData = data;
    };

    $scope.SelectEmployeeByButton = function () {
        if ($scope.valueData === '') {
            ShowResult('Please at first select row.', 'failure', 'employeeProfilePopUp');
            return;
        }
        $scope.selectEmployeedblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.closeEmployeeProfilePopUp = function () {
        $scope.employeeId = '';
        $scope.FullName = '';
        angular.element(document.querySelector('#employeeProfilePopUp')).modal('hide');
    };

    $scope.employeeProfileClear = function (field, name) {
        $scope.manPowerbudgetmasterNew[field] = null;
        $scope.manPowerbudgetmasterNew[name] = null;
    };

    //*********************** Position PopUp Start *************************************
    $scope.positionSearchList = [];
    $scope.positionDataList = [];
    $scope.positionSearch = [];
    $scope.positionUrl = 'Organizations/Position/querybyentityid';
    $scope.positionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionPopUp = function (entityId) {
        if (baseService.isUndefinedOrNull(entityId)) {
            $scope.positionDataList = [];
            ShowResult('Entity selection required.', 'failure');
        }
        else {
            $scope.positionParameters.entityId = entityId;
            $scope.getPositionData = function (pageno) {
                baseService.paginationBase($scope.positionUrl, pageno, $scope.positionParameters)
                    .then(function (response) {
                        $scope.positionDataList = response.Rows;
                        $scope.positionParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.positionSearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.positionDataList, $scope.positionSearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#positionPopUp')).modal('show');
            $scope.getPositionData();
        }
    };

    $scope.closePositionPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.getPositionCode = function (id) {
        $scope.positionData = [];
        $scope.positionSearch = [];
        $http({
            method: 'GET',
            url: 'Organizations/Position/get?companyId=' + $scope.manPowerbudgetmasterNew.CompanyId + '&id=' + id
        }).then(function successCallback(response) {
            $scope.positionData = [];
            $scope.positionData.push(response.data);
            baseService.getDDLSearchColumn($scope.positionData, $scope.positionSearch);
        });
    };

    $scope.selectPositionPopUp = function (data) {
        $scope.selectedPositionId = data.Id;
        $scope.getPRJobDescription($scope.selectedPositionId);
        $scope.manPowerbudgetmasterNew.PositionId = $scope.selectedPositionId;
        $scope.manPowerbudgetmasterNew.PositionName = data.UserName;
        $scope.getPositionCode($scope.selectedPositionId);
        angular.element(document.querySelector('#positionPopUp')).modal('hide');
    };

    $scope.clearPosition = function () {
        $scope.selectedPositionId = null;
        $scope.manPowerbudgetmasterNew.PositionId = null;
        $scope.manPowerbudgetmasterNew.PositionName = null;
        $scope.positionData = [];
        $scope.positionSearch = [];
    };
    //*********************** Position PopUp End *************************************

    //*********************** Entity PopUp Start *************************************
    $scope.entitySearchList = [];
    $scope.entityDataList = [];
    $scope.entitySearch = [];
    $scope.entityUrl = 'Organizations/entity/getlist?companyId=';
    $scope.entityParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.entityPopUp = function (companyId) {
        if (baseService.isUndefinedOrNull(companyId)) {
            $scope.entityDataList = [];
            ShowResult('Company selection required.', 'failure');
        }
        else {
            $scope.entityParameters.companyId = companyId;
            $scope.getEntityData = function (pageno) {
                baseService.paginationBase($scope.entityUrl + companyId, pageno, $scope.entityParameters)
                    .then(function (response) {
                        $scope.entityDataList = response.Rows;
                        $scope.entityParameters.total_count = response.Total;
                        if (baseService.arrayLength($scope.entitySearchList) === 0) {
                            baseService.getDDLSearchColumn($scope.entityDataList, $scope.entitySearchList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#entityPopUp')).modal('show');
            $scope.getEntityData();
        }
    };

    $scope.closeEntityPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.msg = null;
    $scope.selectEntityPopUp = function (entity) {
        $scope.selectedEntityId = entity.Id;
        $scope.manPowerbudgetmasterNew.EntityId = $scope.selectedEntityId;
        $scope.manPowerbudgetmasterNew.EntityName = entity.UserName;
        if (entity.IsProductionEntity) {
            $scope.msg = "Production";
        } else {
            $scope.msg = "Non Production";
        }
        // Nullify current selected position
        $scope.selectedPositionId = null;
        $scope.manPowerbudgetmasterNew.PositionId = null;
        $scope.manPowerbudgetmasterNew.PositionName = null;
        $scope.getLineCbo($scope.selectedEntityId);
        $scope.getShiftCbo($scope.selectedEntityId);
        $scope.getEntityMapData($scope.selectedEntityId);
        //$scope.GetCostCenterCboByCompanyandEntity($scope.manPowerbudgetmasterNew.EntityId);
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.getEntityMapData = function (id) {
        $scope.entityData = [];
        $scope.entitySearch = [];
        $http({
            method: 'GET',
            url: 'Organizations/entity/get?id=' + id
        }).then(function successCallback(response) {
            $scope.entityData = [];
            $scope.entityData.push(response.data);
            $scope.getCboWorkGroupWithPlant(response.data.PlantId);
            baseService.getDDLSearchColumn($scope.entityData, $scope.entitySearch);
        });
    };

    $scope.getPRJobDescription = function (id) {
        $http.get('Organizations/Position/getpositionjobdescriptionlist?positionId=' + id)
            .then(function (response) {
                $scope.jobDescriptionSelectedList = response.data.Rows;
                if ($scope.jobDescriptionSelectedList.length > 0) {
                    angular.forEach($scope.jobDescriptionSelectedList, function (element, i) {
                        element.Id = null;
                    });
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            });
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

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'JobDescriptionCategoryName',
        searchBy: 'JobDescriptionCategoryName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.addJobDescription = function () {
        $scope.searchByJobDescriptionList = [
            {
                'name': 'Category',
                'value': 'JobDescriptionCategoryName'
            },
            {
                'name': 'Sub Category',
                'value': 'JobDescriptionSubCategoryName'
            },
            {
                'name': 'Item',
                'value': 'JobDescriptionItemName'
            },
            {
                'name': 'Level',
                'value': 'JobLevel'
            },
            {
                'name': 'Primary/Secondary',
                'value': 'PrimaryOrSecondary'
            },
            {
                'name': 'Frequency',
                'value': 'Frequency'
            }
        ];

        $scope.popUpUrl = 'employees/jobdescription/getjobdescriptionlist?jobDescriptionIds=' + isJobDescriptionIdExistGrid($scope.jobDescriptionSelectedList);
        $scope.getJDData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.jobDescriptionList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#jobDescriptionPopUp')).modal('show');
        $scope.getJDData();
    };

    function isJobDescriptionIdExistGrid(list) {
        $scope.jobDescriptionIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.jobDescriptionIds.push(list[i]['JobDescriptionId']);
                }
            }
        }
        return JSON.stringify($scope.jobDescriptionIds);
    }

    //End JobList for modal
    $scope.jobDescriptionSelectdCloseListPopUp = function () {
        angular.forEach($scope.jobDescriptionList, function (item) {
            if (item.Flag) {
                $scope.jobDescriptionSelectedList.push(
                    {
                        JobDescriptionId: item.Id,
                        PositionId: $scope.manPowerbudgetmasterNew.Id,
                        JobDescriptionCategoryName: item.JobDescriptionCategoryName,
                        JobDescriptionSubCategoryName: item.JobDescriptionSubCategoryName,
                        JobDescriptionItemName: item.JobDescriptionItemName,
                        JobLevel: item.JobLevel,
                        PrimaryOrSecondary: item.PrimaryOrSecondary,
                        Frequency: item.Frequency,
                        NatureOfActivity: item.NatureOfActivity,
                        SystemOrManual: item.SystemOrManual,
                        EstimatedTimeRequired: item.EstimatedTimeRequired,
                        Flag: item.Flag,
                        Archive: false,
                        Active: true
                    }
                );
            }
        });
        angular.element(document.querySelector('#jobDescriptionPopUp')).modal('hide');
        if ($scope.jobDescriptionSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.getPMPBJobDescription = function (id) {
        $http.get('Organizations/manpowerbudget/GetManpowerBudgetList?manpowerBudgetId=' + id)
            .then(function (response) {
                $scope.jobDescriptionSelectedList = response.data.Rows;
                if ($scope.jobDescriptionSelectedList.length > 0) {
                    $scope.tableShow = true;
                }
                else {
                    $scope.tableShow = false;
                }
            });
    };

    // Deleting Rows from CompanyDepartmentList
    $scope.valuePassInDelModal = function (index, JobDescriptionId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.JobDescriptionId = JobDescriptionId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data?';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]?';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.deletePRJobDescriptionList = function () {
        for (var i = 0; i < $scope.jobDescriptionSelectedList.length; i++) {
            if ($scope.jobDescriptionSelectedList[i].Id === null && $scope.jobDescriptionSelectedList[i].JobDescriptionId === $scope.JobDescriptionId) {
                $scope.jobDescriptionSelectedList.splice($scope.index, 1);
            }
            else if ($scope.jobDescriptionSelectedList[i].Id !== null && $scope.jobDescriptionSelectedList[i].JobDescriptionId === $scope.JobDescriptionId)
                $scope.jobDescriptionSelectedList[i].Archive = true;
        }
        $scope.id = null;
        $scope.index = null;
        $scope.JobDescriptionId = null;
        if ($scope.jobDescriptionSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    function reDirectToRequiredTab() {
        if ($scope.formTab1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.formTab2.$invalid) {
            $scope.setTab(2);
        }
    }

    $scope.countTotalNumber = function (male, female) {
        if (baseService.isUndefinedOrNull(male))
            male = 0;
        else if (baseService.isUndefinedOrNull(female))
            female = 0;
        $scope.manpowerBudgetDetail.TotalNumber = parseInt(male) + parseInt(female);
    };

    $scope.manPowerBudgetDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'CONVERT(DATETIME, EffectiveDate, 106)',
        searchBy: 'ManpowerBudgetId',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.manpowerBudgetDetailList = [];
    $scope.getManpowerBudgetDetail = function () {
        $scope.popUpUrl = 'Organizations/ManpowerBudget/QueryDetail?manpowerBudgetId=' + $scope.manPowerbudgetmasterNew.Id;
        baseService.setCurrentPage('manpowerBudgetAllowanceList');
        $scope.getManpowerBudgetDetailData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.manPowerBudgetDetailParameters)
                .then(function (result) {
                    $scope.manpowerBudgetDetailList = result.Rows;
                    $scope.manPowerBudgetDetailParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getManpowerBudgetDetailData();
    };

    $scope.addManpowerBudgetDetail = function () {
        if (!baseService.isUndefinedOrNull($scope.manpowerBudgetDetail.EffectiveDate) && !baseService.isUndefinedOrNull($scope.manpowerBudgetDetail.Male) && !baseService.isUndefinedOrNull($scope.manpowerBudgetDetail.Female)) {
            if ($scope.ActionManpowerBudgetDetail === 'Add') {
                if (checkExist($scope.manpowerBudgetDetailList, $scope.manpowerBudgetDetail.EffectiveDate, $scope.manpowerBudgetDetail.Male, $scope.manpowerBudgetDetail.Female) === false) {
                    $scope.manpowerBudgetDetail.EffectiveDate = $filter('dateFiltering')($scope.manpowerBudgetDetail.EffectiveDate);
                    $scope.manpowerBudgetDetailList.push($scope.manpowerBudgetDetail)
                } else {
                    return ShowResult("This Combination is already exist in list", 'failure');
                }
            } else {
                $scope.manpowerBudgetDetailList[$scope.detailIndex] = $scope.manpowerBudgetDetail;
            }
            $scope.manpowerBudgetDetail = {};
            $scope.detailIndex = -1;
            $scope.ActionManpowerBudgetDetail = 'Add'
        }
    }
    function checkExist(list, date, male, female) {
        angular.forEach(list, function (item) {
            if (item.EffectiveDate === date && item.Male === male && item.Female === female) {
                return true;
            }
        });
        return false;
    }

    $scope.allowanceParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'CONVERT(DATETIME, EffectiveDate, 106)',
        searchBy: 'ManpowerBudgetId',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.manpowerBudgetAllowanceList = [];
    $scope.getAllowance = function () {
        $scope.popUpUrl = 'Organizations/ManpowerBudget/QueryAllowance?manpowerBudgetId=' + $scope.manPowerbudgetmasterNew.Id;
        baseService.setCurrentPage('manpowerBudgetAllowanceList');
        $scope.getAllowanceData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.allowanceParameters)
                .then(function (result) {
                    $scope.manpowerBudgetAllowanceList = result.Rows;
                    $scope.allowanceParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getAllowanceData();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.formTabC.$valid && $scope.formTab1.$valid && $scope.formTab2.$valid) {
            angular.copy($scope.manPowerbudgetmasterNew, $scope.manPowerbudgetmaster);
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'manpowerBudget': $scope.manPowerbudgetmaster,
                        'manpowerBudgetJobDescription': $scope.jobDescriptionSelectedList,
                        'manpowerBudgetDetailList': $scope.manpowerBudgetDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields();
                        $scope.getManpowerBudgetDetail();
                        $scope.clearManpowerBudgetDetail();
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
                    data: {
                        'manpowerBudget': $scope.manPowerbudgetmaster,
                        'manpowerBudgetJobDescription': $scope.jobDescriptionSelectedList,
                        'manpowerBudgetDetailList': $scope.manpowerBudgetDetailList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.getData();
                            ClearFields();
                            $scope.getManpowerBudgetDetail();
                            $scope.clearManpowerBudgetDetail();
                        }
                        ClearFields();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    };

    $scope.ActionAllowance = 'Save';
    $scope.SaveAllowance = function () {
        if (!baseService.isUndefinedOrNull($scope.manpowerBudgetAllowance.MinimumSalary))
            if (parseInt($scope.manpowerBudgetAllowance.MaximumSalary) < parseInt($scope.manpowerBudgetAllowance.MinimumSalary)) {
                return ShowResult('Maximum salary must be greater than minimum salary.', 'failure');
            }
        var date = new Date($scope.manpowerBudgetAllowance.EffectiveDate).getDate();
        if (date > 1) {
            return ShowResult('Selected date must be 1st day of month.', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if (baseService.isUndefinedOrNull($scope.manPowerbudgetmasterNew.Id))
            return ShowResult('Manpower Budget can not be empty', 'failure');
        $scope.manpowerBudgetAllowance.ManpowerBudgetId = $scope.manPowerbudgetmasterNew.Id
        if ($scope.companySSFormTab4.$valid) {
            if ($scope.ActionAllowance === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/ManpowerBudget/CreateAllowance',
                    data: { 'manpowerBudgetAllowance': $scope.manpowerBudgetAllowance },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getAllowance();
                        $scope.clearAllowance();
                    }
                });
                return true;
            }
            else if ($scope.ActionAllowance === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/ManpowerBudget/EditAllowance',
                    data: { 'manpowerBudgetAllowance': $scope.manpowerBudgetAllowance },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getAllowance();
                        $scope.clearAllowance();
                    }
                });
                return true;
            }
        }
    };

    $scope.ActionManpowerBudgetDetail = 'Add';
    $scope.SaveManpowerBudgetDetail = function () {
        $scope.$broadcast('show-errors-check-validity');
        if (baseService.isUndefinedOrNull($scope.manPowerbudgetmasterNew.Id))
            return ShowResult('ManPower Budget can not be empty', 'failure');
        $scope.manpowerBudgetDetail.ManpowerBudgetId = $scope.manPowerbudgetmasterNew.Id
        if ($scope.companySSFormTab3.$valid) {
            if ($scope.ActionAllowance === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/ManpowerBudget/CreateDetail',
                    data: {
                        'manpowerBudgetDetail': $scope.manpowerBudgetDetail
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getManpowerBudgetDetail();
                        $scope.clearManpowerBudgetDetail();
                    }
                });
                return true;
            }
            else if ($scope.ActionAllowance === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/ManpowerBudget/EditDetail',
                    data: { 'manpowerBudgetDetail': $scope.manpowerBudgetDetail },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getManpowerBudgetDetail();
                        $scope.clearManpowerBudgetDetail();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.manPowerbudgetmasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.manPowerbudgetmasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.manPowerbudgetmasters.splice($scope.index, 1);
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

    $scope.getManpowerBudgetDetailUpdate = function (data, index) {
        $scope.detailIndex = index;
        $scope.ActionManpowerBudgetDetail = 'Update';
        $scope.manpowerBudgetDetail = data;
    }

    $scope.manpowerDetailId = null;
    $scope.manpowerDetailIndex = -1;
    $scope.valuePassInManpowerDetailDelModal = function (data, index) {
        $scope.manpowerDetailId = data.Id;
        $scope.manpowerDetailTempInfo = data;
        $scope.manpowerDetailIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete permanently?';
        angular.element(document.querySelector('#confirmgenericDetailPopUp')).modal('show');
    };

    $scope.removeDetailRow = function () {
        for (var i = 0; i < $scope.manpowerBudgetDetailList.length; i++) {
            if ($scope.manpowerBudgetDetailList[i].Id === null && $scope.manpowerBudgetDetailList[i].EffectiveDate === $scope.manpowerDetailTempInfo.EffectiveDate && $scope.manpowerBudgetDetailList[i].Male === $scope.manpowerDetailTempInfo.Male && $scope.manpowerBudgetDetailList[i].Female === $scope.manpowerDetailTempInfo.Female && $scope.manpowerBudgetAllowanceList[i].TotalNumber === $scope.manpowerDetailTempInfo.TotalNumber) {
                $scope.manpowerBudgetDetailList.splice($scope.manpowerDetailIndex, 1);
            }
            else if ($scope.manpowerBudgetDetailList[i].Id !== null && $scope.manpowerBudgetDetailList[i].Id === $scope.manpowerDetailId)
                $scope.deleteDetail($scope.manpowerDetailId, i);
        }
        $scope.manpowerDetailId = null;
        $scope.manpowerDetailIndex = -1;
    };

    $scope.deleteDetail = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: $scope.path + '/DeleteDetail',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.manpowerBudgetDetailList.length; i++) {
                        if ($scope.manpowerBudgetDetailList[i].Id === id) {
                            $scope.manpowerBudgetDetailList.splice(i, 1);
                            break;
                        }
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.clearManpowerBudgetDetail = function () {
        $scope.manpowerBudgetDetail = {};
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        var companyId = $scope.manPowerbudgetmasterNew.CompanyId;
        $scope.manPowerbudgetmaster = {};
        $scope.manpowerBudgetDetailList = [];
        $scope.manpowerBudgetAllowanceList = [];
        $scope.manPowerbudgetmasterNew = {};
        $scope.manPowerbudgetmasterNew.CompanyId = companyId;
        $scope.manPowerbudgetmasterNew.Active = true;
        $scope.jobDescriptionSelectedList = [];
        $scope.msg = null;
        $scope.tableShow = false;
        $scope.IsRosterApplicable = false;
        $scope.IsScattedWeekOffApplicable = false;
        $scope.clearPosition();
        $scope.clearEntity();
        $scope.SavedAdditionalPlanList = [];
    }

    $scope.selectMessage = '';
    $scope.manpowerBudgetReport = function () {
        if ($scope.manPowerbudgetmasterNew.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else {
            $scope.selectMessage = '';
            location.href = 'Organizations/manpowerbudget/manpowerbudgetreport?companyId=' + $scope.manPowerbudgetmasterNew.CompanyId;
        }
    };

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    $scope.AttendanceGroupList = [];
    $scope.getAttendanceGroup = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetAttendanceGroup",
        }).then(function successCallback(response) {
            $scope.AttendanceGroupList = response.data;
        });
    }
    $scope.getAttendanceGroup();

    //$scope.CostCenterList = [];
    //$scope.GetCostCenterCboByCompanyandEntity = function (EntityId) {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + "GetCostCenterCbo?CompanyId="+$scope.manPowerbudgetmasterNew.CompanyId+ '&EntityId=' + EntityId,
    //    }).then(function successCallback(response) {
    //        $scope.CostCenterList = response.data;
    //    });
    //}
   

    $scope.employeeList = [];
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCodeNumeric',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getEmployeeListUrl = 'employees/EmployeeInformation/GetWithoutUserEmployeeList';
    $scope.ShowEmployeeListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            baseService.paginationBase($scope.getEmployeeListUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeedblClick = function (data) {
        $scope.manPowerbudgetmasterNew.ResponsiblePerson = data.SystemID;
        $scope.manPowerbudgetmasterNew.Responsible = data.EmployeeName;
        //$scope.userNew.EmployeeCode = data.EmployeeCode;
        //$scope.userNew.Active = true;
        //$scope.userNew.Email = data.EmailId;
        //$scope.userNew.Phone = data.CellPhnNo;
        //$scope.userNew.DateOfBirth = $filter('dateFiltering')(data.DateOfBirth);
        //$scope.imageSrc = virtualPath.EmployeePic + data.Image;
        //$scope.fullName = true;
        //$scope.dOB = true;
        //$scope.employeeData = '';
        //setUserImage(data);
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    //#region AdditionalPlan

    $scope.ManpowerBudgetAdditionalPlan = {
        Id: null,
        ManpowerBudgetId: null,
        FromDate: null,
        ToDate: null,
        AdditionalPlan: 0,
        Remarks: null
    }
    $scope.ManpowerBudgetAdditionalPlanNew = Object.assign({}, $scope.ManpowerBudgetAdditionalPlan);

    $scope.SaveAdditional = function () {
        try {
            $scope.ManpowerBudgetAdditionalPlanNew.ManpowerBudgetId = $scope.manPowerbudgetmasterNew.Id;
            if (baseService.isUndefinedOrNull($scope.ManpowerBudgetAdditionalPlanNew.ManpowerBudgetId)) {
                throw "Manpower Budget is required.";
            }
            if (baseService.isUndefinedOrNull($scope.ManpowerBudgetAdditionalPlanNew.FromDate)) {
                throw "From Date is required.";
            }

            if (baseService.isUndefinedOrNull($scope.ManpowerBudgetAdditionalPlanNew.ToDate)) {
                throw "ToDate is required.";
            }
            if (baseService.isUndefinedOrNull($scope.ManpowerBudgetAdditionalPlanNew.AdditionalPlan)) {
                throw "Additional Plan is required.";
            }
            $http({
                method: 'POST',
                url: '/Organizations/ManpowerBudget/CreateAdditional',
                data: { 'data': $scope.ManpowerBudgetAdditionalPlanNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearAdditionalPlan();
                    $scope.GetSavedAdditionalPlanData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.SaveKPIResponsible = function () {
        try {
            $scope.manpowerBudgetKPIResponsibleNew.ManpowerBudgetId = $scope.manPowerbudgetmasterNew.Id;
            if (baseService.isUndefinedOrNull($scope.manpowerBudgetKPIResponsibleNew.ManpowerBudgetId)) {
                throw "Manpower Budget is required.";
            }
            $http({
                method: 'POST',
                url: '/Organizations/ManpowerBudget/CreateKPIResponsible',
                data: { 'data': $scope.manpowerBudgetKPIResponsibleNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedKPIResponsibleData();
                    KPIClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.ClearAdditionalPlan = function () {
        $scope.ManpowerBudgetAdditionalPlan = {
            Id: null,
            ManpowerBudgetId: null,
            FromDate: null,
            ToDate: null,
            AdditionalPlan: 0,
            Remarks: null
        }
        $scope.ManpowerBudgetAdditionalPlanNew = Object.assign({}, $scope.ManpowerBudgetAdditionalPlan);
    }

    $scope.ClearKPI = function () {
        KPIClearFields();
    };

    function KPIClearFields() {
        $scope.Action = "Save";
        $scope.manpowerBudgetKPIResponsibleNew = Object.assign({}, $scope.manpowerBudgetKPIResponsible);
    }

    $scope.SavedAdditionalPlanList = [];
    $scope.GetSavedAdditionalPlanData = function () {
        $http.get('Organizations/ManpowerBudget/GetSavedAdditionalPlanData?masterId=' + $scope.manPowerbudgetmasterNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SavedAdditionalPlanList = response.data;
                }
            });
    }

    $scope.EditPlanAdditionalPlan = function (obj) {
        obj.data.FromDate = $filter('dateFiltering')(new Date(obj.data.FromDate), 'dd-MM-yyyy');
        obj.data.ToDate = $filter('dateFiltering')(new Date(obj.data.ToDate), 'dd-MM-yyyy');
        $scope.ManpowerBudgetAdditionalPlanNew = Object.assign({}, obj.data);
    }

    $scope.manpowerBudgetKPIResponsibleList = [];
    $scope.GetSavedKPIResponsibleData = function () {
        $http.get('Organizations/ManpowerBudget/GetSavedKPIResponsibleData?masterId=' + $scope.manPowerbudgetmasterNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.manpowerBudgetKPIResponsibleList = response.data;
                }
            });
    }

    $scope.GetKPIDetails = function (obj) {
        obj.data.EffectiveDate = $filter('dateFiltering')(new Date(obj.data.EffectiveDate), 'dd-MM-yyyy');
        $scope.manpowerBudgetKPIResponsibleNew = Object.assign({}, obj.data);
    }
 
    $scope.selectResponsiblePerson = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.manpowerBudgetKPIResponsibleNew.ResponsiblePersonId = e.data.SystemId;
        $scope.manpowerBudgetKPIResponsibleNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectTeamLeader = function () {
        $scope.getTeamLeader();
        angular.element(document.querySelector('#TeamLeaderPopup')).modal('show');
    }

    $scope.TeamLeaderList = [];
    $scope.getTeamLeader = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetTeamLeader',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.TeamLeaderList = resp.data;
        });
    }

    $scope.doubleTeamLeader = function (e) {
        $scope.manpowerBudgetKPIResponsibleNew.TeamLeaderId = e.data.SystemId;
        $scope.manpowerBudgetKPIResponsibleNew.TeamLeader = e.data.EmployeeName;
        angular.element(document.querySelector('#TeamLeaderPopup')).modal('hide');
    }

    $scope.closeTeamLeaderPopUp = function () {
        angular.element(document.querySelector('#TeamLeaderPopup')).modal('hide');
    }
    //#endregion

}