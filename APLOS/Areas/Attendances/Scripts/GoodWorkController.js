'use strict';
GoodWorkController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function GoodWorkController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Good Work';
    $scope.ModelList = [];
    $scope.path = 'Attendances/GoodWork/';
    $scope.saveUrl = $scope.path + 'CreateGoodWork';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    //$scope.deleteUrl = $scope.path + 'delete/';
    $scope.deleteChildUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.LoadEmpListUrl = $scope.path + 'LoadEmployeelist';
    $scope.Action = 'Save';
    $scope.passwordShow = true;
    $controller("employeeBaseController", { $scope: $scope, $http: $http });
    //***********************************Good Work ********************************************************//

    $scope.ModelTemp = {
        Id: null,
        WorkDate: null,
        EmployeeCategory: null,
        Department: null,
        SubSection: null,
        Section: null,
        Designation:null,
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
        Remarks: null
    };
    $scope.ModelEmpNew = Object.assign({}, $scope.ModelEmpTemp);


    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetEmployeeCategoryList",
        }).then(function successCallback(response) {
            $scope.EmployeeCategoryList = response.data;
        });
    }
    $scope.getEmployeeCategory();


    $scope.selectShift = function () {
        $scope.getsS();
        angular.element(document.querySelector('#ShiftPop')).modal('show');
    }


    ////Load Employee

    $scope.EmployeeList = [];
    $scope.SelectedEmployeeList = [];
    $scope.getEmploymeeList = function () {
        try {
            $http.get($scope.LoadEmpListUrl + '?empCategory=' + $scope.ModelNew.EmployeeCategory + '&department=' + $scope.ModelNew.EmployeeCategory + '&section=' + $scope.ModelNew.Section
                + '&subSection=' + $scope.ModelNew.SubSection + '&designation=' + $scope.ModelNew.Designation)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmployeeList = response.data;
                        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
                        eDialog.open();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetSelectedEmployeeList = function () {
        var eDialog = $("#dialogEmployeeInfo").data("ejDialog");
        eDialog.close();
        try {
            $scope.SelectedEmployeeList = [];
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                if ($scope.EmployeeList[i].CheckBoxSelect === true) {
                    $scope.SelectedEmployeeList.push($scope.EmployeeList[i]);
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.refreshTemplateemployee4 = function (args) {
        $("#headchk4").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmployeeInfoList").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.EmployeeList.length; i++) {
                $scope.EmployeeList[i].CheckBoxSelect = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmployeeInfoList").data("ejGrid");
        gridObj.refreshContent();
    };


    $scope.RemoveSelectedEmployeeList = function () {
        var gridObj = $("#GridSelectedEmployeeInfoList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        try {
            $scope.SelectedEmployeeList.splice($scope.SelectedEmployeeList.indexOf(data), 1);
            gridObj.refreshContent();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };


    ////Load Employee

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

    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.DepartmentList = result;
    });
    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.SectionList = result;
    });

    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.SubSectionList = result;
    });
    cboService.getbyDesignationMasterCbo(function (result) {
        $scope.designationList = result;
    });

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
        $scope.GoodWorkList[$scope.tempIndex].ApprovedById = null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByCode = null;
        $scope.GoodWorkList[$scope.tempIndex].ApprovedByName = null;
    }

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    // #region CalcTime
    $scope.getMinute = function (data, index) {
        try {
            if (!baseService.isUndefinedOrNull(data.FromTime) && !baseService.isUndefinedOrNull(data.ToTime)) {
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

    $scope.removeRow = function (data) {
        $http({
            method: 'GET',
            url: 'Attendances/GoodWork/DeleteChildUrl?Id=' + data.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getGoodWorkEmpList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew, 'goodWorkDetail': $scope.GoodWorkList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Clear();
                    $scope.getData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.GoodWorkList = [];
        return true;
    };
    $scope.getData = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetGoodWorkList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.getData();

     $scope.GetDblClick = function (args) {
         $scope.ModelNew = Object.assign({}, args.data);
         $scope.GetGoodWorkDetailCenter();
        /* $scope.getGoodWorkEmpList();*/
         //$scope.showByWhomEmployeeListPopUp();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.GoodWorkList = [];
    $scope.GetGoodWorkDetailCenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetGoodWorkDetailCenter?goodWorkId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.GoodWorkList = resp.data;
        });
    }
    //$scope.GetGoodWorkDetailCenter();

    // #endregion CalcTime
    ////***********************************User ********************************************************//

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



}