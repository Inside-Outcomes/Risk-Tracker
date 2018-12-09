
var app = angular.module('AngularAuthApp',
    [
        'ngRoute',
        'datatables',
        'LocalStorageModule',
        'angular-loading-bar',
        'ui.bootstrap',
        'ngFileUpload'
    ]);

app.config(function ($routeProvider) {
    $routeProvider.when("/home", {
        templateUrl: "/app/views/home.html"
    });

    $routeProvider.when("/admin", {
        templateUrl: "/app/views/adminOrgHome.html"
    });
    $routeProvider.when("/admin/risks", {
        templateUrl: "/app/views/adminRisksHome.html"
    });
    $routeProvider.when("/admin/:subview/:guid", {
        templateUrl: "/app/views/adminOrgHome.html"
    });

    $routeProvider.when("/advisor", {
        templateUrl: "/app/views/advisor.html"
    });
    $routeProvider.when("/advisor/:subview", {
        templateUrl: "/app/views/advisor.html"
    });

    $routeProvider.when("/coordinator", {
        templateUrl: "/app/views/coordinator.html"
    });
    $routeProvider.when("/coordinator/:subview", {
        templateUrl: "/app/views/coordinator.html"
    });

    $routeProvider.when("/supervisor", {
        templateUrl: "/app/views/supervisor.html"
    });
    $routeProvider.when("/supervisor/:subview", {
        templateUrl: "/app/views/supervisor.html"
    });
    $routeProvider.when("/supervisor/:subview/:guid", {
        templateUrl: "/app/views/supervisor.html"
    });

    $routeProvider.when("/profile", {
        templateUrl: "/app/views/profile.html"
    });

    $routeProvider.when("/norole", {
        templateUrl: "/app/views/norole.html"
    })
    
    $routeProvider.otherwise({ redirectTo: "/home" });
});

//var serviceBase = 'http://localhost:50128/';
//var serviceBase = 'http://ngauthenticationapi.azurewebsites.net/';
var serviceBase = './';
app.constant('ngAuthSettings', {
    apiServiceBaseUri: serviceBase,
    clientId: 'ngAuthApp'
});

app.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptorService');
});

app.run(['authService', function (authService) {
    authService.fillAuthData();
}]);

var cacheBustBaseUrl = 'app/views';
var cacheBustParam = 'v=' + ~~(Date.now() / 60000);

function getTimeVersionedUrl(url) {
    // only do for html templates of this app
    // NOTE: the path to test for is app dependent!
    if (!url || url.indexOf(cacheBustBaseUrl) < 0 || url.indexOf('.html') < 0) return url;
    // create a URL param that changes every minute
    // and add it intelligently to the template's previous url
    if (url.indexOf('?') > 0) {
        if (url.indexOf('v=') > 0)
            return url.replace(/v=([0-9]*)/, cacheBustParam);
        return url + '&' + cacheBustParam;
    }
    return url + '?' + cacheBustParam;
}

// this defeats Angular's $templateCache on a 1-minute interval
// as a side-effect it also defeats HTTP (browser) caching
app.config(function ($httpProvider) {
    $httpProvider.interceptors.push(function () {
        return {
            'request': function (config) {
                config.url = getTimeVersionedUrl(config.url);
                return config;
            }
        };
    });
});


app.filter('nl2br', ['$sce', function ($sce) {
    return function (msg, is_xhtml) {
        var is_xhtml = is_xhtml || false;
        var breakTag = is_xhtml ? '<br />' : '<br>';
        var msg = (msg + '').replace(/([^>\r\n]?)(\r\n|\n\r|\n|\r)/g, '$1' + breakTag +'$2');
        return $sce.trustAsHtml(msg);
    }
}]);

app.directive('addressForm', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/views/common/addressform.html',
        scope: {
            address: '=',
            nowebsite: '@',
            notitle: '@'
        }
    };
});

app.directive('addressDisplay', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/views/common/address.html',
        scope: {
            address: '=',
            brief: '@'
        }
    };
});

app.directive('clientList', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/views/common/clientlist.html',
        scope: {
            clients: '=',
            controller: '=',
            brief: '@'
        }
    }
});

app.directive('demographicsDisplay', function() {
    return {
        retrict: 'E',
        templateUrl: 'app/views/common/demographics.html',
        scope: {
            demographics: '='
        }
    };
});
app.directive('missingDemographics', function () {
    return {
        retrict: 'E',
        templateUrl: 'app/views/common/missingDemographics.html',
        scope: {
            demographics: '='
        }
    };
});
app.directive('demographicsForm', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/views/common/demographicsForm.html',
        scope: {
            demographics: '=',
            notitle: '@',
            modal: '='
        }
    };
})
app.directive('questionsDisplay', function () {
    return {
        retrict: 'E',
        templateUrl: 'app/views/common/questions.html',
        scope: {
            questions: '='
        }
    };
})

app.directive('questionsForm', function () {
    return {
        restrict: 'E',
        templateUrl: 'app/views/common/questionsForm.html',
        scope: {
            questions: '='
        }
    };
});

app.directive('ngBindAttrs', function () {
    return function (scope, element, attrs) {
        scope.$watch(attrs.ngBindAttrs, function(value) {
            angular.forEach(value, function(value, key) {
                attrs.$set(key, value);
            })
        }, true);
    };
});

app.controller('editDialogController',
        ['$scope', '$modalInstance', 'title', 'thingToEdit',
function ($scope, $modalInstance, title, thingToEdit) {
    $scope.title = title;
    $scope.thingToEdit = thingToEdit;
    $scope.opened = false;
    $scope.modal = $scope;

    $scope.update = function () {
        $modalInstance.close($scope.thingToEdit);
    }

    $scope.cancel = function () {
        $modalInstance.dismiss('cancel');
    }

    $scope.open = function ($event) {
        $event.preventDefault();
        $event.stopPropagation();

        $scope.opened = true;
    };
}]);
