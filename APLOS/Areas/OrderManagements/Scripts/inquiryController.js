'use strict';
inquiryController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function inquiryController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Inquiry";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.inquirys = [];
    $scope.monthList = [];
    $scope.path = 'OrderManagements/inquiry/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.inquiry = {
        Id: null
        , EntityId: null
        , BuyerId: null
        ,BuyerMasterId:null
        , BuyerDepartmentId: null
        , BuyerDivisionId: null
        , BuyerActivityId:null
        , ResponsiblePerson: null
        , EmployeeId: null
        , ResponsiblePersonId: null
        , ProductionProcessGroupId: null
        , NoOfItems: 0
        , Quantity: 0
    };
    $scope.inquiryNew = Object.assign({}, $scope.inquiry);

    $scope.getDataWithEntity = function () {
        $scope.searchByList = [
            {
                'name': 'Buyer',
                'value': 'BuyerName'
            }
        ];
        baseService.init($scope.getListUrl, null, null, null, 'BuyerName', 'BuyerName');
        baseService.setCurrentPage('inquirys');
        $scope.getData = function (pageno) {
            $rootScope.parameters.entityId = $scope.inquiryNew.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.inquirys = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    // #region DDL
    $scope.productionProcessGroupList = [];
    cboService.productionProcessGroupCbo(null, function (result) {
        $scope.productionProcessGroupList = result.Rows;
    });
    $scope.entityList = [];
    //cboService.getCboProductionEntityByCompany($window.companyGroupId, $window.companyId, function (result) {
    //    $scope.entityList = result;
    //});
    $scope.getEntityListCbo = function (id) {
        var url = '/OrderManagements/Inquiry/GetEntityCboWithProductionProcessGroup?productionProcessGroupId=' + id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    $scope.buyerList = [];
    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });

    $scope.onChangeBuyer = function () {
        $scope.buyerDepartmentList = [];
        $scope.buyerDivisionList = [];
        $scope.buyerBrandList = [];
        if (baseService.isUndefinedOrNull($scope.inquiryNew.BuyerId)) {
            $scope.inquiryNew.BuyerDepartmentId = null;
            $scope.inquiryNew.BuyerDivisionId = null;
            $scope.inquiryNew.BuyerBrandId = null;
            return;
        }
        cboService.getBuyerDepartmentCboByBuyer($scope.inquiryNew.BuyerId, function (result) {
            $scope.buyerDepartmentList = result;
        });
        cboService.getBuyerDivisionCboByBuyer($scope.inquiryNew.BuyerId, function (result) {
            $scope.buyerDivisionList = result;
        });
        cboService.getBuyerBrandCboByBuyer($scope.inquiryNew.BuyerId, function (result) {
            $scope.buyerBrandList = result;
        });
    };
    // #endregion
    //Buyer for modal
    $scope.buyerBrandList = [];
    $scope.buyerList = [];
    $scope.buyerPOPUP = function () {
        if (baseService.isUndefinedOrNull($scope.inquiryNew.EntityId)) {
            return ShowResult("Select entity first.", 'failure');
        }
        $scope.searchByBuyerList = [
            {
                'name': 'Buyer',
                'value': 'BuyerName'
            },
            {
                'name': 'Department',
                'value': 'DepartmentName'
            },
            {
                'name': 'Division',
                'value': 'DivisionName'
            }
        ];
        $scope.parameters.searchBy = 'BuyerName';
        baseService.init('Parties/BuyerMaster/GetList?entityId=' + $scope.inquiryNew.EntityId, null, null, null, 'BuyerName', 'BuyerName');
        $scope.getBuyerData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getBuyerData();
        angular.element(document.querySelector('#buyerSearchModal')).modal('show');
    };
    //Passing Data For IntermediateItemEntity List
    $scope.buyerCloseListPopUp = function (data) {
        $scope.inquiryNew.BuyerMasterId = data.Id;
        $scope.inquiryNew.BuyerId = data.BuyerId;
        $scope.inquiryNew.BuyerName = data.BuyerName;
        $scope.inquiryNew.DepartmentName = data.DepartmentName;
        $scope.inquiryNew.DivisionName = data.DivisionName;
        $scope.getBuyerActivity();
        angular.element(document.querySelector('#buyerSearchModal')).modal('hide');
    };
    $scope.buyerActivityList = [];
    $scope.getBuyerActivity = function () {
        cboService.getActivityWithBuyerMasterCbo($scope.inquiryNew.BuyerMasterId, function (result) {
            $scope.buyerActivityList = result;
        });
    }
    $scope.getBuyerBrand = function () {
        cboService.getBuyerBrandCboByBuyer($scope.inquiryNew.BuyerId, function (result) {
            $scope.buyerBrandList = result;
        });
    }
    //#end
    /*****ResposiblePerson*****************/
    $scope.excludeList = ['Image', 'Flag'];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: '',
        searchBy: '',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function getEmployeeInformationData() {
        $scope.sbEmployeeInformation = [];
        $scope.employeeinformationData = [];
        $scope.popUpTitle = '';
        var popUpUrl = '';
        $scope.popUpTitle = 'Employee Profile';
        popUpUrl = 'OrderManagements/Inquiry/GetQueryForResponsible?entityId=' + $scope.inquiryNew.EntityId + '&buyerMasterId=' + $scope.inquiryNew.BuyerMasterId + '&buyerActivityId=' + $scope.inquiryNew.BuyerActivityId;
        $scope.popUpParameters.sort = 'EmployeeCode';
        $scope.popUpParameters.searchBy = 'FirstName';
        baseService.setCurrentPage('dataList');
        $scope.loadEIData = function (pageno) {
            baseService.paginationBase(popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.employeeinformationData = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        $scope.loadEIData();
    }
    $scope.showEmployeeInformationModal = function () {
        if (baseService.isUndefinedOrNull($scope.inquiryNew.BuyerActivityId)) {
            return  ShowResult("Select Buyer Activity", 'failure');
        }
        getEmployeeInformationData();
        angular.element(document.querySelector('#employeepopup')).modal('show');
    };
    $scope.getEmployee = function (ob) {
        $scope.inquiryNew.ResponsiblePerson = ob.EmployeeName;
        $scope.inquiryNew.ResponsiblePersonId = ob.SystemId;
        angular.element(document.querySelector('#employeepopup')).modal('hide');
    };
    $scope.clearResponsiblePerson = function () {
        $scope.inquiryNew.ResponsiblePerson = null;
        $scope.inquiryNew.ResponsiblePersonId = null;
    };
    //-----------------
    //Commitment
    $scope.commitmentAddConfirmModal = function () {
        $scope.message_confirmation = 'Do you want to link with commitment number';
        angular.element(document.querySelector('#confirmgenericPopUpForCommitment')).modal('show');
    };

    $scope.confirmToShowCommitmentModal = function () {
        $scope.showCommitmentFormModal();
    };
    //
    $scope.showCommitmentFormModal = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            getCommitmentSavedList(id);
        }
        angular.element(document.querySelector('#commitmentFormModal')).modal('show');
    };
    function getCommitmentSavedList(id) {
        var url = '/OrderManagements/Inquiry/GetCommitmentInquiryList?inquiryId=' + id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.commitmentInquirySelectedList = response.data;
        });
    }
    $scope.closeCommitmentFormModal = function () {
        angular.element(document.querySelector('#commitmentFormModal')).modal('hide');
    };
    $scope.ShowCommitmentPopUp = function () {
        $scope.ShowIntermediateItemList();
        angular.element(document.querySelector('#commitmentSearchModal')).modal('show');
    };
    //CommitmentList for modal
    $scope.commitmentList = [];
    $scope.commitmentInquirySelectedList = [];
    $scope.ShowIntermediateItemList = function () {
        $scope.searchByCommitmentList = [
            {
                'name': 'Finished Goods',
                'value': 'FinishedGoods'
            },
            {
                'name': 'Process',
                'value': 'ProcessName'
            },
            {
                'name': 'SubProcess',
                'value': 'SubProcessName'
            },
            {
                'name': 'Buyer',
                'value': 'BuyerName'
            }
        ];
        if ($scope.inquiryNew.EntityId === null) {
            return ShowResult('Please at first select entity......', 'failure');
        }
        baseService.init('OrderManagements/Commitment/getlist', null, null, null, 'FinishedGoods', 'FinishedGoods');
        $scope.getCommitmentData = function (pageno) {
            $rootScope.parameters.entityId = $scope.inquiryNew.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.commitmentList = result.Rows;
                    angular.forEach($scope.commitmentInquirySelectedList, function (item) {
                        for (var i = 0; i < $scope.commitmentList.length; i++) {
                            if ($scope.commitmentList[i]['Id'] == item.CommitmentId) {
                                $scope.commitmentList.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getCommitmentData();
    };
    //Passing Data For IntermediateItemEntity List
    $scope.CommitmentSelectdCloseListPopUp = function () {
        angular.forEach($scope.commitmentList, function (item) {
            if (item.Flag) {
                $scope.commitmentInquirySelectedList.push(
                    {
                        Id: null,
                        CommitmentId: item.Id,
                        EntityId: $scope.inquiryNew.EntityId,
                        InquiryId: $scope.inquiryNew.Id,
                        FinishedGoods: item.FinishedGoods,
                        ProcessName: item.ProcessName,
                        SubProcessName: item.SubProcessName,
                        BuyerName: item.BuyerName,
                        LSD: item.LSD,
                        Flag: item.Flag
                    }
                );
            }
        });
        angular.element(document.querySelector('#commitmentSearchModal')).modal('hide');
    };
    //Save
    $scope.hasDuplicate = function (list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].CommitmentId == list[x].CommitmentId) {
                    throw list[i].UserName + " has duplicate row";
                }
            }
        }
    };
    $scope.CommitmentSave = function () {
        try {
            $scope.hasDuplicate($scope.commitmentInquirySelectedList);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/Inquiry/CommitmentInquiryCreate',
                    data: { 'commitmentInquiry': $scope.commitmentInquirySelectedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.closeCommitmentFormModal();
                        ClearFields();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from IntermediateItemEntityList
    $scope.valuePassInDelModal = function (index, IntermediateItemId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.IntermediateItemId = IntermediateItemId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteIntermediateItemEntityList = function () {
        $scope.intermediateItemEntityList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.IntermediateItemId = null;
        if ($scope.intermediateItemEntityList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //End Commitment
    //ProductInquiry
    $scope.brandList = [];
    cboService.getCboBrand(function (result) {
        $scope.brandList = result;
    });
    $scope.productList = [];
    $scope.productInquirySelectedList = [];
    $scope.productInquiryAddConfirmModal = function () {
        $scope.message_confirmation = 'Do you want to add product ?';
        angular.element(document.querySelector('#confirmgenericPopUpForProduct')).modal('show');
    };
    $scope.confirmToShowProductModal = function () {
        $scope.showProductFormModal();
    };
    $scope.notConfirmToShowProductModal = function () {
        $scope.commitmentAddConfirmModal();
    };
    //
    $scope.inquiryTempId = null;
    $scope.showProductFormModal = function (id) {
        if (!baseService.isUndefinedOrNull(id)) {
            $scope.inquiryTempId = id;
            getProductSavedList(id);
        }
        angular.element(document.querySelector('#productFormModal')).modal('show');
    };
    function getProductSavedList(id) {
        var url = '/OrderManagements/Inquiry/GetProductInquiryList?inquiryId=' + id;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            $scope.productInquirySelectedList = response.data;
        });
    }
    $scope.closeProductFormModal = function () {
        angular.element(document.querySelector('#productFormModal')).modal('hide');
    };
    $scope.ShowProductPopUp = function () {
        $scope.ShowProductItemList();
        angular.element(document.querySelector('#productSearchModal')).modal('show');
    };
    //ProductList for modal

    $scope.ShowProductItemList = function () {
        $scope.searchByProductList = [
            {
                'name': 'Finished Goods',
                'value': 'FinishedGoods'
            },
            {
                'name': 'Material Group',
                'value': 'MaterialGroupName'
            },
            {
                'name': 'Product Master',
                'value': 'ProductMasterName'
            }
        ];
        if ($scope.inquiryNew.EntityId == null) {
            return ShowResult('Please at first select entity......', 'failure');
        }
        baseService.init('OrderManagements/Inquiry/GetProductInquiryWithEntity', null, null, null, 'FinishedGoods', 'FinishedGoods');
        $scope.getProductData = function (pageno) {
            $rootScope.parameters.entityId = $scope.inquiryNew.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.productList = result.Rows;
                    angular.forEach($scope.productInquirySelectedList, function (item) {
                        for (var i = 0; i < $scope.productList.length; i++) {
                            if ($scope.productList[i]['Id'] == item.MaterialMasterId) {
                                $scope.productList.splice(i, 1);
                            }
                        }
                    });
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getProductData();
    };
    //Passing Data For IntermediateItemEntity List
    $scope.ProductSelectdCloseListPopUp = function () {
        angular.forEach($scope.productList, function (item) {
            if (item.Flag) {
                $scope.productInquirySelectedList.push(
                    {
                        Id: null,
                        MaterialMasterId: item.Id,
                        EntityId: $scope.inquiryNew.EntityId,
                        InquiryId: $scope.inquiryNew.Id != null ? $scope.inquiryNew.Id : $scope.inquiryTempId,
                        BrandId: null,
                        Quantity: 0,
                        TargetPrice: null,
                        LSD: null,
                        ShipmentDate: null,
                        IsDevelopment: false,
                        IsPreCosting: false,
                        FinishedGoods: item.FinishedGoods,
                        MaterialGroupName: item.MaterialGroupName,
                        ProductMasterName: item.ProductMasterName,
                        Flag: item.Flag
                    }
                );
            }
        });
        angular.element(document.querySelector('#productSearchModal')).modal('hide');
    };
    //Save
    $scope.hasProductDuplicate = function (list) {
        for (var i = 0; i < list.length; i++) {
            for (var x = i + 1; x < list.length; x++) {
                if (list[i].MaterialMasterId == list[x].MaterialMasterId) {
                    throw list[i].FinishedGoods + " has duplicate row";
                }
            }
        }
    };
    $scope.ProductSave = function () {
        try {
            $scope.hasProductDuplicate($scope.productInquirySelectedList);
            $scope.$broadcast('show-errors-check-validity');
            angular.forEach($scope.productInquirySelectedList, function (item) {
                if (item.Quantity === 0) {
                    throw "Quantity can not be zero";
                }
            })
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: 'OrderManagements/Inquiry/ProductInquiryCreate',
                    data: { 'productInquiry': $scope.productInquirySelectedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.closeProductFormModal();
                        $scope.productInquirySelectedList = [];
                        $scope.commitmentAddConfirmModal();
                    }
                });
                return true;
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //Deleting Rows from IntermediateItemEntityList
    $scope.valuePassInDelModal = function (index, IntermediateItemId, id) {
        $scope.id = id;
        $scope.index = index;
        $scope.IntermediateItemId = IntermediateItemId;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + id + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteIntermediateItemEntityList = function () {
        $scope.intermediateItemEntityList.splice($scope.index, 1);
        $scope.id = null;
        $scope.index = null;
        $scope.IntermediateItemId = null;
        if ($scope.intermediateItemEntityList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    };
    //End ProductInquiry
    //ProductInquiryDetail
    $scope.jobWorkTypeList = [];
    cboService.getEnumCbo("enum/GetProductionProcessGroupEnumCbo", function (result) {
        $scope.jobWorkTypeList = result;
    });
    $scope.getproductInquiryDetail = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/Inquiry/GetProductInquiryDetailList?productInquiryId=' + $scope.ProductInquiryId
        }).then(function successCallback(response) {
            $scope.productInquiryDetailSelectedList = response.data;
        });
    };
    $scope.productInquiryDetailPopUp = function (index, id) {
        $scope.ProductInquiryId = id;
        $scope.getproductInquiryDetail();
        angular.element(document.querySelector('#productInquiryDetailFormModal')).modal('show');
    }
    $scope.productInquiryDetailSelectedList = [];
    $scope.processSetDetailTblShow = false;
    $scope.productInquiryDataList = [];
    $scope.valueData = '';
    $scope.ShowProductProcessGroupPopUp = function () {
        $scope.productProcessGroupPopUp();
        angular.element(document.querySelector('#productProcessGroupModal')).modal('show');
    }

    $scope.productProcessGroupPopUp = function () {
        $scope.searchByProductProcessGroupList = [
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'User Name',
                'value': 'ProductionProcessGroupName'
            }, {
                'name': 'Standard Name',
                'value': 'StandardName'
            }
        ];
        $scope.productProcessGroupParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "ProductionProcessGroupName",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.productProcessGroupUrl = 'OrderManagements/Inquiry/GetProductProcessGroupWithNotId?processProductionGroupId=' + $scope.inquiryNew.ProductionProcessGroupId;
        $scope.getProductProcessGroupData = function (pageno) {
            baseService.paginationBase($scope.productProcessGroupUrl, pageno, $scope.productProcessGroupParameters)
                .then(function (result) {
                    $scope.productInquiryDataList = result.Rows;
                    $scope.productProcessGroupParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'productProcessGroupModal');
                }).finally(function () {
                });
        };
        $scope.getProductProcessGroupData();
    };

    function isProcessIdExistInGrid(list) {
        $scope.processIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] === false) {
                    $scope.processIds.push(list[i]['ProcessId']);
                }
            }
        }
        return JSON.stringify($scope.processIds);
    }

    $scope.selectPSDDoubleClick = function (data) {
        $scope.addProcessSetDetails(data);
        $scope.closePSDPopUp();
    };
    $scope.selectPSDSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectPSDByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'processSetDetailPopUp');
        }
        $scope.selectPSDDoubleClick($scope.valueData);
        $scope.closePSDPopUp();
    };
    $scope.closePSDPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#processSetDetailPopUp')).modal('hide');
    };
    $scope.addProductInquiryDetails = function () {
        angular.forEach($scope.productInquiryDataList, function (item) {
            if (item.Flag) {
                $scope.productInquiryDetailSelectedList.push(
                    {
                        Id: null,
                        ProductionProcessGroupId: item.Id,
                        Code: item.Code,
                        StandardName: item.StandardName,
                        ProductionProcessGroupName: item.ProductionProcessGroupName,
                        EntityId: $scope.inquiryNew.EntityId,
                        ProductInquiryId: $scope.ProductInquiryId,
                        JobWorkApplicable: false,
                        JobWorkType: null,
                        EntityOrVendorId: null,
                        EntityOrVendorName: null,
                        class: 'new',
                        setDisable: true,
                        Flag: item.Flag
                    }
                );
            }
        });
        angular.element(document.querySelector('#productProcessGroupModal')).modal('hide');
        //$scope.productInquiryDetailSelectedList.push({
        //    Id: $scope.pk(),
        //    ProcessSetId: $scope.processSetNew.Id,
        //    ProcessId: data.ProcessId,
        //    ProcessName: data.UserName,
        //    Sequence: $scope.processSetDetails.length + 1,
        //    IsBaseProcess: false,
        //    Days: 0,
        //    Symbol: '+',
        //    ProductionCycleTime: 1,
        //    JobWorkApplicable: false,
        //    JobWorkType: null,
        //    EntityOrVendorId: null,
        //    EntityOrVendorName: null,
        //    Archive: false,
        //    class: 'new',
        //    setDisable: true
        //});
        //if (!$scope.processSetDetailTblShow)
        //    $scope.processSetDetailTblShow = true;
    };
    function isJobWorkType(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].JobWorkApplicable && list[i].JobWorkType === null
                    && (list[i].EntityIdWithinCompany === null
                        || list[i].EntityIdWithinGroup === null
                        || list[i].VendorId === null)
                ) {
                    throw 'Please select job work type and entity/vendor.......!';
                }
                if (!baseService.isUndefinedOrNull(list[i].JobWorkType)) {
                    if (baseService.isUndefinedOrNull(list[i].EntityOrVendorName)) {
                        throw 'Please insert entity/vendor.......!';
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.clearEntityOrVendor = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionProcessGroupId === id) {
                list[i].EntityIdWithinCompany = null;
                list[i].EntityIdWithinGroup = null;
                list[i].VendorId = null;
                list[i].EntityOrVendorName = null;
                break;
            }
        }
    };
    $scope.clearJobType = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionProcessGroupId === id) {
                list[i].JobWorkType = null;
                break;
            }
        }
    };
    $scope.SetDisable = function (id) {
        for (var i = 0; i < $scope.productInquiryDetailSelectedList.length; i++) {
            if ($scope.productInquiryDetailSelectedList[i].ProductionProcessGroupId === id) {
                if ($scope.productInquiryDetailSelectedList[i].JobWorkApplicable) {
                    return $scope.productInquiryDetailSelectedList[i].setDisable = false;
                }
                else {
                    return $scope.productInquiryDetailSelectedList[i].setDisable = true;
                }
            }
        }
    };

    $scope.popUpTitle = '';
    $scope.valueData = '';
    $scope.popUp = function (id) {
        $scope.popUpList = [];
        $scope.popUpDataList = [];
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Name',
            searchBy: "Name",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        if (isJobWorkApplicable($scope.productInquiryDetailSelectedList, id))
            return ShowResult('Please select at first job work type..............!', 'failure');
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.productInquiryDetailSelectedList, id);
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.id = id;
        $scope.getPopUpData();
    };

    $scope.selectDoubleClick = function (data) {
        valueSetInGrid($scope.productInquiryDetailSelectedList, data, $scope.id);
        $scope.id = '';
        $scope.closePopUp();
    };
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };
    function typeCheckAndCreateUrl(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionProcessGroupId === id) {
                if (list[i].JobWorkType === 'Internal') {
                    $scope.popUpTitle = 'Entity within company';
                    return 'OrderManagements/Inquiry/GetEntityWithInternal?companyGroupId=' + $window.companyGroupId + '&productionProcessGroupId=' + id;
                }
                else if (list[i].JobWorkType === 'External') {
                    $scope.popUpTitle = 'Entity in group';
                    return 'Parties/vendorcompanydata/getpartyfromvendor?companyGroupId=' + $window.companyGroupId + '&companyId=' + $window.companyId;
                }
            }
        }
    }
    function isJobWorkApplicable(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionProcessGroupId === id) {
                if (baseService.isUndefinedOrNull(list[i].JobWorkType))
                    return true;
                else
                    return false;
            }
        }
    }
    function valueSetInGrid(list, data, id) {
        $scope.clearEntityOrVendor(list, id);
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductionProcessGroupId === id) {
                if (list[i].JobWorkType === 'Internal') {
                    list[i].InternalEntityId = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                else if (list[i].JobWorkType === 'External') {
                    list[i].VendorId = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                break;
            }
        }
    }
    $scope.ProductInquiryDetailSave = function () {
        try {
            // $scope.hasProductDuplicate($scope.productInquirySelectedList);
            $http({
                method: 'POST',
                url: 'OrderManagements/Inquiry/ProductInquiryDetailCreate',
                data: { 'productInquiryDetail': $scope.productInquiryDetailSelectedList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#productInquiryDetailFormModal')).modal('hide');
                }
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //ProductInquiryDetail
    $scope.Get = function (id, index) {
        $scope.index = index;
        angular.copy($scope.inquirys[$scope.index], $scope.inquiry);
        angular.copy($scope.inquiry, $scope.inquiryNew);
        //$scope.onChangeBuyer();
        $scope.getBuyerActivity();
        if ($scope.inquiryNew.ResponsiblePersonId != null) {
            $scope.inquiryNew.ResponsiblePersonId = $scope.inquiryNew.ResponsiblePersonId;
        }

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.inquiryNewForm.$valid) {
            angular.copy($scope.inquiryNew, $scope.inquiry);
            if ($scope.inquiry.Quantity === 0) {
                throw ShowResult("Quantity can not be zero.", 'failure');
            }
            if ($scope.inquiry.NoOfItems === 0) {
                throw ShowResult("NoOfItems can not be zero.", 'failure');
            }
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'inquiry': $scope.inquiry
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        angular.copy(response.data.inquiry, $scope.inquiryNew);
                        $scope.getDataWithEntity();
                        $scope.productInquiryAddConfirmModal();
                        baseService.paginationAdd();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: {
                        'inquiry': $scope.inquiry
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1)
                            angular.copy($scope.inquiry, $scope.inquirys[$scope.index]);
                        ClearFields();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.inquiryNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.inquiryNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.inquirys.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.Clear = function () {
        $scope.inquiry = {};
        $scope.inquiryNew = {};
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.inquiry = {};
        $scope.inquiryNew = {
            EntityId: $scope.inquiryNew.EntityId
            , BuyerId: null
            , BuyerDepartmentId: null
            , BuyerDivisionId: null
            , NoOfItems: 0
            , Quantity: 0
        };
        $scope.getDataWithEntity();
        $scope.productInquirySelectedList = [];
        $scope.commitmentInquirySelectedList = [];
    }
    // #endregion
}