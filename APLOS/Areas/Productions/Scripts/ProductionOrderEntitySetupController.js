'use strict';
ProductionOrderEntitySetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','cboService'];
function ProductionOrderEntitySetupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Production Order Entity Setup";
    $scope.Action = 'Save';
    $scope.path = 'Productions/ProductionOrderEntitySetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.ProductionOrderEntitySetup = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        ProductionEntityId: null,
        FromEntityId: null,
        Type: null,
        OrderType: null,
        IsApplicable: false
    };
    $scope.ProductionOrderEntitySetupNew = Object.assign({}, $scope.ProductionOrderEntitySetup);

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.ProductionOrderEntitySetupNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };



    $scope.entityList = [];
    $scope.getEntity = function () {
        $scope.entities = [];
        $scope.entityValue = [];
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + $scope.ProductionOrderEntitySetupNew.PlantId
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    };

    $scope.typeList = [
        { Value: "Process", Text: "Process" },
        { Value: "MasterOrderEntity", Text: "MasterOrderEntity" }
    ];

    $scope.OrderTypeList = [
        { Value: "Manufacture", Text: "Manufacture" },
        { Value: "JobWork", Text: "Job Work" },
        { Value: "OutSource", Text: "Out Source" },
        { Value: "Other", Text: "Other" }
    ];
    $scope.searchBy = "PlantId"; $scope.search = "";
    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetList?column=' + $scope.searchBy + '&value=' + $scope.search + '&CompanyId=' + $scope.ProductionOrderEntitySetupNew.CompanyId + '&PlantId=' + $scope.ProductionOrderEntitySetupNew.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
           
        });
    }

    $scope.Get = function (args) {
        $scope.ProductionOrderEntitySetupNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProductionOrderEntitySetupForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ProductionOrderEntitySetupNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ProductionOrderEntitySetupNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ProductionOrderEntitySetupNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.PlantId = $scope.ProductionOrderEntitySetupNew.PlantId;
        $scope.CompanyId = $scope.ProductionOrderEntitySetupNew.CompanyId;
        $scope.ProductionOrderEntitySetup = {
            Id: null,
            CompanyId: $scope.CompanyId,
            PlantId: $scope.PlantId,
            ProductionEntityId: null,
            FromEntityId: null,
            Type: null,
            OrderType: null,
            IsApplicable: false
        };
        $scope.ProductionOrderEntitySetupNew = Object.assign({}, $scope.ProductionOrderEntitySetup);
    }

    $scope.message_detailconfirmation = null;
    $scope.removeDetail = function (obj) {
        $scope.modelNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.modelNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.modelNew.Id + ' ]';
        angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
    }

    $scope.DeleteBomDetail = function () {
        $http({
            method: 'POST',
            url: 'Productions/ProductionOrderEntitySetup/Delete?id=' + $scope.modelNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };






}