'use strict';
app.controller('homeController',
        ['$location', 'authService',
          function ($location, authService) {

    var home = this;

    home.loginData = {
        userName: "",
        password: "",
        ready: function () {
            return this.userName && this.password;
        }
    }; // loginData

    home.needToLogin = "";
    home.message = "";

    function loggedIn() {
        if (authService.authentication.isAdmin())
            $location.path('/admin');
        else if (authService.authentication.isSupervisor())
            $location.path('/advisor');
        else if (authService.authentication.isCoordinator())
            $location.path('/advisor');
        else if (authService.authentication.isAdvisor())
            $location.path('/advisor');
        else if (authService.authentication.isAuth)
            $location.path('/norole');
        else
            home.needToLogin = "yes";
    } // loggedIn

    home.login = function () {
        if (!home.loginData.ready())
            return;

        authService
            .login(home.loginData)
            .then(function (response) {
                loggedIn();
            },
            function (err) {
                home.message = err.error_description;
            });
    }; // login

    if (authService.authentication.isAuth)
        loggedIn();
    else
        home.needToLogin = "yes";
}]);