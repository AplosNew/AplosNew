'use strict';
IndividualGratuityPolicyController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function IndividualGratuityPolicyController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Individual Gratuity Policy';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/IndividualGratuityPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    //#region employee Load
    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Payrolls/IndividualGratuityPolicy/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
            for (var l = 0; l < $scope.employee.length; l++) {

                var st = new Date($scope.employee[l].DOJ);
                var ed = new Date();

                var nowyear = ed.getFullYear();
                var nowmonth = ed.getMonth() + 1;
                var nowday = ed.getDate();

                var styear = st.getFullYear();
                var stmonth = st.getMonth() + 1;
                var stday = st.getDate();

                var age = nowyear - styear;
                var age_month = nowmonth - stmonth;
                var age_day = nowday - stday;

                if (age_month < 0 || age_month === 0 && age_day < 0) {
                    age = parseInt(age) - 1;
                    age_month += 12;
                }
                if (age_month === 12) {
                    age_month = 0;
                    age = age + 1;
                }
                $scope.employee[l].TYear = age;
                $scope.employee[l].TMonth = age_month;
            }
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.leaveApplication = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        LegalDesignation: null,
        DOJ: null,
        DOC: null,
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);


    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;
        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.getData($scope.leaveApplicationNew.EmpSystemID);
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }

    function checkExistCustomer(list, SystemID) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeSystemId == SystemID) {
                return true;
            }
        }
        return false;
    }

    $scope.EmployeeListTemp = [];
    $scope.saveemployeedata = function () {

        var row = $filter('filter')($scope.employee, { 'CheckBoxSelect': true });

        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            for (var i = 0; i < row.length; i++) {
                var obj = {};
                //try {
                    if (checkExistCustomer($scope.EmployeeListTemp, row[i].SystemID) == false) {
                        obj.Id = null;
                        obj.EmployeeSystemId = row[i].SystemID;
                        obj.EmployeeName = row[i].EmployeeName;
                        obj.DOB = row[i].DOB;
                        obj.DOJ = row[i].DOJ;
                        obj.Designation = row[i].Designation;
                        obj.EmployeeCode = row[i].EmployeeCode;
                        obj.FatherName = row[i].FatherName;

                        $scope.EmployeeListTemp.push(obj);

                        for (var l = 0; l < $scope.EmployeeListTemp.length; l++) {

                            var st = new Date($scope.EmployeeListTemp[l].DOJ);
                            var ed = new Date();

                            var nowyear = ed.getFullYear();
                            var nowmonth = ed.getMonth() + 1;
                            var nowday = ed.getDate();

                            var styear = st.getFullYear();
                            var stmonth = st.getMonth() + 1;
                            var stday = st.getDate();

                            var age = nowyear - styear;
                            var age_month = nowmonth - stmonth;
                            var age_day = nowday - stday;

                            if (age_month < 0 || age_month === 0 && age_day < 0) {
                                age = parseInt(age) - 1;
                                age_month += 12;
                            }
                            if (age_month === 12) {
                                age_month = 0;
                                age = age + 1;
                            }
                            $scope.EmployeeListTemp[l].TYear = age;
                            $scope.EmployeeListTemp[l].TMonth = age_month;
                        }
                    }
                //    else {
                //        throw "Individual Gratuity Policy is Already Defined for EmployeeCode " + row[i].EmployeeCode+"";
                //    }
                //} catch (e) {
                //    ShowResult(e, 'info');
                //}
            }
        }
        $scope.Back();
    };

    $scope.Back = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employee.length; i++) {
                $scope.employee[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };

    //#endregion 




    $scope.EmployeeListTemp = [];
    $scope.getMasterData = function () {
        $scope.EmpSytemIdList = [];
        for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
            if (baseService.arrayLength($scope.EmpSytemIdList) == 0) {
                $scope.EmpSytemIdList = "'" + $scope.EmployeeListTemp[i].SystemID + "'";
            }
            else {
                $scope.EmpSytemIdList += ",'" + $scope.EmployeeListTemp[i].SystemID + "'";
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { 'EmpSytemIDList': $scope.EmpSytemIdList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeListTemp = response.data;
            for (var i = 0; i < $scope.EmployeeListTemp.length; i++) {
                //$scope.EmployeeListTemp[i].DOJ = $scope.ProfessionalTaxOB.TaxYearId;
                var st = new Date($scope.EmployeeListTemp[i].DOJ);
                var ed = new Date();

                var nowyear = ed.getFullYear();
                var nowmonth = ed.getMonth() + 1;
                var nowday = ed.getDate();

                var styear = st.getFullYear();
                var stmonth = st.getMonth() + 1;
                var stday = st.getDate();

                var age = nowyear - styear;
                var age_month = nowmonth - stmonth;
                var age_day = nowday - stday;

                if (age_month < 0 || age_month === 0 && age_day < 0) {
                    age = parseInt(age) - 1;
                    age_month += 12;
                }
                if (age_month === 12) {
                    age_month = 0;
                    age = age + 1;
                }

                $scope.EmployeeListTemp[i].TYear = age;
                $scope.EmployeeListTemp[i].TMonth = age_month;

            }
        });
    }
    $scope.getMasterData();

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'EmpList': $scope.EmployeeListTemp },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }

    //#region Delete Master

    $scope.RemoveMaster = function (obj) {
        $scope.Id = obj.Id;
        if (!baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "Delete",
            data: { 'Id': $scope.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    //#endregion
    $scope.GratuityInsList = [];
    $scope.getData = function () {
        $scope.GratuityInsList = [];
        $http({
            method: 'GET',
            url: $scope.path + "GetGratuityIns",
        }).then(function successCallback(response) {
            $scope.GratuityInsList = response.data;
        });
    }
    $scope.getData();

    //#region Get GP data

    $scope.GPDetails = {
        IsRound: null,
        MaturityFromYear: null,
        MaturityToYear: null,
    }

    //$scope.GPList = [];
    $scope.getGP = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetGPDetails",
        }).then(function successCallback(response) {
            $scope.GPDetails = response.data[0];
        });
    }
    $scope.getGP();
    //#endregion

}