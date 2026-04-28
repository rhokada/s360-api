using System;

namespace WebApi.Models
{
    public class AdmRolePermissionModel
    {
        public int    AdmRolePermissionId { get; set; }
        public int    AdmRoleId           { get; set; }
        public int    AdmPageId           { get; set; }
        public string Slug                { get; set; }
        public string Menu                { get; set; }
        public string Icon                { get; set; }
        public bool   Read                { get; set; }
        public bool   Create              { get; set; }
        public bool   Delete              { get; set; }
        public bool   Alter               { get; set; }
    }

    public class AdmRolePermissionFilterModel
    {
        public int AdmRoleId { get; set; }
    }

    public class AdmRolePermissionUpsertModel
    {
        public int  AdmRoleId { get; set; }
        public int  AdmPageId { get; set; }
        public bool Read      { get; set; }
        public bool Create    { get; set; }
        public bool Delete    { get; set; }
        public bool Alter     { get; set; }
    }
}
