'use strict';
QMSDefectMasterController.$inject = ['cboService','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function QMSDefectMasterController(cboService,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "QMS Defect Master";
    $scope.QMSDefectMasterList = [];

    $scope.RepairTypeList = [];
    $scope.CriticalityLevelList = [];

    $scope.path = 'QMS/QMSDefectMaster/';

    $scope.getListUrl = $scope.path + 'getlist';

    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.saveUrl = $scope.path + 'create';
    $scope.saveurlsublocation = $scope.path + 'createqmssublocation';

    $scope.deleteUrl = $scope.path + 'delete/';

    baseService.init($scope.getListUrl);


    $scope.searchBy = "UserName"; $scope.search = "";


    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'StandardName', name: "Standard Name" }, { value: 'ShortName', name: "Short Name" }, { value: 'UserCode', name: "User  Code" }, { value: 'UserName', name: "User Name" }, { value: 'CriticalityLevelId', name: "Criticality Level" }, { value: 'StandardCode', name: "Standard Code" }];


    // #region ddl

    $http({
        method: 'GET',
        url: 'QMS/QMSDefectMaster/getrepairtypelist/',
    }).then(function successCallback(response) {
        $scope.RepairTypeList = response.data;
    });

    $http({
        method: 'GET',
        url: 'QMS/QMSDefectMaster/getcriticalitylevel/',
    }).then(function successCallback(response) {
        $scope.CriticalityLevelList = response.data;
    });


    // #end region

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QMSDefectMasterList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        StandardName: null,
        StandardCode: null,
        DefectCause: null,
        UserName: null,
        UserCode: null,
        ShortName: null,
      //  ProductivityApplicable: false,
        CriticalityLevelId: null,
        RepairTypeId: null,

        ProductApplicable: false,
        MachineApplicable: false,
        TestApplicable: false,
        MaterialApplicable: false,
        ProcessApplicable: false,
        SkillApplicable: false,
        ProcessParameterApplicable: false
    };
    $scope.QMSDefectMaster = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.QMSDefectMaster.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {
        $scope.QMSDefectMaster = Object.assign({}, args.data);
        GetQmsTestApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProductApplicableData($scope.QMSDefectMaster.Id);
        GetQmsMaterialApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProcessApplicableData($scope.QMSDefectMaster.Id);
        GetQmsMachineApplicableData($scope.QMSDefectMaster.Id);
        GetQmsSkillApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProcessParameterApplicableData($scope.QMSDefectMaster.Id);
        GetQmsDefectTypeData($scope.QMSDefectMaster.Id);
        GetQmsDefectCheckData($scope.QMSDefectMaster.Id);
        GetQmsDefectZoneData($scope.QMSDefectMaster.Id);
        GetQmsOperationData($scope.QMSDefectMaster.Id);
        GetQmsQualityData($scope.QMSDefectMaster.Id);
        GetQmsInspectionData($scope.QMSDefectMaster.Id);
        $scope.setTab(6);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    $scope.Action = 'Save';

    // To show data in grid
    $scope.Getgrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.QMSDefectMasterList = response.data;

        });
    }

    $scope.Save = function () {
        try {
            if ($scope.QMSDefectMaster.ProductApplicable && $scope.QmsProductApplicableList.length < 1) {

                throw 'Please Enter Product Master Tab First';
            }
            if ($scope.QMSDefectMaster.TestApplicable && $scope.QmsTestApplicableList.length < 1) {

                throw 'Please Enter QMS Testing Master Tab First';
            }
            if ($scope.QMSDefectMaster.MaterialApplicable && $scope.QmsMaterialApplicableList.length < 1) {

                throw 'Please Enter Material Master Tab First';
            }
            if ($scope.QMSDefectMaster.ProcessApplicable && $scope.QmsProcessApplicableList.length < 1) {

                throw 'Please Enter Process Tab First';
            }
            if ($scope.QMSDefectMaster.MachineApplicable && $scope.QmsMachineApplicableList.length < 1) {

                throw 'Please Enter Machine Master Tab First';
            }
            if ($scope.QMSDefectMaster.SkillApplicable && $scope.QmsSkillApplicableList.length < 1) {

                throw 'Please Enter Skill Tab First';
            }
            if ($scope.QMSDefectMaster.ProcessParameterApplicable && $scope.QmsProcessParameterApplicableList.length < 1) {

                throw 'Please Enter Process Parameter Tab First';
            }
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.General.$valid) {

                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: {
                        'data': $scope.QMSDefectMaster, 'TestApplicableList': $scope.QmsTestApplicableList, 'ProductApplicableList': $scope.QmsProductApplicableList,
                        'MaterialApplicableList': $scope.QmsMaterialApplicableList, 'ProcessApplicableList': $scope.QmsProcessApplicableList, 'MachineApplicableList': $scope.QmsMachineApplicableList,
                        'SkillApplicableList': $scope.QmsSkillApplicableList, 'ProcessParameterApplicableList': $scope.QmsProcessParameterApplicableList, 'DefectTypeList': $scope.QmsDefectTypeList,
                        'DefectCheckList': $scope.QmsDefectCheckList, 'DefectZoneList': $scope.QmsDefectZoneList, 'OperationList': $scope.QmsOperationList, 'QualityList': $scope.QmsQualityList,
                        'InspectionList': $scope.QmsInspectionList
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.QMSDefectMaster = response.data.Data;
                        GetQmsTestApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsProductApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsMaterialApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsProcessApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsMachineApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsSkillApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsProcessParameterApplicableData($scope.QMSDefectMaster.Id);
                        GetQmsDefectTypeData($scope.QMSDefectMaster.Id);
                        GetQmsDefectCheckData($scope.QMSDefectMaster.Id);
                        GetQmsDefectZoneData($scope.QMSDefectMaster.Id);
                        GetQmsOperationData($scope.QMSDefectMaster.Id);
                        GetQmsQualityData($scope.QMSDefectMaster.Id);
                        GetQmsInspectionData($scope.QMSDefectMaster.Id);
                        $scope.Action = 'Update';
                        $scope.Getgrid();

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        }
        catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.QMSDefectMaster.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.QMSDefectMaster.Id,
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

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());

        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.QMSDefectMaster = Object.assign({}, $scope.ModelTemp);
        $scope.QMSDefectMaster.Sequence = seq;
        GetQmsTestApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProductApplicableData($scope.QMSDefectMaster.Id);
        GetQmsMaterialApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProcessApplicableData($scope.QMSDefectMaster.Id);
        GetQmsMachineApplicableData($scope.QMSDefectMaster.Id);
        GetQmsSkillApplicableData($scope.QMSDefectMaster.Id);
        GetQmsProcessParameterApplicableData($scope.QMSDefectMaster.Id);
        GetQmsDefectTypeData($scope.QMSDefectMaster.Id);
        GetQmsDefectCheckData($scope.QMSDefectMaster.Id);
        GetQmsDefectZoneData($scope.QMSDefectMaster.Id);
        GetQmsOperationData($scope.QMSDefectMaster.Id);
        GetQmsQualityData($scope.QMSDefectMaster.Id);
        GetQmsInspectionData($scope.QMSDefectMaster.Id);
        $scope.setTab();
 
    }

    // #region Tab
    //  $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // #endregion

    // *************** TABS *******************
