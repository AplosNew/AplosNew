'use strict';
generalLedgerVSfixedAssetsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function generalLedgerVSfixedAssetsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'GL VS FA';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.MasterLCList = [];
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.reportParameters = {
        FromDate: null,
        ToDate: null
        //LCType: 'contract'
    };

    //$scope.GetMasterLCList = function () {

    //    try {

    //        if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
    //            throw 'Please enter from date';

    //        if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
    //            throw 'Please enter to date';

    //          $http({
    //                method: 'POST',
    //              url: $scope.path + "GetMasterLCList",
    //              data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate, lcType: $scope.reportParameters.LCType },
    //                dataType: 'JSON'

    //          }).then(function successCallback(response) {
    //              if (response.data.Error == false) {
    //                  for (var i = 0; i < response.data.DATA.length; i++) {
    //                      response.data.DATA[i].LCOpeningDate = new Date(response.data.DATA[i].LCOpeningDate);
    //                      response.data.DATA[i].ExpiryDate = new Date(response.data.DATA[i].ExpiryDate);
    //                  }
    //                  $scope.MasterLCList = response.data.DATA;
    //              }
    //              else {
    //                  ShowResult(response.data.Message, 'failure');
    //              }
           
    //          }),
    //              function errorCallBack(response) {
    //               ShowResult(response.data.Message, 'failure');

    //              }
    //    }
    //     catch (e) {

    //     }
    //}


    $scope.glVSfaReport = function () {

        try {

            //if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
            //    throw 'Please enter from date';

            //if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
            //    throw 'Please enter to date';

            //var MasterLCList = "";
            //for (var i = 0; i < $scope.MasterLCList.length; i++) {
            //    if ($scope.MasterLCList[i].isSelected == true) {
            //        if (MasterLCList == "")
            //            MasterLCList = "'" + $scope.MasterLCList[i].MasterLCNo + "'";
            //        else
            //            MasterLCList += ",'" + $scope.MasterLCList[i].MasterLCNo + "'";
            //    }
            //}

           // var file_src = $scope.path + "MasterLCReport?MasterLCList=" + MasterLCList; 
            var file_src = $scope.path + "glVSfaReport"; 
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.MasterOrderReport = function () {
    //    try {
    //        var file_src = $scope.path + "MasterOrderReport?MasterOrderId=1935" ;
    //        $rootScope.report(file_src);
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

    

}


