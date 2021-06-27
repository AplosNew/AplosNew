'use strict';
OTLimitSettingController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OTLimitSettingController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'OT Limit Setting';
    $scope.path = 'Attendances/OTLimitSetting/';
    $scope.Action = 'Save';
    $scope.GetOTLimitSettingListUrl = $scope.path + 'GetOTLimitSettingList';
    $scope.GetEditDataUrl = $scope.path + 'GetEditData';    
    $scope.saveUrl = $scope.path + 'Create';    
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.plantList = [];  
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });
    $scope.companyOnChange = function () {     
        $scope.plantList = [];
        cboService.getCboPlantByCompany($scope.OTLimitSettingModel.CompanyId, function (result) {
            $scope.plantList = result;
        });
    }
    $scope.OTLimitSettingModel = {
        Id: null,
        CompanyId: null,
        PlantId: null,      
        UserName: null,
        Description: null,
        Active: true,        
        MinOTLimitParDay :null,
        MaxOTLimitParDay: null,  

        MaxWeekOffOTLimitParDay: null,
        MaxHolidayOTLimitParDay: null,


        //MinOTLimitParWeek :null,
        MaxOTLimitParWeek :null,                        
        OTReductionFactor :null,
        Week: null
    }
    $scope.OTLimitSettinglist = [];
    $scope.GetOTLimitSettingList = function () {
        $scope.OTLimitSettinglist = [];
        $http({
            method: 'POST',
            url: $scope.GetOTLimitSettingListUrl,
            data: { PlantId: $scope.OTLimitSettingModel.PlantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.OTLimitSettinglist = response.data;//DailyAllowanceList
        });
    }
    $scope.Save = function () {
        try
        {
            if ($scope.OTLimitSettingModel.CompanyId == null) {
                throw "Enter Company ...";
            }
            if ($scope.OTLimitSettingModel.PlantId == null || $scope.OTLimitSettingModel.PlantId == '' || $scope.OTLimitSettingModel.PlantId == 'undefined') {
                throw "Enter Plant...";
            }
            if ($scope.OTLimitSettingModel.UserName == null || $scope.OTLimitSettingModel.UserName == '' || $scope.OTLimitSettingModel.UserName == 'undefined') {
                throw "Enter UserName...";
            }
            if ($scope.OTLimitSettingModel.Week == null || $scope.OTLimitSettingModel.Week == '' || $scope.OTLimitSettingModel.Week == 'undefined') {
                throw "Enter Week...";
            }
            if ($scope.OTLimitSettingModel.MinOTLimitParDay == null || $scope.OTLimitSettingModel.MinOTLimitParDay == '' || $scope.OTLimitSettingModel.MinOTLimitParDay == 'undefined') {
                throw "Enter Minimum OT Limit Per Day...";
            }
            else {
                if ($scope.OTLimitSettingModel.MinOTLimitParDay < 0) {
                    throw "Enter Positive Minimum OT Limit Per Day Value..";
                }
            }
            if ($scope.OTLimitSettingModel.MaxOTLimitParDay == null || $scope.OTLimitSettingModel.MaxOTLimitParDay == '' || $scope.OTLimitSettingModel.MaxOTLimitParDay == 'undefined') {
                throw "Enter Max OT Limit Per Day...";
            }
            else {
                if ($scope.OTLimitSettingModel.MaxOTLimitParDay < 0) {
                    throw "Enter Positive Max OT Limit Per Day Value..";
                }
            }
            if ($scope.OTLimitSettingModel.MaxOTLimitParWeek == null || $scope.OTLimitSettingModel.MaxOTLimitParWeek == '' || $scope.OTLimitSettingModel.MaxOTLimitParWeek == 'undefined') {
                throw "Enter Max OT Limit Per Week...";
            }
            else {
                if ($scope.OTLimitSettingModel.MaxOTLimitParWeek < 0) {
                    throw "Enter Positive Max OT Limit Per Week Value..";
                }
            }
            if ($scope.OTLimitSettingModel.OTReductionFactor == null || $scope.OTLimitSettingModel.OTReductionFactor == '' || $scope.OTLimitSettingModel.OTReductionFactor == 'undefined') {
                throw "Enter OT Reduction Factor...";
            } else {
                if ($scope.OTLimitSettingModel.OTReductionFactor < 0) {
                    throw "Enter Positive OT Reduction Factor Value..";
                }
            }
            if ($scope.OTLimitSettingModel.MaxOTLimitParDay >= $scope.OTLimitSettingModel.MinOTLimitParDay) {

            }
            else {
                throw "Max OT Limit Per Day Can't be smaller then Min. OT Limit";
            }
            if ($scope.OTLimitSettingModel.MaxOTLimitParWeek >= $scope.OTLimitSettingModel.MaxOTLimitParDay) {

            }
            else {
                throw "Max OT Limit Per Week Can't be smaller then Max. OT Limit";
            }
            //$scope.$broadcast('show-errors-check-validity');            
            if ($scope.Action === 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.OTLimitSettingModel,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');                        
                        $scope.GetOTLimitSettingList();
                        $scope.Clear();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action === 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.OTLimitSettingModel,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetOTLimitSettingList();
                        $scope.Clear();
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetEditData = function (args) {
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }           
            $http.get($scope.GetEditDataUrl + '?Id=' + args.data.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.Action = 'Update';
                        $scope.OTLimitSettingModel = {
                            Id: null,
                            CompanyId: null,
                            PlantId: null,                          
                            UserName: null,
                            Description: null,
                            Active: true,
                            MinOTLimitParDay: null,
                            MaxOTLimitParDay: null,
                            MaxWeekOffOTLimitParDay: null,
                            MaxHolidayOTLimitParDay: null,
                            //MinOTLimitParWeek: null,
                            MaxOTLimitParWeek: null,
                            OTReductionFactor: null,
                            Week: null
                        }
                        $scope.OTLimitSettingModel.Id = response.data[0].Id;
                        $scope.OTLimitSettingModel.PlantId = response.data[0].PlantID;
                        $scope.OTLimitSettingModel.Week = response.data[0].Week;
                        //$scope.OTLimitSettingModel.ToDay = response.data[0].ToDay;
                        $scope.OTLimitSettingModel.UserName = response.data[0].UserName;
                        $scope.OTLimitSettingModel.Description = response.data[0].Description;
                        $scope.OTLimitSettingModel.Active = response.data[0].Active;
                        $scope.OTLimitSettingModel.MinOTLimitParDay = response.data[0].MinOTLimitParDay;
                        $scope.OTLimitSettingModel.MaxOTLimitParDay = response.data[0].MaxOTLimitParDay;
                        //$scope.OTLimitSettingModel.MinOTLimitParWeek = response.data[0].MinOTLimitParWeek;
                        $scope.OTLimitSettingModel.MaxOTLimitParWeek = response.data[0].MaxOTLimitParWeek;
                        $scope.OTLimitSettingModel.OTReductionFactor = response.data[0].OTReductionFactor;
                        $scope.OTLimitSettingModel.CompanyId = response.data[0].CompanyIds;

                        $scope.OTLimitSettingModel.MaxWeekOffOTLimitParDay = response.data[0].MaxWeekOffOTLimitParDay;
                        $scope.OTLimitSettingModel.MaxHolidayOTLimitParDay = response.data[0].MaxHolidayOTLimitParDay;

                        //$scope.companyOnChange();
                    }
                },
                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };









    
    $scope.message_confirmation = 'Are you sure to Delete This Setting ?';

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.OTLimitSettingModel.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.OTLimitSettingModel.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                     $scope.GetOTLimitSettingList();
                        $scope.Clear();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
                $scope.GetSequence();
            });

        }
    };

   

    $scope.Clear = function ()  {
        $scope.Action = 'Save';
        $scope.OTLimitSettingModel = {
            Id: null,
            CompanyId: $scope.OTLimitSettingModel.CompanyId,
            PlantId: $scope.OTLimitSettingModel.PlantId,
           
            UserName: null,
            Description: null,
            Active: true,
            MinOTLimitParDay: null,
            MaxOTLimitParDay: null,
            MaxWeekOffOTLimitParDay: null,
            MaxHolidayOTLimitParDay: null,
            //MinOTLimitParWeek: null,
            MaxOTLimitParWeek: null,
            OTReductionFactor: null

        }
    }

   
}