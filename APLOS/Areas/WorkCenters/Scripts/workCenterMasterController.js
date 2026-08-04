'use strict';
WorkCenterMasterController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$http", "cboService", "$window", '$controller'];
function WorkCenterMasterController(commonMessage, $scope, $rootScope, baseService, $http, cboService, $window, $controller) {
    $rootScope.title = "Work Center Master";
    $scope.Action = 'Save';
    $scope.ActionDetail = 'Save';
    $scope.btndeletemaster = true;
    $scope.SaveDetailDisabled = false;
    $scope.searchbyLineList = [];
    $scope.lineList = [];
    $scope.message_confirmation = "";
    $scope.WorkCenterName = "";
    $scope.path = 'WorkCenters/workcentermaster/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByPlant';

    $scope.searchbyMasterlist = [];
    $scope.searchbyAssetlist = [];
    $scope.searchbyMachinelist = [];
    $scope.searchbyDetaillist = [];

    $scope.processList = [];
    $scope.masterList = [];
    $scope.detailList = [];
    $scope.machineList = [];
    $scope.unitList = [];

    $scope.Data = [];
    $scope.detail = {
        Id: null,
        FixedAssetItemId: null,
        SerialNo: null, FixedAsset: null,
        MachineType: null,
        MachineClass: null,
        Archive: null,
        WorkCenterMasterId: null
    }
    $scope.master = {
        PlantId: null,
        UnitId: null
    };
    $scope.mastermodal = {
        Id: null,
        Sequence: null,
        WorkCenterCategoryId: null,
        WorkCenterSubcategoryId: null,
        ProcessId: null,
        Code: null,
        UserName: null,
        StandardName: null,
        Description: null,
        Capacity: 0,
        PlantId: null,
        CompanyId: null,
        UnitId: null,
        CapacityProcessUoMId: null,
        LineId: null,
        UoMId: null,
        PlanEfficiency: 0,
        MaxTimePerDay: null,
        StandardTimePerDay: 0,
        PlanBudgetCapacityPerDay: null,
        DailyFixedCost: null,
        VariableCost: null,
        CurrencyId: null,
        SPT: null,
        CM: null,
        NoOfWorkStation: null,
        MonthlyNoOfDays: null,
        ResponsiblePersonId: null,
        ResponsiblePersonName: null,
        MentorId: null,
        MentorName: null,
        BuyerId: null,
        AccountHolder: null,
        AccountHolderName: null,
        AccountInCharge: null,
        AccountInChargeName: null,
        GroupingData: null,
        Active: true,
        OperationBulletinId: null,
        OperationBulletin: null,
        IsBusinessProcess: false
    };
    // #region DDL

    $scope.bulletinMasters = [];
    $scope.getBulletinData = function () {
        $scope.bulletinMasters = [];
        $http({
            method: 'GET',
            url: 'IE/bulletintemplate/getlist'
        }).then(function successCallback(response) {
            $scope.bulletinMasters = response.data;

            $("#BulletinPoUp").ejDialog("setTitle", "Operation Bulletin");
            var eDialog = $("#BulletinPoUp").data("ejDialog");
            eDialog.open();

            var gridObj = $("#Gridbulletin").data("ejGrid");
            gridObj.clearFiltering();
        });
    }

    $scope.CloseOperationBulletinPopup = function () {
        var eDialog = $("#BulletinPoUp").data("ejDialog");
        eDialog.close();
    }

    $scope.SetOperationBulletinData = function (args) {
        $scope.mastermodal.OperationBulletinId = args.data.Id;
        $scope.mastermodal.OperationBulletin = args.data.AlternativeName;
        $scope.CloseOperationBulletinPopup();
    }

    $scope.uomList = [];
    $scope.getuoMCbo = function (id) {
        $http.get('Setups/ProcessUoM/GetUoMCboByProcess?processId=' + id)
            .then(function successCallback(response) {
                $scope.uomList = response.data;
                for (var t = 0; t < baseService.arrayLength($scope.uomList); t++) {
                    if ($scope.uomList[t].IsBaseUom === 1) {
                        $scope.mastermodal.UoMId = $scope.uomList[t].Value;
                        break;
                    }
                }
            });
    };

    $scope.capacityUoMList = [];
    $scope.getCapacityUoMCbo = function (id) {
        $http.get('Setups/ProcessUoM/GetCapacityUoMCboByProcess?processId=' + id)
            .then(function successCallback(response) {
                $scope.capacityUoMList = response.data;
                if (baseService.arrayLength($scope.capacityUoMList) > 0) {
                    $scope.mastermodal.CapacityProcessUoMId = response.data[0].Value;
                }
            });
    }
    $scope.currencyList = [];

    function CurrencyList() {
        cboService.getCboTransactionCurrencyByCompany('', function (result) {
            $scope.currencyList = result;
        });
    };
    CurrencyList();

    $scope.getPlanBudgetCapacity = function () {
        try {
            $scope.mastermodal.PlanBudgetCapacityPerDay = ($scope.mastermodal.Capacity * ($scope.mastermodal.PlanEfficiency / 100) * $scope.mastermodal.StandardTimePerDay);
        } catch (e) {
            ShowResult(e, "Error", 'masteraddeditpopup');
        }
    }

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.mastermodal.Sequence = response.data;
            });
    }
    $scope.GetSequence();

    $scope.createguid = function (prefix) {
        var d = new Date().getTime();
        d += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = 'uid-';
        }
        d = prefix + d;
        return d;
    };
    $scope.loadPlant = function (companyId) {
        try {
            //$http.get($scope.path + "getplantcbo?companyId=" + companyId)
            //  .then(function (response) {
            //      $scope.plantList = response.data;
            //  });

            //$http.get($scope.path + "getunitcbo?companyId=" + companyId)
            // .then(function (response) {
            //     $scope.unitList = response.data;
            // });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDL = function () {
        try {
            cboService.getCboPlantByCompany($window.companyId, function (response) {
                $scope.plantList = response;
            })
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.getUnitList = function () {
        try {
            if ($scope.master.PlantId == null) {
                return $scope.unitList = [];
            }
            cboService.getCboUnitByCompany($window.companyId, function (response) {
                $scope.unitList = response;
            });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $http.get("workcenters/companygroupworkcentercategory/getcbo/")
        .then(function (response) {
            $scope.workCenterCategoryList = [];
            $scope.workCenterCategoryList = response.data;
        });
    $http.get("workcenters/companygroupworkcentersubcategory/getcbo/")
        .then(function (response) {
            $scope.workCenterSubcategoryList = [];
            $scope.workCenterSubcategoryList = response.data;
        });
    function loadDDLMasterModal() {
        try {


        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.loadDDLDetailChild = function () {
        try {
            $http.get($scope.path + "getmmuomcbo?materialmasterid=" + $scope.detailchildmodal.MaterialMasterId)
                .then(function (response) {
                    console.log(response.data)
                    $scope.uomChildList = response.data;
                    //$scope.detailchildmodal.BaseUOMId = BaseUOMId;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.getData = function () {
        baseService.init($scope.path + 'getlist', null, 25, null, 'FixedAssetCategory,FixedAssetSubcategory,Vendor,SerialNo,InvoiceDate,MachineType', 'SerialNo');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMasterlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMasterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    }
    function setSelected(uomid) {
        for (var i = 0; i < baseService.arrayLength($scope.machineList); i++) {
            if ($scope.machineList[i].Id == uomid) {
                $scope.machineList[i].IsSelectedID = true;
                break;
            }
        }
    }
    $scope.getMMData = function () {
        baseService.init($scope.path + 'getmaterialmasterlist', null, 25, null, 'Description', 'Description');
        $scope.loadMMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterDatalist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMData();
    }
    $scope.getAssetData = function () {
        baseService.init('fixedassets/companygroupfixedasset/getlist', null, 25, null, 'UserName', 'UserName');
        $scope.loadAssetData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.assetData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyAssetlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyAssetlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadAssetData();
    }
    $scope.getMachineList = function () {
        baseService.init('fixedassets/fixedassetmaster/getshortlist', null, 25, null, 'MachineType', 'MachineType');
        $scope.loadMachineData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.machineList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMachinelist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMachinelist);
                    }
                    //set selected
                    for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                        if ($scope.detailList[i].Archive == false) {
                            var uid = $scope.detailList[i].FixedAssetItemId;
                            setSelected(uid);
                        }
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMachineData();
    }
    $scope.getVendorData = function () {
        baseService.init('Parties/party/GetVendorList', null, 25, null, 'Description', 'Description');
        $scope.loadVendorData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.vendorData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyVendorlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyVendorlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadVendorData();
    }
    $scope.getMMForRMData = function () {
        baseService.init($scope.path + 'getmaterialmasterlist', null, 25, null, 'Description', 'Description');
        $scope.loadMMForRMData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmForRMData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMaterialMasterForRMDatalist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMaterialMasterForRMDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMForRMData();
    }

    $scope.getMasterData = function (data) {
        $scope.uomList = [];
        $scope.capacityUoMList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterlist?masterid=' + data.data.Id,
        }).then(function successCallback(response) {
            $scope.masterAddEditPopup("Edit");
            if (baseService.arrayLength(response.data) > 0) {
                $scope.mastermodal = response.data[0];
                $http.get('Setups/ProcessUoM/GetUoMCboByProcess?processId=' + $scope.mastermodal.ProcessId)
                    .then(function successCallback(result) {
                        $scope.uomList = result.data;
                    });
                $http.get('Setups/ProcessUoM/GetCapacityUoMCboByProcess?processId=' + $scope.mastermodal.ProcessId)
                    .then(function successCallback(response) {
                        $scope.capacityUoMList = response.data;
                    });
            }//if length>0
        })//success

    }
    $scope.getMasterList = function () {
        $http.get($scope.path + "getlist?plantid=" + $scope.master.PlantId + "&entityid=" + $scope.master.EntityId)
            .then(function (response) {
                $scope.masterList = [];
                $scope.masterList = response.data;
                if (baseService.arrayLength($scope.searchbyMasterlist) == 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyMasterlist);
                }
            });
        $scope.processList = [];
        $http.get("processes/entityprocesstag/getentityprocesscbo?entityId=" + $scope.master.EntityId)
            .then(function (response) {
                $scope.processList = response.data;
            });
    }
    $scope.getDetailData = function (id) {
        $http.get($scope.path + "getlist?plantid=" + plantid)
            .then(function (response) {
                $scope.masterList = [];
                $scope.masterList = response.data;
                if (baseService.arrayLength($scope.searchbyMasterlist) == 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyMasterlist);
                }
            });
    }
    $scope.getDetailList = function (masterid) {
        $scope.mastermodal.Id = masterid;
        $http.get($scope.path + "getdetaillist?masterid=" + masterid)
            .then(function (response) {
                $scope.detailList = [];
                $scope.detailList = response.data;
                if (baseService.arrayLength($scope.searchbyDetaillist) == 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyDetaillist);
                }
            });
    }
    $scope.showMasterList = function () {
        var val = baseService.isUndefinedOrNull($scope.master.EntityId);
        if (val) {
            return false;
        }
        else {
            return true;
        }
    }
    function loadMachineforPlant(plantid, currentmasterid) {
        $scope.allmachineList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillistbyplant?plantid=' + plantid + '&currentmasterid=' + currentmasterid,
        }).then(function successCallback(response) {//getmmuomcbo
            try {
                $scope.allmachineList = response.data;
                //console.log($scope.detailList);
                //console.log($scope.allmachineList);
                for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                    CheckMachineDuplication($scope.detailList[i].FixedAssetItemId, $scope.allmachineList);
                }
            } catch (e) {
                throw e;
            }
        })
    };
    //********************Line****************/
    $scope.lineListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getLineData = function () {
        baseService.setCurrentPage('lineList');
        $scope.loadLineData = function (pageno) {
            baseService.paginationBase('WorkCenters/WorkCenterMaster/GetLineList?entityId=' + $scope.master.EntityId, pageno, $scope.lineListParameters)
                .then(function (result) {
                    $scope.lineList = result.Rows;
                    $scope.lineListParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.searchbyLineList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyLineList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadLineData();
    };
    $scope.LineSearchPopup = function () {
        $scope.getLineData();
        angular.element(document.querySelector('#lineModal')).modal('show');
    };
    $scope.selectLineData = function (data) {
        $scope.mastermodal.LineId = data.Id;
        $scope.mastermodal.LineName = data.UserName;
        angular.element(document.querySelector('#lineModal')).modal('hide');
    };
    $scope.clearLine = function () {
        $scope.mastermodal.LineId = null;
        $scope.mastermodal.LineName = null;
    };
    // $scope.url = $scope.path + 'getweekendlist?plantId=' + $scope.offDayMaster.PlantId + '&fromDate=' + $scope.offDayMaster.FromDate + '&toDate=' + $scope.offDayMaster.ToDate;

    function CheckMachineDuplication(machineid, from_db_List) {
        try {
            for (var i = 0; i < baseService.arrayLength(from_db_List); i++) {
                if (from_db_List[i].FixedAssetItemId == machineid) {
                    throw "Serial No: [" + from_db_List[i].SerialNo + "], Machine Type: [" + from_db_List[i].MachineType + "], Machine Class: [" + from_db_List[i].MachineClass + "] has already been tagged with Work Center: [" + from_db_List[i].WorkCenter + "]...";
                }
            }
        }
        catch (ex) {
            throw ex;
        }
    }
    ///**************************************************grid row selected event function*********************************

    $scope.GetMachinneCode = function (id, SerialNo, MachineType, MachineClass) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if (id == $scope.detailList[i].FixedAssetItemId && $scope.detailList[i].Archive == false) {
                    throw "Machine having serial : [" + SerialNo + "] has already been taken...";
                }
            }
            var _guid = $scope.createguid("Id");

            $scope.detail.Archive = false;
            $scope.detail.Id = _guid;
            $scope.detail.FixedAssetItemId = id;
            $scope.detail.SerialNo = SerialNo;
            $scope.detail.MachineType = MachineType;
            $scope.detail.MachineClass = MachineClass;
            $scope.detail.WorkCenterMasterId = $scope.mastermodal.Id;
            var ob = angular.copy($scope.detail);
            $scope.detailList.push(ob);

            if (baseService.arrayLength($scope.searchbyDetaillist) == 0) {
                baseService.getDDLSearchColumn($scope.detailList, $scope.searchbyDetaillist);
            }

            angular.element(document.querySelector('#machinesearchpopup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'Information');
        }
    };
    function GetSelectedMachine(id, SerialNo, FixedAsset, MachineType, MachineClass) {
        for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
            if (id == $scope.detailList[i].FixedAssetItemId && $scope.detailList[i].Archive == false) {
                // throw "Machine having serial : [" + SerialNo + "] has already been taken...";
                return;
            }
        }
        var _guid = $scope.createguid("Id");

        $scope.detail.Archive = false;
        $scope.detail.Id = _guid;
        $scope.detail.FixedAssetItemId = id;
        $scope.detail.SerialNo = SerialNo;
        $scope.detail.FixedAsset = FixedAsset;
        $scope.detail.MachineType = MachineType;
        $scope.detail.MachineClass = MachineClass;
        $scope.detail.WorkCenterMasterId = $scope.mastermodal.Id;
        var ob = angular.copy($scope.detail);
        $scope.detailList.push(ob);
    }
    $scope.GetMachinneCode = function () {
        try {
            $scope.SaveDetailDisabled = false;
            for (var m = 0; m < baseService.arrayLength($scope.machineList); m++) {
                if ($scope.machineList[m].IsSelectedID) {
                    GetSelectedMachine($scope.machineList[m].Id, $scope.machineList[m].SerialNo, $scope.machineList[m].FixedAsset, $scope.machineList[m].MachineType, $scope.machineList[m].MachineClass);
                }
            }

            if (baseService.arrayLength($scope.searchbyDetaillist) == 0) {
                baseService.getDDLSearchColumn($scope.detailList, $scope.searchbyDetaillist);
            }

            angular.element(document.querySelector('#machinesearchpopup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'Information');
        }
    };
    $scope.clearAssetCode = function () {
        $scope.master.FixedAssetId = null;
        $scope.master.FixedAsset = null;
    };
    $scope.getMachineTypeCode = function (id, username) {
        $scope.master.MachineTypeId = id;
        $scope.master.MachineType = username;
        angular.element(document.querySelector('#machinetypemodal')).modal('hide');
    };
    $scope.clearMachineTypeCode = function () {
        $scope.master.MachineTypeId = null;
        $scope.master.MachineType = null;
    };

    $scope.CheckAllUOM = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < baseService.arrayLength($scope.machineList); i++) {
            $scope.machineList[i].IsSelectedID = _isselected;
        }
    }
    $scope.getVendorCode = function (id, username) {
        $scope.master.VendorId = id;
        $scope.master.Vendor = username;
        angular.element(document.querySelector('#vendormodal')).modal('hide');
    };
    $scope.clearVendorCode = function () {
        $scope.master.VendorId = null;
        $scope.master.Vendor = null;
    };
    $scope.GetMasterIndex = function (id) {
        //$scope.masterindex = index;
        //$scope.master = $scope.masterList[$scope.masterindex];
        //console.log($scope.master);
        $scope.getMasterData(id);
        // $scope.getDetailData(id);
        //$scope.btnDetailEntryPopup = true;
        // $scope.bulletinmastermodal = $scope.bulletinmasterList[$scope.masterindex];
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };
    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw ('[' + fieldName + '] is required...')
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length != 5) {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            if (fieldValue.substr(2, 1) != ':') {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            var a = parseInt(fieldValue.substr(0, 2));
            if (a > 23) {
                throw fieldName + ' can not be greater than 23...';
            }
            if (a < 0) {
                throw fieldName + ' can not be negetive...';
            }
            var b = parseInt(fieldValue.substr(3, 2));
            if (b > 59) {
                throw fieldName + ' can not be greater than 59...';
            }
            if (b < 0) {
                throw fieldName + ' can not be negetive...';
            }

            if (a == 0 && b == 0) {
                throw fieldName + ' can not be blank...';
            }
            //first 2 digit check integer
            //last 2 digit check integer
        } catch (e) {
            throw e;
        }
    }
    function ValidationMaster() {
        try {
            //CheckField($scope.master.PlantId, 'Plant');
            //CheckField($scope.mastermodal.ProcessId, 'Process');
            //CheckField($scope.mastermodal.WorkCenterCategoryId, 'Work Center Category');
            //CheckField($scope.mastermodal.WorkCenterSubcategoryId, 'Work Center Subcategory');
            //CheckField($scope.mastermodal.Code, 'Code');
            //CheckField($scope.mastermodal.UserName, 'User Name');
            //CheckField($scope.mastermodal.Capacity, 'Capacity');

            IsCodeUnique($scope.mastermodal.Code, $scope.mastermodal.Id);
        } catch (e) {
            throw e;
        }
    }
    function ValidationDetail() {
        try {
            //loadMachineforPlant($scope.master.PlantId, $scope.mastermodal.Id);
        } catch (e) {
            throw e;
        }
    }

    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailchildList); i++) {
                if (ob.Id != $scope.detailchildList[i].Id) {
                    if (ob.MaterialMasterId == $scope.detailchildList[i].RawMaterialId) {
                        throw "Material Master: [" + ob.MaterialMasterDescription + "] has already been taken...";
                    }//id
                }//id
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.ShowHead = function (mid) {
        if (mid == null || mid == '') {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.ShowHeadButton = function (mmid) {
        if (mmid == null || mmid == '') {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.showCharacteristicsGrid = function (hasCharForMM) {
        if (hasCharForMM == null || hasCharForMM == '') {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.loadProcessAsperConfig = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getprocessasperconfigcbo?materialmasterid=' + $scope.master.MaterialMasterId,
        }).then(function successCallback(response) {
            var r = response.data;
            if (baseService.arrayLength(r) > 0) {
                $scope.processList = r;
            }
        })
    };
    function IsCodeUnique(code, id) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.masterList); i++) {
                if ($scope.masterList[i].Code == code && $scope.masterList[i].Id != id) {
                    throw "Code: [" + code + "] exists in Work Center: [" + $scope.masterList[i].UserName + "]...";
                }
            }//for
        } catch (e) {
            throw e;
        }
    };

    $scope.getPlantConfigByPlant = function (plantid) {
        //cboService.getCboProductionEntityByPlant(null, null, plantid, function (result) {
        //    $scope.entityList = result;
        //});
        $http({
            method: 'POST',
            url: "Processes/EntityProcessTag/GetEntity?plantId=" + plantid
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
        });
    }
    ///**************************************************save delete and clear function*********************************
    $scope.SaveMaster = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.MasterForm.$valid && $scope.masterForm1.$valid) {
                ValidationMaster();
                $scope.mastermodal.PlantId = $scope.master.PlantId;
                $scope.mastermodal.EntityId = $scope.master.EntityId;
                $scope.mastermodal.CompanyId = $window.companyId;
                $http({
                    method: 'POST',
                    url: $scope.path + 'create',
                    dataType: 'JSON',
                    data: { 'master': $scope.mastermodal }
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure', 'masteraddeditpopup');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.masterAddEditPopup("NEW");
                        $scope.getMasterList();
                        $scope.GetSequence();
                        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure', 'masteraddeditpopup');
                });
                return true;
            }//valid
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ClosePopup = function () {
        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
    }

    $scope.DeleteMaster = function () {
        try {
            if ($scope.mastermodal.Id == null || $scope.mastermodal.Id == '') {
                throw 'No Work Center is selected...';
            }
            $http({
                method: 'POST',
                url: $scope.path + 'delete',
                dataType: 'JSON',
                data: { 'masterid': $scope.mastermodal.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.masterAddEditPopup('NEW');
                    $scope.getMasterList();
                    $scope.GetSequence();
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }

    $scope.MainPageToModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = $scope.master[i];
        }
    }

    function ClearMasterModal() {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = null;
        }
        $scope.btndeletemaster = false;
        $scope.Action = 'Save';
        loadDDLMasterModal();
        $scope.uomList = [];
    }
    $scope.ModalToMainPage = function () {
        for (var i in $scope.master) {
            $scope.master[i] = $scope.mastermodal[i];
        }
    }
    $scope.getPlantCompanyWise = function () {
        try {
            if ($scope.mastermodal.CompanyId.length == 0) {
                throw "Select Company first...";
            }
            $scope.loadPlant($scope.mastermodal.CompanyId);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }

    ///**************************************************show modal*********************************
    getBuyerLIst();
    function getBuyerLIst() {
        $scope.buyerLIst = [];
        cboService.getCboBuyer(function (result) {
            $scope.buyerLIst = result;
        });
    }
    $scope.masterAddEditPopup = function (flag) {
        try {
            if (flag === 'NEW') {
                ClearMasterModal();
                $scope.GetSequence();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
            else if (flag === 'DELETE') {
                ClearMaster();
                $scope.GetSequence();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
            }
            else {
                ClearMasterModal();
                $scope.btndeletemaster = true;
                $scope.Action = 'Update';
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.masterSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };
    $scope.showMMModal = function () {
        $scope.getMMData();
        angular.element(document.querySelector('#mmmodal')).modal('show');
    };
    $scope.showAssetModal = function () {
        $scope.getAssetData();
        angular.element(document.querySelector('#assetmodal')).modal('show');
    };
    $scope.showMachineModal = function () {
        $scope.getMachineList();
        angular.element(document.querySelector('#machinesearchpopup')).modal('show');
    };
    $scope.showVendorModal = function () {
        $scope.getVendorData();
        angular.element(document.querySelector('#vendormodal')).modal('show');
    };

    $scope.deleteMasterPopup = function (data) {
        try {
            $scope.mastermodal.Id = data.data.Id;
            if (baseService.isUndefinedOrNull(data.data.Id)) {
                throw "Select a Work Center...";
            }
            $scope.message_confirmation = "Are you sure to delete [" + data.data.Id + "] ";
            angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
        //$rootScope.passValue(_id, $scope.masterindex);
    }
    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        $scope.DeleteMaster();
    };
    $scope.loadDDL();

    // #region Employee Mentor

    $scope.employeeFilterList = [
        {
            'name': 'Employee Code',
            'value': 'EmployeeCode'
        },
        {
            'name': 'First Name',
            'value': 'FirstName'
        },
        {
            'name': 'Middle Name',
            'value': 'MiddleName'
        },
        {
            'name': 'Last Name',
            'value': 'LastName'
        },
        {
            'name': 'Employee Name',
            'value': 'EmployeeName'
        },
        {
            'name': 'Designation',
            'value': 'DesignationName'
        },
        {
            'name': 'Entity',
            'value': 'EntityName'
        },
        {
            'name': 'Department',
            'value': 'Department'
        },
        {
            'name': 'Employment Type',
            'value': 'EmploymentType'
        },
        {
            'name': 'Status',
            'value': 'EmployeeStatus'
        }
    ];

    $scope.employeeParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'EmployeeCode, FirstName, MiddleName, LastName '
        , searchBy: 'EmployeeCode'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };

    $scope.showEmployeeListPopUp = function (flag) {
        $scope.respOrMentor = flag;
        if ($scope.respOrMentor === 'ResponsiblePerson') { $scope.popUpTitle = 'Responsible Person'; }
        else if ($scope.respOrMentor === 'Mentor') { $scope.popUpTitle = 'Mentor'; }
        else if ($scope.respOrMentor === 'AccountHolder') { $scope.popUpTitle = 'Account Holder'; }
        else if ($scope.respOrMentor === 'AccountInCharge') { $scope.popUpTitle = 'Account InCharge'; }
        baseService.setCurrentPage('employeeList');
        $scope.searchEmployeeByList = [];
        $scope.getEmployeeData = function (pageno) {
            $scope.employeeParameters.plantId = $scope.master.PlantId;
            baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                .then(function (result) {
                    $scope.employeeList = result.Rows;
                    $scope.employeeParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#employeePopUp')).modal('show');
        $scope.getEmployeeData();
    };

    $scope.selectEmployeePopUp = function (index, id) {
        $scope.employeeIndex = index;
        $scope.selectedEmployee = id;
    };

    $scope.closeEmployeePopUp = function () {
        if ($scope.employeeIndex !== -1) {
            var employee = $scope.employeeList[$scope.employeeIndex];
            if ($scope.respOrMentor === 'ResponsiblePerson') {
                $scope.mastermodal.ResponsiblePersonId = employee.SystemId;
                $scope.mastermodal.ResponsiblePersonName = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'Mentor') {
                $scope.mastermodal.MentorId = employee.SystemId;
                $scope.mastermodal.MentorName = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'AccountHolder') {
                $scope.mastermodal.AccountHolder = employee.SystemId;
                $scope.mastermodal.AccountHolderName = employee.EmployeeName;
            }
            else if ($scope.respOrMentor === 'AccountInCharge') {
                $scope.mastermodal.AccountInCharge = employee.SystemId;
                $scope.mastermodal.AccountInChargeName = employee.EmployeeName;
            }
        }
        $scope.hideEmployeePopUp();
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUp')).modal('hide');
        $scope.employeeIndex = -1;
        $scope.selectedEmployee = null;
    };

    // #endregion Employee Mentor

    $scope.ProcessId = null;
    // #region Detail Pop
    $scope.details = function (data) {
        $scope.masterId = data.data.Id;
        $scope.entityId = data.data.EntityId;
        $scope.ProcessId = data.data.ProcessId;
        $scope.tempList = [];
        $scope.effectiveDateList = [];
        $scope.budgetCodeList = [];
        $scope.productPriorityList = [];
        $scope.shiftList = [];
        $scope.WCSList = [];
        $scope.SBList = [];
        $scope.WCGList = [];
        $scope.budgetCodeModel = {
            Id: null
            , WorkCenterMasterId: null
            , ManpowerBudgetId: null
            , ManpowerBudgetCode: null
            , EntityCode: null
            , EntityName: null
            , PositionCode: null
            , Position: null
            , NoOfResource: null
        };
        $scope.ModelNewWCG = Object.assign({}, $scope.ModelWCGTemp);
        $http.get($scope.path + 'GetDetalsData?masterId=' + $scope.masterId)
            .then(function (response) {
                $scope.effectiveDateList = response.data.eDate;
                //$scope.effectiveDateList = $filter('orderBy')(response.data.eDate, '-StartDate');
                $scope.budgetCodeList = response.data.bCode;
                $scope.productPriorityList = response.data.priority;
            });
        $scope.GetWorkCenterWiseShiftList();
        $scope.GetWCSSequence();
        angular.element(document.querySelector('#detailPopUp')).modal('show');
    }
    // #endregion

    // #region Effective Date
    $scope.effectiveDateList = [];
    $scope.addNewDate = function () {
        if (baseService.arrayLength($scope.effectiveDateList) > 0) {
            if (baseService.isUndefinedOrNull($scope.effectiveDateList[$scope.effectiveDateList.length - 1].EndDate))
                return ShowResult('Please input previous end date.', 'failure', 'detailPopUp');
            var endDate = new Date($scope.effectiveDateList[$scope.effectiveDateList.length - 1].EndDate);
            endDate.setDate(endDate.getDate() + 1);
            $scope.effectiveDateList.push({
                Id: null
                , WorkCenterMasterId: $scope.masterId
                , StartDate: endDate
                , EndDate: null
                , Hour: null
            });
        }
        else
            $scope.effectiveDateList.push({
                Id: null
                , WorkCenterMasterId: $scope.masterId
                , StartDate: new Date()
                , EndDate: null
                , Hour: null
            });
    }
    // #endregion

    // #region Manpower Budget
    $scope.manpowerBudgetUrl = 'Organizations/ManpowerBudget/GetListByEntity';
    $scope.manpowerBudgetParameters = {
        limit: 10,
        offset: 0,
        order: 'ASC',
        sort: 'Code',
        searchBy: 'Code',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.manpowerBudgetPopUp = function () {
        $scope.manpowerBudgetParameters.searchBy = 'Code';
        $scope.manpowerBudgetDataList = [];
        $scope.manpowerBudgetSearchList = [];
        $scope.manpowerBudgetParameters.entityId = $scope.entityId;
        $scope.getManpowerBudgetData = function (pageno) {
            baseService.paginationBase($scope.manpowerBudgetUrl, pageno, $scope.manpowerBudgetParameters)
                .then(function (response) {
                    $scope.manpowerBudgetDataList = response.Rows;
                    $scope.manpowerBudgetParameters.total_count = response.Total;
                    if (baseService.arrayLength($scope.manpowerBudgetSearchList) === 0)
                        baseService.getDDLSearchColumn($scope.manpowerBudgetDataList, $scope.manpowerBudgetSearchList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('show');
        $scope.getManpowerBudgetData();
    };
    $scope.closeManpowerBudgetPopUp = function () {
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };
    $scope.selectManpowerBudgetPopUp = function (data) {
        for (var i = 0; i < baseService.arrayLength($scope.budgetCodeList); i++) {
            if ($scope.budgetCodeList[i].ManpowerBudgetId === data.Id)
                return ShowResult('This ' + data.Code + ' already exist.', 'failure', 'manpowerBudgetPopUp');
        }
        $scope.budgetCodeModel = {
            Id: null
            , WorkCenterMasterId: $scope.masterId
            , ManpowerBudgetId: data.Id
            , ManpowerBudgetCode: data.Code
            , Male: data.BudgetedMale
            , Female: data.BudgetedFemale
            , TotalManpower: data.BudgetedTotal
            , EntityCode: data.EntityCode
            , EntityName: data.Entity
            , PositionCode: data.PositionCode
            , Position: data.Position

        };
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };
    $scope.manpowerBudgetAdd = function () {
        if (baseService.isUndefinedOrNull($scope.budgetCodeModel.ManpowerBudgetId))
            return ShowResult('Please select manpower budget.', 'failure', 'detailPopUp');
        if (baseService.isUndefinedOrNull($scope.budgetCodeModel.NoOfResource) || parseInt($scope.budgetCodeModel.NoOfResource) === 0)
            return ShowResult('Please input no of resource.', 'failure', 'detailPopUp');
        $scope.budgetCodeList.push({
            Id: null
            , WorkCenterMasterId: $scope.masterId
            , ManpowerBudgetId: $scope.budgetCodeModel.ManpowerBudgetId
            , ManpowerBudgetCode: $scope.budgetCodeModel.ManpowerBudgetCode
            , NoOfResource: $scope.budgetCodeModel.NoOfResource
            , EntityCode: $scope.budgetCodeModel.EntityCode
            , EntityName: $scope.budgetCodeModel.EntityName
            , PositionCode: $scope.budgetCodeModel.PositionCode
            , Position: $scope.budgetCodeModel.Position
        });
        $scope.clearManPowereBudget();
        angular.element(document.querySelector('#manpowerBudgetPopUp')).modal('hide');
    };
    $scope.clearManPowereBudget = function () {
        $scope.budgetCodeModel = {
            Id: null
            , WorkCenterMasterId: null
            , ManpowerBudgetId: null
            , ManpowerBudgetCode: null
            , Male: null
            , Female: null
            , TotalManpower: null
            , NoOfResource: null
        };
    }
    // #endregion

    // #region Product
    $scope.productPriorityList = [];
    $scope.searchProdList = [
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Product Category',
            'value': 'ProductCategory'
        },
        {
            'name': 'Product SubCategory',
            'value': 'ProductSubCategory'
        }
    ];
    $scope.productPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'UserName',
        searchBy: 'UserName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.productPopUp = function () {
        $scope.productPopUpList = [];
        //baseService.setCurrentPage('dataList');
        //$scope.productPopUpParameters.ids = baseService.getColumnValueList($scope.productPriorityList, 'MaterialMasterId');
        $scope.productPopUpParameters.sort = 'UserName';
        $scope.productPopUpParameters.searchBy = 'UserName';
        $scope.getProductPopUpData = function (pageno) {
            //baseService.paginationBase($scope.path + 'GetMaterialMasterList', pageno, $scope.productPopUpParameters)
            //baseService.paginationBase('OrderManagements/commitment/GetProductMasterList', pageno, $scope.productPopUpParameters)
            baseService.paginationBase($scope.path + 'GetProductMasterList', pageno, $scope.productPopUpParameters)
                .then(function (result) {
                    $scope.productPopUpDataList = result.Rows;
                    $scope.productPopUpParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.productPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.productPopUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'productPopUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#productPopUpId')).modal('show');
        $scope.getProductPopUpData();
    }
    $scope.selectProduct = function (data) {
        data.Priority = $scope.productPriorityList.length + 1;
        $scope.productPriorityList.push(data);
        $scope.closeProductPopUp();
    }
    $scope.closeProductPopUp = function () {
        angular.element(document.querySelector('#productPopUpId')).modal('hide');
    }
    // #endregion

    // #region Shift
    $scope.shiftList = [];
    $scope.searchShiftList = [
        {
            'name': 'Shift Name',
            'value': 'ShiftDefinationName'
        },
        {
            'name': 'Description',
            'value': 'ShiftDefinationDescription'
        },
        {
            'name': 'Shift Type',
            'value': 'ShiftType'
        }
    ];
    $scope.ShiftPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'ShiftDefinationName',
        searchBy: 'ShiftDefinationName',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.ShiftPopUp = function () {
        $scope.ShiftPopUpList = [];
        $scope.ShiftPopUpParameters.sort = 'ShiftDefinationName';
        $scope.ShiftPopUpParameters.searchBy = 'ShiftDefinationName';
        $scope.getShiftPopUpData = function (pageno) {

            baseService.paginationBase($scope.path + 'GetShiftList?ShiftDefinationIDs=' + isShiftDefinationIDExistGrid($scope.selectedShiftList), pageno, $scope.ShiftPopUpParameters)
                .then(function (result) {
                    $scope.ShiftPopUpDataList = result.Rows;
                    $scope.ShiftPopUpParameters.total_count = result.Total;

                    for (var t = 0; t < baseService.arrayLength($scope.ShiftPopUpDataList); t++) {
                        $scope.ShiftPopUpDataList[t].Flag = baseService.valueCheckInList($scope.tempList, 'ShiftDefinationID', $scope.ShiftPopUpDataList[t].ShiftDefinationID);
                    }

                    if (baseService.arrayLength($scope.ShiftPopUpList) == 0)
                        baseService.getDDLSearchColumn(result.Rows, $scope.ShiftPopUpList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'ShiftPopUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#ShiftPopUpId')).modal('show');
        $scope.getShiftPopUpData();
    }

    function isShiftDefinationIDExistGrid(list) {
        $scope.ShiftDefinationIDs = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                $scope.ShiftDefinationIDs.push(list[i]['ShiftDefinationID']);
            }
        }
        return JSON.stringify($scope.ShiftDefinationIDs);
    }

    $scope.tempList = [];
    $scope.pushInTempList = function (event, data) {
        try {
            if (event.currentTarget.checked) {
                if (checkExistTempList($scope.tempList, data.ShiftDefinationID) === false) {
                    $scope.tempList.push(data);
                }
                else {
                    for (var i = 0; i < baseService.arrayLength($scope.tempList); i++) {
                        if ($scope.tempList[i].ShiftDefinationID === data.ShiftDefinationID) {
                            $scope.tempList.splice(i, 1);
                            break;
                        }
                    }

                    $scope.tempList.push(data);
                }
            }
            else {
                for (var t = 0; t < baseService.arrayLength($scope.tempList); t++) {
                    if ($scope.tempList[t].ShiftDefinationID === data.ShiftDefinationID) {
                        $scope.tempList.splice(t, 1);
                        break;
                    }
                }
            }
        } catch (e) {
            event.currentTarget.checked = false;
            ShowResult(e, "failure");
        }
    }

    function checkExistTempList(list, id) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].ShiftDefinationID === id) {
                return true;
            }
        }
        return false;
    }
    $scope.selectedShiftList = [];
    $scope.closePopUp = function () {
        if (baseService.arrayLength($scope.tempList) > 0) {
            angular.forEach($scope.tempList, function (item) {
                $scope.selectedShiftList.push({
                    Id: null
                    , WorkCenterMasterId: $scope.masterId
                    , ShiftDefinationID: item.ShiftDefinationID
                    , ShiftDefinationName: item.ShiftDefinationName
                    , ShiftDefinationDescription: item.ShiftDefinationDescription
                    , ShiftType: item.ShiftType
                    , InTime: item.InTime
                    , OutTime: item.OutTime
                    , ProductionHours: item.ProductionHours
                });
            });
        }
        angular.element(document.querySelector('#ShiftPopUpId')).modal('hide');
    };

    $scope.closeShiftPopUp = function () {
        angular.element(document.querySelector('#ShiftPopUpId')).modal('hide');
    }

    $scope.GetWorkCenterWiseShiftList = function () {
        $http({
            method: 'GET',
            url: 'WorkCenters/WorkCenterMaster/GetWorkCenterWiseShiftList?workCenterMasterId=' + $scope.masterId
        }).then(function successCallback(response) {
            $scope.selectedShiftList = response.data;
        });
        $scope.GetWorkCenterMasterSubProcessList();
    };
    $scope.cIndex = -1;
    $scope.valuePassInDelModal = function (index, data) {
        $scope.id = data.Id;
        $scope.cIndex = index;
        $scope.message_confirmation = 'Are you sure want to permanently delete [ ' + data.ShiftDefinationName + ' ]';
        angular.element(document.querySelector('#confirmgenericPopUp')).modal('show');
    };

    $scope.DeleteShift = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST',
                url: 'WorkCenters/WorkCenterMaster/DeleteShift?id=' + $scope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.selectedShiftList.splice($scope.cIndex, 1);
                    $scope.cIndex = -1;
                    $scope.GetWorkCenterWiseShiftList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } else {
            $scope.selectedShiftList.splice($scope.cIndex, 1);
            $scope.cIndex = -1;
        }
    };

    // #endregion

    // #region SubProcess

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.companySubProcessList = [];
    $scope.SubProcessPopUp = function () {

        $scope.popUpUrl = 'WorkCenters/WorkCenterMaster/GetListForSubProcess/?processId=' + $scope.ProcessId + '&WorkCenterMasterId=' + $scope.masterId + '&subProcessIds=' + isProcessIdExistGrid($scope.companySubProcessList);
        $scope.getCompanySubProcessData = function (pageno) {
            $rootScope.parameters.processId = $scope.ProcessId;
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.subProcesses = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });

            angular.element(document.querySelector('#subProcessPopUp')).modal('show');
        };
        $scope.getCompanySubProcessData();
    };
    function isProcessIdExistGrid(list) {
        $scope.ProcessIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] == false) {
                    $scope.ProcessIds.push(list[i]['SubProcessId']);
                }
            }
        }
        return JSON.stringify($scope.ProcessIds);
    }
    $scope.searchSubProcessByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        }
    ];
    $scope.closeSubProcessListPopUp = function () {
        angular.element(document.querySelector('#subProcessPopUp')).modal('hide');
    };

    $scope.tempList = [];
    $scope.tempArchiveList = [];

    $scope.SubProcessList = [];
    $scope.addSubProcess = function () {
        angular.forEach($scope.subProcesses, function (a) {
            if (a.Flag) {
                $scope.SubProcessList.push({
                    Id: null,
                    Code: a.Code,
                    SubProcessId: a.Id,
                    SubProcessName: a.UserName,
                    SubProcessCategoryName: a.SubProcessCategoryName,
                    ProcessId: $scope.ProcessId,
                    WorkCenterMasterId: $scope.masterId,
                    Archive: false,
                    class: 'new'
                });
            }
        });
        if (!$scope.tableShow)
            $scope.tableShow = true;
        angular.element(document.querySelector('#subProcessPopUp')).modal('hide');
    }

    $scope.GetWorkCenterMasterSubProcessList = function () {
        $http({
            method: 'GET',
            url: 'WorkCenters/WorkCenterMaster/GetWorkCenterMasterSubProcessList?workCenterMasterId=' + $scope.masterId
        }).then(function successCallback(response) {
            $scope.SubProcessList = response.data;
        });
        $scope.GetWCSList();
    };

    $scope.cIndex = -1;
    $scope.valuePassInSPDelModal = function (index, data) {
        $scope.id = data.Id;
        $scope.cIndex = index;
        $scope.message_confirmation = 'Are you sure want to permanently delete [ ' + data.SubProcessName + ' ]';
        angular.element(document.querySelector('#confirmgenericSPPopUp')).modal('show');
    };

    $scope.DeleteSP = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST',
                url: 'WorkCenters/WorkCenterMaster/DeleteSP?id=' + $scope.id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SubProcessList.splice($scope.cIndex, 1);
                    $scope.cIndex = -1;
                    $scope.GetWorkCenterMasterSubProcessList();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } else {
            $scope.SubProcessList.splice($scope.cIndex, 1);
            $scope.cIndex = -1;
        }
    };


    // #endregion

    $scope.detailSave = function () {
        try {
            //if (baseService.arrayLength($scope.effectiveDateList) == 0 || baseService.arrayLength($scope.budgetCodeList) == 0 || baseService.arrayLength($scope.productPriorityList) == 0)
            //    throw 'Can not save without data.';
            if (baseService.arrayLength($scope.effectiveDateList) == 0)
                throw 'Can not save without data.';
            for (var i = 0; i < baseService.arrayLength($scope.budgetCodeList); i++) {
                if (baseService.isUndefinedOrNull($scope.budgetCodeList[i].NoOfResource) || $scope.budgetCodeList[i].NoOfResource === 0)
                    throw 'No of resource required.';
            }
            for (var t = 0; t < baseService.arrayLength($scope.effectiveDateList); t++) {
                var row = $scope.effectiveDateList[t];
                if (t != 0) {//compare between previous end date and current start date
                    var previousRow = $scope.effectiveDateList[t - 1];
                    if (new Date(row.StartDate) < new Date(previousRow.EndDate)) throw 'Start date ' + row.StartDate + ' must be greater than end date ' + previousRow.EndDate;
                }

                if (t !== $scope.effectiveDateList.length - 1) //end date can be null when new entry
                    if (new Date(row.StartDate) > new Date(row.EndDate)) throw 'Start date ' + row.StartDate + ' must be less than end date ' + row.EndDate;
                if (baseService.isUndefinedOrNull(row.Hour)) {
                    throw 'Hour is required.';
                }
            }
            $http({
                method: 'POST',
                url: $scope.path + 'detailSave',
                dataType: 'JSON',
                data: {
                    'masterId': $scope.masterId
                    , 'effectiveDateList': $scope.effectiveDateList
                    , 'budgetCodeList': $scope.budgetCodeList
                    , 'productPriorityList': $scope.productPriorityList
                    , 'shiftList': $scope.selectedShiftList
                    , 'subProcessList': $scope.SubProcessList
                }
            }).then(function (response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure', 'detailPopUp');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.masterId = null;
                    $scope.effectiveDateList = [];
                    $scope.budgetCodeList = [];
                    $scope.productPriorityList = [];
                    $scope.selectedShiftList = [];
                    angular.element(document.querySelector('#detailPopUp')).modal('hide');
                }
            }, function (response) {
                ShowResult(response.status.Message, 'failure', 'detailPopUp');
            });
        } catch (e) {
            ShowResult(e, '', 'detailPopUp')
        }
    }
    $scope.removeRowModal = function (ob, index, list) {
        try {
            $scope.list = list;
            $scope.removeIndex = index;
            if ($scope.list === "productPriorityList") {
                $scope.message_confirmation = "Are you sure want to delete [" + ob.UserName + "] ";
            }
            else if ($scope.list === "effectiveDateList") {
                $scope.message_confirmation = "Are you sure want to delete [" + ob.StartDate + "" + 'to' + "" + ob.EndDate + "] ";
            } else {
                $scope.message_confirmation = "Are you sure want to delete [" + ob.ManpowerBudgetCode + "] ";
            }
            angular.element(document.querySelector('#confirmRowRemove')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'falure', 'detailPopUp');
        }
    }
    $scope.removeRow = function () {
        $scope[$scope.list].splice($scope.removeIndex, 1);
        $scope.list = null;
        $scope.removeIndex = -1;
        angular.element(document.querySelector('#confirmRowRemove')).modal('hide');
    };

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        MachineAppicable: false,
        MachineName: null,
        ArticleId: null,
        SkillLevel: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetWCSSequence = function () {
        cboService.getSequence('WorkCenters/WorkCenterMaster/GetWCSAutoSequence?WorkCenterMasterId=' + $scope.masterId, function (data) {
            $scope.ModelNew.Sequence = data;
        });
    };

    $scope.WCSList = [];
    $scope.GetWCSList = function () {
        $scope.WCSList = [];
        $http({
            method: 'GET',
            url: 'WorkCenters/WorkCenterMaster/GetWCSkill?WorkCenterMasterId=' + $scope.masterId
        }).then(function successCallback(response) {
            $scope.WCSList = response.data;
            $scope.GetSBList();
        });
    }

    $scope.SkillList = [];
    $scope.GetCboSkillCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/MachineMasterUI/GetCboSkill'
        }).then(function successCallback(response) {
            $scope.SkillList = response.data;
        });
    }
    $scope.GetCboSkillCbo();

    $scope.SkillGroupingList = [];
    $scope.GetCboSkillGroupingCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/SkillGrouping/GetSkillgrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();

    $scope.ActivityList = [];
    $scope.GetActivityCbo = function () {
        $http({
            method: 'GET',
            url: 'Accounts/Activity/GetCbo'
        }).then(function successCallback(response) {
            $scope.ActivityList = response.data;
        });
    }
    $scope.GetActivityCbo();

    $scope.GetWSC = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
    };

    $scope.articleList = [];
    $scope.articleParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'StandardName'
        , searchBy: "StandardName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.articlePopUp = function () {
        try {

            $scope.articleDataList = [];
            $scope.articleUrl = 'WorkCenters/WorkCenterMaster/GetMachine';
            baseService.setCurrentPage('dataList');
            $scope.getarticleData = function (pageno) {
                baseService.paginationBase($scope.articleUrl, pageno, $scope.articleParameters)
                    .then(function (result) {
                        $scope.articleDataList = result.Rows;
                        $scope.articleParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.articleList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.articleList);
                        }
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'articleId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#articleId')).modal('show');
            $scope.getarticleData();
        } catch (e) {
            ShowResult(e, '', 'materialId');
        }

    };

    $scope.selectArticle = function (data) {
        $scope.ModelNew.ArticleId = data.Id;
        $scope.ModelNew.MachineName = data.StandardName;
        $scope.closeArticle();
    };
    $scope.closeArticle = function () {
        angular.element(document.querySelector('#articleId')).modal('hide');
    };



    $scope.SaveWCS = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $scope.ModelNew.WorkCenterMasterId = $scope.masterId;
            $http({
                method: 'POST',
                url: 'WorkCenters/WorkCenterMaster/CreateWCSkill',
                data: { 'data': $scope.ModelNew, 'WorkCenterMasterId': $scope.ModelNew.WorkCenterMasterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearwcsFields(response.data.Sequence);
                    $scope.GetWCSList();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Clearwcs = function () {
        ClearwcsFields($scope.GetWCSSequence());
        return true;
    };

    function ClearwcsFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }

    $scope.SBModel = {
        Id:null,
        WorkCenterMasterId: null,
        SkillMasterId: null,
        RequiredManPower: 0,
        AllotedManpower: 0,
        Remarks:null
    }
    $scope.SBModelNew = Object.assign({}, $scope.SBModel);


    $scope.OperationMasterList = [];
    $scope.showOperationPopUp = function () {
        $scope.OperationMasterList = [];
        $scope.Operation = "Operation Master";
        $http.get('employees/EmployeeInformation/GetOperationMaster')
            .then(function (response) {
                $scope.OperationMasterList = response.data;
            });

        angular.element(document.querySelector('#OperationPopUp')).modal('show');
    };

    $scope.SetOperation = function (args) {
        var gridObj = $("#GridOP").data("ejGrid");
        $scope.data = gridObj.getSelectedRecords()[0];
        $scope.SBModelNew.SkillMasterId = $scope.data.Id;
        $scope.SBModelNew.SkillName = $scope.data.UserName;
        angular.element(document.querySelector('#OperationPopUp')).modal('hide');
    }


    $scope.SaveSB = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SBModelNewForm.$valid) {
            $scope.SBModelNew.WorkCenterMasterId = $scope.masterId;
            $http({
                method: 'POST',
                url: 'WorkCenters/WorkCenterMaster/CreateSB',
                data: { 'data': $scope.SBModelNew, 'WorkCenterMasterId': $scope.masterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearSBFields();
                    $scope.GetSBList();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.SBList = [];
    $scope.GetSBList = function () {
        $scope.SBList = [];
        $http({
            method: 'GET',
            url: 'WorkCenters/WorkCenterMaster/GetWCSkillBudget?WorkCenterMasterId=' + $scope.masterId
        }).then(function successCallback(response) {
            $scope.SBList = response.data;
            $scope.GetWCGList();
        });
    }

    $scope.GetSB = function (args) {
        $scope.SBModelNew = Object.assign({}, args.data);
        $scope.SBAction = 'Update';
    };

    $scope.ClearSB = function () {
        ClearSBFields();
        return true;
    };
    $scope.SBAction = 'Save';
    function ClearSBFields() {
        $scope.SBAction = 'Save';
        $scope.SBModel = {
            Id: null,
            WorkCenterMasterId: null,
            SkillMasterId: null,
            RequiredManPower: 0,
            AllotedManpower: 0,
            Remarks: null
        }
        $scope.SBModelNew = Object.assign({}, $scope.SBModel);

    }

    //#region WorkCenterGroup

    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.WCGList = [];
    $scope.GetWCGroupPopUp = function () {
        $http({
            method: 'POST',
            url: "WorkCenters/WorkCenterMaster/GetWCGroup",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.WCGList = response.data;
        });

        angular.element(document.querySelector('#WCGroupPopUp')).modal('show');
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWise });
    };

    function CheckBoxSelectAllWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridQCG").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.WCGList.length; i++) {
                $scope.WCGList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridQCG").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.WCGwiseList = [];
    function MakeData() {

        for (var i = 0; i < $scope.WCGList.length; i++) {
            if ($scope.WCGList[i].Flag == true) {
                if (checkExists($scope.WCGwiseList, $scope.WCGList[i].Id) === false) {
                    var ob = {};
                    ob.Id = -(Math.floor(Math.random() * 100) + 1);
                    ob.WorkCenterGroupId = $scope.WCGList[i].Id;
                    ob.WorkCenterMasterId = $scope.masterId;
                    ob.Sequence = $scope.WCGList[i].Sequence;
                    ob.Code = $scope.WCGList[i].Code;
                    ob.ShortName = $scope.WCGList[i].ShortName;
                    ob.StandardName = $scope.WCGList[i].StandardName;
                    ob.UserName = $scope.WCGList[i].UserName;

                    $scope.WCGwiseList.push(ob);
                }
                else {
                    throw "This Work Center Group " + $scope.WCGList[i].UserName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].WorkCenterGroupId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseWCG = function () {
        try {
            MakeData();
            $scope.SaveWCG();
            angular.element(document.querySelector('#WCGroupPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveWCG = function () {
        try {
           
            $http({
                method: 'POST',
                url: 'WorkCenters/WorkCenterMaster/SaveWorkCenterWiseGroup',
                data: { 'data': $scope.WCGwiseList, 'masterId': $scope.masterId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');                   
                    $scope.GetWCGList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.WCGwiseList = [];
    $scope.GetWCGList = function () {
        $scope.WCGwiseList = [];
        $http({
            method: 'GET',
            url: 'WorkCenters/WorkCenterMaster/GetWCWGroup?WorkCenterMasterId=' + $scope.masterId
        }).then(function successCallback(response) {
            $scope.WCGwiseList = response.data;
        });
    }
    

    //#endregion
};