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
        Id: null,
        EntityId: null,
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
        FromTime: null,
        ToTime: null,
        Minute: null,
        DepartmentId: null,
        Department: null,
        ShiftId: null,
        Shift: null,
        //IfAssetApplicable: false,
        AssetId: null,
        Asset: null,
        ResponsiblePersonCode: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        Remark: null,
        DetentionCodeId: null,
        DetentionCode: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTransaction);
    $scope.IsVisible = false;
    $scope.EntityList = [];
    $scope.selectEntity = function () {
        $http({
            method: 'POST',
            //url: $scope.path + 'getEntity',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity",
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });
        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

   

    $scope.workcenterList = [];
    $scope.GetworkcenterData = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetWCCbo?entityId=' + $scope.ModelNew.EntityId + '&processId=' + $scope.ModelNew.ProcessId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.workcenterList = response.data;
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GetworkcenterData();
    $scope.GetEntityProcessByWorkCenter = function (workcenterid) {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetEntityProcessByWorkCenter?id=' + workcenterid
        }).then(function successCallback(response)
        {
            $scope.ModelNew.EntityId = response.data[0].EntityId;
            $scope.ModelNew.ProcessId = response.data[0].ProcessId;
            $scope.ModelNew.Entity = response.data[0].Entity;
            $scope.ModelNew.Process = response.data[0].Process;
        });
    }

    $scope.doubleEntity = function (e) {
        $scope.ModelNew.EntityId = e.data.Id;
        $scope.ModelNew.Entity = e.data.UserName;
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
        $scope.GetworkcenterData();
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
    $scope.IsAssetApplicable = false; $scope.IsWorkCenterApplicable = false; $scope.IsMachineParaApplicable = false;
    $scope.getAssetWorkCenterApplicable = function () {
        for (var i = 0; i < $scope.DetentionList.length; i++) {
            if ($scope.DetentionList[i].Value == $scope.ModelNew.DetentionId) {
                $scope.IsAssetApplicable = $scope.DetentionList[i].IsAssetApplicable;
                $scope.IsWorkCenterApplicable = $scope.DetentionList[i].IsWorkCenterApplicable;
                $scope.IsMachineParaApplicable = $scope.DetentionList[i].IsMachineParaApplicable;
            }
        }
    }

    $scope.selectAsset = function () {
        $scope.GetAssetTypeList();
        angular.element(document.querySelector('#AssetPop')).modal('show');
    }

    $scope.AssetTypeList = [];
    $scope.GetAssetTypeList = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterTransaction/GetAssetTypeList',
            data: { 'machineMasterId': $scope.ModelNew.MachineMasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.AssetTypeList = response.data;
        });
    }
    $scope.GetAssetTypeList();

    $scope.doubleAsset = function (e) {
        $scope.ModelNew.AssetId = e.data.Id;
        $scope.ModelNew.Asset = e.data.AssetName;
        angular.element(document.querySelector('#AssetPop')).modal('hide');
    }

    $scope.closeAssetPopUp = function () {
        angular.element(document.querySelector('#AssetPop')).modal('hide');
    }


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

    $scope.GetDetentionTypeById = function (detentionid) {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionTypeById?id=' + detentionid
        }).then(function successCallback(response) {
            $scope.ModelNew.DetentionType = response.data[0].DetentionType;
            $scope.ModelNew.DetentionTypeId = response.data[0].DetentionTypeId;
            
        });
    }

    $scope.DetentionCodeList = [];
    $scope.GetDetentionCodeList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionCodeList'
        }).then(function successCallback(response) {
            $scope.DetentionCodeList = response.data;
        });
    }
    $scope.GetDetentionCodeList();

    $scope.GetDetentionTypeAndDetentionByCode = function (detentioncode) {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionTypeAndDetentionByCode?code=' + detentioncode
        }).then(function successCallback(response) {
            $scope.ModelNew.DetentionType = response.data[0].DetentionType;
            $scope.ModelNew.DetentionTypeId = response.data[0].DetentionTypeId;
            $scope.ModelNew.Detention = response.data[0].Detention;
            $scope.ModelNew.DetentionId = response.data[0].DetentionId;

        });
    }

    $scope.changeAsset = function () {
        if ($scope.ModelNew.IfAssetApplicable == false) {
            $scope.ModelNew.AssetId = null;
            $scope.ModelNew.Asset = null;
        }

    }

    $scope.getMinute = function () {
        try {
            $scope.MinuteUrl = 'IE/MachineMasterTransaction/GetMinute/'
            $http({
                method: 'POST',
                url: $scope.MinuteUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.ModelNew.Minute = response.data;
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Save = function () {
        try {
            angular.copy($scope.ModelNew, $scope.ModelTransaction);
            $scope.$broadcast('show-errors-check-validity');

            if ($scope.ModelNewForm.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'data': $scope.ModelNew,
                        'DetentionParaList': $scope.DetentionParaList
                    },
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
            Id: null,
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
            FromTime:null,
            ToTime: null,
            Minute: null,
            DepartmentId: null,
            Department: null,
            ShiftId: null,
            Shift: null,
            IfAssetApplicable: false,
            AssetId: null,
            ResponsiblePersonId: null,
            ResponsiblePerson: null,
            Remark: null,
        };
        $scope.Action = 'Save';
        $scope.DetentionParaList = [];
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
        if ($scope.IsMachineParaApplicable == true) {
            $scope.IsVisible = true;
            $scope.getDetentionParaPoPUp();
        }
        else {
            $scope.IsVisible = false;
        }
        $scope.GetworkcenterData();
        $scope.getAssetWorkCenterApplicable();
        $scope.loadParameterList();
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
            url: 'IE/MachineMasterTransaction/GetMachineMasterTransaction',
        }).then(function successCallback(response) {
            $scope.GriddataMachineMasterData = response.data;
        });
    };
    $scope.getData();
    $scope.ParameterList = [];
    $scope.loadParameterList = function () {
        try {
            $scope.ParameterList = [];
            $http.get('IE/MachineMasterTransaction/GetDetentionParameter?DetentionId=' + $scope.ModelNew.DetentionId)
                .then(
                    function successCallback(response) {
                        $scope.ParameterList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };

    $scope.DetentionParaList = [];
    $scope.getDetentionParaPoPUp = function (data) {
        try {
            $scope.DetentionParaList = [];
            $http.get('Productions/ProductionSummary/GetDetentionParaData?DetentionId=' + $scope.ModelNew.DetentionId + '&processId=' + $scope.ModelNew.ProcessId + '&masterId=' + $scope.ModelNew.Id)
                .then(
                    function successCallback(response) {
                        $scope.DetentionParaList = response.data;
                        if ($scope.IsMachineParaApplicable == true) {
                            $scope.IsVisible = true;
                        }
                        else {
                            $scope.IsVisible = false;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.CalculateDetention = function () {
        try {
            /*$scope.NewObject.Quantity = 0;*/
            $http({
                method: 'POST',
                url: 'Productions/ProductionSummary/CalculateDetention',
                data: { 'OpenHeadNew': $scope.DetentionParaList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if ($scope.IsMachineParaApplicable == true) {
                for (var i = 0; i < response.data.NewData.length; i++) {
                    for (var j = 0; j < $scope.DetentionParaList.length; j++) {
                        if (response.data.NewData[i].UserName == $scope.DetentionParaList[j].UserName) {
                            $scope.DetentionParaList[j].Value = response.data.NewData[i].Value;
                        }
                    }
                    $scope.Save();
                    //if (response.data.NewData[i].IsProduction == true) {
                    //    $scope.NewObject.Quantity += response.data.NewData[i].Value;
                    //}
                }
            }
                else
                {
                    $scope.Save();
                }
            }, function errorCallback(response) {
                $scope.ShowResultCustom(response.status.Message, "failure");
            });
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    }
}