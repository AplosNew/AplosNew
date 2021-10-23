
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
        $scope.ShowDiv = true;
        var eDialog = $("#LineDesign").data("ejDialog");
        if (obj.data.HasLayout == false) {
            $scope.CopyTable(obj.data);
        }
        else {
            $scope.GetLineLayout(obj.data);
        }
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
                    ShowResult(response.data.Message, 'success');
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
                data: { 'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal, 'OperationId': $scope.selectednode.items[0].addInfo.OperationId, 'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId },
                url: $scope.path + 'SearchEmployee'

            }).then(function successCallback(response) {
                $scope.EmployeeList = response.data;

            });
        } catch (e) {

        }
    }
    $scope.ViewEmployeeStatus = function (args) {
        try {
            $scope.selectednode.items[0].addInfo.EmployeeId = args.data.Id;
            $scope.selectednode.items[0].addInfo.EmployeeName = args.data.EmployeeName;
            $scope.selectednode.items[0].addInfo.EmpPicPath = args.data.EmpPicPath;
            $scope.selectednode.items[0].addInfo.Designation = args.data.Designation;
            $scope.selectednode.items[0].addInfo.EmployeeCode = args.data.EmployeeCode;

            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
    }

    $scope.nodes = [];
    $scope.operationList = [];
    $scope.operationButtonClick = function (args) {
        $scope.selectednode = args;
        $http({
            method: 'GET',
            url: 'IE/LineLayoutForProductionBulletin/GetOperationList'
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
            angular.element(document.querySelector("#modalOperationList")).modal("toggle");
        });
    }

    $scope.employeeButtonClick = function (args) {

        $scope.selectednode = args;
        $scope.OpenEmployeeSearchBox();
    }

    $scope.SaveDiagram = function () {
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
                    ShowResult(response.data.Message, 'success');

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        } catch (e) {
            ShowResult(e, "failure");
        }
    }
}