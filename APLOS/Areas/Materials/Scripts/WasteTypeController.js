'use strict';
WasteTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function WasteTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Waste Type';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/WasteType/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";

    $scope.searchByList = [
        {
            value: 'Id',
            name: "Id",
        },
        {
            value: 'Code',
            name: "Code",
        },
        {
            value: 'ShortName',
            name: "Short Name",
        },
        {
            value: 'StandardName',
            name: "Standard Name",
        },
        {
            value: 'UserName',
            name: "User Name"
        },
        
        {
            value: 'Remarks',
            name: "Remarks",
        }
       
    ];

    
    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "Get",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            //ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Code: null,
        Sequence: 0,
        ShortName: null,
        UserName: null,
        StandardName: null,
        Active: true,
        Remarks: null

    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    //=======================================SAVE============================================
    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
            $scope.getResponsiblePersonId();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                'data': $scope.ModelNew,
                'responsiblePerson': $scope.EmployeeId,
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
    //=======================================SAVE CLOSE======================================

    //  Delete
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

    // clear Data
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.GetSequence();       
            $scope.ModelTemp = {
                Id: null,
                Code:null,
                Sequence: 0,
                ShortName:null,
                UserName: null,
                StandardName: null,               
                Active: true,
                Remarks: null

            };

        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
}