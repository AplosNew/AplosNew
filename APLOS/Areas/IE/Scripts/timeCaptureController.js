'use strict';
TimeCaptureController.$inject = ["cboService", "commonMessage", "$scope", "$rootScope", "baseService", "$routeParams", "$location", "$http", "$filter", "$controller", "$sce"];
function TimeCaptureController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller, $sce) {
    $rootScope.title = "Time Capture";
    $scope.Action = 'Save';
    $scope.message_confirmation = '';
    $scope.path = 'IE/timeCapture/';
    $scope._starttime = "";
    $scope._endtime = "";
    $scope._settime = "";
    $scope._filename = "";
    $scope._plabackrate = 1;
    $scope.tab = 1;
    $scope.cycle = 1;
    $scope.cycleName = "CT1";
    $scope.speedName = "x1";
    $scope.AvgMaxMin = 1;
    $scope.AvgMM = "AVG";
    $scope.GSDButtonID = "";
    $scope.TotalElement = 100;
    $scope.TotalTabs = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
    $scope.TotalElements = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    $scope.TotalCT = [1, 2, 3, 4, 5];

    $scope.IsAvgCT1 = false;
    $scope.IsAvgCT2 = false;
    $scope.IsAvgCT3 = false;
    $scope.IsAvgCT4 = false;
    $scope.IsAvgCT5 = false;
    $scope.IsCreateNewVersion = false;
    $scope.IsCalculation = true;
    $scope.CopyVersion = "";
    $scope.IsNewVideo = false;

    $("#filec").prop("disabled", true);


    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });
    $scope.getArticle = function () {

        if (baseService.isUndefinedOrNull($scope.operationdata.MaterialMasterId))
            return ShowResult('This material has no attribute', 'failure');
        $scope.getArticleSearchList($scope.operationdata.MaterialMasterId);
    };
    $scope.selectarticle = function (ob) {
        try {

            $scope.VAS.ArticleId = ob.Id;
            $scope.VAS.MachineName = ob.StandardName;
            angular.element(document.querySelector('#articleSearchPop')).modal('hide');
            $scope.itemIndex = -1;
            $scope.mmChangeFlag = true;
        } catch (e) {
            ShowResult(e, '', 'articleSearchPop');
        }
    };

    $scope.VideoHeight = 250;
    $scope.ResizeVideoWithAspectRatio = function () {
        setInterval(function () {
            var divVideoArea = document.getElementById("divVideoArea");
            var video = document.getElementById("vdid");

            var AreaHeight = divVideoArea.clientHeight;
            if ($scope.VideoHeight != AreaHeight && AreaHeight > $scope.VideoHeight) {
                video.style.height = AreaHeight - 150 + "px";
            }

            $scope.VideoHeight = AreaHeight;

        }, 100);
    };
    $scope.ResizeVideoWithAspectRatio();
    var vid = document.getElementById("vdid");
    $scope.VideoPercentage = 0;
    $scope.SeekbarDuration = '';
    vid.ontimeupdate = function () {
        try {
            if (baseService.isUndefinedOrNull(vid.currentTime) || baseService.isUndefinedOrNull(vid.duration)
                || isNaN(vid.currentTime) || isNaN(vid.duration)) {
                $scope.SeekbarDuration = '';
                $("#custom-seekbar span").css("width", "0%");
                $scope.SetVideoStartPoint(null);
                VideoDurationUsed();
                return;
            }


            $scope.VideoPercentage = (vid.currentTime / vid.duration) * 100;
            $("#custom-seekbar span").css("width", $scope.VideoPercentage + "%");
            $scope.SeekbarDuration = toHHMMSS(vid.currentTime) + '/' + toHHMMSS(vid.duration);

            $("#timepicker").ejTimePicker({ maxTime: toHHMMSS(vid.duration) });
            $scope.$apply();
        } catch (e) {

        }

    };

    $scope.videoStartTime = 0;

    $scope.SetVideoStartPoint = function (args) {

        try {
            var times = args;
            var totalSeconds = 0;
            if (baseService.isUndefinedOrNull(args) == false) {
                times = args.value.split(":");
            }
            else {
                if (baseService.isUndefinedOrNull($scope.VAS.videoStartTime))
                    $scope.VAS.videoStartTime = '00:00:00';

                times = $scope.VAS.videoStartTime.split(":");
            }
            totalSeconds = (dbl(times[0]) * 3600) + (dbl(times[1]) * 60) + (dbl(times[2]));
            $scope.videoStartTime = totalSeconds;
            $scope.VAS.videoStartTime = toHHMMSS(totalSeconds);
            if (totalSeconds > vid.duration) {
                $scope.videoStartTime = vid.duration;
                $scope.VAS.videoStartTime = toHHMMSS(vid.duration);
            }


            vid.currentTime = $scope.videoStartTime;

        } catch (e) {

        }
    }

    var toHHMMSS = (secs) => {

        //var duration = secs * 1000;
        //var milliseconds = parseInt(((secs * 1000) % 1000) / 100),
        var sec_num = parseInt(secs, 10)
        var hours = Math.floor(sec_num / 3600)
        var minutes = Math.floor(sec_num / 60) % 60
        var seconds = sec_num % 60

        //var milliseconds = parseInt((duration % 1000)),
        //    seconds = Math.floor((duration / 1000) % 60),
        //    minutes = Math.floor((duration / (1000 * 60)) % 60),
        //    hours = Math.floor((duration / (1000 * 60 * 60)) % 24);
        //return [hours, minutes, seconds]
        //    .map(v => v < 10 ? "0" + v : v)
        //    .filter((v, i) => v !== "00" || i > 0)
        //    .join(":")

        //return lpad("0", 2, hours) + ":" + lpad("0", 2, minutes) + ":" + lpad("0", 2, seconds) + ":" + lpad("0", 3, milliseconds);
        return lpad("0", 2, hours) + ":" + lpad("0", 2, minutes) + ":" + lpad("0", 2, seconds);
    }
    function lpad(padString, length, number) {
        var str = number + "";
        while (str.length < length)
            str = padString + str;
        return str;
    }
    $scope.VAS = {
        Id: '',
        OperationVariationSystemId: '',
        OperationCode: '',
        OperationSAM: null,
        VasDescription: null,
        VASSAM: null,
        StandardSAM: null,
        ProductionSystemId: null,
        BHTValue: 0,
        AvgMaxMin: 1,
        Frequency: null,
        SPI: null,
        RPM: null,
        MachineAllowances: null,
        PersonalAllowances: null,
        AdditionalAllowances: null,
        IsAvgCT1: false,
        IsAvgCT2: false,
        IsAvgCT3: false,
        IsAvgCT4: false,
        IsAvgCT5: false,
        Version: "",
        OperationVersion: "0",
        videoStartTime: "00:00:00",
        ArticleId: null,
        MachineName: null,
        OperatorId: '',
        Remarks: '',
        FileName: ''
    };

    $scope.bHTList = [];
    $scope.operationVersionList = [];
    $scope.gSDCodeList = [];
    $scope.operationList = [];
    $scope.elementTypesList = [];
    $scope.videoSources = [];

    $("#filec").change(function () {
        if (angular.isUndefined($scope.VAS.OperationVariationSystemId) || $scope.VAS.OperationVariationSystemId === "" || $scope.VAS.OperationVariationSystemId === null) {
            ShowResult('Please Select Operation First..!', 'failure');
            return false;
        }
        else {
            $scope.IsNewVideo = true;
            renderVideo(this.files[0]);
        }
    });

    $scope.the_url = "";
    $scope.filename = "";
    $scope.filedata = "";
    function renderVideo(file) {
        $scope.filedata = file;
        var reader = new FileReader();
        reader.onload = function (event) {
            $scope.the_url = event.target.result;
            if (angular.isUndefined($scope.VAS.Version) || $scope.VAS.Version === "" || $scope.VAS.Version === null) {
                ShowResult('Please Select Operation Version..!', 'failure');
            }
            else {
                $scope.VAS.videoStartTime = "00:00:00";
                $scope.SetVideoStartPoint(null);
                VideoDurationUsed();
                $scope.$apply();
                $('#vdid').html("<source id='vdids' src='" + $scope.the_url + "' type='video/mp4'>");
                $scope.filename = file.name;
                $("#custom-seekbar span").css("width", $scope.VideoPercentage + "%");

            }

        };

        reader.readAsDataURL(file);
    }

    $scope.getOperationList = function () {
        //$scope.Clear();
        $('#data-vid').empty();
        $scope.Action = 'Save';
        $http({
            method: 'GET',
            url: $scope.path + 'GetOperationList'
        }).then(function successCallback(response) {
            $scope.operationList = response.data;
            angular.element(document.querySelector("#modalOperationList")).modal("toggle");
        });
    };

    $scope.SaveDataAndUploadFile = function () {

        if ($scope.isVersionApproved === true) {
            ShowResult("Cannot edit approved version", "failure");
            return false;
        }

        $scope.getCalculateSAM();
        $scope.$broadcast('show-errors-check-validity');
        var arrTimeDetails = [];

        for (var i = 1; i <= $scope.TotalElement; i++) {
            if ($("#GSD_" + i).val() !== "" && $("#ddlET_" + i).val()) {
                if (i == 1) {
                    arrTimeDetails.push(i);
                }
                arrTimeDetails.push($("#GSD_" + i).val());
                arrTimeDetails.push($("#ddlET_" + i).val());
                for (var j = 1; j <= 5; j++) {
                    arrTimeDetails.push($("#CT_" + j + "_" + i).val());
                }

                arrTimeDetails.push($("#CT_Avg_" + i).val());
                arrTimeDetails.push($("#CT_Rat_" + i).val());
                arrTimeDetails.push($("#CT_BT_SEC_" + i).val());
                if (i >= 1 && i <= parseInt($scope.TotalElement) - 1) {
                    var nextRow = parseInt(i) + 1;
                    arrTimeDetails.push($("#hdGSD_" + i).val() + "/" + nextRow);
                }
                else if (i == $scope.TotalElement) {
                    arrTimeDetails.push($("#hdGSD_" + i).val() + "/");
                }
            }
            else {
                if (i === i) {
                    if ($("#GSD_" + 1).val() === "") {
                        ShowResult('Please Set The Element Code Value..!', 'failure');
                        return false;
                    }

                    if ($("#ddlET_" + 1).val() === "") {
                        ShowResult('Please Set The Element Type Value..!', 'failure');
                        return false;
                    }
                }
            }
        }

        var data = new FormData();
        data.append("file", $scope.filedata);
        data.append("operationData", JSON.stringify($scope.VAS));
        data.append("operationChild", arrTimeDetails.toString());
        data.append("CopyVersion", $scope.CopyVersion.toString());
        data.append("IsNewVideo", $scope.IsNewVideo.toString());

        try {
            if ($scope.timeCaptureForm.$valid && $scope.IsCalculation === true) {
                $http({
                    method: "POST",
                    url: $scope.path + "UploadVideoData",
                    withCredentials: true,
                    processData: false,
                    headers: { 'Content-Type': undefined },
                    contentType: undefined,
                    dataType: JSON,
                    data: data,
                    transformRequest: angular.identity
                }).then(function successCallback(response) {
                    if (response.data.Error == true) {
                        ShowResult(response.data.Message, "failure");
                        $scope.Action = 'Save';
                    }
                    else {
                        $("#filec").prop("disabled", true);
                        $scope.IsCreateNewVersion = true;
                        $scope.VAS.Id = response.data.Id;
                        $scope.VAS.Version = response.data.Version;
                        $scope.VAS.OperationVersion = response.data.Version;
                        $scope.LoadSelectedOperationVersionData($scope.VAS.OperationVariationSystemId);
                        ShowResult(response.data.Message, "success");
                        $scope.Action = 'Update';
                    }
                }, function errorCallback(response) {
                    ShowResult(response.status.Message, "failure");
                });
            }
        } catch (e) {
            ShowResult(e, "failure");
        }
    };
    $scope.ArchiveData = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'ArchiveData',
            data: {
                'Id': $scope.VAS.Id
            }
        }).then(function successCallback(response) {
            if (response.data.Error == false) {
                ShowResult(response.data.Message, "success");
                $scope.Clear();
            }
            else {
                ShowResult(response.data.Message, "failure");

            }
        }, function errorCallback(response) {
            ShowResult(response.status.Message, "failure");
        });

    }
    $scope.getBHT = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetBHTList'

        }).then(function successCallback(response) {
            $scope.bHTList = response.data;
        });
    };

    $scope.getElementType = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetElementType'

        }).then(function successCallback(response) {
            $scope.elementTypesList = response.data;
            $(".ddlET").val("");
        });
    };

    $scope.GetPSAValue = function () {
        if ($scope.VAS.ProductionSystemId == undefined || $scope.VAS.ProductionSystemId === "") {
            $scope.VAS.BHTValue = "0";
            return false;
        }

        $http({
            method: 'POST',
            url: $scope.path + 'GetPSAValue',
            data: {
                'ProductionSystemId': $scope.VAS.ProductionSystemId
            }
        }).then(function successCallback(response) {
            $scope.VAS.BHTValue = response.data[0].FactorValue;
        });
    };
    $scope.getGSDCode = function () {
        $http({
            method: 'GET',
            url: $scope.path + 'GetGSDCodeList'

        }).then(function successCallback(response) {
            $scope.gSDCodeList = response.data;
        });
    };
    $scope.getDoubleClickElementCode = function ($event) {
        var x = $event;
        $scope.bulletindata = x.data;
        $("#GSD_" + $scope.GSDButtonID).val($scope.bulletindata.Code);
        $("#hdGSD_" + $scope.GSDButtonID).val($scope.bulletindata.TMU);
        $("#GSDTMU_" + $scope.GSDButtonID).val($scope.bulletindata.TMU);
        angular.element(document.querySelector('#modalGSD')).modal('toggle');
    }
    $scope.onClickBulletinApprove = function (z) {
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.bulletindata = gridObj.getSelectedRecords()[0];
        $("#GSD_" + $scope.GSDButtonID).val($scope.bulletindata.Code);
        $("#hdGSD_" + $scope.GSDButtonID).val($scope.bulletindata.TMU);
        $("#GSDTMU_" + $scope.GSDButtonID).val($scope.bulletindata.TMU);
        angular.element(document.querySelector('#modalGSD')).modal('toggle');
    };
    $scope.recordoperationdoubleclick = function ($event) {
        $scope.Clear();
        $("#spOVName").empty();
        $("#spOPName").empty();
        $("#spMCName").empty();
        $("#spFileName").empty();

        var x = $event;
        $scope.VAS.OperationVariationSystemId = x.data.Id;
        $scope.VAS.OperationCode = x.data.Code;
        $scope.operationdata = x.data;
        $("#spOVName").html(x.data.OperationVariationName);
        $("#spOPName").html(x.data.OperationName);
        $("#spMCName").html(x.data.MachineName);
        $scope.LoadOperationData($scope.VAS.OperationVariationSystemId);
        $scope.LoadOperationVersionData($scope.VAS.OperationVariationSystemId);
        $scope.LoadSelectedProductionSystem($scope.VAS.OperationVariationSystemId);
        angular.element(document.querySelector('#modalOperationList')).modal('toggle');

        VideoDurationUsed();
    };
    $scope.onClickOperationDetails = function (z) {
        $scope.Clear();
        $("#spOVName").empty();
        $("#spOPName").empty();
        $("#spMCName").empty();
        var x = "#" + z;
        var gridObj = $(x).data("ejGrid");
        $scope.operationdata = gridObj.getSelectedRecords()[0];
        $scope.VAS.OperationVariationSystemId = $scope.operationdata.Id;
        $scope.VAS.OperationCode = $scope.operationdata.Code;

        $("#spOVName").html($scope.operationdata.OperationVariationName);
        $("#spOPName").html($scope.operationdata.OperationName);
        $("#spMCName").html($scope.operationdata.MachineName);

        $scope.LoadOperationData($scope.VAS.OperationVariationSystemId);
        $scope.LoadOperationVersionData($scope.VAS.OperationVariationSystemId);
        angular.element(document.querySelector('#modalOperationList')).modal('toggle');
    };

    $scope.LoadOperationData = function (OperationVariationSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedOperation',
            data: {
                'OperationVariationSystemId': OperationVariationSystemId, 'Version': $scope.VAS.Version
            }
        }).then(function successCallback(response) {
            if (response.data.length === 0)
                $scope.setOperationData(response);

            $("#filec").removeAttr("disabled");

        });
    };
    $scope.LoadOperationVersionData = function (OperationVariationSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedOperationVersions',
            data: {
                'OperationVariationSystemId': OperationVariationSystemId
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.operationVersionList = response.data;
                $scope.VAS.Version = response.data[0]["MaxVersion"];
            }
            else {
                $scope.operationVersionList = [];
                $scope.VAS.Version = 1;
            }
        });
    };
    $scope.LoadSelectedProductionSystem = function (OperationVariationSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + 'GetSelectedProductionSystem',
            data: {
                'OperationVariationSystemId': OperationVariationSystemId
            }
        }).then(function successCallback(response) {
            if (response.data.length > 0) {
                $scope.VAS.ProductionSystemId = response.data[0]["Id"]
                $scope.VAS.BHTValue = response.data[0]["FactorValue"];
            }
            else {
                $scope.VAS.ProductionSystemId = "";
                $scope.VAS.BHTValue = "";
            }
        });
    };
    $scope.LoadSelectedOperationVersionData = function (OperationVariationSystemId) {
        $http({
            method: 'POST',
            url: $scope.path + 'LoadSelectedOperationVersionData?OperationVariationSystemId=' + OperationVariationSystemId

        }).then(function successCallback(response) {
            $scope.operationVersionList = response.data;
        });
    };

    $scope.GetOperationVersionData = function () {
        $('#vdid').html("");
        $("#custom-seekbar span").css("width", $scope.VideoPercentage + "%");
        $('#spFileName').text("");

        if ($scope.VAS.OperationVersion === "")
            return false;

        $http({
            method: 'POST',
            url: $scope.path + 'GetOperationVersionData',
            data: {
                'OperationVariationSystemId': $scope.VAS.OperationVariationSystemId, 'Version': $scope.VAS.OperationVersion
            }
        }).then(function successCallback(response) {
            $scope.setOperationData(response);
            if ($scope.VAS.OperationVersion === "0") {
                $scope.LoadOperationVersionData($scope.VAS.OperationVariationSystemId);
                $scope.LoadSelectedProductionSystem($scope.VAS.OperationVariationSystemId);
                $scope.IsCreateNewVersion = false;
            }
            else {
                $scope.IsCreateNewVersion = true;
                $("#filec").attr("disabled", true);
            }
        });
        $scope.reloadVideo();

        var controls1 = moviecontainer.querySelector("figcaption");
        //var playpause1 = controls1.querySelector("a");
        var playpause1 = document.getElementById("btnPlayPause");
        playpause1.innerHTML = "<i class='fa fa-play-circle-o fa-fw'></i>";

        if ($scope.VAS.OperationVersion === "0")
            $scope.Action = "Save";
    };
    $scope.isVersionApproved = null;
    $scope.setOperationData = function (response) {
        $("#vdid").removeAttr("src");
        $("#filec").val([]);
        $('#vdid').html("");
        $("#custom-seekbar span").css("width", $scope.VideoPercentage + "%");

        $(".GSD_TMU").val("");
        $('#divResultArea input').val('');
        $(".ddlET").val("");

        $scope.tab = 1;
        $scope.cycle = 1;
        $scope.cycleName = "CT1";
        $scope.speedName = "x1";
        $scope.AvgMaxMin = 1;
        $scope.AvgMM = "AVG";
        $scope.isVersionApproved = null;
        if (response.data.length > 0) {
            $scope.isVersionApproved = response.data[0].IsApproved;
            $scope.VAS.Id = response.data[0].Id;
            $scope.VAS.Frequency = response.data[0].Frequency;
            $scope.VAS.SPI = response.data[0].SPI;
            $scope.VAS.RPM = response.data[0].RPM;
            $scope.VAS.MachineAllowances = response.data[0].MachineAllowances;
            $scope.VAS.PersonalAllowances = response.data[0].PersonalAllowances;
            $scope.VAS.AdditionalAllowances = response.data[0].AdditionalAllowances;
            $scope.VAS.OperationSAM = response.data[0].OperationSAM;
            $scope.VAS.VASSAM = response.data[0].VASSAM;
            $scope.VAS.StandardSAM = response.data[0].StandardSAM;
            $scope.VAS.OperatorId = response.data[0].OperatorId;
            $scope.VAS.Version = response.data[0].Version.toString();
            $scope.VAS.ProductionSystemId = response.data[0].ProductionSystemId.toString();
            $scope.VAS.BHTValue = response.data[0].BHTValue;
            $scope.VAS.IsAvgCT1 = response.data[0].IsAvgCT1;
            $scope.VAS.IsAvgCT2 = response.data[0].IsAvgCT2;
            $scope.VAS.IsAvgCT3 = response.data[0].IsAvgCT3;
            $scope.VAS.IsAvgCT4 = response.data[0].IsAvgCT4;
            $scope.VAS.IsAvgCT5 = response.data[0].IsAvgCT5;
            $scope.VAS.AvgMaxMin = response.data[0].AvgMaxMin;
            $scope.VAS.Remarks = response.data[0].Remarks;
            $scope.VAS.VasDescription = response.data[0].VasDescription;
            $scope.VAS.VASQuantity = response.data[0].VASQuantity;
            $scope.VAS.videoStartTime = response.data[0].videoStartTime;
            $scope.VAS["ArticleId"] = response.data[0].ArticleId;
            $scope.VAS["MachineName"] = response.data[0].MachineName;

            $scope.VAS.AddedBy = response.data[0].AddedBy;
            $scope.VAS.AddedDate = response.data[0].AddedDate;
            $scope.VAS.ApprovedBy = response.data[0].ApprovedBy;
            $scope.VAS.ApprovedDate = response.data[0].ApprovedDate;

            var _count = 1;
            var _totalTMU = 0;
            $.each(response.data, function (i, item) {
                var _currentRow = parseInt(i) + 1;

                $("#CT_" + _count + "_" + _currentRow).val(item.CT1 == "0" ? "" : item.CT1);
                _count++;

                $("#CT_" + _count + "_" + _currentRow).val(item.CT2 == "0" ? "" : item.CT2);
                _count++;

                $("#CT_" + _count + "_" + _currentRow).val(item.CT3 == "0" ? "" : item.CT3);
                _count++;

                $("#CT_" + _count + "_" + _currentRow).val(item.CT4 == "0" ? "" : item.CT4);
                _count++;

                $("#CT_" + _count + "_" + _currentRow).val(item.CT5 == "0" ? "" : item.CT5);

                $("#GSD_" + _currentRow).val(item.ElementCode);
                $("#ddlET_" + _currentRow).val(item.ElementTypeId);
                $("#CT_Avg_" + _currentRow).val(item.TimeAvg);
                $("#CT_Rat_" + _currentRow).val(item.Ratings);
                $("#CT_BT_SEC_" + _currentRow).val(item.BasicTime);
                $("#hdGSD_" + _currentRow).val(item.TMU);
                $("#GSDTMU_" + _currentRow).val(item.TMU);

                _totalTMU += parseInt(item.TMU);

                _count = 1;
            });

            $("#txtTotalTMU").val(_totalTMU);
            $("#filec").removeAttr("disabled");
            $("#spFileName").text(response.data[0].OriginalVideoName);
            $scope.VAS.FileName = response.data[0].OriginalVideoName;
            $scope.Action = 'Update';
            $('#vdid').html("<source id='vdids' src='POPResources/vas/" + response.data[0].VASVideoName + "' type='video/mp4'>");
            $scope.reloadVideo();
        }
        else {
            $scope.VAS.Id = '';
            $scope.VAS.OperationId = $scope.operationdata.Id;
            $scope.VAS.Frequency = $scope.operationdata.Frequency;
            $scope.VAS.SPI = $scope.operationdata.SPI;
            $scope.VAS.RPM = $scope.operationdata.RPM;
            $scope.VAS.MachineAllowances = $scope.operationdata.MachineAllowance;
            $scope.VAS.PersonalAllowances = $scope.operationdata.PersonalAllowance;
            $scope.VAS.AdditionalAllowances = $scope.operationdata.AdditionalAllowances;
            $scope.VAS.OperationSAM = $scope.operationdata.TotalSAM;
            $scope.VAS.VASSAM = $scope.operationdata.VASSAM;
            $scope.VAS.StandardSAM = $scope.operationdata.StandardSAM;
            $scope.VAS.OperatorId = "";
            $scope.VAS.AvgMaxMin = 1;
            $scope.VAS.videoStartTime = '00:00:00';

            //$scope.VAS.Version = 1;
            $scope.VAS.ProductionSystemId = null;
            $scope.VAS.Remarks = "";
            $scope.VAS.VasDescription = null;
            $scope.VAS.VASQuantity = 1;
            $scope.VAS.BHTValue = "";
            $scope.VAS.IsAvgCT1 = false;
            $scope.VAS.IsAvgCT2 = false;
            $scope.VAS.IsAvgCT3 = false;
            $scope.VAS.IsAvgCT4 = false;
            $scope.VAS.IsAvgCT5 = false;

            $scope.VAS["ArticleId"] = $scope.operationdata.ArticleId;
            $scope.VAS["MachineName"] = $scope.operationdata.MachineName;

            $scope.VAS.AddedBy = null;
            $scope.VAS.AddedDate = null;
            $scope.VAS.ApprovedBy = null;
            $scope.VAS.ApprovedDate = null;

            //$scope.operationVersionList = [];
            $scope.IsVisible = false;

            $(".ddlET").val("");
            $("#filec").removeAttr("disabled");
        }
        $scope.reloadVideo();
        $scope.CTAverage();

    };

    $scope.getGSDCode();
    $scope.getBHT();
    $scope.getElementType();
    var URL = window.URL || window.webkitURL;
    $("#filec").change(function () {
        $scope.VAS.FileName = baseService.isUndefinedOrNull(this.files[0].name) ? $scope.VAS.FileName : this.files[0].name;
        $("#spFileName").text($scope.VAS.FileName);
        if (angular.isUndefined($scope.VAS.Version) || $scope.VAS.Version === "" || $scope.VAS.Version === null) {
            ShowResult('Please Select Operation Version..!', 'failure');
            return false;
        }
        else {
            $scope.IsNewVideo = true;
            playSelectedFile(this.files[0]);
            $scope.filedata = this.files[0];
        }
    });

    var playSelectedFile = function (file) {
        var videoNode = document.querySelector('video');
        var fileURL = URL.createObjectURL(file);
        videoNode.src = fileURL;
        $scope.video_url = fileURL;
    };

    $scope.getStart = function () {
        var vEs = angular.element(document.querySelector('#vdid'));
        vEs.playbackRate = $scope._plabackrate;
        vEs[0].play();
        $scope._starttime = vEs[0].currentTime.toFixed(3);
    };
    $scope.getEnd = function () {
        var vEs = angular.element(document.querySelector('#vdid'));
        vEs[0].pause();
        $scope._endtime = vEs[0].currentTime.toFixed(3);
        var ft = $scope._starttime * 1;
        var tt = $scope._endtime * 1;
        var _duration = tt - ft;
        if (_duration <= 0) return ShowResult("'Start Time' should be greater than 'End Time'");
        else {
            SetTimeDuration($scope.cycle, _duration.toFixed(3));
        }
    };
    function SetTimeDuration(cycle, duration) {
        //alert(cycle + ' - ' + duration);
        var cycleType = cycle;
        for (var i = 1; i <= 100; i++) {
            var cycleTime = $("#CT_" + cycleType + "_" + i).val();
            if (cycleTime == '') {
                $("#CT_" + cycleType + "_" + i).val(duration);
                ShowAverage(i);
                break;
            }
        }
    }

    $scope.VideoStartEnabled = true;
    function VideoDurationUsed() {
        $scope.VideoStartEnabled = true;
        try {
            for (var index = 1; index <= 100; index++) {
                for (var i = 1; i <= 5; i++) {
                    var _CurrentValue = $("#CT_" + i + "_" + index).val();
                    if (_CurrentValue != "") {
                        var _total = dbl(_CurrentValue);
                        if (_total > 0) {
                            $scope.VideoStartEnabled = false;
                            return;
                        }
                    }
                }
            }
            $scope.CTAverage();
        } catch (e) {

        }
    }

    function ShowAverage(index) {
        $scope.VideoStartEnabled = true;
        var calvalue = $scope.AvgMM;
        var valArr = [];
        var Avg = 0.0;
        var _total = 0.00;
        var _count = 0;

        if ($('input[name="chkCycle"]:checked').length == 0) {
            for (var i = 1; i <= 5; i++) {
                var _CurrentValue = $("#CT_" + i + "_" + index).val();
                if (_CurrentValue != "") {
                    _total += dbl(_CurrentValue);
                    _count++;
                    valArr.push(_CurrentValue);
                    $scope.VideoStartEnabled = false;
                }
            }
        } else {
            for (var i = 1; i <= 5; i++) {
                if ($("#chkCycle" + i).is(':checked')) {
                    var _CurrentValue1 = $("#CT_" + i + "_" + index).val();
                    if (_CurrentValue1 != "") {
                        _total += dbl(_CurrentValue1);
                        _count++;
                        valArr.push(_CurrentValue1);
                        $scope.VideoStartEnabled = false;
                    }
                }
            }
        }
        //alert(calvalue);
        if (calvalue == "AVG")
            Avg = _total / _count;
        else if (calvalue == "MAX")
            Avg = Math.max.apply(Math, valArr);
        else if (calvalue == "MIN")
            Avg = Math.max.apply(Math, valArr);

        if (isNaN(dbl(Avg).toFixed(3))) {
            $("#CT_Avg_" + index).val('');
        }
        else {
            $("#CT_Avg_" + index).val(dbl(Avg).toFixed(3));
        }

        var _ratingVal = $("#CT_Rat_" + index).val();
        if (_ratingVal != "") {
            var _btSec = dbl(dbl(Avg).toFixed(3)) * dbl(_ratingVal) / 100;
            $("#CT_BT_SEC_" + index).val(dbl(_btSec).toFixed(3));
        }

        $scope.CTAverage();
    }

    $scope.CTAverageModel = [];
    $scope.TotalAverage = 0;
    $scope.TotalBTSec = 0;
    $scope.CTAverage = function () {

        var _id = $scope.VAS.AvgMaxMin;
        if (_id == "1")
            $scope.AvgMM = "AVG";
        else if (_id == "2")
            $scope.AvgMM = "MAX";
        else if (_id == "3")
            $scope.AvgMM = "MIN";


        var calvalue = $scope.AvgMM;
        $scope.CTAverageModel = [];
        for (var i = 1; i <= 5; i++) {
            var _total = 0;
            var _count = 0;

            var MaxVal = 0.000; var MinValue = 0.000;
            for (var index = 1; index <= 100; index++) {
                var _CurrentValue1 = $("#CT_" + i + "_" + index).val();
                if (_CurrentValue1 != "") {
                    _total += dbl(_CurrentValue1);
                    _count++;

                    if (MinValue == 0.000)
                        MinValue = dbl(_CurrentValue1);


                    var cValue = angular.isUndefinedOrNull(_CurrentValue1) ? 0.000 : dbl(_CurrentValue1);
                    if (cValue > MaxVal)
                        MaxVal = cValue;

                    if (cValue < MinValue)
                        MinValue = cValue;
                }


            }

            var Avg = 0.000;
            if (calvalue == "AVG") {
                if (_count > 0)
                    Avg = _total / _count;
            }
            else if (calvalue == "MAX") {
                Avg = MaxVal;
            }
            else if (calvalue == "MIN")
                Avg = MinValue;

            $scope.CTAverageModel.push({ CT: i, Value: _total == 0 ? '' : _total.toFixed(3) });

            //total average and  BT
            $scope.TotalAverage = 0;
            $scope.TotalBTSec = 0;
            for (var index = 1; index <= 100; index++) {
                $scope.TotalAverage += dbl($("#CT_Avg_" + index).val());
                $scope.TotalBTSec += dbl($("#CT_BT_SEC_" + index).val());
            }
            $scope.TotalAverage = $scope.TotalAverage == 0 ? '' : $scope.TotalAverage.toFixed(3);
            $scope.TotalBTSec = $scope.TotalBTSec == 0 ? '' : $scope.TotalBTSec.toFixed(3);
        }

    }

    function dbl(val) {
        if (!isNaN(parseFloat(val)) && isFinite(val))
            return parseFloat(val);

        return 0;
    }

    $scope.setPlay = function (id, index) {
        var vEs = angular.element(document.querySelector('#vdidm'));
        $scope.fromToRow = $scope.fromToTable[index];
        vEs[0].src = $scope.video_url + "#t=" + $scope.fromToRow.StartTime + "," + $scope.fromToRow.EndTime + "";
    };
    $scope.setRate = function (rate) {
        var vEs = angular.element(document.querySelector('#vdid'));
        vEs[0].playbackRate = rate;
        $scope._plabackrate = rate;
        $scope.speedName = 'x' + rate;
    };

    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.SetGSDValue = function (obj) {
        var OperationVariationSystemId = $scope.VAS.OperationVariationSystemId;
        if (OperationVariationSystemId !== "") {
            var _buttonID = obj.target.attributes.id.value.replace("GSD_", '');
            $scope.GSDButtonID = _buttonID;
            angular.element(document.querySelector('#modalGSD')).modal('toggle');
        }
        else {
            ShowResult('Please Select a Operation..!', 'failure');
            return false;
        }
    };
    $scope.setCycle = function (newCycle) {
        $scope.cycleName = "CT" + newCycle;
        $scope.cycle = newCycle;
    };
    $scope.setAvgMaxMin = function (AvgId) {

        $scope.VAS.AvgMaxMin = AvgId;
        if (AvgId == "1")
            $scope.AvgMM = "AVG";
        else if (AvgId == "2")
            $scope.AvgMM = "MAX";
        else if (AvgId == "3")
            $scope.AvgMM = "MIN";

        $scope.FindCycleAvgMaxMin();

    };

    $scope.changeRatings = function (Index) {
        var _ratingVal = $("#CT_Rat_" + Index).val();
        if (_ratingVal > 100) {
            ShowResult("Max Value Exceeded !!", 'failure');
            $("#CT_Rat_" + Index).val("");
            $("#CT_Rat_" + Index).focus();
            return false;
        }
        var _avgValue = $("#CT_Avg_" + Index).val();
        if (_ratingVal != "" && _avgValue != "") {
            var _btSec = dbl(_avgValue) * dbl(_ratingVal) / 100;
            $("#CT_BT_SEC_" + Index).val(dbl(_btSec).toFixed(3));
        }
        else {
            $("#CT_BT_SEC_" + Index).val('');
        }


    };

    $scope.FindCycleAvgMaxMin = function () {
        if ($scope.AvgMM == "") {
            var _id = $scope.VAS.AvgMaxMin;
            if (_id == "1")
                $scope.AvgMM = "AVG";
            else if (_id == "2")
                $scope.AvgMM = "MAX";
            else if (_id == "3")
                $scope.AvgMM = "MIN";
        }
        var calvalue = $scope.AvgMM;
        var arrVal = [];
        var calculatedValue = 0.00;
        var txtVal = 0.00;
        var _count = 0;
        for (var i = 1; i <= 100; i++) {
            if ($('input[name="chkCycle"]:checked').length == 0) {
                for (var j = 1; j <= 5; j++) {
                    var txtcurrentVal = $("#CT_" + j + "_" + i).val();
                    if (txtcurrentVal != "") {
                        txtVal += dbl(txtcurrentVal);
                        _count++;
                        arrVal.push(txtcurrentVal);
                    }
                    else {
                        txtVal += dbl(0.00);
                    }
                }
            }
            else {
                arrVal = [];
                for (var k = 1; k <= 5; k++) {
                    if ($("#chkCycle" + k).is(':checked')) {
                        var txtcurrentVal1 = $("#CT_" + k + "_" + i).val();
                        if (txtcurrentVal1 != "") {
                            txtVal += dbl(txtcurrentVal1);
                            _count++;
                            arrVal.push(txtcurrentVal1);
                        }
                        else {
                            txtVal += dbl(0.00);
                        }
                    }
                }
            }

            if (calvalue == "AVG") {
                if (_count == 0 && txtVal == 0) {
                    calculatedValue = "0.00";
                }
                else {
                    calculatedValue = txtVal / _count;
                }
            }
            else if (calvalue == "MAX") {
                if (arrVal.length > 0) {
                    calculatedValue = Math.max.apply(Math, arrVal);
                }
                else {
                    calculatedValue = 0.00;
                }
            }
            else if (calvalue == "MIN") {
                if (arrVal.length > 0) {
                    calculatedValue = Math.min.apply(Math, arrVal);
                }
                else {
                    calculatedValue = 0.00;
                }
            }

            //findout the value
            if (calculatedValue == "0.00") {
                $("#CT_Avg_" + i).val("");
            }
            else {
                $("#CT_Avg_" + i).val(dbl(calculatedValue).toFixed(3));
            }
            $scope.changeRatings(i);
            txtVal = 0.00;
            calculatedValue = 0.00;
            _count = 0;
            arrVal = [];
        }

        $scope.CTAverage();
    };

    $scope.getCalculateSAM = function () {
        $scope.IsCalculation = true;
        var BTSECSum = 0.0;
        var CTRating = 0.0;
        var Gross_GSD_SAM_SUM = 0.0;
        var Final_GSD_SAM_SUM = 0.0;
        var Total_GSD_SAM_SUM = 0.0;
        var TotalAllowance = 0.0;
        var CaltotalAllowance = 0.0;
        var FinalSAM = 0.00;

        for (var i = 1; i <= $scope.TotalElement; i++) {
            var _isValue = false;
            for (var j = 1; j <= 5; j++) {
                var _cycleValue = $("#CT_" + j + "_" + i).val();
                if (_cycleValue != "")
                    _isValue = true;
            }
            if (_isValue === true) {
                if ($("#GSD_" + i).val() === "") {
                    ShowResult('Select Element Code for Element - ' + i, 'failure');
                    $scope.IsCalculation = false;
                    return false;
                }

                if ($("#ddlET_" + i).val() === null || $("#ddlET_" + i).val() === "") {
                    ShowResult('Select Element Type for Element - ' + i, 'failure');
                    $scope.IsCalculation = false;
                    return false;
                }

                if ($("#CT_Rat_" + i).val() === "") {
                    ShowResult('Insert Rating Value for Element - ' + i, 'failure');
                    $scope.IsCalculation = false;
                    return false;
                }
            }
        }


        $('.CTRating').each(function () {
            if ($(this).val() !== "") {
                CTRating += dbl(InitValue($(this).val()));
            }
        });

        if (CTRating === 0.0 || CTRating === "") {
            ShowResult('Rating(%) Value is Empty..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        $('.BTSEC').each(function () {
            if ($(this).val() !== "") {
                var _gsdID = $(this).attr('id').replace('CT_BT_SEC_', 'GSD_');
                var _gsdValue = $("#" + _gsdID).val();
                BTSECSum += dbl(InitValue($(this).val()));
            }
        });

        if (BTSECSum === 0.0 || BTSECSum === "") {
            ShowResult('BT(SEC) Value is Empty..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }
        $scope.VAS.Frequency = InitValue($scope.VAS.Frequency);
        var _isFrequency = $scope.VAS.Frequency;

        if (_isFrequency == "") {
            ShowResult('Frequency Should Not be Empty..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        BTSECSum = (dbl(BTSECSum) / 60) * dbl(_isFrequency);

        $('.GSD_TMU').each(function () {
            if ($(this).val() != "") {
                var _SelectGSDColumn = $(this).attr("id").replace('hd', '');
                var _GSDCode = $("#" + _SelectGSDColumn).val();
                Gross_GSD_SAM_SUM += dbl(InitValue($(this).val()));
            }
        });

        $("#txtTotalTMU").val(Gross_GSD_SAM_SUM);

        if (Gross_GSD_SAM_SUM === 0.0 || Gross_GSD_SAM_SUM === "") {
            ShowResult('Select Element Code..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        if ($scope.VAS.MachineAllowances === "") {
            ShowResult('Enter Machine Allowances..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        if ($scope.VAS.PersonalAllowances === "") {
            ShowResult('Enter Personal Allowances..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        if ($scope.VAS.AdditionalAllowances === "") {
            ShowResult('Enter Additional Allowances..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        if (baseService.isUndefinedOrNull($scope.VAS.ProductionSystemId)) {
            ShowResult('Select Production System Allowance..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }

        if ($scope.VAS.BHTValue == "" || $scope.VAS.BHTValue == "0" || baseService.isUndefinedOrNull($scope.VAS.BHTValue)) {
            ShowResult('Select Production System Allowance Value..!', 'failure');
            $scope.IsCalculation = false;
            return false;
        }
        if (baseService.isUndefinedOrNull($scope.VAS.VASQuantity) || $scope.VAS.VASQuantity == '' || $scope.VAS.VASQuantity <= 0)
            $scope.VAS.VASQuantity = 1;


        $scope.VAS.PersonalAllowances = InitValue($scope.VAS.PersonalAllowances);
        $scope.VAS.AdditionalAllowances = InitValue($scope.VAS.AdditionalAllowances);
        $scope.VAS.MachineAllowances = InitValue($scope.VAS.MachineAllowances);
        $scope.VAS.BHTValue = InitValue($scope.VAS.BHTValue);

        Gross_GSD_SAM_SUM = (dbl(Gross_GSD_SAM_SUM) / 2000) * dbl(_isFrequency);

        TotalAllowance = dbl($scope.VAS.MachineAllowances) + dbl($scope.VAS.PersonalAllowances) + dbl($scope.VAS.AdditionalAllowances) + dbl($scope.VAS.BHTValue);

        CaltotalAllowance = (dbl(BTSECSum) * (1 + (dbl(TotalAllowance) / 100)));

        Total_GSD_SAM_SUM = (dbl(Gross_GSD_SAM_SUM));

        // var _BHTValue = $scope.VAS.BHTValue;

        FinalSAM = dbl(CaltotalAllowance / $scope.VAS.VASQuantity);
        Final_GSD_SAM_SUM = dbl(Total_GSD_SAM_SUM) + ((dbl(Total_GSD_SAM_SUM)) * (dbl(TotalAllowance) / 100));

        $scope.VAS.VASSAM = dbl(FinalSAM).toFixed(3);
        $scope.VAS.StandardSAM = dbl(Final_GSD_SAM_SUM).toFixed(3);
    };

    function InitValue($model) {
        try {
            if (baseService.isUndefinedOrNull($model))
                $model = 0;

        } catch (e) {

        }
        return $model;
    }

    $scope.clearAllElements = function () {
        $scope.message_confirmation = 'Are You Sure Want to Clear All Elements?';
        angular.element(document.querySelector('#confirmProcessClearElementPopUp')).modal('show');
    };

    $scope.clearElements = function () {
        $(".btnGSD").val("");
        $(".GSD_TMU").val("0");
        $(".btnGSDTMU").val("");
        $(".ddlET").val("");
        $('#divResultArea input').val('');
        $('input:checkbox[name=chkCycle]:checked').each(function () {
            $(this).prop("checked", false);
        });
        $scope.VAS.ProductionSystemId = "";
        $scope.VAS.BHTValue = 0;
        $scope.VAS.VASSAM = "";
        $scope.VAS.StandardSAM = "";
        $("#txtTotalTMU").val("");
        VideoDurationUsed();
    };

    $scope.clearSelectedField = function (Id) {
        $scope.SelectedCellId = Id;
        $scope.message_confirmation = 'Are You Sure Want to Clear Cells?';
        angular.element(document.querySelector('#confirmProcessClearCellPopUp')).modal('show');
    };

    $scope.clearSelectedCells = function () {
        var Id = $scope.SelectedCellId;

        $("#GSD_" + Id).val("");
        $("#hdGSD_" + Id).val("0");
        $("#ddlET_" + Id).val("");
        $("#CT_1_" + Id).val("");
        $("#CT_2_" + Id).val("");
        $("#CT_3_" + Id).val("");
        $("#CT_4_" + Id).val("");
        $("#CT_5_" + Id).val("");
        $("#CT_Avg_" + Id).val("");
        $("#CT_Rat_" + Id).val("");
        $("#CT_BT_SEC_" + Id).val("");
        $("#GSDTMU_" + Id).val("");

        $scope.CTAverage();
    };
    var moviecontainer = document.getElementById("customcontrols");
    var movie = moviecontainer.querySelector("video");
    var controls = moviecontainer.querySelector("figcaption");
    var playpause = document.getElementById("btnPlayPause");//controls.querySelector("a");
    movie.removeAttribute("controls");
    controls.style.display = "block";





    $scope.setPlayPauseActivity = function () {

        $scope.VideoStartEnabled = false;
        if ($scope.VAS.OperationVariationSystemId !== "") {
            if ($scope.VAS.FileName !== undefined && $scope.VAS.FileName !== "") {
                if (movie.paused) {
                    movie.play();
                    playpause.innerHTML = "<i class='fa fa-stop-circle-o fa-fw'></i>";
                    $scope.getStart();
                } else {
                    movie.pause();
                    playpause.innerHTML = "<i class='fa fa-play-circle-o fa-fw'></i>";
                    $scope.getEnd();
                }
            }
            else {
                ShowResult('Please Select a Video File..!', 'failure');
                return false;
            }

            //VideoDurationUsed();
        }
        else {
            ShowResult('Please Select Operation..!', 'failure');
            return false;
        }
    };

    document.querySelector('video').addEventListener('ended', function (evt) {
        playpause.innerHTML = "<i class='fa fa-play-circle-o fa-fw'></i>";
    });

    $scope.reloadVideo = function () {
        var vEs = angular.element(document.querySelector('#vdid'));
        vEs[0].pause();
        vEs[0].currentTime = 0;

        var times = $scope.VAS.videoStartTime.split(":");
        var totalSeconds = (dbl(times[0]) * 3600) + (dbl(times[1]) * 60) + (dbl(times[2]));
        if (totalSeconds > vEs[0].currentTime) {
            vEs[0].currentTime = totalSeconds;
        }

        vEs[0].load();
        $("#custom-seekbar span").css("width", "0%");
        //$scope.SetVideoStartPoint(null);
        VideoDurationUsed();
    };

    $scope.playBackward = function () {
        var vEs = angular.element(document.querySelector('#vdid'));
        vEs[0].currentTime += -10;

        var times = $scope.VAS.videoStartTime.split(":");
        var totalSeconds = (dbl(times[0]) * 3600) + (dbl(times[1]) * 60) + (dbl(times[2]));
        if (totalSeconds > vEs[0].currentTime) {
            vEs[0].currentTime = totalSeconds;
        }

        vEs[0].play();
    };

    $scope.createNewVersion = function () {
        $scope.tab = 1;
        $scope.cycle = 1;
        $scope.cycleName = "CT1";
        $scope.speedName = "x1";
        $scope.AvgMaxMin = 1;
        $scope.AvgMM = "AVG";

        $scope.VAS.Id = "";
        $scope.VAS.OperationVersion = "0";
        $scope.Action = 'Save';
        $scope.CopyVersion = $scope.VAS.Version;
        $scope.IsNewVideo = false;
        $scope.getMaxVersion();
        $scope.IsVisible = false;
        $scope.IsCreateNewVersion = false;
        $scope.isVersionApproved = null;
        $("#filec").prop("disabled", true);
    };

    $scope.getMaxVersion = function () {
        $http({
            method: 'POST',
            url: $scope.path + 'GetMaxVersion?OperationVariationSystemId=' + $scope.VAS.OperationVariationSystemId

        }).then(function successCallback(response) {
            $scope.VAS.Version = response.data[0]["Version"];
        });
    };

    $scope.changeTimes = function (ID) {

        ShowAverage(ID);
    };

    $scope.Clear = function () {
        $("#filec").val([]);
        $(".GSD_TMU").val("");
        $(".btnGSDTMU").val("");
        $(".ddlET").val("");
        $('#divResultArea input').val('');
        $(".GSD_TMU").val(0);
        $('input:checkbox[name=chkCycle]:checked').each(function () {
            $(this).prop("checked", false);
        });
        $("#filec").prop("disabled", true);
        $scope.operationdata = {};
        $scope.VAS = {};
        $scope.VAS.AvgMaxMin = 1;
        $scope.VAS.Version = "";
        $scope.VAS.OperationVersion = "0";
        $scope.VAS.videoStartTime = "00:00:00";
        $scope.VAS.OperatorId = '';
        $scope.VAS.Remarks = '';
        $scope.VAS.VasDescription = null;
        $scope.VAS.VASQuantity = 1;
        $scope.isVersionApproved = null;
        var videoElement = document.getElementById('vdid');
        videoElement.pause();
        videoElement.removeAttribute('src');
        videoElement.load();
        $("#custom-seekbar span").css("width", "0%");
        $scope.SetVideoStartPoint(null);
        VideoDurationUsed();

        $("#spOVName").text("");
        $("#spOPName").text("");
        $("#spMCName").text("");
        $("#txtTotalTMU").val("");
        $("#spFileName").empty();
        $scope.tab = 1;
        $scope.cycle = 1;
        $scope.cycleName = "CT1";
        $scope.speedName = "x1";
        $scope.AvgMaxMin = 1;
        $scope.AvgMM = "AVG";
        $scope.IsVisible = false;
        $('#vdid').empty();
        $scope.operationVersionList = [];
        $scope.CTAverage();
    };
}