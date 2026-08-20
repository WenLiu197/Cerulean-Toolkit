using System;
using System.Collections.Generic;
using System.Text;

namespace CeruleanToolkit.Core.Interfaces
{
    public interface IConfigService
    {
        AppConfig GetConfig();
        void Save();

        void Init();
    }
}
