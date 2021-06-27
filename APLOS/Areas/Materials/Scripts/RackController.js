'use strict';
RackController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function RackController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Rack";
    $scope.Action = 'Save';
    $scope.RackList = [];
    $scope.index = -1;
    $scope.path = 'Materials/Rack/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';

    $scope.rack = {
        Id: null
        , StorageLocationId: null
        , Sequence: 0.0
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , PlantId: null
        , Storage: null
        , Rows: 1
        , Columns: 1
        , Active: true
    };
    $scope.rackNew = Object.assign({}, $scope.rack);

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }
    $scope.bin = {
        Id: null,
        RackId: null,
        Code: null,
        Row: 0,
        Column: 0,
        UserName: null
    }
    $scope.binList = [];

    $scope.GenerateBIN = function () {
        $scope.binList = [];
        if ($scope.rackNew.Rows <= 0 || $scope.rackNew.Columns <= 0 || baseService.isUndefinedOrNull($scope.rackNew.Rows) || baseService.isUndefinedOrNull($scope.rackNew.Columns)) {

            ShowResult('Rows and Columns should greater than 0.','failure')
        }

        for (var ROW = 1; ROW <= $scope.rackNew.Rows; ROW++) {
            
            for (var COL = 1; COL <= $scope.rackNew.Columns; COL++) {
                var tempItem = Object.assign({}, $scope.bin);
                tempItem.Code = "R" + ROW + "C" + COL;
                tempItem.UserName = tempItem.Code;
                tempItem.Row = ROW;
                tempItem.Column = COL;

                $scope.binList.push(tempItem);
            }
        }
    }

    $scope.RackList = [];

    $scope.LoadRackList = function () {
        $http({

            method: 'Get',
            url: 'Materials/Rack/LoadRackList'
        }).then(function successCallback(response) {
            $scope.RackList = response.data;
        }
        )
    }
    $scope.LoadRackList();

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.rack.Sequence = data;
            $scope.rackNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.StorageList = [];
    $scope.storage = function () {
        $http.get($scope.getStorage)
            .then(function (response) {
                $scope.StorageList = response.data;
            });

    }
    $scope.storage();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.rackNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'RackData': $scope.rackNew, 'BinData': $scope.binList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.LoadRackList();
                    $scope.GetDetails({ data: { Id: response.data.Data.Id } });
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.rackNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'Delete?Id=' + $scope.rackNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                   
                    ClearFields(response.data.Sequence);
                    $scope.LoadRackList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.GetDetails = function (args) {
        $http({

            method: 'Get',
            url: 'Materials/Rack/LoadEditData?RackID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.rackNew = response.data.rack[0];
            $scope.binList = response.data.bin;
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )

    }


    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.rackNew = Object.assign({}, $scope.rack);
        $scope.rackNew.Sequence= seq;
        $scope.binList =[];

    }
}