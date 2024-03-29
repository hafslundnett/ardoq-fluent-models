﻿using System.Collections.Generic;

namespace ArdoqFluentModels.Ardoq
{
    public interface IArdoqModel
    {
        int GetReferenceTypeByName(string name);
        Dictionary<string, string> ComponentTypes { get; set; }
        string GetComponentTypeByName(string name);
        string Name { get; set; }
        string Id { get; set; }
    }
}
