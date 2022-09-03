'use strict';
ResidenceStatusAllocationReportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ResidenceStatusAllocationReportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Residence Status Report';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/ResidenceStatusAllocation/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';

    // Tab Change
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.tab2 ;
    $scope.setTab2 = function (newTab) {
        $scope.tab2 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab2 === tabNum;
    };

    $scope.tab3;
    $scope.setTab3 = function (newTab) {
        $scope.tab3 = newTab;
    };
    $scope.isSet2 = function (tabNum) {
        return $scope.tab3 === tabNum;
    };


    //#region The Filters 

    $scope.Reportfilters = [];
    $scope.getResidenceStatusReportFilters = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'getResidenceReportFilters',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.Reportfilters = response.data;
            var columnList = [
                { field: 'EmployeeId', width: 20, headerText: "Employee Id", type: "string" },
                { field: 'Designation', width: 20, headerText: "EmployeeGiven/LegalDesignation", type: "string" },
                { field: 'EmployeeName', width: 20, headerText: "Name", type: "string" },
                { field: 'Section', width: 20, headerText: "Section", type: "string" },
                { field: 'SubSection', width: 20, headerText: "Sub Section", type: "string" },
                { field: 'Department', width: 20, headerText: "Department", type: "string" },
                { field: 'Entity', width: 20, headerText: "Entity", type: "string" },
                { field: 'ResidenceGroup', width: 20, headerText: "Residence Group", type: "string" },
                { field: 'ResidenceId', width: 20, headerText: "Residence Id", type: "string" },
                { field: 'ResidenceNumber', width: 20, headerText: "Residence Number", type: "string" },
                { field: 'Block', width: 20, headerText: "Block", type: "string" },
                { field: 'ResidentType', width: 20, headerText: "Resident Type", type: "string" },
                //{ field: 'ResidenceCategory', width: 20, headerText: "Residence Category", type: "string" },
                { field: 'ResidenceSubCategory', width: 20, headerText: "Sub Category", type: "string" }

            ];
            $("#Reportfilters").ejGrid({
                dataSource: $scope.Reportfilters,
                minWidth: 450, minHeight: 400,
                allowFiltering: true, allowPaging: true, enableTouch: true, responsive: true, allowTextWrap: true, allowScrolling: true,
                filterSettings: { filterType: "excel" },
                columns: columnList
            });

            var gridObj = $("#Reportfilters").data("ejGrid");
            gridObj.refreshContent(true);
            gridObj.refreshTemplate();
            $("#Reportfilters").children('.e-pager.e-js.e-pager').hide();
            $("#Reportfilters").children('.e-gridcontent.e-droppable.e-js').hide();
            $("#Reportfilters").children('.e-gridcontent').hide();
        });
    }
    $scope.getResidenceStatusReportFilters();



    $scope.parameters = [];
    $scope.filterComplete = function () {

        var g = $("#Reportfilters").data("ejGrid");
        var fl = g.getFilteredRecords();
        if (fl.length == 0) {
            fl = $scope.Reportfilters;
        }


        var parameters = [];
        parameters.push({ "Key": "EmployeeId", "Value": getString(fl, "EmployeeId") });
        //parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });
        //parameters.push({ "Key": "PlantId", "Value": getString(fl, "PlantId") });
        //parameters.push({ "Key": "EmployeeTypeId", "Value": getString(fl, "EmployeeTypeId") });
        //parameters.push({ "Key": "ResidenceGroupId", "Value": getString(fl, "ResidenceGroupId") });
       
        $scope.parameters = parameters;
    }

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

    $scope.ViewRSAFilters = function () {
        $scope.filterComplete();
        $http({
            method: "POST",
            url: $scope.path + 'GetRSAFiltersViewData',
            data: { 'parameters': $scope.parameters },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;
        })
    }

    $scope.DownloadResidenceStatusAllocationReport = function () {

        var dataList = [];
        var g = $("#GridRSA").data("ejGrid");
        dataList = g.getFilteredRecords();
        var ids = "";
        if (baseService.arrayLength(dataList) > 0) {
            for (var i = 0; i < dataList.length; i++) {
                if (ids == "") {
                    ids = "'','" + dataList[i].EmployeeId + "'";
                }
                else {
                    ids += ",'" + dataList[i].EmployeeId + "'";
                }
            }
        }
        else {
            for (var i = 0; i < $scope.ModelList.length; i++) {
                if (ids == "") {
                    ids = "'','" + $scope.ModelList[i].EmployeeId + "'";
                }
                else {
                    ids += ",'" + $scope.ModelList[i].EmployeeId + "'";
                }
            }
        }
        $scope.fileName = 'Residence Status Allocation.xlsx';
        $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';

        $http({
            method: 'POST',
            url: $scope.path + "ResidenceStatusAllocationReport",
            data: { 'employeeId': ids },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                //$window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    }

    //$scope.AvailablePopUpData = function (data) {
    //    location.href = $scope.path + "GetEmployeeDeleteInfo?grnId=" + data.Id;
    //};
    $scope.PlantId = null;
    $scope.dataList = [];
    $scope.AvailablePopUpData = function (data) {
        $scope.ResidenceId = data.data.ResidenceMasterId;
        $scope.PlantId = data.data.PlantId;
        $scope.dataList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'getemployeeDataList?plantId=' + data.data.PlantId
        }).then(function successCallback(response) {
            $scope.dataList = response.data;
            $scope.UnallocationView();
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.closeEmployeePopUps = function () {
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        MakeData();
        $scope.SaveAllocation();
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
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
                    throw "This Employee " + $scope.dataList[i].EmployeeCode + " is already taken.";
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
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //$scope.ModelUnallocationList = [];
    //$scope.UnallocationView = function () {
    //    $http({
    //        method: "Get",
    //        url: $scope.path + 'viewUnallocation?PlantId=' + $scope.PlantId,
    //        dataType: 'JSON'
    //    }).then(function successCallback(response) {
    //        $scope.ModelUnallocationList = response.data;
    //    })
    //}
    //$scope.UnallocationView();




    
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

    //$scope.EmployeeCategoryList = [];
    //$scope.getEmployeeCategory = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + "getEmployeeCategory",
    //        //data: { 'EmpId': $scope.SelectedEmployeeId},
    //        dataType: 'JSON',
    //    }).then(function successcallback(response) {
    //        $scope.EmployeeCategoryList = response.data;
            
    //    })
    //}
    //$scope.getEmployeeCategory();

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

    //$scope.save = function () {
    //    $http({
    //        method: 'POST',
    //        url: $scope.path + 'save',
    //        data: {
    //            'data': $scope.ResidenceData,
    //            'EmployeeId': $scope.SelectedEmployeeId,
    //            'ResidenceMasterId': $scope.selResidenceMasterId,
    //        },
    //        dataType: 'JSON',
    //    }).then(function successCallback(response) {
    //        if (response.data.Error === true) {
    //            ShowResult(response.data.Message, 'failure');
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'success');
    //        }
    //    });
    //}

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


    //-----------------------------------------------------------------------------------

//    function openModal() {
//        $('.confirm-delete').addClass('hide');
//        $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
//        $('#myModal').modal('show');
//    }
////-----------------------------------------------------------------------------------

    //----------------------------------Written By Nitesh------------------------------------
    $scope.ModelTemp = {
        PartialVacantFullyOccupied: null

    };

    $scope.detailResidenceStatusGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "detailResidenceStatusGrid",
            data: { 'PartialVacantFullyOccupied': $scope.ModelNew.PartialVacantFullyOccupied },
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

    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    $scope.detailResidenceStatusReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsDetailResidenceStatus",
            data: { 'PartialVacantFullyOccupied': $scope.ModelNew.PartialVacantFullyOccupied },
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

    $scope.pendingForUnAllocationReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsPendingForUnallocation",
           
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

    $scope.ResidenceSummaryReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsResidenceSummary",
           
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

    $scope.PendingForAllocation = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsPendingForAllocation",

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

    $scope.allResidenceMasterReport = function () {
        $http({
            method: 'POST',
            url: $scope.path + "XlsAllResidenceMaterReport",
            
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

   

    $scope.DetailResidenceStatusList = []
    $scope.detailResidenceStatusGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "detailResidenceStatusGrid",
            data:
            {
                'PartialVacantFullyOccupied': $scope.ModelNew.PartialVacantFullyOccupied
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.DetailResidenceStatusList = response.data
        })
    };

    $scope.PendingForAllocationList = [];
    $scope.pendingForAllocationGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "pendingForAllocationGrid",
            
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.PendingForAllocationList = response.data
        })
    }

    $scope.PendingForUnallocationList = [];
    $scope.pendingForUnAllocationGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "pendingForUnAllocationGrid",
           
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.PendingForUnallocationList = response.data
        })
    }

    $scope.ResidenceSummaryList = [];
    $scope.residenceSummarGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + "residenceSummarGrid",
            
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.ResidenceSummaryList = response.data
            $scope.AvailablePopUpData(this);
        })
    }

    //----------------------------------Written By Nitesh End------------------------------------ 
}