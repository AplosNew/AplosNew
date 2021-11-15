'use strict';
multipleResignationApprovalNewController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function multipleResignationApprovalNewController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Multiple Seperation Approval';
    $scope.ApprovedList = [];
    $scope.StatusList = [];
    $scope.popUpDataList = [];
    $scope.pendingList = [];
    $scope.appliedList = [];
    $scope.tempList = [];
    $scope.tempList2 = [];
    $scope.message = null;
    $scope.remarks_popup = '';
    $scope.index_popup = -1;
    $scope.remarks_popup2 = '';
    $scope.index_popup2 = -1;
    $scope.message = '';
    $scope.SeparationTypeList = [];
    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
    $scope.onCreate = function (args) {

        $("#buttonName").ejButton({
            text: "OK",
            click: function (args) {
                var eDialog = $("#dialogRemarks").data("ejDialog");
                eDialog.close();
            }
        });
    }
    $scope.Resignation = {
        Id: null,
        ResignationDate: null,
        Reason: null,
        AttachLetter: null,
        ApprovedDate: null,
        EffectiveDate: null,
        ApprovedEffectiveDate: null,
        SeparationType: null,
        SpecialFollowUP: false,
        Remarks: null,
        EmployeeId: null,
        PlantId: null,
        CompanyId: null,
        EmployeeName: null,
        EmployeeCode: null,
        Designation: null,
        EmployeeCategory: null,
        DOJ: null,
        DOC: null,
        IsApproved: false,
        Active: false

    };

    $scope.searchByList = [

        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Resignation Date',
            'value': 'ResignationDate'
        }

    ];
    cboService.getEnumCbo("enum/GetApprovalStatusCbo", function (result) {
        $scope.StatusList = result;
    });

    //cboService.GetSeparationTypeCbo(function (result) {
    //    $scope.SeparationTypeList = result;
    //});


    $scope.getSeparationType = function () {
        $http({
            method: 'Get',
            url: 'employees/resignationapprovalmultiplenew/GetSeparationType/'

        }).then(function successCallback(response) {
            if (response.data.Error === true) {

                ShowResult(response.data.Message, "failure");
            }
            else {
                $scope.SeparationTypeList = response.data;

            }
        });
    }
    $scope.getSeparationType();

    //$scope.Save = function () {
    //    try {

    //        Validate($scope.recruitmentSelections);
    //        Validate($scope.tempList);
    //        $scope.savedisable = true;
    //        $scope.save_list = [];
    //        //for (var i = 0; i < baseService.arrayLength($scope.recruitmentSelections); i++) {
    //        //    if ($scope.recruitmentSelections[i].Active) {
    //        //        var ob = $scope.recruitmentSelections[i];
    //        //        $scope.save_list.push(ob);
    //        //    }
    //        //}
    //        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
    //            var ob = $scope.tempList[i];
    //            $scope.save_list.push(ob);
    //        }
    //        $http({
    //            method: 'POST',
    //            url: 'employees/resignationapprovalmultiple/create',
    //            data: { 'ResignationList': $scope.save_list }
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                $scope.btnDisable = false;
    //                $scope.savedisable = false;
    //                ShowResult(response.data.Message, "failure");
    //            }
    //            else {
    //                $scope.tempList = [];
    //                $scope.getAppliedListData();
    //                $scope.getPendingistData();
    //                ShowResult(response.data.Message, "success");
    //                $scope.savedisable = false;


    //            }
    //        }, function errorCallback(response) {
    //            $scope.savedisable = false;
    //            ShowResult(response.status.Message, "failure");
    //        });
    //        $scope.savedisable = false;
    //        return true;


    //    } catch (e) {
    //        $scope.savedisable = false;

    //        ShowResult(e, "failure");
    //    }
    //};
    $scope.SaveApproved = function () {
        try {
            ValidateApproved($scope.pendingList);
            ValidateApproved($scope.tempList2);
            $scope.savedisable = true;
            $scope.save_list1 = [];
            for (var i = 0; i < baseService.arrayLength($scope.tempList2); i++) {
                var ob = $scope.tempList2[i];
                $scope.save_list1.push(ob);
            }
            $http({
                method: 'POST',
                url: 'employees/resignationapprovalmultiplenew/create',
                data: { 'ResignationList': $scope.save_list1 }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.tempList2 = [];
                    $scope.pendingList = [];
                    $scope.getAppliedListData();
                    $scope.getPendingistData();
                    $scope.savedisable = false;

                }
            }, function errorCallback(response) {
                $scope.savedisable = false;
                ShowResult(response.status.Message, "failure");
            });
            $scope.savedisable = false;
            return true;


        } catch (e) {
            $scope.savedisable = false;

            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }
    function IsExists(Id, aplist) {
        try {
            var r = false;
            for (var i = 0; i < baseService.arrayLength(aplist); i++) {
                if (aplist[i].Id === Id) {
                    r = true;
                }
            }
            return r;
        } catch (e) {
            throw e;
        }
    }
    $scope.selectPLMultiple = function () {
        for (var i = 0; i < baseService.arrayLength($scope.pendingList); i++) {
            if ($scope.pendingList[i].IsSelectedId) {
                var ob = $scope.pendingList[i];
                if (IsExists(ob.Id, $scope.pendingList) === false) {
                    $scope.pendingList.push(ob);
                }//exists
            }//sel
        }//for
        angular.element(document.querySelector('#searchpopupml')).modal('hide');
    };
    $scope.selectChValueId = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.EmployeeId) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].EmployeeId === data.EmployeeId) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                    if ($scope.tempList[i].EmployeeId === data.EmployeeId) {
                        $scope.tempList.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    $scope.selectChValueId2 = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList2, data.EmployeeId) === false) {
                    $scope.tempList2.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList2); i++) {
                        if ($scope.tempList2[i].EmployeeId === data.EmployeeId) {
                            $scope.tempList2.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList2.push(data);
                }
            }
            else {
                for (var i = 0; i < baseService.arrayLength($scope.tempList2); i++) {
                    if ($scope.tempList2[i].EmployeeId === data.EmployeeId) {
                        $scope.tempList2.splice(i, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    //Cache
    function cacheActiveValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return true;
            }
        }
        return false;
    }
    function cacheSPFollowupValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].Remarks;
            }
        }
        return null;
    }
    function cacheRemarkValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].Remarks;
            }
        }
        return null;
    }
    function cacheStatusValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].ApprovalStatus;
            }
        }
        return null;
    }
    function cacheAPEDValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].ApprovedEffectiveDate;
            }
        }
        return null;
    }
    // end cache
    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.data.AttachLetter;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ResignationLetter + '/' + data.data.Id + extention;
    };
    $scope.filePath = virtualPath.ResignationLetter + '/';

    $scope.SelectionParameters = {
        limit: 10,
        offset: 10,
        order: 'ASC',
        sort: 'EmployeeName',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getListUrl = 'employees/resignationapprovalmultiplenew/multipleResignationPendingList';
    baseService.init($scope.getListUrl, null, null, null, 'EmployeeCode', 'EmployeeCode');
    $scope.getAppliedListData = function () {
        try {
            $scope.searchList = [];
            $scope.LoadDataList = function (pageno) {
                baseService.pagination(pageno)
                    .then(function (result) {
                        //if (result.Error)
                        //    return $scope.message = data.Message;
                        //else {
                        $scope.recruitmentSelections = result.Rows;
                        //console.log($scope.recruitmentSelections);
                        $rootScope.total_count = result.Total;
                        //$scope.message = result.Message;
                        for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
                            $scope.recruitmentSelections[i].Active = cacheActiveValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                            $scope.recruitmentSelections[i].SpecialFollowUP = cacheSPFollowupValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                            $scope.recruitmentSelections[i].Remarks = cacheRemarkValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                            $scope.recruitmentSelections[i].ApprovalStatus = cacheStatusValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                            $scope.recruitmentSelections[i].ApprovedEffectiveDate = cacheAPEDValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                            if (baseService.arrayLength($scope.searchList) === 0)
                                baseService.getDDLSearchColumn(result.Rows, $scope.searchList);
                        }
                        //}
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.LoadDataList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.getAppliedListData();

    $scope.SelectionParameters1 = {
        limit: 10,
        offset: 10,
        order: 'ASC',
        sort: 'AEFDate',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.XgetPendingistData = function () {
        try {
            $scope.Url = 'employees/resignationapprovalmultiplenew/MultipleResignationAppliedList';
            //baseService.setCurrentPage('dataList');
            $scope.LoadList = function (pageno) {

                baseService.paginationBase($scope.Url, pageno, $scope.SelectionParameters1)
                    .then(function (data) {
                        $scope.pendingList = data.Rows;
                        //$scope.SelectionParameters1.total_count = data.Total;
                        //$scope.message = data.Message;
                        for (var i = 0; i < $scope.pendingList.length; i++) {
                            $scope.pendingList[i].Active = cacheActiveValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            var remark = cacheRemarkValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            $scope.pendingList[i].Remarks = remark === null ? $scope.pendingList[i].Remarks : remark;
                            var status = cacheStatusValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            $scope.pendingList[i].ApprovalStatus = status === null ? $scope.pendingList[i].ApprovalStatus : status;
                            var date = cacheAPEDValue($scope.tempList2, $scope.pendingList[i].EmployeeId);

                            //$scope.pendingList[i].ApprovedEffectiveDate = (date === null ? $scope.pendingList[i].ApprovedEffectiveDate : date);

                            if (status === 'Rejected') {
                                $scope.pendingList[i].ApprovedEffectiveDate = '';
                            } else {
                                $scope.pendingList[i].ApprovedEffectiveDate = date === null ? $scope.pendingList[i].ApprovedEffectiveDate : date;
                            }
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.LoadList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.total_count = 0.00;
    $scope.getPendingistData = function () {
        try {
            $scope.Url = 'employees/resignationapprovalmultiplenew/MultipleResignationAppliedList';
            //baseService.setCurrentPage('dataList');
            $scope.LoadList = function (pageno) {

                $http({
                    method: 'GET',
                    url: $scope.Url,
                    params: {},
                    dataType: 'JSON'
                })
                    .then(function (response) {
                        $scope.appliedList = response.data;
                        //$rootScope.total_count = result.Total;
                        //$scope.message = data.Message;
                        for (var i = 0; i < $scope.pendingList.length; i++) {
                            $scope.pendingList[i].Active = cacheActiveValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            var remark = cacheRemarkValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            $scope.pendingList[i].Remarks = remark === null ? $scope.pendingList[i].Remarks : remark;
                            var status = cacheStatusValue($scope.tempList2, $scope.pendingList[i].EmployeeId);
                            $scope.pendingList[i].ApprovalStatus = status === null ? $scope.pendingList[i].ApprovalStatus : status;
                            var date = cacheAPEDValue($scope.tempList2, $scope.pendingList[i].EmployeeId);

                            //$scope.pendingList[i].ApprovedEffectiveDate = (date === null ? $scope.pendingList[i].ApprovedEffectiveDate : date);

                            if (status === 'Rejected') {
                                $scope.pendingList[i].ApprovedEffectiveDate = '';
                            } else {
                                $scope.pendingList[i].ApprovedEffectiveDate = date === null ? $scope.pendingList[i].ApprovedEffectiveDate : date;
                            }

                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            $scope.LoadList();
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.getPendingistData();

    $scope.setStatus = function (ob, Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId) {
                $scope.tempList[i].ApprovalStatus = $scope.recruitmentSelections[index].ApprovalStatus;
            }
        }
    };
    $scope.setRemarks = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].Remarks = $scope.recruitmentSelections[index].Remarks;
        }
    };
    $scope.setApprovalDate = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].ApprovedEffectiveDate = $scope.recruitmentSelections[index].ApprovedEffectiveDate;
        }
    };
    $scope.setFinalApprovalDate = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList2); i++) {
            if (Id === $scope.tempList2[i].EmployeeId)
                $scope.tempList2[i].ApprovedEffectiveDate = $scope.pendingList[index].ApprovedEffectiveDate;
        }
    };
    $scope.setFinalStatus = function (ob, Id, index) {
        //if (ob.ApprovalStatus = 'Rejected') {
        //    ob.ApprovedEffectiveDate = '';
        //}
        //else {
        //    ob.ApprovedEffectiveDate = $scope.pendingList[index].ApprovedEffectiveDate;
        //}

        if (ob.ApprovalStatus === 'Approved' || ob.ApprovalStatus === 'Hold') {
            ob.ApprovedEffectiveDate = $scope.pendingList[index].ApprovedEffectiveDate;
        } else {
            ob.ApprovedEffectiveDate = '';
        }
        for (var i = 0; i < baseService.arrayLength($scope.tempList2); i++) {
            if (Id === $scope.tempList2[i].EmployeeId) {
                $scope.tempList2[i].ApprovalStatus = $scope.pendingList[index].ApprovalStatus;
            }
        }
    };

    //$scope.showEntityPopUp = function () {
    //    $http.get('employees/resignationapprovalmultiple/GetEntityByEmployee')
    //        .then(function (response) {
    //            $scope.entityList = response.data;
    //        });
    //    angular.element(document.querySelector('#entityPopUp')).modal('show');
    //}
    //$scope.EntityList = function () {
    //    $http.get('employees/resignationapprovalmultiple/getEntity')
    //        .then(function successCallback(response) {
    //            if (!baseService.isUndefinedOrNull(response.data.Message)) {
    //                $scope.message = response.data.Message;
    //            }
    //            else {
    //                $scope.message = response.data;
    //            }

    //        }
    //        ), function errorCallBack(response) {
    //            showResult(response.Message, 'failure');
    //        }
    //}
    //$scope.EntityList();

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }

    function Validate(list) {
        try {
            var count = 0;
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                if (list[i].Active) {
                    count++;
                    var ob = list[i];

                    if (baseService.isUndefinedOrNull(ob.Remarks) === true) {
                        throw 'Remarks is required for Employee [' + ob.EmployeeCode + ']';
                    }

                    if (ob.ApprovalStatus === 'Pending' || ob.ApprovalStatus === '') {
                        throw 'select approval status for Employee [' + ob.EmployeeCode + ']';
                    }

                    if (baseService.isUndefinedOrNull(ob.ApprovalStatus) === true) {
                        throw 'select approval status for Employee [' + ob.EmployeeCode + ']';
                    }
                    if (ob.ApprovalStatus === 'Hold' || ob.ApprovalStatus === 'Approved') {
                        if (baseService.isUndefinedOrNull(ob.ApprovedEffectiveDate) === true) {
                            throw 'Approved Effective date is required for Employee [' + ob.EmployeeCode + ']'
                        }
                    }

                    if (baseService.isUndefinedOrNull(ob.ApprovedEffectiveDate) === true && ob.ApprovalStatus !== 'Rejected') {
                        throw 'select Approved Effective Date for Employee [' + ob.EmployeeCode + ']';
                    }

                    var regdate = new Date(ob.ResignationDate);
                    var appeffdate = new Date(ob.ApprovedEffectiveDate);
                    if (regdate > appeffdate) {
                        throw 'Approved Effective date cannot be less than Resignation Submission date for Employee [' + ob.EmployeeCode + ']';
                    }

                    var effDate = new Date(ob.ApprovedEffectiveDate);
                    var effDate2 = $filter('date')(effDate, 'dd-MMM-yy')
                    var effectiveDate = new Date(effDate2);

                    var d = new Date();
                    d.setDate(d.getDate() + 90);
                    var d1 = $filter('date')(d, 'dd-MMM-yy');
                    var d2 = new Date(d1);
                    if (effDate > d2) {
                        throw 'Approved Effective Date Cannot be Greater then [' + d1 + '] for Employee [' + ob.EmployeeCode + ']';
                    }


                }//active
                else {
                    count++;
                    var ob = list[i];
                    if (ob.ApprovalStatus) {
                        throw 'Please select the employee [' + ob.EmployeeCode + ']';
                    }//not active
                }
            }//for
            if (count === 0) {
                throw 'Please select an Employee';
            }
        } catch (e) {
            throw e;
        }
    }
    function ValidateApproved(list) {
        try {
            var count = 0;
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                if (list[i].Active) {
                    count++;
                    var ob = list[i];
                    if (baseService.isUndefinedOrNull(ob.Remarks) === true) {
                        throw 'Remarks is required for Employee [' + ob.EmployeeCode + ']';
                    }
                    if (ob.ApprovalStatus === 'Hold' || ob.ApprovalStatus === 'Approved') {
                        if (baseService.isUndefinedOrNull(ob.ApprovedEffectiveDate) === true) {
                            throw 'Approved Effective date is required for Employee [' + ob.EmployeeCode + ']';
                        }
                    }
                    if (baseService.isUndefinedOrNull(ob.ApprovalStatus) === true) {
                        throw 'Select approval status for Employee [' + ob.EmployeeCode + ']';
                    }
                    var regdate = new Date(ob.ResignationDate);
                    var appeffdate = new Date(ob.ApprovedEffectiveDate);
                    if (regdate > appeffdate) {
                        throw 'Applied Effective date must be greater than Resignation date for Employee [' + ob.EmployeeCode + ']';
                    }

                    var effDate = new Date(ob.ApprovedEffectiveDate);
                    var effDate2 = $filter('date')(effDate, 'dd-MMM-yy');
                    var effectiveDate = new Date(effDate2);

                    var d = new Date();
                    d.setDate(d.getDate() + 90);
                    var d1 = $filter('date')(d, 'dd-MMM-yy');
                    var d2 = new Date(d1);
                    if (effDate > d2) {
                        throw 'Approved Effective Date Cannot be Greater then [' + d1 + '] for Employee [' + ob.EmployeeCode + ']';
                    }
                }
            }//for

        } catch (e) {
            throw e;
        }
    }

    function ValidateApprovedUpdated(list) {
        try {
            var count = 0;
            //for (var i = 0; i < baseService.arrayLength(list); i++) {
                //if (list[i].Active) {
                   // count++;
                    var ob = list;
                    if (baseService.isUndefinedOrNull(ob.Remarks) === true) {
                       // manualValidation('GridResignationAppliedEmployee', true, 'Remarks is required for Employee [' + ob.EmployeeCode + ']'); 
                        throw 'Remarks is required for Employee [' + ob.EmployeeCode + ']';
                    }
                    if (ob.ApprovalStatus === 'Hold' || ob.ApprovalStatus === 'Approved') {
                        if (baseService.isUndefinedOrNull(ob.ApprovedEffectiveDate) === true) {
                            //manualValidation('GridResignationAppliedEmployee', true, 'Approved Effective date is required for Employee [' + ob.EmployeeCode + ']');
                            throw 'Approved Effective date is required for Employee [' + ob.EmployeeCode + ']';
                        }
                    }
                    if (baseService.isUndefinedOrNull(ob.ApprovalStatus) === true) {
                        //manualValidation('GridResignationAppliedEmployee', true, 'Select approval status for Employee [' + ob.EmployeeCode + ']');
                        throw 'Select approval status for Employee [' + ob.EmployeeCode + ']';
                    }
                    var regdate = new Date(ob.ResignationDate);
                    var appeffdate = new Date(ob.ApprovedEffectiveDate);
                    if (regdate > appeffdate) {
                        //manualValidation('GridResignationAppliedEmployee', true, 'Applied Effective date must be greater than Resignation date for Employee [' + ob.EmployeeCode + ']');
                        throw 'Applied Effective date must be greater than Resignation date for Employee [' + ob.EmployeeCode + ']';
                    }

                    var effDate = new Date(ob.ApprovedEffectiveDate);
                    var effDate2 = $filter('date')(effDate, 'dd-MMM-yy');
                    var effectiveDate = new Date(effDate2);

                    var d = new Date();
                    d.setDate(d.getDate() + 90);
                    var d1 = $filter('date')(d, 'dd-MMM-yy');
                    var d2 = new Date(d1);
                    if (effDate > d2) {
                        //manualValidation('GridResignationAppliedEmployee', true, 'Approved Effective Date Cannot be Greater then [' + d1 + '] for Employee [' + ob.EmployeeCode + ']');
                        throw 'Approved Effective Date Cannot be Greater then [' + d1 + '] for Employee [' + ob.EmployeeCode + ']';
                    }
                //}
            //}//for

        } catch (e) {
            throw e;
        }
    }
    $scope.getRemarkColor = function (item) {
        var remark = item.Remarks;
        if (remark === null || remark === '') {
            return 'empty';
        } else {
            return 'filled';
        }
    };
    $scope.tempRowData = {};
    $scope.loadRemarks = function (args) {        
        $scope.tempRowData = args.data;
        
        var eDialog = $("#dialogRemarks").data("ejDialog");
        eDialog.open();
        //angular.element(document.querySelector('#remarksPopUpId')).modal('show');
    };
    $scope.setRemarks_popup = function (list, EmployeeId, remarks) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                list[i].Remarks = remarks;
                break;
            }
        }
    };
    $scope.closeRemarkPopUp = function () {
        var r = $scope.remarks_popup;
        var id_x = $scope.index_popup;
        $scope.recruitmentSelections[id_x].Remarks = r;
        var empId = $scope.recruitmentSelections[id_x].EmployeeId;

        $scope.setRemarks_popup($scope.tempList, empId, r);
        angular.element(document.querySelector('#remarksPopUpId')).modal('hide');
    };

    $scope.loadFinalRemarks = function (Remarks, index) {
        $scope.index_popup2 = index;
        $scope.remarks_popup2 = Remarks;
        angular.element(document.querySelector('#remarksPopUpId2')).modal('show');
    };
    $scope.setFinalRemarks_popup = function (list, EmployeeId, remarks) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                list[i].Remarks = remarks;
                break;
            }
        }

    };
    $scope.closeFinalRemarkPopUp = function () {
        var r = $scope.remarks_popup2;
        var id_x = $scope.index_popup2;
        $scope.pendingList[id_x].Remarks = r;
        var empId = $scope.pendingList[id_x].EmployeeId;
        $scope.setFinalRemarks_popup($scope.tempList2, empId, r);
        angular.element(document.querySelector('#remarksPopUpId2')).modal('hide');
    };

    $scope.Clear = function () {
        ClearOb($scope.Resignation);
    };
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }


    $scope.ReportEmployeeInfo = function () {

        location.href = 'employees/resignationapprovalmultiplenew/ReportEmployeeInfo';
    };
    $scope.As_ReportEmployeeInfo = function () {

        location.href = 'employees/resignationapprovalmultiplenew/As_ReportEmployeeInfo';
    };


    $("#uploadBtn").change(function () {
        //$scope.filedata = this.files[0];
    });
    function selectDoubleClick(data) {
        $scope.Resignation.EmployeeId = data.EmployeeId;
        $scope.Resignation.EmployeeName = data.EmployeeName;
        $scope.Resignation.EmployeeCode = data.EmployeeCode;
        $scope.Resignation.Designation = data.Designation;
        $scope.Resignation.DOJ = data.DOJ;
        $scope.Resignation.DOC = data.DOC;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.PlantId = data.PlantId;
        $scope.Resignation.ResignationDate = data.ResignationDate;
        $scope.Resignation.EffectiveDate = data.EffectiveDate;
        $scope.Resignation.EmployeeCategory = data.EmployeeCategory;
        $scope.Resignation.Reason = data.Reason;
        $scope.Resignation.AttachLetter = data.AttachLetter;
        $scope.Resignation.Id = data.Id;
        $scope.Resignation.IsApproved = data.IsApproved;
        $scope.Resignation.SpecialFollowUP = data.SpecialFollowUP;
        $scope.Resignation.Remarks = data.Remarks;
        $scope.Resignation.ApprovedEffectiveDate = data.ApprovedEffectiveDate;
        $scope.closePopUp();
    }
    $scope.getSearchObject = function (ob) {
        try {
            switch ($scope.search_flag) {
                case 'EMP':
                    selectDoubleClick(ob);
                    break;
                default:
                // $scope.getMaterialMasterSearchData();
            }
            $scope.search_flag = '';
            angular.element(document.querySelector('#search_popup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };



    $scope.actionCompleteSingleEmployee = function (args) {
        try {
            if (args == "refresh" || args.requestType == "refresh") {
                var scrollerwidth = 0;
                var gridObj = null;

                try {
                    gridObj = $("#GridResignationAppliedEmployeeList").ejGrid("instance");
                    //scrollerwidth = $("#TabEmployee").width();//Obtain the width of the container
                    gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth, height: 400 } });//pass the obtainer width and height to gridmodel options
                    gridObj.windowonresize();
                } catch (e) {

                }



            }
        } catch (e) {

        }
    };
    $window.onload = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
    };
    $window.onresize = function (event) {
        $scope.actionCompleteSingleEmployee("refresh");
    };
    $scope.rowDataBoundSingleEmployee = function rowDataBoundSingleEmployee(e) {

        if (!baseService.isUndefinedOrNull(e.data.ErrorMessage) && e.data.ErrorMessage != "")
            e.row.css("background-color", "#ff0000");

    };

    function nullrecorder(val) {
        if (baseService.isUndefinedOrNull(val))
            return "";

        return val;
    }

    $scope.Save = function () {
        var DataToBeSaved = [];
        for (var i = 0; i < $scope.appliedList.length; i++) {
            // $scope.appliedList[i].ErrorMessage = "";

            if (
                nullrecorder($scope.appliedList[i].ApprovedEffectiveDate) != nullrecorder($scope.appliedList[i].ApprovedEffectiveDateOld)
                || nullrecorder($scope.appliedList[i].SeparationType) != nullrecorder($scope.appliedList[i].SeparationTypeOld)
                || nullrecorder($scope.appliedList[i].Remarks) != nullrecorder($scope.appliedList[i].RemarksOld)
                || nullrecorder($scope.appliedList[i].ApprovalStatus) != nullrecorder($scope.appliedList[i].ApprovalStatusOld)
                || nullrecorder($scope.appliedList[i].SpecialFollowUp) != nullrecorder($scope.appliedList[i].SpecialFollowUpOld)
            ) {
                try {
                    ValidateApprovedUpdated($scope.appliedList[i]);
                    DataToBeSaved.push($scope.appliedList[i]);
                    $http({

                        method: 'POST',
                        url: 'employees/resignationapprovalmultiplenew/UpdateApprovalStatus',
                        data: { 'ResignationList': DataToBeSaved }

                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');

                            for (var i = 0; i < response.data.Data.length; i++) {
                                var row = $filter('filter')($scope.employeeAttendanceBySingleDate, { 'Id': response.data.Data[i].Id });
                                if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
                                    row[0].ErrorMessage = response.data.Data[i].ErrorMessage;
                                }
                            }
                            $scope.getPendingistData();
                            var gridObj = $("#GridResignationAppliedEmployeeList").data("ejGrid");
                            gridObj.refreshContent();
                        }
                        else {
                            ShowResult(response.data.Message, 'success');

                            $scope.selectSigleDate();

                        }

                    });
                } catch (e) {
                    ShowResult(e, 'failure');

                }             

            }       
        }


        
    };


}
