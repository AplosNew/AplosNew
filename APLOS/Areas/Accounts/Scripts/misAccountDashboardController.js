'use strict';
misAccountDashboardController.$inject = ['cboService', '$scope', '$rootScope', 'baseService', '$http', '$filter', '$window'];
function misAccountDashboardController(cboService, $scope, $rootScope, baseService, $http, $filter, $window) {
    $scope.getFromDate;
    $scope.dateRange = {};

    $scope.ElasticCheckboxModel = {
        value: true

    };
    $scope.isActivity = false;

    $scope.expFactDate = {
        factDate: 'PostingDate'
    };
    var dateData;

    $scope.dayOrPeriod = null;

    $scope.itemGroupOption = [
        { value: "0", Name: "PL" },
        { value: "1", Name: "BS" }
    ];

    var EntityWiseVarianceListPl = null;
    $scope.dataLoad = function () {
     
        if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
            throw ShowResult("From date can not be greater then to date", 'failure');
        }
        else {
            $scope.GetBudgetWisevarianceList();
            $scope.dashBoardChangeMIS();

            $scope.titleLine = null;

            $scope.titleLine = $scope.itemName.bold() + " items from " + $scope.dateRange.fromDate.bold() + " to " + $scope.dateRange.toDate.bold() + " in the basis of " + $scope.expFactDate.factDate.bold();

            document.getElementById("title").style.display = "block";


            document.getElementById("title").innerHTML = $scope.titleLine;
        }
    };

    $scope.invalidFromDate = false;
    $scope.checkFromDate = function () {
        var msg = "";
        if (new Date($scope.dateRange.fromDate) > new Date()) {
            msg = "From date must be below or equal to current date!";
            $scope.invalidFromDate = true;
        }
        else if (new Date($scope.dateRange.fromDate) > new Date($scope.dateRange.toDate)) {
            msg = "From date must be below or equal To date!";
            $scope.invalidFromDate = true;
        } else {
            $scope.invalidFromDate = false;
        }
        return manualValidation("div_FromDate", $scope.invalidFromDate, msg);
    };

    $scope.invalidToDate = false;
    $scope.checkToDate = function () {
        var msg = "";
        if (new Date($scope.dateRange.toDate) > new Date()) {
            msg = "To date must be below or equal to current date!";
            $scope.invalidToDate = true;
        }
        else if (new Date($scope.dateRange.toDate) < new Date($scope.dateRange.fromDate)) {
            msg = "To date must be greater then or equal From date!";
            $scope.invalidFromDate = true;
        }
        else {
            $scope.invalidToDate = false;
        }
        return manualValidation("div_ToDate", $scope.invalidToDate, msg);
    };

    $scope.gap = -2;
    $scope.budgetResponsiblePersonId = null;
    $scope.overAllStatusList = [];

    $scope.BudgetDateSuper = [];
    $scope.BudgetWiseExpenseList = [];
    $scope.BudgetWiseExpenseTillToday = [];

    $scope.budgetCategoryList = [];
    $scope.budgetCategoryWiseAmountList = [];
    $scope.budgetSubCategoryList = [];
    $scope.budgetItemList = [];

    $scope.ctgWiseVarianceList = [];

    $scope.entityListfrmCboCompanyGroup = [];
    $scope.entityListfrmCboCompany = [];
    $scope.entityListfrmCboPlant = [];
    $scope.entityListfrmCboDivision = [];
    $scope.entityListfrmCboSubDivision = [];
    $scope.entityListfrmCboUnit = [];

    $scope.entityListfrmCboCompanyGroupIntact = [];
    $scope.entityListfrmCboCompanyIntact = [];
    $scope.entityListfrmCboPlantIntact = [];
    $scope.entityListfrmCboDivisionIntact = [];
    $scope.entityListfrmCboSubDivisionIntact = [];
    $scope.entityListfrmCboUnitIntact = [];
    $scope.entityListfrmCboEntityIntact = [];

    var misAccountActivityDetail = document.getElementById("misAccountActivityDetail");
    misAccountActivityDetail.style.display = "none";

    var backButtonMisAccountDivActivityDetail = document.getElementById("backButtonMisAccountDivActivityDetail");
    backButtonMisAccountDivActivityDetail.style.display = "none";
    var agGridLoader;

    $scope.budgetType = "0";
    $scope.itemName = "PL";

    $scope.titleLine = null;

    $scope.itemNameChange = function () {
        $scope.itemName = document.getElementById("itemGroupData").options[document.getElementById('itemGroupData').selectedIndex].text;
    };
    $scope.voucher = {
        VoucherId: null,
        voucherDate: null
    };
    var dateShow = document.getElementById("dateShow");

    var dateRefresh = document.getElementById("dateRefresh");
    dateRefresh.style.display = "block";

    var misAccountDivPrdWiseDetail = document.getElementById("misAccountDivPrdWiseDetail");
    misAccountDivPrdWiseDetail.style.display = "none";

    var misAccountDivExceptionPeriodDetail = document.getElementById("misAccountDivExceptionPeriodDetail");
    misAccountDivExceptionPeriodDetail.style.display = "none";

    var backButtonMisAccountDivPrdWiseDetail = document.getElementById("backButtonMisAccountDivPrdWiseDetail");
    backButtonMisAccountDivPrdWiseDetail.style.display = "block";
    var itemGroup = document.getElementById("itemGroup");

    var orgStrSelectBox = document.getElementById("orgStrSelectBoxdiv"); //Organizational Structure Select Box

    var dateRange = document.getElementById("dateRange");

    var dateType = document.getElementById("dateType");

    var backbutton = document.getElementById("backButton");

    var mainBlock = document.getElementById("mainDivbl");

    var misDashboard = document.getElementById("MISDashboard");

    var pnlButton = document.getElementById("pnlButton");
    var misButton = document.getElementById("misButton");

    var mpButton = document.getElementById("mPButtons");

    var misAccountDiv2 = document.getElementById("misAccountDiv2");
    misAccountDiv2.style.display = "none";

    var misAccountDivExceptionPeriod = document.getElementById("misAccountDivExceptionPeriod");
    misAccountDivExceptionPeriod.style.display = "none";

    var misAccountDiv1 = document.getElementById("misAccountDiv1");
    misAccountDiv1.style.display = "block";

    $scope.misDiv3 = function (x) {
        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "block";
    };

    $scope.misBackButtonClick = function () {
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "none";
        if ($scope.isActivity === false) {
            misAccountActivityDetail.style.display = "none";
            misAccountDiv1.style.display = "block";
            orgStrSelectBox.style.display = "block";
            $scope.isActivity = false;
        }
        else {
            misAccountActivityDetail.style.display = "block";
            misAccountDiv1.style.display = "none";
            orgStrSelectBox.style.display = "none";
            $scope.isActivity = true;

        }

    };

    $scope.backButtonMisAccountDivActivityDetailClick = function () {
        misAccountDiv1.style.display = "block";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "none";
        misAccountActivityDetail.style.display = "none";
        orgStrSelectBox.style.display = "block";
    };

    $scope.misBackButtonClick2 = function () {
        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "block";
        misAccountDivPrdWiseDetail.style.display = "none";
        misAccountActivityDetail.style.display = "none";
    };

    $scope.backButtonMisAccountDivPrdWiseDetailClick = function () {
        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriodDetail.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "block";
        misAccountActivityDetail.style.display = "none";
    };

    $scope.backbuttonClick = function () {

        document.getElementById("title").style.display = "none";
        dateRefresh.style.display = "block";
        itemGroup.style.display = "block";
        misDashboard.style.display = "none";
        dateRange.style.display = "block";
        dateType.style.display = "block";
        misButton.style.display = "block";
        mainBlock.style.display = "block";
        backbutton.style.display = "none";
        dateShow.style.display = "none";
        misAccountActivityDetail.style.display = "none";
        misAccountDivExceptionPeriodDetail.style.display = "none";
        $scope.isActivity = false;

        document.getElementById("postingDate").disabled = false;
        document.getElementById("entryDate").disabled = false;
    };

    backbutton.style.display = "none";
    mainBlock.style.display = "block";
    dateShow.style.display = "none";

    misDashboard.style.display = "none";

    var formDate = new Date($scope.dateRange.fromDate);
    var toDate = new Date($scope.dateRange.toDate);
    $scope.dashBoardChangeMIS = function () {
        if (formDate > toDate) {
            throw ShowResult("From date can not be greater then to date", 'failure');
        }
        if ($scope.dateRange.fromDate === "" || $scope.dateRange.fromDate === undefined || $scope.dateRange.fromDate === null) {
            throw ShowResult("From Date can not be empty", 'failure');
        }
        if ($scope.dateRange.toDate === "" || $scope.dateRange.toDate === undefined || $scope.dateRange.toDate === null) {
            throw ShowResult("To Date can not be empty", 'failure');
        }
        else {

            $scope.clearEntityFilter(); $scope.GetMasterFilterationData();
            itemGroup.style.display = "none";
            misDashboard.style.display = "block";
            backbutton.style.display = "block";
            dateShow.style.display = "block";
            dateRange.style.display = "none";
            dateType.style.display = "none";
            misButton.style.display = "none";
            misAccountDiv1.style.display = "block";
            misAccountDiv2.style.display = "none";
            mainBlock.style.display = "none";
            misAccountDivExceptionPeriod.style.display = "none";
            misAccountActivityDetail.style.display = "none";
            dateRefresh.style.display = "none";
            misAccountDivPrdWiseDetail.style.display = "none";
            orgStrSelectBox.style.display = "block";
        }
    };

    $scope.dashBoardChangePNL = function () {
        misDashboard.style.display = "none";
        dateRange.style.display = "none";
        dateType.style.display = "block";
        misButton.style.display = "block";
        mainBlock.style.display = "none";
        backbutton.style.display = "block";
        dateShow.style.display = "block";
        misAccountActivityDetail.style.display = "none";
    };


    $scope.BudgetMasterWiseList = [];

    $scope.getBudgetMasterWiseAmount = function (x, dayOrPeriod) {
        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "block";
        misAccountDivExceptionPeriod.style.display = "none";
        misAccountDivPrdWiseDetail.display = "none";
        misAccountActivityDetail.style.display = "none";
        orgStrSelectBox.style.display = "none";

        $scope.dayOrPeriod = dayOrPeriod;

        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetMasterWiseAmountElastic/',
            params: {
                'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId,
                'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': x.budgetCategoryId,
                'budgetSubCategory': x.budgetSubcategoryId, 'budget': x.budgetId, 'activity': x.ActivityId, 'budgetMasterId': x.BudgetMasterId,
                'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'dayOrPeriod': dayOrPeriod, 'dateType': $scope.expFactDate.factDate,
                'EntryPeriodId': x.EntryPeriodId, 'PostingPeriodId': x.PostingPeriodId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetMasterWiseList = response.data;
        });
    };


    $scope.misAccountExceptionPeriod = function (x, screenType) {
        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "block";
        misAccountDivPrdWiseDetail.display = "none";
        misAccountDivExceptionPeriodDetail.style.display = "none";
        if (screenType === "activity") {
            misAccountActivityDetail.style.display = "block";
        }
        else {
            misAccountActivityDetail.style.display = "none";
        }


        orgStrSelectBox.style.display = "none";
        var budgetmasterList = [];
        if (typeof x.budgetMasterList !== "undefined") {
            budgetmasterList = x.budgetMasterList;
        }
        else {
            budgetmasterList.push(x.BudgetMasterId);
        }

        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetMasterWiseExceptionAmount/',
            params: {
                'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId,
                'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': x.budgetCategoryId,
                'budgetSubCategory': x.budgetSubcategoryId, 'budget': x.budgetId, 'Activity': x.ActivityId, 'budgetMasterId': budgetmasterList,
                'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ExceptionPeriodWiseAmountList = response.data;
        });
    };
    $scope.voucherSntnce = [];
    $scope.ExceptionPeriodWiseAmountDetailList = [];
    var GAPExceptionPeriodWiseAmountDetailList = [];
    $scope.misAccountExceptionPeriodDetail = function (x) {

        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "none";
        misAccountDivPrdWiseDetail.display = "none";
        misAccountDivExceptionPeriodDetail.style.display = "block";
        misAccountActivityDetail.style.display = "none";
        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetMasterWiseExceptionAmountDetail/',
            params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'budgetMasterId': x.BudgetMasterId, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'periodName': x.PostingPeriod },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ExceptionPeriodWiseAmountDetailList = response.data;
            GAPExceptionPeriodWiseAmountDetailList = $scope.ExceptionPeriodWiseAmountDetailList;

        });
    };

    $scope.gapCalculation = function () {
        $scope.ExceptionPeriodWiseAmountDetailList = [];
        $scope.ExceptionPeriodWiseAmountDetailList = GAPExceptionPeriodWiseAmountDetailList.filter(function (amountList) {
            return amountList.GAP < $scope.gap;
        });

    };
    $scope.BudgetMasterPrdWiseList = [];

    $scope.GetBudgetMasterWiseVoucherDetail = function (x) {

        misAccountDiv1.style.display = "none";
        misAccountDiv2.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "none";
        misAccountDivExceptionPeriod.style.display = "none";
        misAccountDivPrdWiseDetail.style.display = "block";
        backButtonMisAccountDivPrdWiseDetail.style.display = "block";
        misAccountDivExceptionPeriodDetail.style.display = "none";
        misAccountActivityDetail.style.display = "none";
        orgStrSelectBox.style.display = "none";

        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetMasterWiseAmount/',
            params: {
                'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId,
                'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId,
                'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': x.ActivityId, 'budgetMasterId': x.BudgetMasterId,
                'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'periodName': $scope.periodName, 'dateType': $scope.expFactDate.factDate,
                'dayOrPeriod': $scope.dayOrPeriod, 'EntryPeriodId': x.EntryPeriodId, 'PostingPeriodId': x.PostingPeriodId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetMasterPrdWiseList = response.data;
        });
    };

    window.chartColors = {
        red: 'rgba(240, 52, 52, .6)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgba(46, 204, 113,.6)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };

    //$scope.getBudgetSubCategoryCbo = function () {
    //    cboService.getBudgetSubCategoryCboByCategory($scope.budgetCategoryId, function (result) {
    //        $scope.budgetSubCategoryList = result;
    //    });
    //};

    $scope.budgetCategoryId = null;
    $scope.budgetSubCategoryId = null;
    $scope.budgetId = null;

    $scope.companyGroup =
        {
            companyGroupId: null,
            companyGroupName: null
        };
    $scope.company =
        {
            companyId: null,
            companyName: null
        };
    $scope.plant =
        {
            plantId: null,
            plantName: null
        };

    $scope.entity = {
        entityId: null,
        entityName: null
    };

    $scope.division = {
        divisionId: null,
        divisionName: null
    };

    $scope.subDivision =
        {
            subDivisionId: null,
            subDivisionName: null
        };
    $scope.unit =
        {
            unitId: null,
            unitName: null
        };
    $scope.lblCompanyGroup = null;
    $scope.lblCompany = null;
    $scope.lblPlant = null;
    $scope.lblDivision = null;
    $scope.lblsubDivision = null;
    $scope.lblUnit = null;

    $scope.entityList = [];

    //$scope.lblStringCompanyGroupChange = function () {
    //    $scope.entityList.splice(0, 7);
    //    if ($scope.companyGroup.companyGroupId !== null && $scope.companyGroup.companyGroupId !== "") {

    //        $scope.companyGroup.companyGroupName = document.getElementById("companyGroup").options[document.getElementById('companyGroup').selectedIndex].text;
    //        $scope.lblCompanyGroup = "Group";
    //        var row = {
    //            Name: $scope.companyGroup.companyGroupName,
    //            value: $scope.lblCompanyGroup,
    //            id: document.getElementById("companyGroup").options[document.getElementById('companyGroup').selectedIndex].value

    //        };
    //        $scope.entityList.push(row);
    //    }
    //    else {
    //        $scope.entityList = [];
    //    }
    //};
    //$scope.lblStringCompanyGroupChange();

    $scope.lblStringCompanyChange = function (list) {
        $scope.entityList.splice(1, 6);
        var row = {
            cmpName: null,
            cmpvalue: null,
            cmpid: null
        };

        if (list === null || list === undefined) {
            $scope.company.companyId = null;
            $scope.plant.plantId = null;
            $scope.division.divisionId = null;
            $scope.division.divisionName = null;
            $scope.subDivision.subDivisionId = null;
            $scope.unit.unitId = null;
            $scope.entity.entityId = null;
        }
        else {
            $scope.company.companyName = list[0].Company;
            $scope.cmpId = list[0].CompanyId;

            row = {
                cmpName: $scope.company.companyName,
                cmpvalue: $scope.lblCompany,
                cmpid: $scope.cmpId

            };
            $scope.entityList.push(row);
        }
        //if ($scope.company.companyId !== null && $scope.company.companyId !== "") {
        //if (list === null || list === undefined) {
        //    $scope.company.companyName = document.getElementById("company").options[document.getElementById('company').selectedIndex].text;
        //    $scope.cmpId = document.getElementById("company").options[document.getElementById('company').selectedIndex].value;
        //}
        //else {
        //    $scope.company.companyName = list[0].Company;
        //    $scope.cmpId = list[0].CompanyId;
        //}
        //$scope.lblCompany = "Company";
        //row = {
        //    cmpName: $scope.company.companyName,
        //    cmpvalue: $scope.lblCompany,
        //    cmpid: $scope.cmpId
        //};
        //$scope.entityList.push(row);
        ////}
    };
    $scope.lblStringCompanyChange();

    $scope.lblStringPlantChange = function (list) {
        $scope.entityList.splice(2, 5);

        var row = {
            plantName: null,
            plantvalue: null,
            plantId: null
        };
        if (list === null || list === undefined) {
            $scope.plant.plantId = null;
            $scope.division.divisionId = null;
            $scope.division.divisionName = null;
            $scope.subDivision.subDivisionId = null;
            $scope.unit.unitId = null;
            $scope.entity.entityId = null;
        }
        else {
            $scope.plant.PlantName = list[0].plant;
            $scope.lblPlant = "Plant ";

            row = {
                Name: $scope.plant.PlantName,
                value: $scope.lblPlant,
                id: list[0].plantId
            };
            $scope.entityList.push(row);
        }


        //if (list === null || list === undefined) {
        //    $scope.plant.plantName = document.getElementById("plant").options[document.getElementById('plant').selectedIndex].text;
        //    $scope.plantId = document.getElementById("plant").options[document.getElementById('plant').selectedIndex].value;
        //}
        //else {
        //    $scope.plant.plantName = list[0].Plant;
        //    $scope.plantId = list[0].plantId;
        //}
        //$scope.lblPlant = "Plant";
        //row = {
        //    Name: $scope.plant.plantName,
        //    value: $scope.lblPlant,
        //    id: $scope.plantId
        //};
        //$scope.entityList.push(row);

    };
    $scope.lblStringPlantChange(null);

    $scope.lblStringDivisionChange = function (list) {
        $scope.entityList.splice(3, 4);
        var row = {
            Name: null,
            value: null,
            id: null
        };
        if (list === null) {
            $scope.division.divisionId = null;
            $scope.division.divisionName = null;
            $scope.subDivision.subDivisionId = null;
            $scope.unit.unitId = null;
            $scope.entity.entityId = null;
        }
        else {
            $scope.division.divisionName = list[0].Division;
            $scope.lblDivision = "Division ";

            row = {
                Name: $scope.division.divisionName,
                value: $scope.lblDivision,
                id: list[0].DivisionId
            };
            $scope.entityList.push(row);
        }
    };
    $scope.lblStringSubDivisionChange = function (list) {
        $scope.entityList.splice(4, 3);
        var row = {
            Name: null,
            value: null,
            id: null
        };
        if (list === null) {
            $scope.subDivision.subDivisionName = null;
            $scope.subDivision.subDivisionId = null;
            $scope.unit.unitId = null;
            $scope.entity.entityId = null;
        }
        else {
            $scope.subDivision.subDivisionName = list[0].subDivision;
            $scope.lblsubDivision = "Sub Division";
            row = {
                Name: $scope.subDivision.subDivisionName,
                value: $scope.lblsubDivision,
                id: list[0].subDivisionId
            };
            $scope.entityList.push(row);
            //}
        }
    };

    $scope.lblStringUnitChange = function (list) {
        $scope.entityList.splice(5, 2);
        row = {
            Name: null,
            value: null,
            id: null
        };
        if (list === null) {
            $scope.unit.unitId = null;
            $scope.unit.unitName = null;//document.getElementById("unit").options[document.getElementById('unit').selectedIndex].text;
            $scope.entity.entityId = null;
        }
        else {
            $scope.unit.unitName = list[0].Unit;
            $scope.lblUnit = "Unit";
            var row = {
                Name: $scope.unit.unitName,
                value: $scope.lblUnit,
                id: list[0].UnitId
            };
            $scope.entityList.push(row);
        }
    };
    $scope.lblStringEntityChange = function (list) {
        $scope.entityList.splice(6, 1);
        //if ($scope.entity.entityId !== null && $scope.entity.entityId !== "") {
        row = {
            Name: null,
            value: null,
            id: null
        };
        if (list === null) {
            $scope.entity.entityId = null;
            $scope.entity.entityName = null;
        }
        else {
            $scope.entity.entityName = document.getElementById("entity").options[document.getElementById('entity').selectedIndex].text;
            $scope.lblEntity = "Entity";
            var row = {
                Name: $scope.entity.entityName,
                value: $scope.lblEntity,
                id: list[0].entityId
            };
            $scope.entityList.push(row);
        }
    };

    $scope.activityList = [];

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboCompanyByCompanyGroup = function () {
        cboService.getCboCompanyByCompanyGroup($scope.companyGroup.companyGroupId, function (result) {
            $scope.companyList = result;
        });
    };
    $scope.getCboCompanyByCompanyGroup();

    $scope.GetEntityDetailFromCompanySelection = function () {
        $scope.entityList = [];
        if ($scope.company.companyId === "") {
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();
            //$scope.companyGroup.companyGroupId = null;
            //$scope.lblCompanyGroup = null;
            //$scope.lblCompany = null;
        }

        else {
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFromCompanySelection',
                params: {
                    'companyId': $scope.company.companyId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.EntityWiseCompanyList = response.data;

            });
        }
    };


    $scope.GetEntityWisePlantCbo = function () {
        cboService.getCboEntityWisePlant($scope.companyGroup.companyGroupId, $scope.company.companyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.GetEntityWisePlantCbo();

    $scope.GetEntityDetailFromPlantSelection = function () {
        $scope.entityList = [];
        if ($scope.plant.plantId === "") {
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();
            //$scope.companyGroup.companyGroupId = null;
            //$scope.company.companyId = null;


            //$scope.lblCompanyGroup = null;
            //$scope.lblCompany = null;
            //$scope.lblPlant = null;
        }

        else {
            $scope.entityList = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFromPlantSelection',
                params: {
                    'plantId': $scope.plant.plantId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.EntityWisePlantList = response.data;

                $scope.companyGroup.companyGroupId = $scope.EntityWisePlantList[0].CompanyGroupId;
                $scope.company.companyId = $scope.EntityWisePlantList[0].CompanyId;

                var row = {
                    Name: $scope.EntityWisePlantList[0].CompanyGroup,
                    value: "Group"
                };
                $scope.entityList.push(row);
                var rowc = {
                    Name: $scope.EntityWisePlantList[0].CompanyName,
                    value: "Company"
                };
                $scope.entityList.push(rowc);
                var rowp = {
                    value: "Plant",
                    Name: $scope.EntityWisePlantList[0].Plant
                };
                $scope.entityList.push(rowp);
            });
        }
    };

    $scope.entityListfrmCbo = [];

    $scope.GetEntityWiseEntityCbo = function (entityListCBOS) {
        if (baseService.arrayLength(entityListCBOS) > 0) {
            $scope.entityListCBO = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityWiseEntityCbo',
                params: {
                    'entityList': entityListCBOS,
                    'plantId': $scope.plant.plantId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.entityListCBO = response.data;

                let array = $scope.entityListCBO;

                $scope.companyGroup.companyGroupId = $window.companyGroupId;
                $scope.company.companyId = $window.companyId;
                $scope.plant.plantId = $window.plantId;

                $scope.entityListfrmCboCompanyGroup = removeDuplicates(array, "CompanyGroupId");
                $scope.entityListfrmCboCompany = removeDuplicates(array, "CompanyId");
                $scope.entityListfrmCboPlant = removeDuplicates(array, "plantId");
                $scope.entityListfrmCboDivision = removeDuplicates(array, "DivisionId");
                $scope.entityListfrmCboSubDivision = removeDuplicates(array, "subDivisionId");
                $scope.entityListfrmCboUnit = removeDuplicates(array, "UnitId");
                $scope.entityListfrmCbo = removeDuplicates(array, "entityId");

                $scope.entityListfrmCboCompanyGroupIntact = removeDuplicates(array, "CompanyGroupId");;
                $scope.entityListfrmCboCompanyIntact = removeDuplicates(array, "CompanyId");
                $scope.entityListfrmCboPlantIntact = removeDuplicates(array, "plantId");
                $scope.entityListfrmCboDivisionIntact = removeDuplicates(array, "DivisionId");
                $scope.entityListfrmCboSubDivisionIntact = removeDuplicates(array, "subDivisionId");
                $scope.entityListfrmCboUnitIntact = removeDuplicates(array, "UnitId");
                $scope.entityListfrmCboEntityIntact = removeDuplicates(array, "UnitId");
            });
        }
        else {
            //$scope.entityListCBO = [];
            $scope.entityListfrmCboCompanyGroup = [];
            $scope.entityListfrmCboCompany = [];
            $scope.entityListfrmCboPlant = [];
            $scope.entityListfrmCboDivision = [];
            $scope.entityListfrmCboSubDivision = [];
            $scope.entityListfrmCboUnit = [];
            $scope.entityListfrmCbo = [];
        }

    };

    $scope.entityChange = function (list) {
        $scope.entityListfrmCboE = $filter('filter')(list, { entityId: $scope.entity.entityId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboE) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboE[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboE[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboE[0].plantId;
            $scope.division.divisionId = $scope.entityListfrmCboE[0].DivisionId;
            $scope.subDivision.subDivisionId = $scope.entityListfrmCboE[0].subDivisionId;
            $scope.unit.unitId = $scope.entityListfrmCboE[0].UnitId;
            $scope.entity.entityId = $scope.entityListfrmCboE[0].entityId;
            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboE); $scope.lblStringPlantChange($scope.entityListfrmCboE);
            $scope.lblStringDivisionChange($scope.entityListfrmCboE); $scope.lblStringSubDivisionChange($scope.entityListfrmCboE); $scope.lblStringUnitChange($scope.entityListfrmCboE);
            $scope.lblStringEntityChange($scope.entityListfrmCboE);
        }
        else {
            $scope.lblStringEntityChange(null);
        }
    };

    $scope.companyChange = function (list, companyGroupId, companyId) {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);

        var item = angular.copy($scope.entityList);
        var last_item = item.reverse()[0];


        EntityWiseVarianceListPl = $scope.varianceList;

        if (companyId === "" || companyId === null) {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId });

            $scope.entityListfrmCboCompany = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId });
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId });
        }
        else {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId });

            $scope.entityListfrmCboCompany = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboCompanyIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
        }

        $scope.entityListfrmCboP = $filter('filter')(list, { CompanyId: $scope.company.companyId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboP) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboP[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboP[0].CompanyId;
            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboP); $scope.lblStringPlantChange($scope.entityListfrmCboP);
            $scope.lblStringDivisionChange($scope.entityListfrmCboP); $scope.lblStringSubDivisionChange($scope.entityListfrmCboP); $scope.lblStringUnitChange($scope.entityListfrmCboP);
            $scope.lblStringEntityChange($scope.entityListfrmCboP);
        }
        else {
            $scope.lblStringCompanyChange(null);
        }

        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);
    };

    $scope.plantChange = function (list, companyGroupId, companyId, plantId) {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);

        EntityWiseVarianceListPl = $scope.varianceList;

        if (plantId === "" || plantId === null) {
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId });
        }
        else if (companyGroupId === null || companyId === null) {
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { plantId: plantId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { plantId: plantId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboPlantIntact, { plantId: plantId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboPlantIntact, { plantId: plantId });
        }
        else {
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboPlantIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
        }

        $scope.entityListfrmCboP = $filter('filter')(list, { plantId: $scope.plant.plantId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboP) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboP[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboP[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboP[0].plantId;
            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboP); $scope.lblStringPlantChange($scope.entityListfrmCboP);
            $scope.lblStringDivisionChange($scope.entityListfrmCboP); $scope.lblStringSubDivisionChange($scope.entityListfrmCboP); $scope.lblStringUnitChange($scope.entityListfrmCboP);
            $scope.lblStringEntityChange($scope.entityListfrmCboP);
        }
        else {
            $scope.lblStringPlantChange(null);
        }

        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);
    };

    $scope.divisionChange = function (list, companyGroupId, companyId, plantId, divisionId) {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);

        if (divisionId === "") {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId });
            $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
        }
        else if (companyGroupId === null || companyId === null || plantId === null || divisionId === null) {
            $scope.varianceList = $filter("filter")($scope.varianceList, { divisionId: divisionId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { DivisionId: divisionId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { DivisionId: divisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { DivisionId: divisionId });
        }
        else {
            // $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId });
            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
        }
        $scope.entityListfrmCboDiv = $filter('filter')($scope.entityListfrmCboEntityIntact, { DivisionId: $scope.division.divisionId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboDiv) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboDiv[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboDiv[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboDiv[0].plantId;
            $scope.division.divisionId = $scope.entityListfrmCboDiv[0].DivisionId;

            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboDiv); $scope.lblStringPlantChange($scope.entityListfrmCboDiv);
            $scope.lblStringDivisionChange($scope.entityListfrmCboDiv);
        }
        else {
            $scope.lblStringDivisionChange(null);
        }
        EntityWiseVarianceListPl = $scope.varianceList;
        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);
    };

    $scope.subDivisionChange = function (list, companyGroupId, companyId, plantId, divisionId, subdivisionId) {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);



        if (subdivisionId === "") {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId });

            $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId });
        }
        else if (companyGroupId === null || companyId === null || plantId === null || divisionId === null) {
            //$scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, {  subDivisionId: subdivisionId });
            $scope.varianceList = $filter("filter")($scope.varianceList, { subDivId: subdivisionId });

            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { subDivisionId: subdivisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { subDivisionId: subdivisionId });
        }
        else {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId, subDivId: subdivisionId });

            $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });
        }

        $scope.entityListfrmCboSubDiv = $filter('filter')(list, { subDivisionId: $scope.subDivision.subDivisionId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboSubDiv) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboSubDiv[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboSubDiv[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboSubDiv[0].plantId;
            $scope.division.divisionId = $scope.entityListfrmCboSubDiv[0].DivisionId;
            $scope.subDivision.subDivisionId = $scope.entityListfrmCboSubDiv[0].subDivisionId;
            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboSubDiv); $scope.lblStringPlantChange($scope.entityListfrmCboSubDiv);
            $scope.lblStringDivisionChange($scope.entityListfrmCboSubDiv);
            $scope.lblStringSubDivisionChange($scope.entityListfrmCboSubDiv);
        }
        else {
            $scope.lblStringSubDivisionChange(null);
        }
        EntityWiseVarianceListPl = $scope.varianceList;

        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);

    };

    $scope.unitChange = function (list, companyGroupId, companyId, plantId, divisionId, subdivisionId, unitId) {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);

        if (unitId === "") {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId, subDivId: subdivisionId });

            // $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });
            // $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });

        }
        else if (companyGroupId === null || companyId === null || plantId === null || divisionId === null || subdivisionId === null) {
            $scope.varianceList = $filter("filter")($scope.varianceList, { unitId: unitId });

            //$scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { UnitId: unitId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { UnitId: unitId });

        }
        else {
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId, subDivId: subdivisionId, unitId: unitId });

            //  $scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId, UnitId: unitId });
            $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId, UnitId: unitId });

        }
        $scope.entityListfrmCboU = $filter('filter')(list, { UnitId: $scope.unit.unitId }, true);

        if (baseService.arrayLength($scope.entityListfrmCbo) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboU[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboU[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboU[0].plantId;
            $scope.division.divisionId = $scope.entityListfrmCboU[0].DivisionId;
            $scope.subDivision.subDivisionId = $scope.entityListfrmCboU[0].subDivisionId;
            $scope.unit.unitId = $scope.entityListfrmCboU[0].UnitId;

            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboU); $scope.lblStringPlantChange($scope.entityListfrmCboU);
            $scope.lblStringDivisionChange($scope.entityListfrmCboU);
            $scope.lblStringSubDivisionChange($scope.entityListfrmCboU);
            $scope.lblStringUnitChange($scope.entityListfrmCboU);
        }
        else {
            $scope.lblStringUnitChange(null);
        }
        EntityWiseVarianceListPl = $scope.varianceList;

        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);

    };

    $scope.entityChange = function (list, companyGroupId, companyId, plantId, divisionId, subdivisionId, unitId, entityId) {

        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceListPl = [];

        $scope.varianceList = angular.copy(varBudgetWiseVarianceList);


        if (entityId === "") {
            // $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            //$scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            //$scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId });
            $scope.varianceList = $filter("filter")($scope.varianceList, { cmpGroupId: companyGroupId, companyId: companyId, plantId: plantId, divisionId: divisionId, subDivId: subdivisionId, unitId: unitId });

            // $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId, UnitId: unitId });
        }
        else if (companyGroupId === null || companyId === null || plantId === null || divisionId === null || subdivisionId === null || unitId === null) {
            $scope.varianceList = $filter("filter")($scope.varianceList, { EntityId: entityId });
            // $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { entityId: entityId });
        }

        else {
            // $scope.entityListfrmCboDivision = $filter("filter")($scope.entityListfrmCboDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId });
            // $scope.entityListfrmCboSubDivision = $filter("filter")($scope.entityListfrmCboSubDivisionIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId, UnitId:unitId });
            //$scope.entityListfrmCboUnit = $filter("filter")($scope.entityListfrmCboUnitIntact, { CompanyGroupId: companyGroupId, CompanyId: companyId, plantId: plantId, DivisionId: divisionId, subDivisionId: subdivisionId, UnitId: unitId });
            $scope.varianceList = $filter("filter")($scope.varianceList, { EntityId: entityId });

            // $scope.entityListfrmCbo = $filter("filter")($scope.entityListfrmCboEntityIntact, { entityId: entityId });
        }
        $scope.entityListfrmCboE = $filter('filter')(list, { entityId: $scope.entity.entityId }, true);

        if (baseService.arrayLength($scope.entityListfrmCboE) > 0) {
            $scope.companyGroup.companyGroupId = $scope.entityListfrmCboE[0].CompanyGroupId;
            $scope.company.companyId = $scope.entityListfrmCboE[0].CompanyId;
            $scope.plant.plantId = $scope.entityListfrmCboE[0].plantId;
            $scope.division.divisionId = $scope.entityListfrmCboE[0].DivisionId;
            $scope.subDivision.subDivisionId = $scope.entityListfrmCboE[0].subDivisionId;
            $scope.unit.unitId = $scope.entityListfrmCboE[0].UnitId;
            $scope.entity.entityId = $scope.entityListfrmCboE[0].entityId;
            $scope.lblStringCompanyGroupChange(); $scope.lblStringCompanyChange($scope.entityListfrmCboE); $scope.lblStringPlantChange($scope.entityListfrmCboE);
            $scope.lblStringDivisionChange($scope.entityListfrmCboE); $scope.lblStringSubDivisionChange($scope.entityListfrmCboE); $scope.lblStringUnitChange($scope.entityListfrmCboE);
            $scope.lblStringEntityChange($scope.entityListfrmCboE);
        }
        else {
            $scope.lblStringEntityChange(null);
        }
        EntityWiseVarianceListPl = $scope.varianceList;

        createXLfilters(EntityWiseVarianceListPl, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);

    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.GetEntityDetailFromEntitySelection = function () {
        $scope.entityList = [];
        if ($scope.entity.entityId === "") {
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();
        }

        else {
            $scope.entityList = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFromEntitySelection',
                params: {
                    'entityId': $scope.entity.entityId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId,
                    'plantId': $scope.plant.plantId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.EntityWiseEntityList = response.data;

                $scope.companyGroup.companyGroupId = $scope.EntityWiseEntityList[0].CompanyGroupId;
                $scope.company.companyId = $scope.EntityWiseEntityList[0].CompanyId;
                $scope.plant.plantId = $scope.EntityWiseEntityList[0].PlantId;

                $scope.division.divisionId = $scope.EntityWiseEntityList[0].DivisionId;
                $scope.subDivision.subDivisionId = $scope.EntityWiseEntityList[0].SubDivisionId;
                $scope.unit.unitId = $scope.EntityWiseEntityList[0].UnitId;

                var row = {
                    Name: $scope.EntityWiseEntityList[0].CompanyGroup,
                    value: "Group"
                };
                $scope.entityList.push(row);

                var rowc = {
                    Name: $scope.EntityWiseEntityList[0].CompanyName,
                    value: "Company"
                };
                $scope.entityList.push(rowc);


                var rowp = {
                    Name: $scope.EntityWiseEntityList[0].plant,
                    value: "Plant"
                };
                $scope.entityList.push(rowp);

                var rowd = {
                    Name: $scope.EntityWiseEntityList[0].Division,
                    value: "Division"
                };
                $scope.entityList.push(rowd);

                var rowsd = {
                    Name: $scope.EntityWiseEntityList[0].SubDivision,
                    value: "SubDivision"
                };
                $scope.entityList.push(rowsd);


                var rowu = {
                    Name: $scope.EntityWiseEntityList[0].Unit,
                    value: "Unit"
                };
                $scope.entityList.push(rowu);

                var rowe = {
                    Name: $scope.EntityWiseEntityList[0].Entity,
                    value: "Entity"
                };
                $scope.entityList.push(rowe);
            });
        }
    };

    $scope.GetEntityWiseDivisionCbo = function () {
        cboService.getCboEntityWiseDivision($scope.companyGroup.companyGroupId, $scope.company.companyId, $scope.plant.plantId, function (result) {
            $scope.divisionList = result;
        });
    };

    $scope.GetEntityDetailFrDivisionCbo = function () {
        $scope.entityList = [];
        if ($scope.division.divisionId === "") {
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();
        }
        else {
            $scope.entityList = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFrDivisionCbo',
                params: {
                    'divisionId': $scope.division.divisionId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId,
                    'plantId': $scope.plant.plantId,
                    'entityId': $scope.entity.entityId

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.EntityWiseDivisionList = response.data;
                $scope.companyGroup.companyGroupId = $scope.EntityWiseDivisionList[0].CompanyGroupId;
                $scope.company.companyId = $scope.EntityWiseDivisionList[0].CompanyId;
                $scope.plant.plantId = $scope.EntityWiseDivisionList[0].PlantId;
                $scope.entity.entityId = $scope.EntityWiseDivisionList[0].entityId;

                var row = {
                    Name: $scope.EntityWiseDivisionList[0].CompanyGroup,
                    value: "Group"
                };
                $scope.entityList.push(row);

                var rowc = {
                    Name: $scope.EntityWiseDivisionList[0].CompanyName,
                    value: "Company"
                };
                $scope.entityList.push(rowc);


                var rowp = {
                    value: "Plant",
                    Name: $scope.EntityWiseDivisionList[0].Plant
                };
                $scope.entityList.push(rowp);

                $scope.entityList.push(rowe);


                var rowd = {
                    value: "Division",
                    Name: $scope.EntityWiseDivisionList[0].Division
                };

                $scope.entityList.push(rowd);

                var rowe = {
                    value: "Entity",
                    Name: $scope.EntityWiseDivisionList[0].Entity
                };
                $scope.entityList.push(rowe);
            });
        }
    };

    $scope.GetEntityWiseSubDivisionCbo = function () {
        cboService.getCboEntityWiseSubDivision($scope.companyGroup.companyGroupId, $scope.company.companyId, $scope.plant.plantId, $scope.division.divisionId, function (result) {
            $scope.subDivisionList = result;
        });
    };
    $scope.GetEntityWiseSubDivisionCbo();


    $scope.GetEntityDetailFromSubDivisionCbo = function () {
        $scope.entityList = [];
        if ($scope.subDivision.subDivisionId === "") {
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();
        }
        else {
            $scope.entityList = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFromSubDivisionCbo',
                params: {
                    'subDivisionId': $scope.subDivision.subDivisionId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId,
                    'plantId': $scope.plant.plantId,
                    'entityId': $scope.entity.entityId,
                    'divisionId': $scope.division.divisionId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.EntityWiseSubDivisionList = response.data;

                $scope.companyGroup.companyGroupId = $scope.EntityWiseSubDivisionList[0].CompanyGroupId;
                $scope.company.companyId = $scope.EntityWiseSubDivisionList[0].CompanyId;
                $scope.plant.plantId = $scope.EntityWiseSubDivisionList[0].PlantId;
                $scope.division.divisionId = $scope.EntityWiseSubDivisionList[0].DivisionId;
                $scope.entity.entityId = $scope.EntityWiseSubDivisionList[0].entityId;

                var row = {
                    Name: $scope.EntityWiseSubDivisionList[0].CompanyGroup,
                    value: "Group"
                };
                $scope.entityList.push(row);

                var rowc = {
                    Name: $scope.EntityWiseSubDivisionList[0].Company,
                    value: "Company"
                };
                $scope.entityList.push(rowc);


                var rowp = {
                    value: "Plant",
                    Name: $scope.EntityWiseSubDivisionList[0].Plant
                };
                $scope.entityList.push(rowp);



                var rowd = {
                    value: "Division",
                    Name: $scope.EntityWiseSubDivisionList[0].Division
                };
                $scope.entityList.push(rowd);
                var rowsd = {
                    value: "SubDivision",
                    Name: $scope.EntityWiseSubDivisionList[0].SubDivision
                };
                $scope.entityList.push(rowsd);

                var rowe = {
                    value: "Entity",
                    Name: $scope.EntityWiseSubDivisionList[0].Entity
                };
                $scope.entityList.push(rowe);
            });
        }
    };

    $scope.GetEntityWiseUnitCbo = function () {
        cboService.getCboEntityWiseUnit($scope.companyGroup.companyGroupId, $scope.company.companyId, $scope.plant.plantId, $scope.division.divisionId, $scope.subDivision.subDivisionId, function (result) {
            $scope.unitList = result;
        });
    };
    $scope.GetEntityWiseUnitCbo();

    $scope.GetEntityDetailFromUnitCbo = function () {
        $scope.entityList = [];
        if ($scope.unit.unitId === "") {
            //$scope.companyGroup.companyGroupId = null;
            //$scope.company.companyId = null;
            //$scope.plant.plantId = null;
            //$scope.division.divisionId = null;
            //$scope.subDivision.subDivisionId = null;
            //$scope.entity.entityId = null;

            //$scope.lblCompanyGroup = null;
            //$scope.lblCompany = null;
            //$scope.lblPlant = null;
            //$scope.lblDivision = null;
            //$scope.lblsubDivision = null;
            //$scope.lblUnit = null;
            $scope.clearEntityFilter();
            $scope.GetBudgetWisevarianceElastic();

        }

        else {
            $scope.entityList = [];
            $http({
                method: 'GET',
                url: 'Accounts/MISAccountDashboard/GetEntityDetailFromUnitCbo',
                params: {
                    'unitId': $scope.unit.unitId,
                    'companyGroupId': $scope.companyGroup.companyGroupId,
                    'companyId': $scope.company.companyId,
                    'plantId': $scope.plant.plantId,
                    'entityId': $scope.entity.entityId,
                    'divisionId': $scope.division.divisionId,
                    'subDivisionId': $scope.subDivision.subDivisionId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                $scope.EntityWiseUnitList = response.data;

                $scope.companyGroup.companyGroupId = $scope.EntityWiseUnitList[0].CompanyGroupId;
                $scope.company.companyId = $scope.EntityWiseUnitList[0].CompanyId;
                $scope.plant.plantId = $scope.EntityWiseUnitList[0].PlantId;
                $scope.division.divisionId = $scope.EntityWiseUnitList[0].DivisionId;
                $scope.subDivision.subDivisionId = $scope.EntityWiseUnitList[0].SubDivisionId;
                $scope.entity.entityId = $scope.EntityWiseUnitList[0].entityId;

                var row = {
                    Name: $scope.EntityWiseUnitList[0].CompanyGroup,
                    value: "Group"
                };
                $scope.entityList.push(row);

                var rowc = {
                    Name: $scope.EntityWiseUnitList[0].Company,
                    value: "Company"
                };
                $scope.entityList.push(rowc);


                var rowp = {
                    value: "Plant",
                    Name: $scope.EntityWiseUnitList[0].Plant
                };
                $scope.entityList.push(rowp);

                var rowd = {
                    value: "Division",
                    Name: $scope.EntityWiseUnitList[0].Division
                };

                $scope.entityList.push(rowd);

                var rowsd = {
                    value: "SubDivision",
                    Name: $scope.EntityWiseUnitList[0].subDivision
                };
                $scope.entityList.push(rowsd);

                var rowu = {
                    value: "Unit",
                    Name: $scope.EntityWiseUnitList[0].Unit

                };
                $scope.entityList.push(rowu);

                var rowe = {
                    value: "Entity",
                    Name: $scope.EntityWiseUnitList[0].Entity
                };
                $scope.entityList.push(rowe);

            });
        }
    };

    $scope.clearEntityFilter = function () {
        $scope.company.companyId = null;
        $scope.plant.plantId = null;
        $scope.division.divisionId = null;
        $scope.subDivision.subDivisionId = null;
        $scope.unit.unitId = null;
        $scope.entity.entityId = null;
        $scope.entityList = [];
        // $scope.lblStringCompanyGroupChange();
        $scope.lblStringCompanyChange();
        $scope.lblStringPlantChange();
    };

    $scope.GetBudgetWiseAmountListElastic = function (data) {
        $scope.VoucherId = data.VoucherId;
        $scope.voucherNo = data.VoucherNo;
        $scope.BudgetWiseAmountList = [];
        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetWiseAmountListElastic/',
            params: {
                'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId,
                'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetMasterId': '', 'Activity': null,
                'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'voucherId': data.VoucherId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetWiseAmountList = response.data;

            angular.element(document.querySelector("#ModalBudgetMasterDetail")).modal('show');

        });
    };
    //------------------Elastic Search----------------------------//

    $scope.setClickedRow = function (index) {
        $scope.selectedRow = index;
    };
    $scope.GetVoucherLatestDate = function () {
        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetVoucherLatestDate/',
            params: { 'dateType': $scope.expFactDate.factDate, 'itemType': $scope.budgetType },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.getFromDate = response.data;
            $scope.dateRange.fromDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);
            $scope.dateRange.toDate = $filter("dateFiltering")($scope.getFromDate[0].PostingDate);

            //$scope.GetBudgetWisevarianceElastic();
        });
    };

    $scope.GetVoucherLatestDate();

    $scope.EntityListFormData = [];
    var entityList = [];
    $scope.BudgetResponsiblePerson = [];
    $scope.BudgetWiseVarianceList = [];
    var varBudgetWiseVarianceList;
    var ctgWiseVarianceList = null;
    var EntityWiseVarianceList;

    $scope.GetBudgetWisevarianceElasticse = function () {
        EntityWiseVarianceList = [];
        $scope.budgetCategoryWiseAmountList = [];
        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetBudgetWisevarianceElastic/',
            params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            //getting data for filteration   
            $scope.BudgetWiseVarianceList = response.data;
            varBudgetWiseVarianceList = response.data;
            let array = response.data;

            if (baseService.arrayLength(varBudgetWiseVarianceList) > 0) {

                entityList = array.map(item => item.EntityId)
                    .filter((value, index, self) => self.indexOf(value) === index);

                $scope.GetEntityWiseEntityCbo(entityList);

                EntityWiseVarianceList = $scope.BudgetWiseVarianceList;

                $scope.BudgetResponsiblePerson = removeDuplicates(response.data, "EmployeeId");
            }
            else {
                EntityWiseVarianceList = [];

                throw ShowResult("There is no data between this dates range, please input another date range", 'failure');
            }
            $scope.budgetCategoryList = [];

            for (var i = 0; i < baseService.arrayLength(EntityWiseVarianceList); i++) {
                if ($scope.budgetCategoryList.indexOf(EntityWiseVarianceList[i].budgetCategoryId) === -1) {
                    $scope.budgetCategoryList.push(EntityWiseVarianceList[i].budgetCategoryId);
                }
            }
            var totalAmount = 0;
            var totalExAmount = 0;
            var totalExcessAmount = 0;
            var totalShortAmount = 0;
            var totalExceptionPosting = 0;
            var totalBudgetAmount = 0;

            var budgetCategoryId = null;
            var budgetCategoryName = null;
            var budgetMasterList = [];
            $scope.budgetCategoryWiseAmountList = [];

            for (var cti = 0; cti < baseService.arrayLength($scope.budgetCategoryList); cti++) {

                budgetCategoryId = null;
                budgetMasterList = [];
                totalAmount = 0;
                totalExAmount = 0;
                totalExcessAmount = 0;
                totalShortAmount = 0;
                totalExceptionPosting = 0;
                totalBudgetAmount = 0;

                for (var jti = 0; jti < baseService.arrayLength(EntityWiseVarianceList); jti++) {
                    if ($scope.budgetCategoryList[cti] === EntityWiseVarianceList[jti].budgetCategoryId) {
                        budgetCategoryName = null;
                        budgetCategoryName = EntityWiseVarianceList[jti].BudgetCategoryName;
                        budgetMasterList.push(EntityWiseVarianceList[jti].BudgetMasterId);
                        totalAmount += EntityWiseVarianceList[jti].Amount;
                        totalExAmount += EntityWiseVarianceList[jti].ExAmount;
                        totalExcessAmount += EntityWiseVarianceList[jti].ExcessAmount;
                        totalShortAmount += EntityWiseVarianceList[jti].ShortAmount;
                        totalExceptionPosting += EntityWiseVarianceList[jti].ExceptionPosting;
                        totalBudgetAmount += EntityWiseVarianceList[jti].BudgetAmount;
                    }
                }
                var row =
                    {
                        budgetCategoryId: $scope.budgetCategoryList[cti],
                        budgetCategoryName: budgetCategoryName,
                        budgetMasterList: budgetMasterList,
                        Amount: totalAmount,
                        ExAmount: totalExAmount,
                        ExcessAmount: totalExcessAmount,
                        ShortAmount: totalShortAmount,
                        ExceptionPosting: totalExceptionPosting,
                        BudgetAmount: totalBudgetAmount
                    };
                $scope.budgetCategoryWiseAmountList.push(row);
            }
            for (var bci = 0; bci < baseService.arrayLength($scope.budgetCategoryList); bci++) {
                ctgWiseVarianceList = EntityWiseVarianceList.filter(function (EntityWiseVarianceList) {
                    return EntityWiseVarianceList.budgetCategoryId === "" + $scope.budgetCategoryList[bci] + "";
                });
                $scope.ctgWiseVarianceList.push(ctgWiseVarianceList);
            }

            console.log("Ctg wise budget Master Id", $scope.budgetCategoryWiseAmountList);

        });

    };
    $scope.budgetSubcategoryWiseAmountList = [];
    $scope.getSubCtgWisedetail = function (catgId) {

        $scope.budgetSubcategoryWiseAmountList = [];
        var subCtgWiseWiseVarianceList = [];
        $scope.budgetSubCategoryList = [];

        subCtgWiseWiseVarianceList = EntityWiseVarianceList.filter(function (EntityWiseVarianceList) {
            return EntityWiseVarianceList.budgetCategoryId === "" + catgId + "";
        });

        for (var i = 0; i < baseService.arrayLength(subCtgWiseWiseVarianceList); i++) {
            if ($scope.budgetSubCategoryList.indexOf(subCtgWiseWiseVarianceList[i].budgetSubcategoryId) === -1) {
                $scope.budgetSubCategoryList.push(subCtgWiseWiseVarianceList[i].budgetSubcategoryId);
            }
        }

        var totalAmount = 0;
        var totalExAmount = 0;
        var totalExcessAmount = 0;
        var totalShortAmount = 0;
        var totalExceptionPosting = 0;
        var totalBudgetAmount = 0;

        var budgetSubcategoryId = null;
        var budgetCategoryId = null;
        var budgetSubcategoryName = null;
        var scBudgetMasterList = [];

        for (var csti = 0; csti < baseService.arrayLength($scope.budgetSubCategoryList); csti++) {
            budgetSubcategoryId = null;
            totalAmount = 0;
            totalExAmount = 0;
            totalExcessAmount = 0;
            totalShortAmount = 0;
            totalExceptionPosting = 0;
            totalBudgetAmount = 0;

            for (var jsti = 0; jsti < baseService.arrayLength(subCtgWiseWiseVarianceList); jsti++) {

                if ($scope.budgetSubCategoryList[csti] === subCtgWiseWiseVarianceList[jsti].budgetSubcategoryId) {
                    budgetSubcategoryName = null;
                    budgetCategoryId = null;
                    budgetCategoryId = subCtgWiseWiseVarianceList[jsti].budgetCategoryId;
                    budgetSubcategoryName = subCtgWiseWiseVarianceList[jsti].BudgetSubCategoryName;
                    scBudgetMasterList.push(subCtgWiseWiseVarianceList[jsti].BudgetMasterId);
                    totalAmount += subCtgWiseWiseVarianceList[jsti].Amount;
                    totalExAmount += subCtgWiseWiseVarianceList[jsti].ExAmount;
                    totalExcessAmount += subCtgWiseWiseVarianceList[jsti].ExcessAmount;
                    totalShortAmount += subCtgWiseWiseVarianceList[jsti].ShortAmount;
                    totalExceptionPosting += subCtgWiseWiseVarianceList[jsti].ExceptionPosting;
                    totalBudgetAmount += subCtgWiseWiseVarianceList[jsti].BudgetAmount;
                }
            }
            var row =
                {
                    budgetCategoryId: budgetCategoryId,
                    budgetSubcategoryId: $scope.budgetSubCategoryList[csti],
                    BudgetSubCategoryName: budgetSubcategoryName,
                    budgetMasterList: scBudgetMasterList,
                    Amount: totalAmount,
                    ExAmount: totalExAmount,
                    ExcessAmount: totalExcessAmount,
                    ShortAmount: totalShortAmount,
                    ExceptionPosting: totalExceptionPosting,
                    BudgetAmount: totalBudgetAmount
                };
            $scope.budgetSubcategoryWiseAmountList.push(row);
        }
    };
    $scope.ItemWiseBudgetDetail = [];
    //$scope.getItemWisedetail = function (catgId, subCtgId) {
    //    $scope.ItemWiseBudgetDetail = $filter('filter')(EntityWiseVarianceList, { budgetCategoryId: catgId, budgetSubcategoryId: subCtgId }, true);
    //};

    var b = true;

    $scope.XLfilters = { list: [], dict: {}, results: [] };

    $scope.markAll = function (field, b) {
        $scope.XLfilters.dict[field].list.forEach((x) => { x.checked = b; });
    };
    $scope.clearAll = function (field) {
        $scope.XLfilters.dict[field].searchText = '';
        $scope.XLfilters.dict[field].list.forEach((x) => { x.checked = true; });
    };
    $scope.XLfiltrate = function () {
        var i, j, k, selected, blocks, filter, option, data = $scope.XLfilters.all, filters = $scope.XLfilters.list;
        $scope.XLfilters.results = [];
        for (j = 0; j < filters.length; j++) {
            filter = filters[j];
            filter.regex = filter.searchText.length ? new RegExp(filter.searchText, 'i') : false;
            for (k = 0, selected = 0; k < filter.list.length; k++) {
                if (!filter.list[k].checked) selected++;
                filter.list[k].visible = false;
                filter.list[k].match = filter.regex ? filter.list[k].title.match(filter.regex) : true;
            }
            filter.isActive = filter.searchText.length > 0 || selected > 0;
        }
        for (i = 0; i < baseService.arrayLength(data); i++) {
            blocks = { allows: [], rejects: [], mismatch: false };
            for (j = 0; j < baseService.arrayLength(filters); j++) {
                filter = filters[j];

                option = filter.dict[data[i][filter.field]];

                if (option !== undefined) {

                    (option.checked ? blocks.allows : blocks.rejects).push(option);

                    if (filter.regex && !option.match) {
                        blocks.mismatch = true;
                    }
                }

            }
            if (blocks.rejects.length === 1) blocks.rejects[0].visible = true;
            else if (blocks.rejects.length === 0 && !blocks.mismatch) {
                $scope.XLfilters.results.push(data[i]);
                blocks.allows.forEach((x) => { x.visible = true; });
            }
        }
        for (j = 0; j < filters.length; j++) {
            filter = filters[j]; filter.options = [];
            for (k = 0; k < filter.list.length; k++) {
                if (filter.list[k].visible && filter.list[k].match) filter.options.push(filter.list[k]);
            }
        }
    };

    function createXLfilters(arr, fields) {
        $scope.XLfilters.all = arr;
        for (var j = 0; j < fields.length; j++) $scope.XLfilters.list.push($scope.XLfilters.dict[fields[j]] = { list: [], dict: {}, field: fields[j], searchText: "", active: false, options: [] });
        for (var i = 0, z; i < baseService.arrayLength(arr); i++) for (j = 0; j < fields.length; j++) {
            z = $scope.XLfilters.dict[fields[j]];
            z.dict[arr[i][fields[j]]] || z.list.push(z.dict[arr[i][fields[j]]] = { title: arr[i][fields[j]], checked: true, visible: false, match: false });
        }
    }

    $scope.GetBudgetWisevarianceList = function () {
        $scope.BudgetWiseVarianceList = [];
        EntityWiseVarianceList = [];

        $scope.varianceList = varBudgetWiseVarianceList;

        document.getElementById("postingDate").disabled = true;
        document.getElementById("entryDate").disabled = true;

        if (baseService.arrayLength($scope.varianceList) > 0) {
            EntityWiseVarianceList = $scope.varianceList;

            console.log("EntityWiseVarianceList", EntityWiseVarianceList);
            createXLfilters(EntityWiseVarianceList, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);

            $scope.dashBoardChangeMIS(EntityWiseVarianceList);
        }
        else {
            EntityWiseVarianceList = [];
        }
        createXLfilters(EntityWiseVarianceList, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);
    };
    $scope.GetResponsiblePersonWiseVarianceList = function () {
        $scope.BudgetWiseVarianceList = [];
        if ($scope.budgetResponsiblePersonId === "" || $scope.budgetResponsiblePersonId === null) {
            $scope.BudgetWiseVarianceList = EntityWiseVarianceList;
        }
        else {
            $scope.BudgetWiseVarianceList = EntityWiseVarianceList.filter(function (varianceList) {
                return varianceList.EmployeeId === "" + $scope.budgetResponsiblePersonId + "";
            });
        }
        createXLfilters($scope.BudgetWiseVarianceList, ['BudgetCategoryName', 'BudgetSubCategoryName', 'BudgetName']);
    };
    $scope.GetActivityWisevarianceElastic = function (x) {
        misAccountDiv1.style.display = "none";
        misAccountActivityDetail.style.display = "block";
        backButtonMisAccountDivActivityDetail.style.display = "block";
        orgStrSelectBox.style.display = "none";
        var budgetmasterList = [];
        if (typeof x.budgetMasterList !== "undefined") {
            budgetmasterList = x.budgetMasterList;
        }
        else {
            budgetmasterList.push(x.BudgetMasterId);
        }
        $scope.isActivity = true;

        $scope.overActivityStatusList = [];
        $http({
            method: 'GET',
            url: 'Accounts/MISAccountDashboard/GetActivityWisevarianceElastic/',
            params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'budgetMasterId': budgetmasterList, 'budgetCategoryId': x.budgetCategoryId, 'dateType': $scope.expFactDate.factDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.overActivityStatusList = response.data;
        });
    };
    $scope.generalVoucherReport = function (VoucherId) {
        location.href = 'Accounts/Voucher/GetDashBoardJournalVoucherReport?reportFormat='+'Pdf'+'&voucherId=' + VoucherId;
    };
    $scope.GroupColumns = ["BudgetCategoryName", "BudgetSubCategoryName", "BudgetName"];
    $scope.summaryRows = [{
        title: "Total Amount", summaryColumns: [
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ExAmount", textAlign: ej.TextAlign.Right, dataMember: "ExAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "Amount", textAlign: ej.TextAlign.Right, dataMember: "Amount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "BudgetAmount", textAlign: ej.TextAlign.Right, dataMember: "BudgetAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ShortAmount", textAlign: ej.TextAlign.Right, dataMember: "ShortAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ExcessAmount", textAlign: ej.TextAlign.Right, dataMember: "ExcessAmount", format: "{0:N2}" },
            { summaryType: ej.Grid.SummaryType.Sum, displayColumn: "ExceptionPosting", textAlign: ej.TextAlign.Right, dataMember: "ExceptionPosting", format: "{0:N2}" }
        ],
        showCaptionSummary: true
    }];
    $scope.tung = function (args) {
        if (args.cellIndex[0] === 1) {
            $scope.getBudgetMasterWiseAmount(args.data, "day");
        }
        if (args.cellIndex[0] === 2) {
            $scope.getBudgetMasterWiseAmount(args.data, "period");
        }
        if (args.cellIndex[0] === 6) {
            $scope.misAccountExceptionPeriod(args.data);
        }
    };

    $scope.BudgetCategoryDataList = [];
    $scope.BudgetSubCategoryDataList = [];
    $scope.BudgetItemDataList = [];







    $scope.captionFormat = "{{:key}} : ({{:count}})";
    //------------------------====--------------
    $scope.filterDataList = [];
    $scope.budgetCategoryWiseAmountList = [];

    $scope.GetMasterFilterationData = function () {
        $http({
            method: 'POST',
            url: 'Accounts/MISAccountDashboard/GetBudgetWisevarianceElastic/',
            params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filterDataList = response.data;

            $("#EntityFilterGrid").children('.e-pager.e-js.e-pager').hide();
            $("#EntityFilterGrid").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#EntityFilterGrid").children('.e-gridcontent').hide();
            $("#EntityFilterGrid").children('.e-grid.e-headercell').css('background-color', 'red');
            $scope.budgetCategoryWiseAmountList = [];
        });
        $http({
            method: 'POST',
            url: 'Accounts/MISAccountDashboard/GetBudgetCategoryWisevarianceElastic/',
            params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BudgetCategoryDataList = response.data;

            $http({
                method: 'POST',
                url: 'Accounts/MISAccountDashboard/GetBudgetSubCategoryWisevarianceElastic/',
                params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetSubCategoryDataList = response.data;
                $http({
                    method: 'POST',
                    url: 'Accounts/MISAccountDashboard/GetBudgetItemWisevarianceElastic/',
                    params: { 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.BudgetItemDataList = response.data;

                    $scope.loadGrid($scope.BudgetCategoryDataList, $scope.BudgetSubCategoryDataList, $scope.BudgetItemDataList);
                });
            });

        });

    };




    $scope.loadGrid = function (categoryData, subCategoryData, itemData) {
        //$scope.BudgetCategoryDataList = [];
        //$scope.BudgetItemDataList = [];
        //$scope.BudgetSubCategoryDataList = [];
       

        $scope.BudgetCategoryDataList = categoryData;
        $scope.BudgetItemDataList = itemData;
        $scope.BudgetSubCategoryDataList = subCategoryData;

      
        var gridObj = $("#Grid").data("ejGrid");

        if (gridObj !== undefined && typeof gridObj === 'object' && typeof gridObj.destroy === 'function') gridObj.destroy();

        $("#Grid").ejGrid({
            dataSource: $scope.BudgetCategoryDataList,
            showSummary: true,
            summaryRows: $scope.summaryRows,
            allowSelection: true,
            selectionType: ej.Grid.SelectionType.Single,
            selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
            cellSelected: $scope.tung,
            columns: [
                { field: "BudgetCategoryName", headerText: 'Category', textAlign: ej.TextAlign.Left, width: 75 },
                { field: "ExAmount", headerText: 'Expense of the day', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                { field: "Amount", headerText: 'Expense of the period', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                { field: "BudgetAmount", headerText: 'Budget of the period', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                { field: "ShortAmount", headerText: 'ShortAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                { field: "ExcessAmount", headerText: 'ExcessAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                { field: "ExceptionPosting", headerText: 'Delay Posting', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" }
            ],
            childGrid: {
                dataSource: $scope.BudgetSubCategoryDataList,
                queryString: "budgetCategoryId",
                showSummary: true,
                summaryRows: $scope.summaryRows,
                allowSelection: true,
                selectionType: ej.Grid.SelectionType.Single,
                selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                cellSelected: $scope.tung,
                columns: [
                    { field: "BudgetSubCategoryName", headerText: 'SubCategory', textAlign: ej.TextAlign.Left, width: 120 },
                    { field: "ExAmount", headerText: 'Expense of the day', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                    { field: "Amount", headerText: 'Expense of the period', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                    { field: "BudgetAmount", headerText: 'Budget of the period', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                    { field: "ShortAmount", headerText: 'ShortAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                    { field: "ExcessAmount", headerText: 'ExcessAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                    { field: "ExceptionPosting", headerText: 'Delay Posting', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" }
                ],
                childGrid: {
                    dataSource: $scope.BudgetItemDataList,
                    queryString: "budgetSubcategoryId",
                    showSummary: true,
                    summaryRows: $scope.summaryRows,
                    allowSelection: true,
                    selectionType: ej.Grid.SelectionType.Single,
                    selectionSettings: { selectionMode: ["cell"], cellSelectionMode: ej.Grid.CellSelectionMode.Box },
                    cellSelected: $scope.tung,
                    columns: [
                        { field: "BudgetName", headerText: 'Budget', textAlign: ej.TextAlign.Left, width: 120 },
                        { field: "ExAmount", headerText: 'Expense for the day', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                        { field: "Amount", headerText: 'Expense for the amount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                        { field: "BudgetAmount", headerText: 'Budget for the amount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                        { field: "ShortAmount", headerText: 'ShortAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                        { field: "ExcessAmount", headerText: 'ExcessAmount', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" },
                        { field: "ExceptionPosting", headerText: 'Delay posting', textAlign: ej.TextAlign.Right, width: 100, format: "{0:N2}" }
                    ]
                }
            }
        }).render();

       

       
        //gridObj.refreshData(); 
        //gridObj.refreshContent();
    };

    $scope.budgetSubcategoryWiseAmountList = [];
    $scope.getSubCtgWisedetail = function (x) {

        var subCategoryListDDD = $scope.budgetCategoryWiseAmountList;
        var subCtgWiseWiseVarianceList = [];
        $scope.budgetSubCategoryList = [];
        $scope.budgetSubcategoryWiseAmountList = [];

        subCtgWiseWiseVarianceList = EntityWiseVarianceList.filter(function (EntityWiseVarianceList) {
            return EntityWiseVarianceList.budgetCategoryId === "" + x.budgetCategoryId + "";
        });

        for (var i = 0; i < baseService.arrayLength(subCtgWiseWiseVarianceList); i++) {
            if ($scope.budgetSubCategoryList.indexOf(subCtgWiseWiseVarianceList[i].budgetSubcategoryId) === -1) {
                $scope.budgetSubCategoryList.push(subCtgWiseWiseVarianceList[i].budgetSubcategoryId);
            }
        }

        var totalAmount = 0;
        var totalExAmount = 0;
        var totalExcessAmount = 0;
        var totalShortAmount = 0;
        var totalExceptionPosting = 0;
        var totalBudgetAmount = 0;

        var budgetSubcategoryId = null;
        var budgetCategoryId = null;
        var budgetSubcategoryName = null;
        var scBudgetMasterList = [];

        for (var csti = 0; csti < baseService.arrayLength($scope.budgetSubCategoryList); csti++) {
            budgetSubcategoryId = null;
            totalAmount = 0;
            totalExAmount = 0;
            totalExcessAmount = 0;
            totalShortAmount = 0;
            totalExceptionPosting = 0;
            totalBudgetAmount = 0;

            for (var jsti = 0; jsti < baseService.arrayLength(subCtgWiseWiseVarianceList); jsti++) {

                if ($scope.budgetSubCategoryList[csti] === subCtgWiseWiseVarianceList[jsti].budgetSubcategoryId) {
                    if (budgetSubcategoryName !== subCtgWiseWiseVarianceList[jsti].BudgetCategoryName) {
                        budgetCategoryId = subCtgWiseWiseVarianceList[jsti].budgetCategoryId;
                        budgetSubcategoryName = subCtgWiseWiseVarianceList[jsti].BudgetSubCategoryName;
                    }
                    scBudgetMasterList.push(subCtgWiseWiseVarianceList[jsti].BudgetMasterId);
                    totalAmount += subCtgWiseWiseVarianceList[jsti].Amount;
                    totalExAmount += subCtgWiseWiseVarianceList[jsti].ExAmount;
                    totalExcessAmount += subCtgWiseWiseVarianceList[jsti].ExcessAmount;
                    totalShortAmount += subCtgWiseWiseVarianceList[jsti].ShortAmount;
                    totalExceptionPosting += subCtgWiseWiseVarianceList[jsti].ExceptionPosting;
                    totalBudgetAmount += subCtgWiseWiseVarianceList[jsti].BudgetAmount;
                }
            }
            var row =
                {
                    budgetCategoryId: budgetCategoryId,
                    budgetSubcategoryId: $scope.budgetSubCategoryList[csti],
                    BudgetSubCategoryName: budgetSubcategoryName,
                    budgetMasterList: scBudgetMasterList,
                    Amount: totalAmount,
                    ExAmount: totalExAmount,
                    ExcessAmount: totalExcessAmount,
                    ShortAmount: totalShortAmount,
                    ExceptionPosting: totalExceptionPosting,
                    BudgetAmount: totalBudgetAmount
                };
            $scope.budgetSubcategoryWiseAmountList.push(row);
        }
    };

    $scope.getItemWisedetail = function (catgId, subCtgId) {
        $scope.ItemWiseBudgetDetail = $filter('filter')(EntityWiseVarianceList, { budgetCategoryId: catgId, budgetSubcategoryId: subCtgId }, true);
    };

    //---------------==----==--=----------------

    $scope.GetEmployeeWiseOptionalOrMandatoryDocumentList = function (x, OptionalOrMandatory) {
        $scope.EmployeeWiseDocList = [];
        $http({
            method: 'POST',
            url: 'employees/DocumentDashboard/GetEmployeeWiseOptionalOrMandatoryDocumentList',
            params: {
                'parameterString': sqlInStatement,
                'employeeId': x.data.EmployeeId,
                'documentType': x.data.DocumentType,
                'OptionalOrMandatory': OptionalOrMandatory,
                'segment': $scope.segmentEmp
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EmployeeWiseDocList = response.data;
        });
        $scope.dataGrid = "#OverAllOverDueDoc";

        angular.element(document.querySelector('#EmpDocModal')).modal('show');
    };

    $scope.actionCompleteSelected = function (args) {
        if (args.requestType === "filtering") {
            var gridObj = $("#EntityFilterGrid").ejGrid("instance");
            var filtereddata = gridObj.getFilteredRecords();
            var sqlInStatement = "";
            var uniqueCompanyGroup = removeDuplicates(filtereddata, 'cmpGroupId');
            var uniqueCompany = removeDuplicates(filtereddata, 'companyId');
            var uniquePlant = removeDuplicates(filtereddata, 'plantId');
            var uniqueDivision = removeDuplicates(filtereddata, 'divisionId');
            var uniqueSubDivision = removeDuplicates(filtereddata, 'subDivId');
            var uniqueUnit = removeDuplicates(filtereddata, 'unitId');
            var uniqueEntity = removeDuplicates(filtereddata, 'EntityId');

            var wcCmpGroup = "";
            if (uniqueCompanyGroup.length > 0) {
                wcCmpGroup = "AND ISNULL(cmpGrp.Id,'null') IN(";
                wcCmpGroup += Array.prototype.map.call(uniqueCompanyGroup, function (item) { return "'" + item.cmpGroupId + "'"; }).join(",") + ")";
            }
            var wcCompany = "";
            if (uniqueCompany.length > 0) {
                wcCompany = "AND ISNULL(Cmp.Id,'null') IN(";
                wcCompany += Array.prototype.map.call(uniqueCompany, function (item) { return "'" + item.companyId + "'"; }).join(",") + ")";
            }
            var wcPlant = "";
            if (uniquePlant.length > 0) {
                wcPlant = " AND ISNULL (ENT.PlantId,'null') IN(";
                wcPlant += Array.prototype.map.call(uniquePlant, function (item) { return "'" + item.plantId + "'"; }).join(",") + ")";
            }
            var wcDivision = "";
            if (uniqueDivision.length > 0) {
                wcDivision = " AND ISNULL(ENT.DivisionId,'null') IN(";
                wcDivision += Array.prototype.map.call(uniqueDivision, function (item) { return "'" + item.divisionId + "'"; }).join(",") + ")";
            }
            var wcSubDivision = "";
            if (uniqueSubDivision.length > 0) {
                wcSubDivision = " AND ISNULL(Ent.SubDivisionId,'') IN(";
                wcSubDivision += Array.prototype.map.call(uniqueSubDivision, function (item) { return "'" + item.subDivId + "'"; }).join(",") + ")";
            }
            var wcUnit = "";
            if (uniqueUnit.length > 0) {
                wcUnit = " AND ISNULL(Ent.UnitId,'null') IN(";
                wcUnit += Array.prototype.map.call(uniqueUnit, function (item) { return "'" + item.unitId + "'"; }).join(",") + ")";
            }
            var wcEntity = "";
            if (uniqueEntity.length > 0) {
                wcEntity = " AND ISNULL(Ent.Id,'null') IN(";
                wcEntity += Array.prototype.map.call(uniqueEntity, function (item) { return "'" + item.EntityId + "'"; }).join(",") + ")";
            }


            sqlInStatement = wcCmpGroup + wcCompany + wcPlant + wcDivision + wcSubDivision + wcUnit + wcEntity;

            $http({
                method: 'POST',
                url: 'Accounts/MISAccountDashboard/GetBudgetCategoryWisevarianceElastic/',
                params: { 'parameterString': sqlInStatement, 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BudgetCategoryDataList = response.data;

                $http({
                    method: 'POST',
                    url: 'Accounts/MISAccountDashboard/GetBudgetSubCategoryWisevarianceElastic/',
                    params: { 'parameterString': sqlInStatement, 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    $scope.BudgetSubCategoryDataList = response.data;
                    $http({
                        method: 'POST',
                        url: 'Accounts/MISAccountDashboard/GetBudgetItemWisevarianceElastic/',
                        params: { 'parameterString': sqlInStatement, 'companyGroupId': $scope.companyGroup.companyGroupId, 'companyId': $scope.company.companyId, 'plantId': $scope.plant.plantId, 'divisionId': $scope.division.divisionId, 'subDivisionId': $scope.subDivision.subDivisionId, 'unitId': $scope.unit.unitId, 'budgetCategory': $scope.budgetCategoryId, 'budgetSubCategory': $scope.budgetSubCategoryId, 'budget': $scope.budgetId, 'Activity': null, 'fromDate': $scope.dateRange.fromDate, 'toDate': $scope.dateRange.toDate, 'bType': $scope.budgetType, 'dateType': $scope.expFactDate.factDate },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        $scope.BudgetItemDataList = response.data;
                        $scope.loadGrid($scope.BudgetCategoryDataList, $scope.BudgetSubCategoryDataList, $scope.BudgetItemDataList);
                    });
                });

            });

        }
    };


    $scope.GetVoucharDetailJS = function (data) {
        var reportFormat = "Pdf";
        var file_src = "";
        //if (baseService.isUndefinedOrNull(data.data.VoucherId))
        //    return ShowResult('No Id found', 'failure');
        //else {
        file_src = 'Accounts/VoucherReport/GetCommonVoucherReport?reportFormat=' + 'Pdf' + '&compnayGroupId=' + data[0].CompanyGroupId + '&companyId=' + data[0].CompanyId + '&plantId=' + data[0].PlantId + '&sourceType=' + data[0].SourceType + '&voucherId=' + data[0].VoucherId + '&inventoryIssueId=' + data[0].InventoryIssueId + '&inventoryReceiveId=' + data[0].InventoryReceiveId + '&salesSourceType=' + data[0].SalesSourceType;

            $window.open(file_src, '_blank');
        //}
    };
}

