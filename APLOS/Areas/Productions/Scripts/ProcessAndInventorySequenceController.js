
'use strict';
ProcessAndInventorySequenceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProcessAndInventorySequenceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Costing Types";
    $scope.Action = 'Save';
    $scope.path = 'Productions/ProcessAndInventorySequence/';

    $scope.getProcessAndInventorySeq = function () {
        //});
        try {
            var file_src = $scope.path + 'GetProcessAndInventorySeq';
            $rootScope.report(file_src);

        } catch (e) {

        }


    }

    $scope.cboPlantList = [];
    $scope.plantId = null;

    
   

    $scope.changeCompany = function (args) {
        $scope.ProcessAndInventorySeq = [];

        cboService.getCboPlantByCompany($scope.SelectedId,function ( result) {
            $scope.cboPlantList = result;
        });
    }

    $scope.actioncomplete = function (args) {

        if (args.action == "rowReordering") {
            var gridObj = $("#GridProcessAndInventorySequence").data("ejGrid");
            // Gets current view data of grid control
            var data = gridObj.getCurrentViewData();

            for (var i = 0; i < data.length; i++) {
                data[i].Sequence = (i + 1);
            }

            $scope.ProcessAndInventorySeq = data;
            gridObj.refreshContent(true);

        }
    }
    $scope.ProcessAndInventorySeq = [];

    $scope.LoadData = function (args) {
         $http({
            method: 'POST',
            url: $scope.path + "GetProcessAndInventorySeq",
             data: { plantId: $scope.plantId},
            dataType: 'JSON'

        }).then(function successCallback(response) {
            if (response.data.Error == false) {


                $scope.ProcessAndInventorySeq = response.data.DATA;
            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }),


            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }

    }

   
    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.path + "Save",
            data: {
                'data': $scope.ProcessAndInventorySeq,
                plantId: $scope.plantId
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
        };


    }





    $scope.SelectedId = null;

    $scope.CompanyList = [];
 
    $scope.LoadCompanyListData = function (args) {
        $http({
            method: 'POST',
            url: $scope.path + "GetCompanyList",
            data: {},
            dataType: 'JSON'

        }).then(function successCallback(response) {
            if (response.data.Error == false) {


                $scope.CompanyList = response.data.DATA;

            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }

    }

    $scope.LoadCompanyListData();

}