'use strict';
BulkIncrementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function BulkIncrementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Bulk Increment';
    $scope.path = 'Payrolls/BulkIncrement/';
    $scope.getEmpListWithSalaryUrl = $scope.path + 'GetEmployeeListWithSalaryInfo';
    $scope.getEmpListWithSalaryByJoinDateUrl = $scope.path + 'GetEmployeeListWithSalaryInfoByJoinDate';
    $scope.getAllEmpListWithSalaryUrl = $scope.path + 'GetAllEmployeeListWithSalaryInfo';
    $scope.getAllIncrementedEmpListWithSalaryUrl = $scope.path + 'GetAllIncrementedEmployeeListWithSalaryInfo';



    //#region Tab




    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };


    // #endregion Tab







    $scope.showbtn = false;
    $scope.custompara = {
        Head: null,
        Percentage: null,
        EffectiveDate: null,
        NextDueDate: null,
        IncrementedEffectiveDate: null,
        IncrementedNextDueDate: null,
        EmployeeLoadType: 'all',
        MonthNo: null,
        LoadEffectiveDate: null,
        MonthNoDOJ: null,
        LoadEffectiveDateDOJ: null
    };
    $scope.EmployeeListWithSalaryInfo = [];
    $scope.IncrementedEmployeeListWithSalaryInfo = [];
    $scope.GetEmployeeListWithSalaryInfo = function () {
        try {

            if ($scope.custompara.EmployeeLoadType === 'custom') {
                if (baseService.isUndefinedOrNull($scope.custompara.LoadEffectiveDate)) {
                    throw "Select Date...";
                }

                var LEDate = $filter('dateFiltering')($scope.custompara.LoadEffectiveDate, 'dd-M-yyyy');
                $http.get($scope.getEmpListWithSalaryUrl + '?MonthNo=' + $scope.custompara.MonthNo + '&LoadEffectiveDate=' + LEDate)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.EmployeeListWithSalaryInfo = [];
                            $scope.EmployeeListWithSalaryInfo = response.data;

                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });
            }
            if ($scope.custompara.EmployeeLoadType === 'all') {

                $http.get($scope.getAllEmpListWithSalaryUrl)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.EmployeeListWithSalaryInfo = [];
                            $scope.EmployeeListWithSalaryInfo = response.data;

                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });
            }
            if ($scope.custompara.EmployeeLoadType === 'customJoinDate') {
                if (baseService.isUndefinedOrNull($scope.custompara.LoadEffectiveDateDOJ)) {
                    throw "Select Date...";
                }

                var LEDates = $filter('dateFiltering')($scope.custompara.LoadEffectiveDateDOJ, 'dd-M-yyyy');
                $http.get($scope.getEmpListWithSalaryByJoinDateUrl + '?MonthNo=' + $scope.custompara.MonthNoDOJ + '&LoadEffectiveDate=' + LEDates)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.Message, 'failure');
                        }
                        else {
                            $scope.EmployeeListWithSalaryInfo = [];
                            $scope.EmployeeListWithSalaryInfo = response.data;

                        }
                    },

                        function errorCallBack(response) {
                            ShowResult(response.Message, 'failure');
                        });
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    //$scope.GetEmployeeListWithSalaryInfo();

    $scope.GetAllIncrementedEmployeeListWithSalaryInfo = function () {
        try {


            $http.get($scope.getAllIncrementedEmpListWithSalaryUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.IncrementedEmployeeListWithSalaryInfo = [];
                        $scope.IncrementedEmployeeListWithSalaryInfo = response.data;

                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetAllIncrementedEmployeeListWithSalaryInfo();
    $scope.GetCalculateEmpListWithSalaryInfo = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.custompara.Head)) {
                throw "Select Head...";
            }
            if (baseService.isUndefinedOrNull($scope.custompara.Percentage)) {
                throw "Please Enter Percentage...";
            }

            var EmployeeList = [];

            if ($scope.custompara.Head == 'Basic') {


                for (var i = 0; i < $scope.EmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.EmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        $scope.EmployeeListWithSalaryInfo[i].Basic = Math.round($scope.EmployeeListWithSalaryInfo[i].BasicOld + $scope.EmployeeListWithSalaryInfo[i].BasicOld * $scope.custompara.Percentage / 100);
                    }
                }


                for (var i = 0; i < $scope.EmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.EmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        EmployeeList.push($scope.EmployeeListWithSalaryInfo[i]);
                    }
                }
                if (EmployeeList.length == 0) {
                    throw "Select Employee...";
                }
                $http({
                    method: 'POST',
                    url: $scope.path + 'Calculate',
                    data: { 'BulkIncrement': EmployeeList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        // ShowResult(response.data.Message, 'success');
                        $scope.EmployeeListWithSalaryInfo = [];
                        $scope.EmployeeListWithSalaryInfo = response.data.data;
                    }
                }, function errorCallback(response) {
                    //ShowResult(response.status.Message, 'failure');
                });


            }
            if ($scope.custompara.Head == 'Gross') {
                for (var i = 0; i < $scope.EmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.EmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        $scope.EmployeeListWithSalaryInfo[i].Gross = Math.round($scope.EmployeeListWithSalaryInfo[i].GrossOld + $scope.EmployeeListWithSalaryInfo[i].GrossOld * $scope.custompara.Percentage / 100);
                        $scope.EmployeeListWithSalaryInfo[i].Amount = Math.round($scope.EmployeeListWithSalaryInfo[i].GrossOld + $scope.EmployeeListWithSalaryInfo[i].GrossOld * $scope.custompara.Percentage / 100);

                    }
                }

                if ($scope.EmployeeListWithSalaryInfo.length == 0) {
                    throw "Select Employee...";
                }

            }





            var gridObj = $("#GridEmployeeListWithSalaryInfos").data("ejGrid");
            gridObj.refreshContent();
            $scope.showbtn = true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveCalculateEmpListWithSalaryInfo = function () {
        try {


            if (baseService.isUndefinedOrNull($scope.custompara.EffectiveDate)) {
                throw "Please Enter Effective Date...";
            }
            if (baseService.isUndefinedOrNull($scope.custompara.NextDueDate)) {
                throw "Please Enter Next Due Date...";
            }
            var EmployeeList = [];
            for (var i = 0; i < $scope.EmployeeListWithSalaryInfo.length; i++) {
                if ($scope.EmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                    EmployeeList.push($scope.EmployeeListWithSalaryInfo[i]);
                }
            }
            if (EmployeeList.length == 0) {
                throw "Select Employee...";
            }
            $http({
                method: 'POST',
                url: $scope.path + 'Save',
                data: { 'BulkIncrement': EmployeeList, 'custompara': $scope.custompara },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.showbtn = false;
                    $scope.GetAllIncrementedEmployeeListWithSalaryInfo();

                    $scope.EmployeeListWithSalaryInfo = [];
                    //$scope.EmployeeListWithSalaryInfo = response.data.data;
                }
            }, function errorCallback(response) {
                //ShowResult(response.status.Message, 'failure');
            });





        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEmployeeListWithSalaryInfos").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeListWithSalaryInfo.length; i++) {
                $scope.EmployeeListWithSalaryInfo[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmployeeListWithSalaryInfos").data("ejGrid");
        gridObj.refreshContent();
    };





    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteSelected1();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridEmployeeListWithSalaryInfos").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridEmployeeListWithSalaryInfos").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };

    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridIncrementedEmployeeListWithSalaryInfos").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridIncrementedEmployeeListWithSalaryInfos").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };






    $scope.refreshTemplateemployee1 = function (args) {
        $("#headchk1").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise1 });
    };

    function CheckBoxSelectAllEmolyeeWise1(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridIncrementedEmployeeListWithSalaryInfos").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.IncrementedEmployeeListWithSalaryInfo.length; i++) {
                $scope.IncrementedEmployeeListWithSalaryInfo[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridIncrementedEmployeeListWithSalaryInfos").data("ejGrid");
        gridObj.refreshContent();
    };

    //incremented
    $scope.GetCalculateincrementedEmpListWithSalaryInfo = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.custompara.Head)) {
                throw "Select Head...";
            }
            if (baseService.isUndefinedOrNull($scope.custompara.Percentage)) {
                throw "Please Enter Percentage...";
            }

            var EmployeeList = [];

            if ($scope.custompara.Head == 'Basic') {


                for (var i = 0; i < $scope.IncrementedEmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.IncrementedEmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        $scope.IncrementedEmployeeListWithSalaryInfo[i].Basic = Math.round($scope.IncrementedEmployeeListWithSalaryInfo[i].BasicOld + $scope.IncrementedEmployeeListWithSalaryInfo[i].BasicOld * $scope.custompara.Percentage / 100);

                    }
                }


                for (var i = 0; i < $scope.IncrementedEmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.IncrementedEmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        EmployeeList.push($scope.IncrementedEmployeeListWithSalaryInfo[i]);
                    }
                }
                if (EmployeeList.length == 0) {
                    throw "Select Employee...";
                }
                $http({
                    method: 'POST',
                    url: $scope.path + 'Calculate',
                    data: { 'BulkIncrement': EmployeeList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        // ShowResult(response.data.Message, 'success');
                        $scope.IncrementedEmployeeListWithSalaryInfo = [];
                        $scope.IncrementedEmployeeListWithSalaryInfo = response.data.data;
                    }
                }, function errorCallback(response) {
                    //ShowResult(response.status.Message, 'failure');
                });


            }
            if ($scope.custompara.Head == 'Gross') {
                for (var i = 0; i < $scope.IncrementedEmployeeListWithSalaryInfo.length; i++) {
                    if ($scope.IncrementedEmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                        $scope.IncrementedEmployeeListWithSalaryInfo[i].Gross = Math.round($scope.IncrementedEmployeeListWithSalaryInfo[i].GrossOld + $scope.IncrementedEmployeeListWithSalaryInfo[i].GrossOld * $scope.custompara.Percentage / 100);
                        $scope.IncrementedEmployeeListWithSalaryInfo[i].Amount = Math.round($scope.IncrementedEmployeeListWithSalaryInfo[i].GrossOld + $scope.IncrementedEmployeeListWithSalaryInfo[i].GrossOld * $scope.custompara.Percentage / 100);

                    }
                }

                if ($scope.IncrementedEmployeeListWithSalaryInfo.length == 0) {
                    throw "Select Employee...";
                }

            }





            var gridObj = $("#GridIncrementedEmployeeListWithSalaryInfos").data("ejGrid");
            gridObj.refreshContent();
            $scope.showbtn = true;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveCalculateIncrementedEmpListWithSalaryInfo = function () {
        try {


            if (!baseService.isUndefinedOrNull($scope.custompara.IncrementedEffectiveDate)) {
                $scope.custompara.EffectiveDate = $scope.custompara.IncrementedEffectiveDate;
            }
            if (!baseService.isUndefinedOrNull($scope.custompara.IncrementedNextDueDate)) {
                $scope.custompara.NextDueDate = $scope.custompara.IncrementedNextDueDate;
            }



            if (baseService.isUndefinedOrNull($scope.custompara.EffectiveDate)) {
                throw "Please Enter Effective Date...";
            }
            if (baseService.isUndefinedOrNull($scope.custompara.NextDueDate)) {
                throw "Please Enter Next Due Date...";
            }
            var EmployeeList = [];
            for (var i = 0; i < $scope.IncrementedEmployeeListWithSalaryInfo.length; i++) {
                if ($scope.IncrementedEmployeeListWithSalaryInfo[i].CheckBoxSelect == true) {
                    EmployeeList.push($scope.IncrementedEmployeeListWithSalaryInfo[i]);
                }
            }
            if (EmployeeList.length == 0) {
                throw "Select Employee...";
            }
            $http({
                method: 'POST',
                url: $scope.path + 'Save',
                data: { 'BulkIncrement': EmployeeList, 'custompara': $scope.custompara },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.showbtn = false;
                    $scope.GetAllIncrementedEmployeeListWithSalaryInfo();
                    //$scope.EmployeeListWithSalaryInfo = [];
                    //$scope.EmployeeListWithSalaryInfo = response.data.data;
                }
            }, function errorCallback(response) {
                //ShowResult(response.status.Message, 'failure');
            });





        } catch (e) {
            ShowResult(e, "failure");
        }
    };
























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