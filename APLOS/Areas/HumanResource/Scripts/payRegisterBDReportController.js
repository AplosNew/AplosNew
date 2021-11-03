'use strict';
payRegisterBDReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$window'];
function payRegisterBDReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $window) {

    $scope.path = 'humanresource/PayRegisterBDReport/';
    $scope.employeeCategoryId = null;
    $scope.dailyComplianceReport = {
        WorkDate: null
    };
    $scope.paymentDate = null;
    $scope.languageId = null;
    $scope.paymentMode = null;
    var sqlInStatement = "";
    $scope.reportStatus = {
        status: "dayStatus"
    };

    $scope.hrStatus = {
        pstatus: 'Default'
    };
    $scope.withStructure = null;
    $scope.sheetType = false;


    $scope.month = "";
    $scope.year = "";
    $scope.isCompletedMonth = null;
    $scope.salaryProcessId = null;

    $scope.unitId = null;
    $scope.departmentId = null;
    $scope.divisionId = null;
    $scope.sectionId = null;
    $scope.subSenctionId = null;
    $scope.payGroupId = null;
    $scope.empGrid = false;
    $scope.localLanguageList = [];
    cboService.getLanguageIdCbo(function (result) {
        $scope.localLanguageList = result;
    });

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
    $scope.unitList = [];
    cboService.getCboUnit(function (result) {
        $scope.unitList = result;
    });

    $scope.divisionList = [];
    cboService.getCboDivisionByCompanyGroup(null, function (result) {
        $scope.divisionList = result;
    });

    $scope.departmentList = [];
    cboService.getCboDepartmentByCompanyGroup(null, function (result) {
        $scope.departmentList = result;
    });

    $scope.subSectionList = [];
    cboService.getCboSubSectionByCompanyGroup(null, function (result) {
        $scope.subSectionList = result;
    });

    $scope.employeeCategoryList = [];
    cboService.getCboEmployeeCategoryGroupByCompanyGroup(null, function (result) {
        $scope.employeeCategoryList = result;
    });

    $scope.designationGroupList = [];
    cboService.getCboDesignationGroupByCompanyGroup(null, function (result) {
        $scope.designationGroupList = result;
    });

    $scope.sectionList = [];
    cboService.getCboSectionByCompanyGroup(null, function (result) {
        $scope.sectionList = result;
    });

    $scope.lineList = [];
    cboService.getCboLineByCompany(null, function (result) {
        $scope.lineList = result;
    });

    $scope.designationList = [];
    cboService.getCboDesignationByCompanyGroup(null, function (result) {
        $scope.designationList = result;
    });


    $scope.payGroupList = [];
    cboService.getPayGroupCbo(function (result) {
        $scope.payGroupList = result;
    });


    $scope.getSalaryProcessIdList = function () {
        $scope.isCompletedMonth = 1;
        cboService.getSalaryProcessIdCboByYearMonth($scope.month, $scope.year, $scope.isCompletedMonth, function (result) {
            $scope.cboSalaryProcessIdList = result;
        });
    };
    $scope.getSalaryProcessIdList();
    $scope.selectedPaymentMode = $("#paymentMode option:selected").text();
    $scope.selectedEmployeeCategory = $("#employeeCategoryId option:selected").text();

    $scope.GetPayRegister = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }
            if (baseService.isUndefinedOrNull($scope.payGroupId)) {
                throw "Select Pay Roll Group.";
            }
            $scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&paymentDate=' + $scope.paymentDate;
            location.href = 'humanresource/PayRegisterBDReport/GetPayRegisterReport?' + $scope.parameters;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

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

    $scope.GetPayRegisterBangla = function () {

        try {
            $scope.withStructure = "";
            if (baseService.isUndefinedOrNull($scope.month)) {
                throw "Select Month.";
            }
            if (baseService.isUndefinedOrNull($scope.year)) {
                throw "Select Year.";
            }

            if (baseService.isUndefinedOrNull($scope.payGroupId)) {
                throw "Select PayGroup.";
            }
            if ($scope.sheetType === true) {
                $scope.withStructure = 'structured';
            }
            //$scope.parameters = 'month=' + $scope.month + '&year=' + $scope.year + '&salaryProcessId=' + $scope.salaryProcessId + '&divisionId=' + $scope.divisionId + '&unitId=' + $scope.unitId + '&sectionId=' + $scope.sectionId + '&subSectionId=' + $scope.subSectionId + '&departmentId=' + $scope.departmentId + '&payGroupId=' + $scope.payGroupId + '&employeeCategoryId=' + $scope.employeeCategoryId + '&paymentDate=' + $scope.paymentDate + '&paymentMode=' + $scope.paymentMode + '&languageId=' + $scope.languageId + '&sqlInStatement=' + sqlInStatement + '&withStructure=' + $scope.withStructure;
            //$rootScope.report("humanresource/PayRegisterBDReport/GetPayRegisterReportBangla?" + $scope.parameters);


            $http({
                method: 'POST',
                url: 'humanresource/PayRegisterBDReport/GetPayRegisterReportBangla',
                data: {
                    'month': $scope.month,
                    'year': $scope.year,
                    'salaryProcessId': $scope.salaryProcessId,
                    'divisionId': $scope.divisionId,
                    'unitId': $scope.unitId,
                    'sectionId': $scope.sectionId,
                    'subSectionId': $scope.subSectionId,
                    'departmentId': $scope.departmentId,
                    'payGroupId': $scope.payGroupId,
                    'employeeCategoryId': $scope.employeeCategoryId,
                    'paymentDate': $scope.paymentDate,
                    'paymentMode': $scope.paymentMode,
                    'languageId': $scope.languageId,
                    'sqlInStatement': sqlInStatement,
                    'withStructure': $scope.withStructure
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
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

            if (baseService.isUndefinedOrNull($scope.payGroupId)) {
                throw "Select PayGroup.";
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
    $scope.GetEmployeeInformation = function () {

        $scope.searchbyonRoleEmpList = [];
        var parameters = { 'salaryId': $scope.salaryProcessId, 'payRollGrup': $scope.payGroupId, 'paymentMode': $scope.paymentMode, 'empCatg': $scope.employeeCategoryId, 'monthNo': $scope.month, 'yearNo': $scope.year };
        $http({
            method: "POST",
            dataType: 'JSON',
            url: 'humanresource/PayRegisterBDReport/GetEmployeeInformation',
            data: parameters
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.empGrid = true;
                $scope.EmployeeList = response.data;
            }
            else {
                ShowResult("No Data Found", 'failure');
            }
        });


    };


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
        console.log("New List", $scope.plantSalaryRegisterSortingParamList);
        console.log($scope.plantSalaryHeadSequenceSelectedListForSave);
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
    }
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



    //---------------------------------------//

}