'use strict';
jwActivityController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$controller'];
function jwActivityController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter,$controller) {
    $rootScope.title = 'Job Work Activity';
    $scope.ModelList = [];
    $scope.path = 'Outsourcing/JWActivity/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.jobWorkTypeList = [];
    cboService.getEnumCbo("enum/GetJobWorkTypeEnumCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });

    $scope.getEmp = function (obj) {
        $scope.ModelNew.ResponsiblePersonId = obj.data.SystemId;
        $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeName;
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
    };

    $scope.employeeList = [];
    $scope.showAllEmployeeListPopUp = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Outsourcing/JWItem/EmployeeListAll'
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.employeeList = response.data;
                angular.element(document.querySelector('#responsiblePersonPopUp')).modal('show');
            }
            else {
                ShowResult("No Data Found", 'failure');
            }
        });
    };
    $scope.serviceCboList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceCboList = response.data;
        });
    $scope.productionPrcoessList = [];
    $http.get('Outsourcing/JWActivity/GetProductionProcessList')
        .then(function (response) {
            $scope.productionPrcoessList = response.data;
        });

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };
    $scope.getData();

    //#region Partial View
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //#endregion

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Type: null,
        ResponsiblePersonId: null,
        Remarks: null,
        ProcessId: null,
        ServiceId: null,
        ResponsiblePersonName :null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

$scope.selectResponsiblePersonPopUp = function (index, id) {
        $scope.updateResponsiblePersonIndex = index;
        $scope.selectedResponsiblePerson = id;
    };
$scope.updateResponsiblePersonIndex = -1;
 $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.updateResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.updateResponsiblePersonIndex];
            $scope.ModelNew.ResponsiblePersonName = employee.EmployeeName;
            $scope.ModelNew.ResponsiblePersonId = employee.SystemId;
        }
      angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
   $scope.clearResponsiblePerson = function()
{
        $scope.ModelNew.ResponsiblePersonName = null;
            $scope.ModelNew.ResponsiblePersonId = null;
}

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
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