'use strict';
machineAttributeController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "$filter"];
function machineAttributeController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Machine Attribute';
    $scope.modelList = [];
    $scope.path = 'Machines/machineattribute/';
    $scope.getListUrl = $scope.path + 'getlist/';
    $scope.updateUrl = $scope.path + 'edit';

    $http.get('Machines/StitchCode/GetCbo')
        .then(function (response) {
            $scope.stitchCodeList = response.data;
        });

    // #region Material Master

    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , AssetMaster: null
        , UserName: null
        , BaseUom: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.popUpList = [];
    $scope.popUpParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpDataList = [];
        $scope.popUpUrl = 'materials/materialmastermachineprocess/getmaterialmasterlist';
        baseService.setCurrentPage('popUpDataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };
    $scope.selectDoubleClick = function (data) {
        $scope.modelNew = data;
        getDetails();
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    // #endregion Material Master


    function getDetails() {
        $http({
            method: 'GET'
            , url: 'Materials/materialmasterarticle/getlist?materialMasterId=' + $scope.modelNew.MaterialMasterId
            , contentType: "application/json; charset=utf-8"
        }).then(function successCallback(response) {
            $scope.modelList = response.data;
        });
    }

    $scope.Save = function () {
        $http({
            method: 'POST'
            , url: $scope.updateUrl
            , data: $scope.modelList
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = {};
        $scope.modelList = [];
    };
}
