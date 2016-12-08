using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Models;
using RiskTracker.Entities;

namespace RiskTracker.Controllers {
  [Authorize]
  public class GraphController : ApiController
  {
    private ProjectOrganisationRepository poRepo_ = new ProjectOrganisationRepository();

    [Route("api/Graph/riskReport")]
    [HttpGet]
    public GraphData GetClients()
    {
      GraphData graphData = new GraphData();

      foreach (var project in allProjects()) {
        var dataSet = new DataSet(project.Name);

        graphData.add(dataSet);
      } // foreach

      return graphData;
    } // GetClients


    private IList<Project> allProjects() {
      var userName = User.Identity.Name;
      return poRepo_.FindManager(userName).Projects;
    } // allProjects
  } // GraphController
} // RiskTracker.Controllers