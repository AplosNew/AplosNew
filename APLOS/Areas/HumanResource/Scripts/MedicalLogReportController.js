'use strict';
MedicalLogReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$interval'];
function MedicalLogReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $interval) {
    $rootScope.title = 'Medical Log Report';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicalLogReport/';
    $scope.EmployeeUrl = 'HumanResource/MedicalLog/getEmployee';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    $scope.openEmpPopUp = function () {
        angular.element(document.querySelector('#empPopUpId')).modal('show');
        $scope.GetMedicalLogEmployee();
    }

    $scope.closeEmpPopUp = function () {

        angular.element(document.querySelector('#empPopUpId')).modal('hide');

    }
    // #region Medince Stock 
    $scope.ModelTempT = {
        MedicineMasterId: null,
        UserName: null,
        To:null
    };
    $scope.ModalNewT = Object.assign({}, $scope.ModelTempT);
    $scope.MedicineList = [];
    $scope.openMedicinePopUp = function () {
        angular.element(document.querySelector('#medicinePopUp')).modal('show');
        $http.get('HumanResource/MedicalLogReport/GetMedicinePopUp')
            .then(
                function successCallback(response) {
                    
                    $scope.MedicineList = response.data;
                    
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.doubleMedcine = function (e) {
        $scope.ModalNewT.MedicineMasterId = e.data.MedicineMasterId;
        $scope.ModalNewT.UserName = e.data.UserName;
        $scope.closeMedicinePopUp();
    }
    $scope.closeMedicinePopUp = function () {

        angular.element(document.querySelector('#medicinePopUp')).modal('hide');

    }
    $scope.MedicineStockList = [];
    $scope.GetMedinceStockGrid = function () {
        $http.get('HumanResource/MedicalLogReport/GetMedinceStockGrid?medicineId=' + $scope.ModalNewT.MedicineMasterId + '&to=' + $scope.ModalNewT.To)
            .then(
                function successCallback(response) {

                    $scope.MedicineStockList = response.data;

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    // #endregion Medince Stock 

    // Medical Log Employee
    $scope.MedicalLogEmp = []
    $scope.GetMedicalLogEmployee = function () {
        $http.get('HumanResource/MedicalLogReport/GetMedicalLogEmployee')
            .then(
                function successCallback(response) {

                    $scope.MedicalLogEmp = response.data;

                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }

    $scope.ModelTemp = {
        FromDate: null,
        ToDate: null,
        EmployeeSysId:null
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);

    $scope.MedicalLogGridList = [];
    $scope.medicallogGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'medicallogGridView',
            data: {
                'from': $scope.ModalNew.FromDate,
                'to': $scope.ModalNew.ToDate,
                'empSystemId': $scope.ModalNew.EmployeeSysId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicalLogGridList = response.data;
        })
    }

    // #region Get All Employee and select by double click
    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.EmployeeUrl,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }
    $scope.getEmployee();

    $scope.doubleEmployee = function (e) {
        $scope.ModalNew.EmployeeSysId = e.data.SystemId;
        $scope.ModalNew.EmployeeName = e.data.EmployeeName;
        $scope.closeEmpPopUp();

    }
    // #endregion Get All Employee and select by double click

    $scope.XlsMedicalLogReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsMedicalLogReport",
            data: {
                'from': $scope.ModalNew.FromDate,
                'to': $scope.ModalNew.ToDate,
                'empSystemId': $scope.ModalNew.EmployeeSysId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.XlsGetMedinceStockReport = function () {
        
        $http.get('HumanResource/MedicalLogReport/XlsGetMedinceStockReport?medicineId=' + $scope.ModalNewT.Id + '&to=' + $scope.ModalNewT.To)
        .then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {

                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };
}