﻿'use strict';
app.controller('risksController',
    ['authService', 'dataService', 'staffService',
function (authService, dataService, staffService) {
    var controller = this;

    controller.isAdmin = authService.authentication.isAdmin();

    controller.rtOptions = {
        'stateSave': true,
        'pageLength': 100
    };

    controller.riskmaps = [];
    controller.message = "";
    controller.themes = ["Personal Circumstances", "Behaviour", "Status"];

    controller.loadAndViewRiskMap = function(riskMap) {
        controller.currentRiskMap = riskMap;
        controller.view = 'riskMap';
        controller.currentRisks = [];
        angular.forEach(controller.risks, function(risk) {
            if (controller.currentRiskMap.riskIds.indexOf(risk.id) != -1)
                controller.currentRisks.push(risk);
        });
    } // loadAndViewRiskMap

    controller.newRiskMap = function () {
        controller.view = 'newRiskMap';
        controller.currentRiskMap = { name: '' };
        controller.editRisks = [];
    } // newRiskMap

    controller.editRiskMap = function() {
        controller.view = 'editRiskMap';
        controller.editRisks = [];
        angular.forEach(controller.risks, function(risk) {
            if (controller.currentRiskMap.riskIds.indexOf(risk.id) != -1)
                controller.editRisks.push(risk);
        });
    } // editRiskMap

     controller.isRiskInRiskMap = function(editRisks, risk) {
        for (var i = 0; i !== editRisks.length; ++i)
            if (editRisks[i].id === risk.id)
                return true;
        return false;
    } // isRiskInRiskMap

    controller.toggleRiskMapRisk = function(editRisks, risk) {
        for (var i = 0; i !== editRisks.length; ++i)
            if (editRisks[i].id === risk.id) {
                editRisks.splice(i, 1);
                return;
            }
        editRisks.push(risk);
    } // toggleRiskMapRisk

    function gatherRiskIds(editRisks) {
        var riskIds = "";
        for (var i = 0; i !== editRisks.length; ++i) {
            if (riskIds !== "")
                riskIds += "|";
            riskIds += editRisks[i].id;
        }
        return riskIds;
    }

    controller.updateRiskMap = function(editRisks) {
        controller.currentRiskMap.riskIds = gatherRiskIds(editRisks);
        controller.message = "Saving Risk Map ...";
        dataService.updateRiskMap(controller.currentRiskMap).then(function (results) {
            controller.message = "";
            controller.loadAndViewRiskMap(results.data);
            controller.reload();
        });
    } // updateRiskMap

    controller.createRiskMap = function (editRisks) {
        controller.currentRiskMap.riskIds = gatherRiskIds(editRisks);
        controller.message = "Creating Risk Map ...";
        dataService.createRiskMap(controller.currentRiskMap).then(function (results) {
            controller.message = "";
            controller.loadAndViewRiskMap(results.data);
            controller.reload();
        });
    }

    controller.loadAndViewRisk = function(risk) {
        controller.currentRisk = risk;
        controller.view = 'risk';
    } // loadAndViewRisk

    controller.showRisks = function() {
        controller.view = 'home';
        controller.viewRisks = true;
    } // showRisks

    controller.newRisk = function () {
        controller.newMode = true;
        controller.riskToEdit = {};
        if (!controller.isAdmin)
            controller.riskToEdit.score = 3;
        controller.view = 'newRisk';
    }

    controller.editRisk = function () {
        controller.newMode = false;
        controller.riskToEdit = clone(controller.currentRisk);
        controller.view = 'editRisk';
    } // editRisk

    controller.updateRisk = function (risk) {
        controller.message = "Saving Risk ...";
        dataService.updateRisk(risk).then(function (results) {
            controller.message = "";
            controller.loadAndViewRisk(results.data);
            controller.reload();
        });
    }

    controller.createRisk = function (risk) {
        controller.message = "Creating Risk ...";
        dataService.createRisk(risk).then(function (results) {
            controller.message = "";
            controller.loadAndViewRisk(results.data);
            controller.reload();
        });
    }

    controller.deleteRisk = function (risk) {
        controller.message = "Deleting Risk ...";
        dataService.deleteRisk(risk).then(function (results) {
            controller.message = "";
            controller.showRisks();
            controller.reload();
        });
    }

    controller.refresh = function () {
        controller.view = 'home';
        controller.currentRiskMap;
        controller.currentRisks = [];
        controller.reload();
    } // refresh

    controller.reload = function () {
        controller.message = "Loading Risk Maps ...";
        dataService.listRiskMaps().then(function (results) {
            controller.message = "Loading Risks ...";
            controller.riskmaps = results.data;
            dataService.listRisks().then(function (results) {
                controller.message = "";
                controller.risks = results.data;
            }, function (error) {
                //alert(error.data.message);
            });
        }, function (error) {
            //alert(error.data.message);
        });
    }

    function clone(obj) {
        if (obj == null || typeof (obj) != 'object')
            return obj;

        var temp = obj.constructor();

        for (var k in obj) {
            if (obj.hasOwnProperty(k)) {
                temp[k] = clone(obj[k]);
            }
        }
        return temp;
    } // clone

    if (!controller.isAdmin) {
        controller.closeRisks = function () {
            window.location = '#/supervisor';
        }

        var _detailsLoaded = function (staffDetails) {
            controller.message = "";
            controller.name = staffDetails.name;
            controller.organisation = staffDetails.organisation;

            controller.refresh();
        } // detailsLoaded

        staffService.load("supervisor").then(_detailsLoaded, controller.errorHandler);
    } else {
        controller.refresh();
    }
}]);