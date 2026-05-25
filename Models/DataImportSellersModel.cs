using System;
using System.Collections.Generic;

namespace WebApi.Models
{
    public class DataImportSellersLogModel
    {
        public int DataImportSellersLogId { get; set; }
        public string FileName { get; set; }
        public string Status { get; set; }
        public int? TotalRows { get; set; }
        public int? ProcessedRows { get; set; }
        public int? ErrorRows { get; set; }
        public int? UserId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime? DhCreate { get; set; }
        public DateTime? DhUpdate { get; set; }
    }

    public class DataImportSellersFilterModel
    {
        public int? DataImportSellersLogId { get; set; }
        public string Status { get; set; }
        public int? UserId { get; set; }
    }

    public class ImportacaoSellersResultado
    {
        public int Success { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
