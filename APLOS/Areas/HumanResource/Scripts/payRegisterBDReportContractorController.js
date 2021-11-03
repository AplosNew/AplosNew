'use strict';
payRegisterBDReportContractorController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function payRegisterBDReportContractorController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/PayRegisterBDReport/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };

    $scope.contractorList = [];
    $scope.contractorId = null;

    $scope.signatoryList = [];
    $scope.paymentDate = $filter('dateFiltering')(Date.now());
    $scope.printDate = $filter('dateFiltering')(Date.now());
    $scope.languageId = null;
    $scope.paymentMode = null;
    $scope.cboSalaryProcessIdList = {};

    $scope.isActive = true;
    $scope.isSeperated = false;
    $scope.isMaternity = false;

    $scope.isManualFilter = false;

    var sqlInStatement = "";
    $scope.reportStatus = {
        status: "dayStatus"
    };

    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.withStructure = true;
    $scope.paperSize = 'Legal';
    $scope.withAttendance = true;
    $scope.sheetType = false;


    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.empGrid = false;
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });
    $scope.paperSizeList = [
        {
            Value: 'Legal',
            Text: 'Legal'
        },
        {
            Value: 'A4',
            Text: 'A4'
        }];

    $scope.monthList = [
        {
            Value: 1,
            Text: 'January'
        },
        {
            Value: 2,
            Text: 'February'
        },
        {
            Value: 3,
            Text: 'March'
        },
        {
            Value: 4,
            Text: 'April'
        },
        {
            Value: 5,
            Text: 'May'
        },
        {
            Value: 6,
            Text: 'June'
        },
        {
            Value: 7,
            Text: 'July'
        },
        {
            Value: 8,
            Text: 'August'
        },
        {
            Value: 9,
            Text: 'September'
        },
        {
            Value: 10,
            Text: 'October'
        },
        {
            Value: 11,
            Text: 'November'
        },
        {
            Value: 12,
            Text: 'December'
        }
    ];

    $scope.year = new Date().getFullYear().toString();
    $scope.month = new Date().getMonth().toString();


    $scope.yearList = [];
    cboService.getCboLeaveYear(function (result) {
        $scope.yearList = result;
    });


    $scope.getSalaryProcessIdList = function (args) {
        $scope.isCompletedMonth = 1;

        var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");


        $scope.month = DropDownListMonth.getSelectedValue();
        $scope.year = DropDownListYear.getSelectedValue();
        if (angular.isUndefinedOrNull($scope.year)) {
            ShowResult("Select Year", 'failure');
        }
        else {
            cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
                $scope.cboSalaryProcessIdList = result;
            });
        }
    };

    $scope.empGridMain = false;

    $scope.empGridShow = function (args) {
        ShowResult('Press the Go Button  After Year/Month Change.', 'success');

        $scope.empGridMain = false;
    };


    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();


    $scope.GetPlantWiseSalaryRegisterSortingParameters = function () {
        //$scope.searchbyonRoleEmpList = [];
        //var parameters = { 'salaryId': $scope.salaryProcessId, 'payRollGrup': $scope.payGroupId, 'paymentMode': $scope.paymentMode, 'empCatg': $scope.employeeCategoryId, 'monthNo': $scope.month, 'yearNo': $scope.year };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'humanresource/PayRegisterBDReport/GetPlantWiseSalaryRegisterSortingParameters',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.plantSalaryRegisterSortingParamList = response.data;
            }
            //angular.element(document.querySelector('#empInfo')).modal('show');
        });


    };

    $scope.exportgriddataUrl = 'GridReports/ExcelExport';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.downloadgriddataPDFUrl = 'GridReports/DownloadPdf';

    $scope.docGrouping = "";
    $scope.onlyEarning = false;
    $scope.GetPayRegisterReport = function (reportType) {

        try {
            $scope.withStructure = "";
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
            if ($scope.sheetType === true) {
                $scope.withStructure = 'structured';
                $scope.onlyEarning = false;
            }
            if ($scope.withAttendance === true) {
                $scope.onlyEarning = false;
            }
            if ($scope.sheetType === false && $scope.withAttendance === false) {
                $scope.onlyEarning = true;
            }



            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");

            var filteredRecords = gridObj.getFilteredRecords();
            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    parameters = [];
                    parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (parameters.length === 0) {
                parameters.push({ "Key": "", "Value": "" });
            }
            if (new Date($scope.paymentDate) < new Date($scope.printDate)) {
                throw "Print date can not be greater then Payment date";
            }
            if (new Date($scope.printDate) > new Date(Date.now())) {
                throw "Print date can not be Less then Current date";
            }
            if (baseService.isUndefinedOrNull($scope.contractorId)) {
                throw "Please Select Contractor.";
            }
            $http({
                method: 'POST',
                url: 'humanresource/PayRegisterBDReport/GetPayRegisterReportContractor',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.salaryProcessId,
                    'paymentDate': $scope.paymentDate,
                    'languageId': $scope.languageId,
                    'withStructure': $scope.withStructure,
                    'groupBy': $scope.groupBy,
                    'parameters': parameters,
                    'sheetBasedOn': $scope.withStructure,
                    'withAttendance': $scope.withAttendance,
                    'paperSize': $scope.paperSize,
                    'reportType': reportType,
                    'printDate': $scope.printDate,
                    'docGrouping': $scope.docGrouping,
                    'isActive': $scope.isActive,
                    'isSeperated': $scope.isSeperated,
                    'isMaternity': $scope.isMaternity,
                    'onlyEarning': $scope.onlyEarning,
                    'ContracotrId': $scope.contractorId

                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (reportType === 'EXCEL') {
                        $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    }
                    if (reportType === 'PDF') {
                        $rootScope.report($scope.downloadgriddataPDFUrl + "?FileName=" + response.data.FileName);
                    }

                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.PayRegisterReportConfigList = [];
    $scope.PayRegisterReportConfigListDefault = [];

    $scope.GetPayRegisterReportConfigList = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPayRegisterReportConfigList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            // if (response.data.length > 0) {
            $scope.PayRegisterReportConfigList = response.data;

        });
    }; 
    //$scope.refreshOnClose = function () {
    //    $scope.PayRegisterReportConfigList = $scope.PayRegisterReportConfigListDefault;

    //};

    $scope.GetPayRegisterReportConfigList();

    $scope.GetPayRegisterReportConfigListModalPOPUP = function () {
        $scope.GetPayRegisterReportConfigList();
        angular.element(document.querySelector('#SalaryRegisterConfig')).modal('show');

    };

    $scope.PayRegisterRowPerPageList = [];
    $scope.GetPayRegisterRowPerPage = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPayRegisterRowPerPage",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            // if (response.data.length > 0) {
            $scope.PayRegisterRowPerPageList = response.data;
            // $scope.PayRegisterReportConfigListDefault = response.data;
            // angular.element(document.querySelector('#SalaryRegisterConfig')).modal('show');
            // }
        });
    }; 
    $scope.GetPayRegisterRowPerPage();
    $scope.GetPayRegisterRowPerPageModalPOPUP = function () {
        $scope.GetPayRegisterRowPerPage();
        angular.element(document.querySelector('#payRegisterRowPerPage')).modal('show');

    };

    $scope.paySlipPdf = "";

    $scope.GetPaySlipRDLC = function () {

        try {
            $scope.withStructure = "";
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }


            var parameters = [];
            var gridObj = $("#empInfoGrid").ejGrid("instance");
            var filteredRecords = gridObj.getFilteredRecords();
            if ($scope.isManualFilter == true) {
                if (filteredRecords.length == 0) {
                    filteredRecords = $scope.EmployeeListTemp;

                }
            }
            if (angular.isUndefinedOrNull(filteredRecords) === false) {
                if (filteredRecords.length > 0) {
                    parameters = [];
                    parameters.push({ "Key": "EmpSystemId", "Value": getString(filteredRecords, "EmpSystemId") });
                }
            }
            if (parameters.length === 0) {
                parameters.push({ "Key": "", "Value": "" });
            }


            $http({
                method: 'POST',
                url: 'humanresource/PayRegisterBDReport/GetPaySlip',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'languageId': $scope.languageId,
                    'parameters': parameters
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$rootScope.report(response.data.FileName);
                    //$scope.paySlipPdf = response.data.FileName;

                    //location.href = response.data.FileName;
                    //location.target = "_blank";
                    window.open(response.data.FileName, '_blank');

                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetPayRegisterBanglaBackup = function () {
        try {

            $scope.withStructure = "";
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }

            if ($scope.sheetType === true) {
                $scope.withStructure = 'structured';
            }
            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&sqlInStatement=' + sqlInStatement + '&withStructure=' + $scope.withStructure;
            $rootScope.report("humanresource/PayRegisterBDReport/GetPayRegisterReportBangla?" + $scope.parameters);
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.SortingParametersList = [
        {
            parameter: "EmployeeCode",
            type: "Sorting"
        },
        {
            parameter: "EmployeeName",
            type: "Sorting"
        },
        {
            parameter: "Entity",
            type: "Sorting"
        },
        {
            parameter: "Division",
            type: "Sorting"
        },
        {
            parameter: "SubDivision",
            type: "Sorting"
        },
        {
            parameter: "Department",
            type: "Sorting"
        },
        {
            parameter: "Section",
            type: "Sorting"
        },
        {
            parameter: "SubSection",
            type: "Sorting"
        },
        {
            parameter: "Unit",
            type: "Sorting"
        },
        {
            parameter: "Designation",
            type: "Sorting"
        },
        {
            parameter: "EmployeeCategory",
            type: "Sorting"
        },
        {
            parameter: "Line",
            type: "Sorting"
        }
    ];
    $scope.GroupingParametersList = [
        {
            parameter: "Department",
            type: "Sorting"
        },
        {
            parameter: "Section",
            type: "Sorting"
        },
        {
            parameter: "SubSection",
            type: "Sorting"
        },
        {
            parameter: "Designation",
            type: "Sorting"
        },
        {
            parameter: "EmployeeCategory",
            type: "Sorting"
        }
    ];
    $scope.sortingParam = {
        Parameter: null,
        Sequence: null
    };
    var count = 0;
    $scope.tempSortingList = [];
    $scope.indexContact = -1;
    $scope.addSortingList = function () {


        if ($scope.sortingParam !== {} && $scope.sortingParam !== 'undefined') {
            if ($scope.indexContact !== -1) {

                if ($scope.plantSalaryRegisterSortingParamList.some(e => e.Parameter === $scope.sortingParam.Parameter) === true) {

                    ShowResult($scope.sortingParam.Parameter + ' is already in the list.', 'failure');
                }
                else {
                    count++;
                    $scope.sortingParam.Sequence = count;
                    $scope.plantSalaryRegisterSortingParamList[$scope.indexContact] = $scope.sortingParam;
                }
            }
            else {
                if ($scope.plantSalaryRegisterSortingParamList.some(e => e.Parameter === $scope.sortingParam.Parameter) === true) {
                    ShowResult($scope.sortingParam.Parameter + ' is already in the list.', 'failure');
                    //|| $scope.plantSalaryRegisterSortingParamList.length === 0
                }
                else {
                    count++;
                    $scope.sortingParam.Sequence = count;
                    $scope.plantSalaryRegisterSortingParamList.push($scope.sortingParam);

                }
            }

            $scope.indexContact = -1;
            $scope.sortingParam = {};
        }
        console.log("First List", $scope.plantSalaryRegisterSortingParamList);

    };

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].data === id) {
                return true;
            }
        }
        return false;
    }
    $scope.EmployeeList = [];
    $scope.EmployeeListDefault = [];
    $scope.EmployeeListTemp = [];

    $scope.GetEmployeeInformationContractor = function () {

        var DropDownListMonth = $("#ddlMonthList").data("ejDropDownList");
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");


        $scope.month = DropDownListMonth.getSelectedValue();
        $scope.year = DropDownListYear.getSelectedValue();

        if (angular.isUndefinedOrNull($scope.year) === false && angular.isUndefinedOrNull($scope.month) === false) {
            var DropDownListSalaryProcess = $("#ddlSalaryProcessId").data("ejDropDownList");
            //$scope.salaryProcessId = DropDownListSalaryProcess.getSelectedValue();
        }

        var monthName = $scope.monthList.filter(function (mnth) {
            return mnth.Value == $scope.month;
        });
        $scope.effectiveDate = daysInMonth($scope.month, $scope.year) + '-' + monthName[0].Text + '-' + $scope.year;

        $scope.searchbyonRoleEmpList = [];
        var parameters = {
            'salaryProcessId': $scope.salaryProcessId, 'effectiveDate': $scope.effectiveDate, 'monthNo': $scope.month, 'yearNo': $scope.year, 'isActive': $scope.isActive, 'isSeperated': $scope.isSeperated, 'isMaternity': $scope.isMaternity
        };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'humanresource/PayRegisterBDReport/GetEmployeeInformationContractor',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.empGridMain = true;

                for (var i = 0; i < response.data.length; i++) {
                    if (angular.isUndefinedOrNull(response.data[i].DOJ) == false) {
                        response.data[i].DOJ = new Date(response.data[i].DOJ);

                    }
                    if (angular.isUndefinedOrNull(response.data[i].DOS) == false) {
                        response.data[i].DOS = new Date(response.data[i].DOS);

                    }
                }
                $scope.EmployeeListDefault = response.data.filter(d => d.isSelect == true);
                $scope.EmployeeList = $scope.EmployeeListDefault;
                $scope.EmployeeListTemp = $scope.EmployeeListDefault;
                $scope.contractorList = [...new Map(response.data.map(item =>
                    [item['ContractorId'], item])).values()];
                //$scope.contractorList = response.data.filter(d => d.isSelect == true);

            }
            else {
                $scope.empGrid = false;
                $scope.empGridMain = false;

                ShowResult("No Data Found", 'failure');

            }
        });


    };
    $scope.contractorChange = function () {
        $scope.EmployeeListTemp = $scope.EmployeeListDefault.filter(d => d.ContractorId == $scope.contractorId);
        $scope.EmployeeList = $scope.EmployeeListDefault.filter(d => d.ContractorId == $scope.contractorId);

        //$scope.EmployeeListDefault = response.data.filter(d => d.isSelect == true);
    }

    $window.onresize = function (event) {
        $scope.actionCompleteSelected();
    };
    $scope.actionCompleteSelected = function (args) {
        try {
            var gridObj = $("#empInfoGrid").ejGrid("instance");

            if (args.requestType === "refresh") {
                var scrollerwidth = $("#empInfo").width();//Obtain the width of the container
                $("#Grid").children('.e-grid.e-headercell').css('height', '100px');
                gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 400 } });//pass the obtainer width and height to gridmodel options
                gridObj.windowonresize();
            }

            if (args.requestType === "filtering") {
                var filtereddata = gridObj.getFilteredRecords();
                var uniqueEmpSystemId = removeDuplicates(filtereddata, 'EmpSystemId');
                var wcEmpCode = "";
                if (uniqueEmpSystemId.length > 0) {
                    wcEmpCode = "IN(";
                    wcEmpCode += Array.prototype.map.call(uniqueEmpSystemId, function (item) { return "'" + item.EmpSystemId + "'"; }).join(",") + ")";
                }
                sqlInStatement = wcEmpCode;
            }


        } catch (e) {
            //$scope.ShowResultCustom(e, 'failure');
        }
    };
    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.getSortingSequence = function () {
        try {
            $scope.getPlantEmployeeSortingSequence();
            angular.element(document.querySelector('#plantSalaryRegisterSortingSequencePopUp')).modal('show');

        } catch (e) {

        }
    }
    //---------------Sorting and Grouping -----------------------//

    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.plantEmployeeSortingSequenceList = [];
    $scope.plantEmployeeSortingSequenceSelectedList = [];
    $scope.sortingCategoryList = [];
    $scope.plantList = [];
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.UpdatePayRegisterReportConfigListURL = $scope.path + 'UpdatePayRegisterReportConfigList';
    $scope.UpdatePayRegisterRowPerPageListURL = $scope.path + 'UpdatePayRegisterRowPerPageList'; 
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.plantEmployeeSortingSequence = {
        Id: null,
        PlantId: null,
        Parameters: null,
        Sequence: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.SortingParametersSelectedList = [];
    //PlantWiseSalaryRegisterSortingParameters
    $scope.plantSalaryRegisterSortingParamList = [];
    $scope.EmployeeSortingSequenceNew = Object.assign({}, $scope.EmployeeSortingSequence);
    $scope.getPlantEmployeeSortingSequence = function () {
        var url = $scope.path + 'GetPlantWiseSalaryRegisterSortingParameters';
        $http({
            method: 'POST',
            url: url
        }).then(function successCallback(response) {
            $scope.plantSalaryRegisterSortingParamList = response.data;
        });
    };

    function checkExist(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SalaryHeadId === id) {
                return true;
            }
        }
        return false;
    }
    $scope.sortingParametersCloseListPopUp = function () {
        var slected = false;
        var tempsortingParameters = $scope.SortingParametersList;
        for (var i = 0; i < tempSalaryHead.length; i++) {
            if (tempSalaryHead[i].Flag) {
                if (checkExist($scope.SortingParametersList, tempsortingParameters[i].parameter) === false) {
                    $scope.SortingParametersSelectedList.push(
                        {
                            Id: null,
                            SalaryHeadId: tempSalaryHead[i].SalaryHeadID,
                            PlantId: $scope.plantSalaryHeadSequenceNew.PlantId,
                            SalaryHead: tempSalaryHead[i].SalaryHead,
                            Description: tempSalaryHead[i].Description,
                            HeadCategory: tempSalaryHead[i].HeadCategory,
                            HeadType: tempSalaryHead[i].HeadType,
                            Sequence: null,
                            Flag: tempSalaryHead[i].Flag
                        }
                    );
                    angular.element(document.querySelector('#plantSalaryRegisterSortingSequencePopUp')).modal('hide');
                } else {
                    return ShowResult("", 'failure', 'plantSalaryHeadSequencePopUp');
                    break;
                }

            } else {
                return ShowResult("", 'failure', 'plantSalaryRegisterSortingSequencePopUp');
                break;
            }
        }
        slected = true;
    }; //flag

    $scope.valuePassInEarningDelModal = function (data, index) {
        $scope.salaryHeadRuleId = data.Parameter;
        $scope.salaryHeadRuleIndex = index;
        if (baseService.isUndefinedOrNull($scope.salaryHeadRuleId))
            $scope.message_confirmation = 'Are you sure want to delete this data....';
        else
            $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Parameter + ' ]';
        //angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
        $scope.DeleteRow();
    };
    $scope.DeleteRow = function () {
        var tempData = $scope.plantSalaryRegisterSortingParamList;
        for (var i = 0; i < tempData.length; i++) {
            if (tempData[i].Parameter === $scope.salaryHeadRuleId) {
                $scope.plantSalaryRegisterSortingParamList.splice(i, 1);
            }
        }
        $scope.salaryHeadRuleId = null;
        $scope.salaryHeadRuleIndex = -1;
        tempData = [];
    };
    //

    //*****Short**/
    var move = function (origin, destination, list) {
        var temp = $scope[list][destination];
        $scope[list][destination] = $scope[list][origin];
        $scope[list][origin] = temp;
    };
    $scope.moveUp = function (index, list) {
        move(index, index - 1, list);
    };
    $scope.moveDown = function (index, list) {
        move(index, index + 1, list);
    };
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.plantSalaryHeadSequence = $scope.plantSalaryHeadSequences[$scope.index];
        $scope.plantSalaryHeadSequenceNew = Object.assign({}, $scope.plantSalaryHeadSequence);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.hasDuplicateSeq = function (data, type) {
        for (var i = 0; i < data.length; i++) {
            if (data[i].HeadType === type) {
                for (var x = i + 1; x < data.length; x++) {
                    if (data[i].Sequence === data[x].Sequence && data[x].HeadType === type) {
                        return true;
                    }
                }//childFor
            }
        }//ParentFor
        return false;
    };

    $scope.plantSalaryHeadSequenceSelectedListForSave = [];
    function combineBothList(list) {
        angular.forEach(list, function (item, key) {
            item.Sequence = key + 1;
            $scope.plantSalaryHeadSequenceSelectedListForSave.push(item);

        });
    }
    $scope.Save = function () {

        try {
            $scope.plantSalaryHeadSequenceSelectedListForSave = [];
            combineBothList($scope.plantSalaryRegisterSortingParamList);
            //combineBothList($scope.deductionPlantSalaryHeadSequenceSelectedList);
            angular.forEach($scope.plantSalaryRegisterSortingParamList, function (item) {
                if (baseService.isUndefinedOrNull(item.Sequence)) {
                    throw "Secquence require";
                }
            });
            $scope.$broadcast('show-errors-check-validity');
            //if ($scope.plantSalaryRegisterSortingParamList.$valid) {
            //    angular.copy($scope.plantSalaryHeadSequenceNew, $scope.plantSalaryHeadSequence);
            //    if (checkIsListExistOnErning($scope.earningPlantSalaryHeadSequenceSelectedList) === false) {
            //        throw "Add at least one earning head sequence";
            //    }
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'PlantWiseSalaryRegisterSortingParameters': $scope.plantSalaryHeadSequenceSelectedListForSave },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetPlantWiseSalaryRegisterSortingParameters();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            //}
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.UpdatePayRegisterReportConfigList = function () {

        try {
            $http({
                method: 'POST',
                url: $scope.UpdatePayRegisterReportConfigListURL,
                data: { 'data': $scope.PayRegisterReportConfigList },
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
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.UpdatePayRegisterRowPerPageList = function () {

        try {
            $http({
                method: 'POST',
                url: $scope.UpdatePayRegisterRowPerPageListURL,
                data: { 'data': $scope.PayRegisterRowPerPageList },
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
        }
        catch (e) {
            ShowResult(e, 'failure');
        }
    };
    // #region SetTab 

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = "Save";
        $scope.plantSalaryHeadSequence = { PlantId: $scope.plantSalaryHeadSequence.PlantId, CompanyId: $scope.plantSalaryHeadSequence.CompanyId };
        $scope.plantSalaryHeadSequenceNew = { PlantId: $scope.plantSalaryHeadSequenceNew.PlantId, CompanyId: $scope.plantSalaryHeadSequenceNew.CompanyId };
        $scope.plantSalaryHeadSequenceNew.Id = null;
        $scope.plantSalaryHeadSequenceNew.Active = true;
        $scope.tempList = [];
    }
    $scope.GetSalaryRegisterSortingParameters = function () {
        angular.element(document.querySelector('#plantSalaryRegisterSortingPopUp')).modal('show');
    };

    $scope.GetSalaryRegisterGroupingParameters = function () {
        angular.element(document.querySelector('#plantSalaryRegisterGroupingPopUp')).modal('show');
    };
    var getString = function (data, column) {
        var string = "''";
        var collection = [];
        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) === false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }

        return string;
    };

    function daysInMonth(month, year) {
        return new Date(year, month, 0).getDate();
    }

    $scope.SelectDefaultValue = function (args) {
        var x = new Date();
        x.setDate(10);
        x.setMonth(x.getMonth() - 1);

        for (var i = 0; i < $scope.yearList.length; i++) {
            if ($scope.yearList[i].Text === x.getFullYear().toString()) {
                $scope.year = $scope.yearList[i].Text;
                $scope.month = (x.getMonth() + 1).toString();
                continue;
            }
        }

        //$scope.year = "2018";
        var DropDownListYear = $("#ddlYearList").data("ejDropDownList");
        DropDownListYear.selectItemByText($scope.year);

    };
    //---------------------------------------//
    //------Multiple Selection(Excel)-------//
    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {

                    $scope.EmployeeList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.EmployeeList[i].isSelect = true;
                            $scope.EmployeeList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    $scope.EmployeeList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.EmployeeList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.EmployeeList[i].Id == filtered[j].Id)
                            $scope.EmployeeList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.EmployeeList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    };
    $scope.saveemployeedata = function () {
        $scope.EmployeeListTemp = [];
        var row = $filter('filter')($scope.EmployeeList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.EmployeeListTemp = row;
            $scope.isManualFilter = true;
        }
        $scope.Back();
    };
    $scope.showEmployeeFilterScreen = function () {
        try {

            var gridObj = $("#Gridemployee").data("ejGrid");
            gridObj.clearFiltering();
            angular.element(document.querySelector('#empfilterPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.EmployeeListTemp = $scope.EmployeeList;
    };
    $scope.Back = function () {
        angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    };

    //------End Multiple Selection(Excel)-------//

    $scope.saveSignatoryConfigUrl = $scope.path + 'saveSignatoryConfig';

    $scope.sigId = null;
    $scope.cindex = null;
    $scope.add = function () {
        if ($scope.signatoryList.length < 5) {
            $scope.signatoryList.push({
                Sequence: null
                , FieldName: null
            });
        }
        else {
            ShowResult("Signatory can not be more then 5!!!", 'failure');
        }


    };

    $scope.saveSignatoryConfig = function () {

        $http({
            method: 'POST',
            url: $scope.saveSignatoryConfigUrl,
            data: { 'PayRegisterSignatoryField': $scope.signatoryList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                ClearFields(response.data.Sequence);
                $scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.getPayRegisterSignatoryFieldUrl = $scope.path + 'GetPayRegisterSignatoryFieldById';
    $scope.actionDeleteUrl = $scope.path + 'DeletePayRegisterSignatoryFieldById/';

    function getPayRegisterSignatoryFieldById(id) {
        $http.get($scope.getPayRegisterSignatoryFieldUrl + id)
            .then(function (response) {
                $scope.signatoryList = response.data;
            });
    }


    $scope.showSignatoryConfig = function () {
        try {
            $scope.getData();


            angular.element(document.querySelector('#signatoryPopUp')).modal('show');


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.removePopup = function (data, index) {
        $scope.sigId = data.Id;
        $scope.cindex = index;
        $scope.message = 'Are you sure want to permanent delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.sigId)) {
            $http({
                method: 'POST'
                , url: $scope.actionDeleteUrl + $scope.sigId
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.signatoryList.splice($scope.cindex, 1);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        }
        else
            $scope.signatoryList.splice($scope.cindex, 1);
        $scope.cindex = -1;
    };

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetPayRegisterSignatoryFieldByList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.signatoryList = response.data;
        });
    };
    $scope.getData();
    //------------------------------------//
    $scope.empNo = 6;

    $scope.empNoChange = function () {
        if ($scope.sheetType === false && $scope.withAttendance === false) {
            $scope.empNo = 9;
        }
        else if ($scope.sheetType === true && $scope.withAttendance === false) {
            $scope.empNo = 9;
        }
        else {
            $scope.empNo = 6;
        }
    };

}