namespace BestoDokan.Application.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty; // = string.Empty;
                                                         // দিয়ে প্রপার্টিটিকে শুরুতেই একটা ফাঁকা
                                                         // স্ট্রিং দিয়ে দেওয়া হয়েছে যাতে ডটনেটে
                                                         // কোনো NullReferenceException-এর
                                                         // ঝামেলা না হয়।

        public string? Description { get; set; } // প্রশ্নবোধক চিহ্ন (string?) দেওয়া আছে,
                                                 // এর মানে হলো এই প্রপার্টিটি Nullable।
    }
}