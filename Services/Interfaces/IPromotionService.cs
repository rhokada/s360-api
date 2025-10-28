using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IPromotionService
    {
        dynamic GetProducts(int promotionGroupId);
        dynamic SendContactForm(ContactForm contactForm);
        List<SysParameters> GetSysParameters();
        List<PromotionFaq> GetPromotionFaq(int promotionId);
        dynamic GetPromotionParameters(int promotionId);
        dynamic GetPromotionWinners(int promotionId);
        dynamic CustomerPriceProducts(int promotionId, int customerId);
        dynamic GetPromotionCompanies(int promotionId);
        dynamic GetPromotionCompaniesUF(int promotionId);
        dynamic GetPromotionCompaniesUfCities(int promotionId, string uf);
        string WhatsAppImage(WhatsAppCoupomModel whatsAppCoupomModel);
        //dynamic WhatsAppGetCustomer(string appId, string WaCell);
        dynamic WhatsAppValidateInputs(string appId, string Input, string Field);
        dynamic WhatsAppCustomerResults(int customerId, string Promo);
        dynamic GetPromotionPopupBanner(int promotionId);
    }
}