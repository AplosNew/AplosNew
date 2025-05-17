'use strict';
inventoryIssueJournalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function inventoryIssueJournalController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, $window) {
    $rootScope.title = "Inventory Issue";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.dataList = [];
    $scope.path = 'Accounts/InventoryPayable/';
    $scope.getListUrl = $scope.path + 'GetIssueJournalList';
    $scope.saveUrl = 'Accounts/InvoicePost/CreateIssue';
    $scope.deleteUrl = 'Accounts/InvoicePost/DeleteIssueJournal';
    $scope.ispostDisable = false;
    $scope.searchByIssueList = [
        {
            value: 'IssueNo'
            , name: 'Issue No'
        },
        {
            value: 'VoucherNo'
            , name: 'Voucher No'
        },
        {
            value: 'MaterialStorage'
            , name: 'Storage'
        },
        {
            value: 'EmployeeName'
            , name: 'Employee'
        },
        {
            value: 'IssueDate'
            , name: 'Issue Date'
        }
        ,
        {
            value: 'OrderRefNo'
            , name: 'OrderRefNo'
        }
        ,
        {
            value: 'SourceNo'
            , name: 'SourceNo'
        }
        ,
        {
            value: 'ContractId'
            , name: 'ContractId'
        }
        ,
        {
            value: 'LCRef'
            , name: 'LC No'
        }
    ];
    $scope.parameters = {
        limit: 10,
        offset: 0,
        order: "ASC",
        sort: "VoucherNo",
        searchBy: "VoucherNo",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    baseService.init($scope.getListUrl, null, null, 'DESC', 'IssueDate', 'IssueNo');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno, $scope.parameters)
            .then(function (result) {
                $scope.dataList = result.Rows;
                for (var i = 0; i < $scope.dataList.length; i++) {
                    $scope.dataList[i].VoucherDate = new Date($scope.dataList[i].VoucherDate);
                    $scope.dataList[i].IssueDate = new Date($scope.dataList[i].IssueDate);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();




    cboService.getCboVoucherTypeIssueJournalList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    $scope.model = {
        Id: null
        , MaterialStorage: null
        , IssueDate: null
        , VoucherTypeId: null
        , OrderRefNo: null
        , Types: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.searchByPopList = [
        {
            value: 'Id'
            , name: 'Issue No'
        },
        {
            value: 'MaterialStorage'
            , name: 'Storage'
        },
        {
            value: 'EmployeeName'
            , name: 'Employee'
        },
        {
            value: 'IssueDate'
            , name: 'Issue Date'
        },
        {
            value: 'OrderRefNo'
            , name: 'OrderRef No'
        },
        {
            value: 'SourceNo'
            , name: 'Source No'
        },
        {
            value: 'ContractId'
            , name: 'Contract'
        },
        {
            value: 'LCRef'
            , name: 'LC'
        },
        {
            value: 'Customer'
            , name: 'Customer'
        }
    ];

    $scope.columnExcluedList = [];
    $scope.popUpDataList = [];
    $scope.searchByIssue = "Id"; $scope.searchIssue = "";

    $scope.popUpTitle = 'Inventory Issue Data';
    $scope.popUp = function () {
        $http({
            method: 'POST',
            url: 'Products/InventoryIssue/GetIssueList',
            data: { column: $scope.searchByIssue, value: $scope.searchIssue },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data;
        });
        angular.element(document.querySelector('#popUpId')).modal('show');
    };

    $scope.selectDoubleClick = function (data) {
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data.data;
        $scope.modelNew.IssueDate = $filter("dateFiltering")($scope.modelNew.IssueDate);
        $scope.modelNew.PostingDate = $filter("dateFiltering")($scope.modelNew.IssueDate);
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        $scope.modelNew.OrderRefNo = data.OrderRefNo;
        $scope.modelNew.Types = data.Types;
        $scope.modelNew.ContractId = data.ContractId;
        $scope.modelNew.SourceNo = data.SourceNo;
        $scope.modelNew.LCRef = data.LCRef;
        $scope.modelNew.Customer = data.Customer;
        $scope.modelNew.IsOrderSpecificy = data.IsOrderSpecificy;
        $scope.ispostDisable = false;
        //getInventoryMaterialList();
        getIssueList()
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    function getInventoryMaterialList() {
        $http.get('Products/InventoryIssue/GetIssueWithGl?issueId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                    response.data[i].budgetList = [];
                    response.data[i].activityList = [];
                    response.data[i].budgetList.push({ BudgetMasterId: response.data[i].BudgetMasterId, BudgetName: response.data[i].BudgetName });
                    response.data[i].activityList.push({ ActivityId: response.data[i].ActivityId, ActivityName: response.data[i].ActivityName });
                }
                $scope.inventoryMaterialList = response.data;
            });
    }
    $scope.validation = function () {
        //if ($scope.inventoryMaterialList.length < 1) {
        //    $scope.issueJournal();
        //}
        if ($scope.inventoryIssueList.length < 1) {
            ShowResult('Please select Issue !', 'failure');
            return true;
        }
        else if ($scope.inventoryIssueList.length) {
            for (var i = 0; i < $scope.inventoryIssueList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.inventoryIssueList[i].BudgetMasterId)) {
                    if (baseService.isUndefinedOrNull($scope.inventoryIssueList[i].ExpenseBudgetMasterId)) {
                        ShowResult('In Material Group Determinate, ' + $scope.inventoryIssueList[i].MaterialGroupMasterName + ',  Expenses GL,Budget and Activity are missing !!', 'failure');
                        return true;
                    }
                    else if (baseService.isUndefinedOrNull($scope.inventoryIssueList[i].IssueBudgetMasterId)) {
                        ShowResult('In Issue,  Budget and Activity are missing !!', 'failure');
                        return true;
                    }
                }
                else
                    return false;
            }

        }
        else
            return false;

    }
    $scope.Post = function () {
        $scope.ispostDisable = true;
        $scope.validation();
        if (!$scope.validation()) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    issueId: $scope.modelNew.Id
                    , voucherTypeId: $scope.modelNew.VoucherTypeId
                    , voucherDetailVMList: $scope.inventoryMaterialList
                    , invIssueDetailList: $scope.inventoryIssueList
                    , invIssueDetailGLList: $scope.inventoryIssueGLList
                    , InventoryReceiveDetailList: $scope.InventoryReceiveDetailList
                },
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function (response) {
                ShowResult(response.data.Message, 'failure');
            };
        }

    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.inventoryMaterialList = [];
        $scope.inventoryIssueList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
        $scope.ispostDisable = false;
    };

    //#region GL, Budget & Activity

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    //$scope.searchGLByList = [

    //    {
    //        'name': 'Account Group',
    //        'value': 'AccountGroupName'
    //    },
    //    {
    //        'name': 'GL',
    //        'value': 'GLGeneralInfoName'
    //    }
    //];
    //$scope.glListParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'asc',
    //    sort: 'GLGeneralInfoCode',
    //    searchBy: "GLGeneralInfoName",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};
    $scope.getGLPopUP = function (index) {
        $scope.index = index;
        $scope.getGLData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.companyConfig.COAId, pageno, $scope.glListParameters)
                .then(function (data) {
                    $scope.glList = data.Rows;
                    $scope.glListParameters.total_count = data.Total;
                    angular.element(document.querySelector('#gltListPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getGLData();
    };



    $scope.downPaymentBudgetList = [];
    function getBudgetList(index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.companyConfig.COAId, $scope.inventoryMaterialList[index].GLGeneralInfoId, function (result) {
            $scope.inventoryMaterialList[index].BudgetMasterId = null;
            $scope.inventoryMaterialList[index].budgetList = [];
            $scope.inventoryMaterialList[index].budgetList = result;
        });
    }

    $scope.closeGltListPopUp = function () {
        $scope.index = -1;
        angular.element(document.querySelector('#gltListPopUp')).modal('hide');
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
            "name": "Ref No",
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

    $scope.GetCOAICodeList = function (index) {
        $scope.index = index;
        $scope.GLUrl1 = "Accounts/glitem/GetIssuePostingGLBudgetActivityList";
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListData();
    };

    $scope.closeCOAICodeListPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector("#GLPopUp")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUp")).modal("show");
        }
    };

    $scope.setSelected = function (data) {
        $scope.inventoryMaterialList[$scope.index].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.inventoryMaterialList[$scope.index].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.inventoryMaterialList[$scope.index].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.inventoryMaterialList[$scope.index].BudgetMasterId = data.BudgetMasterId;
        $scope.inventoryMaterialList[$scope.index].BudgetName = data.BudgetName;
        $scope.getActivity($scope.index, data);
        $scope.inventoryMaterialList[$scope.index].ActivityId = data.ActivityId;
        $scope.closeCOAICodeListPopUp();
    };

    $scope.refreshGL = function (index) {
        $scope.inventoryMaterialList[index].GLGeneralInfoId = null;
        $scope.inventoryMaterialList[index].GLName = null;
        $scope.inventoryMaterialList[index].BudgetMasterId = null;
        $scope.inventoryMaterialList[index].ActivityId = null;
        $scope.inventoryMaterialList[index].budgetList = null;
        $scope.inventoryMaterialList[index].activityList = null;
    };
    $scope.activityList = [];
    $scope.getActivity = function (index, data) {
        cboService.getBudgetMasterActivityCbo($scope.inventoryMaterialList[index].BudgetMasterId, function (result) {
            $scope.inventoryMaterialList[index].ActivityId = null;
            $scope.inventoryMaterialList[index].activityList = [];
            $scope.inventoryMaterialList[index].activityList = result;
            $scope.inventoryMaterialList[index].ActivityId = data.ActivityId;

        });
    };
    $scope.inventoryIssueList = [];
    $scope.inventoryIssueGLList = [];
    $scope.InventoryReceiveDetailList = [];
    function getIssueList() {
        $http.get('Products/InventoryIssue/GetInventoryMaterialIssueList?issueId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryIssueList = response.data.Rows;
                getIssueGLList($scope.modelNew.Id);
                getInventoryReceiveDetailListByIssueId($scope.modelNew.Id);
            });
    }
    function getIssueGLList(id) {
        $http.get('Accounts/InventoryPayable/GetInventoryMaterialIssueGLList?issueId=' + id)
            .then(function (response) {
                $scope.inventoryIssueGLList = response.data.Rows;
                $scope.issueJournal();
            });
    }
    function getInventoryReceiveDetailListByIssueId(id) {
        $http.get('Accounts/InventoryPayable/GetInventoryReceiveDetailListByIssueId?issueId=' + id)
            .then(function (response) {
                $scope.InventoryReceiveDetailList = response.data.Rows;
            });
    }

    $scope.inventoryMaterialList = [];
    $scope.issueJournal = function () {
        $scope.inventoryMaterialList = [];
        $scope.invGL = {};
        $scope.invGLDr = {};
        for (var d = 0; d < $scope.inventoryIssueGLList.length; d++) {
            if (baseService.isUndefinedOrNull($scope.modelNew.OrderRefNo)) {
                var getRowDr = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueGLList[d].BudgetMasterId, "ActivityId": $scope.inventoryIssueGLList[d].ActivityId });
                if (getRowDr.length == 0 && $scope.inventoryIssueGLList[d].BudgetMasterId != null) {
                    $scope.invGLDr.GLGeneralInfoId = $scope.inventoryIssueGLList[d].GLGeneralInfoId;
                    $scope.invGLDr.GLGeneralInfoCode = $scope.inventoryIssueGLList[d].GLGeneralInfoCode;
                    $scope.invGLDr.GLGeneralInfoName = $scope.inventoryIssueGLList[d].GLGeneralInfoName;
                    $scope.invGLDr.BudgetMasterId = $scope.inventoryIssueGLList[d].BudgetMasterId;
                    $scope.invGLDr.BudgetName = $scope.inventoryIssueGLList[d].BudgetName;
                    $scope.invGLDr.ActivityId = $scope.inventoryIssueGLList[d].ActivityId;
                    $scope.invGLDr.ActivityName = $scope.inventoryIssueGLList[d].ActivityName;
                    $scope.invGLDr.CostCenterId = $scope.inventoryIssueGLList[d].CostCenterId;
                    $scope.invGLDr.TrnType = "Dr";
                    $scope.invGLDr.Dr = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Amount = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Cr = 0;
                    $scope.inventoryMaterialList.push($scope.invGLDr);
                    $scope.invGLDr = {};
                }
                else if ($scope.inventoryIssueGLList[d].BudgetMasterId != null) {
                    for (var k = 0; k < $scope.inventoryMaterialList.length; k++) {
                        if (getRowDr[0].BudgetMasterId == $scope.inventoryMaterialList[k].BudgetMasterId
                            && getRowDr[0].ActivityId == $scope.inventoryMaterialList[k].ActivityId) {


                            var dr = parseFloat($scope.inventoryMaterialList[k].Dr.toFixed(4)) + parseFloat($scope.inventoryIssueGLList[d].TrnAmount.toFixed(4));
                            $scope.inventoryMaterialList[k].Dr = parseFloat(dr.toFixed(4));
                            $scope.inventoryMaterialList[k].Amount = parseFloat(dr.toFixed(4));
                            dr = 0;
                        }
                    }
                }
            }
            else if (!baseService.isUndefinedOrNull($scope.modelNew.OrderRefNo) && $scope.modelNew.Types == 'InventoryJWIssue') {
                var getRowDr = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueGLList[d].JWBudgetMasterId, "ActivityId": $scope.inventoryIssueGLList[d].JWActivityId });
                if (getRowDr.length == 0 && $scope.inventoryIssueGLList[d].JWBudgetMasterId != null) {
                    $scope.invGLDr.GLGeneralInfoId = $scope.inventoryIssueGLList[d].JWGLGeneralInfoId;
                    $scope.invGLDr.GLGeneralInfoCode = $scope.inventoryIssueGLList[d].JWGLGeneralInfoCode;
                    $scope.invGLDr.GLGeneralInfoName = $scope.inventoryIssueGLList[d].JWGLGeneralInfoName;
                    $scope.invGLDr.BudgetMasterId = $scope.inventoryIssueGLList[d].JWBudgetMasterId;
                    $scope.invGLDr.BudgetName = $scope.inventoryIssueGLList[d].JWBudgetName;
                    $scope.invGLDr.ActivityId = $scope.inventoryIssueGLList[d].JWActivityId;
                    $scope.invGLDr.ActivityName = $scope.inventoryIssueGLList[d].JWActivityName;
                    $scope.invGLDr.TrnType = "Dr";
                    $scope.invGLDr.Dr = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Amount = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Cr = 0;
                    $scope.inventoryMaterialList.push($scope.invGLDr);
                    $scope.invGLDr = {};
                }
                else if ($scope.inventoryIssueGLList[d].BudgetMasterId != null) {
                    for (var k = 0; k < $scope.inventoryMaterialList.length; k++) {
                        if (getRowDr[0].BudgetMasterId == $scope.inventoryMaterialList[k].BudgetMasterId
                            && getRowDr[0].ActivityId == $scope.inventoryMaterialList[k].ActivityId) {


                            var dr = parseFloat($scope.inventoryMaterialList[k].Dr.toFixed(4)) + parseFloat($scope.inventoryIssueGLList[d].TrnAmount.toFixed(4));
                            $scope.inventoryMaterialList[k].Dr = parseFloat(dr.toFixed(4));
                            $scope.inventoryMaterialList[k].Amount = parseFloat(dr.toFixed(4));
                            dr = 0;
                        }
                    }
                }
            }
            else {
                var getRowDr = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueGLList[d].WIPBudgetMasterId, "ActivityId": $scope.inventoryIssueGLList[d].ActivityId });
                if (getRowDr.length == 0 && $scope.inventoryIssueGLList[d].BudgetMasterId != null) {
                    $scope.invGLDr.GLGeneralInfoId = $scope.inventoryIssueGLList[d].WIPGLGeneralInfoId;
                    $scope.invGLDr.GLGeneralInfoCode = $scope.inventoryIssueGLList[d].WIPGLGeneralInfoCode;
                    $scope.invGLDr.GLGeneralInfoName = $scope.inventoryIssueGLList[d].WIPGLName;
                    $scope.invGLDr.BudgetMasterId = $scope.inventoryIssueGLList[d].WIPBudgetMasterId;
                    $scope.invGLDr.BudgetName = $scope.inventoryIssueGLList[d].WIPBudgetName;
                    $scope.invGLDr.ActivityId = $scope.inventoryIssueGLList[d].WIPActivityId;
                    $scope.invGLDr.ActivityName = $scope.inventoryIssueGLList[d].WIPActivityName;
                    $scope.invGLDr.TrnType = "Dr";
                    $scope.invGLDr.Dr = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Amount = $scope.inventoryIssueGLList[d].TrnAmount;
                    $scope.invGLDr.Cr = 0;
                    $scope.inventoryMaterialList.push($scope.invGLDr);
                    $scope.invGLDr = {};
                }

                else if ($scope.inventoryIssueGLList[d].BudgetMasterId != null) {
                    for (var k = 0; k < $scope.inventoryMaterialList.length; k++) {
                        if (getRowDr[0].BudgetMasterId == $scope.inventoryMaterialList[k].BudgetMasterId
                            && getRowDr[0].ActivityId == $scope.inventoryMaterialList[k].ActivityId) {


                            var dr = parseFloat($scope.inventoryMaterialList[k].Dr.toFixed(4)) + parseFloat($scope.inventoryIssueGLList[d].TrnAmount.toFixed(4));
                            $scope.inventoryMaterialList[k].Dr = parseFloat(dr.toFixed(4));
                            $scope.inventoryMaterialList[k].Amount = parseFloat(dr.toFixed(4));
                            dr = 0;
                        }
                    }
                }
            }
        }
        for (var i = 0; i < $scope.inventoryIssueGLList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.modelNew.OrderRefNo) && $scope.modelNew.Types == 'InventoryJWIssue') {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueGLList[i].PostDrBudgetMasterId, "ActivityId": $scope.inventoryIssueGLList[i].PostDrActivityId });
                if (getRow.length == 0 && $scope.inventoryIssueGLList[i].PostDrBudgetMasterId != null) {
                    $scope.invGL.GLGeneralInfoId = $scope.inventoryIssueGLList[i].PostDrGLGeneralInfoId;
                    $scope.invGL.GLGeneralInfoCode = $scope.inventoryIssueGLList[i].GAccountCode;
                    $scope.invGL.GLGeneralInfoName = $scope.inventoryIssueGLList[i].GUserName;
                    $scope.invGL.BudgetMasterId = $scope.inventoryIssueGLList[i].PostDrBudgetMasterId;
                    $scope.invGL.BudgetName = $scope.inventoryIssueGLList[i].BUserName;
                    $scope.invGL.ActivityId = $scope.inventoryIssueGLList[i].PostDrActivityId;
                    $scope.invGL.ActivityName = $scope.inventoryIssueGLList[i].AUserName;
                    $scope.invGL.TrnType = "Cr";
                    $scope.invGL.Cr = parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                    $scope.invGL.Amount = parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                    $scope.invGL.Dr = 0;
                    $scope.inventoryMaterialList.push($scope.invGL);
                    $scope.invGL = {};

                }
                else if ($scope.inventoryIssueGLList[i].PostDrBudgetMasterId != null) {
                    for (var j = 0; j < $scope.inventoryMaterialList.length; j++) {
                        if (getRow[0].BudgetMasterId == $scope.inventoryMaterialList[j].BudgetMasterId
                            && getRow[0].ActivityId == $scope.inventoryMaterialList[j].ActivityId) {
                            var cr = parseFloat($scope.inventoryMaterialList[j].Cr.toFixed(4)) + parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                            $scope.inventoryMaterialList[j].Cr = parseFloat(cr.toFixed(4));
                            $scope.inventoryMaterialList[j].Amount = parseFloat(cr.toFixed(4));
                            cr = 0;
                        }
                    }
                }

            }
            else {
                var getRow = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueGLList[i].PostDrBudgetMasterId, "ActivityId": $scope.inventoryIssueGLList[i].PostDrActivityId });
                if (getRow.length == 0 && $scope.inventoryIssueGLList[i].PostDrBudgetMasterId != null) {
                    $scope.invGL.GLGeneralInfoId = $scope.inventoryIssueGLList[i].PostDrGLGeneralInfoId;
                    $scope.invGL.GLGeneralInfoCode = $scope.inventoryIssueGLList[i].GAccountCode;
                    $scope.invGL.GLGeneralInfoName = $scope.inventoryIssueGLList[i].GUserName;
                    $scope.invGL.BudgetMasterId = $scope.inventoryIssueGLList[i].PostDrBudgetMasterId;
                    $scope.invGL.BudgetName = $scope.inventoryIssueGLList[i].BUserName;
                    $scope.invGL.ActivityId = $scope.inventoryIssueGLList[i].PostDrActivityId;
                    $scope.invGL.ActivityName = $scope.inventoryIssueGLList[i].AUserName;

                    $scope.invGL.TrnType = "Cr";
                    $scope.invGL.Cr = parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                    $scope.invGL.Amount = parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                    $scope.invGL.Dr = 0;
                    $scope.inventoryMaterialList.push($scope.invGL);
                    $scope.invGL = {};

                }
                else if ($scope.inventoryIssueGLList[i].PostDrBudgetMasterId != null) {
                    for (var m = 0; m < $scope.inventoryMaterialList.length; m++) {
                        if (getRow[0].BudgetMasterId == $scope.inventoryMaterialList[m].BudgetMasterId
                            && getRow[0].ActivityId == $scope.inventoryMaterialList[m].ActivityId) {
                            var cr = parseFloat($scope.inventoryMaterialList[m].Cr.toFixed(4)) + parseFloat($scope.inventoryIssueGLList[i].TrnAmount.toFixed(4));
                            $scope.inventoryMaterialList[m].Cr = parseFloat(cr.toFixed(4));
                            $scope.inventoryMaterialList[m].Amount = parseFloat(cr.toFixed(4));
                            cr = 0;
                        }
                    }
                }

            }
        }
    }
    $scope.EditableissueJournal = function () {
        var drc = $scope.inventoryMaterialList.length;
        while (drc--) {
            if ($scope.inventoryMaterialList[drc]['TrnType'] === "Dr") {
                $scope.inventoryMaterialList.splice(drc, 1);
            }
        }

        $scope.invGL = {};
        $scope.invGLDr = {};
        for (var j = 0; j < $scope.inventoryIssueList.length; j++) {
            var getRowDr1 = $filter("filter")($scope.inventoryMaterialList, { "BudgetMasterId": $scope.inventoryIssueList[j].BudgetMasterId, "ActivityId": $scope.inventoryIssueList[j].ActivityId });
            if (getRowDr1.length == 0 && $scope.inventoryIssueList[j].BudgetMasterId != null) {
                $scope.invGLDr.GLGeneralInfoId = $scope.inventoryIssueList[j].GLGeneralInfoId;
                $scope.invGLDr.GLGeneralInfoCode = $scope.inventoryIssueList[j].GLGeneralInfoCode;
                $scope.invGLDr.GLGeneralInfoName = $scope.inventoryIssueList[j].GLGeneralInfoName;
                $scope.invGLDr.BudgetMasterId = $scope.inventoryIssueList[j].BudgetMasterId;
                $scope.invGLDr.BudgetName = $scope.inventoryIssueList[j].BudgetName;
                $scope.invGLDr.ActivityId = $scope.inventoryIssueList[j].ActivityId;
                $scope.invGLDr.ActivityName = $scope.inventoryIssueList[j].ActivityName;
                $scope.invGLDr.TrnType = "Dr";
                $scope.invGLDr.Dr = $scope.inventoryIssueList[j].TrnAmount;
                $scope.invGLDr.Amount = $scope.inventoryIssueList[j].TrnAmount;
                $scope.invGLDr.Cr = 0;
                $scope.inventoryMaterialList.push($scope.invGLDr);
                $scope.invGLDr = {};
            }
            else if ($scope.inventoryIssueList[j].BudgetMasterId != null) {
                for (var k = 0; k < $scope.inventoryMaterialList.length; k++) {
                    if (getRowDr1[0].BudgetMasterId == $scope.inventoryMaterialList[k].BudgetMasterId
                        && getRowDr1[0].ActivityId == $scope.inventoryMaterialList[k].ActivityId) {


                        var dr = parseFloat($scope.inventoryMaterialList[k].Dr.toFixed(4)) + parseFloat($scope.inventoryIssueList[j].TrnAmount.toFixed(4));
                        $scope.inventoryMaterialList[k].Dr = parseFloat(dr.toFixed(4));
                        $scope.inventoryMaterialList[k].Amount = parseFloat(dr.toFixed(4));
                        dr = 0;
                    }
                }
            }
        }
    }
    $scope.searchglByListMB = [
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
            "name": "Ref No",
            "value": "RefNo"
        }
    ];

    $scope.glListParametersMB = {
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

    $scope.GetCOAICodeListMB = function (index, data) {
        $scope.indexMB = index;
        $scope.TempData = {};
        $scope.TempData.bMasterId = data.BudgetMasterId;
        $scope.TempData.AId = data.ActivityId;
        $scope.TempData.Amount = data.TrnAmount;
        $scope.GLUrl1MB = "Accounts/glitem/GetIssuePostingGLBudgetActivityList";
        $scope.GetCOAICodeListDataMB = function (pageno) {
            baseService.paginationBase($scope.GLUrl1MB, pageno, $scope.glListParametersMB)
                .then(function (result) {
                    $scope.cOAICodeListMB = result.Rows;
                    $scope.glListParametersMB.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUpMB")).modal("show");
        $scope.modalShow = true;
        $scope.GetCOAICodeListDataMB();
    };

    $scope.closeCOAICodeListPopUpMB = function () {
        angular.element(document.querySelector("#GLPopUpMB")).modal("hide");
    };

    $scope.closeCOAICodeListPopUpSelectedMB = function () {
        if ($scope.rowSelectedMB !== null) {
            angular.element(document.querySelector("#GLPopUpMB")).modal("hide");
        } else {
            angular.element(document.querySelector("#cancelPopUpMB")).modal("show");
        }
    };

    $scope.searchissueAUCglByList = [
        {
            "name": "Fixed Asset",
            "value": "FixedAssetName"
        },
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

    $scope.issueAUCglListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "FixedAssetName",
        searchBy: "FixedAssetName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.issueAUCglList = [];
    $scope.GetIssueAUCList = function (index, data) {
        $scope.indexMB = index;
        $scope.TempData = {};
        $scope.TempData.bMasterId = data.BudgetMasterId;
        $scope.TempData.AId = data.ActivityId;
        $scope.TempData.Amount = data.TrnAmount;
        $scope.IssueAUCGLUrl = "Accounts/glitem/GetIssueAUCGLBudgetActivity";
        baseService.setCurrentPage('issueAUCglList');
        $scope.GetIssueAUCGLData = function (pageno) {

            baseService.paginationBase($scope.IssueAUCGLUrl, pageno, $scope.issueAUCglListParameters)
                .then(function (result) {
                    $scope.issueAUCglList = result.Rows;
                    $scope.issueAUCglListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("show");
        $scope.modalShow = true;
        $scope.GetIssueAUCGLData();
    };

    $scope.closeIssueAUCglListPopUp = function () {
        angular.element(document.querySelector("#IssueAUCGLPopUp")).modal("hide");
    };
    $scope.setissueAUCglSelected = function (data) {
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.inventoryIssueList[$scope.indexMB].BudgetMasterId = data.BudgetMasterId;
        $scope.inventoryIssueList[$scope.indexMB].BudgetName = data.BudgetName;
        $scope.inventoryIssueList[$scope.indexMB].ActivityId = data.ActivityId;
        $scope.inventoryIssueList[$scope.indexMB].ActivityName = data.ActivityName;
        $scope.EditableissueJournal();
        $scope.closeIssueAUCglListPopUp();
        //$scope.issueJournalNewBudjetAdd($scope.inventoryIssueList[$scope.indexMB]);
    };
    $scope.refreshGLMB = function (index) {
        $scope.inventoryIssueList[index].GLGeneralInfoId = null;
        $scope.inventoryIssueList[index].GLName = null;
        $scope.inventoryIssueList[index].BudgetMasterId = null;
        $scope.inventoryIssueList[index].ActivityId = null;
        $scope.inventoryIssueList[index].budgetList = null;
        $scope.inventoryIssueList[index].activityList = null;
    };



    $scope.setSelectedMB = function (data) {
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoCode = data.GLGeneralInfoCode;
        $scope.inventoryIssueList[$scope.indexMB].GLGeneralInfoName = data.GLGeneralInfoName;
        $scope.inventoryIssueList[$scope.indexMB].BudgetMasterId = data.BudgetMasterId;
        $scope.inventoryIssueList[$scope.indexMB].BudgetName = data.BudgetName;
        $scope.inventoryIssueList[$scope.indexMB].ActivityId = data.ActivityId;
        $scope.inventoryIssueList[$scope.indexMB].ActivityName = data.ActivityName;
        $scope.closeCOAICodeListPopUpMB();
        $scope.EditableissueJournal();
        //$scope.issueJournalNewBudjetAdd($scope.inventoryIssueList[$scope.indexMB]);
    };
    $scope.refreshGLMB = function (index) {
        $scope.inventoryIssueList[index].GLGeneralInfoId = null;
        $scope.inventoryIssueList[index].GLName = null;
        $scope.inventoryIssueList[index].BudgetMasterId = null;
        $scope.inventoryIssueList[index].ActivityId = null;
        $scope.inventoryIssueList[index].budgetList = null;
        $scope.inventoryIssueList[index].activityList = null;
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.delete = function (issueId, voucherId) {
        $http({
            method: "POST",
            url: $scope.deleteUrl,
            data: {
                "issueId": issueId, "voucherId": voucherId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                $scope.Clear();
                $scope.issueId = null;
                $scope.voucherId = null;
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };

    $scope.issueId = null;
    $scope.confirmDelete = function (issueId, voucherId) {
        $scope.issueId = issueId;
        $scope.voucherId = voucherId;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector("#confirmDeletePopUp")).modal("show");
    };

    $scope.downloadIssue = function () {

        location.href = "Products/InventoryIssue/IssueReport?grnId=" + $scope.modelNew.Id;
    };
    $scope.downloadIssueExcel = function () {
        var reportFormat = "Excel";
        location.href = "Outsourcing/OSIssueReturn/GetIIPrintReport?reportFormat=" + reportFormat + '&IssueId=' + $scope.modelNew.Id;
    };

    $scope.issueReportDownload = function (x) {
        if (x.Types)
            $window.open("Products/InventoryIssue/JWValAddedIssueReport?grnId=" + x.IssueNo);// href = target = "_blank"
        else
            $window.open("Products/InventoryIssue/IssueReport?grnId=" + x.IssueNo);// href = target = "_blank"

    };

}