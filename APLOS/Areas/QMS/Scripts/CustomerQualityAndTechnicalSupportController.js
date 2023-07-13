'use strict';
CustomerQualityAndTechnicalSupportController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function CustomerQualityAndTechnicalSupportController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'Complaint Master';
    $scope.path = 'QMS/CustomerQualityAndTechnicalSupport/';
    $scope.partyType = 'Vendor';
    $scope.Action = 'Save';
    $scope.employeeUrl = $scope.path + 'GetEmployeeListByWhom';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };

    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.ModelList = [];
    $scope.GetDate = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetData",
           
            dataType: 'JSON'
        }).then(function successCallback(res) {
            $scope.ModelList = res.data;
        })
    }
    $scope.GetDate();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.ModelTemp = {
        Id: null,
        SalesId: null,
        ArticleId: null,
        PartyName: null,
        ComplaintDate:null,
        ToCloseDate:null,
        PartyCode: null,
        CustomerId: null,
        ResponsiblePersonId: null,
        ResponsiblePerson: null,
        ResponsiblePersonCode: null,
        ByWhomId: null,
        ByWhomeCode: null,
        ByWhomeName:null

    }
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp)

    $scope.partyParameters = {
        limit: 10
        , offset: 0
        , order: 'ASC'
        , sort: 'UserName, PartyAccountGroupName'
        , searchBy: 'UserName'
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };


    $scope.productNew = Object.assign({}, $scope.product);
    $scope.partyList = [];


    // CLOSE PARTY POP UP
    $scope.closePartyPopUp = function (x) {
        var party = x.data;

        $scope.ModelNew.PartyCode = party.Code;
        $scope.ModelNew.PartyName = party.UserName;
        $scope.ModelNew.CustomerId = party.Id;

        $scope.hidePartyPopUp();
        $scope.GetArticle();
    };

    $scope.ArticleList = [];
    $scope.GetArticle = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetArticle",
            data: {
                'salesId': $scope.ModelNew.CustomerId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ArticleList = response.data;
        })
    }

    $scope.InvoicenumberList = [];
    $scope.GetInvoiceNumber = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInvoiceNumber",
            data: {
                'articleId': $scope.ModelNew.ArticleId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InvoicenumberList = response.data;
            $scope.GetComplaint();
        })
    }
    //$scope.GetInvoiceNumber()

    //#region Responsible Person
   
    $scope.employeeParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'EmployeeCode, FirstName, MiddleName, LastName ',
        searchBy: 'EmployeeCode',
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };

    $scope.Name = null;
    $scope.employeeList = [];
    $scope.showEmployeeListPopUp = function (name) {
        $scope.employeeList = [];
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#employeePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectEmployeePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.ModelNew.ResponsiblePersonId = data.SystemId;
        $scope.ModelNew.ResponsiblePerson  = data.EmployeeName;
        $scope.ModelNew.ResponsiblePersonCode = data.EmployeeCode;

        angular.element(document.querySelector('#employeePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideEmployeePopUp = function () {
        angular.element(document.querySelector('#employeePopUps')).modal('hide');
    };

    
    //#endregion Responsible Person

    //#region ByWhom
    $scope.showByWhomListPopUp = function (name) {
        $scope.employeeList = [];
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#ByWhomePopUps')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectByWhomePopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.ModelNew.ByWhomId = data.SystemId;
        $scope.ModelNew.ByWhomeName = data.EmployeeName;
        $scope.ModelNew.ByWhomeCode = data.EmployeeCode;

        angular.element(document.querySelector('#ByWhomePopUps')).modal('hide');
        $scope.Name = null;
    };

    $scope.hideByWhomePopUp = function () {
        angular.element(document.querySelector('#ByWhomePopUps')).modal('hide');
    };
    //#endregion ByWhom

    // #region ActionTOBETaken
    $scope.obj = {};
    $scope.ShowAtionToBeTakenPopUp = function () {
        angular.element(document.querySelector('#actiontobetakenPopUp')).modal('show');
       
        $scope.GetCustomerStatus();
    }

    $scope.CloseAtionToBeTakenPopUp = function () {
        angular.element(document.querySelector('#actiontobetakenPopUp')).modal('hide');

    }

    $scope.ActionToBeTakenTemp = {
        Id: null,
        ByWhomId: null,
        ByWhomeCode: null,
        ByWhomeName: null,
        TargetDate: null,
        CurrentStatusId: null,
        FinalClosingStatusId: null,
        Remarks: null

    }
    $scope.ActionToBeTakenModel = Object.assign({}, $scope.ActionToBeTakenTemp);

    $scope.showByWhomActionTakenPopUp = function (name) {
        $scope.employeeList = [];
        try {
            $scope.Name = name;

            $scope.employeeParameters.searchBy = 'EmployeeCode';
            baseService.setCurrentPage('employeeList');
            $scope.searchEmployeeByList = [];
            $scope.getEmployeeData = function (pageno) {
                baseService.paginationBase($scope.employeeUrl, pageno, $scope.employeeParameters)
                    .then(function (result) {
                        $scope.employeeList = result.Rows;
                        $scope.employeeParameters.total_count = result.Total;

                        if (baseService.arrayLength($scope.searchEmployeeByList) === 0)
                            baseService.getDDLSearchColumn(result.Rows, $scope.searchEmployeeByList);
                        $scope.employeeParameters.searchBy = 'EmployeeCode';
                    }, function () {
                        ShowResult(commonMessage.NetworkError, 'failure');
                    }).finally(function () {
                    });
            };
            angular.element(document.querySelector('#ByWhomePopUpssec')).modal('show');
            $scope.getEmployeeData();
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectByWhomeActiontakenPopUp = function (index, data) {
        $scope.employeeIndex = index;

        $scope.ModelNew.ByWhomId = data.SystemId;
        $scope.ModelNew.ByWhomeName = data.EmployeeName;
        $scope.ModelNew.ByWhomeCode = data.EmployeeCode;

        angular.element(document.querySelector('#ByWhomePopUps')).modal('hide');
        $scope.Name = null;
    };
   
    $scope.CheckedInvoiceNumberList = [];
    $scope.ClosePopupOnSelectAllField = function () {

        $scope.CheckedUserGroupList = [];
        for (var i = 0; i < $scope.InvoicenumberList.length; i++) {

            if ($scope.InvoicenumberList[i].isSelected) {
                $scope.CheckedInvoiceNumberList.push($scope.InvoicenumberList[i]);
            }

        }
        $http({
            method: 'POST',
            url: $scope.path + 'Create',
            data: {
                'actiontakenObj': $scope.ActionToBeTakenModel,
                'invoicelist': $scope.CheckedInvoiceNumberList,
                'headerid': $scope.ModelNew.Id
            },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');


            }
        });



    }
    // #endregion ActionTOBETaken

    $scope.ComplaintList = [];
    $scope.GetComplaint = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetComplaint",
           
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ComplaintList = response.data;
        })
    }

    $scope.StatusList = [];
    $scope.GetCustomerStatus = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetCustomerStatus',
            dataType:'JSON'
        }).then(function successCallback(response) {
            $scope.StatusList = response.data;
        })
    }

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        var d = new Date();
        let date = d.getDate().toString();
        let year = d.getFullYear().toString();
        const monthNames = ["Jan", "Feb", "March", "Apr", "May", "June", "Jul", "August", "Sep", "Oct", "Nov", "Dec"];
        let month = monthNames[d.getMonth().toString()];
        let curdate = date.concat("-", month, "-", year);
        
        if ($scope.ModelNew.ToCloseDate == curdate) {
            ShowResult("Close date shoud be greater then today date.");
            throw "Close date shoud be greater then today date."
        }
       
        $http({
            method: 'POST',
            url: $scope.path + 'Save',
            data: {
                'datas': $scope.ModelNew,
                
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.ModelNew.Id = response.data.Data.Id;
                $scope.Action = 'Update';
                

            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }

      
    };

    
}