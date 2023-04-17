'use strict';
LCReportsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function LCReportsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'LC Reports';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.MasterLCList = [];
    $scope.path = 'Commercial/LCReports/';
    $scope.reportParameters = {
        FromDate: null,
        ToDate: null,
        //LCType: null
        LCType: 'contract'
    };

    $scope.GetMasterLCList = function () {

        try {

            if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
                throw 'Please enter from date';

            if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
                throw 'Please enter to date';

              $http({
                    method: 'POST',
                  url: $scope.path + "GetMasterLCList",
                  data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate, lcType: $scope.reportParameters.LCType },
                    dataType: 'JSON'

              }).then(function successCallback(response) {
                  if (response.data.Error == false) {
                      for (var i = 0; i < response.data.DATA.length; i++) {
                          response.data.DATA[i].LCOpeningDate = new Date(response.data.DATA[i].LCOpeningDate);
                          response.data.DATA[i].ExpiryDate = new Date(response.data.DATA[i].ExpiryDate);
                      }
                      $scope.MasterLCList = response.data.DATA;
                  }
                  else {
                      ShowResult(response.data.Message, 'failure');
                  }
           
              }),
                  function errorCallBack(response) {
                   ShowResult(response.data.Message, 'failure');

                  }
        }
         catch (e) {

         }
    }


    $scope.MasterLCReport = function () {

        try {

            if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
                throw 'Please enter from date';

            if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
                throw 'Please enter to date';

            var MasterLCList = "";
            for (var i = 0; i < $scope.MasterLCList.length; i++) {
                if ($scope.MasterLCList[i].isSelected == true) {
                    if (MasterLCList == "")
                        MasterLCList = "'" + $scope.MasterLCList[i].MasterLCNo + "'";
                    else
                        MasterLCList += ",'" + $scope.MasterLCList[i].MasterLCNo + "'";
                }
            }
            var file_src = $scope.path + "MasterLCReport?MasterLCList=" + MasterLCList; 
            $rootScope.report(file_src);

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath

    //$scope.MasterLCReport = function () {
    //    if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
    //        throw 'Please enter from date';

    //    if (angular.isUndefinedOrNull($scope.reportParameters.ToDate))
    //        throw 'Please enter to date';

    //    var MasterLCList = "";
    //    for (var i = 0; i < $scope.MasterLCList.length; i++) {
    //        if ($scope.MasterLCList[i].isSelected == true) {
    //            if (MasterLCList == "")
    //                MasterLCList = "'" + $scope.MasterLCList[i].MasterLCNo + "'";
    //            else
    //                MasterLCList += ",'" + $scope.MasterLCList[i].MasterLCNo + "'";
    //        }
    //    }

    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "MasterLCReport",

    //        data: {'MasterLCList': MasterLCList},
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FullPath + "&fileName=" + response.data.FileName);//downloadgriddataUrlPath
    //        }
    //    }, function errorCallback(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    });
    //}

    $scope.MasterOrderReport = function () {

        //var MasterOrderId = "1935";
        try {
            var file_src = $scope.path + "MasterOrderReport?MasterOrderId=1935" ;
            $rootScope.report(file_src);


        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    

   //get data from MasterLC
    
    //$scope.GetMasterLCList = function () {
    //    try {
    //        if (angular.isUndefinedOrNull($scope.reportParameters.FromDate))
    //            throw 'Please enter from date';

    //        if (angula.isUndefinedOrNull($scope.repo.ToDate))
    //            throw 'please enter to date';

    //        $http({
    //            method: 'POST',
    //            url: $scope.path + "GetMasterLCList",
    //            data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            for (var i = 0; i < response.data.length; i++) {
    //                response.data[i].MasterLCDate = new Date(response.data[i].MasterLCDate);
    //                response.data[i].MasterLCDate = new Date(response.data[i].MasterLCDate);
    //            }
    //            $scope.MasterLCList = response.data;
    //        }),
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }

    //    } catch (e) {

    //    }
}


