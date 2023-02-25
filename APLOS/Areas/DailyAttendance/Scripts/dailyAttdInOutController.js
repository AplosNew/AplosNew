'use strict';
dailyAttdInOutController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$cookies', '$window'];
function dailyAttdInOutController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $cookies, $window) {
    $scope.user = $rootScope.empid;
    $rootScope.title = 'Pre-Recruitment Employee';
    $scope.Action = 'Save';
    $scope.imageSrc = null;
    $scope.showdiv = false;
    $scope.imageBtnDisable = false;
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'recruitments/home/';
    $scope.getListUrl = $scope.path + 'getlist?empid=' + $scope.user;
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.message = "";
    $scope.savedisable = false;
    virtualPath.ROOT_FOLDRR = '/' + getCookie('ROOT_FOLDRR');
    virtualPath.EmpPic = virtualPath.ROOT_FOLDRR + '/PreRecruitments/EmpPic/';
    function getCookie(cname) {
        var name = cname + "=";
        var decodedCookie = decodeURIComponent(document.cookie);
        var ca = decodedCookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) === ' ') {
                c = c.substring(1);
            }
            if (c.indexOf(name) === 0)
                return c.substring(name.length, c.length);
        }
        return "";
    }
    $scope.preRecruitmentEmployee = {
        Id: null,
        Image: null,
        InterviewRankingId: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        DOB: null,
        PositionID: null,
        BudgetId: null,
        IsDirect: true,
        FullName: null,
        Gender: null,
        NationalID: null,
        Phone: null,
        Email: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        EmpType: null,
        Status: null,
        TIN: null,
        FatherName: null,
        MotherName: null,
        CitizenID: null,
        ReligionID: null,
        CivilStatusID: null,
        BloodGroupID: null,
        SpouseName: null,
        SpouseNationalID: null,
        SpouseOccupation: null,
        NoOfChildren: null,
        PresentAddress1: null,
        PresentAddress2: null,
        ParmanentAddress1: null,
        ParmanentAddress2: null,
        PresThanaID: null,
        ParmThanaID: null,
        PresPostOfficeID: null,
        ParmPostOfficeID: null,
        PresZipCode: null,
        ParmZipCode: null,
        PresDistrictID: null,
        ParmDistrictID: null,
        PresCityID: null,
        ParmCityID: null,
        PresCountryID: null,
        EmrCntPer1Name: null,
        EmrCntPer2Name: null,
        EmrCntPer1CellNo: null,
        EmrCntPer2CellNo: null,
        SubmitDateTime: null,
        SelectionDateTime: null,
        SelectedBy: null,
        Submitted: false,
        ReadyForCandidateAccess: false,
        AppAddedDateTime: null,
        AppAddedBy: null,
        SelectionStatus: null,
        MarriagedayCelebrationDate: null,
        BirthdayCelebrationDate: null,
        SpecialReviewAmount: null,
        SpecialReviewDuration: null,
        TotalSalary: null,
        AgreedDOJ: null,
        PresentArea: null,
        ParmanentArea: null,
        PresPostOfficeName: null,
        PresCountryName: null,
        PresDistrictName: null,
        PresThanaName: null,
        IsKnownPerson: false,
        NumberOfKnownPerson: 1,
        EmrCntPer1CellNo2: null,
        EmrCntPer1CellNo3: null,
        EmrCntPer2CellNo2: null,
        EmrCntPer2CellNo3: null,
        HasPartner: false,
        SalaryRangeForTax: 0,
        ApplyingAsFresher: false,
        IsTINRequiredForSalaryAbove: false
    };

    $scope.preRecruitmentEmpReference = {
        SystemID: null,
        PreRecruitmentEmployeeId: $scope.user,
        Ref1Name: null,
        Ref1EmployerName: null,
        Ref1EmployerAddress: null,
        Ref1Designation: null,
        Ref1CellPhnNo: null,
        Ref1TelePhnNo: null,
        Ref1Email: null,
        Ref1Address: null,
        Ref2Name: null,
        Ref2EmployerName: null,
        Ref2EmployerAddress: null,
        Ref2Designation: null,
        Ref2CellPhnNo: null,
        Ref2TelePhnNo: null,
        Ref2Email: null,
        Ref2Address: null
    };

    $scope.preRecruitmentEmpQualification = {
        SystemID: null,
        PreRecruitmentEmployeeId: $scope.user,
        EductLevelSystemID: null,
        StreamId: null,
        CountryId: null,
        HasDistinction: false,
        Session: null,
        IsGeneral: false,
        IsEnglishMedium: false,
        IsMadrasah: false,
        IsVocational: false,
        IsOther: false,
        OtherEductType: null,
        ExamDegreeType: null,
        ConcMajor: null,
        InstituteName: null,
        IsForeignInstitute: false,
        ResultSystemID: null,
        Marks: null,
        CGPA: null,
        Scale: null,
        YearOfPass: null,
        Duration: null,
        Achievement: null
    };
    $scope.preRecruitmentEmpQualificationNew = Object.assign({}, $scope.preRecruitmentEmpQualification);

    $scope.preRecruitmentEmpTraining = {
        SystemID: null,
        PreRecruitmentEmployeeId: $scope.user,
        TrainingTitle: null,
        TopicCovered: null,
        InstituteName: null,
        CountrySystemID: null,
        Location: null,
        TrainingYear: null,
        Duration: null,
        DurationUOM: null
    };
    $scope.preRecruitmentEmpTrainingNew = Object.assign({}, $scope.preRecruitmentEmpTraining);

    $scope.preRecruitmentEmpExperience = {
        SystemID: null,
        PreRecruitmentEmployeeId: $scope.user,
        Employer: null,
        Designation: null,
        StartDate: null,
        EndDate: null,
        JobDescription: null,
        Achievement: null,
        IsPartTime: false,
        DurationYear: null,
        DurationMonth: null,
        IsCurrentJob: false
    };
    $scope.preRecruitmentEmpExperienceNew = Object.assign({}, $scope.preRecruitmentEmpExperience);

    $scope.preRecruitmentDocument = {
        Id: null,
        PreRecruitmentEmployeeId: null,
        FileId: null,
        FileName: null,
        ComplianceDocumentId: null
    };

    cboService.getCivilStatus(function (result) {
        $scope.civilStatusList = result;
    });

    $scope.setHeight = function (id) {
        var element = angular.element(document.getElementById(id));
        $scope.height = element[0].scrollHeight;
    };

    $scope.VisibleDiv = function () {
        if ($scope.showdiv === true) {
            return true;
        }
        else {
            return false;
        }
    };
    $scope.preRecruitmentEmployee.ApplyingAsFresher = false;
    $scope.LoadData = function (user) {
        $http.get($scope.getListUrl)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.preRecruitmentEmployee = response.data[0];
                    if ($scope.preRecruitmentEmployee.ReadyForCandidateAccess !== true) {
                        $scope.preRecruitmentEmployee = [];
                        $scope.message = "Candidate Access is Denied!!!";
                    }
                    else {
                        $scope.user = user;
                        $scope.preRecruitmentEmployee.DOB = $filter('dateFiltering')(response.data[0].DOB, 'dd-M-yyyy');
                        $scope.preRecruitmentEmployee.BirthdayCelebrationDate = $filter('dateFiltering')(response.data[0].BirthdayCelebrationDate, 'dd-M-yyyy');
                        $scope.preRecruitmentEmployee.MarriagedayCelebrationDate = $filter('dateFiltering')(response.data[0].MarriagedayCelebrationDate, 'dd-M-yyyy');
                        $scope.preRecruitmentEmployee.AgreedDOJ = $filter('dateFiltering')(response.data[0].AgreedDOJ, 'dd-M-yyyy');

                        $scope.imageSrc = virtualPath.EmpPic + $scope.preRecruitmentEmployee.Image;
                        $rootScope.Gender = $scope.preRecruitmentEmployee.Gender;
                        $scope.CompanyGroupLogo = virtualPath.LogoOrImage + $scope.preRecruitmentEmployee.CompanyGroupLogo;
                        $rootScope.FullName = $scope.preRecruitmentEmployee.FullName;
                        $rootScope.CompanyGroupId = $scope.preRecruitmentEmployee.GroupID;
                        $rootScope.PhoneLength = $scope.preRecruitmentEmployee.PhoneLength;
                        $rootScope.BudgetId = $scope.preRecruitmentEmployee.BudgetId;
                        $rootScope.PlantId = $scope.preRecruitmentEmployee.PlantId;
                        $rootScope.EmpType = $scope.preRecruitmentEmployee.EmpType;
                        $rootScope.CountryId = $scope.preRecruitmentEmployee.CountryId;
                        $scope.GivenDesignationId = $scope.preRecruitmentEmployee.GivenDesignationId;
                        $scope.Tin = $scope.preRecruitmentEmployee.TINCaption;
                        if (baseService.isUndefinedOrNull($scope.Tin)) {
                            $scope.Tin = "TIN";
                        }
                        $scope.Nid = $scope.preRecruitmentEmployee.NIDCaption;
                        if (baseService.isUndefinedOrNull($scope.Nid)) {
                            $scope.Nid = "National ID";
                        }
                        $scope.NidLength = $scope.preRecruitmentEmployee.NIDLength;
                        $scope.TinLength = $scope.preRecruitmentEmployee.TINLength;
                        $scope.SalaryRangeForTax = $scope.preRecruitmentEmployee.TINRequiredForSalaryAbove;
                        $scope.SalaryRangeForTaxRequired = $scope.preRecruitmentEmployee.IsTINRequiredForSalaryAbove;
                        $scope.TotalSalary = $scope.preRecruitmentEmployee.TotalSalary;
                        $scope.NationalID = $scope.preRecruitmentEmployee.NationalID;
                        $scope.TIN = $scope.preRecruitmentEmployee.TIN;

                        $scope.loadAll();

                        $scope.citizenList = [];
                        addressService.getCountryCbo(function (result) {
                            $scope.citizenList = result;
                            $scope.preRecruitmentEmployee.CitizenID = $rootScope.CountryId;

                            $scope.PresCountryList = result;
                            $scope.preRecruitmentEmployee.PresCountryID = $rootScope.CountryId;

                            $scope.GetPresStateOnCountryChange($scope.preRecruitmentEmployee.PresCountryID);
                            $scope.onPreCountryChange($scope.preRecruitmentEmployee.PresCountryID);
                            $scope.getPresDisOnPreStateChange($scope.preRecruitmentEmployee.PresStateId);
                            $scope.GetPresPoliceStationCboByDistrictChange($scope.preRecruitmentEmployee.PresDistrictID);
                            $scope.GetPresPostOfficeCboByDistrictChange($scope.preRecruitmentEmployee.PresDistrictID);

                            $scope.ParmCountryList = result;
                            $scope.preRecruitmentEmployee.ParmCountryID = $rootScope.CountryId;
                            $scope.GetParmStateOnCountryChange($scope.preRecruitmentEmployee.ParmCountryID);
                            $scope.onParmCountryChange($scope.preRecruitmentEmployee.ParmCountryID);
                            $scope.getParmDisOnParmStateChange($scope.preRecruitmentEmployee.ParmStateId);
                            $scope.GetParmPoliceStationCboByDistrictChange($scope.preRecruitmentEmployee.ParmDistrictID);
                            $scope.GetParmPostOfficeCboByDistrictChange($scope.preRecruitmentEmployee.ParmDistrictID);
                        });

                        $scope.celebrationMarriage();
                    }
                }
                else {
                    $scope.preRecruitmentEmployee = [];
                    $scope.message = "Invalid Employee Id!!!";
                }
                //show div
                if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.Salutation)) {
                    $scope.showdiv = false;
                }
                else {
                    if ($scope.preRecruitmentEmployee.Salutation.length > 0) {
                        $scope.showdiv = true;
                    }
                    else {
                        $scope.showdiv = false;
                    }
                }
            });
    };

    $scope.load = function () {
        var id = $scope.user;
        $scope.getListUrl = $scope.path + 'getlist?empid=' + id;
        $scope.LoadData(id);
    };

    angular.element(document).ready(function () {
        $scope.LoadData($scope.user);
    });

    $scope.loadAll = function () {
        $scope.LoadReferenceData();
        $scope.GetJobDescriptionData();
        $scope.LoadQualificationData();
        $scope.LoadExperienceData();
        $scope.LoadTrainingData();
        $scope.getSalutationList();
        $scope.Loaddocumentdatalist();
        $scope.GetSalaryHeadsData();
        if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.Image)) {
            $scope.imageSrc = null;
            if ($rootScope.Gender === 'Male') {
                $scope.imageSrc = "Recruitments/Images/male-alt.png";
            } else {
                $scope.imageSrc = "Recruitments/Images/female-alt.png";
            }
        }
    };

    $scope.GetSalaryHeadsData = function () {
        $http.get('humanresource/salaryfixation/getheadlist?preRecruitmentEmployeeId=' + $scope.user + '&givenDesignationId=' + $scope.GivenDesignationId + '&plantId=' + $rootScope.PlantId)
            .then(function (response) {
                $scope.salaryHeadList = response.data;
                DistributeList($scope.salaryHeadList);
            });
    };

    function DistributeList(mainlist) {
        $scope.salaryHeadListMonth = [];
        $scope.salaryHeadListLeave = [];
        $scope.salaryHeadListAc = [];
        $scope.salaryHeadListAnc = [];
        try {
            for (var i = 0; i < baseService.arrayLength(mainlist); i++) {
                if (mainlist[i].IsMonthly) {
                    $scope.salaryHeadListMonth.push(mainlist[i]);
                }
                else if (mainlist[i].IsAnnualCash) {
                    $scope.salaryHeadListAc.push(mainlist[i]);
                }
                else if (mainlist[i].IsAnnualNonCash) {
                    if (mainlist[i].IsAnnualNonCash) {
                        mainlist[i].CurrentStatus = mainlist[i].CurrentStatusN;
                        mainlist[i].ExpectedStatus = mainlist[i].ExpectedStatusN;
                        $scope.salaryHeadListAnc.push(mainlist[i]);
                    }
                }
                else {
                    if (mainlist[i].IsLeave) {
                        mainlist[i].CurrentStatus = mainlist[i].CurrentStatusL;
                        mainlist[i].ExpectedStatus = mainlist[i].ExpectedStatusL;
                        $scope.salaryHeadListLeave.push(mainlist[i]);
                    }
                }
            }
            $scope.salaryHeadListMonth = sortObj($scope.salaryHeadListMonth, 'SequenceNo');
            $scope.salaryHeadListLeave = sortObj($scope.salaryHeadListLeave, 'SequenceNo');
            $scope.salaryHeadListAc = sortObj($scope.salaryHeadListAc, 'SequenceNo');
            $scope.salaryHeadListAnc = sortObj($scope.salaryHeadListAnc, 'SequenceNo');
        } catch (e) {
            throw e;
        }
    }

    function CombineList(mainlist) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListMonth); i++) {
                mainlist.push($scope.salaryHeadListMonth[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListAc); i++) {
                mainlist.push($scope.salaryHeadListAc[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListAnc); i++) {
                mainlist.push($scope.salaryHeadListAnc[i]);
            }
            for (var i = 0; i < baseService.arrayLength($scope.salaryHeadListLeave); i++) {
                mainlist.push($scope.salaryHeadListLeave[i]);
            }
        } catch (e) {
            throw e;
        }
    }

    function sortObj(list, key) {
        function compare(a, b) {
            a = a[key];
            b = b[key];
            var type = (typeof (a) === 'string' ||
                typeof (b) === 'string') ? 'string' : 'number';
            var result;
            if (type === 'string') result = a.localeCompare(b);
            else result = a - b;
            return result;
        }
        return list.sort(compare);
    }

    $scope.celebrationMarriage = function () {
        if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.CivilStatusID)) {
            $scope.celebrationType = $.grep($scope.civilStatusList, function (item) {
                return item.Value === $scope.preRecruitmentEmployee.CivilStatusID;
            })[0].HasPartner;
            if ($scope.celebrationType) {
                //
            }
            else {
                $scope.preRecruitmentEmployee.MarriagedayCelebrationDate = null;
                $scope.preRecruitmentEmployee.SpouseNationalID = null;
                $scope.preRecruitmentEmployee.SpouseName = null;
                $scope.preRecruitmentEmployee.SpouseOccupation = null;
                $scope.preRecruitmentEmployee.NoOfChildren = null;
            }
        }
    };

    $scope.LoadReferenceData = function () {
        $http.get('Recruitments/home/getreferencedata?id=' + $scope.user)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.preRecruitmentEmpReference = response.data[0];
                }
            });
    };

    $scope.LoadQualificationData = function () {
        $http.get('Recruitments/home/getqualificationdata?id=' + $scope.user)
            .then(function (response) {
                $scope.preRecruitmentEmpQualifications = response.data;
            });
    };

    $scope.LoadExperienceData = function () {
        $http.get('Recruitments/home/getexperiencedata?id=' + $scope.user)
            .then(function (response) {
                $scope.preRecruitmentEmpExperiences = response.data;
            });
    };

    $scope.LoadTrainingData = function () {
        $http.get('Recruitments/home/gettrainingdata?id=' + $scope.user)
            .then(function (response) {
                $scope.preRecruitmentEmpTrainings = response.data;
            });
    };

    $scope.disableBtn = function () {
        if ($scope.preRecruitmentEmployee.Submitted === false) {
            return false;
        }
        else {
            $scope.savedisable = true;
            return true;
        }
    };

    $scope.GetJobDescriptionData = function () {
        $http.get('Recruitments/home/getjobdata?id=' + $scope.user)
            .then(function (response) {
                $scope.jobDescriptions = response.data;
            });
    };

    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            $scope.imageSrc = virtualPath.EmpPic + data.Image;
            $scope.imageBtnDisable = true;
            $scope.preRecruitmentEmployee.Image = data.Image;
        }
        else {
            $scope.imageBtnDisable = false;
            $scope.preRecruitmentEmployee.Image = null;
        }
    }

    $scope.picdata = null;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.countDate = function () {
        if ($scope.preRecruitmentEmpExperienceNew.IsCurrentJob === false) {
            var st = new Date($scope.preRecruitmentEmpExperienceNew.StartDate);
            var ed = new Date($scope.preRecruitmentEmpExperienceNew.EndDate);

            var nowyear = ed.getFullYear();
            var nowmonth = ed.getMonth() + 1;
            var nowday = ed.getDate();

            var styear = st.getFullYear();
            var stmonth = st.getMonth() + 1;
            var stday = st.getDate();

            var age = nowyear - styear;
            var age_month = nowmonth - stmonth;
            var age_day = nowday - stday;

            if (age_month < 0 || (age_month === 0 && age_day < 0)) {
                age = parseInt(age) - 1;
                age_month += 12;
            }
            if (age_month === 12) {
                age_month = 0;
                age = age + 1;
            }

            $scope.preRecruitmentEmpExperienceNew.DurationYear = age;
            $scope.preRecruitmentEmpExperienceNew.DurationMonth = age_month;
        }
        else {
            var st = new Date($scope.preRecruitmentEmpExperienceNew.StartDate);
            var ed = new Date($scope.preRecruitmentEmpExperienceNew.EndDate);
            var nowyear = ed.getFullYear();
            var nowmonth = ed.getMonth() + 1;
            var nowday = ed.getDate();

            var styear = st.getFullYear();
            var stmonth = st.getMonth() + 1;
            var stday = st.getDate();

            var age = nowyear - styear;
            var age_month = nowmonth - stmonth;
            var age_day = nowday - stday;

            if (age_month < 0 || (age_month === 0 && age_day < 0)) {
                age = parseInt(age) - 1;
                age_month += 12;
            }
            if (age_month === 12) {
                age_month = 0;
                age = age + 1;
            }

            $scope.preRecruitmentEmpExperienceNew.DurationYear = age;
            $scope.preRecruitmentEmpExperienceNew.DurationMonth = age_month;
        }
    };

    

    $scope.getSalutationList = function () {
        $http({
            method: 'GET',
            url: 'employees/salutation/getcbo?companyGroupId=' + $rootScope.CompanyGroupId,
        }).then(function (response) {
            $scope.salutaionList = response.data;
        });
    };

    cboService.getCboReligion(function (result) {
        $scope.religionList = result;
    });

    cboService.getCboBloodGroup(function (result) {
        $scope.bloodGroupList = result;
    });

    addressService.getCountryCbo(function (result) {
        $scope.PresCountryList = result;
        $scope.preRecruitmentEmployee.PresCountryID = $rootScope.CountryId;

        $scope.ParmCountryList = result;
        $scope.preRecruitmentEmployee.ParmCountryID = $rootScope.CountryId;
    });

    $scope.GetParmStateOnCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.ParmStateList = result;
        });
    };

    $scope.GetPresStateOnCountryChange = function (countryId) {
        addressService.getCboStateByCountry(countryId, function (result) {
            $scope.PresStateList = result;
        });
    };

    $scope.onPreCountryChange = function (countryId) {
        addressService.getCboCityByCountry(countryId, function (result) {
            $scope.PresCityList = result;
        });
    };

    $scope.onParmCountryChange = function (countryId) {
        addressService.getCboCityByCountry(countryId, function (result) {
            $scope.ParmCityList = result;
        });
    };

    $scope.getPresDisOnPreStateChange = function (stateId) {
        addressService.getCboDistrictByState(stateId, function (result) {
            $scope.PresDistrictList = result;
        });
    };

    $scope.getParmDisOnParmStateChange = function (stateId) {
        addressService.getCboDistrictByState(stateId, function (result) {
            $scope.ParmDistrictList = result;
        });
    };

    $scope.GetParmPoliceStationCboByDistrictChange = function (districtId) {
        addressService.getPoliceStationCboByDistrictChange(districtId, function (result) {
            $scope.ParmPoliceStationList = result;
        });
    };

    $scope.GetPresPoliceStationCboByDistrictChange = function (districtId) {
        addressService.getPoliceStationCboByDistrictChange(districtId, function (result) {
            $scope.PresPoliceStationList = result;
        });
    };

    $scope.GetPresPostOfficeCboByDistrictChange = function (districtId) {
        addressService.getCboPostOfficeByDistrict(districtId, function (result) {
            $scope.PresPostOfficeList = result;
        });
    };

    $scope.GetParmPostOfficeCboByDistrictChange = function (districtId) {
        addressService.getCboPostOfficeByDistrict(districtId, function (result) {
            $scope.ParmPostOfficeList = result;
        });
    };

    addressService.getCboArea(function (result) {
        $scope.AreaList = result;
    });

    cboService.getCboQualificationLevel(function (result) {
        $scope.EductLevelSystemList = result;
    });

    cboService.getCboQualificationStream(function (result) {
        $scope.StreamList = result;
    });

    $scope.IsSameAddress = false;
    $scope.SetAddress = function () {
        if ($scope.IsSameAddress) {
            $scope.preRecruitmentEmployee.ParmanentAddress1 = $scope.preRecruitmentEmployee.PresentAddress1;
            $scope.preRecruitmentEmployee.ParmanentAddress2 = $scope.preRecruitmentEmployee.PresentAddress2;
            $scope.preRecruitmentEmployee.ParmCountryID = $scope.preRecruitmentEmployee.PresCountryID;
            $scope.GetParmStateOnCountryChange($scope.preRecruitmentEmployee.ParmCountryID);
            $scope.preRecruitmentEmployee.ParmStateId = $scope.preRecruitmentEmployee.PresStateId;

            $scope.preRecruitmentEmployee.ParmDistrictID = $scope.preRecruitmentEmployee.PresDistrictID;
            $scope.getParmDisOnParmStateChange($scope.preRecruitmentEmployee.ParmStateId);
            $scope.GetParmPostOfficeCboByDistrictChange($scope.preRecruitmentEmployee.ParmDistrictID);
            $scope.preRecruitmentEmployee.ParmCityID = $scope.preRecruitmentEmployee.PresCityID;
            $scope.onParmCountryChange($scope.preRecruitmentEmployee.ParmCountryID);
            $scope.GetParmPoliceStationCboByDistrictChange($scope.preRecruitmentEmployee.ParmDistrictID);
            $scope.preRecruitmentEmployee.ParmThanaID = $scope.preRecruitmentEmployee.PresThanaID;
            $scope.preRecruitmentEmployee.ParmPostOfficeID = $scope.preRecruitmentEmployee.PresPostOfficeID;
            $scope.preRecruitmentEmployee.ParmZipCode = $scope.preRecruitmentEmployee.PresZipCode;
            $scope.preRecruitmentEmployee.ParmanentArea = $scope.preRecruitmentEmployee.PresentArea;
        }
        else {
            $scope.preRecruitmentEmployee.ParmanentAddress1 = null;
            $scope.preRecruitmentEmployee.ParmanentAddress2 = null;
            $scope.preRecruitmentEmployee.ParmCountryID = null;
            $scope.preRecruitmentEmployee.ParmStateId = null;
            $scope.preRecruitmentEmployee.ParmDistrictID = null;
            $scope.preRecruitmentEmployee.ParmCityID = null;
            $scope.preRecruitmentEmployee.ParmThanaID = null;
            $scope.preRecruitmentEmployee.ParmPostOfficeID = null;
            $scope.preRecruitmentEmployee.ParmZipCode = null;
            $scope.preRecruitmentEmployee.ParmanentArea = null;
        }
    };

    $scope.SameDOB = false;
    $scope.SetSameDOB = function () {
        if ($scope.SameDOB) {
            $scope.preRecruitmentEmployee.BirthdayCelebrationDate = $scope.preRecruitmentEmployee.DOB;
        } else {
            $scope.preRecruitmentEmployee.BirthdayCelebrationDate = null;
        }
    };

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }
    function Validation() {
        try {
            CheckField($scope.preRecruitmentEmployee.Salutation, "Salutation");
            CheckField($scope.preRecruitmentEmployee.FirstName, "First Name");
            CheckField($scope.preRecruitmentEmployee.LastName, "Last Name");
            CheckField($scope.preRecruitmentEmployee.Phone, "Phone");
            CheckField($scope.preRecruitmentEmployee.Email, "Email");
            CheckField($scope.preRecruitmentEmployee.DOB, "Date of Birth");
            CheckField($scope.preRecruitmentEmployee.BirthdayCelebrationDate, "Birthday Celebration Date");
            CheckField($scope.preRecruitmentEmployee.NationalID, "" + $scope.Nid + "");
        } catch (e) {
            throw e;
        }
    }

    function ValidationMaster() {
        try {
            CheckField($scope.preRecruitmentEmployee.FatherName, "Father's Name");
            CheckField($scope.preRecruitmentEmployee.CitizenID, "Citizen");
            CheckField($scope.preRecruitmentEmployee.BloodGroupID, "Blood Group");
            CheckField($scope.preRecruitmentEmployee.CivilStatusID, "Civil Status");
        } catch (e) {
            throw e;
        }
    }

    function ValidateQualification() {
        try {
            CheckField($scope.preRecruitmentEmpQualificationNew.EductLevelSystemID, "Level Of Education");
            CheckField($scope.preRecruitmentEmpQualificationNew.StreamId, "Stream");
            CheckField($scope.preRecruitmentEmpQualificationNew.ExamDegreeType, "Exam/Degree Title");
            CheckField($scope.preRecruitmentEmpQualificationNew.InstituteName, "Institute Name");
            CheckField($scope.preRecruitmentEmpQualificationNew.YearOfPass, "Year of Passing");
        } catch (e) {
            throw e;
        }
    }

    function ValidateTraining() {
        try {
            CheckField($scope.preRecruitmentEmpTrainingNew.TrainingTitle, "Training Title");
            CheckField($scope.preRecruitmentEmpTrainingNew.InstituteName, "Institute Name");
            CheckField($scope.preRecruitmentEmpTrainingNew.TrainingYear, "Training Year");
            CheckField($scope.preRecruitmentEmpTrainingNew.Duration, "Duration");
            CheckField($scope.preRecruitmentEmpTrainingNew.DurationUOM, "Duration");
        } catch (e) {
            throw e;
        }
    }

    function ValidateExperience() {
        try {
            CheckField($scope.preRecruitmentEmpExperienceNew.Employer, "Employer");
            CheckField($scope.preRecruitmentEmpExperienceNew.Designation, "Designation");
            CheckField($scope.preRecruitmentEmpExperienceNew.StartDate, "Start Date");
            if ($scope.preRecruitmentEmpExperienceNew.IsCurrentJob === false) {
                CheckField($scope.preRecruitmentEmpExperienceNew.EndDate, "End Date");
            }
        } catch (e) {
            throw e;
        }
    }

    function validationForExperience() {
        try {
            var sDate = $filter('dateFiltering')($scope.preRecruitmentEmpExperienceNew.StartDate, 'dd-MM-yyyy');
            var eDate = $filter('dateFiltering')($scope.preRecruitmentEmpExperienceNew.EndDate, 'dd-MM-yyyy');
            if ($scope.preRecruitmentEmpExperienceNew.IsCurrentJob === false) {
                if (new Date(sDate) === new Date(eDate) || new Date(sDate) > new Date(eDate)) {
                    throw "End Date must be greater than Start Date !!!";
                }
                else {
                    //
                }
            }
        } catch (e) {
            throw e;
        }
    }

    // #endregion

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.QualificationDocument + '/' + data.FileId + extention;
    };

    $scope.ExperienceFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.ExperienceDocument + '/' + data.FileId + extention;
    };

    $scope.TrainingFileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.TrainingDocument + '/' + data.FileId + extention;
    };

    $scope.fileId = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $("#uploadBtn").change(function () {
        $scope.filedata = this.files[0];
    });
    $("#uploadBtn2").change(function () {
        $scope.filedata = this.files[0];
    });
    $("#uploadBtn3").change(function () {
        $scope.filedata = this.files[0];
    });

    $("#uploadBtn4").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getNum = function () {
        if ($scope.preRecruitmentEmployee.IsKnownPerson)
            $scope.preRecruitmentEmployee.NumberOfKnownPerson = 0;
        else
            $scope.preRecruitmentEmployee.NumberOfKnownPerson = 1;
    };

    $scope.Save = function () {
        try {
            //$scope.currentDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');
            //var cd = new Date($scope.currentDate);
            //var bd = new Date($scope.preRecruitmentEmployee.DOB);
            //if (bd >= cd) {
            //    throw "DOB can not equal or greater than current date.";
            //}
            Validation();

            if (isNaN($scope.preRecruitmentEmployee.Phone)) {
                throw "Enter the valid Phone Number";
            }
            //if ($scope.preRecruitmentEmployee.Phone.length != $rootScope.PhoneLength) {
            //    throw "Phone Number must be " + $rootScope.PhoneLength + " character.";
            //}
            if (isNaN($scope.preRecruitmentEmployee.NumberOfKnownPerson)) {
                throw "Enter the valid Number";
            }
            if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.preRecruitmentEmployee.Email)) {
                //
            } else {
                throw "You have entered an invalid email address.";
            }

            if ($scope.preRecruitmentEmployee.IsKnownPerson === true && baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.NumberOfKnownPerson)) {
                throw "Number Of Known Person is required.";
            }
            if ($scope.preRecruitmentEmployee.NumberOfKnownPerson < 0) {
                throw "Number of known person can not be less than zero";
            }
            //if ($scope.preRecruitmentEmployee.NationalID.length != $scope.NidLength) {
            //    throw "" + $scope.Nid + " must be " + $scope.NidLength + " character.";
            //}

            $scope.savedisable = true;
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                var picData = new FormData();
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'create',
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            picData.append("preRecruitmentEmployee", angular.toJson(data.preRecruitmentEmployee));
                            if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                                picData.append('file', data.file);
                            }
                            return picData;
                        },
                        data: {
                            'preRecruitmentEmployee': $scope.preRecruitmentEmployee
                            , 'file': $scope.picdata
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                            $scope.savedisable = false;
                            $scope.showdiv = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.savedisable = false;
                            $scope.showdiv = true;
                            $scope.preRecruitmentEmployee.Image = response.data.PreRecruitmentEmployee.Image;
                            $scope.preRecruitmentEmployee.IsKnownPerson = response.data.PreRecruitmentEmployee.IsKnownPerson;
                            $scope.preRecruitmentEmployee.NumberOfKnownPerson = response.data.PreRecruitmentEmployee.NumberOfKnownPerson;
                            $scope.preRecruitmentEmployee.NationalID = response.data.PreRecruitmentEmployee.NationalID;
                            $scope.Loaddocumentdatalist();
                        }
                    }, function errorCallback(response) {
                        $scope.savedisable = false;
                        $scope.showdiv = false;
                    });
                    return true;
                }
            }
        } catch (e) {
            $scope.savedisable = false;
            $scope.showdiv = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SavePersonal = function () {
        try {
            ValidationMaster();
            if ($scope.preRecruitmentEmployee.FatherName === $scope.preRecruitmentEmployee.FullName) {
                throw "Your name and your father name can not be same.";
            }

            //if ($scope.SalaryRangeForTaxRequired) {
            //    if ($scope.TotalSalary > $scope.SalaryRangeForTax && baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.TIN)) {
            //        throw "" + $scope.Tin + " No is required as per company rule.";
            //    }
            //}

            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.TIN)) {
            //    if ($scope.preRecruitmentEmployee.TIN.length != $scope.TinLength) {
            //        throw "" + $scope.Tin + " must be " + $scope.TinLength + " character.";
            //    }
            //}

            $scope.celebrationType = $.grep($scope.civilStatusList, function (item) {
                return item.Value === $scope.preRecruitmentEmployee.CivilStatusID;
            })[0].HasPartner;
            if ($scope.celebrationType) {
                //if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.MarriagedayCelebrationDate)) {
                //    throw "Marriage day Celebration Date is required.";
                //}
                if (new Date() < new Date($scope.preRecruitmentEmployee.MarriagedayCelebrationDate)) {
                    throw "Marriage day Celebration Date is can not be greater than todays date.";
                }
                //if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.SpouseName)) {
                //    throw "Spouse Name is required.";
                //}
            }

            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.SpouseNationalID)) {
            //    if ($scope.preRecruitmentEmployee.SpouseNationalID.length != $scope.NidLength) {
            //        throw "Spouse " + $scope.Nid + " must be " + $scope.NidLength + " character.";
            //    }
            //}

            $scope.savedisable = true;
            //$scope.$broadcast('show-errors-check-validity');
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'createpersonal',
                        data: $scope.preRecruitmentEmployee,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            $scope.savedisable = false;
                            ShowResult(response.data.Message, "success");
                        }
                    }, function errorCallback(response) {
                        $scope.savedisable = false;
                        //ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    $scope.SaveAddress = function () {
        try {
            if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.PresentAddress1)) {
                throw "Present Address 1 is required";
            }
            if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.ParmanentAddress1)) {
                throw "Parmanent Address 1 is required";
            }
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer1CellNo)) {
                    throw "Enter the valid Cell Number in Cell No1";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer1CellNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo2)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer1CellNo2)) {
                    throw "Enter the valid Cell Number in Cell No2";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo2)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer1CellNo2.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo3)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer1CellNo3)) {
                    throw "Enter the valid Cell Number in Cell No3";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer1CellNo3)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer1CellNo3.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer2CellNo)) {
                    throw "Enter the valid Cell Number in Cell No1";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer2CellNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo2)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer2CellNo2)) {
                    throw "Enter the valid Cell Number in Cell No2";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo2)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer2CellNo2.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo3)) {
                if (isNaN($scope.preRecruitmentEmployee.EmrCntPer2CellNo3)) {
                    throw "Enter the valid Cell Number in Cell No3";
                }
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.EmrCntPer2CellNo3)) {
            //    if ($scope.preRecruitmentEmployee.EmrCntPer2CellNo3.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            $scope.savedisable = true;
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'createaddress',
                        data: $scope.preRecruitmentEmployee,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                            $scope.savedisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.savedisable = false;
                        }
                    }, function errorCallback(response) {
                        //ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    // #region SaveReference

    $scope.SaveReference = function () {
        try {
            if (isNaN($scope.preRecruitmentEmpReference.Ref1CellPhnNo)) {
                throw "Enter the valid Cell Number";
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpReference.Ref1CellPhnNo)) {
            //    if ($scope.preRecruitmentEmpReference.Ref1CellPhnNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (isNaN($scope.preRecruitmentEmpReference.Ref2CellPhnNo)) {
                throw "Enter the valid Cell Number";
            }
            //if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpReference.Ref2CellPhnNo)) {
            //    if ($scope.preRecruitmentEmpReference.Ref2CellPhnNo.length != $rootScope.PhoneLength) {
            //        throw "Cell Number must be " + $rootScope.PhoneLength + " character.";
            //    }
            //}
            if (isNaN($scope.preRecruitmentEmpReference.Ref1TelePhnNo)) {
                throw "Enter the valid Tele Phone Number";
            }
            if (isNaN($scope.preRecruitmentEmpReference.Ref2TelePhnNo)) {
                throw "Enter the valid Tele Phone Number";
            }
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpReference.Ref1Email)) {
                if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.preRecruitmentEmpReference.Ref1Email)) {
                    //
                } else {
                    throw "You have entered an invalid email address.";
                }
            }
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpReference.Ref2Email)) {
                if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,8})+$/.test($scope.preRecruitmentEmpReference.Ref2Email)) {
                    //
                } else {
                    throw "You have entered an invalid email address.";
                }
            }
            $scope.savedisable = true;
            // $scope.$broadcast('show-errors-check-validity');
            if ($scope.preRecruitmentEmployeeForm1.$valid) {
                $scope.preRecruitmentEmpReference.PreRecruitmentEmployeeId = $scope.user;
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: 'Recruitments/home/createreference',
                        data: $scope.preRecruitmentEmpReference,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                            $scope.savedisable = false;
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.savedisable = false;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            $scope.savedisable = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region SaveQualification

    $scope.SaveQualification = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb.';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpQualificationNew.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpQualificationNew.FileName)) {
                if ($scope.preRecruitmentEmpQualificationNew.FileName.length > 50) {
                    throw "File Name must be less than 50 character."
                }
            }
            $scope.preRecruitmentEmpQualificationNew.FileId = $scope.fileId();

            ValidateQualification();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpQualificationNew.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;

            var formData = new FormData();

            if ($scope.preRecruitmentEmpQualifications.length <= 0 && baseService.isUndefinedOrNull($scope.preRecruitmentEmpQualificationNew.FileName)) {
                throw "Attachment is mandatory.";
            }

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createqualification',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpQualificationNew", angular.toJson(data.preRecruitmentEmpQualificationNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpQualificationNew': $scope.preRecruitmentEmpQualificationNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        $scope.savedisable = false;
                        ShowResult(response.data.Message, "failure", "QualificationPopUp");
                    }
                    else {
                        ShowResult(response.data.Message, "success", "QualificationPopUp");
                        $scope.btnDisable = false;
                        $scope.LoadQualificationData();
                        $scope.Clear();
                        $scope.savedisable = false;
                        ClearFile();
                        $scope.preRecruitmentEmpQualificationNew.SystemID = null;
                        addressService.getCountryCbo(function (result) {
                            $scope.CountryList = result;
                            $scope.preRecruitmentEmpQualificationNew.CountryId = $rootScope.CountryId;
                        });
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "QualificationPopUp");
                    $scope.savedisable = false;
                });
                return true;
            }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "QualificationPopUp");
        }
    };

    // #endregion

    // #region SaveTraining

    document.getElementById("uploadBtn").onchange = function () {
        var filename = document.getElementById("uploadFile").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile").value = res;
    };
    document.getElementById("uploadBtn2").onchange = function () {
        var filename = document.getElementById("uploadFile2").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile2").value = res;
    };
    document.getElementById("uploadBtn3").onchange = function () {
        var filename = document.getElementById("uploadFile3").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile3").value = res;
    };

    $scope.SaveTraining = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpTrainingNew.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpTrainingNew.FileName)) {
                if ($scope.preRecruitmentEmpTrainingNew.FileName.length > 50) {
                    throw "File Name must be less than 50 character."
                }
            }
            $scope.preRecruitmentEmpTrainingNew.FileId = $scope.fileId();

            ValidateTraining();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpTrainingNew.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;
            var formData = new FormData();
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createtraining',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpTrainingNew", angular.toJson(data.preRecruitmentEmpTrainingNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpTrainingNew': $scope.preRecruitmentEmpTrainingNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        $scope.savedisable = false;
                        ShowResult(response.data.Message, "failure", "TrainingPopUp");
                    }
                    else {
                        ShowResult(response.data.Message, "success", "TrainingPopUp");
                        $scope.btnDisable = false;
                        $scope.LoadTrainingData();
                        $scope.Clear();
                        $scope.savedisable = false;
                        ClearFile();
                        $scope.preRecruitmentEmpTrainingNew.SystemID = null;
                        addressService.getCountryCbo(function (result) {
                            $scope.CountryList = result;
                            $scope.preRecruitmentEmpTrainingNew.CountryId = $rootScope.CountryId;
                        });
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "TrainingPopUp");
                    $scope.savedisable = false;
                });
                return true;
            }
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "TrainingPopUp");
        }
    };

    // #endregion

    // #region SaveExperience

    $scope.SaveExperience = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpExperienceNew.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.preRecruitmentEmpExperienceNew.FileName)) {
                if ($scope.preRecruitmentEmpExperienceNew.FileName.length > 50) {
                    throw "File Name must be less than 50 character."
                }
            }
            $scope.preRecruitmentEmpExperienceNew.FileId = $scope.fileId();

            ValidateExperience();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpExperienceNew.PreRecruitmentEmployeeId = $scope.user;
            validationForExperience();
            $scope.btnDisable = true;
            var formData = new FormData();

            //if ($scope.preRecruitmentEmpExperiences.length <= 0 && baseService.isUndefinedOrNull($scope.preRecruitmentEmpExperienceNew.FileName)) {
            //    throw "Attachment is mandatory.";
            //}

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createexperience',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpExperienceNew", angular.toJson(data.preRecruitmentEmpExperienceNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpExperienceNew': $scope.preRecruitmentEmpExperienceNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure", "ExperiencePopUp");
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success", "ExperiencePopUp");
                        $scope.btnDisable = false;
                        $scope.LoadExperienceData();
                        $scope.Clear();
                        $scope.savedisable = false;
                        ClearFile();
                        $scope.preRecruitmentEmpExperienceNew.SystemID = null;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "ExperiencePopUp");
                    $scope.savedisable = false;
                });
                //angular.element(document.querySelector('#ExperiencePopUp')).modal('hide');
                return true;
            }
            // }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "ExperiencePopUp");
        }
    };

    // #endregion

    $scope.setHeight = function (id) {
        var element = angular.element(document.getElementById(id));
        $scope.height = element[0].scrollHeight;
    }

    $scope.yearList = [];
    $scope.SetYearOfPassing = function () {
        var endYear = new Date();
        var ey = parseInt(endYear.getFullYear());
        for (var i = ey; i > 1945 - 1; i--) {
            var ob = {
                Value: i,
                Text: i
            };
            $scope.yearList.push(ob);
        }
    };

    $scope.indexQua = -1;
    $scope.QualificationData = function (data, index) {
        $scope.preRecruitmentEmpQualificationNew = Object.assign({}, data);
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.preRecruitmentEmpQualificationNew.FileName = data.FileName;
        var filename = document.getElementById("uploadFile").value = data.FileName;
        $scope.preRecruitmentEmpQualificationNew.YearOfPass = data.YearOfPass.toString();
        $scope.SetYearOfPassing();
        $scope.indexQua = index;
    };

    $scope.indexTrn = -1;
    $scope.TrainingData = function (data, index) {
        $scope.preRecruitmentEmpTrainingNew = Object.assign({}, data);
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.preRecruitmentEmpTrainingNew.FileName = data.FileName;
        var filename = document.getElementById("uploadFile2").value = data.FileName;
        $scope.indexTrn = index;
    };

    $scope.indexExp = -1;
    $scope.ExperienceData = function (data, index) {
        $scope.preRecruitmentEmpExperienceNew = Object.assign({}, data);
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.preRecruitmentEmpExperienceNew.FileName = data.FileName;
        var filename = document.getElementById("uploadFile3").value = data.FileName;
        $scope.indexExp = index;
    };

    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
    });

    $scope.qualificationShow = function () {
        $scope.SetYearOfPassing();
        addressService.getCountryCbo(function (result) {
            $scope.CountryList = result;
            $scope.preRecruitmentEmpQualificationNew.CountryId = $rootScope.CountryId;
        });
        $scope.Clear();
        angular.element(document.querySelector('#QualificationPopUp')).modal('show');
    }

    $scope.TrainingShow = function () {
        addressService.getCountryCbo(function (result) {
            $scope.CountryList = result;
            $scope.preRecruitmentEmpTrainingNew.CountrySystemID = $rootScope.CountryId;
        });
        $scope.Clear();
        angular.element(document.querySelector('#TrainingPopUp')).modal('show');
    }

    $scope.ExperienceShow = function () {
        $scope.Clear();
        $scope.preRecruitmentEmpExperienceNew.EndDate = $filter('dateFiltering')(Date.now(), 'dd-MM-yyyy');
        $scope.countDate();
        angular.element(document.querySelector('#ExperiencePopUp')).modal('show');
    };

    $scope.confirmQualificationDelete = function (Id) {
        $scope.deleteQualificationId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteQualification = function () {
        $http({
            method: 'POST',
            url: 'Recruitments/home/deletequalification',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteQualificationId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadQualificationData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.QualificationRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmQualiDelete')).modal('show');
    };

    $scope.removeQualification = function () {
        angular.element(document.querySelector('#confirmQualiDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpQualificationNew.SystemID)) {
            document.getElementById('uploadBtn').value = '';
            $scope.filedata = '';
            $scope.preRecruitmentEmpQualificationNew.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile').value = "";
        }
        else {
            $scope.ClearQualification();
        }
    };

    $scope.confirmCloseQualificationDelete = function () {
        angular.element(document.querySelector('#confirmQualiDelete')).modal('hide');
    };

    $scope.SaveQualific = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpQualificationNew.FileName = fileName;
            $scope.preRecruitmentEmpQualificationNew.FileId = $scope.fileId();

            ValidateQualification();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpQualificationNew.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;

            var formData = new FormData();

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createqualification',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpQualificationNew", angular.toJson(data.preRecruitmentEmpQualificationNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpQualificationNew': $scope.preRecruitmentEmpQualificationNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        $scope.savedisable = false;
                        ShowResult(response.data.Message, "failure", "QualificationPopUp");
                    }
                    else {
                        ShowResult(response.data.Message, "success", "QualificationPopUp");
                        $scope.btnDisable = false;
                        $scope.LoadQualificationData();
                        $scope.savedisable = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "QualificationPopUp");
                });
                return true;
            }
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "QualificationPopUp");
        }
    };

    $scope.ClearQualification = function () {
        document.getElementById('uploadBtn').value = '';
        $scope.filedata = '';
        $scope.preRecruitmentEmpQualificationNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile').value = "";
        $scope.SaveQualific();
    };

    $scope.confirmTrainingDelete = function (Id) {
        $scope.deleteTrainingId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteTraining = function () {
        $http({
            method: 'POST',
            url: 'Recruitments/home/deletetraining',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteTrainingId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadTrainingData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.TrainingRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmTrainDelete')).modal('show');
    };

    $scope.removeTraining = function () {
        angular.element(document.querySelector('#confirmTrainDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpTrainingNew.SystemID)) {
            document.getElementById('uploadBtn2').value = '';
            $scope.filedata = '';
            $scope.preRecruitmentEmpTrainingNew.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile2').value = "";
        }
        else {
            $scope.ClearTraining();
        }
    };

    $scope.confirmCloseTrainingDelete = function () {
        angular.element(document.querySelector('#confirmTrainDelete')).modal('hide');
    };

    $scope.SaveTrain = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpTrainingNew.FileName = fileName;
            $scope.preRecruitmentEmpTrainingNew.FileId = $scope.fileId();

            ValidateTraining();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpTrainingNew.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;
            var formData = new FormData();
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createtraining',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpTrainingNew", angular.toJson(data.preRecruitmentEmpTrainingNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpTrainingNew': $scope.preRecruitmentEmpTrainingNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        $scope.savedisable = false;
                        ShowResult(response.data.Message, "failure", "TrainingPopUp");
                    }
                    else {
                        ShowResult(response.data.Message, "success", "TrainingPopUp");
                        $scope.btnDisable = false;
                        $scope.LoadTrainingData();
                        $scope.savedisable = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "TrainingPopUp");
                });
                return true;
            }
            // }
        } catch (e) {
            $scope.savedisable = false;
            $scope.btnDisable = false;
            ShowResult(e, "failure", "TrainingPopUp");
        }
    };

    $scope.ClearTraining = function () {
        document.getElementById('uploadBtn2').value = '';
        $scope.filedata = '';
        $scope.preRecruitmentEmpTrainingNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile2').value = "";
        $scope.SaveTrain();
    };

    $scope.confirmExperienceDelete = function (Id) {
        $scope.deleteExperienceId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "]? ";
    };

    $scope.DeleteExperience = function () {
        $http({
            method: 'POST',
            url: 'Recruitments/home/deleteexperience',
            dataType: 'JSON',
            data: { 'Id': $scope.deleteExperienceId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadExperienceData();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.ExperienceRemove = function () {
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmExpDelete')).modal('show');
    };

    $scope.removeExperience = function () {
        angular.element(document.querySelector('#confirmExpDelete')).modal('hide');
        if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpExperienceNew.SystemID)) {
            document.getElementById('uploadBtn3').value = '';
            $scope.filedata = '';
            $scope.preRecruitmentEmpExperienceNew.FileName = "";
            $scope.filedata = {};
            document.getElementById('uploadFile3').value = "";
        }
        else {
            $scope.ClearExperience();
        }
    };

    $scope.confirmCloseExperienceDelete = function () {
        angular.element(document.querySelector('#confirmExpDelete')).modal('hide');
    };

    $scope.SaveExp = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.preRecruitmentEmpExperienceNew.FileName = fileName;
            $scope.preRecruitmentEmpExperienceNew.FileId = $scope.fileId();

            ValidateExperience();
            $scope.savedisable = true;
            $scope.preRecruitmentEmpExperienceNew.PreRecruitmentEmployeeId = $scope.user;
            validationForExperience();
            $scope.btnDisable = true;
            var formData = new FormData();
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'Recruitments/home/createexperience',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentEmpExperienceNew", angular.toJson(data.preRecruitmentEmpExperienceNew));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentEmpExperienceNew': $scope.preRecruitmentEmpExperienceNew, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure", "ExperiencePopUp");
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success", "ExperiencePopUp");
                        $scope.btnDisable = false;
                        $scope.LoadExperienceData();
                        $scope.savedisable = false;
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "ExperiencePopUp");
                });
                //angular.element(document.querySelector('#ExperiencePopUp')).modal('hide');
                return true;
            }
            // }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "ExperiencePopUp");
        }
    };

    $scope.ClearExperience = function () {
        document.getElementById('uploadBtn3').value = '';
        $scope.filedata = '';
        $scope.preRecruitmentEmpQualificationNew.FileName = "";
        $scope.filedata = {};
        document.getElementById('uploadFile3').value = "";
        $scope.SaveExp();
    };

    $scope.Clear = function () {
        ClearFields();
        ClearFile();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.preRecruitmentEmpQualificationNew = {};
        $scope.preRecruitmentEmpTrainingNew = {};
        $scope.preRecruitmentEmpExperienceNew = {};
    }

    function ClearFile() {
        document.getElementById('uploadBtn').value = '';
        document.getElementById('uploadBtn2').value = '';
        document.getElementById('uploadBtn3').value = '';
        $scope.filedata = '';
        $scope.preRecruitmentEmpQualificationNew.FileName = "";
        $scope.preRecruitmentEmpTrainingNew.FileName = "";
        $scope.preRecruitmentEmpExperienceNew.FileName = "";
        document.getElementById('uploadFile').value = "";
        document.getElementById('uploadFile2').value = "";
        document.getElementById('uploadFile3').value = "";
        $scope.filedata = {};
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };

    // #region Document

    $scope.Loaddocumentdatalist = function () {
        $http.get('recruitments/home/getdocumentdatalist?companyGroupId=' + $rootScope.CompanyGroupId + '&budgetId=' + $rootScope.BudgetId + '&pId=' + $scope.user + '&plantId=' + $rootScope.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
                //$scope.getColor($scope.documentdataList.FileName);
            });
    };

    $scope.getInd = function (idx, dt) {
        $scope.indext = idx;
        $scope.documentData = dt;
    }

    $scope.docList = [];
    $scope.preRecruitmentDocumentList = [];
    $scope.fileNameChanged = function (d) {
        $scope.filedata = [];
        try {
            var tempInd = $scope.indext;
            var filename = d.value;
            var res = filename.replace(/C:\\fakepath\\/i, '');
            document.getElementById("" + tempInd + "").value = res;
            $scope.filedata = d.files[0];

            var fName = res;
            if (checkFileExist($scope.preRecruitmentDocumentList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if (checkSameFileExist($scope.documentdataList, fName)) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' This file already added, Please choose another one.';
            }

            if ($scope.filedata.size > 2000000) {
                document.getElementById("" + tempInd + "").value = "";
                throw fName + ' File size must be below 2 mb';
            }
            $scope.preRecruitmentDocumentList.push($scope.filedata);

            var nn = $scope.documentData;
            nn.FileName = fName;
            if (nn.FileName.length > 50) {
                throw "File Name must be less than 50 character.";
            }
            nn.PreRecruitmentEmployeeId = $scope.user;
            $scope.docList.push(nn);
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    function checkFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].name === name) {
                return true;
            }
        }
        return false;
    }
    function checkSameFileExist(list, name) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FileName === name) {
                return true;
            }
        }
        return false;
    }

    $scope.fg = false;
    $scope.DocShow = function (data) {
        $scope.documentdata = data;
        $scope.filedata = {};
        if (!baseService.isUndefinedOrNull(data.FileName))
            $scope.filedata.name = data.FileName;
        else
            $scope.filedata = null;
        $scope.documentdata.FileName = data.FileName;
        var filename = document.getElementById("uploadFile").value = data.FileName;

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.NationalID)) {
                $scope.documentdata.DocNumber = $scope.NationalID;
            }
            else {
                $scope.documentdata.DocNumber = $scope.preRecruitmentEmployee.NationalID;
            }
        }

        if ($scope.documentdata.ProfileType === 'NID') {
            if (!baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = true;
            }
            else if (baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                $scope.fg = false;
            }
        }
        angular.element(document.querySelector('#DocPopUp')).modal('show');
    };

    $scope.getColor = function (item) {
        var remark = item.FileName;
        if (remark === null || remark === '') {
            return 'empty';
        } else {
            return 'filled';
        }
    };

    document.getElementById("uploadBtn4").onchange = function () {
        var filename = document.getElementById("uploadFile4").value = this.value;
        var res = filename.replace(/C:\\fakepath\\/i, '');
        document.getElementById("uploadFile4").value = res;
    };

    $scope.SaveDocument = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.filedata) && $scope.filedata.size > 2000000)
                throw $scope.filedata.name + ' File size must be below 2 mb';
            var fileName = null;
            if (!baseService.isUndefinedOrNull($scope.filedata))
                fileName = $scope.filedata.name;
            $scope.documentdata.FileName = fileName;
            if (!baseService.isUndefinedOrNull($scope.documentdata.FileName)) {
                if ($scope.documentdata.FileName.length > 50) {
                    throw "File Name must be less than 50 character.";
                }
            }

            if ($scope.documentdata.DocNumberRequired === true) {
                if (baseService.isUndefinedOrNull($scope.documentdata.DocNumber)) {
                    throw "Document Number is required.";
                }
            }
            if ($scope.documentdata.DocDateRequired === true) {
                if (baseService.isUndefinedOrNull($scope.documentdata.DocDate)) {
                    throw "Document Date is required.";
                }
            }

            if ($scope.documentdata.OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdata.FileName)) {
                throw 'File attachment is Mandatory';
            }

            $scope.savedisable = true;
            //$scope.documentdata.PreRecruitmentEmployeeId = $scope.user;
            $scope.btnDisable = true;
            var formData = new FormData();

            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'recruitments/home/createdocument',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        formData.append("preRecruitmentDocument", angular.toJson(data.preRecruitmentDocument));
                        if (baseService.isUndefinedOrNull($scope.filedata) === false) {
                            formData.append('file', data.file);
                        }
                        return formData;
                    },
                    data: { 'preRecruitmentDocument': $scope.documentdata, 'file': $scope.filedata }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure", "DocPopUp");
                        $scope.savedisable = false;
                    }
                    else {
                        ShowResult(response.data.Message, "success", "DocPopUp");
                        $scope.btnDisable = false;
                        $scope.Loaddocumentdatalist();
                        $scope.filedata = {};
                        $scope.savedisable = false;
                        angular.element(document.querySelector('#DocPopUp')).modal('hide');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure", "DocPopUp");
                    $scope.savedisable = false;
                });
                return true;
            }
        } catch (e) {
            $scope.btnDisable = false;
            $scope.savedisable = false;
            ShowResult(e, "failure", "DocPopUp");
        }
    };

    $scope.DocDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.PreRecruitmentDocument + '/' + data.FileId + extention;
    };

    $scope.DocumentRemove = function (id) {
        $scope.idd = id;
        $scope.message_confirmation = 'Are you sure to remove this file?';
        angular.element(document.querySelector('#confirmDocDelete')).modal('show');
        $scope.docList = [];
        $scope.preRecruitmentDocumentList = [];
        $scope.filedata = {};
    };
    $scope.removeDoc = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
        $http({
            method: 'POST',
            url: 'Recruitments/home/deletedocument?Id=' + $scope.idd,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure', "DocPopUp");
            }
            else {
                ShowResult(response.data.Message, 'success', "DocPopUp");
                $scope.Loaddocumentdatalist();
                $scope.documentdata.FileName = "";
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure', "DocPopUp");
        });
        return true;
    };

    $scope.confirmCloseDocDelete = function () {
        angular.element(document.querySelector('#confirmDocDelete')).modal('hide');
    };
    $scope.confirmSubmit = function () {
        $scope.confirm = $scope.user;
        //$scope.message_confirmation = "Are you sure to Submit? <br/> You won't be able to change any information after submission.";
        $scope.message_confirmation = "Are you sure you want to submit? You won’t be able to modify your data after this.";
        angular.element(document.querySelector('#confirmSubmit')).modal('show');
    };

    function fileValidation() {
        for (var i = 0; i < $scope.documentdataList.length; i++) {
            if ($scope.documentdataList[i].OptionalOrMandatory === 'Mandatory' && baseService.isUndefinedOrNull($scope.documentdataList[i].FileName)) {
                throw "File is 	Mandatory for " + $scope.documentdataList[i].DocumentName + ".";
            }
        }
    }

    function QualyfileValidation() {
        for (var i = 0; i < $scope.preRecruitmentEmpQualifications.length; i++) {
            if (i === 0) {
                if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpQualifications[i].FileName)) {
                    throw "File is 	Mandatory qualification tab for Education Level " + $scope.preRecruitmentEmpQualifications[i].EducationLevel + ".";
                }
            } else {
                break;
            }
        }
    }

    function TrainfileValidation() {
        for (var i = 0; i < $scope.preRecruitmentEmpTrainings.length; i++) {
            if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpTrainings[i].FileName)) {
                throw "File is 	Mandatory for Training Title " + $scope.preRecruitmentEmpTrainings[i].TrainingTitle + ".";
            }
        }
    }

    function ExpfileValidation() {
        for (var i = 0; i < $scope.preRecruitmentEmpExperiences.length; i++) {
            if (i === 0) {
                if (baseService.isUndefinedOrNull($scope.preRecruitmentEmpExperiences[i].FileName)) {
                    throw "File is 	Mandatory in experience tab for Employer " + $scope.preRecruitmentEmpExperiences[i].Employer + ".";
                }
            } else {
                break;
            }
        }
    }

    $scope.SaveSalary = function () {
        $scope.salaryHeadList = [];
        CombineList($scope.salaryHeadList);
        for (var i = 0; i < $scope.salaryHeadList.length; i++) {
            $scope.salaryHeadList[i].PreRecruitmentEmployeeID = $scope.user;
            $scope.salaryHeadList[i].AddedBy = $scope.preRecruitmentEmployee.FirstName;
            $scope.salaryHeadList[i].AddedDate = new Date();
        }
        if ($scope.Action === "Save") {
            $http({
                method: 'POST',
                url: 'humanresource/salaryfixation/createsalary',
                data: { 'salaryFixationList': $scope.salaryHeadList, 'companyGroupId': $rootScope.CompanyGroupId, 'plantid': $rootScope.PlantId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.salaryHeadList = response.data.SalaryFixation;
                    //$scope.ClearFields();
                }
            }, function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            });
        }
        //}
    };

    $scope.Submit = function () {
        try {
            Validation();
            ValidationMaster();
            if (baseService.isUndefinedOrNull($scope.preRecruitmentEmployee.Image)) {
                throw "Please upload your Image.";
            }
            QualyfileValidation();
            //TrainfileValidation();
            ExpfileValidation();
            fileValidation();
            //$scope.$broadcast('show-errors-check-validity');
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl = $scope.path + 'createfinal',
                        data: { 'Id': $scope.confirm, 'preRecruitmentEmployee': $scope.preRecruitmentEmployee },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.preRecruitmentEmployee.Submitted = true;
                        }
                    }, function errorCallback(response) {
                        //ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Logout = function () {
        $http({
            method: 'GET',
            url: 'recruitments/home/Logout'
        }).then(function (result) {
            $window.location.href = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath + '/prerecruitment?Id=' + $scope.user;
            //$window.location.href = $location.protocol() + '://' + $location.host() + ':' + $location.port() + result.data.BasePath;
        });
    };
}