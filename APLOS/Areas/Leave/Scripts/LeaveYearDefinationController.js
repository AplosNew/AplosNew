'use strict';
LeaveYearDefinationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LeaveYearDefinationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Leave Year Defination';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Leave/LeaveYearDefination/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];


    $scope.companyList = [];
    $scope.getCompany = function () {
        cboService.getCompanyGroupCompanyCbo(null, function (result) {
            $scope.companyList = result;
        });
    };
    $scope.getCompany();

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.ModelNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        Sequence: 0,
        Code: null,
        FromDate: null,
        ToDate: null,
        ProcessingDate: null,
        ShortName: null,
        StandardName: null,
        RespersonId: null,
        responsiblePerson: null,
        UserName: null,
        Remarks: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();


    $scope.EmployeeList = [];
    $scope.getStartUp = function () {

        if (baseService.isUndefinedOrNull($scope.ModelNew.PlantId)) {

        }
        else {

            $http({
                method: 'POST',
                url: $scope.path + 'GetEmps?PlantId=' + $scope.ModelNew.PlantId,
            }).then(function succ(resp) {
                $scope.EmployeeList = resp.data;
            });
        }
    }
    


    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';

        $scope.getPlant();
        $scope.getStartUp();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    //Getting the Responsible Person
    $scope.selectResp = function () {
        angular.element(document.querySelector('#employeesModal')).modal('show');
    }

    $scope.doubleResp = function (e) {
        $scope.ModelNew.responsiblePerson = e.data.EmployeeName;
        $scope.ModelNew.RespersonId = e.data.SystemId;
        angular.element(document.querySelector('#employeesModal')).modal('hide');
    }


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}