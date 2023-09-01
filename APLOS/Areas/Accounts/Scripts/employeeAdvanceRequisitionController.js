'use strict';
employeeAdvanceRequisitionController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http","$controller"];
function employeeAdvanceRequisitionController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
   
    $scope.path = 'accounts/Advance/';
    $scope.currencyList = [];   
    $scope.Action = 'Save';
    //search
    $controller('currencyBaseController', { $scope: $scope, $http: $http });

    $scope.modelFilterByList = [
        { value: 'SystemId', name: 'SystemId ' },
        { value: 'RequisitionAddedDate', name: 'AddedDate' },
        { value: 'RequisitionRequiredDate', name: 'RequiredDate ' },
        { value: 'Amount', name: 'Amount ' },
        { value: 'Remarks', name: 'Remarks ' },
        { value: 'CheckedBy', name: 'Checked By' },
        { value: 'ApprovedBy', name: 'Approved By' },
        { value: 'AdvanceType', name: 'Advance Type' }

    ];
    $scope.EmpAdvanceReqList = [];
    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlPlantList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });
    };


   
    $scope.searchCol = "SystemId";
    $scope.searchVal = "";
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "EmployeeAdvanceRequisitionGetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.EmpAdvanceReqList = response.data;
        });
    };
    $scope.getData();

    $scope.CheckedDataList = [];
    $scope.getCheckedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeCheckedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedDataList = response.data;
        });
    };
    //$scope.getCheckedData();

    $scope.CheckedHoldDataList = [];
    $scope.getCheckedHoldData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeCheckedHoldDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedHoldDataList = response.data;
        });
    };
   // $scope.getCheckedHoldData();

    $scope.CheckedRejectDataList = [];
    $scope.getCheckedRejectData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeCheckedRejectDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckedRejectDataList = response.data;
        });
    };
    //$scope.getCheckedRejectData();

    $scope.ApprovedDataList = [];
    $scope.getApprovedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeApprovedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedDataList = response.data;
        });
    };
    //$scope.getApprovedData();

    $scope.ApprovedHoldDataList = [];
    $scope.getApprovedHoldData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeApprovedHoldDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedHoldDataList = response.data;
        });
    };
    //$scope.getApprovedHoldData();

    $scope.ApprovedRejectDataList = [];
    $scope.getApprovedRejectData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeApprovedRejectDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedRejectDataList = response.data;
        });
    };
    //$scope.getApprovedRejectData();

    $scope.checkedByList = [];
    $scope.getCboCheckedByList = function () {
        cboService.getAuthorizationConfigCbo('EmployeeAdvanceCheckedBy', function (result) {
            $scope.checkedByList = result;
            //if ($scope.checkedByList.length == 1) {
            //    $scope.model.CheckedBy = $scope.checkedByList[0].Id;
            //}
        });
    };
    $scope.getCboCheckedByList();
    $scope.PostedDataList = [];
    $scope.getPostedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeePostedDataList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.PostedDataList = response.data;
        });
    };

    $scope.modelMain = {
        SystemId: "",
        EmpSystemId: null,
        CurrencyId: null,
        RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        RequisitionRequiredDate: null,
        Amount: null,
        Remarks: null,
        CheckedBy: null,
        ApprovedBy: null,
        AdvanceType: "General"
    };
    $scope.model = Object.assign({}, $scope.modelMain);

 

    cboService.getCurrencyCboForPotal(null, function (result) {
        $scope.currencyList = result;

        $scope.model.CurrencyId = $scope.selectBaseCurrency();
    });

    //cboService.getCompanyGroupCurrencyCbo(null, function (result) {
    //    $scope.currencyList = result;
    //    $scope.detailModel.CurrencyId = $scope.selectBaseCurrency();
    //});

    

    $scope.Get = function (args) {
        $scope.model = Object.assign({}, $scope.modelMain);
        $scope.LoadData(args.data.SystemId);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    $scope.LoadData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + "Get?Id=" + Id
        }).then(function successCallback(response) {
            $scope.model = response.data.master[0];
            $scope.Action = 'Update';
        });
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField($scope.model.RequisitionRequiredDate, "Required Date");

            if (new Date($scope.model.RequisitionAddedDate) > new Date($scope.model.RequisitionRequiredDate)) {
                throw "Required Date cann't less than Entry Date.";
            }

            CheckField($scope.model.Amount, "Amount");
            CheckField($scope.model.CurrencyId, "Currency");
            CheckField($scope.model.CheckedBy, "Checked By");
            CheckField($scope.model.Remarks, "Remarks");
        } catch (e) {
            throw e;
        }
    }

    $scope.Save = function () {
        try {
            ValidationMaster();
            
            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            $scope.model.CheckedBy = CheckedBy;

            $http({
                method: 'POST',
                data: { EmpAdvanceReqList: $scope.model },
                url: $scope.path + "EmployeeAdvanceRequisitionSave"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    
                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "EmployeeAdvanceRequisitionDelete?id=" + $scope.model.SystemId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Cancel = function () {
        $scope.Action = 'Save';
        //var DropDownListObj = $("#ddlCurrencyList").data("ejDropDownList");
        //DropDownListObj.uncheckAll();
        $("#gridEmpAdvanceReqList").ejGrid("instance").refreshContent();
        $scope.modelMain = {
            SystemId: "",
            EmpSystemId: null,
            CurrencyId: null,
            RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
            RequisitionRequiredDate: null,
            Amount: null,
            Remarks: null,
            CheckedBy: null,
            ApprovedBy: null,
            AdvanceType: "General"
        };
        $scope.model = Object.assign({}, $scope.modelMain);
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.onClickPdfPrint = function (args) {

        var gridObj = $("#gridEmpAdvanceReqList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };
    $scope.PdfPrint = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "30",
            height: "20",

            click: $scope.onClickPdfPrint
        }
    }];

    $scope.onClickExcelPrint = function (args) {

        var gridObj = $("#gridEmpAdvanceReqList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };
    $scope.ExcelPrint = [{

        type: "details", buttonOptions: {
            text: "Excel",
            width: "40",
            height: "20",

            click: $scope.onClickExcelPrint
        }
    }];

    $scope.onClickApprovedPdfPrint = function (args) {

        var gridObj = $("#grid4").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };
    $scope.ApprovedPdfPrint = [{

        type: "details", buttonOptions: {
            text: "PDF",
            width: "30",
            height: "20",

            click: $scope.onClickApprovedPdfPrint
        }
    }];
}