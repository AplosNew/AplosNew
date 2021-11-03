'use strict';
function ProductSubCategoryAttributeController(commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Product SubCategory Attribute";
    $scope.Action = 'Save';
    $scope.ChAction = 'Add Row';
    $scope.index = -1;
    $scope.tableShow = false;
    $scope.productSubCategoryAttributes = [];
    $scope.path = 'Products/productsubcategoryattribute/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    baseService.init($scope.getListUrl);
    $scope.getData = function (pageno) {
        $rootScope.parameters.productSubCategoryId = $scope.productSubCategoryAttributeNew.ProductSubCategoryId;
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.productSubCategoryAttributes = [];
                $scope.productSubCategoryAttributes = result;
                if (result.length > 0)
                    $scope.tableShow = true;
                else
                    $scope.tableShow = false;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };

    $scope.productSubCategoryList = [];
    $scope.materialAttributeList = [];
    $http({
        method: 'GET',
        url: 'Products/productsubcategory/getcbo'
    }).then(function successCallback(response) {
        $scope.productSubCategoryList = response.data;
    });

    $scope.productSubCategoryAttribute = {
        Id: null,
        ProductSubCategoryId: null,
        MaterialAttributeId: null,
        MaterialAttributeName: null,
        Sequence: null,
        Active: true,
        IsFreeField: true,
        IsPreDefinedField: true,
        IsMandatory: true
    };
    $scope.productSubCategoryAttributeNew = Object.assign({}, $scope.productSubCategoryAttribute);

    // #region material attribute popup

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Code',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.popUpDataList = [];
    $scope.popUpList = [];
    $scope.popUp = function () {
        if ($scope.productSubCategoryAttributeNew.ProductSubCategoryId == null) {
            ShowResult('Please select product subcategory...!', 'failure');
            return;
        }
        $scope.popUpUrl = 'Materials/materialattribute/getmaterialattributedata';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    if (baseService.arrayLength($scope.popUpList) == 0) {
                        baseService.getDDLSearchColumn(result.Rows, $scope.popUpList);
                        $scope.popUpParameters.total_count = result.Total;
                        //$scope.popUpList = result.Rows;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    }
    $scope.selectDoubleClick = function (data) {
        // Do Somthing
        $scope.closePopUp();
    }
    $scope.selectSingleClick = function (data) {
        $scope.valueData = data;
    }
    $scope.selectByButton = function () {
        if ($scope.valueData == '') {
            ShowResult('Please at first select row', '#popUpId');
            return;
        }
        $scope.selectDoubleClick($scope.valueData)
        $scope.closePopUp();
    }
    $scope.closePopUp = function () {
        $scope.valueData = '';
        ClearField();
        angular.element(document.querySelector('#popUpId')).modal('hide');
    }

    // #endregion
    $scope.selectDoubleClick = function (data) {
        try {
            if ($scope.productSubCategoryAttributes.length > 5) {
                throw 'Total no of material attribute can not be more than 6...!';
            }
            var isAvailable = false;
            //$scope.materialAttributeName = document.getElementById("materialAttributeId").options[document.getElementById('materialAttributeId').selectedIndex].text;
            for (var i = 0; i < $scope.productSubCategoryAttributes.length; i++) {
                isAvailable = listValidation($scope.productSubCategoryAttributes[i].MaterialAttributeId, data.Id, i);
                if (isAvailable) {
                    throw 'This material attribute : [' + data.UserName + '] has been already taken';
                }
            }
            angular.copy($scope.productSubCategoryAttributeNew, $scope.productSubCategoryAttribute);
            // isAvailable true == add new
            if (!isAvailable) {
                if ($scope.index == -1) {
                    $scope.productSubCategoryAttribute.MaterialAttributeId = data.Id;
                    $scope.productSubCategoryAttribute.MaterialAttributeName = data.UserName;
                    $scope.productSubCategoryAttributes.push($scope.productSubCategoryAttribute);
                }
                else {
                    $scope.productSubCategoryAttributes[$scope.index] = $scope.productSubCategoryAttribute;
                    $scope.productSubCategoryAttribute[$scope.index].MaterialAttributeId = data.Id;
                    $scope.productSubCategoryAttributes[$scope.index].MaterialAttributeName = data.UserName;
                }
                $scope.tableShow = true;
                $scope.index = -1;
                $scope.closePopUp();
            }
        } catch (err) {
            ShowResult(err, 'failure', 'popUpId');
        }
    }
    function listValidation(oldValue, newValue, index) {
        var isAvailable = false;
        // MaterialAttributeId
        if ($scope.index == -1) {
            if (oldValue == newValue) {
                isAvailable = true;
                return isAvailable;
            }
        }
        else {
            if ($scope.index != index) {
                if (oldValue == newValue) {
                    isAvailable = true;
                    return isAvailable;
                }
            }
        }
        return isAvailable;
    }
    $scope.Get = function (id, index) {
        $scope.index = index;
        $scope.productSubCategoryAttribute = $scope.productSubCategoryAttributes[$scope.index];
        $scope.productSubCategoryAttributeNew = Object.assign({}, $scope.productSubCategoryAttribute);
        $scope.ChAction = 'Update Row';
    };
    $scope.Select = function (id, index) {
        $scope.Get(id, index);
    }
    function isSeqValid(list) {
        try {
            if (list == null || list.length <= 0) {
                throw 'Please insert at lest one row';
            }
            var newList = [];
            for (var i = 0; i < list.length; i++) {
                if (list[i].IsFreeField == false && list[i].IsPreDefinedField == false) {
                    throw 'Please select free field or pre-defined field or both';
                }
                var seq = parseInt(list[i].Sequence);
                if (list[i].Sequence == null) {
                    throw 'Sequence can not be null';
                }
                if (newList.indexOf(seq) == -1) {
                    newList.push(seq);
                }
                else {
                    throw 'Duplicate Sequence [' + seq + '] found in grid';
                }
            }
        } catch (e) {
            throw e;
        }
    }
    $scope.Save = function () {
        try {
            //$scope.materialAttributeName = document.getElementById("materialAttributeId").options[document.getElementById('materialAttributeId').selectedIndex].text;
            isSeqValid($scope.productSubCategoryAttributes);
            if ($scope.Action == "Save") {
                $http({
                    method: 'POST',
                    url: $scope.saveUrl,
                    data: { productSubCategoryAttributes: $scope.productSubCategoryAttributes },
                    dataType: 'JSON'
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, 'failure');
                    }
                    else {
                        ShowResult(response.data.Message, 'success');
                        ClearFields();
                    }
                }), function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            }
        } catch (err) {
            ShowResult(err, 'failure');
        }
    }
    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.productSubCategoryAttributeNew.ProductSubCategoryId)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.productSubCategoryAttributeNew.ProductSubCategoryId,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error == true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getData();
                    ClearFields(response.data.Sequence);
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    }
    $scope.Clear = function () {
        ClearFields();
        $scope.productSubCategoryAttributes = [];
        $scope.productSubCategoryAttributeNew.ProductSubCategoryId = null;
        $scope.tableShow = false;
        return true;
    }
    function ClearFields() {
        //$scope.Action = "Save";
        $scope.productSubCategoryAttribute = {};
        $scope.productSubCategoryAttributeNew = { ProductSubCategoryId: $scope.productSubCategoryAttributeNew.ProductSubCategoryId };
        $scope.productSubCategoryAttributeNew.Active = true;
        $scope.productSubCategoryAttributeNew.IsFreeField = true;
        $scope.productSubCategoryAttributeNew.IsPreDefinedField = true;
        $scope.productSubCategoryAttributeNew.IsMandatory = false;
    }
    function ClearField() {
        //$scope.Action = "Save";
        $scope.productSubCategoryAttribute = {};
        $scope.productSubCategoryAttributeNew = { ProductSubCategoryId: $scope.productSubCategoryAttributeNew.ProductSubCategoryId };
        $scope.productSubCategoryAttributeNew.Active = true;
        $scope.productSubCategoryAttributeNew.IsFreeField = true;
        $scope.productSubCategoryAttributeNew.IsPreDefinedField = true;
        $scope.productSubCategoryAttributeNew.IsMandatory = false;
    }

    $scope.tblIndex = -1;
    $scope.DeleteModal = function (index) {
        $scope.message_confirmation = '';
        $scope.tblIndex = index;
        $scope.message_confirmation = 'Are you sure want to delete this data....';
        angular.element(document.querySelector('#confirmAttributePopUp')).modal('show');
    };

    $scope.removeRow = function () {
        $scope.productSubCategoryAttributes.splice($scope.tblIndex, 1);
        $scope.tblIndex = -1;
        if ($scope.productSubCategoryAttributes.length > 0)
            $scope.tableShow = true;
        else
            $scope.tableShow = false;
    };
}
ProductSubCategoryAttributeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];