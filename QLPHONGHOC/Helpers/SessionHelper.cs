using Microsoft.AspNetCore.Http;

namespace QLPhongHoc.Helpers
{
    public static class SessionHelper
    {
        public const string SessionUserId = "MaTaiKhoan";
        public const string SessionHoTen = "HoTen";
        public const string SessionTenVaiTro = "TenVaiTro";
        public const string SessionMaVaiTro = "MaVaiTro";

        public static void SetString(this ISession session, string key, string value)
        {
            session.SetString(key, value ?? string.Empty);
        }

        public static string GetStringSafe(this ISession session, string key)
        {
            return session.GetString(key) ?? string.Empty;
        }

        public static int? GetInt(this ISession session, string key)
        {
            var s = session.GetString(key);
            if (int.TryParse(s, out var v)) return v;
            return null;
        }
    }
}
