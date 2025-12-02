'use strict';
DetentionTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DetentionTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.title = 'Detention Type';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'IE/DetentionType/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;


    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
            
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
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getBudget();
        }
    };

    $scope.SaveDT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
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
    }

    // Code By Nitesh

    $scope.BudgetCodeList = [];
    $scope.userBudgetCodeList = [];
    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBudgetCode'
        }).then(function successCallback(response) {
            $scope.BudgetCodeList = response.data;

            for (var i = 0; i < $scope.userBudgetCodeList.length; i++) {
                for (var j = 0; j < $scope.BudgetCodeList.length; j++) {
                    if ($scope.userBudgetCodeList[i].Id === $scope.BudgetCodeList[j].Id) {
                        $scope.BudgetCodeList[j].chk = true;
                    }
                }
            }
        });
    }
    $scope.chkdBudgetCodeList = [];
    $scope.BudgetCodeGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
            $scope.BudgetCodeList[i].chk = ChkOrUnchk;
            $scope.chkdBudgetCodeList = $scope.BudgetCodeList[i].chk;
        }

        var gridObj = $("#GridDetentionMasterBudgetCode").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.openBudgetCodePopUp = function () {
        $scope.getBudgetCode();
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('show');
    }

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    };

    function checkBudgetCodeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DepartmentDataList = [];
    $scope.SaveBudgetCode = function () {

        try {
           
            if (baseService.arrayLength($scope.BudgetCodeList) > 0) {
                angular.forEach($scope.BudgetCodeList, function (a) {
                    if (checkBudgetCodeExist($scope.userBudgetCodeList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            ob.ManPowerBudgetId = a.ManPowerBudgetId;
                            ob.departmentTypeId = a.Id;
                            ob.Code = a.Code;
                            ob.Entity = a.Entity;
                            
                            $scope.userBudgetCodeList.push(ob);
                            ob = {};
                        }
                    }

                });
            }
            
            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.path + 'SaveBudgetCode',
                data: {
                    'data': $scope.userBudgetCodeList,
                    'detentionTypeId': $scope.ModelNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    
                   
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeBudgetCodePopUp();
    };

    $scope.getBudget = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getBudget',
            data: { 'detentionTypeId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userBudgetCodeList = [];
            $scope.userBudgetCodeList = resp.data;
        });
    }

    $scope.removeDeptRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemoveDeptPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
}