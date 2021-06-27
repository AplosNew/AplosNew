'use strict';
function OperationVideoUploadController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $sce) {
    $rootScope.title = "Operation Video Upload";
    $scope.Action = 'Save';
    $scope.operationVideoUploadList = [];
    $scope.index = -1;
    $scope.indexGetTime = 0;
    $scope.path = 'IE/operationvideoupload/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl, null, 25, null, null, 'Sequence');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.operationVideoUploadList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $("#filec").change(function () {
        console.log("video file has been chosen")
        //grab the first image in the fileList
        //in this example we are only loading one file.
        //console.log(this.files[0].size);
        renderVideo(this.files[0]);

    });
    $scope.the_url = "";
    $scope.filename = "";
    $scope.filedata = "";
    function renderVideo(file) {
        $scope.filedata = file;
        var reader = new FileReader();
        reader.onload = function (event) {
            $scope.the_url = event.target.result;
            $('#data-vid').html("<video width='400' controls><source id='vdid' src='" + $scope.the_url + "' type='video/mp4'></video>");
            $scope.filename = file.name;
            //$('#name-vid').html(file.name)
        }
        reader.readAsDataURL(file);
    }
    $scope.uploadFile = function () {
        var data = new FormData();
        console.log($scope.filedata);
        data.append("file", $scope.filedata);
        $http({
            method: "POST",
            url: $scope.saveUrl,
            withCredentials: true,
            processData: false,
            headers: { 'Content-Type': undefined },
            contentType: undefined,
            dataType: JSON,
            data: data,
            transformRequest: angular.identity
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, "failure");
            }
            else {
                ShowResult(response.data.Message, "success");
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });
    };

    $scope.searchList = [
        {
            'name': 'Id',
            'value': 'Id'
        },
        {
            'name': 'File Name',
            'value': 'FileName'
        },
        {
            'name': 'Sequence',
            'value': 'Sequence'
        }
    ];
    $scope.getData();
    $http({
        method: 'GET',
        url: 'IE/operationvideoupload/getoperationvideouploadlistcbo/'
    }).then(function successCallback(response) {
        // $scope.operationList = result;
    });

    $scope.operationVideoUpload = {
        Id: null,
        OperationId: null,
        ProcessId: null,
        FileName: null,
        Sequence: null,
        Active: true,
        AddedBy: null,
        AddedDate: $filter("date")(Date.now(), 'yyyy-MM-dd'),
        AddedFromIP: null,
        UpdatedDate: $filter("date")(Date.now(), 'yyyy-MM-dd')
    };

    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.operationVideoUpload = $scope.operationVideoUploadList[$scope.index];
    };
    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.timecaptureForm.$valid) {
            if ($scope.Action == 'Save') {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: $scope.operationVideoUpload,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.operationVideoUploadList.push(response.data.operationVideoUpload);
                        baseService.paginationAdd();
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
            else if ($scope.Action == 'Update') {
                $http({
                    method: 'POST',
                    url: $scope.updateUrl,
                    data: $scope.operationVideoUpload,
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        if ($scope.index > -1) {
                            $scope.operationVideoUploadList[$scope.index] = $scope.operationVideoUpload;
                        }
                        ClearFields(response.data.Sequence);
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, 'failure');
                });
                return true;
            }
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.operationVideoUpload.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.operationVideoUpload.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.operationVideoUploadList.splice($scope.index, 1);
                    baseService.paginationRemove();
                    ClearFields(response.data.Sequence);
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, 'failure');
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, 'failure');
        }
        return true;
    }
    $scope.Clear = function () {
        console.log($scope.operationVideoUpload.FileName);
        ClearFields();
        return true;
    }
    function ClearFields(seq) {
        console.log($scope.operationVideoUpload.FileName);
        $scope.Action = 'Save';
        $scope.operationVideoUpload = {};
        $scope.operationVideoUpload.Sequence = seq;
        $scope.operationVideoUpload.FileName = seq;
        $scope.operationVideoUpload.OperationId = seq;
        $scope.operationVideoUpload.Active = true;
    }
    function ClearFields() {
        console.log($scope.operationVideoUpload.FileName);
        $scope.Action = "Save";
        $scope.operationVideoUpload = "";
        //$scope.characteristicsValue.IsFixedNoOfCharacter = true;
        //$scope.characteristicsValue.IsRestrictable = true;
        //$scope.characteristicsValue.IsEntryRequired = true;
        //$scope.characteristicsValue.IsActive = true;
        //$scope.characteristicsValue.IsDefault = false;
    }
    $scope.ShowProcessList = function () {
        var modalOptions = {
            closeButtonText: 'Cancel',
            actionButtonText: 'Delete Characteristics Value',
            headerText: 'Delete ' + custName + '?',
            bodyText: 'Are you sure you want to delete this Characteristics Value?'
        };
        modalService.showModal({}, modalOptions).then(function (result) {
            if (result === 'ok') {
                dataService.deleteCustomer(id).then(function () {
                    for (var i = 0; i < vm.customers.length; i++) {
                        if (vm.customers[i].id === id) {
                            vm.customers.splice(i, 1);
                            break;
                        }
                    }
                    filterCustomers(vm.searchText);
                }, function (error) {
                    $window.alert('Error deleting Characteristics Value: ' + error.message);
                });
            }
        });
    }
    $scope.Show = function () {
        //GetAll();
    }
    $scope.processList = [
        {
            'name': 'Process1',
            'value': 'P1'
        },
        {
            'name': 'Process2',
            'value': 'P2'
        },
        {
            'name': 'Process3',
            'value': 'P3'
        }
    ];
    $scope.operationList = [
        {
            'name': 'Operation1',
            'value': 'OP1'
        },
        {
            'name': 'Operation2',
            'value': 'OP2'
        },
        {
            'name': 'Operation3',
            'value': 'OP3'
        },
        {
            'name': 'Operation4',
            'value': 'OP4'
        },
        {
            'name': 'Operation5',
            'value': 'OP5'
        }
    ];
};
OperationVideoUploadController.$inject = ["commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$sce"];
