'use strict';
separationtypeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function separationtypeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Separation Type';
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.SeparationTypes = [];
    $scope.path = 'Employees/SeparationType/';
    $scope.getSHListUrl = $scope.path + 'GetSalaryHeadlist';
    $scope.getETListUrl = $scope.path + 'GetEmploymentTypelist';
    $scope.getETFixedListUrl = $scope.path + 'GetEmploymentTypelistForFiexdDays';
    $scope.getDataForEditUrl = $scope.path + 'GetDataForEdit';
    $scope.saveUrl = $scope.path + 'SaveSeparationType';
    $scope.getListUrl = $scope.path + 'GetSeparationTypelist';
    $scope.getUrl = $scope.path + 'get';
    $scope.getSeqUrl = $scope.path + 'getautosequence';

    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';

    $scope.salaryRuleGeneral = {
        FormulaDescription: null,
        FormulaIDDescription: null
    };
    //$scope.getData();
    $scope.salaryHeadList = [];
    $scope.getSH = function () {
        try {
            $http.get($scope.getSHListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.salaryHeadList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSH();
    $scope.EmploymentTypeList = [];
    $scope.getEmploymentType = function () {
        try {
            $http.get($scope.getETListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmploymentTypeList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getEmploymentType();
    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.SetFormula = function (formula) {

        if (formula === 'SHead') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.SalaryHeadIdFormula)) {
                $scope.salaryRuleGeneral.SalaryHeadFormula = $("#SalaryHeadFormula option:selected").text();

                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.SalaryHeadFormula;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.SalaryHeadIdFormula;
            }

            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Operator') {
            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Operator)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Operator;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Operator;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Precedence') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Precedence)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Precedence;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Precedence;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
        else if (formula === 'Value') {
            $scope.salaryRuleGeneral.FormulaDescription = null;
            $scope.salaryRuleGeneral.FormulaIDDescription = null;

            if (!baseService.isUndefinedOrNull($scope.salaryRuleGeneral.Value)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.salaryRuleGeneral.Value;
                $scope.salaryRuleGeneral.FormulaDesID = $scope.salaryRuleGeneral.Value;
            }
            $scope.FormulaArray.push($scope.salaryRuleGeneral.FormulaDes);
            $scope.FormulaIdArray.push($scope.salaryRuleGeneral.FormulaDesID);

            $scope.salaryRuleGeneral.FormulaIDDescription = null;
            $scope.salaryRuleGeneral.FormulaDescription = null;
            for (var i = 0; i < $scope.FormulaArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                    $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
                }
            }

            for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
                if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                    $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];
                }
                else {
                    $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
                }
            }

        }
    };

    $scope.RemoveFormula = function () {
        $scope.salaryRuleGeneral.FormulaDesID = null;

        var count = $scope.FormulaArray.length;
        $scope.FormulaArray.splice(count - 1);

        var count = $scope.FormulaIdArray.length;
        $scope.FormulaIdArray.splice(count - 1);

        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.salaryRuleGeneral.FormulaDes = null;
        for (var i = 0; i < $scope.FormulaArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaDescription)) {
                $scope.salaryRuleGeneral.FormulaDes = $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription = $scope.FormulaArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDes += $scope.FormulaArray[i];
                $scope.salaryRuleGeneral.FormulaDescription += ' ' + $scope.FormulaArray[i];
            }
        }

        for (var i = 0; i < $scope.FormulaIdArray.length; i++) {
            if (baseService.isUndefinedOrNull($scope.salaryRuleGeneral.FormulaIDDescription)) {
                $scope.salaryRuleGeneral.FormulaDesID = $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription = $scope.FormulaIdArray[i];


            } else {
                $scope.salaryRuleGeneral.FormulaDesID += $scope.FormulaIdArray[i];
                $scope.salaryRuleGeneral.FormulaIDDescription += ' ' + $scope.FormulaIdArray[i];
            }
        }
    };

    $scope.TempModel = {
        Id: null,
        YearNo: 0,
        DayNo: 0,
        RoundUp: false,
        EmploymentType: null
    };
    $scope.SeparationTypeDetails = [];
    $scope.CreateTempList = function () {
        $scope.SeparationTypeDetails = [];
        for (var i = 0; i < 30; i++) {
            $scope.TempModel = {};
            $scope.TempModel.Id = i + 1;
            $scope.TempModel.YearNo = i + 1;
            $scope.TempModel.DayNo = 0;
            $scope.TempModel.RoundUp = false;
            $scope.SeparationTypeDetails.push($scope.TempModel);          
        }


    };
    $scope.CreateTempList();
    $scope.AddNewRoweparationTypeDetails = function () {      
            $scope.TempModel = {};
            $scope.TempModel.Id = 0
            $scope.TempModel.YearNo = 0;
            $scope.TempModel.DayNo = 0;
            $scope.TempModel.RoundUp = false;
        $scope.SeparationTypeDetails.push($scope.TempModel);
        var gridObj = $("#GridSeparationTypesList").data("ejGrid");
        gridObj.refreshContent();
        
    };


    $scope.SeparationType = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        FormulaDes: null,
        FormulaDesID: null,
        IsGratuityApplicable: false,
        IsNetPayWithFinalSattlement: false,
        IsActive: true,
        AddedBy: null,
        AddedDate: new Date('dd-MMM-yyyy'),
        AddedFromIP: null,
        UpdatedDate: null
    };
    $scope.SeparationTypesList = [];
    $scope.getSeparationTypesList = function () {
        try {
            $http.get($scope.getListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.SeparationTypesList = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.getSeparationTypesList();

    $scope.EmploymentTypelistForFiexdDays = [];
    $scope.GetEmploymentTypelistForFiexdDays = function () {
        try {
            $http.get($scope.getETFixedListUrl)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        $scope.EmploymentTypelistForFiexdDays = response.data;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.GetEmploymentTypelistForFiexdDays();

    $scope.GetSequence = function () {
        $http.get($scope.getSeqUrl)
            .then(function (response) {
                $scope.SeparationType.Sequence = response.data[0].Sequence;
            });
    };

    $scope.CheckIdUse = function (id) {
        $http.get('accounts/SeparationType/checkiduse?id=' + id)
            .then(function (response) {
                $scope.checkIdUsedValue = response.data;
            });
    };

    $scope.GetSequence();



    $scope.Save = function () {

        $scope.SeparationType.FormulaDes = $scope.salaryRuleGeneral.FormulaDescription;
        $scope.SeparationType.FormulaDesID = $scope.salaryRuleGeneral.FormulaIDDescription;
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: { 'SeparationTypeData': $scope.SeparationType, 'SeparationTypeDetailsData': $scope.SeparationTypeDetails, 'SeparationTypeFixedDayAmountData': $scope.EmploymentTypelistForFiexdDays },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.SeparationTypes.push(response.data.SeparationType);
                baseService.paginationAdd();
                ClearFields(response.data.Sequence);
                $scope.getSeparationTypesList();
            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, 'failure');
        });



        $scope.$broadcast('show-errors-check-validity');
        if ($scope.SeparationTypeForm.$valid) {
            if ($scope.Action === 'Save') {

                return true;
            }
            else if ($scope.Action === 'Update') {
                //$http({
                //    method: 'POST',
                //    url: $scope.updateUrl,
                //    data: $scope.SeparationType,
                //    dataType: 'JSON'
                //}).then(function successCallback(response) {
                //    if (response.data.Error == true) {
                //        ShowResult(response.data.Message, 'failure');
                //    }
                //    else {
                //        ShowResult(response.data.Message, 'success');
                //        if ($scope.index > -1) {
                //            $scope.SeparationTypes[$scope.index] = $scope.SeparationType;
                //        }
                //        ClearFields(response.data.Sequence);
                //    }
                //}, function errorCallback(response) {
                //    ShowResult(response.status.Message, 'failure');
                //});
                //return true;
            }
        }
    };
    $scope.SeparationTypeNew = {};
    $scope.getDataForEdit = function (Id) {
        try {
            $http.get($scope.getDataForEditUrl + '?Id=' + Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.Message, 'failure');
                    }
                    else {

                        //$scope.SeparationTypeDetailsTemp = [];
                        //for (var i = 0; i < 30; i++) {
                        //    $scope.TempModel = {};
                        //    $scope.TempModel.Id = i + 1;
                        //    $scope.TempModel.YearNo = i + 1;
                        //    $scope.TempModel.DayNo = 0;
                        //    $scope.TempModel.RoundUp = false;
                        //    $scope.SeparationTypeDetailsTemp.push($scope.TempModel);
                        //}


                        $scope.EmploymentTypelistForFiexdDays = [];
                        $scope.SeparationTypeDetailsNew = [];
                        $scope.SeparationTypeDetails = [];

                        $scope.SeparationType = response.data.SeparationType[0];
                        $scope.SeparationTypeDetails = response.data.SeparationTypeDetails;
                        $scope.EmploymentTypelistForFiexdDays = response.data.SeparationTypeFixedAmount;


                       
                        //for (var i = 0; i < 30; i++) {
                        //    $scope.TempModel = {};
                        //    var IsEqeal = false;
                        //    for (var j = 0; j < $scope.SeparationTypeDetailsNew.length; j++) {

                        //        if (i + 1 == $scope.SeparationTypeDetailsNew[j].YearNo) {
                        //            $scope.TempModel.Id = $scope.SeparationTypeDetailsNew[j].Id;
                        //            $scope.TempModel.YearNo = $scope.SeparationTypeDetailsNew[j].YearNo;
                        //            $scope.TempModel.DayNo = $scope.SeparationTypeDetailsNew[j].DayNo;
                        //            $scope.TempModel.RoundUp = $scope.SeparationTypeDetailsNew[j].RoundUp;
                        //            $scope.TempModel.EmploymentType = $scope.SeparationTypeDetailsNew[j].EmploymentType;
                        //            IsEqeal = true;
                        //        }


                        //    }
                        //    if (IsEqeal === false) {
                        //        $scope.TempModel.Id = i + 1;
                        //        $scope.TempModel.YearNo = i + 1;
                        //        $scope.TempModel.DayNo = 0;
                        //        $scope.TempModel.RoundUp = false;
                        //        $scope.TempModel.EmploymentType = '';
                        //    }
                        //    $scope.SeparationTypeDetails.push($scope.TempModel);
                        //}







                        $scope.salaryRuleGeneral.FormulaDescription = $scope.SeparationType.FormulaDes;
                        $scope.salaryRuleGeneral.FormulaIDDescription = $scope.SeparationType.FormulaDesID;
                    }
                },

                    function errorCallBack(response) {
                        ShowResult(response.Message, 'failure');
                    });


        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.EditSeparationType = function (obj) {
        var gridObj = $("#GridSeparationTypesList").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        $scope.SeparationTypeNew = data;
        $scope.getDataForEdit(data.Id);
        //$scope.getsalaryRuleGeneral($scope.salaryRuleNew.SystemID);
        //$scope.getSH();
        //$scope.getAutoSequence($scope.salaryRuleNew.SystemID);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
        // $scope.getSalaryRuleESIC();
    }




    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.SeparationType.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.SeparationType.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.SeparationTypes.splice($scope.index, 1);
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
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.SeparationType = {};
        $scope.SeparationType.Sequence = seq;
        $scope.SeparationType.IsActive = true;
        $scope.checkIdUsedValue = false;
        $scope.salaryRuleGeneral.FormulaDescription = null;
        $scope.salaryRuleGeneral.FormulaIDDescription = null;
        $scope.CreateTempList();
        $scope.GetEmploymentTypelistForFiexdDays();
    }
};