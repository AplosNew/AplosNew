'use strict';
bankReconciliationDataUploadController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService', 'fileReader', "$controller", "$filter"];
function bankReconciliationDataUploadController($scope, $http, $location, $rootScope, $window, $compile, baseService, fileReader, $controller, $filter) {
    $scope.path = 'banks/bankreconciliation/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $rootScope.title = 'Bank Reconciliation Data Upload';
    $controller("bankBaseController", { $scope: $scope, $http: $http });
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    $scope.SaveDataList = []

    $scope.bankReconciliation = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        CompanyId: $window.companyId,
        BankMasterId: null,
        BankName: null,
        BankBranch: null,
        BankAccount: null,
        BankGL: null,
        BankCurrency: null,
        OpeningBlance: null,
        ClosingBalance: null,
        BankStatementNo: null,
        EmployeeName: null,
        EmployeeId: null,
        FromDate: null,
        ToDate: $filter("dateFiltering")(Date.now())
    };
    $scope.bankReconciliationNew = Object.assign({}, $scope.bankReconciliation);
    $scope.getCutOffDate = function () {
        $http.get("Accounts/OpeningBalance/GetACCCutOffDate")
            .then(function (response) {
                if (response.data !== null) {
                    $scope.cutOffDate = $filter("dateFiltering")(response.data.CutOffDate);
                }
                else {
                    ShowResult("Opening Balance Cut Off date not found!", "failure");
                }
            });
    }
    $scope.getCutOffDate();

    $scope.selectBankPopUp = function (index, id) {
        $scope.bankIndex = index;
        $scope.selectedBank = id;
    };

    $scope.closeBankPopUp = function () {
        if ($scope.bankIndex !== -1) {
            selectBankRow();
        }
        angular.element(document.querySelector("#bankPopUp")).modal("hide");
        $scope.bankIndex = -1;
    };

    function selectBankRow() {
        var bank = $scope.bankList[$scope.bankIndex];
        if (bank.GLGeneralInfoId === null) {
            ShowResult("Bank GL not found!", "failure");
        }
        else if (bank.CurrencyId === null) {
            ShowResult("Bank Transaction Currency not found!", "failure");
        }
        else {
            $scope.bankReconciliationNew.BankMasterId = bank.BankMasterId;
            $scope.bankReconciliationNew.BankName = bank.BankName;
            $scope.bankReconciliationNew.BankAccount = bank.AccountTitle;
            $scope.bankReconciliationNew.BankGL = bank.GLItem;
            $scope.bankReconciliationNew.BankCurrency = bank.CurrencyCode;
            $scope.bankReconciliationNew.BankBranch = bank.BankBranchName;
            $scope.bankReconciliationNew.GLGeneralInfoId = bank.GLGeneralInfoId;
            $scope.bankReconciliationNew.BankGL = bank.GLGeneralInfoId + ' - ' + bank.GLGeneralInfoName;
            $scope.getBankReconLastDate($scope.bankReconciliationNew.BankMasterId);
           // $scope.clear();
        }
    }
    $scope.getBankReconLastDate = function (id) {
        $http.get("Banks/BankReconciliation/GetBankReconUploadLastDate?bankMasterId=" + id)
            .then(function (response) {

                $scope.bankReconciliationNew.FromDate = response.data.FromDate;
                $scope.bankReconciliationNew.OpeningBlance = response.data.ClosingBalance;
                $scope.bankReconciliationNew.ClosingBalance = null;
                $scope.bankReconciliationNew.BankStatementNo = null;
                if ($scope.bankReconciliationNew.FromDate == null)
                    $scope.bankReconciliationNew.FromDate = $filter("dateFiltering")(Date.now());
            });
    }
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.bankReconciliationNew.EmployeeName = employee.EmployeeName;
            $scope.bankReconciliationNew.EmployeeId = employee.SystemId;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };
    $scope.clearEmployee = function () {
        $scope.bankReconciliationNew.EmployeeName = null;
        $scope.bankReconciliationNew.EmployeeId = null;
    };

    $scope.LoadData = function () {
        try {

            if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.BankMasterId)) {
                throw "Please Select Bank.";
            }
            $http({
                method: 'GET',
                url: $scope.path + 'LoadBankReconciliationUploadedData?bankMasterId=' + $scope.bankReconciliationNew.BankMasterId 

            }).then(function successCallback(response) {
                if (response.data.Error === true) {

                    ShowResult(response.data.Message, "failure");

                }
                else {
                    $scope.SaveDataList = [];
                    $scope.SaveDataList = response.data;

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };


    $scope.tabh = 11;
    $scope.setTab11 = function (newTab) {
        $scope.tabh = newTab;
        $scope.employees = [];

    };
    $scope.isSet11 = function (tabNum) {
        return $scope.tabh === tabNum;
    };
    $scope.setTab22 = function (newTab) {
        $scope.tabh = newTab;

    };
    $scope.isSet22 = function (tabNum) {
        return $scope.tabh === tabNum;
    };


    $scope.BankReconciliationUploadedData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ShowSaveBtn = false;

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.bankReconciliationNew.FileName = $scope.picdata.name;
                }

                if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.BankMasterId)) {
                    throw "Please Select Bank.";
                }

                if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.BankStatementNo)) {
                    throw "Please Select Bank StatementNo.";
                }

                if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.EmployeeId)) {
                    throw "Please Select By Whom.";
                }
                if (new Date($scope.bankReconciliationNew.ToDate) < new Date($scope.bankReconciliationNew.FromDate)) {
                    throw "To date must be below or equal to From Date!";
                }


                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'modelNew': $scope.bankReconciliationNew
                        , 'file': $scope.picdata
                       
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.BankReconciliationUploadedData = [];
                        $scope.BankReconciliationUploadedData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.invalidDocDate = false;
    $scope.checkDocDate = function () {
        var msg = "";
        //if (new Date($scope.voucher.ToDate) > new Date()) {
        //    $scope.invalidDocDate = true;
        //    msg = "Doc date must be below or equal to current Date!";
        //}
        if (new Date($scope.bankReconciliationNew.ToDate) < new Date($scope.bankReconciliationNew.FromDate)) {
            msg = "To date must be below or equal to From Date!";
            $scope.invalidDocDate = true;
        }

        else $scope.invalidDocDate = false;

        return manualValidation("div_ToDate", $scope.invalidDocDate, msg);
    };

    $scope.save = function () {

        try {
            if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.BankMasterId)) {
                throw "Please Select Bank.";
            }

            if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.BankStatementNo)) {
                throw "Please Select Bank StatementNo.";
            }

            if (baseService.isUndefinedOrNull($scope.bankReconciliationNew.EmployeeId)) {
                throw "Please Select By Whom.";
            }

            $.ajax({
                type: "POST",
                url: $scope.path + 'SaveBankReconciliationUploadData',
                data: {
                    'bankReconciliationUploadvm': $scope.bankReconciliationNew
                    ,'bankReconciliationUploadedDataList': $scope.BankReconciliationUploadedData
                    
                   
                },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {
                        $scope.ShowSaveBtn = true;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.LoadData();
                        $scope.BankReconciliationUploadedData = [];
                        $scope.bankReconciliationNew = {};
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.onClickReportDownloadExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'GetBankReconciliationUploadedDataReport?reportFormat=' + reportFormat + '&bankReconciliationUploadId=' + data.Id, '_blank');
    };

    $scope.onClickReportDownloadforMailExcel = function (data) {
        var reportFormat = "Excel";
        if (baseService.isUndefinedOrNull(data.Id)) return ShowResult('No Id found', 'failure');
        $window.open($scope.path + 'GetBankReconciliationUploadedDataForMailReport?reportFormat=' + reportFormat, '_blank');
    };

    $scope.onClickDeletePopUp = function (x) {
        var data = x;
        $scope.bankReconciliationUploadId = data.Id;
        $scope.message_delete_confirmation = "Are you sure to Delete?";
        angular.element(document.querySelector('#confirmDeletePopUp')).modal('show');
    };
    $scope.delete = function (bankReconciliationUploadId) {
        $http({
            method: "POST",
            url: $scope.path + 'DeleteBankReconciliationUploadedData',
            data: {
                "bankReconciliationUploadId": bankReconciliationUploadId
            },
            dataType: "JSON"
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
        return true;
    };
}





