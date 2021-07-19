using System;

namespace SmartHead.EfCore.Extensions.Interfaces
{
    public interface IHasModificationTime
    {
        DateTime? LastModificationTime { get; set; }
    }
}