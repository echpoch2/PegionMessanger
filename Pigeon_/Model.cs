using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Pigeon_
{
    [Serializable]
    public class Post
    {
        public AuthToken sender
        {
            get; set;
        }
        public AuthToken adresser
        {
            get; set;
        }
        public object Body
        {
            get;set;
        }
        public DateTime Date
        {
            get; set;
        }
        public string Flag
        {
            get; set;
        }
        public Post()
        {

        }
    }
    [Serializable]
    [XmlInclude(typeof(AuthToken))]
    [XmlInclude(typeof(ObservableCollection<AuthToken>))]
    [XmlInclude(typeof(ObservableCollection<Post>))]
    [XmlInclude(typeof(Post))]
    public class Information
    {
        public string header;
        public object info;
    }
    [Serializable]
    public class Message
    {
        public string title;
        public List<Information> data;

    }
    [Serializable]
    public class AuthToken
    {
       public string id;
        public string login;
        public DateTime register_date;
        public DateTime online_date;
        public string first_name;
        public string last_name;
        public string last_ip;
        public string img_url;
        public string password;
        public string email;
        public string Login
        {
            get; set;
        }
        public string Email
        {
            get; set;
        }
        public string Id
        {
            get; set;
        }
        public int UnReadPosts
        {
            get;set;
        }
        public AuthToken(string id, string email, string login, string password)
        {
            this.id = id;
            this.email = email;
            this.login = login;
            this.password = password;
            Id = id;
            Login = login;
            Email = email;

        }
        public AuthToken()
        {
      
        }
    }
    class DelegateCommand : ICommand
    {
        Action<object> execute;
        Func<object, bool> canExecute;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute != null)
            {
                return canExecute(parameter);
            }
            return true;
        }

        public void Execute(object parameter)
        {
            if (execute != null)
                execute(parameter);
        }
        public DelegateCommand(Action<object> executeAction) : this(executeAction, null)
        {

        }
        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecuteFunc)
        {
            canExecute = canExecuteFunc;
            execute = executeAction;
        }


    }
}
