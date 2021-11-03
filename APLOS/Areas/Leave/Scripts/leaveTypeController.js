'use strict';
leaveTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function leaveTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Leave Type';
    $scope.path = 'Leave/LeaveType/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.LeaveType = {
        Id: null,
        CompanyGroupId: null,
        LeaveType: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        CityId: null,
        Remarks: null,
        IsESIC: false,
        IsGeneral: false,
    };
    $scope.LeaveTypeModel = Object.assign({}, $scope.LeaveType);

    $scope.ModelList = [];
    $scope.getData = function () {       
        $scope.ModelList = [];
        $http.get('Leave/LeaveType/getlist')
            .then(function (response) {
                $scope.ModelList = response.data;
               // console.log($scope.ModelList);
            });
    };
    $scope.getData();


    $scope.recorddoubleclick = function (args) {
        try {
            $scope.LeaveTypeModel = Object.assign({}, args.data);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
        $scope.getData();
    };


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.LeaveTypeModel.Sequence = data;
        })
    };
    $scope.GetSequence();

    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'LeaveType': $scope.LeaveTypeModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields($scope.GetSequence());
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.LeaveTypeModel.Id)) {
            $http.get('Leave/LeaveType/Delete?Id=' + $scope.LeaveTypeModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.LeaveTypeModel = Object.assign({}, $scope.stoppage);
                        ClearFields($scope.GetSequence());
                        $scope.getData();
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
        $scope.LeaveTypeModel = {};
        $scope.LeaveTypeModel.Sequence = seq;
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
            CheckField("Code", $scope.LeaveTypeModel.Code);
            CheckField("User Name", $scope.LeaveTypeModel.UserName);
            CheckField("Leave Type", $scope.LeaveTypeModel.LeaveType);
        } catch (ex) {
            throw ex;
        }
    };

}