using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face.Contract;

namespace CognitiveServicesDemo.Areas.Faces.Models
{
    public class IdentifyFacesModel
    {
        public IList<SelectListItem> PersonGroups = new List<SelectListItem>();
        public IList<IdentifiedFace> IdentifiedFaces = new List<IdentifiedFace>();
        public string ImageDump;
        public string Error;
    }

    public class IdentifiedFace
    {
        public Face Face;
        public IDictionary<Guid, string> PersonCandidates = new Dictionary<Guid, string>();
    }
}