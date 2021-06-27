'use strict';
stitchCodeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter'];
function stitchCodeController(commonMessage, $scope, $rootScope, baseService, $http, $filter) {
    $rootScope.title = "Stitch Code";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.modelList = [];
    $scope.path = 'Machines/StitchCode/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.model = {
        Id: null
        , CompanyGroupId: null
        , PlantId: null
        , Sequence: 0.0
        , Needle: 0
        , Bobbin: 0
        , Looper: 0
        , Code: null
        , TMU: null
        , ShortName: null
        , StandardName: null
        , UserName: null
        , Description: null
        , Remarks: null
        , Active: true
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    baseService.init('Machines/StitchCode/getlist');
    $scope.getData = function () {
        $scope.getSearchData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.modelList = [];
                    $scope.modelList = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        }; $scope.getSearchData();
    };
    $scope.getData();

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.modelNew.Sequence = response.data;
            });
    };
    $scope.GetSequence();

    $scope.Get = function (index) {
        $scope.index = index;
        $scope.model = $scope.modelList[$scope.index];
        $scope.modelNew = Object.assign({}, $scope.model);
        $scope.modelNew.Bobbin = isNaN($scope.modelNew.Bobbin) ? 0 : $scope.modelNew.Bobbin;
        $scope.modelNew.Looper = isNaN($scope.modelNew.Looper) ? 0 : $scope.modelNew.Looper;

        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        try {
            $scope.modelNew.Bobbin = isNaN($scope.modelNew.Bobbin) ? 0 : $scope.modelNew.Bobbin;
            $scope.modelNew.Looper = isNaN($scope.modelNew.Looper) ? 0 : $scope.modelNew.Looper;

            if (!baseService.isUndefinedOrNull($scope.modelNew.Bobbin) && !isNaN($scope.modelNew.Bobbin) && $scope.modelNew.Bobbin != 0
                && !baseService.isUndefinedOrNull($scope.modelNew.Looper) && !isNaN($scope.modelNew.Looper) && $scope.modelNew.Looper != 0) {
                throw "Please insert data only for Bobbin or for Looper.";
            }

            if (($scope.modelNew.Bobbin + $scope.modelNew.Looper + $scope.modelNew.Needle) > 100) {
                throw "Sum cann't greater than 100.";
            }
            if (($scope.modelNew.Bobbin + $scope.modelNew.Looper + $scope.modelNew.Needle) < 100) {
                throw "Sum cann't less than 100.";
            }

            if (!baseService.isUndefinedOrNull($scope.modelNew.Bobbin) || $scope.modelNew.Bobbin > 0) {
                if ($scope.modelNew.Bobbin != 0) {
                    if (($scope.modelNew.Bobbin + $scope.modelNew.Needle) > 100) {
                        throw "Sum of Needle and Bobbin cann't greater than 100.";
                    }
                    if (($scope.modelNew.Bobbin + $scope.modelNew.Needle) < 100) {
                        throw "Sum of Needle and Bobbin cann't less than 100.";
                    }
                }
            }

            if (!baseService.isUndefinedOrNull($scope.modelNew.Looper) || $scope.modelNew.Looper > 0) {
                if ($scope.modelNew.Looper != 0) {
                    if (($scope.modelNew.Looper + $scope.modelNew.Needle) > 100) {
                        throw "Sum of Needle and Looper cann't greater than 100.";
                    }
                    if (($scope.modelNew.Looper + $scope.modelNew.Needle) < 100) {
                        throw "Sum of Needle and Looper cann't less than 100.";
                    }
                }
            }

            angular.copy($scope.modelNew, $scope.model);
            $scope.$broadcast('show-errors-check-validity');
            if ($scope.modelNewForm.$valid) {
                if ($scope.Action === "Save") {
                    $http({
                        method: 'POST'
                        , url: $scope.saveUrl
                        , data: $scope.model
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failureStitchCode');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.modelList.push(response.data.entity);
                            $scope.modelList = $filter('orderBy')($scope.modelList, 'Sequence');
                            baseService.paginationAdd();
                            ClearFields(response.data.Sequence);
                        }
                    }), function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    };
                }
                else if ($scope.Action === "Update") {
                    $http({
                        method: 'POST'
                        , url: $scope.updateUrl
                        , data: $scope.model
                        , dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.modelList[$scope.index] = $scope.model;
                                $scope.modelList = $filter('orderBy')($scope.modelList, 'Sequence');
                            }
                            ClearFields(response.data.Sequence);
                        }
                    }, function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    });
                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Delete = function () {
        try {
            if (!baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                $http({
                    method: 'POST'
                    , url: $scope.deleteUrl + $scope.modelNew.Id
                    , dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.modelList.splice($scope.index, 1);
                        baseService.paginationRemove();
                        ClearFields(response.data.Sequence);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };
    function ClearFields(seq) {
        $scope.Action = "Save";
        $scope.model = {};
        $scope.modelNew = { Sequence: seq, Active: true };
        $scope.formulaDetails = [];
    }

    $scope.ind = -1;
    $scope.FormulaClick = function (x, index) {
        $scope.ind = index;
        for (var i = 0; i < $scope.formulaDetails.length; i++) {
            if ($scope.ind == i) {
                if ($scope.formulaDetails[i].SPI == x.SPI && x.IsFormula == true) {
                    $scope.formulaDetails[i].FixedValue = 0;
                }
                if ($scope.formulaDetails[i].SPI == x.SPI && x.IsFormula == false) {
                    $scope.formulaDetails[i].IsFormula = false;
                    $scope.formulaDetails[i].Formula = null;
                }
            }
            $scope.ind = -1;
            break;
        }

    }
    $scope.FormulaModel = {
        Id: null,
        SPI: 1,
        StitchCodeId: null,
        IsFormula: true,
        FixedValue: 0,
        Formula: null
    }
    $scope.formulaDetails = [];


    $scope.ShowFormulaPopup = function () {
        $scope.formulaDetails = [];
        try {
            if (baseService.isUndefinedOrNull($scope.modelNew.Id)) {
                throw 'Select a Stitch Code.';
            }

            $http({
                method: 'GET'
                , url: 'Machines/StitchCode/GetSPIFormulaList?StitchCodeId=' + $scope.modelNew.Id
                , contentType: "application/json; charset=utf-8"
            }).then(function successCallback(response) {
                $scope.formulaDetails = response.data;

                if (baseService.arrayLength($scope.formulaDetails) <= 0) {

                    for (var i = 1; i < 21; i++) {
                        var obj = angular.copy($scope.FormulaModel);
                        obj.SPI = i;
                        obj.StitchCodeId = $scope.modelNew.Id;
                        $scope.formulaDetails.push(obj);
                    }
                }

            });
            
            angular.element(document.querySelector('#detailpopup')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.AddNewRow = function () {
        $scope.formulaDetails.push({
            Id: null
            , SPI: $scope.formulaDetails.length+1
            , StitchCodeId: $scope.modelNew.Id
            , IsFormula: true
            , FixedValue: 0
            , Formula: null
        });
    };

    $scope.SaveSPI = function () {
        try {
            if (baseService.arrayLength($scope.formulaDetails) > 0) {
                $http({
                    method: 'POST',
                    url: 'Machines/StitchCode/CreateSPI',
                    data: {
                        'data': $scope.formulaDetails
                    },
                    dataType: 'JSON'
                    , contentType: "application/json charset=utf-8"
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');

                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                };
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };

}
