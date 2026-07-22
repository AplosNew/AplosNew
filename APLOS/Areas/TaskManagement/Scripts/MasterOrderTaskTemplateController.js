'use strict';
MasterOrderTaskTemplateController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function MasterOrderTaskTemplateController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    //$scope.GEEMasterOrderId = '1957';
    //$controller("TaskScheduleController", { $scope: $scope, $http: $http });
    $scope.GEEMasterOrderId = '';
    $scope.GEEdisplayprop = "none;";
    $scope.GEEpath = 'TaskManagement/MasterOrderTaskTemplate/';
    $scope.GEEpathTaskMaster = 'TaskManagement/TaskMasterCreation/';
    $scope.GEEgetSeqUrl = $scope.GEEpath + 'getautosequence';
    $scope.GEEmodelMain = {
        Id: null,
        Sequence: 0,
        Code: null,
        EmployeeId: null,
        EmployeeCode: null,//view only
        EmployeeName: null,//view only
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };

    $scope.GEETaskCategoryList = [];
    $scope.GEETaskAppliedOnList = [];
    $scope.GEETaskDependentDateList = [];
    $scope.GEETaskTypeList = ["Normal", "Important", "Critical"];
    $scope.GEEResponsiblePersonCategoryList = ["Buyer", "Entity", "Employee"];

    $scope.GEEmodel = Object.assign({}, $scope.GEEmodelMain);
    $scope.GEETaskMasters = [];
    $scope.GEESubTaskList = [];
    $scope.GEEMasterOrderTaskTemplate = [];

    $scope.GEEmodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'Code', name: 'Code ' },
        { value: 'UserName', name: 'Template Name ' },
        { value: 'ShortName', name: 'Short Name ' },
        { value: 'StandardName', name: 'Standard Name ' },
        { value: 'Remarks', name: 'Remarks ' }
    ];
    $scope.GEEEmployeemodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'EmployeeCode', name: 'Code ' },
        { value: 'EmployeeName', name: 'Name ' },
        { value: 'Department', name: 'Department ' },
        { value: 'Designation', name: 'Designation ' },
        { value: 'Section', name: 'Section ' },
        { value: 'SubSection', name: 'Sub Section ' }
    ];
    $http({
        method: 'GET',
        url: $scope.GEEpathTaskMaster + "GetMasterData"
    }).then(function successCallback(response) {
        $scope.GEETaskCategoryList = response.data.TaskCategory;
        $scope.GEETaskAppliedOnList = response.data.TaskAppliedOn;

    });




    $scope.GEEsearchCol = "EmployeeName";
    $scope.GEEsearchVal = "";
    $scope.GEEEmployeeSearchCol = "EmployeeName";
    $scope.GEEEmployeeSearchVal = "";
    $scope.GEEgetData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.GEEpath + "GetList?column=" + $scope.GEEsearchCol + "&value=" + $scope.GEEsearchVal
        }).then(function successCallback(response) {
            $scope.GEEMasterOrderTaskTemplate = response.data;
        });


    }; $scope.GEEgetData();
    $scope.GEEGet = function () {

        $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);

    };


    $scope.GEEGetSelectedTasks = function (MasterOrderId) {
        $scope.GEEMasterOrderId = MasterOrderId;
        $http({
            method: 'POST',
            data: { TemplateId: $scope.GEEMasterOrderId },
            url: $scope.GEEpath + "GetSelectedTaskList"
        }).then(function successCallback(response) {
            $scope.GEEUpdateLoggedTnA();

            $scope.GEESubTaskList = [];
            $scope.GEEDependencyList = [];
            $scope.GEESingleMasterOrderTaskTemplate = {};

            $scope.GEETaskMasters = response.data;
            $scope.GEEgenerateGanttResource();

            $scope.GEEGetSelectedTasks2();
        });
    }

    $scope.GEEGetSelectedTasks2 = function () {
        $http({
            method: 'POST',
            data: { TemplateId: $scope.GEEMasterOrderId },
            url: $scope.GEEpath + "GetSelectedTaskList"
        }).then(function successCallback(response) {


            $scope.GEEgenerateGantt();


        });
    }
    $scope.GEEGetSequence = function () {
        cboService.getSequence($scope.GEEgetSeqUrl, function (data) {
            $scope.GEEmodel.Sequence = data;
        });
    };
    $scope.GEEGetSequence();
    $scope.GEESubTask = {};
    $scope.GEEEditSubTask = function (args) {

        SubTask = args.data;
    };

    $scope.GEESaveMaster = function () {
        try {
            $scope.GEE$broadcast('show-errors-check-validity');
            if ($scope.GEEmodelForm.$valid) {
                $http({
                    method: 'POST',
                    data: { taskmaster: $scope.GEEmodel },
                    url: $scope.GEEpath + "SaveMaster"
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        ShowResult(response.data.Message, 'success');
                        $scope.GEEMasterOrderId = response.data.Id;
                        $scope.GEEgetData();
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.GEEDelete = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.GEEpath + "Delete?id=" + $scope.GEEMasterOrderId
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GEECancel();
                    $scope.GEEgetData();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GEECancel = function () {

        $scope.GEEmodel = Object.assign({}, $scope.GEEmodelMain);
        $scope.GEEganttdata = [];
        $scope.GEEganttPredecessorData = [];
        $scope.GEEGetSequence();
    }


    //search task master
    //search
    $scope.GEEmodelFilterByListSearchTask = [
        { value: 'Id', name: 'Id ' },
        { value: 'Sequence', name: 'Seq ' },
        { value: 'Code', name: 'Code ' },
        { value: 'TaskDescription', name: 'Task Desc ' },
        { value: 'TaskCategory', name: 'Task Category ' },
        { value: 'Department', name: 'Department ' },
        { value: 'UserDefineTask', name: 'User DefineTask ' },
        { value: 'Process', name: 'Process ' },
        { value: 'TaskAppliedOn', name: 'Task Applied On ' },
        { value: 'TaskType', name: 'Task Type ' }
    ];
    $scope.GEETaskMasterList = [];
    $scope.GEEsearchColTask = "TaskDescription";
    $scope.GEEsearchValTask = "";
    $scope.GEEgetTaskData = function () {
        try {
            if (angular.isUndefinedOrNull($scope.GEEMasterOrderId))
                throw 'Save Task Template First';

            $("#dialogSearchTask").data("ejDialog").open();
            $http({
                method: 'GET',
                data: { 'parameters': null },
                url: $scope.GEEpath + "SearchTaskMaster?column=" + $scope.GEEsearchColTask + "&value=" + $scope.GEEsearchValTask
            }).then(function successCallback(response) {
                $scope.GEETaskMasterList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



    };

    $scope.GEETaskSearchSelectedData = function (args) {
        $("#dialogSearchTask").data("ejDialog").close();
        $scope.GEECopyData(args.data.Id);

    };
    $scope.GEECopyData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.GEEpath + "CopyTask?TaskId=" + Id + "&TemplateMasterId=" + $scope.GEEMasterOrderId
        }).then(function successCallback(response) {
            if (response.data.Error == false) {
                $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);
            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    //gantt chart area
    $scope.GEEeditSettings = {
        allowEditing: true,
        allowAdding: true,
        allowDeleting: true,
        showDeleteConfirmDialog: true,
        allowIndent: true,
        editMode: "cellEditing"
    };
    $scope.GEEtoolclick = function (args) {
        if (args.itemName == "Add Task")
            $scope.GEEgetTaskData();

    }
    $scope.GEEtoolbarSettings = {
        showToolbar: true,

        toolbarItems: [
            //ej.Gantt.ToolbarItems.Add,
            ej.Gantt.ToolbarItems.Edit,
            ej.Gantt.ToolbarItems.Delete,
            ej.Gantt.ToolbarItems.Update,
            ej.Gantt.ToolbarItems.Cancel,
            //ej.Gantt.ToolbarItems.Indent,
            //ej.Gantt.ToolbarItems.Outdent,
            //ej.Gantt.ToolbarItems.ExpandAll,
            //ej.Gantt.ToolbarItems.CollapseAll
        ],
        customToolbarItems: [{
            text: "", tooltipText: "Add Task", templateID: "#AddNewIcon"
        }]
    };

    $scope.GEEcolumnTemplateResource = [{ resourceId: 0, resourceName: "", EmpPicPath: '' }];
    $scope.GEEganttdata = [];
    $scope.GEEganttPredecessorData = [];
    $scope.GEEgenerateGantt = function () {


        try {
            $scope.GEEdisplayprop = "block;";


            $scope.GEEganttdata = [];
            var ganttdata = [];
            $scope.GEEganttPredecessorData = [];
            for (var i = 0; i < $scope.GEETaskMasters.length; i++) {
                var _data = {
                    taskID: $scope.GEETaskMasters[i].Id,
                    taskName: $scope.GEETaskMasters[i].UserDefineTask,
                    startDate: new Date($scope.GEETaskMasters[i].startDate),
                    endDate: new Date("01/10/2017"),
                    predecessor: $scope.GEETaskMasters[i].predecessor,
                    IsTaskMilestone: $scope.GEETaskMasters[i].IsTaskMilestone,
                    IsFirstTask: $scope.GEETaskMasters[i].IsFirstTask,
                    IsLastTask: $scope.GEETaskMasters[i].IsLastTask,
                    IsMandatory: $scope.GEETaskMasters[i].IsMandatory,
                    TaskAppliedOnEnum: $scope.GEETaskMasters[i].TaskAppliedOnEnum,
                    offset: '',
                    duration: $scope.GEETaskMasters[i].Duration,
                    progress: "0",
                    resourceId: []
                }
                if (angular.isUndefinedOrNull($scope.GEETaskMasters[i].resourceId) == false)
                    _data.resourceId.push(parseInt($scope.GEETaskMasters[i].resourceId));

                ganttdata.push(_data);

                if ($scope.GEETaskMasters[i].RepeatTask == false)
                    $scope.GEEganttPredecessorData.push(_data);
            }


            $scope.GEEganttdata = ganttdata;

        } catch (e) {

        }


    }
    $scope.GEEgenerateGanttResource = function () {



        $scope.GEEdisplayprop = "block;";

        //$scope.GEEcolumnTemplateResource = [];
        var columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
        var tempstring = [];
        //resourceId,resourceName
        for (var i = 0; i < $scope.GEEcolumnTemplateResource.length; i++) {
            tempstring.push(parseInt($scope.GEEcolumnTemplateResource[i].resourceId));
        }
        for (var i = 0; i < $scope.GEETaskMasters.length; i++) {
            columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
            try {
                if (angular.isUndefinedOrNull($scope.GEETaskMasters[i].resourceId) == false) {
                    if (tempstring.includes(parseInt($scope.GEETaskMasters[i].resourceId)) == false) {

                        columnTemplateResourceData.resourceId = parseInt($scope.GEETaskMasters[i].resourceId);
                        columnTemplateResourceData.resourceName = $scope.GEETaskMasters[i].resourceName;
                        columnTemplateResourceData.EmpPicPath = $scope.GEETaskMasters[i].EmpPicPath;

                        $scope.GEEcolumnTemplateResource.push(columnTemplateResourceData);

                        tempstring.push(parseInt($scope.GEETaskMasters[i].resourceId));
                    }
                }
            } catch (e) {

            }

        }



    }
    $scope.GEEUpdateLoggedTnA = function () {

        $http({
            method: 'POST',
            url: $scope.GEEpath + 'GenerateTnALog',
            data: { MasterOrderId: $scope.GEEMasterOrderId },
            dataType: 'JSON'
        }).then(function successCallback(response) {

        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    $scope.GEEganttedit = function (args) {
        var ganttObj = $("#angularganttForEdit").data("ejGantt");
    }
    $scope.GEEganttactionComplete = function (args) {

        //$scope.GEEUpdateFullTaskDuration();

    }

    $scope.GEEganttactionbegin = function (args) {

        $scope.GEEUpdateFullTaskDuration();
        if (args.requestType == "beforeOpenAddDialog") {

        }
        if (args.requestType == "OpenAddDialog") {

        }
        if (args.requestType == "validateLinkedTask") {

        }

        if (args.requestType == "beforeOpenEditDialog") {
            $scope.GEEEditMasterOrderTaskTemplate(args.data.item.taskID);
            $("#tabEdit").data("ejTab").showItem(0);
            args.cancel = true;
        }
        if (args.requestType == "delete") {
            $scope.GEEDeleteTask(args.data.item.taskID);
        }

    }

    $scope.GEEtaskbarTooltipTemplateId = "tooltipTemplate";
    $scope.GEEganttquerycellinfo = function (args) {
        var ganttObj = $("#angularganttForEdit").data("ejGantt");
        var data = ganttObj.model.dataSource();
        try {
            var col = this.getColumns();


            col[0].visible = false;
            col[1].width = "200px";
            col[2].visible = false;
            col[3].visible = false;
            col[4].visible = false;
            col[5].width = "70px";
            col[6].visible = false;
            col[7].visible = false;
            col[8].visible = false;

            ganttObj.ejGantt("setSplitterPosition", "280px");

        } catch (e) {

        }
    }
    $scope.GEEganttqueryTaskbarInfo = function (args) {
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
        if (args.data.item.IsTaskMilestone == true) {
            args.taskbarBorder = "fuchsia";
        }
        if (args.data.item.IsFirstTask == true) {
            args.taskbarBorder = "red";
        }
        if (args.data.item.IsLastTask == true) {
            args.taskbarBorder = "#F5EE00";
        }
    }
    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    $scope.GEEContextMenuOpen = function (args) {
        args.contextMenuItems = [];
        args.contextMenuItems.push({
            headerText: "Edit Task",
            menuId: "EditTask",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.GEEEditMasterOrderTaskTemplate(args.data.item.taskID);
                $("#tabEdit").data("ejTab").showItem(0);

            }
        });

        args.contextMenuItems.push({
            headerText: "Edit Sub Task",
            menuId: "EditSubTask",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.GEEEditMasterOrderTaskTemplate(args.data.item.taskID);
                $("#tabEdit").data("ejTab").showItem(1);

            }
        });

        args.contextMenuItems.push({
            headerText: "Edit Dependency",
            menuId: "EditDependency",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.GEEEditMasterOrderTaskTemplate(args.data.item.taskID);
                $("#tabEdit").data("ejTab").showItem(2);

            }
        });

        //args.contextMenuItems.push({
        //    headerText: "Delete Task",
        //    menuId: "DeleteTask",
        //    iconPath: "url(Navigation-Up-02-WF.png)",
        //    eventHandler: function () {
        //        //event handler for custom menu items
        //    }
        //});
    }


    ///////edit tasks
    $scope.GEESingleMasterOrderTaskTemplate = {};
    $scope.GEEpredecessorTypeddl = [{ predecessorType: "FS", predecessorName: "Finish-Start" }, { predecessorType: "SS", predecessorName: "Start-Start" },
    { predecessorType: "SF", predecessorName: "Start-Finish" }, { predecessorType: "FF", predecessorName: "Finish-Finish" }]
    $scope.GEEDependency = { fromTaskId: null, toTaskId: null, predecessorType: null, offset: 0 }
    $scope.GEEDependencyList = [];

    $scope.GEEEditingTaskTemplateId = '';
    $scope.GEEEditMasterOrderTaskTemplate = function (id) {
        $scope.GEESubTaskList = [];
        $scope.GEEDependencyList = [];
        $scope.GEEEditingTaskTemplateId = id;
        $("#dialogEditTask").data("ejDialog").open();


        $http({
            method: 'GET',
            url: $scope.GEEpath + "GetSingleMasterOrderTaskTemplate?Id=" + id
        }).then(function successCallback(response) {
            try {
                $scope.GEESingleMasterOrderTaskTemplate = response.data.Task[0];
                $scope.GEESubTaskList = response.data.SubTasks;
                $scope.GEEDependencyList = response.data.DependencyList;
                $scope.GEEtempdateid = $scope.GEESingleMasterOrderTaskTemplate.TaskDependentDatesId;

                $("#GEEgridPredecessor").ejGrid("instance").refreshContent();
                $("#GEEgridSubTask").ejGrid("instance").refreshContent();

                $scope.GEEhideNewOrRepeatOrderCheckbox = false;
                if ($scope.GEESingleMasterOrderTaskTemplate.TaskDependentOn == 'MasterOrder') {
                    $scope.GEEhideNewOrRepeatOrderCheckbox = true;
                    $scope.GEESingleMasterOrderTaskTemplate.ForNewOrder = false;
                }
                $scope.GEELoadDependentDates(null);
            } catch (e) {

            }

        });

        $("#tabEdit").ejTab({ headerSize: "30px" });
    }


    $scope.GEEUpdateTask = function (model) {

        try {
            if (angular.isUndefinedOrNull($scope.GEESingleMasterOrderTaskTemplate.TaskDescription))
                throw 'Enter task description';

            if ($scope.GEESingleMasterOrderTaskTemplate.ResponsiblePersonCategory == "Employee")
                if (angular.isUndefinedOrNull($scope.GEESingleMasterOrderTaskTemplate.EmployeeId))
                    throw 'Select Employee';


            $http({
                method: 'POST',
                data: { MasterOrderTaskTemplate: $scope.GEESingleMasterOrderTaskTemplate },
                url: $scope.GEEpath + "UpdateTask"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $("#dialogEditTask").data("ejDialog").close();
                    ShowResult(response.data.Message, 'success');

                    $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GEEhideNewOrRepeatOrderCheckbox = false;
    $scope.GEELoadDependentDates = function (args) {
        $http({
            method: 'GET',
            url: $scope.GEEpathTaskMaster + "GetDependentDateData?dependon=" + $scope.GEESingleMasterOrderTaskTemplate.TaskAppliedOnId
        }).then(function successCallback(response) {
            $scope.GEETaskDependentDateList = response.data.TaskDependentDates;
            $scope.GEESingleMasterOrderTaskTemplate.TaskDependentDatesId = $scope.GEEtempdateid;

            $scope.GEEhideNewOrRepeatOrderCheckbox = false;
            if (response.data.TaskDependentDates[0].TaskDependentOn == 'MasterOrder') {
                $scope.GEEhideNewOrRepeatOrderCheckbox = true;
                $scope.GEESingleMasterOrderTaskTemplate.ForNewOrder = false;
            }
        });
    }
    $scope.GEEtempdateid = null;
    $scope.GEESelectDependentDates = function (args) {

        try {
            $scope.GEESingleMasterOrderTaskTemplate.TaskDependentDatesId = $scope.GEEtempdateid;
            var DropDownListObj = $("#GEETaskDependentDateList").data("ejDropDownList");
            DropDownListObj.selectItemByValue($scope.GEEtempdateid);
        } catch (e) {

        }

    }

    $scope.GEEUpdateTaskDuration = function (model) {

        try {

            //$scope.GEEUpdateFullTaskDuration();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.GEEUpdateFullTaskDuration = function () {

        try {
            var _data = $("#angularganttForEdit").data("ejGantt").model.dataSource();
            $http({
                method: 'POST',
                data: {
                    TaskTemplateIds: _data,
                    MasterOrderId: $scope.GEEMasterOrderId
                },
                url: $scope.GEEpath + "UpdateFullTaskDuration"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GEEDeleteTask = function (Id) {

        try {

            $http({
                method: 'POST',
                data: { id: Id },
                url: $scope.GEEpath + "DeleteMasterOrderTaskTemplate"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.GEESaveSubTask = function (model) {

        try {

            $http({
                method: 'POST',
                data: { TaskTemplateId: $scope.GEEEditingTaskTemplateId, MasterOrderTaskTemplate: model.data },
                url: $scope.GEEpath + "SaveSubTasks"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');

                    $scope.GEEEditMasterOrderTaskTemplate($scope.GEEEditingTaskTemplateId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.GEEDeleteSubTask = function (model) {

        try {

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: $scope.GEEpath + "DeleteSubTask"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.GEESaveMasterOrderTaskTemplateDependency = function (model) {

        try {

            var gridObj = $("#GEEgridPredecessor").ejGrid("instance");
            var _PreData = gridObj.model.dataSource();

            var TaskTemplateId = $scope.GEEEditingTaskTemplateId;
            $http({
                method: 'POST',
                data: { TaskTemplateId: TaskTemplateId, MasterOrderTaskTemplate: model.data },
                url: $scope.GEEpath + "SaveMasterOrderTaskTemplateDependency"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');

                    $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);

                    $scope.GEEEditMasterOrderTaskTemplate(TaskTemplateId);

                }
                else {
                    ShowResult(response.data.Message, 'failure');

                    $scope.GEEDependencyList = _PreData;
                    gridObj.refreshContent();
                    model.cancel = true;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.GEEDependencyList = _PreData;
            gridObj.refreshContent();
        }
    }
    $scope.GEEDeleteMasterOrderTaskTemplateDependency = function (model) {

        try {
            var gridObj = $("#GEEgridPredecessor").ejGrid("instance");
            var _PreData = gridObj.model.dataSource();
            var TaskTemplateId = $scope.GEEEditingTaskTemplateId;

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: $scope.GEEpath + "DeleteMasterOrderTaskTemplateDependency"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');


                    //$scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);
                    $scope.GEEEditMasterOrderTaskTemplate(TaskTemplateId);
                    $scope.GEEGetSelectedTasks($scope.GEEMasterOrderId);

                }
                else {
                    $scope.GEEDependencyList = _PreData;
                    gridObj.refreshContent();
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.GEEDependencyList = _PreData;
            gridObj.refreshContent();
        }
    }



    //search employee
    $scope.GEEWhereEmployeeNeeded = '';
    $scope.GEEEmployeeList = [];
    $scope.GEEOpenEmployeeSearchBox = function (WhereEmployeeNeeded) {
        $scope.GEEWhereEmployeeNeeded = WhereEmployeeNeeded;
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.open();

        $scope.GEEgetEmployeeData();
    }
    $scope.GEEgetEmployeeData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.GEEEmployeeSearchCol, 'value': $scope.GEEEmployeeSearchVal },
                url: $scope.GEEpath + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.GEEEmployeeList = response.data;

            });
        } catch (e) {

        }
    }

    $scope.GEEViewEmployeeStatus = function (args) {

        try {
            $scope.GEEGetSingleEmployee(args.data.Id);
        } catch (e) {

        }
    }
    $scope.GEEGetSingleEmployee = function (Id) {
        try {
            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'Id': Id },
                url: $scope.GEEpath + 'GetSingleEmployee'

            }).then(function successCallback(response) {

                if ($scope.GEEWhereEmployeeNeeded == "MASTER") {
                    $scope.GEEmodel.EmployeeId = response.data[0].SystemId;
                    $scope.GEEmodel.EmployeeCode = response.data[0].EmployeeCode;
                    $scope.GEEmodel.EmployeeName = response.data[0].EmployeeName;
                }
                else if ($scope.GEEWhereEmployeeNeeded == "TASK") {
                    $scope.GEESingleMasterOrderTaskTemplate.EmployeeId = response.data[0].SystemId;
                    $scope.GEESingleMasterOrderTaskTemplate.EmployeeCode = response.data[0].EmployeeCode;
                    $scope.GEESingleMasterOrderTaskTemplate.EmployeeName = response.data[0].EmployeeName;

                }

            });
        } catch (e) {

        }
    }
}