'use strict';
DocumentLocationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DocumentLocationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Document Location';
    $scope.ModelList = [];
    $scope.DslModelList = [];
    $scope.path = 'qms/documentlocation/';

    $scope.getListUrl = $scope.path + 'getlist';
    $scope.dslgetListUrl = $scope.path + 'getlistdsl';

    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getSeqUrldsl = $scope.path + 'getautosequencedsl';

    $scope.saveUrl = $scope.path + 'create';
    $scope.dslsaveUrl = $scope.path + 'createdsl';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.dsldeleteUrl = $scope.path + 'deletedsl/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.dslsearchBy = "UserName"; $scope.dslsearch = "";

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.dslsearchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.GetSequenceDsl($scope.ModelNew.Id);
        $scope.getDataDsl($scope.ModelNew.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

        });
    }

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
                    $scope.ModelNew = response.data.Data;
                    $scope.getDataDsl($scope.ModelNew.Id);
                    $scope.Action = 'Update';
                    $scope.Getgrid();
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
        $scope.DslModelList = [];
        $scope.GetSequenceDsl($scope.ModelNew.Id);
        $scope.getDataDsl($scope.ModelNew.Id);
    }


    // pop up

    $scope.Popup = function () {
        angular.element(document.querySelector('#SublocationPoUp')).modal('show');
    }

    $scope.CloseProcess = function () {
        ClearFieldsDsl($scope.GetSequenceDsl($scope.ModelNew.Id));
        angular.element(document.querySelector('#SublocationPoUp')).modal('hide');
    }

    // Document Sub location
    $scope.DslModelList = [];
    $scope.getDataDsl = function (DocumentLocationId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetListDsl?DocumentLocationId=" + DocumentLocationId,
            data: { column: $scope.dslsearchBy, value: $scope.dslsearch },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DslModelList = response.data;

        });
    }

    $scope.DSLModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        DocumentLocationId: null
    };
    $scope.DSLModelNew = Object.assign({}, $scope.DSLModelTemp);


    $scope.GetSequenceDsl = function (DocumentLocationId) {
        $http.get("qms/DocumentLocation/GetAutoSequenceDsl?DocumentLocationId=" + DocumentLocationId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.DSLModelNew.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
   


    $scope.GetDsl = function (args) {
        $scope.Popup();
        $scope.DSLModelNew = Object.assign({}, args.data);
    
        $scope.ActionDSL = 'Update';

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.ActionDSL = 'Save';



    $scope.SaveDsl = function () {
        $scope.DSLModelNew.DocumentLocationId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DSLModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.dslsaveUrl,
                data: { 'data': $scope.DSLModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDsl(response.data.Sequence);
                    $scope.getDataDsl($scope.ModelNew.Id);
                    $scope.GetSequenceDsl($scope.ModelNew.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.DeleteDsl = function () {
        if (!baseService.isUndefinedOrNull($scope.DSLModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.dsldeleteUrl + $scope.DSLModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDsl(response.data.Sequence);
                    $scope.getDatadsl($scope.ModelNew.Id);
                    $scope.GetSequenceDsl($scope.ModelNew.Id);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.ClearDsl = function () {
        ClearFieldsDsl($scope.GetSequenceDsl($scope.ModelNew.Id));
        return true;
    };

    function ClearFieldsDsl(seq) {
        $scope.ActionDSL = 'Save';
        $scope.DSLModelNew = Object.assign({}, $scope.DSLModelTemp);
        $scope.DSLModelNew.Sequence = seq;
        $scope.getDataDsl($scope.ModelNew.Id);
        $scope.GetSequenceDsl($scope.ModelNew.Id);
    }
}