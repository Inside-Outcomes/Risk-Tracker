'use strict';
app.controller('adminRisksController',
    ['dataService', function (dataService) {
    var controller = this;

    controller.rtOptions = {
        'stateSave': true,
        'pageLength': 100
    };


    controller.riskmaps = [];
    controller.message = "";

    controller.loadAndViewRiskMap = function(riskMap) {
        controller.currentRiskMap = riskMap;
        controller.view = 'riskMap';
    } // loadAndViewRiskMap

    controller.newRiskMap = function () {
        controller.view = 'newRiskMap';
        controller.currentRiskMap = { name: '' };
        controller.editRisks = [];
    } // newRiskMap

    controller.editRiskMap = function() {
        controller.view = 'editRiskMap';
        controller.editRisks = [];
        angular.forEach(controller.currentRiskMap.risks, function(risk) {
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

    controller.updateRiskMap = function(editRisks) {
        controller.currentRiskMap.risks = editRisks;
        controller.message = "Saving Risk Map ...";
        dataService.updateRiskMap(controller.currentRiskMap).then(function (results) {
            controller.message = "";
            controller.loadAndViewRiskMap(results.data);
            controller.reload();
        });
    } // updateRiskMap

    controller.createRiskMap = function (editRisks) {
        controller.currentRiskMap.risks = editRisks;
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
        controller.riskToEdit = {};
        controller.view = 'newRisk';
    } // newRisk

    controller.editRisk = function () {
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
    } // updateRisk

    controller.createRisk = function (risk) {
        controller.message = "Creating Risk ...";
        dataService.createRisk(risk).then(function (results) {
            controller.message = "";
            controller.loadAndViewRisk(results.data);
            controller.reload();
        });
    } // createRisk

    controller.deleteRisk = function (risk) {
        controller.message = "Deleting Risk ...";
        dataService.deleteRisk(risk).then(function (results) {
            controller.message = "";
            controller.showRisks();
            controller.reload();
        })
    } // deleteRisk

    controller.showOutcomeFrameworks = function () {
        controller.view = 'home';
        controller.viewOF = true;
    } // showRisks

    controller.newOutcomeFramework = function () {
        controller.frameworkToEdit = {};
        controller.view = 'newOutcomeFramework';
    } // newOutcomeFramework

    controller.editOutcomeFramework = function (framework) {
        controller.frameworkToEdit = clone(framework);
        controller.view = 'editOutcomeFramework';
    } // editOutcomeFramework

    controller.createOutcomeFramework = function (outcomeFramework) {
        controller.message = "Saving Outcome Framework ...";
        dataService.createOutcomeFramework(outcomeFramework).then(function (results) {
            controller.message = "";
            controller.showOutcomeFrameworks();
            controller.reload();
        });
    } // createOutcomeFramework 

    controller.updateOutcomeFramework = function (outcomeFramework) {
        controller.message = "Saving OutcomeFramework ...";
        dataService.updateOutcomeFramework(outcomeFramework).then(function (results) {
            controller.message = "";
            controller.showOutcomeFrameworks();
            controller.reload();
        });
    } // updateOutcomeFramework

    /////////////////
    controller.refresh = function () {
        controller.view = 'home';
        controller.currentRiskMap;
        controller.reload();
    } // refresh

    controller.reload = function () {
        _loadRiskMaps(controller);
    }

    function _loadRiskMaps(controller) {
        controller.message = "Loading Risk Maps ...";
        dataService.listRiskMaps().then(function (results) {
            controller.message = "";
            controller.riskmaps = results.data;
            _loadRisks(controller);
        }, function (error) {
            controller.message = error.data.message;
        });
    } // _loadRiskMaps

    function _loadRisks(controller) {
        controller.message = "Loading Risks ...";
        dataService.listRisks().then(function (results) {
            controller.message = "";
            controller.risks = results.data;
            _loadOutcomeFrameworks(controller);
        }, function (error) {
            controller.message = error.data.message;
        })
    } // _loadRisks

    function _loadOutcomeFrameworks(controller) {
        controller.message = "Loading Outcome Frameworks ...";
        dataService.listOutcomeFrameworks().then(function (results) {
            controller.message = "";
            controller.outcomeframeworks = results.data;
        }, function (error) {
            controller.message = error.data.message;
        });
    } // _loadOutcomeFrameworks

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

    controller.refresh();
}]);