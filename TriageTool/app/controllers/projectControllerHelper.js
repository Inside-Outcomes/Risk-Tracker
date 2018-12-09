'use strict';
var projectControllerHelper = (function () {
    var _setupController =
        function (controller,
                  dataService,
                  $modal,
                  restrictToProject) {

            controller.includeProjectChooser = !(restrictToProject);

            var errorHandler = function (error) {
                if (error.data && error.data.message)
                    controller.message = error.data.message;
                else
                    controller.message = error.status + ": " + error.statusText;
            } // errorHandler

            controller.addStaffForm = function (org) {
                controller.staff = { projectIds: [], roles: [] };
                controller.view = 'newStaff';
                controller.buttonView = 'cancelToView';
            } // addStaffForm

            controller.editStaffForm = function (staff) {
                controller.staff = clone(staff);
                controller.view = 'editStaff';
                controller.buttonView = 'cancelToView';
            } // editStaffForm

            controller.toggleStaffRole = function (role) {
                var index = controller.staff.roles.indexOf(role);
                if (index > -1)
                    controller.staff.roles.splice(index, 1);
                else
                    controller.staff.roles.push(role);
            } // toggleStaffRole

            controller.addNewStaffMember = function (org, staff) {
                dataService.addNewStaffMember(org, staff, pd()).then(controller.updateStaff, errorHandler);
            } // addNewStaffMember

            controller.updateStaffMember = function (org, staff) {
                dataService.updateStaffMember(org, staff, pd()).then(controller.updateStaff, errorHandler);
            } // updateStaffMember

            controller.updateStaffPassword = function (org, staff) {
                controller.message = "Saving ...";
                dataService.updateStaffPassword(org, staff, pd()).then(
                    function (results) { controller.message = ""; },
                    errorHandler
                );
            } // updateStaffPassword

            controller.deleteStaffMember = function (org, staff) {
                _dialogBox('confirmDialog.html',
                            "Delete Staff Member",
                            "Are you sure you want to delete this staff member?",
                            function (note) {
                                controller.message = "Deleting ...";
                                dataService.deleteStaffMember(org, staff).then(controller.updateStaff, errorHandler);
                            });
            } // deleteStaffMember

            controller.generateUserName = function (obj) {
                if (obj.userName)
                    return;

                obj.userName = obj.name.toLowerCase().replace(/ /g, '');
            } // generateUserName

            controller.addReferralAgencyForm = function (org) {
                controller.agency = { associatedRiskIds: [] };
                controller.reviewdateopen = false;
                controller.view = 'newReferralAgency';
                controller.buttonView = 'cancelToView';
            } // addReferralAgencyForm

            controller.editReferralAgencyForm = function (agency) {
                controller.agency = clone(agency);
                controller.reviewdateopen = false;
                controller.view = 'editReferralAgency';
                controller.buttonView = 'cancelToView';
            } // editReferralAgencyForm

            controller.addNewReferralAgency = function (org, agency) {
                dataService.addNewReferralAgency(org, agency, pd()).then(controller.updateAgency, errorHandler);
            } // addNewReferralAgency

            controller.updateReferralAgency = function (org, agency) {
                dataService.updateReferralAgency(org, agency, pd()).then(controller.updateAgency, errorHandler);
            } // updateReferralAgency

            controller.deleteReferralAgency = function (org, staff) {
                _dialogBox('confirmDialog.html',
                            "Delete Referral Agency",
                            "Are you sure you want to delete this agency?",
                            function (note) {
                                controller.message = "Deleting ...";
                                dataService.deleteReferralAgency(org, staff).then(controller.updateAgency, errorHandler);
                            });
            } // deleteReferralAgency

            function pd() {
                return restrictToProject ? controller.currentProject : null;
            }

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

        };

    var projControl = {};
    projControl.setup = _setupController;
    return projControl;

}());