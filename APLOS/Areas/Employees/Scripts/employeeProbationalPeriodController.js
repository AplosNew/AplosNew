'use strict';
employeeProbationalPeriodController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter','$window'];
function employeeProbationalPeriodController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = 'Probational Period';
    $scope.EmployeeProbationalPeriod = [];
    $scope.ApprovalList = [];
    $scope.entityList = [];
    $scope.tempList = [];
    $scope.message = '';
    $scope.EmpId = '';
    $scope.PromotionFlag = '';
    $scope.remarks_popup = '';
    $scope.index_popup = -1;
    $scope.maxDate = new Date().toDateString();

    $scope.EmployeeProbationalPeriod = {
        Id: null,
        EmployeeId: null,
        EmployeeName: null,
        Email: null,
        GivenDesignation: null,
        PlantId: null,
        CompanyId: null,
        EmployeeCode: null,
        Designation: null,
        EmployeeCategory: null,
        ApprovalStatus: null,
        Present: new Date(),
        Remarks: null,
        DOJ: null,
        DOC: null,
        NewDOC: null,
        ConfirmAfterDays: null,
        ExtendedDays: null,
        FDOC: null,
        IsPastDOCAllowed: false,
        pastDOCdaysAllowed: null
    };

    $scope.searchByList = [
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        },
        {
            'name': 'Given Designation',
            'value': 'GivenDesignation'
        }
    ];

    cboService.getCboPlant(function (result) {
        $scope.PlantList = result;
    });
    $scope.RefreshBody = function () {
        $scope.loadEmployee($scope.Resignation.PlantId);
    };

    $scope.Save = function () {
        try {
            Validate($scope.recruitmentSelections);
            Validate($scope.tempList);
            $scope.savedisable = true;
            $scope.EmployeeProbationalPeriod = [];
            for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                //if ($scope.recruitmentSelections[i].Active) {
                var ob = $scope.tempList[i];
                $scope.EmployeeProbationalPeriod.push(ob);
                //}
            }

            $http({
                method: 'POST',
                url: 'employees/employeeprobationalperiod/create',
                data: { 'EmployeeProbationalList': $scope.EmployeeProbationalPeriod }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;

                    ShowResult(response.data.Message, "failure");
                }
                else {
                    //$scope.releaseChecked();
                    $scope.tempList = [];
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;
                    $scope.Clear();
                    $scope.LoadDataList();
                   
                   
                   
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
    $scope.SaveWithpromotion = function (EmployeeId) {
        try {
         
            Validate($scope.recruitmentSelections);
            Validate($scope.tempList);
            $scope.savedisable = true;
            $scope.EmployeeProbationalPeriod = [];
            for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                //if ($scope.recruitmentSelections[i].Active) {
                var ob = $scope.tempList[i];
                $scope.EmployeeProbationalPeriod.push(ob);
                //}
            }
            if ($scope.EmployeeProbationalPeriod[0].ApprovalStatus !== 'Confirm') {
                throw 'Select  Confirm';
            }
            if ($scope.EmployeeProbationalPeriod[0].EmployeeId !== EmployeeId) {
                throw 'Select  Employee ';
            }
            $http({
                method: 'POST',
                url: 'employees/employeeprobationalperiod/create',
                data: { 'EmployeeProbationalList': $scope.EmployeeProbationalPeriod }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.btnDisable = false;
                    $scope.savedisable = false;

                    ShowResult(response.data.Message, "failure");
                }
                else {
                    //$scope.releaseChecked();
                    $scope.tempList = [];
                    ShowResult(response.data.Message, "success");
                    $scope.savedisable = false;

                    $scope.LoadDataList();
                    $scope.LoadConfirmedDataList();
                    if ($scope.EmployeeProbationalPeriod.length > 0) {
                        $scope.EmpId = $scope.EmployeeProbationalPeriod[0].EmployeeId;
                        $scope.PromotionFlag = 'Confirmation';
                        $window.open('/applicationpanel#!/promotion?EmpId=' + $scope.EmpId + '&PromotionFlag=' + $scope.PromotionFlag + '&DOC=' + $scope.EmployeeProbationalPeriod[0].DOC, '_blank');
                    }
                    //-----
                    //$http({
                    //    method: 'POST',
                    //    url: 'humanresource/employeepromotion/Promotion',
                    //    data: { 'ConfirmedDataList': $scope.EmployeeProbationalPeriod }
                    //}).then(function successCallback(response) {
                    //    if (response.data.Error === true) {

                    //        ShowResult(response.data.Message, "failure");
                    //    }

                    //});
                    //-----
                    $scope.Clear();
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
        for (var i = 0; i < baseService.arrayLength($scope.popUpDataList); i++) {
            if ($scope.popUpDataList[i].IsSelectedId) {
                var ob = $scope.popUpDataList[i];
                if (IsExists(ob.Id, $scope.ApprovedList) === false) {
                    $scope.ApprovedList.push(ob);
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
                for (var j = 0; j < baseService.arrayLength($scope.tempList); i++) {
                    if ($scope.tempList[j].EmployeeId === data.EmployeeId) {
                        $scope.tempList.splice(j, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempList(list, EmployeeId) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return true;
            }
        }
        return false;
    }

    function cacheActiveValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return true;
            }
        }
        return false;
    }

    function cacheExtendedValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].ConfirmAfterDays;
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

    function cacheNewDOCValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].NewDOC;
            }
        }
        return null;
    }

    function cacheFDOCValue(list, EmployeeId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === EmployeeId) {
                return list[i].FDOC;
            }
        }
        return null;
    }

    $scope.setStatus = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId) {
                $scope.tempList[i].ApprovalStatus = $scope.recruitmentSelections[index].ApprovalStatus;
                if ($scope.tempList[i].ApprovalStatus === 'Confirm') {
                    $scope.tempList[i].FDOC = '';
                }
            }
        }
    };

    $scope.setRemarks = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].Remarks = $scope.recruitmentSelections[index].Remarks;
        }
    };

    $scope.setExtendedDays = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].ConfirmAfterDays = $scope.recruitmentSelections[index].ConfirmAfterDays;
        }
    };

    $scope.setApprovalDate = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].NewDOC = $scope.recruitmentSelections[index].NewDOC;
        }
    };

    $scope.setFDOC = function (Id, index) {
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId)
                $scope.tempList[i].FDOC = $scope.recruitmentSelections[index].FDOC;
        }
    };

    $scope.olds = true;
    $scope.present = true;
    $scope.future = false;

    $scope.SelectionParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'DOCSort',
        searchBy: "EmployeeName",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };
    $scope.recruitmentSelections = [];
    $scope.LoadDataList = function () {
        $scope.SelectionParameters.offset = 0;
        $scope.GetAllData = function (pageno) {
            baseService.paginationBase('employees/employeeprobationalperiod/GetColorEmployeeList?old=' + $scope.olds + '&present=' + $scope.present + '&future=' + $scope.future, pageno, $scope.SelectionParameters)
                .then(function (data) {
                    $scope.recruitmentSelections = data.Rows;
                    angular.forEach($scope.recruitmentSelections, function (item, i) {
                        $scope.setProbationCount(item.EmployeeId, i);
                    });
                    $scope.SelectionParameters.total_count = data.Total;
                    for (var i = 0; i < $scope.recruitmentSelections.length; i++) {
                        $scope.recruitmentSelections[i].Active = cacheActiveValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        $scope.recruitmentSelections[i].ConfirmAfterDays = cacheExtendedValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        $scope.recruitmentSelections[i].Remarks = cacheRemarkValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        $scope.recruitmentSelections[i].ApprovalStatus = cacheStatusValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        $scope.recruitmentSelections[i].NewDOC = cacheNewDOCValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        $scope.recruitmentSelections[i].FDOC = cacheFDOCValue($scope.tempList, $scope.recruitmentSelections[i].EmployeeId);
                        if (baseService.arrayLength($scope.recruitmentSelections) === 0)
                            baseService.getDDLSearchColumn(data.Rows, $scope.recruitmentSelections);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.GetAllData();
    };
    $scope.LoadDataList();


    $scope.confirmationTemplateList = [];
    $scope.Id = null;
    cboService.getconfirmationTemplateCbo(function (result) {
        $scope.confirmationTemplateList = result;
    });

    // #region GetConfirmedEmployee

    $scope.confirmSearchList = [
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Designation',
            'value': 'Designation'
        },
        {
            'name': 'Given Designation',
            'value': 'GivenDesignation'
        }
    ];

    $scope.employeeparameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeName',
        searchBy: "EmployeeName",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };

    $scope.employeeList = [];
    $scope.LoadConfirmedDataList = function () {
        baseService.init('employees/employeeprobationalperiod/getconfirmedemployeedata', null, 10, null, 'EmployeeName', 'EmployeeName');
        $scope.GetListData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.employeeList = [];
                    $scope.employeeList = result.Rows;
                    $scope.employeeparameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.GetListData();
    };
    $scope.LoadConfirmedDataList();


    $scope.empparameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'EmployeeCode',
        searchBy: "EmployeeCode",
        pageSize: 10,
        total_count: 0,
        search: "",
        serverPagination: true
    };

    $scope.empList = [];
    $scope.LoadInActiveDataList = function () {
        baseService.init('Employees/EmployeeProbationalPeriod/GetIactiveEmployeeData', null, 10, null, 'EmployeeCode', 'EmployeeCode');
        $scope.GetData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.empList = result.Rows;
                    $scope.empparameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.GetData();
    };
    //$scope.LoadInActiveDataList();

    $scope.PrintLocal = function (empId, id, empType) {
        if (!baseService.isUndefinedOrNull(id)) {
            location.href = 'employees/employeeprobationalperiod/employeeconfirmation?empId=' + empId + '&empType=' + empType + '&tempId=' + id;
        }
    };

    //#endregion

    $scope.loadProbation = function (Id) {
        $http.get('employees/employeeprobationalperiod/getprobationbyid?EmployeeId=' + Id)
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#probationPopUp')).modal('show');
    };
    $scope.EmployeeInActive = function (Id) {
        $http.post('employees/employeeprobationalperiod/EmployeeInActive?EmployeeId=' + Id)
            .then(function (response) {
                ShowResult(response.data.Message, "success");
                $scope.LoadConfirmedDataList();
                $scope.LoadInActiveDataList();
            });
        //angular.element(document.querySelector('#probationPopUp')).modal('show');
    };
    $scope.EmployeeActive = function (Id) {
        $http.post('employees/employeeprobationalperiod/EmployeeActive?EmployeeId=' + Id)
            .then(function (response) {
                ShowResult(response.data.Message, "success");
                $scope.LoadConfirmedDataList();
                $scope.LoadInActiveDataList();
            });
        //angular.element(document.querySelector('#probationPopUp')).modal('show');
    };
    $scope.loadRemarks = function (Remarks, index) {
        $scope.index_popup = index;
        $scope.remarks_popup = Remarks;
        angular.element(document.querySelector('#remarksPopUpId')).modal('show');
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
        //$scope.remarks_popup = null;
        angular.element(document.querySelector('#remarksPopUpId')).modal('hide');
    };

    $scope.setProbationCount = function (Id, index) {
        $http.get('employees/employeeprobationalperiod/getprobationbyid?EmployeeId=' + Id)
            .then(function (response) {
                $scope.entityList = response.data;
                $scope.recruitmentSelections[index].cData = $scope.entityList.length;
            });
    };

    $scope.calcultedFinalDate = function (ob, Id, index) {
        var docDays = parseInt(ob.DOCDay);
        var exdocDays = 0;
        if (baseService.isUndefinedOrNull(ob.ConfirmAfterDays)) {
            exdocDays = 0;
        }
        else {
            exdocDays = parseInt(ob.ConfirmAfterDays);
        }

        var _fdocDays = docDays + exdocDays;
        var intdoj = new Date(ob.DOJ);
        intdoj.setDate(intdoj.getDate() + _fdocDays);
        var fdoc = $filter('dateFiltering')(intdoj, 'dd-MMM-yy');
        ob.FDOC = fdoc;
        if (baseService.isUndefinedOrNull(ob.ConfirmAfterDays)) {
            ob.FDOC = '';
        }
        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId) {
                if ($scope.tempList[i].ApprovalStatus === 'Extend') {
                    $scope.tempList[i].NewDOC = '';
                }
                $scope.tempList[i].ConfirmAfterDays = ob.ConfirmAfterDays;

                if ($scope.tempList[i].ApprovalStatus === 'Extend') {
                    $scope.tempList[i].FDOC = ob.FDOC;
                }
                //break;
            }//if(Id)
        }//for
    };

    $scope.clearDays = function (ob, Id, index) {
        if (ob.ApprovalStatus === 'Confirm') {
            ob.ConfirmAfterDays = '';
            ob.FDOC = '';
        }
        if (ob.ApprovalStatus === 'Extend') {
            ob.NewDOC = '';
        }

        for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
            if (Id === $scope.tempList[i].EmployeeId) {
                $scope.tempList[i].ApprovalStatus = $scope.recruitmentSelections[index].ApprovalStatus;

                if ($scope.recruitmentSelections[index].ApprovalStatus === 'Confirm') {
                    $scope.tempList[i].ConfirmAfterDays = '';
                    $scope.tempList[i].FDOC = '';
                }
            }
        }
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw fieldName + ' is required...';
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

                    if (!ob.Active) {
                        throw 'Please select the employee [' + ob.EmployeeCode + ']';
                    }
                    if (ob.ApprovalStatus === 'Confirm') {
                        //if (baseService.isUndefinedOrNull(ob.ApprovalStatus) == true) {
                        //    throw 'Confirmation Date is required for Employee [' + ob.EmployeeCode + ']';
                        //}
                        CheckField(ob.NewDOC, 'Confirmation Date for employee [' + ob.EmployeeCode + ']');
                    }
                    if (ob.ApprovalStatus === 'Extend') {
                        if (baseService.isUndefinedOrNull(ob.ConfirmAfterDays) === true) {
                            throw 'Extended Days is required for Employee [' + ob.EmployeeCode + ']';
                        }
                    }
                    if (ob.ApprovalStatus === 'Extend') {
                        if (ob.ConfirmAfterDays === 0) {
                            throw 'Zero value in Extended days is not allowed for Employee [' + ob.EmployeeCode + ']';
                        }
                        if (ob.ConfirmAfterDays < 0) {
                            throw 'Negative value in Extended days is not allowed for Employee [' + ob.EmployeeCode + ']';
                        }
                    }

                    //var today = $filter('date')(new Date(), 'dd-MMM-yy');
                    //var ndoc = $filter('date')(ob.NewDOC, 'dd-MMM-yy');

                    if (ob.ApprovalStatus === 'Confirm') {
                        if (ob.IsPastDOCAllowed === false) {
                            var today = new Date();
                            var ndoc = new Date(ob.NewDOC);
                            if (ndoc < today) {
                                throw 'Past Confirmation date is not allowed for employee [' + ob.EmployeeCode + ']';
                            }
                        }
                        else {
                            var ndoc1 = new Date(ob.NewDOC);
                            var d = new Date();
                            d.setDate(d.getDate() - ob.pastDOCdaysAllowed);
                            var d1 = $filter('date')(d, 'dd-MMM-yy');
                            var d2 = new Date(d1);
                            if (d2 > ndoc1) {
                                //throw "Past Confirmation date before " + ob.pastDOCdaysAllowed + " Days is not allowed for employee [" + ob.EmployeeCode + "]";
                                throw "Employee [" + ob.EmployeeCode + "] cann't be  Confirmed " + ob.pastDOCdaysAllowed + " days ago.";
                            }
                        }
                    }

                    var td = new Date();
                    var futureday = new Date(ob.NewDOC);

                    if (ob.ApprovalStatus === 'Confirm') {
                        if (futureday > td) {
                            throw 'Future Confirmation date is not allowed for employee [' + ob.EmployeeCode + ']';
                        }
                    }

                    if (baseService.isUndefinedOrNull(ob.Remarks) === true) {
                        throw 'Remarks is required for Employee [' + ob.EmployeeCode + ']';
                    }

                    if (baseService.isUndefinedOrNull(ob.ApprovalStatus) === true) {
                        throw 'Select Confirmation Status for Employee [' + ob.EmployeeCode + ']';
                    }
                    var doj = new Date(ob.DOJ);
                    var fdoc = new Date(ob.FDOC);
                    if (doj > fdoc) {
                        throw 'Date of Confirmation cannot be less then date of join for Employee [' + ob.EmployeeCode + ']';
                    }
                }//active
                else {
                    count++;
                    var ob = list[i];
                    if (ob.ApprovalStatus) {
                        throw 'Please select the employee [' + ob.EmployeeCode + ']';
                    }
                }
            }//for
            if (count === 0) {
                throw 'Please select an employee';
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.Clear = function () {
        ClearOb($scope.EmployeeProbationalPeriod);
    };

    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }

    $scope.showEntity = function () {
        $http.get('employees/employeeprobationalperiod/getentitybyemployee')
            .then(function (response) {
                $scope.entityList = response.data;
            });
        angular.element(document.querySelector('#entityPopUp')).modal('show');
    };

    $scope.EntityList = function () {
        $http.get('employees/employeeprobationalperiod/getEntity')
            .then(function successCallback(response) {
                if (!baseService.isUndefinedOrNull(response.data.Message)) {
                    $scope.message = response.data.Message;
                }
                else {
                    $scope.message = response.data;
                }
            }
            ), function errorCallBack(response) {
                showResult(response.Message, 'failure');
            };
    };

    $scope.EntityList();

    $scope.getDOCColor = function (item) {
        var confirmDay = new Date(item.DOC);
        //var confirmDay = $filter('dateFiltering')(new Date(item.DOC));
        var toDay = new Date();
        //var toDay = $filter('dateFiltering')(new Date());
        if ($filter('dateFiltering')(confirmDay, 'dd-MMM-yyyy') === $filter('dateFiltering')(toDay, 'dd-MMM-yyyy')) {
            return 'current';
        } else if (confirmDay < toDay) {
            return 'olds';
        } else if (confirmDay > toDay) {
            return 'pending';
        }
    };

    $scope.getRemarkColor = function (item) {
        var remark = item.Remarks;
        if (remark === null || remark === '') {
            return 'empty';
        } else {
            return 'filled';
        }
    };

    // #region setTab
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion
}