'use strict';
specialIssueControlUpdateController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function specialIssueControlUpdateController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "SpecialIssueControlUpdate";
    $scope.Action = 'Save';
    $scope.path = 'Machines/SpecialIssueControlUpdate/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveIssueItemUpdateUrl = $scope.path + 'createItem';
    var CurrentTime = new Date();

    $scope.ShiftList = [];
    $scope.GetShiftList = function () {
        $http({
            method: 'GET',
            url: 'Machines/SpecialIssueControlUpdate/GetShiftList'
        }).then(function successCallback(response) {
            $scope.ShiftList = response.data;
        });
    }
    $scope.GetShiftList();

    
    $scope.issueupdate = {
        Id: null
        , Date: $filter('dateFiltering')(new Date(), 'dd-MM-yyyy')
        , Shift: null
        , ShiftInchargeId: null
        , ShiftIncharge: null
        , IssueId:null
        , Issue: null
        , Time: CurrentTime
        , Remarks: null
    };
    $scope.issueupdateNew = Object.assign({}, $scope.issueupdate);

    $scope.Item = {
        Id: null
        , SpecialIssueItem: null
        , Actiontaken: null
        , ActiontakenById: null
        , ActiontakenBy: null
        , SampleSize: null
        , Remarks: null
        , Value: null
        , SpecialIssueControlId:null
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    //$scope.IssueControlUpdateList = [];
    //$scope.LoadIssueControlUpdateList = function () {
    //    $http({
    //        method: 'Get',
    //        url: 'Machines/SpecialIssueControlUpdate/LoadIssueControlUpdateList'
    //    }).then(function successCallback(response) {
    //        $scope.IssueControlUpdateList = response.data;
    //        var gridObj = $("#GridIssueControlUpdate").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    //    });
    //}
    //$scope.LoadIssueControlUpdateList();

    $scope.IssueItemDetailsList = [];
    $scope.GetIssueItemPopup = function () {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlUpdate/LoadIssueItemDetailsList?IssueId=' + $scope.issueupdateNew.IssueId
        }).then(function successCallback(response) {
            $scope.IssueItemDetailsList = response.data;
            var gridObj = $("#GridUpdateIssueItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            //angular.element(document.querySelector('#IssueItemPopup')).modal('show');
        }
        )
    }

    $scope.refreshTemplateIssueItem = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllIssueItem });
    };
    function CheckBoxSelectAllIssueItem(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridUpdateIssueItem").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.IssueItemDetailsList.length; i++) {
                $scope.IssueItemDetailsList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridUpdateIssueItem").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
    };

    $scope.closeIssueItemPopUp = function () {
        angular.element(document.querySelector('#IssueItemPopup')).modal('hide');
    }

   
    $scope.selectEmployee = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ShifInChargePopup')).modal('show');
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
        $scope.issueupdateNew.ShiftInchargeId = e.data.SystemId;
        $scope.issueupdateNew.ShiftInCharge = e.data.EmployeeName;
        angular.element(document.querySelector('#ShifInChargePopup')).modal('hide');
    }

    $scope.closeShifInChargePopUp = function () {
        angular.element(document.querySelector('#ShifInChargePopup')).modal('hide');
    }

    $scope.selectIssue = function () {
        $scope.getIssue();
        angular.element(document.querySelector('#IssuePopup')).modal('show');
    }

    $scope.IssueList = [];
    $scope.getIssue = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetIssue',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.IssueList = resp.data;
        });
    }

    $scope.doubleIssue = function (e) {
        $scope.issueupdateNew.IssueId = e.data.Id;
        $scope.issueupdateNew.Issue = e.data.SpecialIssueName;
        angular.element(document.querySelector('#IssuePopup')).modal('hide');
    }

    $scope.closeIssuePopUp = function () {
        angular.element(document.querySelector('#IssuePopup')).modal('hide');
    }

    $scope.CheckValidValue = function () {
        for (var i = 0; i < $scope.IssueItemDetailsList.length; i++)
        {
            if (baseService.isUndefinedOrNull($scope.IssueItemDetailsList[i].Value)) {

                throw "Value is required";
            }
        }
    }

    $scope.Save = function () {
        try {
            $scope.CheckValidValue();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.SpecialIssueControlUpdateForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'IssueUpdateData': $scope.issueupdateNew },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        //ShowResult(response.data.Message, 'success');
                        $scope.SaveIssueUpdateItemDetails(response.data.Data.Id);
                        //$scope.LoadIssueControlUpdateList();
                        //IssueUpdateClearFields();


                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        }
        catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SaveIssueUpdateItemDetails = function (Id) {
        try {

            $scope.SaveList = [];
            for (var i = 0; i < $scope.IssueItemDetailsList.length; i++) {
                    $scope.SaveList.push($scope.IssueItemDetailsList[i]);
            }

            $http({
                method: 'POST',
                url: $scope.saveIssueItemUpdateUrl,
                data: {
                    "DataList": $scope.SaveList,
                    'Pid': Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    ShowResult(response.data.Message, 'success');
                    $scope.Action = 'Save';
                }

            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    $scope.GetICUDetails = function (args) {
        $http({
            method: 'Get',
            url: 'Machines/SpecialIssueControlUpdate/LoadICUEditData?ICUId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.issueupdateNew = response.data.issueupdate[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
   
    $scope.Clear = function () {
        IssueUpdateClearFields();
    };
    $scope.ItemClear = function () {
        ItemClearFields();
    };
   
    function IssueUpdateClearFields() {
        $scope.Action = "Save";
        $scope.issueupdateNew = Object.assign({}, $scope.issueupdate);
        $scope.IssueItemDetailsList = [];
    }

    function ItemClearFields() {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.item);  
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
   
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'Machines/SpecialIssueControl/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.issueNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
    
    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'Machines/SpecialIssueControl/IssueDelete?id=' + $scope.issueNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadSpecialIssueMasterList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
}