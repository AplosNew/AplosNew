
'use strict';
independentOrderController.$inject = ['accountService', '$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$controller'];
function independentOrderController(accountService, $window, cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $controller) {
    $rootScope.title = "Independent Order";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.files = [];
    $scope.orderCategoryList = [];
    $scope.orderStatusList = [];
    $scope.searchMasterFilterList = [];
    $scope.personCboList = [];
    $scope.attributeList = [];
    $scope.personList = [];

    $scope.path = 'OrderManagements/masterorder/';
    $scope.getListUrl = $scope.path + 'GetIdependentList';
    $scope.saveUrl = $scope.path + 'CreateIndependent';
    $scope.updateUrl = $scope.path + 'CreateIndependent';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListResponsible';
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });
    $controller("TaskScheduleController", { cboService: cboService, $scope: $scope, $http: $http });
    
    $scope.file = {
        Id: null
        , CompanyId: $window.companyId
        , PlantId: $window.plantId
        , EntityId: null
        , CommitmentId: null
        , InquiryId: null
        , PartyId: null
        , BuyerId: null
        , BuyerBrandId: null
        , BuyerDivisionId: null
        , BuyerDepartmentId: null
        , TestingStandardId: null
        , MasterOrderNo: null
        , OrderStatusId: null
        , OrderCategoryId: null
        , SeasonId: null
        , OrderYear: null
        , CurrencyId: null
        , OrderType: 'Independent'
        , TotalQty: null
        , NoOfLineItem: null
        , ResponsiblePersonId: null
        , ResponsiblePersonName: null
        , InvoicingPartyPlantId: null
        , InvoicingByAddress: null
        , InvoicingState: null
        , InvoicingGSTIN: null
        , DeliveryPartyPlantId: null
        , DeliveryByAddress: null
        , DeliveryState: null
        , DeliveryGSTIN: null
        , OrderWastagePercentage: null
        , ExtraOrderPercentage: null
        , IsExtraOrderPercentage: false
        , TotalQtyUOMId: null
        , IsReplacement: false
        , Type: null
        , SpecialTaxId: null
        , BuyerReferenceNo: null
        , OwnReferenceNo: null
    };
    $scope.fileNew = Object.assign({}, $scope.file);

    $scope.getData = function () {
        $rootScope.parameters.companyId = $scope.fileNew.CompanyId;
        baseService.init($scope.getListUrl, null, null, null, 'MasterOrderNo', 'MasterOrderNo');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.files = result.Rows;
                    if (baseService.arrayLength($scope.searchMasterFilterList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchMasterFilterList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    };
    $scope.getData();
  
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
                $scope.fileNew.OrderYear = $scope.yearList[i].Text;
            }
        }

    };
    $scope.getYearOfHaving();

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (response) {
        $scope.companyList = response;
    });

    //$scope.plantList = [];
    //$scope.getPlantCbo = function () {
    //    cboService.getCboPlantByCompany($scope.fileNew.CompanyId, function (response) {
    //        $scope.plantList = response;
    //    });
    //};

    //$scope.specialTaxList = [];
    //$scope.getSpecialTaxByPlantCbo = function () {
    //    cboService.getCboSpecialTaxByPlant($scope.fileNew.PlantId, function (response) {
    //        $scope.specialTaxList = response;
    //    });
    //};

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
        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.fileNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
            $scope.divisionList = result;
        });
        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
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
            //$scope.GetResponsiblePersonList();
        });
    }
    $scope.getAllEntities();


    $scope.getPlantConfigByPlant = function () {
        $scope.isBuyerApplicable = false;
        $scope.fileNew.BuyerId = null;
        $scope.fileNew.BuyerDivisionId = null;
        $scope.fileNew.BuyerBrandId = null;
        $scope.fileNew.TestingStandardId = null;
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/GetPlantConfigDataByPlantId?plantid=' + $window.plantId
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0)
                $scope.isBuyerApplicable = response.data[0].BuyerApplicable;
        });
    };
    $scope.getPlantConfigByPlant();

    //$scope.irregularList = [];
    //$http.get("OrderManagements/MasterOrder/GetSpecialTaxList?plantId=" + $window.plantId)
    //    .then(function (response) {
    //        $scope.irregularList = response.data;
    //    });

    $scope.taskList = [];
    $scope.GEEMasterOrderId = '';
    $scope.GVMasterOrderId = '';
    //$scope.tabTNA = 1;
    //$scope.setTabTNA = function (newTab) {
    //    $scope.tabTNA = newTab;
    //};
    //$scope.isSetTNA = function (tabNum) {
    //    return $scope.tabTNA === tabNum;
    //};
    //$scope.onactivetab = function (args) {
    //    if (args.activeIndex == 0)
    //        $scope.GEEGetSelectedTasks($scope.fileNew.Id);
    //    else
    //        $scope.GVGetSelectedTasks2($scope.fileNew.Id);
    //}
    //$scope.getTaskList = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.fileNewForm.$valid) {

    //        if ($scope.fileNew.Id != null) {
    //            $("#dialogViewTNADetail").data("ejDialog").open();
    //            $scope.GEEMasterOrderId = $scope.fileNew.Id;
    //            $scope.GVMasterOrderId = $scope.fileNew.Id;

    //            $scope.GEEGetSelectedTasks($scope.fileNew.Id);
    //            $scope.GVGetSelectedTasks2($scope.fileNew.Id);


    //        }
    //    }
    //}

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

    cboService.getCboTransactionCurrencyByCompany('', function (result) {
        $scope.currencyList = [];
        $scope.currencyList = result;
        $scope.fileNew.CurrencyId = $filter("filter")($scope.currencyList, { IsBaseCurrency: 1 })[0].CurrencyId;
    });

    // #endregion Ddl

    //$scope.tab = 1;
    //$scope.setTab = function (newTab) {
    //    $scope.tab = newTab;
    //};
    //$scope.isSet = function (tabNum) {
    //    return $scope.tab === tabNum;
    //};

    $scope.Get = function (index) {
        $scope.getPlantConfigByPlant();
        $scope.index = index;
        angular.copy($scope.files[$scope.index], $scope.file);
        $scope.file.IsExtraOrderPercentage = $scope.file.ExtraOrderPercentage > 0;
        angular.copy($scope.file, $scope.fileNew);
        $scope.fileNew.OrderYear = parseInt($scope.fileNew.OrderYear);
        $scope.Action = 'Update';
        getPartyPlantList();
        $scope.GetResponsiblePersonList();
        //GetDepartmentPersonCbo();
       // getMasterItemList();
        $scope.getAllEntities();
        $scope.buyerChange();


        $http.get("Parties/BuyerBrand/GetCbo?buyerId=" + $scope.fileNew.BuyerId)
            .then(function (response) {
                $scope.brandList = response.data;
            });
        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
            $scope.divisionList = result;

            cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                $scope.departmentList = result;
            });
        });
        if (!$rootScope.isCollapsed) $rootScope.toggle();
        $scope.currency = $scope.fileNew.Currency;
        $scope.currency = $("#Currency option:selected").text();

        //cboService.getCboSpecialTaxByPlant($scope.fileNew.PlantId, function (response) {
        //    $scope.specialTaxList = response;
        //});

        if ($scope.fileNew.IsExtraOrderPercentage === false) {
            $scope.fileNew.ExtraOrderPercentage = 0;
        }

        //if (!baseService.isUndefinedOrNull($scope.fileNew.SpecialTaxId)) {
        //    $scope.fileNew.SpecialTaxId = $scope.fileNew.SpecialTaxId;
        //    $scope.SpecialTax = true;
        //} else {
        //    $scope.SpecialTax = false;
        //}
        $scope.mmChangeFlag = false;


    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $scope.customerName = $scope.fileNew.CustomerName;
        $scope.ResponsiblePersonName = $scope.fileNew.ResponsiblePersonName;
        $scope.ResponsiblePersonId = $scope.fileNew.ResponsiblePersonId;
        if ($scope.isBuyerApplicable) {
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerId)) {
                return ShowResult('Buyer is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDivisionId)) {
                return ShowResult('Division is required.', 'failure');
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.BuyerDepartmentId)) {
                return ShowResult('Department is required.', 'failure');
            }
        }

        if (parseFloat(baseService.isUndefinedOrNull($scope.fileNew.TotalQty) ? 0 : $scope.fileNew.TotalQty) === 0) return ShowResult('Please insert total qty.', 'failure');

        if (baseService.isUndefinedOrNull($scope.fileNew.TotalQtyUOMId)) {
            return ShowResult('Total Quantity UoM is required.', 'failure');
        }

        if ($scope.fileNew.IsExtraOrderPercentage && $scope.fileNew.ExtraOrderPercentage === 0)
            return ShowResult('Please insert Extra Order Percentage.', 'failure');
        if (!baseService.isUndefinedOrNull($scope.fileNew.OrderWastagePercentage)) {
            if ($scope.fileNew.OrderWastagePercentage > 99) {
                return ShowResult('Order Wastage Percentage should less than 99 Percent.', 'failure');
            }
        }

        if ($scope.fileNewForm.$valid) {

            angular.copy($scope.fileNew, $scope.file);
            if ($scope.Action === "Save") {

                

                $http({
                    method: 'POST'
                    , url: $scope.saveUrl
                    , data: { 'entity': $scope.file }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.fileNew = response.data.MasterOrder;
                        $scope.getData();
                        
                        $scope.getAllEntities();
                        $scope.Action = 'Update';
                        $scope.fileNew.CustomerName = $scope.customerName;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });
                        ClearFields();
                        
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action === "Update") {
                
                $http({
                    method: 'POST'
                    , url: $scope.updateUrl
                    , data: {
                        'entity': $scope.file
                    }
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetResponsiblePersonList();
                        $scope.getData();
                        //GetDepartmentPersonCbo();
                        $scope.getAllEntities();
                        $scope.mmChangeFlag = false;
                        $scope.fileNew.ResponsiblePersonName = $scope.ResponsiblePersonName;
                        $scope.fileNew.ResponsiblePersonId = $scope.ResponsiblePersonId;
                       
                        cboService.getBuyerDivisionCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.divisionList = result;
                        });
                        cboService.getBuyerDepartmentCboByBuyer($scope.fileNew.BuyerId, function (result) {
                            $scope.departmentList = result;
                        });
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
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
        if (!baseService.isUndefinedOrNull($scope.fileNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.fileNew.Id,
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
        $scope.file = {};
        $scope.fileNew = {
            EntityId: null
            , PlantId: $scope.fileNew.PlantId
            , OrderType: 'Independent'
            , PartyId: null
            , CompanyId: $scope.fileNew.CompanyId
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
        if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            ShowResult('Select Company', 'failure');
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
            ShowResult('Select Plant', 'failure');
            return false;
        }
        baseService.setCurrentPage('partyList');
        $scope.getPartyList = function (pageno) {
            $scope.partyUrl = $scope.path + 'GetCompanyPartyDataList?companyId=' + $scope.fileNew.CompanyId + '&plantId=' + $scope.fileNew.PlantId + '&partyType=' + $scope.partyType;
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
            $scope.fileNew.PartyCode = party.Code;
            $scope.fileNew.CustomerName = party.UserName;
            $scope.fileNew.PartyId = party.Id;
            $scope.fileNew.CurrencyId = party.CurrencyId;
            $scope.fileNew.PartyAccountGroupId = party.PartyAccountGroupId;
        }
        $scope.personList = [];
        getPartyPlantList();
        //GetDepartmentPersonCbo();
        $scope.hidePartyPopUp();
    };

    //$scope.GetResponsiblePersonList = function () {
    //    $scope.personList = [];
    //    $http.get($scope.path + "GetResponsiblePersonList?masterId=" + $scope.fileNew.Id)
    //        .then(function (response) {
    //            $scope.personList = response.data;
    //            if ($scope.fileNew.PlantId !== null && ($scope.personList === null || $scope.personList.length <= 0)) {
    //                $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId + '&flag=' + false;
    //                $scope.getPopUpData = function (pageno) {
    //                    baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
    //                        .then(function (result) {
    //                            if (baseService.arrayLength(result) !== 0) {
    //                                for (var i = 0; i < result.length; i++) {
    //                                    var obj = result[i];
    //                                    $scope.personList.push({
    //                                        Id: obj.Id
    //                                        , MasterOrderId: $scope.fileNew
    //                                        , CustomerDivisionId: obj.CustomerDivisionId
    //                                        , OrderResponsibleDepartmentId: obj.OrderResponsibleDepartmentId
    //                                        , Department: obj.Department
    //                                        , OurRespnsiblePersonId: obj.OurRespnsiblePersonId
    //                                        , EmployeeCode: obj.EmployeeCode
    //                                        , EmployeeName: obj.EmployeeName
    //                                        , PartyRespnsiblePersonId: obj.PartyRespnsiblePersonId
    //                                        , PartyRespnsiblePerson: obj.PartyRespnsiblePerson
    //                                    });
    //                                }
    //                                GetDepartmentPersonCbo();
    //                            }
    //                        }, function () {
    //                            ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
    //                        }).finally(function () {
    //                        });
    //                };
    //                $scope.getPopUpData();
    //            }
    //        });
    //};

    

    function GetDepartmentPersonCbo() {
        $scope.personCboList = [];
        $http.get($scope.path + "GetDepartmentPersonCbo?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId)
            .then(function (response) {
                $scope.personCboList = response.data;
            });
    }

    

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
        if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) return ShowResult('Select plant', 'failure');
        $scope.popUpDataList = [];
        $scope.popUpUrl = $scope.path + "GetDepartmentPersonList?plantId=" + $scope.fileNew.PlantId + '&partyAccountGroupId=' + $scope.fileNew.PartyAccountGroupId + '&partyId=' + $scope.fileNew.PartyId + '&flag=' + flag;
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
            , MasterOrderId: $scope.fileNew
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
            if (baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
                throw 'Select Company';
            }
            if (baseService.isUndefinedOrNull($scope.fileNew.PlantId)) {
                throw 'Select Plant';
            }

            $scope.Name = name;
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                $scope.employeeParameters.plantId = $scope.fileNew.PlantId;
                $scope.employeeParameters.partyAccountGroupId = $scope.fileNew.PartyAccountGroupId;
                $scope.employeeParameters.partyId = $scope.fileNew.PartyId;
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
                $scope.fileNew.ResponsiblePersonId = employee.SystemId;
                $scope.fileNew.ResponsiblePersonName = employee.EmployeeName;
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
                $scope.fileNew.InvoicingState = state;
                $scope.fileNew.InvoicingGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.fileNew.InvoicingByAddress = address;
            }
            else if (flag === 'shipTo') {
                $scope.fileNew.DeliveryState = state;
                $scope.fileNew.DeliveryGSTIN = $.grep($scope.partyPlantList, function (item) { return item.Value === id; })[0].GSTIN;
                return $scope.fileNew.DeliveryByAddress = address;
            }
        }
        else {
            if (flag === 'billTo') {
                $scope.fileNew.InvoicingState = null;
                $scope.fileNew.InvoicingGSTIN = null;
                return $scope.fileNew.InvoicingByAddress = null;
            }
            else if (flag === 'shipTo') {
                $scope.fileNew.DeliveryState = null;
                $scope.fileNew.DeliveryGSTIN = null;
                return $scope.fileNew.DeliveryByAddress = null;
            }
        }
    };

    function getPartyPlantList() {
        $scope.partyPlantList = [];
        $http.get('Parties/party/GetPartyPlantCbo?partyId=' + $scope.fileNew.PartyId).then(function (response) {
            angular.forEach(response.data, function (item) {
                $scope.partyPlantList.push(item);
                if (item.IsDefault) {
                    $scope.fileNew.InvoicingPartyPlantId = item.Value;
                    $scope.fileNew.DeliveryPartyPlantId = item.Value;
                    $scope.fileNew.InvoicingByAddress = item.Address1;
                    $scope.fileNew.DeliveryByAddress = item.Address1;
                    $scope.fileNew.InvoicingState = item.StateName;
                    $scope.fileNew.InvoicingGSTIN = item.GSTIN;
                    $scope.fileNew.DeliveryState = item.StateName;
                    $scope.fileNew.DeliveryGSTIN = item.GSTIN;
                }
            });
        });
    }

    //#endregion Party plant 

    
}


