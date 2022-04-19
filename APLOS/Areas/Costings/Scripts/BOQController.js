'use strict';
BOQController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function BOQController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'BOQ';
    $scope.path = "Costings/BOQ/";
    $scope.ModelBase = { Id: null, CustomerId: null, CustomerName: null, EmployeeSystemId: null, EmployeeName: null, Remarks: null, UserName: null };
    $scope.Model = Object.assign({}, $scope.ModelBase);
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.EditList = [];
    $scope.SelectedEdit = {};
    $scope.searchByEdit = [{ name: 'BOM Id', value: 'Id' }, { name: 'Sales Order Id', value: 'SalesOrderId' }, { name: 'Master Order Id', value: 'MasterOrderId' },
    { name: 'Buyer Order#', value: 'BuyerOrderNo' }, { name: 'Own Order#', value: 'OwnOrderNo' },
    { name: 'Buyer Item', value: 'BuyerItemNo' }, { name: 'Own Item', value: 'OwnItemNo' },
    { name: 'Resp. Person', value: 'EmployeeName' }, { name: 'User Name', value: 'UserName' },
    { name: 'Description', value: 'Description' }, { name: 'Selected Item', value: 'ItemList' },
    { name: 'Order Status', value: 'OrderStatusName' }, { name: 'Order Category', value: 'OrderCategoryName' }]; $scope.searchEdit = 'SalesOrderId'; $scope.searchEditValue = '';
    $scope.GetEditList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEditList",
            data: { column: $scope.searchEdit, value: $scope.searchEditValue },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EditList = response.data.DATA;
        });
    }
    $scope.GetEditList();

    $scope.SelectEdit = function (args) {
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.Model = Object.assign({}, args.data);
        $scope.GetItemList();
    }



    $scope.MaterialAttachmentList = [];
    $scope.MaterialQtyEditList = [];
    $scope.GetItemList = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetItemList",
            data: { CostingBOQMasterId: $scope.Model.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialAttachmentList = response.data.DATA;
        });

        $http({
            method: 'POST',
            url: $scope.path + "GetItemListForQtyEdit",
            data: { CostingBOQMasterId: $scope.Model.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MaterialQtyEditList = response.data.DATA;
        });
    }
    //$scope.GetItemList();

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['BOM'];
    $scope.SelectedMaterial = {};
    $scope.getMaterial = function (data) {
        $scope.SelectedMaterial = data;
        $scope.getMaterialMasterbyTypePopUp();
    };
    $scope.selectMaterialByType = function (ob) {

        $scope.SelectedMaterial.MaterialMasterId = ob.Id;
        $scope.SelectedMaterial.Material = ob.UserName;
        $scope.SelectedMaterial.ArticleId = null;
        $scope.SelectedMaterial.Article = null;
        $scope.SelectedMaterial["HasAttribute"] = ob.HasAttribute;
        $scope.SelectedMaterial["WithSKU"] = ob.WithSKU;
        $scope.mmChangeFlag = true;
        if (ob.HasAttribute) {
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        UpdateGrid($scope.SelectedMaterial);

        $scope.closeMaterialMasterbyTypePopUp();
    };
    $scope.selectarticle = function (ob) {
        try {

            $scope.SelectedMaterial.MaterialMasterId = ob.MaterialMasterId;
            $scope.SelectedMaterial.Material = ob.MaterialMasterName;
            $scope.SelectedMaterial.ArticleId = ob.Id;
            $scope.SelectedMaterial.Article = ob.StandardName;


            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
            UpdateGrid($scope.SelectedMaterial);
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };
    $scope.getArticle = function (data) {
        $scope.SelectedMaterial = data;
        if (!baseService.isUndefinedOrNull($scope.SelectedMaterial.MaterialMasterId) && !$scope.SelectedMaterial.HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.DirectMaterialList[$scope.itemIndex].MaterialMasterId);
    };
    function UpdateGrid(data) {
        for (var i = 0; i < $scope.MaterialAttachmentList.length; i++) {
            if ($scope.MaterialAttachmentList[i].CostingItemId == data.CostingItemId) {
                $scope.MaterialAttachmentList[i].MaterialMasterId = $scope.SelectedMaterial.MaterialMasterId;
                $scope.MaterialAttachmentList[i].Material = $scope.SelectedMaterial.Material;
                $scope.MaterialAttachmentList[i].ArticleId = $scope.SelectedMaterial.ArticleId;
                $scope.MaterialAttachmentList[i].Article = $scope.SelectedMaterial.Article;
                $scope.MaterialAttachmentList[i].Vendor = $scope.SelectedMaterial.Vendor;
                break;
            }
        }

        for (var i = 0; i < $scope.MaterialQtyEditList.length; i++) {
            if ($scope.MaterialQtyEditList[i].CostingItemId == data.CostingItemId) {
                $scope.MaterialQtyEditList[i].Material = $scope.SelectedMaterial.Material;
                $scope.MaterialQtyEditList[i].Article = $scope.SelectedMaterial.Article;
                $scope.MaterialQtyEditList[i].Vendor = $scope.SelectedMaterial.Vendor;
                $scope.MaterialQtyEditList[i]["WithSKU"] = $scope.SelectedMaterial.WithSKU;
            }
        }


        var gridObj = $("#GridMaterialAttachment").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

        gridObj = $("#GridMaterialQuantity").data("ejGrid");
        gridObj.refreshContent(true);
        gridObj.refreshTemplate();

    }

   // $controller('partyBaseController', { $scope: $scope, $http: $http });
    $scope.partyType = 'Vendor';

    //#region Customer info
    $scope.partySearchByList = [
        {
            'name': $scope.partyType + ' Code',
            'value': 'Code'
        },
        {
            'name': $scope.partyType + ' Name',
            'value': 'PartyName'
        },
        {
            'name': 'Account Group',
            'value': 'PartyAccountGroupName'
        },
        {
            'name': 'Country',
            'value': 'CountryName'
        },
        {
            'name': 'State',
            'value': 'StateName'
        },
        {
            'name': 'Currency',
            'value': 'CurrencyCode'
        }
    ];
    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'PartyName, PartyAccountGroupName'
        , searchBy: 'PartyName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.partyList = [];
    //$scope.showPartyPopUp = function (data) {
    //    $scope.SelectedMaterial = data;
    //    $scope.partyList = [];
    //    $scope.getPartyList = function (pageno) {

    //        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataList/' + 'GetCompanyPartyDataList?companyId=' + $window.companyId + '&PlantId=' + $window.plantId + '&partyType=' + $scope.partyType;
    //        baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
    //            .then(function (result) {
    //                $scope.partyList = result.Rows;
    //                $scope.partyParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#partyPopUp')).modal('show');
    //    $scope.getPartyList();
    //};

    $scope.searchByPartyList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Party Group',
            'value': 'PartyGroup'
        }
    ];

    $scope.showPartyPopUpNew = function (data) {
       
        $scope.SelectedMaterial = data;
        $scope.partyList = [];
        if ($scope.partyType === 'Customer' || $scope.partyType === 'Vendor') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + $scope.partyType;
        }
        else if ($scope.partyType === 'Party') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Director') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        else if ($scope.partyType === 'Other') {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';
        }
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        //}
        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
            

            $scope.SelectedMaterial.Vendor = party.UserName;
            $scope.SelectedMaterial.VendorId = party.Id;
            angular.element(document.querySelector('#partyPopUp')).modal('hide');
            UpdateGrid($scope.SelectedMaterial);
        

    };



    $scope.Update = function () {
        $http({
            method: 'POST',
            url: $scope.path + "Save",
            data: { Id: $scope.Model.Id, MaterialAttachmentData: $scope.MaterialAttachmentList, QuantityData: $scope.MaterialQtyEditList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.Model = response.data.DATA;
                $scope.GetEditList();
                $scope.GetItemList();
            }
            // $scope.closeEntryDialog();
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    }


    //$scope.SelectedItemData = {};
    //$scope.UploadTableName = '';
    //$scope.uploadUrl = $scope.path + "UploadAttachment/";
    //$scope.ShowUploadBox = function (data/*, costingStage, DBTableName*/) {
    //    $scope.SelectedItemData = data;


    //    var _title = "Pre Costing";

    //    //$("#UploadBox").ejDialog("setTitle", "Upload File (" + _title + ")");
    //    var eDialog = $("#UploadBox").data("ejDialog");
    //    eDialog.open();
    //}


    //$scope.uploadPBUrl ="Costings/BOQ/UploadAttachment";

    //$scope.onBeginPBUpload = function (args) {
    //    try {
    //        if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
    //            throw 'Please select/save the production order first'

    //        args.data = $scope.bulletinTemplateNew.Id;
    //    } catch (e) {

    //        args.cancel = true;
    //        ShowResult(e, 'Error');
    //    }

    //}
    //$scope.errorPBPicUpload = function (e) {
    //    if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
    //        ShowResult('Please select/save the production order first', 'Error');
    //    else
    //        ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    //}


    //#region Production Bulletin Picture upload





    $scope.onBeginPBUpload = function (args) {
        try {
            args['data'] = args.model.Data;
            $scope.GetItemList();
        } catch (e) {
            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadPBUrl = "Costings/BOQ/UploadAttachment";
    $scope.fileselect = function (e) {

    }
    $scope.errorPBPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.bulletinTemplateNew.Id))
            ShowResult('Please select/save the production order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.closePartyPopUpNew = function () {
        angular.element(document.querySelector('#partyPopUp')).modal('hide');
    };

    $scope.Report = function (obj) {
        try {
            //$scope.Id = obj.data.Id;
            $scope.fileName = "CostingBOQReport.xls";
            $http({
                method: 'POST',
                url: $scope.path + "GetCostingBOQReport",
                data: { 'boqId': obj.data.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    // $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}

