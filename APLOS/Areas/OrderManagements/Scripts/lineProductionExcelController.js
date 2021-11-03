'use strict';
lineProductionExcelController.$inject = ['fileReader', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function lineProductionExcelController(fileReader, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Line Production Excel";
    $scope.Action = 'Save';
    $scope.productionList = [];
    $scope.path = 'OrderManagements/LineProductionBooking/';
    $scope.getListUrl = $scope.path + 'getlistbydate';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveNoApplicablePcsRateUrl = $scope.path + 'updateNoApplicablePcsRate?id=';

    $scope.model = {
        Id: null
        , ProductionDate: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.getList = function () {
        if (baseService.isUndefinedOrNull($scope.modelNew.ProductionDate))
            return $scope.productionList = [];
        $http({
            method: "GET"
            , url: $scope.getListUrl
            , params: {
                'date': $filter('dateFiltering')($scope.modelNew.ProductionDate, 'dd-MM-yyyy')
            }
            , dataType: "json"
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else
                $scope.productionList = response.data;
        }), function errorCallBack(response) {
            showResult(response.data.Message, 'failure');
        };
    };

    //#region
    $("#upload").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getFile = function () {
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {

            });
    };
    $scope.getExcelData = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelNewForm.$valid) {
            var formData = new FormData();
            $http({
                method: 'POST',
                url: $scope.path + 'PostExcelData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    formData.append('toDate', data.toDate);
                    formData.append('file', data.file);
                    return formData;
                },
                data: {
                    'toDate': $scope.modelNew.ProductionDate
                    , 'file': $scope.filedata
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    document.getElementById("upload").value = '';
                    $scope.getList();
                }
            }, function errorCallback(response) {
                ShowResult(response.Message, 'failure');
            });
        }
    };


    $scope.noApplicablePcsRate = function (id) {
        $http({
            method: 'POST'
            , url: $scope.saveNoApplicablePcsRateUrl + id
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
            }
        }, function errorCallback(response) {
            ShowResult(response.Message, 'failure');
        });
    };
    //#endregion 

}
