using System;

namespace Logic.Service
{
    public class GuidGenerator
    {
        public static string GenerateGuidWithoutDashesUppercase()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
    }
}
