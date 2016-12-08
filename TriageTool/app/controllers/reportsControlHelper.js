'use strict';
var reportsController = (function () {
    var _setupReportController =
        function (controller,
                  dataService) {
            controller.report = null;
            controller.reportView = null;
 
            controller.viewReportsList = function() {
                controller.report = null;
                controller.reportView = null;
            }

            controller.selectReport = function () {
                controller.availableReports.forEach(function (report) {
                    if (report.name == controller.reportView) 
                        controller.report = report;
                });
                controller.loadReport();
            }

            controller.loadReport = function () {
                controller.currentprojects.forEach(function (project) {
                    if (project.id == controller.currentprojectid)
                        controller.currentproject = project;
                });
                controller.currentlocation = void 0;
                controller.currentlocations.forEach(function (location) {
                    if (location.id == controller.currentlocationid)
                        controller.currentlocation = location;
                });

                if (controller.report.allfields && !controller.currentfield) {
                    return;
                }

                if (controller.currentproject) {
                    if (controller.report)
                        controller.message = "Loading ...";
                        dataService.loadReport(controller.organisation,
                                               controller.currentproject,
                                               controller.report.loc ? controller.currentlocation : null,
                                               controller.report.name,
                                               controller.startdate,
                                               controller.enddate,
                                               controller.report.allfields ? controller.currentfield : null).then(
                            function (results) {
                                controller.message = "";
                                controller.reportData = results.data;
                            }
                        );
                } // if ...
            } // loadReport
            controller.updateReportLocations = function () {
                controller.currentlocations = []
                controller.allLocations.forEach(function (location) {
                    var add = false;
                    location.projectIds.forEach(function (locProjId) {
                        if (locProjId == controller.currentprojectid)
                            add = true;
                    });
                    if (location.id == controller.currentlocationid && (!add))
                        controller.currentlocationid = "";
                    if (add)
                        controller.currentlocations.push(location);
                });
                if (controller.report.allfields) {
                    controller.allfields = [];
                    controller.currentfield = "";
                    var proj = null;
                    controller.currentprojects.forEach(function (project) {
                        if (project.id == controller.currentprojectid)
                            proj = project;
                    });
                    dataService.loadReportFields(controller.organisation, proj).then(
                            function (results) {
                                controller.allfields = results.data;
                            }
                        );
                } // if ...
            } // updateReportLocations

            controller.loadAndViewReports = function (org) {
                controller.organisation = org;
                controller.startdate = null;
                controller.startdateopened = false;
                controller.enddate = null;
                controller.enddateopened = false;
                controller.currentlocation = null;
                controller.currentlocations = [];
                controller.currentproject = null;
                controller.currentprojects = [];
                controller.currentriskmaps = [];
                controller.viewReportsList();
                dataService.listLocations(org).then(
                    function (results) {
                        controller.alllocations = results.data;
                    }
                );
                dataService.listProjects(org).then(
                    function (results) {
                        controller.currentprojects = results.data;
                    }
                );
                dataService.listRiskMaps(org).then(
                    function (results) {
                        controller.currentriskmaps = results.data;
                    }
                );
            } // loadAndViewReports

            controller.loadReportsForProject = function () {
                controller.startdate = null;
                controller.startdateopened = false;
                controller.enddate = null;
                controller.enddateopened = false;
                controller.currentlocation = controller.currentLocation;
                controller.currentlocationid = controller.currentLocation.id;
                controller.currentlocations = [ controller.currentLocation ];
                controller.currentproject = controller.currentProject;
                controller.currentprojectid = controller.currentProject.id;
                controller.currentprojects = [ controller.currentProject ];
                controller.currentriskmaps = [];
                controller.viewReportsList();

                dataService.listRiskMaps(controller.organisation).then(
                    function (results) {
                        controller.currentriskmaps = results.data;
                    }
                );
            } // loadReportsForProject

            controller.opendatepicker = function ($event, which) {
                $event.preventDefault();
                $event.stopPropagation();

                if (which == 'start') {
                    controller.startdateopened = !controller.startdateopened;
                    controller.enddateopened = false;
                }
                if (which == 'end') {
                    controller.enddateopened = !controller.enddateopened;
                    controller.startdateopened = false;
                }
            };

            controller.loadAsCSV = function () {
                window.location.href = controller.reportData.additional.csvurl;
            }

            controller.availableReports = [
                { name: 'actionrequired', title: "Action Required", startdate: true, loc: true },
                { name: 'activityreport', title: "Activity Report", loc: true, dates: true },
                { name: 'adhoc', title: "Ad Hoc Report", loc: true, dates: true, allfields: true },
                { name: 'audit', title: "Audit Report", loc: true, startdate: true },
                { name: 'commissioners', title: "Commissioners Report", loc: true, dates: true },
                { name: 'opendata', title: "Open Data Report", dates: true },
                { name: 'pcprogress', title: "Personal Circumstances Progress Report", loc: true, dates: true },
                { name: 'resolution', title: 'Resolution Report', loc: true, dates: true },
                { name: 'export', title: 'Export Data', loc: true, dates: true }
            ];

        };

    var reportsControl = {};
    reportsControl.setup = _setupReportController;
    return reportsControl;
}());