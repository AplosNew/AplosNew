'use strict';
ProcessWiseProductionBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ProcessWiseProductionBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Process Wise Production Booking Controller';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/ProcessWiseProductionBooking/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);

    $scope.ModelTransaction = {
        Id: null,
        EntityId: null,
       
        Date: new Date().toLocaleString('en-US', { timeZone: 'UTC' }),
        ProcessId: null,           
        ShiftId: null,          
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remark: null,
        ProductionQuantity: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);

    $scope.EntityList = [];
    $scope.selectEntity = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEntity',          
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }
    $scope.selectEntity();


    
   /* $scope.GetworkcenterData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetWCCbo',
            data: {
                'processId': $scope.ModelNew.ProcessId,
                'entityId': $scope.ModelNew.EntityId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.workcenterList = response.data;
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }*/

    $scope.doubleEntity = function (e) {
        $scope.ModelNew.EntityId = e.data.EntityId;
        $scope.ModelNew.Entity = e.data.EntityName;
        $scope.GetworkcenterData();
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

    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.getEmployee();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        $scope.getEmployee();
    }
    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
       
    }

    $scope.EmployeeId = null;
    $scope.EmployeeName = null;
    $scope.doubleEmploye = function (e) {
        $scope.EmployeeId = e.data.SystemId;
        $scope.EmployeeName = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
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
    $scope.getsS();

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
            data: {
                'entityId': $scope.ModelNew.EntityId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    //$scope.getsP();

    $scope.doubleProcess = function (e) {
        $scope.ModelNew.ProcessId = e.data.Id;
        $scope.ModelNew.Process = e.data.Process;
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPop')).modal('hide');
    }

    $scope.ArticleList = [];
    $scope.getArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getArticle',
            dataType:'JSON',
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
        });
    }

    $scope.Save = function () {
        $scope.check_uncheck();
        try {
            angular.copy($scope.ModelNew, $scope.ModelTransaction);
            $scope.$broadcast('show-errors-check-validity');

                $http({
                    method: 'POST',
                    url: $scope.path + 'Save',
                    data: {
                        'data': $scope.ModelNew,
                        'responsiblepersonId': $scope.EmployeeId,

                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Data.Id;
                        $scope.SaveChild();
                        $scope.getData();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');

                }
            
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }

    };

    $scope.SaveChild = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.path + 'SaveChild',
                data: {
                    'headerId': $scope.ModelNew.Id,
                    'workcenterlist': $scope.chkdWorkCenterList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //window.location.href = $scope.path +'Aplos'
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }

        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    }

    $scope.Clear = function () {
        $scope.EmployeeId = null;
        $scope.ModelNew = {
            Id: null,
            EntityId: null,
            Date: new Date().toLocaleString('en-US', { timeZone: 'UTC' }),
            ProcessId: null,
            ShiftId: null,
            ResponsiblePersonId: null,
            ResponsiblePerson: null,
            Remark: null,
            ProductionQuantity:null,
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
        $scope.GetworkcenterData();
        $scope.getAssetWorkCenterApplicable();
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

    $scope.GriddataMachineMasterData = [];
    $scope.getData = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            url: 'Productions/ProcessWiseProductionBooking/GetMachineMasterTransaction',
        }).then(function successCallback(response) {
            $scope.GriddataMachineMasterData = response.data;
        });
    };
    $scope.getData();

    $scope.workcenterList = [];
    $scope.processWiseGridView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetWCCbo',
            data: {
                'processId': $scope.ModelNew.ProcessId,
                'entityId': $scope.ModelNew.EntityId,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.workcenterList = response.data;
        });
    }

    //=============================================PROCESS WISE BOOKIN CHECK BOX================================

    $scope.chkdWorkCenterList = [];
    $scope.check_uncheck = function () {
        for (var i = 0; i < $scope.workcenterList.length; i++) {
            //if ($scope.workcenterList[i].IsSelectSlrProc === true) {
            //    $scope.chkdWorkCenterList.push($scope.workcenterList[i])
            //}

            if ($scope.workcenterList[i].ProductionQuantity != null) {
                $scope.chkdWorkCenterList.push($scope.workcenterList[i])
            }
        }
    }

    /*$scope.ActiveProcessWisecbx = function (args) {
        $("#cbxhead").ejCheckBox({ "change": chkFilteredData });
    };

    function chkFilteredData(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridProcessWise").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.workcenterList.length; i++) {
                $scope.workcenterList[i].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].IsSelectSlrProc = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridOTCompensation").data("ejGrid");
       // gridObj.refreshContent();
    };*/
    //=============================================PROCESS WISE BOOKIN CHECK BOX================================
}