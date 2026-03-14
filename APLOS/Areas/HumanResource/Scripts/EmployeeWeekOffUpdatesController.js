'use strict';
EmployeeWeekOffUpdatesController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeWeekOffUpdatesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Week Off Updates';
    $rootScope.title1 = 'Week Off Updates';
    $scope.Action = 'Save';
    var url = "humanresource/WeekOffUpdates/";
    $scope.path = "humanresource/WeekOffUpdates/";



    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // ***** Code For the Fist Tab

    $scope.employee = null;
    $scope.EmpSystemId = null;
    $scope.EmpScatteredList = [];
    $scope.EffectiveDates = new Date();
    $scope.EmpGridList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getEmployees"
    }).then(function succ(resp) {
        $scope.EmployeesList = resp.data;
    });

    $scope.EmployeesList = [];
    $scope.selectEmployee = function () {
        angular.element(document.querySelector('#employeesModal')).modal('show');
    }

    var nw = document.getElementById("normalweek");
    var sw = document.getElementById("scWeekhtml");

    sw.style.display = "none";
    nw.style.display = "block";

    $scope.doubleEmployee = function (e) {
        $scope.employee = e.data.EmployeeName;
        $scope.EmpSystemId = e.data.SystemId;
        $scope.EmpGridList = [];
        angular.element(document.querySelector('#employeesModal')).modal('hide');

        $http({
            method: 'POST',
            url: $scope.path + "getEmpWeekOff",
            data: {'EmpId':$scope.EmpSystemId}
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.WekName = resp.data[0].UserName;
                $scope.WekId = resp.data[0].WOHeaderId;
                $scope.EffectiveDates = resp.data[0].EffectiveDate;

                $scope.EmpGridList = resp.data;
            }
            else {
                $scope.WekName =null;
                $scope.WekId = null;
                $scope.EffectiveDates = null;
            }
            
        });

        if ($scope.budgetsList.includes(e.data.BudgetCode)) {
            $http({
                method: 'POST',
                url: $scope.path + "getWeekOffsLists",
                data: { 'EmpID': $scope.EmpSystemId }
            }).then(function succ(resp) {
                $scope.EmpScatteredList = resp.data;
            });

            nw.style.display = "none";
            sw.style.display = "block";
        }
        else {
            nw.style.display = "block";
            sw.style.display = "none";
        }
        

    }

    $scope.ScWeekName = null;
    //Scattered Week Lists
    $scope.scatteredWeek = function () {
        angular.element(document.querySelector("#scatteredWeekModal")).modal("show");
    }
    // Double Clicking ScatterdWeek Off Modal
    $scope.doubleScatteredWeek = function (e) {
        if (e.data.Emps > $scope.EmpScatteredList[0].Emps) {
            ShowResult("Can only Select the minimum amount Employees Week Offs!!", 'failure');
            throw "Invalid Request!!";
        }
        else {
            $scope.WekId = e.data.WOHeaderId;
            $scope.ScWeekName = e.data.UserName;
            angular.element(document.querySelector("#scatteredWeekModal")).modal("hide");
        }
        
    }

    $scope.weekList = [];

    function getWeekOff() {
        $http({
            method: 'GET',
            url: $scope.path + "getWeekOffCbo"
        }).then(function succ(resp) {
            $scope.weekList = resp.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + "getBudgets"
        }).then(function succ(resp) {
            $scope.budgetsList = resp.data;
        });
    }
    getWeekOff();

    $scope.WekId = null;

    $scope.saveSingle = function () {

        if (angular.isUndefinedOrNull($scope.WekId) || angular.isUndefinedOrNull($scope.EffectiveDates) || angular.isUndefinedOrNull($scope.EmpSystemId)) {
            ShowResult("All Selections are Mandatory!!", 'failure');
            throw ("Invalid Request");
        }

        $http({
            method: 'POST',
            url: url + 'SaveSingle',
            data: { 'EmpId': $scope.EmpSystemId, 'EffectiveDate': $scope.EffectiveDates, 'WeekId': $scope.WekId }
           
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {

                    $http({
                        method: 'POST',
                        url: $scope.path + "getEmpWeekOff",
                        data: { 'EmpId': $scope.EmpSystemId }
                    }).then(function succ(resp) {
                        if (resp.data.length > 0) {
                            $scope.WekName = resp.data[0].UserName;
                            $scope.WekId = resp.data[0].WOHeaderId;
                            $scope.EffectiveDates = resp.data[0].EffectiveDate;
                            $scope.ScWeekName = resp.data[0].UserName;
                            $scope.EmpGridList = resp.data;
                            if (sw.style.display == "block") {
                                $http({
                                    method: 'POST',
                                    url: $scope.path + "getWeekOffsLists",
                                    data: { 'EmpID': $scope.EmpSystemId }
                                }).then(function succ(resp) {
                                    $scope.EmpScatteredList = resp.data;
                                });
                            }
                        }
                        else {
                            $scope.WekName = null;
                            $scope.WekId = null;
                            $scope.EffectiveDates = null;
                            $scope.EmpGridList = resp.data;
                        }

                    });
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        });
    }

    $scope.clearSingle = function () {
        $scope.employee = null;
        $scope.EmpSystemId = null;
        $scope.EffectiveDates = new Date();
        $scope.WekId = null;
        $scope.EmpScatteredList = [];
        $scope.ScWeekName = null;
        nw.style.display = "block";
        sw.style.display = "none";
    }

    // Tab Attendance Process Code

    $scope.EmployeeList = [];

    $scope.EmployeePopUp = function () {
        if ($scope.selectedValues.FromDate != null) {

            angular.element(document.querySelector("#EmployeePop")).modal("show");
            //$scope.getEmpDetailsData();
        }
        else {
            ShowResult("Please Select Effective Date", 'failure');
        }
    }
    $scope.getEmpDetailsData = function () {

        $http({
            method: 'POST',
            data: { EffectiveDate: $scope.selectedValues.FromDate },
            url: $scope.path + 'getDistinctEmployeesToBeProcessed'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;

        });
    }

    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.EmpSelectedData = [];
    $scope.SelectEmPDetails = function () {
        $scope.EmpSelectedData = [];
        for (var j = 0; j < $scope.EmployeeList.length; j++) {
            if ($scope.EmployeeList[j].isSelected == true) {

                $scope.EmpSelectedData.push($scope.EmployeeList[j]);
                $scope.EmployeeList[j].isSelected = true;
            }
            else {
                $scope.EmployeeList[j].isSelected = false;
            }
        }
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

   

    $scope.ProcessAttendance = function () {
        if ($scope.selectedValues.FromDate != null && $scope.EmpSelectedData != null) {
            var EmpString = "''";

            for (var j = 0; j < $scope.EmpSelectedData.length; j++) {
             
                EmpString+= ",'" + $scope.EmpSelectedData[j].EmpSystemId + "'";

            }
            $http({
                method: 'POST',
                data: { EffectiveDate: $scope.selectedValues.FromDate, EmpData: EmpString },
                url: $scope.path + 'ProcessAttendance'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {

                    ShowResult("Saved Successfully ...", 'success');
                }
            });
        }
        else {
            ShowResult("Please Select Prerequisite Data", 'failure');
        }
    }


    $scope.selectedValues = {
        FromDate: null
    };

    //  #region  Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetWeekOffSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.UploadedData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage2").change(function () {
        $scope.picdata = this.files[0];
    });


    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportWeekOffData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.UploadedData = [];
                        $scope.UploadedData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.SaveUploadedData = function () {
        try {
            for (var i = 0; i < $scope.UploadedData.length; i++) {
                if (baseService.isUndefinedOrNull($scope.UploadedData[i].EmpSystemId)) {
                    throw "ServiceMasterId is required.";
                }
                if (baseService.isUndefinedOrNull($scope.UploadedData[i].WOHeaderId)) {
                    throw "TaxCodeId is required.";
                }
                $scope.UploadedData[i].Id = null;
                $scope.UploadedData[i].Active = true;
            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveUploadedWeekOffData',
                data: {
                    'data': $scope.UploadedData
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UploadedData = [];
                    $("#uploadImage2").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };
    //  #endregion Data Upload Download TDS

   
}