//$('#cutOffDate').datepicker('setEndDate', new Date(Date.now()));
$scope.activityMaster[$scope.fieldId] = data.Id;
count.setDate(count.getDate() + 1);
companySubProcess
delete item['$$hashKey'];
delete item.$$hashKey;
parameters.order = "asc";
parameters.sort = "Name";
parameters.searchBy = "Name";
parameters.limit = 10;

var tomorrow = new Date();
tomorrow.setDate(tomorrow.getDate() + 1);
$filter("filter")($scope.currencyExchangeRate, { ParallelCurrencyType: "HardCurrency" })
$rootScope.parameters.processId = $scope.operationNew.ProcessId;
$scope.operationParameters.processid = $scope.bulletinmaster.ProcessId;

baseService.isUndefinedOrNull($scope.valueData)
$http.get($scope.getLotNumberUrl)
    .then(function (response) {
        $scope.checkLotNew.LotNumber = response.data;
    });
//param

$http({
    method: "post",
    url: '/useraccessrestriction/update',
    data: $scope.restrictionList,
    dataType: "json"
}).then(function successCallback(response) {
    if (response.data.Error == true) {
        ShowResult(response.data.Message, "failure");
    }
    else {
        ShowResult(response.data.Message, "success");
        $scope.getData();
        $scope.restrictionList = [];
    }
}, function errorCallback(response) {
    ShowResult(response.status.Message, "failure");
});
///hr
<hr style="width:95%; text-align:center;" />
//-----------------Enum cbo calling ------------------
cboService.getEnumCbo("enum/GetPaymentModeCbo", function (result) {
    $scope.PaymentModeList = result;
});

cboService.getShipModeCbo(function (result) {
    $scope.shipModeList = result;
});

//////////////////////PopUp Search Start/////////////////////////////
$rootScope.searchByList = [
    {
        'name': 'Sequence',
        'value': 'Sequence'
    },
    {
        'name': 'Code',
        'value': 'Code'
    },
    {
        'name': 'Short Name',
        'value': 'ShortName'
    },
    {
        'name': 'Standard Name',
        'value': 'StandardName'
    },
    {
        'name': 'User Name',
        'value': 'UserName'
    }
];
$scope.popUpList = [];
$scope.valueData = '';
$scope.popUpParameters = {
    limit: 10,
    offset: 0,
    order: 'asc',
    sort: 'Code',
    searchBy: "UserName",
    pageSize: 10,
    total_count: 0,
    search: null,
    serverPagination: true
};
$scope.popUp = function () {
    $scope.popUpUrl = '';
    baseService.setCurrentPage('dataList');
    $scope.getPopUpData = function (pageno) {
        baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
            .then(function (result) {
                $scope.popUpDataList = result.Rows;
                $scope.popUpParameters.total_count = result.Total;
                if (baseService.arrayLength($scope.popUpList) == 0) {
                    baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
            }).finally(function () {
            });
    };
    angular.element(document.querySelector('#popUpId')).modal('show');
    $scope.getPopUpData();
}

$scope.selectDoubleClick = function (data) {
    // Do Somthing
    $scope.closePopUp();
}

$scope.selectSingleClick = function (data) {
    $scope.valueData = data;
}
$scope.selectByButton = function () {
    if (baseService.isUndefinedOrNull($scope.valueData)) {
        alert('Please at first select row');
        return;
    }
    $scope.selectDoubleClick($scope.valueData)
    $scope.closePopUp();
}
$scope.closePopUp = function () {
    $scope.valueData = '';
    angular.element(document.querySelector('#popUpId')).modal('hide');
}
//////////////////////PopUp Search End/////////////////////////////

//////////////************GUID**************///////////////////////
function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
        s4() + '-' + s4() + s4() + s4();
}

/////////////////// data or object send from one controller to another ////////////////////////////////

//inject['dataShare']
//1st Controller--

$scope.send = function (data) {
    dataShare.sendData(data);
};

//--inject['dataShare']
//--2nd Controller--
function GetUserInfoFromUserAccess() {
    $scope.userRoleDetail = dataShare.getData();
}
/////////////////// End ////////////////////////////////
document.getElementById("altUOMId").options[document.getElementById('altUOMId').selectedIndex].text;
angular.element("#year :selected").text();
//////////////////////////////////////////////////////////////////

