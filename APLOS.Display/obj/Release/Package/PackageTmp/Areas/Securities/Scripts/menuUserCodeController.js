'use strict';
menuUserCodeController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService', '$sce'];
function menuUserCodeController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService, $sce) {
    $scope.isManualFilter = false;
    $scope.MenuList = [];
    $scope.MenuListTemp = [];
    $scope.companyGroupId = null;
    $scope.actionList = [];
    $scope.companyGroupList = [];
    $scope.menuHierarchy = [];
    $scope.companyGroupMenuMaster = {
        Id: null,
        ModuleId: null,
        CompanyGroupId: null,
        MenuFrameId: null
    };
    $scope.Attachmenttab = 1;
    $scope.AttachmentsetTab = function (newTab) {
        $scope.Attachmenttab = newTab;
    };
    $scope.AttachmentisSet = function (tabNum) {
        return $scope.Attachmenttab === tabNum;

    };
    $scope.SelectedItemsModel = {
        Module: null,
        MenuFrame: null,
        MenuGroup: null,
        MenuSubGroup: null,
        PanelName: null
    };
    $scope.SelectedItems = Object.assign({}, $scope.SelectedItemsModel);
    $scope.EditButttonShow = false;
    $scope.disable = true;
    $scope.path = 'Securities/MenuUserCode/';
    $scope.getListUrl = $scope.path + 'GetMenuDetailList';
    $scope.getActionListUrl = $scope.path + 'GeActionListByMenu?menuId=';
    $scope.getMenuSeqUrl = $scope.path + 'GetAutoMenuSequence';
    $scope.actionDeleteUrl = $scope.path + 'DeleteMenuAction/';

    $scope.ModuleList = [];
    $scope.companyGroupList = [];
    $scope.companyGroupMenuMaster = {
        Id: null,
        ModuleId: null,
        CompanyGroupId: null,
        MenuFrameId: null
    };

    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboModuleByCompanyGroup = function () {
        cboService.getCboModuleByCompanyGroup($scope.companyGroupMenuMaster.CompanyGroupId, function (result) {
            $scope.moduleList = result;
        });
    };

    $scope.AplosCorePath = '';
    $scope.GetAplosCoreUrl = function () {
        $http({
            method: 'GET',
            url: 'Menus/MenuSync/GetAplosCoreUrl'
        }).then(function successCallback(response) {
            $scope.AplosCorePath = response.data;
        });
    };
    $scope.GetAplosCoreUrl();
    $scope.menuFarmeGet = function () {
        $http({
            method: 'GET',
            url: 'Menus/menumaster/getmenuframebymoduleidcbo?moduleId=' + $scope.companyGroupMenuMaster.ModuleId
        }).then(function successCallback(response) {
            $scope.menuFrameList = response.data;
        });
    };
    cboService.getCboModule(function (result) {
        $scope.ModuleList = result;
    });

    $scope.SubModuleList = [];
    cboService.getCboSubModule(function (result) {
        $scope.SubModuleList = result;
    });
    $scope.menuFrameList = [];
    $scope.menuGroupList = [];
    $scope.menuSubGroupList = [];

    $http({
        method: 'GET',
        url: 'Menus/menuframe/getmenuframecbo'
    }).then(function successCallback(response) {
        $scope.menuFrameList = response.data;
    });

    $http({
        method: 'get',
        url: 'Menus/menugroup/getmenugroupcbo'
    }).then(function successCallback(response) {
        $scope.menuGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/menusubgroup/getmenusubgroupcbo'
    }).then(function successCallback(response) {
        $scope.menuSubGroupList = response.data;
    });


    $scope.menuMasterModel = {
        MenuName: null,
        Id: null,
        ModuleId: null,
        MenuItemGroup: null,
        Sequence: null,
        MenuFrameId: null,
        MenuGroupId: null,
        MenuSubGroupId: null,
        PanelName: null,
        Description: null,
        IsExternalMenu: false,
        Remarks: null,
        Active: true,
        Code: null,
        UserCode: null,
        Controller: null,
        Href: null
    };
    $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);


    $scope.hierarchy = function () {
        var x = $scope.menuMaster;
        $http({
            method: 'GET',
            url: 'Menus/MenuCreation/GeMenuHierarchy?panelname=' + $scope.SelectedItems.PanelName
        }).then(function successCallback(response) {
            $scope.menuHierarchy = response.data.DATA;

            for (var i = 0; i < response.data.MASTER.length; i++) {
                $scope.menuHierarchy.push(response.data.MASTER[i]);
            }
        });
    }



    cboService.getCboCompanyGroup(function (result) {
        $scope.companyGroupList = result;
    });

    $scope.getCboModuleByCompanyGroup = function () {
        cboService.getCboModuleByCompanyGroup($scope.companyGroupMenuMaster.CompanyGroupId, function (result) {
            $scope.moduleList = result;
        });
    };

    $scope.menuFarmeGet = function () {
        $http({
            method: 'GET',
            url: 'Menus/menumaster/getmenuframebymoduleidcbo?moduleId=' + $scope.companyGroupMenuMaster.ModuleId
        }).then(function successCallback(response) {
            $scope.menuFrameList = response.data;
        });
    };


    $scope.GetMenuSyncFromAPI = function () {
        $http({
            method: 'POST',
            url: 'Menus/MenuSync/GetMenuListForSync'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MenuDetailList();
                //$scope.hierarchy();
            }
        });
    };


    $scope.MenuDetailList = function () {
        $http({
            method: 'GET',
            url: 'Securities/MenuUserCode/GetMenuDetailList?moduleId=' + $scope.companyGroupMenuMaster.ModuleId + '&MenuFrameId=' + $scope.companyGroupMenuMaster.MenuFrameId
        }).then(function successCallback(response) {
            for (var i = 0; i < response.data.length; i++) {
                response.data[i]["Image"] = $sce.trustAsHtml(response.data[i]["Image"]);
            }
            $scope.MenuList = response.data;
            $scope.MenuListTemp = response.data;
            $scope.hierarchy();
        });
    };
    $scope.MenuDetailList();

    $scope.changeBckCol = function (args) {
        try {
            if (args.data.MarkForDeletion == true)
                args.row.css("background-color", "#ff0000");
        } catch (e) {

        }
    };

    $scope.nodeclick = function (args) {
        try {
            var treeObj = $("#treeView").data("ejTreeView");
            var hasChild = treeObj.hasChildNode($("#" + args.id));
            if (hasChild)
                return;


            //$scope.uncheckAll();

            $http({
                method: 'GET',
                url: 'Menus/MenuCreation/GetMenu?id=' + args.id + "&panelname=" + $scope.SelectedItems.PanelName
            }).then(function successCallback(response) {

                $scope.menuMaster = response.data[0];
                $scope.GetCompanyGroup();
                getMenuActionList(args.id);
                var eDialog = $("#dialogEntry").data("ejDialog");
                eDialog.open();
            });

        } catch (e) {

        }
    }

    $scope.TreeExpandAll = function () {
        var treeObj = $("#treeView").data("ejTreeView");
        treeObj.expandAll();
    }
    $scope.TreeCollapseAll = function () {
        var treeObj = $("#treeView").data("ejTreeView");
        treeObj.collapseAll();
    }
    $scope.TreeSelectAll = function () {
        var treeObj = $("#treeView").data("ejTreeView");
        treeObj.checkAll();
    }
    $scope.TreeUnselectAll = function () {
        var treeObj = $("#treeView").data("ejTreeView");
        treeObj.unCheckAll();
    }
    $scope.TreeUpdateCheckedItems = function () {
        $scope.MenuListTemp = [];
        var treeObj = $("#treeView").data("ejTreeView");
        var row = treeObj.getTreeData();
        var MenuMasters = [];
        for (var i = 0; i < row.length; i++) {
            if (!row[i].NODETYPE)
                continue;

            if (row[i].NODETYPE == 'MENU' && row[i].isChecked == true)
                MenuMasters.push({ MenuMasterId: row[i].id });
        }
        //ej.DataManager(row).executeLocal(ej.Query().where("className", "equal", "e-item"));
        //var row = $filter('filter')($scope.MenuList, { 'isToBeSelect': true });
        //if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
        //    $scope.MenuListTemp = row;
        //    $scope.isManualFilter = true;
        //    $scope.MenuListTemp = ej.DataManager(row).executeLocal(ej.Query().select(["MenuMasterId", "ModuleId"]));
        //}
        $http({
            method: 'POST',
            url: 'Menus/MenuSync/UpdateCompanyGroupMenuMasterFromTree',
            data: { 'CompanyGroupMenuMaster': MenuMasters, 'PanelName': $scope.SelectedItems.PanelName },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MenuDetailList();

            }
        });


    }

    //------Multiple Selection(Excel)-------//
    $scope.saveCompanyGroupMenuMaster = function () {
        $scope.MenuListTemp = [];
        var row = $scope.MenuList; ///$filter('filter')($scope.MenuList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.MenuListTemp = row;
            $scope.isManualFilter = true;
            $scope.MenuListTemp = ej.DataManager(row).executeLocal(ej.Query().select(["MenuMasterId", "ModuleId", "UserDefineCode"]));
        }
        $http({
            method: 'POST',
            url: 'Securities/MenuUserCode/SaveCompanyGroupMenuMaster',
            data: { 'CompanyGroupMenuMaster': $scope.MenuListTemp },
            dataType: 'JSON'
        }).then(function successCallback(response) {
            if (response.data.Error === true) {
                ShowResult(response.data.Message, 'failure');
            }
            else {
                ShowResult(response.data.Message, 'success');
                $scope.MenuDetailList();

            }
        });


    };

    function checkChangeemployee(e) {

        var val = e.model.value;
        //item level check
        var row = $filter('filter')($scope.employeeAttendanceBySingleDateSelection, { 'Id': e.model.value });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (e.model.checkState == "check")
                row[0].Active = true;
            else
                row[0].Active = false;
        }

    }
    function headCheckChangeemployee(e) {
        if (e.model.checkState == "check") {

            // var gridObj = $("#Gridemployee").data("ejGrid");
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MenuList.length; i++) {

                    $scope.MenuList[i].isSelect = true;
                }
            }
            else {
                for (var i = 0; i < $scope.MenuList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MenuList[i].EmpSystemId == filtered[j].EmpSystemId)
                            // $scope.MenuList[i].isSelect = true;
                            $scope.MenuList[i].isToBeSelect = true;
                    }

                }
            }

            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": true });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        else {
            var filtered = $("#Gridemployee").data("ejGrid").getFilteredRecords();
            if (angular.isUndefinedOrNull(filtered) || filtered.length == 0) {
                for (var i = 0; i < $scope.MenuList.length; i++) {
                    $scope.MenuList[i].isToBeSelect = false;
                }
            }
            else {
                for (var i = 0; i < $scope.MenuList.length; i++) {
                    for (var j = 0; j < filtered.length; j++) {
                        if ($scope.MenuList[i].Id == filtered[j].Id)
                            $scope.MenuList[i].isToBeSelect = false;
                    }

                }
            }
            var checkbox = $("#Gridemployee .rowCheckbox").ejCheckBox();
            for (var i = 0; i < checkbox.length; i++) {
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": null });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "checked": false });
                $($("#Gridemployee .rowCheckbox")[i]).ejCheckBox({ "change": checkChangeemployee });
            }
        }
        //header level check
    }
    $scope.dataBoundemployee = function (args) {
        $("#Gridemployee .rowCheckbox").ejCheckBox({ "change": checkChange });
        $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });

    };
    $scope.refreshTemplateemployee = function (args) {
        if (args.rowIndex == 0) {
            $("#headchk").ejCheckBox({ "change": headCheckChangeemployee });
        }

        var valobj = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0];
        var val = $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox()[0].defaultValue;

        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": null });
        var row = $filter('filter')($scope.MenuList, { 'EmpSystemId': val });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            if (row[0].isToBeSelect == true)
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": true });
            else
                $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "checked": false });

        }
        $($("#Gridemployee .rowCheckbox")[args.rowIndex]).ejCheckBox({ "change": checkChangeemployee });
    };
    $scope.saveemployeedata = function () {
        $scope.MenuListTemp = [];
        var row = $filter('filter')($scope.MenuList, { 'isToBeSelect': true });
        if (!baseService.isUndefinedOrNull(row) && row.length > 0) {
            $scope.MenuListTemp = row;
            $scope.isManualFilter = true;
        }

    };
    //$scope.showEmployeeFilterScreen = function () {
    //    try {

    //        var gridObj = $("#Gridemployee").data("ejGrid");
    //        gridObj.clearFiltering();
    //        angular.element(document.querySelector('#empfilterPopUp')).modal('show');


    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }
    //};
    $scope.clearManualFilter = function () {
        $scope.isManualFilter = false;
        $scope.MenuListTemp = $scope.MenuList;
    };
    //$scope.Back = function () {
    //    angular.element(document.querySelector('#empfilterPopUp')).modal('hide');
    //};
    //--------------------------------------//

    //--------------Menu Edit/Update--------//

    //$scope.GetMenuData = function (args) {
    //    $scope.menuMaster = Object.assign({}, args.data);
    //    getMenuActionList($scope.menuMaster.Id);
    //    $scope.Action = 'Update';

    //    var eDialog = $("#dialogEntry").data("ejDialog");
    //    eDialog.open();
    //};

    $scope.nodeDragStop = function (args) {
        var treeObj = $("#treeView").data("ejTreeView");


        //if (treeObj.hasChildNode($("#" + args.draggedElementData.id)) == true)
        //    args.cancel = true;

        //if (treeObj.hasChildNode($("#" + args.targetElementData.id)) == false)
        //    args.cancel = true;


        var draggedElementData = ej.DataManager($scope.menuHierarchy).executeLocal(ej.Query().where("id", "equal", args.draggedElementData.id));
        var targetElementData = ej.DataManager($scope.menuHierarchy).executeLocal(ej.Query().where("id", "equal", args.targetElementData.id));

        if (draggedElementData.length == 0 || targetElementData.length == 0)
            args.cancel = true;

        if (draggedElementData[0].NODETYPE == 'CONTAINER')
            args.cancel = true;

        if (targetElementData[0].NODETYPE == 'MENU')
            args.cancel = true;


        var tree = treeObj.getParent(args.targetElementData.id);
        if (tree.length == 0)
            args.cancel = true;
    };
    $scope.nodeDropped = function (args) {
        var treeObj = $("#treeView").data("ejTreeView");
        var allSiblings = treeObj.getChildren(args.targetElementData.id);
        var siblingSequence = [];
        for (var i = 0; i < allSiblings.length; i++) {
            siblingSequence.push({ Sequence: (i + 1), Id: allSiblings[i].id });
        }

        var tree = treeObj.getParent(args.targetElementData.id);
        var path = [];
        path.push(args.droppedElementData.id);
        path.push(args.targetElementData.id);

        var id = args.targetElementData.id;
        while (tree.length > 0) {
            tree = treeObj.getParent(id);
            if (tree.length > 0) {
                id = tree[0].id;
                path.push(tree[0].id);
            }

        }

        var TargetHierarchy = ej.DataManager($scope.menuHierarchy).executeLocal(ej.Query().where("id", "equal", args.targetElementData.id));
        var SourceMenu = ej.DataManager($scope.menuHierarchy).executeLocal(ej.Query().where("id", "equal", args.droppedElementData.id));

        try {
            $http({
                method: 'POST',
                data: {
                    'NewHierarchy': TargetHierarchy[0], 'Menu': SourceMenu[0], 'PanelName': $scope.SelectedItems.PanelName, 'Siblings': siblingSequence
                },
                url: 'Menus/MenuCreation/UpdateMenuLocation'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {

                    ShowResult(response.data.Message, 'success');
                    //$scope.menuMaster = Object.assign({}, $scope.menuMasterModel);
                    //$scope.hierarchy();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            });
        } catch (e) {

        }
    };

    $scope.saveMenu = function () {
        try {
            //$scope.menuMaster.PanelName = $scope.SelectedItems.PanelName;

            var ddlMenuFrameList = $("#ddlMenuFrameList").data("ejDropDownList");
            var ddlMenuSubGroupList = $("#ddlMenuSubGroupList").data("ejDropDownList");
            var ddlMenuGroupList = $("#ddlMenuGroupList").data("ejDropDownList");
            var ddlModuleList = $("#ddlModuleList").data("ejDropDownList");


            $scope.menuMaster.MenuFrameId = ddlMenuFrameList.getSelectedValue();
            $scope.menuMaster.MenuGroupId = ddlMenuGroupList.getSelectedValue();
            $scope.menuMaster.MenuSubGroupId = ddlMenuSubGroupList.getSelectedValue();
            $scope.menuMaster.ModuleId = ddlModuleList.getSelectedValue();



            if (angular.isUndefinedOrNull($scope.menuMaster.Id) == true)
                $scope.menuMaster.MenuItemGroup = $scope.SelectedItems.MenuGroup;

            if (angular.isUndefinedOrNull($scope.menuMaster.PanelName) == true)
                throw "Please provide panel name";

            if (angular.isUndefinedOrNull($scope.menuMaster.ModuleId) == true)
                throw "Please provide module name";

            if (angular.isUndefinedOrNull($scope.menuMaster.MenuFrameId) == true)
                throw "Please provide menu frame name";


            if (angular.isUndefinedOrNull($scope.menuMaster.MenuSubGroupId) == false
                && angular.isUndefinedOrNull($scope.menuMaster.MenuGroupId) == true)
                throw "Please provide menu group because you have selected menu sub group";

            $http({
                method: 'POST',
                data: { 'menuMaster': $scope.menuMaster, 'companygroup': $scope.CompanyGroup, 'MenuAction': $scope.actionList },
                url: 'Menus/MenuSync/SaveMenu'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    var eDialog = $("#dialogEntry").data("ejDialog");
                    eDialog.close();
                    ShowResult(response.data.Message, 'success');
                    $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);
                    $scope.hierarchy();
                    $scope.MenuDetailList();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            })


        } catch (e) {
            ShowResult(e, 'failure');
        }
    };
    //$scope.GetMenuSequence = function () {
    //    cboService.getSequence($scope.getMenuSeqUrl, function (data) {
    //        $scope.menuMaster.Sequence = data;
    //    });
    //};
    //$scope.GetMenuSequence();

    //angular.isUndefinedOrNull = function (val) {
    //    return angular.isUndefined(val) || val === null || val === ""
    //};
    //$scope.DeleteMenu = function (obj) {

    //    var singleRowData = obj;
    //    $http({
    //        method: 'POST',
    //        data: { 'MenuId': singleRowData.Id, 'MenuMasterId': singleRowData.MenuMasterId },
    //        url: 'Menus/MenuSync/DeleteMenu'
    //    }).then(function successCallback(response) {
    //        if (response.data.Error == false) {
    //            var eDialog = $("#dialogEntry").data("ejDialog");
    //            eDialog.close();
    //            ShowResult(response.data.Message, 'success');
    //            $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);

    //            $scope.MenuDetailList();
    //        }
    //        else {
    //            ShowResult(response.data.Message, 'failure');
    //        }

    //    });
    //};
    //$scope.AddNewEntry = function () {
    //    try {

    //        //$scope.menuMaster.PanelName = $scope.SelectedItems.PanelName;

    //        if (angular.isUndefinedOrNull($scope.menuMaster.Id) == true)
    //            $scope.menuMaster.MenuItemGroup = $scope.SelectedItems.MenuGroup;


    //        if (angular.isUndefinedOrNull($scope.menuMaster.Id) == true)
    //            $scope.menuMaster.MenuItemGroup = $scope.SelectedItems.MenuGroup;


    //        if (angular.isUndefinedOrNull($scope.menuMaster.ModuleId) == true)
    //            throw "Please provide module name";

    //        if (angular.isUndefinedOrNull($scope.menuMaster.MenuFrameId) == true)
    //            throw "Please provide menu frame name";


    //        if (angular.isUndefinedOrNull($scope.menuMaster.MenuSubGroupId) == false
    //            && angular.isUndefinedOrNull($scope.menuMaster.MenuGroupId) == true)
    //            throw "Please provide menu group because you have selected menu sub group";

    //        $scope.GetCompanyGroup();
    //        $scope.uncheckAll();
    //        //$scope.clear();
    //        $scope.menuMasterModel.Id = null;
    //        var eDialog = $("#dialogEntry").data("ejDialog");
    //        eDialog.open();



    //    } catch (e) {
    //        ShowResult(e, 'failure');
    //    }


    //};
    //$scope.removePopup = function (data, index) {
    //    $scope.id = data.Id;
    //    $scope.cindex = index;
    //    $scope.message = 'Are you sure want to permanent delete this?';
    //    angular.element(document.querySelector('#removerPopUp')).modal('show');
    //};

    //$scope.removeRow = function () {
    //    if (!baseService.isUndefinedOrNull($scope.id)) {
    //        $http({
    //            method: 'POST'
    //            , url: $scope.actionDeleteUrl + $scope.id
    //            , dataType: 'JSON'
    //        }).then(function successCallback(response) {
    //            if (response.data.Error === true) {
    //                ShowResult(response.data.Message, 'failure');
    //            }
    //            else {
    //                ShowResult(response.data.Message, 'success');
    //                $scope.actionList.splice($scope.cindex, 1);
    //            }
    //        }, function errorCallback(response) {
    //            ShowResult(status.Message, 'failure');
    //        });
    //    }
    //    else
    //        $scope.actionList.splice($scope.cindex, 1);
    //    $scope.cindex = -1;
    //};

    //function getMenuActionList(menuId) {
    //    $http.get($scope.getActionListUrl + menuId)
    //        .then(function (response) {
    //            $scope.actionList = response.data;
    //        });
    //}

    $scope.add = function () {
        $scope.actionList.push({
            Id: null
            , MenuId: null
            , Action: null
            , UserName: null
            , Description: null
            , Active: true
        });
    };

    $scope.GetMenuInfoDoc = function (args) {
        $scope.menuMaster = Object.assign({}, args.data);
        location.href = 'Menus/MenuSync/GetMenuInfoDoc?menuId=' + $scope.menuMaster.Id;

    };
    //-------------------------------------//

}