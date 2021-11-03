'use strict';
commitmentController.$inject = ['cboService', '$window', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function commitmentController(cboService, $window, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Commitment";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.message = null;
    $scope.commitments = [];
    $scope.monthList = [];
    $scope.path = 'OrderManagements/commitment/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    function getDataByEntity() {
        $scope.searchList = [];
        $scope.excludeList = ['Process Name', 'SubProcess Name'];
        baseService.init($scope.getListUrl, null, null, null, 'BuyerMaster', 'BuyerMaster');
        baseService.setCurrentPage('commitments');
        $scope.getData = function (pageno) {
            $rootScope.parameters.entityId = $scope.commitmentNew.EntityId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.commitments = result.Rows;
                    $scope.commitmentNew = { EntityId: $scope.commitmentNew.EntityId, Active: $scope.commitmentNew.Active };
                    if (baseService.arrayLength($scope.searchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }
    $scope.commitment = {
        Id: null
        , EntityId: null
        , BuyerId: null
        , BuyerMasterId: null
        , BuyerBrandId: null
        , Buyer: null
        , BuyerProgramId: null
        , MaterialMasterId: null
        , FinishedGoods: null
        , MaterialGroup: null
        , BaseUoM: null
        , ProductMaster: null
        , SeasonId: null
        , ProcessId: null
        , SubProcessId: null
        , CurrencyId: null
        , SalesGroupId: null
        , NumberOfLineDays: null
        , UoMId: null
        , FOB: 0
        , CM: 0
        , SPT: 0
        , Efficiency: 0
        , Target: 0
        , LSD: null
        , ClosingDate: null
        , Remarks: null
        , Year: null
    };
    $scope.commitmentNew = Object.assign({}, $scope.commitment);


$scope.onChangeEntity = function () {
    $scope.salesGroupList = [];
    $scope.commitments = [];
    if (baseService.isUndefinedOrNull($scope.commitmentNew.EntityId))
        return $scope.commitmentNew.SalesGroupId = null;
    getDataByEntity();
    getSalesGroupCbo();
};
// #region DDL
$scope.entityList = [];
//cboService.getCboProductionEntityByCompany(null, $window.companyId, function (result) {
//    $scope.entityList = result;
//});
$scope.getAllEntities = function () {
    $http({
        method: 'POST',
        url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
    }).then(function successCallback(response) {
        $scope.entityList = response.data;
    });
}
$scope.getAllEntities();

$scope.buyerList = [];
cboService.getCboBuyer(function (result) {
    $scope.buyerList = result;
});
$scope.seasonList = [];
cboService.getCboSeasons(function (result) {
    $scope.seasonList = result;
});
//$scope.ourStyleList = [];
//$scope.getOurStyle = function () {
//    cboService.getBuyerStyleCboByBuyer($scope.commitmentNew.BuyerId, function (result) {
//        $scope.ourStyleList = result;
//    });
//};
$scope.buyerProgramList = [];
$scope.getCboBuyerProgram = function () {
    cboService.getCboBuyerProgram($scope.commitmentNew.BuyerId, function (result) {
        $scope.buyerProgramList = result;
    });
};
$scope.yearList = [];
$scope.getYearOfHaving = function () {
    $scope.yearList = [];
    var endYear = new Date();
    var ey = parseInt(endYear.getFullYear());
    for (var i = ey; i <= 2099; i++) {
        var ob = {
            Value: i,
            Text: i
        };
        $scope.yearList.push(ob);
    }
};
$scope.getYearOfHaving();
var month = new Array();
month[0] = "Jan";
month[1] = "Feb";
month[2] = "Mar";
month[3] = "Apr";
month[4] = "May";
month[5] = "Jun";
month[6] = "Jul";
month[7] = "Aug";
month[8] = "Sep";
month[9] = "Oct";
month[10] = "Nov";
month[11] = "Dec";
$scope.getCmMonth = function () {
    if ($scope.commitmentNew.LSD !== null)

        $scope.monthList = [];
    var endMonth = new Date($scope.commitmentNew.LSD);
    for (var i = 1; i <= 12; i++) {
        var newDate = new Date(endMonth.getFullYear(), endMonth.getMonth() - 1 + i, i);
        var cmonth = newDate.getMonth();
        var cyear = newDate.getFullYear();
        var ob = {
            Id: null
            , CommitmentId: $scope.commitmentNew.Id
            , CMonth: cmonth
            , CYear: cyear
            , MonthYear: month[cmonth] + '-' + cyear
            , Qty: null
        };
        $scope.monthList.push(ob);
    }
};
cboService.getCboTransactionCurrencyByCompany('', function (result) {
    $scope.cmcurrencyList = [];
    $scope.cmcurrencyList = result;
});

function getSalesGroupCbo() {
    $http.get($scope.path + 'GetSalesGroupCbo?entityId=' + $scope.commitmentNew.EntityId)
        .then(function (response) {
            $scope.salesGroupList = response.data;
        });
}
// #endregion
//Buyer for modal
$scope.buyerBrandList = [];
$scope.buyerList = [];
$scope.buyerPOPUP = function () {
    if (baseService.isUndefinedOrNull($scope.commitmentNew.EntityId)) {
        return ShowResult("Select entity first.", 'failure');
    }
    $scope.searchByBuyerList = [
        {
            'name': 'Buyer',
            'value': 'BuyerName'
        },
        {
            'name': 'Department',
            'value': 'DepartmentName'
        },
        {
            'name': 'Division',
            'value': 'DivisionName'
        }
    ];
    $scope.parameters.searchBy = 'BuyerName';
    baseService.init('Parties/BuyerMaster/GetAllBuyerMasterList?entityId=' + $scope.commitmentNew.EntityId, null, null, null, 'BuyerName', 'BuyerName');
    $scope.getBuyerData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getBuyerData();
    angular.element(document.querySelector('#buyerSearchModal')).modal('show');
};
//Passing Data For IntermediateItemEntity List
$scope.buyerCloseListPopUp = function (data) {
    $scope.commitmentNew.BuyerMasterId = data.Id;
    $scope.commitmentNew.BuyerId = data.BuyerId;
    $scope.commitmentNew.BuyerName = data.BuyerName;
    $scope.commitmentNew.BuyerDepartmentId = data.BuyerDepartmentId;
    $scope.commitmentNew.DepartmentName = data.DepartmentName;
    $scope.commitmentNew.BuyerDivisionId = data.BuyerDivisionId;
    $scope.commitmentNew.DivisionName = data.DivisionName;
    //$scope.getOurStyle();
    $scope.getBuyerBrand();
    $scope.getCboBuyerProgram();
    angular.element(document.querySelector('#buyerSearchModal')).modal('hide');
};
$scope.getBuyerBrand = function () {
    cboService.getBuyerBrandCboByBuyer($scope.commitmentNew.BuyerId, function (result) {
        $scope.buyerBrandList = result;
    });
};
//#end
//Value Added Process for modal
$scope.valueAddedProcessList = [];
$scope.valueAddedProcessSelectedList = [];
$scope.valueAddedProcessPOPUP = function () {
    $scope.searchByValueAddedProcessList = [
        {
            'name': 'Process',
            'value': 'ProcessName'
        },
        {
            'name': 'SubProcess',
            'value': 'SubProcessName'
        }
    ];
    $scope.parameters.searchBy = 'ProcessName';
    baseService.init('Processes/Process/GetLoadProcessWithSubProcess', null, null, null, 'ProcessName', 'ProcessName');
    $scope.getValuAddedProcessData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.valueAddedProcessList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getValuAddedProcessData();
    angular.element(document.querySelector('#valueAddedProcessSearchModal')).modal('show');
};
//Passing Data For IntermediateItemEntity List
$scope.valueAddedProcessCloseListPopUp = function () {
    angular.forEach($scope.valueAddedProcessList, function (item) {
        if (item.Flag) {
            $scope.valueAddedProcessSelectedList.push(
                {
                    Id: null,
                    ProcessId: item.Id,
                    ProcessName: item.ProcessName,
                    SubProcessId: item.SubProcessId,
                    SubProcessName: item.SubProcessName,
                    CompanyGroupId: $window.companyGroupId,
                    Flag: item.Flag
                }
            );
        }
    });
    angular.element(document.querySelector('#valueAddedProcessSearchModal')).modal('hide');
};
//#end
$scope.lsdDateMessage = '';
$scope.getLSDValidation = function () {
    if (new Date($filter('dateFiltering')($scope.commitmentNew.LSD, 'dd-MM-yyyy')) < new Date($filter('dateFiltering')(new Date(), 'dd-MM-yyyy'))) {
        $scope.lsdDateMessage = 'LSD date can not be below to current Date ';
    }
    else if (new Date($filter('dateFiltering')($scope.commitmentNew.LSD, 'dd-MM-yyyy')) >= new Date($filter('dateFiltering')(new Date(), 'dd-MM-yyyy'))) {
        $scope.lsdDateMessage = '';
    }
};
$scope.Get = function (id, index) {
    $scope.index = index;
    angular.copy($scope.commitments[$scope.index], $scope.commitment);
    angular.copy($scope.commitment, $scope.commitmentNew);

    //$scope.getOurStyle();
    $scope.getCboBuyerProgram();
    getSalesGroupCbo();
    $scope.getBuyerBrand();
    //var materialMasterId = [];
    //materialMasterId.push($scope.commitmentNew.MaterialMasterId);
    //cboService.getUomCboByMaterialMaster(JSON.stringify(materialMasterId), function (result) {
    //    $scope.uomList = result;
    //});
    $scope.getCmMonth();
    $http.get($scope.path + 'GetMonthList?masterId=' + $scope.commitmentNew.Id).then(function (response) {
        angular.forEach($scope.monthList, function (item, i) {
            //if (findValueFromMonth(item.CMonth, item.CYear, response.data)) {
            var dbob = $filter("filter")(response.data, { CommitmentId: item.CommitmentId, CMonth: item.CMonth, CYear: item.CYear })[0];
            if (!baseService.isUndefinedOrNull(dbob) && item.CMonth === dbob.CMonth && item.CYear === dbob.CYear)
                $scope.monthList[i] = dbob;
            //}
        })

        var totalQty = 0;
        for (var i = 0; i < $scope.monthList.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.monthList[i].Id)) {
                totalQty += $scope.monthList[i].Qty;
            }
        }
        $scope.commitmentNew.NumberOfLineDays = totalQty / $scope.commitmentNew.Target;

    });
    $http.get($scope.path + 'QueryCommitmentValueAdded?masterId=' + $scope.commitmentNew.Id).then(function (response) {
        $scope.valueAddedProcessSelectedList = response.data;
    });
    $scope.Action = 'Update';
    if (!$rootScope.isCollapsed) $rootScope.toggle();
};
function findValueFromMonth(month, year, list) {
    for (var i = 0; i < list.length; i++) {
        var item = list[i];
        if (item.CMonth === month && item.CYear === year) {
            return true;
            break;
        }
    }
    return false;
}
function getMonthSaveList() {
    $scope.monthSaveList = [];
    angular.forEach($scope.monthList, function (item) {
        if (!baseService.isUndefinedOrNull(item.Qty)) {
            $scope.monthSaveList.push(item);
        }
    })
}
$scope.Save = function () {
    $scope.$broadcast('show-errors-check-validity');
    if ($scope.commitmentNewForm.$valid) {
        if (parseInt($scope.commitmentNew.CM) >= parseInt($scope.commitmentNew.FOB)) return ShowResult('CM must be less than FOB');
        angular.copy($scope.commitmentNew, $scope.commitment);
        getMonthSaveList();
        if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'commitment': $scope.commitment
                    , 'monthList': $scope.monthSaveList
                    , 'cvAddedList': $scope.valueAddedProcessSelectedList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.copy(response.data.commitment, $scope.commitment);
                    getDataByEntity();
                    ClearFields();
                    $scope.valueAddedProcessSelectedList = [];
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
        else if ($scope.Action == "Update") {
            $http({
                method: 'POST',
                url: $scope.updateUrl,
                data: {
                    'commitment': $scope.commitment
                    , 'monthList': $scope.monthSaveList
                    , 'cvAddedList': $scope.valueAddedProcessSelectedList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1)
                        angular.copy($scope.commitment, $scope.commitments[$scope.index]);
                    getDataByEntity();
                    ClearFields();
                    $scope.valueAddedProcessSelectedList = [];
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
    }
};
$scope.Delete = function () {
    if (!baseService.isUndefinedOrNull($scope.commitmentNew.Id)) {
        $http({
            method: 'POST',
            url: $scope.deleteUrl + $scope.commitmentNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.commitments.splice($scope.index, 1);
                baseService.paginationRemove();
                ClearFields();
                $scope.valueAddedProcessSelectedList = [];
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
};
$scope.Clear = function () {
    $scope.commitment = {};
    $scope.commitmentNew = {};
    monthModelClear()
    $scope.monthList = [];
    $scope.valueAddedProcessSelectedList = [];
    $scope.Action = "Save";
};
function ClearFields() {
    $scope.Action = "Save";
    monthModelClear()
    $scope.monthList = [];
    $scope.valueAddedProcessSelectedList = [];
    $scope.commitment = {};
    $scope.commitmentNew = {
        Id: null
        , EntityId: $scope.commitmentNew.EntityId
        , FOB: 0
        , CM: 0
        , SPT: 0
        , Efficiency: 0
        , Target: 0
    };
}

$scope.tab = 1;
$scope.setTab = function (newTab) {
    $scope.tab = newTab;
};
$scope.isSet = function (tabNum) {
    return $scope.tab === tabNum;
};

// #region Finished Goods
$scope.popUpList = [];
$scope.popUpDataList = [];
$scope.excluedColumnList = [];
$scope.popUpParameters = {
    limit: 10,
    offset: 0,
    order: 'asc',
    sort: 'UserName',
    searchBy: 'UserName',
    pageSize: 10,
    total_count: 0,
    search: null,
    serverPagination: true
};
//$scope.popUp = function () {
//    $scope.popUpUrl = $scope.path + 'GetMaterialMasterList';
//    baseService.setCurrentPage('dataList');
//    $scope.getPopUpData = function (pageno) {
//        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
//            .then(function (result) {
//                $scope.popUpDataList = result.Rows;
//                $scope.popUpParameters.total_count = result.Total;
//                if (baseService.arrayLength($scope.popUpList) == 0)
//                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
//            }, function () {
//                ShowResult(commonMessage.NetworkError, 'failure', 'search_popup');
//            }).finally(function () {
//            });
//    };
//    $scope.getPopUpData();
//    angular.element(document.querySelector('#search_popup')).modal('show');
//};
$scope.popUp = function () {
    $scope.popUpUrl = $scope.path + 'GetProductMasterList';
    baseService.setCurrentPage('dataList');
    $scope.getPopUpData = function (pageno) {
        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
            .then(function (result) {
                $scope.popUpDataList = result.Rows;
                $scope.popUpParameters.total_count = result.Total;
                if (baseService.arrayLength($scope.popUpList) === 0)
                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'search_popup');
            }).finally(function () {
            });
    };
    $scope.getPopUpData();
    angular.element(document.querySelector('#search_popup')).modal('show');
};
$scope.getFinishedGoods = function (data) {
    //$scope.uomList = [];
    $scope.commitmentNew.ProductMasterId = data.Id;
    $scope.commitmentNew.ProductMaster = data.UserName;
    //$scope.commitmentNew.MaterialGroup = data.MaterialGroup;
    //$scope.commitmentNew.ProductMaster = data.ProductMaster;
    //$scope.commitmentNew.BaseUoM = data.BaseUoM;
    //if (baseService.isUndefinedOrNull($scope.commitmentNew.MaterialMasterId)) return $scope.commitmentNew.UoMId = null;
    //var materialMasterId = [];
    //materialMasterId.push($scope.commitmentNew.MaterialMasterId);
    //cboService.getUomCboByMaterialMaster(JSON.stringify(materialMasterId), function (result) {
    //    $scope.uomList = result;
    //    $scope.commitmentNew.UoMId = $filter('filter')($scope.uomList, { IsBaseUom: 1 })[0].Value;
    //})
    angular.element(document.querySelector('#search_popup')).modal('hide');
};
// #endregion
$scope.getUoMText = function () {
    if (baseService.isUndefinedOrNull($scope.commitmentNew.UoMId))
        return $scope.commitmentNew.BaseUoM = null;
    $scope.commitmentNew.BaseUoM = $filter('filter')($scope.uomList, { Value: $scope.commitmentNew.UoMId })[0].Text;
};

$scope.monthModel = {
    Id: null
    , CommitmentId: null
    , MonthYear: null
    , Qty: null
};
$scope.monthIndex = -1;
$scope.manualValidationAddRemove = function (divId, value, message) {
    if (baseService.isUndefinedOrNull(value))
        return manualValidation(divId, true, message);
    else
        return manualValidation(divId, false);
};
$scope.addMonthYear = function () {
    $scope.manualValidationAddRemove('div_MonthYear', $scope.monthModel.MonthYear, 'Month is required');
    $scope.manualValidationAddRemove('div_MonthQty', $scope.monthModel.Qty, 'Quantity is required');
    if (parseInt($scope.monthModel.Qty) === 0) return manualValidation('div_MonthQty', true, 'Quantity can\'t be zero.');
    var data = Object.assign({}, $scope.monthModel);
    angular.copy($scope.monthModel, data);

    // monthList depends on LSD

    if ($scope.monthIndex === -1) {
        $scope.monthList.push({
            Id: data.Id
            , CommitmentId: $scope.commitmentNew.Id
            , MonthYear: data.MonthYear
            , Qty: data.Qty
        });
    }
    else
        $scope.monthList[$scope.monthIndex] = data;
    monthModelClear();
};
$scope.editMonthYear = function (data, index) {
    angular.copy(data, $scope.monthModel);
    $scope.monthIndex = index;
};
function monthModelClear() {
    $scope.monthModel = {};
    $scope.monthIndex = -1;
}

$scope.rowRemoveModal = function (data, index) {
    $scope.monthIndex = index;
    $scope.message_confirmation = 'Are you sure want to delete permanently this data....';
    angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
};
$scope.removeRow = function () {
    $scope.monthList.splice($scope.monthIndex, 1);
    monthModelClear();
};

$scope.valuePassInDelModal = function (data, index) {
    $scope.Id = data.Id;
    $scope.bActivityIndex = index;
    if (baseService.isUndefinedOrNull($scope.Id))
        $scope.message_confirmation = 'Are you sure want to delete this data....';
    else
        $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + data.ProcessName + ' ]';
    angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
};

$scope.DeleteData = function () {
    if (baseService.isUndefinedOrNull($scope.Id)) {
        $scope.valueAddedProcessSelectedList.splice($scope.bActivityIndex, 1);
    }
    else {
        $http({
            method: 'POST',
            url: 'OrderManagements/commitment/DeleteProcess?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.valueAddedProcessSelectedList.splice($scope.bActivityIndex, 1);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    }
};

}