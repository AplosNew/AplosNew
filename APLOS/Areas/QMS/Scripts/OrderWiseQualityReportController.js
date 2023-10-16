'use strict';
OrderWiseQualityReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function OrderWiseQualityReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "OrderWiseQualityReport";
    $scope.Action = 'Save';
    $scope.GradeLists = [];
    $scope.PartyNatureLists = [];
    $scope.path = 'QMS/OrderWiseQualityReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.saveUrlComments = $scope.path + 'createComments';
    var todate = new Date(), y = todate.getFullYear(), m = todate.getMonth();
    todate.setDate(todate.getDate() - 7);

    $scope.OrderWiseQualityReportList = [];
    $scope.View = function () {
        try {
            $scope.OrderWiseQualityReportList = [];
            $http.get('QMS/OrderWiseQualityReport/LoadOrderWiseQualityReport?FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&PartyNature=' + $scope.statusNew.PartyNature + '&EntityId=' + $scope.statusNew.EntityId)
                .then(function (response) {
                    $scope.OrderWiseQualityReportList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    //$scope.View();

    $scope.CommentEntryLists = [];
    $scope.MOId = null;
    $scope.POId = null;
    $scope.Lot = null;
    $scope.EI = null;
    $scope.CommentEntry = function (data) {
        $scope.CommentEntryLists = [];
        $scope.NewObject = data.data;
        $scope.MOId = $scope.NewObject.MOLineItemNo;
        $scope.POId = $scope.NewObject.PONo;
        $scope.Lot = $scope.NewObject.LotNumber;
        $scope.EI = $scope.NewObject.EntityId;

        try {
            $http.get('QMS/OrderWiseQualityReport/getCommentEntryData?MOLineItemNo=' + $scope.NewObject.MOLineItemNo + '&PONo=' + $scope.NewObject.PONo + '&LotNo=' + $scope.NewObject.LotNumber  +'&EntityId=' + $scope.NewObject.EntityId)
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
            $http.get('QMS/OrderWiseQualityReport/getCommentData?MOId=' + $scope.MOId + '&POId=' + $scope.POId + '&LotNo=' + $scope.Lot )
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
            url: 'QMS/OrderWiseQualityReport/LoadCommentEntryEditData?CommentId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.CommentsNew = response.data.comment[0];
        }
        )
    }

    $scope.Comments = {
        Id: null
        , MOLineItemNo: null
        , PONo: null
        , LotNo: null
        , Comment: null
        , ByWhomId: null
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
            url: 'QMS/OrderWiseQualityReport/GetEntityLists'
        }).then(function successCallback(response) {
            $scope.EntityLists = response.data;
        });
    }
    $scope.GetEntityLists();

    $scope.ByWhomLists = [];
    $scope.GetByWhomLists = function () {
        $http({
            method: 'GET',
            url: 'QMS/OrderWiseQualityReport/GetByWhomLists'
        }).then(function successCallback(response) {
            $scope.ByWhomLists = response.data;
        });
    }
    $scope.GetByWhomLists();

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
            url: 'QMS/OrderWiseQualityReport/CommentsDelete?id=' + $scope.CommentsNew.Id,
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
        $scope.ParameterNew.Comment = $scope.NewObject.Comment;
        try {
            $http.get('QMS/OrderWiseQualityReport/getOrderWiseParameterData?IssueId=' + $scope.NewObject.IssueId + '&ProductionOrderId=' + $scope.NewObject.PONo + '&LotNumber=' + $scope.NewObject.LotNumber + '&FromDate=' + $scope.statusNew.FromDate + '&ToDate=' + $scope.statusNew.ToDate + '&EntityId=' + $scope.NewObject.EntityId)
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

    $scope.OWQReport = function () {
        var dataList = [];
        var g = $("#GridOrderWiseQualityReport").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.OrderWiseQualityReportList;
        }

        $scope.fileName = "Order Wise Quality Report";

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
}

