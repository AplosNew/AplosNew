'use strict';
AdditionalPayDayController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function AdditionalPayDayController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Additional Pay Day';
    $scope.path = 'Attendances/AdditionalPayDay/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'SaveD';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';

    $scope.AdditionalPayDayMasterModel = {
        Id: null,
        PolicyName: null,
        PolicyDescription: null,
        PlantId: null,
        IsActive: true,
        CompanyId: null
    };

    $scope.plantList = [];
    $scope.CustomPara = {
        CompanyId: null,
        PlantId: null
    };

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.CustomPara.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }
    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#AttendanceBonusD").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#AttendanceBonusD").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };

    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadData = function () {
        $scope.ModelList = [];
        $http.get('Attendances/AdditionalPayDay/GetSalaryHeadListeList?masterid=' + $scope.AdditionalPayDayMasterModel.Id)
            .then(function (response) {
                $scope.SalaryHeadList = response.data;

            });
    };
    
    $scope.ModelList = [];
    $scope.getData = function () {
        $scope.ModelList = [];
        $scope.AdditionalPayDayDetailsList = [];
        $http.get('Attendances/AdditionalPayDay/GetList?PlantId=' + $scope.AdditionalPayDayMasterModel.PlantId)
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();
    
    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#EmpGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.SalaryHeadList.length; i++) {
                $scope.SalaryHeadList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#EmpGrid").data("ejGrid");
        gridObj.refreshContent();
    };

  
   
    $scope.OpenAdditionalPolicyDialog = function () {
        $scope.getLeaveTypeList();
        var eDialog = $("#dialogPFSetting").data("ejDialog");
        eDialog.open();
    };
    
    $scope.AdditionalPayDayDetailsModel = {
        Id: null,
        SalaryHeadId: null,
        HolidayPayDayMasterId: null,
        MID: null,
    };
   
    $scope.AdditionalPayDayDetailsList = [];
    $scope.getDetails = function () {
        $http.get('Attendances/AdditionalPayDay/GetDetailsList?MasterId=' + $scope.AdditionalPayDayMasterModel.Id + '&PlantId=' + $scope.AdditionalPayDayMasterModel.PlantId)
            .then(function (response) {
                $scope.AdditionalPayDayDetailsList = response.data;

            });
    };

    $scope.DetailsId = null;
    $scope.SaveDetails = function () {
        try {

            var NewdataList = [];
            for (var i = 0; i < $scope.SalaryHeadList.length; i++) {
                if ($scope.SalaryHeadList[i].CheckBoxSelect == true) {
                    NewdataList.push($scope.SalaryHeadList[i]);
                }
            }
            if (NewdataList.length <= 0) {
                throw "Select Atleast One..";
            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Details': NewdataList, 'MasterId': $scope.AdditionalPayDayDetailsModel.MID},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                    $scope.getDetails();
                    var eDialog = $("#dialogAdditionalPayDay").data("ejDialog");
                    eDialog.close();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.MasterId = null;
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveMUrl,
                data: { 'Master': $scope.AdditionalPayDayMasterModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.AdditionalPayDayDetailsModel.MID = response.data.MasterId;
                    $scope.AdditionalPayDayMasterModel.Id = response.data.MasterId;
                    $scope.getData();
                    $scope.getDetails();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridAdditionalPayDay").data("ejGrid");
        $scope.AdditionalPayDayMasterModel = gridObj.getSelectedRecords()[0];

        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
        }
        $scope.getDetails($scope.AdditionalPayDayMasterModel.Id);
        $scope.AdditionalPayDayDetailsModel.MID=$scope.AdditionalPayDayMasterModel.Id;
    };

    $scope.recorddoubleclickDetails = function () {
        try {

        } catch (e) {

        }
    };

    $scope.DeleteMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.AdditionalPayDayMasterModel.Id)) {
            $http.get('Attendances/AdditionalPayDay/DeleteM?MID=' + $scope.AdditionalPayDayMasterModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ClearM($scope.AdditionalPayDayMasterModel);
                        $scope.Clear();
                        $scope.getData();
                        $scope.AdditionalPayDayDetailsList = [];
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.DeleteDetails = function (data) {
        try {
            var gridObj = $("#AdditionalPayDayDetails").data("ejGrid");
            $scope.AdditionalPayDayDetailsModel = gridObj.getSelectedRecords()[0];
            
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { 'DetailsId': $scope.AdditionalPayDayDetailsModel.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');                
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getDetails();
                    $scope.AdditionalPayDayDetailsModel.MID = $scope.AdditionalPayDayMasterModel.Id;

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearM = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {   
        $scope.AdditionalPayDayMasterModel = {
            Id: null,
            PolicyName: null,
            PolicyDescription: null,
            PlantId: $scope.AdditionalPayDayMasterModel.PlantId,
            IsActive: true,
        };
        $scope.getData();
        $scope.AdditionalPayDayDetailsList = [];
        $scope.AdditionalPayDayDetailsModel.MID = null;
    }

    $scope.Clear = function () {
        ClearField();
        return true;
    };

    function ClearField() {
    }

    function CheckField(fieldname, field) {
        try {
            if (field == null || field == '' || field == 'undefined') {
                throw "" + fieldname +" cannot be blank";
            }
            //if (baseService.isUndefinedOrNull(field)) {
            //    throw "[" + fieldname + "] can not be blank...";
            //}
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            //CheckField("Company", $scope.AdditionalPayDayMasterModel.CompanyId);
            CheckField("Plant", $scope.AdditionalPayDayMasterModel.PlantId);
            CheckField("Policy Name", $scope.AdditionalPayDayMasterModel.PolicyName);
            CheckField("Policy Description", $scope.AdditionalPayDayMasterModel.PolicyDescription);
        } catch (ex) {
            throw ex;
        }
    };    
    
    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogAdditionalPayDay").data("ejDialog");
            eDialog.open();    
            $scope.getSalaryHeadData();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
}