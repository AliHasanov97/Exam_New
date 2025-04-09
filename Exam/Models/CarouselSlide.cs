namespace Exam.Models
{
    public class CarouselSlide
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPlaying { get; set; }
    }
}