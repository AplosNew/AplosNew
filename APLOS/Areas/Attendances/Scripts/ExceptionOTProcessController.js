'use strict';
ExceptionOTProcessController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader', '$filter'];
function ExceptionOTProcessController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader, $filter) {
    $scope.path = 'Attendances/ExceptionOTProcess/';
    $rootScope.title = 'Exception OT Process';

    //#region - - - S I N G L E D A T E T A B - - -

    $scope.empGridShow = function (args) {
        try {
            var FromDate = new Date($scope.FromDateSingleDate);
            var ToDate = new Date($scope.ToDate);
            if (FromDate > ToDate) {
                throw "FromDate Cannot be greater than ToDate";
            }
            else {
                ShowResult('Press the Go Button after Selecting Previous Date', 'success');
                $scope.empGrid = false;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.employeeAttendanceBySingleDate = [];
    $scope.employeeAttendanceBySingleDateSelection = [];
    $scope.allShiftSingleDay = [];
    $scope.selectSigleDate = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.FromDateSingleDate)) {
                throw "Select FromDate..";
            }
            if (baseService.isUndefinedOrNull($scope.ToDate)) {
                throw "Select ToDate..";
            }
            var FromDate = new Date($scope.FromDateSingleDate);
            var ToDate = new Date($scope.ToDate);
            var Today = new Date(Date.now());
            if (FromDate > Today) {
                $scope.FromDateSingleDate = null;
                $scope.employeeAttendanceBySingleDate = [];
                $scope.employeeAttendanceBySingleDateSelection = [];
                $scope.empGrid = false;
                throw "Select Past Date..";
            }
            if (ToDate > Today) {
                $scope.ToDate = null;
                $scope.employeeAttendanceBySingleDate = [];
                $scope.employeeAttendanceBySingleDateSelection = [];
                $scope.empGrid = false;
                throw "Select Past Date..";

            }
            $http({
                method: 'POST',
                url: $scope.path + "GetEmployee?FDate=" + $scope.FromDateSingleDate + '&TDate=' + $scope.ToDate,
            }).then(function successCallback(response) {
                $scope.employeeAttendanceBySingleDate = [];
                $scope.empGrid = true;
                for (var i = 0; i < response.data.length; i++) {
                    if (response.data[i].Id != null) {
                        $scope.employeeAttendanceBySingleDate.push(response.data[i]);
                    }
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.Save = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.employeeAttendanceBySingleDate.length; i++) {
            $scope.employeeAttendanceBySingleDate[i].ErrorMessage = "";
            try {
                DataToBeSaved.push($scope.employeeAttendanceBySingleDate[i]);
            } catch (e) {
            }
        }
        for (var i = 0; i < DataToBeSaved.length; i++) {
            DataToBeSaved[i].WorkDate = $scope.FromDateSingleDate;
        }
        $http({
            method: "POST",
            dataType: 'JSON',
            data: { 'data': DataToBeSaved, 'WorkDate': $scope.FromDateSingleDate, 'ToDate': $scope.ToDate },
            url: $scope.path + 'Save'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectSigleDate();
            }
        });
    }




    //#endregion

    //#region -- Single date filter --



    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEmployeeList?FDate=" + $scope.FromDateSingleDate + '&TDate=' + $scope.ToDate,
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }

    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#empfilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Back = function () {
        angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    };

    $scope.saveemployeedata = function () {
        $scope.employeeAttendanceBySingleDate = [];
        var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.employeeAttendanceBySingleDate = row;
            $scope.isManualFilter = true;
        }
        $scope.Back();
    };

    //$scope.EmployeeListF = {};
    //$scope.saveemployeedata = function () {
    //    $scope.employeeAttendanceBySingleDate = [];
    //    $scope.EmployeeListF = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
    //    var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
    //    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
    //        for (var i = 0; i < $scope.EmployeeListF.length; i++) {
    //            var FromDate = new Date($scope.FromDateSingleDate);
    //            var ToDate = new Date($scope.ToDate);
    //            while (FromDate <= ToDate) {
    //                $scope.EmployeeListF[i].push(FromDate);
    //                $scope.employeeAttendanceBySingleDate.push($scope.EmployeeListF[i]);
    //                FromDate = (new Date(attdnDate.setDate(FromDate.getDate() + 1)));
    //            }
    //        }
    //        $scope.employeeAttendanceBySingleDate = row;
    //        $scope.isManualFilter = true;
    //    }
    //    $scope.Back();
    //};

    $scope.message_detailconfirmation = null;
    $scope.removeDetail = function (obj) {

        $scope.MasterID = obj.data.Id;
        if (!baseService.isUndefinedOrNull($scope.MasterID))
            $scope.message_detailconfirmation = 'Are you sure You want to delete permanently ?';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
    }

    $scope.DeleteDetail = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Delete?id=' + $scope.MasterID
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.selectSigleDate();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };


    //#endregion

}





