'use strict';
ProductiveAllowanceRateSetupController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'cboService'];
function ProductiveAllowanceRateSetupController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, cboService) {
    $rootScope.title = 'Productive Allowance & Rate Setup';
    $scope.Action = 'Save';
    $scope.path = 'Productions/ProductiveAllowanceRateSetup/';

    //Tabs Changes
    function Tabs() {
        var bindAll = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].addEventListener('click', change, false);
            }
        }

        var clear = function () {
            var menuElements = document.querySelectorAll('[data-tab]');
            for (var i = 0; i < menuElements.length; i++) {
                menuElements[i].classList.remove('active');
                var id = menuElements[i].getAttribute('data-tab');
                document.getElementById(id).classList.remove('active');
            }
        }

        var change = function (e) {
            clear();
            e.target.classList.add('active');
            var id = e.currentTarget.getAttribute('data-tab');
            document.getElementById(id).classList.add('active');
            if ($rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        }

        bindAll();
    }

    var connectTabs = new Tabs();


    //Variables
    $scope.HeaderPa = {
        Id: null,
        UserName: null,
        EffectiveDate: null,
        Remarks: null
    };

    $scope.HeaderRs = {
        Id: null,
        UserName: null,
        EffectiveDate: null,
        Remarks: null
    };


    //Data Lists
    $scope.ProcessList = [];
    $scope.EntityList = [];
    $scope.PaHeaderList = [];
    $scope.RsHeaderList = [];
    $scope.PaChildList = [];
    $scope.RsChildList = [];


    //Getting the Initial Data Sets

    $scope.getAllData = function(){
        $http({
            method: 'POST',
            url: $scope.path + "getProcess",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessList = response.data;
        });

        $http({
            method: 'POST',
            url: $scope.path + "getEntity",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityList = response.data;
        });


        
    }

    $scope.getAllData();
    
    //Getting the MasterData
    $scope.getMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMasterData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PaHeaderList = response.data;
        });
    }
    $scope.getMasterData();

    //Double Clicking The PA Header Grid
    $scope.getPaHeaderGrid = function (e) {
        var processArr = e.data.Processes.split(',');
        var entityArr = e.data.Entity.split(',');

        var Prs = $("#selProcess").data("ejDropDownList").selectItemByText(processArr);
        var Ers = $("#selEntity").data("ejDropDownList").selectItemByText(entityArr);
        Object.assign($scope.HeaderPa, e.data);
        //$scope.HeaderPa.Id = e.data.Id;
        //$scope.HeaderPa.UserName = e.data.UserName;
        //$scope.HeaderPa.EffectiveDate = e.data.EffectiveDate;
        //$scope.HeaderPa.Remarks = e.data.Remarks;
        $scope.getPaChildList($scope.HeaderPa.Id);

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        
    }

    //Saving the Header For ProductiveAllowance
    $scope.saveHeaderPA = function () {

        //Getting the Values from the DropDowns
        var DropDownJobLocationListObjP = $("#selProcess").data("ejDropDownList");
        var processLists =DropDownJobLocationListObjP.getSelectedValue().split(",");

        var DropDownJobLocationListObjE = $("#selEntity").data("ejDropDownList");
        var entityLists = DropDownJobLocationListObjE.getSelectedValue().split(",");

        if (processLists.length < 1) {
            ShowResult('Process/Processes are not selected!', 'failure');
            throw ("Invalid Request!");
        }

        if (entityLists.length < 1) {
            ShowResult('Entity/Entities are not selected!', 'failure');
            throw ("Invalid Request!");
        }

        $http({
            method: 'POST',
            url: $scope.path + "saveHeaderPa",
            data: {
                'headerData':$scope.HeaderPa,
                'process':processLists,
                'entity':entityLists,
                },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {
                ShowResult(response.data.Msg, 'success');
                //$scope.HeaderPa = response.data.Data;
                Object.assign($scope.HeaderPa, response.data.Data);
                $scope.getMasterData();
                $scope.getPaChildList($scope.HeaderPa.Id);
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });
    }


    //Saving the Header For rateSetup
    $scope.saveHeaderRS = function () {

        //Getting the Values from the DropDowns
        var DropDownJobLocationListObjP = $("#selProcessrs").data("ejDropDownList");
        var processListsrs = DropDownJobLocationListObjP.getSelectedValue().split(",");

        var DropDownJobLocationListObjE = $("#selEntityrs").data("ejDropDownList");
        var entityListsrs = DropDownJobLocationListObjE.getSelectedValue().split(",");

        if (processListsrs.length < 1) {
            ShowResult('Process/Processes are not selected!', 'failure');
            throw ("Invalid Request!");
        }

        if (entityListsrs.length < 1) {
            ShowResult('Entity/Entities are not selected!', 'failure');
            throw ("Invalid Request!");
        }

        $http({
            method: 'POST',
            url: $scope.path + "saveHeaderRs",
            data: {
                'headerData': $scope.HeaderRs,
                'process': processListsrs,
                'entity': entityListsrs,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {
                ShowResult(response.data.Msg, 'success');
                //$scope.HeaderPa = response.data.Data;
                Object.assign($scope.HeaderRs, response.data.Data);
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });
    }

    //Clearing Header PA
    $scope.clearHeaderPA = function () {
        $scope.HeaderPa = {
            Id: null,
            UserName: null,
            EffectiveDate: null,
            Remarks: null
        };
        $("#selProcess").data("ejDropDownList").clearText();
        $("#selEntity").data("ejDropDownList").clearText();
        $scope.PaChildList = [];
    }

    //Child Tab Showing
    //var j = document.getElementById("tab_show");
    //j.style.display = "none";
    ////Showing the Childs
    //function showTabs() {
    //    if ($scope.Header.Id != null) {
    //        j.style.display = "block";
    //    }
    //    else {
    //        j.style.display = "none";
    //    }
    //}



    // Get Child Function

    $scope.getPaChildList = function (s) {
        $http({
            method: 'POST',
            url: $scope.path + "getPaChildList",
            data : {'Id' : s},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PaChildList = response.data;
        });
    }

    //Saving the Pa Child List
    $scope.saveChildPa = function () {
        $http({
            method: 'POST',
            url: $scope.path + "saveChildPa",
            data: {
                'childData': $scope.PaChildList,
                'headerId': $scope.HeaderPa.Id,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {
                ShowResult(response.data.Msg, 'success');
                //$scope.HeaderPa = response.data.Data;
                Object.assign($scope.PaChildList, response.data.Data);
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });
    }

    // Clearing Child Pa
    $scope.clearChildPa = function () {
        for (var i = 0; i < $scope.PaChildList.length; i++) {
            $scope.PaChildList[i].SkillAllowance = 0;
            $scope.PaChildList[i].AdditionOperationAllowance = 0;
        }
    }
    
}