using System;

namespace Pcf.Dotnet.Core.Security.Steeltoe.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}