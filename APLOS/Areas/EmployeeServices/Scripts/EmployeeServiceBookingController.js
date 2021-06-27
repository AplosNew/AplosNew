'use strict';
EmployeeServiceBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeServiceBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Service Booking';
    $scope.EmployeeServiceBookingList = [];
    $scope.SelectedQuantityList = [];
    $scope.SelectedAmountList = [];
    $scope.SelectedReadingList = [];

    $scope.ShiftIdList = [];
    $scope.CategoryIdList = [];
    $scope.EmployeeServicesList = [];

    $scope.UOMList = [];

    $scope.path = 'EmployeeServices/EmployeeServiceBooking/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    baseService.init($scope.getListUrl);

    $scope.searchBy = "Service"; $scope.search = "";


    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'EmployeeId', name: "Employee" }, { value: 'EmployeeServiceCategoryId', name: "Category" }, { value: 'UOMId', name: "UOM" }, { value: 'Service', name: "Service" }];


    // #region ddl

    //$scope.uOMList = [];
    //cboService.getUoMCbo(function (response) {
    //    $scope.UOMList = response;
    //});

    $http({
        method: 'GET',
        url: 'EmployeeServices/EmployeeServiceBooking/getshift/'
    }).then(function successCallback(response) {
        $scope.ShiftIdList = response.data;
    });

    $http({
        method: 'GET',
        url: 'EmployeeServices/EmployeeServiceBooking/getservices/'
    }).then(function successCallback(response) {
        $scope.EmployeeServicesList = response.data;
    });

    $scope.GetCategoryList = function () {
        $scope.CategoryIdList = [];
        $http({
            method: 'GET',
            url: 'EmployeeServices/EmployeeServiceBooking/getcategory?serviceid=' + $scope.EmployeeServiceBooking.EmployeeServicesId + '&CatId=' + $scope.EmployeeServiceBooking.EmployeeServiceCategoryId
        }).then(function successCallback(response) {
            $scope.CategoryIdList = response.data;
            
            });
        $scope.GetUOM();
    }

    $scope.GetUOM = function () {
        $scope.UOMList = [];
        $http({
            method: 'GET',
            url: 'EmployeeServices/EmployeeServiceBooking/getuom?serviceid=' + $scope.EmployeeServiceBooking.EmployeeServicesId
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
            if (baseService.arrayLength($scope.UOMList) > 0) {
                $scope.EmployeeServiceBooking.UOMId = response.data[0].Value;
                $scope.EmployeeServiceBooking.UOM = response.data[0].Text;
            }
        });
    }

    $scope.GetDuplicateData = function () {
        $scope.DuplicateDataList = [];
        $http({
            method: 'POST',
            data: { ServicesId: $scope.EmployeeServiceBooking.EmployeeServicesId, CategoryId: $scope.EmployeeServiceBooking.EmployeeServiceCategoryId, Date: $scope.EmployeeServiceBooking.Date, EmployeeId: $scope.EmployeeServiceBooking.EmployeeId },
            url: 'EmployeeServices/EmployeeServiceBooking/getduplicatedata'
        }).then(function successCallback(response) {
            $scope.DuplicateDataList = response.data;
            if ($scope.DuplicateDataList.length > 0)
               alert("Same Date,Service, Category and Employee already exists!!");
        });
    }

    $scope.GetForm = function () {
        $scope.FormList = [];
        $http({
            method: 'POST',
            data: { CategoryId: $scope.EmployeeServiceBooking.EmployeeServiceCategoryId, ServicesId: $scope.EmployeeServiceBooking.EmployeeServicesId },
            url: 'EmployeeServices/EmployeeServiceBooking/getform'
        }).then(function successCallback(response) {
            $scope.FormList = response.data;
            if (baseService.arrayLength($scope.FormList) > 0) {
       
                $scope.EmployeeServiceBooking.Form = response.data[0].Form;
                if ($scope.EmployeeServiceBooking.Form == "Quantity") {
                    $scope.GetGridQuantityDataToShow();
                }
                if ($scope.EmployeeServiceBooking.Form == "Value") {
                    $scope.GetGridAmountDataToShow();
                }
                if ($scope.EmployeeServiceBooking.Form == "Reading") {
                    $scope.GetGridReadingDataToShow();
                }
            }
        });
    }

    $scope.GetReadingQuantity = function () {
        try {
            var From = parseFloat($scope.EmployeeServiceBooking.From);
            var To = parseFloat($scope.EmployeeServiceBooking.To);
            if (To > From) {
                var RQuantity = To - From;
                $scope.EmployeeServiceBooking.Quantity = RQuantity;
            }
            else {
                throw 'To should be greater than From';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.GetParticulars = function () {
        try {
            if ($scope.EmployeeServiceBooking.Chargeable == false) {
                throw 'Please Enter Particulars / Remarks';
            }
            else {
                $scope.EmployeeServiceBooking.Chargeable = true;
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeServiceBookingList = response.data;
            ClearFields();
            ClearFieldsForms();
        });
    }
    $scope.getData();

    var d = new Date();

    var hh = d.getHours();
    var mm = d.getMinutes();
    mm = (mm < 10 ? '0' + mm : mm);
    var ss = d.getSeconds()

    //   var _Time = hh + ":" + mm + ":" + ss;
    var _Time = hh + ":" + mm;

    $scope.ModelTemp = {
        Id: null,
        EmployeeId: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        UOMId: null,
        Time: _Time,
        ShiftId: null,
        EmployeeServiceCategoryId: null,
        Chargeable: true,
        From: null,
        To: null,
        Quantity: null,
        Amount: null,
        Particulars: null,
        BillOtherReferenceNo: null,
        Form: null,
        EmpName: null,
        EmployeeCode: null,
        EmployeeStatus: null,
        Service: null,
        EmployeeServicesId: null,
        UOM: null,
        IsProcessed: false,
    };
    $scope.EmployeeServiceBooking = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.EmployeeServiceBooking = Object.assign({}, args.data);
        $scope.EmployeeServiceBooking.Date = $scope.EmployeeServiceBooking.EmpServiceDate;
        $scope.EmployeeServiceBooking.Time = $scope.EmployeeServiceBooking.GetTime;
        $scope.GetCategoryList();
 //       $scope.GetUOM();
        $scope.GetForm();
        if ($scope.EmployeeServiceBooking.Chargeable == "True") {
            $scope.EmployeeServiceBooking.Chargeable = true;
        }
        else {
            $scope.EmployeeServiceBooking.Chargeable = false;
        }
        ClearFieldsForms();
  //      $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeServiceBookingList = response.data;

        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $scope.GetDuplicateData();
            $scope.GetParticulars();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.EmployeeServiceBooking },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.EmployeeServiceBooking = response.data.Data;
                    //     $scope.Action = 'Update';
                    $scope.Getgrid();
                    $scope.GetGridQuantityDataToShow();
                    $scope.GetGridAmountDataToShow();
                    $scope.GetGridReadingDataToShow();
                    ClearFieldsForms();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsForms() {
        $scope.Action = 'Save';
        $scope.EmployeeServiceBooking.Id = null;
        $scope.EmployeeServiceBooking.EmployeeId = null;
        $scope.EmployeeServiceBooking.Chargeable = true;
        $scope.EmployeeServiceBooking.From = null;
        $scope.EmployeeServiceBooking.To = null;
        $scope.EmployeeServiceBooking.Quantity = null;
        $scope.EmployeeServiceBooking.Amount = null;
        $scope.EmployeeServiceBooking.Particulars = null;
        $scope.EmployeeServiceBooking.BillOtherReferenceNo = null;
        $scope.EmployeeServiceBooking.EmpName = null;
        $scope.EmployeeServiceBooking.EmployeeCode = null;
        $scope.EmployeeServiceBooking.EmployeeStatus = null;

    }

    $scope.Clear = function () {
        ClearFields();

        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.EmployeeServiceBooking = Object.assign({}, $scope.ModelTemp);
    }

    // Employee POP up

    $scope.EmpList = [];
    $scope.EmpPopUp = function () {
        angular.element(document.querySelector("#EmployeePop")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.EmployeeServiceBooking.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;
        });
    }

    $scope.EmpClear = function () {
        $scope.EmployeeServiceBooking.EmployeeId = null;
        $scope.EmployeeServiceBooking.EmpName = null;
        $scope.EmployeeServiceBooking.EmployeeCode = null;
        $scope.EmployeeServiceBooking.EmployeeStatus = null;
    };
    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmployeeData = function (obj) {

        var data = obj.data;
        $scope.EmployeeServiceBooking.EmployeeCode = data.Code;
        $scope.EmployeeServiceBooking.EmployeeId = data.Id;
        $scope.EmployeeServiceBooking.EmpName = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    };
    // # end region  Employee

    $scope.GetGridQuantityDataToShow = function () {
        $scope.SelectedQuantityList = [];
        $http({
            method: 'POST',
            data: { ServicesId: $scope.EmployeeServiceBooking.EmployeeServicesId, CategoryId: $scope.EmployeeServiceBooking.EmployeeServiceCategoryId, Date: $scope.EmployeeServiceBooking.EmpServiceDate },
            url: 'EmployeeServices/EmployeeServiceBooking/getgriddatatoshow'
        }).then(function successCallback(response) {
            $scope.SelectedQuantityList = response.data;
        });
    }

    $scope.GetGridAmountDataToShow = function () {
        $scope.SelectedAmountList = [];
        $http({
            method: 'POST',
            data: { ServicesId: $scope.EmployeeServiceBooking.EmployeeServicesId, CategoryId: $scope.EmployeeServiceBooking.EmployeeServiceCategoryId, Date: $scope.EmployeeServiceBooking.EmpServiceDate },
            url: 'EmployeeServices/EmployeeServiceBooking/getgriddatatoshow'
        }).then(function successCallback(response) {
            $scope.SelectedAmountList = response.data;
        });
    }

    $scope.GetGridReadingDataToShow = function () {
        $scope.SelectedReadingList = [];
        $http({
            method: 'POST',
            data: { ServicesId: $scope.EmployeeServiceBooking.EmployeeServicesId, CategoryId: $scope.EmployeeServiceBooking.EmployeeServiceCategoryId, Date: $scope.EmployeeServiceBooking.EmpServiceDate },
            url: 'EmployeeServices/EmployeeServiceBooking/getgriddatatoshow'
        }).then(function successCallback(response) {
            $scope.SelectedReadingList = response.data;
        });
    }

    $scope.DelQuantity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelQuantity?Id=' + $scope.QuantityId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.GetGridQuantityDataToShow();
            }

        });
    }

    $scope.ConfirmDeleteQuantityTab = function (Id) {
        $scope.QuantityId = Id;
        angular.element(document.querySelector("#DeleteQuantity")).modal("show");
    }

    $scope.DelAmount = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelAmount?Id=' + $scope.AmountId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.GetGridAmountDataToShow();
            }

        });
    }

    $scope.ConfirmDeleteAmount = function (Id) {
        $scope.AmountId = Id;
        angular.element(document.querySelector("#DeleteAmount")).modal("show");
    }

    $scope.DelReading = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelReading?Id=' + $scope.ReadingId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.GetGridReadingDataToShow();
            }

        });
    }

    $scope.ConfirmDeleteReading = function (Id) {
        $scope.ReadingId = Id;
        angular.element(document.querySelector("#DeleteReading")).modal("show");
    }

}