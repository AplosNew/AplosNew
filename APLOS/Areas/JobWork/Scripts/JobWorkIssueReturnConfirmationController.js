'use strict';
JobWorkIssueReturnConfirmationController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobWorkIssueReturnConfirmationController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    $scope.path = 'JobWork/JobWorkIssueReturnConfirmation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

    //////// Drop Down

    var d = new Date();
    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ConfirmationIssueModelTemp = {
        Id: null,
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Status: null,
        PartyId: null,
        PartyName: null,
        PartyCode: null,
    };
    $scope.ConfirmationIssue = Object.assign({}, $scope.ConfirmationIssueModelTemp);

    $scope.Disable = false;
    $scope.ConfirmationIssueChildList = [];
    $scope.Search = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ConfirmationIssueGeneralForm.$valid) {
            $scope.ConfirmationIssueChildList = [];
            $http({
                method: 'POST',
                data: { FromDate: $scope.ConfirmationIssue.FromDate, ToDate: $scope.ConfirmationIssue.ToDate, Status: $scope.ConfirmationIssue.Status, PartyId: $scope.ConfirmationIssue.PartyId },
                url: $scope.path + 'GetSearchedData',
            }).then(function successCallback(response) {
                $scope.ConfirmationIssueChildList = response.data;
                if ($scope.ConfirmationIssueChildList.length > 0) {
                    $scope.Disable = true;
                }
            });
        }
    }

    // #region Vendor/ Party field

    $scope.PositionList = [];
    $scope.PartyPopUp = function () {
        angular.element(document.querySelector("#PosPopUp")).modal("show");
        $scope.getPosDetailsData();

    }
    $scope.getPosDetailsData = function () {
        $scope.PositionList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.ConfirmationIssue.Id },
            url: $scope.path + 'LoadAllPartyDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        });
    }

    $scope.PartyClear = function () {
        $scope.ConfirmationIssue.PartyId = null;
        $scope.ConfirmationIssue.PartyName = null;
        $scope.ConfirmationIssue.PartyCode = null;
    };
    $scope.closePositionPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPositionData = function (obj) {
        var data = obj.data;
        $scope.ConfirmationIssue.PartyCode = data.Code;
        $scope.ConfirmationIssue.PartyId = data.Id;
        $scope.ConfirmationIssue.PartyName = data.UserName;
        angular.element(document.querySelector('#PosPopUp')).modal('hide');
    };
    // # end region

    $scope.Clear = function () {
        ClearFields();
        $scope.ConfirmationIssueChildList = [];
        $scope.Disable = false;
    };

    function ClearFields() {
        $scope.ConfirmationIssue = Object.assign({}, $scope.ConfirmationIssueModelTemp);
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ValidateQuantity = function (RowData) {
        try {
            
            for (var i = 0; i < $scope.ConfirmationIssueChildList.length > 0; i++) {
            
                if ($scope.ConfirmationIssueChildList[i].IssueChildId === RowData.IssueChildId) {
                    var IssuedQty = parseFloat($scope.ConfirmationIssueChildList[i].TotalIssuedQuantity)
                    var ConfQty = parseFloat(RowData.ConfirmedQty);
                   
                    if (ConfQty > IssuedQty) {
                        $scope.ConfirmationIssueChildList[i].ConfirmedQty = null;
                            throw 'Confirmed Issue Quantity cannot be greater than Total Issue Quantity';
                        }
                   }
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    //Save Function 
    $scope.SaveConfirmedIssueChildTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var checkedData = [];
        
        for (var i = 0; i < $scope.ConfirmationIssueChildList.length; i++) {
            if ($scope.ConfirmationIssueChildList[i].isSelected == true)
                checkedData.push($scope.ConfirmationIssueChildList[i]);
                $scope.ConfirmationIssue.IsConfirmed = true;
        }
        try {
            if (checkedData.length == 0) {
                throw 'Please Select at least one Confirmed Quantity';
            }
            $http({
                method: 'POST',
                data: { ConfirmedIssueChildData: checkedData, IsConfirmed: $scope.ConfirmationIssue.IsConfirmed },
                url: $scope.path + 'SaveConfirmedIssueChildTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.ConfirmationIssue = response.data.Data;
                    $scope.Clear();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    //  TRANSFORMATION CONFIRMATION ISSUE

    $scope.TransConfirmationIssueModelTemp = {
        Id: null,
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Status: null,
        PartyVendorId: null,
        PartyVendorName: null,
        PartyVendorCode: null,
    };
    $scope.TransConfirmationIssue = Object.assign({}, $scope.TransConfirmationIssueModelTemp);

    $scope.TransDisable = false;

    $scope.TransConfirmationIssueChildList = [];
    $scope.SearchTransConfirmationIssue = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TransConfirmationIssueGeneralForm.$valid) {
            $scope.TransConfirmationIssueChildList = [];
            $http({
                method: 'POST',
                data: { FromDate: $scope.TransConfirmationIssue.FromDate, ToDate: $scope.TransConfirmationIssue.ToDate, Status: $scope.TransConfirmationIssue.Status},
                url: $scope.path + 'GetSearchTransConfirmationIssue',
            }).then(function successCallback(response) {
                $scope.TransConfirmationIssueChildList = response.data;
                if ($scope.TransConfirmationIssueChildList.length > 0) {
                    $scope.TransDisable = true;
                }
            });
        }
    }

    // #region Vendor/ Party field

    //$scope.PartyVendorList = [];
    //$scope.PartyVendorPopUp = function () {
    //    angular.element(document.querySelector("#PartyVendorPopUp")).modal("show");
    //    $scope.getPartyVendordata();

    //}
    //$scope.getPartyVendordata = function () {
    //    $scope.PartyVendorList = [];
    //    $http({
    //        method: 'POST',
    //        data: { Id: $scope.TransConfirmationIssue.Id },
    //        url: $scope.path + 'LoadAllPartyVendorForSelection'
    //    }).then(function successCallback(response) {
    //        $scope.PartyVendorList = response.data;
    //    });
    //}

    //$scope.PartyVendorClear = function () {
    //    $scope.TransConfirmationIssue.PartyVendorId = null;
    //    $scope.TransConfirmationIssue.PartyVendorName = null;
    //    $scope.TransConfirmationIssue.PartyVendorCode = null;
    //};
    //$scope.closePartyVendorPopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}

    //$scope.setPartyVendorData = function (obj) {
    //    var data = obj.data;
    //    $scope.TransConfirmationIssue.PartyVendorCode = data.Code;
    //    $scope.TransConfirmationIssue.PartyVendorId = data.Id;
    //    $scope.TransConfirmationIssue.PartyVendorName = data.UserName;
    //    angular.element(document.querySelector('#PartyVendorPopUp')).modal('hide');
    //};
    // # end region


    //Save Function 
    $scope.SaveTransConfirmationIssueChildTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var FetchData = [];

        for (var i = 0; i < $scope.TransConfirmationIssueChildList.length; i++) {
            if ($scope.TransConfirmationIssueChildList[i].isSelected == true)
                FetchData.push($scope.TransConfirmationIssueChildList[i]);
                $scope.TransConfirmationIssue.IsConfirmed = true;
        }
        try {
            if (FetchData.length == 0) {
                throw 'Please Select at least one Confirmed Quantity';
            }
            $http({
                method: 'POST',
                data: { TransConfirmedIssueChildData: FetchData, IsConfirmed: $scope.TransConfirmationIssue.IsConfirmed },
                url: $scope.path + 'SaveTransConfirmationIssueChildTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.TransConfirmationIssue = response.data.Data;
                    $scope.ClearTransConfirmationIssue();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.ClearTransConfirmationIssue = function () {
        ClearFieldsTransChild();
        $scope.TransConfirmationIssueChildList = [];
        $scope.TransDisable = false;
    };

    function ClearFieldsTransChild() {
        $scope.TransConfirmationIssue = Object.assign({}, $scope.TransConfirmationIssueModelTemp);
    }

    $scope.ValidateTransQuantity = function (RowData) {
        try {

            for (var i = 0; i < $scope.TransConfirmationIssueChildList.length > 0; i++) {

                if ($scope.TransConfirmationIssueChildList[i].TransIssueChildId === RowData.TransIssueChildId) {
                    var IssuedQty = parseFloat($scope.TransConfirmationIssueChildList[i].TotalIssuedQuantity)
                    var ConfQty = parseFloat(RowData.TransConfirmedQty);

                    if (ConfQty > IssuedQty) {
                        $scope.TransConfirmationIssueChildList[i].TransConfirmedQty = null;
                        throw 'Confirmed Issue Quantity cannot be greater than Total Issue Quantity';
                    }
                }
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }

    }

    $scope.ConfirmPrintTab = function (data) {
        try {
            $scope.PrintTabId = data.TransContractId;
            $scope.IssueId = data.IssueId;
            var reportFormat = "Excel";
            window.open('JobWork/JobWorkIssueReturn/GetTransformationPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId + '&IssueId=' + $scope.IssueId, '_blank');
            //   $scope.getData();

        } catch (e) {

        }
    };

}