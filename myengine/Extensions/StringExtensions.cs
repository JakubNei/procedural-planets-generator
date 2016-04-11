namespace MyEngine
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        // from http://stackoverflow.com/questions/623104/byte-to-hex-string
        /// <summary>
        /// Returns byte array from string hex representation, 010204081020 would return {1, 2, 4, 8, 16, 32}.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] FromHexString(this string str)
        {
            if (str.Length == 0 || str.Length % 2 != 0)
                return new byte[0];

            byte[] buffer = new byte[str.Length / 2];
            char c;
            for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
            {
                // Convert first half of byte
                c = str[sx];
                buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

                // Convert second half of byte
                c = str[++sx];
                buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
            }

            return buffer;
        }

        //from http://stackoverflow.com/questions/5154970/how-do-i-create-a-hashcode-in-net-c-for-a-string-that-is-safe-to-store-in-a
        public static int GetPlatformIndependentHashCode(this string text)
        {
            if (text.IsNullOrEmpty()) return 0;
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
    }

}