'use strict';
HRReportMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function HRReportMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'HR Report Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/HRReportMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    // ALL POP UPs
    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // Get Sequence
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            //$scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployee",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeList = response.data;
        });
    }

    $scope.getEmployee();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Category: null,
        SubCategory: null,
        StandardName: null,
        UserName: null,
        ShortName: null,
        Code: null,
        Active: true,
        Remarks: null,
        Category: null,
        SubCategory: null,
        EscalationDays: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.SelEmpList = [];

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    $scope.SelEmpList = [];

    $scope.ChildMasterID = null;
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
                data: {
                    'datas': $scope.ModelNew,
                    'Employee': $scope.SelectedEmployeeId,
                },
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
        $scope.Employee = null
        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            Category: null,
            SubCategory: null,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
            Category: null,
            SubCategory: null,
            EsclationDays: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;

        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];

        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            $scope.EmployeeList[i].isSelected = false;
        }

    }

    $scope.SelectedEmployeeId = null;
    $scope.EmployeeId = null;
    $scope.SelEmployeeInfoList = [];
    $scope.Employee = null;
    $scope.selectEmpDetail = function (e) {

        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.EntityList = [];
    $scope.userMPList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetEntity",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
            

        });
    }
    $scope.GetEntity();

    $scope.BudgetList = [];
    $scope.GetBudget = function () {
       // $scope.CheckedEntity = [];
        var DropDownObj = $("#entityId").data("ejDropDownList");
        var CheckedEntity = DropDownObj.getSelectedValue().split(",");
        $http({
            method: 'POST',
            url: $scope.path + "GetBudgetCode",
            data: { 'entitylist': CheckedEntity},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetList = response.data;

        });
    }

    //$scope.GetEntity = function () {
    //    var DropDownJobLocationListObjP = $("#medicinePurposeId").data("ejDropDownList");
    //    var mdcnPrpsLists = DropDownJobLocationListObjP.getSelectedValue().split(",");
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "GetEntity",
    //        data: { 'medincinepurpose': mdcnPrpsLists },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.CategoryList = response.data;
    //        $scope.ModelNew.Category = $scope.CategoryList;

    //    });
    //}
    
    
}