'use strict';
function ProductionOrderSubprocessController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    ///----------------------------------------------------------------------------------------------------------------------
    ///1.declaration
    ///2.function
    ///3.loadtime call
    ///
    ///------------------------------------------------------------------------SubsectionStructure-----------------------------------------------
    ///1.declaration----------------------------------------------------------------------------------------------------------
    ///variable
    $rootScope.title = "Production Order Subprocess";
    $scope.Action = 'Save';
    $scope.rowCount = 10;
    $scope.maxRow = 2;
    $scope.ActionDetail = 'Save';
    $scope.btndeletemaster = false;
    $scope.SaveDetailDisabled = false;
    $scope.ShowbtnSave = false;
    $scope.DisableRecipeddl = false;
    $scope.ShowRecipeSearch = false;
    $scope.ShowPOGrid = false;
    //$scope.FirstInputDate = false;

    $scope.message_confirmation = "";
    $scope.WorkCenterName = "";

    $scope.path = 'OrderManagements/productionOrdersubprocess/';
    $scope.deleteUrl = $scope.path + 'deletedetail?id=';
    $scope.sbBmaster = [];
    $scope.searchbyCharacteristicsValuelist = [];
    $scope.characteristicsValueData = [];
    $scope.sbproductionBatchMasterList = [];
    $scope.productionBatchMasterList = [];
    $scope.subprocessSetlist = [];
    $scope.sbSubprocessSetlist = [];
    $scope.masterList = [];
    $scope.detailList = [];
    $scope.detailChildList = [];
    $scope.entityList = [];

    $scope.Data = [];
    $scope.detailmodal = {
        Id: null,
        CycleTime: null,
        AvgWeight: null,
        LoadFactor: null,
        Qty: null,
        EntityId: null,
        WeightUomId: null,
        ProcessCriteriaId: null,
        ProductionBatchMasterId: null,
        ProcessId: null,
        Characteristics1Value: null,
        Characteristics1ValueId: null,
        Characteristics2Value: null,
        Characteristics2ValueId: null,
        Characteristics3Value: null,
        Characteristics3ValueId: null,
        SelectedCharacteristics: null,
        Characteristics1Selected: null,
        Characteristics2Selected: null,
        Characteristics3Selected: null,
        Characteristics1Id: null,
        Characteristics2Id: null,
        Characteristics3Id: null,
        Characteristics1: null,
        Characteristics2: null,
        Characteristics3: null,
        ProcessTypeId: null
    };
    //$scope.detailmodal = {
    //    Id: null,
    //    CycleTime: null,
    //    AvgWeight: null,
    //    LoadFactor: null,
    //    Qty: null,
    //    WeightUomId: null,
    //    ProcessCriteriaId: null,
    //    MasterId: null
    //};
    $scope.pref = {
        BatchNo: null,
        BatchDate: null,
        FileNo: null,
        Buyer: null,
        Customer: null,
        ProductionOrder: null,
        MaterialMaster: null,
        ProcessType: null,
        ProcessTypeId: null,
        Qty: null,
        MaterialMasterId: null
    };
    $scope.master = {
        Id: null,
        EntityId: null,
        Process: null,
        ProcessId: null
    };

    $scope.productionBatchSubprocessSet = {
        Id: null,
        SubProcessSetDetailId: null,
        ProductionBatchProcessCriteriaId: null,
        CompanyGroupId: null,
        CompanyId: null,
        EntityId: null,
        ProductionBatchMasterId: null,
        ProcessId: null,
        ProcessTypeId: null,
        SubProcessSetId: null
    }

    $scope.weightUomList = [];
    function LoadweightUom(materialMasterId) {
        $http.get("Processes/ProcessCriteria/GetWeightUomCbo?materialmasterid=" + materialMasterId)
            .then(function (response) {
                $scope.weightUomList = response.data;
                //console.log('$scope.weightUomList', $scope.weightUomList);
            });
    }

    $scope.masterIndex = -1;

    $http({
        method: 'GET',
        url: 'Processes/processset/enumjobworktypelistcbo'
    }).then(function successCallback(response) {
        $scope.jobWorkTypeList = response.data;
    });

    ///declaration ends-----------------------------------------------------------------------------------------------------
    ///2.function----------------------------------------------------------------------------------------------------

    ///**************************************************get data from database*********************************
    function ClearOb(obj) {
        for (var i in obj) {
            obj[i] = null;
        }
    }
    function createguid(prefix) {
        var d = new Date().getTime();
        d += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = 'uid-';
        }
        d = prefix + d;
        return d;
    };
    function loadDDL(pomid) {
        $scope.recipeList = [];
        $scope.productionstatusList = [];
        //'Materials/materialmaster/materialmastersearch',
        //$http.get("OrderManagements/recipemaster/getrecipebypocbo?pomid=" + pomid )
        //      .then(function (response) {
        //          $scope.recipeList = response.data;
        //      });

        $http.get("Materials/materialmaster/materialmastercbo?materialmasterid=" + pomid)
            .then(function (response) {
                $scope.recipeList = response.data;
                //console.log($scope.recipeList);
            });
        $http.get("OrderManagements/productionstatus/getcbo")
            .then(function (response) {
                //console.log(response);
                $scope.productionstatusList = response.data;
            });
    };

    $scope.getData = function () {
        baseService.init($scope.path + 'loadbatchlist', null, $scope.rowCount, null, 'BatchNo', 'Lsd');
        $scope.loadBatchMasterData = function (pageno) {
            $rootScope.parameters.mmid = $scope.master.MaterialMasterId;
            $rootScope.parameters.pomid = $scope.pomaster.ProductionOrderMasterId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    if (arrayLength($scope.sbBmaster) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbBmaster);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadBatchMasterData();
    }

    function LodaDetail(processid, processtypeid, batchid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getlist?processid=' + processid + '&processtypeid=' + processtypeid + '&batchid=' + batchid,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.detailList = response.data;
                //console.log("$scope.detailList", $scope.detailList);
            }
        })//success
    }
    function loadBatch(entityid, processid) {
        baseService.setCurrentPage('productionBatchMasterList');
        baseService.init("OrderManagements/productionOrder/loadbatch/", null, $scope.maxRow, null, 'BatchNo', 'BatchNo');
        $scope.loadBatchData = function (pageno) {
            $rootScope.parameters.entityid = entityid;
            $rootScope.parameters.processid = processid;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.productionBatchMasterList = [];
                    $scope.productionBatchMasterList = result.Rows;
                    //console.log('227', $scope.productionBatchMasterList);
                    if (baseService.arrayLength($scope.sbproductionBatchMasterList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbproductionBatchMasterList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadBatchData();
    }
    function loadBatchEdit(entityid, processid) {
        baseService.setCurrentPage('productionBatchMasterList');
        baseService.init("OrderManagements/productionOrder/loadbatchedit/", null, 25, null, 'BatchNo', 'BatchNo');
        $scope.loadBatchData = function (pageno) {
            $rootScope.parameters.entityid = entityid;
            $rootScope.parameters.processid = processid;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.productionBatchMasterList = [];
                    $scope.productionBatchMasterList = result.Rows;
                    //console.log('227', $scope.productionBatchMasterList);
                    if (baseService.arrayLength($scope.sbproductionBatchMasterList) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbproductionBatchMasterList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadBatchData();
    }

    function getQty() {
        if (baseService.isUndefinedOrNull($scope.detailmodal.Qty)) {
            return 0;
        }
        else {
            return $scope.detailmodal.Qty;
        }
    }

    $scope.countOrder = function () {
        var total = 0;
        for (var i = 0; i < $scope.detailList.length; i++) {
            if ($scope.detailList[i].Id !== $scope.detailmodal.Id) {
                total += $scope.detailList[i].Qty;
            }
        }
        total = $scope.pref.Qty - total - getQty();
        if (total < 0) {
            //var result = $scope.pref.Qty - total;
            $scope.detailmodal.Qty = 0;
            total = $scope.pref.Qty - total;
        }
        return total;
    };

    function SetSelectedInSearchList(searchlist, selectedlsit) {
        for (var i = 0; i < baseService.arrayLength(selectedlsit); i++) {
            SetSelectedByID(selectedlsit[i].ProductionOrderMaterialMasterId, searchlist);
        }
    }
    function SetSelectedByID(id, searchlist) {
        for (var i = 0; i < baseService.arrayLength(searchlist); i++) {
            if (searchlist[i].ProductionOrderMaterialMasterId === id) {
                searchlist[i].IsSelectedID = true;
                break;
            }
        }
    }
    function setSelected(uomid) {
        for (var i = 0; i < arrayLength($scope.machineList); i++) {
            if ($scope.machineList[i].Id === uomid) {
                $scope.machineList[i].IsSelectedID = true;
                break;
            }
        }
    }
    function AddnewOnly(mmid, pomid) {
        try {
            if (baseService.isUndefinedOrNull(pomid)) {
                throw "Select Production Order...";
            }
            if (baseService.isUndefinedOrNull(mmid)) {
                throw "Select Material...";
            }

            $scope.master.ProductionOrderMasterId = pomid;
            $scope.master.MaterialMasterId = mmid;
            $scope.master.BuyerId = $scope.pomaster.BuyerId;
            $scope.detailList = [];
            $scope.DisableRecipeddl = true;
            $scope.ShowPOGrid = true;
            $scope.ShowbtnSave = true;//
            //console.log('new', $scope.master);
            //console.log($scope.pomaster);
            ///get LSD Setting
            getLSD($scope.pomaster.BuyerId);
        } catch (e) {
            throw e;
        }
    }

    function GetId(obj, list) {
        try {
            var _id = '';
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                if (obj.MaterialMasterId === list[i].MaterialMasterId && obj.ProductionOrderCharacteristicsValue1stId === list[i].ProductionOrderCharacteristicsValue1stId && obj.ProductionOrderCharacteristicsValue2ndId === list[i].ProductionOrderCharacteristicsValue2ndId) {
                    _id = list[i].Id;
                    break;
                }
            }
            return _id;
        } catch (e) {
            throw e;
        }
    }
    $scope.getMasterList = function (plantid) {
        $http.get($scope.path + "getlist?plantid=" + plantid)
            .then(function (response) {
                $scope.masterList = [];
                $scope.masterList = response.data;
                if (arrayLength($scope.searchbyMasterlist) === 0) {
                    baseService.getDDLSearchColumn(response.data, $scope.searchbyMasterlist);
                }
            });
    }
    function LoadProcessType() {
        $http.get("Processes/processtype/getcbo")
            .then(function (response) {
                //console.log(response);
                $scope.processTypeList = response.data;
            });
    }
    $scope.showthis = function (id) {
        var val = baseService.isUndefinedOrNull(id);
        return !val;
    }
    $scope.MMLevel = false;
    $scope.showRecipePopup = function (ob, index) {
        try {
            if (baseService.isUndefinedOrNull(ob.Characteristics1ValueId) && baseService.isUndefinedOrNull(ob.Characteristics2ValueId) && baseService.isUndefinedOrNull(ob.Characteristics3ValueId)) {
                //throw "All Characteristics can not be blank...";
                $scope.MMLevel = true;
            }
            //ClearOb($scope.master);
            console.log('ob', ob)
            $scope.master = ob;
            $scope.masterIndex = index;
            $scope.loadRecipe(ob);
            angular.element(document.querySelector('#recipepopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }

    ///**************************************************grid row selected event function*********************************
    $scope.SearchBatch = function () {
        getLSD($scope.pomaster.BuyerId);
        $scope.getData();

        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    }
    $scope.CheckAllRecipe = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < arrayLength($scope.recipeData); i++) {
            $scope.recipeData[i].IsSelectedID = _isselected;
        }
    }
    $scope.CheckAll = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < arrayLength($scope.obsearchList); i++) {
            $scope.obsearchList[i].IsSelectedID = _isselected;
        }
    }
    $scope.GetMasterIndex = function (id) {
        try {
            ClearBody();
            $scope.master.Id = id;
            GetMasterRow(id);
            getOrderBreakDown('EDIT', $scope.master.RecipeMasterId, $scope.pomaster.ProductionOrderMasterId);
            GetWorkcenter(id);
            angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    function GetTotalDetailQty(list) {
        try {
            var _totalqty = 0;
            for (var i = 0; i < baseService.arrayLength(list); i++) {
                if (list[i].Archive === false) {
                    _totalqty += list[i].Qty;
                }
            }
            $scope.master.Qty = _totalqty;
        } catch (e) {
            throw e;
        }
    }
    function GetSelectedOB(search_list, selectedgrid) {
        var _LSD = new Date();
        var isfirst = true;
        var _CD = new Date();
        //console.log('sear',search_list);
        //console.log('selected',selectedgrid);
        for (var i = 0; i < baseService.arrayLength(search_list); i++) {
            if (search_list[i].IsSelectedID) {
                if (IsAvailableInDetail(search_list[i], selectedgrid) === false) {
                    //console.log('00',search_list[i]);
                    GetLsd(search_list[i], $scope.lsdList);
                    selectedgrid.push(search_list[i]);
                    //_totalqty += search_list[i].Qty;
                    var dd1 = new Date(search_list[i].Lsd);
                    var cd1 = new Date(search_list[i].CommitmentDate);

                    if (isfirst) {
                        isfirst = false
                        _LSD = dd1
                        _CD = cd1;
                    }

                    if (_LSD > dd1) {
                        _LSD = dd1;
                    }

                    if (_CD < cd1) {
                        _CD = cd1;
                    }
                }//dupli
            }//selected
        }//for

        var _tlsd = $filter('dateFiltering')(_LSD, 'dd-MMM-yy');
        var _tcd = $filter('dateFiltering')(_CD, 'dd-MMM-yy');
        $scope.master.TargetCommitmentDate = _tcd;
        $scope.master.TargetLsd = _tlsd;
        GetTotalDetailQty(selectedgrid);
    }
    function SetSelectedOBEdit(selectedgrid) {
        var _totalqty = 0;
        var _LSD = new Date();
        var isfirst = true;
        var _CD = new Date();
        //console.log('sear',search_list);
        //console.log('selected',selectedgrid);
        for (var i = 0; i < baseService.arrayLength(selectedgrid); i++) {
            GetLsd(selectedgrid[i], $scope.lsdList);
            //selectedgrid.push(search_list[i]);
            _totalqty += selectedgrid[i].Qty;
            var dd1 = new Date(selectedgrid[i].Lsd);
            var cd1 = new Date(selectedgrid[i].CommitmentDate);

            if (isfirst) {
                isfirst = false
                _LSD = dd1
                _CD = cd1;
            }

            if (_LSD > dd1) {
                _LSD = dd1;
            }

            if (_CD < cd1) {
                _CD = cd1;
            }
        }//for
        var _tlsd = $filter('dateFiltering')(_LSD, 'dd-MMM-yy');
        var _tcd = $filter('dateFiltering')(_CD, 'dd-MMM-yy');
        $scope.master.TargetCommitmentDate = _tcd;
        $scope.master.TargetLsd = _tlsd;
        $scope.master.Qty = _totalqty;
    }
    function GetLsd(ob, lsdlist) {
        // console.log('11',ob);
        // console.log('22',lsdlist);

        for (var i = 0; i < baseService.arrayLength(lsdlist); i++) {
            // if (lsdlist[i].BuyerId == ob.BuyerId && lsdlist[i].ShipmentModeId == ob.ShipmentModeId)
            if (lsdlist[i].BuyerId == ob.BuyerId && lsdlist[i].ShipModeId === ob.ShipmentModeId) {
                var ExFactoryLeadTime = lsdlist[i].ExFactoryLeadTime;
                var FinishingLeadTime = lsdlist[i].FinishingLeadTime;
                var ProductionLeadTime = lsdlist[i].ProductionLeadTime;
                var DD = ob.DeliveryDateID;

                var dd1 = new Date(ob.DeliveryDateID);
                var cd1 = new Date(ob.DeliveryDateID);

                var d = ProductionLeadTime + ExFactoryLeadTime;
                var cd = FinishingLeadTime + ExFactoryLeadTime;

                dd1 = DateDeduct(dd1, d);
                cd1 = DateDeduct(cd1, cd);
                //var _LSD = new Date();
                //_LSD.setDate(dd1.getDate() - (ProductionLeadTime - ExFactoryLeadTime));

                //cd1.setDate(cd1 - (FinishingLeadTime - ExFactoryLeadTime));
                //console.log('lsd', _LSD);
                //console.log('cd', cd1);
                var _ldsF = $filter('dateFiltering')(dd1, 'dd-MMM-yy');
                var _cdF = $filter('dateFiltering')(cd1, 'dd-MMM-yy');

                ob.Lsd = _ldsF;
                ob.CommitmentDate = _cdF;
                break;
                //ob.CommitmentDate = dd1 - FinishingLeadTime - ExFactoryLeadTime;
            }
        }//for
    }
    function DateDeduct(date, days) {
        var declareDate = new Date(date);
        declareDate.setDate(declareDate.getDate() - days);
        //var dateFormated = $filter("date")(declareDate, 'dd-MMM-yyyy');
        return declareDate;
    }
    function DateAdd(date, days) {
        var declareDate = new Date(date);
        declareDate.setDate(declareDate.getDate() + days);
        //var dateFormated = $filter("date")(declareDate, 'dd-MMM-yyyy');
        return declareDate;
    } function DateAddFormatted(date, days) {
        var declareDate = new Date(date);
        declareDate.setDate(declareDate.getDate() + days);
        var dateFormated = $filter("date")(declareDate, 'dd-MMM-yyyy');
        return dateFormated;
    }
    function IsAvailableInDetail(ob, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].ProductionOrderMaterialMasterId === ob.ProductionOrderMaterialMasterId && list[i].Archive === 0) {
                return true;
            }
        }
        return false;
    }
    $scope.selectOBMultiple = function () {
        try {
            GetSelectedOB($scope.obsearchList, $scope.detailList)
            angular.element(document.querySelector('#obmodal')).modal('hide');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.getWCCode = function () {
        //console.log($scope.wcList);
        //get selected id from wclist
        $scope.wcsaveList = GetSelectedWorkcenter($scope.wcList);
        //set them in wcgrid in the body
        angular.element(document.querySelector('#wcpopup')).modal('hide');
    }
    function GetWorkcenter(masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getworkcenterlist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.wcsaveList = response.data;
            if (arrayLength($scope.searchbyWClist) === 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyWClist);
            }
        })//success
    }
    function GetSelectedWorkcenter(list) {
        var listnew = [];
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var local = angular.copy(list[i]);
            if (local.IsSelectedID) {
                listnew.push(local);
            }//if
        }//for
        return listnew;
    }
    function GetMasterRow(masterid) {
        $scope.master.Id = masterid;
        $http({
            method: 'GET',
            url: $scope.path + 'getmaster?id=' + masterid,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(result) > 0) {
                $scope.master = result[0];
                $scope.master.Id = masterid;
            }
            else {
                ClearOb($scope.master);
            }
        })//success
    }
    function GetDetailRow(masterid, batchqtylist) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailbymasterid?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.detailEditList = [];
            $scope.detailEditList = response.data;

            //set qty for all Rows
            SetBatchQty($scope.detailList, batchqtylist);
            //set qty for only selected Rows
            SetBatchQtyEdit($scope.detailList, $scope.detailEditList);
        })//success
    }
    //CalculationCheck
    $scope.IsTargetEnabled = true;
    $scope.CalculationCheck = function (event) {
        //console.log(event);
        try {
            var _isselected = event.target.checked;
            var _val = event.target.value;
            Calculate(_val);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.CalculationFromtext = function (val) {
        // console.log(val);
        try {
            Calculate(val);
            $scope.GetDays($scope.master.IncrementType, $scope.master.FirstDayOutPut, $scope.master.MinRequiredTargetHourly, $scope.master.StandardTime);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    function Calculate(txt) {
        try {
            if (txt === "Target") {
                $scope.master.NoOfWorkStation = CalculateWS();
                $scope.master.TargetHourlyFull = CalculateTarget(100);
                $scope.IsTargetEnabled = true;
            }
            else {
                $scope.master.MinRequiredTargetHourly = CalculateTarget($scope.master.Efficiency);
                $scope.master.TargetHourlyFull = CalculateTarget(100);
                $scope.IsTargetEnabled = false;
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.GetDays = function (txt, fdo, hourlytarget, totaltime) {
        try {
            //console.log('kkk',txt);
            //console.log('kkk', $scope.master.IncrementType);
            var dailyoutput = hourlytarget * totaltime;
            var iv = CalculateIncrementValue(txt);//hourly
            iv = iv * totaltime;//daily iv
            var _days = 1;
            var _cumi_output = fdo;
            while (_cumi_output < dailyoutput) {
                _days++;
                _cumi_output += iv + fdo;
                // console.log(_days, _cumi_output);

                if (iv <= 0) {
                    _days = 0;
                    break;
                }
            }
            //console.log(txt);
            // console.log('2', dailyoutput);
            $scope.master.DaysToGetTarget = _days;
        } catch (e) {
            throw e;
        }
    }
    function CalculateIncrementValue(isfixed) {
        try {
            var iv = CheckNullReturnZero($scope.master.IncrementValue);
            if (isfixed === "Fixed") {
                return iv;
            }
            else {
                var c = CheckNullReturnZero($scope.master.FirstDayOutPut);
                return iv * c / 100;
            }
        } catch (e) {
            throw e;
        }
    }
    function CalculateTarget(eff) {
        try {
            //var st = CheckNullReturnOne($scope.master.StandardTime);
            // var ws = CheckNull($scope.master.NoOfWorkStation, "Work Station");
            var ws = CheckNullReturnZero($scope.master.NoOfWorkStation);
            //var h = CheckNullReturnZero($scope.master.MinRequiredTargetHourly);
            var sam = CheckNullReturnOne($scope.master.Sam);
            var eff = CheckNullReturnOne(eff);
            eff = eff / 100;
            var res = ws * 60 / (sam / eff);
            var resu = res.toFixed(2);// val_ave.toFixed(3),
            var result = parseFloat(resu);
            return result;
        } catch (e) {
            throw e;
        }
    }
    function CalculateWS() {
        try {
            //var st = CheckNullReturnOne($scope.master.StandardTime);
            //var ws = CheckNull($scope.master.NoOfWorkStation, "Work Station");
            //var h = CheckNull($scope.master.MinRequiredTargetHourly, "Hourly Target");
            var h = CheckNullReturnZero($scope.master.MinRequiredTargetHourly);
            var sam = CheckNullReturnOne($scope.master.Sam);
            var eff = CheckNullReturnOne($scope.master.Efficiency);
            eff = eff / 100;
            var res = h * sam / (eff * 60);
            var resu = res.toFixed(2);// val_ave.toFixed(3),
            var result = parseInt(resu);
            return result;
        } catch (e) {
            throw e;
        }
    }
    function CheckNull(val, field) {
        try {
            if (baseService.isUndefinedOrNull(val)) {
                throw field + " can not be blank...";
            }
            else {
                return val;
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckNullReturnZero(val) {
        if (baseService.isUndefinedOrNull(val)) {
            return 0;
        }
        else {
            return val;
        }
    }
    function CheckNullReturnOne(val) {
        if (baseService.isUndefinedOrNull(val)) {
            return 1;
        }
        else {
            return val;
        }
    }
    $scope.ClearBody = function () {
        ClearOb($scope.pref);
        ClearOb($scope.detailmodal);
        ClearOb($scope.detailChildmodal);
        $scope.detailList = [];
        $scope.detailChildList = [];
        loadProcessList($scope.master.EntityId);
    }
    $scope.getBatchSingle = function (ob) {
        ClearOb($scope.pref);
        //$scope.ShowRecipeSearch = true;
        //console.log('twin', ob);
        $scope.pref.BatchNo = ob.BatchNo;
        $scope.pref.BatchDate = ob.BatchDate;
        $scope.pref.ProductionOrder = ob.ProductionOrder;
        $scope.pref.MaterialMaster = ob.MaterialMaster;
        $scope.pref.MaterialMasterId = ob.MaterialMasterId;
        $scope.pref.FileNo = ob.FileNo;
        $scope.pref.Buyer = ob.Buyer;
        $scope.pref.Customer = ob.Customer;
        $scope.pref.Qty = ob.Qty;
        $scope.pref.ProcessType = ob.ProcessType;
        $scope.pref.ProcessTypeId = ob.ProcessTypeId;
        angular.element(document.querySelector('#pomastersearchpopup')).modal('hide');
        LodaDetail($scope.master.ProcessId, $scope.pref.ProcessTypeId, $scope.pref.BatchNo);
        $scope.getCharacteristics();
    };

    function loadProcessList(entityid) {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProcessCbo?entityid=' + entityid,
        }).then(function successCallback(response) {
            $scope.processList = [];
            $scope.processList = response.data;
            // SetSelectedOBEdit($scope.detailList);
            // GetTotalDetailQty($scope.detailList);
        })
    }
    function loadSubprocessSet(processid, processtypeid) {
        $http({
            method: 'GET',
            url: $scope.path + 'loadsubprocessset?processid=' + processid + '&processtypeid=' + processtypeid,
        }).then(function successCallback(response) {
            $scope.subprocessSetList = [];
            $scope.subprocessSetList = response.data;
        })
    }

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
            //$scope.master.BatchDate = new Date();
            //var bdate = new Date($scope.master.BatchDate);
            var today = new Date().toLocaleDateString('en-GB', {
                day: 'numeric',
                month: 'short',
                year: 'numeric'
            }).split(' ').join('-');

            var d1 = new Date($scope.master.BatchDate);
            var d2 = new Date($scope.master.FirstInputDate);
            //console.log(today);
            //console.log(d1);
            //console.log(d2);
            if (d2 < d1) {
                throw "[First Input date] must be smaller than [Batch Date] !!!";
            }
            if ($scope.isLsdLesser()) {
                CheckField($scope.master.LsdRemark, "Lsd Remark");
            }
            if ($scope.isCommitmentDateLesser()) {
                CheckField($scope.master.CommitmentDateRemarks, "Commitment Date Remarks");
            }
            CheckQty($scope.detailList);
        } catch (e) {
            throw e;
        }
    }
    function CheckQty(list) {
        var _qty = 0;
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (baseService.isUndefinedOrNull(list[i].Qty) == false) {
                _qty += list[i].Qty;
            }
        }
        if (_qty <= 0) {
            throw "Minimum Batch qty must be greater than zero....";
        }
    }
    $scope.setSelected = function (idSelectedVote) {
        $scope.indexGetTime = idSelectedVote;
    }
    $scope.setTab = function (newTab) {
        if (newTab == 1) {
            //$scope.checkChilList();
        }
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    cboService.getCboProductionEntityByCompany($window.companyGroupId, $window.companyId, function (result) {
        $scope.entityList = result;
    });

    //$http({
    //    method: 'GET',
    //    url: 'OrderManagements/productionbatch/getcompanywiseplantcbo'
    //}).then(function successCallback(response) {
    //    $scope.plantList = data.Rows;
    //    console.log($scope.plantList);
    //});

    function loadPOGrid(pomid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getproductionordergrid?pomid=' + pomid,
        }).then(function successCallback(response) {//getmmuomcbo
            //loadRecipeList(pomid);

            $scope.detailList = [];
            $scope.detailList = response.data;
            $scope.grid.Characteristics1Has = HasValueC1($scope.detailList);
            $scope.grid.Characteristics2Has = HasValueC2($scope.detailList);
            $scope.grid.Characteristics3Has = HasValueC3($scope.detailList);
            ///get saved recipe list
            //GetRecipeListSavedByPOM
            SetQty($scope.detailList);
            loadRecipeListSaved(pomid);
            if (arrayLength($scope.searchbyMasterlist) == 0) {
                baseService.getDDLSearchColumn(result, $scope.searchbyMasterlist);
            }
        })
    };
    function GetMaterialMasterList(masterlist) {
        var mmlist = [];
        for (var i = 0; i < arrayLength(masterlist); i++) {
            if (mmlist.indexOf(masterlist[i].MaterialMasterId) == -1) {
                mmlist.push(
                    {
                        MaterialMasterId: masterlist[i].MaterialMasterId,
                        PommUnit: masterlist[i].PommUnit,
                        PommUnitId: masterlist[i].PommUnitId,
                        Uom: masterlist[i].Uom,
                        UomId: masterlist[i].UomId,
                        Characteristics1Uom: masterlist[i].Characteristics1Uom,
                        Characteristics1UomId: masterlist[i].Characteristics1UomId,
                        Characteristics1Qty: masterlist[i].Characteristics1Qty,
                        Qty: masterlist[i].Qty,
                        Characteristics23Qty: masterlist[i].Characteristics23Qty,
                        MMQty: masterlist[i].MMQty,
                        IsMaterialMasterLevel: masterlist[i].IsMaterialMasterLevel,
                        IsFirstLevel: masterlist[i].IsFirstLevel,
                        IsSecondLevel: masterlist[i].IsSecondLevel,
                        ProductionOrderCharacteristicsValue1stId: masterlist[i].ProductionOrderCharacteristicsValue1stId,
                        ProductionOrderCharacteristicsValue2ndId: masterlist[i].ProductionOrderCharacteristicsValue2ndId
                    }
                );
            }//if
        }//for
        return mmlist;
    }
    function SetQty(masterlist) {
        //IsMaterialMasterLevel IsFirstLevel IsSecondLevel Characteristics1Qty Characteristics23Qty MMQty
        var mmlist = GetMaterialMasterList(masterlist);
        for (var i = 0; i < arrayLength(mmlist); i++) {
            SetMMQty(mmlist[i], masterlist);
        }
    }
    function SetMMQty(mmob, masterlist) {
        for (var i = 0; i < arrayLength(masterlist); i++) {
            if (masterlist[i].MaterialMasterId == mmob.MaterialMasterId) {
                if (masterlist[i].ProductionOrderCharacteristicsValue1stId == mmob.ProductionOrderCharacteristicsValue1stId && masterlist[i].ProductionOrderCharacteristicsValue2ndId == mmob.ProductionOrderCharacteristicsValue2ndId) {
                    if (mmob.IsMaterialMasterLevel) {
                        masterlist[i].Qty = mmob.MMQty;
                        masterlist[i].Characteristics23Qty = mmob.MMQty;
                        masterlist[i].Uom = mmob.PommUnit;
                        masterlist[i].UomId = mmob.PommUnitId;
                    }
                    else if (mmob.IsSecondLevel) {
                        masterlist[i].Uom = mmob.Uom;
                        masterlist[i].Characteristics23Qty = mmob.Characteristics23Qty;
                        masterlist[i].Qty = mmob.Qty;
                        masterlist[i].UomId = mmob.UomId;
                    }
                    else {
                        masterlist[i].Uom = mmob.Characteristics1Uom;
                        masterlist[i].Characteristics23Qty = mmob.Characteristics1Qty;
                        masterlist[i].Qty = mmob.Characteristics1Qty;
                        masterlist[i].UomId = mmob.Characteristics1UomId;
                    }
                    break;
                }//sku same
            }//mmid same
        }//for
    }
    function loadRecipeListSaved(pomid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getrecipelistsavedbypom?pomid=' + pomid,
        }).then(function successCallback(response) {//getmmuomcbo
            $scope.recipeListSave = [];
            $scope.recipeListSave = response.data;
            $scope.Action = "Save";
            if (arrayLength(result) > 0) {
                $scope.Action = "Update";
            }
            else {
                $scope.Action = "Save";
            }
            SetRecipeValueSelectedEdit($scope.detailList, $scope.recipeListSave);
        })
    };

    $scope.SetDisable = function (index) {
        //for (var i = 0; i < $scope.detailChildList.length; i++) {
        //    if ($scope.detailChildList[i].Id === id) {
        if ($scope.detailChildList[index].IsJobWorkApplicable) {
            $scope.detailChildList[index].setDisable = false;
        }
        else {
            $scope.detailChildList[index].JobWorkType = null;
            $scope.detailChildList[index].setDisable = true;
        }
        //    }
        //}
    }

    ///**************************************************save delete and clear function*********************************
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.isWorkingDays();
            $scope.master.ProductionOrderMasterId = $scope.pomaster.ProductionOrderMasterId;
            // $scope.master.MaterialMasterId = $scope.detailList[0].MaterialMasterId;
            // console.log('mas', $scope.master);
            //console.log('detail', $scope.detailList);
            $scope.SaveMasterDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createmaster',
                dataType: 'JSON',
                data: { 'master': $scope.master, 'detaillist': $scope.detailList, 'workcenterlist': $scope.wcsaveList, 'psetlist': $scope.processSetList }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SaveMasterDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
            //}//valid
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    function DeleteMaster() {
        try {
            if (baseService.isUndefinedOrNull($scope.master.Id)) {
                throw 'No Batch is selected...';
            }
            $http({
                method: 'POST',
                url: $scope.path + 'deletemaster',
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearHead();
                    ClearBody();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    function ValidationDetail() {
        try {
            CheckField($scope.detailmodal.EntityId, "EntityId");
            CheckField($scope.detailmodal.ProcessId, "Process");
            CheckField($scope.detailmodal.ProcessTypeId, "Process Type");
            CheckField($scope.detailmodal.ProductionBatchMasterId, "Batch No");
            CheckField($scope.detailmodal.ProcessCriteriaId, "Process Criteria");
            CheckField($scope.detailmodal.AvgWeight, "Avg Weight");
            CheckField($scope.detailmodal.CycleTime, "Cycle Time");
            CheckField($scope.detailmodal.LoadFactor, "Load Factor");
            CheckField($scope.detailmodal.Qty, "Qty");
        } catch (e) {
            throw e;
        }
    }
    $scope.SaveDetail = function () {
        try {
            $scope.detailmodal.EntityId = $scope.master.EntityId;
            $scope.detailmodal.ProcessId = $scope.master.ProcessId;
            $scope.detailmodal.ProcessTypeId = $scope.pref.ProcessTypeId;
            $scope.detailmodal.ProductionBatchMasterId = $scope.pref.BatchNo;
            //$scope.detailmodal.ProductionBatchMasterId = $scope.pref.BatchNo;
            ValidationDetail();
            //$scope.detail.WorkCenterMasterId = $scope.mastermodal.Id;
            //$scope.detailmodal.MaterialMasterId = $scope.master.MaterialMasterId;
            //for (var i in $scope.detail) {
            //    $scope.detail[i] = $scope.detailmodal[i];
            //}
            console.log($scope.detailmodal);
            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createdetail',
                dataType: 'JSON',
                data: { 'detail': $scope.detailmodal }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure', 'detailentrypopup');
                }
                else {
                    angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'detailentrypopup');
                    LodaDetail($scope.master.ProcessId, $scope.pref.ProcessTypeId, $scope.pref.BatchNo);
                    $scope.SaveDetailDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure', 'detailentrypopup');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error', 'detailentrypopup');
        }
    }

    ///common function ends-------------------------------------------------------------------------------------------------
    ///3.customised function------------------------------------------------------------------------------------------------

    ///**************************************************show modal*********************************

    $scope.AddNew = function () {
        try {
            ClearBody();
            $scope.ShowDetail = true;
            $scope.master.Id = null;
            AddnewOnly($scope.master.MaterialMasterId, $scope.pomaster.ProductionOrderMasterId);
            GetMMDefaultSetting($scope.master.MaterialMasterId);
            LoadProcessSetNew($scope.master.EntityId);
            //getOrderBreakDown('ADDNEW',$scope.master.MaterialMasterId, $scope.pomaster.ProductionOrderMasterId);
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    function LoadProcessSetNew(entityid) {
        $scope.processSetList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'loadprocesssetbyentity?entityid=' + entityid,
        }).then(function successCallback(response) {
            $scope.processSetList = response.data;
        })
    }
    function LoadProcessSetEdit(batchid) {
        $scope.processSetList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'loadprocesssetbybatch?batchid=' + batchid,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                $scope.processSetList = response.data;
            }
            else {
                //if in edit mode no processSet is found saved, let the setting table load.
                LoadProcessSetNew($scope.master.EntityId);
            }
        })
    }
    $scope.clearOrderBreakDown = function () {
        $scope.FirstInputDate = false;
        //$scope.master.MaterialMasterId = null;
        ClearBody();
    }

    $scope.clearBulletinMaster = function () {
        $scope.master.BulletinMasterId = null;
    }

    $scope.masterAddEditPopup = function (flag) {
        try {
            if (flag == 'NEW') {
                ClearMasterModal();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
            else if (flag == 'DELETE') {
                ClearMaster();
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

    $scope.addProdRefPopup = function () {
        try {
            CheckField($scope.master.EntityId, "Entity");
            CheckField($scope.master.ProcessId, "Process");

            loadBatch($scope.master.EntityId, $scope.master.ProcessId);
            angular.element(document.querySelector('#pomastersearchpopup')).modal('show');
            //$scope.SaveMasterDisabled = false;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.editProdRefPopup = function () {
        try {
            CheckField($scope.master.EntityId, "Entity");
            CheckField($scope.master.ProcessId, "Process");

            loadBatchEdit($scope.master.EntityId, $scope.master.ProcessId);
            angular.element(document.querySelector('#pomastersearchpopup')).modal('show');
            //$scope.SaveMasterDisabled = false;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.showSubprocessSetModal = function () {
        try {
            //CheckField($scope.master.EntityId, "Entity");
            //CheckField($scope.master.ProcessId, "Process");
            LoaddSubprocessSet($scope.master.EntityId, $scope.master.ProcessId, $scope.pref.ProcessTypeId);
            //loadBatch($scope.master.EntityId, $scope.master.ProcessId);
            angular.element(document.querySelector('#subprocesssearchmodal')).modal('show');
            //$scope.SaveMasterDisabled = false;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    function LoaddSubprocessSet(entityid, processid, processtypeid) {
        baseService.setCurrentPage('subprocessSetlist');
        baseService.init($scope.path + 'getsubprocesslist', null, $scope.maxRow, null, 'Code', 'Code');
        $scope.loadSubprocessSetData = function (pageno) {
            $rootScope.parameters.entityid = $scope.master.EntityId;
            $rootScope.parameters.processid = $scope.master.ProcessId;
            $rootScope.parameters.processtypeid = $scope.pref.ProcessTypeId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.subprocessSetlist = result.Rows;
                    console.log($scope.subprocessSetlist);
                    if (baseService.arrayLength($scope.sbSubprocessSetlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbSubprocessSetlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadSubprocessSetData();
    }
    $scope.selectSubprocessSetMultiple = function () {
        console.log('$scope.subprocessSetlist', $scope.subprocessSetlist);
        console.log('$scope.detailChildList', $scope.detailChildList);
        angular.forEach($scope.subprocessSetlist, function (x) {
            if (x.IsSelectedId) {
                console.log('sing', x);
                if (CheckExistsDetail(x.SubProcessSetDetailId, $scope.detailChildList) == false) {
                    $scope.detailChildList.push({
                        Id: createguid("PRSP"),
                        Subprocess: x.Subprocess,
                        EntityOrVendorName: x.EntityOrVendorName,
                        Code: x.Code,
                        Description: x.Description,
                        Days: x.Days,
                        Symbol: x.Symbol,
                        JobWorkApplicable: x.JobWorkApplicable,
                        RequiredDays: x.RequiredDays,
                        Sequence: x.Sequence,
                        DefaultPlanning: x.DefaultPlanning,
                        IsSelectedId: x.IsSelectedId,
                        SubProcessSetId: x.SubProcessSetId,
                        SubProcessSetDetailId: x.SubProcessSetDetailId,
                        ProductionBatchProcessCriteriaId: $scope.detailmodal.Id,
                        ProductionBatchMasterId: $scope.pref.BatchNo,
                        ProcessId: x.ProcessId,
                        ProcessTypeId: x.ProcessTypeId,
                        EntityId: $scope.master.EntityId
                    });
                }//CheckExists not
            }//if selected
        });//forEach
        angular.element(document.querySelector('#subprocesssearchmodal')).modal('hide');
        //console.log('ss', $scope.detailChildList);
    }
    function CheckExists(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetId == id) {
                return true;
            }
        }
        return false;
    }
    function CheckExistsDetail(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetDetailId == id) {
                return true;
            }
        }
        return false;
    }

    //$scope.showDeleteConfirmation = function (data) {
    //    console.log('1354',data);
    //    $scope.message_confirmation = '';
    //    $scope.gridId = data.Id;
    //    $scope.message_confirmation = 'Are you sure want to delete [ ' + data.Subprocess + ' ]';
    //    angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    //};
    //$scope.deleteDetailChildGrid = function () {
    //    for (var i = 0; i < $scope.detailChildList.length; i++) {
    //        if ($scope.detailChildList[i].Id == $scope.gridId) {
    //            $scope.detailChildList.splice(i, 1);
    //            break;
    //        }
    //    }
    //    $scope.gridId = '';
    //    if ($scope.detailChildList.length > 0)
    //        return true;
    //    else
    //        return false;
    //};

    $scope.showDeleteConfirmation = function (data) {
        $scope.id = data.SubProcessSetDetailId;
        $scope.message_confirmation = 'Are you sure to delete [ ' + data.Subprocess + ' ]';
        angular.element(document.querySelector('#confirmContactdelete')).modal('show');
    };

    $scope.deleteDetailChildGrid = function () {
        for (var i = 0; i < $scope.detailChildList.length; i++) {
            if ($scope.id == $scope.detailChildList[i].SubProcessSetDetailId) {
                $scope.detailChildList.splice(i, 1);
                break;
            }
        }
    };

    //$scope.getCharacteristics();
    $scope.getCharacteristicsValueData = function (characteristicsid) {
        //baseService.init($scope.path + 'getcharacteristicsvaluelist', null, 25, null, 'Description', 'Description');
        baseService.setCurrentPage('characteristicsValueData');
        baseService.init('materials/characteristicsvalue/characteristicsvaluesearh', null, $scope.maxRow, null, 'Code', 'Code');
        $scope.loadCharacteristicsValueData = function (pageno) {//loadProcessData
            $rootScope.parameters.CharacteristicsId = characteristicsid;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.characteristicsValueData = result.Rows;
                    //console.log(result.Rows);
                    if (baseService.arrayLength($scope.searchbyCharacteristicsValuelist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyCharacteristicsValuelist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadCharacteristicsValueData();
    }
    $scope.getCharacteristics = function () {
        //$scope.master.EntityId, $scope.pref.MaterialMasterId
        $http({
            method: 'GET',
            url: $scope.path + 'GetSkuAsperConfig/',
            params: { entityid: $scope.master.EntityId, materialmasterid: $scope.pref.MaterialMasterId }
        }).then(function successCallback(response) {
            //console.log('data', response.data);
            ClearCharacteristics();
            if (baseService.arrayLength(response.data) > 0) {
                $scope.detailmodal.SelectedCharacteristics = response.data[0].SelectedCharacteristics;
                $scope.detailmodal.Characteristics1Selected = response.data[0].Characteristics1Selected;
                $scope.detailmodal.Characteristics2Selected = response.data[0].Characteristics2Selected;
                $scope.detailmodal.Characteristics3Selected = response.data[0].Characteristics3Selected;

                $scope.detailmodal.Characteristics1 = response.data[0].Characteristics1;
                $scope.detailmodal.Characteristics2 = response.data[0].Characteristics2;
                $scope.detailmodal.Characteristics3 = response.data[0].Characteristics3;

                $scope.detailmodal.Characteristics1Id = response.data[0].Characteristics1Id;
                $scope.detailmodal.Characteristics2Id = response.data[0].Characteristics2Id;
                $scope.detailmodal.Characteristics3Id = response.data[0].Characteristics3Id;
            }
            else {
                if ($scope.detailmodal.ProcessId != null && $scope.detailmodal.ProcessId != '') {
                    ShowResult('No data found in Recipe Config...', 'Error');
                }
            }
        })
    }
    $scope.showCharacteristicsGrid = function (hasCharForMM) {
        if (hasCharForMM == null || hasCharForMM == '') {
            return false;
        }
        else {
            return true;
        }
    }
    $scope.clearCharacteristics1Value = function () {
        $scope.detailmodal.Characteristics1ValueId = null;
        $scope.detailmodal.Characteristics1Value = null;
    };
    $scope.clearCharacteristics2Value = function () {
        $scope.detailmodal.Characteristics2ValueId = null;
        $scope.detailmodal.Characteristics2Value = null;
    };
    $scope.clearCharacteristics3Value = function () {
        $scope.detailmodal.Characteristics3ValueId = null;
        $scope.detailmodal.Characteristics3Value = null;
    };
    $scope.searchCharacteristics3Value = function (cvid) {
        $scope.dim = "3";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics2Value = function (cvid) {
        $scope.dim = "2";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics1Value = function (cvid) {
        $scope.dim = "1";
        $scope.getCharacteristicsValueData(cvid);
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.getCharacteristicsValueCode = function (id, Description) {
        if ($scope.dim == "1") {
            if (id == null || id == '') {
                $scope.detailmodal.Characteristics1ValueId = null;
                $scope.detailmodal.Characteristics1Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detailmodal.Characteristics1ValueId = id;
                $scope.detailmodal.Characteristics1Value = Description;
            }
        }
        else if ($scope.dim == "2") {
            if (id == null || id == '') {
                $scope.detailmodal.Characteristics2ValueId = null;
                $scope.detailmodal.Characteristics2Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detailmodal.Characteristics2ValueId = id;
                $scope.detailmodal.Characteristics2Value = Description;
            }
        }
        else if ($scope.dim == "3") {
            if (id == null || id == '') {
                $scope.detailmodal.Characteristics3ValueId = null;
                $scope.detailmodal.Characteristics3Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detailmodal.Characteristics3ValueId = id;
                $scope.detailmodal.Characteristics3Value = Description;
            }
        }
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('hide');
    };

    $scope.SaveDetailChild = function () {
        try {
            $http({
                method: 'POST',
                url: $scope.path + 'savedetailchild',
                dataType: 'JSON',
                data: {
                    'ProductionBatchSubprocessCritariaId': $scope.detailmodal.Id
                    , 'productionBatchSubprocessSet': $scope.detailChildList
                }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure', 'detailchildentrypopup');
                }
                else {
                    angular.element(document.querySelector('#detailchildentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success', 'detailchildentrypopup');
                    //$scope.SaveDetailChildDisabled = false;
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure', 'detailchildentrypopup');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error', 'detailchildentrypopup');
        }
    }
    $scope.GetProcessName = function () {
        var Process = angular.element("#Process :selected").text();
        $scope.master.Process = Process;
    }
    $scope.clearDetailChild = function () {
        $scope.detailChildList = [];
    }
    $scope.charSetting = [];

    function ClearCharacteristics() {
        $scope.detailmodal.SelectedCharacteristics = null;
        $scope.detailmodal.Characteristics1Selected = null;
        $scope.detailmodal.Characteristics2Selected = null;
        $scope.detailmodal.Characteristics3Selected = null;

        $scope.detailmodal.Characteristics1 = null;
        $scope.detailmodal.Characteristics2 = null;
        $scope.detailmodal.Characteristics3 = null;

        $scope.detailmodal.Characteristics1Id = null;
        $scope.detailmodal.Characteristics2Id = null;
        $scope.detailmodal.Characteristics3Id = null;
    }
    $scope.detailEntryPopup = function (flag, ob) {
        LoadweightUom($scope.pref.MaterialMasterId);
        //Validation
        if (flag == "EDIT") {
            //$scope.materialindex = index;
        }
        else {
            $scope.detailmodal.Id = null;
            //ClearOb($scope.detailmodal);
        }

        ClearOb($scope.detail);
        //clearObject($scope.master);
        //$scope.uomList = [];
        LoaddllDetail(flag, ob);
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };
    function LoaddllDetail(flag, ob_criteria) {
        $http({
            method: 'GET',
            url: 'Processes/processcriteria/getcriteriacbo'
        }).then(function (response) {
            $scope.processCriteriaList = response.data;
            //console.log(' $scope.processCriteriaList', response.data);
            if (flag == "EDIT") {
                //load a criteria info
                GetDetailById(ob_criteria.Id, ob_criteria.Qty);
            }
        });
    }
    function GetDetailById(id, qty) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailgriddata?id=' + id,
        }).then(function (response) {
            // $scope.getCharacteristics();
            //console.log(qty, response.data[0]);
            $scope.detailmodal.AvgWeight = response.data[0].AvgWeight;
            $scope.detailmodal.CycleTime = response.data[0].CycleTime;
            $scope.detailmodal.Id = response.data[0].Id;
            $scope.detailmodal.LoadFactor = response.data[0].LoadFactor;
            $scope.detailmodal.ProcessCriteriaId = response.data[0].ProcessCriteriaId;
            $scope.detailmodal.Qty = response.data[0].Qty;;
            $scope.detailmodal.WeightUomId = response.data[0].WeightUomId;
            $scope.detailmodal.Characteristics1Value = response.data[0].Characteristics1Value;
            $scope.detailmodal.Characteristics1ValueId = response.data[0].Characteristics1ValueId;
            $scope.detailmodal.Characteristics2Value = response.data[0].Characteristics2Value;
            $scope.detailmodal.Characteristics2ValueId = response.data[0].Characteristics2ValueId;
            $scope.detailmodal.Characteristics3Value = response.data[0].Characteristics3Value;
            $scope.detailmodal.Characteristics3ValueId = response.data[0].Characteristics3ValueId;
            //console.log('uuu', $scope.detailmodal);
            //console.log('uuur', response.data[0]);
        });
    }
    $scope.detailChildEntryPopups = function (ob_criteria) {
        //if (id == null || id == "") {
        //    ShowResult("Select a 'Line Item' first....")
        //    return;
        //}
        console.log('ob_criteria', ob_criteria);
        //ProcessId
        //ProcessTypeId
        getDetailChildData(ob_criteria.Id, $scope.master.EntityId, ob_criteria.ProcessId, ob_criteria.ProcessTypeId);
        $scope.detailmodal.Id = ob_criteria.Id;
        //$scope.detailchildmodal.RecipeSubprocessId = ob_criteria.Id;
        //$scope.detailChildindex = -1;
        $scope.SaveDetailChildDisabled = false;
        //$scope.ActionDetailChild = 'Save';
        //ClearDetailChild();
        // $scope.CancelDetailChild();
        //$scope.getDetailChildData(ob_criteria.Id);
        // $scope.loadDDL();
        angular.element(document.querySelector('#detailchildentrypopup')).modal('show');
    };

    function getDetailChildData(productionBatchProcessCriteriaId, entityid, processid, processtypeid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetailchildlist?productionBatchProcessCriteriaId=' + productionBatchProcessCriteriaId +
            '&entityid=' + entityid +
            '&processid=' + processid +
            '&processtypeid=' + processtypeid,
        }).then(function successCallback(response) {
            $scope.detailChildList = response.data;
            //if (baseService.arrayLength($scope.sbSubprocessSetlist) == 0) {
            //    baseService.getDDLSearchColumn(response.data, $scope.sbSubprocessSetlist);
            //}
            //console.log('xxx',$scope.detailChildList);
        })
    };

    //function GetWorkCenterList() {
    //    $http.get("OrderManagements/workcentermaster/GetWorkCenterByCompany/")
    //          .then(function (response) {
    //              $scope.wcList = [];
    //              $scope.wcList = response.data;
    //              if (arrayLength($scope.searchbyWClist) == 0) {
    //                  baseService.getDDLSearchColumn(response.data, $scope.searchbyWClist);
    //              }
    //          });
    //}
    function GetWorkCenterList() {
        baseService.init('OrderManagements/workcentermaster/getlistbyplantandprocess/', null, 10, null, 'UserName', 'UserName');
        $scope.loadWCData = function (pageno) {
            $rootScope.parameters.plantid = $scope.pomaster.PlantId;
            $rootScope.parameters.processid = $scope.recipeinfo.ProcessId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.wcList = result.Rows;
                    if (arrayLength($scope.searchbyWClist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyWClist);
                    }
                    SetWCSelected($scope.wcsaveList, $scope.wcList);
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadWCData();
    }
    function SetWCSelected(savelist, modallist) {
        for (var i = 0; i < baseService.arrayLength(savelist); i++) {
            var id = savelist[i].Id;
            SetSelectedWC(id, modallist);
        }
    }
    function SetSelectedWC(id, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (list[i].Id == id) {
                list[i].IsSelectedID = true;
                break;
            }
        }
    }
    $scope.WorkcenterPopup = function () {
        GetWorkCenterList();
        angular.element(document.querySelector('#wcpopup')).modal('show');
    };
    $scope.getRecipeRow = function () {
        RemoveRows($scope.master.ProductionOrderCharacteristicsValue1stId, $scope.master.ProductionOrderCharacteristicsValue2ndId);
        AddRows($scope.master.ProductionOrderCharacteristicsValue1stId, $scope.master.ProductionOrderCharacteristicsValue2ndId, $scope.master.ProductionOrderMasterId, $scope.masterIndex);
        angular.element(document.querySelector('#recipepopup')).modal('hide');

        //console.log($scope.recipeListSave);
    }
    $scope.CheckAllWC = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < arrayLength($scope.wcList); i++) {
            $scope.wcList[i].IsSelectedID = _isselected;
        }
    }
    function RemoveRows(pc1Id, pc2Id) {//RecipeMasterId ProductionOrderCharacteristicsValue2ndId ProductionOrderCharacteristicsValue1stId ProductionOrderMasterId
        var recipeListSavenew = [];
        for (var i = 0; i < arrayLength($scope.recipeListSave); i++) {
            if ($scope.recipeListSave[i].ProductionOrderCharacteristicsValue1stId == pc1Id && $scope.recipeListSave[i].ProductionOrderCharacteristicsValue2ndId == pc2Id) {
                if (IsSelected($scope.recipeListSave[i].RecipeMasterId) == false) {
                    //$scope.recipeListSave.s(i, 1);
                }
                else {
                    recipeListSavenew.push($scope.recipeListSave[i]);
                }
            }
            else {
                recipeListSavenew.push($scope.recipeListSave[i]);
            }
        }//for
        $scope.recipeListSave = [];
        $scope.recipeListSave = angular.copy(recipeListSavenew);
        //console.log(recipeListSavenew);
    }
    function IsSelected(recipeid) {
        var r = false;
        for (var i = 0; i < arrayLength($scope.recipeData); i++) {
            if ($scope.recipeData[i].Id == recipeid) {
                if ($scope.recipeData[i].IsSelectedID) {
                    r = true;
                    break;
                }//selected
                else {
                    r = false;
                    break;
                }
            }//id found
        }//for
        return r;
    }
    function AddRows(pc1Id, pc2Id, Id, index) {
        var recipe = "";
        var process = "";
        var count = 0;

        for (var i = 0; i < arrayLength($scope.recipeData); i++) {
            if ($scope.recipeData[i].IsSelectedID) {
                recipe = $scope.recipeData[i].Recipe;
                process = $scope.recipeData[i].Process;
                if (ExistsRecipe($scope.recipeData[i].Id, $scope.recipeListSave, pc2Id, pc1Id) == false) {
                    count++;
                    $scope.recipeListSave.push(
                        {
                            RecipeMasterId: $scope.recipeData[i].Id,
                            Process: $scope.recipeData[i].Process,
                            Recipe: $scope.recipeData[i].Recipe,
                            ProductionOrderCharacteristicsValue2ndId: pc2Id,
                            ProductionOrderCharacteristicsValue1stId: pc1Id,
                            ProductionOrderMasterId: Id
                        }
                    );//push /RecipeMasterId ProductionOrderCharacteristicsValue2ndId ProductionOrderCharacteristicsValue1stId ProductionOrderMasterId
                }//not taken yet
                else {
                    count++;//taken already
                }
            }//IsSelectedID
        }//for
        $scope.Action = "Update";
        if (count == 1) {
            $scope.masterList[index].Recipe = recipe;
            $scope.masterList[index].Process = process;
            $scope.masterList[index].Taggable = true;
        }
        else {
            if (count > 1) {
                $scope.masterList[index].Recipe = '';
                $scope.masterList[index].Process = '';
                $scope.masterList[index].Taggable = true;
            }
            else {
                $scope.masterList[index].Recipe = '';
                $scope.masterList[index].Process = '';
                $scope.masterList[index].Taggable = false;
            }
        }
    }
    function SetRecipeValueSelectedEdit(masterlist, recipelist) {
        for (var i = 0; i < arrayLength(masterlist); i++) {
            FindMasterRow(masterlist[i], recipelist);
        }
    }
    function FindMasterRow(MasterOb, recipelist) {
        var recipe = '';
        var process = '';
        var count = 0;
        for (var i = 0; i < arrayLength(recipelist); i++) {
            if (recipelist[i].ProductionOrderCharacteristicsValue1stId == MasterOb.ProductionOrderCharacteristicsValue1stId
                && recipelist[i].ProductionOrderCharacteristicsValue2ndId == MasterOb.ProductionOrderCharacteristicsValue2ndId
            ) {
                recipe = recipelist[i].Recipe;
                process = recipelist[i].Process;
                count++;
            }
        }//for
        if (count == 1) {
            MasterOb.Recipe = recipe;
            MasterOb.Process = process;
            MasterOb.Taggable = true;
        }
        else {
            if (count > 1) {
                MasterOb.Recipe = '';
                MasterOb.Process = '';
                MasterOb.Taggable = true;
            }
            else {
                MasterOb.Recipe = '';
                MasterOb.Process = '';
                MasterOb.Taggable = false;
            }
        }//else >1 or 0
    }
    function ExistsRecipe(RecipeId, saveList, pc2Id, pc1Id) {
        for (var i = 0; i < arrayLength(saveList); i++) {
            if (saveList[i].RecipeMasterId == RecipeId && saveList[i].ProductionOrderCharacteristicsValue2ndId == pc2Id && saveList[i].ProductionOrderCharacteristicsValue1stId == pc1Id) {
                return true;
            }
        }
        return false;
    }
    function SetRecipeSelected(recipelist, savelist, pc1Id, pc2Id) {
        for (var i = 0; i < arrayLength(savelist); i++) {
            if (savelist[i].ProductionOrderCharacteristicsValue2ndId == pc2Id && savelist[i].ProductionOrderCharacteristicsValue1stId == pc1Id) {
                if (IsAvailable(savelist[i].RecipeMasterId, recipelist));
            }//ids
        }//for
    }
    function IsAvailable(id, list) {
        for (var i = 0; i < arrayLength(list); i++) {
            if (id == list[i].Id) {
                list[i].IsSelectedID = true;
                break;
            }
        }
    }

    $scope.WCIndex = -1;
    $scope.deleteWCPopup = function (ob, index) {
        try {
            if (baseService.isUndefinedOrNull(ob.Id)) {
                throw "Select a Work center...";
            }
            $scope.message_confirmation = "Are you sure to delete [" + ob.UserName + "] ";
            angular.element(document.querySelector('#confirmwcdelete')).modal('show');
            $scope.WCIndex = index;
        } catch (e) {
            ShowResult(e, 'Error');
        }
        //$rootScope.passValue(_id, $scope.masterindex);
    }
    $scope.deleteMasterPopup = function (id) {
        try {
            if (baseService.isUndefinedOrNull(id)) {
                throw "Select a Batch...";
            }
            $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
            angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
        //$rootScope.passValue(_id, $scope.masterindex);
    }
    $scope.removeWCYes = function () {
        angular.element(document.querySelector('#confirmwcdelete')).modal('hide');
        $scope.wcsaveList.splice($scope.WCIndex, 1);
        $scope.WCIndex = -1;
        //for (var i = 0; i < baseService.arrayLength($scope.wcsaveList); i++) {
        //}
    };
    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        DeleteMaster();
    };
    function ClearBody() {
        try {
            $scope.ShowDetail = false;
            $scope.DisableRecipeddl = false;
            $scope.btndeletemaster = false;
            $scope.ShowPOGrid = false;
            $scope.ShowbtnSave = false;
            $scope.detailList = [];
            $scope.wcList = [];
            $scope.wcsaveList = [];
        } catch (e) {
            throw e;
        }
    }
    function ClearHead() {
        try {
            $scope.DisableRecipeddl = false;
            $scope.ShowRecipeSearch = false;
            //$scope.FirstInputDate = false;
            var today = new Date().toLocaleDateString('en-GB', {
                day: 'numeric',
                month: 'short',
                year: 'numeric'
            }).split(' ').join('-');
            $scope.master.BatchDate = today;
            $scope.recipeList = [];
            ClearOb($scope.pomaster);
        } catch (e) {
            throw e;
        }
    }

    $scope.isWorkingDays = function () {
        try {
            if ((new Date($scope.master.MinWorkingDays) > new Date($scope.master.DaysToGetTarget))) {
                throw "Minumum Working Days Must be Smaller Than Target Days !!!";
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.isLsdLesser = function () {
        try {
            var TargetLSD = $filter('dateFiltering')($scope.master.TargetLsd, 'dd-MM-yyyy');
            var LSD = $filter('dateFiltering')($scope.master.Lsd, 'dd-MM-yyyy');
            if ((new Date($scope.master.TargetLsd) > new Date($scope.master.Lsd))) {
                return true;
            }
            else {
                $scope.master.LsdRemark = null;
                return false;
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.isCommitmentDateLesser = function () {
        try {
            var targetCommitmentDate = $filter('dateFiltering')($scope.master.TargetCommitmentDate, 'dd-MM-yyyy');
            var commitmentDate = $filter('dateFiltering')($scope.master.CommitmentDate, 'dd-MM-yyyy');
            if ((new Date($scope.master.TargetCommitmentDate) > new Date($scope.master.CommitmentDate))) {
                return true;
            }
            else {
                $scope.master.CommitmentDateRemarks = null;
                return false;
            }
        } catch (e) {
            throw e;
        }
    }

    $scope.deleteDetail = function (ob) {
        var _ProcessCriteria = '';
        if (baseService.isUndefinedOrNull(ob.ProcessCriteria)) {
            _ProcessCriteria = angular.element("#processcriteria :selected").text();
        }
        else {
            _ProcessCriteria = ob.ProcessCriteria;
        }
        //console.log('99',kk);
        $scope.detailmodal.Id = ob.Id;
        $scope.message_confirmation = "Are you sure to delete :-[" + _ProcessCriteria + "]";
        angular.element(document.querySelector('#cdetaildelete')).modal('show');
    }
    $scope.deleteDetailYes = function () {
        $scope.DeleteDetailData();
        angular.element(document.querySelector('#cdetaildelete')).modal('hide');
    };
    function Isfound(id, list) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            if (id == list[i].ProductionOrderMaterialMasterId) {
                list[i].Archive = true;
                break;
            }
        }
    }
    ///3.loadtime call******************************************************************************************************
    ///service
    // #region Delete
    function ClearDetailModal() {
        $scope.detailmodal.AvgWeight = null;
        $scope.detailmodal.CycleTime = null;
        $scope.detailmodal.Id = null;
        $scope.detailmodal.LoadFactor = null;
        $scope.detailmodal.ProcessCriteriaId = null;
        $scope.detailmodal.Qty = null;
        $scope.detailmodal.WeightUomId = null;
        $scope.detailmodal.Characteristics1Value = null;
        $scope.detailmodal.Characteristics1ValueId = null;
        $scope.detailmodal.Characteristics2Value = null;
        $scope.detailmodal.Characteristics2ValueId = null;
        $scope.detailmodal.Characteristics3Value = null;
        $scope.detailmodal.Characteristics3ValueId = null;
    }
    $scope.DeleteDetailData = function () {
        if (!baseService.isUndefinedOrNull($scope.detailmodal.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.detailmodal.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    LodaDetail($scope.master.ProcessId, $scope.pref.ProcessTypeId, $scope.pref.BatchNo);
                    ClearDetailModal();
                    angular.element(document.querySelector('#detailentrypopup')).modal('hide');
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
    // #endregion

    //$scope.getPOMaster();

    //$scope.ClearDetail = function () {
    //    ClearOb($scope.detailmodal);
    //}

    ///function
    ///loadtime call ends***************************************************************************************************
    ClearHead();
    ClearBody();

    //#region Sub Process Set

    $http({
        method: 'GET',
        url: 'Processes/processset/enumjobworktypelistcbo'
    }).then(function successCallback(response) {
        $scope.jobWorkTypeList = response.data;
    });

    $scope.processSetDetailTblShow = false;
    $scope.subProcessSetDetails = [];
    $scope.valueData = '';
    $scope.subprocessSetDetailDataList = [];
    $scope.subprocessSetDetailList = [];
    $scope.subprocessSetDetailParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "SubProcessName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.subprocessSetDetailPopUp = function () {
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.CompanyId)) {
            return ShowResult('Please at first select company......', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.ProcessId)) {
            return ShowResult('Please select Process......', 'failure');
        }
        if (baseService.isUndefinedOrNull($scope.subProcessSetNew.RequiredTimeUnit)) {
            return ShowResult('Please at first select required time unit......', 'failure');
        }
        $scope.subprocessSetDetailUrl = 'Processes/companysubprocess/getlist?companyId=' + $scope.subProcessSetNew.CompanyId +
            '&processId=' + $scope.subProcessSetNew.ProcessId;
        $scope.getProcessPopUpData = function (pageno) {
            baseService.paginationBase($scope.subprocessSetDetailUrl, pageno, $scope.subprocessSetDetailParameters)
                .then(function (result) {
                    $scope.subprocessSetDetailDataList = result.Rows;
                    $scope.subprocessSetDetailParameters.total_count = result.Total;
                    if (baseService.arrayLength($scope.subprocessSetDetailList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.subprocessSetDetailList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processSetDetailPopUp');
                }).finally(function () {
                });
        };

        angular.element(document.querySelector('#subprocessSetDetailPopUp')).modal('show');
        $scope.getProcessPopUpData();
    }
    function isProcessIdExistInGrid(list) {
        $scope.processIds = [];
        if (list.length > 0) {
            for (var i = 0; i < list.length; i++) {
                if (list[i]['Archive'] == false) {
                    $scope.processIds.push(list[i]['SubProcessId']);
                }
            }
        }
        return JSON.stringify($scope.processIds);
    }

    $scope.selectPSDDoubleClick = function (data) {
        $scope.addProcessSetDetails(data);
        $scope.closePSDPopUp();
    }
    $scope.selectPSDSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectPSDByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'subprocessSetDetailPopUp');
        }
        $scope.selectPSDDoubleClick($scope.valueData)
        $scope.closePSDPopUp();
    }
    $scope.closePSDPopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#subprocessSetDetailPopUp')).modal('hide');
    }

    $scope.addProcessSetDetails = function (data) {
        $scope.subProcessSetDetails.push({
            Id: $scope.pk(),
            SubProcessSetId: $scope.subProcessSetNew.Id,
            SubProcessId: data.SubProcessId,
            SubProcessName: data.SubProcessName,
            Sequence: $scope.subProcessSetDetails.length + 1,
            IsBaseProcess: false,
            Days: 0,
            Symbol: '+',
            ProductionCycleTime: 1,
            JobWorkApplicable: false,
            JobWorkType: null,
            EntityIdWithinCompany: null,
            EntityIdWithinGroup: null,
            VendorId: null,
            EntityOrVendorName: null,
            setDisable: true,
            class: 'new',
            Archive: false
        });
        if (!$scope.processSetDetailTblShow)
            $scope.processSetDetailTblShow = true;
    };
    $scope.valuePassInDelModal = function (data, index) {
        $scope.message_confirmation = '';
        $scope.gridId = data.Id;
        $scope.message_confirmation = 'Are you sure want to delete [ ' + data.SubProcessName + ' ]';
        angular.element(document.querySelector('#confirmDelPopUp')).modal('show');
    };
    $scope.removeRow = function () {
        for (var i = 0; i < $scope.subProcessSetDetails.length; i++) {
            if ($scope.subProcessSetDetails[i].Id == $scope.gridId) {
                $scope.subProcessSetDetails.splice(i, 1);
            }
        }
        $scope.gridId = '';
        if ($scope.subProcessSetDetails.length > 0)
            $scope.processSetDetailTblShow = true;
        else
            $scope.processSetDetailTblShow = false;
    };
    $scope.pk = function () {
        return 'new' + Math.floor(Math.random() * 900000) + 100000;
    };

    $scope.setPlusOrMinus = function (event, index) {
        for (var i = 0; i <= $scope.detailChildList.length - 1; i++) {
            if (i < index) {
                $scope.detailChildList[i].Symbol = '-';
                $scope.detailChildList[i].IsBaseProcess = false;
            }
            else if (i > index) {
                $scope.detailChildList[i].Symbol = '+';
                $scope.detailChildList[i].IsBaseProcess = false;
            }
            else if (i == index) {
                $scope.detailChildList[i].Symbol = null;
                $scope.detailChildList[i].Days = 0;
                $scope.detailChildList[i].IsBaseProcess = true;
            }
        }
    }
    function daysSortValidation(list) {
        try {
            var seq = 0;
            var seqNeg = 0;
            var isNeg = true;
            if (list[0].Days == 0) {
                isNeg = false;
            } else {
                seqNeg = parseInt(list[0].Days);
                seqNeg += 1;
            }
            for (var i = 0; i < list.length; i++) {
                if (isNeg == false) {//0,1,2
                    if (list[i].Days >= seq) {
                        seq = list[i].Days;
                    }
                    else//0,1,3,2
                        throw "Lag days sequence is not valid.....!";
                }
                else //2,1,0,1,2 or2,1,0
                {
                    if (list[i].Days <= seqNeg) {//2,1,0
                        seqNeg = list[i].Days;
                        if (list[i].Days == 0) {
                            isNeg = false;
                            seq = 0;
                        }
                    }
                    else {
                        //2,3,1,0,1,2
                        throw "Lag days sequence is not valid.....!";
                    }
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function isJobWorkType(list) {
        try {
            for (var i = 0; i < list.length; i++) {
                if (list[i].JobWorkApplicable && baseService.isUndefinedOrNull(list[i].JobWorkType)
                    && (baseService.isUndefinedOrNull(list[i].EntityIdWithinCompany)
                        || baseService.isUndefinedOrNull(list[i].EntityIdWithinGroup)
                        || baseService.isUndefinedOrNull(list[i].VendorId))
                ) {
                    throw 'Please select job work type or entity/vendor.......!';
                }
            }
        } catch (e) {
            ShowResult(e, 'failure')
        }
    }

    $scope.getDetails = function () {
        $http({
            method: 'GET',
            url: 'Processes/subprocessset/getsubprocesssetdetaillist?subprocessSetId=' + $scope.subProcessSetNew.Id
        }).then(function successCallback(response) {
            $scope.subProcessSetDetails = response.data;
            if ($scope.subProcessSetDetails.length > 0)
                $scope.processSetDetailTblShow = true;
            else
                $scope.processSetDetailTblShow = false;
        });
    }

    var move = function (origin, destination) {
        var temp = $scope.detailChildList[destination];
        var symbolIndex = null;
        $scope.detailChildList[destination] = $scope.detailChildList[origin];
        $scope.detailChildList[origin] = temp;
        //$scope.subProcessSetDetails[origin].Sequence = destination + 1;
        for (var i = 0; i < $scope.detailChildList.length; i++) {
            $scope.detailChildList[i].Sequence = i + 1;
            if ($scope.detailChildList[i].IsBaseProcess) {
                symbolIndex = i;
            }
        }
        $scope.setPlusOrMinus(null, symbolIndex);
    };
    $scope.moveUp = function (index) {
        move(index, index - 1);
    };
    $scope.moveDown = function (index) {
        move(index, index + 1);
    };
    //#endregion

    //#region

    $scope.valueData = '';
    $scope.id = "";
    $scope.showPopupSubprocessSet = function (ob) {
        if (isJobWorkApplicable($scope.detailChildList, ob.SubProcessSetDetailId)) {
            return ShowResult('Please select at first job work type..............!', 'failure', 'detailchildentrypopup');
        }
        $scope.id = ob.SubProcessSetDetailId;
        $scope.getVendorEntity(ob);
        angular.element(document.querySelector('#popUpId')).modal('show');
    }
    function typeCheckAndCreateUrl(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetDetailId == id) {
                if (list[i].JobWorkType === 'EntityWithinCompany') {
                    $scope.popUpTitle = "Entity Within Company Search";
                    return 'organizations/entity/withincompany?companyId=' + $window.companyId + '&entityId=' + $scope.master.EntityId;
                }
                else if (list[i].JobWorkType === 'EntityWithinGroup') {
                    $scope.popUpTitle = "Entity Within Group (other than this company) Search";
                    return 'organizations/entity/withingroup?companyGroupId=' + $window.companyGroupId + '&companyId=' + $window.companyId + '&entityId=' + $scope.master.EntityId;
                }
                else {
                    $scope.popUpTitle = "Vendor Search";
                    return 'parties/vendorcompanydata/getpartyfromvendor?companyGroupId=' + $window.companyGroupId + '&companyId=' + $window.companyId;
                }
            }
        }//for
    }

    $scope.vendorEntityList = [];
    $scope.sbvendorEntityList = [];
    $scope.popUpUrl == "";

    $scope.getVendorEntity = function (ob) {
        $scope.popUpUrl = typeCheckAndCreateUrl($scope.detailChildList, ob.SubProcessSetDetailId);
        console.log($scope.popUpUrl);
        baseService.setCurrentPage('vendorEntityList');
        baseService.init($scope.popUpUrl, null, $scope.maxRow, null, 'Name', 'Name');
        $scope.loadVendorEntity = function (pageno) {
            //$rootScope.parameters.entityid = $scope.master.EntityId;
            //$rootScope.parameters.pomid = $scope.pomaster.ProductionOrderMasterId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.vendorEntityList = result.Rows;
                    console.log('888', $scope.masterList);
                    if (baseService.arrayLength($scope.sbvendorEntityList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.sbvendorEntityList);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'detailchildentrypopup');
                }).finally(function () {
                });
        }; $scope.loadVendorEntity();
    }
    $scope.selectSubrocessSetSingle = function (ob) {
        console.log('ob', ob);
        valueSetInGrid($scope.detailChildList, ob, $scope.id)
        $scope.id = '';
        $scope.closePopUp();
    }
    $scope.selectDoubleClick = function (data) {
        valueSetInGrid($scope.detailChildList, data, $scope.id)
        $scope.id = '';
        $scope.closePopUp();
    }
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData)) {
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }

    function isJobWorkApplicable(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetDetailId == id) {
                if (baseService.isUndefinedOrNull(list[i].JobWorkType))
                    return true;
                else
                    return false;
            }
        }
    }
    function valueSetInGrid(list, data, id) {
        $scope.clearEntityOrVendor(list, id);
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetDetailId == id) {
                if (list[i].JobWorkType === 'EntityWithinCompany') {
                    list[i].EntityIdWithinCompany = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                else if (list[i].JobWorkType === 'EntityWithinGroup') {
                    list[i].EntityIdWithinGroup = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                else {
                    list[i].VendorId = data.Id;
                    list[i].EntityOrVendorName = data.Name;
                }
                break;
            }
        }
    }
    $scope.clearEntityOrVendor = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SubProcessSetDetailId === id) {
                //list[i].EntityIdWithinCompany = null;
                //list[i].EntityIdWithinGroup = null;
                //list[i].VendorId = null;
                //list[i].EntityOrVendorName = null;

                list[i].EntityIdWithinCompany = null;
                list[i].EntityIdWithinGroup = null;
                list[i].EntityOrVendorId = null;
                list[i].EntityOrVendorName = null;

                break;
            }
        }
    };
    $scope.clearJobType = function (list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === id) {
                list[i].JobWorkType = null;
                break;
            }
        }
    };
    //#endregion
};
ProductionOrderSubprocessController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];