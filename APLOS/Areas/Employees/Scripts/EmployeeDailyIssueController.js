'use strict';
EmployeeDailyIssueController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function EmployeeDailyIssueController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$window) {
    $rootScope.title = 'Daily Issue';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.DailyIssues = [];
    $scope.path = 'employees/EmployeeInformation/';
    $scope.saveUrl = $scope.path + 'CreateDailyIssue';
    $scope.deleteUrl = $scope.path + 'DeleteDailyIssue/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.searchDIBy = "Category"; $scope.searchDI = "";
    $scope.searchByDIList = [{ value: 'Id', name: "Id" }, { value: 'Category', name: "Category" }, { value: 'IssueType', name: "Issue" }, { value: 'CriticalityLevel', name: "CriticalityLevel" }, { value: 'ResponsiblePerson', name: "ResponsiblePerson" }, { value: 'Remarks', name: "Remarks" }];

    $scope.CriticalityLevelList = [{ Value: 'Critical', Text: "Critical" }, { Value: 'Normal', Text: "Normal" }];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDailyIssueDataList",
            data: { column: $scope.searchDIBy, value: $scope.searchDI },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DailyIssues = response.data;
        });
    }
    $scope.getData();

    $scope.DailyIssue = {
        Id: null,
        Category: null,
        ResolveTime: null,
        CriticalityLevel: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        IssueType: null,
        HandleTime: null,
        Remarks: null,
        ShiftId:null
    };

    $scope.DailyIssueNew = Object.assign({}, $scope.DailyIssue);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";
    $scope.searchByList = [{ value: 'SystemId', name: "SystemId" }, { value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "Employee Name" }, { value: 'Department', name: "Department" }, { value: 'LegalDesignation', name: "Designation" }];


    $scope.employee = [];
    $scope.getPopUpData = function () {
        //$scope.employee = [];
        //$scope.getEmpData();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.emppath = 'employees/employeeinformation/';
    $scope.getEmpData = function () {
        $http({
            method: 'Get',
            //url: $scope.emppath + "GetEmployeeDataList",
            url: 'employees/authorizationconfig/getallemployeedata',
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }

    $scope.setEmpData = function (obj) {
        var data = obj.data;
        $scope.DailyIssueNew.ResponsiblePersonCode = data.EmployeeCode;
        $scope.DailyIssueNew.ResponsiblePersonId = data.EmpSystemId;
        $scope.DailyIssueNew.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');

    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.fixedShitList = [];
    $scope.GetShiftCbo = function () {
        $scope.fixedShitList = [];
        $http.get('employees/EmployeeInformation/GetCboShiftDefinationByPlant?plantId=' + $window.plantId)
            .then(function (response) {
                $scope.fixedShitList = response.data;
            });
    };
    $scope.GetShiftCbo();


    $scope.Get = function (data) {
        $scope.DailyIssueNew = Object.assign({}, data.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: 'employees/EmployeeInformation/CreateDailyIssue',
                data: { 'data': $scope.DailyIssueNew },
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
        if (!baseService.isUndefinedOrNull($scope.DailyIssueNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.DailyIssueNew.Id,
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
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.DailyIssue = {};
        $scope.DailyIssueNew = {};
    }

    // #region checkbox all

    $scope.ResponsiblePersonList = [];
    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    function MakeData() {
        for (var i = 0; i < $scope.popUpDataList.length; i++) {
            if ($scope.popUpDataList[i].Flag == true) {
                if (checkExists($scope.ResponsiblePersonList, $scope.popUpDataList[i].SystemID) === false) {
                    var ob = {};
                    ob.Id = -(Math.floor(Math.random() * 1000000) + 1);
                    ob.EmpSystemId = $scope.popUpDataList[i].SystemId;
                    ob.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    ob.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    ob.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    ob.Department = $scope.popUpDataList[i].Department;
                    ob.Company = $scope.popUpDataList[i].Company;
                    ob.Plant = $scope.popUpDataList[i].Plant;
                    ob.Section = $scope.popUpDataList[i].Section;
                    ob.SubSection = $scope.popUpDataList[i].SubSection;
                    ob.Line = $scope.popUpDataList[i].Line;

                    $scope.ResponsiblePersonList.push(ob);
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmpSystemId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveEmployee = function () {
        try {
            if (baseService.arrayLength($scope.ResponsiblePersonList) < 0) {
                throw "Select Employee.";
            }
            $http({
                method: 'POST',
                url: 'employees/EmployeeInformation/CreateResPerson',
                data: { 'data': $scope.ResponsiblePersonList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.closePopUp = function () {
        MakeData();
        $scope.SaveEmployee();
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.getSavedEmpData = function () {
        $http({
            method: 'GET',
            url: 'employees/EmployeeInformation/GetResPersonList'
        }).then(function successCallback(response) {
            $scope.ResponsiblePersonList = response.data;

        });
    };
    $scope.getSavedEmpData();

    $scope.message_confirmation = "";
    $scope.removeEmp = function (data) {
        $scope.runobj = data.data;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.runobj.EmployeeCode + ' ]';
        angular.element(document.querySelector('#confirmRunDelPopUp')).modal('show');
    };
    $scope.DeleteEmployee = function () {
        if (!baseService.isUndefinedOrNull($scope.runobj.Id)) {
            $http({
                method: 'POST',
                url: 'employees/EmployeeInformation/DeleteResPerson?id=' + $scope.runobj.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedEmpData();
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }

    };



}