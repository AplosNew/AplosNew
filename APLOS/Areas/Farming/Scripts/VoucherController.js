'use strict';
VoucherController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function VoucherController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Voucher';
    $scope.VoucherList = [];
    $scope.VoucherChildList = [];
  
    $scope.ICSMasterList = [];
    $scope.CropPlanningList = [];
    $scope.LocationList = [];
    $scope.CustomerList = [];
    $scope.FarmerList = [];
    $scope.FarmerFatherHusbandNameList = [];
    $scope.FarmerRegistrationList = [];
   
    $scope.path = 'Farming/Voucher/';

    $scope.getListUrl = $scope.path + 'getlist';

 //   $scope.saveUrl = $scope.path + 'create';
 
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "Date"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'PreparedById', name: "Prepared By" }, { value: 'Date', name: "Date" }, { value: 'Time', name: "Time" }, { value: 'ApprovedById', name: "Approved By" }];
 

    // #region ddl


 
        $http({
            method: 'GET',
            url: 'Farming/PurchaseBookingSoda/geticsmaster/',
        }).then(function successCallback(response) {
            $scope.ICSMasterList = response.data;
        });

    // #end region

    $scope.SearchVoucher = function () {
        $scope.VoucherChildList = [];
        $http({
            method: 'POST',
            data: { FromDate: $scope.Voucher.FromDate, ToDate: $scope.Voucher.ToDate, ICSMasterID: $scope.Voucher.ICSMasterID },
            url: $scope.path + 'getvoucherlist'
        }).then(function successCallback(response) {
            $scope.VoucherChildList = response.data;
        });
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VoucherList = response.data;
            ClearFields();
            
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
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        FromDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ToDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Time: _Time,
        PreparedById: null,
        ApprovedById: null,
        CustomerId: null,
        ICSMasterID: null,
        EmployeeStatus: null,
        EmpStatus: null
};
    $scope.Voucher = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.Voucher = Object.assign({}, args.data);
        $scope.Voucher.Time = $scope.Voucher.VoucherTime;
        $scope.Voucher.Date = $scope.Voucher.VoucherDate;
        $scope.GetVoucherGeneratedList();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();   
        }
    };

    $scope.GetVoucherGeneratedList = function () {
        $scope.VoucherChildList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getvouchergeneratedlist?Id=' + $scope.Voucher.Id
        }).then(function successCallback(response) {
            $scope.VoucherChildList = response.data;
            if (baseService.arrayLength(response.data) > 0) {   
            }
        });
    }


    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.VoucherList = response.data;
         
        });
    }

    $scope.Clear = function () {
        ClearFields();
       
        return true;
    };

    function ClearFields() {
    //    $scope.Action = 'Save';
        $scope.Voucher = Object.assign({}, $scope.ModelTemp);
        $scope.VoucherChildList = [];
    }

    ///////*********************Tabs*******************************

    //Save Function 
  
    $scope.GenerateVoucher = function () {
        $scope.$broadcast('show-errors-check-validity');  
        if ($scope.General.$valid) {
            var checkedData = [];
            for (var i = 0; i < $scope.VoucherChildList.length; i++) {
                //if ($scope.VoucherChildList[i].isSelected == true)
                checkedData.push($scope.VoucherChildList[i]);
                var IsVoucher = true;
            }
            try {
                if (checkedData.length == 0) {
                    throw 'First Search the Voucher Details';
                }
                $http({
                    method: 'POST',
                    data: { VoucherChildData: checkedData, data: $scope.Voucher, IsVoucherData: IsVoucher},
                    url: $scope.path + 'Create'
                   
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.Getgrid();
                  //      $scope.getData();
                    }
                    });
            }
            catch (e) {
                ShowResult(e, "failure");
            }

        }
    }

    // #region ResPerson field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.Voucher.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.Voucher.PreparedById = null;
        $scope.Voucher.ResponsiblePerson = null;
        $scope.Voucher.EmployeeCode = null;
        $scope.Voucher.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.Voucher.EmployeeCode = data.Code;
        $scope.Voucher.PreparedById = data.Id;
        $scope.Voucher.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    // #region Approve By field

    $scope.EmpApproveByList = [];
    $scope.ApproveByPopUp = function () {
        angular.element(document.querySelector("#EmpPopUpResPerson")).modal("show");
        $scope.getEmpApproveByDetailsData();

    }
    $scope.getEmpApproveByDetailsData = function () {
        $scope.EmpApproveByList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.Voucher.Id },
            url: $scope.path + 'LoadAllEmpApproveByDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpApproveByList = response.data;
        });
    }

    $scope.ApproveByClear = function () {
        $scope.Voucher.ApprovedById = null;
        $scope.Voucher.EmpName = null;
        $scope.Voucher.EmpCode = null;
        $scope.Voucher.EmpStatus = null;
    };
    $scope.closeEmpApproveByPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpApproveByData = function (obj) {

        var data = obj.data;
        $scope.Voucher.EmpCode = data.Code;
        $scope.Voucher.ApprovedById = data.Id;
        $scope.Voucher.EmpName = data.EmployeeName;
        angular.element(document.querySelector('#EmpPopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

}