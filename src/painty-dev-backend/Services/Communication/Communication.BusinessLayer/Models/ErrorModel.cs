﻿using Newtonsoft.Json;

namespace Communication.BusinessLayer.Models
{
    public class ErrorModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("message")]
        public string? Message { get; set; }
        public override string ToString()
        {
            return Code + " " + Message;
        }
    }
}
