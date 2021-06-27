'use strict';
individualFixedOTController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function individualFixedOTController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Off Duty Hours';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Attendances/InvididualFixedOT/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.IndividualFixedOT = {
        Id: null,
        YearNo: null,
        MonthNo: null
    }

    $scope.yearlist = [];
    $scope.GetCbo = function () {
        $http.get('Attendances/AttendanceProcessUI/GetCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.yearlist = [];
                        $scope.yearlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetCbo();

    $scope.dataList = [];
    $scope.GetEmployeeDeleteInfo = function () {
        try {
            if ($scope.IndividualFixedOT.YearNo == null) {
                throw ("Year No is required.");
            }

            if (baseService.isUndefinedOrNull($scope.IndividualFixedOT.MonthNo)) {
                throw ("Month No is required.");
            }
            $scope.employeeInfo = {};
            $scope.dataList = [];
            $http({
                method: 'GET',
                url: 'employees/EmployeeDelete/getFixedOTemployee?YearNo=' + $scope.IndividualFixedOT.YearNo + '&MonthNo=' + $scope.IndividualFixedOT.MonthNo
            }).then(function successCallback(response) {
                $scope.dataList = response.data;
            });
            angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.employeeInfo = {};
    $scope.SetData = function (obj) {
        var emp = obj.data;
        $scope.employeeInfo.EmpSystemID = emp.SystemID;
        $scope.employeeInfo.EmpPic = virtualPath.EmployeePic + emp.EmpPicPath;
        $scope.employeeInfo.EmployeeCode = emp.EmployeeCode;
        $scope.employeeInfo.EmployeeName = emp.EmployeeName;
        $scope.employeeInfo.DOJ = emp.DOJ;
        $scope.employeeInfo.DOC = emp.DOC;
        $scope.employeeInfo.EmailId = emp.EmailId;
        $scope.employeeInfo.Code = emp.Code;
        $scope.employeeInfo.Section = emp.Section;
        $scope.employeeInfo.SubSection = emp.SubSection;
        $scope.employeeInfo.MinimumOT = emp.MinimumOT;
        $scope.employeeInfo.Department = emp.Department;
        $scope.employeeInfo.LegalDesignation = emp.LegalDesignation;
        $scope.employeeInfo.Allow = emp.Allow;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.GetShiftList = {};
        $scope.GetPreData($scope.employeeInfo.EmpSystemID);

    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.recorddoubleclick = function (args) {
        $scope.GetPreData();
        $scope.Action = 'Update';
    };

    $scope.TotalOT = null;
    $scope.GetIndividualFixedOTList = [];
    $scope.GetPreData = function (empId) {
        $scope.GetIndividualFixedOTList = [];
        $http.get('Attendances/InvididualFixedOT/GetIndividualFixedOT?empId=' + empId + '&MonthNo=' + $scope.IndividualFixedOT.MonthNo + '&YearNo=' + $scope.IndividualFixedOT.YearNo)
            .then(function (response) {
                $scope.GetIndividualFixedOTList = response.data;
                var total = 0; var Previoustotal = 0;

                for (var i = 0; i < $scope.GetIndividualFixedOTList.length; i++) {
                    Previoustotal += $scope.GetIndividualFixedOTList[i].OTHr;
                    $scope.PreTotal = Previoustotal;
                }

                for (var i = 0; i < $scope.GetIndividualFixedOTList.length; i++) {
                    total += $scope.GetIndividualFixedOTList[i].OTHr;
                }

                $scope.TotalOT = total;
                $scope.RemainingOT = $scope.employeeInfo.MinimumOT - $scope.TotalOT;
            });
    };

    $scope.RemainingOT = null
    $scope.ChangeOutTime = function () {
        var total = 0;
        $scope.TotalOT = 0;
        for (var i = 0; i < $scope.GetIndividualFixedOTList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.GetIndividualFixedOTList[i].OTHrNew)) {
                total += parseFloat($scope.GetIndividualFixedOTList[i].OTHrNew);
                $scope.TotalOT = total;
                $scope.RemainingOT = $scope.employeeInfo.MinimumOT - $scope.TotalOT;
            }
        }
    };

    $scope.Save = function () {
        try {

            if ($scope.employeeInfo.Allow == 'NO') {
                if ($scope.TotalOT > $scope.employeeInfo.MinimumOT) {
                    throw 'Total OT cannot be bigger then Minimum OT hour...';
                }
            }

            var IndividualFixedOTListNew = [];
            for (var i = 0; i < $scope.GetIndividualFixedOTList.length; i++) {
                if ($scope.GetIndividualFixedOTList[i].CheckBoxSelect == true) {
                    if (baseService.isUndefinedOrNull($scope.GetIndividualFixedOTList[i].OTHrNew)) {
                        throw 'Please Select OT Min';
                    }
                    else {
                        IndividualFixedOTListNew.push($scope.GetIndividualFixedOTList[i]);
                    }
                }
            }
            if (IndividualFixedOTListNew.length == 0) {
                throw 'Please Check..';
            }

            $scope.$broadcast('show-errors-check-validity');
            if ($scope.IndividualFixedOTForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'IndividualFixedOT': IndividualFixedOTListNew },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            var gridObj = $("#GridIndividualFixedOT").data("ejGrid");
                            gridObj.refreshContent(true);

                            $scope.GetPreData($scope.employeeInfo.EmpSystemID);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }

            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {

        $scope.Action = 'Save';
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.IndividualFixedOT.Id)) {
            $http.get('Leave/OffDutyHours/Delete?Id=' + $scope.IndividualFixedOT.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.Action = 'Save';
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

}