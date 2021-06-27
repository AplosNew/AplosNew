'use strict';
plantSettingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService','fileReader'];
function plantSettingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, fileReader) {
    $rootScope.title = "Plant Setting";
    $scope.plantSettings = [];
    $scope.Action = 'Save';
    $scope.path = 'setups/plantsetting/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.plantSettings = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.plantSetting = {
        Id: null,
        Sequence: null,
        ModuleId: null,
        PlantId: null,
        ModuleName: null,
        AuthorizedSignature: null,
        Active: true
    };

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Module Name',
            'value': 'ModuleName'
        }
    ];

    $scope.plantList = [];
    cboService.getCboPlantByCompanyGroup(null, function (result) {
        $scope.plantList = result;
    });

    $scope.moduleList = [
        {
            'Text': 'HR',
            'Value': 'HR'
        },
        {
            'Text': 'ACC',
            'Value': 'ACC'
        }
    ];

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.plantSetting.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantSetting = $scope.plantSettings[$scope.index];
        $scope.imageSrc = virtualPath.AuthorizedSignature + $scope.plantSetting.AuthorizedSignature;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
 
    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.plantSettingForm.$valid) {
                var picData = new FormData();
                if ($scope.Action === "Save" || $scope.Action === "Update") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'create',
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            picData.append("plantSetting", angular.toJson(data.plantSetting));
                            if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                                picData.append('file', data.file);
                            }
                            return picData;
                        },
                        data: {
                            'plantSetting': $scope.plantSetting
                            , 'file': $scope.picdata
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            ClearFields(response.data.Sequence);
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.plantSetting.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.plantSetting.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.plantSettings.splice($scope.index, 1);
                    baseService.paginationRemove();
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
        $scope.plantSetting = {};
        $scope.plantSetting.Sequence = seq;
        $scope.plantSetting.Active = true;
        $scope.picdata = {};
        $scope.picdata = "";
        $scope.plantSetting.AuthorizedSignature = null;
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    }

}