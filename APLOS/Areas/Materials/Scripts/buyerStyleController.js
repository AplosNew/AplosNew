'use strict';
function BuyerStyleController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "BuyerStyle";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.buyerStyles = [];
    $scope.showTbl = false;
    $scope.path = 'Materials/buyerstyle/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    
    $scope.buyerStyle = {
        Id: null,
        BuyerId: null,
        CompanyGroupId: null,
        OurStyleId: null,
        OurStyleName: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.buyerStyleNew = Object.assign({}, $scope.buyerStyle);
    $scope.getBuyerStyleList = function (buyerId) {
        baseService.init('Materials/buyerstyle/getlist?buyerId=' + buyerId, null, null, null, "Sequence", "UserName");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    if (result.Rows.length > 0) {
                        $scope.buyerStyles = result.Rows;
                        $scope.showTbl = true;
                    }
                    else
                        $scope.showTbl = false;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    }
    $rootScope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Our Style',
            'value': 'OurStyleName'
        }
    ];

    $scope.buyerList = [];
    $http({
        method: 'GET',
        url: 'Parties/buyer/getcbo'
    }).then(function (response) {
        $scope.buyerList = response.data;
    });
    $scope.ourStyleList = [];
    $http({
        method: 'GET',
        url: 'Materials/ourstyle/getcbo'
    }).then(function (response) {
        $scope.ourStyleList = response.data;
    });
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.buyerStyleNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.buyerStyle = $scope.buyerStyles[$scope.index];
        $scope.buyerStyleNew = $scope.buyerStyle;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.buyerStyleForm.$valid) {
            $scope.ourStyleId = document.getElementById("ourStyleId").options[document.getElementById('ourStyleId').selectedIndex].text
            //angular.copy($scope.buyerStyleNew, $scope.buyerStyle);
            $scope.buyerStyle = Object.assign({}, $scope.buyerStyleNew);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                        $scope.buyerStyle = response.data.BuyerStyle;
                        $scope.buyerStyle.OurStyleName = $scope.ourStyleId;
                        $scope.buyerStyles.push($scope.buyerStyle);
                        $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.buyerStyle,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {

                            $scope.buyerStyle.OurStyleName = $scope.ourStyleId;
                            $scope.buyerStyles[$scope.index] = $scope.buyerStyle;
                            $scope.buyerStyles = $filter('orderBy')($scope.buyerStyles, 'Sequence');
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.buyerStyleNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.buyerStyleNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.buyerStyles.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.buyerStyle = {};
        $scope.buyerStyleNew = { BuyerId: $scope.buyerStyleNew.BuyerId };
        $scope.buyerStyleNew.Sequence = seq;
        $scope.buyerStyleNew.Active = true;
    }
};
BuyerStyleController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
