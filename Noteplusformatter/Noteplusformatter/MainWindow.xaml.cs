using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HtmlAgilityPack;

namespace Noteplusformatter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] filenames;
        List<Note> notes = new List<Note>();
        string directory;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = ".txt|*.txt";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true && dialog.FileNames.Length >= 1)
            {
                filelist.ItemsSource = dialog.FileNames;
                filenames = dialog.FileNames;
                filesum.Text = dialog.FileNames.Length + "个文件";
                FileInfo f = new FileInfo(filenames[0]);
                directory = f.DirectoryName;
                fileaddress.Text = directory;
            }
        }

        private void filelist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string filepath = filelist.SelectedItem as string;
            if (filepath != null)
            {
                string content = File.ReadAllText(filepath);
                filecontent.Text = content;
            }
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(filecontent.Text);
            Note diary = new Note();
            // doc.Load(filelist.SelectedItem as string);
            if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
            {
                MessageBox.Show("Error!");
            }
            else
            {
                if (doc.DocumentNode != null)
                {
                    HtmlNode node = doc.DocumentNode.SelectSingleNode("note");
                    if (node == null)
                    {
                        formattedcontent.Text = "文件内容不正确";
                        return;
                    }
                    diary.id = node.Attributes[0].Value;
                    diary.categoryId = node.Attributes[1].Value;
                    diary.encrypted = node.Attributes[2].Value;
                    diary.title = node.Attributes[3].Value;
                    diary.modifiedDateTime = node.Attributes[4].Value;
                    diary.createdDateTime = node.Attributes[5].Value;
                    diary.content = node.InnerText;

                    formattedcontent.Text = node.InnerText;

                }
            }
        }

        private void ConvertAll_Click(object sender, RoutedEventArgs e)
        {
            HtmlDocument doc = new HtmlDocument();
            int count = 0;
            int errorcount = 0;
            for (int i = 0; i < filenames.Length; i++)
            {

                doc.Load(filenames[i], Encoding.UTF8);
                Note diary = new Note();
                // doc.Load(filelist.SelectedItem as string);
                if (doc.ParseErrors != null && doc.ParseErrors.Count() > 0)
                {
                    Console.WriteLine("error occoured");
                    errorcount++;
                    continue;
                }
                else
                {
                    if (doc.DocumentNode != null)
                    {
                        HtmlNode node = doc.DocumentNode.SelectSingleNode("note");
                        if (node != null)
                        {
                            diary.id = node.Attributes[0].Value;
                            diary.categoryId = node.Attributes[1].Value;
                            diary.encrypted = node.Attributes[2].Value;
                            diary.title = node.Attributes[3].Value;
                            diary.modifiedDateTime = node.Attributes[4].Value;
                            diary.createdDateTime = node.Attributes[5].Value;
                            diary.content = node.InnerText;
                            notes.Add(diary);
                            count++;
                        }
                        else
                        {
                            Console.WriteLine("error occoured: no note node");
                            errorcount++;
                            continue;
                        }

                    }
                    else
                    {
                        continue;
                    }
                }
            }
            var ddd = notes.OrderBy(x => x.categoryId).ThenBy(t => t.title).GroupBy(y => y.categoryId);
            List<IGrouping<string, Note>> categroup = new List<IGrouping<string, Note>>(ddd);
            for (int i = 0; i < categroup.Count(); i++)
            {
                string categoryId = categroup[i].Key;
                List<Note> IdNotes = new List<Note>(categroup[i]);
                File.AppendAllText($"{directory}\\result.txt", categoryId + "\r\n" + IdNotes[0].createdDateTime, Encoding.UTF8);
                for (int j = 0; j < IdNotes.Count; j++)
                {
                    File.AppendAllText($"{directory}\\result.txt", IdNotes[j].title + "\r\n" + IdNotes[j].content + "\r\n", Encoding.UTF8);

                }
                File.AppendAllText($"{directory}\\result.txt", "\r\n\r\n\r\n\r\n\r\n", Encoding.UTF8);


            }
            // List<Note> Ordered = new List<Note>(ddd);
            formattedcontent.Text = $"{count}个文件转换完成\n{errorcount}个文件格式不正确\n合并后的文件已经写入到{directory}\\result.txt中";

        }
    }
}
