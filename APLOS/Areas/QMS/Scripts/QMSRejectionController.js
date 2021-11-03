'use strict';
QMSRejectionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QMSRejectionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'QMS Rejection';
    $scope.QMSRejectionList = [];
    $scope.RejectionChildTabList = [];
    
    $scope.ShiftMasterList = [];
    $scope.ProcessList = [];
    $scope.ProductionReferenceList = [];
    $scope.LocationList = [];
    $scope.StockKeepingUnitList = [];
    $scope.QMSDefectMasterList = [];
    $scope.GradeMasterList = [];
  
    $scope.path = 'QMS/QMSRejection/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlrejectionchild = $scope.path + 'CreateInspectionChild';

    $scope.deleteUrl = $scope.path + 'delete/';
  
  

    baseService.init($scope.getListUrl);


    $scope.searchBy = "Code"; $scope.search = "";
   

    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Process', name: "Process" }, { value: 'InspectionType', name: "Inspection Type" }, { value: 'Code', name: "Code" }];
 

    // #region ddl

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getprocess/',
    }).then(function successCallback(response) {
        $scope.ProcessList = response.data;
    });
 

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getproductionreference/',
    }).then(function successCallback(response) {
        $scope.ProductionReferenceList = response.data;
        });


    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getshiftmaster/',
    }).then(function successCallback(response) {
        $scope.ShiftMasterList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getlocationlist/',
    }).then(function successCallback(response) {
        $scope.LocationList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getskulist/',
    }).then(function successCallback(response) {
        $scope.StockKeepingUnitList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getdefectmasterlist/',
    }).then(function successCallback(response) {
        $scope.QMSDefectMasterList = response.data;
        });

    $http({
        method: 'GET',
        url: 'QMS/QMSRejection/getgradelist/',
    }).then(function successCallback(response) {
        $scope.GradeMasterList = response.data;
    });

    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QMSRejectionList = response.data;
            ClearFields(response.data);
         
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Date: new Date(),
        ShiftMasterId: null,
        EmployeeId: null,
        ProcessId: null,
        LocationId: null,
        ResponsiblePersonId: null,
        ProductionReferenceId: null,
        Remarks: null,
        EmployeeStatus: null,
        EmpIStatus: null,
        Customer: null,

};
    $scope.QMSRejection = Object.assign({}, $scope.ModelTemp);


    $scope.enable = true;

    $scope.Get = function (args) {

        $scope.QMSRejection = Object.assign({}, args.data);
        $scope.getRejectionChildData($scope.QMSRejection.Id);
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
            $scope.QMSRejectionList = response.data;
         
        });
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.General.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.QMSRejection},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.QMSRejection = response.data.Data;     
                    $scope.Getgrid();
                    $scope.getRejectionChildData($scope.QMSRejection.Id);
                    $scope.Action = 'Update';
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.QMSRejection.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.QMSRejection.Id,
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
        $scope.QMSRejection = Object.assign({}, $scope.ModelTemp);
        $scope.getRejectionChildData($scope.QMSRejection.Id);
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
            data: { Id: $scope.QMSRejection.Id },
            url: $scope.path + 'LoadAllResPersonDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpResPersonList = response.data;
        });
    }

    $scope.ResponsiblePersonClear = function () {
        $scope.QMSRejection.ResponsiblePersonId = null;
        $scope.QMSRejection.ResponsiblePerson = null;
        $scope.QMSRejection.EmployeeCode = null;
        $scope.QMSRejection.EmployeeStatus = null;
    };
    $scope.closeEmpResPersonPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmpData = function (obj) {

        var data = obj.data;
        $scope.QMSRejection.EmployeeCode = data.Code;
        $scope.QMSRejection.ResponsiblePersonId = data.Id;
        $scope.QMSRejection.ResponsiblePerson = data.EmployeeName;
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
            data: { Id: $scope.QMSRejection.Id },
            url: $scope.path + 'LoadAllEmpDetailsForSelection'
        }).then(function successCallback(response) {
            $scope.EmpList = response.data;
        });
    }

    $scope.EmpClear = function () {
        $scope.QMSRejection.EmployeeId = null;
        $scope.QMSRejection.EmpName = null;
        $scope.QMSRejection.EmpCode = null;
        $scope.QMSRejection.EmpIStatus = null;
    };
    $scope.closeEmpPopUp = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");

    }
    $scope.setEmployeeData = function (obj) {

        var data = obj.data;
        $scope.QMSRejection.EmpCode = data.Code;
        $scope.QMSRejection.EmployeeId = data.Id;
        $scope.QMSRejection.EmpName = data.EmployeeName;
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

    $scope.RejectionChildModelTemp = {
        Id: null,
        QMSRejectionMasterId: null,
        StockKeepingUnitId: null,
        QMSDefectMasterId: null,
        GradeMasterId: null,
        NoOfPics: null,
        RepairablePics: null,
    };
    $scope.RejectionChild = Object.assign({}, $scope.RejectionChildModelTemp);

    $scope.SaveRejectionChild = function () {
        $scope.RejectionChild.QMSRejectionMasterId = $scope.QMSRejection.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.RejectionChildForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlrejectionchild,
                data: { 'data': $scope.RejectionChild },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.RejectionChild = response.data.Data;
              
                    $scope.getRejectionChildData($scope.QMSRejection.Id);

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };


    function ClearFieldsRejectionChild() {
        $scope.RejectionChild = Object.assign({}, $scope.RejectionChildModelTemp);
    }

    $scope.getRejectionChildData = function (QMSRejectionId) {
        
        $http({
            method: 'GET',
            url: $scope.path + 'GetListRejectionChild?QMSRejectionId=' + QMSRejectionId
        }).then(function successCallback(response) {
            $scope.RejectionChildTabList = response.data;
            ClearFieldsRejectionChild();
        });
    }


    $scope.DeleteRejectionChild = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'DeleteRejectionChild?Id=' + $scope.RejectionChildTabId
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
                $scope.getRejectionChildData($scope.QMSRejection.Id);
                ClearFieldsRejectionChild();
            }

        });
    }

    $scope.ConfirmDeleteRejectionChildTab = function (Id) {
        $scope.RejectionChildTabId = Id;
        angular.element(document.querySelector("#DeleteRejectionChildTabPopUp")).modal("show");
    }


}