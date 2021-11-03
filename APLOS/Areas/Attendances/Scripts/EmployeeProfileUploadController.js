'use strict';
EmployeeProfileUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader'];
function EmployeeProfileUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader) {
    $scope.path = 'Attendances/EmployeeProfileUpload/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.title = 'Employee Profile';
    $scope.AttdnRawData = [];
    $scope.LeaveList = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $("#uploadshift").change(function () {
        $scope.picdata = this.files[0];
    });
    $("#uploadweekoff").change(function () {
        $scope.picdata = this.files[0];
    });
    $("#uploadleave").change(function () {
        $scope.picdata = this.files[0];
    });
    $("#uploadBank").change(function () {
        $scope.picdata = this.files[0];
    });

    //$scope.getFile = function () {
    //    $scope.progress = 0;
    //    fileReader.readAsDataUrl($scope.file, $scope)
    //        .then(function (result) {
    //            $scope.imageSrc = result;
    //        });
    //};

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope, $scope.file)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ModelNew = {
        Id: null,
        FileName: null

    };
    function GetShortList(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === null || list[i].EmployeeCode === '' || list[i].EmployeeCode === 'undefined') {

            }
            else {
                list2.push(list[i]);
            }
        }
        return list2;
    }
    function GetShortListShift(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === null || list[i].EmployeeCode === '' || list[i].EmployeeCode === 'undefined') {

            }
            else {
                var isFixed = false;
                var isRoster = false;

                if (list[i].ShiftSystemId === null || list[i].ShiftSystemId === '' || list[i].ShiftSystemId === 'undefined') {
                   
                }
                else {
                    isFixed = true;
                }

                if (list[i].RosterSystemID === null || list[i].RosterSystemID === '' || list[i].RosterSystemID === 'undefined') {
                   
                }
                else {
                    isRoster = true;
                }
                //===============================================
                if (isFixed && isRoster) {
                   
                }
                else if (isFixed === false && isRoster === false) {

                }
                else {
                    list2.push(list[i]);
                }
            }
        }
        return list2;
    }
    function GetShortListLeave(list) {
        var list2 = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === null || list[i].EmployeeCode === '' || list[i].EmployeeCode === 'undefined') {

            }
            else {
                if (list[i].LTSystemID === null || list[i].LTSystemID === '' || list[i].LTSystemID === 'undefined') {

                }
                else {
                    list2.push(list[i]);
                }
            }
        }
        return list2;
    }

    $scope.tabh = 11;
    $scope.setTab = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    //$scope.btnProcess=false;
    $scope.btnshow = function () {
        //$scope.ShowSaveBtn = true;
        return $scope.ShowSaveBtn;
    };
    //============================================================common=================================================================
    $scope.ImportData = function () {
        try {
            $scope.msg = "";
            //$scope.btnProcess = true;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: 'Attendances/EmployeeProfileUpload/ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.AttdnRawData = [];
                        //console.log('33', response.data);
                        var x = GetShortList(response.data);
                        //console.log('x', x);
                        $scope.AttdnRawData = x;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    //$scope.onrowdatabound = function (e) {
    //    if (e.data.Remarks !== '')
    //       // e.row.css("background-color", "red");
    //};

    $scope.msg = "";

    $scope.save = function () {

        try {
            //for (var i = 0; i < $scope.AttdnRawData.length; i++) {

            //    if ($scope.AttdnRawData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }

            //}
            $scope.msg = '';
            $scope.ShowSaveBtn = false;            
            $.ajax({
                type: "POST",
                url: 'Attendances/EmployeeProfileUpload/SaveProfileData',
                data: { 'epList': $scope.AttdnRawData },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.msg = "Data Saved Successfully ...";
                        $scope.AttdnRawData = [];
                        $("#uploadImage").val(null);
                       
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

     $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
         location.href = 'Attendances/EmployeeProfileUpload/GetSampleFile?reportFormat=' + ReportFormat;
    };


    //====================shift
    $scope.ShiftAssignment = [];
    $scope.GetSampleFileShift = function () {
        var ReportFormat = 'Excel';

        //var EmployeeIds = '';
        //if ($scope.SelectedEmployeeList.length == 0) {
        //    throw "Please Select Employee";
        //}
        //for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {

        //    if (EmployeeIds == "")
        //        EmployeeIds = "'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //    else
        //        EmployeeIds = EmployeeIds + ",'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //}

        location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat;
        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
    };
    $scope.ImportDataShift = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: 'Attendances/EmployeeProfileUpload/ImportDataShift',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.ShiftAssignment = [];
                        //console.log('33', response.data);
                        var x = GetShortListShift(response.data);
                        console.log('d', x);
                        $scope.ShiftAssignment = x;
                        console.log('d', $scope.ShiftAssignment);
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.saveShift = function () {

        try {
            //for (var i = 0; i < $scope.AttdnRawData.length; i++) {

            //    if ($scope.AttdnRawData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }

            //}

            $.ajax({
                type: "POST",
                url: 'Attendances/EmployeeProfileUpload/SaveShiftData',
                data: { 'epList': $scope.ShiftAssignment },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.ShiftAssignment = [];
                        $("#uploadshift").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $window.onresize = function (event) {
        $scope.actionComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#GridShiftAssignment").ejGrid("instance");
                var scrollerwidth = $("#mainmain").width();//Obtain the width of the container
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

    //====================Leave
    $scope.ImportDataLeave = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: 'Attendances/EmployeeProfileUpload/ImportDataLeave',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.LeaveList = [];
                        console.log('33', response.data);
                        var x = GetShortListLeave(response.data);
                        console.log('d', x);
                        $scope.LeaveList = x;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.GetSampleFileLeave = function () {
        var ReportFormat = 'Excel';

        //var EmployeeIds = '';
        //if ($scope.SelectedEmployeeList.length == 0) {
        //    throw "Please Select Employee";
        //}
        //for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {

        //    if (EmployeeIds == "")
        //        EmployeeIds = "'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //    else
        //        EmployeeIds = EmployeeIds + ",'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //}

        location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileLeave?reportFormat=' + ReportFormat;
        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
    };
    $scope.saveLeave = function () {

        try {
            //for (var i = 0; i < $scope.AttdnRawData.length; i++) {

            //    if ($scope.AttdnRawData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }

            //}

            $.ajax({
                type: "POST",
                url: 'Attendances/EmployeeProfileUpload/SaveLeaveData',
                data: { 'epList': $scope.LeaveList },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.LeaveList = [];
                        $("#uploadLeave").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //====================weekoff


    $scope.EmployeeInformationList = [];
    $scope.SelectedEmployeeList = [];
    $scope.LoadEmployeeList = function () {
        try {

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();




            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.SelectEmployee = function () {
        try {
            $scope.SelectedEmployeeList = [];
            for (var i = 0; i < $scope.EmployeeInformationList.length; i++) {

                if ($scope.EmployeeInformationList[i].CheckBoxSelect==true) {
                    $scope.SelectedEmployeeList.push($scope.EmployeeInformationList[i]);
                }
               
            }
            //if (baseService.isUndefinedOrNull($scope.SelectedEmployeeList)) {
            if ($scope.SelectedEmployeeList.length==0) {
                throw "Please Select Employee";
            } else {
                var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                eDialog.close();
            }

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

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeInformationList.length; i++) {
                $scope.EmployeeInformationList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };



    $scope.WeekOffAssignment = [];
    $scope.GetSampleFileWeekOff = function () {
        try {
            var ReportFormat = 'Excel';
            //var EmployeeIds = '';
            //if ($scope.SelectedEmployeeList.length == 0) {
            //    throw "Please Select Employee";
            //}
            //for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {

            //    if (EmployeeIds == "")
            //        EmployeeIds = "'" + $scope.SelectedEmployeeList[i].SystemId + "'";
            //    else
            //        EmployeeIds = EmployeeIds + ",'" + $scope.SelectedEmployeeList[i].SystemId + "'";
            //}



            location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileWeeOff?reportFormat=' + ReportFormat ;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.ImportDataWeekOff = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: 'Attendances/EmployeeProfileUpload/ImportDataWeekOff',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.WeekOffAssignment = [];                       
                        $scope.WeekOffAssignment = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.SaveWeekOff = function () {

        try {
            //for (var i = 0; i < $scope.AttdnRawData.length; i++) {

            //    if ($scope.AttdnRawData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }

            //}

            $.ajax({
                type: "POST",
                url: 'Attendances/EmployeeProfileUpload/SaveWeekOffData',
                data: { 'empList': $scope.WeekOffAssignment },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.WeekOffAssignment = [];
                        $("#uploadweekoff").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //====================Bank
    $scope.BankInfoList = [];
    $scope.ImportDataBank = function () {
        try {


            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }


                $http({
                    method: 'POST',
                    url: 'Attendances/EmployeeProfileUpload/ImportDataBank',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'modelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.BankInfoList = [];
                        console.log('33', response.data);
                        var x = GetShortListLeave(response.data);
                        console.log('d', x);
                        $scope.BankInfoList = x;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.GetSampleFileBank = function () {
        var ReportFormat = 'Excel';

        //var EmployeeIds = '';
        //if ($scope.SelectedEmployeeList.length == 0) {
        //    throw "Please Select Employee";
        //}
        //for (var i = 0; i < $scope.SelectedEmployeeList.length; i++) {

        //    if (EmployeeIds == "")
        //        EmployeeIds = "'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //    else
        //        EmployeeIds = EmployeeIds + ",'" + $scope.SelectedEmployeeList[i].SystemId + "'";
        //}

        location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileBank?reportFormat=' + ReportFormat;
        //location.href = 'Attendances/EmployeeProfileUpload/GetSampleFileShift?reportFormat=' + ReportFormat + '&EmployeeIds=' + EmployeeIds;
    };
    $scope.saveBank = function () {

        try {
            //for (var i = 0; i < $scope.AttdnRawData.length; i++) {

            //    if ($scope.AttdnRawData[i].Remarks !== '') {
            //        throw "Please Upload valied data";
            //    }

            //}

            $.ajax({
                type: "POST",
                url: 'Attendances/EmployeeProfileUpload/SaveBankData',
                data: { 'epList': $scope.BankInfoList },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {

                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.BankInfoList = [];
                        $("#uploadLeave").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

}





