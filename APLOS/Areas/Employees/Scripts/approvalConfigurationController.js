'use strict';
approvalConfigurationController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function approvalConfigurationController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Approval Configuration';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.approvalconfigurations = [];
    $scope.path = 'employees/approvalconfiguration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, 'Entity', 'Entity');

    $scope.getData = function (pageno) {
        $scope.approvalConfigurationNew.Id = null;
        $scope.approvalConfigurationNew.OrgDocRP = null;
        $scope.approvalConfigurationNew.OrgDocRPEC = null;
        $scope.approvalConfigurationNew.DocumentResponsible = null;
        $scope.approvalConfigurationNew.ProfileUploadRP = null;
        $scope.approvalConfigurationNew.ProfileUploadRPEC = null;
        $scope.approvalConfigurationNew.ProfileUploadRPerson = null;
        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRP = null;
        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceApprovedByPerson = null;
        $scope.approvalConfigurationNew.ResignationApproval = null;
        $scope.approvalConfigurationNew.ResignationApprovalEC = null;
        $scope.approvalConfigurationNew.Resignationperson = null;
        $scope.approvalConfigurationNew.SalaryRP = null;
        $scope.approvalConfigurationNew.SalaryResponsible = null;
        $scope.approvalConfigurationNew.ProbationRP = null;
        $scope.approvalConfigurationNew.ProbationRPEC = null;
        $scope.approvalConfigurationNew.ProbationResponsible = null;
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRP = null;
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPerson = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocRP = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocRPEC = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocRPerson = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocRP = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocRPEC = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocRPerson = null;
        $scope.approvalConfigurationNew.ResigRecruitPlanningRP = null;
        $scope.approvalConfigurationNew.ResigRecruitPlanningRPerson = null;
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRP = null;
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPerson = null;
        $scope.approvalConfigurationNew.ResignationApply = null;
        $scope.approvalConfigurationNew.ResignationApplyEC = null;
        $scope.approvalConfigurationNew.ResignationApplyPerson = null;
        $scope.approvalConfigurationNew.LeaveApproval = null;
        $scope.approvalConfigurationNew.LeaveApprovalPerson = null;
        $scope.approvalConfigurationNew.ProductionPlanning = null;
        $scope.approvalConfigurationNew.ProductionPlanningEC = null;
        $scope.approvalConfigurationNew.ProductionPlanningPerson = null;

        $scope.approvalConfigurationNew.UpperEmployeeStatus = null;
        $scope.approvalConfigurationNew.DocumentEmployeeStatus = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocEmployeeStatus = null;
        $scope.approvalConfigurationNew.RecruitmentFinalEmployeeStatus = null;
        $scope.approvalConfigurationNew.SalaryEmployeeStatus = null;
        $scope.approvalConfigurationNew.ProbationEmployeeStatus = null;
        $scope.approvalConfigurationNew.ResignationEmployeeStatus = null;
        $scope.approvalConfigurationNew.ProfileEmployeeStatus = null;
        $scope.approvalConfigurationNew.ResigRecruitEmployeeStatus = null;
        $scope.approvalConfigurationNew.OrgDocEmployeeStatus = null;
        $scope.approvalConfigurationNew.ResignationApplyEmployeeStatus = null;
        $scope.approvalConfigurationNew.LeaveEmployeeStatus = null;
        $scope.approvalConfigurationNew.ProductionEmployeeStatus = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocEmployeeStatus = null;

        $scope.approvalConfigurationNew.SalaryAdvanceApproval = null;
        $scope.approvalConfigurationNew.SalaryAdvanceApprovalPerson = null;
        $scope.approvalConfigurationNew.SalaryAdvanceApprovalStatus = null;

        $scope.approvalConfigurationNew.SalaryFixationApproval = null;
        $scope.approvalConfigurationNew.SalaryFixationApprovalEC = null;
        $scope.approvalConfigurationNew.SalaryFixationApprovalPerson = null;
        $scope.approvalConfigurationNew.SalaryFixationApprovalStatus = null;

        $scope.approvalConfigurationNew.ManualAttendanceApproval = null;
        $scope.approvalConfigurationNew.ManualAttendanceApprovalEC = null;
        $scope.approvalConfigurationNew.ManualAttendanceApprovalPerson = null;
        $scope.approvalConfigurationNew.ManualAttendanceApprovalStatus = null;


        $scope.approvalConfigurationNew.ExpanseBookingRP = null;
        $scope.approvalConfigurationNew.ExpanseBooking = null;
        $scope.approvalConfigurationNew.ExpanseBookingStatus = null;

        $scope.approvalConfigurationNew.InOutAttendanceApproval = null;
        $scope.approvalConfigurationNew.InOutAttendanceApprovalEC = null;
        $scope.approvalConfigurationNew.InOutAttendance = null;
        $scope.approvalConfigurationNew.InOutAttendanceStatus = null;
        $scope.approvalConfigurationNew.InOutAttendancePerson = null;

        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRPEC = null;
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPEC = null;
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPEC = null;
        $scope.approvalConfigurationNew.SalaryRPEC = null;
        $scope.approvalConfigurationNew.ResigRecruitPlanningRPEC = null;
        $scope.approvalConfigurationNew.LeaveApprovalEC = null;
        $scope.approvalConfigurationNew.SalaryAdvanceApprovalEC = null;
        $scope.approvalConfigurationNew.ExpanseBookingRPEC = null;
        $scope.approvalConfigurationNew.InOutAttendanceEC = null;


        $rootScope.parameters.plantId = $scope.approvalConfigurationNew.PlantId;
        $rootScope.parameters.entityId = $scope.approvalConfigurationNew.EntityId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.approvalconfigurations = result.Rows;
                if ($scope.approvalconfigurations.length > 0) {
                    $scope.approvalConfigurationNew.Id = $scope.approvalconfigurations[0].Id;
                    $scope.approvalConfigurationNew.OrgDocRP = $scope.approvalconfigurations[0].OrgDocRP;
                    $scope.approvalConfigurationNew.OrgDocRPEC = $scope.approvalconfigurations[0].OrgDocRPEC;
                    $scope.approvalConfigurationNew.DocumentResponsible = $scope.approvalconfigurations[0].DocumentResponsible;
                    $scope.approvalConfigurationNew.ProfileUploadRP = $scope.approvalconfigurations[0].ProfileUploadRP;
                    $scope.approvalConfigurationNew.ProfileUploadRPEC = $scope.approvalconfigurations[0].ProfileUploadRPEC;
                    $scope.approvalConfigurationNew.ProfileUploadRPerson = $scope.approvalconfigurations[0].ProfileUploadRPerson;
                    $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRP = $scope.approvalconfigurations[0].UpperDesignationAndSpecialAllowanceRP;
                    $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceApprovedByPerson = $scope.approvalconfigurations[0].UpperDesignationAndSpecialAllowanceApprovedByPerson;
                    $scope.approvalConfigurationNew.ResignationApproval = $scope.approvalconfigurations[0].ResignationApproval;
                    $scope.approvalConfigurationNew.ResignationApprovalEC = $scope.approvalconfigurations[0].ResignationApprovalEC;
                    $scope.approvalConfigurationNew.Resignationperson = $scope.approvalconfigurations[0].Resignationperson;
                    $scope.approvalConfigurationNew.SalaryRP = $scope.approvalconfigurations[0].SalaryRP;
                    $scope.approvalConfigurationNew.SalaryRPEC = $scope.approvalconfigurations[0].SalaryRPEC;
                    $scope.approvalConfigurationNew.SalaryResponsible = $scope.approvalconfigurations[0].SalaryResponsible;
                    $scope.approvalConfigurationNew.ProbationRP = $scope.approvalconfigurations[0].ProbationRP;
                    $scope.approvalConfigurationNew.ProbationRPEC = $scope.approvalconfigurations[0].ProbationRPEC;
                    $scope.approvalConfigurationNew.ProbationResponsible = $scope.approvalconfigurations[0].ProbationResponsible;
                    $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRP = $scope.approvalconfigurations[0].RecruitmentFinalConfirmationRP;
                    $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPEC = $scope.approvalconfigurations[0].RecruitmentFinalConfirmationRPEC;
                    $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPerson = $scope.approvalconfigurations[0].RecruitmentFinalConfirmationRPerson;
                    $scope.approvalConfigurationNew.PreRecruitmentDocRP = $scope.approvalconfigurations[0].PreRecruitmentDocRP;
                    $scope.approvalConfigurationNew.PreRecruitmentDocRPEC = $scope.approvalconfigurations[0].PreRecruitmentDocRPEC;
                    $scope.approvalConfigurationNew.PreRecruitmentDocRPerson = $scope.approvalconfigurations[0].PreRecruitmentDocRPerson;
                    $scope.approvalConfigurationNew.PostRecruitmentDocRP = $scope.approvalconfigurations[0].PostRecruitmentDocRP;
                    $scope.approvalConfigurationNew.PostRecruitmentDocRPEC = $scope.approvalconfigurations[0].PostRecruitmentDocRPEC;
                    $scope.approvalConfigurationNew.PostRecruitmentDocRPerson = $scope.approvalconfigurations[0].PostRecruitmentDocRPerson;
                    $scope.approvalConfigurationNew.ResigRecruitPlanningRP = $scope.approvalconfigurations[0].ResigRecruitPlanningRP;
                    $scope.approvalConfigurationNew.ResigRecruitPlanningRPerson = $scope.approvalconfigurations[0].ResigRecruitPlanningRPerson;
                    $scope.approvalConfigurationNew.PostRecruitmentOrgDocRP = $scope.approvalconfigurations[0].PostRecruitmentOrgDocRP;
                    $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPEC = $scope.approvalconfigurations[0].PostRecruitmentOrgDocRPEC;
                    $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPerson = $scope.approvalconfigurations[0].PostRecruitmentOrgDocRPerson;
                    $scope.approvalConfigurationNew.ResignationApply = $scope.approvalconfigurations[0].ResignationApply;
                    $scope.approvalConfigurationNew.ResignationApplyEC = $scope.approvalconfigurations[0].ResignationApplyEC;
                    $scope.approvalConfigurationNew.ResignationApplyPerson = $scope.approvalconfigurations[0].ResignationApplyPerson;
                    $scope.approvalConfigurationNew.LeaveApproval = $scope.approvalconfigurations[0].LeaveApproval;
                    $scope.approvalConfigurationNew.LeaveApprovalEC = $scope.approvalconfigurations[0].LeaveApprovalEC;
                    $scope.approvalConfigurationNew.LeaveApprovalPerson = $scope.approvalconfigurations[0].LeaveApprovalPerson;
                    $scope.approvalConfigurationNew.ProductionPlanning = $scope.approvalconfigurations[0].ProductionPlanning;
                    $scope.approvalConfigurationNew.ProductionPlanningEC = $scope.approvalconfigurations[0].ProductionPlanningEC;
                    $scope.approvalConfigurationNew.ProductionPlanningPerson = $scope.approvalconfigurations[0].ProductionPlanningPerson;

                    $scope.approvalConfigurationNew.UpperEmployeeStatus = $scope.approvalconfigurations[0].UpperEmployeeStatus;
                    $scope.approvalConfigurationNew.DocumentEmployeeStatus = $scope.approvalconfigurations[0].DocumentEmployeeStatus;
                    $scope.approvalConfigurationNew.PreRecruitmentDocEmployeeStatus = $scope.approvalconfigurations[0].PreRecruitmentDocEmployeeStatus;
                    $scope.approvalConfigurationNew.RecruitmentFinalEmployeeStatus = $scope.approvalconfigurations[0].RecruitmentFinalEmployeeStatus;
                    $scope.approvalConfigurationNew.SalaryEmployeeStatus = $scope.approvalconfigurations[0].SalaryEmployeeStatus;
                    $scope.approvalConfigurationNew.ProbationEmployeeStatus = $scope.approvalconfigurations[0].ProbationEmployeeStatus;
                    $scope.approvalConfigurationNew.ResignationEmployeeStatus = $scope.approvalconfigurations[0].ResignationEmployeeStatus;
                    $scope.approvalConfigurationNew.ProfileEmployeeStatus = $scope.approvalconfigurations[0].ProfileEmployeeStatus;
                    $scope.approvalConfigurationNew.ResigRecruitEmployeeStatus = $scope.approvalconfigurations[0].ResigRecruitEmployeeStatus;
                    $scope.approvalConfigurationNew.OrgDocEmployeeStatus = $scope.approvalconfigurations[0].OrgDocEmployeeStatus;
                    $scope.approvalConfigurationNew.ResignationApplyEmployeeStatus = $scope.approvalconfigurations[0].ResignationApplyEmployeeStatus;
                    $scope.approvalConfigurationNew.LeaveEmployeeStatus = $scope.approvalconfigurations[0].LeaveEmployeeStatus;
                    $scope.approvalConfigurationNew.ProductionEmployeeStatus = $scope.approvalconfigurations[0].ProductionEmployeeStatus;
                    $scope.approvalConfigurationNew.PostRecruitmentDocEmployeeStatus = $scope.approvalconfigurations[0].PostRecruitmentDocEmployeeStatus;

                    $scope.approvalConfigurationNew.SalaryAdvanceApproval = $scope.approvalconfigurations[0].SalaryAdvanceApproval;
                    $scope.approvalConfigurationNew.SalaryAdvanceApprovalEC = $scope.approvalconfigurations[0].SalaryAdvanceApprovalEC;
                    $scope.approvalConfigurationNew.SalaryAdvanceApprovalPerson = $scope.approvalconfigurations[0].SalaryAdvanceApprovalPerson;
                    $scope.approvalConfigurationNew.SalaryAdvanceApprovalStatus = $scope.approvalconfigurations[0].SalaryAdvanceApprovalStatus;

                    $scope.approvalConfigurationNew.ExpanseBookingRP = $scope.approvalconfigurations[0].ExpanseBookingRP;
                    $scope.approvalConfigurationNew.ExpanseBooking = $scope.approvalconfigurations[0].ExpanseBooking;
                    $scope.approvalConfigurationNew.ExpanseBookingEC = $scope.approvalconfigurations[0].ExpanseBookingEC;
                    $scope.approvalConfigurationNew.ExpanseBookingStatus = $scope.approvalconfigurations[0].ExpanseBookingStatus;

                    $scope.approvalConfigurationNew.SalaryFixationApproval = $scope.approvalconfigurations[0].SalaryFixationApproval;
                    $scope.approvalConfigurationNew.SalaryFixationApprovalEC = $scope.approvalconfigurations[0].SalaryFixationApprovalEC;
                    $scope.approvalConfigurationNew.SalaryFixationApprovalPerson = $scope.approvalconfigurations[0].SalaryFixationApprovalPerson;
                    $scope.approvalConfigurationNew.SalaryFixationApprovalStatus = $scope.approvalconfigurations[0].SalaryFixationApprovalStatus;

                    $scope.approvalConfigurationNew.ManualAttendanceApproval = $scope.approvalconfigurations[0].ManualAttendanceApproval;
                    $scope.approvalConfigurationNew.ManualAttendanceApprovalEC = $scope.approvalconfigurations[0].ManualAttendanceApprovalEC;
                    $scope.approvalConfigurationNew.ManualAttendanceApprovalPerson = $scope.approvalconfigurations[0].ManualAttendanceApprovalPerson;
                    $scope.approvalConfigurationNew.ManualAttendanceApprovalStatus = $scope.approvalconfigurations[0].ManualAttendanceApprovalStatus;

                    $scope.approvalConfigurationNew.InOutAttendance = $scope.approvalconfigurations[0].InOutAttendance;
                    $scope.approvalConfigurationNew.InOutAttendanceEC = $scope.approvalconfigurations[0].InOutAttendanceEC;
                    $scope.approvalConfigurationNew.InOutAttendancePerson = $scope.approvalconfigurations[0].InOutAttendancePerson;
                    $scope.approvalConfigurationNew.InOutAttendanceStatus = $scope.approvalconfigurations[0].InOutAttendanceStatus;
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $rootScope.searchByList = [
        {
            'name': 'Plant',
            'value': 'Plant'
        },
        {
            'name': 'Entity',
            'value': 'Entity'
        },
        {
            'name': 'Document',
            'value': 'DocumentApprovedByPerson'
        },
        {
            'name': 'Upper Designation & Special Allowance',
            'value': 'UpperDesignationAndSpecialAllowanceApprovedByPerson'
        }
    ];

    $scope.approvalConfigurationNew = {
        Id: null,
        CompanyId: null,
        PlantId: null,
        EntityId: null,
        UpperDesignationAndSpecialAllowanceRP: null,
        OrgDocRP: null,
        PreRecruitmentDocRP: null,
        PostRecruitmentDocRP: null,
        RecruitmentFinalConfirmationRP: null,
        SalaryRP: null,
        ProbationRP: null,
        ResignationApproval: null,
        ProfileUploadRP: null,
        UpperDesignationAndSpecialAllowanceApprovedByPerson: null,
        ResignationResponsiblePerson: null,
        SalaryResponsiblePerson: null,
        ProbationResponsiblePerson: null,
        RecruitmentResponsiblePerson: null,
        DocumentResponsiblePerson: null,
        PreRecruitmentDocRPerson: null,
        PostRecruitmentDocRPerson: null,
        RecruitmentFinalConfirmationRPerson: null,
        ProfileUploadRPerson: null,
        ResigRecruitPlanningRP: null,
        ResigRecruitPlanningRPerson: null,
        PostRecruitmentOrgDocRP: null,
        PostRecruitmentOrgDocRPerson: null,
        ResignationApplyPerson: null,
        ResignationApply: null,
        LeaveApproval: null,
        LeaveApprovalPerson: null,
        ProductionPlanning: null,
        ProductionPlanningPerson: null,
        UpperEmployeeStatus: null,
        DocumentEmployeeStatus: null,
        PreRecruitmentDocEmployeeStatus: null,
        RecruitmentFinalEmployeeStatus: null,
        SalaryEmployeeStatus: null,
        ProbationEmployeeStatus: null,
        ResignationEmployeeStatus: null,
        ProfileEmployeeStatus: null,
        ResigRecruitEmployeeStatus: null,
        OrgDocEmployeeStatus: null,
        ResignationApplyEmployeeStatus: null,
        LeaveEmployeeStatus: null,
        ProductionEmployeeStatus: null,
        ManualAttendanceApproval: null,
        ManualAttendanceApprovalPerson: null,
        ManualAttendanceApprovalStatus: null,

        InOutAttendance: null,
        InOutAttendancePerson: null,
        InOutAttendanceStatus: null
    };

    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.getPlant = function () {
        cboService.getCboPlantByCompany($scope.approvalConfigurationNew.CompanyId, function (result) {
            $scope.PlantList = result;
        });
    };
    $scope.getEntityWithChange = function () {
        cboService.getCboEntityByPlant(null, $scope.approvalConfigurationNew.CompanyId, $scope.approvalConfigurationNew.PlantId, function (result) {
            $scope.EntityList = result;
        });
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.approvalConfigurationForm.$valid) {
            if (baseService.isUndefinedOrNull($scope.approvalConfigurationNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.approvalConfigurationNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.approvalConfigurationNew = response.data.ApprovalConfiguration;
                        $scope.approvalconfigurations.push($scope.approvalConfigurationNew);
                        $scope.getData();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
            else if (!baseService.isUndefinedOrNull($scope.approvalConfigurationNew.Id)) {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.approvalConfigurationNew,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.approvalconfigurations[$scope.index] = $scope.approvalConfigurationNew;
                            $scope.approvalConfigurations = $filter('orderBy')($scope.approvalConfigurations, 'PlantId');
                        }
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    };

    // #region  Dynamic PopUp
    $scope.popUpList = [];

    $scope.popUp = function (name) {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: '',
            searchBy: '',
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        try {
            if (baseService.isUndefinedOrNull($scope.approvalConfigurationNew.PlantId)) {
                throw "First select plant.";
            }
            $scope.popUpUrl = '';
            $scope.popUpParameters.sort = '';
            $scope.popUpParameters.searchBy = '';

            $scope.popUpUrl = 'employees/approvalconfiguration/getallemployeedata';
            $scope.popUpParameters.sort = 'EmployeeCodeNumeric';
            $scope.popUpParameters.searchBy = 'EmployeeCode';

            if (name === 'ProfileUpload') {
                $scope.popUpTitle = 'Profile Upload';
            }
            else if (name === 'ResignationResponsiblePerson') {
                $scope.popUpTitle = 'Resignation';
            }
            else if (name === 'UpperDesignation') {
                $scope.popUpTitle = 'Upper Designation & Special Allowance';
            }
            else if (name === 'SalaryResponsiblePerson') {
                $scope.popUpTitle = 'Salary';
            }
            else if (name === 'ProbationResponsiblePerson') {
                $scope.popUpTitle = 'Probation';
            }
            else if (name === 'RecruitmentFinalConfirmationRP') {
                $scope.popUpTitle = 'Recruitment Final Confirmation';
            }
            else if (name === 'DocumentResponsiblePerson') {
                $scope.popUpTitle = 'Document Responsible Person';
            }
            else if (name === 'PreRecruitmentDocRP') {
                $scope.popUpTitle = 'PreRecruitment Document Approval Person';
            }
            else if (name === 'PostRecruitmentDocRP') {
                $scope.popUpTitle = 'PostRecruitment Document Approval Person';
            }
            else if (name === 'ResigRecruitPlanningRPerson') {
                $scope.popUpTitle = 'Resignation Recruitment Planning';
            }
            else if (name === 'PostRecruitmentOrganization') {
                $scope.popUpTitle = 'Post Recruitment Organization Document';
            }
            else if (name === 'ResignationApply') {
                $scope.popUpTitle = 'Resignation Apply';
            }
            else if (name === 'LeaveApproval') {
                $scope.popUpTitle = 'Leave Approval';
            }
            else if (name === 'ProductionPlanning') {
                $scope.popUpTitle = 'Production Planning';
            }
            else if (name === 'SalaryAdvanceApproval') {
                $scope.popUpTitle = 'Salary Advance Approval';
            }
            else if (name === 'SalaryFixationApproval') {
                $scope.popUpTitle = 'Salary Fixation Approval';
            }
            else if (name === 'ManualAttendanceApproval') {
                $scope.popUpTitle = 'Manual Attendance Approval';
            }
            else if (name === 'InOutAttendanceApproval') {
                $scope.popUpTitle = 'InOut Attendance Approval';
            }
            else {
                $scope.popUpTitle = 'Expanse Booking';
            }

            $scope.popUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.popUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };

            $scope.fieldName = name;
            angular.element(document.querySelector('#popUp')).modal('show');
            $scope.popUpData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectdblClick = function (data) {
        setPartyName(data);
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    function setPartyName(ob) {
        if ($scope.fieldName === 'ProfileUpload') {
            $scope.approvalConfigurationNew.ProfileUploadRP = ob.SystemId;
            $scope.approvalConfigurationNew.ProfileUploadRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ProfileUploadRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ResignationResponsiblePerson') {
            $scope.approvalConfigurationNew.ResignationApproval = ob.SystemId;
            $scope.approvalConfigurationNew.ResignationApprovalEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.Resignationperson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'UpperDesignation') {
            $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRP = ob.SystemId;
            $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceApprovedByPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'SalaryResponsiblePerson') {
            $scope.approvalConfigurationNew.SalaryRP = ob.SystemId;
            $scope.approvalConfigurationNew.SalaryRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.SalaryResponsible = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ProbationResponsiblePerson') {
            $scope.approvalConfigurationNew.ProbationRP = ob.SystemId;
            $scope.approvalConfigurationNew.ProbationRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ProbationResponsible = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'RecruitmentFinalConfirmationRP') {
            $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRP = ob.SystemId;
            $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'DocumentResponsiblePerson') {
            $scope.approvalConfigurationNew.OrgDocRP = ob.SystemId;
            $scope.approvalConfigurationNew.OrgDocRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.DocumentResponsible = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'PreRecruitmentDocRP') {
            $scope.approvalConfigurationNew.PreRecruitmentDocRP = ob.SystemId;
            $scope.approvalConfigurationNew.PreRecruitmentDocRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.PreRecruitmentDocRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'PostRecruitmentDocRP') {
            $scope.approvalConfigurationNew.PostRecruitmentDocRP = ob.SystemId;
            $scope.approvalConfigurationNew.PostRecruitmentDocRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.PostRecruitmentDocRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'PostRecruitmentOrganization') {
            $scope.approvalConfigurationNew.PostRecruitmentOrgDocRP = ob.SystemId;
            $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ResigRecruitPlanningRPerson') {
            $scope.approvalConfigurationNew.ResigRecruitPlanningRP = ob.SystemId;
            $scope.approvalConfigurationNew.ResigRecruitPlanningRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ResigRecruitPlanningRPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ResignationApply') {
            $scope.approvalConfigurationNew.ResignationApply = ob.SystemId;
            $scope.approvalConfigurationNew.ResignationApplyEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ResignationApplyPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'LeaveApproval') {
            $scope.approvalConfigurationNew.LeaveApproval = ob.SystemId;
            $scope.approvalConfigurationNew.LeaveApprovalEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.LeaveApprovalPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ProductionPlanning') {
            $scope.approvalConfigurationNew.ProductionPlanning = ob.SystemId;
            $scope.approvalConfigurationNew.ProductionPlanningEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ProductionPlanningPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'SalaryAdvanceApproval') {
            $scope.approvalConfigurationNew.SalaryAdvanceApproval = ob.SystemId;
            $scope.approvalConfigurationNew.SalaryAdvanceApprovalEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.SalaryAdvanceApprovalPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'SalaryFixationApproval') {
            $scope.approvalConfigurationNew.SalaryFixationApproval = ob.SystemId;
            $scope.approvalConfigurationNew.SalaryFixationApprovalEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.SalaryFixationApprovalPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ManualAttendanceApproval') {
            $scope.approvalConfigurationNew.ManualAttendanceApproval = ob.SystemId;
            $scope.approvalConfigurationNew.ManualAttendanceApprovalEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ManualAttendanceApprovalPerson = ob.EmployeeName;
        }
        else if ($scope.fieldName === 'ExpanseBooking') {
            $scope.approvalConfigurationNew.ExpanseBookingRP = ob.SystemId;
            $scope.approvalConfigurationNew.ExpanseBookingRPEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.ExpanseBooking = ob.EmployeeName;
        }

        else if ($scope.fieldName === 'InOutAttendanceApproval') {
            $scope.approvalConfigurationNew.InOutAttendance = ob.SystemId;
            $scope.approvalConfigurationNew.InOutAttendanceEC = ob.EmployeeCode;
            $scope.approvalConfigurationNew.InOutAttendancePerson = ob.EmployeeName;
        }
    }
    $scope.valueData = '';
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    };
    $scope.SelectByButton = function () {
        if ($scope.valueData === '') {
            alert('Please at first select row');
            return;
        }
        $scope.selectdblClick($scope.valueData);
        $scope.valueData = '';
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };
    // #endregion

    $scope.clearProfileUpload = function () {
        $scope.approvalConfigurationNew.ProfileUploadRP = null;
        $scope.approvalConfigurationNew.ProfileUploadRPEC = null;
        $scope.approvalConfigurationNew.ProfileUploadRPerson = null;
    };
    $scope.clearResignation = function () {
        $scope.approvalConfigurationNew.ResignationApproval = null;
        $scope.approvalConfigurationNew.ResignationApprovalEC = null;
        $scope.approvalConfigurationNew.Resignationperson = null;
    };
    $scope.clearProbation = function () {
        $scope.approvalConfigurationNew.ProbationRP = null;
        $scope.approvalConfigurationNew.ProbationRPEC = null;
        $scope.approvalConfigurationNew.ProbationResponsible = null;
    };
    $scope.clearRecruitmentFinalConfirmation = function () {
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRP = null;
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPEC = null;
        $scope.approvalConfigurationNew.RecruitmentFinalConfirmationRPerson = null;
    };
    $scope.clearDesignation = function () {
        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRP = null;
        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceRPEC = null;
        $scope.approvalConfigurationNew.UpperDesignationAndSpecialAllowanceApprovedByPerson = null;
    };
    $scope.clearPreRecruitmentDocRP = function () {
        $scope.approvalConfigurationNew.PreRecruitmentDocRP = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocRPEC = null;
        $scope.approvalConfigurationNew.PreRecruitmentDocRPerson = null;
    };
    $scope.clearPostRecruitmentDocRP = function () {
        $scope.approvalConfigurationNew.PostRecruitmentDocRP = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocRPEC = null;
        $scope.approvalConfigurationNew.PostRecruitmentDocRPerson = null;
    };
    $scope.clearDocument = function () {
        $scope.approvalConfigurationNew.OrgDocRP = null;
        $scope.approvalConfigurationNew.OrgDocRPEC = null;
        $scope.approvalConfigurationNew.DocumentResponsible = null;
    };
    $scope.clearSalary = function () {
        $scope.approvalConfigurationNew.SalaryRP = null;
        $scope.approvalConfigurationNew.SalaryRPEC = null;
        $scope.approvalConfigurationNew.SalaryResponsible = null;
    };
    $scope.clearResigRecruitPlanningRP = function () {
        $scope.approvalConfigurationNew.ResigRecruitPlanningRP = null;
        $scope.approvalConfigurationNew.ResigRecruitPlanningRPEC = null;
        $scope.approvalConfigurationNew.ResigRecruitPlanningRPerson = null;
    };
    $scope.clearPostRecruitmentOrganization = function () {
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRP = null;
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPEC = null;
        $scope.approvalConfigurationNew.PostRecruitmentOrgDocRPerson = null;
    };
    $scope.clearResignationApply = function () {
        $scope.approvalConfigurationNew.ResignationApply = null;
        $scope.approvalConfigurationNew.ResignationApplyEC = null;
        $scope.approvalConfigurationNew.ResignationApplyPerson = null;
    };
    $scope.clearLeaveApproval = function () {
        $scope.approvalConfigurationNew.LeaveApproval = null;
        $scope.approvalConfigurationNew.LeaveApprovalEC = null;
        $scope.approvalConfigurationNew.LeaveApprovalPerson = null;
    };
    $scope.clearProductionPlanning = function () {
        $scope.approvalConfigurationNew.ProductionPlanning = null;
        $scope.approvalConfigurationNew.ProductionPlanningEC = null;
        $scope.approvalConfigurationNew.ProductionPlanningPerson = null;
    };
    $scope.clearSalaryAdvanceApproval = function () {
        $scope.approvalConfigurationNew.SalaryAdvanceApproval = null;
        $scope.approvalConfigurationNew.SalaryAdvanceApprovalEC = null;
        $scope.approvalConfigurationNew.SalaryAdvanceApprovalPerson = null;
    };
    $scope.clearSalaryFixationApproval = function () {
        $scope.approvalConfigurationNew.SalaryFixationApproval = null;
        $scope.approvalConfigurationNew.SalaryFixationApprovalEC = null;
        $scope.approvalConfigurationNew.SalaryFixationApprovalPerson = null;
    };
    $scope.clearManualAttendanceApproval = function () {
        $scope.approvalConfigurationNew.ManualAttendanceApproval = null;
        $scope.approvalConfigurationNew.ManualAttendanceApprovalEC = null;
        $scope.approvalConfigurationNew.ManualAttendanceApprovalPerson = null;
    };
    $scope.clearExpanseBooking = function () {
        $scope.approvalConfigurationNew.ExpanseBookingRP = null;
        $scope.approvalConfigurationNew.ExpanseBookingRPEC = null;
        $scope.approvalConfigurationNew.ExpanseBooking = null;
    };
    $scope.clearInOutAttendanceApproval = function () {
        $scope.approvalConfigurationNew.InOutAttendance = null;
        $scope.approvalConfigurationNew.InOutAttendanceEC = null;
        $scope.approvalConfigurationNew.InOutAttendancePerson = null;
    };
}