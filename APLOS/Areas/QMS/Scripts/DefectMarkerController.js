'use strict';
DefectMarkerController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$timeout', 'fileReader', '$window'];
function DefectMarkerController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $timeout, fileReader, $window) {
    $rootScope.title = 'Defect Marker';
    $scope.Action = 'Save';
    $scope.DefectMasterModelList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.saveUrl = $scope.path + 'CreateDefectMarkerMaster';
    $scope.deleteUrl = $scope.path + 'deletedefect/';

    $scope.searchBy = "Entity"; $scope.search = "";
    $scope.searchByList = [{ value: 'Entity', name: "Entity" }, { value: 'WorkCenterMaster', name: "WorkCenterMaster" }, { value: 'ProductionOrder', name: "ProductionOrder" }];
    $scope.productionSummaryNew = { Id: null, EntityId: null, WorkCenterMasterId: null, MarkDate: null, ProductionOrderId: null, BuyerItem: null, OwnItem: null, BuyerOrder: null, OwnOrder: null, Remarks: null, ProductionShiftId: null, SalesOrderId: null, ResponsiblePersonId: null, ResponsiblePerson: null }


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetDefectMarkerMasterList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.DefectMasterModelList = response.data;
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.productionSummaryNew = Object.assign({}, args.data);
        $scope.loadWC();
        $scope.GetShiftList();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.productionSummaryNew.EntityId = $scope.entityList[0].Value;
            }
        });
    }
    $scope.getAllEntities();

    $scope.wcList = [];
    $scope.loadWC = function () {
        $http.get('Productions/Productionsummary/GetWCCbo?entityId=' + $scope.productionSummaryNew.EntityId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.wcList = response.data;
                }
            });
    };

    $scope.shiftList = [];
    $scope.GetShiftList = function () {
        $scope.shiftList = [];
        $http.get('Productions/Productionsummary/GetShiftCbo?wcId=' + $scope.productionSummaryNew.WorkCenterMasterId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.shiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        $scope.productionSummaryNew.ProductionShiftId = $scope.shiftList[0].Value;
                    }
                }
            });
    }

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
        $scope.loadExistingDefects($scope.productionSummaryNew.Id);
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
                data: { 'data': $scope.productionSummaryNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.productionSummaryNew.Id = response.data.Data.Id;
                    $scope.getData();

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
        const clickX = (event.clientX - rect.left) / rect.width;
        const clickY = (event.clientY - rect.top) / rect.height;

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
        $scope.currentDefect.Type = $("#dtype option:selected").text();
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
    $scope.deleteDefect = function (id) {
        $scope.defects = $scope.defects.filter(d => d.id !== id);
        $scope.drawDefects();
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
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/GetImageAndDefects',
            data: { masterId: $scope.productionSummaryNew.Id },
            dataType: 'JSON',
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                // Construct full image path (adjust your path here)
                $scope.ImageFile = response.data.ImageFile;
                const imagePath = virtualPath.GarmentPic + response.data.ImageFile;

                $scope.imageSrc = imagePath;
                $scope.imageLoaded = true;
                $scope.defects = response.data.Defects.map(d => ({
                    id: d.Id,
                    x: parseFloat(d.XNormalized),
                    y: parseFloat(d.YNormalized),
                    Type: d.Type,
                    Description: d.Description
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
                DefectMarkerMasterId: $scope.productionSummaryNew.Id,
                ImageFile: hasNewImage ? input.files[0].name : existingFileName, // ✅ use existing file name in edit mode
                Width: $scope.originalImageWidth,
                Height: $scope.originalImageHeight,
                Defects: $scope.defects.map(d => ({
                    Id: d.id || 0,
                    DefectMarkerMasterId: $scope.productionSummaryNew.Id,
                    Width: $scope.originalImageWidth,
                    Height: $scope.originalImageHeight,
                    XNormalized: d.x,
                    YNormalized: d.y,
                    Type: d.Type,
                    Description: d.Description
                }))
            };

            const formData = new FormData();

            formData.append("masterId", $scope.productionSummaryNew.Id);
            formData.append("defectsJson", JSON.stringify(payload));

            // ✅ Only attach image file if a new one is selected
            if (hasNewImage) {
                formData.append("imageFile", input.files[0]);
            }

            // Send to MVC
            $http.post("/QMS/QualityProcess/SaveImageAndDefects", formData, {
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






}