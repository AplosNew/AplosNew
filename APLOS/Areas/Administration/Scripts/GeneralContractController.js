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
        $scope.$broadcast('show-errors-check-validity');
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
                
               // ClearFields(response.data.Sequence);
                //$scope.getData();

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    };

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
               

                // ClearFields(response.data.Sequence);
                //$scope.getData();

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
    // #endregion Search fun for 

    // #region General Contract Item Master
    $scope.openGeneralContractMaster = function () {
        angular.element(document.querySelector('#contractmasterPopup')).modal('show');
        $scope.GetContractMaster()
    }
    $scope.closeGeneralContractMaster = function () {
        angular.element(document.querySelector('#contractmasterPopup')).modal('hide');
        
    }
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        angular.element(document.querySelector('#contractmasterPopup')).modal('show');
    };

    $scope.GetContractMaster = function () {
        $http.get('Administration/GeneralContract/GetContractMaster')
            .then(function successCallback(response) {
                $scope.ModelList = response.data;
            });
    }
    // #endregion General Contract Item Master

    // #region Double Tap open grid
    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        angular.element(document.querySelector('#contractmasterPopup')).modal('hide');
        }
    
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
        $http.get('Administration/GeneralContract/GetVendorBasedEmployee?vendorId=' + $scope.ModelNew.PartyId)
            .then(function successCallback(response) {
                $scope.VendorEmployeeList = response.data;
            });
    }
    // #endregion GetFun

    // #region SelectFun
    var currentDate = new Date();
    $scope.SelectedItemList = [];
    $scope.AddItems = function () {
        if (baseService.arrayLength($scope.ModelList) > 0) {
            angular.forEach($scope.ModelList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = '';
                    ob.GeneralContractId = $scope.ModelNew.Id;
                    ob.ContractMasterId = a.ContractMasterId;
                    ob.ContractMaster = a.ContractMaster
                    ob.MinQty = '';
                    ob.MaxQty = '';
                    ob.AvgQty = '';
                    ob.Rate = '';
                    ob.EffectiveDate = currentDate;
                    ob.FileName = '';
                    $scope.SelectedItemList.push(ob);
                    ob = {};
                    a.chk = false;
                    angular.element(document.querySelector('#contractmasterPopup')).modal('hide');
                }

            });
        }
       
        
    };

    $scope.SelectedCheckedByList = [];
    $scope.AddCheckedBy = function () {
        if (baseService.arrayLength($scope.CheckedByList) > 0) {
            angular.forEach($scope.CheckedByList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = '';
                    ob.SystemId = a.SystemId;
                    ob.EmployeeCode = a.EmployeeCode;
                    ob.EmployeeName = a.EmployeeName;
                    ob.DOJ = a.DOJ;
                    ob.Department = a.Department;
                    ob.Section = a.Section;
                    ob.SubSection = a.SubSection;
                    ob.LegalDesignation = a.LegalDesignation
                    ob.Designation = a.Designatio;
                    ob.isCheck = false;
                    $scope.SelectedCheckedByList.push(ob);
                    ob = {};
                    a.chk = false;
                    angular.element(document.querySelector('#checkbyPopUp')).modal('hide');
                }

            });
        }


    };

    $scope.SelectedApprovedByList = [];
    $scope.AddApprovedBy = function () {
        if (baseService.arrayLength($scope.ApprovedByList) > 0) {
            angular.forEach($scope.ApprovedByList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = '';
                    ob.SystemId = a.SystemId;
                    ob.EmployeeCode = a.EmployeeCode;
                    ob.EmployeeName = a.EmployeeName;
                    ob.DOJ = a.DOJ;
                    ob.Department = a.Department;
                    ob.Section = a.Section;
                    ob.SubSection = a.SubSection;
                    ob.LegalDesignation = a.LegalDesignation
                    ob.Designation = a.Designation
                    ob.isApprove = false;
                    $scope.SelectedApprovedByList.push(ob);
                    ob = {};
                    a.chk = false;
                    angular.element(document.querySelector('#approvedbyPopUp')).modal('hide');
                }

            });
        }


    };

    $scope.SelectedEntityList = [];
    $scope.AddEntity = function () {
        if (baseService.arrayLength($scope.EntityList) > 0) {
            angular.forEach($scope.EntityList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = '';
                    ob.EntityId = a.Id;
                    ob.UserName = a.UserName;
                    ob.Code = a.Code;
                    ob.EntityType = a.EntityType;
                    $scope.SelectedEntityList.push(ob);
                    ob = {};
                    a.chk = false;
                    angular.element(document.querySelector('#entityPopUp')).modal('hide');
                }

            });
        }

    };

    $scope.SelectedVendorEmployee = [];
    $scope.AddVendorEmployee = function () {
        if (baseService.arrayLength($scope.VendorEmployeeList) > 0) {
            angular.forEach($scope.VendorEmployeeList, function (a) {

                if (a.chk) {
                    var ob = {};
                    ob.Id = '';
                    ob.SystemId = a.SystemId;
                    ob.EmployeeCode = a.EmployeeCode;
                    ob.EmployeeName = a.EmployeeName;
                    ob.DOJ = a.DOJ;
                    ob.Department = a.Department;
                    ob.Section = a.Section;
                    ob.SubSection = a.SubSection;
                    ob.LegalDesignation = a.LegalDesignation
                    ob.Designation = a.Designation
                    $scope.SelectedVendorEmployee.push(ob);
                    ob = {};
                    a.chk = false;
                    angular.element(document.querySelector('#vendorEmployePopUp')).modal('hide');
                }

            });
        }

    };
     // #endregion SelectFun

    // #region RemoveRow
    $scope.RemoveParticularRow = function (item) {
        $scope.SelectedItemList.splice($scope.SelectedItemList.indexOf(item), 1)
    }

    $scope.RemoveCheckedRow = function (item) {
        $scope.SelectedCheckedByList.splice($scope.SelectedCheckedByList.indexOf(item), 1)
    }

    $scope.RemoveApprovedRow = function (item) {
        $scope.SelectedApprovedByList.splice($scope.SelectedApprovedByList.indexOf(item), 1)
    }
    $scope.RemoveEntityRow = function (item) {
        $scope.SelectedEntityList.splice($scope.SelectedEntityList.indexOf(item), 1)
    }
    $scope.RemoveVendorEmployeeRow = function (item) {
        $scope.SelectedEntityList.splice($scope.SelectedEntityList.indexOf(item), 1)
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
}