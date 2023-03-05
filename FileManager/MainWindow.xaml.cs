using FileManager.Commands;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileManager;


public partial class MainWindow : Window
{
    private string backPath = null!;
    public ICommand OpenCommand { get; set; }
    public ICommand ButtonCommand { get; set; }
    public ICommand DeleteCommand { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        DataContext = this;

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        //var path = Path.GetPathRoot(Environment.SystemDirectory);

        ArgumentNullException.ThrowIfNull(path);
        DirectoryInfo directory = new(path);

        backPath = directory?.Parent.FullName;

        foreach (var d in directory.GetDirectories())
        {
            RigtSideTree.Items.Add(d);
            LeftSideTree.Items.Add(d);
        }

        foreach (var f in directory.GetFiles())
        {
            RigtSideTree.Items.Add(f);
            LeftSideTree.Items.Add(f);
        }


        DeleteCommand = new RelayCommand(ExecuteDeleteCommand, CanEcexuteCommand);
        OpenCommand = new RelayCommand(ExecuteOpenCommand, CanEcexuteCommand);
        ButtonCommand = new RelayCommand(ExecuteButtonCommand,CanExecuteButtonCommand);
    }

    private void ExecuteDeleteCommand(object? obj)
    {
        if(obj is TreeView t)
        {
            var item = t.SelectedItem;
            try
            {
                if (item is DirectoryInfo directory)
                    DeleteDirectory(directory);

                if (item is FileInfo file)
                    file.Delete();


                LeftSideTree.Items.Remove(item);
                RigtSideTree.Items.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

        }
    }

    private bool CanExecuteButtonCommand(object? obj) => backPath != null && !string.IsNullOrWhiteSpace(backPath);

    private void ExecuteButtonCommand(object? obj)
    {

        if (obj is TreeView t)
        {
            var directory = new DirectoryInfo(backPath);
            backPath = directory.Parent?.FullName;
            ManageUpTreeView(directory, t);
        }


        //if (obj is TreeView view)
        //{
        //    if (view.Items.Count == 0) return;

        //    var item = view.Items[0];

        //    if (item == null) return;

        //    if (item is DirectoryInfo directory)
        //    {

        //        if (directory.Parent?.Parent is null) return;

        //        ManageUpTreeView(directory.Parent.Parent, view);
        //    }
        //    if (item is FileInfo file)
        //    {
        //        var directory2 = file.Directory;

        //        if (directory2?.Parent?.Parent is null) return;

        //        ManageUpTreeView(directory2.Parent.Parent, view);
        //    }
        //}

    }

    private void ExecuteOpenCommand(object? parameter)
    {
        if (parameter is TreeView t)
        {
            if (t.SelectedItem is DirectoryInfo directory)
            {
                backPath = directory.Parent.FullName;
                ManageUpTreeView(directory, t);
            }
            else if (t.SelectedItem is FileInfo file)
            {
                backPath = file.Directory.FullName;
                OpenWithDefaultProgram(file.FullName);
            }

        }
    }

    private bool CanEcexuteCommand(object? parameter)
    {
        if (parameter is TreeView t)
            return t.SelectedItem != null;

        return false;
    }

    private void DeleteDirectory(DirectoryInfo directory)
    {
        foreach (var f in directory.GetFiles())
            f.Delete();

        foreach (var d in directory.GetDirectories())
            DeleteDirectory(d);
         
        directory.Delete();
    }


    private void ManageUpTreeView(DirectoryInfo directory, TreeView view)
    {
        if (view == null || directory == null) return;

        try
        {
            var directories = directory.GetDirectories();
            var files = directory.GetFiles();

            view.Items.Clear();


            foreach (var d in directories)
                view.Items.Add(d);

            foreach (var f in files)
                view.Items.Add(f);
        }
        catch (Exception)
        {
            MessageBox.Show("Access Denied");
            return;
        }
    }

    public static void OpenWithDefaultProgram(string path)
    {
        using Process fileopener = new Process();

        fileopener.StartInfo.FileName = "explorer";
        fileopener.StartInfo.Arguments = "\"" + path + "\"";
        fileopener.Start();
    }
}



