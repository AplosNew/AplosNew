'use strict';
//skillMatrixController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
empActiveInActiveController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function empActiveInActiveController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Active Inactive';
    
    ///========================Start Here===========

    // #region All Tab Control
    $scope.tab = 1;
    $scope.setTabFirst = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldataInactive();
        //$scope.GRN = 0;

    };
    $scope.isSetFirst = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.getalldataInactive();
    };

    $scope.setTabSecond = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldataActive();
        //$scope.GRN = 1;
    };
    $scope.isSetSecond = function (tabNum) {
        return $scope.tab === tabNum;
        $scope.getalldataActive();
    };
    // End PO approve

    //Start Load Data On Grid 

    $scope.GriddataInactive = [];
    $scope.getalldataInactive = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'employees/EmpActiveInActive/GetListForInActive',
        }).then(function successCallback(response) {
            $scope.GriddataInactive = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.getalldataInactive();
    $scope.GriddataActive = [];
    $scope.getalldataActive = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'employees/EmpActiveInActive/GetListForActive',
        }).then(function successCallback(response) {
            $scope.GriddataActive = response.data;
            //entrydata = copy(searchdata);
        });
    };

 $scope.getalldataActive();
 //End Load Data On Grid 
        //Start on click
    $scope.onClickInActive = function (args) {
        //debugger;
        var gridObj = $("#GridInActive").data("ejGrid");
        //getting corresponding record 
        $scope.InActive = gridObj.getSelectedRecords()[0];
        $scope.SystemId = $scope.InActive.SystemId;
        angular.element(document.querySelector('#ActionPopUp')).modal('show');
       //$scope.InActiveAlert();
    };
    $scope.commandInActive = [{
        type: "details", buttonOptions: { 
            text: "Re-activate",
            width: "100",
            height: "30",
            click: $scope.onClickInActive
           
        }
    }]; 

       //End on click
    // Alert 
    $scope.InActiveAlert = function () {
        $scope.Inactivemessage = 'Are you sure want to Active?';
        angular.element(document.querySelector('#InactiveAlert')).modal('show');
    };   
    $scope.onClickActive = function (args) {
        debugger;
        var gridObj = $("#GridActive").data("ejGrid");
        //getting corresponding record 
        $scope.Active = gridObj.getSelectedRecords()[0];
        $scope.SystemId = $scope.Active.SystemId;
        $scope.ActiveAlert();
    };
    $scope.commandActive = [{
        type: "details", buttonOptions: {
            text: "In Active",
            width: "100",
            height: "30",
            click: $scope.onClickActive
        }
    }];   
    $scope.ActiveAlert = function () {
        $scope.Activemessage = 'Are you sure want to Separate?';
        angular.element(document.querySelector('#ActiveAlert')).modal('show');
    };
    $scope.UpdateInActiveToActive = function () {
        $http({
            method: 'POST',
            url: 'employees/EmpActiveInActive/InActiveToActive',
            data: {
                'SystemId': $scope.SystemId,
                'Reason': $scope.reason 
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataInactive();
                $scope.ClosedPOPUp();
                
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }
    $scope.UpdateActiveToInActive = function () {
        $http({
            method: 'POST',
            url: 'employees/EmpActiveInActive/ActiveToInActive',
            data: {
               
                'SystemId': $scope.SystemId
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getalldataActive();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }




    //#region POP up Open
    //$scope.getActionPopUp = function () {
       

    //    debugger;
    //    //$scope.Currency = $("#currency option:selected").text();
    //    //$scope.currentMaterialRow = index;
    //    //$scope.currentInventoryReceiveDetailIdRow = Id;
    //    //$scope.taxAbleAmnt = data.TrnAmount;
    //    //$scope.percentageColumn = flag;        
    //    angular.element(document.querySelector('#ActionPopUp')).modal('show');
        
    //};

    //#endregion


    $scope.ClosedPOPUp = function (args) {
        
        angular.element(document.querySelector('#ActionPopUp')).modal('hide');
        //$scope.InActiveAlert();
    };

    
}