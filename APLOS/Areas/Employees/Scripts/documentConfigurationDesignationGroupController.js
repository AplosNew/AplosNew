'use strict';
DocumentConfigurationDesignationGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DocumentConfigurationDesignationGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Document Configuration DesignationGroup';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.documentConfigurationDesignationGroups = [];
    $scope.path = 'employees/documentConfigurationDesignationGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.documentConfigurationDesignationGroup = {
        Id: null,
        CompanyId: null,
        ResponsiblePersonBy: 'DocumentSet',
        CompanyGroupId: null,
        PlantId: null,
        ComplianceDocumentSetId: null,
        EmployeeCategoryId: null,
        ComplianceDocumentSetName: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        EmploymentType:null
    };
    $scope.documentConfigurationDesignationGroupNew = Object.assign({}, $scope.documentConfigurationDesignationGroup);
    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.documentConfigurationDesignationGroupNew.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.designationGroupList = [];
    cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
        $scope.designationGroupList = result;
    });
    $scope.employeeTypeList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeTypeList = result;
    });
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    //cboService.getCboEmploymentType(function (result) {
    //    $scope.EmploymentTypeList = result;
    //});

    cboService.getEnumCbo('enum/GetEmploymentTypeCbo', function (result) {
        $scope.EmploymentTypeList = result;
    });
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
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
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

    $scope.ShowEmployeeListPopUp = function (index) {
        if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.PlantId)) {
            return ShowResult('Plant selection required.', 'failure');
        }
        $scope.documentListIndex = index;
        $scope.getEmployeeData = function (pageno) {
            baseService.paginationBase('employees/EmployeeInformation/GetEmployeeSearchList?plantId=' + $scope.documentConfigurationDesignationGroupNew.PlantId, pageno, $scope.employeeParameters)
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
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.selectEmployeedblClick = function (data) {

        if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document') {
            $scope.complianceDocumentConfigurationList[$scope.documentListIndex].ResponsiblePersonName = data.EmployeeName;
            $scope.complianceDocumentConfigurationList[$scope.documentListIndex].ResponsiblePersonId = data.SystemId;
        }
        $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonName = data.EmployeeName;
        $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonId = data.SystemId;
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.clearResponsiblePerson = function (index) {
        if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document') {
            $scope.complianceDocumentConfigurationList[index].ResponsiblePersonName = null;
            $scope.complianceDocumentConfigurationList[index].ResponsiblePersonId = null;
        }
        $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonName = null;
        $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonId = null;
    };
    //************************************ Employee PopUp End ****************************************
    //*********************** ComplianceDocument PopUp Start *************************************
    $scope.complianceDocumentSearchList = [
        {
            'Text': 'User Name',
            'Value': 'UserName'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Short Name',
            'Value': 'ShortName'
        },
        {
            'Text': 'Standard Name',
            'Value': 'StandardName'
        }
    ];
    $scope.complianceDocumentDataList = [];
    $scope.complianceDocumentSearch = [];
    $scope.complianceDocumentUrl = 'employees/ComplianceDocumentSet/GetList';
    $scope.complianceDocumentParameters = {
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

    $scope.complianceDocumentPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.PlantId)) {
            $scope.complianceDocumentDataList = [];
            return ShowResult('Plant selection required.', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.EmployeeCategoryId)) {
            $scope.complianceDocumentDataList = [];
            return ShowResult('Employee type selection required.', 'failure');
        }
        $scope.getComplianceDocumentData = function (pageno) {
            baseService.paginationBase($scope.complianceDocumentUrl, pageno, $scope.complianceDocumentParameters)
                .then(function (response) {
                    $scope.complianceDocumentDataList = response.Rows;
                    $scope.complianceDocumentParameters.total_count = response.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#complianceDocumentPopUp')).modal('show');
        $scope.getComplianceDocumentData();
    };
    $scope.closeComplianceDocumentPopUp = function () {
        $scope.ComplianceDocumentSetId = '';
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetName = '';
        angular.element(document.querySelector('#complianceDocumentPopUp')).modal('hide');
    };
    $scope.selectComplianceDocumentPopUp = function (complianceDocument) {
        $scope.selectedComplianceDocumentId = complianceDocument.Id;
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId = $scope.selectedComplianceDocumentId;
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetName = complianceDocument.UserName;
        if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document' && !baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId)) {
            $scope.getDocumentList();
        }
        // Nullify current selected position 
        angular.element(document.querySelector('#complianceDocumentPopUp')).modal('hide');

    };
    $scope.clearComplianceDocument = function () {
        $scope.selectedComplianceDocumentId = null;
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId = null;
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetName = null;
        $scope.complianceDocumentData = [];
        $scope.complianceDocumentSearch = [];
    };
    //*********************** ComplianceDocument PopUp End *************************************
    $scope.clearDocument = function () {
        $scope.complianceDocumentConfigurationList = [];
        $scope.tempDocumentList = [];
    };
    $scope.clearDocumentSet = function () {
        $scope.complianceDocumentConfigurationSetList = [];
        $scope.tempDocumentSetList = [];
        if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document' && !baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId)) {
            $scope.getDocumentList();
        }
    };
    $scope.documentTypeList = null;
    $scope.getDocumentSet = function () {
        var url = 'employees/DocumentConfigurationDesignationGroup/GetDocumentSet?plantId=' + $scope.documentConfigurationDesignationGroupNew.PlantId + '&employeeTypeId=' + $scope.documentConfigurationDesignationGroupNew.EmployeeCategoryId
            + '&employmentType=' + $scope.documentConfigurationDesignationGroupNew.EmploymentType;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            if (response.data.Rows.length > 0) {
                $scope.documentConfigurationDesignationGroupNew.Id = response.data.Rows[0].Id;
                $scope.documentConfigurationDesignationGroupNew.CompanyGroupId = response.data.Rows[0].CompanyGroupId;
                $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetName = response.data.Rows[0].ComplianceDocumentSetName;
                $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId = response.data.Rows[0].ComplianceDocumentSetId;
                $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonName = response.data.Rows[0].ResponsiblePersonName;
                $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonId = response.data.Rows[0].ResponsiblePersonId;
                $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy = response.data.Rows[0].ResponsiblePersonBy;
                $scope.getDocumentList();
            }
            else {
                $scope.Clear();
            }
        });
    };
    $scope.getDocumentList = function () {
        $scope.complianceDocumentConfigurationList = [];
        if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId)) {
            return ShowResult('Please select Document set.', 'failure');
        }
        if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document') {
            var url = 'employees/DocumentConfigurationDesignationGroup/GetDocumentSetAssignDetailList?complianceDocumentSetId=' + $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId + '&plantId=' + $scope.documentConfigurationDesignationGroupNew.PlantId + '&employeeTypeId=' + $scope.documentConfigurationDesignationGroupNew.EmployeeCategoryId;
            $http({
                method: 'GET',
                url: url
            }).then(function successCallback(response) {
                $scope.complianceDocumentConfigurationList = response.data;
            });
        }
    };
    //-----------------
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.companyId = $scope.documentConfigurationDesignationGroupNew.CompanyId;
        $rootScope.parameters.plantId = $scope.documentConfigurationDesignationGroupNew.PlantId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.documentConfigurationDesignationGroups = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.documentConfigurationDesignationGroup = $scope.documentConfigurationDesignationGroups[$scope.index];
        $scope.documentConfigurationDesignationGroupNew = Object.assign({}, $scope.documentConfigurationDesignationGroup);
        $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetName= $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSet;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.documentConfigurationDesignationGroupSaveList = [];
    function addDocumentSetListForSave() {
        $scope.documentConfigurationDesignationGroupSaveList.push(
            {
                Id: $scope.documentConfigurationDesignationGroupNew.Id,
                CompanyGroupId: $scope.documentConfigurationDesignationGroupNew.CompanyGroupId,
                PlantId: $scope.documentConfigurationDesignationGroupNew.PlantId,
                EmployeeCategoryId: $scope.documentConfigurationDesignationGroupNew.EmployeeCategoryId,
                CompanyId: $scope.documentConfigurationDesignationGroupNew.CompanyId,
                ResponsiblePersonId: $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonId,
                ComplianceDocumentSetId: $scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId,
                ComplianceDocumentId: null
            }
        );
    }
    function addDocumentListForSave(list) {
        angular.forEach(list, function (item) {
            item.EmployeeCategoryId = $scope.documentConfigurationDesignationGroup.EmployeeCategoryId;
            item.PlantId = $scope.documentConfigurationDesignationGroup.PlantId;
            item.ComplianceDocumentSetId = $scope.documentConfigurationDesignationGroup.ComplianceDocumentSetId;
            if (checkDocumentExist($scope.documentConfigurationDesignationGroupSaveList, item.ComplianceDocumentId, item.ResponsiblePersonId) === false) {
                $scope.documentConfigurationDesignationGroupSaveList.push(item);
            }
        });
    }
    function checkDocumentResponsibleExist(list) {
        for (var i = 0; i < list.length; i++) {
            if (!baseService.isUndefinedOrNull(list[i].ResponsiblePersonId)) {
                return true;
            }
        }
        return false;
    }
    function checkDocumentExist(list, ComplianceDocumentId, ResponsiblePersonId) {
        angular.forEach(list, function (item) {
            if (item.ComplianceDocumentId === ComplianceDocumentId && item.ResponsiblePersonId === ResponsiblePersonId) {
                return true;
            }
        });
        return false;
    }
    $scope.Save = function () {
        $scope.documentConfigurationDesignationGroupSaveList = [];
        angular.copy($scope.documentConfigurationDesignationGroupNew, $scope.documentConfigurationDesignationGroup);

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentConfigurationDesignationGroupForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.ComplianceDocumentSetId)) {
                return ShowResult('Document Set required!!', 'failure');
            }
            //if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'DocumentSet') {
            //    if (baseService.isUndefinedOrNull($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonId)) {
            //        return ShowResult('Rsponsible person required!!', 'failure');
            //    }
            //}
            if ($scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy === 'Document' && $scope.complianceDocumentConfigurationList.length > 0) {
                if (checkDocumentResponsibleExist($scope.complianceDocumentConfigurationList) === false) {
                    return ShowResult('Please select at least one responsible person!!', 'failure');
                }
                addDocumentListForSave($scope.complianceDocumentConfigurationList);
            }
            if ($scope.Action === 'Save' || $scope.Action === 'Update') {
                $http({
                    method: 'post',
                    url: $scope.saveUrl,
                    data: { 'entity': $scope.documentConfigurationDesignationGroup, 'entities': $scope.documentConfigurationDesignationGroupSaveList },
                    dataType: 'json'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        ClearFields();
                        $scope.getDocumentSet();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.documentConfigurationDesignationGroupSaveList = [];
                    $scope.tempDocumentList = [];
                    $scope.tempDocumentSetList = [];
                };
            }
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.documentConfigurationDesignationGroup = { CompanyId: $scope.documentConfigurationDesignationGroup.CompanyId, PlantId: $scope.documentConfigurationDesignationGroup.PlantId};
        $scope.documentConfigurationDesignationGroupNew = { CompanyId: $scope.documentConfigurationDesignationGroupNew.CompanyId, PlantId: $scope.documentConfigurationDesignationGroupNew.PlantId};
        $scope.documentConfigurationDesignationGroupNew.ResponsiblePersonBy = 'DocumentSet';
        $scope.documentConfigurationDesignationGroupSaveList = [];
        $scope.complianceDocumentConfigurationList = [];
    }
}
