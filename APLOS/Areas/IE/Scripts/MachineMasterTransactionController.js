'use strict';
MachineMasterTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function MachineMasterTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Machine Master Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'IE/MachineMasterTransaction/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $scope.year = new Date().getFullYear().toString();

    //Omar Start

    $scope.ModelTransaction = {
        Id: 0, 
        EntityId: 0,
        Entity: null,
        DetentionId: null,
        Detention: null, 
        DetentionTypeId: null,
        DetentionType: null,
        Date: null,
        MachineMaster: null,
        MachineMasterId: null,
        ProcessId: null,
        Process: null,
        FromTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' }),
        ToTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' }),
        Minute: null,
        DepartmentId: null,
        Department: null,
        ShiftId: null,
        Shift: null,
        IfAssetApplicable: false,
        SelectAsset: null,
        ResponsiblePersonCode: null, 
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remark: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);


    $scope.selectEntity = function () {
        $scope.getsE();
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

    $scope.EntityList = [];
    $scope.getsE = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEntity',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
    }

    $scope.doubleEntity = function (e) {
            $scope.ModelNew.EntityId = e.data.EntityId;
            $scope.ModelNew.Entity = e.data.EntityName;
            angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

    $scope.selectDepartment = function () {
        $scope.getsD();
        angular.element(document.querySelector('#DepartmentPop')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getsD = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDepartment',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DepartmentList = resp.data;
        });
    }

    $scope.doubleDepartment = function (e) {
        $scope.ModelNew.DepartmentId = e.data.DepartmentId;
        $scope.ModelNew.Department = e.data.DepartmentName;
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }


    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }


    $scope.selectMachine = function () {
        $scope.getsM();
        angular.element(document.querySelector('#MachinePop')).modal('show');
    }

    $scope.MachineList = [];
    $scope.getsM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMachine',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MachineList = resp.data;
        });
    }

    $scope.doubleMachine = function (e) {
        $scope.ModelNew.MachineMasterId = e.data.MachineMasterId;
        $scope.ModelNew.MachineMaster = e.data.MachineMaster;
        angular.element(document.querySelector('#MachinePop')).modal('hide');
    }

    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePop')).modal('hide');
    }

    $scope.selectProcess = function () {
        $scope.getsP();
        angular.element(document.querySelector('#ProcessPop')).modal('show');
    }

    $scope.ProcessList = [];
    $scope.getsP = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getProcess',
            data: { 'machineMasterId': $scope.ModelNew.MachineMasterId },
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });
    }

    $scope.doubleProcess = function (e) {
        $scope.ModelNew.ProcessId = e.data.Id;
        $scope.ModelNew.Process = e.data.Process;
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }

    $scope.DetentionList = [];
    $scope.GetDetentionList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionList'
        }).then(function successCallback(response) {
            $scope.DetentionList = response.data;
        });
    }
    $scope.GetDetentionList();

    $scope.DetentionTypeList = [];
    $scope.GetDetentionTypeList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionTypeList'
        }).then(function successCallback(response) {
            $scope.DetentionTypeList = response.data;
        });
    }
    $scope.GetDetentionTypeList();


    $scope.Save = function () {
        try {

            angular.copy($scope.ModelNew, $scope.ModelTransaction);
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
                        $scope.ModelNew.Id = response.data.Id;
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');

                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }

    };

    $scope.Clear = function () {
        $scope.ModelNew = {
            Id: 0,
            EntityId: 0,
            Entity: null,
            DetentionId: null,
            Detention: null,
            DetentionTypeId: null,
            DetentionType: null,
            Date: null,
            Machine: null,
            MachineId: null,
            ProcessId: null,
            Process: null,
            FromTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' }),
            ToTime: new Date().toLocaleTimeString({}, { hour12: true, hour: 'numeric', minute: 'numeric' }),
            Minute: null,
            DepartmentId: null,
            Department: null,
            ShiftId: null,
            Shift: null,
            IfAssetApplicable: false,
            SelectAsset: null,
            ResponsiblePersonId: null,
            ResponsiblePerson: null,
            Remark: null,
        };
        $scope.Action = 'Save';
    };

    //Omar End

    $scope.ModelMeetItem = {
        Id: null,
        MeetingAgendaId: null,
        MeetingItemHeaderId: null,
    };
    $scope.ModelMeetingItem = Object.assign({}, $scope.ModelMeetItem);


    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
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
                    $scope.getData();
                    $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

   

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


    $scope.Name = null;
    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;
        
        $scope.ModelNew.ResponsiblePersonId = data.SystemId;
        $scope.ModelNew.ResponsiblePerson = data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };


    

   


    function checkDoubleMeeting(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SystemId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMeetingWise });
    };

    function CheckBoxSelectAllMeetingWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#meetingInfoGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MeetingList.length; i++) {
                $scope.MeetingList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Active = ChkOrUnchk;
            }
        }
        var gridObj = $("#meetingInfoGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    function checkChangeMeeting(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.EmployeeBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }

    function headCheckChangeMeeting(e) {
        if (e.model.checkState == "check") {

            var filtered = $("#Gridmeeting").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MeetingList.length; i++) {

                    $scope.MeetingList[i].Active = true;
                }
            }
            else {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MeetingList[i].Id == filtered[j].Id)
                            // $scope.ModelList[i].isSelect = true;
                            $scope.MeetingList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridmeeting .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeMeeting });
            }
        }
        else {
            var filtered = $("#Gridmeeting").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    $scope.MeetingList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.MeetingList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MeetingList[i].Id == filtered[j].Id)
                            $scope.MeetingList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridmeeting.rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridmeeting.rowCheckbox")[i]).ejCheckBox({ "change": checkChangeMeeting });
            }
        }
        //header level check
    }

    $scope.dataBoundemployee = function (args) {
        $("#Gridmeeting .rowCheckbox").ejCheckBox({ "change": checkChangeMeeting });
        $("#headchk").ejCheckBox({ "change": headCheckChangeMeeting });

    };

    
}