// ******************* TEST APPLICABLE TAB ****************

    // #region TEST APPLICABLE TAB

    $scope.TestApplicableList = [];
    $scope.GetTestApplicableDetails = function () {
        try {
            $scope.TestApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetTestApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.TestApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#TestApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseTestAppPopUp = function () {
        angular.element(document.querySelector('#TestApplicablePopUp')).modal('hide');
    }

    $scope.QmsTestApplicableList = [];
    $scope.SelectedTestApplicable = function () {

        if (baseService.arrayLength($scope.TestApplicableList) > 0) {
            angular.forEach($scope.TestApplicableList, function (a) {
                if (checkTestApplicalbeExist($scope.QmsTestApplicableList, a.Id) === false) {
                if (a.Active) {
                    $scope.QmsTestApplicableList.push({
                        Id: null
                        , TestApplicableId: a.Id
                        , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                        , Sequence: a.Sequence
                        , Code: a.Code
                        , ShortName: a.ShortName
                        , StandardName: a.StandardName
                        , UserName: a.UserName
                       
                    });
                }
                   }
            });
        }
        else
            angular.forEach($scope.QmsTestApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsTestApplicableList, 'Id', a.TestApplicableId))
                    $scope.QmsTestApplicableList.splice(a, 1);
            });

        $scope.CloseTestAppPopUp();
    };

   
    function GetQmsTestApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsTestApplicablData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsTestApplicableList = response.data;
        });
    }

    function checkTestApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].TestApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelRowModal = function (Id) {
        $scope.QmsTestAppId = Id;
        angular.element(document.querySelector("#removerTAPopUp")).modal("show");
    }

    $scope.DeleteTestApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsTestAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteTestApp?id=' + $scope.QmsTestAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsTestApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion
    //*********** TEST APPLICABLE TAB End**********************

    // ******************* PRODUCT APPLICABLE TAB ****************

    // #region PRODUCT APPLICABLE TAB

    $scope.ProductApplicableList = [];
    $scope.GetProductApplicableDetails = function () {
        try {
            $scope.ProductApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetProductApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.ProductApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#ProductApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseProductAppPopUp = function () {
        angular.element(document.querySelector('#ProductApplicablePopUp')).modal('hide');
    }

    $scope.QmsProductApplicableList = [];
    $scope.SelectedProductApplicable = function () {

        if (baseService.arrayLength($scope.ProductApplicableList) > 0) {
            angular.forEach($scope.ProductApplicableList, function (a) {
                if (checkProductApplicalbeExist($scope.QmsProductApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsProductApplicableList.push({
                            Id: null
                            , ProductApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsProductApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsProductApplicableList, 'Id', a.ProductApplicableId))
                    $scope.QmsProductApplicableList.splice(a, 1);
            });

        $scope.CloseProductAppPopUp();
    };


    function GetQmsProductApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsProductApplicablData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsProductApplicableList = response.data;
        });
    }

    function checkProductApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProductApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelProductRowModal = function (Id) {
        $scope.QmsProductAppId = Id;
        angular.element(document.querySelector("#removerPAPopUp")).modal("show");
    }

    $scope.DeleteProductApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsProductAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteProductApp?id=' + $scope.QmsProductAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsProductApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion
    //*********** PRODUCT APPLICABLE TAB End**********************

    // ******************* MATERIAL APPLICABLE TAB ****************
    // #region MATERIAL APPLICABLE TAB

    $scope.MaterialApplicableList = [];
    $scope.GetMaterialApplicableDetails = function () {
        try {
            $scope.MaterialApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetMaterialApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.MaterialApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#MaterialApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseMaterialAppPopUp = function () {
        angular.element(document.querySelector('#MaterialApplicablePopUp')).modal('hide');
    }

    $scope.QmsMaterialApplicableList = [];
    $scope.SelectedMaterialApplicable = function () {

        if (baseService.arrayLength($scope.MaterialApplicableList) > 0) {
            angular.forEach($scope.MaterialApplicableList, function (a) {
                if (checkMaterialApplicalbeExist($scope.QmsMaterialApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsMaterialApplicableList.push({
                            Id: null
                            , MaterialApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsMaterialApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsMaterialApplicableList, 'Id', a.MaterialApplicableId))
                    $scope.QmsMaterialApplicableList.splice(a, 1);
            });

        $scope.CloseMaterialAppPopUp();
    };


    function GetQmsMaterialApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsMaterialApplicableData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsMaterialApplicableList = response.data;
        });
    }

    function checkMaterialApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MaterialApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelMaterialRowModal = function (Id) {
        $scope.QmsMaterialAppId = Id;
        angular.element(document.querySelector("#removerMAPopUp")).modal("show");
    }

    $scope.DeleteMaterialApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsMaterialAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteMaterialApp?id=' + $scope.QmsMaterialAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsMaterialApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion MATERIAL    // ******************* PROCESS APPLICABLE TAB ****************
    // #region PROCESS APPLICABLE TAB

    $scope.ProcessApplicableList = [];
    $scope.GetProcessApplicableDetails = function () {
        try {
            $scope.ProcessApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetProcessApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.ProcessApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#ProcessApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseProcessAppPopUp = function () {
        angular.element(document.querySelector('#ProcessApplicablePopUp')).modal('hide');
    }

    $scope.QmsProcessApplicableList = [];
    $scope.SelectedProcessApplicable = function () {

        if (baseService.arrayLength($scope.ProcessApplicableList) > 0) {
            angular.forEach($scope.ProcessApplicableList, function (a) {
                if (checkProcessApplicalbeExist($scope.QmsProcessApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsProcessApplicableList.push({
                            Id: null
                            , ProcessApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsProcessApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsProcessApplicableList, 'Id', a.ProcessApplicableId))
                    $scope.QmsProcessApplicableList.splice(a, 1);
            });

        $scope.CloseProcessAppPopUp();
    };


    function GetQmsProcessApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsProcessApplicableData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsProcessApplicableList = response.data;
        });
    }

    function checkProcessApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelProcessRowModal = function (Id) {
        $scope.QmsProcessAppId = Id;
        angular.element(document.querySelector("#removerProcessAPopUp")).modal("show");
    }

    $scope.DeleteProcessApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsProcessAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteProcessApp?id=' + $scope.QmsProcessAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsProcessApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion PROCESS
    // ******************* MACHINE APPLICABLE TAB ****************
    // #region MACHINE APPLICABLE TAB

    $scope.MachineApplicableList = [];
    $scope.GetMachineApplicableDetails = function () {
        try {
            $scope.MachineApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetMachineApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.MachineApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#MachineApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseMachineAppPopUp = function () {
        angular.element(document.querySelector('#MachineApplicablePopUp')).modal('hide');
    }

    $scope.QmsMachineApplicableList = [];
    $scope.SelectedMachineApplicable = function () {

        if (baseService.arrayLength($scope.MachineApplicableList) > 0) {
            angular.forEach($scope.MachineApplicableList, function (a) {
                if (checkMachineApplicalbeExist($scope.QmsMachineApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsMachineApplicableList.push({
                            Id: null
                            , MachineApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                            , MachineCategory: a.MachineCategory
                            , MachineSubCategory: a.MachineSubCategory

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsMachineApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsMachineApplicableList, 'Id', a.MachineApplicableId))
                    $scope.QmsMachineApplicableList.splice(a, 1);
            });

        $scope.CloseMachineAppPopUp();
    };


    function GetQmsMachineApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsMachineApplicableData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsMachineApplicableList = response.data;
        });
    }

    function checkMachineApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].MachineApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelMachineRowModal = function (Id) {
        $scope.QmsMachineAppId = Id;
        angular.element(document.querySelector("#removerMachineAPopUp")).modal("show");
    }

    $scope.DeleteMachineApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsMachineAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteMachineApp?id=' + $scope.QmsMachineAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsMachineApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion MACHINE

    // ******************* SKILL APPLICABLE TAB ****************
    // #region SKILL APPLICABLE TAB

    $scope.SkillApplicableList = [];
    $scope.GetSkillApplicableDetails = function () {
        try {
            $scope.SkillApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetSkillApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.SkillApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#SkillApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseSkillAppPopUp = function () {
        angular.element(document.querySelector('#SkillApplicablePopUp')).modal('hide');
    }

    $scope.QmsSkillApplicableList = [];
    $scope.SelectedSkillApplicable = function () {

        if (baseService.arrayLength($scope.SkillApplicableList) > 0) {
            angular.forEach($scope.SkillApplicableList, function (a) {
                if (checkSkillApplicalbeExist($scope.QmsSkillApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsSkillApplicableList.push({
                            Id: null
                            , SkillApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                            , SkillCategory: a.SkillCategory
                           

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsSkillApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsSkillApplicableList, 'Id', a.SkillApplicableId))
                    $scope.QmsSkillApplicableList.splice(a, 1);
            });

        $scope.CloseSkillAppPopUp();
    };


    function GetQmsSkillApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsSkillApplicableData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsSkillApplicableList = response.data;
        });
    }

    function checkSkillApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].SkillApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelSkillRowModal = function (Id) {
        $scope.QmsSkillAppId = Id;
        angular.element(document.querySelector("#removerSkillAPopUp")).modal("show");
    }

    $scope.DeleteSkillApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsSkillAppId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteSkillApp?id=' + $scope.QmsSkillAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsSkillApplicableData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };


    // #endregion SKILL
    // ******************* PROCESS PARAMETER APPLICABLE TAB ****************
    // #region PROCESS PARAMETER APPLICABLE TAB

    $scope.ProcessParameterApplicableList = [];
    $scope.GetProcessParameterApplicableDetails = function () {
        try {
            $scope.ProcessParameterApplicableList = [];
            $http.get("QMS/QMSDefectMaster/GetProcessParameterApplicableData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.ProcessParameterApplicableList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#ProcessParameterApplicablePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseProcessParameterAppPopUp = function () {
        angular.element(document.querySelector('#ProcessParameterApplicablePopUp')).modal('hide');
    }

    $scope.QmsProcessParameterApplicableList = [];
    $scope.SelectedProcessParameterApplicable = function () {

        if (baseService.arrayLength($scope.ProcessParameterApplicableList) > 0) {
            angular.forEach($scope.ProcessParameterApplicableList, function (a) {
                if (checkProcessParameterApplicalbeExist($scope.QmsProcessParameterApplicableList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsProcessParameterApplicableList.push({
                            Id: null
                            , ProcessParameterApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                           
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsProcessParameterApplicableList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsProcessParameterApplicableList, 'Id', a.ProcessParameterApplicableId))
                    $scope.QmsProcessParameterApplicableList.splice(a, 1);
            });

        $scope.CloseProcessParameterAppPopUp();
    };


    function GetQmsProcessParameterApplicableData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsProcessParameterApplicableData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsProcessParameterApplicableList = response.data;
        });
    }

    function checkProcessParameterApplicalbeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessParameterApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelProcessParameterRowModal = function (Id) {
        $scope.QmsProcessParameterAppId = Id;
        angular.element(document.querySelector("#removerProcessParameterAPopUp")).modal("show");
    }

    $scope.DeleteProcessParameterApp = function () {
        if (baseService.isUndefinedOrNull($scope.QmsProcessParameterAppId)) {
         
        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteProcessParameterApp?id=' + $scope.QmsProcessParameterAppId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsProcessParameterApplicableData($scope.QMSDefectMaster.Id);
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion PROCESS PARAMETER

// ******************* DEFECT TYPE TAB ****************
    // #region DEFECT TYPE TAB

    $scope.DefectTypeList = [];
    $scope.GetDefectTypeDetails = function () {
        try {
            $scope.DefectTypeList = [];
            $http.get("QMS/QMSDefectMaster/GetDefectTypeData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.DefectTypeList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#DefectTypePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseDefectTypePopUp = function () {
        angular.element(document.querySelector('#DefectTypePopUp')).modal('hide');
    }

    $scope.QmsDefectTypeList = [];
    $scope.SelectedDefectType = function () {

        if (baseService.arrayLength($scope.DefectTypeList) > 0) {
            angular.forEach($scope.DefectTypeList, function (a) {
                if (checkDefectTypeExist($scope.QmsDefectTypeList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsDefectTypeList.push({
                            Id: null
                            , DefectTypeId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                           
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsDefectTypeList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsDefectTypeList, 'Id', a.DefectTypeId))
                    $scope.QmsDefectTypeList.splice(a, 1);
            });

        $scope.CloseDefectTypePopUp();
    };


    function GetQmsDefectTypeData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsDefectTypeData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsDefectTypeList = response.data;
        });
    }

    function checkDefectTypeExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DefectTypeId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelDefectTypeRowModal = function (Id) {
        $scope.QmsDefectTypeId = Id;
        angular.element(document.querySelector("#removerDefectTypePopUp")).modal("show");
    }

    $scope.DeleteDefectType = function () {
        if (baseService.isUndefinedOrNull($scope.QmsDefectTypeId)) {
        
        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteDefectType?id=' + $scope.QmsDefectTypeId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsDefectTypeData($scope.QMSDefectMaster.Id);
          
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion DEFECT TYPE
// ******************* DEFECT CHECK LEVEL TAB ****************
    // #region DEFECT CHECK LEVEL TAB

    $scope.DefectCheckList = [];
    $scope.GetDefectCheckDetails = function () {
        try {
            $scope.DefectCheckList = [];
            $http.get("QMS/QMSDefectMaster/GetDefectCheckData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.DefectCheckList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#DefectCheckPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseDefectCheckPopUp = function () {
        angular.element(document.querySelector('#DefectCheckPopUp')).modal('hide');
    }

    $scope.QmsDefectCheckList = [];
    $scope.SelectedDefectCheck = function () {

        if (baseService.arrayLength($scope.DefectCheckList) > 0) {
            angular.forEach($scope.DefectCheckList, function (a) {
                if (checkDefectCheckExist($scope.QmsDefectCheckList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsDefectCheckList.push({
                            Id: null
                            , DefectCheckLevelId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                           
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsDefectCheckList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsDefectCheckList, 'Id', a.DefectCheckLevelId))
                    $scope.QmsDefectCheckList.splice(a, 1);
            });

        $scope.CloseDefectCheckPopUp();
    };


    function GetQmsDefectCheckData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsDefectCheckData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsDefectCheckList = response.data;
        });
    }

    function checkDefectCheckExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DefectCheckLevelId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelDefectCheckRowModal = function (Id) {
        $scope.QmsDefectCheckId = Id;
        angular.element(document.querySelector("#removerDefectCheckPopUp")).modal("show");
    }

    $scope.DeleteDefectCheck = function () {
        if (baseService.isUndefinedOrNull($scope.QmsDefectCheckId)) {
        
        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteDefectCheck?id=' + $scope.QmsDefectCheckId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsDefectCheckData($scope.QMSDefectMaster.Id);
          
                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion DEFECT CHECK LEVEL    // ******************* DEFECT ZONE TAB ****************
    // #region DEFECT ZONE TAB

    $scope.DefectZoneList = [];
    $scope.GetDefectZoneDetails = function () {
        try {
            $scope.DefectZoneList = [];
            $http.get("QMS/QMSDefectMaster/GetDefectZoneData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.DefectZoneList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#DefectZonePopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseDefectZonePopUp = function () {
        angular.element(document.querySelector('#DefectZonePopUp')).modal('hide');
    }

    $scope.QmsDefectZoneList = [];
    $scope.SelectedDefectZone = function () {

        if (baseService.arrayLength($scope.DefectZoneList) > 0) {
            angular.forEach($scope.DefectZoneList, function (a) {
                if (checkDefectZoneExist($scope.QmsDefectZoneList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsDefectZoneList.push({
                            Id: null
                            , DefectZoneId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsDefectZoneList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsDefectZoneList, 'Id', a.DefectZoneId))
                    $scope.QmsDefectZoneList.splice(a, 1);
            });

        $scope.CloseDefectZonePopUp();
    };


    function GetQmsDefectZoneData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsDefectZoneData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsDefectZoneList = response.data;
        });
    }

    function checkDefectZoneExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].DefectZoneId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelDefectZoneRowModal = function (Id) {
        $scope.QmsDefectZoneId = Id;
        angular.element(document.querySelector("#removerDefectZonePopUp")).modal("show");
    }

    $scope.DeleteDefectZone = function () {
        if (baseService.isUndefinedOrNull($scope.QmsDefectZoneId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteDefectZone?id=' + $scope.QmsDefectZoneId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsDefectZoneData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion DEFECT ZONE

    // ******************* OPERATION TAB ****************
    // #region OPERATION TAB

    $scope.OperationList = [];
    $scope.GetOperationDetails = function () {
        try {
            $scope.OperationList = [];
            $http.get("QMS/QMSDefectMaster/GetOperationData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.OperationList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#OperationPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseOperationPopUp = function () {
        angular.element(document.querySelector('#OperationPopUp')).modal('hide');
    }

    $scope.QmsOperationList = [];
    $scope.SelectedOperation = function () {

        if (baseService.arrayLength($scope.OperationList) > 0) {
            angular.forEach($scope.OperationList, function (a) {
                if (checkOperationExist($scope.QmsOperationList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsOperationList.push({
                            Id: null
                            , OperationActivityId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsOperationList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsOperationList, 'Id', a.OperationActivityId))
                    $scope.QmsOperationList.splice(a, 1);
            });

        $scope.CloseOperationPopUp();
    };


    function GetQmsOperationData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsOperationData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsOperationList = response.data;
        });
    }

    function checkOperationExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].OperationActivityId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelOperationRowModal = function (Id) {
        $scope.QmsOperationId = Id;
        angular.element(document.querySelector("#removerOperationPopUp")).modal("show");
    }

    $scope.DeleteOperation = function () {
        if (baseService.isUndefinedOrNull($scope.QmsOperationId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteOperation?id=' + $scope.QmsOperationId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsOperationData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion OPERATION

    // ******************* QUALITY TAB ****************
    // #region QUALITY TAB

    $scope.QualityList = [];
    $scope.GetQualityDetails = function () {
        try {
            $scope.QualityList = [];
            $http.get("QMS/QMSDefectMaster/GetQualityData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.QualityList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#QualityPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseQualityPopUp = function () {
        angular.element(document.querySelector('#QualityPopUp')).modal('hide');
    }

    $scope.QmsQualityList = [];
    $scope.SelectedQuality = function () {

        if (baseService.arrayLength($scope.QualityList) > 0) {
            angular.forEach($scope.QualityList, function (a) {
                if (checkQualityExist($scope.QmsQualityList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsQualityList.push({
                            Id: null
                            , QualityActivityId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                            , QMSActivityCategory: a.QMSActivityCategory
                            , QualityActivityCheckType: a.QualityActivityCheckType

                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsQualityList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsQualityList, 'Id', a.QualityActivityId))
                    $scope.QmsQualityList.splice(a, 1);
            });

        $scope.CloseQualityPopUp();
    };


    function GetQmsQualityData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsQualityData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsQualityList = response.data;
        });
    }

    function checkQualityExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].QualityActivityId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelQualityRowModal = function (Id) {
        $scope.QmsQualityId = Id;
        angular.element(document.querySelector("#removerQualityPopUp")).modal("show");
    }

    $scope.DeleteQuality = function () {
        if (baseService.isUndefinedOrNull($scope.QmsQualityId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteQuality?id=' + $scope.QmsQualityId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsQualityData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion QUALITY

    // ******************* INSPECTION TAB ****************
    // #region INSPECTION TAB

    $scope.InspectionList = [];
    $scope.GetInspectionDetails = function () {
        try {
            $scope.InspectionList = [];
            $http.get("QMS/QMSDefectMaster/GetInspectionData")
                .then(
                    function successCallback(response) {
                        if (baseService.arrayLength(response.data) > 0) {
                            $scope.InspectionList = response.data;
                        }
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            angular.element(document.querySelector('#InspectionPopUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.CloseInspectionPopUp = function () {
        angular.element(document.querySelector('#InspectionPopUp')).modal('hide');
    }

    $scope.QmsInspectionList = [];
    $scope.SelectedInspection = function () {

        if (baseService.arrayLength($scope.InspectionList) > 0) {
            angular.forEach($scope.InspectionList, function (a) {
                if (checkInspectionExist($scope.QmsInspectionList, a.Id) === false) {
                    if (a.Active) {
                        $scope.QmsInspectionList.push({
                            Id: null
                            , InspectionApplicableId: a.Id
                            , QMSDefectMasterId: $scope.QMSDefectMaster.Id
                            , Sequence: a.Sequence
                            , Code: a.Code
                            , ShortName: a.ShortName
                            , StandardName: a.StandardName
                            , UserName: a.UserName
                        });
                    }
                }
            });
        }
        else
            angular.forEach($scope.QmsInspectionList, function (a) {
                if (!baseService.valueCheckInList($scope.QmsInspectionList, 'Id', a.InspectionApplicableId))
                    $scope.QmsInspectionList.splice(a, 1);
            });

        $scope.CloseInspectionPopUp();
    };


    function GetQmsInspectionData(QMSDefectMasterId) {
        $http({
            method: 'GET',
            url: 'QMS/QMSDefectMaster/GetQmsInspectionData?QMSDefectMasterId=' + QMSDefectMasterId
        }).then(function successCallback(response) {
            $scope.QmsInspectionList = response.data;
        });
    }

    function checkInspectionExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].InspectionApplicableId === Id) {
                return true;
            }
        }
        return false;
    }

    $scope.DelInspectionRowModal = function (Id) {
        $scope.QmsInspectionId = Id;
        angular.element(document.querySelector("#removerInspectionPopUp")).modal("show");
    }

    $scope.DeleteInspection = function () {
        if (baseService.isUndefinedOrNull($scope.QmsInspectionId)) {

        }
        else {
            $http({
                method: 'POST',
                url: 'QMS/QMSDefectMaster/DeleteInspection?id=' + $scope.QmsInspectionId
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    GetQmsInspectionData($scope.QMSDefectMaster.Id);

                }
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
        }
    };

    // #endregion INSPECTION

};