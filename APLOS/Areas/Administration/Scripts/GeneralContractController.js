'use strict';
GeneralContractController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function GeneralContractController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'General Contract';
    $scope.ModelList = [];
    $scope.path = 'Administration/GeneralContract/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.Action = 'Save';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = 'Administration/GeneralContractItemMaster/Delete'
    $scope.partyType = 'Vendor';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.HeaderList = [];
    $scope.GetHeaderList = function () {
        $http.get('Administration/GeneralContract/GetHeaderList')
            .then(function successCallback(response) {
                $scope.HeaderList = response.data;
            })
    }
    $scope.GetHeaderList();

    $scope.contractItemDetail = []
    $scope.GetContractItemDetail = function () {
        $http.get('Administration/GeneralContract/GetContractItemDetail?gcId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.SelectedItemList = response.data;
            })
    }

    $scope.SelectedCheckedByList = [];
    $scope.GetCheckByList = function () {
        $http.get('Administration/GeneralContract/GetCheckByList?gcId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.SelectedCheckedByList = response.data;
            })
    }
    $scope.GetApproveByList = function () {
        $http.get('Administration/GeneralContract/GetApproveByList?gcId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.SelectedApprovedByList = response.data;
            })
    }

    $scope.GetSaveEntityList = function () {
        $http.get('Administration/GeneralContract/GetSaveEntityList?gcId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.SelectedEntityList = response.data;
            })
    }

    // #region Double Tap open grid
    $scope.Get = function (args) {
        $scope.ModelNew.PartyId = args.data.PartyId;
        $scope.ModelNew.PartyName = args.data.PartyName;
        $scope.ModelNew.PartyCode = args.data.PartyCode;
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.GetContractItemDetail();
        $scope.GetGeneralContractEmployee();
        $scope.GetCheckByList();
        $scope.GetApproveByList();
        $scope.GetSaveEntityList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();

        }
    };
    // #endregion Double Tap open grid

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };



    $scope.productNew = Object.assign({}, $scope.product);
    $scope.partyList = [];


    // CLOSE PARTY POP UP
    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.PartyName = party.UserName;
        $scope.ModelNew.PartyId = party.Id;

        $scope.hidePartyPopUp();
    };

    //#region List object
    $scope.ModelTemp = {
        Id: null,
        UserName: null,
        StandardName: null,
        ShortName: null,
        PartyId: null,
        PartyName: null,
        PartyCode: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    //  #region Save
    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            for (var i = 0; i < $scope.SelectedItemList.length; i++) {
                if (baseService.isUndefinedOrNull($scope.SelectedItemList[i].EffectiveDate)) {
                    throw 'Please select Effective Date first';
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: {
                    'data': $scope.ModelNew,
                    'contractItemDetail': $scope.SelectedItemList,
                    'checkby': $scope.SelectedCheckedByList,
                    'approveby': $scope.SelectedApprovedByList,
                    'entity': $scope.SelectedEntityList
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.GetContractItemDetail();
                    $scope.GetGeneralContractEmployee();
                    $scope.GetCheckByList();
                    $scope.GetApproveByList();
                    $scope.GetSaveEntityList();
                    $scope.GetHeaderList();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (e)
        {
            ShowResult(e, 'Error');
        };
    }

    $scope.SaveVendorEmployee = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: 'Administration/GeneralContract/SaveVendorEmployee',
            data: {
                'vendoremployee': $scope.SelectedVendorEmployee,
                'headerId': $scope.ModelNew.Id
            },
            dataType: 'JSON'
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
    };
    //  #endregion Save

    // #region Search fun for 
    $scope.searchByGeneralContractItem = "UserName";
    $scope.searchGeneralContractItem = "";

    $scope.GeneralContractItemSearchByList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
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
        },

        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Category',
            'value': 'Category'
        },
    ];

    $scope.getData = function () {
        $http.get('Administration/GeneralContractItemMaster/GetList?column=' + $scope.searchByGeneralContractItem + '&value=' + $scope.searchGeneralContractItem)
            .then(function successCallback(response) {

                $scope.ModelList = response.data
            });
    }

    $scope.GetGeneralContractEmployee = function () {
        $http.get('Administration/GeneralContract/GetGeneralContractEmployee?gcId=' + $scope.ModelNew.Id)
            .then(function successCallback(response) {
                $scope.SelectedVendorEmployee = response.data;
            })
    }
    // #endregion Search fun for 

    // #region General Contract Item Master
    $scope.openGeneralContractMaster = function () {
        angular.element(document.querySelector('#contractmasterPopup')).modal('show');
        $scope.GetContractMaster()
    }
    $scope.closeGeneralContractMaster = function () {
        angular.element(document.querySelector('#contractmasterPopup')).modal('hide');

    }
    //$scope.Get = function (args) {
    //    $scope.ModelNew = Object.assign({}, args.data);
    //    angular.element(document.querySelector('#contractmasterPopup')).modal('show');
    //};

    $scope.GetContractMaster = function () {
        $http.get('Administration/GeneralContract/GetContractMaster')
            .then(function successCallback(response) {
                $scope.ModelList = response.data;
            });
    }
    // #endregion General Contract Item Master

    // #region Double Tap open grid
    //$scope.Get = function (args) {
    //    $scope.ModelNew = Object.assign({}, args.data);
    //    angular.element(document.querySelector('#contractmasterPopup')).modal('hide');
    //    }

    // #endregion Double Tap open grid

    //----------------------------------------------------------------------------------------------------

    // #region Child
    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    //  #region Open POP UP
    $scope.OpenContractorPopUp = function () {
        angular.element(document.querySelector('#contractorPopUp')).modal('show');
    }
    $scope.OpenCheckByPopUp = function () {
        angular.element(document.querySelector('#checkbyPopUp')).modal('show');
        $scope.GetForCheckedByList();
    }
    $scope.OpenApprovedbyPopUp = function () {
        angular.element(document.querySelector('#approvedbyPopUp')).modal('show');
        $scope.GetForApprovedByByList();
    }
    $scope.OpenEntityPopUp = function () {
        angular.element(document.querySelector('#entityPopUp')).modal('show');
        $scope.GetEntityList();
    }
    $scope.OpenVendorEmployeePopUp = function () {
        angular.element(document.querySelector('#vendorEmployePopUp')).modal('show');        
        $scope.GetVendorBasedEmployee();
    }
    //  #endregion Open  POP UP

    //  #region close Pop Up
    $scope.CloseContractorPopUp = function () {
        angular.element(document.querySelector('#contractorPopUp')).modal('hide');
    }
    $scope.CloseCheckByPopUp = function () {
        angular.element(document.querySelector('#checkbyPopUp')).modal('hide');
    }
    $scope.CloseApprovedbyPopUp = function () {
        angular.element(document.querySelector('#approvedbyPopUp')).modal('hide');
    }
    $scope.CloseEntityPopUp = function () {
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    }
    $scope.CloseVendorEmployeePopUp = function () {
        angular.element(document.querySelector('#vendorEmployePopUp')).modal('hide');
    }
    //  #endregion close Pop Up

    // #region GetFun
    // #region  Get Contractor List
    $scope.ContractorList = [];
    $scope.GetContractorList = function () {
        $http.get('Administration/GeneralContract/GetContractorList')
            .then(function successCallback(response) {
                $scope.ContractorList = response.data;
            });
    }
    // #endregion  Get Contractor List

    // #region  Get Entity List
    $scope.EntityList = [];
    $scope.GetEntityList = function () {
        $http.get('Administration/GeneralContract/GetEntity')
            .then(function successCallback(response) {
                $scope.EntityList = response.data;
            });
    }
    // #endregion  Get Entity List

    // #region  Checked By
    $scope.CheckedByList = [];
    $scope.GetForCheckedByList = function () {
        $http.get('Administration/GeneralContract/GetForCheckedByList')
            .then(function successCallback(response) {
                $scope.CheckedByList = response.data;
            });
    }
    // #endregion  Checked By

    // #region  Approved By
    $scope.ApprovedByList = [];
    $scope.GetForApprovedByByList = function () {

        $http.get('Administration/GeneralContract/GetForApprovedByByList')
            .then(function successCallback(response) {
                $scope.ApprovedByList = response.data;
            });
    }
    // #endregion  Approved By

    $scope.VendorEmployeeList = [];
    $scope.GetVendorBasedEmployee = function () {
        $http.get('Administration/GeneralContract/GetVendorBasedEmployee')
            .then(function successCallback(response) {
                $scope.VendorEmployeeList = response.data;
            });
    }
    // #endregion GetFun

    // #region SelectFun
    var currentDate = new Date();

    $scope.AddItems = function () {
        //if (true) {
        for (var i = 0; i < $scope.ModelList.length; i++) {
            if ($scope.ModelList[i].chk == true) {
                if (checkDoubleGCItem($scope.SelectedItemList, $scope.ModelList[i].ContractMasterId) === false) {
                    $scope.SelectedItemList.push($scope.ModelList[i]);
                }
            }
        }
        //}
        //else {
        //    if (baseService.arrayLength($scope.ModelList) > 0) {
        //        angular.forEach($scope.ModelList, function (a) {

        //            if (a.chk) {
        //                var ob = {};
        //                ob.Id = '';
        //                ob.GeneralContractId = $scope.ModelNew.Id;
        //                ob.ContractMasterId = a.ContractMasterId;
        //                ob.ContractMaster = a.ContractMaster
        //                ob.MinQty = '';
        //                ob.MaxQty = '';
        //                ob.AvgQty = '';
        //                ob.Rate = '';
        //                ob.EffectiveDate = currentDate;
        //                ob.FileName = '';
        //                $scope.SelectedItemList.push(ob);
        //                ob = {};
        //                a.chk = false;
        //            }

        //        });
        //    }
        //}
        angular.element(document.querySelector('#contractmasterPopup')).modal('hide');

    };
    function checkDoubleGCItem(list, ContractMasterId) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ContractMasterId === ContractMasterId) {
                return true;
            }
        }
        return false;
    }

    $scope.AddCheckedBy = function () {
         for (var i = 0; i < $scope.CheckedByList.length; i++) {
            if ($scope.CheckedByList[i].chk == true) {
                if (checkDoubleEmpInformation($scope.SelectedCheckedByList, $scope.CheckedByList[i].SystemId) === false) {
                    $scope.SelectedCheckedByList.push($scope.CheckedByList[i]);
                }
            }
        }
        angular.element(document.querySelector('#checkbyPopUp')).modal('hide');
    };


    $scope.SelectedApprovedByList = [];
    $scope.AddApprovedBy = function () {
       for (var i = 0; i < $scope.ApprovedByList.length; i++) {
            if ($scope.ApprovedByList[i].chk == true) {
                if (checkDoubleEmpInformation($scope.SelectedApprovedByList, $scope.ApprovedByList[i].SystemId) === false) {
                    $scope.SelectedApprovedByList.push($scope.ApprovedByList[i]);
                }
            }
        }
        angular.element(document.querySelector('#approvedbyPopUp')).modal('hide');
    };

    $scope.SelectedEntityList = [];
    $scope.AddEntity = function () {
         for (var i = 0; i < $scope.EntityList.length; i++) {
            if ($scope.EntityList[i].chk == true) {
                if (checkDoubleEntity($scope.SelectedEntityList, $scope.EntityList[i].Id) === false) {
                    $scope.SelectedEntityList.push($scope.EntityList[i]);
                }
            }
        }
        angular.element(document.querySelector('#entityPopUp')).modal('hide');
    };
    function checkDoubleEntity(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EntityId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.SelectedVendorEmployee = [];
    $scope.AddVendorEmployee = function () {
        for (var i = 0; i < $scope.VendorEmployeeList.length; i++) {
            if ($scope.VendorEmployeeList[i].chk == true) {
                if (checkDoubleEmpInformation($scope.SelectedVendorEmployee, $scope.VendorEmployeeList[i].SystemId) === false) {
                    $scope.SelectedVendorEmployee.push($scope.VendorEmployeeList[i]);
                }
            }
        } 
        angular.element(document.querySelector('#vendorEmployePopUp')).modal('hide');
        $scope.SaveVendorEmployee();
    };

    function checkDoubleEmpInformation(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeId === Id) {
                return true;
            }
        }
        return false;
    }
    // #endregion SelectFun

    // #region RemoveRow
    $scope.RemoveParticularRowConfirmation = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmParticularRowRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RemoveContractItemDetailRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId.Id)) {
            $scope.SelectedItemList.splice($scope.SelectedItemList.indexOf($scope.tempId), 1)
        }

        else {
            $http({
                method: 'POST',
                url: $scope.path + 'delete',
                data: { 'item': $scope.tempId.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetContractItemDetail();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RemoveCheckedByRowConfirmation = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmCheckedByPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RemoveCheckedByRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId.Id)) {
            $scope.SelectedCheckedByList.splice($scope.SelectedCheckedByList.indexOf($scope.tempId), 1)
        }

        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deleteCheckedBy',
                data: { 'item': $scope.tempId.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetCheckByList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RemoveApprovedByRowConfirmation = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmApprovedByPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RemoveApprovedByRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId.Id)) {
            $scope.SelectedApprovedByList.splice($scope.SelectedApprovedByList.indexOf($scope.tempId), 1)
        }

        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deleteApprovedBy',
                data: { 'item': $scope.tempId.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetApproveByList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RemoveEntityRowConfirmation = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmEntityPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RemoveEntityRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId.Id)) {
            $scope.SelectedEntityList.splice($scope.SelectedEntityList.indexOf($scope.tempId), 1)
        }

        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deleteEntity',
                data: { 'item': $scope.tempId.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetSaveEntityList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.RemoveVendorEmployeeRowConfirmation = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmVendorEmpRemovePopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.RemoveVendorEmpRow = function () {
        if (baseService.isUndefinedOrNull($scope.tempId.Id)) {
            $scope.SelectedVendorEmployee.splice($scope.SelectedVendorEmployee.indexOf($scope.tempId), 1)
        }

        else {
            $http({
                method: 'POST',
                url: $scope.path + 'deleteVendorEmployee',
                data: { 'item': $scope.tempId.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGeneralContractEmployee();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    // #endregion RemoveRow

    //#region MOI File 
    $scope.ItemId = null;
    $scope.onBeginUpload = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.model.Data))
                throw 'Please select/save the order first'
            $scope.ItemId = args.model.Data;
            args.data = args.model.Data;
        } catch (e) {

            args.cancel = true;
            ShowResult(e, 'Error');
        }

    }
    $scope.uploadUrl = "Administration/GeneralContract/SaveDefault";
    $scope.fileselect = function (e) {

    }
    $scope.errorPicUpload = function (e) {
        if (angular.isUndefinedOrNull($scope.ItemId))
            ShowResult('Please select/save the order first', 'Error');
        else
            ShowResult("The selected file size is too large. Please select a file less than " + Math.round(e.model.fileSize / (1024 * 1024)) + "MB", 'failure');
    }

    $scope.FileDownload = function (data) {
        $scope.dwonloadUrl = null;
        var str = data.FileName;
        var extention = str.substr(str.indexOf('.'));
        $scope.dwonloadUrl = virtualPath.GeneralContractPath + '/' + data.Id + extention;

    };

    $scope.getFileList = function () {
        $http({
            method: 'Get',
            url: 'Administration/GeneralContract/LoadMaintenancePendingdScheduleList?ToDate=' + $scope.statusNew.ToDate + '&FromDate=' + $scope.statusNew.FromDate + '&MaintenanceId=' + $scope.PlannedId
        }).then(function successCallback(response) {
            $scope.MaintenanceStatusPlannedDetailsList = response.data;
            var gridObj = $("#GridPlannedMachineAsset").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#MachineAssetPop')).modal('show');
        }
        )
    }



    //#endregion
    // #endregion Child

    //  #region Clear
    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelTemp = {
            Id: null,
            UserName: null,
            StandardName: null,
            ShortName: null,
            PartyId: null,
            PartyName: null,
            PartyCode: null,
        };
        $scope.SelectedVendorEmployee = [];
        $scope.SelectedItemList = [];
        $scope.SelectedCheckedByList = [];
        $scope.SelectedApprovedByList = [];
        $scope.SelectedEntityList = [];
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    }
    //  #endregion Clear
}