'use strict';
DocumentStatusUpdateController.$inject = ['fileReader', 'cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DocumentStatusUpdateController(fileReader, cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = 'Employee Document Status Update';
    $scope.path = 'employees/employeedocumentassignment/';

    $scope.modalNew = {};

    $scope.EmpCatList = [];
    $scope.getEmpCategory = function () {
        $http({
            method: 'GET',
            url: 'Leave/AnnualLeaveProcess/GetEmpCategory'
        }).then(function success(response) {
            $scope.EmpCatList = response.data;
        })
    }
    $scope.getEmpCategory();

    $scope.GetDOJFromToDate = function () {
        $http({
            method: 'GET',
            url: $scope.path + "GetDOJFromToDate",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.modalNew.FromDate = response.data[0].FromDate;
            $scope.modalNew.ToDate = response.data[0].ToDate;
        });
    };
    $scope.GetDOJFromToDate();

    $scope.popUpDataList = [];
    $scope.getEmpPopUpData = function () {
        try {
            var EmployeeTypeObj = $("#EmpCategoryDropdown").data("ejDropDownList");
            $scope.EmpCategoryString = EmployeeTypeObj.getSelectedValue().split(",");

            if ($scope.EmpCategoryString == "") {
                throw ("Please Select Employee Category ...");
            }

            $scope.popUpDataList = [];
            $http({
                method: 'GET',
                url: 'employees/employeedocumentassignment/GetDocumentEmployeeData',
                params: {
                    'FromDate': $scope.modalNew.FromDate,
                    'ToDate': $scope.modalNew.ToDate,
                    'EmpCategory': $scope.EmpCategoryString
                }

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
            });
            angular.element(document.querySelector('#popUp')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    $scope.setEmpData = function (obj) {
        var data = obj.data;
        $scope.modalNew.EmployeeCode = data.EmployeeCode;
        $scope.modalNew.EmployeeId = data.SystemId;
        $scope.modalNew.EmployeeName = data.EmployeeName;
        $scope.CompanyGroupID = data.GroupID;
        $scope.PlantId = data.PlantId;
        $scope.Loaddocumentdatalist();
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.closeEmployeePopUp = function () {
        angular.element(document.querySelector('#popUp')).modal('hide');
    }

    $scope.documentdataList = [];
    $scope.Loaddocumentdatalist = function () {
        $http.get('employees/employeeinformation/GetEmpAllDocumentDataList?companyGroupId=' + $scope.CompanyGroupID + '&pId=' + $scope.modalNew.EmployeeId + '&plantId=' + $scope.PlantId)
            .then(function (response) {
                $scope.documentdataList = response.data;
            });
    };

    $scope.idList = [];
    $scope.sqlInStatement = null;
    $scope.CountDelItem = 0;
    function MakeMultiDeleteData() {
        $scope.CountDelItem = 0;
        for (var di = 0; di < $scope.documentdataList.length; di++) {
            if ($scope.documentdataList[di].Applicable == false) {
                $scope.idList.push($scope.documentdataList[di]);
                $scope.CountDelItem++;
            }
        }

        if ($scope.idList.length > 0) {
            var uniqueMasterOrderId = removeDuplicates($scope.idList, 'Id');
            var wcEmpCode = "";
            if (uniqueMasterOrderId.length > 0) {
                wcEmpCode = "IN(";
                wcEmpCode += Array.prototype.map.call(uniqueMasterOrderId, function (item) { return "'" + item.Id + "'"; }).join(",") + ")";
            }
            $scope.sqlInStatement = wcEmpCode;
        }

    }

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }

    $scope.Save = function () {
        $scope.List = [];
        if ($scope.documentdataList.length > 0)

            MakeMultiDeleteData();
        for (var i = 0; i < $scope.documentdataList.length; i++) {
            if ($scope.documentdataList[i].Applicable) {
                $scope.List.push($scope.documentdataList[i]);
            }
        }

        $http({
            method: 'POST',
            //url: 'employees/employeedocumentassignment/DeleteMultiDocument?id=' + $scope.sqlInStatement
            // method: 'POST',
            url: "employees/employeedocumentassignment/DeleteUpdateMultiDocument",
            data: { 'id': $scope.sqlInStatement, 'data': $scope.List, 'EmpId': $scope.modalNew.EmployeeId},
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.sqlInStatement = null;
                $scope.Clear();
                $scope.List = [];
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });
    }


    $scope.Clear = function () {
        $scope.modalNew.EmployeeCode = null;
        $scope.modalNew.EmployeeId = null;
        $scope.modalNew.EmployeeName = null;
        $scope.CompanyGroupID = null;
        $scope.PlantId = null;
        $scope.documentdataList = [];
    }

};




