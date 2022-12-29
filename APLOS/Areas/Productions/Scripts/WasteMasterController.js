'use strict';
WasteMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/WasteMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    //$scope.CompanyId = null;
    //$scope.CompanyList = [];
    $scope.ProcessId = null;
    $scope.ProcessList = [];
    $scope.EntityList = [];
    $scope.UOMList = [];
    $scope.BudgetList = [];
    $scope.Budget = null;

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

        //$http({
        //    method: 'GET',
        //    url: $scope.path + "getCompany",
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.CompanyList = response.data;
        //});

        $http({
            method: 'POST',
            url: $scope.path + "getProcess",
            data: { 'proId': $scope.ProcessId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });

        //$http({
        //    method: 'POST',
        //    url: $scope.path + "getBudget",
        //    dataType: 'JSON'
        //}).then(function successCallback(response) {
        //    $scope.BudgetList = response.data;
        //});

        $http({
            method: 'GET',
            url: $scope.path + "getUOM",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        });
    }
    $scope.getData();

    //$scope.getPlant = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "getPlants",
    //        data: { 'cmpId': $scope.CompanyId },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.PlantList = response.data;
    //    });
    //}

    //$scope.getEntity = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "getEntity",
    //        data: {'PlantId' : $scope.PlantId},
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.EntityList = response.data;
    //    });
    //}

    $scope.getBudgets = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getBudget",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetList = response.data;
        });
    }
    
    $scope.getBudgets();




    $scope.selectBudget = function () {
        //if (angular.isUndefinedOrNull($scope.ModelNew.EntityId)) {
        //    ShowResult("Please First Select the Entity!!" , 'failure');
        //    throw ("Invalid!!");
        //}
        angular.element(document.querySelector('#BudgetPop')).modal('show');
    }

    //$scope.doubleBudget = function (e) {
    //    $scope.Budget = e.data.Code;
    //    $scope.ModelNew.BudgetId = e.data.Id;
    //    angular.element(document.querySelector('#Budget')).modal('hide');
    //}

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        WasteTypeId:null,
        ProcessId: null,
        UOMId: null,
        Category: null,
        SubCategory: null,
        ItemName: null,
        StandardRate: null,
        Code: null,
        Remarks:null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.SelBudList = [];

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        var AllData = [];
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            data: {'Id':args.data.Id},
            dataType: 'JSON'
        }).then(function successCallback(resp) {
            $scope.BudgetIds = [];
            $scope.SelBudList = [];
            AllData = resp.data.master;
            var child = resp.data.child;
            var ob = {};
            $scope.ModelNew = Object.assign({}, AllData[0]);
            for (var i = 0; i < child.length; i++) {
                ob[child[i].BudgetId] = true;
                $scope.BudgetIds.push(child[i].BudgetId);
                
            }

            for (var i = 0; i < $scope.BudgetList.length; i++) {
                if ($scope.BudgetList[i].Id in ob) {
                    $scope.BudgetList[i].isSelected = true;
                    $scope.SelBudList.push($scope.BudgetList[i]);
                }
                else {
                    $scope.BudgetList[i].isSelected = false;
                }
            }


        });

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        //if (angular.isUndefinedOrNull($scope.ModelNew.BudgetId)) {
        //    ShowResult('No Budget Code Selected!!' , 'failure');
        //    throw ("Invalid");
        //}

        if (angular.isUndefinedOrNull($scope.ModelNew.UOMId)) {
            ShowResult('No UOM Selected!!', 'failure');
            throw ("Invalid");
        }


        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'datas': $scope.ModelNew , 'budgets' :$scope.BudgetIds },
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
        $scope.CompanyId = null;
        $scope.PlantId = null;
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.BudgetIds = [];
        $scope.SelBudList = [];
    }

    // Addition of the Modal Operations for Budget Child
    $scope.closeBudPopUp = function () {
        angular.element(document.querySelector('#BudgetPop')).modal('hide');
    }

    $scope.BudgetIds = [];

    $scope.selectBudDetail = function () {
        $scope.BudgetIds = [];
        $scope.SelBudList = [];
        for (var i = 0; i < $scope.BudgetList.length; i++) {
            if ($scope.BudgetList[i].isSelected == true) {
                $scope.BudgetIds.push($scope.BudgetList[i].Id);
                $scope.SelBudList.push($scope.BudgetList[i]);
            }
        }

        angular.element(document.querySelector('#BudgetPop')).modal('hide');
    }

    $scope.WasteTypeList = [];
    $scope.getWasteType = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetWasteType",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WasteTypeList = response.data;
        });
    }
    $scope.getWasteType();
}