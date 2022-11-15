'use strict';
QualificationMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QualificationMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Qualification Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/QualificationMaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'Delete/';

    // #region Sequence
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    // #endregion Sequence

    // #region All Get Function
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
    // #endregion All Get Function

    // #region Objects
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,        
        IsActive: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    // #endregion Objects

    // #region Double Click
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
        }
    };
    // #endregion Double Click

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

    // #region Delete
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
    // #endregion Delete

    // #region Clear
    $scope.Clear = function () {
        ClearFields();
        return true;
    };


    function ClearFields() {
        $scope.Action = 'Save';

        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            Code:null,
            ShortName: null,
            StandardName: null,
            UserName: null,            
            IsActive: true
        };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    // #endregion Clear
}