'use strict';
SubsectionStructureController.$inject = ['cboService', "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
function SubsectionStructureController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    ///----------------------------------------------------------------------------------------------------------------------
    ///1.declaration
    ///2.function
    ///3.loadtime call
    ///
    ///------------------------------------------------------------------------SubsectionStructure-----------------------------------------------
    ///1.declaration----------------------------------------------------------------------------------------------------------
    ///variable
    $rootScope.title = "Subsection Structure";
    $scope.Action = 'Save';
    $scope.gridDetailGrid = false;
    $scope.btnDetailEntryPopup = false;
    $scope.btndeletemaster = true;
    $scope.isdeletedetail = false;
    $scope.message_confirmation = "";
    $scope.ActionDetail = 'Save';//SaveDetailDisabled
    $scope.SaveDetailDisabled = false;//DeleteMaster

    $scope.path = 'Organizations/subsectionstructure/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrlMaster = $scope.path + 'createmaster';
    $scope.saveUrlDetail = $scope.path + 'createdetail';
    $scope.deleteUrlmaster = $scope.path + 'deletemaster';
    $scope.deleteUrlDetail = $scope.path + 'deletedetail';
    ///list
    $scope.searchbyDetaillist = [];
    $scope.searchbyMasterlist = [];

    $scope.masterList = [];
    $scope.detailList = [];
    $scope.processList = [];
    $scope.companyList = [];
    $scope.plantList = [];
    $scope.unitList = [];

    $scope.departmentList = [];
    $scope.lineList = [];
    $scope.subsectionList = [];
    $scope.sectionList = [];
    $scope.divisionList = [];

    $scope.Data = [];
    $scope.detail = {
        Id: null,
        SubsectionStructureMasterId: null,
        SubsectionId: null,
        SectionId: null,
        LineId: null,
        DivisionId: null,
        DepartmentId: null,
        Archive: false
    };
    $scope.detailmodal = {
        Id: null,
        SubsectionStructureMasterId: null,
        SubsectionId: null,
        SectionId: null,
        LineId: null,
        DivisionId: null,
        DepartmentId: null,
        Archive: false
    };
    $scope.master = {
        Id: null,
        Description: null,
        Code: null,
        LunchStartTime: null,
        ProcessId: null,
        Process: null,
        LunchEndTime: null,
        StartTime: null,
        Plant: null,
        PlantId: null,
        Unit: null,
        UnitId: null,
        CompanyId: null,
        Company: null,
        ApplicableForProduction: false,
        ApplicableForWIP: false,
        ApplicableForIncentive: false,
        ApplicableForBulletin: false,
        Sequence: null,
        Archive: false
    };
    $scope.mastermodal = {
        Id: null,
        Description: null,
        Code: null,
        LunchStartTime: null,
        ProcessId: null,
        Process: null,
        LunchEndTime: null,
        StartTime: null,
        Plant: null,
        PlantId: null,
        Unit: null,
        UnitId: null,
        CompanyId: null,
        Company: null,
        ApplicableForProduction: false,
        ApplicableForWIP: false,
        ApplicableForIncentive: false,
        ApplicableForBulletin: false,
        Sequence: null,
        Archive: null
    };

    ///other
    $scope.index = -1;
    $scope.masterindex = -1;
    $scope.detailindex = -1;
    ///declaration ends-----------------------------------------------------------------------------------------------------
    ///2.function----------------------------------------------------------------------------------------------------

    ///**************************************************get data from database*********************************

    $scope.loadPlant = function (companyId) {
        try {
            cboService.getCboPlantByCompany(companyId, function (result) {
                $scope.plantList = result;
            });

            cboService.getCboUnitByCompany(companyId, function (result) {
                $scope.unitList = result;
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
            cboService.getCboCompanyByCompanyGroup(null, function (result) {
                $scope.companyList = result;
            });

            //$scope.loadPlant();

            //$http.get($scope.path + "getprocesscbo?companyId=" + $scope.master.CompanyId)
            //  .then(function (response) {
            //      $scope.processList = response.data;
            //  });
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

    $scope.loadDDLDetail = function () {
        try {
            cboService.getCboDepartmentByCompany($scope.master.CompanyId, function (result) {
                $scope.departmentList = result;
            });

            //$scope.loadPlant();
            $http.get($scope.path + "getlinelistcbo?CompanyId=" + $scope.master.CompanyId)
                .then(function (response) {
                    $scope.lineList = response.data;
                    //console.log(response);
                });
            $http.get($scope.path + "getsubsectionlistcbo?CompanyId=" + $scope.master.CompanyId)
                .then(function (response) {
                    $scope.subsectionList = response.data;
                });
            $http.get($scope.path + "getsectionlistcbo?CompanyId=" + $scope.master.CompanyId)
                .then(function (response) {
                    $scope.sectionList = response.data;
                });
            $http.get($scope.path + "getdivisionlistcbo?CompanyId=" + $scope.master.CompanyId)
                .then(function (response) {
                    $scope.divisionList = response.data;
                    //console.log(response);
                });
            $http.get($scope.path + "getsubdivisionlistcbo?CompanyId=" + $scope.master.CompanyId)
                .then(function (response) {
                    $scope.subdivisionList = response.data;
                    //console.log(response);
                });
        } catch (e) {
            ShowResult(e, "Error");
        }
    };

    $scope.getData = function () {
        baseService.init($scope.path + 'getlist', null, 25, null, 'Sequence', 'Description');
        $scope.loadMasterData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.masterList = result.Rows;
                    if (baseService.arrayLength($scope.searchbyMasterlist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyMasterlist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadMasterData();
    };
    $scope.getProcessData = function () {
        baseService.init($scope.path + 'GetList', null, 25, null, 'Sequence', 'ProcessCode');
        $scope.loadProcessData = function (pageno) {//loadProcessData
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.processData = result.Rows;
                    if (baseService.arrayLength($scope.searchbyProcessDatalist) === 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.searchbyProcessDatalist);
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.loadProcessData();
    };
    $scope.getDetailData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getdetaillist?masterid=' + masterid
        }).then(function successCallback(response) {
            $scope.detailList = [];
            $scope.detailList = response.data;
            if (baseService.arrayLength($scope.searchbyDetaillist) === 0) {
                baseService.getDDLSearchColumn(response.data, $scope.searchbyDetaillist);
            }
        });
    };
    $scope.getMasterData = function (masterid) {
        $http({
            method: 'GET',
            url: $scope.path + 'getmasterlist?masterid=' + masterid
        }).then(function successCallback(response) {
            $scope.masterList = [];
            $scope.masterList = response.data;
            if (baseService.arrayLength($scope.masterList) > 0) {
                $scope.master = $scope.masterList[0];
                //show add detail button
                if ($scope.master.Id !== null && $scope.master.Id.length > 0) {//add edit
                    $scope.btnDetailEntryPopup = true;
                }//not null
            }//if length>0
        });//success
    };

    ///**************************************************grid row selected event function*********************************
    $scope.ApplicableFor = "";
    $scope.getProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = id;
        $scope.mastermodal.Process = code;
        angular.element(document.querySelector('#processmodal')).modal('hide');
    };
    $scope.clearProcessCode = function (id, code) {
        $scope.mastermodal.ProcessId = null;
        $scope.mastermodal.Process = null;
    };
    $scope.GetMasterIndex = function (id, index) {
        $scope.masterindex = index;
        $scope.ApplicableFor = "";
        $scope.master = $scope.masterList[$scope.masterindex];
        $scope.ApplicableFor = GetApplicableFor($scope.master);
        //console.log($scope.master);
        $scope.getDetailData($scope.master.Id);
        $scope.btnDetailEntryPopup = true;
        // $scope.bulletinmastermodal = $scope.bulletinmasterList[$scope.masterindex];
        angular.element(document.querySelector('#mastersearchpopup')).modal('hide');
    };
    function GetApplicableFor(ob) {
        var _af = "";
        if (ob.ApplicableForBulletin) {
            _af = "Bulletin";
        }

        if (ob.ApplicableForIncentive) {
            if (_af === "") {
                _af = "Incentive";
            }
            else {
                _af += ", Incentive";
            }
        }

        if (ob.ApplicableForProduction) {
            if (_af === "") {
                _af = "Production";
            }
            else {
                _af += ", Production";
            }
        }

        if (ob.ApplicableForWIP) {
            if (_af === "") {
                _af = "WIP";
            }
            else {
                _af += ", WIP";
            }
        }

        return _af;
    }
    function CheckField(fieldValue, fieldName) {
        try {
            if (fieldValue === null || fieldValue === '') {
                throw '[' + fieldName + '] is required...';
            }
        } catch (e) {
            throw e;
        }
    }
    function CheckFieldTime(fieldValue, fieldName) {
        try {
            CheckField(fieldValue, fieldName);
            if (fieldValue.length !== 5) {
                throw fieldName + ' is not correct format...Ex: 08:00, 15:30 (HH:mm)';
            }
            if (fieldValue.substr(2, 1) !== ':') {
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

            if (a === 0 && b === 0) {
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
            CheckField($scope.mastermodal.CompanyId, 'Company');
            CheckField($scope.mastermodal.PlantId, 'Plant');
            CheckField($scope.mastermodal.Sequence, 'Sequence');
            CheckField($scope.mastermodal.Code, 'Code');
            CheckField($scope.mastermodal.UnitId, 'Unit');
            CheckField($scope.mastermodal.Description, 'Description');
            CheckField($scope.mastermodal.ProcessId, 'Process');
            CheckFieldTime($scope.mastermodal.StartTime, 'Start Time');
            CheckFieldTime($scope.mastermodal.LunchStartTime, 'Lunch Start Time');
            CheckFieldTime($scope.mastermodal.LunchEndTime, 'Lunch End Time');
        } catch (e) {
            throw e;
        }
    }
    function ValidationDetail() {
        try {
            CheckField($scope.detailmodal.DivisionId, 'Division');
            CheckField($scope.detailmodal.DepartmentId, 'Department');
            CheckField($scope.detailmodal.SectionId, 'Section');
            CheckField($scope.detailmodal.SubsectionId, 'Subsection');
            CheckField($scope.detailmodal.LineId, 'Line');

            CheckDuplicate($scope.detailmodal);
        } catch (e) {
            throw e;
        }
    }
    function CheckDuplicate(ob) {
        try {
            for (var i = 0; i < baseService.arrayLength($scope.detailList); i++) {
                if (ob.Id !== $scope.detailList[i].Id) {
                    if ($scope.detailList[i].DivisionId === ob.DivisionId && $scope.detailList[i].DepartmentId === ob.DepartmentId && $scope.detailList[i].SectionId == ob.SectionId && $scope.detailList[i].SubsectionId == ob.SubsectionId && $scope.detailList[i].LineId == ob.LineId) {
                        throw 'Same Combination already exists...';
                    }
                }//id
            }
        } catch (e) {
            throw e;
        }
    }

    ///**************************************************save delete and clear function*********************************
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.ModalToMainPage();
            $http({
                method: 'POST',
                url: $scope.saveUrlMaster,
                dataType: 'JSON',
                data: { 'master': $scope.master }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    //get data by id
                    $scope.getMasterData(response.data.id);
                    //hide master entry modal
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    //update time change the button text from update to save
                    if ($scope.Action != 'Save') {
                        $scope.Action = 'Save';
                    }
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.SaveDetail = function () {
        try {
            ValidationDetail();
            $scope.detailmodal.SubsectionStructureMasterId = $scope.master.Id;
            for (var i in $scope.detailmodal) {
                $scope.detail[i] = $scope.detailmodal[i];
            }
            console.log($scope.detail);
            $scope.SaveDetailDisabled = true;
            $http({
                method: 'POST',
                url: $scope.saveUrlDetail,
                dataType: 'JSON',
                data: { 'detail': $scope.detail }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    ShowResult(response.data.Message, 'success');
                    $scope.getDetailData($scope.master.Id);
                    $scope.gridDetailGrid = true;
                    //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
                    $scope.SaveDetailDisabled = false;
                    if ($scope.ActionDetail != 'Save') {
                        $scope.ActionDetail = 'Save';
                    }
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        //var fdata = new FormData();
        //fdata.append('file', $scope.filedata);
        //fdata.append('fileName', "99");
        //fdata.append("model", angular.toJson(data.model));
        //fdata.append('Id', "20161");
        //data: { 'glGeneralInfo': $scope.glinfo, 'glCompanyInfo': $scope.newGlcominfo, 'glAccountType': $scope.glaccounttypies },
        if ($scope.Action == 'Save') {
            $http({
                method: 'POST',
                url: $scope.saveUrlMaster,
                withCredentials: true,
                processData: false,
                //headers: { 'Content-Type': undefined },
                //dataType: 'JSON',
                data: { 'file': $scope.filedata, 'cm': $scope.timecapture, 'detail': $scope.fromToTable },
                transformRequest: angular.identity
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //$scope.SaveChild();
                    //ShowResult(data.Message, 'success');
                    //$scope.timecaptureList.push(data.TimeCapture);
                    //baseService.paginationAdd();
                    //ClearFields(data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
        else if ($scope.Action == 'Update') {
            $http({
                method: 'POST',
                url: $scope.updateUrl,
                data: $scope.timeCapture,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.index > -1) {
                        $scope.timeCaptureList[$scope.index] = $scope.timecapture;
                    }
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        }
        //}
    };
    $scope.DeleteMaster = function () {
        try {
            $scope.master.Id = $scope.mastermodal.Id;
            if ($scope.master.Id == null || $scope.master.Id == '') {
                throw 'No Subsection Structure is found...';
            }
            $http({
                method: 'POST',
                url: $scope.deleteUrlmaster,
                dataType: 'JSON',
                data: { 'masterid': $scope.master.Id }
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
                    $scope.masterAddEditPopup('DELETE');
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
            return true;
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };
    $scope.DeleteDetail = function () {
        $scope.detailmodal.SubsectionStructureMasterId = $scope.master.Id;
        //$scope.detail.Id = $scope.detailmodal.Id;
        $scope.detail.Id = $scope.detailid_delete;
        $http({
            method: 'POST',
            url: $scope.deleteUrlDetail,
            dataType: 'JSON',
            data: { 'detailid': $scope.detail.Id }
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //other child
                $scope.getDetailData($scope.detailmodal.SubsectionStructureMasterId);
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });
        return true;
    };

    $scope.MainPageToModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = $scope.master[i];
        }
    };
    $scope.ClearMasterModal = function () {
        for (var i in $scope.mastermodal) {
            $scope.mastermodal[i] = null;
        }
    };

    $scope.ClearDetailModal = function () {
        for (var i in $scope.detailmodal) {
            $scope.detailmodal[i] = null;
        }
    };

    $scope.ClearMaster = function () {
        for (var i in $scope.master) {
            $scope.master[i] = null;
        }
    };

    $scope.ClearDetail = function () {
        for (var i in $scope.detail) {
            $scope.detail[i] = null;
        }
    };

    $scope.ModalToMainPage = function () {
        for (var i in $scope.master) {
            $scope.master[i] = $scope.mastermodal[i];
        }
    };
    $scope.CancelDetail = function () {
        angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
    };

    $scope.getPlantCompanyWise = function () {
        try {
            if ($scope.mastermodal.CompanyId.length == 0) {
                throw "Select Company first...";
            }
            $scope.loadPlant($scope.mastermodal.CompanyId);

            $http.get($scope.path + "getprocesscbo?companyId=" + $scope.mastermodal.CompanyId)
                .then(function (response) {
                    $scope.processList = response.data;
                });
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.masterAddEditPopup = function (flag) {
        try {
            if (flag == 'NEW') {
                //console.log($scope.path);
                //$scope.isdeletedetail = false;
                $scope.btndeletemaster = false;
                $scope.gridDetailGrid = false;
                $scope.btnDetailEntryPopup = false;
                $scope.Action = 'Save';
                $scope.ClearMasterModal();
                $scope.ClearMaster();
                $scope.detailList = [];
                $scope.loadDDL();
                $scope.loadSequence();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('show');
            }
            else if (flag == 'DELETE') {
                $scope.btndeletemaster = false;
                $scope.gridDetailGrid = false;
                $scope.btnDetailEntryPopup = false;
                $scope.Action = 'Save';
                $scope.ClearMasterModal();
                $scope.ClearMaster();
                $scope.detailList = [];
                $scope.loadDDLDetail();
                angular.element(document.querySelector('#masteraddeditpopup')).modal('hide');
            }
            else {
                $scope.loadDDL();
                //$scope.loadDDLDetail();
                $scope.loadPlant($scope.master.CompanyId);
                //$scope.isdeletedetail = true;
                $scope.btndeletemaster = true;
                $scope.Action = 'Update';
                $scope.MainPageToModal();
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
    $scope.showProcessModal = function () {
        $scope.getProcessData();
        angular.element(document.querySelector('#processmodal')).modal('show');
    };

    $scope.detailEntryPopup = function (flag) {
        if ($scope.master.Id == null || $scope.master == "") {
            ShowResult("Select a 'Master' first....");
            return;
        }
        $scope.detailindex = -1;
        $scope.SaveDetailDisabled = false;
        $scope.CancelDetail();
        $scope.loadDDLDetail();
        if (flag == 'NEW') {
            $scope.detailmodal = Object.assign({}, $scope.detail);
            for (var i in $scope.detailmodal) {
                $scope.detailmodal[i] = null;
            }
            //console.log($scope.bulletindetail);
            // console.log($scope.bulletindetailmodal);
            $scope.ActionDetail = 'Save';
        }
        else {
            $scope.ActionDetail = 'Update';
        }
        $scope.loadDDL();
        angular.element(document.querySelector('#detailentrypopup')).modal('show');
    };

    $scope.deleteMaster = function () {
        var _id = $scope.mastermodal.Id;
        $scope.message_confirmation = "Are you sure to delete [" + _id + "] ";
        angular.element(document.querySelector('#confirmmasterdelete')).modal('show');
        //$rootScope.passValue(_id, $scope.masterindex);
    };

    $scope.removeMasterYes = function () {
        angular.element(document.querySelector('#confirmmasterdelete')).modal('hide');
        $scope.DeleteMaster();
    };
    $scope.removeRowYes = function () {
        //$scope.DeleteDetail();
        //angular.element(document.querySelector('#detailentrypopup')).modal('hide');
    };

    $scope.detailid_delete = null;
    $scope.deleteDetailGrid = function (id) {
        try {
            if (baseService.isUndefinedOrNull(id)) {
                throw "Select a Subsection Structure...";
            }

            $scope.detailid_delete = id;
            $scope.message_confirmation = "Are you sure to delete [" + id + "] ";
            angular.element(document.querySelector('#confirmdetaildelete')).modal('show');
        } catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeDetailYes = function () {
        $scope.DeleteDetail();
        angular.element(document.querySelector('#confirmdetaildelete')).modal('hide');
    };

    //$scope.deleteDetailModal = function (index) {
    //    $scope.message_confirmation = "Are you sure to delete [" + $scope.detailmodal.Id + "] ";
    //   // $rootScope.passValue($scope.detailmodal.Id, index);
    //}
    //For Detail
    $scope.getDetailRow = function (index) {
        $scope.detailEntryPopup('EDIT');
        $scope.detailindex = index;
        $scope.detail = $scope.detailList[$scope.detailindex];
        $scope.detailmodal = Object.assign({}, $scope.detail);
    };

    ///3.loadtime call******************************************************************************************************
    ///service
    baseService.init($scope.getListUrl, null, 25, null, 'Process', 'Process');

    ///function
    ///loadtime call ends***************************************************************************************************
}