//#region Lib
'use strict';
ParameterMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ParameterMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Parameter Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/ParameterMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //#endregion Lib

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    // #region Master
    //  #region Auto Seq
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    //  #endregion Auto Seq

    //  #region All Get
    $scope.getData = function () {
        $http.get('Productions/ParameterMaster/GetList')
            .then(
                function successCallback(response) {
                    $scope.ModelList = response.data;
                    ClearFields(response.data.Sequence);
                    $scope.GetSequence();
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
           
        }
    };

    //  #endregion All Get
    
   // #region Get Employee Budget Code
    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.GetResponsiblePersonBudgetCode();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }
    

    $scope.EmployeeId = null;
    $scope.Employee = null;
    $scope.doubleEmploye = function (e) {
        $scope.ModelNew.EmpSystemId = e.data.SystemId;
        $scope.ModelNew.EmployeeName = e.data.EmployeeName;
        $scope.ModelNew.EmployeeCode = e.data.EmployeeCode;
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        /*$scope.viewFurniturePolicyGrids();*/
    }

    $scope.ResponsiblePersonList = [];
    $scope.GetResponsiblePersonBudgetCode = function () {
        $http.get('Productions/ParameterMaster/GetResponsiblePersonBudgetCode').then(           
                function successCallback(response) {                    
                        $scope.ResponsiblePersonList = response.data;                  
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    }
    // #endregion Get Employee Budget Code

    //#region List object
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code:null,
        UserName: null,
        StandardName: null,
        ShortName:null,
        IsActive: true,
        Remarks: null,
        EmployeeName: null,
        EmployeeCode: null,
        EmpSystemId:null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    // #region Save
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
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
    // #endregion Save

    //  #region Delete
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
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    //  #endregion Delete

    //  #region Clear
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            Code: null,
            UserName: null,
            StandardName: null,
            ShortName: null,
            IsActive: true,
            Remarks: null,
            EmployeeName: null,
            EmployeeCode: null,
            EmpSystemId: null
        };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //  #endregion Clear

    // #endregion Master

    // #region Child 1
    
    // #endregion Child 1
    
}