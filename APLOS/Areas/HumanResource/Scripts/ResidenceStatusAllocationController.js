'use strict';
ResidenceStatusAllocationController.$inject = ['cboService', '$window','commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceStatusAllocationController(cboService, $window,commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Status/Allocation/Unallocation';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceStatusAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 = 1;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };


    //#region The Filters 

    $scope.filters = [];
    $scope.getResidenceStatusFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getResidenceFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filters = response.data;
            var columnList = [
                { field: 'ResidenceGroup', width: 20, headerText: "Residence Group", type: "string" },
                { field: 'Plant', width: 20, headerText: "Plant", type: "string" },
                { field: 'Location', width: 20, headerText: "Location", type: "string" },
                { field: 'EmployeeType', width: 20, headerText: "Employee Type", type: "string" },
                { field: 'ServiceType', width: 20, headerText: "EmpService Type", type: "string" },
                { field: 'Rooms', width: 20, headerText: "Rooms", type: "string" },
                { field: 'Block', width: 20, headerText: "Block If Applicable", type: "string" },
                { field: 'ResidenceSubCategory', width: 20, headerText: "Residence SubCategory", type: "string" },
                { field: 'Floor', width: 20, headerText: "Floor", type: "string" },
                { field: 'ResidentType', width: 20, headerText: "Resident Type", type: "string" },
                { field: 'ResidenceNumber', width: 20, headerText: "Residence Number", type: "string" },
                { field: 'VacancyStatus', width: 20, headerText: "Vacancy Status", type: "string" },
                { field: 'AssetName', width: 20, headerText: "Asset Name", type: "string" },
                //{ field: 'Vacancy', width: 20, headerText: "Vacancy", type: "string" },

            ];
            $("#filters").ejGrid({
                dataSource: $scope.filters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#filters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#filters").children('.e-pager.e-js.e-pager').hide();
            $("#filters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#filters").children('.e-gridcontent').hide();
        });
    }
    //$scope.getResidenceStatusFilters();

    //$scope.parameters = [];
    //$scope.filterComplete = function () {

    //    var g = $("#filters").data("ejGrid");
    //    var fl = g.getFilteredRecords();
    //    if (fl.length == 0) {
    //        fl = $scope.filters;
    //    }


    //    var parameters = [];
    //    parameters.push({ "Key": "ResidenceMasterId", "Value": getString(fl, "ResidenceMasterId") });
    //    parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });
    //    parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
    //    parameters.push({ "Key": "EmployeeTypeId", "Value": getString(fl, "EmployeeTypeId") });
    //    //parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });
       
    //    $scope.parameters = parameters;
    //}

    var getString = function (data, column) {
        var string = "''";
        var collection = [];

        for (var i = 0; i < data.length; i++) {
            if (collection.includes(data[i][column]) == false) {
                string += ",'" + data[i][column] + "'";
                collection.push(data[i][column]);
            }
        }
        return string;
    }


    //#endregion The Filters


    // #statrt Region Add Filter By Nitesh
    $scope.filtersN = [];
    $scope.getResidenceStatusFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getResidenceFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.filtersN = response.data;
            var columnList = [
                { field: 'ResidenceGroup', width: 20, headerText: "Employee Category", type: "string" },
                { field: 'EmployeeType', width: 20, headerText: "Employee Type", type: "string" },
                { field: 'Plant', width: 20, headerText: "Location", type: "string" },
                { field: 'ResidentType', width: 20, headerText: "Resident Type", type: "string" },
                { field: 'Block', width: 20, headerText: "Block", type: "string" },
                { field: 'Floor', width: 20, headerText: "Floor", type: "string" },
                { field: 'ResidenceNumber', width: 20, headerText: "Residence Number", type: "string" },
                { field: 'ServiceType', width: 20, headerText: "EmpService Type", type: "string" },
                { field: 'Rooms', width: 20, headerText: "Rooms", type: "number" },
                { field: 'Vacancy', width: 20, headerText: "Vacancy", type: "number" },
                { field: 'Occupied', width: 20, headerText: "Occupied", type: "number" },
                { field: 'Available', width: 20, headerText: "Available", type: "number" },

                //{ field: 'Block', width: 20, headerText: "Block If Applicable", type: "string" },               
                //{ field: 'ResidenceSubCategory', width: 20, headerText: "Residence SubCategory", type: "string" },               
                //{ field: 'VacancyStatus', width: 20, headerText: "Vacancy Status", type: "string" },
                //{ field: 'AssetName', width: 20, headerText: "Asset Name", type: "string" },
                //{ field: 'Vacancy', width: 20, headerText: "Vacancy", type: "string" },

            ];
            $("#GridEdit").ejGrid({
                dataSource: $scope.filtersN,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#GridEdit").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#GridEdit").children('.e-pager.e-js.e-pager').hide();
            $("#GridEdit").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#GridEdit").children('.e-gridcontent').hide();
        });
    }
    //$scope.getResidenceStatusFilters();

    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#GridEdit").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.filters;
        }


        var parameters = [];
        parameters.push({ "Key": "ResidenceMasterId", "Value": getString(fl, "ResidenceMasterId") });
        parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });
        parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        parameters.push({ "Key": "EmployeeTypeId", "Value": getString(fl, "EmployeeTypeId") });
        //parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });

        $scope.parameters = parameters;
    }
    // #end Region Add Filter By Nitesh





    $scope.view = function () {
        $scope.filterComplete();
        $http({
            method: "POST",
            url: $scope.path + 'GetViewData',
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        })
    }


    //$scope.AvailablePopUpData = function (data) {
    //    location.href = $scope.path + "GetEmployeeDeleteInfo?grnId=" + data.Id;
    //};
    $scope.PlantId = null;
    $scope.dataList = [];
    $scope.availableNumber = null;
    $scope.AvailablePopUpData = function (data) {
        $scope.ResidenceId = data.data.ResidenceMasterId;
        $scope.PlantId = data.data.PlantId;
        $scope.availableNumber = data.data.Available;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getemployeeDataList?plantId=' + data.data.PlantId + '&residenceGroupId=' + $scope.ResidedenceGroupId + '&EmployeeTypeId=' + data.data.EmployeeTypeId
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
            
            //$scope.getResidence();
            
            $scope.UnallocationView();

        });
        var gridObj = $("#GridEmp").data("ejGrid");
        gridObj.clearFiltering();  // clears all the filtering
       // angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
        $scope.openPopup('dialogemployeeNewPopUp');
    }

    $scope.closePopup = function (popupName) {
        angular.element(document.querySelector("#" + popupName + "")).modal("hide");
        try {
            $("#" + popupName).data("ejDialog").close();
        } catch (e) {

        }
    }
    $scope.openPopup = function (popupName) {

        try {
            $("#" + popupName).data("ejDialog").open();
        } catch (e) {

        }
    }

    $scope.closeEmployeePopUps = function () {
        $scope.closePopup('dialogemployeeNewPopUp');
       // angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }



    $scope.closeEmployeePopUp = function () {
        MakeData();
        $scope.SaveAllocation();
        
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllPartyWise });
    };

    function CheckBoxSelectAllPartyWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEmp").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.dataList.length; i++) {
                $scope.dataList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEmp").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.saveList = [];
    function MakeData() {
        for (var i = 0; i < $scope.dataList.length; i++) {
            if ($scope.dataList[i].isSelected == true) {
                if (checkExists($scope.saveList, $scope.dataList[i].EmployeeCode) === false) {
                    var ob = {};
                    ob.Id = null;
                    ob.EmployeeCode = $scope.dataList[i].EmployeeCode;
                    ob.EmployeeName = $scope.dataList[i].EmployeeName;
                    ob.EmployeeSystemId = $scope.dataList[i].SystemID;
                    ob.ResidenceId = $scope.ResidenceId;
                    ob.isOccupied = true;
                    ob.Date = Date.now();
                    $scope.saveList.push(ob);
                }
                else {
                    ShowResult ("This Employee " + $scope.dataList[i].EmployeeCode + " is already taken.",'failure');
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].EmployeeCode === id) {
                return true;
            }
        }
        return false;
    }

    $scope.SaveAllocation = function () {
        try {
            if (baseService.arrayLength($scope.saveList)==0)
            {
                throw "Select Employee";
            }
            $http({
                method: 'POST',
                url: $scope.path + 'residenceStatusSave',
                data: { 'EmployeeList': $scope.saveList},
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UnallocationView();
                    $scope.view();
                    $scope.saveList = [];
                    angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.ModelUnallocationList = [];
    $scope.UnallocationView = function () {
        if (baseService.isUndefinedOrNull($scope.PlantId))
        {
            $scope.PlantId = $window.plantId;
        }
        $http({
            method: "Get",
            url: $scope.path + 'viewUnallocation?PlantId=' + $scope.PlantId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelUnallocationList = response.data;
            //$scope.SaveAllocation();
        })
    }
    $scope.UnallocationView();




    
    $scope.popupEmployeeList = [];
    $scope.PopupEmployeeView = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'PopupEmployeeView',
            data: {
                'EmployeeCategorySystemID': $scope.selectedData.EmployeeCategoryId,
                'fromDate': $scope.selectedData.fromDate,
                'toDate': $scope.selectedData.toDate,
            }

        }).then(function successCallback(response) {
            $scope.popupEmployeeList = response.data;
            document.getElementById("EmpGrid").style.display = "block";
        })
    }

    $scope.selResidenceMasterId = null;
    $scope.selResidenceMaster = function (e) {
        $scope.selResidenceMasterId = e.data.Id;
        $scope.openChildGrid();
        $scope.getResidenceStatusLocation();
    }

    $scope.openChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('show');
    }
    $scope.closeChildGrid = function () {
        angular.element(document.querySelector('#EmpPop')).modal('hide');
    }



    $scope.Clear = function () {
        ClearFields();
        return true;
    };

    function ClearFields() {
        $scope.Action = "Save";
        $scope.PlantList = [];
        $scope.LocationList = [];
        $scope.ResidenceGroupIdList = [];
        $scope.ResidenceCategoryList = [];
        $scope.ResidenceSubCategoryList = [];
        $scope.BlockList = [];
        $scope.AssetNameList = [];
        $scope.ResidentTypeList = [];
        $scope.FloreList = [];
        $scope.ResidenceNumberList = [];
        $scope.EmployeeTypeIdList = [];
        $scope.RoomList = [];
        $scope.selectedData = {
            Id: null,
            PlantId: null,
            ResidedenceGroupId: null,
            EmployeeCategoryId: null,
            Location: null,
            AssetName: null,
            ResidenceSubCategory: null,
            ResidenceCategory: null,
            Rooms: null,
            Block: null,
            ResidenceType: null,
            Floor: null,
            ResidenceType: null,
            ResidenceNumber: null,
            VacancyStatus: null,
            isActive: 0,
        };
        $scope.ModelList = [];
    }

    $scope.EmployeeList = [];
    $scope.getEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getEmployee',
            data: {
                'PlantId': $scope.selectedData.PlantId,
                'ResidenceGroupId': $scope.selectedData.ResidenceGroupId,
                'EmployeeCategoryId': $scope.selectedData.EmployeeCategoryId,
            },
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        });
    }

    $scope.selectEmpDetail = function () {
        $scope.EmployeeIds = [];
        $scope.SelEmpList = [];
        for (var i = 0; i < $scope.EmployeeList.length; i++) {
            
            if ($scope.EmployeeList[i].isSelected == true) {
                $scope.SelEmpList.push($scope.EmployeeList[i]);
            }
        }

        if ($scope.SelEmpList.length > $scope.selectedData.VacancyList) {
            ShowResult('Selected Greater than vacancy allowed', 'failure');
            throw ('Invalid Request');
        }
        else {
            angular.element(document.querySelector('#EmpPop')).modal('hide');
        }
       
        $scope.getSelected();
    }

    $scope.EmpList = [];
    $scope.getSelected = function () {
        $scope.EmpList = $scope.SelEmpList;
         
    }


    // TAB - 2
    // ALL POP UPs

    // POP OPEN
    $scope.selectEmployee = function () {

        angular.element(document.querySelector('#EmployeePop')).modal('show');
    }

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    // POP CLOSED
    $scope.closeEmpPopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
    }
    // Select Emp
    $scope.EmployeeSelectedName = null;
    $scope.SelectedEmployeeId = null;
    $scope.selEmp = function (e) {
        $scope.SelectedEmployeeId = e.data.SystemId;
        $scope.EmployeeId = e.data.EmployeeId;
        $scope.SelEmployeeInfoList = e.data;
        $scope.Employee = e.data.EmployeeName;
        
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        
       
    }

    $scope.EmployeeList = [];
    $scope.getAllEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getAllEmployee',
            data: { 'EmpCategoryId': $scope.EmpCategoryId},
        }).then(function success(resp) {
            $scope.EmployeeList = resp.data;
        })
    }
    //$scope.getAllEmployee();

    $scope.openEmpCategoryPopup = function () {

        angular.element(document.querySelector('#EmpCategoryPop')).modal('show');
    }

    $scope.EmployeeCategoryList = [];
    $scope.getEmployeeCategory = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getEmployeeCategory",
            //data: { 'EmpId': $scope.SelectedEmployeeId},
            dataType: 'JSON',
        }).then(function successcallback(response) {
            $scope.EmployeeCategoryList = response.data;
            
        })
    }
    $scope.getEmployeeCategory();

    $scope.EmpCategoryId = null;
    $scope.EmpCategoryName = null;
    $scope.selEmployeeCategory = function (e) {
        $scope.EmpCategoryId = e.data.Id;
        $scope.EmpCategoryName = e.data.UserName;
        angular.element(document.querySelector('#EmpCategoryPop')).modal('hide');
      //  $scope.getAllEmployee();
    }


    $scope.ResidenceMasterList = [];
    $scope.getResidenceMaster = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getResidenceMaster',

        }).then(function success(resp) {
            $scope.ResidenceMasterList = resp.data;
        })
    }

    

    // Data Saved
    $scope.selectedDataR = {
        Id: null,
        isOccupied:false,
    };
    $scope.ResidenceData = Object.assign({}, $scope.selectedDataR);


    //$scope.ResidenceStatusSave = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'residenceStatusSave',
    //        data: {
    //            'EmployeeList': $scope.EmployeeList,
    //            'ResidenceMasterId': $scope.ResidenceGroupIdList[0].Id,
    //        },
    //        dataType: 'JSON',
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //        }
    //        $scope.Clear();
    //    });
    //}


    $scope.ResidenceStatusLocationList = [];
    $scope.getResidenceStatusLocation = function () {
        $http({
            method: "POST",
            url: $scope.path + "getResidenceStatusLocation",
            data: {                
                'EmployeeId': $scope.SelectedEmployeeId,
                'ResidenceMasterId': $scope.selResidenceMasterId,
            },
        }).then(function seccessCallback(response) {
            $scope.ResidenceStatusLocationList = response.data
        })
            
    }

    $scope.refreshTemplateemployee = function (args) {
        $("#headcheck").ejCheckBox({ "change": CheckBoxSelectAllPartyWises });
    };

    function CheckBoxSelectAllPartyWises(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEUnallocation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ModelUnallocationList.length; i++) {
                $scope.ModelUnallocationList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEUnallocation").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveRSU = function () {
        $scope.unallocationLoop = [];
        for (var i = 0; i < $scope.ModelUnallocationList.length; i++)
        {

            if ($scope.ModelUnallocationList[i].isSelected)
            {
                $scope.unallocationLoop.push($scope.ModelUnallocationList[i]);
            }
        }
        $http({
            method: 'POST',
            url: $scope.path + 'SaveRSUnallocation',
            data: { 'employeeList': $scope.unallocationLoop },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.UnallocationView();
                $scope.view();
            }
        });
    }

    //-----------------------------------------------------------------------------------

    function openModal() {
        $('.confirm-delete').addClass('hide');
        $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
        $('#myModal').modal('show');
    }
