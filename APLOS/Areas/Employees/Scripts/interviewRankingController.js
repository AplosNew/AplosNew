'use strict';
interviewRankingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function interviewRankingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $rootScope.title = "InterviewRanking";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.interviewRankings = [];
    $scope.path = 'employees/interviewranking/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Sequence', null);

    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.interviewRankings = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.interviewRanking = {
        Id: null,
        CompanyGroupId: $window.companyGroupId,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null
    };
    $scope.interviewRankingNew = Object.assign({}, $scope.interviewRanking);

    // #region GetSequence
    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.interviewRankingNew.Sequence = response.data;
            });
    }
    $scope.GetSequence();
    // #endregion

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.interviewRanking = $scope.interviewRankings[$scope.index];
        $scope.interviewRankingNew = Object.assign({}, $scope.interviewRanking);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            $scope.interviewRankingNew.CompanyGroupId = $window.companyGroupId;
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.interviewRankingForm.$valid) {
                angular.copy($scope.interviewRankingNew, $scope.interviewRanking);
                if ($scope.Action == "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.interviewRankingNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.getData();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
                else if ($scope.Action == "Update") {

                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.interviewRankingNew,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");

                            $scope.getData();
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.interviewRankingNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.interviewRankingNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.interviewRankings.splice($scope.index, 1);
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    }

    // #region Clear
    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.interviewRanking = {};
        $scope.interviewRankingNew = {};
        $scope.interviewRankingNew.Sequence = seq;
    }
    // #endregion
}