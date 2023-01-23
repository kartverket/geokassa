using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace gridfiles
{
    public abstract class JsonObject
    {
        public virtual bool ValidateJson(string jsonString)
        {
            return false;
        }
        
        public virtual JSchema JSchema { get; }
        
        public virtual string JSchemaString { get; }
    }
}