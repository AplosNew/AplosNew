'use strict';
longAbsenteeismAssignController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function longAbsenteeismAssignController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Long Absenteeism Assignment';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'HumanResource/LongAbsenteeismAssign/';
    $scope.getListUnassignUrl = $scope.path + 'GetAbsenteeismList';
    $scope.getListAssignUrl = $scope.path + 'GetAbsenteeismAssignedList';
    $scope.updateData = $scope.path + 'UpdateEmployeeStatus';
    $scope.saveChildUrlD = $scope.path + 'Save';

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
                    'empids': emplist, 'flag': "LONG ABSENTEEISM"
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

            var emplist = [];
            for (var i = 0; i < $scope.assigned.length; i++) {
                if ($scope.assigned[i].Active == true)
                    emplist.push($scope.assigned[i].Id)
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
    $scope.ViewEmployeeStatusModel = [];
    $scope.ViewEmployeeStatus = function (args) {
        try {

            $http({
                method: 'POST',
                url: $scope.path + 'ViewEmployeeStatus',
                data: { 'empid': args.data.Id, 'firstabsentdate': args.data.FirstAbsentDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                var eDialog = $("#dialogProductionPlanView").data("ejDialog");
                eDialog.open();

                $scope.attendanceStatus = response.data;

            });
        } catch (e) {
            ShowResult(e, 'failure', 'longabsent');
        }
    }



    $scope.LongAbsModel = {
        Id: null,
        EmpSystemId: null,
        DisciplinaryActionCategoryId: null,
        Description: null,
        EntryDate: $filter('dateFiltering')(Date.now()),
        ActionType: 'LA',
        Letters: null,
        LettersFormat: null,
        DADID: null,
        EmployeeCode: null,
        EmployeeName: null,
        Department: null,
        designation: null,
        EntryDate: null,
        NextLetterDueDate: null,
        LetterIssueDate: null,
        Sequence: null,
        DisciplinaryActionSettingDetailsId: null,
        DisciplinaryActionCategoryId: null,
        OVERDUE: null,
        EmployeeDisciplinaryActionDetailsId: null
    }

    $scope.AddFunction = function () {
        try {
            var eDialog = $("#LongAbsenteeismInfo").data("ejDialog");
            eDialog.open();
            var gridObj = $("#Gridassigned").data("ejGrid");
            $scope.LongAbsModel = gridObj.getSelectedRecords()[0];
            $scope.GetActionCategory();
            $scope.ShowcaseLetter = null;
            $scope.UserName = null;
            $scope.EmployeeCode = $scope.LongAbsModel.EmployeeCode;
            $scope.EmployeeName = $scope.LongAbsModel.EmployeeName; 
            $scope.Department = $scope.LongAbsModel.Department;  
            $scope.designation = $scope.LongAbsModel.designation;  
            $scope.LetterFormatList = [];
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Actionlist = [];
    $scope.GetActionCategory = function () {
        $http.get('HumanResource/LongAbsenteeismAssign/GetActionCategory')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.Actionlist = [];
                        $scope.Actionlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.saveChild = function () {
        try {
            Validation();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveChildUrlD,
                data: { 'longAbsenteeism': $scope.LongAbsModel, 'disciplinaryActionDetails': $scope.LongAbsModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LongAbsModel.DADID = response.data.DADID;
                    var eDialog = $("#LongAbsenteeismInfo").data("ejDialog");
                    eDialog.close();
                    $scope.getassigneddata();

                    var gridObj = $("#Gridassigned").data("ejGrid");
                    gridObj.refreshContent();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function Validation() {
        try {
            CheckField("Letter Issue Day", $scope.LongAbsModel.DisciplinaryActionCategoryId);
            CheckField("Description", $scope.LongAbsModel.Description);
            CheckField("EntryDate", $scope.LongAbsModel.EntryDate);

        } catch (ex) {
            throw ex;
        }
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridLongAbsenteeismGet").data("ejGrid");
        $scope.LongAbsModel = gridObj.getSelectedRecords()[0];
    };

    $scope.LetterList = [];
    $scope.LetterFormatList = [];
    $scope.getLetterDescription = function () {
        $http.get('humanresource/LongAbsenteeismAssign/GetAllDescription?DisciplinaryActionCategoryId=' + $scope.LongAbsModel.DisciplinaryActionCategoryId)
            .then(function (response) {
                $scope.LetterList = response.data;

                 $scope.count = $scope.LetterList.length;
                //switch (count) {
                //    case 1:
                //        $scope.text = "One";
                //        break;
                //    case 2:
                //        $scope.text  = "Two";
                //        break;
                //    case 3:
                //        $scope.text  = "Three";
                //        break;
                //    case 4:
                //        $scope.text  = "Four";
                //        break;
                //    case 5:
                //        $scope.text  = "Five";
                //        break;
                //    case 6:
                //        $scope.text  = "Six";
                //        break;
                //    case 7:
                //        $scope.text  = "Seven";
                //        break;
                //    case 8:
                //        $scope.text  = "Eight";
                //        break;
                //    default:
                //        $scope.text  = "Zero";
                //}
                //$scope.TextIncount = $scope.text ;
                
                if ($scope.LetterList.length > 0) {




                    $scope.ShowcaseLetter = "Yes";
                    $scope.UserName = $scope.LetterList[0].UserName;

                    $scope.count = $scope.LetterList[0].Count;
                    $scope.UserName = $scope.LetterList[0].UserName;

                    $scope.LongAbsModel.EntryDate = $scope.LongAbsModel.FirstAbsentDate;

                    var date = new Date($scope.LongAbsModel.FirstAbsentDate);
                    var newDate = date.setDate(date.getDate() + Number($scope.LetterList[0].LetterIssueDay));

                    
                    $scope.LongAbsModel.LetterIssueDate = $filter('dateFiltering')(new Date(newDate), 'dd-MM-yyyy');


                    var date2 = new Date($scope.LongAbsModel.LetterIssueDate);
                    var newNextDueDate = date2.setDate(date2.getDate() + Number($scope.LetterList[0].NextLetterDueDate));
                    $scope.LongAbsModel.NextLetterDueDate = $filter('dateFiltering')(new Date(newNextDueDate), 'dd-MM-yyyy');


                    $scope.LongAbsModel.DisciplinaryActionSettingDetailsId = $scope.LetterList[0].Id;
                    $scope.LongAbsModel.DisciplinaryActionCategoryId = $scope.LetterList[0].DisciplinaryActionCategoryId;
                    $scope.Sequence = $scope.LetterList[0].Sequence;



                    




                    $scope.Id = $scope.LetterList[0].Id
                    $http.get('humanresource/LongAbsenteeismAssign/GetLetterFormet?LetterFormetId=' + $scope.Id)
                        .then(function (response) {
                            $scope.LetterFormatList = response.data;
                            for (var i = 0; i < $scope.LetterFormatList.length; i++) {
                                if ($scope.LetterFormatList[i].IsDefault == true) {
                                    $scope.LongAbsModel.LettersFormat = $scope.LetterFormatList[i].Id;
                                }
                            }
                        });
                }
                else {
                    $scope.LetterFormatList = [];
                    $scope.ShowcaseLetter = null;
                    $scope.UserName = null;
                    $scope.LongAbsModel.LettersFormat = null;
                    $scope.LongAbsModel.Description = null;
                    $scope.LongAbsModel.EntryDate = null;
                }
            });
    };


}