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
    $scope.dispatchPlanNew = {
        Id: null,
        FromDate: $filter("dateFiltering")(Date.now()),
        ToDate: $filter("dateFiltering")(Date.now()),
        CloseDate: $filter("dateFiltering")(Date.now()),
        PlanNo: null,
        ResponsiblePersonId: null,
        CheckBy: null,
        ApproveBy: null,
        MonthNo: null,
        YearNo: null,
        RevisionNo: null,
        ByWhom: null,

    };

    //$scope.dispatchPlanNew = Object.assign({}, $scope.dispatchPlanVM);

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
        $scope.dispatchPlanNew.PlanNo = $scope.dispatchPlanNew.YearNo+'-' + $scope.dispatchPlanNew.MonthNo+'-' + $scope.dispatchPlanNew.RevisionNo;
    }
    $scope.Save = function () {
        //$scope.$broadcast('show-errors-check-validity');
        //if ($scope.dispatchPlanNewForm.$valid) {
        if ($scope.Action == "Save") {
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
                    //$scope.getData();
                    //$scope.serviceMasters = $filter('orderBy')($scope.serviceMasters, 'Sequence');
                    //baseService.paginationAdd();
                    //ClearFields(response.data.Sequence);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        //else if ($scope.Action == "Update") {
        //    $http({
        //        method: 'POST',
        //        url: $scope.updateUrl,
        //        data: $scope.serviceMaster,
        //        dataType: 'JSON'
        //    }).then(function successCallback(response) {
        //        if (response.data.Error == true)
        //            ShowResult(response.data.Message, 'failure');
        //        else {
        //            ShowResult(response.data.Message, 'success');
        //            $scope.getData();
        //            ClearFields(response.data.Sequence);
        //        }
        //    }, function errorCallBack(response) {
        //        ShowResult(response.data.Message, 'failure');
        //    });
        //}
        // }
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
}

