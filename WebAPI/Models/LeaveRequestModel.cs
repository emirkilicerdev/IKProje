using WebAPI.Models;

public class LeaveRequest
{
    public int Id { get; set; }

    public int LeaveTypeId { get; set; }
    public LeaveTypeModel LeaveType { get; set; }

    public int LeaveStatusId { get; set; }
    public LeaveStatusModel LeaveStatus { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public string Reason { get; set; } = default!;

    public int UserId { get; set; }
    public UserModel User { get; set; }

    public int? ApprovedById { get; set; }
    public UserModel? ApprovedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
