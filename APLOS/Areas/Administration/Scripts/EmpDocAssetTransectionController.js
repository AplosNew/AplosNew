'use strict';
EmpDocAssetTransectionController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function EmpDocAssetTransectionController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = "Emp Doc & Asset Transaction";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.orderCategories = [];
    $scope.path = 'Administration/EmpDocAssetTransection/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.getDMSeqUrl = $scope.path + 'GetDMAutoSequence';
    $scope.saveDMUrl = $scope.path + 'CreateDocumentationMaster';
    $scope.deleteDMUrl = $scope.path + 'DeleteDocumentaitonMaster/';
    $scope.partyType = "Party";
    //  $controller("partyBaseController", { $scope: $scope, $http: $http });
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelList = [];
    $scope.searchBy = "UserName"; $scope.search = "";
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

    $scope.documentation = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        Purpose: null,
        Category: null,
        AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.documentationNew = Object.assign({}, $scope.documentation);

    $scope.searchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        }
    ];

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.documentationNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    $scope.Get = function (obj) {
        $scope.documentationNew = Object.assign({}, obj.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
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

    $scope.CategoryList = [];
    $scope.GetCategoryByMaster = function () {
        $http({
            method: 'GET',
            url: $scope.path +  'GetCategory'
        }).then(function successCallback(response) {
            $scope.CategoryList = [];
            if (baseService.arrayLength(response.data) > 0) {
                $scope.CategoryList = response.data;
            }
        });
    };

    $scope.employeeUrl = 'OrderManagements/masterorder/GetEmployeeListResponsible';
    $scope.showEmployeeListPopUp = function () {
        try {
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
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
        var employee = $scope.employeeList[$scope.employeeIndex];
        $scope.documentationNew.ResponsiblePersonId = employee.SystemId;
        $scope.documentationNew.ResponsiblePerson = employee.EmployeeName;
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.PurposeList = [
        { Value: "Sale", Text: "Sale" },
        { Value: "Purchase", Text: "Purchase" },
        { Value: "Expense", Text: "Job Expense" }
    ];

    $scope.CategoryList = [
        { Value: "Local", Text: "Local" },
        { Value: "Overseas", Text: "Overseas" }
    ];

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentationNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.documentationNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $scope.GetSequence()
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.documentationNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $scope.GetSequence()
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    $scope.Clear = function () {
        ClearFields();
        $scope.GetSequence();
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.documentation = {};
        $scope.documentationNew = {};
        $scope.documentationNew.PlanningPriority = seq;
        $scope.documentationNew.Active = true;
    }


    $scope.documentationMaster = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Source: null,
        DocumentType: null,
        DocumentFormat: null,
        AddedBy: null, AddedDate: null, AddedFromIP: null, UpdatedBy: null, UpdatedDate: null, UpdatedFromIP: null
    };
    $scope.documentationMasterNew = Object.assign({}, $scope.documentationMaster);

    $scope.DocumentFormatList = [{ Value: 'PDF', Text: 'PDF' },
    { Value: 'JPEG', Text: 'JPEG' },
    { Value: 'Excel', Text: 'Excel' },
    { Value: 'Word', Text: 'Word' },
    { Value: 'Register', Text: 'Register' },
    { Value: 'Form', Text: 'Form' },
    { Value: 'Email', Text: 'Email' },
    { Value: 'PPT', Text: 'PPT' },
    { Value: 'CrystalReport', Text: 'Crystal Report' },
    { Value: 'Txt', Text: 'Txt' },
    { value: 'CSV', Text: 'CSV' }]

    $scope.DMModelList = [];
    $scope.getDMData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDMList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DMModelList = response.data;
        });
    }
    $scope.getDMData();

    $scope.GetDMSequence = function () {
        $http.get($scope.getDMSeqUrl)
            .then(function (response) {
                $scope.documentationMasterNew.Sequence = response.data;
            });
    };
    $scope.GetDMSequence();

    $scope.DMAction = "Save";

    $scope.GetDM = function (obj) {
        $scope.documentationMasterNew = Object.assign({}, obj.data);
        $scope.DMAction = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDM = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.documentationMasterNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveDMUrl,
                data: { 'data': $scope.documentationMasterNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDMFields();
                    $scope.GetDMSequence();
                    $scope.getDMData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.message_Detailconfirmation = null;
    $scope.RemoveDM = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationMasterNew.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpBudget')).modal('show');
    }

    $scope.DeleteDM = function () {
        if (!baseService.isUndefinedOrNull($scope.documentationMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteDMUrl + $scope.documentationMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearDMFields($scope.GetDMSequence());
                    $scope.getDMData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
    };
    $scope.ClearDM = function () {
        ClearDMFields($scope.GetDMSequence());
        return true;
    };
    function ClearDMFields(seq) {
        $scope.DMAction = "Save";
        $scope.documentationMaster = {};
        $scope.documentationMasterNew = {};
        $scope.documentationMasterNew.Active = true;
    }

    $scope.SelectedDocumentationMasterModelList = [];
    $scope.DocumentSetId = null;
    $scope.ShowDocumentationMaster = function (obj) {
        $scope.DocumentSetId = obj.data.Id;
        $http({
            method: 'Get',
            url: $scope.path + "GetDocumentSetDetailList?documentSetId=" + $scope.DocumentSetId,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedDocumentationMasterModelList = response.data;

        });
        angular.element(document.querySelector('#SelectdDocumentationMasterPopUp')).modal('show');
    }

    $scope.CloseSelectedDMPopUp = function () {
        try {
            angular.element(document.querySelector('#SelectdDocumentationMasterPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.OpManList = [
        { Value: "Optional", Text: "Optional" },
        { Value: "Mandatory", Text: "Mandatory" },
    ];

    $scope.DocumentationMasterModelList = [];
    $scope.GetDocumentationMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDMList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DocumentationMasterModelList = response.data;
            angular.element(document.querySelector('#DocumentationMasterPopUp')).modal('show');

        });
    }

    // #region checkbox all DocumentMaster

    $scope.refreshTemplategrid = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridDocumentationMaster").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.DocumentationMasterModelList.length; i++) {
                $scope.DocumentationMasterModelList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridDocumentationMaster").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.CloseDMPopUp = function () {
        try {
            MakeData();
            angular.element(document.querySelector('#DocumentationMasterPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DocumentationMasterId === id) {
                return true;
            }
        }
        return false;
    }

    function MakeData() {
        for (var i = 0; i < $scope.DocumentationMasterModelList.length; i++) {
            if ($scope.DocumentationMasterModelList[i].Flag == true) {
                if (checkExists($scope.SelectedDocumentationMasterModelList, $scope.DocumentationMasterModelList[i].Id) === false) {
                    var ob = {};

                    ob.Id = null;
                    ob.DocumentSetId = $scope.DocumentSetId;
                    ob.DocumentationMasterId = $scope.DocumentationMasterModelList[i].Id;
                    ob.Sequence = $scope.DocumentationMasterModelList[i].Sequence;
                    ob.Code = $scope.DocumentationMasterModelList[i].Code;
                    ob.ShortName = $scope.DocumentationMasterModelList[i].ShortName;
                    ob.StandardName = $scope.DocumentationMasterModelList[i].StandardName;
                    ob.UserName = $scope.DocumentationMasterModelList[i].UserName;
                    ob.Source = $scope.DocumentationMasterModelList[i].Source;
                    ob.DocumentType = $scope.DocumentationMasterModelList[i].DocumentType;
                    ob.DocumentFormat = $scope.DocumentationMasterModelList[i].DocumentFormat;
                    ob.OptionalOrMandatory = null;
                    ob.Active = true;

                    $scope.SelectedDocumentationMasterModelList.push(ob);
                    ob = {};
                }
            }
        }

    }

    $scope.GetSavedDocMaster = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetDocumentSetDetailList?documentSetId=" + $scope.DocumentSetId,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SelectedDocumentationMasterModelList = response.data;

        });
    }

    $scope.SaveTaggedDocmuent = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/Documentation/SaveTaggedDocmuent",
            data: { 'data': $scope.SelectedDocumentationMasterModelList, 'documentSetId': $scope.DocumentSetId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetSavedDocMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.searchByParty = "UserName"; $scope.searchParty = "";
    $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
    $scope.partyList = [];

    $scope.SavedPartyDocSetList = [];
    $scope.GetPartyDocumentSetDetailList = function () {
        $http({
            method: 'Get',
            url: $scope.path + "GetPartyDocumentSetDetailList?documentSetId=" + $scope.DocumentSetId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.SavedPartyDocSetList = response.data;
            for (var i = 0; i < $scope.SavedPartyDocSetList.length; i++) {
                for (var j = 0; j < $scope.partyList.length; j++) {
                    if ($scope.SavedPartyDocSetList[i].Id == $scope.partyList[j].Id) {
                        $scope.partyList[j].CheckState = true;
                    }
                }
            }
            console.log($scope.partyList);
        });
    }

    $scope.showPartyPopUpNew = function (obj) {
        $scope.DocumentSetId = obj.data.Id;
        $scope.partyUrl = 'Parties/party/GetCompanyPartyDataListNew';

        $http({
            method: 'POST',
            url: $scope.partyUrl,
            data: { column: $scope.searchByParty, value: $scope.searchParty },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.partyList = response.data;
            $scope.GetPartyDocumentSetDetailList();
        });

        angular.element(document.querySelector('#partyPopUp')).modal('show');
    };


    // #region checkbox all Party

    $scope.refreshTemplatePartygrid = function (args) {
        $("#headchkParty").ejCheckBox({ "change": CheckBoxSelectAllParty });
    };

    function CheckBoxSelectAllParty(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#partyPopUpGrid").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.partyList.length; i++) {
                $scope.partyList[i].CheckState = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#partyPopUpGrid").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    $scope.SelectedpartyList = [];
    $scope.closeAndSave = function () {
        try {
            for (var i = 0; i < $scope.partyList.length; i++) {
                if ($scope.partyList[i].CheckState == true) {
                    $scope.SelectedpartyList.push($scope.partyList[i]);

                }
            }

            if (baseService.arrayLength($scope.SelectedpartyList) === 0) {
                throw "Select Party.";
            }
            $http({
                method: 'POST',
                url: 'OrderManagements/documentation/CreateDocumentSetWithParty',
                data: {
                    'DocumentSetId': $scope.DocumentSetId,
                    'SelectedpartyList': JSON.stringify($scope.SelectedpartyList)
                },
                dataType: 'JSON'
                , contentType: "application/json charset=utf-8"
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
            

            angular.element(document.querySelector('#partyPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    
   


}