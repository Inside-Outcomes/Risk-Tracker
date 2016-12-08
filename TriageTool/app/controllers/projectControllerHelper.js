'use strict';
var projectControllerHelper = (function () {
    var _setupController =
        function (controller,
                  dataService,
                  $modal,
                  restrictToProject) {

            controller.includeProjectChooser = !(restrictToProject);

            controller.addProjectQuestion = function (org, project) {
                var newQuestion = {
                    question: "",
                    answers: ""
                };

                var modalInstance = $modal.open({
                    templateUrl: 'projectQuestion.html',
                    controller: 'editDialogController',
                    backdrop: 'static',
                    resolve: {
                        title: function () { return "Project question" },
                        thingToEdit: function () { return newQuestion; }
                    }
                });

                modalInstance.result.then(function (question) {
                    dataService.addProjectQuestion(org, project, question).then(function (results) {
                        controller.allProjects = results.data;
                        project.questions.push(question);
                    })
                });
            } // projectQuestionPopup

            controller.updateProjectQuestion = function (org, project, originalQuestion) {
                var newQuestion = {
                    id: originalQuestion.id,
                    question: originalQuestion.question,
                    answers: originalQuestion.answers
                };

                var modalInstance = $modal.open({
                    templateUrl: 'projectQuestion.html',
                    controller: 'editDialogController',
                    backdrop: 'static',
                    resolve: {
                        title: function () { return "Project question" },
                        thingToEdit: function () { return newQuestion; }
                    }
                });

                modalInstance.result.then(function (question) {
                    dataService.updateProjectQuestion(org, project, question).then(function (results) {
                        controller.allProjects = results.data;
                        originalQuestion.question = newQuestion.question;
                        originalQuestion.answers = newQuestion.answers;
                        controller.viewQuestions = true;
                    })
                });
            } // projectQuestionPopup

            var errorHandler = function (error) {
                if (error.data && error.data.message)
                    controller.message = error.data.message;
                else
                    controller.message = error.status + ": " + error.statusText;
            } // errorHandler

            controller.addNewLocation = function (org, location) {
                dataService.addNewLocation(org, location, pd()).then(controller.updateLocationData, errorHandler);
            } // addNewLocation

            controller.updateLocation = function (org, location) {
                dataService.updateLocation(org, location, pd()).then(controller.updateLocationData, errorHandler);
            } // updateLocation

            controller.deleteLocation = function (org, location) {
                _dialogBox('confirmDialog.html',
                            "Delete Location",
                            "Are you sure you want to delete this location?",
                            function (note) {
                                controller.message = "Deleting ...";
                                dataService.deleteLocation(org, location, pd()).then(controller.updateLocationData, errorHandler);
                            });
            } // deleteProject

            controller.addLocationForm = function (org) {
                controller.location = { projectIds: [] };
                controller.view = 'newLocation';
                controller.buttonView = 'cancelToView';
            } // addStaffForm

            controller.editLocationForm = function (location) {
                controller.location = clone(location);
                dataService.canDeleteLocation(controller.organisation, controller.location).then(
                    function (results) {
                        controller.location.candelete = results.data;
                    }
                );
                controller.view = 'editLocation';
                controller.buttonView = 'cancelToView';
            } // editStaffForm

            controller.addStaffForm = function (org) {
                controller.staff = { projectIds: [], roles: [] };
                controller.view = 'newStaff';
                controller.buttonView = 'cancelToView';
            } // addStaffForm

            controller.editStaffForm = function (staff) {
                controller.staff = clone(staff);
                controller.view = 'editStaff';
                controller.buttonView = 'cancelToView';
            } // editStaffForm

            controller.toggleStaffRole = function (role) {
                var index = controller.staff.roles.indexOf(role);
                if (index > -1)
                    controller.staff.roles.splice(index, 1);
                else
                    controller.staff.roles.push(role);
            } // toggleStaffRole

            controller.addNewStaffMember = function (org, staff) {
                dataService.addNewStaffMember(org, staff, pd()).then(controller.updateStaff, errorHandler);
            } // addNewStaffMember

            controller.updateStaffMember = function (org, staff) {
                dataService.updateStaffMember(org, staff, pd()).then(controller.updateStaff, errorHandler);
            } // updateStaffMember

            controller.updateStaffPassword = function (org, staff) {
                controller.message = "Saving ...";
                dataService.updateStaffPassword(org, staff, pd()).then(
                    function (results) { controller.message = ""; },
                    errorHandler
                );
            } // updateStaffPassword

            controller.deleteStaffMember = function (org, staff) {
                _dialogBox('confirmDialog.html',
                            "Delete Staff Member",
                            "Are you sure you want to delete this staff member?",
                            function (note) {
                                controller.message = "Deleting ...";
                                dataService.deleteStaffMember(org, staff).then(controller.updateStaff, errorHandler);
                            });
            } // deleteStaffMember

            controller.generateUserName = function (obj) {
                if (obj.userName)
                    return;

                obj.userName = obj.name.toLowerCase().replace(/ /g, '');
            } // generateUserName




            function pd() {
                return restrictToProject ? controller.currentProject : null;
            }

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

        };

    var projControl = {};
    projControl.setup = _setupController;
    return projControl;

}());