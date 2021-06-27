'use strict';
OTManagementController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function OTManagementController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'OT Management';
    $scope.path = 'Attendances/OTManagement/';
    $scope.EmployeeForOTConfirmationListUrl = $scope.path + 'GetEmployeeForOTConfirmation';
    $scope.OTConfirmationSaveUrl = $scope.path + 'SaveOTConfirmation';
    $scope.EmployeeConfirmedListUrl = $scope.path + 'GetConfirmedEmployeeDataForGrid';
    $scope.EmployeePostDeviationListUrl = $scope.path + 'GetPostDeviationEmployeeDataForGrid';
    $scope.EmployeeMissPunchListUrl = $scope.path + 'GetMissPunchEmployeeDataForGrid';
    $scope.OTConfirmationEmpWiseSaveUrl = $scope.path + 'SaveEmpWiseOTConfirmation';
    $scope.ShowOTValue = false;
    $scope.employees = [];



    $scope.customPara = {
        procdate: $filter('dateFiltering')(Date.now('dd-M-yyyy')),
        otcons: null,
        MinimumOTMinute: null,
        OTConsiderOn: null,
        OTFractionCalculate: null,
        IsPreallocationBasedOT: false,
        IsPunchBasedOT: false
    };
    $scope.customPara.procdate = $filter('dateFiltering')(Date.now('dd-M-yyyy'));
    $scope.ShowHourMinute = false;
    $scope.ShowDecimal = false;
    $scope.IsPreallocationBasedOT = false;
    $scope.IsPunchBasedOT = false;
    $scope.ShowSaveButton = false;
    $scope.ActiveTab = 'TobeConfirmed';



    $scope.TobeConfirmedCount = 0;
    $scope.ConfirmedCount = 0;
    $scope.ReConfirmedRequiredCount = 0;
    $scope.MissPunchCount = 0;
    $scope.MaternityWithOTCount = 0;

    $scope.TobeConfirmedEmployees = [];
    $scope.ConfirmedEmployees = [];
    $scope.ReConfirmedRequiredEmployees = [];
    $scope.MissPunchCountEmployees = [];
    $scope.MaternityWithOTEmployees = [];

    $scope.NWDayType = null;
    $scope.HDayType = null;
    $scope.WDayType = null;
    $scope.IsOTConfirmationAutoForZeroAuto = null;
    $scope.IsOTConfirmationAfterLock = null;

    if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {

        var myDate = new Date();
        var previousDay = new Date(myDate);
        previousDay.setDate(myDate.getDate() - 0);
        $scope.customPara.procdate = $filter('dateFiltering')(previousDay, 'dd-M-yyyy');
    }
 



    $scope.datewiseEmpDataLoad = function () {
        $scope.TobeConfirmedCount = 0;
        $scope.ConfirmedCount = 0;
        $scope.ReConfirmedRequiredCount = 0;
        $scope.MissPunchCount = 0;


        if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
            var myDate = new Date();
            var previousDay = new Date(myDate);
            previousDay.setDate(myDate.getDate()-0);
            $scope.customPara.procdate = $filter('dateFiltering')(previousDay, 'dd-M-yyyy');
        }
        $scope.LoadEmployeeDataForGrid();
        //$scope.LoadConfirmedEmployeeDataForGrid();
        $scope.LoadPostDeviationEmployeeDataForGrid();
        $scope.LoadMissPunchEmployeeDataForGrid();
        //$scope.LoadConfirmedEmployeeDataForGrid();       
        //$scope.LoadEmployeeDataForGrid();
        $scope.setTab2(2);
        //$scope.LoadConfirmedEmployeeDataForGrid();
        //if ($scope.tab === 1) {
        //    $scope.employees = [];
        //    $scope.LoadEmployeeDataForGrid();
        //}
        //else if ($scope.tab === 2) {
        //    $scope.employees = [];
        //    $scope.LoadConfirmedEmployeeDataForGrid();
        //}

        //else if ($scope.tab === 3) {
        //    $scope.employees = [];
        //    $scope.LoadPostDeviationEmployeeDataForGrid();
        //}
        //else if ($scope.tab === 4) {
        //    $scope.employees = [];
        //    $scope.LoadMissPunchEmployeeDataForGrid();
        //}

    };
    // #region Tab




    $scope.tab = 2;
    $scope.tabh = 1;
    $scope.setTab1 = function (newTab) {
        $scope.tab = newTab;
        $scope.LoadEmployeeDataForGrid();
        $scope.ShowSaveButton = true;
        $scope.ActiveTab = 'TobeConfirmed';

    };
    $scope.isSet1 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab2 = function (newTab) {
        $scope.tab = newTab;
        $scope.LoadConfirmedEmployeeDataForGrid();
        $scope.ShowSaveButton = false;
        $scope.ActiveTab = 'Confirmed';

    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab3 = function (newTab) {
        $scope.tab = newTab;
        $scope.LoadPostDeviationEmployeeDataForGrid();
        $scope.ShowSaveButton = true;
        $scope.ActiveTab = 'ReConfirmedRequired';
    };
    $scope.isSet3 = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.setTab4 = function (newTab) {
        $scope.tab = newTab;
        $scope.LoadMissPunchEmployeeDataForGrid();
        $scope.ShowSaveButton = false;
        $scope.ActiveTab = 'MissPunch';
    };
    $scope.isSet4 = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.setTab5 = function (newTab) {
        $scope.tab = newTab;
        $scope.ShowSaveButton = true;
        $scope.ActiveTab = 'MLV';
    };
    $scope.isSet5 = function (tabNum) {
        return $scope.tab === tabNum;
    };





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
    // #endregion

    // #endregion

    $scope.messageText = "";

    $scope.ShowResultCustom = function (message, type) {
        $("#dialogMessage").ejDialog("setTitle", "Success");
        $scope.messageText = message;
        $scope.messageTitle = "Message";

        if (type === "failure")
            $("#dialogMessage").ejDialog("setTitle", "Error");

        var eDialog = $("#dialogMessage").data("ejDialog");
        eDialog.open();

    };

    $window.onresize = function (event) {

        $scope.actionCompleteSelected();
        $scope.actionCompleteSelected1();
        $scope.actionCompleteSelected2();
        $scope.actionCompleteSelected3();
        $scope.actionCompleteSelected4();
        $scope.actionCompleteSelected6();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#Grid").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container

                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                //args.requestType: "filtering"
                //var filtereddata = gridObj.getFilteredRecords();
                //var scrollerheight = ($("#OuterContainer").height()) - ($(".e-gridheader").outerHeight()) - ($(".e-pager").outerHeight());//Obtain the height of the container and subtract it from gridheader and pager
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            //$scope.ShowResult(e, 'failure');
        }
    };
    $scope.actionCompleteSelected1 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridConfirmed").ejGrid("instance");
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
    $scope.actionCompleteSelected2 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridPostDeviation").ejGrid("instance");
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
    $scope.actionCompleteSelected3 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridMissPunch").ejGrid("instance");
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
    $scope.actionCompleteSelected6 = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridMaternityWithOT").ejGrid("instance");
                var scrollerwidth = $("#tabpaneldiv").width();//Obtain the width of the container                
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResult(e, 'failure');
        }
    };
    $scope.onrowdatabound = function (e) {
        if (e.data.IsDeviation === 1)
            e.row.css("background-color", "orange");
        if (e.data.ExtraOT === 'YES')
            e.row.css("background-color", "red");
    };
    $scope.onrowdataboundExtraOT = function (e) {
        if (e.data.ExtraOT === 'YES')
            e.row.css("background-color", "red");
    };
   





    $scope.LoadEmployeeDataForGrid = function () {
        try {
            //var previousDay = null;
            if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
                var myDate = new Date();
                var previousDay = new Date(myDate);
                previousDay.setDate(myDate.getDate()-0);
                $scope.customPara.procdate = $filter('dateFiltering')(previousDay, 'dd-M-yyyy');
            }
            
            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.EmployeeForOTConfirmationListUrl + "?ProcDate=" + $scope.day + "&OTvalCons=" + $scope.customPara.otcons,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                }
                else {                  

                    $scope.NWDayType = response.data.NWDayType;
                    $scope.HDayType = response.data.HDayType;
                    $scope.WDayType = response.data.WDayType;
                    $scope.IsPreallocationBasedOT = response.data.IsPreallocationBasedOT;
                    $scope.IsPunchBasedOT = response.data.IsPunchBasedOT;
                    if (response.data.OTConsiderOn === 'Hour Minute Value') {
                        $scope.ShowHourMinute = true;
                    }
                    if (response.data.OTConsiderOn === 'Decimal Value') {
                        $scope.ShowDecimal = true;
                    }

                    if (response.data.IsOTConfirmationAutoForZeroAuto === true) {
                        $scope.IsOTConfirmationAutoForZeroAuto = 'Zero OT Confirmation Auto';
                    }

                    $scope.IsOTConfirmationAfterLock = null;
                    if (response.data.IsOTConfirmationAfterLock === true) {
                        $scope.IsOTConfirmationAfterLock = 'OT confirmation after day Lock is enable in plant setting.';
                    }
                   
                    //  IsPreallocationBasedOT: false,
                    //  IsPunchBasedOT: false
                    if (response.data.IsPreallocationBasedOT === true || response.data.IsPunchBasedOT === true) {
                        $scope.TobeConfirmedEmployees = [];
                        $scope.TobeConfirmedEmployees = response.data.data;

                        $scope.TobeConfirmedCount = 0;
                        if ($scope.TobeConfirmedEmployees.length > 0) {
                            $scope.TobeConfirmedCount = $scope.TobeConfirmedEmployees.length;
                        }



                        $scope.ShowOTValue = response.data.ShowOTValue;

                        $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                        $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                        $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                        $scope.customPara.IsPreallocationBasedOT = !response.data.IsPreallocationBasedOT;
                        $scope.customPara.IsPunchBasedOT = !response.data.IsPunchBasedOT;
                        var gridObj = $("#Grid").data("ejGrid");
                        gridObj.clearFiltering();

                        if (response.data.OTConsiderOn === 'Hour Minute Value') {
                            //gridObj.hideColumns("DeviceOTHrHour");
                            //gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("NormalOTHrHour");
                            //gridObj.hideColumns("NormalOTHrMinute");
                            gridObj.hideColumns("NormalOTHrInDecimal");
                        }
                        if (response.data.OTConsiderOn === 'Decimal Value') {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");

                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");

                            //gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("NormalOTHrHour");
                            gridObj.hideColumns("NormalOTHrMinute");
                            //gridObj.hideColumns("NormalOTHrInDecimal");
                        }


                        if (response.data.IsPreallocationBasedOT === true) {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }
                        if (response.data.IsPunchBasedOT === true) {
                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }

                        if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                            if (response.data.OTConsiderOn === 'Hour Minute Value') {


                                gridObj.showColumns("DeviceOTHrHour");
                                gridObj.showColumns("DeviceOTHrMinute");
                                gridObj.showColumns("OTPreallocationHour");
                                gridObj.showColumns("OTPreallocationMinute");

                            }
                            if (response.data.OTConsiderOn === 'Decimal Value') {
                                gridObj.showColumns("OTPreallocationDecimal");
                                gridObj.showColumns("DeviceOTHrInDecimal");




                            }



                            //gridObj.showColumns("EmployeeName");
                            //gridObj.showColumns("EmployeeName");
                        }




                        ///Maternity with OT
                        $scope.MaternityWithOTEmployees = [];
                        $scope.MaternityWithOTEmployees = response.data.EmpMaternityWithOT;
                        if (baseService.isUndefinedOrNull(response.data.EmpMaternityWithOT)) {
                            $scope.MaternityWithOTCount = 0;
                        } else {
                            $scope.MaternityWithOTCount = response.data.EmpMaternityWithOT.length;
                        }
                        var gridObjMaternityWithOT = $("#GridMaternityWithOT").data("ejGrid");
                        gridObjMaternityWithOT.clearFiltering();

                        if (response.data.OTConsiderOn === 'Hour Minute Value') {
                            //gridObj.hideColumns("DeviceOTHrHour");
                            //gridObj.hideColumns("DeviceOTHrMinute");
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrInDecimal");
                            gridObjMaternityWithOT.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("NormalOTHrHour");
                            //gridObj.hideColumns("NormalOTHrMinute");
                            gridObjMaternityWithOT.hideColumns("NormalOTHrInDecimal");
                        }
                        if (response.data.OTConsiderOn === 'Decimal Value') {
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrHour");
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrMinute");

                            gridObjMaternityWithOT.hideColumns("OTPreallocationHour");
                            gridObjMaternityWithOT.hideColumns("OTPreallocationMinute");

                            //gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObjMaternityWithOT.hideColumns("NormalOTHrHour");
                            gridObjMaternityWithOT.hideColumns("NormalOTHrMinute");
                            //gridObj.hideColumns("NormalOTHrInDecimal");
                        }


                        if (response.data.IsPreallocationBasedOT === true) {
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrHour");
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrMinute");
                            gridObjMaternityWithOT.hideColumns("DeviceOTHrInDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }
                        if (response.data.IsPunchBasedOT === true) {
                            gridObjMaternityWithOT.hideColumns("OTPreallocationHour");
                            gridObjMaternityWithOT.hideColumns("OTPreallocationMinute");
                            gridObjMaternityWithOT.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }

                        if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                            if (response.data.OTConsiderOn === 'Hour Minute Value') {


                                gridObjMaternityWithOT.showColumns("DeviceOTHrHour");
                                gridObjMaternityWithOT.showColumns("DeviceOTHrMinute");
                                gridObjMaternityWithOT.showColumns("OTPreallocationHour");
                                gridObjMaternityWithOT.showColumns("OTPreallocationMinute");

                            }
                            if (response.data.OTConsiderOn === 'Decimal Value') {
                                gridObjMaternityWithOT.showColumns("OTPreallocationDecimal");
                                gridObjMaternityWithOT.showColumns("DeviceOTHrInDecimal");

                            }



                            //gridObj.showColumns("EmployeeName");
                            //gridObj.showColumns("EmployeeName");
                        }
                        $scope.ShowSaveButton = true;
                    } else {
                        ShowResult('Select Punch Based OT/Preallocation Based OT on setting.', 'failure');
                    }


                    if (response.data.IsOTConfirmationAfterLock === true) {
                        $scope.TobeConfirmedEmployees = [];
                        $scope.TobeConfirmedCount = 0;

                        $scope.MaternityWithOTEmployees = [];
                        $scope.MaternityWithOTCount = 0;
                        ShowResult('OT confirmation after day Lock is enable in plant setting.', 'failure');
                    }

                }
            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };


           


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadConfirmedEmployeeDataForGrid = function () {
        try {
            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            if (baseService.isUndefinedOrNull($scope.day)) {
                ShowResult('Select date...', 'failure');
            }

            $http({
                method: 'GET',
                url: $scope.EmployeeConfirmedListUrl + "?ProcDate=" + $scope.day + "&OTvalCons=" + $scope.customPara.otcons,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowResult(response.Message, 'failure');
                }
                else {




                    $scope.IsPreallocationBasedOT = response.data.IsPreallocationBasedOT;
                    $scope.IsPunchBasedOT = response.data.IsPunchBasedOT;
                    if (response.data.OTConsiderOn === 'Hour Minute Value') {
                        $scope.ShowHourMinute = true;
                    }
                    if (response.data.OTConsiderOn === 'Decimal Value') {
                        $scope.ShowDecimal = true;
                    }




                   

                    if (response.data.IsPreallocationBasedOT === true || response.data.IsPunchBasedOT === true) {
                        $scope.ConfirmedEmployees = [];
                        $scope.ConfirmedEmployees = response.data.data;
                        $scope.ConfirmedCount = 0;
                        if ($scope.ConfirmedEmployees.length > 0) {
                            $scope.ConfirmedCount = $scope.ConfirmedEmployees.length;
                        }

                        $scope.ShowOTValue = response.data.ShowOTValue;

                        $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                        $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                        $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                        $scope.customPara.IsPreallocationBasedOT = !response.data.IsPreallocationBasedOT;
                        $scope.customPara.IsPunchBasedOT = !response.data.IsPunchBasedOT;
                        var gridObj = $("#GridConfirmed").data("ejGrid");
                        gridObj.clearFiltering();
                        gridObj.hideColumns("CNormalOTHrHour");
                        gridObj.hideColumns("CNormalOTHrMinute");
                        gridObj.hideColumns("CNormalOTHrInDecimal");
                        if (response.data.OTConsiderOn === 'Hour Minute Value') {
                            //gridObj.hideColumns("DeviceOTHrHour");
                            //gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("NormalOTHrHour");
                            //gridObj.hideColumns("NormalOTHrMinute");
                            gridObj.hideColumns("NormalOTHrInDecimal");
                            gridObj.hideColumns("CNormalOTHrInDecimal");
                        }
                        if (response.data.OTConsiderOn === 'Decimal Value') {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");

                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");

                            //gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("NormalOTHrHour");
                            gridObj.hideColumns("NormalOTHrMinute");

                            gridObj.hideColumns("CNormalOTHrHour");
                            gridObj.hideColumns("CNormalOTHrMinute");
                            //gridObj.hideColumns("NormalOTHrInDecimal");
                        }


                        if (response.data.IsPreallocationBasedOT === true) {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }
                        if (response.data.IsPunchBasedOT === true) {
                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }

                        if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                            if (response.data.OTConsiderOn === 'Hour Minute Value') {


                                gridObj.showColumns("DeviceOTHrHour");
                                gridObj.showColumns("DeviceOTHrMinute");
                                gridObj.showColumns("OTPreallocationHour");
                                gridObj.showColumns("OTPreallocationMinute");
                                gridObj.showColumns("CNormalOTHrHour");
                                gridObj.showColumns("CNormalOTHrMinute");


                            }
                            if (response.data.OTConsiderOn === 'Decimal Value') {
                                gridObj.showColumns("OTPreallocationDecimal");
                                gridObj.showColumns("DeviceOTHrInDecimal");
                                ;
                                gridObj.showColumns("CNormalOTHrInDecimal");




                            }



                            //gridObj.showColumns("EmployeeName");
                            //gridObj.showColumns("EmployeeName");
                        }


                        $scope.ShowSaveButton = true;
                    } else {
                        ShowResult('Select Punch Based OT/Preallocation Based OT on setting.', 'failure');
                    }



                }
            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };



        } catch (e) {
           ShowResult(e, "failure");
        }
    };

    $scope.LoadPostDeviationEmployeeDataForGrid = function () {
        try {
            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            if (baseService.isUndefinedOrNull($scope.day)) {
               ShowResult('Select date...', 'failure');
            }

            $http({
                method: 'GET',
                url: $scope.EmployeePostDeviationListUrl + "?ProcDate=" + $scope.day + "&OTvalCons=" + $scope.customPara.otcons,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                   ShowResult(response.Message, 'failure');
                }
                else {


                    $scope.IsPreallocationBasedOT = response.data.IsPreallocationBasedOT;
                    $scope.IsPunchBasedOT = response.data.IsPunchBasedOT;
                    if (response.data.OTConsiderOn === 'Hour Minute Value') {
                        $scope.ShowHourMinute = true;
                    }
                    if (response.data.OTConsiderOn === 'Decimal Value') {
                        $scope.ShowDecimal = true;
                    }

                    //$scope.ShowResult(response.Message, 'success');
                    //$scope.employees = null;
                    //$scope.employees = response.data.data;
                    //$scope.ShowOTValue = response.data.ShowOTValue;
                    //$scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                    //$scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                    //$scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;


                    if (response.data.IsPreallocationBasedOT === true || response.data.IsPunchBasedOT === true) {
                        $scope.ReConfirmedRequiredEmployees = [];
                        $scope.ReConfirmedRequiredEmployees = response.data.data;

                        $scope.ReConfirmedRequiredCount = 0;
                        if ($scope.ReConfirmedRequiredEmployees.length > 0) {
                            $scope.ReConfirmedRequiredCount = $scope.ReConfirmedRequiredEmployees.length;
                        }

                        $scope.ShowOTValue = response.data.ShowOTValue;

                        $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                        $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                        $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                        $scope.customPara.IsPreallocationBasedOT = !response.data.IsPreallocationBasedOT;
                        $scope.customPara.IsPunchBasedOT = !response.data.IsPunchBasedOT;
                        var gridObj = $("#GridPostDeviation").data("ejGrid");
                        gridObj.clearFiltering();
                        if (response.data.OTConsiderOn === 'Hour Minute Value') {
                            //gridObj.hideColumns("DeviceOTHrHour");
                            //gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("NormalOTHrHour");
                            //gridObj.hideColumns("NormalOTHrMinute");
                            gridObj.hideColumns("NormalOTHrInDecimal");
                            gridObj.hideColumns("CNormalOTHrInDecimal");
                        }
                        if (response.data.OTConsiderOn === 'Decimal Value') {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");

                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");

                            //gridObj.hideColumns("DeviceOTHrInDecimal");
                            gridObj.hideColumns("NormalOTHrHour");
                            gridObj.hideColumns("NormalOTHrMinute");

                            gridObj.hideColumns("CNormalOTHrHour");
                            gridObj.hideColumns("CNormalOTHrMinute");
                            //gridObj.hideColumns("NormalOTHrInDecimal");
                        }


                        if (response.data.IsPreallocationBasedOT === true) {
                            gridObj.hideColumns("DeviceOTHrHour");
                            gridObj.hideColumns("DeviceOTHrMinute");
                            gridObj.hideColumns("DeviceOTHrInDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }
                        if (response.data.IsPunchBasedOT === true) {
                            gridObj.hideColumns("OTPreallocationHour");
                            gridObj.hideColumns("OTPreallocationMinute");
                            gridObj.hideColumns("OTPreallocationDecimal");
                            //gridObj.hideColumns("EmployeeName");
                        }

                        if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                            if (response.data.OTConsiderOn === 'Hour Minute Value') {


                                gridObj.showColumns("DeviceOTHrHour");
                                gridObj.showColumns("DeviceOTHrMinute");
                                gridObj.showColumns("OTPreallocationHour");
                                gridObj.showColumns("OTPreallocationMinute");

                            }
                            if (response.data.OTConsiderOn === 'Decimal Value') {
                                gridObj.showColumns("OTPreallocationDecimal");
                                gridObj.showColumns("DeviceOTHrInDecimal");




                            }



                            //gridObj.showColumns("EmployeeName");
                            //gridObj.showColumns("EmployeeName");
                        }


                        $scope.ShowSaveButton = true;
                    } else {
                        ShowResult('Select Punch Based OT/Preallocation Based OT on setting.', 'failure');
                    }
                }
            }), function errorCallBack(response) {
               ShowResult(response.Message, 'failure');
            };



        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.LoadMissPunchEmployeeDataForGrid = function () {
        try {
            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            if (baseService.isUndefinedOrNull($scope.day)) {
                ShowResult('Select date...', 'failure');
            }

            $http({
                method: 'GET',
                url: $scope.EmployeeMissPunchListUrl + "?ProcDate=" + $scope.day + "&OTvalCons=" + $scope.customPara.otcons,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                   ShowResult(response.Message, 'failure');
                }
                else {



                    //$scope.ShowResult(response.Message, 'success');
                    $scope.MissPunchCountEmployees = [];
                    $scope.MissPunchCountEmployees = response.data.data;
                    $scope.MissPunchCount = 0;
                    if ($scope.MissPunchCountEmployees.length > 0) {
                        $scope.MissPunchCount = $scope.MissPunchCountEmployees.length;
                    }
                    $scope.ShowOTValue = response.data.ShowOTValue;
                    $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                    $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                    $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                    var gridObj = $("#GridMissPunch").data("ejGrid");
                    gridObj.clearFiltering();
                }
            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };



        } catch (e) {
           ShowResult(e, "failure");
        }
    };

    

   

    $scope.SaveData = function () {

        try {

            $scope.employees = [];

            if ($scope.tab === 1) {
                $scope.employees = [];
                for (var i = 0; i < $scope.TobeConfirmedEmployees.length; i++) {

                    if ($scope.TobeConfirmedEmployees[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.TobeConfirmedEmployees[i]);
                    }

                }
            }
            else if ($scope.tab === 2) {
                $scope.employees = [];
                for (var i = 0; i < $scope.ConfirmedEmployees.length; i++) {

                    if ($scope.ConfirmedEmployees[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.ConfirmedEmployees[i]);
                    }

                }
            }

            else if ($scope.tab === 3) {
                $scope.employees = [];
                for (var i = 0; i < $scope.ReConfirmedRequiredEmployees.length; i++) {

                    if ($scope.ReConfirmedRequiredEmployees[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.ReConfirmedRequiredEmployees[i]);
                    }

                }
            }
            else if ($scope.tab === 5) {
                $scope.employees = [];
                for (var i = 0; i < $scope.MaternityWithOTEmployees.length; i++) {

                    if ($scope.MaternityWithOTEmployees[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.MaternityWithOTEmployees[i]);
                    }

                }
            }



            if ($scope.employees.length == 0) {
                throw "Please select data";
            }
            $.ajax({
                type: "POST",
                url: $scope.OTConfirmationSaveUrl,
                data:
                {
                    'employeeOTInformation': $scope.employees,
                    'ProcDate': $scope.customPara.procdate
                },
                dataType: "json",
                success: function (response) {
                    //$scope.ShowResult(data.Message, "success");
                    
                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.datewiseEmpDataLoad();
                    }

                }

            });
        } catch (e) {
            ShowResult(e, "failure");
        }

    };




    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    };
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.TobeConfirmedEmployees, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.TobeConfirmedEmployees.length; i++) {
                    $scope.TobeConfirmedEmployees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.TobeConfirmedEmployees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.TobeConfirmedEmployees[i].SystemID === filtered[j].SystemID)
                            $scope.TobeConfirmedEmployees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.TobeConfirmedEmployees.length; i++) {
                    $scope.TobeConfirmedEmployees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.TobeConfirmedEmployees[i].SystemID == filtered[j].SystemID)
                            $scope.TobeConfirmedEmployees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.TobeConfirmedEmployees, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }




    function checkChangeemployee1(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.ReConfirmedRequiredEmployees, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee1(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridPostDeviation").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.ReConfirmedRequiredEmployees.length; i++) {
                    $scope.ReConfirmedRequiredEmployees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.ReConfirmedRequiredEmployees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.ReConfirmedRequiredEmployees[i].SystemID === filtered[j].SystemID)
                            $scope.ReConfirmedRequiredEmployees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridPostDeviation .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee1 });
            }
        }
        else {
            var filtered = $("#GridPostDeviation").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.ReConfirmedRequiredEmployees.length; i++) {
                    $scope.ReConfirmedRequiredEmployees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.ReConfirmedRequiredEmployees[i].SystemID == filtered[j].SystemID)
                            $scope.ReConfirmedRequiredEmployees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridPostDeviation .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridPostDeviation .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee1 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee1 = function (args) {
        $("#GridPostDeviation .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk1").ejCheckBox({ "change": headCheckChangeemployee1 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee1 = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk1").ejCheckBox({ "change": headCheckChangeemployee1 });
        }

        var valobj = $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.ReConfirmedRequiredEmployees, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridPostDeviation .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee1 });
    }





    //////2
    function checkChangeemployee2(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.ConfirmedEmployees, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee2(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridConfirmed").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.ConfirmedEmployees.length; i++) {
                    $scope.ConfirmedEmployees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.ConfirmedEmployees[i].SystemID === filtered[j].SystemID)
                            $scope.ConfirmedEmployees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridConfirmed .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee2 });
            }
        }
        else {
            var filtered = $("#GridConfirmed").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.ConfirmedEmployees.length; i++) {
                    $scope.ConfirmedEmployees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.ConfirmedEmployees[i].SystemID == filtered[j].SystemID)
                            $scope.ConfirmedEmployees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridConfirmed .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridConfirmed .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee2 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee2 = function (args) {
        $("#GridConfirmed .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk2").ejCheckBox({ "change": headCheckChangeemployee2 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee2 = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk2").ejCheckBox({ "change": headCheckChangeemployee2 });
        }

        var valobj = $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.ConfirmedEmployees, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridConfirmed .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee2 });
    };



    /////////mvl

    function checkChangeemployee6(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.MaternityWithOTEmployees, { 'SystemID': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee6(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridMaternityWithOT").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.MaternityWithOTEmployees.length; i++) {
                    $scope.MaternityWithOTEmployees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.MaternityWithOTEmployees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MaternityWithOTEmployees[i].SystemID === filtered[j].SystemID)
                            $scope.MaternityWithOTEmployees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridMaternityWithOT .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee6 });
            }
        }
        else {
            var filtered = $("#GridMaternityWithOT").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MaternityWithOTEmployees.length; i++) {
                    $scope.MaternityWithOTEmployees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MaternityWithOTEmployees[i].SystemID == filtered[j].SystemID)
                            $scope.MaternityWithOTEmployees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridMaternityWithOT .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridMaternityWithOT .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee6 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee6 = function (args) {
        $("#GridMaternityWithOT .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk6").ejCheckBox({ "change": headCheckChangeemployee6 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee6 = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk6").ejCheckBox({ "change": headCheckChangeemployee6 });
        }

        var valobj = $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.MaternityWithOTEmployees, { 'SystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridMaternityWithOT .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee6 });
    };

    // #endregion


   // maternity for ot 
    $scope.commandMaternityOT = [{
        type: "details", buttonOptions: {
            text: "Details",
            width: "70",
            height: "20",

            click: onClickMaternityOT
        }
    }];

    $scope.ApprovalTitle = "";
    $scope.MaternityDetails = {};
    function onClickMaternityOT(arg) {
        //$scope.ApprovalTitle = "Employee Salary Structure Change Approval";
       

        var gridObj = $("#GridMaternityWithOT").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];


        //angular.copy(data, $scope.budgetCodeChangeOld);

        //$scope.budgetCodeChangeNew.Code = $scope.budgetCodeChange.Code;
        //$scope.budgetCodeChangeNew.GivenDesignationId = $scope.budgetCodeChange.GivenDesignationId;
        //$scope.imageSrc = virtualPath.EmployeePic + $scope.budgetCodeChangeOld.EmpPicPath;
        //$scope.Action = 'Update';
        //$scope.getEmpSalaryInfoDefineData(data.SystemId);
        $http({
            method: 'GET',
            url: $scope.path + "GetMaternityDetailsForOTConfirmation?EmpId=" + data.SystemID+"&WDate='"+$scope.customPara.procdate+"'" ,
            //data: JSON.stringify(data),
            headers: {
                'Content-Type': 'application/json'
            }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.Message, 'failure');
            }
            else {
                $scope.MaternityDetails = response.data.data[0];
                $scope.imageSrc = virtualPath.EmployeePic + $scope.MaternityDetails.EmpPicPath;

                var eDialog = $("#dialogMaternityDetails").data("ejDialog");
                eDialog.open();

                ////$scope.ShowResult(response.Message, 'success');
                //$scope.MissPunchCountEmployees = [];
                //$scope.MissPunchCountEmployees = response.data.data;
                //$scope.MissPunchCount = 0;
                //if ($scope.MissPunchCountEmployees.length > 0) {
                //    $scope.MissPunchCount = $scope.MissPunchCountEmployees.length;
                //}
                //$scope.ShowOTValue = response.data.ShowOTValue;
                //$scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                //$scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                //$scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                //var gridObj = $("#GridMissPunch").data("ejGrid");
                //gridObj.clearFiltering();
            }
        }), function errorCallBack(response) {
            ShowResult(response.Message, 'failure');
        };


    }

    ////

    $scope.pathemp = 'HumanResource/attendanceProcessData/'; // For Employee load Emp wise ot confirm...

    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }

    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllEmployee = function () {

        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
       

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate },
            url: $scope.pathemp + 'getAllEmployees'

        }).then(function successCallback(response) {
           
            if (response.data.Error === true) {
                ShowResult(response.Message, 'failure');
            }
            else {
                eDialog.open();
                $scope.selectemployee = response.data;
            }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
    };
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) == false)
            $scope.selectedSinglemployee = args.data;

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'EmpId': $scope.selectedSinglemployee.Id, 'FDate': $scope.FromDate, 'TDate': $scope.ToDate },
            url: $scope.path + 'GetEmployeeWiseDataForOTConfirmation'

        }).then(function successCallback(response) {
            //$scope.employees = [];
            //$scope.employees = response.data.data;



            $scope.IsPreallocationBasedOT = response.data.IsPreallocationBasedOT;
            $scope.IsPunchBasedOT = response.data.IsPunchBasedOT;
            if (response.data.OTConsiderOn === 'Hour Minute Value') {
                $scope.ShowHourMinute = true;
            }
            if (response.data.OTConsiderOn === 'Decimal Value') {
                $scope.ShowDecimal = true;
            }

            if (response.data.IsOTConfirmationAutoForZeroAuto === true) {
                $scope.IsOTConfirmationAutoForZeroAuto = 'Zero OT Confirmation Auto';
            }

            $scope.IsOTConfirmationAfterLock = null;
            if (response.data.IsOTConfirmationAfterLock === true) {
                $scope.IsOTConfirmationAfterLock = 'OT confirmation after day Lock is enable in plant setting.';
            }




            //  IsPreallocationBasedOT: false,
            //  IsPunchBasedOT: false
            if (response.data.IsPreallocationBasedOT === true || response.data.IsPunchBasedOT === true) {
                $scope.employees = [];
                $scope.employees = response.data.data;

                $scope.TobeConfirmedCount = 0;
                if ($scope.employees.length > 0) {
                    $scope.TobeConfirmedCount = $scope.employees.length;
                }



                $scope.ShowOTValue = response.data.ShowOTValue;

                $scope.customPara.MinimumOTMinute = response.data.MinimumOTMinute;
                $scope.customPara.OTConsiderOn = response.data.OTConsiderOn;
                $scope.customPara.OTFractionCalculate = response.data.OTFractionCalculate;
                $scope.customPara.IsPreallocationBasedOT = !response.data.IsPreallocationBasedOT;
                $scope.customPara.IsPunchBasedOT = !response.data.IsPunchBasedOT;
                var gridObj = $("#GridEmpWise").data("ejGrid");
                gridObj.clearFiltering();

                if (response.data.OTConsiderOn === 'Hour Minute Value') {
                    //gridObj.hideColumns("DeviceOTHrHour");
                    //gridObj.hideColumns("DeviceOTHrMinute");
                    gridObj.hideColumns("DeviceOTHrInDecimal");
                    gridObj.hideColumns("OTPreallocationDecimal");
                    //gridObj.hideColumns("NormalOTHrHour");
                    //gridObj.hideColumns("NormalOTHrMinute");
                    gridObj.hideColumns("NormalOTHrInDecimal");
                }
                if (response.data.OTConsiderOn === 'Decimal Value') {
                    gridObj.hideColumns("DeviceOTHrHour");
                    gridObj.hideColumns("DeviceOTHrMinute");

                    gridObj.hideColumns("OTPreallocationHour");
                    gridObj.hideColumns("OTPreallocationMinute");

                    //gridObj.hideColumns("DeviceOTHrInDecimal");
                    gridObj.hideColumns("NormalOTHrHour");
                    gridObj.hideColumns("NormalOTHrMinute");
                    //gridObj.hideColumns("NormalOTHrInDecimal");
                }


                if (response.data.IsPreallocationBasedOT === true) {
                    gridObj.hideColumns("DeviceOTHrHour");
                    gridObj.hideColumns("DeviceOTHrMinute");
                    gridObj.hideColumns("DeviceOTHrInDecimal");
                    //gridObj.hideColumns("EmployeeName");
                }
                if (response.data.IsPunchBasedOT === true) {
                    gridObj.hideColumns("OTPreallocationHour");
                    gridObj.hideColumns("OTPreallocationMinute");
                    gridObj.hideColumns("OTPreallocationDecimal");
                    //gridObj.hideColumns("EmployeeName");
                }

                if (response.data.IsPreallocationBasedOT === true && response.data.IsPunchBasedOT === true) {

                    if (response.data.OTConsiderOn === 'Hour Minute Value') {


                        gridObj.showColumns("DeviceOTHrHour");
                        gridObj.showColumns("DeviceOTHrMinute");
                        gridObj.showColumns("OTPreallocationHour");
                        gridObj.showColumns("OTPreallocationMinute");

                    }
                    if (response.data.OTConsiderOn === 'Decimal Value') {
                        gridObj.showColumns("OTPreallocationDecimal");
                        gridObj.showColumns("DeviceOTHrInDecimal");




                    }


                }



            }


            //var gridObj = $("#GridChangeAttendance").data("ejGrid");
            //gridObj.refreshContent();


            if (response.data.IsOTConfirmationAfterLock === true) {
                $scope.employees = [];
               
                //$scope.MaternityWithOTCount = 0;
                ShowResult('OT confirmation after day Lock is enable in plant setting.', 'failure');
            }
        });


    }

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
    function checkChangeemployee4(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employees, { 'WorkDate': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState === "check")
                row[0].CheckBoxSelect = true;
            else
                row[0].CheckBoxSelect = false;
        }

    }
    function headCheckChangeemployee4(e) {
        if (e.model.checkState === "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length === 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employees.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].WorkDate === filtered[j].WorkDate)
                            $scope.employees[i].CheckBoxSelect = true;
                    }

                }
            }

            var checkbox = $("#GridEmpWise .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee4 });
            }
        }
        else {
            var filtered = $("#GridEmpWise").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employees.length; i++) {
                    $scope.employees[i].CheckBoxSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employees[i].WorkDate == filtered[j].WorkDate)
                            $scope.employees[i].CheckBoxSelect = false;
                    }

                }
            }
            var checkbox = $("#GridEmpWise .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridEmpWise .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee4 });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee4 = function (args) {
        $("#GridEmpWise .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        //$("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
        //$("#EntityFilterGrid").children('.e-gridcontent').hide();
        //$("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
    }
    $scope.refreshTemplateemployee4 = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk4").ejCheckBox({ "change": headCheckChangeemployee4 });
        }

        var valobj = $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employees, { 'WorkDate': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].CheckBoxSelect == true)
                $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridEmpWise .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee4 });
    }



    $scope.SaveDataEmpWise = function () {
        try {
            var otcd = [];
            var EmpSysIDForExtraOT = "";
            for (var i = 0; i < $scope.employees.length; i++) {

                if ($scope.employees[i].CheckBoxSelect === true) {
                    //otcd.push($scope.employees[i].WorkDate);
                    otcd.push($scope.employees[i]);
                    if ($scope.employees[i].ExtraOT == "YES") {
                        if (EmpSysIDForExtraOT == "") {
                            EmpSysIDForExtraOT = "'" + $scope.employees[i].EmployeeCode + "'";
                        }
                        else {
                            EmpSysIDForExtraOT = EmpSysIDForExtraOT + ", '" + $scope.employees[i].EmployeeCode  + "'";
                        }
                    }//
                }

            }

            if (otcd.length == 0) {
                throw "Please select data";
            }
            if (!baseService.isUndefinedOrNull(EmpSysIDForExtraOT)) {
                throw "This employee exceeding the OT limit [" + EmpSysIDForExtraOT + "]";
            }
            for (var i = 0; i < otcd.length; i++) {

               
                        ///save
                        //$http({
                        //    method: 'POST',
                        //    url: $scope.OTConfirmationEmpWiseSaveUrl,
                        //    data:
                        //    {
                        //        'employeeOTInformation': $scope.employees[j],
                        //        'ProcDate': $scope.customPara.procdate
                        //    },
                        //    dataType: 'JSON'
                        //}).then(function successCallback(response) {
                        //    if (response.data.Error === true) {
                        //        throw response.data.Message ;
                        //    }
                        //    //else {
                        //    //    ShowResult(response.data.Message, 'success');
                               
                        //    //}
                        //}), function errorCallBack(response) {
                        //    ShowResult(response.data.Message, 'failure');
                        //};








                        
                            $.ajax({
                                type: "POST",
                                url: $scope.OTConfirmationEmpWiseSaveUrl,
                                data:
                                {
                                    'employeeOTInformation': otcd[i],
                                    'ProcDate': otcd[i].WorkDate
                                },
                                dataType: "json",
                                success: function (data) {
                                    
                                        if (data.Error === true) {

                                            //throw data.Message;
                                            ShowResult(data.Message, 'failure');
                                            return;

                                        }
                                        else {
                                            //ShowResult(data.Message, 'success');

                                        }
                                    
                                },
                                error: function (data) {
                                    throw data.Message;
                                }
                            });
                        
                        ///
                 

            }


            $scope.selectSignleEmployee();





        } catch (e) {
            ShowResult(e, 'failure');
        }





    };

}