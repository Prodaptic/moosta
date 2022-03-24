using shortid;
using shortid.Configuration;

namespace Moosta.Core
{
    public static class IdentifierTools
    {
        private static readonly GenerationOptions _options = new GenerationOptions
        {
            UseNumbers = true,
            UseSpecialCharacters = false,
            Length = 16
        };

        public static string GenerateId()
        {
            return ShortId.Generate(_options);
        }
    }
}
