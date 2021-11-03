'use strict';
ReportingGroupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ReportingGroupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Reporting Group';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Setups/ReportingGroup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    //#region Master Part
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;            
            //$scope.GetSequence();
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
        }
        $scope.getDetails($scope.ModelNew.Id);
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
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
                //ClearFields(response.data.Sequence);
                $scope.BudgetCodeTag.ReportGroupId = response.data.Data.Id;
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
        $scope.DetailsList = [];
        $scope.ModelNew.Sequence = seq;
        $scope.BudgetCodeTag = {
            Id: null,
            ReportGroupId: null,
            ManpowerBudgetId: null,

        }
    }
    //#endregion

    //#region TAB
    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };

    //#endregion

    //#region --Budget Code--

    $scope.name = null;
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
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
    $scope.popUp = function (name) {
        $scope.name = name;
        //if ($scope.name === 'Budget') {
        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.popUpParameters.sort = 'Code';
        $scope.popUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'Setups/ReportingGroup/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    //$scope.popUpParameters.sort = 'Code';
                    //$scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();


    };
    $scope.BudgetCodeTag = {
        Id: null,
        ReportGroupId: null,
        ManpowerBudgetId: null,

    }
    $scope.selectDoubleClick = function (data) {
        if ($scope.name === 'Budget') {
            $scope.BudgetCodeTag.ManpowerBudgetId = data.Code;            
            angular.element(document.querySelector('#popUpId')).modal('hide');
            $scope.SaveDetails();
        }
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.SaveDetails = function () {
        try {
            $scope.BudgetCodeTag.ReportGroupId = $scope.ModelNew.Id
            $http({
                method: 'POST',
                url: $scope.path + "SaveDetails",
                data: { 'Details': $scope.BudgetCodeTag },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDetails(response.data.Data.ReportGroupId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.DetailsList = [];
    $scope.getDetails = function (MasterId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetDetailsList",
            data: { Mid: MasterId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DetailsList = response.data;            
        });
    }

    $scope.RemoveDetail = function (obj) {
        $scope.Id = obj.data;
        
        if (!baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }
    $scope.DeleteChild = function () {
        var DeleteId = $scope.Id.Id;
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteDetails?Id=' + DeleteId,
        }).then(function successCallback(response) {

            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetails($scope.Id.ReportGroupId);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };
    //#endregion

}