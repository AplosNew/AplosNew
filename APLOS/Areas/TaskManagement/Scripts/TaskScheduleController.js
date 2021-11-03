'use strict';
TaskScheduleController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http", "$controller"];
function TaskScheduleController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http, $controller) {
    //$scope.MasterOrderId = '1957';
    //$scope.GEEMasterOrderId = '1957';
    $scope.GVdisplayprop = "none;";
    $scope.GVpath = 'TaskManagement/TaskSchedule/';
    $scope.GVMasterOrderId = '';
    //$controller("MasterOrderTaskTemplateController", { cboService: cboService, $scope: $scope, $http: $http });

    $scope.GVcolumnTemplateResource = [{ resourceId: 0, resourceName: "", EmpPicPath: '' }];
    $scope.GVganttdata = [];
    $scope.GVTaskMasters = [];
    $scope.GVGanttStartDate = new Date('10/31/2019');
    $scope.GVGanttEndDate = new Date('12/31/2019');
    $scope.GVProjectStartDate = '';
    $scope.GVProjectEndDate = '';
    $scope.GVDuration = '';
    $scope.GVcolumnTemplateResource = [{ resourceId: 0, resourceName: "", EmpPicPath: '' }];
    $scope.GVGetSelectedTasks2 = function (MasterOrderId) {
        $scope.GVMasterOrderId = MasterOrderId;
        $http({
            method: 'POST',
            data: { TemplateId: $scope.GVMasterOrderId },
            url: $scope.GVpath + "GetSelectedTaskList"
        }).then(function successCallback(response) {
            $scope.GVTaskMasters = response.data.DATA;

            $scope.GVgenerateGanttResource();

            var ganttObj = $("#angulargantt").ejGantt("instance");
            ganttObj.updateScheduleDates($scope.GVGanttStartDate, $scope.GVGanttEndDate);

            $scope.GVLoadSelectedTasks();
            //$("#dialogViewIssueDetail").data("ejDialog").open();



        });
    }
    $scope.GVLoadSelectedTasks = function () {
        $http({
            method: 'POST',
            data: { TemplateId: $scope.GVMasterOrderId },
            url: $scope.GVpath + "GetSelectedTaskList"
        }).then(function successCallback(response) {
            $scope.GVTaskMasters = response.data.DATA;

            $scope.GVGanttStartDate = new Date(response.data.GanttStartDate);
            $scope.GVGanttEndDate = new Date(response.data.GanttEndDate);

            $scope.GVProjectStartDate = response.data.ProjectStartDate;
            $scope.GVProjectEndDate = response.data.ProjectEndDate;
            $scope.GVDuration = response.data.Duration;


            $scope.GVgenerateGantt();

            //
            var ganttObj = $("#angulargantt").ejGantt("instance");
            ganttObj.updateScheduleDates($scope.GVGanttStartDate, $scope.GVGanttEndDate);

        });
    }

    $scope.GVgenerateGantt = function () {


        $scope.GVdisplayprop = "block;";
        $scope.GVganttdata = [];
        for (var i = 0; i < $scope.GVTaskMasters.length; i++) {

            var _data = {
                taskID: $scope.GVTaskMasters[i].Id,
                taskName: $scope.GVTaskMasters[i].TaskDescription,
                startDate: new Date($scope.GVTaskMasters[i].TempStartDate),
                endDate: new Date($scope.GVTaskMasters[i].TempEndDate),
                predecessor: null,
                TaskAppliedOnEnum: $scope.GVTaskMasters[i].TaskAppliedOnEnum,//$scope.GVTaskMasters[i].predecessor,

                TempStartDate: $scope.GVTaskMasters[i].TempStartDate,
                TempEndDate: $scope.GVTaskMasters[i].TempEndDate,

                ActualStartDate: $scope.GVTaskMasters[i].ActualStartDate,
                ActualEndDate: $scope.GVTaskMasters[i].ActualEndDate,

                SequentialStartDate: $scope.GVTaskMasters[i].SequentialStartDate,
                SequentialEndDate: $scope.GVTaskMasters[i].SequentialEndDate,

                OriginalSequentialStartDate: $scope.GVTaskMasters[i].OriginalSequentialStartDate,
                OriginalSequentialEndDate: $scope.GVTaskMasters[i].OriginalSequentialEndDate,

                //IsTaskMilestone: $scope.GVTaskMasters[i].IsTaskMilestone,
                //IsFirstTask: $scope.GVTaskMasters[i].IsFirstTask,
                //IsLastTask: $scope.GVTaskMasters[i].IsLastTask,
                //IsMandatory: $scope.GVTaskMasters[i].IsMandatory,
                Active: $scope.GVTaskMasters[i].Active,
                HasActualDate: $scope.GVTaskMasters[i].HasActualDate,
                HasPredecessorActualDate: $scope.GVTaskMasters[i].HasPredecessorActualDate,
                offset: '',
                duration: $scope.GVTaskMasters[i].Duration,
                progress: "0",
                resourceId: []
            }
            if (angular.isUndefinedOrNull($scope.GVTaskMasters[i].resourceId) == false)
                _data.resourceId.push(parseInt($scope.GVTaskMasters[i].resourceId));

            $scope.GVganttdata.push(_data);
        }

    }
    $scope.GVgenerateGanttResource = function () {



        var columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
        var tempstring = [];
        //resourceId,resourceName
        for (var i = 0; i < $scope.GVcolumnTemplateResource.length; i++) {
            tempstring.push(parseInt($scope.GVcolumnTemplateResource[i].resourceId));
        }
        for (var i = 0; i < $scope.GVTaskMasters.length; i++) {
            columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
            try {
                if (angular.isUndefinedOrNull($scope.GVTaskMasters[i].resourceId) == false) {
                    if (tempstring.includes(parseInt($scope.GVTaskMasters[i].resourceId)) == false) {

                        columnTemplateResourceData.resourceId = parseInt($scope.GVTaskMasters[i].resourceId);
                        columnTemplateResourceData.resourceName = $scope.GVTaskMasters[i].resourceName;
                        columnTemplateResourceData.EmpPicPath = $scope.GVTaskMasters[i].EmpPicPath;

                        $scope.GVcolumnTemplateResource.push(columnTemplateResourceData);

                        tempstring.push(parseInt($scope.GVTaskMasters[i].resourceId));
                    }
                }
            } catch (e) {

            }

        }



    }
    $scope.GVganttquerycellinfo = function (args) {
        var ganttObj = $("#angulargantt").data("ejGantt");
        var data = ganttObj.model.dataSource();
        try {
            var col = this.getColumns();
            col.splice(2, 0, { field: "duration", headerText: "Duration", mappingName: "Duration", width: "60px" });
            col.splice(3, 0, { field: "OriginalSequentialStartDate", headerText: "Org. Seq. Start Date", mappingName: "originalsequentialstartdate", width: "100px" });
            col.splice(4, 0, { field: "OriginalSequentialEndDate", headerText: "Org. Seq. End Date", mappingName: "originalsequentialenddate", width: "100px" });
            col.splice(5, 0, { field: "TempStartDate", headerText: "Start Date", mappingName: "tempstartdate", width: "100px" });
            col.splice(6, 0, { field: "TempEndDate", headerText: "End Date", mappingName: "tempenddate", width: "100px" });
            col.splice(7, 0, { field: "ActualStartDate", headerText: "Own Start Date", mappingName: "actualstartdate", width: "100px" });
            col.splice(8, 0, { field: "ActualEndDate", headerText: "Own End Date", mappingName: "actualenddate", width: "100px" });
            col.splice(9, 0, { field: "SequentialStartDate", headerText: "Seq. Start Date", mappingName: "sequentialstartdate", width: "100px" });
            col.splice(10, 0, { field: "SequentialEndDate", headerText: "Seq. End Date", mappingName: "sequentialenddate", width: "100px" });
            
            col[0].visible = false;
            //col[9].visible = false;
            //col[10].visible = false;
            col[11].visible = false;
            col[12].visible = false;
            col[13].visible = false;
            col[14].visible = false;
            col[15].visible = false;
            ganttObj.ejGantt("setSplitterPosition", "280px");

        } catch (e) {

        }
    }

    $scope.GVtaskbarTooltipTemplateId = "GVtooltipTemplate";
    $scope.GVganttqueryTaskbarInfo = function (args) {
        args.taskbarBorder = "transparent";

        if (args.data.item.TaskAppliedOnEnum == "MasterOrder") {
            args.taskbarBackground = "#3498DB";
        }
        if (args.data.item.TaskAppliedOnEnum == "Style") {
            args.taskbarBackground = "#DAF7A6";
        }
        if (args.data.item.TaskAppliedOnEnum == "SalesOrder") {
            args.taskbarBackground = "#FFC300";
        }
        if (args.data.item.TaskAppliedOnEnum == "ProductionOrder") {
            args.taskbarBackground = "#BB8FCE";
        }


        if (args.data.item.Active == false) {
            args.taskbarBorder = "#red";
        }
        if (args.data.item.HasActualDate == false) {
            args.taskbarBorder = "#000000";
        }
        if (args.data.item.HasPredecessorActualDate == false) {
            args.taskbarBorder = "#EAECEE";
        }


    }


}