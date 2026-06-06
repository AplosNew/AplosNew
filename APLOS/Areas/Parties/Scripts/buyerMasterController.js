'use strict';
BuyerMasterController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$window'];
function BuyerMasterController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $window) {
    $rootScope.title = 'Buyer Definition';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerMasters = [];
    $scope.path = 'Parties/buyerMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveTaskUrl = $scope.path + 'CreateTask';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.buyerMaster = {
        Id: null,
        CompanyGroupId: null,
        BuyerId: null,
        BuyerDepartmentId: null,
        BuyerDivisionId: null,
        IsAllAssign: false,
        UserName: null,
        RepeatOrderLeadTime: null,
        RepeatProductionLeadTime: null,
        NormalOrderLeadTime: null,
        NormalProductionLeadTime: null,
        FirstAccountHolderId: null,
        SecondAccountHolderId: null

    };
    $scope.buyerMasterNew = Object.assign({}, $scope.buyerMaster);

    $scope.buyerMasterDetail = {
        Id: null,
        EntityId: null,
        BuyerMasterId: null
    };

    $scope.BuyerMasterActivity = {
        Id: null,
        BuyerMasterDetailId: null,
        BuyerActivityId: null,
        EmployeeOneId: null,
        EmployeeTwoId: null,
        EmployeeThreeId: null,
        EmployeeFourId: null,
        EmployeeFiveId: null,
        Active: true
    };

    //#region Employee Search

    $scope.empearch = "";
    $scope.searchByEmp = "EmployeeCode"; $scope.search = "";
    $scope.searchEmpByList = [{ value: 'SystemID', name: "SystemID" }, { value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "EmployeeName" }];


    $scope.employee = [];
    $scope.name = null;
    $scope.getEmpData = function (name) {
        if (!baseService.isUndefinedOrNull(name)) {
            $scope.name = name;
        }
        $scope.employee = [];
        $scope.popUpEmpDataList = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/getemployeelist',
            data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: $window.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
            $scope.popUpEmpDataList = response.data;
            angular.element(document.querySelector('#employeeNewPopUp')).modal('show');

        });
    }

    $scope.setEmpData = function (obj) {
        if ($scope.name == "First") {
            $scope.buyerMasterNew.FirstAccountHolderId = obj.data.SystemID;
            $scope.buyerMasterNew.FirstAccountHolder = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        else {
            $scope.buyerMasterNew.SecondAccountHolderId = obj.data.SystemID;
            $scope.buyerMasterNew.SecondAccountHolder = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        }
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };


    //#endregion 

    // #region cbo

    $scope.buyerList = [];
    $scope.buyerActivityList = [];
    $scope.departmentList = [];
    $scope.divisionList = [];

    cboService.getCboBuyer(function (result) {
        $scope.buyerList = result;
    });
    //cboService.getCboDepartmentByCompanyGroup(null, function (result) {
    //    $scope.departmentList = result;
    //})
    $scope.GetBuyerDepartmentCbo = function () {
        cboService.getBuyerDepartmentCboByBuyer($scope.buyerMasterNew.BuyerId, function (result) {
            $scope.departmentList = result;
        });
    };

    $scope.taskTemplateMasterCboList = [];
    cboService.getTaskTemplateMasterCbo(function (result) {
        $scope.taskTemplateMasterCboList = result;
    });

    $scope.GetBuyerDivisionCbo = function () {
        cboService.getBuyerDivisionCboByBuyer($scope.buyerMasterNew.BuyerId, function (result) {
            $scope.divisionList = result;
            //$scope.divisionList.unshift({ 'Value': '-1', 'Text': 'ALL' });
        });
    };

    $scope.getBuyerActivity = function () {
        cboService.getCboBuyerActivity('Buyer', function (result) {
            $scope.buyerActivityList = result;
        });
    };

    // #endregion 

    // #region ************Buyer Activity*********

    $scope.SelectedActivityList = [];

    $scope.showBuyerActivityPopUp = function (id, index) {
        $scope.activityIndex = index;
        $scope.bEntity = $scope.buyerMasterEntityList[index].EntityId;
        //$scope.buyerMasterDetail.BuyerActivityId = $scope.buyerMasterEntityList[index].BuyerActivityId;
        //$scope.buyerMasterDetail.BuyerMasterDetailId = $scope.buyerMasterEntityList[index].data.Id;
        $scope.buyerMasterDetail.BuyerMasterDetailId = id;

        getActivityByMasterDetail($scope.buyerMasterDetail.BuyerMasterDetailId);
        angular.element(document.querySelector('#buyerActivityPopUp')).modal('show');
    };

    $scope.addActivity = function () {

        if (baseService.isUndefinedOrNull($scope.BuyerMasterActivity.BuyerActivityId)) {
            ShowResult('Select Activity', 'failure', 'buyerActivityPopUp');
            return false;
        }
        var ob = {

            //BuyerMasterDetailId: BuyerMasterDetailId === null ? BuyerMasterDetailId : $scope.buyerMasterDetail.BuyerMasterDetailId,
            BuyerMasterDetailId: $scope.buyerMasterDetail.BuyerMasterDetailId,
            BuyerActivityId: $scope.BuyerMasterActivity.BuyerActivityId,
            BuyerActivityName: document.getElementById("activityId").options[document.getElementById('activityId').selectedIndex].text,
            EmployeeOneId: null,
            EmployeeOneName: null,
            EmployeeTwoId: null,
            EmployeeTwoName: null,
            EmployeeThreeId: null,
            EmployeeThreeName: null,
            EmployeeFourId: null,
            EmployeeFourName: null,
            EmployeeFiveId: null,
            EmployeeFiveName: null
        };
        $scope.SelectedActivityList.push(ob);
    };

    $scope.closeBuyerActivityPopUp = function () {
        angular.element(document.querySelector('#buyerActivityPopUp')).modal('hide');
    };

    $scope.ActivitySave = function () {
        $http({
            method: 'POST'
            , url: $scope.path + 'CreateActivity'
            , data: { 'activityList': $scope.SelectedActivityList, 'buyerMasterDetailId': $scope.buyerMasterDetail.BuyerMasterDetailId }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', 'buyerActivityPopUp');
            }
            else {

                angular.element(document.querySelector('#buyerActivityPopUp')).modal('hide');
                ShowResult(response.data.Message, 'success', 'buyerActivityPopUp');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure', 'buyerActivityPopUp');
        };
    };

    //Deleting Rows from MaterialFormList
    $scope.valuePassInActivityFormDelModal = function (index, data) {
        $scope.buyerMsterActivityId = data.Id;
        $scope.bActivityIndex = index;
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.BuyerActivityName + ' ]';
        angular.element(document.querySelector('#confirmActivityPopUp')).modal('show');
    };

    $scope.DeleteActivitySavedItem = function () {
        if (baseService.isUndefinedOrNull($scope.buyerMsterActivityId)) {
            $scope.SelectedActivityList.splice($scope.bActivityIndex, 1);
        }
        else {
            $http({
                method: 'POST',
                url: 'Parties/BuyerMaster/DeleteActivity?id=' + $scope.buyerMsterActivityId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'buyerActivityPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'buyerActivityPopUp');
                    $scope.SelectedActivityList.splice($scope.bActivityIndex, 1);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure', 'buyerActivityPopUp');
            }).finally(function () {
            });
        }
    };

    //#endregion 

    // #region *********************** Entity PopUp Start 
    $scope.buyerMasterEntityList = [];
    $scope.entitySearchList = [];
    $scope.entityDataList = [];
    $scope.entitySearch = [];
    $scope.entityUrl = 'Organizations/entity/QueryEntityByBuyer/';
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
        //if (baseService.isUndefinedOrNull($scope.buyerMasterNew.BuyerId)) {
        //    ShowResult('Select Activity', 'failure', 'buyerActivityPopUp');
        //    return false;
        //}
        $scope.entityParameters.companyId = $window.companyId;
        $scope.entityParameters.buyerMasterId = $scope.buyerMasterNew.Id;
        $scope.getEntityData = function (pageno) {
            baseService.paginationBase($scope.entityUrl + companyId, pageno, $scope.entityParameters)
                .then(function (response) {
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

    $scope.selectEntityPopUp = function () {
        angular.forEach($scope.entityDataList, function (item) {
            if (item.Flag) {
                if ($filter("filter")($scope.buyerMasterEntityList, { EntityId: item.Id }).length === 0) {
                    var ob = {
                        Id: null,
                        EntityId: item.Id,
                        UserName: item.UserName,
                        Plant: item.Plant,
                        Division: item.Division,
                        Unit: item.Unit,
                        BuyerMasterId: null
                        //SelectedActivityList: []
                    };
                    $scope.buyerMasterEntityList.push(ob);
                }
            }
        });
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };

    $scope.clearEntity = function () {
        $scope.selectedEntityId = null;
        $scope.entityAllowanceNew.DesignationGroupId = null;
        $scope.entityAllowanceNew.EntityId = null;
        $scope.entityAllowanceNew.EntityName = null;
        $scope.clearPosition();
        $scope.entityData = [];
        $scope.entitySearch = [];
    };
    // #endregion *********************** Entity PopUp End

    $scope.searchByList = [
        {
            name: 'Buyer',
            value: 'BuyerName'
        },
        {
            name: 'Department',
            value: 'DepartmentName'
        }
        ,
        {
            name: 'Division',
            value: 'DivisionName'
        },
        {
            name: 'Product',
            value: 'MaterialMasterName'
        }
    ];

    $rootScope.parameters.searchBy = 'BuyerName';
    //$scope.getListData = function () {
    //    baseService.init("Parties/buyerMaster/getList", null, null, null, "BuyerName", "BuyerName");
    //    $scope.getData = function (pageno) {
    //        baseService.pagination(pageno)
    //            .then(function (result) {
    //                $scope.buyerMasters = result.Rows;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure');
    //            }).finally(function () {
    //            });
    //    };
    //    $scope.getData();
    //};
    //$scope.getListData();
    baseService.init("Parties/buyerMaster/getList", null, null, null, "BuyerName", "BuyerName");
    $scope.getData = function (pageno) {
        $scope.buyerActivityList = [];
        $scope.buyerMasterEntityList = [];
        $scope.buyerMasters = [];
        $scope.buyerMaster = {};
        $scope.buyerMasterNew.Id = null;
        $scope.Action = 'Save';
        $scope.buyerMasterNew = { BuyerId: $scope.buyerMasterNew.BuyerId };
        $rootScope.parameters.buyerId = $scope.buyerMasterNew.BuyerId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.buyerMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    function getDetailWithMaster(id) {
        $http.get('Parties/buyerMaster/GetDetailList?masterId=' + id)
            .then(function (response) {
                $scope.buyerMasterEntityList = response.data;
                //angular.forEach($scope.buyerMasterEntityList, function (item, i) {
                //    $scope.buyerMasterEntityList[i].SelectedActivityList = $filter("filter")(response.data, { EntityId: item.EntityId, BuyerMasterId: item.BuyerMasterId });
                //});
            });
    }

    function getActivityByMasterDetail(id) {
        $http.get('Parties/buyerMaster/GetActitvityList?masterDetailId=' + id)
            .then(function (response) {
                $scope.SelectedActivityList = response.data;
            });
    }

    $scope.removeRowModal = function (ob, index) {
        try {
            $scope.message_confirmation = "Are you sure want to delete permanently [" + ob.UserName + "] ";
            angular.element(document.querySelector('#removePopUp')).modal('show');
            $scope.popUpIndex = index;
            $scope.tempId = ob.Id;
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.tempId)) {
            $scope.DeleteEntity($scope.popUpIndex, $scope.tempId);
        } else {
            $scope.buyerMasterEntityList.splice($scope.popUpIndex, 1);
        }
        angular.element(document.querySelector('#removePopUp')).modal('hide');
    };

    $scope.DeleteEntity = function (index, id) {
        $http({
            method: 'POST'
            , url: $scope.path + 'DeleteEntity'
            , data: { 'id': id }
            , dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                getDetailWithMaster($scope.buyerMasterNew.Id);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
        $scope.buyerMasterEntityList.splice(index, 1);
        //}

    };

    // #region Finished Goods
    $scope.popUpList = [];
    $scope.popUpDataList = [];
    $scope.excluedColumnList = [];
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = 'OrderManagements/commitment/GetProductMasterList';
        baseService.setCurrentPage('dataList');
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.popUpList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'search_popup');
                }).finally(function () {
                });
        };
        $scope.getPopUpData();
        angular.element(document.querySelector('#search_popup')).modal('show');
    };
    $scope.getFinishedGoods = function (data) {
        $scope.buyerMasterNew.ProductMasterId = data.Id;
        $scope.buyerMasterNew.ProductMasterName = data.UserName;
        angular.element(document.querySelector('#search_popup')).modal('hide');
    };
    // #endregion


    /*****ResposiblePerson*****************/
    $scope.showEmployeeInformationModal = function () {
        getEmployeeInformationData();
        angular.element(document.querySelector('#employeepopup')).modal('show');
    };
    $scope.excludeList = ['Image', 'Flag'];
    $scope.popUpEmpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode',
        searchBy: 'EmployeeName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.sbEmployeeInformation = [];
    $scope.employeeinformationData = [];
    function getEmployeeInformationData() {
        $scope.popUpTitle = '';
        var popUpUrl = '';
        $scope.popUpTitle = 'Employee Profile';
        popUpUrl = 'employees/EmployeeInformation/GetEmployeeListByCompanyGroup';
        //$scope.popUpEmpParameters.sort = 'EmployeeCode';
        //$scope.popUpEmpParameters.searchBy = 'EmployeeName';
        baseService.setCurrentPage('dataList');
        $scope.loadEIData = function (pageno) {
            baseService.paginationBase(popUpUrl, pageno, $scope.popUpEmpParameters)
                .then(function (result) {
                    $scope.employeeinformationData = result.Rows;
                    $scope.popUpEmpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'employeepopup');
                }).finally(function () {
                });
        };
        $scope.loadEIData();
    }
    $scope.getEmployee = function () {
        var selectedEmplist = $filter("filter")($scope.employeeinformationData, { Flag: true });
        if (selectedEmplist.length > 5) {
            return ShowResult('You have selected more than five employee', 'failure', 'employeepopup');
        }
        angular.forEach($scope.buyerMasterEntityList, function (item) {
            for (var i = 0; i < selectedEmplist.length; i++) {
                var index = i + 1;
                if (index === 1) {
                    item.EmployeeOneId = selectedEmplist[i].SystemId;
                    item.EmployeeOneName = selectedEmplist[i].EmployeeName;
                } else if (index === 2) {
                    item.EmployeeTwoId = selectedEmplist[i].SystemId;
                    item.EmployeeTwoName = selectedEmplist[i].EmployeeName;
                } else if (index === 3) {
                    item.EmployeeThreeId = selectedEmplist[i].SystemId;
                    item.EmployeeThreeName = selectedEmplist[i].EmployeeName;
                } else if (index === 4) {
                    item.EmployeeFourId = selectedEmplist[i].SystemId;
                    item.EmployeeFourName = selectedEmplist[i].EmployeeName;
                } else {
                    item.EmployeeFiveId = selectedEmplist[i].SystemId;
                    item.EmployeeFiveName = selectedEmplist[i].EmployeeName;
                }
            }
        });
        angular.element(document.querySelector('#employeepopup')).modal('hide');
    };
    $scope.closeResponsiblePerson = function () {
        angular.element(document.querySelector('#employeepopup')).modal('hide');
    };
    //#endregion
    //*****ResposiblePerson By Entity*****************/
    $scope.showEmployeeInformationByRowModal = function (id, name, index) {
        $scope.empNme = name;
        $scope.empId = id;
        $scope.entityTempIndex = index;
        getEmployeeInformationByEntityData();

        if ($scope.empNme === 'TaskEmployeeName' || $scope.empNme === 'CostingCreatedBy' || $scope.empNme === 'CostingCheckedBy' || $scope.empNme === 'CostingApprovedBy') {
            angular.element(document.querySelector('#Taskemployeepopup')).modal('show');
        } else {
            angular.element(document.querySelector('#employeepopupByEntity')).modal('show');
        }

    };
    $scope.excludeList = ['Image', 'Flag'];
    $scope.popUpEmpByEntityParameters = {
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

    $scope.sbEmployeeInformation = [];
    $scope.employeeinformationData = [];
    function getEmployeeInformationByEntityData() {
        $scope.popUpTitle = '';
        var popUpUrl = '';
        $scope.popUpTitle = 'Employee Profile';
        popUpUrl = 'employees/EmployeeInformation/GetEmployeeListByCompanyGroup';
        $scope.popUpEmpByEntityParameters.sort = 'EmployeeCode';
        $scope.popUpEmpByEntityParameters.searchBy = 'EmployeeCode';
        baseService.setCurrentPage('dataList');
        $scope.loadEIByEntityData = function (pageno) {
            baseService.paginationBase(popUpUrl, pageno, $scope.popUpEmpByEntityParameters)
                .then(function (result) {
                    $scope.employeeinformationData = result.Rows;
                    $scope.popUpEmpByEntityParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.sbEmployeeInformation) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbEmployeeInformation);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'employeepopupByEntity');
                }).finally(function () {
                });
        };
        $scope.loadEIByEntityData();
    }

    $scope.getEmployeeByEntity = function (data) {
        if (!checkExist($scope.SelectedActivityList, data.SystemId)) {
            $scope.SelectedActivityList[$scope.entityTempIndex][$scope.empId] = data.SystemId;
            $scope.SelectedActivityList[$scope.entityTempIndex][$scope.empNme] = data.EmployeeName;
            angular.element(document.querySelector('#employeepopupByEntity')).modal('hide');
        } else {
            return ShowResult('Selected employee already taken.', 'failure', 'employeepopupByEntity');
        }

    };

    $scope.getTaskEmployee = function (data) {
        if ($scope.empNme === 'TaskEmployeeName') {

            $scope.taskTemplateList[$scope.entityTempIndex][$scope.empId] = data.SystemId;
            $scope.taskTemplateList[$scope.entityTempIndex][$scope.empNme] = data.EmployeeName;
        }
        else if ($scope.empNme === 'CostingCreatedBy') {
            $scope.buyerMasterNew.CostingCreatedBy = data.SystemId;
            $scope.buyerMasterNew.CostingCreatedByName = data.EmployeeName;
        }
        else if ($scope.empNme === 'CostingCheckedBy') {
            $scope.buyerMasterNew.CostingCheckedBy = data.SystemId;
            $scope.buyerMasterNew.CostingCheckedByName = data.EmployeeName;

        }
        else if ($scope.empNme === 'CostingApprovedBy') {
            $scope.buyerMasterNew.CostingApprovedBy = data.SystemId;
            $scope.buyerMasterNew.CostingApprovedByName = data.EmployeeName;

        }

        angular.element(document.querySelector('#Taskemployeepopup')).modal('hide');

    };

    $scope.closeResponsiblePersonByEntity = function () {
        angular.element(document.querySelector('#employeepopupByEntity')).modal('hide');
    };

    $scope.clearResponsible = function () {
        if (!$scope.buyerMasterNew.IsAllAssign) {
            angular.forEach($scope.buyerMasterEntityList, function (item) {
                item.EmployeeOneId = null;
                item.EmployeeOneName = null;
                item.EmployeeTwoId = null;
                item.EmployeeTwoName = null;
                item.EmployeeThreeId = null;
                item.EmployeeThreeName = null;
                item.EmployeeFourId = null;
                item.EmployeeFourName = null;
                item.EmployeeFiveId = null;
                item.EmployeeFiveName = null;
            });
        }
    };

    function checkExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeOneId === Id || list[i].EmployeeTwoId === Id || list[i].EmployeeThreeId === Id || list[i].EmployeeFourId === Id || list[i].EmployeeFiveId === Id) {
                return true;
            }
        }
        return false;
    }

    //#endregion
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerMaster = $scope.buyerMasters[$scope.index];
        $scope.buyerMasterNew = Object.assign({}, $scope.buyerMaster);
        $scope.getBuyerActivity($scope.buyerMasterNew.BuyerId);
        getDetailWithMaster($scope.buyerMasterNew.Id);
        $scope.GetBuyerDepartmentCbo();
        $scope.GetBuyerDivisionCbo();
        $scope.getTaskTemplateData($scope.buyerMasterNew.TaskTemplateMasterId);
        //$scope.getSavedTaskTemplateData($scope.buyerMasterNew.Id);
        if (baseService.isUndefinedOrNull($scope.buyerMasterNew.BuyerDepartmentId)) {
            $scope.buyerMasterNew.BuyerDepartmentId = "ALL";
        }
        if (baseService.isUndefinedOrNull($scope.buyerMasterNew.BuyerDivisionId)) {
            $scope.buyerMasterNew.BuyerDivisionId = "ALL";
        }
        $scope.GetBuyerMasterEntity();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    function getBuyerMasterDetailSaveList() {
        angular.forEach($scope.buyerMasterEntityList, function (item) {
            angular.forEach(item.SelectedActivityList, function (item2) {
                var ob = {
                    EntityId: item2.EntityId,
                    BuyerActivityId: item2.BuyerActivityId,
                    BuyerMasterId: item2.BuyerMasterId,
                    EmployeeOneId: item2.EmployeeOneId,
                    EmployeeTwoId: item2.EmployeeTwoId,
                    EmployeeThreeId: item2.EmployeeThreeId,
                    EmployeeFourId: item2.EmployeeFourId,
                    EmployeeFiveId: item2.EmployeeFiveId
                };
                $scope.buyerMasterDetailSaveList.push(ob);
            });
        });
    }

    $scope.Save = function () {
        try {

            angular.copy($scope.buyerMasterNew, $scope.buyerMaster);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.buyerMasterNewForm.$valid) {

                //if (baseService.arrayLength($scope.buyerMasterEntityList) < 0 || baseService.arrayLength($scope.buyerMasterEntityList) == 0) {
                //    throw "Entity is required.";
                //}

                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'buyerMaster': $scope.buyerMaster, 'buyerMasterDetails': $scope.buyerMasterEntityList, 'activityList': $scope.SelectedActivityList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            ClearFields();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: { 'buyerMaster': $scope.buyerMaster, 'buyerMasterDetails': $scope.buyerMasterEntityList, 'activityList': $scope.SelectedActivityList },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getData();
                            ClearFields();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.taskTemplateList = [];
    $scope.getTaskTemplateData = function () {
        $scope.taskTemplateList = [];
        $http.get("Parties/BuyerMaster/GetTaskData?taskTemplateMasterId=" + $scope.buyerMasterNew.TaskTemplateMasterId)
            .then(
                function successCallback(response) {
                    //if (baseService.arrayLength(response.data) > 0) {
                    $scope.taskTemplateList = response.data;
                    //}

                    $scope.getSavedTaskTemplateData($scope.buyerMasterNew.Id);
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });

    };

    $scope.getSavedTaskTemplateData = function (buyerMasterId) {
        $http.get("Parties/BuyerMaster/GetSavedBuyerTaskData?buyerMasterId=" + buyerMasterId)
            .then(
                function successCallback(response) {


                    if (baseService.arrayLength(response.data) > 0) {
                        for (var i = 0; i < response.data.length; i++) {
                            for (var j = 0; j < $scope.taskTemplateList.length; j++) {
                                if (!baseService.isUndefinedOrNull(response.data[i].Id)) {
                                    if ($scope.taskTemplateList[j].TaskMasterId === response.data[i].TaskMasterId) {
                                        $scope.taskTemplateList[j].Id = response.data[i].Id;
                                        $scope.taskTemplateList[j].TaskEmployeeName = response.data[i].TaskEmployeeName;
                                        $scope.taskTemplateList[j].EmpSystemId = response.data[i].EmpSystemId;
                                        $scope.taskTemplateList[j].Active = true;
                                    }
                                }
                            }
                        }
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.savedTaskTemplateList = [];
    $scope.SaveTask = function () {
        try {
            $scope.savedTaskTemplateList = [];
            for (var i = 0; i < $scope.taskTemplateList.length; i++) {
                if ($scope.taskTemplateList[i].Active) {
                    $scope.taskTemplateList[i].BuyerMasterId = $scope.buyerMasterNew.Id;
                    $scope.savedTaskTemplateList.push($scope.taskTemplateList[i]);
                }
                if (!baseService.isUndefinedOrNull($scope.taskTemplateList[i].Id)) {
                    if (checkExist($scope.savedTaskTemplateList, $scope.taskTemplateList[i].TaskMasterId) === false) {
                        $scope.savedTaskTemplateList.push($scope.taskTemplateList[i]);
                    }
                }

            }

            $http({
                method: 'POST',
                url: $scope.saveTaskUrl,
                data: { 'entities': $scope.savedTaskTemplateList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getSavedTaskTemplateData($scope.buyerMasterNew.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TaskMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.buyerMaster = {};
        $scope.buyerMasterNew = { BuyerId: $scope.buyerMasterNew.BuyerId };
        $scope.employeeinformationData = [];
        $scope.buyerMasterEntityList = [];
        $scope.entityDataList = [];
        $scope.BuyerMasterEntityList = [];
        $scope.taskTemplateList = [];
    }

    // #region checkbox all MO Entity
    $scope.entityList = [];
    $scope.BuyerMasterEntityList = [];
    $scope.MOentityPopUp = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.buyerMasterNew.Id)) {
                throw "Select Buyer Definition.";
            }
            $http({
                method: 'GET',
                url: 'Parties/BuyerMaster/GetMasterOrderEntity'
            }).then(function successCallback(response) {
                $scope.entityList = response.data;
            });
            angular.element(document.querySelector('#MPOentityPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetBuyerMasterEntity = function () {
        $http({
            method: 'GET',
            url: 'Parties/BuyerMaster/GetBuyerMasterEntity?masterId=' + $scope.buyerMasterNew.Id
        }).then(function successCallback(response) {
            $scope.BuyerMasterEntityList = response.data;
        });

    };

    $scope.GetBuyerMasterEntityPOPUp = function () {
        angular.element(document.querySelector('#BuyerMasterEntityPopUp')).modal('show');
    };
    $scope.CloseBuyerMasterEntity = function () {
        angular.element(document.querySelector('#BuyerMasterEntityPopUp')).modal('hide');
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridENT").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.entityList.length; i++) {
                $scope.entityList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridENT").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.selectedentityList = [];
    function MakeData() {
        for (var i = 0; i < $scope.entityList.length; i++) {
            if ($scope.entityList[i].Flag == true) {
                if (checkExists($scope.selectedentityList, $scope.entityList[i].EntityId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.BuyerMasterId = $scope.buyerMasterNew.Id;
                    ob.EntityId = $scope.entityList[i].EntityId;

                    $scope.selectedentityList.push(ob);
                    ob = {};
                }
                else {
                    throw "This Entity " + $scope.entityList[i].UserName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EntityId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseMOE = function () {
        try {
            MakeData();
            $scope.SaveMOE();
            angular.element(document.querySelector('#MPOentityPopUp')).modal('hide');

        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveMOE = function () {
        try {

            $http({
                method: 'POST',
                url: 'Parties/BuyerMaster/SaveMOE',
                data: { 'data': $scope.selectedentityList, 'masterId': $scope.buyerMasterNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetBuyerMasterEntity();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    // #endregion checkbox all


}