'use strict';
shiftCreationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function shiftCreationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Shift Creation';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Attendances/ShiftCreation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.ShiftDefinationModel = {
        SystemID: null,
        ShiftDefinationName: null,
        ShiftDefinationDescription: null,
        UserName: false,
        SequenceNo: 0,
        IsActive: true,
        DefaultShift: true,
        ShiftType: null,
        InTime: null,
        InTimeStartMargin: null,
        LateMargin: null,
        AbsentEndMargin: null,
        OutTime: null,
        OutTimeEndMargin: null,
        OTStartTime: 0,
        IsGapInclude: true,
        BreakStratTime: null,
        BreakEndTime: null,
        BreakPeriod: null,
        WorkingHour: null,
        EarlyIn: true,
        LateIn: true,
        LateInMargin: null,
        EarlyOut: null,
        EarlyOutMargin: null,
        EarlyInMargin: null,
        LateOutMargin: null,
        LateOut: true,
        LateOutRoundMargin: null,
        LateInRoundMargin: null,
        EarlyOutRoundMargin: null,
        EarlyInRoundMargin: null,
        LateOutRoundMarginType: null,
        LateInRoundMarginType: null,
        EarlyOutRoundMarginType: null,
        EarlyInRoundMarginType: null,
        IncludeBreakTimeInOT: null,
        ShortLeaveMaxLimit: null,
        HalfDayAbsentMaxLimit: null,
        EarlyOutMaxLimit: null,
        EarlyOutToleranceMargin: null,
        LateInToleranceMargin: null,
        IsLunchOutApplicable: true,
        IsEarlyOutApplicable: true,
        LateInMaxLimit: null,
        IsLateInApplicable: true,
        RawINDefinitionFrom: null,
        RawINDefinitionTo: null,
        RawOUTDefinitionFrom: null,
        RawOUTDefinitionTo: null,
        LateMarginSeconds: "59",
        INAfterOUTAsOTStart: false,
    };

    $scope.ShiftList = [];
    $scope.getListData = function () {
        $http.get('Attendances/ShiftCreation/getShiftlist')
            .then(
                function successCallback(response) {
                    $scope.ShiftList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.ShiftList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.getListData();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridLeavePolicy").data("ejGrid");
        $scope.ShiftDefinationModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {

        }
        $scope.getListData();
    };

    function validation() {
        try {

        }
        catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            ValidationMaster();
            if ($scope.ShiftDefinationModel.LateMarginSeconds > 60) {
                throw 'Seconds is not allow more then 60';
            }
            if ($scope.ShiftDefinationModel.EarlyIn == true && $scope.ShiftDefinationModel.EarlyInMargin == null) {
                throw 'Early In Margin can not Null';
            }
            if ($scope.ShiftDefinationModel.EarlyIn == true && $scope.ShiftDefinationModel.LateInRoundMargin == null) {
                throw 'Late In Round Margin can not Null';
            }
            if ($scope.ShiftDefinationModel.EarlyIn == true && $scope.ShiftDefinationModel.EarlyInRoundMarginType == null) {
                throw 'Early In Round Margin Type can not Null';
            }


            if ($scope.ShiftDefinationModel.LateIn == true && $scope.ShiftDefinationModel.LateInMargin == null) {
                throw 'Late In Margin Can Not Null';
            }
            if ($scope.ShiftDefinationModel.LateIn == true && $scope.ShiftDefinationModel.LateInMargin == null) {
                throw 'Late In Margin Can Not Null';
            }
            if ($scope.ShiftDefinationModel.LateIn == true && $scope.ShiftDefinationModel.LateInRoundMarginType == null) {
                throw 'Late In Round Margin Type Can Not Null';
            }


            if ($scope.ShiftDefinationModel.LateOut == true && $scope.ShiftDefinationModel.OTStartTime == null) {
                throw 'OT Start Time Can Not Null';
            }
            if ($scope.ShiftDefinationModel.LateOut == true && $scope.ShiftDefinationModel.LateOutRoundMargin == null) {
                throw 'Late Out Round Margin Can Not Null';
            }
            if ($scope.ShiftDefinationModel.LateOut == true && $scope.ShiftDefinationModel.LateOutRoundMarginType == null) {
                throw 'Late Out Round Margin Type Can Not Null';
            }

            if ($scope.ShiftDefinationModel.IsLateInApplicable == true && $scope.ShiftDefinationModel.LateInMaxLimit == null) {
                throw 'Late In MaxLimit Can Not Null';
            }
            if ($scope.ShiftDefinationModel.IsLateInApplicable == true && $scope.ShiftDefinationModel.LateInToleranceMargin == null) {
                throw 'Late In Tolerance Margin Can Not Null';
            }


            if ($scope.ShiftDefinationModel.IsEarlyOutApplicable == true && $scope.ShiftDefinationModel.EarlyOutMaxLimit == null) {
                throw 'Early Out MaxLimit Can Not Null';
            }
            if ($scope.ShiftDefinationModel.IsEarlyOutApplicable == true && $scope.ShiftDefinationModel.EarlyOutToleranceMargin == null) {
                throw 'Early Out Tolerance Margin Can Not Null';
            }

            $scope.$broadcast('show-errors-check-validity');
           
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'ShiftCreationData': $scope.ShiftDefinationModel},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Update';
                        $scope.Clear();
                        $scope.getListData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.ShiftDefinationModel,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Update';
                        $scope.Clear();
                        $scope.getListData();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
           
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ShiftDefinationModel.SystemID)) {
            $http.get('Attendances/ShiftCreation/Delete?SystemID=' + $scope.ShiftDefinationModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ShiftDefinationModel = {};
                        ClearFields();
                        $scope.getListData();
                        $scope.GetSequence();
                        
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function (obj) {
        ClearFields($scope.GetSequence());
    };

    function ClearFields(obj) {
        $scope.Action = 'Save';
        for (var i in obj) {
            obj[i] = null;
        }
        $scope.ShiftDefinationModel = {};
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

            CheckField("In Time", $scope.ShiftDefinationModel.InTime);
            CheckField("Out Time", $scope.ShiftDefinationModel.OutTime);
            CheckField("Break Strat Time", $scope.ShiftDefinationModel.BreakStratTime);
            CheckField("Break End Time", $scope.ShiftDefinationModel.BreakEndTime);
            CheckField("Shift Defination Name", $scope.ShiftDefinationModel.ShiftDefinationName);
            CheckField("Shift Defination Description", $scope.ShiftDefinationModel.ShiftDefinationDescription);
            CheckField("Shift Type", $scope.ShiftDefinationModel.ShiftType);

        } catch (ex) {
            throw ex;
        }
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.ShiftDefinationModel.SequenceNo = response.data[0].SequenceNo;
            });
    };
    $scope.GetSequence();

    $scope.ChangeEarlyIn = function () {
        $scope.ShiftDefinationModel.EarlyInMargin = null;
        $scope.ShiftDefinationModel.LateInRoundMargin = null;
        $scope.ShiftDefinationModel.EarlyInRoundMarginType = null;
    };
    $scope.ChangeLateIn = function () {
        $scope.ShiftDefinationModel.LateInMargin= null;
        $scope.ShiftDefinationModel.ShiftDefinationModelName = null;
        $scope.ShiftDefinationModel.LateInRoundMarginType = null;
    };
    $scope.ChangeLateOut = function () {
        $scope.ShiftDefinationModel.OTStartTime = 0;
        $scope.ShiftDefinationModel.LateOutRoundMargin = null;
        $scope.ShiftDefinationModel.LateOutRoundMarginType = null;
    };
    $scope.ChangeEarlyOut = function () {
        $scope.ShiftDefinationModel.EarlyOutMargin = null;
        $scope.ShiftDefinationModel.EarlyOutRoundMargin = null;
        $scope.ShiftDefinationModel.EarlyOutRoundMarginType = null;
    };
    $scope.ChangeEarlyOutApplication = function () {
        $scope.ShiftDefinationModel.EarlyOutMaxLimit = 0;
        $scope.ShiftDefinationModel.EarlyOutToleranceMargin = 0;
    }
    $scope.ChangeLateInApplication = function () {
        $scope.ShiftDefinationModel.LateInMaxLimit = 0;
        $scope.ShiftDefinationModel.LateInToleranceMargin = 0;
    }
}