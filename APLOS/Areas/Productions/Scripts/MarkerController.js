'use strict';
MarkerController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function MarkerController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Marker';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/Marker/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    //#region Finishing Goods & Articale
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.getMaterial = function (index) {

        $scope.materialType = 'ProductDefinition';
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();


    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        $scope.getArticleSearchList($scope.ModelNew.FGMaterialMasterId);
    };

    $scope.FGmsg = null;
    $scope.selectMaterialByType = function (ob) {
        try {
            $scope.FGMId = $scope.ModelNew.FGMaterialMasterId;

            $http({
                method: 'GET',
                url: 'OrderManagements/BOMMaster/GetBOMSKUMappingDataForValidation?BOMMasterId=' + $scope.ModelNew.Id
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data) > 0 && $scope.FGMId !== ob.Id) {
                    ShowResult("As this Finish Goods has Matrix level SKU, so Finish Goods change is not acceptable.", 'failure');
                }
                else {
                    $scope.ModelNew.FGMaterialMasterId = ob.Id;
                    $scope.ModelNew.FGMaterialMaster = ob.UserName;
                    $scope.ModelNew.ProductMasterName = ob.ProductMasterName;
                    $scope.ModelNew.FGArticleId = null;
                    $scope.ModelNew.FGArticle = null;
                    $scope.ModelNew.HasAttribute = ob.HasAttribute;
                    $scope.ModelNew.WithSKU = ob.WithSKU;
                    if ($scope.ModelNew.HasAttribute) {
                        $scope.materialType = null;
                        $scope.getArticleSearchList(ob.Id);
                    } else {
                        $scope.closeMaterialMasterbyTypePopUp();
                        return ShowResult('This material has no attribute', 'failure');
                    }
                    if ($scope.ModelNew.WithSKU) {
                        $scope.FGmsg = "has";
                    } else {
                        $scope.FGmsg = "has no";
                    }
                    $scope.getFGCharacteristicsList($scope.ModelNew.FGMaterialMasterId);
                    $scope.HSNCodeId = ob.HSNCodeId;
                    $scope.closeMaterialMasterbyTypePopUp();
                }
            })
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.selectarticle = function (ob) {
        try {
            $scope.ModelNew.FGMaterialMasterId = ob.MaterialMasterId;
            $scope.ModelNew.FGMaterialMaster = ob.MaterialMasterName;
            $scope.ModelNew.FGArticleId = ob.Id;
            $scope.ModelNew.FGArticle = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function () {
        $scope.ModelNew.ArticleId = null;
        $scope.ModelNew.FGArticle = null;
    };
    $scope.getFGCharacteristicsList = function (id) {
        //$scope.clearCharNames();
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
        });

    };


    $scope.getFGCharacteristicsListNew = function (id,MasterId) {
        $http({
            method: 'GET',
            url: 'Materials/MaterialMaster/getcharacteristicsbymaterialmasterid/',
            params: {
                materialMasterId: id
            }
        }).then(function (response) {
            $scope.characteristicsList = [];
            $scope.characteristicsList = response.data.charData;
            $scope.GetFGCharacteristicsValueCboAfterSave();
        });

    };
    $scope.TotalRatio = 0;
    $scope.FGCharacteristicsValueList = [];
    $scope.GetFGCharacteristicsValueCboAfterSave = function () {
        $scope.TotalRatio = 0;
        for (var i = 0; i < $scope.characteristicsList.length; i++) {
            if ($scope.ModelNew.CharacteristicsId == $scope.characteristicsList[i].Value) {
                var valueAssignmentLevel = $scope.characteristicsList[i].ValueAssignmentLevel;
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "getCharacteristicsValueByCharacteristicsIdAfterSave",
            data: { 'materialMasterId': $scope.ModelNew.FGMaterialMasterId, 'characteristicsId': $scope.ModelNew.CharacteristicsId, 'valueAssignmentLevel': valueAssignmentLevel, 'MarkerMasterId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FGCharacteristicsValueList = response.data;
            for (var i = 0; i < $scope.FGCharacteristicsValueList.length; i++) {
                if ($scope.FGCharacteristicsValueList[i].Ratio != 0 || !baseService.isUndefinedOrNull($scope.FGCharacteristicsValueList[i].Ratio)) {
                    $scope.SKUDisable = true;
                    break;
                }
                else {
                    $scope.SKUDisable = false;
                }
            }
            for (var i = 0; i < $scope.FGCharacteristicsValueList.length; i++) {
                if ($scope.FGCharacteristicsValueList[i].Ratio != 0 || !baseService.isUndefinedOrNull($scope.FGCharacteristicsValueList[i].Ratio)) {
                    $scope.TotalRatio = parseFloat($scope.FGCharacteristicsValueList[i].Ratio) + parseFloat($scope.TotalRatio);
                }
            }
        });
        if (baseService.isUndefinedOrNull($scope.ModelNew.HeaderName)) {
            $scope.HeaderName = $("#SKU option:selected").text();
        }
        else {
            $scope.HeaderName = $scope.ModelNew.HeaderName;
        }
    }

    $scope.FGCharacteristicsValueList = [];
    $scope.GetFGCharacteristicsValueCbo = function () {
        for (var i = 0; i < $scope.characteristicsList.length; i++) {
            if ($scope.ModelNew.CharacteristicsId == $scope.characteristicsList[i].Value) {
                var valueAssignmentLevel = $scope.characteristicsList[i].ValueAssignmentLevel;
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "getCharacteristicsValueByCharacteristicsId",
            data: { 'materialMasterId': $scope.ModelNew.FGMaterialMasterId, 'characteristicsId': $scope.ModelNew.CharacteristicsId, 'valueAssignmentLevel': valueAssignmentLevel },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FGCharacteristicsValueList = response.data;
        });
        $scope.HeaderName = $("#SKU option:selected").text();
    }

    //#endregion

    $scope.SKUDisable = false;

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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
        Active: true,
        FGMaterialMasterId: null,
        FGMaterialMaster: null,
        FGArticleId: null,
        FGArticle: null,
        FabricWidthId: null,
        ShrinkageGroupId: null,
        CharacteristicsId: null,
        ShadeId: null,
        Length: null,
        Attachment: null,
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
        $scope.getFGCharacteristicsListNew($scope.ModelNew.FGMaterialMasterId, $scope.ModelNew.Id);
        $scope.HeaderName = $scope.ModelNew.HeaderName;

        //$scope.filedata.name = $scope.ModelNew.Attachment;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.getDetails = function (MasterId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetDetailsList",
            data: { 'masterid': MasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FGCharacteristicsValueList = response.data;
        });
    };

    //#region File 

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };


    //#endregion

    $scope.Save = function () {
        try {

            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            if (baseService.isUndefinedOrNull(fileName))
                fileName = $scope.ModelNew.Attachment;
            $scope.ModelNew.Attachment = fileName;
            if (!baseService.isUndefinedOrNull($scope.ModelNew.Attachment)) {
                if ($scope.ModelNew.Attachment.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }
            var formData = new FormData();

            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,

                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("data", angular.toJson(data.data));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        formData.append("details", angular.toJson(data.details));
                        return formData;
                    },

                    data: { 'data': $scope.ModelNew, 'details': $scope.FGCharacteristicsValueList,'file': $scope.filedata },
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
        } catch (e) {
            ShowResult(e, 'failure');
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
            StandardName: null,
            UserName: null,
            Description: null,
            Remarks: null,
            Active: true,
            FGMaterialMasterId: null,
            FGMaterialMaster: null,
            FGArticleId: null,
            FGArticle: null,
            FabricWidthId: null,
            ShrinkageGroupId: null,
            CharacteristicsId: null,
            ShadeId: null,
            Length: null,
        };
        $scope.ModelNew.Sequence = seq;
        $scope.FGCharacteristicsValueList = [];
        $scope.characteristicsList = [];
        $scope.SKUDisable = false;
    }

    $scope.FabricWidthList = [];
    $scope.getFabricWidth = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetFabricWidth",
        }).then(function successCallback(response) {
            $scope.FabricWidthList = response.data;
        });
    }
    $scope.getFabricWidth();

    $scope.ShrinkageGroupList = [];
    $scope.getShrinkageGroup = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetShrinkageGroup",
        }).then(function successCallback(response) {
            $scope.ShrinkageGroupList = response.data;
        });
    }
    $scope.getShrinkageGroup();

    $scope.ShadeList = [];
    $scope.getShade = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetShade",
        }).then(function successCallback(response) {
            $scope.ShadeList = response.data;
        });
    }
    $scope.getShade();

}