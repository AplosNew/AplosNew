'use strict';
preRecruitmentEmployeeController.$inject = ['addressService', 'fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function preRecruitmentEmployeeController(addressService, fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Pre Recruitment Employee';
    $scope.Action = 'Save';
    $scope.imageSrc = null;
    $scope.imageBtnDisable = false;
    $scope.index = -1;
    $scope.preRecruitmentEmployees = [];
    $scope.path = 'employees/prerecruitmentemployee/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    var Id = null;

    $scope.LoadData = function () {
        $http.get($scope.getListUrl)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.preRecruitmentEmployee = response.data[0];
                }
            });
    };
    $scope.LoadData();

    $scope.LoadReferenceData = function () {
        $http.get('employees/prerecruitmentemployee/getreferencedata')
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.preRecruitmentEmpReference = response.data[0];
                }
            });
    };
    $scope.LoadReferenceData();

    $scope.preRecruitmentEmployee = {
        Id: null,
        Image: null,
        InterviewRankingId: null,
        GroupID: null,
        CompanyId: null,
        PlantId: null,
        UnitId: null,
        SubdivisionID: null,
        DivisionId: null,
        DepartmentId: null,
        SectionId: null,
        SubSectionId: null,
        LineId: null,
        DesignationGroupId: null,
        DesignationSystemID: null,
        PositionID: null,
        BudgetCode: null,
        IsDirect: true,
        FullName: null,
        Gender: null,
        NationalID: null,
        DOB: null,
        Phone: null,
        Email: null,
        Salutation: null,
        FirstName: null,
        MiddleName: null,
        LastName: null,
        NickName: null,
        EmployeeName: null,
        CivilStatus: null,
        EmpType: null,
        Status: null,
        NatureOfEmployement: null,
        TIN: null,
        FatherName: null,
        MotherName: null,
        CitizenID: null,
        ReligionID: null,
        CivilStatusID: null,
        ResignationID: null,
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
        EmrCntPer2CellNo: null
    };
    function setUserImage(data) {
        if (!baseService.isUndefinedOrNull(data.SystemId)) {
            $scope.imageSrc = $rootScope.EmployeeImage + data.Image;
            $scope.imageBtnDisable = true;
            $scope.preRecruitmentEmployee.Image = data.Image;
        }
        else {
            $scope.imageBtnDisable = false;
            $scope.preRecruitmentEmployee.Image = null;
        }
    }
    $scope.filedata = null;
    $("#uploadImage").change(function () {
        $scope.filedata = this.files[0];
    });
    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };
    $scope.preRecruitmentEmpReference = {
        SystemID: null,
        EmpSystemID: $scope.preRecruitmentEmployee.Id,
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
        EmpSystemID: $scope.preRecruitmentEmployee.Id,
        EductLevelSystemID: null,
        StreamId: null,
        CountryId: null,
        HasDistinction: true,
        Session: null,
        IsGeneral: true,
        IsEnglishMedium: true,
        IsMadrasah: true,
        IsVocational: true,
        IsOther: true,
        OtherEductType: null,
        ExamDegreeType: null,
        ConcMajor: null,
        InstituteName: null,
        IsForeignInstitute: true,
        ResultSystemID: null,
        Marks: null,
        CGPA: null,
        Scale: null,
        YearOfPass: null,
        Duration: null,
        Achievement: null
    };

    $scope.preRecruitmentEmpTraining = {
        SystemID: null,
        EmpSystemID: $scope.preRecruitmentEmployee.Id,
        TrainingTitle: null,
        TopicCovered: null,
        IntituteName: null,
        CountrySystemID: null,
        Location: null,
        TrainingYear: null,
        Duration: null
    };

    $scope.preRecruitmentEmpExperience = {
        SystemID: null,
        EmpSystemID: $scope.preRecruitmentEmployee.Id,
        Employer: null,
        Designation: null,
        StartDate: null,
        EndDate: null,
        JobDescription: null,
        Achievement: null
    };

    // #region AllDropDown

    cboService.getCboReligion(function (result) {
        $scope.religionList = result;
    });

    cboService.getCboResignation(function (result) {
        $scope.ResignationList = result;
    });

    addressService.getCboPostOffice(function (result) {
        $scope.PostOfficeList = result;
    });

    addressService.getCboThana(function (result) {
        $scope.ThanaList = result;
    });

    addressService.getCboDistrict(function (result) {
        $scope.DistrictList = result;
    });

    addressService.getCountryCbo(function (result) {
        $scope.CountryList = result;
        $scope.citizenList = result;
    });

    addressService.getCboCity(function (result) {
        $scope.CityList = result;
    });

    addressService.getCboArea(function (result) {
        $scope.AreaList = result;
    });

    cboService.getCboQualificationLevel(function (result) {
        $scope.EductLevelSystemList = result;
    });

    cboService.getCboQualificationStream(function (result) {
        $scope.StreamList = result;
    });

    // #endregion

    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                var formData = new FormData();
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        headers: { 'Content-Type': undefined },
                        transformRequest: function (data) {
                            formData.append("preRecruitmentEmployee", angular.toJson(data.preRecruitmentEmployee));
                            formData.append('file', data.file);
                            return formData;
                        },
                        data: {
                            'preRecruitmentEmployee': $scope.preRecruitmentEmployee
                            , 'file': $scope.filedata
                        }
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            $scope.preRecruitmentEmployee.Id = response.data.empId;
                            $scope.preRecruitmentEmployee.Id = $rootScope.EmployeeImage + $scope.preRecruitmentEmployee.Image;
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

    // #region SaveReference

    $scope.SaveReference = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.preRecruitmentEmployeeForm.$valid) {
                //angular.copy($scope.preRecruitmentEmployeeNew, $scope.preRecruitmentEmployee);
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST',
                        url: 'employees/prerecruitmentemployee/createreference',
                        data: $scope.preRecruitmentEmpReference,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, "failure");
                        }
                        else {
                            ShowResult(response.data.Message, "success");
                            preRecruitmentEmpReference.SystemID = response.data.empId;
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, "failure");
                    });
                    return true;
                }
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region SaveQualification

    $scope.SaveQualification = function () {
        try {
            $scope.btnDisable = true;
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'employees/prerecruitmentemployee/createqualification',
                    data: $scope.preRecruitmentEmpQualification,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.btnDisable = false;
                        $scope.LoadQualificationData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                return true;
            }
            // }
        } catch (e) {
            $scope.btnDisable = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region SaveTraining

    $scope.SaveTraining = function () {
        try {
            $scope.btnDisable = true;
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'employees/prerecruitmentemployee/createtraining',
                    data: $scope.preRecruitmentEmpTraining,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.btnDisable = false;
                        $scope.LoadTrainingData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                angular.element(document.querySelector('#TrainingPopUp')).modal('hide');
                return true;
            }
            // }
        } catch (e) {
            $scope.btnDisable = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion

    // #region SaveExperience

    $scope.SaveExperience = function () {
        try {
            $scope.btnDisable = true;
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: 'employees/prerecruitmentemployee/createexperience',
                    data: $scope.preRecruitmentEmpExperience,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.btnDisable = false;
                        ShowResult(response.data.Message, "failure");
                    }
                    else {
                        ShowResult(response.data.Message, "success");
                        $scope.btnDisable = false;
                        $scope.LoadExperienceData();
                        $scope.Clear();
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
                angular.element(document.querySelector('#ExperiencePopUp')).modal('hide');
                return true;
            }
            // }
        } catch (e) {
            $scope.btnDisable = false;
            ShowResult(e, "failure");
        }
    };

    // #endregion

    $scope.LoadQualificationData = function () {
        $http.get('employees/prerecruitmentemployee/getqualificationdata')
            .then(function (response) {
                $scope.preRecruitmentEmpQualifications = response.data;
            });
    };
    $scope.LoadQualificationData();

    $scope.LoadExperienceData = function () {
        $http.get('employees/prerecruitmentemployee/getexperiencedata')
            .then(function (response) {
                $scope.preRecruitmentEmpExperiences = response.data;
            });
    };
    $scope.LoadExperienceData();

    $scope.LoadTrainingData = function () {
        $http.get('employees/prerecruitmentemployee/gettrainingdata')
            .then(function (response) {
                $scope.preRecruitmentEmpTrainings = response.data;
            });
    };
    $scope.LoadTrainingData();

    $scope.qualificationShow = function () {
        angular.element(document.querySelector('#QualificationPopUp')).modal('show');
    };

    $scope.indexQua = -1;
    $scope.QualificationData = function (id, index) {
        $scope.indexQua = index;
        var obj = $scope.preRecruitmentEmpQualifications[$scope.indexQua];
        for (var i in $scope.preRecruitmentEmpQualification) {
            $scope.preRecruitmentEmpQualification[i] = obj[i];
        }
        // $scope.Action = 'Update';
    };

    $scope.indexTrn = -1;
    $scope.TrainingData = function (id, index) {
        $scope.indexTrn = index;
        var obj = $scope.preRecruitmentEmpTrainings[$scope.indexTrn];
        for (var i in $scope.preRecruitmentEmpTraining) {
            $scope.preRecruitmentEmpTraining[i] = obj[i];
        }
        // $scope.Action = 'Update';
    };

    $scope.indexExp = -1;
    $scope.ExperienceData = function (id, index) {
        $scope.indexExp = index;
        var obj = $scope.preRecruitmentEmpExperiences[$scope.indexExp];
        for (var i in $scope.preRecruitmentEmpExperience) {
            $scope.preRecruitmentEmpExperience[i] = obj[i];
        }
        // $scope.Action = 'Update';
    };

    $scope.TrainingShow = function () {
        angular.element(document.querySelector('#TrainingPopUp')).modal('show');
    };

    $scope.ExperienceShow = function () {
        angular.element(document.querySelector('#ExperiencePopUp')).modal('show');
    };

    $scope.confirmQualificationDelete = function (Id) {
        $scope.deleteQualificationId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "] ";
    };

    $scope.DeleteQualification = function () {
        $http({
            method: 'POST',
            url: 'employees/prerecruitmentemployee/deletequalification',
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

    $scope.confirmExperienceDelete = function (Id) {
        $scope.deleteExperienceId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "] ";
    };

    $scope.DeleteExperience = function () {
        $http({
            method: 'POST',
            url: 'employees/prerecruitmentemployee/deleteexperience',
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

    $scope.confirmTrainingDelete = function (Id) {
        $scope.deleteTrainingId = Id;
        $scope.message_confirmation = "Are you sure to delete [" + Id + "] ";
    };

    $scope.DeleteTraining = function () {
        $http({
            method: 'POST',
            url: 'employees/prerecruitmentemployee/deletetraining',
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

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.preRecruitmentEmpQualification = {};
        $scope.preRecruitmentEmpTraining = {};
        $scope.preRecruitmentEmpExperience = {};

        $scope.preRecruitmentEmpQualification = {
            SystemID: null,
            EmpSystemID: null,
            EductLevelSystemID: null,
            StreamId: null,
            CountryId: null,
            HasDistinction: true,
            Session: null,
            IsGeneral: true,
            IsEnglishMedium: true,
            IsMadrasah: true,
            IsVocational: true,
            IsOther: true,
            OtherEductType: null,
            ExamDegreeType: null,
            ConcMajor: null,
            InstituteName: null,
            IsForeignInstitute: true,
            ResultSystemID: null,
            Marks: null,
            CGPA: null,
            Scale: null,
            YearOfPass: null,
            Duration: null,
            Achievement: null
        };

        $scope.preRecruitmentEmpTraining = {
            SystemID: null,
            EmpSystemID: null,
            TrainingTitle: null,
            TopicCovered: null,
            IntituteName: null,
            CountrySystemID: null,
            Location: null,
            TrainingYear: null,
            Duration: null
        };

        $scope.preRecruitmentEmpExperience = {
            SystemID: null,
            EmpSystemID: null,
            Employer: null,
            Designation: null,
            StartDate: null,
            EndDate: null,
            JobDescription: null,
            Achievement: null
        };
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.clearImage = function () {
        $scope.imageSrc = '';
        document.getElementById("uploadImage").value = '';
        document.getElementById("uploadImageSrc").setAttribute('src', null);
    };
}