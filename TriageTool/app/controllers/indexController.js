'use strict';
app.controller('indexController',
      [ '$window', '$location', 'authService', 'staffService',
        function ($window, $location, authService, staffService) {
    var index = this;

    index.logOut = function () {
        authService.logOut();
        staffService.logOut();
        $window.location.href = 'index.html';
    } // logOut

    index.openAdvisor = function () {
        $location.path('/advisor');
    }
    index.openCoordinator = function () {
        $location.path('/coordinator');
    }
    index.openSupervisor = function () {
        $location.path('/supervisor');
    }
    index.openProfile = function () {
        $location.path('/profile');
    }

    index.showAdvisor = false; //authService.authentication.isAdvisor() && authService.authentication.multiRole();
    index.showCoordinator = false; //authService.authentication.isCoordinator() && authService.authentication.multiRole();
    index.showSupervisor = false; //authService.authentication.isSupervisor() && authService.authentication.multiRole();
    index.showDivider = false; //authService.authentication.multiRole();

    index.isAuth = authService.authentication.isAuth;
    index.userName = authService.authentication.userName;
    index.isCollapsed = true
}]);