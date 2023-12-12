'use strict';
MasterPlanSetUpController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function MasterPlanSetUpController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Master Plan SetUp";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ModelList = [];
    $scope.path = 'Productions/MasterPlanSetUp/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateData';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Sequence', name: "Sequence" }, { value: 'Id', name: "MPD.Id" }, { value: 'UserName', name: "User Name" }, { value: 'Particular', name: "Particular" }, { value: 'Process', name: "Process" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null
        , Sequence: null
        , UserName: null
        , Particular: null
        , ProcessId: null
        , Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.ModelNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.ProcessList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'Productions/MasterPlan/GetProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetProcessList();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.Process = args.data.Process;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.modelForm.$valid) {
            $http({
                method: "POST",
                url: 'Productions/MasterPlanSetUp/CreateMPSetUp',
                data: {
                    'data': $scope.ModelNew
                },
                dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                        $scope.getData();
                        $scope.GetSequence();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
        }
    };
   

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + $scope.ModelNew.Id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $scope.GetSequence();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.ModelNew = {
            Sequence: seq,
              Id: null
            , UserName: null
            , Particular: null
            , ProcessId: null
            , Remarks: null
        };
    }
}