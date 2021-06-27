'use strict';
bonusPolicyController.$inject = ['$window', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function bonusPolicyController($window, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Bonus Policy';
    $scope.path = 'Attendances/BonusPolicy/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveBP = $scope.path + 'SaveBP';
    $scope.saveLeaveUrl = $scope.path + 'SaveLeave';
    $scope.saveMUrl = $scope.path + 'SaveM';
    $scope.deleteUrl = $scope.path + 'DeleteDetails/';

    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.BonusPolicy.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }

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

    $scope.setTab33 = function (newTab) {
        $scope.tabh = newTab;

    };

    $scope.SalaryHeadList = [];
    $scope.getSalaryHeadListList = function () {
        $http.get('Attendances/BonusPolicy/GetSalaryHeadListeList')
            .then(function (response) {
                $scope.SalaryHeadList = response.data;
            });
    };
    $scope.getSalaryHeadListList();

    $scope.ModelList = [];
    $scope.getData = function () {
        $http.post('Attendances/BonusPolicy/GetList')
            .then(function (response) {
                $scope.ModelList = response.data;
                $scope.PlantCompanyList();
            });
    };
    $scope.getData();

    $scope.PlantList = [];
    $scope.PlantCompanyList = function () {
        $http.post('Attendances/BonusPolicy/GetPlant')
            .then(function (response) {
                $scope.PlantList = response.data;

            });
    };
    //$scope.PlantCompanyList();

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();

    };
    $scope.actionCompleteSelected = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#BonusPolicyDetailsId").ejGrid("instance");
                var scrollerwidth = $("#NewId").width();

                $("#BonusPolicyDetailsId").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 150 } });
                gridObj.windowonresize();
            }
        } catch (e) {

        }
    };




    $scope.BonusPolicy = {
        MID: null,
        SystemID: null,
        PolicyName: null,
        BonusDescription: null,
        DefaultPolicy: false,
        EntitleFrm: null,
        ServiceLengthType: 'Month',
    };

    $scope.BonusPolicyPlantWise = {
        ID: null,
        BonusPolicyID: null,
        PlantId: null,
    }



    $scope.DataList = [];
    $scope.getDetails = function () {
        $http.get('Attendances/BonusPolicy/GetDetailsList?MasterId=' + $scope.BonusPolicy.MID)
            .then(function (response) {
                $scope.DataList = [];
                if (response.data.length == 0) {
                    $scope.DataList.push(Object.assign({}, $scope.BonusPolicyDetailModel));
                }
                else {
                    $scope.DataList = response.data;
                }

            });
    };

    $scope.DetailsId = null;
    $scope.Save = function () {
        try {
            if ($scope.DataList.length == 0) {
                throw "Policy Detail Cannot be blank..";
            }
            for (var i = 0; i < $scope.DataList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.DataList[i].MinServLen)) {
                    throw 'Define minimum service length!';
                }
                if (baseService.isUndefinedOrNull($scope.DataList[i].MaxServLen)) {
                    throw 'Define maximum service length!';
                }
                if ($scope.DataList[i].MinServLen > $scope.DataList[i].MaxServLen) {
                    throw 'Minimum service length cannot be more than maximum service length!';
                }

                if ($scope.DataList[i].DisbursementType == 'Percentage' && $scope.DataList[i].PerctSalaryHeadID == null) {
                    throw 'Please select Salary Head...';
                }
                if ($scope.DataList[i].DisbursementType == 'Proportionate' && $scope.DataList[i].PerctSalaryHeadID == null) {
                    throw 'Please select Salary Head...';
                }
                if ($scope.DataList[i].DisbursementType == 'Proportionate' && $scope.DataList[i].BonusPercentage == null) {
                    throw 'Define percentage amount!';
                }
                if ($scope.DataList[i].DisbursementType == 'Percentage' && $scope.DataList[i].BonusPercentage == null) {
                    throw 'Define percentage amount!';
                }
                if ($scope.DataList[i].DisbursementType == 'Proportionate' && $scope.DataList[i].DivisionFactor == null) {
                    throw 'Define division factor!';
                }
                if ($scope.DataList[i].DisbursementType == 'Percentage') {
                    if (baseService.isUndefinedOrNull($scope.DataList[i].MinBonusAmt)) {
                        throw 'Define minimum bonus amount!';
                    }
                }
                if ($scope.DataList[i].DisbursementType == 'Proportionate') {
                    if (baseService.isUndefinedOrNull($scope.DataList[i].MinBonusAmt)) {
                        throw 'Define minimum bonus amount!';
                    }
                }
                if ($scope.DataList[i].DisbursementType == 'Fixed') {
                    $scope.DataList[i].DivisionFactor = 0;
                    $scope.DataList[i].PerctSalaryHeadID = null;
                    $scope.DataList[i].BonusPercentage = 0;
                    $scope.DataList[i].MinBonusAmt = 0;
                    $scope.DataList[i].IsFixed = true;
                    $scope.DataList[i].IsPercentage = false;
                    $scope.DataList[i].IsProportionate = false;
                }
                if ($scope.DataList[i].DisbursementType == 'Percentage') {
                    $scope.DataList[i].DivisionFactor = 0;
                    $scope.DataList[i].FixedAmount = 0;
                    $scope.DataList[i].IsFixed = false;
                    $scope.DataList[i].IsPercentage = true;
                    $scope.DataList[i].IsProportionate = false;
                }
                if ($scope.DataList[i].DisbursementType == 'Proportionate') {
                    $scope.DataList[i].FixedAmount = 0;
                    $scope.DataList[i].IsFixed = false;
                    $scope.DataList[i].IsPercentage = false;
                    $scope.DataList[i].IsProportionate = true;
                }

                if ($scope.DataList[i].FixedAmount < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].DivisionFactor < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].MinBonusAmt < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].BonusPercentage < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].MinServLen < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].MaxServLen < 0) {
                    throw "Positive Value only";
                }
                if ($scope.DataList[i].ServiceLengthType == 'Month' && $scope.DataList[i].MinServLen >= 0) {
                    if ($scope.DataList[i].MaxServLen > $scope.DataList[i].MinServLen) { }
                    else {
                        throw "Must be grater then Min Serv Length";
                    }
                }
            }

            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Details': $scope.DataList, 'MasterId': $scope.BonusPolicy.MID },
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
                data: { 'Master': $scope.BonusPolicy },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.BonusPolicyDetailModel.MID = response.data.MasterId;
                    $scope.BonusPolicy.MID = response.data.MasterId;
                    $scope.getData();
                    $scope.PlantCompanyList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.recorddoubleclick = function () {
        var gridObj = $("#BonusPolicyMasterId").data("ejGrid");
        $scope.BonusPolicy = gridObj.getSelectedRecords()[0];
        $scope.BonusPolicyDetailModel.MID = $scope.BonusPolicy.MID;
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
        }
        $scope.getDetails($scope.BonusPolicy.MID);
    };

    $scope.recorddoubleclickDetails = function () {
        $scope.dataList = [];
        var gridObj = $("#BonusPolicyDetailsId").data("ejGrid");
        $scope.BonusPolicyDetailModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.getLeaveTypeList();
        } catch (e) {

        }
    };

    //$scope.DeleteMaster = function () {
    //    if (!baseService.isUndefinedOrNull($scope.BonusPolicy.MID)) {
    //        $http.get('Attendances/AttendanceBonusPolicy/DeleteM?SystemID=' + $scope.BonusPolicy.MID)
    //            .then(function successCallback(response) {
    //                if (response.data.Error === true) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //                else {
    //                    ShowResult(response.data.Message, 'success');
    //                    $scope.ClearM();
    //                    $scope.Clear();
    //                    $scope.getData();
    //                }
    //                function errorCallBack(response) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //            });
    //    }
    //};

    $scope.DeleteMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteM?SystemID=' + $scope.BonusPolicy.MID,
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ClearM();
                $scope.Clear();
                $scope.getData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.DeleteDetailsFunction = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.deleteUrl,
                data: { 'DetailsId': $scope.BonusPolicyDetailModel.SystemID },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getDetails();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.ClearM = function (obj) {
        ClearFields(obj);
        return true;
    };

    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
        $scope.BonusPolicyDetailsList = [];
        //$scope.getData();
        $scope.BonusPolicy = {
            MID: null,
            SystemID: null,
            PolicyName: null,
            BonusDescription: null,
            DefaultPolicy: false,
            EntitleFrm: null,
            ServiceLengthType: 'Month',
        };
        $scope.BonusPolicyDetail = {
            SystemID: null,
            DisbursementType: 'Fixed',
            BPMSystemID: null,
            EmpCategorySysID: null,
            MinServLen: 0,
            MaxServLen: 0,
            FixedAmount: 0,
            PerctSalaryHeadID: null,
            BonusPercentage: 0,
            DivisionFactor: 0,
            MinBonusAmt: 0,
            IsFixed: false,
            IsPercentage: false,
            IsProportionate: false,
        };
        $scope.DataList = [];
    }

    $scope.Clear = function () {
        ClearField();
        return true;
    };

    function ClearField() {
        $scope.BonusPolicyDetail = {
            SystemID: null,
            DisbursementType: 'Fixed',
            BPMSystemID: null,
            EntitleFrm: 'DOJ',
            EmpCategorySysID: null,
            MinServLen: 0,
            MaxServLen: 0,
            FixedAmount: 0,
            PerctSalaryHeadID: null,
            BonusPercentage: 0,
            DivisionFactor: 0,
            MinBonusAmt: 0,
            ServiceLengthType: 'Month',
            IsFixed: false,
            IsPercentage: false,
            IsProportionate: false,
        };
        $scope.BonusPolicyDetailModel = Object.assign({}, $scope.BonusPolicyDetail);
        $scope.dataList = [];
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Policy Name", $scope.BonusPolicy.PolicyName);
            CheckField("Entitle From", $scope.BonusPolicy.EntitleFrm);
        } catch (ex) {
            throw ex;
        }
    };

    $scope.ShowDiv = false;
    $scope.AddLineIdem = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
            eDialog.open();
            $scope.BonusPolicyDetail = {
                SystemID: null,
                DisbursementType: 'Fixed',
                BPMSystemID: null,
                EntitleFrm: null,
                EmpCategorySysID: null,
                MinServLen: 0,
                MaxServLen: 0,
                FixedAmount: 0,
                PerctSalaryHeadID: null,
                BonusPercentage: 0,
                DivisionFactor: 0,
                MinBonusAmt: 0,
                ServiceLengthType: 'Month',
                IsFixed: false,
                IsPercentage: false,
                IsProportionate: false,
            };

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.confirmdelete = false;
    $scope.Confirm = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.open();
        $("#dialogAPI_wrapper").css({ 'position': 'fixed' }).css({ 'top': '200px' });
    };
    $scope.ConfirmClose = function () {
        var eDialog = $("#dialogAPI").data("ejDialog");
        eDialog.close();
    };

    //Get  Leave Data


    $scope.PlantWiseBPolicyList = [];
    $scope.getplantPolicy = function () {
        $http.get("Attendances/BonusPolicy/GetPlantBonusPolicy?plantID=" + $scope.PlantId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PlantWiseBPolicyList = response.data;
                        if (baseService.arrayLength($scope.PlantWiseBPolicyList) > 0) {
                            for (var i = 0; i < $scope.PlantWiseBPolicyList.length; i++) {
                                $scope.PlantWiseBPolicyList[i].PlantId = $scope.PlantId;
                            }
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.ShowDiv = false;
    $scope.AddLineItem = function (obj) {
        try {
            $scope.ShowDiv = true;
            $scope.PlantId = obj.data.PlantId;
            var eDialog = $("#policyID").data("ejDialog");
            eDialog.open();
            $scope.BonusPolicyPlantWise = {
                ID: null,
                BonusPolicyID: null,
                PlantId: $scope.PlantId
            }
            $scope.getplantPolicy();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.AddItem = function (obj) {
        try {
            $scope.ShowDiv = true;
            $scope.PolicyId = obj.data.MID;
            var eDialog = $("#PpolicyID").data("ejDialog");
            eDialog.open();
            $scope.BonusPolicyPlantWise = {
                ID: null,
                BonusPolicyID: $scope.PolicyId,
                PlantId: null
            }
            $scope.getplantPolicyy();

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.PlantWiseBPolicyListt = [];
    $scope.getplantPolicyy = function () {
        $http.get("Attendances/BonusPolicy/GetPlantBonusPolicyy?PolicyId=" + $scope.PolicyId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.PlantWiseBPolicyListt = response.data;
                        if (baseService.arrayLength($scope.PlantWiseBPolicyListt) > 0) {
                            for (var i = 0; i < $scope.PlantWiseBPolicyListt.length; i++) {
                                $scope.PlantWiseBPolicyListt[i].BonusPolicyID = $scope.PolicyId;
                            }
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.PlantWiseBPolicyList = [];
    $scope.SaveBP = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveBP,
                data: { 'BP': $scope.PlantWiseBPolicyList/*, plantID: $scope.PlantId */ },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getplantPolicy();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.SaveBPlant = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.saveBP,
                data: { 'BP': $scope.PlantWiseBPolicyListt },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getplantPolicyy();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    $scope.Detail = {
        Id: null,
        Minimum: 0,
        Maximum: 0,
        FirstDayEfficiency: 0,
        Increment: 0,
        LastDayEfficiency: 0
    }

    $scope.BonusPolicyDetail = {
        SystemID: null,
        DisbursementType: 'Fixed',
        BPMSystemID: null,
        EmpCategorySysID: null,
        MinServLen: 0,
        MaxServLen: 0,
        FixedAmount: 0,
        PerctSalaryHeadID: null,
        BonusPercentage: 0,
        DivisionFactor: 0,
        MinBonusAmt: 0,
        IsFixed: false,
        IsPercentage: false,
        IsProportionate: false,
    };
    $scope.BonusPolicyDetailModel = Object.assign({}, $scope.BonusPolicyDetail);

    //$scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.BonusPolicyDetailModel));

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }
    $scope.SubmitH = function (data) {

        try {
            if (data.MinServLen < 0)
                throw 'Minimum value cannot be negative';

            if (data.MaxServLen < 0)
                throw 'Maximum value cannot be negative';


            if (data.MinServLen >= data.MaxServLen)
                throw 'Maximum value should be greater than minimum value';



            var newObj = Object.assign({}, $scope.BonusPolicyDetailModel);
            if (data != null) {
                newObj = {
                    SystemID: null,
                    DisbursementType: 'Fixed',
                    BPMSystemID: null,
                    EmpCategorySysID: null,
                    MinServLen: data.MaxServLen,
                    MaxServLen: 0,
                    FixedAmount: 0,
                    PerctSalaryHeadID: null,
                    BonusPercentage: 0,
                    DivisionFactor: 0,
                    MinBonusAmt: 0,
                    IsFixed: false,
                    IsPercentage: false,
                    IsProportionate: false,
                }
            }

            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

}