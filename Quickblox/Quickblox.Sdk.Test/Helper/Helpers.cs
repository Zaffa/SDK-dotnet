﻿using Windows.System.Profile;

namespace Quickblox.Sdk.Test.Helper
{
    public class Helpers
    {
        public static string GetHardwareId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            return Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(token.Id);
        }
    }
}
