'use strict';
OutPunchConfigurationController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function OutPunchConfigurationController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Out Punch Configuration';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/OutPunchConfiguration/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        PlantId: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        InPunchStartTime: null,
        LastPunchOutTime: null,
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.PlantList = [];
    function getPlants() {
        $http({
            method: 'POST',
            url: 'HumanResource/DayStatusMaster/getPlants',

        }).then(function success(response) {
            $scope.PlantList = [];
            $scope.PlantList = response.data;
        })
    };




    getPlants();




    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        //$scope.ModelNew = Object.assign({}, args.data);
        //$scope.ShiftChildList = args.data.Child;
        $http({
            method: 'POST',
            url: $scope.path + 'Get',

            data: {'Id':args.data.Id},
        }).then(function success(response) {
            $scope.ShiftChildList = [];
            $scope.ModelNew = Object.assign({}, response.data.Master[0]);
            $scope.ShiftChildList = response.data.Child;
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
            $scope.Sequences = response.data.Child.length;
            console.log($scope.ModelNew)
        })
        
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {

            if ($scope.ShiftChildList.length > 0) {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'data': $scope.ModelNew, 'child': $scope.ShiftChildList },
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
            else {
                alert("Please Add Child Shifts!!");
                throw ("Please Add Child Shifts!!");
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
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
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.Sequences = 0;
        $scope.ShiftChildList = [];
    }


    //Child Codes Adding and Deleting

    $scope.Sequences = 0;
    $scope.ShiftChildList = [];

    $scope.InPunchLimit = "";
    $scope.OutPunchLimit = "";
    $scope.AddShift = function () {
        var obj = {
            Id : null,
            MasterId: null,
            Sequence: 0,
            InPunchLimit: null,
            OutPunchLimit: null 
        };
        $scope.Sequences++;
        obj.Sequence = $scope.Sequences;
        $scope.ShiftChildList.push(obj);
        //refresh();
    }

    $scope.AddTile = function (e) {
        var obj = {
            Id: null,
            MasterId: null,
            Sequence: 0,
            InPunchLimit: null,
            OutPunchLimit: null
        };
        $scope.Sequences++;
        obj.Sequence = $scope.Sequences;
        obj.InPunchLimit = e.InPunchLimit;
        obj.OutPunchLimit = e.OutPunchLimit;
        $scope.ShiftChildList.push(obj);
    }

    $scope.DeleteTile = function (e) {
        for (var i = 0; i < $scope.ShiftChildList.length; i++) {
            if ($scope.ShiftChildList[i]["Sequence"] == e.Sequence) {
                $scope.ShiftChildList.splice(i, 1);
            }
        }
    }

    $scope.refreshSequence = function () {
        if ($scope.ShiftChildList.length > 0) {
            for (var i = 0; i < $scope.ShiftChildList.length; i++) {
                $scope.ShiftChildList[i]["Sequence"] = i + 1;
            }
        }
        refresh();
    }

    
    function refresh() {
        var gridObj = $("#GridEdit").data("ejGrid");
        gridObj.dataSource($scope.ShiftChildList);
    }
}