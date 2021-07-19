using System;
using System.ComponentModel.DataAnnotations;

namespace SmartHead.EfCore.Extensions.Interfaces
{
    public interface IHasCreationTime
    {
        [Required]
        DateTime CreationTime { get; set; }
    }
}