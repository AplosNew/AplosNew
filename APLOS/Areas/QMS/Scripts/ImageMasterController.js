'use strict';
ImageMasterController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$timeout', 'fileReader', '$window'];
function ImageMasterController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $timeout, fileReader, $window) {
    $rootScope.title = 'Image Master';
    $scope.Action = 'Save';
    $scope.ImageMasterList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.saveUrl = $scope.path + 'CreateImageMarkerMaster';
    $scope.saveEntityUrl = $scope.path + 'CreateImageMarkerEntity';
    $scope.saveProductUrl = $scope.path + 'CreateImageMarkerProduct';
    $scope.deleteUrl = $scope.path + 'deletedefect/';

    $scope.searchBy = "Id"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'UserName', name: "UserName" }, { value: 'StandardName', name: "StandardName" }];
   // $scope.productionSummaryNew = { Id: null, EntityId: null, WorkCenterMasterId: null, MarkDate: null, ProductionOrderId: null, BuyerItem: null, OwnItem: null, BuyerOrder: null, OwnOrder: null, Remarks: null, ProductionShiftId: null, SalesOrderId: null, ResponsiblePersonId: null, ResponsiblePerson: null }
    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };


    //$scope.processList = [];
    //$http({
    //    method: 'GET',
    //    url: "QMS/QualityProcess/GetProcessCbo",
    //    dataType: 'JSON'
    //}).then(function successCallback(response) {
    //    $scope.processList = response.data;

    //});

    $scope.ModelNew = {
        Id: null,
        StandardName: null,
        UserName: null,
        ProcessId: null,
        Remarks: null
    }
    $scope.EntityModelNew = {
        Id: null,
        ImageMasterId: null,
        EntityId: null
    }
    $scope.ProductModelNew = {
        Id: null,
        ImageMasterId: null,
        ProductMasterId: null
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetImageMasterList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageMasterList = response.data;
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EntityModelNew.ImageMasterId = $scope.ModelNew.Id;
        $scope.ProductModelNew.ImageMasterId = $scope.ModelNew.Id;

        $scope.getProductData();
        $scope.getImageEntityData();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });


    $scope.ImageProductMasterList = [];
    $scope.getProductData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetImageProductMasterList?ImageMasterId=" + $scope.ProductModelNew.ImageMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageProductMasterList = response.data;
        });
    }

    $scope.ImageEntityList = [];
    $scope.getImageEntityData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetImageEntityList?ImageMasterId=" + $scope.EntityModelNew.ImageMasterId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageEntityList = response.data;
            $scope.GetprocessList();
        });
    }

    $scope.productMasterList = [];
    $scope.GetProductMasterCbo = function () {
        $http.get('Products/ProductMaster/GetCbo')
            .then(function (response) {
                $scope.productMasterList = response.data.Rows;
                console.log('productMasterList', $scope.productMasterList);
            });
    }
    $scope.GetProductMasterCbo();


    $scope.defectTypeList = [];
    $scope.GetDefectTypeCbo = function () {
        $scope.shiftList = [];
        $http.get('QMS/DefectType/GetCbo')
            .then(function (response) {
                $scope.defectTypeList = response.data;
            });
    }
    $scope.GetDefectTypeCbo();

    $scope.modelFilterByList = [
        { 'name': 'Prod. Order#', 'value': 'Id' },
        { 'name': 'Prod. Status', 'value': 'ProductionStatus' },
        { 'name': 'Material', 'value': 'Material' },
        { 'name': 'Product', 'value': 'Product' },
        { 'name': 'Product Category', 'value': 'ProductCategory' },
        { 'name': 'Master Order No', 'value': 'MasterOrderId' },
        { 'name': 'Buyer Order#', 'value': 'BuyerRefNo' },
        { 'name': 'Own Order#', 'value': 'OwnRefNo' },
        { 'name': 'Buyer Item#', 'value': 'StyleNo' },
        { 'name': 'Own Item#', 'value': 'OwnStyleNo' },
        { 'name': 'SO No', 'value': 'SONo' },
        { 'name': 'SO Desc', 'value': 'SODesc' },
        { 'name': 'Buyer', 'value': 'buyer' },
        { 'name': 'Customer', 'value': 'Customer' },
    ];

    $scope.PRSearchColumn = 'Id';
    $scope.PRSearchValue = null;
    $scope.modelList = [];
    $scope.getPOData = function () {
        try {
            $scope.modelList = [];
            if (baseService.isUndefinedOrNull($scope.productionSummaryNew.EntityId)) {
                throw "Entity is required.";
            }
            $http({
                method: 'POST',
                data: {
                    'entityid': $scope.productionSummaryNew.EntityId, 'column': $scope.PRSearchColumn, 'value': $scope.PRSearchValue
                },
                url: 'Materials/MaterialIssueControl/getlist'
            }).then(function successCallback(response) {
                $scope.modelList = response.data;
                angular.element(document.querySelector('#POItemPopup')).modal('show');
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    $scope.rowDataBound = function rowDataBound(e) {
        if (e.data.Balance != 0) {
            e.row.css("background-color", '#FFFF00')
        }

    }

    $scope.SetPO = function ($event) {
        $scope.productionSummaryNew.ProductionOrderId = $event.data.Id;
        $scope.productionSummaryNew.BuyerItem = $event.data.BuyerItem;
        $scope.productionSummaryNew.OwnItem = $event.data.OwnItem;
        $scope.productionSummaryNew.BuyerOrder = $event.data.BuyerOrder;
        $scope.productionSummaryNew.OwnOrder = $event.data.OwnOrder;
        angular.element(document.querySelector('#POItemPopup')).modal('hide');
    }

    $scope.ShowDefectMarkingpopUp = function () {
        angular.element(document.querySelector('#DefectMarkingPopup')).modal('show');
        $scope.loadExistingDefects($scope.ModelNew.Id);
    }

    $scope.CloseDefectMarkingpopUp = function () {
        angular.element(document.querySelector('#DefectMarkingPopup')).modal('hide');
    }

    $scope.SalesOrderListForProductionOrderId = [];
    $scope.getSalesOrderByProdOrderList = function () {
        $http({
            method: 'GET',
            url: 'OrderManagements/ProductionOrder/GetProductionRecipeMaterialList?productionOrderId=' + $scope.productionSummaryNew.ProductionOrderId
        }).then(function successCallback(response) {
            $scope.SalesOrderListForProductionOrderId = response.data;
            angular.element(document.querySelector('#SOItemPopup')).modal('show');

        });
    }
    $scope.SetSO = function ($event) {
        $scope.productionSummaryNew.SalesOrderId = $event.data.SalesOrderId;
        $scope.getSalesOrderColorSizeList();
        angular.element(document.querySelector('#SOItemPopup')).modal('hide');
    }
    $scope.CloseSOpopUp = function () {
        angular.element(document.querySelector('#SOItemPopup')).modal('hide');
    }

    $scope.colorList = [];
    $scope.sizeList = [];
    $scope.getSalesOrderColorSizeList = function () {
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetColorSizeCbo?soId=' + $scope.productionSummaryNew.SalesOrderId
        }).then(function successCallback(response) {
            $scope.colorList = response.data.colorItem;
            $scope.sizeList = response.data.sizeItem;

        });
    }

    $scope.empearch = "";
    $scope.searchByEmp = "EmployeeCode"; $scope.search = "";
    $scope.searchEmpByList = [{ value: 'SystemID', name: "SystemID" }, { value: 'EmployeeCode', name: "Employee Code" }, { value: 'EmployeeName', name: "EmployeeName" }];


    $scope.employee = [];
    $scope.getPopUpData = function () {
        $scope.employee = [];
        $scope.popUpEmpDataList = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/getemployeelist',
            data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: $window.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
            $scope.popUpEmpDataList = response.data;
        });
    }
    $scope.getPopUpData();

    $scope.getEmpData = function () {
        $scope.employee = [];
        $scope.popUpEmpDataList = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/getemployeelist',
            data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: $window.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
            $scope.popUpEmpDataList = response.data;
        });
    }

    $scope.setEmpData = function (obj) {
        $scope.productionSummaryNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.productionSummaryNew.ResponsiblePerson = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
    };
   

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.ModelNew.Id = response.data.Data.Id;
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.EntitySave = function () {
        $scope.EntityModelNew.ImageMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveEntityUrl,
                data: { 'data': $scope.EntityModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getImageEntityData();
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };
    $scope.ProductMasterSave = function () {
        $scope.ProductModelNew.ImageMasterId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveProductUrl,
                data: { 'data': $scope.ProductModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getProductData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    //// Trigger hidden input  

    $scope.defects = [];
    $scope.imageSrc = null;
    $scope.ImageFile = null;
    $scope.imageLoaded = false;
    $scope.originalImageWidth = 0;
    $scope.originalImageHeight = 0;

    var defectCanvas, garmentImage, ctx;

    $scope.showDefectModal = false;
    $scope.modalPosition = { x: 0, y: 0 };
    $scope.currentDefect = {};

    $scope.triggerImageUpload = function () {
        const input = document.getElementById('imageInput');
        if (input) input.click(); // open dialog
    };

    // load image from file input
    $scope.loadImage = function (element) {
        const file = element.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = function (e) {
            $scope.$apply(function () {
                $scope.imageSrc = e.target.result;
                $scope.imageLoaded = true;
                $timeout($scope.prepareCanvas, 100);
            });
        };
        reader.readAsDataURL(file);
    };


    $scope.prepareCanvas = function () {
        garmentImage = document.getElementById("garmentImage");
        defectCanvas = document.getElementById("defectCanvas");

        if (!garmentImage || !defectCanvas) return;

        ctx = defectCanvas.getContext("2d");

        // Match canvas to image size
        defectCanvas.width = garmentImage.clientWidth;
        defectCanvas.height = garmentImage.clientHeight;

        $scope.originalImageWidth = garmentImage.naturalWidth;
        $scope.originalImageHeight = garmentImage.naturalHeight;

        // Add click listener only once
        defectCanvas.onclick = $scope.onCanvasClick;

        $scope.drawDefects();
    };



    // handle click to add or edit defect
    $scope.onCanvasClick = function (event) {
        if (!$scope.imageLoaded) return;

        const canvas = document.getElementById('defectCanvas');
        const rect = canvas.getBoundingClientRect();
        //const clickX = (event.clientX - rect.left) / rect.width;
        //const clickY = (event.clientY - rect.top) / rect.height;
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;

        const x = (event.clientX - rect.left) * scaleX;
        const y = (event.clientY - rect.top) * scaleY;

        const clickX = x / canvas.width;
        const clickY = y / canvas.height;

        // detect if clicking an existing defect
        const clickedDefect = $scope.defects.find(d => {
            const px = d.x * rect.width;
            const py = d.y * rect.height;
            const dx = (event.clientX - rect.left) - px;
            const dy = (event.clientY - rect.top) - py;
            return Math.sqrt(dx * dx + dy * dy) < 10;
        });

        if (clickedDefect) {
            // open editor for existing defect
            $scope.editDefect(clickedDefect, event);
        } else {
            // create new defect
            $scope.currentDefect = { x: clickX, y: clickY, Type: '', Description: '', id: Date.now() };
            $scope.modalPosition = { x: event.pageX, y: event.pageY };
            $scope.showDefectModal = true;
            $scope.$applyAsync();
        }
    };


    // draw red markers
    $scope.drawDefects = function () {
        const canvas = document.getElementById('defectCanvas');
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        $scope.defects.forEach(d => {
            const px = d.x * canvas.width;
            const py = d.y * canvas.height;
            ctx.beginPath();
            ctx.arc(px, py, 6, 0, Math.PI * 2);
            ctx.fillStyle = '#ef4444';
            ctx.fill();
            ctx.strokeStyle = '#b91c1c';
            ctx.stroke();
        });
    };

    // save current defect (create or update)
    $scope.saveDefect = function () {
        $scope.currentDefect.Zone = $("#zone option:selected").text();
        const idx = $scope.defects.findIndex(d => d.id === $scope.currentDefect.id);
        if (idx >= 0) $scope.defects[idx] = angular.copy($scope.currentDefect);       
        else $scope.defects.push(angular.copy($scope.currentDefect));
        $scope.showDefectModal = false;
        $scope.drawDefects();
    };

    // delete current defect (from popup)
    $scope.deleteCurrentDefect = function () {
        $scope.defects = $scope.defects.filter(d => d.id !== $scope.currentDefect.id);
        $scope.showDefectModal = false;
        $scope.drawDefects();
    };

    // delete from list
    $scope.areaDeleteList = [];
    $scope.deleteDefect = function (id) {
        $scope.areaDeleteList.push($scope.defects.find(d => d.id === id));
        $scope.defects = $scope.defects.filter(d => d.id !== id);
        $scope.drawDefects();
        $scope.DeleteMarkedPoint(id);
    };

    $scope.DeleteMarkedPoint = function (id) {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteProductArea?id=' + id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.loadExistingDefects($scope.ModelNew.Id);
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };

    // edit from list
    $scope.editDefect = function (defect, event) {
        $scope.currentDefect = angular.copy(defect);
        $scope.modalPosition = { x: event.pageX, y: event.pageY };
        $scope.showDefectModal = true;
    };



    // close popup without saving
    $scope.closeDefectModal = function () {
        $scope.showDefectModal = false;
    };


    // clear everything
    $scope.clearDefects = function () {
        $scope.defects = [];
        $scope.imageSrc = null;
        $scope.imageLoaded = false;
    };

    $scope.loadExistingDefects = function () {
        $scope.defects = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/GetImageAreas',
            data: { masterId: $scope.ModelNew.Id },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (!baseService.isUndefinedOrNull(response.data.ImageFile)) {
                // Construct full image path (adjust your path here)
                $scope.ImageFile = response.data.ImageFile;
                const imagePath = virtualPath.GarmentPic + response.data.ImageFile;

                $scope.imageSrc = imagePath;
                $scope.imageLoaded = true;
                $scope.defects = response.data.ImageAreas.map(d => ({
                    id: d.Id,
                    x: parseFloat(d.XAxis),
                    y: parseFloat(d.YAxis),
                    Code: d.Code,
                    ImageID: d.ImageID,
                    ImageName: d.ImageName,
                    AreaName: d.AreaName,
                    Zone: d.Zone,
                    Remarks: d.Remarks
                }));

                // Wait for image render then draw defects
                $timeout($scope.prepareCanvas, 300);
            }
        });
    };

    //$scope.loadExistingDefects = function (masterId) {
    //    $http.post("/QMS/QualityProcess/GetImageAndDefects", { params: { masterId: masterId } })
    //        .then(function (response) {
    //            if (response.data.Success) {
    //                // Construct full image path (adjust your path here)
    //                $scope.ImageFile = response.data.ImageFile;
    //                const imagePath = virtualPath.GarmentPic + response.data.ImageFile;

    //                $scope.imageSrc = imagePath;
    //                $scope.imageLoaded = true;
    //                $scope.defects = response.data.Defects.map(d => ({
    //                    id: d.Id,
    //                    x: parseFloat(d.XNormalized),
    //                    y: parseFloat(d.YNormalized),
    //                    Type: d.Type,
    //                    Description: d.Description
    //                }));

    //                // Wait for image render then draw defects
    //                $timeout($scope.prepareCanvas, 300);
    //            } else {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //        })
    //        .catch(function (error) {
    //            ShowResult(error, 'failure');
    //        });
    //};

    // save example



    $scope.saveDefects = function () {
        try {
            if (!$scope.defects || $scope.defects.length === 0) {
                throw "No defects to save!";
                return;
            }

            const input = document.getElementById("imageInput");
            const hasNewImage = input && input.files && input.files.length > 0;

            // 🟢 Extract existing filename from imageSrc (for edit mode)
            let existingFileName = null;
            if ($scope.imageSrc) {
                const parts = $scope.imageSrc.split('/');
                existingFileName = parts[parts.length - 1];
            }

            // Prepare payload
            const payload = {
                ImageMasterId: $scope.ModelNew.Id,
                ImageFile: hasNewImage ? input.files[0].name : existingFileName, // ✅ use existing file name in edit mode
                Width: $scope.originalImageWidth,
                Height: $scope.originalImageHeight,
                ImageAreas: $scope.defects.map(d => ({
                    Id: d.id || 0,
                    ImageMasterId: $scope.ModelNew.Id,
                    Width: $scope.originalImageWidth,
                    Height: $scope.originalImageHeight,
                    XAxis: d.x,
                    YAxis: d.y,
                    Zone: d.Zone,
                    AreaName: d.AreaName,
                    Code: d.Code,
                    Remarks: d.Remarks
                })),
                AreaDeleteData: $scope.areaDeleteList
            };

            const formData = new FormData();

            formData.append("masterId", $scope.ModelNew.Id);
            formData.append("defectsJson", JSON.stringify(payload));
            formData.append("deletesData", $scope.areaDeleteList);

            // ✅ Only attach image file if a new one is selected
            if (hasNewImage) {
                formData.append("imageFile", input.files[0]);
            }

            // Send to MVC
            $http.post("QMS/QualityProcess/SaveImageArea", formData, {
                transformRequest: angular.identity,
                headers: { "Content-Type": undefined }
            })
                .then(function (response) {
                    if (response.data.Success) {
                        ShowResult(response.data.Message, 'success');
                    } else {
                        ShowResult(response.data.Message, 'failure');
                    }
                })
                .catch(function (error) {
                    ShowResult(error, 'failure');
                });

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    // keep canvas updated on resize
    window.addEventListener('resize', function () {
        if ($scope.imageLoaded) $scope.prepareCanvas();
    });

    $scope.processList = [];
    $scope.processPopUp = function () {
        $http({
            method: 'GET',
            url: "QMS/QualityProcess/GetProcessList",
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.processList = response.data;
        });

        angular.element(document.querySelector('#processPopUp')).modal('show');
    };

    $scope.refreshTemplateemployee = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAllWise });
    };

    function CheckBoxSelectAllWise(e) {
        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;
        }

        var filtered = $("#GridP").data("ejGrid").getFilteredRecords();
        if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
            for (var i = 0; i < $scope.processList.length; i++) {
                $scope.processList[i].Flag = ChkOrUnchk;
            }
        }
        else {
            for (var j = 0; j < filtered.length; j++) {
                filtered[j].CheckBoxSelect = ChkOrUnchk;
            }
        }
        var gridObj = $("#GridP").data("ejGrid");
        gridObj.refreshContent();
    };

    $scope.ImageMasterProcessList = [];
    function MakeData() {

        for (var i = 0; i < $scope.processList.length; i++) {
            if ($scope.processList[i].Flag == true) {
                if (checkExists($scope.ImageMasterProcessList, $scope.processList[i].Id) === false) {
                    var ob = {};
                    ob.Id = -(Math.floor(Math.random() * 100) + 1);
                    ob.ProcessId = $scope.processList[i].Id;
                    ob.ImageMasterId = $scope.ModelNew.Id;
                    ob.Sequence = $scope.processList[i].Sequence;
                    ob.Code = $scope.processList[i].Code;
                    ob.ShortName = $scope.processList[i].ShortName;
                    ob.StandardName = $scope.processList[i].StandardName;
                    ob.UserName = $scope.processList[i].UserName;

                    $scope.ImageMasterProcessList.push(ob);
                }
                else {
                    throw "This Process " + $scope.processList[i].UserName + " is already taken.";
                }
            }
        }

    }

    function checkExists(list, id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].ProcessId === id) {
                return true;
            }
        }
        return false;
    }

    $scope.CloseProcess = function () {
        try {
            MakeData();
            $scope.SaveProcess();
            angular.element(document.querySelector('#processPopUp')).modal('hide');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    }

    $scope.SaveProcess = function () {
        try {

            $http({
                method: 'POST',
                url: 'QMS/QualityProcess/SaveProcess',
                data: { 'data': $scope.ImageMasterProcessList, 'masterId': $scope.ModelNew.Id },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.GetprocessList();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            };

        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.ImageMasterProcessList = [];
    $scope.GetprocessList = function () {
        $scope.ImageMasterProcessList = [];
        $http({
            method: 'GET',
            url: 'QMS/QualityProcess/GetImageMasterProcess?masterId=' + $scope.ModelNew.Id
        }).then(function successCallback(response) {
            $scope.ImageMasterProcessList = response.data;
        });
    }

    $scope.message_detailconfirmation = null;
    $scope.removeProcess = function (obj) {
        $scope.bomDetailNew = obj.data;
        if (!baseService.isUndefinedOrNull($scope.bomDetailNew.Id))
            $scope.message_detailconfirmation = 'Are you sure want to delete permanently [ ' + $scope.bomDetailNew.UserName + ' ]';
        angular.element(document.querySelector('#confirmProcessPopUp')).modal('show');
    }

    $scope.DeleteProcess = function () {
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/DeleteProcess?id=' + $scope.bomDetailNew.Id
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.GetprocessList();
            }
        }, function () {
            ShowResult(commonMessage.NetworkError, 'failure');
        }).finally(function () {
        });

    };



}