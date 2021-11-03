'use strict';
GLItemController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'toaster', '$compile'];
function GLItemController(cboService, commonMessage, $scope, $rootScope, baseService, $http, $filter, toaster, $compile) {
    $rootScope.title = 'GL Item';
    $scope.Action = 'Save';
    $scope.path = 'accounts/getglcompanyinfolist/';
    $scope.saveUrl = $scope.path + 'create';
    $scope.glItems = [];
    $scope.glinfo = {
        Id: null,
        COAId: null,
        COALevel1Id: null,
        COALevel2Id: null,
        COALevel3Id: null,
        COALevel4Id: null,
        COALevel5Id: null,
        COALevel6Id: null,
        Sequence: null,
        AccountCode: null,
        RefNo: null,
        VoucherType: null,
        AccountGroupId: null,
        IsPostingAutomaticOnly: false,
        IsManufacturing: false,
        IsService: false,
        IsTreding: false,
        IsClearingAccount: false,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.glinfoNew = Object.assign({}, $scope.glinfo);
    $scope.newGlcominfo = [];
    $scope.glcominfo = {
        Id: null,
        Sequence: null,
        GLGeneralInfoId: null,
        CompanyId: null,
        CurrencyId: null,
        TaxCategory: null,
        PostingWithoutTaxAllow: true,
        AlternativeGL: null,
        AlternativeCOAId: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedDate: null
    };

    $scope.searchByGlList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Account Code',
            'value': 'AccountCode'
        },
        {
            'name': 'GL',
            'value': 'UserName'
        },
        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        }
    ];

    $scope.glaccounttype = [{
        Id: null,
        GLGeneralInfoId: null,
        AccountType: null,
        Active: true,
        AddedBy: null,
        AddedDate: new Date(),
        AddedFromIP: null,
        UpdatedBy: null,
        UpdatedDate: null,
        UpdatedFromIP: null
    }];
    $scope.member = {
        states: [{ id: "AL" }]
    };
    $scope.selected_items = [];
    $scope.AccountGroupList = [];
    $scope.GlCompanyInfoList = [];
    $scope.CurrenyList = [];
    $scope.AlternativeGLCboList = [];
    $scope.cOAList = [];

    $scope.LengthAccountCode = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/coa/GetGLLengthCbo?id=' + item
        }).then(function successCallback(response) {
            $scope.maxLength = response.data[0];
        });
    };

    $scope.onCOAChange = function (item) {
        $scope.paginationShow = true;
        $scope.checkIsLevelMandatory(item);

        if (item) {
            $scope.selectedCoaId = item;
        }
        $scope.LengthAccountCode(item);

        baseService.init('accounts/glitem/getglgeneralinfolist?COAId=' + item, null, null, null, "Sequence", "AccountCode");
        $scope.getData = function (pageno) {
            baseService.pagination(pageno)
                .then(function (result) {
                    $scope.glItems = result.Rows;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getData();
        ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
    };

    $scope.onAGroupCOAChange = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/accountgroup/getaccountgroupcoawiselistcbo?COAId=' + item
        }).then(function successCallback(response) {
            $scope.AccountGroupList = response.data;
        });
    };

    $http({
        method: 'GET',
        url: 'accounts/alternativegl/getcbo'
    }).then(function successCallback(response) {
        $scope.AlternativeGLCboList = response.data;
    });

    cboService.getCboChartOfAccount('', function (result) {
        $scope.cOAList = result;
    });

    $scope.checkIsLevelMandatory = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/coa/checklevelmandatory?coaid=' + id
        }).then(function successCallback(response) {
            $scope.checkLevelIsMandatory = response.data;
            if ($scope.checkLevelIsMandatory) {
                COARelationship(id);
            }
        });
    };

    $scope.checkIsLevelMandatoryUpdate = function (id) {
        $http({
            method: 'GET',
            url: 'accounts/coa/checklevelmandatory?coaid=' + id
        }).then(function successCallback(response) {
            $scope.checkLevelIsMandatory = response.data;
        });
    };

    $scope.initaccountype = function () {
        $scope.AccountTypeList = [
            {
                "AccountType": "Asset",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            },
            {
                "AccountType": "Customer",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            },
            {
                "AccountType": "Vendor",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            },
            {
                "AccountType": "Employee",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            },
            {
                "AccountType": "Material",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            },
            {
                "AccountType": "Tax",
                "Id": null,
                "GLGeneralInfoId": null,
                "Active": false
            }];
    };
    $scope.initaccountype();

    $scope.GetGLAccountTypeByGLId = function (glid) {
        $scope.initaccountype();
        $http({
            method: 'GET',
            url: 'accounts/glitem/getglaccounttypebyglid?glid=' + glid
        }).then(function successCallback(response) {
            var result = response.data.Rows;
            angular.forEach($scope.AccountTypeList, function (id, index) {
                for (var i = 0, len = result.length; i < len; i++) {
                    if (result[i].AccountType === id.AccountType) {
                        $scope.AccountTypeList[index] = result[i];
                        break;
                    }
                }
            });
        });
    };

    $scope.GetAccountTypeList = function () {
        angular.element(document.querySelector('#accountTypePopUp')).modal('show');
    };

    $scope.closeAccountTypePopUp = function () {
        angular.element(document.querySelector('#accountTypePopUp')).modal('hide');
    };

    $scope.removeRow = function () {
        angular.element(document.querySelector('#alternativeGlPopUp')).modal('hide');
    };

    $scope.AccountGroupNumberChange = [];
    $scope.GetAccountGroupNumberChange = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/accountgroup/getaccountgroupnumberchange?accountGroupId=' + item
        }).then(function successCallback(response) {
            $scope.FromNumberRange = response.data.Rows[0]['FromNumberRange'];
            $scope.ToNumberRange = response.data.Rows[0]['ToNumberRange'];
        });
    };

    $scope.checkAccountCode = function (code) {
        $scope.accCodeValid = true;
        if ((code > $scope.FromNumberRange) && (code < $scope.ToNumberRange)) {
            $scope.accCodeValid = false;
        }
        $scope.showMsg = "Account Code must be between " + $scope.FromNumberRange + " to" + $scope.ToNumberRange;
    };

    $scope.onCOAChangeSequence = function (item) {
        $http({
            method: 'GET',
            url: 'accounts/glitem/getautosequence?coaid=' + item
        }).then(function successCallback(response) {
            $scope.glinfoNew.Sequence = response.data;
        });
    };

    //$scope.GetGlComInfoSequence = function () {
    //    $http.get('accounts/glitem/getglcominfosequence')
    //        .then(function (response) {
    //            $scope.glcominfo.Sequence = response.data;
    //        });
    //}

    //$scope.GetGlComInfoSequence();
    $scope.glaccounttypemodel = [];
    $scope.checkGLItemNull = function () {
        if ($scope.glinfo.UserName == null) {
            $scope.pop('error', 'GL can\'t be null');
            return false;
        } else {
            return true;
        }
    };

    function CheckExists(id, list) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].CompanyId == id) {
                return true;
            }
        }
        return false;
    }
    $scope.Save = function () {
        angular.copy($scope.glinfoNew, $scope.glinfo);
        $scope.glaccounttypies = [];
        angular.forEach($scope.AccountTypeList, function (a) {
            $scope.glaccounttypies.push(
                {
                    AccountType: a.AccountType,
                    Id: a.Id,
                    GLGeneralInfoId: a.GLGeneralInfoId,
                    Active: a.Active
                }
            );
        });

        if ($scope.checkGLItemNull()) {
            $scope.$broadcast('show-errors-check-validity');
            reDirectToRequiredForm();
            if ($scope.glinfoNewForm1.$valid && $scope.glinfoNewForm2.$valid) {
                if ($scope.Action == 'Save' && !$scope.accCodeValid) {
                    $http({
                        method: 'POST',
                        url: 'accounts/glitem/create',
                        data: {
                            'glGeneralInfo': $scope.glinfo,
                            'glAccountType': $scope.glaccounttypies
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            baseService.paginationAdd();
                            $scope.getData();
                            ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
                        }
                    });
                    return true;
                }
                else if ($scope.Action == 'Update') {
                    $http({
                        method: 'POST',
                        url: 'accounts/glitem/edit',
                        data: {
                            'glGeneralInfo': $scope.glinfo,
                            'glAccountType': $scope.glaccounttypies
                        },
                        dataType: 'JSON'
                    }).then(function successCallback(response) {
                        if (response.data.Error == true) {
                            ShowResult(response.data.Message, 'failure');
                        }
                        else {
                            ShowResult(response.data.Message, 'success');
                            if ($scope.index > -1) {
                                $scope.glItems[$scope.index] = $scope.glinfo;
                            }
                            $scope.getData();
                            ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
                        }
                    }, function errorCallback(response) {
                        ShowResult(response.status.Message, 'failure');
                    });
                    return true;
                }
            }
        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.glinfo.Id)) {
            $http({
                method: 'POST',
                url: "accounts/glitem/delete/" + $scope.glinfo.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, "failure");
                }
                else {
                    ShowResult(response.data.Message, "success");
                    $scope.glItems.splice($scope.index, 1);
                    baseService.paginationAdd();
                    $scope.getData();
                    ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
                }
            }, function errorCallback(response) {
                ShowResult(response.status.Message, "failure");
            });
        }
        else {
            ShowResult(commonMessage.primaryKeyNullMessage, "failure");
        }
        return true;
    };

    $scope.rowSelected = null;
    $scope.alterNativeShow = [];
    $scope.setSelected = function (x) {
        $scope.rowSelected = x.Id;
        $scope.GlCompanyInfoList[$scope.setIndex].AccountCode = x.AccountCode;
        $scope.GlCompanyInfoList[$scope.setIndex].UserName = x.UserName;
        $scope.setIndex = -1;
        $scope.getData();
    };

    $scope.GetGLCompanyInfoListByGLId = function (glid) {
        $http({
            method: 'GET',
            url: 'accounts/glitem/getglcompanyinfolistbyglid?glid=' + glid
        }).then(function successCallback(response) {
            $scope.GlCompanyInfoList = response.data.Rows;
            $scope.selectedCoaId = $scope.glinfo.COAId;
        });
    };

    $scope.index = -1;
    $scope.Get = function (id, index, COAId) {
        $scope.index = index;
        $scope.glinfo = $scope.glItems[$scope.index];
        $scope.glinfoNew = Object.assign({}, $scope.glinfo);
        $scope.glinfo.AddedDate = $filter('dateFilter')($scope.glinfo.AddedDate);
        $scope.glinfo.UpdatedDate = $filter('dateFilter')($scope.glinfo.UpdatedDate);
        $scope.Action = "Update";
        $scope.GetGLAccountTypeByGLId(id);
        $scope.checkIsLevelMandatoryUpdate($scope.glinfo.COAId);
        $scope.GetAccountGroupNumberChange($scope.glinfo.AccountGroupId);
        $scope.showMsg = null;
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.pop = function (type, msg) {
        toaster.pop({
            type: type,
            body: msg,
            timeout: 3000
        });
    };

    $scope.Clear = function () {
        ClearFields($scope.onCOAChangeSequence($scope.glinfoNew.COAId));
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.glinfoNew = { COAId: $scope.glinfoNew.COAId };
        $scope.glinfoNew.Sequence = seq;
        $scope.newGlcominfo = [];
        $scope.AccountTypeList = [];
        $scope.initaccountype();
        $scope.glinfoNew.Active = true;
        $scope.index = -1;
        $scope.showMsg = null;
    }

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
        console.log($scope.tab);
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    cboService.getCboChartOfAccountLevel1(function (result) {
        $scope.COALevel1List = result;
    });

    cboService.getCboChartOfAccountLevel2(function (result) {
        $scope.COALevel2List = result;
    });

    cboService.getCboChartOfAccountLevel3(function (result) {
        $scope.COALevel3List = result;
    });

    cboService.getCboChartOfAccountLevel4(function (result) {
        $scope.COALevel4List = result;
    });

    cboService.getCboChartOfAccountLevel5(function (result) {
        $scope.COALevel5List = result;
    });

    cboService.getCboChartOfAccountLevel6(function (result) {
        $scope.COALevel6List = result;
    });

    function reDirectToRequiredForm() {
        if ($scope.glinfoNewForm2.$invalid) {
            $scope.setTab(1);
        }
        else if ($scope.glinfoNewForm4.$invalid) {
            $scope.setTab(2);
        }
    }

    function COARelationship(item, data) {
        $scope.left = '';
        $scope.right = '';
        $http.get('accounts/chartofaccountrelationship/getformdata?coaid=' + item)
            .then(function (response) {
                var obj = response.data.Rows;
                angular.forEach(obj, function (obj, i) {
                    var colData = null;
                    if (data != null) {
                        colData = data['Col' + obj.Sequence];
                    }
                    if (i % 2 == 0) {
                        $scope.left += '<div class="form-group" show-errors>' +
                            '<label class="col-sm-5 control-label">' + obj.UserName + '<sup>*</sup></label>' +
                            '<div class="col-sm-7 show-message"><div class="select-style">' +
                            '<select ng-model="glinfoNew.' + obj.StandardName + 'Id" class="form-control" ng-options="item.Value as item.Text for item in ' + obj.StandardName + 'List" required name="' + obj.StandardName + '"><option value=""></option></select>' +
                            '</div></div>' +
                            '</div>';
                    }
                    else {
                        $scope.right += '<div class="form-group" show-errors>' +
                            '<label class="col-sm-5 control-label">' + obj.UserName + '<sup>*</sup></label>' +
                            '<div class="col-sm-7 show-message"><div class="select-style">' +
                            '<select ng-model="glinfoNew.' + obj.StandardName + 'Id" class="form-control" ng-options="item.Value as item.Text for item in ' + obj.StandardName + 'List" required name="' + obj.StandardName + '"><option value=""></option></select>' +
                            '</div></div>' +
                            '</div>';
                    }
                });
            });
    }

    $scope.maxLengthCheck = function (object) {
        if (object.value.length > object.maxLength)
            object.value = object.value.slice(0, object.maxLength);
    };

    $scope.isNumeric = function (evt) {
        var theEvent = evt || window.event;
        var key = theEvent.keyCode || theEvent.which;
        key = String.fromCharCode(key);
        var regex = /[0-9]|\./;
        if (!regex.test(key)) {
            theEvent.returnValue = false;
            if (theEvent.preventDefault) theEvent.preventDefault();
        }
    };

    $scope.selectMessage = '';
    $scope.glMasterReport = function (COAId) {
        if (COAId == null) {
            $scope.selectMessage = 'Select COA';
        }
        else {
            $scope.selectMessage = '';
            location.href = 'accounts/glitem/glmasterreport?COAId=' + COAId;
        }
    };
}