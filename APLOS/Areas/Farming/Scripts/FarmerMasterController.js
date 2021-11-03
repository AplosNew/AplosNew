'use strict';
FarmerMasterController.$inject = ['$window', '$timeout', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FarmerMasterController($window, $timeout, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Farmer Master';
    $scope.FarmerMasterList = [];

    $scope.ToDoFilePath = virtualPath.FarmerMaster;

    $scope.CountryList = [];
    $scope.TalukaList = [];
    $scope.StateList = [];
    $scope.UOMList = [];
    $scope.VillageList = [];
    $scope.DistrictList = [];
    
   
    $scope.path = 'Farming/FarmerMaster/';

    $scope.getListUrl = $scope.path + 'getlist';
   
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlFarmermasterplot = $scope.path + 'savefarmermasterplot';
 
    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "FarmerName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'FarmerRegistrationID', name: "Tracenet ID" }, { value: 'Date', name: "Date" }, { value: 'State', name: "State" }, { value: 'FarmerName', name: "Farmer Name" }, { value: 'Villages', name: "Village" }, { value: 'District', name: "District" }, { value: 'Taluk', name: "Taluk" }];
 

    // #region ddl
    $http({
        method: 'GET',
        url: 'Farming/FarmerMaster/getcountrylist/',
    }).then(function successCallback(response) {
        $scope.CountryList = response.data;
        });

    $scope.GetState = function () {
        $scope.StateList = [];
        $http({
            method: 'GET',
            url: 'Farming/FarmerMaster/getstatelist?CountryId=' + $scope.FarmerMaster.CountryId
        }).then(function successCallback(response) {
            $scope.StateList = response.data;
        });
    }

    $scope.GetDistrict = function () {
        $scope.DistrictList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getdistrictlist?StateId=' + $scope.FarmerMaster.StateId
        }).then(function successCallback(response) {
            $scope.DistrictList = response.data;
        });
    }

    $scope.GetTaluk = function () {
        $scope.TalukaList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'gettaluklist?DistrictId=' + $scope.FarmerMaster.DistrictId
        }).then(function successCallback(response) {
            $scope.TalukaList = response.data;
        });
    }

    $scope.GetVillages = function () {
        $scope.VillageList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getvillageslist?TalukId=' + $scope.FarmerMaster.TalukaId
        }).then(function successCallback(response) {
            $scope.VillageList = response.data;
        });
    }

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.UOMList = response;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.FarmerMasterList = response.data;
            ClearFields();
         
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Reference: null,
        FarmerRegistrationID: null,
        FarmerRegistrationDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        FarmerName: null,
        FarmerFatherHusbandName: null,
        Gender: null,
        NationalID: null,
        MobileNo: null,
        Address1: null,
        Address2: null,
        VillageId: null,
        TalukaId: null,
        DistrictId: null,
        StateId: null,
        CountryId: null,
        Villages: null,
        Taluk: null,
        District: null,
        State: null,
        Pincode: null,
        UOMId: null,
        TotalArea: null,
        ResponsiblePersonId: null,
        DebitGLCode: null,
        CreditGLCode: null,
        Active: true,
        Remarks: null,
        EmployeeStatus: null,
        BankName: null,
        AccountNo: null,
        IFSCCode: null
};
    $scope.FarmerMaster = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {

        $scope.FarmerMaster = Object.assign({}, args.data);
        $scope.getFarmerMasterPlotData($scope.FarmerMaster.Id);
        $scope.GetState();
        $scope.GetDistrict();
        $scope.GetTaluk();
        $scope.GetVillages();
        $scope.setTab(1);
        $scope.Action = 'Update';
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
            $scope.FarmerMasterList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.FarmerMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.FarmerMaster = response.data.Data;
                  
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                    $scope.getFarmerMasterPlotData($scope.FarmerMaster.Id);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.FarmerMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.FarmerMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
       
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.FarmerMaster = Object.assign({}, $scope.ModelTemp);
        $scope.getFarmerMasterPlotData($scope.FarmerMaster.Id);
        $scope.setTab();
      
      
    }


    ///////////////////////////////////  Responsible Person Pop Up  ////////////////////////////////////////


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
            data: { Id: $scope.FarmerMaster.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.FarmerMaster.ResponsiblePersonId = null;
        $scope.FarmerMaster.ResponsiblePerson = null;
        $scope.FarmerMaster.EmployeeCode = null;
        $scope.FarmerMaster.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.FarmerMaster.EmployeeCode = data.Code;
        $scope.FarmerMaster.ResponsiblePersonId = data.Id;
        $scope.FarmerMaster.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////

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

 // *************** Farmer Master Plot Tab *******************

    $scope.SelectedFarmerMasterPlotTabList = [];
    
    $scope.FarmerMasterPlotModelTemp = {
        Id: null,
        FarmerMasterId: null,
        PlotNameNo: null,
        PlotArea: null,
        Survey: null,
        Latitude: null,
        Longitude: null,
        PlotStatus: null,
        Active: true,
        Remarks: null,
        ICSMasterId: null,
        FarmerRegistrationID: null,
        FarmerRegistrationDate: null,
        InspectionDate: null,
        ApprovalDate: null,
        RenewalPeriod: null,
        FileName: null,
        FileNameee: null,
     
    };
    $scope.FarmerMasterPlot = Object.assign({}, $scope.FarmerMasterPlotModelTemp);


    function ClearFieldsFarmerMasterPlot() {
       
        $scope.FarmerMasterPlot = Object.assign({}, $scope.FarmerMasterPlotModelTemp);
        

    }

    $scope.getFarmerMasterPlotData = function (FarmerMasterId) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetListFarmerMasterPlot?FarmerMasterId=' + FarmerMasterId
        }).then(function successCallback(response) {
            $scope.SelectedFarmerMasterPlotTabList = response.data;           
            ClearFieldsFarmerMasterPlot();
            ClearDocument();
            var FRId = parseFloat($scope.FarmerMaster.FarmerRegistrationID);
            $scope.FarmerMasterPlot.FarmerRegistrationID = FRId;
            var FRDate = $scope.FarmerMaster.FarmerRegistrationDate;
            $scope.FarmerMasterPlot.FarmerRegistrationDate = FRDate;
        });
    }


    $scope.DeleteFarmerMasterPlot = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteFarmerMasterPlot?Id=' + $scope.FarmerMasterPlotTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getFarmerMasterPlotData($scope.FarmerMaster.Id);
                
            }

        });
    }

    $scope.ConfirmDeleteFarmerMasterPlotTab = function (Id) {
        $scope.FarmerMasterPlotTabId = Id;
        angular.element(document.querySelector("#DeleteFarmerMasterPlotTabPopUp")).modal("show");
    }
    ////********** Tab end ***************

    $scope.ICSMasterList = [];
    $scope.PlotStatusList = [];

    $http({
        method: 'GET',
        url: 'Farming/FarmerMaster/geticsMasterlist/',
    }).then(function successCallback(response) {
        $scope.ICSMasterList = response.data;
        });

    $http({
        method: 'GET',
        url: 'Farming/FarmerMaster/getplotstatuslist/',
    }).then(function successCallback(response) {
        $scope.PlotStatusList = response.data;
        });

    $scope.CheckPlotArea = function () {
            var TotalArea = $scope.FarmerMaster.TotalArea;
            var SelectedPlotArea = parseFloat($scope.FarmerMasterPlot.PlotArea);
            $http({
                method: 'GET',
                url: 'Farming/FarmerMaster/getplotareasum?FarmerMasterId=' + $scope.FarmerMaster.Id
            }).then(function successCallback(response) {
                var PlotAreaSum = response.data[0].TotalArea;
                var Sum = parseFloat(PlotAreaSum) + SelectedPlotArea;
                try {
                    if (Sum > TotalArea) {
                        $scope.FarmerMasterPlot.PlotArea = null;
                        throw 'Plot Area should be less than Total Area';
                    }
                }
                catch (e) {
                    ShowResult(e, "failure");
                }  
             
             });  
    }

    //File Attachment-----Start
    //Attach and File


    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };

    //File Download
    
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
     //   $scope.dwonloadUrl = virtualPath.FarmerMaster + '/' + data.Id + extention;
        $scope.dwonloadUrl = 'E:\Shash\Aplos\NewlyProject\LatestProject\APLOSNew\APLOS\POPResources\Farming\FarmerMaster' + '/' + data.FileName;
    };

    //Detach file  button Method and id confirmDocumentDelete
    $scope.DocumentRemove = function () {
     //   $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('show');
    };
    $scope.removeDocument = function () {
       
        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.FarmerMasterPlot.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };

    //FarmerMasterPlot Detach file method
    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };
    // Clear Method for FarmerMasterPlot
    function ClearDocument() {
        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.FarmerMasterPlot.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
    };

    //File Attachment-----End


    $scope.SaveFarmerMasterPlotTab = function () {
        $scope.FarmerMasterPlot.FarmerMasterId = $scope.FarmerMaster.Id;
  //      $scope.$broadcast('show-errors-check-validity');
   //     if ($scope.FarmerMasterPlotForm.$valid) {
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.FarmerMasterPlot.FileName = fileName;
        if (!baseService.isUndefinedOrNull($scope.FarmerMasterPlot.FileName)) {
            if ($scope.FarmerMasterPlot.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        var formData = new FormData();
        $http({
            method: 'POST',
            url: $scope.saveUrlFarmermasterplot,
            headers: { 'Content-Type': undefined },
            transformRequest: function (data) {
                formData.append("FarmerMasterPlot", angular.toJson(data.FarmerMasterPlot));
                if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                    formData.append('file', data.file);
                }
                return formData;
            },
            data: { 'FarmerMasterPlot': $scope.FarmerMasterPlot, 'file': $scope.filedata }

        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.FarmerMasterPlot = response.data.Data;
                $scope.getFarmerMasterPlotData($scope.FarmerMaster.Id);
               

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

     //    }
    };

    //#region start Farmer Master Reports
   // $scope.ConfirmPrintTab = function (Id) {
   //     $scope.FarmerMasterPrintTabId = Id;

   ////     var data = args.data;
   //     var reportFormat = "Excel";

   //     try {
   //         window.open('Farming/FarmerMaster/GetFarmerMasterPrintReport?reportFormat=' + reportFormat + '&FarmerMasterPrintId=' + $scope.FarmerMasterPrintTabId, '_blank');
   //         $scope.getData();

   //     } catch (e) {

   //     }
   // };

    //#endregion end Farmer Master Reports
}