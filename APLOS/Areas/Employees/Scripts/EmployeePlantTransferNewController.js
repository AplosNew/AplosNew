'use strict';
EmployeePlantTransferNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$compile', '$window'];

function EmployeePlantTransferNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $compile, $window) {
    $rootScope.title = 'Employee Promotion';
    $scope.path = 'Employees/EmployeePlantTransferNew/';

    $scope.getEmployeeListUrl = $scope.path + 'LoadEmployeelist';
    $scope.PlantWiseEmployeeDetailsUrl = $scope.path + 'PlantWiseEmployeeDetails';
    $scope.SaveDataUrl = $scope.path + 'SaveData';
    $scope.GetBudgetCodeListUrl = $scope.path + 'GetBudgetCodeList';

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.PlantId = null;
    $scope.CompanyId = null;
    $scope.plantList = [];
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }




    $scope.clear = function () {
        
        $scope.CustomPara = {
            EmpSystemId: null,
            PlantId: null,
            JobLocationId: null,
            EffectiveDate: null,
            ShiftId: null,
            BudgetCode: null,
            BudgetCodeId: null
        };
        $scope.NewEmployeeModel = {};
        $scope.JobLocationList = [];
        $scope.ShiftList = [];
        $scope.BCList = [];
        $scope.EmployeeModel = {};
          
    };


    $scope.CustomPara = {
        EmpSystemId: null,
        PlantId: null,
        JobLocationId: null,
        EffectiveDate: null,
        ShiftId: null,
        BudgetCode: null,
        BudgetCodeId: null
    }


    $scope.EmployeeInformationList = [];
    $scope.LoadEmployeeList = function () {
        try {

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();




            $http.get($scope.getEmployeeListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeInformationList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.EmployeeModel = {};
    $scope.SelectEmployee = function () {
        try {

            var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
            $scope.EmployeeModel = gridObj.getSelectedRecords()[0];

            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.close();
            $scope.CompanyId = $scope.EmployeeModel.CompanyId;
            $scope.companyOnChange();
            //$http.get($scope.getSTSCUrl + '?EmpSystemId=' + $scope.EmployeeModel.SystemId)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            $scope.FinalSettlementModel = response.data;
            //            $scope.btnSave = true;
            //        }
            //    },

            //        function errorCallBack(response) {
            //            ShowResult(response.data.Message, 'failure');
            //        });





        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.NewEmployeeModel = {};
    $scope.JobLocationList = [];
    $scope.ShiftList = [];
    $scope.BCList = [];

    $scope.PlantChange = function () {
        try {

            $scope.CustomPara.BudgetCode = null;
            $scope.CustomPara.BudgetCodeId = null;
            $scope.BCList = [];
            $scope.NewEmployeeModel = {};
            $scope.JobLocationList = [];
            $scope.ShiftList = [];


            $scope.CustomPara.EmpSystemId = $scope.EmployeeModel.SystemId;



            $http.get($scope.PlantWiseEmployeeDetailsUrl + '?EmployeeId=' + $scope.EmployeeModel.SystemId + '&PlantId=' + $scope.CustomPara.PlantId)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        $scope.NewEmployeeModel = response.data.data[0];
                        $scope.JobLocationList = response.data.Joblocation;
                        $scope.ShiftList = response.data.shift;

                        //start
                        $http.get($scope.GetBudgetCodeListUrl + '?PlantId=' + $scope.CustomPara.PlantId)
                            .then(function successCallback(response) {
                                if (response.data.Error === true) {
                                    ShowResult(response.data.Message, 'failure');
                                }
                                else {

                                    $scope.BCList = response.data.Rows;
                                }
                            },

                                function errorCallBack(response) {
                                    ShowResult(response.data.Message, 'failure');
                                });
                        //end
                        ///

                        $http({
                            method: 'GET',
                            url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + legalDesignationId
                        }).then(function successCallback(response) {
                            $scope.givenDesignationList = response.data;

                            $scope.employeeNew.GivenDesignationId = response.data[0].Value;


                        })
                        ///

                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.SaveData = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.NewEmployeeModel.LegalDesignation)) {
                throw "Legal Designation is not configuration on this plant.";
            }

            if (baseService.isUndefinedOrNull($scope.NewEmployeeModel.LeavePolicyName)) {
                throw "Leave Policy is not configuration on this plant.";
            }

            if (baseService.isUndefinedOrNull($scope.NewEmployeeModel.SalaryRuleName)) {
                throw "Salary Rule is not configuration on this plant.";
            }

            if (baseService.isUndefinedOrNull($scope.CustomPara.PlantId)) {
                throw "please select plant.";
            }

            if (baseService.isUndefinedOrNull($scope.CustomPara.JobLocationId)) {
                throw "please select Job Location.";
            }
            if (baseService.isUndefinedOrNull($scope.CustomPara.EffectiveDate)) {
                throw "please Enter Effective Date.";
            }
           


            $http({
                method: 'POST',
                url: $scope.SaveDataUrl,
                data: { 'data': $scope.CustomPara, 'olddata': $scope.EmployeeModel, 'newdata': $scope.NewEmployeeModel, 'bc': $scope.BudgetCodeModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.clear();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    $scope.GetGivenDesignationByLegalDesignaiton = function (legalDesignationId) {
        $http({
            method: 'GET',
            url: 'Employees/BudgetCodeChange/GetGivenDesignationByLegalDesignationCbo?legalDesignationId=' + legalDesignationId
        }).then(function successCallback(response) {
            $scope.givenDesignationList = response.data;
            
                $scope.employeeNew.GivenDesignationId = response.data[0].Value;
                
           
        })
        
    };



    $scope.LoadBCList = function () {
        try {

            var eDialog = $("#dialogBC").data("ejDialog");
            eDialog.open();



            //$http.get($scope.getEmployeeListUrl)
            //    .then(function successCallback(response) {
            //        if (response.data.Error === true) {
            //            ShowResult(response.data.Message, 'failure');
            //        }
            //        else {
            //            $scope.EmployeeInformationList = response.data;
            //        }
            //    },

            //        function errorCallBack(response) {
            //            ShowResult(response.data.Message, 'failure');
            //        });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.BudgetCodeModel = {
        Id:null,
        Code:null,
        DesignationSystemID :null,
        UnitId: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        SubdivisionID: null,
        LineId: null,
        EmploymentType: null,
        PositionID: null,
        IsDirect: null

    };
    $scope.SelectBC = function () {
        try {
            
            $scope.CustomPara.BudgetCode = null;
            $scope.CustomPara.BudgetCodeId = null;

            var gridObj = $("#GridBC").data("ejGrid");
            var data = gridObj.getSelectedRecords()[0];

            var eDialog = $("#dialogBC").data("ejDialog");
            eDialog.close();

            //$scope.BudgetCodeModel = data;
            $scope.CustomPara.BudgetCode = data.Code;
            $scope.CustomPara.BudgetCodeId = data.Id;

            $scope.BudgetCodeModel.Code = data.Code;
            $scope.BudgetCodeModel.Id = data.Id;
            $scope.BudgetCodeModel.DesignationSystemID = data.DesignationId;
            $scope.BudgetCodeModel.UnitId = data.UnitId;
            $scope.BudgetCodeModel.DivisionId = data.DivisionId;
            $scope.BudgetCodeModel.DepartmentId = data.DepartmentId;
            $scope.BudgetCodeModel.SectionId = data.SectionId;
            $scope.BudgetCodeModel.SubSectionId = data.SubSectionId;
            $scope.BudgetCodeModel.SubdivisionID = data.SubdivisionID;
            $scope.BudgetCodeModel.LineId = data.LineId;
            $scope.BudgetCodeModel.EmploymentType = data.EmploymentType;
            $scope.BudgetCodeModel.PositionID = data.PositionID;
            $scope.BudgetCodeModel.IsDirect = data.IsDirect;

          




        } catch (e) {
            ShowResult(e, "failure");
        }
    };



}