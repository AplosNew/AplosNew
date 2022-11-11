'use strict';
DetentionLogoutController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$window"];
function DetentionLogoutController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
    $rootScope.title = "Detention Logout";
    $scope.Action = 'Save';
    $scope.path = 'Materials/DetentionLogout/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getStorage = $scope.path + 'StorageSql';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'Delete';
    //$scope.downloadgriddataUrl = 'GridReports/Download';

    $scope.downloadgriddataUrlPath = 'GridReports/DownloadUsingFullPath';//DownloadUsingPath




    $scope.DepartmentList = [];
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;

    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    // All List Declaration
    $scope.DetentionLogGridList = [];

    // Get Detention Log Grid
    $scope.getDetentionLogGrid = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDetentionLogGrid',
        }).then(function successCallback(response) {
            $scope.DetentionLogGridList = response.data;
            
        })
    }
    $scope.getDetentionLogGrid();

    $scope.DetLogResPersonList = [];
    $scope.getDetentionLogResponsiblePerson = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getDetentionLogResponsiblePerson',
            data: {
                'detentionLogId': $scope.ModalNew.Id,
            }
        }).then(function successCallback(response) {
            $scope.DetLogResPersonList = [];
            $scope.DetLogResPersonList = response.data;

        })
    }

    // #region Detention Log update
    var LogTime = new Date();
    $scope.ModalTemp = {
        Id: null,
        DetentionTypeId: null,
        DepartmentId:null,
        WorkCenterId: null,
        WorkCenter: null,
        Department: null,
        CellPhnNo: null,
        IssueByNo: null,
        Remarks: null,
        UpdateRemarks:null,
        LoginTime: LogTime,
        EmployeeName: null,
        isUpdate: false,
        isClose: false,
        LogoutTime: LogTime,
        ByWhom: null,
        ProcessId: null
    };
    $scope.ModalNew = Object.assign({}, $scope.ModalTemp);

    $scope.getByWhom = function () {
        $http({
            method: "POST",
            url: $scope.path + 'getByWhom',
        }).then(function successCallBack(response) {
            $scope.ModalNew.ByWhom = response.data[0].ByWhom;
        })
    }
    //$scope.getByWhom();

    //-------------------------------------------------------------------

    // Responsible Person
    $scope.openEmployeePopUp = function () {
       // $scope.getsR();
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('show');
    }

    $scope.ResponsibleList = [];
    $scope.userResponsiblePersonList = [];
    $scope.getsR = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/GetDetentionResponsible?detentionTypeId=' + $scope.ModalNew.DetentionTypeId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ResponsibleList = resp.data;

            
        });
    }


    $scope.closeResponsiblePopUp = function () {
        angular.element(document.querySelector('#ResponiblePersonPop')).modal('hide');
    }


    $scope.DetentionTypeList = [];
    $scope.getDetentionType = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getDetentionTypeListByDepartment'
        }).then(function successCallback(response) {
            $scope.DetentionTypeList = response.data;
            $scope.GetDepartment();
        });
    }
    $scope.getDetentionType();

    $scope.WorkCenterList = [];
    $scope.getWorkCenter = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/GetWorkCenter?processId=' + $scope.ModalNew.ProcessId,
        }).then(function successCallback(response) {
            $scope.WorkCenterList = response.data;

        })
    }
    //$scope.getWorkCenter();

    $scope.getRespPersonContactNo = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getRespPersonContactNo?ResponsiblePersonId=' + $scope.ResponsiblePersonId,
        }).then(function successCallback(response) {
            $scope.ModalNew.CellPhnNo = response.data[0].CellPhnNo;
        })
    }

    $scope.getIssueByNo = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getIssueByNo',
        }).then(function successCallback(response) {
            $scope.ModalNew.IssueByNo = response.data[0].IssueByNo;
        })
    }
    $scope.getIssueByNo();

    $scope.DepartmentList = [];
    $scope.GetDepartment = function () {
        $http.get('Materials/DetentionLog/GetDepartment?detentiontypeId=' + $scope.ModalNew.DetentionTypeId)
            .then(
                function successCallback(response) {
                    $scope.DepartmentList = response.data;
                    
                    //if (!baseService.isUndefinedOrNull($scope.ModalNew.DepartmentId)) {
                    //    for (var i = 0; i < $scope.DepartmentList.length; i++) {
                    //        if ($scope.DepartmentList[i].Value == $scope.ModalNew.DepartmentId) {
                    //            $scope.ModalNew.DepartmentId = $scope.DepartmentList[i].Value;
                    //            break;
                    //        }
                    //    }
                    //}
                }
            )
    }
    //$scope.GetDepartment();

    $scope.LogoutTime = null;
    $scope.UpdateTime = null;
    $scope.Get = function (args) {

        $scope.ModalNew = Object.assign({}, args.data);
        $scope.ModalNew.EmployeeName = args.data.EmployeeName;
        
        $scope.LogoutTime = LogTime;
        $scope.UpdateTime = LogTime;
        $scope.ModalNew.Id = args.data.Id;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $scope.getWorkCenter();
            $scope.GetDepartment();
            $scope.getsR();
            $scope.getDetentionLogResponsiblePerson();
            $scope.getByWhom();
            $rootScope.toggle();
           
        }
    };

    $scope.Save = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/Save',
            data: {
                'data': $scope.ModalNew,
                'ResponsiblePersonId': $scope.ResponsiblePersonId,
                'detentionLogId': $scope.ModalNew.Id
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //$scope.SaveResponsiblePerson();
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    // #endregion Detention Log update

    $scope.Submit = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/saveDtentionLogout',
            data: {
                'data': $scope.ModalNew,
                'ResponsiblePersonId': $scope.ResponsiblePersonId,
                'detentionLogId': $scope.ModalNew.Id,
                'logouttime': $scope.LogoutTime
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
               // $scope.SaveResponsiblePerson();
                angular.element(document.querySelector('#myModal')).modal('hide');
                ShowResult(response.data.Message, 'success');

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }

    $scope.removeDetentionLResPerson = function (name, index, listName, tempId, listId) {
        try {
            $scope.popUpIndex = index;
            $scope.listName = listName;
            $scope.tempDeptId = tempId;
            $scope.listId = listId;
            $scope.message_confirmation = "Are you sure you want to update [" + name + "]  ?";
            angular.element(document.querySelector('#confirmRemoveDetentionLRPersonPopUp')).modal('show');
        }
        catch (e) {
            ShowResult(e, 'Error');
        }
    };

    $scope.removeDetentiontRow = function (e) {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/DetentionLogRespPerDelete?Id=' + $scope.tempDeptId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.getDetentionLogResponsiblePerson();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    };

    // Check Responsible Person and in anothore grid
    //-------------------------------------------------------------------------
    $scope.chkdResponsiblePersonList = [];
    $scope.ResponsiblePersonGridAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {

        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }
        var filtered = $("#GridResponsible").data("ejGrid").getFilteredRecords();
        //if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.ResponsibleList.length; i++) {
                $scope.ResponsibleList[i].isActive = ChkOrUnchk;
                //$scope.chkdResponsiblePersonList = $scope.ResponsibleList[i].isActive;
            }
        //}
        //else {
        //    for (var j = 0; j < filtered.length; j++) {
        //        filtered[j].IsActive = ChkOrUnchk;
        //    }
        //}
        var gridObj = $("#GridResponsible").data("ejGrid"); gridObj.refreshContent();
        gridObj.refreshTemplate();
        
    };



    function checkResponsiblePersonExist(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].Id === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.SendResponsiblePerson = function () {
        if (baseService.arrayLength($scope.ResponsibleList) > 0) {
            angular.forEach($scope.ResponsibleList, function (a) {
                
                if (a.chk) {
                    var ob = {};
                    ob.Id = null;
                    ob.ResponsiblePersonId = a.ResponsiblePersonId;
                    ob.EmployeeCode = a.EmployeeCode;
                    ob.EmployeeName = a.ResponsiblePerson;
                    ob.CellPhnNo = a.CellPhnNo;
                    ob.Department = a.Department;
                    ob.Section = a.Section;
                    ob.SubSection = a.SubSection;
                    ob.LegalDesignation = a.LegalDesignation;
                    ob.isActive = a.isActive
                    ob.chk = a.chk;
                    
                    $scope.userResponsiblePersonList.push(ob);
                    ob = {};
                }
                

            });
        }

        $scope.$broadcast('show-errors-check-validity');
       
        $http({
            method: 'POST',
            url: $scope.path + 'saveDtentionLogResPerson',
            data: {
                'data': $scope.userResponsiblePersonList,
                'detentionLogId': $scope.ModalNew.Id,
                'flag': $scope.userResponsiblePersonList.chk,
            },
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


        $scope.closeResponsiblePopUp();

    };


    //-------------------------------------------------------------------------
   
    $scope.SaveResponsiblePerson = function () {
        try {

            //if (baseService.arrayLength($scope.BudgetCodeList) > 0) {
            //    angular.forEach($scope.ResponsibleList, function (a) {
            //        if (checkResponsiblePersonExist($scope.userResponsiblePersonList, a.Id) === false) {
            //            if (a.chk) {
            //                var ob = {};
            //                ob.Id = null;                           
            //                ob.isActive = a.isActive
            //                $scope.userResponsiblePersonList.push(ob);
            //                ob = {};
            //            }
            //        }

            //    });
            //}

            $scope.SaveResponsibleList = [];
            for (var i = 0; i < $scope.ResponsibleList.length; i++) {
                $scope.SaveResponsibleList.push($scope.ResponsibleList[i]);
            }
            $scope.$broadcast('show-errors-check-validity');

            $http({
                method: 'POST',
                url: 'Materials/DetentionLog/saveDtentionLogResPerson',
                data: {
                    'data': $scope.SaveResponsibleList,
                    'detentionLogId': $scope.ModalNew.Id
                },
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
        catch (ex) {
            ShowResult(ex, 'failure');
        }
        $scope.closeResponsiblePopUp();
    };
    //-------------------------------------------------------------------------
    //-------------------------------------------------------------------------

    // Get Machine Master Asset
    $scope.MachineMasterAssetList = [];
    $scope.getMachineMasterAsset = function () {
        $http({
            method: 'POST',
            url: 'Materials/DetentionLog/getMachineMasterAsset',
            dataType: 'JSON',
        }).then(function successCallback(response) {
            $scope.MachineMasterAssetList = response.data;
           // $scope.getWorkCenter();
        });
    }
    $scope.getMachineMasterAsset();

    // #region Reports

    $scope.ModalTempClosedDetention = {
        From: null,
        To: null,
        DepartmentId: null,
        DetentionTypeId: null
    };
    $scope.ModalNewClosedDetention = Object.assign({}, $scope.ModalTempClosedDetention);
    $scope.ClosedDetentionList = [];
    $scope.GetClosedDetentionGridReport = function () {
        $http.get('Materials/DetentionLogout/GetClosedDetentionGridReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
            .then(function successCallback(response) {
                $scope.ClosedDetentionList = response.data;
            })
    }

    $scope.PendingDetentionList = [];
    $scope.GetPendingDetentionGridView = function () {
        $http.get('Materials/DetentionLogout/GetPendingDetentionGridView?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
            .then(function successCallback(response) {
                $scope.PendingDetentionList = response.data;
            })
    }

    $scope.fileName = "ClosedDetentionReport.xlsx";
    $scope.XlsGetClosedDetentionReport = function () {

        //$http.get('Materials/DetentionLogout/XlsGetClosedDetentionReport?from=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId)
        $http({
            method: 'POST',
            url: 'Materials/DetentionLogout/XlsGetClosedDetentionReport?parameters=' + $scope.ModalNewClosedDetention.From + '&to=' + $scope.ModalNewClosedDetention.To + '&departmentId=' + $scope.ModalNewClosedDetention.DepartmentId + '&detentiontypeId=' + $scope.ModalNewClosedDetention.DetentionTypeId,
            dataType: 'JSON',
        })
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);

                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };

    $scope.fileName = "PendingDetentionReport.xlsx";
    $scope.XlsGetPendingDetentionView = function () {

        $http.get('Materials/DetentionLogout/XlsGetPendingDetentionView?parameters=' + $scope.ModalNewClosedDetention)
            .then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {

                    //$rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                    $window.open($scope.downloadgriddataUrlPath + "?FullPath=" + response.data.FileName + "&fileName=" + $scope.fileName);
                }
            }, function errorCallback(response) {
                ShowResult(response.data.Message, 'failure');
            });

    };
    // #endregion Reports
}
//-----------------------------------------------------------------------------------

function openModal() {
    $('.confirm-delete').addClass('hide');
    $('#myModal .modal-header, .modal-footer, .modal-body').removeClass('hide');
    $('#myModal').modal('show');
}
//-----------------------------------------------------------------------------------