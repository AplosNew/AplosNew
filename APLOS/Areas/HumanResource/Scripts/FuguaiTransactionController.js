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


    /*
    // ALL POP UPs

     // Observed By
    $scope.OpenEmployeePopUp = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }

    // Entity Master
    $scope.OpenEntityPopUp = function () {

        angular.element(document.querySelector('#EntityPop')).modal('show');
    }

   
    $scope.closeEntityPopUp = function () {
        angular.element(document.querySelector('#EntityPop')).modal('hide');
    }

   // Fuguai Master
    $scope.OpenFuguaiPopUp = function () {

        angular.element(document.querySelector('#FuguaiPop')).modal('show');
    }


    $scope.closeFuguaiPopUp = function () {
        angular.element(document.querySelector('#FuguaiPop')).modal('hide');
    }
    */

    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
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
           // document.getElementById("ObservedBy").value = $scope.ObservedByList;
            
        });
        
    }
    $scope.getObservedBy();

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
           
            url: $scope.path + 'getSubCategory',
        }).then(function success(response) {
            $scope.subcategoryList = response.data;
        });
    }
    $scope.getSubCategory();
   
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

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'datas': $scope.ModelNew,
                    
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

    // Commitment Date
    $scope.getCommitmentDate = function () {
        var today = new Date();
        var dd = String(today.getDate()).padStart(2, '0');
        var mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
        var yyyy = today.getFullYear();

        today = mm + '/' + dd + '/' + yyyy;
        //$scope.ModelNew.CommitmentDate = today.getFullYear() + '-' + (today.getMonth() + 1) + '-' + today.getDate();
        $scope.ModelNew.CommitmentDate = today;
        document.getElementById("CommitmentDate").value = $scope.ModelNew.CommitmentDate;
    }
   // $scope.getCommitmentDate();

    $scope.EvalEscalationDays = function () {
        var commitDate = new Date($scope.CommitmentDate);
        appointment.setDate(commitDate.getDate() + 2);
    }
        
}