'use strict';
GoodWorkSetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function GoodWorkSetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'GoodWorkSetup';
    $scope.Action = 'Save';
    $scope.BCAction = 'Save';
    $scope.AuthorityAction = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWorkSetup/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'UserCode', name: "UserCode" }, { value: 'UserName', name: "User Name" }, { value: 'Remarks', name: "Remarks" }];

    //for tab
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
        Id: null,
        UserCode: null,
        UserName: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetGoodWorkEntitySetupData();
        $scope.GetGoodWorkBudgetCodeData();
        $scope.GetGoodWorkAuthorityData();
        $scope.GetCheckByData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.popUpEmpDataList = [];
    $scope.showEmployeeListPopUp = function () {
        try {
            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: 'OrderManagements/SalesOrderApproval/GetAllActiveEmpData'

            }).then(function successCallback(response) {
                $scope.popUpEmpDataList = response.data;
            });

            angular.element(document.querySelector('#popUp')).modal('show');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.ModelNew.ResponsiblePersonId = arg.data.SystemId;
        $scope.ModelNew.ResponsiblePerson = arg.data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = arg.data.EmployeeCode;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.ModelNew.ResponsiblePersonId = null;
        $scope.ModelNew.ResponsiblePerson = null;
        $scope.ModelNew.ResponsiblePersonCode = null;
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


    //#region Entity

    $scope.entitySearchList = [];
    $scope.entityDataList = [];
    $scope.entitySearch = [];
    $scope.entityUrl = 'Organizations/entity/getlist?companyId=';
    $scope.entityParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'UserName',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.entityPopUp = function () {
        $scope.getEntityData = function (pageno) {
            baseService.paginationBase($scope.entityUrl + $window.companyId, pageno, $scope.entityParameters)
                .then(function (response) {
                    for (var i = 0; i < response.Rows.length; i++) {
                        response.Rows[i].Flag = false;
                    }
                    $scope.entityDataList = response.Rows;
                    $scope.entityParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.entitySearchList) === 0) {
                        baseService.getDDLSearchColumn($scope.entityDataList, $scope.entitySearchList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#entityPopUp')).modal('show');
        $scope.getEntityData();
    };
    $scope.closeEntityPopUp = function () {
        $scope.entityId = '';
        $scope.EntityName = '';
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.selectedEntityList = [];
    $scope.selectEntityPopUp = function () {
        if (baseService.arrayLength($scope.entityDataList) > 0) {
            angular.forEach($scope.entityDataList, function (a) {
                if (checkExistTempList($scope.selectedEntityList, a.Id) === false) {
                    if (a.Flag) {
                        $scope.selectedEntityList.push({
                            Id: null
                            , EntityId: a.Id
                            , GoodWorkSetupId: $scope.ModelNew.Id
                            , Code: a.Code
                            , UserName: a.UserName
                            , Plant: a.Plant
                            , Division: a.Division
                            , SubDivision: a.SubDivision
                            , Unit: a.Unit
                            , EffectiveDate: a.EffectiveDate
                            , IsProductionEntity: a.IsProductionEntity
                        });
                    }
                }

            });
        }
        else
            $scope.selectedEntityList = [];
        angular.forEach($scope.selectedEntityList, function (a) {
            if (!baseService.valueCheckInList($scope.entityDataList, 'Id', a.EntityId))
                $scope.selectedEntityList.splice(a, 1);
        });
        $scope.closeEntityPopUp();
        $scope.SaveEntity();
    };

    $scope.SaveEntity = function () {
        try {
            $http({
                method: 'POST',
                url: 'Attendances/GoodWorkSetup/CreateEntity',
                data: {
                    'data': $scope.selectedEntityList
                    , 'goodWorkSetupId': $scope.ModelNew.Id
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGoodWorkEntitySetupData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetGoodWorkEntitySetupData = function () {
        $http({
            method: 'GET',
            url: "Attendances/GoodWorkSetup/GetGoodWorkEntitySetupData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.selectedEntityList = response.data;
        });
    }
    $scope.removeEntity = function (tempId) {
        try {
            $scope.tempId = tempId.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmEntityRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeEntityRow = function () {
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/EntityDelete',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkEntitySetupData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };


    $scope.tempList = [];
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.Id) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].Id === data.Id) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EntityId === Id) {
                return true;
            }
        }
        return false;
    }


    //#endregion

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.selectedEntityList = [];
        $scope.BudgetCodeList = [];
        $scope.authorizationList = [];
        $scope.CheckByList = [];
    }

    //#region BudgetCode

    $scope.name = null;
    $scope.popUpTitle = "Manpower Budget Information";
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.budgetpopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "Code",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUpBudgetCode = function () {

        $scope.popUpDataList = [];
        $scope.popUpList = [];
        $scope.budgetpopUpParameters.sort = 'Code';
        $scope.budgetpopUpParameters.searchBy = 'Code';
        $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.budgetpopUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.budgetpopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                    }
                    //$scope.popUpParameters.sort = 'Code';
                    //$scope.popUpParameters.searchBy = 'Code';
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.BudgetCodeList = [];
    $scope.selectDoubleClick = function (data) {
        try {
            var ob = {};
            if (checkDoubleBudgetCode($scope.BudgetCodeList, data.Id) === false) {
                ob.BudgetId = data.Id;
                ob.Entity = data.EntityName;
                ob.Division = data.Division;
                ob.Department = data.Department;
                ob.Section = data.Section;
                ob.SubSection = data.SubSection;
                ob.EmployeeType = data.EmployeeType;
                ob.Designation = data.Designation;
                ob.Activity = data.Activity;
                ob.UserGroup = data.UserGroup;
                ob.Process = data.Process;
                ob.BudgetCode = data.Code;
                //ob.IsOTEntitled = data.IsOTEntitled;
                if (data.IsOTEntitled == 0) {
                    ob.IsOTEntitled = 'No';
                } else
                    ob.IsOTEntitled = 'Yes'
                ob.IsGoodWorkApplicable = false,
                    ob.IsCompensatoryApplicable = false,
                    ob.IsEmployeeApplicable = false,
                    ob.GoodWorkCategory = null,
                    ob.Remarks = null,
                    $scope.BudgetCodeList.push(ob);
                ob = {};
            }
            angular.element(document.querySelector('#popUpId')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    function checkDoubleBudgetCode(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.clearCode = function () {
        $scope.employeeNew.BudgetCode = null;
        $scope.employeeNew.Code = null;
        $scope.employeeNew.EntityName = null;
        $scope.employeeNew.Designation = null;
        $scope.employeeNew.PositionName = null;

        $scope.employeeNew.DesignationId = null;
        $scope.employeeNew.UnitId = null;
        $scope.employeeNew.DivisionId = null;
        $scope.employeeNew.DepartmentId = null;
        $scope.employeeNew.SectionId = null;
        $scope.employeeNew.SubSectionId = null;
        $scope.employeeNew.SubdivisionID = null;
        $scope.employeeNew.LineId = null;
        $scope.employeeNew.EmployeeCodeTypeId = null;
        $scope.employeeNew.EmploymentType = null;
        $scope.employeeNew.PositionID = null;
        $scope.employeeNew.IsDirect = false;
    };

    $scope.GetOnRollByBudget = function (budgetId) {
        try {
            $http.get('employees/EmployeeInformation/GetOnRollByBudget?budgetId=' + budgetId)
                .then(function (response) {
                    if (response.data[0].TotalNumber < response.data[0].OnRollManPwr || response.data[0].TotalNumber == response.data[0].OnRollManPwr) {
                        ShowResult("On Roll Manpower is exceeding Budgeted Manpower.", 'failure', 'popUpId');;
                    }
                    else {
                        angular.element(document.querySelector('#popUpId')).modal('hide');
                    }
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
        angular.element(document.querySelector('#LDPopUp')).modal('hide');
    };


    $scope.BCSave = function () {

        for (var i = 0; i < $scope.BudgetCodeList.length; i++) {
            if (baseService.isUndefinedOrNull($scope.BudgetCodeList[i].GoodWorkCategory) || $scope.BudgetCodeList[i].GoodWorkCategory === 0) {
                ShowResult('Good Work Category can not be blank...');
                return false;
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + "CreateBudgetCode",
            data: {
                'data': $scope.BudgetCodeList
                , 'goodWorkSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkBudgetCodeData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };


    $scope.GetGoodWorkBudgetCodeData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetGoodWorkBudgetCodeSetupData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.BudgetCodeList = response.data;
        });
    }

    $scope.removeBudgetCode = function (tempId) {
        try {
            $scope.tempId = tempId.data.Id;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmBudgetCodeRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeBudgetCodeRow = function () {
        $http({
            method: 'POST',
            url: 'Attendances/GoodWorkSetup/BudgetCodeDelete',
            data: { 'Id': $scope.tempId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetGoodWorkBudgetCodeData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    //#endregion BudgetCode

    //#region
    // #region  Dynamic PopUp

    $scope.popUpList = [];
    $scope.employeeInformation = {
        PlantId: $window.plantId
        , EmployeeCode: null
        , EmployeeName: null
        , SystemId: null

    };
    $scope.popUpDataList = [];
    $scope.popUpEmployee = function (obj) {
        try {
            $scope.tabName = obj;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/authorizationconfig/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUpEmp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CheckByList = [];
    $scope.authorizationList = [];
    $scope.selectdblClick = function (data) {
        try {
            var data = data.data;
            var ob = {};
            if ($scope.tabName == 'Authority') {
                if (checkDoubleAuthorityCode($scope.authorizationList, data.EmployeeCode) === false) {
                    ob.AuthorityId = data.SystemId;
                    ob.EmployeeName = data.EmployeeName;
                    ob.EmployeeCode = data.EmployeeCode;
                    ob.CompanyId = data.CompanyId;
                    ob.Company = data.Company;
                    ob.Plant = data.Plant;
                    ob.Designation = data.LegalDesignation;
                    ob.Department = data.Department;
                    ob.Section = data.Section;
                    ob.SubSection = data.SubSection;
                    ob.Line = data.Line;

                    $scope.authorizationList.push(ob);
                    $scope.AuthoritySave();
                    ob = {};
                }
                angular.element(document.querySelector('#popUpEmp')).modal('hide');
            }
            else {
                if (checkDoubleAuthorityCode($scope.CheckByList, data.EmployeeCode) === false) {
                    ob.CheckById = data.SystemId;
                    ob.EmployeeName = data.EmployeeName;
                    ob.EmployeeCode = data.EmployeeCode;
                    ob.CompanyId = data.CompanyId;
                    ob.Company = data.Company;
                    ob.Plant = data.Plant;
                    ob.Designation = data.LegalDesignation;
                    ob.Department = data.Department;
                    ob.Section = data.Section;
                    ob.SubSection = data.SubSection;
                    ob.Line = data.Line;

                    $scope.CheckByList.push(ob);
                    $scope.CheckBySave();
                    ob = {};
                }
                angular.element(document.querySelector('#popUpEmp')).modal('hide');

            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    function checkDoubleAuthorityCode(list, EmployeeCode) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === EmployeeCode) {
                return true;
            }
        }
        return false;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUpEmp')).modal('hide');
    };
    // #endregion

    $scope.AuthoritySave = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CreateAuthority",
            data: {
                'data': $scope.authorizationList
                , 'goodWorkSetupId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //$scope.GetGoodWorkAuthorityData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetGoodWorkAuthorityData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetGoodWorkAuthorityData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.authorizationList = response.data;
        });
    }

    $scope.CheckBySave = function () {
        $http({
            method: 'POST',
            url: $scope.path + "CreateCheckBy",
            data: { 'data': $scope.CheckByList, 'goodWorkSetupId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.GetCheckByData = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetCheckByData?goodWorkSetupId=" + $scope.ModelNew.Id
        }).then(function (response) {
            $scope.CheckByList = response.data;
        });
    }
    //#endregion

}