'use strict';
app.controller('advisorController',
       ['$routeParams', '$modal', 'dataService', 'authService', 'staffService', 'localStorageService', 'Upload', '$scope',
         function ($routeParams, $modal, dataService, authService, staffService, localStorageService, Upload, $scope) {
  var advisor = this;
  advisor.view = "";
  advisor.message = "";

  clientController.setup(advisor, dataService, $modal, localStorageService, Upload, $scope);

  advisorControllerHelper.setup(advisor, dataService, staffService, 'advisor');
}]);

