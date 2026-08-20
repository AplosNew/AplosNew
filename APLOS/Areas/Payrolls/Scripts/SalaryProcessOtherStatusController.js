'use strict';
SalaryProcessOtherStatusController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function SalaryProcessOtherStatusController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Salary Process (Other Status)';
    $scope.Action = 'Save';
    $scope.path = 'payrolls/SalaryProcessOtherStatus/';

    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];//
    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });
    function Check(obj, controlname) {
        try {
            if (obj === undefined || obj === null || obj === '') {
                throw (controlname + ' can not be blank...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.CreateToDate = function () {
        var date = new Date($scope.FromDate_sep);
        var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
        var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        $scope.lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
        $scope.lastDay = $filter('dateFiltering')(new Date(lastDay), 'dd-MM-yyyy');
        $scope.ToDate_sep = $scope.lastDay;
        //alert(lastDay);
        // console.log($scope.lastDay);
    }






    $scope.EmployeeList_sep = [];
    $scope.GetEmployee_sep = function () {
        try {
            $scope.EmployeeList_sep = [];

            Check($scope.Description, "Description");
            Check($scope.FromDate_sep, 'From Date');
            Check($scope.ToDate_sep, 'To Date');

            var parameters = { 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetSeparatedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                //console.log('kk', response);
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    $scope.EmployeeList_sep = response.data.data;
                    $scope.GetEmployee_sep_zero();
                    //$scope.GetEmployee_sep_Approved();
                }
            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF

    $scope.EmployeeList_sep_zero = [];
    $scope.GetEmployee_sep_zero = function () {
        try {
            $scope.EmployeeList_sep_zero = [];

            var parameters = { 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetSeparatedEmpZeroPresentInfo',
                data: parameters
            }).then(function successCallback(response) {

                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    console.log('kk', response);
                    $scope.EmployeeList_sep_zero = response.data.data;
                    //$scope.GetEmployee_sep_zero();
                    $scope.GetEmployee_sep_Approved();
                }

            });//$http
        } catch (ex) {
            ShowResult(ex.Message, 'Information');
        }  //catch           
    };//EOF

    $scope.EmployeeList_sep_Approved = [];
    $scope.GetEmployee_sep_Approved = function () {
        try {
            var parameters = { 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetSeparatedApprovedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    $scope.EmployeeList_sep_Approved = response.data.data;
                }
            });//$http
        } catch (ex) {
            ShowResult(ex.Message, 'Information');
        }  //catch           
    };//EOF


    $scope.onactivedatabound = function (e) {
        if (e.data.IsLocked === 'NO') {
            e.row.css("background-color", "brown");
            e.row.css("color", "white");
        }
    };

    $scope.EmployeeList_mlv = [];
    $scope.GetEmployee_mlv = function () {
        try {
            $scope.EmployeeList_mlv = [];
            //if (angular.isUndefinedOrNull($scope.FromDate_mlv)) {
            //    throw ("Select From Date");
            //}
            //if (angular.isUndefinedOrNull($scope.ToDate_mlv)) {
            //    throw ("Select To Date");
            //}
            Check($scope.Description_mlv, "Description");
            Check($scope.FromDate_mlv, 'From Date');
            Check($scope.ToDate_mlv, 'To Date');

            var parameters = { 'FromDate': $scope.FromDate_mlv, 'ToDate': $scope.ToDate_mlv };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetmlvEmpInfo',
                data: parameters
            }).then(function successCallback(response) {

                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    $scope.EmployeeList_mlv = response.data.data;
                    $scope.GetEmployee_mlv_Approved();
                }

            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF

    $scope.EmployeeList_mlv_Approved = [];
    $scope.GetEmployee_mlv_Approved = function () {
        try {
            $scope.EmployeeList_mlv_Approved = [];

            var parameters = { 'FromDate': $scope.FromDate_mlv, 'ToDate': $scope.ToDate_mlv };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetMLVApprovedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {

                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    $scope.EmployeeList_mlv_Approved = response.data.data;
                }
            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF

    $scope.EmployeeList_tbs = [];
    $scope.GetEmployee_tbs = function () {
        try {
            $scope.EmployeeList_tbs = [];
            if (angular.isUndefinedOrNull($scope.FromDate_tbs)) {
                throw ("Select From Date");
            }
            if (angular.isUndefinedOrNull($scope.ToDate_tbs)) {
                throw ("Select To Date");
            }

            var parameters = { 'FromDate': $scope.FromDate_tbs, 'ToDate': $scope.ToDate_tbs };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GettbsEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                //if (response.data.length > 0) {
                $scope.EmployeeList_tbs = response.data;
                //}
                //else {
                //    ShowResult("No Data Found", 'Information');
                //}
            });//$http
        } catch (ex) {
            ShowResult(ex.Message, 'Information');
        }  //catch           
    };//EOF




    $('.datepicker').datepicker({
        //startDate: '-2m',
        //endDate: '-0d',
        //datesDisabled: $scope.DisabledDates,
        format: 'dd-M-yyyy',
        todayHighlight: true,
        //minDate: 0,
        autoclose: true,
        inline: true,
        changeMonth: true,

        //beforeShowDay: function (date) {
        //    var eventDates = {};
        //    eventDates[new Date('12/04/2014')] = new Date('12/04/2014');
        //    eventDates[new Date('12/06/2014')] = new Date('12/06/2014');
        //    eventDates[new Date('12/20/2014')] = new Date('12/20/2014');

        //    var highlight = eventDates[date];
        //    if (highlight) {
        //        return [true, "event", highlight];
        //    } else {
        //        return [true, '', ''];
        //    }
        //}

    });





    $scope.OTUnConfirmedEmployees = [];
    $scope.UnApprovedEmployees = [];
    $scope.ShiftNotAssignEmployees = [];
    $scope.AttdencenotNotProcEmployees = [];
    $scope.employees = [];
    $scope.customPara = {
        lockDate: null
    };

    $scope.OTUnConfirmedEmployeesCount = null;
    $scope.UnApprovedEmployeesCount = null;
    $scope.ShiftNotAssignEmployeesCount = null;
    $scope.AttdencenotNotProcEmployeesCount = null;
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;







    $scope.messageText = "";

    $scope.SaveUnLockData = function () {



        try {
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                $scope.ShowResultCustom("Select Date...", 'failure');
            }



            $.ajax({
                type: "POST",
                url: $scope.saveUnLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                },
                dataType: "json",
                success: function (data) {
                    $scope.ShowResultCustom($scope.customPara.lockDate + " is Un-Loked...", "success");

                }

            });
        } catch (e) {
            $scope.ShowResultCustom(e.Message, 'failure');
        }



    };





    $scope.employees = [];
    $scope.LockEmpList = [];
    $scope.TobeLockEmpList = [];

    //$scope.customPara = {
    //    lockDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy')
    //};


    $scope.LockEmpListCount = null;

    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;



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
    $scope.actionCompleteSelected7 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridLockEmployeeList").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container
                gridObj.clearFiltering();
                $("#GridLockEmployeeList").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };






    //$scope.customPara = {
    //    lockDate: null
    //};


    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;







    $scope.messageText = "";

    $scope.SaveUnLockData = function () {



        try {
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                $scope.ShowResultCustom("Select Date...", 'failure');
            }



            $.ajax({
                type: "POST",
                url: $scope.saveUnLockUrl,
                data:
                {
                    'lockDate': $scope.customPara.lockDate
                },
                dataType: "json",
                success: function (data) {
                    ShowResult($scope.customPara.lockDate + " is Un-Loked...", "success");

                }

            });
        } catch (e) {
            ShowResult(e.Message, 'failure');
        }



    };















    //#region Employee wise

    $scope.EmployeeLockData = [];
    $scope.EmployeeReLockData = [];
    $scope.getUnLockDateList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Please Enter Date.";
            }


            $http({
                method: "GET",
                dataType: 'JSON',
                //data: { 'lockDate': $scope.customPara.lockDate},
                url: $scope.GetLockEmployeeListUrl + '?lockDate=' + $scope.customPara.lockDate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    $scope.EmployeeReLockData = [];
                    $scope.EmployeeLockData = [];
                    $scope.EmployeeLockData = response.data.LockEmployees;
                    $scope.EmployeeReLockData = response.data.ReLockEmployees;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.getReLockDateList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.customPara.lockDate)) {
                throw "Please Enter Date.";
            }


            $http({
                method: "GET",
                dataType: 'JSON',
                //data: { 'lockDate': $scope.customPara.lockDate},
                url: $scope.GetReLockEmployeeListUrl + '?lockDate=' + $scope.customPara.lockDate

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {
                    $scope.EmployeeReLockData = [];
                    $scope.EmployeeLockData = [];
                    $scope.EmployeeLockData = response.data.LockEmployees;
                    $scope.EmployeeReLockData = response.data.ReLockEmployees;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.SaveEmployeeWiseUnLockData = function () {

        var UnLockEmployeeList = [];
        for (var i = 0; i < $scope.EmployeeLockData.length; i++) {

            if ($scope.EmployeeLockData[i].CheckBoxSelect === true) {
                UnLockEmployeeList.push($scope.EmployeeLockData[i].SystemID);
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'lockDate': $scope.customPara.lockDate, 'UnLockEmployeeList': UnLockEmployeeList },
            url: $scope.saveUnLockEmployeeListUrl

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.Message, 'failure');
            }
            else {
                //$scope.EmployeeLockData = response.data.Employees;

                ShowResult(response.data.Message, "success");

                //var gridObj = $("#GridEmpWise").data("ejGrid");
                //gridObj.refreshContent();
                $scope.EmployeeLockData = [];
                $scope.EmployeeReLockData = [];
                $scope.getUnLockDateList();
                $scope.getReLockDateList();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });


    };
    ///separated

    $scope.Emp_sep_Process = function () {

        try {
            $scope.msg = "";

            Check($scope.Description, "Description");
            Check($scope.FromDate_sep, 'From Date');
            Check($scope.ToDate_sep, 'To Date');

            var eList = [];
            var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0)
                filtered = $scope.EmployeeList_sep;

            eList = GetEmpList(filtered);

            $scope.btnProcess = false;
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep, 'pDescription': $scope.Description, 'eList': eList
                },
                contentType: "application/json charset=utf-8",
                url: $scope.path + '/ProcessSalarySep'

            }).then(function successCallback(response) {
                $scope.btnProcess = true;
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.msg = "Successfully Completed !!!";
                    ShowResult(response.data.Message, "success");
                }
            }, function errorCallback(response) {
                $scope.btnProcess = true;
                ShowResult(response.status.Message, 'failure');
            });//http
        } catch (e) {
            ShowResult(e, "Info");
        }


    };


    $scope._Emp_sep_Process = function () {
        var eList = [];
        var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0)
            filtered = $scope.EmployeeList_sep;

        eList = GetEmpList(filtered);
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep, 'pDescription': $scope.Description, 'eList': eList },
            url: $scope.path + '/ProcessSalarySep'

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, "success");
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });//http
    };
    ///MLV
    $scope.Emp_mlv_Process = function () {

        var eList = [];
        eList = GetEmpList($scope.EmployeeList_mlv);
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'FromDate': $scope.FromDate_mlv, 'ToDate': $scope.ToDate_mlv, 'pDescription': $scope.Description_mlv, 'eList': eList },
            url: $scope.path + '/ProcessSalaryMLV'

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, "success");
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });//http
    };


    // Usage



    $scope.actionCompleteSelected4 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridEmpWise").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };

    $scope.actionCompleteSelected5 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridEmpMLV").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };







    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };
    function CheckBoxSelectAllEmolyeeWise(e) {
        if (e.model.checkState === "check") {
            for (var i = 0; i < $scope.EmployeeList_sep.length; i++) {
                //$scope.EmployeeLockData[i].CheckBoxSelect = false;
                //if ($scope.EmployeeLockData[i].IsLock === false)
                $scope.EmployeeList_sep[i].IsSelectSlrProc = true;
            }
        }
        else {

            for (var i = 0; i < $scope.EmployeeList_sep.length; i++) {
                $scope.EmployeeList_sep[i].IsSelectSlrProc = false;
            }
        }
        var gridObj = $("#GridEmpWise").data("ejGrid");
        gridObj.refreshContent();
    };



    function GetEmpList(eList) {
        var e_separated = [];
        for (var i = 0; i < eList.length; i++) {
            if (eList[i].IsSelectSlrProc === true) {
                e_separated.push(eList[i].EmpSystemID);
            }
        }
        return e_separated;
    };


    $scope.refreshTemplateemployee5 = function (args) {
        $("#headchk5").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise5 });
    };

    function CheckBoxSelectAllEmolyeeWise5(e) {



        if (e.model.checkState === "check") {

            for (var i = 0; i < $scope.EmployeeList_mlv.length; i++) {
                //$scope.EmployeeLockData[i].CheckBoxSelect = false;
                //if ($scope.EmployeeLockData[i].IsLock === false)
                $scope.EmployeeList_mlv[i].IsSelectSlrProc = true;
            }
        }
        else {

            for (var i = 0; i < $scope.EmployeeList_mlv.length; i++) {
                $scope.EmployeeList_mlv[i].IsSelectSlrProc = false;
            }
        }
        var gridObj = $("#GridEmpMLV").data("ejGrid");
        gridObj.refreshContent();
    };
    //#endregion





}

