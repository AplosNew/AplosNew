'use strict';
employeeDisciplinaryActionController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function employeeDisciplinaryActionController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Disciplinary Action';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.employeedisciplinaryActions = [];
    $scope.employeedisciplinaryActionsCount = [];
    $scope.path = 'humanresource/employeeDisciplinaryAction/';
    $scope.getListUrl = $scope.path + 'getListbyEmployee';
    $scope.getListCountUrl = $scope.path + 'getlistCount';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    //baseService.init($scope.getListUrl, null, null, null, "EntryDate", "EmpSystemId");   
    baseService.init($scope.getListCountUrl, null, null, null, "EmpSystemId", "EmpSystemId");

    $scope.getDataCount = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.employeeDisciplinaryActionsCount = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getDataCount();


    $scope.employeeDisciplinaryAction = {
        Id: null,
        EmployeeCode: null,
        EmpSystemId: null,
        EmployeeName: null,
        Description: null,
        EntryDate: null,
        DisciplinaryActionCategoryId: null
    };

    $scope.employeeDisciplinaryActionNew = Object.assign({}, $scope.employeeDisciplinaryAction);

    $scope.AddNew = function () {
        if (baseService.isUndefinedOrNull($scope.employeeDisciplinaryActionNew.EmployeeCode)) {
            ShowResult("Select Employee.", 'failure');
            return false;
        }
        ClearField();
        angular.element(document.querySelector('#entrydisciplinaryActionPopUp')).modal('show');
    };

    // #region Employee
    $scope.popUp = function () {

        try {

            $scope.employeeParameters = {
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

            $scope.searchEmployeeByList = [
                {
                    name: 'Employee Code',
                    value: 'EmployeeCode'
                },
                {
                    name: 'Employee Name',
                    value: 'EmployeeName'
                },
                {
                    name: 'Given Designation',
                    value: 'GivenDesignation'
                },
                {
                    name: 'Legal Designation',
                    value: 'LegalDesignation'
                },
                {
                    name: 'Department',
                    value: 'Department'
                }
                ,
                {
                    name: 'Entity',
                    value: 'EntityName'
                }

            ];

            $scope.popUpUrl = '';
            //$scope.employeeParameters.sort = '';
            $scope.employeeParameters.searchBy = '';
            $scope.popUpTitle = 'Employee';
            $scope.popUpUrl = 'employees/approvalconfiguration/getemployeedatalist';
            $scope.employeeParameters.sort = 'EmployeeCode';
            $scope.employeeParameters.searchBy = 'EmployeeCode';
            $scope.employeeParameters.offset = 0;
            baseService.setCurrentPage("employeeList");
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', '#employeePopUp');
                    }).finally(function () {
                    });
            };
            $scope.fieldName = name;
            angular.element(document.querySelector('#employeePopUp')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.setData = function (data) {
        $scope.Clear();
        $scope.employeeDisciplinaryActionNew.EmployeeCode = data.EmployeeCode;
        $scope.employeeDisciplinaryActionNew.EmpSystemId = data.SystemId;
        $scope.employeeDisciplinaryActionNew.EmployeeName = data.EmployeeName;
        $scope.employeeDisciplinaryActionNew.DOJ = $filter('dateFiltering')(data.DOJ, 'dd-MMM-yyyy');
        $scope.employeeDisciplinaryActionNew.GivenDesignation = data.GivenDesignation;
        $scope.employeeDisciplinaryActionNew.LegalDesignation = data.LegalDesignation;
        $scope.employeeDisciplinaryActionNew.Department = data.Department;
        $scope.imageSrc = virtualPath.EmployeePic + data.EmpPicPath;
        $scope.DisciplinaryActionPopUp1(data.SystemId);
        angular.element(document.querySelector('#employeePopUp')).modal('hide');

        $scope.employeeParameters.offset = 0;
    };

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
    }

    $scope.DisciplinaryActionPopUp = function (empid) {
        $http({
            method: 'GET',
            url: 'humanresource/employeeDisciplinaryAction/GetListByEmployee?EmpSysId=' + empid
        }).then(function successCallback(response) {
            $scope.employeeDisciplinaryActions = response.data;
        });
        angular.element(document.querySelector('#disciplinaryActionPopUp')).modal('show');
    }
    $scope.DisciplinaryActionPopUp1 = function (empid) {
        $http({
            method: 'GET',
            url: 'humanresource/employeeDisciplinaryAction/GetListByEmployee?EmpSysId=' + empid
        }).then(function successCallback(response) {
            $scope.employeeDisciplinaryActions = response.data;
        });
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        //angular.element(document.querySelector('#disciplinaryActionPopUp')).modal('show');
    }

    $scope.CloseDisciplinaryActionPopUp = function () {
        angular.element(document.querySelector('#disciplinaryActionPopUp')).modal('hide');
    }
    $scope.CloseEntryDisciplinaryActionPopUp = function () {
        angular.element(document.querySelector('#entrydisciplinaryActionPopUp')).modal('hide');
    }
    // #endregion

    //$scope.disciplinaryList = [];
    //cboService.getDisciplinaryCategotyCbo(function (result) {
    //    $scope.disciplinaryList = result;
    //})

    $scope.disciplinaryList = [];
    $scope.GETcbo = function () {
        $http.get('humanresource/disciplinaryActionCategory/GetList')
            .then(function (response) {
                $scope.disciplinaryList = response.data;
            });
    }
    $scope.GETcbo();

    $scope.GetChild = function (id, index) {
        $scope.index = index;
        $scope.employeeDisciplinaryAction = $scope.employeeDisciplinaryActions[$scope.index];
        $scope.employeeDisciplinaryActionNew = Object.assign({}, $scope.employeeDisciplinaryAction);
        $scope.Action = 'Update';
        angular.element(document.querySelector('#entrydisciplinaryActionPopUp')).modal('show');
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.employeeDisciplinaryAction = $scope.employeeDisciplinaryActionsCount[$scope.index];
        $scope.employeeDisciplinaryActionNew = Object.assign({}, $scope.employeeDisciplinaryAction);
        $scope.imageSrc = virtualPath.EmployeePic + $scope.employeeDisciplinaryActionNew.EmpPicPath;
        //$scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {

        var fd = $filter('dateFiltering')($scope.employeeDisciplinaryActionNew.EntryDate, 'dd-MM-yyyy');

        if (new Date(fd) > new Date()) {
            ShowResult("Entry Date Can't not be greater than today's Date", 'failure', 'entrydisciplinaryActionPopUp');
            return false;
        }
        angular.copy($scope.employeeDisciplinaryActionNew, $scope.employeeDisciplinaryAction);
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.employeeDisciplinaryActionNewForm1.$valid) {
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.employeeDisciplinaryAction,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'entrydisciplinaryActionPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.employeeDisciplinaryActions.push(response.data.employeeDisciplinaryAction);
                        $scope.employeeDisciplinaryActions = $filter('orderBy')($scope.employeeDisciplinaryActions, 'Id');
                        baseService.paginationAdd();
                        $scope.getDataCount();
                        $scope.DisciplinaryActionPopUp1(response.data.EmployeeDisciplinaryAction.EmpSystemId);
                        ClearField();
                        angular.element(document.querySelector('#entrydisciplinaryActionPopUp')).modal('hide');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'entrydisciplinaryActionPopUp');
                }
            }
            else {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.employeeDisciplinaryAction,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure', 'entrydisciplinaryActionPopUp');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.DisciplinaryActionPopUp1($scope.employeeDisciplinaryAction.EmpSystemId);
                        ClearField();
                        angular.element(document.querySelector('#entrydisciplinaryActionPopUp')).modal('hide');

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure', 'entrydisciplinaryActionPopUp');
                }
            }

        }
    };
    $scope.valuePassInDelModal = function (data) {
        $scope.Id = data.Id;
        if (baseService.isUndefinedOrNull($scope.Id))
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        else
            $scope.message_confirmation = 'Are you sure want to parmenently delete <b> [ ' + data.EmployeeCode + ' - ' + data.EmployeeName + ']</b>';
        angular.element(document.querySelector('#confirm_PopUp')).modal('show');
    };
    $scope.removeRow = function () {
        if (baseService.isUndefinedOrNull($scope.Id) === true) {
            $scope.employeeDisciplinaryActions.splice($scope.empIndex, 1);
            //$scope.empIndex = -1;
            $scope.Id = null;
        } else {
            $scope.Remove($scope.Id);
        }
        angular.element(document.querySelector('#confirm_PopUp')).modal('hide');
    };
    $scope.Remove = function (Id) {
        if (!baseService.isUndefinedOrNull(Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.employeeDisciplinaryActions.splice($scope.index, 1);
                    baseService.paginationRemove();
                    $scope.DisciplinaryActionPopUp1($scope.employeeDisciplinaryActionNew.EmpSystemId);
                    ClearField();
                    $scope.getDataCount();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.employeeDisciplinaryAction = {};
        $scope.imageSrc = {};
        $scope.employeeDisciplinaryActionNew = {};
        $scope.employeeDisciplinaryActions = [];
        $scope.employeeDisciplinaryActionNew.Active = true;
    }
    function ClearField() {
        $scope.Action = 'Save';
        $scope.employeeDisciplinaryActionNew.EntryDate = null;
        $scope.employeeDisciplinaryActionNew.Description = '';
        $scope.employeeDisciplinaryActionNew.DisciplinaryActionCategoryId = {};
        $scope.employeeDisciplinaryActionNew.Active = true;
    }












    $scope.LongAbsModel = {
        Id: null,
        EmpSystemId: null,
        DisciplinaryActionCategoryId: null,
        Description: null,
        EntryDate: $filter('dateFiltering')(Date.now()),
        ActionType: null,
        Letters: null,
        LettersFormat: null,
        DADID: null,
        EmployeeCode: null,
        EmployeeName: null,
        Department: null,
        designation: null,
        EntryDate: null,
        NextLetterDueDate: null,
        LetterIssueDate: null,
        Sequence: null,
        DisciplinaryActionSettingDetailsId: null,
        DisciplinaryActionCategoryId: null,
        OVERDUE: null,
        EmployeeDisciplinaryActionDetailsId: null
    }

    $scope.AddFunction = function (arg) {
        try {
            $scope.ShowSaveButton = true;
            $scope.ClearModel();
            var eDialog = $("#LongAbsenteeismInfo").data("ejDialog");
            eDialog.open();
            //var gridObj = $("#GridLeaveYearEndProcessSummary").data("ejGrid");
            //$scope.LongAbsModel = gridObj.getSelectedRecords()[0];


            $scope.LongAbsModel = arg;
            //$scope.GetActionCategory();
            $scope.ShowcaseLetter = null;
            $scope.UserName = null;
            $scope.EmployeeCode = $scope.LongAbsModel.EmployeeCode;
            $scope.EmployeeName = $scope.LongAbsModel.EmployeeName;
            $scope.Department = $scope.LongAbsModel.Department;
            $scope.designation = $scope.LongAbsModel.GivenDesignation;

            $scope.getLetterDescription();
            $scope.LetterFormatList = [];
            $scope.GetEmployeeDisciplinaryActionDetailsList();
        } catch (e) {
            ShowResult(e, "failure");
        }
    };



    $scope.ShowSaveButton = true;
    $scope.Actionlist = [];
    $scope.GetActionCategory = function () {
        $http.get('HumanResource/LongAbsenteeismAssign/GetActionCategory')
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.Actionlist = [];
                        $scope.Actionlist = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };
    $scope.GetActionCategory();

    $scope.EmployeeDisciplinaryActionDetailsList = [];
    $scope.GetEmployeeDisciplinaryActionDetailsList = function () {
        $scope.EmployeeDisciplinaryActionDetailsList = [];
        $http.get('humanresource/employeeDisciplinaryAction/GetEmployeeDisciplinaryActionDetailsList?Id=' + $scope.LongAbsModel.EmployeeDisciplinaryActionId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.EmployeeDisciplinaryActionDetailsList = [];
                        $scope.EmployeeDisciplinaryActionDetailsList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
    };





    $scope.saveChild = function () {
        try {
            Validation();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: 'humanresource/employeeDisciplinaryAction/SaveDAL',
                data: { 'longAbsenteeism': $scope.LongAbsModel, 'disciplinaryActionDetails': $scope.LongAbsModel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LongAbsModel.DADID = response.data.DADID;
                    var eDialog = $("#LongAbsenteeismInfo").data("ejDialog");
                    eDialog.close();
                    //$scope.getassigneddata();

                    //$scope.GetDueEmployeeDisciplinaryActionsList();                  
                    $scope.DisciplinaryActionPopUp1($scope.LongAbsModel.EmpSystemId);
                    var gridObj = $("#GridLeaveYearEndProcessSummary").data("ejGrid");
                    gridObj.refreshContent();

                    //var gridObj2 = $("#CompletedEmployeeDisciplinaryActionsList").data("ejGrid");
                    //gridObj2.refreshContent();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ClearModel = function () {
        $scope.LongAbsModel = {
            Id: null,
            EmpSystemId: null,
            DisciplinaryActionCategoryId: null,
            Description: null,
            EntryDate: $filter('dateFiltering')(Date.now()),
            ActionType: null,
            Letters: null,
            LettersFormat: null,
            DADID: null,
            EmployeeCode: null,
            EmployeeName: null,
            Department: null,
            designation: null,
            EntryDate: null,
            NextLetterDueDate: null,
            LetterIssueDate: null,
            Sequence: null,
            DisciplinaryActionSettingDetailsId: null,
            DisciplinaryActionCategoryId: null

        }

    }
    $scope.LetterList = [];
    $scope.LetterFormatList = [];
    $scope.getLetterDescription = function () {
        $http.get($scope.path + 'GetAllDescriptionForDA?DisciplinaryActionCategoryId='
            + $scope.LongAbsModel.DisciplinaryActionCategoryId
            + '&DisciplinaryActionSettingDetailsId=' + $scope.LongAbsModel.DisciplinaryActionSettingDetailsId
            + '&EmployeeDisciplinaryActionDetailsId=' + $scope.LongAbsModel.EmployeeDisciplinaryActionDetailsId
            + '&EmployeeDisciplinaryActionId=' + $scope.LongAbsModel.EmployeeDisciplinaryActionId)
            .then(function (response) {
                $scope.LetterList = response.data;


                //switch (count) {
                //    case 1:
                //        $scope.text = "One";
                //        break;
                //    case 2:
                //        $scope.text  = "Two";
                //        break;
                //    case 3:
                //        $scope.text  = "Three";
                //        break;
                //    case 4:
                //        $scope.text  = "Four";
                //        break;
                //    case 5:
                //        $scope.text  = "Five";
                //        break;
                //    case 6:
                //        $scope.text  = "Six";
                //        break;
                //    case 7:
                //        $scope.text  = "Seven";
                //        break;
                //    case 8:
                //        $scope.text  = "Eight";
                //        break;
                //    default:
                //        $scope.text  = "Zero";
                //}
                //$scope.TextIncount = $scope.text ;

                if ($scope.LetterList.length > 0) {
                    $scope.ShowcaseLetter = "Yes";
                    $scope.count = $scope.LetterList[0].Count;
                    $scope.UserName = $scope.LetterList[0].UserName;
                    $scope.LongAbsModel.EntryDate = $scope.LetterList[0].EntryDate;
                    $scope.LongAbsModel.NextLetterDueDate = $scope.LetterList[0].NextLetterDueDate;
                    $scope.LongAbsModel.LetterIssueDate = $scope.LetterList[0].LetterIssueDate;

                   

                    $scope.LongAbsModel.DisciplinaryActionSettingDetailsId = $scope.LetterList[0].Id;
                    $scope.LongAbsModel.DisciplinaryActionCategoryId = $scope.LetterList[0].DisciplinaryActionCategoryId;
                    $scope.Sequence = $scope.LetterList[0].Sequence;



                    $scope.Id = $scope.LetterList[0].Id
                    $http.get('humanresource/LongAbsenteeismAssign/GetLetterFormet?LetterFormetId=' + $scope.Id)
                        .then(function (response) {
                            $scope.LetterFormatList = response.data;
                            for (var i = 0; i < $scope.LetterFormatList.length; i++) {
                                if ($scope.LetterFormatList[i].IsDefault == true) {
                                    $scope.LongAbsModel.LettersFormat = $scope.LetterFormatList[i].Id;
                                }
                            }
                        });
                }
                else {
                    $scope.LetterFormatList = [];
                    $scope.ShowcaseLetter = null;
                    $scope.UserName = null;
                    $scope.Sequence = null;
                    $scope.LongAbsModel.LettersFormat = null;
                    $scope.LongAbsModel.Description = null;
                    $scope.LongAbsModel.EntryDate = null;
                    $scope.ShowSaveButton = false;
                }
            });
    };
    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };
    function Validation() {
        try {
            CheckField("LettersFormat", $scope.LongAbsModel.LettersFormat);
            CheckField("Letter Issue Day", $scope.LongAbsModel.DisciplinaryActionCategoryId);
            CheckField("Description", $scope.LongAbsModel.Description);
            CheckField("EntryDate", $scope.LongAbsModel.EntryDate);
           
        } catch (ex) {
            throw ex;
        }
    };


    //#region Report
    $scope.PrintDisciplinaryActionLetter = function () {
        try {
     

            var EmployeeDisciplinaryActionDetailsId = null;

            
                var gridObj = $("#GridEmployeeDisciplinaryActionDetailsList").data("ejGrid");
                EmployeeDisciplinaryActionDetailsId = gridObj.getSelectedRecords()[0].Id;
           


            try {
                location.href = $scope.path + 'DisciplinaryActionLetterInMSWord?EmployeeDisciplinaryActionDetailsId=' + EmployeeDisciplinaryActionDetailsId;
            } catch (e) {
                ShowResult(e, "failure");
            }



        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    // #endregion Report
}