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
                        controller.view = "client";
                    },
                    controller.errorHandler);
        } // _openClient

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

        var _clientAddNoteDialog = function () {
            _dialogBox('app/views/dialog/notesDialog.html',
                       "Add Note",
                       "",
                       _saveNote);
        } // _clientAddNoteDialog

        var _clientEditNodeDialog = function (id, text) {
            _dialogBox('app/views/dialog/notesDialog.html',
                       'Edit Note',  
                       text,
                       function (noteText) {
                         _saveNote(noteText, "UpdateNote/" + id);
                       });
        } // _clientEditNoteDialog
 
        var _clientGeneralNote = function (generalNote) {
            _dialogBox('app/views/dialog/notesDialog.html',
                       'Edit General Note',
                       generalNote,
                       function (noteText) {
                         _saveNote(noteText, "AddGeneralNote");
                       });
        } // _clientGeneralNote

        var _clientReferToService = function () {
            _dialogBox('referralDialog.html',
                       "Refer to another service",
                       { service: "", note: "" },
                       function (referral) {
                         var noteText = "Referred to " + referral.service + "\n" + referral.note;
                         _saveNote(noteText, "AddReferral");
                       });
        } // _clientReferToService

        var _clientDidNotAttend = function () {
            _dialogBox('app/views/dialog/notesDialog.html',
                       "Did Not Attend",
                       "",
                       function (note) {
                           var noteText = "Did not attend\n" + note;
                           _saveNote(noteText, "DidNotAttend");
                       });
        } // _clientDidNotAttend

        var _clientEditName = function () {
            _dialogBox('nameDialog.html',
                       'Edit Name',
                       { name : controller.currentClient.name },
                       function (name) {
                           var updateClient = clone(controller.currentClient);
                           updateClient.name = name.name;
                           dataService.clientUpdate(controller.currentClient.id, updateClient).then(function (results) {
                               controller.currentClient = results.data;
                           }, _errorHandler);
                       });
        }

        var _clientEditReferenceId = function () {
            _dialogBox('referenceIdDialog.html',
                       'Edit Reference ',
                       { name: controller.currentClient.referenceId },
                       function (name) {
                           var updateClient = clone(controller.currentClient);
                           updateClient.referenceId = name.name;
                           dataService.clientUpdate(controller.currentClient.id, updateClient).then(function (results) {
                               controller.currentClient = results.data;
                           }, _errorHandler);
                       });
        }

        var _clientEditAddress = function () {
            var addressToEdit =
            _dialogBox('addressDialog.html',
                       "Edit Address",
                       clone(controller.currentClient.address),
                       function (address) {
                         var updateClient = clone(controller.currentClient);
                         updateClient.address = address;
                         dataService.clientUpdate(controller.currentClient.id, updateClient).then(function (results) {
                           controller.currentClient = results.data;
                         }, _errorHandler);
                       });
        } // _clientEditAddress

        var _clientEditQuestions = function () {
            _dialogBox('questionsDialog.html',
                       'Update Questions',
                       clone(controller.currentClient.questions),
                       function (questions) {
                           dataService.clientQuestions(controller.currentClient.id, questions).then(function (results) {
                               controller.currentClient = results.data;
                           }, _errorHandler);
                       });
        }

        var _clientEditDemographics = function () {
            var demo = {
                client: clone(controller.currentClient.demographics),
                choices: demographics
            };
            _dialogBox('demographicDialog.html',
                       "Update Demographic Information",
                       demo,
                       function (demo) {
                           var updateClient = clone(controller.currentClient);
                           updateClient.demographics = demo.client;
                           dataService.clientUpdate(controller.currentClient.id, updateClient).then(function (results) {
                               controller.currentClient = results.data;
                           }, _errorHandler);
                       });
        } // _clientEditDemographics

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
        var _clientResolveRisk = function (riskId) {
            _clientRisk(dataService.clientResolveRisk, riskId);
        } // _clientResolveRisk
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

        var _clientDischarge = function () {
              _dialogBox('dischargeDialog.html',
                        "Discharge Client",
                        "",
                        function (note) {
                            dataService.clientDischarge(controller.currentClient.id, note).then(function (results) {
                                controller.refreshClientList();
                                controller.showClientList();
                            }, _errorHandler);
                        });
        } // _dischargeClient

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

        var _clientChooseUploadFile = function () {
            $scope.files = null;
            $scope.progress = 0;
            $scope.message = ""
            $scope.error = "";
            $scope.uploaded = []

            $scope.uploadFn = function (files) {
                if (!(files && files.length))
                    return;

                $scope.error = "";
                $scope.progress = 0;
                for (var i = 0; i < files.length; ++i) {
                    (function (file) {
                        for (var index in $scope.uploaded)
                            if (file.name === $scope.uploaded[index])
                                return;

                        $scope.uploaded.push(file.name);

                        Upload.upload({
                            url: 'api/Client/' + controller.currentClient.id + '/FileUpload',
                            file: file
                        }).progress(function (evt) {
                            $scope.message = "Uploading " + file.name;
                            var prog = parseInt(100.0 * evt.loaded / evt.total);
                            $scope.progress = prog;
                        }).success(function (data, status, headers, config) {
                            $scope.message = "Uploaded " + file.name;
                            controller.currentClient = data;
                        }).error(function (data, status, headers, config) {
                            $scope.message = "";
                            $scope.error += file.name + " upload failed : " + status + ".";
                        });
                    })(files[i]);
                } // for ...
            }
            
            $scope.$watch('files', function () { $scope.uploadFn($scope.files); });

            _dialogBox('uploadFileDialog.html',
                       "Upload File",
                       $scope,
                       function () { });
        } // _clientChooseUploadFile

        var _downloadFile = function (fileGuid) {
            dataService.downloadFile(controller.currentClient.id, fileGuid);
        } // _downloadFile

        var _deleteFile = function (fileName, fileGuid) {
            controller.message = "Deleting " + fileName + " ...";
            dataService.deleteFile(controller.currentClient.id, fileGuid).then(function (results) {
                controller.currentClient.files = results.data.files;
                controller.message = "";
            }, _errorHandler);
        } // _deleteFile

        var _newClientForm = function () {
            controller.newclient = {
                address: {}
            };
            controller.availableProjects = controller.allProjects;
            if (controller.availableProjects.length == 1)
                controller.newclient.projId = controller.availableProjects[0].id
            controller.updateAvailableLocations();
            controller.view = 'newclient';
        } // _newClientForm

        var _checkReferenceId = function () {
            if (!controller.newclient.referenceId)
                return;
            dataService.referenceCheck(controller.organisation, controller.newclient.referenceId).then(function (results) {
                controller.newclient.references = results.data;
                controller.newclient.referenceWarning = results.data.projects.length || results.data.discharged.length;
            });
        } // _checkReferenceId

        var _clientReopenNoteDialog = function (name, guid) {
            _dialogBox('app/views/dialog/notesDialog.html',
                       'Reopen ' + name,
                       "",
                       function (noteText) { _reopenClient(guid, noteText) });
        } // _clientReopenNoteDialog

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
            controller.message = "Creating ...";
            dataService.
                createClient(controller.newclient).
                then(
                    function (request) {
                        controller.openClient(request.data.id);
                        controller.refreshClientList();
                    },
                    controller.errorHandler);
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

        controller.clientAddNoteDialog = _clientAddNoteDialog;
        controller.clientEditNoteDialog = _clientEditNodeDialog;
        controller.clientReferToService = _clientReferToService;
        controller.clientDidNotAttend = _clientDidNotAttend;
        controller.clientGeneralNote = _clientGeneralNote;

        controller.clientEditName = _clientEditName;
        controller.clientEditReferenceId = _clientEditReferenceId
        controller.clientEditAddress = _clientEditAddress;
        controller.clientEditQuestions = _clientEditQuestions;
        controller.clientEditDemographics = _clientEditDemographics;
        controller.clientAddRisk = _clientAddRisk;
        controller.clientResolveRisk = _clientResolveRisk;
        controller.clientRemoveRisk = _clientRemoveRisk;
        controller.clientReopenRisk = _clientReopenRisk;

        controller.clientDischarge = _clientDischarge;
        controller.clientDelete = _clientDelete;

        controller.toggleGuidance = _toggleGuidance;
        controller.showGuidance = _showGuidance;

        controller.errorHandler = _errorHandler;

        controller.newClientForm = _newClientForm;
        controller.checkReferenceId = _checkReferenceId;
        controller.clientReopenNoteDialog = _clientReopenNoteDialog;
        controller.createNewClient = _createNewClient;

        controller.updateAvailableLocations = _updateAvailableLocations;

        controller.clientChooseUploadFile = _clientChooseUploadFile;
        controller.clientChangeLocationProject = _clientChangeLocationProject;

        controller.downloadFile = _downloadFile;
        controller.deleteFile = _deleteFile;
      };

    var clientControl = {};
    clientControl.setup = _setupClientController;
    return clientControl;
}());

