'use strict';
OrderWiseQualityReportController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function OrderWiseQualityReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "OrderWiseQualityReport";
    $scope.Action = 'Save';
    $scope.GradeLists = [];
    //$scope.CriticalLevelLists = [];
    //$scope.CriticalLevelGridLists = [];
    $scope.path = 'QMS/OrderWiseQualityReport/';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrlUpd = 'GridReports/ExcelExportUpd';
    $scope.saveUrlComments = $scope.path + 'createComments';
    //$scope.saveUrlUCPDValue = $scope.path + 'createUCPRequirement';
    //$scope.ParameterStatusLists = [];
    //var date = new Date(), y = date.getFullYear(), m = date.getMonth();
    //date.setDate(date.getDate() - 3);

    $scope.OrderWiseQualityReportList = [];
    $scope.View = function () {
        try {
            $scope.OrderWiseQualityReportList = [];
            $http.get('QMS/OrderWiseQualityReport/LoadOrderWiseQualityReport')
                .then(function (response) {
                    $scope.OrderWiseQualityReportList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.View();

    $scope.CommentEntryLists = [];
    $scope.MOId = null;
    $scope.POId = null;
    $scope.Lot = null;
    $scope.CommentEntry = function (data) {
        $scope.CommentEntryLists = [];
        $scope.NewObject = data.data;
        $scope.MOId = $scope.NewObject.MOLineItemNo;
        $scope.POId = $scope.NewObject.PONo;
        $scope.Lot = $scope.NewObject.LotNumber;

        try {
            $http.get('QMS/OrderWiseQualityReport/getCommentEntryData?MOLineItemNo=' + $scope.NewObject.MOLineItemNo + '&PONo=' + $scope.NewObject.PONo + '&LotNo=' + $scope.NewObject.LotNumber)
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
                'LotNumber': $scope.Lot
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

    //$scope.ParameterStatusLists = [
    //    {
    //        'Value': 'Pending',
    //        'Text': 'Pending'
    //    },
    //    {
    //        'Value': 'ToApprove',
    //        'Text': 'ToApprove'
    //    }
    //];

    //$scope.CriticalLevelLists = [
    //    {
    //        'Value': 'High',
    //        'Text': 'High'
    //    },
    //    {
    //        'Value': 'Very High',
    //        'Text': 'Very High'
    //    },
    //    {
    //        'Value': 'Medium',
    //        'Text': 'Medium'
    //    },
    //    {
    //        'Value': 'Low',
    //        'Text': 'Low'
    //    }
    //];

    //$scope.CriticalLevelGridLists = [
    //    {
    //        'Value': 'High',
    //        'Text': 'High'
    //    },
    //    {
    //        'Value': 'Very High',
    //        'Text': 'Very High'
    //    },
    //    {
    //        'Value': 'Medium',
    //        'Text': 'Medium'
    //    },
    //    {
    //        'Value': 'Low',
    //        'Text': 'Low'
    //    }
    //];

    //$scope.status = {
    //    Id: null,
    //    ParameterStatus: null,
    //};
    //$scope.statusNew = Object.assign({}, $scope.status);

    //$scope.CustomerUpdatePara = {
    //    Id: null,
    //    LineItemNo: null,
    //    EmployeeId: null,
    //    ApprovedById: null,
    //    CriticalLevel: null,
    //    Remarks: null,
    //    ApprovalStatus:null
    //};
    //$scope.CustomerUpdateParaNew = Object.assign({}, $scope.CustomerUpdatePara);

    //$scope.ParameterResponsiblePersonLists = [];
    //$scope.GetParameterResponsiblePersonLists = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/OrderWiseQualityReport/GetParameterResponsiblePersonLists'
    //    }).then(function successCallback(response) {
    //        $scope.ParameterResponsiblePersonLists = response.data;
    //    });
    //}
    //$scope.GetParameterResponsiblePersonLists();

    //$scope.ParameterApprovalPersonLists = [];
    //$scope.GetParameterApprovalPersonLists = function () {
    //    $http({
    //        method: 'GET',
    //        url: 'QMS/OrderWiseQualityReport/GetParameterApprovalPersonLists'
    //    }).then(function successCallback(response) {
    //        $scope.ParameterApprovalPersonLists = response.data;
    //    });
    //}
    //$scope.GetParameterApprovalPersonLists();

    //$scope.selectGridResponsible = function (data) {
    //    $scope.Newobject = data.data;
    //    $scope.getEmployee();
    //    angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    //}

    //$scope.EmployeeList = [];
    //$scope.getEmployee = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'GetEmployee',
    //        dataType: 'JSON'
    //    }).then(function succ(resp) {
    //        $scope.EmployeeList = resp.data;
    //    });
    //}

    //$scope.doubleEmployee = function (e) {
    //    $scope.Newobject.ResponsiblePersonId = e.data.SystemId;
    //    $scope.Newobject.ResponsiblePerson = e.data.EmployeeName;
    //    angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    //}

    //$scope.closeResponsiblePersonPopUp = function () {
    //    angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    //}

    
    //$scope.GetDetails = function ($event) {
    //    $scope.CustomerUpdateParaNew.LineItemNo = $event.data.LineItemNo;
    //    $scope.CustomerUpdateParaNew.Id = $event.data.Id;
    //    $scope.CustomerUpdateParaNew.EmployeeId = $event.data.EmployeeId;
    //    $scope.CustomerUpdateParaNew.ApprovedById = $event.data.ApprovedById;
    //    $scope.CustomerUpdateParaNew.CriticalLevel = $event.data.CriticalLevel;
    //    $scope.CustomerUpdateParaNew.Remarks = $event.data.Remarks;
    //    $scope.GetParameterResponsiblePersonLists();
    //    $scope.GetParameterApprovalPersonLists();
    //    $scope.CUPDList = [];
    //    //$scope.loadCUPD();
    //    angular.element(document.querySelector('#UpdateCustomerParameterPopUp')).modal('show');
    //}
    //$scope.UCPId = null;
    //$scope.CustomerUpdateSave = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.saveUrlCustomerUpdatePara,
    //        data: {
    //            'CustomerUpdateParaData': $scope.CustomerUpdateParaNew,
    //            'ApprovalStatus':  'ToApprove'
    //        },
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.UCPId = response.data.Data.Id;
    //            $scope.loadCUPD();
    //            $scope.View();
    //        }
    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    }
    //};

    //$scope.CUPDList = [];
    //$scope.loadCUPD = function () {
    //    try {
    //        $http.get('QMS/OrderWiseQualityReport/GetCRPCbo?MasterId=' + $scope.UCPId + '&LineItemNo=' + $scope.CustomerUpdateParaNew.LineItemNo)
    //            .then(function (response) {
    //                $scope.CUPDList = response.data;
    //            });
    //    } catch (ex) {
    //        ShowResult(ex, 'Info');
    //    }
    //};

    //$scope.SaveUCPD = function (data) {
    //    try {
    //        if (baseService.isUndefinedOrNull(data.data.MinRequirement) && baseService.isUndefinedOrNull(data.data.MaxRequirement)) {
    //            throw "Please enter requirement and proceed";
    //        }
    //        data.data.UCPId = $scope.UCPId;
    //        $http({
    //            method: 'POST',
    //            url: $scope.saveUrlUCPDValue,
    //            data: { 'UCPRequirementDetailsData': data.data },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.loadCUPD();
    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    } catch (e) {
    //        ShowResult(e, 'failure');

    //    }
    //};
}

