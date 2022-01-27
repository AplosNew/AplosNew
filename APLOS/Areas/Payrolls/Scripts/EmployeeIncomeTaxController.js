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
        TaxPolicyHeaderId: null
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

    // #region Saving Header Region

    $scope.SaveEmployeeInfoHeader = function () {
        if (!baseService.isUndefinedOrNull($scope.EmployeeIncomeTaxModel.EmpSystemId)) {
            $http({
                method: 'POST',
                url: $scope.path + "Create",
                data: { 'data': $scope.EmployeeIncomeTaxModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   // $scope.getInvestDeductMaster();
                    $scope.EmployeeIncomeTaxModel.Id = response.data.Data.Id;

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

}
