'use strict';
app.factory('staffService', ['dataService', function (dataService) {
    var service = this;
    service.loaded = false;
    service.staffDetails = {
        name: "",
        organisation: {},
        projects: [],
        locations: []
    };

    var _load = function (type) {
        if (service.loaded == type)
            return {
                then: function (success) {
                    success(service.staffDetails);
                } // then
            }; // fakePromise

        return dataService.loadStaff(type).then(function (results) {
            service.loaded = type;

            service.staffDetails.name = results.data.name;
            service.staffDetails.organisation = results.data.organisation;
            service.staffDetails.projects = results.data.projects;
            service.staffDetails.locations = results.data.locations;

            return service.staffDetails;
        });
    } // _load

    var _logOut = function () {
        service.loaded = false;
    } // _logOut

    var staffServiceFactory = {};
    staffServiceFactory.advisor = service.staffDetails;
    staffServiceFactory.load = _load;
    staffServiceFactory.logOut = _logOut;
    return staffServiceFactory;
}]);