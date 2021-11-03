'use strict';
SewingCodeController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function SewingCodeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Additional Element Code";
    $scope.Action = 'Save';
    $scope.path = 'IE/SewingCode/';

    $scope.VASMain = {
        Id: '',
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: '',
        TMU: null,
        MCHand: '',
        SPI: 0,
        RPM: 0,
        NoOfStart: 1,
        NoOfStop: 1,
        LengthInCM: 0,
        Activity: '',
        Element: ''
    };
    $scope.VAS = Object.assign({}, $scope.VASMain);
    $scope.sewingCodeList = [];
    $scope.handlingFactorList = [];
    $scope.stoppingAccuracyList = [];
    $scope.GeneralSetting = { EachStartTMU: 9.5, EachStopTMU: 8.5 };


    //$scope.handlingFactorList = [{ value: '1.00', name: 'N' }, { value: '1.10', name: 'L' }, { value: '1.20', name: 'M' }, { value: '1.30', name: 'H' }];
    //$scope.stoppingAccuracyList = [{ value: '0', name: 'A' }, { value: '9', name: 'B' }, { value: '17', name: 'C' }];


    $scope.getAllData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllData'

        }).then(function successCallback(response) {
            $scope.sewingCodeList = response.data;
        });
    };

    $scope.GetBasicSettings = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBasicSettings'
        }).then(function successCallback(response) {
            if (response.data.CS.length > 0)
                $scope.GeneralSetting = response.data.CS[0];

            $scope.stoppingAccuracyList = response.data.SA;
            $scope.handlingFactorList = response.data.HF;
        });
    };
    $scope.GetBasicSettings();
    $scope.recorddoubleclick = function ($event) {
        var x = $event;
        $scope.RowId = x.data.Id;
        $scope.PopulateSelectedDate($scope.RowId);
    };

    $scope.LoadSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.Id = $scope.selecteddata.Id;
        $scope.PopulateSelectedDate($scope.VAS.Id);
    };
    $scope.PopulateSelectedDate = function (Id) {
        $scope.Clear();
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedData',
            data: {
                'Id': Id
            }
        }).then(function successCallback(response) {
            $scope.VAS = response.data[0];
            $scope.Action = 'Update';

            $scope.getTMUValue();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }

        });
    };
    $scope.MakeCode = function () {
        if (angular.isUndefinedOrNull($scope.VAS["ShortName"]))
            $scope.VAS["ShortName"] = '';

        $scope.VAS.Code = "S" + parseInt($scope.VAS.LengthInCM) + $("#ddlMCHand option:selected").text() + "" + $("#ddlStoppingAccuracy option:selected").text() + "" + $scope.VAS.ShortName;

    }
    $scope.SaveData = function () {
       // $scope.VAS.Code = "S" + parseInt($scope.VAS.LengthInCM) + $("#ddlMCHand option:selected").text() + "" + $("#ddlStoppingAccuracy option:selected").text() + "" + $scope.VAS.ShortName;
        $scope.MakeCode();
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.ElementCodeForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.path + "SaveData",
                    data: { 'elementType': $scope.VAS },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.getAllData();
                        ShowResult(response.data.Message, 'success');
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.DeleteSelectedData = function (Id) {
        var x = "#" + Id;
        var gridObj = $(x).data("ejGrid");
        $scope.selecteddata = gridObj.getSelectedRecords()[0];
        $scope.VAS.Id = $scope.selecteddata.Id;

        $scope.message_confirmation = 'Are you sure want to Remove?';
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "DeleteSelectedData?Id=" + $scope.VAS.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.getAllData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CalculationsMain = { StraightMachineTMUX: 0, GuideAndTense: 0, StopAccuracy: 0, HandlingFactorInPercentage: 0, StraightMachineTMU: 0 };
    $scope.Calculations = Object.assign({}, $scope.CalculationsMain);


    $scope.getTMUValue = function () {

        if (!$scope.VAS.SPI || !$scope.VAS.RPM || !$scope.VAS.LengthInCM)
            return false;


        try {
            var _OperationLength = $scope.VAS.LengthInCM;
            var _SPC = $scope.VAS.SPI / 2.54;//Inch to CM factor
            var _RPM = $scope.VAS.RPM;
            var TMUFactor = 0.0006;
            var _StartTMU = $scope.GeneralSetting.EachStartTMU;
            var _StopTMU = $scope.GeneralSetting.EachStopTMU;

            var StraightMachineTMU = 0;
            if (_RPM != 0)
                StraightMachineTMU = ((_SPC * _OperationLength) / _RPM) / TMUFactor

            var TotalStartAndStopTMU = ($scope.VAS.NoOfStart * _StartTMU) + ($scope.VAS.NoOfStop * _StopTMU);


            var SelectedHandlingFactor = ej.DataManager($scope.handlingFactorList).executeLocal(ej.Query().where("Id", "equal", $scope.VAS.HandlingFactorId));
            var SelectedStoppingAccuracy = ej.DataManager($scope.stoppingAccuracyList).executeLocal(ej.Query().where("Id", "equal", $scope.VAS.StopAccuracyId));


            var StopAccuracyTMU = 0;
            if (SelectedStoppingAccuracy.length > 0)
                StopAccuracyTMU = SelectedStoppingAccuracy[0].ValueInTMU;

            var HandlingFactorPercent = 1.0;//default 100%
            if (SelectedHandlingFactor.length > 0)
                HandlingFactorPercent = SelectedHandlingFactor[0].AdditionalRate;


            var TotalTMU = StraightMachineTMU + TotalStartAndStopTMU + StopAccuracyTMU;




            var TotalMachineTime = TotalTMU * HandlingFactorPercent;


            $scope.Calculations.StraightMachineTMUX = StraightMachineTMU.toFixed(2);
            $scope.Calculations.StraightMachineTMU = TotalTMU.toFixed(2);
            $scope.Calculations.GuideAndTense = (TotalTMU * (HandlingFactorPercent - 1)).toFixed(2)// Math.ceil(_guideAndTense);
            $scope.Calculations.StopAccuracy = (StopAccuracyTMU).toFixed(2);
            $scope.Calculations.HandlingFactorInPercentage = ((HandlingFactorPercent - 1) * 100).toFixed(2);


            $scope.VAS.TMU = Math.ceil(TotalMachineTime);


            //$scope.VAS.Code = "S" + parseInt($scope.VAS.LengthInCM) + $("#ddlMCHand option:selected").text() + "" + $("#ddlStoppingAccuracy option:selected").text();

            //var _TMU = (parseFloat(_OutPut) * parseFloat(_High_Speed_Factor) * parseFloat(_guideAndTense) * parseFloat(SeamInch)) + _Start_And_End_Time + parseFloat(SA);



        } catch (e) {

        }

    }
    $scope.getTMUValue_ = function () {
        var SeamInch = $scope.VAS.LengthInCM;
        $("#lblSwingTime").text("");
        var _SPC = 0.00;
        var _SPI = $scope.VAS.SPI;
        var _RPM = $scope.VAS.RPM;
        var _OutPut = 0.0;
        var _Start_And_End_Time = $scope.GeneralSetting.EachStartTMU + $scope.GeneralSetting.EachStopTMU;// 17;
        var _guideAndTense = 0;
        var _Rate_Of_Feed = 0.00;
        var _High_Speed_Factor = 0.00;
        var SPT = 0.00;

        if (_SPI === undefined || _RPM === undefined || SeamInch === undefined)
            return false;

        if (!_SPI && !_RPM && !SeamInch) {
            return false;
        }
        else {
            _SPC = parseFloat(_SPI / 2.54);
            SPT = parseFloat(_RPM) * 0.0006;
        }
        _OutPut = ((parseFloat(_SPC)) / parseFloat(SPT));

        _Rate_Of_Feed = parseFloat(_RPM) / parseFloat(_SPC);

        if (parseFloat(_Rate_Of_Feed) > 445) {
            _High_Speed_Factor = ((parseFloat(4.5) - parseFloat(_OutPut)) * (parseFloat(4.5) - parseFloat(_OutPut)) / 100) + 1;
        }
        else {
            _High_Speed_Factor = 1;
        }




        var SelectedHandlingFactor = ej.DataManager($scope.handlingFactorList).executeLocal(ej.Query().where("Id", "equal", $scope.VAS.HandlingFactorId));
        var SelectedStoppingAccuracy = ej.DataManager($scope.stoppingAccuracyList).executeLocal(ej.Query().where("Id", "equal", $scope.VAS.StopAccuracyId));

        var HF = 0;
        if (SelectedHandlingFactor.length > 0)
            HF = SelectedHandlingFactor[0].AdditionalRate;

        var SA = 0;
        if (SelectedStoppingAccuracy.length > 0)
            SA = SelectedStoppingAccuracy[0].ValueInTMU;


        _guideAndTense = HF;// $scope.VAS.MCHand;


        $scope.Calculations.StraightMachineTMUX = Math.ceil(_OutPut);
        $scope.Calculations.GuideAndTense = Math.ceil(_guideAndTense);
        $scope.Calculations.StopAccuracy = Math.ceil(SA);



        //$("#lblStopAccuracy").text(SA);
        //$("#lblSwingTime").text(Math.ceil(_OutPut));
        //$("#lblGuideAndTense").text(Math.ceil(_guideAndTense));





        $scope.VAS.Code = "S" + parseInt(SeamInch) + $("#ddlMCHand option:selected").text() + "" + $("#ddlStoppingAccuracy option:selected").text() + "" + $scope.VAS.ShortName;

        var _TMU = (parseFloat(_OutPut) * parseFloat(_High_Speed_Factor) * parseFloat(_guideAndTense) * parseFloat(SeamInch)) + _Start_And_End_Time + parseFloat(SA);

        $scope.VAS.TMU = Math.ceil(_TMU);
    };

    $scope.Cancel = function () {
        $scope.Clear();
        $rootScope.toggle();
    };

    $scope.getAllData();

    $scope.Clear = function () {
        // $scope.VAS = { NoOfStart: 1, NoOfStop: 1, SPI: 0, RPM: 0 };
        $scope.VAS = Object.assign({}, $scope.VASMain);
        $scope.Calculations = Object.assign({}, $scope.CalculationsMain);
        //$scope.VAS.Id = '';
        //$scope.VAS.MCHand = '1.20';
        //$scope.VAS.StoppingAccuracy = '0';
        //$scope.VAS.Description = '';
    };
}