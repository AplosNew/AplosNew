'use strict';
maintenanceSchedulingController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function maintenanceSchedulingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "MaintenanceScheduling";
    $scope.CategoryList = [];
    $scope.SubCategoryList = [];
    $scope.CriticalLevelLists = [];
    $scope.ItemTypeList = [];
    $scope.ItemCategoryList = [];
    $scope.CostTypeList = [];
    $scope.EstimationLevelList = [];
    $scope.GroupList = [];
    $scope.Action = 'Save';
    $scope.path = 'Machines/MaintenanceScheduling/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlMachine = $scope.path + 'createMachineGroup';
    $scope.saveUrlAsset = $scope.path + 'createAsset';
    $scope.saveUrlItem = $scope.path + 'createItem';
    $scope.saveUrlParameter = $scope.path + 'createParameter';
    $scope.saveUrlStores = $scope.path + 'createStores';
    $scope.saveUrlBudgetCode = $scope.path + 'createBudgetCode';
   
    

    $scope.CategoryList = [
        {
            'Value': 'Preventive',
            'Text': 'Preventive'
        },
        {
            'Value': 'Routine',
            'Text': 'Routine'
        }
    ];

    $scope.SubCategoryList = [
        {
            'Value': 'Schedule',
            'Text': 'Schedule'
        },
        {
            'Value': 'Overhauling',
            'Text': 'Overhauling'
        }
    ];
    $scope.CriticalLevelLists = [
        {
            'Value': 'Normal',
            'Text': 'Normal'
        },
        {
            'Value': 'Important',
            'Text': 'Important'
        },
        {
            'Value': 'Critical',
            'Text': 'Critical'
        }
    ];
    $scope.ItemTypeList = [
        {
            'Value': 'Electrical',
            'Text': 'Electrical'
        },
        {
            'Value': 'Electronics',
            'Text': 'Electronics'
        },
        {
            'Value': 'Mechanical',
            'Text': 'Mechanical'
        },
        {
            'Value': 'Production',
            'Text': 'Production'
        },
        {
            'Value': 'Other',
            'Text': 'Other'
        }
    ];
    $scope.ItemCategoryList = [
        {
            'Value': 'Consumable',
            'Text': 'Consumable'
        },
        {
            'Value': 'Store & Spares',
            'Text': 'Store & Spares'
        }
    ];
    $scope.CostTypeList = [
        {
            'Value': 'Operation',
            'Text': 'Operation'
        },
        {
            'Value': 'Asset',
            'Text': 'Asset'
        }
    ];
    $scope.EstimationLevelList = [
        {
            'Value': '100%',
            'Text': '100%'
        },
        {
            'Value': '70%',
            'Text': '70%'
        },
        {
            'Value': '50%',
            'Text': '50%'
        },
        {
            'Value': '0%',
            'Text': '0%'
        }
    ];
    $scope.GroupList = [
        {
            'Value': '1',
            'Text': '1'
        },
        {
            'Value': '2',
            'Text': '2'
        },
        {
            'Value': '3',
            'Text': '3'
        },
        {
            'Value': '4',
            'Text': '4'
        }
    ];
    $scope.schedule = {
        Id: null
        , ScheduleCode: null
        , Category: null
        , SubCategory: null
        , StandaredName: null
        , MachineMasterId:null
        , MachineName: null
        , ScheduleDays: null
        , MinScheduleMinutes: null
        , ResponsiblePersoneBgtCodeId: null
        , ResponsiblePersoneBgtCode: null
        , UserName:null
        , Make: null
        , MinScheduleDays: null
        , MaxScheduleMinutes: null
        , Model: null
        , MaxScheduleDays: null
        , StandardScheduleMinutes: null
        , IsActive: true
        , Particulars: null
        , Department: null
        , DepartmentId: null
        , MaintenanceGroup: null
    };
    $scope.scheduleNew = Object.assign({}, $scope.schedule);

    $scope.Machine = {
        Id: null
        , SNO: null
        , MachineRefCode: null
        , WorkCentre: null
        , MCGroup: null
        , Remarks: null
    };

    $scope.Item = {
        Id: null
        , SNO: null
        , ItemName: null
        , CriticalLevel: null
        , IsAuditable: null
        , ByWhomId:null
        , ByWhom:null
        , Remarks: null
        , MaintenanceSchedulingId: null
        , ItemType:null
        , ItemMinutes: null
        , ExceptionDays: null
        , ProductionQty: null
        , ReportApplicable: true
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Parameter = {
        Id: null
        , SNO: null
        , CheckPoints: null
        , Remarks: null
        , ItemId:null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.Stores = {
        Id: null
        , SNO: null
        , ItemName: null
        , UOMId:null
        , UOM: null
        , EstimatedQty: null
        , Category: null
        , ArticleId:null
        , Article: null
        , CostType: null
        , EstimationLevel: null
        , MaintenanceSchedulingId: null
        , Remarks: null
    };
    $scope.StoresNew = Object.assign({}, $scope.Stores);

    $scope.PersonBudget = {
        Id: null
        , SNO: null
        , PersonBudgetCodeId: null
        , PersonBudgetCode: null
        , Group: null
        , MaintenanceSchedulingId: null
    }
    $scope.PersonBudgetNew = Object.assign({}, $scope.PersonBudget);

    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }

    // #region For AutoSequenceNo
    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/MaintenanceScheduling/GetItemAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

    $scope.GeneratStoresSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/MaintenanceScheduling/GetStoresAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.StoresNew.SNO = response.data;
        });
    }
    $scope.GeneratStoresSequenceNo();

    $scope.GeneratPersonBudgetSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'Machines/MaintenanceScheduling/GetPersonBudgetAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.PersonBudgetNew.SNO = response.data;
        });
    }
    $scope.GeneratPersonBudgetSequenceNo();
   
    $scope.refreshTemplateMachineAsset = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllAsset });
    };
    function CheckBoxSelectAllAsset(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridMachine").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ScheduleMachineList.length; i++) {
                $scope.ScheduleMachineList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMachine").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.refreshTemplateMachineGroup = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllMachine });
    };
    function CheckBoxSelectAllMachine(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridMachineGroup").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ScheduleMachineGroupList.length; i++) {
                $scope.ScheduleMachineGroupList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMachineGroup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.MaintenanceMasterList = [];
    $scope.LoadMaintenanceMasterList = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadMaintenanceMasterList'
        }).then(function successCallback(response) {
            $scope.MaintenanceMasterList = response.data;
            var gridObj = $("#GridMaintenanceSchedulingMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadMaintenanceMasterList();
   
    $scope.ScheduleMachineList = [];
    $scope.LoadMachineDetails = function (data,pid) {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadMachineDetails?MachineId=' + data + '&ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.ScheduleMachineList = response.data;
        }
        )
    }

    $scope.ScheduleMachineGroupList = [];
    $scope.LoadMachineGroupDetails = function (pid) {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadMachineGroupDetails?ScheduleId=' + pid
        }).then(function successCallback(response) {
            $scope.ScheduleMachineGroupList = response.data;
        }
        )
    }

    $scope.MachineGroupSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ScheduleMachineGroupList.length; i++) {
                if ($scope.ScheduleMachineGroupList[i].Flag == true) {
                    $scope.ScheduleMachineGroupList[i].MaintenanceSchedulingId = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.ScheduleMachineGroupList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrlMachine,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.LoadMachineGroupDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.MachineSave = function () {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.ScheduleMachineList.length; i++) {
                if ($scope.ScheduleMachineList[i].Flag == true) {
                    $scope.ScheduleMachineList[i].MaintenanceSchedulingId = $scope.scheduleNew.Id;
                    $scope.SaveList.push($scope.ScheduleMachineList[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrlAsset,
                data: {
                    "DataList": $scope.SaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    //$scope.LoadMachineDetails($scope.scheduleNew.MachineMasterId, $scope.scheduleNew.Id);
                    $scope.LoadMachineDetails($scope.scheduleNew.Id);
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.ScheduleItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadItemDetails?ScheduleId='+$scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleItemList = response.data;
        }
        )
    }

    $scope.ScheduleStoresList = [];
    $scope.LoadStoresDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadStoresDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleStoresList = response.data;
        }
        )
    }

    $scope.ScheduleBudgetCodeList = [];
    $scope.LoadBudgetCodeDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadBudgetCodeDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleBudgetCodeList = response.data;
        }
        )
    }

    $scope.selectMachine = function () {
        $scope.getsM();
        angular.element(document.querySelector('#MachinePop')).modal('show');
    }

    $scope.MachineList = [];
    $scope.getsM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMachine',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MachineList = resp.data;
        });
    }

    $scope.doubleMachine = function (e) {
        $scope.scheduleNew.MachineMasterId = e.data.MachineMasterId;
        $scope.scheduleNew.MachineName = e.data.MachineMaster;
        $scope.scheduleNew.Make = e.data.Make;
        $scope.scheduleNew.Model = e.data.Model;
        $scope.scheduleNew.Particulars = e.data.Particulars;
        angular.element(document.querySelector('#MachinePop')).modal('hide');
    }

    $scope.closeMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePop')).modal('hide');
    }

    $scope.selectBudgetCode = function () {
        $scope.getBudgetCode();
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('show');
    }

    $scope.BudgetCodeList = [];
    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.BudgetCodeList = resp.data;
        });
    }

    $scope.doubleBudgetCode = function (e) {
        $scope.scheduleNew.ResponsiblePersoneBgtCodeId = e.data.ManPowerBudgetId;
        $scope.scheduleNew.ResponsiblePersoneBgtCode = e.data.Code;
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.selectEmployee = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ByWhomPop')).modal('show');
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
        $scope.ItemNew.ByWhomId = e.data.SystemId;
        $scope.ItemNew.ByWhom = e.data.EmployeeName;
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.closeByWhomPopUp = function () {
        angular.element(document.querySelector('#ByWhomPop')).modal('hide');
    }

    $scope.selectUOM = function () {
        $scope.getUOM();
        angular.element(document.querySelector('#UOMPopUp')).modal('show');
    }

    $scope.UOMList = [];
    $scope.getUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUOM',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UOMList = resp.data;
        });
    }

    $scope.doubleUOM = function (e) {
        $scope.StoresNew.UOMId = e.data.UOMId;
        $scope.StoresNew.UOM = e.data.UOM;
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }


    $scope.selectArticle = function () {
        $scope.getArticle();
        angular.element(document.querySelector('#ArticlePopUp')).modal('show');
    }

    $scope.ArticleList = [];
    $scope.getArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetArticle',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ArticleList = resp.data;
        });
    }

    $scope.doubleArticle = function (e) {
        $scope.StoresNew.ArticleId = e.data.ArticleId;
        $scope.StoresNew.Article = e.data.ArticleName;
        angular.element(document.querySelector('#ArticlePopUp')).modal('hide');
    }

    $scope.closeArticlePopUp = function () {
        angular.element(document.querySelector('#ArticlePopUp')).modal('hide');
    }

    $scope.selectPersonBudgetCode = function () {
        $scope.getPersonBudgetCode();
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('show');
    }

    $scope.PersonBudgetCodeList = [];
    $scope.getPersonBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetPersonBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.PersonBudgetCodeList = resp.data;
        });
    }

    $scope.doublePersonBudgetCode = function (e) {
        $scope.PersonBudgetNew.PersonBudgetCodeId = e.data.ManPowerBudgetId;
        $scope.PersonBudgetNew.PersonBudgetCode = e.data.Code;
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('hide');
    }

    $scope.closePersonBudgetCodePopUp = function () {
        angular.element(document.querySelector('#PersonBudgetCodePopUp')).modal('hide');
    }

    $scope.selectDepartment = function () {
        $scope.getDepartment();
        angular.element(document.querySelector('#DepartmentPopUp')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getDepartment = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetDepartment',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DepartmentList = resp.data;
        });
    }

    $scope.doubleDepartment = function (e) {
        $scope.scheduleNew.DepartmentId = e.data.DepartmentId;
        $scope.scheduleNew.Department = e.data.Department;
        angular.element(document.querySelector('#DepartmentPopUp')).modal('hide');
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPopUp')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MaintenanceScheduleForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ScheduleData': $scope.scheduleNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadMaintenanceMasterList();
                    ScheduleClearFields();
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MaintenanceScheduleItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew,
                    'Pid':$scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails($scope.scheduleNew.Id);
                    ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.StoresSave = function () {
        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope.MaintenanceScheduleStoresForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlStores,
                data: {
                    'StoresData': $scope.StoresNew,
                    'Pid': $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadStoresDetails($scope.scheduleNew.Id);
                    StoresClearFields($scope.GeneratStoresSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
       /* }*/
    };

    $scope.BudgetCodeSave = function () {
        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope.MaintenanceScheduleBudgetForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlBudgetCode,
                data: {
                    'BudgetCodeData': $scope.PersonBudgetNew,
                    'Pid': $scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadBudgetCodeDetails($scope.scheduleNew.Id);
                    BudgetCodeClearFields($scope.GeneratPersonBudgetSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        /*}*/
    };

    $scope.ParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.ItemNew.Id;
        $scope.ItemNew.Id = ItemId;
        try {
            $http.get('Machines/MaintenanceScheduling/getParameterData?ItemId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.getParameter = function (data) {
        try {
            $http.get('Machines/MaintenanceScheduling/getParameterData?ItemId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.GetParameterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadParameterEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.Parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.SaveParameterData = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrlParameter,
                data: {
                    'ParameterData': $scope.ParameterNew,
                    'Pid': $scope.ItemNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getParameter($scope.ItemNew.Id);
                    ParameterClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };


    $scope.tab = 5;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.GetDetails = function (args) {
        $scope.ScheduleMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadScheduleEditData?ScheduleID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.scheduleNew = response.data.schedule[0];
            $scope.scheduleNew.MachineName = response.data.schedule[0].MachineName;
            $scope.scheduleNew.MachineMasterId = response.data.schedule[0].MachineMasterId;
            $scope.scheduleNew.ResponsiblePersoneBgtCode = response.data.schedule[0].ResponsiblePersoneBgtCode;
            $scope.LoadMachineGroupDetails($scope.ScheduleMasterId);
            $scope.LoadMachineDetails($scope.scheduleNew.MachineMasterId, $scope.ScheduleMasterId);
            $scope.LoadItemDetails($scope.ScheduleMasterId);
            $scope.LoadStoresDetails($scope.ScheduleMasterId);
            $scope.LoadBudgetCodeDetails($scope.ScheduleMasterId);
            $scope.GeneratItemSequenceNo($scope.ScheduleMasterId);
            $scope.GeneratStoresSequenceNo($scope.ScheduleMasterId);
            $scope.GeneratPersonBudgetSequenceNo($scope.ScheduleMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.GetStoresDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadStoresEditData?StoresId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.StoresNew = response.data.Stores[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.GetBudgetCodeDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/MaintenanceScheduling/LoadBudgetCodeEditData?BudgetCodeId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.PersonBudgetNew = response.data.PersonBudget[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.Clear = function () {
        ScheduleClearFields();
    };
    $scope.ItemClear = function () {
        ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
    };
    $scope.SaveParameterClear = function () {
        ParameterClearFields();
    };
    $scope.StoresClear = function () {
        StoresClearFields($scope.GeneratStoresSequenceNo($scope.scheduleNew.Id));
    };
    $scope.BudgetCodeClear = function () {
        BudgetCodeClearFields($scope.GeneratPersonBudgetSequenceNo($scope.scheduleNew.Id));
    };
    
    function ScheduleClearFields() {
        $scope.Action = "Save";
        $scope.scheduleNew = Object.assign({}, $scope.schedule);
        $scope.ScheduleMachineList = [];
    }

    function ItemClearFields(seq) {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.Item);
        $scope.ItemNew.SNO = seq;
    }

    function StoresClearFields(seq) {
        $scope.Action = "Save";
        $scope.StoresNew = Object.assign({}, $scope.Stores);
        $scope.StoresNew.SNO = seq;
    }

    function BudgetCodeClearFields(seq) {
        $scope.Action = "Save";
        $scope.PersonBudgetNew = Object.assign({}, $scope.PersonBudget);
        $scope.PersonBudgetNew.SNO = seq;
    }

    function ParameterClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }

    $scope.removeRowModal = function (index,data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveItem')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeStoresModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempStoresId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveStores')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeBudgetCodeModal = function (index, data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempbudgetId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveBudgetCode')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/MaintenanceScheduling/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.scheduleNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeStoresRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/MaintenanceScheduling/StoresDelete?id=' + $scope.tempStoresId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadStoresDetails($scope.scheduleNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.removeBudgetCodeRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/MaintenanceScheduling/BudgetCodeDelete?id=' + $scope.tempbudgetId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadBudgetCodeDetails($scope.scheduleNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/MaintenanceScheduling/ScheduleDelete?id=' + $scope.scheduleNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadMaintenanceMasterList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
}