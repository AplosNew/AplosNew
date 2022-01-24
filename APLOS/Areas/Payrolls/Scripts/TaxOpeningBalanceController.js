'use strict';
TaxOpeningBalanceController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'fileReader'];
function TaxOpeningBalanceController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, fileReader) {
    $rootScope.title = 'Tax Opening Balance';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/TaxOpeningBalance/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    baseService.init($scope.getListUrl);
    $scope.saveBP = $scope.path + 'SaveTaxPolicyPlantWise';
    $scope.employee = [];

    //#region employee Load
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: $scope.path+ 'getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.EmployeeModel = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        LegalDesignation: null,
        DOJ: null,
        DOC: null,
        DOB: null,
    };
    $scope.EmployeeInfoModel = Object.assign({}, $scope.EmployeeModel);


    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;

        $scope.EmployeeInfoModel.EmployeeCode = data.EmployeeCode;
        $scope.EmployeeInfoModel.EmpSystemID = data.SystemID;
        $scope.EmployeeInfoModel.EmployeeName = data.EmployeeName;
        $scope.EmployeeInfoModel.LegalDesignation = data.LegalDesignation;
        $scope.EmployeeInfoModel.DOJ = data.DOJ;
        $scope.EmployeeInfoModel.DOC = data.DOC;
        $scope.EmployeeInfoModel.DOB = data.DOB;
        $scope.EmployeeInfoModel.GenderID = data.GenderID;
        $scope.EmployeeInfoModel.Department = data.Department;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.getData($scope.EmployeeInfoModel.EmpSystemID);
        $scope.TabShow($scope.EmployeeInfoModel.DOJ);
        $scope.getMasterData();
        $scope.GetDedInvest();
        $scope.GetIncomeTabValue();
        $scope.GetDedInvestDed();
       
        $scope.countDate();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.EmployeeModel = { SectionId: $scope.EmployeeModel.SectionId };
        $scope.EmployeeInfoModel = { SectionId: $scope.EmployeeInfoModel.SectionId };
        $scope.employeeInfo = [];
        $scope.EmployeeModels = [];
        $scope.EmployeeInfoModel.LeaveDayType = 'FullDay';
        $scope.LeaveBalanceList = [];
        $scope.LeaveTransactionList = [];
        $scope.imageSrc = virtualPath.EmployeePic + '';
    }

    $scope.EmployeeListTemp = [];
    $scope.saveemployeedata = function (data) {
        //$scope.EmployeeListTemp = [];
        var row = data;
        //if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
        $scope.EmployeeListTemp.push(row);
        //}
        $scope.Back();
    };

    $scope.Back = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#Grid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.employee.length; i++) {
                $scope.employee[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#Grid").data("ejGrid");
        gridObj.refreshContent();
    };

    //#endregion 

    //#region Tax Year

    $scope.YearList = [];
    $scope.TaxTypeList = [];
    $scope.getData = function () {
         $http({
            method: 'GET',
            url: $scope.path + 'GetTaxYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxType',
        }).then(function successCallback(response) {
            $scope.TaxTypeList = response.data;
        });
    }
    $scope.getData();

    $scope.taxb = 0;
    $scope.TabShow = function (empDoj) {
        //$scope.EmpDoj = empDoj;
        $http({
            method: 'GET',
            url: $scope.path + 'GetTabValue?Doj=' + $scope.EmployeeInfoModel.DOJ + '&TaxYeadId=' + $scope.ProfessionalTaxOB.TaxYearId,
        }).then(function successCallback(response) {
            if (response.data.length == 0) {
                $scope.taxb = 1;
                $scope.tab = 3;
            }
            else {
                $scope.taxb = 0;
                $scope.tab = 1;
            }
        });
    }

    //#endregion

    $scope.ProfessionalTaxOB = {
        Id: null,
        EmpSystemId: null,
        TaxYearId: null,
        TaxTypeId: null,
        OpeningTaxableIncomeEarned: null,
        OpeningTaxPaid: null,
    }
    $scope.EmployeeListTemp = [];
    $scope.ProfessionalTaxOB.TaxYearId = null;
    $scope.ProfessionalTaxOB.TaxTypeId = null;
    $scope.getMasterData = function (empId) {
        $scope.EmployeeInfoModel.OpeningTaxableIncomeEarned = null;
        $scope.EmployeeInfoModel.OpeningTaxPaid = null;
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { 'TaxYear': $scope.ProfessionalTaxOB.TaxYearId, 'TaxType': $scope.ProfessionalTaxOB.TaxTypeId, 'empid': $scope.EmployeeInfoModel.EmpSystemID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //$scope.EmployeeListTemp = response.data;
            if (baseService.arrayLength(response.data) > 0) {

                $scope.EmployeeInfoModel.Id = response.data[0].Id;
                $scope.EmployeeInfoModel.OpeningTaxableIncomeEarned = response.data[0].OpeningTaxableIncomeEarned;
                $scope.EmployeeInfoModel.OpeningTaxPaid = response.data[0].OpeningTaxPaid;
            }
        });
    }
    $scope.getMasterData();

    $scope.ClearM = function () {
        ClearMFields();
        return true;
    };

    function ClearMFields() {
        $scope.Action = 'Save';
        $scope.ProfessionalTaxOB.TaxYearId = null;
        $scope.ProfessionalTaxOB.TaxTypeId = null;
        $scope.EmployeeInfoModel.EmployeeCode = null;
        $scope.EmployeeInfoModel.EmployeeName = null;
        $scope.EmployeeInfoModel.DOJ = null;
        $scope.EmployeeInfoModel.LegalDesignation = null;
        $scope.EmployeeInfoModel.OpeningTaxableIncomeEarned = null;
        $scope.EmployeeInfoModel.OpeningTaxPaid = null;
        $scope.EmployeeInfoModel.Id = null;
        $scope.YearList = [];
        $scope.TaxTypeList = [];
    }

    //#region Delete Master

    $scope.RemoveMaster = function (obj) {
        $scope.Id = obj.Id;
        if (!baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete permanently ?';
        angular.element(document.querySelector('#confirmMasterPopUp')).modal('show');
    }
    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "Delete",
            data: { 'Id': $scope.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMasterData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    //#endregion

    
    $scope.TaxPolicyName = null;
   

    $scope.CheckValidation = function (args) {
        if (args.isInteraction == false)
            return;
        if (args.isChecked == false)
            return;

        var optionBase = '';
        for (var i = 0; i < $scope.TaxableIncomePara.length; i++) {
            if (args.model.id == $scope.TaxableIncomePara[i].TaxFormulaId) {
                optionBase = $scope.TaxableIncomePara[i].OptionBase;
                break;
            }
        }

        for (var i = 0; i < $scope.TaxableIncomePara.length; i++) {
            if (optionBase == $scope.TaxableIncomePara[i].OptionBase) {
                if (args.model.id == $scope.TaxableIncomePara[i].TaxFormulaId)
                    continue;

                $scope.TaxableIncomePara[i].IsSelect = false;
                break;
            }
        }

        var gridObj = $("#Grid3").data("ejGrid");
        gridObj.refreshContent();
    }  

    //#region AGE CALCUALTE
    $scope.DurationYear = 0;
    $scope.DurationMonth = 0;
    $scope.countDate = function () {
        var st = new Date($scope.EmployeeInfoModel.DOB);
        var ed = new Date();

        var nowyear = ed.getFullYear();
        var nowmonth = ed.getMonth() + 1;
        var nowday = ed.getDate();

        var styear = st.getFullYear();
        var stmonth = st.getMonth() + 1;
        var stday = st.getDate();

        var age = nowyear - styear;
        var age_month = nowmonth - stmonth;
        var age_day = nowday - stday;

        if (age_month < 0 || age_month === 0 && age_day < 0) {
            age = parseInt(age) - 1;
            age_month += 12;
        }
        if (age_month === 12) {
            age_month = 0;
            age = age + 1;
        }

        $scope.DurationYear = age;
        $scope.DurationMonth = age_month;

    };

    //#endregion

    //#region Attachment 

    $scope.UploadTableName = 'IncomeTaxItemTransaction';
    $scope.uploadUrl = $scope.path + "UploadAttachment/";
    $scope.confirmFileDelete = function () {
        angular.element(document.querySelector("#confirmFileDelete")).modal("show");
    }
    $scope.getFileList = function () {
        var MasterID = '';
        if (!baseService.isUndefinedOrNull($scope.MasterIdAfterFileSave))
            MasterID = $scope.MasterIdAfterFileSave;
        else
            MasterID = $scope.MasterId
        $http({
            method: 'POST', url: $scope.path + 'GetFileInfo', dataType: 'JSON',
            data: { Id: MasterID/*$scope.InvestMent.TaxYearId, taxtype: $scope.InvestMent.TaxTypeId, empsysteid: $scope.InvestMent.EmpSystemId*/ }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                for (var i = 0; i < $scope.DeductionTax.length; i++) {
                    if ($scope.DeductionTax[i].Id == MasterID) {
                        $scope.DeductionTax[i].FileName = response.data[0].FileName;
                        break;
                    }
                }
                for (var i = 0; i < $scope.IncomeTax.length; i++) {
                    if ($scope.IncomeTax[i].Id == MasterID) {
                        $scope.IncomeTax[i].FileName = response.data[0].FileName;
                        break;
                    }
                }
                for (var i = 0; i < $scope.InvestMentTax.length; i++) {
                    if ($scope.InvestMentTax[i].Id == MasterID) {
                        $scope.InvestMentTax[i].FileName = response.data[0].FileName;
                        break;
                    }
                }
                for (var i = 0; i < $scope.TaxableIncomePara.length; i++) {
                    if ($scope.TaxableIncomePara[i].Id == MasterID) {
                        $scope.TaxableIncomePara[i].FileName = response.data[0].FileName;
                        break;
                    }
                }
                $scope.MasterId = null;
                $scope.MasterIdAfterFileSave = null;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.errorUpload = function (e) {
        ShowResult(e.error, 'failure');
        //    ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.MasterIdAfterFileSave = null;
    $scope.onBeginUpload = function (args) {
        try {
            var _data = [{ Id: args.model.Id, TableName: $scope.UploadTableName }];
            $scope.MasterIdAfterFileSave = args.model.Id;
            args.data = JSON.stringify(_data);
        } catch (e) {
            args.cancel = true;
            ShowResult(e, 'Error');
        }
    }
    $scope.MasterId = null;
    $scope.confirmFileDelete = function (args) {
        $scope.MasterId = args.data.Id;
        angular.element(document.querySelector("#confirmFileDelete")).modal("show");
    }
    $scope.DeleteFile = function () {
        try {
            $http({
                method: 'POST', url: $scope.path + 'DeleteFile', dataType: 'JSON',
                data: { Id: $scope.MasterId, TableName: $scope.UploadTableName }

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.getFileList();
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    //#endregion

    //#region Taxable Income Parameter Attachment
    $scope.UploadTableNames = 'TaxableIncomeparameter';
    $scope.getTaxableIncomeFileList = function () {
        var MasterIDs = '';
        if (!baseService.isUndefinedOrNull($scope.MasterIdAfterFileSaves))
            MasterIDs = $scope.MasterIdAfterFileSaves;
        else
            MasterIDs = $scope.MasterIds
        $http({
            method: 'POST', url: $scope.path + 'GetTaxableIncomeFileInfo', dataType: 'JSON',
            data: { Id: MasterIDs/*$scope.InvestMent.TaxYearId, taxtype: $scope.InvestMent.TaxTypeId, empsysteid: $scope.InvestMent.EmpSystemId*/ }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                for (var i = 0; i < $scope.TaxableIncomePara.length; i++) {
                    if ($scope.TaxableIncomePara[i].Id == MasterIDs) {
                        $scope.TaxableIncomePara[i].FileName = response.data[0].FileName;
                        break;
                    }
                }
                $scope.MasterIds = null;
                $scope.MasterIdAfterFileSaves = null;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.MasterIdAfterFileSaves = null;
    $scope.onBeginUploadTaxableIncome = function (args) {
        try {
            var _data = [{ Id: args.model.Id, TableName: $scope.UploadTableNames }];
            $scope.MasterIdAfterFileSaves = args.model.Id;
            args.data = JSON.stringify(_data);
        } catch (e) {
            args.cancel = true;
            ShowResult(e, 'Error');
        }
    }

    $scope.MasterIds = null;
    $scope.confirmTaxableIncomeFileDelete = function (args) {
        $scope.MasterIds = args.data.Id;
        angular.element(document.querySelector("#confirmFileDeletes")).modal("show");
    }
    $scope.DeleteInfoFile = function () {
        try {
            $http({
                method: 'POST', url: $scope.path + 'DeleteFile', dataType: 'JSON',
                data: { Id: $scope.MasterIds, TableName: $scope.UploadTableNames }

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.getTaxableIncomeFileList();
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    //#endregion

}
