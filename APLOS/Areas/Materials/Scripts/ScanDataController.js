'use strict';
ScanDataController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ScanDataController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "Scan Data";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.materialAttributes = [];
    $scope.path = 'Materials/ScanData/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    // #region Get Item Scan Child Data
    $scope.ItemScanChildList = [];
    $scope.ItemScanChild = function () {
        $http({
            method: 'GET',
            url: 'Materials/ScanData/GetItemScanChild'
        }).then(function successCallback(response) {
            $scope.ItemScanChildList = response.data;
        })
    }
    $scope.ItemScanChild();
    // #endregion Get Item Scan Child Data

    //  #region Save
    
    $scope.Save = function () {        
        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: {
                'data' : $scope.ItemScanChildList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };
    //  #endregion Save
}