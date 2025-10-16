using System.Net.Mime;
using GZCTF.Middlewares;
using GZCTF.Models.Request.Info;
using GZCTF.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace GZCTF.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class IoSchoolTeamController(
    UserManager<UserInfo> userManager,
    ILogger<IoSchoolTeamController> logger,
    ITeamRepository teamRepository,
    IStringLocalizer<Program> localizer) : ControllerBase
{
    [HttpPost]
    [RequireRobot]
    [EnableRateLimiting(nameof(RateLimiter.LimitPolicy.Concurrency))]
    [ProducesResponseType(typeof(TeamInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RequestResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTeam([FromBody] IoSchoolTeamCreateModel model, CancellationToken token)
    {
        var captain = await userManager.Users.SingleOrDefaultAsync(u => u.UserName == model.UserName, token);
        if (captain is null)
        {
            return BadRequest(new RequestResponse(localizer[nameof(Resources.Program.Team_NewCaptainNotFound)]));
        }

        var curCaptainTeams = await teamRepository.GetUserTeams(captain, token);
        if (curCaptainTeams.Length != 0)
        {
            return BadRequest(new RequestResponse("This user already has a team"));
        }

        var team = await teamRepository.CreateTeam(
            new() { Name = model.UserName, Bio = null },
            captain!,
            token
        );

        await userManager.UpdateAsync(captain!);

        logger.Log(StaticLocalizer[nameof(Resources.Program.Team_Created), team.Name], captain,
            TaskStatus.Success);

        return Ok(TeamInfoModel.FromTeam(team));
    }
}