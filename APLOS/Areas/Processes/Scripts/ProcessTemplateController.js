'use strict';
ProcessTemplateController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService', '$window'];
function ProcessTemplateController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService, $window) {
    $scope.title = 'Process Template'
    $scope.path = 'Processes/ProcessTemplate/';
    $scope.Action = 'Save';

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.processTemp = {
        Id: null,
        StandardName: null,
        UserName: null,
        ResponsiblePerson: null,
        ResponsiblePersonId: null,
        EmployeeSystemId: null,
        ProcessManagementId: null,
        Remarks: null
    }
    $scope.ProcessManagementNew = Object.assign({}, $scope.processTemp)

    // #endregion TAB CHANGE

    // #region Get Methods

    $scope.ModelData = [];
    $scope.GetProcessTempData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcessTempData',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.ModelData = response.data;
               

            });
    }
    $scope.GetProcessTempData();

    $scope.Get = function (args) {
        $scope.ProcessManagementNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        $scope.GetSavedProcessByHeader();
        $scope.GetProcessManagementDataList();
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    }

    $scope.ddProcessManagementList = [];
    $scope.ProcessManagementDD = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ProcessManagementDD',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.ddProcessManagementList = response.data;

            });
    }
    $scope.ProcessManagementDD();

    $scope.ProcessManagementList = [];
    $scope.GetProcessManagementDataList = function () {
        $http({
            method: 'POST'
            ,url: 'Processes/ProcessTemplate/GetProcesDataList'
            , data: { 'headerid': $scope.ProcessManagementNew.Id}
        }).then(function successCallback(response) {
            $scope.ProcessManagementList = response.data;

        }
        )
    }
    

    $scope.PMMaterialList = [];
    $scope.GetSavedProcessByHeader = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessTemplate/GetSavedProcessByHeader',
            data: { 'headerid': $scope.ProcessManagementNew.Id},
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.PMMaterialList = response.data;

            }
            );
    }
    $scope.GetSavedProcessByHeader();

    $scope.PMUtilityList = [];
    $scope.GetProcessUtility = function () {
        $http({
            method: 'POST',
            url: 'Processes/ProcessTemplate/GetProcessUtility',
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                $scope.PMUtilityList = response.data;

            }
            );
    }
    $scope.GetProcessUtility();
    // #endregion Get Methods

    // #region HeaderSave
   

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
    // #endregion HeaderSave

    $scope.SelectedProcessList = [];
    $scope.SaveProcess = function () {
        for (var i = 0; i < $scope.ProcessManagementList.length; i++) {
            if ($scope.ProcessManagementList[i].Flag == true && ($scope.ProcessManagementList[i].isActive == null || $scope.ProcessManagementList[i].isActive == false)) {
                $scope.SelectedProcessList.push($scope.ProcessManagementList[i]);
            }
           

        }

        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcess',
            data: {
                'datalist': $scope.SelectedProcessList,
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

    $scope.ProcessParamList = [];
    $scope.GetProcessParamData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetProcessParamData',
            dataType:'JSON'
        })
            .then(function successCallback(response) {
                $scope.ProcessParamList = response.data;
            })
    }

    $scope.SelectedProcessParamList = [];
    $scope.SaveProcessParam = function () {
        for (var i = 0; i < $scope.ProcessParamList.length; i++) {
            if ($scope.ProcessParamList[i].Flag) {
                $scope.SelectedProcessParamList.push($scope.ProcessParamList[i])
            }
        }
        //$http({
        //    method: 'POST',
        //    url: $scope.path + 'SaveProcessParam',
        //    data: {
                
        //    },
        //    dataType: 'JSON'
        //})
        //    .then(function successCallback(response) {
        //        if (response.data.Error === true) {
        //            ShowResult(response.data.Message, 'failure');
        //        }
        //        else {
        //            ShowResult(response.data.Message, 'success');

        //        }
        //    })
    }


    $scope.ChkdMaterialList = [];
    $scope.SaveProcessMaterial = function () {
        for (var i = 0; i < $scope.MaterialList.length; i++) {
            if ($scope.MaterialList[i].Flag ) {
                $scope.ChkdMaterialList.push($scope.MaterialList[i]);
            }
            //else if($scope.MaterialList[i].Flag == false){
            //    $scope.ChkdMaterialList.push($scope.MaterialList[i]);
            //}

        }

        //$http({
        //    method: 'POST',
        //    url: $scope.path + 'SaveProcessMaterial',
        //    data: {
        //        'datalist': $scope.ChkdMaterialList,
        //        'headerid': $scope.ProcessManagementNew.Id
        //    },
        //    dataType: 'JSON'
        //})
        //    .then(function successCallback(response) {
        //        if (response.data.Error === true) {
        //            ShowResult(response.data.Message, 'failure');
        //        }
        //        else {
        //            ShowResult(response.data.Message, 'success');

        //        }
        //    })

    }

    $scope.checkUtilityList = [];
    $scope.SaveProcessUtility = function () {
        for (var i = 0; i < $scope.UtilityList.length; i++) {
            if ($scope.UtilityList[i].Flag) {
                $scope.checkUtilityList.push($scope.UtilityList[i]);
            }
        }
        //$http({
        //    method: 'POST',
        //    url: $scope.path + 'SaveProcessUtility',
        //    data: { 'datalist': $scope.checkUtilityList },
        //    dataType: 'JSON'
        //})
        //    .then(function successCallback(response) {
        //        if (response.data.Error === true) {
        //            ShowResult(response.data.Message, 'failure');
        //        }
        //        else {
        //            ShowResult(response.data.Message, 'success');

        //        }
        //    })

    }

    $scope.SaveProcessParamChild = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'SaveProcessParamChild',
            data: {
                'datalist': $scope.SelectedProcessParamList,
                'processutlity': $scope.checkUtilityList,
                'processMaterial': $scope.ChkdMaterialList,
                'headerid': $scope.ProcessManagementNew.Id
            },
            dataType:'JSON'
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

    $scope.EmployeedblClick = function (args) {
        $scope.ProcessManagementNew.ResponsiblePersonId = args.data.SystemID;
        $scope.ProcessManagementNew.ResponsiblePerson = args.data.EmployeeName;
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    }

    $scope.ProcessMaterialList = [];
    $scope.OpenProcessMaterialPopUp = function () {
        angular.element(document.querySelector('#porcessMaterialPopUp')).modal('show');
        $http({
            method: 'POST',
            url:'Processes/ProcessManagement/LoadMaterialGrid'
        })
            .then(function successCallback(response) {
                $scope.ProcessMaterialList = response.data;
            });
    }

    $scope.ProcessUtilityList = [];
    $scope.OpenProcessUtilityPopUp = function () {
        angular.element(document.querySelector('#porcessUtilityPopUp')).modal('show');
        $http({
            method: 'POST',
            url: 'Processes/ProcessManagement/LoadUtilityGrid'
        })
            .then(function successCallback(response) {
                $scope.ProcessUtilityList = response.data;
            });
    }


}