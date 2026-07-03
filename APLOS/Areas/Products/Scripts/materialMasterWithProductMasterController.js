'use strict';
materialMasterWithProductMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function materialMasterWithProductMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Product";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Products/productdefinition/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.savesingleUrl = $scope.path + 'CreateProductDefinition';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.GetFabricList = function () {
        baseService.init($scope.getListUrl, null, null, null, 'UserName', 'UserName');
        $scope.getData = function (pageno) {
            var tempParam = [];
            tempParam.push($scope.searchModel.UserName);
            tempParam.push($scope.searchModel.BaseUoM);
            tempParam.push($scope.searchModel.ProductMasterName);
            tempParam.push($scope.searchModel.SeasonName);
            tempParam.push($scope.searchModel.OurStyleName);
            $rootScope.parameters.tempParam = JSON.stringify(tempParam);
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.buyerStyles = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        angular.element(document.querySelector('#fabricId')).modal('show');
    };
    $scope.model = {
        Id: null
        , MaterialMasterId: null
        , Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , ProductMasterId: null
        , SeasonId: null
        , OurStyleId: null
        , CostAndManufacture: null
        , CostAndManufactureCurrencyId: null
        , DaysToReachTheTarget: null
        , FirstdayOutPut: null
        , IsFixed: 'Fixed'
        , IncrementValue: null
        , ProcessId:null
        , Active: true
    };

    $scope.searchModel = {
        Code: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , BaseUoM: null
        , ProductMasterName: null
        , SeasonName: null
        , OurStyleName: null
    };

    $scope.productMasterList = [];
    cboService.getPMCbo(function (result) {
        $scope.productMasterList = result.Rows;
    });

    $scope.materialMasters = [];
    $scope.getSavedData = function () {
        $scope.materialMasters = [];
        $http.get("Products/ProductDefinition/GetSavedData")
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.materialMasters = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        //}
    };
    $scope.getSavedData();

    $scope.searchdata = [];
    $scope.LoadUnsavedData = function () {
        $scope.searchdata = [];
        $http.get('Products/ProductDefinition/GetUnSavedMaterialMasterList')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.searchdata = response.data;
                    }
                    cboService.getPMCbo(function (result) {
                        $scope.productMasterList = result.Rows;
                    });
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        //}
    };

    $scope.onClick = function (args) {
        var gridObj = $("#Grid").data("ejGrid");
        //getting corresponding record             
        var data = gridObj.getSelectedRecords()[0];
        $scope.valuePassInDelModal(data);

    };
    $scope.command = [{
        type: "details", buttonOptions: {
            text: "Delete",
            width: "100",
            click: $scope.onClick
        }
    }];

    $scope.valuePassInDelModal = function (data) {
        $scope.tempEmpOb = data;
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.UserName + ']</b>';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.UserName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempEmpOb.Id) === true) {
            $scope.materialMasters.splice($scope.empIndex, 1);
            //$scope.empIndex = -1;
            $scope.tempEmpOb.Id = null;
        } else {
            $scope.removeFromDb($scope.tempEmpOb.Id, $scope.empIndex);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.removeFromDb = function (id, index) {
        try {
            $http({
                method: 'POST',
                url: 'Products/ProductDefinition/Delete',
                dataType: 'JSON',
                data: { 'id': id }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.materialMasters = [];
                    $scope.searchdata = [];
                    $scope.getSavedData();
                    $scope.LoadUnsavedData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    // #region checkbox all

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.searchdata, { 'MaterialMasterId': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Flag = true;
            else
                row[0].Flag = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Flag = true;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].MaterialMasterId == filtered[j].MaterialMasterId)
                            $scope.searchdata[i].Flag = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    $scope.searchdata[i].Flag = false;
                }
            }
            else {
                for (var i = 0; i < $scope.searchdata.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.searchdata[i].MaterialMasterId == filtered[j].MaterialMasterId)
                            $scope.searchdata[i].Flag = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    }
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.searchdata, { 'MaterialMasterId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].Flag == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    }

    // #endregion

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.model = $scope.buyerStyles[$scope.index];
        $scope.model = Object.assign({}, $scope.model);
        getArticleList();
        getEfficencyList();
        $scope.Action = 'Update';
        angular.element(document.querySelector('#fabricId')).modal('hide');
    };

    $scope.ValidationForUoM = function () {
        for (var i = 0; i < $scope.searchdata.length; i++) {
            for (var j = 0; j < $scope.productMasterList.length; j++) {
                if ($scope.searchdata[i].ProductMasterId == $scope.productMasterList[j].Value) {
                    if ($scope.searchdata[i].BaseUOMId != $scope.productMasterList[j].BaseUOMId) {
                       // throw "Material Master UoM and Product Master UoM should same.";
                        ShowResult("Material Master UoM and Product Master UoM should same.", "failure");
                    }
                }
            }
            
        }
    }

  

    function MakeDataForSave() {

        $scope.materialMasters = [];
        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Flag == true) {
                $scope.materialMasters.push($scope.searchdata[i]);
            }
        }

        ////getting corresponding record             
        //for (var j = 0; j < $scope.materialMasters.length; j++) {
        //    $scope.materialMasters[j].ProductMasterId = $scope.model.ProductMasterId;
        //}
    }

    $scope.Save = function () {
        try {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                for (var j = 0; j < $scope.productMasterList.length; j++) {
                    if ($scope.searchdata[i].ProductMasterId == $scope.productMasterList[j].Value) {
                        if ($scope.searchdata[i].BaseUOMId != $scope.productMasterList[j].BaseUOMId) {
                             throw "Material Master UoM and Product Master UoM should same.";
                        }
                    }
                }

            }

            MakeDataForSave();
            if (baseService.arrayLength($scope.materialMasters)==0) {
                throw "Select an Item.";
            }
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.productmaterialForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.savesingleUrl,
                    data: $scope.materialMasters,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.materialMasters = [];
                        $scope.searchdata = [];
                        $scope.getSavedData();
                        $scope.LoadUnsavedData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.model.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.model.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
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

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.model = { Active: true, IsFixed: 'Fixed' };
        $scope.prdNameList = [];
        $scope.articleList = [];
        $scope.efficencyList = [];
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
}