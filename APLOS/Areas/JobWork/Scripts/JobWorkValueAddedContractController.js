'use strict';
JobWorkValueAddedContractController.$inject = ['$window','cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function JobWorkValueAddedContractController($window,cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.ToDoFilePath = virtualPath.JobWorkValueAddedContract;
    $scope.ToDownloadFilePath = virtualPath.JobWorkTransformationContract;
    $rootScope.title = 'Job Work Contract';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.EntityList = [];
    $scope.AllPlantList = [];
    $scope.AllEntityList = [];
    $scope.MaterialLocationList = [];
    $scope.path = 'JobWork/JobWorkValueAddedContract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "p.UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'p.UserName', name: "Party Name" }, { value: 'e.UserName', name: "Entity" }, { value: 'Date', name: "Date" }];

    //////// Drop Down
  

    $http({
        method: 'GET',
        url: 'JobWork/JobWorkValueAddedContract/getmateriallocation/',
    }).then(function successCallback(response) {
        $scope.MaterialLocationList = response.data;
        });
    

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields();
            $scope.ClearMaterial();
            $scope.ClearTransformation();
            $scope.SelectedMatPlanningTabList = [];
            $scope.ClearMatPlanning();
            

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
        Time: _Time,
        PlantId:null,
        EntityId: null,
        VendorPartyId: null,
        ProcessStartDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ProcessEndDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ContractClosingDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        UserContractReference: null,
        Remarks: null,
        PartyCode: null,
        PartyName: null,
        ContractStatus:"Active",

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetAllPlantList = function () {
        $scope.AllPlantList = [];
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkValueAddedContract/getplant/'
        }).then(function successCallback(response) {
            $scope.AllPlantList = response.data;
            for (var q = 0; q < $scope.AllPlantList.length; q++) {
                if ($scope.AllPlantList[q].Value == $window.plantId) {
                    $scope.ModelNew.PlantId = $scope.AllPlantList[q].Value;
                }
            }
        });
    }

    $scope.GetAllPlantList();


    $scope.GetEntityPlantWise = function () {
        if ($scope.ModelNew.PlantId == null) {
            var PLT = $window.plantId
            $scope.ModelNew.PlantId = PLT;
        }
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllEntity?PlantId=' + $scope.ModelNew.PlantId
        }).then(function successCallback(response) {
            $scope.AllEntityList = response.data;

        });
    }
    $scope.GetEntityPlantWise();


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        if ($scope.ModelNew.TabType == "Transformation") {
    
            $scope.Transformation = Object.assign({}, args.data);
            $scope.Transformation.Date = $scope.ModelNew.ValueAddedDate;
            $scope.Transformation.ProcessStartDate = $scope.ModelNew.VAProcessStartDate;
            $scope.Transformation.ProcessEndDate = $scope.ModelNew.VAProcessEndDate;
            $scope.Transformation.ContractClosingDate = $scope.ModelNew.VAContractClosingDate;
            $scope.Transformation.Time = $scope.ModelNew.VACTime;
            $scope.Transformation.EntityId = $scope.ModelNew.EntityId;
            $scope.Transformation.ContractStatus = $scope.ModelNew.ContractStatus;
            $scope.Transformation.ContractId = $scope.ModelNew.Id;
            $scope.GetPlantList();
            $scope.getAllEntity();

            $scope.getMatPlanningData();
            $scope.setTab(2);
            $scope.Action = 'Update';
        }
        else {
            $scope.ModelNew = Object.assign({}, args.data);
            $scope.ModelNew.Date = $scope.ModelNew.ValueAddedDate;
            $scope.ModelNew.ProcessStartDate = $scope.ModelNew.VAProcessStartDate;
            $scope.ModelNew.ProcessEndDate = $scope.ModelNew.VAProcessEndDate;
            $scope.ModelNew.ContractClosingDate = $scope.ModelNew.VAContractClosingDate;
            $scope.ModelNew.Time = $scope.ModelNew.VACTime;
            $scope.ModelNew.ContractStatus = $scope.ModelNew.ContractStatus;
            $scope.GetAllPlantList();
            $scope.GetEntityPlantWise();
         //   $scope.ModelNew.EntityId = $scope.ModelNew.EntityId;
            $scope.getMaterialPlanningData();
            $scope.setTab(1);

            $scope.Action = 'Update';
        }
       
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewGeneralForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew = response.data.Data;
                    $scope.Getgrid();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        $scope.getData();
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.getMaterialPlanningData();
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
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllPartyDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        });
    }

    $scope.PartyClear = function () {
        $scope.ModelNew.VendorPartyId = null;
        $scope.ModelNew.PartyName = null;
        $scope.ModelNew.PartyCode = null;
    };
    $scope.closePositionPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPositionData = function (obj) {
        var data = obj.data;
        $scope.ModelNew.PartyCode = data.Code;
        $scope.ModelNew.VendorPartyId = data.Id;
        $scope.ModelNew.PartyName = data.UserName;
        angular.element(document.querySelector('#PosPopUp')).modal('hide');
    };
    // # end region

    // Child data

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.subtab = 1;
    $scope.settheTab = function (newsubTab) {
        $scope.subtab = newsubTab;
    };
    $scope.Set = function (tabsubNum) {
        return $scope.subtab === tabsubNum;
    };

    $scope.subtabMatP = 1;
    $scope.settheTabMatP = function (newsubTabMatP) {
        $scope.subtabMatP = newsubTabMatP;
    };
    $scope.SetMatP = function (tabsubNum) {
        return $scope.subtabMatP === tabsubNum;
    };

    $scope.JobWorkItemMasterList = [];
    $scope.OutputMaterialUOMList = [];
    $scope.ArticleCodeList = [];
    $scope.RateApplyList = [];
    $scope.CurrencyList = [];
    $scope.SelectedMaterialPlanningTabList = [];
    $scope.JobActivityList = [];

    $http({
        method: 'GET',
        url: $scope.path + 'getjobworkactivitylist',
    }).then(function successCallback(response) {
        $scope.JobActivityList = response.data;
    });

    $scope.GetJWIVAM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getjobworkitemlist?JWActivityId=' + $scope.MaterialPlanning.JobActivityId,
        }).then(function successCallback(response) {
            $scope.JobWorkItemMasterList = response.data;
        });
    }
  

    $http({
        method: 'GET',
        url: $scope.path + 'getoutputunit',
    }).then(function successCallback(response) {
        $scope.OutputMaterialUOMList = response.data;
        });

    $scope.VMValues = [];
    $scope.GetVMValues = function () {
        $scope.VMValues = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetVMValues?JobWorkItemId=' + $scope.MaterialPlanning.JobWorkItemMasterId + '&JWActivityId=' + $scope.MaterialPlanning.JobActivityId ,
        }).then(function successCallback(response) {
            $scope.VMValues = response.data;
            if ($scope.VMValues.length > 0) {
                $scope.MaterialPlanning.MaterialMasterId = $scope.VMValues[0].MaterialMasterId;
                $scope.MaterialPlanning.MaterialCode = $scope.VMValues[0].MaterialCode;
                $scope.MaterialPlanning.MaterialName = $scope.VMValues[0].Material;
                $scope.MaterialPlanning.OutputMaterialUOMId = $scope.VMValues[0].UnitId;
                $scope.MaterialPlanning.RateApplyId = $scope.VMValues[0].RateApplicable;
                $scope.MaterialPlanning.RatePerUnit = $scope.VMValues[0].MinRate;
                $scope.MaterialPlanning.Rejection = $scope.VMValues[0].StdRejection;
                $scope.MaterialPlanning.ValueLoss = $scope.VMValues[0].StdValueLoss;
            }
        });
    }

    //$scope.GetArticleCode = function () {
    //    $scope.ArticleCodeList = [];
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getarticlecode?JobWorkItemId=' + $scope.MaterialPlanning.JobWorkItemMasterId,
    //    }).then(function successCallback(response) {
    //        $scope.ArticleCodeList = response.data;
    //    });
    //}

    //$scope.GetRateApply = function () {
    //    $scope.RateApplyList = [];
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getrateapplylist?JobWorkItemId=' + $scope.MaterialPlanning.JobWorkItemMasterId,
    //    }).then(function successCallback(response) {
    //        $scope.RateApplyList = response.data;
    //    });
    //}
  
    $scope.GetCurrency = function () {
        $scope.CurrencyList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getcurrency?JobWorkItemId=' + $scope.MaterialPlanning.JobWorkItemMasterId + '&JWActivityId=' + $scope.MaterialPlanning.JobActivityId,
        }).then(function successCallback(response) {
            $scope.CurrencyList = response.data;
            if ($scope.CurrencyList.length > 0) {
                $scope.MaterialPlanning.CurrencyId = $scope.CurrencyList[0].Value;
            }
        });
    }

    // #region field

    $scope.MMList = [];
    $scope.MaterialMasterPopUp = function () {
        angular.element(document.querySelector("#MMPopUp")).modal("show");
        $scope.getMMData();

    }
    $scope.getMMData = function () {
        $scope.MMList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.MaterialPlanning.Id },
            url: $scope.path + 'LoadAllMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.MMList = response.data;
        });
    }

    $scope.MaterialMasterClear = function () {
        $scope.MaterialPlanning.MaterialMasterId = null;
        $scope.MaterialPlanning.MaterialName = null;
        $scope.MaterialPlanning.MaterialCode = null;

    };
    $scope.closeMMPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMMData = function (obj) {
        var data = obj.data;
        $scope.MaterialPlanning.MaterialCode = data.Code;
        $scope.MaterialPlanning.MaterialMasterId = data.Id;
        $scope.MaterialPlanning.MaterialName = data.MaterialName;
        $scope.MaterialPlanning.OutputMaterialUOMId = data.BaseUOMId;
        angular.element(document.querySelector('#MMPopUp')).modal('hide');
        $scope.MMArticlePopUp();
    };
    // # end region

    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.MMAList = [];
    $scope.MMArticlePopUp = function () {
        angular.element(document.querySelector("#MMAPopUp")).modal("show");
        $scope.getMMArticleData();

    }
    $scope.getMMArticleData = function () {
        $scope.MMAList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.MaterialPlanning.Id, MaterialMstId: $scope.MaterialPlanning.MaterialMasterId },
            url: $scope.path + 'LoadAllMMArticle'
        }).then(function successCallback(response) {
            $scope.MMAList = response.data;
        });
    }

    $scope.MMArticleClear = function () {
        $scope.MaterialPlanning.ArticleCodeId = null;
        $scope.MaterialPlanning.ArticleName = null;
        $scope.MaterialPlanning.ArticleCode = null;

    };
    $scope.closeMMAPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMMAData = function (obj) {
        var data = obj.data;
        $scope.MaterialPlanning.ArticleCode = data.ArticleCode;
        $scope.MaterialPlanning.ArticleCodeId = data.ArticleId;
        $scope.MaterialPlanning.ArticleName = data.StandardName;
        angular.element(document.querySelector('#MMAPopUp')).modal('hide');
    };
    // # end region

    $scope.MaterialPlanningModelTemp = {
        Id: null,
        JobWorkValueAddedContractMasterId: null,
        JobWorkItemMasterId: null,
        MaterialSpecification: null,
        MaterialReference: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        ArticleCodeId: null,
        OrderSpecific: null,
        RequiredCapacity: null,
        RateApplyId: null,
        CurrencyId: null,
        RatePerUnit: null,
        Rejection: null,
        ValueLoss: null,
        ResponsiblePersonId: null,
        Remarks: null,
        FileName: null,
        MaterialLocationId: null,
        MaterialType: null,
        FinalOutputCategory: null,
        JobActivityId: null,

        MaterialCode: null,
        MaterialName: null,
        MaterialMasterId: null,
        ArticleCode: null,
        ArticleName: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
        EmployeeStatus: null,
  //      Tolerance: null,

    };
    $scope.MaterialPlanning = Object.assign({}, $scope.MaterialPlanningModelTemp);

    //File Attachment-----Start
    //Attach and File

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById('uploadBtn4').onchange = function () {
        var filename = document.getElementById('uploadFile4').value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById('uploadFile4').value = res;
      
    };

    //File Download

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        //   $scope.dwonloadUrl = virtualPath.FarmerMaster + '/' + data.Id + extention;
        $scope.dwonloadUrl = 'E:\Shash\Aplos\NewlyProject\LatestProject1\ApopMainProjectMaster\APLOS\POPResources\JobWork\JobWorkValueAddedContract' + '/' + data.FileName;
    };

    //Detach file  button Method and id confirmDocumentDelete
    $scope.DocumentRemove = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('show');
    };
    $scope.removeDocument = function () {

        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.MaterialPlanning.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };

    //MaterialPlanning Detach file method
    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };
    // Clear Method for MaterialPlanning
    function ClearDocument() {
        document.getElementById('uploadBtn4').value = '';
        $scope.filedata = '';
        $scope.MaterialPlanning.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile4').value = "";
    };

    //File Attachment-----End


    $scope.SaveMaterial = function () {
        $scope.MaterialPlanning.JobWorkValueAddedContractMasterId = $scope.ModelNew.Id;
        //      $scope.$broadcast('show-errors-check-validity');
        //     if ($scope.FarmerMasterPlotForm.$valid) {
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.MaterialPlanning.FileName = fileName;
        if (!baseService.isUndefinedOrNull($scope.MaterialPlanning.FileName)) {
            if ($scope.MaterialPlanning.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        var formData = new FormData();
        $http({
            method: 'POST',
            url: $scope.path + 'saveUrlMaterialPlanning',
            headers: { 'Content-Type': undefined },
            transformRequest: function (data) {
                formData.append("MaterialPlanning", angular.toJson(data.MaterialPlanning));
                if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                    formData.append('file', data.file);
                }
                return formData;
            },
            data: { 'MaterialPlanning': $scope.MaterialPlanning, 'file': $scope.filedata }


        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MaterialPlanning = response.data.Data;
                $scope.getMaterialPlanningData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        //    }
    };

    $scope.SaveJWOutputAction = 'Save';
    $scope.GetJWOutputDataToEdit = function (args) {
        $scope.MaterialPlanning = Object.assign({}, args.data);
        $scope.GetJWIVAM();
        $scope.GetCurrency();
        $scope.getMaterialDataToEdit();
        $scope.SaveJWOutputAction = 'Update';

    };

    $scope.MaterialDataToEdit = [];
    $scope.getMaterialDataToEdit = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'MaterialMSTDataToEdit?ArticleId=' + $scope.MaterialPlanning.ArticleCodeId
        }).then(function successCallback(response) {
            $scope.MaterialDataToEdit = response.data;
            if ($scope.MaterialDataToEdit.length > 0) {
                $scope.MaterialPlanning.MaterialName = $scope.MaterialDataToEdit[0].MaterialName;
                $scope.MaterialPlanning.MaterialCode = $scope.MaterialDataToEdit[0].MaterialCode;
                $scope.MaterialPlanning.MaterialMasterId = $scope.MaterialDataToEdit[0].MaterialMasterId;
                $scope.MaterialPlanning.OutputMaterialUOMId = $scope.MaterialDataToEdit[0].OutputMaterialUOMId;
            }
        });
    }


    $scope.ClearMaterial = function () {
        ClearFieldsMaterialPlanningChildData();
        $scope.getMaterialPlanningData();
        $scope.SaveJWOutputAction = 'Save';
    }

    function ClearFieldsMaterialPlanningChildData() {

        $scope.MaterialPlanning = Object.assign({}, $scope.MaterialPlanningModelTemp);
    }

    $scope.getMaterialPlanningData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMaterialPlanningData?MasterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedMaterialPlanningTabList = response.data;

        });
    }


    $scope.DelMaterialPlanning = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelMaterialPlanning?Id=' + $scope.ChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMaterialPlanningData();
                ClearFieldsMaterialPlanningChildData();
            }

        });
    }

    $scope.ConfirmDeleteMaterialPlotTab = function (Id) {
        $scope.ChildTabId = Id;
        angular.element(document.querySelector("#DeleteChildTabPopUp")).modal("show");
    }

    // #region field

    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.ModelNew.Id },
            url: $scope.path + 'LoadAllEmpDetails'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.MaterialPlanning.ResponsiblePersonId = null;
        $scope.MaterialPlanning.ResponsiblePerson = null;
        $scope.MaterialPlanning.EmployeeCode = null;
        $scope.MaterialPlanning.EmployeeStatus = null;

    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.MaterialPlanning.EmployeeCode = data.Code;
        $scope.MaterialPlanning.ResponsiblePersonId = data.Id;
        $scope.MaterialPlanning.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region

  
    //********** Tab end ***************

    // Order Wise Requirement tab

  //  $scope.MonthsList = [];
    $scope.ConfirmPopUp = function (data) {
        $scope.MaterialPlanningTabId = data.Id;
        $scope.UnitId = data.OutputMaterialUOMId;
        $scope.OrderWiseReq.Quantity = data.Quantity;
        $scope.PQuantity = data.Quantity;
        $scope.OrderWiseReq.PlanQuantity = $scope.PQuantity;
        $scope.OrderWiseReq.ArtclCode = data.ArticleCode
        $scope.GetOrderWiseUOM();
        $scope.getOrderWiseData();
        angular.element(document.querySelector("#MonthsPopUp")).modal("show");

    }
   

    $scope.closeOrderWiseTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.CustomerList = [];
    $scope.MasterOrderNoList = [];
    $scope.MasterOrderItemList = [];
    $scope.UOMList = [];
    $scope.OrderWiseRequirementList = [];

    $http({
        method: 'GET',
        url: $scope.path + 'getcustomerlist',
    }).then(function successCallback(response) {
        $scope.CustomerList = response.data;
    });

    $scope.GetMasterOrderNo = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterorderlist?CustomerId=' + $scope.OrderWiseReq.CustomerId,
        }).then(function successCallback(response) {
            $scope.MasterOrderNoList = response.data;
        });
    }

    $scope.GetMasterOrderItem = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterorderitemlist?MasterOrderNoId=' + $scope.OrderWiseReq.MasterOrderNoId,
        }).then(function successCallback(response) {
            $scope.MasterOrderItemList = response.data;
        });
    }

    $scope.GetOrderWiseUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getoutputunit',
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
            if (baseService.arrayLength($scope.UOMList) > 0) {
                $scope.OrderWiseReq.OutputMaterialUOMId = $scope.UnitId;
            }
        });
    }
  

    $scope.OrderWiseReqModelTemp = {
        Id: null,
        JobWorkValueAddedContractChildMasterId: null,
        OrderType: null,
        CustomerId: null,
        MasterOrderNoId: null,
        MasterOrderItemId: null,
        ParticularSpecification: null,
        Remarks: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        PlanQuantity: null,
        ArtclCode: null,
       
    };
    $scope.OrderWiseReq = Object.assign({}, $scope.OrderWiseReqModelTemp);

    $scope.SaveOrderWiseTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.OrderWiseReqForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveOrderWiseReq',
                data: { 'data': $scope.OrderWiseReq, 'ChildMasterId': $scope.MaterialPlanningTabId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.OrderWiseReq = response.data.Data;
                    ClearFieldsOrderWiseChildData();
                    $scope.getOrderWiseData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DelOrderWise = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelOrderWise?Id=' + $scope.OrderWiseChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getOrderWiseData();
                ClearFieldsOrderWiseChildData();
            }

        });
    }

    $scope.ConfirmDeleteOrderWiseTab = function (Id) {
        $scope.OrderWiseChildTabId = Id;
        angular.element(document.querySelector("#DeleteOrderWiseChildTabPopUp")).modal("show");
    }

    $scope.ClearOrderWiseTab = function () {
        ClearFieldsOrderWiseChildData();
    }

    function ClearFieldsOrderWiseChildData() {
        $scope.OrderWiseReq.Id = null;
        $scope.OrderWiseReq.JobWorkValueAddedContractChildMasterId = null;
        $scope.OrderWiseReq.OrderType = null;
        $scope.OrderWiseReq.CustomerId = null;
        $scope.OrderWiseReq.MasterOrderNoId = null;
        $scope.OrderWiseReq.MasterOrderItemId = null;
        $scope.OrderWiseReq.ParticularSpecification = null;
        $scope.OrderWiseReq.Remarks = null;
        $scope.OrderWiseReq.PlanQuantity = $scope.PQuantity;
        $scope.GetOrderWiseUOM();
    }

    $scope.getOrderWiseData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getOrderWiseData?MaterialMasterId=' + $scope.MaterialPlanningTabId
        }).then(function successCallback(response) {
            $scope.OrderWiseRequirementList = response.data;

        });
    }

    /////// TRANSFORMATION TAB ///////////////

    $scope.ValidateTransDate = function () {
        try {

            if (new Date($scope.Transformation.Date) > new Date()) {
                $scope.Transformation.Date = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                throw 'Date should not be greater than Current date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }


    $scope.ValidateProcessEndDate = function () {
        try {

            if (new Date($scope.Transformation.ProcessEndDate) < new Date($scope.Transformation.ProcessStartDate)) {
                $scope.Transformation.ProcessEndDate = $filter('dateFiltering')(new Date(), 'dd-M-yyyy');
                throw 'Process End Date should not be less than Process Start Date.';
            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    // #region Vendor/ Party field

    $scope.PartyList = [];
    $scope.VendorPopUp = function () {
        angular.element(document.querySelector("#PartyPopUp")).modal("show");
        $scope.getPartyDetailsData();

    }
    $scope.getPartyDetailsData = function () {
        $scope.PartyList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.Transformation.Id },
            url: $scope.path + 'LoadAllVendorDetails'
        }).then(function successCallback(response) {
            $scope.PartyList = response.data;
        });
    }

    $scope.VendorClear = function () {
        $scope.Transformation.VendorPartyId = null;
        $scope.Transformation.PartyName = null;
        $scope.Transformation.PartyCode = null;
    };
    $scope.closePartyPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.setPartyData = function (obj) {
        var data = obj.data;
        $scope.Transformation.PartyCode = data.Code;
        $scope.Transformation.VendorPartyId = data.Id;
        $scope.Transformation.PartyName = data.UserName;
        angular.element(document.querySelector('#PartyPopUp')).modal('hide');
    };
    // # end region

    $scope.TransformationModelTemp = {
        Id: null,
        Date: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Time: _Time,
        PlantId: null,
        EntityId: null,
        VendorPartyId: null,
        ProcessStartDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ProcessEndDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        ContractClosingDate: $filter('dateFiltering')(new Date(), 'dd-M-yyyy'),
        Remarks: null,
        PartyCode: null,
        PartyName: null,
        ContractStatus: "Active",
        ContractId: null,

    };
    $scope.Transformation = Object.assign({}, $scope.TransformationModelTemp);

    // #region ddl

    $scope.GetPlantList = function () {
        $scope.PlantList = [];
        $http({
            method: 'GET',
            url: 'JobWork/JobWorkValueAddedContract/getplant/'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;
            for (var p = 0; p < $scope.PlantList.length; p++) {
                if ($scope.PlantList[p].Value == $window.plantId) {
                    $scope.Transformation.PlantId = $scope.PlantList[p].Value;
                }

            }

        });
    }
    $scope.GetPlantList();

    $scope.getAllEntity = function () {
        if ($scope.Transformation.PlantId == null) {
            var PL = $window.plantId
            $scope.Transformation.PlantId = PL;
        }
        $http({
            method: 'GET',
            url: $scope.path + 'GetAllEntity?PlantId=' + $scope.Transformation.PlantId
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;

        });
    };
    $scope.getAllEntity();

    $scope.SaveTransformation = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TransformationForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveTransformation',
                data: { 'data': $scope.Transformation },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Transformation = response.data.Data;
                    $scope.Transformation.ContractId = $scope.Transformation.Id;
                    $scope.Getgrid();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.ClearTransformation = function () {
        $scope.Transformation = Object.assign({}, $scope.TransformationModelTemp);
        $scope.Action = 'Save';
        $scope.SelectedMatPlanningTabList = [];
        $scope.ClearMatPlanning();
    }

    $scope.DelTransData = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelTransData?Id=' + $scope.Transformation.Id
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getData();
                //$scope.getMatPlanningData();
                //ClearMatPlanning();
            }

        });
    }

    $scope.DelTransformation = function () {
        angular.element(document.querySelector("#DelTransformationData")).modal("show");
    }

    

   // Mat Planning of Transformation tab

    $scope.SelectedMatPlanningTabList = [];
    $scope.JobWorkItemMstList = [];
    $scope.MaterialLocList = [];
    $scope.OMatUOMList = [];
    $scope.ArticleList = [];
    $scope.RateList = [];
    $scope.CurrencyyyList = [];
    $scope.SelectedMaterialPlanningTabList = [];
    $scope.JobWorkActivityList = [];

    $http({
        method: 'GET',
        url: $scope.path + 'getactivitylistTransformation',
    }).then(function successCallback(response) {
        $scope.JobWorkActivityList = response.data;
       
    });

    $scope.GetJWItems = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getTransformationjobworkitemlist?ActivityId=' + $scope.MatPlanning.JobActivityId,
        }).then(function successCallback(response) {
            $scope.JobWorkItemMstList = response.data;
            $scope.MatPlanning.OutputMaterialUOMId = null;
            $scope.MatPlanning.ByProductApplicable = null;
            $scope.MaterialMstClear();
        });
    }

    $scope.GetTransmstList = [];
    $scope.GetJWitemDataFromTrans = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetJWitemDataFromTrans?ActivityId=' + $scope.MatPlanning.JobActivityId + '&JWItemId=' + $scope.MatPlanning.JobWorkItemMasterId,
        }).then(function successCallback(response) {
            $scope.GetTransmstList = response.data;
            if ($scope.GetTransmstList.length > 0) {
                
                $scope.MatPlanning.ByProductApplicable = $scope.GetTransmstList[0].ByProductApplicable;
            }
        });
    }
   

    $http({
        method: 'GET',
        url: 'JobWork/JobWorkValueAddedContract/getmateriallocation/',
    }).then(function successCallback(response) {
        $scope.MaterialLocList = response.data;
    });

    $http({
        method: 'GET',
        url: $scope.path + 'getoutputunit',
    }).then(function successCallback(response) {
        $scope.OMatUOMList = response.data;
    });

    $scope.GetArticle = function () {
        $scope.ArticleList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getarticlecode?JobWorkItemId=' + $scope.MatPlanning.JobWorkItemMasterId,
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
            if ($scope.ArticleList.length > 0) {
                $scope.MatPlanning.ArticleCodeId = $scope.ArticleList[0].Value;

            }
        });
    }

    $scope.GetRate = function () {
        $scope.RateList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'gettransformationrateapplylist?JobWorkItemId=' + $scope.MatPlanning.JobWorkItemMasterId + '&ActivityId=' + $scope.MatPlanning.JobActivityId,
        }).then(function successCallback(response) {
            $scope.RateList = response.data;
            if ($scope.RateList.length > 0) {
                $scope.MatPlanning.RateApplyId = $scope.RateList[0].Value;
                $scope.MatPlanning.RatePerUnit = $scope.RateList[0].MinRate;
                $scope.MatPlanning.MaxRate = $scope.RateList[0].MaxRate;
       
            }
        });
    }

    $scope.ValidateRate = function () {
        try {
            var MinimumRate = parseFloat($scope.MatPlanning.RatePerUnit);
            var MaximumRate = parseFloat($scope.MatPlanning.MaxRate);
            if (MinimumRate > MaximumRate) {
                $scope.MatPlanning.RatePerUnit = null;
                throw 'Rate Per Unit cannot be greater than Maximum Rate ' + MaximumRate + ' ';
            }
        }
        catch (e) {

            ShowResult(e, "failure");
            throw e;
        }
    }

    $scope.GetCurrencyyy = function () {
        $scope.CurrencyyyList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'gettransformationcurrency?JobWorkItemId=' + $scope.MatPlanning.JobWorkItemMasterId + '&ActivityId=' + $scope.MatPlanning.JobActivityId,
        }).then(function successCallback(response) {
            $scope.CurrencyyyList = response.data;
            if ($scope.CurrencyyyList.length > 0) {
                $scope.MatPlanning.CurrencyId = $scope.CurrencyyyList[0].Value;

            }
        });
    }
    $scope.GetMatMstJW = [];
    $scope.GetMaterialfromJW = function () {
        $scope.GetMatMstJW = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetMaterialfromJW?JobWorkItemId=' + $scope.MatPlanning.JobWorkItemMasterId,
        }).then(function successCallback(response) {
            $scope.GetMatMstJW = response.data;
            if ($scope.GetMatMstJW.length > 0) {
                $scope.MatPlanning.MaterialMasterId = $scope.GetMatMstJW[0].Id;
                $scope.MatPlanning.MaterialName = $scope.GetMatMstJW[0].Material;
                $scope.MatPlanning.MaterialCode = $scope.GetMatMstJW[0].Code;
                $scope.MatPlanning.OutputMaterialUOMId = $scope.GetMatMstJW[0].UnitId;

            }
        });
    }

    // #region field

    $scope.MaterialMstList = [];
    $scope.MaterialMstPopUp = function () {
        angular.element(document.querySelector("#MaterialPopUp")).modal("show");
        $scope.getMaterialMstDetailsData();

    }
    $scope.getMaterialMstDetailsData = function () {
        $scope.MaterialMstList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.MatPlanning.Id },
            url: $scope.path + 'LoadAllMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.MaterialMstList = response.data;
        });
    }

    $scope.MaterialMstClear = function () {
        $scope.MatPlanning.MaterialMasterId = null;
        $scope.MatPlanning.MaterialName = null;
        $scope.MatPlanning.MaterialCode = null;

    };
    $scope.closeMaterialMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialMstData = function (obj) {
        var data = obj.data;
        $scope.MatPlanning.MaterialCode = data.Code;
        $scope.MatPlanning.MaterialMasterId = data.Id;
        $scope.MatPlanning.MaterialName = data.MaterialName;
        $scope.MatPlanning.OutputMaterialUOMId = data.BaseUOMId;
        angular.element(document.querySelector('#MaterialPopUp')).modal('hide');
        $scope.MaterialMstArticlePopUp();
    };
    // # end region

    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.MaterialArticleMstList = [];
    $scope.MaterialMstArticlePopUp = function () {
        angular.element(document.querySelector("#MaterialArticlePopUp")).modal("show");
        $scope.getMaterialMstArticleData();

    }
    $scope.getMaterialMstArticleData = function () {
        $scope.MaterialArticleMstList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.MatPlanning.Id, MaterialMstId: $scope.MatPlanning.MaterialMasterId },
            url: $scope.path + 'LoadAllMaterialMstArticle'
        }).then(function successCallback(response) {
            $scope.MaterialArticleMstList = response.data;
        });
    }

    $scope.MaterialMstArticleClear = function () {
        $scope.MatPlanning.ArticleCodeId = null;
        $scope.MatPlanning.ArticleName = null;
        $scope.MatPlanning.ArticleCode = null;

    };
    $scope.closeMaterialArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setMaterialArticleData = function (obj) {
        var data = obj.data;
        $scope.MatPlanning.ArticleCode = data.ArticleCode;
        $scope.MatPlanning.ArticleCodeId = data.ArticleId;
        $scope.MatPlanning.ArticleName = data.StandardName;
        angular.element(document.querySelector('#MaterialArticlePopUp')).modal('hide');
    };
    // # end region

    $scope.MatPlanningModelTemp = {
        Id: null,
        JobWorkTransformationContractMasterId: null,
        JobWorkItemMasterId: null,
        MaterialSpecification: null,
        MaterialReference: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        ArticleCodeId: null,
        OrderSpecific: null,
        RequiredCapacity: null,
        ByProductApplicable: null,
        RateApplyId: null,
        CurrencyId: null,
        RatePerUnit: null,
        Rejection: null,
        ValueLoss: null,
        ResponsiblePersonId: null,
        Remarks: null,
        FileName: null,
        MaterialLocationId: null,
        MaterialType: null,
        FinalOutputCategory: null,
        JobActivityId: null,
        MaterialCode: null,
        MaterialName: null,
        MaterialMasterId: null,
        ArticleCode: null,
        ArticleName: null,
        EmployeeCode: null,
        ResponsiblePerson: null,
        EmployeeStatus: null,
        Tolerance:null,

    };
    $scope.MatPlanning = Object.assign({}, $scope.MatPlanningModelTemp);

    //File Attachment-----Start
    //Attach and File

    $("#uploadattachment").change(function () {
        $scope.filedata = this.files[0];
    });

    document.getElementById('uploadattachment').onchange = function () {
        var filename = document.getElementById('uploadFile').value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById('uploadFile').value = res;

    };

    //File Download

    $scope.FileDownloadURL = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
     //   $scope.dwonloadUrl = 'E:\Shash\Aplos\NewlyProject\LatestProject1\ApopMainProjectMaster\APLOS\POPResources\JobWork\JobWorkTransformationContract' + '/' + data.FileName;
        $scope.dwonloadUrl = virtualPath.JobWorkTransformationContract + '/' + data.FileName;
    };

    //Detach file  button Method and id confirmDocumentDelete
    $scope.DocumentRemove = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
    };
    $scope.removeDoc = function () {

        document.getElementById('uploadattachment').value = '';
        $scope.filedata = '';
        $scope.MatPlanning.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
    };

    //MatPlanning Detach file method
    $scope.confirmCloseDocumentDelete = function () {
        angular.element(document.querySelector('#confirmDocumentDelete')).modal('hide');
    };
    // Clear Method for MatPlanning
    function ClearDocument() {
        document.getElementById('uploadattachment').value = '';
        $scope.filedata = '';
        $scope.MatPlanning.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
    };

    //File Attachment-----End


    $scope.SaveMatPlanning = function () {
        $scope.MatPlanning.JobWorkTransformationContractMasterId = $scope.Transformation.Id;
        //      $scope.$broadcast('show-errors-check-validity');
        //     if ($scope.FarmerMasterPlotForm.$valid) {
        if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
            throw $scope.filedata.name + ' File size must be below 2 mb';
        var fileName = null;
        if (!baseService.isUndefinedOrNull($scope.filedata))
            fileName = $scope.filedata.name;
        $scope.MatPlanning.FileName = fileName;
        if (!baseService.isUndefinedOrNull($scope.MatPlanning.FileName)) {
            if ($scope.MatPlanning.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
        }
        var formData = new FormData();
        $http({
            method: 'POST',
            url: $scope.path + 'saveUrlMatPlanning',
            headers: { 'Content-Type': undefined },
            transformRequest: function (data) {
                formData.append("MatPlanning", angular.toJson(data.MatPlanning));
                if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                    formData.append('file', data.file);
                }
                return formData;
            },
            data: { 'MatPlanning': $scope.MatPlanning, 'file': $scope.filedata }


        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MatPlanning = response.data.Data;
                $scope.getMatPlanningData();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

        //    }
    };

    //$scope.downloadgriddataUrl = 'GridReports/Download';

    //$scope.ToDownloadFilePath = function (data) {
    //    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + data);
    //}
    

    $scope.ClearMatPlanning = function () {
        $scope.MatPlanning = Object.assign({}, $scope.MatPlanningModelTemp);
        $scope.getMatPlanningData();
        $scope.SaveAction = 'Save';
    }

    $scope.SaveAction = 'Save';

    $scope.GetMatPlanningDataToEdit = function (args) {
        $scope.MatPlanning = Object.assign({}, args.data);
        $scope.GetJWItems();
        $scope.GetRate();
        $scope.GetCurrencyyy();
        $scope.GetJWitemDataFromTrans();
        $scope.getMatMstDataToEdit();
        $scope.SaveAction = 'Update';
     
    };

    $scope.MatMstDataToEdit = [];
    $scope.getMatMstDataToEdit = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMatMstDataToEdit?ArticleId=' + $scope.MatPlanning.ArticleCodeId
        }).then(function successCallback(response) {
            $scope.MatMstDataToEdit = response.data;
            if ($scope.MatMstDataToEdit.length > 0) {
                $scope.MatPlanning.MaterialName = $scope.MatMstDataToEdit[0].MaterialName;
                $scope.MatPlanning.MaterialCode = $scope.MatMstDataToEdit[0].MaterialCode;
                $scope.MatPlanning.MaterialMasterId = $scope.MatMstDataToEdit[0].MaterialMasterId;
                $scope.MatPlanning.OutputMaterialUOMId = $scope.MatMstDataToEdit[0].OutputMaterialUOMId;
            }

        });
    }
   
    $scope.getMatPlanningData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMatPlanningData?MasterId=' + $scope.Transformation.Id
        }).then(function successCallback(response) {
            $scope.SelectedMatPlanningTabList = response.data;
        });
    }


    $scope.DelMatPlanning = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelMatPlanning?Id=' + $scope.ChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMatPlanningData();
                $scope.ClearMatPlanning();
            }

        });
    }

    $scope.ConfirmDeleteMatPlotTab = function (Id) {
        $scope.ChildTabId = Id;
        angular.element(document.querySelector("#DelChildTabPopUp")).modal("show");
    }

    // #region field

    $scope.EmployeeResPersonList = [];
    $scope.ResPersonPopUp = function () {
        angular.element(document.querySelector("#EmpPopUpResPerson")).modal("show");
        $scope.getEmpData();

    }
    $scope.getEmpData = function () {
        $scope.EmployeeResPersonList = [];
        $http({
            method: 'POST',
            data: { Id: $scope.Transformation.Id },
            url: $scope.path + 'LoadAllResponsiblePersonDetails'
        }).then(function successCallback(response) {
            $scope.EmployeeResPersonList = response.data;
        });
    }

    $scope.ResPersonClear = function () {
        $scope.MatPlanning.ResponsiblePersonId = null;
        $scope.MatPlanning.ResponsiblePerson = null;
        $scope.MatPlanning.EmployeeCode = null;
        $scope.MatPlanning.EmployeeStatus = null;

    };
    $scope.closePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmployeeData = function (obj) {

        var data = obj.data;
        $scope.MatPlanning.EmployeeCode = data.Code;
        $scope.MatPlanning.ResponsiblePersonId = data.Id;
        $scope.MatPlanning.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmpPopUpResPerson')).modal('hide');
    };
    // # end region


    //********** Tab end ***************

    // ORDER WISE REQUIREMENT TAB UNDER TRANSFORMATION TAB

    // Order Wise Requirement tab

    //  $scope.MonthsList = [];
    $scope.ConfirmOrderWisePopUp = function (data) {
        $scope.MatPlanningTabId = data.Id;
        $scope.UnitId = data.OutputMaterialUOMId;
        $scope.TransformOrderWiseReq.Quantity = data.Quantity;
        $scope.PQuantity = data.Quantity;
        $scope.TransformOrderWiseReq.PlanQuantity = $scope.PQuantity;
        $scope.TransformOrderWiseReq.ArtclCode = data.ArticleCode
        $scope.GetTransformOrderWiseUOM();
        $scope.getTransformOrderWiseData();
        angular.element(document.querySelector("#OrderWisePopUp")).modal("show");

    }


    $scope.closeTransformOrderWiseReqTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.TransformOrderWiseRequirementList = [];
    $scope.AllCustomerList = [];
    $scope.AllMasterOrderNoList = [];
    $scope.AllMasterOrderItemList = [];
    $scope.AllUOMList = [];

    $http({
        method: 'GET',
        url: $scope.path + 'getcustomerlist',
    }).then(function successCallback(response) {
        $scope.AllCustomerList = response.data;
    });

    $scope.GetAllMasterOrderNo = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterorderlist?CustomerId=' + $scope.TransformOrderWiseReq.CustomerId,
        }).then(function successCallback(response) {
            $scope.AllMasterOrderNoList = response.data;
        });
    }

    $scope.GetAllMasterOrderItem = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterorderitemlist?MasterOrderNoId=' + $scope.TransformOrderWiseReq.MasterOrderNoId,
        }).then(function successCallback(response) {
            $scope.AllMasterOrderItemList = response.data;
        });
    }

    $scope.GetTransformOrderWiseUOM = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getoutputunit',
        }).then(function successCallback(response) {
            $scope.AllUOMList = response.data;
            if (baseService.arrayLength($scope.AllUOMList) > 0) {
                $scope.TransformOrderWiseReq.OutputMaterialUOMId = $scope.UnitId;
            }
        });
    }


    $scope.TransformOrderWiseReqModelTemp = {
        Id: null,
        JobWorkTransformationContractChildMasterId: null,
        OrderType: null,
        CustomerId: null,
        MasterOrderNoId: null,
        MasterOrderItemId: null,
        ParticularSpecification: null,
        Remarks: null,
        OutputMaterialUOMId: null,
        Quantity: null,
        PlanQuantity: null,
        ArtclCode: null,

    };
    $scope.TransformOrderWiseReq = Object.assign({}, $scope.TransformOrderWiseReqModelTemp);

    $scope.SaveTransformOrderWiseReqTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.TransformOrderWiseReqForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'SaveTransformOrderWiseReqTab',
                data: { 'data': $scope.TransformOrderWiseReq, 'ChildMasterId': $scope.MatPlanningTabId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TransformOrderWiseReq = response.data.Data;
                    ClearFieldsTransformOrderWiseChildData();
                    $scope.getTransformOrderWiseData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DelTransformOrderWise = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelTransformOrderWise?Id=' + $scope.TransformOrderWiseChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getTransformOrderWiseData();
                ClearFieldsTransformOrderWiseChildData();
            }

        });
    }

    $scope.ConfirmDeleteTransformOrderWiseTab = function (Id) {
        $scope.TransformOrderWiseChildTabId = Id;
        angular.element(document.querySelector("#DelTransformOrderWiseChildTabPopUp")).modal("show");
    }

    $scope.ClearTransformOrderWiseReqTab = function () {
        ClearFieldsTransformOrderWiseChildData();
    }

    function ClearFieldsTransformOrderWiseChildData() {
        $scope.TransformOrderWiseReq.Id = null;
        $scope.TransformOrderWiseReq.JobWorkTransformationContractChildMasterId = null;
        $scope.TransformOrderWiseReq.OrderType = null;
        $scope.TransformOrderWiseReq.CustomerId = null;
        $scope.TransformOrderWiseReq.MasterOrderNoId = null;
        $scope.TransformOrderWiseReq.MasterOrderItemId = null;
        $scope.TransformOrderWiseReq.ParticularSpecification = null;
        $scope.TransformOrderWiseReq.Remarks = null;
        $scope.TransformOrderWiseReq.PlanQuantity = $scope.PQuantity;
        $scope.GetTransformOrderWiseUOM();
    }

    $scope.getTransformOrderWiseData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getTransformOrderWiseData?MaterialMasterId=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.TransformOrderWiseRequirementList = response.data;

        });
    }

    // MATERIAL INPUT TAB OF TRANSFORMATION TAB

    // MATERIAL INPUT TAB UNDER TRANSFORMATION TAB

    // MATERIAL INPUT tab

    $scope.ConfirmMaterialInputPopUp = function (data) {
         $scope.MatPlanningTabId = data.Id;
        $scope.JWInputId = data.JobWorkItemMasterId;
        $scope.JWActivityId = data.JobActivityId;
        $scope.getMatInputListData();
        $scope.getMaterialInputData();
        
        angular.element(document.querySelector("#MaterialInputPopUp")).modal("show");
    }


    $scope.closeMaterialInputTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.MaterialInputList = [];
    //$scope.MaterialMasterList = [];
    //$scope.InputUOMList = [];

    $scope.MatInputList = [];
    $scope.getMatInputListData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMatInputListData?JobWorkItemId=' + $scope.JWInputId + '&ActivityId=' + $scope.JWActivityId + '&Id=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.MatInputList = response.data;
        });
    }

    // Select All Check Box 

    $scope.refreshTemplateMatInput = function () {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectMatInput });
    };

    function CheckBoxSelectMatInput(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridMatInput").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.MatInputList.length; i++) {
                $scope.MatInputList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridMatInput").data("ejGrid");
        gridObj.refreshContent();
    };


    //$http({
    //    method: 'GET',
    //    url: $scope.path + 'getmateriallist',
    //}).then(function successCallback(response) {
    //    $scope.MaterialMasterList = response.data;
    //});

  
    //$scope.GetMaterialInputUOM = function () {
    //    $http({
    //        method: 'GET',
    //        url: $scope.path + 'getoutputunit',
    //    }).then(function successCallback(response) {
    //        $scope.InputUOMList = response.data;
    //        if (baseService.arrayLength($scope.InputUOMList) > 0) {
    //            $scope.MaterialInput.InputMaterialUOMId = $scope.UnitId;
    //        }
    //    });
    //}

    $scope.GetGrossConsumption = function (data) {

        for (var i = 0; i < $scope.MatInputList.length > 0; i++) {
            if ($scope.MatInputList[i].Id === data.Id) {

                if ($scope.MatInputList[i].NetConsumption !== null && $scope.MatInputList[i].ValueLoss !== null && $scope.MatInputList[i].Rejection !== null) {
                    var NConsumption = parseFloat($scope.MatInputList[i].NetConsumption);
                    var VLoss = parseFloat($scope.MatInputList[i].ValueLoss);
                    var Rejection = parseFloat($scope.MatInputList[i].Rejection);
                    //    var Res = Math.abs((NConsumption) / (100 - VLoss));
                    var Res = Math.abs(NConsumption * (parseFloat(1) + (VLoss / 100) + (Rejection / 100)));
              //      var Result = Math.abs(Res * 100);
                    var RoundRes = Math.round(Res * 100) / 100;
                    $scope.MatInputList[i].GrossConsumption = RoundRes;
                }
            }
        }
     
    }


    $scope.MaterialInputModelTemp = {
        Id: null,
        JobWorkTransformationContractChildMasterId: null,
        MaterialMasterId: null,
        MaterialSpecification: null,
        InputMaterialUOMId: null,
        NetConsumptionOutputUnit: null,
        Rejection: null,
        ValueLoss: null,
        GrossConsumption: null,
        ResponsiblePersonId: null,
        Remarks: null,
     
    };
    $scope.MaterialInput = Object.assign({}, $scope.MaterialInputModelTemp);

    //Save Function 
    $scope.SaveMaterialInputTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var MatInputSelData = [];
        for (var i = 0; i < $scope.MatInputList.length; i++) {
            if ($scope.MatInputList[i].isSelected == true)
                MatInputSelData.push($scope.MatInputList[i]);
        }
        try {
            if (MatInputSelData.length == 0) {
                throw 'Please Select at least one Material Input';
            }
            $http({
                method: 'POST',
                data: { SelectedMatInputData: MatInputSelData, ChildMasterId: $scope.MatPlanningTabId },
                url: $scope.path + 'SaveMaterialInputTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.getMatInputListData();
                    $scope.getMaterialInputData();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    //$scope.SaveMaterialInputTab = function () {
    //    $scope.$broadcast('show-errors-check-validity');
    //    if ($scope.MatInputNewForm.$valid) {
    //        $http({
    //            method: 'POST',
    //            url: $scope.path + 'SaveMaterialInputTab',
    //            data: { 'data': $scope.MaterialInput, 'ChildMasterId': $scope.MatPlanningTabId },
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.MaterialInput = response.data.Data;
    //                ClearFieldsMaterialInputChildData();
    //                $scope.getMaterialInputData();

    //            }
    //        }), function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    }
    //};

    $scope.DelMaterialInput = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelMaterialInput?Id=' + $scope.MaterialInputChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getMaterialInputData();
                $scope.getMatInputListData();
                //ClearFieldsMaterialInputChildData();
            }

        });
    }

    $scope.ConfirmDeleteMaterialInputTab = function (Id) {
        $scope.MaterialInputChildTabId = Id;
        angular.element(document.querySelector("#DelMaterialInputChildTabPopUp")).modal("show");
    }

    //$scope.ClearMaterialInputTab = function () {
    //    ClearFieldsMaterialInputChildData();
    //}

    //function ClearFieldsMaterialInputChildData() {
    //    $scope.MaterialInput.Id = null;
    //    $scope.MaterialInput.JobWorkTransformationContractChildMasterId = null;
    //    $scope.MaterialInput.MaterialMasterId = null;
    //    $scope.MaterialInput.MaterialSpecification = null;
    //    $scope.MaterialInput.NetConsumptionOutputUnit = null;
    //    $scope.MaterialInput.Rejection = null;
    //    $scope.MaterialInput.ValueLoss = null;
    //    $scope.MaterialInput.GrossConsumption = null;
    //    $scope.MaterialInput.ResponsiblePersonId = null;
    //    $scope.MaterialInput.Remarks = null;
    //    $scope.MaterialInputResPersonClear();
    //    $scope.GetMaterialInputUOM();
    //}

    $scope.getMaterialInputData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getMaterialInputData?MaterialMasterId=' + $scope.MatPlanningTabId
        }).then(function successCallback(response) {
            $scope.MaterialInputList = response.data;

        });
    }

    // #region field

    //$scope.MaterialInputEmployeeResPersonList = [];
    //$scope.MaterialInputResPersonPopUp = function () {
    //    angular.element(document.querySelector("#MaterialInputEmpPopUpResPerson")).modal("show");
    //    $scope.getMatInputEmpData();

    //}
    //$scope.getMatInputEmpData = function () {
    //    $scope.MaterialInputEmployeeResPersonList = [];
    //    $http({
    //        method: 'POST',
    //        data: { Id: $scope.MatPlanningTabId },
    //        url: $scope.path + 'LoadMatInputResponsiblePersonDetails'
    //    }).then(function successCallback(response) {
    //        $scope.MaterialInputEmployeeResPersonList = response.data;
    //    });
    //}

    //$scope.MaterialInputResPersonClear = function () {
    //    $scope.MaterialInput.ResponsiblePersonId = null;
    //    $scope.MaterialInput.ResponsiblePerson = null;
    //    $scope.MaterialInput.EmployeeCode = null;
    //    $scope.MaterialInput.EmployeeStatus = null;

    //};
    //$scope.closeMaterialInputTabPopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}
    //$scope.setMaterialInputEmployeeData = function (obj) {

    //    var data = obj.data;
    //    $scope.MaterialInput.EmployeeCode = data.Code;
    //    $scope.MaterialInput.ResponsiblePersonId = data.Id;
    //    $scope.MaterialInput.ResponsiblePerson = data.EmployeeName;
    //    angular.element(document.querySelector('#MaterialInputEmpPopUpResPerson')).modal('hide');
    //};
    // # end region

    // BY PRODUCT TAB

    // Select All Check Box 

    $scope.refreshTemplateemployee = function () {
        $("#BPheadchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        var filtered = $("#GridByProductMaster").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ByProductMasterList.length; i++) {
                $scope.ByProductMasterList[i].isSelected = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {

                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridByProductMaster").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.ConfirmByProductPopUp = function (data) {
        $scope.MatInputTabId = data.Id;
  //      $scope.UnitId = data.InputMaterialUOMId;
  //      $scope.GetByProductUOM();
          $scope.getByProductMasterData();
          $scope.getByProductData();
        angular.element(document.querySelector("#ByProductPopUp")).modal("show");

    }


    $scope.closeByProductTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }

    $scope.ByProductList = [];
  //  $scope.MaterialMstList = [];
 //   $scope.UnitOMList = [];

    $scope.ByProductMasterList = [];
    $scope.getByProductMasterData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getByProductMasterData?JobWorkItemId=' + $scope.JWInputId + '&ActivityId=' + $scope.JWActivityId + '&Id=' + $scope.MatInputTabId
        }).then(function successCallback(response) {
            $scope.ByProductMasterList = response.data;
        });
    }

    // #region field By product

    $scope.BPMaterialMstList = [];
    $scope.BPMaterialMstPopUp = function (data) {
        angular.element(document.querySelector("#BPMaterialPopUp")).modal("show");
        $scope.getMaterialDetailsData(data);
    }

    $scope.getMaterialDetailsData = function (data) {
        $scope.BPMaterialMstList = [];

        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {
                $scope.MatMstId = $scope.ByProductMasterList[i].BPMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'LoadMaterialMstDetails'
        }).then(function successCallback(response) {
            $scope.BPMaterialMstList = response.data;
        });
    }

    $scope.BPMaterialMstClear = function (data) {
        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {
                $scope.ByProductMasterList[i].BPMaterialId = null;
                $scope.ByProductMasterList[i].BPMaterialCode = null;
                $scope.ByProductMasterList[i].ByProductMaterial = null;
            }
        }
    };

    $scope.closeBPMaterialMstPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setBPMaterialMstData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.ByProductMasterList[b].BPMaterialId = data.Id;
        $scope.ByProductMasterList[b].BPMaterialCode = data.Code;
        $scope.ByProductMasterList[b].ByProductMaterial = data.MaterialName;

        $scope.ByProductMasterList[b].BPArticleId = null;
        $scope.ByProductMasterList[b].BPArticleCode = null;
        $scope.ByProductMasterList[b].BPArticleName = null;

        angular.element(document.querySelector('#BPMaterialPopUp')).modal('hide');
    };
    // # end region


    // GET ARTICLE
    // MATERIAL MASTER ARTICLE
    // #region field

    $scope.BPMaterialArticleMstList = [];
    $scope.BPMaterialMstArticlePopUp = function (RowData) {
        angular.element(document.querySelector("#BPMaterialArticlePopUp")).modal("show");
        $scope.getArticleData(RowData);

    }
    $scope.getArticleData = function (RowData) {
        $scope.BPMaterialArticleMstList = [];

        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === RowData.Id) {
                $scope.MatMstId = $scope.ByProductMasterList[i].BPMaterialId;
                $scope.a = i;
            }
        }

        $http({
            method: 'POST',
            data: { MaterialMstId: $scope.MatMstId },
            url: $scope.path + 'LoadMaterialMstArticle'
        }).then(function successCallback(response) {
            $scope.BPMaterialArticleMstList = response.data;
        });
    }

    $scope.BPMaterialMstArticleClear = function (data) {
        for (var i = 0; i < $scope.ByProductMasterList.length > 0; i++) {
            if ($scope.ByProductMasterList[i].Id === data.Id) {

                $scope.ByProductMasterList[i].BPArticleId = null;
                $scope.ByProductMasterList[i].BPArticleCode = null;
                $scope.ByProductMasterList[i].BPArticleName = null;
            }
        }
    };

    $scope.closeBPMaterialArticlePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setBPMaterialArticleData = function (obj) {
        var b = $scope.a;
        var data = obj.data;
        $scope.ByProductMasterList[b].BPArticleId = data.ArticleId;
        $scope.ByProductMasterList[b].BPArticleCode = data.ArticleCode;
        $scope.ByProductMasterList[b].BPArticleName = data.StandardName;
        //$scope.SelectedArticleId = data.ArticleId;
        //$scope.GetByDefaultRate($scope.a);
        //$scope.GetLotNumberList($scope.a);
        angular.element(document.querySelector('#BPMaterialArticlePopUp')).modal('hide');
    };

    $scope.ByProductModelTemp = {
        Id: null,
        JobWorkTransformationContractChild3MasterId: null,
        MaterialMasterId: null,
        MaterialSpecification: null,
        StandardQuantityInputUnit: null,
        CurrencyId: null,
        StandardRatePerUnit: null,
        ResponsiblePersonId: null,
        Remarks: null,
        Tolerance: null,

    };
    $scope.ByProduct = Object.assign({}, $scope.ByProductModelTemp);

    // Save Function for By Product(Transformation)

    //Save Function 
    $scope.SaveByProductTab = function () {
        $scope.$broadcast('show-errors-check-validity');
        var checkedData = [];
        try {
        for (var i = 0; i < $scope.ByProductMasterList.length; i++) {
            if ($scope.ByProductMasterList[i].isSelected == true) {
                if ($scope.ByProductMasterList[i].StandardRate > 0) {
                    checkedData.push($scope.ByProductMasterList[i]);
                }
                else {
                    throw 'Standard Rate should be greater than zero';
                }
                if ($scope.ByProductMasterList[i].Tolerance > 0) {
                    checkedData.push($scope.ByProductMasterList[i]);
                }
                else {
                    throw 'Tolerance should be greater than zero';
                }
            }       
        }
        
            if (checkedData.length == 0) {
                throw 'Please Select at least one By Product';
            }
            $http({
                method: 'POST',
                data: { ByProductMstData: checkedData, ChildMasterId: $scope.MatInputTabId },
                url: $scope.path + 'SaveByProductTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
           //         $scope.IssueChild = response.data.Data;
                    $scope.getByProductMasterData();
                    $scope.getByProductData();
                }
            });

        }
        catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.DelByProduct = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DelByProduct?Id=' + $scope.ByProductTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");

                $scope.getByProductData();
                $scope.getByProductMasterData();
            //    ClearFieldsByProductChildData();
            }

        });
    }

    $scope.ConfirmDeleteByProductTab = function (Id) {
        $scope.ByProductTabId = Id;
        angular.element(document.querySelector("#DelByProductTabPopUp")).modal("show");
    }

    $scope.getByProductData = function () {

        $http({
            method: 'GET',
            url: $scope.path + 'getByProductData?MaterialInputId=' + $scope.MatInputTabId
        }).then(function successCallback(response) {
            $scope.ByProductList = response.data;

        });
    }

    // #region field

    //$scope.ByProductEmployeeResPersonList = [];
    //$scope.ByProductResPersonPopUp = function () {
    //    angular.element(document.querySelector("#ByProductEmpPopUpResPerson")).modal("show");
    //    $scope.getbyproductEmpData();

    //}
    //$scope.getbyproductEmpData = function () {
    //    $scope.ByProductEmployeeResPersonList = [];
    //    $http({
    //        method: 'POST',
    //        data: { Id: $scope.MatInputTabId },
    //        url: $scope.path + 'LoadByProductResponsiblePersonDetails'
    //    }).then(function successCallback(response) {
    //        $scope.ByProductEmployeeResPersonList = response.data;
    //    });
    //}

    //$scope.ByProductResPersonClear = function () {
    //    $scope.ByProduct.ResponsiblePersonId = null;
    //    $scope.ByProduct.EmployeeName = null;
    //    $scope.ByProduct.EMPCode = null;
    //    $scope.ByProduct.EMPStatus = null;

    //};
    //$scope.closeByProductPopUp = function (popupName) {
    //    angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    //}
    //$scope.setByProductEmpData = function (obj) {

    //    var data = obj.data;
    //    $scope.ByProduct.EMPCode = data.Code;
    //    $scope.ByProduct.ResponsiblePersonId = data.Id;
    //    $scope.ByProduct.EmployeeName = data.EmployeeName;
    //    angular.element(document.querySelector('#ByProductEmpPopUpResPerson')).modal('hide');
    //};
    // # end region

    //#region start Reports
    $scope.ConfirmPrintTab = function (data) {
        try {
            $scope.PrintTabId = data.Id;
            var TabType = data.TabType;
            if (TabType == "Value Added") {
                //     var data = args.data;
                var reportFormat = "Excel";
                window.open('JobWork/JobWorkValueAddedContract/GetValueAddedPrintReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
                $scope.getData();
            }
            if (TabType == "Transformation") {
                //     var data = args.data;
                var reportFormat = "Excel";
                window.open('JobWork/JobWorkValueAddedContract/GetTransformationContractReport?reportFormat=' + reportFormat + '&PrintTabId=' + $scope.PrintTabId, '_blank');
                $scope.getData();
            }

        } catch (e) {

        }
    };

    //#endregion end Value Added Contract Reports


}