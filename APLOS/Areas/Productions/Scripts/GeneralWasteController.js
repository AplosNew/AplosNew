'use strict';
GeneralWasteController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function GeneralWasteController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'General Waste';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/GeneralWaste/';
    $scope.downloadgriddataUrlPath = $scope.path + 'DownloadUsingFullPath';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = null; $scope.search = null;
    
    $scope.Entity = null;
    $scope.EntityList = [];
    $scope.View = null;
    $scope.ViewList = [];
    $scope.ViewGridPop = [];
    $scope.FromDate = null;

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }



        $scope.getEntity = function () {
            $http({
                method: 'GET',
                url: $scope.path + "getEntity",
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.EntityList = response.data;
            });
        }

        $scope.getEntity();




        $scope.selectEntity = function () {
           
            angular.element(document.querySelector('#EntityPop')).modal('show');
        }

       
    $scope.Entity = null;

        $scope.doubleEntity = function (e) {
            $scope.EntityId = e.data.EntityId;
            $scope.Entity = e.data.EntityName;
                angular.element(document.querySelector('#EntityPop')).modal('hide');
         }

    
    $scope.getView = function () {

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            ShowResult('Please Select an Entity!','failure');
            throw ('Invalid Request!');
        }
        if (angular.isUndefinedOrNull($scope.FromDate) ) {
            ShowResult("Please Select the Date in Range!", 'failure');
            throw ('Invalid Request!!');
        }

        if (new Date($scope.FromDate) > new Date()) {
            ShowResult("please Select past and present Date!", 'failure');
            throw ('Invalid Request');
        }

        $http({
            method: 'POST',
            url: $scope.path + "getView",
            data: {'Id': $scope.EntityId, 'FromDate': $scope.FromDate},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
                throw ('Invalid Request!');
            }
            $scope.ViewGridPop= [];
            $scope.ViewGridPop = response.data;
            
        });
    }

     
    $scope.SaveAll = function () {
        $scope.$broadcast('show-errors-check-validity');

        if (angular.isUndefinedOrNull($scope.EntityId)) {
            ShowResult('Please select Entity !!', 'failure');
            throw ("Invalid");
        }

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'Data': $scope.ViewGridPop, 'Date': $scope.FromDate, 'LocationId': $scope.WasteLocationId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getView();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        
    };




       
       

   
        $scope.ModelTemp = {
            Id: null,
            Sequence: 0,
            ProcessId: null,
            UOMId: null,
            Category: null,
            SubCategory: null,
            ItemName: null,
            StandardRate: null,
            Code: null,
            Remarks: null,
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

        $scope.SelBudList = [];

        $scope.GetSequence = function () {
            cboService.getSequence($scope.getSeqUrl, function (data) {
                $scope.ModelTemp.Sequence = data;
                $scope.ModelNew.Sequence = data;
            });
        };
        $scope.GetSequence();

        $scope.Get = function (args) {

            var AllData = [];
            $http({
                method: 'POST',
                url: $scope.path + "Get",
                data: { 'Id': args.data.Id },
                dataType: 'JSON'
            }).then(function successCallback(resp) {
                $scope.BudgetIds = [];
                $scope.SelBudList = [];
                AllData = resp.data.master;
                var child = resp.data.child;
                var ob = {};
                $scope.ModelNew = Object.assign({}, AllData[0]);
                for (var i = 0; i < child.length; i++) {
                    ob[child[i].EntityId] = true;
                    $scope.EntityIds.push(child[i].BId);

                }

                for (var i = 0; i < $scope.EntityList.length; i++) {
                    if ($scope.EntityList[i].Id in ob) {
                        $scope.EntityList[i].isSelected = true;
                        $scope.SelEntList.push($scope.EntityList[i]);
                    }
                    else {
                        $scope.BudgetList[i].isSelected = false;
                    }
                }


            });

            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        };

        $scope.Save = function () {
            $scope.$broadcast('show-errors-check-validity');

            //if (angular.isUndefinedOrNull($scope.ModelNew.BudgetId)) {
            //    ShowResult('No Budget Code Selected!!' , 'failure');
            //    throw ("Invalid");
            //}

            if (angular.isUndefinedOrNull($scope.ModelNew.UOMId)) {
                ShowResult('No UOM Selected!!', 'failure');
                throw ("Invalid");
            }


            if ($scope.ModelNewForm.$valid) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'datas': $scope.ModelNew, 'Entity': $scope.EntityIds },
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
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }

            }
        };

        //$scope.Delete = function () {
        //    if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
        //        $http({
        //            method: 'POST',
        //            url: $scope.deleteUrl + $scope.ModelNew.Id,
        //            dataType: 'JSON'
        //        }).then(function successCallback(response) {
        //            if (response.data.Error === true) {
        //                ShowResult(response.data.Message, 'failure');
        //            }
        //            else {
        //                ShowResult(response.data.Message, 'success');
        //                ClearFields(response.data.Sequence);
        //                $scope.getData();
        //            }
        //            function errorCallBack(response) {
        //                ShowResult(response.data.Message, 'failure');
        //            }
        //        });
        //    }
        //};

        $scope.Clear = function () {
            ClearFields($scope.GetSequence());
            return true;
        };

        function ClearFields(seq) {
            $scope.Action = 'Save';
            $scope.CompanyId = null;
            $scope.PlantId = null;
            $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
            $scope.ModelNew.Sequence = seq;
            $scope.EntityIds = [];
            $scope.SelEntList = [];
        }

        // Addition of the Modal Operations for Budget Child
        $scope.closePopUp = function () {
            angular.element(document.querySelector('#EntityPop')).modal('hide');
        }

        $scope.EntityIds = [];

    $scope.WasteLocationId = null;

    $scope.getWasteLocationList = function () {
        try {
            $http({
                method: 'Get',
                url: 'Productions/GeneralWaste/GetWasteLocationList',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.WasteLocationList = response.data;
            });
        }
        catch (ex) {
            ShowResult(ex, "failure");
        }
    };
    $scope.getWasteLocationList();

    $scope.Report = function () {
        try {
            $scope.fileName = "GeneralWasteReport.xlsx";
            $http({
                method: 'POST',
                url: $scope.path + "GetGeneralWasteReport",
                data: { 'Id': $scope.EntityId },
                dataType: 'JSON'
            }).then(function successCallback(response) {

                if (response.data.Error == false) {
                    //$window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                    $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
}