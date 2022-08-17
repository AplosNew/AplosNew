'use strict';
QuaityProcessBookingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function QuaityProcessBookingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Quaity Process Booking";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.ModelList = [];
    $scope.gradeList = [];
    $scope.path = 'Productions/QuaityProcessBooking/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.gradeList = [{ 'Value': 'A', 'Text': 'A' }, { 'Value': 'B', 'Text': 'B' }, { 'Value': 'C', 'Text': 'C' }, { 'Value': 'D', 'Text': 'D' }];

    $scope.productionSummary = {
        Id: null,
        ProcessId: null,
        QualityProcessId:null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),
        ProductionShiftId: null,
        LotNumber:null,
        BatchNo: null,
        SampleSize: null,
        ProductionGrade: null,
        Remarks:null,
        CheckedBy: null,
        CheckedByName: null
    };
    $scope.productionSummaryNew = Object.assign({}, $scope.productionSummary);

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.GetList = function () {
        $http({
            method: 'GET',
            url: 'Productions/QuaityProcessBooking/GetList'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        });
    }
    $scope.GetList();

    $scope.getprocessList = function () {
        $http({
            method: 'GET',
            url: "Processes/Process/GetProductionProcessList"
        }).then(function successCallback(response) {
            $scope.processList = response.data.Rows;
        });
    }
    $scope.getprocessList();

    $scope.qualityprocessList = [];
    $scope.getqualityprocessList = function () {
        $http({
            method: 'GET',
            url: "Productions/QuaityProcessBooking/GetQualityProcessCbo?ProcessId=" + $scope.productionSummaryNew.ProcessId
        }).then(function successCallback(response) {
            $scope.qualityprocessList = response.data;
        });
    }
    

    $scope.CheckValidLotNumber = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.productionSummaryNew.LotNumber)) {
                //if (/^[ A-Za-z0-9_@./#&+-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                if (/^[ A-Za-z0-9_./-]*$/.test($scope.productionSummaryNew.LotNumber)) {
                    ///
                } else {
                    throw "You have entered an invalid value for Lot Number.";
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.shiftList = [];
    cboService.GetProductionShiftCbo(function (result) {
        $scope.shiftList = result;
        if (baseService.arrayLength(result) === 1) {
            $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
        }
    });

    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] is required.";
            }

        } catch (ex) {
            throw ex;
        }
    }

    function ValidationDetail(master) {
        try {
            CheckField("Production Summary Id", master.Id);
            CheckField("Sales Order", master.SalesOrderId);
            CheckField("MaterialMaster", master.MaterialMasterId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
        } catch (ex) {
            throw ex;
        }
    }

    function ValidationPreMaster() {
        try {
            CheckField("Process", $scope.productionSummaryNew.ProcessId);
            CheckField("Quality Process", $scope.productionSummaryNew.QualityProcessId);
            CheckField("Production Date", $scope.productionSummaryNew.ProductionDate);
            CheckField("Shift", $scope.productionSummaryNew.ProductionShiftId);
        } catch (ex) {
            throw ex;
        }
    }

    $scope.IsGo = false;
    $scope.masterGo = function (isdisabled) {
        try {
            ValidationPreMaster();
            $scope.getProdBookedData();
            $scope.SetGo(isdisabled);
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };

    $scope.SetGo = function (isdisabled) {
        $scope.IsGo = isdisabled;
    };

    $scope.SetBack = function (isdisabled) {
        $scope.IsGo = isdisabled;
        $scope.ClearMasterPart();
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
    };

    $scope.ProdBookedDataList = [];
    $scope.getProdBookedData = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/QuaityProcessBooking/GetProductionBookingData?processId=' + $scope.productionSummaryNew.ProcessId + '&productionDate=' + $scope.productionSummaryNew.ProductionDate + '&ProductionShiftId=' + $scope.productionSummaryNew.ProductionShiftId)
                .then(function (response) {
                    $scope.ProdBookedDataList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    $scope.QualityProcessParameterList = [];
    $scope.GetQualityProcessParameterList = function () {
        try {
            $scope.ProdQtyCount = 0;
            $http.get('Productions/QuaityProcessBooking/GetQualityProcessParameterList?processId=' + $scope.productionSummaryNew.QualityProcessId, '&masterId=' + $scope.productionSummaryNew.Id)
                .then(function (response) {
                    $scope.QualityProcessParameterList = response.data;
                });
        } catch (ex) {
            ShowResult(ex, 'Info');
        }
    };


    $scope.refreshTemplateAdditionalInfo = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#PBGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ProdBookedDataList.length; i++) {
                $scope.ProdBookedDataList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].Flag = ChkOrUnchk;
            }
        }
        var gridObj = $("#PBGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.ProdBookedSaveList = [];
    function MakeProdBookedData() {
        $scope.ProdBookedSaveList = [];
        for (var i = 0; i < $scope.ProdBookedDataList.length; i++) {
            if ($scope.ProdBookedDataList[i].Flag == true) {
                var ob = {};
                ob.Id = null;
                ob.ProductionSummaryId = $scope.ProdBookedDataList[i].ProductionSummaryId;
                ob.QuaityProcessBookingId = null;
                $scope.ProdBookedSaveList.push(ob);
                ob = {};
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].AdditionalInfoId === id) {
                return true;
            }
        }
        return false;
    }

    // #region Employee Mentor

    $scope.employeeFilterList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'EmployeeCode, FirstName, MiddleName, LastName '
        , searchBy: 'EmployeeCode'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.employeeUrl = 'WorkCenters/workcentermaster/GetEmployeeListByPlant';
    $scope.employeeList = [];
    $scope.showEmployeeListPopUp = function (flag) {
        $scope.respOrMentor = flag;
         if ($scope.respOrMentor === 'CheckedBy') { $scope.popUpTitle = 'CheckedBy'; }
        baseService.setCurrentPage('employeeList');
        $scope.searchEmployeeByList = [];
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.plantId = $window.plantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
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

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

 

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.respOrMentor === 'CheckedBy') {
                $scope.productionSummaryNew.CheckedBy = employee.SystemId;
                $scope.productionSummaryNew.CheckedByName = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
    };

    $scope.ClearEmployee = function () {
        if ($scope.respOrMentor === 'CheckedBy') {
            $scope.productionSummaryNew.CheckedBy = null;
            $scope.productionSummaryNew.CheckedByName = null;
        }
    };


    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    // #endregion Employee Mentor

    $scope.Save = function () {
        try {
            CheckField($scope.productionSummaryNew.LotNumber, "LotNumber");
            CheckField($scope.productionSummaryNew.BatchNo, "Batch No");
            CheckField($scope.productionSummaryNew.SampleSize, "Sample Size");
            CheckField($scope.productionSummaryNew.ProductionGrade, "Grade");
            MakeProdBookedData();
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.productionSummaryNew, 'ProdBookedSaveList': $scope.ProdBookedSaveList, 'ParameterList': $scope.QualityProcessParameterList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                  
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.Action = "Save";
        $scope.productionSummary = {};
        $scope.productionSummaryNew = {};
        $scope.productionSummaryNew.Active = true;
        $scope.productionSummaryNew.ProductionDate = $filter("date")(Date.now(), 'dd-MMM-yyyy');
        $scope.SetBack(false);
        $scope.IsGo = false;
        $scope.ProductionSummaryDetail = [];
        $scope.LineGridList = [];
    }
}