'use strict';
machineMasterControllers.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function machineMasterControllers(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Machine Master'; 
    $scope.Action = 'Save';  
    $scope.Action1 = 'Save';
    $scope.OperationActivityList = [];
    $scope.OperationTypeList = [];
    $scope.OperationCategoryList = [];
    $scope.SkillList = [];
    $scope.MachineMasterList = [];
    $scope.ProcessList = [];
    $scope.legalDesignationList = [];
    $scope.SkillGroupingList = [];
    $scope.GetDataByMasterOrderIdList = [];
    $scope.EntityList = [];
    $scope.PositionList = [];
   
  
    $scope.path = 'Machines/MachineMaster/';//ControlerName
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.saveUrl1 = $scope.path + 'CreateManpower';
    $scope.updateUrl1 = $scope.path + 'EditManpower';
    $scope.deleteUrl1 = $scope.path + 'DeleteManpower/';
    $scope.model = {
        Id: null,
        CompanyGroupId: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        OperationActivityId: null,
        OperationTypeId: null,
        OperationCategoryId: null,
        SkillId: null,
        Type: null,
        MachineMasterId: null,
        SkillGroupId: null,
        LegalDesignationId: null,
        ProcessId: null,
        ProposedSalary: null,
        Remarks: null,
        Active: null
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.modelM = {
        Id: null,       
        CompanyGroupId:null,
        Sequence: null,
        OperationMasterId: null,
      EntityId: null,
      PositionId: null,
     Caption: null,
      ManpowerBudget: null,
      Active: null      
    };
    $scope.modelNewM = Object.assign({}, $scope.modelM);
    
   

 
  
    // #region GET Display DTA ON GRID
    $scope.GriddataOperationMaster = [];
    $scope.getaldataOperationMaster = function () {
        debugger;
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'IE/OperationMaster/GetOperationMaster', 
        }).then(function successCallback(response) {
            $scope.GriddataOperationMaster = response.data;
           
            //entrydata = copy(searchdata);
        });
    };
    $scope.getaldataOperationMaster();

    $scope.GetOperationPositionMp = [];
    $scope.GetOperationPositionMPBudget = function () {
        $http({
            method: "GET",
            dataType: 'JSON',
            //url: $scope.getSearchListUrl,
            url: 'IE/OperationMaster/GetOperationPositionMPBudget',
        }).then(function successCallback(response) {
            $scope.GetOperationPositionMp = response.data;
            //entrydata = copy(searchdata);
        });
    };
    $scope.GetOperationPositionMPBudget();

    
//#endregion



    // #region Bind Data on DropdownList 

    $scope.EntityCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboEntity'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });
    }
    $scope.EntityCbo();
    $scope.PositionCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboPosition'
        }).then(function successCallback(response) {
            $scope.PositionList = response.data;
        });
    }
    $scope.PositionCbo();
    
    $scope.OperationActivityCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationActivity'
        }).then(function successCallback(response) {
            $scope.OperationActivityList = response.data;
        });
    }
    $scope.OperationActivityCbo();

    $scope.GetCboOperationTypeCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationType'
        }).then(function successCallback(response) {
            $scope.OperationTypeList = response.data;
        });
    }
    $scope.GetCboOperationTypeCbo();



    $scope.GetCboOperationCategoryCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboOperationCategory'
        }).then(function successCallback(response) {
            $scope.OperationCategoryList = response.data;
        });
    }
    $scope.GetCboOperationCategoryCbo();


    $scope.GetCboSkillCbo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkill'
        }).then(function successCallback(response) {
            $scope.SkillList = response.data;
        });
    }
    $scope.GetCboSkillCbo();



    $scope.GetCboMachineMasterCbo = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboMachineMaster'
        }).then(function successCallback(response) {
            $scope.MachineMasterList = response.data;
        });
    }
    $scope.GetCboMachineMasterCbo();


    $scope.GetCboSkillGroupingCbo = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboSkillGrouping'
        }).then(function successCallback(response) {
            $scope.SkillGroupingList = response.data;
        });
    }
    $scope.GetCboSkillGroupingCbo();







    $scope.GetCbolegalDesignation = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCbolegalDesignation'
        }).then(function successCallback(response) {
            $scope.legalDesignationList = response.data;
        });
    }
    $scope.GetCbolegalDesignation();


    $scope.GetCboProcess = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetCboProcess'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });
    }
    $scope.GetCboProcess();
    
   
   
