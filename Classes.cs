using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GriefReport
{
    class Grief
    {
        public int x, y;
        public string Name;
        public DateTime Date;
        public Grief(int x, int y, string name, DateTime date)
        {
            this.x = x;
            this.y = y;
            Name = name;
            Date = date;
        }
    }

    class Message
    {
        public int x, y;
        public string Name;
        public DateTime Date;
        public string Report;
        public Message(int x, int y, string name, DateTime date, string report)
        {
            this.x = x;
            this.y = y;
            Name = name;
            Date = date;
            Report = report;
        }
    }
}
