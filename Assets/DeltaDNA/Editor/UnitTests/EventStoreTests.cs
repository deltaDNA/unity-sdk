using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;

namespace DeltaDNA
{
    [TestFixture]
    [Category("EventStore Tests")]
    internal class EventStoreTests
    {
        [Test]
        public void ClearStreamTest()
        {
            Stream s = new MemoryStream();
            string obj = "{'hello':'world'}";
            byte[] data = Encoding.UTF8.GetBytes(obj);
            s.Write(data, 0, data.Length);

            EventStore.ClearStream(s);

            Assert.That(s.Length, Is.EqualTo(0));
            Assert.That(s.Position, Is.EqualTo(0));
        }

        [Test]
        public void PushEventTest()
        {
            Stream s = new MemoryStream();
            string obj = "{'hello':'world'}";
            EventStore.PushEvent(obj, s);

            Assert.That(s.Length, Is.EqualTo(21));
            Assert.That(s.Position, Is.EqualTo(21));
        }

        [Test]
        public void ReadEventsTest()
        {
            Stream s = new MemoryStream();

            EventStore.PushEvent("{'hello':'world'}", s);
            EventStore.PushEvent("{'go': 'bears'}", s);
            EventStore.PushEvent("{'score': 5}", s);
            s.Seek(0, SeekOrigin.Begin);

            List<string> events = new List<string>();
            EventStore.ReadEvents(s, events);

            Assert.That(events.Count, Is.EqualTo(3));
            Assert.That(events.ToArray()[0], Is.EqualTo("{'hello':'world'}"));
            Assert.That(events.ToArray()[1], Is.EqualTo("{'go': 'bears'}"));
            Assert.That(events.ToArray()[2], Is.EqualTo("{'score': 5}"));
        }

        [Test]
        public void SwapStreamsTest()
        {
            Stream s1 = new MemoryStream();
            EventStore.PushEvent("{'hello':'world'}", s1);
            EventStore.PushEvent("{'go': 'bears'}", s1);
            EventStore.PushEvent("{'score': 5}", s1);

            var originalLength = s1.Length;

            Stream s2 = new MemoryStream();

            EventStore.SwapStreams(ref s1, ref s2);

            Assert.That(s1.Position, Is.EqualTo(0));
            Assert.That(s1.Length, Is.EqualTo(0));
            Assert.That(s2.Position, Is.EqualTo(0));
            Assert.That(s2.Length, Is.EqualTo(originalLength));
        }
    }
}

