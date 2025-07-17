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

        public async Task<Payment> MakePaymentAsync(ApplicationUser user, Plan plan, PaymentFormViewModel paymentForm)
        {
            var price = plan.MonthlyPrice.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            var request = new CreatePaymentRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString(),
                Price = price,
                PaidPrice = price,
                Currency = Currency.TRY.ToString(),
                Installment = 1,
                PaymentChannel = PaymentChannel.WEB.ToString(),
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),
                CallbackUrl = "https://localhost:5001/Subscription/PaymentCallback", // Gerekirse güncelle
                PaymentCard = new PaymentCard
                {
                    CardHolderName = paymentForm.CardHolderName,  // Gerçek kullanıcı verisiyle doldurulmalı
                    CardNumber = paymentForm.CardNumber,
                    ExpireMonth = paymentForm.ExpireMonth,
                    ExpireYear = paymentForm.ExpireYear,
                    Cvc = paymentForm.Cvc,
                    RegisterCard = 0
                },
                Buyer = new Buyer
                {
                    Id = user.Id,
                    Name = user.UserName ?? "Ad",
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
                },
                BillingAddress = new Address
                {
                    ContactName = user.UserName,
                    City = "İstanbul",
                    Country = "Türkiye",
                    Description = "Fatura adresi",
                    ZipCode = "34732"
                },
                BasketItems = new List<BasketItem>
        {
            new BasketItem
            {
                Id = plan.Id.ToString(),
                Name = plan.Name,
                Category1 = "Abonelik",
                ItemType = BasketItemType.VIRTUAL.ToString(),
                Price = price
            }
        }
            };

            // Iyzico'nun Payment.Create metodu async değilse, blocking'i önlemek için Task.Run ile sarmak mantıklıdır
            var payment = await Task.Run(() => Payment.Create(request, _options));

            return payment;
        }

    }
}
