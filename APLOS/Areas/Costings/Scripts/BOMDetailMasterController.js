'use strict';
BOMDetailMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function BOMDetailMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'BOMDetail';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Costings/BOMDetailMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "GroupName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'GroupName', name: "Group Name" }];
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null, GroupName: null, GroupInchargeId: null, DepartmentalHeadId: null, Remark: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetChild1Data();
        $scope.GetChild2Data();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpList = [];
    $scope.SelectedEmpList = [];

    $scope.popUpDataList = [];
    $scope.state = null;
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.ModelNew.ResponsiblePersonId = arg.data.SystemId;
        $scope.ModelNew.ResponsiblePerson = arg.data.EmployeeName;
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
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
                    $scope.ModelNew.Id = response.data.Data.Id;
                    //ClearFields();
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
        $scope.SelectedPlantList = [];
    }

    $scope.ModelC = { Id: null, BOMDetailMasterId: null, CustomerId: null, Code: null, ProductCode: null, ProductCodeId: null, OwnRefNo: null, Version: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }

    $scope.ProductLibraryList = [];
    $scope.GetProductLibraryList = function () {
        $http({
            method: 'GET',
            url: 'Costings/BOMDetailMaster/GetProductLibrary'
        }).then(function successCallback(response) {
            $scope.ProductLibraryList = response.data;
            angular.element(document.querySelector('#ProductLibraryPopUp')).modal('show');
        });
    };

    $scope.SetProductLibrary = function (obj) {
        $scope.ModelC.ProductCodeId = obj.data.Id;
        $scope.ModelC.Code = obj.data.Code;
        $scope.ModelC.ProductCode = obj.data.UserName;
        angular.element(document.querySelector('#ProductLibraryPopUp')).modal('hide');
    }

    $scope.clearProductLibrary = function () {
        $scope.ModelC.ProductCodeId = null;
        $scope.ModelC.ProductCode = null;
        $scope.ModelC.Code = null;
    }


    $scope.closeProductLibraryPopUp = function () {
        angular.element(document.querySelector('#ProductLibraryPopUp')).modal('hide');
    }

    $scope.CustomerList = [];
    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.ModelC.PartyCode = party.Code;
        $scope.ModelC.PartyName = party.UserName;
        $scope.ModelC.CustomerId = party.Id;

        $scope.hidePartyPopUp();
    };

    $scope.SaveChild1Data = function () {
        try {
            $scope.ModelC.BOMDetailMasterId = $scope.ModelNew.Id;
            if (baseService.isUndefinedOrNull($scope.ModelC.BOMDetailMasterId)) {
                throw "Select Master.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelC.CustomerId)) {
                throw "Select Customer";
            }
            //if (baseService.isUndefinedOrNull($scope.ModelC.ProductCodeId)) {
            //    throw "Select Product Code";
            //}

            $http({
                method: 'POST',
                url: 'Costings/BOMDetailMaster/CreateChild1Data',
                data: { 'data': $scope.ModelC },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearChild1();
                    $scope.GetChild1Data();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearChild1 = function () {
        $scope.ModelC = { Id: null, BOMDetailMasterId: null, CustomerId: null, Code: null, ProductCode: null, ProductCodeId: null, OwnRefNo: null, Version: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null }
    }

    $scope.Child1DataList = [];
    $scope.GetChild1Data = function () {
        $http.get('Costings/BOMDetailMaster/GetChild1Data?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.Child1DataList = response.data;
                }
            });

    }

    $scope.GetChild1 = function (args) {
        $scope.ModelC = Object.assign({}, args.data);
    };

    $scope.ModelSO = {
        Id: null,
        BOMDetailMasterId: null,
        CustomerId: null,
        ProductCodeId: null,
        OwnRefNo: null,
        Version: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };

    $scope.GetDetailChild = function (obj) {
        $scope.BOMDetailChild1 = obj.data;
        $scope.GetSavedSOData($scope.BOMDetailChild1.Id);
        angular.element(document.querySelector('#DetailChildPopUp')).modal('show');
    }

    $scope.SOItemList = [];
    $scope.GetSOPopUp = function () {
        $scope.SOItemList = [];
        $http.get('Costings/BOMDetailMaster/GetSOData')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SOItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#SOItemPopup')).modal('show');
    };

    $scope.selectSOItem = function ($event) {
        try {
            var soitem = $event.data;
            $scope.ModelSO.SOId = soitem.SOId;
            angular.element(document.querySelector('#SOItemPopup')).modal('hide');

        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.SaveSOData = function () {
        try {
            $scope.ModelSO.BOMDetailChild1Id = $scope.BOMDetailChild1.Id;
            if (baseService.isUndefinedOrNull($scope.ModelSO.BOMDetailChild1Id)) {
                throw "Select Master.";
            }
            if (baseService.isUndefinedOrNull($scope.ModelSO.SOId)) {
                throw "Select SO No.";
            }

            $http({
                method: 'POST',
                url: 'Costings/BOMDetailMaster/CreateSOData',
                data: { 'data': $scope.ModelSO },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearSO();
                    $scope.GetSavedSOData($scope.BOMDetailChild1.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearSO = function () {
        $scope.ModelSO = {
            Id: null,
            BOMDetailMasterId: null,
            CustomerId: null,
            ProductCodeId: null,
            OwnRefNo: null,
            Version: null,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null
        };
    }

    $scope.SODataList = [];
    $scope.GetSavedSOData = function (masterId) {
        $http.get('Costings/BOMDetailMaster/GetSavedSOData?masterId=' + masterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SODataList = response.data;
                }
            });
    }

    $scope.Child2 = {
        Id: null,
        BOMDetailMasterId: null,
        CostingItemId: null,
        FirstCharacteristicsValueId: null,
        SecondCharacteristicsValueId: null,
        MaterialMasterId: null,
        ArticleId: null,
        VendorId: null,
        Remarks: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    }
    $scope.Child2New = Object.assign({}, $scope.Child2);


    $scope.CostingItemList = [];
    $scope.GetCostingItemData = function () {
        $scope.CostingItemList = [];
        $http.get('Costings/BOMDetailMaster/GetCostingItemData')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.CostingItemList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#CostingItemPopup')).modal('show');
    };

    $scope.selectCostingItem = function ($event) {
        try {
            $scope.Child2New.CostingItemId = $event.data.CostingItemId;
            $scope.Child2New.CostingItem = $event.data.UserName;
            angular.element(document.querySelector('#CostingItemPopup')).modal('hide');

        } catch (ex) {
            ShowResult(ex, 'error');
        }
    }

    $scope.CloseCostingItemPopUp = function () {
        angular.element(document.querySelector('#CostingItemPopup')).modal('hide');
    }

    $scope.SKU1List = [];
    $scope.GetFirstSKUCbo = function () {
        $http.get('Costings/BOMDetailMaster/GetFirstSKUCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SKU1List = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetFirstSKUCbo();

    $scope.SKU2List = [];
    $scope.GetSecondSKUCbo = function () {
        $http.get('Costings/BOMDetailMaster/GetSecondSKUCbo')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.SKU2List = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetSecondSKUCbo();

    $scope.materialMasterbyTypeList = [];
        $scope.materialType = 'BOM';
    //$scope.getMaterial = function () {
    //    $scope.getMaterialMasterbyTypePopUp();
    //};

   
    $scope.selectMaterialByType = function (ob) {
        try {
            $scope.Child2New.MaterialMasterId = ob.Id;
            $scope.Child2New.MaterialMaster = ob.UserName;
            $scope.Child2New.MaterialCode = ob.Code;
            $scope.Child2New.ArticleId = null;
            $scope.Child2New.Article = null;
            $scope.Child2New.HasAttribute = ob.HasAttribute;
            $scope.Child2New.WithSKU = ob.WithSKU;
            if ($scope.Child2New.HasAttribute) {
                $scope.materialType = null;
                $scope.getArticleSearchList(ob.Id);
            } else {
                $scope.closeMaterialMasterbyTypePopUp();
                return ShowResult('This material has no attribute', 'failure');
            }
            
            $scope.closeMaterialMasterbyTypePopUp();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        $scope.getArticleSearchList($scope.bomNew.FGMaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.Child2New.MaterialMasterId = ob.MaterialMasterId;
            $scope.Child2New.MaterialMaster = ob.MaterialMasterName;
         //   $scope.Child2New.MaterialCode=
            $scope.Child2New.ArticleId = ob.Id;
            $scope.Child2New.Article = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function () {
        $scope.Child2New.ArticleId = null;
        $scope.Child2New.Article = null;
        $scope.Child2New.Article = null;
    };

    $scope.searchByVendor = "UserName"; $scope.searchVendor = "";
    $scope.searchByVendorList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: 'Vendor' }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

    $scope.vendorList = [];
    $scope.showVendorPopUpNew = function () {
        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew?partyType=' + 'Vendor';
        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByVendor, value: $scope.searchVendor },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.vendorList = response.data;
        });
        angular.element(document.querySelector('#VendorPopUp')).modal('show');
    };

    $scope.SelectVendor = function (x) {
        var party = x.data;
        $scope.Child2New.VendorCode = party.Code;
        $scope.Child2New.VendorName = party.UserName;
        $scope.Child2New.VendorId = party.Id;

        $scope.closeVendorPopUpNew();
    };

    $scope.closeVendorPopUpNew = function () {
        angular.element(document.querySelector('#VendorPopUp')).modal('hide');
    }

    $scope.SaveChild2Data = function () {
        try {
            $scope.Child2New.BOMDetailMasterId = $scope.ModelNew.Id;
            if (baseService.isUndefinedOrNull($scope.Child2New.CostingItemId)) {
                throw "Select CostingItem.";
            }
            //if (baseService.isUndefinedOrNull($scope.Child2New.FirstCharacteristicsValueId)) {
            //    throw "Select SKU1";
            //}
            //if (baseService.isUndefinedOrNull($scope.Child2New.SecondCharacteristicsValueId)) {
            //    throw "Select SKU2";
            //}
            //if (baseService.isUndefinedOrNull($scope.Child2New.MaterialMasterId)) {
            //    throw "Select Material";
            //}
            //if (baseService.isUndefinedOrNull($scope.Child2New.ArticleId)) {
            //    throw "Select Article";
            //}
            //if (baseService.isUndefinedOrNull($scope.Child2New.VendorId)) {
            //    throw "Select Vendor";
            //}

            $http({
                method: 'POST',
                url: 'Costings/BOMDetailMaster/CreateChild2Data',
                data: { 'data': $scope.Child2New },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearChild2();
                    $scope.GetChild2Data();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearChild2 = function () {
        $scope.Child2 = {
            Id: null,
            BOMDetailMasterId: null,
            CostingItemId: null,
            FirstCharacteristicsValueId: null,
            SecondCharacteristicsValueId: null,
            MaterialMasterId: null,
            ArticleId: null,
            VendorId: null,
            Remarks: null,
            AddedBy: null,
            AddedDate: null,
            AddedFromIP: null,
            UpdatedBy: null,
            UpdatedDate: null,
            UpdatedFromIP: null
        }
        $scope.Child2New = Object.assign({}, $scope.Child2);
    }

    $scope.Child2DataList = [];
    $scope.GetChild2Data = function () {
        $http.get('Costings/BOMDetailMaster/GetChild2Data?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.Child2DataList = response.data;
                }
            });

    }

    $scope.GetChild2 = function (args) {
        $scope.Child2New = Object.assign({}, args.data);
    };

    $scope.message_confirmation = null;
    $scope.removeChild1 = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.PartyName + ' ]';
        angular.element(document.querySelector('#confirmChild1PopUp')).modal('show');
    }

    $scope.DeleteChild1 = function () {
        $http({
            method: 'POST',
            url: 'Costings/BOMDetailMaster/DeleteChild1?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetChild1Data();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.removeChildSO = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.SOId + ' ]';
        angular.element(document.querySelector('#confirmChildSOPopUp')).modal('show');
    }

    $scope.DeleteChildSO = function () {
        $http({
            method: 'POST',
            url: 'Costings/BOMDetailMaster/DeleteChildSO?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedSOData($scope.BOMDetailChild1.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.removeChild2 = function (obj) {

        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.CostingItem + ' ]';
        angular.element(document.querySelector('#confirmChild2PopUp')).modal('show');
    }

    $scope.DeleteChild2 = function () {
        $http({
            method: 'POST',
            url: 'Costings/BOMDetailMaster/DeleteChild2?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetChild2Data();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

}