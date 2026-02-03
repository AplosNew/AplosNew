'use strict';
mailSendController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function mailSendController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Mail Send';
    $scope.path = 'Setups/MailSend/';
    $scope.getListUrl = $scope.path + 'Getlist';
    $scope.PathShiftChange = 'HumanResource/CompliedShiftAssignment/';

    $scope.ApprovedForResignationEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ApprovedForResignationEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendAccountDelayPosting
$scope.SendAccountDelayPosting = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendAccountDelayPosting',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//
    $scope.SendIncrementDueEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendIncrementDueEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.ApprovedEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ApprovedEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.PreApprovedEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'PreApprovedEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//done
    $scope.ProbationPeriodList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ProbationPeriodList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//done
    $scope.AppliedResignationEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'AppliedResignationEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.ResignationDueList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ResignationDueList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }//done SendResignationToBeApprovedList
    $scope.SaparatedEmployeeList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SaparatedEmployeeList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    //
    $scope.ResignationToBeApprovedList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ResignationToBeApprovedList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.DailyAttendanceNotification = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DailyAttendanceReport',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.compliedShiftRotationDate = null;
    $scope.CompliedShiftModal = function () {
        angular.element(document.querySelector('#rotateStartModal')).modal('show');
    };
    $scope.CompliedshiftChange = function () {
        $http({
            method: 'POST',
            url: $scope.PathShiftChange + 'CompliedshiftChange',
            params: {
                'rotationDate': $scope.compliedShiftRotationDate,
                'addedBy': "",
                'ip': "",
                'appVersion':""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.DailyMissedPunchReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DailyMissedPunchReport',
            params: {                
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.DailyAttendanceFromAppReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DailyAttendanceFromAppReport',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.ManualAttendanceNotification = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendManualAttendanceEmployeeList',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.SendDailyDevicePunchList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendDailyDevicePunchList',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.SendDailyAttendanceSummary = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendDailyAttendanceSummary',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.SendYesterdayAbsentNotificationList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendYesterdayAbsentNotificationList',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.SendYesterdayMissedPunchNotificationList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendYesterdayMissedPunchNotificationList',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.SendMonthlyAttendanceInformationReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendMonthlyAttendanceInformationReport',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.SendDailyAttendanceAuditReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendDailyAttendanceAuditReport',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.SendDailyProductionReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendDailyProductionReport',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLateAttendancePosting
    $scope.SendLateAttendancePosting = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendLateAttendancePosting',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLateAttendancePosting
    $scope.SendRunTaskNotification = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendRunTaskNotification',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': "",
                'companyId':"CG20171"
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLateAttendancePosting
    $scope.SendYestedayOverstayMail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendYestedayOverstayMail',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLateAttendancePosting
    $scope.SendTNAReportMail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendTNAReportMail',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendTNAReportMail
    $scope.SendLastFewDaysPayableCreatedMail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendLastFewDaysPayableCreatedMail',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLastFewDaysPayableCreatedMail

    $scope.SendLastFewDaysPaymentMadeMail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendLastFewDaysPaymentMadeMail',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };//SendLastFewDaysPaymentMadeMail


    $scope.SendEmpApprovalMail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SendEmpApprovalMailReport',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.SaveScanToPacking = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SaveScandataToBooking',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.SavePendingBankReconciliation = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SavePendingBankReconciliation',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.LVProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'LVProcess',
            params: {
                'addedBy': "",
                'ip': "",
                'appVersion': ""
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };


}