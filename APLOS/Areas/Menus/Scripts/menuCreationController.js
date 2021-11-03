'use strict';
MenuCreationController.$inject = ['commonMessage', '$scope', '$rootScope', 'baseService', '$http', '$filter', 'cboService'];
function MenuCreationController(commonMessage, $scope, $rootScope, baseService, $http, $filter, cboService) {
    $rootScope.title = "Menu";
    $scope.Action = 'Save';
    $scope.menuList = [];
    $scope.actionList = [];
    $scope.disable = false;
    $scope.path = 'Menus/menucreation/';
    $scope.getListUrl = $scope.path + 'getallmenulist';
    $scope.getActionListUrl = $scope.path + 'GeActionListByMenu?menuId=';
    $scope.getMenuSeqUrl = $scope.path + 'GetAutoMenuSequence';

    $scope.menuHierarchy = [];
    $scope.moduleList = []; $scope.moduleId = '';
    $scope.menuFrameList = []; $scope.menuFrameId = '';
    $scope.menuGroupList = []; $scope.menuGroupId = '';
    $scope.menuSubGroupList = []; $scope.menuSubGroupId = '';
    $scope.EditButttonShow = true;
    $scope.saveUrl = $scope.path + 'Create';
    $scope.updateUrl = $scope.path + 'Edit';
    $scope.deleteUrl = $scope.path + 'Delete/';
    $scope.actionDeleteUrl = $scope.path + 'DeleteMenuAction/';
    $scope.SelectedItemsModel = {
        Module: null,
        MenuFrame: null,
        MenuGroup: null,
        MenuSubGroup: null,
        PanelName: null
    };
    $scope.SelectedItems = Object.assign({}, $scope.SelectedItemsModel);

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
        UserDefineCode: null,
        Description: null,
        IsExternalMenu: false,
        Remarks: null,
        Active: true,
        Code: null,
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
    $scope.hierarchy();
    $scope.CompanyGroup = [];

    $scope.MenuList = [];
    $scope.MenuDetailList = function () {
        $http({
            method: 'GET',
            url: 'Menus/MenuCreation/GetMenuDetailList'
        }).then(function successCallback(response) {
            $scope.MenuList = response.data;
        });
    };
    $scope.MenuDetailList();


    $scope.GetMenuData = function (args) {
        $scope.menuMaster = Object.assign({}, args.data);
        getMenuActionList();
        $scope.Action = 'Update';
        $scope.GetCompanyGroup();
        var eDialog = $("#dialogEntry").data("ejDialog");
        eDialog.open();
    };


    $http({
        method: 'GET',
        url: 'Menus/MenuCreation/GeModuleList'
    }).then(function successCallback(response) {
        $scope.moduleList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuFrame/GetMenuFrameCbo'
    }).then(function successCallback(response) {
        $scope.menuFrameList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuGroup/GetMenuGroupCbo'
    }).then(function successCallback(response) {
        $scope.menuGroupList = response.data;
    });

    $http({
        method: 'GET',
        url: 'Menus/MenuSubGroup/GetMenuSubGroupCbo'
    }).then(function successCallback(response) {
        $scope.menuSubGroupList = response.data;
    });


    //////////////////ENTRY///////////////////////
    $scope.selModule = function (args) {
        $scope.SelectedItems.Module = args.data.StandardName;
        $scope.menuMaster.ModuleId = args.data.Id;
    }
    $scope.selFrame = function (args) {
        $scope.SelectedItems.MenuFrame = args.data.Text;
        $scope.menuMaster.MenuFrameId = args.data.Value;
    }
    $scope.selGroup = function (args) {
        $scope.SelectedItems.MenuGroup = args.data.Text;
        $scope.menuMaster.MenuGroupId = args.data.Value;

    }
    $scope.selSubGroup = function (args) {
        $scope.SelectedItems.MenuSubGroup = args.data.Text;
        $scope.menuMaster.MenuSubGroupId = args.data.Value;
    }
    $scope.onGroupLoad = function (args) {
        try {
            if ($scope.CompanyGroup.length > 0) {
                var ind = [];

                for (var i = 0; i < $scope.CompanyGroup.length; i++) {
                    if ($scope.CompanyGroup[i].IsSaved == true)
                        ind.push(i);
                }
                $('#lstCompanyGroup').ejListBox("checkItemsByIndices", ind);

            }
        } catch (e) {

        }

    }
    $scope.uncheckAll = function () {
        try {
            var ind = [];

            for (var i = 0; i < $scope.CompanyGroup.length; i++) {
                ind.push(i);
                $('#lstCompanyGroup').ejListBox("uncheckItemByIndex", i);
            }
            //$('#lstCompanyGroup').ejListBox("checkItemsByIndices", ind);
        } catch (e) {

        }

    }

    $scope.GetCompanyGroup = function () {
        $scope.uncheckAll();

        $http({
            method: 'POST',
            data: { 'MenuMasterId': $scope.menuMaster.MenuMasterId },
            url: 'Menus/MenuCreation/GetCompanyGroup'
        }).then(function successCallback(response) {
            $scope.uncheckAll();
            $scope.CompanyGroup = response.data;
        });
    }
    $scope.GetCompanyGroup();
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
                getMenuActionList();
                var eDialog = $("#dialogEntry").data("ejDialog");
                eDialog.open();
            });

        } catch (e) {

        }

    }

    angular.isUndefinedOrNull = function (val) {
        return angular.isUndefined(val) || val === null || val === ""
    }
    $scope.AddNewEntry = function () {
        try {

            //$scope.menuMaster.PanelName = $scope.SelectedItems.PanelName;

            if (angular.isUndefinedOrNull($scope.menuMaster.Id) == true)
                $scope.menuMaster.MenuItemGroup = $scope.SelectedItems.MenuGroup;


            if (angular.isUndefinedOrNull($scope.menuMaster.Id) == true)
                $scope.menuMaster.MenuItemGroup = $scope.SelectedItems.MenuGroup;


            if (angular.isUndefinedOrNull($scope.menuMaster.ModuleId) == true)
                throw "Please provide module name";

            if (angular.isUndefinedOrNull($scope.menuMaster.MenuFrameId) == true)
                throw "Please provide menu frame name";


            if (angular.isUndefinedOrNull($scope.menuMaster.MenuSubGroupId) == false
                && angular.isUndefinedOrNull($scope.menuMaster.MenuGroupId) == true)
                throw "Please provide menu group because you have selected menu sub group";

            $scope.GetCompanyGroup();
            $scope.uncheckAll();
            //$scope.clear();
            $scope.menuMasterModel.Id = null;
            var eDialog = $("#dialogEntry").data("ejDialog");
            eDialog.open();



        } catch (e) {
            ShowResult(e, 'failure');
        }


    };
    $scope.removePopup = function (data, index) {
        $scope.id = data.Id;
        $scope.cindex = index;
        $scope.message = 'Are you sure want to permanent delete this?';
        angular.element(document.querySelector('#removerPopUp')).modal('show');
    };

    $scope.removeRow = function () {
        if (!baseService.isUndefinedOrNull($scope.id)) {
            $http({
                method: 'POST'
                , url: $scope.actionDeleteUrl + $scope.id
                , dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    $scope.actionList.splice($scope.cindex, 1);
                }
            }, function errorCallback(response) {
                ShowResult(status.Message, 'failure');
            });
        }
        else
            $scope.actionList.splice($scope.cindex, 1);
        $scope.cindex = -1;
    };

    function getMenuActionList() {
        var Actions = null;
        try {
            Actions = $scope.menuMaster.SourceCodeMenu.Actions;
        } catch (e) {

        }
        $http({
            method: 'POST'
            , url: $scope.getActionListUrl
            , dataType: 'JSON'
            , data: { menuId: $scope.menuMaster.Id, SourceCodeMenuActions: Actions }
        }).then(function successCallback(response) {
            $scope.actionList = response.data;

        }, function errorCallback(response) {
            ShowResult(status.Message, 'failure');
        });

        //$http.get($scope.getActionListUrl + menuId)
        //    .then(function (response) {
        //        $scope.actionList = response.data;
        //    });
    }

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

    $scope.tabChange = function () {
        $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);

        $scope.SelectedItemsModel.PanelName = $scope.SelectedItems.PanelName;
        $scope.SelectedItems = Object.assign({}, $scope.SelectedItemsModel);
        $scope.clear();
    };

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

    $scope.checkChange = function (args) {

        if (args.isInteraction == true) {
            args.data.IsSaved = args.isChecked;
            for (var i = 0; i < $scope.CompanyGroup.length; i++) {
                if (args.data.Id == $scope.CompanyGroup[i].Id) {
                    $scope.CompanyGroup[i].IsSaved = args.isChecked;
                    break;
                }
            }
        }

    };
    $scope.GetMenuSequence = function () {
        cboService.getSequence($scope.getMenuSeqUrl, function (data) {
            $scope.menuMaster.Sequence = data;
        });
    };
    $scope.GetMenuSequence();

    $scope.saveMenu = function () {
        try {
            //$scope.menuMaster.PanelName = $scope.SelectedItems.PanelName;
            $scope.GetMenuSequence();
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
                url: 'Menus/MenuCreation/SaveMenu'
            }).then(function successCallback(response) {
                if (response.data.Error == false) {
                    var eDialog = $("#dialogEntry").data("ejDialog");
                    eDialog.close();
                    ShowResult(response.data.Message, 'success');
                    $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);
                    //$scope.hierarchy();
                    $scope.MenuDetailList();
                }
                else {
                    ShowResult(response.data.Message, 'failure');
                }

            });

        } catch (e) {
            ShowResult(e, 'failure');
        }



    };

    $scope.DeleteMenu = function (obj) {

        var singleRowData = obj.data;
        $http({
            method: 'POST',
            data: { 'MenuId': singleRowData.Id, 'MenuMasterId': singleRowData.MenuMasterId },
            url: 'Menus/MenuCreation/DeleteMenu'
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

        });
    };
    $scope.clear = function () {
        //$scope.uncheckAll();
        $('#lstCompanyGroup').ejListBox("unselectAll");
        $('#lstmoduleList').ejListBox("unselectAll");
        $('#lstmenuFrameList').ejListBox("unselectAll");
        $('#lstmenuGroupList').ejListBox("unselectAll");
        $('#lstmenuSubGroupList').ejListBox("unselectAll");
        $scope.Action = 'Save';

        var panel = $scope.SelectedItems.PanelName;
        $scope.actionList = [];
        $scope.menuMaster = Object.assign({}, $scope.menuMasterModel);
        $scope.SelectedItems = Object.assign({}, $scope.SelectedItemsModel);
        $scope.SelectedItems.PanelName = panel;
    };


    ///////////////END ENTRY/////////////////////
    $scope.changeBckCol = function (args) {
        try {
            if (args.data.MarkForDeletion == true)
                args.row.css("background-color", "#ff0000");

            if (angular.isUndefinedOrNull(args.data.Id) == true)
                args.row.css("background-color", "#FFF97F");
        } catch (e) {

        }
    }
    $scope.changeBckColAction = function (args) {
        try {
            if (angular.isUndefinedOrNull(args.data.Id) == true)
                args.row.css("background-color", "#00ff00");
        } catch (e) {

        }
    }
}