'use strict';
ProcessManagementController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessManagementController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.title = 'Process Management'
    $scope.path = 'Processes/ProcessManagement/';
    $scope.Action = 'Save';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.UCtab = 1;
    $scope.SetUCTab = function (newTab) {
        $scope.UCtab = newTab;
    }

    $scope.isSetUC = function (newTab) {
        return $scope.UCtab === newTab;
    }
    // #endregion TAB CHANGE

    $scope.ProcessManagementTemp = {
        Id: null,
        StandardName: null,
        UserName: null,
        Process: null,
        SubProcess: null,
        MinSPTTime: null,
        MaxSPTTime: null,
        StandardSPTTime: null,
        ResponsiblePerson: null,
        EmployeeName: null,
        IsWorkCenterApplicable: null
    }
    $scope.ProcessManagementNew = Object.assign({}, $scope.ProcessManagementTemp)

    $scope.DataList = [];
    $scope.GetProcessManagementDataList = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/GetProcessManagementDataList'
        }).then(function successCallback(response) {
            $scope.DataList = response.data;

        }
        )
    }

    $scope.GetProcessManagementDataList();

    $scope.Get = function (args) {
        $scope.ProcessManagementNew = Object.assign({}, args.data)
        $scope.Action = 'Update';
        $scope.LoadEntityDetails();
        $scope.LoadUtilityGrid();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.EntityList = [];
    $scope.LoadEntityDetails = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadEntityDetails',
            data: { 'headerId': $scope.ProcessManagementNew.Id }
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        }
        )
    }
    //$scope.LoadEntityDetails();

    $scope.ProcessList = [];
    $scope.LoadProcessList = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        }
        )
    }
    $scope.LoadProcessList();

    $scope.SubProcessList = [];
    $scope.LoadSubProcessList = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadSubProcessList'
        }).then(function successCallback(response) {
            $scope.SubProcessList = response.data;
        }
        )
    }
    $scope.LoadSubProcessList();

    $scope.MaterialList = [];
    $scope.LoadMaterialGrid = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadMaterialGrid'
        }).then(function successCallback(response) {
            $scope.MaterialList = response.data;
        }
        )
    }
    $scope.LoadMaterialGrid();

    $scope.UtilityList = [];
    $scope.LoadUtilityGrid = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadUtilityGrid',
            data: { 'headerId': $scope.ProcessManagementNew.Id }
        }).then(function successCallback(response) {
            $scope.UtilityList = response.data;
        }
        )
    }


    $scope.ResponsiblePersonList = [];
    $scope.LoadResponsiblePopupData = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadResponsiblePopupData'

        }).then(function successCallback(response) {
            $scope.ResponsiblePersonList = response.data;
            angular.element(document.querySelector('#employeePopUps')).modal('show');
        }
        )

    }

    $scope.UOMList = [];
    $scope.GetUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUOM',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.UOMList = response.data;
            })
    }
    $scope.GetUOM();

    $scope.QualityManagementMasterWorkCenterList = [];
    $scope.GetWorkcenter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'LoadWorkCenterDetails',
            data: { 'processId': $scope.ProcessManagementNew.Process },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.QualityManagementMasterWorkCenterList = response.data;
            })
    }
    $scope.GetWorkcenter();

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: { 'data': $scope.ProcessManagementNew },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetProcessManagementDataList();
                }
            })
    }

    $scope.SelectedEntityList = [];
    $scope.SaveProcessEntity = function () {
        for (var i = 0; i < $scope.EntityList.length; i++) {
            //if ($scope.EntityList[i].Flag) {
            //    $scope.SelectedEntityList.push($scope.EntityList[i]);
            //}

            if ($scope.EntityList[i].Flag == true && ($scope.EntityList[i].IsActive == null || $scope.EntityList[i].IsActive == false)) {
                $scope.SelectedEntityList.push($scope.EntityList[i]);
            }
            else if ($scope.EntityList[i].Flag == false && $scope.EntityList[i].Id != null) {

                $scope.SelectedEntityList.push($scope.EntityList[i]);
            }

        }
        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcessEntity',
            data: {
                'datalist': $scope.SelectedEntityList,
                'headerid': $scope.ProcessManagementNew.Id
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            })
    }

    $scope.ChkdMaterialList = [];
    $scope.SaveProcessMaterial = function () {
        for (var i = 0; i < $scope.MaterialList.length; i++) {
            if ($scope.MaterialList[i].Flag) {
                $scope.ChkdMaterialList.push($scope.MaterialList[i]);
            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcessMaterial',
            data: {
                'datalist': $scope.ChkdMaterialList,
                'headerid': $scope.ProcessManagementNew.Id
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            })

    }

    $scope.checkUtilityList = [];
    $scope.SaveProcessUtility = function () {
        for (var i = 0; i < $scope.UtilityList.length; i++) {
            //if ($scope.UtilityList[i].Flag) {
            //    $scope.checkUtilityList.push($scope.UtilityList[i]);
            //}

            if ($scope.UtilityList[i].Flag == true && ($scope.UtilityList[i].IsActive == null || $scope.UtilityList[i].IsActive == false)) {
                $scope.checkUtilityList.push($scope.UtilityList[i]);
            }
            else if ($scope.UtilityList[i].Flag == false && $scope.UtilityList[i].Id != null) {

                $scope.checkUtilityList.push($scope.UtilityList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcessUtility',
            data: {
                'datalist': $scope.checkUtilityList,
                'headerid': $scope.ProcessManagementNew.Id
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            })

    }

    $scope.chkdWorkcenterList = [];
    $scope.SaveWorkcenter = function () {
        for (var i = 0; i < $scope.QualityManagementMasterWorkCenterList.length; i++) {
            if ($scope.QualityManagementMasterWorkCenterList[i].Flag) {
                $scope.chkdWorkcenterList.push($scope.QualityManagementMasterWorkCenterList[i]);

            }
        }

        $http({
            method: 'POST',
            url: $scope.path + 'SaveWorkcenter',
            data: {
                'datalist': $scope.chkdWorkcenterList

            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                }
            })
    }



    $scope.EmployeedblClick = function (args) {
        $scope.ProcessManagementNew.ResponsiblePerson = args.data.EmployeeCode;
        $scope.ProcessManagementNew.EmployeeName = args.data.EmployeeName;
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    }


    //----------------------------------------------------------------------------------------------------------------------------

    // #region Process Parameter
    $scope.getSeqUrl = $scope.path + 'GetSequence';
    $scope.ActionPP = 'Save';

    $scope.GetProcessParameter = function (args) {
        $scope.PMPModelNew = Object.assign({}, args.data)
        $scope.ActionPP = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.ProcessParamList = [];
    $scope.LoadProcessParameterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'LoadProcessParameterData',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.ProcessParamList = response.data;
            })
    }
    $scope.LoadProcessParameterData();

    $scope.PMTemp = {
        Id: null,
        Sequence: null,
        ItemName: null,
        UOMId: null,
        Min: null,
        Max: null,
        Remarks: null,
        IsUtilityApplicable: null
    }
    $scope.PMPModelNew = Object.assign({}, $scope.PMTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.PMTemp.Sequence = data;
            $scope.PMPModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.SaveProcessParameter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcessParameter',
            data: { 'data': $scope.PMPModelNew },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProcessParameterData();
                }
            })
    }

    $scope.message_Detailconfirmation = null;
    $scope.RemoveDetailConsumptionMatrix = function (data) {
        $scope.DetailConsumptionSKUMapping = data;
        if (!baseService.isUndefinedOrNull($scope.DetailConsumptionSKUMapping.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
    }

    $scope.DeleteProcessParameter = function () {
        
    }
    // #endregion Process Parameter

    //------------------------------------------------------------------------------------------------------------------------------
    // #region OWM

    $scope.ModelTemp = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        UserName: null,
        StandardName: null,
        Active:true,
        Remarks:null
    }
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequenceOWM = function () {
        cboService.getSequence($scope.path + 'GetSequenceOWM', function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequenceOWM();

    $scope.EditMode = function (args) {
        $scope.ModelNew = Object.assign({}, args.data)
       
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.OWMDataList = [];
    $scope.GetOWMData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetOWMData',
            dataType:'JSON'
        })
            .then(function successCallback(response) {
                $scope.OWMDataList = response.data;
            })
    }
    $scope.GetOWMData();


    $scope.OWMSave = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'OWMSave',
            data: { 'data': $scope.ModelNew },
            dataType:'JSON'
        })
            .then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetOWMData();
                    ClearFields($scope.GetSequenceOWM());
                }
            })
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';

        $scope.ModelNew = {
            Id: null,
            Sequence: 0,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
        };
        $scope.ModelNewGPL = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNewGPL.Sequence = seq;

    }

    $scope.message_confirmation = "";
    $scope.removeProcessManagementRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmDetailPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeProcessManagementRow = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteProcessManagementOWM?id=' + $scope.ModelNew.Id,
           
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
    // #endregion OWM
   //------------------------------------------------------------------------------------------------------------------------------


   //------------------------------------------------------------------------------------------------------------------------------
    $scope.ModelTempGPL = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        UserName: null,
        StandardName: null,
        Active: true,
        Remarks: null
    }
    $scope.ModelNewGPL = Object.assign({}, $scope.ModelTempGPL);

    $scope.GetSequenceGPL = function () {
        cboService.getSequence($scope.path + 'GetSequenceGPL', function (data) {
            $scope.ModelTempGPL.Sequence = data;
            $scope.ModelNewGPL.Sequence = data;
        });
    };
    $scope.GetSequenceGPL();

    $scope.GPLEditMode = function (args) {
        $scope.ModelNewGPL = Object.assign({}, args.data)

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.GPLDataList = [];
    $scope.GetGPLData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetGPLData',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.GPLDataList = response.data;

            });
    }
    $scope.GetGPLData();

    $scope.GPLSave = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GPLSave',
            data: { 'data': $scope.ModelNew },
            dataType: 'JSON'
        })
            .then(function (response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetGPLData();
                    ClearFieldsGPL($scope.GetSequenceGPL());
                }
            })
    }

    $scope.ClearGPL = function () {
        ClearFieldsGPL($scope.GetSequenceGPL());
        return true;
    };

    function ClearFieldsGPL(seq) {
        $scope.Action = 'Save';

        $scope.ModelNewGPL = {
            Id: null,
            Sequence: 0,
            StandardName: null,
            UserName: null,
            ShortName: null,
            Code: null,
            Active: true,
            Remarks: null,
        };
        $scope.ModelNewGPL = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNewGPL.Sequence = seq;

    }
    $scope.message_confirmation = "Are you sure want to permanent delete ?";
    $scope.removeProcessManagementGPLRowModal = function (tempId) {
        try {
            $scope.tempId = tempId;
            $scope.message_confirmation = "Are you sure want to permanent delete ?";
            angular.element(document.querySelector('#confirmDetailGPLPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeProcessManagementGPLRow = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'DeleteProcessManagementGPL?id=' + $scope.ModelNewGPL.Id,

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
   //------------------------------------------------------------------------------------------------------------------------------


}