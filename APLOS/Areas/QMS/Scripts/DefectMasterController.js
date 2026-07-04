'use strict';
DefectMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DefectMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Defect Master';
    $rootScope.titleDP = 'Defect Point';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.getSeqUrl = $scope.path + 'getdefectmasterautosequence';
    $scope.saveUrl = $scope.path + 'createdefectmaster';
    $scope.deleteUrl = $scope.path + 'deletedefectmaster/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "DefectNames"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'DefectCode', name: "DefectCode" }, { value: 'DefectNames', name: "DefectNames" }, { value: 'Remarks', name: "Remarks" }];


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDefectMasterList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        SrNo: null,
        DefectCategory: null,
        DefectCode: null,
        Remarks: null,
        DefectNames: null,
        DefectsLocalName: null,
        ProcessId: null,
        QualityProcessId: null,
        TypesOfDefects: null,
        Zone: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.processList = [];
    $http({
        method: 'GET',
        url: "QMS/QualityProcess/GetProcessCbo",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.processList = response.data;

    });



    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.SrNo = data;
            $scope.ModelNew.SrNo = data;
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

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
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
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}