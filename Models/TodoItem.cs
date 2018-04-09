using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1.Models
{
    class TodoItem
    {
        private string id;
        public string Id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTimeOffset duedate { get; set; }
        public bool? completed { get; set; }

        public TodoItem(string title, string description, DateTimeOffset duedate)
        {
            this.id = Guid.NewGuid().ToString();
            this.Id = this.id;
            this.title = title;
            this.description = description;
            this.duedate = duedate;
            this.completed = false;
        }
    }
}
