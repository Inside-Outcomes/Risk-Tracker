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

    index.showAdvisor = authService.authentication.isAdvisor() && authService.authentication.multiRole();
    index.showCoordinator = authService.authentication.isCoordinator() && authService.authentication.multiRole();
    index.showSupervisor = authService.authentication.isSupervisor() && authService.authentication.multiRole();
    index.showDivider = authService.authentication.multiRole();

    index.isAuth = authService.authentication.isAuth;
    index.userName = authService.authentication.userName;
    index.isCollapsed = true
}]);