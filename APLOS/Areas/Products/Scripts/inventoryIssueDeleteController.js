'use strict';
inventoryIssueDeleteController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function inventoryIssueDeleteController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Inventory Issue";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.products = [];
    $scope.path = 'Products/InventoryIssue/';
    $scope.getListUrl = $scope.path + 'GetDeletableIssueList';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.currentDate = new Date(Date.now());

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });

    $scope.searchByList = [
        {
            value: 'Id'
            , name: 'Issue No'
        },
        {
            value: 'MaterialStorage'
            , name: 'Storage Location'
        },
        {
            value: 'IssueDate'
            , name: 'Issue Date'
        }
    ];
    baseService.init($scope.getListUrl, null, null, 'DESC', 'Id', 'Id');
    $scope.getData = function (pageno) {
        //debugger;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.issueList = [];
                $scope.issueList = result.Rows;

            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $http({
        method: 'GET',
        url: 'Materials/MaterialStorage/getcbo'
    }).then(function (response) {
        $scope.storageList = response.data;
    });
    $scope.product = {
        Id: null
        , ComapnyGroupId: null
        , CompanyId: null
        , PlantId: null
        , PlantName: null
        , EntityId: null
        , EntityName: null
        , MaterialStorageId: null
        , IssueDate: null
        , Remarks: null
        , EmployeeId: null
        , EmployeeName: null
        , IssueType: 'Revenue'
    };
    $scope.IssueType = 'Revenue';
    $scope.productNew = Object.assign({}, $scope.product);

    $scope.changeType = function (data) {
        $scope.IssueType = data;
    }

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.product = $scope.issueList[index];
        $scope.productNew = Object.assign({}, $scope.product);
        $scope.materialStockList = [];
        $scope.specificStockList = [];

        getIssueDetailList();

        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };


    $scope.IssueReport = function (data) {
        location.href = "Products/InventoryIssue/IssueReport?grnId=" + data.Id;
    };

    $scope.issueId = null;
    $scope.confirmDelete = function (issueId,voucherId) {
        $scope.issueId = issueId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.delete = function (issueId, voucherId) {
        if (voucherId!=null)
            $scope.deleteUrl = "Products/InventoryIssue/PostedIssueDelete";
        else
            $scope.deleteUrl = "Products/InventoryIssue/NonPostedIssueDelete";

        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "issueId": issueId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.issueId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

}