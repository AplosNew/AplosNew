'use strict';
shiftTimeChangeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function shiftTimeChangeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Shift Time Change';
    $scope.path = 'Attendances/ShiftTimeChange/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.ModelList = [];
    $scope.getData = function () {
        $scope.stoppageNew = Object.assign({}, $scope.stoppage);
        $scope.ModelList = [];
        $http.get('Attendances/ShiftTimeChange/GetList')
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();

    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    var FirstDay = new Date(y, m, 1);
    $scope.ShiftTimeChange = {
        Id: null,
        SystemID: null,
        GroupID: null,
        PlantID: null,
        FromDate: $filter('dateFiltering')(FirstDay),
        ToDate: $filter('dateFiltering')(Date.now()),
        InTime: null,
        InTimeStartMargin: null,
        LateMargin: null,
        AbsentEndMargin: null,
        OutTime: null,
        OutTimeEndMargin: null,
        OTStartTime: null,
        BreakStratTime: null,
        BreakEndTime: null,
        BreakPeriod: null,
        WorkingHour: null,
        ShiftDefinationID: null,
        Remarks: null,
        ShiftType: null,
        ShiftDefinationName: null,
        ShiftSystemID: null,
        IsGapInclude: null,
        IncludeBreakTimeInOT: null,
        HalfDayAbsentMaxLimit: null,
        BreakPeriod: null,
        IsLateInApplicable: null,
        LateInMaxLimit: null,
        LateInToleranceMargin: null,
        IsEarlyOutApplicable: null,
        EarlyOutMaxLimit: null,
        EarlyOutToleranceMargin: null,
        IsLunchOutApplicable: null,
        LateMarginSeconds: "59",
        RawINDefinitionFrom: null,
        RawOUTDefinitionFrom: null,
        RawINDefinitionTo: null,
        RawOUTDefinitionTo: null,
        //INAfterOUTAsOTStart: false,
        HalfDayDuration: null,
        ShortDuration: null,
        MaxOutDuration: null,
        FullDayDuration: null,
        ShiftDuration: null,
        HoursWithoutOT: null,

    };
    $scope.ShiftTimeChangeModel = Object.assign({}, $scope.ShiftTimeChange);

   $scope.dataList = [];
   $scope.ShiftInfo = {};
    $scope.GetShiftInfo = function () {
        $http({
            method: 'GET',
            url: 'Attendances/ShiftTimeChange/getShift'
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
        });
        angular.element(document.querySelector('#ShiftPopUp')).modal('show');
    }

    $scope.ShiftInfo = {};
    $scope.SetShiftData = function (obj) {
        $scope.GetShiftInfo();
        var shi = obj.data;
        $scope.ShiftInfo.ShiftSystemID = shi.ShiftSystemID;
        $scope.ShiftTimeChangeModel.ShiftDefinationID = shi.ShiftSystemID;
        $scope.ShiftInfo.ShiftDefinationName = shi.ShiftDefinationName;
        $scope.ShiftInfo.ShiftDefinationDescription = shi.ShiftDefinationDescription;
        $scope.ShiftInfo.ShiftType = shi.ShiftType;
        $scope.ShiftTimeChangeModel.ShiftType = shi.ShiftType;
        $scope.ShiftTimeChangeModel.InTime = shi.InTime;
        $scope.ShiftTimeChangeModel.InTimeStartMargin = shi.InTimeStartMargin; 
        $scope.ShiftTimeChangeModel.LateMargin = shi.LateMargin;
        $scope.ShiftTimeChangeModel.AbsentEndMargin = shi.AbsentEndMargin;
        $scope.ShiftTimeChangeModel.OutTime = shi.OutTime;
        $scope.ShiftTimeChangeModel.OTStartTime = shi.OTStartTime;
        $scope.ShiftTimeChangeModel.IsGapInclude = shi.IsGapInclude;
        $scope.ShiftTimeChangeModel.OutTimeEndMargin = shi.OutTimeEndMargin;
        $scope.ShiftTimeChangeModel.WorkingHour = shi.WorkingHour;
        $scope.ShiftTimeChangeModel.BreakStratTime = shi.BreakStratTime;
        $scope.ShiftTimeChangeModel.BreakPeriod = shi.BreakPeriod;
        $scope.ShiftTimeChangeModel.IsLunchOutApplicable = shi.IsLunchOutApplicable;
        $scope.ShiftTimeChangeModel.IsLateInApplicable = shi.IsLateInApplicable;
        $scope.ShiftTimeChangeModel.LateInMaxLimit = shi.LateInMaxLimit;
        $scope.ShiftTimeChangeModel.LateInToleranceMargin = shi.LateInToleranceMargin;
        $scope.ShiftTimeChangeModel.BreakEndTime = shi.BreakEndTime;
        $scope.ShiftTimeChangeModel.IncludeBreakTimeInOT = shi.IncludeBreakTimeInOT;
        $scope.ShiftTimeChangeModel.HalfDayAbsentMaxLimit = shi.HalfDayAbsentMaxLimit;
        $scope.ShiftTimeChangeModel.IsEarlyOutApplicable = shi.IsEarlyOutApplicable;
        $scope.ShiftTimeChangeModel.EarlyOutMaxLimit = shi.EarlyOutMaxLimit;
        $scope.ShiftTimeChangeModel.EarlyOutToleranceMargin = shi.EarlyOutToleranceMargin;
        $scope.ShiftTimeChangeModel.LateMarginSeconds = shi.LateMarginSeconds;
        $scope.ShiftTimeChangeModel.RawINDefinitionFrom = shi.RawINDefinitionFrom;
        $scope.ShiftTimeChangeModel.RawOUTDefinitionFrom = shi.RawOUTDefinitionFrom;
        $scope.ShiftTimeChangeModel.RawINDefinitionTo = shi.RawINDefinitionTo;
        $scope.ShiftTimeChangeModel.RawOUTDefinitionTo = shi.RawOUTDefinitionTo;
        //$scope.ShiftTimeChangeModel.INAfterOUTAsOTStart = shi.INAfterOUTAsOTStart;
        $scope.ShiftTimeChangeModel.HalfDayDuration = shi.HalfDayDuration;
        $scope.ShiftTimeChangeModel.ShortDuration = shi.ShortDuration;
        $scope.ShiftTimeChangeModel.FullDayDuration = shi.FullDayDuration;
        $scope.ShiftTimeChangeModel.MaxOutDuration = shi.MaxOutDuration;
        $scope.ShiftTimeChangeModel.ShiftDuration = shi.ShiftDuration;
        $scope.ShiftTimeChangeModel.HoursWithoutOT = shi.HoursWithoutOT;

        angular.element(document.querySelector('#ShiftPopUp')).modal('hide');
    };

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPopUp')).modal('hide');
    }
    
    $scope.Save = function () {
        try {
            $scope.ShiftTimeChangeModel.ShiftDefinationID =$scope.ShiftInfo.ShiftSystemID
            if (baseService.isUndefinedOrNull($scope.ShiftTimeChangeModel.ShiftDefinationID)) {
                throw 'Please Select Shift';
            }
            if ($scope.ShiftTimeChangeModel.LateMarginSeconds > 60) {
                throw 'Seconds is not allow more then 60';
            }
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.ShiftTimeChangeModel ,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
           
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.recorddoubleclick = function () { 
        var gridObj = $("#GridShiftTimeChange").data("ejGrid");
        $scope.ShiftTimeChangeModel = gridObj.getSelectedRecords()[0];
     
        $scope.ShiftInfo.ShiftSystemID = $scope.ShiftTimeChangeModel.ShiftSystemID;
        $scope.ShiftInfo.ShiftDefinationName = $scope.ShiftTimeChangeModel.ShiftDefinationName;
        $scope.ShiftInfo.ShiftDefinationDescription = $scope.ShiftTimeChangeModel.ShiftDefinationDescription;
        $scope.ShiftInfo.ShiftType = $scope.ShiftTimeChangeModel.ShiftType;
          try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {

        }
         $scope.getData();
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ShiftTimeChangeModel.SystemID)) {
            $http.get('Attendances/ShiftTimeChange/Delete?SystemID=' + $scope.ShiftTimeChangeModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                        $scope.getData();
                        if ($rootScope.isCollapsed) {
                            $rootScope.toggle();
                        }
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
        $scope.ShiftTimeChangeModel = Object.assign({}, $scope.ShiftTimeChange);

        $scope.ShiftInfo.ShiftSystemID = null;
        $scope.ShiftTimeChangeModel.ShiftDefinationID = null;
        $scope.ShiftInfo.ShiftDefinationName = null;
        $scope.ShiftInfo.ShiftDefinationDescription = null;
        $scope.ShiftInfo.ShiftType = null;
        $scope.ShiftTimeChangeModel.ShiftType = null;
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("In Time", $scope.ShiftTimeChangeModel.InTime);
            CheckField("Time Start Margin", $scope.ShiftTimeChangeModel.InTimeStartMargin);
            CheckField("Out Time", $scope.ShiftTimeChangeModel.OutTime);
            CheckField("OT Start Time", $scope.ShiftTimeChangeModel.OTStartTime);
            CheckField("Out Time End Margin", $scope.ShiftTimeChangeModel.OutTimeEndMargin);
            CheckField("Shift Duration", $scope.ShiftTimeChangeModel.ShiftDuration);
            CheckField("HalfDay Duration", $scope.ShiftTimeChangeModel.HalfDayDuration);
            CheckField("FullDay Duration", $scope.ShiftTimeChangeModel.FullDayDuration);
            CheckField("ShortDay Duration", $scope.ShiftTimeChangeModel.ShortDuration);
            CheckField("MaxOut Duration", $scope.ShiftTimeChangeModel.MaxOutDuration);
            CheckField("Hours Without OT", $scope.ShiftTimeChangeModel.HoursWithoutOT);

        } catch (ex) {
            throw ex;
        }
    };

    $scope.ChangeLateInApplication = function () {
        $scope.ShiftTimeChangeModel.LateInMaxLimit = 0;
        $scope.ShiftTimeChangeModel.LateInToleranceMargin = 0;
    }

    $scope.ChangeEarlyOutApplication = function () {
        $scope.ShiftTimeChangeModel.EarlyOutMaxLimit = 0;
        $scope.ShiftTimeChangeModel.EarlyOutToleranceMargin = 0;
    }
}