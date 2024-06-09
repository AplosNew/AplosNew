'use strict';
SalesProcessController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function SalesProcessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sales Process';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'SalesManagements/Sales/';
    $scope.getListUrl = $scope.path + 'getSalesProcesslist';
    $scope.getSeqUrl = $scope.path + 'GetSalesProcessAutoSequence';
    $scope.saveUrl = $scope.path + 'CreateSalesProcess';
    $scope.deleteUrl = $scope.path + 'deleteSalesProcess/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'SalesProcess', name: "Sales Process" }, { value: 'Sequence', name: "Sequence" }, { value: 'Remarks', name: "Remarks" }];

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
            url: $scope.path + "getSalesProcesslist",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            $scope.GetSequence();
        });
    }
    $scope.getData();
    $scope.SPList = [];
    $scope.getSPData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetSalesProcessTransactionList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SPList = response.data;
        });
    }

    $scope.CheckBankApplicable = function (obj) {
        if (obj.data.IsBankApplicable == false) {
            $scope.rowdata.BankMasterId = null;
            $scope.rowdata.BankName = null;
            $scope.rowdata.AccountNumber = null;
            var gridObj = $("#GridSalPT").data("ejGrid");
            gridObj.refreshContent();
            gridObj.refreshTemplate();
        }
    }

    $scope.rowdata = {};
    $scope.BankInfolist = [];
    $scope.GetBankInfo = function (obj) {
        try {
            $scope.rowdata = obj.data;
            if ($scope.rowdata.IsBankApplicable == true) {
                $http.get('SalesManagements/Sales/GetBankMaster')
                    .then(
                        function successCallback(response) {
                            if (baseService.arrayLength(response.data) > 0) {
                                $scope.BankInfolist = response.data;
                            }
                        },
                        function errorCallback(response) {
                            ShowResult(response, 'failure');
                        });
                angular.element(document.querySelector('#BankInFoPopUp')).modal('show');
            }
            else {
                throw "Select Bank Applicable.";
            }

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SetBankData = function (obj) {
        $scope.rowdata.BankMasterId = obj.data.Id;
        $scope.rowdata.BankName = obj.data.BankName;
        $scope.rowdata.AccountNumber = obj.data.AccountNumber;
        var gridObj = $("#GridSalPT").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');
    };
    $scope.closeBankInfoPopUp = function (obj) {
        angular.element(document.querySelector('#BankInFoPopUp')).modal('hide');
    };

    $scope.DepartmentList = [];
    $scope.showDepartmentPopUp = function (obj) {
        try {
            $scope.rowdata = obj.data;

            $http({
                method: 'GET',
                url: 'SalesManagements/Sales/GetDepartment'
            }).then(function successCallback(response) {
                $scope.DepartmentList = response.data;
            });
            angular.element(document.querySelector('#DPTPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SetDPTData = function (obj) {
        $scope.rowdata.DepartmentId = obj.data.Id;
        $scope.rowdata.Department = obj.data.UserName;
        var gridObj = $("#GridSalPT").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
        angular.element(document.querySelector('#DPTPopUp')).modal('hide');
    };
    $scope.closeDPTPopup = function (obj) {
        angular.element(document.querySelector('#DPTPopUp')).modal('hide');
    };

    $scope.popUpDataList = [];
    $scope.showResponsiblePersonListPopUp = function (obj) {
        try {
            $scope.rowdata = obj.data;
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
        $scope.rowdata.ResponsiblePersonId = data.SystemID;
        $scope.rowdata.ResponsiblePerson = data.EmployeeName;
        var gridObj = $("#GridSalPT").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        SalesProcess: null,
        Remarks: null,
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
                    $scope.ModelNew.Id = response.data.Data.Id;
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
        $scope.materialList = [];
    }


    $scope.SaveSPT = function () {
      
            $http({
                method: 'POST',
                url: 'SalesManagements/Sales/CreateSalesProcessTran',
                data: { 'data': $scope.SPList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSPData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };







}