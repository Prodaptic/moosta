using System;
using System.Collections.Generic;
using System.Text;

namespace Moosta.Core
{
    public static class IdentifierTools
    {
        public static string GenerateId()
        {
            var id = CSharpVitamins.ShortGuid.NewGuid();
            return id.ToString();
        }
    }
}
