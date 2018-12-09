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

        var _sendAndPrint = function () {
            var doneFn = function() {
                _dialogBox('completedReport.html',
                           "Referral Report Completed",
                           controller.currentClient,
                           _createNewClient
                           );
            };
            var errorFn = function () {
                controller.currentClient.noemail = false;
                _dialogBox('errorReport.html',
                           "Referral Report Error",
                           controller.currentClient,
                           _sendAndPrint);
            }
            if (controller.currentClient.noemail) 
                dataService.closeReferralReport(controller.currentClient.id).then(doneFn, errorFn);
            else
                dataService.emailReferralReport(controller.currentClient.id).then(doneFn, errorFn);
        } // _sendAndPrint

        var _completeWizard = function () {
            controller.currentClient.noemail = false;
            _dialogBox('emailDisclaimer.html',
                       "Referral Report Completion",
                       controller.currentClient,
                       _sendAndPrint);
        } // _completedWizard

        var _printForm = function () {
            window.print();
        } // _printForm

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
                address: {},
            };

            if (controller.currentProject)
                controller.selectProject(controller.currentProject);
            else {
                if (controller.allProjects && controller.allProjects.length == 1)
                    controller.selectProject(controller.allProjects[0]);
                else
                    controller.view = 'chooseproject';
            }

            controller.wizard = "client";
        } // _createNewClient

        var _selectProject = function (project) {
            controller.currentProject = project;
            if (controller.currentClient)
                controller.currentClient.projectId = controller.currentProject.id
            if (controller.riskMap.name != project.riskFramework) {
                controller.message = "Loading ...";
                dataService.fetchRiskMap(project.riskFramework).then(function (results) {
                    controller.message = "";
                    controller.riskMap = results.data;
                }, controller.errorHandler);
            } // if ...
            controller.view = "triage";
        } // selectProject

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

        controller.selectProject = _selectProject;
        controller.openClient = _openClient;
        controller.nextToRiskMap = _nextToRiskMap;
        controller.backToClient = _backToClient;
        controller.nextToReferrals = _nextToReferrals;
        controller.backToRiskMap = _backToRiskMap;
        controller.completeWizard = _completeWizard;
        controller.printForm = _printForm;

        controller.clientAddRisk = _clientAddRisk;
        controller.clientManageRisk = _clientManageRisk;
        controller.clientRemoveRisk = _clientRemoveRisk;
        controller.clientReopenRisk = _clientReopenRisk;

        controller.clientDelete = _clientDelete;

        controller.toggleGuidance = _toggleGuidance;
        controller.showGuidance = _showGuidance;

        controller.errorHandler = _errorHandler;

        controller.createNewClient = _createNewClient;
      };

    var clientControl = {};
    clientControl.setup = _setupClientController;
    return clientControl;
}());


