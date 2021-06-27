'use strict';
QMSInspectionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QMSInspectionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'QMS Inspection';
    $scope.QMSInspectionList = [];
    $scope.InspectionChildTabList = [];
    
    $scope.InspectionTypeList = [];
    $scope.ProcessList = [];
    $scope.InspectionLevelList = [];
    $scope.InspectionMasterList = [];
    $scope.ProductionReferenceList = [];
    $scope.LocationList = [];
    $scope.StatusList = [];
    $scope.QMSDefectMasterList = [];
    $scope.QMSDefectZoneList = [];
    $scope.SkillList = [];
  
    $scope.path = 'QMS/QMSInspection/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';

    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "Code"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Process', name: "Process" }, { value: 'InspectionType', name: "Inspection Type" }, { value: 'Code', name: "Code" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getprocess/',
    }).then(function successCallback(response) {
        $scope.ProcessList = response.data;
    });


    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getinspectionmasterlist/',
    }).then(function successCallback(response) {
        $scope.InspectionMasterList = response.data;
        });

    $scope.GetInspectionLevel = function () {
        $scope.InspectionLevelList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getinspectionlevel?InspectionMasterId=' + $scope.QMSInspection.InspectionMasterId
        }).then(function successCallback(response) {
            $scope.InspectionLevelList = response.data;
        });
    }
 

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getproductionreference/',
    }).then(function successCallback(response) {
        $scope.ProductionReferenceList = response.data;
        });


    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getinspectiontype/',
    }).then(function successCallback(response) {
        $scope.InspectionTypeList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getlocationlist/',
    }).then(function successCallback(response) {
        $scope.LocationList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getstatuslist/',
    }).then(function successCallback(response) {
        $scope.StatusList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getdefectmasterlist/',
    }).then(function successCallback(response) {
        $scope.QMSDefectMasterList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getdefectzonelist/',
    }).then(function successCallback(response) {
        $scope.QMSDefectZoneList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSInspection/getskilllist/',
    }).then(function successCallback(response) {
        $scope.SkillList = response.data;
    });

    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QMSInspectionList = response.data;
            ClearFields(response.data);
         
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Date: new Date(),
        InspectionMasterId: null,
        InspectionTypeId: null,
        InspectionLevelId: null,
        EmployeeId: null,
        ProcessId: null,
        LocationId: null,
        ResponsiblePersonId: null,
        ProductionReferenceId: null,
        BatchReferenceNo: null,
        BatchSize: null,
        SampleSize: null,
        NoOfDefectiveUnit: null,
        StatusId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmpIStatus: null,
        Customer: null,

};
    $scope.QMSInspection = Object.assign({}, $scope.ModelTemp);

  

    $scope.enable = true;
    $scope.Get = function (args) {

        $scope.QMSInspection = Object.assign({}, args.data);
        $scope.GetInspectionLevel();
        $scope.getInspectionChildData($scope.QMSInspection.Id);
        $scope.enable = false;
        $scope.Action = 'Update';
     
        $scope.setTab(1);
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
            $scope.QMSInspectionList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.QMSInspection, 'InspectionChildData': $scope.InspectionChild },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.QMSInspection = response.data.Data;     
                    $scope.Getgrid();
                    $scope.getInspectionChildData($scope.QMSInspection.Id);
                    $scope.Action = 'Update';
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.QMSInspection.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.QMSInspection.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data);
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
        $scope.QMSInspection = Object.assign({}, $scope.ModelTemp);
        $scope.getInspectionChildData($scope.QMSInspection.Id);
        $scope.enable = true;
        $scope.setTab();
      
    }

    ///////////////////////////////////  Responsible Person Pop Up  ////////////////////////////////////////


    // #region ResPerson field

  
    $scope.EmpResPersonList = [];
    $scope.ResponsiblePersonPopUp = function () {
        angular.element(document.querySelector("#EmployeePopUpResPerson")).modal("show");
        $scope.getResPersonDetailsData();

    }
    $scope.getResPersonDetailsData = function () {
        $scope.EmpResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.QMSInspection.Id },
            url: $scope.path + 'LoadAllResPersonDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.QMSInspection.ResponsiblePersonId = null;
        $scope.QMSInspection.ResponsiblePerson = null;
        $scope.QMSInspection.EmployeeCode = null;
        $scope.QMSInspection.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.QMSInspection.EmployeeCode = data.Code;
        $scope.QMSInspection.ResponsiblePersonId = data.Id;
        $scope.QMSInspection.ResponsiblePerson = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePopUpResPerson')).modal('hide');
    };
    // # end region ResPerson

    ///////////////////////////////////  Responsible Person Pop Up End ////////////////////////////////////////

    ///////////////////////////////////  Employee Pop Up  ////////////////////////////////////////


    // #region Employee field


    $scope.EmpList = [];
    $scope.EmpPopUp = function () {
        angular.element(document.querySelector("#EmployeePop")).modal("show");
        $scope.getEmpDetailsData();

    }
    $scope.getEmpDetailsData = function () {
        $scope.EmpList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.QMSInspection.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;
        });
    }

    $scope.EmpClear = function () {
        $scope.QMSInspection.EmployeeId = null;
        $scope.QMSInspection.EmpName = null;
        $scope.QMSInspection.EmpCode = null;
        $scope.QMSInspection.EmpIStatus = null;
    };
    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmployeeData = function (obj) {

        var data = obj.data;
        $scope.QMSInspection.EmpCode = data.Code;
        $scope.QMSInspection.EmployeeId = data.Id;
        $scope.QMSInspection.EmpName = data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    };
    // # end region  Employee

    ///////////////////////////////////  Employee Pop Up End ////////////////////////////////////////


    /////*********************Tabs*******************************
    // #region Tab
    $scope.tab = 1;
  
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    $scope.InspectionChildModelTemp = {
        Id: null,
        QMSInspectionId: null,
        QMSDefectMasterId: null,
        QMSDefectZoneId: null,
        MajorMinor: null,
        NoOfDefect: null,
        SkillId: null,
        DefectResponsiblePersonId: null,
        EmpICStatus: null,
    };
    $scope.InspectionChild = Object.assign({}, $scope.InspectionChildModelTemp);


    function ClearFieldsInspectionChild() {
             $scope.InspectionChild = Object.assign({}, $scope.InspectionChildModelTemp);
    }

    $scope.getInspectionChildData = function (QMSInspectionId) {
        
        $http({
            method: 'GET',
            url: $scope.path + 'GetListInspectionChild?QMSInspectionId=' + QMSInspectionId
        }).then(function successCallback(response) {
            $scope.InspectionChildTabList = response.data;
            ClearFieldsInspectionChild();
        });
    }


    $scope.DeleteInspectionChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteInspectionChild?Id=' + $scope.InspectionChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getInspectionChildData($scope.QMSInspection.Id);
                ClearFieldsInspectionChild();
            }

        });
    }

    $scope.ConfirmDeleteInspectionChildTab = function (Id) {
        $scope.InspectionChildTabId = Id;
        angular.element(document.querySelector("#DeleteInspectionChildTabPopUp")).modal("show");
    }

    // #region Defect Responsible person


    $scope.DefectResPersonList = [];
    $scope.DefectResPersonPopUp = function () {
        angular.element(document.querySelector("#DefectResPersonPop")).modal("show");
        $scope.getDefectResPonDetailsData();

    }
    $scope.getDefectResPonDetailsData = function () {
        $scope.DefectResPersonList = [];

        $http({
            method: 'POST',
            data: { Id: $scope.QMSInspection.Id },
            url: $scope.path + 'LoadAllDefResPonDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.DefectResPersonList = response.data;
        });
    }

    $scope.DefectResPersonClear = function () {
        $scope.InspectionChild.DefectResponsiblePersonId = null;
        $scope.InspectionChild.DefResPonName = null;
        $scope.InspectionChild.DefResPonCode = null;
        $scope.InspectionChild.EmpICStatus = null;
    };
    $scope.closeDefectResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setDefectResPersonData = function (obj) {

        var data = obj.data;
        $scope.InspectionChild.DefResPonCode = data.Code;
        $scope.InspectionChild.DefectResponsiblePersonId = data.Id;
        $scope.InspectionChild.DefResPonName = data.EmployeeName;
        angular.element(document.querySelector('#DefectResPersonPop')).modal('hide');
    };
    // # end region   Defect Responsible person

    ///////////////////////////////////   Defect Responsible person Pop Up End ////////////////////////////////////////

}