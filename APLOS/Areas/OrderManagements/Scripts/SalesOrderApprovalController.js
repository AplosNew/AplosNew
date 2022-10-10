'use strict';
SalesOrderApprovalController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function SalesOrderApprovalController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Sales Order Approval';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'OrderManagements/SalesOrderApproval/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "GroupName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'GroupName', name: "Group Name" }];
    $scope.partyType = 'Customer';
    $controller('partyBaseController', { $scope: $scope, $http: $http });
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
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null, GroupName: null, GroupInchargeId: null, DepartmentalHeadId: null, Remark: null, Active: true, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetSavedPlantData();
        $scope.GetSavedCustomerData();
        $scope.GetCheckByData();
        $scope.GetApproveByData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpList = [];
    $scope.SelectedEmpList = [];

    $scope.popUpDataList = [];
    $scope.state = null;
    $scope.showEmployeeListPopUp = function (state) {
        try {
            $scope.state = state;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmployeeData'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            if ($scope.state == "ApproveBy" || $scope.state == "CheckBy") {
                angular.element(document.querySelector('#EmppopUp')).modal('show');
            } else {
                angular.element(document.querySelector('#popUp')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        if ($scope.state == "DH") {
            $scope.ModelNew.DepartmentalHeadId = arg.data.SystemId;
            $scope.ModelNew.DepartmentalHead = arg.data.EmployeeName;
        }
        else if ($scope.state == "GI") {
            $scope.ModelNew.GroupInchargeId = arg.data.SystemId;
            $scope.ModelNew.GroupInchargeName = arg.data.EmployeeName;
        }
        else {
            $scope.ModelC.AccountInchargeId = arg.data.SystemId;
            $scope.ModelC.AccountInchargeName = arg.data.EmployeeName;
        }
        $scope.closePopUp();
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

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
                    //ClearFields();
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
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.SelectedPlantList = [];
    }

    $scope.modelPlant = { CompanyId: null }

    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (response) {
        $scope.companyList = response;
    });

    $scope.plantList = [];
    $scope.ShowPlantpopUp = function () {
        try {
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetplantByCompany?companyId=' + $scope.modelPlant.CompanyId

            }).then(function successCallback(response) {
                $scope.plantList = response.data;
            });
            angular.element(document.querySelector('#plantpopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.refreshTemplatePlant = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPlant });
    };

    function CheckBoxSelectAllPlant(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridPlant").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.plantList.length; i++) {
                $scope.plantList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridPlant").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SelectedPlantList = [];
    $scope.closePlantPopUp = function () {
        var obj = {};
        for (var i = 0; i < $scope.plantList.length; i++) {
            if ($scope.plantList[i].Flag) {
                obj.Id = null;
                obj.SalesOrderApprovalMasterId = $scope.ModelNew.Id;
                obj.PlantId = $scope.plantList[i].Id;
                obj.Code = $scope.plantList[i].Code;
                obj.ShortName = $scope.plantList[i].ShortName;
                obj.StandardName = $scope.plantList[i].StandardName;
                obj.UserName = $scope.plantList[i].UserName;
                obj.Sequence = $scope.plantList[i].Sequence;

                $scope.SelectedPlantList.push(obj);
                obj = {};
            }
        }
        $scope.SaveSelectedPlant();
        angular.element(document.querySelector('#plantpopUp')).modal('hide');
    };

    $scope.SaveSelectedPlant = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/SalesOrderApproval/CreatePlant',
                data: { "data": $scope.SelectedPlantList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSavedPlantData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SelectedPlantList = [];
    $scope.GetSavedPlantData = function () {
        $http.get('OrderManagements/SalesOrderApproval/GetSavedPlantData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SelectedPlantList = response.data;
                }
            });

    }

    $scope.valuePassInModal = function (data) {
        $scope.Id = data.data.Id;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete parmanently [ ' + data.data.UserName + ' ]';
        angular.element(document.querySelector('#removePlantPopUp')).modal('show');
    };

    $scope.DeletePlant = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/SalesOrderApproval/DeletePlant?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedPlantData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.ModelC = { Id: null, SalesOrderApprovalMasterId: null, CustomerId: null, Remark: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, PartyName: null, PartyCode: null, AccountInchargeName: null, AccountInchargeId: null }

    $scope.CustomerList = [];

    $scope.closePartyPopUp = function (x) {
        var party = x.data;
        $scope.ModelC.PartyCode = party.Code;
        $scope.ModelC.PartyName = party.UserName;
        $scope.ModelC.CustomerId = party.Id;

        $scope.hidePartyPopUp();
    };

    $scope.SaveCustomer = function () {
        try {
            $scope.ModelC.SalesOrderApprovalMasterId = $scope.ModelNew.Id;
            if (baseService.isUndefinedOrNull($scope.ModelC.CustomerId)) {
                throw "Select Customer";
            }
            if (baseService.isUndefinedOrNull($scope.ModelC.AccountInchargeId)) {
                throw "Select Account Incharge";
            }

            $http({
                method: 'POST',
                url: 'OrderManagements/SalesOrderApproval/CreateCustomer',
                data: { 'data': $scope.ModelC },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearCustomer();
                    $scope.GetSavedCustomerData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ClearCustomer = function () {
        $scope.ModelC = { Id: null, SalesOrderApprovalMasterId: null, CustomerId: null, Remark: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null, PartyName: null, PartyCode: null, AccountInchargeName: null, AccountInchargeId: null }
    }

    $scope.SelectedCustomerList = [];
    $scope.GetSavedCustomerData = function () {
        $http.get('OrderManagements/SalesOrderApproval/GetSavedCustomerData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SelectedCustomerList = response.data;
                }
            });

    }

    // #region  ResponsibleEmployee
    $scope.popUpList = [];
    $scope.SelectedEmpList = [];

   
    $scope.popUpDataList = [];

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridEmpPopUp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                $scope.popUpDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmpPopUp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.closeEmpPopUp = function () {
        var obj = {};
        if ($scope.state == "CheckBy") {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].Flag) {
                    obj.Id = null;
                    obj.SalesOrderApprovalMasterId = $scope.ModelNew.Id;
                    obj.EmpSystemId = $scope.popUpDataList[i].SystemId;
                    obj.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    obj.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    obj.Company = $scope.popUpDataList[i].Company;
                    obj.Plant = $scope.popUpDataList[i].Plant;
                    obj.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    obj.Department = $scope.popUpDataList[i].Department;
                    obj.Section = $scope.popUpDataList[i].Section;
                    obj.SubSection = $scope.popUpDataList[i].SubSection;
                    obj.Line = $scope.popUpDataList[i].Line;

                    $scope.SelectedCheckByEmpList.push(obj);
                    obj = {};
                }
            }
            $scope.SaveCheckbyEmpList();
        } else {
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].Flag) {
                    obj.Id = null;
                    obj.SalesOrderApprovalMasterId = $scope.ModelNew.Id;
                    obj.EmpSystemId = $scope.popUpDataList[i].SystemId;
                    obj.EmployeeCode = $scope.popUpDataList[i].EmployeeCode;
                    obj.EmployeeName = $scope.popUpDataList[i].EmployeeName;
                    obj.Company = $scope.popUpDataList[i].Company;
                    obj.Plant = $scope.popUpDataList[i].Plant;
                    obj.LegalDesignation = $scope.popUpDataList[i].LegalDesignation;
                    obj.Department = $scope.popUpDataList[i].Department;
                    obj.Section = $scope.popUpDataList[i].Section;
                    obj.SubSection = $scope.popUpDataList[i].SubSection;
                    obj.Line = $scope.popUpDataList[i].Line;

                    $scope.SelectedApproveByEmpList.push(obj);
                    obj = {};
                }
            }
            $scope.SaveApprovebyEmpList();
        }
        angular.element(document.querySelector('#EmppopUp')).modal('hide');
    };

    $scope.onrowdatabound = function (e) {
        if (e.data.EmployeeStatus === 'Separated')
            e.row.css("background-color", "red");
    };


    $scope.SaveCheckbyEmpList = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/SalesOrderApproval/CreateCheckBy',
                data: { "data": $scope.SelectedCheckByEmpList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetCheckByData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.SelectedCheckByEmpList = [];
    $scope.GetCheckByData = function () {
        $http.get('OrderManagements/SalesOrderApproval/GetCheckByData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SelectedCheckByEmpList = response.data;
                }
            });

    }

    $scope.valuePassCheckByModal = function (data) {
        $scope.Id = data.data.Id;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete parmanently [ ' + data.data.EmployeeCode + ' ]';
        angular.element(document.querySelector('#removeCheckByPopUp')).modal('show');
    };

    $scope.DeleteCheckByEmployee = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/SalesOrderApproval/DeleteCheckByEmployee?id=' + $scope.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetCheckByData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    };

    $scope.SaveApprovebyEmpList = function () {
        try {
            $http({
                method: 'POST',
                url: 'OrderManagements/SalesOrderApproval/CreateApproveBy',
                data: { "data": $scope.SelectedApproveByEmpList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetApproveByData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };
    $scope.SelectedApproveByEmpList = [];
    $scope.GetApproveByData = function () {
        $http.get('OrderManagements/SalesOrderApproval/GetApproveByData?masterId=' + $scope.ModelNew.Id)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.SelectedApproveByEmpList = response.data;
                }
            });

    }

    // #endregion



}