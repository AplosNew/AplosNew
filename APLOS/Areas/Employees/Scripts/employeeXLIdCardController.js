'use strict';
employeeXLIdCardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter', 'cboService'];
function employeeXLIdCardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter, cboService) {
    $rootScope.title = "Print Employee Id Card";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.empList = [];
    $scope.path = 'employees/employeeidcard/';

    // #region Employee

    $rootScope.tempList = [];
    $scope.getEmployeeListUrl = 'employees/EmployeeInformation/GetPlantEmployeeList';
    $scope.employeeList = [];
    $scope.searchEmployeeList = [
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
            'name': 'Employee Category',
            'value': 'EmployeeCategoryName'
        }
    ];

    $scope.ShowEmployeeListPopUp = function () {
        $rootScope.tempList = [];
        angular.forEach($scope.empIdList, function (a) {
            $rootScope.tempList.push(a);
        });
        baseService.setCurrentPage('employeeList');
        baseService.init($scope.getEmployeeListUrl, null, null, null, 'EmployeeCode, FirstName, MiddleName, LastName ', 'EmployeeCode');
        $rootScope.parameters.plantId = null;
        $rootScope.parameters.employeeIds = JSON.stringify([]);// baseService.getColumnValueList($scope.empMobileAuths, 'EmployeeId');
        $scope.getEmployeeData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    for (var t = 0; t < baseService.arrayLength($scope.employeeList); t++) {
                        $scope.employeeList[t].Flag = $rootScope.tempList.includes($scope.employeeList[t].EmployeeCode);
                    }
                    angular.element(document.querySelector('#employeePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getEmployeeData();
    };
    $scope.pushTempList = function (data, event) {
        if (event.currentTarget.checked) {
            $rootScope.tempList.push(data.EmployeeCode);
            $scope.empList.push(data);
        }
        else {
            $rootScope.tempList.splice($rootScope.tempList.indexOf(data.EmployeeCode), 1);
            for (var i = 0; i < baseService.arrayLength($scope.empList); i++) {
                if ($scope.empList[i].SystemId === data.SystemId)
                    $scope.empList.splice(i, 1);
            }
        }
        //console.log($scope.empList.length);
    };
    $scope.empIdList = [];

    $scope.SelectEmployeeByButton = function () {
        $scope.empIdList = [];
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!$scope.empIdList.includes(a))
                    $scope.empIdList.push(a);
            });
        }
        else $scope.empIdList = [];
        angular.forEach($scope.empIdList, function (a) {
            if (!$rootScope.tempList.includes(a))
                $scope.empIdList.splice($scope.empIdList.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $scope.prinModel = {
        IsWorker: false
        , IsEmployee: false
        , CompanyLogo: null
        , CompanyName: null
        , CompanyAddress: null
        , MobileNo: null
        , EmployeeCode: null
        , EmployeeName: null
        , EmployeePic: null
        , DesignationName: null
        , DOJ: null
        , BloodGroup: null
        , CardHolderSignature: null
        , AuthorizedSignature: null
        , EmploymentType: null
        , Department: null
        , LineNo: null
        , PresentAddress1: null
        , NameLabel: null
        , DesignationLabel: null
        , DepartmentLabel: null
        , LineLabel: null
        , EmploymentTypeName: null
        , UtilityName: null
        , NIDLabel: null
        , BloodGroupLabel: null
        , ParmanentAddress1Local: null
        , ParmanentAddress: null
        , MobileNoLabel: null
        , EmergencyTellNoLabel: null
        , validdate: null
        , EmrCntPer1CellNo: null
    };

    $scope.IssueDate = $filter('date')(new Date(), 'dd-MM-yyyy');

    $scope.emp = {
        Id: null
        , EmployeeCode: null
        , IdCardFormat: null
    };
    $scope.setData = function (data) {
        $scope.emp.Id = data.SystemId;
        $scope.emp.EmployeeCode = data.EmployeeCode;
        $scope.emp.EmployeeName = data.EmployeeName;
        $scope.emp.IdCardFormat = data.IdCardFormat;
        $scope.emp.EmploymentType = data.EmploymentType;
        $scope.CloseEmployeePopUp();
    };

    // #endregion

    $scope.idCardTemplateList = [];
    $scope.Id = null;
    cboService.getIdCardTemplateCbo(function (result) {
        $scope.idCardTemplateList = result;
    });


    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
        $scope.empList = [];
        $rootScope.tempList = [];
        $scope.empIdList = [];
        $scope.emp = {};
        $scope.prinModel = { IsWorker: false, IsEmployee: false };
    }

    $scope.Print = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.emp.tempId)) {
                if (baseService.isUndefinedOrNull($scope.emp.Id))
                    throw "First select employee.";
                else
                    location.href = 'Employees/EmployeeIdCard/PrintEmployeeIDCard?empId=' + $scope.emp.Id + '&tempId=' + $scope.emp.tempId + '&empType=' + $scope.emp.EmploymentType;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    var finalEnlishToBanglaNumber = { '0': '০', '1': '১', '2': '২', '3': '৩', '4': '৪', '5': '৫', '6': '৬', '7': '৭', '8': '৮', '9': '৯' };
    String.prototype.getDigitBanglaFromEnglish = function () {
        var retStr = this;
        for (var x in finalEnlishToBanglaNumber) {
            retStr = retStr.replace(new RegExp(x, 'g'), finalEnlishToBanglaNumber[x]);
        }
        return retStr;
    };
    //var english_number = "1-2-3-456";
    //var bangla_converted_number = english_number.getDigitBanglaFromEnglish();

}