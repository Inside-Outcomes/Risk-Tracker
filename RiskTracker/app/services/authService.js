'use strict';
app.factory('authService', ['$http', '$q', 'localStorageService', 'ngAuthSettings', function ($http, $q, localStorageService, ngAuthSettings) {
    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    var authServiceFactory = {};

    var _authentication = {
        isAuth: false,
        userName: "",
        userRole: "",
        isAdmin: function () {
            return this.userRole.indexOf("admin") != -1;
        },
        isAdvisor: function () {
            return this.userRole.indexOf("advisor") != -1;
        },
        isCoordinator: function() {
            return this.userRole.indexOf("coordinator") != -1;
        },
        isSupervisor: function() {
            return this.userRole.indexOf("supervisor") != -1;
        },
        multiRole : function() {
            return this.userRole.indexOf(',') != -1;
        }
    };

    var _saveRegistration = function (registration) {
        _logOut();

        return $http.post(serviceBase + 'api/account/register', registration).then(function (response) {
            return response;
        });

    };

    var _login = function (loginData) {
        var data = "grant_type=password" +
            "&username=" + loginData.userName +
            "&password=" + loginData.password +
            "&client_id=" + ngAuthSettings.clientId;

        var deferred = $q.defer();

        $http.post(serviceBase + 'token',
                   data,
                   { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }
             )
            .success(function (response) {
                localStorageService.set('authorizationData',
                    {
                        token: response.access_token,
                        userName: response.userName,
                        userRole: response.userRole,
                        refreshToken: response.refresh_token
                    });
                _authentication.isAuth = true;
                _authentication.userName = response.userName;
                _authentication.userRole = response.userRole;

                deferred.resolve(response);
            })
            .error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });

        return deferred.promise;
    };

    var _logOut = function () {
        localStorageService.remove('authorizationData');

        _authentication.isAuth = false;
        _authentication.userName = "";
        _authentication.userRole = "";
    };

    var _fillAuthData = function () {
        var authData = localStorageService.get('authorizationData');
        if (authData) {
            _authentication.isAuth = true;
            _authentication.userName = authData.userName;
            _authentication.userRole = authData.userRole;
        }
    };

    var _refreshToken = function () {
        var deferred = $q.defer();

        var authData = localStorageService.get('authorizationData');

        if (authData) {
            var data = "grant_type=refresh_token&refresh_token=" + authData.refreshToken + "&client_id=" + ngAuthSettings.clientId;
            localStorageService.remove('authorizationData');

            $http.post(serviceBase + 'token', data, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } }).success(function (response) {
                localStorageService.set('authorizationData', { token: response.access_token, userName: response.userName, refreshToken: response.refresh_token });

                deferred.resolve(response);
            }).error(function (err, status) {
                _logOut();
                deferred.reject(err);
            });
        }

        return deferred.promise;
    };

    authServiceFactory.saveRegistration = _saveRegistration;
    authServiceFactory.login = _login;
    authServiceFactory.logOut = _logOut;
    authServiceFactory.fillAuthData = _fillAuthData;
    authServiceFactory.authentication = _authentication;
    authServiceFactory.refreshToken = _refreshToken;

    return authServiceFactory;
}]);