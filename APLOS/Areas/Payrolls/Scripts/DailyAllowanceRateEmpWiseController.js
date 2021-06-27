'use strict';
DailyAllowanceRateEmpWiseController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyAllowanceRateEmpWiseController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Daily Allowance';
    $scope.path = 'Payrolls/DailyAllowance/';
    $scope.getAllowanceUrl = $scope.path + 'GetAllowanceDaily';
    $scope.getAllowanceRateUrl = $scope.path + 'GetDailyAllowanceRate';
    
    $scope.getShiftInfoUrl = $scope.path + 'GetEmployeeCategoryInfo';
    $scope.getDailyAllowanceUrl = $scope.path + 'GetDailyAllowance';
    $scope.SaveDailyAllowanceUrl = $scope.path + 'SaveDailyAllowanceRate';
    $scope.deleteDailyAllowancerateUrl = $scope.path + 'DeleteRate';






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

    $scope.DailyAllowanceRateList = [];
    $scope.getDailyAllowanceRate = function () {
        try {
            $http.get($scope.getAllowanceRateUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.DailyAllowanceRateList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getDailyAllowanceRate();




    $scope.SaveDailyAllowanceData = function () {
       
        try {
            if (baseService.isUndefinedOrNull($scope.DailyAllowanceType)) {
                throw "Enter Allowance.";
            };
            for (var i = 0; i < $scope.ShiftInfoList.length; i++) {
                if ($scope.ShiftInfoList[i].CheckBoxSelect===true) {
                    if (baseService.isUndefinedOrNull($scope.ShiftInfoList[i].Rate)) {
                        throw "Enter Rate.";
                    };
                  
                }
            }
            $.ajax({
                type: "POST",
                url: $scope.SaveDailyAllowanceUrl,
                data: { 'DailyAllowanceRateData': $scope.ShiftInfoList, 'DailyAllowanceType': $scope.DailyAllowanceType },
                dataType: "json",
                success: function (data) {
                    if (data.Error === true) {
                        ShowResult(data.Message, "failure");
                    }
                    else {
                        ShowResult(data.Message, "success");
                        $scope.getDailyAllowanceRate();
                        $scope.ShiftInfoList = [];
                        $scope.getShiftInfo();
                    }

                }

            });



        } catch (e) {
            ShowResult(e, "failure");
        }
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
            url: $scope.deleteDailyAllowancerateUrl,
            data:
            {

                'Id': $scope.custompara
            },
            dataType: "json",
            success: function (response) {
                //$scope.ShowResult(data.Message, "success");
                ShowResult(response.Message, 'success');
                $scope.getDailyAllowanceRate();
              
            }

        });
    };

}