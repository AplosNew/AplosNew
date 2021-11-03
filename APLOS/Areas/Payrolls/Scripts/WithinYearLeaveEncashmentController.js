'use strict';
WithinYearLeaveEncashmentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function WithinYearLeaveEncashmentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Leave Encashment';
    $scope.Action = 'Save';
   
    $scope.path = 'Payrolls/LeaveEncashmentEntry/';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.getYearlyCalendarUrl = $scope.path + 'LoadYearlyCalendar';
    $scope.getLoadLeaveEncashmentTypesUrl = $scope.path + 'LoadLeaveEncashmentTypes';
    $scope.getLvEncashmentUrl = $scope.path + 'GetLeaveEncashmentData';
    $scope.getLvEncashmentListUrl = $scope.path + 'GetSevedWithInYearLeaveEncashmentData';
    $scope.saveLvEncashmentUrl = $scope.path + 'SaveWithInYearLeaveEncashment';

    $scope.custompara = {
        FromDate: null,
        ToDate: null,
        EmployeeLoadType: 'all'      
    };
    $scope.LeaveEncashmentList = [];
    $scope.GetWithInYearLeaveEncashmentDataUrl = $scope.path + 'GetWithInYearLeaveEncashmentData';
    $scope.GetWithInYearLeaveEncashmentData = function () {
        try {
          
            var ToDayDate = new Date();

            if ($scope.custompara.EmployeeLoadType === 'custom') {
                if (baseService.isUndefinedOrNull($scope.custompara.FromDate)) {
                    throw "Select  Date...";
                }
                //if (baseService.isUndefinedOrNull($scope.custompara.ToDate)) {
                //    throw "Select To Date...";
                //}

                var FromDate = $filter('dateFiltering')($scope.custompara.FromDate, 'dd-M-yyyy');
                //var ToDate = $filter('dateFiltering')($scope.custompara.ToDate, 'dd-M-yyyy');

                if (new Date(FromDate) > new Date(ToDayDate)) {
                    throw "Date less than or Equal todays date";
                }
                //if (new Date(ToDate) > new Date(ToDayDate)) {
                //    throw "Date less than or Equal todays date";
                //}


                $http.get($scope.GetWithInYearLeaveEncashmentDataUrl + '?FromDate=' + FromDate)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.LeaveEncashmentList = [];
                            $scope.LeaveEncashmentList = response.data.LeaveInfo;
                            $scope.YearNo = response.data.YearNo;
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

              
            }
            if ($scope.custompara.EmployeeLoadType === 'all') {
                if (baseService.isUndefinedOrNull($scope.custompara.ToDate)) {
                    $scope.custompara.ToDate = new Date('dd-MMM-yyyy');
                }
                $http.get($scope.GetWithInYearLeaveEncashmentDataUrl)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            $scope.LeaveEncashmentList = [];
                            $scope.LeaveEncashmentList = response.data.LeaveInfo;
                            $scope.YearNo = response.data.YearNo;

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
            }















            //var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            //$scope.EmployeeModel = gridObj.getSelectedRecords()[0];

            //var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            //eDialog.close();

           

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
    $scope.YearNo = null;
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

            //if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
            //    throw "Please Select year";
            //}
            if (baseService.isUndefinedOrNull($scope.custompara.FromDate)) {
                $scope.custompara.FromDate = new Date();
            }
            var ToDate = $filter('dateFiltering')($scope.custompara.FromDate, 'dd-M-yyyy');
            $http.get($scope.getLvEncashmentListUrl + '?ToDate=' + ToDate)
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
            //if (baseService.isUndefinedOrNull($scope.CustomPara.YearlyCalendarId)) {
            //    throw "Please Select year";
            //}
            //if (baseService.isUndefinedOrNull($scope.CustomPara.EncashmentDate)) {
            //    throw "Please Select Encashment Date";
            //}
           
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
                    model.BroughtForward = $scope.LeaveEncashmentList[i].BroughtForward;
                    model.DaysCanBeSanctioned = $scope.LeaveEncashmentList[i].DaysCanBeSanctioned;
                    model.AvailedLeave = $scope.LeaveEncashmentList[i].AvailedLeave;
                    model.CarryForward = $scope.LeaveEncashmentList[i].CarryForward;
                    model.EmployeeCode = $scope.LeaveEncashmentList[i].EmployeeCode;
                    //model.Balance = $scope.LeaveEncashmentList[i].Balance;
                    EmployeeList.push(model);

                }
            }
            var ToDate = $filter('dateFiltering')($scope.custompara.FromDate, 'dd-M-yyyy');

           
            $http({
                method: 'POST',
                url: $scope.saveLvEncashmentUrl,
                data: { 'leaveEncashment': EmployeeList,  'EncashmentDate': ToDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadLeaveEncashmentList();
                  
                    $scope.GetWithInYearLeaveEncashmentData();
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