'use strict';
QRCodeGeneratorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$controller'];
function QRCodeGeneratorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $controller) {
    $rootScope.title = "QR Code Generate";
    $scope.Action = 'Save';
    $scope.characterlist = [];
    $scope.lengthCheck = false;
    $scope.index = -1;
    $scope.partyType = 'Vendor';
    $scope.path = 'Materials/QRCodeGenerator/';
    
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelTemp = {
        PartyCode: null,
        PartyName: null,
        CustomerId: null,
        PO: null,
        Portno: null,
        GrossWeight:null
    }
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ClearPO = function () {
        ClearPOFields();
        return true;
    }
    function ClearPOFields() {
        $scope.ModelNew = {
            PartyCode: null,
            PartyName: null,
            CustomerId: null
        }
    }

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };


    $scope.productNew = Object.assign({}, $scope.product);
    $scope.partyList = [];


    // CLOSE PARTY POP UP
    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.PartyName = party.UserName;
        $scope.ModelNew.CustomerId = party.Id;
        $scope.hidePartyPopUp();
        $scope.LoadGrid();
       
    };

    $scope.DataList = [];
    $scope.LoadGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'LoadGrid',
            data: {
                'customerId': $scope.ModelNew.CustomerId,
                'poid': $scope.ModelNew.POId
            },
            dataType: 'JSON'
        })
            .then(function successCallback(res) {
                $scope.DataList = res.data;
            });
    }

    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEntity',
            dataType:'JSON'
        })
            .then(function successCallback(res) {
                $scope.EntityList = res.data;
            });
    }
    $scope.GetEntity();

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

    $scope.Get = function (args) {
        angular.element(document.querySelector('#weighingmachinepopup')).modal('show');
        $scope.QRCodeGenerateModel = Object.assign({}, args.data);
        //$scope.GetWeighingScale();
        //$scope.GetGrossWeight();
        $scope.GetPort();
    }

    $scope.WeighingScaleList = [];
    $scope.GetWeighingScale = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetWeighingScale",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WeighingScaleList = response.data;

        });
    }

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

    $scope.CalcNetWeight = function () {
        var netwt = $scope.QRCodeGenerateModel.GrossWeight - $scope.QRCodeGenerateModel.TierWeight;
        $scope.QRCodeGenerateModel.NetWeight = netwt;
    }

    $scope.GrossWeightList = [];
    var GrossWeightText = null;
    $scope.GetGrossWeight = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + "GetGrossWeight",
            data: {'mno':x},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.GrossWeightList = response.data;
            if ($scope.GrossWeightList.length == 1) {
                $scope.QRCodeGenerateModel.GrossWeightId = response.data[0].Value;
                $scope.QRCodeGenerateModel.GrossWeight = response.data[0].Text;
                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.toString();



            }
            GrossWeightText = response.data[0].Text;

        });
    }

    //setInterval(function () {
    //    $scope.GetGrossWeight();
    //}, 10000)

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
        GrossWeight: null,
        GrossWeightId: null,
        TierWeight:null,
        WeighingScaleNo:null,
        MinWeight: null,
        MaxWeight: null,
        Portno:null
    }
    $scope.QRCodeGenerateModel = Object.assign({}, $scope.QRCodeGeneratorTemp);

    $scope.validateMaxWeight = function () {
        if ($scope.QRCodeGenerateModel.MinWeight > $scope.QRCodeGenerateModel.MaxWeight) {
            ShowResult("Max weight should greater than min weight. ");
            throw "Max weight should greater than min weight. ";
        }
    }

    $scope.validateNetWeight = function () {
        if ($scope.QRCodeGenerateModel.NetWeight >= $scope.QRCodeGenerateModel.MinWeight && $scope.QRCodeGenerateModel.NetWeight <= $scope.QRCodeGenerateModel.MaxWeight) {
            ShowResult("Net weight should be between min or max weight. ");
            throw "Net weight should be between min or max weight. ";
        }
    }

    $scope.downloadgriddataUrlPath = 'GridReports/PPTFileDownLoad';
    $scope.SendDataToGenerateQR = function () {

        try {
            if ($scope.QRCodeGenerateModel.MinWeight > $scope.QRCodeGenerateModel.MaxWeight) {
                ShowResult("Max weight should greater than min weight. ");
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

    $scope.PortNoList = [];
    $scope.GetPort = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPort',
            dataType: 'JSON'

        })
            .then(function successCalback(res) {
               $scope.PortNoList = res.data;
                //$scope.ModelNew.Portno = res.data;
            })
    }
    $scope.GetPort();

    $scope.ConnectPortConnection = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Connect',
            dataType: 'JSON'
        })
            .then(function successCalback(res) {
                $scope.QRCodeGenerateModel.GrossWeight = res.data;
                
            })
    }
}