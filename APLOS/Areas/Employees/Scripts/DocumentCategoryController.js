'use strict';
DocumentCategoryController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function DocumentCategoryController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Document Category";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.DocumentCategoryList = [];
    $scope.path = 'employees/DocumentCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName"); 
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.DocumentCategoryList = response.data;
        });
    }
    $scope.getData();

    $scope.document = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.documentNew = Object.assign({}, $scope.document);

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.documentNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.documentNew = Object.assign({}, args.data);
        /* $scope.GetActivity(args.data.Id);*/
        /*$scope.getActivityGridData($scope.ModelNew.Id);*/
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'data': $scope.documentNew},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                /* ClearFields(response.data.Sequence);*/
                $scope.getData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.documentNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.documentNew.Id,
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
        $scope.Action = "Save";
        $scope.document = {};
        $scope.documentNew = {};
        $scope.documentNew.Sequence = seq;
        $scope.documentNew.Active = true;
    }
}