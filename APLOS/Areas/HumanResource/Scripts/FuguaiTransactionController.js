'use strict';
FuguaiTransactionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function FuguaiTransactionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Fuguai Transaction';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/FuguaiTransaction/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';  
    baseService.init($scope.getListUrl);

    //----------------------------------------------------------------------------------------//
     // ALL POP UPs
    $scope.OpenEmployeePopUp = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }


    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    $scope.openObservedBy = function () {

        angular.element(document.querySelector('#ObservedByPopup')).modal('show');
        $scope.getObservedBy();
    }
    $scope.closeObservedBy = function () {

        angular.element(document.querySelector('#ObservedByPopup')).modal('hide');
    }
     //------------------------------------------------------------------------------------//
   

    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

   
    // All List

    $scope.EntityList = [];
    $scope.CategoryList = [];
    $scope.DepartmentList = [];
    $scope.MachineList = [];
    $scope.ObservedByList = [];
    $scope.TagList = [];
    $scope.PersonList = [];
    $scope.ProcessList = [];
    $scope.MachineRefList = [];
    $scope.ResponsiblePersonList = [];
    $scope.ProcessList = [];
    $scope.subcategoryList = [];

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        Time:null,
        EntityId: null,
        ObservedById: null,
        ZoneMasterId: null,
        ZoneCategory: null,               
        Detail: null,
        PriorityLevel: null,
        ResponsibleDepartmentId: null,
        ResponsiblePersonId: null,
        TargetDate: null,
        CommitmentDate: null,
        StoryPoint: null,
        Remarks: null,
        CurrentStatus: null,
        ProcessId: null,
        PersonId: null,
        IsMachineApplicable: null,
        MachineMasterId: null,
        MachineRef: null,
        FinalStatus: null,
        CloseDate: null,
        TagColor: null,
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    // ALL GET FUNCTIONS
    
    $scope.getEntity = function () {
        $http({
            method: 'POST',          
            url: $scope.path + 'getEntity',
        }).then(function success(response) {
            $scope.EntityList = response.data;
            
        });
        
    }

    $scope.getEntity();

    $scope.ObservedBy = null;
    $scope.getObservedBy = function () {
        $http({
            method: 'POST',
           
            url: $scope.path + 'getObservedBy',
        }).then(function success(response) {
            
            $scope.ObservedByList = response.data;
        });
        
    }
    //$scope.getObservedBy();

    $scope.getCategory = function () {
        $http({
            method: 'POST',
           
            url: $scope.path + 'getCategory',
        }).then(function success(response) {
            $scope.CategoryList = response.data;
        });
    }
   $scope.getCategory();

    $scope.getTag = function () {
        $http({
            method: 'POST',
            data: {
                'categoryText': $scope.ModelNew.ZoneCategory,
            },
            url: $scope.path + 'getTag',
        }).then(function success(response) {
            $scope.TagList = response.data;
        });
    }
    // $scope.getTag();

    $scope.getSubCategory = function () {
        $http({
            method: 'POST',
            data: {
                'categoryText': $scope.ModelNew.ZoneCategory,
                'FuguaiId': $scope.ModelNew.ZoneMasterId,
            },
            url: $scope.path + 'getSubCategory',
        }).then(function success(response) {
            $scope.subcategoryList = response.data;
        });
    }
    //$scope.getSubCategory();
   
    $scope.getDepartment = function () {
        $http({
            method: 'POST',
           
            url: $scope.path + 'getDepartment',
        }).then(function success(response) {
            $scope.DepartmentList = response.data;
        });
    }
    $scope.getDepartment();

    $scope.geResponsiblePerson = function () {
        $http({
            method: 'POST',
            data: {
                'DepartmentId': $scope.ModelNew.ResponsibleDepartmentId,
            },
            url: $scope.path + 'getResponsiblePerson',
        }).then(function success(response) {
            $scope.ResponsiblePersonList = response.data;
        });
    }

    $scope.getProcess = function () {
        $http({
            method: 'POST',
            data: {
                'EntityId': $scope.ModelNew.EntityId,
            },
            url: $scope.path + 'getProcess',
        }).then(function success(response) {
            $scope.ProcessList = response.data;
        });
    }

    $scope.getMachine = function () {
        $http({
            method: 'POST',
            data: {
                'processId': $scope.ModelNew.ProcessId,
            },
            url: $scope.path + 'getMachine',
        }).then(function success(response) {
            $scope.MachineList = response.data;
        });
    }

    $scope.getMachineRef = function () {
        $http({
            method: 'POST',
            data: {
                'mmId': $scope.ModelNew.MachineMasterId,
            },
            url: $scope.path + 'getMachineRef',
        }).then(function success(response) {
            $scope.MachineRefList = response.data;
        });
    }

    $scope.getResponsiblePerson = function () {
        $http({
            method: 'POST',
            
            url: $scope.path + 'getResponsiblePerson',
        }).then(function success(response) {
            $scope.ResponsiblePersonList = response.data;
        });
    }

    // Select Observe  By
    
    $scope.ObserveByName = null;
    $scope.ObserveById = null;
    $scope.selectObservedBy = function (e) {
        $scope.ObserveByName = e.data.UserName;
        $scope.ObserveById = e.data.Id;
        $scope.closeObservedBy();
        
    }

    $scope.ResponsiblePerson = null;
    $scope.ResponsiblePersonId = null;
    $scope.selectResponsible = function (e) {
        $scope.ResponsiblePerson = e.data.EmployeeName;
        $scope.ResponsiblePersonId = e.data.SystemId;
        $scope.closeEmpPopUp();
    }
    // Select Observe  By End
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'datas': $scope.ModelNew,
                    'ObservedById': $scope.ObserveById,
                    'ResponsiblePerson':$scope.ResponsiblePersonId,
                },
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
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

       
    };
   
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };

    // clear Data
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        Time: null,
        EntityId: null,
        ObservedById: null,
        ZoneMasterId: null,
        Tag: null,
        Detail: null,
        PriorityLevel: null,
        ResponsibleDepartmentId: null,
        ResponsiblePersonId: null,
        TargetDate: null,
        CommitmentDate: null,
        StoryPoint: 2.00,
        Remarks: null,
        CurrentStatus: null,
        ProcessId: null,
        PersonId: null,
        IsMachineApplicable: null,
        MachineMasterId: null,
        MachineNo: null,
        FinalStatus: null,
        CloseDate: null,
        TagColor: null,

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

   
        
}