baseService.$on('$locationChangeStart', function (event, current, previous) {
    console.log("Previous URL" + previous);
    console.log("current URL" + current);
});
baseService.$on('$locationChangeSuccess', function (event, current, previous) {
    console.log("Previous URL" + previous);
    console.log("current URL" + current);
});
/////////////////////////////////////////angularCboIdCrossCheck//////////////////////////////////////////////////////////////////////////////////////

var characteristicsName = $.grep($scope.characterCbo, function (item) {
    return item.Value == $scope.materialgridcharacs.CharacteristicsId;
})[0].Text;

var list = [];
angular.forEach($scope.characs, function (element, i) {
    list.push(element.CharacteristicsId);
});

if (list.indexOf($scope.materialgridcharacs.CharacteristicsId) == -1) {
    $scope.characs.push(
        {
            CharacteristicsId: $scope.materialgridcharacs.CharacteristicsId,
            CharacteristicsName: characteristicsName,
            Sort: $scope.materialgridcharacs.Sort
        }
    )
}
// #region checkAll

$scope.CheckAll = function (event, list) {
    //console.log(event);
    var _isselected = event.target.checked;
    var _name = event.target.name;
    for (var i = 0; i < baseService.arrayLength(list); i++) {
        list[i][_name] = _isselected;
    }
}

$scope.UnCheck = function (event) {
    var _isselected = event.target.checked;
    var _name = event.target.name;
    $scope[_name] = allTrue(_name);
}

function allTrue(name, list) {
    var flag = false;
    for (var i = 0; i < baseService.arrayLength(list); i++) {
        if (list[i][name]) {
            flag = true;
        }
        else {
            flag = false;
            break;
        }
    }
    return flag;
}
// #endregion

$scope.tab = 1;
$scope.setTab = function (newTab) {
    $scope.tab = newTab;
};

$scope.isSet = function (tabNum) {
    return $scope.tab === tabNum;
};
// #region ReturnToRequiredTab
function reDirectToRequiredTab() {
    if ($scope.partyForm2.$invalid) {
        $scope.setTab(1);
    }
    else if ($scope.partyForm3.$invalid) {
        $scope.setTab(2);
    }
}

// #endregion
$scope.userNew.UpdatedDate = $filter('dateFiltering')($scope.userNew.UpdatedDate);
($filter('filter')($scope.entityList, { Value: id }));

function isProcessIdExistGrid(list) {
    $scope.ProcessIds = [];
    if (list.length > 0) {
        for (var i = 0; i < list.length; i++) {
            if (list[i]['Archive'] == false) {
                $scope.ProcessIds.push(list[i]['ProcessId']);
            }
        }
    }
    return JSON.stringify($scope.ProcessIds);
}
$scope.addProcess = function () {
    if (!isRowSelected($scope.processList)) {
        ShowResult('Please select at least one row...!', 'failure', 'processPopUp');
        return;
    }
    angular.forEach($scope.processList, function (a) {
        if (a.Flag) {
            $scope.skillProcessList.push({
                Id: null,
                ProcessId: a.Id,
                Sequence: a.Sequence,
                Code: a.Code,
                UserName: a.UserName,
                LocalName: a.LocalName,
                MaterialType: a.MaterialType,
                Active: a.Active,
                Archive: false
            });
        }
    });
    if (!$scope.processTblShow)
        $scope.processTblShow = true;
    $scope.CloseProcessPopUp();
};
function isRowSelected(ilst) {
    try {
        var flag = false;
        for (var i = 0; i < ilst.length; i++) {
            if (ilst[i].Flag) {
                return flag = true;
            }
        }
    } catch (e) {
    }
}
$scope.valuePassInDelModal = function (data, index) {
    $scope.message_confirmation = '';
    $scope.processId = data.ProcessId;
    if (baseService.isUndefinedOrNull($scope.id))
        $scope.message_confirmation = 'Are you sure want to delete this data....';
    else
        $scope.message_confirmation = 'Are you sure want to delete [ ' + name + ' ]';
    angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
};

