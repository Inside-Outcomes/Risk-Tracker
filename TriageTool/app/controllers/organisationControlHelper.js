'use strict';
var orgController = (function () {
    var _setupOrgController =
        function (controller,
                  dataService,
                  $modal) {
            controller.roles = ['advisor', 'coordinator', 'supervisor'];
            controller.message = "";
            controller.view = 'home';
            controller.buttonView = 'home';
            controller.organisation = null;

            controller.cancel = function () {
                controller.message = "";
                controller.view = 'home';
                controller.buttonView = 'home';
            } // cancel

            projectControllerHelper.setup(controller, dataService, $modal);

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

            var errorHandler = function (error) {
                if (error.data && error.data.message)
                    controller.message = error.data.message;
                else
                    controller.message = error.status + ": " + error.statusText;
            } // errorHandler

            controller.updateLocationData = function (results) {
                controller.viewLocation = true;
                controller.viewOrg();
                controller.allLocations = results.data;
            } // updateLocationData

            controller.updateStaff = function (results) {
                controller.viewStaff = true;
                controller.viewOrg();
                controller.allStaff = results.data;
            } // updateStaff

            controller.viewOrg = function () {
                controller.message = "";
                controller.view = 'viewOrg';
                controller.buttonView = 'editOrg';
            } // viewOrg

            controller.loadOrg = function (org) {
                controller.organisation = org;
                controller.allProjects = [];
                controller.allStaff = [];
                controller.allLocations = [];
                controller.allRiskmaps = [];
                dataService.listProjects(org).then(
                    function (results) {
                        controller.allProjects = results.data;
                    }
                );
                dataService.listStaff(org).then(
                    function (results) {
                        controller.allStaff = results.data;
                    }
                );
                dataService.listLocations(org).then(
                    function (results) {
                        controller.allLocations = results.data;
                    }
                );
                dataService.listRiskMaps(org).then(
                    function (results) {
                        controller.allRiskmaps = results.data;
                    }
                );
            } // loadOrg

            controller.loadAndViewOrg = function (org) {
                controller.loadOrg(org);
                controller.viewOrg();
            } // loadAndViewOrg

            controller.editOrg = function (org) {
                controller.editorg = clone(org);
                controller.view = 'editOrg';
                controller.buttonView = 'cancelToView';
            } // editOrg

            controller.updateOrg = function (org) {
                dataService.updateOrganisation(org).then(
                    function (results) { 
                        controller.organisation = results.data;
                        controller.viewOrg();
                    },
                    controller.errorHandler
                );
            } // updateOrg

            controller.addProjectForm = function (org) {
                controller.organisation = org;
                controller.project = { address: clone(org.address) }
                if (controller.allRiskmaps.length == 1)
                    controller.project.riskFramework = controller.allRiskmaps[0].name;
                controller.view = 'newProject';
                controller.buttonView = 'cancelToView';
            } // addProjectForm
            
            controller.editProjectForm = function (proj) {
                controller.project = clone(proj);
                dataService.canDeleteProject(controller.organisation, controller.project).then(
                    function (results) {
                        controller.project.candelete = results.data;
                    }
                );
                controller.view = 'editProject';
                controller.buttonView = 'cancelToView';
            } // editProjectForm

            controller.toggleStaffProject = function (projId) {
                var index = controller.staff.projectIds.indexOf(projId);
                if (index > -1)
                    controller.staff.projectIds.splice(index, 1);
                else
                    controller.staff.projectIds.push(projId);
            } // toggleStaffProject

            controller.toggleLocationProject = function (projId) {
                var index = controller.location.projectIds.indexOf(projId);
                if (index > -1)
                    controller.location.projectIds.splice(index, 1);
                else
                    controller.location.projectIds.push(projId);
            } // toggleLocationProject

            var updateProjectData = function (results) {
                controller.message = "";
                controller.viewOrg();
                controller.allProjects = results.data;
            } // updateProjectDate

            controller.addNewProject = function (org, proj) {
                controller.message = "Saving ...";
                dataService.addNewProject(org, proj).then(updateProjectData, errorHandler);
            } // addNewProject

            controller.updateProject = function (org, proj) {
                controller.message = "Saving ...";
                dataService.updateProject(org, proj).then(updateProjectData, errorHandler);
            } // updateProject

            controller.deleteProject = function (org, staff) {
                _dialogBox('confirmDialog.html',
                            "Delete Project",
                            "Are you sure you want to delete this project?",
                            function (note) {
                                controller.message = "Deleting ...";
                                dataService.deleteProject(org, staff).then(updateProjectData, errorHandler);
                            });
            } // deleteProject

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

            var _objName = function (objId, objList) {
                if (objList === void 0)
                    return "";

                for (var i = 0; i != objList.length; ++i)
                    if (objList[i].id == objId)
                        return objList[i].name;
            } // _objName

            controller.projectName = function (projId) { return _objName(projId, controller.allProjects); }
            controller.locationName = function (locId) { return _objName(locId, controller.allLocations); };
        };

    var orgControl = {};
    orgControl.setup = _setupOrgController;
    return orgControl;
}());