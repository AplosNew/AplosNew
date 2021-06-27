'use strict';
LayOffController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LayOffController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Lay Off";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.hSNTaxPercentages = [];
    $scope.path = 'Employees/LayOff/';
    $scope.LoadLayOffurl = $scope.path + 'LoadLayOff';
    $scope.HolidayCategoryurl = $scope.path + 'HolidayCategory';
    $scope.searchEmployeesurl = $scope.path + 'searchEmployees';
    $scope.editEmployeesurl = $scope.path + 'GetView';

    $scope.gettaxcategoriesurl = $scope.path + 'GetTaxCategories';
    $scope.saveurl = $scope.path + 'Save';
    $scope.deleteurl = $scope.path + 'Delete';

    $scope.plantSave = false;
    $scope.employeeSave = true;


    $scope.HolidayType = [{ 'type': 'Week-Off', 'Id': 'W' }, { 'type': 'Holiday', 'Id': 'H' }];
    $scope.HolidayCategorycbo = [];
    $scope.LayOff = {
        Id: null,
        PlantId: null,
        Description: null,
        FromDate: null,
        ToDate: null
    };
    $scope.LayOffEmployee = [];

    $scope.loadLayOff = function () {

        $http({
            method: 'POST',
            url: $scope.LoadLayOffurl,
            data: { 'id': $scope.LayOff.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.LayOff = {};
                //if (response.data.length > 0) {
                $scope.LayOff = response.data.master[0];
                $scope.selectedemployees = response.data.employee;

                $scope.searchEmployee();
                //}

                $scope.ShowSave();
            }
        }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    }
    // $scope.loadLayOff();

    $scope.loadHolidayCategory = function () {
        $http({
            method: 'POST',
            url: $scope.HolidayCategoryurl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {

            }
            else {
                $scope.HolidayCategorycbo = response.data;
            }
        }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    }
    $scope.loadHolidayCategory();

    $scope.treatment = function () {
        if ($scope.LayOff.ToDateTreatmentType == "W")
            $scope.LayOff.HolidayCategoryId = "";
    }
    $scope.ShowSave = function () {
        $scope.employeeSave = true;
        if (angular.isUndefinedOrNull($scope.LayOff.ForEntirePlant) == false) {
            if ($scope.LayOff.ForEntirePlant == true) {
                $scope.employeeSave = false;
            }
        }
    }
    $scope.actionCompleteSelected = function (args) {
        //if (args.requestType == "filtering") {
        //    var gridObj = $("#GridSOItemSelected").ejGrid("instance");
        //    for (var i = 0; i < $scope.searchdata.length; i++) {
        //        $scope.searchdata[i].Active = false;
        //    }
        //    //var gridObj = $("#Grid").data("ejGrid");
        //    gridObj.render(); 

        //}
    }
    $scope.IsFutureDOJAccepted = false;
    $scope.minDate = "";
    $scope.FutureDateAccepted = function () {
        $scope.minDate = $scope.LayOff.FromDate;
        if (Date.parse($scope.LayOff.ToDate) < Date.parse($scope.LayOff.FromDate))
            $scope.minDate = $scope.LayOff.ToDate;

        $scope.searchEmployee();
    }

    $scope.selectemployee = function () {

        try {
            $scope.validations();
            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');

            $scope.minDate = $scope.LayOff.FromDate;
            if (Date.parse($scope.LayOff.ToDate) < Date.parse($scope.LayOff.FromDate))
                $scope.minDate = $scope.LayOff.ToDate;

            $scope.searchEmployee();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    /////EMPLOYEE SEARCH/////
    $scope.searchfield = "EmployeeCode"; $scope.searchtext = ""; $scope.searchdata = [], $scope.selectedemployees = [];
    $scope.searchByList = [{ 'name': 'Employee Code', 'value': 'EmployeeCode' },
    { 'name': 'Employee Name', 'value': 'EmployeeName' },
    { 'name': 'Department', 'value': 'Department' },
    { 'name': 'Designation', 'value': 'Designation' }];

    $scope.searchEmployee = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.searchEmployeesurl,
                data: {
                    'column': $scope.searchfield,
                    'value': $scope.searchtext,
                    'offdate': $scope.LayOff.FromDate,
                    'ToDate': $scope.LayOff.ToDate,
                    'IsFutureDOJAccepted': $scope.IsFutureDOJAccepted
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                }
                else {
                    $scope.searchdata = response.data;

                    //default checking on sceen data
                    for (var i = 0; i < $scope.searchdata.length; i++) {
                        for (var j = 0; j < $scope.selectedemployees.length; j++) {
                            if ($scope.selectedemployees[j].Id == $scope.searchdata[i].Id)
                                $scope.searchdata[i].Active = true;
                        }
                    }
                }
            }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.searchdata, { 'Id': e.model.value });
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
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].Id == filtered[j].Id)
                            $scope.searchdata[i].Active = true;
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
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].Id == filtered[j].Id)
                            $scope.searchdata[i].Active = false;
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
        var row = $filter('filter')($scope.searchdata, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }



    /////save/////////
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    $scope.validations = function () {
        try {

            if (angular.isUndefinedOrNull($scope.LayOff.FromDate) == true)
                throw "Please select From Date";
            if (angular.isUndefinedOrNull($scope.LayOff.ToDate) == true)
                throw "Please select To Date";


        } catch (e) {
            throw e;
        }
    }

    $scope.saveemployeedata = function () {


        try {
            $scope.selectedemployees = [];
            for (var i = 0; i < $scope.searchdata.length; i++) {
                if ($scope.searchdata[i].Active == true) {
                    $scope.selectedemployees.push($scope.searchdata[i]);
                }

            }

            $scope.Back();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.saveplantdata = function () {
        try {
            $scope.validations();
            $http({
                method: 'POST',
                url: $scope.saveurl,
                data: {
                    'masterdata': $scope.LayOff, 'employeedata': $scope.selectedemployees
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.deleteEmployee = function (args) {
        try {

            $http({
                method: 'POST',
                url: $scope.path + 'deleteemployee',
                data: {
                    'masterdata': $scope.LayOff, 'employeedata': args.data.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error == false) {
                    for (var i = 0; i < $scope.selectedemployees.length; i++) {
                        if ($scope.selectedemployees[i].Id == args.data.Id) {
                            $scope.selectedemployees.splice(i, 1);
                        }
                    }

                    for (var i = 0; i < $scope.searchdata.length; i++) {
                        if ($scope.searchdata[i].Id == args.data.Id) {
                            $scope.searchdata[i].Active = false;
                        }
                    }

                    ShowResult(response.data.Message, 'success');
                    var gridObj = $("#Gridselectedemployee").data("ejGrid");
                    gridObj.refreshContent();

                    var gridObjs = $("#Gridemployee").data("ejGrid");
                    gridObjs.refreshContent();
                }
                else {
                    ShowResult(response.data.Message, 'failure');

                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }


    }

    ////edit screen
    $scope.editdata = [];
    $scope.editListLayOff = function () {

        $http({
            method: 'POST',
            url: $scope.editEmployeesurl,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.editdata = response.data;

            }
        }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    }


    $scope.editdoubleclick = function (args) {
        $scope.LayOff.Id = args.data.Id;
        $scope.loadLayOff();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.Delete = function () {
        try {
            if (angular.isUndefinedOrNull($scope.LayOff.Id))
                throw 'Select compensation off policy first';
            $http({
                method: 'POST',
                url: $scope.deleteurl,
                data: {
                    'id': $scope.LayOff.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.Clear();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.LayOff = {};
        $scope.LayOffEmployee = [];
        $scope.selectedemployees = [];
        $scope.editListLayOff();
        $scope.searchEmployee();
        $scope.employeeSave = true;
        $scope.Back();
    }

    $scope.Back = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    }
    $scope.Clear();
}