var demographics = (function () {
    var demographics = {};
    demographics.ethnicOrigin = [
        "",
        "White British",
        "White English",
        "White Scottish",
        "White Welsh",
        "White Northern Irish",
        "White Irish",
        "White Gypsy or Irish Traveller",
        "White Other",
        "Mixed White & Black Carribean",
        "Mixed White & Black African",
        "Mixed Other",
        "Asian or Asian British Bangladeshi",
        "Asian or Asian British Chinese",
        "Asian or Asian British Indian",
        "Asian or Asian British Pakistani",
        "Asian or Asian British Other",
        "Black or Black British African",
        "Black or Black British Caribbean",
        "Black or Black British Other",
        "Other Arab",
        "Other Any Other",
        "Unknown",
        "Declined to Reply"
    ];
    demographics.gender = [
        "",
        "Male",
        "Female",
        "Trans",
        "Trans-male",
        "Trans-female",
        "Unknown",
        "Declined to Reply"
    ];
    demographics.maritalStatus = [
        "",
        "Single",
        "Married",
        "Co-habiting",
        "Civil partnership",
        "Separated but legally married/in civil partnership",
        "Divorced/civil partnership dissolved",
        "Widowed/suriving civil partner"
    ];
    demographics.householdType = [
        "",
        "Single person with dependent children",
        "Single person without dependent children",
        "Couple with dependent children",
        "Couple without dependent children",
        "Other adults only"
    ];
    demographics.housingType = [
        "",
        "Own outright",
        "Buying home (mortgage)",
        "Shared ownership",
        "Council/ALMO tenant",
        "Housing association/RSL tenant",
        "Private tenant",
        "Housing through job/business",
        "Staying with friends/relatives (rent free)",
        "Staying with friends/relatives (paying rent)",
        "Hostel",
        "Prison",
        "Homeless (incl B&B tenant)",
        "Other"
    ];
    demographics.householdIncome = [
        "",
        "<£400pcm",
        "£400 - £599pcm",
        "£600 - £799pcm",
        "£800 - £999pcm",
        "£1000 - £1499pcm",
        "£1500 - £1999pcm",
        "£2000 - £2999pcm",
        ">£3000 pcm"
    ];
    demographics.employmentStatus = [
        "",
        "Employed >30 hours per week",
        "Employed between 16 and 29 hours a week",
        "Employed <16 hours per week",
        "Unemployed - seeking paid employment",
        "Self-employed",
        "Volunteer",
        "Retired",
        "Student",
        "On Government scheme for employment",
        "Carer - elderly/disabled",
        "Carer - children",
        "Looking after home - non-dependents",
        "Looking after home - dependents",
        "Permanently sick/disabled",
        "Other"
    ];
    demographics.disability = [
        "",
        "Not disabled/no health problems",
        "Disabled",
        "Long term health condition",
        "Unknown/withheld"
    ];
    demographics.disabilityType = [
        "",
        "Long term health condition",
        "Mental Health",
        "Physical impairment (non-sensory)",
        "Visual impairment",
        "Hearing impairment",
        "Deaf",
        "Learning difficulty",
        "Cognitive impairment",
        "Multiple impairments",
        "Other disability/type not given"
    ];
    return demographics;
})();

