'use strict';
WeekOffUpdatesController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WeekOffUpdatesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Week Off Updates';
    $rootScope.title1 = 'Week Off Updates';
    $scope.Action = 'Save';
    var url = "humanresource/WeekOffUpdates/";
    $scope.path = "humanresource/WeekOffUpdates/";


    // Code For the Second Tab

    var x = document.getElementById("RosterBudgetGrid");
    var y = document.getElementById("SavedTable");

    x.style.display = "none";
    y.style.display = "none";

    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";


        try {
            window.open('humanresource/WeekOffUpdates/GetSampleReport?reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        $http({
            method: 'GET',
            url: url + 'getCurrentList'
        }).then(function success(response) {
            $scope.currentList = [];
            $scope.currentList = response.data;
            x.style.display = "none";
            y.style.display = "block";
        })
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
            if ($scope.fileData.length == 0 ) {
                
                throw ("Please Select A File!!");
            }
           

            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

                $http({
                    method: 'POST',
                    url: url + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        fileData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                            fileData.append('file', data.file);
                           
                        }
                        return fileData;
                    },
                    data: { 'modelNew': $scope.ModelNew,  'file': $scope.fileData }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");

                    }

                    else {
                        try {
                            $scope.ExcelUploadData = response.data;
                            x.style.display = "block";
                            y.style.display = "none";
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

        
        

        $http({
            method: 'POST',
            url: url + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData}
        }).then(function successCallback(response) {
        if (response.data.Error === true) {
            ShowResult(response.data.Message, "failure");
        }
        else {
            try {
                if ($rootScope.isCollapsed == true) {
                    $rootScope.toggle();
                }
                $scope.getCurrentFileList();
                ShowResult(response.data.Message, 'success')
            }
            catch (e) {

                ShowResult(e, "failure");
            }
        }
    });
    }

    // ***** Code For the Fist Tab

    $scope.employee = null;
    $scope.EmpSystemId = null;
    $scope.EffectiveDate = new Date();

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

    $scope.doubleEmployee = function (e) {
        $scope.employee = e.data.EmployeeName;
        $scope.EmpSystemId = e.data.SystemId;

        angular.element(document.querySelector('#employeesModal')).modal('hide');

        $http({
            method: 'POST',
            url: $scope.path + "getEmpWeekOff",
            data: {'EmpId':$scope.EmpSystemId}
        }).then(function succ(resp) {
            if (resp.data.length > 0) {
                $scope.WekName = resp.data[0].UserName;
                $scope.WekId = resp.data[0].WOHeaderId;
                $scope.EffectiveDate = resp.data[0].EffectiveDate;
            }
            else {
                $scope.WekName =null;
                $scope.WekId = null;
                $scope.EffectiveDate = null;
            }
            
        });

    }

    $scope.weekList = [];

    function getWeekOff() {
        $http({
            method: 'GET',
            url: $scope.path + "getWeekOff"
        }).then(function succ(resp) {
            $scope.weekList = resp.data;
        })
    }
    getWeekOff();

    $scope.WekId = null;

    $scope.saveSingle = function () {

        if (angular.isUndefinedOrNull($scope.WekId) || angular.isUndefinedOrNull($scope.EffectiveDate) || angular.isUndefinedOrNull($scope.EmpSystemId)) {
            ShowResult("All Selections are Mandatory!!", 'failure');
            throw ("Invalid Request");
        }

        $http({
            method: 'POST',
            url: url + 'SaveSingle',
            data: { 'EmpId': $scope.EmpSystemId, 'EffectiveDate': $scope.EffectiveDate, 'WeekId': $scope.WekId }
           
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
                            $scope.EffectiveDate = resp.data[0].EffectiveDate;
                        }
                        else {
                            $scope.WekName = null;
                            $scope.WekId = null;
                            $scope.EffectiveDate = null;
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
        $scope.EffectiveDate = new Date();
        $scope.WekId = null;
    }

}