using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
   public class CustomerUnsubscribeModel
   {

        public string guid { get; set; }
        public string appId { get; set; }

        public int CustomerId { get; set; }

        public string Documentnumber { get; set; }

    }
}