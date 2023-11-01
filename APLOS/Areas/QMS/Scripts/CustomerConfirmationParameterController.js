'use strict';
CustomerConfirmationParameterController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function CustomerConfirmationParameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "CustomerConfirmationParameter";
    $scope.Action = 'Save';
    $scope.CriticalLevelLists = [];
    $scope.path = 'QMS/CustomerConfirmationParameter/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.saveUrlCustomerUpdatePara = $scope.path + 'createCustomerConfirmationPara';
    $scope.saveUrlCCPDValue = $scope.path + 'createCCPRequirement';
    $scope.ParameterStatusLists = [];
    var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    date.setDate(date.getDate() - 3);

    $scope.ParameterStatusLists = [
        {
            'Value': 'Pending',
            'Text': 'Pending'
        },
        {
            'Value': 'ToApprove',
            'Text': 'ToApprove'
        }
        ,
        {
            'Value': 'Approved',
            'Text': 'Approved'
        }
    ];

    $scope.CriticalLevelLists = [
        {
            'Value': 'High',
            'Text': 'High'
        },
        {
            'Value': 'Very High',
            'Text': 'Very High'
        },
        {
            'Value': 'Medium',
            'Text': 'Medium'
        },
        {
            'Value': 'Low',
            'Text': 'Low'
        }
    ];

    $scope.CriticalLevelGridLists = [
        {
            'Value': 'High',
            'Text': 'High'
        },
        {
            'Value': 'Very High',
            'Text': 'Very High'
        },
        {
            'Value': 'Medium',
            'Text': 'Medium'
        },
        {
            'Value': 'Low',
            'Text': 'Low'
        }
    ];

    $scope.status = {
        Id: null,
        ParameterStatus: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.CustomerUpdatePara = {
        Id: null,
        LineItemNo: null,
        EmployeeId: null,
        ApprovedById: null,
        CriticalLevel: null,
        Remarks: null,
        ApprovalStatus:null
    };
    $scope.CustomerUpdateParaNew = Object.assign({}, $scope.CustomerUpdatePara);

    $scope.ParameterResponsiblePersonLists = [];
    $scope.GetParameterResponsiblePersonLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/CustomerConfirmationParameter/GetParameterResponsiblePersonLists'
        }).then(function successCallback(response) {
            $scope.ParameterResponsiblePersonLists = response.data;
        });
    }
    $scope.GetParameterResponsiblePersonLists();

    $scope.ParameterApprovalPersonLists = [];
    $scope.GetParameterApprovalPersonLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/CustomerConfirmationParameter/GetParameterApprovalPersonLists'
        }).then(function successCallback(response) {
            $scope.ParameterApprovalPersonLists = response.data;
        });
    }
    $scope.GetParameterApprovalPersonLists();

    $scope.CustomerRequirementControlList = [];
    $scope.View = function () {
        try {
            $scope.QCCompleteList = [];
            $http.get('QMS/CustomerConfirmationParameter/LoadCustomerConfirmationParameter')
                .then(function (response) {
                    $scope.CustomerRequirementControlList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.View();

    $scope.UCPId = null;
    $scope.GetDetails = function ($event) {
        $scope.CustomerUpdateParaNew.LineItemNo = $event.data.LineItemNo;
        $scope.CustomerUpdateParaNew.Id = $event.data.Id;
        $scope.CustomerUpdateParaNew.EmployeeId = $event.data.EmployeeId;
        $scope.CustomerUpdateParaNew.ApprovedById = $event.data.ApprovedById;
        $scope.CustomerUpdateParaNew.CriticalLevel = $event.data.CriticalLevel;
        $scope.CustomerUpdateParaNew.Remarks = $event.data.Remarks;
        $scope.GetParameterResponsiblePersonLists();
        $scope.GetParameterApprovalPersonLists();
        $scope.UCPId = $event.data.UCPId;
        $scope.loadCCPD();
        angular.element(document.querySelector('#UpdateCustomerParameterPopUp')).modal('show');
    }
    $scope.CustomerUpdateSave = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlCustomerUpdatePara,
            data: {
                'CustomerUpdateParaData': $scope.CustomerUpdateParaNew,
                'ApprovalStatus':  'Approved'
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.View();
                angular.element(document.querySelector('#UpdateCustomerParameterPopUp')).modal('hide');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.CCPDList = [];
    $scope.loadCCPD = function () {
        try {
            $http.get('QMS/CustomerConfirmationParameter/GetCCPCbo?MasterId=' + $scope.UCPId + '&LineItemNo=' + $scope.CustomerUpdateParaNew.LineItemNo)
                .then(function (response) {
                    $scope.CCPDList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveUCPD = function (data) {
        try {
            if (baseService.isUndefinedOrNull(data.data.MinRequirement) && baseService.isUndefinedOrNull(data.data.MaxRequirement)) {
                throw "Please enter requirement and proceed";
            }
            data.data.UCPId = $scope.UCPId;
            $http({
                method: 'POST',
                url: $scope.saveUrlCCPDValue,
                data: { 'UCPRequirementDetailsData': data.data },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.loadCCPD();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.selectGridResponsible = function (data) {
        $scope.Newobject = data.data;
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }
}

