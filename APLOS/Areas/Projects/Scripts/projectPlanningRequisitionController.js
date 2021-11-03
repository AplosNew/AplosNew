'use strict';
function ProjectPlanningRequisitionController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Project Planning Requisition ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.convertedUOMQuantity = 0
    $scope.projectPlanningRequisitions = [];
    $scope.projectPlanningMaterialSavedListDetail = [];
    $scope.testDropDown = null;
    $scope.path = 'Projects/projectPlanningRequisition/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchByProjectPlanningRequisitionList = [
        {
            'name': 'Vendor',
            'value': 'Vendor'
        },
        {
            'name': 'Project Planning',
            'value': 'Title'
        },
        {
            'name': 'Vendor ReferanceNo',
            'value': 'VendorReferanceNo'
        }
    ];
    $scope.projectPlanningRequisitionListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Vendor',
        searchBy: "Vendor",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getProjectPlanningRequisition = function () {
        $scope.projectPlanningRequisitionListParameters.search = null;
        $scope.GetProjectPlanningRequisitionListData = function (pageno) {
            baseService.paginationBase($scope.getListUrl, pageno, $scope.projectPlanningRequisitionListParameters)
                .then(function (data) {
                    $scope.projectPlanningRequisitions = data.Rows;
                    $scope.projectPlanningRequisitionListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningRequisitionPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningRequisitionListData();
    };

    $scope.GetProjectPlanningRequisitionInfo = function (data) {
        $scope.getProjectPlanningInfoOnChange(data.ProjectPlanningId);
        $scope.projectPlanningRequisitionNew = data;
        $scope.projectPlanningRequisitionNew.RequisitionDate = $filter('dateFiltering')(data.RequisitionDate);
        angular.element(document.querySelector('#projectPlanningRequisitionPopUp')).modal('hide');
    }
    $scope.projectPlanningRequisition = {
        Id: null,
        ProjectPlanningId: null,
        Description: null,
        RequisitionDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    //$scope.ProjectPlanningRequisitionDetail = {
    //    Id: null,
    //    ProjectPlanningRequisitionId: null,
    //    ProjectPlanningDetailId: null,
    //    Quantity: null,
    //    Rate: null,
    //    AddedBy: null,
    //    AddedDate: new Date(),
    //    AddedFromIP: null,
    //    UpdatedDate: null

    //}
    $('.datepicker').datepicker({
        forceParse: false,
        format: 'dd-M-yyyy', autoclose: true, reset: true, todayHighlight: true, setDate: new Date()
    });
    $scope.projectPlanningRequisitionNew = Object.assign({}, $scope.projectPlanningRequisition);
    $scope.multiSelectSettings = {
        scrollableHeight: 'auto',
        smartButtonMaxItems: 3,
        scrollable: true,
        showCheckAll: false,
        showUncheckAll: false,
        enableSearch: true,
        dynamicTitle: true,
    };
    $scope.selectedAaa = [];
    $scope.multi4events = {
        onItemSelect: function (events, item) {
            //$scope.Filter = new Object();
            //$scope.Filter.selectedText = { 'Machine Type': 'Manual' };
            $scope.cboCratetor($scope.materialMasterIds, 'MaterialMasterId');
            angular.forEach($scope.list, function (x) {
                angular.forEach(x.valueListName, function (y) {
                    if (y.Value === item.Value) {
                        if (checkNotMexist(item.Value) === false) {
                            $scope.selectedAaa.push(y.Text);
                            $scope.selectedText = y.Text;
                        }
                        //$scope.selectedText = createIdList($scope.selectedAaa);
                        //getArticle();
                        //return;
                    }
                })
            });
            // $scope.selectedText = $filter('searchFilter')($scope.itemSelected);
        }, onItemDeselect: function (item) {
            $scope.selectedText = "";
        }
    };
    $scope.onItemSelect = function (item) {
        console.log(item);
    }
    function checkNotMexist(value) {
        for (var i = 0; i < baseService.arrayLength($scope.selectedAaa); i++) {
            if ($scope.selectedAaa[i] === value) {
                return true;
                break;
            }
        }
        return false;
    }
    //$scope.materialAttributeValuIds = [];
    /***Cbo***************/
    //$scope.ProjectPlanningList = [];
    //cboService.getCboProjectPlanning(function (result) {
    //    $scope.ProjectPlanningList = result;
    //});
    function getUomList(materailMasterId) {
        $http({
            method: 'GET',
            url: 'Projects/projectplanningpurchaseorder/GetUomList?materailMasterId=' + $scope.materailMasterId,
        }).then(function successCallback(response) {
            $scope.alterNativeUomList = response.data;
        })
    };
    getUomList();
    //$scope.uOMList = [];
    //$http({
    //    method: 'GET',
    //    url: 'Setups/unitofmeasurement/getcbo/',
    //}).then(function successCallback(response) {
    //    $scope.uOMList = response.data;
    //});
    //--------------
    $scope.getProjectPlanningInfoOnChange = function (id) {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanning/GetProjectPlanningById?id=' + id,
        }).then(function successCallback(response) {
            $scope.projectPlanningInfo = response.data.Rows[0];
            $scope.projectPlanningInfo.ProjectPlanningTitle = response.data.Rows[0].Title;
            getmaterialMasterSavedList();
        })
        $scope.ProjectPlanningRequisitionDetailSelectedList = [];
    }
    //cboService.getCboProjectPlanningCategory(function (result) {
    //    $scope.ProjectPlanningCategoryList = result;
    //});
    //cboService.getCboProjectPlanningSubCategory(function (result) {
    //    $scope.ProjectPlanningSubCategoryList = result;
    //});
    //*****************ProjectPlanningSearch********************/
    $scope.getProjectPlanningSearchPopup = function () {
        $scope.searchByProjectPlanningList = [
            {
                'name': 'Code',
                'value': 'Code'
            },
            {
                'name': 'Description',
                'value': 'Description'
            },
            {
                'name': 'Title',
                'value': 'Title'
            },
            {
                'name': 'Status',
                'value': 'Status'
            }
        ];
        $scope.projectPlanningListParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "Code",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.GetProjectPlanningListData = function (pageno) {
            baseService.paginationBase('Projects/projectPlanning/getlist', pageno, $scope.projectPlanningListParameters)
                .then(function (data) {
                    $scope.projectPlannings = data.Rows;
                    for (var i = 0; i < $scope.projectPlannings.length; i++) {
                        if ($scope.projectPlannings[i].EmployeeId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].EmployeeName;
                        }
                        else if ($scope.projectPlannings[i].PositionId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].PositionName;
                        } else if ($scope.projectPlannings[i].ManpowerBudgetId != null) {
                            $scope.projectPlannings[i].ResponsiblePersonName = $scope.projectPlannings[i].ManpowerBudgetName;
                        }
                    }
                    $scope.projectPlanningListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#projectPlanningPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetProjectPlanningListData();
    }
    $scope.GetProjectPlanningInfo = function (data) {
        $scope.projectPlanningInfo = data;
        $scope.projectPlanningInfo.ProjectPlanningTitle = data.Title;
        $scope.projectPlanningRequisitionNew.ProjectPlanningId = data.Id;
        angular.element(document.querySelector('#projectPlanningPopUp')).modal('hide');
    }

    //-----------
    //*************MaterialMasterSearch************/

    //Asset//
    $scope.projectPlanningMaterialMasterSearchPopup = function () {
        $scope.projectPlanningDetailTempList = [];
        getMaterailMasterData();
        angular.element(document.querySelector('#MaterialMasterSearchModal')).modal('show');
    };
    $scope.searchbyMaterailMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMaster'
        },
    ]
    $scope.materialMasterListParameters = {
        limit: 5,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 5,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    function getMaterailMasterData() {
        $scope.materialMasterList = [];
        baseService.setCurrentPage('projectPlanningMaterialMasterList');
        $scope.loadMaterialMasterData = function (pageno) {
            baseService.paginationBase('Projects/projectplanning/ProjectplanninMaterialMasterSavedListForRequisition?materialType=Asset&projectPlanningId=' + $scope.projectPlanningRequisitionNew.ProjectPlanningId, pageno, $scope.materialMasterListParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        result.Rows[i].ProjectPlanningRequisitionDetailId = $scope.ProjectPlanningRequisitionDetailId;
                    }
                    $scope.projectPlanningMaterialMasterList = result.Rows;
                    $scope.materialMasterListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterData();
    };
    $scope.materialMasterModalCloseListPopUp = function () {
        ProjectPlaningMaterialListSave();
        angular.element(document.querySelector('#MaterialMasterSearchModal')).modal('hide');
    }
    $scope.ProjectPlaningRequisitionMaterialListForSave = [];

    //NonAsset//
    $scope.projectPlanningMaterialMasterNonAssetSearchPopup = function () {
        $scope.projectPlanningDetailTempList = [];
        getMaterailMasterNonAssetData();
        angular.element(document.querySelector('#MaterialMasterNonAssetSearchModal')).modal('show');
    };
    $scope.searchbyMaterailMasterNonAssetList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'BaseUom',
            'value': 'BaseUom'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMaster'
        },
    ]
    $scope.materialMasterNonAssetListParameters = {
        limit: 5,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 5,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    function getMaterailMasterNonAssetData() {
        $scope.materialMasterNonAssetList = [];
        baseService.setCurrentPage('projectPlanningMaterialMasterNonAssetList');
        $scope.loadMaterialMasterNonAssetData = function (pageno) {
            baseService.paginationBase('Projects/projectplanning/ProjectplanninMaterialMasterSavedListForRequisition?materialType=AllMaterialMaster&projectPlanningId=' + $scope.projectPlanningRequisitionNew.ProjectPlanningId, pageno, $scope.materialMasterNonAssetListParameters)
                .then(function (result) {
                    for (var i = 0; i < result.Rows.length; i++) {
                        result.Rows[i].ProjectPlanningRequisitionDetailId = $scope.ProjectPlanningRequisitionDetailId;
                    }
                    $scope.projectPlanningMaterialMasterNonAssetList = result.Rows;
                    $scope.materialMasterNonAssetListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMaterialMasterNonAssetData();
    };
    $scope.materialMasterNonAssetModalCloseListPopUp = function () {
        ProjectPlaningMaterialListSave();
        angular.element(document.querySelector('#MaterialMasterNonAssetSearchModal')).modal('hide');
    }
    $scope.ProjectPlaningRequisitionMaterialListForSave = [];

    //
    function ProjectPlaningMaterialListSave() {
        //angular.forEach($scope.articleList, function (item) {
        angular.forEach($scope.projectPlanningDetailTempList, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterExist($scope.ProjectPlaningRequisitionMaterialListForSave, item.Id) === false) {
                    $scope.ProjectPlaningRequisitionMaterialListForSave.push(
                        {
                            Id: null,
                            ProjectPlanningMaterialMasterId: item.Id,
                            MaterialMasterId: item.MaterialMasterId,
                            ProjectPlanningRequisitionId: $scope.projectPlanningRequisitionNew.Id,
                            Code: item.Code,
                            UserName: item.UserName,
                            PlanningCurrencyId: $scope.projectPlanningInfo.CurrencyId,
                            PlanningCurrencyName: $scope.projectPlanningInfo.CurrencyName,
                            BaseUom: item.BaseUom,
                            BaseUOMId: item.BaseUOMId,
                            PlanningUOMId: item.PlanningUOMId,
                            PlanningUOM: item.PlanningUOM,
                            FixedAssetName: item.FixedAssetName,
                            AssetType: item.AssetType,
                            alernativeUomLists: buildUomDropDown($scope.alterNativeUomList, item.MaterialMasterId),
                            AlternativeUomId: item.PlanningUOMId,
                            PlanningQuantity: item.Quantity,
                            RaisedQuantity: item.RaisedQuantity === null ? 0 : item.RaisedQuantity,
                            Quantity: null,
                            BaseUoMQuantity: null,
                            Rate: null,
                            Amount: null
                        });
                }
            }
        });
        if ($scope.ProjectPlaningRequisitionMaterialListForSave.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    }
    function checkMaterialMasterExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProjectPlanningMaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    //
    //*********UOM DropDown**********/
    function selectedDDL(list) {
        try {
            var uomId = null;
            for (var i = 0; i < list.length; i++) {
                if (list[i].IsPo) {
                    uomId = list[i].Value;
                    return uomId;
                }
            }
            return uomId;
        } catch (e) {
        }
    }
    var finalUomDropDownList = [];
    function buildUomDropDown(list, id) {
        finalUomDropDownList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                if (finalUomDropDownList.length > 0) {
                    if (checkExistUOM(list[i].UoMID) === false) {
                        finalUomDropDownList.push({
                            Text: list[i].UoM,
                            Value: list[i].UoMID,
                            Id: list[i].Id,
                            IsPo: setPo(list, list[i].Id, list[i].UoMID),
                        });
                    }
                } else {
                    finalUomDropDownList.push({
                        Text: list[i].UoM,
                        Value: list[i].UoMID,
                        Id: list[i].Id,
                        IsPo: setPo(list, list[i].Id, list[i].UoMID),
                    });
                }
            }
        }

        return finalUomDropDownList;
    }
    function checkExistUOM(uomId) {
        for (var i = 0; i < finalUomDropDownList.length; i++) {
            if (finalUomDropDownList[i].Value === uomId) {
                return true;
                break;
            }
        }
        return false;
    }
    function setPo(list, id, uomId) {
        try {
            var hasValue = false;
            for (var i = 0; i < list.length; i++) {
                if (list[i].Id === id && list[i].UoMID === uomId && list[i].IsPo) {
                    hasValue = true;
                }
            }
            return hasValue;
        } catch (e) {
        }
    }
    $scope.getTotalResult = function () {
        calculateTotalQuantity();
    }
    function calculateTotalQuantity() {
        $scope.totalQuantity = 0;
        for (var i = 0; i < $scope.ProjectPlaningRequisitionMaterialListForSave.length; i++) {
            $scope.totalQuantity += parseInt($scope.ProjectPlaningRequisitionMaterialListForSave[i].Quantity);
        }
    }
    //------------------
    /*****MaterialMasterAddPopUp***/
    $scope.materialMasterFormAddPopup = function () {
        $scope.ProjectPlaningRequisitionMaterialListForSave = [];
        angular.element(document.querySelector('#materialMasterFormModal')).modal('show');
    };
    $scope.materialMasterFormModalCloseListPopUp = function () {
        angular.element(document.querySelector('#materialMasterFormModal')).modal('hide');
    };

    //--------------
    /****Material Master Get Save***********/
    function getmaterialMasterSavedList() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningRequisition/GetProjectplanningRequisitionMaterialMasterSavedList?projectPlanningRequisitionId=' + $scope.projectPlanningRequisitionNew.Id,
        }).then(function successCallback(response) {
            $scope.projectPlanningMaterialSavedListDetail = response.data;
        })
    };
    /*****Material Master Save*******/
    function checkMMValidation() {
        if (!$scope.ProjectPlaningRequisitionMaterialListForSave.length > 0) {
            throw "No list found to save";
        }
        angular.forEach($scope.ProjectPlaningRequisitionMaterialListForSave, function (item) {
            if (parseInt(item.Quantity) <= 0 || item.Quantity === null) {
                throw item.UserName + " must be greater than 0";
            }
        });
    }
    $scope.ProjectPlanningRequisitionMaterialMasterSave = function () {
        angular.copy($scope.projectPlanningRequisitionNew, $scope.projectPlanningRequisition);
        try {
            checkMMValidation();
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Projects/projectPlanningRequisition/MaterialMasterCreate/',
                    data: {
                        'projectPlanningRequisition': $scope.projectPlanningRequisition
                        , 'projectPlanningRequisitionMaterial': $scope.ProjectPlaningRequisitionMaterialListForSave
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningRequisitionById($scope.projectPlanningRequisition.Id);
                        $scope.ProjectPlanningRequisitionMaterialSelectedList = [];
                        $scope.ProjectPlaningRequisitionMaterialListForSave = [];
                        $scope.materialMasterFormModalCloseListPopUp();
                        getmaterialMasterSavedList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.projectplanningRequisition,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.projectplanningRequisitions[$scope.index] = $scope.projectplanningRequisition;
                            $scope.projectplanningRequisitions = $filter('orderBy')($scope.projectplanningRequisitions, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure', 'materialMasterFormModal');
        }
    }
    function checkMMAValidation() {
        angular.forEach($scope.PPRequisitionMaterialListForSave, function (item) {
            if (parseInt(item.Quantity) <= 0 || item.Quantity === null) {
                throw item.StandardName + " must be greater than 0";
            }
        });
    }
    $scope.ProjectPlanningRequisitionMaterialMasterAticleSave = function () {
        try {
            checkMMAValidation();
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: 'Projects/ProjectPlanningRequisition/RequisitionArticleCreate',
                    data: { 'requisitionMaterialMasterId': $scope.PPRequisitionMaterialMasterId, 'requisitionArticleList': $scope.PPRequisitionMaterialListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningRequisitionById(response.data.ProjectPlanningRequisitionId);
                        $scope.PPRequisitionMaterialListForSave = [];
                        $scope.materialMasterArticleAddFormPopupClose();
                        getmaterialMasterSavedList();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'materialMasterArticleFormModal');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'materialMasterArticleFormModal');
        }
    }

    //**************************** Deleting child with master ************************* //
    $scope.valuePassInProjectPlanningRequisitionDelModal = function (index, Id) {
        $scope.ProjectPlanningRequisitionSelectedId = Id;
        $scope.ProjectPlanningRequisitionSelectedIdIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningRequisitionSelectedId + ' ]';
        angular.element(document.querySelector('#confirmgenericRequisitionSelecteDeldPopUp')).modal('show');
    };

    $scope.DeleteRequisitionSelectedItem = function () {
        $http({
            method: 'POST',
            url: 'Projects/ProjectPlanningRequisition/DeleteProjectPlanningRequisitionWithChild?id=' + $scope.ProjectPlanningRequisitionSelectedId,
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            } else {
                ShowResult(response.data.Message, 'success');
                $scope.projectPlanningRequisitions.splice($scope.ProjectPlanningRequisitionSelectedIdIndex, 1);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
        $scope.ProjectPlanningRequisitionSelectedId = null;
    };
    //********
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.projectPlanningRequisition = $scope.projectPlanningRequisitions[$scope.index];
        $scope.projectPlanningRequisitionNew = Object.assign({}, $scope.projectPlanningRequisition);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function getProjectPlanningRequisitionById(Id) {
        $http({
            method: 'GET',
            url: 'Projects/projectPlanningRequisition/GetProjectPlanningRequisitionById?id=' + Id,
        }).then(function successCallback(response) {
            if (response.data.Rows.length > 0) {
                $scope.getProjectPlanningInfoOnChange(response.data.Rows[0].ProjectPlanningId);
                $scope.projectPlanningRequisitionNew = response.data.Rows[0];
                $scope.projectPlanningRequisitionNew.RequisitionDate = $filter('dateFiltering')(response.data.Rows[0].RequisitionDate);
            }
            calculateTotalQuantity();
        })
    }
    //************Add New Article***********//
    // #region article
    $scope.searchFreeField = false;
    $scope.attributeList = [];
    $scope.newArticleList = [];
    $scope.articleHead = [];
    $scope.article = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , MaterialMasterArticleValues: []
    };
    $scope.articleNew = Object.assign({}, $scope.article);
    $scope.articleFormPopUp = function () {
        getAttribute();
        angular.element(document.querySelector('#articlePoUp')).modal('show');
    }
    //function getAttribute() {
    //    $scope.attributeList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.model.Id,
    //    }).then(function successCallback(response) {
    //        $scope.attributeList = response.data;
    //        if (baseService.arrayLength(response.data) == 0)
    //            return ShowResult('This material has no attribute', 'failure');
    //        for (var i = 0; i < $scope.attributeList.length; i++) {
    //            $scope.searchFreeField = $scope.attributeList[i].MaterialAttributeValueFreeText !== null ? true : false;
    //            var isFree = $scope.attributeList[i].IsFreeField;
    //            $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    //        }
    //    })
    //}
    $scope.CloseArticlePopUp = function () {
        articleClear();
        angular.element(document.querySelector('#articlePoUp')).modal('hide');
        CloseModalShowResult('articlePoUp');
    }
    $scope.AddArticle = function () {
        try {
            if (baseService.arrayLength($scope.attributeList) === 0)
                throw 'This material has no attribute';
            articleFieldValidation($scope.articleNew.Code, 'Code');
            articleFieldValidation($scope.articleNew.ShortName, 'ShortName');
            articleFieldValidation($scope.articleNew.StandardName, 'StandardName');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                var _invalid = $scope.IsMandatoryButNull($scope.attributeList[i].IsMandatory, $scope.attributeList[i].MaterialAttributeValueFreeText);
                if (_invalid)
                    throw $scope.attributeList[i].MaterialAttributeName + ' value is required!';
            }
            //uniqueCheckInArticleList($scope.articleList, $scope.articleNew);
            //for (var t = 0; t < $scope.articleList.length; t++) {
            //    if (!materialValueDuplecateCheck($scope.articleList[t].MaterialMasterArticleValues, $scope.attributeList))
            //        throw 'This combination already exist.!';
            //}
            $scope.articleNew.MaterialMasterId = $scope.articleMaterialMasterId;
            angular.forEach($scope.attributeList, function (element, i) {
                $scope.articleNew.MaterialMasterArticleValues.push({
                    Id: baseService.pk()
                    , MaterialMasterId: $scope.articleMaterialMasterId
                    , MaterialMasterAttributeId: element.MaterialMasterAttributeId
                    , MaterialMasterArticleId: $scope.articleNew.Id
                    , MaterialAttributeId: element.MaterialAttributeId
                    , MaterialAttributeName: element.MaterialAttributeName
                    , MaterialAttributeValueId: element.MaterialAttributeValueId
                    , MaterialMasterAttributeValueId: baseService.isUndefinedOrNull(element.MaterialMasterAttributeValueId) ? 0 : element.MaterialMasterAttributeValueId
                    , MaterialAttributeValueFreeText: element.MaterialAttributeValueFreeText
                });
            });
            $scope.article = Object.assign({}, $scope.articleNew);
            $scope.newArticleList.push($scope.article);
            CloseModalShowResult('articlePoUp');
            articleClear();
        } catch (e) {
            ShowResult(e, 'failure', 'articlePoUp')
        }
    }
    function uniqueCheckInArticleList(mainList, model) {
        for (var i = 0; i < mainList.length; i++) {
            if (mainList[i].Code == model.Code)
                throw 'Code is already exist in grid.!';
            else if (mainList[i].ShortName == model.ShortName)
                throw 'Short name is already exist in grid.!';
            else if (mainList[i].StandardName == model.StandardName)
                throw 'Standard name is already exist in grid.!';
        }
    }
    function materialValueDuplecateCheck(list, tempList) {
        var hasDifferent = false;
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialAttributeValueFreeText !== tempList[i].MaterialAttributeValueFreeText) {
                hasDifferent = true;
                break;
            }
        }
        return hasDifferent;
    }
    function articleClear() {
        $scope.articleNew = {
            Id: null
            , MaterialGroupMasterId: null
            , MaterialMasterId: null
            , Code: null
            , ShortName: null
            , StandardName: null
            , MaterialMasterArticleValues: []
        };
    }
    function articleFieldValidation(field, fieldName) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw fieldName + ' is required.!';
            }
        } catch (e) {
            throw e;
        }
    }
    function getarticleHed(list, newList, flag) {
        if (flag) {
            for (var i = 0; i < list.length; i++) {
                newList.push({ MaterialAttributeName: list[i].MaterialAttribute.UserName });
            }
        }
        else {
            for (var t = 0; t < list.length; t++) {
                newList.push({ MaterialAttributeName: list[t].MaterialAttributeName });
            }
        }
    }

    //****************deleting Article************/

    $scope.valuePassInPOArticleMasterModal = function (index, data) {
        $scope.articleIndex = index;
        $scope.articleId = data.Id;
        $scope.message_confirmation = 'Are you sure want to delete this data....';
        angular.element(document.querySelector('#confirmgenericPOArticleDetailSelectedItem')).modal('show');
    };
    $scope.confirmgenericPOArticleDetailSelectedItem = function () {
        var usedArticle = {}
        var url = 'Projects/ProjectPlanningPurchaseOrder/GetPPRequisitionArticleIsusedOnPurchaseOrderArticle?id=' + $scope.articleId;
        $http({
            method: 'GET',
            url: url
        }).then(function successCallback(response) {
            usedArticle = response.data;
            if (usedArticle != "") {
                return ShowResult("This article is used on po article", 'failure');
            } else {
                $scope.PPRequisitionMaterialListForSave.splice($scope.articleIndex, 1);
            }
        });
    }

    // #endregion article

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
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.valuePoUp = function (data, index) {
        $scope.materialAttributeValueUrl = 'Materials/MaterialMasterArticle/GetAttributeValueList';
        baseService.setCurrentPage('valueList');
        $scope.getValueData = function (pageno) {
            $scope.valueParameters.assignment = data.ValueAssignmentLevel;
            $scope.valueParameters.mmAttributeId = data.MaterialMasterAttributeId;
            $scope.valueParameters.attributeId = data.MaterialAttributeId;
            baseService.paginationBase($scope.materialAttributeValueUrl, pageno, $scope.valueParameters)
                .then(function (result) {
                    $scope.valueList = result.Rows;
                    $scope.valueParameters.total_count = result.Total;
                    $scope.valueindex = index;
                    $scope.searchFreeField = true;
                    angular.element(document.querySelector('#attributeValuePoUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getValueData();
    }
    //$scope.getAttrValue = function (data) {
    //    $scope.attributeList[$scope.valueindex].MaterialAttributeValueId = data.MaterialAttributeValueId;
    //    $scope.attributeList[$scope.valueindex].MaterialMasterAttributeValueId = data.MaterialMasterAttributeValueId;
    //    $scope.attributeList[$scope.valueindex].MaterialAttributeValueFreeText = data.UserName;
    //    $scope.attributeList[$scope.valueindex].FlagDisable = $scope.searchFreeField;
    //    $scope.valueindex = -1;
    //    angular.element(document.querySelector('#attributeValuePoUp')).modal('hide');
    //}
    //$scope.materialAttributeValueClear = function (index) {
    //    $scope.attributeList[index].MaterialAttributeValueId = null;
    //    $scope.attributeList[index].MaterialMasterAttributeValueId = null;
    //    $scope.attributeList[index].MaterialAttributeValueFreeText = null;
    //    $scope.searchFreeField = false;
    //    var isFree = $scope.attributeList[index].IsFreeField;
    //    $scope.attributeList[index].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
    //}
    //$scope.closeValuePopUp = function () {
    //    angular.element(document.querySelector('#attributeValuePoUp')).modal('hide');
    //    CloseModalShowResult('attributeValuePoUp');
    //}

    // #endregion value

    //$scope.idNullByFreeText = function (id, index) {
    //    if ($scope.attributeList[index].MaterialAttributeId == id) {
    //        $scope.attributeList[index].MaterialAttributeValueId = null;
    //        $scope.attributeList[index].MaterialMasterAttributeValueId = null;
    //    }
    //}
    $scope.IsFreeFieldOrNot = function (IsFreeField) {
        if (IsFreeField) {
            if ($scope.searchFreeField)
                return true;//disabled true
            else
                return false;//disabled false
        }
        else
            return true;//disabled true
    }
    $scope.IsMandatoryButNull = function (isMandatory, materialAttributeValueFreeText) {
        if (isMandatory) {
            if (baseService.isUndefinedOrNull(materialAttributeValueFreeText)) return true;
            else return false;
        }
        else return false;
    }

    //$scope.ArticleSave = function () {
    //    $scope.AddArticle();
    //    $http({
    //        method: 'POST',
    //        url: 'Materials/materialmasterarticle/create',
    //        data: $scope.newArticleList,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.model = {};
    //            $scope.newArticleList = {};
    //        }
    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    }
    //}

    //***********End********/
    //****************MaterialMasterArticle************/
    $scope.materialMasterArticleAddFormPopup = function (data) {
        $scope.articleMaterialMasterId = data.MaterialMasterId;
        $scope.PPRequisitionMaterialMasterId = data.Id;
        $scope.PPRequisitionUoM = data.RequsitionUoM;
        $scope.setSingleRow = [];
        $scope.PPRequisitionMaterialListForSave = [];
        getCheckAttribute();
    }
    function getCheckAttribute() {
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.articleMaterialMasterId,
        }).then(function successCallback(response) {
            if (response.data.length === 0) {
                return ShowResult("This material has no attribute", 'failure');
            } else {
                getMaterialMasterArticleSavedData();
                angular.element(document.querySelector('#materialMasterArticleFormModal')).modal('show');
            }
        })
    }
    $scope.materialMasterArticleAddFormPopupClose = function () {
        angular.element(document.querySelector('#materialMasterArticleFormModal')).modal('hide');
    }
    function getMaterialMasterArticleSavedData() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningRequisition/GetProjectplanningRequisitionMaterialMasterArticleSavedList?requisitionMaterialMasterId=' + $scope.PPRequisitionMaterialMasterId + '&projectPlanningRequisitionId=' + $scope.projectPlanningRequisitionNew.Id,
        }).then(function successCallback(response) {
            $scope.PPRequisitionMaterialListForSave = [];
            getArticleSaveValue(response.data);
        })
    }
    //---------
    //***************MaterialMaster Article Search Popup*********/
    $scope.materialMasterArticleSearchPopup = function () {
        getMaterialAttributeValue();
        getArticle();
        angular.element(document.querySelector('#materialMasterArticleSearchPopup')).modal('show');
    }

    $scope.articleHead = [];
    $scope.articleList = [];
    $scope.materattributeValueDdlList = [];
    function getMaterialAttributeValue() {
        $http({
            method: 'GET',
            url: 'Projects/ProjectPlanningRequisition/GetMaterialMasterAttributeValueList?materialMasterId=' + $scope.articleMaterialMasterId,
        }).then(function successCallback(response) {
            $scope.materattributeValueDdlList = response.data;
        })
    }
    $scope.articleDropDownCbo = [];
    function makeArticleDropDownList(list, headingName, materialAttributeId, list2) {
        var valueListName = headingName.replace(/\s/g, '');
        var modelName = headingName.replace(/\s/g, '');
        $scope[modelName] = [];
        valueListName = valueListName + 'List'
        $scope[valueListName] = [];
        angular.forEach(list, function (item, i) {
            if (item.MaterialAttributeId === materialAttributeId)
                $scope[valueListName].push({
                    Value: baseService.pk(),
                    Text: item.Text
                });
        });
        list2.push({
            modelName: $scope[modelName],
            valueListName: $scope[valueListName],
            labelName: headingName
        });
        //createDDlModel();
    }

    function createdddlforattribute() {
        $scope.list = [];
        angular.forEach($scope.articleHead, function (item) {
            makeArticleDropDownList($scope.materattributeValueDdlList, item.MaterialAttributeName, item.MaterialAttributeId, $scope.list);
        });
    }
    //function createDDlModel() {
    //    angular.forEach($scope.list, function (item, i) {
    //        $scope[item.modelName] = [];
    //    });
    //}
    function getAttribute() {
        $scope.attributeList = [];
        $http({
            method: 'GET',
            url: 'Materials/materialmaster/getmaterialmasterattributelist?materialMasterId=' + $scope.articleMaterialMasterId,
        }).then(function successCallback(response) {
            $scope.attributeList = response.data;
            if (baseService.arrayLength(response.data) == 0)
                return ShowResult('This material has no attribute', 'failure');
            for (var i = 0; i < $scope.attributeList.length; i++) {
                $scope.searchFreeField = $scope.attributeList[i].MaterialAttributeValueFreeText !== null ? true : false;
                var isFree = $scope.attributeList[i].IsFreeField;
                $scope.attributeList[i].FlagDisable = $scope.IsFreeFieldOrNot(isFree);
            }
        })
    }
    function getArticleSaveValue(list) {
        $scope.articleHead = [];
        $scope.articleList = [];
        if (list.length > 0) {
            $http({
                method: 'GET',
                url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.articleMaterialMasterId,
                contentType: "application/json; charset=utf-8",
            }).then(function successCallback(response) {
                if (baseService.arrayLength(response.data)) {
                    $scope.valueData = response.data
                    var valueData = response.data
                    $http({
                        method: 'GET',
                        url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.articleMaterialMasterId,
                        contentType: "application/json; charset=utf-8",
                    }).then(function successCallback(response) {
                        $scope.articleHead = response.data;
                        if (baseService.arrayLength($scope.articleHead)) {
                            for (var i = 0; i < list.length; i++) {
                                list[i].MaterialMasterArticleValues = [];
                                for (var a = 0; a < $scope.articleHead.length; a++) {
                                    list[i].MaterialMasterArticleValues.push({
                                        Id: null
                                        , MaterialMasterId: null
                                        , MaterialMasterAttributeId: null
                                        , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                        , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                        , MaterialMasterArticleId: null
                                        , MaterialAttributeValueId: null
                                        , MaterialMasterAttributeValueId: null
                                        , MaterialAttributeValueFreeText: null
                                    });
                                }
                            }
                        }
                        for (var t = 0; t < baseService.arrayLength(list); t++) {
                            var articleRow = Object.assign({}, list[t]);
                            checkValueSubMaterialSavedId(valueData, articleRow);
                            $scope.articleList.push(articleRow);
                        }
                        getSavedSingleRow();
                    })
                }
            })
        }
    }
    function checkValueSubMaterialSavedId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.PPReuisitionArticleId === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }
    function getSavedSingleRow() {
        for (var t = 0; t < baseService.arrayLength($scope.articleList); t++) {
            var at = $scope.articleList[t];
            var ob = {};
            ob.Id = at.Id;
            ob.Code = at.Code;
            ob.ShortName = at.ShortName;
            ob.StandardName = at.StandardName;
            ob.MaterialMasterId = at.MaterialMasterId;
            ob.PPRequisitionMaterialMasterId = at.PPRequisitionMaterialMasterId;
            ob.PPReuisitionArticleId = at.PPReuisitionArticleId;
            ob.ProjectPlanningRequisitionId = at.ProjectPlanningRequisitionId;
            ob.Quantity = at.Quantity;
            ob.RequisitionUoM = at.RequisitionUoM;
            angular.forEach($scope.articleHead, function (item) {
                ob[item.MaterialAttributeName] = getV(at.MaterialMasterArticleValues, at.PPReuisitionArticleId, item.MaterialAttributeId);
            });
            $scope.PPRequisitionMaterialListForSave.push(ob);
        }
    }
    function getArticle() {
        $scope.articleHead = [];
        $scope.articleList = [];
        $http({
            method: 'GET',
            url: 'Materials/materialmasterarticle/getlist?materialMasterId=' + $scope.articleMaterialMasterId,
            contentType: "application/json; charset=utf-8",
        }).then(function successCallback(response) {
            $scope.articles = response.data;
            var articles = response.data;
            if (articles.length > 0) {
                $http({
                    method: 'GET',
                    url: 'Materials/materialmasterarticle/GetArticleValueList?materialMasterId=' + $scope.articleMaterialMasterId,
                    contentType: "application/json; charset=utf-8",
                }).then(function successCallback(response) {
                    if (baseService.arrayLength(response.data)) {
                        $scope.valueData = response.data
                        var valueData = response.data
                        $http({
                            method: 'GET',
                            url: 'Materials/materialmasterarticle/getarticlvaluehead?materialMasterId=' + $scope.articleMaterialMasterId,
                            contentType: "application/json; charset=utf-8",
                        }).then(function successCallback(response) {
                            $scope.articleHead = response.data;
                            if (baseService.arrayLength($scope.articleHead)) {
                                for (var i = 0; i < articles.length; i++) {
                                    articles[i].MaterialMasterArticleValues = [];
                                    for (var a = 0; a < $scope.articleHead.length; a++) {
                                        articles[i].MaterialMasterArticleValues.push({
                                            Id: null
                                            , MaterialMasterId: null
                                            , MaterialMasterAttributeId: null
                                            , MaterialAttributeId: $scope.articleHead[a].MaterialAttributeId
                                            , MaterialAttributeName: $scope.articleHead[a].MaterialAttributeName
                                            , MaterialMasterArticleId: null
                                            , MaterialAttributeValueId: null
                                            , MaterialMasterAttributeValueId: null
                                            , MaterialAttributeValueFreeText: null
                                        });
                                    }
                                }
                            }
                            for (var t = 0; t < baseService.arrayLength(articles); t++) {
                                var articleRow = Object.assign({}, articles[t]);
                                checkValueSubMaterialId(valueData, articleRow);
                                $scope.articleList.push(articleRow);
                            }
                            createdddlforattribute();
                            getSingleRow();
                        })
                    }
                })
            }
        });
    }
    function getSingleRow() {
        $scope.setSingleRow = [];
        for (var t = 0; t < baseService.arrayLength($scope.articleList); t++) {
            var at = $scope.articleList[t];
            var ob = {};
            ob.Id = at.Id;
            ob.Code = at.Code;
            ob.ShortName = at.ShortName;
            ob.StandardName = at.StandardName;
            angular.forEach($scope.articleHead, function (item) {
                ob[item.MaterialAttributeName] = getV(at.MaterialMasterArticleValues, at.Id, item.MaterialAttributeId);
            });
            $scope.setSingleRow.push(ob);
        }
    }
    function getV(list, articleId, MaterialAttributeId) {
        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            if (item.MaterialMasterArticleId === articleId && item.MaterialAttributeId === MaterialAttributeId) {
                return item.MaterialAttributeValueFreeText;
                break;
            }
        }
        return null;
    }
    function checkValueSubMaterialId(valueData, articleRow) {
        for (var v = 0; v < baseService.arrayLength(articleRow.MaterialMasterArticleValues); v++) {
            var valueRow = articleRow.MaterialMasterArticleValues[v];
            for (var tt = 0; tt < baseService.arrayLength(valueData); tt++) {
                if (articleRow.Id === valueData[tt].MaterialMasterArticleId
                    && valueRow.MaterialAttributeId === valueData[tt].MaterialAttributeId) {
                    var newValue = valueData[tt];
                    valueRow.Id = newValue.Id;
                    valueRow.MaterialMasterId = newValue.MaterialMasterId;
                    valueRow.MaterialMasterAttributeId = newValue.MaterialMasterAttributeId;
                    valueRow.MaterialAttributeId = newValue.MaterialAttributeId;
                    valueRow.MaterialAttributeName = newValue.MaterialAttributeName;
                    valueRow.MaterialMasterArticleId = newValue.MaterialMasterArticleId;
                    valueRow.MaterialAttributeValueId = newValue.MaterialAttributeValueId;
                    valueRow.MaterialMasterAttributeValueId = newValue.MaterialMasterAttributeValueId;
                    valueRow.MaterialAttributeValueFreeText = newValue.MaterialAttributeValueFreeText;
                    break;
                }
            }
        }
    }
    //function checkSeelectc(articleRow) {
    //    for (var x = 0; x < articleRow.MaterialMasterArticleValues.length; x++) {
    //        var valueRow = articleRow.MaterialMasterArticleValues[x];
    //        for (var i = 0; i < $scope.selectedAaa.length; i++) {
    //            if (valueRow.MaterialAttributeValueFreeText === $scope.selectedAaa[i]) {
    //                return true;
    //                break;
    //            }
    //        }
    //        return false;
    //    }
    //}
    $scope.materialMasterArticleSearchModalCloseListPopUp = function () {
        PPMaterialArticleListSave();
        angular.element(document.querySelector('#materialMasterArticleSearchPopup')).modal('hide');
    }
    $scope.PPRequisitionMaterialListForSave = [];
    function PPMaterialArticleListSave() {
        angular.forEach($scope.setSingleRow, function (item) {
            if (item.Flag) {
                if (checkMaterialMasterArticleExist($scope.PPRequisitionMaterialListForSave, item.Id) === false) {
                    item.PPReuisitionArticleId = item.Id;
                    item.Id = null;
                    item.PPRequisitionMaterialMasterId = $scope.PPRequisitionMaterialMasterId;
                    item.MaterialMasterId = $scope.articleMaterialMasterId;
                    item.ProjectPlanningRequisitionId = $scope.projectPlanningRequisitionNew.Id;
                    item.RequisitionUoM = $scope.PPRequisitionUoM;
                    item.Quantity = null;
                    $scope.PPRequisitionMaterialListForSave.push(item);
                }
            }
        })
        console.log('$scope.PPRequisitionMaterialListForSave', $scope.PPRequisitionMaterialListForSave)
    }
    function checkMaterialMasterArticleExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProjectPlanningMaterialMasterId === id) {
                return true;
            }
        }
        return false;
    }
    //---------
    $scope.Save = function () {
        angular.copy($scope.projectPlanningRequisitionNew, $scope.projectPlanningRequisition);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.projectPlanningRequisitionForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'projectPlanningRequisition': $scope.projectPlanningRequisition },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getProjectPlanningRequisitionById(response.data.ProjectPlanningRequisitionId);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.projectPlanningRequisition = {};
        $scope.projectPlanningRequisitionNew = { RequisitionDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy') };
        $scope.projectPlanningInfo = {};
        $scope.projectPlanningRequisitionNew.Id = null
        $scope.ProjectPlaningProjectPlanningListForSave = [];
        $scope.projectPlanningMaterialSavedListDetail = [];
        $scope.PPRequisitionMaterialListForSave = [];
        $scope.projectPlanningRequisitionNew.Active = true;
    }
    //******************* newly added ********************************//
    $scope.projectPlanningDetailTempList = [];
    $scope.selectPODetailChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistPODetailList($scope.projectPlanningDetailTempList, data.Id) === false) {
                    $scope.projectPlanningDetailTempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.projectPlanningDetailTempList.length; i++) {
                    if ($scope.projectPlanningDetailTempList[i].Id === data.Id) {
                        $scope.projectPlanningDetailTempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistPODetailList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    $scope.valuePassInMaterialFormDelModal = function (index, Id) {
        $scope.ProjectPlanningPoDetailSelectedId = Id;
        $scope.ProjectPlanningPoDetailSelectedIdIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.ProjectPlanningPoDetailSelectedId + ' ]';
        angular.element(document.querySelector('#confirmgenericMaterialDetailSelectedPopUp')).modal('show');
    };

    $scope.DeleteMaterialDetailSelectedItem = function () {
        for (var i = 0; i < $scope.ProjectPlaningRequisitionMaterialListForSave.length; i++) {
            if (baseService.isUndefinedOrNull($scope.ProjectPlanningPoDetailSelectedId)) {
                $scope.ProjectPlaningRequisitionMaterialListForSave.splice($scope.ProjectPlanningPoDetailSelectedIdIndex, 1);
            }
        }
        $scope.ProjectPlanningPoDetailSelectedId = null;
        $scope.ProjectPlanningPoDetailSelectedIdIndex = null;
    };

    $scope.editProjectPlanningDetail = function (index, data) {
        $scope.projectPlanningEditIndex = index;
        $scope.ProjectPlanningRequisitionSavedEditTempList = Object.assign({}, data);
        angular.element(document.querySelector('#ProjectPlanningDetailEditPopUp')).modal('show');
    }

    $scope.projectPlanningDetailEditSave = function () {
        $scope.ProjectPlanningRequisitionSavedEditTempList.ReverseQuantity = $scope.ProjectPlanningRequisitionSavedEditTempList.Quantity;
        $scope.projectPlanningPOMaterialSavedList[$scope.projectPlanningEditIndex] = Object.assign({}, $scope.ProjectPlanningRequisitionSavedEditTempList);
        //$scope.projectPlaningPORequisitionMaterialListForSave[$scope.projectPlanningEditIndex] = Object.assign({}, $scope.ProjectPlanningDetailSavedEditTempList);
        $scope.PPPurchaseOrderDetailEdit();
    }

    //***************************** delete only child **************************//
    $scope.valuePassInPORecMasterModal = function (index, Id) {
        $scope.selectedChildId = Id;
        $scope.bIndex = index;
        if (baseService.isUndefinedOrNull($scope.id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + selectedChildId + ' ]';
        angular.element(document.querySelector('#confirmgenericPORecDetailSelectedItem')).modal('show');
    };

    $scope.DeletePORecDetailSelectedItem = function () {
        for (var i = 0; i < $scope.projectPlanningMaterialSavedListDetail.length; i++) {
            if ($scope.projectPlanningMaterialSavedListDetail[i].Id == $scope.selectedChildId) {
                $http({
                    method: 'POST',
                    url: 'projects/ProjectPlanningRequisition/DeleteProjectPlanningRequisition?id=' + $scope.selectedChildId,
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    } else {
                        ShowResult(response.data.Message, 'success');
                        $scope.projectPlanningMaterialSavedListDetail.splice($scope.bIndex, 1);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
            }
        }
        $scope.selectedChildId = null;
        //$scope.bIndex = null;
    };

    $scope.ProjectPlanningRequisitionMaterialMasterEdit = function () {
        angular.copy($scope.projectPlanningRequisitionNew, $scope.projectPlanningRequisition);
        try {
            angular.forEach($scope.ProjectPlanningRequisitionMaterialEditSaveList, function (item) {
                if (parseInt(item.Quantity) <= 0 || item.Quantity === null || item.Quantity === undefined)
                    throw item.UserName + " Quantity must be greater than 0"
            });
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.projectPlanningPOmaterialMasterForm.$valid) {
                // checkValidation();
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: 'Projects/projectPlanningRequisition/MaterialMasterCreate/',
                        data: {
                            'projectPlanningRequisition': $scope.projectPlanningRequisition
                            , 'projectPlanningRequisitionMaterial': $scope.ProjectPlanningRequisitionMaterialEditSaveList
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure', 'ProjectPlanningRequisitionEditPopUp1');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            getProjectPlanningRequisitionById($scope.projectPlanningRequisition.Id);
                            $scope.ProjectPlanningRequisitionMaterialSelectedList = [];
                            $scope.ProjectPlaningRequisitionMaterialListForSave = [];
                            getmaterialMasterSavedList();
                            angular.element(document.querySelector('#ProjectPlanningRequisitionEditPopUp1')).modal('hide');
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'ProjectPlanningRequisitionEditPopUp1');
                    }
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'ProjectPlanningRequisitionEditPopUp1');
        }
    }

    $scope.editProjectPlanningRequisition = function (index, data) {
        $scope.projectPlanningEditIndex = index;
        $scope.ProjectPlanningRequisitionSavedEditTempList = Object.assign({}, data);
        $scope.ProjectPlanningRequisitionSavedEditTempList.alernativeUomLists = buildUomDropDown($scope.alterNativeUomList, data.MaterialMasterId);
        $scope.ProjectPlanningRequisitionSavedEditTempList.AlternativeUomId = selectedDDL(buildUomDropDown($scope.alterNativeUomList, data.MaterialMasterId));
        angular.element(document.querySelector('#ProjectPlanningRequisitionEditPopUp1')).modal('show');
    }
    $scope.projectPlanningRequisitionEditSave = function () {
        $scope.ProjectPlanningRequisitionMaterialEditSaveList = [];
        $scope.ProjectPlanningRequisitionMaterialEditSaveList.push($scope.ProjectPlanningRequisitionSavedEditTempList);
        $scope.ProjectPlanningRequisitionMaterialMasterEdit();
    }
    ////////////////  ********************   /////////////////////////////////

    $scope.convertMaterialUOMQuantity = function (fromUOMId, toUOMId, Quantity) {
        $http({
            method: 'GET',
            url: 'Setups/UOMConversion/GetUOMValueConvert?fromUOMId=' + fromUOMId + '&toUOMId=' + toUOMId + '&quantity=' + Quantity,
        }).then(function successCallback(response) {
            $scope.convertedUOMQuantity = response.data[0].ReverseQuantity
        });
    }
    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion

    //angular.element(document.querySelector('#framework')).multiselect({
    //    nonSelectedText: 'Select Framework',
    //    enableFiltering: true,
    //    enableCaseInsensitiveFiltering: true,
    //    buttonWidth: 'auto'
    //});
    $scope.getSetV = function (event, data) {
        $scope.sss = data;
    }
}
ProjectPlanningRequisitionController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];