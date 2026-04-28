using System;

namespace WebApi.Models
{
    public class AdmPageModel
    {
        public int      AdmPageId { get; set; }
        public string   Slug      { get; set; }
        public string   Menu      { get; set; }
        public string   Icon      { get; set; }
        public DateTime? DhCreate { get; set; }
        public DateTime? DhUpdate { get; set; }
    }

    public class AdmPageFilterModel
    {
        public int?    AdmPageId { get; set; }
        public string  Slug      { get; set; }
        public string  Menu      { get; set; }
    }

    public class AdmPageCreateModel
    {
        public string Slug { get; set; }
        public string Menu { get; set; }
        public string Icon { get; set; }
    }

    public class AdmPageUpdateModel
    {
        public int    AdmPageId { get; set; }
        public string Slug      { get; set; }
        public string Menu      { get; set; }
        public string Icon      { get; set; }
    }
}
