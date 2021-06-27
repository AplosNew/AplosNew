'use strict';
offDutyHoursController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function offDutyHoursController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Off Duty Hours';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.maternityLeaveTransactions = [];
    $scope.path = 'Leave/OffDutyHours/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.leaveTypelist = [];
    $scope.GetCbo = function () {
        $http.get('Leave/OffDutyHours/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.leaveTypelist = [];
                        $scope.leaveTypelist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.dataList = [];
        $scope.employeeInfo = {};
    $scope.GetEmployeeDeleteInfo = function () {
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: 'employees/EmployeeDelete/getemployeeDelete'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.OffDutyHoursModelOriginal = {
        Id: null,
        EmpSystemId: null,
        FromDate: null,
        ToDate: null,
        WorkDate: null,
        DurationInMin: 0,
        HourlyLeaveReasonId: null,
        DurationInHours: 0
    }
    $scope.OffDutyHoursModel = Object.assign({}, $scope.OffDutyHoursModelOriginal);

    $scope.ChangeDate = function (args) {
        if (args.isInteraction == true) {
            $scope.OffDutyHoursModel.WorkDate = $filter('dateFiltering')($scope.OffDutyHoursModel.FromDate, 'dd-MM-yyyy');
        }

         if (!baseService.isUndefinedOrNull($scope.OffDutyHoursModel.WorkDate)) {
            $scope.GetShiftData($scope.employeeInfo.EmpSystemID, $scope.OffDutyHoursModel.WorkDate);
        }
    }

    $scope.changeshiftInfo = function (args) {
        if (args.isInteraction == true) {
            if (!baseService.isUndefinedOrNull($scope.OffDutyHoursModel.WorkDate)) {
                $scope.GetShiftData($scope.employeeInfo.EmpSystemID, $scope.OffDutyHoursModel.WorkDate);

            }
        }
    }

    $scope.ChangeDatedoublefunction = function (args) {
        $scope.ChangeDate(args);
        $scope.ChangeDuration();
    }

    $scope.ChangeDuration = function () {

        //TWO DATE SELECT GET MINITE//
        //var diff = Math.abs(new Date($scope.OffDutyHoursModel.FromDate) - new Date($scope.OffDutyHoursModel.ToDate));
        //var minutes = Math.floor((diff / 1000) / 60);
        //$scope.OffDutyHoursModel.Duration = minutes;

        if (!baseService.isUndefinedOrNull($scope.OffDutyHoursModel.FromDate) && !baseService.isUndefinedOrNull($scope.OffDutyHoursModel.DurationInMin)) {
            //Date then minite get get new date//
            var dt = new Date($scope.OffDutyHoursModel.FromDate);
            var minutes = $scope.OffDutyHoursModel.DurationInMin;
            var d = dt.setTime(dt.getTime() + minutes * 60000);
            $scope.OffDutyHoursModel.ToDate = dt;
        }
    }

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
        $scope.OffDutyHoursModel = Object.assign({}, $scope.OffDutyHoursModelOriginal);
        $scope.GetShiftList = {};
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
        //$scope.GetShiftData($scope.employeeInfo.EmpSystemID);
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.GetOffDutyList = [];
    $scope.GetPreData = function (empId) {
        $scope.OffDutyHoursModel = Object.assign({}, $scope.OffDutyHoursModelOriginal);
        $scope.GetOffDutyList = [];
        $http.get('Leave/OffDutyHours/GetOffDuty?empId=' + empId)
            .then(function (response) {
                $scope.GetOffDutyList = response.data;

            });
    };

    $scope.GetShiftList = {};
    $scope.GetShiftData = function (EmpSystemID, WorkDate) {
        $scope.GetShiftList = {};
        $http.get('Leave/OffDutyHours/GetShiftInfo?EmpSystemID=' + EmpSystemID + '&WorkDate=' + WorkDate)
            .then(function (response) {
                if (!baseService.isUndefinedOrNull(response.data)) {
                    $scope.GetShiftList = response.data.ShiftInfo[0];
                }
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
            CheckField("From Date", $scope.OffDutyHoursModel.FromDate);
            CheckField("To Date", $scope.OffDutyHoursModel.ToDate);

        } catch (ex) {
            throw ex;
        }
    }

    $scope.recorddoubleclick = function (args) {
        //var gridObj = $("#Grid").data("ejGrid");
        $scope.OffDutyHoursModel = Object.assign({}, args.data); // gridObj.getSelectedRecords()[0];
        $scope.Action = 'Update';
    };

    $scope.Save = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.OffDutyHoursModel.HourlyLeaveReasonId)) {
                throw ("Leave Reason is required.");
            }

            if (baseService.isUndefinedOrNull($scope.OffDutyHoursModel.DurationInMin)) {
                throw ("Duration Min is required.");
            }

            if ($scope.OffDutyHoursModel.DurationInMin==0) {
                throw ("Duration Min Can't Be Zero.");
            }

            if (baseService.isUndefinedOrNull($scope.OffDutyHoursModel.FromDate)) {
                throw ("From Date is required.");
            }

            else if (baseService.isUndefinedOrNull($scope.OffDutyHoursModel.WorkDate)) {
                throw ("Work Date is required.");
            }

            else if (new Date($scope.OffDutyHoursModel.FromDate) < new Date($scope.OffDutyHoursModel.WorkDate)) {

                throw ("Work Date must be above or equal to From Date.");
            }

            $scope.OffDutyHoursModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
            ValidationMaster();
            if ($scope.OffDutyHoursForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.OffDutyHoursModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.OffDutyHoursModel = {};
                            $scope.OffDutyHoursModelOriginal = {};
                            $scope.GetShiftList = {};
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.OffDutyHoursModel,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Save';
                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                            $scope.OffDutyHoursModel = {};
                            $scope.OffDutyHoursModelOriginal = {};
                            $scope.GetShiftList = {};
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
        $scope.OffDutyHoursModel = {};
        $scope.GetShiftList = {};
        $scope.GetOffDutyList = [];
        $scope.Action = 'Save';
    }

    $scope.Delete = function () {
        $scope.OffDutyHoursModel.EmpSystemId = $scope.employeeInfo.EmpSystemID
        if (!baseService.isUndefinedOrNull($scope.OffDutyHoursModel.Id)) {
            $http.get('Leave/OffDutyHours/Delete?Id=' + $scope.OffDutyHoursModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        $scope.OffDutyHoursModel = {};
                        $scope.GetShiftList = {};
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };


    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var firstDay = new Date(y, m, 1);

    $scope.ManualOutTimeDateWise = {
        FromDate: $filter('dateFiltering')(firstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        ReportFormat: 'Excel',
       
    };

    $scope.GetHourlyOtReport = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_FromDate', true, "From Date is required.");
                ShowResult("From Date is required.", 'failure');
            }
            else if (baseService.isUndefinedOrNull($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_ToDate', true, "To Date is required.");
                ShowResult("To Date is required.", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.FromDate) > new Date($scope.ManualOutTimeDateWise.ToDate)) {
                manualValidation('div_FromDate', true, "From date must be below or equal to To Date");
                ShowResult("From date must be below or equal to To Date", 'failure');
            }
            else if (new Date($scope.ManualOutTimeDateWise.ToDate) < new Date($scope.ManualOutTimeDateWise.FromDate)) {
                manualValidation('div_ToDate', true, "To date must be above or equal to From Date.");
                ShowResult("To date must be above or equal to From Date.", 'failure');
            }
            else {
                var url = 'Leave/OffDutyHours/GetHourlyLeave?reportFormat=Excel' + ' &FromDate=' + $scope.ManualOutTimeDateWise.FromDate + ' &ToDate=' + $scope.ManualOutTimeDateWise.ToDate;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



}