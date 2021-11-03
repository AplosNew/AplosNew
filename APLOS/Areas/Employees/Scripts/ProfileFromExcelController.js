ProfileFromExcelController.$inject = ['$scope', '$http', '$location', "$rootScope", '$window', "$compile", 'baseService'];
function ProfileFromExcelController($scope, $http, $location, $rootScope, $window, $compile, baseService) {
    $scope.title = 'Employee Profile';

    //$scope.employeeProfileFrom = {
    //    SLNo: null,
    //    SystemId: null,
    //    EmployeeCode: null,
    //    FirstName: null,
    //    LastName: null,
    //    DOB: null,
    //    Gender: null,
    //    Religion: null,
    //    BloodGroup: null,
    //    CivilStatus: null,
    //    Phone: null,
    //    Email: null,
    //    NationalID: null,
    //    FatherName: null,
    //    DOJ: null,
    //    ManpowerBudgetCode: null,
    //    Company: null,
    //    Division: null,
    //    SubDivision: null,
    //    Unit: null,
    //    Department: null,
    //    Designation: null,
    //    Section: null,
    //    SubSection: null,
    //    IsCompleted: null,
    //    IsLocked: null,
    //    IsActive: null
    //};
    
    //$scope.selectedFile = null;
    //$scope.Message = "";

    //$scope.loadFile = function (files) {
    //    $scope.$apply(function () {

    //        $scope.selectedFile = files[0];
    //    });
    //};
    //var ColList = ["EmployeeCode", "Salutation", "FirstName", "LastName", "EmployeeName", "FatherName", "MotherName", "MaritalStatus", "SpouseName", "PresentAddress1", "PresentAddress2",
    //    "PermanentAddress1", "PermanentAddress2", "EmploymentType", "Gender", "Religion", "BloodGroup", "PhoneNo", "CardNumber", "NID", "DOB", "CelebrationDOB", "DOJ", "P.Period", "ShiftEffectiveDate", "RosterShiftName"
    //    , "AssignShiftName", "WeekOffEffectiveDate", "AlignWithCompany", "IndividualWeekOff", "JobLocation", "LegalDesignation", "BudgetCode"];
    //$scope.handleFile = function () {
    //    var file = $scope.selectedFile;
    //    if (file) {
    //        var reader = new FileReader();
    //        reader.onload = function (e) {
    //            var data = e.target.result;
    //            var workbook = XLSX.read(data, { type: 'binary' });
    //            var first_sheet_name = workbook.SheetNames[0];
    //            var dataObjects = XLSX.utils.sheet_to_json(workbook.Sheets[first_sheet_name]);
    //            var savelist = [];

    //            for (var i = 0; i < dataObjects.length; i++) {
    //                var ob = dataObjects[i];

    //                for (var j in $scope.employeeProfileFrom) {
    //                    $scope.employeeProfileFrom[j] = null;
    //                }

    //                var c = 0;
    //                for (var k in ob) {
    //                    $scope.employeeProfileFrom[ColList[c]] = ob[k];
    //                    $scope.newList = angular.copy($scope.employeeProfileFrom);
    //                    c++;
    //                }
    //                savelist.push($scope.newList);
    //            }
    //            if (dataObjects.length > 0) {
    //                $scope.save(savelist);
    //            } else {
    //                $scope.msg = "Error : Something Wrong !";
    //            }
    //        };
    //        reader.onerror = function (ex) {
    //        };
    //        reader.readAsBinaryString(file);
    //    }
    //};

    //$scope.save = function (data) {
    //    $http({
    //        method: 'POST',
    //        url: "employeeprofilefromexcel/save",
    //        data: JSON.stringify(data),
    //        headers: {
    //            'Content-Type': 'application/json'
    //        }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //        }
    //    }), function errorCallBack(response) {
    //        ShowResult(response.data.Message, 'failure');
    //    };
    //};

}