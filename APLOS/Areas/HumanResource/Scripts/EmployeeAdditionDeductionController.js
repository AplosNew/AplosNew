'use strict';
EmployeeAdditionDeductionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function EmployeeAdditionDeductionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Employee Addition Earnings/Deductions';
    $scope.Action = 'Save';
    $scope.path = 'HumanResource/EmployeeAdditionDeduction/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    var tab = document.getElementById("tab_show");
    tab.style.display = "none";

    function tabShow() {
        if (angular.isUndefinedOrNull($scope.Master.Id)) {
            tab.style.display = "none";
        }
        else {
            tab.style.display = "block";
        }
    }

    $scope.Master = {
        Id:null,
        Type: null,
        Sequence: 0,
        Category: null,
        SubCategory: null,
        StandardName: null,
        UserName: null,
        ShortName: null,
        PolicyRef: null,
        CalculationHeadId: null,
        isFixed: false,
        isPercentage: "Yes",
        AdditionDeductionHeadId: null,
        Amount: 0,
        Period: null,
        Frequency: 0,
        EffectiveDate: null,
        ResponsiblePersonId: null,
        Remarks: null,
        Active: false,
        isHeadApplicable: false,
        HeadValueId: null,
    };

   

   
    // The Tab Switching Code

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

   
    



    //*********************  Operations Staring for the Pages  ******************************\\
    //Getting the Salary Head
    $scope.salaryHeadList = [];
    cboService.getSlrHeadCbo(function (result) {
        $scope.salaryHeadList = result;
    });

    //Getting the Addition Deduction Head
    $scope.AdditionDeductionList = [];
    $scope.fillAdditionDeductionList = function () {
        var type = "";
        if ($scope.Master.Type == "Addition") {
            type = "E";
        }
        if ($scope.Master.Type == "Deduction") {
            type = "D"
        }
        if (angular.isUndefinedOrNull($scope.Master.Type)) {
            ShowResult("Please Select a Type to get Addition/Deduction Head");
            throw ("Invalid");
        }

        $http({
            method: 'POST',
            url: $scope.path + 'getAdditionDeductionHead',
            data: {'Type':type}
        }).then(function succ(resp) {
            $scope.AdditionDeductionList = [];
            $scope.AdditionDeductionList = resp.data;
        });
    }

    //Getting the Master Grid
    $scope.masterList = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getMaster',
        }).then(function succ( resp ){
            $scope.masterList = [];
            $scope.masterList = resp.data;
        });
    }
    $scope.getMaster();

    $scope.getMasterDetails = function (e) {
        $scope.Master = e.data;
        $scope.RespPerson = e.data.ResponsiblePerson;
        $scope.fillAdditionDeductionList();
        if ($scope.Master.isPercentage == true) {
            $scope.Master.isPercentage = "Yes";
        }
        else {
            $scope.Master.isPercentage = "No";
        }

        if ($scope.Master.isHeadApplicable == true) {
            document.getElementById("HeadValue").style.display = "block";
        }
        else {
            document.getElementById("HeadValue").style.display = "none";
        }
        //Calling the Period Child List
        $http({
            method: 'POST',
            url: $scope.path + 'getPeriodChildData',
            data: {'MasterId': $scope.Master.Id}
        }).then(function success(resp){
            
            $scope.periodList = resp.data;

            if ($scope.periodList[0].Period == "Weekly") {
                monthly.style.display = "none";
                weekly.style.display = "block";
            }
            else {
                monthly.style.display = "block";
                weekly.style.display = "none";
            }

            $scope.Action = "Update";
            
            
        })

        //Calling the Plant Child List
        $http({
            method: 'POST',
            url: $scope.path + 'getPlantChildData',
            data: { 'MasterId': $scope.Master.Id }
        }).then(function success(resp) {

            $scope.childDataList = [];
            $scope.childDataList = resp.data;
            $scope.Child.MasterId = $scope.Master.Id;

        })

        tabShow();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    //Checking for more than one entry
    $scope.checkMonth = function () {
        var arr = "";
        for (var i = 0; i < $scope.periodList.length; i++) {
            arr = arr + $scope.periodList[i].Month + " " ;
        }

        for (var i = 0; i < $scope.periodList.length; i++) {
            var j = new RegExp($scope.periodList[i].Month, 'g');
            
            if (arr.match(j).length > 1) {
                ShowResult("One Month Cannot be Chosen More than Once!!");
                throw ("Invalid");
            }
        }
    }
    

    //*********************  Operations for the Master Tab  *************************\\

    //Refreshing the Head Value Html Element
    $scope.refreshHead = function () {
        if ($scope.Master.isHeadApplicable == true) {
            document.getElementById("HeadValue").style.display = "block";
        }
        else {
            document.getElementById("HeadValue").style.display = "none";
        }
    }


    //Getting the Responsible Persons List
    $scope.EmployeesList = [];
    $http({
        method: 'GET',
        url: $scope.path + "getEmployees",
        dataType: 'JSON'
    }).then(function successCallback(response) {
        $scope.EmployeesList = [];
        $scope.EmployeesList = response.data;
    });

    // Selection of the Responsible Persons
    $scope.selectRespPerson = function () {
        angular.element(document.querySelector('#RespPersonModal')).modal('show');
    }
    
    $scope.doubleRespPerson = function (e) {
        $scope.RespPerson = e.data.EmployeeName;
        $scope.Master.ResponsiblePersonId = e.data.SystemId;
        angular.element(document.querySelector('#RespPersonModal')).modal('hide');
    }

    
    //Saving the Master
    $scope.saveMaster = function () {
        $scope.$broadcast('show-errors-check-validity');

        if ($scope.Master.Frequency <= 0 || $scope.Master.Frequency > 12) {
            ShowResult("Frequency cannot be 0 or more than 12!");
            throw ("Invalid");
        }

        if ($scope.Master.isHeadApplicable == true && $scope.Master.HeadValueId == null) {
            ShowResult("Select a Head Value!");
            throw ("Invalid");
        }

        if ($scope.MasterForm.$valid) {

            if ($scope.Master.isPercentage == "Yes") {
                $scope.Master.isPercentage = true;
                $scope.Master.isFixed = false;
            }
            else {
                $scope.Master.isFixed = true;
                $scope.Master.isPercentage = false;
            }

            

            $http({
                method: 'POST',
                url: $scope.path + 'saveMaster',
                data: {'Master' : $scope.Master}
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    if ($scope.Master.isPercentage == true) {
                        $scope.Master.isPercentage = "Yes";
                    }
                    else {
                        $scope.Master.isPercentage = "No";
                    }
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.Master.Id = response.data.Data.Id;
                    $scope.Child.MasterId = $scope.Master.Id;
                    tabShow();
                    $scope.periodList = [];
                    $scope.fillPeriodChild();
                    if ($scope.Master.isPercentage == true) {
                        $scope.Master.isPercentage = "Yes";
                    }
                    else {
                        $scope.Master.isPercentage = "No";
                    }
                    $scope.getMaster();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
                if ($scope.Master.isPercentage == true) {
                    $scope.Master.isPercentage = "Yes";
                }
                else {
                    $scope.Master.isPercentage = "No";
                }
            }
        }
    }

    

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.Master.Sequence = data;
        });
    };
    $scope.GetSequence();
    
    //For Deleting of A Master
    $scope.Delete = function () {
        if ($scope.childDataList.length > 0) {
            ShowResult("There are Child Data in this Master. First Delete Those!", 'failure');
            throw ("There are Child Data in this Master. First Delete Those!");
        }

        $http({
            method: 'POST',
            url: $scope.path + 'deleteMaster',
            data: { 'id': $scope.Master.Id }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getMaster();
                $scope.Clear();
                if ($rootScope.isCollapsed) {
                    $rootScope.toggle();
                }
                $scope.ConvertBool();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    //Clears The Master as well as the Child
    $scope.Clear = function () {
        $scope.Master = {
            Id: null,
            Type: null,
            Sequence: 0,
            Category: null,
            SubCategory: null,
            StandardName: null,
            UserName: null,
            ShortName: null,
            PolicyRef: null,
            CalculationHeadId: null,
            isFixed: false,
            isPercentage: "Yes",
            AdditionDeductionHeadId: null,
            Amount: 0,
            Period: null,
            Frequency: 0,
            EffectiveDate: null,
            ResponsiblePersonId: null,
            Remarks: null,
            Active:false,
        };
        $scope.GetSequence();
        $scope.childDataList = [];
        $scope.Child = {
            Id: null,
            MasterId: null,
            PlantId: null,
            EmpTypeId: null,
            DesignationId: null,
        };
        $scope.periodList = [];
        tabShow();
    }

    
    //**********  Operations for the Child Tab  ********************\\

    $scope.WeekDays = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
    $scope.MonthsList = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];

    var weekly = document.getElementById("Weekly");
    var monthly = document.getElementById("Monthly");
    weekly.style.display = "none";
    monthly.style.display = "none";

    $scope.tryIt = function()
    {
        $scope.fillPeriodChild();
    }


    //Filling the Period Child

    $scope.weeklyList = [];
    $scope.monthlyList = [];

    $scope.periodList = [];

    $scope.fillPeriodChild = function () {
        if ($scope.Master.Period == "Weekly") {
            monthly.style.display = "none";
            weekly.style.display = "block";
            $scope.weeklyList = [];

            var obj = {
                Id: null,
                Seq:0,
                MasterId: $scope.Master.Id,
                Period: $scope.Master.Period,
                WeekDay:null,
            };

            if ($scope.Master.Frequency > 0) {
                for (var i = 0; i < $scope.Master.Frequency; i++) {
                    obj.Seq = i + 1;
                    $scope.weeklyList.push(obj);
                    $scope.periodList.push(obj);
                    var obj = {
                        Id: null,
                        Seq: 0,
                        MasterId: $scope.Master.Id,
                        Period: $scope.Master.Period,
                        WeekDay: null,
                    };
                }
            }
            
        }

        else {
            weekly.style.display = "none";
            monthly.style.display = "block";
            $scope.monthlyList = [];

            var obj = {
                Id: null,
                Seq: 0,
                MasterId: $scope.Master.Id,
                Period: $scope.Master.Period,
                Month: null,
                MonthDay: null,
            };

            if ($scope.Master.Frequency > 0) {
                for (var i = 0; i < $scope.Master.Frequency; i++) {
                    obj.Seq = i + 1;
                    $scope.monthlyList.push(obj);
                    $scope.periodList.push(obj);
                    var obj = {
                        Id: null,
                        Seq: 0,
                        MasterId: $scope.Master.Id,
                        Period: $scope.Master.Period,
                        Month: null,
                        MonthDay: null,
                    };
                }
            }
        }
    }


    //Filling of the Plant And Employee Type and Designation List
    $scope.PlantList = [];
    $scope.EmpTypeList = [];
    $scope.DesignationList = [];
    $scope.EmploymentTypeList = [];
    //Filling of the Plant And Employee Type List

    $scope.fillPlantsEmps = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getPlants',

        }).then(function success(response) {
            $scope.PlantList = [];
            $scope.PlantList = response.data;
        })

        $http({
            method: 'POST',
            url: $scope.path + 'getEmpType',

        }).then(function success(response) {
            $scope.EmpTypeList = [];
            $scope.EmpTypeList = response.data;
        })

        $http({
            method: 'GET',
            url: $scope.path + 'getEmploymentType',

        }).then(function success(response) {
            $scope.EmploymentTypeList  = [];
            $scope.EmploymentTypeList  = response.data;
        })

    }

    $scope.fillDesignationList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDesignation',
            data: {'empType': $scope.Child.EmpTypeId}
        }).then(function success(response) {
            $scope.DesignationList = [];
            $scope.DesignationList = response.data;
        });
    }


    $scope.fillPlantsEmps();

    $scope.Child = {
        Id: null,
        MasterId: null,
        PlantId: null,
        EmpTypeId: null,
        DesignationId: null,
        EmploymentType: null,
    };
    $scope.childDataList = [];
    //Saving the Period  Child
    $scope.savePeriodChild = function () {

        if ($scope.periodList.length > 0) {

            periodValidations();

            $http({
                method: 'POST',
                url: $scope.path + 'savePeriodChild',
                data: { 'Periods': $scope.periodList }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

        
    }

    function periodValidations() {
        if ($scope.periodList[0].Period == "Weekly") {
            for (var i = 0; i < $scope.periodList.length; i++) {
                if (angular.isUndefinedOrNull($scope.periodList[i].WeekDay)) {
                    ShowResult("Please Fill all the Week Days Fields");
                    throw ("Invalid");
                }
            }
        }
        else {
            for (var i = 0; i < $scope.periodList.length; i++) {
                if (angular.isUndefinedOrNull($scope.periodList[i].Month) || angular.isUndefinedOrNull($scope.periodList[i].MonthDay)) {
                    ShowResult("Please Fill all the Month & Month Days Fields");
                    throw ("Invalid");
                }
            }
        }

        if ($scope.periodList[0].Period == "Monthly") {
            for (var i = 0; i < $scope.periodList.length; i++) {
                if ($scope.periodList[i].Month == "January" || $scope.periodList[i].Month == "March" || $scope.periodList[i].Month == "May" ||
                    $scope.periodList[i].Month == "July" || $scope.periodList[i].Month == "August" || $scope.periodList[i].Month == "October" || $scope.periodList[i].Month == "December") {
                    if (parseInt($scope.periodList[i].MonthDay) <= 0 || parseInt($scope.periodList[i].MonthDay) > 31) {
                        ShowResult($scope.periodList[i].Month + " cannot have " + $scope.periodList[i].MonthDay +" as Day Number ");
                        throw ("Invalid");
                    }
                }
                if ($scope.periodList[i].Month == "April" || $scope.periodList[i].Month == "June" || $scope.periodList[i].Month == "September" ||
                    $scope.periodList[i].Month == "November") {
                    if (parseInt($scope.periodList[i].MonthDay) <= 0 || parseInt($scope.periodList[i].MonthDay) > 30) {
                        ShowResult($scope.periodList[i].Month + " cannot have " + $scope.periodList[i].MonthDay + " as Day Number ");
                        throw ("Invalid");
                    }
                }

                if ($scope.periodList[i].Month == "February") {
                    if (parseInt($scope.periodList[i].MonthDay) <= 0 || parseInt($scope.periodList[i].MonthDay) > 29) {
                        ShowResult($scope.periodList[i].Month + " cannot have " + $scope.periodList[i].MonthDay + " as Day Number ");
                        throw ("Invalid");
                    }
                }
            }
        }

        $scope.checkMonth();
    }


    // Saving the Plant Emp Child
    $scope.saveChild = function()
    {
        $scope.Child.MasterId = $scope.Master.Id;
        $http({
            method: 'POST',
            url: $scope.path + 'savePlantChild',
            data: { 'Child': $scope.Child }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                updateChild();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    //Deleting the Plant Emp Child Data
    $scope.DeleteChildData = [];
    $scope.confirmModal = function (data) {
        $scope.DeleteChildData = [];
        $scope.DeleteChildData = data;
        angular.element(document.querySelector('#confirmPOPUPD')).modal('show');
    }

    $scope.DeleteChild = function () {

        var obj = $scope.DeleteChildData;
        if (!baseService.isUndefinedOrNull(obj.Id)) {
            $http({
                method: 'POST',
                url: $scope.path + 'DeleteChild',
                data: { 'id': obj.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    updateChild();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    function updateChild() {
        $http({
            method: 'POST',
            url: $scope.path + 'getPlantChildData',
            data: { 'MasterId': $scope.Master.Id }
        }).then(function success(resp) {

            $scope.childDataList = [];
            $scope.childDataList = resp.data;

        })
    }
}