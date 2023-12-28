'use strict';
TermsAndConditionsController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$window'];
function TermsAndConditionsController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $window) {
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
            data: { column: $scope.searchBy, value: $scope.search, CompanyId: $scope.ModelNew.CompanyId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ModelList = response.data;

            $scope.GetSequence();
        });
    }
    //$scope.getData();

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
        Mandatory: false,
        CompanyId: null
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
        $scope.GridTitle();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }

    };
    $scope.companyList = [];
    cboService.getCompanyGroupCompanyCbo(null, function (result) {
        $scope.companyList = result;
    });

    $scope.GroupNameList = [];
    $scope.GetGroupNameList = function () {
        $http.get('OrderManagements/TermsAndConditions/GetGroupNameList')
            .then(function (response) {
                $scope.GroupNameList = response.data;
            });
    }
    $scope.GetGroupNameList();


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
        TermsAndConditionsMasterId: $scope.ModelNew.Id,
        Title: null,
        Sequence: 0
    }

    $scope.titleAction = true;
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
                Title: null,
                Header: null,
                Description: null
            };

            newObj.Id = null;
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

    $scope.SaveGrid = function (model) {
        $scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.saveGridUrl,
            data: { 'GridData': model.data, 'TitleId': $scope.TitleId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetRemarksByMaster($scope.TitleId);
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.EditTitle = function (obj) {
        $scope.TitleModel = obj.data;
    }

    $scope.SaveTitle = function () {
        try {
            $scope.TitleModel.TermsAndConditionsMasterId = $scope.ModelNew.Id;
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveTitleUrl,
                data: { 'TitleData': $scope.TitleModel, 'TitleId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.TitleModel = {
                        Id: null,
                        TermsAndConditionsMasterId: $scope.ModelNew.Id,
                        Title: null
                    };
                    $scope.GridTitle();
                }


            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure')
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

    $scope.ClearTitle = function () {
        $scope.TitleModel = {};

    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
        $scope.TitleModel = {};
        $scope.TitleList = [];
        $scope.POPupList = [];

    }

    $scope.showRemarksPopUp = function (args) {
        $scope.TitleId = args.Id;
        $scope.POPupList = [];
        $scope.GetRemarksByMaster($scope.TitleId);
        angular.element(document.querySelector('#GridPopUp')).modal('show');
    }

    $scope.closeRemarksPopUp = function () {

        angular.element(document.querySelector('#GridPopUp')).modal('hide');
    }

    $scope.POPupList = [];

    $scope.GetRemarksByMaster = function (id) {
        $scope.POPupList = [];
        $http.get('OrderManagements/TermsAndConditions/GetPopUp?TermsAndConditionsDetailId=' + id)
            .then(function successCallback(response) {
                $scope.POPupList = response.data;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            })
    }
    $scope.DeleteRemarks = function (model) {
        try {

            $http({
                method: 'POST',
                data: { id: model.data.Id },
                url: 'OrderManagements/TermsAndConditions/DeletePopup'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetRemarksByMaster($scope.TitleId);
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.DeleteTitle = function () {
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

    $scope.DeletePopUp = function () {
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

    $scope.message_detailconfirmation = null;
    $scope.removeBoMDetail = function (obj) {
        $scope.TitleModel = obj.data;
        if (!baseService.isUndefinedOrNull($scope.TitleModel.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.TitleModel.Title + ' ]';
        angular.element(document.querySelector('#confirmBoMDetailPopUp')).modal('show');
        /* $scope.TitleModel = {};*/
        /*       $scope.ClearTitle();*/
    }

    $scope.DeleteBomDetail = function () {
        $http({
            method: 'POST',
            url: 'OrderManagements/TermsAndConditions/DeleteTitle?id=' + $scope.TitleModel.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GridTitle();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    $window.onresize = function (event) {
        $scope.actionComplete();
    };

    $scope.actionComplete = function (args) {
        try {
            if (args.requestType === "refresh") {
                var gridObj = $("#gridRemarks").ejGrid("instance");
                //var scrollerwidth = $("#GridPopUp").width();//Obtain the width of the container
                //gridObj.option({ allowScrolling: true, scrollSettings: { width: scrollerwidth - 20, height: 300, width: 1080 } });//pass the obtainer width and height to gridmodel options
                //gridObj.windowonresize();

                if (args.action == "rowReordering") {
                    gridObj = $("#gridRemarks").data("ejGrid");
                    // Gets current view data of grid control
                    var data = gridObj.getCurrentViewData();
                    var sorteddata = ej.DataManager(data).executeLocal(ej.Query().select(["Id"]));
                    $http({
                        method: 'POST',
                        url: $scope.path + "UpdateMaterialSequence",
                        data: { data: sorteddata }
                    }).then(function successCallback(response) {

                    });
                }
            }
        } catch (e) {
            // $scope.ShowResultCustom(e, 'failure');
        }
    };

}