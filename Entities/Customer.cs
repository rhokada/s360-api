using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApi.Entities
{
   public class Customer
   {
      public int? CustomerId { get; set; }
      public string CustomerName { get; set; }
      public string LastName { get; set; }
      public DateTime BirthDate { get; set; }
      public string DocumentNumber { get; set; }
      //public int DocumentTypeId { get; set; }
      //public int CountryId { get; set; }
      public string AreaCode { get; set; }
      public string CellNumber { get; set; }
      public string Email { get; set; }
      public string StreetAddress { get; set; }
      public string AddressNumber { get; set; }
      public string AddressLatitude { get; set; }
      public string AddressLongitude { get; set; }
      public string PostalCode { get; set; }
      public string Neighborhood { get; set; }
      public string City { get; set; }
      public string State { get; set; }
      public bool SMSContact { get; set; }
      public bool EmailContact { get; set; }
      public string Password { get; set; }
      public string ChangePassword { get; set; }
      public string Gender { get; set; }
      public string LiveIn { get; set; }
      public string ResidentialAreaCode { get; set; }
      public string ResidentialPhoneNumber { get; set; }
      public string AddressComplement { get; set; }
      public string WatchdogDescription { get; set; }
      public string DeviceToken { get; set; }
      public string AppId { get; set; }
      //public DateTime CreateDate { get; set; }
      public string ResponsibleName { get; set; }
      public string NationalIdCard { get; set; }
      public string GenderDescription { get; set; }
      public string PromotionalCode { get; set; }
      public string CompanyName { get; set; }
      public string FacebookToken { get; set; }
      public string FacebookUserId { get; set; }
      public bool ResidentialNumberIsWhatsApp { get; set; }
      [NotMapped]
      public string recaptcha { get; set; }
   }
}