'use strict';
InvoiceBudgetChargesSettingController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function InvoiceBudgetChargesSettingController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Invoice Budget Charges Setting';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Commercial/InvoiceBudgetChargesSetting/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "Process"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Process', name: "Process" }, { value: 'ExpenseType', name: "ExpenseType" }
        , { value: 'DrBudgetCode', name: "DrBudgetCode" }, { value: 'CrBudgetCode', name: "CrBudgetCode" }, { value: 'Days', name: "Days" }
        , { value: 'PaymentTerms', name: "PaymentTerms" }];

    $scope.getData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetList?companyId=" + $scope.ModelNew.CompanyId,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };


    $scope.ModelTemp = {
        Id: null,
        CompanyId: null,
        Process: null,
        Type: null,
        ExpenseType: null,
        DrGL: null,
        DrActivityId: null,
        DrBudgetCode: null,
        CrGL: null,
        CrActivityId: null,
        CrBudgetCode: null,
        Days: 0,
        PaymentTerms: 0,
        EstimatedPercentageValue: 0,
        EstimatedMaxValue: 0,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null,

        DrGLGeneralInfoName: null,
        DrGLGeneralInfoCode: null,
        DrGLGeneralInfoId: null,
        DrActivityName: null,
        DrActivityCode: null,
        DrActivityId: null,
        DrBudgetName: null,
        DrBudgetCode: null,
        DrBudgetMasterId: null,

        CrGLGeneralInfoName: null,
        CrGLGeneralInfoCode: null,
        CrGLGeneralInfoId: null,
        CrActivityName: null,
        CrActivityCode: null,
        CrActivityId: null,
        CrBudgetName: null,
        CrBudgetCode: null,
        CrBudgetMasterId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup($window.companyGroupId, function (result) {
        $scope.companyList = result;
    });

    $scope.overHeadTypeList = [];
    $http({
        method: 'GET',
        url: 'Commercial/OverHeadType/GetCbo'
    }).then(function successCallback(response) {
        $scope.overHeadTypeList = response.data;
    });

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
                data: { 'entity': $scope.ModelNew },
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
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.CompanyId = $scope.ModelNew.CompanyId;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.CompanyId = $scope.CompanyId;
    }


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
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.flag = null;
    $scope.GetCOAICodeList = function (flag) {
        $scope.flag = flag;
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityByCompnay?companyId=" + $scope.ModelNew.CompanyId;
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

    $scope.setSelected = function (data) {
        $scope.addRow(data, $scope.flag);
    };

    $scope.addRow = function (data, flag) {
        if (flag === 'Dr') {
            $scope.ModelNew.DrBudgetMasterId = data.BudgetMasterId;
            $scope.ModelNew.DrBudgetCode = data.BudgetCode;
            $scope.ModelNew.DrBudgetName = data.BudgetName;
            $scope.ModelNew.DrActivityId = data.ActivityId;
            $scope.ModelNew.DrActivityCode = data.ActivityCode;
            $scope.ModelNew.DrActivityName = data.ActivityName;

            $scope.ModelNew.DrGLGeneralInfoId = data.GLGeneralInfoId;
            $scope.ModelNew.DrGLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.ModelNew.DrGLGeneralInfoName = data.GLGeneralInfoName;

            $scope.closeCOAICodeListPopUp();
        } else {
            $scope.ModelNew.CrBudgetMasterId = data.BudgetMasterId;
            $scope.ModelNew.CrBudgetCode = data.BudgetCode;
            $scope.ModelNew.CrBudgetName = data.BudgetName;
            $scope.ModelNew.CrActivityId = data.ActivityId;
            $scope.ModelNew.CrActivityCode = data.ActivityCode;
            $scope.ModelNew.CrActivityName = data.ActivityName;

            $scope.ModelNew.CrGLGeneralInfoId = data.GLGeneralInfoId;
            $scope.ModelNew.CrGLGeneralInfoCode = data.GLGeneralInfoCode;
            $scope.ModelNew.CrGLGeneralInfoName = data.GLGeneralInfoName;

            $scope.closeCOAICodeListPopUp();
        }
    };

}