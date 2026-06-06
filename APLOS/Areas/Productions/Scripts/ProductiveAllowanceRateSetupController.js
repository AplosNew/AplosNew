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
    $scope.getPaMasterData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getMasterData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PaHeaderList = response.data;
        });
    }
    $scope.getPaMasterData();

   

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
                $scope.getPaMasterData();
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
                $scope.getRsMasterData();
                $scope.getRsChildList($scope.HeaderRs.Id);
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

    //Get Plants List and Company List

    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getPlants',
            params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }


    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();

    $scope.BudgetPlantId = null;
    $scope.fileData = [];
    

    $scope.currentList = [];
    $scope.getCurrentFileList = function () {

        if ($scope.BudgetPlantId == "" || $scope.BudgetPlantId == undefined) {
            ShowResult("Please First Select a Plant!!", 'failure');
            throw ("Invalid!!");
        }

        $http({
            method: 'GET',
            url: $scope.path + 'getCurrentList',
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

    //Getting The Sample
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
            window.open('Productions/ProductiveAllowanceRateSetup/GetSampleReport?plantId=' + $scope.BudgetPlantId + '&name=' + plantName + '&reportFormat=' + reportFormat, '_blank');

        } catch (e) {

        }
    }

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
                url: $scope.path + 'ImportData',
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
            url: $scope.path + 'SaveFileList',
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

    ///// ******************************************** Special Operation Rate

    //Variables
    $scope.DatesList = [];
    $scope.EffectiveDateSP = null;
    $scope.SpOpMasterList = [];
    $scope.EntityListSP = [];
    $scope.ProcessListSP=[];

    $scope.EntitySpName = null;
    $scope.SpOp = {
        Id: null,
        EntityId: null,
        ProcessId: null,
        AllowancePercentage: null,
        Remarks:null,
    };

    // Getting the MAster
    $scope.getSPMsater = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getSpOpMaster',
        }).then(function successCallback(resp) {
            $scope.SpOpMasterList = resp.data;
        });

        

        $http({
            method: 'GET',
            url: $scope.path + "getEntitySP",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.EntityListSP = response.data;
        });
    }

    $scope.getSPMsater();

    $scope.getProcessSP = function () {
        $http({
            method: 'GET',
            url: $scope.path + "getProcessSP",
            params: {EntityId : $scope.SpOp.EntityId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ProcessListSP = response.data;
        });
    }

    //Double Clicking Master Table
    $scope.GetMasterData = function (e) {
        $scope.SpOp = e.data;
        $http({
            method: 'GET',
            url: $scope.path + 'getSpOpDates',
            params: {HeaderId : $scope.SpOp.Id}
        }).then(function successCallback(resp) {
            for (var i = 0; i < resp.data.length; i++) {
                $scope.DatesList.push(resp.data[i].EffectiveDate);
            }
            $scope.getProcessSP();
             //$("#SpPr").data("ejDropDownList").selectItemByText($scope.SpOp.EntityNameSp);
             //$("#SpEn").data("ejDropDownList").selectItemByText($scope.SpOp.ProcessNameSp);
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }

        });
    }
    //Changing Entity
    $scope.EntityChangeSp = function () {
        var obj = $('#dropDownEntity').data("ejDropDownList");
        $scope.EntitySpName = obj.option("Text");
        $scope.SpOp.EntityId = obj.option("Value");
    }

    //Seletion of Effective Dates
    $scope.EffectiveDateSP;
    $scope.DatesList = [];
    $scope.AddDates = function () {
        var c = 0;
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i].EffectiveDate === $scope.EffectiveDateSP) {
                c++;
            }
        }
        if (c === 0) {
            if (($scope.EffectiveDateSP + '').length < 21 && ($scope.EffectiveDateSP + '').length > 5) {

                $scope.DatesList.push($scope.EffectiveDateSP);
            }
        }
    }

    //Delete The Date
    $scope.DeleteDate = function (e) {
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i] === e) {
                $scope.DatesList.splice(i, 1);
            }
        }
    }

    //Saving the Data
    $scope.saveOperations = function () {

        //var DropDownJobLocationListObjP = $("#SpPr").data("ejDropDownList");
        //var Proc = DropDownJobLocationListObjP.getSelectedValue();

        //var DropDownJobLocationListObjE = $("#SpEn").data("ejDropDownList");
        //var En = DropDownJobLocationListObjE.getSelectedValue();

        //if (Proc.length < 1) {
        //    ShowResult('Process/Processes are not selected!', 'failure');
        //    throw ("Invalid Request!");
        //}

        //if (En.length < 1) {
        //    ShowResult('Entity/Entities are not selected!', 'failure');
        //    throw ("Invalid Request!");
        //}

        //$scope.SpOp.EntityId = En;
        //$scope.SpOp.ProcessId = Proc;

        $http({
            method: 'POST',
            url: $scope.path + "saveOperations",
            data: {
                'data': $scope.SpOp , 'dates':$scope.DatesList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {

                ShowResult(response.data.Msg, 'success');
                $scope.getOperations();
                $scope.clearOperations();
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });

    }

    //Clearing of the Data
    $scope.clearOperations = function () {
        $scope.DatesList = [];
        $scope.EffectiveDateSP = null;

        $scope.SpOp = {
            Id: null,
            EntityId: null,
            ProcessId: null,
            AllowancePercentage: null,
            Remarks: null,
        };
        $("#SpEn").data("ejDropDownList").clearText();
        $("#SpPr").data("ejDropDownList").clearText();


    }


    /// The Page for EmployeeTimeOut Applicable

    $scope.title = "Employee Time Out Applicable";
    $scope.Action = 'Save';
    $scope.ModelListeet = [];
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    //$scope.searchBy = "UserName"; $scope.search = "";
    //$scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.companyListeet = [];
    $scope.entityListeet  = [];
    $scope.plantListeet  = [];
    $scope.processListeet  = [];
    $scope.companyIdeet  = null;
    $scope.plantIdeet  = null;

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            //data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelListeet = response.data;
            
            ClearFields();

        });

        $http({
            method: 'GET',
            url: $scope.path + 'GetCompanys',
            //params: { 'cmp': $scope.Company }
        }).then(function success(response) {
            $scope.companyListeet  = response.data;
        })
       

    }
    $scope.getData();

    $scope.getPlant = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetPlant",
            params: { cmp: $scope.companyIdeet  },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.plantListeet  = response.data;
        });
    }

    $scope.getEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetEntity",
            params: { plant: $scope.plantIdeet  },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.entityListeet  = response.data;
        });
    }

    $scope.getProcesses = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetProcess",
            params: { entity: $scope.ModelNeweet.EntityId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.processListeet  = response.data;
        });
    }

    $scope.ModelTemp = {
        Id: null,
        EntityId: null,
        ProcessId: null,
        IsApplicable: false,
    };
    $scope.ModelNeweet = Object.assign({}, $scope.ModelTemp);



    $scope.Get = function (args) {

        $scope.ModelNeweet = Object.assign({}, args.data);
        $scope.Action = 'Update';

        $scope.plantIdeet = $scope.ModelNeweet.PlantId;
        $scope.companyIdeet = $scope.ModelNeweet.CompanyId;

        $scope.getPlant();
        $scope.getEntity();
        $scope.getProcesses();

        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNeweet },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Deletet = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNeweet.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNeweet.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields();
                    $scope.getData();
                    $('#myModal').modal('hide');
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = 'Save';
        $scope.ModelNeweet = Object.assign({}, $scope.ModelTemp);
    }

    // Getting the Order Size MasterData start

    $scope.orderSizeObj = {
        Id: null,
        Days: null,
        Basic: null,
        Critical: null,
        SemiCritical: null,
        Special: null,
        Remark: null
    };

    $scope.orderSizeList = [];
    $scope.getOrderSizeAllowance = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getOrderSizeAllowanceData",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.orderSizeList = response.data;
        });
    }
    $scope.getOrderSizeAllowance();

    $scope.GetOrderSize = function (args) {

        $scope.orderSizeObj = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };
    //$scope.getRsHeaderGrid = function (e) {
    //    var processArr = e.data.Processes.split(',');
    //    var entityArr = e.data.Entity.split(',');

    //    var Prs = $("#selProcessrs").data("ejDropDownList").selectItemByText(processArr);
    //    var Ers = $("#selEntityrs").data("ejDropDownList").selectItemByText(entityArr);
    //    Object.assign($scope.orderSizeObj, e.data);
    //    if (!$rootScope.isCollapsed) {
    //        $rootScope.toggle();
    //    }

    //}

    $scope.SaveOrderSize = function () {
        $http({
            method: 'POST',
            url: $scope.path + "saveOrderSizeAllowance",
            data: {
                'Data': $scope.orderSizeObj,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {

            if (response.data.Error == "No") {
                ShowResult(response.data.Msg, 'success');
                Object.assign($scope.orderSizeObj, response.data.Data);
                $scope.getOrderSizeAllowance();
            }
            else {
                ShowResult(response.data.Msg, 'failure');
            }
        });
    }

    $scope.clearOrderSize = function () {
        $scope.orderSizeObj = {
            Id: null,
            Days: null,
            Basic: null,
            Critical: null,
            SemiCritical: null,
            Special: null,
            Remark: null
        };
        $scope.Action = 'Save';

    }
}

//-----------------------------------------------------------------------------------

function openModal() {
    $('.confirm-delete').addClass('hide');
    $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
    $('#myModal').modal('show');
}
//-----------------------------------------------------------------------------------