$scope.removeProcessRow = function () {
    for (var i = 0; i < $scope.skillProcessList.length; i++) {
        if ($scope.skillProcessList[i].Id == null && $scope.skillProcessList[i].ProcessId == $scope.processId) {
            $scope.skillProcessList.splice(i, 1);
        }
        else if ($scope.skillProcessList[i].Id != null && $scope.skillProcessList[i].ProcessId == $scope.processId)
            $scope.skillProcessList[i].Archive = true;
    }
    if ($scope.skillProcessList.length > 0) {
        $scope.processTblShow = true;
    }
    else {
        $scope.processTblShow = false;
    }
};
$scope.psDeleteRow = function () {
    for (var i = 0; i < $scope.processUomList.length; i++) {
        if ($scope.processUomList[i].Id == $scope.Id && $scope.Id.startsWith('new')) {
            $scope.processUomList.splice(i, 1);
        }
        else if ($scope.processUomList[i].Id != null && $scope.processUomList[i].Id == $scope.Id)
            $scope.processUomList[i].Archive = true;
    }
    if ($scope.processUomList.length > 0) {
        $scope.processUomTableShow = true;
    }
    else {
        $scope.processUomTableShow = false;
    }
};

for (var i = 0; i < $scope.processUomList.length; i++) {
    if ($scope.processUomList[i].Id.startsWith('new')) {
        $scope.processUomList[i].Id = null;
    }
}

$scope.cIndex = -1;
$scope.editSalary = function (data, index) {
    for (var i = 0; i < $scope.operationCategorySalaryList.length; i++) {
        if ($scope.operationCategorySalaryList[i].EffectiveDate == data.EffectiveDate) {
            $scope.cIndex = i;
        }
    }
    $scope.operationCategorySalary = $scope.operationCategorySalaryList[$scope.cIndex];
    $scope.operationCategorySalaryNew = angular.copy($scope.operationCategorySalary);
    $scope.CAction = 'Update Row';
}



$scope.pk = function () {
    return 'new' + Math.floor(Math.random() * 900000) + 100000;
};



$http({
    method: "GET",
    url: $scope.path + 'GetList',
    params: {
        'plantId': $scope.plantId,
        'toDate': $scope.toDate,
        'companyId': $window.companyId
    },
    dataType: "json"
}).then(function successCallback(response) {
    if (response.data.Error == true) {
        ShowResult(response.data.Message, 'failure');
    }
    else {
        //ShowResult(response.data.Message, 'success');
    }
}), function errorCallBack(response) {
    showResult(response.data.Message, 'failure');
}


/***********ArrayList Marge****************/
$scope.CurrencyList = function myfunction() {

    //$scope.currencyList.push($scope.invoiceDetailCurrencyrow);
    //$scope.currencyList.push($scope.voucherDetailCurrencyrow);
    merge($scope.invoiceDetailCurrencyrow, $scope.voucherDetailCurrencyrow);
};
function merge(array1, array2) {
    var ids = [];
    var merge_obj = [];

    array1.map(function (ele) {
        if (!(ids.indexOf(ele.Id) > -1)) {
            ids.push(ele.Id);
            merge_obj.push(ele);
        }
    });

    array2.map(function (ele) {
        var index = ids.indexOf(ele.Id);
        if (!(index > -1)) {
            ids.push(ele.Id);
            merge_obj.push(ele);
        } else {
            merge_obj[index] = ele;
        }
    });
    $scope.currencyList = merge_obj;
    console.log('mmm', merge_obj, $scope.currencyList);
}




// duplicate checking list
function listValidation(oldValue, newValue, index) {
    var isAvailable = false;
    // MaterialAttributeId
    if ($scope.index == -1) {
        if (oldValue == newValue) {
            isAvailable = true;
            return isAvailable;
        }
    }
    else {
        if ($scope.index != index) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
    }
    return isAvailable;
}
function isSeqValid(list) {
    try {
        if (list == null || list.length <= 0) {
            throw 'Please insert at lest one row';
        }
        var newList = [];
        for (var i = 0; i < list.length; i++) {
            if (list[i].IsFreeField == false && list[i].IsPreDefinedField == false) {
                throw 'Please select free field or pre-defined field or both';
            }
            var seq = list[i].Sequence;
            if (list[i].Sequence == null) {
                throw 'Sequence can not be null';
            }
            if (newList.indexOf(seq) == -1) {
                newList.push(seq);
            }
            else {
                throw 'Duplicate Sequence [' + seq + '] found in grid';
            }
        }
    } catch (e) {
        throw e;
    }
}

