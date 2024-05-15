'use strict';
InputCreditController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function InputCreditController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Input Credit';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/Sales/';
    $scope.getListUrl = $scope.path + 'getinputcreditlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'createinputcredit';
    $scope.deleteUrl = $scope.path + 'deleteinputcredit/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getinputcreditlist",
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
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.checkdByList = [];
    $scope.GetcheckByCboList = function () {
        $http({
            method: 'GET',
            url: 'Payrolls/FinalSettlement/GetApprovedByCbo'
        }).then(function successCallback(response) {
            $scope.approvedByList = response.data;
            if (baseService.arrayLength($scope.approvedByList) == 1) {
                $scope.FinalSettlementModel.ApproveById = $scope.approvedByList[0].Value;
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
}