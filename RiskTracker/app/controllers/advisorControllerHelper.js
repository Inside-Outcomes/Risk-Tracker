'use strict';
var advisorControllerHelper = (function () {
    var _setupController =
        function (controller,
                  dataService,
                  staffService,
                  role) {
            controller.selectLocation = function (location) {
                controller.currentLocation = location;
                controller.currentProject = null;

                controller.availableProjects = [];
                controller.allProjects.forEach(function (proj) {
                    var add = false;
                    controller.currentLocation.projectIds.forEach(function (locProjId) {
                        if (locProjId == proj.id)
                            add = true;
                    });
                    if (add)
                        controller.availableProjects.push(proj);
                });

                if (controller.availableProjects.length == 1)
                    controller.selectProject(controller.availableProjects[0]);
                else
                    controller.view = 'chooseproject';
            } // selectLocation

            controller.selectAllLocations = function () {
                controller.currentLocation = null;
                controller.currentProject = null;

                controller.availableProjects = [];
                controller.allProjects.forEach(function (proj) {
                    controller.availableProjects.push(proj);
                });

                if (controller.availableProjects.length == 1)
                    controller.selectProject(controller.availableProjects[0]);
                else
                    controller.view = 'chooseproject';
            } // selectAllLocations

            controller.selectProject = function (project) {
                controller.currentProject = project;
                if (controller.riskMap.name != project.riskFramework)
                    dataService.fetchRiskMap(project.riskFramework).then(function (results) {
                        controller.riskMap = results.data;
                    }, controller.errorHandler);
                controller.clientList = [];
                controller.refreshClientList();
                controller.message = "Loading ...";                
            } // selectProject

            controller.refreshClientList = function () {
                var locId = !!controller.currentLocation ? controller.currentLocation.id : null;
                var projId = controller.currentProject.id;
                dataService.listLocProjClients(locId, projId, controller.showDischarged).then(function (results) {
                    controller.clientList = results.data;
                    controller.message = "";
                }, controller.errorHandler);
                controller.showClientList();
            } // _refreshClientList

            controller.showClientList = function () {
                controller.view = "clientlist";
            } // showClientList

            var _detailsLoaded = function (staffDetails) {
                controller.message = "";
                controller.name = staffDetails.name;
                controller.organisation = staffDetails.organisation;
                controller.allProjects = staffDetails.projects;
                controller.allLocations = staffDetails.locations;

                if (controller.allLocations.length == 1)
                    controller.selectLocation(controller.allLocations[0]);
                else
                    controller.view = 'chooselocation';
            } // detailsLoaded

            staffService.load(role).then(_detailsLoaded, controller.errorHandler);
        };

    var advisorControl = {};
    advisorControl.setup = _setupController;
    return advisorControl;
}());