//#region Lib
'use strict';
ParameterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ParameterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Parameter';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Productions/Parameter/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.updateUrl = $scope.path + 'Update';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.HeaderList = [];
    //#endregion Lib

    //  #region Header

    //#region List object
    $scope.ModelTemp = {
        Id: null,
        Code: null,
        Sequence: 0,
        ShortName: null,
        StandardName: null,
        UserName: null,
        EmployeeName: null,
        EmpSystemId: null,
        BudgetCode: null,
        //ProcessId: null,
        //MachineMasterId: null,
        //ParameterId: null,
        //ProcessCategory: null,
        //CriticalLevel: null,
        //UOMId: null,
        //UOMName: null,
        //PeriodQuality: null,
        //Frequency: null,
        //QA: null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    // #region Save

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + 'Save',
                data: {
                    'datas': $scope.ModelNew

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
        // Parameter
        $scope.MachineMasterId = null;
        $scope.CreateParameter = function () {
            var params = {

                'ProcessId': $scope.ProcessId,
                'MachineMasterId': $scope.MachineMasterId,
                'ParameterId': $scope.ParameterId,
                'ProcessCategory': $scope.ProcessCategory,
                'CriticalLevel': $scope.CriticalLevel,
                'UOMId': $scope.UOMId,
                'CheckinPeriod': $scope.CheckinPeriod,
                'CheckinFrequency': $scope.CheckinFrequency,
                'AuditingDays': $scope.AuditingDays,
                'CheckinDays': $scope.CheckinDays
            }
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.path + 'CreateParameter',
                data: {
                    'headerid': $scope.ModelNew.Id,
                    parameter: params
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.GetList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        };

        // Product
        $scope.SaveProductList = [];
        $scope.CreateProductWithParameterSetup = function () {
            $scope.SaveList = [];
            for (var i = 0; i < $scope.ProductList.length; i++) {
                if ($scope.ProductList[i].Active) {
                    $scope.SaveProductList.push($scope.ProductList[i]);
                }
            }
            $scope.$broadcast('show-errors-check-validity');
            if (baseService.arrayLength($scope.SaveProductList) > 0) {
                $http({
                    method: 'POST',
                    url: $scope.path + 'CreateProductWithParameterSetup',
                    data: {
                        models: $scope.SaveProductList,
                        headerid: $scope.ModelNew.Id,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');

                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Data.Id;
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

        };

        // Workcenter
        $scope.SaveWorkcenterList = [];
        $scope.CreateWorkcenterWithParameterSetup = function () {
            $scope.SaveWorkcenterList = [];
            for (var i = 0; i < $scope.WorkcenterList.length; i++) {
                if ($scope.WorkcenterList[i].Active) {
                    $scope.SaveWorkcenterList.push($scope.WorkcenterList[i]);
                }
            }
            $scope.$broadcast('show-errors-check-validity');
            if (baseService.arrayLength($scope.SaveProductList) > 0) {
                $http({
                    method: 'POST',
                    url: $scope.path + 'CreateWorkcenterWithParameterSetup',
                    data: {
                        models: $scope.SaveWorkcenterList,
                        headerid: $scope.ModelNew.Id,
                    },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error === true) {
                        ShowResult(response.data.Message, 'failure');

                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        $scope.ModelNew.Id = response.data.Data.Id;
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }

        };

        // #endregion Save

        // #region    EmployeePop
        $scope.OpeEmployeePopUp = function () {
            angular.element(document.querySelector('#EmployeePop')).modal('show');
            $scope.GetResponsiblePerson();
        }
        $scope.closeEmployeePopUp = function () {
            angular.element(document.querySelector('#EmployeePop')).modal('hide');

        }
        $scope.EmployeeList = [];
        $scope.GetResponsiblePerson = function () {
            $http({
                method: 'GET',
                url: $scope.path + 'GetResponsiblePerson',
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.EmployeeList = resp.data;
            });

        }

        $scope.EmployeeId = null;
        $scope.Employee = null;
        $scope.doubleEmploye = function (e) {
            $scope.ModelNew.SystemId = e.data.SystemId;
            $scope.ModelNew.EmployeeName = e.data.EmployeeName;
            angular.element(document.querySelector('#EmployeePop')).modal('hide');
            /*$scope.viewFurniturePolicyGrids();*/
        }

        $scope.getResponsiblePersonId = function () {
            $http({
                method: 'POST',
                data: { 'ResponsiblePersonId': $scope.EmployeeId, },
                url: $scope.path + 'getResponsiblePersonId',
            }).then(function success(response) {
                $scope.ResponsiblePerson = JSON.stringify(response.data[0].EmployeeName.replace(/\"/g, ""));
                $scope.ResponsiblePerson = $scope.ResponsiblePerson.replace(/\"/g, "");

            });
        }
        // #endregion    EmployeePop

        //  #endregion Header

        // #region  Child

        // #region  Product
        $scope.OpeProductPopUp = function () {
            angular.element(document.querySelector('#ProductPopup')).modal('show');
            $scope.GetProduct();
        }
        $scope.closeProductPopUp = function () {
            angular.element(document.querySelector('#ProductPopup')).modal('hide');

        }
        $scope.ProductList = [];
        $scope.GetProduct = function () {
            $http({
                method: 'GET',
                url: $scope.path + 'GetProduct',
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.ProductList = resp.data;
            });

        }
        // #endregion  Product

        // #region  Machine
        $scope.OpenMachinePopUp = function () {
            angular.element(document.querySelector('#MachinePopup')).modal('show');
            $scope.GetMachine();
        }
        $scope.closeMachinePopUp = function () {
            angular.element(document.querySelector('#MachinePopup')).modal('hide');

        }
        $scope.MachineList = [];
        $scope.GetMachine = function () {
            $http({
                method: 'GET',
                url: $scope.path + 'GetMachine',
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.MachineList = resp.data;
            });

        }
        // #endregion  Machine

        // #region  Workcenter
        $scope.OpenWorkcenterPopUp = function () {
            angular.element(document.querySelector('#WorkcenterPopup')).modal('show');
            $scope.GetWorkcenter();
        }
        $scope.closeWorkcenterPopUp = function () {
            angular.element(document.querySelector('#WorkcenterPopup')).modal('hide');

        }
        $scope.WorkcenterList = [];
        $scope.GetWorkcenter = function () {
            $http({
                method: 'GET',
                url: $scope.path + 'GetWorkcenter',
                dataType: 'JSON'
            }).then(function succ(resp) {
                $scope.WorkcenterList = resp.data;
            });

        }
        // #endregion  Workcenter

        // #region TAB CHANGE
        $scope.tab = 1;
        $scope.setTab = function (newTab) {
            $scope.tab = newTab;
        };

        $scope.isSet = function (tabNum) {
            return $scope.tab === tabNum;
        };
        // #endregion TAB CHANGE

        //  #region Get Fun

        //  #region All Get
        $scope.getData = function () {
            $http.get('Productions/Parameter/GetList')
                .then(
                    function successCallback(response) {
                        $scope.ModelList = response.data;
                        ClearFields(response.data.Sequence);
                        $scope.GetSequence();
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        //$scope.getData();

        $scope.Get = function (args) {
            document.getElementById("savebtn").style.display = "none";
            document.getElementById("updatebtn").style.display = "block";
            $scope.ModelNew.UOMId = args.data.UOMId;
            $scope.ModelNew.UOMId = args.data.UOMName;
            $scope.ModelNew = Object.assign({}, args.data);
            $scope.EmployeeId = args.data.ResponsiblePerson;
            $scope.Action = 'Update';
            if (!$rootScope.isCollapsed) {
                $rootScope.toggle();

            }
        };

        //  #endregion All Get
        $scope.ProcessList = [];
        $scope.getProcess = function () {
            $http.get('Productions/ParameterMaster/GetProcess')
                .then(
                    function successCallback(response) {
                        $scope.ProcessList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        $scope.getProcess();

        $scope.ParameterMasterList = [];
        $scope.getParameterMaster = function () {
            $http.get('Productions/Parameter/GetParameterMaster')
                .then(
                    function successCallback(response) {
                        $scope.ParameterMasterList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        $scope.getParameterMaster();

        $scope.MachineMasterList = [];
        $scope.GetMachineMaster = function () {
            $http.get('Productions/Parameter/GetMachineMaster')
                .then(
                    function successCallback(response) {
                        $scope.MachineMasterList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        $scope.GetMachineMaster();

        $scope.GetList = function () {
            $http.get('Productions/Parameter/GetList')
                .then(
                    function successCallback(response) {
                        $scope.ModelList = response.data;
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        $scope.GetList();
        //  #endregion Get Fun

        //  #region UOM
        $scope.UOMList = [];
        $scope.getUOM = function () {
            $http({
                method: 'POST',
                url: 'HumanResource/MedicineMaster/getUOM',
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.UOMList = response.data;
            })
        }
        $scope.getUOM();

        $scope.doubleClkUOM = function (e) {
            $scope.UOMName = e.data.StandardName;
            $scope.UOMId = e.data.Id;
            $scope.closeUOMPopUp();
        }

        $scope.openUOMPopUp = function () {
            angular.element(document.querySelector('#UOMPopUpId')).modal('show');
        }

        $scope.closeUOMPopUp = function () {
            angular.element(document.querySelector('#UOMPopUpId')).modal('hide');
        }

        $scope.searchByUOM = "UserName";
        $scope.searchUM = "";

        $scope.UOMSearchByList = [
            {
                'name': 'User Name',
                'value': 'UserName'
            },
            {
                'name': 'Standard Name',
                'value': 'StandardName'
            }
        ];


        $scope.searchUOM = function () {
            $http({
                method: 'POST',
                url: $scope.path + 'searchUOM',
                data: { column: $scope.searchByUOM, value: $scope.searchUM },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.UOMList = response.data;
            });
        }
        //  #endregion UOM





        // #region  update
        $scope.Update = function () {
            $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.updateUrl,
                data: {
                    'data': $scope.ModelNew,
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');

                    $scope.GetList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        };
        // #endregion   update

        //  #region Delete
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
        //  #endregion Delete

        // #endregion  Child

        //#region BudgetCode

        $scope.name = null;
        $scope.popUpTitle = "Manpower Budget Information";
        $scope.popUpList = [];
        $scope.valueData = '';
        $scope.budgetpopUpParameters = {
            limit: 10,
            offset: 0,
            order: 'asc',
            sort: 'Code',
            searchBy: "Code",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUp = function () {

            $scope.popUpDataList = [];
            $scope.popUpList = [];
            $scope.budgetpopUpParameters.sort = 'Code';
            $scope.budgetpopUpParameters.searchBy = 'Code';
            $scope.popUpUrl = 'employees/recruitment/getbudgetcodelist';
            baseService.setCurrentPage('dataList');
            $scope.getPopUpData = function (pageno) {
                baseService.paginationBase($scope.popUpUrl, pageno, $scope.budgetpopUpParameters)
                    .then(function (result) {
                        $scope.popUpDataList = result.Rows;
                        $scope.budgetpopUpParameters.total_count = result.Total;
                        if (baseService.arrayLength($scope.popUpList) === 0) {
                            baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        }
                        //$scope.popUpParameters.sort = 'Code';
                        //$scope.popUpParameters.searchBy = 'Code';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#popUpId')).modal('show');
            $scope.getPopUpData();
        };

        $scope.selectDoubleClick = function (data) {
            try {
                $scope.ModelNew.BudgetCode = data.Id;
                $scope.ModelNew.Code = data.Code;
                angular.element(document.querySelector('#popUpId')).modal('hide');


            } catch (e) {
                ShowResult(e, 'failure');
            }
        };

        $scope.clearCode = function () {
            $scope.ModelNew.BudgetCode = null;
            $scope.ModelNew.Code = null;

        };

        $scope.GetOnRollByBudget = function (budgetId) {
            try {
                $http.get('employees/EmployeeInformation/GetOnRollByBudget?budgetId=' + budgetId)
                    .then(function (response) {
                        if (response.data[0].TotalNumber < response.data[0].OnRollManPwr || response.data[0].TotalNumber == response.data[0].OnRollManPwr) {
                            ShowResult("On Roll Manpower is exceeding Budgeted Manpower.", 'failure', 'popUpId');;
                        }
                        else {
                            angular.element(document.querySelector('#popUpId')).modal('hide');
                        }
                    });
            } catch (e) {
                ShowResult(e, 'failure');
            }
        };

        //#endregion BudgetCode
  }
