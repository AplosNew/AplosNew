'use strict';
PlantConfigController.$inject = ['cboService', "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function PlantConfigController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.path = 'Setups/plantconfig/';
    $scope.getListUrl = "Setups/plantconfig/GetMasterSearchData/";
    $scope.searchByList = [];
    baseService.init($scope.getListUrl, null, 10, null, 'PlantName', 'PlantName');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (response) {
                $scope.plantConfigList = response.Rows;
                if (baseService.arrayLength($scope.searchByList) == 0)
                    baseService.getDDLSearchColumn(response.Rows, $scope.searchByList);
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.plantConfig = {
        Id: null
        , BuyerApplicable: false
        , PlantId: null
        , PlantName: null
        , CompanyGroupId: null
        , CompanyGroupName: null
        , CompanyId: null
        , CompanyName: null
        , BlanketDefaultLength: null
        , BlanketDefaultWidth: null
        , IsBlanketDefaultLengthValuesChangeable:false
        , IsBlanketDefaultWidthValuesChangeable:false
        , IsAfterWashShrinkageOnActual:false
        , ProcessAssign: "ProductionOrder"
        , Active: true
        , FabRollPrefix: null
        , Operation: 'Operation Master'
        , OperationInProductionBookingWillBeCapturebyBulletin: false
        , MachineBudgetLevel: null
        , IsMachineChangeableinBulletinTemplate:false
    };

    $scope.plantConfigList = [];
    $scope.companyGroupList = [];
    $scope.companyList = [];

    $scope.machineBudgetLevelList = [];
    $scope.machineBudgetLevelList = [
        { Value: "Plant", Text: "Plant" },
        { Value: "Entity", Text: "Entity" }
    ];

    $scope.plantList = [];
    $scope.processList = [];
    $scope.truefalseList = [
        {
            'Text': 'YES',
            'Value': 'True'
        },
        {
            'Text': 'NO',
            'Value': 'False'
        }
    ];


    $scope.dayList = [
        {
            'Text': 'Saturday',
            'Value':'Saturday'
        },
        {
            'Text': 'Sunday',
            'Value': 'Sunday'
        },
        {
            'Text': 'Monday',
            'Value': 'Monday'
        },
        {
            'Text': 'Tuesday',
            'Value': 'Tuesday'
        },
        {
            'Text': 'Wednesday',
            'Value': 'Wednesday'
        },
        {
            'Text': 'Thursday',
            'Value': 'Thursday'
        },
        {
            'Text': 'Friday',
            'Value': 'Friday'
        }
    ]
    $scope.mainList = [];
    $scope.getPrdordSettingData = function () {
        $http({
            method: 'GET',
            url: 'Setups/prdordsetting/GetList?groupId=' + $scope.plantConfig.CompanyGroupId + '&companyId=' + $scope.plantConfig.CompanyId + '&plantId=' + $scope.plantConfig.PlantId
        }).then(function successCallback(response) {
            $scope.mainList = response.data;
        });
    };

    //Functions
    function clearObject(Obj) {
        try {
            for (var cObj in Obj) {
                Obj[cObj] = null;
            }
        } catch (e) {
            throw e;
        }
    }
    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });
    $scope.getCboCompanyByCompanyGroup = function (companyGroupId) {
        cboService.getCboCompanyByCompanyGroup(companyGroupId, function (result) {
            $scope.companyList = result;
        });
    };
    //For Plant
    $scope.getPlantList = function (CompanyId) {
        $http({
            method: 'GET',
            url: 'Setups/plantconfig/getplantlist?CompanyId=' + CompanyId
        }).then(function successCallback(response) {
            $scope.plantList = response.data;
        });
        //For Process  url: 'Setups/plantconfig/getprocesslist/',
        $http({
            method: 'GET',
            url: 'Processes/companyprocess/GetCompanyProcessCbo?companyId=' + CompanyId
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });
    }
    $scope.Get = function (id, index) {
        $scope.index = index;
        $http({
            method: 'GET',
            url: $scope.path + 'GetPlantConfigDataById?Id=' + id
        }).then(function successCallback(response) {
            $scope.plantConfig = response.data.masterData[0];
            $scope.getCboCompanyByCompanyGroup($scope.plantConfig.CompanyGroupId);

            //Plant
            $http({
                method: 'GET',
                url: 'Setups/plantconfig/getplantlist?CompanyId=' + $scope.plantConfig.CompanyId
            }).then(function successCallback(response) {
                $scope.plantList = response.data;
            });

            $http({
                method: 'GET',
                url: 'Processes/companyprocess/GetCompanyProductionProcessCbo?companyId=' + $scope.plantConfig.CompanyId
            }).then(function successCallback(response) {
                $scope.processList = response.data;
                });
            $scope.getPrdordSettingData();
            $scope.Action = "Update";
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        });
    };
    $scope.SaveMaster = function () {
        try {
            $http({
                method: 'GET',
                url: $scope.path + 'GetPlantWiseDuplicateData',
                params: { Id: $scope.plantConfig.Id, CompanyGroupId: $scope.plantConfig.CompanyGroupId, CompanyId: $scope.plantConfig.CompanyId, PlantId: $scope.plantConfig.PlantId }
            }).then(function successCallback(response) {
                try {
                    if (baseService.arrayLength(response.data.dData) > 0) throw "Can not Save. Data already Exist with this plant...";
                    //End Validation
                    $http({
                        method: 'POST',
                        url: $scope.path + 'savemaster',
                        data: { 'pmaster': $scope.plantConfig, 'prdOrdSetting': $scope.mainList},
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true)
                            ShowResult(response.data.Message, 'failure');
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.plantConfig = response.data.MasterData[0];
                            $scope.Clear();
                            $scope.getData();
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
                catch (e) {
                    ShowResult(e, 'Error');
                }
            });
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.plantConfig.Id)) {
            $http({
                method: 'POST',
                url: "Setups/plantconfig/delete/" + $scope.plantConfig.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.Clear();
                    $scope.getData();
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.companyList = [];
        $scope.plantList = [];
        $scope.processList = [];
        $scope.plantConfig = { ProcessAssign: "ProductionOrder", Operation: 'Operation Master' };
        $scope.mainList = [];
    }

    $scope.ShowuomdimensionList = function () {
        var modalOptions = {
            closeButtonText: 'Cancel',
            actionButtonText: 'Delete Customer',
            headerText: 'Delete ' + custName + '?',
            bodyText: 'Are you sure you want to delete this customer?'
        };
        modalService.showModal({}, modalOptions).then(function (result) {
            if (result === 'ok') {
                dataService.deleteCustomer(id).then(function () {
                    for (var i = 0; i < vm.customers.length; i++) {
                        if (vm.customers[i].id === id) {
                            vm.customers.splice(i, 1);
                            break;
                        }
                    }
                    filterCustomers(vm.searchText);
                }, function (error) {
                    $window.alert('Error deleting customer: ' + error.message);
                });
            }
        });
    }
}