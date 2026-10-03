using System.Collections.Generic;
using System.Text;

namespace DogHeist.Core.MatchLog
{
    /// <summary>
    /// Mã băm ngắn của bộ thông số, để gom các ván cùng một vòng cân bằng.
    /// Dùng FNV-1a 32 bit: đơn giản, ổn định giữa các máy, không cần thư viện mã hóa.
    /// </summary>
    public static class ConfigFingerprint
    {
        private const uint OffsetBasis = 2166136261u;
        private const uint Prime = 16777619u;
        private const char Separator = '\u001f';

        public static string Compute(IEnumerable<string> parts)
        {
            var hash = OffsetBasis;
            if (parts != null)
            {
                foreach (var part in parts)
                {
                    hash = Mix(hash, Encoding.UTF8.GetBytes(part ?? string.Empty));
                    // Ký tự phân cách để ("ab","c") khác ("a","bc").
                    hash = Mix(hash, Encoding.UTF8.GetBytes(Separator.ToString()));
                }
            }

            return hash.ToString("x8");
        }

        private static uint Mix(uint hash, byte[] bytes)
        {
            unchecked
            {
                foreach (var b in bytes)
                {
                    hash ^= b;
                    hash *= Prime;
                }
            }

            return hash;
        }
    }
}
