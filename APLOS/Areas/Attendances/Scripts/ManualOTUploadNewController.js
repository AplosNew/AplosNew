'use strict';
ManualOTUploadNewController.$inject = ['$window',"addressService", 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ManualOTUploadNewController($window,addressService, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Manual OT Upload';
    $scope.OTFilingList = [];
    $scope.SelectedEmpINOUTListExcel = [];
    $scope.PlantList = [];

    $scope.path = 'Attendances/ManualOTUploadNew/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "EmployeeCode"; $scope.search = "";


    $scope.searchByList = [{ value: 'EmployeeCode', name: "Employee Code" }, { value: 'OThour', name: "OT hour" }];


    // #region ddl

    $scope.GetPlantList = function () {
        $scope.PlantList = [];
        $http({
            method: 'GET',
            url: 'Attendances/ManualOTUploadNew/getplant/'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            for (var p = 0; p < $scope.PlantList.length; p++) {
                if ($scope.PlantList[p].Value == $window.plantId) {
                    $scope.OTManual.PlantId = $scope.PlantList[p].Value;
                }

            }
     
        });
    }
    

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
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        WorkDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        InTime: _Time,
        OutTime: _Time,
        OThour: null,
        EmpName: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        Remarks: null,
        IsConfirmed: false,
        APDEmpWorkDate: null,
        PlantId: null,
    };
    $scope.OTManual = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.OTManual = Object.assign({}, args.data);
        $scope.LoadEmpOfShiftWorkDateForExcel();
        $scope.enable = true;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Action = 'Save';

    $scope.ValidateToDate = function () {

        try {

            //if (new Date() < new Date($scope.OTManual.ToDate)) {
            //    $scope.OTManual.ToDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
            //    throw 'To Date should not be greater than Current date.';
            //}
            //if (new Date($scope.OTManual.ToDate) < new Date($scope.OTManual.FromDate)) {
            //    $scope.OTManual.ToDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
            //    throw 'To Date should not be less than From date.';
            //}

        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }


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

    $scope.Clear = function () {
        ClearFields();
        

        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.OTManual = Object.assign({}, $scope.ModelTemp);
        ClearDocument();
        $scope.SelectedEmpINOUTListExcel = [];
        $scope.GetPlantList();
        $scope.enable = false;
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
        if (baseService.arrayLength($scope.SelectedEmpINOUTListExcel) > 0)
            $scope.enable = true;
        else
            $scope.enable = false;
    }

    // Upload Excel 

    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    function ClearDocument() {
        document.getElementById('uploadImage').value = '';
        $scope.picdata = '';
        $scope.ModelNew.FileName = "";
        $scope.picdata = {};
        document.getElementById('uploadImage').value = "";
        $scope.SelectedEmpINOUTListExcel = [];
    };


    $scope.GetSample = function () {

        var reportFormat = "Excel";

        try {
            window.open('Attendances/ManualOTUploadNew/GetSampleReport?reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.SelectedEmpINOUTListExcel = [];
    $scope.ModelNew = {
        FileName: null
    }

    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.uploadexcelForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }
                $http({
                    method: 'POST',
                    url: 'Attendances/ManualOTUploadNew/ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("ModelNew", angular.toJson(data.ModelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: { 'ModelNew': $scope.ModelNew, 'file': $scope.picdata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }

                    else {
                        try {
                            $scope.ExcelUploadData = response.data;
                            $scope.CheckWorkingDateRange();
                            $scope.getEmpDetailsDataForExcel();
         
                        }
                    
                        catch (e) {

                            ShowResult(e, "failure");
                        }

                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.xyz = [];
    $scope.getEmpDetailsDataForExcel = function () {
        $scope.FilteredEmpList = [];
        //    var ExcelDataList = [];
        $scope.ExcelDataList = [];
        var ExcelDates = [];
        $scope.TempEmpSysId = [];
        $scope.xyz = [];
       
        for (var j = 0; j < $scope.ExcelUploadData.length; j++) {
            $scope.ExcelUploadData[j].WorkingDate = $filter('dateFiltering')(new Date($scope.ExcelUploadData[j].WorkingDate), 'dd-M-yyyy');
            $scope.ExcelDataList.push($scope.ExcelUploadData[j]);
        }

         $http({
            method: 'POST',
             data: { Id: $scope.OTManual.Id, PlantId: $scope.OTManual.PlantId, ToDate: $scope.OTManual.ToDate, FromDate: $scope.OTManual.FromDate, GetValuesOfExcel: $scope.ExcelDataList },
             url: 'Attendances/ManualOTUploadNew/LoadAllEmpDetails/'
        }).then(function successCallback(response) {
            $scope.FilteredEmpList = response.data;
            if ($scope.FilteredEmpList.length > 0) {
                for (var a = 0; a < $scope.ExcelDataList.length; a++) {

                    var GetEmpSystemId = $filter("filter")($scope.FilteredEmpList, { "Code": $scope.ExcelDataList[a].EmployeeCode, "APDEmpWorkDate": $scope.ExcelDataList[a].WorkingDate});
                    if (GetEmpSystemId == 0) {
                        
                    }
                    else {
                        $scope.TempEmpSysId = [];
                        $scope.TempEmpSysId = GetEmpSystemId;
                        $scope.TempEmpSysId[0].OTHr = $scope.ExcelDataList[a].OTHour;
                 //       $scope.TempEmpSysId[0].WorkingDate = $scope.ExcelDataList[a].WorkingDate;
                        if ($scope.SelectedEmpINOUTListExcel.length > 0) {
                            var count = $scope.SelectedEmpINOUTListExcel.length;
                            $scope.SelectedEmpINOUTListExcel[count] = $scope.TempEmpSysId[0];
                        }
                        else {
                            $scope.SelectedEmpINOUTListExcel[0] = $scope.TempEmpSysId[0];
                        }
                    }
                }
                
            }

        //$http({
        //    method: 'POST',
        //    data: { Id: $scope.OTManual.Id, PlantId: $scope.OTManual.PlantId, ToDate: $scope.OTManual.ToDate, FromDate: $scope.OTManual.FromDate, GetValuesOfExcel: ExcelDataList },
        //    url: 'Attendances/ManualOTUploadNew/LoadAllEmpDetailsForSelection/'
        //}).then(function successCallback(response) {
        //    $scope.FilteredEmpList = response.data;

            //if (baseService.arrayLength($scope.ExcelUploadData) > 0) {
                
            //    for (var e = 0; e < $scope.ExcelUploadData.length; e++) {
            //        for (var q = 0; q < $scope.FilteredEmpList.length; q++) {
                        
            //            if (baseService.arrayLength($scope.SelectedEmpINOUTListExcel) > 0) {
            //                var t = $scope.SelectedEmpINOUTListExcel.length;
            //                var DatesForExcel = $filter('dateFiltering')(new Date($scope.ExcelUploadData[e].WorkingDate), 'dd-M-yyyy');
            //                var FilteredDateVar = $filter('dateFiltering')(new Date($scope.FilteredEmpList[q].APDEmpWorkDate), 'dd-M-yyyy');

            //                if (($scope.ExcelUploadData[e].EmployeeCode == $scope.FilteredEmpList[q].Code) && (DatesForExcel == FilteredDateVar)) {
            //                    var OTHrMinForExcel = $scope.ExcelUploadData[e].OTHour;
            //                    var FilteredOTHrForExcel = $scope.FilteredEmpList[q].OTHr;

            //                    if (OTHrMinForExcel < FilteredOTHrForExcel) {
                               
            //                        $scope.FilteredEmpList[q].OTHr = OTHrMinForExcel;
            //                    }
            //                    else {
                       
            //                        $scope.FilteredEmpList[q].OTHr = FilteredOTHrForExcel;
            //                    }
            //                    $scope.SelectedEmpINOUTListExcel[t] = $scope.FilteredEmpList[q];
              
            //                    $scope.EnableDisableShift();
            //                }
            //            }
            //            else {
            //                try {
            //                    var DatesForExcel = $filter('dateFiltering')(new Date($scope.ExcelUploadData[e].WorkingDate), 'dd-M-yyyy');
            //                    var FilteredDateVar = $filter('dateFiltering')(new Date($scope.FilteredEmpList[q].APDEmpWorkDate), 'dd-M-yyyy');
   
            //                    if (($scope.ExcelUploadData[e].EmployeeCode == $scope.FilteredEmpList[q].Code) && (DatesForExcel == FilteredDateVar)) {
            //                        var OTHrMinForExcel = $scope.ExcelUploadData[e].OTHour;
            //                        var FilteredOTHrForExcel = $scope.FilteredEmpList[q].OTHr;
            //                        if (OTHrMinForExcel < FilteredOTHrForExcel) {
                                      
            //                            $scope.FilteredEmpList[q].OTHr = OTHrMinForExcel;
            //                        }
            //                        else {
                                 
            //                            $scope.FilteredEmpList[q].OTHr = FilteredOTHrForExcel;
            //                        }
            //                        $scope.SelectedEmpINOUTListExcel[0] = $scope.FilteredEmpList[q];
                           
            //                        $scope.EnableDisableShift();
            //                    }
            //                } catch (e) {
            //                    throw e;
            //                }
            //            }

            //        }
            //    }

            //}


        });
    }

    $scope.CheckWorkingDateRange = function () {
        try {
            if (baseService.arrayLength($scope.ExcelUploadData) > 0) {
                for (var c = 0; c < $scope.ExcelUploadData.length; c++) {
                    var DatesForExcel = $filter('dateFiltering')(new Date($scope.ExcelUploadData[c].WorkingDate), 'dd-M-yyyy');
      
                    if (new Date(DatesForExcel) >= new Date($scope.OTManual.FromDate) && new Date(DatesForExcel) <= new Date($scope.OTManual.ToDate)) {
                    }
                    else {
                        throw 'Working Date should be between From-Date and To-Date';
                        return $scope.SelectedEmpINOUTListExcel;
                    }
                }
            }
        }
        catch (e) {
            throw e;
        }
   
    }

    $scope.CheckValidationsForExcelUpload = function () {
        try {
            for (var i = 0; i < $scope.SelectedEmpINOUTListExcel.length; i++) {
   
                    //if ($scope.SelectedEmpINOUTListExcel[i].Category == null) {
                    //    throw 'Attendance is not processed ' + $scope.SelectedEmpINOUTListExcel[i].Code + ' ';
                    //}

                    //if ($scope.SelectedEmpINOUTListExcel[i].Category == "Present" || $scope.SelectedEmpINOUTListExcel[i].Category == "Late" || $scope.SelectedEmpINOUTListExcel[i].Category == "Weekend" || $scope.SelectedEmpINOUTListExcel[i].Category == "Holiday") {

                    //}
                    //else {
                    //    throw 'You cant add OT for the Day Status ' + $scope.SelectedEmpINOUTListExcel[i].Category + '   of the Employee ' + $scope.SelectedEmpINOUTListExcel[i].Code + '  ';

                    //}

                    //if ($scope.SelectedEmpINOUTListExcel[i].APDOutTime == null) {
                    //    throw 'The Employee ' + $scope.SelectedEmpINOUTListExcel[i].Code + ' has Missing Out time';
                    //}

                    //if ($scope.SelectedEmpINOUTListExcel[i].IsOTEntitled == false || $scope.SelectedEmpINOUTListExcel[i].IsOTEntitled == null) {
                    //    throw 'The Employee ' + $scope.SelectedEmpINOUTListExcel[i].Code + ' is not OT Entitled';
                    //  }

            //    $scope.ShowSaveBtn = false;
            }
        }
        catch (e) {

            ShowResult(e, "failure");
            throw e;
        }

    }

    $scope.SaveExcel = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
         //   $scope.CheckValidationsForExcelUpload();
            var MultipleExcelDataList = [];
            for (var j = 0; j < $scope.SelectedEmpINOUTListExcel.length; j++) {
                MultipleExcelDataList.push($scope.SelectedEmpINOUTListExcel[j]);

            }
            try {
                if (MultipleExcelDataList.length == 0) {
                    throw 'Enter atleast one Employee OT';
                }
                $http({
                    method: 'POST',
                    data: { data: $scope.OTManual, SaveMultipleEmpOTExcel: MultipleExcelDataList },
                    url: $scope.path + 'SaveExcelData'

                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Getgrid();
                        ClearFields();
                    }
                });
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        }
    }

    $scope.LoadEmpOfShiftWorkDateForExcel = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadEmpOfShiftWorkDate?EmpWorkDate=' + $scope.OTManual.WorkDate
        }).then(function successCallback(response) {
            $scope.SelectedEmpINOUTListExcel = response.data;
        });
    }

    // Remove from List

    $scope.RemoveEMPDataExcel = function () {
        var EmpDelIdForExcel = $scope.EMPIdForExcel;
        var EmpDelWorkDateForExcel = $scope.EMPWorkingDateForExcel;
        for (var i = 0; i < $scope.SelectedEmpINOUTListExcel.length; i++) {
            if ($scope.SelectedEmpINOUTListExcel[i].EmployeeSystemId === EmpDelIdForExcel && $scope.SelectedEmpINOUTListExcel[i].APDEmpWorkDate === EmpDelWorkDateForExcel) {
                $scope.SelectedEmpINOUTListExcel.splice(i, 1);
                return $scope.SelectedEmpINOUTListExcel;
            }
        }
    }

    $scope.ConfirmRemoveEmpINOUTDataForExcel = function (data) {
        $scope.EMPIdForExcel = data.EmployeeSystemId;
        $scope.EMPWorkingDateForExcel = data.APDEmpWorkDate;
        angular.element(document.querySelector("#RemoveEmpDataForExcel")).modal("show");
    }

}