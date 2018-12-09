'use strict'
app.controller('supervisorController',
       ['$routeParams', '$modal', 'dataService', 'staffService', 'localStorageService', 'Upload', '$scope',
         function ($routeParams, $modal, dataService, staffService, localStorageService, Upload, $scope) {
  var controller = this;
  controller.view = "";
  controller.message = "Loading ...";

  clientController.setup(controller, dataService, $modal, localStorageService, Upload, $scope);

  orgController.setup(controller, dataService, $modal, $routeParams);

  reportsController.setup(controller, dataService);

  var _showOptions = function () {
      controller.view = "options";
      if (controller.createNewClient)
          controller.createNewClient();
      controller.buttonView = "";
  } // _showOptions

  var _showOrgDetails = function () {
      controller.view = "admin";
      controller.loadAndViewOrg(controller.organisation);
  } // showOrgDetails

  var _showReports = function () {
      controller.view = "reports";
      controller.loadAndViewReports(controller.organisation);
  } // showReport

  var _showRisksEditor = function () {
      window.location = '#/supervisor/risks';
  } // showRisksEditor

  var _loadOrgDetails = function (org) {
      dataService.fetchOrgDetails(org).then(function (results) {
          controller.organisation = results.data;
          controller.orgs = [results.data];
          _showOrgDetails();
      }, controller.errorHandler);
  } // _loadOrgDetails 

  var _detailsLoaded = function (staffDetails) {
      controller.message = "";
      controller.name = staffDetails.name;
      controller.allProjects = staffDetails.projects;
      _loadOrgDetails(staffDetails.organisation);
  } // detailsLoaded

  staffService.load("supervisor").then(_detailsLoaded, controller.errorHandler);

  controller.showOptions = _showOptions;
  controller.showOrgDetails = _showOrgDetails;
  controller.showReports = _showReports;
  controller.showRisksEditor = _showRisksEditor;

}]);

