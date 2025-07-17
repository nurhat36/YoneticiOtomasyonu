using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoneticiOtomasyonu.Models;

namespace YoneticiOtomasyonu.Services
{
    public class IyzicoPaymentService
    {
        private readonly Options _options;

        public IyzicoPaymentService()
        {
            _options = new Options
            {
                ApiKey = "sandbox-5bN4qkXcS3w0raSDJJ9K0WCR5ayeogSq",
                SecretKey = "sandbox-3jG6iB35NG5sGO2idpToKcMCu8Ivcrs2",
                BaseUrl = "https://sandbox-api.iyzipay.com"
            };
        }

        public async Task<Payment> MakePaymentAsync(ApplicationUser user, Plan plan)
        {
            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = plan.MonthlyPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                PaidPrice = plan.MonthlyPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "https://localhost:5001/Subscription/PaymentCallback" // şimdilik boş olacak
            };

            request.PaymentCard = new PaymentCard
            {
                CardHolderName = "Ahmet Yılmaz",
                CardNumber = "5528790000000008", // Iyzico test kartı
                ExpireMonth = "12",
                ExpireYear = "2030",
                Cvc = "123",
                RegisterCard = 0
            };

            request.Buyer = new Buyer
            {
                Id = user.Id,
                Name = user.UserName,
                Surname = "Soyad",
                GsmNumber = "+905551112233",
                Email = user.Email,
                IdentityNumber = "11111111111",
                LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationAddress = "Test adres",
                Ip = "85.34.78.112",
                City = "İstanbul",
                Country = "Türkiye",
                ZipCode = "34732"
            };

            request.BillingAddress = new Address
            {
                ContactName = user.UserName,
                City = "İstanbul",
                Country = "Türkiye",
                Description = "Fatura adresi",
                ZipCode = "34732"
            };

            request.BasketItems = new List<BasketItem>
            {
                new BasketItem
                {
                    Id = plan.Id.ToString(),
                    Name = plan.Name,
                    Category1 = "Abonelik",
                    ItemType = BasketItemType.VIRTUAL.ToString(),
                    Price = plan.MonthlyPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                }
            };

            var payment = Payment.Create(request, _options);
            return await await Task.FromResult(payment);
        }
    }
}
