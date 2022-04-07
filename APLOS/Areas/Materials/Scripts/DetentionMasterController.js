'use strict';
DetentionMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "DetentionMaster";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';

    $scope.detention = {
        Id: null
        , DetentionCategory: null
        , DetentionSubCategory: null
        , DetentionStandaredName: null
        , DetentionUserName: null
        , DetentionType: null
        , DetentionCriticality: null
        , ResponsiblePersion: null
        , DetentionTarget: null
        , DetentionPlan: null
        , IsAvoidable: true
    };
    $scope.detentionNew = Object.assign({}, $scope.detention);

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }

    $scope.DetentionList = [];
    $scope.LoadDetentionList = function () {
        $http({

            method: 'Get',
            url: 'Materials/DetentionMaster/LoadDetentionList'
        }).then(function successCallback(response) {
            $scope.DetentionList = response.data;
        }
        )
    }
    $scope.LoadDetentionList();
    //$scope.GetSequence = function () {
    //    cboService.getSequence($scope.getSeqUrl, function (data) {
    //        $scope.Detention.Sequence = data;
    //        $scope.DetentionNew.Sequence = data;
    //    });
    //};
    //$scope.GetSequence();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DetentionMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DetentionData': $scope.detentionNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.LoadDetentionList();
                    DetentionClearFields();
                   /* $scope.GetDetails({ data: { Id: response.data.Data.Id } });*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.rackNew.Id)) {
    //        $http({
    //            method: 'POST'
    //            , url: $scope.path + 'Delete?Id=' + $scope.rackNew.Id
    //            , dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');                   
    //                ClearFields(response.data.Sequence);
    //                $scope.LoadRackList();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};

    $scope.GetDetails = function (args) {
        $http({

            method: 'Get',
            url: 'Materials/DetentionMaster/LoadEditData?DetentionID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.detentionNew = response.data.detention[0];
           
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.Clear = function () {
        return true;
    };

//    function ClearFields(seq) {
//        $scope.Action = "Save";
//        $scope.detentionNew = Object.assign({}, $scope.detention);
///*        $scope.rackNew.Sequence= seq;*/
//        $scope.binList =[];

//    }

    function DetentionClearFields() {
        $scope.Action = "Save";
        $scope.detentionNew = Object.assign({}, $scope.detention);

    }
}