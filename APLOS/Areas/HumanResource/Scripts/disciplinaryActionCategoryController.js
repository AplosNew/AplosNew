'use strict';
disciplinaryActionCategoryController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function disciplinaryActionCategoryController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Disciplinary Action Category';
    $scope.Action = 'Save';
    $scope.path = 'humanresource/disciplinaryActionCategory/';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.saveDetailsUrlD = $scope.path + 'SaveD';
    $scope.saveChildUrlD = $scope.path + 'SaveC';
    $scope.saveMUrl = $scope.path + 'SaveM';
    
    $scope.ModelList = [];
    $scope.getData = function () {
        $http.get('humanresource/disciplinaryActionCategory/GetList')
            .then(function (response) {
                $scope.ModelList = response.data;
            });
    };
    $scope.getData();

    $scope.ModelSequenceList = [];
    $scope.getSequence = function () {
        $http.get('humanresource/disciplinaryActionCategory/GetAutoSequence')
            .then(function (response) {
                $scope.disciplinaryActionCategoryNew.Sequence = response.data;
            });
    };
    $scope.getSequence();
    
    $scope.DeleteMaster = function () {
        if (!baseService.isUndefinedOrNull($scope.disciplinaryActionCategoryNew.Id)) {
            $http.get('humanresource/disciplinaryActionCategory/Delete?Id=' + $scope.disciplinaryActionCategoryNew.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.disciplinaryActionCategoryNew = Object.assign({}, $scope.disciplinaryActionCategory);
                        $scope.getData();
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };


    $scope.DeleteChild = function () {
        if (!baseService.isUndefinedOrNull($scope.ShowCauseLetterChildModel.Id)) {
            $http.get('humanresource/disciplinaryActionCategory/DeleteChild?Id=' + $scope.ShowCauseLetterChildModel.Id)
                .then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.GetChildData($scope.sigId);
                    }
                    function errorCallBack(response) {
                        ShowResult(response.data.Message, 'failure');
                    }
                });
        }
    };


    $scope.ModelChildList = [];
    $scope.GetChildData = function (sigId) {
        $scope.ShowCauseLetterChildModel = {
            //Id: null,
            DisciplinaryActionSettingDetailsId: null,
            DisciplinaryActionCategoryId: null,
            LetterFormat: null,
            LetterName: null,
            LetterLanguage: null,
        };

        $http.get('humanresource/disciplinaryActionCategory/GetChildData?DetailsId=' + sigId)
            .then(function (response) {
                $scope.ModelChildList = response.data;
            });
    };

    $scope.recorddoubleclick = function () {
        var gridObj = $("#GridDiciplinaryActionCategory").data("ejGrid");
        $scope.disciplinaryActionCategoryNew = gridObj.getSelectedRecords()[0];
        try {
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();
            }
        } catch (e) {
        }
        $scope.getData();
        $scope.getShowCauseLetter();
        $scope.disciplinaryActionCategoryNew.IsShowCaseLetter = false;
    };

    $scope.recorddoubleclickChild = function () {
        var gridObj = $("#GridDiciplinaryActionCategoryChild").data("ejGrid");
        $scope.ShowCauseLetterChildModel = gridObj.getSelectedRecords()[0];
    };

    $scope.ShowCauseLetterList = [];
    $scope.getShowCauseLetter = function () {
        $http.get('humanresource/disciplinaryActionCategory/GetShowCaseLetter?DisciplinaryActionCategoryId=' + $scope.disciplinaryActionCategoryNew.Id)
            .then(function (response) {
                $scope.ShowCauseLetterList = response.data;
            });
    };


    $scope.disciplinaryActionCategory = {
        Id: null,
        Sequence: null,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,

        Remarks: null,
        Active: true,
        IsShowCaseLetter: false,
    };
    $scope.disciplinaryActionCategoryNew = Object.assign({}, $scope.disciplinaryActionCategory);

    $scope.ShowCauseLetterDetailsModel = {
        Id: null,
        Sequence: 0,
        DisciplinaryActionCategoryId: null,
        LetterIssueDay: null,
        IsSeparable: false,
        IsActive: true,
        Description: null,

    };

    $scope.ShowCauseLetterChildModel = {
        Id: null,
        DisciplinaryActionSettingDetailsId: null,
        DisciplinaryActionCategoryId: null,
        LetterFormat: null,
        LetterName: null,
        LetterLanguage: null,
        IsDefault: false,
        Active: true,
    };

    $scope.ShowDiv = false;
    $scope.AddNew = function () {
        try {
            $scope.ShowDiv = true;
            var eDialog = $("#DisciplinaryActionInfo").data("ejDialog");
            eDialog.open();
            $scope.ShowCauseLetterDetailsModel = {};
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.disciplinaryActionCategoryNew = Object.assign({}, $scope.disciplinaryActionCategory);
    
    function CheckField(fieldname, field) {
        try {
            if (baseService.isUndefinedOrNull(field)) {
                throw "[" + fieldname + "] can not be blank...";
            }
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationMaster() {
        try {
            CheckField("Code", $scope.disciplinaryActionCategoryNew.Code);
            CheckField("Short Name", $scope.disciplinaryActionCategoryNew.ShortName);
            CheckField("Standard Name", $scope.disciplinaryActionCategoryNew.StandardName);
            CheckField("User Name", $scope.disciplinaryActionCategoryNew.UserName);
        } catch (ex) {
            throw ex;
        }
    };

    function ValidationDetails() {
        try {
            CheckField("Letter Issue Day", $scope.ShowCauseLetterDetailsModel.LetterIssueDay);
            CheckField("Description", $scope.ShowCauseLetterDetailsModel.Description);

        } catch (ex) {
            throw ex;
        }
    };

    function ValidationChild() {
        try {
            CheckField("Letter Format", $scope.ShowCauseLetterChildModel.LetterFormat);
            CheckField("Letter Language", $scope.ShowCauseLetterChildModel.LetterLanguage);
            CheckField("Letter Name", $scope.ShowCauseLetterChildModel.LetterName);

        } catch (ex) {
            throw ex;
        }
    };
    
    $scope.MasterId = null;
    $scope.SaveMaster = function () {
        try {
            ValidationMaster();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveMUrl,
                data: { 'Master': $scope.disciplinaryActionCategoryNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.disciplinaryActionCategoryNew.Id = response.data.MasterId;
                    $scope.getData();
                    $scope.getShowCauseLetter();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.saveDetails = function () {
        try {
            ValidationDetails();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveDetailsUrlD,
                data: { 'Details': $scope.ShowCauseLetterDetailsModel, 'MasterId': $scope.disciplinaryActionCategoryNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ShowCauseLetterDetailsModel.Id = response.data.DetailsId;
                    $scope.getShowCauseLetter();
                    $scope.ShowCauseLetterDetailsModel = {
                        Id: null,
                        Sequence: 0,
                        DisciplinaryActionCategoryId: null,
                        LetterIssueDay: null,
                        IsSeparable: false,
                        IsActive: true,
                        Description: null,

                    };
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.saveChild = function () {
        try {
            ValidationChild();
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveChildUrlD,
                data: { 'Child': $scope.ShowCauseLetterChildModel, 'DetailsId': $scope.ShowCauseLetterDetailsModel.Id, 'MasterId': $scope.disciplinaryActionCategoryNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetChildData($scope.sigId);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    
    $scope.LanguageList = [];
    $scope.getLanguage = function () {
        $http.get('humanresource/disciplinaryActionCategory/GetLanguage')
            .then(function (response) {
                $scope.LanguageList = response.data;
            });
    };
    $scope.getLanguage();

    $scope.AddDetails = function (data, index) {
        try {
            $scope.sigId = data.Id;
            $scope.Description = data.Description;
            $scope.ShowCauseLetterDetailsModel.Id = $scope.sigId;
            $scope.GetChildData($scope.sigId);
            $scope.ShowCauseLetterChildModel.DisciplinaryActionSettingDetailsId = $scope.sigId;
            var eDialog = $("#DisciplinaryActionChildInfo").data("ejDialog");
            eDialog.open();
            $scope.ShowCauseLetterChildModel = {};

        } catch (e) {
            ShowResult(e, "failure");
        }

    };

    $scope.Edit = function (data, index) {
        $scope.sigId = data.Id;
        $scope.cindex = index;
        $scope.ShowCauseLetterDetailsModel.Id = $scope.sigId;
        $scope.ShowDiv = true;
        var eDialog = $("#DisciplinaryActionInfo").data("ejDialog");
        eDialog.open();
        $scope.ShowCauseLetterDetailsModel.Id = data.Id;
        $scope.ShowCauseLetterDetailsModel.Sequence = data.Sequence;
        $scope.ShowCauseLetterDetailsModel.DisciplinaryActionCategoryId = data.DisciplinaryActionCategoryId;
        $scope.ShowCauseLetterDetailsModel.LetterIssueDay = data.LetterIssueDay;
        $scope.ShowCauseLetterDetailsModel.IsSeparable = data.IsSeparable;
        $scope.ShowCauseLetterDetailsModel.IsActive = data.IsActive;
        $scope.ShowCauseLetterDetailsModel.Description = data.Description;
    };

    $scope.removePopup = function (data, index) {
        $scope.sigId = data.Id;
        $scope.cindex = index;
        $scope.ShowCauseLetterDetailsModel.Id = $scope.sigId;
        $scope.message = 'Are you sure want to permanent delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.sigId)) {
            if (!baseService.isUndefinedOrNull($scope.ShowCauseLetterDetailsModel.Id)) {
                $http.get('humanresource/disciplinaryActionCategory/DeleteDetails?Id=' + $scope.ShowCauseLetterDetailsModel.Id)
                    .then(function successCallback(response) {
                        if (response.data.Error === true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            $scope.getShowCauseLetter();

                        }
                        function errorCallBack(response) {
                            ShowResult(response.data.Message, 'failure');
                        }
                    });
            }
        };
    }

    $scope.ClearM = function (obj) {
        $scope.disciplinaryActionCategoryNew = Object.assign({}, $scope.disciplinaryActionCategory);
        $scope.ShowCauseLetterList = [];
        $scope.disciplinaryActionCategoryNew.IsShowCaseLetter = false;

    };

    $scope.ClearC = function () {
        $scope.ShowCauseLetterChildModel = {};
    };

}