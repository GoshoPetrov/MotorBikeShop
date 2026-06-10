namespace MotorBikeShop.Models;

/// <summary>
/// View model representing a single comment on a bike detail page.
/// </summary>
public class CommentViewModel
{
    public int Id { get; set; }
    public int BikeModelId { get; set; }
    public string Content { get; set; } = null!;
    public string AuthorUserName { get; set; } = null!;
    public string AuthorId { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the current user can delete this comment.
    /// Computed server-side: true if current user is the author or an admin.
    /// </summary>
    public bool CanDelete { get; set; }
}