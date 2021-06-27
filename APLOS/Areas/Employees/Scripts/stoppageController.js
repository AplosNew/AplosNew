'use strict';
stoppageController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function stoppageController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Stoppage';
    $scope.index = -1;
    $scope.path = 'employees/Stoppage/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

    $scope.companyList = [];
    $scope.getCompany = function () {
        $http.get('employees/Stoppage/GetCompany')
            .then(function (response) {
                $scope.companyList = response.data;

            });
    };
    $scope.getCompany();

    $scope.cityList = [];
    $scope.getCityList = function () {
        $http.get('employees/Stoppage/GetCity?CompanyId=' + $scope.stoppageNew.CompanyId)
            .then(function (response) {
                $scope.cityList = response.data;
            });
    };
  

    $scope.ModelList = [];
    $scope.getData = function () {
        $scope.stoppageNew = Object.assign({}, $scope.stoppage);
        $scope.ModelList = [];
        $http.get('employees/Stoppage/getlist')
            .then(function (response) {
                $scope.ModelList = response.data;

            });
    };
    $scope.getData();

    $scope.recorddoubleclick = function (args) {
        try {
             $scope.stoppageNew = Object.assign({}, args.data);
            //var gridObj = $("#GridEdit").data("ejGrid");
            //$scope.stoppageNew = gridObj.getSelectedRecords()[0];
             $scope.getCityList();
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.stoppage = {
        Id: null,
        CompanyGroupId: null,
        CompanyId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        CityId: null,
        Remarks: null,
        Active: true
    };
    $scope.stoppageNew = Object.assign({}, $scope.stoppage);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.stoppageNew.Sequence = data;
        })
    };
    $scope.GetSequence();
    
    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.stoppageNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.stoppageNew ,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields($scope.GetSequence());
                        $scope.getData();
                        $scope.stoppageNew.Active = true;
                        //$scope.companyList = [];
                        $scope.cityList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.stoppageNew.Id)) {
            $http.get('employees/Stoppage/Delete?Id=' + $scope.stoppageNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.stoppageNew = Object.assign({}, $scope.stoppage);
                        ClearFields($scope.GetSequence());
                        $scope.getData();
                        $scope.stoppageNew.Active = true;
                        //$scope.companyList = [];
                        $scope.cityList = [];
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.stoppage = {};
        $scope.stoppageNew = {};
        $scope.stoppageNew.Sequence = seq;
        $scope.stoppageNew.Active = true;
    }

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Code", $scope.stoppageNew.Code);
            CheckField("Short Name", $scope.stoppageNew.ShortName);
            CheckField("Standard Name", $scope.stoppageNew.StandardName);
            CheckField("User Name", $scope.stoppageNew.UserName);
            CheckField("City", $scope.stoppageNew.CityId);

        } catch (ex) {
            throw ex;
        }
    };

}