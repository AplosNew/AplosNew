'use strict';
MedicineReceiptController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', '$window' ,'baseService', '$routeParams', '$location', '$http', '$controller' ,'$filter'];
function MedicineReceiptController(cboService, commonMessage, $scope, $rootScope, $window, baseService, $routeParams, $location, $http, $controller, $filter) {
    $rootScope.title = 'Medicine Receipt';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'HumanResource/MedicineReceipt/';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'Save';
    $scope.saveUrlP = $scope.path + 'SavePurpose';
    $scope.deleteUrl = $scope.path + 'Delete/';
    baseService.init($scope.getListUrl);
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.partyType = 'Vendor';
    $controller('partyBaseController', { $scope: $scope, $http: $http });

    // GET CURRENT DATE
    var curDate = new Date()
    // OBJECT LIST DECLARED 
    $scope.ModelTemp = {
        Id: null,
        InvoiceDate: curDate,
        PartyName: null,
        PartyCode: null,
        InvoiceNumber: null,
        PartyId: null,
        PlantId: null,
        //Medicine:null,
    };
    $scope.ModalNew = Object.assign({}, $scope.ModelTemp);


    $scope.searchByMedicine = "UserName"; $scope.searchMedicine = "";
    $scope.MedicineSearchByList = [
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'Category',
            'value': 'Category'
        },
        
    ];

    // #region GET FUNCTIONS
    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetList",
            data: { column: $scope.searchByMedicine, value: $scope.searchMedicine },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;           
        });
    }

    $scope.PlantList = [];
    $scope.getPlant = function () {
        $http({
            method: 'POST',
            url: $scope.path + "getPlant",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.PlantList = response.data;

        });
    }
    $scope.getPlant();
    // #endregion GET FUNCTIONS
   
    

    
    $scope.MedicineList = [];
    var oblenth = Object.keys($scope.ModalNew).length;
    $scope.addMedicine = function () {
        for (var i = 0; i <= Object.keys($scope.ModalNew).length; i++) {
            $scope.ModelList = $scope.ModalNew.push($scope.ModalNew["Medicine"]);
        }
        $scope.displaychild();
    }

    // #REGION ALL GET FUNCTIONS
    $scope.userMedicineList = [];
    $scope.getMedicineData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineData',
            dataType:'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineList = response.data;

            for (var i = 0; i < $scope.userMedicineList.length; i++) {
                for (var j = 0; j < $scope.MedicineList.length; j++) {
                    if ($scope.userMedicineList[i].Id === $scope.MedicineList[j].Id) {
                        $scope.MedicineList[j].chk = true;
                    }
                }
            }
        });
    }
    $scope.getMedicineData();

    $scope.MedicineReceiptList = [];
    $scope.getMedicineReceipt = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'getMedicineReceipt',
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.MedicineReceiptList = response.data;
        });
    }
    $scope.getMedicineReceipt();
    //  #ENDREGION REGION ALL GET FUNCTIONS

    

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
       
        $scope.ModalNew.PartyCode = party.Code;
        $scope.ModalNew.PartyName = party.UserName;
        $scope.ModalNew.PartyId = party.Id;
        
        $scope.hidePartyPopUp();
    };

    // #REGION SELECT MEDICINE FROM POPUP SCREEN AND SEND IN CHILD GRID
    $scope.openMedicinePopUp = function () {
        angular.element(document.querySelector('#medicinePopUp')).modal('show');
        
    }

    $scope.closeMedicinePopUp = function () {
        
        angular.element(document.querySelector('#medicinePopUp')).modal('hide');
       
    }
    $scope.SendMedicine = function () {
        if (baseService.arrayLength($scope.MedicineList) > 0) {
            angular.forEach($scope.MedicineList, function (a) {
               
                if (a.chk) {
                    var ob = {};
                    ob.Id = a.Id;
                    ob.UserName = a.UserName
                    $scope.userMedicineList.push(ob);
                    ob = {};
                    a.chk = false;
                }
               
            });
        }

        //$scope.closeMedicinePopUp();
        $scope.displaychild();
    };
    // #ENDREGION SELECT MEDICINE FROM POPUP SCREEN AND SEND IN CHILD GRID

    // DISPLAY CHILD GRID
    $scope.displaychild = function () {
        // check input field Validations
       /* if (baseService.isUndefinedOrNull($scope.ModalNew.InvoiceNo)) {
            ShowResult('Invoice Number is Required.', 'failure');
            throw 'Invalid Request';
        }
        if (baseService.isUndefinedOrNull($scope.ModalNew.PartyName)) {
            ShowResult('Vendor is Required.', 'failure');
            throw 'Invalid Request';
        } if (baseService.isUndefinedOrNull($scope.ModalNew.InvoiceDate)) {
            ShowResult('InvoiceDate is Required.', 'failure');
            throw 'Invalid Request';
        }*/
        //$scope.addMedicine();
        document.querySelector("#medicinereceiptchildId").style.display = "block";
        
    }

    // FETCH VALUE FROM QANTITY, RATE AND CALCULATE
    $scope.ob = {};
    $scope.calcAmount = function (data1, index) {




        if (data1.Quantity == null || data1.Quantity == '') {
            $scope.userMedicineList[index].Rate = data1.Amount / 1
        }
        else if (data1.Amount == null || data1.Amount == '') {
            $scope.userMedicineList[index].Rate = 1 / data1.Quantity
        }
        else {
            $scope.userMedicineList[index].Rate = data1.Amount / data1.Quantity
        }
    }

    // #REGION SAVE

    $scope.SaveHeader = function () {
        $scope.$broadcast('show-errors-check-validity');
        $http({
            method: 'POST',
            url: $scope.path + 'SaveHeader',
            data: {
                'data': $scope.ModalNew,
                'medicinelist': $scope.userMedicineList,
                'partyId': $scope.ModalNew.PartyId
            },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                $scope.ModalNew.Id = response.data.Data.Id;                
                ShowResult(response.data.Message, 'success');
                $scope.getMedicineReceipt();
            }
        }), function errorCallBack(response) {
            ShowResult(response.data.Message, 'failure');
        }
    }
    // #ENDREGION SAVE

   
}
