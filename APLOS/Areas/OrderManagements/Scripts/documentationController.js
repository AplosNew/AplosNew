'use strict';
documentationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function documentationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Documentation";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.orderCategories = [];
    $scope.path = 'OrderManagements/documentation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.getDMSeqUrl = $scope.path + 'GetDMAutoSequence';
    $scope.saveDMUrl = $scope.path + 'CreateDocumentationMaster';
    $scope.deleteDMUrl = $scope.path + 'DeleteDocumentaitonMaster/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelList = [];
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.documentation = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        ResponsiblePerson:null,
        ResponsiblePersonId:null,
        Purpose:null,
        Category: null,
        AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.documentationNew = Object.assign({}, $scope.documentation);

    $scope.searchByList = [
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
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.documentationNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    $scope.Get = function (obj) {
        $scope.documentationNew = Object.assign({}, obj.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';
    $scope.showEmployeeListPopUp = function () {
        try {
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };
    $scope.closeEmployeePopUp = function () {
        var employee = $scope.employeeList[$scope.employeeIndex];
        $scope.documentationNew.ResponsiblePersonId = employee.SystemId;
        $scope.documentationNew.ResponsiblePerson = employee.EmployeeName;
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.PurposeList = [
        { Value: "Sale", Text: "Sale" },
        { Value: "Purchase", Text: "Purchase" },
        { Value: "Expense", Text: "Job Expense" }
    ];

    $scope.CategoryList = [
        { Value: "Local", Text: "Local" },
        { Value: "Overseas", Text: "Overseas" }
    ];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentationNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.documentationNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.documentationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.PlanningPriority);
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
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.documentation = {};
        $scope.documentationNew = {};
        $scope.documentationNew.PlanningPriority = seq;
        $scope.documentationNew.Active = true;
    }


    $scope.documentationMaster = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Source: null,
        DocumentType: null,
        DocumentFormat: null,
        AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.documentationMasterNew = Object.assign({}, $scope.documentationMaster);

    $scope.DocumentFormatList = [{ Value: 'PDF', Text: 'PDF' },
        { Value: 'JPEG', Text: 'JPEG' },
        { Value: 'Excel', Text: 'Excel' },
        { Value: 'Word', Text: 'Word' },
        { Value: 'Register', Text: 'Register' },
        { Value: 'Form', Text: 'Form' },
        { Value: 'Email', Text: 'Email' },
        { Value: 'PPT', Text: 'PPT' },
        { Value: 'CrystalReport', Text: 'Crystal Report' },
        { Value: 'Txt', Text: 'Txt' },
        { value: 'CSV', Text: 'CSV' }]
    console.log($scope.DocumentFormatList);

    $scope.DMModelList = [];
    $scope.getDMData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDMList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DMModelList = response.data;
        });
    }
    $scope.getDMData();

    $scope.GetDMSequence = function () {
        $http.get($scope.getDMSeqUrl)
            .then(function (response) {
                $scope.documentationMasterNew.Sequence = response.data;
            });
    };
    $scope.GetDMSequence();

    $scope.DMAction = "Save";

    $scope.GetDM = function (obj) {
        $scope.documentationMasterNew = Object.assign({}, obj.data);
        $scope.DMAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDM = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentationMasterNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveDMUrl,
                data: { 'data': $scope.documentationMasterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDMFields();
                    $scope.GetDMSequence();
                    $scope.getDMData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    $scope.DeleteDM = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteDMUrl + $scope.documentationMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDMFields(response.data.PlanningPriority);
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
    $scope.ClearDM = function () {
        ClearDMFields($scope.GetDMSequence());
        return true;
    };
    function ClearDMFields(seq) {
        $scope.DMAction = "Save";
        $scope.documentationMaster = {};
        $scope.documentationMasterNew = {};
        $scope.documentationMasterNew.Active = true;
    }












}