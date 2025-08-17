using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;
using System.Linq;
using System;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class LeaveRequestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/LeaveRequest
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetAll()
    {
        var leaveRequests = await _context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ApprovedBy)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.LeaveStatus)
            .Select(lr => new LeaveRequestResponseDto
            {
                Id = lr.Id,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType.Name,
                LeaveStatusId = lr.LeaveStatusId,
                LeaveStatusName = lr.LeaveStatus.Name,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                UserId = lr.UserId,
                UserName = lr.User.Username,
                ApprovedById = lr.ApprovedById,
                ApprovedByName = lr.ApprovedBy != null ? lr.ApprovedBy.Username : null,
                CreatedAt = lr.CreatedAt
            })
            .ToListAsync();

        return Ok(leaveRequests);
    }

    // GET: api/LeaveRequest/5
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestResponseDto>> GetById(int id)
    {
        var leaveRequest = await _context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ApprovedBy)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.LeaveStatus)
            .Where(lr => lr.Id == id)
            .Select(lr => new LeaveRequestResponseDto
            {
                Id = lr.Id,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType.Name,
                LeaveStatusId = lr.LeaveStatusId,
                LeaveStatusName = lr.LeaveStatus.Name,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                UserId = lr.UserId,
                UserName = lr.User.Username,
                ApprovedById = lr.ApprovedById,
                ApprovedByName = lr.ApprovedBy != null ? lr.ApprovedBy.Username : null,
                CreatedAt = lr.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (leaveRequest == null)
            return NotFound();

        return Ok(leaveRequest);
    }

    // POST: api/LeaveRequest
    [HttpPost]
    public async Task<ActionResult<LeaveRequestResponseDto>> Create([FromBody] LeaveRequestCreateDto dto)
    {
        var leaveRequest = new LeaveRequest
        {
            LeaveTypeId = dto.TypeId,
            LeaveStatusId = dto.StatusId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            UserId = dto.UserId,
            ApprovedById = dto.ApprovedById,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();

        var createdLeaveRequestDto = await _context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ApprovedBy)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.LeaveStatus)
            .Where(lr => lr.Id == leaveRequest.Id)
            .Select(lr => new LeaveRequestResponseDto
            {
                Id = lr.Id,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType.Name,
                LeaveStatusId = lr.LeaveStatusId,
                LeaveStatusName = lr.LeaveStatus.Name,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                UserId = lr.UserId,
                UserName = lr.User.Username,
                ApprovedById = lr.ApprovedById,
                ApprovedByName = lr.ApprovedBy != null ? lr.ApprovedBy.Username : null,
                CreatedAt = lr.CreatedAt
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetById), new { id = leaveRequest.Id }, createdLeaveRequestDto);
    }

    // PUT: api/LeaveRequest/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] LeaveRequestCreateDto dto)
    {
        var existingLeave = await _context.LeaveRequests.FindAsync(id);
        if (existingLeave == null)
            return NotFound();

        existingLeave.LeaveTypeId = dto.TypeId;
        existingLeave.LeaveStatusId = dto.StatusId;
        existingLeave.StartDate = dto.StartDate;
        existingLeave.EndDate = dto.EndDate;
        existingLeave.Reason = dto.Reason;
        existingLeave.UserId = dto.UserId;
        existingLeave.ApprovedById = dto.ApprovedById;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/LeaveRequest/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var leaveRequest = await _context.LeaveRequests.FindAsync(id);
        if (leaveRequest == null)
            return NotFound();

        _context.LeaveRequests.Remove(leaveRequest);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/LeaveRequest/user
    [HttpGet("user")]
    public async Task<ActionResult<IEnumerable<LeaveRequestResponseDto>>> GetByCurrentUser()
    {
        // JWT içinden kullanıcı ID'sini al
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized("Kullanıcı kimliği bulunamadı.");

        var leaveRequests = await _context.LeaveRequests
            .Include(lr => lr.User)
            .Include(lr => lr.ApprovedBy)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.LeaveStatus)
            .Where(lr => lr.UserId == userId)
            .Select(lr => new LeaveRequestResponseDto
            {
                Id = lr.Id,
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType.Name,
                LeaveStatusId = lr.LeaveStatusId,
                LeaveStatusName = lr.LeaveStatus.Name,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                Reason = lr.Reason,
                UserId = lr.UserId,
                UserName = lr.User.Username,
                ApprovedById = lr.ApprovedById,
                ApprovedByName = lr.ApprovedBy != null ? lr.ApprovedBy.Username : null,
                CreatedAt = lr.CreatedAt
            })
            .ToListAsync();

        return Ok(leaveRequests);
    }   
}   
