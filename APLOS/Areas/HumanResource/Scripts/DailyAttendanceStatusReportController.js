DailyAttendanceStatusReportController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', 'toaster', 'cboService', '$controller', '$window'];
function DailyAttendanceStatusReportController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, toaster, cboService, $controller, $window) {
    $rootScope.title = 'Daily Attendance Status';
    $scope.path = 'humanresource/DailyAttendanceStatusReport/';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.Action = 'Save';
    $scope.InStatusList = [];
    $scope.LateStatusList = [];
    $scope.EmpCategoryList = [];
    $scope.ResponsiblePersonList = [];
    $scope.TeamLeaderList = [];
    $scope.FavList = [];
    $scope.EmpStatusList = [];
    $scope.ShiftList = [];

    $scope.EmpStatusList = [
        {
            Text: "LONG ABSENTEEISM",
            Value:"LONG ABSENTEEISM"
        },
        {
            Text: "TBS",
            Value:"TBS"
        }
    ]

    $scope.InStatusList = [
        {
            Text: "EI",
            Value: "EI"
        },
        {
            Text: "IM",
            Value: "IM"
        },
        {
            Text: "IN",
            Value: "IN"
        },
        {
            Text: "LI",
            Value: "LI"
        },
        {
            Text: "O",
            Value: "O"
        },
    ]

    $scope.DayStatusList = [
        {
            Text: "A",
            Value:"A"
        },
        {
            Text: "AH",
            Value: "AH"
        },
        {
            Text: "AW",
            Value: "AW"
        },
        {
            Text: "CH",
            Value: "CH"
        },
        {
            Text: "CL",
            Value: "CL"
        },
        {
            Text: "CW",
            Value: "CW"
        },
        {
            Text: "EM",
            Value: "EM"
        },
        {
            Text: "H",
            Value: "H"
        },
        {
            Text: "HDCL",
            Value: "HDCL"
        },
        {
            Text: "HDP",
            Value: "HDP"
        },
        {
            Text: "HDPL",
            Value: "HDPL"
        },
        {
            Text: "HDSL",
            Value: "HDSL"
        },
        {
            Text: "HL",
            Value: "HL"
        },
        {
            Text: "HP",
            Value: "HP"
        },
        {
            Text: "L",
            Value: "L"
        },
        {
            Text: "LWP",
            Value: "LWP"
        },
        {
            Text: "ML",
            Value: "ML"
        },
        {
            Text: "OD",
            Value: "OD"
        },
        {
            Text: "P",
            Value: "P"
        },
        {
            Text: "PL",
            Value: "PL"
        },
        {
            Text: "PW",
            Value: "PW"
        },
        {
            Text: "SL",
            Value: "SL"
        },
        {
            Text: "W",
            Value: "W"
        },
        {
            Text: "WAH",
            Value: "WAH"
        },
        {
            Text: "WAW",
            Value: "WAW"
        },
        {
            Text: "WP",
            Value: "WP"
        },
    ]

    $scope.ModelTemp = {
        Id: null,
        FavoriteName:null,
        InStatus:null,
        FromDate:null,
        ToDate: null,
        EmployeecategoryId: null,
        TeamLeaderId:null,
        EmpSystemId:null,
        ShiftDefinationId:null,
        EmployeeStatus: null,
        FavoriteFilteruserId: null,
        DayStatus: null,
        ResponsiblePersonId: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data); 
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
           /* $scope.GetEmployeeCategory();*/
            //$rootScope.toggle(); 
            $scope.GetDailyAttendanceStatus();
        }
    };

    // #region    EmployeePop
    $scope.OpeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('show');
        $scope.GetResponsiblePerson();
    }
    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#EmployeePop')).modal('hide');

    }
    $scope.EmployeeList = [];
    $scope.GetResponsiblePerson = function () {
        $http({
            method: 'GET',
            url: 'Productions/Parameter/GetResponsiblePerson',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmployeeList = resp.data;
        });

    }

    

    //$scope.EmpSystemId = null;
    //$scope.EmployeeName = null;
    
    $scope.doubleEmploye = function (e) {
        $scope.ModelNew.EmpSystemId = e.data.EmpSystemId;
        $scope.ModelNew.EmployeeName = e.data.EmployeeName;
       // $scope.TeamLeader = e.data.EmployeeName;
        angular.element(document.querySelector('#EmployeePop')).modal('hide');
        /*$scope.viewFurniturePolicyGrids();*/
    }



    $scope.getResponsiblePersonId = function () {
        $http({
            method: 'POST',
            data: { 'ResponsiblePersonId': $scope.EmployeeId, },
            url: $scope.path + 'getResponsiblePersonId',
        }).then(function success(response) {
            $scope.ResponsiblePerson = JSON.stringify(response.data[0].EmployeeName.replace(/\"/g, ""));
            $scope.ResponsiblePerson = $scope.ResponsiblePerson.replace(/\"/g, "");

        });
    }
        // #endregion    EmployeePop

    $scope.GetShift = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetShift',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ShiftList = resp.data;
        });
    }
    $scope.GetShift();

    $scope.GetEmployeeCategory = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEmployeeCategory',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EmpCategoryList = resp.data;
        });
    }
    $scope.GetEmployeeCategory();

    $scope.FavoriteList = [];
    $scope.GetFavoriteListByUser = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetFavoriteListByUser',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FavoriteList = resp.data;
        });
    }
    $scope.GetFavoriteListByUser();

    $scope.FavoriteFilterList = [];
    $scope.GetFavoriteFilterByUser = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetFavoriteFilterByUser',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.FavoriteFilterList = resp.data;
        });
    }
    $scope.GetFavoriteFilterByUser();

    $scope.GetFavouriteFilter = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetFavouriteFilter?filterId=' + $scope.ModelNew.Id ,
            
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ModelNew = Object.assign({}, resp.data[0]);
            
        });
    }
    //$scope.GetFavouriteFilter();

    $scope.DailyAttendanceStatusList = [];
    $scope.GetDailyAttendanceStatus = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetDailyAttendanceStatus?instatus=' + $scope.ModelNew.InStatus + '&fromdate=' + $scope.ModelNew.FromDate + '&todate=' + $scope.ModelNew.ToDate + '&employeecategory=' + $scope.ModelNew.EmployeecategoryId + '&teamleaderid=' + $scope.ModelNew.TeamLeaderId + '&responsibleperson=' + $scope.ModelNew.ResponsiblePersonId + '&shift=' + $scope.ModelNew.ShiftDefinationId + '&employeestatus=' + $scope.ModelNew.EmployeeStatus + '&daystatus=' + $scope.ModelNew.DayStatus,           
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.DailyAttendanceStatusList = resp.data;
        });
    }
    

    $scope.OpenTeamLeaderPopUp = function () {
        angular.element(document.querySelector('#TeamPopupId')).modal('show');
        $scope.GetTeamLeader();
    }
    $scope.closeTeamLeaderPopUp = function () {
        angular.element(document.querySelector('#TeamPopupId')).modal('hide');

    }

    $scope.GetTeamLeader = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetTeamLeader',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.TeamLeaderList = resp.data;
        });
    }

    $scope.doubleTeamLeader = function (e) {
        $scope.ModelNew.TeamLeaderId = e.data.EmpSystemId;
        
        $scope.ModelNew.TeamLeader = e.data.EmployeeName;
        angular.element(document.querySelector('#TeamPopupId')).modal('hide');
        
    }

    // #region Save
    $scope.Save = function () {
        debugger;
        $scope.$broadcast('show-errors-check-validity');
       /* if ($scope.ModelNewForm.$valid) {*/
            $http({
                method: 'POST',
                url: $scope.path + $scope.Action,
                data: {
                    'datas': $scope.ModelNew,
                    'employeeId': $window.employeeId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.closeFavoritePopUp();
                    $scope.GetFavoriteListByUser();     
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
       /* }*/

    };
    // #endregion Save

    $scope.exportgriddataUrl = 'GridReports/ExcelExportUpd';
    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';
    $scope.summaryfileName = "Daily Attendance Status.xlsx";

    $scope.XlsDailyAttendanceReport = function () {
        var dataList = [];
        var g = $("#Grid").data("ejGrid");
        dataList = g.getFilteredRecords();
        if (dataList.length == 0)
        {
            
             dataList = $scope.DailyAttendanceStatusList;
        }
        $scope.fileName = 'Daily Attendance Status.xlsx';
            $http({
                method: "POST",
                
               // url: $scope.path + 'GetDailyAttendanceStatusXls',
                url: $scope.exportgriddataUrl,
                data: {
                    //'instatus': $scope.ModelNew.InStatus,
                    //'fromdate': $scope.ModelNew.FromDate,
                    //'todate': $scope.ModelNew.ToDate,
                    //'employeecategory': $scope.ModelNew.EmployeecategoryId,
                    //'teamleaderid': $scope.ModelNew.TeamLeaderId,
                    //'responsibleperson': $scope.ModelNew.EmpSystemId,
                    //'shift': $scope.ModelNew.ShiftDefinationId,
                    //'employeestatus': $scope.ModelNew.EmployeeStatus,
                    //'daystatus': $scope.ModelNew.DayStatus,
                    'data': dataList,
                    'reportFileName': $scope.fileName,
                },
                dataType: 'JSON'
            })
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        //$rootScope.report($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.summaryfileName);
                        
                        //$rootScope.report($scope.downloadgriddataUrlPath + "?fileName=" + $scope.summaryfileName);
                        $window.open($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    }
                }, function errorCallback(response) {
                    ShowResult(response.data.Message, 'failure');
                });

    };

    $scope.XlsDailyAttendanceSummaryReport = function () {
        
        $scope.fileName = 'Daily Attendance Status Summary.xlsx';
        $http({
            method: "POST",

            url: $scope.path + 'GetDailyAttendanceStatusXls',
            
            data: {
                'instatus': $scope.ModelNew.InStatus,
                'fromdate': $scope.ModelNew.FromDate,
                'todate': $scope.ModelNew.ToDate,
                'employeecategory': $scope.ModelNew.EmployeecategoryId,
                'teamleaderid': $scope.ModelNew.TeamLeaderId,
                'responsibleperson': $scope.ModelNew.ResponsiblePersonId,
                'shift': $scope.ModelNew.ShiftDefinationId,
                'employeestatus': $scope.ModelNew.EmployeeStatus,
                'daystatus': $scope.ModelNew.DayStatus,
                
            },
            dataType: 'JSON'
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);

                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
   
    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';
        $scope.DailyAttendanceStatusList = [];
        $scope.ModelTemp = {
            Id: null,
            FavoriteName: null,
            InStatus: null,
            FromDate: null,
            ToDate: null,
            EmployeecategoryId: null,
            TeamLeaderId: null,
            EmpSystemId: null,
            ShiftDefinationId: null,
            EmployeeStatus: null,
            FavoriteFilteruserId: null,
            DayStatus: null,
            ResponsiblePersonId: null
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

        
    }
    $scope.ClearEmployeePopup = function () {
        ClearEmployeePopFields();
        return true;
    };
    function ClearEmployeePopFields() {



        $scope.ModelNew.EmpSystemId = null;

        $scope.ModelNew.EmployeeName = null;


    }
    $scope.ClearTeamLeaderPopUp = function () {
        ClearTeamLeaderPopUpFields();
        return true;
    }
    function ClearTeamLeaderPopUpFields() {



        $scope.ModelNew.TeamLeader = null;

        $scope.ModelNew.TeamLeaderId = null;


    }

    $scope.OpenFavoritePopUp = function () {
        angular.element(document.querySelector('#FavoritePopupId')).modal('show');
        
    }
    $scope.closeFavoritePopUp = function () {
        angular.element(document.querySelector('#FavoritePopupId')).modal('hide');

    }

    $scope.RemoveFavoriteFilter = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveFavoriteFilter?id=' + $scope.ModelNew.Id,
            
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
}