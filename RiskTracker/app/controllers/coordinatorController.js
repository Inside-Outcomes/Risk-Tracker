'use strict';
app.controller('coordinatorController',
       ['$routeParams', '$modal', 'dataService', 'staffService', 'authService', 'localStorageService', 'Upload', '$scope',
function ($routeParams, $modal, dataService, staffService, authService, localStorageService, Upload, $scope) {
    var controller = this;
    controller.view = "";
    controller.message = "Loading ...";
    controller.roles = ['advisor', 'coordinator'];

    clientController.setup(controller, dataService, $modal, localStorageService, Upload, $scope);

    reportsController.setup(controller, dataService);

    advisorControllerHelper.setup(controller, dataService, staffService, 'coordinator');

    projectControllerHelper.setup(controller, dataService, $modal, true);

    var _showProject = function () {
        controller.view = "viewProject";
        controller.buttonView = '';
        controller.project = controller.currentProject;

        controller.allStaff = [];
        dataService.listStaff(controller.organisation, controller.project).then(
            function (results) {
                controller.allStaff = results.data;
            }
        );
    } // showProject

    var _showReports = function () {
        controller.view = "reports";
        controller.loadReportsForProject(controller.organisation);
    } // showReports

    controller.updateLocationData = function (results) {
        controller.viewLocation = true;
        controller.message = "";
        controller.showProject();
        controller.allLocations = results.data;
    } // updateLocationData

    controller.updateStaff = function (results) {
        controller.viewStaff = true;
        controller.message = "";
        controller.showProject();
        controller.allStaff = results.data;
    } // updateLocationData

    controller.showReports = _showReports;
    controller.showProject = _showProject;
}]);

