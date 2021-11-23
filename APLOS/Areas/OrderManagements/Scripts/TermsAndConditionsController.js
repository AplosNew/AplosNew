'use strict';
TermsAndConditionsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function TermsAndConditionsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Terms And Conditions';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'OrderManagements/TermsAndConditions/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.saveGridUrl = $scope.path + 'SaveData';
    $scope.saveTitleUrl = $scope.path + 'SaveTitle';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }, { value: 'Description', name: "Description" }, { value: 'Remarks', name: "Remarks" }];


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Type: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        MaxLimit: 0,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        Mandatory: false
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.typesList = [];
    cboService.getEnumCbo('Enum/GetTermsAndConditionsEnumCbo', function (result) {
        $scope.typesList = result;
    });


    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
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
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    /* ClearFields(response.data.Sequence);*/
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };
    $scope.GridList = [];
    $scope.TitleModel = {
        Id: null,
        TermsAndConditionsMasterId: null,
        TermsAndConditionsChildId : null,
        TermsAndConditionsDetailId: null,
        Title: null,
        Header: null,
        Description: null
    }
    $scope.TitleList = [];
    $scope.GridTitle = function () {

        $http.get('OrderManagements/TermsAndConditions/GetTitle?masterID=' + $scope.ModelNew.Id)
            .then(function (response) {

                //for (var i = 0; i < response.data.length; i++) {
                //    try {
                //        response.data[i].AddedDate = new Date(response.data[i].AddedDate);
                //    } catch (e) {

                //    }
                $scope.TitleList = response.data;
            });
    }

    $scope.loadGrid = function () {
        try {
            if ($scope.GridList > 0) {


                for (var i = 0; i < $scope.GridList.length; i++) {
                    if (baseService.isUndefinedOrNull($scope.GridList[i].Description)) {
                        throw "Description is empty.";
                    }
                    if (baseService.isUndefinedOrNull($scope.GridList[i].Header)) {
                        throw "Header Description is empty.";
                    }
                }
            }
            var newObj = {
                Id: null,
                TermsAndConditionsMasterId: null,
                TermsAndConditionsChildId: null,
                TermsAndConditionsDetailId: null,
                Title:null,
                Header: null,
                Description: null
            };

            newObj.Title = $scope.TitleModel.Title;
            newObj.Header = null;
            newObj.Description = null;

            $scope.GridList.push(newObj);
            newObj = {
                Id: null,
                TermsAndConditionsMasterId: null,
                TermsAndConditionsChildId: null,
                TermsAndConditionsDetailId: null,
                Title: null,
                Header: null,
                Description: null
            };
        } catch (e) {
            ShowResult(e, 'info');
        }
    };
    // $scope.loadGrid();

    $scope.SaveGrid = function () {
        $scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveGridUrl,
            data: { 'GridData': $scope.POPupList, 'TitleId': $scope.TitleId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                /*ClearFields(response.data.Sequence);*/
                //$scope.LoadRackList();
                //$scope.GetDetails({ data: { Id: response.data.Data.Id } });
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.SaveTitle = function () {
        $scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveTitleUrl,
            data: { 'TitleData': $scope.TitleModel, 'TitleId': $scope.ModelNew.Id},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                /*ClearFields(response.data.Sequence);*/
                //$scope.LoadRackList();
                //$scope.GetDetails({ data: { Id: response.data.Data.Id } });
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };




    $scope.Remove = function (index) {
        var removed = $scope.GridList.splice(index, 1);
        $scope.TitleModel = removed;
        //$scope.Detail.pop(); 
    }

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.TitleModel = {};
    }

    $scope.showRemarksPopUp = function (args) {
        $scope.TitleId = args.Id;
        //$scope.GetRemarksByMaster($scope.TitleModel.TermsAndConditionsDetailId);
        angular.element(document.querySelector('#GridPopUp')).modal('show');
    }

    $scope.closeRemarksPopUp = function () {

        angular.element(document.querySelector('#GridPopUp')).modal('hide');
    }

    $scope.POPupList = [];

    $scope.GetRemarksByMaster = function (id) {
        $scope.POPupList = [];
        $http.get('OrderManagements/TermsAndConditions/GetPopUp?TermsAndConditionsDetailId=' + id)
            //.then(function (response) {

            //    for (var i = 0; i < response.data.length; i++) {
            //        try {
            //            response.data[i].AddedDate = new Date(response.data[i].AddedDate);
            //        } catch (e) {

            //        }
            //    }
        $scope.POPupList = response.data;
     /*       });*/
    }

    $scope.SaveRemarks = function (model) {
        try {
            $scope.OrderControlNew.ControlTypeId = $scope.OrderControl.ControlTypeId;
            $http({
                method: 'POST',
                data: { data: $scope.OrderControlNew, child: model.data },
                url: 'OrderManagements/OrderControl/Create'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetDayAndLevel();
                    $scope.GetRemarksByMaster($scope.OrderControlNew.Id);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }
    $scope.DeleteRemarks = function (model) {
        try {

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: 'OrderManagements/OrderControl/Delete'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRemarksByMaster($scope.OrderControlNew.Id);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

}