
'use strict';
DailyTargetController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter'];
function DailyTargetController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter) {
    $rootScope.title = "Daily Target";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.costingTypeses = [];
    $scope.path = 'Productions/DailyTarget/';
    $scope.Copy = $scope.path + 'CopyFromTable';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.saveUrl = $scope.path + 'create';
    $scope.updateUrl = $scope.path + 'edit';
    $scope.deleteUrl = $scope.path + 'delete/';
    $scope.WithEmployee = false;
    $scope.WithMachine = false;


    $scope.DailyProductionTarget = {
        Id: null,
        DailyProductionTargetID: null,
        Line: null,
        PRNo: null,
        MaterialMasterArticleId: null,
        MaterialMasterId: null,
        Manpower: null,
        SMV: null,
        TotalHour: null,
        PlantId: null,
        EntityId: null,
        ProcessId: null,
        ProductionDate: $filter("date")(Date.now(), 'dd-MMM-yyyy'),

    };
    $scope.DailyProductionTargetNew = Object.assign({}, $scope.DailyProductionTarget);


    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.DailyProductionTargetNew.EntityId = $scope.entityList[0].Value;
                $scope.loadProcessList($scope.DailyProductionTargetNew.EntityId);
            }
        });
    };
    $scope.getAllEntities();

    $scope.processList = [];
    $scope.loadProcessList = function (entityid) {
        cboService.GetEntityProcessCbo(entityid, function (result) {
            $scope.processList = result;
            if (baseService.arrayLength(result) === 1) {
                $scope.DailyProductionTargetNew.ProcessId = $scope.processList[0].Value;

            }
        });
    };
    $scope.ShiftList = [];
    $scope.GetShiftList = function () {
        $http.get('Productions/EmployeeOperations/GetShift?processId=' + $scope.DailyProductionTargetNew.ProcessId + '&entityId=' + $scope.DailyProductionTargetNew.EntityId)
            .then(function (response) {
                if (baseService.arrayLength(response.data) > 0) {
                    $scope.ShiftList = response.data;
                    if (baseService.arrayLength(response.data) === 1) {
                        DailyProductionTargetNew.ShiftId = $scope.ShiftList[0].Value;
                    }
                }
            });
    }

    $scope.listFromProcessOrSFGInventory = [];
    $scope.GetSFGMovementFromCbo = function (entity) {
        $http({
            method: 'GET',
            url: 'Productions/DailyTarget/GetProcessFromCbo?entity=' + entity,
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error == true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                //sccuess 
                $scope.listFromProcessOrSFGInventory = response.data;

            }
        }, function errorCallback(response) {
            ShowResult(response.data.Message, 'failure');
        });
    };

    $scope.changeProcess = function () {
        $scope.Process = $("#Process option:selected").text();
        $scope.Status = null;
        $scope.Status = $.grep($scope.listFromProcessOrSFGInventory, function (item) {
            return item.ProcessId === $scope.DailyProductionTargetNew.ProcessId;
        })[0].Status;

        for (var i = 0; i < $scope.listFromProcessOrSFGInventory.length; i++) {
            if ($scope.DailyProductionTargetNew.ProcessId === $scope.listFromProcessOrSFGInventory[i].ProcessId) {
                $scope.DailyProductionTargetNew.ProductionBookingLevel = $scope.listFromProcessOrSFGInventory[i].ProductionBookingLevel;
                $scope.LotNumberCapture = $scope.listFromProcessOrSFGInventory[i].LotNumberCapture;
                $scope.LotNumberMandatory = $scope.listFromProcessOrSFGInventory[i].LotNumberMandatory;
                $scope.IsFirst = $scope.listFromProcessOrSFGInventory[i].IsFirst;
                $scope.Status = $scope.listFromProcessOrSFGInventory[i].Status;
                $scope.Sequence = $scope.listFromProcessOrSFGInventory[i].Sequence - 1;
                break;
            }
        }
    };

    $scope.DailyTargetList = [];
    $scope.getDailytarget = function () {

        try {

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.EntityId))
                throw 'Plase select entity';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.ProcessId))
                throw 'Plase select process';

            if (angular.isUndefinedOrNull($scope.DailyProductionTargetNew.ProductionDate))
                throw 'Plase select target date';

            $http({

                method: 'GET',
                url: 'Productions/DailyTarget/GetDailyTarget?EntityId=' + $scope.DailyProductionTargetNew.EntityId + '&ProcessId=' + $scope.DailyProductionTargetNew.ProcessId + '&ProductionDate=' + $scope.DailyProductionTargetNew.ProductionDate,
            }).then(function successCallback(response) {
                $scope.DailyTargetList = response.data;
            }
            )
        } catch (e) {
            ShowResult(e, 'failure');
        }

    }


    $scope.DailyTargetAllCheck = function (args) {
        $("#headchk").ejCheckBox({ "change": CheckBoxSelectAll });
    };

    function CheckBoxSelectAll(e) {


        var ChkOrUnchk = false;
        if (e.model.checkState === "check") {
            ChkOrUnchk = true;

        }

        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Active = ChkOrUnchk;
        }

        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
    };



    $scope.Save = function () {
        try {
            $scope.$broadcast('show-errors-check-validity');

            for (var i = 0; i < $scope.DailyTargetList.length; i++) {
                if ($scope.DailyTargetList[i].Active) {
                    if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].PRNo) == true) {
                        throw "Please select Production Order No. for '" + $scope.DailyTargetList[i].Line + "'";
                        if (baseService.isUndefinedOrNull($scope.DailyTargetList[i].Manpower) == true) {
                            throw "Manpower is Empty.";
                        }
                    }
                }
            }
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'DailyTargetData': $scope.DailyTargetList, 'TargetDate': $scope.DailyProductionTargetNew.ProductionDate, 'EntityId': $scope.DailyProductionTargetNew.EntityId, 'ProcessId': $scope.DailyProductionTargetNew.ProcessId },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    /*ClearFields(response.data.Sequence);*/
                    $scope.getDailytarget();
                    /* $scope.GetDetails({ data: { Id: response.data.Data.Id } });*/
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, 'failure');

        }
    };

    $scope.Clear = function () {
        ClearFields();
        return true;
    }
    function ClearFields() {
        $scope.Action = "Save";
        $scope.DailyProductionTarget = {}
        $scope.DailyTargetList = [];
        $scope.SOItemList = [];
    }


    //search PR
    $scope.SelectedLineForPR = {};
    $scope.SOItemList = [];
    $scope.SearchPRPopup = function (data) {
        $scope.SelectedLineForPR = data;
        if (baseService.isUndefinedOrNull(data.WorkCenterMasterId)) {
            return ShowResult('Please Work Center.', 'failure');
        }
        $scope.SOItemList = [];
        $http.get($scope.path + 'GetProductionOrderPOPUp?entityid=' + $scope.DailyProductionTargetNew.EntityId + '&processId=' + $scope.DailyProductionTargetNew.ProcessId)
            .then(
                function successCallback(response) {
                    $scope.SOItemList = response.data;
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');

    }

    $scope.selectSOItem = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            if ($scope.SelectedLineForPR.WorkCenterMasterId == $scope.DailyTargetList[i].WorkCenterMasterId) {
                $scope.DailyTargetList[i].PRNo = args.data.Id;
                $scope.DailyTargetList[i].Material = args.data.Material;
                $scope.DailyTargetList[i].Article = args.data.Article;
                $scope.DailyTargetList[i].MaterialMasterId = args.data.MaterialMasterId;
                $scope.DailyTargetList[i].MaterialMasterArticleId = args.data.ArticleId;
                $scope.DailyTargetList[i].CustomerPONo = args.data.CustomerPONo;
                $scope.DailyTargetList[i].BuyerItemNo = args.data.BuyerItemNo;
                $scope.DailyTargetList[i].SMV = args.data.SPT;
                angular.element(document.querySelector('#POItemPopup')).modal('hide');
                break;
            }
        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        gridObj.refreshTemplate();
    }
    $scope.CalculateTotalQuantity = function (args) {
        for (var i = 0; i < $scope.DailyTargetList.length; i++) {
            $scope.DailyTargetList[i].Quantity = (dbl($scope.DailyTargetList[i].QuantityPerHour) * dbl($scope.DailyTargetList[i].TotalHour)).toFixed(0);

        }
        var gridObj = $("#GridDailyTargetList").data("ejGrid");
        gridObj.refreshContent();
        //gridObj.refreshTemplate();
    }
    $scope.rowDataBound = function rowDataBound(e) {

        if (e.data.IsManual == true)
            e.row.css("background-color", '#d1e5ff');


    }
    $scope.ShowDiv = false;
    $scope.AddLineItemG = function (obj) {
        $scope.SelectedLine = obj.data;
        $scope.ShowDiv = true;
        var eDialog = $("#dialogLineDesign").data("ejDialog");
        $("#dialogLineDesign").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogLineDesign").ejDialog("refresh");

        eDialog.open();
        if (obj.data.HasLayout == false) {
            $scope.CopyTable(obj.data);
        }
        else {
            $scope.GetLineLayout(obj.data);
        }

    };

    $scope.PopupItem = function (data) {
        $scope.SelectedLine = data;
        $scope.ShowDiv = true;
        var eDialog = $("#dialogLineDesignReport").data("ejDialog");
        $("#dialogLineDesignReport").ejDialog("refresh");

        eDialog.open();


    };


    $scope.CopyTable = function (data) {
        try {
            $scope.SelectedLineForPR = data;
            $http({
                method: 'POST',
                url: $scope.Copy,
                data: { 'entityid': $scope.DailyProductionTargetNew.EntityId, 'processId': $scope.DailyProductionTargetNew.ProcessId, 'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate, 'SelectedLine': $scope.SelectedLineForPR },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $scope.SelectedLineForPR.HasLayout = true;
                    $scope.SelectedLineForPR.CanCopy = false;
                    var gridObj = $("#gridEmployeeReplace").data("ejGrid");
                    gridObj.refreshContent();
                    $scope.GetLineLayout(data);
                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.GetLineLayout = function (data) {
        try {
            $scope.nodes = [];
            $scope.NodeIndex = 0;
            $scope.NodeSeed = (new Date()).getTime();
            $scope.SelectedLineForPR = data;
            $http({
                method: "POST",
                url: $scope.path + 'GetSaveData',
                data: {
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                response.data = JSON.parse(response.data[0].Layout);

                var diagram = $("#diagram").ejDiagram("instance");
                diagram.clear();
                diagram.add(response.data);
                $scope.UpdateEmployeeAttendanceAndProductionInfo();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };

    $scope.drawingToolsList = [
        {
            id: "Rectangle_Tool", tooltiptext: "Rectangle",
            spriteCss: "glyphicon glyphicon-stop",
        }, {
            id: "RoundedRectangle_Tool", tooltiptext: "RoundRect",
            spriteCss: "glyphicon glyphicon-unchecked",
        }, {
            id: "Ellipse_Tool", tooltiptext: "Ellipse",
            spriteCss: "glyphicon glyphicon-cd",
        }
        , {
            id: "UpArrow", tooltiptext: "UpArrow",
            spriteCss: "glyphicon glyphicon-arrow-up",
        }
        , {
            id: "RightArrow", tooltiptext: "RightArrow",
            spriteCss: "glyphicon glyphicon-arrow-right blue",
        }
        , {
            id: "DownArrow", tooltiptext: "DownArrow",
            spriteCss: "glyphicon glyphicon-arrow-down",
        }
        , {
            id: "LeftArrow", tooltiptext: "LeftArrow",
            spriteCss: "glyphicon glyphicon-arrow-left",
        },
        {
            id: "Textbox_Tool", tooltiptext: "Textbox",
            spriteCss: "glyphicon glyphicon-text-background",
        },
        {
            id: "Image_Tool", tooltiptext: "Image",
            spriteCss: "glyphicon glyphicon-picture",
        },
        {
            id: "Html_Tool", tooltiptext: "Html",
            spriteCss: "glyphicon glyphicon-header",
        },
    ];

    $scope.width = "100%";
    $scope.height = "300px";

    $scope.pageSettings = { scrollLimit: "diagram", boundaryConstraints: ej.datavisualization.Diagram.BoundaryConstraints.Diagram };


    $scope.selectednode = null;
    $scope.NodeIndex = 0;
    $scope.NodeSeed = (new Date()).getTime();
    $scope.nodeCollectionChange = function (args) {

        if (args["state"] != "changed")
            return;

        if (args["changeType"] == "remove") {
            for (var i = 0; i < $scope.nodes.length; i++) {
                if ($scope.nodes[i]["id"] == args.element["id"]) {
                    $scope.nodes.splice(i, 1);
                    break;
                }
            }
        }

        if (args["changeType"] == "insert") {
            if (args["cause"] != "clipBoard") {
                for (var i = 0; i < $scope.nodes.length; i++) {
                    if ($scope.nodes[i]["id"] == args.element["id"]) {
                        return;
                    }
                }
            }

            $scope.NodeIndex++;
            args.element["id"] = $scope.NodeSeed + '-' + $scope.NodeIndex;
            $scope.nodes.push(args.element);
        }

    }
    $scope.onItemclick = function (args) {
        var diagram = $("#diagram").ejDiagram("instance");
        var option = args.currentTarget.id;
        switch (option) {
            case "Rectangle_Tool":
                diagram.model.drawType = { type: "basic", shape: "rectangle" };
                break;
            case "RoundedRectangle_Tool":
                diagram.model.drawType = { type: "basic", shape: "rectangle", "cornerRadius": 5 };
                break;
            case "Ellipse_Tool":
                diagram.model.drawType = { type: "basic", shape: "ellipse" };
                break;
            case "UpArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 1l-15 15h9v16h12v-16h9z" };
                break;
            case "RightArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M31 16l-15-15v9h-16v12h16v9z" };
                break;
            case "DownArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 31l15-15h-9v-16h-12v16h-9z" };
                break;
            case "LeftArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M1 16l15 15v-9h16v-12h-16v-9z" };
                break;
            case "Polygon_Tool":
                diagram.model.drawType = { type: "basic", shape: "polygon", points: [{ x: 13.560, y: 67.524 }, { x: 21.941, y: 41.731 }, { x: 0.000, y: 25.790 }, { x: 27.120, y: 25.790 }, { x: 35.501, y: 0.000 }, { x: 43.882, y: 25.790 }, { x: 71.000, y: 25.790 }, { x: 49.061, y: 41.731 }, { x: 57.441, y: 67.524 }, { x: 35.501, y: 51.583 }, { x: 13.560, y: 67.524 }] };
                break;
            case "Textbox_Tool":
                diagram.model.drawType = { type: "text", textBlock: { "text": "TextNode", textAlign: ej.datavisualization.Diagram.TextAlign.Center }, fillColor: "transparent", borderColor: "transparent" };
                break;
            case "Image_Tool":
                diagram.model.drawType = { type: "image", source: "content/images/Employees/6.png" };
                break;
            case "Html_Tool":
                diagram.model.drawType = {
                    type: "html", templateId: "htmlTemplate"
                };
                break;
        }

        var _tool = diagram.tool();
        diagram.update({ tool: _tool | ej.datavisualization.Diagram.Tool.DrawOnce });
        //  diagram.update({ tool: _tool });
    }
    $scope.OpenEmployeeSearchBox = function () {
        var eDialog = $("#dialogSearchEmployee").data("ejDialog");
        $("#dialogSearchEmployee").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogSearchEmployee").ejDialog("refresh");

        eDialog.open();

        $scope.getEmployeeData();
    }
    $scope.EmployeemodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'EmployeeCode', name: 'Code ' },
        { value: 'EmployeeName', name: 'Name ' },
        { value: 'Department', name: 'Department ' },
        { value: 'Designation', name: 'Designation ' },
        { value: 'Section', name: 'Section ' },
        { value: 'SubSection', name: 'Sub Section ' },
        { value: 'OtherSkills', name: 'Other Skills ' }
    ];
    $scope.searchCol = "UserName";
    $scope.searchVal = "";
    $scope.EmployeeSearchCol = "EmployeeName";
    $scope.EmployeeSearchVal = "";
    $scope.WhereEmployeeNeeded = '';
    $scope.EmployeeList = [];
    $scope.getEmployeeData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal,
                    'OperationId': $scope.selectednode.items[0].addInfo.OperationId,
                    'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.ViewEmployeeStatus = function (args) {
        try {

            if (angular.isUndefinedOrNull(args.data.WorkCenterMasterId) == false) {
                if (args.data.WorkCenterMasterId != $scope.SelectedLine.WorkCenterMasterId) {
                    ShowResult("Employee has already been " + args.data.AssignmentStatus, 'failure');
                    return;
                }
            }
            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("id", "notEqual", $scope.selectednode.items[0].id));
            for (var i = 0; i < exists.length; i++) {
                if (exists[i].addInfo.EmployeeId == args.data.Id) {
                    ShowResult("Employee has already been " + args.data.AssignmentStatus, 'failure');
                    return;
                }
            }

            $scope.selectednode.items[0].addInfo.EmployeeId = args.data.Id;
            $scope.selectednode.items[0].addInfo.EmployeeName = args.data.EmployeeName;
            $scope.selectednode.items[0].addInfo.EmpPicPath = args.data.EmpPicPath;
            $scope.selectednode.items[0].addInfo.Designation = args.data.Designation;
            $scope.selectednode.items[0].addInfo.EmployeeCode = args.data.EmployeeCode;
            $scope.selectednode.items[0].addInfo["DayStatus"] = args.data.DayStatus;
            $scope.selectednode.items[0].addInfo["DayColor"] = args.data.DayColor;

            $scope.ConstructReplaceEmployee();

            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
    }


    //////////////////////////////////////////
    $scope.OpenFixedAssetSearchBox = function () {
        var eDialog = $("#dialogSearchFixedAsset").data("ejDialog");
        $("#dialogSearchFixedAsset").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogSearchFixedAsset").ejDialog("refresh");

        eDialog.open();

        $scope.getFixedAssetData();
    }

    $scope.FixedAssetmodelFilterByList = [
        { value: 'Id', name: 'Id ' },
        { value: 'Model', name: 'Model ' },
        { value: 'SerialNo', name: 'SerialNo ' },
        { value: 'YearOfManufacture', name: 'Year ' },
        { value: 'Description', name: 'Description ' },
        { value: 'AssetNo', name: 'Asset No ' },
        { value: 'Status', name: 'Status ' },
        { value: 'Brand', name: 'Brand ' },
        { value: 'CountryOfOrigin', name: 'Country Of Origin ' },
        { value: 'Vendor', name: 'Vendor ' }
    ];
    $scope.FixedAssetSearchCol = "Description";
    $scope.FixedAssetSearchVal = "";
    $scope.WhereFixedAssetNeeded = '';
    $scope.FixedAssetList = [];
    $scope.getFixedAssetData = function () {
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: { 'column': $scope.FixedAssetSearchCol, 'value': $scope.FixedAssetSearchVal, 'ArticleId': $scope.selectednode.items[0].addInfo.ArticleId },
                url: $scope.path + 'SearchFixedAsset'

            }).then(function successCallback(response) {
                $scope.FixedAssetList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.ViewFixedAssetStatus = function (args) {
        try {

            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("id", "notEqual", $scope.selectednode.items[0].id));
            for (var i = 0; i < exists.length; i++) {
                if (exists[i].addInfo.FixedAssetRegisterId == args.data.Id) {
                    ShowResult("Asset has already been tagged with another workstation", 'failure');
                    return;
                }
            }

            $scope.selectednode.items[0].addInfo.FixedAssetRegisterId = args.data.Id;
            $scope.selectednode.items[0].addInfo.FixedAssetRegisterDesc = args.data.FixedAssetDesc;

            var eDialog = $("#dialogSearchFixedAsset").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
    }


    $scope.nodes = [];
    $scope.operationList = [];
    $scope.operationButtonClick = function (args) {
        $scope.selectednode = args;
        //$scope.selectednode.items[0].addInfo;
        $http({
            method: 'GET',
            url: 'IE/LineLayoutForProductionBulletin/GetOperationList?ProductionBulletinMasterId=' + $scope.SelectedLine.ProductionBulletinId + '&ProcessId=' + $scope.DailyProductionTargetNew.ProcessId,
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
            angular.element(document.querySelector("#modalOperationList")).modal("toggle");
        });
    }

    $scope.recordoperationdoubleclick = function (args) {

        try {
            $scope.selectednode.items[0].addInfo.MaterialMasterId = args.data.MaterialMasterId;
            $scope.selectednode.items[0].addInfo.MaterialMasterDesc = args.data.MaterialMasterDesc;
            $scope.selectednode.items[0].addInfo.ArticleId = args.data.ArticleId;
            $scope.selectednode.items[0].addInfo.ArticleDesc = args.data.ArticleDesc;
            $scope.selectednode.items[0].addInfo.ArticleShortName = args.data.ArticleShortName;
            $scope.selectednode.items[0].addInfo.OperationId = args.data.OperationId;
            $scope.selectednode.items[0].addInfo.OperationDesc = args.data.OperationDesc;
            $scope.selectednode.items[0].addInfo.OperationVariationId = args.data.OperationVariationId;
            $scope.selectednode.items[0].addInfo.OperationVariationDesc = args.data.OperationVariationDesc;
            $scope.selectednode.items[0].addInfo.MachineOrHand = args.data.IsMachineRequired;
            $scope.selectednode.items[0].addInfo.TotalSPT = args.data.TotalSPT;
            //$scope.selectednode.items[0].addInfo.WorkstationTargetPerHour = args.data.WorkstationTargetPerHour;

            $scope.selectednode.items[0].addInfo.FixedAssetRegisterId = null;
            $scope.selectednode.items[0].addInfo.FixedAssetRegisterDesc = null;
            $scope.selectednode.items[0].addInfo.EmployeeId = null;
            $scope.selectednode.items[0].addInfo.EmployeeName = null;
            $scope.selectednode.items[0].addInfo.EmpPicPath = null;
            $scope.selectednode.items[0].addInfo.Designation = null;
            $scope.selectednode.items[0].addInfo.EmployeeCode = null;
            $scope.selectednode.items[0].addInfo["DayStatus"] = null;
            $scope.selectednode.items[0].addInfo["DayColor"] = null;

            angular.element(document.querySelector("#modalOperationList")).modal("hide");
        } catch (e) {

        }
    }

    $scope.EmployeeSearchFrom = 'card';
    $scope.employeeButtonClick = function (args, source, nodename) {
        $scope.EmployeeSearchFrom = source;

        if (angular.isUndefinedOrNull(nodename) == false) {
            var exists = ej.DataManager($scope.nodes).executeLocal(ej.Query().where("name", "equal", nodename));
            if (exists)
                $scope.selectednode = { "items": exists };
        }
        else {
            $scope.selectednode = args;
        }
        $scope.OpenEmployeeSearchBox();
    }
    $scope.FixedAssetButtonClick = function (args) {

        $scope.selectednode = args;
        $scope.OpenFixedAssetSearchBox();
    }
    $scope.ViewEmployeeCard = function (args) {

        $scope.selectednode = args;
        $scope.GetEmployeeCard();
    }

    $scope.ExplicitSave = false;
    $scope.SaveDiagram = function () {
        var _explicitSave = $scope.ExplicitSave;
        $scope.ExplicitSave = false;
        try {

            $http({
                method: 'POST',
                url: $scope.path + "SaveDiagram",
                data: {
                    'Nodes': $scope.nodes, 'Design': JSON.stringify($scope.nodes),
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    //if (_explicitSave)
                    ShowResult(response.data.Message, 'success');

                    $scope.SelectedLine.ManPowerWithMachine = response.data.Data[0].TotalMachine;
                    $scope.SelectedLine.ManPowerWithHand = response.data.Data[0].TotalHand;
                    $scope.SelectedLine.HasLayout = true;

                    //var gridObj = $("#GridDailyTargetList").data("ejGrid");
                    //gridObj.refreshContent();
                    //$scope.getDailytarget();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    }
    $scope.showCardIcons = false;
    $scope.EmployeeCard = [];
    $scope.EmployeeCardSkills = [];
    $scope.GetEmployeeCard = function () {
        $scope.EmployeeCard = [];
        $scope.EmployeeCardSkills = [];
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'EmployeeId': $scope.selectednode.items[0].addInfo.EmployeeId,
                    'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId,
                    'AssetRegisterId': $scope.selectednode.items[0].addInfo.FixedAssetRegisterId,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'GetEmployeeCard'

            }).then(function successCallback(response) {
                $scope.EmployeeCard = response.data;
                $scope.EmployeeCardSkills = response.data[0][0].SkillList;
            });
            var eDialog = $("#dialogEmployeeCard").data("ejDialog");
            $("#dialogEmployeeCard").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
            $("#dialogEmployeeCard").ejDialog("refresh");
            eDialog.open();
        } catch (e) {

        }
    }

    $scope.contextMenu = { items: [{ "id": "Properties", "name": "Properties", "text": "Properties", "image": "", "style": "" }] };
    $scope.onDiagramContextMenuClick = function (args) {
        if (args.text == 'Properties') {
            $scope.selectednode = args;
            $scope.ShowEditEmployeeCard();
        }
    }
    $scope.UpdateColor = function (args) {
        if (args.isInteraction == false)
            return;
        var diagram = $("#diagram").ejDiagram("instance");
        $scope.selectednode.target[args.model.Field] = args.value;

        var Property = args.model.Field;
        try {
            if ($scope.selectednode.target.hasOwnProperty('children')) {
                for (var i = 0; i < $scope.selectednode.target.children.length; i++) {
                    diagram.updateNode($scope.selectednode.target.children[i], { Property: args.value });
                }
            }
            else {
                diagram.updateNode($scope.selectednode.target.name, { Property: args.value });
            }
        } catch (e) { }
    }
    $scope.ShowEditEmployeeCard = function () {
        try {

            var eDialog = $("#dialogEditNode").data("ejDialog");
            $("#dialogEditNode").ejDialog("refresh");
            $("#dialogEditNode").ejDialog("refresh");

            eDialog.open();
        } catch (e) {

        }
    }

    $scope.UpdateEmployeeAttendanceAndProductionInfo = function () {
        try {

            var empIds = "''";
            for (var i = 0; i < $scope.nodes.length; i++) {
                empIds += ",'" + $scope.nodes[i].addInfo.EmployeeId + "'";
            }

            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'EmployeeId': empIds,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate
                },
                url: $scope.path + 'UpdateEmployeeAttendanceAndProductionInfo'

            }).then(function successCallback(response) {
                for (var i = 0; i < $scope.nodes.length; i++) {
                    var exists = ej.DataManager(response.data).executeLocal(ej.Query().where("EmployeeId", "equal", $scope.nodes[i].addInfo.EmployeeId));
                    if (exists.length > 0) {
                        $scope.nodes[i].addInfo["DayStatus"] = exists[0].DayStatus;
                        $scope.nodes[i].addInfo["DayColor"] = exists[0].DayColor;
                        $scope.nodes[i].addInfo["ProductionQuantity"] = exists[0].ProductionQuantity;
                    }
                }

                $scope.SaveDiagram();
            });

        } catch (e) {

        }
    }


    //chang employee
    $scope.ReplaceEmployeeList = [];
    $scope.GetListOfReplaceEmployees = function () {

        $scope.ConstructReplaceEmployee();

        var eDialog = $("#dialogEmployeeReplace").data("ejDialog");
        $("#dialogEmployeeReplace").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
        $("#dialogEmployeeReplace").ejDialog("refresh");
        eDialog.open();

    }
    $scope.ConstructReplaceEmployee = function () {
        $scope.ReplaceEmployeeList = [];
        for (var i = 0; i < $scope.nodes.length; i++) {
            var model = Object.assign({}, $scope.nodes[i].addInfo);
            if (model.hasOwnProperty('EmployeeId')) {
                if (model.DayStatus == 'A' || model.DayStatus == 'LV' || angular.isUndefinedOrNull(model.DayStatus)) {
                    model["name"] = $scope.nodes[i]["name"];
                    $scope.ReplaceEmployeeList.push(model);
                }
            }
        }

        try {
            var gridObj = $("#gridEmployeeReplace").data("ejGrid");
            gridObj.refreshContent();
        } catch (e) {

        }

    }

    $scope.ProductionEntryList = [];
    $scope.ConstructProductionEntry = function () {
        $scope.ProductionEntryList = [];
        for (var i = 0; i < $scope.nodes.length; i++) {
            var model = Object.assign({}, $scope.nodes[i].addInfo);
            if (model.hasOwnProperty('EmployeeId')) {
                if (angular.isUndefinedOrNull(model["EmployeeId"]))
                    continue;

                model["name"] = $scope.nodes[i]["name"];
                model["CurrentQuantity"] = 0;
                $scope.ProductionEntryList.push(model);
            }
        }

        try {
            var gridObj = $("#gridProductionEntry").data("ejGrid");
            gridObj.refreshContent();

            var eDialog = $("#dialogProductionEntry").data("ejDialog");
            $("#dialogProductionEntry").ejDialog({ actionButtons: ["close", "minimize", "maximize"] });
            $("#dialogProductionEntry").ejDialog("refresh");
            eDialog.open();
        } catch (e) {

        }

    }


    $scope.SaveProductionQuantity = function () {
        try {

            $http({
                method: "POST",
                url: $scope.path + 'SaveProductionData',
                data: {
                    'ProductionData': $scope.ProductionEntryList,
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {
                var eDialog = $("#dialogProductionEntry").data("ejDialog");
                eDialog.close();
                $scope.UpdateEmployeeAttendanceAndProductionInfo();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.downloadgriddataUrl = 'GridReports/Download';
    $scope.DownloadReport = function () {
        try {
            var Entity = $("#ddlEntity option:selected").text();
            var Process = $("#ProcessId option:selected").text();
            $http({
                method: 'POST',
                url: 'Productions/MachineLayoutReport/Report',
                data: {
                    'EntityId': $scope.DailyProductionTargetNew.EntityId,
                    'ProcessId': $scope.DailyProductionTargetNew.ProcessId,
                    'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLine.WorkCenterMasterId,
                    'Data': $scope.SelectedLine, 'EntityName': Entity, 'ProcessName': Process, 'WithEmp': $scope.WithEmployee, 'WithMachine': $scope.WithMachine
                }
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    $rootScope.report($scope.downloadgriddataUrl + "?FileName=" + response.data.FileName);
                }
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


    //chart
    $scope.graphmaxheight = function (list, column) {
        var _graphmaxheight = 10;
        _graphmaxheight = 10;
        for (var i = 0; i < list.length; i++) {
            if (list[i][column] > _graphmaxheight)
                _graphmaxheight = list[i][column];
        }

        return _graphmaxheight + (_graphmaxheight * .30);
    }

    $scope.graphmaxwidth = function (list, width) {
        if (baseService.isUndefinedOrNull(width))
            width = 100;

        return ((list.length * width) + 100) + 'px';
    }
    $scope.StripLineSetting = [];
    $scope.BottleneckData = [];
    $scope.BottleneckValue = 0;
    $scope.GetBottleneck = function (data) {
        $scope.SelectedLineForPR = data;
        $scope.StripLineSetting = [];
        $scope.BottleneckData = [];
        try {
            var eDialog = $("#dialogBottleneckGraph").data("ejDialog");
            eDialog.open();
            $http({
                method: "POST",
                url: $scope.path + 'GetBottleneck',
                data: {
                    'ProcessId': $scope.DailyProductionTargetNew.ProcessId,
                    'ProductionOrderId': $scope.SelectedLineForPR.PRNo,
                    'TargetDate': $scope.DailyProductionTargetNew.ProductionDate,
                    'WorkCenterMasterId': $scope.SelectedLineForPR.WorkCenterMasterId
                },
                dataType: 'JSON',
            }).then(function successCallback(response) {


                $scope.BottleneckData = response.data.GraphData;

                var LowerBoundValue = response.data.StripLine[0].LowerBoundValue;
                var LowerBoundText = response.data.StripLine[0].LowerBoundText;
                var UpperBoundValue = response.data.StripLine[0].UpperBoundValue;
                var UpperBoundText = response.data.StripLine[0].UpperBoundText;

                $scope.BottleneckValue = LowerBoundValue;

                if (LowerBoundValue > 0) {
                    $scope.StripLineSetting.push({ start: 0, end: LowerBoundValue, text: '', textAlignment: 'middlecenter', color: '#F5B7B1', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });
                    if (LowerBoundValue < 100)
                        $scope.StripLineSetting.push({ start: LowerBoundValue, end: UpperBoundValue, text: '', textAlignment: 'middlecenter', color: '#FCF3CF', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });

                }
                if (UpperBoundValue < 100)
                    $scope.StripLineSetting.push({ start: UpperBoundValue, end: 100, text: '', textAlignment: 'middlecenter', color: '#D5F5E3', font: { size: '18px', color: 'blue' }, zIndex: 'behind', borderWidth: 0, visible: true });



                var chartObj = $("#ChartBottleneck").data("ejChart");
                chartObj.redraw();
            });
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.chartBottleneckPreRender = function (args) {

        try {
            var points = args.model.series[0].points;//WIP

            for (var i = 0; i < points.length; i++) {
                points[i].fill = args.model.series[0].dataSource[i].Color;
                if (points[i].y < $scope.BottleneckValue)
                    points[i].fill = "#ff0000";
            }
        } catch (e) {

        }
    }

}