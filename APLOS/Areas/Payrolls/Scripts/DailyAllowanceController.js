'use strict';
DailyAllowanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyAllowanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Daily Allowance';
    $scope.path = 'Payrolls/DailyAllowance/';
    $scope.getAllowanceUrl = $scope.path + 'GetAllowanceDaily';
    $scope.getShiftInfoUrl = $scope.path + 'GetShiftInfo';
    $scope.getDailyAllowanceUrl = $scope.path + 'GetDailyAllowance';
    $scope.SaveDailyAllowanceUrl = $scope.path + 'SaveDailyAllowance';
    $scope.deleteDailyAllowanceUrl = $scope.path + 'Delete';






    $scope.DailyAllowanceType = null;
    $scope.AllowanceList = [];
    $scope.getAllowance = function () {
        try {
            $http.get($scope.getAllowanceUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.AllowanceList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getAllowance();



    $scope.ShiftInfoList = [];
    $scope.getShiftInfo = function () {
        try {
            $http.get($scope.getShiftInfoUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.ShiftInfoList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getShiftInfo();

    $scope.DailyAllowanceList = [];
    $scope.getDailyAllowance = function () {
        try {
            $http.get($scope.getDailyAllowanceUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.DailyAllowanceList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getDailyAllowance();




    $scope.SaveDailyAllowanceData = function () {
       
        try {
            if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
                throw "Enter Allowance.";
            };
            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {
                if ($scope.ShiftInfoList[i].CheckBoxSelect===true) {
                    if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].EffectiveTime)) {
                        throw "Enter Effective Time.";
                    };
                    if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].FromDate)) {
                        throw "Enter From Date.";
                    };
                    if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].ToDate)) {
                        throw "Enter To Date.";
                    };
                }
            }
            $.ajax({
                type: "POST",
                url: $scope.SaveDailyAllowanceUrl,
                data: { 'DailyAllowanceData': $scope.ShiftInfoList, 'DailyAllowanceType': $scope.DailyAllowanceType },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.getDailyAllowance();
                        $scope.ShiftInfoList = [];
                        $scope.getShiftInfo();
                    }

                }

            });



        } catch (e) {
            ShowResult(e, "failure");
        }
    };

   



    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        //console.log('ok');


        if (e.model.checkState === "check") {

            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {
             
                $scope.ShiftInfoList[i].CheckBoxSelect = true;
            }
        }
        else {
            //console.log('co-ok');
            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {

                $scope.ShiftInfoList[i].CheckBoxSelect = false;


            }
        }
        //var gridObj = $("#GridShiftInfo").data("ejGrid");
        //gridObj.refreshContent();
    };

    $scope.custompara = {};
    $scope.message_confirmation = null;
    $scope.remove = function (obj) {
        var gridObj = $("#GridShiftInfoShow").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.custompara = data.Id;
        //if (!baseService.isUndefinedOrNull($scope.employeeInformation.SystemId))
            $scope.message_confirmation = 'Are you sure to Delete This Setting ?';
        angular.element(document.querySelector('#confirmPopUp')).modal('show');
    };

    $scope.Delete = function () {
       
        $.ajax({
            type: "POST",
            url: $scope.deleteDailyAllowanceUrl,
            data:
            {

                'Id': $scope.custompara
            },
            dataType: "json",
            success: function (response) {
                //$scope.ShowResult(data.Message, "success");
                ShowResult(response.Message, 'success');
                $scope.getDailyAllowance();
              
            }

        });
    };




    



}