// #region ReturnToRequiredTab
function reDirectToRequiredTab() {
    if ($scope.partyForm2.$invalid) {
        $scope.setTab(1);
    }
    else if ($scope.partyForm3.$invalid) {
        $scope.setTab(2);
    }

}
// #endregion

//****Push on list via checkbox********//
$scope.selectedTaxCodeGlList = [];
$scope.getSelectedTaxCodeList = function (x, index) {
    if (x.Active && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId) === false) {
        $scope.selectedTaxCodeGlList.push(x);
    } else if (x.Active === false && checkAvailable($scope.selectedTaxCodeGlList, x.CompanyId)) {
        for (var i = 0; i < $scope.selectedTaxCodeGlList.length; i++) {
            if ($scope.selectedTaxCodeGlList[i].CompanyId === x.CompanyId) {
                $scope.selectedTaxCodeGlList.splice(i, 1);
            } else {

            }
        }
    }
    console.log($scope.selectedTaxCodeGlList)
}
//*************



$scope.characteristicsValue = function (id, listName) {
    $scope.characteristicsValueDataList = [];
    chListName = listName;
    $scope.characteristicsValueUrl = '/materials/characteristicsvalue/getlist?characteristicsId=' + id + '&ids=' + isChIdExistGrid($scope[chListName]);
    baseService.setCurrentPage('characteristicsValueDataList');
    $scope.getcharacteristicsValueData = function (pageno) {
        baseService.paginationBase($scope.characteristicsValueUrl, pageno, $scope.characteristicsValueParameters)
            .then(function (result) {
                $scope.characteristicsValueDataList = result.Rows;
                $scope.characteristicsValueParameters.total_count = result.Total;
                for (var i = 0; i < $scope.characteristicsValueDataList.length; i++) {
                    $scope.characteristicsValueDataList[i].Flag = tempList.includes($scope.characteristicsValueDataList[i].Id)
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'chValuePopUp');
            }).finally(function () {
            });
    };
    angular.element(document.querySelector('#chValuePopUp')).modal('show');
    $scope.getcharacteristicsValueData();
}
var tempList = [];
$scope.selectChValueId = function (event, id) {
    if (event.currentTarget.checked)
        tempList.push(id);
    else
        tempList.splice(tempList.indexOf(id), 1);
}
var comGroupName = $.grep($scope.companyGroupList, function (item) {
    return item.Value === id;
})[0].Text;
};


$scope.Save = function () {
    try {
        $http({
            method: "POST",
            url: $scope.saveUrl,
            data: $scope.materialMaster,
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    } catch (e) {

    }
}



$scope.manualValidationAddRemove = function (divId, fieldName, message) {
    var msg = fieldName + ' is required.';
    msg = baseService.isUndefinedOrNull(message) ? msg : message;
    var str = fieldName
    if (baseService.isUndefinedOrNull($scope.materialValueNew[str.replace(/\s/g, '')]))
        return manualValidation(divId, true, msg);
    else
        return manualValidation(divId, false);
};
$scope.addMasterAttributeValue = function () {
    $scope.manualValidationAddRemove('div_1', 'Sequence');
    $scope.manualValidationAddRemove('div_2', 'Code');
    $scope.manualValidationAddRemove('div_3', 'Short Name');
    $scope.manualValidationAddRemove('div_4', 'Standard Name');
    $scope.manualValidationAddRemove('div_5', 'User Define Name');
};

//$scope.detailModel.BaseUOMId = $filter("filter")($scope.uoMList, { IsBaseUom: 1 })[0].Value; 
//var partyPlantList = $filter('filter')($scope.partyPlantList, { PartyId: partyId }, true);
$('#cutOffDate').datepicker({
    'setStartDate': new Date($scope.productNew.CutOffDate)
});


function checkSameValueInColumnList(list, fieldName) {
    var flag = true;
    for (var i = 0; i < baseService.arrayLength(list); i++) {
        if (list[i][fieldName] === (i > 0 ? list[i - 1][fieldName] : list[i][fieldName]))
            flag = true;
        else flag = false;
    }
    return flag;
}