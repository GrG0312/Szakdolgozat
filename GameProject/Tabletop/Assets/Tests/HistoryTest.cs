using NUnit.Framework;
using Model.GameModel;

namespace Tests
{
    public class HistoryTest
    {
        [Test]
        public void SizeTest()
        {
            History<int> h = new History<int>();
            Assert.AreEqual(5, h.Limit);
        }
        [Test]
        public void PeekPopTest()
        {
            History<int> h = new History<int>();
            h.Push(1);
            h.Push(2);
            h.Push(3);
            h.Push(4);
            h.Push(5);
            Assert.AreEqual(5, h.Peek());
            Assert.AreEqual(5, h.Pop());
        }
        [Test]
        public void DroppingOldestTest()
        {
            History<int> h = new History<int>();
            h.Push(1);
            h.Push(2);
            h.Push(3);
            h.Push(4);
            h.Push(5);
            Assert.AreEqual(5, h.Peek());
            h.Push(6);
            Assert.AreEqual(6, h.Peek());
            Assert.IsFalse(h.Contains(1));
        }
    }
}
