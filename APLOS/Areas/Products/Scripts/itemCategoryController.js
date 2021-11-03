'use strict';
function ItemCategoryController($scope, $routeParams, $location, $http, $filter) {
    $scope.tableShow = false;
    $scope.Action = 'New';
    $scope.itemCategory = {
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
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };

    GetAll();
    function GetSequence() {
        $http.get("Products/ItemCategory/GetAutoSequence")
            .then(function (response) {
                $scope.itemCategory.Sequence = response.data;
            });
    }

    $scope.Get = function (id) {
        $http.get("Products/ItemCategory/GetItemCategoryById/" + id)
            .then(function (response) {
                $scope.itemCategory = response.data;
            });
        $scope.Action = "Update";
    };

    $scope.Save = function () {
        $scope.itemCategory.AddedDate = $filter("date")(Date.now(), 'yyyy-MM-dd');
        $scope.itemCategory.UpdatedDate = null;
        if ($scope.Action == "New") {
            $scope.Action = "Save";
            GetSequence();
            return true;
        }
        if ($scope.Action == "Save") {
            $http({
                method: 'POST',
                url: "Products/ItemCategory/Create",
                data: $scope.itemCategory,
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
            });
            GetAll();
            return true;
        }
        else if ($scope.Action === "Update") {
            $http({
                method: 'POST',
                url: "Products/ItemCategory/Edit",
                data: $scope.itemCategory,
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
            });
            GetAll();
            return true;
        }
    }

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: "Products/ItemCategory/Delete/" + $scope.itemCategory.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                ClearFields()
                GetAll();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message);
        });
        return true;
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "New";
        $scope.itemCategory.Id = "";
        $scope.itemCategory.Sequence = "";
        $scope.itemCategory.Code = "";
        $scope.itemCategory.ShortName = "";
        $scope.itemCategory.StandardName = "";
        $scope.itemCategory.UserName = "";
        $scope.itemCategory.Description = "";
        $scope.itemCategory.Remarks = "";
        $scope.itemCategory.IsActive = true;
    }

    $scope.ShowemployeeCatList = function () {
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
        $scope.tableShow = true;
        GetAll();
    }

    $scope.page = 1;

    $scope.nBtn = false;
    $scope.pBtn = true;

    function GetAll() {
        $scope.take = 3;
        // You could use Restangular here with a route resource.
        $http.get('Products/ItemCategory/GetItemCategory?pageSize=' + $scope.take + '&pageNumber=' + $scope.page).then(function (response) {
            $scope.itemCategorys = response.data.ItemCategoryData;
            $scope.total_count = response.data.count;
            $scope.pages = response.data.totalPages;

        });


        $scope.myHTML = '<li> <a href="#" ng-disabled="pBtn" aria-label="Previous" ng-click="previousPage()"> <span aria-hidden="true">&laquo;</span> </a> </li>';

        for (var i = 1; i <= 10; i++) {

            if (i == $scope.page) {
                $scope.myHTML += '<li>' + i + '</li>'
            }
            else {
                $scope.myHTML += "<li> <a href=Products/ItemCategory/GetItemCategory?pageSize=" + $scope.take + "&pageNumber=" + i + ">" + i + "</a></li>";
                //@Html.ActionLink(i.ToString(), "Index", "Product", new { page = i }, null)<span>&nbsp;</span>
            }
        }

        $scope.myHTML += '<li> <a href="#" ng-disabled="nBtn" aria-label="Next" ng-click="nextPage()"> <span aria-hidden="true">&raquo;</span> </a></li>';
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
            $scope.nBtn = true;
        }
    };

    $scope.myText = "My name is: <h1>John Doe</h1>";
}
ItemCategoryController.$inject = ["$scope", "$routeParams", "$location", "$http", "$filter"];
