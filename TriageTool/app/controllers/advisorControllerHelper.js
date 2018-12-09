'use strict';
var advisorControllerHelper = (function () {
    var _setupController =
        function (controller,
                  dataService,
                  staffService,
                  role) {

            var _detailsLoaded = function (staffDetails) {
                controller.message = "";
                controller.name = staffDetails.name;
                controller.organisation = staffDetails.organisation;
                controller.allProjects = staffDetails.projects;

                if (controller.createNewClient)
                    controller.createNewClient();
            } // detailsLoaded

            staffService.load(role).then(_detailsLoaded, controller.errorHandler);
        };

    var advisorControl = {};
    advisorControl.setup = _setupController;
    return advisorControl;
}());