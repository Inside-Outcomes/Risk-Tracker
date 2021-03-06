﻿'use strict';
app.factory('dataService', ['$http', 'ngAuthSettings', function ($http, ngAuthSettings) {
    var serviceBase = ngAuthSettings.apiServiceBaseUri;

    var uriAction = function (action, params) {
        var uri = serviceBase + 'api/' + action;
        if (params)
          for (var i = 0; i != params.length; ++i)
            uri += '/' + params[i];
        return uri;
    } // uriAction

    var clientsUri = function (orgId, locId, projId) {
        var uri = serviceBase + 'api/Clients';
        if (orgId)
            uri += "/Organisation/" + orgId;
        if (locId)
            uri += "/Location/" + locId;
        if (projId)
            uri += "/Project/" + projId;
        return uri;
    } // clientsUri
    var clientAction = function () {
        return uriAction('Client', arguments);
    } // client

    var orgsUri = serviceBase + 'api/Organisations';
    var orgAction = function () {
        return uriAction('Organisation', arguments);
    } // client
    
    var riskMapsUri = serviceBase + 'api/RiskMaps';
    var riskMapAction = function () {
        return uriAction('RiskMap', arguments);
    } // riskMapAction

    var risksUri = serviceBase + 'api/Risks';
    var riskAction = function () {
        return uriAction('Risk', arguments);
    } // riskAction

    var reportAction = function () {
        return uriAction('Report', arguments);
    } // reportAction

    var refIdAction = function () {
        return uriAction('ReferenceId', arguments);
    } // refIdAction

    var advisorUri = serviceBase + 'api/Advisor';
    var coordinatorUri = serviceBase + 'api/Coordinator';
    var supervisorUri = serviceBase + 'api/Supervisor';
    var userUri = serviceBase + 'api/User';

    function _get(uri) {
        return $http.get(uri);
    } // _get
    function _post(uri, data) {
        return $http.post(uri, data);
    } // _post
    function _put(uri, data) {
        return $http.put(uri, data);
    } // _put
    function _delete(uri) {
        return $http.delete(uri);
    }

    //////////////////////////////////////////////
    var _loadStaff = function (type) {
        if (type == "supervisor")
            return _loadSupervisor();
        if (type == "coordinator")
            return _loadCoordinator();
        if (type == "advisor")
            return _loadAdvisor();
    }
    var _loadAdvisor = function () {
        return $http.get(advisorUri);
    } // _loadAdvisor
    var _loadCoordinator = function() {
        return $http.get(coordinatorUri);
    } // _loadCoordinator
    var _loadSupervisor = function () {
        return $http.get(supervisorUri);
    } // _loadAdvisor
    var _loadUser = function () {
        return $http.get(userUri);
    } // _loadUser
    var _updateUser = function (user) {
        return $http.put(userUri, user);
    }
    var _updateUserPassword = function (pwd) {
        return $http.put(userUri + "pwd", { password: pwd });
    }

    //////////////////////////////////////////////
    var _listClients = function(orgData, showDischarged) {
        return _get(clientsUri(orgData.id) + "?discharged=" + showDischarged);
    } // _listClients
    var _listLocProjClients = function (locId, projId, showDischarged) {
        return _get(clientsUri(null, locId, projId) + "?discharged=" + showDischarged);
    } // _listLocProjClients

    var _fetchClient = function (guid) {
        return _get(clientAction(guid));
    } // _fetchClient

    var _createClient = function (client) {
        return _post(clientsUri(), client);
    } // _createClient
    var _clientAddNote = function (guid, noteText, noteType) {
        noteType = noteType || 'AddNote'
        var newNote = { text: noteText }
        return _put(clientAction(guid, noteType), newNote);
    } // _addNote
    var _clientUpdate = function (guid, client) {
        return _put(clientAction(guid, 'Update'), client);
    } // _clientUpdate
    var _clientAddRisk = function (guid, riskId) {
        return _put(clientAction(guid, 'AddRisk'), { id: riskId });
    } // _clientAddRisk
    var _clientManageRisk = function (guid, riskId) {
        return _put(clientAction(guid, 'ManageRisk'), { id: riskId });
    } // _clientManageRisk
    var _clientRemoveRisk = function (guid, riskId) {
        return _put(clientAction(guid, 'RemoveRisk'), { id: riskId });
    } // _clientRemoveRisk
    var _clientReopenRisk = function (guid, riskId) {
        return _put(clientAction(guid, 'ReopenRisk'), { id: riskId });
    } // _clientReopenRisk
    var _clientDischarge = function (guid, noteText) {
        return _put(clientAction(guid, 'Discharge'), { text: noteText });
    } // _clientDischarge
    var _clientDelete = function (guid) {
        return _delete(clientAction(guid));
    } // _clientDelete
    var _clientQuestions = function (guid, questions) {
        return _put(clientAction(guid, 'Questions'), questions);
    } // _clientQuestions
    var _clientReopen = function (guid, noteText) {
        return _put(clientAction(guid, 'Reopen'), { text: noteText });
    } // _clientReopen
    var _clientReferralReport = function (guid) {
        return _post(clientAction(guid, 'ReferralReport'));
    } // _clientReferralReport
    var _emailReferralReport = function (guid) {
        return _post(clientAction(guid, 'SendReferralReport'));
    } // _emailReferralReport
    var _closeReferralReport = function (guid) {
        return _post(clientAction(guid, 'CloseReferralReport'));
    } // _closeReferralReport

    /////////////////////////////////////////////////
    var _listOrganisations = function () {
        return _get(orgsUri);
    } // _listOrganisations

    var _createOrganisation = function (orgData) {
        return _post(orgsUri, orgData);
    } // _createOrganisation
    var _updateOrganisation = function (orgData) {
        return _put(orgAction(orgData.id, 'Update'), orgData);
    } // _updateOrganisation
    var _canDeleteOrganisation = function (orgData) {
        return _get(orgAction(orgData.id, 'CanDelete'));
    } // _canDeleteOrganisation
    var _deleteOrganisation = function (orgData) {
        return _delete(orgAction(orgData.id));
    } // _deleteOrganisation
    var _suspendOrganisation = function (orgData) {
        return _put(orgAction(orgData.id, 'Suspend'));
    } // _suspendOrganisation
    var _activateOrganisation = function (orgData) {
        return _put(orgAction(orgData.id, 'Activate'));
    } // _activateOrganisation

    var _listProjects = function (orgData) {
        return _get(orgAction(orgData.id, 'Projects'));
    } // _listProjects
    var _addNewProject = function (orgData, projData) {
        return _post(orgAction(orgData.id, 'AddProject'), projData);
    } // _addNewProject
    var _updateProject = function (orgData, projData) {
        return _put(orgAction(orgData.id, 'Project', projData.id, 'Update'), projData);
    } // _updateProject
    var _canDeleteProject = function (orgData, projData) {
        return _get(orgAction(orgData.id, 'Project', projData.id, 'CanDelete'));
    } // _canDeleteProject
    var _deleteProject = function (orgData, projData) {
        return _delete(orgAction(orgData.id, 'Project', projData.id));
    } // _deleteProject
    var _fetchOrgDetails = function (orgData) {
        return _get(orgAction(orgData.id));
    } // _loadOrgDetails
    var _listStaff = function (orgData, projData) {
        if (projData)
            return _get(orgAction(orgData.id, 'StaffMembers', projData.id));
        return _get(orgAction(orgData.id, 'StaffMembers'));
    } // _listStaff
    var _addNewStaffMember = function (orgData, newStaffMember, projData) {
        if (projData)
            return _post(orgAction(orgData.id, 'AddStaffMember', projData.id), newStaffMember);
        return _post(orgAction(orgData.id, 'AddStaffMember'), newStaffMember);
    } // _addNewStaffMember
    var _updateStaffMember = function (orgData, staffMember, projData) {
        if (projData)
            return _put(orgAction(orgData.id, 'StaffMember', projData.id, staffMember.id, 'Update'), staffMember);
        return _put(orgAction(orgData.id, 'StaffMember', staffMember.id, 'Update'), staffMember);
    } // _updateStaffMember
    var _updateStaffPassword = function (orgData, staffMember) {
        return _put(orgAction(orgData.id, 'StaffMember', staffMember.id, 'Password'), staffMember);
    } // _updateStaffMember
    var _deleteStaffMember = function (orgData, staffMember, projData) {
        if (projData)
            return _delete(orgAction(orgData.id, 'StaffMember', projData.id, staffMember.id));
        return _delete(orgAction(orgData.id, 'StaffMember', staffMember.id));
    } // _deleteStaffMember
    var _listReferralAgencies = function (orgData, projData) {
        if (projData)
            return _get(orgAction(orgData.id, 'ReferralAgencies', projData.id));
        return _get(orgAction(orgData.id, 'ReferralAgencies'));
    }
    var _addNewReferralAgency = function (orgData, newReferralAgency, projData) {
        if (projData)
            return _post(orgAction(orgData.id, 'AddReferralAgency', projData.id), newReferralAgency);
        return _post(orgAction(orgData.id, 'AddReferralAgency'), newReferralAgency);
    } // _addNewReferralAgency
    var _updateReferralAgency = function (orgData, referralAgency, projData) {
        if (projData)
            return _put(orgAction(orgData.id, 'ReferralAgency', projData.id, referralAgency.id, 'Update'), referralAgency);
        return _put(orgAction(orgData.id, 'ReferralAgency', referralAgency.id, 'Update'), referralAgency);
    } // _updateReferralAgency
    var _deleteReferralAgency = function (orgData, referralAgency, projData) {
        if (projData)
            return _delete(orgAction(orgData.id, 'ReferralAgency', projData.id, referralAgency.id));
        return _delete(orgAction(orgData.id, 'ReferralAgency', referralAgency.id));
    } // _deleteReferralAgency

    /////////////////////////////////////////////////
    var _listRiskMaps = function (orgData) {
        if (orgData)
            return _get(orgAction(orgData.id, 'RiskMaps'));
        return _get(riskMapsUri);
    } // _listRiskMaps

    var _fetchRiskMap = function (id) {
        return _get(riskMapAction(id));
    } // _fetchRiskMap

    var _updateRiskMap = function (riskMap) {
        return _put(riskMapAction(riskMap.id), riskMap);
    } // _fetchRiskMap

    var _createRiskMap = function (riskMap) {
        return _post(riskMapsUri, riskMap);
    } // _createRiskMap

    var _listRisks = function (orgData) {
        if (orgData)
            return _get(orgAction(orgData.id, 'Risks'));
        return _get(risksUri);
    } // _listRisks

    var _fetchRisk = function (guid) {
        return _get(riskAction(guid));
    } // _fetchRisk

    var _updateRisk = function (risk) {
        return _put(riskAction(risk.id), risk);
    } // _updateRisk

    var _createRisk = function (risk) {
        return _post(risksUri, risk);
    } // _createRisk

    var _deleteRisk = function (risk) {
        return _delete(riskAction(risk.id));
    } // _deleteRisk

    /////////////////////////////////////////////////
    var _loadReport = function (org, project, location, name, startDate, endDate, field) {
        var url = reportAction(org.id, name);
        if (project) {
            url = reportAction(org.id, project.id, name);
            if (location)
                url = reportAction(org.id, project.id, location.id, name);
        }
        var params = "";
        if (startDate) {
            params += (params.length == 0) ? "?" : "&";
            params += "startDate=" + startDate.getFullYear() + "-" + (startDate.getMonth() + 1) + "-" + startDate.getDate();
        }
        if (endDate) {
            params += (params.length == 0) ? "?" : "&";
            params += "endDate=" + endDate.getFullYear() + "-" + (endDate.getMonth() + 1) + "-" + endDate.getDate();
        }
        if (field) {
            params += (params.length == 0) ? "?" : "&";
            params += "field=" + field;
        }
        return _get(url + params);
    } // _loadReport

    var _loadReportFields = function (org, project) {
        var url = reportAction(org.id, project.id, 'fields');
        return _get(url);
    }

    var dataServiceFactory = {};
    dataServiceFactory.loadStaff = _loadStaff;
    dataServiceFactory.loadUser = _loadUser;
    dataServiceFactory.updateUser = _updateUser;
    dataServiceFactory.updateUserPassword = _updateUserPassword;

    dataServiceFactory.listClients = _listClients;
    dataServiceFactory.listLocProjClients = _listLocProjClients;
    dataServiceFactory.createClient = _createClient;
    dataServiceFactory.fetchClient = _fetchClient;
    dataServiceFactory.clientAddNote = _clientAddNote;
    dataServiceFactory.clientUpdate = _clientUpdate;
    dataServiceFactory.clientAddRisk = _clientAddRisk;
    dataServiceFactory.clientManageRisk = _clientManageRisk;
    dataServiceFactory.clientRemoveRisk = _clientRemoveRisk;
    dataServiceFactory.clientReopenRisk = _clientReopenRisk;
    dataServiceFactory.clientDischarge = _clientDischarge;
    dataServiceFactory.clientDelete = _clientDelete;
    dataServiceFactory.clientReopen = _clientReopen;
    dataServiceFactory.loadReferralReport = _clientReferralReport;
    dataServiceFactory.emailReferralReport = _emailReferralReport;
    dataServiceFactory.closeReferralReport = _closeReferralReport;

    dataServiceFactory.listOrganisations = _listOrganisations;
    dataServiceFactory.createOrganisation = _createOrganisation;
    dataServiceFactory.updateOrganisation = _updateOrganisation;
    dataServiceFactory.canDeleteOrganisation = _canDeleteOrganisation;
    dataServiceFactory.deleteOrganisation = _deleteOrganisation;
    dataServiceFactory.suspendOrganisation = _suspendOrganisation;
    dataServiceFactory.activateOrganisation = _activateOrganisation;
    dataServiceFactory.fetchOrgDetails = _fetchOrgDetails;
    dataServiceFactory.listProjects = _listProjects;
    dataServiceFactory.addNewProject = _addNewProject;
    dataServiceFactory.updateProject = _updateProject;
    dataServiceFactory.canDeleteProject = _canDeleteProject;
    dataServiceFactory.deleteProject = _deleteProject;
    dataServiceFactory.listStaff = _listStaff;
    dataServiceFactory.addNewStaffMember = _addNewStaffMember;
    dataServiceFactory.updateStaffMember = _updateStaffMember;
    dataServiceFactory.updateStaffPassword = _updateStaffPassword;
    dataServiceFactory.deleteStaffMember = _deleteStaffMember;
    dataServiceFactory.listReferralAgencies = _listReferralAgencies;
    dataServiceFactory.addNewReferralAgency = _addNewReferralAgency;
    dataServiceFactory.updateReferralAgency = _updateReferralAgency;
    dataServiceFactory.deleteReferralAgency = _deleteReferralAgency;

    dataServiceFactory.listRiskMaps = _listRiskMaps;
    dataServiceFactory.fetchRiskMap = _fetchRiskMap;
    dataServiceFactory.updateRiskMap = _updateRiskMap;
    dataServiceFactory.createRiskMap = _createRiskMap;

    dataServiceFactory.listRisks = _listRisks;
    dataServiceFactory.fetchRisk = _fetchRisk;
    dataServiceFactory.updateRisk = _updateRisk;
    dataServiceFactory.createRisk = _createRisk;
    dataServiceFactory.deleteRisk = _deleteRisk;

    dataServiceFactory.loadReport = _loadReport;
    dataServiceFactory.loadReportFields = _loadReportFields;

    return dataServiceFactory;
}]);