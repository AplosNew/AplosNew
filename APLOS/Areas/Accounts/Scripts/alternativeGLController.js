'use strict';
function AlternativeGLController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Alternative GL";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.alternativeGLs = [];
    $scope.path = 'accounts/alternativegl/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'AccountCode', 'AccountCode');
    $scope.getData = function (pageno, alternativeCoaId) {
        $rootScope.parameters.AlternativeCOAId = alternativeCoaId;
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.alternativeGLs = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };
    $scope.getData();

    $rootScope.searchByAlternativeGLList = [
       {
           'name': 'Account Code',
           'value': 'AccountCode'
       },
       {
           'name': 'User Name',
           'value': 'UserName'
       },
       {
           'name': 'Description',
           'value': 'Description'
       }

    ];
    $scope.alternativeGL = {
        Id: null,
        AlternativeCOAId: null,
        AccountCode: null,
        Sequence: 0,
        UserName: null,
        Description: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
   
    $scope.onAlternativeCOAChangeSequence = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/alternativegl/getautosequence?acoaid=' + item
        }).then(function successCallback(response) {
            $scope.alternativeGL.Sequence = response.data;
        });
    };

    $scope.Get = function (id, index) {
        $scope.disableField = true;
        $scope.index = index;
        $scope.alternativeGL = $scope.alternativeGLs[$scope.index];
        $scope.alternativeGL.AddedDate = $filter('dateFilter')($scope.alternativeGL.AddedDate);
        $scope.alternativeGL.UpdatedDate = $filter('dateFilter')($scope.alternativeGL.UpdatedDate);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

   

    $scope.onCOAChange = function (item) {
        $scope.getData(null, item);
    };

    $scope.alternativeCOAList = [];
    $http({
        method: 'GET',
        url: 'accounts/alternativecoa/getalternativecoalistcbo/'
    }).then(function successCallback(response) {
        $scope.alternativeCOAList = response.data;
    });

    $scope.getLength = function (id) {

        $http({
            method: 'GET',
            url: 'accounts/alternativecoa/getlengthofglcbo?id=' + id
        }).then(function successCallback(response) {
            $scope.maxLength = response.data[0]['Text'];
        });
    };
    /*============For Check MaxLenth=============*/
    $scope.maxLengthCheck = function (object) {
        $scope.$watch('alternativeGL.AccountCode', function (newValue) {
            if (newValue.length > object.maxLength)
                object.value = object.value.slice(0, object.maxLength);
        });
    };

    $scope.isNumeric = function (evt) {
        var theEvent = evt || window.event;
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
        var regex = /[0-9]|\./;
        if (!regex.test(key)) {
            theEvent.returnValue = false;
            if (theEvent.preventDefault) theEvent.preventDefault();
        }
    };

    /*============End For Check MaxLenth=============*/

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.alternativeGLForm.$valid) {
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.alternativeGL,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.alternativeGLs.push(response.data.AlternativeGL);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.alternativeGL,
                    dataType: "JSON"
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -4) {
                            $scope.alternativeGLs[$scope.index] = $scope.alternativeGL;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.alternativeGL.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.alternativeGL.Id,
                dataType: "JSON"
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.alternativeGLs.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
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
        $scope.Action = "Save";
        $scope.alternativeGL = { AlternativeCOAId: $scope.alternativeGL.AlternativeCOAId };
        $scope.alternativeGL.Sequence = seq;
        $scope.alternativeGL.Active = true;
    }
}
AlternativeGLController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
