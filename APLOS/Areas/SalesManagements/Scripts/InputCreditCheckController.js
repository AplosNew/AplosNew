'use strict';
InputCreditCheckController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function InputCreditCheckController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Input Credit Check';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/Sales/';
    $scope.getListUrl = $scope.path + 'getinputcreditlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateInputCredit';
    $scope.deleteUrl = $scope.path + 'deleteinputcredit/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.POApprovalList = [
        {
            'Text': 'Checked',
            'Value': 'Checked'
        }
    ];

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInputCreditCheckList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
           
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        UserRef: null,
        MonthNo: null,
        FromDate: null,
        ToDate: null,
        ResponsiblePersonId: null,
        CheckById: null,
        CheckByStatus: 'To Be Checked',
        ApproveById: null,
        ApprovedStatus: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

  
    $scope.ApproveByList = [];
    $scope.GetApprovedByCbo = function () {
        $http({
            method: 'GET',
            url: 'SalesManagements/Sales/GetApprovedByCbo'
        }).then(function successCallback(response) {
            $scope.ApproveByList = response.data;
        });
    }
    $scope.GetApprovedByCbo();

    $scope.detailTemp = "#tabGridContents";
    $scope.detailgrid = function detailGridData(e) {

        var filteredData = e.data["Id"];

        $http({
            method: 'GET',
            url: 'SalesManagements/Sales/GetTaggedSalesMaterialDataList?inputCreditId=' + filteredData
        }).then(function successCallback(response) {
            $scope.InvoiceNoList = response.data;

            var data = ej.DataManager($scope.InvoiceNoList).executeLocal(ej.Query().where("InputCreditId", "equal", parseInt(filteredData), true).take(100));

            e.detailsElement.find("#detailGrid").ejGrid({

                dataSource: data,
                columns: [
                    { field: "Id", headerText: "Id", width: 50 },
                    { field: "MasterOrderId", headerText: "MasterOrderId", width: 100 },
                    { field: "SONo", headerText: "SONo", width: 50 },
                    { field: "PONumber", headerText: "PONumber", width: 50 },
                    { field: "DeliveryDate", headerText: "Delivery Date", width: 100 },
                    { field: "DestinationName", headerText: "Destination Name", width: 100 },
                    { field: "MaterialMasterName", headerText: "Material", width: 100 },
                    { field: "MaterialMasterArticleName", headerText: "Article", width: 100 },
                    { field: "HSNCode", headerText: "HSNCode", width: 50 },
                    { field: "SKU1", headerText: "SKU1", width: 40 },
                    { field: "SKU2", headerText: "SKU2", width: 40 },
                    { field: "TransactionRate", headerText: "Rate", width: 50 },
                    { field: "TransactionQty", headerText: "Qty", width: 50 },
                    { field: "TransactionAmount", headerText: "Amount", width: 50 },
                    { field: "TaxAmount", headerText: "Tax Amount", width: 50 },
                    { field: "ServiceCharge", headerText: "Service Charge", width: 50 },
                    { field: "ServiceTax", headerText: "Service Tax", width: 50 }

                ]
            });
            e.detailsElement.find(".tabcontrol").ejTab();
        });


    }



    $scope.btndisable = false;
    $scope.SaveCheckData = function (args) {
        try {
            if (baseService.isUndefinedOrNull(args.data.ApproveById)) {
                throw "Select Approve By Person.";
            }
            if (args.data.CheckByStatus =='To Be Checked') {
                throw "Select Checked Status.";
            }
            $scope.btndisable = true;
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/CreateCheckBy',
                data: { 'data': args.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btndisable = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.btndisable = false;
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

  
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.materialList = [];
    }
}