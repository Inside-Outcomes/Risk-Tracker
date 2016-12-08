'use strict'
app.controller('supervisorController',
       ['$routeParams', '$modal', 'dataService', 'staffService', 'localStorageService', 'Upload', '$scope',
         function ($routeParams, $modal, dataService, staffService, localStorageService, Upload, $scope) {
  var advisor = this;
  advisor.view = "";
  advisor.message = "Loading ...";

  clientController.setup(advisor, dataService, $modal, localStorageService, Upload, $scope);

  orgController.setup(advisor, dataService, $modal);

  reportsController.setup(advisor, dataService);

  var _loadClientList = function () {
      advisor.message = "Loading ...";
      advisor.clientList = [];
      advisor.refreshClientList();
      advisor.showClientList();
  } // _loadClientList

  var _refreshClientList = function () {
      dataService.listClients(advisor.organisation, advisor.showDischarged).then(function (results) {
          advisor.clientList = results.data;
          advisor.message = "";
      }, advisor.errorHandler);
  } // _refreshClientList

  var _showClientList = function () {
      advisor.view = "clientlist";
      advisor.loadOrg(advisor.organisation);
  } // showClientList

  var _showOrgDetails = function () {
      advisor.view = "admin";
      advisor.loadAndViewOrg(advisor.organisation);
  } // showOrgDetails

  var _showReports = function () {
      advisor.view = "reports";
      advisor.loadAndViewReports(advisor.organisation);
  } // showReports

  var _loadOrgDetails = function (org) {
      dataService.fetchOrgDetails(org).then(function (results) {
          advisor.organisation = results.data;
          advisor.orgs = [results.data];
          if ($routeParams.subview == 'client' && $routeParams.guid != null) {
              advisor.openClient($routeParams.guid);
              advisor.clientList = [];
              advisor.refreshClientList();
          }
          else
              _loadClientList();
      }, advisor.errorHandler);
  } // _loadOrgDetails 

  var _detailsLoaded = function (staffDetails) {
      advisor.message = "";
      advisor.name = staffDetails.name;
      advisor.allProjects = staffDetails.projects;
      advisor.allLocations = staffDetails.locations;
      _loadOrgDetails(staffDetails.organisation);
  } // detailsLoaded

  staffService.load("supervisor").then(_detailsLoaded, advisor.errorHandler);

  advisor.showClientList = _showClientList;
  advisor.refreshClientList = _refreshClientList;
  advisor.showOrgDetails = _showOrgDetails;
  advisor.showReports = _showReports;

}]);

