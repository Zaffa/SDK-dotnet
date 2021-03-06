﻿using Newtonsoft.Json;
using Quickblox.Sdk.GeneralDataModel.Request;

namespace Quickblox.Sdk.Modules.UsersModule.Requests
{
    public class UserSignUpRequest : BaseRequestSettings
    {
        [JsonProperty("user")]
        public UserRequest User { get; set; }
    }
}
