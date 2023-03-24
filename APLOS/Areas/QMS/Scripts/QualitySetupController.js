'use strict';
QualitySetupController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QualitySetupController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Quality Setup';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    // #region QMS Activity Category
    $rootScope.titleAC = 'QMS Activity Category';
    $scope.ActionAC = 'Save';
    $scope.ACModelList = [];
    $scope.acpath = 'QMS/QMSActivityCategory/';
    $scope.getListUrlAC = $scope.acpath + 'getlist';
    $scope.getSeqUrlAC = $scope.acpath + 'getautosequence';
    $scope.saveUrlAC = $scope.acpath + 'create';
    $scope.deleteUrlAC = $scope.acpath + 'delete/';
    baseService.init($scope.getListUrlAC);
    $scope.searchByAC = "UserName"; $scope.searchAC = "";
    $scope.searchByListAC = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];
    

    $scope.getDataAC = function () {
        $http({
            method: 'POST',
            url: $scope.acpath + "GetList",
            data: { column: $scope.searchByAC, value: $scope.searchAC },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ACModelList = response.data;
            ClearFieldsAC(response.data.Sequence);
            $scope.GetSequenceAC();
        });
    }
    $scope.getDataAC();

    $scope.ModelTempAC = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ACModelNew = Object.assign({}, $scope.ModelTempAC);

    $scope.GetSequenceAC = function () {
        cboService.getSequence($scope.getSeqUrlAC, function (data) {
            $scope.ModelTempAC.Sequence = data;
            $scope.ACModelNew.Sequence = data;
        });
    };
    $scope.GetSequenceAC();

    $scope.GetAC = function (args) {

        $scope.ACModelNew = Object.assign({}, args.data);
        $scope.ActionAC = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveAC = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ACModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlAC,
                data: { 'data': $scope.ACModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsAC(response.data.Sequence);
                    $scope.getDataAC();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteAC = function () {
        if (!baseService.isUndefinedOrNull($scope.ACModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlAC + $scope.ACModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsAC(response.data.Sequence);
                    $scope.getDataAC();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.message_Detailconfirmation = null;
    $scope.RemoveDetailConsumptionMatrix = function () {
       
        if (!baseService.isUndefinedOrNull($scope.ACModelNew.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpAC')).modal('show');
    }

    $scope.ClearAC = function () {
        ClearFieldsAC($scope.GetSequenceAC());
        return true;
    };

    function ClearFieldsAC(seq) {
        $scope.ActionAC = 'Save';
        $scope.ACModelNew = Object.assign({}, $scope.ModelTempAC);
        $scope.ACModelNew.Sequence = seq;
    }
    // #endregion QMS Activity Category

    // #region Quality Activity Check Type
    $rootScope.titleACT = 'Quality Activity Check Type';
    $scope.ModelListACT = [];
    $scope.ACTPath = 'QMS/QualityActivityCheckType/';
    $scope.getListUrlACT = $scope.ACTPath + 'getlist';
    $scope.getSeqUrlACT = $scope.ACTPath + 'getautosequence';
    $scope.saveUrlACT = $scope.ACTPath + 'create';
    $scope.deleteUrlACT = $scope.ACTPath + 'delete/';
    baseService.init($scope.getListUrlACT);
    $scope.searchByACT = "UserName"; $scope.searchACT = "";
    $scope.searchByListACT = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataACT = function () {
        $http({
            method: 'POST',
            url: $scope.ACTPath + "GetList",
            data: { column: $scope.searchByACT, value: $scope.searchACT },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListACT = response.data;
        });
    }
    $scope.getDataACT();

    $scope.ModelTempACT = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewACT = Object.assign({}, $scope.ModelTempACT);

    $scope.GetSequenceACT = function () {
        cboService.getSequence($scope.getSeqUrlACT, function (data) {
            $scope.ModelTempACT.Sequence = data;
            $scope.ModelNewACT.Sequence = data;
        });
    };
    $scope.GetSequenceACT();

    $scope.GetACT = function (args) {

        $scope.ModelNewACT = Object.assign({}, args.data);
        $scope.ActionACT = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveACT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewACTForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlACT,
                data: { 'data': $scope.ModelNewACT },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ACTClearFields(response.data.Sequence);
                    $scope.getDataACT();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteACT = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewACT.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlACT + $scope.ModelNewACT.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ACTClearFields(response.data.Sequence);
                    $scope.getDataACT();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveACT = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewACT.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpQACT')).modal('show');
    }

    $scope.ClearACT = function () {
        ACTClearFields($scope.GetSequenceACT());
        return true;
    };

    function ACTClearFields(seq) {
        $scope.ActionACT = 'Save';
        $scope.ModelNewACT = Object.assign({}, $scope.ModelTempACT);
        $scope.ModelNewACT.Sequence = seq;
    }
    // #endregion Quality Activity Check Type

    // #region Defect Type
    $rootScope.titleDT = 'Defect Type';
    $scope.ActionDT = 'Save';
    $scope.ModelListDT = [];
    $scope.pathDT = 'QMS/DefectType/';
    $scope.getListUrlDT = $scope.pathDT + 'getlist';
    $scope.getSeqUrlDT = $scope.pathDT + 'getautosequence';
    $scope.saveUrlDT = $scope.pathDT + 'create';
    $scope.deleteUrlDT = $scope.pathDT + 'delete/';
    baseService.init($scope.getListUrlDT);
    $scope.searchByDT = "UserName"; $scope.searchDT = "";
    $scope.searchByListDT = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataDT = function () {
        $http({
            method: 'POST',
            url: $scope.pathDT + "GetList",
            data: { column: $scope.searchByDT, value: $scope.searchDT },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListDT = response.data;
            ClearFieldsDT(response.data.Sequence);
            $scope.GetSequenceDT();
        });
    }
    $scope.getDataDT();

    $scope.ModelTempDT = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewDT = Object.assign({}, $scope.ModelTempDT);

    $scope.GetSequenceDT = function () {
        cboService.getSequence($scope.getSeqUrlDT, function (data) {
            $scope.ModelTempDT.Sequence = data;
            $scope.ModelNewDT.Sequence = data;
        });
    };
    $scope.GetSequenceDT();

    $scope.GetDT = function (args) {

        $scope.ModelNewDT = Object.assign({}, args.data);
        $scope.ActionDT = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewFormDT.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDT,
                data: { 'data': $scope.ModelNewDT },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDT(response.data.Sequence);
                    $scope.getDataDT();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDT = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDT.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDT + $scope.ModelNewDT.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDT(response.data.Sequence);
                    $scope.getDataDT();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveDT = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDT.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDT')).modal('show');
    }

    $scope.ClearDT = function () {
        ClearFieldsDT($scope.GetSequenceDT());
        return true;
    };

    function ClearFieldsDT(seq) {
        $scope.ActionDT = 'Save';
        $scope.ModelNewDT = Object.assign({}, $scope.ModelTempDT);
        $scope.ModelNewDT.Sequence = seq;
    }
    // #endregion Defect Type

    //  #region Check Level
    $rootScope.titleDCL = 'Defect Check Level';
    $scope.ActionDCL = 'Save';
    $scope.ModelListDCL = [];
    $scope.pathDCL = 'QMS/DefectCheckLevel/';
    $scope.getListUrlDCL = $scope.pathDCL + 'getlist';
    $scope.getSeqUrlDCL = $scope.pathDCL + 'getautosequence';
    $scope.saveUrlDCL = $scope.pathDCL + 'create';
    $scope.deleteUrlDCL = $scope.pathDCL + 'delete/';
    baseService.init($scope.getListUrlDCL);
    $scope.searchByDCL = "UserName"; $scope.searchDCL = "";
    $scope.searchByListDCL = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataDCL = function () {
        $http({
            method: 'POST',
            url: $scope.pathDCL + "GetList",
            data: { column: $scope.searchByDCL, value: $scope.searchDCL },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListDCL = response.data;
            ClearFieldsDCL(response.data.Sequence);
            $scope.GetSequenceDCL();
        });
    }
    $scope.getDataDCL();

    $scope.ModelTempDCL = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewDCL = Object.assign({}, $scope.ModelTempDCL);

    $scope.GetSequenceDCL = function () {
        cboService.getSequence($scope.getSeqUrlDCL, function (data) {
            $scope.ModelTempDCL.Sequence = data;
            $scope.ModelNewDCL.Sequence = data;
        });
    };
    $scope.GetSequenceDCL();

    $scope.GetDCL = function (args) {

        $scope.ModelNewDCL = Object.assign({}, args.data);
        $scope.ActionDCL = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDCL = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewDCLForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDCL,
                data: { 'data': $scope.ModelNewDCL },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDCL(response.data.Sequence);
                    $scope.getDataDCL();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDCL = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDCL.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDCL + $scope.ModelNewDCL.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDCL(response.data.Sequence);
                    $scope.getDataDCL();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveDCL = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDCL.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
    }

    $scope.ClearDCL = function () {
        ClearFieldsDCL($scope.GetSequenceDCL());
        return true;
    };

    function ClearFieldsDCL(seq) {
        $scope.ActionDCL = 'Save';
        $scope.ModelNewDCL = Object.assign({}, $scope.ModelTempDCL);
        $scope.ModelNewDCL.Sequence = seq;
    }
    //  #endregion Check Level

    // #region  Defect Zone
    $rootScope.titleDZ = 'Defect Zone';
    $scope.ActionDZ = 'Save';
    $scope.ModelListDZ = [];
    $scope.pathDZ = 'QMS/DefectZone/';
    $scope.getListUrlDZ = $scope.pathDZ + 'getlist';
    $scope.getSeqUrlDZ = $scope.pathDZ + 'getautosequence';
    $scope.saveUrlDZ = $scope.pathDZ + 'create';
    $scope.deleteUrlDZ = $scope.pathDZ + 'delete/';
    baseService.init($scope.getListUrlDZ);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByListDZ = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataDZ = function () {
        $http({
            method: 'POST',
            url: $scope.pathDZ + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListDZ = response.data;
            ClearFieldsDZ(response.data.Sequence);
            $scope.GetSequenceDZ();
        });
    }
    $scope.getDataDZ();

    $scope.ModelTempDZ = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewDZ = Object.assign({}, $scope.ModelTempDZ);

    $scope.GetSequenceDZ = function () {
        cboService.getSequence($scope.getSeqUrlDZ, function (data) {
            $scope.ModelTempDZ.Sequence = data;
            $scope.ModelNewDZ.Sequence = data;
        });
    };
    $scope.GetSequenceDZ();

    $scope.GetDZ = function (args) {

        $scope.ModelNewDZ = Object.assign({}, args.data);
        $scope.ActionDZ = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveDZ = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewFormDZ.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlDZ,
                data: { 'data': $scope.ModelNewDZ },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDZ(response.data.Sequence);
                    $scope.getDataDZ();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteDZ = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewDZ.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlDZ + $scope.ModelNewDZ.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsDZ(response.data.Sequence);
                    $scope.getDataDZ();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveDZ = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewDZ.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpDZ')).modal('show');
    }

    $scope.ClearDZ = function () {
        ClearFieldsDZ($scope.GetSequenceDZ());
        return true;
    };

    function ClearFieldsDZ(seq) {
        $scope.ActionDZ = 'Save';
        $scope.ModelNewDZ = Object.assign({}, $scope.ModelTempDZ);
        $scope.ModelNewDZ.Sequence = seq;
    }
    // #endregion  Defect Zone

    // #region Repair Type
    $rootScope.titleRT = 'Repair Type';
    $scope.ActionRT = 'Save';
    $scope.ModelListRT = [];
    $scope.pathRT = 'QMS/RepairType/';
    $scope.getListUrlRT = $scope.pathRT + 'getlist';
    $scope.getSeqUrlRT = $scope.pathRT + 'getautosequence';
    $scope.saveUrlRT = $scope.pathRT + 'create';
    $scope.deleteUrlRT = $scope.pathRT + 'delete/';
    baseService.init($scope.getListUrlRT);
    $scope.searchByRT = "UserName"; $scope.searchRT = "";
    $scope.searchByListRT = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataRT = function () {
        $http({
            method: 'POST',
            url: $scope.pathRT + "GetList",
            data: { column: $scope.searchByRT, value: $scope.searchRT },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListRT = response.data;
            ClearFieldsRT(response.data.Sequence);
            $scope.GetSequenceRT();
        });
    }
    $scope.getDataRT();

    $scope.ModelTempRT = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewRT = Object.assign({}, $scope.ModelTempRT);

    $scope.GetSequenceRT = function () {
        cboService.getSequence($scope.getSeqUrlRT, function (data) {
            $scope.ModelTempRT.Sequence = data;
            $scope.ModelNewRT.Sequence = data;
        });
    };
    $scope.GetSequenceRT();

    $scope.GetRT = function (args) {

        $scope.ModelNewRT = Object.assign({}, args.data);
        $scope.ActionRT = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveRT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewRTForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlRT,
                data: { 'data': $scope.ModelNewRT },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsRT(response.data.Sequence);
                    $scope.getDataRT();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteRT = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewRT.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlRT + $scope.ModelNewRT.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsRT(response.data.Sequence);
                    $scope.getDataRT();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveRT = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewRT.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpRT')).modal('show');
    }

    $scope.ClearRT = function () {
        ClearFieldsRT($scope.GetSequenceRT());
        return true;
    };

    function ClearFieldsRT(seq) {
        $scope.ActionRT = 'Save';
        $scope.ModelNewRT = Object.assign({}, $scope.ModelTempRT);
        $scope.ModelNewRT.Sequence = seq;
    }
    // #endregion Repair Type

    // #region Grade Master
    $rootScope.titleGM = 'Grade Master';
    $scope.ActionGM = 'Save';
    $scope.ModelListGM = [];
    $scope.pathGM = 'QMS/GradeMaster/';
    $scope.getListUrlGM = $scope.pathGM + 'getlist';
    $scope.getSeqUrlGM = $scope.pathGM + 'getautosequence';
    $scope.saveUrlGM = $scope.pathGM + 'create';
    $scope.deleteUrlGM = $scope.pathGM + 'delete/';
    baseService.init($scope.getListUrlGM);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByListGM = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataGM = function () {
        $http({
            method: 'POST',
            url: $scope.pathGM + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListGM = response.data;
            ClearFieldsGM(response.data.Sequence);
            $scope.GetSequenceGM();
        });
    }
    $scope.getDataGM();

    $scope.ModelTempGM = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewGM = Object.assign({}, $scope.ModelTempGM);

    $scope.GetSequenceGM = function () {
        cboService.getSequence($scope.getSeqUrlGM, function (data) {
            $scope.ModelTempGM.Sequence = data;
            $scope.ModelNewGM.Sequence = data;
        });
    };
    $scope.GetSequenceGM();

    $scope.GetGM = function (args) {

        $scope.ModelNewGM = Object.assign({}, args.data);
        $scope.ActionGM = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveGM = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewGMForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlGM,
                data: { 'data': $scope.ModelNewGM },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsGM(response.data.Sequence);
                    $scope.getDataGM();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteGM = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewGM.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlGM + $scope.ModelNewGM.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsGM(response.data.Sequence);
                    $scope.getDataGM();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveGM = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewGM.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpGM')).modal('show');
    }

    $scope.ClearGM = function () {
        ClearFieldsGM($scope.GetSequenceGM());
        return true;
    };

    function ClearFieldsGM(seq) {
        $scope.ActionGM = 'Save';
        $scope.ModelNewGM = Object.assign({}, $scope.ModelTempGM);
        $scope.ModelNewGM.Sequence = seq;
    }
    // #endregion Grade Master

    // #region  Inspection Type
    $rootScope.titleIT = 'Inspection Type';
    $scope.ActionIT = 'Save';
    $scope.ModelListIT = [];
    $scope.pathIT = 'QMS/InspectionType/';
    $scope.getListUrlIT = $scope.pathIT + 'getlist';
    $scope.getSeqUrlIT = $scope.pathIT + 'getautosequence';
    $scope.saveUrlIT = $scope.pathIT + 'create';
    $scope.deleteUrlIT = $scope.pathIT + 'delete/';
    baseService.init($scope.getListUrlIT);
    $scope.searchByIT = "UserName"; $scope.searchIT = "";
    $scope.searchByListIT = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataIT = function () {
        $http({
            method: 'POST',
            url: $scope.pathIT + "GetList",
            data: { column: $scope.searchByIT, value: $scope.searchIT },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListIT = response.data;
            ClearFieldsIT(response.data.Sequence);
            $scope.GetSequenceIT();
        });
    }
    $scope.getDataIT();

    $scope.ModelTempIT = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewIT = Object.assign({}, $scope.ModelTempIT);

    $scope.GetSequenceIT = function () {
        cboService.getSequence($scope.getSeqUrlIT, function (data) {
            $scope.ModelTempIT.Sequence = data;
            $scope.ModelNewIT.Sequence = data;
        });
    };
    $scope.GetSequenceIT();

    $scope.GetIT = function (args) {

        $scope.ModelNewIT = Object.assign({}, args.data);
        $scope.ActionIT = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveIT = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewITForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlIT,
                data: { 'data': $scope.ModelNewIT },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsIT(response.data.Sequence);
                    $scope.getDataIT();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteIT = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewIT.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlIT + $scope.ModelNewIT.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsIT(response.data.Sequence);
                    $scope.getDataIT();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveIT = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewIT.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpIT')).modal('show');
    }

    $scope.ClearIT = function () {
        ClearFieldsIT($scope.GetSequenceIT());
        return true;
    };

    function ClearFieldsIT(seq) {
        $scope.ActionIT = 'Save';
        $scope.ModelNewIT = Object.assign({}, $scope.ModelTempIT);
        $scope.ModelNewIT.Sequence = seq;
    }
    // #endregion  Inspection Type

    // #region Inspection Master
    $rootScope.titleIM = 'Inspection Master';
    $scope.ActionIM = 'Save';
    $scope.ModelListIM = [];
    $scope.pathIM = 'QMS/InspectionMaster/';
    $scope.getListUrlIM = $scope.pathIM + 'getlist';
    $scope.getSeqUrlIM = $scope.pathIM + 'getautosequence';
    $scope.saveUrlIM = $scope.pathIM + 'create';
    $scope.deleteUrlIM = $scope.pathIM + 'delete/';
    baseService.init($scope.getListUrlIM);
    $scope.searchByIM = "UserName"; $scope.searchIM = "";
    $scope.searchByListIM = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataIM = function () {
        $http({
            method: 'POST',
            url: $scope.pathIM + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListIM = response.data;
            ClearFieldsIM(response.data.Sequence);
            $scope.GetSequenceIM();
        });
    }
    $scope.getDataIM();

    $scope.ModelTempIM = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Category:null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewIM = Object.assign({}, $scope.ModelTempIM);

    $scope.GetSequenceIM = function () {
        cboService.getSequence($scope.getSeqUrlIM, function (data) {
            $scope.ModelTempIM.Sequence = data;
            $scope.ModelNewIM.Sequence = data;
        });
    };
    $scope.GetSequenceIM();

    $scope.GetIM = function (args) {

        $scope.ModelNewIM = Object.assign({}, args.data);
        $scope.ActionIM = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ActivityCategoryList = [];
    $scope.GetQMSActivityCategory = function () {
        $http({
            method: 'GET',
            url: $scope.pathIM + 'GetQMSActivityCategory',
            data: { 'data': $scope.ModelNewIM },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ActivityCategoryList = response.data;
        })
    }

    $scope.GetQMSActivityCategory();

    $scope.SaveIM = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewIMForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlIM,
                data: { 'data': $scope.ModelNewIM },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsIM(response.data.Sequence);
                    $scope.getDataIM();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteIM = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewIM.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlIM + $scope.ModelNewIM.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsIM(response.data.Sequence);
                    $scope.getDataIM();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveIM = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewIM.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpIM')).modal('show');
    }

    $scope.ClearIM = function () {
        ClearFieldsIM($scope.GetSequenceIM());
        return true;
    };

    function ClearFieldsIM(seq) {
        $scope.ActionIM = 'Save';
        $scope.ModelNewIM = Object.assign({}, $scope.ModelTempIM);
        $scope.ModelNewIM.Sequence = seq;
    }
    // #endregion Inspection Master

    // #region Quality Status
    $rootScope.titleQS = 'Quality Status';
    $scope.ActionQS = 'Save';
    $scope.ModelListQS = [];
    $scope.pathQS = 'QMS/QualityStatus/';
    $scope.getListUrlQS = $scope.pathQS + 'getlist';
    $scope.getSeqUrlQS = $scope.pathQS + 'getautosequence';
    $scope.saveUrlQS = $scope.pathQS + 'create';
    $scope.deleteUrlQS = $scope.pathQS + 'delete/';
    baseService.init($scope.getListUrlQS);
    $scope.searchByQS = "UserName"; $scope.searchQS = "";
    $scope.searchByListQS = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getDataQS = function () {
        $http({
            method: 'POST',
            url: $scope.pathQS + "GetList",
            data: { column: $scope.searchByQS, value: $scope.searchQS },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListQS = response.data;
            ClearFieldsQS(response.data.Sequence);
            $scope.GetSequenceQS();
        });
    }
    $scope.getDataQS();

    $scope.ModelTempQS = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNewQS = Object.assign({}, $scope.ModelTempQS);

    $scope.GetSequenceQS = function () {
        cboService.getSequence($scope.getSeqUrlQS, function (data) {
            $scope.ModelTempQS.Sequence = data;
            $scope.ModelNewQS.Sequence = data;
        });
    };
    $scope.GetSequenceQS();

    $scope.GetQS = function (args) {

        $scope.ModelNewQS = Object.assign({}, args.data);
        $scope.ActionQS = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.SaveQS = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewQSForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlQS,
                data: { 'data': $scope.ModelNewQS },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsQS(response.data.Sequence);
                    $scope.getDataQS();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.DeleteQS = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNewQS.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlQS + $scope.ModelNewQS.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFieldsQS(response.data.Sequence);
                    $scope.getDataQS();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.RemoveQS = function () {

        if (!baseService.isUndefinedOrNull($scope.ModelNewQS.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpQS')).modal('show');
    }

    $scope.ClearQS = function () {
        ClearFieldsQS($scope.GetSequenceQS());
        return true;
    };

    function ClearFieldsQS(seq) {
        $scope.ActionQS = 'Save';
        $scope.ModelNewQS = Object.assign({}, $scope.ModelTempQS);
        $scope.ModelNewQS.Sequence = seq;
    }
    // #endregion Quality Status
}