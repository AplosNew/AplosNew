'use strict';
DebitCreditNoteProcessControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DebitCreditNoteProcessControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'DebitCreditNoteProcess';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/AdjustmentNote/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'DebitCreditNoteProcessControlCreate';
    $scope.deleteUrl = $scope.path + 'DebitCreditNoteProcessControlDelete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "Process"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Process', name: "Process" }, { value: 'Type', name: "Type" }, { value: 'Reason', name: "Reason" }, { value: 'DrControl', name: "DrControl" }, { value: 'CrControl', name: "CrControl" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDebitCreditNoteProcessControlList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //for (var i = 0; i < response.data.length; i++) {
            //    response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            //}
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.TypeList = [];
    cboService.getEnumCbo("enum/GetChargesTypeEnumCbo", function (result) {
        $scope.TypeList = result;
    });

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Process: null,
        Type: null,
        Reason: null,
        DrControl: null,
        DrControlId: null,
        CrControl: null,
        CrControlId: null,
        Remarks: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

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
            "name": "RefNo",
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

    $scope.tabType = "";
    $scope.GetCOAICodeList = function (data) {
        $scope.tabType = data;
        $scope.GLUrl1 = "Accounts/glitem/GetAllGLBudgetActivityList";
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

        if ($scope.tabType == 'DrControlId') {
            $scope.ModelNew.DrControl = data.ActivityName;
            $scope.ModelNew.DrControlId = data.BudgetMasterActivityId;
        }

        else if ($scope.tabType == 'CrControlId') {
            $scope.ModelNew.CrControl = data.ActivityName;
            $scope.ModelNew.CrControlId = data.BudgetMasterActivityId;
        }
        $scope.closeCOAICodeListPopUp();
    };
}