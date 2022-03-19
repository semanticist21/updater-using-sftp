using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Updater.services;

namespace Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        #region [ for performacne testing ]

        //List<CustomConnectionInfo> abeec = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> kk3a34bc = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> kkl3a1bc = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a4bc = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab5c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab6c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> abc99 = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab1c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab7c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab3c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab4c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab2c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> abc3 = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> abc54 = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a44b1c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a3bc = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a1b3c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a33b2c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a1b2c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> a1b1c = new List<CustomConnectionInfo>();
        //List<CustomConnectionInfo> ab83c = new List<CustomConnectionInfo>();

       

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    //SftpUpdater updater = new SftpUpdater
        //    //(new CustomConnectionInfo
        //    //{
        //    //    Address = "195.144.107.198",
        //    //    Port = 22,
        //    //    User = "demo",
        //    //    Password = "password",
        //    //    SftpFileDirectory = "/",
        //    //    FileDirectory = Directory.GetCurrentDirectory()
        //    //}
        //    //);
        //    //if (updater.IsConnected)
        //    //{
        //    var main = new MainWindow();
        //    main.Show();

        //    List<string> a = new List<string>();

        //    List<string> b = new List<string>();
        //    List<string> c = new List<string>();
        //    List<string> d = new List<string>();
        //    List<string> f = new List<string>();

        //    Stopwatch watch = new Stopwatch();

        //    int idx = 0;
        //    while (idx < 1000)
        //    {
        //        a.Add("yoloman");
        //        idx++;
        //    }
        //    ReadOnlySpan<string> abc = new ReadOnlySpan<string>(a.ToArray());

        //    int idx2 = 0;
        //    double foreachNum = 0;
        //    double Spannum = 0;
        //    double spanFornum = 0;
        //    double forNum = 0;
        //    while (idx2 < 5)
        //    {
        //        watch.Start();
        //        foreach (string item in a)
        //        {
        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);

        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);

        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);
        //            b.Add(item);
        //            executeQuery();
        //        }
        //        watch.Stop();
        //        foreachNum += watch.Elapsed.TotalMilliseconds;
        //        watch.Reset();

        //        watch.Start();
        //        foreach (string item in CollectionsMarshal.AsSpan(a))
        //        {
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            c.Add(item);
        //            executeQuery();
        //        }
        //        watch.Stop();
        //        Spannum += watch.Elapsed.TotalMilliseconds;
        //        watch.Reset();

        //        watch.Start();


        //        for (int i = 0; i < abc.Length; i++)
        //        {
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);

        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);

        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            d.Add(abc[i]);
        //            executeQuery();
        //        }
        //        watch.Stop();
        //        spanFornum += watch.Elapsed.TotalMilliseconds;

        //        watch.Reset();
        //        b.Clear();

        //        watch.Start();
        //        for (int i = 0; i < a.Count; i++)
        //        {
        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);

        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);

        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            f.Add(a[i]);
        //            executeQuery();
        //        }
        //        watch.Stop();
        //        forNum += watch.Elapsed.TotalMilliseconds;
        //        watch.Reset();
        //        c.Clear();
        //        b.Clear();
        //        d.Clear();
        //        f.Clear();
        //        idx2++;
        //    }

        //    Console.WriteLine("foreach consumed {0} !", foreachNum / (idx2 * 1000));
        //    Console.WriteLine("spanForeach consumed {0} !", Spannum / (idx2 * 1000));
        //    Console.WriteLine("spanFor consumed {0} !", spanFornum / (idx2 * 1000));
        //    Console.WriteLine("for consumed {0} !", forNum / (idx2 * 1000));

        //    //}
        //    //else
        //    //{
        //    //    Environment.Exit(0);
        //    //}
        //}
        //private void executeQuery()
        //{
        //    string a = "aa";
        //    int i = 1;
        //    List<CustomConnectionInfo> bb = new List<CustomConnectionInfo>();
        //    int idx = 0;
        //    while (idx < 100000)
        //    {
        //        kk3a34bc.Clear();
        //        kkl3a1bc.Clear();
        //        a4bc.Clear();
        //        ab5c.Clear();
        //        ab6c.Clear();
        //        abc99.Clear();
        //        ab1c.Clear();
        //        ab7c.Clear();
        //        ab3c.Clear();
        //        ab4c.Clear();
        //        ab2c.Clear();
        //        abc3.Clear();
        //        a44b1c.Clear();
        //        a3bc.Clear();
        //        a1b3c.Clear();
        //        a33b2c.Clear();
        //        a1b2c.Clear();
        //        a1b1c.Clear();
        //        ab83c.Clear();
        //        CustomConnectionInfo abcd = new CustomConnectionInfo();
        //        abcd.Address = "asd";
        //        abcd.Port = 34123;
        //        abcd.Password = "234134124";
        //        abcd.User = "ad";
        //        CustomConnectionInfo abce = new CustomConnectionInfo();
        //        abce.Address = "asd";
        //        abce.Port = 34123;
        //        abce.Password = "234134124";
        //        abce.User = "ad";
        //        CustomConnectionInfo abcf = new CustomConnectionInfo();
        //        abcf.Address = "asd";
        //        abcf.Port = 34123;
        //        abcf.Password = "234134124";
        //        abcf.User = "ad";
        //        bb.Add(abcd);
        //        bb.Add(abce);
        //        bb.Add(abcf);
        //        bb.Clear();
        //        idx++;
        //    }

        //}
        #endregion
    }
}
