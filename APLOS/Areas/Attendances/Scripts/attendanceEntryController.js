'use strict';
attendanceEntryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function attendanceEntryController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Attendance Entry';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/AttendanceEntry/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';


    //#region Tab
    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    //#region Tab

    $scope.dataList = [];
    $scope.employeeInfo = { EmployeeCode: null };
    $scope.GetEnterEmployeeInfo = function () {
        var parameters = {
            'SearchValue': $scope.employeeInfo.EmployeeCode
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceEntry/GetEmpInfo',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.employeeInfo.EmpSystemID = response.data[0].SystemID;
                $scope.employeeInfo.EmployeeCode = response.data[0].EmployeeCode;
                $scope.employeeInfo.EmployeeName = response.data[0].EmployeeName;
                $scope.employeeInfo.DOJ = response.data[0].DOJ;
                $scope.employeeInfo.DOC = response.data[0].DOC;
                $scope.employeeInfo.EmailId = response.data[0].EmailId;
                $scope.employeeInfo.Code = response.data[0].Code;
                $scope.employeeInfo.Section = response.data[0].Section;
                $scope.employeeInfo.SubSection = response.data[0].SubSection;
                $scope.employeeInfo.Department = response.data[0].Department;
                $scope.employeeInfo.LegalDesignation = response.data[0].LegalDesignation;
                $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
            }
            else {
                ShowResult("Please Select Correct Employee Code", 'failure');
            }
        });

    };


    $scope.dataList = [];
    $scope.employeeInfo = { EmployeeCode: null };
    $scope.employeeInfo = {};
    $scope.GetEmployeeInfo = function () {
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    };

    $scope.GetEnterEmployeeOutInfo = function () {
        var parameters = {
            'SearchValue': $scope.employeeInfoOut.EmployeeCode
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'Attendances/AttendanceEntry/GetEmpInfo',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.employeeInfoOut.EmpSystemID = response.data[0].SystemID;
                $scope.employeeInfoOut.EmployeeCode = response.data[0].EmployeeCode;
                $scope.employeeInfoOut.EmployeeName = response.data[0].EmployeeName;
                $scope.employeeInfoOut.DOJ = response.data[0].DOJ;
                $scope.employeeInfoOut.DOC = response.data[0].DOC;
                $scope.employeeInfoOut.EmailId = response.data[0].EmailId;
                $scope.employeeInfoOut.Code = response.data[0].Code;
                $scope.employeeInfoOut.Section = response.data[0].Section;
                $scope.employeeInfoOut.SubSection = response.data[0].SubSection;
                $scope.employeeInfoOut.Department = response.data[0].Department;
                $scope.employeeInfoOut.LegalDesignation = response.data[0].LegalDesignation;
                $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
            }
            else {
                ShowResult("Please Select Correct Employee Code", 'failure');
            }
        });
    };


    $scope.dataListOut = [];
    $scope.employeeInfoOut = { EmployeeCode: null };
    $scope.employeeInfoOut = {};
    $scope.GetEmployeeInfoOut = function () {
        $scope.dataListOut = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataListOut = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUpOut')).modal('show');
    };

    $scope.AttendanceEntryInOriginal = {
        Id: null,
        PType: 'IN',
        PDate: $filter('dateFiltering')(Date.now()),
        InTime: new Date(),
        EmployeeId: null,
        Latitude: null,
        Longitude: null,
        Remarks: null,
        Remarks: null,
        IsProcessed: false,
        IsLocked: false,
        SourceFlag: null,
        INLocationDesc: null,
        OutLocationDesc: null,
        isApprovedIN: false,
        ApprovedByIN: null,
        ApprovalDateIN: null,
        isApprovedOUT: false,
        ApprovedByOUT: null,
        ApprovalDateOUT: null,
        LatitudeOUT: null,
        LongitudeOUT: null,
        RemarksOUT: null,
        LocationDesc: null,
    }
    $scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);

    $scope.AttendanceEntryOutOriginal = {
        Id: null,
        PType: 'OUT',
        PDate: $filter('dateFiltering')(Date.now()),
        OutTime: new Date(),
        EmployeeId: null,
        Latitude: null,
        Longitude: null,
        Remarks: null,
        Remarks: null,
        IsProcessed: false,
        IsLocked: false,
        SourceFlag: null,
        INLocationDesc: null,
        OutLocationDesc: null,
        isApprovedIN: false,
        ApprovedByIN: null,
        ApprovalDateIN: null,
        isApprovedOUT: false,
        ApprovedByOUT: null,
        ApprovalDateOUT: null,
        LatitudeOUT: null,
        LongitudeOUT: null,
        RemarksOUT: null,
        LocationDesc: null,
    }
    $scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);

    $scope.SetData = function (obj) {
        $scope.employeeInfo = {};
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.DOC = emp.DOC;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Section = emp.Section;
        $scope.employeeInfo.SubSection = emp.SubSection;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        //$scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
        $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
    };

    $scope.SetDataOut = function (obj) {
        $scope.employeeInfoOut = {};
        var empOut = obj.data;
        $scope.employeeInfoOut.EmpSystemID = empOut.SystemID;
        $scope.employeeInfoOut.EmpPic = virtualPath.EmployeePic + empOut.EmpPicPath;
        $scope.employeeInfoOut.EmployeeCode = empOut.EmployeeCode;
        $scope.employeeInfoOut.EmployeeName = empOut.EmployeeName;
        $scope.employeeInfoOut.DOJ = empOut.DOJ;
        $scope.employeeInfoOut.DOC = empOut.DOC;
        $scope.employeeInfoOut.EmailId = empOut.EmailId;
        $scope.employeeInfoOut.Code = empOut.Code;
        $scope.employeeInfoOut.Section = empOut.Section;
        $scope.employeeInfoOut.SubSection = empOut.SubSection;
        $scope.employeeInfoOut.Department = empOut.Department;
        $scope.employeeInfoOut.LegalDesignation = empOut.LegalDesignation;
        angular.element(document.querySelector('#employeeNewPopUpOut')).modal('hide');
        // $scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
        $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);

    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }
    $scope.closeEmployeePopUpOut = function () {
        angular.element(document.querySelector('#employeeNewPopUpOut')).modal('hide');
    }

    $scope.AttendanceEntryInList = [];
    $scope.GetPreData = function (empId, WorkDate) {
        // $scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
        $scope.AttendanceEntryInList = [];
        $http.get('Attendances/AttendanceEntry/GetOffDuty?empId=' + empId + '&FromDate=' + WorkDate)
            .then(function (response) {
                $scope.AttendanceEntryInList = response.data;
            });
    };

    $scope.AttendanceEntryOUTList = [];
    $scope.GetPreDataOut = function (empId, WorkDate) {
        //$scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
        $scope.AttendanceEntryOUTList = [];
        $http.get('Attendances/AttendanceEntry/GetAttendanceEntry?empId=' + empId + '&FromDate=' + WorkDate)
            .then(function (response) {
                $scope.AttendanceEntryOUTList = response.data;
            });
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationIN() {
        try {
            CheckField("Work Date", $scope.AttendanceEntryIn.PDate);
            CheckField("In Time", $scope.AttendanceEntryIn.InTime);
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationOUT() {
        try {
            CheckField("Work Date", $scope.AttendanceEntryOut.PDate);
            CheckField("Out Time", $scope.AttendanceEntryOut.OutTime);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
        $scope.AttendanceEntryIn = Object.assign({}, args.data);
        $scope.Action = 'Update';
    };

    $scope.recorddoubleclickOut = function (args) {
        $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
        $scope.AttendanceEntryOut = Object.assign({}, args.data);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        try {
            //var currentMinute = new Date().getMinutes();
            var datetime = $filter('date')(Date.now(),'dd-MMM-yyyy hh:mm a')
            if ($scope.AttendanceEntryIn.PType ='IN') {
                if (datetime < $scope.AttendanceEntryIn.InTime) {
                    throw 'Future Time is not allowed..';
                }
            }

            $scope.AttendanceEntryIn.EmployeeId = $scope.employeeInfo.EmpSystemID
            if (baseService.isUndefinedOrNull($scope.AttendanceEntryIn.EmployeeId)) {
                throw 'Please Select Employee...';
            }
            ValidationIN();

            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.AttendanceEntryIn,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
                        //$scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
                        $scope.AttendanceEntryIn.PDate = $filter('dateFiltering')(Date.now());
                        $scope.AttendanceEntryIn.InTime = new Date();
                        $scope.AttendanceEntryIn.PType = 'IN';
                        $scope.Action = 'Save';
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };

            }

            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.AttendanceEntryIn,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Save';
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
                        //$scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
                        $scope.AttendanceEntryIn.PDate = $filter('dateFiltering')(Date.now());
                        $scope.AttendanceEntryIn.InTime = new Date();
                        $scope.AttendanceEntryIn.PType = 'IN';
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveOut = function () {
        try {
            var datetime = $filter('date')(Date.now(), 'dd-MMM-yyyy hh:mm a')
            if ($scope.AttendanceEntryOut.PType = 'OUT') {
                if (datetime < $scope.AttendanceEntryOut.OutTime) {
                    throw 'Future Time is not allowed..';
                }
            }
            $scope.AttendanceEntryOut.EmployeeId = $scope.employeeInfoOut.EmpSystemID
            if (baseService.isUndefinedOrNull($scope.AttendanceEntryOut.EmployeeId)) {
                throw 'Please Select Employee...';
            }
            ValidationOUT();
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.AttendanceEntryOut,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
                        //$scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
                        $scope.AttendanceEntryOut.PDate = $filter('dateFiltering')(Date.now());
                        $scope.AttendanceEntryOut.OutTime = new Date();
                        $scope.AttendanceEntryOut.PType = 'OUT';
                        $scope.Action = 'Save';
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.AttendanceEntryOut,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Save';
                        $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
                        // $scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
                        $scope.AttendanceEntryOut.PDate = $filter('dateFiltering')(Date.now());
                        $scope.AttendanceEntryOut.OutTime = new Date();
                        $scope.AttendanceEntryOut.PType = 'OUT';
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
        $scope.employeeInfo = {};
        $scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
        $scope.Action = 'Save';
        $scope.AttendanceEntryInList = [];
        $scope.AttendanceEntryIn.PDate = $filter('dateFiltering')(Date.now());
        $scope.AttendanceEntryIn.InTime = new Date();
        $scope.AttendanceEntryIn.PType = 'IN';
    }
    $scope.ClearOut = function () {
        ClearFieldsOut();
    };
    function ClearFieldsOut() {
        $scope.employeeInfoOut = {};
        $scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
        $scope.Action = 'Save';
        $scope.AttendanceEntryOUTList = [];
        $scope.AttendanceEntryOut.PDate = $filter('dateFiltering')(Date.now());
        $scope.AttendanceEntryOut.OutTime = new Date();
        $scope.AttendanceEntryOut.PType = 'OUT';
    }

    $scope.Delete = function () {
        $scope.AttendanceEntryIn.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.AttendanceEntryIn.Id)) {
            $http.get('Attendances/AttendanceEntry/Delete?Id=' + $scope.AttendanceEntryIn.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID, $scope.AttendanceEntryIn.PDate);
                        //$scope.AttendanceEntryIn = Object.assign({}, $scope.AttendanceEntryInOriginal);
                        $scope.AttendanceEntryIn.PDate = $filter('dateFiltering')(Date.now());
                        $scope.AttendanceEntryIn.InTime = new Date();
                        $scope.AttendanceEntryIn.PType = 'IN';
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteOut = function () {
        $scope.AttendanceEntryOut.EmpSystemId = $scope.employeeInfoOut.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.AttendanceEntryOut.Id)) {
            $http.get('Attendances/AttendanceEntry/DeleteOut?Id=' + $scope.AttendanceEntryOut.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreDataOut($scope.employeeInfoOut.EmpSystemID, $scope.AttendanceEntryOut.PDate);
                        $scope.AttendanceEntryOut = Object.assign({}, $scope.AttendanceEntryOutOriginal);
                        //$scope.AttendanceEntryOut.PDate = $filter('dateFiltering')(Date.now());
                        //$scope.AttendanceEntryOut.OutTime = new Date();
                        //$scope.AttendanceEntryOut.PType = 'OUT';
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}