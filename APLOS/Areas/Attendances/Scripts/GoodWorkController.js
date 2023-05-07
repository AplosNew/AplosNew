'use strict';
GoodWorkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateUserEditControl';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    //$scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.Action = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    //***********************************Good Work ********************************************************//

    $scope.ModelTemp = {
        Id: null,
        Date: null,
        ShiftId: null,
        Shift: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);


    $scope.ModelEmpTemp = {
        Id: null,
        EmployeeCode: null,
        EmployeeName: null,
        FromTime: null,
        ToTime: null,
        CalculatedTime: null,
        Purpose: null,
        PurposeCategory: null,
        ApprovedById: null,
        ApprovedByName: null,
        Remarks: null,
        SubSection: null,
        Section: null,
        Department: null
    };
    $scope.ModelEmpNew = Object.assign({}, $scope.ModelEmpTemp);

    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }

    $scope.ShiftList = [];
    $scope.getsS = function () {
        $http({
            method: 'GET',
            url: 'employees/route/getShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }

    $scope.doubleShift = function (e) {
        $scope.ModelNew.ShiftId = e.data.ShiftId;
        $scope.ModelNew.Shift = e.data.ShiftDefination;
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPop')).modal('hide');
    }

    $scope.showEmployeeListPopUp = function () {
        baseService.setCurrentPage('employeeList');
        $scope.getEmployeeData = function (pageno) {
            var url = null;
            if (baseService.isUndefinedOrNull($scope.employeeUrl)) {
                url = 'Attendances/GoodWork/GetEmployeeListByPlant';
            }
            else {
                url = $scope.employeeUrl;
            }
            baseService.paginationBase(url, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            $scope.ModelEmpNew.EmployeeId = employee.SystemId;
            $scope.ModelEmpNew.EmployeeCode = employee.EmployeeCode;
            $scope.ModelEmpNew.EmployeeName = employee.EmployeeName;
            $scope.getGoodWorkEmpList($scope.ModelEmpNew.EmployeeId);
            //$scope.getEmployeeWiseOutstandingAdvance($scope.voucher.EmployeeId);
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector("#employeePopUp")).modal("hide");
    };

    $scope.GoodWorkList = [];
    $scope.getGoodWorkEmpList = function (employeeId) {
        $http({
            method: "get",
            url: "Attendances/GoodWork/GetGoodWorkDataList?empId=" + employeeId
        }).then(function successCallback(response) {
            $scope.GoodWorkList = response.data;
        });
    };
    $scope.getGoodWorkEmpList();

    $scope.popUpDataList = [];
    $scope.showByWhomEmployeeListPopUp = function (index) {
        try {
            $scope.tempIndex = index;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Attendances/GoodWork/GetAllActiveEmployeeData'
            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SelectEmployee = function (arg) {
        $scope.GoodWorkList[$scope.tempIndex].ApprovedById = arg.data.SystemId;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByCode = arg.data.EmployeeCode;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByName = arg.data.EmployeeName;
        $scope.closePopUp();
    }


    $scope.clearEmp = function () {
        $scope.GoodWorkList[$scope.tempIndex].ApprovedById =null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByCode= null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByName =null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    // #region CalcTime
    $scope.obj = {};
    $scope.getMinute = function (data,index) {
        try {
            if (!baseService.isUndefinedOrNull(data.FromTime) && !baseService.isUndefinedOrNull(data.ToTime)) {
                $scope.obj = data.data;
                $scope.MinuteUrl = 'Attendances/GoodWork/GetMinute'
                $http({
                    method: 'POST',
                    url: $scope.MinuteUrl,
                    data: { 'data': data },
                    dataType: 'JSON'
                }).then(function successCallback(response) {

                    /*data.CalculatedTime = response.data;*/
                    $scope.GoodWorkList[index].CalculatedTime = response.data;
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    //$scope.GoodWorkEmployeeList = [];
    //$scope.GetGoodWorkcenter = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'GetGoodWorkcenter',
    //        dataType: 'JSON'
    //    }).then(function succ(resp) {
    //        $scope.GoodWorkEmployeeList = resp.data;
    //    });
    //}
    //$scope.GetGoodWorkcenter();

    // #endregion CalcTime
    ////***********************************User ********************************************************//
    //$scope.removeRow = function (data) {
    //    /* $scope.HrefDataList.splice(index, 1);*/

    //    $http({
    //        method: 'GET',
    //        url: 'TaskManagement/TaskAppliedOn/DeleteChildUrl?Id=' + data.Id,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.getHrefList();
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //};

    //$scope.HrefDataList = [];
    //$scope.getHrefList = function () {
    //    $http({
    //        method: "get",
    //        url: "TaskManagement/TaskAppliedOn/GetHrefDatasList?hrefId=" + $scope.ModelNew.UserId
    //    }).then(function successCallback(response) {
    //        $scope.HrefDataList = response.data;
    //    });
    //};

    ////***********************************Href ********************************************************//
    //$scope.hrefvalueData = '';
    //$scope.popUpHrefParameters = {
    //    limit: 10,
    //    offset: 0,
    //    order: 'asc',
    //    sort: 'Id',
    //    searchBy: "Id",
    //    pageSize: 10,
    //    total_count: 0,
    //    search: null,
    //    serverPagination: true
    //};

    //$scope.popUpHref = function () {
    //    $scope.popUpHrefDataList = [];
    //    $scope.popUpUrl = 'TaskManagement/TaskAppliedOn/GetHreflist';
    //    $scope.getPopUpHrefData = function (data) {
    //        baseService.paginationBase($scope.popUpUrl, data, $scope.popUpHrefParameters)
    //            .then(function (result) {
    //                $scope.popUpHrefDataList = result.Rows;
    //                $scope.popUpHrefParameters.total_count = result.Total;
    //            }, function () {
    //                ShowResult(commonMessage.NetworkError, 'failure', 'popUphrefId');
    //            }).finally(function () {
    //            });
    //    };
    //    angular.element(document.querySelector('#popUphrefId')).modal('show');
    //    $scope.getPopUpHrefData();
    //};


    //$scope.setSelected = function (data) {
    //    $scope.selectHrefDoubleClick(data);
    //};

    //$scope.selectHrefDoubleClick = function (a) {

    //    var obj = {};
    //    obj.Id = null;
    //    obj.HrefId = a.Id;
    //    obj.Href = a.Href;
    //    obj.Controller = a.Controller;
    //    obj.Description = a.Description;

    //    $scope.HrefDataList.push(obj);
    //    obj = {};

    //    $scope.closeHrefPopUp();
    //};

    //function checkProcessExist(list, Id) {
    //    for (var i = 0; i < list.length; i++) {
    //        if (list[i].HrefId === Id) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //$scope.selectHrefSingleClick = function (data) {
    //    $scope.hrefrowSelected = data.Id;
    //    $scope.ModelNew.Href = data.Href;
    //    $scope.hrefvalueData = data;
    //};

    //$scope.selectByButtonHref = function () {
    //    if (baseService.isUndefinedOrNull($scope.hrefvalueData)) {
    //        return ShowResult('Please at first select row', 'failure', 'popUphrefId');
    //    }
    //    $scope.selectHrefDoubleClick($scope.hrefvalueData)
    //    $scope.closeHrefPopUp();
    //};
    //$scope.closeHrefPopUp = function () {
    //    $scope.hrefvalueData = '';
    //    angular.element(document.querySelector('#popUphrefId')).modal('hide');
    //};
    ////***********************************Href ********************************************************//


    //$scope.Save = function () {
    //    try {
    //        $scope.$broadcast('show-errors-check-validity');

    //        if ($scope.ModelNewForm.$valid) {
    //            if ($scope.ModelNew.Password == $scope.ModelNew.RePassword) {

    //                $http({
    //                    method: 'POST',
    //                    url: $scope.saveUrl,
    //                    data: {
    //                        'data': $scope.ModelNew
    //                        , 'userECDetail': $scope.HrefDataList
    //                    },
    //                    dataType: 'JSON'
    //                }).then(function successCallback(response) {
    //                    if (response.data.Error === true) {
    //                        ShowResult(response.data.Message, 'failure');
    //                    }
    //                    else {
    //                        ShowResult(response.data.Message, 'success');
    //                        $scope.Clear();
    //                        $scope.getData();
    //                    }
    //                }), function errorCallBack(response) {
    //                    ShowResult(response.data.Message, 'failure');
    //                }
    //            }
    //            else {
    //                ShowResult('Password and Confirm Password does not match!', 'failure');
    //            }
    //        }

    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};


    //$scope.getData = function () {
    //    $http({
    //        method: 'Get',
    //        url: $scope.path + "GetUserEditControlList",
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ModelList = response.data;
    //    });
    //}
    //$scope.getData();

    //$scope.ModelDetailList = [];
    //$scope.GetUserEditControlDetailData = function () {
    //    $http({
    //        method: 'Get',
    //        url: $scope.path + "GetUserEditControlDetailList?userEditControlId=" + $scope.ModelNew.Id,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ModelDetailList = response.data;
    //    });
    //}
    ////$scope.getUserEditControlDetailData();

    //$scope.GetDblClick = function (args) {
    //    $scope.ModelNew = Object.assign({}, args.data);
    //    $scope.ModelNew.RePassword = args.data.Password;
    //    $scope.getData();
    //    $scope.getHrefList();
    //    //$scope.GetUserEditControlDetailData($scope.ModelNew.Id);
    //    $scope.Action = 'Update';
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }
    //};

    //$scope.Delete = function () {
    //    if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
    //        $http({
    //            method: 'POST',
    //            url: 'TaskManagement/TaskAppliedOn/Delete',
    //            data: {'Id': $scope.ModelNew.Id},
    //            dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.getData();
    //                $scope.Clear();
    //            }
    //            function errorCallBack(response) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        });
    //    }
    //};

    //$scope.Clear = function () {
    //    $scope.Action = 'Save';
    //    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //    $scope.HrefDataList = [];
    //    //$scope.getHrefList = [];
    //    return true;
    //};


}