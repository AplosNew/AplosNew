'use strict';
masterOrderUploadController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function masterOrderUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Master Order Upload";

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    //  #region Master Order Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'OrderManagements/MasterOrder/GetMOSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.ShowSaveBtn = false;
    $scope.moData = [];
    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/ImportMOData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.moData = [];
                        $scope.moData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.SaveMOData = function () {
        try {
            $scope.ShowSaveBtn = true;
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveMOData',
                data: { 'dataList': $scope.moData },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = true;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.moData = [];
                    $("#uploadImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            $scope.ShowSaveBtn = false;
        }
    };
    //  #endregion SaveMOData Upload Download

    $scope.fileNew = { CompanyId: null, MasterOrderNo: null, ItemNo:null }

    $scope.companyList = [];
    $scope.GetCompanyCboList = function () {
        try {

            $http({
                method: 'Get',
                url: 'OrderManagements/masterorder/GetCompanyCboList'
            }).then(function successCallback(response) {
                $scope.companyList = response.data;
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.GetCompanyCboList();

    $scope.SearchColumn = 'MasterOrderNo';
    $scope.SearchValue = null;

    $scope.modelFilterByList = [
        { 'name': 'Creation Date', 'value': 'AddedDate' },
        { 'name': 'Created By', 'value': 'AddedBy' },
        { 'name': 'Order Type', 'value': 'OrderType' },
        { 'name': 'Plant', 'value': 'UserName' },
        { 'name': 'Entity', 'value': 'Entity' },
        { 'name': 'Customer Name', 'value': 'CustomerName' },
        { 'name': 'Buyer', 'value': 'Buyer' },
        { 'name': 'Master Order No', 'value': 'MasterOrderNo' },
        { 'name': 'Order Category', 'value': 'OrderCategory' },
        { 'name': 'Order Year', 'value': 'OrderYear' },
        { 'name': 'Total Qty', 'value': 'TotalQty' },
        { 'name': 'Line Item No', 'value': 'NoOfLineItem' },
        { 'name': 'Responsible Person', 'value': 'ResponsiblePersonName' },
        { 'name': 'Bill To', 'value': 'InvoicingPartyPlant' },
        { 'name': 'Ship To', 'value': 'DeliveryPartyPlant' },
        { 'name': 'Buyer Ref. No-Item', 'value': 'BuyerReferenceNoItem' },
        { 'name': 'Own Item', 'value': 'OwnItem' },
        { 'name': 'Buyer Orde/Ref No', 'value': 'BuyerReferenceNo' },
        { 'name': 'Own Order/Ref No', 'value': 'OwnReferenceNo' }
    ];
    $scope.files = [];
    $scope.ShowMasterOrder = function () {
        $scope.files = [];
        if (!baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            $http({
                method: 'POST',
                data: {
                    'companyId': $scope.fileNew.CompanyId, 'column': $scope.SearchColumn, 'value': $scope.SearchValue
                },
                url: 'OrderManagements/masterorder/getlist'
            }).then(function successCallback(response) {
                $scope.files = response.data;
            });
            angular.element(document.querySelector('#MOPopUpNew')).modal('show');
        }
    };

    $scope.getData = function () {
        if (!baseService.isUndefinedOrNull($scope.fileNew.CompanyId)) {
            $http({
                method: 'POST',
                data: {
                    'companyId': $scope.fileNew.CompanyId, 'column': $scope.SearchColumn, 'value': $scope.SearchValue
                },
                url: 'OrderManagements/masterorder/getlist'
            }).then(function successCallback(response) {
                $scope.files = response.data;
            });
        }
    };

    $scope.Set = function (index) {
        $scope.index = index.data;
        $scope.fileNew.MasterOrderNo = $scope.index.Id;
        angular.element(document.querySelector('#MOPopUpNew')).modal('hide');
    }

    $scope.GetMOISampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'OrderManagements/MasterOrder/GetMOISampleFile?reportFormat=' + ReportFormat;
    };

    $("#uploadMOIImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.moiData = [];
    $scope.ImportMOIData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNew2Form.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/ImportMOIData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("fileNew", angular.toJson(data.fileNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.moiData = [];
                        $scope.moiData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.SaveMOIData = function () {
        try {
            $scope.ShowSaveBtn = true;
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveMOIData',
                data: { 'dataList': $scope.moiData, 'masterId': $scope.fileNew.MasterOrderNo },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = true;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.moiData = [];
                    $("#uploadMOIImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            $scope.ShowSaveBtn = false;
        }
    };

    $scope.moiList = [];
    $scope.ShowMasterOrderItem = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/MasterOrder/GetMasterItemList?masterOrderId=' + $scope.fileNew.MasterOrderNo
        }).then(function successCallback(response) {
            $scope.moiList = response.data;
            angular.element(document.querySelector('#MOIPopUpNew')).modal('show');
        });
    }

    $scope.SetMOI = function (index) {
        $scope.itemindex = index.data;
        $scope.fileNew.ItemNo = $scope.itemindex.Id;
        angular.element(document.querySelector('#MOIPopUpNew')).modal('hide');
    }

    $scope.GetSOSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = 'OrderManagements/MasterOrder/GetSOSampleFile?reportFormat=' + ReportFormat;
    };

    $("#uploadSOImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.soData = [];
    $scope.ImportSOData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNew3Form.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: 'OrderManagements/MasterOrder/ImportSOData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("fileNew", angular.toJson(data.fileNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.soData = [];
                        $scope.soData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    $scope.SaveSOData = function () {
        try {
            $scope.ShowSaveBtn = true;
            $http({
                method: 'POST',
                url: 'OrderManagements/MasterOrder/SaveSOData',
                data: { 'dataList': $scope.soData, 'masterId': $scope.fileNew.ItemNo },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.ShowSaveBtn = true;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.soData = [];
                    $("#uploadSOImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
            $scope.ShowSaveBtn = false;
        }
    };

}