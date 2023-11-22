namespace SecureNote.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public bool IsEpic { get; set; } =  false;
        public string? ProfilePictureFilename { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool IsPublic { get; set; }
    }

    public class PwdResetCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int Code { get; set; }
    }
}
