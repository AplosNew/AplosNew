'use strict';
employeeAdvanceRequisitionApprovalController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function employeeAdvanceRequisitionApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {

    $scope.path = 'accounts/Advance/';
    $scope.currencyList = [];   
    $scope.Action = 'Update';
    //search
    $scope.modelFilterByList = [
        { value: 'SystemId', name: 'SystemId ' },
        { value: 'RequisitionAddedDate', name: 'AddedDate' },
        { value: 'RequisitionRequiredDate', name: 'RequiredDate ' },
        { value: 'Amount', name: 'Amount ' },
        { value: 'Remarks', name: 'Remarks ' },
        { value: 'CheckedBy', name: 'Checked By' },
        { value: 'ApprovedBy', name: 'Approved By' }

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
    cboService.getCompanyGroupCurrencyCbo(null,function (result) {
        $scope.currencyList = result;
    });

    $scope.searchCol = "SystemId";
    $scope.searchVal = "";

    $scope.CheckEmpAdvanceReqList = [];

    $scope.getData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeAdvanceRequisitionForArroveList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.CheckEmpAdvanceReqList = response.data;
        });
    };
    $scope.getData();

    $scope.EmpAdvanceReqApprovedList = [];
    $scope.getApprovedData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetEmployeeAdvanceRequisitionArrovedList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.EmpAdvanceReqApprovedList = response.data;
        });
    };
    $scope.getApprovedData();

    $scope.ApprovedHoldDataList = [];
    $scope.getApprovedHoldData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetApprovedByEmployeeAdvanceRequisitionHoldList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedHoldDataList = response.data;
        });
    };

    $scope.ApprovedrejectDataList = [];
    $scope.getApprovedRejectData = function () {
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetApprovedByEmployeeAdvanceRequisitionRejectList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.ApprovedrejectDataList = response.data;
        });
    };


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

    $scope.approvedByList = [];
    $scope.getCboApprovedByList = function () {
        cboService.getAuthorizationConfigCbo('EmployeeAdvanceApproveBy', function (result) {
            $scope.approvedByList = result;
        });
    };
    $scope.getCboApprovedByList();

 
    $scope.modelMain = {
        SystemId: null,
        EmpSystemId: null,
        CurrencyId: null,
        RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        RequisitionRequiredDate: null,
        Amount: null,
        Remarks: null,
        CheckedBy: null,
        ApprovedBy: null,
        EmployeeName: null,
        Department:null
    };
    $scope.model = Object.assign({}, $scope.modelMain);

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
            $scope.LoadschedulingData(Id);
        });
    };

    $scope.getAdvanceReqScheduleList = [];
    $scope.LoadschedulingData = function (Id) {
        $http({
            method: 'GET',
            url: "accounts/Advance/GetAdvanceReqScheduleListByRequisitionId?requisitionId=" + Id
        }).then(function successCallback(response) {
            $scope.getAdvanceReqScheduleList = response.data;
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
            var DropDownListObj = $("#ddlCurrencyList").data("ejDropDownList");
            var currencyId = DropDownListObj.getSelectedValue();
            $scope.model.CurrencyId = currencyId;

            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            $scope.model.CheckedBy = CheckedBy;

            $http({
                method: 'POST',
                data: { EmpAdvanceReqList: $scope.model },
                url: $scope.path + "EmployeeAdvanceRequisitionApprove"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    
                    $scope.Cancel();
                    $scope.getData();
                    $scope.getApprovedData();
                    $scope.Action = 'Update';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Reject = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.Amount)) {
                throw "Amount is required.";
            }
            if (baseService.isUndefinedOrNull($scope.model.Reason)) {
                throw "Reject Reason is required.";
            }
            var DropDownListObj = $("#ddlCurrencyList").data("ejDropDownList");
            var currencyId = DropDownListObj.getSelectedValue();
            $scope.model.CurrencyId = currencyId;

            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            $scope.model.CheckedBy = CheckedBy;

            $http({
                method: 'POST',
                data: { EmpAdvanceReqList: $scope.model },
                url: $scope.path + "EmployeeAdvanceRequisitionApprovedRejected"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Update';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Hold = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.model.Amount)) {
                throw "Amount is required.";
            }
            if (baseService.isUndefinedOrNull($scope.model.Reason)) {
                throw "Hold Reason is required.";
            }
            var DropDownListObj = $("#ddlCurrencyList").data("ejDropDownList");
            var currencyId = DropDownListObj.getSelectedValue();
            $scope.model.CurrencyId = currencyId;

            var DropDownListcheckedBy = $("#ddlcheckedByList").data("ejDropDownList");
            var CheckedBy = DropDownListcheckedBy.getSelectedValue();
            $scope.model.CheckedBy = CheckedBy;

            $http({
                method: 'POST',
                data: { EmpAdvanceReqList: $scope.model },
                url: $scope.path + "EmployeeAdvanceRequisitionApprovedHold"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');

                    $scope.Cancel();
                    $scope.getData();
                    $scope.Action = 'Update';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
 

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Cancel = function () {
        $scope.Action = 'Update';
        var DropDownListObj = $("#ddlCurrencyList").data("ejDropDownList");
        DropDownListObj.uncheckAll();
        $("#gridEmpAdvanceReqCheckList").ejGrid("instance").refreshContent();


        $scope.modelMain = {
            SystemId: null,
            EmpSystemId: null,
            CurrencyId: null,
            RequisitionAddedDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
            RequisitionRequiredDate: null,
            Amount: null,
            Remarks: null,
            CheckedBy: null,
            ApprovedBy: null
        };
        $scope.model = Object.assign({}, $scope.modelMain);
    };


    $scope.onClickReportDownloadWord = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };

    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Pdf";
        if (baseService.isUndefinedOrNull(data.SystemId)) return ShowResult('No Id found', 'failure');
        $window.open('Accounts/Advance/GetEmployeeAdvanceReportPortal?reportFormat=' + reportFormat + '&employeeAdvanceRequisitionId=' + data.SystemId, '_blank');
    };

}