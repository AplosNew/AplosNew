'use strict';
MeetingAgendaController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MeetingAgendaController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Meeting Agenda';
    $scope.Action = 'Save'; 
    $scope.ModelList = [];
    $scope.path = 'MeetingManagement/MeetingAgenda/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/'; 
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.year = new Date().getFullYear().toString();

    $scope.ModelAgenda = {
        Id: null,
        MeetingOrganizedById: null,
        MeetingOrganizedByCode: null,
        MeetingOrganizedBy: null,
        ChairedById: null,
        ChairedByCode: null,
        ChairedBy: null,
        Date: null,
        Location: null,
        MeetingName: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelAgenda);

    

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

   
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        
        try {
            angular.copy($scope.ModelNew, $scope.ModelAgenda);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
                
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'data': $scope.ModelNew },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.ModelNew.Id = response.data.Id;
                            $scope.getData();
                            $scope.Clear();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                   
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.ModelNew = {
            Id: null,
            MeetingOrganizedById: null,
            MeetingOrganizedByCode: null,
            MeetingOrganizedBy: null,
            ChairedById: null,
            ChairedByCode: null,
            ChairedBy: null,
            Date: null,
            Location: null,
            MeetingName: null
        };
        $scope.Action = 'Save';
    };
    

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });


    $scope.meetingTypeList = [];
    cboService.getCbomeetingType(function (result) {
        $scope.meetingTypeList = result;
    });

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

    
    $scope.Name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;
        
        if ($scope.Name == 'Main') {
            $scope.ModelNew.MeetingOrganizedById = data.SystemId;
            $scope.ModelNew.MeetingOrganizedBy = data.EmployeeName;
            $scope.ModelNew.MeetingOrganizedByCode = data.EmployeeCode;
        }
        else {
            $scope.ModelNew.ChairedById = data.SystemId;
            $scope.ModelNew.ChairedBy = data.EmployeeName;
            $scope.ModelNew.ChairedByCode = data.EmployeeCode;
        }
        
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };


   
}