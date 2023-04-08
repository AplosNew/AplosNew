//#region Lib
'use strict';
ParameterMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function ParameterMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Parameter Master';
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

    // #region TAB CHANGE
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };
        // #endregion TAB CHANGE

    //#region List object
    $scope.ModelTemp = {
        Id: null,
        Code: null,
        StandardName: null,
        UserName: null,
        EmployeeName: null,
        EmpSystemId: null,
        BudgetCode: null,
        BudgetCodeId:null,
        Remarks: null
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
    //#endregion List object

    //  #region Header

    $scope.Get = function (args) {
        //document.getElementById("savebtn").style.display = "none";
        //document.getElementById("updatebtn").style.display = "block";
        
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EmployeeId = args.data.ResponsiblePerson;
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $scope.GetSavedParameterChild();
            $scope.GetSavedEntity();
            $scope.GetSavedProduct();
            $scope.GetSavedWorkcenter();
            $scope.GetParameterEntity();
            $scope.GetSavedProcess();
            $scope.GetSavedMachine();
            $scope.GetQualityProcess()
            $rootScope.toggle();

        }
    };
        // #region Save

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.path + $scope.Action,
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
                'CheckinDays': $scope.CheckinDays,
                'Remarks': $scope.Remarks
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
                   /* $scope.ModelNew.Id = response.data.Data.Id*/
                    $scope.GetList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        };

        // Product
        $scope.SaveProductList = [];
        $scope.CreateProductWithParameterSetup = function () {
            $scope.SaveProductList = [];
            if (baseService.arrayLength($scope.ProductList) > 0) {
                angular.forEach($scope.ProductList, function (a) {

                    if (a.chk) {
                        var ob = {};

                        ob.ProductMasterId = a.Id;
                        ob.Id = null;
                       
                        $scope.SaveProductList.push(ob);
                        ob = {};
                        a.chk = false;
                    }


                });
            }

            $scope.$broadcast('show-errors-check-validity');
            /*if (baseService.arrayLength($scope.SaveProductList) > 0) {*/
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
                        $scope.closeProductPopUp();
                        //$scope.ModelNew.Id = response.data.Data.Id;
                        $scope.GetSavedProduct();
                        $scope.SaveProductList = [];
                       
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
          /*  }*/

    };


        // Workcenter
        $scope.SaveWorkcenterList = [];
    $scope.CreateWorkcenterWithParameterSetup = function () {
        $scope.SaveWorkcenterList = [];
            if (baseService.arrayLength($scope.WorkcenterList) > 0) {
                angular.forEach($scope.WorkcenterList, function (a) {

                    if (a.chk) {
                        var ob = {};

                        ob.WorkcenterId = a.Id;
                        ob.Id = null;

                        $scope.SaveWorkcenterList.push(ob);

                        ob = {};
                        a.chk = false;
                    }


                });
            }
            $scope.$broadcast('show-errors-check-validity');
           /* if (baseService.arrayLength($scope.SaveProductList) > 0) {*/
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
                        $scope.closeWorkcenterPopUp();
                        //$scope.ModelNew.Id = response.data.Data.Id;
                        $scope.GetSavedWorkcenter();
                        $scope.SaveWorkcenterList = [];
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
           /* }*/

    };

    

        // #endregion Save

        // #region  update
    $scope.Update = function () {
        //$scope.$broadcast('show-errors-check-validity');
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
    $scope.RemoveParticularRow = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveProduct?productid=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedProduct();
                $scope.getData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.RemoveWorkcenterRow = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveWorkcenter?workcenterid=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedWorkcenter();
                $scope.getData();
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }

    $scope.Delete = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveParameterRow?parameterid=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedParameterChild();
               /* $scope.getData();*/
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
        //  #endregion Delete

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
        $scope.ModelNew.EmpSystemId = e.data.EmpSystemId;
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

        //  #region Get Fun
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
            url: $scope.path + 'GetWorkcenter?paramEntityId=' + $scope.paramEntityId,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.WorkcenterList = resp.data;
        });

    }
        // #endregion  Workcenter

        //  #region All Get
        $scope.getData = function () {
            $http.get('Productions/Parameter/GetList')
                .then(
                    function successCallback(response) {
                        $scope.ModelList = response.data;
                       // ClearFields(response.data.Sequence);
                       
                    },
                    function errorCallback(response) {
                        ShowResult(response, 'failure');
                    });
        }
        
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

        //  #endregion All Get

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
            $scope.ModelNew.BudgetCodeId = data.Id;
            $scope.ModelNew.BudgetCode = data.Code;
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

    $scope.ParameterWorkcenterList = [];
    $scope.GetSavedWorkcenter = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedWorkcenter?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ParameterWorkcenterList = resp.data;
        });
    }

    $scope.ParameterProductList = [];
    $scope.GetSavedProduct = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedProduct?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            
            $scope.ParameterProductList = resp.data;
        });
    }

    $scope.SavedParameterList = [];
    $scope.GetSavedParameterChild = function () {
       
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedParameterChild?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.Id = resp.data[0].Id;
             $scope.MachineMasterId = resp.data[0].MachineMasterId;
            $scope.ParameterId = resp.data[0].ParameterId;
            $scope.ProcessCategory = resp.data[0].ProcessCategory;
            $scope.CriticalLevel = resp.data[0].CriticalLevel;
            $scope.UOMId = resp.data[0].UOMId;
            $scope.UOMName = resp.data[0].UOMName;
            $scope.CheckinPeriod = resp.data[0].CheckinPeriod;
            $scope.CheckinFrequency = resp.data[0].CheckinFrequency;
            $scope.ProcessId = resp.data[0].ProcessId;
            $scope.Process = resp.data[0].Process;
            $scope.AuditingDays = resp.data[0].AuditingDays;
            $scope.CheckinDays = resp.data[0].CheckinDays;
            $scope.Remarks = resp.data[0].Remarks;
            $scope.SavedParameterList = resp.data
                //Object.assign({}, resp.data);
        });
    }

    $scope.GetParameter = function (args) {
       
        $scope.ProcessId = args.data.ProcessId;
        $scope.MachineMasterId = args.data.MachineMasterId;
        $scope.ParameterId = args.data.ParameterId;
        $scope.ProcessCategory = args.data.ProcessCategory;
        $scope.CriticalLevel = args.data.CriticalLevel;
        $scope.UOMId = args.data.UOMId;
        $scope.UOMName = args.data.UOMName;
        $scope.CheckinPeriod = args.data.CheckinPeriod;
        $scope.CheckinFrequency = args.data.CheckinFrequency;
       
        $scope.AuditingDays = args.data.AuditingDays;
        $scope.CheckinDays = args.data.CheckinDays;
        $scope.Remarks = args.data.Remarks;
       
        //if (!$rootScope.isCollapsed) {
           
        //}
    }
        //  #endregion Get Fun

        // #endregion  Child

        //  #region Clear
    $scope.ClearParameter = function () {
        ClearParameterFields();
        return true;
    };

    function ClearParameterFields() {
        $scope.Action = 'Save';
       
        $scope.ProcessId = null;
        $scope.MachineMasterId = null;
        $scope.ParameterId = null;
        $scope.ProcessCategory = null;
        $scope.CriticalLevel = null;
        $scope.UOMId = null;
        $scope.UOMName = null;
        $scope.CheckinPeriod = null;
        $scope.CheckinFrequency = null;

        $scope.AuditingDays = null;
        $scope.CheckinDays = null;
        $scope.Remarks = null;

    }

    $scope.Clear = function () {
        ClearFields();
        return true;
    };
    function ClearFields() {
        $scope.Action = 'Save';

        $scope.ModelTemp = {
            Id: null,
            Code: null,
            StandardName: null,
            UserName: null,
            EmployeeName: null,
            EmpSystemId: null,
            BudgetCode: null,
            BudgetCodeId: null,
            Remarks: null
        };
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    }
        //  #endregion Clear

    //  #region Entity Tab
    $scope.OpenEntityPopUp = function () {
        angular.element(document.querySelector('#EntityPopupId')).modal('show');
        $scope.GetEntity();
    }
    $scope.closeEntityPopUp = function () {
        angular.element(document.querySelector('#EntityPopupId')).modal('hide');

    }
    $scope.EntityList = [];
    $scope.GetEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetEntity',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.EntityList = resp.data;
        });

    }
    

    $scope.SaveEntityList = [];
    $scope.CreateEntityWithParameterSetup = function () {
        $scope.SaveEntityList = [];
        if (baseService.arrayLength($scope.EntityList) > 0) {
            angular.forEach($scope.EntityList, function (a) {

                if (a.chk) {
                    var ob = {};

                    ob.EntityId = a.EntityId;
                    ob.Id = null;

                    $scope.SaveEntityList.push(ob);
                    ob = {};
                    a.chk = false;
                }
            });
        }

       
        $http({
            method: 'POST',
            url: $scope.path + 'CreateEntityWithParameterSetup',
            data: {
                models: $scope.SaveEntityList,
                headerid: $scope.ModelNew.Id,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.closeProductPopUp();
                //$scope.ModelNew.Id = response.data.Data.Id;
                $scope.GetSavedEntity();
                $scope.SaveEntityList = [];

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
       
    };

    $scope.ParameterEntityList = []
    $scope.GetSavedEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedEntity?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {

            $scope.ParameterEntityList = resp.data;
        });
    }

    $scope.RemoveEntityRow = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveWorkcenter?RemoveEntityRow=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedEntity();
                
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
    //  #endregion Entity Tab

    //  #region Process Tab
    $scope.OpenProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPopupId')).modal('show');
        $scope.GetProcess();
    }
    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#ProcessPopupId')).modal('hide');

    }
    $scope.ProcessList = [];
    $scope.GetProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetProcess',
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.ProcessList = resp.data;
        });

    }
    

    $scope.SaveProcessList = [];
    $scope.CreateProcessWithParameterSetup = function () {
        $scope.SaveProcessList = [];
        if (baseService.arrayLength($scope.ProcessList) > 0) {
            angular.forEach($scope.ProcessList, function (a) {

                if (a.chk) {
                    var ob = {};

                    ob.ProcessId = a.ProcessId;
                    ob.Id = null;

                    $scope.SaveProcessList.push(ob);
                    ob = {};
                    a.chk = false;
                }
            });
        }


        $http({
            method: 'POST',
            url: $scope.path + 'CreateProcessWithParameterSetup',
            data: {
                models: $scope.SaveProcessList,
                headerid: $scope.ModelNew.Id,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.closeProductPopUp();
                //$scope.ModelNew.Id = response.data.Data.Id;
                $scope.GetSavedProcess();
                $scope.SaveProcessList = [];

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.ParameterProcessList = []
    $scope.GetSavedProcess = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedProcess?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {

            $scope.ParameterProcessList = resp.data;
        });
    }

    $scope.RemoveProcessRow = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveWorkcenter?RemoveProcessRow=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedProcess();
               
            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
    //  #endregion Process Tab

    //  #region Machine Tab
    $scope.OpenMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePopupId')).modal('show');
        $scope.GetMachine();
    }
    $scope.CloseMachinePopUp = function () {
        angular.element(document.querySelector('#MachinePopupId')).modal('hide');

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
   

    $scope.SaveMachineList = [];
    $scope.CreateMachineWithParameterSetup = function () {
        $scope.SaveMachineList = [];
        if (baseService.arrayLength($scope.MachineList) > 0) {
            angular.forEach($scope.MachineList, function (a) {

                if (a.chk) {
                    var ob = {};

                    ob.MachineMasterId = a.MachineMasterId;
                    ob.Id = null;

                    $scope.SaveMachineList.push(ob);
                    ob = {};
                    a.chk = false;
                }
            });
        }


        $http({
            method: 'POST',
            url: $scope.path + 'CreateMachineWithParameterSetup',
            data: {
                models: $scope.SaveMachineList,
                headerid: $scope.ModelNew.Id,
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');

            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.CloseMachinePopUp();
                //$scope.ModelNew.Id = response.data.Data.Id;
                $scope.GetSavedMachine();
                $scope.SaveMachineList = [];

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

    };

    $scope.ParameterMachineList = []
    $scope.GetSavedMachine = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetSavedMachine?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {

            $scope.ParameterMachineList = resp.data;
        });
    }

    $scope.RemoveMachineRow = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveWorkcenter?RemoveMachineRow=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                //ClearFields(response.data.Sequence);
                $scope.GetSavedProcess();

            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
    //  #endregion Machine Tab

    $scope.SavedParamEntityId = [];
    $scope.GetParameterEntity = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetParameterEntity?headerid=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {

            $scope.SavedParamEntityId = resp.data;
        });
    }

    // #region QUALITY PROCESS TAB
    $scope.QPAction = 'Save';
    $scope.QPSaveUrl = $scope.path + 'QPSave';

    $scope.QualityProcessTemp = {
        Id:null,
        QualityProcess: null,
        StandardProcess:null
    }
    $scope.QualityProcess = Object.assign({}, $scope.ModelTemp);


    $scope.QPSave = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.QualityProcessForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.QPSaveUrl,
                data: {
                    'data': $scope.QualityProcess,
                    'headerId': $scope.ModelNew.Id

                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');

                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.QualityProcess.Id = response.data.Data.Id;
                    $scope.QPClear();
                    $scope.GetQualityProcess();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.QualityProcessList = [];
    $scope.GetQualityProcess = function () {
        $scope.QualityProcessList = [];
        $http({
            method: 'GET',
            url: $scope.path + 'GetQualityProcess?headerId=' + $scope.ModelNew.Id,
            dataType: 'JSON'
        }).then(function succ(resp) {
            $scope.QualityProcessList = resp.data;
            $scope.QualityProcess = Object.assign({}, resp.data[0])
        });

    }

    $scope.GetOnDblClick = function (x) {
        
        $scope.QualityProcess = Object.assign({}, x.QualityProcess)
        $scope.QPAction = 'Update';
    }

    $scope.QPClear = function () {
        QPClearFields();
        return true;
    };
    function QPClearFields() {
        $scope.QPAction = 'Save';

        $scope.QualityProcessTemp = {
            Id: null,
            QualityProcess: null,
            StandardProcess: null
        }
        $scope.QualityProcess = Object.assign({}, $scope.ModelTemp);

    }

    $scope.RemoveQualityPeocess = function (x) {
        $http({
            method: 'POST',
            url: $scope.path + 'RemoveQualityPeocess?Id=' + x.Id,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                
                $scope.GetQualityProcess()

            }
            function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        });
    }
    // #endregion QUALITY PROCESS TAB

  }
