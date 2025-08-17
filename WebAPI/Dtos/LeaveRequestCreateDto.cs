public class LeaveRequestCreateDto
{
    public int TypeId { get; set; }
    public int StatusId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = default!;
    public int UserId { get; set; }
    public int? ApprovedById { get; set; }
}
