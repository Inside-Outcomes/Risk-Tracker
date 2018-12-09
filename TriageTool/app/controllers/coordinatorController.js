'use strict';
app.controller('coordinatorController',
       ['$routeParams', '$modal', 'dataService', 'authService', 'staffService', 'localStorageService', 'Upload', '$scope',
         function ($routeParams, $modal, dataService, authService, staffService, localStorageService, Upload, $scope) {
  var controller = this;
  controller.view = "";
  controller.message = "";

  clientController.setup(controller, dataService, $modal, localStorageService, Upload, $scope);

  reportsController.setup(controller, dataService);

  advisorControllerHelper.setup(controller, dataService, staffService, 'coordinator');

  var _showOptions = function () {
      controller.view = "options";
      controller.buttonView = "";
  } // _showOptions

  var _showReports = function () {
      controller.view = "reports";
      controller.loadAndViewReports(controller.organisation);
  } // showReports

  controller.showOptions = _showOptions;
  controller.showReports = _showReports;

}]);

