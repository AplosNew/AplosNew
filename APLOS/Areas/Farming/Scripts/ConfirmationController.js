'use strict';
ConfirmationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ConfirmationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Sauda Confirmation';
    $scope.PurchaseBookingSodaList = [];
  
    $scope.ICSMasterList = [];
    $scope.CropPlanningList = [];
    $scope.LocationList = [];
    $scope.CustomerList = [];
    $scope.FarmerList = [];
    $scope.FarmerFatherHusbandNameList = [];
    $scope.FarmerRegistrationList = [];

    $scope.path = 'Farming/Confirmation/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
 
    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "Date"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'LocationId', name: "Location" }, { value: 'Date', name: "Date" }, { value: 'Time', name: "Time" }, { value: 'CropPlanningId', name: "Crop Planning" }, { value: 'ValidationDate', name: "Validation Date" }, { value: 'CustomerId', name: "Customer" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'Farming/Confirmation/getlocation/',
    }).then(function successCallback(response) {
        $scope.LocationList = response.data;
        });
 

    //$scope.GetICSMaster = function () {
    //    $scope.ICSMasterList = [];
        $http({
            method: 'GET',
            url: 'Farming/Confirmation/geticsmaster/',
        }).then(function successCallback(response) {
            $scope.ICSMasterList = response.data;
           
        });
 //   }

    $http({
        method: 'GET',
        url: 'Farming/Confirmation/getcustomer/',
    }).then(function successCallback(response) {
        $scope.CustomerList = response.data; 
        });

    $scope.GetCropPlanning = function () {
        $scope.CropPlanningList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getcropplanning?ICSMasterId=' + $scope.PurchaseBookingSoda.ICSMasterID
        }).then(function successCallback(response) {
            $scope.CropPlanningList = response.data;
        });
    }

    $scope.GetFarmer = function () {
        $scope.FarmerList = [];
        $http({
            method: 'POST',
            data: { ICSMasterID: $scope.PurchaseBookingSoda.ICSMasterID, CropPlanningId: $scope.PurchaseBookingSoda.CropPlanningId },
            url: $scope.path + 'getfarmer'
        }).then(function successCallback(response) {
            $scope.FarmerList = response.data;
            //     $scope.PurchaseBookingSoda.Farmer = response.data[0].Text;
        });
    }

    $scope.GetFarmerFather = function () {
        $scope.FarmerFatherHusbandNameList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getfarmerfather?FarmerID=' + $scope.PurchaseBookingSoda.Farmer
        }).then(function successCallback(response) {
            $scope.FarmerFatherHusbandNameList = response.data;
        });
    }
    $scope.GetFarmerRegistrationId = function () {
        $scope.FarmerRegistrationList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getfarmerregistrationid?FarmerFatherID=' + $scope.PurchaseBookingSoda.FarmerFatherHusbandNameId
        }).then(function successCallback(response) {
            $scope.FarmerRegistrationList = response.data;
        });
    }
  

    $scope.GetValidationDate = function () {
        var selecteddate = new Date($scope.PurchaseBookingSoda.Date);
        $http({
            method: 'GET',
            url: 'Farming/Confirmation/getvalidationenumvalue/',
        }).then(function successCallback(response) {
            $scope.ValidationEnumValue = response.data;
            var VEnumValue = response.data[0].Text;           
        var valdate = selecteddate.setDate(selecteddate.getDate() + parseFloat(VEnumValue));
            $scope.PurchaseBookingSoda.ValidationDate = $filter('dateFiltering')(new Date(valdate), 'dd-MM-yyyy');
            });
    }


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PurchaseBookingSodaList = response.data;
            for (var i = 0; i < $scope.PurchaseBookingSodaList.length; i++) {
                var DateSelected = new Date($scope.PurchaseBookingSodaList[i].Date);
                var BookingDays = new Date();
                var res = Math.abs(BookingDays - DateSelected) / 1000;
                var days = Math.floor(res / 86400);
                $scope.PurchaseBookingSodaList[i].BookDays = days;

                if ($scope.PurchaseBookingSodaList[i].IsConfirmed == 1) {
                    $scope.PurchaseBookingSodaList[i].BookingStatus = "Confirmed";
            
                
                }
            }
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
        LocationId: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Time: _Time,
        CropPlanningId: null,
        CropPlanning: null,
        ValidationDate: null,
        CustomerId: null,
        Farmer: null,
        FarmerFatherHusbandNameId: null,
        FarmerRegId: null,
        ICSMasterID: null,
};
    $scope.PurchaseBookingSoda = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.PurchaseBookingSoda = Object.assign({}, args.data);
        $scope.PurchaseBookingSoda.Time = $scope.PurchaseBookingSoda.BookingTime;
        $scope.PurchaseBookingSoda.Date = $scope.PurchaseBookingSoda.BookingDate;
        $scope.PurchaseBookingSoda.ValidationDate = $scope.PurchaseBookingSoda.ValidateDate;
        $scope.GetCropPlanning();
        $scope.GetFarmer();
        $scope.GetFarmerFather();
        $scope.GetFarmerRegistrationId();
        $scope.GetValidationDate();
        $scope.getPBSChildTabData();
        $scope.setTab(1);
   //     $scope.Action = 'Update';
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
            $scope.PurchaseBookingSodaList = response.data;
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        //      if ($scope.PBSchildForm.$valid) {
        try {
        var HeaderConfirmedData = [];
      
        for (var i = 0; i < $scope.PurchaseBookingSodaList.length; i++) {
            if ($scope.PurchaseBookingSodaList[i].isSelected == true) {
                HeaderConfirmedData.push($scope.PurchaseBookingSodaList[i]);
                $scope.Confirmation.IsConfirmed = true;
                $scope.Confirmation.IsApproved = false;
            }
            //else {
            //    throw 'Please dont select Confirmed Soda';
            //}
               
        }

    //    try {
            if (HeaderConfirmedData.length == 0) {
                throw 'Please select at least one Confirmation Data';
            }
            $http({
                method: 'POST',
                data: { HeaderConfirmedChildTabData: HeaderConfirmedData, IsConfirmedData: $scope.Confirmation.IsConfirmed, IsApprovedData: $scope.Confirmation.IsApproved },
                url: $scope.path + 'Create'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
              //      $scope.purchasebookingsodachild = response.data.Data;
                    $scope.getData();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }

        //     }
    }

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.PurchaseBookingSoda.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.deleteUrl + $scope.PurchaseBookingSoda.Id,
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                ClearFields();
    //                $scope.getData();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};

    $scope.Clear = function () {
        ClearFields();
       
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.PurchaseBookingSoda = Object.assign({}, $scope.ModelTemp);
        $scope.getPBSChildTabData();
        $scope.setTab(); 
    }

    ///////*********************Tabs*******************************
    // #region Tab
    //  $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    $scope.purchasebookingsodachildModelTemp = {
        Id: null,
        PurchaseBookingSodaMasterId: null,
        CropPlanningChildId: null,
        Quantity: null,
        Rate: null,
        TargetRate: null,
  //      BalanceBook: null,
        BalancePurchase: null,
        Remarks: null,
        PlanQuantity: null,
        PlanQuantityId: null

    };
    $scope.purchasebookingsodachild = Object.assign({}, $scope.purchasebookingsodachildModelTemp);

    $scope.ConfirmationModelTemp = {
        Id: null,
        PurchaseBookingSodaMasterId: null,
        CropPlanningChildId: null,
        Quantity: null,
        Rate: null,
        TargetRate: null,
        //      BalanceBook: null,
        BalancePurchase: null,
        Remarks: null,
        PlanQuantity: null,
        PlanQuantityId: null,
        IsConfirmed: false,
        IsApproved: false

    };
    $scope.Confirmation = Object.assign({}, $scope.ConfirmationModelTemp);

    // #region Purchase Booking Soda child Tab
   
    $scope.PBSChildList = [];
    $scope.getPBSChildTabData = function () {
        $scope.PBSChildList = [];
        $http({
            method: 'POST',
            data: { PurchaseBookingSodaMasterId: $scope.PurchaseBookingSoda.Id},
            url: $scope.path + 'LoadAllPBSChildTabForSelection'
        }).then(function successCallback(response) {
            $scope.PBSChildList = response.data;
  
        });
    }

    $scope.SelectedPurchaseBookingSodaChildTabList = [];
    $scope.LoadAllSelectedPBSChildTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedPBSChildTab?PurchaseBookingSodaMasterId=' + $scope.PurchaseBookingSoda.Id
        }).then(function successCallback(response) {
            $scope.SelectedPurchaseBookingSodaChildTabList = response.data;
        });
    }
    //Save Function 
    $scope.PBSChildTabId = '';
    $scope.SavePBSChildTab = function () {
        $scope.$broadcast('show-errors-check-validity');
   
  //      if ($scope.PBSchildForm.$valid) {
        var checkedData = [];
        var ConfirmedData = [];
            for (var i = 0; i < $scope.PBSChildList.length; i++) {
                if ($scope.PBSChildList[i].CheckBoxSelect == true) {
                    checkedData.push($scope.PBSChildList[i]);
                    $scope.Confirmation.IsConfirmed = true;
                    $scope.Confirmation.IsApproved = false;
                }
                else {
                    $scope.PBSChildList[i].ConfirmedQuantity = 0;
                    $scope.PBSChildList[i].ConfirmedRate = 0;
                    checkedData.push($scope.PBSChildList[i]);
                    $scope.Confirmation.IsConfirmed = true;
                    $scope.Confirmation.IsApproved = false;
                }
                                        
        }
      
            try {
                if (checkedData.length == 0) {
                    throw 'Please select at least one Confirmation Data';
                }
                $http({
                    method: 'POST',
                    data: { PBSChildTabData: checkedData, ConfirmedChildTabData: ConfirmedData, data: $scope.purchasebookingsodachild, PurchaseBookingSodaChildMasterId: $scope.PurchaseBookingSoda.Id, IsConfirmedData: $scope.Confirmation.IsConfirmed, IsApprovedData: $scope.Confirmation.IsApproved },
                    url: $scope.path + 'SavePurchaseBookingSodaChild'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.purchasebookingsodachild = response.data.Data;
                        $scope.getPBSChildTabData();
                    }
                });

            }
            catch (e) {
                ShowResult(e, "failure");
            }

   //     }
    }
    $scope.ClearPBSChildTab = function () {
        $scope.getPBSChildTabData();
    }

     function ClearFieldspurchasebookingsodachild() {

        $scope.purchasebookingsodachild = Object.assign({}, $scope.purchasebookingsodachildModelTemp);
 
    }

    //$scope.DeletePurchaseBookingSodaChild = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'DeletePurchaseBookingSodaChild?Id=' + $scope.PBSChildTabId
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == true) {
    //            ShowResult(response.data.Message, "failure");
    //        }
    //        else {
    //            ShowResult(response.data.Message, "success");
    //            $scope.getPBSChildTabData();
    //        }

    //    });
    //}

    //$scope.ConfirmDeletePurchaseBookingSodaChildTab = function (Id) {
    //    $scope.PBSChildTabId = Id;
    //    angular.element(document.querySelector("#DeletePurchaseBookingSodaChildTabPopUp")).modal("show");
    //}

    //$scope.closePBSChildTabPopUp = function (popupName) {
    //    angular.element(document.querySelector("#PBSChildPopUp")).modal("hide");
    //}


    // # end region Document Tab

    $scope.refreshTemplateemployee = function () {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridPurchaseBookingSodaChildTab").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.PBSChildList.length; i++) {
                $scope.PBSChildList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {

                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }


        }
        var gridObj = $("#GridPurchaseBookingSodaChildTab").data("ejGrid");
        gridObj.refreshContent();
    };



}