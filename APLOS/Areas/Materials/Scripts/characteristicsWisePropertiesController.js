'use strict';
function CharacteristicsWisePropertiesController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    ///----------------------------------------------------------------------------------------------------------------------
    ///1.declaration
    ///2.function
    ///3.loadtime call
    ///
    ///------------------------------------------------------------------------SubsectionStructure-----------------------------------------------
    ///1.declaration----------------------------------------------------------------------------------------------------------
    ///variable
    $rootScope.title = "Characteristics Wise Properties";
    $scope.Action = 'Save';
    $scope.ActionDetail = "Save";
    $scope.masterIsChanged = false;
    //$scope.boolCharacteristicsGrid = false;
    $scope.AddEdit = null;
    $scope.IsCharacteristicsVisible = false;
    $scope.IsAdd = true;
    $scope.IsDel = false;
    $scope.CharacteristicsSearchTitle = "";
    $scope.gridDetailGrid = false;
    $scope.btnDetailEntryPopup = false;
    $scope.btndeletemaster = true;
    $scope.isdeletedetail = false;
    $scope.message_confirmation = "";
    $scope.ActionDetail = 'Save';//SaveDetailDisabled
    $scope.SaveDetailDisabled = false;//DeleteMaster
    $scope.btnDisabledSave = false;

    $scope.path = 'Materials/characteristicswiseproperties/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    //$scope.getSeqUrl = $scope.path + 'getautosequence';
    ////$scope.saveUrlMaster = $scope.path + 'createmaster';
    //$scope.saveUrlDetail = $scope.path + 'createdetail';
    //$scope.deleteUrlmaster = $scope.path + 'deletemaster';
    //$scope.deleteUrlDetail = $scope.path + 'deletedetail';
    ///list
    $scope.searchbyMasterlist = [];
    $scope.searchbyMaterialMasterDatalist = [];
    $scope.searchbyCharacteristicslist = [];
    $scope.searchbyMMUOMlist = [];
    $scope.searchbyCharacteristicsValuelist = [];
    $scope.searchbyDetaillist = [];

    $scope.masterList = [];
    $scope.detailuomList = [];
    $scope.detailList = [];
    $scope.detailfactorList = [];
    $scope.processList = [];
    $scope.companyList = [];
    $scope.plantList = [];
    $scope.unitList = [];
    $scope.CharacteristicsData = [];
    $scope.characteristicsValueData = [];
    $scope.mmUOMData = [];
    $scope.uomList = [];

    $scope.Data = [];
    $scope.detailuom = {
        Id: null,
        UOM: null,
        UOMCode: null,
        UOMId: null,
        CharacteristicsWisePropertiesMasterId: null,
        CharacteristicsWisePropertiesDetailId: null,
        Archive: false
    };

    $scope.detail = {
        Id: null,
        CharacteristicsWisePropertiesMasterId: null,
        MaterialMasterId: null,
        Characteristics1ValueId: null,
        Characteristics2ValueId: null,
        Characteristics3ValueId: null,
        Characteristics1Value: null,
        Characteristics2Value: null,
        Characteristics3Value: null,
        Archive: false
    };
    $scope.detailfactor = {
        Id: null,
        AUOM: null,
        BaseUOM: null,
        CharacteristicsWisePropertiesMasterId: null,
        CharacteristicsWisePropertiesDetailId: null,
        AlternativeUOMId: null,
        AlternativeUOMFactor: null,
        BaseUOMId: null,
        BaseUOMFactor: null,
        Archive: false
    };
    $scope.master = {
        Id: null,
        MaterialMasterId: null,
        Description: null,
        Code: null,
        GridNO: null,
        SelectedCharacteristics: null,
        Characteristics1Selected: false,
        Characteristics2Selected: false,
        Characteristics3Selected: false,
        Characteristics1: null,
        Characteristics2: null,
        Characteristics3: null,
        Characteristics1Id: null,
        Characteristics3Id: null,
        Characteristics2Id: null,
        UserName: null,
        MaterialType: null,
        MaterialGroup: null,
        MaterialGridId: null,
        BaseUOM: null,
        BaseUOMId: null,
        Sequence: null,
        Archive: false
    };

    ///other
    $scope.index = -1;
    $scope.masterindex = -1;
    $scope.detailindex = -1;
    $scope.dim = null;
    ///declaration ends-----------------------------------------------------------------------------------------------------
    ///2.function----------------------------------------------------------------------------------------------------
    // $http.get($scope.path + "getuomcbo?mmid=" + mmid)
    ///**************************************************get data from database*********************************
    $scope.CheckAllUOM = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < baseService.arrayLength($scope.mmUOMData); i++) {
            $scope.mmUOMData[i].IsSelectedID = _isselected;
        }
    }
    $scope.CheckAll = function (event) {
        //console.log(event);
        var _isselected = event.target.checked;
        for (var i = 0; i < baseService.arrayLength($scope.CharacteristicsData); i++) {
            $scope.CharacteristicsData[i].IsSelected = _isselected;
        }
    }
    $scope.loadCharacteristics = function (mmid) {
        try {
            $http.get($scope.path + "getcharacteristicslist?mmid=" + mmid)
                .then(function (response) {
                    //console.log(response);
                    $scope.CharacteristicsData = response.data;
                    if ($scope.CharacteristicsData == null || $scope.CharacteristicsData.length == 0) {
                        for (i in $scope.master) {
                            $scope.master[i] = null;
                        }
                        ShowResult('Characteristics not found for this [Material Master] ...', 'Information');
                    }
                    else {
                        //CharacteristicsData
                        for (var i = 0; i < baseService.arrayLength($scope.CharacteristicsData); i++) {
                            if ($scope.CharacteristicsData[i].Id == $scope.master.Characteristics1Id) {
                                $scope.CharacteristicsData[i].IsSelected = true;
                            }
                            if ($scope.CharacteristicsData[i].Id == $scope.master.Characteristics2Id) {
                                $scope.CharacteristicsData[i].IsSelected = true;
                            }
                            if ($scope.CharacteristicsData[i].Id == $scope.master.Characteristics3Id) {
                                $scope.CharacteristicsData[i].IsSelected = true;
                            }
                        }//for
                    }//else if data found
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadAllUOMCmb = function () {
        try {
            $http.get($scope.path + 'getuomcbo?materialmasterid=' + $scope.master.MaterialMasterId)
                .then(function (response) {
                    $scope.uomList = response.data;
                    //console.log(response.data);
                    //console.log($scope.uomList);
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadSequence = function () {
        try {
            $http.get($scope.getSeqUrl)
                .then(function (response) {
                    $scope.mastermodal.Sequence = response.data;
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };
    $scope.loadDDL = function () {
        try {
            cboService.getCboCompanyByCompanyGroup(' ', function (result) {
                $scope.companyList = result;
            });
            $http.get($scope.path + "getunitcbo/")
                .then(function (response) {
                    $scope.unitList = response.data;
                });
            $http.get($scope.path + "getprocesscbo/")
                .then(function (response) {
                    $scope.processList = response.data;
                });
            //$http.get($scope.getSeqUrl)
            //  .then(function (response) {
            //      //console.log(response);
            //      $scope.mastermodal.Sequence = response.data;
            //      console.log($scope.master);
            //  });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    function setUOMSelected(uomid) {
        for (var i = 0; i < baseService.arrayLength($scope.mmUOMData); i++) {
            if ($scope.mmUOMData[i].Id == uomid) {
                $scope.mmUOMData[i].IsSelectedID = true;
                break;
            }
        }
    }
    function setUOMSelectionBlank() {
        for (var i = 0; i < baseService.arrayLength($scope.mmUOMData); i++) {
            $scope.mmUOMData[i].IsSelectedID = false;
        }
    }
    $scope.getData = function () {
        baseService.init($scope.path + 'getlist', null, 25, null, 'MaterialMasterCode', 'MaterialMasterCode');
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
    $scope.getMMData = function () {
        baseService.init('Materials/materialmaster/materialmastersearch', null, 25, null, 'UserName', 'UserName');
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
    $scope.getMMUOMData = function (mmid) {
        baseService.init($scope.path + 'getmmuomlist?mmid=' + mmid, null, 25, null, 'UserName', 'UserName');
        $scope.loadMMUOMData = function (pageno) {//loadProcessData
            //$rootScope.parameters.CharacteristicsId = $scope.desMstCompanyWise.CompanyId;
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.mmUOMData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMMUOMlist) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMMUOMlist);
                    }
                    setUOMSelectionBlank();
                    //set  some id selected
                    for (var i = 0; i < baseService.arrayLength($scope.detailuomList); i++) {
                        if ($scope.detailuomList[i].Archive == false) {
                            var uid = $scope.detailuomList[i].UOMId;
                            setUOMSelected(uid);
                        }
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMMUOMData();
    }
    $scope.getCharacteristicsValueData = function (characteristicsid) {
        baseService.init('materials/characteristicsvalue/characteristicsvaluesearh', null, 25, null, 'Code', 'Code');
        //baseService.init($scope.path + 'getcharacteristicsvaluelist', null, 25, null, 'Description', 'Description');
        $scope.loadCharacteristicsValueData = function (pageno) {//loadProcessData
            $rootScope.parameters.CharacteristicsId = characteristicsid;
            $rootScope.parameters.ids = '';
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

    $scope.getMasterData = function (masterid, obj) {
        $http({
            method: 'GET',
            url: $scope.path + 'getlist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.masterList = [];
            $scope.masterList = response.data;
            // console.log($scope.bulletindetailList);
            if (baseService.arrayLength($scope.masterList) == 0) {
                //$scope.masterAdd('NEW');
            }
            else {
                $scope.master = $scope.masterList[0];
                $scope.loadAllUOMCmb();
                SetMMData(obj);
                selectedCharacteristicsConcate();
            }
        })
    }
    function setDetailButton() {
        if (baseService.arrayLength($scope.detailList) > 0) {
            $scope.IsDel = true;
            $scope.IsAdd = false;
            $scope.IsCharacteristicsbtnVisible = false;
        }
        else//as no child exists we can update master
        {
            $scope.IsCharacteristicsbtnVisible = true;
            $scope.IsDel = true;
            $scope.IsAdd = true;
        }
    }
    function loadDetail(masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist?masterid=' + masterid,
        }).then(function successCallback(response) {
            $scope.detailList = [];
            $scope.detailList = response.data;
            setDetailButton();
            if (baseService.arrayLength($scope.searchbyDetaillist) == 0) {
                baseService.getDDLSearchColumn(result.Rows, $scope.searchbyDetaillist);
            }
        })
    }
    $scope.getUOMData = function (detailid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getuomlist?detailid=' + detailid,
        }).then(function successCallback(response) {
            $scope.detailuomList = [];
            $scope.detailuomList = response.data;
            //console.log($scope.detailuomList);
            if ($scope.detailuomList.length == 0) {
                $scope.gridDetailGrid = false;
            }
            else {
                $scope.gridDetailGrid = true;
            }
        })
    }
    $scope.getUOMFactorData = function (detailid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getuomfactorlist?detailid=' + detailid,
        }).then(function successCallback(response) {
            $scope.detailfactorList = [];
            $scope.detailfactorList = response.data;
            // console.log($scope.bulletindetailList);
            if ($scope.detailfactorList.length > 0) {
                $scope.detailfactor = $scope.detailfactorList[0];
            }
            else {
                for (var v in $scope.detailfactor) {
                    $scope.detailfactor[v] = null;
                }
            }
            $scope.detailfactor.BaseUOM = $scope.master.BaseUOM;
            $scope.detailfactor.BaseUOMId = $scope.master.BaseUOMId;
        })
    }

    $scope.getDetailData = function (detailid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetail?id=' + detailid,
        }).then(function successCallback(response) {
            if (baseService.arrayLength(response.data) > 0) {
                ClearOb($scope.detail);//Characteristics1Value
                $scope.detail = response.data[0];
            }
            else {
                ClearOb($scope.detail);
                ShowResult("Detail data not found", 'Error');
            }
        })
    }
    ///**************************************************grid row selected event function*********************************
    $scope.createguid = function (prefix) {
        var d = new Date().getTime();
        d += (parseInt(Math.random() * 100)).toString();
        if (undefined === prefix) {
            prefix = 'uid-';
        }
        d = prefix + d;
        return d;
    };
    $scope.showCharacteristicsGrid = function (mmid) {
        var _result = false;
        if (mmid == null || mmid == '') {
            _result = false;
        }
        else {
            _result = true;
        }
        //console.log(mmid);
        return _result;
    }
    function GetselectCharacteristics() {
        $scope.master.Characteristics1Selected = false;
        $scope.master.Characteristics1 = '';
        $scope.master.Characteristics2Selected = false;
        $scope.master.Characteristics2 = '';
        $scope.master.Characteristics3Selected = false;
        $scope.master.Characteristics3 = '';
        //Characteristics1Id
        $scope.master.Characteristics1Id = '';
        $scope.master.Characteristics2Id = '';
        $scope.master.Characteristics3Id = '';

        var str = '';
        for (var i = 0; i < baseService.arrayLength($scope.CharacteristicsData); i++) {
            var iss = $scope.CharacteristicsData[i].IsSelected;
            var _sort = $scope.CharacteristicsData[i].Sort;
            // console.log(iss);
            if (iss) {
                if (str == '') {
                    str = $scope.CharacteristicsData[i].Alias;
                }
                else {
                    str += ', ' + $scope.CharacteristicsData[i].Alias;
                }
                if (_sort == '1') {
                    $scope.master.Characteristics1Selected = true;
                    $scope.master.Characteristics1 = $scope.CharacteristicsData[i].Alias;
                    $scope.master.Characteristics1Id = $scope.CharacteristicsData[i].Id;
                }
                else if (_sort == '2') {
                    $scope.master.Characteristics2Selected = true;
                    $scope.master.Characteristics2 = $scope.CharacteristicsData[i].Alias;
                    $scope.master.Characteristics2Id = $scope.CharacteristicsData[i].Id;
                }
                else if (_sort == '3') {
                    $scope.master.Characteristics3Selected = true;
                    $scope.master.Characteristics3 = $scope.CharacteristicsData[i].Alias;
                    $scope.master.Characteristics3Id = $scope.CharacteristicsData[i].Id;
                }
            }
        }
        return str;
    }

    $scope.selectCharacteristics = function () {
        ///load cmb for uom
        //$scope.loadAllUOMCmb();
        //console.log($scope.CharacteristicsData);
        $scope.master.SelectedCharacteristics = GetselectCharacteristics();
        $scope.masterIsChanged = true;
        angular.element(document.querySelector('#characteristicspopup')).modal('hide');
    }
    function selectedCharacteristicsConcate() {
        var str = '';

        var c1 = $scope.master.Characteristics1;
        var c2 = $scope.master.Characteristics2;
        var c3 = $scope.master.Characteristics3;
        // console.log(iss);
        //str = setVal(c1);
        //str += setVal(c2);
        //str += setVal(c3);

        if (baseService.isUndefinedOrNull(c1) == false) {
            if (str == '') {
                str = c1;
            }
            else {
                str += ', ' + c1;
            }
        }

        if (baseService.isUndefinedOrNull(c2) == false) {
            if (str == '') {
                str = c2;
            }
            else {
                str += ', ' + c2;
            }
        }

        if (baseService.isUndefinedOrNull(c3) == false) {
            if (str == '') {
                str = c3;
            }
            else {
                str += ', ' + c3;
            }
        }

        $scope.master.SelectedCharacteristics = str;
    }
    function setVal(c) {
        var str = '';
        if (c != null || c != "") {
            if (str == '') {
                str = c;
            }
            else {
                str += ', ' + c;
            }
        }
        return str;
    }
    function SetMMData(obj) {
        $scope.master.MaterialMasterId = obj.Id;
        $scope.master.Description = obj.Description;
        $scope.master.Code = obj.Code;
        $scope.master.UserName = obj.UserName;
        $scope.master.MaterialType = obj.MaterialType;
        $scope.master.MaterialGroup = obj.MaterialGroupMaster;
        $scope.master.GridNO = obj.GridName;
        $scope.master.MaterialGridId = obj.MaterialGridId;
        $scope.master.BaseUOM = obj.BaseUom;
        $scope.master.BaseUOMId = obj.BaseUOMId;
        $scope.detailfactor.BaseUOM = obj.BaseUom;
        $scope.detailfactor.BaseUOMId = obj.BaseUOMId;
    }
    $scope.getMMCode = function (obj) {
        $http({
            method: 'GET',
            url: $scope.path + 'getMasterId?materialmasterid=' + obj.Id,
        }).then(function successCallback(response) {
            $scope.masterIsChanged = false;
            $scope.masterAdd('NEW');
            if (response.data[0] == null) {//no data found for this mmid
                if (obj.MaterialGridId == null || obj.MaterialGridId == '') {//no grid id found so in blank mode
                    $scope.IsCharacteristicsbtnVisible = false;
                    ShowResult('This Material Master has no Grid ...', 'Information');
                }
                else {//grid id found so in add new mode
                    $scope.IsCharacteristicsbtnVisible = true;//SelectedCharacteristics
                    SetMMData(obj);
                    $scope.loadAllUOMCmb();
                }
            }
            else {//master id found against this mmid so in edit mode
                $scope.IsCharacteristicsbtnVisible = false;
                $scope.masterAdd('EDIT');
                var masterid = result[0].Id;
                $scope.getMasterData(masterid, obj);
                loadDetail(masterid);
                $scope.btnDetailEntryPopup = true;
            }
        })
        angular.element(document.querySelector('#mmmodal')).modal('hide');
    };
    function ClearOb(ob) {
        for (var i in ob) {
            ob[i] = null;
        }
    }
    $scope.clearMMCode = function () {
        //$scope.master.MaterialMasterId = null;
        //$scope.master.Description = null;
        //$scope.master.Code = null;
        //$scope.master.UserName = null;
        //$scope.master.MaterialType = null;
        //$scope.master.MaterialGroup = null;
        //$scope.master.MaterialGridId = null;
        //$scope.master.GridNO = null;
        //$scope.master.SelectedCharacteristics = null;
        //$scope.master.BaseUOM = null;
        //$scope.master.BaseUOMId = null;

        $scope.detailuomList = [];
        $scope.detailList = [];
        $scope.detailfactorList = [];

        ClearOb($scope.detailfactor);
        ClearOb($scope.detailuom);
        ClearOb($scope.detail);
        ClearOb($scope.master);
    };
    $scope.getCharacteristicsValueCode = function (id, Description) {
        if ($scope.dim == "1") {
            if (id == null || id == '') {
                $scope.detail.Characteristics1ValueId = null;
                $scope.detail.Characteristics1Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detail.Characteristics1ValueId = id;
                $scope.detail.Characteristics1Value = Description;
            }
        }
        else if ($scope.dim == "2") {
            if (id == null || id == '') {
                $scope.detail.Characteristics2ValueId = null;
                $scope.detail.Characteristics2Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detail.Characteristics2ValueId = id;
                $scope.detail.Characteristics2Value = Description;
            }
        }
        else if ($scope.dim == "3") {
            if (id == null || id == '') {
                $scope.detail.Characteristics3ValueId = null;
                $scope.detail.Characteristics3Value = null;
                // ShowResult('This Material Master has no Grid ...', 'Information');
            }
            else {
                $scope.detail.Characteristics3ValueId = id;
                $scope.detail.Characteristics3Value = Description;
            }
        }
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('hide');
    };
    $scope.clearCharacteristics1Value = function () {
        $scope.detail.Characteristics1ValueId = null;
        $scope.detail.Characteristics1Value = null;
    };
    $scope.clearCharacteristics2Value = function () {
        $scope.detail.Characteristics2ValueId = null;
        $scope.detail.Characteristics2Value = null;
    };
    $scope.clearCharacteristics3Value = function () {
        $scope.detail.Characteristics3ValueId = null;
        $scope.detail.Characteristics3Value = null;
    };
    function IsExist(UOMId) {
        var _result = false;
        for (var i = 0; i < baseService.arrayLength($scope.detailuomList); i++) {
            if (!$scope.detailuomList[i].Archive) {//if archive once it can b added aagain
                if ($scope.detailuomList[i].UOMId == UOMId) {
                    _result = true;
                    break;
                }
            }//uomid
        }
        return _result;
    }
    $scope.getUOMCode = function () {
        ClearOb($scope.detailuom);
        for (var i = 0; i < baseService.arrayLength($scope.mmUOMData); i++) {
            if ($scope.mmUOMData[i].IsSelectedID) {
                if (!IsExist($scope.mmUOMData[i].Id)) {
                    $scope.detailuomList.push(
                        {
                            Id: $scope.createguid('UOM'),
                            UOM: $scope.mmUOMData[i].UserName,
                            UOMCode: $scope.mmUOMData[i].Code,
                            UOMId: $scope.mmUOMData[i].Id,
                            CharacteristicsWisePropertiesMasterId: $scope.master.Id,
                            CharacteristicsWisePropertiesDetailId: $scope.detail.Id,
                            Archive: false
                        })
                }///IsExist
            }//selected
        }//for
        // console.log($scope.detailuomList);
        angular.element(document.querySelector('#UOMpopup')).modal('hide');
    };
    $scope.clearUOMCode = function () {
        $scope.detailuom.UOMId = null;
        $scope.detailuom.UOM = null;
    };
    $scope.masterAdd = function (AddEdit) {
        $scope.AddEdit = AddEdit;
        $scope.message_confirmation = "";

        if (AddEdit == 'NEW') {
            $scope.clearMMCode();
            $scope.IsDel = false;
            $scope.IsAdd = true;
            $scope.Action = "Save";
        }
        else {
            $scope.IsDel = true;
            $scope.IsAdd = true;
            $scope.Action = "Update";
            // $scope.IsCharacteristicsVisible = true;
        }
    }
    $scope.showAddEdit = function (AddEdit) {
        var _result = false;
        if (AddEdit == 'NEW') {
            _result = true;
        }
        else if (AddEdit == 'EDIT') {
            _result = true;
        }
        else {
            _result = false;
        }
        return _result;
    }

    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue == null || fieldValue == '') {
                throw (fieldName + ' is required...')
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckCha(cha, chav, chavid, isselected) {
        if (isselected) {
            if (chav == null || chav == '') {
                throw ('Value for ' + cha + ' can not be blank...')
            }
            if (chavid == null || chavid == '') {
                throw ('ValueID for ' + cha + ' is not found...')
            }
        }
    }
    function CheckUOM(list) {
        try {
            if (baseService.arrayLength(list) == 0) {
                throw ('No UOM is selected...')
            }
            else {
                var hasnovalue = false;
                for (var i = 0; i < baseService.arrayLength(list); i++) {
                    if (list[i].Archive == false) {
                        hasnovalue = true;
                        break;
                    }
                }
                if (!hasnovalue) {
                    throw ('No UOM is selected...')
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function HasUOM(list) {
        try {
            if (baseService.arrayLength(list) == 0) {
                return false;
            }
            else {
                var hasnovalue = false;
                for (var i = 0; i < baseService.arrayLength(list); i++) {
                    if (list[i].Archive == false) {
                        hasnovalue = true;
                        break;
                    }
                }
                if (!hasnovalue) {
                    return false;
                }
                else {
                    return true;
                }
            }
        } catch (e) {
            throw e;
        }
    }
    function ValidationMaster() {
        try {
            CheckField($scope.master.MaterialMasterId, 'Material Master');
            CheckField($scope.master.BaseUOM, 'Base Uom');
            CheckField($scope.master.BaseUOMId, 'Base UomId');

            if (($scope.master.Characteristics1 == null || $scope.master.Characteristics1 == "") && ($scope.master.Characteristics2 == null || $scope.master.Characteristics2 == "") && ($scope.master.Characteristics3 == null || $scope.master.Characteristics3 == "")) {
                throw "Minimum One Characteristics should be selected...";
            }
            ///Check characteristics
            //CheckCha($scope.master.Characteristics1, $scope.master.Characteristics1Value, $scope.master.Characteristics1ValueId, $scope.master.Characteristics1Selected);
            //CheckCha($scope.master.Characteristics2, $scope.master.Characteristics2Value, $scope.master.Characteristics2ValueId, $scope.master.Characteristics2Selected);
            //CheckCha($scope.master.Characteristics3, $scope.master.Characteristics3Value, $scope.master.Characteristics3ValueId, $scope.master.Characteristics3Selected);
            ///check uomFactor
            //if ($scope.detailfactor.AlternativeUOMId != null && $scope.detailfactor.AlternativeUOMId!='') {
            //    CheckField($scope.detailfactor.AlternativeUOMFactor, 'Alternative UOM Factor');
            //    CheckField($scope.detailfactor.AlternativeUOMId, 'Alternative UOM');
            //    CheckField($scope.detailfactor.BaseUOMFactor, 'Base UOM Factor');

            //    if ($scope.detailfactor.AlternativeUOMId == $scope.detailfactor.BaseUOMId) {
            //        throw '[Alternative UOM] and [Base UOM] can not be same...';
            //    }
            //}
            //else
            //{
            //    ///UOM
            //    CheckUOM($scope.detailuomList);
            //}

            //$scope.detailfactorList = [];
            //$scope.detailfactorList.push($scope.detailfactor);
        } catch (e) {
            throw e;
        }
    }
    function ValidationDetail() {
        try {
            CheckField($scope.master.Id, 'MasterId');

            CheckCha($scope.master.Characteristics1, $scope.detail.Characteristics1Value, $scope.detail.Characteristics1ValueId, $scope.master.Characteristics1Selected);
            CheckCha($scope.master.Characteristics2, $scope.detail.Characteristics2Value, $scope.detail.Characteristics2ValueId, $scope.master.Characteristics2Selected);
            CheckCha($scope.master.Characteristics3, $scope.detail.Characteristics3Value, $scope.detail.Characteristics3ValueId, $scope.master.Characteristics3Selected);
            ///check uomFactor
            if ($scope.detailfactor.AlternativeUOMId != null && $scope.detailfactor.AlternativeUOMId != '') {
                CheckField($scope.detailfactor.AlternativeUOMFactor, 'Alternative UOM Factor');
                CheckField($scope.detailfactor.AlternativeUOMId, 'Alternative UOM');
                CheckField($scope.detailfactor.BaseUOMFactor, 'Base UOM Factor');

                if ($scope.detailfactor.AlternativeUOMId == $scope.detailfactor.BaseUOMId) {
                    throw '[Alternative UOM] and [Base UOM] can not be same...';
                }
            }
            else {
                ///UOM
                CheckUOM($scope.detailuomList);
            }
            $scope.detail.CharacteristicsWisePropertiesMasterId = $scope.master.Id;
            $scope.detailfactorList = [];
            if ($scope.detailfactor.BaseUOMFactor != null && $scope.detailfactor.BaseUOMFactor != "") {
                $scope.detailfactorList.push($scope.detailfactor);
            }

            CheckDetailDuplicate($scope.detailList, $scope.detail)
        } catch (e) {
            throw e;
        }
    }
    function CheckDetailDuplicate(list, ob) {
        for (var i = 0; i < baseService.arrayLength(list); i++) {
            var cv1 = ob.Characteristics1ValueId;
            var cv2 = ob.Characteristics2ValueId;
            var cv3 = ob.Characteristics3ValueId;

            var _cv1 = ob.Characteristics1Value;
            var _cv2 = ob.Characteristics2Value;
            var _cv3 = ob.Characteristics3Value;

            if (list[i].Id != ob.Id && list[i].Characteristics1ValueId == cv1 && list[i].Characteristics2ValueId == cv2 && list[i].Characteristics3ValueId == cv3) {
                var v1 = SetValue($scope.master.Characteristics1, _cv1);
                var v2 = SetValue($scope.master.Characteristics2, _cv2);
                var v3 = SetValue($scope.master.Characteristics3, _cv3);

                var msg = CreateMessage(v1, v2, v3);
                throw msg + " has already been taken...";
                //throw $scope.master.Characteristics1 + " :[" + _cv1 + "], " + $scope.master.Characteristics2 + " :[" + _cv2 + "], " + $scope.master.Characteristics3 + " :[" + _cv3 + "] has already been taken...";
                //throw SetValue($scope.master.Characteristics1, _cv1) + $scope.master.Characteristics2 + " :[" + _cv2 + "], " + $scope.master.Characteristics3 + " :[" + _cv3 + "] has already been taken...";
            }//if
        }//for
    }
    function CreateMessage(v1, v2, v3) {
        var msg = "";
        if (v1 != "") {
            msg = v1;
        }
        if (v2 != "") {
            if (msg != "") {
                msg += ", " + v2;
            }
            else {
                msg = v2;
            }
        }
        if (v3 != "") {
            if (msg != "") {
                msg += ", " + v3;
            }
            else {
                msg = v3;
            }
        }
        return msg;
    }
    function SetValue(c, cv) {
        // var val = $scope.master.Characteristics1 + " :[" + _cv + "], ";
        if (c != null && c != "") {
            return c + " :[" + cv + "]";
        }
        else {
            return "";
        }
    }
    ///**************************************************save delete and clear function*********************************
    $scope.Save = function () {
        try {
            ValidationMaster();
            $scope.btnDisabledSave = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createmaster',
                // data: { master: $scope.master, uomlist: $scope.detailuomList, uomfactorlist: $scope.detailfactorList },
                data: { master: $scope.master },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.btnDisabledSave = false;
                    $scope.masterIsChanged = false;
                    $scope.master.Id = data.id;
                    ShowResult(response.data.Message, 'success');
                    //$scope.getMasterData(data.id)
                    //$scope.masterAdd('NEW');
                    $scope.masterAdd('Edit');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.SaveDetail = function () {
        try {
            ValidationDetail();
            $scope.btnDisabledSave = true;
            $http({
                method: 'POST',
                url: $scope.path + 'createdetail',
                data: { detail: $scope.detail, uomlist: $scope.detailuomList, uomfactorlist: $scope.detailfactorList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.btnDisabledSave = false;
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    loadDetail($scope.master.Id);
                    //$scope.getMasterData(data.id)
                    //$scope.masterAdd('NEW');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.DeleteMaster = function () {
        try {
            if ($scope.master == null || $scope.master.Id == null || $scope.master.Id == '') {
                throw ('No ID found to be deleted...')
            }
            $http({
                method: 'POST',
                url: $scope.path + 'deletemaster',
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                    $scope.masterIsChanged = false;
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.masterAdd('NEW');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    }
    $scope.DeleteDetail = function () {
        try {
            if ($scope.detail == null || $scope.detail.Id == null || $scope.detail.Id == '') {
                throw ('No ID found to be deleted...')
            }
            $http({
                method: 'POST',
                url: $scope.path + 'deletedetail',
                dataType: 'JSON',
                data: { 'detailid': $scope.detail.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    loadDetail($scope.master.Id)
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
    $scope.ClearMasterModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = null;
        }
    }
    $scope.ClearDetailModal = function () {
        for (var i in $scope.detailmodal) {
            $scope.detailmodal[i] = null;
        }
    }
    $scope.ClearMaster = function () {
        for (var i in $scope.master) {
            $scope.master[i] = null;
        }
    }
    $scope.ClearDetail = function () {
        for (var i in $scope.detailuom) {
            $scope.detailuom[i] = null;
        }
    }
    $scope.ModalToMainPage = function () {
        for (var i in $scope.master) {
            $scope.master[i] = $scope.mastermodal[i];
        }
    }
    $scope.CancelDetail = function () {
        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
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

    $scope.showDetailGrid = function (detailuomList) {
        var _result = false;
        if (detailuomList == null) {
            _result = false;
        }
        else {
            if (baseService.arrayLength(detailuomList) > 0) {
                _result = true;
            }
            else {
                _result = false;
            }
        }
        //console.log(mmid);
        return _result;
    }

    ///common function ends-------------------------------------------------------------------------------------------------
    ///3.customised function------------------------------------------------------------------------------------------------

    ///**************************************************show modal*********************************
    $scope.searchCharacteristics3Value = function (cvid) {
        $scope.dim = "3";
        $scope.getCharacteristicsValueData(cvid);
        $scope.CharacteristicsSearchTitle = $scope.master.Characteristics3;
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics2Value = function (cvid) {
        $scope.dim = "2";
        $scope.getCharacteristicsValueData(cvid);
        $scope.CharacteristicsSearchTitle = $scope.master.Characteristics2;
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };
    $scope.searchCharacteristics1Value = function (cvid) {
        $scope.dim = "1";
        $scope.getCharacteristicsValueData(cvid);
        $scope.CharacteristicsSearchTitle = $scope.master.Characteristics1;
        angular.element(document.querySelector('#characteristicsValuepopup')).modal('show');
    };

    $scope.uomPopup = function () {
        $scope.getMMUOMData($scope.master.MaterialMasterId);
        angular.element(document.querySelector('#UOMpopup')).modal('show');
    };
    $scope.characteristicsPopup = function () {
        $scope.loadCharacteristics($scope.master.MaterialMasterId);
        angular.element(document.querySelector('#characteristicspopup')).modal('show');
    };

    $scope.masterSearchPopup = function () {
        $scope.getData();
        angular.element(document.querySelector('#mastersearchpopup')).modal('show');
    };
    $scope.showMMModal = function () {
        $scope.getMMData();
        angular.element(document.querySelector('#mmmodal')).modal('show');
    };

    function ClearDetail() {
        ClearOb($scope.detail);

        $scope.detailfactorList = [];
        ClearOb($scope.detailfactor);

        $scope.detailuomList = [];
        ClearOb($scope.detailuom);

        $scope.detailfactor.BaseUOM = $scope.master.BaseUOM;
        $scope.detailfactor.BaseUOMId = $scope.master.BaseUOMId;
    }

    $scope.detailEntryPopup = function (flag) {
        try {
            if ($scope.master.Id == null || $scope.master == "") {
                throw ("Select a 'Master' first....");
            }
            if ($scope.masterIsChanged) {
                throw ("Master data is changed and needed to be saved first....");
            }

            if (flag == 'NEW') {
                $scope.ActionDetail = 'Save';
                ClearDetail();
            }
            else {
                //console.log('4.0', $scope.detail);
                $scope.getDetailData($scope.detail.Id);
                $scope.getUOMFactorData($scope.detail.Id);
                $scope.getUOMData($scope.detail.Id);
                $scope.gridDetailGrid = true;
                $scope.ActionDetail = 'Update';
                // console.log('4',$scope.detail);
            }
            angular.element(document.querySelector('#detailentrypopup')).modal('show');
        } catch (e) {
            ShowResult(e, "Error")
        }
    };
    $scope.getDetailRow = function (id) {
        if (id == null || id == "") {
            throw ("Select a 'Detail' first....");
        }

        $scope.detail.Id = id;
        //console.log('4.-1', $scope.detail);
        $scope.detailEntryPopup('EDIT');
    }

    $scope.deleteMasterPopup = function () {
        var _id = $scope.master.Id;
        $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
        angular.element(document.querySelector('#confirmasterdelete')).modal('show');
    }
    $scope.removeMasterYes = function () {
        $scope.DeleteMaster();
    };
    $scope.removeDetailYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#confirdetaildelete')).modal('hide');
    };
    $scope.deleteDetailPopup = function (id) {
        $scope.detail.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
        angular.element(document.querySelector('#confirdetaildelete')).modal('show');
    }
    $scope.deleteUOMPopup = function (id, uom) {
        $scope.detailuom.Id = id;
        $scope.message_confirmation = "Are you sure to delete [" + uom + "] ";
        angular.element(document.querySelector('#confiruomdelete')).modal('show');
    }
    $scope.removeUOMYes = function () {
        for (var i = 0; i < baseService.arrayLength($scope.detailuomList); i++) {
            if ($scope.detailuomList[i].Id == $scope.detailuom.Id) {
                $scope.detailuomList[i].Archive = true;
            }
        }
        angular.element(document.querySelector('#confiruomdelete')).modal('hide');
    };
    //For detailuom

    $scope.childdeleteId = '';

    ///3.loadtime call******************************************************************************************************
    ///service
    //$scope.loadAllUOMCmb();
    baseService.init($scope.getListUrl, null, 25, null, 'Process', 'Process');
    $scope.masterAdd('NEW');
    ///function
    ///loadtime call ends***************************************************************************************************
};
CharacteristicsWisePropertiesController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];