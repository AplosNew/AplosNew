'use strict';
QMSMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QMSMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'QMS Master';
    $scope.QMSMasterList = [];
    $scope.SelectedEntityTabList = [];
  
    $scope.ProcessList = [];
    $scope.WorkCenterList = [];
    $scope.InspectionMasterList = [];
    $scope.InspectionTypeList = [];

    

    $scope.path = 'QMS/QMSMaster/';

    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getListUrlentity = $scope.path + 'getlistentity';

    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlentity = $scope.path + 'createentity';

    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'ShortName', name: "Short Name" }, { value: 'UserName', name: "User Name" }, { value: 'Code', name: "Code" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'QMS/QMSMaster/getprocess/',
    }).then(function successCallback(response) {
        $scope.ProcessList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/QMSMaster/getworkcenter/',
    }).then(function successCallback(response) {
        $scope.WorkCenterList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/QMSMaster/getinspectionmasterlist/',
    }).then(function successCallback(response) {
        $scope.InspectionMasterList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSMaster/getinspectiontype/',
    }).then(function successCallback(response) {
        $scope.InspectionTypeList = response.data;
    });

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QMSMasterList = response.data;
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
        ProcessId: null,
        Location: null,
        WorkCenterId: null,
        InspectionTypeId: null,
        InspectionMasterId: null,
        ResponsiblePersonId: null,
        QualityInchargeId: null,
        QualityHeadId: null,
        QualityUOMId: null,
        QualityBenchmarkParameter: null,
        Remarks: null,
        EmployeeStatus: null,
        EmpIStatus: null,
        EmpInStatus: null,
  
};
    $scope.QMSMaster = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.QMSMaster.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.QMSMaster = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.setTab(1);
        $scope.getEntityData($scope.QMSMaster.Id);

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
            $scope.QMSMasterList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.QMSMaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.QMSMaster = response.data.Data;
                    $scope.getEntityData($scope.QMSMaster.Id);
                    $scope.Action = 'Update';
                    $scope.Getgrid();
                    
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.QMSMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.QMSMaster.Id,
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
        $scope.QMSMaster = Object.assign({}, $scope.ModelTemp);
        $scope.QMSMaster.Sequence = seq;
        $scope.SelectedEntityTabList = [];
        $scope.getEntityData($scope.QMSMaster.Id);
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
            data: { Id: $scope.QMSMaster.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.QMSMaster.ResponsiblePersonId = null;
        $scope.QMSMaster.ResponsiblePerson = null;
        $scope.QMSMaster.EmployeeCode = null;
        $scope.QMSMaster.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.QMSMaster.EmployeeCode = data.Code;
        $scope.QMSMaster.ResponsiblePersonId = data.Id;
        $scope.QMSMaster.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////

    ///////////////////////////////////  Quality Head Pop Up  ////////////////////////////////////////


    // #region Quality head field


    $scope.EmpQualityHeadList = [];
    $scope.QualityHeadPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpQualityHead")).modal("show");
        $scope.getqualityheadDetailsData();

    }
    $scope.getqualityheadDetailsData = function () {
        $scope.EmpQualityHeadList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.QMSMaster.Id },
            url: $scope.path + 'LoadAllQualityHeadDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpQualityHeadList = response.data;
        });
    }

    $scope.QualityHeadClear = function () {
        $scope.QMSMaster.QualityHeadId = null;
        $scope.QMSMaster.QualityHeadName = null;
        $scope.QMSMaster.EmpCode = null;
        $scope.QMSMaster.EmpIStatus = null;
    };
    $scope.closeEmpQualityHeadPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpQualityHeadData = function (obj) {

        var data = obj.data;
        $scope.QMSMaster.EmpCode = data.Code;
        $scope.QMSMaster.QualityHeadId = data.Id;
        $scope.QMSMaster.QualityHeadName = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpQualityHead')).modal('hide');
    };
    // # end region  Quality head

    ///////////////////////////////////   Quality head Pop Up End ////////////////////////////////////////
  

    ///////////////////////////////////  Quality Incharge Pop Up  ////////////////////////////////////////


    // #region Quality Incharge field


    $scope.EmpQualityInchargeList = [];
    $scope.QualityInchargePopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpQualityIncharge")).modal("show");
        $scope.getqualityInchargeDetailsData();

    }
    $scope.getqualityInchargeDetailsData = function () {
        $scope.EmpQualityInchargeList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.QMSMaster.Id },
            url: $scope.path + 'LoadAllQualityInchargeDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpQualityInchargeList = response.data;
        });
    }

    $scope.QualityInchargeClear = function () {
        $scope.QMSMaster.QualityInchargeId = null;
        $scope.QMSMaster.QualityInchargeName = null;
        $scope.QMSMaster.EmppCode = null;
        $scope.QMSMaster.EmpInStatus = null;
    };
    $scope.closeEmpQualityInchargePopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpQualityInchargeData = function (obj) {

        var data = obj.data;
        $scope.QMSMaster.EmppCode = data.Code;
        $scope.QMSMaster.QualityInchargeId = data.Id;
        $scope.QMSMaster.QualityInchargeName = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpQualityIncharge')).modal('hide');
    };
    // # end region  Quality Incharge

    ///////////////////////////////////   Quality Incharge Pop Up End ////////////////////////////////////////


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


    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.PlantList = [];
    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.entitymaster.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };

    $scope.EntityList = [];
    $scope.getEntityWithChange = function () {
        $scope.EntityList = [];
        cboService.getCboEntityByPlant(null, $scope.entitymaster.CompanyId, $scope.entitymaster.PlantId, function (result) {
            $scope.EntityList = result;
        });
    };


    $scope.EntityModelTemp = {
        Id: null,
        QMSMasterId: null,
        EntityId: null,
  
    };
    $scope.entitymaster = Object.assign({}, $scope.EntityModelTemp);

    $scope.SaveEntity = function () {
        $scope.entitymaster.QMSMasterId = $scope.QMSMaster.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.entitymasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlentity,
                data: { 'data': $scope.entitymaster },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.entitymaster = response.data.Data;
                    ClearFieldsEntity();
                    $scope.getEntityData($scope.QMSMaster.Id);
                
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
 

    function ClearFieldsEntity() {
        $scope.Action = 'Save';
        $scope.entitymaster = Object.assign({}, $scope.ModelTemp);
        $scope.getEntityData($scope.QMSMaster.Id);
        
    }

    $scope.getEntityData = function (QMSMasterId) {
        $http({
            method: 'POST',
            url: $scope.path + "GetListEntity?QMSMasterId=" + QMSMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedEntityTabList = response.data; 
        });
    }
  

    $scope.DeleteEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteSelectedEntityTab?Id=' + $scope.EntityTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");       
                $scope.getEntityData($scope.QMSMaster.Id);
                ClearFieldsEntity();
            }

        });
    }

    $scope.ConfirmDeleteEntityTab = function (Id) {
        $scope.EntityTabId = Id;
        angular.element(document.querySelector("#DeleteEntityTabPopUp")).modal("show");
    }

}