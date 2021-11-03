'use strict';
incrementGroupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function incrementGroupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Increment Group';
    $scope.Action = 'Save';
    $scope.path = 'Payrolls/IncrementType/';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.IncrementTypeModel = {
        Id: null,
        Name: null,
        Sequence: 0,
        ShortName: null,
        UserName: null,
        Description: null,
        Code: null,
        StandardName: null,
        Remarks: null,
        Active: false
    };
    
    $scope.Save = function () {
        try {
            ValidationMaster();

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'master': $scope.IncrementTypeModel},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDepartmenthkp();
                    $scope.IncrementTypeModel = {};                  
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    
    $scope.Clear = function (obj) {
        ClearFields($scope.GetSequence());
       
    };
    function ClearFields(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
        $scope.Action = 'Save';
        $scope.IncrementTypeModel = {};
    }

    $scope.IncrementTypeList = [];
    $scope.GetDepartmenthkp = function () {
        try {
            $http({
                method: 'GET',
                url: 'Payrolls/IncrementType/GetIncrementTypeInformation'
            }).then(function successCallback(response) {
                $scope.IncrementTypeList = response.data.IncrementTypeInfo;
            });

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetDepartmenthkp ();


    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }

        } catch (ex) {
            throw ex;
        }
    }
    
    function ValidationMaster() {
        try { 
            CheckField("Sequence", $scope.IncrementTypeModel.Sequence);
            CheckField("ShortName", $scope.IncrementTypeModel.ShortName);
            CheckField("UserName", $scope.IncrementTypeModel.UserName);
            
            CheckField("Code", $scope.IncrementTypeModel.Code);
            CheckField("StandardName", $scope.IncrementTypeModel.StandardName);
           
        } catch (ex) {
            throw ex;
        }
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.IncrementTypeModel.Sequence = response.data[0].Sequence;
            });
    };
    $scope.GetSequence();

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridDesignation").data("ejGrid");
        $scope.IncrementTypeModel = gridObj.getSelectedRecords()[0];
        try {
            $scope.Action = 'Update';
           
        } catch (e) {

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.IncrementTypeModel.Id)) {

            $http.get('Payrolls/IncrementType/Delete?Id=' + $scope.IncrementTypeModel.Id)

                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');                      
                        $scope.IncrementTypeModel = {};
                        $scope.GetDepartmenthkp();
                        $scope.GetSequence();
                        ClearFields();                       
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };
    
};