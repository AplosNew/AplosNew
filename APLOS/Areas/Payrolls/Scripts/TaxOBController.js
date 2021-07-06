'use strict';
TaxOBController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'fileReader'];
function TaxOBController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, fileReader) {
    $rootScope.title = 'Tax Opening Balance';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/TaxOB/';
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
            url: 'Payrolls/TaxOB/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };

    $scope.leaveApplication = {
        EmployeeCode: null,
        EmpSystemID: null,
        EmployeeName: null,
        LegalDesignation: null,
        DOJ: null,
        DOC: null,
        DOB: null,
    };
    $scope.leaveApplicationNew = Object.assign({}, $scope.leaveApplication);


    $scope.setEmpData = function (obj) {
        $scope.Clear();
        var data = obj.data;

        $scope.leaveApplicationNew.EmployeeCode = data.EmployeeCode;
        $scope.leaveApplicationNew.EmpSystemID = data.SystemID;
        $scope.leaveApplicationNew.EmployeeName = data.EmployeeName;
        $scope.leaveApplicationNew.LegalDesignation = data.LegalDesignation;
        $scope.leaveApplicationNew.DOJ = data.DOJ;
        $scope.leaveApplicationNew.DOC = data.DOC;
        $scope.leaveApplicationNew.DOB = data.DOB;
        $scope.leaveApplicationNew.GenderID = data.GenderID;
        $scope.leaveApplicationNew.Department = data.Department;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.getData($scope.leaveApplicationNew.EmpSystemID);
        $scope.TabShow($scope.leaveApplicationNew.DOJ);
        //$scope.saveemployeedata(obj.data);
        $scope.getMasterData();
        $scope.GetDedInvest();
        $scope.GetIncomeTabValue();
        $scope.GetDedInvestDed();
        $scope.GetTaxableIncomePara();
        $scope.countDate();
        //$scope.getFileList();
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
        $scope.leaveApplication = { SectionId: $scope.leaveApplication.SectionId };
        $scope.leaveApplicationNew = { SectionId: $scope.leaveApplicationNew.SectionId };
        $scope.employeeInfo = [];
        $scope.leaveApplications = [];
        $scope.leaveApplicationNew.LeaveDayType = 'FullDay';
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
        //----------------------GetTaxYear
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxYear',
        }).then(function successCallback(response) {
            $scope.YearList = response.data;
        });
        //----------------------GetTaxType
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
            url: $scope.path + 'GetTabValue?Doj=' + $scope.leaveApplicationNew.DOJ + '&TaxYeadId=' + $scope.ProfessionalTaxOB.TaxYearId,
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
        $scope.leaveApplicationNew.OpeningTaxableIncomeEarned = null;
        $scope.leaveApplicationNew.OpeningTaxPaid = null;
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { 'TaxYear': $scope.ProfessionalTaxOB.TaxYearId, 'TaxType': $scope.ProfessionalTaxOB.TaxTypeId, 'empid': $scope.leaveApplicationNew.EmpSystemID },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //$scope.EmployeeListTemp = response.data;
            if (baseService.arrayLength(response.data) > 0) {

                $scope.leaveApplicationNew.Id = response.data[0].Id;
                $scope.leaveApplicationNew.OpeningTaxableIncomeEarned = response.data[0].OpeningTaxableIncomeEarned;
                $scope.leaveApplicationNew.OpeningTaxPaid = response.data[0].OpeningTaxPaid;
            }
        });
    }
    $scope.getMasterData();

    $scope.Save = function () {
        $scope.leaveApplicationNew.TaxYearId = $scope.ProfessionalTaxOB.TaxYearId;
        $scope.leaveApplicationNew.TaxTypeId = $scope.ProfessionalTaxOB.TaxTypeId;
        //$scope.leaveApplicationNew.OpeningTaxableIncomeEarned;
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'EmpList': $scope.leaveApplicationNew },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData($scope.leaveApplicationNew.EmpSystemID);

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.ClearM = function () {
        ClearMFields();
        return true;
    };

    function ClearMFields() {
        $scope.Action = 'Save';
        $scope.ProfessionalTaxOB.TaxYearId = null;
        $scope.ProfessionalTaxOB.TaxTypeId = null;
        $scope.leaveApplicationNew.EmployeeCode = null;
        $scope.leaveApplicationNew.EmployeeName = null;
        $scope.leaveApplicationNew.DOJ = null;
        $scope.leaveApplicationNew.LegalDesignation = null;
        $scope.leaveApplicationNew.OpeningTaxableIncomeEarned = null;
        $scope.leaveApplicationNew.OpeningTaxPaid = null;
        $scope.leaveApplicationNew.Id = null;
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

    $scope.InvestMentTax = [];
    $scope.IncomeTax = [];
    $scope.DeductionTax = [];

    $scope.GetDedInvest = function () {
        //$scope.EmpDoj = empDoj;
        $http({
            method: 'GET',
            url: $scope.path + 'GetIncomeTaxTransaction?TaxYear=' + $scope.ProfessionalTaxOB.TaxYearId + '&TaxType=' + $scope.ProfessionalTaxOB.TaxTypeId + '&empId=' + $scope.leaveApplicationNew.EmpSystemID,
        }).then(function successCallback(response) {
            $scope.InvestMentTax = response.data;
        });
    };

    $scope.GetIncomeTabValue = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetIncomeTabValue?TaxYear=' + $scope.ProfessionalTaxOB.TaxYearId + '&TaxType=' + $scope.ProfessionalTaxOB.TaxTypeId + '&empId=' + $scope.leaveApplicationNew.EmpSystemID,
        }).then(function successCallback(response) {
            $scope.IncomeTax = response.data;
        });
    };

    $scope.GetDedInvestDed = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetIncomeTaxTransactionDed?TaxYear=' + $scope.ProfessionalTaxOB.TaxYearId + '&TaxType=' + $scope.ProfessionalTaxOB.TaxTypeId + '&empId=' + $scope.leaveApplicationNew.EmpSystemID,
        }).then(function successCallback(response) {
            $scope.DeductionTax = response.data;
        });
    }
    $scope.TaxableIncomePara = [];
    $scope.TaxPolicyName = null;
    $scope.GetTaxableIncomePara = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaxableIncomePara?TaxYear=' + $scope.ProfessionalTaxOB.TaxYearId + '&TaxType=' + $scope.ProfessionalTaxOB.TaxTypeId + '&empId=' + $scope.leaveApplicationNew.EmpSystemID,
        }).then(function successCallback(response) {
            $scope.TaxableIncomePara = response.data;
            if (response.data.length > 0) {
                $scope.TaxPolicyName = response.data[0].TaxPolicyName;
            }
        });
    }

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

    //#region save ---investment / deduction ---
    $scope.InvestMent = [];

    $scope.InvestMent = {
        Id: null,
        EmpSystemId: null,
        TaxYearId: null,
        TaxTypeId: null,
    }

    $scope.SaveDeduction = function () {
        try {
            $scope.InvestMent.TaxYearId = $scope.ProfessionalTaxOB.TaxYearId;
            $scope.InvestMent.TaxTypeId = $scope.ProfessionalTaxOB.TaxTypeId;
            $scope.InvestMent.EmpSystemId = $scope.leaveApplicationNew.EmpSystemID;
            $http({
                method: 'POST',
                url: $scope.path + 'SaveInvestment',
                data: { 'Investment': $scope.InvestMent, 'ChildList': $scope.DeductionTax },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDedInvestDed();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.SaveInvestment = function () {
        try {
            $scope.InvestMent.TaxYearId = $scope.ProfessionalTaxOB.TaxYearId;
            $scope.InvestMent.TaxTypeId = $scope.ProfessionalTaxOB.TaxTypeId;
            $scope.InvestMent.EmpSystemId = $scope.leaveApplicationNew.EmpSystemID;

            $http({
                method: 'POST',
                url: $scope.path + 'SaveInvestment',
                data: { 'Investment': $scope.InvestMent, 'ChildList': $scope.InvestMentTax },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDedInvest();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.SaveIncome = function () {
        try {
            $scope.InvestMent.TaxYearId = $scope.ProfessionalTaxOB.TaxYearId;
            $scope.InvestMent.TaxTypeId = $scope.ProfessionalTaxOB.TaxTypeId;
            $scope.InvestMent.EmpSystemId = $scope.leaveApplicationNew.EmpSystemID;
            $http({
                method: 'POST',
                url: $scope.path + 'SaveInvestment',
                data: { 'Investment': $scope.InvestMent, 'ChildList': $scope.IncomeTax },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetIncomeTabValue();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    $scope.SaveTaxableIncomeEx = function () {
        try {
            $scope.InvestMent.TaxYearId = $scope.ProfessionalTaxOB.TaxYearId;
            $scope.InvestMent.TaxTypeId = $scope.ProfessionalTaxOB.TaxTypeId;
            $scope.InvestMent.EmpSystemId = $scope.leaveApplicationNew.EmpSystemID;
            $http({
                method: 'POST',
                url: $scope.path + 'SaveTaxableIncomeEx',
                data: { 'Investment': $scope.InvestMent, 'ChildList': $scope.TaxableIncomePara },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetTaxableIncomePara();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    //#endregion

    //#region AGE CALCUALTE
    $scope.DurationYear = 0;
    $scope.DurationMonth = 0;
    $scope.countDate = function () {
        var st = new Date($scope.leaveApplicationNew.DOB);
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

    //$scope.onBeginUploadTaxableIncome = function (args) {
    //    try {
    //        var _data = [{ Id: args.model.Id, TableName: $scope.UploadTableNames }];
    //        $scope.MasterIdAfterFileSaves = args.model.Id;
    //        args.data = JSON.stringify(_data);

    //        $http({
    //            method: 'POST',
    //            url: $scope.path + 'UploadAttachment',
    //            dataType: 'JSON',
    //            data: { UploadDefault: , UploadDefault_data: args.data}
    //        }).then(function successCallback(response) {
    //            if (response.data.Error == true) {
    //                ShowResult('error', 'failure');
    //            }
    //            else {
    //                $scope.getTaxableIncomeFileList();
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult('Failed', 'failure');
    //        });

    //    } catch (e) {
    //        args.cancel = true;
    //        ShowResult(e, 'Error');
    //    }
    //}

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
