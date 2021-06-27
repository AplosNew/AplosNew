employeeBaseMultipleController.$inject = ['$scope', '$http', 'baseService'];
function employeeBaseMultipleController($scope, $http, baseService) {
    //********************************* Employee PopUp Start ***********************************************
    $scope.responsiblePersonHiddenControlId = null;
    $scope.responsiblePersonTextControlId = null;
    $scope.employeeList = [];
    $scope.employeeIndex = -1;
    $scope.responsiblePersonIndex = -1;
    $scope.selectedEmployee = null;
    $scope.searchEmployeeByList = [
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
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

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

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
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

    $scope.showResponsiblePersonListPopUp = function (Id, Text) {
        $scope.responsiblePersonHiddenControlId = Id;
        $scope.responsiblePersonTextControlId = Text;
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.selectResponsiblePersonPopUp = function (index, id) {
        $scope.responsiblePersonIndex = index;
        $scope.selectedResponsiblePerson = id;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };
    $scope.hideResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.showEmployeeGroupListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'employees/EmployeeInformation/GetEmployeeListByCompanyGroup';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
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
}
//employeeBaseController.$inject = ['$scope', '$http', 'baseService'];
//function employeeBaseController($scope, $http, baseService) {
//    //********************************* Employee PopUp Start ***********************************************
//    $scope.employeeList = [];
//    $scope.employeeIndex = -1;
//    $scope.responsiblePersonIndex = -1;
//    $scope.mentorPersonIndex = -1;

//    $scope.selectedEmployee = null;
//    $scope.selectedMentorPerson = null;
//    $scope.searchEmployeeByList = [
//        {
//            'name': 'Employee Code',
//            'value': 'EmployeeCode'
//        },
//        {
//            'name': 'First Name',
//            'value': 'FirstName'
//        },
//        {
//            'name': 'Middle Name',
//            'value': 'MiddleName'
//        },
//        {
//            'name': 'Last Name',
//            'value': 'LastName'
//        },
//        {
//            'name': 'Employee Name',
//            'value': 'EmployeeName'
//        },
//        {
//            'name': 'Designation',
//            'value': 'DesignationName'
//        },
//        {
//            'name': 'Entity',
//            'value': 'EntityName'
//        },
//        {
//            'name': 'Department',
//            'value': 'Department'
//        },
//        {
//            'name': 'Employment Type',
//            'value': 'EmploymentType'
//        },
//        {
//            'name': 'Status',
//            'value': 'EmployeeStatus'
//        }
//    ];

//    $scope.employeeParameters = {
//        limit: 10,
//        offset: 0,
//        order: 'asc',
//        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
//        searchBy: 'EmployeeCode',
//        pageSize: 10,
//        total_count: 0,
//        search: null,
//        serverPagination: true
//    };


//    $scope.showEmployeeListPopUp = function () {
//        baseService.setCurrentPage('employeeList');
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#employeePopUp')).modal('show');
//        $scope.getEmployeeData();
//    };



//    $scope.showAuthorisePersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#authorisedPersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showIssueSubTaskResponsiblePersonPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#issueSubTaskResponsiblePersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showUpdateResponsiblePersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#updateResponsiblePersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showInternalResponsiblePersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#internalResponsiblePersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showExternalResponsiblePersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#externalResponsiblePersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showFollowUpResponsiblePersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#followupResponsiblePersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    $scope.showMentorPersonListPopUp = function () {
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByPlant';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#mentorPersonPopUp')).modal('show');
//        $scope.getEmployeeData();
//    };

//    //assignTo popup
//    $scope.selectEmployeePopUp = function (index, id) {
//        $scope.employeeindex = index;
//        $scope.selectedEmployee = id;
//    };
//    $scope.selectUpdateResponsiblePersionPopUp = function (index, id) {
//        $scope.updateResponsiblePersonIndex = index;
//        $scope.selectedupdateResponsiblePersonId = id;
//    };
//    $scope.selectFollowUpResponsiblePersonPopUp = function (index, id) {
//        $scope.followUpResponsiblePersonIndex = index;
//        $scope.selectedFollowUpResponsiblePersonId = id;
//    };
//    $scope.selectInternalResponsiblePersonPopUp = function (index, id) {
//        $scope.internalResponsiblePersonIndex = index;
//        $scope.selectedInternalResponsiblePersonId = id;
//    };
//    $scope.selectExtarnalResponsiblePersonPopUp = function (index, id) {
//        $scope.externalResponsiblePersonIndex = index;
//        $scope.selectedExternalResponsiblePersonId = id;
//    };
//    $scope.selectResponsiblePersonPopUp = function (index, id) {
//        $scope.responsiblePersonIndex = index;
//        $scope.selectedResponsiblePerson = id;
//    };

//    //$scope.selectAuthorisePopUp = function (index, id) {
//    //    $scope.authorisepersonindex = index;    
//    //    $scope.selectedauthorisepersonId = id;
//    //};
//    $scope.selectAuthorisePopUp = function (index, id) {
//        $scope.authorisepersonindex = index;
//        $scope.selectedauthorisepersonId = id;
//    };
//    $scope.selectMentorPopUp = function (index, id) {
//        $scope.mentorPersonIndex = index;
//        $scope.selectedMentorPerson = id;
//    };
//    //$scope.hideIssueTaskResponsiblePersonPopUp = function () {
//    //    alert('hidden'); angular.element(document.querySelector('#issueSubTaskResponsiblePersonPopUp')).modal('hide');
//    //    $scope.issueSubTaskResponsiblePersonIndex = -1;
//    //    $scope.issueSubTaskResponsiblePersonId = null;
//    //};
//    $scope.hideEmployeePopUp = function () {
//        angular.element(document.querySelector('#employeePopUp')).modal('hide');
//        $scope.employeeIndex = -1;
//        $scope.selectedEmployee = null;
//    };
//    $scope.hideResponsiblePersonPopUp = function () {
//        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
//        $scope.employeeIndex = -1;
//        $scope.selectedEmployee = null;
//    };

//    $scope.hideAuthorisePersonPopUp = function () {
//        angular.element(document.querySelector('#authorisedPersonPopUp')).modal('hide');
//        $scope.authorisePersonIndex = -1;
//        $scope.selectedAuthorisePerson = null;
//    };
//    $scope.hideMentorPersonPopUp = function () {
//        angular.element(document.querySelector('#mentorPersonPopUp')).modal('hide');
//        $scope.mentorPersonIndex = -1;
//        $scope.selectedMentorPerson = null;
//    };
//    $scope.showEmployeeGroupListPopUp = function () {
//        baseService.setCurrentPage('employeeList');
//        $scope.getEmployeeData = function (pageno) {
//            var url = null;
//            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
//                url = 'employees/EmployeeInformation/GetEmployeeListByCompanyGroup';
//            }
//            else {
//                url = $scope.employeeUrl;
//            }
//            baseService.paginationBase(url, pageno, $scope.employeeParameters)
//                .then(function (result) {
//                    $scope.employeeList = result.Rows;
//                    $scope.employeeParameters.total_count = result.Total;
//                }, function () {
//                    ShowResult(commonMessage.NetworkError, 'failure');
//                }).finally(function () {
//                });
//        };
//        angular.element(document.querySelector('#employeePopUp')).modal('show');
//        $scope.getEmployeeData();
//    };
//}