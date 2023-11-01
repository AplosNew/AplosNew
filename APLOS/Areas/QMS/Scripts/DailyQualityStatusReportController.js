'use strict';
DailyQualityStatusReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function DailyQualityStatusReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "DailyQualityStatusReport";
    $scope.Action = 'Save';
    $scope.GradeLists = [];
    $scope.PartyNatureLists = [];
    $scope.path = 'QMS/DailyQualityStatusReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.saveUrlComments = $scope.path + 'createComments';
    var todate = new Date(), y = todate.getFullYear(), m = todate.getMonth();
    todate.setDate(todate.getDate() - 7);

    $scope.DailyQualityStatusReportList = [];
    $scope.View = function () {
        try {
            $scope.DailyQualityStatusReportList = [];
            $http.get('QMS/DailyQualityStatusReport/LoadDailyQualityStatusReport?FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&PartyNature=' + $scope.statusNew.PartyNature + '&EntityId=' + $scope.statusNew.EntityId)
                .then(function (response) {
                    $scope.DailyQualityStatusReportList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //$scope.View();

    $scope.CommentEntryLists = [];
    $scope.POId = null;
    $scope.Lot = null;
    $scope.EI = null;
    $scope.CommentEntry = function (data) {
        $scope.CommentEntryLists = [];
        $scope.NewObject = data.data;
        $scope.POId = $scope.NewObject.PONo;
        $scope.Lot = $scope.NewObject.LotNumber;
        $scope.EI = $scope.NewObject.EntityId;

        try {
            $http.get('QMS/DailyQualityStatusReport/getCommentEntryData?PONo=' + $scope.NewObject.PONo + '&LotNo=' + $scope.NewObject.LotNumber + '&EntityId=' + $scope.NewObject.EntityId)
                .then(
                    function successCallback(response) {
                        $scope.CommentEntryLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridCommentEntry").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#CommentEntryPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.getComment = function () {
        try {
            $http.get('QMS/DailyQualityStatusReport/getCommentData?POId=' + $scope.POId + '&LotNo=' + $scope.Lot)
                .then(
                    function successCallback(response) {
                        $scope.CommentEntryLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridCommentEntry").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetCommentEntryDetails = function (args) {
        $http({
            method: 'Get',
            url: 'QMS/DailyQualityStatusReport/LoadCommentEntryEditData?CommentId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.CommentsNew = response.data.comment[0];
        }
        )
    }

    $scope.Comments = {
        Id: null
        , PONo: null
        , LotNo: null
        , Comment: null
        , ByWhomId: null
        , ByWhom: null
        , Grade: null
        , EntityId: null
    }
    $scope.CommentsNew = Object.assign({}, $scope.Comments);

    $scope.GradeLists = [
        {
            'Value': 'Good',
            'Text': 'Good'
        },
        {
            'Value': 'Pass',
            'Text': 'Pass'
        },
        {
            'Value': 'Fail',
            'Text': 'Fail'
        },
        {
            'Value': 'Reject',
            'Text': 'Reject'
        }
    ];

    $scope.PartyNatureLists = [
        {
            'Value': 'Domestic',
            'Text': 'Domestic'
        },
        {
            'Value': 'Overseas',
            'Text': 'Overseas'
        },
        {
            'Value': 'Domestic,Overseas',
            'Text': 'Domestic,Overseas'
        }
    ];

    $scope.EntityLists = [];
    $scope.GetEntityLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/DailyQualityStatusReport/GetEntityLists'
        }).then(function successCallback(response) {
            $scope.EntityLists = response.data;
        });
    }
    $scope.GetEntityLists();

    $scope.selectByWhom = function () {
        $scope.getByWhom();
        angular.element(document.querySelector('#ByWhomPopup')).modal('show');
    }

    $scope.ByWhomList = [];
    $scope.getByWhom = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ByWhomList = resp.data;
        });
    }

    $scope.doubleByWhom = function (e) {
        $scope.CommentsNew.ByWhomId = e.data.SystemId;
        $scope.CommentsNew.ByWhom = e.data.EmployeeName;
        angular.element(document.querySelector('#ByWhomPopup')).modal('hide');
    }

    $scope.closeByWhomPopup = function () {
        angular.element(document.querySelector('#ByWhomPopup')).modal('hide');
    }


    $scope.SaveCommentsData = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrlComments,
            data: {
                'CommentsData': $scope.CommentsNew,
                'MOItem': $scope.MOId,
                'POId': $scope.POId,
                'LotNumber': $scope.Lot,
                'EntityId': $scope.EI

            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getComment();
                CommentClearFields();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

    $scope.CommentsClear = function () {
        CommentClearFields();
    };

    function CommentClearFields() {
        $scope.Action = "Save";
        $scope.CommentsNew = Object.assign({}, $scope.Comments);
    }

    $scope.CommentsDelete = function () {
        $http({
            method: 'POST',
            url: 'QMS/DailyQualityStatusReport/CommentsDelete?id=' + $scope.CommentsNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getComment();
                CommentClearFields();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Parameter = {
        QualityStatus: null
        , Date: null
        , MOLineItemNo: null
        , POStatus: null
        , PONo: null
        , LotNo: null
        , Article: null
        , Customer: null
        , Grade: null
        , Comment: null
        , IssueId: null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.ParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        $scope.ParameterNew.QualityStatus = $scope.NewObject.QualityStatus;
        $scope.ParameterNew.Date = $scope.NewObject.Date;
        $scope.ParameterNew.POStatus = $scope.NewObject.POStatus;
        $scope.ParameterNew.MOLineItemNo = $scope.NewObject.MOLineItemNo;
        $scope.ParameterNew.Article = $scope.NewObject.Article;
        $scope.ParameterNew.Customer = $scope.NewObject.Customer;
        $scope.ParameterNew.PONo = $scope.NewObject.PONo;
        $scope.ParameterNew.LotNo = $scope.NewObject.LotNumber;
        $scope.ParameterNew.Grade = $scope.NewObject.Grade;
        $scope.ParameterNew.ByWhom = $scope.NewObject.ByWhom;
        $scope.ParameterNew.Comment = $scope.NewObject.CommentDetails;
        try {
            $http.get('QMS/DailyQualityStatusReport/getDailyQualityStatusParameterData?ProductionOrderId=' + $scope.NewObject.PONo + '&LotNumber=' + $scope.NewObject.LotNumber + '&FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&EntityId=' + $scope.NewObject.EntityId)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.status = {
        Id: null,
        FromDate: $filter('dateFiltering')(todate, 'dd-MM-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy'),
        PartyNature: null,
        EntityId: null,
    };
    $scope.statusNew = Object.assign({}, $scope.status);

    $scope.DQSReport = function () {
        var dataList = [];
        var g = $("#GridDailyQualityStatusReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.DailyQualityStatusReportList;
        }

        $scope.fileName = "Daily Quality Status Report";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.OWParameterReport = function () {
        var dataList = [];
        var g = $("#GridParameter").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ParameterLists;
        }

        $scope.fileName = "Order Wise Parameter Report";

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrlUpd,
            data: { 'reportFileName': $scope.fileName, 'data': dataList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    $scope.DQSjobcardreportFunc = function () {
        try {
            var gridObj = $("#GridParameter").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            if (filtereddata.length == 0) {
                filtereddata = $scope.ParameterLists;
            }
            $scope.ParameterListsNew = [];
            for (var i = 0; i < filtereddata.length; i++) {
                    $scope.ParameterListsNew.push(filtereddata[i]);
            }

            if ($scope.ParameterListsNew.length > 50) {
                throw "Maximum 50 'Job card' can be downloded at a time";
            }
            else {
                var url = $scope.path + '/GetDailyQualityStatusParameterJobCardReport?fromDate=' + $scope.statusNew.FromDate + '&toDate=' + $scope.statusNew.ToDate + '&IssueId=' + $scope.NewObject.IssueId + '&ProductionOrderId=' + $scope.NewObject.PONo + '&LotNumber=' + $scope.NewObject.LotNumber + '&EntityId=' + $scope.NewObject.EntityId + '&QualityStatus=' + $scope.ParameterNew.QualityStatus + '&Date=' + $scope.ParameterNew.Date;
                $rootScope.report(url);
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.QualityStatus == 'Pending') {
            e.row.css("background-color", '#FFFDD0');
        }
        else if (e.data.QualityStatus == 'Pass') {

            e.row.css("background-color", '#90EE90');
        }
        else if (e.data.QualityStatus == 'Fail') {

            e.row.css("background-color", '#ffb38a');
        }
        else {
            e.row.css("background-color", '#F62817');

        }
    }

    $scope.PDrowDataBound = function PDrowDataBound(e) {

        if (e.data.ParameterGradeStatus == 'Pending') {
            e.row.css("background-color", '#FFFDD0');
        }
        if (e.data.ParameterGradeStatus == 'Reject') {
            e.row.css("background-color", '#F62817');
        }
        if (e.data.ParameterGradeStatus == 'Fail') {
            e.row.css("background-color", '#ffb38a');
        }
        //else {
        //    e.row.css("background-color", '#90EE90');

        //}
    }
}

