'use strict';
TaskMasterCreationController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$filter", "$window", "$http"];
function TaskMasterCreationController(cboService, commonMessage, $scope, $rootScope, baseService, $filter, $window, $http) {

    $scope.path = 'TaskManagement/TaskMasterCreation/';
    $scope.TaskCategoryList = [];
    $scope.TaskSubCategoryList = [];
    $scope.DepartmentList = [];
    $scope.ProcessList = [];
    $scope.TaskAppliedOnList = [];
    $scope.TaskDependentDateList = [];
    $scope.TaskTypeList = ["Normal", "Important", "Critical"];
    $scope.ResponsiblePersonCategoryList = ["Buyer", "Entity", "Employee"];
    $scope.PlantList = [];
    $scope.SubTaskList = [];
    $scope.TaskMasterList = [];
    //search
    $scope.modelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'Sequence', name: 'Seq ' },
        { value: 'Code', name: 'Code ' },
        { value: 'TaskDescription', name: 'Task Desc ' },
        { value: 'TaskCategory', name: 'Task Category ' },
        { value: 'Department', name: 'Department ' },
        { value: 'UserDefineTask', name: 'User DefineTask ' },
        { value: 'Process', name: 'Process ' },
        { value: 'TaskType', name: 'Task Type ' }
    ];

    $scope.create = function (args) {
        $("#checkBox").ejCheckBox({
            change: function (args) {
                var obj = $("#ddlPlantList").ejDropDownList("instance");
                if (args.isChecked) obj.checkAll();
                else obj.uncheckAll();
            },
            text: "Select All",
            cssClass: "ddlSelectAllCheckBox"
        });

    };


    $scope.searchCol = "TaskDescription";
    $scope.searchVal = "";
    $scope.getData = function () {

        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.TaskMasterList = response.data;
        });


    };
    $http({
        method: 'GET',
        url: $scope.path + "GetMasterData"
    }).then(function successCallback(response) {
        $scope.TaskCategoryList = response.data.TaskCategory;
        $scope.TaskSubCategoryList = response.data.TaskSubCategory;
        $scope.DepartmentList = response.data.Department;
        $scope.ProcessList = response.data.Process;
        $scope.TaskAppliedOnList = response.data.TaskAppliedOn;
        $scope.PlantList = response.data.Plant;

    });
    $scope.getData();

    $scope.hideNewOrRepeatOrderCheckbox = true;
    $scope.LoadDependentDates = function (args) {
        $http({
            method: 'GET',
            url: $scope.path + "GetDependentDateData?dependon=" + $scope.model.TaskAppliedOnId
        }).then(function successCallback(response) {
            $scope.TaskDependentDateList = response.data.TaskDependentDates;
            $scope.tempdateid = $scope.model.TaskDependentDatesId;

            $scope.hideNewOrRepeatOrderCheckbox = false;
            if (response.data.TaskDependentDates[0].TaskDependentOn == 'MasterOrder') {
                $scope.hideNewOrRepeatOrderCheckbox = true;
                $scope.model.ForNewOrder = false;
            }

        });
    }
    $scope.tempdateid = null;
    $scope.SelectDependentDates = function (args) {

        try {
            $scope.model.TaskDependentDatesId = $scope.tempdateid;
            var DropDownListObj = $("#TaskDependentDateList").data("ejDropDownList");
            DropDownListObj.selectItemByValue($scope.tempdateid);
        } catch (e) {

        }

    }

    $scope.GetSequence = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetAutoSequence"
        }).then(function successCallback(response) {
            $scope.modelMain.Sequence = response.data;
            $scope.model = Object.assign({}, $scope.modelMain);
        });

    };
    $scope.GetSequence();

    $scope.modelMain = {
        Id: null,
        Sequence: 1,
        TaskDescription: null,
        UserDefineTask: null,
        Code: null,
        StandardName: null,
        Active: true,
        ForNewOrder: false,
        HasSubTaskList: 0,
        TaskCategoryId: null,
        TaskSubCategoryId: null,
        ProcessId: null,
        DepartmentId: null,
        IsMandatory: false,
        TaskType: null,
        IsTaskMilestone: false,
        TaskAppliedOnId: null,
        TaskDependentDatesId: null,
        Remarks: null,
        LagDays: 0,
        StandardDays: 0,
        WillSendEmail: false,
        WillSendSMS: false,
        ConsiderOffDays: true,
        RepeatTask: false,
        ResponsiblePersonCategory: null
    };
    $scope.model = Object.assign({}, $scope.modelMain);

    $scope.Get = function (args) {
        $scope.model = Object.assign({}, $scope.modelMain);
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
            $scope.SubTaskList = response.data.subtasks;

            if (angular.isUndefinedOrNull(response.data.subtasks) == true
                || response.data.subtasks.length == 0) {
                $scope.SubTaskList = [];
            }

            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            for (var j = 0; j < response.data.plants.length; j++) {
                DropDownListObj.selectItemByValue(response.data.plants[j].PlantId);
            }

        });
    }

    $scope.SubTask = {};
    $scope.EditSubTask = function (args) {

        SubTask = args.data;
    };

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }
    $scope.Save = function () {
        try {

            CheckField("Task Description", $scope.model.TaskDescription);
            CheckField("User Define Task", $scope.model.UserDefineTask);
            CheckField("Code", $scope.model.Code);
            CheckField("Standard Name", $scope.model.StandardName);
            CheckField("Sequence", $scope.model.Sequence);
            CheckField("Task Category", $scope.model.TaskCategoryId);
            CheckField("Task SubCategory", $scope.model.TaskSubCategoryId);
            CheckField("Process", $scope.model.ProcessId);
            CheckField("Department", $scope.model.DepartmentId);
            CheckField("Task Type", $scope.model.TaskType);
            CheckField("Task AppliedOn", $scope.model.TaskAppliedOnId);
            CheckField("Task Dependent Date", $scope.model.TaskDependentDatesId);
            CheckField("Responsible Person Category", $scope.model.ResponsiblePersonCategory);
            CheckField("Lag Days", $scope.model.LagDays);
            CheckField("Standard Days", $scope.model.StandardDays);
            CheckField("Story Point", $scope.model.StoryPoint);

            var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
            var plantList = DropDownListObj.getSelectedValue();

            if (plantList.length == 0)
                throw "Select plant(s)";


            $scope.SubTaskList = $("#gridSubTaskList").ejGrid("instance").model.dataSource();

            $http({
                method: 'POST',
                data: { taskmaster: $scope.model, subtasks: $scope.SubTaskList, plants: plantList },
                url: $scope.path + "Save"
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadData(response.data.Id);
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
        $scope.SubTaskList = [];
        $scope.TaskDependentDateList = [];
        var DropDownListObj = $("#ddlPlantList").data("ejDropDownList");
        DropDownListObj.uncheckAll();
        $("#gridSubTaskList").ejGrid("instance").refreshContent();
        $scope.GetSequence();
        $scope.model = null;
    };

}