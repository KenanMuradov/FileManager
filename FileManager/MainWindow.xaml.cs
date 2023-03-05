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
    private string copyPath = null!;
    public ICommand OpenCommand { get; set; }
    public ICommand CopyCommand { get; set; }
    public ICommand PasteCommand { get; set; }
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
        CopyCommand = new RelayCommand(ExecuteCopyCommand, CanEcexuteCommand);
        PasteCommand = new RelayCommand(ExecutePasteCommand, CanPasteEcexuteCommand);
        OpenCommand = new RelayCommand(ExecuteOpenCommand, CanEcexuteCommand);
        ButtonCommand = new RelayCommand(ExecuteButtonCommand, CanExecuteButtonCommand);
    }

    private bool CanPasteEcexuteCommand(object? obj)
    {
        if (obj is TreeView t)
        {
            if(t.SelectedItem is DirectoryInfo && copyPath != null)
                return true;
        }
        return false;
    }

    private void ExecutePasteCommand(object? obj)
    {
        if (obj is TreeView t)
        {
            if (t.SelectedItem is FileInfo)
                return;

            if (t.SelectedItem is DirectoryInfo d)
            {
                if (File.Exists(copyPath))
                {
                    var file = new FileInfo(copyPath);

                    if (file is null)
                        return;

                    try
                    {
                        FileCopier(file.Name, file.Directory.FullName, d.FullName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if(Directory.Exists(copyPath))
                {
                    var directory  = new DirectoryInfo(copyPath);

                    DirectoryCopier(directory.FullName, d.FullName);
                }
                
            }
        }

        copyPath = null!;
    }

    private void ExecuteCopyCommand(object? obj)
    {
        if (obj is TreeView t)
        {
            if (t.SelectedItem is DirectoryInfo d)
                copyPath = d.FullName;
            else if (t.SelectedItem is FileInfo f)
                copyPath = f.FullName;

        }
    }

    private void ExecuteDeleteCommand(object? obj)
    {
        if (obj is TreeView t)
        {
            var item = t.SelectedItem;
            try
            {
                if (item is DirectoryInfo directory)
                    DeleteDirectory(directory);

                else if (item is FileInfo file)
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

    private void OpenWithDefaultProgram(string path)
    {
        using Process fileopener = new Process();

        fileopener.StartInfo.FileName = "explorer";
        fileopener.StartInfo.Arguments = "\"" + path + "\"";
        fileopener.Start();
    }

    private void FileCopier(string fileName, string sourcePath, string targetPath)
    {

        string sourceFile = Path.Combine(sourcePath, fileName);
        string destFile = Path.Combine(targetPath, fileName);

        File.Copy(sourceFile, destFile, true);
    }

    private void DirectoryCopier(string sourcePath,string targetPath)
    {
        if (!Directory.Exists(sourcePath))
            return;

        var directory = new DirectoryInfo(sourcePath);
        var resultPath = Path.Combine(targetPath, directory.Name);

        Directory.CreateDirectory(resultPath);

        foreach (var f in directory.GetFiles())
        {
            FileCopier(f.Name,sourcePath, resultPath);
        }

        foreach(var d in directory.GetDirectories())
        {
            DirectoryCopier(d.FullName, resultPath);
        }
    }
}



