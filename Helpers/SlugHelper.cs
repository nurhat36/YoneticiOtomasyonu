namespace YoneticiOtomasyonu.Helpers // kendi namespace'ine göre düzenle
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return Guid.NewGuid().ToString("N").Substring(0, 8);

            var slug = fullName.ToLower()
                .Replace(" ", "-")
                .Replace("ı", "i")
                .Replace("ç", "c")
                .Replace("ğ", "g")
                .Replace("ö", "o")
                .Replace("ş", "s")
                .Replace("ü", "u")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("?", "")
                .Replace("!", "");

            return slug;
        }
    }
}
