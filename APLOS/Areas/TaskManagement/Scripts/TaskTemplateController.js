'use strict';
TaskTemplateController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function TaskTemplateController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {
    $scope.displayprop = "none;";
    $scope.path = 'TaskManagement/TaskTemplate/';
    $scope.pathTaskMaster = 'TaskManagement/TaskMasterCreation/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.modelMain = {
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

    $scope.TaskCategoryList = [];
    $scope.TaskAppliedOnList = [];
    $scope.TaskDependentDateList = [];
    $scope.TaskTypeList = ["Normal", "Important", "Critical"];
    $scope.ResponsiblePersonCategoryList = ["Buyer", "Entity", "Employee"];

    $scope.model = Object.assign({}, $scope.modelMain);
    $scope.TaskMasters = [];
    $scope.SubTaskList = [];
    $scope.TaskTemplate = [];

    $scope.modelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'Code', name: 'Code ' },
        { value: 'UserName', name: 'Template Name ' },
        { value: 'ShortName', name: 'Short Name ' },
        { value: 'StandardName', name: 'Standard Name ' },
        { value: 'Remarks', name: 'Remarks ' }
    ];
    $scope.EmployeemodelFilterByList = [
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
        url: $scope.pathTaskMaster + "GetMasterData"
    }).then(function successCallback(response) {
        $scope.TaskCategoryList = response.data.TaskCategory;
        $scope.TaskAppliedOnList = response.data.TaskAppliedOn;

    });

    $scope.hideNewOrRepeatOrderCheckbox = false;


    $scope.searchCol = "UserName";
    $scope.searchVal = "";
    $scope.EmployeeSearchCol = "EmployeeName";
    $scope.EmployeeSearchVal = "";
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.TaskTemplate = response.data;
        });


    }; $scope.getData();
    $scope.Get = function (args) {

        $scope.LoadData(args.data.Id);
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    $scope.LoadData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + "Get?Id=" + Id
        }).then(function successCallback(response) {
            $scope.model = response.data.master[0];
            $scope.GetSelectedTasks();

        });
    }
    $scope.GetSelectedTasks = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetSelectedTaskList?TemplateId=" + $scope.model.Id
        }).then(function successCallback(response) {

            $scope.SubTaskList = [];
            $scope.DependencyList = [];
            $scope.SingleTaskTemplate = {};

            $scope.TaskMasters = response.data;
            $scope.generateGanttResource();

            $scope.GetSelectedTasks2();
        });
    }
    $scope.GetSelectedTasks2 = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetSelectedTaskList?TemplateId=" + $scope.model.Id
        }).then(function successCallback(response) {


            $scope.generateGantt();


        });
    }
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.model.Sequence = data;
        });
    };
    $scope.GetSequence();
    $scope.SubTask = {};
    $scope.EditSubTask = function (args) {

        SubTask = args.data;
    };

    $scope.SaveMaster = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelForm.$valid) {
                $http({
                    method: 'POST',
                    data: { taskmaster: $scope.model },
                    url: $scope.path + "SaveMaster"
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        ShowResult(response.data.Message, 'success');
                        $scope.model.Id = response.data.Id;
                        $scope.getData();
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
    $scope.Delete = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + "Delete?id=" + $scope.model.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Cancel();
                    $scope.getData();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Cancel = function () {

        $scope.model = Object.assign({}, $scope.modelMain);
        $scope.ganttdata = [];
        $scope.ganttPredecessorData = [];
        $scope.GetSequence();
    }


    //search task master
    //search
    $scope.modelFilterByListSearchTask = [
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
    $scope.TaskMasterList = [];
    $scope.searchColTask = "TaskDescription";
    $scope.searchValTask = "";
    $scope.getTaskData = function () {
        try {
            if (angular.isUndefinedOrNull($scope.model.Id))
                throw 'Save Task Template First';

            $("#dialogSearchTask").data("ejDialog").open();
            $http({
                method: 'GET',
                data: { 'parameters': null },
                url: $scope.path + "SearchTaskMaster?column=" + $scope.searchColTask + "&value=" + $scope.searchValTask
            }).then(function successCallback(response) {
                $scope.TaskMasterList = response.data;
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }



    };

    $scope.TaskSearchSelectedData = function (args) {
        $("#dialogSearchTask").data("ejDialog").close();
        $scope.CopyData(args.data.Id);

    };
    $scope.CopyData = function (Id) {
        $http({
            method: 'POST',
            url: $scope.path + "CopyTask?TaskId=" + Id + "&TemplateMasterId=" + $scope.model.Id
        }).then(function successCallback(response) {

            if (response.data.Error == false) {
                $scope.LoadData($scope.model.Id);
            }
            else {
                ShowResult(response.data.Message, 'failure');
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }


    //gantt chart area
    $scope.editSettings = {
        allowEditing: true,
        allowAdding: true,
        allowDeleting: true,
        showDeleteConfirmDialog: true,
        allowIndent: true,
        editMode: "cellEditing"
    };
    $scope.toolclick = function (args) {
        if (args.itemName == "Add Task")
            $scope.getTaskData();

    }
    $scope.toolbarSettings = {
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

    $scope.columnTemplateResource = [{ resourceId: 0, resourceName: "", EmpPicPath: '' }];
    $scope.ganttdata = [];
    $scope.ganttPredecessorData = [];
    $scope.generateGantt = function () {



        $scope.displayprop = "block;";


        $scope.ganttdata = [];
        $scope.ganttPredecessorData = [];
        for (var i = 0; i < $scope.TaskMasters.length; i++) {
            var _data = {
                taskID: $scope.TaskMasters[i].Id,
                taskName: $scope.TaskMasters[i].UserDefineTask,
                startDate: new Date($scope.TaskMasters[i].startDate),
                endDate: new Date("01/10/2017"),
                predecessor: $scope.TaskMasters[i].predecessor,
                IsTaskMilestone: $scope.TaskMasters[i].IsTaskMilestone,
                IsFirstTask: $scope.TaskMasters[i].IsFirstTask,
                IsLastTask: $scope.TaskMasters[i].IsLastTask,
                IsMandatory: $scope.TaskMasters[i].IsMandatory,
                TaskAppliedOnEnum: $scope.TaskMasters[i].TaskAppliedOnEnum,
                Active: $scope.TaskMasters[i].Active,
                offset: '',
                duration: $scope.TaskMasters[i].Duration,
                progress: "0",
                resourceId: []
            }
            if (angular.isUndefinedOrNull($scope.TaskMasters[i].resourceId) == false)
                _data.resourceId.push(parseInt($scope.TaskMasters[i].resourceId));

            $scope.ganttdata.push(_data);

            if ($scope.TaskMasters[i].RepeatTask == false)
                $scope.ganttPredecessorData.push(_data);
        }

    }
    $scope.generateGanttResource = function () {



        $scope.displayprop = "block;";

        //$scope.columnTemplateResource = [];
        var columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
        var tempstring = [];
        //resourceId,resourceName
        for (var i = 0; i < $scope.columnTemplateResource.length; i++) {
            tempstring.push(parseInt($scope.columnTemplateResource[i].resourceId));
        }
        for (var i = 0; i < $scope.TaskMasters.length; i++) {
            columnTemplateResourceData = { resourceId: 0, resourceName: "", EmpPicPath: '' };
            try {
                if (angular.isUndefinedOrNull($scope.TaskMasters[i].resourceId) == false) {
                    if (tempstring.includes(parseInt($scope.TaskMasters[i].resourceId)) == false) {

                        columnTemplateResourceData.resourceId = parseInt($scope.TaskMasters[i].resourceId);
                        columnTemplateResourceData.resourceName = $scope.TaskMasters[i].resourceName;
                        columnTemplateResourceData.EmpPicPath = $scope.TaskMasters[i].EmpPicPath;

                        $scope.columnTemplateResource.push(columnTemplateResourceData);

                        tempstring.push(parseInt($scope.TaskMasters[i].resourceId));
                    }
                }
            } catch (e) {

            }

        }



    }

    $scope.ganttedit = function (args) {
        var ganttObj = $("#angulargantt").data("ejGantt");
    }
    $scope.ganttactionComplete = function (args) {

        //$scope.UpdateFullTaskDuration();

    }

    $scope.ganttactionbegin = function (args) {

        $scope.UpdateFullTaskDuration();
        if (args.requestType == "beforeOpenAddDialog") {

        }
        if (args.requestType == "OpenAddDialog") {

        }
        if (args.requestType == "validateLinkedTask") {

        }

        if (args.requestType == "beforeOpenEditDialog") {
            $scope.EditTaskTemplate(args.data.item.taskID);
            $("#tabEdit").data("ejTab").showItem(0);
            args.cancel = true;
        }
        if (args.requestType == "delete") {
            $scope.DeleteTask(args.data.item.taskID);
        }

    }

    $scope.taskbarTooltipTemplateId = "tooltipTemplate";
    $scope.ganttquerycellinfo = function (args) {
        var ganttObj = $("#angulargantt").data("ejGantt");
        var data = ganttObj.model.dataSource();
        try {
            var col = this.getColumns();


            col[0].width = "60px";
            col[1].width = "200px";
            col[2].visible = false;
            col[3].visible = false;
            col[4].visible = false;
            col[5].width = "70px";
            col[6].visible = false;
            col[7].visible = false;
            col[8].visible = false;

            ganttObj.ejGantt("setSplitterPosition", "330px");

        } catch (e) {

        }
    }
    $scope.ganttqueryTaskbarInfo = function (args) {
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
    $scope.ContextMenuOpen = function (args) {
        args.contextMenuItems = [];
        args.contextMenuItems.push({
            headerText: "Edit Task",
            menuId: "EditTask",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.EditTaskTemplate(args.data.item.taskID);
                $("#tabEdit").data("ejTab").showItem(0);

            }
        });

        args.contextMenuItems.push({
            headerText: "Edit Sub Task",
            menuId: "EditSubTask",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.EditTaskTemplate(args.data.item.taskID);
                $("#tabEdit").data("ejTab").showItem(1);

            }
        });

        args.contextMenuItems.push({
            headerText: "Edit Dependency",
            menuId: "EditDependency",
            iconPath: "url(Navigation-Up-02-WF.png)",
            eventHandler: function (args) {
                $scope.EditTaskTemplate(args.data.item.taskID);
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
    $scope.SingleTaskTemplate = {};
    $scope.predecessorTypeddl = [{ predecessorType: "FS", predecessorName: "Finish-Start" }, { predecessorType: "SS", predecessorName: "Start-Start" },
    { predecessorType: "SF", predecessorName: "Start-Finish" }, { predecessorType: "FF", predecessorName: "Finish-Finish" }]
    $scope.Dependency = { fromTaskId: null, toTaskId: null, predecessorType: null, offset: 0 }
    $scope.DependencyList = [];

    $scope.EditingTaskTemplateId = '';
    $scope.EditTaskTemplate = function (id) {
        $scope.SubTaskList = [];
        $scope.DependencyList = [];
        $scope.EditingTaskTemplateId = id;
        $("#dialogEditTask").data("ejDialog").open();


        $http({
            method: 'GET',
            url: $scope.path + "GetSingleTaskTemplate?Id=" + id
        }).then(function successCallback(response) {
            try {
                $scope.SingleTaskTemplate = response.data.Task[0];
                $scope.SubTaskList = response.data.SubTasks;
                $scope.DependencyList = response.data.DependencyList;
                $scope.tempdateid = $scope.SingleTaskTemplate.TaskDependentDatesId;
                $("#gridPredecessor").ejGrid("instance").refreshContent();
                $("#gridSubTask").ejGrid("instance").refreshContent();


                $scope.hideNewOrRepeatOrderCheckbox = false;
                if ($scope.model.TaskDependentOn == 'MasterOrder') {
                    $scope.hideNewOrRepeatOrderCheckbox = true;
                    $scope.model.ForNewOrder = false;
                }
                $scope.LoadDependentDates(null);
            } catch (e) {

            }

        });

        $("#tabEdit").ejTab({ headerSize: "30px" });
    }
    $scope.tempdateid = null;
    $scope.LoadDependentDates = function (args) {
        $http({
            method: 'GET',
            url: $scope.pathTaskMaster + "GetDependentDateData?dependon=" + $scope.SingleTaskTemplate.TaskAppliedOnId
        }).then(function successCallback(response) {
            $scope.TaskDependentDateList = response.data.TaskDependentDates;
            $scope.SingleTaskTemplate.TaskDependentDatesId = $scope.tempdateid;
            for (var i = 0; i < $scope.TaskDependentDateList.length; i++) {
                if ($scope.SingleTaskTemplate.TaskDependentDatesId == $scope.TaskDependentDateList[i].Id) {
                    $scope.SingleTaskTemplate.TaskDependentDatesId = $scope.TaskDependentDateList[i].Id;
                    break;
                }
            }

            $scope.hideNewOrRepeatOrderCheckbox = false;
            if (response.data.TaskDependentDates[0].TaskDependentOn == 'MasterOrder') {
                $scope.hideNewOrRepeatOrderCheckbox = true;
                $scope.model.ForNewOrder = false;
            }
        });
    }

    $scope.SelectDependentDates = function (args) {

        try {
            $scope.SingleTaskTemplate.TaskDependentDatesId = $scope.tempdateid;
            var DropDownListObj = $("#TaskDependentDateList").data("ejDropDownList");
            DropDownListObj.selectItemByValue($scope.tempdateid);
        } catch (e) {

        }

    }


    $scope.UpdateTask = function (model) {

        try {
            if (angular.isUndefinedOrNull($scope.SingleTaskTemplate.TaskDescription))
                throw 'Enter task description';

            if ($scope.SingleTaskTemplate.ResponsiblePersonCategory == "Employee")
                if (angular.isUndefinedOrNull($scope.SingleTaskTemplate.EmployeeId))
                    throw 'Select Employee';


            $http({
                method: 'POST',
                data: { taskTemplate: $scope.SingleTaskTemplate },
                url: $scope.path + "UpdateTask"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    $("#dialogEditTask").data("ejDialog").close();
                    ShowResult(response.data.Message, 'success');

                    $scope.LoadData($scope.model.Id);

                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.UpdateTaskDuration = function (model) {

        try {

            //$scope.UpdateFullTaskDuration();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.UpdateFullTaskDuration = function () {

        try {
            var _data = $("#angulargantt").data("ejGantt").model.dataSource();
            $http({
                method: 'POST',
                data: {
                    taskTemplateids: _data,
                    TaskTemplateMasterId: $scope.model.Id
                },
                url: $scope.path + "UpdateFullTaskDuration"
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

    $scope.DeleteTask = function (Id) {

        try {

            $http({
                method: 'POST',
                data: { id: Id },
                url: $scope.path + "DeleteTaskTemplate"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadData($scope.model.Id);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveSubTask = function (model) {

        try {

            $http({
                method: 'POST',
                data: { TaskTemplateId: $scope.EditingTaskTemplateId, taskTemplate: model.data },
                url: $scope.path + "SaveSubTasks"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');

                    $scope.EditTaskTemplate($scope.EditingTaskTemplateId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.DeleteSubTask = function (model) {

        try {

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: $scope.path + "DeleteSubTask"
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


    $scope.SaveTaskTemplateDependency = function (model) {

        try {

            var gridObj = $("#gridPredecessor").ejGrid("instance");
            var _PreData = gridObj.model.dataSource();

            var tasktemplateid = $scope.EditingTaskTemplateId;
            $http({
                method: 'POST',
                data: { TaskTemplateId: tasktemplateid, taskTemplate: model.data },
                url: $scope.path + "SaveTaskTemplateDependency"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');

                    $scope.LoadData($scope.model.Id);

                    $scope.EditTaskTemplate(tasktemplateid);

                }
                else {
                    ShowResult(response.data.Message, 'failure');

                    $scope.DependencyList = _PreData;
                    gridObj.refreshContent();
                    model.cancel = true;
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.DependencyList = _PreData;
            gridObj.refreshContent();
        }
    }
    $scope.DeleteTaskTemplateDependency = function (model) {

        try {
            var gridObj = $("#gridPredecessor").ejGrid("instance");
            var _PreData = gridObj.model.dataSource();
            var tasktemplateid = $scope.EditingTaskTemplateId;

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: $scope.path + "DeleteTaskTemplateDependency"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    //ShowResult(response.data.Message, 'success');


                    //$scope.GetSelectedTasks();
                    $scope.EditTaskTemplate(tasktemplateid);
                    $scope.LoadData($scope.model.Id);

                }
                else {
                    $scope.DependencyList = _PreData;
                    gridObj.refreshContent();
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
            $scope.DependencyList = _PreData;
            gridObj.refreshContent();
        }
    }



    //search employee
    $scope.WhereEmployeeNeeded = '';
    $scope.EmployeeList = [];
    $scope.OpenEmployeeSearchBox = function (WhereEmployeeNeeded) {
        $scope.WhereEmployeeNeeded = WhereEmployeeNeeded;
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        eDialog.open();

        $scope.getEmployeeData();
    }
    $scope.getEmployeeData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal },
                url: $scope.path + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }

    $scope.ViewEmployeeStatus = function (args) {

        try {
            $scope.GetSingleEmployee(args.data.Id);
        } catch (e) {

        }
    }
    $scope.GetSingleEmployee = function (Id) {
        try {
            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();

            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'Id': Id },
                url: $scope.path + 'GetSingleEmployee'

            }).then(function successCallback(response) {

                if ($scope.WhereEmployeeNeeded == "MASTER") {
                    $scope.model.EmployeeId = response.data[0].SystemId;
                    $scope.model.EmployeeCode = response.data[0].EmployeeCode;
                    $scope.model.EmployeeName = response.data[0].EmployeeName;
                }
                else if ($scope.WhereEmployeeNeeded == "TASK") {
                    $scope.SingleTaskTemplate.EmployeeId = response.data[0].SystemId;
                    $scope.SingleTaskTemplate.EmployeeCode = response.data[0].EmployeeCode;
                    $scope.SingleTaskTemplate.EmployeeName = response.data[0].EmployeeName;

                }

            });
        } catch (e) {

        }
    }
}