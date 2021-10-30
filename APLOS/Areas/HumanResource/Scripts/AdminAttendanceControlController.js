'use strict';
AdminAttendanceControlController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$window', '$filter'];
function AdminAttendanceControlController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $window, $filter) {
    $rootScope.title = 'Admin Attendance Control';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/AdminAttendanceControl/';
    $scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.RemarksEmps = null;

    $scope.ModelNew = {
        CompanyId: null,
        PlantId: null,
    };


    $scope.ModelNewx = {
        CompanyId: null,
        PlantId: null,
    };

    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: 'humanresource/RosterPattern/getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.ModelNew.CompanyId }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }

    $scope.PlantListx = [];
    $scope.getPlantsx = function () {
        $http({
            method: 'GET',
            url: 'HumanResource/RosterPattern/getPlants',
            params: { 'cmp': $scope.ModelNewx.CompanyId }
        }).then(function success(response) {
            $scope.PlantListx = response.data;
        })
    }

    $scope.selectedStatus = [];
    $scope.getDayStatus = function (data) {

        angular.element(document.querySelector('#StatusModal')).modal('show');
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            if (data.data.Id == $scope.employeeAttendance[i].Id &&
                data.data.WorkDate == $scope.employeeAttendance[i].WorkDate) {


                $scope.A = i;
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'PlantId': $scope.ModelNew.PlantId },
            url: $scope.path + 'GetDayStatus'

        }).then(function successCallback(response) {
            $scope.selectedStatus = response.data;

        });
    };


    $scope.doubleStatus = function (e) {

        $scope.changestatus = e.data.DayType;
        var x = $scope.A;
        $scope.employeeAttendance[x].DayStatusNew = $scope.changestatus;
        angular.element(document.querySelector('#StatusModal')).modal('hide');
        $scope.lastIndex = 0;
    }


    $scope.selectedStatusx = [];
    $scope.getDayStatusx = function (data) {

        angular.element(document.querySelector('#StatusModalx')).modal('show');
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            if (data.data.Id == $scope.employeeAttendanceBySingleDate[i].Id &&
                data.data.WorkDate == $scope.employeeAttendanceBySingleDate[i].WorkDate) {

                $scope.Ax = i;
            }

        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'PlantId': $scope.ModelNewx.PlantId },
            url: $scope.path + 'GetDayStatus'

        }).then(function successCallback(response) {
            $scope.selectedStatusx = response.data;

        });
    };


    $scope.doubleStatusx = function (e) {

        $scope.changestatusx = e.data.DayType;
        var x = $scope.Ax;
        $scope.employeeAttendanceBySingleDate[x].DayStatusNew = $scope.changestatusx;
        angular.element(document.querySelector('#StatusModalx')).modal('hide');
        $scope.lastIndex = 0;
    }


    $scope.FromDateSingleDate = '';
    $scope.FromDate = '';
    $scope.ToDate = '';
    $scope.SetAs = 'In';
    $scope.Intime = null;
    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }
    $scope.queryCellInfo = function (args) {
        try {
            if (args.data.IsManualDayStatus == true) {
                if (args.column.field == "IsManualDayStatus" || args.column.field == "DayStatus") {
                    args.cell.bgColor = "#FF911D";
                }
            }
        } catch (e) {

        }

    }
    $scope.selectemployee = [];
    $scope.selectedSinglemployee = {};
    $scope.getAllEmployee = function () {

        Validation();
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'fromdate': $scope.FromDate, 'todate': $scope.ToDate, 'PlantId': $scope.ModelNew.PlantId },
            url: $scope.path + 'getAllEmployees'

        }).then(function successCallback(response) {
            $scope.selectemployee = response.data;

        });

    }
    $scope.employeeAttendanceBySingleDateSelection = [];
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    $scope.employeeAttendanceBySingleDateSelection[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDateSelection[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDateSelection[i].Active = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    $scope.employeeAttendanceBySingleDateSelection[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.employeeAttendanceBySingleDateSelection.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.employeeAttendanceBySingleDateSelection[i].Id == filtered[j].Id)
                            $scope.employeeAttendanceBySingleDateSelection[i].Active = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }
    $scope.saveemployeedata = function () {

        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Active': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.employeeAttendanceBySingleDate = row;
        }
        $scope.Back();
    }
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Back = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    }

    $scope.employeeAttendance = [];
    $scope.employeeAttendanceBySingleDate = [];
    $scope.allShift = [];
    $scope.selectSignleEmployee = function (args) {
        var eDialog = $("#dialogEmployeeSelect").data("ejDialog");
        eDialog.close();
        if (baseService.isUndefinedOrNull(args) == false)
            $scope.selectedSinglemployee = args.data;
        Validation();
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': $scope.selectedSinglemployee.Id, 'fromdate': $scope.FromDate, 'todate': $scope.ToDate, 'PlantId': $scope.ModelNew.PlantId },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendance = response.data.data;
            $scope.allShift = response.data.shift;

            var gridObj = $("#GridAttendanceControl").data("ejGrid");
            gridObj.refreshContent();
        });


    }
    $scope.allShiftSingleDay = [];

    $scope.selectSigleDate = function () {
        Validationx();
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'employeeid': '', 'fromdate': $scope.FromDateSingleDate, 'todate': $scope.FromDateSingleDate, 'PlantId': $scope.ModelNewx.PlantId },
            url: $scope.path + 'getAttendanceData'

        }).then(function successCallback(response) {
            $scope.employeeAttendanceBySingleDate = response.data.data;
            $scope.employeeAttendanceBySingleDateSelection = response.data.data;
            $scope.allShiftSingleDay = response.data.shift;

            var gridObj = $("#GridAttendanceControlBySingleDate").data("ejGrid");
            gridObj.refreshContent();

        });
    }

    $window.onload = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $window.onresize = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
        $scope.actionCompleteSingleDay("refresh");

    }
    $scope.actionCompleteSingleEmployee = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                $scope.RemarksEmps = null;
                try {
                    gridObj = $("#GridAttendanceControl").ejGrid("instance");
                    scrollerwidth = $("#Tab").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();

                } catch (e) {

                }
            }
        } catch (e) {

        }
    }
    $scope.actionCompleteSingleDay = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;
                $scope.RemarksEmps = null;
                try {
                    gridObj = $("#GridAttendanceControlBySingleDate").ejGrid("instance");
                    scrollerwidth = $("#TabEmployee").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();
                } catch (e) {

                }



            }
        } catch (e) {

        }
    }
    $scope.changeShift = function (args) {

        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            for (var j = 0; j < $scope.allShift.length; j++) {
                if ($scope.employeeAttendance[i].ShiftName == $scope.allShift[j].UserName) {
                    $scope.employeeAttendance[i].ShiftSystemID = $scope.allShift[j].SystemID;



                }


            }

        }

        return;

    }
    $scope.ActionchangeShift = function (args) {

    }

    $scope.shiftinfo = {};
    $scope.selectedShiftInfo = function (args) {
        var eDialog = $("#ViewShiftInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'systemid': args.data.ShiftSystemID, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getShift'

        }).then(function successCallback(response) {
            $scope.shiftinfo = response.data[0];
        });


    }

    $scope.attendanceinfo = [];
    $scope.showAttendanceInfo = function (args) {
        var eDialog = $("#ViewAttendanceInfo").data("ejDialog");
        eDialog.open();

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'empsystemid': args.data.Id, 'WorkDate': args.data.WorkDate },
            url: $scope.path + 'getAttendance'

        }).then(function successCallback(response) {
            $scope.attendanceinfo = response.data;
        });


    }
    $scope.rowDataBoundSingleEmployee = function rowDataBoundSingleEmployee(e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");

    }
    $scope.SaveSingleEmployee = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendance.length; i++) {
            $scope.employeeAttendance[i].ErrorMessage = "";
            try {
                if (
                    nullrecorder($scope.employeeAttendance[i].ShiftSystemID) != nullrecorder($scope.employeeAttendance[i].ShiftSystemIDOriginal)
                    || nullrecorder($scope.employeeAttendance[i].InDate) != nullrecorder($scope.employeeAttendance[i].InDateOriginal)
                    || nullrecorder($scope.employeeAttendance[i].InTime) != nullrecorder($scope.employeeAttendance[i].InTimeOriginal)
                    || nullrecorder($scope.employeeAttendance[i].OutDate) != nullrecorder($scope.employeeAttendance[i].OutDateOriginal)
                    || nullrecorder($scope.employeeAttendance[i].OutTime) != nullrecorder($scope.employeeAttendance[i].OutTimeOriginal)
                    || nullrecorder($scope.employeeAttendance[i].DayStatus) != nullrecorder($scope.employeeAttendance[i].DayStatusNew)
                ) {
                    DataToBeSaved.push($scope.employeeAttendance[i]);

                }
            } catch (e) {

            }

        }

        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved , 'Remarks': $scope.RemarksEmps },
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendance, { 'WorkDate': response.data.Data[i].WorkDate });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }


                var gridObj = $("#GridAttendanceControl").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSignleEmployee();

            }


        });


    }

    function Validation() {
        try {

            CheckField("Company", $scope.ModelNew.CompanyId);
            CheckField("Plant", $scope.ModelNew.PlantId);

        } catch (ex) {
            throw ex;
        }
    }

    function Validationx() {
        try {

            CheckField("Company", $scope.ModelNewx.CompanyId);
            CheckField("Plant", $scope.ModelNewx.PlantId);

        } catch (ex) {
            throw ex;
        }
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                ShowResult("" + fieldname + " can not be null...", 'failure');
                throw "" + fieldname + " can not be null...";
            }
        } catch (ex) {
            throw ex;
        }
    }

    $scope.SetIn = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Intime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridAttendanceControlBySingleDate").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].InTime = $scope.Intime;
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SetOut = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.Intime)) {
                throw "Select Time..";
            }
            var gridObj = $("#GridAttendanceControlBySingleDate").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.employeeAttendanceBySingleDate
            }
            for (var i = 0; i < filteredRecords.length; i++) {
                filteredRecords[i].OutTime = $scope.Intime;
            }
            $scope.employeeAttendanceBySingleDate = filteredRecords;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveSingleDay = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                if (
                    nullrecorder($scope.employeeAttendanceBySingleDate[i].ShiftSystemID) != nullrecorder($scope.employeeAttendanceBySingleDate[i].ShiftSystemIDOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].InDate) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].InTime) != nullrecorder($scope.employeeAttendanceBySingleDate[i].InTimeOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDate) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutDateOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTime) != nullrecorder($scope.employeeAttendanceBySingleDate[i].OutTimeOriginal)
                    || nullrecorder($scope.employeeAttendanceBySingleDate[i].DayStatus) != nullrecorder($scope.employeeAttendanceBySingleDate[i].DayStatusNew)
                ) {
                    DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i]);
                }
            } catch (e) {

            }
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved, 'Remarks': $scope.RemarksEmps},
            url: $scope.path + 'SaveSingleEmployee'

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

                for (var i = 0; i < response.data.Data.length; i++) {
                    var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': response.data.Data[i].Id });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                    }
                }

                var gridObj = $("#GridAttendanceControlBySingleDate").data("ejGrid");
                gridObj.refreshContent();
            }
            else {
                ShowResult(response.data.Message, 'success');

                $scope.selectSigleDate();

            }

        });
    }
    //
    //
    //
    // For the Update Tab
    //
    ///
    ////
    //

    $scope.RemarksUpload = null;

    //For the Employee Selection Modal
    $scope.EmpList = [];

    $scope.EmpsListCh = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getEmployees",
            params: { 'plantId': $scope.UpPlantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;
           
        });
    }

    $scope.selectEmps = function () {
        angular.element(document.querySelector('#EmpSelectModal')).modal('show');
    }


    var EmpSelList = "''";
    $scope.EmpSels = null;
    
    $scope.closeEmpSel = function () {
        $scope.EmpSelList = "''";
        if ($scope.EmpList.length > 0) {
            
            for (var i = 0; i < $scope.EmpList.length; i++) {
                if ($scope.EmpList[i].checked == true) {
                    $scope.EmpSels = $scope.EmpList[i].EmployeeCode;
                    EmpSelList = EmpSelList + ",'" + $scope.EmpList[i].SystemId + "'";
                }
            }
        }
        
    }


    // The Importing Sections

    $scope.UpPlantId = null;
    $scope.UpFromDate = null;
    $scope.UpToDate = null;

    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if (angular.isUndefinedOrNull($scope.UpPlantId) || angular.isUndefinedOrNull($scope.UpFromDate) || angular.isUndefinedOrNull($scope.UpToDate)) {
            ShowResult("Please Select Plant, From and To Date!!");
            throw ("Invalid Request");
        }

        try {

            $http({
                method: 'POST',
                url: $scope.path + 'GetSampleReport',
                data: {
                    'PlId': $scope.UpPlantId,
                    'FD': $scope.UpFromDate,
                    'TD': $scope.UpToDate,
                    'Emps': EmpSelList,
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });

        } catch (e) {

        }
    }


    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });

    $scope.ExcelUploadData = [];


    //IMporting The Data From the Excel File

    $scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }


            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: $scope.path + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);
                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                    }

                    catch (e) {

                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Save the File Data
    $scope.saveFileList = function () {

        if (angular.isUndefinedOrNull($scope.RemarksUpload) || $scope.RemarksUpload.length >! 1) {
            ShowResult("Filling the Remarks are mandatory!!");
            throw ("Invalid Request!!");
        }


        $http({
            method: 'POST',
            url: $scope.path + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData, 'FD': $scope.UpFromDate, 'TD': $scope.UpToDate, 'PlId': $scope.UpPlantId, 'Emps': EmpSelList, 'Remarks' : $scope.RemarksUpload }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    if ($rootScope.isCollapsed == true) {
                        $rootScope.toggle();
                    }
                    //$scope.getCurrentFileList();
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        });
    }
}