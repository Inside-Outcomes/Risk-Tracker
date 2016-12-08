'use strict';
var clientController = (function () {
    var _setupClientController =
      function (controller,
                dataService,
                $modal,
                localStorageService,
                Upload,
                $scope) {
        controller.currentClient = {};
        controller.riskMap = { name: '' };

        controller.dtOptions = {
            'stateSave': true
        };

        var _openClient = function (id) {
            controller.message = "";
            controller.currentClient = {};
            dataService.
                fetchClient(id).
                then(
                    function (request) {
                        controller.message = "";
                        controller.currentClient = request.data;
                        controller.riskAssessment = controller.currentClient.currentRiskAssessment.themeAssessments;
                        controller.wizard = "client";
                    },
                    controller.errorHandler);
        } // _openClient

        var _nextToRiskMap = function () {
            var nextAction = function (request) {
                controller.message = "";
                controller.currentClient = request.data;
                controller.riskAssessment = controller.currentClient.currentRiskAssessment.themeAssessments;
                controller.wizard = "riskmap";
            };

            controller.message = "Saving ...";
            if (!controller.currentClient.id)
                dataService.createClient(controller.currentClient).then(nextAction, controller.errorHandler);
            else
                dataService.clientUpdate(controller.currentClient.id, controller.currentClient).then(nextAction, controller.errorHandler);
        } // _nextToRiskMap

        var _backToClient = function () {
            controller.wizard = "client";
        } // _backToClient

        var _nextToReferrals = function () {
            var nextAction = function (request) {
                controller.message = "";
                controller.referrals = request.data;
                controller.wizard = "referrals";
            };

            controller.message = "Saving ...";
            dataService.loadReferralReport(controller.currentClient.id).then(nextAction, controller.errorHandler);
        }

        var _backToRiskMap = function () {
            controller.wizard = "riskmap";
        }

        var _completeWizard = function () {
          _dialogBox('completedReport.html',
                     "Referral Report Completed",
                     controller.currentClient,
                     function () {
                       controller.createNewClient();
                     });
        } // _completedWizard

        var _dialogBox = function (templateUrl,
                                   title,
                                   thingToEdit,
                                   saveFn) {
            var modalInstance = $modal.open({
                templateUrl: templateUrl,
                controller: 'editDialogController',
                backdrop: 'static',
                resolve: {
                    title: function () { return title },
                    thingToEdit: function () { return thingToEdit; }
                }
            });

            modalInstance.result.then(saveFn);
        } // _dialogBox

        var _saveNote = function (noteText, action) {
            dataService.clientAddNote(controller.currentClient.id, noteText, action).then(function (results) {
                controller.currentClient = results.data;
            }, _errorHandler);
        }

        var _clientChangeLocationProject = function () {
            var alternatives = {
                locations: [],
                projects: [],
                locationId: controller.currentClient.locationId,
                projectId: controller.currentClient.projectId
            };
            dataService.listProjects(controller.organisation).then(function (results) {
                alternatives.projects = results.data;
            });
            dataService.listLocations(controller.organisation).then(function (results) {
                alternatives.locations = results.data;
            });

            _dialogBox('changeLocationProject.html',
                       "Change Location or Project",
                       alternatives,
                       function (alternatives) {
                           var updateClient = clone(controller.currentClient);
                           updateClient.locationId = alternatives.locationId;
                           updateClient.projectId = alternatives.projectId;
                           dataService.clientUpdate(controller.currentClient.id, updateClient).then(function (results) {
                               controller.currentClient = results.data;
                           }, _errorHandler);
                       });
        } // _clientChangeLocationProject

        var _clientAddRisk = function (riskId) {
            _clientRisk(dataService.clientAddRisk, riskId);
        } // _clientAddRisk
        var _clientManageRisk = function (riskId) {
            _clientRisk(dataService.clientManageRisk, riskId);
        } // _clientManageRisk
        var _clientRemoveRisk = function (riskId) {
            _clientRisk(dataService.clientRemoveRisk, riskId);
        } // _clientRemoveRisk
        var _clientReopenRisk = function (riskId) {
            _clientRisk(dataService.clientReopenRisk, riskId);
        } // _clientReopenRisk
        var _clientRisk = function (actionFn, riskId) {
            controller.message = "";
            actionFn.call(dataService, controller.currentClient.id, riskId).then(function (result) {
                controller.currentClient = result.data;
                controller.riskAssessment = controller.currentClient.currentRiskAssessment.themeAssessments;
            }, _errorHandler);
        } // _clientRisk

        var _clientDelete = function () {
            _dialogBox('deleteClient.html',
                      "Delete Client",
                      "",
                      function (note) {
                          dataService.clientDelete(controller.currentClient.id).then(function (results) {
                              controller.refreshClientList();
                              controller.showClientList();
                          }, _errorHandler);
                      });
        } // _dischargeClient

        var _reopenClient = function (guid, noteText) {
            dataService.
                clientReopen(guid, noteText).
                then(
                    function (request) {
                        _openClient(guid);
                    },
                    controller.errorHandler);
        } // _reopenClient

        var _createNewClient = function () {
            controller.currentClient = {
                address: {}
            };
            controller.wizard = "client";
        } // _createNewClient

        var _updateAvailableLocations = function (projId) {
            controller.availableLocations = []

            controller.allLocations.forEach(function (location) {
                var add = false;
                location.projectIds.forEach(function (locProjId) {
                    if (locProjId == projId)
                        add = true;
                });
                if (add)
                    controller.availableLocations.push(location);
            });

            controller.newclient.locationId =
                (controller.availableLocations.length == 1) ? controller.availableLocations[0].id : undefined;
        } // _updateAvailableLocations

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

        var _toggleGuidance = function () {
            var _show = _showGuidance() ? "hide" : "show";
            localStorageService.set("showGuidance", _show);
        } // _toggleGuidance
        var _showGuidance = function () {
            var s = localStorageService.get("showGuidance");
            s = s || "show";
            return s == "show";
        } // _showGuidance

        var _errorHandler = function (error) {
            if (error.data && error.data.message)
                controller.message = error.data.message;
            else
                controller.message = error.status + ": " + error.statusText;
        } // errorHandler

        controller.openClient = _openClient;
        controller.nextToRiskMap = _nextToRiskMap;
        controller.backToClient = _backToClient;
        controller.nextToReferrals = _nextToReferrals;
        controller.backToRiskMap = _backToRiskMap;
        controller.completeWizard = _completeWizard;

        controller.clientAddRisk = _clientAddRisk;
        controller.clientManageRisk = _clientManageRisk;
        controller.clientRemoveRisk = _clientRemoveRisk;
        controller.clientReopenRisk = _clientReopenRisk;

        controller.clientDelete = _clientDelete;

        controller.toggleGuidance = _toggleGuidance;
        controller.showGuidance = _showGuidance;

        controller.errorHandler = _errorHandler;

        controller.createNewClient = _createNewClient;

        controller.updateAvailableLocations = _updateAvailableLocations;

        controller.clientChangeLocationProject = _clientChangeLocationProject;
      };

    var clientControl = {};
    clientControl.setup = _setupClientController;
    return clientControl;
}());


