'use strict';
SalesChalanController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalesChalanController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sales Chalan';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/SalesChalan/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchBy = "UserRef"; $scope.search = "";
    $scope.searchByList = [{ value: 'UserRef', name: "UserRef" }, { value: 'VechileNo', name: "VechileNo" }, { value: 'ByWhom', name: "ByWhom" }, { value: 'MobileNo', name: "MobileNo" }];

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
        Id: null,
        VechileNo: null,
        ByWhomId: null,
        MobileNo: null,
        SecurityInChargeId: null,
        ResponsiblePersonId: null,
        CheckById: null,
        ApproveById: null,
        UserRef: null,
        Destination: null,
        FromDate: null,
        ToDate: null,
        Remark: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.FromDate = $filter("dateFiltering")($scope.ModelNew.FromDate);
        $scope.ModelNew.ToDate = $filter("dateFiltering")($scope.ModelNew.ToDate);
        $scope.GetInvoiceDataByChalan();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GetInvoiceDataByChalan = function () {
        $http({
            method: 'GET',
            url: 'SalesManagements/SalesChalan/GetInvoiceDataByChalan?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.InvoiceNoList = response.data;
        });
    }


    $scope.popUpDataList = [];
    $scope.name = null;
    $scope.popUp = function (name) {
        try {
            $scope.name = name;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        if ($scope.name == 'ByWhom') {
            $scope.ModelNew.ByWhomId = ob.SystemId;
            $scope.ModelNew.ByWhom = ob.EmployeeName;
        } else if ($scope.name == 'SecurityInCharge') {
            $scope.ModelNew.SecurityInChargeId = ob.SystemId;
            $scope.ModelNew.SecurityInCharge = ob.EmployeeName;
        } else if ($scope.name == 'CheckBy') {
            $scope.ModelNew.CheckById = ob.SystemId;
            $scope.ModelNew.CheckBy = ob.EmployeeName;
        }
        else if ($scope.name == 'ApproveBy') {
            $scope.ModelNew.ApproveById = ob.SystemId;
            $scope.ModelNew.ApproveBy = ob.EmployeeName;
        }
        else {
            $scope.ModelNew.ResponsiblePersonId = ob.SystemId;
            $scope.ModelNew.ResponsiblePerson = ob.EmployeeName;
        }
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.clearEmpPop = function () {
        if ($scope.name == 'ByWhom') {
            $scope.ModelNew.ByWhomId = null;
            $scope.ModelNew.ByWhom = null;
        } else if ($scope.name == 'SecurityInCharge') {
            $scope.ModelNew.SecurityInChargeId = null;
            $scope.ModelNew.SecurityInCharge = null;
        }
        else if ($scope.name == 'CheckBy') {
            $scope.ModelNew.CheckById = null;
            $scope.ModelNew.CheckBy = null;
        }
        else if ($scope.name == 'ApproveBy') {
            $scope.ModelNew.ApproveById = null;
            $scope.ModelNew.ApproveBy = null;
        }
        else {
            $scope.ModelNew.ResponsiblePersonId = null;
            $scope.ModelNew.ResponsiblePerson = null;
        }
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.searchdata = [];
    $scope.GetInvoiceData = function () {
        $scope.searchdata = [];
        $http({
            method: 'GET',
            url: 'SalesManagements/SalesChalan/GetInvoiceData?fromDate=' + $scope.ModelNew.FromDate + '&toDate=' + $scope.ModelNew.ToDate
        }).then(function successCallback(response) {
            $scope.searchdata = response.data;
            $scope.ShowResultCustom();
        });
    }
  

    $scope.ShowResultCustom = function (message, type) {
        $("#InvoicePoUp").ejDialog("setTitle", "Invoice Info");
        var eDialog = $("#InvoicePoUp").data("ejDialog");
        eDialog.open();
        var gridObj = $("#GridInvoice").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridInvoice").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.searchdata.length; i++) {
                $scope.searchdata[i].Checked = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Checked = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridInvoice").data("ejGrid");
        gridObj.refreshContent();

    };
    $scope.InvoiceNoList = [];
    function MakeData() {
        for (var i = 0; i < $scope.searchdata.length; i++) {
            if ($scope.searchdata[i].Checked == true) {
                if (checkExists($scope.InvoiceNoList, $scope.searchdata[i].InvoiceId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.InvoiceId = $scope.searchdata[i].InvoiceId;
                    ob.Customer = $scope.searchdata[i].Customer;
                    ob.Date = $scope.searchdata[i].Date;
                    ob.NoOfPackage = $scope.searchdata[i].NoOfPackage;
                    ob.NetWeight = $scope.searchdata[i].NetWeight;
                    ob.GrossWeight = $scope.searchdata[i].GrossWeight;
                    ob.Destination = $scope.searchdata[i].Destination;
                    ob.Remark = $scope.searchdata[i].Remark;

                    $scope.InvoiceNoList.push(ob);
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InvoiceId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseInvoice = function () {
        try {
            MakeData();
            var eDialog = $("#InvoicePoUp").data("ejDialog");
            eDialog.close();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'details': $scope.InvoiceNoList },
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
        $scope.InvoiceNoList = [];
    }
}