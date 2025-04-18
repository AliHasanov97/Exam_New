using Exam.Models;

namespace Exam.ViewModels
{
    public class BuyViewModel
    {
        public required Subject Subject { get; set; }
        public required User User { get; set; }
    }
}
