namespace Shortix.UrlShortener.Core.Extensions
{
    public static class Base62EncodingExtensions
    {
        private static readonly char[] Base62 =
        {
            '0','1','2','3','4','5','6','7','8','9',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'
        };

        public static string EncodeToBase62(this long number)
        {
            if (number == 0) return Base62[0].ToString();

            var result = new Stack<char>();
            while (number > 0)
            {
                var remainder = number % 62;

                result.Push(Base62[remainder]);

                number /= 62;
            }

            return new string([.. result]);
        }
    }
}