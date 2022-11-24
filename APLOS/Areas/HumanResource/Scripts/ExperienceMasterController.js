'use strict';
ExperienceMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window', 'baseService', '$routeParams', '$location', '$http', '$controller', '$filter'];
function ExperienceMasterController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $controller, $filter) {
    $rootScope.title = 'Experience Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ExperienceMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);

    $scope.DepartmentList = [];
    $scope.GetDepartment = function () {
        $http.get('HumanResource/ExperienceMaster/GetDepartment')
            .then(
                function successCallback(response) {
                    $scope.DepartmentList = response.data;

                }
            )
    }
    $scope.GetDepartment();

    //  #region SEQUENCE
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModalNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    //  #endregion SEQUENCE 

    // #region  GET MAIN GRID DATA

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();
    // #endregion  GET MAIN GRID DATA

    //  #region FORM OBJECT DECLARATION & INITIALIZATION
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        AreaOfExperience: null,
        DepartmentId: null,
        IsActive: true
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);


    //  #endregion FORM OBJECT DECLARATION & INITIALIZATION

    // #region DOUBLE CLICK ON GRID OPEN FORM
    $scope.Get = function (args) {
        $scope.ModalNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
        }
    };
    // #endregion DOUBLE CLICK ON GRID OPEN FORM CLOSE

    // #region SAVE
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModalNew,
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

   
    // #endregion SAVE CLOSE

    // #region DELETE 
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModalNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModalNew.Id,
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
    // #endregion DELETE 

    // #region CLEAR FORM
    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';


        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            AreaOfExperience: null,
            DepartmentId: null,
            IsActive: true
        };

        $scope.ModalNew = Object.assign({}, $scope.ModelTemp);
    }


    // #endregion CLEAR FORM
    
}