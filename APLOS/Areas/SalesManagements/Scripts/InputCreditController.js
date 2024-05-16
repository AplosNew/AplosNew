'use strict';
InputCreditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function InputCreditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Input Credit';
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

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getinputcreditlist",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
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
        UserRef: null,
        MonthNo: null,
        FromDate: null,
        ToDate: null,
        ResponsiblePersonId: null,
        CheckById: null,
        CheckByStatus: 'To Be Checked',
        ApproveById: null,
        ApproveByStatus: null,
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

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.checkedByList = [];
    $scope.GetcheckByCboList = function () {
        $http({
            method: 'GET',
            url: 'SalesManagements/Sales/GetCheckByCbo'
        }).then(function successCallback(response) {
            $scope.checkedByList = response.data;
            if (baseService.arrayLength($scope.checkedByList) == 1) {
                $scope.ModelNew.CheckById = $scope.checkedByList[0].Value;
            }
        });
    }
    $scope.GetcheckByCboList();

    $scope.monthList = [
        { Value: 1, Text: 'Jan' },
        { Value: 2, Text: 'Feb' },
        { Value: 3, Text: 'Mar' },
        { Value: 4, Text: 'Apr' },
        { Value: 5, Text: 'May' },
        { Value: 6, Text: 'Jun' },
        { Value: 7, Text: 'Jul' },
        { Value: 8, Text: 'Aug' },
        { Value: 9, Text: 'Sep' },
        { Value: 10, Text: 'Oct' },
        { Value: 11, Text: 'Nov' },
        { Value: 12, Text: 'Dec' }
    ];

    $scope.popUpDataList = [];
    $scope.showResponsiblePersonListPopUp = function () {
        try {
            $scope.Name = name;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/leaveApplication/getemployeelist'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        var data = arg.data;

        $scope.ModelNew.ResponsiblePersonId = data.SystemID;
        $scope.ModelNew.ResponsiblePerson = data.EmployeeName;

        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.materialList = [];
    $scope.GetMaterialList = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.ModelNew.FromDate)) {
                throw "Select From Date";
            }
            if (baseService.isUndefinedOrNull($scope.ModelNew.ToDate)) {
                throw "Select To Date";
            }
            $http({
                method: 'GET',
                url: 'SalesManagements/Sales/GetSalesMaterialDataList?fromDate=' + $scope.ModelNew.FromDate + '&toDate=' + $scope.ModelNew.ToDate
            }).then(function successCallback(response) {
                $scope.materialList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridSM").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.materialList.length; i++) {
                $scope.materialList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridSM").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.ModelNew.FromDate = $filter('dateFiltering')(new Date($scope.ModelNew.FromDate ), 'dd-MM-yyyy');
        $scope.ModelNew.ToDate = $filter('dateFiltering')(new Date($scope.ModelNew.ToDate ), 'dd-MM-yyyy');
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
                    $scope.ModelNew.Id = response.data.Data.Id;
                    //ClearFields(response.data.Sequence);
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
}