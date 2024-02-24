'use strict';
balanceSheetSchedulingController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function balanceSheetSchedulingController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
    // #endregion TAB CHANGE

    //  #region BalanceSheetScheduling
    $scope.ActionBalanceSheetScheduling = 'Save';
    $scope.indexBalanceSheetScheduling = -1;
    $scope.balanceSheetSchedulings = [];
    $scope.pathBalanceSheetScheduling = 'accounts/BalanceSheetScheduling/';
    $scope.getListUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'GetList';
    $scope.saveUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'create';
    $scope.deleteUrlBalanceSheetScheduling = $scope.pathBalanceSheetScheduling + 'delete/';
    baseService.init($scope.getListUrlBalanceSheetScheduling);

    $scope.searchBy = "OptionNo"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'OptionNo', name: "OptionNo" }, { value: 'Type', name: "Type" }, { value: 'DetailApplicable', name: "DetailApplicable" }, { value: 'GroupSequence', name: "Group Sequence" }, { value: 'Group', name: "Group" }, { value: 'SubGroupSequence', name: "Sub Group Sequence" }, { value: 'SubGroup', name: "Sub Group" }, { value: 'UserGroup', name: "User Group" }, { value: 'UserSubGroup', name: "User Sub Group" }];

    $scope.getDataBalanceSheetScheduling = function () {
        $http({
            method: 'POST',
            url: $scope.pathBalanceSheetScheduling + "GetList",
            data: {},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.balanceSheetSchedulings = response.data;
        });
    }
    $scope.getDataBalanceSheetScheduling();

    $scope.balanceSheetScheduling = {
        Id: null,
        OptionNo: null,
        Type: null,
        DetailApplicable: null,
        GroupSequence: null,
        Group: null,
        SubGroupSequence: null,
        SubGroup: null,
        UserGroup: null,
        UserSubGroup: null,
        ItemSequence: null,
        ItemNo: null,
        Item: null,
        SubItemNo: null,
        SubItem: null,
        ScheduleNo: null,
        ScheduleName: null,
        UserItem: null,
        UserScheduleName: null,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null,
        IsCalculate: false,
        IsActive: false,
        FormulaDes: null
    };

    $scope.FormulaArray = [];
    $scope.FormulaIdArray = [];

    $scope.FormulaDetails = [];

    $scope.Get = function (args) {
        $scope.balanceSheetScheduling = Object.assign({}, args.data);
        $scope.ActionBalanceSheetScheduling = 'Update';

        $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;

        $scope.FormulaDetails = $scope.balanceSheetScheduling.FormulaDes;

        //for (var i = 0; i < $scope.FormulaDetails.length; i++) {
        //    if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.FormulaDes)) {
        //        $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].Id == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);
        //    } else {
        //        $scope.balanceSheetScheduling.FormulaDes = $scope.FormulaDetails[i].Id == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId;
        //    }
        //}
        $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.BalanceSheetSchedulingList = [];
    $scope.ShowBalanceSheetSchedulingPopUp = function () {
        $scope.Url = 'accounts/BalanceSheetScheduling/GetBalanceSheetSchedulingList?id=' + $scope.balanceSheetScheduling.Id;

        $http({
            method: 'Get',
            url: $scope.Url,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.BalanceSheetSchedulingList = response.data;
        });
        angular.element(document.querySelector('#BalanceSheetSchedulingPopUp')).modal('show');
    };

    $scope.SetBalanceSheetSchedulingData = function (obj) {
        $scope.balanceSheetScheduling.HeadIdFormula = obj.data.Id;
        angular.element(document.querySelector('#BalanceSheetSchedulingPopUp')).modal('hide');
    }

    $scope.closeBalanceSheetSchedulingPopUp = function () {
        angular.element(document.querySelector('#BalanceSheetSchedulingPopUp')).modal('hide');
    }


    $scope.SaveBalanceSheetScheduling = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.balanceSheetSchedulingForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrlBalanceSheetScheduling,
                data: { 'data': $scope.balanceSheetScheduling },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFieldsBalanceSheetScheduling();
                    $scope.getDataBalanceSheetScheduling();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        }

    };

    $scope.DeleteBalanceSheetScheduling = function () {
        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrlBalanceSheetScheduling + $scope.balanceSheetScheduling.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ClearFieldsBalanceSheetScheduling();
                    $scope.getDataBalanceSheetScheduling();
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
    $scope.ClearFieldsBalanceSheetScheduling = function () {
        $scope.ActionBalanceSheetScheduling = 'Save';
        $scope.balanceSheetScheduling = {
            Id: null,
            OptionNo: null,
            Type: null,
            GroupSequence: null,
            Group: null,
            SubGroupSequence: null,
            SubGroup: null,
            UserGroup: null,
            UserSubGroup: null,
            ItemSequence: null,
            ItemNo: null,
            Item: null,
            SubItemNo: null,
            SubItem: null,
            ScheduleNo: null,
            ScheduleName: null,
            UserItem: null,
            UserScheduleName: null,
            AddedBy: null,
            AddedDate: new Date(),
            AddedFromIP: null,
            UpdatedDate: null,
            IsCalculate: false,
            IsActive: false,
            FormulaDes: null
        };
    }
    $scope.message_Detailconfirmation = null;
    $scope.RemoveBalanceSheetScheduling = function () {
        if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Id))
            $scope.message_Detailconfirmation = 'Are you sure want to delete permanently';
        angular.element(document.querySelector('#confirmDetailPopUpLevel1')).modal('show');
    }
    //  #endregion BalanceSheetScheduling

    //  #region BalanceSheetScheduling Data Upload Download
    $scope.GetSampleFile = function () {
        var ReportFormat = 'Excel';
        location.href = $scope.pathBalanceSheetScheduling + 'GetSampleFile?reportFormat=' + ReportFormat;
    };
    $scope.BalanceSheetSchedulingUploadedData = [];
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
                    url: $scope.pathBalanceSheetScheduling + 'ImportData',
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
                        $scope.BalanceSheetSchedulingUploadedData = [];
                        $scope.BalanceSheetSchedulingUploadedData = response.data;
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
    $scope.saveBalanceSheetSchedulingUploadedData = function () {

        try {
            $.ajax({
                type: "POST",
                url: $scope.pathBalanceSheetScheduling + 'SaveBalanceSheetSchedulingUploadedData',
                data: {
                    'balanceSheetSchedulingUploadedDataList': $scope.BalanceSheetSchedulingUploadedData
                },
                dataType: "json",
                success: function (response) {
                    if (response.Error === true) {
                        $scope.ShowSaveBtn = true;
                        ShowResult(response.Message, 'failure');
                    }
                    else {
                        ShowResult(response.Message, 'success');
                        $scope.BalanceSheetSchedulingUploadedData = [];
                        $("#uploadImage").val(null);
                        $scope.ShowSaveBtn = false;
                    }

                }

            });

        } catch (e) {
            $scope.ShowSaveBtn = false;
            ShowResult(e, 'failure');

        }
    };
    //  #endregion BalanceSheetScheduling Data Upload Download

    $scope.OperatorList = [{ Text: "*", Value: "*" }, { Text: "/", Value: "/" }, { Text: "+", Value: "+" }, { Text: "-", Value: "-" }];
    $scope.FormulaDetails = [];
    $scope.SetFormula = function (formula) {
        try {
            var formulaObj = {};

            if (formula === 'SHead') {

                formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                formulaObj.balanceSheetSchedulingId = $scope.balanceSheetScheduling.Id == null ? null : $scope.balanceSheetScheduling.Id;
                formulaObj.balanceSheetSchedulingHeadId = $scope.balanceSheetScheduling.HeadIdFormula;
                formulaObj.SalaryHead = $("#HeadFormula option:selected").text();
                formulaObj.Component = null;
                $scope.FormulaDetails.push(formulaObj);

                $scope.balanceSheetScheduling.FormulaDes = '';
                $scope.balanceSheetScheduling.FormulaDescription = '';

                for (var i = 0; i < $scope.FormulaDetails.length; i++) {
                    if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.FormulaDes)) {
                        $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);
                    } else {
                        $scope.balanceSheetScheduling.FormulaDes = $scope.FormulaDetails[i].balanceSheetSchedulingHeadId;
                    }
                }

                $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;


            }
            else if (formula === 'Operator') {
                if ($scope.FormulaDetails.length != 0) {
                    if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Operator)) {


                        formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                        formulaObj.balanceSheetSchedulingId = $scope.balanceSheetScheduling.Id == null ? null : $scope.balanceSheetScheduling.Id;
                        formulaObj.balanceSheetSchedulingHeadId = null;
                        formulaObj.Component = $scope.balanceSheetScheduling.Operator;
                        formulaObj.SalaryHead = $scope.balanceSheetScheduling.Operator;;

                        $scope.FormulaDetails.push(formulaObj);

                        $scope.balanceSheetScheduling.FormulaDes = '';

                        $scope.balanceSheetScheduling.FormulaDescription = '';
                        $scope.balanceSheetScheduling.FormulaIDDescription = '';

                        for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                            $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);

                        }

                        $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;

                    }
                }
                else {
                    throw "First select Head or input value.";
                }

            }
            else if (formula === 'Precedence') {


                if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Precedence)) {


                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.balanceSheetSchedulingId = $scope.balanceSheetScheduling.Id == null ? null : $scope.balanceSheetScheduling.Id;
                    formulaObj.balanceSheetSchedulingHeadId = null;
                    formulaObj.SalaryHead = $scope.balanceSheetScheduling.Precedence;
                    formulaObj.Component = $scope.balanceSheetScheduling.Precedence;
                    $scope.FormulaDetails.push(formulaObj);


                    $scope.balanceSheetScheduling.FormulaDes = '';

                    $scope.balanceSheetScheduling.FormulaDescription = '';
                    $scope.balanceSheetScheduling.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);

                    }

                    $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;

                }


            }

            else if (formula === 'Value') {

                if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.Value)) {

                    formulaObj.Sequence = $scope.FormulaDetails.length + 1;
                    formulaObj.balanceSheetSchedulingId = $scope.balanceSheetScheduling.Id == null ? null : $scope.balanceSheetScheduling.Id;
                    formulaObj.balanceSheetSchedulingHeadId = null;
                    formulaObj.SalaryHead = $scope.balanceSheetScheduling.Value;
                    formulaObj.Component = $scope.balanceSheetScheduling.Value;
                    $scope.FormulaDetails.push(formulaObj);

                    $scope.balanceSheetScheduling.FormulaDes = '';

                    $scope.balanceSheetScheduling.FormulaDescription = '';
                    $scope.balanceSheetScheduling.FormulaIDDescription = '';

                    for (var i = 0; i < $scope.FormulaDetails.length; i++) {

                        $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);

                    }

                    $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;

                }
            }
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.RemoveFormula = function () {

        var maxseq = Math.max.apply(Math, $scope.FormulaDetails.map(function (o) { return o.Sequence; }))

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (maxseq === $scope.FormulaDetails[i].Sequence) {
                $scope.FormulaDetails.splice(i, 1);
                break;
            }
        }

        $scope.balanceSheetScheduling.FormulaDes = '';

        $scope.balanceSheetScheduling.FormulaDescription = '';

        for (var i = 0; i < $scope.FormulaDetails.length; i++) {
            if (!baseService.isUndefinedOrNull($scope.balanceSheetScheduling.FormulaDes)) {
                $scope.balanceSheetScheduling.FormulaDes += ' ' + ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);
            } else {
                $scope.balanceSheetScheduling.FormulaDes = ($scope.FormulaDetails[i].balanceSheetSchedulingHeadId == null ? $scope.FormulaDetails[i].Component : $scope.FormulaDetails[i].balanceSheetSchedulingHeadId);
            }
        }

        $scope.balanceSheetScheduling.FormulaDescription = $scope.balanceSheetScheduling.FormulaDes;

    }
}