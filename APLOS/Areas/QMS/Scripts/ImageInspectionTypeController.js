'use strict';
ImageInspectionTypeController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$timeout', 'fileReader', '$window'];
function ImageInspectionTypeController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $timeout, fileReader, $window) {
    $rootScope.title = 'Image Inspection Type';
    $scope.Action = 'Save';
    $scope.ImageInspectionTypeList = [];
    $scope.path = 'QMS/QualityProcess/';
    $scope.saveUrl = $scope.path + 'CreateImageInspectionType';
    $scope.saveEntityUrl = $scope.path + 'CreateImageInspectionTypeEntity';
    $scope.saveProductUrl = $scope.path + 'CreateImageInspectionTypeProcess';
    $scope.saveEntryLevelUrl = $scope.path + 'CreateImageInspectionTypeEntryLevel';
    $scope.saveBudgetUrl = $scope.path + 'CreateImageInspectionTypeUserApp';
    $scope.saveEmployeeUrl = $scope.path + 'CreateImageInspectionTypeEmployee';
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

    $scope.ModelNew = {
        Id: null,
        StandardName: null,
        UserName: null,
        Remarks: null,
        RefreshTime: 0,
        ScrollingTime:0
    }
    $scope.EntityModelNew = {
        Id: null,
        InspectionTypeId: null,
        EntityId: null
    }
    $scope.ProcessModelNew = {
        Id: null,
        InspectionTypeId: null,
        ProcessId: null,
        ProcessName: null
    }
    $scope.entryLevel = {
        Id: null,
        InspectionTypeId: null,
        Grade: null,
        UserName: null,
        LineItem: null,
        ProductCode: null,
        ProductionOrder: null,
        SalesOrder: null,
        SKU1: null,
        SKU2: null,
        SKU3: null,
        MaxQty: null,
        Remarks: null,
        Picture: null,
        Operation: null,
        Defect: null,
        IsProduction:false
    }

    $scope.UserApplicableModelNew = {
        Id: null,
        InspectionTypeId: null,
        BudgetId: null,
        BudgetName: null
    }
    $scope.EmployeeModelNew = {
        Id: null,
        InspectionTypeId: null,
        EmployeeId: null,
        EmployeeName: null,
        EmployeeCode: null
    }

    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageInspectionTypeList = response.data;
        });
    }
    $scope.getData();

    $scope.Get = function (args) {
        $scope.ModelNew = Object.assign({}, args.data);
        $scope.EntityModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.ProcessModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.entryLevel.InspectionTypeId = $scope.ModelNew.Id;
        $scope.UserApplicableModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.EmployeeModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.getInspectionTypeProcess();
        $scope.getImageEntityData();
        $scope.getInspectionTypeEntryLevel();
        $scope.getInspectionTypeBudget();
        $scope.getInspectionTypeEmployee();
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.entityList = [];
    cboService.getCboEntityByPlant(null, null, "", function (result) {
        $scope.entityList = result;
    });


    $scope.ImageProcessMasterList = [];
    $scope.getInspectionTypeProcess = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeProcessList?imageInspectionTypeId=" + $scope.ProcessModelNew.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageProcessMasterList = response.data;
        });
    }

    $scope.ImageEntityList = [];
    $scope.getImageEntityData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeEntityList?imageInspectionTypeId=" + $scope.EntityModelNew.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.ImageEntityList = response.data;
        });
    }

    $scope.InspectionTypeEntryLevelList = [];
    $scope.getInspectionTypeEntryLevel = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeEntryLevelList?imageInspectionTypeId=" + $scope.entryLevel.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InspectionTypeEntryLevelList = response.data;
        });
    }

    $scope.InspectionTypeBudgetList = [];
    $scope.getInspectionTypeBudget = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeUserAppList?imageInspectionTypeId=" + $scope.UserApplicableModelNew.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InspectionTypeBudgetList = response.data;
        });
    }

    $scope.InspectionTypeEmployeeList = [];
    $scope.getInspectionTypeEmployee = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetInspectionTypeEmployeeList?imageInspectionTypeId=" + $scope.EmployeeModelNew.InspectionTypeId,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.InspectionTypeEmployeeList = response.data;
        });
    }

    
     

    $scope.employee = [];
    $scope.getEmployeePopUpData = function () {
        $scope.employee = [];
        $scope.popUpEmpDataList = [];
        $http({
            //method: 'POST',
            //url: 'QMS/QualityProcess/getemployeelist',
            method: 'GET',
            url: 'employees/authorizationconfig/getallemployeedata'
           // data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: $window.plantId },
            //dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.employee = response.data;
        });
        angular.element(document.querySelector('#employeeNewPopUp')).modal('show');
    }

    $scope.setEmpData = function (obj) {
        $scope.EmployeeModelNew.Id = null;
        $scope.EmployeeModelNew.EmployeeId = obj.data.SystemId;
        $scope.EmployeeModelNew.EmployeeName = obj.data.EmployeeName;
        angular.element(document.querySelector('#employeeNewPopUp')).modal('hide');
        $scope.EmployeeSave();
    };

    function removeDuplicates(myArr, prop) {
        return myArr.filter((obj, pos, arr) => {
            return arr.map(mapObj => mapObj[prop]).indexOf(obj[prop]) === pos;
        });
    }
    $scope.resposiblePersonList = [];
    $scope.getResponsiblePersonData = function () {
        $scope.resposiblePersonList = [];
        $scope.popUpEmpDataList = [];
        $http({
            method: 'POST',
            url: 'QMS/QualityProcess/getemployeelist',
            data: { column: $scope.searchByEmp, value: $scope.empearch, plantId: $window.plantId },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            $scope.resposiblePersonList = response.data;
            $scope.popUpEmpDataList = response.data;
        });
        angular.element(document.querySelector('#responsibleNewPopUp')).modal('show');
    }

    $scope.setResponseData = function (obj) {
        $scope.ModelNew.ResponsiblePersonId = obj.data.SystemID;
        $scope.ModelNew.ResponsiblePersonName = obj.data.EmployeeCode + "-" + obj.data.EmployeeName;
        angular.element(document.querySelector('#responsibleNewPopUp')).modal('hide');
    };

    $scope.popUpDataList = [];
    $scope.popUpBudgetCode = function () {
        try {
            var entityCode = "";
            if ($scope.ImageEntityList.length > 0) {
                var uniqueEntityId = removeDuplicates($scope.ImageEntityList, 'EntityId');
                var entityCode = "";
                if (uniqueEntityId.length > 0) {
                    entityCode = "IN(";
                    entityCode += Array.prototype.map.call(uniqueEntityId, function (item) { return "'" + item.EntityId + "'"; }).join(",") + ")";
                }
                $scope.sqlInStatement = entityCode;
            }
            $scope.popUpUrl = 'employees/recruitment/GetManpowerBudgetListByEntitySql?entityids=' + $scope.sqlInStatement;

            $scope.popUpEmpDataList = [];
            $http({
                method: 'GET',
                url: $scope.popUpUrl

            }).then(function successCallback(response) {
                $scope.popUpDataList = response.data;
                for (var j = 0; j < $scope.InspectionTypeBudgetList.length; j++) {
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        if ($scope.InspectionTypeBudgetList[j].BudgetId == $scope.popUpDataList[i].Id) {
                            $scope.popUpDataList.splice(i, 1);
                        }
                    }
                }
            });
            angular.element(document.querySelector('#popUpId')).modal('show');
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.selectDoubleClick = function () {
        try {
            var ob = {};
            for (var i = 0; i < $scope.popUpDataList.length; i++) {
                if ($scope.popUpDataList[i].isSelected == true) {
                    //if (checkDoubleGWS($scope.InspectionTypeBudgetList, $scope.popUpDataList[i].BudgetId) === false) {
                    ob.Id = null;
                    ob.BudgetName = $scope.popUpDataList[i].BudgetName;
                        ob.BudgetId = $scope.popUpDataList[i].BudgetId;
                    ob.InspectionTypeId = $scope.ModelNew.Id;
                        $scope.InspectionTypeBudgetList.push(ob);
                        ob = {};
                    //}
                }
            }
            angular.element(document.querySelector('#popUpId')).modal('hide');
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    function checkDoubleGWS(list, Id) {
        for (var i = 0; i < list.length; i++) {
            if (list[i].BudgetId === Id) {
                return true;
            }
        }
        return false;
    }


    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
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
    };

    $scope.EntitySave = function () {
        $scope.EntityModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
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
    };
    $scope.ProcessSave = function () {
        $scope.ProcessModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveProductUrl,
                data: { 'data': $scope.ProcessModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getInspectionTypeProcess();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

    };

    $scope.GetInspectionTypeEntryLevel = function (Row) {
        $scope.entryLevel = Object.assign({}, Row.data);
    };

    $scope.EntryLevelSave = function () {
        $scope.entryLevel.InspectionTypeId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveEntryLevelUrl,
                data: { 'data': $scope.entryLevel },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                   $scope.getInspectionTypeEntryLevel();
                    clearInspectionTypeEntryLevel();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

    };

    function clearInspectionTypeEntryLevel() {
        $scope.entryLevel = {
            Id: null,
            InspectionTypeId: null,
            Grade: null,
            UserName: null,
            LineItem: null,
            ProductCode: null,
            ProductionOrder: null,
            SalesOrder: null,
            SKU1: null,
            SKU2: null,
            SKU3: null,
            MaxQty: null,
            Remarks: null,
            Picture: null,
            Operation: null,
            Defect: null,
            IsProduction: false
        }
    }


    $scope.UserAppSave = function () {
        $scope.UserApplicableModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveBudgetUrl,
                data: { 'datas': $scope.InspectionTypeBudgetList },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getInspectionTypeBudget();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

    };
    $scope.EmployeeSave = function () {
        $scope.EmployeeModelNew.InspectionTypeId = $scope.ModelNew.Id;
        $scope.$broadcast('show-errors-check-validity');
            $http({
                method: 'POST',
                url: $scope.saveEmployeeUrl,
                data: { 'data': $scope.EmployeeModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.getInspectionTypeEmployee();
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
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

    $scope.processSearchList = [
        {
            'name': 'Sequence',
            'value': 'Sequence'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Local Name',
            'value': 'LocalName'
        },
        {
            'name': 'Alias',
            'value': 'Alias'
        }
    ];
    $scope.processPopUpParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'Sequence',
        searchBy: "UserName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.processPopUpDataList = [];
    $scope.processPopUp = function () {
        if (baseService.isUndefinedOrNull($window.companyId))
            return ShowResult('Please at first select company.', 'failure');

        $scope.popUpProcessUrl = 'QMS/QualityProcess/GetProductionProcessList'
        $scope.getProcessData = function (pageno) {
            baseService.paginationBase($scope.popUpProcessUrl, pageno, $scope.processPopUpParameters)
                .then(function (result) {
                    $scope.processPopUpDataList = result.Rows;
                    $scope.processPopUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'processPopUp');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#processPopUp')).modal('show');
        $scope.getProcessData();
    };

    $scope.closeProcessPopUp = function () {
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };

    $scope.processAdd = function (data) {
        $scope.ProcessModelNew.ProcessId = data.Id;
        $scope.ProcessModelNew.ProcessName = data.StandardName;
        angular.element(document.querySelector('#processPopUp')).modal('hide');
    };




}