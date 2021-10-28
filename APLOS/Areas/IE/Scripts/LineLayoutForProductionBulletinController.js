'use strict';
LineLayoutForProductionBulletinController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', "$controller"];
function LineLayoutForProductionBulletinController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $scope.path = 'IE/LineLayoutForProductionBulletin/'
    $rootScope.title = 'Line Layout For Production Bulletin';

    $scope.nodes = [];
    $scope.operationList = [];
    $scope.operationButtonClick = function (args) {
        $scope.selectednode = args;
        $http({
            method: 'GET',
            url: $scope.path + 'GetOperationList'
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
            angular.element(document.querySelector("#modalOperationList")).modal("toggle");
        });
    }

    $scope.employeeButtonClick = function (args) {

        $scope.selectednode = args;
        $scope.OpenEmployeeSearchBox();
    }
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

    $scope.LineLayout = function (args) {
        try {
            $scope.nodes = [];
            $scope.NodeIndex = 0;
            $scope.NodeSeed = (new Date()).getTime();

            $scope.modelNew.ProductionOrderId = args.data.POId;
            $scope.modelNew.ProductionBulletinTemplateMasterId = args.data.ProductionBulletinTemplateMasterId;
            $scope.modelNew.BaseProcess = args.data.BaseProcess;
            $http({
                method: "GET",
                dataType: 'JSON',
                url: $scope.path + 'GetSaveData?BulletinId=' + args.data.ProductionBulletinTemplateMasterId,
            }).then(function successCallback(response) {
                if (response.data.length > 0) {
                    angular.element(document.querySelector('#POItemPopup')).modal('hide');
                    response.data = JSON.parse(response.data[0].Layout);
                    var diagram = $("#diagram").ejDiagram("instance");
                    diagram.clear();
                    diagram.add(response.data);

                    $scope.GetProductionPlanningData('', $scope.modelNew.ProductionOrderId, $scope.modelNew.BaseProcess);
                }
                else {
                    $scope.GetData();
                }
            });
            //$http({
            //    method: "GET",
            //    dataType: 'JSON',
            //    url: $scope.path + 'GetAllData?BulletinId=' + args.data.ProductionBulletinTemplateMasterId,
            //}).then(function successCallback(response) {
            //    angular.element(document.querySelector('#POItemPopup')).modal('hide');
            //    for (var i = 0; i < response.data.length; i++) {
            //        try {


            //            response.data[i].type = ej.datavisualization.Diagram.Shapes[response.data[i].type];

            //            try {
            //                for (var l = 0; l < response.data[i].labels.length; i++) {
            //                    response.data[i].labels[k].textAlign = ej.datavisualization.Diagram.TextAlign[response.data[i].labels[k].textAlign];
            //                }
            //            } catch (e) {

            //            }
            //        } catch (e) {

            //        }
            //    }

            //    $scope.nodes = response.data;
            //    var diagram = $("#diagram").ejDiagram("instance");               
            //    diagram.add(response.data);
            //    $scope.GetProductionPlanningData('', $scope.modelNew.ProductionOrderId, args.data.BaseProcess);
            //});
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };

    $scope.GetData = function () {
        try {
            $http({
                method: "GET",
                dataType: 'JSON',
                url: $scope.path + 'GetAllData?BulletinId=' + $scope.modelNew.ProductionBulletinTemplateMasterId,
            }).then(function successCallback(response) {
                angular.element(document.querySelector('#POItemPopup')).modal('hide');
                for (var i = 0; i < response.data.length; i++) {
                    try {


                        response.data[i].type = ej.datavisualization.Diagram.Shapes[response.data[i].type];

                        try {
                            for (var l = 0; l < response.data[i].labels.length; i++) {
                                response.data[i].labels[k].textAlign = ej.datavisualization.Diagram.TextAlign[response.data[i].labels[k].textAlign];
                            }
                        } catch (e) {

                        }
                    } catch (e) {

                    }
                }

                $scope.nodes = response.data;
                var diagram = $("#diagram").ejDiagram("instance");
                diagram.clear();
                diagram.add(response.data);
                $scope.GetProductionPlanningData('', $scope.modelNew.ProductionOrderId, $scope.modelNew.BaseProcess);
            });
        } catch (e) {
            ShowResult(e, 'failure');
        }
    };


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
        { value: 'SubSection', name: 'Sub Section ' }
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
                data: { 'column': $scope.EmployeeSearchCol, 'value': $scope.EmployeeSearchVal },
                url: 'IE/LineDesigner/SearchEmployee'

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

            var eDialog = $("#dialogSearchEmployee").data("ejDialog");
            eDialog.close();
        } catch (e) {

        }
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

            angular.element(document.querySelector("#modalOperationList")).modal("hide");
        } catch (e) {

        }
    }
    $scope.selectarticle = function (args) {
        try {
            $scope.selectednode.items[0].addInfo.ArticleId = args.Id;
            $scope.selectednode.items[0].addInfo.ArticleDesc = args.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
        } catch (e) {

        }
    }

    $scope.contextMenu = { items: [{ "id": "Properties", "name": "Properties", "text": "Properties", "image": "", "style": "" }] };
    $scope.onDiagramContextMenuClick = function (args) {
        if (args.text == 'Properties') {
            $scope.selectednode = args;
            $scope.ShowEditOperationVariationCard();
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
    $scope.ShowEditOperationVariationCard = function () {
        try {

            var eDialog = $("#dialogEditNode").data("ejDialog");
            eDialog.open();
        } catch (e) {

        }
    }

    $scope.showCardIcons = false;
    $scope.OperationVariationCard = [];
    $scope.OperationVariationCardSkills = [];
    $scope.ViewOperationVariationCard = function (args) {

        $scope.selectednode = args;
        $scope.GetOperationVariationCard();
    }
    $scope.GetOperationVariationCard = function () {
        $scope.OperationVariationCard = [];
        $scope.OperationVariationCardSkills = [];
        try {
            $http({
                method: "POST",
                dataType: 'JSON',
                data: {
                    'OperationVariationId': $scope.selectednode.items[0].addInfo.OperationVariationId
                },
                url: $scope.path + 'GetOperationVariationCard'

            }).then(function successCallback(response) {
                $scope.OperationVariationCard = response.data;
            });
            var eDialog = $("#dialogOperationVariationCard").data("ejDialog");
            eDialog.open();
        } catch (e) {

        }
    }

    $scope.entityList = [];
    $scope.getAllEntities = function () {
        $http({
            method: 'POST',
            url: "OrderManagements/productionOrderSchedulingParametersType1/GetEntity"
        }).then(function successCallback(response) {
            $scope.entityList = response.data;
            if (baseService.arrayLength(response.data) === 1) {
                $scope.modelNew.ProductionEntityId = $scope.entityList[0].Value;
                //default                
            }
        });
    }
    $scope.getAllEntities();

    $scope.ProductionOrderList = [];
    $scope.getProductionOrderPopUp = function () {
        if ($scope.modelNew.ProductionEntityId == null) {
            ShowResult("Select Production Entity..", 'failure');
        }
        $scope.ProductionOrderList = [];
        $http.get("IE/LineLayoutForProductionBulletin/GetProductionOrderDataList?entityId=" + $scope.modelNew.ProductionEntityId)
            .then(
                function successCallback(response) {
                    if (baseService.arrayLength(response.data) > 0) {
                        $scope.ProductionOrderList = response.data;
                    }
                },
                function errorCallback(response) {
                    ShowResult(response, 'failure');
                });
        angular.element(document.querySelector('#POItemPopup')).modal('show');
    };

    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.machineButtonClick = function (args) {
        $scope.selectednode = args;
        if (baseService.isUndefinedOrNull($scope.selectednode.items[0].addInfo.MaterialMasterId))
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.selectednode.items[0].addInfo.MaterialMasterId);
    };
    $scope.VWCDATA = [];
    $scope.GetProductionPlanningData = function (id, PRID, BaseProcess) {
        try {
            $http({
                method: 'POST',
                url: "OrderManagements/productionOrderSchedulingParametersType1/GetProductionPlanningData?planrowid=" + id + "&ProductionOrderId=" + PRID + "&processid=" + BaseProcess
            }).then(function successCallback(res) {

                $scope.VWCDATA = res.data.WCDATA;
                //getAllDisplayParameters();
            });
        } catch (e) {

        }
    }
    $scope.summaryRowsForWorkCenter = [{
        title: "Total Planned Qty", summaryColumns: [{ summaryType: ej.Grid.SummaryType.Sum, displayColumn: "PlannedQuantity", dataMember: "PlannedQuantity", format: "{0:N0}" }],
        showCaptionSummary: true

    }];

    $scope.graphmaxheight = 10;
    $scope.graphmaxwidth = '200px';
    $scope.dataSourceLineGraph = [];
    $scope.showlinegraph = function (args) {

        try {
            $scope.graphmaxwidth = '200px';
            $http({
                method: 'GET',
                url: "OrderManagements/productionOrderSchedulingParametersType1/GetProductionPlanGraph?orderid=" + args.data.ProductionOrderID + "&workcentrid=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {
                $scope.graphmaxheight = 10;
                for (var i = 0; i < res.data.length; i++) {
                    if (res.data[i].Quantity > $scope.graphmaxheight)
                        $scope.graphmaxheight = res.data[i].Quantity;
                }

                $scope.graphmaxwidth = ((res.data.length * 30) + 200) + 'px';
                $scope.graphmaxheight = $scope.graphmaxheight + ($scope.graphmaxheight * .10);

                $scope.dataSourceLineGraph = res.data;

                $("#graph").ejDialog("setTitle", "Production Plan for Workcenter [" + args.data.WorkCenter + "], Production Order#" + args.data.ProductionOrderID);
                var eDialog = $("#graph").data("ejDialog");
                eDialog.open();
            });



        } catch (e) {

        }
    }
    $scope.WORKCENTERPARAMS = {};
    $scope.WORKCENTERProductList = [];
    $scope.workcenterclick = function (args) {
        try {
            $http({
                method: 'GET',
                url: "OrderManagements/productionOrderSchedulingParametersType1/getWorkcenterParametersDisplay?WorkCenterMasterId=" + args.data.WorkCenterMasterId
            }).then(function successCallback(res) {

                $scope.WORKCENTERPARAMS = res.data.WORKCENTERPARAMS[0];
                $scope.WORKCENTERProductList = res.data.WORKCENTERProductList;

                $("#dialogWorkCenterParameters").ejDialog("setTitle", "Configurations for Work Center [" + $scope.WORKCENTERPARAMS.WorkCenter + "]");
                var eDialog = $("#dialogWorkCenterParameters").data("ejDialog");
                eDialog.open();
            });
        } catch (e) {

        }
    }
    $scope.printChart = function (chartname) {
        var chartObj = $('#' + chartname).ejChart("instance");
        chartObj.print(chartname);
    }
    $scope.Save = function () {
        try {

            $http({
                method: 'POST',
                url: $scope.path + "Save",
                data: {
                    'Nodes': $scope.nodes, Design: JSON.stringify($scope.nodes),
                    ProductionBulletinTemplateMasterId: $scope.modelNew.ProductionBulletinTemplateMasterId,
                    EntityId: $scope.modelNew.ProductionEntityId, ProductionOrderId: $scope.modelNew.ProductionOrderId
                    , ProcessId: $scope.modelNew.BaseProcess
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
    };
}