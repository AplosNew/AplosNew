'use strict';
UtilityMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function UtilityMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Utility Master';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Materials/UtilityMaster/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveChildUrl = $scope.path + 'CreateChild';

    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.Action = 'Save';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    $scope.searchByUMList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];

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

    $scope.UtilityMasterList = [];
    $scope.GetUtilityMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityMasterData?UtilityMasterId=" + $scope.ModelNew.Id,
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.UtilityMasterList = response.data;
            angular.element(document.querySelector('#UtilityMasterPopUp')).modal('show');
        });
    }

    $scope.GetUtilityMaster = function (obj) {
        $scope.ModelNew.InPutSource = obj.data.UserName;
        $scope.ModelNew.InPutSourceId = obj.data.Id;
        angular.element(document.querySelector('#UtilityMasterPopUp')).modal('hide');
        $scope.searchByUMList = '';
    }


    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    cboService.getCboEntityByPlant(null, null, " ", function (result) {
        $scope.entityList = result;
    });

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        UtilityGroup: null,
        UtilitySubGroup: null,
        UtilityCategory: null,
        UtilityGroupId: null,
        Item: null,
        UoMId: null,
        UoM: null,
        IsPartyApplicable: false,
        PartyId: null,
        PartyCode: null,
        PartyName: null,
        IsReadingApplicable: false,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        AdminId: null,
        Admin: null,
        Description: null,
        EntryLegDays: null,
        Remarks: null,
        Active: true,
        MultiplyingFactor: 0
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.UtilityGroupList = [];
    cboService.getUtilityGroupCbo(function (response) {
        $scope.UtilityGroupList = response;
    });

    $scope.uOMList = [];
    cboService.getUoMCbo(function (response) {
        $scope.uOMList = response;
    });

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.searchByParty = "UserName"; $scope.searchParty = "";

    $scope.partyList = [];
    $scope.partyType = "Vendor";
    $scope.ShowCustomerPopUpNew = function (partyType) {
        $scope.partyType = partyType;
        $scope.searchByPartyList = [{ value: 'Code', name: "Code" }, { value: 'UserName', name: $scope.partyType }, { value: 'PartyAccountGroupName', name: "Account Group" }, { value: 'CurrencyCode', name: "Currency" }, { value: 'CountryName', name: "Country" }, { value: 'StateName', name: "State" }];
        if ($scope.partyType == "Vendor" || $scope.partyType == "Customer") {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + $scope.partyType + '&CompanyId=' + '' + '&PlantId=' + '';
        } else {
            $scope.partyUrl = 'Parties/party/GetCompanyPartyDataSearch?partyType=' + null + '&CompanyId=' + '' + '&PlantId=' + '';
        }

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
        $scope.ModelNew.PartyName = party.UserName;
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
            $scope.ModelNew.PartyName = null;
            $scope.ModelNew.PartyId = null;
        }
        else {
            $scope.ModelNew.PartyName = party.UserName;
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
            //$scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        //$scope.employeeParameters.searchBy = 'EmployeeCode';
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
            if ($scope.Name == 'ad') {
                $scope.ModelNew.AdminId = employee.SystemId;
                $scope.ModelNew.Admin = employee.EmployeeName;
            }

        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    $scope.popUpDataList = [];
    $scope.popUp = function (name) {
        try {
            $scope.Name = name;
            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'Materials/UtilityMaster/getallemployeedata'

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectdblClick = function (obj) {
        var ob = obj.data;
        if ($scope.Name == 'ad') {
            $scope.ModelNew.AdminId = ob.SystemId;
            $scope.ModelNew.Admin = ob.EmployeeName;
        } else {
            $scope.ModelNew.ResponsiblePersonId = ob.SystemId;
            $scope.ModelNew.ResponsiblePersonName = ob.EmployeeName;
        }
      

        angular.element(document.querySelector('#popUp')).modal('hide');
    };

    $scope.closePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    };


    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.getUtilityGridData(args.data.Id);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.utilityDetails = [];
    $scope.getUtilityGridData = function (id) {
        $http({
            method: 'POST',
            url: $scope.path + "GetUtilityData",
            data: { 'UtilityMasterId': id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.utilityDetails = response.data;
            GetUtilityMasterAssetData();
        });
    }

    $scope.UtilityDetaildoubleclick = function (args) {
        $scope.ModelChildNew = Object.assign({}, args);
    };

    $scope.removeUtilityDetailsRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmUtilityDetailsRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeUtilityDetailsRow = function () {
        $http({
            method: 'POST',
            url: 'Materials/UtilityMaster/utilityDetailsDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getUtilityGridData($scope.ModelNew.Id);
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Save = function () {
        if ($scope.ModelNew.ResponsiblePersonName === null || $scope.ModelNew.ResponsiblePersonName === "") {
            ShowResult('Select Responsible Person Name', 'failure');
            return false;
        }
        else if ($scope.ModelNew.Admin === null || $scope.ModelNew.Admin === "") {
            ShowResult('Select Admin', 'failure');
            return false;
        }

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm) {
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
                    $scope.ModelNew.Id = response.data.Id;
                    //ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

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
    $scope.ModelChild = {
        Id: null,
        EffectiveDate: null,
        Rate: 0,
        Remark: null
    };
    $scope.ModelChildNew = Object.assign({}, $scope.ModelChild);

    $scope.SaveChild = function () {
        $http({
            method: 'POST',
            url: $scope.saveChildUrl,
            data: { 'data': $scope.ModelChildNew, 'UtilityMasterId': $scope.ModelNew.Id },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getData();
                $scope.getUtilityGridData($scope.ModelNew.Id);
                $scope.ClearUtilityDetail();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
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
        $scope.utilityDetails = [];
        $scope.SelectedAssetDataList = [];
    }

    $scope.ClearUtilityDetail = function () {
        $scope.ModelChildNew = Object.assign({}, $scope.ModelChild);
    }

    // #region Asset

    $scope.assetDataList = [];
    $scope.GetAssetDataList = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetAssetData'
        }).then(function successCallback(response) {
            $scope.assetDataList = response.data;
            for (var i = 0; i < $scope.assetDataList.length; i++) {
                for (var j = 0; j < $scope.SelectedAssetDataList.length; j++) {
                    if ($scope.assetDataList[i].MachineMasterAssetId == $scope.SelectedAssetDataList[j].MachineMasterAssetId) {
                        $scope.assetDataList.splice(i, 1);
                    }
                }
            }
        });
        angular.element(document.querySelector('#AssetPopUp')).modal('show');
    };

    $scope.closeAssetPopUp = function () {
        MakeData();
        $scope.SaveAssets();
        angular.element(document.querySelector('#AssetPopUp')).modal('hide');
    }

    // #region checkbox all

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllEmolyeeWise });
    };

    function CheckBoxSelectAllEmolyeeWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridAsset").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.assetDataList.length; i++) {
                $scope.assetDataList[i].Active = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridAsset").data("ejGrid");
        gridObj.refreshContent();
    };

    // #endregion checkbox all

    function GetUtilityMasterAssetData() {
        $http({
            method: 'GET',
            url: 'Materials/UtilityMaster/GetUtilityMasterAssetData?UtilityMasterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.SelectedAssetDataList = response.data;
        });
    }

    $scope.SelectedAssetDataList = [];
    function MakeData() {
        for (var i = 0; i < $scope.assetDataList.length; i++) {
            if ($scope.assetDataList[i].Active == true) {
                if (checkExists($scope.SelectedAssetDataList, $scope.assetDataList[i].MachineMasterAssetId) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.UtilityMasterId = $scope.ModelNew.Id;
                    ob.MachineMasterAssetId = $scope.assetDataList[i].MachineMasterAssetId;
                    ob.Entity = $scope.assetDataList[i].Entity;
                    ob.AssetCode = $scope.assetDataList[i].AssetCode;
                    ob.AssetName = $scope.assetDataList[i].AssetName;
                    ob.AssetDetail = $scope.assetDataList[i].AssetDetail;
                    ob.AssetReference = $scope.assetDataList[i].AssetReference;
                    ob.WorkCenterMaster = $scope.assetDataList[i].WorkCenterMaster;

                    $scope.SelectedAssetDataList.push(ob);
                    ob = {};
                }
                else {
                    throw "This Asset " + $scope.assetDataList[i].AssetName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MachineMasterAssetId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveAssets = function () {
        try {
            if (baseService.arrayLength($scope.SelectedAssetDataList) < 0) {
                throw "Select Asset.";
            }

            $http({
                method: 'POST',
                url: 'Materials/UtilityMaster/CreateAsset',
                data: { 'assets': $scope.SelectedAssetDataList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetUtilityMasterAssetData();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.message_detailconfirmation = null;
    $scope.removeAsset = function (obj) {

        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.AssetName + ' ]';
        angular.element(document.querySelector('#confirmAssetPopUp')).modal('show');
    }

    $scope.DeleteAsset = function () {
        $http({
            method: 'POST',
            url: 'Materials/UtilityMaster/DeleteAsset?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                GetUtilityMasterAssetData();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };
    // #endregion Asset

    //  #region UtilityCategory
    $scope.UCAction = 'Save';

    $scope.UtilityCategoryList = [];
    $scope.GetUtilityCategoryList = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUtilityCategoryList',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.UtilityCategoryList = response.data;
            })
    }
    //   $scope.GetUtilityCategoryList();

    $scope.UtilityCategoryTemp = {
        Id: null,
        CategoryName: null,
        UOMId: null,
        Remarks: null
    }
    $scope.UtilityCategoryModelNew = Object.assign({}, $scope.UtilityCategoryTemp);

    $scope.SaveUtilityCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SaveUtilityCategory',
            data: { 'data': $scope.UtilityCategoryModelNew },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.ClearUtilityCategory();
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetUtilityCategoryList();
                }
            })
    }

    $scope.DeleteUtilityCategory = function () {
        $http({
            method: 'POST',
            url: 'Materials/UtilityMaster/DeleteUtilityCategory?id=' + $scope.UtilityCategoryModelNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');

            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $scope.ClearUtilityCategory = function () {
        ClearFieldsUtilityCategory();
        return true;
    };

    function ClearFieldsUtilityCategory() {
        $scope.UCAction = 'Save';
        $scope.UtilityCategoryModelNew = {
            Id: null,
            CategoryName: null,
            UOMId: null,
            Remarks: null
        };

        $scope.UtilityCategoryModelNew = Object.assign({}, $scope.UtilityCategoryTemp);
    }
    //  #endregion UtilityCategory
}