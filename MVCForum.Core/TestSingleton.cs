using System;

namespace MVCForum.Domain
{
    public interface ITestSingleton
    {
        DateTime Time { get; set; }
        void Refresh();
    }

    public class TestSingleton : ITestSingleton
    {
        public DateTime Time { get; set; }

        public TestSingleton()
        {
            Time = DateTime.Now;
        }

        public void Refresh()
        {
            
        }
    }
}