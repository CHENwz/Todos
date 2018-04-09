using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace App1.ViewModels
{
    class TodoItemViewModel
    {
        private static TodoItemViewModel ViewModel;
        private ObservableCollection<Models.TodoItem> allItems = new ObservableCollection<Models.TodoItem>();
        public ObservableCollection<Models.TodoItem> AllItems { get { return this.allItems; } }
        private Models.TodoItem selectedItem = default(Models.TodoItem);
        public Models.TodoItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        
        private TodoItemViewModel()
        {
            this.allItems.Add(new Models.TodoItem("现操作业", "好难", new DateTimeOffset(DateTime.Now)));
            this.allItems.Add(new Models.TodoItem("计组作业", "也好难", new DateTimeOffset(DateTime.Now)));
        }

        public static TodoItemViewModel GetInstance()
        {
            if(ViewModel == null)
            {
                ViewModel = new TodoItemViewModel();
            }
            return ViewModel;
        }

        public void AddTodoItem(string title, string description, DateTimeOffset duedate)
        {
            this.allItems.Add(new Models.TodoItem(title, description, duedate));
        }

        public void RemoveTodoItem(string id)
        {
            this.allItems.Remove(this.selectedItem);
            this.selectedItem = null;
        }

        public void UpdateTodoItem(string id, string title, string description, DateTimeOffset duedate)
        {
            if (title == "")
            {
                var i = new MessageDialog("The title cannot be empty!").ShowAsync();
                return;
            }
            if (description == "")
            {
                var i = new MessageDialog("The description cannot be empty!").ShowAsync();
                return;
            }
            if (duedate.Date < DateTimeOffset.Now)
            {
                var i = new MessageDialog("The date you set cannot exceed the due date!").ShowAsync();
                return;
            }
            this.selectedItem.title = title;
            this.selectedItem.description = description;
            this.selectedItem.duedate = duedate;
            this.selectedItem = null;
        }
    }
}
