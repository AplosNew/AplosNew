'use strict';
DesignationMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DesignationMasterController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Designation Master';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.designationMasters = [];
    $scope.path = 'Organizations/designationmaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.updateSyncUrl = 'employees/budgetcodechange/SyncGivenDesignation';
    baseService.init($scope.getListUrl, null, null, null, 'DesignationName', 'DesignationName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.designationMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.designationMaster = {
        Id: null,
        CompanyGroupId: null,
        DesignationGroupId: null,
        DesignationGroupName: null,
        DesignationId: null,
        DesignationName: null,
        EmployeeCategoryId: null,
        EmployeeCategoryName: null,
        RecruitmentProcessSetId: null,
        Code: null,
        UserName: null,
        Active: true,
        Archive: null
    };
    $scope.designationMasterNew = angular.copy($scope.designationMaster);
    $scope.searchByList = [
        {
            'name': 'Designation Group',
            'value': 'DesignationGroupName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
        ,
        {
            'name': 'Employee Category',
            'value': 'EmployeeCategoryName'
        }
    ];
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.designationMaster = $scope.designationMasters[$scope.index];
        $scope.designationMasterNew = angular.copy($scope.designationMaster);
        $scope.getdesgMstLegalDesg();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // #region ddl

    $scope.designationGroupList = [];
    $http({
        method: 'GET',
        url: 'Organizations/designationgroup/getcbo'
    }).then(function successCallback(response) {
        $scope.designationGroupList = response.data;
    });

    $scope.designationList = [];
    $http({
        method: 'GET',
        url: 'Organizations/designation/getcbo'
    }).then(function successCallback(response) {
        $scope.designationList = response.data;
    });

    $scope.empTypeList = [];
    $http({
        method: 'GET',
        url: 'employees/employeecategory/getcbo'
    }).then(function successCallback(response) {
        $scope.empTypeList = response.data;
    });

    cboService.getCboRecruitmentProcessSetByCompanyGroup(null, function (result) {
        $scope.recruitmentProcessSetList = result;
    });

    // #endregion
    $scope.Save = function () {
        $scope.designationGroupName = document.getElementById("designationGroupId").options[document.getElementById('designationGroupId').selectedIndex].text;
        $scope.designationName = document.getElementById("designationId").options[document.getElementById('designationId').selectedIndex].text;
        $scope.employeeTypeName = document.getElementById("empTypeId").options[document.getElementById('empTypeId').selectedIndex].text;
        angular.copy($scope.designationMasterNew, $scope.designationMaster);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.designationMasterNewForm.$valid) {
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'designationMaster': $scope.designationMaster
                        , 'legalDesig': $scope.desLegalList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.designationMaster = response.data.DesignationMaster;
                        $scope.designationMaster.DesignationGroupName = $scope.designationGroupName;
                        $scope.designationMaster.DesignationName = $scope.designationName;
                        $scope.designationMaster.EmployeeCategoryName = $scope.employeeTypeName;
                        $scope.designationMasters.push($scope.designationMaster);
                        baseService.paginationAdd();
                        ClearFields();
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
                        'designationMaster': $scope.designationMaster
                        , 'legalDesig': $scope.desLegalList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.designationMaster.DesignationGroupName = $scope.designationGroupName;
                            $scope.designationMaster.DesignationName = $scope.designationName;
                            $scope.designationMaster.EmployeeCategoryName = $scope.employeeTypeName;
                            $scope.designationMasters[$scope.index] = $scope.designationMaster;
                        }
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.designationMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.designationMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.designationMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
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
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.designationMaster = {};
        $scope.designationMasterNew = {};
        $scope.designationMasterNew.Active = true;
        $scope.desLegalList = [];
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #region Legal Designation
    $scope.desLegalList = [];
    $scope.getdesgMstLegalDesg = function () {
        $http({
            method: 'GET',
            url: 'Organizations/designationmaster/DesMstLegalDesignation?desMstId=' + $scope.designationMasterNew.Id
        }).then(function successCallback(response) {
            $scope.desLegalList = response.data.Rows;
        });
    };
    $scope.valueData = '';
    $scope.searchlegalDesigList = [
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
        }
        ,
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.legalDesigParameters = {
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
    $scope.legalDesignationPopUp = function () {
        $scope.legalDesigUrl = 'Organizations/legalDesignation/getlist?ids=' + isLDIdExistGrid($scope.desLegalList);
        baseService.setCurrentPage('legalDesignationList');
        $scope.getLegalDesigData = function (pageno) {
            baseService.paginationBase($scope.legalDesigUrl, pageno, $scope.legalDesigParameters)
                .then(function (result) {
                    $scope.legalDesignationList = result.Rows;
                    $scope.legalDesigParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.legalDesignationList.length; i++) {
                        $scope.legalDesignationList[i].Flag = tempList.includes($scope.legalDesignationList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'legalDesigPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#legalDesigPopUp')).modal('show');
        $scope.getLegalDesigData();
    };
    function isLDIdExistGrid(list) {
        $scope.LegalDesignationIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                $scope.LegalDesignationIds.push(list[i]['LegalDesignationId']);
            }
        }
        return JSON.stringify($scope.LegalDesignationIds);
    }
    $scope.closeLegalDesigPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#legalDesigPopUp')).modal('hide');
    };
    $scope.addLegalDesig = function () {
        if (!isRowSelected($scope.legalDesignationList)) {
            ShowResult('Please select at least one row!', 'failure', 'legalDesigPopUp');
            return;
        }
        $http({
            method: 'GET',
            url: 'Organizations/designationmaster/legaldesignationlistbyid?legalDesiIds=' + JSON.stringify(tempList)
        }).then(function successCallback(response) {
            //$scope.desLegalList = response.data.Rows;
            for (var i = 0; i < response.data.Rows.length; i++) {
                $scope.desLegalList.push({
                    Id: null,
                    DesignationMasterId: $scope.designationMasterNew.Id,
                    LegalDesignationId: response.data.Rows[i].LegalDesignationId,
                    Sequence: response.data.Rows[i].Sequence,
                    Code: response.data.Rows[i].Code,
                    ShortName: response.data.Rows[i].ShortName,
                    StandardName: response.data.Rows[i].StandardName,
                    UserName: response.data.Rows[i].UserName,
                    Active: response.data.Rows[i].Active
                });
            }
            tempList = [];
        });
        $scope.closeLegalDesigPopUp();
    };
    function isRowSelected(ilst) {
        try {
            var flag = false;
            for (var i = 0; i < ilst.length; i++) {
                if (ilst[i].Flag) {
                    return flag = true;
                }
            }
        } catch (e) {
            throw e;
        }
    }
    var tempList = [];
    $scope.selectLdValueId = function (event, id) {
        if (event.currentTarget.checked)
            tempList.push(id);
        else
            tempList.splice(tempList.indexOf(id), 1);
    };
    // #endregion
    $scope.legalIndex = -1;
    $scope.valuePassInDelModal = function (index, data) {
        $scope.message_confirmation = '';
        $scope.tempEmpOb = data;
        $scope.legalIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.UserName + ']</b>';
        angular.element(document.querySelector('#confirmLegalDesigPopUp')).modal('show');
    };
    $scope.removeLegalDesigRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.desLegalList.splice($scope.legalIndex, 1);
            $scope.empIndex = -1;
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeLegalDesignationFromDb($scope.tempEmpOb.Id, $scope.legalIndex);
        }

        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeLegalDesignationFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Organizations/DesignationMaster/LegalDesignationDelete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.desLegalList.splice($scope.legalIndex, 1);
                    $scope.empIndex = -1;
                    $scope.tempEmpOb.Id = null;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.designationMasterReport = function () {
        location.href = 'Organizations/designationmaster/designationmasterreport';
    };


    $scope.confirmToSync = function () {
        $scope.message_confirmation = "Are you sure want to Sync this data....";
        angular.element(document.querySelector("#confirmPostPopUp")).modal("show");
    };


    $scope.SyncDesignation = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.updateSyncUrl,

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    angular.element(document.querySelector("#confirmPostPopUp")).modal("hide");
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
            return true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}