'use strict';
CostingTypeComponentController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function CostingTypeComponentController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "Costing Type Component";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeComponentList = [];
    $scope.path = 'Costings/costingTypeComponent/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
  

    $scope.CostingComponentList = [];


    $scope.CostingTypeList = [];
    cboService.getCostingTypesCbo(function (response) {
        $scope.CostingTypeList = response;
    });


    $scope.CloseCostingSubCategoryPopUp = function (args) {

        $("#CostingSubCategoryPoUp").ejDialog();
        var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
        eDialog.close();
    };

    $scope.GetCostingComponent = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/quickCostingMaster/getCostingSubCategory',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.CostingSubCategoryList = response.data;

                    if ($scope.SelectedCostingComponetList.length > 0) {
                        for (var i = 0; i < $scope.SelectedCostingComponetList.length; i++) {
                            for (var j = 0; j < $scope.CostingSubCategoryList.length; j++) {
                                if ($scope.SelectedCostingComponetList[i].CostingComponentId == $scope.CostingSubCategoryList[j].Id) {
                                    $scope.CostingSubCategoryList[j].flag = true;
                                }
                            }
                        }
                    }


                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });
        } catch (e) {

        }
    };


    $scope.CostingSubCategoryList = [];
    $scope.ShowCostingSubCategoryPopUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.CostingTypeComponent.CostingType)) {
                throw "Select Costing Type.";
            }
            $scope.GetCostingComponent();

            $("#CostingSubCategoryPoUp").ejDialog();
            var eDialog = $("#CostingSubCategoryPoUp").data("ejDialog");
            eDialog.open();

            var gridObj = $("#GridOperation").data("ejGrid");
            gridObj.clearFiltering();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.CostingTypeComponent = {

        Id: null,
        Sequence: null,
        CostingType: null,
        CostingComponentId: null
    };

    $scope.CostingComponent = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null


    };

    $scope.SelectedCostingComponetList = [];

    $scope.AddCostingComponentToList = function () {
        if ($scope.CostingSubCategoryList.length > 0) {
            for (var i = 0; i < $scope.CostingSubCategoryList.length; i++) {
                if ($scope.CostingSubCategoryList[i].flag == true) {
                    if (checkExist($scope.SelectedCostingComponetList, $scope.CostingSubCategoryList[i].Id) === false) {
                        $scope.SelectedCostingComponetList.push({
                            Id: null,
                            Sequence: null,
                            CostingComponentId: $scope.CostingSubCategoryList[i].Id,
                            CostingType: $scope.CostingTypeComponent.CostingType,
                            Code: $scope.CostingSubCategoryList[i].Code,
                            ShortName: $scope.CostingSubCategoryList[i].ShortName,
                            StandardName: $scope.CostingSubCategoryList[i].StandardName,
                            UserName: $scope.CostingSubCategoryList[i].UserName
                        });
                    }
                }
            }
        }
        $scope.CloseCostingSubCategoryPopUp();
    };

    function checkExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CostingComponentId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.SelectedListForSave = [];
    function combineBothList(list) {
        angular.forEach(list, function (item, key) {
            item.Sequence = key + 1;
            $scope.SelectedListForSave.push(item);
        });
    }


    $scope.Save = function () {

        $scope.SelectedListForSave = [];
        combineBothList($scope.SelectedCostingComponetList);
        angular.forEach($scope.SelectedCostingComponetList, function (item) {
            if (baseService.isUndefinedOrNull(item.Sequence)) {
                throw "Secquence require";
            }
        });
        if ($scope.Action === 'Save') {
            $http({
                method: 'POST',
                url: 'Costings/costingTypeComponent/Create',
                data: { 'data': $scope.SelectedListForSave },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetCostingTypeComponent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.GetCostingTypeComponent = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/costingTypeComponent/GetCostingComponent?costingType=' + $scope.CostingTypeComponent.CostingType,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.SelectedCostingComponetList = response.data;
                    console.log($scope.SelectedCostingComponetList);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.RemoveCostingComponentRow = function () {
        try {
            $http({
                method: 'GET',
                url: 'Costings/costingTypeComponent/Delete?Id=' + $scope.SelectedCostingComponetList[$scope.indexToremove].Id,

                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetCostingTypeComponent();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {

        }
        $scope.SelectedCostingComponetList.splice($scope.indexToremove, 1);
    };

    $scope.RemoveCostingComponentConfirmation = function (index) {
        $scope.indexToremove = index;
        $scope.message_confirmation = "Are you sure to Delete permanently?";
        angular.element(document.querySelector("#RemoveCostingComponentConfirmationPopup")).modal("show");
    };

    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;
    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };

}
