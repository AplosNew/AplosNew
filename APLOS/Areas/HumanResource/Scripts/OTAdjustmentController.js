'use strict';
OTAdjustmentController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function OTAdjustmentController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'OT Adjustment';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/OTAdjustment/';
    $scope.showSubmit = true;
    $scope.customPara = {
        procdate: $filter('dateFiltering')(Date.now()),
        otcons: null,
        MinimumOTMinute: null,
        OTConsiderOn: null,
        OTFractionCalculate: null,
        IsPreallocationBasedOT: false,
        IsPunchBasedOT: false
    };
    $scope.Model = { AttendanceDate: new Date(), FromDate: new Date(), FromTime: '06:00 PM', ToDate: new Date, ToTime: '07:00 PM', Minutes: 60 };
    $scope.dataWithinTimeRange = [];
    $scope.dataWithoutTimeRange = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.ChangeAttendanceDate = function (args) {

        $scope.Model.FromDate = $scope.Model.AttendanceDate;
        $scope.Model.ToDate = $scope.Model.AttendanceDate;

    }
    $scope.Back = function () {
        $scope.showSubmit = true;
    }

    $scope.onloadRange = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkRange").ejCheckBox({ "change": headerCheckRange });
        }
    }
    $scope.onloadOutRange = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkOutRange").ejCheckBox({ "change": headerCheckOutRange });
        }
    }
    function headerCheckRange(e) {
        if (e.model.checkState == "check") {
            for (var i = 0; i < $scope.dataWithinTimeRange.length; i++) {
                $scope.dataWithinTimeRange[i].Active = false;
            }
            var gridObj = $("#GridWithinTimeRange").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length <= 0)
                filtereddata = $scope.dataWithinTimeRange;
            for (var i = 0; i < filtereddata.length; i++) {
                filtereddata[i].Active = true;
            }
        }
        else {
            for (var i = 0; i < $scope.dataWithinTimeRange.length; i++) {
                $scope.dataWithinTimeRange[i].Active = false;
            }


        }

        var gridObj = $("#GridWithinTimeRange").data("ejGrid");
        gridObj.refreshContent();
    }
    function headerCheckOutRange(e) {
        if (e.model.checkState == "check") {
            for (var i = 0; i < $scope.dataWithoutTimeRange.length; i++) {
                $scope.dataWithoutTimeRange[i].Active = false;
            }

            var gridObj = $("#GridWithoutTimeRange").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length <= 0)
                filtereddata = $scope.dataWithoutTimeRange;
            for (var i = 0; i < filtereddata.length; i++) {
                filtereddata[i].Active = true;
            }
        }
        else {
            for (var i = 0; i < $scope.dataWithoutTimeRange.length; i++) {
                $scope.dataWithoutTimeRange[i].Active = false;
            }
        }

        var gridObj = $("#GridWithoutTimeRange").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.HRMSSettings = {};
    $scope.GetHRMSSettings = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetHrmsSettings",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.HRMSSettings = response.data[0];

        });
    }
    $scope.SaveSingleEmployee = function () {
        var DATA1 = ej.DataManager($scope.dataWithinTimeRange).executeLocal(ej.Query().where("Active", "equal", true));
        var DATA2 = ej.DataManager($scope.dataWithoutTimeRange).executeLocal(ej.Query().where("Active", "equal", true));


        $http({
            method: 'POST',
            url: $scope.path + "SaveSingleEmployee",
            data: { parameters: $scope.Model, data1: DATA1, data2: DATA2 },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Submit();
            }

        });
    }
    $scope.Submit = function () {
        $scope.showSubmit = true;


        $scope.dataWithinTimeRange =[];
        $scope.dataWithoutTimeRange = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: { parameters: $scope.Model },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.showSubmit = false;


                ConvertToHourMinute(response.data.DATARange);
                ConvertToHourMinute(response.data.DATAOutRange);


                $scope.dataWithinTimeRange = response.data.DATARange;
                $scope.dataWithoutTimeRange = response.data.DATAOutRange;

            }

        });
    }
    $scope.rowDataBound = function (e) {

        if (e.data.OTHr != e.data.TotalOTHr)
            e.row.css("background-color", "#FF9F9F");


    }
    $scope.AdditionalMinutes = function () {
        //$scope.dataWithinTimeRange = [];
        //$scope.dataWithoutTimeRange = [];
        for (var i = 0; i < $scope.dataWithinTimeRange.length; i++) {
            if ($scope.dataWithinTimeRange[i].Active == true) {

                $scope.dataWithinTimeRange[i].NewOT = $scope.dataWithinTimeRange[i].TotalOTHr + $scope.Model.Minutes;
            }
            $scope.dataWithinTimeRange[i].NewOTDisplay = $scope.dataWithinTimeRange[i].NewOT;

        }
        for (var i = 0; i < $scope.dataWithoutTimeRange.length; i++) {
            if ($scope.dataWithoutTimeRange[i].Active == true) {

                $scope.dataWithoutTimeRange[i].NewOT = $scope.dataWithoutTimeRange[i].TotalOTHr + $scope.Model.Minutes;
            }
            $scope.dataWithoutTimeRange[i].NewOTDisplay = $scope.dataWithoutTimeRange[i].NewOT;

        }

        ConvertToHourMinute($scope.dataWithinTimeRange);
        ConvertToHourMinute($scope.dataWithoutTimeRange);


        var gridObj = $("#GridWithinTimeRange").data("ejGrid");
        gridObj.refreshContent();

        gridObj = $("#GridWithoutTimeRange").data("ejGrid");
        gridObj.refreshContent();


    }
    $scope.DeductionMinutes = function () {

        for (var i = 0; i < $scope.dataWithinTimeRange.length; i++) {
            if ($scope.dataWithinTimeRange[i].Active == true) {

                $scope.dataWithinTimeRange[i].NewOT = $scope.dataWithinTimeRange[i].TotalOTHr - $scope.Model.Minutes;
                if ($scope.dataWithinTimeRange[i].NewOT < 0)
                    $scope.dataWithinTimeRange[i].NewOT = 0;
            }

            $scope.dataWithinTimeRange[i].NewOTDisplay = $scope.dataWithinTimeRange[i].NewOT;

        }
        for (var i = 0; i < $scope.dataWithoutTimeRange.length; i++) {
            if ($scope.dataWithoutTimeRange[i].Active == true) {

                $scope.dataWithoutTimeRange[i].NewOT = $scope.dataWithoutTimeRange[i].TotalOTHr - $scope.Model.Minutes;
                if ($scope.dataWithoutTimeRange[i].NewOT < 0)
                    $scope.dataWithoutTimeRange[i].NewOT = 0;
            }
            $scope.dataWithoutTimeRange[i].NewOTDisplay = $scope.dataWithoutTimeRange[i].NewOT;

        }

        ConvertToHourMinute($scope.dataWithinTimeRange);
        ConvertToHourMinute($scope.dataWithoutTimeRange);


        var gridObj = $("#GridWithinTimeRange").data("ejGrid");
        gridObj.refreshContent();

        gridObj = $("#GridWithoutTimeRange").data("ejGrid");
        gridObj.refreshContent();

    }


    function ConvertToHourMinute(model) {
        // num.toString().padStart(3, "0")
        if ($scope.HRMSSettings.OTConsiderOn === 'Hour Minute Value') {
            for (var i = 0; i < model.length; i++) {
                model[i].NewOTDisplay = hourMinutes(model[i].NewOT);
                model[i].TotalOTHrDisplay = hourMinutes(model[i].TotalOTHr);
                model[i].OTHrDisplay = hourMinutes(model[i].OTHr);
            }

        }
    }

    function hourMinutes(decimal) {
        var hour = 0;
        var minutes = 0;
        while (decimal >= 60) {
            decimal = decimal - 60;
            hour++;
        }
        minutes = decimal;

        return hour.toString().padStart(2, "0") + ":" + minutes.toString().padStart(2, "0");
    }
}