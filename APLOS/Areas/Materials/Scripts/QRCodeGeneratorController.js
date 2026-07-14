////'use strict';
////QRCodeGeneratorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$controller','$window'];
////function QRCodeGeneratorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $controller, $window) {
////    $rootScope.title = "QR Code Generate";
////    $scope.Action = 'Save';
////    $scope.characterlist = [];
////    $scope.lengthCheck = false;
////    $scope.index = -1;
////    $scope.partyType = 'Vendor';
////    $scope.path = 'Materials/QRCodeGenerator/';
    
////    $controller('partyBaseController', { $scope: $scope, $http: $http });

////    $scope.tab = 1;
////    $scope.setTab = function (newTab) {
////        $scope.tab = newTab;
////    };

////    $scope.isSet = function (tabNum) {
////        return $scope.tab === tabNum;
////    };

////    $scope.ModelTemp = {
////        PartyCode: null,
////        PartyName: null,
////        CustomerId: null,
////        PO: null,
////        Portno: null,
////        GrossWeight: null,
////        ByWhomId: $window.employeeId,
////        ByWhomName: $window.employeeName
////    }
////    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

////    // #region  Dynamic PopUp
////    $scope.popUpList = [];
////    $scope.popUpDataList = [];
////    $scope.GetByWhomPopupData = function () {
////        try {
          
////                $scope.popUpDataList = [];
////                $http({
////                    method: 'GET',
////                    url: 'employees/authorizationconfig/getallemployeedata'

////                }).then(function successCallback(response) {
////                    $scope.popUpDataList = response.data;
////                });
////                angular.element(document.querySelector('#EmpPopUp')).modal('show');
           
////        } catch (e) {
////            ShowResult(e, 'failure');
////        }
////    };


////    $scope.selectdblClick = function (obj) {
////        var ob = obj.data;
////        $scope.ModelNew.ByWhomId = ob.SystemId;
////        $scope.ModelNew.ByWhomName   = ob.EmployeeName;
////        $scope.ModelNew.ByWhomCode = ob.EmployeeCode;
////        angular.element(document.querySelector('#EmpPopUp')).modal('hide');
////    };

////    $scope.closePopUp = function () {
////        angular.element(document.querySelector('#EmpPopUp')).modal('hide');
////    };

////    $scope.ProductionOrderList = [];
////    $scope.PRSearchColumn = null;
////    $scope.PRSearchValue = null;
////    $scope.GetProductionOrderPopUp = function () {
////        if (!baseService.isUndefinedOrNull($scope.ModelNew.EntityId)) {
////            $http({
////                method: 'POST',
////                data: {
////                    'entityid': $scope.ModelNew.EntityId, 'processid': $scope.ModelNew.ProcessId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
////                },
////                url: 'Outsourcing/OSTransformationPO/GetProductionOredrList'
////            }).then(function successCallback(response) {
////                $scope.ProductionOrderList = response.data;
////                angular.element(document.querySelector('#POItemPopup')).modal('show');
////            });
////        }
////    };

////    function checkItemExist(list, Id) {
////        for (var i = 0; i < list.length; i++) {
////            if (list[i].POId === Id) {
////                return true;
////            }
////        }
////        return false;
////    }

////    $scope.selectedProductionOrder = [];
////    $scope.SetPrOData = function () {
////        var gridObj = $("#GridPO").data("ejGrid");
////        var data = gridObj.getSelectedRecords()[0];
////        if (checkItemExist($scope.selectedProductionOrder, data.POId) === false) {
////            $scope.selectedProductionOrder.push(data);
////        }
////    }

////    // #endregion



////    $scope.ClearPO = function () {
////        ClearPOFields();
////        return true;
////    }
////    function ClearPOFields() {
////        $scope.ModelNew = {
////            PartyCode: null,
////            PartyName: null,
////            CustomerId: null
////        }
////    }

////    $scope.partyParameters = {
////        limit: 10
////        , offset: 0
////        , order: 'ASC'
////        , sort: 'UserName, PartyAccountGroupName'
////        , searchBy: 'UserName'
////        , pageSize: 10
////        , total_count: 0
////        , search: null
////        , serverPagination: true
////    };


////    $scope.productNew = Object.assign({}, $scope.product);
////    $scope.partyList = [];


////    // CLOSE PARTY POP UP
////    $scope.closePartyPopUp = function (x) {
////        var party = x.data;

////        $scope.ModelNew.PartyCode = party.Code;
////        $scope.ModelNew.PartyName = party.UserName;
////        $scope.ModelNew.CustomerId = party.Id;
////        $scope.hidePartyPopUp();
////        $scope.LoadGrid();
       
