'use strict';
entityFixedAssetsRegisterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function entityFixedAssetsRegisterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Entity Fixed Assets Register';
    $scope.Action = 'Save';
    $scope.path = 'FixedAssets/EntityFixedAssetsRegister/';
    var dt = new Date();

    $scope.reportParameters = {
        FromDate: $filter("dateFiltering")(new Date(dt.setDate(dt.getDate() - 10))), //$filter("dateFiltering")(Date.now()) - 10,
        ToDate: $filter("dateFiltering")(Date.now()),
        TransactionType: 'LoanTaken',
        ReportFormat: 'Excel',
        VoucherId: null
    };

    //$scope.material = {
    //    ReportFormat: 'Pdf',
    //    FromDate: $filter('dateFiltering')(Date.now()),
    //    ToDate: $filter('dateFiltering')(Date.now()),
    //    GRNandAccPType: 'GRNPosted',
    //    DateType: 'PostingDate',
    //    IsOrderSpecific: true,
    //    IsNonOrderSpecific: false
    //};


    $scope.EntityFixedAssetRegisterList = [];
    $scope.GetEntityFixedAssetRegisterData = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "GetEntityFixedAssetRegisterDataList",
                //data: { FromDate: $scope.reportParameters.FromDate, ToDate: $scope.reportParameters.ToDate },
                dataType: 'JSON'

            }).then(function successCallback(response) {

                //if (response.data.Error == false) {
                //    for (var i = 0; i < response.data.DATA.length; i++) {
                //      response.data.DATA[i].MasterLCDate = new Date(response.data.DATA[i].MasterLCDate);
                //    }
                //    $scope.EntityFixedAssetRegister = response.data.DATA;
                //}
                //else {
                //    ShowResult(response.data.Message, 'failure');
                //}


                $scope.EntityFixedAssetRegisterList = response.data.DATA;

            }),
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
        }

        catch (e) {

        }
    }
    $scope.GetEntityFixedAssetRegisterData();

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });


    //$scope.InvoiceSummaryReport = function () {

    //    try {
    //        var NewMasterLCList = [];
    //        for (var i = 0; i < $scope.MasterLCList.length; i++) {
    //            if ($scope.MasterLCList[i].isSelected == true) {

    //                if (NewMasterLCList, $scope.MasterLCList[i].PartyId) {
    //                    NewMasterLCList.push($scope.MasterLCList[i].PartyId);
    //                }
    //            }
    //        }
    //        if (NewMasterLCList.length == 0) {
    //            //(angular.isUndefinedOrNull(NewMasterLCList)) 
    //            ShowResult('Please select at least one Party', 'failure');
    //            //throw 'Please enter to date';

    //        } else {
    //            var file_src = $scope.path + "PartyPaymentStatusReport?MasterLCList=" + NewMasterLCList;
    //            $rootScope.report(file_src);
    //        }


    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //}

   
}


