'use strict';
function OrderControlStageController($scope, $routeParams, $location, $http, $filter, $compile) {
    $scope.Action = 'New';
    GetAll();
    $scope.orderConSt = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        IsActive: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        UpdatedFromIP: null
    };

    function GetSequence() {
        $http.get("OrderManagements/OrderControlStage/GetAutoSequence")
            .then(function (response) {
                $scope.orderConSt.Sequence = response.data;
            });
    }

    $scope.Get = function (id) {
        $http.get("OrderManagements/OrderControlStage/GetorderControlStageById/" + id)
            .then(function (response) {
                $scope.orderConSt = response.data;
            });
        $scope.Action = "Update";
    };

    $scope.Save = function () {
        $scope.orderConSt.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.orderConSt.UpdatedDate = null;
        if ($scope.Action == "New") {
            $scope.Action = "Save";
            GetSequence();
            return true;
        }
        if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: "OrderManagements/OrderControlStage/Create",
                data: $scope.orderConSt,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    ClearFields();
                    GetAll();
                }
            }, function errorCallback(response) {

            });
            GetAll();
            return true;
        }
        else if ($scope.Action == "Update") {
            $http({
                method: 'POST',
                url: "OrderManagements/OrderControlStage/Edit",
                data: $scope.orderConSt,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    ClearFields();
                    GetAll();
                }
            }, function errorCallback(response) {

            });
            GetAll();
            return true;
        }
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/OrderControlStage/Delete/" + $scope.orderConSt.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                ClearFields();
                GetAll();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message);
        });
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "New";
        $scope.orderConSt.Id = "";
        $scope.orderConSt.Sequence = "";
        $scope.orderConSt.Code = "";
        $scope.orderConSt.ShortName = "";
        $scope.orderConSt.StandardName = "";
        $scope.orderConSt.UserName = "";
        $scope.orderConSt.Description = "";
        $scope.orderConSt.Remarks = "";
        $scope.orderConSt.IsActive = true;
    }

    $scope.ShowOrderStList = function () {
        var caolvl1alOptions = {
            closeButtonText: 'Cancel',
            actionButtonText: 'Delete Customer',
            headerText: 'Delete ' + custName + '?',
            bodyText: 'Are you sure you want to delete this customer?'
        };
        caolvl1alService.showcaolvl1al({}, caolvl1alOptions).then(function (result) {
            if (result === 'ok') {
                dataService.deleteCustomer(id).then(function () {
                    for (var i = 0; i < vm.customers.length; i++) {
                        if (vm.customers[i].id === id) {
                            vm.customers.splice(i, 1);
                            break;
                        }
                    }
                    filterCustomers(vm.searchText);
                }, function (error) {
                    $window.alert('Error deleting customer: ' + error.message);
                });
            }
        });
    }

    $scope.Show = function () {
        GetAll();
    }

    $scope.al = function (i) {
        $scope.take = 3;
        // You could use Restangular here with a route resource.
        $http.get('OrderManagements/OrderControlStage/GetorderControlStage?pageSize=' + $scope.take + '&pageNumber=' + i)
            .then(function (response) {
                $scope.orderConSts = response.data.OrderControlStageData;
                $scope.total_count = $scope.orderConSts.length;
                $scope.pages = response.data.totalPages;

            });

        $scope.myHTML = '<li> <a href="javascipt:void(0)" ng-disabled="pBtn" aria-label="Previous" ng-click="previousPage()"> <span aria-hidden="true">&laquo;</span> </a> </li>';

        for (var i = 1; i <= $scope.total_count; i++) {

            if (i == $scope.page) {
                $scope.myHTML += '<li>' + i + '</li>';
            }
            else {
                $scope.myHTML += "<li> <a href='javascipt:void(0)' ng-click=al(" + i + ")>" + i + "</a></li>";
                //@Html.ActionLink(i.ToString(), "Index", "Product", new { page = i }, null)<span>&nbsp;</span>
            }
        }

        $scope.myHTML += '<li> <a href="javascipt:void(0)" ng-disabled="nBtn" aria-label="Next" ng-click="nextPage()"> <span aria-hidden="true">&raquo;</span> </a></li>';

    };

    $scope.page = 1;

    $scope.nBtn = false;
    $scope.pBtn = true;

    function GetAll() {
        $scope.take = 3;
        // You could use Restangular here with a route resource.
        $http.get('OrderManagements/OrderControlStage/GetorderControlStage?pageSize=' + $scope.take + '&pageNumber=' + $scope.page)
            .then(function (response) {
                $scope.orderConSts = response.data.OrderControlStageData;
                $scope.total_count = $scope.orderConSts.length;
                $scope.pages = response.data.totalPages;
            });
        $scope.myHTML = '<li> <a href="javascipt:void(0)" ng-disabled="pBtn" aria-label="Previous" ng-click="previousPage()"> <span aria-hidden="true">&laquo;</span> </a> </li>';

        for (var i = 1; i <= $scope.total_count; i++) {

            if (i == $scope.page) {
                $scope.myHTML += '<li>' + i + '</li>';
            }
            else {
                $scope.myHTML += "<li> <a href='javascipt:void(0)' ng-click=al(" + i + ")>" + i + "</a></li>";
                //@Html.ActionLink(i.ToString(), "Index", "Product", new { page = i }, null)<span>&nbsp;</span>
            }
        }

        $scope.myHTML += '<li> <a href="javascipt:void(0)" ng-disabled="nBtn" aria-label="Next" ng-click="nextPage()"> <span aria-hidden="true">&raquo;</span> </a></li>';

    }

    $scope.nextPage = function () {
        if ($scope.page < $scope.pages) {
            $scope.page++;
            GetAll();
            $scope.pBtn = false;
        }
    };

    $scope.previousPage = function () {
        if ($scope.page > 1) {
            $scope.page--;
            GetAll();
            $scope.pBtn = true;
        }
    };
}
OrderControlStageController.$inject = ["$scope", "$routeParams", "$location", "$http", "$filter", "$compile"];