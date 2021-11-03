'use strict';
ItemConsumptionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', '$window'];
function ItemConsumptionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $window) {
    $rootScope.title = 'Item Consumption';
    $scope.path = "Costings/ItemConsumption/";

    $scope.Height = [];
    $scope.Width = [];
    $scope.ComponentList = [];

    //#region --Model--

    $scope.ModelNew = {
        Id: null,
        ProductMasterId: null,
        CostingItemId: null,
        Description: null,
        GSMValue: null,

    }

    $scope.ComponentNew = {
        Id: null,
        ItemConsumtionMasterId: null,
        ComponentName: null,
        NoOfParts: 1,
        AreaType: '',
    }

    //#endregion

    $scope.ProductMasterList = [];
    cboService.getProductMasterCbo(function (result) {
        $scope.ProductMasterList = result.Rows;
    });

    //#region -- G E T 

    $scope.ModelList = [];
    $scope.getData = function (CostingItemId) {
        var CostItemId = null;
        if (baseService.isUndefinedOrNull(CostingItemId)) {
            CostItemId = $scope.ModelNew.CostingItemId;
        }
        else {
            CostItemId = CostingItemId
        }

        $http({

            method: 'POST',
            url: $scope.path + "GetMaster",
            data: { Production: $scope.ModelNew.ProductMasterId, CostingItem: CostItemId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.length != 0) {
                $scope.ModelNew = response.data[0];
                $scope.getComponentData($scope.ModelNew.Id);
            }
            else {
                $scope.ModelNew = {
                    Id: null,
                    ProductMasterId: $scope.ModelNew.ProductMasterId,
                    CostingItemId: $scope.ModelNew.CostingItemId,
                    CostingItemName: $scope.ModelNew.CostingItemName,
                }
                $scope.ComponentNew = {
                    Id: null,
                    ItemConsumtionMasterId: null,
                    ComponentName: null,
                    NoOfParts: 1,
                    AreaType: '',
                }
                $scope.Height = [];
                $scope.Width = [];
                $scope.ComponentList = [];
                $scope.Height.push(Object.assign({}, $scope.HeightDetail));
                $scope.Width.push(Object.assign({}, $scope.WidthDetail));
            }


        });
    }

    $scope.getMData = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetData",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getMData();

    $scope.getDetails = function (obj) {
        $scope.ModelNew = Object.assign({}, obj.data);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        $scope.getComponentData($scope.ModelNew.Id);
    }

    $scope.getComponentData = function (MasterId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetComponent",
            data: { MId: MasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ComponentList = response.data;

        });
    }

    $scope.GetComponentDetails = function (obj) {
        $scope.ComponentNew = obj.data;
        $scope.getChild(obj.data.ItemConsumtionMasterId, obj.data.Id);
    }

    $scope.ChildDetails = [];
    $scope.getChild = function (MasterId, ComponentId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetChildData",
            data: { MId: MasterId, ComId: ComponentId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ChildDetails = response.data;
            $scope.Height = [];
            $scope.Width = [];
            for (var i = 0; i < $scope.ChildDetails.length; i++) {
                if ($scope.ChildDetails[i].ParameterName == "Height") {
                    $scope.Height.push($scope.ChildDetails[i]);
                }
                else {
                    $scope.Width.push($scope.ChildDetails[i]);
                }
            }

        });
    }

    $scope.getHeight = function (obj) {
        $scope.HeightDetail = obj.data;
    }
    $scope.getWidth = function (obj) {
        $scope.WidthDetail = obj.data;
    }


    //#endregion

    //#region --Costing Item--

    $scope.showCostingItemListWithOperationPopUp = function () {
        try {


            angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("show");
            $scope.AddNewCostingItem();


        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.CostingItemList = [];
    $scope.AddNewCostingItem = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: 'Costings/ItemConsumption/GetCostingItemForSelection',
                data: {},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.CostingItemList = response.data;

            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.hideCostingItemListWithOperationPopUp = function () {
        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");
    }

    $scope.setData = function (obj) {
        $scope.ModelNew.CostingItemId = obj.data.Id;
        $scope.ModelNew.CostingItemName = obj.data.UserName;
        //$scope.getData(obj.data.Id);
        angular.element(document.querySelector("#costingItemListWithOperationPopUp")).modal("hide");
    }

    //#endregion

    //#region --Submit---

    $scope.HeightDetail = {
        Id: null,
        ParameterName: 'Height',
        Parameter: null,
        Actual: null,
        Allowance: 0,
        WithAllowance: null,
        Number: 1,
        Total: null,
        tempId: null,
    }
    $scope.WidthDetail = {
        Id: null,
        ParameterName: 'Width',
        Parameter: null,
        Actual: null,
        Allowance: 0,
        WithAllowance: null,
        Number: 1,
        Total: null,
        tempId: null
    }

    $scope.Height.push(Object.assign({}, $scope.HeightDetail));
    $scope.Width.push(Object.assign({}, $scope.WidthDetail));

    $scope.Calculation = function ($data) {
        $data.WithAllowance = $data.Actual + $data.Allowance;
        $data.Total = $data.WithAllowance * $data.Number;
    }
    $scope.SubmitH = function ($data) {
        try {
            if ($data.tempId == null) {
                $data.tempId = createTempId("H");
            }
            if ($data.Parameter === null || $data.Parameter === 'undefined' || $data.Parameter === '') {
                throw "Parameter name cannot be null";
            }
            else {

                if ($data.tempId != null) {
                    for (var i = 0; i < $scope.Height.length; i++) {
                        if ($data.tempId != $scope.Height[i].tempId) {
                            var p = $scope.Height[i].Parameter;
                            if (p === $data.Parameter) {
                                throw " Parameter name can not be same!";
                            }
                        }
                    }
                }
                else {
                    for (var i = 0; i < $scope.Height.length; i++) {
                        var p = $scope.Height[i].Parameter;
                        if (p === $data.Parameter) {
                            throw " Parameter name can not be same!";
                        }
                    }
                }
            }
            if ($data.Actual === null || $data.Actual === 'undefined' || $data.Actual === '') {
                throw "Actual cannot be null";
            }
            else {
                if ($data.Actual < 0) {
                    throw "Actual data cannot be negative";
                }
                else {

                }
            }
            if ($data.Allowance === null || $data.Allowance === 'undefined' || $data.Allowance === '') {
                $data.Allowance = 0;
            }
            else {
                if ($data.Allowance < 0) {
                    throw "Allowance data cannot be negative";
                }
                else {

                }
            }
            if ($data.Number < 1) {
                throw "Number Of components cann't be less then 1";
            }

            $data.WithAllowance = $data.Actual + $data.Allowance;
            $data.Total = $data.WithAllowance * $data.Number;



            var newObj = Object.assign({}, $scope.HeightDetail);
            $scope.Height.push(newObj);


        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.SubmitW = function ($data) {
        try {
            if ($data.tempId == null) {
                $data.tempId = createTempId("W");
            }
            if ($data.Parameter === null || $data.Parameter === 'undefined' || $data.Parameter === '') {
                throw "Parameter name cannot be null";
            }
            else {

                if ($data.tempId != null) {
                    for (var i = 0; i < $scope.Width.length; i++) {
                        if ($data.tempId != $scope.Width[i].tempId) {
                            var p = $scope.Width[i].Parameter;
                            if (p === $data.Parameter) {
                                throw " Parameter name can not be same!";
                            }
                        }
                    }
                }
                else {
                    for (var i = 0; i < $scope.Width.length; i++) {
                        var p = $scope.Width[i].Parameter;
                        if (p === $data.Parameter) {
                            throw " Parameter name can not be same!";
                        }
                    }
                }
            }
            if ($data.Actual === null || $data.Actual === 'undefined' || $data.Actual === '') {
                throw "Actual cannot be null";
            }
            else {
                if ($data.Actual < 0) {
                    throw "Actual data cannot be negative";
                }
                else {

                }
            }
            if ($data.Allowance === null || $data.Allowance === 'undefined' || $data.Allowance === '') {
                $data.Allowance = 0;
            }
            else {
                if ($data.Allowance < 0) {
                    throw "Allowance data cannot be negative";
                }
                else {

                }
            }
            if ($data.Number < 1) {
                throw "Number Of components cann't be less then 1";
            }

            $data.WithAllowance = $data.Actual + $data.Allowance;
            $data.Total = $data.WithAllowance * $data.Number;



            var newObj = Object.assign({}, $scope.WidthDetail);
            $scope.Width.push(newObj);
        } catch (e) {
            ShowResult(e, 'info');
        }
    };


    function createTempId(prefix) {
        var v = new Date().getTime();
        v += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = '-';
        }
        v = prefix + v;
        return v;
    }


    $scope.Clear = function () {
        $scope.HeightDetail = {
            Id: null,
            ParameterName: 'Height',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
        $scope.WidthDetail = {
            Id: null,
            ParameterName: 'Width',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
    }
    //#endregion

    //#region - - Validation - -

    $scope.AreaTypeValidation = function () {
        if ($scope.ComponentNew.AreaType == 'Circle') {
            $scope.WidthDetail = {
                Id: null,
                ParameterName: 'Width',
                Parameter: null,
                Actual: null,
                Allowance: 0,
                WithAllowance: null,
                Number: null,
                Total: null
            }
            $scope.Width = [];
            $scope.Width.push(Object.assign({}, $scope.WidthDetail));
        }
        else {
            //$scope.Height.push(Object.assign({}, $scope.HeightDetail));
            //$scope.Width.push(Object.assign({}, $scope.WidthDetail));
        }
    }

    //#endregion

    //#region - - S A V E - -

    $scope.Save = function () {
        try {
            ValidationMaster();
            if ($scope.ModelNew.GSMValue < 0) {
                throw "GSM value cannot be negative or zero";
            }
            if ($scope.ComponentNew.NoOfParts < 1) {
                throw "No. Of Parts cannot be less than 1";
            }

            if ($scope.ComponentList.length == 0) {
                CheckField("Component Name", $scope.ComponentNew.ComponentName);
                CheckField("Area Type", $scope.ComponentNew.AreaType);
                CheckField("No. Of Parts", $scope.ComponentNew.NoOfParts);
            }

            $scope.SaveChildDataList = [];
            for (var i = 0; i < $scope.Height.length; i++) {
                if ($scope.Height[i].Parameter == null && $scope.Height[i].Actual == null) {
                    $scope.Height.splice(i, 1);
                }

            }
            if ($scope.Height.length == 0) {
                $scope.Height.push(Object.assign({}, $scope.HeightDetail));
                throw "Parameter 1 is required..";
            }
            else {
                for (var i = 0; i < $scope.Height.length; i++) {
                    $scope.SaveChildDataList.push($scope.Height[i]);
                }
            }

            for (var i = 0; i < $scope.Width.length; i++) {
                if ($scope.Width[i].Parameter == null && $scope.Width[i].Actual == null) {
                    $scope.Width.splice(i, 1);
                }
            }
            if ($scope.ComponentNew.AreaType != 'Circle') {
                if ($scope.Width.length == 0) {
                    $scope.Width.push(Object.assign({}, $scope.WidthDetail));
                    throw "Parameter 2 is required..";
                }
                else {
                    for (var i = 0; i < $scope.Width.length; i++) {
                        $scope.SaveChildDataList.push($scope.Width[i]);
                    }
                }
            }

            if ($scope.SaveChildDataList.length == 0) {
                throw "Insert Parameter Value";
            }
            $http({
                method: 'POST',
                url: $scope.path + "Save",
                data: { 'MasterData': $scope.ModelNew, 'ComponentData': $scope.ComponentNew, 'ChildData': $scope.SaveChildDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.getMData();
                    $scope.getComponentData($scope.ModelNew.Id);
                    $scope.ClearComponent();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Product Master", $scope.ModelNew.ProductMasterId);
            CheckField("Costing Item", $scope.ModelNew.CostingItemId);
            CheckField("Description", $scope.ModelNew.Description);
            CheckField("GSM Value", $scope.ModelNew.GSMValue);
            //CheckField("Component Name", $scope.ComponentNew.ComponentName);
            //CheckField("Area Type", $scope.ComponentNew.AreaType);
            //CheckField("No. Of Parts", $scope.ComponentNew.NoOfParts);
        } catch (ex) {
            throw ex;
        }
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    //#endregion

    //#region - - D e l e t e - -

    $scope.Remove = function (index) {
        var removed = $scope.Height.splice(index, 1);
        $scope.Detail = removed;
        if ($scope.Height.length == 0) {
            $scope.Height.push(Object.assign({}, $scope.HeightDetail));
        }
        //
        //$scope.Width.push(Object.assign({}, $scope.WidthDetail));
        //$scope.Detail.pop();
    }
    $scope.RemoveW = function (index) {
        var removedW = $scope.Width.splice(index, 1);
        $scope.Details = removedW;
        if ($scope.Width.length == 0) {
            $scope.Width.push(Object.assign({}, $scope.WidthDetail));
        }

        //$scope.Detail.pop();
    }

    $scope.RemoveHeight = function (obj) {

        if (baseService.isUndefinedOrNull(obj.data.Id)) {
            for (var i = 0; i < $scope.Height.length; i++) {
                if (obj.data.Parameter == $scope.Height[i].Parameter) {
                    $scope.Height.splice(i, 1);
                }
            }
        }
        else {
            for (var i = 0; i < $scope.Height.length; i++) {
                if (obj.data.Id == $scope.Height[i].Id) {
                    $scope.Height.splice(i, 1);
                }
            }
            $scope.DeleteChild(obj.data.Id);
        }
    }

    $scope.RemoveWidth = function (obj) {
        if (baseService.isUndefinedOrNull(obj.data.Id)) {
            for (var i = 0; i < $scope.Width.length; i++) {
                if (obj.data.Parameter == $scope.Width[i].Parameter) {
                    $scope.Width.splice(i, 1);
                }
            }
        }
        else {
            for (var i = 0; i < $scope.Width.length; i++) {
                if (obj.data.Id == $scope.Width[i].Id) {
                    $scope.Width.splice(i, 1);
                }
            }
            $scope.DeleteChild(obj.data.Id);
        }

    }
    $scope.DeleteChild = function (ChildId) {
        $http({
            method: 'POST',
            url: $scope.path + "DeleteChild",
            data: { Id: ChildId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.message_confirmation = null;
    $scope.RemoveComponents = function (obj) {
        $scope.ComponentId = obj.data.Id;
        if (!baseService.isUndefinedOrNull($scope.ComponentId))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmPopUpDetails')).modal('show');
    }

    $scope.DeleteComponents = function () {
        $http({
            method: 'POST',
            url: $scope.path + "DeleteComponents",
            data: { Id: $scope.ComponentId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult("Delete Parameter First..!");
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearComponent();
                $scope.getComponentData($scope.ModelNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + "Delete",
                data: { Id: $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult("Delete Component Data first..");
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getMData();
                    $scope.ClearMaster();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    //#endregion

    //#region Clear Master
    $scope.ClearMaster = function () {
        $scope.HeightDetail = {
            Id: null,
            ParameterName: 'Height',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
        $scope.WidthDetail = {
            Id: null,
            ParameterName: 'Width',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
        $scope.ModelNew = {
            Id: null,
            ProductMasterId: null,
            CostingItemId: null,
            GSMValue: null,

        }
        $scope.ChildDetails = [];
        $scope.Height = [];
        $scope.Width = [];
        $scope.ComponentList = [];
        $scope.ComponentNew = {
            Id: null,
            ItemConsumtionMasterId: null,
            ComponentName: null,
            NoOfParts: 1,
            AreaType: '',
        }
        $scope.Height.push(Object.assign({}, $scope.HeightDetail));
        $scope.Width.push(Object.assign({}, $scope.WidthDetail));
    }

    $scope.ClearComponent = function () {
        $scope.ComponentNew = {
            Id: null,
            ItemConsumtionMasterId: $scope.ModelNew.Id,
            ComponentName: null,
            NoOfParts: 1,
            AreaType: '',
        }
        $scope.HeightDetail = {
            Id: null,
            ParameterName: 'Height',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
        $scope.WidthDetail = {
            Id: null,
            ParameterName: 'Width',
            Parameter: null,
            Actual: null,
            Allowance: 0,
            WithAllowance: null,
            Number: 1,
            Total: null
        }
        $scope.Height = [];
        $scope.Width = [];
        $scope.Height.push(Object.assign({}, $scope.HeightDetail));
        $scope.Width.push(Object.assign({}, $scope.WidthDetail));

    }
    //#endregion

}

