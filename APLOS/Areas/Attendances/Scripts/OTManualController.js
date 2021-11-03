'use strict';
OTManualController.$inject = ["addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTManualController(addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Manual';
    $scope.OTFilingList = [];

    $scope.ShiftIdList = [];
    $scope.DepartmentList = [];
    $scope.SectionList = [];
    $scope.SubSectionList = [];
    $scope.SelectedEmpINOUTList = [];
    $scope.EntityList = [];
    $scope.PlantList = [];

    $scope.UOMList = [];

    $scope.path = 'Attendances/OTManual/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }, { value: 'OTWorkDate', name: "Work Date" }];


    // #region ddl
    $scope.GetPlant = function () {
        $scope.PlantList = [];
        $http({
            method: 'GET',
            url: 'Attendances/OTManual/getplant/'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            if (baseService.arrayLength($scope.PlantList) > 0) {
                $scope.OTManual.PlantId = response.data[0].Text;
                $scope.OTManual.PlantValue = response.data[0].Value;
            }
        });
    }
  
 

    $scope.getEntityWithChange = function () {
        $scope.EntityList = [];
        $http({
            method: 'GET',
            url: 'Attendances/OTManual/getentity/'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }

    $scope.GetShift = function () {
        $scope.ShiftIdList = [];
        $http({
            method: 'GET',
            url: 'Attendances/OTManual/getshift?PlantId=' + $scope.OTManual.PlantValue
        }).then(function successCallback(response) {
            $scope.ShiftIdList = response.data;
        });
    }
   

    $http({
        method: 'GET',
        url: 'Attendances/OTManual/getdepartment/'
    }).then(function successCallback(response) {
        $scope.DepartmentList = response.data;
        });

    $http({
        method: 'GET',
        url: 'Attendances/OTManual/getsection/'
    }).then(function successCallback(response) {
        $scope.SectionList = response.data;
        });

    $http({
        method: 'GET',
        url: 'Attendances/OTManual/getsubsection/'
    }).then(function successCallback(response) {
        $scope.SubSectionList = response.data;
    });

    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OTFilingList = response.data;
            ClearFields();
            ClearFieldsForms();
            $scope.GetPlant();
            $scope.getEntityWithChange();
        });
    }
    $scope.getData();

    var d = new Date();

    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        EmpSystemId: null,
        WorkDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        InTime: null,
        OutTime: null,
        ShiftSystemId: null,
        OThour: null,
        SubSectionId: null,
        SectionId: null,
        DepartmentId: null,
        EmpName: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        Remarks: null,
        IsConfirmed: false,
  //      PlantId: null,
        EntityId: null,
    };
    $scope.OTManual = Object.assign({}, $scope.ModelTemp);

    $scope.ValidateSelectedDate = function () {

        try {

            if (new Date() < new Date($scope.OTManual.WorkDate )) {
                $scope.OTManual.WorkDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                throw 'Work Date should not be greater than Current date.';
            }
 
        }
        catch (e) {
            ShowResult(e, "failure");
        }
     
    }

    $scope.Get = function (args) {

        $scope.OTManual = Object.assign({}, args.data);
        $scope.OTManual.WorkDate = $scope.OTManual.OTWorkDate;
        $scope.OTManual.InTime = $scope.OTManual.OTInTime;
        $scope.OTManual.OutTime = $scope.OTManual.OTOutTime;
        $scope.getEntityWithChange();
        $scope.GetShift();
        $scope.LoadEmpOfShiftWorkDate();
        ClearFieldsForms();
        $scope.enable = true;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OTFilingList = response.data;

        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            var MultipleDataList = [];
            for (var j = 0; j < $scope.SelectedEmpINOUTList.length; j++) {
                MultipleDataList.push($scope.SelectedEmpINOUTList[j]);
         
            }
            try {
                if (MultipleDataList.length == 0) {
                    throw 'Enter atleast one Employee OT';
                }
                $http({
                    method: 'POST',
                    data: { SaveMultipleEmpOT: MultipleDataList, data: $scope.OTManual},
                    url: $scope.path + 'Create'

                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Getgrid();
                        ClearFieldsForms();
                        $scope.Clear();
                    }
                });
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        }
    }


    function ClearFieldsForms() {
        $scope.Action = 'Save';       
        $scope.OTManual.EmpSystemId = null;
        $scope.OTManual.EmpName = null;
        $scope.OTManual.EmployeeCode = null;
        $scope.OTManual.EmployeeStatus = null;
        $scope.OTManual.InTime = null;
        $scope.OTManual.OutTime = null;
        $scope.OTManual.EMPOThour = null;
        $scope.OTManual.ManualOT = null;
       
    }

    $scope.Clear = function () {
        ClearFields();

        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.OTManual = Object.assign({}, $scope.ModelTemp);
        $scope.SelectedEmpINOUTList = [];
        $scope.EmpList = [];
  //      $scope.EntityList = [];
        $scope.ShiftIdList = [];
        $scope.enable = false;
        $scope.GetPlant();
    }

    ///////*********************Tabs*******************************
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    // Enable Disable Shift
    $scope.enable = false;
    $scope.EnableDisableShift = function () {
        if (baseService.arrayLength($scope.SelectedEmpINOUTList) > 0 )
            $scope.enable = true;
        else
            $scope.enable = false;
    }

    // Select All Check Box 

    $scope.refreshTemplateemployee = function () {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridEmployee").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmpList.length; i++) {
                $scope.EmpList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridEmployee").data("ejGrid");
        gridObj.refreshContent();
    };


    // Employee POP up

    $scope.EmpList = [];
    $scope.EmpPopUp = function () {
        angular.element(document.querySelector("#EmployeePop")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.OTManual.Id, PlantId: $scope.OTManual.PlantValue, DepartmentId: $scope.OTManual.DepartmentId, SectionId: $scope.OTManual.SectionId, SubSectionId: $scope.OTManual.SubSectionId, EmpCode: $scope.OTManual.EmployeeCode, EmpWorkDate: $scope.OTManual.WorkDate },
            url: 'Attendances/OTManual/LoadAllEmpDetailsForSelection/'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;
        });
    }

    $scope.EmpClear = function () {
        $scope.OTManual.EmpSystemId = null;
        $scope.OTManual.EmpName = null;
        $scope.OTManual.EmployeeCode = null;
        $scope.OTManual.EmployeeStatus = null;
        $scope.OTManual.InTime = null;
        $scope.OTManual.OutTime = null;
        $scope.OTManual.EMPOThour = null;
        $scope.OTManual.ManualOT = null;
        $scope.OTManual.EmpDayStatus = null;
    };

    $scope.EmpClearOTM = function () {
        $scope.OTManual.EmpSystemId = null;
        $scope.OTManual.EmpName = null;
        $scope.OTManual.EmployeeStatus = null;
        $scope.OTManual.InTime = null;
        $scope.OTManual.OutTime = null;
        $scope.OTManual.EMPOThour = null;
        $scope.OTManual.ManualOT = null;
        $scope.OTManual.EmpDayStatus = null;
    };
    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.SelectEmPDetails = function () {
        var EmpSelectedData = [];
        try {
            for (var i = 0; i < $scope.EmpList.length; i++) {
                if ($scope.EmpList[i].isSelected == true) {

                    if ($scope.EmpList[i].Category == null) {
                        throw 'Attendance is not processed ' + $scope.EmpList[i].Code + ' ';
                    }

                    if ($scope.EmpList[i].Category == "Present" || $scope.EmpList[i].Category == "Late" || $scope.EmpList[i].Category == "Weekend" || $scope.EmpList[i].Category == "Holiday") {
                       
                    }
                    else {
                        throw 'You cant add OT for the Day Status ' + $scope.EmpList[i].Category + '   of the Employee ' + $scope.EmpList[i].Code + '  ';

                    }

                    if ($scope.EmpList[i].APDOutTime == null) {
                        throw 'The Employee ' + $scope.EmpList[i].Code + ' has Missing Out time';
                    }
                    if ($scope.EmpList[i].IsOTEntitled == "No" || $scope.EmpList[i].IsOTEntitled == null) {
                        throw 'The Employee ' + $scope.EmpList[i].Code + ' is not OT Entitled';
                    }
            
                  
                }

            }
        }
        catch (e) {
            
            ShowResult(e, "failure");
            throw e;
        }
      

        for (var i = 0; i < $scope.EmpList.length; i++) {
            if ($scope.EmpList[i].isSelected == true) {
                if ($scope.EmpList[i].OTHr < $scope.OTManual.OThour) {
                    var MinOTH = $scope.EmpList[i].OTHr;
                    $scope.EmpList[i].OTHr = MinOTH;
                }
                else {
                    MinOTH = $scope.OTManual.OThour;
                    $scope.EmpList[i].OTHr = MinOTH;
                }
            }
         
        }
        for (var j = 0; j < $scope.EmpList.length; j++) {
            if ($scope.EmpList[j].isSelected == true) {
                if (baseService.arrayLength($scope.SelectedEmpINOUTList) > 0) {
                    for (var b = 0; b < $scope.SelectedEmpINOUTList.length; b++) {
           
                        var a = $scope.SelectedEmpINOUTList.length;
                        if ($scope.SelectedEmpINOUTList[b].EmployeeSystemId == $scope.EmpList[j].EmployeeSystemId && $scope.SelectedEmpINOUTList[b].APDEmpWorkDate == $scope.EmpList[j].APDEmpWorkDate) {

                            $scope.SelectedEmpINOUTList.splice(b, 1);
                        }

                    }
                    if ($scope.EmpList[j].isSelected == true) {
                        a = $scope.SelectedEmpINOUTList.length;
                        EmpSelectedData.push($scope.EmpList[j]);
                  
                        $scope.SelectedEmpINOUTList[a] = $scope.EmpList[j];
                        $scope.EnableDisableShift();
                    }
                   
                }
                else {
                    EmpSelectedData.push($scope.EmpList[j]);
                    $scope.SelectedEmpINOUTList = EmpSelectedData;
                    $scope.EnableDisableShift();
                }

            }
        }
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }


    $scope.GetEMPSysId = function () {
       $scope.EmpInOutList = [];
       var EMPdata = [];
        $http({
            method: 'POST',
            data: { Id: $scope.OTManual.Id, PlantId: $scope.OTManual.PlantValue, DepartmentId: $scope.OTManual.DepartmentId, SectionId: $scope.OTManual.SectionId, SubSectionId: $scope.OTManual.SubSectionId, EmpCode: $scope.OTManual.EmployeeCode, EmpWorkDate: $scope.OTManual.WorkDate },
            url: 'Attendances/OTManual/getempinouttime/'
        }).then(function successCallback(response) {
            $scope.EmpInOutList = response.data;
            try {
                if (baseService.arrayLength($scope.EmpInOutList) > 0) {

                    $scope.OTManual.EmpName = response.data[0].EmployeeName;
                    $scope.OTManual.InTime = response.data[0].APDEmpInDateAndTime;
                    $scope.OTManual.OutTime = response.data[0].APDEmpOutDateAndTime;
                    $scope.OTManual.EMPOThour = response.data[0].OTHr;
                    $scope.OTManual.ManualOT = response.data[0].ManualOT;
                    $scope.OTManual.EmpDayStatus = response.data[0].Category;
                    var i = 0;

                    if ($scope.EmpInOutList[i].PlantId != $scope.OTManual.PlantValue) {
                        $scope.EmpClearOTM();
                        throw 'This Employee does not exist in this Plant';
                    }

                    if ($scope.EmpInOutList[i].Category == null) {
                        $scope.EmpClearOTM();
                        throw 'Attendance is not processed ' + $scope.EmpInOutList[i].Code + ' ';
                    }

                    if ($scope.EmpInOutList[i].Category == "Present" || $scope.EmpInOutList[i].Category == "Late" || $scope.EmpInOutList[i].Category == "Weekend" || $scope.EmpInOutList[i].Category == "Holiday") {

                    }
                    else {
                        $scope.EmpClearOTM();
                        throw 'You cant add OT for the Day Status ' + $scope.EmpInOutList[i].Category + '   of the Employee ' + $scope.EmpInOutList[i].Code + '  ';

                    }

                    if ($scope.EmpInOutList[i].APDOutTime == null) {
                        $scope.EmpClearOTM();
                        throw 'The Employee ' + $scope.EmpInOutList[i].Code + ' has Missing Out time';
                    }
                    if ($scope.EmpInOutList[i].IsOTEntitled == false || $scope.EmpInOutList[i].IsOTEntitled == null) {
                        $scope.EmpClearOTM();
                        throw 'The Employee ' + $scope.EmpInOutList[i].Code + ' is not OT Entitled';
                    }

                    if ($scope.OTManual.EMPOThour < $scope.OTManual.OThour) {
                        var MinOTHour = $scope.OTManual.EMPOThour;
                        $scope.EmpInOutList[i].OTHr = MinOTHour;
                    }
                    else {
                        var MinOTHour = $scope.OTManual.OThour;
                        $scope.EmpInOutList[i].OTHr = MinOTHour;
                    }

                    if (baseService.arrayLength($scope.SelectedEmpINOUTList) > 0) {
                        var a = $scope.SelectedEmpINOUTList.length;
                        for (var b = 0; b < $scope.SelectedEmpINOUTList.length; b++) {
                            if ($scope.SelectedEmpINOUTList[b].EmployeeSystemId == $scope.EmpInOutList[i].EmployeeSystemId && $scope.SelectedEmpINOUTList[b].APDEmpWorkDate == $scope.EmpInOutList[i].APDEmpWorkDate) {
                                throw 'Same Employee OT already exist';
                            }
                        }
                        EMPdata.push($scope.EmpInOutList[i]);
                        $scope.SelectedEmpINOUTList[a] = EMPdata[i];
                        $scope.EnableDisableShift();

                    }
                    else {
                        EMPdata.push($scope.EmpInOutList[i]);
                        $scope.SelectedEmpINOUTList = EMPdata;
                        $scope.EnableDisableShift();
                    }
                }
                else {
                    $scope.EmpClearOTM();
                    throw 'The Employee ' + $scope.OTManual.EmployeeCode + ' doesnt exist in this Plant';
                }
              
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        });
    }

    // # end region  Employee

    $scope.RemoveEMPData = function () {
        var EmpDelId = $scope.EMPId;
        for (var i = 0; i < $scope.SelectedEmpINOUTList.length; i++) {
            if ($scope.SelectedEmpINOUTList[i].EmployeeSystemId === EmpDelId) {
                $scope.SelectedEmpINOUTList.splice(i, 1);
                return $scope.SelectedEmpINOUTList;
            }
        }
    }

    $scope.ConfirmRemoveEmpINOUTDataTab = function (EmployeeSystemId) {
        $scope.EMPId = EmployeeSystemId;
        angular.element(document.querySelector("#RemoveEmpData")).modal("show");
    }

    $scope.LoadEmpOfShiftWorkDate = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadEmpOfShiftWorkDate?EmpWorkDate=' + $scope.OTManual.WorkDate
        }).then(function successCallback(response) {
            $scope.SelectedEmpINOUTList = response.data;
        });
    }

  
}