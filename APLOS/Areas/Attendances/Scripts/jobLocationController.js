'use strict';
jobLocationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function jobLocationController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Job Location';
    $scope.Action = 'Save';
    $scope.path = 'Attendances/JobLocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.JobLocationModel = {
        SystemID: null,
        JobLocation: null,
        GroupID: null,
        PlantID: null,
        CompanyId: null,
    };

    $scope.JobLocationList = [];
    $scope.getListData = function () {
        $http.get('Attendances/JobLocation/getJobLocationlist?plantId=' + $scope.JobLocationModel.PlantID)
            .then(
                function successCallback(response) {
                    $scope.JobLocationList = [];
                    if (baseService.arrayLength(response.data) > 0) {
                        if (!baseService.isUndefinedOrNull(response.data)) {
                            $scope.JobLocationList = response.data;
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    //$scope.getListData();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridJobLocation").data("ejGrid");
        $scope.JobLocationModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
        } catch (e) {
        }
        $scope.getListData();
    };

    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'JobLocation': $scope.JobLocationModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                    
                    ClearFields();
                    $scope.getListData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.JobLocationModel.SystemID)) {
            $http.get('Attendances/JobLocation/Delete?SystemID=' + $scope.JobLocationModel.SystemID)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getListData();                        
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.JobLocationModel = {
            SystemID: null,
            JobLocation: null,
            GroupID: null,
            PlantID: $scope.JobLocationModel.PlantID,
            CompanyId: $scope.JobLocationModel.CompanyId,
        };
        $scope.Action = 'Save';

        
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationMaster() {
        try {
            CheckField("Job Location", $scope.JobLocationModel.JobLocation);         
        } catch (ex) {
            throw ex;
        }
    }

    // Plant Load
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.JobLocationModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }


}