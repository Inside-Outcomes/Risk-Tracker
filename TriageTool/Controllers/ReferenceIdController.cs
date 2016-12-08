using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using RiskTracker.Entities;
using RiskTracker.Models;

namespace RiskTracker.Controllers
{
    [Authorize]
    public class ReferenceIdController : ApiController
    {
      public class RegisteredData {
        public RegisteredData(Project p, ClientName cn) {
          Project = p.Name;
          Name = cn.Name;
          Guid = cn.Id.ToString();
        }

        public string Project { get; set; }
        public string Name { get; set; }
        public string Guid { get; set; }
      } // RegisteredData
      
      public class ReferenceCheck {
        public ReferenceCheck() {
          Projects = new List<RegisteredData>();
          Discharged = new List<RegisteredData>();
          External = false;
        }

        public List<RegisteredData> Projects { get; set; }
        public List<RegisteredData> Discharged { get; set; }
        public bool External { get; set; }
      } // ReferenceCheck

      private ClientRepository repo_ = new ClientRepository();
      private ProjectOrganisationRepository poRepo_ = new ProjectOrganisationRepository();

      [Route("api/ReferenceId/{orgId:guid}/{refId}")]
      [ResponseType(typeof(ReferenceCheck))]
      [HttpGet]
      public IHttpActionResult CheckReference(Guid orgId, String refId) {
        var result = new ReferenceCheck();

        List<ClientName> clients = repo_.SearchByRefId(refId).ToList();
        if (clients.Count == 0)
          return Ok(result);

        List<Project> projects = poRepo_.FetchProjects(orgId).ToList();

        bool externalOrg = false;
        foreach (ClientName cn in clients) {
          Project p = projects.Find(pp => pp.Id == cn.ProjectId);
          if (p == null) {
            externalOrg = true;
            continue;
          } // if ...

          if (!cn.Discharged.HasValue)
            result.Projects.Add(new RegisteredData(p, cn));
          else
            result.Discharged.Add(new RegisteredData(p, cn));
        } // foreach

        if (externalOrg)
          result.External = true;

        return Ok(result);
      } // CheckReference
    } // ReferenceIdController
}