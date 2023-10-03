'use strict';
ProductionOrderEntitySetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
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
        IsApplicable: false,
        DrGLGeneralInfoId: null, DrBudgetMasterId: null, DrActivityId: null, CrGLGeneralInfoId: null, CrBudgetMasterId: null, CrActivityId: null, DrControlId: null, CrControlId: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
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
            IsApplicable: false,
            DrGLGeneralInfoId: null, DrBudgetMasterId: null, DrActivityId: null, CrGLGeneralInfoId: null, CrBudgetMasterId: null, CrActivityId: null, DrControlId: null, CrControlId: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
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

    $scope.Delete= function () {
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



    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GLType = null;
    $scope.GetCOAICodeList = function (name) {
        $scope.GLType = name;
        $scope.GLUrl1 = "accounts/glitem/GetBankGLAccountCode?companyId=" + $scope.ProductionOrderEntitySetupNew.CompanyId;
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };
    $scope.set = function () {
        if ($scope.selectedCode !== null) {
            $scope.selectedCode = null;
        }
    };

    $scope.rowSelected = null;
    $scope.setSelected = function (x) {
        if ($scope.GLType == 'Dr') {
            $scope.rowSelected = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.DrCOAICode = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.DrGLGeneralInfoId = x.GLGeneralInfoId;
            $scope.set();
            $scope.selectedCode = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.DrGLGeneralInfoName = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
            getDrBudget();
        }
        else {
            $scope.rowSelected = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.CrCOAICode = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.CrGLGeneralInfoId = x.GLGeneralInfoId;
            $scope.set();
            $scope.selectedCode = x.GLGeneralInfoCode;
            $scope.ProductionOrderEntitySetupNew.CrGLGeneralInfoName = x.GLGeneralInfoCode + " - " + x.GLGeneralInfoName;
            getCrBudget();
        }
    };

    $scope.clearGLData = function () {
        $scope.ProductionOrderEntitySetupNew.GLGeneralInfoName = null;
    };

    $scope.drbudgetList = [];
    function getDrBudget() {
        cboService.getBudgetMasterCboByCompanyAndGLId($scope.ProductionOrderEntitySetupNew.CompanyId, $scope.ProductionOrderEntitySetupNew.DrGLGeneralInfoId, function (result) {
            $scope.drbudgetList = result;
        });
    }

    $scope.crbudgetList = [];
    function getCrBudget() {
        cboService.getBudgetMasterCboByCompanyAndGLId($scope.ProductionOrderEntitySetupNew.CompanyId, $scope.ProductionOrderEntitySetupNew.CrGLGeneralInfoId, function (result) {
            $scope.crbudgetList = result;
        });
    }

    $scope.dractivityList = [];
    $scope.getDrActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.ProductionOrderEntitySetupNew.DrBudgetMasterId, function (result) {
            $scope.dractivityList = result;
        });
    };

    $scope.cractivityList = [];
    $scope.getCrActivity = function () {
        cboService.getBudgetMasterActivityCbo($scope.ProductionOrderEntitySetupNew.CrBudgetMasterId, function (result) {
            $scope.cractivityList = result;
        });
    };

}