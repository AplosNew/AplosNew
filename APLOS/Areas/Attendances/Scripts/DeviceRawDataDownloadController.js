'use strict';
DeviceRawDataDownloadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function DeviceRawDataDownloadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Raw Data Download';
    $scope.index = -1;
    $scope.path = 'Attendances/DeviceRawDataDownload/';

    var oldText = $filter('dateFiltering')(Date.now());
    var NewText = oldText.replace("-", "");
    $scope.someText = NewText.replace("-", "");

    $scope.rawdatadownload = {
        WorkDate: $filter('dateFiltering')(Date.now()),
        //TextOrExcel: 'text'
    }

    $scope.changefilename = function () {
        var oldText = $filter('dateFiltering')($scope.rawdatadownload.WorkDate, 'dd-MM-yyyy');
        var NewText = oldText.replace("-", "");
        $scope.someText = NewText.replace("-", "");
    }

    $scope.Download = function () {
        try {          
                if (baseService.isUndefinedOrNull($scope.someText)) {
                    throw 'Please Write File Name';
                }
                if (baseService.isUndefinedOrNull($scope.rawdatadownload.WorkDate)) {
                    throw 'Please Select Work Date';
                }
                $scope.parameters = '&WorkDate=' + $scope.rawdatadownload.WorkDate + '&someText=' + $scope.someText;
            location.href = $scope.path + 'GetRawData?' + $scope.parameters;
                //location.href = $scope.path + 'GetPFcsv?' + $scope.parameters;
           
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    

}