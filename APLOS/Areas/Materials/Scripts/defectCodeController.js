'use strict';
function DefectCodeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Defect Code";
    $scope.Action = 'Save';
    $scope.CAction = 'Add Row';
    $scope.index = -1;
    $scope.indexdetails = -1;
    $scope.defectCodes = [];
    $scope.path = 'Materials/defectCode/';
    $scope.getListUrl = $scope.path + 'getlist';
    //$scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getDefectCodeDetailListUrl = 'Materials/defectcode/getdefectcodedetaillist/';
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, null, null, "ProcessName", "ProcessName");
    $scope.getData = function (pageno) {
        $rootScope.parameters.processId = $scope.defectCodeNew.ProcessId;
        baseService.pagination(pageno)
                   .then(function (result) {
                       $scope.defectCodes = result.Rows;
                   }, function () {
                       ShowResult(commonMessage.NetworkError, 'failure');
                   }).finally(function () {
                   });
    };

    $scope.defectCode = {
        Id: null,
        CompanyGroupId: null,
        ProcessId: null,
        ProcessName: null,
        Code: null,
        Description: null,
        Active: true,
        Archive: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.defectCodeNew = Object.assign({}, $scope.defectCode);

    $scope.defectCodeDetail = {
        Id: null,
        DefectCodeId: null,
        Zone: null,
        ZoneName: null,
        Point: null,
        Archive: null,
        AddedBy: null,
        AddedDate: null,
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    };
    $scope.defectCodeDetailNew = Object.assign({}, $scope.defectCodeDetail);

    $scope.processIdList = [];

    $http({
        method: 'GET',
        url: 'Processes/process/getcbo'
    }).then(function successCallback(response) {
        $scope.processIdList = response.data;
    });
    $http({
        method: 'GET',
        url: 'Materials/fgzone/getcbo',
    }).then(function successCallback(response) {
        $scope.zoneList = response.data;
    });
    $scope.defectCodeDetails = [];
    $scope.addRow = function () {
        angular.copy($scope.defectCodeDetailNew, $scope.defectCodeDetail);
        if ($scope.defectCodeDetail.Zone == null) {
            ShowResult('Please select zone', 'failure');
            return;
        }
        if ($scope.defectCodeDetail.Point == 0) {
            ShowResult('Point value must be grater then zero.', 'failure');
            return;
        }
        if ($scope.defectCodeDetail.Point == null) {
            ShowResult('Please input point value.', 'failure');
            return;
        }
        try {
            var isAvailable = false;
            for (var i = 0; i < $scope.defectCodeDetails.length; i++) {
                isAvailable = listValidation($scope.defectCodeDetails[i].Zone, $scope.defectCodeDetail.Zone, i);
                if (isAvailable) {
                    throw 'This zone has been already taken';
                }
            }
            if (!isAvailable) {
                $scope.defectCodeDetail.ZoneName = document.getElementById("zoneId").options[document.getElementById('zoneId').selectedIndex].text
                if ($scope.indexdetails != -1) {
                    $scope.defectCodeDetails[$scope.indexdetails] = $scope.defectCodeDetail;
                }
                else {
                    $scope.defectCodeDetail.DefectCodeId = $scope.defectCodeNew.Id;
                    $scope.defectCodeDetails.push($scope.defectCodeDetail);
                }
                $scope.indexdetails = -1;
                $scope.CAction = 'Add Row';
                $scope.defectCodeDetail = {};
                $scope.defectCodeDetailNew = {};
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // MaterialAttributeId
        if ($scope.indexdetails == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope.indexdetails != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    $scope.dDetailsId = [];
    $scope.removeRow = function () {
        if ($rootScope.id != null)
            $scope.dDetailsId.push($rootScope.id)
        $scope.defectCodeDetails.splice($rootScope.index, 1);
        $rootScope.id = null;
    };

    $scope.GetDefectCodeDetail = function (id, index) {
        $scope.indexdetails = index;
        $scope.defectCodeDetail = $scope.defectCodeDetails[$scope.indexdetails];
        $scope.defectCodeDetailNew = Object.assign({}, $scope.defectCodeDetail);
        $scope.CAction = 'Update Row';

    };

    $rootScope.searchByDefectCodeList = [
        {
            'name': 'Process',
            'value': 'ProcessName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        }
    ];

    $scope.getDefectCodeDetail = function () {
        $scope.parameters = {
            limit: 20,
            offset: 0,
            order: 'asc',
            sort: 'Zone',
            searchBy: "DefectCodeId",
            search: $scope.defectCodeNew.Id
        };
        baseService.paginationBase($scope.getDefectCodeDetailListUrl, 1, $scope.parameters)
            .then(function (result) {
                $scope.defectCodeDetails = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.defectCode = $scope.defectCodes[$scope.index];
        $scope.defectCodeNew = Object.assign({}, $scope.defectCode);
        $scope.getDefectCodeDetail();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.processName = $("#processId option:selected").text();
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.defectCodeForm.$valid) {
            angular.copy($scope.defectCodeNew, $scope.defectCode);
            if ($scope.Action === "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { 'defectCode': $scope.defectCode, 'defectCodeDetail': $scope.defectCodeDetails },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.defectCode = response.data.DefectCode;
                        $scope.defectCode.ProcessName = $scope.processName;
                        $scope.defectCodes.push($scope.defectCode);
                        baseService.paginationAdd();
                        ClearFields();
                    }
                })
            }
            else if ($scope.Action == "Update") {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: { defectCode: $scope.defectCode, defectCodeDetail: $scope.defectCodeDetails, deletedItems: $scope.dDetailsId },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.defectCode.ProcessName = $scope.processName;
                            $scope.defectCodes[$scope.index] = $scope.defectCode;
                        }
                        ClearFields();
                    }
                })
            }
        }
    }

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.defectCodeNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.defectCodeNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.defectCodes.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            });
        }
    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    }

    function ClearFields() {
        $scope.CAction = 'Add Row';
        $scope.Action = "Save";
        $scope.defectCode = {};
        $scope.defectCodeNew = { ProcessId: $scope.defectCodeNew.ProcessId };
        $scope.defectCodeNew.Active = true;
        $scope.defectCodeDetails = [];
        $scope.defectCodeDetail = {};
        $scope.defectCodeDetailNew = {};
        //$scope.defectCode.Sequence = seq;
    }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
};
DefectCodeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
