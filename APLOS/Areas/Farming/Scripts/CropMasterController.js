'use strict';
CropMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function CropMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Crop Master';
    $scope.CropMasterList = [];
    $scope.SelectedCropTypeTabList = [];
    $scope.SelectedCropProcessTabList = [];
    $scope.SelectedCropProcessMonthTabList = [];
  
    $scope.CropCategoryList = [];
    $scope.CropSubCategoryList = [];
    $scope.AreaUOMList = [];
    $scope.OutputUOMList = [];
    $scope.TransactionTypeList = [];
   
    $scope.path = 'Farming/CropMaster/';

    $scope.getListUrl = $scope.path + 'getlist';
   
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlCropType = $scope.path + 'SaveCropType';
    $scope.saveUrlCropProcess = $scope.path + 'SaveCropProcess';
    $scope.saveUrlCropProcessMonth = $scope.path + 'SaveCropProcessMonth';
 
    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'ShortName', name: "Short Name" }, { value: 'UserName', name: "User Name" }, { value: 'Code', name: "Code" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/getcropcategory/',
    }).then(function successCallback(response) {
        $scope.CropCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/getcropsubcategory/',
    }).then(function successCallback(response) {
        $scope.CropSubCategoryList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/gettransactiontype/',
    }).then(function successCallback(response) {
        $scope.TransactionTypeList = response.data;
    });

    //$http({
    //    method: 'GET',
    //    url: 'Farming/CropMaster/getoutputuom/',
    //}).then(function successCallback(response) {
    //    $scope.OutputUOMList = response.data;
    //});

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.AreaUOMList = response;
    });

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.OutputUOMList = response;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.CropMasterList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
        $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        UserName: null,
        CropCategoryId: null,
        CropSubCategoryId: null,
        AreaUOMId: null,
        OutputUOMId: null,
        MinimumOutput: null,
        MaximumOutput: null,
        AvreageOutput: null,
        MinimumRate: null,
        MaximumRate: null,
        AverageRate: null,
        ResponsiblePersonId: null,
        Remarks: null,
        EmployeeStatus: null,
        TransactionTypeId: null,
};
    $scope.CropMaster = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.CropMaster.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.CropMaster = Object.assign({}, args.data);
        $scope.getCropTypeData($scope.CropMaster.Id);
        $scope.getCropProcessData($scope.CropMaster.Id);
        $scope.GetSequenceCropProcess($scope.CropMaster.Id);
        $scope.LoadAllSelectedMonthsTab();
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
            $scope.CropMasterList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.CropMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CropMaster = response.data.Data;
                  
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                    $scope.getCropTypeData($scope.CropMaster.Id);
                    $scope.getCropProcessData($scope.CropMaster.Id);
                    $scope.LoadAllSelectedMonthsTab(); 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.CropMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.CropMaster.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
       
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.CropMaster = Object.assign({}, $scope.ModelTemp);
        $scope.CropMaster.Sequence = seq;
        $scope.SelectedCropTypeTabList = [];
        $scope.getCropTypeData($scope.CropMaster.Id);
        $scope.getCropProcessData($scope.CropMaster.Id);
        $scope.GetSequenceCropProcess($scope.CropMaster.Id);
        $scope.SelectedCropProcessTabList = [];
        $scope.SelectedCropProcessMonthTabList = [];
        $scope.LoadAllSelectedMonthsTab();
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
            data: { Id: $scope.CropMaster.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.CropMaster.ResponsiblePersonId = null;
        $scope.CropMaster.ResponsiblePerson = null;
        $scope.CropMaster.EmployeeCode = null;
        $scope.CropMaster.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.CropMaster.EmployeeCode = data.Code;
        $scope.CropMaster.ResponsiblePersonId = data.Id;
        $scope.CropMaster.ResponsiblePerson = data.EmployeeName;
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

 // *************** Crop Type Tab *******************

    $scope.CropTypeList = [];
    $scope.LandCategoryList = [];

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/getcroptype/',
    }).then(function successCallback(response) {
        $scope.CropTypeList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/getlandcategory/',
    }).then(function successCallback(response) {
        $scope.LandCategoryList = response.data;
        });

    $scope.CropTypeModelTemp = {
        Id: null,
        CropMasterId: null,
        CropTypeId: null,
        LandCategoryId: null,
        MinimumOutput: null,
        MaximumOutput: null,
        AvreageOutput: null,
        MinimumRate: null,
        MaximumRate: null,
        AverageRate: null,
        Remarks: null,
     
    };
    $scope.CropType = Object.assign({}, $scope.CropTypeModelTemp);

    $scope.SaveCropType = function () {
        $scope.CropType.CropMasterId = $scope.CropMaster.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.croptypeForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlCropType,
                data: { 'data': $scope.CropType },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.CropType = response.data.Data;
                    
                    $scope.getCropTypeData($scope.CropMaster.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsCropType() {
       
        $scope.CropType = Object.assign({}, $scope.CropTypeModelTemp);
        

    }

    $scope.getCropTypeData = function (CropMasterId) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetListCropType?CropMasterId=' + CropMasterId
        }).then(function successCallback(response) {
            $scope.SelectedCropTypeTabList = response.data;
            ClearFieldsCropType();
        });
    }


    $scope.DeleteCropType = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedCropTypeTab?Id=' + $scope.CropTypeTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCropTypeData($scope.CropMaster.Id);
                ClearFieldsCropType();
            }

        });
    }

    $scope.ConfirmDeleteCropTypeTab = function (Id) {
        $scope.CropTypeTabId = Id;
        angular.element(document.querySelector("#DeleteCropTypeTabPopUp")).modal("show");
    }
    //********** Tab end ***************

    // *************** Crop Process Tab ****************

    $scope.FarmingProcessList = [];
   

    $http({
        method: 'GET',
        url: 'Farming/CropMaster/getfarmingprocess/',
    }).then(function successCallback(response) {
        $scope.FarmingProcessList = response.data;
    });


    $scope.CropProcessModelTemp = {
        Id: null,
        CropMasterId: null,
        Sequence: 0,
        FarmingProcessId: null,
        StandardDays: null,
        StandardDuration: null,
        Remarks: null,

    };
    $scope.cropprocess = Object.assign({}, $scope.CropProcessModelTemp);

    $scope.GetSequenceCropProcess = function (CropMasterId) {
        $http.get("Farming/CropMaster/GetAutoSequenceCropProcess?CropMasterId=" + CropMasterId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.cropprocess.Sequence = response.data[0].Sequence;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };

    $scope.SaveCropProcess = function () {
        $scope.cropprocess.CropMasterId = $scope.CropMaster.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.cropprocessForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlCropProcess,
                data: { 'data': $scope.cropprocess },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.cropprocess = response.data.Data;
                    $scope.getCropProcessData($scope.CropMaster.Id);
                    $scope.GetSequenceCropProcess($scope.CropMaster.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsCropProcess(seq) {

        $scope.cropprocess = Object.assign({}, $scope.CropProcessModelTemp);
        $scope.cropprocess.Sequence = seq;
        $scope.GetSequenceCropProcess($scope.CropMaster.Id);


    }

    $scope.getCropProcessData = function (CropMasterId) {

        $http({
            method: 'GET',
            url: $scope.path + 'GetListCropProcess?CropMasterId=' + CropMasterId
        }).then(function successCallback(response) {
            $scope.SelectedCropProcessTabList = response.data;
            ClearFieldsCropProcess(response.data.Sequence);
        });
    }


    $scope.DeleteCropProcess = function () {
        $http({
            method: 'POST',
            data: { CropMasterId: $scope.CropMaster.Id, Id: $scope.CropProcessTabId},
            url: $scope.path + 'DeleteSelectedCropProcessTab'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getCropProcessData($scope.CropMaster.Id);
                $scope.GetSequenceCropProcess($scope.CropMaster.Id);
            }

        });
    }

    $scope.ConfirmDeleteCropProcessTab = function (Id) {
        $scope.CropProcessTabId = Id;
        angular.element(document.querySelector("#DeleteCropProcessTabPopUp")).modal("show");
    }

    //************ End *********************

    // ********* Crop Process Month Tab ************************8

    $scope.MonthsList = [];
    $scope.ConfirmPopUpMonths = function (FarmingProcessId) {
        $scope.FarmingProcessTabId = FarmingProcessId;
        angular.element(document.querySelector("#MonthsPopUp")).modal("show");
        $scope.getMonthsTabData();

    }
    $scope.getMonthsTabData = function () {
        $scope.MonthsList = [];

        $http({
            method: 'POST',
            data: { CropMasterId: $scope.CropMaster.Id, FarmingProcessId: $scope.FarmingProcessTabId },
            url: $scope.path + 'LoadAllMonthsForSelection'
        }).then(function successCallback(response) {
            $scope.MonthsList = response.data;
        });
    }

    $scope.SelectedCropProcessMonthTabList = [];
    $scope.LoadAllSelectedMonthsTab = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'LoadAllSelectedMonthsTab?CropMasterId=' + $scope.CropMaster.Id
        }).then(function successCallback(response) {
            $scope.SelectedCropProcessMonthTabList = response.data;
        });
    }


    //Save Function In CropprocessMonth Table
    $scope.MonthsTabId = '';
    $scope.SaveMonthsTab = function () {

        var checkedData = [];
        for (var i = 0; i < $scope.MonthsList.length; i++) {
            if ($scope.MonthsList[i].isSelected == true)
                checkedData.push($scope.MonthsList[i]);
        }


        try {
            if (checkedData.length == 0) {
                throw 'Please select at least one Month';
            }


            $http({
                method: 'POST',
                data: { CropMasterId: $scope.CropMaster.Id, MonthTabData: checkedData, FarmingProcessId: $scope.FarmingProcessTabId },
                url: $scope.path + 'SaveMonthsTab'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.LoadAllSelectedMonthsTab();
                }

            });
        }
        catch (e) {
            ShowResult(e, "failure");
        }


    }

    $scope.DeleteCropProcessMonth = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedCropProcessMonthTab?Id=' + $scope.CropProcessMonthTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.LoadAllSelectedMonthsTab();

            }

        });
    }

    $scope.ConfirmDeleteCropProcessMonthTab = function (Id) {
        $scope.CropProcessMonthTabId = Id;
        angular.element(document.querySelector("#DeleteCropProcessMonthTabPopUp")).modal("show");
    }

    $scope.closeMonthsTabPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    // # end region Crop Process Month
}