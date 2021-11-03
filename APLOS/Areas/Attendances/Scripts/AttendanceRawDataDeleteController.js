'use strict';
AttendanceRawDataDeleteController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function AttendanceRawDataDeleteController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Attendance Raw Data Delete';
    $scope.path = 'Attendances/AttendanceRawDataDelete/';
    $scope.AttendanceRawDataDateWiseUrl = $scope.path + 'GetAttendanceRawDataDateWise';
    $scope.SaveAttendanceRawDataDateWiseUrl = $scope.path + 'SaveAttendanceRawDataDateWise';

    $scope.GetAllEmploteeListUrl = $scope.path + 'GetAllEmploteeList';
    $scope.AttendanceRawDataEmployeeWiseUrl = $scope.path + 'GetAttendanceRawDataEmployeeWise';
    $scope.SaveAttendanceRawDataEmployeeWiseUrl = $scope.path + 'SaveAttendanceRawDataEmployeeWise';

    $scope.ShowSaveButton = false;
    $scope.AttendanceRawDataDateWise = [];
    $scope.customPara = {
        procdate: $filter('dateFiltering')(Date.now()),
        fromdate: null,
        todate: null

    };
    $scope.onrowdatabound = function (e) {
        if (e.data.InTimeRowID === 'YES')
            e.row.css("background-color", "#fde3a7");
        if (e.data.OutTimeRowID === 'YES')
            e.row.css("background-color", "#fde3a7");
    };
    $scope.onrowdatabound1 = function (e) {
        if (e.data.InTimeRowID === 'YES')
            e.row.css("background-color", "#fde3a7");
        if (e.data.OutTimeRowID === 'YES')
            e.row.css("background-color", "#fde3a7");
    };
    $scope.GetAttendanceRawDataDateWise = function () {
        try {
            //var previousDay = null;
            if (baseService.isUndefinedOrNull($scope.customPara.procdate)) {
                //
            }

            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.AttendanceRawDataDateWiseUrl + "?WDate=" + $scope.day,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.AttendanceRawDataDateWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    for (var i = 0; i < $scope.AttendanceRawDataDateWise.length; i++) {
                        $scope.AttendanceRawDataDateWise[i].DOJ = new Date($scope.AttendanceRawDataDateWise[i].DOJ);
                    }
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateAttendanceRawDataDateWise = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AttendanceRawDataDateWise.length; i++) {
                $scope.AttendanceRawDataDateWise[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };


    // #region Tab











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


    $window.onresize = function (event) {

        $scope.actionCompleteSelected();
        $scope.actionCompleteSelected1();


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
                var gridObj = $("#GridAttendanceRawDataEmployeeWise").ejGrid("instance");
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






    $scope.SaveData = function () {


        try {
            $scope.employees = [];

            if ($scope.tabh === 11) {
                $scope.employees = [];
                for (var i = 0; i < $scope.TobeConfirmedEmployees.length; i++) {

                    if ($scope.TobeConfirmedEmployees[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.TobeConfirmedEmployees[i]);
                    }

                }
            }
            else if ($scope.tabh === 22) {
                $scope.employees = [];
                for (var i = 0; i < $scope.AttendanceRawDataDateWise.length; i++) {

                    if ($scope.AttendanceRawDataDateWise[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.AttendanceRawDataDateWise[i]);
                    }

                }
            }




            $scope.day = $filter('dateFiltering')($scope.customPara.procdate, 'dd-M-yyyy');

            $http({
                method: 'POST',
                url: $scope.SaveAttendanceRawDataDateWiseUrl,
                data:
                {
                    'AttendanceRawData': $scope.employees,
                    'WDate': $scope.day

                },
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetAttendanceRawDataDateWise();
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };


            //$.ajax({
            //    type: "POST",
            //    url: $scope.SaveAttendanceRawDataDateWiseUrl,
            //    data:
            //    {
            //        'AttendanceRawData': $scope.employees,
            //        'WDate': $scope.day

            //    },
            //    dataType: "json",
            //    success: function (response) {
            //        if (response.Error === true) {
            //            ShowResult(response.Message, 'failure');
            //        }
            //        else {
            //            ShowResult(response.Message, 'success');
            //            $scope.GetAttendanceRawDataDateWise();
            //        }
            //    },
            //    error: function (response) {
            //        ShowResult(response.Message, "failure");
            //    }

            //});
        } catch (e) {
            ShowResult(e, "failure");
        }

    };


    // employee wise 


    $scope.Employeelist = [];
    $scope.getAllEmployee = function () {
        try {
            //var previousDay = null;
            if (baseService.isUndefinedOrNull($scope.customPara.fromdate)) {
                throw "Enter From Date";
            }
            if (baseService.isUndefinedOrNull($scope.customPara.todate)) {
                throw "Enter to Date";
            }

            $http({
                method: 'GET',
                url: $scope.GetAllEmploteeListUrl,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.Employeelist = response.data.data;
                    var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
                    eDialog.open();
                    $scope.ShowSaveButton = true;
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.employees = {};
    $scope.AttendanceRawDataEmployeeWise = [];
    $scope.selectSignleEmployee = function () {
        try {
            $scope.employees = {};
            var gridObj = $("#GridWorkCenterProduct").data("ejGrid");
            $scope.employees = gridObj.getSelectedRecords()[0];
            var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
            eDialog.close();


            $scope.GetAttendanceRawDataEmployeeWise();


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetAttendanceRawDataEmployeeWise = function () {
        try {



            var FromDate = $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy');
            var ToDate = $filter('dateFiltering')($scope.customPara.todate, 'dd-M-yyyy');
            $http({
                method: 'GET',
                url: $scope.AttendanceRawDataEmployeeWiseUrl + "?FromDate=" + FromDate + "&ToDate=" + ToDate + "&EmpSystemId=" + $scope.employees.SystemId,
                //data: JSON.stringify(data),
                headers: {
                    'Content-Type': 'application/json'
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.Message, 'failure');
                } else {
                    $scope.AttendanceRawDataEmployeeWise = response.data.data;
                    $scope.ShowSaveButton = true;
                    for (var i = 0; i < $scope.AttendanceRawDataEmployeeWise.length; i++) {
                        $scope.AttendanceRawDataEmployeeWise[i].DOJ = new Date($scope.AttendanceRawDataEmployeeWise[i].DOJ);
                        $scope.AttendanceRawDataEmployeeWise[i].PDate = new Date($scope.AttendanceRawDataEmployeeWise[i].PDate);
                    }
                }

            }), function errorCallBack(response) {
                ShowResult(response.Message, 'failure');
            };


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateAttendanceRawDataEmployeeWise = function (args) {
        $("#headchk1").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise1 });
    };

    function CheckBoxSelectAllEmolyeeWise1(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridAttendanceRawDataEmployeeWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.AttendanceRawDataEmployeeWise.length; i++) {
                $scope.AttendanceRawDataEmployeeWise[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridAttendanceRawDataEmployeeWise").data("ejGrid");
        gridObj.refreshContent();
    };
    $scope.SaveDataEmployeeWise = function () {


        try {
            $scope.employees = [];

            if ($scope.tabh === 11) {
                $scope.employees = [];
                for (var i = 0; i < $scope.AttendanceRawDataEmployeeWise.length; i++) {

                    if ($scope.AttendanceRawDataEmployeeWise[i].CheckBoxSelect === true) {
                        $scope.employees.push($scope.AttendanceRawDataEmployeeWise[i]);
                    }

                }
            }

            if ($scope.employees.length == 0) {
                throw "Please select data";
            }



            var FromDate = $filter('dateFiltering')($scope.customPara.fromdate, 'dd-M-yyyy');
            var Todate = $filter('dateFiltering')($scope.customPara.todate, 'dd-M-yyyy');

            $http({
                method: "POST",
                dataType: 'JSON',
                data:
                {
                    'AttendanceRawData': $scope.employees,
                    'pFromDate': FromDate,
                    'pToDate': Todate
                },
                url: $scope.SaveAttendanceRawDataEmployeeWiseUrl,

            }).then(function successCallback(response) {

                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.GetAttendanceRawDataEmployeeWise();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });//http

            
        } catch (e) {
            ShowResult(e, "failure");
        }

    };
}