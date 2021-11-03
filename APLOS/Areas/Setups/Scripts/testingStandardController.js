'use strict';
TestingStandardController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function TestingStandardController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Testing Standard ";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.testingStandards = [];
    $scope.testingSelectedList = [];
    $scope.BuyerList = [];
    $scope.machineTypeData = [];
    $scope.searchbyMachineTypelist = [];
    $scope.fixedAssetMasterFormList = [];
    $scope.materialMasterFormList = [];
    $scope.path = 'Setups/testingStandard/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.searchByTestingStandardList = [
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
    $scope.testingStandardListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getTestingStandard = function () {
        $scope.GetTestingStandardListData = function (pageno) {
            baseService.paginationBase($scope.getListUrl, pageno, $scope.testingStandardListParameters)
                .then(function (data) {
                    $scope.testingStandards = data.Rows;
                    $scope.testingStandardListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#testingStandardPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetTestingStandardListData();
    };

    $scope.testingStandard = {
        Id: null,
        CompanyGroupId: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        TestingId: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.TestingStandardDetail = {
        Id: null,
        TestingId: null,
        TestingStandardId: null,
        TestingCategoryId: null,
        OriginatingProcessId: null,
        TestingProcessId:null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    }
    $scope.TestingStandardBuyer = {
        Id: null,
        TestingStandardId: null,
        BuyerId: null,
        BuyerName: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    }
    $scope.testingStandardNew = Object.assign({}, $scope.testingStandard);

    /***Cbo***************/
    $scope.testingCategoryList = [];
    cboService.getTestingCategoryCbo(function (result) {
        $scope.testingCategoryList = result;
    });

    $scope.processList = [];
    cboService.getCompanyProductionProcessCbo($window.companyId, function (result) {
        $scope.processList = result;
    })
    //--------------
    $scope.GetTestingStandardInfo = function (data) {
        $scope.testingStandardNew = data;
        $scope.getTestingStandardDetail();
        $scope.getTestingStandardBuyer();
        angular.element(document.querySelector('#testingStandardPopUp')).modal('hide');
    }
    //-----------
    //Deleting Rows from TestingStandarsList
    $scope.valuePassInTestingStandardSavedListDelModal = function (index, Id) {
        $scope.TestingStandardId = Id;
        $scope.testingStandardIndex = index;
        if (baseService.isUndefinedOrNull($scope.TestingStandardId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.TestingStandardId + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };
    $scope.DeleteTestingStandardSavedItem = function () {
        for (var i = 0; i < $scope.testingStandards.length; i++) {
            if ($scope.testingStandards[i].Id == $scope.TestingStandardId) {
                $http({
                    method: 'POST',
                    url: 'Setups/TestingStandard/Delete?id=' + $scope.TestingStandardId,
                }).then(function successCallback(response) {
                    ShowResult(response.data.Message, 'success');
                    $scope.testingStandards.splice(i, 1);
                    $scope.TestingStandardId = null;
                    $scope.testingStandardIndex = null;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
                return;
            }
        }
    };
    //
    //******************TestingStandardBuyer**************/
    $scope.ShowTestingBuyerForm = function () {
        angular.element(document.querySelector('#testingStandardBuyerFormPopUp')).modal('show');
    }
    $scope.searchByBuyerList = [
        ,
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
    $scope.BuyerListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetTestingBuyerList = function () {
        $scope.GLUrl3 = 'Parties/Buyer/GetList'
        baseService.setCurrentPage('BuyerList');
        $scope.GetBuyerListDatas = function (pageno) {
            //baseService.init($scope.GLUrl3, pageno, $scope.BuyerListParameters);
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.BuyerListParameters)
                .then(function (data) {
                    $scope.BuyerList = data.Rows;
                    $scope.BuyerListParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.BuyerList.length; i++) {
                        $scope.BuyerList[i].Flag = getActive($scope.tempBuyerList, $scope.BuyerList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#buyerPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetBuyerListDatas();
    };
    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BuyerId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.buyerSelectedList = [];
    $scope.buyerSelectdCloseListPopUp = function () {
        angular.forEach($scope.tempBuyerList, function (item) {
            if (item.Flag && checkExist($scope.buyerSelectedList, item.Id) === false) {
                $scope.buyerSelectedList.push(
                    {
                        Id: null,
                        BuyerId: item.Id,
                        TestingStandardId: null,
                        Code: item.Code,
                        ShortName: item.ShortName,
                        StandardName: item.StandardName,
                        UserName: item.UserName
                    }
                );
            }
        });
        angular.element(document.querySelector('#buyerPopUp')).modal('hide');
        if ($scope.buyerSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    }
    $scope.testingBuyerCloseListPopUp = function () {
        angular.element(document.querySelector('#testingStandardBuyerFormPopUp')).modal('hide');
    }
    //-----------------
    //getTestingStandardBuyer***********/
    $scope.TestingStandardBuyerSavedList = [];
    $scope.getTestingStandardBuyer = function () {
        $http({
            method: 'GET',
            url: 'Setups/TestingStandard/GetTestingStandardBuyer?testingStandardId=' + $scope.testingStandardNew.Id,
        }).then(function successCallback(response) {
            $scope.TestingStandardBuyerSavedList = response.data;
            //console.log('TestingStandardBuyerSavedList', $scope.TestingStandardBuyerSavedList);
        })
    }
    //********BuyerCheckBoxExist**********/
    $scope.tempBuyerList = [];
    $scope.selectBuyerChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempBuyerList($scope.tempBuyerList, data.Id) === false) {
                    $scope.tempBuyerList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempBuyerList.length; i++) {
                    if ($scope.tempBuyerList[i].Id === data.Id) {
                        $scope.tempBuyerList.splice(i, 1);
                    }
                    // break;
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempBuyerList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    //**********EndCheckBox*********/
    //-----------

    //Deleting Rows from TestingStandardBuyerList
    $scope.valuePassInBuyerSavedDelModal = function (index, Id) {
        $scope.TestingStandardBuyerId = Id;
        $scope.buyerIndex = index;
        if (baseService.isUndefinedOrNull($scope.TestingStandardBuyerId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.TestingStandardBuyerId + ' ]';
        angular.element(document.querySelector('#confirmGenericPopUpForBuyerSaved')).modal('show');
    };

    $scope.DeleteTestingStandardBuyerSavedItem = function () {
        for (var i = 0; i < $scope.TestingStandardBuyerSavedList.length; i++) {
            if ($scope.TestingStandardBuyerSavedList[i].Id == $scope.TestingStandardBuyerId) {
                $http({
                    method: 'POST',
                    url: 'Setups/TestingStandard/DeleteTestingStandardBuyer?id=' + $scope.TestingStandardBuyerId,
                }).then(function successCallback(response) {
                    ShowResult(response.data.Message, 'success');
                    $scope.TestingStandardBuyerSavedList.splice(i, 1);
                    $scope.TestingStandardBuyerId = null;
                    $scope.buyerIndex = null;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
                return;
            }
        }
    };
    //
    //Deleting Rows from TestingStandardBuyerList
    $scope.valuePassInBuyerDelModal = function (index, Id) {
        $scope.TestingStandardBuyerTempId = Id;
        $scope.buyerIndex = index;
        if (baseService.isUndefinedOrNull($scope.TestingStandardBuyerTempId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.TestingStandardBuyerTempId + ' ]';
        angular.element(document.querySelector('#confirmGenericPopUpForBuyer')).modal('show');
    };

    $scope.DeleteTestingStandardBuyerTempItem = function () {
        $scope.buyerSelectedList.splice($scope.buyerIndex, 1);
    };
    //
    //******************TestingStandardDetail**************/
    $scope.ShowTestingDeailForm = function () {
        angular.element(document.querySelector('#testingStandardDetailFormPopUp')).modal('show');
    }
    $scope.searchByTestingDeailList = [
        ,
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

    $scope.TestingDeailListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GetTestingDeailList = function () {
        $scope.GLUrl3 = 'Setups/testing/gettestingdata?testingCategoryId=' + $scope.TestingStandardDetail.TestingCategoryId + '&testingStandardId=' + $scope.testingStandardNew.Id
        baseService.setCurrentPage('TestingDeailList');
        $scope.GetTestingListDatas = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.TestingDeailListParameters)
                .then(function (data) {
                    $scope.TestingDeailList = data.Rows;
                    $scope.TestingDeailListParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.TestingDeailList.length; i++) {
                        $scope.TestingDeailList[i].Flag = getActive($scope.tempTestingList, $scope.TestingDeailList[i].Id);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#testingPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetTestingListDatas();
    };

    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TestingId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.testingSelectdCloseListPopUp = function () {
        angular.forEach($scope.tempTestingList, function (item) {
            if (item.Flag && checkExist($scope.testingSelectedList, item.Id) === false) {
                $scope.testingSelectedList.push(
                    {
                        Id: null,
                        TestingId: item.Id,
                        TestingStandardId: null,
                        Code: item.Code,
                        ShortName: item.ShortName,
                        StandardName: item.StandardName,
                        UserName: item.UserName,
                        TestingCategoryName: item.TestingCategoryName,
                        Value: null,
                        OriginatingProcessId: null,
                        TestingProcessId:null
                    }
                );
            }
        });
        angular.element(document.querySelector('#testingPopUp')).modal('hide');
        if ($scope.testingSelectedList.length > 0) {
            $scope.tableShow = true;
        }
        else {
            $scope.tableShow = false;
        }
    }
    $scope.budgetMasterFormCloseListPopUp = function () {
        angular.element(document.querySelector('#testingStandardDetailFormPopUp')).modal('hide');
    }
    //********TestingCheckBoxExist**********/
    $scope.tempTestingList = [];
    $scope.selectTestingChValue = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempTestingList($scope.tempTestingList, data.Id) === false) {
                    $scope.tempTestingList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempTestingList.length; i++) {
                    if ($scope.tempTestingList[i].Id === data.Id) {
                        $scope.tempTestingList.splice(i, 1);
                    }
                    // break;
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }
    function checkExistTempTestingList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                return true;
            }
        }
        return false;
    }
    //**********EndCheckBox*********/
    //-----------------
    //getTestingStandardDetail***********/
    $scope.TestingStandardDetailSavedList = [];
    $scope.getTestingStandardDetail = function () {
        $http({
            method: 'GET',
            url: 'Setups/TestingStandard/GetTestingStandardDetail?testingStandardId=' + $scope.testingStandardNew.Id,
        }).then(function successCallback(response) {
            $scope.TestingStandardDetailSavedList = response.data;
            //console.log('TestingStandardDetailSavedList', $scope.TestingStandardDetailSavedList);
        })
    }

    //-----------
    //Deleting Rows from TestingDetailList
    $scope.valuePassInTestingStandardDetailSavedListSavedDelModal = function (index, data) {
        $scope.TestingStandardDetail = data.Id;
        $scope.testingDetailIndex = index;
        if (baseService.isUndefinedOrNull($scope.TestingStandardDetail))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete permanently [ ' + data.UserName + ' ]';
        angular.element(document.querySelector('#confirmGenericPopUpForTestingDeailSaved')).modal('show');
    };
    $scope.DeleteTestingStandardDetailSavedItem = function () {
        for (var i = 0; i < $scope.TestingStandardDetailSavedList.length; i++) {
            if ($scope.TestingStandardDetailSavedList[i].Id == $scope.TestingStandardDetail) {
                $http({
                    method: 'POST',
                    url: 'Setups/TestingStandard/DeleteTestingStandardDetail?id=' + $scope.TestingStandardDetail,
                }).then(function successCallback(response) {
                    ShowResult(response.data.Message, 'success');
                    $scope.TestingStandardDetailSavedList.splice(i, 1);
                    $scope.TestingStandardDetail = null;
                    $scope.testingDetailIndex = null;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
                return;
            }
        }
    };
    //
    //Deleting Rows from TestingStandardBuyerList
    $scope.valuePassInTestingDelModal = function (index, Id) {
        $scope.TestingStandardDetailTempId = Id;
        $scope.testingDetailIndex = index;
        if (baseService.isUndefinedOrNull($scope.TestingStandardDetailTempId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + $scope.TestingStandardDetailTempId + ' ]';
        angular.element(document.querySelector('#confirmGenericPopUpForTestingDeail')).modal('show');
    };

    $scope.DeleteTestingStandardDetailItem = function () {
        $scope.testingSelectedList.splice($scope.testingDetailIndex, 1);
    };
    //
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.testingStandard = $scope.testingStandards[$scope.index];
        $scope.testingStandardNew = Object.assign({}, $scope.testingStandard);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    function checkAmount() {
        angular.forEach($scope.testingSelectedList.length > 0 ? $scope.testingSelectedList : $scope.TestingStandardDetailSavedList, function (item) {
            if (item.Amount < 1) {
                return false;
            } else {
                return true;
            }
        });
        return true;
    }
    function getTestingStandardById(Id) {
        $http({
            method: 'GET',
            url: 'Setups/testingStandard/GetTestingStandardById?id=' + Id,
        }).then(function successCallback(response) {
            $scope.testingStandardNew = response.data.Rows[0];
            $scope.getTestingStandardDetail();
            $scope.getTestingStandardBuyer();
        })
    }
    $scope.Save = function () {
        console.log('testingSelectedList', $scope.testingSelectedList)
        angular.copy($scope.testingStandardNew, $scope.testingStandard);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.testingStandardForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'testingStandard': $scope.testingStandard },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getTestingStandardById(response.data.TestingStandardId);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.testingStandard,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.testingStandards[$scope.index] = $scope.testingStandard;
                            $scope.testingStandards = $filter('orderBy')($scope.testingStandards, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.TestingDeailSave = function () {
        angular.copy($scope.testingStandardNew, $scope.testingStandard);
        if (checkAmount() === false) {
            return ShowResult('Budget Master can not be less then 1!!', 'failure');
        }
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.testingStandardDetailForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'testingStandard': $scope.testingStandard, 'TestingStandardDetail': $scope.testingSelectedList.length > 0 ? $scope.testingSelectedList : $scope.TestingStandardDetailSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getTestingStandardById(response.data.TestingStandardId);
                        $scope.budgetMasterFormCloseListPopUp();
                        $scope.tempTestingList = [];
                        $scope.testingSelectedList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.testingStandard,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.testingStandards[$scope.index] = $scope.testingStandard;
                            $scope.testingStandards = $filter('orderBy')($scope.testingStandards, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.TestingBuyerSave = function () {
        angular.copy($scope.testingStandardNew, $scope.testingStandard);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.testingStandardBuyerForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'testingStandard': $scope.testingStandard, 'testingStandardBuyer': $scope.buyerSelectedList.length > 0 ? $scope.buyerSelectedList : $scope.TestingStandardBuyerSavedList },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        getTestingStandardById(response.data.TestingStandardId);
                        $scope.testingBuyerCloseListPopUp();
                        $scope.tempBuyerList = [];
                        $scope.buyerSelectedList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.testingStandard,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.testingStandards[$scope.index] = $scope.testingStandard;
                            $scope.testingStandards = $filter('orderBy')($scope.testingStandards, 'Sequence');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.testingStandardNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.testingStandardNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.testingStandards.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    // #region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion


    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.testingStandard = {};
        $scope.testingStandardNew = {};
        $scope.testingStandardNew.Id = null
        $scope.machineTypeMasterList = [];
        $scope.BuyerList = [];
        $scope.TestingStandardDetailSavedList = [];
        $scope.TestingStandardBuyerSavedList = [];
        $scope.testingSelectedList = [];
        $scope.buyerSelectedList = [];
        $scope.testingStandardNew.Active = true;
    }
}