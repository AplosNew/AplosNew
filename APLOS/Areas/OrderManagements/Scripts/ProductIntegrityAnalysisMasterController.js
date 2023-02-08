'use strict';
ProductIntegrityAnalysisMasterController.$inject = ["cboService","commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter"];
function ProductIntegrityAnalysisMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "ProductIntegrityAnalysisMaster";
    $scope.CriticalLevelLists = [];
    $scope.Action = 'Save';
    $scope.path = 'OrderManagements/ProductIntegrityAnalysisMaster/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.saveUrlItem = $scope.path + 'CreateItem';
    $scope.saveUrlParameter = $scope.path + 'CreateParameter';

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
        , StandardName: null
        , ShortName: null
        , ResponsiblePersonId: null
        , ResponsiblePerson: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
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
        , ProcessId: null
        , Category: null
        , Remarks: null
        
    };
    $scope.ItemNew = Object.assign({}, $scope.Item);

    $scope.Parameter = {
        Id: null
        , PredefineValue: null
        , Remarks: null
        , ItemId:null
    }
    $scope.ParameterNew = Object.assign({}, $scope.Parameter);

    $scope.selectUOM = function () {
        $scope.getUOM();
        angular.element(document.querySelector('#UOMPopUp')).modal('show');
    }

    $scope.UOMList = [];
    $scope.getUOM = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetUOM',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.UOMList = resp.data;
        });
    }

    $scope.doubleUOM = function (e) {
        $scope.ItemNew.UOMId = e.data.UOMId;
        $scope.ItemNew.UOM = e.data.UOM;
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }

    $scope.closeUOMPopUp = function () {
        angular.element(document.querySelector('#UOMPopUp')).modal('hide');
    }


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
            $scope.PIAMNew.ResponsiblePerson = response.data.PIAM[0].ResponsiblePerson;
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
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/GetItemAutoSequence?PIAMId=' + $scope.PIAMNew.Id
        }).then(function successCallback(response) {
            $scope.ItemNew.SNO = response.data;
        });
    }
    $scope.GeneratItemSequenceNo();

   
    $scope.selectResponsiblePerson = function () {
        $scope.getEmployee();
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('show');
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetEmployee',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.doubleEmployee = function (e) {
        $scope.PIAMNew.ResponsiblePersonId = e.data.SystemId;
        $scope.PIAMNew.ResponsiblePerson = e.data.EmployeeName;
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.closeResponsiblePersonPopUp = function () {
        angular.element(document.querySelector('#ResponsiblePersonPopup')).modal('hide');
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ProductIntegrityAnalysisMasterForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'PIAMData': $scope.PIAMNew},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadProductIntegrityAnalysisMasterList();
                    PIAMClearFields();
                 
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }    
    };

    $scope.ProductAnalysisItemList = [];
    $scope.LoadItemDetails = function () {
        $http({

            method: 'Get',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/LoadItemDetails?ProductId=' + $scope.PIAMNew.Id
        }).then(function successCallback(response) {
            $scope.ProductAnalysisItemList = response.data;
        }
        )
    }

    $scope.ItemSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.PIAMItemForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlItem,
                data: {
                    'ItemData': $scope.ItemNew,
                    'Pid':$scope.PIAMNew.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.LoadItemDetails($scope.PIAMNew.Id);
                    ItemClearFields($scope.GeneratItemSequenceNo($scope.PIAMNew.Id));

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
        PIAMClearFields();
    };
  
    $scope.ItemClear = function () {
        ItemClearFields($scope.GeneratItemSequenceNo($scope.PIAMNew.Id));
    };
    $scope.SaveParameterClear = function () {
        ParameterClearFields();
    };
   
    function PIAMClearFields() {
        $scope.Action = "Save";
        $scope.PIAMNew = Object.assign({}, $scope.PIAM);
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
                $scope.LoadItemDetails($scope.PIAMNew.Id);
                ItemClearFields($scope.GeneratItemSequenceNo($scope.PIAMNew.Id));
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    $scope.Delete = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/ProductIntegrityAnalysisMaster/PIAMDelete?id=' + $scope.PIAMNew.Id,
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