////    };

////    $scope.DataList = [];
////    $scope.LoadGrid = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + 'LoadGrid',
////            data: {
////                'customerId': $scope.ModelNew.CustomerId,
////                'poid': $scope.ModelNew.PO
////            },
////            dataType: 'JSON'
////        })
////            .then(function successCallback(res) {
////                $scope.DataList = res.data;
////            });
////    }

////    $scope.EntityList = [];
////    $scope.getAllEntities = function () {
////        $http({
////            method: 'POST',
////            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
////        }).then(function successCallback(response) {
////            $scope.EntityList = response.data;
////            if (baseService.arrayLength(response.data) === 1) {
////                $scope.ModelNew.EntityId = $scope.EntityList[0].Value;
////                //default
////                $scope.loadProcessList($scope.ModelNew.EntityId);
////            }
////        });
////    };
////    $scope.getAllEntities();


////    $scope.loadProcessList = function (entityid) {
////        cboService.GetEntityProcessCbo(entityid, function (result) {
////            $scope.processList = result;
////            if (baseService.arrayLength(result) === 1) {
////                $scope.ModelNew.ProcessId = $scope.processList[0].Value;
////            }
////        });
////    };


////    $scope.POList = [];
////    $scope.GetPO = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetPO",
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.POList = response.data;

////        });
////    }
////    $scope.GetPO();

////    $scope.SelectedRowOB = {};
////    $scope.SelectedRowList = [];
////    $scope.Get = function (args) {
       
////        $scope.SelectedRowOB.Article = args.data.Article;
        
////        $scope.SelectedRowOB.ArticleId = args.data.ArticleId;
       
////        $scope.SelectedRowOB.Id = args.data.Id;
        
////        $scope.SelectedRowOB.PO = args.data.PO;
        
////        $scope.SelectedRowOB.ProductCode = args.data.ProductCode;
       
////        $scope.SelectedRowOB.ProductionStatus = args.data.ProductionStatus;
       
////        $scope.SelectedRowOB.Shade = args.data.Shade;

////        $scope.SelectedRowList.push($scope.SelectedRowOB);
       
////        angular.element(document.querySelector('#weighingmachinepopup')).modal('show');
////        $scope.QRCodeGenerateModel = Object.assign({}, args.data);
////        //$scope.GetWeighingScale();
////        //$scope.GetGrossWeight();
////        $scope.GetPort();
////    }

////    $scope.CreateAnotherRows = function () {

////        for (var i = 0; i < 1; i++) {
////            var obj = angular.copy($scope.SelectedRowList[0]);
////            obj.MaxWeight = null;
////            obj.MinWeight = null;
////            obj.LOT = null;
////            obj.NoOfPackets = null;
////            obj.NetWeight = null;
////            obj.GrossWeight = null;
////            obj.TierWeight = null;
////            $scope.SelectedRowList.push(obj);
            
////        }

////    }

////    $scope.WeighingScaleList = [];
////    $scope.GetWeighingScale = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetWeighingScale",
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.WeighingScaleList = response.data;

////        });
////    }

////    $scope.ArticleList = [];
////    var ArticleName = null;
////    $scope.GetArticle = function (args) {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetArticle",
////            data: { 'poid': args.value },
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.ArticleList = response.data;
////            if ($scope.ArticleList.length == 1) {
////                $scope.QRCodeGenerateModel.Article = response.data[0].Value;
////                $scope.GetProductCode(response.data[0].Value);
////                ArticleName = response.data[0].Text;
////            }


////        });
////    }

////    $scope.ProductCodeList = [];
////    var productcodeText = null;
////    $scope.GetProductCode = function (articleid) {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetProductCode",
////            data: { 'articleid': articleid },
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.ProductCodeList = response.data;
////            if ($scope.ProductCodeList.length == 1) {
////                $scope.QRCodeGenerateModel.ProductCode = response.data[0].Value;
////                productcodeText = response.data[0].Code;
////                $scope.GetShade(response.data[0].Value);
////            }


////        });
////    }

////    $scope.ShadeList = [];
////    var ShadeText = null;
////    $scope.GetShade = function (prodId) {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetShade",
////            data: { 'prodId': prodId },
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.ShadeList = response.data;
////            if ($scope.ShadeList.length == 1) {
////                $scope.QRCodeGenerateModel.Shade = response.data[0].Value;
////            }
////            ShadeText = response.data[0].Text;

////        });
////    }

////    $scope.NetWeightList = [];
////    var NetWeightText = null;
////    $scope.GetNetWeight = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetNetWeight",

