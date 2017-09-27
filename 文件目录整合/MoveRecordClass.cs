using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 文件目录整合
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MoveRecordClass:IDisposable
    {
        string recordSrcPath;
        string recordDstPath;

        [JsonProperty]
        public string RecordSrcPath { get => recordSrcPath; set => recordSrcPath = value; }
        [JsonProperty]
        public string RecordDstPath { get => recordDstPath; set => recordDstPath = value; }

        public void Dispose()
        {
            RecordSrcPath = string.Empty;
            RecordDstPath = string.Empty;
        }
    }
}
