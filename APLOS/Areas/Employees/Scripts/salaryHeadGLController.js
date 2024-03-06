'use strict';
salaryHeadGLController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function salaryHeadGLController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = 'Salary Head GL';
    $scope.Action = 'Save';
    $scope.btnActionAll = true;
    $scope.index = -1;
    $scope.salaryHeadSelectList = [];
    $scope.salaryHeadGLList = [];
    $scope.selectSalaryHeadMasterWithCombineList = [];
    $scope.AssetTypeGLList = [];
    $scope.ExpenseTypeGLList = [];
    $scope.path = 'employees/salaryHeadGL/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'SaveSalaryHeadGL';
    $scope.editUrl = $scope.path + 'EditSalaryHeadGL';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.EditSalaryHeadGLList = [];
    $scope.salaryHeadGL = {
        Id: null,
        CompanyId: null,
        //PlantId: null,
        COAId: null,
        CoaName: null,
        UpToDate: null,
        ManpowerBudgetId: null,
        ResponsiblePersonBy: null,
        SalaryHeadId: null,
        DrDirectGLId: null,
        DrDirectBudgetMasterId: null,
        DrDirectActivityId: null,
        DrInDirectGLId: null,
        DrInDirectBudgetMasterId: null,
        DrInDirectActivityId: null,
        PreGLId: null,
        PreBudgetId: null,
        PreActivityId: null,

        DrDirectOtherGLCode: null,
        DrDirectOtherGL: null,
        CrDirectOtherGLCode: null,
        CrDirectOtherGL: null,
        DrInDirectOtherGLCode: null,
        DrInDirectOtherGL: null,
        CrInDirectOtherGLCode: null,
        CrInDirectOtherGL: null

    };

    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    cboService.getEnumCbo("enum/GetSalaryPayableGroupEnum", function (result) {
        $scope.salaryPayableGroupList = result;
    });

    $scope.COAList = {};
    $scope.getCoa = function () {
        $http({
            method: 'GET',
            url: 'employees/SalaryHeadGL/GetCoaInfo?companyId=' + $scope.salaryHeadGL.CompanyId
        }).then(function successCallback(response) {
            $scope.salaryHeadGL.CoaName = response.data[0].CoaName;
            $scope.salaryHeadGL.COAId = response.data[0].COAId;
        });
    };

    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.salaryHeadGL.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };
    $scope.COAList = {};
    $scope.getCoa = function () {
        $http({
            method: 'GET',
            url: 'employees/SalaryHeadGL/GetCoaInfo?companyId=' + $scope.salaryHeadGL.CompanyId
        }).then(function successCallback(response) {
            $scope.salaryHeadGL.CoaName = response.data[0].CoaName;
            $scope.salaryHeadGL.COAId = response.data[0].COAId;
        });
    };

    $scope.getPlantList = function () {
        cboService.getCboPlantByCompany($scope.salaryHeadGL.CompanyId, function (result) {
            $scope.plantList = result;
        });
    };


    $scope.selectsalaryHeadGLWithCombineList = [];
    $scope.getsalaryHeadGLWithCoa = function (str) {
        $scope.selectsalaryHeadGLWithCombineList = [];
        $scope.tempList = [];
        if (str === 'all') {
            if ($scope.salaryHeadGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Employees/SalaryHeadGL/getlistwithcombine?coaId=' + $scope.salaryHeadGL.COAId;
        }
        if (str === 'notassing') {
            $scope.btnActionAll = true;
            if ($scope.salaryHeadGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Employees/SalaryHeadGL/getlistwithcombinenotassing?coaId=' + $scope.salaryHeadGL.COAId;
        }
        if (str === 'assing') {
            $scope.btnActionAll = true;
            if ($scope.salaryHeadGL.COAId === null) {
                return ShowResult("Select COA first", 'failure');
            }
            $scope.url = 'Employees/SalaryHeadGL/getlistwithcombineassing?coaId=' + $scope.salaryHeadGL.COAId;
        }
        baseService.setCurrentPage('selectsalaryHeadGLWithCombineList');
        baseService.init($scope.url, null, null, null, 'SalaryHead', 'SalaryHead');
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    if (result.Rows.length > 0) {
                        $scope.selectsalaryHeadGLWithCombineList = result.Rows;
                        // GetPartyAccountVD(result.Rows);
                    }
                    if (result.Rows.length > 0) {
                        $scope.tableShow = true;
                    } else {
                        $scope.tableShow = false;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
    };
    $scope.GetPartyAccountVDs = [];
    function GetPartyAccountVD(data) {
        $http.get('FixedAssets/fixedAssetMasterGL/getpartyaccountvd')
            .then(function (response) {
                $scope.selectFixedAssetMasterWithCombineList = data;
                $scope.GetPartyAccountVDs = response.data.Rows;
                for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
                    $scope.selectFixedAssetMasterWithCombineList[i].Flag = getActive($scope.tempList, $scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterId); //$scope.tempList.includes($scope.selectFixedAssetMasterWithCombineList[i].FixedAssetMasterId)
                }
                angular.forEach($scope.accountGroupList, function (item, j) {
                    for (var i = 0; i < $scope.selectFixedAssetMasterWithCombineList.length; i++) {
                        var ob = assignDomesticVendor($scope.GetPartyAccountVDs, $scope.selectFixedAssetMasterWithCombineList[i].PartyAccountGroupId, $scope.selectFixedAssetMasterWithCombineList[i].Id, item.PartyAccountGroupId);
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'GL'] = ob.GL;
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'Budget'] = ob.Budget;
                        $scope.selectFixedAssetMasterWithCombineList[i]['C' + j + 'Activity'] = ob.Activity;
                    }
                });
            });
    }
    $scope.tempList = [];
    $scope.tempIdList = [];
    $scope.selectChValueId = function (event, SalaryHeadID, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempListId($scope.tempList, data.SalaryHeadID) === false) {
                    $scope.tempList.push(data);
                }
            }
            else {
                for (var i = 0; i < $scope.tempList.length; i++) {
                    if ($scope.tempList[i].SalaryHeadID === data.SalaryHeadID) {
                        $scope.tempList.splice(i, 1);
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    };

    function checkExistTempListId(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === Id) {
                return true;
            }
        }
        return false;
    }
    function getActive(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].FixedAssetMasterId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.validation = function () {
        var keepGoing = true;
        angular.forEach($scope.salaryHeadGLListForSave, function (item) {
            if (keepGoing) {
                //    switch (item) {
                //        case (item.HeadType == 'D' && !baseService.isUndefinedOrNull(item.DrDirectActivityId) && !baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)):
                //            {
                //                ShowResult("Salary Head " + item.SalaryHead + " is deduction Type. Please input only Credit GL!", "failure");
                //                keepGoing = false;
                //            }
                //        case (item.HeadCategory == 'Net Payable'
                //            && baseService.isUndefinedOrNull(item.CrDirectBudgetMasterId) && baseService.isUndefinedOrNull(item.CrDirectActivityId)):
                //            {
                //                ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input  Credit GL only!", "failure");
                //                keepGoing = false;
                //            }
                //        case (item.HeadCategory == 'Net Payable'
                //            && !baseService.isUndefinedOrNull(item.DrDirectActivityId) && !baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)):
                //            {
                //                ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input  Credit GL only!", "failure");
                //                keepGoing = false;
                //            }
                //}

                if (item.HeadType == 'D' && !baseService.isUndefinedOrNull(item.DrDirectActivityId) && !baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)) {
                    ShowResult("Salary Head " + item.SalaryHead + " is deduction Type. Please input only Credit GL!", "failure");
                    keepGoing = false;
                    return true;
                }
                if (item.HeadCategory == 'ESIC Employer Contribution'
                    && baseService.isUndefinedOrNull(item.DrDirectActivityId) && baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)
                    && baseService.isUndefinedOrNull(item.CrDirectBudgetMasterId) && baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)) {
                    ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input Debit and  Credit both GL!", "failure");
                    keepGoing = false;
                    return true;
                }
                if (item.HeadCategory == 'PF Employer Contribution'
                    && baseService.isUndefinedOrNull(item.DrDirectActivityId) && baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)
                    && baseService.isUndefinedOrNull(item.CrDirectBudgetMasterId) && baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)) {
                    ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input Debit and  Credit both GL!", "failure");
                    keepGoing = false;
                    return true;
                }
                if (item.HeadCategory == 'Net Payable'
                    && baseService.isUndefinedOrNull(item.CrDirectBudgetMasterId) && baseService.isUndefinedOrNull(item.CrDirectActivityId)) {
                    ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input  Credit GL only!", "failure");
                    keepGoing = false;
                    return true;
                }
                if (item.HeadCategory == 'Net Payable'
                    && !baseService.isUndefinedOrNull(item.DrDirectActivityId) && !baseService.isUndefinedOrNull(item.DrDirectBudgetMasterId)) {
                    ShowResult("Salary Head " + item.SalaryHead + " is E Type. Please input  Credit GL only!", "failure");
                    keepGoing = false;
                    return true;
                }
            }

        });
        if (keepGoing == false) {
            return true;
        }
        else {
            return false;

        }
    };


    $scope.addGlForSelectble = function () {
        $scope.salaryHeadGLListForSave = [];
        //$scope.tempList = [];
        //angular.forEach($scope.selectFixedAssetMasterWithCombineList, function (item) {
        angular.forEach($scope.tempList, function (item) {
            if (item.Flag) {
                item.DrDirectGLId = $scope.salaryHeadGL.DrDirectGLId;
                item.DrDirectBudgetMasterId = $scope.salaryHeadGL.DrDirectBudgetMasterId;
                item.DrDirectActivityId = $scope.salaryHeadGL.DrDirectActivityId;
                item.CrDirectGLId = $scope.salaryHeadGL.CrDirectGLId;
                item.CrDirectBudgetMasterId = $scope.salaryHeadGL.CrDirectBudgetMasterId;
                item.CrDirectActivityId = $scope.salaryHeadGL.CrDirectActivityId;
                item.DrInDirectGLId = $scope.salaryHeadGL.DrInDirectGLId;
                item.DrInDirectBudgetMasterId = $scope.salaryHeadGL.DrInDirectBudgetMasterId;
                item.DrInDirectActivityId = $scope.salaryHeadGL.DrInDirectActivityId;
                item.CrInDirectGLId = $scope.salaryHeadGL.CrInDirectGLId;
                item.CrInDirectBudgetMasterId = $scope.salaryHeadGL.CrInDirectBudgetMasterId;
                item.CrInDirectActivityId = $scope.salaryHeadGL.CrInDirectActivityId;

                item.DrDirectOtherGLCode = $scope.salaryHeadGL.DrDirectOtherGLCode;
                item.DrDirectOtherGL = $scope.salaryHeadGL.DrDirectOtherGL;
                item.CrDirectOtherGLCode = $scope.salaryHeadGL.CrDirectOtherGLCode;
                item.CrDirectOtherGL = $scope.salaryHeadGL.CrDirectOtherGL;
                item.DrInDirectOtherGLCode = $scope.salaryHeadGL.DrInDirectOtherGLCode;
                item.DrInDirectOtherGL = $scope.salaryHeadGL.DrInDirectOtherGL;
                item.CrInDirectOtherGLCode = $scope.salaryHeadGL.CrInDirectOtherGLCode;
                item.CrInDirectOtherGL = $scope.salaryHeadGL.CrInDirectOtherGL;

                item.COAId = $scope.salaryHeadGL.COAId;
                item.CompanyId = $scope.salaryHeadGL.CompanyId;
                //item.PlantId = $scope.salaryHeadGL.PlantId;
                $scope.salaryHeadGLListForSave.push(item);
            }
        });
    }

    $scope.Save = function () {
        //$scope.addGlForSelectble();
        //if (baseService.isUndefinedOrNull($scope.AssetGLId) && baseService.isUndefinedOrNull($scope.AccumulatedDirectGLGLId) && baseService.isUndefinedOrNull($scope.DirectGLGLId) && baseService.isUndefinedOrNull($scope.AssetUnderConstructionGLId) && baseService.isUndefinedOrNull($scope.DownPaymentGLId) && baseService.isUndefinedOrNull($scope.ClearingAccountGLId) && baseService.isUndefinedOrNull($scope.GainOnSaleAssetGLId) && baseService.isUndefinedOrNull($scope.LossOnSaleAssetGLId) && baseService.isUndefinedOrNull($scope.LossOnDisposalAssetGLId) && checkVendorReconGLIsAssinged($scope.accountGroupList)) {
        //    return ShowResult("Please Select at least one GL!!", 'failure');
        //}
        if ($scope.SalaryHeadGlListByAccountGroup.length < 1) {
            return ShowResult("No list found!!", 'failure');
        }

        for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
            $scope.SalaryHeadGlListByAccountGroup[i].CompanyId = $scope.salaryHeadGL.CompanyId;
            $scope.SalaryHeadGlListByAccountGroup[i].COAId = $scope.salaryHeadGL.COAId;
            //$scope.SalaryHeadGlListByAccountGroup[i].PlantId = $scope.salaryHeadGL.PlantId;
            $scope.SalaryHeadGlListByAccountGroup[i].SalaryHeadId = $scope.SalaryHeadId;
        }

        $scope.$broadcast('show-errors-check-validity');
        if ($scope.salaryHeadGLNewForm.$valid) {//&& !$scope.validation()
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'salaryHeadGL': $scope.SalaryHeadGlListByAccountGroup,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        baseService.paginationAdd();
                        $scope.Clear();
                        $scope.getsalaryHeadGLWithCoa('all');
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }
        }
    };
    $scope.Clear = function () {
        $scope.tempList = [];
        $scope.refreshDirectGL()
        $scope.refreshCrDirectGL()
        $scope.refreshInDirectGL()
        $scope.refreshCrInDirectGL()
        $scope.salaryHeadGLListForSave = [];
    }
    $scope.searchDirectGLByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.DirectListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GlDrDirectIndex = "All";
    $scope.DirectGLList = [];
    $scope.GetDirectGLList = function (index) {
        if (!baseService.isUndefinedOrNull(index)) {
            $scope.GlDrDirectIndex = index;
        }
        else {
            $scope.GlCrInDirectIndex = "All";
        }
        if ($scope.salaryHeadGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        //$scope.GLUrl1 = 'accounts/glitem/GetExpenseGLBudgetActivityCOAWise?coaId=' + $scope.salaryHeadGL.COAId;
        $scope.GLUrl1 = 'employees/salaryheadgl/GetCRDirectIndirectGL?coaId=' + $scope.salaryHeadGL.COAId;
        //baseService.setCurrentPage('DirectGLList');
        $scope.GetDirectGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl1, pageno, $scope.DirectListParameters)
                .then(function (data) {
                    $scope.DirectGLList = data.Rows;
                    $scope.DirectListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#DirectGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetDirectGLListData();
    };
    $scope.closeDirectGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#DirectGLListPopUp')).modal('hide');
        }
    };
    $scope.setDirectGLSelected = function (x) {
        if ($scope.GlDrDirectIndex == "All") {

            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {

                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectGLId = x.GLGeneralInfoId;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectBudgetMasterId = x.BudgetMasterId;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectBudgetName = x.BudgetName;
                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectActivityId = x.ActivityId;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectActivityName = x.ActivityName;
            }


        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DrDirectGLId = x.GLGeneralInfoId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DrDirectBudgetMasterId = x.BudgetMasterId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DirectBudgetName = x.BudgetName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DrDirectActivityId = x.ActivityId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrDirectIndex].DirectActivityName = x.ActivityName;
        }
    };
    $scope.refreshDirectGL = function (index) {
        if (baseService.isUndefinedOrNull(index)) {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectGLId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectGLName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectBudgetMasterId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectBudgetName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DrDirectActivityId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DirectActivityName = null;
            }
        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[index].DrDirectGLId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DirectGLName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DrDirectBudgetMasterId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DirectBudgetName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DrDirectActivityId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DirectActivityName = null;
        }
    };

    $scope.searchCrDirectGLByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.CrDirectListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GlCrDirectIndex = "All";
    $scope.CrDirectGLList = [];
    $scope.GetCrDirectGLList = function (index) {
        if (!baseService.isUndefinedOrNull(index)) {
            $scope.GlCrDirectIndex = index;
        }
        else {
            $scope.GlCrDirectIndex = "All";
        }
        if ($scope.salaryHeadGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl3 = 'employees/salaryheadgl/GetCRDirectIndirectGL?coaId=' + $scope.salaryHeadGL.COAId;
        //baseService.setCurrentPage('DirectGLList');
        $scope.GetCrDirectGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl3, pageno, $scope.CrDirectListParameters)
                .then(function (data) {
                    $scope.CrDirectGLList = data.Rows;
                    $scope.CrDirectListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CrDirectGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCrDirectGLListData();
    };
    $scope.closeCrDirectGLListPopUpSelected = function () {
        if ($scope.CRDrowSelected !== null) {
            angular.element(document.querySelector('#CrDirectGLListPopUp')).modal('hide');
        }
    };
    $scope.setCrDirectGLSelected = function (x) {
        if ($scope.GlCrDirectIndex == "All") {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectGLId = x.GLGeneralInfoId;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectBudgetName = x.BudgetName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectBudgetMasterId = x.BudgetMasterId;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectActivityName = x.ActivityName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectActivityId = x.ActivityId;
            }
        }

        else {
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectGLId = x.GLGeneralInfoId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectBudgetName = x.BudgetName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectBudgetMasterId = x.BudgetMasterId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectActivityName = x.ActivityName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrDirectIndex].CrDirectActivityId = x.ActivityId;
        }
    };
    $scope.refreshCrDirectGL = function (index) {
        if (baseService.isUndefinedOrNull(index)) {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectGLName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectGLId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectBudgetName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectBudgetMasterId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectActivityName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrDirectActivityId = null;
            }
        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectGLName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectGLId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectBudgetName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectBudgetMasterId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectActivityName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrDirectActivityId = null;
        }

    };

    $scope.searchInDirectGLByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.InDirectListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GlDrInDirectIndex = "All";
    $scope.GetInDirectList = function (index) {
        if (!baseService.isUndefinedOrNull(index)) {
            $scope.GlDrInDirectIndex = index;
        }
        else {
            $scope.GlDrInDirectIndex = "All";
        }
        if ($scope.salaryHeadGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        //$scope.GLUrl2 = 'accounts/glitem/GetExpenseGLBudgetActivityCOAWise?coaId=' + $scope.salaryHeadGL.COAId;
        $scope.GLUrl2 = 'employees/salaryheadgl/GetCRDirectIndirectGL?coaId=' + $scope.salaryHeadGL.COAId;
        $scope.GetInDirectGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl2, pageno, $scope.InDirectListParameters)
                .then(function (data) {
                    $scope.InDirectGLList = data.Rows;
                    $scope.InDirectListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#InDirectGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetInDirectGLListData();
    };
    $scope.closeInDirectGLListPopUpSelected = function () {
        if ($scope.rowSelected !== null) {
            angular.element(document.querySelector('#InDirectGLListPopUp')).modal('hide');
        }
    };
    $scope.setInDirectGLSelected = function (x) {
        if ($scope.GlDrInDirectIndex == "All") {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectBudgetName = x.BudgetName;
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectActivityName = x.ActivityName;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectGLId = x.GLGeneralInfoId;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectBudgetMasterId = x.BudgetMasterId;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectActivityId = x.ActivityId;
            }
        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].InDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].InDirectBudgetName = x.BudgetName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].InDirectActivityName = x.ActivityName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].DrInDirectGLId = x.GLGeneralInfoId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].DrInDirectBudgetMasterId = x.BudgetMasterId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlDrInDirectIndex].DrInDirectActivityId = x.ActivityId;
        }

    };
    $scope.refreshInDirectGL = function (index) {
        if (baseService.isUndefinedOrNull(index)) {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectGLName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectBudgetName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].InDirectActivityName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectGLId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectBudgetMasterId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].DrInDirectActivityId = null;
            }
        } else {
            $scope.SalaryHeadGlListByAccountGroup[index].InDirectGLName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].InDirectBudgetName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].InDirectActivityName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DrInDirectGLId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DrInDirectBudgetMasterId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].DrInDirectActivityId = null;
        }

    };


    $scope.searchCrInDirectGLByList = [
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.CrInDirectListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'AccountGroupName',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.GlCrInDirectIndex = "All";
    $scope.GetCrInDirectList = function (index) {
        if (!baseService.isUndefinedOrNull(index)) {
            $scope.GlCrInDirectIndex = index;
        }
        else {
            $scope.GlCrInDirectIndex = "All";
        }
        if ($scope.salaryHeadGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $scope.GLUrl4 = 'employees/salaryheadgl/GetCRDirectIndirectGL?coaId=' + $scope.salaryHeadGL.COAId;
        $scope.GetCrInDirectGLListData = function (pageno) {
            baseService.paginationBase($scope.GLUrl4, pageno, $scope.CrInDirectListParameters)
                .then(function (data) {
                    $scope.CrInDirectGLList = data.Rows;
                    $scope.CrInDirectListParameters.total_count = data.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#CrInDirectGLListPopUp')).modal('show');
        $scope.modalShow = true;
        $scope.GetCrInDirectGLListData();
    };
    $scope.closeCrInDirectGLListPopUpSelected = function () {
        if ($scope.CRIrowSelected !== null) {
            angular.element(document.querySelector('#CrInDirectGLListPopUp')).modal('hide');
        }
    };
    $scope.setCrInDirectGLSelected = function (x) {
        if ($scope.GlCrInDirectIndex == "All") {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectGLId = x.GLGeneralInfoId;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectBudgetName = x.BudgetName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectBudgetMasterId = x.BudgetMasterId;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectActivityName = x.ActivityName;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectActivityId = x.ActivityId;
            }
        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectGLName = x.GLGeneralInfoCode + ' - ' + x.GLGeneralInfoName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectGLId = x.GLGeneralInfoId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectBudgetName = x.BudgetName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectBudgetMasterId = x.BudgetMasterId;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectActivityName = x.ActivityName;
            $scope.SalaryHeadGlListByAccountGroup[$scope.GlCrInDirectIndex].CrInDirectActivityId = x.ActivityId;
        }

    };
    $scope.refreshCrInDirectGL = function (index) {
        if (baseService.isUndefinedOrNull(index)) {
            for (var i = 0; i < $scope.SalaryHeadGlListByAccountGroup.length; i++) {
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectGLName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectGLId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectBudgetName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectBudgetMasterId = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectActivityName = null;
                $scope.SalaryHeadGlListByAccountGroup[i].CrInDirectActivityId = null;
            }
        }
        else {
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectGLName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectGLId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectBudgetName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectBudgetMasterId = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectActivityName = null;
            $scope.SalaryHeadGlListByAccountGroup[index].CrInDirectActivityId = null;
        }

    };

    $scope.editSalaryGL = function (data, index) {
        $scope.EditSalaryHeadGLList = [];
        $scope.EditSalaryHeadGLList.push($scope.selectsalaryHeadGLWithCombineList[index]);
        angular.element(document.querySelector('#editSalaryHeadGLPopUp')).modal('show');
    }
    $scope.closeEditSalaryHeadGLPop = function () {
        angular.element(document.querySelector('#editSalaryHeadGLPopUp')).modal('hide');

    }
    $scope.updateSalaryHeadGL = function () {
        $scope.EditSalaryHeadGLList[0].COAId = $scope.salaryHeadGL.COAId;
        $scope.EditSalaryHeadGLList[0].CompanyId = $scope.salaryHeadGL.CompanyId;
        //$scope.EditSalaryHeadGLList[0].PlantId = $scope.salaryHeadGL.PlantId;
        if ($scope.editSalaryHeadGLForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.editUrl,
                data: {
                    'editSalaryHeadGL': $scope.EditSalaryHeadGLList[0],
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeEditSalaryHeadGLPop();
                    $scope.getsalaryHeadGLWithCoa('all');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        }
    }

    $scope.getSalaryHeadGlReport = function () {
        var file_src = 'employees/salaryHeadGL/GetSalaryHeadGlReport';
        //  $scope.path = 'employees/salaryHeadGL/';
        $rootScope.report(file_src);
    }

    //#region PopUp for salary Head
    $scope.SalaryHeadName = null;
    $scope.SalaryHeadType = null;
    $scope.SalaryHeadTransactionType = null;
    $scope.SalaryHeadId = null;
    $scope.SalaryHeadGlList = [];
    $scope.SalaryHeadGlListByAccountGroup = [];
    $scope.getsalaryHead = function () {
        if ($scope.salaryHeadGL.COAId === null) {
            return ShowResult("Select COA first", 'failure');
        }
        $http.get('employees/SalaryHeadGL/GetListWithSalaryHead?coaId=' + $scope.salaryHeadGL.COAId)
            .then(function (response) {
                $scope.SalaryHeadGlList = response.data.Rows;
            });
        angular.element(document.querySelector('#SalaryHeadNewPopUp')).modal('show');
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#SalaryHeadNewPopUp')).modal('hide');
    };
    $scope.DrDisable = false;
    $scope.CrDisable = false;
    $scope.setData = function (obj) {
        try {

            if (obj.data.TransactionType == "Both" || obj.data.TransactionType == "Dr." || obj.data.TransactionType == "Cr.") {
                $scope.Clear();
                $scope.SalaryHeadName = obj.data.SalaryHead;
                $scope.SalaryHeadType = obj.data.HeadType;
                $scope.SalaryHeadTransactionType = obj.data.TransactionType;
                $scope.SalaryHeadId = obj.data.SalaryHeadID;

                if ($scope.SalaryHeadTransactionType == "Both") {
                    $scope.DrDisable = false;
                    $scope.CrDisable = false;
                }
                if ($scope.SalaryHeadTransactionType == "Dr.") {
                    $scope.DrDisable = false;
                    $scope.CrDisable = true;
                }
                if ($scope.SalaryHeadTransactionType == "Cr.") {
                    if ($scope.SalaryHeadName == "Net Pay") {
                        $scope.DrDisable = false;
                        $scope.CrDisable = false;
                    }
                    else {
                        $scope.DrDisable = true;
                        $scope.CrDisable = false;
                    }
                    
                }

                angular.element(document.querySelector('#SalaryHeadNewPopUp')).modal('hide');
                $scope.GetSalaryHeadGl(obj.data.SalaryHeadID);
            }
            else {
                throw "Cannot Select this SalaryHead [TransactionType: " + obj.data.TransactionType + "] ";
            }

            //var data = obj.data;

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetSalaryHeadGl = function (SalaryHeadId) {
        try {
            $http.get('employees/SalaryHeadGL/GetSalaryHeadGlbySalaryHead?SalaryHeadId=' + SalaryHeadId)
                .then(function (response) {
                    if (response.data.Error) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                    $scope.SalaryHeadGlListByAccountGroup = response.data.Rows;
                    }
                });
        } catch (e) {
            ShowResult(e, 'info');
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.SalaryHeadName = null;
        $scope.SalaryHeadType = null;
        $scope.SalaryHeadTransactionType = null;
        $scope.SalaryHeadId = null;
    }
    //#endregion


}