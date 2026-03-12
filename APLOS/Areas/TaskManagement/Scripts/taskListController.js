'use strict';
taskListController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'signalR'];
function taskListController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, signalR) {
    $rootScope.title = 'Task List';
    $scope.path = "TaskManagement/TaskList/";
    $scope.proper = [{ paneSize: "20%" }, {}]
    $scope.taskmanagerSubTaskList = [];
    $scope.TaskCategoryList = [];
    $scope.TaskSubCategoryList = [];
    $scope.UserList = [];
    $scope.registerSignalR = null;
    $scope.RegisterSuccessful = false;
    $scope.signalRTimeOut = 0;
    $scope.messageQueue = [];
    $scope.ToDoFilePath = virtualPath.IssueTransactionDocument;

    $http({
        method: 'POST', url: $scope.path + 'GetUser', dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.UserList = response.data.UserList;
        try {
            $scope.EMPINFO = response.data.LoginUser[0];
            $scope.EMPID = response.data.Id;
            signalR.connection.qs = { 'UserToken': $scope.EMPID };
            signalR.Hub = signalR.connection.createHubProxy('aplosbroadcasthub')
            signalR.EmployeeID = $scope.EMPID;
            $scope.countTotalUnread();

            signalR.Hub.on("GetNewTask", function (TaskId, Status) {

                $scope.getTaskAccordingToRresponsiblePersonList();
                $scope.GetAllUnreadThreads();
                $scope.GetAllUnreadTasks();

            });
            signalR.Hub.on("GetTaskComment", function (TaskId, Message, Task) {

                if ($scope.ToDoId == TaskId) {
                    $scope.CommentsList.push(Message);

                    $http({
                        method: 'POST', url: $scope.path + 'UpdateToDoCommentReadStatus', dataType: 'JSON',
                        data: { ToDoId: TaskId }

                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult('error', 'failure');
                        }
                        else {
                        }
                    }, function errorCallback(response) {
                        ShowResult('Failed', 'failure');
                    });
                }
                else {

                    var getRow = $filter("filter")($scope.UnreadCommentList, { "Id": TaskId });
                    if (getRow.length == 0) {
                        $scope.UnreadCommentList.push(Task);
                    }
                    //$scope.commentScroll();
                    //$scope.GetAllUnreadThreads();
                }

            });
            signalR.Hub.on("GetChat", function (Message) {


                if ($scope.ChatMasterId == Message.ChatMasterId) {
                    $scope.CurrentChat.push(Message);
                    $scope.ReadChatByChatMasterId(Message.ChatMasterId);
                }
                else {

                    var getRow = $filter("filter")($scope.UserList, { "Id": Message.EmployeeId });
                    if (getRow.length > 0) {
                        // $scope.UnreadCommentList.push(Task);
                        getRow[0].UnreadChat = Message.Chat;
                        getRow[0].UnreadChatDateCreated = Message.DateCreated;
                        getRow[0].UnreadChatCount = 1;

                        $scope.countTotalUnread();
                    }
                    //$scope.commentScroll();
                    //$scope.GetAllUnreadThreads();
                }

            });

            signalR.Hub.on("GetAllConnectedPeople", function (s) {
                try {


                    for (var i = 0; i < $scope.UserList.length; i++) {
                        $scope.UserList[i].IsConnected = false;
                    }
                    for (var k = 0; k < s.length; k++) {
                        for (var i = 0; i < $scope.UserList.length; i++) {
                            if ($scope.UserList[i].Id == s[k]) {
                                $scope.UserList[i].IsConnected = true;
                                break;
                            }
                        }
                    }

                    $scope.refreshUsers();
                } catch (e) {

                }

            });
            signalR.Hub.on("GetCurrentConnected", function (CurrentConnected) {
                try {


                    if (CurrentConnected != $scope.EMPID) {
                        for (var i = 0; i < $scope.UserList.length; i++) {
                            if ($scope.UserList[i].Id == CurrentConnected) {
                                if ($scope.UserList[i].IsConnected != true) {
                                    $scope.UserList[i].IsConnected = true;
                                    var message = $scope.UserList[i].EmployeeName + " connected";
                                    $scope.animatelivemessage(message);
                                    $scope.refreshUsers();
                                }
                                break;
                            }
                        }
                    }
                } catch (e) {

                }
            });
            signalR.Hub.on("GetCurrentDisconnected", function (CurrentDisconnected) {
                try {
                    if (CurrentDisconnected != $scope.EMPID) {
                        for (var i = 0; i < $scope.UserList.length; i++) {
                            if ($scope.UserList[i].Id == CurrentDisconnected) {
                                if ($scope.UserList[i].IsConnected != false) {
                                    $scope.UserList[i].IsConnected = false;
                                    var message = $scope.UserList[i].EmployeeName + " disconnected";
                                    $scope.animatelivemessage(message);
                                    $scope.refreshUsers();
                                }
                                break;
                            }
                        }
                    }
                } catch (e) {

                }
            });
            signalR.StartSignalR().then(function () {
                if (signalR.isInitialized) {
                    //$scope.IsConnectedToHub = true;
                    $scope.GetAllConnectedUsers();

                }
                else {
                    //$scope.IsConnectedToHub = false;

                }
            });
        } catch (e) {

        }

    }, function errorCallback(response) {
    });

    $scope.IsConnectedToHub = false;
    $scope.IsConnectedToHubCheck = function () {
        $timeout(function () {
            $scope.IsConnectedToHub = signalR.isInitialized;
            $scope.IsConnectedToHubCheck();
        }, 20000);
    }
    $scope.IsConnectedToHubCheck();

    $scope.livemessage = '';
    $scope.showlivemessage = false;
    $scope.animatelivemessage = function (message) {
        if ($scope.livemessage == message)
            return;

        $scope.messageQueue.unshift(message);

        if ($scope.livemessage == true)
            return;


        $scope.ShowMessageQueue();
    }
    $scope.ShowMessageQueue = function () {
        if ($scope.messageQueue.length == 0)
            return;

        $scope.showlivemessage = true;
        $scope.livemessage = $scope.messageQueue[$scope.messageQueue.length - 1];
        $timeout(function () {
            $scope.livemessage = '';
            $scope.showlivemessage = false;
            $scope.messageQueue.pop();

            if ($scope.messageQueue.length > 0)
                $scope.ShowMessageQueue();
        }, 1500);
    }

    $scope.GetAllConnectedUsers = function () {
        signalR.Hub.invoke("SendAllConnectedPeople", $scope.EMPID);
        signalR.Hub.invoke("connect", $scope.EmployeeID);
    }

    $scope.refreshUsers = function () {
        try {

            $scope.UserList = $filter('orderBy')($scope.UserList, 'IsConnected', true)
            var gridObj = $("#loggeduserlist").data("ejGrid");
            gridObj.refreshContent();

        } catch (e) {

        }
    }
    //$scope.animatelivemessage("tarek Talukder logged in");

    $scope.EMPID = $window.employeeId;
    $scope.EMPINFO = {};
    $scope.DisableEditTask = true;
    $scope.DisableEditTaskDetail = true;
    $scope.TaskCurrentStatus = [{ Id: "ToStart", UserName: "To Start" }, { Id: "InProgress", UserName: "In Progress" }];
    $scope.TaskCurrentStatusFilter = [{ Id: "", UserName: "" }, { Id: "ToStart", UserName: "To Start" }, { Id: "InProgress", UserName: "In Progress" }];
    $scope.menuitemTemp = [

        { count: 0, unread: 0, value: "Home", text: "My Creation", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-home", selected: "selected", authorizationType: "CreatedBy" },
        { count: 0, unread: 0, value: "MyTasks", text: "My Tasks", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-user", authorizationType: "AssignTo" },
        { count: 0, unread: 0, value: "CheckBy", text: "To Check", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-ok", authorizationType: "CheckBy" },
        { count: 0, unread: 0, value: "CrossCheckBy", text: "To Cross Check", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-check", authorizationType: "CrossCheckBy" },
        { count: 0, unread: 0, value: "ApproveBy", text: "To Approve", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-pawn", authorizationType: "ApproveBy" },

        { count: 0, unread: 0, value: "UpdateAudit", text: "Update", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-flash", authorizationType: "UpdateAudit" },
        { count: 0, unread: 0, value: "FollowUpAudit", text: "Follow-up Audit", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-flash", authorizationType: "FollowUpAudit" },
        { count: 0, unread: 0, value: "InternalAudit", text: "Internal Audit", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-flash", authorizationType: "InternalAudit" },
        { count: 0, unread: 0, value: "ExternalAudit", text: "External Audit", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-flash", authorizationType: "ExternalAudit" }
    ];

    $scope.TaskClosingStatus = [
        { text: "Active", value: "Active", contentType: "textandimage", prefixicon: "e-icon e-login", selected: "selected" },
        { text: "Closed", value: "Closed", contentType: "textandimage", prefixicon: "e-icon e-login" }];
    $scope.TaskListToolbarSettings = {
        showToolbar: true,

        toolbarItems: [

            ej.Grid.ToolBarItems.Search
        ],
        customToolbarItems: [
            {
                text: "", tooltip: "Additional Search Parameters", templateID: "#AdditionalFilter", itemId: 'AdditionalFilter'
            },
            {
                text: "", tooltip: "Clear Search", templateID: "#ClearAdditionalFilter", itemId: 'ClearAdditionalFilter'
            }]
    };

    $scope.TaskinstantFilterMain = [
        { text: "Today", basetext: "Today", value: "Today", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-calendar e-large", selected: "selected", count: 0 },
        { text: "Over Due", basetext: "Over Due", value: "OverDue", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-time e-large", count: 0 },
        { text: "Next Week", basetext: "Next Week", value: "ThisWeek", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-calendar e-large", count: 0 },
        { text: "Future Tasks", basetext: "Future Tasks", value: "Future", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-calendar e-large", count: 0 },
        { text: "Review & Close", basetext: "Review & Close", value: "ToClose", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-ok-sign e-large", count: 0 },
        { text: "High Priority", basetext: "High Priority", value: "HighPriority", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-ok-sign e-large", count: 0 },
        { text: "Actionable", basetext: "Actionable", value: "ToCloseReview", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-ok-sign e-large", count: 0 },
        { text: "Non Actionable", basetext: "Non Actionable", value: "CloseWithoutReview", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-ok-sign e-large", count: 0 },
        { text: "Unread", basetext: "Unread", value: "Unread", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-book e-large", count: 0 },
        { text: "All Tasks", basetext: "All Tasks", value: "All", contentType: "textandimage", prefixIcon: "glyphicon glyphicon-book e-large", count: 0 },

    ];

    $scope.menuitem = Object.assign([], $scope.menuitemTemp);
    $scope.SelectedMenu = Object.assign({}, $scope.menuitem[0]);
    $scope.menuClick = function (args) {

        $scope.SelectedMenu = Object.assign({}, args.data);


        //select default button when menu changes
        //$scope.CurrentSelectedFilterButton = 0;
        //var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
        //var element = $("#groupButtonForFilter").find('li')[0];
        //groupButtonObj.selectItem(element);




        var gridObj = $("#GridIssueTransaction").data("ejGrid");
        gridObj.refreshContent(true);

        var gridObj = $("#menuPane").data("ejGrid");
        var selectedRows = gridObj.getSelectedRows()[0].rowIndex;


        var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
        for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {
            var element = $("#groupButtonForFilter").find('li')[i];
            element.children[0].children[1].textContent = $scope.TaskinstantFilterMain[i].text;
        }

        for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

            var element = $("#groupButtonForFilter").find('li')[i];
            groupButtonObj.showItem(element);
            if ($scope.TaskinstantFilterMain[i].value == 'ToCloseReview' || $scope.TaskinstantFilterMain[i].value == 'CloseWithoutReview') {
                groupButtonObj.hideItem(element);
            }
        }

        if ($scope.SelectedMenu.authorizationType == 'CreatedBy') {
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

                var element = $("#groupButtonForFilter").find('li')[i];
                if ($scope.TaskinstantFilterMain[i].value == 'Unread') {
                    groupButtonObj.hideItem(element);
                }
            }

        }
        if ($scope.SelectedMenu.authorizationType == 'AssignTo') {
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

                var element = $("#groupButtonForFilter").find('li')[i];
                if ($scope.TaskinstantFilterMain[i].value == 'ToClose') {
                    groupButtonObj.hideItem(element);
                }
            }

        }

        if ($scope.SelectedMenu.authorizationType == 'CheckBy'
            || $scope.SelectedMenu.authorizationType == 'CrossCheckBy'
            || $scope.SelectedMenu.authorizationType == 'ApproveBy'
        ) {

            var ReviewIndex = 0;
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {
                var element = $("#groupButtonForFilter").find('li')[i];
                groupButtonObj.hideItem(element);

                if ($scope.TaskinstantFilterMain[i].value == 'ToCloseReview'
                    || $scope.TaskinstantFilterMain[i].value == 'CloseWithoutReview'
                    || $scope.TaskinstantFilterMain[i].value == 'Unread') {

                    groupButtonObj.showItem(element);
                    if (ReviewIndex == 0)
                        ReviewIndex = i;

                }
            }

            var args = { index: ReviewIndex };

            $scope.CurrentSelectedFilterButton = ReviewIndex;
            var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
            // var element = $("#groupButtonForFilter").find('li')[$scope.CurrentSelectedFilterButton];
            var element = $("#groupButtonForFilter").find('li')[ReviewIndex];
            groupButtonObj.selectItem(element);

            //$scope.ToDoFilter(args)
        }
        else {
            var args = { index: 0 };
            $scope.CurrentSelectedFilterButton = 0;
            var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
            var element = $("#groupButtonForFilter").find('li')[0];
            groupButtonObj.selectItem(element);

            //$scope.ToDoFilter(args);
        }




        $scope.getTaskAccordingToRresponsiblePersonList();

    }

    $scope.MenuCreate = function (args) {
        var gridObj = $("#menuPane").data("ejGrid");
        gridObj.selectRows(0);
    }
    $scope.queryCellInfo = function (args) {
        try {

            //today's task
            var DueDate = new Date(args.data.DueDate);
            if (DueDate.getDate() == new Date().getDate()
                && DueDate.getMonth() == new Date().getMonth()
                && DueDate.getFullYear() == new Date().getFullYear()) {
                args.cell.bgColor = "#E6F0FF";
            }

            //overdue
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) < new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#FFF4E6";
            }

            //future
            if (new Date(DueDate.getFullYear(), DueDate.getMonth(), DueDate.getDate()) > new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())) {
                args.cell.bgColor = "#F5FFE6";
            }



        } catch (e) {

        }

    }
    $scope.ToDoModelBase = {
        DueDate: new Date(), TaskPriority: 1, TaskCategoryId: null, TaskSubCategoryId: null, TaskDetailDescription: '', StoryPoint: 0,
        TaskDescription: '', CurrentStatus: 'ToStart',
        AssignTo: { EmployeeId: null, EmployeeCode: null, EmployeeName: null, EmpPicPath: '' },
        CheckBy: { EmployeeId: null, EmployeeCode: null, EmployeeName: null, EmpPicPath: '' },
        CrossCheckBy: { EmployeeId: null, EmployeeCode: null, EmployeeName: null, EmpPicPath: '' },
        ApproveBy: { EmployeeId: null, EmployeeCode: null, EmployeeName: null, EmpPicPath: '' },
        Schedule: {}
    };
    $scope.ToDoModel = Object.assign({}, $scope.ToDoModelBase);

    $scope.datalist = [];
    $scope.SubTaskList = [];
    $scope.CommentList = [];
    $scope.TaskDesc = "";
    $scope.TempTaskDesc = "";
    $scope.TempTaskDetailDesc = "";
    $scope.SubTaskDesc = "";
    $scope.Comment = "";
    $scope.verticalmenutype = ej.MenuType.NormalMenu;
    $scope.verticalmenuorientation = ej.Orientation.Vertical;

    $scope.TempEditData = null;
    $scope.AddData = function () {
        if ($scope.TaskDesc === "")
            return;
        var data = { "Id": "NULL", "TaskDescription": $scope.TaskDesc };

        $http({
            method: 'POST', url: $scope.path + 'AddToDo', dataType: 'JSON',
            data: { ToDo: data }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.issueTransactionList = response.data.TaskData;


                var _data = {
                    data: { AuthorizationType: 'CreatedBy', Id: response.data.TaskSingleData[0].Id, TaskDescription: response.data.TaskSingleData[0].TaskDescription }
                };
                $scope.TempEditData = _data;
                $scope.message_confirmation = "Do you want to delegate this task?";
                angular.element(document.querySelector('#confirmAssignToOther')).modal('show');

                $scope.getTaskAccordingToRresponsiblePersonList();

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


        $scope.TaskDesc = "";
    }


    $scope.UnreadCommentList = [];
    $scope.UnreadTaskList = [];
    $scope.ToDoId = '';
    $scope.CommentText = '';
    $scope.CommentsList = [];
    $scope.AddToDoComment = function () {
        if ($scope.CommentText === "")
            return;
        var data = { "Id": "NULL", "CommentText": $scope.CommentText };

        $http({
            method: 'POST', url: $scope.path + 'AddToDoComment', dataType: 'JSON',
            data: { ToDo: data, ToDoId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.CommentsList = response.data.CommentsList;
                $scope.SendCommentsLive($scope.CommentsList[$scope.CommentsList.length - 1]);

                //$scope.commentScroll();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


        $scope.CommentText = "";
    }
    $scope.SendCommentsLive = function (comment) {

        //  var data = { EmpPicPath: $scope.EMPINFO.EmpPicPath, EmployeeName: $scope.EMPINFO.EmployeeName, CreatedTime: new Date(), CommentText: comment };
        var data = comment;
        try {
            if ($scope.ToDoModel.CreatedBy.EmployeeId != $scope.EMPID)
                signalR.Hub.invoke("SendTaskComment", $scope.ToDoModel.CreatedBy.EmployeeId, $scope.ToDoId, data, $scope.ToDoModel);
        } catch (e) {

        }
        try {
            if ($scope.ToDoModel.AssignTo.EmployeeId != $scope.EMPID)
                signalR.Hub.invoke("SendTaskComment", $scope.ToDoModel.AssignTo.EmployeeId, $scope.ToDoId, data, $scope.ToDoModel);

        } catch (e) {

        }
        try {
            if ($scope.ToDoModel.CheckBy.EmployeeId != $scope.EMPID)
                signalR.Hub.invoke("SendTaskComment", $scope.ToDoModel.CheckBy.EmployeeId, $scope.ToDoId, data, $scope.ToDoModel);

        } catch (e) {

        }

        try {
            if ($scope.ToDoModel.CrossCheckBy.EmployeeId != $scope.EMPID)
                signalR.Hub.invoke("SendTaskComment", $scope.ToDoModel.CrossCheckBy.EmployeeId, $scope.ToDoId, data, $scope.ToDoModel);

        } catch (e) {

        }

        try {
            if ($scope.ToDoModel.ApproveBy.EmployeeId != $scope.EMPID)
                signalR.Hub.invoke("SendTaskComment", $scope.ToDoModel.ApproveBy.EmployeeId, $scope.ToDoId, data, $scope.ToDoModel);
        } catch (e) {

        }

    }


    $scope.ScrollHeightComment = 0;
    $scope.ScrollHeightSubTask = 0;
    $('#wtfCommentScroll').scroll(function (event) {
        if ($scope.ScrollHeightComment == $('#wtfCommentScroll')[0].scrollHeight)
            return;
        $scope.ScrollHeightComment = $('#wtfCommentScroll')[0].scrollHeight;
        //$('#wtfCommentScroll').scrollTop($('#wtfCommentScroll')[0].scrollHeight - $('#wtfCommentScroll')[0].clientHeight);
        $("#wtfCommentScroll").animate({
            scrollTop: $('#wtfCommentScroll')[0].scrollHeight - $('#wtfCommentScroll')[0].clientHeight
        }, 500);
    });
    $('#wtfSubTaskScroll').scroll(function (event) {
        if ($scope.ScrollHeightSubTask == $('#wtfSubTaskScroll')[0].scrollHeight)
            return;
        $scope.ScrollHeightSubTask = $('#wtfSubTaskScroll')[0].scrollHeight;
        $('#wtfSubTaskScroll').scrollTop($('#wtfSubTaskScroll')[0].scrollHeight - $('#wtfSubTaskScroll')[0].clientHeight);
    });
    $scope.commentScroll = function (args) {
        try {
            $('#wtfCommentScroll').scrollTop($('#wtfCommentScroll')[0].scrollHeight - $('#wtfCommentScroll')[0].clientHeight);

            args.cell.style.background = "#ffffff";


        } catch (e) {

        }
    }
    $scope.SubTaskScroll = function (args) {
        $('#wtfSubTaskScroll').scrollTop($('#wtfSubTaskScroll')[0].scrollHeight - $('#wtfCommentScroll')[0].clientHeight);
    }

    $scope.getCommentList = function () {
        $scope.CommentsList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllComments', dataType: 'JSON',
            data: { ToDoId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.CommentsList = response.data;

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.GetAllUnreadThreads = function () {
        $scope.UnreadCommentList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllUnreadThreads', dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.UnreadCommentList = response.data;
            }
        }, function errorCallback(response) {

        });
    }
    $scope.GetAllUnreadThreads();


    $scope.GetAllUnreadTasks = function () {
        $scope.UnreadTaskList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllUnreadTasks', dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error == true) {

            }
            else {
                $scope.UnreadTaskList = response.data;
            }
        }, function errorCallback(response) {

        });
    }
    $scope.GetAllUnreadTasks();

    $scope.SubTaskText = '';
    $scope.SubTasksList = [];
    $scope.AddToDoSubTask = function () {
        if ($scope.SubTaskText === "")
            return;
        var data = { "Id": "NULL", "TaskDetail": $scope.SubTaskText };

        $http({
            method: 'POST', url: $scope.path + 'AddToDoSubTask', dataType: 'JSON',
            data: { ToDo: data, ToDoId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.SubTasksList = response.data.SubTasksList;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


        $scope.SubTaskText = "";
    }
    $scope.getSubTaskList = function () {
        $scope.SubTasksList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllSubTasks', dataType: 'JSON',
            data: { ToDoId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.SubTasksList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.UpdateToDoSubTask = function (args) {
        var gridObj = $("#anglistviewSubTasks").ejGrid("instance");
        var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];
        $http({
            method: 'POST', url: $scope.path + 'UpdateToDoSubTasks', dataType: 'JSON',
            data: { ToDoSubTask: currRow }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }


    $scope.SelectedSubTask = [];
    $scope.SubTaskSelectionType = 'View';//View,Edit
    $scope.SubTaskRemarksList = [];
    $scope.SubTotalRemarks = '';
    $scope.showSubTaskRemarks = function (data, flag) {
        $scope.SelectedSubTask = data;
        $scope.SubTaskSelectionType = flag;
        getAllSubTaskRemarks();

        var eDialog = $("#dialogSubTaskRemarks").data("ejDialog");
        eDialog.open();


    }
    function getAllSubTaskRemarks() {
        $scope.SubTaskRemarksList = [];

        $http({
            method: 'POST', url: $scope.path + 'GetToDoSubTasksRemarks', dataType: 'JSON',
            data: { SubTaskId: $scope.SelectedSubTask.Id }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.SubTaskRemarksList = response.data;


                for (var i = 0; i < $scope.SubTasksList.length; i++) {
                    if ($scope.SubTasksList[i].Id == $scope.SelectedSubTask.Id) {
                        $scope.SubTasksList[i].hasRemarks = false;
                        if ($scope.SubTaskRemarksList.length > 0)
                            $scope.SubTasksList[i].hasRemarks = true;
                    }
                }
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });


    }
    $scope.deleteSubTaskRemarks = function (id) {

        $http({
            method: 'POST', url: $scope.path + 'DeleteToDoSubTasksRemarks', dataType: 'JSON',
            data: { Id: id }

        }).then(function successCallback(response) {

            getAllSubTaskRemarks();
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.UpdateToDoSubTasksRemarks = function () {
        if ($scope.SubTotalRemarks === "")
            return;

        $http({
            method: 'POST', url: $scope.path + 'UpdateToDoSubTasksRemarks', dataType: 'JSON',
            data: { TaskMasterId: $scope.SelectedSubTask.TaskManagerMasterId, SubTaskId: $scope.SelectedSubTask.Id, Remarks: $scope.SubTotalRemarks }

        }).then(function successCallback(response) {

            getAllSubTaskRemarks();
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

        $scope.SubTotalRemarks = "";
    }


    $scope.TempToDo = null;
    $scope.DeleteToDoSubTask = function (args) {
        $scope.TempToDo = args;
        $scope.message_confirmation = "Are you sure to delete the selected sub task?";
        angular.element(document.querySelector('#confirmDeleteSubTask')).modal('show');
    }
    $scope.DeleteToDoSubTaskFinal = function () {
        $http({
            method: 'POST', url: $scope.path + 'DeleteToDoSubTasks', dataType: 'JSON',
            data: { ToDoSubTask: $scope.TempToDo.data }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.getSubTaskList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }


    $scope.FilesList = [];
    $scope.TempFileDesc = '';
    $scope.onBeginUpload = function (args) {
        try {
            if ($scope.ToDoId == null || $scope.ToDoId == '')
                throw 'Please select/save the To Do first'

            if ($scope.TempFileDesc == null || $scope.TempFileDesc == '')
                throw 'Please add file description first'

            var _data = { ToDoId: $scope.ToDoId, FileDescription: $scope.TempFileDesc };

            args.data = JSON.stringify(_data);

         
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = $scope.path + "SaveDefault/";
    $scope.fileselect = function (e) {

    }


    $scope.errorPicUpload = function (e) {

        if ($scope.ToDoId == null || $scope.ToDoId == '')
            throw 'Please select/save the To Do first'

        else if ($scope.TempFileDesc == null || $scope.TempFileDesc == '')
            throw 'Please add file description first'

        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }
    $scope.getFileList = function () {

        $scope.TempFileDesc = '';
        $scope.FilesList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllFiles', dataType: 'JSON',
            data: { ToDoId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.FilesList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.message_confirmation = '';
    $scope.TempAttachment = null;
    $scope.DeleteToDoFile = function (args) {

        $scope.TempAttachment = args;
        $scope.message_confirmation = "Are you sure to delete the selected file?";
        angular.element(document.querySelector('#confirmDeleteAttachment')).modal('show');

    }
    $scope.OpenFileUploadPOPUp = function () {
        var eDialog = $("#dialogFileUpload").data("ejDialog");
        eDialog.open();

        $scope.TempFileDesc = '';
    }
    $scope.UpdateToDoFile = function (id, desc) {

        $http({
            method: 'POST', url: $scope.path + 'UpdateFile', dataType: 'JSON',
            data: { Id: id, Description: desc }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
               
                ShowResult(response.data.Message, 'success');
                //$scope.getFileList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.DeleteToDoFileFinal = function () {

        $http({
            method: 'POST', url: $scope.path + 'DeleteFile', dataType: 'JSON',
            data: { FileId: $scope.TempAttachment }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.getFileList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }

    $scope.enableCommitmentDate = false;
    $scope.enableRevisedCommitmentDate = false;
    $scope.getToDo = function () {

        $http({
            method: 'POST',
            url: $scope.path + "GetMasterData",
            data: { ToDoId: $scope.ToDoId }
        }).then(function successCallback(response) {
            $scope.TaskCategoryList = response.data.TaskCategory;
            $scope.TaskSubCategoryList = response.data.TaskSubCategory;

            $http({
                method: 'POST', url: $scope.path + 'GetToDo', dataType: 'JSON',
                data: { ToDoId: $scope.ToDoId }

            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult('error', 'failure');
                }
                else {
                    $scope.enableCommitmentDate = false;
                    $scope.enableRevisedCommitmentDate = false;

                    $scope.ToDoModel = response.data.DATA;

                    if ($scope.ToDoModel.CreatedBy.EmployeeId != $scope.EMPID) {
                        if ($scope.ToDoModel.AssignTo.EmployeeId == $scope.EMPID || $scope.ToDoModel.AssignTo.EmployeeId == $scope.ToDoModel.CreatedBy.EmployeeId) {
                            if (baseService.isUndefinedOrNull($scope.ToDoModel.AssignTo.RevisedCommitmentDate) == true) {
                                $scope.enableCommitmentDate = true;
                            }
                            if (baseService.isUndefinedOrNull($scope.ToDoModel.AssignTo.CommitmentDate) == false) {
                                $scope.enableRevisedCommitmentDate = true;
                            }
                        }
                    }
                    $scope.RefreshMenu();

                    $scope.GetAllUnreadThreads();
                    $scope.GetAllUnreadTasks();
                }
            }, function errorCallback(response) {
                ShowResult('Failed', 'failure');
            });
        });


    }
    $scope.UpdateToDoAuth = function () {
        $http({
            method: 'POST', url: $scope.path + 'UpdateToDoAuth', dataType: 'JSON',
            data: { ToDo: $scope.ToDoModel, ToDoEmployee: $scope.ToDoModel, TaskManagerMasterId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {


                $scope.sendStatusNotification();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.sendStatusNotification = function () {
        try {
            signalR.Hub.invoke("SendNewTask", $scope.ToDoModel.AssignTo.EmployeeId, $scope.ToDoModel.Id, 'Added');

        } catch (e) {

        }

        try {
            signalR.Hub.invoke("SendNewTask", $scope.ToDoModel.CheckBy.EmployeeId, $scope.ToDoModel.Id, 'Added');

        } catch (e) {

        }

        try {
            signalR.Hub.invoke("SendNewTask", $scope.ToDoModel.CrossCheckBy.EmployeeId, $scope.ToDoModel.Id, 'Added');

        } catch (e) {

        }

        try {
            signalR.Hub.invoke("SendNewTask", $scope.ToDoModel.ApproveBy.EmployeeId, $scope.ToDoModel.Id, 'Added');
        } catch (e) {

        }
        $scope.getTaskList();
    }
    $scope.UpdateToDoMaster = function (args) {
        try {
            if (args.isInteraction == false)
                return;
        } catch (e) {

        }



        $http({
            method: 'POST', url: $scope.path + 'UpdateToDoMaster', dataType: 'JSON',
            data: { ToDo: $scope.ToDoModel, TaskManagerMasterId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

                $scope.TempTaskDesc = $scope.ToDoModel.TaskDescription;
                $scope.TempTaskDetailDesc = $scope.ToDoModel.TaskDetailDescription;

                $scope.getTaskList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.UpdateToDoMasterForRating = function (args) {
        if (args.isInteraction == false)
            return;
        try {
            $scope.ToDoModel.TaskPriority = args.value;
        } catch (e) {

        }

        $http({
            method: 'POST', url: $scope.path + 'UpdateToDoMaster', dataType: 'JSON',
            data: { ToDo: $scope.ToDoModel, TaskManagerMasterId: $scope.ToDoId }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.UpdateToDoMasterCurrentStatus = function (args) {
        if (args.isInteraction == false)
            return;
        var gridObj = $("#GridIssueTransaction").ejGrid("instance");
        var currRow = gridObj.model.currentViewData[this.element.closest("tr").index()];

        $scope.SelectedMenu.authorizationType = currRow.AuthorizationType;

        //bool closed, string authorizationtype, string TaskManagerMasterId
        var ApiName = "UpdateToDoMasterStatus";
        if (currRow.TaskTypeGroup.toUpperCase() == "TODO")
            ApiName = "UpdateToDoMasterStatusForToDo";

        $http({
            method: 'POST', url: $scope.path + ApiName, dataType: 'JSON',
            data: { closed: args.isChecked, authorizationtype: $scope.SelectedMenu.authorizationType, TaskManagerMasterId: currRow.Id }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {

                var gridObj = $("#menuPane").data("ejGrid");
                var selectedRows = gridObj.getSelectedRows()[0].rowIndex;

                $scope.SelectedMenu = Object.assign({}, $scope.menuitem[selectedRows]);
                //$scope.sendStatusNotification();
                $scope.getTaskList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }

    $scope.DeleteToDo = function (args) {
        $http({
            method: 'POST', url: $scope.path + 'DeleteToDo', dataType: 'JSON',
            data: { ToDo: args.data }

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.getTaskList();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }


    $scope.AllowToDoEdit = false;
    $scope.getToChat = function (args) {
        var Object = $("#multiAccordion").data("ejAccordion");
        Object.collapseAll();

        for (var i = 0; i < $scope.UnreadCommentList.length; i++) {
            if ($scope.UnreadCommentList[i].Id === args.data.Id) {
                $scope.UnreadCommentList.splice(i, 1);
                i--;
            }
        }
        $scope.GetEdit(args);
    }
    $scope.getToTask = function (args) {
        for (var i = 0; i < $scope.UnreadTaskList.length; i++) {
            if ($scope.UnreadTaskList[i].Id === args.data.Id) {
                $scope.UnreadTaskList.splice(i, 1);
                i--;
            }
        }
        $scope.GetEdit(args);
    }
    $scope.EditTaskClose = function (args) {
        $scope.ToDoId = '';
    }
    $scope.Get = function (args) {
        try {
            if (args.data.IsRead == false) {

                args.data.IsRead = true;
                $scope.SelectedMenu.unread = parseInt($scope.SelectedMenu.unread) - 1;
            }

        } catch (e) {

        }
        var gridObjGoToPage = $("#GridIssueTransaction").data("ejGrid");
        gridObjGoToPage.gotoPage(1);
        $scope.GetEdit(args);
    }
    $scope.GetEdit = function (args) {
        $scope.AllowToDoEdit = false;




        $scope.SelectedMenu.authorizationType = args.data.AuthorizationType;
        $scope.ToDoId = args.data.Id;
        $scope.TempTaskDesc = args.data.TaskDescription;
        $scope.TempTaskDetailDesc = args.data.TaskDetailDescription;
        $scope.ToDoModel = Object.assign({}, $scope.ToDoModel);
        $scope.ToDoModel.TaskDescription = args.data.TaskDescription;
        $scope.ToDoModel.TaskPriority = args.data.TaskPriority;

        try {
            if ($scope.ToDoModel.TaskPriority == null)
                $scope.ToDoModel.TaskPriority = 0;
        } catch (e) {

        }

        if (args.data.CreatedById == $scope.EMPID || args.data.AuthorizationType == "CreatedBy")
            $scope.AllowToDoEdit = true;

        var eDialog = $("#dialogEditTask").data("ejDialog");
        eDialog.open();

        $scope.getCommentList();
        $scope.getSubTaskList();
        $scope.getFileList();
        $scope.getToDo();
        $scope.getssueDetails();


        var gridObj = $("#GridIssueTransaction").data("ejGrid");
        gridObj.refreshContent(true);



    }

    $scope.taskManagerMasterNew = {
        Id: null,
        IssueTransactionId: null,
        CreatedById: null,
        AssignToId: null,
        TaskType: null,
        TaskDescription: null,
        DueDate: null,
        RevisedCommitmentDate: null,
        CommitmentDate: null
    };

    $scope.taskAuditNew = {
        Id: null,
        TaskManagerMasterId: null,

        Remarks: null,
        CommitmentDate: null,
        RevisedCommitmentDate: null

    };
    $scope.taskManagerSubTask = {
        Id: null,
        IsDone: false,
        taskManagerMasterId: null
    };


    //#region Monir
    $scope.issueTransactionList = [];
    $scope.getToDoList = function () {

        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetToDoList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.issueTransactionList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    //$scope.getToDoList();

    $scope.selectedTaskStatus = 'Active';
    $scope.changeTaskStatus = function (args) {
        $scope.selectedTaskStatus = $scope.TaskClosingStatus[args.index].value;
        $scope.getTaskAccordingToRresponsiblePersonList();
    }

    $scope.getTaskAccordingToRresponsiblePersonList = function () {

        var authForLoadData = '';
        var flagForLoadData = '';
        try {
            //var gridObj = $("#menuPane").data("ejGrid");
            //var selectedRows = gridObj.getSelectedRows()[0].rowIndex;

            //authForLoadData = $scope.menuitem[selectedRows].authorizationType;
            //flagForLoadData = $scope.menuitem[selectedRows].value;

            authForLoadData = $scope.SelectedMenu.authorizationType;
            flagForLoadData = $scope.SelectedMenu.value;


        } catch (e) {
            authForLoadData = $scope.SelectedMenu.authorizationType;
            flagForLoadData = $scope.SelectedMenu.value;
        }




        $http({
            method: 'GET',
            url: 'TaskManagement/TaskList/GetTaskAccordingToRresponsiblePersonList?authorizationType=' + authForLoadData + "&flag=" + flagForLoadData + "&taskstatus=" + $scope.selectedTaskStatus,//+ args.data.menuitem.authorizationType,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.issueTransactionList = response.data.DATA;
                $scope.EMPID = response.data.EMPID;



                for (var i = 0; i < $scope.issueTransactionList.length; i++) {
                    $scope.issueTransactionList[i].DueDateFilter = new Date($scope.issueTransactionList[i].DueDateFilter);
                    try { $scope.issueTransactionList[i].CommitmentDateFilter = new Date($scope.issueTransactionList[i].CommitmentDateFilter); } catch (e) { }

                }

                $scope.RefreshMenu();
                $scope.taskCountForFilter();
                //var gridObj = $("#GridIssueTransaction").data("ejGrid");
                //gridObj.refreshContent();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.RefreshMenu = function () {

        $http({
            method: 'GET',
            url: 'TaskManagement/TaskList/GetMenu?taskstatus=' + $scope.selectedTaskStatus,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                var STAT = response.data.STAT;
                var gridObj = $("#menuPane").data("ejGrid");
                var selectedRows = gridObj.getSelectedRows()[0].rowIndex;

                for (var i = 0; i < $scope.menuitem.length; i++) {
                    $scope.menuitem.count = 0;
                }

                for (var i = 0; i < STAT.length; i++) {
                    var val = STAT[i].TaskType;
                    var row = $filter('filter')($scope.menuitem, { 'value': val });
                    if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                        row[0].count = STAT[i].NoOfTasks;
                        row[0].unread = STAT[i].Unread;
                    }
                }

                gridObj.refreshContent(true);
                gridObj.selectRows(selectedRows);






            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.getTaskList = function () {

        $scope.getTaskAccordingToRresponsiblePersonList();
    }
    $scope.getTaskList();

    $scope.controllPartialView = function (currentView) {
        $scope.currentPartialView = "~/Areas/TaskManagement/Views/TaskList/IssueTransactionList.cshtml";
    }
    $scope.controllPartialView();

    $scope.saveAssignToMeDetail = function (args) {
        try {
            if (args.isInteraction == false)
                return;
        } catch (e) {

        }
        try {

            $http({
                method: 'POST',
                url: 'TaskManagement/TaskList/SaveAssignToMeDetail',

                data: {
                    taskAuditNew: $scope.ToDoModel.AssignTo
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $rootScope.ShowError(response.data.Message, 'failure');
                }
                else {

                }
            }, function errorCallBack(response) {
                $rootScope.ShowError(response.data.Message, 'failure');
            });
        } catch (e) {
            $rootScope.ShowError(response.data.Message, 'failure');
        }

    }
    $scope.saveStoryPoint = function (args) {
        try {
            if (args.isInteraction == false)
                return;
        } catch (e) {

        }
        try {

            $http({
                method: 'POST',
                url: 'TaskManagement/TaskList/saveStoryPoint',

                data: {
                    StoryPoint: $scope.ToDoModel.StoryPoint, Id: $scope.ToDoModel.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    $rootScope.ShowError(response.data.Message, 'failure');
                }
                else {
                    $scope.getTaskList();
                }
            }, function errorCallBack(response) {
                $rootScope.ShowError(response.data.Message, 'failure');
            });
        } catch (e) {
            $rootScope.ShowError(response.data.Message, 'failure');
        }

    }

    $scope.isCommitmentDateNull = true;
    $scope.issueTransactionId = null;
    $scope.EditAssignToMeDetail = function (args) {

        $scope.taskAuditNew.CommitmentDate = args.data.CommitmentDate;
        $scope.taskAuditNew.Remarks = args.data.Remarks;
        $scope.taskAuditNew.RevisedCommitmentDate = args.data.RevisedCommitmentDate;
        $scope.taskAuditNew.Id = args.data.TaskAuditId;
        $scope.taskAuditNew.TaskManagerMasterId = args.data.Id;
        $scope.taskManagerSubTask = args.data.Id;

        if (args.data.CommitmentDate == null) {
            $scope.isCommitmentDateNull = true;
        }
        else {
            $scope.isCommitmentDateNull = false;
        }

        $scope.getTaskManagerSubTasks();
        //$scope.getSubTaskByTaskManagerMasterId();
        $scope.ViewAssignToMeDetail();
    }
    $scope.ViewAssignToMeDetail = function () {
        angular.element(document.querySelector('#viewAssignToMeDetail')).modal('show');
    }

    $scope.subTaskListDone = [];



    $scope.getSubTaskByTaskManagerMasterId = function () {
        $http({
            method: 'GET',
            url: 'TaskManagement/TaskList/GetSubTaskByTaskManagerMasterId?taskManagerMasterId=' + $scope.taskManagerMasterNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
                return;
            }
            else {
                $scope.SubTaskList = response.data;

                return;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
            return;
        });
    }

    $scope.SaveIssueSubTask = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: 'issueTracker/IssueSubTask/Create',
            data: $scope.issueSubTaskNew,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

                angular.element(document.querySelector('#viewAssignToMeDetail')).modal('hide');
                $scope.clear();
            }
        }, function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        });

    }

    $scope.clear = function () {
        $scope.taskAuditNew.Remarks = null;
        $scope.taskAuditNew.RevisedCommitmentDate = null;
        $scope.taskAuditNew.CommitmentDate = null;


    }

    $scope.hideIssueTaskPopUp = function () {
        angular.element(document.querySelector('#viewAssignToMeDetail')).modal('hide');
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ToDayTaskList = [];
    $scope.getTodayTaskList = function () {
        $http({
            method: 'GET',
            url: 'issueTracker/IssueTransaction/GetTodayTaskList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                //ShowResult('Data is released', 'failure');
                $scope.ToDayTaskList = response.data;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }

    $scope.getTaskManagerSubTasks = function () {
        //try {
        $http({
            method: 'GET',
            url: 'TaskManagement/TaskList/GetTaskManagerSubTasksByResponsiblePersonId?taskManagerMasterId=' + $scope.taskAuditNew.TaskManagerMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
                return;
            }
            else {
                //ShowResult('Data is released', '  success');
                $scope.SubTaskList = response.data;

                return;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
            return;
        });
        //} catch (ex) {
        //     ShowResult(ex.Message, 'failure');
        //}
    }

    $scope.isDoneChange = function (args) {

        for (var i = 0; i < $scope.SubTaskList.length; i++) {

            if (args.model.value == $scope.SubTaskList[i].Id) {
                $scope.SubTaskList[i].IsDone = args.isChecked;
            }
        }

    }

    $scope.ScheduledTaskList = [];
    $scope.taskScheduleMasterId = '';
    $scope.GetScheduledTaskList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetScheduledTaskList',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
                return;
            }
            else {
                $scope.ScheduledTaskList = response.data;

                return;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
            return;
        });

    }
    $scope.UpdateScheduledTaskListPopUp = function (args) {
        $scope.taskScheduleMasterId = args.data.Id;
        $scope.message_confirmation = "Are you sure to delete the selected schedule?";
        angular.element(document.querySelector('#confirmDeleteSchedule')).modal('show');
    }

    $scope.UpdateScheduledTaskList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'UpdateScheduledTaskList',
            data: { taskmasterid: $scope.taskScheduleMasterId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
                return;
            }
            else {
                $scope.GetScheduledTaskList();

                return;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
            return;
        });

    }



    $scope.EmployeemodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'EmployeeCode', name: 'Code ' },
        { value: 'EmployeeName', name: 'Name ' },
        { value: 'Department', name: 'Department ' },
        { value: 'Designation', name: 'Designation ' },
        { value: 'Section', name: 'Section ' },
        { value: 'SubSection', name: 'Sub Section ' }
    ];
    $scope.searchCol = "UserName";
    $scope.searchVal = "";
    $scope.EmployeeSearchCol = "EmployeeName";
    $scope.EmployeeSearchVal = "";
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
                var k = $scope.ToDoModel[$scope.WhereEmployeeNeeded];

                $scope.ToDoModel[$scope.WhereEmployeeNeeded]['EmployeeId'] = response.data[0].SystemId;
                $scope.ToDoModel[$scope.WhereEmployeeNeeded]['EmployeeCode'] = response.data[0].EmployeeCode;
                $scope.ToDoModel[$scope.WhereEmployeeNeeded]['EmployeeName'] = response.data[0].EmployeeName;
                $scope.ToDoModel[$scope.WhereEmployeeNeeded]['EmpPicPath'] = response.data[0].EmpPicPath;




                $scope.UpdateToDoAuth();


            });
        } catch (e) {

        }
    }

    $scope.DeleteEmployee = function (flag) {

        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'id': $scope.ToDoId, 'authType': flag },
                url: $scope.path + 'DeleteAuthEmployee'

            }).then(function successCallback(response) {

                try {
                    signalR.Hub.invoke("SendNewTask", $scope.ToDoModel[flag].EmployeeId, $scope.ToDoModel.Id, 'Deleted');

                } catch (e) {

                }

                $scope.ToDoModel[flag] = Object.assign({}, $scope.ToDoModelBase[flag]);

            });
        } catch (e) {

        }
    }


    //task manager schedule
    $scope.taskScheduleMain = {
        Id: null,
        RepeatType: "Daily",
        StartDate: new Date(),
        EndDate: new Date(),
        AfterNoOfAccurence: 1,
        EveryInterval: 1,
        RepeatByDayNumber: 1,
        RepeatbyNthWeek: 'First',
        RepeatByMonth: 'January',
        RepeatbyOfEarly: 'January',
        RepeatByWeek: 'Sunday',

        IsAfter: false,
        IsOn: false,
        IsNever: true,
        WeeklyRepeatationBycommaSepDayName: "",
        Details: null,
        isWeekly: false,
        isYearly: false,
        isDaily: true,
        EveryWeekDay: false,

        isRepeatByDay: true,
        isRepeatByTheNthWeekForMonthly: false,

        isRepeatByTheMonth: true,
        isRepeatByTheNthWeekForYearly: false,
        OnPreviousAccomplishment: true

    };
    $scope.taskSchedule = Object.assign({}, $scope.taskScheduleMain);
    $scope.AuditSchedulerStatement = {
        RepeatType: '',
        Details: ''
    }
    $scope.dayList = [
        { day: 'Sun', isChecked: false },
        { day: 'Mon', isChecked: false },
        { day: 'Tue', isChecked: false },
        { day: 'Wed', isChecked: false },
        { day: 'Thu', isChecked: false },
        { day: 'Fri', isChecked: false },
        { day: 'Sat', isChecked: false }
    ];
    $scope.EveryRepeatedFlag = $scope.taskSchedule.RepeatType;
    $scope.flagCarringRecurring = '';

    $scope.ChangeToNever = function () {

        $scope.taskSchedule.IsNever = true;
        $scope.taskSchedule.IsAfter = false;
        $scope.taskSchedule.IsOn = false;

    };
    $scope.ChangeToAfter = function () {

        $scope.taskSchedule.IsAfter = true;
        $scope.taskSchedule.IsNever = false;
        $scope.taskSchedule.IsOn = false;

    };
    $scope.ChangeToOn = function () {

        $scope.taskSchedule.IsOn = true;
        $scope.taskSchedule.IsNever = false;
        $scope.taskSchedule.IsAfter = false;

    };

    //repeatType for monthly
    $scope.ChangeToRepeatByDay = function () {

        $scope.taskSchedule.isRepeatByDay = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForMonthly = false;

    };
    $scope.ChangeToRepeatByTheNthWeekForMonthly = function () {

        $scope.taskSchedule.isRepeatByTheNthWeekForMonthly = true;
        $scope.taskSchedule.isRepeatByDay = false;

    };

    //repeatType for yearly 
    $scope.ChangeToRepeatByTheNthWeek = function () {

        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = true;
        $scope.taskSchedule.isRepeatByDay = false;
        $scope.taskSchedule.isRepeatByTheMonth = false;

    };
    //repeatType for yearly 
    $scope.ChangeToRepeatByTheNthWeekForYearly = function () {

        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = true;
        $scope.taskSchedule.isRepeatByTheMonth = false;

    };
    $scope.ChangeToRepeatByTheMonth = function () {

        $scope.taskSchedule.isRepeatByTheMonth = true;
        $scope.taskSchedule.isRepeatByTheNthWeekForYearly = false;

    };

    $scope.SaveReccuringData = function () {
        var x = '';
        if ($scope.dayList.length > 0) {
            for (var i = 0; i < $scope.dayList.length; i++) {
                if ($scope.dayList[i].isChecked == true) {
                    x += $scope.dayList[i].day + ',';
                }

            }
        }
        $scope.taskSchedule.WeeklyRepeatationBycommaSepDayName = x.slice(0, -1);

        $scope.SaveRecurring();
    }
    $scope.AssignTaskScheduleFromResponseData = function (taskScheduleFromResponse) {

        $scope.taskSchedule = taskScheduleFromResponse;
        var arr = taskScheduleFromResponse.WeeklyRepeatationBycommaSepDayName.split(",");
        if (arr.length > 0) {
            for (var i = 0; i < arr.length; i++) {
                for (var j = 0; j < $scope.dayList.length; j++) {
                    if ($scope.dayList[j].day === arr[i]) {
                        $scope.dayList[j].isChecked = true;
                        break;
                    }
                }
            }
        }
    }
    $scope.checkRepeatedStatus = function () {

        $scope.EveryRepeatedFlag = 'Day';
        $scope.taskSchedule.isWeekly = false;
        $scope.taskSchedule.isYearly = false;
        $scope.taskSchedule.isDaily = false;
        $scope.taskSchedule.EveryWeekDay = false;
        if ($scope.taskSchedule.RepeatType === 'Daily') {
            $scope.EveryRepeatedFlag = 'Day';
            $scope.taskSchedule.isDaily = true;
        }
        if ($scope.taskSchedule.RepeatType === 'Weekly') {
            $scope.EveryRepeatedFlag = 'Week';
            $scope.taskSchedule.isWeekly = true;
        }
        else if ($scope.taskSchedule.RepeatType === 'Monthly') {
            $scope.EveryRepeatedFlag = 'Month';
        }
        else if ($scope.taskSchedule.RepeatType === 'Yearly') {
            $scope.EveryRepeatedFlag = 'Year';
            $scope.taskSchedule.isYearly = true;
        }
        else if ($scope.taskSchedule.RepeatType === 'Every') {
            $scope.taskSchedule.isDaily = true;
            $scope.taskSchedule.EveryWeekDay = true;


        }
    }
    $scope.CreateTaskScheduleMessage = function (Schedule) {
        if (Schedule.RepeatType === 'Daily') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeat ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Day(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Weekly') {


            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeat ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Week(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + Schedule.EndDate;
            }
        }
        else if (Schedule.RepeatType === 'Monthly') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeat ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Month(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + 'occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.taskSchedule.isRepeatByDay == true) {
                $scope.taskSchedule.Details += 'Repeat On ' + Schedule.RepeatByDayNumber + ' day(s) of the month';
            }
            else if ($scope.taskSchedule.isRepeatByTheNthWeekForMonthly == true) {
                $scope.taskSchedule.Details += 'Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of the month';
            }
        }
        else if (Schedule.RepeatType === 'Yearly') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeat ' + Schedule.RepeatType;
            $scope.taskSchedule.Details += ' Every ' + Schedule.EveryInterval + ' Year(s) starting from ' + $filter("dateFiltering")(Schedule.StartDate);

            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }

            if ($scope.taskSchedule.isRepeatByTheMonth == true) {
                $scope.taskSchedule.Details += ' Repeat On ' + Schedule.RepeatByDayNumber + ' Day(s) of ' + Schedule.RepeatByMonth;
            }
            else if ($scope.taskSchedule.isRepeatByTheNthWeekForYearly == true) {
                $scope.taskSchedule.Details += ' Repeat On ' + Schedule.RepeatbyNthWeek + ' ' + Schedule.RepeatByWeek + ' of ' + Schedule.RepeatbyOfEarly;
            }

        }
        else if (Schedule.RepeatType === 'Every') {

            $scope.taskSchedule.Details = '';

            $scope.taskSchedule.Details += 'Repeat ' + Schedule.RepeatType + ' Week Day';
            $scope.taskSchedule.Details += 'Week Days starting from ' + $filter("dateFiltering")(Schedule.StartDate);
            if (Schedule.IsNever == true) {
                $scope.taskSchedule.Details += ' and Never End';
            }
            else if (Schedule.IsAfter == true) {
                $scope.taskSchedule.Details += ' and End After ' + Schedule.AfterNoOfAccurence + ' occurrence(s)';
            }
            else if (Schedule.IsOn == true) {
                $scope.taskSchedule.Details += 'and End On ' + $filter("dateFiltering")(Schedule.EndDate);
            }
        }


    }
    $scope.GetRecurringData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTaskSchedule?ToDoId=' + $scope.ToDoId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                try {
                    $scope.returnedTaskSchedule = {};
                    if (response.data.length > 0) {
                        $scope.returnedTaskSchedule = response.data[0];
                    }
                    else {
                        $scope.returnedTaskSchedule = Object.assign({}, $scope.taskScheduleMain)
                    }
                    $scope.AssignTaskScheduleFromResponseData($scope.returnedTaskSchedule);
                    $scope.checkRepeatedStatus();
                } catch (e) {
                    ;
                }

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });

    }
    $scope.SaveRecurring = function () {
        $scope.CreateTaskScheduleMessage($scope.taskSchedule);
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'CreateTaskSchedule',
                data: { taskSchedule: $scope.taskSchedule, ToDoId: $scope.ToDoId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.ToDoModel.Schedule = Object.assign({}, $scope.taskSchedule);
                    $scope.hideSchedulerPopUp();
                    ShowResult(response.data.Message, 'success');

                }

            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.showTaskSchedulerPopUp = function () {
        $scope.GetRecurringData();
        var eDialog = $("#dialogScheduleAA").data("ejDialog");
        eDialog.open();
    }

    $scope.hideSchedulerPopUp = function () {
        var eDialog = $("#dialogScheduleAA").data("ejDialog");
        eDialog.close();
    }
    $scope.ToDoIssueDisplayModel = {};
    $scope.DisplayIssueDetails = function () {

        if (baseService.isUndefinedOrNull($scope.ToDoIssueDisplayModel) == false) {
            var eDialog = $("#dialogViewIssueDetail").data("ejDialog");
            eDialog.open();

        }

    }
    $scope.isDisableIssueDetail = true;
    $scope.getssueDetails = function () {
        $scope.ToDoIssueDisplayModel = {};
        $scope.isDisableIssueDetail = true;
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'GetIssueDetail',
                data: { ToDoId: $scope.ToDoId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.ToDoIssueDisplayModel = response.data[0];
                    if (baseService.isUndefinedOrNull($scope.ToDoIssueDisplayModel) == false)
                        $scope.isDisableIssueDetail = false;
                }

            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }


    $scope.applyState = function (obj) {
        var gridObj = $("#PersistenceGrid").ejGrid("instance");
        gridObj.model.filterSettings.filteredColumns = obj.filterCol;
        gridObj.refreshContent();//Refresh the Grid to apply the saved settings 

        //update filter collection to cFilteredCols 
        var filterCols = gridObj.filterColumnCollection, fcol = [];
        for (var c = 0; c < filterCols.length; c++) {
            fcol.push(filterCols[c].field);
        }
        gridObj._excelFilter.cFilteredCols = fcol;

        //refresh the header to update the icons 
        gridObj.refreshHeader();

    }
    $scope.CurrentSelectedFilterButton = 0;
    $scope.MainFilter = function (index) {


        $scope.CurrentSelectedFilterButton = index;

        var selectedItem = $scope.TaskinstantFilterMain[index];
        var gridObj = $("#GridIssueTransaction").ejGrid("instance");
        gridObj.model.filterSettings.filteredColumns = [];
        switch (selectedItem.value) {
            case 'All':

                gridObj.model.filterSettings.filteredColumns = [{ field: "Id", operator: "notequal", value: '', predicate: "and", matchcase: false }];
                break;
            case 'OverDue':
                gridObj.model.filterSettings.filteredColumns = [{ field: "DueDateFilter", operator: "lessthan", value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()), predicate: "and", matchcase: true },
                { field: "CurrentStatus", operator: "notequal", value: 'ToClose', predicate: "and", matchcase: false }];
                break;
            case 'Today':
                gridObj.model.filterSettings.filteredColumns = [{ field: "DueDateFilter", operator: "equal", value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()), predicate: "and", matchcase: true }];
                break;
            case 'ThisWeek':
                var _currentDate = new Date();
                var numberOfDaysToAdd = 9;
                _currentDate.setDate(_currentDate.getDate() + numberOfDaysToAdd);

                gridObj.model.filterSettings.filteredColumns = [{ field: "DueDateFilter", operator: "greaterthan", value: new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()), predicate: "and", matchcase: true }
                    , { field: "DueDateFilter", operator: "lessthan", value: new Date(_currentDate.getFullYear(), _currentDate.getMonth(), _currentDate.getDate()), predicate: "and", matchcase: true }];
                break;
            case 'Future':
                var _currentDate = new Date();
                var numberOfDaysToAdd = 9;
                _currentDate.setDate(_currentDate.getDate() + numberOfDaysToAdd);

                gridObj.model.filterSettings.filteredColumns = [{ field: "DueDateFilter", operator: "greaterthanorequal", value: new Date(_currentDate.getFullYear(), _currentDate.getMonth(), _currentDate.getDate()), predicate: "and", matchcase: true }];
                break;
            case 'ToClose':
                gridObj.model.filterSettings.filteredColumns = [{ field: "CurrentStatus", operator: "equal", value: 'ToClose', predicate: "and", matchcase: false }];
                break;
            case 'HighPriority':
                gridObj.model.filterSettings.filteredColumns = [{ field: "TaskPriority", operator: "greaterthanorequal", value: $scope.HighPriority, predicate: "and", matchcase: false }];
                break;
            case 'Unread':
                gridObj.model.filterSettings.filteredColumns = [{ field: "IsRead", operator: "equal", value: false, predicate: "and", matchcase: false }];
                break;
            case 'ToCloseReview':
                gridObj.model.filterSettings.filteredColumns = [{ field: "CurrentStatus", operator: "equal", value: 'ToClose', predicate: "and", matchcase: false }];
                break;
            case 'CloseWithoutReview':
                gridObj.model.filterSettings.filteredColumns = [{ field: "CurrentStatus", operator: "notequal", value: 'ToClose', predicate: "and", matchcase: false }];
                break;
            default:
                break;
        }
    }

    $scope.AdditionalFilterExec = function () {

        var gridObj = $("#GridIssueTransaction").ejGrid("instance");

        try {
            gridObj.model.filterSettings.enableComplexBlankFilter = false;
            gridObj.model.filterSettings.enableInterDeterminateState = false;

            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.AssignedBy) == false)
                gridObj.model.filterSettings.filteredColumns.push({ field: "CreatedById", operator: "equal", value: $scope.AdditionalFilter.AssignedBy, predicate: "and", matchcase: false });

            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.AssignedTo) == false) {
                gridObj.model.filterSettings.filteredColumns.push({ field: "AssignToId", iscomplex: true, operator: "equal", value: $scope.AdditionalFilter.AssignedTo, predicate: "and", matchcase: false });
            }
            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.DueDate) == false) {
                gridObj.model.filterSettings.filteredColumns.push({ field: "DueDateFilter", operator: "equal", value: new Date(new Date($scope.AdditionalFilter.DueDate).getFullYear(), new Date($scope.AdditionalFilter.DueDate).getMonth(), new Date($scope.AdditionalFilter.DueDate).getDate()), predicate: "and", matchcase: true });
            }
            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.DepartmentId) == false) {
                gridObj.model.filterSettings.filteredColumns.push({ field: "DepartmentId", operator: "equal", value: $scope.AdditionalFilter.DepartmentId, predicate: "and", matchcase: false });
            }
            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.CommitmentDate) == false)
                gridObj.model.filterSettings.filteredColumns.push({ field: "CommitmentDateFilter", operator: "equal", value: new Date(new Date($scope.AdditionalFilter.CommitmentDate).getFullYear(), new Date($scope.AdditionalFilter.CommitmentDate).getMonth(), new Date($scope.AdditionalFilter.CommitmentDate).getDate()), predicate: "and", matchcase: true });

            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.CurrentStatus) == false)
                gridObj.model.filterSettings.filteredColumns.push({ field: "CurrentStatus", operator: "contains", value: $scope.AdditionalFilter.CurrentStatus, predicate: "and", matchcase: false });

            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.Category) == false)
                gridObj.model.filterSettings.filteredColumns.push({ field: "TaskCategory", operator: "contains", value: $scope.AdditionalFilter.Category, predicate: "and", matchcase: false });

            if (baseService.isUndefinedOrNull($scope.AdditionalFilter.SubCategory) == false)
                gridObj.model.filterSettings.filteredColumns.push({ field: "TaskSubCategory", operator: "contains", value: $scope.AdditionalFilter.SubCategory, predicate: "and", matchcase: false });
        } catch (e) {

        }
    }

    $scope.HighPriority = 4.5;
    $scope.taskCountForFilter = function () {


        //first normat filter
        var All = $scope.issueTransactionList.length; var HighPriority = 0; var unread = 0; var OverDue = 0; var Today = 0; var Future = 0; var ThisWeek = 0; var ToClose = 0; var ToCloseReview = 0; var CloseWithoutReview = 0;

        var _currentDate = new Date();
        var numberOfDaysToAdd = 9;
        _currentDate.setDate(_currentDate.getDate() + numberOfDaysToAdd);
        try {
            for (var i = 0; i < $scope.issueTransactionList.length; i++) {
                 
                if ($scope.issueTransactionList[i].DueDateFilter < new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate()))
                    if ($scope.issueTransactionList[i].CurrentStatus != 'ToClose')
                        OverDue++;



                if (new Date($scope.issueTransactionList[i].DueDateFilter).getFullYear() == new Date().getFullYear()
                    && new Date($scope.issueTransactionList[i].DueDateFilter).getMonth() == new Date().getMonth()
                    && new Date($scope.issueTransactionList[i].DueDateFilter).getDate() == new Date().getDate()
                ) {
                    Today++;
                }

                if ($scope.issueTransactionList[i].DueDateFilter > new Date(new Date().getFullYear(), new Date().getMonth(), new Date().getDate())
                    && $scope.issueTransactionList[i].DueDateFilter < new Date(_currentDate.getFullYear(), _currentDate.getMonth(), _currentDate.getDate()))
                    ThisWeek++;

                if ($scope.issueTransactionList[i].DueDateFilter >= new Date(_currentDate.getFullYear(), _currentDate.getMonth(), _currentDate.getDate()))
                    Future++;

                if ($scope.issueTransactionList[i].CurrentStatus == 'ToClose') {
                    ToClose++; ToCloseReview++;
                }

                if ($scope.issueTransactionList[i].CurrentStatus != 'ToClose') {
                    CloseWithoutReview++;
                }

                if ($scope.issueTransactionList[i].TaskPriority >= $scope.HighPriority) {
                    HighPriority++;
                }

                if ($scope.issueTransactionList[i].IsRead == 0) {
                    unread++;
                }


            }
        } catch (e) {

        }

        //second approval filter
        try {
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {
                switch ($scope.TaskinstantFilterMain[i].value) {
                    case 'All':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + All + ')';
                        break;
                    case 'OverDue':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + OverDue + ')';
                        break;
                    case 'Today':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + Today + ')';
                        break;
                    case 'ThisWeek':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + ThisWeek + ')';
                        break;
                    case 'Future':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + Future + ')';
                        break;
                    case 'ToClose':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + ToClose + ')';
                        break;
                    case 'HighPriority':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + HighPriority + ')';
                        break;
                    case 'ToCloseReview':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + ToCloseReview + ')';
                        break;
                    case 'CloseWithoutReview':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + CloseWithoutReview + ')';
                        break;
                    case 'Unread':
                        $scope.TaskinstantFilterMain[i].text = $scope.TaskinstantFilterMain[i].basetext + '(' + unread + ')';
                        break;
                    default:
                        break;
                }
            }


            var gridObj = $("#menuPane").data("ejGrid");
            var selectedRows = gridObj.getSelectedRows()[0].rowIndex;


            var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
            //groupButtonObj.items[0].innerText = 'tarek talukder';

            //groupButtonObj.hideItem(element);
            //var data = groupButtonObj.model.dataSource;
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {
                var element = $("#groupButtonForFilter").find('li')[i];
                element.children[0].children[1].textContent = $scope.TaskinstantFilterMain[i].text;
            }

            //$scope.TaskinstantFilter = [];
            //$scope.TaskinstantFilter.push(Object.assign({}, $scope.TaskinstantFilterMain[0]));
            for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

                var element = $("#groupButtonForFilter").find('li')[i];
                groupButtonObj.showItem(element);
                if ($scope.TaskinstantFilterMain[i].value == 'ToCloseReview' || $scope.TaskinstantFilterMain[i].value == 'CloseWithoutReview') {
                    groupButtonObj.hideItem(element);
                }
            }

            if ($scope.menuitem[selectedRows].authorizationType == 'CreatedBy') {
                for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

                    var element = $("#groupButtonForFilter").find('li')[i];
                    if ($scope.TaskinstantFilterMain[i].value == 'Unread') {
                        groupButtonObj.hideItem(element);
                    }
                }

            }

            if ($scope.menuitem[selectedRows].authorizationType == 'AssignTo') {
                for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {

                    var element = $("#groupButtonForFilter").find('li')[i];
                    if ($scope.TaskinstantFilterMain[i].value == 'ToClose') {
                        groupButtonObj.hideItem(element);
                    }
                }

            }


            if ($scope.menuitem[selectedRows].authorizationType == 'CheckBy'
                || $scope.menuitem[selectedRows].authorizationType == 'CrossCheckBy'
                || $scope.menuitem[selectedRows].authorizationType == 'ApproveBy'
            ) {

                var ReviewIndex = 0;
                for (var i = 0; i < $scope.TaskinstantFilterMain.length; i++) {
                    var element = $("#groupButtonForFilter").find('li')[i];
                    groupButtonObj.hideItem(element);

                    if ($scope.TaskinstantFilterMain[i].value == 'ToCloseReview'
                        || $scope.TaskinstantFilterMain[i].value == 'CloseWithoutReview'
                        || $scope.TaskinstantFilterMain[i].value == 'Unread') {
                        {
                            groupButtonObj.showItem(element);
                            if (ReviewIndex == 0)
                                ReviewIndex = i;
                        }
                    }
                }
                var args = {
                    index: $scope.CurrentSelectedFilterButton
                };

                $scope.ToDoFilter(args)
            }
            else {

                var args = {
                    index: $scope.CurrentSelectedFilterButton
                };

                $scope.ToDoFilter(args);
            }


            var gridObj = $("#GridIssueTransaction").ejGrid("instance");
            gridObj.refreshContent(true);


        } catch (e) {

        }


    }

    $scope.ToDoFilter = function (args) {


        var gridObj = $("#GridIssueTransaction").ejGrid("instance");
        try {
            var grid = $("#GridIssueTransaction").ejGrid("instance");
            grid.clearFiltering();


            gridObj.model.filterSettings.filteredColumns = obj.filterCol;
            gridObj.refreshContent(true);

        } catch (e) {

        }

        $scope.MainFilter(args.index);


        gridObj._excelFilter._predicates = [];
        gridObj.refreshContent(true);
        //gridObj.refreshHeader();
        //gridObj.setWidthToColumns();

    }

    $scope.DepartmentList = [];
    $scope.AdditionalFilterMain = { AssignedBy: '', AssignedTo: '', DueDate: null, CommitmentDate: null, CurrentStatus: '', Category: '', SubCategory: '', DepartmentId: null };
    $scope.AdditionalFilter = Object.assign({}, $scope.AdditionalFilterMain);
    $scope.ClearAdditionalFilterScreen = function () {
        $scope.AdditionalFilter = Object.assign({}, $scope.AdditionalFilterMain);
    }
    $scope.showAdditionalFilter = function () {
        var eDialog = $("#dialogAdditionalFilter").data("ejDialog");
        eDialog.open();

    }
    $scope.SubmitAdditionalFilter = function () {
        var eDialog = $("#dialogAdditionalFilter").data("ejDialog");
        eDialog.close();


        var gridObj = $("#GridIssueTransaction").ejGrid("instance");
        try {
            var grid = $("#GridIssueTransaction").ejGrid("instance");
            grid.clearFiltering();


            gridObj.model.filterSettings.filteredColumns = obj.filterCol;
            gridObj.refreshContent(true);

        } catch (e) {

        }

        //$scope.CurrentSelectedFilterButton = $scope.TaskinstantFilterMain.length - 1;
        //var groupButtonObj = $("#groupButtonForFilter").ejGroupButton('instance');
        //var element = $("#groupButtonForFilter").find('li')[$scope.CurrentSelectedFilterButton];
        //groupButtonObj.selectItem(element);

        $scope.MainFilter($scope.CurrentSelectedFilterButton);
        $scope.AdditionalFilterExec();




        gridObj._excelFilter._predicates = [];
        gridObj.refreshContent(true);


    }
    $scope.TaskListToolbarClick = function (args) {
        if (args.itemId == "GridIssueTransaction_AdditionalFilter") {
            $scope.showAdditionalFilter();
        }
        if (args.itemId == "GridIssueTransaction_ClearAdditionalFilter") {
            $scope.DefaultFilter();
        }

    }
    $scope.DefaultFilter = function () {
        var gridObj = $("#GridIssueTransaction").ejGrid("instance");
        gridObj.model.filterSettings.filteredColumns = [];
        gridObj._excelFilter._predicates = [];
        gridObj.refreshContent(true);


        $scope.MainFilter($scope.CurrentSelectedFilterButton);
        gridObj.refreshContent(true);
    }
    $scope.AutoCompleteEmployeeInfo = "<div style=\"width:300px;height:35px;\"><div class=\"col-xs-12 col-sm-2 col-md-2 text-left\" style=\"padding:0px;margin:0px;\"><img onerror=\"$(arguments[0].currentTarget).attr('src','images/blankuser.png')\" src=\"POPResources/EmployeeProfiles/EmpPic/${EmpPicPath}\" style=\"float:left;position:absolute; height:40px;width:40px;border:2px solid #efefef; border-radius:50%;\" /></div><div class=\"col-xs-12 col-sm-10 col-md-10  text-left\" style=\"padding-left:4px;margin:0px;\"><div class=\"row\" style=\"padding:0px;margin:0px;\">${EmployeeName}</div> </div> </div>";
    $http({
        method: 'GET',
        url: $scope.path + "GetMasterDataForFilter"
    }).then(function successCallback(response) {

        $scope.DepartmentList = response.data.Department;
        $scope.ProcessList = response.data.Process;


    });




    $scope.ScrollHeightChat = 0;
    $('#wtfChatScroll').scroll(function (event) {
        if ($scope.ScrollHeightChat == $('#wtfChatScroll')[0].scrollHeight)
            return;
        $scope.ScrollHeightChat = $('#wtfChatScroll')[0].scrollHeight;
        $("#wtfChatScroll").animate({
            scrollTop: $('#wtfChatScroll')[0].scrollHeight - $('#wtfChatScroll')[0].clientHeight
        }, 500);
    });

    $scope.ChatScroll = function (args) {
        try {
            $('#wtfChatScroll').scrollTop($('#wtfChatScroll')[0].scrollHeight - $('#wtfChatScroll')[0].clientHeight);

            args.cell.style.background = "#ffffff";


        } catch (e) {

        }
    }

    $scope.chatParticipants = [];
    $scope.UnreadChatList = 0;
    $scope.chatActionButtons = ["close"];// ["close", "collapsible", "maximize", "minimize", "pin"];
    $scope.ChatText = '';
    $scope.UnReadChatCount = 0;
    $scope.CurrentChat = [];
    $scope.chatList = [];
    $scope.chatterList = [];
    $scope.ChatMasterId = '';
    $scope.GetAllChatList = function () {

        $scope.chatList = [];
        $http({
            method: 'POST', url: $scope.path + 'GetAllChatForList', dataType: 'JSON',

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.chatList = response.data.AllChats;
                $scope.UnReadChatCount = 0;
                for (var i = 0; i < $scope.chatList.length; i++) {
                    if ($scope.chatList[i].IsRead == false)
                        $scope.UnReadChatCount++;
                }
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.LoadChatFromThread = function (args) {

        $http({
            method: 'POST', url: $scope.path + 'GetAllChatForList', dataType: 'JSON',

        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.chatList = response.data.AllChats;
                $scope.UnReadChatCount = 0;
                for (var i = 0; i < $scope.chatList.length; i++) {
                    if ($scope.chatList[i].IsRead == false)
                        $scope.UnReadChatCount++;
                }
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.ReadChatByChatMasterId = function (ChatMasterId) {

        $http({
            method: 'POST', url: $scope.path + 'ReadChatByEmployeeId', dataType: 'JSON',
            data: { ChatMasterId: $scope.ChatMasterId }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                //args.data.IsRead = true;

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.ReadChatByThreadId = function (args) {

        $http({
            method: 'POST', url: $scope.path + 'ReadChatByThreadId', dataType: 'JSON',
            data: { ChatMasterId: args.data.ChatMasterId }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                args.data.IsRead = true;

            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }


    $scope.openSingleChatWindow = function (args) {
        var eDialog = $("#dialogChat").data("ejDialog");
        $("#dialogChat").ejDialog("setTitle", args.data.EmployeeName);

        eDialog.open();
        $scope.CreateChatThreadForSingle(args.data.Id);
    }
    $scope.CreateChatThreadForSingle = function (EmployeeId) {

        $scope.chatParticipants = [];
        $http({
            method: 'POST', url: $scope.path + 'CreateSingleChatMaster', dataType: 'JSON',
            data: { ToId: EmployeeId }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.ChatMasterId = response.data.ChatId;
                $scope.CurrentChat = response.data.CurrentChat;
                $scope.chatParticipants = response.data.chatParticipants;

                var getRow = $filter("filter")($scope.UserList, { "Id": EmployeeId });
                if (getRow.length > 0) {
                    // $scope.UnreadCommentList.push(Task);
                    getRow[0].UnreadChat = '';
                    getRow[0].UnreadChatDateCreated = '';
                    getRow[0].UnreadChatCount = 0;

                    $scope.countTotalUnread();
                }


                $scope.GetAllChat();
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.GetAllChat = function () {


        $http({
            method: 'POST', url: $scope.path + 'GetAllChat', dataType: 'JSON',
            data: { ChatMasterId: $scope.ChatMasterId }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.chatList = response.data.SingleChat;
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.countTotalUnread = function () {
        $scope.UnReadChatCount = 0;
        for (var i = 0; i < $scope.UserList.length; i++) {
            $scope.UnReadChatCount += $scope.UserList[i].UnreadChatCount;
        }
    }
    $scope.AddSingleChat = function () {

        if ($scope.ChatText.trim() == '')
            return;

        $http({
            method: 'POST', url: $scope.path + 'CreateChat', dataType: 'JSON',
            data: { ChatMasterId: $scope.ChatMasterId, ChatMessage: $scope.ChatText }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult('error', 'failure');
            }
            else {
                $scope.ChatText = '';
                $scope.CurrentChat.push(response.data.SingleChat[0]);

                for (var i = 0; i < $scope.chatParticipants.length; i++) {
                    signalR.Hub.invoke("SendChat", $scope.chatParticipants[i].EmployeeId, response.data.SingleChat[0]);
                }
            }
        }, function errorCallback(response) {
            ShowResult('Failed', 'failure');
        });
    }
    $scope.ChatWindowClose = function () {
        $scope.ChatMasterId = '';
    }
}

