'use strict';
ComplaintController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ComplaintController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Complaint Master';
    $scope.path = 'QMS/Complaint/';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.ModelList = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelNewTemp = {
        Id: null,
        Sequence: 0,
        Code:null,
        ShortName:null,
        UserName: null,
        StandardName: null,
        Remark: null,
        Active: true
    }
    $scope.ModelNew = Object.assign({}, $scope.ModelNewTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelNewTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'data': $scope.ModelNew,

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
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
        $scope.Action = 'Save';

        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            Code: null,
            ShortName: null,
            UserName: null,
            StandardName: null,
            Remark: null,
            Active: true
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
    // ---------------------------------------------------------------------------------------------------------

    // #region StatusMaster
    $scope.ActionStatus = 'Save';
    $scope.getStatusSeqUrl = $scope.path + 'GetStatusSequence';

    $scope.GetStatusSequence = function () {
        cboService.getSequence($scope.getStatusSeqUrl, function (data) {
            $scope.ModelStatusTemp.Sequence = data;
            $scope.ModelStatusNew.Sequence = data;
        });
    };
    $scope.GetStatusSequence();

    $scope.GetStatus = function (args) {
        $scope.ModelStatusNew = Object.assign({}, args.data);
        $scope.ActionStatus = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ModelStatusList = [];
    $scope.getStatusData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetStatusList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelStatusList = response.data;
           
            $scope.GetStatusSequence();
        });
    }
    $scope.getStatusData();

    $scope.ModelStatusTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        UserName: null,
        StandardName: null,
        Remark: null,
        Active: true
    }
    $scope.ModelStatusNew = Object.assign({}, $scope.ModelStatusTemp);

    $scope.SaveStatus = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.ModelStatusNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveStatus',
                data: {
                    'data': $scope.ModelStatusNew,

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearStatusFields(response.data.Sequence);
                    //$scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteStatus = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelStatusNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteStatus' + $scope.ModelStatusNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearStatusFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.ClearStatus = function () {
        ClearStatusFields($scope.GetSequence());
        return true;
    };

    function ClearStatusFields(seq) {
        $scope.ActionStatus = 'Save';

        $scope.ModelStatusNew = {
            Id: null,
            Sequence: 0,
            Code: null,
            ShortName: null,
            UserName: null,
            StandardName: null,
            Remark: null,
            Active: true
        };
        $scope.ModelStatusNew = Object.assign({}, $scope.ModelStatusNew);
        $scope.ModelStatusNew.Sequence = seq;
    }
    
    // #endregion StatusMaster

}