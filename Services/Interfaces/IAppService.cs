using System.Collections.Generic;
using WebApi.Entities;
using WebApi.Models;

namespace WebApi.Services.Interfaces
{
    public interface IAppService
    {
        IEnumerable<Customer> GetAll();
        dynamic AppUserHomeData(int UserId);
        dynamic AppSupSellersList(int UserId);
        dynamic AppSupCustomersList(int UserId);
        dynamic AppSupQuestionsList(int UserId); //, string SurveyTypeCd);
        dynamic AppSaveSupportRequest(SupportRequest supportrequest);
        dynamic AppSupportRequestTypeList ();
        dynamic AppDataImport(DataImport dataimport);
        dynamic CheckExistEmail(string Email, string AppId);
        dynamic Wa_CheckExistCell(string Cell, string AppId);
        dynamic CheckExistCell(string Cell, string AppId);
        object CheckExistDocumentNumber(string DocumentNumber, string AppId);
        dynamic CreateCustomer(Customer customer);
        dynamic SaveCustomerComplement(CustomerComplement customerComplement);
        Customer GetCurrentCustomer(int CustomerId);
        dynamic UpdateCustomer(Customer customer);
        dynamic ReadPurchaseReceipt(PurchaseReceipt purchaseReceipt);
        dynamic SavePuchasereceiptImage(PurchaseReceipt purchaseReceipt);
        dynamic GetParticipations(int customerId, int promotionId);
        dynamic GetInstantPrizeParticipations(int customerId, int promotionId);
        dynamic SaveCodeCaptured_PrePurchaseReceipt(PurchaseReceipt purchaseReceipt);
        dynamic CheckPurchaseRepeat(PurchaseReceipt purchaseReceipt);
        dynamic SaveQrCaptured(QrCaptured qrCaptured);
        dynamic PurchaseReceiptFinish(PurchaseReceipt purchaseReceipt);
        dynamic RecoveryPasswordGenerated(string cpf, string appId, int promotionId);
        dynamic RecoveryPassword(string guid, string password, string appId);
        dynamic CustomerUnsubscribe(string guid, string appId, int CustomerId, string Documentnumber);
        dynamic BoletoRegistro(int promotionGroupId, string cnpj_emissor, string cpf_cliente, string nr_boleto, string vl_boleto, string dt_vcto, string dt_pgto, string email, string customername);
        dynamic UpdatePassword(string oldPassword, string newPassword, int customerId);
        public dynamic PromotionInstantPrizeCheckParticipation(int customerId);
        public dynamic ConsCustomerLoyaltyPoints(int customerId, CustomerLoyaltyPointsFilter dados);
        public dynamic ConsCustomerLoyaltyPointsLastBalance(int customerId, int promotionId);
        public dynamic LoyaltyPrizeCoupons(int customerId, int promotionId);
        public dynamic LoyaltyCustomerPrizeCoupons(int customerId, int promotionId);
        public dynamic LoyaltyCustomerExchangePrizeCoupons(int customerId, int promotionId, string cdcoupon);
        public dynamic LoyaltyPrizeCouponDetail(int customerId, int promotionId, int loyaltyprizecouponid);
        public dynamic GetParticipationsNew(int customerId); //teste okada

    }
}