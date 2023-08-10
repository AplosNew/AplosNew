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
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.EntityList = [];
    $scope.LoadEntityDetails = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadEntityDetails',
            data: { 'headerId': $scope.ProcessManagementNew.Id}
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
            url: 'Processes/ProcessManagement/LoadUtilityGrid'
        }).then(function successCallback(response) {
            $scope.UtilityList = response.data;
        }
        )
    }
    $scope.LoadUtilityGrid();

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
            dataType:'JSON'
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
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.QualityManagementMasterWorkCenterList = response.data;
            })
    }
    $scope.GetWorkcenter();

    $scope.ProcessManagementTemp = {
        Id: null,
        StandardName: null,
        UserName:null,
        Process: null,
        SubProcess:null,
        MinSPTTime: null,
        MaxSPTTime:null,
        StandardSPTTime: null,
        ResponsiblePerson: null,
        EmployeeName: null,
        IsWorkCenterApplicable:null
    }
    $scope.ProcessManagementNew = Object.assign({}, $scope.ProcessManagementTemp)

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
            if ($scope.EntityList[i].Flag) {
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
    }

    $scope.checkUtilityList = [];
    $scope.SaveProcessUtility = function () {
        for (var i = 0; i < $scope.UtilityList.length; i++) {
            if ($scope.UtilityList[i].Flag) {
                $scope.checkUtilityList.push($scope.UtilityList[i]);
            }

            $http({
                method: 'POST',
                url: $scope.path + 'SaveProcessUtility',
                data: { 'datalist': $scope.checkUtilityList },
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
    }

    $scope.chkdWorkcenterList = [];
    $scope.SaveWorkcenter = function(){
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
        $scope.ActionPP  = 'Update';
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
        Id:null,
        Sequence: null,
        ItemName: null,
        UOMId: null,
        Min: null,
        Max: null,
        Remarks: null,
        IsUtilityApplicable:null
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
}