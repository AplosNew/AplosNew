'use strict';
JobWorkIssueReturnController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobWorkIssueReturnController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.ToDoFilePath = virtualPath.JobWorkValueAddedContract;
    $scope.ToDownloadFilePath = virtualPath.JobWorkTransformationContract;
    $rootScope.title = 'Job Work Issue/ Return';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.IndividualReportList = [];
    $scope.IssueTypeList = [];
    $scope.JobWorkLocationList = [];
    $scope.TransformationTypeList = [];
    $scope.EntityList = [];
    $scope.MaterialLocationList = [];
    $scope.path = 'JobWork/JobWorkIssueReturn/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

    //////// Drop Down

    $http({
        method: 'GET',
        url: 'JobWork/JobWorkIssueReturn/gejobworklocation/',
    }).then(function successCallback(response) {
        $scope.JobWorkLocationList = response.data;
        });

    var d = new Date();
    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        Type: null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.IssueModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ByWhomId: null,
        IssueReturn: null,
        JobWorkLocationId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
        IsConfirmed: false,
    };
    $scope.Issue = Object.assign({}, $scope.IssueModelTemp);

    $scope.getData = function () {
        if ($scope.ModelNew.Type == null) {
            var IssueType = "ValueAdded";
            $scope.ModelNew.Type = IssueType;
        }
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search, Type: $scope.ModelNew.Type },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            $scope.ShowHomeList = true;
            $scope.ShowReport = false;
            //        ClearFields();

        });
    }
    $scope.getData();

    $scope.ShowHomeList = true;
    $scope.ShowReport = false;
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        if ($scope.ModelNew.TabType == "Transformation") {
    
            $scope.Transformation = Object.assign({}, args.data);
            var PId = $scope.Transformation.Id;
            var TabType = $scope.Transformation.TabType;
            $scope.IssueTransformation.JWContractId = $scope.Transformation.Id;
            $scope.IssueTransformation.ContractType = 'Transformation';
            $scope.TabTypeNew = $scope.Transformation.TabType;
            $http({
                method: 'POST',
                url: $scope.path + "GetDataById",
                data: { Id: PId, TabType: TabType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.TransformationTypeList = response.data;
                $scope.IssueTransformation.JWContractId = response.data[0].Id;
                if ($scope.TransformationTypeList.length > 0) {
                    $scope.GetTransformationChildData();
                    //$scope.ShowHomeList = false;
                    //$scope.ShowReport = true;
                    //   $scope.GetIndividualReportData();
                    
                    $scope.SelectedTConEntity();
                    $scope.SelectedTConMaterialStorage();
                    $scope.getdataInventoryIssue();
                }

                });

            $scope.setTab(2);
        }
        else {
            
            $scope.ModelNew = Object.assign({}, args.data);
            var PId = $scope.ModelNew.Id;
            var TabType = $scope.ModelNew.TabType;
            $scope.IssueTransformation.ContractType = 'Value Added';
            $scope.TabTypeNew = $scope.Transformation.TabType;
            $http({
                method: 'POST',
                url: $scope.path + "GetDataById",
                data: { Id: PId, TabType: TabType},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.IssueTypeList = response.data;

                if ($scope.IssueTypeList.length > 0) {
                    $scope.GetValueAddedChildData();
                }

                });

            $scope.setTab(1);
        }
        $scope.ModelNew.Type = $scope.TabTypeNew;
        //if (!$rootScope.isCollapsed) {
        //    $rootScope.toggle();
        //}
    };

    //$scope.GetIndividualReportData = function () {
    //    $scope.IndividualReportList = [];
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'GetIndividualReportData?Id=' + $scope.Transformation.Id,
    //    }).then(function successCallback(response) {
    //        $scope.IndividualReportList = response.data;
    //    });
    //}

    $scope.GridInventoryIssuedata = [];
    $scope.getdataInventoryIssue = function () {
        $scope.GridInventoryIssuedata = [];
        $http({
            method: "GET",
            url: $scope.path + 'GetDataByInventoryIssue?Id=' + $scope.Transformation.Id,
        }).then(function successCallback(response) {
            $scope.GridInventoryIssuedata = response.data;
            if ($scope.GridInventoryIssuedata.length == 0) {
                $scope.ShowHomeList = true;
                $scope.ShowReport = false;
                $scope.setTab(2);
                if (!$rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
            }
            else {
                $scope.ShowHomeList = false;
                $scope.ShowReport = true;
                $scope.setTab(2);
            }

        });

    };

    $scope.GetValueAddedChildData = function () {
        $scope.IssueChildList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetValueAddedChildData?PKId=' + $scope.ModelNew.Id,
        }).then(function successCallback(response) {
            $scope.IssueChildList = response.data;
        });
    }

    $scope.GetTransformationChildData = function () {
        $scope.IssueTransformationChildList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetTransformationChildData?PKId=' + $scope.Transformation.Id,
        }).then(function successCallback(response) {
            $scope.IssueTransformationChildList = response.data;
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.IssueGeneralForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.Issue },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Issue = response.data.Data;

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Clear = function () {
        ClearFields();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.Issue = Object.assign({}, $scope.IssueModelTemp);
    }

    //   // #region field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.Issue.Id },
            url: $scope.path + 'LoadAllEmpDetails'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.Issue.ByWhomId = null;
        $scope.Issue.ResponsiblePerson = null;
        $scope.Issue.EmployeeCode = null;
        $scope.Issue.EmployeeStatus = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.Issue.EmployeeCode = data.Code;
        $scope.Issue.ByWhomId = data.Id;
        $scope.Issue.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
 //   // # end region

    //  ISSUE CHILD DATA

    $scope.IssueChildList = [];

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

   

    $scope.IssueChildModelTemp = {
        Id: null,
        JobWorkIssueReturnMasterId: null,
        ContractLineItemId: null,
        OrderChildId: null,
        Quantity: null,
        Remarks: null,
        Active: null,
       
    };
    $scope.IssueChild = Object.assign({}, $scope.IssueChildModelTemp);

    $scope.ValidateQuantity = function (RowData) {
        try {
            
            for (var i = 0; i < $scope.IssueChildList.length > 0; i++) {
                if ($scope.IssueChildList[i].OrderSpecific == "Yes") {
                    if ($scope.IssueChildList[i].Id === RowData.Id && $scope.IssueChildList[i].OWRId === RowData.OWRId) {
                        var IssueQty = parseFloat(RowData.BalToIssue);
                        var BalQty = parseFloat($scope.IssueChildList[i].OWRQuantity) - parseFloat($scope.IssueChildList[i].IssueQuantity)
                        if (IssueQty > BalQty) {
                            $scope.IssueChildList[i].BalToIssue = BalQty;
                            throw 'Issue Quantity cannot be greater than Balance to Issue';
                        }
                   }
                }
                if ($scope.IssueChildList[i].OrderSpecific == "NO") {
                    if ($scope.IssueChildList[i].Id === RowData.Id) {
                        var IssueQty = parseFloat(RowData.BalToIssue);
                        var BalQty = parseFloat($scope.IssueChildList[i].VCCQuantity) - parseFloat($scope.IssueChildList[i].IssueQuantity)
                        if (IssueQty > BalQty) {
                            $scope.IssueChildList[i].BalToIssue = BalQty;
                            throw 'Issue Quantity cannot be greater than Balance to Issue';
                        }
                    }
                }
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    //Save Function 
    $scope.SaveIssueChildTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var checkedData = [];
        for (var i = 0; i < $scope.IssueChildList.length; i++) {
            if ($scope.IssueChildList[i].isSelected == true)
                checkedData.push($scope.IssueChildList[i]);
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please Enter at least one Quantity';
            }
            $http({
                method: 'POST',
                data: { IssueChildTabData: checkedData, MasterId: $scope.Issue.Id },
                url: $scope.path + 'SaveIssueChild'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.IssueChild = response.data.Data;
                    $scope.GetValueAddedChildData($scope.ModelNew.Id);
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }

        //     }
    }
    $scope.ClearIssueChildTab = function () {
        ClearFieldsIssueChild();
        $scope.IssueChildList = [];
        $scope.IssueTypeList = [];
        $scope.getData();

    }

    function ClearFieldsIssueChild() {
        $scope.Issue = Object.assign({}, $scope.IssueModelTemp);
    }

    // REPORTS OF VALUE ADDED ISSUE/ REPORT

    $scope.DownloadReport = function (data) {
        try {
            $scope.PrintTabId = $scope.ModelNew.Id;
            $scope.IssueId = $scope.Issue.Id;
            var TabType = $scope.ModelNew.TabType;
            if (TabType == "Value Added") {
                var reportFormat = "Excel";
                window.open('JobWork/JobWorkIssueReturn/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
                $scope.getData();
            }

        } catch (e) {

        }
    };

    //#endregion end Reports

    //  TRNASFORMATION ISSUE

    $scope.JobWorkLocList = [];
    $scope.EntityList = [];


    $scope.SelectedTConMaterialStorage = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkIssueReturn/gejobworklocation?TId=' + $scope.Transformation.Id,
        }).then(function successCallback(response) {
            $scope.JobWorkLocList = response.data;
            if ($scope.JobWorkLocList.length > 0) {
                $scope.IssueTransformation.MaterialStorageId = $scope.JobWorkLocList[0].Value;
            }
        });
    }

    $scope.SelectedTConEntity = function () {
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkIssueReturn/getentitylist/',
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
            if ($scope.TransformationTypeList.length > 0) {
                for (var q = 0; q < $scope.EntityList.length; q++) {
                    if ($scope.EntityList[q].Value == $scope.TransformationTypeList[0].EntityId) {
                        $scope.IssueTransformation.EntityId = $scope.EntityList[q].Value;
                    }
                }
            }
        });
    }

    $scope.IssueTransformationModelTemp = {
        Id: null,
        IssueDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        EmployeeId: null,
        Types: 'InventoryJWIssue',
        MaterialStorageId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
        IsConfirmed: false,
        EntityId: null,
        IssueType: 'Revenue',
        JWContractId: null,
        ContractType:null

    };
    $scope.IssueTransformation = Object.assign({}, $scope.IssueTransformationModelTemp);

    $scope.ValidateIssueDate = function () {
        try {

            if (new Date($scope.IssueTransformation.Date) > new Date()) {
                $scope.IssueTransformation.Date = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                throw 'Issue Date should not be greater than Current date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

   // #region field

    $scope.EmployeeResPersonList = [];
    $scope.EmpPopUp = function () {
        angular.element(document.querySelector("#EmpPopUpResPerson")).modal("show");
        $scope.getEmpData();

    }
    $scope.getEmpData = function () {
        $scope.EmployeeResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.IssueTransformation.Id },
            url: $scope.path + 'LoadAllResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.EmployeeResPersonList = response.data;
        });
    }

    $scope.EmpClear = function () {
        $scope.IssueTransformation.EmployeeId = null;
        $scope.IssueTransformation.EmpName = null;
        $scope.IssueTransformation.EmpCode = null;
        $scope.IssueTransformation.EmpStatus = null;

    };
    $scope.closePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmployeeData = function (obj) {

        var data = obj.data;
        $scope.IssueTransformation.EmpCode = data.Code;
        $scope.IssueTransformation.EmployeeId = data.Id;
        $scope.IssueTransformation.EmpName = data.EmployeeName;
        angular.element(document.querySelector('#EmpPopUpResPerson')).modal('hide');
    };
    // # end region

    $scope.SaveIssueTransformation = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.IssueTransformationForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveIssueTransformation',
                data: { 'data': $scope.IssueTransformation, 'ContractId': $scope.Transformation.Id, 'ContractType': $scope.ModelNew.TabType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.IssueTransformation = response.data.Data;

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.ClearIssueTransformation = function () {
        ClearFieldsIssueTransformation();
    };

    function ClearFieldsIssueTransformation() {
        $scope.Action = 'Save';
        $scope.IssueTransformation = Object.assign({}, $scope.IssueTransformationModelTemp);
    }


    //   TRANSFORMATION ISSUE CHILD

    $scope.IssueTransformationChildList = [];
    $scope.MaterialInputList = [];
    $scope.detailList=[];

    $scope.SelectMaterialPlanning = function () {
        //$scope.product = Object.assign({}, $scope.productNew);
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueDate)) {
            ShowResult("Select the issue date");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.EntityId)) {
            ShowResult("Select the Entity");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.MaterialStorageId)) {
            ShowResult("Select the Material Storage");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueType)) {
            ShowResult("Select the type");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.EmpName)) {
            ShowResult("Select the wby whom");
            return false;

        }
        $scope.detailModel = {
            Id: null
            , InventoryReveiveId: null
            //, MaterialStorageId: $scope.productNew.MaterialStorageId
            , InventoryMaterialId: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , MaterialTypeName: null
            , OurStyleName: null
            , Description: null
            , MaterialGroupMasterName: null
            , ProductMasterName: null
            , IsOurStyleRequired: false
            , IsProductMstRequired: false
            , FirstCharacteristicsId: null
            , FirstCharacteristicsValueId: null
            , SecondCharacteristicsId: null
            , SecondCharacteristicsValueId: null
            , ThirdCharacteristicsId: null
            , ThirdCharacteristicsValueId: null
            , TransactionQty: null
            , TransactionUoMId: null
            , TransactionUoM: null
            , BaseQty: null
            , BaseUOMId: null
            , BaseUoM: null
            , BaseUoMFactor: null
            , TransactionRate: null
            , TotalQty: 0
            , AvgRate: null
            //, InventoryIssueId: $scope.productNew.Id
            , AvgAmount: null
            , PolicyRate: null
            , PolicyAmount: null
            , Policy: null
            , ActivityName: null
            , BudgetMasterId: null
            , ActivityId: null
            , IssueId: null
            , CostCenterId: null
        };
        var SelectedData = [];
        for (var i = 0; i < $scope.IssueTransformationChildList.length; i++) {
            if ($scope.IssueTransformationChildList[i].isSelected == true)
                SelectedData.push($scope.IssueTransformationChildList[i]);
        }

        $http({
            method: 'POST',
            data: { SelectedMaterialPlanningData: SelectedData },
            url: $scope.path + 'GetMaterialInputData'
        }).then(function successCallback(response) {
            $scope.MaterialInputList = response.data;
            $scope.detailList = response.data;
            if ($scope.MaterialInputList.length > 0 && $scope.detailList.length > 0) {
                $scope.CostCenterLoadNew();
            }

         //   $scope.detailList = response.data;
        });

    }
    $scope.GetRate = [];
    $scope.GetLotNoRate = function (RowData) {
        $scope.GetRate = [];
        $scope.LotNum = RowData.LotNumber;
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkIssueReturn/GetLotNoRate?LotNumber=' + $scope.LotNum,
        }).then(function successCallback(response) {
            $scope.GetRate = response.data;
            for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
                if ($scope.MaterialInputList[i].Id === RowData.Id) {
                    $scope.MaterialInputList[i].Rate = response.data[0].MaterialTranRate;

                }
            }
            });
    }

    $scope.GetMaterialValue = function (RowData) {
        try {
            for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
                if ($scope.MaterialInputList[i].Id === RowData.Id) {
                    if (parseFloat(RowData.Quantity) <= parseFloat($scope.MaterialInputList[i].BalanceToIssue)) {
                        var MaterialRate = parseFloat($scope.MaterialInputList[i].Rate);
                        var MaterialQty = parseFloat($scope.MaterialInputList[i].Quantity);
                        var MaterialValue = parseFloat(MaterialRate * MaterialQty);
                        var Num = MaterialValue.toFixed(2);
                        $scope.MaterialInputList[i].Value = Num;
                    }
                    else {
                        RowData.Quantity = null;
                        RowData.Value = null;
                        throw 'To Issue Quantity cannot be greater than Balance to Issue';
                    }
                }
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    // #region field

    $scope.MaterialMstList = [];
    $scope.MaterialMstPopUp = function (data) {
        angular.element(document.querySelector("#MaterialPopUp")).modal("show");
        $scope.getMaterialMstDetailsData(data);
    }

    $scope.getMaterialMstDetailsData = function (data) {
        $scope.MaterialMstList = [];

        for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
            if ($scope.MaterialInputList[i].Id === data.Id) {
                $scope.MatMstId = $scope.MaterialInputList[i].InputMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.MaterialMstList = response.data;
        });
    }

    $scope.MaterialMstClear = function (data) {
        for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
            if ($scope.MaterialInputList[i].Id === data.Id) {
                $scope.MaterialInputList[i].InputMaterialId = null;
                $scope.MaterialInputList[i].InputMaterialCode = null;
                $scope.MaterialInputList[i].InputMaterial = null;
            }
        }
    };

    $scope.closeMaterialMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialMstData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.MaterialInputList[b].InputMaterialId = data.Id;
        $scope.MaterialInputList[b].InputMaterialCode = data.Code;
        $scope.MaterialInputList[b].InputMaterial = data.MaterialName;

        $scope.MaterialInputList[b].MaterialMasterArticleId = null;
        $scope.MaterialInputList[b].ArticleCode = null;
        $scope.MaterialInputList[b].ArticleName = null;
        
        angular.element(document.querySelector('#MaterialPopUp')).modal('hide');
    };
    // # end region


    // GET ARTICLE
    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.MaterialArticleMstList = [];
    $scope.MaterialMstArticlePopUp = function (RowData, index) {
        $scope.indexforDetail = index;
        angular.element(document.querySelector("#MaterialArticlePopUp")).modal("show");
        $scope.getMaterialMstArticleData(RowData);

    }
    $scope.getMaterialMstArticleData = function (RowData) {
        $scope.MaterialArticleMstList = [];
      
            for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
                if ($scope.MaterialInputList[i].Id === RowData.Id) {
                    $scope.MatMstId = $scope.MaterialInputList[i].InputMaterialId;
                    $scope.SelectedMaterialInputId = $scope.MaterialInputList[i].Id;
                    $scope.a = i;
                }
            }
       
        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.MatMstId },
            url: $scope.path + 'LoadAllMaterialMstArticle'
        }).then(function successCallback(response) {
            $scope.MaterialArticleMstList = response.data;
        });
    }

    $scope.MaterialMstArticleClear = function (data) {
        for (var i = 0; i < $scope.MaterialInputList.length > 0; i++) {
            if ($scope.MaterialInputList[i].Id === data.Id) {

                $scope.MaterialInputList[i].MaterialMasterArticleId = null;
                $scope.MaterialInputList[i].ArticleCode = null;
                $scope.MaterialInputList[i].ArticleName = null;
            }
        }
    };

    $scope.closeMaterialArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialArticleData = function (obj) {
        $scope.detailModel = {
            Id: null
            , InventoryReveiveId: null
            , MaterialStorageId: $scope.IssueTransformation.MaterialStorageId
            , InventoryMaterialId: null
            , MaterialMasterId: null
            , MaterialMasterName: null
            , ArticleId: null
            , ArticleName: null
            , MaterialTypeName: null
            , OurStyleName: null
            , Description: null
            , MaterialGroupMasterName: null
            , ProductMasterName: null
            , IsOurStyleRequired: false
            , IsProductMstRequired: false
            , FirstCharacteristicsId: null
            , FirstCharacteristicsValueId: null
            , SecondCharacteristicsId: null
            , SecondCharacteristicsValueId: null
            , ThirdCharacteristicsId: null
            , ThirdCharacteristicsValueId: null
            , TransactionQty: null
            , TransactionUoMId: null
            , TransactionUoM: null
            , BaseQty: null
            , BaseUOMId: null
            , BaseUoM: null
            , BaseUoMFactor: null
            , TransactionRate: null
            , TotalQty: 0
            , AvgRate: null
            //, InventoryIssueId: $scope.productNew.Id
            , AvgAmount: null
            , PolicyRate: null
            , PolicyAmount: null
            , Policy: null
            , ActivityName: null
            , BudgetMasterId: null
            , ActivityId: null
            , IssueId: null
            , CostCenterId: null
        };
        var b = $scope.a;
        var data = obj.data;
        $scope.MaterialInputList[b].MaterialMasterArticleId = data.ArticleId;
        $scope.MaterialInputList[b].ArticleCode = data.ArticleCode;
        $scope.MaterialInputList[b].ArticleName = data.StandardName;
        $scope.SelectedArticleId = data.ArticleId;

        $scope.detailModel.MaterialMasterId = data.MaterialMasterId;
        $scope.detailModel.ArticleId = data.ArticleId;

        $scope.detailList[$scope.indexforDetail].MaterialMasterId = data.MaterialMasterId;
        $scope.detailList[$scope.indexforDetail].ArticleId = data.ArticleId;
        //$scope.GetByDefaultRate($scope.a);
        //$scope.GetLotNumberList($scope.a);
        $scope.GetIssuedDetailList($scope.a);
        getMaterialStock(b);
        angular.element(document.querySelector('#MaterialArticlePopUp')).modal('hide');
    };

    $scope.ByDefRate = [];
    $scope.GetByDefaultRate = function (c) {
        $scope.MaterialInputList[c].Rate = null;
        $http({
            method: 'GET',
            url: $scope.path + 'GetByDefaultRate?ArticleId=' + $scope.SelectedArticleId,
        }).then(function successCallback(response) {
            $scope.ByDefRate = response.data;
            if ($scope.ByDefRate.length > 0) {
                $scope.MaterialInputList[c].Rate = $scope.ByDefRate[0].Rate;
            }
        });
    }

    //  GET LOT NUMBER

    $scope.LotNumList = [];
    $scope.GetLotNumberList = function (x) {
        $scope.MaterialInputList[x].LotNumberList = null;
        $http({
            method: 'GET',
            url: $scope.path + 'GetLotNumberList?ArticleId=' + $scope.SelectedArticleId + '&MaterialId=' + $scope.MatMstId,
        }).then(function successCallback(response) {
            $scope.LotNumList = response.data;
            if ($scope.LotNumList.length > 0) {
                $scope.MaterialInputList[x].LotNumberList = response.data;
            }
        });
    }

    //  GET Planned, Issued, Balance Quantity

    $scope.IssuedDetailList = [];
    $scope.GetIssuedDetailList = function (x) {
        $scope.detailList[x].PlannedQty = null;
        $scope.detailList[x].IssuedQty = null;
        $scope.detailList[x].BalanceQty = null;
        $http({
            method: 'GET',
            url: $scope.path + 'GetIssuedDetailList?ArticleId=' + $scope.SelectedArticleId + '&MaterialId=' + $scope.MatMstId + '&MaterialInputId=' + $scope.SelectedMaterialInputId + '&ContractId=' + $scope.Transformation.Id,
        }).then(function successCallback(response) {
            $scope.IssuedDetailList = response.data;
            if ($scope.IssuedDetailList.length > 0) {

           //     $scope.detailList[$scope.indexforDetail].MaterialMasterId = data.MaterialMasterId;
                $scope.detailList[x].PlannedQty = $scope.IssuedDetailList[0].RequiredQuantity;
                $scope.detailList[x].IssuedQty = $scope.IssuedDetailList[0].TIRCTotalQty;
                $scope.detailList[x].BalanceQty = $scope.IssuedDetailList[0].BalanceToIssue;
            }
        });
    }

    // # end region

    $scope.TransformationChildModelTemp = {
        Id: null,
        TransformationIssueReturnMasterId: null,
        MaterialInputId: null,
        InputMaterialId: null,
        Quantity: null,
        Remarks: null,
        MaterialMasterArticleId: null,
        Value: null,
        LotNumber: null,

    };
    $scope.TransformationChild = Object.assign({}, $scope.TransformationChildModelTemp);

    //Save Function 
    //$scope.SaveTransformationChild = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    var SelectedQtyData = [];
    //    for (var i = 0; i < $scope.MaterialInputList.length; i++) {
    //        if ($scope.MaterialInputList[i].isSelected == true)
    //            SelectedQtyData.push($scope.MaterialInputList[i]);
    //    }
    //    try {
    //        if (SelectedQtyData.length == 0) {
    //            throw 'Please Enter at least one Quantity';
    //        }
    //        $http({
    //            method: 'POST',
    //            data: { SelectedQuantityData: SelectedQtyData, MasterId: $scope.IssueTransformation.Id },
    //            url: $scope.path + 'SaveTransformationChild'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult(response.data.Message, "failure");
    //            }
    //            else {
    //                ShowResult(response.data.Message, "success");
    //                $scope.TransformationChild = response.data.Data;
    //                $scope.SelectMaterialPlanning();
    //                $scope.GetIndividualReportData();
    //            }
    //        });

    //    }
    //    catch (e) {
    //        ShowResult(e, "failure");
    //    }
    //}


    $scope.ClearIssueTransformationChildTab = function () {
        ClearFieldsIssueTransformation();
        $scope.TransformationTypeList = [];
        $scope.IssueTransformationChildList = [];
        $scope.MaterialInputList = [];
        $scope.getData();

    }

    // DOWNLOAD REPORT

    $scope.DownloadIssueTransformationReport = function (data) {
        try {
            $scope.PrintTabId = $scope.Transformation.Id;
            $scope.IssueId = $scope.IssueTransformation.Id;
            var reportFormat = "Excel";
            window.open('JobWork/JobWorkIssueReturn/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
     //       $scope.getData();

        } catch (e) {

        }
    };

  

//

 $scope.CostCenterLoadNew = function () {
        debugger 
        $http({
            method: "GET",
            url: 'JobWork/JobWorkIssueReturn/GetCostCenterLoadNewFun?EntityId=' + $scope.IssueTransformation.EntityId
        }).then(function successCallback(response) {
            $scope.costCenterList = response.data;
        });
    }

    $scope.CostCenterLoadNew();

    //#region Expense activity select
    $scope.setSelected = function (data) {
        //debugger;
        $scope.addRow(data);
        $scope.closeCOAICodeListPopUp();
        $scope.setSelectedforGL(data);
    };

    $scope.addRow = function (data) {
        $scope.detailModel.GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.detailModel.BudgetMasterId = data.BudgetMasterId;
        $scope.detailModel.ActivityId = data.ActivityId;
        $scope.detailModel.BudgetName = data.BudgetName;
        $scope.getActivity(data);
    };
    $scope.activityList = [];
    $scope.getActivity = function (data) {
        cboService.getBudgetMasterActivityCbo(data.BudgetMasterId, function (result) {
            $scope.detailModel.ActivityId = null;
            $scope.activityList = [];
            $scope.activityList = result;
            $scope.detailModel.ActivityId = data.ActivityId;

        });
    };
    $scope.searchglByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "RefNo",
            "value": "RefNo"
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.popUp = function (index) {
        //debugger;
        $scope.customerInvoiceGLList = [];
        //baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetAllGLBudgetActivityPostingAutomaticOnly", pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
        $scope.issueSlipDetailIndex = index;
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function (x) {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };


    $scope.setSelectedforGL = function (data) {
        //debugger;
        $scope.MaterialInputList[$scope.issueSlipDetailIndex].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.MaterialInputList[$scope.issueSlipDetailIndex].BudgetMasterId = data.BudgetMasterId;
        $scope.MaterialInputList[$scope.issueSlipDetailIndex].ExpenseActivityId = data.ActivityId;
        $scope.MaterialInputList[$scope.issueSlipDetailIndex].ActivityName = data.GLGeneralInfoCode + '-' + data.ActivityName;
        $scope.MaterialInputList[$scope.issueSlipDetailIndex].BudgetName = data.BudgetName;
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };


    //#endregion



  $scope.materialStockList = [];
    $scope.specificStockList = [];
    //debugger;
    $scope.getSpecificMaterialStockForSlipIssue = function (data, index) {

        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
                ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
                return false;
            }
        }
        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].TransactionQty > $scope.detailList[i].RequestedQty) {
                ShowResult("Issue qty can not gaterthen Requested Qty");
                return false;
            }
        }

        $scope.index = index;
        data.MaterialStorageId = $scope.IssueTransformation.MaterialStorageId;
        $http({
            method: 'POST'
            , url: 'Products/InventoryIssue/GetSpecificMaterialStock/'
            , data: { entity: data, issueDate: $scope.IssueTransformation.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;

            for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
                var row = $scope.specificStockList[i];
                for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
                    var newRow = $scope.materialStockList[t];
                    if (newRow.InventoryReceiveDetailId === row.InventoryReceiveDetailId) {
                        newRow.Flag = true;
                        newRow.RequisitionQty = row.RequisitionQty;
                        break;
                    }
                }
            }

            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    //$scope.getRequisitionList = function (issueDetailId) {
    //    $scope.materialStockList = [];
    //    $scope.specificStockList = [];
    //    $http({
    //        method: 'POST'
    //        , url:'Products/InventoryIssue/GetRequisitionList/'
    //        , data: { issueDetailId: issueDetailId }
    //        , dataType: 'JSON'
    //    }).then(function (response) {
    //        $scope.materialStockList = response.data;
    //        angular.element(document.querySelector('#stockPopUp')).modal('show');
    //    }), function (response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};
    
    function qtyValidation(list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Flag) {
                if (parseFloat(list[i].RequisitionQty) > parseFloat(list[i].StockQty)) throw 'Requisition Qty can\'t greater than stock qty.';
            }
        }
    }
    function validationWithTotal(list) {
        var totalQty = 0;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            list[i].RequisitionQty = baseService.isUndefinedOrNull(list[i].RequisitionQty) === true ? 0 : parseFloat(list[i].RequisitionQty);
            if (list[i].Flag) {
                if (parseFloat(list[i].RequisitionQty) === 0)
                    throw 'Please input requisition qty';
                else {
                    if (list[i].TransactionUoMId !== list[i].BaseUOMId) totalQty += parseFloat(list[i].RequisitionQty) * parseFloat(list[i].BaseUoMFactor);
                    else totalQty += parseFloat(list[i].RequisitionQty).toFixed(2);
                }
            }
        }
        var qty = parseFloat($scope.MaterialInputList[$scope.index].TransactionQty) * parseFloat($scope.MaterialInputList[$scope.index].BaseUoMFactor);
        if (totalQty > qty && qty !== totalQty) throw 'Issue qty can\'t over ' + qty + ' .';
        if (totalQty < qty && qty !== totalQty) throw 'Issue qty can\'t less ' + qty + ' .';

    }

    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.delData = ob;
            $scope.message_confirmation = "Are you sure want to permanent delete [" + ob.MaterialMasterName + "] ";
            angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
            $scope.popUpIndex = index;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.delData.Id)) {
            $http({
                method: 'POST'
                , url: $scope.deleteUrl + '?issueDetailId=' + $scope.delData.Id
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else
                    ShowResult(response.data.Message, 'success');
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        for (var i = 0; i < baseService.arrayLength($scope.specificStockList); i++) {
            if ($scope.specificStockList[i].InventoryMaterialId === $scope.delData.InventoryMaterialId)
                $scope.specificStockList.splice(i, 1);
        }
        $scope.MaterialInputList.splice($scope.popUpIndex, 1);
        $scope.delData = null;
        $scope.popUpIndex = -1;
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('hide');
    };

    //function getIssueDetailList() {
    //    $http.get($scope.path + 'GetIssueDetailByIssueId?issueId=' + $scope.productNew.Id)
    //        .then(function (response) {
    //            $scope.detailList = response.data;
    //            $scope.detailModel.IssueId = $scope.detailList[0].InventoryIssueId;
    //        });
    //}

    //$scope.SaveSlipIssue = function () {
    //    var UIStatus = $("#SlipAssetIssueUI").val();
    //    if (UIStatus === 'Asset') {
    //        if ($scope.materialStockList.length === 0) {
    //            ShowResult('Please select Specific GRN');
    //            return false;
    //        }
    //    }
    //    //debugger;
    //    if ($scope.MaterialInputList.length === 0) {
    //        ShowResult('Please select Atlest one material');
    //        return false;
    //    }
    //    //debugger;
    //    for (var i = 0; i < $scope.MaterialInputList.length; i++) {
    //        if ($scope.MaterialInputList[i].TransactionQty > $scope.MaterialInputList[i].PostingQty) {
    //            ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
    //            return false;
    //        }


    //    }
    //    for (var i = 0; i < $scope.MaterialInputList.length; i++) {
    //        if ($scope.MaterialInputList[i].TransactionQty > $scope.MaterialInputList[i].RequestedQty) {
    //            ShowResult("Issue qty can not gaterthen Requested Qty");
    //            return false;
    //        }
    //    }

    //    $scope.productNew.IssueRequestMasterId = $scope.issueId;
    //    if ($scope.Action === "Save") {
    //        $http({
    //            method: 'POST'
    //            , url: $scope.saveUrl
    //            , data: {
    //                entities: $scope.MaterialInputList
    //                , specificStockList: $scope.specificStockList
    //                , inventoryIssue: $scope.productNew
    //                , IssueTypeStatus: UIStatus

    //            }
    //            , dataType: 'JSON'
    //        }).then(function (response) {
    //            if (response.data.Error === true)
    //                ShowResult(response.data.Message, 'failure');
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.Clear();
    //                $scope.getData();
    //                $scope.productNew.Id = response.data.inventoryIssue.Id;
    //            }
    //        }), function (response) {
    //            ShowResult(response.data.Message, 'failure');
    //        };
    //    }
    //    else ShowResult('Please issue material', 'failure');
    //};

    $scope.ApprovedStockList = [];
    $scope.getApprovedStock = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetApprovedStockDetail'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.ApprovedStockList = response.data;
            angular.element(document.querySelector('#ApprovedStockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.closeApprovedStockPopUp = function () {
        angular.element(document.querySelector('#ApprovedStockPopUp')).modal('hide');
    };

    $scope.ApprovedStockBeyondIssueDateList = [];
    $scope.getApprovedStockDetailBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetApprovedStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.ApprovedStockBeyondIssueDateList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.PostingStockList = [];
    $scope.getPostingStock = function (data) {
        $http({
            method: "POST",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'Products/InventoryIssue/GetPostingStockDetail',
            data: { entity: data, issueDate: $scope.productNew.IssueDate }

        }).then(function successCallback(response) {
            $scope.PostingStockList = response.data;
            angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
            //entrydata = copy(searchdata);
        });
    };

    //$scope.PostingStockList = [];
    //$scope.getPostingStock = function (data) {
    //    $http({
    //        method: 'POST'
    //        , url: $scope.path + 'GetPostingStockDetail'
    //        , data: { entity: data, issueDate: $scope.productNew.IssueDate }
    //        , dataType: 'JSON'
    //    }).then(function (response) {
    //        $scope.PostingStockList = response.data;
    //        angular.element(document.querySelector('#PostingStockPopUp')).modal('show');
    //    }), function (response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};
    $scope.closePostingStockPopUp = function () {
        angular.element(document.querySelector('#PostingStockPopUp')).modal('hide');
    };

    $scope.PostingStockBeyondIssueDateList = [];
    $scope.getPostingStockBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetPostingStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.PostingStockBeyondIssueDateList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };

    $scope.UnApprovedStockList = [];
    $scope.getUnApprovedStock = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetUnApprovedStockDetail'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.UnApprovedStockList = response.data;
            angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.closeUnApprovedStockPopUp = function () {
        angular.element(document.querySelector('#UnApprovedStockPopUp')).modal('hide');
    };

    $scope.UnApprovedStockDetailBeyondIssueDateList = [];
    $scope.getUnApprovedStockDetailBeyondIssueDate = function (data) {
        $http({
            method: 'POST'
            , url: $scope.path + 'GetUnApprovedStockDetailBeyondIssueDate'
            , data: { entity: data, issueDate: $scope.productNew.IssueDate }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.UnApprovedStockDetailBeyondIssueDateList = response.data;
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };


    //#region sk
    $scope.detailModelTemp = [];
    function getMaterialStock(b) {
      
        $http({
            method: 'POST',
            url: 'Products/InventoryIssue/GetJWStock',
            data: { entity: $scope.detailModel, issueDate: $scope.IssueTransformation.IssueDate },
            dataType: 'JSON'
        }).then(function (response) {
            
            $scope.detailList[$scope.indexforDetail].TotalQty = response.data.TotalQty;
            $scope.detailList[$scope.indexforDetail].PostingQty = response.data.PostingQty;
            $scope.detailList[$scope.indexforDetail].PostingQuantity = response.data.PostingQuantity;
            $scope.detailList[$scope.indexforDetail].ApprovedQty = response.data.ApprovedQty;
            $scope.detailList[$scope.indexforDetail].UnApprovedQty = response.data.UnApprovedQty;
            if (baseService.isUndefinedOrNull($scope.detailList[$scope.indexforDetail].TotalQty))
                $scope.errorText = 'This material has no stock';
            else $scope.errorText = null;
			
            
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    }
    $scope.getRequisitionList = function (issueDetailId) {
        $scope.materialStockList = [];
        $scope.specificStockList = [];
        $http({
            method: 'POST'
            , url: $scope.path + 'GetRequisitionList'
            , data: { issueDetailId: issueDetailId }
            , dataType: 'JSON'
        }).then(function (response) {
            $scope.materialStockList = response.data;
            angular.element(document.querySelector('#stockPopUp')).modal('show');
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.closeStockPopUp = function () {
        angular.element(document.querySelector('#stockPopUp')).modal('hide');
    };
    $scope.SaveSlipIssue = function () {
        var UIStatus = $("#SlipAssetIssueUI").val();
        //if (UIStatus === 'Asset') {
        //    if ($scope.materialStockList.length === 0) {
        //        ShowResult('Please select Specific GRN');
        //        return false;
        //    }
        //}
        //debugger;
        //if ($scope.detailList.length === 0) {
        //    ShowResult('Please select Atlest one material');
        //    return false;
        //}
        ////debugger;
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueDate)) {
            ShowResult("Select the issue date");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.EntityId)) {
            ShowResult("Select the Entity");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.MaterialStorageId)) {
            ShowResult("Select the Material Storage");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.IssueType)) {
            ShowResult("Select the type");
            return false;

        }
        if (baseService.isUndefinedOrNull($scope.IssueTransformation.EmpName)) {
            ShowResult("Select the wby whom");
            return false;

        }
        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].TransactionQty > $scope.detailList[i].PostingQty) {
                ShowResult("Issue qty can not gaterthen  Ready for issue Qty");
                return false;
            }
            //if ($scope.detailList[i].TransactionQty > $scope.detailList[i].BalanceQty) {
            //    ShowResult("Issue qty can not gaterthen  Balance Qty");
            //    return false;
            //}
            if (baseService.isUndefinedOrNull($scope.detailList[i].CostCenterId)) {
                ShowResult("Select the cost center");
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailList[i].MaterialMaster)) {
                ShowResult("Select Material Master");
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailList[i].ArticleName)) {
                ShowResult("Select ArticleName");
                return false;
            }
            if (baseService.isUndefinedOrNull($scope.detailList[i].TransactionQty)) {
                ShowResult("Enter the Issue Qty");
                return false;
            }
            if ($scope.detailList[i].TransactionQty=='0') {
                ShowResult("Enter the Issue Qty");
                return false;
            }

        }
        //for (var i = 0; i < $scope.detailList.length; i++) {
        //    if ($scope.detailList[i].TransactionQty > $scope.detailList[i].RequestedQty) {
        //        ShowResult("Issue qty can not gaterthen Requested Qty");
        //        return false;
        //    }
        //}

        //$scope.productNew.IssueRequestMasterId = $scope.issueId;
        if ($scope.Action === "Save") {
            $http({
                method: 'POST'
                , url: 'Products/InventoryIssue/JWIssueCreate'
                , data: {
                     entities: $scope.detailList
                    , specificStockList: $scope.specificStockList
                    , inventoryIssue: $scope.IssueTransformation
                    , IssueTypeStatus: 'Inventory'

                }
                , dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.Clear();
                   // $scope.getData();
                    //$scope.productNew.Id = response.data.inventoryIssue.Id;
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else ShowResult('Please issue material', 'failure');
    };

    $scope.addMaterialStock = function () {
        //debugger;
        try {
            qtyValidation($scope.materialStockList);
            validationWithTotal($scope.materialStockList);
            for (var i = baseService.arrayLength($scope.specificStockList) - 1; i >= 0; i--) {
                var row = $scope.specificStockList[i];
                for (var t = 0; t < baseService.arrayLength($scope.materialStockList); t++) {
                    var newRow = $scope.materialStockList[t];
                    if (row.InventoryReceiveDetailId === newRow.InventoryReceiveDetailId) { // update or delete
                        if (newRow.Flag) row.RequisitionQty = newRow.RequisitionQty;
                        else $scope.specificStockList.splice(i, 1);
                    }
                }
            }
            for (var n = 0; n < baseService.arrayLength($scope.materialStockList); n++) { // add
                var nRow = $scope.materialStockList[n];
                nRow.BaseQty = $scope.materialStockList[n].BaseIssueQty;
                if (!baseService.valueCheckInList($scope.specificStockList, 'InventoryReceiveDetailId', nRow.InventoryReceiveDetailId) && nRow.Flag)
                    //$scope.detailModel.IsSpecific = true;
                    $scope.specificStockList.push(nRow);
            }
            //$scope.detailList[$scope.index].TransactionQty = issueQty;
            angular.element(document.querySelector('#stockPopUp')).modal('hide');
            CloseModalShowResult();
        } catch (e) {
            ShowResult(e, 'failure', 'stockPopUp');
        }
    };
    //#endregion

    // PRINT JOB WORK TRANSFORMATION REPORT

    $scope.AllTabPrint = function (z) {
        //debugger;
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        location.href = "Products/InventoryIssue/JobWorkIssueReport?grnId=" + data.Id;

    };

  $scope.ConfirmPrintTab = function (p) {
        try {
        var x = "#" + p;
        var gridObj = $(x).data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];

            $scope.PrintTabId = data.JWContractId;
            $scope.IssueId = data.Id;
            var reportFormat = "Excel";
            window.open('JobWork/JobWorkIssueReturn/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
         //   $scope.getData();

        } catch (e) {

        }
    };

    // Transformation Stock Wise Status

    $scope.GetShowStorageLocationList = [];
    $scope.stockwisestatus = function (RowData, index) {
        $scope.GetShowStorageLocationList = [];
        angular.element(document.querySelector("#ShowLOcationWiseStock")).modal("show");

        for (var i = 0; i < $scope.detailList.length > 0; i++) {
            if ($scope.detailList[i].Id === RowData.Id) {
                $scope.MatMstId = $scope.detailList[i].InputMaterialId;
                $scope.SelectedArticleId = $scope.detailList[i].MaterialMasterArticleId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.MatMstId, ArticleId: $scope.SelectedArticleId, issueDate: $scope.IssueTransformation.IssueDate },
            url: 'Products/InventoryIssue/StorageLocationStockWise/'
        }).then(function successCallback(response) {
            $scope.GetShowStorageLocationList = response.data;
        });
    }

     $scope.GetPopUpShowStorageLocationClosed=function() {
      angular.element(document.querySelector('#ShowLOcationWiseStock')).modal('hide');

      }
}