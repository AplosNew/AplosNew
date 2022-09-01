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
        $http.get("Banks/BankReconciliation/GetBankReconLastDate?bankMasterId=" + id)
            .then(function (response) {

                $scope.bankReconciliationNew.FromDate = response.data.FromDate;
                $scope.bankReconciliationNew.OpeningBlance = response.data.ClosingBalance;
                $scope.bankReconciliationNew.ClosingBalance = null;
                $scope.bankReconciliationNew.BankStatementNo = null;
                if ($scope.bankReconciliationNew.FromDate == null)
                    $scope.bankReconciliationNew.FromDate = $filter("dateFiltering")($scope.cutOffDate);
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

            if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                throw "Please Select Year.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo) || $scope.ModelNew.MonthNo == 0) {
                throw "Please Select Month.";
            }
            $http({
                method: 'GET',
                url: $scope.path + 'LoadData?SalaryHeadId=' + $scope.ModelNew.SalaryHeadId + '&MonthNo=' + $scope.ModelNew.MonthNo + '&YearNo=' + $scope.ModelNew.YearNo

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


    $scope.AttdnRawData = [];
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

    $scope.ModelNew = {
        Id: null,
        FileName: null,
        SalaryHeadId: null,
        YearNo: null,
        MonthNo: null

    };
    

    $scope.ShowSaveBtn = false;

    $scope.ModelNew.YearNo = new Date().getFullYear().toString();
    $scope.ModelNew.MonthNo = new Date().getMonth().toString();

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                if (!baseService.isUndefinedOrNull($scope.picdata)) {
                    $scope.ModelNew.FileName = $scope.picdata.name;
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadId)) {
                    throw "Please Select Salary Head.";
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                    throw "Please Select Year.";
                }

                if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo)) {
                    throw "Please Select Month.";
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
                        'modelNew': $scope.ModelNew
                        , 'file': $scope.picdata
                        //, 'pSalaryHeadId': $scope.ModelNew.SalaryHeadId 
                        //, 'pYearNo': $scope.ModelNew.YearNo 
                        //, 'pMonthNo': $scope.ModelNew.MonthNo 
                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.AttdnRawData = [];
                        $scope.AttdnRawData = response.data;
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


    $scope.onrowdatabound = function (e) {
        if (e.data.Remarks !== '') {
            if (e.data.Remarks == 'Salary has been locked')
                e.row.css("background-color", "yellow");
            else
                e.row.css("background-color", "red");
        }

    };



    $scope.save = function () {

        try {
            for (var i = 0; i < $scope.AttdnRawData.length; i++) {

                if ($scope.AttdnRawData[i].Remarks !== '') {
                    if ($scope.AttdnRawData[i].Remarks !== 'Salary has been locked')
                        throw "Please Upload valid data";
                }

            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.SalaryHeadId)) {
                throw "Please Select Salary Head.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.YearNo)) {
                throw "Please Select Year.";
            }

            if (baseService.isUndefinedOrNull($scope.ModelNew.MonthNo)) {
                throw "Please Select Month.";
            }

            $.ajax({
                type: "POST",
                url: $scope.path + 'SaveExternalData',
                data: {
                    'data': $scope.AttdnRawData
                    , 'SalaryHeadId': $scope.ModelNew.SalaryHeadId
                    , 'YearNo': $scope.ModelNew.YearNo
                    , 'MonthNo': $scope.ModelNew.MonthNo
                },
                dataType: "json",
                success: function (response) {


                    if (response.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.AttdnRawData = [];
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

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath
    $scope.DownloadReport = function () {
        try {
            var MonthName = "";
            $scope.fileName = "ExternalDataUploadFromExcel.xls";
            if ($scope.SaveDataList.length == 0) {
                throw "Load Data first..";
            }
            var parameters = [];
            var gridObj = $("#GridUploadedData").data("ejGrid");
            var filteredRecords = gridObj.getFilteredRecords();
            if (filteredRecords.length == 0) {
                filteredRecords = $scope.SaveDataList;
            }

            parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
            parameters.push({ "Key": "SalaryHeadID", "Value": getString(filteredRecords, "SalaryHeadID") });
            parameters.push({ "Key": "HeadType", "Value": getString(filteredRecords, "HeadType") });
            parameters.push({ "Key": "EntryCurrencyID", "Value": getString(filteredRecords, "EntryCurrencyID") });
            parameters.push({ "Key": "EntryAmount", "Value": getString(filteredRecords, "EntryAmount") });

            for (var i = 0; i < $scope.monthList.length; i++) {
                if ($scope.ModelNew.MonthNo == $scope.monthList[i].Value) {
                    MonthName = $scope.monthList[i].Text;
                }
            }

            $http({
                method: 'POST',
                url: 'Payrolls/ExternalDataUploadFromExcel/ExternalDataUploadReport',
                data: {
                    'EmployeeList': parameters[0].Value, 'SalaryHeadId': $scope.ModelNew.SalaryHeadId, 'MonthNo': $scope.ModelNew.MonthNo, 'YearNo': $scope.ModelNew.YearNo
                    , 'SalaryHeadIDs': parameters[1].Value, 'HeadType': parameters[2].Value, 'CurrencyID': parameters[3].Value, 'EntryAmount': parameters[4].Value, 'MonthName': MonthName
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);//downloadgriddataUrlPath
                }
            });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    var getString = function (data, column) {
        var kk = "";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                if (kk === "") {
                    kk += "'" + data[i][column] + "'";
                }
                else {
                    kk += ",'" + data[i][column] + "'";
                }

                collection.push(data[i][column]);
            }
        }
        return kk;
    };
    $scope.ShowDiv = false;
    $scope.edit = {
        Id: null,
        EmpCode: null,
        SalaryHead: null,
        EmpName: null,
        Amount: null,
    }
    $scope.Edit = function (obj) {
        $scope.edit.Id = obj.data.SystemID;
        $scope.edit.EmpCode = obj.data.EmployeeCode;
        $scope.edit.SalaryHead = obj.data.SalaryHead;
        $scope.edit.EmpName = obj.data.EmployeeName;
        $scope.edit.Amount = obj.data.EntryAmount;

        $scope.salaryLockCheck(obj.data.EmpSystemId);

    };
    $scope.IsSalaryLock = false;
    $scope.salaryLockCheck = function (EmpSystemId) {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'GetSalaryLock?EmpSystemId=' + EmpSystemId + '&MonthNo=' + $scope.ModelNew.MonthNo + '&YearNo=' + $scope.ModelNew.YearNo,
            }).then(function successCallback(response) {
                if (response.data.length != 0) {
                    if (response.data[0].IsLocked == true) {
                        ShowResult("Salary Locked for this Employee..", "failure");
                    }
                    else {
                        $scope.ShowDiv = true;
                        var eDialog = $("#Edit").data("ejDialog");
                        $("#Edit").ejDialog("setTitle", " Edit");
                        eDialog.open();
                    }
                }
                else {

                    /*if ($scope.IsSalaryLock == false) {*/
                    $scope.ShowDiv = true;
                    var eDialog = $("#Edit").data("ejDialog");
                    $("#Edit").ejDialog("setTitle", " Edit");
                    eDialog.open();
                    //}
                }
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.UpdateUpload = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + "UpdateUpload",
                data: { 'ExternalUploadUpdate': $scope.edit },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    var eDialog = $("#Edit").data("ejDialog");
                    eDialog.close();
                    $scope.LoadData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
}