//#endregion


    // #region For AutoSequenceNo
    $scope.GeneratSequenceNo = function () {
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetAutoSequence'
        }).then(function successCallback(response) {
           $scope.modelNew.Sequence = response.data;
        });
    }
    $scope.GeneratSequenceNo();

    
 //#endregion AutoSequenceNo

    // #region For AutoSequenceNo For ManPower
    $scope.GetAutoSequenceForManPower = function () {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetAutoSequenceForManPower'
        }).then(function successCallback(response) {
            $scope.modelNewM.Sequence = response.data;
        });
    }
    $scope.GetAutoSequenceForManPower();


 //#endregion AutoSequenceNo

    

 // #region Data Save Update and Delete
   

    $scope.Save = function () {
        debugger;
        angular.copy($scope.modelNew, $scope.model);
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.modelNewForm.$valid) {
                if ($scope.Action === 'Save') {
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            //$scope.getData();
                            ShowResult(response.data.Message, 'failure');
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action = 'Update';                    
                           
                            $scope.getaldataOperationMaster();
                            $scope.Clear();
                            $scope.modelNew.OperationMasterIdID = response.data.Id;  
                     
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action === 'Update') {
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl,
                        data: $scope.model,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            //$scope.getData();
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getaldataOperationMaster();
                        }
                    }, function errorCallBack(response) {
                        //$scope.getData();
                        //ShowResult(response.data.Message, 'failure');
                        throw response.data.Message;
                    });
                }
           }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    


    $scope.Delete = function () {

        if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.modelNew.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getaldataOperationMaster();
                    //ClearFields();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    
        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.DeleteManpower = function () {

        if (!baseService.isUndefinedOrNull($scope.modelNewM.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl1 + $scope.modelNewM.Id,
                dataType: 'JSON'
            }).then(function (response) {
                if (response.data.Error === true)
                    ShowResult(response.data.Message, 'failure');
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getaldataOperationMaster();
                    ClearFieldss();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }

        else
            ShowResult('First delete all line item.', 'failure');
    };
    $scope.Clear = function () {
        ClearFields($scope.GeneratSequenceNo());
        return true;
    };
    $scope.Clear1 = function () {
        ClearFieldss($scope.GetAutoSequenceForManPower());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.OperationMaster = {};
        $scope.modelNew = { Active: true };
        $scope.modelNew.Active = true;
        $scope.modelNew.Sequence = seq;
    }
    function ClearFieldss(seq) {
        $scope.Action1 = 'Save';
        $scope.OperationMaster = {};
        $scope.modelNewM = { Active: true };
        //$scope.modelNew.Active = true;
        $scope.modelNewM.Sequence = seq;
    }
 
 //#endregion 


    $scope.recorddoubleclick = function ($event) {
        debugger;       
        var x = $event;
        $scope.OMId = x.data.Id;
       
       // $scope.modelNew.OperationMasterIdID = response.data.Id;  
        $scope.GetDataByMasterOrderIdfn($scope.OMId);
       // $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action = 'Update';
       
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.recorddoubleclickMP = function ($event) {
        debugger;
        var x = $event;
        $scope.OMId = x.data.Id;
        $scope.OperationMasterId = x.data.OperationMasterId;
        $scope.GetDataByMasterOrderIdfnMP($scope.OMId);
        $scope.Action1 = 'Update';
        if (!$rootScope.isCollapsed) $rootScope.toggle();
    };
    $scope.GetDataByMasterOrderIdfn = function (OMId) {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderId?id=' + OMId
        }).then(function successCallback(response) {
          
            $scope.modelNew = response.data[0];
            $scope.modelNew.OperationMasterIdID = response.data[0].Id;

        });
    }

    $scope.GetDataByMasterOrderIdfnMP = function (OMId) {
        debugger;
        $http({
            method: 'GET',
            url: 'IE/OperationMaster/GetDataByMasterOrderIdMP?id=' + OMId
        }).then(function successCallback(response) {
            $scope.modelNewM = response.data[0];


        });
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        $scope.getalldata1();

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    $scope.SaveManpower = function () {
        debugger;
        angular.copy($scope.modelNewM, $scope.modelM);
        $scope.modelM.OperationMasterId = $scope.modelNew.OperationMasterIdID;   
        
        $scope.$broadcast('show-errors-check-validity');
        try {
            if ($scope.modelNewForm1.$valid) {
                if ($scope.Action1 === 'Save') {
                    if ($scope.modelM.PositionId === null) {
                        ShowResult('Please select Position');
                    }
                    else if ($scope.modelM.Caption === null) {
                        ShowResult('Please input Caption');
                    }
                    else if ($scope.modelM.ManpowerBudget === null) {
                        ShowResult('Please input Manpower Budget');
                    }
                    $http({
                        method: 'POST',
                        url: $scope.saveUrl1,
                        data: $scope.modelM,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            //$scope.getData();
                            ShowResult(response.data.Message, 'failure');
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.Action1 = 'Update';
                            $scope.GetOperationPositionMPBudget();
                            $scope.Clear();

                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');

                    };
                }
                else if ($scope.Action1 === 'Update') {
                    $scope.modelM.OperationMasterId = $scope.OperationMasterId;
                    if ($scope.modelM.PositionId === null) {
                        ShowResult('Please select Position');
                    }
                    else if ($scope.modelM.Caption === null) {
                        ShowResult('Please input Caption');
                    }
                    else if ($scope.modelM.ManpowerBudget === null) {
                        ShowResult('Please input Manpower Budget');
                    }
                    $http({
                        method: 'POST',
                        url: $scope.updateUrl1,
                        data: $scope.modelM,
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                            //$scope.getData();
                            throw response.data.Message;
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.GetOperationPositionMPBudget();
                        }
                    }, function errorCallBack(response) {
                        //$scope.getData();
                        //ShowResult(response.data.Message, 'failure');
                        throw response.data.Message;
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
}