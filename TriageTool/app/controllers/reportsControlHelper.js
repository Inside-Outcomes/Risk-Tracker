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
                if (!controller.report.projects) {
                    controller.currentproject = null;
                }
                if (controller.report.allfields && !controller.currentfield) {
                    return;
                }

                if (controller.currentproject || !controller.report.projects) {
                    if (controller.report)
                        controller.message = "Loading ...";
                        dataService.loadReport(controller.organisation,
                                               controller.currentproject,
                                               null,
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

            controller.loadAndViewReports = function (org) {
                controller.organisation = org;
                controller.startdate = null;
                controller.startdateopened = false;
                controller.enddate = null;
                controller.enddateopened = false;
                controller.currentproject = null;
                controller.currentprojects = [];
                controller.currentriskmaps = [];
                controller.viewReportsList();
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
                { name: 'opendata', title: "Open Data Report", dates: true, projects: true },
                { name: 'agencyreview', title: "Referral Agency Review", dates:false, projects: false  }
            ];

        };

    var reportsControl = {};
    reportsControl.setup = _setupReportController;
    return reportsControl;
}());