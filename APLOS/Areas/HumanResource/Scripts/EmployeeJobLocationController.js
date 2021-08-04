'use strict';
EmployeeJobLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function EmployeeJobLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Employee Job Location';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'humanresource/EmployeeJobLocation/';
   
    $scope.modal = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        DOJ: null,
        DOC: null,
        DesignationGroup:null,
        LegalDesignation: null,
        SystemID: null,
        JobLcSystemID: null,
        EffectiveDate:null
    }
    $scope.modalNew = Object.assign({}, $scope.modal);



    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'employees/leaveApplication/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;
        $scope.modalNew.EmployeeCode = data.EmployeeCode;
        $scope.modalNew.EmpSystemID = data.SystemID;
        $scope.modalNew.EmployeeName = data.EmployeeName;
        $scope.modalNew.DOJ = data.DOJ;
       
        $scope.modalNew.DesignationGroup = data.DesignationGroup;
        $scope.modalNew.LegalDesignation = data.LegalDesignation;
        $scope.modalNew.Department = data.Department;
       
        $scope.modalNew.JobLcSystemID = data.JobLcSystemID;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;

        if (baseService.isUndefinedOrNull(data.EffectiveDate))
            $scope.modalNew.EffectiveDate = data.DOJ;
        else
            $scope.modalNew.EffectiveDate = data.EffectiveDate;

        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };


    $scope.JobLocationList = [];
    $scope.LoadAllJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.Flag = "Load All";
        $scope.PlantId = null;
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
            });

        $scope.Flag = "Load Less";
    };

    $scope.Flag = "Load Less";
    $scope.LoadPlantJobLocation = function () {
        $scope.JobLocationList = [];
        $scope.PlantId = null;
        $scope.Flag = "Load Less";
        $http.get('employees/EmployeeInformation/GetJobLocationCbo?flag=' + $scope.Flag)
            .then(function (response) {
                $scope.JobLocationList = response.data;
            });
        $scope.Flag = "Load All";
    };
    $scope.LoadPlantJobLocation();

    $scope.fixedShitList = [];
    $scope.PlantId = null;
    $scope.GetShiftCbo = function () {
        $scope.fixedShitList = [];
        $scope.PlantId = $.grep($scope.JobLocationList, function (item) {
            return item.SystemID === $scope.employeeNew.JobLocationID;
        })[0].PlantID;

        $http.get('employees/EmployeeInformation/GetCboShiftDefinationByPlant?plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.fixedShitList = response.data;
            });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.EmployeeJobLocationForm.$valid) {
            $http({
                method: 'POST',
                url: 'HumanResource/EmployeeJobLocation/Create',
                data: { 'data': $scope.modalNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Clear = function () {
        $scope.modal = {
            EmployeeCode: null,
            EmpSystemID: null,
            EmployeeName: null,
            DOJ: null,
            DOC: null,
            DesignationGroup: null,
            LegalDesignation: null,
            SystemID: null,
            JobLcSystemID: null,
            EffectiveDate: null
        }
        $scope.modalNew = Object.assign({}, $scope.modal);

        $scope.imageSrc = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }

}