'use strict';
ProductIntegrityAnalysisMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function ProductIntegrityAnalysisMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProductIntegrityAnalysisMaster";
    $scope.CriticalLevelLists = [];
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/ProductIntegrityAnalysisMaster/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveUrlItem = $scope.path + 'createItem';

    $scope.CriticalLevelLists = [
        {
            'Value': 'Normal',
            'Text': 'Normal'
        },
        {
            'Value': 'Important',
            'Text': 'Important'
        },
        {
            'Value': 'Critical',
            'Text': 'Critical'
        }
    ];
   
    $scope.PIAM = {
        Id: null
        , Code: null
        , StandaredName: null
        , ShortName: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , UserName:null
        , Remarks: null
        , IsActive: true
    };
    $scope.PIAMNew = Object.assign({}, $scope.PIAM);

    $scope.Item = {
        Id: null
        , PIAMID: null
        , SNO: null
        , ItemName: null
        , CriticalLevel: null
        , UOM: null
        , UOMId: null
        , ProductionProcess: null
        , Category: null
        , Remarks: null
        
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Parameter = {
        Id: null
        , SNO: null
        , CheckPoints: null
        , Remarks: null
        , ItemId:null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);


    $scope.ProcessList = [];
    $scope.GetProcessList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/GetProcessList'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetProcessList();


    $scope.ProductIntegrityAnalysisMasterList = [];
    $scope.LoadProductIntegrityAnalysisMasterList = function () {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/LoadProductIntegrityAnalysisMasterList'
        }).then(function successCallback(response) {
            $scope.ProductIntegrityAnalysisMasterList = response.data;
            var gridObj = $("#GridProductIntegrityAnalysisMaster").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        }
        )
    }
    $scope.LoadProductIntegrityAnalysisMasterList();

    $scope.GetDetails = function (args) {
        $scope.PIAMMasterId = args.data.Id;
        $http({
            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/LoadPIAMEditData?PIAMID=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.PIAMNew = response.data.PIAM[0];
            $scope.PIAMNew.ResponsiblePersoneBgtCode = response.data.PIAM[0].ResponsiblePersoneBgtCode;
            $scope.LoadItemDetails($scope.PIAMMasterId);
            $scope.GeneratItemSequenceNo($scope.PIAMMasterId);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }


    $scope.GeneratItemSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/GetItemAutoSequence?scheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ItemNew.SNO = response.data;
        });
    }
  /*  $scope.GeneratItemSequenceNo();*/

   
    $scope.selectBudgetCode = function () {
        $scope.getBudgetCode();
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('show');
    }

    $scope.BudgetCodeList = [];
    $scope.getBudgetCode = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetBudgetCode',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.BudgetCodeList = resp.data;
        });
    }

    $scope.doubleBudgetCode = function (e) {
        $scope.scheduleNew.ResponsiblePersoneBgtCodeId = e.data.ManPowerBudgetId;
        $scope.scheduleNew.ResponsiblePersoneBgtCode = e.data.Code;
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.closeBudgetCodePopUp = function () {
        angular.element(document.querySelector('#BudgetCodePopUp')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.MaintenanceScheduleForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'ScheduleData': $scope.scheduleNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProductIntegrityAnalysisMasterList();
                    ScheduleClearFields();
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.ScheduleItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'Machines/SkillManagement/LoadItemDetails?ScheduleId=' + $scope.scheduleNew.Id
        }).then(function successCallback(response) {
            $scope.ScheduleItemList = response.data;
        }
        )
    }

    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProductIntegrityAnalysisMasterItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew,
                    'Pid':$scope.scheduleNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails($scope.scheduleNew.Id);
                    ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
    };

    $scope.ParameterLists = [];
    $scope.getParameterPopup = function (data) {
        $scope.NewObject = data.data;
        var ItemId = $scope.ItemNew.Id;
        $scope.ItemNew.Id = ItemId;
        try {
            $http.get('OrderManagements/ProductIntegrityAnalysisMaster/getParameterData?ItemId=' + $scope.NewObject.Id)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
            angular.element(document.querySelector('#ParameterPoUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.getParameter = function (data) {
        try {
            $http.get('OrderManagements/ProductIntegrityAnalysisMaster/getParameterData?ItemId=' + data)
                .then(
                    function successCallback(response) {
                        $scope.ParameterLists = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
            var gridObj = $("#GridParameter").data("ejGrid"); gridObj.refreshContent(); gridObj.refreshTemplate();
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.GetParameterDetails = function (args) {
        $http({
            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/LoadParameterEditData?ParameterId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ParameterNew = response.data.Parameter[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    $scope.SaveParameterData = function () {
            $http({
                method: 'POST',
                url: $scope.saveUrlParameter,
                data: {
                    'ParameterData': $scope.ParameterNew,
                    'Pid': $scope.ItemNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getParameter($scope.ItemNew.Id);
                    ParameterClearFields();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
    };


    $scope.tab = 0;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;


    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    
    $scope.GetItemDetails = function (args) {
        $http({
            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/LoadItemEditData?ItemId=' + args.data.Id
        }).then(function successCallback(response) {
            $scope.ItemNew = response.data.item[0];
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }
        )
    }
    
    $scope.Clear = function () {
        ScheduleClearFields();
    };
  
    $scope.ItemClear = function () {
        ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
    };
    $scope.SaveParameterClear = function () {
        ParameterClearFields();
    };
   
    function ScheduleClearFields() {
        $scope.Action = "Save";
        $scope.scheduleNew = Object.assign({}, $scope.schedule);
        $scope.ScheduleMachineList = [];
    }

    function ItemClearFields(seq) {
        $scope.Action = "Save";
        $scope.ItemNew = Object.assign({}, $scope.Item);
        $scope.ItemNew.SNO = seq;
    }

   
    function ParameterClearFields() {
        $scope.Action = "Save";
        $scope.ParameterNew = Object.assign({}, $scope.Parameter);
    }
 
    $scope.removeRowModal = function (index,data) {
        try {
            $scope.popUpIndex = index;
            $scope.tempId = data;
            $scope.message_confirmation = "Are you sure you want to delete?";
            angular.element(document.querySelector('#confirmRemoveItem')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };
    
    
    $scope.removeItemRow = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/ItemDelete?id=' + $scope.tempId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadItemDetails($scope.scheduleNew.Id);
                ItemClearFields($scope.GeneratItemSequenceNo($scope.scheduleNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/ScheduleDelete?id=' + $scope.scheduleNew.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.LoadProductIntegrityAnalysisMasterList();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };
}