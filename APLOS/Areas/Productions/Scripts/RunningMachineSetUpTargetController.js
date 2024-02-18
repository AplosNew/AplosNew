
'use strict';
RunningMachineSetUpTargetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RunningMachineSetUpTargetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Daily Target";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/RunningMachineSetUpTarget/';
    $scope.Copy = $scope.path + 'CopyFromTable';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveSingleRowUrl = $scope.path + 'createSingleRow';
    $scope.UpdateSingleRowUrl = $scope.path + 'UpdateSingleRow';
    $scope.saveUrlItem = $scope.path + 'createItem';
    $scope.saveUrlItemValue = $scope.path + 'createItemValue';
    $scope.saveUrlReasonValue = $scope.path + 'createReasonValue';
    $scope.saveUrlDetentionWC = $scope.path + 'createDetentionWC';
    $scope.saveUrlParameter = $scope.path + 'createParameter';
    $scope.saveUrlParameterValue = $scope.path + 'createParameterValue';
    $scope.saveUrlSinglePValue = $scope.path + 'createSinglePValue';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.WithEmployee = false;
    $scope.WithMachine = false;

    $scope.DateValidation = function (ProductionDate) {
        try {
            if (new Date(ProductionDate) > new Date()) {
                throw "Target Date must be below or equal to current Date!";
            }

        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };

    $scope.ParameterDateValidation = function (ParameterDate) {
        try {
            var date = new Date();
            date.setDate(date.getDate() - 1);
            $scope.YDate = $filter('dateFiltering')(date);

            if (ParameterDate < $scope.YDate) {
                throw "Parameter date should be today's or yestarday's date only.";
            }
            if (new Date(ParameterDate) > new Date()) {
                throw "Parameter Date must be below or equal to current Date!";
            }

        }
        catch (ex) {
            ShowResult(ex, 'failure');
        }
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $http.get('Productions/Productionsummary/GetShiftList?processId=' + $scope.DailyProductionTargetNew.ProcessId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.DailyProductionTargetNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

    $scope.ProcessItemList = [];
    $scope.GetProcessItemList = function () {
        $http({
            method: 'GET',
            url: 'Productions/RunningMachineSetUpTarget/GetProcessItemList'
        }).then(function successCallback(response) {
            $scope.ProcessItemList = response.data;
        });
    }
    $scope.GetProcessItemList();

    $scope.ProcessParameterList = [];
    $scope.GetProcessParameterList = function () {
        $http({
            method: 'GET',
            url: 'Productions/RunningMachineSetUpTarget/GetProcessParameterList'
        }).then(function successCallback(response) {
            $scope.ProcessParameterList = response.data;
        });
    }
    $scope.GetProcessParameterList();

    $scope.ItemList = [];
    $scope.LoadItemDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadItemDetails'
        }).then(function successCallback(response) {
            $scope.ItemList = response.data;
        }
        )
    }
    $scope.LoadItemDetails();

    $scope.ParameterList = [];
    $scope.LoadParameterDetails = function () {
        $http({
            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadParameterDetails'
        }).then(function successCallback(response) {
            $scope.ParameterList = response.data;
        }
        )
    }
    $scope.LoadParameterDetails();

    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadItemDetailsEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.GetParameterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadParameterDetailsEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }

    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ItemDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails();
                    ItemClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ParameterSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ParameterDetailsForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlParameter,
                data: {
                    'ParameterData': $scope.ParameterNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadParameterDetails();
                    ParameterClearFields();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.Item = {
        Id: null,
        ProcessId: null,
        ItemName: null,
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Parameter = {
        Id: null,
        ProcessId: null,
        ParameterName: null,
    };
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.ItemClear = function () {
        ItemClearFields();
    };

    function ItemClearFields() {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.Item);
    }

    $scope.ParameterClear = function () {
        ParameterClearFields();
    };

    function ParameterClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }

    $scope.DailyProductionTarget = {
        Id: null,
        DailyProductionTargetID: null,
        Line: null,
        PRNo: null,
        MaterialMasterArticleId: null,
        MaterialMasterId: null,
        Manpower: null,
        SMV: null,
        TotalHour: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        ProductionShiftId: null,
        TargetDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        HeaderResponsiblePerson: null,
        HeaderIncharge: null,
        HeaderResponsiblePersonId: null,
        HeaderInchargeId: null,
        HeaderPlanHour: 0,
        HeaderEfficiency: 0,
        PlanHours: null,
        Efficiency: null,
        SalesOrderId: null,
        DetentionSum: 0

    };
    $scope.DailyProductionTargetNew = Object.assign({}, $scope.DailyProductionTarget);

    $scope.ProcessDetention = {
        Id: null,
        RMSTargetId: null,
        EntityId: null,
        ProcessId: null,
        ShiftId: null,
        Date: null,
        WorkCenterMasterId: null,
        DepartmentId: null,
        DetentionId: null,
        ResponsiblePersonId: null,
        WorkCenter: null,
        Detention: null,
        DetentionType: null,
        DetentionTypeId: null,
        Department: null,
        ResponsiblePerson: null,
        Remark: null,
        Minute: null,
    };

    // Refreshing the serials
    function refreshSerial() {
        for (var j = 0; j < $scope.DailyTargetList.length; j++) {
            $scope.DailyTargetList[j].Serial = j;
        }
    }
    // Add Tiles
    $scope.AddTile = function (e) {
        console.log(e);
        let ob = {};
        Object.assign(ob, e);
        ob.Active = 0;
        ob.Id = null;
        ob.Article = null;
        ob.WorkCenterMasterId = e.WorkCenterMasterId;
        ob.ProductionOrderId = null;
        ob.LotNumber = null;
        ob.PlanHours = null;
        ob.Efficiency = null;
        ob.TargetFD = null;
        ob.TargetProductionFP = null;
        ob.Remarks = null;
        ob.ResponsiblePersonId = e.ResponsiblePersonId;
        ob.InChargeId = e.InChargeId;
        $scope.DailyTargetList.splice(e.Serial + 1, 0, ob);
        refreshSerial();
    }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.DailyProductionTargetNew.EntityId = $scope.entityList[0].Value;
                $scope.loadProcessList($scope.DailyProductionTargetNew.EntityId);
            }
        });
    };
    $scope.getAllEntities();

    $scope.processList = [];
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.DailyProductionTargetNew.ProcessId = $scope.processList[0].Value;

            }
        });
    };

    $scope.selectResponsiblePerson = function () {
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
        $scope.DailyProductionTargetNew.HeaderResponsiblePersonId = e.data.SystemId;
        $scope.DailyProductionTargetNew.HeaderResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectIncharge = function () {
        $scope.getIncharge();
        angular.element(document.querySelector('#InchargePopup')).modal('show');
    }



    $scope.InchargeList = [];
    $scope.getIncharge = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.InchargeList = resp.data;
        });
    }

    $scope.doubleIncharge = function (e) {
        $scope.DailyProductionTargetNew.HeaderInchargeId = e.data.SystemId;
        $scope.DailyProductionTargetNew.HeaderIncharge = e.data.EmployeeName;
        angular.element(document.querySelector('#InchargePopup')).modal('hide');
    }

    $scope.closeInchargePopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.selectGridResponsible = function (data) {
        $scope.Newobject = data.data;
        $scope.getsR();
        angular.element(document.querySelector('#ResponsiblePopup')).modal('show');
    }

    $scope.ResponsibleList = [];
    $scope.getsR = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;
        });
    }

    $scope.doubleResponsible = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
        $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.closeResponsiblePopup = function () {
        angular.element(document.querySelector('#ResponsiblePopup')).modal('hide');
    }

    $scope.selectGridIncharge = function (data) {
        $scope.Newobject = data.data;
        $scope.getsI();
        angular.element(document.querySelector('#InchargeGridPopup')).modal('show');
    }

    $scope.InchargeGridList = [];
    $scope.getsI = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.InchargeGridList = resp.data;
        });
    }

    $scope.doubleInchargeGrid = function (e) {
        $scope.Newobject.InChargeId = e.data.SystemId;
        $scope.Newobject.InCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');
    }

    $scope.closeInchargeGridPopup = function () {
        angular.element(document.querySelector('#InchargeGridPopup')).modal('hide');

    }

    $scope.RMSTargetId = null;
    $scope.RMSTargetItemList = [];
    $scope.getItemValuePopup = function (Id) {
        //$scope.NewObject = data.data;
        //$scope.RMSTargetId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadProcessItemList?ProcessId=' + $scope.DailyProductionTargetNew.ProcessId + '&RMSTargetId=' + Id
        }).then(function successCallback(response) {
            $scope.RMSTargetItemList = response.data;
            var gridObj = $("#GridItemValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ItemValuePopup')).modal('show');
        }
        )
    }

    $scope.closeItemValuePopup = function () {
        angular.element(document.querySelector('#ItemValuePopup')).modal('hide');
    }

    $scope.ProductionId = null;
    $scope.ProductionReasonList = [];
    $scope.getReasonValuePopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProductionId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadProcessReasonList?ProcessId=' + data.data.ProcessId + '&ProductionId=' + $scope.ProductionId
        }).then(function successCallback(response) {
            $scope.ProductionReasonList = response.data;
            var gridObj = $("#GridReasonValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ReasonValuePopup')).modal('show');
        }
        )
    }

    $scope.closeReasonValuePopup = function () {
        angular.element(document.querySelector('#ReasonValuePopup')).modal('hide');
    }

    $scope.ProductionParameterList = [];
    $scope.getParameterValuePopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ProductionId = $scope.NewObject.Id;
        $http({

            method: 'Get',
            url: 'Productions/RunningMachineSetUpTarget/LoadProcessParameterList?ProcessId=' + data.data.ProcessId + '&ProductionId=' + $scope.ProductionId
        }).then(function successCallback(response) {
            $scope.ProductionParameterList = response.data;
            var gridObj = $("#GridParameterValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterValuePopup')).modal('show');
        }
        )
    }

    $scope.closeParameterValuePopup = function () {
        angular.element(document.querySelector('#ParameterValuePopup')).modal('hide');
    }

    $scope.SaveReasonValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProductionReasonList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.ProductionReasonList[i].ReasonValue)) {
                    $scope.ProductionReasonList[i].ProductionId = $scope.ProductionId;
                    $scope.SaveList.push($scope.ProductionReasonList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlReasonValue,
                data: { 'ProductionReasonData': $scope.SaveList },
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

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.SaveParameterValue = function () {
        try {
            $scope.ParameterSaveList = [];
            for (var i = 0; i < $scope.ProductionParameterList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.ProductionParameterList[i].ParameterValue)) {
                    $scope.ProductionParameterList[i].ProductionId = $scope.ProductionId;
                    $scope.ParameterSaveList.push($scope.ProductionParameterList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlParameterValue,
                data: { 'ProductionParameterData': $scope.ParameterSaveList },
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

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.SaveSinglePValue = function (data) {
        try {
            if (baseService.isUndefinedOrNull(data.data.ParameterValue)) {
                throw "Please enter Parameter Value and proceed";
            }
            data.data.ProductionId = $scope.ProductionId;
            $http({
                method: 'POST',
                url: $scope.saveUrlSinglePValue,
                data: { 'ProductionParameterData': data.data },
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

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };


    //$scope.refreshTemplateItemValue = function (args) {
    //    $("#IVheadchk").ejCheckBox({ "change": CheckBoxSelectAllItemValue });
    //};
    //function CheckBoxSelectAllItemValue(e) {
    //    var ChkOrUnchk = false;
    //    if (e.model.checkState === "check") {
    //        ChkOrUnchk = true;
    //    }

    //    var filtered = $("#GridItemValuePopup").data("ejGrid").getFilteredRecords();
    //    if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
    //        for (var i = 0; i < $scope.RMSTargetItemList.length; i++) {
    //            $scope.RMSTargetItemList[i].IsActive = ChkOrUnchk;
    //        }
    //    }
    //    else {
    //        for (var j = 0; j < filtered.length; j++) {
    //            filtered[j].IsActive = ChkOrUnchk;
    //        }
    //    }
    //    var gridObj = $("#GridItemValuePopup").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //};


    $scope.listFromProcessOrSFGInventory = [];
    $scope.GetSFGMovementFromCbo = function (entity) {
        $http({
            method: 'GET',
            url: 'Productions/RunningMachineSetUpTarget/GetProcessFromCbo?entity=' + entity,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //sccuess 
                $scope.listFromProcessOrSFGInventory = response.data;

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.changeProcess = function () {
        $scope.Process = $("#Process option:selected").text();
        $scope.Status = null;
        $scope.Status = $.grep($scope.listFromProcessOrSFGInventory, function (item) {
            return item.ProcessId === $scope.DailyProductionTargetNew.ProcessId;
        })[0].Status;

        for (var i = 0; i < $scope.listFromProcessOrSFGInventory.length; i++) {
            if ($scope.DailyProductionTargetNew.ProcessId === $scope.listFromProcessOrSFGInventory[i].ProcessId) {
                $scope.DailyProductionTargetNew.ProductionBookingLevel = $scope.listFromProcessOrSFGInventory[i].ProductionBookingLevel;
                $scope.LotNumberCapture = $scope.listFromProcessOrSFGInventory[i].LotNumberCapture;
                $scope.LotNumberMandatory = $scope.listFromProcessOrSFGInventory[i].LotNumberMandatory;
                $scope.IsFirst = $scope.listFromProcessOrSFGInventory[i].IsFirst;
                $scope.Status = $scope.listFromProcessOrSFGInventory[i].Status;
                $scope.Sequence = $scope.listFromProcessOrSFGInventory[i].Sequence - 1;
                break;
            }
        }
    };

    $scope.DailyTargetList = [];
    $scope.getDailytarget = function () {

        try {
            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.EntityId))
                throw 'Please select entity';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.ProcessId))
                throw 'Please select process';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.TargetDate))
                throw 'Please select target date';

            if ($scope.DailyProductionTargetNew.HeaderPlanHour > 24)
                throw 'Enter Hours should not be greater than 24';

            if ($scope.DailyProductionTargetNew.HeaderEfficiency > 100)
                throw 'Enter Efficiency should not be greater than 100';

            $http({

                method: 'GET',
                url: 'Productions/RunningMachineSetUpTarget/GetDailyTarget?EntityId=' + $scope.DailyProductionTargetNew.EntityId + '&ProcessId=' + $scope.DailyProductionTargetNew.ProcessId + '&TargetDate=' + $scope.DailyProductionTargetNew.TargetDate + '&ProductionShiftId=' + $scope.DailyProductionTargetNew.ProductionShiftId + '&HeaderResponsiblePersonId=' + $scope.DailyProductionTargetNew.HeaderResponsiblePersonId + '&HeaderInchargeId=' + $scope.DailyProductionTargetNew.HeaderInchargeId + '&HeaderPlanHour=' + $scope.DailyProductionTargetNew.HeaderPlanHour + '&HeaderEfficiency=' + $scope.DailyProductionTargetNew.HeaderEfficiency,
            }).then(function successCallback(response) {
                $scope.DailyTargetList = response.data;
                for (var i = 0; i < $scope.DailyTargetList.length; i++) {
                    Object.assign($scope.DailyTargetList[i], { 'Serial': parseInt(i) });
                }
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }


    $scope.DailyTargetAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Active = ChkOrUnchk;
        }

        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
    };

    //$scope.Save = function () {
    //    try {
    //     /*   $scope.$broadcast('show-errors-check-validity');*/
    //        $scope.SaveList = [];
    //        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
    //            if ($scope.DailyTargetList[i].Active == true) {
    //                if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].ProductionOrderId) == true) {
    //                    throw "Please select Production Order No. for '" + $scope.DailyTargetList[i].Line + "'";
    //                }
    //                if ($scope.DailyTargetList[i].Efficiency > 100) {
    //                    throw "Efficiency should not be greater than 100";
    //                }
    //                $scope.DailyTargetList[i].EntityId = $scope.DailyProductionTargetNew.EntityId;
    //                $scope.DailyTargetList[i].ProcessId = $scope.DailyProductionTargetNew.ProcessId;
    //                $scope.DailyTargetList[i].TargetDate = $scope.DailyProductionTargetNew.TargetDate;
    //                $scope.DailyTargetList[i].ProductionShiftId = $scope.DailyProductionTargetNew.ProductionShiftId;
    //                $scope.SaveList.push($scope.DailyTargetList[i]);
    //            }
    //        }
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrl,
    //            data: { 'DailyTargetData': $scope.SaveList, 'TargetDate': $scope.DailyProductionTargetNew.TargetDate, 'EntityId': $scope.DailyProductionTargetNew.EntityId, 'ProcessId': $scope.DailyProductionTargetNew.ProcessId },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.getDailytarget();
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    } catch (e) {
    //        ShowResult(e, 'failure');

    //    }
    //};

    $scope.RMSId = null;
    $scope.SaveSingleRow = function (data) {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if (baseService.isUndefinedOrNull(data.data.PlanHours)) {
                throw "Please enter plan hours and proceed";
            }
            if (baseService.isUndefinedOrNull(data.data.Efficiency)) {
                throw "Please enter Efficiency and proceed";
            }
            if (parseInt(data.data.PlanHours) < 1) {
                throw "PlanHours should not be less than 1";
            }
            if (parseInt(data.data.Efficiency) < 10 || parseInt(data.data.Efficiency) > 100) {
                throw "Efficiency should not be less than 10 and greater than 100";
            }
            //$scope.SaveList = [];
            //for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            if (baseService.isUndefinedOrNull(data.data.Id) == true) {
                if (baseService.isUndefinedOrNull(data.data.ProductionOrderId) == true) {
                    throw "Please select Production Order No. for '" + data.data.Line + "'";
                }
                data.data.EntityId = $scope.DailyProductionTargetNew.EntityId;
                data.data.ProcessId = $scope.DailyProductionTargetNew.ProcessId;
                data.data.TargetDate = $scope.DailyProductionTargetNew.TargetDate;
                data.data.ProductionShiftId = $scope.DailyProductionTargetNew.ProductionShiftId;
                //    $scope.SaveList.push($scope.DailyTargetList[i]);
                //}
            }
            $http({
                method: 'POST',
                url: $scope.saveSingleRowUrl,
                data: { 'DailyTargetData': data.data, 'TargetDate': $scope.DailyProductionTargetNew.TargetDate, 'EntityId': $scope.DailyProductionTargetNew.EntityId, 'ProcessId': $scope.DailyProductionTargetNew.ProcessId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //ShowResult(response.data.Message, 'success');
                    $scope.RMSId = response.data.Id;
                    $scope.getItemValuePopup(response.data.Id);
                    $scope.getDailytarget();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.UpdateSingleRow = function (data) {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if (baseService.isUndefinedOrNull(data.data.PlanHours)) {
                throw "Please enter plan hours and proceed";
            }
            if (baseService.isUndefinedOrNull(data.data.Efficiency)) {
                throw "Please enter Efficiency and proceed";
            }
            if (parseInt(data.data.Efficiency) > 100) {
                throw "Efficiency should not be greater than 100";
            }

            $scope.UpdateList = [];
            for (var i = 0; i < $scope.DailyTargetList.length; i++) {
                if ($scope.DailyTargetList[i].TargetFD > 0 && parseFloat(data.data.TargetFD) > 0 && $scope.DailyTargetList[i].Id == data.data.Id) {
                    if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].ProductionOrderId) == true) {
                        throw "Please select Production Order No. for '" + $scope.DailyTargetList[i].Line + "'";
                    }
                    $scope.DailyTargetList[i].EntityId = $scope.DailyProductionTargetNew.EntityId;
                    $scope.DailyTargetList[i].ProcessId = $scope.DailyProductionTargetNew.ProcessId;
                    $scope.DailyTargetList[i].TargetDate = $scope.DailyProductionTargetNew.TargetDate;
                    $scope.DailyTargetList[i].ProductionShiftId = $scope.DailyProductionTargetNew.ProductionShiftId;
                    $scope.UpdateList.push($scope.DailyTargetList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.UpdateSingleRowUrl,
                data: { 'DailyTargetData': $scope.UpdateList, 'TargetDate': $scope.DailyProductionTargetNew.TargetDate, 'EntityId': $scope.DailyProductionTargetNew.EntityId, 'ProcessId': $scope.DailyProductionTargetNew.ProcessId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.RMSId = response.data.Id;
                    $scope.getItemValuePopup(response.data.Id);
                    /*$scope.getDailytarget();*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.SaveItemValue = function () {
        try {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.RMSTargetItemList.length; i++) {
                if (!baseService.isUndefinedOrNull($scope.RMSTargetItemList[i].ItemValue)) {
                    $scope.RMSTargetItemList[i].RMSTargetId = $scope.RMSId;
                    $scope.SaveList.push($scope.RMSTargetItemList[i]);
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrlItemValue,
                data: { 'RMSTargetItemData': $scope.SaveList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //$scope.getDailytarget();
                }
                angular.element(document.querySelector('#ItemValuePopup')).modal('hide');
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };


    $scope.RMSTargetId = null;
    $scope.DailyProductionTargetNew.DetentionSum = 0;
    $scope.ProcessDetentionLists = [];
    $scope.getProcessDetentionPopupPoPUp = function (data) {
        $scope.NewObject = data.data;

        var processid = $scope.DailyProductionTargetNew.ProcessId;
        var entityid = $scope.DailyProductionTargetNew.EntityId;
        var targetdate = $scope.DailyProductionTargetNew.TargetDate;
        var shiftid = $scope.DailyProductionTargetNew.ProductionShiftId;
        $scope.DailyProductionTargetNew = data.data;
        $scope.RMSTargetId = $scope.DailyProductionTargetNew.Id;
        $scope.DailyProductionTargetNew.ProcessId = processid;
        $scope.DailyProductionTargetNew.EntityId = entityid;
        $scope.DailyProductionTargetNew.TargetDate = targetdate;
        $scope.DailyProductionTargetNew.ProductionShiftId = shiftid;
        $scope.DailyProductionTargetNew.workCenter = data.data.Line;
        $scope.DailyProductionTargetNew.workCenterId = data.data.WorkCenterMasterId;
        try {
            //ValidationMaster();
            $scope.ProcessDetentionLists = [];
            for (var i = 1; i < 6; i++) {
                var obj = angular.copy($scope.ProcessDetention);
                obj.Id = null;
                obj.ProcessId = $scope.DailyProductionTargetNew.ProcessId;
                obj.EntityId = $scope.DailyProductionTargetNew.EntityId;
                obj.TargetDate = $scope.DailyProductionTargetNew.TargetDate;
                obj.ProductionShiftId = $scope.DailyProductionTargetNew.ProductionShiftId;
                obj.workCenter = $scope.DailyProductionTargetNew.workCenterId;
                obj.RMSTargetId = $scope.RMSTargetId;
                obj.Sequence = i;
                $scope.ProcessDetentionLists.push(obj);
            }

            $http.get('Productions/RunningMachineSetUpTarget/GetProcessDetentionData?processId=' + $scope.DailyProductionTargetNew.ProcessId + '&entityId=' + $scope.DailyProductionTargetNew.EntityId + '&productionDate=' + $scope.DailyProductionTargetNew.TargetDate + '&shiftId=' + $scope.DailyProductionTargetNew.ProductionShiftId + '&workcenter=' + data.data.WorkCenterMasterId + '&RMSTargetId=' + data.data.Id)
                .then(
                    function successCallback(response) {
                        if (response.data.length > 0) {

                            for (var j = 0; j < response.data.length; j++) {
                                for (var k = 0; k < $scope.ProcessDetentionLists.length; k++) {
                                    if ($scope.ProcessDetentionLists[k].Sequence == response.data[j].Sequence) {
                                        $scope.ProcessDetentionLists[k].Flag = response.data[j].Flag;
                                        $scope.ProcessDetentionLists[k].Id = response.data[j].Id;
                                        $scope.ProcessDetentionLists[k].workCenter = response.data[j].WorkCenter;
                                        $scope.ProcessDetentionLists[k].RMSTargetId = response.data[j].RMSTargetId;
                                        $scope.ProcessDetentionLists[k].DepartmentId = response.data[j].DepartmentId;
                                        $scope.ProcessDetentionLists[k].DepartmentName = response.data[j].DepartmentName;
                                        $scope.ProcessDetentionLists[k].DetentionTypeList = response.data[j].DetentionTypeList;
                                        $scope.ProcessDetentionLists[k].DetentionList = response.data[j].DetentionList;
                                        $scope.ProcessDetentionLists[k].DetentionId = response.data[j].DetentionId;
                                        $scope.ProcessDetentionLists[k].DetentionTypeId = response.data[j].DetentionTypeId;
                                        $scope.ProcessDetentionLists[k].Detention = response.data[j].Detention;
                                        $scope.ProcessDetentionLists[k].Minute = response.data[j].Minute;
                                        $scope.ProcessDetentionLists[k].ResponsiblePersonId = response.data[j].ResponsiblePersonId;
                                        $scope.ProcessDetentionLists[k].ResponsiblePerson = response.data[j].ResponsiblePerson;
                                        $scope.ProcessDetentionLists[k].Remark = response.data[j].Remark;
                                    }

                                }
                            }

                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#articlePoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    var currRow = null;
    $scope.DetentionList = [];
    $scope.GetDetentionList = function (data) {
        var gridObj = $("#ProductionSummaryDetentionWC").ejGrid("instance");
        currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/GetDetentionListWC?DetentiontypeId=' + currRow.DetentionTypeId
        }).then(function successCallback(response) {
            currRow.DetentionList = response.data;
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();

        });
    };

    $scope.selectDepartment = function (data) {
        $scope.Newobject = data.data;
        $scope.getsD();
        $scope.NewObject.DetentionId = null;
        $scope.NewObject.DetentionList = null;
        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        angular.element(document.querySelector('#DepartmentPop')).modal('show');
    }

    $scope.DepartmentList = [];
    $scope.getsD = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterTransaction/GetDetentionDepartment',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DepartmentList = resp.data;
        });
    }

    $scope.doubleDepartment = function (e) {
        $scope.Newobject.DepartmentId = e.data.DepartmentId;
        $scope.Newobject.DepartmentName = e.data.DepartmentName;
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
        $scope.getDetentionTypeListByDepartment($scope.Newobject.DepartmentId);
        $scope.getDetentionListByDepartment($scope.Newobject.DepartmentId);
    }

    $scope.closeDepartmentPopUp = function () {
        angular.element(document.querySelector('#DepartmentPop')).modal('hide');
    }

    $scope.getDetentionTypeListByDepartment = function (departmentid) {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/getDetentionTypeListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            //$scope.DetentionList = null;
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                if ($scope.ProcessDetentionLists[i].DetentionId == null) {
                    $scope.ProcessDetentionLists[i].DetentionTypeList = response.data;
                }
            }
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }
    $scope.getDetentionListByDepartment = function (departmentid) {
        $scope.Newobject.DetentionList = null;
        $http({
            method: 'GET',
            url: 'IE/MachineMasterTransaction/getDetentionListByDepartment?departmentid=' + departmentid
        }).then(function successCallback(response) {
            //$scope.DetentionList = null;
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                if ($scope.ProcessDetentionLists[i].DetentionId == null) {
                    $scope.ProcessDetentionLists[i].DetentionList = response.data;
                }
            }
            var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        });
    }

    $scope.selectDetResponsible = function (data) {
        $scope.Newobject = data.data;
        $scope.Newobject.DetentionId = data.data.DetentionId;
        $scope.getsDetR();
        angular.element(document.querySelector('#DetResponiblePersonPopup')).modal('show');
    }

    $scope.DetResponsibleList = [];
    $scope.getsDetR = function () {
        $http({
            method: 'POST',
            url: 'IE/MachineMasterTransaction/GetDetentionResponsible?detentionId=' + $scope.Newobject.DetentionId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DetResponsibleList = resp.data;
        });
    }

    $scope.doubleDetResponsible = function (e) {
        $scope.Newobject.ResponsiblePersonId = e.data.ResponsiblePersonId;
        $scope.Newobject.ResponsiblePerson = e.data.ResponsiblePerson;
        angular.element(document.querySelector('#DetResponiblePersonPopup')).modal('hide');
    }

    $scope.closeDetResponsiblePopUp = function () {
        angular.element(document.querySelector('#DetResponiblePersonPopup')).modal('hide');
    }

    $scope.SaveDetentionWC = function () {
        try {

            $scope.DetentionSaveList = [];
            for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                //if ($scope.ProcessDetentionLists[i].Flag == true)
                if (!baseService.isUndefinedOrNull($scope.ProcessDetentionLists[i].Minute)) {
                    $scope.ProcessDetentionLists[i].RMSTargetId = $scope.DailyProductionTargetNew.Id;
                    $scope.ProcessDetentionLists[i].EntityId = $scope.DailyProductionTargetNew.EntityId;
                    $scope.ProcessDetentionLists[i].ProcessId = $scope.DailyProductionTargetNew.ProcessId;
                    $scope.ProcessDetentionLists[i].Date = $scope.DailyProductionTargetNew.TargetDate;
                    $scope.ProcessDetentionLists[i].shiftid = $scope.DailyProductionTargetNew.ProductionShiftId;
                    $scope.ProcessDetentionLists[i].WorkCenterId = $scope.DailyProductionTargetNew.WorkCenterMasterId;
                    $scope.DetentionSaveList.push($scope.ProcessDetentionLists[i]);
                }
            }


            $http({
                method: 'POST',
                url: $scope.saveUrlDetentionWC,
                data: {
                    "DataList": $scope.DetentionSaveList,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                    var Sum = 0;
                    for (var i = 0; i < $scope.ProcessDetentionLists.length; i++) {
                        if (!baseService.isUndefinedOrNull($scope.ProcessDetentionLists[i].Minute)) {
                            Sum = parseInt(Sum) + parseInt($scope.ProcessDetentionLists[i].Minute);
                        }

                    }
                    $scope.NewObject.SumMin = Sum;
                    //$scope.getProcessDetention();
                    var gridObj = $("#GridDailyTargetList").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                }
                angular.element(document.querySelector('#articlePoUp')).modal('hide');
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    //$scope.getProcessDetention = function () {
    //    try {
    //        $scope.ProcessDetentionLists = [];
    //        for (var i = 1; i < 6; i++) {
    //            var obj = angular.copy($scope.ProcessDetention);
    //            obj.Id = null;
    //            obj.ProcessId = $scope.DailyProductionTargetNew.ProcessId;
    //            obj.EntityId = $scope.DailyProductionTargetNew.EntityId;
    //            obj.TargetDate = $scope.DailyProductionTargetNew.TargetDate;
    //            obj.ProductionShiftId = $scope.DailyProductionTargetNew.ProductionShiftId;
    //            obj.workCenter = $scope.DailyProductionTargetNew.workCenterId;
    //            obj.Sequence = i;
    //            $scope.ProcessDetentionLists.push(obj);
    //        }

    //        $http.get('Productions/RunningMachineSetUpTarget/GetProcessDetentionData?processId=' + $scope.DailyProductionTargetNew.ProcessId + '&entityId=' + $scope.DailyProductionTargetNew.EntityId + '&productionDate=' + $scope.DailyProductionTargetNew.TargetDate + '&shiftId=' + $scope.DailyProductionTargetNew.ProductionShiftId + '&workcenter=' + data.data.WorkCenterMasterId)
    //            .then(
    //                function successCallback(response) {
    //                    if (response.data.length > 0) {

    //                        for (var j = 0; j < response.data.length; j++) {
    //                            for (var k = 0; k < $scope.ProcessDetentionLists.length; k++) {
    //                                if ($scope.ProcessDetentionLists[k].Sequence == response.data[j].Sequence) {
    //                                    $scope.ProcessDetentionLists[k].Flag = response.data[j].Flag;
    //                                    $scope.ProcessDetentionLists[k].Id = response.data[j].Id;
    //                                    $scope.ProcessDetentionLists[k].workCenter = response.data[j].WorkCenter;
    //                                    $scope.ProcessDetentionLists[k].DepartmentId = response.data[j].DepartmentId;
    //                                    $scope.ProcessDetentionLists[k].DepartmentName = response.data[j].DepartmentName;
    //                                    $scope.ProcessDetentionLists[k].DetentionTypeList = response.data[j].DetentionTypeList;
    //                                    $scope.ProcessDetentionLists[k].DetentionList = response.data[j].DetentionList;
    //                                    $scope.ProcessDetentionLists[k].DetentionId = response.data[j].DetentionId;
    //                                    $scope.ProcessDetentionLists[k].DetentionTypeId = response.data[j].DetentionTypeId;
    //                                    $scope.ProcessDetentionLists[k].Detention = response.data[j].Detention;
    //                                    $scope.ProcessDetentionLists[k].Minute = response.data[j].Minute;
    //                                    $scope.ProcessDetentionLists[k].ResponsiblePersonId = response.data[j].ResponsiblePersonId;
    //                                    $scope.ProcessDetentionLists[k].ResponsiblePerson = response.data[j].ResponsiblePerson;
    //                                    $scope.ProcessDetentionLists[k].Remark = response.data[j].Remark;
    //                                }
    //                            }
    //                        }

    //                    }
    //                },
    //                function errorCallback(response) {
    //                    ShowResult(response, 'failure');
    //                });
    //        var gridObj = $("#ProductionSummaryDetentionWC").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }

    //};

    //$scope.LoadDetentionList = function () {
    //    try {
    //        $http.get('Productions/RunningMachineSetUpTarget/GetProcessDetentionData?processId=' + $scope.DailyProductionTargetNew.ProcessId + '&entityId=' + $scope.DailyProductionTargetNew.EntityId + '&productionDate=' + $scope.DailyProductionTargetNew.TargetDate + '&shiftId=' + $scope.DailyProductionTargetNew.ProductionShiftId + '&workcenter=' + data.data.WorkCenterMasterId)
    //            .then(function (response) {
    //                $scope.ProcessDetentionLists = response.data;
    //            });
    //    } catch (ex) {
    //        ShowResult(ex, 'Info');
    //    }
    //};

    $scope.deleteMasterWC = function (master) {
        if (!baseService.isUndefinedOrNull(master.data.Id)) {
            $http({
                method: 'POST',
                url: 'Productions/RunningMachineSetUpTarget/DeleteMasterWC?id=' + master.data.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    for (var i = 0; i < $scope.wcList.length; i++) {
                        if ($scope.DailyTargetList[i].Id == master.data.Id) {
                            $scope.DailyTargetList[i].Id = null;
                            break;
                        }
                    }
                    var gridObj = $("#GridDailyTargetList").data("ejGrid");
                    gridObj.refreshContent();
                    gridObj.refreshTemplate();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult("Production not found...", 'Info');
        }
    }
    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.DailyProductionTarget = {}
        $scope.DailyTargetList = [];
        $scope.SOItemList = [];
    }


    //search PR
    $scope.SelectedLineForPR = {};
    $scope.SOItemList = [];
    $scope.SearchPRPopup = function (data) {
        $scope.SelectedLineForPR = data;
        if (baseService.isUndefinedOrNull(data.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.SOItemList = [];
        $http.get($scope.path + 'GetProductionOrderPOPUp?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&processId=' + $scope.DailyProductionTargetNew.ProcessId + '&PlanHours=' + $scope.DailyProductionTargetNew.HeaderPlanHour)
            .then(
                function successCallback(response) {
                    $scope.SOItemList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    }

    $scope.selectSOItem = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            if ($scope.SelectedLineForPR.WorkCenterMasterId == $scope.DailyTargetList[i].WorkCenterMasterId && $scope.SelectedLineForPR.ProductionOrderId == $scope.DailyTargetList[i].ProductionOrderId) {
                //if ($scope.SelectedLineForPR.ProductionOrderId == $scope.DailyTargetList[i].ProductionOrderId) {
                $scope.DailyTargetList[i].ProductionOrderId = args.data.Id;
                $scope.DailyTargetList[i].Material = args.data.Material;
                $scope.DailyTargetList[i].Article = args.data.Article;
                $scope.DailyTargetList[i].MaterialMasterId = args.data.MaterialMasterId;
                $scope.DailyTargetList[i].MaterialMasterArticleId = args.data.ArticleId;
                $scope.DailyTargetList[i].CustomerPONo = args.data.CustomerPONo;
                $scope.DailyTargetList[i].BuyerItemNo = args.data.BuyerItemNo;
                $scope.DailyTargetList[i].SMV = args.data.SPT;
                $scope.DailyTargetList[i].LotNumber = args.data.LotNumber;

                $scope.DailyTargetList[i].TargetProductionFP = (dbl(60 / dbl($scope.DailyTargetList[i].SMV)) * ($scope.DailyTargetList[i].WorkStation) * ($scope.DailyTargetList[i].PlanHours)).toFixed(0);
                $scope.DailyTargetList[i].TargetFD = (dbl(60 / dbl($scope.DailyTargetList[i].SMV)) * ($scope.DailyTargetList[i].WorkStation) * ($scope.DailyTargetList[i].PlanHours) * dbl($scope.DailyTargetList[i].Efficiency) / 100).toFixed(0);

                angular.element(document.querySelector('#POItemPopup')).modal('hide');
                //}
                break;

            }
        }

        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }
    $scope.CalculateTotalQuantity = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Quantity = (dbl($scope.DailyTargetList[i].QuantityPerHour) * dbl($scope.DailyTargetList[i].TotalHour)).toFixed(0);

        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        //gridObj.refreshTemplate();
    }


    $scope.CalculateTargetProductioin = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].TargetProductionFP = (dbl(60 / dbl($scope.DailyTargetList[i].SMV)) * ($scope.DailyTargetList[i].WorkStation) * ($scope.DailyTargetList[i].PlanHours)).toFixed(0);
        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.CalculatePlanProductioin = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].TargetFD = (dbl(60 / dbl($scope.DailyTargetList[i].SMV)) * ($scope.DailyTargetList[i].WorkStation) * ($scope.DailyTargetList[i].PlanHours) * dbl($scope.DailyTargetList[i].Efficiency) / 100).toFixed(0);
        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
    }

    $scope.DifferenceFP = null;
    $scope.CalculateTargetProductioinDiff = function (data) {
        $scope.NewObject = data.data;
        $scope.RMSDiffId = $scope.NewObject.Id;
        $http({
            method: 'GET',
            url: 'Productions/RunningMachineSetUpTarget/GetTargetProductioinDiff?RMSTargetId=' + $scope.RMSDiffId
        }).then(function successCallback(response) {
            $scope.DifferenceFP = response.data[0].DifferenceFP;
        });
    }

    $scope.getSalesOrderPopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getSalesOrder();
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('show');
    }

    $scope.SalesOrderItemList = [];
    $scope.getSalesOrder = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSalesOrder?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.DailyProductionTargetNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.SalesOrderItemList = resp.data;
        });
    }

    $scope.selectSalesOrderItem = function (e) {
        $scope.Newobject.SalesOrderId = e.data.SOId;
        $scope.Newobject.SOArticle = e.data.Article;
        angular.element(document.querySelector('#SalesOrderItemPopup')).modal('hide');
    }

    $scope.getMasterOrderItemPopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getMasterOrderItem();
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('show');
    }

    $scope.MasterOrderItemList = [];
    $scope.getMasterOrderItem = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMasterOrderItem?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.DailyProductionTargetNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.MasterOrderItemList = resp.data;
        });
    }

    $scope.selectMasterOrderItem = function (e) {
        $scope.Newobject.MasterOrderItemId = e.data.MasterOrderItemId;
        $scope.Newobject.MOIArticle = e.data.Article;
        angular.element(document.querySelector('#MasterOrderItemPopup')).modal('hide');
    }

    $scope.getProductCodePopUp = function (data) {
        $scope.Newobject = data.data;
        $scope.getProductCode();
        angular.element(document.querySelector('#ProductCodePopup')).modal('show');
    }

    $scope.ProductCodeList = [];
    $scope.getProductCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProductCode?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&workCenterMasterId=' + $scope.Newobject.WorkCenterMasterId + '&productionLevel=' + $scope.Newobject.BookingLevel + '&processId=' + $scope.DailyProductionTargetNew.ProcessId + '&ProductionOrderId=' + $scope.Newobject.ProductionOrderId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProductCodeList = resp.data;
        });
    }

    $scope.selectProductCode = function (e) {
        $scope.Newobject.MasterOrderItemId = e.data.MOIId;
        $scope.Newobject.ProductCodeArticle = e.data.Article;
        angular.element(document.querySelector('#ProductCodePopup')).modal('hide');
    }

    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.IsManual == true)
            e.row.css("background-color", '#d1e5ff');


    }
    $scope.ShowDiv = false;
    $scope.AddLineItemG = function (obj) {
        $scope.SelectedLine = obj.data;
        $scope.ShowDiv = true;
        var eDialog = $("#dialogLineDesign").data("ejDialog");
        $("#dialogLineDesign").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogLineDesign").ejDialog("refresh");

        eDialog.open();
        if (obj.data.HasLayout == false) {
            $scope.CopyTable(obj.data);
        }
        else {
            $scope.GetLineLayout(obj.data);
        }

    };

    $scope.PopupItem = function (data) {
        $scope.SelectedLine = data;
        $scope.ShowDiv = true;
        var eDialog = $("#dialogLineDesignReport").data("ejDialog");
        $("#dialogLineDesignReport").ejDialog("refresh");

        eDialog.open();


    };


    $scope.CopyTable = function (data) {
        try {
            $scope.SelectedLineForPR = data;
            $http({
                method: 'POST',
                url: $scope.Copy,
                data: { 'entityid': $scope.DailyProductionTargetNew.EntityId, 'processId': $scope.DailyProductionTargetNew.ProcessId, 'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate, 'SelectedLine': $scope.SelectedLineForPR },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.SelectedLineForPR.HasLayout = true;
                    $scope.SelectedLineForPR.CanCopy = false;
                    var gridObj = $("#gridEmployeeReplace").data("ejGrid");
                    gridObj.refreshContent();
                    $scope.GetLineLayout(data);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetLineLayout = function (data) {
        try {
            $scope.nodes = [];
            $scope.NodeIndex = 0;
            $scope.NodeSeed = (new Date()).getTime();
            $scope.SelectedLineForPR = data;
            $http({
                method: "POST",
                url: $scope.path + 'GetSaveData',
                data: {
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                response.data = JSON.parse(response.data[0].Layout);

                var diagram = $("#diagram").ejDiagram("instance");
                diagram.clear();
                diagram.add(response.data);
                $scope.UpdateEmployeeAttendanceAndProductionInfo();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.drawingToolsList = [
        {
            id: "Rectangle_Tool", tooltiptext: "Rectangle",
            spriteCss: "glyphicon glyphicon-stop",
        }, {
            id: "RoundedRectangle_Tool", tooltiptext: "RoundRect",
            spriteCss: "glyphicon glyphicon-unchecked",
        }, {
            id: "Ellipse_Tool", tooltiptext: "Ellipse",
            spriteCss: "glyphicon glyphicon-cd",
        }
        , {
            id: "UpArrow", tooltiptext: "UpArrow",
            spriteCss: "glyphicon glyphicon-arrow-up",
        }
        , {
            id: "RightArrow", tooltiptext: "RightArrow",
            spriteCss: "glyphicon glyphicon-arrow-right blue",
        }
        , {
            id: "DownArrow", tooltiptext: "DownArrow",
            spriteCss: "glyphicon glyphicon-arrow-down",
        }
        , {
            id: "LeftArrow", tooltiptext: "LeftArrow",
            spriteCss: "glyphicon glyphicon-arrow-left",
        },
        {
            id: "Textbox_Tool", tooltiptext: "Textbox",
            spriteCss: "glyphicon glyphicon-text-background",
        },
        {
            id: "Image_Tool", tooltiptext: "Image",
            spriteCss: "glyphicon glyphicon-picture",
        },
        {
            id: "Html_Tool", tooltiptext: "Html",
            spriteCss: "glyphicon glyphicon-header",
        },
    ];

    $scope.width = "100%";
    $scope.height = "300px";

    $scope.pageSettings = { scrollLimit: "diagram", boundaryConstraints: ej.datavisualization.Diagram.BoundaryConstraints.Diagram };


    $scope.selectednode = null;
    $scope.NodeIndex = 0;
    $scope.NodeSeed = (new Date()).getTime();
    $scope.nodeCollectionChange = function (args) {

        if (args["state"] != "changed")
            return;

        if (args["changeType"] == "remove") {
            for (var i = 0; i < $scope.nodes.length; i++) {
                if ($scope.nodes[i]["id"] == args.element["id"]) {
                    $scope.nodes.splice(i, 1);
                    break;
                }
            }
        }

        if (args["changeType"] == "insert") {
            if (args["cause"] != "clipBoard") {
                for (var i = 0; i < $scope.nodes.length; i++) {
                    if ($scope.nodes[i]["id"] == args.element["id"]) {
                        return;
                    }
                }
            }

            $scope.NodeIndex++;
            args.element["id"] = $scope.NodeSeed + '-' + $scope.NodeIndex;
            $scope.nodes.push(args.element);
        }

    }
    $scope.onItemclick = function (args) {
        var diagram = $("#diagram").ejDiagram("instance");
        var option = args.currentTarget.id;
        switch (option) {
            case "Rectangle_Tool":
                diagram.model.drawType = { type: "basic", shape: "rectangle" };
                break;
            case "RoundedRectangle_Tool":
                diagram.model.drawType = { type: "basic", shape: "rectangle", "cornerRadius": 5 };
                break;
            case "Ellipse_Tool":
                diagram.model.drawType = { type: "basic", shape: "ellipse" };
                break;
            case "UpArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 1l-15 15h9v16h12v-16h9z" };
                break;
            case "RightArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M31 16l-15-15v9h-16v12h16v9z" };
                break;
            case "DownArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 31l15-15h-9v-16h-12v16h-9z" };
                break;
            case "LeftArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M1 16l15 15v-9h16v-12h-16v-9z" };
                break;
            case "Polygon_Tool":
                diagram.model.drawType = { type: "basic", shape: "polygon", points: [{ x: 13.560, y: 67.524 }, { x: 21.941, y: 41.731 }, { x: 0.000, y: 25.790 }, { x: 27.120, y: 25.790 }, { x: 35.501, y: 0.000 }, { x: 43.882, y: 25.790 }, { x: 71.000, y: 25.790 }, { x: 49.061, y: 41.731 }, { x: 57.441, y: 67.524 }, { x: 35.501, y: 51.583 }, { x: 13.560, y: 67.524 }] };
                break;
            case "Textbox_Tool":
                diagram.model.drawType = { type: "text", textBlock: { "text": "TextNode", textAlign: ej.datavisualization.Diagram.TextAlign.Center }, fillColor: "transparent", borderColor: "transparent" };
                break;
            case "Image_Tool":
                diagram.model.drawType = { type: "image", source: "content/images/Employees/6.png" };
                break;
            case "Html_Tool":
                diagram.model.drawType = {
                    type: "html", templateId: "htmlTemplate"
                };
                break;
        }

        var _tool = diagram.tool();
        diagram.update({ tool: _tool | ej.datavisualization.Diagram.Tool.DrawOnce });
        //  diagram.update({ tool: _tool });
    }
    $scope.OpenEmployeeSearchBox = function () {
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        $("#dialogSearchEmployee").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogSearchEmployee").ejDialog("refresh");

        eDialog.open();

        $scope.getEmployeeData();
    }
    $scope.EmployeemodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'EmployeeCode', name: 'Code ' },
        { value: 'EmployeeName', name: 'Name ' },
        { value: 'Department', name: 'Department ' },
        { value: 'Designation', name: 'Designation ' },
        { value: 'Section', name: 'Section ' },
        { value: 'SubSection', name: 'Sub Section ' },
        { value: 'OtherSkills', name: 'Other Skills ' }
    ];
    $scope.searchCol = "UserName";
    $scope.searchVal = "";
    $scope.EmployeeSearchCol = "EmployeeName";
    $scope.EmployeeSearchVal = "";
    $scope.WhereEmployeeNeeded = '';
    $scope.EmployeeList = [];
    $scope.getEmployeeData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal,
                    'OperationId': $scope.selectednode.items[0].addInfo.OperationId,
                    'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.ViewEmployeeStatus = function (args) {
        try {

            if (angular.isUndefinedOrNull(args.data.WorkCenterMasterId) == false) {
                if (args.data.WorkCenterMasterId != $scope.SelectedLine.WorkCenterMasterId) {
                    ShowResult("Employee has already been " + args.data.AssignmentStatus, 'failure');
                    return;
                }
            }
            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("id", "notEqual", $scope.selectednode.items[0].id));
            for (var i = 0; i < exists.length; i++) {
                if (exists[i].addInfo.EmployeeId == args.data.Id) {
                    ShowResult("Employee has already been " + args.data.AssignmentStatus, 'failure');
                    return;
                }
            }

            $scope.selectednode.items[0].addInfo.EmployeeId = args.data.Id;
            $scope.selectednode.items[0].addInfo.EmployeeName = args.data.EmployeeName;
            $scope.selectednode.items[0].addInfo.EmpPicPath = args.data.EmpPicPath;
            $scope.selectednode.items[0].addInfo.Designation = args.data.Designation;
            $scope.selectednode.items[0].addInfo.EmployeeCode = args.data.EmployeeCode;
            $scope.selectednode.items[0].addInfo["DayStatus"] = args.data.DayStatus;
            $scope.selectednode.items[0].addInfo["DayColor"] = args.data.DayColor;

            $scope.ConstructReplaceEmployee();

            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
    }


    //////////////////////////////////////////
    $scope.OpenFixedAssetSearchBox = function () {
        var eDialog = $("#dialogSearchFixedAsset").data("ejDialog");
        $("#dialogSearchFixedAsset").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogSearchFixedAsset").ejDialog("refresh");

        eDialog.open();

        $scope.getFixedAssetData();
    }

    $scope.FixedAssetmodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'Model', name: 'Model ' },
        { value: 'SerialNo', name: 'SerialNo ' },
        { value: 'YearOfManufacture', name: 'Year ' },
        { value: 'Description', name: 'Description ' },
        { value: 'AssetNo', name: 'Asset No ' },
        { value: 'Status', name: 'Status ' },
        { value: 'Brand', name: 'Brand ' },
        { value: 'CountryOfOrigin', name: 'Country Of Origin ' },
        { value: 'Vendor', name: 'Vendor ' }
    ];
    $scope.FixedAssetSearchCol = "Description";
    $scope.FixedAssetSearchVal = "";
    $scope.WhereFixedAssetNeeded = '';
    $scope.FixedAssetList = [];
    $scope.getFixedAssetData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.FixedAssetSearchCol, 'value': $scope.FixedAssetSearchVal, 'ArticleId': $scope.selectednode.items[0].addInfo.ArticleId },
                url: $scope.path + 'SearchFixedAsset'

            }).then(function successCallback(response) {
                $scope.FixedAssetList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.ViewFixedAssetStatus = function (args) {
        try {

            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("id", "notEqual", $scope.selectednode.items[0].id));
            for (var i = 0; i < exists.length; i++) {
                if (exists[i].addInfo.FixedAssetRegisterId == args.data.Id) {
                    ShowResult("Asset has already been tagged with another workstation", 'failure');
                    return;
                }
            }

            $scope.selectednode.items[0].addInfo.FixedAssetRegisterId = args.data.Id;
            $scope.selectednode.items[0].addInfo.FixedAssetRegisterDesc = args.data.FixedAssetDesc;

            var eDialog = $("#dialogSearchFixedAsset").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
    }


    $scope.nodes = [];
    $scope.operationList = [];
    $scope.operationButtonClick = function (args) {
        $scope.selectednode = args;
        //$scope.selectednode.items[0].addInfo;
        $http({
            method: 'GET',
            url: 'IE/LineLayoutForProductionBulletin/GetOperationList?ProductionBulletinMasterId=' + $scope.SelectedLine.ProductionBulletinId + '&ProcessId=' + $scope.DailyProductionTargetNew.ProcessId,
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
            angular.element(document.querySelector("#modalOperationList")).modal("toggle");
        });
    }

    $scope.recordoperationdoubleclick = function (args) {

        try {
            $scope.selectednode.items[0].addInfo.MaterialMasterId = args.data.MaterialMasterId;
            $scope.selectednode.items[0].addInfo.MaterialMasterDesc = args.data.MaterialMasterDesc;
            $scope.selectednode.items[0].addInfo.ArticleId = args.data.ArticleId;
            $scope.selectednode.items[0].addInfo.ArticleDesc = args.data.ArticleDesc;
            $scope.selectednode.items[0].addInfo.ArticleShortName = args.data.ArticleShortName;
            $scope.selectednode.items[0].addInfo.OperationId = args.data.OperationId;
            $scope.selectednode.items[0].addInfo.OperationDesc = args.data.OperationDesc;
            $scope.selectednode.items[0].addInfo.OperationVariationId = args.data.OperationVariationId;
            $scope.selectednode.items[0].addInfo.OperationVariationDesc = args.data.OperationVariationDesc;
            $scope.selectednode.items[0].addInfo.MachineOrHand = args.data.IsMachineRequired;
            $scope.selectednode.items[0].addInfo.TotalSPT = args.data.TotalSPT;
            //$scope.selectednode.items[0].addInfo.WorkstationTargetPerHour = args.data.WorkstationTargetPerHour;

            $scope.selectednode.items[0].addInfo.FixedAssetRegisterId = null;
            $scope.selectednode.items[0].addInfo.FixedAssetRegisterDesc = null;
            $scope.selectednode.items[0].addInfo.EmployeeId = null;
            $scope.selectednode.items[0].addInfo.EmployeeName = null;
            $scope.selectednode.items[0].addInfo.EmpPicPath = null;
            $scope.selectednode.items[0].addInfo.Designation = null;
            $scope.selectednode.items[0].addInfo.EmployeeCode = null;
            $scope.selectednode.items[0].addInfo["DayStatus"] = null;
            $scope.selectednode.items[0].addInfo["DayColor"] = null;

            angular.element(document.querySelector("#modalOperationList")).modal("hide");
        } catch (e) {

        }
    }

    $scope.EmployeeSearchFrom = 'card';
    $scope.employeeButtonClick = function (args, source, nodename) {
        $scope.EmployeeSearchFrom = source;

        if (angular.isUndefinedOrNull(nodename) == false) {
            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("name", "equal", nodename));
            if (exists)
                $scope.selectednode = { "items": exists };
        }
        else {
            $scope.selectednode = args;
        }
        $scope.OpenEmployeeSearchBox();
    }
    $scope.FixedAssetButtonClick = function (args) {

        $scope.selectednode = args;
        $scope.OpenFixedAssetSearchBox();
    }
    $scope.ViewEmployeeCard = function (args) {

        $scope.selectednode = args;
        $scope.GetEmployeeCard();
    }

    $scope.ExplicitSave = false;
    $scope.SaveDiagram = function () {
        var _explicitSave = $scope.ExplicitSave;
        $scope.ExplicitSave = false;
        try {

            $http({
                method: 'POST',
                url: $scope.path + "SaveDiagram",
                data: {
                    'Nodes': $scope.nodes, 'Design': JSON.stringify($scope.nodes),
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //if (_explicitSave)
                    ShowResult(response.data.Message, 'success');

                    $scope.SelectedLine.ManPowerWithMachine = response.data.Data[0].TotalMachine;
                    $scope.SelectedLine.ManPowerWithHand = response.data.Data[0].TotalHand;
                    $scope.SelectedLine.HasLayout = true;

                    //var gridObj = $("#GridDailyTargetList").data("ejGrid");
                    //gridObj.refreshContent();
                    //$scope.getDailytarget();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.showCardIcons = false;
    $scope.EmployeeCard = [];
    $scope.EmployeeCardSkills = [];
    $scope.GetEmployeeCard = function () {
        $scope.EmployeeCard = [];
        $scope.EmployeeCardSkills = [];
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'EmployeeId': $scope.selectednode.items[0].addInfo.EmployeeId,
                    'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId,
                    'AssetRegisterId': $scope.selectednode.items[0].addInfo.FixedAssetRegisterId,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'GetEmployeeCard'

            }).then(function successCallback(response) {
                $scope.EmployeeCard = response.data;
                $scope.EmployeeCardSkills = response.data[0][0].SkillList;
            });
            var eDialog = $("#dialogEmployeeCard").data("ejDialog");
            $("#dialogEmployeeCard").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
            $("#dialogEmployeeCard").ejDialog("refresh");
            eDialog.open();
        } catch (e) {

        }
    }

    $scope.contextMenu = { items: [{ "id": "Properties", "name": "Properties", "text": "Properties", "image": "", "style": "" }] };
    $scope.onDiagramContextMenuClick = function (args) {
        if (args.text == 'Properties') {
            $scope.selectednode = args;
            $scope.ShowEditEmployeeCard();
        }
    }
    $scope.UpdateColor = function (args) {
        if (args.isInteraction == false)
            return;
        var diagram = $("#diagram").ejDiagram("instance");
        $scope.selectednode.target[args.model.Field] = args.value;

        var Property = args.model.Field;
        try {
            if ($scope.selectednode.target.hasOwnProperty('children')) {
                for (var i = 0; i < $scope.selectednode.target.children.length; i++) {
                    diagram.updateNode($scope.selectednode.target.children[i], { Property: args.value });
                }
            }
            else {
                diagram.updateNode($scope.selectednode.target.name, { Property: args.value });
            }
        } catch (e) { }
    }
    $scope.ShowEditEmployeeCard = function () {
        try {

            var eDialog = $("#dialogEditNode").data("ejDialog");
            $("#dialogEditNode").ejDialog("refresh");
            $("#dialogEditNode").ejDialog("refresh");

            eDialog.open();
        } catch (e) {

        }
    }

    $scope.UpdateEmployeeAttendanceAndProductionInfo = function () {
        try {

            var empIds = "''";
            for (var i = 0; i < $scope.nodes.length; i++) {
                empIds += ",'" + $scope.nodes[i].addInfo.EmployeeId + "'";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'EmployeeId': empIds,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'UpdateEmployeeAttendanceAndProductionInfo'

            }).then(function successCallback(response) {
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var exists = ej.DataManager(response.data).executeLocal(ej.Query().where("EmployeeId", "equal", $scope.nodes[i].addInfo.EmployeeId));
                    if (exists.length > 0) {
                        $scope.nodes[i].addInfo["DayStatus"] = exists[0].DayStatus;
                        $scope.nodes[i].addInfo["DayColor"] = exists[0].DayColor;
                        $scope.nodes[i].addInfo["ProductionQuantity"] = exists[0].ProductionQuantity;
                    }
                }

                $scope.SaveDiagram();
            });

        } catch (e) {

        }
    }


    //chang employee
    $scope.ReplaceEmployeeList = [];
    $scope.GetListOfReplaceEmployees = function () {

        $scope.ConstructReplaceEmployee();

        var eDialog = $("#dialogEmployeeReplace").data("ejDialog");
        $("#dialogEmployeeReplace").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogEmployeeReplace").ejDialog("refresh");
        eDialog.open();

    }
    $scope.ConstructReplaceEmployee = function () {
        $scope.ReplaceEmployeeList = [];
        for (var i = 0; i < $scope.nodes.length; i++) {
            var model = Object.assign({}, $scope.nodes[i].addInfo);
            if (model.hasOwnProperty('EmployeeId')) {
                if (model.DayStatus == 'A' || model.DayStatus == 'LV' || angular.isUndefinedOrNull(model.DayStatus)) {
                    model["name"] = $scope.nodes[i]["name"];
                    $scope.ReplaceEmployeeList.push(model);
                }
            }
        }

        try {
            var gridObj = $("#gridEmployeeReplace").data("ejGrid");
            gridObj.refreshContent();
        } catch (e) {

        }

    }

    $scope.ProductionEntryList = [];
    $scope.ConstructProductionEntry = function () {
        $scope.ProductionEntryList = [];
        for (var i = 0; i < $scope.nodes.length; i++) {
            var model = Object.assign({}, $scope.nodes[i].addInfo);
            if (model.hasOwnProperty('EmployeeId')) {
                if (angular.isUndefinedOrNull(model["EmployeeId"]))
                    continue;

                model["name"] = $scope.nodes[i]["name"];
                model["CurrentQuantity"] = 0;
                $scope.ProductionEntryList.push(model);
            }
        }

        try {
            var gridObj = $("#gridProductionEntry").data("ejGrid");
            gridObj.refreshContent();

            var eDialog = $("#dialogProductionEntry").data("ejDialog");
            $("#dialogProductionEntry").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
            $("#dialogProductionEntry").ejDialog("refresh");
            eDialog.open();
        } catch (e) {

        }

    }


    $scope.SaveProductionQuantity = function () {
        try {

            $http({
                method: "POST",
                url: $scope.path + 'SaveProductionData',
                data: {
                    'ProductionData': $scope.ProductionEntryList,
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                var eDialog = $("#dialogProductionEntry").data("ejDialog");
                eDialog.close();
                $scope.UpdateEmployeeAttendanceAndProductionInfo();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.DownloadReport = function () {
        try {
            var Entity = $("#ddlEntity option:selected").text();
            var Process = $("#ProcessId option:selected").text();
            $http({
                method: 'POST',
                url: 'Productions/MachineLayoutReport/Report',
                data: {
                    'EntityId': $scope.DailyProductionTargetNew.EntityId,
                    'ProcessId': $scope.DailyProductionTargetNew.ProcessId,
                    'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLine.WorkCenterMasterId,
                    'Data': $scope.SelectedLine, 'EntityName': Entity, 'ProcessName': Process, 'WithEmp': $scope.WithEmployee, 'WithMachine': $scope.WithMachine
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //chart
    $scope.graphmaxheight = function (list, column) {
        var _graphmaxheight = 10;
        _graphmaxheight = 10;
        for (var i = 0; i < list.length; i++) {
            if (list[i][column] > _graphmaxheight)
                _graphmaxheight = list[i][column];
        }

        return _graphmaxheight + (_graphmaxheight * .30);
    }

    $scope.graphmaxwidth = function (list, width) {
        if (baseService.isUndefinedOrNull(width))
            width = 100;

        return ((list.length * width) + 100) + 'px';
    }
    $scope.StripLineSetting = [];
    $scope.BottleneckData = [];
    $scope.BottleneckValue = 0;
    $scope.GetBottleneck = function (data) {
        $scope.SelectedLineForPR = data;
        $scope.StripLineSetting = [];
        $scope.BottleneckData = [];
        try {
            var eDialog = $("#dialogBottleneckGraph").data("ejDialog");
            eDialog.open();
            $http({
                method: "POST",
                url: $scope.path + 'GetBottleneck',
                data: {
                    'ProcessId': $scope.DailyProductionTargetNew.ProcessId,
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {


                $scope.BottleneckData = response.data.GraphData;

                var LowerBoundValue = response.data.StripLine[0].LowerBoundValue;
                var LowerBoundText = response.data.StripLine[0].LowerBoundText;
                var UpperBoundValue = response.data.StripLine[0].UpperBoundValue;
                var UpperBoundText = response.data.StripLine[0].UpperBoundText;

                $scope.BottleneckValue = LowerBoundValue;

                if (LowerBoundValue > 0) {
                    $scope.StripLineSetting.push({ start: 0, end: LowerBoundValue, text: '', textAlignment: 'middlecenter', color: '#F5B7B1', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });
                    if (LowerBoundValue < 100)
                        $scope.StripLineSetting.push({ start: LowerBoundValue, end: UpperBoundValue, text: '', textAlignment: 'middlecenter', color: '#FCF3CF', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });

                }
                if (UpperBoundValue < 100)
                    $scope.StripLineSetting.push({ start: UpperBoundValue, end: 100, text: '', textAlignment: 'middlecenter', color: '#D5F5E3', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });



                var chartObj = $("#ChartBottleneck").data("ejChart");
                chartObj.redraw();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.chartBottleneckPreRender = function (args) {

        try {
            var points = args.model.series[0].points;//WIP

            for (var i = 0; i < points.length; i++) {
                points[i].fill = args.model.series[0].dataSource[i].Color;
                if (points[i].y < $scope.BottleneckValue)
                    points[i].fill = "#ff0000";
            }
        } catch (e) {

        }
    }

}