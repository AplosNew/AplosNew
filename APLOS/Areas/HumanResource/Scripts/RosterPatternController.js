'use strict';
RosterPatternController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function RosterPatternController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Roster Pattern Creation';
    $rootScope.title1 = 'Roster Pattern Planning';
    $scope.Action = 'Save';
    var url = "humanresource/RosterPattern/";

    $scope.Action = "Save";
    $scope.Action1 = "Save";

    var headerId = document.getElementById("RosterId");


    //The PAging System
    var x = document.getElementById("FDiv");
    var y = document.getElementById("SDiv");
    x.style.display = "block";
    y.style.display = "none";
   
    $scope.clickdde1 = function () {
        if (x.style.display === "none") {
            y.style.display = "none";
            x.style.display = "block";
           
        }
    };

    $scope.clickdde2 = function () {
        if (y.style.display === "none") {

            y.style.display = "block";
            x.style.display = "none";

        }
    };
   

    //// The Region For Roster Processing (Not to be included in this code Just for Testing)
    $scope.FromDateF;
    $scope.ToDateF;
    $scope.Run = function () {
        $http({
            method: 'GET',
            url: url + 'run',
        }).then(function success(response) {
            console.log("Done!!");
        });
    }

    // The End Region


    //The Header Modal
    $scope.Header = {
        Id: null,
        StandardName: null,
        ShortName: null,
        Description: null,
        Remarks: null,
        Active: false,
        PlantId: null,
        UserName: null,
    }

    //Get Plants List and Company List
    $scope.PlantList = [];
    $scope.getPlants = function () {
        $http({
            method: 'GET',
            url: url + 'getPlants',
            params: {'cmp':$scope.Company}
        }).then(function success(response) {
            $scope.PlantList = response.data;
        })
    }


    $scope.Company = null;
    $scope.CompanyList = [];
    $scope.getCompany = function () {
        $http({
            method: 'GET',
            url: url + 'getCompany'
        }).then(function success(response) {
            $scope.CompanyList = response.data;
        })
    }

    $scope.getCompany();
    $scope.weeklyStatusList = [];
    $scope.getWeeklyStatus = function () {
        $http({
            method: "GET",
            url: "HumanResource/RosterPattern/GetWeeklyStatusCbo"
        }).then(function successCallback(response) {
            $scope.weeklyStatusList = response.data;
        });
    }
    $scope.getWeeklyStatus();
    //Get The Main Master Grid
    $scope.masterGrid = [];
    $scope.getMaster = function () {
        $http({
            method: 'GET',
            url: url + 'getMaster'
        }).then(function success(response) {
            $scope.masterGrid = response.data;
        })
    }

    $scope.getMaster();
    var restoreShiftsChild = [];
    //Double Click on Master
    $scope.fillUpdates = function (e) {
        $scope.Header = e.data;
        $http({
            method: 'GET',
            url: url + 'getChilds',
            params: { 'Id': e.data.Id },
        }).then(function success(response) {
            if ($rootScope.isCollapsed == false) {
                $rootScope.toggle();
            }
            headerId.style.display = "block";
            $scope.DatesList = response.data.Dates;
            $scope.Action = "Update";
            $scope.Action1 = "Update";

            $scope.ShiftChildList = response.data.Shifts;
            restoreShiftsChild = $scope.ShiftChildList.length;
            var ll = $scope.ShiftChildList.length;
            $scope.Sequences = ll;
        })
    }



    //Shifts List Modal
    $scope.ShiftsList = [];
    $scope.AddShift = function () {

        $http({
            method: 'GET',
            url: url + 'SearchShift',
            params: { 'PlantId': $scope.Header.PlantId }
        }).then(function success(response) {
            $scope.ShiftsList = [];
            $scope.ShiftsList = response.data.Data;
            angular.element(document.querySelector('#ShiftModal')).modal('show');
        })
    }

    //Double Click Inside Shift Modal
    $scope.ShiftChildList = [];
    $scope.Sequences = 0;
    $scope.shiftSelected = function (e) {
        if ($scope.Header.Id == null || $scope.Header.Id == undefined || $scope.Header.Id.length < 3 || $scope.Header.PlantId == null || $scope.Header.PlantId == undefined) {
            ShowResult("Please First Save the Roster !!" , 'failure');
            throw ("Please First Save the Roster !!");
        }
        if ($scope.isEdit === 0) {
            var obj = {
                Id: null,
                RPHeaderId: null,
                ShiftSequence: 0,
                Days31: null,
                Days30: null,
                Days29: null,
                Days28: null,
                ShiftDefinitionID: null,
                ShiftName: null,
            }

            obj.RPHeaderId = $scope.Header.Id;
            obj.ShiftSequence = $scope.Sequences;
            obj.ShiftDefinitionID = e.data.ShiftDefinationID;
            obj.ShiftName = e.data.ShiftDefinationName;

            $scope.ShiftChildList.push(obj);
            angular.element(document.querySelector('#ShiftModal')).modal('hide');
        }
        else {
            //console.log($scope.editSequence);
            for (var i = 0; i < $scope.ShiftChildList.length; i++) {
                if ($scope.ShiftChildList[i].ShiftSequence === $scope.editSequence) {
                    $scope.ShiftChildList[i].ShiftDefinitionID = e.data.ShiftDefinationID;
                    $scope.ShiftChildList[i].ShiftName = e.data.ShiftDefinationName;
                    break;
                }
            }
            $scope.isEdit = 0;
            $scope.editSequence = 0;
            refresh();
            angular.element(document.querySelector('#ShiftModal')).modal('hide');
        }

    }

    function refresh() {
        var gridObj = $("#ShiftList").data("ejGrid");
        gridObj.dataSource($scope.ShiftChildList);
    }
    //The Add Tile in the Shifts Child Grids
    $scope.AddTile = function (e) {
        console.log(e);
        var obj = {
            Id: null,
            RPHeaderId: null,
            ShiftSequence: 0,
            Days31: null,
            Days30: null,
            Days29: null,
            Days28: null,
            ShiftDefinitionID: null,
            ShiftName : null,
        }

        $scope.Sequences++;
        obj.RPHeaderId = e.RPHeaderId;
        obj.ShiftSequence = $scope.Sequences;
        obj.ShiftDefinitionID = e.ShiftDefinitionID;
        obj.ShiftName = e.ShiftName;
        $scope.ShiftChildList.push(obj);
    }

    //Selection of Edit Tile
    $scope.isEdit = 0;
    $scope.editSequence = 0;
    $scope.EditTile = function (e) {

        if ($scope.ShiftsList.length > 0) {
            angular.element(document.querySelector('#ShiftModal')).modal('show');
            $scope.isEdit = 1;
            $scope.editSequence = 0;
            $scope.editSequence = e.ShiftSequence;
        }
        else {
            $scope.AddShift();
            angular.element(document.querySelector('#ShiftModal')).modal('show');
            $scope.isEdit = 1;
            $scope.editSequence = 0;
            $scope.editSequence = e.ShiftSequence;
        }


    }

    //Delete Tile in the Shifts Child Grids
    $scope.DeleteTile = function (e) {
        for (var i = 0; i < $scope.ShiftChildList.length; i++) {
            if ($scope.ShiftChildList[i].ShiftSequence === e.ShiftSequence) {
                $scope.ShiftChildList.splice(i, 1);
            }
        }


    }


    //Seletion of Executive Dates
    $scope.EffectiveDate;
    $scope.DatesList = [];
    $scope.AddDates = function () {
        var c = 0;
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i].EffectiveDate === $scope.EffectiveDate) {
                c++;
            }
        }
        if (c === 0) {
            if (($scope.EffectiveDate + '').length < 21 && ($scope.EffectiveDate + '').length > 5) {

                $scope.DatesList.push({ Id: null, RPHeaderId: null, EffectiveDate: $scope.EffectiveDate });
            }
        }
    }

    //Delete The Date
    $scope.DeleteDate = function (e) {
        for (var i = 0; i < $scope.DatesList.length; i++) {
            if ($scope.DatesList[i].EffectiveDate === e) {
                $scope.DatesList.splice(i, 1);
            }
        }
    }

    //Save Master Data and Effective Dates Child
    $scope.SaveMasters = function () {
        $scope.$broadcast('show-errors-check-validity');

        validationsMaster();

        $http({
            method: 'POST',
            url: url + "saveMasters",
            data: { 'Master': $scope.Header, 'Effective': $scope.DatesList },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Header.Id = response.data.ids;
                headerId.style.display = "block";
                $scope.Action = "Update";
                $scope.getMaster();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    //Delete Master
    //$scope.Delete = function () {
    //    if (restoreShiftsChild > 0) {
    //        ShowResult("There are Child Data in this Master. First Delete Those!", 'failure');
    //        throw ("There are Child Data in this Master. First Delete Those! If Already Deleted then Update it!");
    //    }

    //    $http({
    //        method: 'POST',
    //        url: url + 'deleteMaster',
    //        data: { 'id': $scope.Header.Id }
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //            $scope.getMaster();
    //            $scope.Clear();
    //            if ($rootScope.isCollapsed) {
    //                $rootScope.toggle();
    //            }
    //        }
    //        function errorCallBack(response) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //    });
    //}

    //Clear Masters
    $scope.ClearMasters = function () {
        $scope.Header = {
            Id: null,
            StandardName: null,
            ShortName: null,
            Description: null,
            Remarks: null,
            Active: false,
            PlantId: null,
            UserName: null,
        };
        $scope.DatesList = [];
        if ($scope.ShiftChildList.length > 0) {
            $scope.ShiftChildList = [];
        }
        headerId.style.display = "none";
        $scope.Action = "Save";
        $scope.Action1 = "Save";
        $scope.Sequences = 0;
    }


    //Save Shifts Child with the RosterId

    $scope.checkChildList = function () {
        if ($scope.ShiftChildList.length == 0) {
            angular.element(document.querySelector('#confirmPopUpChild')).modal('show');
        }
        else {
            $scope.SaveShifts();
        }
    }

    $scope.SaveShifts = function () {
        try {


            if ($scope.Header.Id === null || $scope.Header.Id === undefined || $scope.Header.Id.length < 3) {
                alert("Please First Create A Roster Master!!");
                throw ("Please First Create A Roster Master!!")
            }

            $http({
                method: 'POST',
                url: url + "saveShifts",
                data: { 'Shifts': $scope.ShiftChildList , 'HeaderId' : $scope.Header.Id},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    if ($scope.Action1 == "Save") {
                        if ($rootScope.isCollapsed == true) {
                            $rootScope.toggle();
                        }
                        $scope.ClearMasters();
                        $scope.getMaster();
                    }
                    else {
                        $scope.getMaster();
                        $scope.restoreChildShifts();
                    }

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }
        catch (e) {
            throw e;
        }
    }

    //restore Child Shifts
    $scope.restoreChildShifts = function () {
        $http({
            method: 'GET',
            url: url + 'getChilds',
            params: { 'Id': $scope.Header.Id },
        }).then(function success(response) {
            $scope.ShiftChildList = response.data.Shifts;
            restoreShiftsChild = $scope.ShiftChildList.length;
            var ll = $scope.ShiftChildList.length;
            $scope.Sequences = ll;
        })
        angular.element(document.querySelector('#confirmPopUpChild')).modal('hide');
    }
    //Clear Shifts
    $scope.ClearShifts = function () {
        if ($scope.ShiftChildList.length > 0) {
            $scope.ShiftChildList = [];
            $scope.Sequences = 0;
        }
    }


    //Refreshing Sequence
    $scope.refreshSequence = function () {
        var c = 0;
        if ($scope.ShiftChildList.length > 0) {
            for (var i = 0; i < $scope.ShiftChildList.length; i++) {
                c++;
                $scope.ShiftChildList[i].ShiftSequence = c;
            }
        }
        refresh();
    }


    //Validations Section
    function validationsMaster() {
        if ($scope.Header.StandardName == null || $scope.Header.ShortName == null || $scope.Header.Description == null || $scope.Header.UserName == null || $scope.Header.PlantId == null) {
            alert("Please Fill All the necessary ");
            throw ("Please Fill All the necessary ");
        }

        if ($scope.DatesList.length <= 0) {
            alert("Please Select Atleast 1 Effective Date ");
            throw ("Please Select Atleast 1 Effective Date ");
        }
    }
    /////
    /////
    /////
    ///Everything For the 2nd Page Budget
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
            window.open('humanresource/RosterPattern/GetSampleReport?plantId='+$scope.BudgetPlantId+'&name='+plantName+'&reportFormat=' + reportFormat, '_blank');

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
            params:{'plantId':$scope.BudgetPlantId}
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
            if ($scope.fileData.length == 0 ) {
                
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
                    data: { 'modelNew': $scope.ModelNew,  'file': $scope.fileData , 'plantId':$scope.BudgetPlantId }
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
            data: { 'data': $scope.ExcelUploadData, 'plantId': $scope.BudgetPlantId}
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