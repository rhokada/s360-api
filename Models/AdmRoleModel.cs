using System;

namespace WebApi.Models
{
    public class AdmRoleModel
    {
        public int      AdmRoleId   { get; set; }
        public string   AdmRoleCd   { get; set; }
        public string   AdmRoleName { get; set; }
        public DateTime? DhCreate   { get; set; }
        public DateTime? DhUpdate   { get; set; }
    }

    public class AdmRoleFilterModel
    {
        public int?   AdmRoleId   { get; set; }
        public string AdmRoleCd   { get; set; }
        public string AdmRoleName { get; set; }
    }

    public class AdmRoleCreateModel
    {
        public string AdmRoleCd   { get; set; }
        public string AdmRoleName { get; set; }
    }

    public class AdmRoleUpdateModel
    {
        public int    AdmRoleId   { get; set; }
        public string AdmRoleCd   { get; set; }
        public string AdmRoleName { get; set; }
    }
}
