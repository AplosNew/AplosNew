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

    //--------------------------------
    //--------------------------------

    // Rate Setup Start Module

    // Getting the RS MasterData start
    $scope.getRsMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getRsMasterData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RsHeaderList = response.data;
        });
    }
    $scope.getRsMasterData();
     // Getting the RS MasterData end

    //Double Clicking The RS Header Grid
    $scope.getRsHeaderGrid = function (e) {
        var processArr = e.data.Processes.split(',');
        var entityArr = e.data.Entity.split(',');

        var Prs = $("#selProcessrs").data("ejDropDownList").selectItemByText(processArr);
        var Ers = $("#selEntityrs").data("ejDropDownList").selectItemByText(entityArr);
        Object.assign($scope.HeaderRs, e.data);
        //$scope.HeaderPa.Id = e.data.Id;
        //$scope.HeaderPa.UserName = e.data.UserName;
        //$scope.HeaderPa.EffectiveDate = e.data.EffectiveDate;
        //$scope.HeaderPa.Remarks = e.data.Remarks;
        $scope.getRsChildList($scope.HeaderRs.Id);

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    }

    //Saving the Header For RateSetup
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

    //Clearing Header RS
    $scope.clearHeaderRS = function () {
        $scope.HeaderRs = {
            Id: null,
            UserName: null,
            EffectiveDate: null,
            Remarks: null
        };
        $("#selProcessrs").data("ejDropDownList").clearText();
        $("#selEntityrs").data("ejDropDownList").clearText();
        $scope.RsChildList = [];
    }


    // Get Rs Child Function List

    $scope.getRsChildList = function (s) {
        $http({
            method: 'POST',
            url: $scope.path + "getRsChildList",
            data: { 'Id': s },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.RsChildList = response.data;
        });
    }

    //Saving the Rs Child List
    $scope.saveChildRs = function () {
        $http({
            method: 'POST',
            url: $scope.path + "saveChildRs",
            data: {
                'childData': $scope.RsChildList,
                'headerId': $scope.HeaderRs.Id,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {

                ShowResult(response.data.Msg, 'success');
                //$scope.HeaderPa = response.data.Data;
                Object.assign($scope.RsChildList, response.data.Data);
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });
    }

    // Clearing Child Rs
    $scope.clearChildRs = function () {
        for (var i = 0; i < $scope.RsChildList.length; i++) {
            $scope.RsChildList[i].Effeciency = 0;
            $scope.RsChildList[i].EffeciencyRate = 0;           
            $scope.RsChildList[i].Remarks = null;
        }
    }

    //  Budget Applicable

    $scope.BudgetPlantId = null;
    $scope.fileData = [];
    $scope.GetSample = function () {
        var reportFormat = "Excel";

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        var plantName = "";
        for (var i = 0; i < $scope.PlantList.length; i++) {
            if ($scope.PlantList[i].Value == $scope.BudgetPlantId) {
                plantName = $scope.PlantList[i].Text;
            }
        }

        try {
            window.open('humanresource/RosterPattern/GetSampleReport?plantId=' + $scope.BudgetPlantId + '&name=' + plantName + '&reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        $http({
            method: 'GET',
            url: url + 'getCurrentList',
            params: { 'plantId': $scope.BudgetPlantId }
        }).then(function success(response) {
            $scope.currentList = [];
            $scope.currentList = response.data;
        })
    }


    $("#uploadFile").change(function () {
        $scope.fileData = this.files[0];
    });
    $scope.ExcelUploadData = [];
    //IMporting The Data From the Excel File

    $scope.ModelNew = {
        FileName: null
    }


    $scope.ImportData = function () {
        try {
            $scope.ExcelUploadData = [];
            $scope.msg = "";
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.fileData.length == 0) {

                throw ("Please Select A File!!");
            }
            if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
                ShowResult("Please First Select a Plant!!", 'failure');
                throw ("Please First Select a Plant!!");
            }

            var fileData = new FormData();
            if (!baseService.isUndefinedOrNull($scope.fileData)) {
                $scope.ModelNew.FileName = $scope.fileData.name;
            }

            $http({
                method: 'POST',
                url: url + 'ImportData',
                headers: { 'Content-Type': undefined },
                transformRequest: function (data) {
                    fileData.append("modelNew", angular.toJson(data.modelNew));
                    if (baseService.isUndefinedOrNull($scope.fileData) === false) {
                        fileData.append('file', data.file);
                        fileData.append('plantId', $scope.BudgetPlantId);
                    }
                    return fileData;
                },
                data: { 'modelNew': $scope.ModelNew, 'file': $scope.fileData, 'plantId': $scope.BudgetPlantId }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, "failure");

                }

                else {
                    try {
                        $scope.ExcelUploadData = response.data;
                    }

                    catch (e) {

                        ShowResult(e, "failure");
                    }

                }
            }, function errorCallback(response) {

            });
            return true;


        } catch (e) {

            ShowResult(e, "failure");
        }
    };

    //Save the File Data
    $scope.saveFileList = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Please First Select a Plant!!");
        }



        $http({
            method: 'POST',
            url: url + 'SaveFileList',
            data: { 'data': $scope.ExcelUploadData, 'plantId': $scope.BudgetPlantId }
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                try {
                    if ($rootScope.isCollapsed == true) {
                        $rootScope.toggle();
                    }
                    $scope.getCurrentFileList();
                    ShowResult(response.data.Message, 'success')
                }
                catch (e) {

                    ShowResult(e, "failure");
                }
            }
        }, function errorCallback(response) {

        });
    }
    
}