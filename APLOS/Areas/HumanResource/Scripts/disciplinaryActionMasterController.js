'use strict';
disciplinaryActionMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function disciplinaryActionMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'disciplinaryActionMaster';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.disciplinaryActionMasters = [];
    $scope.damList = [];
    $scope.path = 'humanresource/disciplinaryactionmaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'EmpSystemId', 'EmpSystemId');
    cboService.getCboPlantByCompany(null, function (result) {
        $scope.PlantList = result;
    });
    $scope.getDataByEmp = function (EmpSystemId) {
        $http.get('humanresource/disciplinaryactionmaster/getlist?EmpSystemId=' + EmpSystemId)
            .then(function (response) {
                $scope.damList = response.data.Rows;
                console.log(response.data);
                if ($scope.damList.length > 0) {
                    $scope.disciplinaryActionMasterNew.Id = $scope.damList[0].Id;

                    //$scope.disciplinaryActionMasterNew.PlantId = $scope.damList[0].PlantId;
                    $scope.disciplinaryActionMasterNew.EmployeeId = $scope.damList[0].SystemId;
                    $scope.disciplinaryActionMasterNew.EmployeeName = $scope.damList[0].EmployeeName;
                    $scope.disciplinaryActionMasterNew.EmployeeCode = $scope.damList[0].EmployeeCode;
                    $scope.disciplinaryActionMasterNew.EmailId = $scope.damList[0].EmailId;
                    $scope.disciplinaryActionMasterNew.BudgetCode = $scope.damList[0].BudgetCode;
                    $scope.disciplinaryActionMasterNew.Designation = $scope.damList[0].Designation;
                    $scope.disciplinaryActionMasterNew.Department = $scope.damList[0].Department;
                    $scope.disciplinaryActionMasterNew.EmpPicPath = $scope.damList[0].EmpPicPath;
                    $scope.disciplinaryActionMasterNew.ResponsiblePersonId = $scope.damList[0].InvestigatorId;
                    $scope.disciplinaryActionMasterNew.ResponsiblePersonName = $scope.damList[0].ResponsiblePersonName;
                    $scope.disciplinaryActionMasterNew.InvestigatorId = $scope.damList[0].InvestigatorId;
                    $scope.disciplinaryActionMasterNew.InvestigatorName = $scope.damList[0].InvestigatorName;
                }
            })
    }

    $scope.disciplinaryActionMaster = {
        Id: null,
        PlantId: null,
        EmployeeName: null,
        EmployeeCode: null,
        BudgetCode: null,
        EmailId: null,
        Designation: null,
        Department: null,
        EmpSystemId: null,
        ActionId: null,
        ActionCriticalityId: null,
        Description: null,
        InvestigatorId: null,
        ResponsiblePersonId: null
    };

    $scope.disciplinaryActionMasterNew = Object.assign({}, $scope.disciplinaryActionMaster);

    $scope.popUpList = [];
    $scope.popUp = function (name) {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'EmployeeCode',
            searchBy: 'EmployeeCode',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        try {
            if (baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.PlantId)) {
                throw "First select plant.";
            }
            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';
            if (name === 'EmployeeInformation') {
                $scope.popUpTitle = 'Employee Information';
                $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist?plantId=' + $scope.disciplinaryActionMasterNew.PlantId;
                $scope.popUpParameters.sort = 'EmployeeName';
                $scope.popUpParameters.searchBy = 'EmployeeName';
            }
            if (name === 'EmployeeInfo') {
                $scope.popUpTitle = 'Responsible Person Information';
                $scope.popUpUrl = 'humanresource/disciplinaryactionmaster/getemployeedatalist?plantId=' + $scope.disciplinaryActionMasterNew.PlantId + '&empId=' + $scope.disciplinaryAction.EmployeeId;
                $scope.popUpParameters.sort = 'EmployeeName';
                $scope.popUpParameters.searchBy = 'EmployeeName';
            }
            if (name === 'EmployeeInfoIvg') {
                $scope.popUpTitle = 'Investigator Information';
                $scope.popUpUrl = 'humanresource/disciplinaryactionmaster/getemployeedatalist?plantId=' + $scope.disciplinaryActionMasterNew.PlantId + '&empId=' + $scope.disciplinaryAction.EmployeeId;
                $scope.popUpParameters.sort = 'EmployeeName';
                $scope.popUpParameters.searchBy = 'EmployeeName';
            }
            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.fieldName = name;
            angular.element(document.querySelector('#popUp')).modal('show');
            $scope.popUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.disciplinaryAction = {};

    $scope.selectdblClick = function (data) {
        setPartyName(data);
        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    function setPartyName(ob) {
        try {
            if ($scope.fieldName === 'EmployeeInformation') {
                $scope.disciplinaryAction.EmployeeId = ob.SystemId;
                $scope.disciplinaryActionMasterNew.EmpSystemId = ob.SystemId;
                $scope.disciplinaryAction.EmployeeName = ob.EmployeeName;
                $scope.disciplinaryAction.EmployeeCode = ob.EmployeeCode;
                $scope.disciplinaryAction.EmailId = ob.EmailId;
                $scope.disciplinaryAction.BudgetCode = ob.BudgetCode;
                $scope.disciplinaryAction.Designation = ob.Designation;
                $scope.disciplinaryAction.Department = ob.Department;
                $scope.disciplinaryAction.EmpPicPath = ob.EmpPicPath;
                $scope.imageSrc = virtualPath.EmployeePic + '/' + $scope.disciplinaryAction.EmpPicPath;
                $scope.getDataByEmp($scope.disciplinaryActionMasterNew.EmpSystemId);
            }
            if ($scope.fieldName === 'EmployeeInfo') {
                $scope.disciplinaryActionMasterNew.ResponsiblePersonId = ob.SystemId;
                $scope.disciplinaryActionMasterNew.ResponsiblePersonName = $scope.disciplinaryActionMasterNew.EmployeeName;
            }
            if ($scope.fieldName === 'EmployeeInfoIvg') {
                $scope.disciplinaryActionMasterNew.InvestigatorId = ob.SystemId;
                $scope.disciplinaryActionMasterNew.InvestigatorName = $scope.disciplinaryActionMasterNew.EmployeeName;
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.AddDisciplinaryActionMaster = function () {
        try {
            //if (baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.EmpSystemId)) {
            //    throw "First select an employee.";
            //}
            $scope.Action = 'Save';
            //$scope.disciplinaryActionMasterNew.Id = null;
            $scope.disciplinaryActionMasterNew.ActionId = null;
            $scope.disciplinaryActionMasterNew.ActionCriticalityId = null;
            $scope.disciplinaryActionMasterNew.Description = null;
            $scope.disciplinaryActionMasterNew.InvestigatorId = null;
            $scope.disciplinaryActionMasterNew.InvestigatorName = null;
            $scope.disciplinaryActionMasterNew.ResponsiblePersonId = null;
            $scope.disciplinaryActionMasterNew.ResponsiblePersonName = null;
            angular.element(document.querySelector('#criticalityPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure')
        }
    };

    $scope.clearResponsiblePerson = function () {
        $scope.disciplinaryActionMasterNew.ResponsiblePersonId = null;
        $scope.disciplinaryActionMasterNew.ResponsiblePersonName = null;
    }

    $scope.clearInvestigator = function () {
        $scope.disciplinaryActionMasterNew.InvestigatorId = null;
        $scope.disciplinaryActionMasterNew.InvestigatorName = null;
    }

    $scope.ClearActionPopUp = function () {
        $scope.disciplinaryActionMasterNew.ActionId = null;
        $scope.disciplinaryActionMasterNew.ActionCriticalityId = null;
        $scope.disciplinaryActionMasterNew.Description = null;
        $scope.disciplinaryActionMasterNew.InvestigatorId = null;
        $scope.disciplinaryActionMasterNew.InvestigatorName = null;
        $scope.disciplinaryActionMasterNew.ResponsiblePersonId = null;
        $scope.disciplinaryActionMasterNew.ResponsiblePersonName = null;
    }

    cboService.getCriticalityCbo(function (result) {
        $scope.criticalityList = result;
    });

    cboService.getActionCbo(function (result) {
        $scope.actionList = result;
    });

    $scope.Save = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.ActionCriticalityId)) {
                throw 'Please select a Criticality..';
            }
            if (baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.ActionId)) {
                throw 'Please select an Action..';
            }
            if (baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.ResponsiblePersonId)) {
                throw 'Please select a Responsible Person..';
            }
            angular.copy($scope.disciplinaryActionMasterNew, $scope.disciplinaryActionMaster);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.addCriticalityForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.disciplinaryActionMaster,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'criticalityPopUp');
                            $scope.getDataByEmp(response.data.DisciplinaryActionMaster.EmpSystemId);
                            angular.element(document.querySelector('#criticalityPopUp')).modal('hide');
                            $scope.ClearActionPopUp();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
                    }
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.disciplinaryActionMaster,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
                        }
                        else {
                            ShowResult(response.data.Message, 'success', 'criticalityPopUp');
                            $scope.getDataByEmp(response.data.DisciplinaryActionMaster.EmpSystemId);
                            angular.element(document.querySelector('#criticalityPopUp')).modal('hide');
                            $scope.ClearActionPopUp();
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure', 'criticalityPopUp');
        }
    };

    $scope.EditActCriticality = function (id, index) {
        $scope.index = index;
        $scope.disciplinaryActionMasterNew = $scope.damList[$scope.index];
        $scope.Action = 'Update';
        angular.element(document.querySelector('#criticalityPopUp')).modal('show');
    };

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.disciplinaryActionMasterNew.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.disciplinaryActionMasterNew.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success', 'criticalityPopUp');
    //                $scope.disciplinaryActionMasters.Delete($scope.disciplinaryActionMasterNew.Id);
    //                //$scope.disciplinaryActionMasters.Delete($scope.index, 1);
    //                angular.element(document.querySelector('#criticalityPopUp')).modal('hide');
    //                //$scope.disciplinaryActionMasters.splice($scope.index, 1);
    //                baseService.paginationRemove();
    //                $scope.ClearActionPopUp();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure', 'criticalityPopUp');
    //            }
    //        });
    //    }
    //};

    $scope.Clear = function () {
        //ClearFields($scope.GetSequence());
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.disciplinaryActionMaster = {};
        $scope.disciplinaryActionMasterNew = {};
        $scope.disciplinaryActionMasterNew.Sequence = seq;
        $scope.disciplinaryActionMasterNew.Active = true;
    }
}