'use strict';
CompensatoryOffNewController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CompensatoryOffNewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Compensatory Off";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.hSNTaxPercentages = [];
    $scope.path = 'Employees/CompensatoryOffNew/';
    $scope.LoadCompensatoryOffurl = $scope.path + 'LoadCompensatoryOff';
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
    $scope.compensatoryOff = {
        Id: null,
        PlantId: null,
        OriginalDate: null,
        CompensatoryDate: null,
        CompensatoryDateTreatmentType: null,
        HolidayCategoryId: null,
        IsOriginalDateOTApplicable: false,
        ForEntirePlant: false,
    };
    $scope.compensatoryOffEmployee = [];

    $scope.loadCompensatoryOff = function () {

        $http({
            method: 'POST',
            url: $scope.LoadCompensatoryOffurl,
            data: { 'id': $scope.compensatoryOff.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.compensatoryOff = {};
                //if (response.data.length > 0) {
                $scope.compensatoryOff = response.data.master[0];
                $scope.selectedemployees = response.data.employee;

                $scope.searchEmployee();
                //}

                $scope.ShowSave();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    // $scope.loadCompensatoryOff();

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
        if ($scope.compensatoryOff.CompensatoryDateTreatmentType == "W")
            $scope.compensatoryOff.HolidayCategoryId = "";
    }
    $scope.ShowSave = function () {
        $scope.employeeSave = true;
        if (angular.isUndefinedOrNull($scope.compensatoryOff.ForEntirePlant) == false) {
            if ($scope.compensatoryOff.ForEntirePlant == true) {
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
        $scope.minDate = $scope.compensatoryOff.OriginalDate;
        if (Date.parse($scope.compensatoryOff.CompensatoryDate) < Date.parse($scope.compensatoryOff.OriginalDate))
            $scope.minDate = $scope.compensatoryOff.CompensatoryDate;

        $scope.searchEmployee();
    }

    $scope.selectemployee = function () {

        try {
            $scope.validations();
            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');

            $scope.minDate = $scope.compensatoryOff.OriginalDate;
            if (Date.parse($scope.compensatoryOff.CompensatoryDate) < Date.parse($scope.compensatoryOff.OriginalDate))
                $scope.minDate = $scope.compensatoryOff.CompensatoryDate;

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
                    'offdate': $scope.compensatoryOff.OriginalDate,
                    'CompensatoryDate': $scope.compensatoryOff.CompensatoryDate,
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

            if (angular.isUndefinedOrNull($scope.compensatoryOff.OriginalDate) == true)
                throw "Please select General Working Date";
            if (angular.isUndefinedOrNull($scope.compensatoryOff.CompensatoryDate) == true)
                throw "Please select compensatory date";

            if (angular.isUndefinedOrNull($scope.compensatoryOff.CompensatoryDateTreatmentType) == true)
                throw "Please select holiday treatment type";

            if ($scope.compensatoryOff.CompensatoryDateTreatmentType == 'H')
                if (angular.isUndefinedOrNull($scope.compensatoryOff.HolidayCategoryId) == true)
                    throw "Please select Holiday Category";

            if ($scope.compensatoryOff.CompensatoryDateTreatmentType == 'W')
                if (angular.isUndefinedOrNull($scope.compensatoryOff.HolidayCategoryId) == false)
                    if ($scope.compensatoryOff.HolidayCategoryId != "")
                        throw "Holiday Category is not allowed for treatment type [week-off], please unselect holiday category";

        } catch (e) {
            throw e;
        }
    }

    $scope.saveemployeedata = function () {


        try {

            for (var i = 0; i < $scope.searchdata.length; i++) {
                var saveList = ej.DataManager($scope.selectedemployees).executeLocal(ej.Query().where("Id", "equal", $scope.searchdata[i].Id));

                if ($scope.searchdata[i].Active == true) {
                    if (saveList.length == 0)
                        $scope.selectedemployees.push($scope.searchdata[i]);
                }
                else {
                    if (saveList.length > 0) {
                        for (var k = 0; k < $scope.selectedemployees.length; k++) {
                            if (saveList[0].Id == $scope.selectedemployees[k].Id) {
                                $scope.selectedemployees.splice(k, 1);
                                break;
                            }
                        }
                    }
                        
                }

            }

            $scope.Back();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function getFields(input, field) {
        var output = [];
        for (var i = 0; i < input.length; ++i)
            output.push(input[i][field]);
        return output;
    }

    $scope.saveplantdata = function () {
        try {

            var data = ej.DataManager($scope.selectedemployees).executeLocal(ej.Query().select(["Id", "EmployeeName"]));


            $scope.validations();
            $http({
                method: 'POST',
                url: $scope.saveurl,
                data: {
                    'masterdata': $scope.compensatoryOff, 'employeedata': data
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
                    'masterdata': $scope.compensatoryOff, 'employeedata': args.data.Id
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
    $scope.editListCompensatoryOff = function () {

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
        $scope.compensatoryOff.Id = args.data.Id;
        $scope.loadCompensatoryOff();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }
    $scope.Delete = function () {
        try {
            if (angular.isUndefinedOrNull($scope.compensatoryOff.Id))
                throw 'Select compensation off policy first';
            $http({
                method: 'POST',
                url: $scope.deleteurl,
                data: {
                    'id': $scope.compensatoryOff.Id
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
        $scope.compensatoryOff = {};
        $scope.compensatoryOffEmployee = [];
        $scope.selectedemployees = [];
        $scope.editListCompensatoryOff();
        $scope.searchEmployee();
        $scope.employeeSave = true;
        $scope.Back();
    }

    $scope.Back = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    }
    $scope.Clear();
}