'use strict';
EmployeeUnderstandingHeadController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EmployeeUnderstandingHeadController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'EmployeeUnderstandingHead';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/EmployeeUnderstandingHead/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveActivityUrl = $scope.path + 'SaveActivity';
    $scope.saveChildUrl = $scope.path + 'CreateChild';
    
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ModelList = response.data;
        });
    }
    $scope.getData();
    $scope.ActivityList = [];
    $scope.getActivityGridData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetActivityList",
            data: { 'EmpUnderstandingHeadId': $scope.ModelNew.Id  },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            $scope.ActivityList = response.data;
        });
    }


    $scope.StatusList = [{ value: 'DefaltInProgress', name: 'Defalt In-Progress' },
        { value: 'Confirm', name: 'Confirm' },
        { value: 'Approved', name: 'Approved' }]

    $scope.ActivityClassList = [{ value: 'Prime', name: 'Prime' },
        { value: 'Secondery', name: 'Secondery' },
        { value: 'Other', name: 'Other' }]

    $scope.ActivityCategoryList = [{ value: 'Planning', name: 'Planning' },
        { value: 'FollowUp', name: 'Follow-Up' },
        { value: 'Decision', name: 'Decision' },
        { value: 'Execution', name: 'Execution' },
        { value: 'Review', name: 'Review' },
        { value: 'Other', name: 'Other (please Specify)' }     ]

    $scope.PriorityList = [{ value: 'Top5', name: 'Top 5' },
        { value: 'Top10', name: 'Top 10' },
        { value: 'Other', name: 'Other' }]

    $scope.ActivityTypeList = [{ value: 'ValueAdded', name: 'Value Added' },
        { value: 'NonValueAddedNecessary', name: 'Non-Value Added (Necessary)' },
        { value: 'NonValueAddedUnnecessary', name: 'Non-Value Added (Unnecessary)' }]

    $scope.ActivityImportanceList = [{ value: 'Normal', name: 'Normal' },
        { value: 'High', name: 'High' },
        { value: 'Medium', name: 'Medium' },
        { value: 'Critical', name: 'Critical' }]

    $scope.PeriodList = [{ value: 'Daily', name: 'Daily' },
        { value: 'Weekly', name: 'Weekly' },
        { value: 'Fortnight', name: 'Fortnight' },
        { value: 'Monthly', name: 'Monthly' },
        { value: 'Quarterly', name: 'Quarterly' },
        { value: 'HalfYearly', name: 'Half Yearly' },
        { value: 'Annually', name: 'Annually' }]

    $scope.FinancialImpactList = [{ value: 'Yes', name: 'Yes' },
        { value: 'No', name: 'No' }]

    
    
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ModelTemp = {
        Id: null,
        Date: null,
        PositionCode: null,
        BudgetCode: null,
        EmployeeCode: null,
        EmployeeName: null,
        EmployeeId: null,
        Status: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.activity = {
        Id: null,
        EmpUnderstandingHeadId: null,
        EmployeeId: null,
        Code: null,
        ActivityName: null,
        ActivityDetail: null,
        PurposeOfTheActivity: null,
        ActivityCategory: null,
        OtherActivityCategory: null,
        ActivityClass: null,
        Priority: null,
        ActivityType: null,
        Period: null,
        Frequency: 1,
        AverageTime: null,
        ActivityImportance: null,
        ValueInActivity: null,
        FinancialImpact: null,
        Documents: false,
        Remarks: null,
        KPI: false
    }
    $scope.activityNew = Object.assign({}, $scope.activity);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.partyList = [];
    $scope.ShowCustomerPopUpNew = function () {
        $scope.partyType = "Customer";
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];

        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + '' + '&PlantId=' + '';

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
        });
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('show');
    };

    $scope.SetCustomerData = function (obj) {
        var party = obj.data;
        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.CustomerName = party.UserName;
        $scope.ModelNew.PartyId = party.Id;

        $scope.hidePartyPopUp();
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.searchParty = '';
    }

    $scope.closeCustomerPopUpNew = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.hidePartyPopUp();
        $scope.partyType = "Customer";
        $scope.searchParty = '';
    }

    $scope.hidePartyPopUp = function () {
        angular.element(document.querySelector('#CustomerPopUpNew')).modal('hide');
        $scope.partyIndex = -1;
        $scope.partySelected = null;
    };

    $scope.ChangeCustomer = function () {
        if ($scope.ModelNew.IfPartyApplicable) {
            $scope.ModelNew.CustomerName = null;
            $scope.ModelNew.PartyId = null;
        }
        else {
            $scope.ModelNew.CustomerName = party.UserName;
            $scope.ModelNew.PartyId = party.Id;
        }

    };

    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';

    $scope.showEmployeeListPopUp = function (name) {
        try {
            $scope.Name = name;
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];

            $scope.ModelNew.EmployeeCode = employee.EmployeeCode;
            $scope.ModelNew.EmployeeId = employee.SystemId;
            $scope.ModelNew.EmployeeName = employee.EmployeeName;
            $scope.ModelNew.PositionCode = employee.PositionCode;
            $scope.ModelNew.BudgetCode = employee.BudgetCode;
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
       /* $scope.GetActivity(args.data.Id);*/
        $scope.getActivityGridData($scope.ModelNew.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.GetActivity = function (args) {
        $scope.activityNew = Object.assign({}, args.data);
    };


    $scope.Save = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   /* ClearFields(response.data.Sequence);*/
                    $scope.getData();
                  
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };
    $scope.SaveActivity = function () {
        $http({
            method: 'POST',
            url: $scope.saveActivityUrl,
            data: { 'data': $scope.activityNew, 'EmpUnderstandingHeadId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
               /* ClearFields(response.data.Sequence);*/
                $scope.getData();
                $scope.getActivityGridData($scope.ModelNew.Id);

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
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
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };


    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }
}