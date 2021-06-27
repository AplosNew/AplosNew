'use strict';
complianceAttendanceSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function complianceAttendanceSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Compliance Attendance Setting';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/ComplianceAttendanceSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.ComplianceAttendanceModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

    $scope.ComplianceAttendanceModel = {
        Id: null,
        MaxOTPerDay: null,
        MaxExtraOTPerDay: null,
        IsNoPunchOnWeekOffForOTEntitle: false,
        IsNoPunchOnWeekOffForOTNotEntitle: false,
        IsNoPunchOnHolidayForOTEntitle: false,
        IsNoPunchOnHolidayForOTNotEntitle: false,
        PlantID: null,
        GroupID: null,
      
    };
    $scope.ComplianceAttendanceList = [];
    $scope.getListData = function () {
        $scope.ComplianceAttendanceModel.Id = null;
        $scope.ComplianceAttendanceModel.MaxOTPerDay= null;
        $scope.ComplianceAttendanceModel.MaxExtraOTPerDay= null;
        $scope.ComplianceAttendanceModel.IsNoPunchOnWeekOffForOTEntitle = false;
        $scope.ComplianceAttendanceModel.IsNoPunchOnWeekOffForOTNotEntitle = false;
        $scope.ComplianceAttendanceModel.IsNoPunchOnHolidayForOTEntitle= false;
        $scope.ComplianceAttendanceModel.IsNoPunchOnHolidayForOTNotEntitle = false;
        $http({
            method: 'POST',
            url: $scope.path + "getComplianceAttendancelist",
            data: { PlantID: $scope.ComplianceAttendanceModel.PlantID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                if (!baseService.isUndefinedOrNull(response.data)) {
                    $scope.ComplianceAttendanceModel.Id = response.data[0].Id;
                    $scope.ComplianceAttendanceModel.MaxOTPerDay = response.data[0].MaxOTPerDay;
                    $scope.ComplianceAttendanceModel.MaxExtraOTPerDay = response.data[0].MaxExtraOTPerDay;
                    $scope.ComplianceAttendanceModel.IsNoPunchOnWeekOffForOTEntitle = response.data[0].IsNoPunchOnWeekOffForOTEntitle;
                    $scope.ComplianceAttendanceModel.IsNoPunchOnWeekOffForOTNotEntitle = response.data[0].IsNoPunchOnWeekOffForOTNotEntitle;
                    $scope.ComplianceAttendanceModel.IsNoPunchOnHolidayForOTEntitle = response.data[0].IsNoPunchOnHolidayForOTEntitle;
                    $scope.ComplianceAttendanceModel.IsNoPunchOnHolidayForOTNotEntitle = response.data[0].IsNoPunchOnHolidayForOTNotEntitle;
                }
            }
        });
    }

    

    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ComplianceAttendance': $scope.ComplianceAttendanceModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ComplianceAttendanceModel.Id)) {
            $http.get('Attendances/ComplianceAttendanceSetting/Delete?Id=' + $scope.ComplianceAttendanceModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                        $scope.getListData();                        
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.ComplianceAttendanceModel = {
            Id: null,
            MaxOTPerDay: null,
            MaxExtraOTPerDay: null,
            IsNoPunchOnWeekOffForOTEntitle: false,
            IsNoPunchOnWeekOffForOTNotEntitle: false,
            IsNoPunchOnHolidayForOTEntitle: false,
            IsNoPunchOnHolidayForOTNotEntitle: false,
            CompanyId: $scope.ComplianceAttendanceModel.CompanyId,
            PlantID: $scope.ComplianceAttendanceModel.PlantID,
        };
    }

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
            CheckField("MaxOTPerDay", $scope.ComplianceAttendanceModel.MaxOTPerDay);         
        } catch (ex) {
            throw ex;
        }
    }

}