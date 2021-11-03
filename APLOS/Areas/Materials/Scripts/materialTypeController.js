'use strict';
MaterialTypeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function MaterialTypeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Material Type";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialTypes = [];
    $scope.path = 'Materials/materialtype/';
    $scope.getListUrl = $scope.path + 'getlist';
    baseService.init($scope.getListUrl, null, null, null, "UserName", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.materialTypes = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.searchByMaterialTypeList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        },
        {
            'name': 'StandardName',
            'value': 'StandardName'
        },
        {
            'name': 'Description',
            'value': 'Description'
        }
    ];

    $scope.materialTypeNew = {
        Id: null
        , CompanyGroupId: null
        , Sequence: null
        , Code: null
        , Description: null
        , Remarks: null
        , Active: true
        , StandardName: null
        , UserName: null
        ,ShortName:null
    };
    $scope.materialType = Object.assign({}, $scope.materialTypeNew);
    //getNatureCbo();

    $scope.GetSequence = function () {
        $http.get("Materials/materialtype/getautosequence")
            .then(function (response) {
                $scope.materialType.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.materialTypeNew = $scope.materialTypes[$scope.index];
        $scope.materialType = Object.assign({}, $scope.materialTypeNew);
        //getNatureCbo();
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.CheckProductMstRrequired = function () {
        if ($scope.materialType.IsProductMstMandatory) {
            if (!$scope.materialType.IsProductMstRequired) {
                ShowResult('Please at first select product(mst) required.', "failure");
                $scope.materialType.IsProductMstMandatory = false;
                return;
            }
        }
    };

    $scope.CheckOurStyleRrequired = function () {
        if ($scope.materialType.IsOurStyleMandatory) {
            if (!$scope.materialType.IsOurStyleRequired) {
                ShowResult('Please at first select our style.', "failure");
                $scope.materialType.IsOurStyleMandatory = false;
                return;
            }
        }
    };

    $scope.CheckProcessRrequired = function () {
        if ($scope.materialType.IsProcessMandatory) {
            if (!$scope.materialType.IsProcessRequired) {
                ShowResult('Please at first select process.', "failure");
                $scope.materialType.IsProcessMandatory = false;
                return;
            }
        }
    };

    $scope.CheckSeassonRrequired = function () {
        if ($scope.materialType.IsSeasonMandatory) {
            if (!$scope.materialType.IsSeasonRequired) {
                ShowResult('Please at first select season.', "failure");
                $scope.materialType.IsSeasonMandatory = false;
                return;
            }
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.materialTypeForm.$valid) {
            //if ($scope.materialType.IsProductMstMandatory && $scope.materialType.IsProductMstRequired === false) {
            //    ShowResult('Product(Mst) mandatory can not be checked without product(Mst) required.', "failure");
            //    return;
            //}
            //if ($scope.materialType.IsOurStyleMandatory && $scope.materialType.IsOurStyleRequired === false) {
            //    ShowResult('Our Style mandatory can not be checked without our style required.', "failure");
            //    return;
            //}
            //if ($scope.materialType.IsProcessMandatory && $scope.materialType.IsProcessRequired === false) {
            //    ShowResult('Process mandatory can not be checked without style required.', "failure");
            //    return;
            //}
            //if ($scope.materialType.IsSeasonMandatory && $scope.materialType.IsSeasonRrequired === false) {
            //    ShowResult('Season mandatory can not be checked without season required.', "failure");
            //    return;
            //}
            $scope.materialTypeNew = Object.assign({}, $scope.materialType);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: "Materials/materialtype/create",
                    //data: { materialType: $scope.materialType, materialTypeNatureList: $scope.natureList },
                    data: { materialType: $scope.materialType},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.materialType = response.data.MaterialType;
                        $scope.materialTypes.push($scope.materialType);
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            else if ($scope.Action === "Update") {
                $http({
                    method: 'POST',
                    url: "Materials/materialtype/edit",
                   // data: { materialType: $scope.materialType, materialTypeNatureList: $scope.natureList },
                    data: { materialType: $scope.materialType},
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        if ($scope.index > -1) {
                            $scope.materialTypes[$scope.index] = $scope.materialType;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.materialType.Id)) {
            $http({
                method: 'POST',
                url: "Materials/materialtype/delete/" + $scope.materialType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.materialTypes.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.materialType = {};
        $scope.materialTypeNew = {};
        $scope.materialType.Sequence = seq;
        $scope.materialType.Active = true;
        $scope.materialType.IsCodeAutoGenerate = false;
        $scope.materialType.IsProductMstRequired = false;
        $scope.materialType.IsProductMstMandatory = false;
        $scope.materialType.IsOurStyleRequired = false;
        $scope.materialType.IsOurStyleMandatory = false;
        $scope.materialType.IsSeasonRequired = false;
        $scope.materialType.IsSeasonMandatory = false;
        $scope.materialType.IsProcessRequired = false;
        $scope.materialType.IsProcessMandatory = false;
        $scope.materialType.IsProcessRouting = false;
        //getNatureCbo();
    }

    //function getNatureCbo() {
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/materialtype/getmaterialtypenaturelistcbo'
    //    }).then(function successCallback(response) {
    //        $scope.natureList = response.data;
    //        getNatureList();
    //    });
    //}
    //$scope.List = [];
    //function getNatureList() {
       
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/materialtype/GetMaterialTypeNatureList',
    //        params: { 'masterId': $scope.materialType.Id }
    //    }).then(function successCallback(response) {
    //        if (baseService.arrayLength(response.data) > 0) {
    //            $scope.List = response.data;
    //            for (var t = 0; t < baseService.arrayLength($scope.natureList); t++) {
    //                for (var i = 0; i < baseService.arrayLength($scope.List); i++) {
    //                    if (!baseService.isUndefinedOrNull($scope.List[i].Id) && $scope.List[i].Nature === $scope.natureList[t].Nature) {
    //                        $scope.natureList[t].Id = $scope.List[i].Id;
    //                        $scope.natureList[t].Flag = $scope.List[i].Flag;
    //                    }
    //                }
    //            }
    //        }
    //    });
    //}

}
