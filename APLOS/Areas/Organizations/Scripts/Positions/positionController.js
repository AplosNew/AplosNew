'use strict';
positionController.$inject = ['commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', 'cboService'];
function positionController(commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile, cboService) {
    $rootScope.title = 'Position Setup';
    $scope.Action = 'Save';
    $scope.jobDescriptionList = [];
    $scope.jobDescriptionSelectedList = [];
    var url = 'Organizations/Position/getlist';
    $scope.dataList = [];
    $scope.fieldDataList = [];
    $scope.processes = [];
    $scope.dataListParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getData = function (pageno) {
        baseService.paginationBase(url, pageno, $scope.dataListParameters)
            .then(function (response) {
                $scope.dataList = response.Rows;
                $scope.dataListParameters.total_count = response.Total;
                if (baseService.arrayLength($scope.fieldDataList) === 0) {
                    baseService.getDDLSearchColumn(response.Rows, $scope.fieldDataList);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.getData();
    $scope.companyStructureSetup = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        DesignationId: null,
        UserName: null,
        Code: null,
        OldRefCode: null,
        EffectiveDate: null,
        EffectiveDateUpTo: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsDirect: false,
        TaskManagementApplicable: false,
        IsHandoverDays: false,
        HandoverDays: 0,
        MinimumSalary: 0,
        MaximumSalary: 0,
        PaymentLink: null,
        Activity: null,
        UserDefineGroup1: null,
        UserDefineGroup2: null,
        UserDefineGroup3: null,
        UserDefineGroup4: null,
        DirectManpowerCost: false,
        CostCenterId: null,
        PerformanceGroupId: null,
        PhysicalVarification: false,
        UserReportGroup: null,
        ProcessId: null,
        GoodWorkPositionCodeId: null,
        GoodWorkPositionCode: null
    };

    $scope.positionAllowance = {
        Id: null,
        PositionId: null,
        CurrencyId: null,
        EffectiveDate: null,
        MinimumSalary: null,
        MaximumSalary: null,
        SkillAllowance: null,
        ResponsibilityAllowance: null,
        Active: true
    };

    cboService.getEnumCbo('enum/GetPaymentLinkCbo', function (result) {
        $scope.paymentLinkList = result;
    });

    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.DivisionList = result;
    });

    cboService.getCboSubDivisionByCompanyGroup(null, function (result) {
        $scope.SubDivisionList = result;
    });

    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.DepartmentList = result;
    });

    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.SectionList = result;
    });

    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.SubSectionList = result;
    });

    cboService.getCompanyGroupCurrencyCbo(null, function (result) {
        $scope.currencyList = result;
    });

    cboService.getbyDesignationMasterCbo(function (result) {
        $scope.designationList = result;
    });

    $scope.CostCenterList = [];
    cboService.getCostCenterCbo(function (result) {
        $scope.CostCenterList = result;
    });

    $scope.PerformanceGroupList = [];
    cboService.getPerformanceGroupListCbo(function (result) {
        $scope.PerformanceGroupList = result;
    });

    //JobList for modal
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
        $scope.popUpUrl = 'employees/jobdescription/getjobdescriptionlist/?jobDescriptionIds=' + isJobDescriptionIdExistGrid($scope.jobDescriptionSelectedList);
        $scope.getData = function (pageno) {
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
        $scope.getData();
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
                        PositionId: $scope.companyStructureSetup.Id,
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
        if ($scope.jobDescriptionSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };

    $scope.getPRJobDescription = function (id) {
        $http.get('Organizations/Position/GetPositionJobDescriptionList?positionId=' + id)
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
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
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

    /***********Allowance*****************/
    $scope.allowanceParameters = {
        limit: 10,
        offset: 0,
        order: 'DESC',
        sort: 'CONVERT(DATETIME, EffectiveDate, 106)',
        searchBy: 'CurrencyCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.positionAllowanceList = [];
    $scope.getAllowanceData = function (pageno) {
        baseService.paginationBase('Organizations/Position/QueryAllowance?positionId=' + $scope.companyStructureSetup.Id, pageno, $scope.allowanceParameters)
            .then(function (result) {
                $scope.positionAllowanceList = result.Rows;
                $scope.allowanceParameters.total_count = result.Total;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Get = function (id) {
        $http.get('Organizations/Position/GetById?id=' + id)
            .then(function (response) {
                $scope.companyStructureSetup = response.data;
                $scope.getCompanyStructurerRelation($scope.companyStructureSetup);
                $scope.getAllowanceData();
                $scope.getPRJobDescription(id);
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                    $scope.Action = 'Update';
                }
            });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        reDirectToRequiredTab();
        if ($scope.companySSFormTab1.$valid && $scope.companySSFormTab2.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/Position/Create',
                    data: { 'positionStructureSetup': $scope.companyStructureSetup, 'positionJobDescription': $scope.jobDescriptionSelectedList },
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
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/Position/Edit',
                    data: { 'positionStructureSetup': $scope.companyStructureSetup, 'positionJobDescription': $scope.jobDescriptionSelectedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.setCurrentPage('dataList');
                        $scope.getData();
                        ClearFields();
                    }
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.companyStructureSetup.Id)) {
            $http({
                method: 'POST',
                url: 'Organizations/Position/Delete/' + $scope.companyStructureSetup.Id,
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
            });
        } else {
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
        $scope.companyStructureSetup = {};
        $scope.jobDescriptionSelectedList = [];
        $scope.positionAllowanceList = [];
        $scope.tableShow = false;
        $scope.companyStructureSetup.Active = true;
    }

    function reDirectToRequiredTab() {
        if ($scope.companySSFormTab1.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.companySSFormTab2.$invalid) {
            $scope.setTab(2);
        }
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
    $scope.getCompanyStructurerRelation = function (data) {
        $scope.left = '';
        $scope.right = '';
        $scope.Action = 'Save';
        $http.get('Organizations/PositionRelationship/getcompanystructurerelationlist')
            .then(function (response) {
                var obj = response.data.Rows;
                if (obj !== null) {
                    angular.forEach(obj, function (item, i) {
                        var dynamicHtml = '';
                        dynamicHtml = '<select tabindex="' + item.Sequence + '" ng-model="companyStructureSetup.' + item.StandardName + 'Id" class="form-control" ng-options="item.Value as item.Text for item in ' + item.StandardName + 'List" required name="' + item.StandardName + '"><option value=""></option></select>';

                        if (i % 2 === 0) {
                            $scope.left += '<div class="form-group" show-errors>' +
                                '<label class="col-sm-4 control-label">' + item.UserName + '<sup>*</sup></label>' +
                                '<div class="col-sm-8 show-message"><div class="select-style">' + dynamicHtml + '</div></div></div>';
                        }
                        else {
                            $scope.right += '<div class="form-group" show-errors>' +
                                '<label class="col-sm-4 control-label">' + item.UserName + '<sup>*</sup></label>' +
                                '<div class="col-sm-8 show-message"><div class="select-style">' + dynamicHtml + '</div></div></div>';
                        }
                    });

                    $scope.left +=
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">Designation<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message">' +
                        '<div class="select-style">' +
                        '<select tabindex="6" ng-model="companyStructureSetup.DesignationId" class="form-control" ng-options="item.Value as item.Text for item in designationList" required name="Designation"><option value=""></option></select>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">Code<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message">' +
                        '<input tabindex="8" type="text" maxlength="10" ng-model="companyStructureSetup.Code" required  name="Code" class="form-control">' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Handover Days Required</label>' +
                        '<div class="col-sm-2">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="10" type="checkbox" ng-model="companyStructureSetup.IsHandoverDays">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +
                        '<label class="col-sm-3 control-label">Handover Days</label>' +
                        '<div class="col-sm-3 show-message">' +
                        '<input tabindex="13" only-numbers type="text" class="form-control" ng-model="companyStructureSetup.HandoverDays" name="HandoverDays">' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Cost Center</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="select-style">' +
                        '<select name="CostCenter" ng-model="companyStructureSetup.CostCenterId" class="col-sm-3 form-control" ng-options="item.Value as item.Text for item in CostCenterList"><option></option></select>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Performance Group</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="select-style">' +
                        '<select name="PerformanceGroup" ng-model="companyStructureSetup.PerformanceGroupId" class="col-sm-3 form-control" ng-options="item.Value as item.Text for item in PerformanceGroupList"><option></option></select>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Description</label>' +
                        '<div class="col-sm-8">' +
                        '<textarea tabindex="12" maxlength="250" class="form-control" Rows="3" ng-model="companyStructureSetup.Description"></textarea>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Position Category</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="7" maxlength="100" type="text" ng-model="companyStructureSetup.PositionCategory" class="form-control">' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Is Machine</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="checkbox-site">' +
                        '<label>' +
                        '<input type="checkbox" ng-model="companyStructureSetup.IsMachine" tabindex="10">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span>' +
                        '</label>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +


                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Machine Name</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="7" maxlength="100" type="text" ng-model="companyStructureSetup.MachineName" class="form-control">' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Direct Manpower</label>' +
                        '<div class="col-sm-2">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="14" type="checkbox" ng-model="companyStructureSetup.IsDirect">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +
                        '<label class="col-sm-4 control-label">Direct Manpower Cost</label>' +
                        '<div class="col-sm-2">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="16" type="checkbox" ng-model="companyStructureSetup.DirectManpowerCost">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Task Management Applicable</label>' +
                        '<div class="col-sm-2">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="16" type="checkbox" ng-model="companyStructureSetup.TaskManagementApplicable">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +
                        '<label class="col-sm-4 control-label">Active</label>' +
                        '<div class="col-sm-2">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="16" type="checkbox" ng-model="companyStructureSetup.Active">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +

                        '</div>';

                    $scope.right +=
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Old Ref Code</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="7" maxlength="10" type="text" ng-model="companyStructureSetup.OldRefCode" class="form-control">' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">User Name<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message">' +
                        '<input tabindex="9" type="text" maxlength="100" ng-model="companyStructureSetup.UserName" class="form-control" required name="User name">' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">Payment Link<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message">' +
                        '<div class="select-style">' +
                        '<select tabindex="11" ng-model="companyStructureSetup.PaymentLink" class="form-control" ng-options="item.Value as item.Text for item in paymentLinkList" required name="PaymentLink"><option value=""></option></select>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Physical Varification</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="checkbox-site">' +
                        '<label><input tabindex="10" type="checkbox" ng-model="companyStructureSetup.PhysicalVarification">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Remarks</label>' +
                        '<div class="col-sm-8">' +
                        '<textarea tabindex="15" maxlength="250" class="form-control" Rows="3" ng-model="companyStructureSetup.Remarks"></textarea>' +
                        '</div>' +
                        '</div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Activity</label>' +
                        '<div class="col-sm-8">' +
                        '<textarea tabindex="17" maxlength="200" class="form-control" Rows="1" ng-model="companyStructureSetup.Activity"></textarea>' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">User Report Group</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="9" type="text" maxlength="100" ng-model="companyStructureSetup.UserReportGroup" class="form-control" name="User Report Group">' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Process</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="input-group">' +
                        '<input type="text" name="Process" ng-model="companyStructureSetup.ProcessName" class="form-control" readonly>' +
                        '<span class="input-group-btn">' +
                        '<button name="submit" ng-click="processPopUp()" class="btn single-small-btn"><i class="cr-icon glyphicon glyphicon-search"></i></button>' +
                        '</span>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +


                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Good Work Position Code</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="input-group">' +
                        '<input type="text" name="Good Work Position Code" ng-model="companyStructureSetup.GoodWorkPositionCode" class="form-control" readonly>' +
                        '<span class="input-group-btn">' +
                        '<button name="submit" ng-click="GWPositionCodePopUp()" class="btn single-small-btn"><i class="cr-icon glyphicon glyphicon-search"></i></button>' +
                        '</span>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +

                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label" title="Responsible Person Position Code">Responsible Person Position Code</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="input-group">' +
                        '<input type="text" name="Good Work Position Code" ng-model="companyStructureSetup.ResponsiblePersonPositionCode" class="form-control" readonly>' +
                        '<span class="input-group-btn">' +
                        '<button name="submit" ng-click="RPPositionCodePopUp()" class="btn single-small-btn"><i class="cr-icon glyphicon glyphicon-search"></i></button>' +
                        '</span>' +
                        '</div>' +
                        '</div>' +
                        '</div>' +

                        '</div>';
                }
            });
    };

    $scope.getCompanyStructurerRelation();

    $scope.positionReport = function () {
        location.href = 'Organizations/position/PositionReport';
    };

    $scope.PositionGroupingData = {
        UserDefineGroup1: null,
        UserDefineGroup2: null,
        UserDefineGroup3: null,
        UserDefineGroup4: null,
        UserDefineGroup5: null,
        UserDefineGroup6: null
    };
    $scope.getPositionGroupingData = function () {
        $http({
            method: 'GET',
            url: 'Organizations/PositionGroupingData/GetData/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.PositionGroupingData = response.data[0];
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup1)) {
                $scope.PositionGroupingData.UserDefineGroup1 = "User Define Group1";
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup2)) {
                $scope.PositionGroupingData.UserDefineGroup2 = "User Define Group2";
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup3)) {
                $scope.PositionGroupingData.UserDefineGroup3 = "User Define Group3";
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup4)) {
                $scope.PositionGroupingData.UserDefineGroup4 = "User Define Group4";
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup5)) {
                $scope.PositionGroupingData.UserDefineGroup5 = "User Define Group5";
            }
            if (baseService.isUndefinedOrNull($scope.PositionGroupingData.UserDefineGroup6)) {
                $scope.PositionGroupingData.UserDefineGroup6 = "User Define Group6";
            }
        });
    };
    $scope.getPositionGroupingData();

    // #region Process

    $scope.processSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];
    $scope.processPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.processPopUp = function () {
        $scope.popUpProcessUrl = 'Processes/Process/GetList?processId=[]';
        $scope.getProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpProcessUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processPopUpDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.processAdd = function (data) {
        $scope.companyStructureSetup.ProcessName = data.UserName;
        $scope.companyStructureSetup.ProcessId = data.Id;
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    //start Good Work Position Code
    $scope.GWPCSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];

    $scope.GWPCPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.GWPositionCodePopUp = function () {
        $scope.name == "GW";
        $scope.GWPCPopUpUrl = 'Organizations/Position/getlist';
        $scope.getGWPCData = function (pageno) {
            baseService.paginationBase($scope.GWPCPopUpUrl, pageno, $scope.GWPCPopUpParameters)
                .then(function (result) {
                    $scope.GWPCPopUpDataList = result.Rows;
                    $scope.GWPCPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'GWPCPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GWPCPopUp')).modal('show');
        $scope.getGWPCData();
    };

    $scope.closeGWPCPopUp = function () {
        angular.element(document.querySelector('#GWPCPopUp')).modal('hide');
    };

    $scope.GWPCAdd = function (data) {
        if ($scope.name == "GW") {
            $scope.companyStructureSetup.GoodWorkPositionCode = data.Code;
            $scope.companyStructureSetup.GoodWorkPositionCodeId = data.Id;
        } else {
            $scope.companyStructureSetup.ResponsiblePersonPositionCode = data.Code;
            $scope.companyStructureSetup.ResponsiblePersonPositionCodeId = data.Id;
        }
        $scope.name == "";
        angular.element(document.querySelector('#GWPCPopUp')).modal('hide');
    };

    $scope.RPPositionCodePopUp = function () {
        $scope.name == "RP";
        $scope.GWPCPopUpUrl = 'Organizations/Position/getlist';
        $scope.getGWPCData = function (pageno) {
            baseService.paginationBase($scope.GWPCPopUpUrl, pageno, $scope.GWPCPopUpParameters)
                .then(function (result) {
                    $scope.GWPCPopUpDataList = result.Rows;
                    $scope.GWPCPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'GWPCPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#GWPCPopUp')).modal('show');
        $scope.getGWPCData();
    };



    // #endregion

}