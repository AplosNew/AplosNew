'use strict';
MultipleLeaveEncashmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function MultipleLeaveEncashmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Leave Encashment';
    $scope.Action = 'Save';
   
    $scope.path = 'Payrolls/LeaveEncashmentEntry/';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getYearlyCalendarUrl = $scope.path + 'LoadYearlyCalendar';
    $scope.getLoadLeaveEncashmentTypesUrl = $scope.path + 'LoadLeaveEncashmentTypes';
    $scope.getLvEncashmentUrl = $scope.path + 'GetLeaveEncashmentData';
    $scope.getLvEncashmentListUrl = $scope.path + 'GetSevedMultipleLeaveEncashmentData';
    $scope.saveLvEncashmentUrl = $scope.path + 'SaveMultipleLeaveEncashment';


    $scope.LeaveEncashmentList = [];
    $scope.GetMultipleLeaveEncashmentDataUrl = $scope.path + 'GetMultipleLeaveEncashmentData';
    $scope.GetMultipleLeaveEncashmentData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
                throw "Please Select Encashment Date";
            }

            //var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            //$scope.EmployeeModel = gridObj.getSelectedRecords()[0];

            //var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            //eDialog.close();

            $http.get($scope.GetMultipleLeaveEncashmentDataUrl  +'?YearNo=' + $scope.CustomPara.YearlyCalendarId + '&EffectiveDate=' + $scope.CustomPara.EncashmentDate)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveEncashmentList = [];
                        $scope.LeaveEncashmentList = response.data.LeaveInfo;

                        for (var i = 0; i < $scope.LeaveEncashmentList.length; i++) {
                            $scope.LeaveEncashmentList[i].DOJ = new Date($scope.LeaveEncashmentList[i].DOJ);
                            $scope.LeaveEncashmentList[i].Rate = (parseFloat($scope.LeaveEncashmentList[i].Rate)).toFixed(2);
                        }
                        $scope.btnSave = true;
                        $scope.LoadLeaveEncashmentList();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
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

        var filtered = $("#GridLeaveEncashmentList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.LeaveEncashmentList.length; i++) {
                $scope.LeaveEncashmentList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridLeaveEncashmentList").data("ejGrid");
        gridObj.refreshContent();
    };





    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
        $scope.actionCompleteSelectedSeved();
        
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLeaveEncashmentList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridLeaveEncashmentList").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    $scope.actionCompleteSelectedSeved = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridSavedLeaveEncashmentList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#GridSavedLeaveEncashmentList").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    
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
   


    $scope.CustomPara = {
        YearlyCalendarId: null,       
        EncashmentDate: new Date(),
        LeaveEncashmentType: null      
    };
    $scope.SalaryInfo = [];
    $scope.YearlyCalendar = [];
    $scope.LeaveEncashmentTypeList = [];


    $scope.LoadYearlyCalendarList = function () {
        try {

           
            $http.get($scope.getYearlyCalendarUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.YearlyCalendar = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadYearlyCalendarList();

    $scope.LoadLeaveEncashmentTypes = function () {
        try {


            $http.get($scope.getLoadLeaveEncashmentTypesUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.LeaveEncashmentTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadLeaveEncashmentTypes();


    
   



    $scope.SeavedLeaveEncashmentList = [];
    $scope.LoadLeaveEncashmentList = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
           
            $http.get($scope.getLvEncashmentListUrl + '?YearId=' + $scope.CustomPara.YearlyCalendarId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.SeavedLeaveEncashmentList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
   










    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
                throw "Please Select year";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
                throw "Please Select Encashment Date";
            }
           
            var EmployeeList = [];
            for (var i = 0; i < $scope.LeaveEncashmentList.length; i++) {
                if ($scope.LeaveEncashmentList[i].CheckBoxSelect == true) {
                    var model = {};
                    model.EmpSystemId = $scope.LeaveEncashmentList[i].EmpSystemId;
                    model.LeaveTypeId = $scope.LeaveEncashmentList[i].LeaveTypeId;
                    model.Days = $scope.LeaveEncashmentList[i].Days;
                    model.Rate = $scope.LeaveEncashmentList[i].Rate;
                    model.YearlyCalendarId = $scope.LeaveEncashmentList[i].YearlyCalendarId;
                    model.PaymentMode = $scope.LeaveEncashmentList[i].PaymentMode;
                    model.BasicAmmount = $scope.LeaveEncashmentList[i].BasicAmmount;
                    model.GrossAmmount = $scope.LeaveEncashmentList[i].GrossAmmount;
                    model.BankSystemID = $scope.LeaveEncashmentList[i].BankSystemID;
                    model.BankBranchId = $scope.LeaveEncashmentList[i].BankBranchId;
                    model.BankAccNo = $scope.LeaveEncashmentList[i].BankAccNo;
                    model.LegalDesignationId = $scope.LeaveEncashmentList[i].LegalDesignationId;
                    model.NewBroughtForward = $scope.LeaveEncashmentList[i].NewBroughtForward;
                    model.NewYearEndEncash = $scope.LeaveEncashmentList[i].NewYearEndEncash;
                    model.NewYearEndLapse = $scope.LeaveEncashmentList[i].NewYearEndLapse;
                    model.NewYearEndEncash = $scope.LeaveEncashmentList[i].NewYearEndEncash;
                    model.EncashmentDate = $scope.LeaveEncashmentList[i].EncashmentDate;
                    model.EmployeeCode = $scope.LeaveEncashmentList[i].EmployeeCode;

                    model.BroughtForward = $scope.LeaveEncashmentList[i].BroughtForward;
                    model.DaysCanBeSanctioned = $scope.LeaveEncashmentList[i].DaysCanBeSanctioned;
                    model.AvailedLeave = $scope.LeaveEncashmentList[i].AvailedLeave;
                    model.CarryForward = $scope.LeaveEncashmentList[i].CarryForward;
                    //model.Balance = $scope.LeaveEncashmentList[i].Balance;
                    EmployeeList.push(model);

                }
            }


           
            $http({
                method: 'POST',
                url: $scope.saveLvEncashmentUrl,
                data: { 'leaveEncashment': EmployeeList, 'YearlyCalendarId': $scope.CustomPara.YearlyCalendarId, 'EncashmentDate': $scope.CustomPara.EncashmentDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadLeaveEncashmentList();
                    $scope.GetMultipleLeaveEncashmentData();
                    $scope.btnSave = false;

                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }




    };




    
};