//-----------------------------------------------------------------------------------

    // REPORT DOWNLOAD
    $scope.ResidenceAllocationReport = function () {
        $scope.filterComplete();
        $scope.fileName = 'To Unassign List';
        var dataList = [];
        var g = $("#GridEdit").data("ejGrid");
        dataList = g.getFilteredRecords();

        if (dataList.length == 0) {
            dataList = $scope.ModelList;
        }

        $http({
            method: 'POST',
            url: $scope.exportgriddataUrl,
            //url: $scope.path + "XlsResidenceAllocationReport",
            //data: { 'parameters': $scope.parameters },
            data: {
                'reportFileName': $scope.fileName,
                'data': dataList
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.ResidenceMasterReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsResidenceMaterReport",
            data: { 'empCurrentStatus': $scope.EmployeeNew.EmployeeCurrentStatus },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                if (baseService.isUndefinedOrNull($scope.EmployeeNew.EmployeeCurrentStatus)) {
                    ShowResult('Employee Current Statusus Required.', 'failure');
                    throw "Invalid Request";
                }
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.allResidenceMasterReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsAllResidenceMaterReport",
            data: { 'empCurrentStatus': $scope.EmployeeNew.EmployeeCurrentStatus },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });

    };

    $scope.ResidenceMasterList = [];
    $scope.gridViewResidenceMAster = function () {
        $http({
            method: 'POST',
            url: $scope.path + "gridViewResidenceMAster",

            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ResidenceMasterList = response.data;
        })
    };

    $scope.EmployeeNew = {
        EmployeeCurrentStatus: null
    };

    $scope.EmployeeStatusList = [];
    $scope.employeeCurrentStatus = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'employeeCurrrentStatus',
            dataType: 'JSON',
        }).then(function successCallback(response) {
           
            $scope.EmployeeStatusList = response.data;
        })
    };
    $scope.employeeCurrentStatus();

    $scope.ResidedenceGroupId = null;
    $scope.ResidenceGroupList = [];
    $scope.getResidence = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getemployeeDataList?plantId=' + $scope.PlantId + '&residenceGroupId=' + $scope.ResidedenceGroupId
        }).then(function successCallback(response) {
            $scope.dataList = response.data;


        });
    }


    $scope.ResidenceGroupList = [];
    $scope.ResidenceGroupCbo = function () {
        $http.get('employees/ResidenceGroup/GetCbo')
            .then(function (response) {
                $scope.ResidenceGroupList = response.data;

                $scope.ResidedenceGroupId = $scope.ResidenceGroupList[0].Value;
               

            });
    }
   
    $scope.ResidenceGroupCbo();

    /*
     *      Pop up screen for occupied employee
     */

    $scope.refreshTemplateOccupiedEmployee = function (args) {
        $("#Occupiedheadcheck").ejCheckBox({ "change": CheckBoxSelectAllOccupiedWises });
    };

    function CheckBoxSelectAllOccupiedWises(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridEOccupiedUnallocation").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.OccupiedEmployeeList.length; i++) {
                $scope.OccupiedEmployeeList[i].isSelected = ChkOrUnchk;
            }
        }
        else {

            for (var j = 0; j < filtered.length; j++) {
                filtered[j].isSelected = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridEOccupiedUnallocation").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.SaveOccupiedRSU = function () {
        try {
            $scope.unallocationLoop = [];
            for (var i = 0; i < $scope.OccupiedEmployeeList.length; i++) {
                if ($scope.OccupiedEmployeeList[i].isSelected) {
                    $scope.unallocationLoop.push($scope.OccupiedEmployeeList[i]);
                }
            }
            if (baseService.arrayLength($scope.unallocationLoop) == 0) {
                throw "Select Employee.";
            }

            $http({
                method: 'POST',
                url: $scope.path + 'SaveRSUnallocation',
                data: { 'employeeList': $scope.unallocationLoop },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeOccupiedEmployeePopUps();
                    /// $scope.OccupiedAvailablePopUpData();
                    $scope.view();
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.ResidenceNumberN = null
    $scope.getResidenceNumber = function (e) {
        $scope.ResidenceNumberN = e.data.ResidenceNumber;
    }

    $scope.OccupiedEmployeeList = [];
    $scope.OccupiedAvailablePopUpData = function (data) {
        $scope.ResidenceId = data.data.ResidenceMasterId;
        $scope.PlantId = data.data.PlantId;
        $scope.availableNumber = data.data.Available;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getOccupiedemployeeDataList?plantId=' + data.data.PlantId + '&residenceGroupId=' + $scope.ResidedenceGroupId + '&residenceNumber=' + $scope.ResidenceNumberN
        }).then(function successCallback(response) {
            $scope.OccupiedEmployeeList = response.data;

            var gridObj = $("#GridEOccupiedUnallocation").data("ejGrid");
            gridObj.clearFiltering();  // clears all the filtering
        });
        angular.element(document.querySelector('#OccupiedemployeeNewPopUp')).modal('show');
    }

    $scope.closeOccupiedEmployeePopUps = function () {
        angular.element(document.querySelector('#OccupiedemployeeNewPopUp')).modal('hide');
    }

    function hideTopGrid() {
        document.getElementById("filters").style.display = "none";
    }
    //hideTopGrid();

   
}