'use strict';
GeneralContractItemMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralContractItemMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'General Contract Master';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContractItemMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'

    //  #region Auto Seq
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();
    //  #endregion Auto Seq

    // #region UOM
    $scope.UOMList = [];
    $scope.getUOM = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/MedicineMaster/getUOM',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        })
    }
    $scope.getUOM();

    $scope.doubleClkUOM = function (e) {
        $scope.ModelNew.UOMName = e.data.StandardName;
        $scope.ModelNew.UOMId = e.data.Id;
        $scope.closeUOMPopUp();
    }

    $scope.openUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUpId')).modal('show');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUpId')).modal('hide');
    }

    $scope.searchByUOM = "UserName";
    $scope.searchUM = "";

    $scope.UOMSearchByList = [
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];

    $scope.UOMList = [];
    $scope.searchUOM = function () {
        $http({
            method: 'POST',
            url: 'HumanResource/MedicineMaster/searchUOM',
            data: { column: $scope.searchByUOM, value: $scope.searchUM },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UOMList = response.data;
        });
    }
    // #endregion UOM

    //  #region Get List
    $scope.getData = function () {
        $http.get('Administration/GeneralContractItemMaster/GetList')
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

    //  #endregion Get List

    // #region Double Tap open grid
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    // #endregion Double Tap open grid

    //  #region Save
    //#region List object
    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        UserName: null,
        StandardName: null,
        ShortName: null,
        UOMName: null,
        UOMId: null,
        Category: null,
        SubCategory: null,
        Purpose: null,
        Detail: null,
        Item:null
        
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object
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
    
    //  #endregion Save

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
            UOMName: null,
            UOMId: null,
            Category: null,
            SubCategory: null,
            Purpose: null,
            Detail: null,
            Item: null
        };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //  #endregion Clear

}