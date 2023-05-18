'use strict';
jwTransformationMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function jwTransformationMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'Job Work Transformation';
    $scope.Action = 'Save';
    $scope.inputMaterialAction = 'Save';
    $scope.byProductAction = 'Save';
    $scope.popUpFlag = 'Main';

    $scope.ModelList = [];
    $scope.path = 'Outsourcing/JWTransformationMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveInputMaterialUrl = $scope.path + 'CreateInputMaterial';
    $scope.saveByProductUrl = $scope.path + 'CreateByProduct';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteByPrductUrl = $scope.path + 'DeleteByPrduct/';
    $scope.deleteInputMaterialUrl = $scope.path + 'DeleteInputMaterial/';

    $scope.productionPrcoessList = [];
    $scope.jobWorkActivityList = [];
    $http.get('Outsourcing/JWActivity/GetProductionProcessList')
        .then(function (response) {
            $scope.productionPrcoessList = response.data;
        });

    $http.get('Outsourcing/JWTransformationMaster/GetJobWorkActivityList')// Only Transformation
        .then(function (response) {
            $scope.jobWorkActivityList = response.data;
        });

    $scope.currencyList = [];
    cboService.getCompanyGroupCurrencyCbo($window.CompanyGroupId, function (result) {
        $scope.currencyList = result;
    });


    $scope.serviceCboList = [];
    $http.get('Setups/CompanyServiceMaster/GetCboList')
        .then(function (response) {
            $scope.serviceCboList = response.data;
        });
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    };
    $scope.getData();

    $scope.ByProductItemList = [];
    $scope.InputMaterialItemList = [];
    $scope.getInputMaterialItemList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInputMaterialItemList",
            data: { 'JWTransformationId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InputMaterialItemList = response.data;
        });
    };
    $scope.getByProductItemList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetByProductItemList",
            data: { 'JWTransformationId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ByProductItemList = response.data;
        });
    };




    $scope.unitOfMeasurementList = [];
    function UomCboByFGMaterialMaster(materilaMasterId) {
        var mmId = []; mmId.push(materilaMasterId);
        cboService.getUomCboByMaterialMaster(JSON.stringify(mmId), function (response) {
            $scope.unitOfMeasurementList = response;
            if (baseService.arrayLength($scope.unitOfMeasurementList) == 1) {
                $scope.ModelNew.OutputMaterialUOMId = $scope.unitOfMeasurementList[0].Value;
            }
        });
    }
    $scope.materialFlag = null;
    //$scope.getMaterial = function (flag) {
    //    $scope.getItemData();
    //    angular.element(document.querySelector('#materialMasterbyTypePopupNew')).modal('show');
    //};
    $scope.getMaterial = function (obj) {
        if ($scope.popUpFlag == 'Main') {
            $scope.ModelNew.OutputMaterialId = obj.data.Id;
            $scope.ModelNew.OutputMaterial = obj.data.UserName;
            $scope.ModelNew.UOM = obj.data.UOM;
            $scope.ModelNew.OutputMaterialUOMId = obj.data.UOMId;
            $scope.ModelNew.Material = obj.data.MaterialMaster;
        }
        if ($scope.popUpFlag == 'InputMaterial') {
            $scope.ModelNewInputMaterial.MaterialId = obj.data.Id;
            $scope.ModelNewInputMaterial.Material = obj.data.UserName;
            $scope.ModelNewInputMaterial.UOM = obj.data.UOM;
            $scope.ModelNewInputMaterial.UOMId = obj.data.UOMId;
        }
        if ($scope.popUpFlag == 'ByProduct') {
            $scope.ModelNewByProdcuct.MaterialId = obj.data.Id;
            $scope.ModelNewByProdcuct.Material = obj.data.UserName;
            $scope.ModelNewByProdcuct.UOM = obj.data.UOM;
            $scope.ModelNewByProdcuct.UOMId = obj.data.UOMId;
        }
        angular.element(document.querySelector('#materialMasterbyTypePopupNew')).modal('hide');
    };


    $scope.getMaterialPOPUp = function () {
        $scope.getItemData();
        angular.element(document.querySelector('#materialMasterbyTypePopupNew')).modal('show');

    };

    $scope.closeMaterialPOPUp = function () {
        angular.element(document.querySelector('#materialMasterbyTypePopupNew')).modal('hide');
    };

    $scope.getMaterialMultiple = function (index, flag) {
        $scope.itemIndex = index;
        $scope.materialFlag = flag;

        $scope.getMaterialMasterbyTypePopUpMultiple();
    };
    $scope.selectMaterialByTypeMultiple = function (ob) {
        if ($scope.materialFlag == 'INPUTMATERIAL') {
            $scope.itemList[$scope.itemIndex].MaterialId = ob.Id;
            $scope.itemList[$scope.itemIndex].MaterialMaster = ob.UserName;
            UomCboByFGMaterialMaster($scope.itemList[$scope.itemIndex].MaterialId);
        }
        else if ($scope.materialFlag == 'BYPRODUCT') {
            $scope.itemList[$scope.itemIndex].OutputMaterialId = ob.Id;
            $scope.itemList[$scope.itemIndex].MaterialMaster = ob.UserName;
            UomCboByFGMaterialMaster($scope.itemList[$scope.itemIndex].MaterialId);
        }
        angular.element(document.querySelector('#materialMasterbyTypePopupMultiple')).modal('hide');
    };


    //#region Partial View
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.materialType = ['Asset', 'Consumable', 'Spare', 'RawMaterial'];
    //#endregion
    $scope.insertflag = false;




    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        JWActivityId: null,
        ProcessId: null,
        ResponsiblePersonId: null,
        OutputMaterialId: null,
        OutputMaterial: null,
        OutputMaterialUOMId: null,
        UOM: null,
        Material: null,
        RateApplicableOn: null,
        CurrencyId: null,
        MinRate: 0,
        MaxRate: 0,
        CycleTimeDays: 0,
        ByProductApplicable: false,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();


    $scope.uOMList = [];
    $scope.GetUOM = function () {
        cboService.getUomCboByMaterialMaster($scope.ModelNew.OutputMaterialId, function (response) {
            $scope.uOMList = response;
        });
    };


    $scope.currencyList = [];
    //cboService.getCboParallelCurrency(function (response) {
    //    $scope.currencyList = response;
    //});



    $scope.selectResponsiblePersonPopUp = function (index, id) {
        $scope.updateResponsiblePersonIndex = index;
        $scope.selectedResponsiblePerson = id;
    };
    $scope.updateResponsiblePersonIndex = -1;
    $scope.closeResponsiblePersonPopUp = function () {
        if ($scope.updateResponsiblePersonIndex !== -1) {
            var employee = $scope.employeeList[$scope.updateResponsiblePersonIndex];
            $scope.ModelNew.ResponsiblePersonName = employee.EmployeeName;
            $scope.ModelNew.ResponsiblePersonId = employee.SystemId;
        }
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    $scope.clearResponsiblePerson = function () {
        $scope.ModelNew.ResponsiblePersonName = null;
        $scope.ModelNew.ResponsiblePersonId = null;
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNewInputMaterial.JWTransformationMasterId = $scope.ModelNew.Id;
        $scope.ModelNewByProdcuct.JWTransformationMasterId = $scope.ModelNew.Id;



        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.insertflag = true;
        $scope.getInputMaterialItemList();
        $scope.getByProductItemList();
    };


    $scope.GetInputMaterial = function (args) {

        $scope.ModelNewInputMaterial = Object.assign({}, args.data);
        $scope.inputMaterialAction = 'Update';
        $scope.openInputMaterialPOPUP();
    };
    $scope.GetByProduct = function (args) {
        $scope.ModelNewByProdcuct = Object.assign({}, args.data);
        $scope.byProductAction = 'Update';
        $scope.openByProductPOPUP();
    };


    $scope.Save = function () {

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            if ($scope.ModelNew.MaxRate < $scope.ModelNew.MinRate) {
                ShowResult('Min Rate cannot be greater Max Rate', 'failure');
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew, 'InputMaterialList': $scope.itemList, 'ByProductList': $scope.ByProductList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.getData();
                        //$scope.insertflag = false;
                        $scope.ModelNew.Id = response.data.Data.Id;
                        if ($scope.ModelNew.Id !== null) {
                            $scope.insertflag = true;
                        }
                        $scope.ModelNewInputMaterial.JWTransformationMasterId = $scope.ModelNew.Id;

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

        }
    };

    $scope.saveInputMaterial = function () {

        $scope.$broadcast('show-errors-check-validity');
        //if ($scope.ModelinputMaterialForm.$valid) {
        $scope.ModelNewInputMaterial.JWTransformationMasterId = $scope.ModelNew.Id;
        $http({
            method: 'POST',
            url: $scope.saveInputMaterialUrl,
            data: { 'data': $scope.ModelNewInputMaterial },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                inputMaterialClearFields();
                $scope.getInputMaterialItemList();


            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

        //}
    };
    $scope.saveByProduct = function () {

        $scope.$broadcast('show-errors-check-validity');
        //if ($scope.ModelinputMaterialForm.$valid) {
        $scope.ModelNewByProdcuct.JWTransformationMasterId = $scope.ModelNew.Id;

        $http({
            method: 'POST',
            url: $scope.saveByProductUrl,
            data: { 'data': $scope.ModelNewByProdcuct },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                byProductClearFields();
                $scope.getByProductItemList();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };

        //}
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
                    ClearFields(response.data.Sequence);
                    byProductClearFields();
                    inputMaterialClearFields();
                    $scope.getData();
                    $scope.getByProductItemList();
                    $scope.getInputMaterialItemList();


                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.DeleteByPrduct = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewByProdcuct.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteByPrductUrl + $scope.ModelNewByProdcuct.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    byProductClearFields();
                    $scope.getByProductItemList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.DeleteInputMaterial = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewInputMaterial.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteInputMaterialUrl + $scope.ModelNewInputMaterial.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    inputMaterialClearFields();
                    $scope.getInputMaterialItemList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.JWActivityList = [];
    $scope.JWActivityListTemp = [];

    //$scope.GetJWActivityList = function (activityId) { 

    //            var parameters = { 'activityId': activityId };
    //            $http({
    //                method: "POST",
    //                dataType: 'JSON',
    //                url: $scope.path + "GetJobWorkActivityList"
    //                //data: parameters
    //            }).then(function successCallback(response) {
    //                if (response.data.length > 0) {
    //                    $scope.empGrid = true;

    //                    $scope.JWActivityList = response.data;

    //                }               
    //                var gridObj = $("#empInfoGrid").data("ejGrid");

    //            });

    //    };
    //$scope.GetJWActivityList(null);


    function checkChangeJWActivity(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.JWActivityList, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headcheckChangeJWActivity(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#GridJWActivity").data("ejGrid");
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {

                    $scope.JWActivityList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.JWActivityList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.JWActivityList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        else {
            var filtered = $("#GridJWActivity").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {
                    $scope.JWActivityList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.JWActivityList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.JWActivityList[i].Id == filtered[j].Id)
                            $scope.JWActivityList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#GridJWActivity .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#GridJWActivity .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeJWActivity });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#GridJWActivity .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });

    };
    $scope.refreshTemplateJWActivity = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headcheckChangeJWActivity });
        }

        var valobj = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#GridJWActivity .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeJWActivity });
    };
    $scope.saveJWActivitydata = function () {
        $scope.JWActivityListTemp = [];
        var row = $filter('filter')($scope.JWActivityList, { 'isToBeSelect': true });

        $scope.JWActivityListTemp = row;


        $scope.Back();
    };
    //$scope.showJWActivityFilterScreen = function () {
    //    try {

    //      var gridObj = $("#GridJWActivity").data("ejGrid");
    //   gridObj.clearFiltering();
    //      angular.element(document.querySelector('#empfilterPopUp')).modal('show');


    // } catch (e) {
    //    ShowResult(e, 'failure');
    // }
    // };

    $scope.addactivity = function () {
        try {

            for (var i = 0; i < $scope.JWActivityList.length; i++) {
                $scope.JWActivityList[i]["isToBeSelect"] = false;

                for (var k = 0; k < $scope.JWActivityListTemp.length; k++) {
                    if ($scope.JWActivityList[i]["JWActivityId"] == $scope.JWActivityListTemp[k]["JWActivityId"]) {
                        $scope.JWActivityList[i]["isToBeSelect"] = true;
                    }


                }
            }

            angular.element(document.querySelector('#JWActivityFilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#JWActivityFilterPopUp')).modal('hide');
    };
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        $scope.JWActivityListTemp = [];
        $scope.inputMaterialClear();
        $scope.byProductClear();
        $scope.insertflag = false;
        return true;
    };


    $scope.inputMaterialClear = function () {

        inputMaterialClearFields();
        // $scope.JWActivityListTemp = [];
        return true;
    };

    $scope.byProductClear = function () {
        byProductClearFields();
        // $scope.JWActivityListTemp = [];
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    function inputMaterialClearFields() {
        $scope.inputMaterialAction = 'Save';
        $scope.InputMaterialItemList = [];
        // $scope.ByProductItemList = [];
        $scope.ModelNewInputMaterial = Object.assign({}, $scope.ModelTempInputMaterial);
    }


    function byProductClearFields() {
        $scope.byProductAction = 'Save';
        $scope.ByProductItemList = [];

        $scope.ModelNewByProdcuct = Object.assign({}, $scope.ModelTempByProdcuct);
    }
    $scope.employeeList = [];
    $scope.showAllEmployeeListPopUp = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Outsourcing/JWItem/EmployeeListAll'
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.employeeList = response.data;
                angular.element(document.querySelector('#responsiblePersonPopUp')).modal('show');
            }
            else {
                ShowResult("No Data Found", 'failure');
            }
        });
    };


    $scope.showAllEmployeeListMultiplePopUp = function ($index, flag) {
        $scope.materialFlag = flag;

        if ($scope.employeeList.length > 0) {
            angular.element(document.querySelector('#responsiblePersonPopUpMultiple')).modal('show');
        }
        else {
            $http({
                method: "GET",
                dataType: 'JSON',
                url: 'Outsourcing/JWActivity/EmployeeListAll'
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    $scope.employeeList = response.data;
                    angular.element(document.querySelector('#responsiblePersonPopUpMultiple')).modal('show');
                }
                else {
                    ShowResult("No Data Found", 'failure');
                }
            });
        }
    };
    $scope.getEmp = function (obj) {
        if ($scope.popUpFlag == 'Main') {
            $scope.ModelNew.ResponsiblePersonId = obj.data.SystemId;
            $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeName;
        }
        if ($scope.popUpFlag == 'InputMaterial') {
            $scope.ModelNewInputMaterial.ResponsiblePersonId = obj.data.SystemId;
            $scope.ModelNewInputMaterial.ResponsiblePersonName = obj.data.EmployeeName;
        }
        if ($scope.popUpFlag == 'ByProduct') {
            $scope.ModelNewByProdcuct.ResponsiblePersonId = obj.data.SystemId;
            $scope.ModelNewByProdcuct.ResponsiblePersonName = obj.data.EmployeeName;
        }
        angular.element(document.querySelector('#responsiblePersonPopUp')).modal('hide');
    };





    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#responsiblePersonPopUp")).modal("hide");
    };
    $scope.jwItemList = [];
    $scope.getItemData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetItemList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.jwItemList = response.data;
        });
    };

    //$scope.getEmpMultiple = function (obj) {

    //    if ($scope.materialFlag == 'INPUTMATERIAL') {
    //        $scope.itemList[$scope.itemIndex].ResponsiblePersonId = obj.data.SystemId;
    //        $scope.itemList[$scope.itemIndex].ResponsiblePersonName = obj.data.EmployeeName;
    //    }
    //    else if ($scope.materialFlag == 'BYPRODUCT') {
    //        $scope.itemList[$scope.itemIndex].ResponsiblePersonId = obj.data.SystemId;
    //        $scope.itemList[$scope.itemIndex].ResponsiblePersonName = obj.data.EmployeeName;
    //    }
    //    angular.element(document.querySelector('#responsiblePersonPopUpMultiple')).modal('hide');
    //};
    //$scope.closeResponsiblePersonPopUpMultiple = function () {
    //    angular.element(document.querySelector("#responsiblePersonPopUpMultiple")).modal("hide");
    //};
    $scope.clearResponsiblePerson = function () {
        $scope.ModelNew.ResponsiblePersonName = null;
        $scope.ModelNew.ResponsiblePersonId = null;
    };
    $scope.itemList = [];
    $scope.ModelTempInputMaterial = {
        Id: null,
        JWTransformationMasterId: null,
        MaterialId: null,
        Material: null,
        UOMId: null,
        UOM: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        MaterialSpecification: null,
        WastagePercentage: 0,
        NetConsumptionOROutputUnit: 0,
        Rejection: 0,
        ValueLoss: null,
        GrossConsumption: null,
        StandardQuantity: null
    };
    $scope.ModelNewInputMaterial = Object.assign({}, $scope.ModelTempInputMaterial);

    $scope.ModelTempByProdcuct = {
        Id: null,
        JWTransformationMasterId: null,
        MaterialId: null,
        Material: null,
        UOMId: null,
        UOM: null,
        ResponsiblePersonId: null,
        MaterialSpecification: null,
        WastagePercentage: 0,
        NetConsumptionOROutputUnit: 0,
        Rejection: 0,
        ValueLoss: 0,
        GrossQuantityOrInputUnit: 0,
        StandardRateORUnit: 0,
        StandardQtyORInputUnit: 0,
        CurrencyId: null
    };
    $scope.ModelNewByProdcuct = Object.assign({}, $scope.ModelTempByProdcuct);

    $scope.GrossConsumptionByProduct = function () {
        $scope.ModelNewByProdcuct.GrossQuantityOrInputUnit = $scope.ModelNewByProdcuct.StandardQtyORInputUnit * (1 - (($scope.ModelNewByProdcuct.Rejection / 100) - ($scope.ModelNewByProdcuct.ValueLoss / 100)));
    };

    $scope.GrossQtyORInputUnit = function () {
        $scope.ModelNewInputMaterial.GrossConsumption = $scope.ModelNewInputMaterial.NetConsumptionOROutputUnit / (1 - (($scope.ModelNewInputMaterial.Rejection / 100) - ($scope.ModelNewInputMaterial.ValueLoss / 100)));
    };
    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
    // #region Material by material type

    $scope.materialMasterbyTypeList = [];
    $scope.searchMaterialMasterList = [
        {
            'Text': 'Material Type',
            'Value': 'MaterialTypeName'
        },
        {
            'Text': 'Material Group',
            'Value': 'MaterialGroupMasterName'
        },
        {
            'Text': 'Code',
            'Value': 'Code'
        },
        {
            'Text': 'Material',
            'Value': 'UserName'
        },
        {
            'Text': 'Product',
            'Value': 'ProductMasterName'
        },
        {
            'Text': 'Id',
            'Value': 'Id'
        }
    ];
    $scope.getMaterialMasterbyTypePopUpMultiple = function () {
        CloseModalShowResult('materialMasterbyTypePopupMultiple');
        angular.element(document.querySelector('#materialMasterbyTypePopupMultiple')).modal('hide');
    };
    $scope.closeMaterialMasterbyTypePopUpMultiple = function () {
        CloseModalShowResult('materialMasterbyTypePopupMultiple');
        angular.element(document.querySelector('#materialMasterbyTypePopupMultiple')).modal('hide');
    };

    // #endregion Material by material type


    $scope.openInputMaterialMasterPopup = function () {
        angular.element(document.querySelector('#inputMaterialMasterPopup')).modal('show');
    };
    $scope.closeInputMaterialMasterPopup = function () {
        angular.element(document.querySelector('#inputMaterialMasterPopup')).modal('hide');
    };

    $scope.openInputMaterialPOPUP = function () {
        $scope.popUpFlag = 'InputMaterial';
        angular.element(document.querySelector('#inputMaterialPoPUp')).modal('show');
    };
    $scope.openByProductPOPUP = function () {
        $scope.popUpFlag = 'ByProduct';
        angular.element(document.querySelector('#byProductMaterialPoPUp')).modal('show');
    };
    $scope.closeInputMaterialPOPUP = function () {
        $scope.popUpFlag = 'Main';
        angular.element(document.querySelector('#inputMaterialPoPUp')).modal('hide');
    };
    $scope.closeByProductPOPUP = function () {
        $scope.popUpFlag = 'Main';
        angular.element(document.querySelector('#byProductMaterialPoPUp')).modal('hide');
    };
}