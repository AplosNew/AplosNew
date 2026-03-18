'use strict';
DispatchPlanController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller', 'cboService', '$window'];
function DispatchPlanController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, cboService, $window) {
    $rootScope.title = "Dispatch Plan";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'OrderManagements/DispatchMaster/';
    $scope.getListUrl = $scope.path + 'GetDispatchPlanList';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'DispatchPlanInsert';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.dispatchPlan = {
        Id: null,
        FromDate: null,
        ToDate: null,
        CloseDate: null,
        PlanNo: null,
        ResponsiblePersonId: null,
        CheckBy: null,
        ApproveBy: null,
        MonthNo: null,
        YearNo: null,
        RevisionNo: null,
        ByWhom: $window.employeeName,
        ByWhomId: $window.employeeId,
        Active: true
    };
    $scope.dispatchPlanNew = Object.assign({}, $scope.dispatchPlan);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.searchByServiceMasterList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'Year',
            'value': 'YearNo'
        },
        {
            'name': 'Month',
            'value': 'MonthNo'
        },
        {
            'name': 'PlanNo',
            'value': 'PlanNo'
        }
    ];
    $scope.dispatchPlanMasters = [];
    baseService.init($scope.getListUrl, null, null, null, "Id", "PlanNo");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.dispatchPlanMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.dispatchPlanNew = $scope.dispatchPlanMasters[$scope.index];
        $scope.dispatchPlanNew.ByWhom = $window.employeeName;
        $scope.dispatchPlanNew.YearNo = Number($scope.dispatchPlanNew.YearNo);
        $scope.Action = "Update";
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };


    $scope.CheckByList = [];
    $scope.getCboCheckedByList = function () {
        cboService.getAuthorizationConfigCbo('DispatchPlanCheckedBy', function (result) {
            $scope.CheckByList = result;
            if ($scope.CheckByList.length == 1) {
                $scope.dispatchPlanNew.CheckBy = $scope.CheckByList[0].Id;
            }
        });
    };
    $scope.getCboCheckedByList();


    $scope.ApproveByList = [];
    $scope.getCboApprovedList = function () {
        cboService.getAuthorizationConfigCbo('DispatchPlanApproveBy', function (result) {
            $scope.ApproveByList = result;
            if ($scope.ApproveByList.length == 1) {
                $scope.dispatchPlanNew.ApproveBy = $scope.ApproveByList[0].Id;
            }
        });
    };
    $scope.getCboApprovedList();



    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $http({
            method: 'GET',
            url: 'Costings/QuickCostingMaster/getemployeelist'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }
    //$scope.getPopUpData();

    $scope.setEmpData = function (obj) {
        $scope.dispatchPlanNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.dispatchPlanNew.ResponsiblePerson = obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };
    $scope.getPlanNo = function () {
        $scope.dispatchPlanNew.PlanNo = null;
        $scope.dispatchPlanNew.PlanNo = $scope.dispatchPlanNew.YearNo + '-' + $scope.dispatchPlanNew.MonthNo + '-' + $scope.dispatchPlanNew.RevisionNo;
    }
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.dispatchPlanNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'data': $scope.dispatchPlanNew
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    $scope.Clear();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    }

    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };

    $scope.UploadedData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.UploadedData = [];
                        $scope.UploadedData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.SaveUploadedData = function () {
        try {
            for (var i = 0; i < $scope.UploadedData.length; i++) {
                //if (baseService.isUndefinedOrNull($scope.UploadedData[i].ServiceMasterId)) {
                //    throw "ServiceMasterId is required.";
                //}
                $scope.UploadedData[i].Id = null;

            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveUploadedData',
                data: {
                    'data': $scope.UploadedData
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UploadedData = [];
                    $("#uploadImage").val(null);
                    $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };

    $scope.Clear = function () {
        $scope.dispatchPlan = {
            Id: null,
            FromDate: null,
            ToDate: null,
            CloseDate: null,
            PlanNo: null,
            ResponsiblePersonId: null,
            CheckBy: null,
            ApproveBy: null,
            MonthNo: null,
            YearNo: null,
            RevisionNo: null,
            ByWhom: $window.employeeName,
            ByWhomId: $window.employeeId,
            Active: true
        };
        $scope.dispatchPlanNew = Object.assign({}, $scope.dispatchPlan);
    }
}

