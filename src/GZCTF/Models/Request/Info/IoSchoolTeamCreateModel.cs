using System.ComponentModel.DataAnnotations;

namespace GZCTF.Models.Request.Info;

public class IoSchoolTeamCreateModel
{
    [Required]
    public string UserName { get; set; }
}