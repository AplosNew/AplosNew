'use strict';
MenuFrameController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter','$sce'];
function MenuFrameController(commonMessage, $scope, $rootScope, baseService, $http, $filter, $sce) {
    $rootScope.title = "Menu Frame";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.menuFrames = [];
    $scope.path = 'Menus/menuFrame/';
    $scope.getListUrl = $scope.path + 'getmenuframeList';
    //baseService.init($scope.getListUrl);
    $scope.ModelList = [];
    $scope.svgImage = null;


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.getListUrl,
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i]["Image"] = $sce.trustAsHtml(response.data[i]["Image"]);
            }
            $scope.ModelList = response.data;

        });
    };
    $scope.getData();

    $scope.model = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Image: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.menuFrame = Object.assign({}, $scope.model);

    $scope.GetSequence = function () {
        $http.get('Menus/menuframe/getautosequence')
            .then(function (response) {
                $scope.menuFrame.Sequence = response.data;
            });
    };
    $scope.GetSequence();


    $scope.loadImage = function () {
        $scope.svgImage = $sce.trustAsHtml($scope.menuFrame.Image);
    };

    $scope.loadImageFromGrid = function () {
        $scope.svgImage = $scope.menuFrame.Image;
    };

    $scope.Get = function (args) {

        $scope.menuFrame = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.loadImageFromGrid();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.menuFrameForm.$valid) {
            $http({
                method: 'POST',
                url: 'Menus/menuframe/Create',
                data: { 'data': $scope.menuFrame },
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
            };
        }
    };


    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.menuFrame.Id)) {
            $http({
                method: 'POST',
                url: 'Menus/menuframe/delete/' + $scope.menuFrame.Id,
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
        $scope.model = {
            Id: null,
            Sequence: null,
            Code: null,
            ShortName: null,
            StandardName: null,
            UserName: null,
            Description: null,
            Remarks: null,
            Image: null,
            Active: true,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedDate: null
        };
        $scope.menuFrame = Object.assign({}, $scope.model);
        $scope.menuFrame.Sequence = seq;
        $scope.menuFrame.Active = true;
        $scope.svgImage = null;
    }


}