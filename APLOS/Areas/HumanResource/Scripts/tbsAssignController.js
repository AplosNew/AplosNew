'use strict';
tbsAssignController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function tbsAssignController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee TBS Assignment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/TBSAssign/';
    $scope.getListUnassignUrl = $scope.path + 'GetAbsenteeismList';
    $scope.getListAssignUrl = $scope.path + 'GetAbsenteeismAssignedList';
    $scope.updateData = $scope.path + 'UpdateEmployeeStatus';

    $scope.Policy = "";
    $scope.assigned = [];
    $scope.unassigned = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.rowDataBoundOrder = function rowDataBoundOrder(e) {

        if (e.data.AbsentDays == 0)
            e.row.css("background-color", "#00ff00");

    }
    function checkChangeUnassigned(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.unassigned, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeUnassigned(e) {
        if (e.model.checkState == "check") {
            for (var i = 0; i < $scope.unassigned.length; i++) {
                $scope.unassigned[i].Active = true;
            }

            var checkbox = $("#Gridunassigned .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeUnassigned });
            }
        }
        else {
            for (var i = 0; i < $scope.unassigned.length; i++) {
                $scope.unassigned[i].Active = false;
            }
            var checkbox = $("#Gridunassigned .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridunassigned .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeUnassigned });
            }
        }
        //header level check
    }
    $scope.dataBoundUnassigned = function (args) {
        $("#Gridunassigned .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeUnassigned });

    }
    $scope.refreshTemplateUnassigned = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeUnassigned });
        }

        var valobj = $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.unassigned, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridunassigned .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeUnassigned });
    }
    $scope.getunassigneddata = function (args) {
        try {

            $http({
                method: 'POST',
                url: $scope.getListUnassignUrl,
                data: { 'plantid': "" },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.unassigned = [];
                }
                else {
                    $scope.Policy = response.data.Policy;
                    $scope.unassigned = response.data.DATA;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure', 'longabsent');
        }
    }
    $scope.getunassigneddata();


    function checkChangeassigned(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.assigned, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeassigned(e) {
        if (e.model.checkState == "check") {
            for (var i = 0; i < $scope.assigned.length; i++) {
                $scope.assigned[i].Active = true;
            }

            var checkbox = $("#Gridassigned .rowCheckboxA").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "change": null });
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "checked": true });
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "change": checkChangeassigned });
            }
        }
        else {
            for (var i = 0; i < $scope.assigned.length; i++) {
                $scope.assigned[i].Active = false;
            }
            var checkbox = $("#Gridassigned .rowCheckboxA").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "change": null });
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "checked": false });
                $($("#Gridassigned .rowCheckboxA")[i]).ejCheckBox({ "change": checkChangeassigned });
            }
        }
        //header level check
    }
    $scope.dataBoundassigned = function (args) {
        $("#Gridassigned .rowCheckboxA").ejCheckBox({ "change": checkChange });
        $("#headchkA").ejCheckBox({ "change": headCheckChangeassigned });

    }
    $scope.refreshTemplateassigned = function (args) {
        if (args.rowIndex == 0) {
            $("#headchkA").ejCheckBox({ "change": headCheckChangeassigned });
        }

        var valobj = $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.assigned, { 'Id': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Active == true)
                $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridassigned .rowCheckboxA")[args.rowIndex]).ejCheckBox({ "change": checkChangeassigned });
    }
    $scope.getassigneddata = function (args) {
        try {

            $http({
                method: 'POST',
                url: $scope.getListAssignUrl,
                data: { 'plantid': "" },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.assigned = [];
                }
                else {
                    $scope.assigned = response.data.DATA;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure', 'longabsent');
        }
    }
    $scope.getassigneddata();





    $scope.Assign = function () {
        try {

            var emplist = [];
            for (var i = 0; i < $scope.unassigned.length; i++) {
                if ($scope.unassigned[i].Active == true)
                    emplist.push($scope.unassigned[i].Id)
            }

            $http({
                method: 'POST',
                url: $scope.updateData,
                data: {
                    'empids': emplist, 'flag': "TBS"
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getunassigneddata();
                    $scope.getassigneddata();
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.UnAssign = function () {
        try {
            var excepemplist = null;
            for (var i = 0; i < $scope.assigned.length; i++) {
                if ($scope.assigned[i].Active == true && $scope.assigned[i].IsFromDA == true && baseService.isUndefinedOrNull($scope.assigned[i].Remarks)) {
                    if (baseService.isUndefinedOrNull(excepemplist)) {
                        excepemplist = $scope.assigned[i].EmployeeCode;
                    } else {
                        excepemplist = excepemplist + ',' + $scope.assigned[i].EmployeeCode;
                    }
                    
                }
                   
            }
            if (!baseService.isUndefinedOrNull(excepemplist)) {
                throw 'Remarks required for this employee['+ excepemplist+'].';
            }


            var emplist = [];
            for (var i = 0; i < $scope.assigned.length; i++) {
                var m = {}
                if ($scope.assigned[i].Active == true) {
                    m.EmpSystemId = $scope.assigned[i].Id;
                    m.IsFromDA = $scope.assigned[i].IsFromDA;
                    m.CaseNo = $scope.assigned[i].CaseNo;
                    m.Remarks = $scope.assigned[i].Remarks;
                    emplist.push(m)
                }
                    //emplist.push($scope.assigned[i].Id)
            }

            $http({
                method: 'POST',
                url: $scope.updateData,
                data: {
                    'empids': emplist, 'flag': "Active"
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getunassigneddata();
                    $scope.getassigneddata();
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.attendanceStatus = [];
    $scope.ViewEmployeeStatus = function (args)
    {
        try
        {

            $http({
                method: 'POST',
                url: $scope.path + 'ViewEmployeeStatus',
                data: { 'empid': args.data.Id, 'firstabsentdate': args.data.FirstAbsentDate },
                dataType: 'JSON'
            }).then(function successCallback(response)
            {
                var eDialog = $("#dialogProductionPlanView").data("ejDialog");
                eDialog.open();

                $scope.attendanceStatus = response.data;

            });
        } catch (e)
        {
            ShowResult(e, 'failure', 'longabsent');
        }
    }

    $scope.tempModel = {};
    $scope.OpenDialog = function (args) {
        try {
            $scope.Remarks = null;
            $scope.tempModel = {};           
            var eDialog = $("#dialogRemarks").data("ejDialog");
            eDialog.open();
            $scope.tempModel = args.data;
          
        } catch (e) {
            ShowResult(e, 'failure', 'longabsent');
        }
    }
    $scope.Remarks = null;
    $scope.CloseDialog = function (args) {
        try {
            var eDialog = $("#dialogRemarks").data("ejDialog");
            eDialog.close();


           
            for (var i = 0; i < $scope.assigned.length; i++) {
                if ($scope.assigned[i].Id == $scope.tempModel.Id)
                    $scope.assigned[i].Remarks = $scope.Remarks;
            }



        } catch (e) {
            ShowResult(e, 'failure', 'longabsent');
        }
    }



}