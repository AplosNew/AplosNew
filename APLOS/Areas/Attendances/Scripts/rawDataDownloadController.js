'use strict';
rawDataDownloadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function rawDataDownloadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Raw Data Download';
    $scope.index = -1;
    $scope.path = 'Attendances/RawDataDownload/';

    var oldText = $filter('dateFiltering')(Date.now());
    var NewText = oldText.replace("-", "");
    $scope.someText = NewText.replace("-", "");

    $scope.rawdatadownload = {
        WorkDate: $filter('dateFiltering')(Date.now()),
        TextOrExcel: 'text'
    }

    $scope.changefilename = function () {
        var oldText = $filter('dateFiltering')($scope.rawdatadownload.WorkDate, 'dd-MM-yyyy');
        var NewText = oldText.replace("-", "");
        $scope.someText = NewText.replace("-", "");
    }
    $scope.Download = function () {
        try {
            var istextformat = '';
            if ($scope.rawdatadownload.TextOrExcel == 'text') {
                if (baseService.isUndefinedOrNull($scope.someText)) {
                    throw 'Please Write File Name';
                }
                if (baseService.isUndefinedOrNull($scope.rawdatadownload.WorkDate)) {
                    throw 'Please Select Work Date';
                }
                istextformat = 'False';
            }
            else {
                if (baseService.isUndefinedOrNull($scope.rawdatadownload.WorkDate)) {
                    manualValidation('div_FromDate', true, "Work Date is required.");
                    //ShowResult("Work Date is required.", 'failure');
                    throw "Work Date is required."
                } 
                istextformat = ' True';
            }

            $scope.parameters = '&WorkDate=' + $scope.rawdatadownload.WorkDate + '&someText=' + $scope.someText + '&reportFormat=Excel' + '&istextformat=' + istextformat;
            //location.href = $scope.path + 'GetRawData?' + $scope.parameters;
           var url= $scope.path + 'GetRawData?' + $scope.parameters;
            $rootScope.report(url);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}