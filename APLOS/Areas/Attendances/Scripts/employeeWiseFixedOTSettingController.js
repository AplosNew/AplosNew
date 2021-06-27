'use strict';
employeeWiseFixedOTSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function employeeWiseFixedOTSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Wise Fixed OT Setting';
    $scope.Action = 'Save';
    $scope.index = -1;

    $scope.path = 'Attendances/EmployeeWiseFixedOTSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.dataList = [];
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.employeeInfo = {};
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.EmployeeWiseFixedOtSetting = {
        Id: null,
        EmpSystemId: null,
       // EffectiveDate: null,
        MinimumOT: null,
        IsExcessAllowed: null        
    }
    $scope.EmployeeWiseFixedOtSettingModel = Object.assign({}, $scope.EmployeeWiseFixedOtSetting);
    
    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
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
        $scope.EmployeeWiseFixedOtSetting = {};
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.EmployeeWiseFixedOTSettingList = [];
    $scope.GetPreData = function (empId) {
        $scope.EmployeeWiseFixedOtSettingModel = Object.assign({}, $scope.EmployeeWiseFixedOtSetting);
        $scope.EmployeeWiseFixedOTSettingList = [];
        $http.get('Attendances/EmployeeWiseFixedOTSetting/GetEmpWiseFOT?empId=' + empId)
            .then(function (response) {
                $scope.EmployeeWiseFixedOTSettingList = response.data;
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

    function ValidationMaster() {
        try {
            //CheckField("From Date", $scope.EmployeeWiseFixedOtSetting.EffectiveDate);
            CheckField("Minimum OT", $scope.EmployeeWiseFixedOtSetting.MinimumOT);

        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        $scope.EmployeeWiseFixedOtSetting = Object.assign({}, args.data);
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        try {           
            ValidationMaster();
            $scope.EmployeeWiseFixedOtSetting.EmpSystemId = $scope.employeeInfo.EmpSystemID
            if ($scope.EmployeeWiseFixedOTForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.EmployeeWiseFixedOtSetting,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.EmployeeWiseFixedOtSetting = {};
                           
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.EmployeeWiseFixedOtSetting,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.EmployeeWiseFixedOtSetting = {};
                           
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
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
        $scope.EmployeeWiseFixedOtSetting = {};
        $scope.EmployeeWiseFixedOTSettingList = [];
        $scope.Action = 'Save';
    }

    $scope.Delete = function () {
        $scope.EmployeeWiseFixedOtSetting.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.EmployeeWiseFixedOtSetting.Id)) {
            $http.get('Attendances/EmployeeWiseFixedOTSetting/Delete?Id=' + $scope.EmployeeWiseFixedOtSetting.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        $scope.EmployeeWiseFixedOtSetting = {};                      
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    

}