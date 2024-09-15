'use strict';
ServiceMasterController.$inject = ['commonMessage', "$window", '$scope', '$rootScope', 'baseService', 'cboService', '$routeParams', '$location', '$http', '$filter'];
function ServiceMasterController(commonMessage, $window, $scope, $rootScope, baseService, cboService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Service Master";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.serviceMasters = [];
    $scope.path = 'Setups/serviceMaster/';
    $scope.getListUrl = $scope.path + 'getlist?ids=null';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    $scope.searchByServiceMasterList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Service Group',
            'value': 'ServiceGroup'
        }
    ];
    baseService.init($scope.getListUrl, null, null, null, "Sequence", "UserName");
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.serviceMasters = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    $scope.serviceMaster = {
        Id: null
        , ServiceGroupId: null
        , CompanyId: null
        , ServiceGroupName: null
        , HSNCodeId:null
        , Sequence: null
        , Code: null
        , UserName: null
        , StandardName: null
        , Description: null
        , Remarks: null
        , Active: true
        , IsPO: true
        , IsApproved: true
        , DrControlId: null
        , CrControlId: null
        , TransactionUoMId:null
    };
    $scope.serviceMasterNew = Object.assign({}, $scope.serviceMaster);

    $scope.serviceGroupList = [];
    $http.get('Setups/ServiceGroup/GetCbo')
        .then(function (response) {
            $scope.serviceGroupList = response.data;
        });
    $scope.companyList = [];
    cboService.getCboCompanyByCompanyGroup(null, function (result) {
        $scope.companyList = result;
    });
    $scope.hsnCodeList = [];
    cboService.getHNSCbo(function (response) {
        $scope.hsnCodeList = response;
    });

    $scope.GetHSNCodeByServiceGroupId = function () {
        $scope.serviceMasterNew.HSNCodeId = null;
        $http.get('Setups/ServiceMaster/GetHSNCodeByServiceGroupId?groupId=' + $scope.serviceMasterNew.ServiceGroupId)
            .then(function (response) {
                
                if (baseService.arrayLength($scope.hsnCodeList)>0) {
                    for (var i = 0; i < $scope.hsnCodeList.length; i++) {
                        if ($scope.hsnCodeList[i].Text === response.data[0].Code) {
                            $scope.serviceMasterNew.HSNCodeId = $scope.hsnCodeList[i].Value;
                        }
                    }
                }
            });
    };


    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.serviceMasterNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();
    $scope.uom = function () {

        cboService.getUoMCbo(function (response) {
            $scope.uoMList = response;
        });
    }
    $scope.uom();
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.serviceMaster = $scope.serviceMasters[$scope.index];
        $scope.serviceMasterNew = Object.assign({}, $scope.serviceMaster);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed)
            $rootScope.toggle();
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.serviceMasterForm.$valid) {
            angular.copy($scope.serviceMasterNew, $scope.serviceMaster);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.serviceMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        $scope.serviceMasters = $filter('orderBy')($scope.serviceMasters, 'Sequence');
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.serviceMaster,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true)
                        ShowResult(response.data.Message, 'failure');
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.getData();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                });
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.serviceMasterNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.serviceMasterNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.serviceMasters.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    }

    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.serviceMaster = {};
        $scope.serviceMasterNew = { Sequence: seq, Active: true };
    }


    $scope.searchglByList = [
        {
            "name": "GL Code",
            "value": "GLGeneralInfoCode"
        },
        {
            "name": "GL Name",
            "value": "GLGeneralInfoName"
        },
        {
            "name": "Account Group",
            "value": "AccountGroupName"
        },
        {
            "name": "COA",
            "value": "COA"
        },
        {
            "name": "Budget",
            "value": "BudgetName"
        },
        {
            "name": "Activity",
            "value": "ActivityName"
        },
        {
            "name": "Ref No",
            "value": "RefNo"
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: "asc",
        sort: "GLGeneralInfoCode",
        searchBy: "ActivityName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.cOAICodeList = [];
    $scope.popUp = function (type) {
        $scope.TrnType = type;
        $scope.cOAICodeList = [];
        baseService.setCurrentPage("cOAICodeList");
        $scope.GetCOAICodeListData = function (pageno) {
            baseService.paginationBase("Accounts/GLItem/GetGLControlList?companyId=" + $scope.serviceMasterNew.CompanyId, pageno, $scope.glListParameters)
                .then(function (result) {
                    $scope.cOAICodeList = result.Rows;
                    $scope.glListParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, "failure", "GLPopUp");
                }).finally(function () {
                });
        };
        angular.element(document.querySelector("#GLPopUp")).modal("show");
        $scope.GetCOAICodeListData();
    };
    $scope.setSelected = function (data,index) {
        if ($scope.TrnType == 'Dr') {
            $scope.serviceMasterNew.DrActivityName = data.ActivityName;
            $scope.serviceMasterNew.DrControlId = data.BudgetMasterActivityId;
        }
        if ($scope.TrnType == 'Cr') {
            $scope.serviceMasterNew.CrActivityName = data.ActivityName;
            $scope.serviceMasterNew.CrControlId = data.BudgetMasterActivityId;
        }
        $scope.closeGLPopUp();
        $scope.Type = null;
    };
    $scope.closeGLPopUp = function () {
        angular.element(document.querySelector("#GLPopUp")).modal("hide");
    };

    //  #region  Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.path + 'GetSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.UploadedData = [];
    $scope.picdata = null;
    $scope.ShowSaveBtn = false;
    $("#uploadImage").change(function () {
        $scope.picdata = this.files[0];
    });

    $scope.getFile = function () {
        $scope.progress = 0;
        fileReader.readAsDataUrl($scope.file, $scope)
            .then(function (result) {
                $scope.imageSrc = result;
            });
    };

    $scope.ImportData = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.ModelNewForm.$valid) {
                var picData = new FormData();
                $http({
                    method: 'POST',
                    url: $scope.path + 'ImportData',
                    headers: { 'Content-Type': undefined },
                    transformRequest: function (data) {
                        picData.append("modelNew", angular.toJson(data.modelNew));
                        if (baseService.isUndefinedOrNull($scope.picdata) === false) {
                            picData.append('file', data.file);
                        }
                        return picData;
                    },
                    data: {
                        'file': $scope.picdata

                    }
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        $scope.ShowSaveBtn = false;
                        ShowResult(response.data.Message, "failure");

                    }
                    else {
                        $scope.UploadedData = [];
                        $scope.UploadedData = response.data;
                        $scope.ShowSaveBtn = true;
                    }
                }, function errorCallback(response) {

                });
                return true;

            }
        } catch (e) {

            ShowResult(e, "failure");
        }
    };
    $scope.SaveUploadedData = function () {
        try {
            for (var i = 0; i < $scope.UploadedData.length; i++) {
                $scope.UploadedData[i].Id = null;
                $scope.UploadedData[i].Active = true;
            }
            $http({
                method: 'POST',
                url: $scope.path + 'SaveUploadedData',
                data: {
                    'data': $scope.UploadedData
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.UploadedData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }


            //$.ajax({
            //    type: "POST",
            //    url: $scope.path + 'SaveUploadedData',
            //    data: {
            //        'data': $scope.UploadedData
            //    },
            //    dataType: "json",
            //    success: function (response) {
            //        if (response.Error === true) {
            //            $scope.ShowSaveBtn = true;
            //            ShowResult(response.Message, 'failure');
            //        }
            //        else {
            //            ShowResult(response.Message, 'success');
            //            $scope.UploadedData = [];
            //            $("#uploadImage").val(null);
            //            $scope.ShowSaveBtn = false;
            //        }

            //    }
            //});

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };
    //  #endregion Data Upload Download

}