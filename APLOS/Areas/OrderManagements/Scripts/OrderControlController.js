'use strict';
OrderControlController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function OrderControlController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Order Control";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.masterDataList = [];
    $scope.SOList = [];
    $scope.PRList = [];
    $scope.RemarkList = [];
    $scope.path = 'OrderManagements/OrderControl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveMasterUrl = $scope.path + 'CreateMaster';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.OrderControl = {
        Id: null, ControlTypeId: null, SalesOrderId: null, ProductionOrderId: null, Remarks: null, CriticalityLevel: null, Status: null
    }
    $scope.OrderControlNew = Object.assign({}, $scope.OrderControl);

    $scope.OrderControlRemarks = { Id: null, OrderControlId: null, Remarks: null, AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null };

    $scope.controlTypeList = [];
    $http.get('OrderManagements/OrderControl/GetControlTypeCbo')
        .then(function (response) {
            $scope.controlTypeList = response.data;
        });

    $scope.SOCriticalityLevelList = [
        { Value: "Critical", Text: "Critical" },
        { Value: "Important", Text: "Important" },
        { Value: "Normal", Text: "Normal" }
    ];

    $scope.StatusList = [
        { Value: "Active", Text: "Active" },
        { Value: "InProgress", Text: "InProgress" },
        { Value: "Complete", Text: "Complete" },
        { Value: "Closed", Text: "Closed" },
        { Value: "Pending", Text: "Pending" }
    ];

    $scope.rowDataBoundOrder = function (e) {
        try {

            try {

                var Fyear = new Date(e.data.Date).getFullYear();
                var FMonth = new Date(e.data.Date).getMonth();
                var FDay = new Date(e.data.Date).getDate();

                var Curyear = new Date().getFullYear();
                var CurMonth = new Date().getMonth();
                var CurDay = new Date().getDate();

                var CurrDate = new Date(Curyear, CurMonth, CurDay);
                var FDate = new Date(Fyear, FMonth, FDay);

                if (FDate < CurrDate)
                    e.row.css("background-color", '#FFAC9A');//red
                else if (FDate > CurrDate)
                    e.row.css("background-color", '#FDFFB8');//yellow
                else //(FDate == CurrDate)
                    e.row.css("background-color", '#ADCAFF');//blue



            } catch (e) {

            }
            //if ($scope.MaterialID != e.data.ArticleId) {
            //    $scope.isAlternative = $scope.isAlternative * -1;
            //    $scope.MaterialID = e.data.ArticleId;
            //}
            //if ($scope.isAlternative > 0)
            //    e.row.css("background-color", '#fff6b7');
            //else
            //    e.row.css("background-color", '#d1e5ff');
        } catch (e) {

        }

    }
    $scope.Day = null;
    $scope.LagDays = null;
    $scope.Level = null;
    $scope.GetDayAndLevel = function () {
        $scope.SOList = [];
        $scope.PRList = [];
        if (!baseService.isUndefinedOrNull($scope.OrderControl.ControlTypeId)) {

            $scope.Day = $.grep($scope.controlTypeList, function (item) {
                return item.Id === $scope.OrderControl.ControlTypeId;
            })[0].Day;

            $scope.LagDays = $.grep($scope.controlTypeList, function (item) {
                return item.Id === $scope.OrderControl.ControlTypeId;
            })[0].LagDays;
            $scope.Level = $("#ControlTypeId option:selected").text();

            if ($scope.Level === 'ShipmentControl') {
                $http.get('OrderManagements/OrderControl/GetSalesOrderData?day=' + $scope.Day + '&lagdays=' + $scope.LagDays + '&level=' + $scope.Level)
                    .then(function (response) {
                        $scope.SOList = response.data;

                        for (var k = 0; k < $scope.SOList.length; k++) {
                            for (var i = 0; i < $scope.CriticalityLevelList.length; i++) {
                                if (i == 2) {
                                    $scope.SOList[k].CriticalityLevel = $scope.CriticalityLevelList[i].Value;
                                }
                            }
                        }
                    });

            }
            else {
                $http.get('OrderManagements/OrderControl/GetProductoinOrderData?day=' + $scope.Day + '&lagdays=' + $scope.LagDays + '&level=' + $scope.Level)
                    .then(function (response) {
                        $scope.PRList = response.data;

                        for (var j = 0; j < $scope.PRList.length; j++) {
                            for (var i = 0; i < $scope.CriticalityLevelList.length; i++) {
                                if (i == 2) {
                                    $scope.PRList[j].CriticalityLevel = $scope.CriticalityLevelList[i].Value;
                                }

                            }
                        }

                    });

            }
        }

    };

    $scope.GetRemarksByMaster = function () {
        $scope.RemarkList = [];
        $http.get('OrderManagements/OrderControl/GetRemarksByMaster?OrderControlId=' + $scope.OrderControlNew.Id)
            .then(function (response) {

                for (var i = 0; i < response.data.length; i++) {
                    try {
                        response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                    } catch (e) {

                    }
                }



                $scope.RemarkList = response.data;
            });
    }

    $scope.SaveRemarks = function (model) {
        try {
            $scope.OrderControlNew.ControlTypeId = $scope.OrderControl.ControlTypeId;
            $http({
                method: 'POST',
                data: { data: $scope.OrderControlNew, child: model.data },
                url: 'OrderManagements/OrderControl/Create'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDayAndLevel();
                    $scope.GetRemarksByMaster($scope.OrderControlNew.Id);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.DeleteRemarks = function (model) {
        try {

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: 'OrderManagements/OrderControl/Delete'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRemarksByMaster($scope.OrderControlNew.Id);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.Save = function () {
        try {
            if ($scope.Level === 'ShipmentControl') {
                for (var i = 0; i < $scope.SOList.length; i++) {
                    $scope.SOList[i].ControlTypeId = $scope.OrderControl.ControlTypeId;
                }
                $scope.$broadcast('show-errors-check-validity');
                if ($scope.OrderControlForm.$valid) {
                    $http({
                        method: 'POST',
                        url: $scope.saveMasterUrl,
                        data: {
                            'data': $scope.SOList
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetDayAndLevel();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
            else {
                for (var i = 0; i < $scope.PRList.length; i++) {
                    $scope.PRList[i].ControlTypeId = $scope.OrderControl.ControlTypeId;
                }
                $scope.$broadcast('show-errors-check-validity');
                if ($scope.OrderControlForm.$valid) {
                    $http({
                        method: 'POST',
                        url: $scope.saveMasterUrl,
                        data: {
                            'data': $scope.PRList
                        },
                        dataType: 'JSON'
                        , contentType: "application/json charset=utf-8"
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetDayAndLevel();
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.showRemarksPopUp = function (args) {
        $scope.OrderControlNew = args;
        $scope.GetRemarksByMaster($scope.OrderControlNew.Id);
        angular.element(document.querySelector('#RemarksPopUp')).modal('show');
    }

    $scope.closeRemarksPopUp = function () {

        angular.element(document.querySelector('#RemarksPopUp')).modal('hide');
    }
}
