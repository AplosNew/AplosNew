'use strict';
MainProcessPlanningController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window', '$compile'];
function MainProcessPlanningController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window, $compile) {
    $rootScope.title = "Main Process Planning";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.mainProcessPlannings = [];
    $scope.path = 'Productions/mainprocessplanning/';
    $scope.toDate = null;
    $scope.plantId = null;
    $scope.processId = null;
    $scope.validationMsg = null;
    $scope.processList = [];
    $http.get("processes/companyprocess/getcbo/")
        .then(function (response) {
            $scope.processList = [];
            $scope.processList = response.data;
        });
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.plantList = [];
    cboService.getCboPlantByCompany($window.companyId, function (result) {
        $scope.plantList = result;
    });
    $scope.tblShow = false;
    $scope.processBtn = true;
    $scope.EnableOrDisableProcessBtn = function () {
        if (!baseService.isUndefinedOrNull($scope.toDate))
            $scope.processBtn = false;
        else
            $scope.processBtn = true;
    }
    $scope.View = function () {
        try {
            if (new Date($scope.toDate) <= Date.now()) {
                throw ("Please select to date greater then today date....................!");
            }
            $scope.validationMsg = null;
            $scope.processBtn = true;
            $scope.processDataList = [];
            $scope.newDataList = [];
            $scope.colList = [];
            $scope.monthYearList = [];
            $scope.dayList = [];
            var distinctPrfList = [];
            if (baseService.isUndefinedOrNull($scope.plantId) && baseService.isUndefinedOrNull($scope.toDate))
                return ShowResult('Please select plant and date..............!', 'failure');
            $http({
                method: "GET",
                url: $scope.path + 'GetList',
                params: {
                    'plantId': $scope.plantId,
                    'toDate': $scope.toDate,
                    'companyId': $window.companyId,
                    'processId': $scope.processId,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.processBtn = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (response.data.length > 0) {
                        $scope.processDataList = response.data;
                        setStyle($scope.processDataList);
                        setMsg($scope.processDataList);
                        $scope.newDataList = newDataList($scope.processDataList, new Date(Date.now()), new Date($scope.toDate))
                        getColumn($scope.newDataList, $scope.colList);
                        distinctPrfList = getPrfFromList($scope.processDataList);
                        //setCellColor($scope.newDataList, $scope.colList, distinctPrfList);
                        $scope.monthYearList = diff(new Date(Date.now()), $scope.toDate, $scope.monthYearList);
                        for (var i = 0; i < $scope.monthYearList.length; i++) {
                            for (var c = 0; c < $scope.monthYearList[i].DateArr.length; c++) {
                                $scope.dayList.push({
                                    Id: i + '' + c,
                                    Day: $scope.monthYearList[i].DateArr[c].Day
                                });
                            }
                        }
                    }
                    else {
                        ShowResult('No data found...........!', 'failure');
                    }
                    $scope.processBtn = false;
                }
            }), function errorCallBack(response) {
                $scope.processBtn = false;
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.Process = function () {
        try {
            if (new Date($scope.toDate) <= Date.now()) {
                throw ("Please select to date greater then today date....................!");
            }
            $scope.validationMsg = null;
            $scope.processBtn = true;
            $scope.processDataList = [];
            $scope.newDataList = [];
            $scope.colList = [];
            $scope.monthYearList = [];
            $scope.dayList = [];
            var distinctPrfList = [];
            if (baseService.isUndefinedOrNull($scope.plantId) && baseService.isUndefinedOrNull($scope.toDate))
                return ShowResult('Please select plant and date..............!', 'failure');
            $http({
                method: "GET",
                url: $scope.path + 'Process',
                params: {
                    'plantId': $scope.plantId,
                    'toDate': $scope.toDate,
                    'companyId': $window.companyId,
                    'processId': $scope.processId,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    $scope.processBtn = false;
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    if (response.data.length > 0) {
                        $scope.processDataList = response.data;
                        setStyle($scope.processDataList);
                        setMsg($scope.processDataList);
                        $scope.newDataList = newDataList($scope.processDataList, new Date(Date.now()), new Date($scope.toDate))
                        getColumn($scope.newDataList, $scope.colList);
                        distinctPrfList = getPrfFromList($scope.processDataList);
                        //setCellColor($scope.newDataList, $scope.colList, distinctPrfList);
                        $scope.monthYearList = diff(new Date(Date.now()), $scope.toDate, $scope.monthYearList);
                        for (var i = 0; i < $scope.monthYearList.length; i++) {
                            for (var c = 0; c < $scope.monthYearList[i].DateArr.length; c++) {
                                $scope.dayList.push({
                                    Id: i + '' + c,
                                    Day: $scope.monthYearList[i].DateArr[c].Day
                                });
                            }
                        }
                    }
                    $scope.processBtn = false;
                }
            }), function errorCallBack(response) {
                $scope.processBtn = false;
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.whiteColor = 'black';
    $scope.blueColor = 'blue';
    $scope.redColor = 'red';
    $scope.normalFont = 'normal';
    $scope.boldFont = 900;
    $scope.validationMsg = null;

    //change font color by on click.
    $scope.colorChangePref = null;
    $scope.onClickChangeFC = function (pRefId) {
        try {
            $scope.validationMsg = null;
            if ($scope.colorChangePref !== pRefId) {
                $scope.colorChangePref = pRefId;
                for (var c = 0; c < $scope.newDataList.length; c++) {
                    var from = new Date(Date.now());
                    var toDate = new Date($scope.toDate);
                    var d = Math.floor((Date.parse(toDate) - Date.parse(from)) / 86400000);
                    var days = parseInt(d);
                    var count = new Date(Date.now());
                    for (var i = 0; i < days; i++) {
                        var dayId = $filter('date')(count, 'yyMMdd');
                        if ($scope.newDataList[c].bodyData[dayId].ProductionBatchMasterId === pRefId) {
                            if (!$scope.newDataList[c].bodyData[dayId].OffDay) {
                                if ($scope.newDataList[c].bodyData[dayId].FontColor)
                                    $scope.newDataList[c].bodyData[dayId].textColor = $scope.redColor;
                                else
                                    $scope.newDataList[c].bodyData[dayId].textColor = $scope.blueColor;
                                $scope.newDataList[c].bodyData[dayId].textWeight = $scope.boldFont;
                                if (baseService.isUndefinedOrNull($scope.validationMsg) && !baseService.isUndefinedOrNull($scope.newDataList[c].bodyData[dayId].Msg))
                                    $scope.validationMsg = $scope.newDataList[c].bodyData[dayId].Msg;
                            }
                        }
                        else {
                            if ($scope.newDataList[c].bodyData[dayId].FontColor)
                                $scope.newDataList[c].bodyData[dayId].textColor = $scope.redColor;
                            else
                                $scope.newDataList[c].bodyData[dayId].textColor = $scope.whiteColor;
                            $scope.newDataList[c].bodyData[dayId].textWeight = $scope.normalFont;
                        }
                        count.setDate(count.getDate() + 1);
                    }
                }
            }
            else {
                $scope.colorChangePref = null;
                for (var c = 0; c < $scope.newDataList.length; c++) {
                    var from = new Date(Date.now());
                    var toDate = new Date($scope.toDate);
                    var d = Math.floor((Date.parse(toDate) - Date.parse(from)) / 86400000);
                    var days = parseInt(d);
                    var count = new Date(Date.now());
                    for (var i = 0; i < days; i++) {
                        var dayId = $filter('date')(count, 'yyMMdd');
                        if ($scope.newDataList[c].bodyData[dayId].FontColor)
                            $scope.newDataList[c].bodyData[dayId].textColor = $scope.redColor;
                        else
                            $scope.newDataList[c].bodyData[dayId].textColor = $scope.whiteColor;
                        $scope.newDataList[c].bodyData[dayId].textWeight = $scope.normalFont;
                        count.setDate(count.getDate() + 1);
                    }
                }
            }
        } catch (e) {
            console.log(e);
        }
    }

    //set style
    function setStyle(mainList) {
        try {
            var filterPRefList = [];
            var lineList = distinctSingleColumn(mainList, 'LineId');
            var lineId = '';
            for (var i = 0; i < baseService.arrayLength(lineList); i++) {
                lineId = lineList[i];
                filterPRefList = distinctLineWisePref(mainList, lineId);// return distinct pref by line wise
                isStyleSame(filterPRefList, mainList);
            }
        } catch (e) {
            throw e;
        }
    }
    //*************************************For Style Set*****************************//
    // distinct line
    function distinctSingleColumn(mainList, fieldName) {
        var tempList = [];
        for (var i = 0; i < baseService.arrayLength(mainList); i++) {
            if (!tempList.includes(mainList[i][fieldName]))
                tempList.push(mainList[i][fieldName]);
        }
        return tempList;
    }
    function distinctLineWisePref(mainList, lineId) {
        try {
            var tempList = [];
            for (var i = 0; i < baseService.arrayLength(mainList); i++) {
                if (mainList[i].LineId === lineId && !lineWisePrefData(tempList, mainList[i].ProductionBatchMasterId)
                    && !mainList[i].OffDay) {
                    tempList.push({
                        Id: mainList[i].Id
                        , LineId: mainList[i].LineId
                        , ProductionBatchMasterId: mainList[i].ProductionBatchMasterId
                        , OurStyleId: mainList[i].OurStyleId
                        , OffDayType: mainList[i].OffDayType
                        , OffDay: mainList[i].OffDay
                        , Lsd: $filter('date')(new Date(mainList[i].Lsd), 'yyyy-MM-dd')
                        //, Lsd: new Date(mainList[i].Lsd)
                        , CommitmentDate: new Date(mainList[i].CommitmentDate)
                        //, CommitmentDate: $filter('date')(new Date(mainList[i].CommitmentDate), 'yyyy-MM-dd')
                        , Date: $filter('date')(new Date(mainList[i].Date), 'yyyy-MM-dd')
                        , EndDate: getEndate(mainList, lineId, mainList[i].ProductionBatchMasterId)
                        , Sequence: mainList[i].Sequence
                        , Color: mainList[i].Color
                        , FontColor: mainList[i].FontColor
                        , Msg: mainList[i].Msg
                    });
                }
            }
            tempList = bubbleSortBasic(tempList);
            return tempList;
        } catch (e) {
            throw e;
        }
    }
    function lineWisePrefData(tempList, pref) {
        try {
            var flag = false;
            for (var i = 0; i < tempList.length; i++) {
                if (tempList[i].ProductionBatchMasterId === pref) {
                    flag = true;
                    break;
                }
            }
            return flag;
        } catch (e) {
            console.log(e);
        }
    }
    // swap function helper
    function swap(array, i, j) {
        var temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
    // be careful: this is a very basic implementation which is nice to understand the deep principle of bubble sort (going through all comparisons) but it can be greatly improved for performances
    function bubbleSortBasic(array) {
        for (var i = 0; i < array.length; i++) {
            for (var j = 1; j < array.length; j++) {
                var previousDate = $filter('date')(new Date(array[j - 1].Date), 'yyyy-MM-dd');
                var currentDate = $filter('date')(new Date(array[j].Date), 'yyyy-MM-dd');
                if (previousDate > currentDate)
                    swap(array, j - 1, j);
            }
        }
        return array;
    }
    function getEndate(mainList, lineId, pRefId) {
        try {
            var endDate = '';
            for (var i = 0; i < mainList.length; i++) {
                if (mainList[i].LineId === lineId && mainList[i].ProductionBatchMasterId === pRefId)
                    endDate = mainList[i].Date;
            }
            return $filter('date')(new Date(endDate), 'yyyy-MM-dd');
        } catch (e) {
            console.log(e);
        }
    }
    function isStyleSame(filterPRefList, mainList) {
        try {
            for (var i = 0; i < filterPRefList.length; i++) {
                if (i > 0) {
                    var previousEndDate = $filter('date')(new Date(filterPRefList[i - 1].EndDate), 'yyyy-MM-dd');
                    var currentEndDate = $filter('date')(new Date(filterPRefList[i].Date), 'yyyy-MM-dd');
                    var diffdays = countDate(previousEndDate, currentEndDate) - 2;
                    var isGpDiff = false;
                    if (diffdays > 1)
                        isGpDiff = isPreviousGap(mainList, previousEndDate, currentEndDate, filterPRefList[i].LineId, diffdays);
                    else isGpDiff = false;
                    if (filterPRefList[i - 1].OurStyleId === filterPRefList[i].OurStyleId && isGpDiff)
                        styleWiseColorSet(mainList, filterPRefList[i].Id, 'pink');
                    else if (filterPRefList[i - 1].OurStyleId === filterPRefList[i].OurStyleId)
                        styleWiseColorSet(mainList, filterPRefList[i].Id, 'yellow');
                    else
                        styleWiseColorSet(mainList, filterPRefList[i].Id, 'pink');
                }
                else
                    styleWiseColorSet(mainList, filterPRefList[i].Id, 'pink');
            }
        } catch (e) {
            console.log(e);
        }
    }
    function isPreviousGap(mainlist, pEndDate, cEndDate, lineId, diffdays) {
        try {
            var pDate = $filter('date')(new Date(pEndDate), 'yyyy-MM-dd');
            var cDate = $filter('date')(new Date(cEndDate), 'yyyy-MM-dd');
            var count = 0;
            for (var i = 0; i < mainlist.length; i++) {
                var listDate = $filter('date')(new Date(mainlist[i].Date), 'yyyy-MM-dd');
                if (mainlist[i].LineId === lineId && pDate < listDate && cDate > listDate && mainlist[i].OffDay)
                    count++;
            }
            if (diffdays > count)
                return true;
            else
                return false;
        } catch (e) {
            console.log(e);
        }
    }
    function styleWiseColorSet(mainList, id, colorClass) {
        try {
            for (var i = 0; i < mainList.length; i++) {
                if (mainList[i].Id === id) {
                    mainList[i].Color = colorClass;
                    break;
                }
            }
        } catch (e) {
            throw e;
        }
    }

    //*************************************End For Style Set*****************************//

    //************************************* For Msg Set*****************************//
    function setMsg(mainList) {
        try {
            var lineList = distinctSingleColumn(mainList, 'LineId');
            for (var i = 0; i < lineList.length; i++) {
                var pRefList = distinctLinePref(mainList, lineList[i])
                filterByPrefIdLineId(mainList, lineList[i], pRefList)
            }
        } catch (e) {
            console.log(e);
        }
    }
    function distinctLinePref(mainList, lineId) {
        var tempList = [];
        for (var i = 0; i < baseService.arrayLength(mainList); i++) {
            if (!baseService.isUndefinedOrNull(mainList[i].ProductionBatchMasterId))
                if (mainList[i].LineId === lineId && !tempList.includes(mainList[i].ProductionBatchMasterId))
                    tempList.push(mainList[i].ProductionBatchMasterId);
        }
        return tempList;
    }
    function filterByPrefIdLineId(mainList, lineId, pRefList) {
        try {
            for (var i = 0; i < pRefList.length; i++) {
                setMsgFC(mainList, lineId, pRefList[i]);
            }
        } catch (e) {
            console.log(e);
        }
    }
    function setMsgFC(mainList, lineId, pRefId) {
        try {
            var date = '';
            var cd = '';
            for (var i = 0; i < mainList.length; i++) {
                if (mainList[i].LineId === lineId && mainList[i].ProductionBatchMasterId === pRefId && !mainList[i].OffDay) {
                    date = $filter('date')(new Date(mainList[i].Date), 'yyyy-MM-dd');
                    cd = $filter('date')(new Date(mainList[i].CommitmentDate), 'yyyy-MM-dd');
                    var flag = getAllLineByPref(mainList, pRefId, mainList[i].MinAllocatedLine)
                    mainList[i].Msg = '<ul class="list-group">'
                    if (date > cd) {
                        mainList[i].FontColor = true;
                        mainList[i].Msg += '<li class="list-group-item list-group-item-danger"><b>This production reference not met the commitment date........!</b></li>';
                    }
                    if (flag)
                        mainList[i].Msg += '<li class="list-group-item list-group-item-warning"><b>This production reference not found the minimum line........!</b></li>';
                    if (date <= cd && !flag)
                        mainList[i].Msg = null;
                    else
                        mainList[i].Msg += '</ul>';
                }
            }
        } catch (e) {
            console.log(e);
        }
    }
    function getAllLineByPref(mainList, pRefId, minLine) {
        try {
            var tempList = [];
            var allLineByPref = '';
            var flag = false;
            for (var i = 0; i < baseService.arrayLength(mainList); i++) {
                if (mainList[i].ProductionBatchMasterId === pRefId && !tempList.includes(mainList[i].LineId))
                    tempList.push(mainList[i].LineId);
            }
            allLineByPref = parseInt(tempList.length);
            if (allLineByPref < parseInt(minLine))
                flag = true;
            return flag;
        } catch (e) {
            console.log(e);
        }
    }
    //*************************************End For Msg Set*****************************//

    // #region Create Body
    function getLineFromList(mainList, lineList) {//get distinct wc from db data
        for (var i = 0; i < mainList.length; i++) {
            if (!baseService.isUndefinedOrNull(mainList[i].LineId)) {
                if (!checkLineExists(mainList[i].LineId, lineList)) {
                    lineList.push({
                        Line: mainList[i].Line,
                        LineId: mainList[i].LineId
                    });
                }
            }
        }
        return lineList;
    }
    function checkLineExists(line, list) {
        for (var i = 0; i < list.length; i++) {
            if ((line === list[i].LineId))
                return true;
        }
        return false;
    }
    function newDataList(mainList, fromDate, toDate) {//create table from distinct wc and db data
        var array = [];
        var lineList = [];
        var distinctLineList = getLineFromList(mainList, lineList);
        for (var i = 0; i < distinctLineList.length; i++) {
            array.push({
                Line: distinctLineList[i].Line,
                LineId: distinctLineList[i].LineId,
                bodyData: createBody(distinctLineList[i].LineId, mainList, fromDate, toDate)
            });
        }
        return array;
    }
    function getColumn(processData, colList) {//get column list from main data list.
        if (processData !== null) {
            var ob = processData[0];
            for (var i in ob.bodyData) {
                colList.push(i);
            }
        }
    }
    function createBody(line, mainList, fromDate, toDate) {//create body list
        var bodyData = [];
        var currentDate = angular.copy(fromDate);
        var dayId = null;
        var diffYear = countDate(fromDate, toDate);
        for (var i = 0; i < diffYear; i++) {
            dayId = $filter('date')(currentDate, 'yyMMdd')
            bodyData[dayId] = createBodyCol(line, mainList, currentDate);
            currentDate = new Date(currentDate.setDate(currentDate.getDate() + 1));
        }
        return bodyData;
    }
    function countDate(fromDate, toDate) {
        var D = Math.floor((Date.parse(toDate) - Date.parse(fromDate)) / 86400000) + 2;
        return D;
    }
    //create specific line wise table body.
    function createBodyCol(line, mainList, date) {
        var listDate = 0;
        var cell = {
            Id: null
            , ProductionBatchMasterId: null
            , OurStyle: null
            , Qty: null
            , OffDayType: null
            , OffDay: false
            , DailyOutPut: null
            , Date: date
            , Lsd: null
            , CommitmentDate: null
            , MinAllocatedLine: null
            , MinRequiredTargetHourly: null
            , MinWorkingDays: null
            , RunningDay: null
            , StandardDailyOutPut: null
            , StandardTime: null
            , TotalQty: null
            , ProductionPriority: null
            , FileNo: null
            , Color: null
            , FontColor: null
            , Msg: null
            , ActualAllocatedLine: null
            , QtyVariance: false
            , textColor: null
            , textWeight: null
        };
        for (var i = 0; i < mainList.length; i++) {
            listDate = $filter('dateFilter')(mainList[i].Date, 'dd-MMM-yyyy')
            if ((line === mainList[i].LineId) && compareDate(listDate, date)) {
                cell.Id = mainList[i].Id;
                cell.ProductionBatchMasterId = mainList[i].ProductionBatchMasterId;
                cell.Qty = mainList[i].Qty;
                cell.OffDayType = mainList[i].OffDayType;
                cell.OffDay = mainList[i].OffDay;
                cell.DailyOutPut = mainList[i].DailyOutPut;
                cell.Date = mainList[i].Date;
                cell.OurStyle = mainList[i].OurStyle;

                cell.Lsd = mainList[i].Lsd;
                cell.CommitmentDate = mainList[i].CommitmentDate;
                cell.MinAllocatedLine = mainList[i].MinAllocatedLine;
                cell.MinRequiredTargetHourly = mainList[i].MinRequiredTargetHourly;
                cell.MinWorkingDays = mainList[i].MinWorkingDays;
                cell.RunningDay = mainList[i].RunningDay;
                cell.StandardDailyOutPut = mainList[i].StandardDailyOutPut;
                cell.StandardTime = mainList[i].StandardTime;
                cell.TotalQty = mainList[i].TotalQty;
                cell.OurStyle = mainList[i].OurStyle;
                cell.ProductionPriority = mainList[i].ProductionPriority;
                cell.FileNo = mainList[i].FileNo;
                cell.Color = mainList[i].Color;
                cell.FontColor = mainList[i].FontColor;
                cell.Msg = mainList[i].Msg;
                cell.ActualAllocatedLine = mainList[i].ActualAllocatedLine;
                cell.QtyVariance = mainList[i].QtyVariance;
                if (mainList[i].FontColor)
                    cell.textColor = $scope.redColor;
                else
                    cell.textColor = $scope.whiteColor;
                cell.textWeight = $scope.normalFont;
                return cell;
            }
        }
        return cell;
    }
    function compareDate(f, t) {
        if ((t.getFullYear() === f.getFullYear())
            && (t.getMonth() === f.getMonth())
            && (t.getDate() === f.getDate()))
            return true;
        else
            return false;
    }
    function setCellColor(list, colList, prfList) {
        var color = '';
        for (var i = 0; i < prfList.length; i++) {
            color = getRandomColor();
            for (var c = 0; c < list.length; c++) {
                getColorProperties(list[c].bodyData, prfList[i], color);
            }
        }
    }
    function getPrfFromList(mainList) {//get distinct prf from db data
        var prfList = [];
        for (var i = 0; i < mainList.length; i++) {
            if (!baseService.isUndefinedOrNull(mainList[i].ProductionBatchMasterId)) {
                if (!prfList.includes(mainList[i].ProductionBatchMasterId)) {
                    prfList.push(mainList[i].ProductionBatchMasterId);
                }
            }
        }
        return prfList;
    }
    function getColorllqProperties(colList, prfId, color) {
        for (var i in colList) {
            if (colList[i].ProductionBatchMasterId === prfId) {
                if (colList[i].OffDay === false)
                    colList[i].Color = color;
            }
        }
    }
    function getRandomColor() {
        var letters = '0123456789ABCDEF';
        var color = '#';
        for (var i = 0; i < 6; i++) {
            color += letters[Math.floor(Math.random() * 16)];
        }
        return color;
    }

    // #endregion Create Body

    // #region Create Header

    var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    function diff(from, to, array) {
        var arr = [];
        var todate = new Date(to);
        var fromYear = from.getFullYear();
        var toYear = todate.getFullYear();
        var diffYear = (12 * (toYear - fromYear)) + todate.getMonth();
        for (var i = from.getMonth(); i <= diffYear; i++) {
            var day = 1;
            var totalDay = 0;
            if (i === from.getMonth())
                day = from.getDate();
            if (diffYear === i)
                totalDay = new Date(fromYear, i, todate.getDate()).getDate();
            else
                totalDay = new Date(fromYear, i + 1, 0).getDate();
            array.push({
                MonthYear: monthNames[i % 12] + " " + Math.floor(fromYear + (i / 12)),
                DateArr: getDate(day, totalDay, todate, i)
            });
        }
        return array;
    }
    function getDate(day, totalDay) {
        var days = [];
        var newDay = day;
        var difDay = totalDay - day;
        for (var i = 0; i <= difDay; i++) {
            days.push({ Day: newDay });
            newDay++;
        }
        return days;
    }
    // #endregion Create Header

    //***************Freeze*****************//
    $scope.freezeFromDate = null;
    $scope.freezeToDate = null;
    $scope.freezeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Lsd',
        searchBy: "Lsd",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.freezePopUp = function () {
        $scope.freezeDataList = [];
        $scope.freezeUrl = 'Productions/ProductionReference/GetBatchListByDate?fromDate=' + $scope.freezeFromDate + '&toDate=' + $scope.freezeToDate;
        baseService.setCurrentPage('freezeDataList');
        $scope.getfreezeData = function (pageno) {
            baseService.paginationBase($scope.freezeUrl, pageno, $scope.freezeParameters)
                .then(function (result) {
                    $scope.freezeDataList = result.Rows;
                    $scope.freezeParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.freezeDataList.length; i++) {
                        $scope.freezeDataList[i].Flag = tempList.includes($scope.freezeDataList[i].Id)
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'freezePopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#freezePopUp')).modal('show');
        $scope.getfreezeData();
    }
    //$scope.DateDisable = function (event) {
    //    var toDate = document.getElementById("freezeToDate");
    //    $("#freezeToDate").datepicker({
    //        format: "dd-MM-yyyy",
    //        autoclose: true,
    //        minDate: 0,
    //    });
    //}
    var tempList = [];
    $scope.selectPrfId = function (event, id) {
        if (event.currentTarget.checked)
            tempList.push(id);
        else
            tempList.splice(tempList.indexOf(id), 1);
    }
    $scope.CloseFreezePopUp = function () {
        tempList = [];
        angular.element(document.querySelector('#freezePopUp')).modal('hide');
    }
    $scope.SaveForFreezing = function () {
        try {
            $http({
                method: "POST",
                url: 'Productions/mainprocessplanning/saveFreezing?ids=' + JSON.stringify(tempList),
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure', 'freezePopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success', 'freezePopUp');
                    $scope.getfreezeData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure', 'freezePopUp');
            }
        } catch (e) {
            throw e;
        }
    }
    //***************Freeze*****************//
}