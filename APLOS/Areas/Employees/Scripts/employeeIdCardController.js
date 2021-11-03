'use strict';
employeeIdCardController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$http', '$filter'];
function employeeIdCardController(commonMessage, $scope, $rootScope, baseService, $routeParams, $http, $filter) {
    $rootScope.title = "Print Employee Id Card";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.empList = [];
    $scope.path = 'employees/employeeidcard/';

    // #region Employee

    $rootScope.tempList = [];
    $scope.getEmployeeListUrl = 'employees/EmployeeInformation/GetPlantEmployeeList';
    $scope.employeeList = [];
    $scope.searchEmployeeList = [
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
            'name': 'Employee Category',
            'value': 'EmployeeCategoryName'
        }
    ];

    $scope.ShowEmployeeListPopUp = function () {
        $rootScope.tempList = [];
        angular.forEach($scope.empIdList, function (a) {
            $rootScope.tempList.push(a);
        });
        baseService.setCurrentPage('employeeList');
        baseService.init($scope.getEmployeeListUrl, null, null, null, 'EmployeeCode, FirstName, MiddleName, LastName ', 'EmployeeCode');
        $rootScope.parameters.plantId = null;
        $rootScope.parameters.employeeIds = JSON.stringify([]);// baseService.getColumnValueList($scope.empMobileAuths, 'EmployeeId');
        $scope.getEmployeeData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    for (var t = 0; t < baseService.arrayLength($scope.employeeList); t++) {
                        $scope.employeeList[t].Flag = $rootScope.tempList.includes($scope.employeeList[t].EmployeeCode);
                    }
                    angular.element(document.querySelector('#employeePopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getEmployeeData();
    };
    $scope.pushTempList = function (data, event) {
        if (event.currentTarget.checked) {
            $rootScope.tempList.push(data.EmployeeCode);
            $scope.empList.push(data);
        }
        else {
            $rootScope.tempList.splice($rootScope.tempList.indexOf(data.EmployeeCode), 1);
            for (var i = 0; i < baseService.arrayLength($scope.empList); i++) {
                if ($scope.empList[i].SystemId === data.SystemId)
                    $scope.empList.splice(i, 1);
            }
        }
        //console.log($scope.empList.length);
    };
    $scope.empIdList = [];

    $scope.SelectEmployeeByButton = function () {
        $scope.empIdList = [];
        if (baseService.arrayLength($rootScope.tempList) > 0) {
            angular.forEach($rootScope.tempList, function (a) {
                if (!$scope.empIdList.includes(a))
                    $scope.empIdList.push(a);
            });
        }
        else $scope.empIdList = [];
        angular.forEach($scope.empIdList, function (a) {
            if (!$rootScope.tempList.includes(a))
                $scope.empIdList.splice($scope.empIdList.indexOf(a), 1);
        });
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };
    $scope.CloseEmployeePopUp = function () {
        $scope.employeeId = '';
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    };

    $scope.prinModel = {
        IsWorker: false
        , IsEmployee: false
        , CompanyLogo: null
        , CompanyName: null
        , CompanyAddress: null
        , MobileNo: null
        , EmployeeCode: null
        , EmployeeName: null
        , EmployeePic: null
        , DesignationName: null
        , DOJ: null
        , BloodGroup: null
        , CardHolderSignature: null
        , AuthorizedSignature: null
        , EmploymentType: null
        , Department: null
        , LineNo: null
        , PresentAddress1: null
        , NameLabel: null
        , DesignationLabel: null
        , DepartmentLabel: null
        , LineLabel: null
        , EmploymentTypeName: null
        , UtilityName: null
        , NIDLabel: null
        , BloodGroupLabel: null
        , ParmanentAddress1Local: null
        , ParmanentAddress: null
        , MobileNoLabel: null
        , EmergencyTellNoLabel: null
        , validdate: null
        , EmrCntPer1CellNo: null
        , Phone:null
    };

    $scope.IssueDate = $filter('date')(new Date(), 'dd-MM-yyyy');

    $scope.emp = {
        Id: null
        , EmployeeCode: null
        , IdCardFormat: null
    };
    $scope.setData = function (data) {
        $scope.emp.Id = data.SystemId;
        $scope.emp.EmployeeCode = data.EmployeeCode;
        $scope.emp.EmployeeName = data.EmployeeName;
        $scope.emp.IdCardFormat = data.IdCardFormat;
        $scope.emp.EmploymentType = data.EmploymentType;
        $http.get('employees/employeeinformation/getemployeebyid?employeeId=' + $scope.emp.Id + '&employeementType=' + $scope.emp.EmploymentType)
            .then(function (response) {
                $scope.prinModel = response.data;
                $scope.prinModel.CompanyLogo = virtualPath.LogoOrImage + response.data.CompanyLogo;
                $scope.prinModel.EmployeePic = virtualPath.EmployeePic + response.data.EmployeePic;
                $scope.prinModel.CardHolderSignature = virtualPath.CardHolderSignature + response.data.CardHolderSignature;
                $scope.prinModel.AuthorizedSignature = virtualPath.AuthorizedSignature + response.data.AuthorizedSignature;
                if (baseService.isUndefinedOrNull(response.data.LegalDesignation)) {
                    $scope.prinModel.LegalDesignation = response.data.DesignationName;
                }
                if ($scope.emp.IdCardFormat === '1') {
                    $scope.prinModel.IsWorker = true;
                    $scope.prinModel.IsEmployee = false;

                    $scope.prinModel.CompanyName = response.data.LocalCompanyName;
                    $scope.prinModel.DesignationName = response.data.LocalDesignationName;
                    $scope.prinModel.Department = response.data.LocalDepartmentName;
                    $scope.prinModel.NameLabel = response.data.NameLabel;
                    $scope.prinModel.DesignationLabel = response.data.DesignationLabel;
                    $scope.prinModel.DepartmentLabel = response.data.DepartmentLabel;
                    $scope.prinModel.LineLabel = response.data.LineLabel;
                    $scope.prinModel.EmploymentTypeLabel = response.data.EmploymentTypeLabel;
                    $scope.prinModel.IDNoLabel = response.data.IDNoLabel;
                    $scope.prinModel.EmploymentTypeName = response.data.EmploymentTypeName;
                    $scope.prinModel.DOJLabel = response.data.DOJLabel;
                    $scope.prinModel.EmergencyTellNoLabel = response.data.EmergencyTellNoLabel;
                    $scope.prinModel.BloodGroupLabel = response.data.BloodGroupLabel;
                    $scope.prinModel.EmployeeName = response.data.EmployeeNameLocal;
                    $scope.prinModel.CompanyAddress = response.data.UtilityName;
                    $scope.prinModel.NIDLabel = response.data.NIDLabel;
                    $scope.prinModel.BloodGroup = response.data.BloodGroup;
                    $scope.prinModel.ParmanentAddress1 = response.data.ParmanentAddress1Local;
                    $scope.prinModel.ParmanentAddress = response.data.ParmanentAddress;
                    $scope.prinModel.MobileNoLabel = response.data.MobileNoLabel;
                    $scope.prinModel.Section = response.data.Section;

                    
                    $scope.prinModel.CompanyMobileNoLabel = response.data.CompanyMobileNoLabel;
                    $scope.prinModel.Phone = response.data.Phone.getDigitBanglaFromEnglish();
                    $scope.prinModel.EmployeeCode = response.data.EmployeeCode.getDigitBanglaFromEnglish();
                    $scope.prinModel.MobileNo = response.data.MobileNo.getDigitBanglaFromEnglish();
                    if (!baseService.isUndefinedOrNull(response.data.EmrCntPer1CellNo)) {
                        $scope.prinModel.EmrCntPer1CellNo = response.data.EmrCntPer1CellNo.getDigitBanglaFromEnglish();
                    }
                    $scope.prinModel.NationalID = response.data.NationalID.getDigitBanglaFromEnglish();
                    $scope.prinModel.DOJ = response.data.DOJ.getDigitBanglaFromEnglish();

                    var issuedt = new Date();
                    $scope.issuedt = new Date(issuedt.setFullYear(issuedt.getFullYear() + 5));
                    $scope.validdate = $filter('date')(new Date($scope.issuedt), 'dd-MM-yyyy');

                    $scope.prinModel.validdate = $scope.validdate.getDigitBanglaFromEnglish();

                    $scope.IssueDate = $scope.IssueDate.getDigitBanglaFromEnglish();
                }
                else {
                    $scope.prinModel.IsEmployee = true;
                    $scope.prinModel.IsWorker = false;
                    $scope.prinModel.CompanyName = response.data.LocalCompanyName;
                    $scope.prinModel.DesignationName = response.data.LocalDesignationName;
                    $scope.prinModel.Department = response.data.LocalDepartmentName;
                    $scope.prinModel.NameLabel = response.data.NameLabel;
                    $scope.prinModel.DesignationLabel = response.data.DesignationLabel;
                    $scope.prinModel.DepartmentLabel = response.data.DepartmentLabel;
                    $scope.prinModel.LineLabel = response.data.LineLabel;
                    $scope.prinModel.EmploymentTypeLabel = response.data.EmploymentTypeLabel;
                    $scope.prinModel.IDNoLabel = response.data.IDNoLabel;
                    $scope.prinModel.EmploymentTypeName = response.data.EmploymentTypeName;
                    $scope.prinModel.DOJLabel = response.data.DOJLabel;
                    $scope.prinModel.EmergencyTellNoLabel = response.data.EmergencyTellNoLabel;
                    $scope.prinModel.BloodGroupLabel = response.data.BloodGroupLabel;
                    $scope.prinModel.EmployeeName = response.data.EmployeeNameLocal;
                    $scope.prinModel.CompanyAddress = response.data.UtilityName;
                    $scope.prinModel.NIDLabel = response.data.NIDLabel;
                    $scope.prinModel.BloodGroup = response.data.BloodGroup;
                    $scope.prinModel.ParmanentAddress1 = response.data.ParmanentAddress1Local;
                    $scope.prinModel.ParmanentAddress = response.data.ParmanentAddress;
                    $scope.prinModel.MobileNoLabel = response.data.MobileNoLabel;
                    $scope.prinModel.Section = response.data.Section;

                   // $scope.prinModel.validdate = response.data.Validity.getDigitBanglaFromEnglish();
                    $scope.prinModel.CompanyMobileNoLabel = response.data.CompanyMobileNoLabel;
                    $scope.prinModel.Phone = response.data.Phone.getDigitBanglaFromEnglish();
                    $scope.prinModel.EmployeeCode = response.data.EmployeeCode.getDigitBanglaFromEnglish();
                    $scope.prinModel.MobileNo = response.data.MobileNo.getDigitBanglaFromEnglish();
                    if (!baseService.isUndefinedOrNull(response.data.EmrCntPer1CellNo)) {
                        $scope.prinModel.EmrCntPer1CellNo = response.data.EmrCntPer1CellNo.getDigitBanglaFromEnglish();
                    }
                    $scope.prinModel.NationalID = response.data.NationalID.getDigitBanglaFromEnglish();
                    $scope.prinModel.DOJ = response.data.DOJ.getDigitBanglaFromEnglish();

                    var issuedt = new Date();
                    $scope.issuedt = new Date(issuedt.setFullYear(issuedt.getFullYear() + 5));
                    $scope.validdate = $filter('date')(new Date($scope.issuedt), 'dd-MM-yyyy');

                    $scope.prinModel.validdate = $scope.validdate.getDigitBanglaFromEnglish();

                    $scope.IssueDate = $scope.IssueDate.getDigitBanglaFromEnglish();
                }
            });
        $scope.CloseEmployeePopUp();
    };

    // #endregion

    $scope.Clear = function () {
        ClearFields();
    };
    function ClearFields() {
        $scope.empList = [];
        $rootScope.tempList = [];
        $scope.empIdList = [];
        $scope.emp = {};
        $scope.prinModel = { IsWorker: false, IsEmployee: false };
        document.getElementById("cSrc").setAttribute('src', null);
        document.getElementById("aSrc").setAttribute('src', null);
        document.getElementById("comSrc").setAttribute('src', null);
        document.getElementById("empSrc").setAttribute('src', null);
    }
    
    $scope.Print = function () {
        var frontElement = null, backElement = null;
        if ($scope.emp.IdCardFormat === '1') {
            frontElement = document.getElementById('workerImgFront');
            backElement = document.getElementById('workerImgBack');
        }
        else {
            frontElement = document.getElementById('empImgFront');
            backElement = document.getElementById('empImgBack');
        }
        var btn = document.getElementById('download');

      
        btn.onclick = function () {
            domtoimage.toBlob(frontElement)
                .then(function (blob) {
                    window.saveAs(blob, $scope.emp.EmployeeCode + '-Fornt.jpg');
                });
            domtoimage.toBlob(backElement)
                .then(function (blob) {
                    window.saveAs(blob, $scope.emp.EmployeeCode + '-Back.jpg');
                });
        };
    };

    //var doc = new jsPDF();
    //var specialElementHandlers = {
    //    '#editor': function (element, renderer) {
    //        return true;
    //    }
    //};

    //$('#download').click(function () {
    //    doc.fromHTML($('#content').html(), 15, 15, {
    //        'width': 170,
    //        'elementHandlers': specialElementHandlers
    //    });
    //    doc.save('sample-file.pdf');
    //});



   
    var finalEnlishToBanglaNumber = { '0': '০', '1': '১', '2': '২', '3': '৩', '4': '৪', '5': '৫', '6': '৬', '7': '৭', '8': '৮', '9': '৯' };
    String.prototype.getDigitBanglaFromEnglish = function () {
        var retStr = this;
        for (var x in finalEnlishToBanglaNumber) {
            retStr = retStr.replace(new RegExp(x, 'g'), finalEnlishToBanglaNumber[x]);
        }
        return retStr;
    };
    //var english_number = "1-2-3-456";
    //var bangla_converted_number = english_number.getDigitBanglaFromEnglish();

   

}