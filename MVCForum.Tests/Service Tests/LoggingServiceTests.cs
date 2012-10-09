using System;
using System.Linq;
using System.Threading;
using MVCForum.Services;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class LoggingServiceTests
    {
        private LoggingService _loggingService;

        [SetUp]
        public void Init()
        {

        }

        [Test]
        public void ThreadingTest()
        {
            _loggingService = new LoggingService();
            _loggingService.Initialise(1000000);

            Thread.Sleep(1000);// Ensure unique archive file name!
            _loggingService.Recycle();

            // Test that many threads compete
            var t1 = new Thread(DoIt);
            var t2 = new Thread(DoIt);
            var t3 = new Thread(DoIt);
            var t4 = new Thread(DoIt);
            var t5 = new Thread(DoIt);
            var t6 = new Thread(DoIt);
            var t7 = new Thread(DoIt);
            var t8 = new Thread(DoIt);
            var t9 = new Thread(DoIt);
            var t10 = new Thread(DoIt);
            var t11 = new Thread(DoIt);
            var t12 = new Thread(DoIt);

            t1.Start("one");
            t2.Start("two");
            t3.Start("three");
            t4.Start("four");
            t5.Start("five");
            t6.Start("six");
            t7.Start("seven");
            t8.Start("eight");
            t9.Start("nine");
            t10.Start("ten");
            t11.Start("eleven");
            t12.Start("twelve");

            // Allow some time for the threads to complete
            Thread.Sleep(5000);

            var logs = _loggingService.ListLogFile();

            Assert.IsTrue(logs.Count == 12);

            // Use of "Any" because threads and locks etc mean the completion of the logs is unpredictable
            Assert.IsTrue(logs.Any(item => item.ErrorMessage == "one"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "two"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "three"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "four"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage == "five"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "six"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "seven"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "eight"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "nine"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage ==  "ten"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage == "eleven"));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage == "twelve"));
        }

        private void DoIt(object message)
        {
            _loggingService.Error(message as string);
        }

        [Test]
        public void RecycleTest()
        {
            // Test that the file recycles

            // Set the size
            _loggingService = new LoggingService();
            _loggingService.Initialise(114); // Allows two lines
            _loggingService.Recycle();
            
            Thread.Sleep(1000);// Ensure unique archive file name!

            // Each line looks like this: "01/10/2012 13:49:01 | mscorlib.dll | InvokeMethod | 9ABC"
            _loggingService.Error("1234");
            _loggingService.Error("5678");

            // At this point have two lines before recycling          

            var logs = _loggingService.ListLogFile();

            Assert.IsTrue(logs.Count == 2);

            // Add another line - will tip over the recycling point
            _loggingService.Error("9ABC");

            logs = _loggingService.ListLogFile();

            Assert.IsTrue(logs.Count == 1);

        }

        [Test]
        public void InnerExceptionTest()
        {
            _loggingService = new LoggingService();
            _loggingService.Initialise(1000000);
            _loggingService.Recycle();

            Thread.Sleep(1000);// Ensure unique archive file name!

            var ex7 = new ApplicationException("Ex_Seventh");
            var ex6 = new ApplicationException("Ex_Sixth", ex7);
            var ex5 = new ApplicationException("Ex_Fifth", ex6);
            var ex4 = new ApplicationException("Ex_Fourth", ex5);
            var ex3 = new ApplicationException("Ex_Third", ex4);
            var ex2 = new ApplicationException("Ex_Second", ex3);
            var ex1 = new ApplicationException("Ex_First", ex2);

            _loggingService.Error(ex1);

            var logs = _loggingService.ListLogFile();

            // Only log down to 5 inner exceptions
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_Fifth")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_Fourth")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_Third")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_Second")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_First")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_Sixth")));
            Assert.IsFalse(logs.Any(item => item.ErrorMessage.Contains("Ex_Seventh")));
        }

        [Test]
        public void ExceptionTest()
        {
            // Test without inner exceptions
            _loggingService = new LoggingService();
            _loggingService.Initialise(1000000);
            _loggingService.Recycle();

            Thread.Sleep(1000);// Ensure unique archive file name!

            var ex1 = new ApplicationException("Ex_First");

            _loggingService.Error(ex1);

            var logs = _loggingService.ListLogFile();

            Assert.IsTrue(logs.Count == 1);

            // Only log down to 5 inner exceptions
            Assert.IsFalse(logs.Any(item => item.ErrorMessage.Contains("INNER EXCEPTION:")));
            Assert.IsTrue(logs.Any(item => item.ErrorMessage.Contains("Ex_First")));          
        }
    }
}