////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.NetWeightList = response.data;
////            if ($scope.NetWeightList.length == 1) {
////                $scope.QRCodeGenerateModel.NetWeightId = response.data[0].Value;
////                $scope.QRCodeGenerateModel.NetWeight = response.data[0].Value;
////                $scope.QRCodeGenerateModel.NetWeight = $scope.QRCodeGenerateModel.NetWeight.toString();



////            }
////            NetWeightText = response.data[0].Text;

////        });
////    }

////    $scope.CalcNetWeight = function () {
////        var netwt = $scope.QRCodeGenerateModel.GrossWeight - $scope.QRCodeGenerateModel.TierWeight;
////        $scope.QRCodeGenerateModel.NetWeight = netwt;
////    }

////    $scope.GrossWeightList = [];
////    var GrossWeightText = null;
////    $scope.GetGrossWeight = function (x) {
////        $http({
////            method: 'POST',
////            url: $scope.path + "GetGrossWeight",
////            data: {'mno':x},
////            dataType: 'JSON'
////        }).then(function successCallback(response) {
////            $scope.GrossWeightList = response.data;
////            if ($scope.GrossWeightList.length == 1) {
////                $scope.QRCodeGenerateModel.GrossWeightId = response.data[0].Value;
////                $scope.QRCodeGenerateModel.GrossWeight = response.data[0].Text;
////                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.toString();



////            }
////            GrossWeightText = response.data[0].Text;

////        });
////    }

    

////    $scope.QRCodeGeneratorTemp = {
////        Id: null,
////        PO: null,
////        ProductCode: null,
////        Article: null,
////        Shade: null,
////        LOT: null,
////        NumberOfCones: null,
////        NetWeight: null,
////        NetWeightId: null,
////        GrossWeight: 0.00,
////        GrossWeightId: null,
////        TierWeight:null,
////        WeighingScaleNo:null,
////        MinWeight: null,
////        MaxWeight: null,
////        Portno:null
////    }
////    $scope.QRCodeGenerateModel = Object.assign({}, $scope.QRCodeGeneratorTemp);

////    $scope.validateMaxWeight = function () {
////        if ($scope.QRCodeGenerateModel.MinWeight > $scope.QRCodeGenerateModel.MaxWeight) {
////            ShowResult("Max weight should greater than min weight. ");
////            throw "Max weight should greater than min weight. ";
////        }
////    }

////    $scope.validateNetWeight = function () {
////        if ($scope.QRCodeGenerateModel.NetWeight >= $scope.QRCodeGenerateModel.MinWeight && $scope.QRCodeGenerateModel.NetWeight <= $scope.QRCodeGenerateModel.MaxWeight) {
////            ShowResult("Net weight should be between min or max weight. ");
////            throw "Net weight should be between min or max weight. ";
////        }
////    }

////    //$scope.downloadgriddataUrlPath = 'GridReports/PPTFileDownLoad';
////    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
////    $scope.FN = null;
////    $scope.SendDataToGenerateQR = function () {

////        try {
////            if ($scope.QRCodeGenerateModel.MinWeight > $scope.QRCodeGenerateModel.MaxWeight) {
////                ShowResult("Max weight should greater than min weight. ");
////                throw "Max weight should greater than min weight. ";
////            }

////            $scope.fileName = "QRCode.pptx";
////            $http({
////                method: 'POST',
////                url: $scope.path + "GenerateQRCode",
////                data: {
////                    'data': $scope.QRCodeGenerateModel,
////                    'ShadeText': ShadeText,
////                    'ArticleName': ArticleName,
////                    'productcodeText': productcodeText,
////                    'NetWeightText': NetWeightText
////                },
////                dataType: 'JSON'
////            }).then(function successCallback(response) {
////                if (response.data.Error === true) {
////                    ShowResult(response.data.Message, 'failure');
////                }
////                else {

////                    //$rootScope.report($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);//downloadgriddataUrlPath
////                   // $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
////                    $scope.FN = virtualPath.QRPdfDocument + "QRCode.pdf";
////                    ShowResult(response.data.Message, 'success');
////                    $scope.QRCodeGenerateModel.Id = response.data.Id;
////                }

////            }), function errorCallBack(response) {
////                ShowResult(response.data.Message, 'failure');

////            }
////        } catch (e) {
////            ShowResult(e, 'failure');
////        }
////    }

////    $scope.PortNoList = [];
////    $scope.GetPort = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + 'GetPort',
////            dataType: 'JSON'

////        })
////            .then(function successCalback(res) {
////               $scope.PortNoList = res.data;
////                //$scope.ModelNew.Portno = res.data;
////            })
////    }
////    $scope.GetPort();

