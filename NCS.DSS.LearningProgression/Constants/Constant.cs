using System;
using System.Collections.Generic;
using System.Text;

namespace NCS.DSS.LearningProgression.Constants
{
    public static class Constant
    {
        public const string BadRequestTemplate = "Bad Request(400) in {0}, payload was {1}.";
        public const string MethodEnteredTemplate = "Entered trigger {0}.";
        public const string MethodExitingTemplate = "Exiting to trigger {0}.";
        public const string MethodGet = "get";
        public const string MethodPut = "put";
        public const string MethodPost = "post";
        public const string MethodPatch = "patch";
    }
}
