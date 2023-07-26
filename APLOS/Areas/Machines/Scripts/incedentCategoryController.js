'use strict';
incedentCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function incedentCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Incedent Category';
    $scope.Action = 'Save';
    $scope.IncedentTitleAction = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Machines/IncedentCategory/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getIncedentTitleSeqUrl = $scope.path + 'IncedentTitleGetAutoSequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.IncedentTitleSaveUrl = $scope.path + 'IncedentTitleSave';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.IncidentTitledeleteUrl = $scope.path + 'IncidentTitledelete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'InchargeNameBgtCode', name: "Incharge Name BgtCode" }, { value: 'SuperUserBgtCode', name: "Super User BgtCode" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

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
        InchargeNameBgtCodeId: null,
        InchargeNameBgtCode: null,
        SuperUserBgtCodeId:null,
        SuperUserBgtCode:null,
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
        }
    };

    $scope.selectInchargeNameBudgetCode = function () {
        $scope.getInchargeNameBudgetCode();
        angular.element(document.querySelector('#InchargeNameBgtCodePopUp')).modal('show');
    }

    $scope.InchargeNameBudgetCodeList = [];
    $scope.getInchargeNameBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetInchargeNameBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.InchargeNameBudgetCodeList = resp.data;
        });
    }

    $scope.doubleInchargeNameBudgetCode = function (e) {
        $scope.ModelNew.InchargeNameBgtCodeId = e.data.ManPowerBudgetId;
        $scope.ModelNew.InchargeNameBgtCode = e.data.Code;
        angular.element(document.querySelector('#InchargeNameBgtCodePopUp')).modal('hide');
    }

    $scope.closeInchargeNameBudgetCodePopUp = function () {
        angular.element(document.querySelector('#InchargeNameBgtCodePopUp')).modal('hide');
    }

    $scope.selectSuperUserBudgetCode = function () {
        $scope.getSuperUserBudgetCode();
        angular.element(document.querySelector('#SuperUserBgtCodePopUp')).modal('show');
    }

    $scope.SuperUserBudgetCodeList = [];
    $scope.getSuperUserBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSuperUserBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SuperUserBudgetCodeList = resp.data;
        });
    }

    $scope.doubleSuperUserBudgetCode = function (e) {
        $scope.ModelNew.SuperUserBgtCodeId = e.data.ManPowerBudgetId;
        $scope.ModelNew.SuperUserBgtCode = e.data.Code;
        angular.element(document.querySelector('#SuperUserBgtCodePopUp')).modal('hide');
    }

    $scope.closeSuperUserBudgetCodePopUp = function () {
        angular.element(document.querySelector('#SuperUserBgtCodePopUp')).modal('hide');
    }


    $scope.Save = function () {
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

    // #region IncedentTitle
    $rootScope.Incedentitle = 'Incedent Title';

    $scope.GetIncedentitleSequence = function () {
        cboService.getSequence($scope.getIncedentTitleSeqUrl, function (data) {
            $scope.IncedentitleModelTemp.Sequence = data;
            $scope.IncidentTitleNew.Sequence = data;
        });
    };
    $scope.GetIncedentitleSequence();

    $scope.IncedentitleGet = function (args) {

        $scope.IncidentTitleNew = Object.assign({}, args.data);
        $scope.IncedentTitleAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.IncedentitleModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,       
        Remarks: null,        
        Active: true
    };
    $scope.IncidentTitleNew = Object.assign({}, $scope.IncedentitleModelTemp);

    $scope.IncedentTitleList = [];
    $scope.getIncedentTitleData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetIncedentTitleList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.IncedentTitleList = response.data;
            IncedentTitleClearFields(response.data.Sequence);
            $scope.GetIncedentitleSequence();
        });
    }
    $scope.getIncedentTitleData();

    $scope.IncedentTitleSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.IncidentTitleNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.IncedentTitleSaveUrl,
                data: { 'data': $scope.IncidentTitleNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    IncedentTitleClearFields(response.data.Sequence);
                    $scope.getIncedentTitleData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.IncedentTitleDelete = function () {
        if (!baseService.isUndefinedOrNull($scope.IncidentTitleNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.IncidentTitledeleteUrl + $scope.IncidentTitleNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    IncedentTitleClearFields(response.data.Sequence);
                    $scope.getIncedentTitleData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.IncedentTitleClear = function () {
        IncedentTitleClearFields($scope.GetIncedentitleSequence());
        return true;
    };


    function IncedentTitleClearFields(seq) {
        $scope.IncedentTitleAction = 'Save';
        $scope.IncidentTitleNew = Object.assign({}, $scope.IncedentitleModelTemp);
        $scope.IncidentTitleNew.Sequence = seq;
    }
    // #endregion IncedentTitle
}