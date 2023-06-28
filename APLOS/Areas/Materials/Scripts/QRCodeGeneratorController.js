'use strict';
QRCodeGeneratorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function QRCodeGeneratorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = "QR Code Generate";
    $scope.Action = 'Save';
    $scope.characterlist = [];
    $scope.lengthCheck = false;
    $scope.index = -1;
    $scope.path = 'Materials/QRCodeGenerator/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.POList = [];
    $scope.GetPO = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPO",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.POList = response.data;

        });
    }
    $scope.GetPO();

    $scope.ArticleList = [];
    var ArticleName = null;
    $scope.GetArticle = function (args) {
        $http({
            method: 'POST',
            url: $scope.path + "GetArticle",
            data: { 'poid': args.value },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
            if ($scope.ArticleList.length == 1) {
                $scope.QRCodeGenerateModel.Article = response.data[0].Value;
                $scope.GetProductCode(response.data[0].Value);
                ArticleName = response.data[0].Text;
            }


        });
    }

    $scope.ProductCodeList = [];
    var productcodeText = null;
    $scope.GetProductCode = function (articleid) {
        $http({
            method: 'POST',
            url: $scope.path + "GetProductCode",
            data: { 'articleid': articleid },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProductCodeList = response.data;
            if ($scope.ProductCodeList.length == 1) {
                $scope.QRCodeGenerateModel.ProductCode = response.data[0].Value;
                productcodeText = response.data[0].Code;
                $scope.GetShade(response.data[0].Value);
            }


        });
    }

    $scope.ShadeList = [];
    var ShadeText = null;
    $scope.GetShade = function (prodId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetShade",
            data: { 'prodId': prodId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ShadeList = response.data;
            if ($scope.ShadeList.length == 1) {
                $scope.QRCodeGenerateModel.Shade = response.data[0].Value;
            }
            ShadeText = response.data[0].Text;

        });
    }

    $scope.NetWeightList = [];
    var NetWeightText = null;
    $scope.GetNetWeight = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetNetWeight",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.NetWeightList = response.data;
            if ($scope.NetWeightList.length == 1) {
                $scope.QRCodeGenerateModel.NetWeightId = response.data[0].Value;
                $scope.QRCodeGenerateModel.NetWeight = response.data[0].Value;
                $scope.QRCodeGenerateModel.NetWeight = $scope.QRCodeGenerateModel.NetWeight.toString();



            }
            NetWeightText = response.data[0].Text;

        });
    }


    setInterval(function () {
        $scope.GetNetWeight();
    }, 10000)

    $scope.QRCodeGeneratorTemp = {
        Id: null,
        PO: null,
        ProductCode: null,
        Article: null,
        Shade: null,
        LOT: null,
        NumberOfCones: null,
        NetWeight: null,
        NetWeightId: null,
        MinWeight: null,
        MaxWeight: null
    }
    $scope.QRCodeGenerateModel = Object.assign({}, $scope.QRCodeGeneratorTemp);

    $scope.downloadgriddataUrlPath = 'GridReports/PPTFileDownLoad';
    $scope.SendDataToGenerateQR = function () {

        try {
            if ($scope.QRCodeGenerateModel.MinWeight > $scope.QRCodeGenerateModel.MaxWeight) {
                throw "Max weight should greater than min weight. ";
            }

            $scope.fileName = "QRCode.pptx";
            $http({
                method: 'POST',
                url: $scope.path + "GenerateQRCode",
                data: {
                    'data': $scope.QRCodeGenerateModel,
                    'ShadeText': ShadeText,
                    'ArticleName': ArticleName,
                    'productcodeText': productcodeText,
                    'NetWeightText': NetWeightText
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    $rootScope.report($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);//downloadgriddataUrlPath
                    ShowResult(response.data.Message, 'success');
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
}