////    $scope.ConnectPortConnection = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + 'Connect',
////            dataType: 'JSON'
////        })
////            .then(function successCalback(res) {
////                $scope.QRCodeGenerateModel.GrossWeight = res.data;
////                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.substring(1)
                
////            })
////    }

////    $scope.GetGrossWeightByWeighingScale = function () {
////        $scope.QRCodeGenerateModel.GrossWeight = null;
////        $http({
////            method: 'POST',
////            url: $scope.path + 'Read',
////            dataType: 'JSON'
////        })
////            .then(function successCalback(res) {
////                $scope.QRCodeGenerateModel.GrossWeight = res.data;
////                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.substring(0, 1)

////            })
////    }

////    var checkConnection = false;
////    $scope.CheckConnection = function () {
////        $http({
////            method: 'POST',
////            url: $scope.path + 'PassConnection',
////            dataType: 'JSON'
////        })
////            .then(function successCallback(res) {
////                checkConnection = res.data;
////                if (checkConnection == 'True')
////                    $scope.GetGrossWeightByWeighingScale();
////            })
////    }

////    $scope.RefereshGrossWeight = function () {
////        $scope.QRCodeGenerateModel.GrossWeight = null;
////        $scope.ConnectPortConnection();
////    }

////    // Read data auto from weighing scale on every 1sec.
    
////    setInterval(function () {
////        if (!baseService.isUndefinedOrNull($scope.QRCodeGenerateModel.Portno)) {
////            $scope.RefereshGrossWeight();
            
////        }
        
////    }, 10000)

////    $scope.generateQRCode = function () {
////        angular.element(document.querySelector('#PreviewScreenpopup')).modal('show');
////        //let website = document.getElementById("website").value;
////        let PO = document.getElementById("po").value;
////        let productcode = document.getElementById("productcode").value;
////        let noofpackets = document.getElementById("noofpackets").value;
////        let article = document.getElementById("article").value;
////        let shade = document.getElementById("shade").value;
////        let netwt = document.getElementById("netwt").value;
////        let lot = document.getElementById("lot").value;

////        let contStr = PO.concat('#', productcode, '#', noofpackets, '#', article, '#', shade, '#', netwt, '#', lot);
////        if (contStr) {
////            let qrcodeContainer = document.getElementById("qrcode");
////            document.getElementById("poText").innerHTML = "PO :  " + PO;
////            document.getElementById("productcodeText").innerHTML = "PROD. CODE :  " + productcode;
////            document.getElementById("noofpacketsText").innerHTML = "NO. OF PACKETS :  " + noofpackets;
////            document.getElementById("articleText").innerHTML = "ARTICLE :  " + article;
////            document.getElementById("shadeText").innerHTML = "SHADE :  " + shade;
////            document.getElementById("netwtText").innerHTML = "NET WT. :  " + netwt;
////            document.getElementById("lotText").innerHTML = "LOT :  " + lot;
////            qrcodeContainer.innerHTML = "";
////            new QRCode(qrcodeContainer, contStr);
////            /*With some styles*/
////            let qrcodeContainer2 = document.getElementById("qrcode");
////               qrcodeContainer2.innerHTML = "";
////               new QRCode(qrcodeContainer2, {
////                 text: contStr,
////                 width: 100,
////                 height: 100,                 
////                 margin:"auto",
////                 //colorDark: "#5868bf",
////                 colorDark: "#000",
////                 //colorLight: "#ffffff",
////                 correctLevel: QRCode.CorrectLevel.H
                 
////               });
////            document.getElementById("qrcode-container").style.display = "block";
            
////        } else {
////            alert("Please enter a valid URL");
////        }

////        // window.print()
////    }

////    $scope.getPrint = function (divName) {
        
////        const printSection = document.getElementById("qrcode").innerHTML;

////        let PO = document.getElementById("po").value;
////        let productcode = document.getElementById("productcode").value;
////        let noofpackets = document.getElementById("noofpackets").value;
////        let article = document.getElementById("article").value;
////        let shade = document.getElementById("shade").value;
////        let netwt = document.getElementById("netwt").value;
////        let grosswt = document.getElementById("grossweight").value;
////        let lot = document.getElementById("lot").value;

////        let POText = document.getElementById("poText").innerHTML = "PO :  " + PO;
////        let productcodeText = document.getElementById("productcodeText").innerHTML = "PROD. CODE :  " + productcode;
////        let noofpacketsText = document.getElementById("noofpacketsText").innerHTML = "NO. OF PACKETS :  " + noofpackets;
////        let articleText = document.getElementById("articleText").innerHTML = "ARTICLE :  " + article;
////        let shadeText = document.getElementById("shadeText").innerHTML = "SHADE :  " + shade;
////        let netwtText = document.getElementById("netwtText").innerHTML = "NET WT. :  " + netwt;
////        let grossText = document.getElementById("grosswtText").innerHTML = "GROSS WT. :  " + grosswt;
////        let lotText = document.getElementById("lotText").innerHTML = "LOT :  " + lot;

      
////        let view = window.open();

