'use strict';
employeeMobileAppsAuthorizationNewController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function employeeMobileAppsAuthorizationNewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Employee Mobile Apps Authorization";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.empMobileAuths = [];
    $scope.path = 'Securities/employeemobileappsauthorization/';
    //$scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrl = $scope.path + 'GetList';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.resetUrl = $scope.path + 'ResetPin';
    $scope.resetGuestUrl = $scope.path + 'ResetGuestPin';
    $scope.savePINUrl = $scope.path + 'UpdateEmployeePIN';

    $scope.saveDisabled = false;

    $scope.empMobileAuthSearchParameters = {
        Id: null,
        PlantId: null,
        CompanyId: null,
        EmployeeId: null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        DesignationGroupId: null,
        DesignationId: null
    };
    $scope.empMobileAuth = {
        Id: null,
        EmployeeId: null,
        EmployeeCode: null,
        EmployeeName: null,
        IsSalaryStructure: false,
        IsPaySlip: false,
        IsMonthlyAttendance: false,
        IsDailyAttendanceNotification: false,
        IsSalaryProcessConfirmationNotification: false,
        IsSalaryDisbursementNotification: false,
        IsIncrementNotification: false,
        IsPromotionNotification: false,
        IsLeaveNotification: false,
        PIN: null,
        DesignationName: null,
        DepartmentName: null
    };

    $scope.getData = function (flag) {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            $http({
                method: 'GET',
                url: $scope.getListUrl,
                params: {
                    'plantId': $scope.empMobileAuthSearchParameters.PlantId
                    , 'unitId': $scope.empMobileAuthSearchParameters.UnitId
                    , 'divisionId': $scope.empMobileAuthSearchParameters.DivisionId
                    , 'departmentId': $scope.empMobileAuthSearchParameters.DepartmentId
                    , 'sectionId': $scope.empMobileAuthSearchParameters.SectionId
                    , 'subSectionId': $scope.empMobileAuthSearchParameters.SubSectionId
                    , 'designationGroupId': $scope.empMobileAuthSearchParameters.DesignationGroupId
                    , 'designationId': $scope.empMobileAuthSearchParameters.DesignationId
                    , 'employeeId': JSON.stringify($scope.empIdList)
                    , 'flag': flag
                }
            }).then(function successCallback(response) {
                $scope.empMobileAuths = [];
                $scope.empMobileAuths = response.data;
            });
        }
    };

    $scope.complanyList = [];
    cboService.getCompanyGroupCompanyCbo($window.companyGroupId, function (result) {
        $scope.complanyList = result;
    });

    //cboService.getCboPlantByCompanyGroup(null, function (result) {
    //    $scope.plantList = result;
    //});

    $scope.plantList = [];
    $scope.getPlantByCompany = function () {
        cboService.getCboPlantByCompany($scope.empMobileAuthSearchParameters.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };

    $scope.setNewPin = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        data.PIN = Math.floor(Math.random() * 900000) + 100000;
        ResetPin(data);

    };
    $scope.setPinCommand = [{
        type: "details", buttonOptions: {
            text: "Generate",
            width: "60",
            click: $scope.setNewPin

        }
    }];

    $scope.setGuestNewPin = function (args) {
        var gridObj = $("#GridGuest").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        data.PIN = Math.floor(Math.random() * 900000) + 100000;
        ResetGusetPin(data);

    };
    $scope.setGusetPinCommand = [{
        type: "details", buttonOptions: {
            text: "Generate",
            width: "60",
            click: $scope.setGuestNewPin

        }
    }];

    function ResetPin(data) {
        $http({
            method: 'POST',
            url: $scope.resetUrl,
            data: data,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.assignList = [];
                $scope.getAssignData();
              
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    function ResetGusetPin(data) {
        $http({
            method: 'POST',
            url: $scope.resetGuestUrl,
            data: data,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getGusetData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }

    $scope.onClick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        $scope.valuePassInDelModal(data);

    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Delete",
            width: "40",
            click: $scope.onClick
        }
    }];


    // #region Assign
    $scope.assignList = [];
    $scope.getAssignData = function () {
        $scope.CompanyId = $scope.empMobileAuthSearchParameters.CompanyId;
        $http.get('Securities/employeemobileappsauthorization/GetAssignedEmployee?plantId=' + $scope.empMobileAuthSearchParameters.PlantId)
            .then(
                function successCallback(response) {
                    $scope.assignList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.assignList = response.data;
                    }
                    $scope.empMobileAuthSearchParameters.CompanyId = $scope.CompanyId;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };
    // #endregion Assign

    // #region unAssign
    $scope.unassignList = [];
    $scope.getUnAssignData = function () {
        $scope.CompanyId = $scope.empMobileAuthSearchParameters.CompanyId;
        if (baseService.isUndefinedOrNull($scope.empMobileAuthSearchParameters.PlantId)) {
            $scope.unassignList = [];
           // ShowResult("Select Plant.", 'failure');
            return false;
        }
        //if (!baseService.isUndefinedOrNull($scope.empMobileAuthSearchParameters.PlantId)) {
        $http.get('Securities/employeemobileappsauthorization/GetUnAssignedEmployee?plantId=' + $scope.empMobileAuthSearchParameters.PlantId)
            .then(
                function successCallback(response) {
                    $scope.unassignList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.unassignList = response.data;
                    }
                    $scope.empMobileAuthSearchParameters.CompanyId = $scope.CompanyId;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        //}
    };
    // #endregion unAssign


    // #region Guset
    $scope.gusetList = [];
    $scope.getGusetData = function () {
       
        $http.get('Employees/GuestUser/GetGusetList')
            .then(
                function successCallback(response) {
                    $scope.gusetList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.gusetList = response.data;
                       
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        //}
    };
    
    // #endregion unAssign

    // #region checkAll
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.unassignList, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].IsSalaryStructure = true;
            else
                row[0].IsSalaryStructure = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Grid1").data("ejGrid");
            var filtered = $("#Grid1").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.unassignList.length; i++) {
                    $scope.unassignList[i].IsSalaryStructure = true;
                }
            }
            else {
                for (var i = 0; i < $scope.unassignList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.unassignList[i].Id == filtered[j].Id)
                            $scope.unassignList[i].IsSalaryStructure = true;
                    }

                }
            }

            var checkbox = $("#Grid1 .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid1").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.unassignList.length; i++) {
                    $scope.unassignList[i].IsSalaryStructure = false;
                }
            }
            else {
                for (var i = 0; i < $scope.unassignList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.unassignList[i].Id == filtered[j].Id)
                            $scope.unassignList[i].IsSalaryStructure = false;
                    }

                }
            }
            var checkbox = $("#Grid1 .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid1 .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.unassignList, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].IsSalaryStructure == true)
                $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });
        }
        $($("#Grid1 .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }
    // #endregion

    // #region employee
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
        }
    ];

    $scope.ShowEmployeeListPopUp = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.form.$valid) {
            $rootScope.tempList = [];
            angular.forEach($scope.empIdList, function (a) {
                $rootScope.tempList.push(a);
            });
            baseService.setCurrentPage('employeeList');
            baseService.init($scope.getEmployeeListUrl, null, null, null, 'EmployeeCode, FirstName, MiddleName, LastName ', 'LastName');
            $rootScope.parameters.plantId = $scope.empMobileAuthSearchParameters.PlantId;
            $rootScope.parameters.employeeIds = baseService.getColumnValueList($scope.empMobileAuths, 'EmployeeId');
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
        }
    };
    $scope.ClearList = function () {
        $scope.empIdList = [];
    };

    $scope.pushTempList = function (data, event, list) {
        if (event.currentTarget.checked)
            $rootScope.tempList.push(data);
        else {
            $rootScope.tempList.splice($rootScope.tempList.indexOf(data), 1);
            list.splice(list.indexOf(data), 1);
        }
    };
    $scope.empIdList = [];

    $scope.SelectEmployeeByButton1 = function () {
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
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
    };

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
    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        angular.element(document.querySelector('#employeePopUp1')).modal('hide');
    };
    function isRowSelected(ilst) {
        var flag = false;
        for (var i = 0; i < ilst.length; i++) {
            if (ilst[i].Flag) {
                return flag = true;
            }
        }
    }

    // #endregion

    // #region CRUD

    function MakeDataForSave() {
        $scope.unassignList = [];
        var gridObj = $("#Grid1").data("ejGrid");
        var rowCheck = $(".rowCheckbox:checked");

        for (var i = 0; i < rowCheck.length; i++) {
            gridObj.multiSelectCtrlRequest = true;
            var rowIndex = $(rowCheck[i]).parents("tr").index();
            gridObj.selectRows($(rowCheck[i]).parents("tr").index());// To prevent unselection of other rows when a checkbox is unchecked after selectAll rows
            var data = gridObj.getSelectedRecords()[0];

            $scope.unassignList.push(data);
        }

    }

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.empMobileAuthSearchParameters.PlantId)) {
                throw "Select Plant.";
            }
            var count = 0;
            for (var i = 0; i < $scope.unassignList.length; i++) {
                if ($scope.unassignList[i].IsSalaryStructure == true) {
                    count++;
                    break;
                }
            }
            if (count == 0) {
                throw "Please Select Employee(s).";
            }

            MakeDataForSave();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: $scope.unassignList,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.unassignList = [];
                    $scope.assignList = [];
                    $scope.getAssignData();
                   // $scope.getUnAssignData();
                    //$scope.LoadUnsavedData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //Edit PIN
    $scope.savesingledata = function (args) {

        var assignList = [];
        assignList.push(args.data);
        var gridObj = $("#Grid").data("ejGrid");
        var data = gridObj.model.dataSource;
        $http({
            method: 'POST',
            url: $scope.savePINUrl,
            data: { 'entities': assignList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                // $scope.getupdatedata();
                $scope.assignList = [];
                $scope.getAssignData();
                $scope.getGusetData();
            }
        });
    }

    //Deleting Rows from RetentionAllowanceList
    $scope.valuePassInDelModal = function (data) {
        $scope.tempEmpOb = data;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.assignList.splice($scope.empIndex, 1);
            //$scope.empIndex = -1;
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeFromDb($scope.tempEmpOb.Id, $scope.empIndex);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Securities/employeemobileappsauthorization/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.unassignList = [];
                    $scope.assignList = [];
                    $scope.getAssignData();
                   // $scope.getUnAssignData();
                    //$scope.LoadUnsavedData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    // #endregion

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.empMobileAuthSearchParameters = { CompanyId: $scope.empMobileAuthSearchParameters.CompanyId };
        $scope.empMobileAuth = {};
        $rootScope.tempList = [];
        $scope.empMobileAuths = [];
        $scope.empIdList = [];
    }

    $scope.rowRemoveModal = function (id, index) {
        $scope.message = '';
        $scope.id = id;
        $scope.index = index;
        $scope.message = 'Are you sure want to remove this data....';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };

    $scope.plantChange = function () {
        $scope.empMobileAuthSearchParameters = { CompanyId: $scope.empMobileAuthSearchParameters.CompanyId,PlantId: $scope.empMobileAuthSearchParameters.PlantId };
        $scope.empMobileAuth = {};
        $rootScope.tempList = [];
        $scope.empMobileAuths = [];
        $scope.empIdList = [];
    };
    //Tablcontrol
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    //End
}