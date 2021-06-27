'use strict';
preallocatedOTController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function preallocatedOTController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Preallocated OT";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/preallocatedot/';
    $scope.saveUrl = $scope.path + 'create';


    $scope.SectionList = [];
    cboService.getSectionCbo(function (result) {
        $scope.SectionList = result;
    });

    $scope.modelNew = { SectionId: null, WorkDate: null, PreallocatedOTHr: null }

    $scope.searchdata = [];
    $scope.popUp = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.modelNew.SectionId)) {
            //    throw 'Select a section before employee selection.';
            //}
            $scope.searchdata = [];
            $http({
                method: 'GET',
                url: 'HumanResource/PreallocatedOT/GetEmployeeBySectionAndWorkDate?sectionId=' + $scope.modelNew.SectionId + '&workDate=' + $scope.modelNew.WorkDate
            }).then(function successCallback(response) {
                $scope.searchdata = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.SetOTHr = function () {
        for (var i = 0; i < $scope.searchdata.length; i++) {
            $scope.searchdata[i].PreallocatedOTHr = $scope.modelNew.PreallocatedOTHr;
            $scope.searchdata[i].WorkDate = $scope.modelNew.WorkDate;
        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent(true);
    };

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === "";
    }
    function checkChangeOperation(e) {

        var val = e.model.value;
        //item level check
        //var row = $filter('filter')($scope.searchdata, { 'EmpSystemID': e.model.value });

        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].EmpSystemID === e.model.value) {
                $scope.searchdata[i].Active = true;
            }
        }

    }
    function headCheckChangeOperation(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Grid").data("ejGrid");
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].EmpSystemID == filtered[j].EmpSystemID)
                            $scope.searchdata[i].Active = true;
                    }

                }
            }

            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeOperation });
            }
        }
        else {
            var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Active = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].EmpSystemID == filtered[j].EmpSystemID)
                            $scope.searchdata[i].Active = false;
                    }

                }
            }
            var checkbox = $("#Grid .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Grid .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeOperation });
            }
        }
        //header level check
    }
    $scope.dataBoundOperation = function (args) {
        $("#Grid .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });

    }
    $scope.refreshTemplateOperation = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeOperation });
        }

        var valobj = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.searchdata, { 'EmpSystemID': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Grid .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeOperation });
    }

    // #endregion

    $scope.saveList = [];
    $scope.Save = function () {
        $scope.saveList = [];
        try {

            for (var i = 0; i < $scope.searchdata.length; i++) {

                if ($scope.searchdata[i].Active) {
                    $scope.saveList.push($scope.searchdata[i]);
                }
            }
            for (var i = 0; i < $scope.saveList.length; i++) {
                $scope.saveList[i].WorkDate = $scope.modelNew.WorkDate;
            }
            for (var i = 0; i < $scope.saveList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.saveList[i].PreallocatedOTHr)) {
                    throw "Over Time hour is required for Employee Code:'" + $scope.saveList[i].EmployeeCode + "'.";
                }
                if ($scope.saveList[i].PreallocatedOTHr < 0) {
                    throw "Over Time hour cann't less than 0 for Employee Code:'" + $scope.saveList[i].EmployeeCode + "'.";
                }
            }



            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'entities': $scope.saveList, 'WorkDate': $scope.modelNew.WorkDate},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.searchdata = [];
                    $scope.popUp();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.modelNew = {};
        $scope.searchdata = [];
        $scope.saveList = [];
    }

}