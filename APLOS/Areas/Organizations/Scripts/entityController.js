'use strict';
entityController.$inject = ['cboService', 'commonMessage', '$rootScope', '$scope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile'];
function entityController(cboService, commonMessage, $rootScope, $scope, baseService, $routeParams, $location, $http, $filter, $compile) {
    $rootScope.title = 'Entity';
    $scope.Action = 'Save';
    var url = 'Organizations/entity/getlist';
    $scope.dataList = [];
    $scope.fieldDataList = [];
    $scope.isUsed = false;

    $scope.companyStructureSetup = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityType: null,
        UserName: null,
        Code: null,
        OldRefCode: null,
        EffectiveDate: null,
        EffectiveDateUpTo: null,
        Description: null,
        Remarks: null,
        Active: true,
        IsExceptionForPlanning: false,
        IsProduction: false,
        FilePrefix: null,
        ThirdPartyBusinessArea: null,
        ThirdPartyProfitCenter: null,
        VATResistrationNo:null
    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getCboPlantByCompany = function (companyId) {
        $scope.Clear();
        cboService.getCboPlantByCompany(companyId, function (result) {
            $scope.PlantList = result;
        });
    };

    cboService.getCboUnitByCompanyGroup(null, function (result) {
        $scope.UnitList = result;
    });

    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.DivisionList = result;
    });

    cboService.getCboSubDivisionByCompanyGroup(null, function (result) {
        $scope.SubDivisionList = result;
    });

    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.SectionList = result;
    });

    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.SubSectionList = result;
    });

    cboService.getCboLineByCompanyGroup(null, function (result) {
        $scope.LineList = result;
    });

    cboService.getCboEmployeeGroupByCompanyGroup(null, function (result) {
        $scope.EmployeeGroupList = result;
    });

    $scope.getCboShiftDefinationByPlant = function (plantId) {
        cboService.getCboShiftDefinationByPlant(plantId, function (result) {
            $scope.ShiftDefinationList = result;
        });
    };

    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.DepartmentList = result;
    });

    $scope.entityTypeList = [];
    cboService.getCboEntityType(function (result) {
        $scope.entityTypeList = result;
    });

    $scope.Get = function (id) {
        $http.get('Organizations/entity/GetById?companyId=' + $scope.companyStructureSetup.CompanyId + '&&id=' + id)
            .then(function (response) {
                $scope.companyStructureSetup = response.data;
                $scope.getCompanyStructurerRelation($scope.companyStructureSetup.CompanyId, $scope.companyStructureSetup);
                $scope.UseChecking($scope.companyStructureSetup.Id);
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                    $scope.Action = 'Update';
                }
            });
    };

    $scope.UseChecking = function (id) {
        $http.get('Organizations/entity/UseChecking/' + id)
            .then(function (response) {
                $scope.isUsed = response.data;
            });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.companySSForm.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: 'Organizations/entity/Create',
                    data: $scope.companyStructureSetup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
                        ClearFields();
                    }
                });
                return true;
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: 'Organizations/entity/Edit',
                    data: $scope.companyStructureSetup,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getDataList();
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
                url: 'Organizations/entity/Delete/' + $scope.companyStructureSetup.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDataList($scope.companyStructureSetup.CompanyId);
                    ClearFields();
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        var cId = $scope.companyStructureSetup.CompanyId;
        $scope.Action = 'Save';
        $scope.companyStructureSetup = { CompanyId: cId };
        $scope.companyStructureSetup.Active = true;
        $scope.isUsed = false;
    }

    $scope.getCompanyStructurerRelation = function (id, data) {
        $scope.getDataList();
        $scope.left = '';
        $scope.right = '';
        $scope.Action = 'Save';
        $http.get('Organizations/EntityRelationship/getcompanystructurerelationfordata?companyId=' + id)
            .then(function (response) {

                var results = response.data.Rows;
                if (results !== null) {
                    angular.forEach(results, function (item, i) {
                        var dynamicHtml = '';
                        dynamicHtml =
                            item.StandardName === 'Plant' ?
                                '<select ng-disabled="isUsed" tabindex="' + item.Sequence + '" ng-model="companyStructureSetup.' + item.StandardName + 'Id" class="form-control" ng-options="item.Value as item.Text for item in ' + item.StandardName + 'List" required name="' + item.StandardName + '" ng-change="getCboShiftDefinationByPlant(companyStructureSetup.' + item.StandardName + 'Id)"><option value=""></option></select>' :
                                '<select ng-disabled="isUsed" tabindex="' + item.Sequence + '" ng-model="companyStructureSetup.' + item.StandardName + 'Id" class="form-control" ng-options="item.Value as item.Text for item in ' + item.StandardName + 'List" required name="' + item.StandardName + '"><option value=""></option></select>';

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
                    $scope.left += '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">Entity Type<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message"><div class="select-style">' +
                        '<select ng-disabled="isUsed" tabindex="1" ng-model="companyStructureSetup.EntityType" class="form-control" ng-options="item.Value as item.Text for item in entityTypeList" required name="Entitytype"><option value=""></option></select>' +
                        '</div></div></div>' +
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">Code<sup>*</sup></label><div class="col-sm-8 show-message">' +
                        '<input ng-disabled="isUsed" tabindex="3" required name="Code" type="text" maxlength="10" ng-model="companyStructureSetup.Code" class="form-control"></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Effective Date</label>' +
                        '<div class="col-sm-8">' +
                        '<input ng-disabled="isUsed" tabindex="5" type="text" class="form-control datepicker" datepicker ng-model="companyStructureSetup.EffectiveDate" name="EffectiveDate">' +
                        '</div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">File Prefix</label>' +
                        '<div class="col-sm-8">' +
                        '<input ng-disabled="isUsed" tabindex="7" type="text" maxlength="10" ng-model="companyStructureSetup.FilePrefix" class="form-control" name="File Prefix"></div></div>' +
                        '<div class="form-group">' +
                        '</div></div>' +
                        '<div class="form-group" > ' +
                        '<label class="col-sm-4 control-label">Active</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="checkbox-site">' +
                        '<label><input ng-disabled="isUsed" tabindex="20" type="checkbox" ng-model="companyStructureSetup.Active" ng-checked="companyStructureSetup.Active">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label></div></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Third Party Business Area</label>' +
                        '<div class="col-sm-8">' +
                        '<input ng-disabled="isUsed" tabindex="9" type="text" maxlength="50" ng-model="companyStructureSetup.ThirdPartyBusinessArea" class="form-control" name="Third Party Business Area"></div></div>'
                        +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Trade Tax</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="11" type="text" maxlength="30" ng-model="companyStructureSetup.TradeTax" class="form-control" name="Trade Tax"></div></div>' +
                        '<label class="col-sm-4 control-label">Description</label>' +
                        '<div class="col-sm-8">' +
                        '<textarea ng-disabled="isUsed" tabindex="13" maxlength="250" class="form-control" Rows="3" ng-model="companyStructureSetup.Description"></textarea>'
                        ;

                    $scope.right += '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Old Ref Code</label><div class="col-sm-8">' +
                        '<input tabindex="2" maxlength="10" type="text" ng-model="companyStructureSetup.OldRefCode" class="form-control"></div></div>' +
                        '<div class="form-group" show-errors>' +
                        '<label class="col-sm-4 control-label">User Name<sup>*</sup></label>' +
                        '<div class="col-sm-8 show-message">' +
                        '<input tabindex="4" type="text" maxlength="30" ng-model="companyStructureSetup.UserName" class="form-control" required name="User name"></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Effective Date UpTo</label><div class="col-sm-8">' +
                        '<input ng-disabled="isUsed" tabindex="6" type="text" class="form-control datepicker" datepicker ng-model="companyStructureSetup.EffectiveDateUpTo" name="EffectiveDateUpTo">' +
                        '</div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Responsible Person</label>' +
                        '<div class="col-sm-5 pdr3" style= "Width:260px;" > ' +
                        '<input tabindex="8" type="text" ng-model="companyStructureSetup.EmployeeName" class="form-control" disabled>' +
                        '</div>' +
                        '<div class="pdl3">' +
                        '<button name="Show" class="btn single-small-btn" ng-click="ShowEmployeeListPopUp()">' +
                        '<i class="cr-icon glyphicon glyphicon-search"></i>' +
                        '</button>' +
                        '<button class="btn single-small-btn" ng-click="employeeProfileClear()">' +
                        '<i class="cr-icon glyphicon glyphicon-refresh"></i>' +
                        '</button></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Exception For Planning</label>' +
                        '<div class="col-sm-8">' +
                        '<div class="checkbox-site">' +
                        '<label><input ng-disabled="isUsed" tabindex="10" type="checkbox" ng-model="companyStructureSetup.IsExceptionForPlanning" ng-checked="companyStructureSetup.IsExceptionForPlanning">' +
                        '<span class="cr"><i class="cr-icon glyphicon glyphicon-ok"></i></span></label></div></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">Third Party Profit Center</label>' +
                        '<div class="col-sm-8">' +
                        '<input ng-disabled="isUsed" tabindex="12" type="text" maxlength="50" ng-model="companyStructureSetup.ThirdPartyProfitCenter" class="form-control" name="Third Party Profit Center"></div></div>' +
                        '<div class="form-group">' +
                        '<label class="col-sm-4 control-label">VAT Resistration No</label>' +
                        '<div class="col-sm-8">' +
                        '<input tabindex="14" type="text" maxlength="20" ng-model="companyStructureSetup.VATResistrationNo" class="form-control" name="VATResistrationNo"></div></div>' +
                        '<div class="form-group"><label class="col-sm-4 control-label">Remarks</label>' +
                        '<div class="col-sm-8"><textarea ng-disabled="isUsed" tabindex="16" maxlength="250" class="form-control" Rows="3" ng-model="companyStructureSetup.Remarks"></textarea></div></div>'
                        ;
                }
            });
    };

    $scope.dataListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.getDataList = function (pageno) {

        $rootScope.parameters.companyId = $scope.companyStructureSetup.CompanyId;
        $scope.dataListUrl = url + '?companyId=' + $scope.companyStructureSetup.CompanyId;
        $scope.getData = function (pageno) {
            $scope.dataList = [];
            baseService.paginationBase($scope.dataListUrl, pageno, $scope.dataListParameters)
                .then(function (response) {
                    $scope.dataList = response.Rows;
                    $scope.dataListParameters.total_count = response.Total;
                    $scope.fieldDataList = [];
                    if (baseService.arrayLength($scope.dataList) !== 0) {
                        baseService.getDDLSearchColumn(response.Rows, $scope.fieldDataList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };

    $scope.selectMessage = '';
    $scope.entityReport = function () {
        if ($scope.companyStructureSetup.CompanyId == null) {
            $scope.selectMessage = 'Select Company';
        }
        else {
            $scope.selectMessage = '';
            location.href = 'Organizations/entity/EntityReport?companyId=' + $scope.companyStructureSetup.CompanyId;
        }
    };

    //********************************* Employee PopUp Start ***********************************************
    $scope.employeeList = [];
    $scope.employeeIndex = -1;
    $scope.selectedEmployee = null;
    $rootScope.searchEmployeeByList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'MiddleName',
            'value': 'MiddleName'
        },
        {
            'name': 'LastName',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        }
    ];

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'LastName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.ShowEmployeeListPopUp = function () {
        $scope.getEmployeeData = function (pageno) {
            baseService.paginationBase('employees/EmployeeInformation/GetEmployeeListByCompanyGroup', pageno, $scope.employeeParameters)
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

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.companyStructureSetup.EmployeeId = employee.SystemId;
            $scope.companyStructureSetup.EmployeeName = employee.EmployeeName;
        }
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.selectEmployeedblClick = function (data) {
        $scope.companyStructureSetup.EmployeeId = data.SystemId;
        $scope.companyStructureSetup.EmployeeName = data.EmployeeName;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $scope.employeeProfileClear = function () {
        $scope.companyStructureSetup.EmployeeId = null;
        $scope.companyStructureSetup.EmployeeName = null;
    }

    //************************************ Employee PopUp End ****************************************
}