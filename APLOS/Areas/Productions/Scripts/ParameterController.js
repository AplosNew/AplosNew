//#region Lib
'use strict';
ParameterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ParameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Parameter';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/Parameter/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //#endregion Lib

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    //  #region Get Fun

    //  #region All Get
    $scope.getData = function () {
        $http.get('Productions/Parameter/GetList')
            .then(
                function successCallback(response) {
                    $scope.ModelList = response.data;
                    ClearFields(response.data.Sequence);
                    $scope.GetSequence();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    //$scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };

    //  #endregion All Get
    $scope.ProcessList = [];
    $scope.getProcess = function () {
        $http.get('Productions/ParameterMaster/GetProcess')
            .then(
                function successCallback(response) {
                    $scope.ProcessList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.getProcess();

    $scope.ParameterMasterList = [];
    $scope.getParameterMaster = function () {
        $http.get('Productions/Parameter/GetParameterMaster')
            .then(
                function successCallback(response) {
                    $scope.ParameterMasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.getParameterMaster();

    $scope.MachineMasterList = [];
    $scope.GetMachineMaster = function () {
        $http.get('Productions/Parameter/GetMachineMaster')
            .then(
                function successCallback(response) {
                    $scope.MachineMasterList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.GetMachineMaster();
    //  #endregion Get Fun

    //  #region UOM
    $scope.UOMList = [];
    $scope.getUOM = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/MedicineMaster/getUOM',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        })
    }
    $scope.getUOM();

    $scope.doubleClkUOM = function (e) {
        $scope.ModelNew.UOMName = e.data.StandardName;
        $scope.ModelNew.UOMId = e.data.Id;
        $scope.closeUOMPopUp();
    }

    $scope.openUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUpId')).modal('show');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUpId')).modal('hide');
    }

    $scope.searchByUOM = "UserName";
    $scope.searchUM = "";

    $scope.UOMSearchByList = [
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];


    $scope.searchUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'searchUOM',
            data: { column: $scope.searchByUOM, value: $scope.searchUM },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        });
    }
    //  #endregion UOM

    //#region List object
    $scope.ModelTemp = {
        Id: null,
        ProcessId: null,
        MachineMasterId: null,
        ParameterId: null,
        ProcessCategory: null,
        CriticalLevel: null,
        UOMId: null,
        UOMName:null,
        PeriodQuality: null,
        Frequency: null,
        QA: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    // #region Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
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
    };
    // #endregion Save

}