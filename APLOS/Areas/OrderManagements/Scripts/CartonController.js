'use strict';
CartonController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function CartonController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Carton";
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'OrderManagements/ProductionOrder/';
    $scope.getListUrl = $scope.path + 'GetCartonMasterList';
    $scope.saveUrl = $scope.path + 'CreateCartonMaster';
    $scope.deleteUrl = $scope.path + 'DeleteCarton/';
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'UserName', name: "User Name" }, { value: 'Remarks', name: "Remarks" }];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };



    $scope.ModelList = [];
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.getListUrl,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();


    $scope.ModelTemp = {
        Id: null,
        UserName: null,
        EmployeeId: null,
        TargetClosingDays: 0,
        Remarks: null,
        StatusType: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        getSavedSalesOrderData($scope.ModelNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
        try {

            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        $scope.ModelNew.EmployeeId = ob.SystemId;
        $scope.ModelNew.EmployeeName = ob.EmployeeName;
        $scope.ModelNew.EmployeeCode = ob.EmployeeCode;
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
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
                    $scope.ModelNew.Id = response.data.Data;
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
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.recipeMaterialListSelected = [];
        $scope.packingCategoryList = [];
    }

    $scope.recipeMaterialFilterList = [
        { 'name': 'Master Order No', 'value': 'MasterOrderNo' },
        { 'name': 'Buyer Order#', 'value': 'BuyerOrderNo' },
        { 'name': 'Own Order#', 'value': 'OwnOrderNo' },
        { 'name': 'Buyer Item#', 'value': 'BuyerReferenceNo' },
        { 'name': 'Own Item#', 'value': 'OwnReferenceNo' },
        {
            'name': 'Material',
            'value': 'MaterialMasterName'
        },
        {
            'name': 'Product Name',
            'value': 'ProductName'
        },
        {
            'name': 'Buyer',
            'value': 'Buyer'
        },
        {
            'name': 'Article',
            'value': 'Article'
        },
        {
            'name': 'Customer',
            'value': 'Customer'
        },
        {
            'name': 'Commitment Date',
            'value': 'CommitmentDate'
        },
        {
            'name': 'Destination',
            'value': 'DestinationName'
        },
        {
            'name': 'Shipment Mode',
            'value': 'ShipmentModeName'
        },
        {
            'name': 'PO Number',
            'value': 'PONumber'
        }
    ];

    $scope.recipeMaterialParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'MaterialMasterName, ArticleName'
        , searchBy: 'MaterialMasterName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.recipeMaterialList = [];
    $scope.recipeMaterialParameters.searchBy = "MaterialMasterName";
    $scope.recipeMaterialParameters.search = "";
    $scope.recipeMaterialPopUp = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('show');
        $scope.serachSoMaterial();

    };

    $scope.summaryRows = [{
        title: "Total Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Qty", dataMember: "Qty", format: "{0:N0}" }],
        showCaptionSummary: true

    }];
    $scope.serachSoMaterial = function serachSoMaterial() {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSRSalesOrderListSearch?column=' + $scope.recipeMaterialParameters.searchBy + '&value=' + $scope.recipeMaterialParameters.search + "&CartonMasterId=" + $scope.ModelNew.Id
        }).then(function successCallback(response) {

            for (var i = 0; i < response.data.length; i++) {
                for (var J = 0; J < $scope.recipeMaterialListSelected.length; J++) {
                    if (response.data[i].SalesOrderId == $scope.recipeMaterialListSelected[J].SalesOrderId)
                        response.data[i].Checked = true;
                }
            }
            $scope.MaterialID = "";//important for changing color
            $scope.recipeMaterialList = response.data;

        });
    }

    $scope.CloseRecipeMaterialPopUp = function () {
        angular.element(document.querySelector('#recipeMaterialPopUp')).modal('hide');
    };

    $scope.recipeMaterialListSelected = [];
    $scope.addRecipeMaterial = function () {

        try {
            var id = "";
            var productid = "";
            var groupid = "";
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {

                    if (baseService.isUndefinedOrNull($scope.recipeMaterialList[i].ArticleId)
                        || $scope.recipeMaterialList[i].ArticleId == "") {
                        throw "Sales order items without product are not allowed";
                    }

                    if (id == "")
                        id = $scope.recipeMaterialList[i].ArticleId;

                    if (productid == "")
                        productid = $scope.recipeMaterialList[i].ProductID;

                    if (groupid == "")
                        groupid = $scope.recipeMaterialList[i].ProductionGrouping;

                    if (!baseService.isUndefinedOrNull($scope.recipeMaterialList[i].ProductionGrouping)) {
                        if ($scope.recipeMaterialList[i].ProductionGrouping != groupid) {
                            throw "Selecting different group materials are not allowed";
                        }
                        else {
                            if ($scope.recipeMaterialList[i].ArticleId != id) {
                                $scope.message_DiffArticleconfirmation = 'You are going to add different articles. Are you sure?';
                                angular.element(document.querySelector('#confirmDiffArticlePopUp')).modal('show');
                            }
                        }

                    } else {
                        if ($scope.recipeMaterialList[i].ArticleId != id)
                            throw "Selecting different articles are not allowed";

                    }
                }
            }

            $scope.recipeMaterialListSelected = [];
            for (var i = 0; i < $scope.recipeMaterialList.length; i++) {
                if ($scope.recipeMaterialList[i].Checked == true) {
                    $scope.recipeMaterialListSelected.push($scope.recipeMaterialList[i]);
                }
            }
            $scope.SaveSalesOrder();
            if (baseService.isUndefinedOrNull($scope.message_DiffArticleconfirmation)) {
                $scope.CloseRecipeMaterialPopUp();
            }
        } catch (e) {
            ShowResult(e, 'failure', 'recipeMaterialPopUp');
        }
    };

    $scope.message_DiffArticleconfirmation = null;
    $scope.message_DiffArticle1confirmation = null;

    $scope.ConDiffArticle = function () {
        $scope.message_DiffArticle1confirmation = 'You are going to add different articles. Are you sure?';
        angular.element(document.querySelector('#confirmDiffArticle1PopUp')).modal('show');
    }

    $scope.OverConDiffArticle = function () {
        $scope.CloseRecipeMaterialPopUp();
    }


    $scope.checkSameRecipe = function (data, index, event) {
        $rootScope.genericPushInTempList(data, event, $scope.recipeMaterialListSelected, 'SalesOrderId', 'SalesOrderId');
    };

    $scope.SaveSalesOrder = function () {
        try {
            if (baseService.arrayLength($scope.recipeMaterialListSelected) == 0) {
                throw "Select Sales Order List.";
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/SaveCartonDetail',
                data: { 'details': $scope.recipeMaterialListSelected, 'CartonMasterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getSavedSalesOrderData($scope.ModelNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    function getSavedSalesOrderData(masterId) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetCartonDetailList?masterId=' + masterId
        }).then(function successCallback(response) {
            $scope.recipeMaterialListSelected = response.data;
            $scope.GetCartonData();
        });
    }

    $scope.message_confirmation = null;
    $scope.removeSO = function (data) {
        $scope.SOobj = data.data;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.SOobj.SalesOrderId + ' ]';
        angular.element(document.querySelector('#confirmSODelPopUp')).modal('show');
    };
    $scope.DeleteSO= function () {
        if (!baseService.isUndefinedOrNull($scope.SOobj.Id)) {
            $http({
                method: 'POST',
                url: 'OrderManagements/ProductionOrder/DeleteCartonSO?id=' + $scope.SOobj.Id
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getSavedSalesOrderData($scope.ModelNew.Id);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }

    };

    $scope.packingTypeList = [];
    $http({
        method: 'GET',
        url: 'OrderManagements/PackingType/GetCbo'
    }).then(function successCallback(response) {
        $scope.packingTypeList = response.data;
    });

    $scope.packingCategoryList = [];
    $scope.GetCartonData = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetCartonCategoryTypeList?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.packingCategoryList = response.data;
        });
    }

    $scope.SaveCartonCategory = function () {
        try {
            if ($scope.packingCategoryList.length > 0) {
                var tempList = [];
                for (var i = 0; i < $scope.packingCategoryList.length; i++) {
                    if ($scope.packingCategoryList[i].Flag) {
                        tempList.push($scope.packingCategoryList[i]);
                    }
                }
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SaveCartonCategory",
                    data: { 'packCatlist': tempList, 'masterId': $scope.ModelNew.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetCartonData();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.sqlsoId = null;
    $scope.skuList = [];
    $scope.GetPackingSKUData = function (typemasterId) {
        $scope.idList = [];
        for (var di = 0; di < $scope.recipeMaterialListSelected.length; di++) {
            $scope.idList.push($scope.recipeMaterialListSelected[di]);
        }

        if ($scope.idList.length > 0) {
            var uniqueSalesOrderId= removeDuplicates($scope.idList, 'SalesOrderId');
            var wcsoId = "";
            if (uniqueSalesOrderId.length > 0) {
                wcsoId = "IN(";
                wcsoId += Array.prototype.map.call(uniqueSalesOrderId, function (item) { return "'" + item.SalesOrderId + "'"; }).join(",") + ")";
            }
            $scope.sqlsoId = wcsoId;
        }
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductionOrder/GetPackingSKUData?soId=' + $scope.sqlsoId + '&CartonTypeId=' + typemasterId
        }).then(function successCallback(response) {
            $scope.skuList = response.data;
        });
    }

    $scope.PRObj = {};
    $scope.GetCartonPOP = function (data) {
        $scope.PRObj = data.data;
        $scope.GetPackingSKUData($scope.PRObj.Id);
        angular.element(document.querySelector('#PRPopUp')).modal('show');
    }

    $scope.SavePR = function () {
        try {
            if ($scope.skuList.length > 0) {
                
                $http({
                    method: 'POST',
                    url: "OrderManagements/ProductionOrder/SaveCarton",
                    data: { 'packregilist': $scope.skuList, 'masterId': $scope.PRObj.Id },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPackingSKUData($scope.PRObj.Id);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ClosePR = function () {
        angular.element(document.querySelector('#PRPopUp')).modal('hide');
    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.barcode = "";

    $scope.barcodeKeyDown = function (event) {
        if (event.key === "Enter") {
            var barcode = $scope.barcode;
            if (barcode) {

                console.log("Barcode:", barcode);

                $scope.processBarcode(barcode);

                // Clear input
                $scope.barcode = "";
            }

            event.preventDefault();
        }
    };

    $scope.processBarcode = function (barcode) {

        alert("Barcode: " + barcode);

        // Call your API here
    };


}