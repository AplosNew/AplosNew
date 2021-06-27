
'use strict';
EfficiencySlabController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function EfficiencySlabController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Efficiency Slab";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/EfficiencySlab/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete';
    

    $scope.Detail = {
        Id: null,
        Minimum: 0,
        Maximum: 0,
        FirstDayEfficiency: 0,
        Increment: 0,
        LastDayEfficiency: 0
    }
    $scope.DataList = [];
    $scope.DataList.push(Object.assign({}, $scope.Detail));
    
    $scope.Clear = function () {
        $scope.Detail = {
            Id:null,
            Minimum: 0,
            Maximum: 0,
            FirstDayEfficiency: 0,
            Increment: 0,
            LastDayEfficiency: 0         
        }
    }

   
    $scope.Remove = function (index) {
        var removed = $scope.DataList.splice(index, 1);
        $scope.Detail = removed;
        //$scope.Detail.pop();
    }
    $scope.SubmitH = function (data) {

        try {
            if (data.Minimum < 0)
                throw 'Minimum value cannot be negative';

            if (data.Maximum < 0)
                throw 'Maximum value cannot be negative';


            if (data.Minimum >= data.Maximum)
                    throw 'Maximum value should be greater than minimum value';

            
        
        var newObj = Object.assign({}, $scope.Detail);
        if (data != null) {
            newObj = {
                Id:null,
                Minimum: data.Maximum,
                Maximum: 0,
                FirstDayEfficiency: 0,
                Increment: 0,
                LastDayEfficiency: 0    
                
            }
        }
        
            $scope.DataList.push(newObj);
        } catch (e) {
            ShowResult(e, 'failure');
        }

    };

    $scope.ChangePlant = function (args) {
        $scope.GetAllSlab();
    }

    $scope.Save = function () {


        try {
            for (var i = 0; i < $scope.DataList.length; i++) {

                if ($scope.DataList[i]['Minimum'] < 0)
                    throw 'Minimum value cannot be negative';

                if ($scope.DataList[i]['Maximum'] < 0)
                    throw 'Maximum value cannot be negative';


                if ($scope.DataList[i]['Minimum'] >= $scope.DataList[i]['Maximum'])
                    throw 'Maximum value should be greater than minimum value';

                if ($scope.DataList[i]['Maximum'] == 0)
                    throw 'Maximum value cannot be empty';

                if ($scope.DataList[i]['Maximum'] <= $scope.DataList[i]['Minimum'])
                    throw 'Maximum value should not be less than minimum value';

                if ($scope.DataList[i]['FirstDayEfficiency'] == 0)
                    throw 'FirstDayEfficiency cannot be empty';

                if ($scope.DataList[i]['Increment'] == 0)
                    throw 'Increment cannot be empty';

                if ($scope.DataList[i]['LastDayEfficiency'] == 0)
                    throw 'LastDayEfficiency cannot be empty';

                if ($scope.SelectedPlantId == null || $scope.SelectedPlantId == '') {
                    ShowResult('Select Plant', 'failure');
                    return;
                }
            }

            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.DataList, 'PlantId': $scope.SelectedPlantId },
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

            };

        } catch (e) {
            ShowResult(e, 'failure');
        }

      
    }
    $scope.Delete = function () {
           $http({
               method: 'GET',
               url: $scope.deleteUrl + '?id=' + $scope.SelectedPlantId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.DataList = [];
                    $scope.DataList.push(Object.assign({}, $scope.Detail));

                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        
    };


    $scope.SelectedPlantId = null;
    $scope.PlantList = [];

    $scope.GetAllSlab = function () {
        $scope.DataList = [];
        $http({
            method: 'POST',
            url: $scope.path + "LoadEfficiencySlab",
            data: { 'PlantId': $scope.SelectedPlantId},
            dataType: 'JSON'

        }).then(function successCallback(response) {
            if(response.data.DATA.length>0)
                $scope.DataList = response.data.DATA;
            else
                $scope.DataList.push(Object.assign({}, $scope.Detail));


        }),function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }

    }

    $scope.LoadPlantListData = function (args) {
        $http({
            method: 'POST',
            url: $scope.path + "GetPlantList",
            data: {},
            dataType: 'JSON'

        }).then(function successCallback(response) {
            if (response.data.Error == false) {


                $scope.PlantList = response.data.DATA;

            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }
    }
    $scope.LoadPlantListData();

    $scope.changeCompany = function (args) {
        $scope.ProcessAndInventorySeq = [];

        cboService.getCboPlantByCompany($scope.SelectedId, function (result) {
            $scope.PlantList = result;
        });
    }

    $scope.SelectedId = null;
    $scope.CompanyList = [];
    $scope.LoadCompanyListData = function (args) {
        $http({
            method: 'POST',
            url: $scope.path + "GetCompanyList",
            data: {},
            dataType: 'JSON'

        }).then(function successCallback(response) {
            if (response.data.Error == false) {


                $scope.CompanyList = response.data.DATA;

            }
            else {
                ShowResult(response.data.Message, 'failure');
            }

        }),

            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');

            }

    }

    $scope.LoadCompanyListData();

}