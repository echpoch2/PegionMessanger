using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Pigeon_
{
    class ViewModel : INotifyPropertyChanged
    {
        private Page curPage;
        int k = 0;
        public Page CurrentPage
        {
            get {
                k++;
                if (k == 1)
                    return curPage = new auth();
                else return curPage;

            }
            set
            {
                curPage = value;
                onPropertyChanged("CurrentPage");
            }
        }
        public string CurrentPageThread
        {
            get { return " "; } set
            { if (value == "index")
                {
                    CurrentPage = new index();
                }
            }
        }
        public ObservableCollection<AuthToken> friends;
        public ObservableCollection<AuthToken> Friends
        {
            get { return friends; }
            set { friends = value; }
        }
        public ObservableCollection<AuthToken> FriendsSearch
        {
            get; set;
        } = new ObservableCollection<AuthToken>();
        AuthToken cf;
        public AuthToken CurrentFriend
        {
            get { return cf; }
            set
            {
                if (value != null)
                {
                    cf = value;
                    currentadresser = null;
                    onPropertyChanged("CurrentAdresser");
                    onPropertyChanged("CurrentFriend");
                    SendPostMode = Visibility.Hidden;
                    ViewInfoFriendMode = Visibility.Visible;
                    onPropertyChanged("SendPostMode");
                    onPropertyChanged("ViewInfoFriendMode");
                    CurrentAdresser = null;
                    
                    int k = -1;
                    foreach(AuthToken a in FriendsRequest)
                    {
                        if(a.id==value.id)
                        {
                            SendRequest = Visibility.Hidden;
                            AcceptRequest = Visibility.Visible;
                            k = 1;
                        }
                    }
                    
                    if(k==-1)
                    {
                        SendRequest = Visibility.Visible;
                        AcceptRequest = Visibility.Hidden;
                    }
                        
                    
                    onPropertyChanged("SendRequest");
                    onPropertyChanged("AcceptRequest");

                    

                }

            }
        }
        public ObservableCollection<AuthToken> FriendsRequest
        {
    get;set;
        }
        public AuthToken currentreq;
        public AuthToken CurrentRequest
        {
            get { return currentreq; }
            set
            {
                CurrentFriend = value;
                currentreq = value;
                CurrentAdresser = null;
  
                
            
            }
        }
        public Visibility SendRequest
        {
            get;set;
        }
        public Visibility AcceptRequest
        {
            get; set;
        }
        public AuthToken currentadresser;
        public AuthToken CurrentAdresser
        {
            get
            {
                return currentadresser;
            }
            set
            {
                
                if (value != null)
                {
                    currentadresser = value;
                    onPropertyChanged("CurrentAdresser");
                    cf = null;
                    currentreq = null;

                    onPropertyChanged("CurrentFriend");
                    Posts.Clear();
                    List<Information> l = new List<Information>();
                    l.Add(new Information() { header = "Adresser", info = AT });
                    l.Add(new Information() { header = "Sender", info = value });
                    SendPostMode = Visibility.Visible;
                    ViewInfoFriendMode = Visibility.Hidden;
                    onPropertyChanged("SendPostMode");
                    onPropertyChanged("ViewInfoFriendMode");
                    Message msg = new Message() { title = "Get Posts", data = l };
                    CurrentFriend = null;
                    SendToServer(msg);
                }

            }
        }
        public Visibility FriendSearchMode
        {
            get; set;
        } = Visibility.Hidden;
        public Visibility FriendViewMode
        {
            get; set;
        } = Visibility.Visible;
        public Visibility ViewInfoFriendMode
        {
            get; set;
        } 
        public Visibility SendPostMode
        {
            get; set;
        }
        public string PostBody
        {
            get; set;
        }
        public string searchfriend;
        public string SearchFriendField
        {
            get { return searchfriend; }
            set
            {
                FriendsSearch.Clear();
                searchfriend = value;
                List<Information> info = new List<Information>();
                info.Add(new Information() { header = "Search Value", info = value });
                Message msg = new Message() { title = "Friend Search", data = info };
                SendToServer(msg);
            }
        }
        public string login_
        {
            get;set;
        }
        public string id
        {
            get; set;
        }
        public string password
        {
            get; set;
        }
        public string repassword
        {
            get; set;
        }
        public string first_name
        {
            get; set;
        }
        public string last_name
        {
            get; set;
        }
        public string email
        {
            get; set;
        }
        public string status_;
        public string status
        {
            get {
                return status_;   
            } set {
                status_ = value;
                onPropertyChanged("status")
                ; }
        }
        public ObservableCollection<Post> Posts
        {
            get;set;
        }
        public ICommand test { get; private set; }
        public ICommand RegisterCommand { get; private set; }
        public ICommand SwitchFriendMode { get; private set; }
        public ICommand AuthCommand { get; private set; }
        public ICommand RegisterPageCommand { get; private set; }
        public ICommand SendCommand { get; private set; }
        public ICommand SendFriendRequestCommand { get; private set; }
        public ICommand AcceptFriendRequestCommand { get; private set; }
        Socket socket;
        IPEndPoint localIP;
        IPEndPoint localIP2;
        AuthToken AT
        {
            get; set;
        }
        int k_ = 0;
        private static ViewModel instance;
        ViewModel()
        {
            
            Posts = new ObservableCollection<Post>();
            status = "status";
            SwitchFriendMode = new DelegateCommand(SwitchFriendModeFunc);
            SendFriendRequestCommand = new DelegateCommand(SendFriendRequest);
            AuthCommand = new DelegateCommand(Auth);
            SendCommand = new DelegateCommand(SendPost);
            RegisterCommand = new DelegateCommand(Register);
            RegisterPageCommand = new DelegateCommand(ShowReg);
            AcceptFriendRequestCommand = new DelegateCommand(AcceptRequestF);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            localIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
            localIP2 = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            Task listeningTask = new Task(Listen);
            listeningTask.Start();
            GetInfo();
           

        }
        public void AcceptRequestF(object obj)
        {
            List<Information> l = new List<Information>();
            l.Add(new Information() { header = "Request sender", info = CurrentFriend });
            l.Add(new Information() { header = "Request accepter", info = AT });
            Message msg = new Message() { title = "Accept Friend Request", data = l };
            SendToServer(msg);
        }
        
        public static ViewModel getInstance()
        {
            if (instance == null)
                instance = new ViewModel();
            return instance;
        }
        public void SendFriendRequest(object obj)
        { List<Information> info = new List<Information>();
            info.Add(new Information() {header="Friend", info = CurrentFriend});
            info.Add(new Information() { header = "Adresser", info = AT});
            Message msg = new Message() { title = "Friend Request", data = info };
            SendToServer(msg);
        }

            public void SwitchFriendModeFunc(object obj)
        {
            if(FriendSearchMode== Visibility.Visible)
            {
                FriendSearchMode = Visibility.Hidden;
                FriendViewMode = Visibility.Visible;

                SendPostMode = Visibility.Visible;
                ViewInfoFriendMode = Visibility.Hidden;
                onPropertyChanged("SendPostMode");
                onPropertyChanged("ViewInfoFriendMode");


            }
            else
            {
                FriendSearchMode = Visibility.Visible;
                FriendViewMode = Visibility.Hidden;

                SendPostMode = Visibility.Hidden;
                ViewInfoFriendMode = Visibility.Visible;

            }
            onPropertyChanged("SendPostMode");
            onPropertyChanged("ViewInfoFriendMode");

            onPropertyChanged("FriendSearchMode");
            onPropertyChanged("FriendViewMode");
        }
        public void SendPost(object obj)
        {
            List<Information> l = new List<Information>();
            l.Add(new Information() { header = "Post", info = new Post() { adresser = CurrentAdresser, Body = PostBody, sender = AT } });
            Message msg = new Message() { title = "Posts", data = l };
            SendToServer(msg);
        }
        public void ShowReg(object obj)
        {
            CurrentPage = new reg();
        }
        public void GetInfo()
        {
            
            if (File.Exists("AuthToken.dat"))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream("AuthToken.dat", FileMode.OpenOrCreate))
                {
                    AT = (AuthToken)formatter.Deserialize(fs);
                }
                login_ = AT.login;
                id = AT.id;
                email = AT.email;
                byte[] data_s = new byte[1024];
                XmlSerializer formatterXML = new XmlSerializer(typeof(Message));
                List<Information> l = new List<Information>();
                l.Add(new Information() { header = "AuthToken", info = AT });
                Message msg = new Message() { title = "GetInfo", data = l };
                using (MemoryStream memstrm = new System.IO.MemoryStream(data_s))
                using (StreamWriter memwtr = new StreamWriter(memstrm))
                    formatterXML.Serialize(memwtr, msg);
                socket.SendTo(data_s, localIP2);
            }
        }
       public void SendToServer(Message msg)
        {
            byte[] data_s = new byte[5096];
            XmlSerializer formatter = new XmlSerializer(typeof(Message));
            using (MemoryStream memstrm = new System.IO.MemoryStream(data_s))
            using (StreamWriter memwtr = new StreamWriter(memstrm))
                formatter.Serialize(memwtr, msg);
            socket.SendTo(data_s, localIP2);
        }
        public void Auth(object obj)
        {
            byte[] data_s = new byte[1024];
            XmlSerializer formatter = new XmlSerializer(typeof(Message));
            List<Information> l = new List<Information>();
            l.Add(new Information() { header = "Login", info = login_ });
            l.Add(new Information() { header = "Password", info = password });
            Message msg = new Message() { title = "Auth", data = l };
            using (MemoryStream memstrm = new System.IO.MemoryStream(data_s))
            using (StreamWriter memwtr = new StreamWriter(memstrm))
                formatter.Serialize(memwtr, msg);
                socket.SendTo(data_s, localIP2);
        }
        public void Register(object obj)
        {
            byte[] data_s = new byte[1024];
            XmlSerializer formatter = new XmlSerializer(typeof(Message));
            List<Information> l = new List<Information>();
            l.Add(new Information() { header = "Login", info = login_ });
            l.Add(new Information() { header = "Password", info = password });
            l.Add(new Information() { header = "Email", info = email });
            Message msg = new Message() { title = "Register", data = l };
            using (MemoryStream memstrm = new System.IO.MemoryStream(data_s))
            using (StreamWriter memwtr = new StreamWriter(memstrm))
                formatter.Serialize(memwtr, msg);


            socket.SendTo(data_s, localIP2);


        }

        public void Listen()
        {
            status = "Listen";
         
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001));
            while (true)
            {
                // получаем сообщение
                StringBuilder builder = new StringBuilder();
                int bytes = 0; // количество полученных байтов
                byte[] data = new byte[5096]; // буфер для получаемых данных
                byte[] data_s = new byte[5096]; // буфер для получаемых данных

                //адрес, с которого пришли данные
                EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                do
                {

                    bytes = socket.ReceiveFrom(data, ref remoteIp);
                    status = "Пришли данные";
                    // чтения и записи данных в потоки.
                    Message msg;
                    XmlSerializer formatter = new XmlSerializer(typeof(Message));
                    using (MemoryStream memstrm = new MemoryStream(data))
                    using (StreamReader memrdr = new StreamReader(memstrm))
                    {
                        msg = (Message)formatter.Deserialize(memrdr);
                        
                    }
                    Executer(msg);                   
                    IPEndPoint remoteFullIp = remoteIp as IPEndPoint;
                    Console.WriteLine(msg.title + " " + remoteFullIp);
                }
                while (socket.Available > 0);
            }
            
            void Executer(Message msg)
            {
             
            switch (msg.title)
                {
                    case "Register Success":
                        foreach (Information i in msg.data)
                        {
                            
                            //if (i.header == "AuthToken")
                            //{
                            //    BinaryFormatter formatter = new BinaryFormatter();
                            //    // получаем поток, куда будем записывать сериализованный объект
                            //    using (FileStream fs = new FileStream("AuthToken.dat", FileMode.OpenOrCreate))
                            //    {
                            //        formatter.Serialize(fs, (AuthToken)i.info);
                            //    }
                            //}

                        }
                        break;
                    case "Auth Success":
                        {
                            foreach (Information i in msg.data)
                            {
                                if (i.header == "AuthToken")
                                {
                    
                                    BinaryFormatter formatter = new BinaryFormatter();
                                    // получаем поток, куда будем записывать сериализованный объект
                                    using (FileStream fs = new FileStream("AuthToken.dat", FileMode.OpenOrCreate))
                                    {
                                        formatter.Serialize(fs, (AuthToken)i.info);
                                    }
                                    GetInfo();
                                }

                            }
                            break;
                        }
                    case "GetInfo Success":
                        
                     
                        Application.Current.Dispatcher.Invoke((Action)delegate {
                            foreach (Information i in msg.data)
                            {
                                if (i.header == "Friends")
                                {
                                    Friends = (ObservableCollection<AuthToken>)i.info;

                                    CurrentAdresser = Friends[0];
                                }
                                if (i.header == "Friends request")
                                {
                                    FriendsRequest = (ObservableCollection<AuthToken>)i.info;
                                }

                            }
                            CurrentPage = new index();
                        });
                        break;
                    case "Posts":
                        if (CurrentAdresser.id == ((Post)msg.data[0].info).sender.id)
                        {
                            Application.Current.Dispatcher.Invoke((Action)delegate {
                                Posts.Add((Post)msg.data[0].info);
                            });

                        }
                        else
                        {
                            
                                Application.Current.Dispatcher.Invoke((Action)delegate {


                                    if (AT.id != ((Post)msg.data[0].info).sender.id)
                                    {
                                        ObservableCollection<AuthToken> v = new ObservableCollection<AuthToken>();
                                        int index=-1;
                                        int k = 0;
                                        foreach(AuthToken a in Friends)
                                        {
                                            if (a.id == ((Post)msg.data[0].info).sender.id)
                                                index = k;
                                            k++;
                                        }
                                        foreach (AuthToken x in Friends)
                                        {
                                            v.Add(x);
                                        }
                                        v[index].UnReadPosts++;
                                        Friends.Clear();
                                        foreach (AuthToken x in v)
                                        {
                                            Friends.Add(x);
                                        }
                                    }
                            });
                        }
                            break;
                    case "Return Posts":                        
                       onPropertyChanged("Posts");
                        Application.Current.Dispatcher.Invoke((Action)delegate {
                            Posts.Add((Post)msg.data[0].info);
                            ObservableCollection<AuthToken> v = new ObservableCollection<AuthToken>();
                            int index = Friends.IndexOf(currentadresser);
                            foreach (AuthToken x in Friends)
                            {
                                v.Add(x);
                            }
                            v[index].UnReadPosts = 0;
                            Friends.Clear();
                            foreach (AuthToken x in v)
                            {
                                Friends.Add(x);
                            }
                        });
                       

                        break;
                    case "Search Friend Result":
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            int k = -1;
                            foreach (AuthToken a in FriendsRequest)
                            {
                                if (a.id == ((AuthToken)msg.data[0].info).id)
                                {

                                    k = 1;
                                }
                            }
                            foreach (AuthToken a in Friends)
                            {
                                if (a.id == ((AuthToken)msg.data[0].info).id)
                                {

                                    k = 1;
                                }
                            }
                            if (((AuthToken)msg.data[0].info).id!=AT.id && k==-1)
                            {
                                FriendsSearch.Add((AuthToken)msg.data[0].info);
                            }
                            
                        });
                        break;




                }

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void onPropertyChanged(string str)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(str));
        }

    }
}
