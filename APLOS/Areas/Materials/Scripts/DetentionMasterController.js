'use strict';
DetentionMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function DetentionMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "DetentionMaster";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';
    $scope.ProcesssaveUrl = $scope.path + 'CreateProcess';
    $scope.DepartmentSaveUrl = $scope.path + 'CreateDepartment';
    $scope.MachineSaveUrl = $scope.path + 'CreateMachine';
    $scope.ResponsibleSaveUrl = $scope.path + 'CreateResponsible';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListInChargePerson';

    $scope.detention = {
        Id: null
        , DetentionCategory: null
        , DetentionSubCategory: null
        , DetentionStandaredName: null
        , DetentionUserName: null
        , DetentionType: null
        , DetentionCriticality: null
        , InchargePersonId: null
        , InchargePerson:null
        , DetentionTarget: null
        , DetentionPlan: null
        , IsAvoidable: false
        , IsAssetApplicable: false
        , IsWorkCenterApplicable: false
    };
    $scope.detentionNew = Object.assign({}, $scope.detention);

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }

    $scope.DetentionList = [];
    $scope.LoadDetentionList = function () {
        $http({

            method: 'Get',
            url: 'Materials/DetentionMaster/LoadDetentionList'
        }).then(function successCallback(response) {
            $scope.DetentionList = response.data;
        }
        )
    }
    $scope.LoadDetentionList();

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };
    $scope.DetentionTest = null;
    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.Name === 'ip') {
                $scope.detentionNew.InchargePersonId = employee.SystemId;
               /* $scope.detentionNew.InchargePerson = employee.EmployeeName;*/
                $scope.DetentionTest = employee.EmployeeName;
            }
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.DetentionMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DetentionData': $scope.detentionNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.LoadDetentionList();
                    DetentionClearFields();
                   /* $scope.GetDetails({ data: { Id: response.data.Data.Id } });*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.rackNew.Id)) {
    //        $http({
    //            method: 'POST'
    //            , url: $scope.path + 'Delete?Id=' + $scope.rackNew.Id
    //            , dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');                   
    //                ClearFields(response.data.Sequence);
    //                $scope.LoadRackList();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GetDetails = function (args) {
        $scope.DetentionMasterId = args.data.Id;
       /* $scope.DetentionTest = args.data.InchargePerson;*/
        $http({
            method: 'Get',
            url: 'Materials/DetentionMaster/LoadEditData?DetentionID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.detentionNew = response.data.detention[0];
            $scope.DetentionTest = response.data.detention[0].InChargePerson;
            $scope.getDetentionMasterProcess();
            $scope.getDetentionMasterDepartment();
            $scope.getDetentionMasterMachine();
            $scope.getDetentionMasterResponsible();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.recorddoubleclick = function ($event) {
        
        var x = $event;
        $scope.DetentionMasterId = x.data.Id;

        // $scope.modelNew.OperationMasterIdID = response.data.Id;  
        /* $scope.GetDataByMasterOrderIdfn($scope.DetentionMasterId);*/
        // $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action = 'Update';
        $scope.getDetentionMasterProcess();
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.getDetentionMasterProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userProcessList = [];
            $scope.userProcessList = resp.data;
        });
    }
    $scope.getDetentionMasterDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDepartment',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userDepartMentList = [];
            $scope.userDepartMentList = resp.data;
        });
    }
    $scope.getDetentionMasterMachine = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMachine',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userMachineList = [];
            $scope.userMachineList = resp.data;
        });
    }

    $scope.getDetentionMasterResponsible = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResponsible',
            data: { 'DetentionMasterId': $scope.DetentionMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.userResponsibleList = [];
            $scope.userResponsibleList = resp.data;
        });
    }

    $scope.Clear = function () {
        DetentionClearFields();
        $scope.userDepartMentList = [];
        $scope.userProcessList = [];
        $scope.userMachineList = [];
        $scope.userResponsibleList = [];
    };
    function DetentionClearFields() {
        $scope.Action = "Save";
        $scope.DetentionTest = null;
        $scope.detentionNew = Object.assign({}, $scope.detention);

    }

    $scope.processPopUpDataList = function () {
        $scope.processDataList = [];
        $scope.processSearchList = [];
        $rootScope.tempList = [];
        CloseShowResult();
        CloseModalShowResult();
        $scope.processPopUpParameters = {
            limit: 10
            , offset: 0
            , order: 'asc'
            , sort: 'UserName'
            , searchBy: "UserName"
            , pageSize: 10
            , total_count: 0
            , search: null
            , serverPagination: true
        };
        $scope.processUrl = 'Processes/Process/GetList?processId=[]';
        baseService.setCurrentPage('processDataList');
        $scope.getProcessDataList = function (pageno) {
            baseService.paginationBase($scope.processUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;

                    if (baseService.arrayLength($scope.userProcessList) > 0) {
                        for (var i = 0; i < $scope.userProcessList.length; i++) {
                            for (var j = 0; j < $scope.processDataList.length; j++) {
                                if ($scope.userProcessList[i].ProcessId === $scope.processDataList[j].Id) {
                                    $scope.processDataList[j].Flag = true;
                                }
                            }
                        }
                    }
                    if (baseService.arrayLength($scope.processSearchList) === 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.processSearchList);
                    angular.element(document.querySelector('#processPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        $scope.getProcessDataList();
    };

    $scope.userProcessList = [];

    $scope.DepartmentPopUpList = function () {
        $http({
            method: 'GET',
            url: 'Materials/DetentionMaster/LoadDepartmentList'
        }).then(function successCallback(response) {
            $scope.DepartmentDataList = response.data;
            for (var i = 0; i < $scope.userDepartMentList.length; i++) {
                for (var j = 0; j < $scope.DepartmentDataList.length; j++) {
                    if ($scope.userDepartMentList[i].DepartmentId === $scope.DepartmentDataList[j].Id) {
                        $scope.DepartmentDataList[j].chk = true;
                    }
                }
            }
            angular.element(document.querySelector('#departmentPopUp')).modal('show');
        });
    };
    $scope.userDepartMentList = [];

    $scope.MachinePopUpList = function () {
        $http({
            method: 'GET',
            url: 'Materials/DetentionMaster/LoadMachineList'
        }).then(function successCallback(response) {
            $scope.MachineDataList = response.data;
            for (var i = 0; i < $scope.userMachineList.length; i++) {
                for (var j = 0; j < $scope.MachineDataList.length; j++) {
                    if ($scope.userMachineList[i].MachineMasterId === $scope.MachineDataList[j].Id) {
                        $scope.MachineDataList[j].chk = true;
                    }
                }
            }
            angular.element(document.querySelector('#MachinePopUp')).modal('show');
        });
    };
    $scope.userMachineList = [];

    $scope.ResponsiblePopUpList = function () {
        $http({
            method: 'GET',
            url: 'Materials/DetentionMaster/LoadResponsibleList'
        }).then(function successCallback(response) {
            $scope.ResponsibleDataList = response.data;
            for (var i = 0; i < $scope.userResponsibleList.length; i++) {
                for (var j = 0; j < $scope.ResponsibleDataList.length; j++) {
                    if ($scope.userResponsibleList[i].ResponsibleMasterId === $scope.ResponsibleDataList[j].Id) {
                        $scope.ResponsibleDataList[j].chk = true;
                    }
                }
            }
            angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('show');
        });
    };
    $scope.userResponsibleList = [];

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };
    $scope.processDataList = [];
    $scope.SaveProcess = function () {

        try {

            if (baseService.arrayLength($scope.processDataList) > 0) {
                angular.forEach($scope.processDataList, function (a) {
                    if (checkProcessExist($scope.userProcessList, a.Id) === false) {
                        if (a.Flag) {
                            var ob = {};
                            ob.Id = null;
                            ob.ProcessId = a.Id;
                            ob.Code = a.Code;
                            ob.Sequence = a.Sequence;
                            ob.ShortName = a.ShortName;
                            ob.StandardName = a.StandardName;
                            ob.ProcessName = a.UserName;
                            $scope.userProcessList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.ProcesssaveUrl,
                data: { 'data': $scope.userProcessList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getDetentionMasterProcess();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeProcessPopUp();
    };
    $scope.DepartmentDataList = [];
    $scope.SaveDepartment = function () {

        try {

            if (baseService.arrayLength($scope.DepartmentDataList) > 0) {
                angular.forEach($scope.DepartmentDataList, function (a) {
                    if (checkDeptExist($scope.userDepartMentList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            ob.DepartmentId = a.Id;
                            ob.Code = a.Code;
                            ob.Sequence = a.Sequence;
                            ob.ShortName = a.ShortName;
                            ob.StandardName = a.StandardName;
                            ob.ProcessName = a.UserName;
                            $scope.userDepartMentList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.DepartmentSaveUrl,
                data: { 'data': $scope.userDepartMentList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getDetentionMasterDepartment();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeDeptPopUp();
    };
    $scope.MachineDataList = [];
    $scope.SaveMachine = function () {

        try {

            if (baseService.arrayLength($scope.MachineDataList) > 0) {
                angular.forEach($scope.MachineDataList, function (a) {
                    if (checkMachineExist($scope.userMachineList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            ob.MachineMasterId = a.Id;
                            ob.Code = a.Code;
                            ob.Sequence = a.Sequence;
                            ob.ShortName = a.ShortName;
                            ob.StandardName = a.StandardName;
                            ob.MachineName = a.UserName;
                            $scope.userMachineList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.MachineSaveUrl,
                data: { 'data': $scope.userMachineList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.processDataList();
                    $scope.getDetentionMasterMachine();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeMachinePopUp();
    };
    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePopUp')).modal('hide');
    };

    $scope.ResponsibleDataList = [];
    $scope.SaveResponsible = function () {

        try {

            if (baseService.arrayLength($scope.ResponsibleDataList) > 0) {
                angular.forEach($scope.ResponsibleDataList, function (a) {
                    if (checkResponsibleExist($scope.userResponsibleList, a.Id) === false) {
                        if (a.chk) {
                            var ob = {};
                            ob.Id = null;
                            ob.ResponsibleMasterId = a.SystemId;
                            ob.EmployeeCode = a.EmployeeCode;
                            ob.EmployeeName = a.EmployeeName;
                            ob.Department = a.Department;
                            ob.Section = a.Section;
                            ob.SubSection = a.SubSection;
                            ob.LegalDesignation = a.LegalDesignation;
                            $scope.userResponsibleList.push(ob);
                            ob = {};
                        }
                    }

                });
            }

            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.ResponsibleSaveUrl,
                data: { 'data': $scope.userResponsibleList, 'DetentionMasterId': $scope.detentionNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getDetentionMasterResponsible();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeResponsiblePopUp();
    };
    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopUp')).modal('hide');
    };

    function checkProcessExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === Id) {
                return true;
            }
        }
        return false;
    }
    function checkDeptExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DepartmentId === Id) {
                return true;
            }
        }
        return false;
    }
    function checkMachineExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MachineMasterId === Id) {
                return true;
            }
        }
        return false;
    }

    function checkResponsibleExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ResPonsibleMasterId === Id) {
                return true;
            }
        }
        return false;
    }
    $scope.closeDeptPopUp = function () {
        angular.element(document.querySelector('#departmentPopUp')).modal('hide');
    };

    $scope.closeProcessPopUp = function () {
        $scope.processUpUrl = null;
        $scope.processDataList = [];
        $scope.processSearchList = [];
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.removeRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeDeptRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemoveDeptPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeMachineRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemoveMachinePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeResponsibleRowModal = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to delete [" + name + "] permanently ?";
            angular.element(document.querySelector('#confirmRemoveResponsiblePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/ProcessDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterProcess();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeDeptRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/DepartmentDelete?id=' + $scope.tempDeptId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterDepartment();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeMachineRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/MachineDelete?id=' + $scope.tempDeptId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterMachine();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.removeResponsibleRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionMaster/ResponsibleDelete?id=' + $scope.tempDeptId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionMasterResponsible();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.DepartmentGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.DepartmentDataList.length; i++) {
            $scope.DepartmentDataList[i].chk = ChkOrUnchk;
        }

        var gridObj = $("#GridDetentionMasterDepartment").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.MachineGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        for (var i = 0; i < $scope.MachineDataList.length; i++) {
            $scope.MachineDataList[i].chk = ChkOrUnchk;
        }

        var gridObj = $("#GridDetentionMasterMachine").data("ejGrid");
        gridObj.refreshContent();
    };



}