'use strict';
QualityProcessNewController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QualityProcessNewController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Quality Process';
    $scope.Action = 'Save';
    $scope.UAction = 'Save';
    $scope.ModelList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'CreateQualityProcess';
    $scope.deleteUrl = $scope.path + 'DeleteQualityProcess/';
    $scope.searchBy = "QualityProcessUserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Process', name: "Process" }, { value: 'QualityProcessUserName', name: "Quality Process User Name" }, { value: 'QualityProcessStandardName', name: "Quality Process Standard Name" }, { value: 'CheckPointUserName', name: "Check Point User Name" }, { value: 'CheckPointStandardName', name: "Check  PointStandard Name" }, { value: 'Remarks', name: "Remarks" }];

    $scope.tab = 8;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ProcessList = [];
    $scope.getProcessCbo = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetProcessCbo",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.getProcessCbo();

    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
    }
    $scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.ModelNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.selectedProductMasterList = [];
    $scope.ProductMasterList = [];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetQualityProcessList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        ProcessId: null,
        QualityProcessUserName: null,
        QualityProcessStandardName: null,
        CheckPointUserName: null,
        CheckPointStandardName: null,
        Remarks: null,
        MaterialApplicable: null,
        OperationApplicable: null,
        GeneralApplicable: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetProdMasterData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
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
                    $scope.ModelNew.Id = response.data.Data.Id;
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
    }

    // #region Product

    $scope.searchByProduct = "UserName"; $scope.searchprod = "";
    $scope.searchByProductList = [{ value: 'UserName', name: "UserName" }, { value: 'StandardName', name: "StandardName" }, { value: 'ProductCategory', name: "ProductCategory" }, { value: 'ProductSubCategory', name: "ProductSubCategory" }];
    $scope.ProductMasterList = [];

    $scope.GetproductPopUp = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/GetProductMasterList',
            data: { column: $scope.searchByProduct, value: $scope.searchprod },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.ProductMasterList = response.data;
        });
        angular.element(document.querySelector('#productPopUpId')).modal('show');

    };

    $scope.GetProductMasterDataList = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/GetProductMasterList',
            data: { column: $scope.searchByProduct, value: $scope.searchprod },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.ProductMasterList = response.data;
        });
    };

    $scope.refreshTemplateProd = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllProd });
    };

    function CheckBoxSelectAllProd(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#ProdGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProductMasterList.length; i++) {
                $scope.ProductMasterList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#ProdGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.PMList = [];
    function MakePMListData() {

        for (var i = 0; i < $scope.ProductMasterList.length; i++) {
            if ($scope.ProductMasterList[i].Flag == true) {
                if (checkPMExists($scope.PMList, $scope.ProductMasterList[i].Id) === false) {
                    var ob = {};
                    ob.Id = Math.floor(Math.random() * 9) - 10;
                    ob.ProductMasterId = $scope.ProductMasterList[i].ProductMasterId;
                    ob.QualityProcessMasterId = $scope.ModelNew.Id;

                    $scope.PMList.push(ob);
                }
                else {
                    throw "This Product Master" + $scope.ProductMasterList[i].UserName + " is already taken.";
                }
            }
        }

    }

    function checkPMExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductMasterId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseProduct = function () {
        try {
            MakePMListData();
            $scope.SavePM();
            angular.element(document.querySelector('#productPopUpId')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SavePM = function () {
        try {

            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/SaveProdMaster',
                data: { 'PMList': $scope.PMList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetProdMasterData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.PMList = [];
    $scope.GetProdMasterData = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetQualityProcessProductMaster?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.PMList = response.data;
            $scope.GetPMAData();
        });
    }

    $scope.message_detailconfirmation = null;
    $scope.removePM = function (obj) {
        $scope.PMNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.PMNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.PMNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmPMPopUp')).modal('show');
    }

    $scope.DeletePM = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteQualityProcessProductMaster?id=' + $scope.PMNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetProdMasterData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion

    // #region Article

    $scope.searchByMaterial = "MaterialMasterName"; $scope.search = "";
    $scope.searchByMaterialList = [{ value: 'MaterialMasterName', name: "Material" }, { value: 'StandardName', name: "Article" }, { value: 'MaterialTypeName', name: "MaterialType" }
        , { value: 'MaterialGroupMasterName', name: "MaterialGroup" }, { value: 'HSNCode', name: "HSNCode" }, { value: 'BusinessProcessName', name: "Business Process" }];

    $scope.ActionSM = "Save";
    $scope.getArticle = function () {
        $scope.GetMaterialMasterWithArticleDataByProductMaster();
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.materialArticleList = [];
    $scope.sqlInStatement = "";
    $scope.GetMaterialMasterWithArticleDataByProductMaster = function () {
        $scope.idList = [];
        for (var di = 0; di < $scope.PMList.length; di++) {
            $scope.idList.push($scope.PMList[di]);
        }

        if ($scope.idList.length > 0) {
            var uniqueProductMasterId = removeDuplicates($scope.idList, 'ProductMasterId');
            var wcEmpCode = "";
            if (uniqueProductMasterId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueProductMasterId, function (item) { return "'" + item.ProductMasterId + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }


        $http({
            method: 'POST',
            url: 'Materials/MaterialMasterArticle/GetMaterialMasterWithArticleDataByProductMaster?ProductMasterId=' + $scope.sqlInStatement,
            data: { column: $scope.searchByMaterial, value: $scope.search },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.materialArticleList = response.data;
        });
        angular.element(document.querySelector('#materialarticleCbxPopUp')).modal('show');

    };

    // #region checkbox all

    $scope.refreshTemplatearticle = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllarticle });
    };

    function CheckBoxSelectAllarticle(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#PAGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialArticleList.length; i++) {
                $scope.materialArticleList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#PAGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.machineList = [];
    function MakeData() {
        for (var i = 0; i < $scope.materialArticleList.length; i++) {
            if ($scope.materialArticleList[i].Flag == true) {
                if (checkExists($scope.machineList, $scope.materialArticleList[i].Id) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.ArticleId = $scope.materialArticleList[i].Id;
                    ob.QualityProcessMasterId = $scope.ModelNew.Id;

                    $scope.machineList.push(ob);
                }
                else {
                    throw "This Article " + $scope.materialArticleList[i].StandardName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ArticleId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseArticle = function () {
        try {
            MakeData();
            $scope.SavePMA();
            angular.element(document.querySelector('#materialarticleCbxPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SavePMA = function () {
        try {
            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/SaveArticel',
                data: { 'machineList': $scope.machineList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetPMAData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.articleList = [];
    $scope.GetPMAData = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetQualityProcessArticle?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.articleList = response.data;
            $scope.getUserNameData();
        });
    }

    $scope.removeART = function (obj) {
        $scope.ARTNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.ARTNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.ARTNew.StandardName + ' ]';
        angular.element(document.querySelector('#confirmARTPopUp')).modal('show');
    }

    $scope.DeleteART = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteQualityProcessArticle?id=' + $scope.ARTNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetPMAData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // #endregion Article

    // #region UserName
    $scope.UserNamemodel = { Id: Math.floor(Math.random() * 9) - 10, UserName: null, QualityProcessMasterId: $scope.ModelNew.Id };

    $scope.SaveU = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.tabForm3.$valid) {
            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/CreateUserName',
                data: { 'data': $scope.UserNamemodel, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getUserNameData();
                    $scope.UserNamemodel = { Id: Math.floor(Math.random() * 9) - 10, UserName: null, QualityProcessMasterId: $scope.ModelNew.Id };
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.GetUN = function (args) {
        $scope.UserNamemodel = Object.assign({}, args.data);
        $scope.UAction = 'Update';
    };

    $scope.ClearU = function () {
        $scope.UserNamemodel = { Id: Math.floor(Math.random() * 9) - 10, UserName: null, QualityProcessMasterId: $scope.ModelNew.Id };
        $scope.UAction = 'Save';
    };

    $scope.UserNameList = [];
    $scope.BudgetCodeList = [];
    $scope.authorizationList = [];
    $scope.popUpEmpDataList = [];
    $scope.getUserNameData = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetQualityProcessUserName?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.UserNameList = response.data;
            $scope.GetBudgetCodeData();
        });
    }

    $scope.removeUN = function (obj) {
        $scope.UNNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.UNNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.UNNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmUNPopUp')).modal('show');
    }

    $scope.DeleteUN = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteUN?id=' + $scope.UNNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getUserNameData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
    // #endregion UserName

    $scope.operationList = [];



    //#region BudgetCode

    $scope.name = null;
    $scope.popUpTitle = "Manpower Budget Information";
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.budgetpopUpParameters = {
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


    $scope.popUpDataList = [];
    $scope.popUpBudgetCode = function () {
        try {
            $scope.popUpUrl = 'employees/recruitment/GetBudgetCodeList';

            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: $scope.popUpUrl

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data.Rows;
                for (var i = 0; i < $scope.popUpDataList.length; i++) {
                    $scope.popUpDataList[i].isSelected = 0;
                }

            });
            angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.BudgetCodeList = [];

    $scope.refreshTemplate = function (args) {
        $("#headchkGWS").ejCheckBox({ "change": CheckBoxSelectGWS });
    };
    function CheckBoxSelectGWS(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridpopUpId").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridpopUpId").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.BudgetCodeList = [];
    $scope.BCList = [];
    $scope.selectDoubleClick = function () {
        try {
            var ob = {};
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].isSelected == true) {
                    if (checkDoubleGWS($scope.BCList, $scope.popUpDataList[i].Id) === false) {
                        ob.Id = Math.floor(Math.random() * 9) - 10;
                        ob.ManpowerBudgetId = $scope.popUpDataList[i].Id;
                        ob.QualityProcessMasterId = $scope.ModelNew.Id;
                        $scope.BCList.push(ob);
                        ob = {};
                    }
                }
            }
            $scope.BCSave();
            angular.element(document.querySelector('#popUpId')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleGWS(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ManpowerBudgetId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };
       
    $scope.BCSave = function () {
        try {
            $http({
                method: 'POST',
                url: "QMS/QualityProcess/CreateBudgetCode",
                data: {
                    'data': $scope.BCList
                    , 'masterId': $scope.ModelNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetBudgetCodeData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetBudgetCodeData = function () {
        $http({
            method: 'GET',
            url: "QMS/QualityProcess/GetQualityProcessManpowerBudget?masterId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.BudgetCodeList = response.data;
            //$scope.GetBudgetedEmployee();
        });
    }

    $scope.removeBC = function (obj) {
        $scope.BCNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.BCNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.BCNew.Code + ' ]';
        angular.element(document.querySelector('#confirmBCPopUp')).modal('show');
    }

    $scope.DeleteBC = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteQualityProcessManpowerBudget?id=' + $scope.BCNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetBudgetCodeData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    //#endregion BudgetCode

}