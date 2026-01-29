'use strict';
EmployeeSalaryProcessController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeSalaryProcessController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = ' Salary Process ';
    $scope.Action = 'Save';
    $scope.path = 'payrolls/SalaryProcessNew/';//
    $scope.cbxMLVR = true;
    $scope.cbxZero = true;
    $scope.btnProcess = false;

    $scope.GetCompanyCboList = function () {
        try {

            $http({
                method: 'Get',
                url: 'OrderManagements/masterorder/GetCompanyCboList'
            }).then(function successCallback(response) {
                $scope.companyList = response.data;
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.CompanyId = null;
    $scope.PlantId = null;
    $scope.GetCompanyCboList();
    $scope.plantList = [];
    $scope.getPlantCbo = function () {
        cboService.getCboPlantByCompany($scope.CompanyId, function (response) {
            $scope.plantList = response;
        });
    };
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
    ];
    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });

    $scope.AllDataset = {
        dtActive: [],
        dtNewlyJoined: [],
        dtSND: [],
        dtSNA: [],
        dtEXemp: [],
        dtAttNotProcessed: [],
        dtPresetZero: [],
        dtApprovedSalary: [],
        dtMaternityReturn: [],
        dtSeparated: [],
        dtDifferentStatus: []
    };

    $scope.msg = '';
    $scope.Description = '';
    $scope.EmployeeList_active = [];
    $scope.EmployeeList_separated = [];
    $scope.EmployeeList_newlyjoined = [];
    $scope.EmployeeList_mlvreturn = [];
    $scope.EmployeeList_ssnd = [];
    $scope.EmployeeList_ssna = [];
    $scope.EmployeeList_excepEmp = [];
    $scope.EmployeeList_attNotprocessed = [];
    $scope.EmployeeList_presentZero = [];
    $scope.EmployeeList_diffStatus = [];
    $scope.EmployeeList_approvedSalary = [];
    function Check(obj, controlname) {
        try {
            if (obj === undefined || obj === null || obj === '') {
                throw (controlname + ' can not be blank...');
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.onactivedatabound = function (e) {
        if (e.data.IsLocked === 'NO') {
            e.row.css("background-color", "brown");
            e.row.css("color", "white");
        }
    };

    $scope.btnshow = function () {
        return $scope.btnProcess;
    };

    $scope.GetEmployee_list = function () {
        try {
            $scope.msg = "";
            $scope.EmployeeList_sep = [];
            Check($scope.Description, "Description");
            Check($scope.FromDate_sep, 'From Date');
            Check($scope.ToDate_sep, 'To Date');
           
            var parameters = { 'Description': $scope.Description, 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep, 'plantId': $scope.PlantId};
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessNew/GetEmpList',
                data: parameters
            }).then(function successCallback(response) {

                if (response.data.Error === true) {

                    ShowResult(response.data.Message, 'Information');
                }
                else {
                    $scope.btnProcess = true;
                    $scope.EmployeeList_active = response.data.Active;
                    $scope.EmployeeList_newlyjoined = response.data.NewlyJoined;
                    $scope.EmployeeList_presentZero = response.data.PresetZero;
                    $scope.EmployeeList_mlvreturn = response.data.MaternityReturn;

                    $scope.EmployeeList_separated = response.data.Separated;
                    $scope.EmployeeList_ssnd = response.data.SND;
                    $scope.EmployeeList_ssna = response.data.SNA;
                    $scope.EmployeeList_excepEmp = response.data.ExcepEmp;
                    $scope.EmployeeList_attNotprocessed = response.data.AttNotProcessed;
                    $scope.EmployeeList_diffStatus = response.data.DifferentStatus;
                    $scope.EmployeeList_approvedSalary = response.data.ApprovedSalary;
                }


            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF
    $scope.cbxActiveCol = false;
    $scope.EnaDisActive = function () {
        //GridEmpWise
        if ($scope.cbxActive || $scope.cbxNewlyJoined) {
            $scope.cbxMLVR = false;
            $scope.cbxZero = false;
            //$scope.cbxPresentDaysZero = false;
            //$scope.cbxMaternityReturn = false;
        }
        else {
            $scope.cbxMLVR = true;
            $scope.cbxZero = true;
            $scope.cbxPresentDaysZero = false;
            $scope.cbxMaternityReturn = false;
        }
    }
    $scope.EmployeeList_sep_Approved = [];
    $scope.GetEmployee_sep_Approved = function () {
        try {
            $scope.EmployeeList_sep_Approved = [];
            if (angular.isUndefinedOrNull($scope.FromDate_sep)) {
                throw ("Select From Date");
            }
            if (angular.isUndefinedOrNull($scope.ToDate_sep)) {
                throw ("Select To Date");
            }

            var parameters = { 'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetSeparatedApprovedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                //    console.log('kk', response);
                //if (response.data.length > 0) {
                $scope.EmployeeList_sep_Approved = response.data;
                //console.log('kkk', $scope.EmployeeList_sep_Approved);
                //}
                //else {
                //    ShowResult("No Data Found", 'Information');
                //}
            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF



    $scope.NegativeSalaryHeadList = [];
    $scope.cbxNegativeSalaryHead = false;
    $scope.NegativeSalaryHeadId = null;
    $scope.GetNegativeSalaryHeadList = function () {
        try {
            $scope.NegativeSalaryHeadList = [];
            $http.get($scope.path + 'GetNegativeSalaryHead')
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.NegativeSalaryHeadList = response.data;

                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetNegativeSalaryHeadList();



    $scope.EmployeeList_mlv = [];
    $scope.GetEmployee_mlv = function () {
        try {
            $scope.EmployeeList_mlv = [];
            if (angular.isUndefinedOrNull($scope.FromDate_mlv)) {
                throw ("Select From Date");
            }
            if (angular.isUndefinedOrNull($scope.ToDate_mlv)) {
                throw ("Select To Date");
            }

            var parameters = { 'FromDate': $scope.FromDate_mlv, 'ToDate': $scope.ToDate_mlv };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetmlvEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                //if (response.data.length > 0) {
                $scope.EmployeeList_mlv = response.data;
                //}
                //else {
                //    ShowResult("No Data Found", 'Information');
                //}
            });//$http
        } catch (ex) {
            ShowResult(ex, 'Information');
        }  //catch           
    };//EOF

    $scope.EmployeeList_mlv_Approved = [];
    $scope.GetEmployee_mlv_Approved = function () {
        try {
            $scope.EmployeeList_mlv_Approved = [];
            if (angular.isUndefinedOrNull($scope.FromDate_mlv)) {
                throw ("Select From Date");
            }
            if (angular.isUndefinedOrNull($scope.ToDate_mlv)) {
                throw ("Select To Date");
            }

            var parameters = { 'FromDate': $scope.FromDate_mlv, 'ToDate': $scope.ToDate_mlv };
            $http({
                method: "POST",
                dataType: 'JSON',
                url: 'payrolls/SalaryProcessOtherStatus/GetMLVApprovedEmpInfo',
                data: parameters
            }).then(function successCallback(response) {
                //if (response.data.length > 0) {
                $scope.EmployeeList_mlv_Approved = response.data;
                //}
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
            ShowResult(ex, 'Information');
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
    });

    //=============================================Active================================
    $scope.ActiveEmpcbx = function (args) {
        $("#cbxheadActive").ejCheckBox({ "change": ActiveEmps });
    };

    function ActiveEmps(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmpActive").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList_active.length; i++) {
                $scope.EmployeeList_active[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmpActive").data("ejGrid");
        gridObj.refreshContent();
    };
    //=============================================Newly Joined================================
    $scope.NewlyJoinedEmpcbx = function (args) {
        $("#cbxheadNewlyJoined").ejCheckBox({ "change": NewlyJoinedEmps });
    };

    function NewlyJoinedEmps(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridNewlyJoined").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList_newlyjoined.length; i++) {
                $scope.EmployeeList_newlyjoined[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridNewlyJoined").data("ejGrid");
        gridObj.refreshContent();
    };
    //==========================================================Newly Joined=====================================================
    //=============================================present days zero==============================
    $scope.PDZeroEmpcbx = function (args) {
        $("#cbxheadPDZero").ejCheckBox({ "change": PDZeroEmps });
    };

    function PDZeroEmps(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmpPDZero").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList_presentZero.length; i++) {
                $scope.EmployeeList_presentZero[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmpPDZero").data("ejGrid");
        gridObj.refreshContent();
    };
    //==========================================================present days zero=====================================================
    //=============================================mlv Return==============================
    $scope.MLVReturnEmpcbx = function (args) {
        $("#cbxheadMLVReturn").ejCheckBox({ "change": MLVReturnEmps });
    };

    function MLVReturnEmps(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmpMLVReturn").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList_mlvreturn.length; i++) {
                $scope.EmployeeList_mlvreturn[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmpMLVReturn").data("ejGrid");
        gridObj.refreshContent();
    };
    //==========================================================mlv Return=====================================================

    //#region  OtherStatus    

    $scope.OtherStatusEmpcbx = function (args) {
        $("#cbxheadOtherStatus").ejCheckBox({ "change": OtherStatusEmps });
    };

    function OtherStatusEmps(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDifferentStatus").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList_diffStatus.length; i++) {
                $scope.EmployeeList_diffStatus[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDifferentStatus").data("ejGrid");
        gridObj.refreshContent();
    };

    //#endregion


    $scope.employees = [];
    $scope.LockEmpList = [];
    $scope.TobeLockEmpList = [];

    $scope.LockEmpListCount = null;

    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;

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
    $scope.LastLockDate = null;
    $scope.DatePickerEnable = true;
    $scope.LoadButtonShow = false;
    $scope.LockButtonShow = false;
    $scope.messageText = "";
    function GetShortColumns(plist) {
        var list = [];
        if (plist != null) {
            for (var i = 0; i < plist.length; i++) {
                var obj = {
                    EmpSystemID: null,
                    IsSelectSlrProc: null
                };
                obj.EmpSystemID = plist[i].EmpSystemID
                obj.IsSelectSlrProc = plist[i].IsSelectSlrProc
                list.push(obj);
            }
        }//null
        return list;
    }
    function GetSelectedCount(plist) {
        var Count = 0;
        if (plist != null) {
            for (var i = 0; i < plist.length; i++) {
                if (plist[i].IsSelectSlrProc === true) {
                    Count++;
                }
            }
        }//null
        return Count;
    }
    $scope.xCreateToDate = function () {
        try {
            //var td = $scope.ToDate_sep;new Date()
            var today = new Date();
            var frmd = new Date($scope.FromDate_sep);
            var fd = new Date($scope.FromDate_sep);
            var toDate = new Date(fd.setYear(fd.getFullYear(), fd.setMonth(fd.getMonth() + 1), fd.setDate(1)));
            var tdate = new Date(toDate);
            var t_d = new Date(tdate.setDate(tdate.getDate() - 1));

            if (frmd.getMonth() === today.getMonth() && frmd.getFullYear() === today.getFullYear()) {
                $scope.ToDate_sep = $filter('dateFiltering')(today, 'dd-MM-yyyy');
            }
            else {
                $scope.ToDate_sep = $filter('dateFiltering')(t_d, 'dd-MM-yyyy');
            }
        } catch (e) {
            ShowResult(e, "Info");
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

    function SetAllList() {
        try {
            $scope.AllDataset = [];
            if ($scope.EmployeeList_active.length > 0) {
                for (var i = 0; i < $scope.EmployeeList_active.length; i++) {
                    if ($scope.EmployeeList_active[i].IsSelectSlrProc == true) {
                        $scope.AllDataset.push($scope.EmployeeList_active[i])
                    }
                }
            }
            if ($scope.EmployeeList_newlyjoined.length > 0) {
                if ($scope.EmployeeList_newlyjoined.length > 0) {
                    for (var n = 0; n < $scope.EmployeeList_newlyjoined.length; n++) {
                        if ($scope.EmployeeList_newlyjoined[n].IsSelectSlrProc == true) {
                            $scope.AllDataset.push($scope.EmployeeList_active[n])
                        }
                    }
                }
            }
            if ($scope.EmployeeList_diffStatus.length > 0) {
                if ($scope.EmployeeList_diffStatus.length > 0) {
                    for (var d = 0; d < $scope.EmployeeList_diffStatus.length; d++) {
                        if ($scope.EmployeeList_diffStatus[d].IsSelectSlrProc == true) {
                            $scope.AllDataset.push($scope.EmployeeList_diffStatus[d])
                        }
                    }
                }
            }
            

            //var active_count_selected = GetSelectedCount($scope.AllDataset.dtActive);
            //var NewlyJoined_count_selected = GetSelectedCount($scope.AllDataset.dtNewlyJoined);

            //if ($scope.cbxActive) {
            //    if (active_count_selected === null || active_count_selected === 0) {
            //        throw ("No Active Employee is selected");
            //    }
            //}

            //if ($scope.cbxNewlyJoined) {
            //    if (NewlyJoined_count_selected === null || NewlyJoined_count_selected === 0) {
            //        throw ("No Newly Joined Employee is selected");
            //    }
            //}

            //if ($scope.cbxPresentDaysZero) {
            //    if ($scope.AllDataset.dtPresetZero === null || $scope.AllDataset.dtPresetZero.length === 0) {
            //        throw ("No Employee is selected in present days zero tab");
            //    }
            //}

            //if ($scope.cbxMaternityReturn) {
            //    if ($scope.AllDataset.dtMaternityReturn === null || $scope.AllDataset.dtMaternityReturn.length === 0) {
            //        throw ("No Employee is selected in Maternity Return tab");
            //    }
            //}
            //if ($scope.cbxNegativeSalaryHead) {
            //    if ($scope.NegativeSalaryHeadId === null) {
            //        throw ("Select Salary Head");
            //    }
            //}



        } catch (e) {
            throw e;
        }
    }

    //var j = new JsonResult()
    //{
    //    ContentEncoding = Encoding.Default,
    //        ContentType = "application/json",
    //        Data = alldataset,
    //        //JsonRequestBehavior = requestBehavior,
    //        MaxJsonLength = int.MaxValue
    //};

    ///Emp_All_Process
    $scope.Emp_All_Process = function () {

        try {
            $scope.msg = "";

            Check($scope.Description, "Description");
            Check($scope.FromDate_sep, 'From Date');
            Check($scope.ToDate_sep, 'To Date');
            $scope.dataobj = {
                FromDate: null, ToDate: null, Description:null,SystemID:null
            }
            $scope.dataobj.FromDate = $scope.FromDate_sep;
            $scope.dataobj.ToDate = $scope.ToDate_sep;
            $scope.dataobj.Description = $scope.Description;
            //'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep, 'pDescription': $scope.Description, 'alldataset': $scope.AllDataset
            SetAllList();
            $scope.btnProcess = false;
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'data': $scope.dataobj, 'alldataset': $scope.AllDataset
                },
                contentType: "application/json charset=utf-8",
                url: 'Payrolls/EmployeeSalaryRuleSetup/Process'

            }).then(function successCallback(response) {
                $scope.btnProcess = true;
                if (response.data.Error === true) {
                    $scope.AllDataset = [];
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
    $scope.xEmp_All_Process = function () {
        if (angular.isUndefinedOrNull($scope.FromDate_sep)) {
            throw ("Select From Date");
        }
        if (angular.isUndefinedOrNull($scope.ToDate_sep)) {
            throw ("Select To Date");
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: {
                'FromDate': $scope.FromDate_sep, 'ToDate': $scope.ToDate_sep, 'pDescription': $scope.Description
                , 'List_active': $scope.EmployeeList_active, 'List_newlyjoined': $scope.EmployeeList_newlyjoined, 'List_mlvreturn': $scope.EmployeeList_mlvreturn
                , 'List_presentZero': $scope.EmployeeList_presentZero, 'List_separated': $scope.EmployeeList_separated, 'List_ssnd': $scope.EmployeeList_ssnd
                , 'List_ssna': $scope.EmployeeList_ssna, 'List_excepEmp': $scope.EmployeeList_excepEmp, 'List_attNotprocessed': $scope.EmployeeList_attNotprocessed
                , 'List_diffStatus': $scope.EmployeeList_diffStatus, 'List_approvedSalary': $scope.EmployeeList_approvedSalary
            },
            url: $scope.path + '/ProcessAll'

        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.msg = "Successfully Completed !!!";
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

