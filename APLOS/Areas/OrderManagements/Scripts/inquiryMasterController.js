'use strict';
inquiryMasterController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function inquiryMasterController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Inquiry Master";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.files = [];
    $scope.orderCategoryList = [];
    $scope.orderStatusList = [];
    $scope.itemList = [];
    $scope.personCboList = [];
    $scope.attributeList = [];
    $scope.personList = [];
    $scope.criticalList = [];
    $scope.inquiryProcessList = [];
    $scope.path = 'OrderManagements/InquiryMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateORUpdate';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListResponsible';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });


    cboService.getEnumCbo("InquiryMaster/GetInquiryProcessCbo", function (result) {
        $scope.inquiryProcessList = result;
    });
    $scope.ModelList = [];
    $scope.searchMasterFilterList = [{ Value: 'Id', Text: 'Id' }, { Value: 'CustomerName', Text: 'Customer' }, { Value: 'ResponsiblePersonName', Text: 'Responsible Person' },
        { Value: 'ProductMaster', Text: 'Product' }, { Value: 'OwnReferenceNo', Text: 'Own Reference' }, { Value: 'BuyerReferenceNo', Text: 'Buyer Reference' }];
    $scope.searchBy = 'Id'; $scope.search = '';
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search, companyId: $scope.ModelNew.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };
    ///$scope.getData();





    $scope.ModelTemp = {
        Id: null
        , CompanyId: null
        , CommitmentId: null
        , PlantId: null
        , EntityId: null
        , OrderType: null
        , PartyId: null
        , BuyerId: null
        , BuyerBrandId: null
        , BuyerDivisionId: null

        , InquiryTypeId: null
        , CriticalLevelId: null
        , SeasonId: null
        , OrderYear: null
        , InquiryClosingDate: null
        , InquiryDate: null
        , ProjectedQty: null
        , NoOfLineItem: null
        , ResponsiblePersonId: null
        , Remarks: null
        , AddedBy: null
        , AddedDate: null
        , AddedFromIP: null
        , UpdatedBy: null
        , UpdatedDate: null
        , UpdatedFromIP: null
        , InvoicingPartyPlantId: null
        , DeliveryPartyPlantId: null
        , InvoicingByAddress: null
        , DeliveryByAddress: null
        , TestingStandardId: null

        , ProjectQtyUOMId: null

        , Type: null
        , SpecialTaxId: null
        , BuyerDepartmentId: null
        , TaskTemplateMasterId: null
        , ContractId: null
        , BuyerReferenceNo: null
        , OwnReferenceNo: null
        , InquirySource: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    $scope.isBuyerApplicable = false;

    // #region Ddl
    $scope.typeList = [
        { Value: "Manufacture", Text: "Manufacture" },
        { Value: "Trading", Text: "Trading" },
        { Value: "JobWork", Text: "Job Work" }
    ];


    $scope.yearList = [];
    $scope.getYearOfHaving = function () {
        $scope.yearList = [];
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear());
        for (var i = ey; i <= 2099; i++) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.yearList.push(ob);
        }

        var d = new Date();
        var n = d.getFullYear();
        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === n) {
                $scope.ModelNew.OrderYear = $scope.yearList[i].Text;
            }
        }

    };
    $scope.getYearOfHaving();

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (response) {
        $scope.companyList = response;
    });

    $scope.plantList = [];
    $scope.getPlantCbo = function () {
        cboService.getCboPlantByCompany($scope.ModelNew.CompanyId, function (response) {
            $scope.plantList = response;
        });
    };

    $scope.specialTaxList = [];
    $scope.getSpecialTaxByPlantCbo = function () {
        cboService.getCboSpecialTaxByPlant($scope.ModelNew.PlantId, function (response) {
            $scope.specialTaxList = response;
        });
    };

    $scope.buyerList = [];
    cboService.getCboBuyer(function (data) {
        $scope.buyerList = data;
    });
    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.departmentList = [];
    $scope.buyerChange = function () {
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.ModelNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.ModelNew.BuyerId, function (result) {
            $scope.divisionList = result;
        });
        cboService.getBuyerDepartmentCboByBuyer($scope.ModelNew.BuyerId, function (result) {
            $scope.departmentList = result;
        });
    };

    cboService.getCboWithBuyer(null, function (result) {
        $scope.testingStandardList = result;
    });



    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            $scope.GetResponsiblePersonList();
        });
    };
    $scope.getAllEntities();


    $scope.getPlantConfigByPlant = function () {
        $scope.isBuyerApplicable = false;
        $scope.ModelNew.BuyerId = null;
        $scope.ModelNew.BuyerDivisionId = null;
        $scope.ModelNew.BuyerBrandId = null;
        $scope.ModelNew.TestingStandardId = null;
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $window.plantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0)
                $scope.isBuyerApplicable = response.data[0].BuyerApplicable;
        });
    };
    $scope.getPlantConfigByPlant();

    $scope.irregularList = [];
    $http.get("OrderManagements/MasterOrder/GetSpecialTaxList?plantId=" + $window.plantId)
        .then(function (response) {
            $scope.irregularList = response.data;
        });

    $scope.taskList = [];
    $scope.GEEMasterOrderId = '';
    $scope.GVMasterOrderId = '';
    $scope.tabTNA = 1;
    $scope.setTabTNA = function (newTab) {
        $scope.tabTNA = newTab;
    };
    $scope.isSetTNA = function (tabNum) {
        return $scope.tabTNA === tabNum;
    };
    $scope.onactivetab = function (args) {
        if (args.activeIndex == 0)
            $scope.GEEGetSelectedTasks($scope.ModelNew.Id);
        else
            $scope.GVGetSelectedTasks2($scope.ModelNew.Id);
    }
    $scope.getTaskList = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            if ($scope.ModelNew.Id != null) {
                $("#dialogViewTNADetail").data("ejDialog").open();
                $scope.GEEMasterOrderId = $scope.ModelNew.Id;
                $scope.GVMasterOrderId = $scope.ModelNew.Id;

                $scope.GEEGetSelectedTasks($scope.ModelNew.Id);
                $scope.GVGetSelectedTasks2($scope.ModelNew.Id);


            }
        }
    }

    $http.get("OrderManagements/ordercategory/getcbo/")
        .then(function (response) {
            $scope.orderCategoryList = response.data;
        });

    $http.get("OrderManagements/orderstatus/getcbo/")
        .then(function (response) {
            $scope.orderStatusList = response.data;
        });

    cboService.getCboSeasons(function (result) {
        $scope.seasonList = result;
    });
    $scope.CriticalLevelData = [];
    $http({
        method: 'GET',
        url: $scope.path + 'GetCriticalLevelData'
    }).then(function successCallback(response) {
        $scope.CriticalLevelData = response.data;
    });
    $scope.InquiryTypeData = [];
    $http({
        method: 'GET',
        url: $scope.path + 'GetInquiryTypeData'
    }).then(function successCallback(response) {
        $scope.InquiryTypeData = response.data;
    });


    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.ModelNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    // #endregion Ddl

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Get = function (index) {
        $scope.getPlantConfigByPlant();
        $scope.index = index;
        angular.copy($scope.ModelList[$scope.index], $scope.ModelTemp);
        angular.copy($scope.ModelTemp, $scope.ModelNew);
        $scope.ModelNew.OrderYear = parseInt($scope.ModelNew.OrderYear);
        $scope.Action = 'Update';
        getPartyPlantList();
        $scope.GetResponsiblePersonList();
       // GetDepartmentPersonCbo();
        getMasterItemList();
        $scope.getAllEntities();
        $scope.buyerChange();


        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.ModelNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.ModelNew.BuyerId, function (result) {
            $scope.divisionList = result;

            cboService.getBuyerDepartmentCboByBuyer($scope.ModelNew.BuyerId, function (result) {
                $scope.departmentList = result;
            });
        });
        if (!$rootScope.isCollapsed) $rootScope.toggle();


        cboService.getCboSpecialTaxByPlant($scope.ModelNew.PlantId, function (response) {
            $scope.specialTaxList = response;
        });

        if ($scope.ModelNew.IsExtraOrderPercentage === false) {
            $scope.ModelNew.ExtraOrderPercentage = 0;
        }

        if (!baseService.isUndefinedOrNull($scope.ModelNew.SpecialTaxId)) {
            $scope.ModelNew.SpecialTaxId = $scope.ModelNew.SpecialTaxId;
            $scope.SpecialTax = true;
        } else {
            $scope.SpecialTax = false;
        }
        $scope.mmChangeFlag = false;


    };

    $scope.Save = function () {
        //$scope.$broadcast('show-errors-check-validity');

        if ($scope.isBuyerApplicable) {
            if (baseService.isUndefinedOrNull($scope.ModelNew.BuyerId)) {
                return ShowResult('Buyer is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.BuyerDivisionId)) {
                return ShowResult('Division is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.BuyerDepartmentId)) {
                return ShowResult('Department is required.', 'failure');
            }
        }

        if (parseFloat(baseService.isUndefinedOrNull($scope.ModelNew.ProjectedQty) ? 0 : $scope.ModelNew.ProjectedQty) === 0) return ShowResult('Please insert projected qty.', 'failure');

        if (baseService.isUndefinedOrNull($scope.ModelNew.ProjectQtyUOMId)) {
            return ShowResult('Projected Quantity UoM is required.', 'failure');
        }


        $scope.ModelNewForm.$valid = true;
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, itemData: angular.toJson($scope.itemList) },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //ClearFields(response.data.Sequence);

                    $scope.ModelNew.Id = response.data.Id;
                    $scope.getData();
                    $scope.setTab(2);
                    getMasterItemList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        }
    };

    $scope.removeMaster = function () {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete";
            angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
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
                    $scope.files.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.deleteItem = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.path + 'deleteItem?id=' + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    getMasterItemList();
                    $scope.id = null;
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        $scope.personList = [];
        $scope.itemList = [];
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.isBuyerApplicable = false;
        $scope.ModelTemp = {};
        $scope.ModelNew = {
            PlantId: null
            , CompanyId: $scope.ModelNew.CompanyId
        };
        $scope.getPlantConfigByPlant();
        $scope.SpecialTax = false;
        $scope.mmChangeFlag = false;
        $scope.customerName = null;
    }

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
    $scope.showPartyPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.CompanyId)) {
            ShowResult('Select Company', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.ModelNew.PlantId)) {
            ShowResult('Select Plant', 'failure');
            return false;
        }
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = $scope.path + 'GetCompanyPartyDataList?companyId=' + $scope.ModelNew.CompanyId + '&plantId=' + $scope.ModelNew.PlantId + '&partyType=' + $scope.partyType;
            baseService.paginationBase($scope.partyUrl, pageno, $scope.partyParameters)
                .then(function (result) {
                    $scope.partyList = result.Rows;
                    $scope.partyParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#partyPopUp')).modal('show');
        $scope.getPartyList();
    };

    $scope.selectPartyPopUpRow = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedParty = id;
    };

    $scope.selectCustomerPopUp = function (index, id) {
        $scope.partyIndex = index;
        $scope.selectedCustomer = id;
    };

    $scope.closePartyPopUp = function () {
        if ($scope.partyIndex !== -1) {
            var party = $scope.partyList[$scope.partyIndex];
            $scope.ModelNew.PartyCode = party.Code;
            $scope.ModelNew.CustomerName = party.UserName;
            $scope.ModelNew.PartyId = party.Id;
            $scope.ModelNew.CurrencyId = party.CurrencyId;
            $scope.ModelNew.PartyAccountGroupId = party.PartyAccountGroupId;
        }
        $scope.personList = [];
        getPartyPlantList();
        //GetDepartmentPersonCbo();
        $scope.hidePartyPopUp();
    };

    $scope.GetResponsiblePersonList = function () {
        $scope.personList = [];
        $http.get($scope.path + "GetResponsiblePersonList?masterId=" + $scope.ModelNew.Id)
            .then(function (response) {
                $scope.personList = response.data;
                if ($scope.ModelNew.PlantId !== null && ($scope.personList === null || $scope.personList.length <= 0)) {
                    $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.ModelNew.PlantId + '&partyAccountGroupId=' + $scope.ModelNew.PartyAccountGroupId + '&partyId=' + $scope.ModelNew.PartyId + '&flag=' + false;
                    $scope.getPopUpData = function (pageno) {
                        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                            .then(function (result) {
                                if (baseService.arrayLength(result) !== 0) {
                                    for (var i = 0; i < result.length; i++) {
                                        var obj = result[i];
                                        $scope.personList.push({
                                            Id: obj.Id
                                            , MasterOrderId: $scope.ModelNew
                                            , CustomerDivisionId: obj.CustomerDivisionId
                                            , OrderResponsibleDepartmentId: obj.OrderResponsibleDepartmentId
                                            , Department: obj.Department
                                            , OurRespnsiblePersonId: obj.OurRespnsiblePersonId
                                            , EmployeeCode: obj.EmployeeCode
                                            , EmployeeName: obj.EmployeeName
                                            , PartyRespnsiblePersonId: obj.PartyRespnsiblePersonId
                                            , PartyRespnsiblePerson: obj.PartyRespnsiblePerson
                                        });
                                    }
                                    //GetDepartmentPersonCbo();
                                }
                            }, function () {
                                ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                            }).finally(function () {
                            });
                    };
                    $scope.getPopUpData();
                }
            });
    };

    $scope.commitmentList = [];

    $scope.showCommitmentPopUp = function () {
        $http.get('OrderManagements/Commitment/GetCommitmentData')
            .then(function (response) {
                $scope.commitmentList = response.data;
            });
        angular.element(document.querySelector('#commitmentPop')).modal('show');
    }

    $scope.SetCommitment = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.ModelNew.CommitmentId = $scope.data.Id;
        angular.element(document.querySelector('#commitmentPop')).modal('hide');
    }

    $scope.CloseCommitment = function () {
        angular.element(document.querySelector('#commitmentPop')).modal('hide');
    }

    //function GetDepartmentPersonCbo() {
    //    $scope.personCboList = [];
    //    $http.get($scope.path + "GetDepartmentPersonCbo?plantId=" + $scope.ModelNew.PlantId + '&partyAccountGroupId=' + $scope.ModelNew.PartyAccountGroupId + '&partyId=' + $scope.ModelNew.PartyId)
    //        .then(function (response) {
    //            $scope.personCboList = response.data;
    //        });
    //}

    $scope.itemIndex = -1;

    $scope.mmChangeFlag = false;

    $scope.materialType = ['FinishedGoods'];

    $scope.getMaterial = function (index) {
        $scope.itemIndex = index;
        $scope.getMaterialMasterbyTypePopUp();
    };

    $scope.selectMaterialByType = function (ob) {
        $scope.itemList[$scope.itemIndex].MaterialMasterId = ob.Id;
        $scope.itemList[$scope.itemIndex].MaterialMasterName = ob.UserName;
        $scope.itemList[$scope.itemIndex].ArticleId = null;
        $scope.itemList[$scope.itemIndex].ArticleName = null;
        $scope.itemList[$scope.itemIndex].InquiryItemId = null;
        $scope.itemList[$scope.itemIndex].SampleItemId = null;
        $scope.itemList[$scope.itemIndex].HasAttribute = ob.HasAttribute;
        $scope.mmChangeFlag = true;
        if ($scope.itemList[$scope.itemIndex].HasAttribute) {
            $scope.getArticleSearchList(ob.Id);
        } else {
            $scope.closeMaterialMasterbyTypePopUp();
            return ShowResult('This material has no attribute', 'failure');
        }
        // getTaxCategoryList(ob.HSNCodeId);
        $scope.HSNCodeId = ob.HSNCodeId;
        $scope.closeMaterialMasterbyTypePopUp();
    };

    $scope.getArticle = function (index) {
        $scope.itemIndex = index;
        if (!baseService.isUndefinedOrNull($scope.itemList[$scope.itemIndex].MaterialMasterId) && !$scope.itemList[$scope.itemIndex].HasAttribute)
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.itemList[$scope.itemIndex].MaterialMasterId);
    };

    $scope.selectarticle = function (ob) {
        try {
            $scope.itemList[$scope.itemIndex].MaterialMasterId = ob.MaterialMasterId;
            $scope.itemList[$scope.itemIndex].MaterialMasterName = ob.MaterialMasterName;
            $scope.itemList[$scope.itemIndex].ArticleId = ob.Id;
            $scope.itemList[$scope.itemIndex].ArticleName = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.clearArticle = function (index) {
        $scope.itemList[index].ArticleId = null;
        $scope.itemList[index].ArticleName = null;
    };

    $scope.getArticleValue = function (articleId, mName, aName) {
        $scope.articleValueList = [];
        $scope.mName = mName;
        $scope.aName = aName;
        $http({
            method: 'GET',
            url: 'Materials/MaterialMasterArticle/GetMaterialArticleValue?articleId=' + articleId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no article value', 'failure');
            $scope.articleValueList = response.data;
            angular.element(document.querySelector('#articleValuePoUp')).modal('show');
        });
    };

    $scope.closeArticleValuePopUp = function () {
        angular.element(document.querySelector('#articleValuePoUp')).modal('hide');
    };

    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Department',
        searchBy: "Department",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUpList = [];
    $scope.popUp = function (flag) {
        if (baseService.isUndefinedOrNull($scope.ModelNew.PlantId)) return ShowResult('Select plant', 'failure');
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.ModelNew.PlantId + '&partyAccountGroupId=' + $scope.ModelNew.PartyAccountGroupId + '&partyId=' + $scope.ModelNew.PartyId + '&flag=' + flag;
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    if (baseService.arrayLength(result) > 0) {
                        for (var i = 0; i < result.length; i++) {
                            if (!baseService.valueCheckInList($scope.personList, 'OrderResponsibleDepartmentId', result[i].OrderResponsibleDepartmentId)) {
                                $scope.popUpDataList.push(result[i]);
                            }
                        }
                    }
                    //$scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (obj) {
        if (baseService.valueCheckInList($scope.personList, 'OrderResponsibleDepartmentId', obj.OrderResponsibleDepartmentId))
            return ShowResult(obj.Department + ' already taken.', '', 'popUpId');
        $scope.personList.push({
            Id: obj.Id
            , MasterOrderId: $scope.ModelNew
            , CustomerDivisionId: obj.CustomerDivisionId
            , OrderResponsibleDepartmentId: obj.OrderResponsibleDepartmentId
            , Department: obj.Department
            , OurRespnsiblePersonId: obj.OurRespnsiblePersonId
            , EmployeeCode: obj.EmployeeCode
            , EmployeeName: obj.EmployeeName
            , PartyRespnsiblePersonId: obj.PartyRespnsiblePersonId
            , PartyRespnsiblePerson: obj.PartyRespnsiblePerson
        });
        //GetDepartmentPersonCbo();
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.Submaterial + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        $scope.personList.splice($scope.popUpIndex, 1);
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeListPopUp = function (name) {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.CompanyId)) {
                throw 'Select Company';
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.PlantId)) {
                throw 'Select Plant';
            }

            $scope.Name = name;
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                $scope.employeeParameters.plantId = $scope.ModelNew.PlantId;
                $scope.employeeParameters.partyAccountGroupId = $scope.ModelNew.PartyAccountGroupId;
                $scope.employeeParameters.partyId = $scope.ModelNew.PartyId;
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.Name === 'mo') {
                $scope.ModelNew.ResponsiblePersonId = employee.SystemId;
                $scope.ModelNew.ResponsiblePersonName = employee.EmployeeName;
            } else {
                $scope.soModel.ResponsiblePersonId = employee.SystemId;
                $scope.soModel.ResponsiblePersonName = employee.EmployeeName;
            }
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.getLineItemType = function () {
        if (baseService.isUndefinedOrNull($scope.ModelNew.Type)) {
            $scope.linetypeList = [
                { Value: "Manufacture", Text: "Manufacture" },
                { Value: "Trading", Text: "Trading" },
                { Value: "JobWork", Text: "Job Work" }
            ];
        }
        else if ($scope.ModelNew.Type === "JobWork") {
            $scope.linetypeList = [
                { Value: "JobWork", Text: "Job Work" }
            ];
        } else {
            $scope.linetypeList = [
                { Value: "Manufacture", Text: "Manufacture" },
                { Value: "Trading", Text: "Trading" }
            ];
        }
    };

    function getMasterItemList() {
        $scope.itemList = [];
        $scope.itemTestingStandardList = [];

        $http.get($scope.path + "GeInquiryItemList?InquiryMasterId=" + $scope.ModelNew.Id)
            .then(function (response) {
                $scope.itemList = response.data;
                if (baseService.arrayLength($scope.itemList) === 0) {
                    for (var i = 0; i < parseInt($scope.ModelNew.NoOfLineItem); i++) {
                        $scope.itemList.push({
                            Id: null
                            , MaterialMasterId: null
                            , MaterialMasterName: null
                            , ArticleId: null
                            , ArticleName: null
                            , InquiryItemId: null
                            , SampleItemId: null
                            , Code: null
                            , BuyerReferenceNo: null
                            , OwnReferenceNo: null
                            , ProjectedQty: null
                            , NoOfSample: null
                            , Type: $scope.ModelNew.Type
                            , Remarks: null
                            , IsRepeat: false
                            , InquiryProcess: null
                            , InquiryProcessList: []
                            , Particulars: null
                            , CostingRequired: false

                        });
                    }
                }
                else {
                    $scope.mmChangeFlag = false;
                }
            });
        $scope.getLineItemType();
    }

    $scope.addNewItem = function () {
        $scope.getLineItemType();
        $scope.itemList.push({
            Id: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , InquiryItemId: null
            , SampleItemId: null
            , Code: null
            , BuyerReferenceNo: null
            , OwnReferenceNo: null
            , ProjectedQty: null
            , NoOfSample: null
            , Type: $scope.ModelNew.Type
            , Remarks: null
            , IsRepeat: false
            , InquiryProcess: null
            , InquiryProcessList: []
            , Particulars: null
            , CostingRequired: false
        });
    };

    $scope.removeLineItem = function (index) {
        $scope.itemList.splice(index, 1);
    };
    //#region Party plant 

    $scope.invoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('show');
    };
    $scope.closeInvoicingPartyPopUp = function () {
        angular.element(document.querySelector('#invoicingPartyPopUp')).modal('hide');
    };
    $scope.billShippAddress = function (id, flag) {
        if (!baseService.isUndefinedOrNull(id)) {
            var address = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].Address1;
            var state = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].StateName;
            if (flag === 'billTo') {
                $scope.ModelNew.InvoicingState = state;
                $scope.ModelNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.ModelNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.ModelNew.DeliveryState = state;
                $scope.ModelNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.ModelNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.ModelNew.InvoicingState = null;
                $scope.ModelNew.InvoicingGSTIN = null;
                return $scope.ModelNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.ModelNew.DeliveryState = null;
                $scope.ModelNew.DeliveryGSTIN = null;
                return $scope.ModelNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.ModelNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.ModelNew.InvoicingPartyPlantId = item.Value;
                    $scope.ModelNew.DeliveryPartyPlantId = item.Value;
                    $scope.ModelNew.InvoicingByAddress = item.Address1;
                    $scope.ModelNew.DeliveryByAddress = item.Address1;
                    $scope.ModelNew.InvoicingState = item.StateName;
                    $scope.ModelNew.InvoicingGSTIN = item.GSTIN;
                    $scope.ModelNew.DeliveryState = item.StateName;
                    $scope.ModelNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    //#endregion Party plant 

    // #region Attribute

    $scope.searchFreeField = false;

    $scope.getAttribute = function (id, materialMasterId, mName) {
        if ($scope.mmChangeFlag) return ShowResult('Please update changes data', 'failure');
        $scope.mName = mName;
        $scope.masterItemId = id;
        var url = '';
        if (baseService.isUndefinedOrNull($scope.masterItemId))
            url = $scope.path + 'GetAttributeListByMaterialMasterId?materialMasterId=' + materialMasterId;
        else
            url = $scope.path + 'GetOrderAttributeListByMasterId?masterItemId=' + $scope.masterItemId + '&materialMasterId=' + materialMasterId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            if (baseService.arrayLength(response.data) === 0)
                return ShowResult('This material has no attribute', 'failure');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].ValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
            angular.element(document.querySelector('#attributePoUp')).modal('show');
        });
    };

    $scope.idNullByValueFreeText = function (id, index) {
        if ($scope.attributeList[index].AttributeId === id) {
            $scope.attributeList[index].MaterialAttributeValueId = null;
            $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        }
    };
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;//disabled true
            else
                return false;//disabled false
        }
        else
            return true;//disabled true
    };
    $scope.IsMandatoryButNull = function (isMandatory, ValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(ValueFreeText)) return true;
            else return false;
        }
        else return false;
    };

    $scope.saveAttribute = function () {
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateAttributeValue'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'attributeValueList': $scope.attributeList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.attributeList = [];
                $scope.masterItemId = null;
                angular.element(document.querySelector('#attributePoUp')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.closeAttributePopUp = function () {
        angular.element(document.querySelector('#attributePoUp')).modal('hide');
    };

    // #endregion Attribute

    // #region value

    $scope.valueindex = -1;
    $scope.searchvalueList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'ShortName',
            'value': 'ShortName'
        },
        {
            'name': 'StanderName',
            'value': 'StanderName'
        },
        {
            'name': 'UserName',
            'value': 'UserName'
        }
    ];
    $scope.valueParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'Code'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.valuePoUp = function (data, index) {
        $scope.materialAttributeValueUrl = 'Materials/MaterialMasterArticle/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.materialMasterId = data.MaterialMasterId;
            $scope.valueParameters.attributeId = data.AttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    };
    $scope.getAttrValue = function (data) {
        $scope.attributeList[$scope.valueindex].AttributeValueId = data.MaterialAttributeValueId;
        $scope.attributeList[$scope.valueindex].ValueFreeText = data.UserName;
        $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
        $scope.valueindex = -1;
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
    };
    $scope.materialAttributeValueClear = function (index) {
        $scope.attributeList[index].MaterialAttributeValueId = null;
        $scope.attributeList[index].MaterialMasterAttributeValueId = null;
        $scope.attributeList[index].ValueFreeText = null;
        $scope.searchFreeField = false;
        var isFree = $scope.attributeList[index].IsFreeField;
        $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    };
    $scope.closeValuePopUp = function () {
        angular.element(document.querySelector('#attributeValuePopUp')).modal('hide');
        CloseModalShowResult('attributeValuePopUp');
    };

    // #endregion value

    // #region Sales Order

    $scope.getSalesOrder = function (id, materialMasterId, mName, aName, hsnCodeId) {
        if ($scope.mmChangeFlag) return ShowResult('Please update changes data', 'failure');
        $scope.mName = baseService.isUndefinedOrNull(aName) ? mName : mName + '   >>>   ' + aName;
        $scope.masterItemId = id;
        $scope.materialMasterId = materialMasterId;
        $scope.currency = $("#Currency option:selected").text();
        $scope.soModel = {
            Id: null
            , MasterOrderItemId: $scope.masterItemId
            , DeliveryDate: null
            , CommitmentDate: null
            , DestinationId: null
            , ShipmentModeId: null
            , CustomerPOId: null
            , PONumber: null
            , UpCharge: null
            , OrderStatusId: $scope.ModelNew.OrderStatusId
            , OrderCategoryId: $scope.ModelNew.OrderCategoryId
            , SOType: null
            , ResponsiblePersonId: $scope.ModelNew.ResponsiblePersonId
            , ResponsiblePersonName: $scope.ModelNew.ResponsiblePersonName
            , Qty: null
            , Rate: null
            , HSNCodeId: hsnCodeId
            , TotalTaxAmount: 0
            , MainRawMaterialInhouseDate: null
            , OtherRawMaterialInhouseDate: null
        };
        getSalesOrderList();
        $scope.getDestination();
        angular.element(document.querySelector('#soPoUp')).modal('show');
    };

    function getSalesOrderList() {
        $scope.salesOrderList = [];
        $http.get('OrderManagements/MasterOrder/GetSOandItemList?masterItemId=' + $scope.masterItemId)
            .then(function (response) {
                $scope.salesOrderList = response.data;
            });
    }

    $scope.getDestination = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/destination/GetCbo/'
        }).then(function successCallback(response) {
            $scope.destinationList = response.data;
        });
        $http({
            method: 'GET',
            url: 'OrderManagements/shipmode/GetCbo/'
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.shipmentModeList = response.data;
            }
        });
    };

    $scope.saveSalesOrder = function () {

        //if ($scope.soModel.PONumber === null || $scope.soModel.OrderStatusId === null || $scope.soModel.OrderCategoryId === null || $scope.soModel.DestinationId === null || $scope.soModel.ShipmentModeId === null || $scope.soModel.Qty === null) {
        //    ShowResult("Please enter mandatory fields", 'failure', 'soPoUp');
        //    return false;
        //}
        if ($scope.soModel.Qty <= 0) {
            ShowResult("Sales order quantity can't be zero", 'failure', 'soPoUp');
            return false;
        }
        if ($scope.soModel.Rate < $scope.soModel.Discount) {
            ShowResult("Sales order discount can't greater than Rate", 'failure', 'soPoUp');
            return false;
        }

        if (!baseService.isUndefinedOrNull($scope.soModel.Id)) {
            if ($scope.delivaryDate !== $scope.soModel.DeliveryDate) {
                if (baseService.isUndefinedOrNull($scope.soModel.Reason)) {
                    ShowResult("Reason is required on Delivery Date change.", 'failure', 'soPoUp');
                    return false;
                }
            }
        }

        $scope.$broadcast('show-errors-check-validity');

        if ($scope.soForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreateSalesOrder'
                    , data: {
                        'masterItemId': $scope.masterItemId
                        , 'salesOrderMaster': $scope.soModel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'soPoUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'soPoUp');
                        getSalesOrderList();
                        clearSO();
                        getMasterItemList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'soPoUp');
                };
            } else {
                getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            }
        }
    };

    function getSalesOrderTaxCategoryUpdateList(salesOrderId) {
        $scope.SoTotalAmount = ((parseFloat($scope.soModel.Qty) * parseFloat($scope.soModel.Rate)) - parseFloat($scope.soModel.Discount)).toFixed(2);
        $http({
            method: 'GET'
            , url: $scope.path + 'getSalesOrderTaxCategoryList?salesOrderId=' + salesOrderId
        }).then(function (response) {
            $scope.taxList = response.data;
            for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
                $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * (parseFloat($scope.taxList[i].Percentage) / 100);
            }
            UpdateSOWithTax();
        });

    }

    function UpdateSOWithTax() {
        $http({
            method: 'POST'
            , url: $scope.path + 'UpdateSalesOrder'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'salesOrderMaster': $scope.soModel
                , 'taxCategoryList': $scope.taxList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'soPoUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'soPoUp');
                getSalesOrderTaxCategoryList(response.data.Id);
                getSalesOrderList();
                clearSO();
                getMasterItemList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'soPoUp');
        };
    }

    $scope.delivaryDate = null;
    $scope.soEdit = function (data) {
        angular.copy(data, $scope.soModel);
        $scope.delivaryDate = null;
        $scope.delivaryDate = $scope.soModel.DeliveryDate;
    };

    $scope.removeSOItemList = function (index, data) {
        $scope.tempEmpOb = data;
        $scope.empIndex = index;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message = 'Are you sure want to parmenently delete?';
        else
            $scope.message = 'Are you sure want to parmenently delete?';
        angular.element(document.querySelector('#removePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.tempEmpOb.Id)) {

            $scope.soDelete($scope.empIndex, $scope.tempEmpOb);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };

    $scope.soDelete = function (index, soModel) {
        //if (confirm("Are you sure to delete")) {
        $http({
            method: 'POST'
            , url: $scope.path + 'DeleteSalesOrder'
            , data: {
                'masterItemId': $scope.masterItemId
                , 'salesOrderMaster': soModel
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'soPoUp');
            }
            else {
                ShowResult(response.data.Message, 'success', 'soPoUp');
                getSalesOrderList();
                clearSO();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'soPoUp');
        };
        $scope.salesOrderList.splice(index, 1);
        //}

    };

    $scope.closeSOPopUp = function () {
        angular.element(document.querySelector('#soPoUp')).modal('hide');
    };

    function clearSO() {
        $scope.soModel = {
            Id: null
            , MasterOrderItemId: $scope.masterItemId
            , DeliveryDate: null
            , CommitmentDate: null
            , DestinationId: null
            , ShipmentModeId: null
            , CustomerPOId: null
            , PONumber: null
            , UpCharge: null
            , OrderStatusId: $scope.ModelNew.OrderStatusId
            , OrderCategoryId: $scope.ModelNew.OrderCategoryId
            , SOType: null
            , ResponsiblePersonId: null
            , Qty: null
            , Rate: null
            , HSNCodeId: $scope.HSNCodeId
            , TotalTaxAmount: 0
            , MainRawMaterialInhouseDate: null
            , OtherRawMaterialInhouseDate: null
        };
    }

    // #region Split Sales Order

    $scope.soSplitModel = {
        Id: null
        , MasterOrderItemId: $scope.masterItemId
        , DeliveryDate: null
        , CommitmentDate: null
        , DestinationId: null
        , ShipmentModeId: null
        , CustomerPOId: null
        , PONumber: null
        , UpCharge: null
        , OrderStatusId: $scope.ModelNew.OrderStatusId
        , OrderCategoryId: $scope.ModelNew.OrderCategoryId
        , SOType: null
        , ResponsiblePersonId: null
        , Qty: null
        , Rate: null
        , HSNCodeId: $scope.HSNCodeId
        , TotalTaxAmount: 0
        , ParentId: null
        , Discount: null
        , LSD: null
        , MainRawMaterialInhouseDate: null
        , OtherRawMaterialInhouseDate: null
    };

    $scope.SplitSO = function (data) {
        $scope.soSplitModel.Id = null
        $scope.soSplitModel.MasterOrderItemId = $scope.masterItemId
        $scope.soSplitModel.DeliveryDate = data.DeliveryDate;
        $scope.soSplitModel.CommitmentDate = data.CommitmentDate;
        $scope.soSplitModel.DestinationId = data.DestinationId;
        $scope.soSplitModel.ShipmentModeId = data.ShipmentModeId;
        $scope.soSplitModel.CustomerPOId = data.CustomerPOId;
        $scope.soSplitModel.PONumber = data.PONumber;
        $scope.soSplitModel.UpCharge = data.UpCharge;
        $scope.soSplitModel.OrderStatusId = $scope.ModelNew.OrderStatusId
        $scope.soSplitModel.OrderCategoryId = $scope.ModelNew.OrderCategoryId
        $scope.soSplitModel.SOType = data.SOType;
        $scope.soSplitModel.ResponsiblePersonId = data.ResponsiblePersonId;
        $scope.soSplitModel.ResponsiblePersonName = data.ResponsiblePersonName;
        $scope.soSplitModel.Rate = data.Rate;
        $scope.soSplitModel.HSNCodeId = $scope.HSNCodeId
        $scope.soSplitModel.TotalTaxAmount = data.TotalTaxAmount;
        $scope.soSplitModel.ParentId = data.Id;
        $scope.soSplitModel.ParentQty = data.Qty;
        $scope.soSplitModel.Qty = 0;
        $scope.soSplitModel.Discount = data.Discount;
        $scope.soSplitModel.LSD = data.LSD;
        $scope.soSplitModel.MainRawMaterialInhouseDate = data.MainRawMaterialInhouseDate;
        $scope.soSplitModel.OtherRawMaterialInhouseDate = data.OtherRawMaterialInhouseDate;
        angular.element(document.querySelector('#soSplitPoUp')).modal('show');
    }

    $scope.closeSplitSOPopUp = function () {
        angular.element(document.querySelector('#soSplitPoUp')).modal('hide');
    };

    $scope.saveSplitSalesOrder = function () {

        if ($scope.soSplitModel.Qty <= 0) {
            ShowResult("Sales order split quantity can't be zero", 'failure', 'soSplitPoUp');
            return false;
        }

        if ($scope.soSplitModel.ParentQty <= $scope.soSplitModel.Qty) {
            ShowResult("Sales order split quantity '" + $scope.soSplitModel.Qty + "' can't greater than or equal Parent quantity '" + $scope.soSplitModel.ParentQty + "'", 'failure', 'soSplitPoUp');
            return false;
        }

        if ($scope.soSplitModel.Rate < $scope.soSplitModel.Discount) {
            ShowResult("Sales order split discount can't greater than Rate", 'failure', 'soSplitPoUp');
            return false;
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.soSplitForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.soSplitModel.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.path + 'CreateSplitSalesOrder'
                    , data: {
                        'masterItemId': $scope.masterItemId
                        , 'salesOrderMaster': $scope.soSplitModel
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'soSplitPoUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success', 'soSplitPoUp');
                        getSalesOrderList();
                        getMasterItemList();
                        angular.element(document.querySelector('#soSplitPoUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'soSplitPoUp');
                };
            }
            //else {
            //    getSalesOrderTaxCategoryUpdateList($scope.soModel.Id);
            //}
        }
    };

    // #endregion Split Sales Order

    // #endregion Sales Order

    // #region Sales Order Tax

    $scope.TaxAction = 'Save';

    accountService.getTaxCategoryMaterialLevelCbo(" ", function (result) {
        $scope.taxCategoryList = result;
    });

    $scope.addTax = function () {
        var data = {
            TotalAmount: 0,
            Id: null,
            HSNCode: $scope.HSNCode,
            HSNCodeId: null,
            UserName: null,
            TaxCategoryId: null,
            SpecialTaxId: null
        };
        $scope.taxList.push(data);

    };

    $scope.getTaxCategoryList = function (data, index) {
        $scope.total = 0;
        $scope.SoTotalAmount = 0;
        if (baseService.isUndefinedOrNull($scope.HSNCodeId)) {
            $scope.HSNCodeId = $scope.soModel.HSNCodeId;
        }
        $scope.salesOrderId = data.Id;
        $scope.taxList = [];
        $scope.soIndex = index;
        $scope.STA = (parseFloat(data.Qty) * parseFloat(data.Rate)) - parseFloat(data.Discount);
        $scope.SoTotalAmount = ($scope.STA).toFixed(2);
        if (data.isTax === 0) {
            $http({
                method: 'GET'
                , url: $scope.path + 'GetTaxCategoryList?masterOrderId=' + $scope.ModelNew.Id + '&plantId=' + $scope.ModelNew.PlantId + '&hsnCodeId=' + $scope.HSNCodeId + '&specialTaxId=' + $scope.ModelNew.SpecialTaxId
            }).then(function (response) {
                $scope.taxList = response.data;
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.HSNCode = response.data[0]['HSNCode'];
                }
                for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
                    $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * parseFloat($scope.taxList[i].Percentage) / 100;
                }
                $scope.TaxAction = 'Save';
            });
        }
        else {
            getSalesOrderTaxCategoryList($scope.salesOrderId);
            $scope.TaxAction = 'Update';
        }
        angular.element(document.querySelector('#taxPopup')).modal('show');
    };

    $scope.onchangeFunction = function (id) {
        $scope.TaxCategoryId = id;
        var getRow = $filter("filter")($scope.taxList, { "TaxCategoryId": id });
        if (getRow.length == 2) {
            ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
        }
    };


    $scope.taxSave = function () {
        if (!baseService.isUndefinedOrNull($scope.TaxCategoryId)) {
            var getRow = $filter("filter")($scope.taxList, { "TaxCategoryId": $scope.TaxCategoryId });
            if (getRow.length == 2) {
                ShowResult("You can't add Same Tax two times", 'failure', 'taxPopup');
                return false;
            }

        }
        for (var i = 0; i < $scope.taxList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.taxList[i].TaxCategoryId)) {
                ShowResult("Select Tax Category.", 'failure', 'taxPopup');
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.taxList[i].Percentage)) {
                ShowResult("Input Percentage.", 'failure', 'taxPopup');
                return false;
            }
            if ($scope.taxList[i].Percentage === 0) {
                ShowResult("Percentage must be greater than 0.", 'failure', 'taxPopup');
                return false;
            }
        }
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateSalesOrderTax'
            , data: {
                'salesOrderId': $scope.salesOrderId
                , 'taxCategoryList': $scope.taxList
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'taxPopup');
            }
            else {
                $scope.salesOrderList[$scope.soIndex].isTax = 1;
                $scope.closeTaxPopUp();
                ShowResult(response.data.Message, 'success', 'soPoUp');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'taxPopup');
        };
    };

    $scope.closeTaxPopUp = function () {
        $scope.soIndex = -1;
        $scope.SoTotalAmount = 0;
        angular.element(document.querySelector('#taxPopup')).modal('hide');
    };

    function getSalesOrderTaxCategoryList(salesOrderId) {
        $http({
            method: 'GET'
            , url: $scope.path + 'getSalesOrderTaxCategoryList?salesOrderId=' + salesOrderId
        }).then(function (response) {
            $scope.taxList = response.data;
            if (baseService.arrayLength(response.data) > 0) {
                $scope.HSNCode = response.data[0]['HSNCode'];
            }
            //for (var i = 0; i < baseService.arrayLength($scope.taxList); i++) {
            //    $scope.taxList[i].TaxAmount = $scope.SoTotalAmount * (parseFloat($scope.taxList[i].Percentage) / 100);
            //}
            $scope.total = 0;
            $scope.totals = 0;
            for (var j = 0; j < $scope.taxList.length; j++) {
                $scope.totals = $scope.totals + $scope.taxList[j].TaxAmount;
            }
            $scope.total = $scope.totals.toFixed(2);

        });
    }

    $scope.calculateTaxAmount = function (data) {
        //data.TotalAmount = Math.round($scope.taxAbleAmnt * data.Percentage) / 100;
        data.TaxAmount = Math.round($scope.SoTotalAmount * data.Percentage) / 100;
    };

    $scope.dindex = -1;
    $scope.removeTax = function (id, index) {
        $scope.tempId = id;
        $scope.delindex = index;
        if (baseService.isUndefinedOrNull($scope.tempId))
            $scope.message = 'Are you sure want to delete?';
        else
            $scope.message = 'Are you sure want to delete?';
        angular.element(document.querySelector('#removPopUp')).modal('show');
    };

    $scope.removeTaxRow = function () {
        $scope.Del($scope.tempId, $scope.delindex);
        angular.element(document.querySelector('#removPopUp')).modal('hide');
    };


    $scope.Del = function (id, delindex) {
        $scope.dindex = delindex;
        for (var i = 0; i < $scope.taxList.length; i++) {
            if ($scope.taxList[i].Id === id) {
                $scope.taxList.splice($scope.dindex, 1);
                return true;
                break;
            }
        }
        $scope.dindex = -1;
    };



    //#endregion Sales Order Tax

    // #region PO Number

    $scope.getPOSearchData = function () {
        $http({
            method: 'GET'
            , url: $scope.path + 'GetListByMasterOrder/'
            , params: {
                companyId: $window.companyId
                , masterOrderId: $scope.ModelNew.Id
            }
        }).then(function successCallback(response) {
            $scope.customerPOlsit = response.data;
            angular.element(document.querySelector('#poSearchPopup')).modal('show');
        });
    };

    $scope.getPOData = function (id, poNumber) {
        $scope.soModel.CustomerPOId = id;
        $scope.soModel.PONumber = poNumber;
        angular.element(document.querySelector('#poSearchPopup')).modal('hide');
    };

    $scope.poFgEntryPopup = function () {
        $scope.poModel = {
            Id: null
            , PONumber: null
            , CustomerId: $scope.ModelNew.PartyId
            , CompanyGroupId: $window.companyGroupId
            , CompanyId: $window.companyId
            , MasterOrderId: $scope.ModelNew.Id
            , PODate: null
            , Active: null
        };
        angular.element(document.querySelector('#poEntryPopup')).modal('show');
    };

    $scope.SavePO = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.poModel.PONumber)) throw "[PO No] can not be blank...";
            if (baseService.isUndefinedOrNull($scope.poModel.PODate)) throw "[PO Date] can not be blank...";
            $http({
                method: 'POST'
                , url: $scope.path + 'CreatePO'
                , data: $scope.poModel
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'poEntryPopup');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'poEntryPopup');

                    $scope.soModel.CustomerPOId = response.data.tuple.Item1;
                    $scope.soModel.PONumber = response.data.tuple.Item2;

                    angular.element(document.querySelector('#poEntryPopup')).modal('hide'); //Hide Detail Add/Edit Modal
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure', 'poEntryPopup');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure', "poEntryPopup");
        }
    };

    // #endregion PO Number

    //#region Characteristics 

    $scope.clearCharNames = function () {
        $scope.char1 = {};
        $scope.char2 = {};
        $scope.char3 = {};
    };

    $scope.getSku = function (salesOrderId, hasFirst, soItemQty) {
        $scope.salesOrderId = salesOrderId;
        $scope.rowName = null;
        $scope.columnName = null;
        $scope.rowNo = null;
        $scope.columnNo = null;
        $scope.clearCharNames();
        $scope.skuList = [];
        $scope.firstSKUList = [];
        $scope.soItemCurentSkuQty = soItemQty;
        if (hasFirst === 0) {
            $http.get($scope.path + 'getcharacteristicsbymaterialmasterid?materialMasterId=' + $scope.materialMasterId)
                .then(function (response) {
                    $scope.characteristicsList = [];
                    $scope.characteristicsList = response.data;
                    if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = [];
                        $scope.addFirstSkuList();
                        angular.element(document.querySelector('#firstPopup')).modal('show');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.colorCharacteristicsId = $scope.characteristicsList[0].Value;
                        $scope.columnName = $scope.characteristicsList[1].Text;
                        $scope.sizeCharacteristicsId = $scope.characteristicsList[1].Value;
                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                        angular.element(document.querySelector('#fourthPopup')).modal('show');

                        $scope.rowNo = 1;
                        $scope.columnNo = 1;
                        $scope.generate();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;
                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        generateCharPopUp();
                    }
                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: $scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                            , FirstCharacteristicsId: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }

                });
        }
        else {
            $http.get($scope.path + 'getAllSkuSalesOrderId?salesOrderId=' + salesOrderId)
                .then(function (response) {
                    var firstData = response.data.firstData;
                    var secondtData = response.data.secondtData;
                    var thirdData = response.data.thirdData;
                    $scope.characteristicsList = [];

                    if (baseService.arrayLength(firstData) > 0) {
                        $scope.characteristicsList.push({
                            Value: firstData[0].CharacteristicsId
                            , Text: firstData[0].CharacteristicsName
                            , CharacteristicsValueId: null //firstData[0].CharacteristicsValueId
                            , ValueFreeText: null //firstData[0].ValueFreeText
                            , ValueAssignmentLevel: firstData[0].ValueAssignmentLevel
                            , MaterialMasterId: firstData[0].MaterialMasterId
                            , Qty: null //firstData[0].Qty
                            , FirstCharacteristicsId: null //firstData[0].Id
                        });
                    }
                    if (baseService.arrayLength(secondtData) > 0) {
                        $scope.characteristicsList.push({
                            Value: secondtData[0].CharacteristicsId
                            , Text: secondtData[0].CharacteristicsName
                            , CharacteristicsValueId: secondtData[0].CharacteristicsValueId
                            , ValueFreeText: secondtData[0].ValueFreeText
                            , ValueAssignmentLevel: secondtData[0].ValueAssignmentLevel
                            , MaterialMasterId: secondtData[0].MaterialMasterId
                            , Qty: secondtData[0].Qty
                        });
                    }
                    if (baseService.arrayLength(thirdData) > 0) {
                        $scope.characteristicsList.push({
                            Value: thirdData[0].CharacteristicsId
                            , Text: thirdData[0].CharacteristicsName
                            , CharacteristicsValueId: thirdData[0].CharacteristicsValueId
                            , ValueFreeText: thirdData[0].ValueFreeText
                            , ValueAssignmentLevel: thirdData[0].ValueAssignmentLevel
                            , MaterialMasterId: thirdData[0].MaterialMasterId
                            , Qty: thirdData[0].Qty
                        });
                    }

                    if (baseService.arrayLength($scope.characteristicsList) !== 0) {
                        $scope.char1 = {
                            Id: $scope.characteristicsList[0].Value
                            , Name: $scope.characteristicsList[0].Text
                            , CharacteristicsValueId: null //$scope.characteristicsList[0].CharacteristicsValueId
                            , ValueFreeText: null //$scope.characteristicsList[0].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null //$scope.characteristicsList[0].Qty
                            , FirstCharacteristicsId: null //$scope.characteristicsList[0].FirstCharacteristicsId
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 1) {
                        $scope.char2 = {
                            Id: $scope.characteristicsList[1].Value
                            , Name: $scope.characteristicsList[1].Text
                            , CharacteristicsValueId: $scope.characteristicsList[1].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[1].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };
                    }
                    if (baseService.arrayLength($scope.characteristicsList) > 2) {
                        $scope.char3 = {
                            Id: $scope.characteristicsList[2].Value
                            , Name: $scope.characteristicsList[2].Text
                            , CharacteristicsValueId: $scope.characteristicsList[2].CharacteristicsValueId
                            , ValueFreeText: $scope.characteristicsList[2].ValueFreeText
                            , ValueAssignmentLevel: $scope.characteristicsList[0].ValueAssignmentLevel
                            , MaterialMasterId: $scope.characteristicsList[0].MaterialMasterId
                            , Qty: null
                        };

                    }

                    if (baseService.arrayLength($scope.characteristicsList) === 3) {
                        $scope.firstSkuEdit(firstData[0]);
                        getSkuMatrix(secondtData, thirdData);
                        $scope.rowName = $scope.characteristicsList[1].Text;
                        $scope.columnName = $scope.characteristicsList[2].Text;
                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('show');
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 2) {
                        getSkuMatrix(firstData, secondtData);
                        $scope.rowName = $scope.characteristicsList[0].Text;
                        $scope.columnName = $scope.characteristicsList[1].Text;
                        //angular.element(document.querySelector('#firstPopup')).modal('hide');
                        //angular.element(document.querySelector('#secondPopup')).modal('show');
                        //angular.element(document.querySelector('#thirdPopup')).modal('hide');

                        angular.element(document.querySelector('#firstPopup')).modal('hide');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                        angular.element(document.querySelector('#fourthPopup')).modal('show');
                        $scope.sumTwoMatQuantity();
                    }
                    else if (baseService.arrayLength($scope.characteristicsList) === 1) {
                        $scope.firstSKUList = firstData;
                        angular.element(document.querySelector('#firstPopup')).modal('show');
                        angular.element(document.querySelector('#secondPopup')).modal('hide');
                        angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    }
                });
        }
        $http.get($scope.path + 'GetChValueCbo?materialId=' + $scope.materialMasterId)
            .then(function (response) {
                $scope.charValueList = [];
                $scope.charValueList = response.data;
            });
    };

    function generateCharPopUp() {
        angular.element(document.querySelector('#generatePopup')).modal('show');
    }

    $scope.generate = function () {
        var firstCharId = '';
        for (var i = 0; i < $scope.rowNo; i++) {
            firstCharId = '-' + (i + 1);
            $scope.skuList.push(
                {
                    Id: firstCharId
                    , SalesOrderId: $scope.salesOrderId
                    , FirstCharacteristicsId: null
                    , SecondCharacteristicsId: null
                    , CharacteristicsId: $scope.colorCharacteristicsId
                    , CharacteristicsValueId: null
                    , ValueFreeText: null
                    , Sequence: i + 1
                    , Qty: null
                    , childList: []
                    , Flag: null
                }
            );
            for (var t = 0; t < $scope.columnNo; t++) {
                $scope.skuList[i].childList.push(
                    {
                        Id: null
                        , SalesOrderId: $scope.salesOrderId
                        , FirstCharacteristicsId: baseService.arrayLength($scope.characteristicsList) === 2 ? firstCharId : null
                        , SecondCharacteristicsId: baseService.arrayLength($scope.characteristicsList) === 3 ? firstCharId : null
                        , CharacteristicsId: $scope.sizeCharacteristicsId
                        , CharacteristicsValueId: null
                        , ValueFreeText: null
                        , Sequence: t + 1
                        , Qty: null
                    }
                );
            }
        }

        angular.element(document.querySelector('#generatePopup')).modal('hide');
        if (baseService.arrayLength($scope.characteristicsList) === 3)
            angular.element(document.querySelector('#thirdPopup')).modal('show');
        else
            angular.element(document.querySelector('#secondPopup')).modal('show');
    };

    function getSkuMatrix(rowDataList, columnDataList) {
        for (var i = 0; i < baseService.arrayLength(rowDataList); i++) {
            $scope.skuList.push({
                Id: rowDataList[i].Id
                , SalesOrderId: rowDataList[i].SalesOrderId
                , FirstCharacteristicsId: rowDataList[i].FirstCharacteristicsId
                , SecondCharacteristicsId: rowDataList[i].SecondCharacteristicsId
                , CharacteristicsId: rowDataList[i].CharacteristicsId
                , CharacteristicsValueId: rowDataList[i].CharacteristicsValueId
                , ValueFreeText: rowDataList[i].ValueFreeText
                , Sequence: rowDataList[i].Sequence
                , Qty: rowDataList[i].Qty
                , childList: []
                , Flag: null
            });
            for (var t = 0; t < baseService.arrayLength(columnDataList); t++) {
                if (columnDataList[t].FirstCharacteristicsId === rowDataList[i].Id || columnDataList[t].SecondCharacteristicsId === rowDataList[i].Id) {
                    $scope.skuList[i].childList.push({
                        Id: columnDataList[t].Id
                        , SalesOrderId: columnDataList[t].SalesOrderId
                        , FirstCharacteristicsId: columnDataList[t].FirstCharacteristicsId
                        , SecondCharacteristicsId: columnDataList[t].SecondCharacteristicsId
                        , CharacteristicsId: columnDataList[t].CharacteristicsId
                        , CharacteristicsValueId: columnDataList[t].CharacteristicsValueId
                        , ValueFreeText: columnDataList[t].ValueFreeText
                        , Sequence: columnDataList[t].Sequence
                        , Qty: columnDataList[t].Qty
                    });
                }
            }
        }
    }

    $scope.addSkuMatrixColumn = function () {
        var t = 0;

        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList.push({
                Id: null
                , SalesOrderId: $scope.salesOrderId
                , FirstCharacteristicsId: null
                , SecondCharacteristicsId: null
                , CharacteristicsId: $scope.char2.Id
                , CharacteristicsValueId: null
                , ValueFreeText: null
                , Sequence: i + 1
                , Qty: null
            });
        }
    }

    $scope.removeSkuMatrixColumn = function (index) {
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList.splice(index, 1);
        }
        $scope.verifySkuMatrix();
    }

    $scope.addSkuMatrixRow = function () {
        var skuChildList = [];
        for (var i = 0; i < baseService.arrayLength($scope.skuList[0].childList); i++) {
            skuChildList.push({
                Id: null
                , SalesOrderId: $scope.salesOrderId
                , FirstCharacteristicsId: null
                , SecondCharacteristicsId: null
                , CharacteristicsId: $scope.char2.Id
                , CharacteristicsValueId: $scope.skuList[0].childList[i].CharacteristicsValueId
                , ValueFreeText: null
                , Sequence: $scope.skuList[0].childList[i].Sequence
                , Qty: null
            });
        }

        $scope.skuList.push({
            Id: '-' + (baseService.arrayLength($scope.skuList) + 1)
            , SalesOrderId: $scope.salesOrderId
            , FirstCharacteristicsId: null
            , SecondCharacteristicsId: null
            , CharacteristicsId: $scope.char1.Id
            , CharacteristicsValueId: $scope.char1.CharacteristicsValueId
            , Sequence: (baseService.arrayLength($scope.skuList) + 1)
            , ValueFreeText: $scope.char1.ValueFreeText
            , Qty: null
            , Flag: '1st'
            , childList: skuChildList
        });
    }

    $scope.removeSkuMatrixRow = function (index) {
        $scope.skuList.splice(index, 1);
        $scope.verifySkuMatrix();
    }

    $scope.verifySkuMatrix = function () {

        $scope.IsSkuColumnIsValid = true;
        if ($scope.skuList[0].childList.length > 1) {
            for (var i = 0; i < $scope.skuList[0].childList.length; i++) {
                var count = 0;
                var iSKU = $scope.skuList[0].childList[i];
                for (var j = 0; j < $scope.skuList[0].childList.length; j++) {
                    var skuChild = $scope.skuList[0].childList[j];
                    if (skuChild.ValueFreeText != null && skuChild.CharacteristicsValueId != null && iSKU.ValueFreeText != null && iSKU.CharacteristicsValueId != null)
                        if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                            count++;
                        }
                }

                if (count > 1) {
                    $scope.skuList[0].childList[i].isDuplicate = true;
                }
                else {
                    $scope.skuList[0].childList[i].isDuplicate = false;
                }
            }
            if (findWithAttr($scope.skuList[0].childList, 'isDuplicate', true) >= 0) {
                $scope.IsSkuColumnIsValid = false;
            }
            else {
                $scope.IsSkuColumnIsValid = true;
            }
        }

        $scope.IsSkuRowIsValid = true;
        if ($scope.skuList.length > 1) {
            for (var i = 0; i < $scope.skuList.length; i++) {
                var count = 0;
                var iSKU = $scope.skuList[i];
                for (var j = 0; j < $scope.skuList.length; j++) {
                    var skuChild = $scope.skuList[j];
                    if (skuChild.ValueFreeText != null && skuChild.CharacteristicsValueId != null && iSKU.ValueFreeText != null && iSKU.CharacteristicsValueId != null)
                        if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                            count++;
                        }
                }

                if (count > 1) {
                    $scope.skuList[i].isDuplicate = true;
                }
                else {
                    $scope.skuList[i].isDuplicate = false;
                }

            }
            if (findWithAttr($scope.skuList, 'isDuplicate', true) >= 0) {
                $scope.IsSkuRowIsValid = false;
            }
            else {
                $scope.IsSkuRowIsValid = true;
            }
        }
    }


    function findWithAttr(array, attr, value) {
        for (var i = 0; i < array.length; i += 1) {
            if (array[i][attr] === value) {
                return i;
            }
        }
        return -1;
    }


    $scope.charSave = function (charLength) {
        //if (baseService.arrayLength($scope.characteristicsList) > 2 || baseService.arrayLength($scope.characteristicsList) === 1) {
        //    var data = $filter('filter')($scope.skuList, { Flag: '1st' }, true);
        //    var qty = 0;
        //    if (baseService.arrayLength($scope.characteristicsList) > 2) qty = parseFloat($filter('sumByKey')($scope.skuList, 'Qty', true));
        //    if (baseService.arrayLength($scope.characteristicsList) === 1) qty = $scope.char1.Qty;
        //    if (baseService.arrayLength(data) === 0) {
        //        $scope.skuList.unshift({
        //            Id: $scope.char1.FirstCharacteristicsId
        //            , SalesOrderId: $scope.salesOrderId
        //            , FirstCharacteristicsId: null
        //            , SecondCharacteristicsId: null
        //            , CharacteristicsId: $scope.char1.Id
        //            , CharacteristicsValueId: $scope.char1.CharacteristicsValueId
        //            , Sequence: 1
        //            , ValueFreeText: $scope.char1.ValueFreeText
        //            , Qty: qty
        //            , Flag: '1st'
        //            , childList: []
        //        });
        //    }
        //    else if (baseService.arrayLength(data) === 1) {
        //        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
        //            if ($scope.skuList[i].CharacteristicsId === $scope.char1.Id) {
        //                $scope.skuList[i].CharacteristicsValueId = $scope.char1.CharacteristicsValueId;
        //                $scope.skuList[i].ValueFreeText = $scope.char1.ValueFreeText;
        //                $scope.skuList[i].Qty = qty;
        //            }
        //        }
        //    }
        //}

        if (charLength == 1) {
            var totlQty = 0;
            for (var i = 0; i < baseService.arrayLength($scope.firstSKUList); i++) {
                if (($scope.firstSKUList[i].ValueFreeText == null || $scope.firstSKUList[i].ValueFreeText == "") && $scope.firstSKUList[i].CharacteristicsValueId == null) {
                    ShowResult("SKU item can't be blank", 'failure', 'firstPopup');
                    return false;
                }
                $scope.firstSKUList[i].IsQtyZero = $scope.firstSKUList[i].Qty <= 0;
                if ($scope.firstSKUList[i].IsQtyZero) {
                    ShowResult("SKU quantity can't be zero", 'failure', 'firstPopup');
                    return false;
                }
                totlQty = totlQty + $scope.firstSKUList[i].Qty;
            }
            if (totlQty > $scope.soItemCurentSkuQty) {
                ShowResult("Sum of SKU quantity can't be greater than " + $scope.soItemCurentSkuQty, 'failure', 'firstPopup');
                return false;
            }

            if (!$scope.IsSkuFormIsValid) {
                ShowResult("Duplicate data", 'failure', 'firstPopup');
                return false;
            }

            $scope.skuList = $scope.firstSKUList;

        } else {


            if ($scope.skuList.length == 1) {
                for (var i = 0; i < $scope.skuList[0].childList.length; i++) {
                    if ($scope.skuList[0].childList[i].Qty <= 0) {
                        ShowResult("SKU quantity can't be zero", 'failure', 'fourthPopup');
                        return false;
                    }
                }
            }

            if ($scope.skuList.length >= 1) {

                for (var j = 0; j < $scope.skuList.length; j++) {
                    var skuPar = $scope.skuList[j];
                    if ((skuPar.ValueFreeText == null || skuPar.ValueFreeText == "") && skuPar.CharacteristicsValueId == null) {
                        ShowResult("SKU item can't be blank", 'failure', 'fourthPopup');
                        return false;
                    }
                    if ($scope.skuList[j].childList.length > 1 && j == 0) {
                        for (var i = 0; i < $scope.skuList[j].childList.length; i++) {
                            var skuChild = $scope.skuList[j].childList[i];
                            if ((skuChild.ValueFreeText == null || skuChild.ValueFreeText == "") && skuChild.CharacteristicsValueId == null) {
                                ShowResult("SKU item can't be blank", 'failure', 'fourthPopup');
                                return false;
                            }
                        }
                    }
                }
            }



            if (baseService.arrayLength($scope.skuList) > 0) {
                $scope.verifySkuMatrix();
                if (!$scope.IsSkuColumnIsValid || !$scope.IsSkuRowIsValid) {
                    ShowResult("Duplicate data", 'failure', 'fourthPopup');
                    return false;
                }
            }

        }

        $http({
            method: 'POST'
            , url: $scope.path + 'CreateCharacteristics'
            , data: {
                'entities': $scope.skuList
                , 'listLength': charLength
                , 'soId': $scope.salesOrderId
            }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                if (baseService.arrayLength($scope.characteristicsList) == 1) {
                    ShowResult(response.data.Message, 'failure', 'firstPopup');
                }
                else {
                    $scope.closeCharPopUp();
                    showCharMessage(response.data.Message, 'failure');
                }
            }
            else {
                if (baseService.arrayLength($scope.characteristicsList) === 1) {
                    getFirstSkuList($scope.salesOrderId);
                    $scope.char1.FirstCharacteristicsId = null;
                    $scope.char1.CharacteristicsValueId = null;
                    $scope.char1.ValueFreeText = null;
                    $scope.char1.CharacteristicsValueName = null;
                    $scope.char1.Qty = null;
                    if (baseService.arrayLength($scope.firstSKUList) === 1) {
                        for (var i = 0; i < baseService.arrayLength($scope.salesOrderList); i++) {
                            if ($scope.salesOrderId === $scope.salesOrderList[i].Id) {
                                $scope.salesOrderList[i].hasFirst = 1;
                                break;
                            }
                        }
                    }
                    showCharMessage(response.data.Message, 'success');
                    $scope.getSalesOrder($scope.masterItemId, $scope.materialMasterId, $scope.mName);
                }
                else {
                    angular.element(document.querySelector('#firstPopup')).modal('hide');
                    angular.element(document.querySelector('#secondPopup')).modal('hide');
                    angular.element(document.querySelector('#thirdPopup')).modal('hide');
                    for (var t = 0; t < baseService.arrayLength($scope.salesOrderList); t++) {
                        if ($scope.salesOrderId === $scope.salesOrderList[t].Id) {
                            $scope.salesOrderList[t].hasFirst = 1;
                            break;
                        }
                    }
                    $scope.salesOrderId = null;
                    $scope.skuList = [];
                    $scope.closeCharPopUp();
                    showCharMessage(response.data.Message, 'success');
                    $scope.getSalesOrder($scope.masterItemId, $scope.materialMasterId, $scope.mName);
                }

            }
        }), function errorCallBack(response) {
            showCharMessage(response.data.Message, 'failure');
        };
    };

    function showCharMessage(message, state) {
        if (baseService.arrayLength($scope.characteristicsList) === 3) ShowResult(message, state, 'thirdPopup');
        if (baseService.arrayLength($scope.characteristicsList) === 2) ShowResult(message, state, 'soPoUp');
        if (baseService.arrayLength($scope.characteristicsList) === 1) ShowResult(message, state, 'firstPopup');
    }

    $scope.sumQty = function (parentData, parentIndex) {
        var tqty = parseFloat($filter('sumByKey')(parentData.childList, 'Qty', true));
        parentData.Qty = isNaN(tqty) ? 0 : parseFloat(tqty);
        $scope.sumTwoMatQuantity();
    };

    $scope.skuTwoMatQuantity = 0;
    $scope.sumTwoMatQuantity = function () {
        var tqty = parseFloat(0);
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            for (var j = 0; j < baseService.arrayLength($scope.skuList[i].childList); j++) {
                tqty = tqty + $scope.skuList[i].childList[j].Qty;
            }
        }
        $scope.skuTwoMatQuantity = isNaN(tqty) ? 0 : parseFloat(tqty);
        if ($scope.skuTwoMatQuantity > $scope.soItemCurentSkuQty) {
            ShowResult("Sum of SKU quantity can't be greater than " + $scope.soItemCurentSkuQty, 'failure', 'fourthPopup');
        }

    };

    $scope.chvChange = function (characteristicsValueId, index) {
        for (var i = 1; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList[index].CharacteristicsValueId = characteristicsValueId;
        }
    };

    $scope.chvKeyChange = function (value, index) {
        for (var i = 0; i < baseService.arrayLength($scope.skuList); i++) {
            $scope.skuList[i].childList[index].ValueFreeText = value;
        }
    };

    $scope.closeCharPopUp = function () {
        $scope.firstSKUList = [];
        $scope.skuList = [];
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
        angular.element(document.querySelector('#fourthPopup')).modal('hide');
    };

    $scope.firstSkuEdit = function (data) {
        $scope.char1.FirstCharacteristicsId = data.Id;
        $scope.char1.CharacteristicsValueId = data.CharacteristicsValueId;
        $scope.char1.ValueFreeText = data.ValueFreeText;
        $scope.char1.CharacteristicsValueName = data.CharacteristicsValueName;
        $scope.char1.Qty = data.Qty;
    };

    function getFirstSkuList() {
        $http.get($scope.path + 'GetFirstSkuList?salesOrderId=' + $scope.salesOrderId)
            .then(function (response) {
                $scope.firstSKUList = [];
                $scope.firstSKUList = response.data;
            });
    }

    $scope.firstSKUList = [];
    $scope.addFirstSkuList = function () {
        $scope.firstSKUList.push({
            Id: null
            , SalesOrderId: $scope.salesOrderId
            , FirstCharacteristicsId: null
            , SecondCharacteristicsId: null
            , CharacteristicsId: 1
            , CharacteristicsValueId: null
            , Sequence: (baseService.arrayLength($scope.firstSKUList) + 1)
            , ValueFreeText: null
            , Qty: null
            , Flag: null
        });

    }

    $scope.removeFirstSkuList = function (id, index) {
        $scope.firstSKUList.splice(index, 1);
        $scope.verifyFirstSkuList();
    }

    $scope.IsSkuFormIsValid = true;
    $scope.verifyFirstSkuList = function () {
        for (var i = 0; i < $scope.firstSKUList.length; i++) {
            var iSKU = $scope.firstSKUList[i];
            var count = 0;
            for (var j = 0; j < $scope.firstSKUList.length; j++) {
                var skuChild = $scope.firstSKUList[j];
                if (iSKU.ValueFreeText == null) {
                    iSKU.ValueFreeText = "";
                }
                if (skuChild.ValueFreeText == null) {
                    skuChild.ValueFreeText = "";
                }

                if (iSKU.ValueFreeText.toLowerCase() == skuChild.ValueFreeText.toLowerCase() && iSKU.CharacteristicsValueId == skuChild.CharacteristicsValueId) {
                    count++;
                }

            }
            if (count > 1) {
                $scope.firstSKUList[i].isDuplicate = true;
                $scope.IsSkuFormIsValid = false;
            }
            else {
                $scope.firstSKUList[i].isDuplicate = false;
                $scope.IsSkuFormIsValid = true;
            }

        }
    }

    $scope.firstSkuClosePopUp = function () {
        $scope.skuList = [];
        $scope.firstSKUList = [];
        $scope.salesOrderId = null;
        angular.element(document.querySelector('#firstPopup')).modal('hide');
        angular.element(document.querySelector('#secondPopup')).modal('hide');
        angular.element(document.querySelector('#thirdPopup')).modal('hide');
    };

    //#endregion Characteristics 

    //#region Generic

    $scope.genericDelete = function (id, flag) {
        $scope.id = id;
        $scope.message_confirmation = "Are you sure want to permanently delete ";
        angular.element(document.querySelector('#genericConfirm')).modal('show');
        $scope.flag = flag;
    };

    $scope.genericRemove = function () {
        if ($scope.flag === 'item')
            $scope.deleteItem();
        else if ($scope.flag === 'so')
            $scope.deleteSO();
        else if ($scope.flag === 'first')
            $scope.firstSkuDelete();
    };

    //#endregion Generic



    //----------Inquiry Process
    $scope.inquiryProcessList = [];
    $scope.inquiryMasterId = null;
    $scope.inquiryItemId = null;
    $scope.selectedLineItem = null;

    $scope.GetInquiryProcessList = function (inquiryMasterId, inquiryItemId, lineData) {
        $scope.selectedLineItem = lineData;

        $http({
            method: 'POST',
            url: $scope.path + "GetInquiryProcessList",
            data: {
                inquiryMasterId: inquiryMasterId,
                inquiryItemId: inquiryItemId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.inquiryMasterId = inquiryMasterId;
            $scope.inquiryItemId = inquiryItemId;
            $scope.inquiryProcessList = response.data;

            for (var i = 0; i < $scope.inquiryProcessList.length; i++) {
                var saveList = ej.DataManager($scope.selectedLineItem.InquiryProcessList).executeLocal(ej.Query().where("ProcessName", "equal", $scope.inquiryProcessList[i]["ProcessName"]));
                if (saveList.length > 0) {
                    $scope.inquiryProcessList[i]["IsApplicable"] = saveList[0]["IsApplicable"];
                }

            }
            $scope.selectedLineItem.InquiryProcessList = [];
            for (var i = 0; i < $scope.inquiryProcessList.length; i++) {
                $scope.selectedLineItem.InquiryProcessList.push(Object.assign({}, $scope.inquiryProcessList[i]));
            }

        });



        angular.element(document.querySelector('#InquiryProcessModal')).modal('show');
    };


    $scope.SaveInquiryProcessList = function () {
        $scope.selectedLineItem.InquiryProcessList = [];
        for (var i = 0; i < $scope.inquiryProcessList.length; i++) {
            $scope.selectedLineItem.InquiryProcessList.push(Object.assign({}, $scope.inquiryProcessList[i]));
        }

        angular.element(document.querySelector('#InquiryProcessModal')).modal('hide');

    };

    //
}


