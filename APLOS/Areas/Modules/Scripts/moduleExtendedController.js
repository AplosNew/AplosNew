'use strict';
ModuleExtendedController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ModuleExtendedController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Module Extended";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Modules/moduleextended/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'save';
    $scope.moduleextended = {
        Id: null,
        CompanyGroupId: null,
        APIKeyWithValue: null,
        SMSEndPoint: null,
        Remarks: null,
        SenderId: null,
        AddedBy: null,
        AddedDate: new Date(),
        UpdatedDate: new Date()
    };

    $scope.companyGroupList = [];
    cboService.getCboCompanyGroup(function (data) {
        $scope.companyGroupList = data;
    });

    $scope.companyList = [];
    $scope.getCboCompanyByCompanyGroup = function (companyGroupId) {
        $scope.companyList = [];
        $scope.moduleList = [];
        $scope.moduleExtends = [];
        cboService.getCboCompanyByCompanyGroup(companyGroupId, function (data) {
            $scope.companyList = data;
        });
    };

    $scope.moduleExtends = [];
    $scope.moduleList = [];

    //$scope.LoadModule = function () {
    //    $scope.moduleList = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Modules/CompanyGroupModule/GetModuleListByCompanyGroup?companyGroupId=' + $scope.moduleextended.CompanyGroupId
    //    }).then(function successCallback(response) {
    //        $scope.moduleList = response.data;
    //    });
    //};

    //$scope.GetList = function () {
    //    $scope.moduleExtends = [];
    //    $http({
    //        method: 'GET',
    //        url: 'Modules/ModuleExtended/GetList?companyGroupId='+ $scope.moduleextended.CompanyGroupId 
    //    }).then(function successCallback(response) {
    //        $scope.moduleExtends = response.data;
    //    });
    //};

    $scope.searchCol = "ME.Id";
    $scope.searchVal = "";
    $scope.getData = function () {
        $scope.moduleExtends = [];
        $http({
            method: 'GET',
            data: { 'parameters': null },
            url: $scope.path + "GetList?column=" + $scope.searchCol + "&value=" + $scope.searchVal
        }).then(function successCallback(response) {
            $scope.moduleExtends = response.data;
        });
    };
    $scope.getData();



    $scope.Get = function (args) {
        $scope.moduleextended = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };


    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '' || fieldValue === undefined) {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField($scope.moduleextended.CompanyGroupId, "Company Group");
            CheckField($scope.moduleextended.SMSEndPoint, "SMS End Point");
            CheckField($scope.moduleextended.APIKeyWithValue, "API Key With Value");
            CheckField($scope.moduleextended.SenderId, "Sender Id");

        } catch (e) {
            throw e;
        }
    }


    $scope.Save = function () {
        try {
            if ($scope.Action == 'Save' || $scope.Action == 'Update') {
                ValidationMaster();
                $http({
                    method: 'POST',
                    data: { moduleextended: $scope.moduleextended },
                    url: $scope.saveUrl,
                }).then(function successCallback(response) {
                    if (response.data.Error == false) {
                        ShowResult(response.data.Message, 'success');

                        $scope.Clear();
                        $scope.getData();
                        $scope.Action = 'Save';
                    }
                    else {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {
            $http({
                method: 'GET',
                url: "Modules/ModuleExtended/Delete?SystemId=" + $scope.moduleextended.Id
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                    $scope.Action = 'Save';
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        $scope.moduleextended = {
            Id: null,
            CompanyGroupId: null,
            APIKeyWithValue: null,
            SMSEndPoint: null,
            Remarks: null,
            SenderId: null,
            AddedBy: null,
            AddedDate: new Date(),
            UpdatedDate: new Date()
        };
    }
}