////        view.document.write('<div class="align-center" style="margin-left:auto !important">' + printSection + '</div>'
////            , '<p id="productText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + productcodeText + '</p>'
////            , '<p id="poText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px; font-size:15px">' + POText + '</p>'
////            , '<p id="lotText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + lotText + '</p>'
////            , '<p id="noofconesText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + noofpacketsText + '</p>'
////            , '<p id="netwtText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + netwtText + '</p>'
////            , '<p id="grosswtText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + grossText + '</p>'
////            , '<p id="shadeText" class="font-black font-weight txt-block" style="line-height:.1; margin-bottom:20px">' + shadeText + '</p>'
////            , '<p id="articleText" class="font-black font-weight txt-block" style="line-height:.7; margin-bottom:20px">' + articleText + '</p>'
            
            
            
            
////        );
        
////        view.print();
////        view.close();
////    }

   
////}


'use strict';
QRCodeGeneratorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$controller', '$window'];
function QRCodeGeneratorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $controller, $window) {
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
        GrossWeight: null
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
                'poid': $scope.ModelNew.PO
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
            dataType: 'JSON'
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

    $scope.SelectedRowOB = {};
    $scope.SelectedRowList = [];
    $scope.Get = function (args) {

        $scope.SelectedRowOB.Article = args.data.Article;

        $scope.SelectedRowOB.ArticleId = args.data.ArticleId;

        $scope.SelectedRowOB.Id = args.data.Id;

        $scope.SelectedRowOB.PO = args.data.PO;

        $scope.SelectedRowOB.ProductCode = args.data.ProductCode;

        $scope.SelectedRowOB.ProductionStatus = args.data.ProductionStatus;

        $scope.SelectedRowOB.Shade = args.data.Shade;

        $scope.SelectedRowList.push($scope.SelectedRowOB);

        angular.element(document.querySelector('#weighingmachinepopup')).modal('show');
        $scope.QRCodeGenerateModel = Object.assign({}, args.data);
        //$scope.GetWeighingScale();
        //$scope.GetGrossWeight();
        $scope.GetPort();
    }

    $scope.CreateAnotherRows = function () {

        for (var i = 0; i < 1; i++) {
            var obj = angular.copy($scope.SelectedRowList[0]);
            obj.MaxWeight = null;
            obj.MinWeight = null;
            obj.LOT = null;
            obj.NoOfPackets = null;
            obj.NetWeight = null;
            obj.GrossWeight = null;
            obj.TierWeight = null;
            $scope.SelectedRowList.push(obj);

        }

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
            data: { 'mno': x },
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
        GrossWeight: 0.00,
        GrossWeightId: null,
        TierWeight: null,
        WeighingScaleNo: null,
        MinWeight: null,
        MaxWeight: null,
        Portno: null
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
    $scope.FN = null;
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

                    //$rootScope.report($scope.downloadgriddataUrlPath + "?FileName=" + response.data.FileName);//downloadgriddataUrlPath
                    // $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $scope.FN = $scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName;
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
                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.substring(1)

            })
    }

    $scope.GetGrossWeightByWeighingScale = function () {
        $scope.QRCodeGenerateModel.GrossWeight = null;
        $http({
            method: 'POST',
            url: $scope.path + 'Read',
            dataType: 'JSON'
        })
            .then(function successCalback(res) {
                $scope.QRCodeGenerateModel.GrossWeight = res.data;
                $scope.QRCodeGenerateModel.GrossWeight = $scope.QRCodeGenerateModel.GrossWeight.substring(0, 1)

            })
    }

    var checkConnection = false;
    $scope.CheckConnection = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'PassConnection',
            dataType: 'JSON'
        })
            .then(function successCallback(res) {
                checkConnection = res.data;
                if (checkConnection == 'True')
                    $scope.GetGrossWeightByWeighingScale();
            })
    }

    $scope.RefereshGrossWeight = function () {
        $scope.QRCodeGenerateModel.GrossWeight = null;
        $scope.ConnectPortConnection();
    }

    // Read data auto from weighing scale on every 1sec.

    setInterval(function () {
        if (!baseService.isUndefinedOrNull($scope.QRCodeGenerateModel.Portno)) {
            $scope.RefereshGrossWeight();

        }

    }, 10000)


}