
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
                data: { 'entityid': $scope.DailyProductionTargetNew.EntityId, 'processId': $scope.DailyProductionTargetNew.ProcessId, 'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate, 'SelectedLine': $scope.SelectedLineForPR},
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

    $scope.GetLineLayout = function (data) {
        try {
            $scope.SelectedLineForPR = data;
            $http({
                method: "POST",
                url: $scope.path + 'GetLineLayoutData',
                data: { 'entityid': $scope.DailyProductionTargetNew.EntityId, 'processId': $scope.DailyProductionTargetNew.ProcessId, 'ProductionDate': $scope.DailyProductionTargetNew.ProductionDate, 'SelectedLine': $scope.SelectedLineForPR},
                dataType: 'JSON',
            }).then(function successCallback(response) {
                response.data = JSON.parse(response.data[0].Layout);
                $scope.nodes = response.data;
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
            spriteCss: "glyphicon glyphicon-list-alt",
        }, {
            id: "RoundedRectangle_Tool", tooltiptext: "RoundRect",
            spriteCss: "icon-RoundedRectangle toolBarIconStyle",
        }, {
            id: "Ellipse_Tool", tooltiptext: "Ellipse",
            spriteCss: "icon-Ellipse toolBarIconStyle",
        }
        , {
            id: "LeftUpArrow", tooltiptext: "Left Up Arrow",
            spriteCss: "icon-LeftUpArrow",
        }
        , {
            id: "UpArrow", tooltiptext: "UpArrow",
            spriteCss: "glyphicon glyphicon-arrow-up",
        }
        , {
            id: "UpRightArrow", tooltiptext: "UpRightArrow",
            spriteCss: "icon-UpRightArrow",
        }
        , {
            id: "RightArrow", tooltiptext: "RightArrow",
            spriteCss: "glyphicon glyphicon-arrow-right blue",
        }
        , {
            id: "RightDownArrow", tooltiptext: "RightDownArrow",
            spriteCss: "icon-RightDownArrow",
        }
        , {
            id: "DownArrow", tooltiptext: "DownArrow",
            spriteCss: "glyphicon glyphicon-arrow-down",
        }
        , {
            id: "LeftDownArrow", tooltiptext: "LeftDownArrow",
            spriteCss: "icon-LeftDownArrow",
        }
        , {
            id: "LeftArrow", tooltiptext: "LeftArrow",
            spriteCss: "glyphicon glyphicon-arrow-left",
        },
        {
            id: "Textbox_Tool", tooltiptext: "Textbox",
            spriteCss: "icon-Textbox toolBarIconStyle",
        },
        {
            id: "Path_Tool", tooltiptext: "Path",
            spriteCss: "icon-Path toolBarIconStyle",
        },
        {
            id: "Image_Tool", tooltiptext: "Image",
            spriteCss: "icon-Image toolBarIconStyle",
        },
        {
            id: "Html_Tool", tooltiptext: "Html",
            spriteCss: "glyphicon glyphicon-list-alt",
        },
    ];

    $scope.width = "100%";
    $scope.height = "300px";

    $scope.pageSettings = { scrollLimit: "diagram", boundaryConstraints: ej.datavisualization.Diagram.BoundaryConstraints.Diagram };


    $scope.selectednode = null;

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
            case "LeftUpArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M0 23l8-8 17 17 7-7-17-17 8-8h-23v23z" };
                break;
            case "UpArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 1l-15 15h9v16h12v-16h9z" };
                break;
            case "UpRightArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M9 0l8 8-17 17 7 7 17-17 8 8v-23h-23z" };
                break;
            case "RightArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M31 16l-15-15v9h-16v12h16v9z" };
                break;
            case "RightDownArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M32 9l-8 8-17-17-7 7 17 17-8 8h23v-23z" };
                break;
            case "DownArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M16 31l15-15h-9v-16h-12v16h-9z" };
                break;
            case "LeftDownArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M23 32l-8-8 17-17-7-7-17 17-8-8v23h23z" };
                break;
            case "LeftArrow":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M1 16l15 15v-9h16v-12h-16v-9z" };
                break;
            case "Polygon_Tool":
                diagram.model.drawType = { type: "basic", shape: "polygon", points: [{ x: 13.560, y: 67.524 }, { x: 21.941, y: 41.731 }, { x: 0.000, y: 25.790 }, { x: 27.120, y: 25.790 }, { x: 35.501, y: 0.000 }, { x: 43.882, y: 25.790 }, { x: 71.000, y: 25.790 }, { x: 49.061, y: 41.731 }, { x: 57.441, y: 67.524 }, { x: 35.501, y: 51.583 }, { x: 13.560, y: 67.524 }] };
                break;
            case "Path_Tool":
                diagram.model.drawType = { type: "basic", shape: "path", pathData: "M78.631,3.425c-0.699-1.229-2.177-2.653-5.222-2.394c-8.975,0.759-26.612,16.34-30.804,22.411c-0.167-0.79-0.551-2.049-1.377-2.741c1.176-2.069,3.035-5.709,3.813-9.156c0.18-0.044,0.338-0.161,0.385-0.41c0.083-0.423,0.146-0.848,0.23-1.268c0.135-0.706-0.962-0.944-1.086-0.245c-0.078,0.431-0.158,0.852-0.234,1.286c-0.04,0.26,0.076,0.464,0.26,0.569c-0.756,3.361-2.575,6.93-3.737,8.975c-0.2-0.105-0.415-0.189-0.661-0.224c-0.07-0.009-0.132,0.005-0.199,0.003c-0.067,0.002-0.129-0.012-0.199-0.003c-0.246,0.035-0.461,0.119-0.661,0.224c-1.162-2.045-2.981-5.613-3.737-8.975c0.185-0.104,0.301-0.309,0.26-0.569c-0.076-0.434-0.156-0.855-0.234-1.286c-0.124-0.699-1.221-0.46-1.086,0.245c0.085,0.42,0.147,0.845,0.23,1.268c0.047,0.249,0.205,0.367,0.385,0.41c0.777,3.446,2.637,7.087,3.813,9.156c-0.826,0.692-1.21,1.951-1.377,2.741C33.203,17.371,15.566,1.789,6.591,1.031C3.546,0.772,2.068,2.196,1.369,3.425c-0.818,1.407-0.185,4.303,0.993,9.321c0.53,2.228,1.075,4.521,1.465,6.779c0.208,1.239,0.404,2.471,0.59,3.65c0.819,5.33,1.503,9.766,3.714,11.187c0.606,0.39,1.313,0.55,2.179,0.442c2.107-0.46,4.627-0.845,7.293-1.12c-2.613,1.77-5.88,4.65-6.953,8.474c-0.827,2.989-0.175,6.031,1.932,9.083c2.482,3.569,5.027,5.915,7.406,7.444c4.756,3.057,8.874,2.843,10.613,2.75c0.179-0.002,0.318-0.014,0.453-0.018c1.324-0.017,2.23-1.868,4.161-6.064c0.948-2.044,2.358-5.088,3.546-6.638c0.249,0.57,0.96,0.972,1.331,1.085c-0.03,0.014-0.067,0.039-0.094,0.049c0.034-0.007,0.074-0.03,0.111-0.042c0.022,0.006,0.055,0.023,0.074,0.027c-0.017-0.006-0.046-0.022-0.066-0.03c0.391-0.131,0.876-0.532,1.119-1.088c1.188,1.549,2.598,4.594,3.546,6.638c1.931,4.196,2.838,6.047,4.161,6.064c0.135,0.004,0.274,0.016,0.453,0.018c1.739,0.093,5.857,0.307,10.613-2.75c2.379-1.529,4.924-3.875,7.406-7.444c2.106-3.052,2.759-6.094,1.932-9.083c-1.073-3.823-4.34-6.704-6.953-8.474c2.667,0.274,5.186,0.659,7.293,1.12c0.866,0.108,1.573-0.053,2.179-0.442c2.211-1.421,2.895-5.857,3.714-11.187c0.185-1.18,0.382-2.411,0.59-3.65c0.39-2.258,0.935-4.551,1.465-6.779C78.816,7.728,79.448,4.832,78.631,3.425z M41.184,48.732c-0.343,0.551-0.781,0.918-1.082,1.065c-0.324-0.135-0.933-0.497-1.286-1.065c0,0-1.506-19.959-1.349-24.911c0,0,0.509-3.147,2.533-3.169c2.024,0.022,2.533,3.169,2.533,3.169C42.69,28.773,41.184,48.732,41.184,48.732z" };
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

        var tool = diagram.tool();
        diagram.update({ tool: tool | ej.datavisualization.Diagram.Tool.DrawOnce })
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

}