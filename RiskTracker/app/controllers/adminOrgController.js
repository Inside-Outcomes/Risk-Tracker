'use strict';
app.controller('adminOrgController',
    ['$routeParams', '$modal', 'authService', 'dataService', 'localStorageService', 'Upload', '$scope',
function ($routeParams, $modal, authService, dataService, localStorageService, Upload, $scope) {

    var controller = this;

    controller.isAdmin = authService.authentication.isAdmin();

    clientController.setup(controller, dataService, $modal, localStorageService, Upload, $scope);

    orgController.setup(controller, dataService, $modal, $routeParams);

    reportsController.setup(controller, dataService);

    controller.orgs = [];
    controller.riskMaps = [];

    controller.newOrgForm = function () {
        controller.neworg = { address: {}, riskMaps: [] }
        controller.message = "";
        controller.view = 'newOrg';
    } // newOrgForm

    controller.createNewOrg = function (org) {
        dataService.createOrganisation(org).then(
            controller.refresh,
            controller.errorHandler
        );
    } // createNewOrg

    controller.canDeleteOrg = function () {
        if (controller.organisation['candelete'] === undefined) {
            controller.organisation.candelete = false;
            dataService.canDeleteOrganisation(controller.organisation).then(
                function (results) {
                    controller.organisation.candelete = results.data;
                }
            );
        }
        return true;
    }

    controller.deleteOrg = function () {
        dataService.deleteOrganisation(controller.organisation).then(
            controller.refresh,
            controller.errorHandler
        );
    }

    controller.suspendOrg = function () {
        dataService.suspendOrganisation(controller.organisation).then(
            function (results) {
                controller.organisation = results.data;
                controller.viewOrg();
            },
            controller.errorHandler
        );
    }
    controller.activateOrg = function () {
        dataService.activateOrganisation(controller.organisation).then(
            function (results) {
                controller.organisation = results.data;
                controller.viewOrg();
            },
            controller.errorHandler
        );
    }

    controller.closeOrg = function () {
        controller.organisation = null;
        controller.cancel();
    }

    controller.refresh = function () {
        controller.view = 'home';
        controller.organisation = null;
        controller.cancel();
        controller.message = "Loading ...";
        dataService.listOrganisations().then(function (results) {
            controller.message = "";
            controller.orgs = results.data;
        }, function (error) {
            //alert(error.data.message);
        });
        dataService.listRiskMaps().then(function (results) {
            controller.riskMaps = results.data;
        });
    } // refresh

    controller.showClientList = function (org) {
        controller.view = "clientlist";
    } // showClientList

    controller.showReports = function () {
        controller.view = "reports";
        controller.loadAndViewReports(controller.organisation);
    } // showReports

    controller.exportEverything = function () {
        controller.view = "exportolump";
        controller.exportUrl = null;
        controller.exportMessage = "Please wait ...";
        controller.exportolump(controller.organisation);
    }

    controller.exportolump = function (organisation) {
        controller.view = "exportolump";
        dataService.exportolump(organisation).then(function (results) {
            if (results.data.msg) 
                controller.exportMessage = results.data.msg;
            if (results.data.url)
                controller.exportUrl = results.data.url;
            if (!results.data.complete)
                setTimeout(function () { controller.exportolump(organisation) }, 2000);
        })
    }

    controller.loadClientList = function () {
        controller.message = "Loading ...";
        controller.clientList = [];
        controller.refreshClientList();
        controller.showClientList();
    } // _loadClientList

    controller.refreshClientList = function () {
        dataService.listClients(controller.organisation, controller.showDischarged).then(function (results) {
            controller.clientList = results.data;
            controller.message = "";
        }, controller.errorHandler);
    } // _refreshClientList

    controller.toggleOrganisationRiskMap = function (org, riskMapIds) {
        var index = org.riskMaps.indexOf(riskMapIds);
        if (index > -1)
            org.riskMaps.splice(index, 1);
        else
            org.riskMaps.push(riskMapIds);
    } // toggleStaffProject    

    controller.refresh();
    if ($routeParams.subview == 'client' && $routeParams.guid != null) {
        controller.openClient($routeParams.guid);
    }
}]);