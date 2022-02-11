'use strict';
EmployeeIncomeTaxController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'fileReader'];
function EmployeeIncomeTaxController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, fileReader) {
    $rootScope.title = 'Employee Income Tax';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Payrolls/EmployeeIncomeTax/';
    $scope.employee = [];

    //#region Tab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion Tab

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
      
        var data = obj.data;
        $scope.TaxPolicyName = null;
        $scope.InvestDeductGridPop = [];
        $scope.EarningGridValue = [];
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
        $scope.EmployeeIncomeTaxModel.EmpSystemId = data.SystemID;
        $scope.getData();       
        $scope.countDate();
        $scope.GetTaxPolicyList();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };


    $scope.EmployeeListTemp = [];
    $scope.saveemployeedata = function (data) {
        var row = data;
        $scope.EmployeeListTemp.push(row);
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

    //#region GetList Functions of Header

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


    // Policy Finding
    $scope.TaxPolicyList = [];
    $scope.GetTaxPolicyList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetTaxPolicy",
            data: {
                'Residence': $scope.EmployeeIncomeTaxModel.CityOfResidence,
                'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId,
                'Gender': $scope.EmployeeInfoModel.GenderID
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.TaxPolicyList = [];
            $scope.TaxPolicyList = response.data;
            $scope.TaxPolicyName = response.data[0].PolicyHeaderName;
            $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId = response.data[0].PolicyHeaderId;
            $scope.EmployeeIncomeTaxModel.StartDate = response.data[0].StartDate;
            $scope.EmployeeIncomeTaxModel.EndDate = response.data[0].EndDate;
            $scope.getInvestDeductionList();
            $scope.getEarningGridList();
            $scope.getNet_EarningFuncn();
            $scope.getTaxableIncomeGridData();
        });
    }

    //#endregion

    // #region Modals Defined

    $scope.EmployeeIncomeTaxModel = {
        Id: null,
        EmpSystemId: null,
        TaxYearId: null,
        TaxTypeId: null,
        CityOfResidence: null,
        CurrentAge: null,
        TaxPolicyHeaderId: null,
        StartDate: null,
        EndDate: null
    }
    
    $scope.EmployeeListTemp = [];  
    $scope.TaxPolicyName = null;

    // #endregion

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


        $scope.EmployeeIncomeTaxModel.CurrentAge = $scope.DurationYear + " Year" +
            $scope.DurationMonth + " Month";
    };

    //#endregion   

    // #region Saving Investment Deduction Function

    $scope.ClearInvestDeduction = function () {
        for (var i = 0; i < $scope.InvestDeductGridPop.length; i++) {
            $scope.InvestDeductGridPop[i].ActualValue = 0;
            $scope.InvestDeductGridPop[i].UserValue = 0;
        }
    }

    $scope.FindMin = function (data) {

        var x = Math.min(data.data.ActualValue, data.data.TaxSavingItemLimit);
        var y = Math.min(data.data.UserValue, x);

        for (var i = 0; i < $scope.InvestDeductGridPop.length; i++)
        {
            if (data.data.IncomeTaxItemChildId == $scope.InvestDeductGridPop[i].IncomeTaxItemChildId)
            {
                $scope.InvestDeductGridPop[i].UserValue = y;
            }
        }
    }

    $scope.SaveInvestDeduction = function () {

        if (!baseService.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.EmpSystemId)) {
            $http({
                method: 'POST',
                url: $scope.path + "SaveInvestDeduction",
                data: {
                    'Masterdata': $scope.EmployeeIncomeTaxModel,
                    'ChildData': $scope.InvestDeductGridPop
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ProcessTaxableIncome();
                    $scope.getInvestDeductionList();                   
               }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        else {
            ShowResult("Please Choose Employee First ...", 'failure');
        }
    };

    // #endregion

    // #region Grid Data Functions

    $scope.InvestDeductGridPop = [];
    $scope.getInvestDeductionList = function () {

        if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetInvestDeductList",
            data: {
                'PolicyHeaderId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
                'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                throw ('Invalid Request!');
            }
            $scope.InvestDeductGridPop = [];
            $scope.InvestDeductGridPop = response.data;

        });
    }

    $scope.EarningGridValue = [];
    $scope.getEarningGridList = function () {

        if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetEarningGridData",
            data: {
                'PolicyId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
                'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId,
                'From': $scope.EmployeeIncomeTaxModel.StartDate,
                'To': $scope.EmployeeIncomeTaxModel.EndDate,
                'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                throw ('Invalid Request!');
            }
            $scope.EarningGridValue = [];
            $scope.EarningGridValue = response.data;

        });
    }

    $scope.SumValue = 0;
    $scope.NetEarningGridPop = [];
    $scope.getNet_EarningFuncn = function () {

        if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetNetEarning",
            data: {
                'PolicyId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
                'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId,
                'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                throw ('Invalid Request!');
            }
            $scope.NetEarningGridPop = [];
            $scope.NetEarningGridPop = response.data;
           
            for (var j = 0; j < $scope.NetEarningGridPop.length; j++)
            {
                $scope.SumValue +=$scope.NetEarningGridPop[j].NetEarning;
            }
        });
    }

    // #endregion

    //#region Attachment 

    $scope.UploadTableName = 'EmployeeInvestmentDeduction';
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
            data: { Id: MasterID }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                for (var i = 0; i < $scope.InvestDeductGridPop.length; i++) {
                    if ($scope.InvestDeductGridPop[i].Id == MasterID) {
                        $scope.InvestDeductGridPop[i].FileName = response.data[0].FileName;
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
    }
    $scope.MasterIdAfterFileSave = null;
    $scope.onBeginUpload = function (args) {
        try {
            var _data = [{ Id: args.model.Id, TableName: $scope.UploadTableName }];
            $scope.MasterIdAfterFileSave = args.model.Id;
            args.data = JSON.stringify(_data);
            $scope.getFileList();
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
      
    // #region Earning Tab Functions

    $scope.SaveEarningData = function () {

        if (!baseService.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.EmpSystemId)) {
            $http({
                method: 'POST',
                url: $scope.path + "SaveEarningData",
                data: {
                    'Masterdata': $scope.EmployeeIncomeTaxModel,
                    'ChildData': $scope.EarningGridValue
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getEarningGridList();
                    $scope.getNet_EarningFuncn();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        else {
            ShowResult("Please Choose Employee First ...", 'failure');
        }
    };

    $scope.ClearEarningData = function () {
        for (var i = 0; i < $scope.EarningGridValue.length; i++) {
            $scope.EarningGridValue[i].OpeningValue = 0;
        }
    }

    // #endregion

    //#region Taxable Income Tab Functions

    $scope.TaxableIncomeGridPopup = [];
    $scope.getTaxableIncomeGridData = function () {

        if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Invalid Request!!');
        }

        $http({
            method: 'POST',
            url: $scope.path + "GetTaxableIncome",
            data: {
                'PolicyId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
                'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId,
                'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                throw ('Invalid Request!');
            }
            $scope.TaxableIncomeGridPopup = [];
            $scope.TaxableIncomeGridPopup = response.data;

        });
    }

    $scope.ProcessTaxableIncome = function () {

        if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
            ShowResult("Please First Configure the Policy !", 'failure');
            throw ('Invalid Request!!');
        }

        if (angular.isUndefinedOrNull($scope.SumValue) || $scope.SumValue==0) {
            throw ('Invalid Request!!');
        }

            $http({
                method: 'POST',
                url: $scope.path + "ProcessTaxableIncome",
                data: {
                    'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId,
                    'PolicyId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
                    'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId,
                    'Earn': $scope.SumValue
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getTaxableIncomeGridData();
                    
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

    }

    //#endregion

    // #region Tax Slab Functions

    //$scope.TaxableIncomeGridPopup = [];
    //$scope.getTaxableIncomeGridData = function () {

    //    if (angular.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId)) {
    //        ShowResult("Please First Configure the Policy !", 'failure');
    //        throw ('Invalid Request!!');
    //    }

    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "GetTaxableIncome",
    //        data: {
    //            'PolicyId': $scope.EmployeeIncomeTaxModel.TaxPolicyHeaderId,
    //            'EmpId': $scope.EmployeeIncomeTaxModel.EmpSystemId,
    //            'YearId': $scope.EmployeeIncomeTaxModel.TaxYearId
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult(response.data.Message, 'failure');
    //            throw ('Invalid Request!');
    //        }
    //        $scope.TaxableIncomeGridPopup = [];
    //        $scope.TaxableIncomeGridPopup = response.data;

    //    });
    //}

    // #endregion
};



