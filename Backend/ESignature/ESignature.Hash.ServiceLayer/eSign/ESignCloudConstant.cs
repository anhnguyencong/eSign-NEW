using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdkTester.eSign
{
    public static class ESignCloudConstant
    {
        // Authorisation methods
        public const int AUTHORISATION_METHOD_SMS = 1;
        public const int AUTHORISATION_METHOD_EMAIL = 2;
        public const int AUTHORISATION_METHOD_MOBILE = 3;
        public const int AUTHORISATION_METHOD_PASSCODE = 4;
        public const int AUTHORISATION_METHOD_UAF = 5;

        // Synchronous / Asynchronous
        public const int ASYNCHRONOUS_CLIENTSERVER = 1;
        public const int ASYNCHRONOUS_SERVERSERVER = 2;
        public const int SYNCHRONOUS = 3;

        // MIME Types
        public const string MIMETYPE_PDF = "application/pdf";
        public const string MIMETYPE_XML = "application/xml";
        public const string MIMETYPE_XHTML_XML = "application/xhtml+xml";

        public const string MIMETYPE_BINARY_WORD = "application/msword";
        public const string MIMETYPE_OPENXML_WORD = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string MIMETYPE_BINARY_POWERPOINT = "application/vnd.ms-powerpoint";
        public const string MIMETYPE_OPENXML_POWERPOINT = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string MIMETYPE_BINARY_EXCEL = "application/vnd.ms-excel";
        public const string MIMETYPE_OPENXML_EXCEL = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string MIMETYPE_MSVISIO = "application/vnd.visio";

        // Hash MIME Types
        public const string MIMETYPE_SHA1 = "application/sha1-binary";
        public const string MIMETYPE_SHA256 = "application/sha256-binary";
        public const string MIMETYPE_SHA384 = "application/sha384-binary";
        public const string MIMETYPE_SHA512 = "application/sha512-binary";
    }
}
