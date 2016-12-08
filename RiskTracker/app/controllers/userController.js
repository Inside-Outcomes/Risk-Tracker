'use strict';
app.controller('userController',
    ['dataService', 'authService', '$timeout',
        function (dataService, authService, $timeout) {
            var controller = this;
            controller.isAdmin = authService.authentication.isAdmin();
            controller.message = "";

            controller.refresh = function () {
                if (controller.isAdmin)
                    return;

                controller.message = "Loading ...";
                dataService.loadUser().then(function (results) {
                    controller.message = "";
                    controller.user = results.data;
                });
            }

            controller.updateDetails = function () {
                controller.message = "Saving ...";
                dataService.updateUser(controller.user).then(function (results) {
                    controller.message = "";
                    controller.user = results.data;
                });
            }

            controller.updatePassword = function () {
                controller.message = "Saving ...";
                dataService.updateUserPassword(controller.user.password).then(function () {
                    controller.user.password = "";
                    controller.user.confirmPassword = "";
                    controller.message = "Your password has been updated";
                    $timeout(function () { controller.message = ""; }, 3000);
                });
            }

            controller.refresh();
        }]);